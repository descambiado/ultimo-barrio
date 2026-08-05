using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Reparación de estructuras (puertas, ventanas, barricadas): el jugador
    /// interactúa con una estructura dañada y consume un repair_kit.
    /// Host-autoritativo vía RPC.
    /// </summary>
    public static class FortificationService
    {
        public const float RepairAmount = 100f;

        /// <summary>Repara una estructura si el jugador tiene un kit (host).</summary>
        public static bool TryRepair( GameObject player, DestructibleStructure structure )
        {
            if ( player is null || structure is null || !Networking.IsHost )
                return false;

            if ( structure.IsDestroyed )
            {
                UI.PlayerFeedback.Push( "La estructura está destruida: construye de nuevo" );
                return false;
            }

            if ( structure.Health >= structure.MaxHealth )
            {
                UI.PlayerFeedback.Push( "La estructura está al máximo" );
                return false;
            }

            var inventory = player.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory is null || inventory.GetCount( "repair_kit" ) < 1 )
            {
                UI.PlayerFeedback.Push( "Necesitas un kit de reparación" );
                return false;
            }

            if ( !inventory.TryRemove( "repair_kit", 1 ) )
                return false;

            structure.Repair( RepairAmount );

            UI.PlayerFeedback.Push( "Estructura reparada" );
            Persistence.PersistenceBridge.RequestSave( "repair" );
            return true;
        }
    }
}
