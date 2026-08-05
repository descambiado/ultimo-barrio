using Sandbox;
using System;

namespace UltimoBarrio.World
{
    /// <summary>
    /// Defines the safe boundary of the playable Alpha zone.
    /// Players outside bounds are teleported to a safe spawn point.
    /// </summary>
    [Title( "World Boundary" )]
    [Category( "Último Barrio — World" )]
    [Icon( "fence" )]
    public sealed class WorldBoundary : Component
    {
        [Property] public BBox Bounds { get; set; } = new BBox( new Vector3( -2000, -2000, -200 ), new Vector3( 2000, 2000, 500 ) );
        [Property] public Vector3 SafeRespawnPosition { get; set; } = new Vector3( 0, 0, 100 );
        [Property] public float CheckInterval { get; set; } = 1.0f;

        private TimeSince _lastCheck;

        protected override void OnUpdate()
        {
            if ( !Networking.IsHost ) return;
            if ( _lastCheck < CheckInterval ) return;
            _lastCheck = 0;

            foreach ( var player in Scene.GetAllComponents<Sandbox.PlayerController>() )
            {
                if ( !Bounds.Contains( player.WorldPosition ) )
                {
                    TeleportToSafe( player.GameObject, player.WorldPosition );
                }
            }
        }

        private void TeleportToSafe( GameObject playerGo, Vector3 fromPosition )
        {
            Log.Info( $"[WorldBoundary] Player {playerGo.Name} fell out of bounds at {fromPosition}. Respawning at safe position {SafeRespawnPosition}." );

            playerGo.WorldPosition = SafeRespawnPosition;

            // Save last safe position in a component or service if available
            var recovery = playerGo.Components.Get<FallRecoveryTracker>( FindMode.InSelf );
            if ( recovery != null )
            {
                recovery.RegisterRecovery( fromPosition, SafeRespawnPosition );
            }
        }

        protected override void DrawGizmos()
        {
            Gizmo.Draw.LineBBox( Bounds );
        }
    }

    /// <summary>
    /// Tracks fall recoveries for a player - does not affect game state, only logging.
    /// </summary>
    [Title( "Fall Recovery Tracker" )]
    [Category( "Último Barrio — World" )]
    [Icon( "warning" )]
    public sealed class FallRecoveryTracker : Component
    {
        public int RecoveryCount { get; private set; }

        public void RegisterRecovery( Vector3 from, Vector3 to )
        {
            RecoveryCount++;
            Log.Info( $"[FallRecovery] Recovery #{RecoveryCount}: {from} -> {to}" );
        }
    }
}
