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
            Log.Info($"[Pickup] OnInteract called by {request.InteractorId}. IsProxy: {IsProxy}");
            if (IsProxy) return;
            var player = Scene.Directory.FindByGuid(request.InteractorId);
            if (player == null)
            {
                Log.Info($"[Pickup] Player {request.InteractorId} not found!");
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
                Log.Info($"[Pickup] No inventory found on player {request.InteractorId}!");
            }
        }
    }
}
