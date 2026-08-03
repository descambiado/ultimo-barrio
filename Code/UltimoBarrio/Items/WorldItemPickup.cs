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
            Log.Info($"[Pickup] OnInteract called by {playerId}. IsProxy: {IsProxy}");
            if (IsProxy) return;
            var player = Scene.Directory.FindByGuid(playerId);
            if (player == null)
            {
                Log.Info($"[Pickup] Player {playerId} not found!");
                return;
            }
            var inventory = player.Components.Get<InventoryComponent>();
            if (inventory != null)
            {
                bool added = inventory.TryAdd(ItemId, Amount);
                Log.Info($"[Pickup] Added to inventory: {added}");
                if (added)
                {
                    GameObject.Destroy();
                }
            }
            else
            {
                Log.Info($"[Pickup] No inventory found on player {playerId}!");
            }
        }
    }
}
