using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Ruta de patrulla de un NPC: lista de waypoints (GameObjects hijos o
    /// asignados en el editor). Sin ruta, el NPC se queda en Idle/Investigación.
    /// </summary>
    [Title( "Patrol Route" )]
    [Category( "Último Barrio — AI" )]
    [Icon( "route" )]
    public sealed class PatrolRoute : Component
    {
        [Property] public List<GameObject> Waypoints { get; set; } = new();

        /// <summary>Si no hay waypoints asignados, usa los hijos nombrados "wp".</summary>
        public IReadOnlyList<GameObject> ResolveWaypoints()
        {
            if ( Waypoints is { Count: > 0 } )
                return Waypoints;

            var resolved = new List<GameObject>();
            foreach ( var child in GameObject.Children )
            {
                if ( child.Name.Contains( "wp", StringComparison.OrdinalIgnoreCase ) )
                    resolved.Add( child );
            }

            return resolved;
        }
    }
}
