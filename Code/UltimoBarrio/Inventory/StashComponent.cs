using Sandbox;
using UltimoBarrio.Core;
using System;

namespace UltimoBarrio
{
    public class StashComponent : Component, IInventory, IInteractable
    {
        [Property] public string InventoryId { get; set; } = string.Empty;
        [Property] public int MaxSlots { get; set; } = 24;
        [Property] public string ApartmentId { get; set; } = "apartment-a01";
        
        [RequireComponent] public InventoryComponent Inventory { get; set; }

        protected override void OnAwake()
        {
            if (Inventory != null && string.IsNullOrEmpty(Inventory.InventoryId))
            {
                Inventory.InventoryId = $"{ApartmentId}:stash";
            }
        }

        public string GetInteractionPrompt(InteractionRequest request) => "Abrir alijo";

        public bool CanInteract(InteractionRequest request)
        {
            var policy = Scene.GetAllComponents<IApartmentAccessPolicy>().FirstOrDefault();
            if (policy != null)
            {
                return policy.CanAccessStash(ApartmentId, request.InteractorId);
            }
            return true;
        }

        public void OnInteract(InteractionRequest request)
        {
            // Open stash UI logic here
        }

        public bool CanAdd(string itemId, int amount) => Inventory.CanAdd(itemId, amount);
        public bool TryAdd(string itemId, int amount) => Inventory.TryAdd(itemId, amount);
        public bool TryRemove(string itemId, int amount) => Inventory.TryRemove(itemId, amount);
        public int GetCount(string itemId) => Inventory.GetCount(itemId);
    }
}
