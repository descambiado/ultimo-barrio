using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio
{
    public class WorldItemPickup : Component, IInteractable
    {
        [Property] public string ItemId { get; set; } = "chatarra";
        [Property] public int Amount { get; set; } = 1;
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        /// <summary>Cargador del arma soltada (persistencia del drop → repickup).</summary>
        [Property] public int AmmoInMag { get; set; }

        public string GetInteractionPrompt( InteractionRequest request )
        {
            var definition = ItemRegistry.GetDefinition( ItemId );
            var name = definition?.DisplayName ?? ItemId;
            return $"Recoger {name} (x{Amount})";
        }

        public bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null ) return false;
            return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
        }

        public void OnInteract( InteractionRequest request )
        {
            if ( IsProxy )
            {
                // Estamos en cliente: pedimos al host.
                RequestPickupOnHost( request.Identity.CanonicalId, request.InteractorObject?.Id ?? Guid.Empty );
            }
            else
            {
                // Estamos en host.
                ProcessPickup( request.InteractorObject );
            }
        }

        [Rpc.Host]
        private void RequestPickupOnHost( string interactorId, Guid interactorObjectId )
        {
            var interactorGo = Scene.Directory.FindByGuid( interactorObjectId );
            ProcessPickup( interactorGo );
        }

        private void ProcessPickup( GameObject interactorGo )
        {
            if ( interactorGo == null ) return;

            // Validación de distancia en servidor (anti-cheat).
            if ( Vector3.DistanceBetween( interactorGo.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
            {
                Log.Warning( $"[WorldItemPickup] Player {interactorGo.Name} intentó recoger desde demasiado lejos." );
                return;
            }

            var inventory = interactorGo.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory == null )
            {
                Log.Warning( $"[WorldItemPickup] Interactor sin InventoryComponent: {interactorGo.Name}" );
                return;
            }

            // Nodo de recurso: la recolección lo agota y respawnea; el pickup
            // NO se destruye (vive en el mismo GameObject que el nodo).
            var node = Components.Get<ResourceNode>( FindMode.InSelf );
            if ( node is not null )
            {
                if ( !node.IsAvailable )
                    return;

                if ( !node.TryHarvest( interactorGo, out _ ) )
                    return;

                DeliverTo( inventory, node.ResolvedHarvestItemId, Amount, 0 );
                return;
            }

            var definition = ItemRegistry.GetDefinition( ItemId );
            int magToRestore = definition is not null && definition.IsWeapon ? AmmoInMag : 0;

            var slot = inventory.AddItem( ItemId, Amount, magToRestore );
            if ( slot is not null )
            {
                // Si el arma no traía cargador, dejamos el del ítem tal cual.
                if ( definition is not null && definition.IsWeapon && magToRestore <= 0 )
                    slot.AmmoInMag = definition.MagazineSize;

                var name = definition?.DisplayName ?? ItemId;
                NotifyPickup( $"Recogido: {name} x{Amount}" );
                GameObject.Destroy();
            }
        }

        private void DeliverTo( InventoryComponent inventory, string itemId, int amount, int ammoInMag )
        {
            var definition = ItemRegistry.GetDefinition( itemId );
            var name = definition?.DisplayName ?? itemId;

            if ( inventory.AddItem( itemId, amount, ammoInMag ) is not null )
            {
                NotifyPickup( $"Recogido: {name} x{amount}" );
            }
        }

        [Rpc.Broadcast]
        private void NotifyPickup( string message )
        {
            UI.PlayerFeedback.Push( message );
        }
    }
}
