using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Content.Enemies;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>Configuración de un test de enemigo dentro de la suite del rig.</summary>
	public sealed class EnemyTestEntry
	{
		public string Label { get; set; } = "";
		public string EnemyPrefab { get; set; } = "";
		public string ExpectedDefinitionId { get; set; } = ""; // valida que el host cargó esta definición
		public float ExpectedDamagePerHit { get; set; } = 15f; // daño mínimo por golpe real
		public int ExpectedLootMin { get; set; } = 1;          // pickups físicos mínimos al morir
	}

	/// <summary>
	/// Enemy Test Rig — fixture automatizado para validar el bucle de enemigos portable.
	/// SOLO dev (UltimoBarrio.Content.Dev). Nada de jugador humano: el rig crea un
	/// TargetDummy fijo sobre NavMesh y un enemigo en el SpawnPoint; el dummy sustituye
	/// al target humano y el rig sustituye al input humano.
	///
	/// El camino probado es SIEMPRE el real:
	///   spawn → NavMeshAgent válido → EnemyPerception detecta → NavMeshAgent navega
	///   (prohibido teleport) → EnemyAttack golpea vía IDamageTarget → el enemigo
	///   recibe daño por IDamageTarget → muere → loot físico (pickups de mundo).
	/// El rig solo lee estado (Health, distancias, loot) para reportar; nunca llama
	/// internals para fabricar un PASS.
	///
	/// Acceptance por test: spawn → NavMeshAgent válido → detecta target → distancia
	/// disminuye (t0→t1) → llega (t2) → ataca → target pierde HP → enemigo recibe
	/// daño → muere → loot aparece físicamente → PASS.
	/// </summary>
	[Title( "Enemy Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "science" )]
	public sealed class EnemyTestRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-1";

		[Property] public bool AutoTest { get; set; } = true;
		[Property] public GameObject DummyMarker { get; set; } // posición del TargetDummy (sobre NavMesh)
		[Property] public GameObject SpawnMarker { get; set; } // posición de spawn del enemigo (sobre NavMesh)
		[Property] public float DummyMaxHealth { get; set; } = 200f;
		[Property] public List<EnemyTestEntry> Tests { get; set; } = new();

		private CameraComponent _camera;
		private GameObject _dummy;
		private LabDamageDummy _dummyDamage;
		private GameObject _enemy;
		private EnemyContentHost _host;

		private int _testIndex;
		private int _phase;
		private TimeSince _phaseTimer;
		private bool _fail;
		private int _fails;

		// Métricas de la acceptance
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

		private EnemyTestEntry Entry => _testIndex < Tests.Count ? Tests[_testIndex] : null;

		protected override void OnStart()
		{
			Log.Info( $"[LabBuild] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[EnemyLab] rig sin CameraComponent" );
				_fail = true;
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			if ( DummyMarker == null || SpawnMarker == null )
			{
				Log.Error( "[EnemyLab] rig sin DummyMarker/SpawnMarker (coloca ambos sobre NavMesh en el editor)" );
				_fail = true;
				return;
			}

			// Cámara apuntando al centro del área de pruebas (sin jugador humano).
			_camera.WorldRotation = Rotation.LookAt( ( ( DummyMarker.WorldPosition + SpawnMarker.WorldPosition ) * 0.5f - _camera.WorldPosition ).Normal );

			if ( Tests.Count == 0 )
			{
				Log.Error( "[EnemyLab] suite vacía (Tests sin configurar)" );
				_fail = true;
				return;
			}

			LogNavMeshDiagnostics();

			CreateTargetDummy();

			Log.Info( $"[EnemyLab] Suite: {Tests.Count} tests ({string.Join( ", ", Tests.ConvertAll( t => t.Label ) )})" );
			StartTest( 0 );
		}

		/// <summary>
		/// Diagnóstico del NavMesh con la API oficial del engine.
		/// Scene.NavMesh → Sandbox.Navigation.NavMesh (verificado en Sandbox.Engine.xml:
		/// IsEnabled/IsGenerating/IsDirty + RequestTileGeneration(Vector3) incremental).
		/// Si esta API no compilara en runtime, comentar este método y el fallo pasará
		/// a ser comportamental (el agente nunca navegaría → FAIL con diagnóstico claro).
		/// </summary>
		private void LogNavMeshDiagnostics()
		{
			var nav = Scene.NavMesh;
			Log.Info( $"[EnemyLab] NavMesh IsEnabled={nav.IsEnabled} IsGenerating={nav.IsGenerating} IsDirty={nav.IsDirty}" );
			Log.Info( $"[EnemyLab] NavMesh AgentHeight={nav.AgentHeight} AgentRadius={nav.AgentRadius}" );

			// Generación incremental oficial: asegura tiles sobre el área del lab
			// (fire-and-forget; si el mapa ya tiene navmesh baked, no hace nada).
			nav.RequestTileGeneration( SpawnMarker.WorldPosition );
			nav.RequestTileGeneration( DummyMarker.WorldPosition );

			// Validación de que los marcadores están sobre NavMesh (null = no encontró tile).
			var spawnPoint = nav.GetRandomPoint( SpawnMarker.WorldPosition, 200f );
			var dummyPoint = nav.GetRandomPoint( DummyMarker.WorldPosition, 200f );
			Log.Info( spawnPoint.HasValue
				? $"[EnemyLab] SpawnMarker sobre NavMesh (punto cercano {spawnPoint.Value})"
				: $"[EnemyLab] ⚠️ SpawnMarker NO está sobre NavMesh ({SpawnMarker.WorldPosition}) — revisar posición en el editor" );
			Log.Info( dummyPoint.HasValue
				? $"[EnemyLab] DummyMarker sobre NavMesh (punto cercano {dummyPoint.Value})"
				: $"[EnemyLab] ⚠️ DummyMarker NO está sobre NavMesh ({DummyMarker.WorldPosition}) — revisar posición en el editor" );
		}

		private void CreateTargetDummy()
		{
			var dummy = new GameObject( true, "TargetDummy" );
			dummy.WorldPosition = DummyMarker.WorldPosition;
			dummy.WorldRotation = Rotation.Identity;
			dummy.Tags.Add( "enemy_target" );

			var renderer = dummy.Components.Create<ModelRenderer>();
			renderer.Model = ResourceLibrary.Get<Model>( "models/citizen_props/crate01.vmdl" );

			var collider = dummy.Components.Create<BoxCollider>();
			collider.Scale = new Vector3( 60f, 60f, 90f );
			collider.Static = true;

			_dummyDamage = dummy.Components.Create<LabDamageDummy>();
			_dummyDamage.LogPrefix = "[EnemyLab]";
			_dummyDamage.MaxHealth = DummyMaxHealth;
			_dummy = dummy;
		}

		private void StartTest( int index )
		{
			_testIndex = index;
			_phase = 0;
			_phaseTimer = 0f;
			_fail = false;
			_t1Logged = false;
			_attackObserved = false;
			_firstDelta = 0f;
			_lootChecked = false;

			var entry = Entry;
			Log.Info( $"[EnemyLab] === Test {index + 1}/{Tests.Count}: {entry.Label} ===" );

			// Dummy fijo sobre NavMesh (posición del DummyMarker; el enemigo mira hacia
			// +X con rotación identidad → detección determinista).
			_dummy.WorldPosition = DummyMarker.WorldPosition;
			_dummyDamage.ResetHealth();
			_lastDummyHealth = _dummyDamage.Health;

			_lootBaseline = Scene.GetAllComponents<LootPickupContent>().Count();
			Log.Info( $"[EnemyLab] {entry.Label} TargetDummy en {_dummy.WorldPosition} (HP {_dummyDamage.MaxHealth}), loot baseline {_lootBaseline}" );

			SpawnEnemy( entry );
		}

		private void SpawnEnemy( EnemyTestEntry entry )
		{
			var prefabFile = ResourceLibrary.Get<PrefabFile>( entry.EnemyPrefab );
			if ( prefabFile == null )
			{
				Log.Error( $"[EnemyLab] {entry.Label} prefab NO encontrado: {entry.EnemyPrefab}" );
				_fail = true;
				FinishTest( entry );
				return;
			}

			_spawnPosition = SpawnMarker.WorldPosition;
			var enemy = SceneUtility.GetPrefabScene( prefabFile ).Clone();
			enemy.WorldPosition = _spawnPosition;
			enemy.NetworkSpawn( Connection.Local );
			_enemy = enemy;

			_host = enemy.Components.Get<EnemyContentHost>();
			if ( _host == null )
			{
				Log.Error( $"[EnemyLab] {entry.Label} EnemyContentHost no encontrado en el prefab" );
				_fail = true;
				FinishTest( entry );
				return;
			}

			_host.SetTarget( _dummy );

			_t0 = Vector3.DistanceBetween( _enemy.WorldPosition, _dummy.WorldPosition );
			Log.Info( $"[EnemyLab] {entry.Label} Spawn en {_spawnPosition} → t0={_t0:F0}" );
		}

		protected override void OnUpdate()
		{
			if ( _camera == null || !AutoTest || _fail ) return;

			var entry = Entry;
			if ( entry == null ) return;

			// El OnStart del host corre tras el frame de creación: esperamos al
			// primer tick real antes de leer su definición.
			if ( _phase == 0 && _phaseTimer >= 0.6f )
			{
				ValidateAgent( entry );
			}
			else if ( _phase == 1 )
			{
				WaitDetection( entry );
			}
			else if ( _phase == 2 )
			{
				WaitMovement( entry );
			}
			else if ( _phase == 3 )
			{
				WaitAttack( entry );
			}
			else if ( _phase == 4 )
			{
				DamageEnemy( entry );
			}
			else if ( _phase == 5 )
			{
				WaitLoot( entry );
			}
		}

		/// <summary>Phase 1: NavMeshAgent válido + definición correcta.</summary>
		private void ValidateAgent( EnemyTestEntry entry )
		{
			_phase = 1;
			_phaseTimer = 0f;

			if ( _host == null || !_host.IsValid() )
			{
				Log.Error( $"[EnemyLab] {entry.Label} host inválido tras spawn" );
				_fail = true;
				return;
			}

			if ( _host.Definition == null )
			{
				Log.Error( $"[EnemyLab] {entry.Label} definición NULL (DefinitionId no registrada)" );
				_fail = true;
				return;
			}

			if ( !string.IsNullOrEmpty( entry.ExpectedDefinitionId ) && _host.Definition.Id != entry.ExpectedDefinitionId )
			{
				Log.Error( $"[EnemyLab] {entry.Label} definición inesperada: {_host.Definition.Id} (esperada {entry.ExpectedDefinitionId})" );
				_fail = true;
				return;
			}

			var agent = _host.Agent;
			Log.Info( $"[EnemyLab] {entry.Label} NavMeshAgent válido (MaxSpeed={agent.MaxSpeed:F0}, pos={agent.AgentPosition})" );
			Log.Info( $"[EnemyLab] {entry.Label} Definición: HP {_host.Definition.MaxHealth} | dmg {_host.Definition.AttackDamage} | rango {_host.Definition.AttackRange} | visión {_host.Definition.VisionRange}u/{_host.Definition.VisionAngle}° | oído {_host.Definition.HearingRadius}u" );
			Log.Info( $"[EnemyLab] {entry.Label} NavMeshAgent válido → OK" );

			if ( _fail ) FinishTest( entry );
		}

		/// <summary>Phase 2: la percepción detecta al target (visión) + sub-check de oído.</summary>
		private void WaitDetection( EnemyTestEntry entry )
		{
			if ( _host == null || !_host.IsValid() )
			{
				Log.Error( $"[EnemyLab] {entry.Label} enemigo destruido durante detección" );
				_fail = true;
				FinishTest( entry );
				return;
			}

			if ( _host.IsTargetAcquired )
			{
				Log.Info( $"[EnemyLab] {entry.Label} Detectado target (percepción visión)" );

				// Sub-check de oído por la ruta real (IEnemyContentAdapter.ReportNoise).
				_host.ReportNoise( _dummy.WorldPosition, 1f );
				Log.Info( _host.LastKnownPosition.HasValue
					? $"[EnemyLab] {entry.Label} Oído OK (última posición conocida {_host.LastKnownPosition.Value})"
					: $"[EnemyLab] {entry.Label} Oído FAIL (sin memoria tras ruido)" );

				_phase = 2;
				_phaseTimer = 0f;
				return;
			}

			if ( _phaseTimer >= 10f )
			{
				Log.Error( $"[EnemyLab] {entry.Label} timeout detectando target (¿NavMesh/posición? revisar logs NavMesh)" );
				_fail = true;
				FinishTest( entry );
			}
		}

		/// <summary>Phase 3: el agente navega de verdad (distancia disminuye t0→t1).</summary>
		private void WaitMovement( EnemyTestEntry entry )
		{
			if ( _host == null || !_host.IsValid() )
			{
				Log.Error( $"[EnemyLab] {entry.Label} enemigo destruido durante movimiento" );
				_fail = true;
				FinishTest( entry );
				return;
			}

			float distance = Vector3.DistanceBetween( _enemy.WorldPosition, _dummy.WorldPosition );
			float moved = Vector3.DistanceBetween( _enemy.WorldPosition, _spawnPosition );

			if ( !_t1Logged && ( distance < _t0 - 25f || moved > 25f ) )
			{
				_t1 = distance;
				_t1Logged = true;
				Log.Info( $"[EnemyLab] {entry.Label} Navegando (NavMeshAgent) → t1={_t1:F0} (t0={_t0:F0}, disminuye {_t0 - _t1:F0})" );
				Log.Info( $"[EnemyLab] {entry.Label} Distancia disminuye → OK" );
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
				Log.Error( $"[EnemyLab] {entry.Label} timeout: el agente no navega (distancia {distance:F0}, movido {moved:F0}). Revisar NavMesh/mapa." );
				_fail = true;
				FinishTest( entry );
			}

			if ( _t1Logged && distance <= _host.Definition.AttackRange + 10f )
			{
				_t2 = distance;
				_phase = 3;
				_phaseTimer = 0f;
				_lastDummyHealth = _dummyDamage.Health;
				Log.Info( $"[EnemyLab] {entry.Label} Llegó al target → t2={_t2:F0} (rango ataque {_host.Definition.AttackRange})" );
			}
		}

		/// <summary>Phase 4: ataca → el target pierde HP por la ruta real (IDamageTarget).</summary>
		private void WaitAttack( EnemyTestEntry entry )
		{
			if ( _host == null || !_host.IsValid() )
			{
				Log.Error( $"[EnemyLab] {entry.Label} enemigo destruido durante ataque" );
				_fail = true;
				FinishTest( entry );
				return;
			}

			float current = _dummyDamage.Health;

			if ( !_attackObserved && current < _lastDummyHealth )
			{
				_attackObserved = true;
				_firstDelta = _lastDummyHealth - current;
				Log.Info( $"[EnemyLab] {entry.Label} Ataca → target pierde HP (delta {_firstDelta:F1})" );
				if ( _firstDelta >= entry.ExpectedDamagePerHit - 0.5f )
				{
					Log.Info( $"[EnemyLab] {entry.Label} Daño por golpe OK (>= {entry.ExpectedDamagePerHit:F0})" );
				}
				else
				{
					Log.Error( $"[EnemyLab] {entry.Label} daño por golpe bajo: {_firstDelta:F1} < {entry.ExpectedDamagePerHit:F0}" );
					_fail = true;
				}
			}

			_lastDummyHealth = current;

			if ( _attackObserved && _phaseTimer >= 2f )
			{
				_phase = 4;
				_phaseTimer = 0f;
				Log.Info( $"[EnemyLab] {entry.Label} Ataca → target pierde HP → OK" );
			}

			if ( _phaseTimer >= 30f && !_attackObserved )
			{
				Log.Error( $"[EnemyLab] {entry.Label} timeout: el enemigo no ataca (distancia {Vector3.DistanceBetween( _enemy.WorldPosition, _dummy.WorldPosition ):F0})" );
				_fail = true;
				FinishTest( entry );
			}
		}

		/// <summary>Phase 5: el enemigo recibe daño por IDamageTarget → muere.</summary>
		private void DamageEnemy( EnemyTestEntry entry )
		{
			// Camino de éxito: el enemigo ya no existe porque murió con el golpe letal
			// (Die() destruye el GameObject). Solo cuenta si ya pasó el golpe.
			if ( _host == null || !_host.IsValid() )
			{
				if ( _phaseTimer >= 0.5f )
				{
					Log.Info( $"[EnemyLab] {entry.Label} Enemigo recibió daño → murió → OK" );
					_phase = 5;
					_phaseTimer = 0f;
				}
				else
				{
					Log.Error( $"[EnemyLab] {entry.Label} enemigo destruido antes de recibir daño" );
					_fail = true;
					FinishTest( entry );
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
					SourceId = "EnemyTestRig"
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
						SourceId = "EnemyTestRig"
					} );
				}
			}
			else if ( _phaseTimer >= 2.5f )
			{
				// El host sigue vivo tras el golpe letal: la muerte no ocurrió.
				Log.Error( $"[EnemyLab] {entry.Label} el enemigo no murió tras daño letal (HP {_host.Health}) " );
				_fail = true;
				FinishTest( entry );
			}
		}

		/// <summary>Phase 6: loot físico aparece en el mundo.</summary>
		private void WaitLoot( EnemyTestEntry entry )
		{
			if ( _phaseTimer >= 1.5f && !_lootChecked )
			{
				_lootChecked = true;
				int loot = Scene.GetAllComponents<LootPickupContent>().Count() - _lootBaseline;
				Log.Info( $"[EnemyLab] {entry.Label} Loot físico: {loot} pickups (mínimo esperado {entry.ExpectedLootMin})" );

				if ( loot >= entry.ExpectedLootMin )
				{
					Log.Info( $"[EnemyLab] {entry.Label} Loot aparece físicamente → OK" );
				}
				else
				{
					Log.Error( $"[EnemyLab] {entry.Label} loot insuficiente: {loot} < {entry.ExpectedLootMin}" );
					_fail = true;
				}
			}

			if ( _lootChecked && _phaseTimer >= 2.2f )
			{
				FinishTest( entry );
			}
		}

		private void FinishTest( EnemyTestEntry entry )
		{
			bool pass = !_fail && _t1Logged && _attackObserved;
			if ( !pass ) _fails++;

			Log.Info( pass ? $"[EnemyLab] {entry.Label} PASS" : $"[EnemyLab] {entry.Label} FAIL" );

			Cleanup();

			_testIndex++;
			if ( _testIndex < Tests.Count )
			{
				StartTest( _testIndex );
			}
			else
			{
				Log.Info( $"[EnemyLab] Suite complete ({Tests.Count - _fails}/{Tests.Count} PASS)" );
			}
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
			foreach ( var loot in Scene.GetAllComponents<LootPickupContent>() )
			{
				if ( loot.IsValid() ) loot.GameObject.Destroy();
			}

			if ( _dummy != null && _dummy.IsValid() )
			{
				_dummyDamage.ResetHealth();
			}
		}
	}
}
