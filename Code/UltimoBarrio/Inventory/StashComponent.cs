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
            if (apt == null || apt.ClaimState == ApartmentClaimState.Unclaimed || string.IsNullOrEmpty(apt.OwnerId))
            {
                return "Alijo Bloqueado (Vivienda Sin Reclamar)";
            }

            bool isOwner = apt.OwnerId == request.Identity.CanonicalId;
            return isOwner ? "Pulsa E para abrir el alijo" : "No puedes acceder a este alijo";
        }

        public bool CanInteract(InteractionRequest request)
        {
            var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == ApartmentId);
            if (apt == null || apt.ClaimState == ApartmentClaimState.Unclaimed || string.IsNullOrEmpty(apt.OwnerId))
            {
                return false;
            }

            var policy = Scene.GetAllComponents<IApartmentAccessPolicy>().FirstOrDefault();
            if (policy != null)
            {
                return policy.CanAccessStash(ApartmentId, request.Identity.CanonicalId);
            }

            bool isOwner = apt.OwnerId == request.Identity.CanonicalId;
            float dist = request.InteractorObject != null ? Vector3.DistanceBetween(WorldPosition, request.InteractorObject.WorldPosition) : 0f;

            Log.Info($"[Stash] PlayerId={request.Identity.CanonicalId}, ApartmentId={ApartmentId}, OwnerId={apt.OwnerId}, IsClaimed={apt.ClaimState}, IsOwner={isOwner}, Distance={dist:F1}, Decision={isOwner}");
            return isOwner;
        }

        public void OnInteract(InteractionRequest request)
        {
            // Interactor opens HUD for this container
        }
    }
}
