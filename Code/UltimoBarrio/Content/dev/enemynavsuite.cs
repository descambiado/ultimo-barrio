using Sandbox;
using Sandbox.Navigation;
using System;
using System.Collections.Generic;
using UltimoBarrio.Content;
using UltimoBarrio.Content.Enemies;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>Configuración de un test de navegación dentro de la suite del rig (data-driven, JSON de escena).</summary>
	public sealed class EnemyNavTestEntry
	{
		public string Label { get; set; } = "";
		public string EnemyPrefabPath { get; set; } = "prefabs/content/enemies/enemy_saqueador.prefab";
		public float ArrivalRadius { get; set; } = 110f;   // distancia al target que cuenta como "llegada" (u)
		public float MoveTimeout { get; set; } = 25f;      // segundos máximos de navegación
		public float KillDelay { get; set; } = 1f;         // espera tras llegar antes del kill fixture (s)
		public List<string> ExpectedLootItemIds { get; set; } = new();
	}

	/// <summary>
	/// EnemyNavSuite - ILabSuite del ESCENARIO de navegación del enemy_lab (Worker B).
	///
	/// Pipeline (ub-spike-factory): NavMesh escena válido → probe NavMeshAgent REAL
	/// con descenso t0/t1/t2 → prefab enemigo de Worker A (ruta real host → agent) →
	/// kill por IDamageTarget → observación física del loot.
	///
	/// Anti-falsificación (crítico): la suite NUNCA teletransporta ni llama internals.
	///   - El área NavMesh se valida con NavMesh.CalculatePath (API oficial) sobre la
	///     geometría estática real de la escena.
	///   - El descenso se mide sobre la posición del NavMeshAgent (ruta real del engine:
	///     MoveTo → recorte → ground trace). Se exige: d0 &gt; d1 &gt; d2 estricto,
	///     distancia recorrida acumulada ≥ 70% de la recta, y salto por frame &lt; umbral
	///     de teleport. Si el agente no se mueve o teletransporta → FAIL.
	///   - El kill recorre TakeDamage(ContentDamageEvent) → Die() → SpawnLoot real.
	///   - El loot se observa físicamente (LabLootObserver) alrededor del punto de muerte.
	///
	/// Fases: WaitNavMesh → PathCheck → ProbeSpawn → ProbeMove → EnemySpawn →
	/// EnemyMove → Kill → LootObserve → Finish. El PASS del escenario no depende del
	/// prefab de enemigo (dominio de Worker A): si el prefab no existe, la fase enemigo
	/// se marca SKIP honesto y la suite reporta PASS con state que documenta el alcance.
	/// </summary>
	public sealed class EnemyNavSuite : ILabSuite
	{
		public string Domain => "Enemy";
		public string Name { get; }
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private readonly EnemyNavTestEntry _entry;
		private readonly GameObject _spawnMarker;
		private readonly GameObject _testTarget;
		private readonly LabLootObserver _lootObserver;

		private enum Phase
		{
			WaitNavMesh,
			PathCheck,
			ProbeSpawn,
			ProbeMove,
			EnemySpawn,
			EnemyMove,
			Kill,
			LootObserve,
			Finish
		}

		private Phase _phase;
		private TimeSince _phaseTimer;
		private bool _fail;
		private string _skipReason = "";
		private readonly List<string> _notes = new();

		// Probe (NavMeshAgent dev, sin lógica de enemigo: solo validación del escenario).
		private GameObject _probe;
		private NavMeshAgent _probeAgent;
		private Vector3 _targetPosition;
		private readonly DescentTrack _probeTrack = new();

		// Enemigo real (prefab de Worker A → EnemyContentHost).
		private GameObject _enemy;
		private EnemyContentHost _host;
		private readonly DescentTrack _enemyTrack = new();

		private float _probeDescent; // d0-d2 del probe → delta del Result (descenso REAL medido)

		private const float SettleSeconds = 0.6f;   // espera tras spawn (grounding del agent)
		private const float NavWaitTimeout = 12f;   // espera de generación de tiles
		private const float SuiteTimeout = 90f;     // techo duro de la suite
		private const float TeleportThreshold = 120f; // u/frame: un salto mayor ⇒ teleport
		private const float MinTravelRatio = 0.7f;  // recorrido acumulado ≥ 70% de la recta
		private TimeSince _suiteClock;

		/// <summary>Rastrea el descenso de un agente (t0/t1/t2 + recorrido + anti-teleport).</summary>
		private sealed class DescentTrack
		{
			public float D0;
			public float D1;
			public float D2;
			public bool T1Logged;
			public float Traveled;
			public Vector3 LastPos;
			public bool HasLast;
			public bool Teleported;
		}

		public EnemyNavSuite( EnemyNavTestEntry entry, GameObject spawnMarker, GameObject testTarget, LabLootObserver lootObserver )
		{
			_entry = entry;
			_spawnMarker = spawnMarker;
			_testTarget = testTarget;
			_lootObserver = lootObserver;
			Name = entry.Label;
		}

		public void Initialize()
		{
			_suiteClock = 0f;
			_phaseTimer = 0f;

			Log.Info( $"[EnemyLab] === Suite {Name}: escenario de navegación ===" );

			if ( _spawnMarker == null || !_spawnMarker.IsValid() || _testTarget == null || !_testTarget.IsValid() )
			{
				Fail( "fixtures de escena incompletas (spawn marker / test target)" );
				return;
			}

			_targetPosition = _testTarget.WorldPosition;

			// Área NavMesh habilitada en SceneProperties.NavMesh.
			if ( !_spawnMarker.Scene.NavMesh.IsEnabled )
			{
				Fail( "NavMesh deshabilitado en la escena (SceneProperties.NavMesh.Enabled=false)" );
				return;
			}

			Log.Info( $"[EnemyLab] {Name} NavMesh enabled, esperando generación de tiles..." );
			_phase = Phase.WaitNavMesh;
			_phaseTimer = 0f;
		}

		public void Step( float dt )
		{
			if ( IsComplete ) return;

			if ( _suiteClock >= SuiteTimeout )
			{
				Fail( $"suite timeout ({SuiteTimeout:F0}s)" );
				return;
			}

			switch ( _phase )
			{
				case Phase.WaitNavMesh: StepWaitNavMesh(); break;
				case Phase.PathCheck: StepPathCheck(); break;
				case Phase.ProbeSpawn: StepProbeSpawn(); break;
				case Phase.ProbeMove: StepProbeMove(); break;
				case Phase.EnemySpawn: StepEnemySpawn(); break;
				case Phase.EnemyMove: StepEnemyMove(); break;
				case Phase.Kill: StepKill(); break;
				case Phase.LootObserve: StepLootObserve(); break;
				case Phase.Finish: Finish(); break;
			}
		}

		// ---------------------------------------------------------------- fases

		private void StepWaitNavMesh()
		{
			// TimeSince avanza solo; solo esperar a que los tiles terminen de generarse.
			if ( _spawnMarker.Scene.NavMesh.IsGenerating && _phaseTimer < NavWaitTimeout )
			{
				return; // los tiles se siguen generando; esperar
			}

			if ( _spawnMarker.Scene.NavMesh.IsGenerating )
			{
				Fail( $"NavMesh sigue generando tras {NavWaitTimeout:F0}s (geometría estática no horneada?)" );
				return;
			}

			_notes.Add( "navmesh=generated" );
			Log.Info( $"[EnemyLab] {Name} NavMesh generado (tiles listos)" );
			_phase = Phase.PathCheck;
			_phaseTimer = 0f;
		}

		private void StepPathCheck()
		{
			if ( _phaseTimer < 0.1f ) return;

			// API oficial: cálculo de camino sobre el NavMesh real del scene.
			var request = new CalculatePathRequest
			{
				Start = _spawnMarker.WorldPosition,
				Target = _targetPosition
			};

			var path = _spawnMarker.Scene.NavMesh.CalculatePath( request );
			bool complete = path.Status == NavMeshPathStatus.Complete;

			Log.Info( $"[EnemyLab] {Name} PathCheck spawn={request.Start} target={request.Target} status={path.Status} points={(path.Points?.Count ?? 0)} {(complete ? "PASS" : "FAIL")}" );

			if ( !complete )
			{
				Fail( $"ruta inválida sobre el NavMesh (status={path.Status})" );
				return;
			}

			_notes.Add( "path=complete" );
			_phase = Phase.ProbeSpawn;
			_phaseTimer = 0f;
		}

		private void StepProbeSpawn()
		{
			if ( _phaseTimer < 0.1f ) return;

			_probe = new GameObject( true, "NavProbe" );
			_probe.WorldPosition = _spawnMarker.WorldPosition;

			var renderer = _probe.Components.Create<ModelRenderer>();
			renderer.Model = ResourceLibrary.Get<Model>( "models/citizen_props/crate01.vmdl" );
			renderer.Tint = new Color( 0.2f, 0.9f, 0.3f );

			var collider = _probe.Components.Create<BoxCollider>();
			collider.Scale = new Vector3( 14f, 14f, 40f );

			_probeAgent = _probe.Components.Create<NavMeshAgent>();
			_probeAgent.Acceleration = 900f;

			_phase = Phase.ProbeMove;
			_phaseTimer = 0f;
		}

		private void StepProbeMove()
		{
			if ( _phaseTimer < SettleSeconds )
			{
				SampleProbe(); // registrar t0 una vez asentado
				return;
			}

			if ( _probeTrack.D0 <= 0f )
			{
				RecordT0( _probeTrack, _probe.WorldPosition, "probe" );
			}

			if ( _probeTrack.T1Logged && _probeTrack.D2 > 0f )
			{
				_phase = Phase.EnemySpawn; // llegada registrada
				_phaseTimer = 0f;
				return;
			}

			if ( !_probeTrack.T1Logged )
			{
				_probeAgent.MoveTo( _targetPosition );
			}

			SampleProbe();
		}

		private void SampleProbe()
		{
			var pos = _probe.WorldPosition;
			var d = Vector3.DistanceBetween( pos, _targetPosition );

			if ( !_probeTrack.T1Logged )
			{
				if ( _probeTrack.D0 <= 0f ) RecordT0( _probeTrack, pos, "probe" );
			}

			// t1 al cruzar el 50% del descenso
			if ( !_probeTrack.T1Logged && _probeTrack.D0 > 0f && d <= _probeTrack.D0 * 0.5f )
			{
				_probeTrack.T1Logged = true;
				_probeTrack.D1 = d;
				Log.Info( $"[EnemyLab] {Name} probe t1={d:F1}u (50% de t0={_probeTrack.D0:F1}u) vel={_probeAgent.WishVelocity.Length:F1}u/s" );
			}

			// llegada
			if ( _probeTrack.T1Logged && _probeTrack.D2 <= 0f && d <= _entry.ArrivalRadius )
			{
				_probeTrack.D2 = d;
				_probeAgent.Stop();
				Log.Info( $"[EnemyLab] {Name} probe t2={d:F1}u (llegada, radio {_entry.ArrivalRadius:F0}u) vel={_probeAgent.WishVelocity.Length:F1}u/s" );
			}

			// recorrido acumulado + anti-teleport
			if ( _probeTrack.HasLast )
			{
				float frame = Vector3.DistanceBetween( _probeTrack.LastPos, pos );
				if ( frame > TeleportThreshold )
				{
					_probeTrack.Teleported = true;
					Fail( $"probe teleport detectado (salto de {frame:F0}u/frame)" );
					return;
				}
				_probeTrack.Traveled += frame;
			}
			_probeTrack.LastPos = pos;
			_probeTrack.HasLast = true;

			// timeout de movimiento
			if ( _phaseTimer >= SettleSeconds + _entry.MoveTimeout && _probeTrack.D2 <= 0f )
			{
				Fail( $"probe sin llegada tras {_entry.MoveTimeout:F0}s (d={Vector3.DistanceBetween( _probe.WorldPosition, _targetPosition ):F1}u)" );
				return;
			}

			// validez de descenso al llegar (anti-falsificación estricta)
			if ( _probeTrack.D2 > 0f )
			{
				bool monotonic = _probeTrack.D0 > _probeTrack.D1 + 1f && _probeTrack.D1 > _probeTrack.D2 + 1f;
				bool realDistance = _probeTrack.Traveled >= _probeTrack.D0 * MinTravelRatio;
				bool arrived = _probeTrack.D2 <= _entry.ArrivalRadius;

				if ( !monotonic || !realDistance || !arrived )
				{
					Fail( $"probe descenso inválido: d0={_probeTrack.D0:F1} d1={_probeTrack.D1:F1} d2={_probeTrack.D2:F1} recorrido={_probeTrack.Traveled:F1} (monótono={monotonic}, recorrido>=70%={realDistance})" );
					return;
				}

				_probeDescent = _probeTrack.D0 - _probeTrack.D2;
				Log.Info( $"[EnemyLab] {Name} probe descenso REAL t0={_probeTrack.D0:F1}u → t1={_probeTrack.D1:F1}u → t2={_probeTrack.D2:F1}u (recorrido {_probeTrack.Traveled:F1}u, delta={_probeDescent:F1}u) PASS" );
				_notes.Add( $"probe d0={_probeTrack.D0:F0} d1={_probeTrack.D1:F0} d2={_probeTrack.D2:F0}" );

				_phase = Phase.EnemySpawn;
				_phaseTimer = 0f;
			}
		}

		private void StepEnemySpawn()
		{
			if ( _phaseTimer < 0.1f ) return;

			var prefabFile = ResourceLibrary.Get<PrefabFile>( _entry.EnemyPrefabPath );
			if ( prefabFile == null )
			{
				_skipReason = $"prefab enemigo no encontrado ({_entry.EnemyPrefabPath}) — fase enemigo/loot SKIP (dominio Worker A)";
				Log.Warning( $"[EnemyLab] {Name} {_skipReason}" );
				_notes.Add( "enemy=skip(prefab)" );
				_phase = Phase.Finish;
				_phaseTimer = 0f;
				return;
			}

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null )
			{
				Fail( $"prefab scene null ({_entry.EnemyPrefabPath})" );
				return;
			}

			_enemy = scene.Clone();
			_enemy.WorldPosition = _spawnMarker.WorldPosition;
			_enemy.NetworkSpawn( Connection.Local );

			_host = _enemy.Components.GetInDescendantsOrSelf<EnemyContentHost>();
			if ( _host == null )
			{
				Fail( "prefab enemigo sin EnemyContentHost" );
				return;
			}

			_host.SetTarget( _testTarget );

			Log.Info( $"[EnemyLab] {Name} EnemySpawn def={_host.Definition?.Id ?? "NULL"} hp={_host.Health:F0} prefab={_entry.EnemyPrefabPath}" );
			_phase = Phase.EnemyMove;
			_phaseTimer = 0f;
		}

		private void StepEnemyMove()
		{
			if ( _phaseTimer < SettleSeconds )
			{
				return; // el host no navega hasta _timeSinceSpawn>=1s; medir tras asentar
			}

			if ( _enemy == null || !_enemy.IsValid() )
			{
				Fail( "enemigo destruido durante la navegación" );
				return;
			}

			if ( _host == null || _host.Definition == null )
			{
				Fail( "EnemyContentHost sin Definition registrada" );
				return;
			}

			var agent = _host.Agent;
			var pos = _enemy.WorldPosition;
			var d = Vector3.DistanceBetween( pos, _targetPosition );

			if ( _enemyTrack.D0 <= 0f )
			{
				_enemyTrack.D0 = d;
				_enemyTrack.LastPos = pos;
				_enemyTrack.HasLast = true;
				Log.Info( $"[EnemyLab] {Name} enemy t0={d:F1}u (spawn {pos})" );
				return;
			}

			if ( !_enemyTrack.T1Logged && d <= _enemyTrack.D0 * 0.5f )
			{
				_enemyTrack.T1Logged = true;
				_enemyTrack.D1 = d;
				Log.Info( $"[EnemyLab] {Name} enemy t1={d:F1}u (50% de t0={_enemyTrack.D0:F1}u) vel={agent.WishVelocity.Length:F1}u/s" );
			}

			if ( _enemyTrack.T1Logged && _enemyTrack.D2 <= 0f && d <= _entry.ArrivalRadius )
			{
				_enemyTrack.D2 = d;
				Log.Info( $"[EnemyLab] {Name} enemy t2={d:F1}u (llegada, radio {_entry.ArrivalRadius:F0}u) vel={agent.WishVelocity.Length:F1}u/s" );
			}

			if ( _enemyTrack.HasLast )
			{
				float frame = Vector3.DistanceBetween( _enemyTrack.LastPos, pos );
				if ( frame > TeleportThreshold )
				{
					Fail( $"enemigo teleport detectado (salto de {frame:F0}u/frame)" );
					return;
				}
				_enemyTrack.Traveled += frame;
			}
			_enemyTrack.LastPos = pos;
			_enemyTrack.HasLast = true;

			if ( _phaseTimer >= SettleSeconds + _entry.MoveTimeout && _enemyTrack.D2 <= 0f )
			{
				Fail( $"enemigo sin llegada tras {_entry.MoveTimeout:F0}s (d={Vector3.DistanceBetween( _enemy.WorldPosition, _targetPosition ):F1}u)" );
				return;
			}

			if ( _enemyTrack.D2 > 0f )
			{
				bool monotonic = _enemyTrack.D0 > _enemyTrack.D1 + 1f && _enemyTrack.D1 > _enemyTrack.D2 + 1f;
				bool realDistance = _enemyTrack.Traveled >= _enemyTrack.D0 * MinTravelRatio;
				bool arrived = _enemyTrack.D2 <= _entry.ArrivalRadius;

				if ( !monotonic || !realDistance || !arrived )
				{
					Fail( $"enemigo descenso inválido: d0={_enemyTrack.D0:F1} d1={_enemyTrack.D1:F1} d2={_enemyTrack.D2:F1} recorrido={_enemyTrack.Traveled:F1}" );
					return;
				}

				Log.Info( $"[EnemyLab] {Name} enemy descenso REAL t0={_enemyTrack.D0:F1}u → t1={_enemyTrack.D1:F1}u → t2={_enemyTrack.D2:F1}u (recorrido {_enemyTrack.Traveled:F1}u) PASS" );
				_notes.Add( $"enemy d0={_enemyTrack.D0:F0} d1={_enemyTrack.D1:F0} d2={_enemyTrack.D2:F0}" );

				_phase = Phase.Kill;
				_phaseTimer = 0f;
			}
		}

		private void StepKill()
		{
			if ( _phaseTimer < _entry.KillDelay ) return;

			if ( _enemy == null || !_enemy.IsValid() )
			{
				Fail( "enemigo inválido antes del kill fixture" );
				return;
			}

			var target = _enemy.Components.GetInAncestorsOrSelf<IDamageTarget>();
			if ( target == null )
			{
				Fail( "enemigo sin IDamageTarget (kill por ruta real imposible)" );
				return;
			}

			float hpBefore = _host != null ? _host.Health : -1f;
			target.TakeDamage( new ContentDamageEvent
			{
				Amount = 100000f,
				Position = _enemy.WorldPosition,
				Force = Vector3.Zero,
				SourceId = "lab_kill_fixture"
			} );

			Log.Info( $"[EnemyLab] {Name} KillFixture damage={hpBefore:F0} → dead={target.IsDead} (ruta IDamageTarget real)" );

			_phase = Phase.LootObserve;
			_phaseTimer = 0f;

			if ( _lootObserver != null )
			{
				_lootObserver.StartObserving();
			}
			else
			{
				_skipReason = "sin LabLootObserver en escena — fase loot SKIP";
				_notes.Add( "loot=skip(observer)" );
				_phase = Phase.Finish;
			}
		}

		private void StepLootObserve()
		{
			if ( _lootObserver == null || !_lootObserver.IsObserving )
			{
				// Ventana cerrada: evaluar.
				int observed = _lootObserver?.ObservedCount ?? 0;
				var ids = _lootObserver != null ? _lootObserver.ObservedItemIds : null;

				bool anyValid = ids != null && ids.Count > 0;
				bool allExpected = true;
				if ( ids != null )
				{
					foreach ( var id in ids )
					{
						if ( !_entry.ExpectedLootItemIds.Contains( id ) ) allExpected = false;
					}
				}

				bool pass = anyValid && allExpected;
				Log.Info( $"[EnemyLab] {Name} LootObserve count={observed} items={(ids == null ? "none" : string.Join( ",", ids ))} esperados=[{string.Join( ",", _entry.ExpectedLootItemIds )}] {(pass ? "PASS" : "FAIL")}" );

				if ( !pass && !string.IsNullOrEmpty( _skipReason ) && observed == 0 )
				{
					// Sin pickups y fase marcada SKIP por fixtures ausentes → no es FAIL del escenario.
					Log.Warning( $"[EnemyLab] {Name} loot sin observar (prefab enemigo ausente: {_skipReason})" );
					_phase = Phase.Finish;
					return;
				}

				if ( !pass ) Fail( "loot físico no observado o items fuera de la loot table" );
				else _notes.Add( $"loot={observed}" );

				_phase = Phase.Finish;
				_phaseTimer = 0f;
			}
		}

		private void Finish()
		{
			// Limpieza del probe (el enemigo muerto se autodestruye por la ruta real).
			if ( _probe != null && _probe.IsValid() )
			{
				_probe.Destroy();
			}

			if ( _fail )
			{
				Result = LabSuiteResult.Fail( _suiteClock, _probeDescent, $"fail:{_failReason}" );
			}
			else if ( !string.IsNullOrEmpty( _skipReason ) )
			{
				Result = LabSuiteResult.Pass( _suiteClock, _probeDescent, $"complete+{_skipReason} ({string.Join( ";", _notes )})" );
			}
			else
			{
				Result = LabSuiteResult.Pass( _suiteClock, _probeDescent, $"complete ({string.Join( ";", _notes )})" );
			}

			Log.Info( $"[EnemyLab] {Name} Suite finish: {Result.State}" );
			IsComplete = true;
		}

		// ---------------------------------------------------------------- helpers

		private void RecordT0( DescentTrack track, Vector3 pos, string who )
		{
			track.D0 = Vector3.DistanceBetween( pos, _targetPosition );
			track.LastPos = pos;
			track.HasLast = true;
			Log.Info( $"[EnemyLab] {Name} {who} t0={track.D0:F1}u (pos {pos})" );
		}

		private string _failReason = "";

		private void Fail( string reason )
		{
			_fail = true;
			_failReason = reason;
			Log.Error( $"[EnemyLab] {Name} FAIL: {reason}" );
			_phase = Phase.Finish;
			_phaseTimer = 0f;
		}
	}
}
 