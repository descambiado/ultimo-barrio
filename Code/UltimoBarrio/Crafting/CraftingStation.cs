using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Inventory;
using System.Collections.Generic;

namespace UltimoBarrio.Crafting
{
    [Title("Crafting Station")]
    [Category("Último Barrio — Crafting")]
    [Icon("construction")]
    public sealed class CraftingStation : Component, IInteractable
    {
        [Property] public string OutputItemId { get; set; } = "ammo_9mm";
        [Property] public int OutputAmount { get; set; } = 12;
        
        [Property] public List<string> InputItems { get; set; } = new List<string> { "item-scrap" };
        [Property] public List<int> InputAmounts { get; set; } = new List<int> { 5 };

        public string GetInteractionPrompt(InteractionRequest request)
        {
            return $"Craft {OutputAmount}x {OutputItemId} (Needs {InputAmounts[0]}x {InputItems[0]})";
        }

        public bool CanInteract(InteractionRequest request)
        {
            return true;
        }

        public void OnInteract(InteractionRequest request)
        {
            if (!Networking.IsHost) return;

            var interactor = request.InteractorObject;
            if (interactor == null) return;

            var playerInv = interactor.Components.GetInDescendantsOrSelf<UltimoBarrioPlayerInventory>();
            if (playerInv == null) return;

            // Check if player has all ingredients
            for (int i = 0; i < InputItems.Count; i++)
            {
                if (i >= InputAmounts.Count) break;
                if (playerInv.GetCount(InputItems[i]) < InputAmounts[i])
                {
                    Log.Info($"[Crafting] Missing {InputAmounts[i]} of {InputItems[i]}");
                    return;
                }
            }

            // Consume ingredients
            for (int i = 0; i < InputItems.Count; i++)
            {
                if (i >= InputAmounts.Count) break;
                playerInv.TryRemove(InputItems[i], InputAmounts[i]);
            }

            // Give output
            playerInv.TryAdd(OutputItemId, OutputAmount);

            Log.Info($"[Crafting] Crafted {OutputAmount}x {OutputItemId}!");
        }
    }
}
