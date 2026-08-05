using Sandbox;
using System;

namespace UltimoBarrio.World
{
    /// <summary>
    /// Callejón o zona peligrosa: aumenta la presencia hostil nocturna en la
    /// SpawnZone más cercana (ver HostileSpawner) y avisa al HUD al entrar.
    /// </summary>
    [Title( "Danger Zone" )]
    [Category( "Último Barrio — World" )]
    [Icon( "warning" )]
    public sealed class DangerZone : Component
    {
        [Property] public float Radius { get; set; } = 600f;

        /// <summary>Hostiles extra que se suman a la SpawnZone solapada de noche.</summary>
        [Property] public int ExtraSpawnCount { get; set; } = 2;

        public bool Contains( Vector3 position )
            => Vector3.DistanceBetween( WorldPosition, position ) <= Radius;
    }
}
