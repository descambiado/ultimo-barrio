using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Content.Enemies;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>Configuración de un test de enemigo dentro de la suite del rig (data-driven, serializada en la escena).</summary>
	public sealed class EnemyTestEntry
	{
		public string Label { get; set; } = "";
		public string EnemyPrefab { get; set; } = "";
		public string ExpectedDefinitionId { get; set; } = ""; // valida que el host cargó esta definición
		public float ExpectedDamagePerHit { get; set; } = 15f; // daño mínimo por golpe real
		public int ExpectedLootMin { get; set; } = 1;          // pickups físicos mínimos al morir
	}

	/// <summary>
	/// EnemySuite — suite ILabSuite del dominio Enemy (infra QA, SOLO dev).
	///
	/// Valida el bucle completo de enemigo portable por la RUTA REAL:
	///   spawn (prefab) → host carga definición → percepción detecta target
	///   (visión real + sub-check oído) → NavMeshAgent navega de verdad (t0→t1→t2,
	///   prohibido teleport) → EnemyAttack golpea vía IDamageTarget (delta HP del
	///   dummy) → el enemigo recibe daño por IDamageTarget → muere → loot físico
	///   (pickups LootPickupContent) → PASS/FAIL.
	///
	/// Anti-falsificación: la suite NUNCA llama internals del host para fabricar
	/// t0/t1/t2 ni el daño; solo sustituye input humano y target humano (dummy).
	/// La navegación se mide con el NavMeshAgent real (AgentPosition/IsNavigating);
	/// no se toca la API Scene.NavMesh (no documentada en el engine) — si el agente
	/// no navega, la suite hace FAIL con diagnóstico claro.
	///
	/// El rig de escena (EnemyTestRig) construye una instancia por entrada y la
	/// registra en ContentRuntimeSuite; el runner unificado ejecuta Initialize/Step
	/// y emite [UBSuite] Enemy.&lt;Label&gt; PASS|FAIL.
	/// </summary>
	public sealed class EnemySuite : ILabSuite
	{
		public string Domain => "Enemy";
		public string Name => _entry.Label;
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private readonly EnemyTestEntry _entry;
		private readonly int _testNumber;
		private readonly int _testTotal;
		private readonly GameObject _dummy;
		private readonly LabDamageDummy _dummyDamage;
		private readonly GameObject _spawnMarker;
		private readonly GameObject _dummyMarker;

		private GameObject _enemy;
		private EnemyContentHost _host;
		private int _phase;
		private TimeSince _phaseTimer;
		private bool _fail;
		private string _failReason = "";
		private Vector3 _spawnPosition;
		private float _t0;
		private float _t1;
		private float _t2;
		private bool _t1Logged;
		private bool _attackObserved;
		private float _lastDummyHealth;
		private float _firstDelta;
		private int _lootBaseline;
		private bool _lootChecked;
		private float _elapsed;
		private bool _initialized;

		public EnemySuite( EnemyTestEntry entry, int testNumber, int testTotal, GameObject dummy, LabDamageDummy dummyDamage, GameObject spawnMarker, GameObject dummyMarker )
		{
			_entry = entry ?? throw new ArgumentNullException( nameof( entry ) );
			_testNumber = testNumber;
			_testTotal = testTotal;
			_dummy = dummy;
			_dummyDamage = dummyDamage;
			_spawnMarker = spawnMarker;
			_dummyMarker = dummyMarker;
		}

		public void Initialize()
		{
			if ( _initialized ) return;
			_initialized = true;

			_phase = 0;
			_phaseTimer = 0f;
			_fail = false;
			_failReason = "";
			_t1Logged = false;
			_attackObserved = false;
			_lootChecked = false;
			_firstDelta = 0f;
			_elapsed = 0f;
			_t0 = 0f;
			_t1 = 0f;
			_t2 = 0f;

			Log.Info( $"[EnemyLab] === Test {_testNumber}/{_testTotal}: {_entry.Label} ===" );

			if ( _spawnMarker == null || _dummyMarker == null )
			{
				Fail( "rig sin SpawnMarker/DummyMarker (coloca ambos sobre NavMesh en el editor)" );
				return;
			}

			// Dummy fijo sobre NavMesh (posición del DummyMarker; el enemigo mira hacia
			// +X con rotación identidad → detección determinista).
			_dummy.WorldPosition = _dummyMarker.WorldPosition;
			_dummy.WorldRotation = Rotation.Identity;
			_dummyDamage.ResetHealth();
			_lastDummyHealth = _dummyDamage.Health;

			_lootBaseline = _dummy.Scene.GetAllComponents<LootPickupContent>().Count();

			SpawnEnemy();
		}

		public void Step( float dt )
		{
			if ( IsComplete || _fail ) return;

			_elapsed += dt; // TimeSince avanza solo con tiempo real; no sumar dt al phase timer

			switch ( _phase )
			{
				case 0: WaitHostStart(); break;
				case 1: WaitDetection(); break;
				case 2: WaitMovement(); break;
				case 3: WaitAttack(); break;
				case 4: DamageEnemy(); break;
				case 5: WaitLoot(); break;
			}
		}

		// --- Fase 0: el OnStart del host corre tras el frame de creación ---

		private void WaitHostStart()
		{
			if ( _phaseTimer < 0.6f ) return;

			_phase = 1;
			_phaseTimer = 0f;

			if ( _host == null || !_host.IsValid() )
			{
				Fail( "EnemyContentHost no encontrado en el prefab" );
				return;
			}

			if ( _host.Definition == null )
			{
				Fail( $"definición NULL (DefinitionId '{_host.DefinitionId}' no registrada)" );
				return;
			}

			if ( !string.IsNullOrEmpty( _entry.ExpectedDefinitionId ) && _host.Definition.Id != _entry.ExpectedDefinitionId )
			{
				Fail( $"definición inesperada: {_host.Definition.Id} (esperada {_entry.ExpectedDefinitionId})" );
				return;
			}

			var agent = _host.Agent;
			Log.Info( $"[EnemyLab] {_entry.Label} NavMeshAgent válido (MaxSpeed={agent.MaxSpeed:F0}, pos={agent.AgentPosition})" );
			Log.Info( $"[EnemyLab] {_entry.Label} Definición: HP {_host.Definition.MaxHealth} | dmg {_host.Definition.AttackDamage} | rango {_host.Definition.AttackRange} | visión {_host.Definition.VisionRange}u/{_host.Definition.VisionAngle}° | oído {_host.Definition.HearingRadius}u | loot '{_host.Definition.LootTableId}'" );
			Log.Info( $"[EnemyLab] {_entry.Label} NavMeshAgent válido → OK" );
		}

		// --- Fase 1: percepción detecta al target (visión real) + sub-check de oído ---

		private void WaitDetection()
		{
			if ( _host == null || !_host.IsValid() )
			{
				Fail( "enemigo destruido durante detección" );
				return;
			}

			if ( _host.IsTargetAcquired )
			{
				Log.Info( $"[EnemyLab] {_entry.Label} Detectado target (percepción visión)" );

				// Sub-check de oído por la ruta real (IEnemyContentAdapter.ReportNoise).
				_host.ReportNoise( _dummy.WorldPosition, 1f );
				Log.Info( _host.LastKnownPosition.HasValue
					? $"[EnemyLab] {_entry.Label} Oído OK (última posición conocida {_host.LastKnownPosition.Value})"
					: $"[EnemyLab] {_entry.Label} Oído FAIL (sin memoria tras ruido)" );

				_phase = 2;
				_phaseTimer = 0f;
				return;
			}

			if ( _phaseTimer >= 10f )
			{
				Fail( "timeout detectando target (¿NavMesh/posición? revisar logs del host)" );
			}
		}

		// --- Fase 2: el agente navega de verdad (distancia disminuye t0→t1, llega t2) ---

		private void WaitMovement()
		{
			if ( _host == null || !_host.IsValid() )
			{
				Fail( "enemigo destruido durante movimiento" );
				return;
			}

			float distance = Vector3.DistanceBetween( _enemy.WorldPosition, _dummy.WorldPosition );
			float moved = Vector3.DistanceBetween( _enemy.WorldPosition, _spawnPosition );

			if ( !_t1Logged && ( distance < _t0 - 25f || moved > 25f ) )
			{
				_t1 = distance;
				_t1Logged = true;
				Log.Info( $"[EnemyLab] {_entry.Label} Navegando (NavMeshAgent) → t1={_t1:F0} (t0={_t0:F0}, disminuye {_t0 - _t1:F0})" );
				Log.Info( $"[EnemyLab] {_entry.Label} Distancia disminuye → OK" );
			}

			// Reasignamos el destino si el agente perdió la ruta ANTES de llegar (objetivo
			// estático; no debería pasar). Tras llegar, el host hace Agent.Stop() y el
			// rig no debe pelear con él.
			if ( !_t1Logged && _host.Agent != null && !_host.Agent.IsNavigating )
			{
				_host.Agent.MoveTo( _dummy.WorldPosition );
			}

			if ( _phaseTimer >= 15f && !_t1Logged )
			{
				Fail( $"timeout: el agente no navega (distancia {distance:F0}, movido {moved:F0}). Revisar NavMesh/mapa." );
				return;
			}

			if ( _t1Logged && distance <= _host.Definition.AttackRange + 10f )
			{
				_t2 = distance;
				_phase = 3;
				_phaseTimer = 0f;
				_lastDummyHealth = _dummyDamage.Health;
				Log.Info( $"[EnemyLab] {_entry.Label} Llegó al target → t2={_t2:F0} (rango ataque {_host.Definition.AttackRange})" );
			}
		}

		// --- Fase 3: ataca → el target pierde HP por la ruta real (IDamageTarget) ---

		private void WaitAttack()
		{
			if ( _host == null || !_host.IsValid() )
			{
				Fail( "enemigo destruido durante ataque" );
				return;
			}

			float current = _dummyDamage.Health;

			if ( !_attackObserved && current < _lastDummyHealth )
			{
				_attackObserved = true;
				_firstDelta = _lastDummyHealth - current;
				Log.Info( $"[EnemyLab] {_entry.Label} Ataca → target pierde HP (delta {_firstDelta:F1})" );
				if ( _firstDelta >= _entry.ExpectedDamagePerHit - 0.5f )
				{
					Log.Info( $"[EnemyLab] {_entry.Label} Daño por golpe OK (>= {_entry.ExpectedDamagePerHit:F0})" );
				}
				else
				{
					Fail( $"daño por golpe bajo: {_firstDelta:F1} < {_entry.ExpectedDamagePerHit:F0}" );
					return;
				}
			}

			_lastDummyHealth = current;

			if ( _attackObserved && _phaseTimer >= 2f )
			{
				_phase = 4;
				_phaseTimer = 0f;
				Log.Info( $"[EnemyLab] {_entry.Label} Ataca → target pierde HP → OK" );
			}

			if ( _phaseTimer >= 30f && !_attackObserved )
			{
				Fail( $"timeout: el enemigo no ataca (distancia {Vector3.DistanceBetween( _enemy.WorldPosition, _dummy.WorldPosition ):F0})" );
			}
		}

		// --- Fase 4: el enemigo recibe daño por IDamageTarget → muere ---

		private void DamageEnemy()
		{
			// Ya murió (Die() destruye el GameObject): contamos solo si pasó el golpe.
			if ( _host == null || !_host.IsValid() )
			{
				if ( _phaseTimer >= 0.5f )
				{
					Log.Info( $"[EnemyLab] {_entry.Label} Enemigo recibió daño → murió → OK" );
					_phase = 5;
					_phaseTimer = 0f;
				}
				else
				{
					Fail( "enemigo destruido antes de recibir daño" );
				}
				return;
			}

			if ( _phaseTimer < 0.5f )
			{
				// Primer golpe: 25% de la vida (daño incremental por la ruta real).
				_host.TakeDamage( new ContentDamageEvent
				{
					Amount = _host.Definition.MaxHealth * 0.25f,
					Position = _host.WorldPosition,
					Force = Vector3.Zero,
					SourceId = "EnemySuite"
				} );
			}
			else if ( _phaseTimer >= 1f && _phaseTimer < 1.5f )
			{
				// Segundo golpe: letal (la ruta real decide la muerte).
				if ( !_host.IsDead )
				{
					_host.TakeDamage( new ContentDamageEvent
					{
						Amount = _host.Definition.MaxHealth + 999f,
						Position = _host.WorldPosition,
						Force = Vector3.Zero,
						SourceId = "EnemySuite"
					} );
				}
			}
			else if ( _phaseTimer >= 2.5f )
			{
				// El host sigue vivo tras el golpe letal: la muerte no ocurrió.
				Fail( $"el enemigo no murió tras daño letal (HP {_host.Health})" );
			}
		}

		// --- Fase 5: loot físico aparece en el mundo ---

		private void WaitLoot()
		{
			if ( _phaseTimer >= 1.5f && !_lootChecked )
			{
				_lootChecked = true;
				int loot = _dummy.Scene.GetAllComponents<LootPickupContent>().Count() - _lootBaseline;
				Log.Info( $"[EnemyLab] {_entry.Label} Loot físico: {loot} pickups (mínimo esperado {_entry.ExpectedLootMin})" );

				if ( loot >= _entry.ExpectedLootMin )
				{
					Log.Info( $"[EnemyLab] {_entry.Label} Loot aparece físicamente → OK" );
				}
				else
				{
					Fail( $"loot insuficiente: {loot} < {_entry.ExpectedLootMin}" );
					return;
				}
			}

			if ( _lootChecked && _phaseTimer >= 2.2f )
			{
				FinishTest();
			}
		}

		// --- Spawn / limpieza ---

		private void SpawnEnemy()
		{
			var prefabFile = ResourceLibrary.Get<PrefabFile>( _entry.EnemyPrefab );
			if ( prefabFile == null )
			{
				Fail( $"prefab NO encontrado: {_entry.EnemyPrefab}" );
				return;
			}

			_spawnPosition = _spawnMarker.WorldPosition;

			// Instanciar con TRANSFORM inicial (API real: GameObject.Clone(PrefabFile, Transform, ...)):
			// el NavMeshAgent (RequireComponent) se crea con el GO YA en el spawn, asi el ancla
			// del NavMesh nace en el spawn (CASE 2 fix en capa de spawn, portable a RaidSpawner).
			var enemy = GameObject.Clone( prefabFile, new Transform( _spawnPosition, Rotation.Identity ) );
			enemy.NetworkSpawn( Connection.Local );
			_enemy = enemy;

			_host = enemy.Components.Get<EnemyContentHost>();
			if ( _host == null )
			{
				Fail( "EnemyContentHost no encontrado en el prefab" );
				return;
			}

			_host.SetTarget( _dummy );


			_t0 = Vector3.DistanceBetween( _enemy.WorldPosition, _dummy.WorldPosition );
			Log.Info( $"[EnemyLab] {_entry.Label} Spawn en {_spawnPosition} → t0={_t0:F0}" );
		}

		private void Cleanup()
		{
			if ( _enemy != null && _enemy.IsValid() )
			{
				_enemy.Destroy();
			}
			_enemy = null;
			_host = null;

			// Limpieza de loot del test para mantener el baseline estable.
			foreach ( var loot in _dummy.Scene.GetAllComponents<LootPickupContent>() )
			{
				if ( loot.IsValid() ) loot.GameObject.Destroy();
			}

			if ( _dummy != null && _dummy.IsValid() )
			{
				_dummyDamage.ResetHealth();
			}
		}

		private void Fail( string reason )
		{
			_fail = true;
			_failReason = reason;
			Log.Error( $"[EnemyLab] {_entry.Label} FAIL: {reason}" );
			FinishTest();
		}

		private void FinishTest()
		{
			bool pass = !_fail && _t1Logged && _attackObserved;
			Log.Info( pass ? $"[EnemyLab] {_entry.Label} PASS" : $"[EnemyLab] {_entry.Label} FAIL" );

			Result = pass
				? LabSuiteResult.Pass( _elapsed, _firstDelta, "complete" )
				: LabSuiteResult.Fail( _elapsed, _firstDelta, $"fail:{_failReason}" );
			IsComplete = true;

			Cleanup();
		}
	}
}
 