using Sandbox;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.WorldTime;

namespace UltimoBarrio.World
{
	/// <summary>
	/// Night Enemy Spawner — saca a los enemigos del lab al barrio real.
	///
	/// Usa EXACTAMENTE el pipeline validado (4 PASS / 0 FAIL): instancia el prefab
	/// del content pack (EnemyContentHost + EnemyPerception + EnemyAttack, data-driven
	/// por EnemyContentRegistry). El enemigo detecta al jugador por el tag
	/// "enemy_target" + IDamageTarget (PlayerContentDamageBridge) sin config manual.
	///
	/// Reglas: solo en fase Night; mantiene un máximo de enemigos vivos; al amanecer
	/// (Day) los retira. El jugador los mata → sueltan loot físico (LootPickupContent).
	/// </summary>
	[Title( "Night Enemy Spawner" )]
	[Category( "Ultimo Barrio — World" )]
	[Icon( "nightlight" )]
	public sealed class NightEnemySpawner : Component
	{
		[Property] public WorldClock Clock { get; set; }
		[Property] public List<GameObject> SpawnPoints { get; set; } = new();
		[Property] public string EnemyPrefabPath { get; set; } = "prefabs/content/enemies/enemy_saqueador.prefab";
		[Property] public int EnemiesPerWave { get; set; } = 2;
		[Property] public float RespawnDelay { get; set; } = 25f;

		private readonly List<GameObject> _alive = new();
		private bool _night;
		private TimeSince _sinceSpawn;

		protected override void OnStart()
		{
			if ( Clock == null )
			{
				Clock = Scene.GetAllComponents<WorldClock>().FirstOrDefault();
			}

			if ( Clock != null )
			{
				Clock.OnPhaseChanged += HandlePhase;
			}

			_night = Clock != null && Clock.CurrentPhase == TimePhase.Night;
		}

		protected override void OnDestroy()
		{
			if ( Clock != null )
			{
				Clock.OnPhaseChanged -= HandlePhase;
			}
		}

		protected override void OnUpdate()
		{
			if ( !Networking.IsHost || !_night ) return;

			_alive.RemoveAll( g => g == null || !g.IsValid() );
			if ( _alive.Count >= EnemiesPerWave ) return;
			if ( _sinceSpawn < RespawnDelay ) return;

			SpawnOne();
			_sinceSpawn = 0f;
		}

		private void HandlePhase( TimePhase phase )
		{
			if ( phase == TimePhase.Night )
			{
				_night = true;
				_sinceSpawn = 0f;
				Log.Info( "[NightSpawner] Noche: activando saqueadores" );
			}
			else if ( phase == TimePhase.Day )
			{
				_night = false;
				foreach ( var enemy in _alive )
				{
					if ( enemy != null && enemy.IsValid() ) enemy.Destroy();
				}
				_alive.Clear();
				Log.Info( "[NightSpawner] Día: saqueadores retirados" );
			}
		}

		private void SpawnOne()
		{
			var prefab = ResourceLibrary.Get<PrefabFile>( EnemyPrefabPath );
			if ( prefab == null )
			{
				Log.Error( $"[NightSpawner] prefab NO encontrado: {EnemyPrefabPath}" );
				return;
			}

			var points = SpawnPoints.Where( p => p != null && p.IsValid() ).ToList();
			if ( points.Count == 0 )
			{
				Log.Warning( "[NightSpawner] sin spawn points configurados" );
				return;
			}

			var point = points[Game.Random.Int( 0, points.Count - 1 )];
			var enemy = SceneUtility.GetPrefabScene( prefab ).Clone();
			enemy.WorldPosition = point.WorldPosition;
			enemy.WorldRotation = point.WorldRotation;
			enemy.NetworkSpawn( Connection.Local );
			_alive.Add( enemy );

			Log.Info( $"[NightSpawner] Saqueador apareció en '{point.Name}' ({enemy.WorldPosition})" );
		}
	}
}
