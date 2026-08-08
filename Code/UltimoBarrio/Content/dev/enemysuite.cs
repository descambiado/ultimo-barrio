using Sandbox;
using System;
using UltimoBarrio.Content.Enemies;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>Configuración de un test de enemigo dentro de la suite del rig (data-driven).</summary>
	public sealed class EnemyTestEntry
	{
		public string Label { get; set; } = "";
		public string EnemyId { get; set; } = "ub_enemy_saqueador";
		public string EnemyPrefab { get; set; } = "prefabs/content/enemies/enemy_saqueador.prefab";
		public float SpawnDistance { get; set; } = 500f;   // separación enemigo→dummy en la línea de persecución
		public float DummyDistance { get; set; } = 100f;   // dummy frente a la cámara (target humano sustituido)
		public float DamageToReceive { get; set; } = 30f;  // daño del fixture al enemigo (ruta IDamageTarget)
		public float ApproachSampleStep { get; set; } = 0.6f; // intervalo entre t0/t1/t2 (navegación real)
	}

	/// <summary>
	/// EnemySuite — suite ILabSuite del dominio Enemy (CONTRATO EJECUTABLE Saqueador, infra QA, SOLO dev).
	///
	/// Contrato canónico (pasos con nombre canónico y criterio PASS; el rig los emite en [EnemyLab]):
	///
	///   1. Spawn         — prefab real clonado + EnemyContentHost encontrado
	///   2. RegistryDef   — host.Definition.Id == EnemyId (EnemyContentRegistry real)
	///   3. Model         — ModelRenderer con modelo cargado (ruta de assets real del arquetipo)
	///   4. NavMeshAgent  — NavMeshAgent presente y activo ([RequireComponent] del host)
	///   5. Detect        — target asignado (SetTarget) + distancia inicial t0
	///   6. Approach      — navegación REAL sobre NavMeshAgent: t0 &gt; t1 &gt; t2 (nunca teleport)
	///   7. Attack        — el enemigo ataca por su ruta (percepción → TryAttack): HP del dummy baja
	///   8. ReceiveDamage — el enemigo recibe daño por IDamageTarget (before/after/delta)
	///   9. Death         — HP 0 → ruta real de muerte → GameObject destruido
	///   10. Loot         — loot table del arquetipo existe y sus WorldPrefab resuelven
	///
	/// Anti-falsificación: la navegación se mide con distancias reales del NavMeshAgent (el rig
	/// NUNCA teletransporta al enemigo) y el daño entra por IDamageTarget (el fixture sustituye al
	/// atacante humano, misma regla que BuildingTestRig.RunDamage). Si el sistema de enemigos o el
	/// fixture no están disponibles en la sesión, emite SKIP honesto con motivo (nunca PASS fabricado).
	///
	/// Registro (contrato del rig): el rig de escena construye una instancia por entrada
	///   new EnemySuite( entry, Scene, camera, dummy )
	/// (cámara y dummy opcionales: se resuelven desde la escena; sin ellas → SKIP), posiciona el
	/// dummy donde la suite lo requiera y la registra en ContentRuntimeSuite. El runner emite
	/// [UBSuite] Enemy.&lt;Label&gt; PASS|FAIL|SKIP.
	/// </summary>
	public sealed class EnemySuite : ILabSuite
	{
		/// <summary>Pasos canónicos del contrato Saqueador (orden exacto; el rig los emite en [EnemyLab]).</summary>
		public enum EnemyStep
		{
			Spawn,
			RegistryDef,
			Model,
			NavMeshAgent,
			Detect,
			Approach,
			Attack,
			ReceiveDamage,
			Death,
			Loot
		}

		public string Domain => "Enemy";
		public string Name => _entry.Label;
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private readonly EnemyTestEntry _entry;
		private readonly Scene _scene;

		private CameraComponent _camera;
		private LabDamageDummy _dummy;
		private GameObject _enemyGo;
		private EnemyContentHost _host;

		private EnemyStep _step;
		private TimeSince _timer;
		private float _elapsed;
		private float _t0;
		private float _t1;
		private float _t2;
		private bool _t1Measured;
		private float _attackDamage;
		private float _attackRange;
		private string _lootTableId = "";
		private bool _deathFired;

		public EnemySuite( EnemyTestEntry entry, Scene scene, CameraComponent camera = null, LabDamageDummy dummy = null )
		{
			_entry = entry ?? throw new ArgumentNullException( nameof( entry ) );
			_scene = scene ?? throw new ArgumentNullException( nameof( scene ) );
			_camera = camera;
			_dummy = dummy;
		}

		public void Initialize()
		{
			_step = EnemyStep.Spawn;
			_timer = 0f;
			_elapsed = 0f;
			_t1Measured = false;
			_deathFired = false;

			// SKIP honesto: sistema/fixture no disponibles en esta sesión.
			if ( !Networking.IsHost )
			{
				Skip( "se requiere host (autoridad del EnemyContentHost)" );
				return;
			}

			if ( EnemyContentRegistry.GetEnemy( _entry.EnemyId ) == null )
			{
				Skip( $"enemy '{_entry.EnemyId}' no registrada (sistema de enemigos ausente)" );
				return;
			}

			if ( ResourceLibrary.Get<PrefabFile>( _entry.EnemyPrefab ) == null )
			{
				Skip( $"prefab '{_entry.EnemyPrefab}' no disponible en esta sesión" );
				return;
			}

			if ( _camera == null && !TryResolveCamera() )
			{
				Skip( "rig sin cámara (línea de persecución y spawn imposibles)" );
				return;
			}

			if ( _dummy == null && !TryResolveDummy() )
			{
				Skip( "rig sin LabDamageDummy (target humano sustituido)" );
				return;
			}

			// Dummy en la línea de persecución (entre la cámara y el spawn), a nivel de suelo.
			_dummy.GameObject.WorldPosition = GroundPosition( _camera.WorldPosition + _camera.WorldRotation.Forward * _entry.DummyDistance );
			_dummy.ResetHealth();
			Log.Info( $"[EnemyLab] {_entry.Label} TargetDummy en {_dummy.GameObject.WorldPosition}" );

			// Spawn del enemigo por la ruta real (prefab → clone → NetworkSpawn → host).
			var prefabFile = ResourceLibrary.Get<PrefabFile>( _entry.EnemyPrefab );
			var enemy = SceneUtility.GetPrefabScene( prefabFile ).Clone();
			enemy.WorldPosition = GroundPosition( _camera.WorldPosition + _camera.WorldRotation.Forward * _entry.SpawnDistance );
			enemy.NetworkSpawn( Connection.Local );
			_enemyGo = enemy;
			_host = enemy.Components.Get<EnemyContentHost>();

			if ( _host == null )
			{
				Fail( "Spawn: prefab sin EnemyContentHost (ruta real)" );
				return;
			}

			_host.SetTarget( _dummy.GameObject );
			Log.Info( $"[EnemyLab] {_entry.Label} Spawn OK prefab={_entry.EnemyPrefab} pos={enemy.WorldPosition}" );
		}

		public void Step( float dt )
		{
			if ( IsComplete ) return;

			_elapsed += dt;
			if ( _elapsed > 60f )
			{
				Fail( "timeout 60s (contrato Saqueador incompleto)" );
				return;
			}

			switch ( _step )
			{
				// Gracia de aparición: OnStart del host corre al frame siguiente y el propio
				// host retrasa el comportamiento 1s (timeSinceSpawn). A los 1.3s todo es real.
				case EnemyStep.Spawn: if ( _timer >= 1.3f ) RunVerifySpawn(); break;
				case EnemyStep.Approach: RunApproach(); break;
				case EnemyStep.Attack: RunAttack(); break;
				case EnemyStep.ReceiveDamage: if ( _timer >= 0.2f ) RunReceiveDamage(); break;
				case EnemyStep.Death: RunDeath(); break;
				case EnemyStep.Loot: if ( _timer >= 0.2f ) RunLoot(); break;
			}
		}

		// ---------- Pasos canónicos ----------

		/// <summary>Pasos 1-5: Spawn / RegistryDef / Model / NavMeshAgent / Detect (medida t0).</summary>
		private void RunVerifySpawn()
		{
			if ( _host == null || !_enemyGo.IsValid )
			{
				Fail( "Spawn: EnemyContentHost no disponible (ruta real)" );
				return;
			}
			Log.Info( $"[EnemyLab] {_entry.Label} Spawn PASS (prefab {_entry.EnemyPrefab})" );

			string defId = _host.Definition?.Id ?? "NULL";
			if ( defId != _entry.EnemyId )
			{
				Fail( $"RegistryDef: def real '{defId}' != '{_entry.EnemyId}'" );
				return;
			}
			Log.Info( $"[EnemyLab] {_entry.Label} RegistryDef PASS ({defId})" );

			var model = _host.Components.GetInChildrenOrSelf<ModelRenderer>()?.Model;
			if ( model == null )
			{
				Fail( "Model: sin modelo cargado en el renderer (ruta de assets del arquetipo)" );
				return;
			}
			Log.Info( $"[EnemyLab] {_entry.Label} Model PASS ({model.ResourceName})" );

			if ( _host.Agent == null || !_host.Agent.Enabled )
			{
				Fail( "NavMeshAgent: ausente o desactivado" );
				return;
			}
			Log.Info( $"[EnemyLab] {_entry.Label} NavMeshAgent PASS" );

			// Detect: target asignado y medida t0 (la persecución real ya está en curso).
			_attackDamage = _host.Definition.AttackDamage;
			_attackRange = _host.Definition.AttackRange;
			_lootTableId = _host.Definition.LootTableId ?? "";
			_t0 = Vector3.DistanceBetween( _enemyGo.WorldPosition, _dummy.GameObject.WorldPosition );
			Log.Info( $"[EnemyLab] {_entry.Label} Detect PASS target='{_dummy.GameObject.Name}' t0={_t0:F1}u" );

			_step = EnemyStep.Approach;
			_timer = 0f;
		}

		/// <summary>Paso 6: navegación real t0 &gt; t1 &gt; t2 sobre NavMeshAgent (nunca teleport).</summary>
		private void RunApproach()
		{
			if ( !_t1Measured )
			{
				if ( _timer >= _entry.ApproachSampleStep )
				{
					_t1 = Vector3.DistanceBetween( _enemyGo.WorldPosition, _dummy.GameObject.WorldPosition );
					_t1Measured = true;
					_timer = 0f;
				}
				return;
			}

			if ( _timer < _entry.ApproachSampleStep ) return;

			_t2 = Vector3.DistanceBetween( _enemyGo.WorldPosition, _dummy.GameObject.WorldPosition );

			// PASS: llegó a rango de ataque (dejó de acercarse para atacar) O la distancia
			// decrece de forma estrictamente continua (acercándose por el navmesh).
			bool arrived = _t2 <= _attackRange + 20f;
			bool approaching = _t1 < _t0 - 10f && _t2 < _t1 - 10f;
			bool pass = arrived || approaching;

			float velocity = _host.Agent.Velocity.Length;
			Log.Info( $"[EnemyLab] {_entry.Label} Approach t0={_t0:F1} t1={_t1:F1} t2={_t2:F1} (rango ataque {_attackRange:F0}) velocity={velocity:F1} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass )
			{
				Fail( $"Approach: la distancia no decrece (t0={_t0:F1} t1={_t1:F1} t2={_t2:F1}; ¿navmesh ausente en la escena? velocity={velocity:F1})" );
				return;
			}

			_step = EnemyStep.Attack;
			_timer = 0f;
		}

		/// <summary>Paso 7: el enemigo ataca por su ruta (percepción → TryAttack → IDamageTarget).</summary>
		private void RunAttack()
		{
			if ( _dummy.Health < _dummy.MaxHealth )
			{
				float delta = _dummy.MaxHealth - _dummy.Health;
				bool pass = delta >= _attackDamage - 5f; // tolerancia de cooldown/pellets
				Log.Info( $"[EnemyLab] {_entry.Label} Attack dummy HP {_dummy.MaxHealth:F0} → {_dummy.Health:F0} (delta {delta:F1}, ataque {_attackDamage:F0}) {(pass ? "PASS" : "FAIL")}" );
				if ( !pass ) { Fail( $"Attack: delta {delta:F1} < esperado {_attackDamage:F0} - tol" ); return; }
				_step = EnemyStep.ReceiveDamage;
				_timer = 0f;
				return;
			}

			if ( _timer >= 12f )
			{
				Fail( "Attack: el dummy no recibió daño en 12s (¿el enemigo no alcanzó o no atacó?)" );
			}
		}

		/// <summary>Paso 8: el enemigo recibe daño por IDamageTarget (fixture = atacante humano).</summary>
		private void RunReceiveDamage()
		{
			if ( _host == null || !_enemyGo.IsValid )
			{
				Fail( "ReceiveDamage: enemigo no disponible" );
				return;
			}

			float before = _host.Health;
			_host.TakeDamage( new ContentDamageEvent
			{
				Amount = _entry.DamageToReceive,
				Position = _enemyGo.WorldPosition,
				SourceId = "lab_enemy_damage_fixture",
				AttackerId = Connection.Local?.Id.ToString() ?? ""
			} );
			float after = _host.Health;
			float delta = before - after;

			bool pass = MathF.Abs( delta - _entry.DamageToReceive ) < 0.5f;
			Log.Info( $"[EnemyLab] {_entry.Label} ReceiveDamage before={before:F0} after={after:F0} (delta {delta:F1}) {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"ReceiveDamage: delta {delta:F1} != {_entry.DamageToReceive:F0}" ); return; }

			_step = EnemyStep.Death;
			_timer = 0f;
		}

		/// <summary>Paso 9: HP 0 → ruta real de muerte (Die → GameObject.Destroy).</summary>
		private void RunDeath()
		{
			if ( _deathFired )
			{
				if ( !_enemyGo.IsValid )
				{
					Log.Info( $"[EnemyLab] {_entry.Label} Death PASS (GO destruido por la ruta real de daño)" );
					_step = EnemyStep.Loot;
					_timer = 0f;
				}
				else if ( _timer >= 2f )
				{
					Fail( "Death: el enemigo no se destruyó tras HP 0" );
				}
				return;
			}

			if ( _host.IsDead )
			{
				_deathFired = true;
				_timer = 0f;
				return;
			}

			if ( _timer >= 0.25f )
			{
				_host.TakeDamage( new ContentDamageEvent
				{
					Amount = 1000f,
					Position = _enemyGo.WorldPosition,
					SourceId = "lab_enemy_death_fixture",
					AttackerId = Connection.Local?.Id.ToString() ?? ""
				} );
				_timer = 0f;
			}
		}

		/// <summary>Paso 10: el loot del arquetipo existe y sus WorldPrefab resuelven (ruta real).</summary>
		private void RunLoot()
		{
			var table = EnemyContentRegistry.GetLootTable( _lootTableId );
			if ( table == null || table.Entries.Count == 0 )
			{
				Fail( $"Loot: tabla '{_lootTableId}' ausente o vacía" );
				return;
			}

			int resolved = 0;
			foreach ( var entry in table.Entries )
			{
				if ( string.IsNullOrEmpty( entry.WorldPrefab ) ) continue;
				if ( ResourceLibrary.Get<PrefabFile>( entry.WorldPrefab ) != null )
				{
					resolved++;
				}
				else
				{
					Log.Warning( $"[EnemyLab] Loot prefab no resuelto: {entry.WorldPrefab}" );
				}
			}

			bool pass = resolved > 0;
			Log.Info( $"[EnemyLab] {_entry.Label} Loot table='{_lootTableId}' {table.Entries.Count} entradas, {resolved} prefabs resueltos {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( "Loot: ningún WorldPrefab de la tabla resuelve" ); return; }

			Finish();
		}

		// ---------- Helpers ----------

		private void Skip( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( reason );
		}

		private void Fail( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Fail( _elapsed, 0f, $"fail:{reason}" );
			Log.Error( $"[EnemyLab] {_entry.Label} FAIL ({reason})" );
		}

		private void Finish()
		{
			float navDelta = MathF.Max( 0f, _t0 - _t2 );
			Log.Info( $"[EnemyLab] {_entry.Label} PASS (contrato Saqueador 10/10, navegación {navDelta:F1}u)" );
			IsComplete = true;
			Result = LabSuiteResult.Pass( _elapsed, navDelta, "complete" );
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

		private bool TryResolveDummy()
		{
			foreach ( var d in _scene.GetAllComponents<LabDamageDummy>() )
			{
				_dummy = d;
				return true;
			}
			return false;
		}

		/// <summary>Ancla la posición al suelo real de la escena (probe idéntico al de BuildPlacementRules).</summary>
		private Vector3 GroundPosition( Vector3 p )
		{
			var start = p + Vector3.Up * 400f;
			var end = p - Vector3.Up * 400f;
			var tr = _scene.Trace.Ray( start, end ).Run();
			return tr.Hit ? new Vector3( p.x, p.y, tr.HitPosition.z ) : p;
		}
	}
}
