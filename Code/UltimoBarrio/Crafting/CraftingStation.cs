using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Inventory;
using System.Collections.Generic;

namespace UltimoBarrio.Crafting
{
    public struct CraftingRecipe
    {
        public string Name { get; set; }
        public string OutputItemId { get; set; }
        public int OutputAmount { get; set; }
        public List<string> InputItems { get; set; }
        public List<int> InputAmounts { get; set; }
    }

    [Title("Crafting Station")]
    [Category("Último Barrio — Crafting")]
    [Icon("construction")]
    public sealed class CraftingStation : Component, IInteractable
    {
        [Property] public List<CraftingRecipe> Recipes { get; set; } = new List<CraftingRecipe>();
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        [Sync] public int CurrentRecipeIndex { get; set; } = 0;

        protected override void OnAwake()
        {
            if (Recipes.Count == 0 && !IsProxy)
            {
                Recipes.Add(new CraftingRecipe { Name = "Ammo 9mm", OutputItemId = "ammo_9mm", OutputAmount = 12, InputItems = new List<string> { "chatarra" }, InputAmounts = new List<int> { 5 } });
                Recipes.Add(new CraftingRecipe { Name = "Vendaje", OutputItemId = "medicina", OutputAmount = 1, InputItems = new List<string> { "chatarra" }, InputAmounts = new List<int> { 3 } });
                Recipes.Add(new CraftingRecipe { Name = "Repair Kit", OutputItemId = "repair_kit", OutputAmount = 1, InputItems = new List<string> { "chatarra", "scrap_parts" }, InputAmounts = new List<int> { 5, 2 } });
                Recipes.Add(new CraftingRecipe { Name = "Improvised Crowbar", OutputItemId = "weapon_crowbar", OutputAmount = 1, InputItems = new List<string> { "chatarra", "scrap_metal" }, InputAmounts = new List<int> { 10, 5 } });
                Recipes.Add(new CraftingRecipe { Name = "Barricada", OutputItemId = "barricade", OutputAmount = 1, InputItems = new List<string> { "chatarra" }, InputAmounts = new List<int> { 20 } });
            }
        }

        public string GetInteractionPrompt(InteractionRequest request)
        {
            if (Recipes == null || Recipes.Count == 0) return "No recipes available";
            var recipe = Recipes[CurrentRecipeIndex];
            string inputs = "";
            for(int i=0; i<recipe.InputItems.Count; i++) {
                inputs += $"{recipe.InputAmounts[i]}x {recipe.InputItems[i]} ";
            }
            return $"[Interact] Craft {recipe.OutputAmount}x {recipe.Name} (Needs {inputs}) | [Run+Interact] Cycle Recipe";
        }

        public bool CanInteract(InteractionRequest request)
        {
            if (request.InteractorObject == null) return false;
            return Vector3.DistanceBetween(request.InteractorObject.WorldPosition, GameObject.WorldPosition) <= MaxInteractionDistance;
        }

        public void OnInteract(InteractionRequest request)
        {
            bool cycleRecipe = Input.Down("Run"); // Shift by default in s&box

            if (IsProxy)
            {
                RequestCraftOnHost(request.Identity.CanonicalId, request.InteractorObject?.Id ?? System.Guid.Empty, cycleRecipe);
            }
            else
            {
                ProcessCraft(request.InteractorObject, cycleRecipe);
            }
        }

        [Rpc.Host]
        private void RequestCraftOnHost(string interactorId, System.Guid interactorObjectId, bool cycleRecipe)
        {
            var interactorGo = Scene.Directory.FindByGuid(interactorObjectId);
            ProcessCraft(interactorGo, cycleRecipe);
        }

        private void ProcessCraft(GameObject interactorGo, bool cycleRecipe)
        {
            if (interactorGo == null) return;
            
            if (Vector3.DistanceBetween(interactorGo.WorldPosition, GameObject.WorldPosition) > MaxInteractionDistance)
            {
                Log.Warning("[CraftingStation] Player tried to interact from too far.");
                return;
            }

            if (Recipes == null || Recipes.Count == 0) return;

            if (cycleRecipe)
            {
                CurrentRecipeIndex = (CurrentRecipeIndex + 1) % Recipes.Count;
                Log.Info($"[CraftingStation] Switched to recipe: {Recipes[CurrentRecipeIndex].Name}");
                return;
            }

            var recipe = Recipes[CurrentRecipeIndex];
            var playerInv = interactorGo.Components.GetInDescendantsOrSelf<UltimoBarrioPlayerInventory>();
            if (playerInv == null) return;

            for (int i = 0; i < recipe.InputItems.Count; i++)
            {
                if (playerInv.GetCount(recipe.InputItems[i]) < recipe.InputAmounts[i])
                {
                    Log.Info($"[CraftingStation] Missing {recipe.InputAmounts[i]} of {recipe.InputItems[i]}");
                    return;
                }
            }

            bool rollback = false;
            int consumedCount = 0;
            for (int i = 0; i < recipe.InputItems.Count; i++)
            {
                if (!playerInv.TryRemove(recipe.InputItems[i], recipe.InputAmounts[i]))
                {
                    rollback = true;
                    break;
                }
                consumedCount++;
            }

            if (rollback)
            {
                for (int i = 0; i < consumedCount; i++) playerInv.TryAdd(recipe.InputItems[i], recipe.InputAmounts[i]);
                Log.Warning("[CraftingStation] Transaction failed, rolled back ingredients.");
                return;
            }

            if (!playerInv.TryAdd(recipe.OutputItemId, recipe.OutputAmount))
            {
                for (int i = 0; i < recipe.InputItems.Count; i++) playerInv.TryAdd(recipe.InputItems[i], recipe.InputAmounts[i]);
                Log.Warning("[CraftingStation] Inventory full, rolled back ingredients.");
                return;
            }

            Log.Info($"[CraftingStation] Crafted {recipe.OutputAmount}x {recipe.OutputItemId}!");
            BroadcastCraftEffects();
        }

        [Rpc.Broadcast]
        private void BroadcastCraftEffects()
        {
            // Placeholder feedback
            Log.Info("Crafting effect played");
        }
    }
}
