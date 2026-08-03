using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Combat
{
    public class WeaponPickup : Component, IInteractable
    {
        [Property] public GameObject WeaponPrefab { get; set; }
        [Property] public string WeaponName { get; set; } = "Pistol";

        public string GetInteractionPrompt(InteractionRequest request)
        {
            return "Press E to pickup " + WeaponName;
        }

        public bool CanInteract(InteractionRequest request)
        {
            return WeaponPrefab != null;
        }

        public void OnInteract(InteractionRequest request)
        {
            if (!Networking.IsHost) return;

            var interactor = request.InteractorObject;
            if (interactor == null) return;

            var equipper = interactor.Components.GetInAncestorsOrSelf<WeaponEquipper>();
            if (equipper != null)
            {
                equipper.EquipWeapon(WeaponPrefab);
                GameObject.Destroy();
            }
        }
    }
}
