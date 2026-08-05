using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Zona de spawn hostil: área del mundo donde el HostileSpawner puede
    /// materializar NPCs por la noche. Sin prefab asignado no genera nada.
    /// </summary>
    [Title( "Spawn Zone" )]
    [Category( "Último Barrio — AI" )]
    [Icon( "radio_button_checked" )]
    public sealed class SpawnZone : Component
    {
        [Property] public float Radius { get; set; } = 400f;
        [Property] public int MaxHostiles { get; set; } = 2;
        [Property] public GameObject HostilePrefab { get; set; }

        public Vector3 RandomPoint()
        {
            var angle = (float)( Random.Shared.NextDouble() * Math.PI * 2.0 );
            var distance = (float)( Random.Shared.NextDouble() * Radius );
            return WorldPosition + new Vector3( MathF.Cos( angle ) * distance, MathF.Sin( angle ) * distance, 20f );
        }
    }

    /// <summary>
    /// Spawner de hostiles ligado al ciclo: de noche materializa saqueadores
    /// en las SpawnZones; al amanecer los retira. Respetan FeatureFlags.EnableAI.
    /// </summary>
    [Title( "Hostile Spawner" )]
    [Category( "Último Barrio — AI" )]
    [Icon( "nightlight" )]
    public sealed class HostileSpawner : Component
    {
        [Property] public WorldTime.WorldClock Clock { get; set; }

        private readonly List<GameObject> _spawned = new();
        private bool _nightActive;

        protected override void OnStart()
        {
            if ( Clock is not null && !IsProxy )
                Clock.OnPhaseChanged += HandlePhaseChanged;
        }

        protected override void OnDestroy()
        {
            if ( Clock is not null )
                Clock.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged( WorldTime.TimePhase phase )
        {
            if ( IsProxy || !Networking.IsHost )
                return;

            if ( !Core.FeatureFlags.EnableAI )
                return;

            if ( phase == WorldTime.TimePhase.Night )
                SpawnNightHostiles();
            else if ( _nightActive )
                DespawnAll();
        }

        private void SpawnNightHostiles()
        {
            if ( _nightActive )
                return;

            _nightActive = true;
            int spawned = 0;

            foreach ( var zone in Scene.GetAllComponents<SpawnZone>() )
            {
                if ( zone is null || !zone.HostilePrefab.IsValid() )
                    continue;

                // Las zonas marcadas como peligrosas (callejones) suman hostiles.
                int extra = 0;
                foreach ( var danger in Scene.GetAllComponents<World.DangerZone>() )
                {
                    if ( danger is not null && danger.Contains( zone.WorldPosition ) )
                        extra += danger.ExtraSpawnCount;
                }

                int total = zone.MaxHostiles + extra;
                for ( int i = 0; i < total; i++ )
                {
                    var hostile = zone.HostilePrefab.Clone( zone.RandomPoint() );
                    hostile.NetworkSpawn();
                    _spawned.Add( hostile );
                    spawned++;
                }
            }

            Log.Info( $"[AI] Noche: {spawned} hostiles spawnados en zonas." );
        }

        private void DespawnAll()
        {
            _nightActive = false;

            foreach ( var go in _spawned )
            {
                if ( go.IsValid() )
                    go.Destroy();
            }

            _spawned.Clear();
            Log.Info( "[AI] Amanecer: hostiles nocturnos retirados." );
        }
    }
}
