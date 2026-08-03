using Sandbox;
using UltimoBarrio.Core;
using System;
using System.Linq;
using UltimoBarrio.Apartments;

namespace UltimoBarrio
{
    public class StashComponent : Component, IWorldContainer, IWorldInteractable, IInteractable
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

        public InventoryComponent GetContainerInventory() => Inventory;

        public string GetInteractionPrompt(InteractionRequest request)
        {
            var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == ApartmentId);
            bool canAccess = apt == null || apt.OwnerId == request.InteractorId || apt.OwnerId == Game.SteamId.ToString();
            return canAccess ? "Pulsa E para abrir el alijo" : "No puedes acceder a este apartamento";
        }

        public bool CanInteract(InteractionRequest request)
        {
            var policy = Scene.GetAllComponents<IApartmentAccessPolicy>().FirstOrDefault();
            if (policy != null)
            {
                return policy.CanAccessStash(ApartmentId, request.InteractorId);
            }
            var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == ApartmentId);
            return apt == null || apt.OwnerId == request.InteractorId || apt.OwnerId == Game.SteamId.ToString();
        }

        public void OnInteract(InteractionRequest request)
        {
            // Interactor opens HUD for this container
        }
    }
}
