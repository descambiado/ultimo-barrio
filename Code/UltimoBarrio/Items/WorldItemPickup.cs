using Sandbox;
using System;

namespace UltimoBarrio
{
    public class WorldItemPickup : Component, IInteractable
    {
        [Property] public string ItemId { get; set; } = "agua";
        [Property] public int Amount { get; set; } = 1;

        public string GetInteractionPrompt()
        {
            return $"Recoger {ItemId} (x{Amount})";
        }

        public bool CanInteract(Guid playerId)
        {
            return true;
        }

        public void OnInteract(Guid playerId)
        {
            if (IsProxy) return;
            var player = Scene.Directory.FindComponentByGuid(playerId);
            if (player != null)
            {
                var inventory = player.Components.Get<InventoryComponent>();
                if (inventory != null && inventory.TryAdd(ItemId, Amount))
                {
                    GameObject.Destroy();
                }
            }
        }
    }
}
