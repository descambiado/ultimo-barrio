using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio
{
    public class WorldItemPickup : Component, IInteractable
    {
        [Property] public string ItemId { get; set; } = "agua";
        [Property] public int Amount { get; set; } = 1;
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        public string GetInteractionPrompt(InteractionRequest request)
        {
            return $"Recoger {ItemId} (x{Amount})";
        }

        public bool CanInteract(InteractionRequest request)
        {
            if (request.InteractorObject == null) return false;
            return Vector3.DistanceBetween(request.InteractorObject.WorldPosition, GameObject.WorldPosition) <= MaxInteractionDistance;
        }

        public void OnInteract(InteractionRequest request)
        {
            if (IsProxy)
            {
                // We are on client, send RPC to host
                RequestPickupOnHost(request.Identity.CanonicalId, request.InteractorObject?.Id ?? Guid.Empty);
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
            
            // Validate distance on server
            if (Vector3.DistanceBetween(interactorGo.WorldPosition, GameObject.WorldPosition) > MaxInteractionDistance)
            {
                Log.Warning($"[WorldItemPickup] Player {interactorGo.Name} tried to pickup from too far.");
                return;
            }

            var inventory = interactorGo.Components.Get<InventoryComponent>();
            if (inventory != null)
            {
                bool added = inventory.TryAdd(ItemId, Amount);
                if (added)
                {
                    if (ItemId.StartsWith("weapon_"))
                    {
                        UltimoBarrio.Missions.MissionJournal.Local?.NotifyProgress(UltimoBarrio.Missions.ObjectiveType.ObtainWeapon, ItemId, Amount);
                    }
                    else if (ItemId == "chatarra")
                    {
                        UltimoBarrio.Missions.MissionJournal.Local?.NotifyProgress(UltimoBarrio.Missions.ObjectiveType.CollectItem, ItemId, Amount);
                    }
                    GameObject.Destroy();
                }
            }
        }
    }
}
