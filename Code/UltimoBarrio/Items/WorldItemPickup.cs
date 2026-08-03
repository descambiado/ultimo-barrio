using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio
{
    public class WorldItemPickup : Component, IInteractable
    {
        [Property] public string ItemId { get; set; } = "agua";
        [Property] public int Amount { get; set; } = 1;

        public string GetInteractionPrompt(InteractionRequest request)
        {
            return $"Recoger {ItemId} (x{Amount})";
        }

        public bool CanInteract(InteractionRequest request)
        {
            return true;
        }

        public void OnInteract(InteractionRequest request)
        {
            if (IsProxy)
            {
                // We are on client, send RPC to host
                RequestPickupOnHost(request.InteractorId, request.InteractorObject?.Id ?? Guid.Empty);
            }
            else
            {
                // We are host
                ProcessPickup(request.InteractorObject);
            }
        }

        [Rpc.Host]
        private void RequestPickupOnHost(string interactorId, Guid interactorObjectId)
        {
            var interactorGo = Scene.Directory.FindByGuid(interactorObjectId);
            ProcessPickup(interactorGo);
        }

        private void ProcessPickup(GameObject interactorGo)
        {
            if (interactorGo == null) return;
            var inventory = interactorGo.Components.Get<InventoryComponent>();
            if (inventory != null)
            {
                bool added = inventory.TryAdd(ItemId, Amount);
                if (added)
                {
                    GameObject.Destroy();
                }
            }
        }
    }
}
