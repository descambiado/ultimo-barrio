using Sandbox;
using System;
using UltimoBarrio.Content.Fortification;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// BuildingSuite — suite ILabSuite del dominio Building (CONTRATO EJECUTABLE, infra QA, SOLO dev).
	///
	/// Ciclo de vida canónico de una fortificación. El rig de escena (BuildingTestRig o el que
	/// decida el coordinador) debe cumplir EXACTAMENTE estos pasos con nombre canónico y criterio
	/// PASS; esta suite es la referencia ejecutable del contrato (la misma ruta real que el rig
	/// validado usa, sin fabricar resultados):
	///
	///   1. PreviewInvalid — placement inválido REJECTED: BuildPlacementRules.Validate == OutOfRange
	///   2. PreviewValid   — placement válido ACCEPTED: Validate == Valid
	///   3. Rotation       — la rotación del preview se acepta (Validate == Valid con yaw configurado)
	///   4. Spawn          — BuildStructureHost.SpawnBuild (autoridad host) y el yaw real spawnado
	///                       coincide con el del preview (rotación aplicada)
	///   5. HP             — Health == MaxHp de la definición + modelo real cargado
	///   6. Damage         — trace real → IDamageTarget.TakeDamage, before/after/delta exactos
	///   7. Repair         — host.Repair con consumo del LabResourceFixture por delegado real
	///                       (HP restaurado + balance descontado RepairCost)
	///   8. Upgrade        — cambio de definición real hacia UpgradeTo: def + MaxHp + modelo
	///   9. Destroy        — daño real hasta 0 HP → GameObject destruido
	///
	/// Anti-falsificación: el placement se valida con BuildPlacementRules real (server) y el daño
	/// con trace real (host → IDamageTarget). La suite NUNCA spawnea sin validar ni llama internals
	/// para fabricar el PASS; solo sustituye input humano y target humano. Si el sistema de
	/// fortificación o el fixture no están disponibles en la sesión, emite SKIP honesto con motivo
	/// (nunca PASS fabricado).
	///
	/// Registro (contrato del rig): el rig construye una instancia por entrada
	///   new BuildingSuite( entry, Scene, camera, fixture, rigGameObject )
	/// (cámara y fixture opcionales: se resuelven desde la escena; sin ellas → SKIP) y la registra
	/// en ContentRuntimeSuite. El runner emite [UBSuite] Building.&lt;Label&gt; PASS|FAIL|SKIP.
	/// </summary>
	public sealed class BuildingSuite : ILabSuite
	{
		/// <summary>Pasos canónicos del contrato (orden exacto del lifecycle; el rig los emite en [BuildingLab]).</summary>
		public enum BuildStep
		{
			PreviewInvalid,
			PreviewValid,
			Rotation,
			Spawn,
			HP,
			Damage,
			Repair,
			Upgrade,
			Destroy
		}

		public string Domain => "Building";
		public string Name => _entry.Label;
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		// Dirección fija +X del lab: misma convención que BuildingTestRig (rig-1 validado).
		// La validación de ground requiere posición horizontal sobre el suelo; el lab coloca
		// la cámara a nivel de suelo (±35u) y las builds a la MISMA altura de la cámara
		// (no re-snappear al suelo: rompería el volume sweep de BuildPlacementRules).
		private static readonly Vector3 LabForward = new( 1f, 0f, 0f );

		private readonly BuildTestEntry _entry;
		private readonly Scene _scene;
		private readonly GameObject _ignoreRoot; // GameObject del rig (la traza lo ignora)

		private CameraComponent _camera;
		private LabResourceFixture _fixture;
		private BuildDefinition _def;
		private BuildStructureHost _build;
		private GameObject _buildGo;
		private Vector3 _validPosition;
		private Rotation _spawnRotation;
		private BuildStep _step;
		private TimeSince _timer;
		private float _elapsed;
		private float _damageDelta;
		private bool _destroyFired;

		public BuildingSuite( BuildTestEntry entry, Scene scene, CameraComponent camera = null, LabResourceFixture fixture = null, GameObject ignoreRoot = null )
		{
			_entry = entry ?? throw new ArgumentNullException( nameof( entry ) );
			_scene = scene ?? throw new ArgumentNullException( nameof( scene ) );
			_camera = camera;
			_fixture = fixture;
			_ignoreRoot = ignoreRoot;
		}

		public void Initialize()
		{
			_step = BuildStep.PreviewInvalid;
			_timer = 0f;
			_elapsed = 0f;
			_damageDelta = 0f;
			_destroyFired = false;

			// SKIP honesto: sistema/fixture no disponibles en esta sesión.
			if ( !Networking.IsHost )
			{
				Skip( "se requiere host (validación server de BuildPlacementRules)" );
				return;
			}

			_def = FortificationContentRegistry.Get( _entry.BuildId );
			if ( _def == null )
			{
				Skip( $"build '{_entry.BuildId}' no registrada (sistema de fortificación ausente)" );
				return;
			}

			if ( ResourceLibrary.Get<PrefabFile>( _def.Prefab ) == null )
			{
				Skip( $"prefab '{_def.Prefab}' no disponible en esta sesión" );
				return;
			}

			if ( _camera == null && !TryResolveCamera() )
			{
				Skip( "rig sin cámara (trace de daño y builder origin imposibles)" );
				return;
			}

			if ( _fixture == null && !TryResolveFixture() )
			{
				Skip( "rig sin LabResourceFixture (repair consume por delegado real)" );
				return;
			}

			Log.Info( $"[BuildingLab] === Suite: {_entry.Label} (build '{_entry.BuildId}') — contrato lifecycle (9 pasos) ===" );
		}

		public void Step( float dt )
		{
			if ( IsComplete ) return;

			_elapsed += dt;
			if ( _elapsed > 60f )
			{
				Fail( "timeout 60s (lifecycle incompleto)" );
				return;
			}

			switch ( _step )
			{
				case BuildStep.PreviewInvalid: if ( _timer >= 0.8f ) RunPreviewInvalid(); break;
				case BuildStep.PreviewValid: if ( _timer >= 0.8f ) RunPreviewValid(); break;
				case BuildStep.Rotation: if ( _timer >= 0.8f ) RunRotation(); break;
				case BuildStep.Spawn: if ( _timer >= 0.8f ) RunSpawn(); break;
				case BuildStep.HP: if ( _timer >= 1f ) RunHp(); break;
				case BuildStep.Damage: if ( _timer >= 0.8f ) RunDamage(); break;
				case BuildStep.Repair: if ( _timer >= 0.8f ) RunRepair(); break;
				case BuildStep.Upgrade: if ( _timer >= 0.8f ) RunUpgrade(); break;
				case BuildStep.Destroy: if ( _timer >= 1f ) RunDestroy(); break;
			}
		}

		// ---------- Pasos canónicos ----------

		private void RunPreviewInvalid()
		{
			var pos = ForwardPosition( _entry.InvalidDistance );
			var result = BuildPlacementRules.Validate( _scene, _def, pos, Rotation.Identity, _camera.WorldPosition, _scene.GetAllComponents<BuildStructureHost>() );

			bool pass = result == BuildPlacementResult.OutOfRange;
			Log.Info( $"[BuildingLab] {_entry.Label} PreviewInvalid ({_entry.InvalidDistance:F0}u fuera de rango) → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"PreviewInvalid: esperado OutOfRange, real {result}" ); return; }
			NextStep();
		}

		private void RunPreviewValid()
		{
			_validPosition = ForwardPosition( _entry.ValidDistance );
			var result = BuildPlacementRules.Validate( _scene, _def, _validPosition, Rotation.Identity, _camera.WorldPosition, _scene.GetAllComponents<BuildStructureHost>() );

			bool pass = result == BuildPlacementResult.Valid;
			Log.Info( $"[BuildingLab] {_entry.Label} PreviewValid ({_entry.ValidDistance:F0}u) → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"PreviewValid: esperado Valid, real {result}" ); return; }
			NextStep();
		}

		private void RunRotation()
		{
			_spawnRotation = Rotation.FromYaw( _entry.SpawnYaw );
			var result = BuildPlacementRules.Validate( _scene, _def, _validPosition, _spawnRotation, _camera.WorldPosition, _scene.GetAllComponents<BuildStructureHost>() );

			bool pass = result == BuildPlacementResult.Valid;
			Log.Info( $"[BuildingLab] {_entry.Label} Rotation yaw={_entry.SpawnYaw:F0} → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Rotation: esperado Valid con yaw {_entry.SpawnYaw:F0}, real {result}" ); return; }
			NextStep();
		}

		private void RunSpawn()
		{
			_build = BuildStructureHost.SpawnBuild( _def, _validPosition, _spawnRotation );
			_buildGo = _build != null ? _build.GameObject : null;

			if ( _build == null )
			{
				Fail( "Spawn: BuildStructureHost.SpawnBuild devolvió null (ruta real)" );
				return;
			}

			// Yaw real del prefab spawnado == yaw del preview (la rotación se aplica al spawn).
			float yaw = _build.WorldRotation.Yaw();
			bool pass = MathF.Abs( yaw - _entry.SpawnYaw ) < 1f;
			Log.Info( $"[BuildingLab] {_entry.Label} Spawn OK pos={_validPosition} yaw={yaw:F1} (esperado {_entry.SpawnYaw:F0}) {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Spawn: yaw real {yaw:F1} != {_entry.SpawnYaw:F0}" ); return; }
			NextStep();
		}

		private void RunHp()
		{
			// OnStart del host corre tras el frame de creación: aquí def/HP/modelo son reales.
			string defId = _build.Definition?.Id ?? "NULL";
			float hp = _build.Health;
			var model = _build.Components.GetInChildrenOrSelf<ModelRenderer>()?.Model;

			bool pass = defId == _entry.BuildId
				&& MathF.Abs( hp - _entry.ExpectedMaxHp ) < 0.5f
				&& model != null;

			Log.Info( $"[BuildingLab] {_entry.Label} HP def={defId} HP={hp:F0}/{_entry.ExpectedMaxHp:F0} model={model?.ResourceName ?? "none"} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"HP: def={defId} hp={hp:F0} model={model?.ResourceName ?? "none"} (esperado {_entry.BuildId}/{_entry.ExpectedMaxHp:F0})" ); return; }
			NextStep();
		}

		private void RunDamage()
		{
			var target = TraceHitTarget();
			if ( target == null || !ReferenceEquals( target, _build ) )
			{
				Fail( $"Damage: trace no impactó el build (target={target?.GetType().Name ?? "null"})" );
				return;
			}

			float before = _build.Health;
			target.TakeDamage( new ContentDamageEvent
			{
				Amount = _entry.DamageAmount,
				Position = _build.WorldPosition,
				SourceId = "lab_build_damage_fixture",
				AttackerId = Connection.Local?.Id.ToString() ?? ""
			} );
			float after = _build.Health;
			_damageDelta = before - after;

			float expected = _entry.ExpectedMaxHp - _entry.DamageAmount;
			bool pass = MathF.Abs( after - expected ) < 0.5f;
			Log.Info( $"[BuildingLab] {_entry.Label} Damage before={before:F0} after={after:F0} (delta {_damageDelta:F1}, esperado {expected:F0}) {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Damage: HP {after:F0} != {expected:F0}" ); return; }
			NextStep();
		}

		private void RunRepair()
		{
			_fixture.SetBalance( _entry.FixtureBalance );
			int balanceBefore = _fixture.Balance;
			float hpBefore = _build.Health;

			bool ok = _build.Repair( _def.RepairAmount, _fixture.TryConsume );

			float expectedHp = MathF.Min( _def.MaxHp, hpBefore + _def.RepairAmount );
			bool pass = ok
				&& MathF.Abs( _build.Health - expectedHp ) < 0.5f
				&& _fixture.Balance == balanceBefore - _def.RepairCost;

			Log.Info( $"[BuildingLab] {_entry.Label} Repair HP {hpBefore:F0} → {_build.Health:F0} balance {balanceBefore} → {_fixture.Balance} (coste {_def.RepairCost}) {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Repair: ok={ok} HP {_build.Health:F0} (esperado {expectedHp:F0}) balance {_fixture.Balance} (esperado {balanceBefore - _def.RepairCost})" ); return; }
			NextStep();
		}

		private void RunUpgrade()
		{
			string beforeModel = ModelName();
			var upgradeDef = FortificationContentRegistry.Get( _def.UpgradeTo );

			_build.Upgrade();

			string afterDefId = _build.Definition?.Id ?? "NULL";
			string afterModel = ModelName();

			bool pass = upgradeDef != null
				&& afterDefId == _def.UpgradeTo
				&& MathF.Abs( _build.Health - _entry.ExpectedUpgradeMaxHp ) < 0.5f
				&& beforeModel != afterModel;

			Log.Info( $"[BuildingLab] {_entry.Label} Upgrade def={afterDefId} HP={_build.Health:F0}/{_entry.ExpectedUpgradeMaxHp:F0} model {beforeModel} → {afterModel} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Upgrade: def={afterDefId} HP={_build.Health:F0} modelo {beforeModel}→{afterModel}" ); return; }
			NextStep();
		}

		private void RunDestroy()
		{
			if ( _destroyFired )
			{
				if ( !_buildGo.IsValid )
				{
					Log.Info( $"[BuildingLab] {_entry.Label} Destroy PASS (GO destruido por daño real)" );
					Finish();
				}
				else if ( _timer >= 2f )
				{
					Fail( "Destroy: el build sigue vivo tras daño 10000" );
				}
				return;
			}

			var target = TraceHitTarget();
			if ( target == null || !ReferenceEquals( target, _build ) )
			{
				Fail( $"Destroy: trace no impactó el build (target={target?.GetType().Name ?? "null"})" );
				return;
			}

			target.TakeDamage( new ContentDamageEvent
			{
				Amount = 10000f,
				Position = _build.WorldPosition,
				SourceId = "lab_build_destroy_fixture",
				AttackerId = Connection.Local?.Id.ToString() ?? ""
			} );
			_destroyFired = true;
			_timer = 0f;
		}

		// ---------- Helpers ----------

		private void NextStep()
		{
			_step++;
			_timer = 0f;
		}

		private void Skip( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( reason );
		}

		private void Fail( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Fail( _elapsed, _damageDelta, $"fail:{reason}" );
			Log.Error( $"[BuildingLab] {_entry.Label} FAIL ({reason})" );
		}

		private void Finish()
		{
			Log.Info( $"[BuildingLab] {_entry.Label} PASS (contrato lifecycle 9/9)" );
			IsComplete = true;
			Result = LabSuiteResult.Pass( _elapsed, _damageDelta, "complete" );
		}

		private bool TryResolveCamera()
		{
			CameraComponent fallback = null;
			foreach ( var cam in _scene.GetAllComponents<CameraComponent>() )
			{
				if ( cam.IsMainCamera )
				{
					_camera = cam;
					return true;
				}
				if ( fallback == null ) fallback = cam;
			}
			if ( fallback != null )
			{
				_camera = fallback;
				return true;
			}
			return false;
		}

		private bool TryResolveFixture()
		{
			foreach ( var f in _scene.GetAllComponents<LabResourceFixture>() )
			{
				_fixture = f;
				return true;
			}
			return false;
		}

		private Vector3 ForwardPosition( float distance )
		{
			// Misma geometría que el BuildingTestRig validado: cámara + X fijo del lab,
			// a la altura de la cámara (el lab sitúa la cámara a nivel de suelo ±35u).
			return _camera.WorldPosition + LabForward * distance;
		}

		private IDamageTarget TraceHitTarget()
		{
			var start = _camera.WorldPosition;
			var end = _camera.WorldPosition + LabForward * 2000f;

			var trace = _scene.Trace.Ray( start, end );
			if ( _ignoreRoot != null ) trace = trace.IgnoreGameObjectHierarchy( _ignoreRoot );
			var tr = trace.Run();

			if ( !tr.Hit ) return null;
			return tr.GameObject.Components.GetInAncestorsOrSelf<IDamageTarget>();
		}

		private string ModelName()
		{
			return _build?.Components.GetInChildrenOrSelf<ModelRenderer>()?.Model?.ResourceName ?? "none";
		}
	}
}
