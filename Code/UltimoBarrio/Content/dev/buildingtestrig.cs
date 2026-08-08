using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Content.Fortification;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>Configuración de un test de build dentro de la suite del rig.</summary>
	public sealed class BuildTestEntry
	{
		public string Label { get; set; } = "";
		public string BuildId { get; set; } = "";          // definition id a validar
		public float ValidDistance { get; set; } = 160f;   // posición válida en la línea del lab (+X)
		public float InvalidDistance { get; set; } = 500f; // fuera de rango → REJECTED
		public float BlockedDistance { get; set; } = 220f; // volumen ocupado → REJECTED
		public float SpawnYaw { get; set; } = 45f;         // rotación de spawn a verificar
		public float ExpectedMaxHp { get; set; } = 150f;
		public float DamageAmount { get; set; } = 50f;
		public int FixtureBalance { get; set; } = 100;
		public float ExpectedUpgradeMaxHp { get; set; } = 400f;
	}

	/// <summary>
	/// Building Test Rig — fixture automatizado del bucle de construcción portable.
	/// SOLO dev. Sustituye input humano y target humano; la validación de placement y
	/// el daño recorren SIEMPRE la ruta real (server):
	///   preview → BuildPlacementRules.Validate (rango, solapamiento, ground, volumen)
	///   spawn   → BuildStructureHost.SpawnBuild (autoridad host)
	///   damage  → trace real → IDamageTarget
	///   repair  → BuildStructureHost.Repair con consumo del LabResourceFixture
	///   upgrade → BuildStructureHost.Upgrade (cambio de definición: modelo + HP)
	///   destroy → daño real hasta 0 HP (destrucción por la ruta probada)
	/// </summary>
	[Title( "Building Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "science" )]
	public sealed class BuildingTestRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-2";

		[Property] public bool AutoTest { get; set; } = true;
		[Property] public List<BuildTestEntry> Tests { get; set; } = new();

		// Dirección fija +X del lab: la validación de ground requiere posición horizontal
		// sobre el suelo, así el rig no depende de convenciones de forward/rotación.
		private static readonly Vector3 LabForward = new( 1f, 0f, 0f );

		private CameraComponent _camera;
		private LabResourceFixture _fixture;
		private BuildDefinition _def;
		private BuildStructureHost _build;
		private GameObject _buildGo;
		private Vector3 _validPosition;
		private int _testIndex;
		private int _step;
		private TimeSince _timer;
		private bool _fail;
		private int _fails;

		private BuildTestEntry Entry => _testIndex < Tests.Count ? Tests[_testIndex] : null;

		protected override void OnStart()
		{
			Log.Info( $"[LabBuild] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[BuildingLab] rig sin CameraComponent" );
				_fail = true;
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			_fixture = Components.Create<LabResourceFixture>();

			if ( !CheckRegistryCoverage() )
			{
				_fail = true;
				return;
			}

			if ( Tests.Count == 0 )
			{
				Log.Error( "[BuildingLab] suite vacía (Tests sin configurar)" );
				_fail = true;
				return;
			}

			Log.Info( $"[BuildingLab] Suite: {Tests.Count} tests ({string.Join( ", ", Tests.ConvertAll( t => t.Label ) )})" );
			StartTest( 0 );
		}

		/// <summary>El registro debe exponer TODOS los objetos como data (9), con la barricada de madera PRIMERA.</summary>
		private bool CheckRegistryCoverage()
		{
			var all = FortificationContentRegistry.All;
			Log.Info( $"[BuildingLab] Registry: {all.Count} definiciones" );
			foreach ( var def in all )
			{
				if ( def == null )
				{
					Log.Error( "[BuildingLab] Registry FAIL: definición null en All" );
					continue;
				}
				Log.Info( $"[BuildingLab] Registry: {def.Id} (cat {def.Category}, HP {def.MaxHp:F0})" );
			}

			if ( all.Count != 9 )
			{
				Log.Error( $"[BuildingLab] Registry FAIL: se esperaban 9 definiciones, hay {all.Count}" );
				return false;
			}

			if ( all[0].Id != "fort_barricade_wood" )
			{
				Log.Error( $"[BuildingLab] Registry FAIL: primera definición '{all[0].Id}' (esperada 'fort_barricade_wood')" );
				return false;
			}

			Log.Info( "[BuildingLab] Registry PASS: 9 definiciones, wooden_barricade primero" );
			return true;
		}

		private void StartTest( int index )
		{
			_testIndex = index;
			_step = 0;
			_timer = 0f;
			_fail = false;
			_build = null;
			_buildGo = null;
			_def = FortificationContentRegistry.Get( Entry.BuildId );
			if ( _def == null )
			{
				Log.Error( $"[BuildingLab] {Entry.Label} FAIL: '{Entry.BuildId}' no registrada en FortificationContentRegistry" );
				_fail = true;
			}

			Log.Info( $"[BuildingLab] === Test {index + 1}/{Tests.Count}: {Entry.Label} (build '{Entry.BuildId}') ===" );
		}

		protected override void OnUpdate()
		{
			if ( _camera == null || !AutoTest ) return;

			var entry = Entry;
			if ( entry == null ) return;

			switch ( _step )
			{
				case 0:
					if ( _timer >= 1f ) RunPreviewInvalid( entry );
					break;
				case 1:
					if ( _timer >= 0.8f ) RunPreviewBlocked( entry );
					break;
				case 2:
					if ( _timer >= 0.8f ) RunPreviewValid( entry );
					break;
				case 3:
					if ( _timer >= 0.8f ) RunSpawn( entry );
					break;
				case 4:
					if ( _timer >= 1f ) RunVerifySpawn( entry );
					break;
				case 5:
					if ( _timer >= 0.8f ) RunPreviewOverlap( entry );
					break;
				case 6:
					if ( _timer >= 0.8f ) RunDamage( entry );
					break;
				case 7:
					if ( _timer >= 0.8f ) RunRepair( entry );
					break;
				case 8:
					if ( _timer >= 0.8f ) RunUpgrade( entry );
					break;
				case 9:
					if ( _timer >= 1f ) RunDestroy( entry );
					break;
				case 10:
					if ( _timer >= 1f ) FinishTest( entry );
					break;
			}
		}

		private void NextStep()
		{
			_step++;
			_timer = 0f;
		}

		private bool GuardDefinition()
		{
			if ( _def == null )
			{
				_fail = true;
				return false;
			}
			return true;
		}

		private bool GuardBuild()
		{
			if ( _build == null )
			{
				_fail = true;
				return false;
			}
			return true;
		}

		private void RunPreviewInvalid( BuildTestEntry entry )
		{
			if ( !GuardDefinition() ) { NextStep(); return; }

			var pos = ForwardPosition( entry.InvalidDistance );
			var result = BuildPlacementRules.Validate( Scene, _def, pos, Rotation.Identity, _camera.WorldPosition, Scene.GetAllComponents<BuildStructureHost>() );

			bool pass = result == BuildPlacementResult.OutOfRange;
			Log.Info( $"[BuildingLab] {entry.Label} Preview invalid ({entry.InvalidDistance:F0}u, fuera de rango) → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunPreviewBlocked( BuildTestEntry entry )
		{
			if ( !GuardDefinition() ) { NextStep(); return; }

			var pos = ForwardPosition( entry.BlockedDistance );

			// Blocker sólido en el volumen de prueba (sustituye una pared/objeto del mundo).
			var blocker = new GameObject( true, "LabBlockVolume" );
			blocker.WorldPosition = pos;
			var collider = blocker.Components.Create<BoxCollider>();
			collider.Scale = new Vector3( 20f, 20f, 20f );
			collider.Static = true;

			var result = BuildPlacementRules.Validate( Scene, _def, pos, Rotation.Identity, _camera.WorldPosition, Scene.GetAllComponents<BuildStructureHost>() );

			blocker.Destroy();

			bool pass = result == BuildPlacementResult.BlockedVolume;
			Log.Info( $"[BuildingLab] {entry.Label} Preview blocked (volumen ocupado) → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunPreviewValid( BuildTestEntry entry )
		{
			if ( !GuardDefinition() ) { NextStep(); return; }

			_validPosition = ForwardPosition( entry.ValidDistance );
			var result = BuildPlacementRules.Validate( Scene, _def, _validPosition, Rotation.FromYaw( entry.SpawnYaw ), _camera.WorldPosition, Scene.GetAllComponents<BuildStructureHost>() );

			bool pass = result == BuildPlacementResult.Valid;
			Log.Info( $"[BuildingLab] {entry.Label} Preview valid ({entry.ValidDistance:F0}u) → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunSpawn( BuildTestEntry entry )
		{
			if ( !GuardDefinition() ) { NextStep(); return; }

			_build = BuildStructureHost.SpawnBuild( _def, _validPosition, Rotation.FromYaw( entry.SpawnYaw ) );
			_buildGo = _build != null ? _build.GameObject : null;

			bool pass = _build != null;
			Log.Info( pass ? $"[BuildingLab] {entry.Label} Spawn OK ({_validPosition})" : $"[BuildingLab] {entry.Label} Spawn FAIL (null)" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunVerifySpawn( BuildTestEntry entry )
		{
			if ( !GuardBuild() ) { NextStep(); return; }

			// OnStart del host corre tras el frame de creación: aquí def/HP/modelo son reales.
			string defId = _build.Definition?.Id ?? "NULL";
			float hp = _build.Health;
			float yaw = _build.WorldRotation.Yaw();
			var model = _build.Components.GetInChildrenOrSelf<ModelRenderer>()?.Model;
			Log.Info( $"[BuildingLab] {entry.Label} Spawned def={defId} HP={hp:F0}/{entry.ExpectedMaxHp:F0} yaw={yaw:F0} model={model?.ResourceName ?? "none"}" );

			bool pass = defId == entry.BuildId
				&& MathF.Abs( hp - entry.ExpectedMaxHp ) < 0.5f
				&& MathF.Abs( yaw - entry.SpawnYaw ) < 1f
				&& model != null;

			Log.Info( pass ? $"[BuildingLab] {entry.Label} VerifySpawn PASS" : $"[BuildingLab] {entry.Label} VerifySpawn FAIL" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunPreviewOverlap( BuildTestEntry entry )
		{
			if ( !GuardDefinition() ) { NextStep(); return; }

			// Misma posición que el build ya colocado → debe rechazarse por solapamiento.
			var result = BuildPlacementRules.Validate( Scene, _def, _validPosition, Rotation.FromYaw( entry.SpawnYaw ), _camera.WorldPosition, Scene.GetAllComponents<BuildStructureHost>() );

			bool pass = result == BuildPlacementResult.OverlapsBuild;
			Log.Info( $"[BuildingLab] {entry.Label} Preview overlap (misma posición) → {result} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunDamage( BuildTestEntry entry )
		{
			if ( !GuardBuild() ) { NextStep(); return; }

			var target = TraceHitTarget();
			if ( target == null || !ReferenceEquals( target, _build ) )
			{
				Log.Error( $"[BuildingLab] {entry.Label} Damage FAIL: trace no impactó el build" );
				_fail = true;
				NextStep();
				return;
			}

			float before = _build.Health;
			target.TakeDamage( new ContentDamageEvent
			{
				Amount = entry.DamageAmount,
				Position = _build.WorldPosition,
				SourceId = "lab_build_damage_fixture",
				AttackerId = Connection.Local?.Id.ToString() ?? ""
			} );

			float expected = entry.ExpectedMaxHp - entry.DamageAmount;
			bool pass = MathF.Abs( _build.Health - expected ) < 0.5f;
			Log.Info( $"[BuildingLab] {entry.Label} Damage {before:F0} → {_build.Health:F0} (esperado {expected:F0}) {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunRepair( BuildTestEntry entry )
		{
			if ( !GuardBuild() ) { NextStep(); return; }
			if ( !GuardDefinition() ) { NextStep(); return; }

			int balanceBefore = _fixture.Balance;
			float hpBefore = _build.Health;

			bool ok = _build.Repair( _def.RepairAmount, _fixture.TryConsume );

			float expectedHp = MathF.Min( _def.MaxHp, hpBefore + _def.RepairAmount );
			bool pass = ok
				&& MathF.Abs( _build.Health - expectedHp ) < 0.5f
				&& _fixture.Balance == balanceBefore - _def.RepairCost;

			Log.Info( $"[BuildingLab] {entry.Label} Repair HP {hpBefore:F0} → {_build.Health:F0} balance {balanceBefore} → {_fixture.Balance} (coste {_def.RepairCost}) {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunUpgrade( BuildTestEntry entry )
		{
			if ( !GuardBuild() ) { NextStep(); return; }
			if ( !GuardDefinition() ) { NextStep(); return; }

			string beforeModel = ModelName();
			var upgradeDef = FortificationContentRegistry.Get( _def.UpgradeTo );

			_build.Upgrade();

			string afterDefId = _build.Definition?.Id ?? "NULL";
			string afterModel = ModelName();

			bool pass = upgradeDef != null
				&& afterDefId == _def.UpgradeTo
				&& MathF.Abs( _build.Health - entry.ExpectedUpgradeMaxHp ) < 0.5f
				&& beforeModel != afterModel;

			Log.Info( $"[BuildingLab] {entry.Label} Upgrade def={afterDefId} HP={_build.Health:F0}/{entry.ExpectedUpgradeMaxHp:F0} model {beforeModel} → {afterModel} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) _fail = true;
			NextStep();
		}

		private void RunDestroy( BuildTestEntry entry )
		{
			if ( !GuardBuild() ) { NextStep(); return; }

			var target = TraceHitTarget();
			if ( target == null || !ReferenceEquals( target, _build ) )
			{
				Log.Error( $"[BuildingLab] {entry.Label} Destroy FAIL: trace no impactó el build" );
				_fail = true;
				NextStep();
				return;
			}

			target.TakeDamage( new ContentDamageEvent
			{
				Amount = 10000f,
				Position = _build.WorldPosition,
				SourceId = "lab_build_destroy_fixture",
				AttackerId = Connection.Local?.Id.ToString() ?? ""
			} );

			NextStep();
		}

		private void FinishTest( BuildTestEntry entry )
		{
			bool destroyed = _buildGo == null || !_buildGo.IsValid;
			bool pass = !_fail && destroyed;
			if ( !destroyed )
			{
				Log.Error( $"[BuildingLab] {entry.Label} Destroy FAIL: el build sigue vivo" );
			}
			if ( !pass ) _fails++;

			Log.Info( pass ? $"[BuildingLab] {entry.Label} PASS" : $"[BuildingLab] {entry.Label} FAIL" );

			_testIndex++;
			if ( _testIndex < Tests.Count )
			{
				StartTest( _testIndex );
			}
			else
			{
				Log.Info( $"[BuildingLab] Suite complete ({Tests.Count - _fails}/{Tests.Count} PASS)" );
			}
		}

		private string ModelName()
		{
			return _build?.Components.GetInChildrenOrSelf<ModelRenderer>()?.Model?.ResourceName ?? "none";
		}

		private IDamageTarget TraceHitTarget()
		{
			var start = _camera.WorldPosition;
			var end = _camera.WorldPosition + LabForward * 2000f;

			var tr = Scene.Trace.Ray( start, end )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.Run();

			if ( !tr.Hit )
			{
				Log.Info( "[BuildingLab] trace MISS" );
				return null;
			}

			return tr.GameObject.Components.GetInAncestorsOrSelf<IDamageTarget>();
		}

		private Vector3 ForwardPosition( float distance )
		{
			return _camera.WorldPosition + LabForward * distance;
		}
	}
}
