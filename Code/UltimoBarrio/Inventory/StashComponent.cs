using Sandbox;
using System;

namespace UltimoBarrio
{
    public class StashComponent : Component, IInventoryOwner, IInteractable
    {
        [Property] public Guid ApartmentId { get; set; }
        
        [RequireComponent] public InventoryComponent Inventory { get; set; }

        public string GetInteractionPrompt() => "Abrir alijo";

        public bool CanInteract(Guid playerId)
        {
            var policy = Scene.GetAllComponents<IApartmentAccessPolicy>().FirstOrDefault();
            if (policy != null)
            {
                return policy.CanAccessStash(ApartmentId, playerId);
            }
            return true;
        }

        public void OnInteract(Guid playerId)
        {
            // Open stash UI logic here
        }

        public bool CanAdd(string itemId, int amount) => Inventory.CanAdd(itemId, amount);
        public bool TryAdd(string itemId, int amount) => Inventory.TryAdd(itemId, amount);
        public bool TryRemove(string itemId, int amount) => Inventory.TryRemove(itemId, amount);
        public int GetCount(string itemId) => Inventory.GetCount(itemId);
    }
}
