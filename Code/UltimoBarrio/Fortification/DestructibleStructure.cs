using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Estructura con salud (puerta, ventana, barricada): recibe daño
    /// host-autoritativo, se repara con kits y se destruye al llegar a 0.
    /// Los saqueadores atacan estructuras (IDamageable); los jugadores las
    /// reparan con repair_kit.
    /// </summary>
    [Title( "Destructible Structure" )]
    [Category( "Último Barrio — Fortification" )]
    [Icon( "shield" )]
    public class DestructibleStructure : Component, IDamageable, IWorldInteractable
    {
        [Property] public string StructureId { get; set; } = "structure";

        [Property] public float MaxHealth { get; set; } = 200f;

        [Sync] public float Health { get; protected set; }

        public bool IsDestroyed => Health <= 0;

        public event Action<float> OnDamaged;
        public event Action OnDestroyed;

        public string GetInteractionPrompt( InteractionRequest request )
        {
            if ( IsDestroyed )
                return "Estructura destruida";

            return Health < MaxHealth
                ? $"Reparar ({MathF.Ceiling( Health )}/{MaxHealth}) — kit de reparación"
                : "Estructura en buen estado";
        }

        public bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null )
                return false;

            return !IsDestroyed && Health < MaxHealth
                && Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= 200f;
        }

        public void OnInteract( InteractionRequest request )
        {
            if ( IsProxy )
            {
                RequestRepairOnHost( request.InteractorObject?.Id ?? Guid.Empty );
                return;
            }

            FortificationService.TryRepair( request.InteractorObject, this );
        }

        [Rpc.Host]
        private void RequestRepairOnHost( Guid interactorObjectId )
        {
            var interactor = Scene.Directory.FindByGuid( interactorObjectId );
            FortificationService.TryRepair( interactor, this );
        }

        protected override void OnStart()
        {
            if ( Networking.IsHost )
                Health = MaxHealth;
        }

        public void TakeDamage( DamageEvent damageEvent )
        {
            if ( !Networking.IsHost || IsDestroyed )
                return;

            Health = FortificationMath.ApplyDamage( Health, damageEvent.Amount, MaxHealth );
            RpcDamageFeedback( damageEvent.Position, Health <= 0f );

            if ( Health <= 0f )
            {
                OnDestroyed?.Invoke();
                OnStructureDestroyed();
            }
            else
            {
                OnDamaged?.Invoke( damageEvent.Amount );
            }
        }

        /// <summary>Reparación host-autoritativa con kit.</summary>
        public void Repair( float amount )
        {
            if ( !Networking.IsHost || IsDestroyed )
                return;

            Health = FortificationMath.ApplyRepair( Health, amount, MaxHealth );
        }

        protected virtual void OnStructureDestroyed()
        {
        }

        [Rpc.Broadcast]
        private void RpcDamageFeedback( Vector3 position, bool destroyed )
        {
            // Hook visual (partículas/audio) por defecto vacío.
        }
    }
}
