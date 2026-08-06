using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;

namespace UltimoBarrio.Apartments
{
    [Title("Apartment Door Policy")]
    [Category("Último Barrio — Apartments")]
    [Icon("door_front")]
    public sealed class ApartmentDoorPolicy : Component, IWorldInteractable
    {
        [Property] public string ApartmentId { get; set; } = "apartment-a01";
        [Property] public bool IsLocked { get; set; } = true;

        protected override void OnStart()
        {
            ApplyPhysicalState();
        }

        /// <summary>
        /// La puerta bloquea de verdad: mientras está cerrada, su Collider es
        /// sólido (IsTrigger=false) y detiene el movimiento; al abrirla se
        /// vuelve trigger, dejando pasar sin dejar de ser el objetivo del
        /// trace de interacción (Scene.Trace sigue impactando triggers).
        /// </summary>
        private void ApplyPhysicalState()
        {
            var col = Components.Get<Collider>();
            if (col != null) col.IsTrigger = !IsLocked;
        }

        public string GetInteractionPrompt(InteractionRequest request)
        {
            var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == ApartmentId);
            if (apt == null || apt.ClaimState == ApartmentClaimState.Unclaimed)
            {
                return "Puerta Bloqueada (Vivida Sin Reclamar)";
            }

            bool isOwner = apt.OwnerId == request.Identity.CanonicalId;
            return isOwner ? "Pulsa E para abrir/cerrar la puerta" : "Puerta Bloqueada (Acceso Denegado)";
        }

        public bool CanInteract(InteractionRequest request)
        {
            var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == ApartmentId);
            if (apt == null || apt.ClaimState == ApartmentClaimState.Unclaimed) return false;

            return apt.OwnerId == request.Identity.CanonicalId;
        }

        public void OnInteract(InteractionRequest request)
        {
            if (!CanInteract(request)) return;

            IsLocked = !IsLocked;
            ApplyPhysicalState();
            Log.Info($"[DoorPolicy] Door {ApartmentId} is now {(IsLocked ? "Locked" : "Unlocked")}");
        }
    }
}
