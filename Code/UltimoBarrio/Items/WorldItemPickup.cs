using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio.World;

namespace UltimoBarrio
{
    public class WorldItemPickup : Component, IInteractable
    {
        [Property] public string ItemId { get; set; } = "chatarra";
        [Property] public int Amount { get; set; } = 1;
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        /// <summary>Cargador del arma soltada (persistencia del drop → repickup).</summary>
        [Property] public int AmmoInMag { get; set; }

        /// <summary>Id de intento asignado por PlayerInteractor para instrumentación temporal (UB.Pickup). No persiste, no afecta la lógica.</summary>
        public int DebugAttemptId { get; set; }

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
            var attemptId = DebugAttemptId;
            if ( attemptId != 0 )
                Log.Info( $"UB.Pickup Attempt={attemptId} HostReceived=True item={ItemId} interactor={interactorGo?.Name}" );

            if ( interactorGo == null )
            {
                if ( attemptId != 0 ) Log.Info( $"UB.Pickup Attempt={attemptId} AddItem=Rejected reason=NoInteractorGameObject" );
                return;
            }

            // Validación de distancia en servidor (anti-cheat).
            if ( Vector3.DistanceBetween( interactorGo.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
            {
                Log.Warning( $"[WorldItemPickup] Player {interactorGo.Name} intentó recoger desde demasiado lejos." );
                if ( attemptId != 0 ) Log.Info( $"UB.Pickup Attempt={attemptId} AddItem=Rejected reason=OutOfRange" );
                return;
            }

            var inventory = interactorGo.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory == null )
            {
                Log.Warning( $"[WorldItemPickup] Interactor sin InventoryComponent: {interactorGo.Name}" );
                if ( attemptId != 0 ) Log.Info( $"UB.Pickup Attempt={attemptId} AddItem=Rejected reason=NoInventoryComponent" );
                return;
            }

            if ( attemptId != 0 )
                Log.Info( $"UB.Pickup Attempt={attemptId} InventoryFound={inventory.InventoryId} Before={ItemId}:{inventory.GetCount( ItemId )}" );

            // Nodo de recurso: la recolección lo agota y respawnea; el pickup
            // NO se destruye (vive en el mismo GameObject que el nodo).
            var node = Components.Get<ResourceNode>( FindMode.InSelf );
            if ( node is not null )
            {
                if ( !node.IsAvailable )
                {
                    if ( attemptId != 0 ) Log.Info( $"UB.Pickup Attempt={attemptId} AddItem=Rejected reason=NodeNotAvailable" );
                    return;
                }

                if ( !node.TryHarvest( interactorGo, out _ ) )
                {
                    if ( attemptId != 0 ) Log.Info( $"UB.Pickup Attempt={attemptId} AddItem=Rejected reason=TryHarvestFailed" );
                    return;
                }

                DeliverTo( inventory, node.ResolvedHarvestItemId, Amount, 0 );
                if ( attemptId != 0 )
                    Log.Info( $"UB.Pickup Attempt={attemptId} After={node.ResolvedHarvestItemId}:{inventory.GetCount( node.ResolvedHarvestItemId )} Consumed=False (nodo respawnea)" );
                return;
            }

            var definition = ItemRegistry.GetDefinition( ItemId );
            int magToRestore = definition is not null && definition.IsWeapon ? AmmoInMag : 0;

            var slot = inventory.AddItem( ItemId, Amount, magToRestore );
            if ( attemptId != 0 )
                Log.Info( $"UB.Pickup Attempt={attemptId} AddItem={(slot is not null ? "Succeeded" : "Failed")}" );

            if ( slot is not null )
            {
                // Si el arma no traía cargador, dejamos el del ítem tal cual.
                if ( definition is not null && definition.IsWeapon && magToRestore <= 0 )
                    slot.AmmoInMag = definition.MagazineSize;

                var name = definition?.DisplayName ?? ItemId;
                NotifyPickup( $"Recogido: {name} x{Amount}" );

                if ( attemptId != 0 )
                    Log.Info( $"UB.Pickup Attempt={attemptId} After={ItemId}:{inventory.GetCount( ItemId )} Consumed=True" );

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

                var journal = Scene.GetAllComponents<Missions.MissionJournal>().FirstOrDefault();
                journal?.NotifyProgress( Missions.ObjectiveType.CollectItem, itemId, amount );
            }
        }

        [Rpc.Broadcast]
        private void NotifyPickup( string message )
        {
            UI.PlayerFeedback.Push( message );
        }
    }
}
