using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Combat;
using UltimoBarrio.Core;

namespace UltimoBarrio.World
{
    /// <summary>
    /// Punto médico del barrio: cura al jugador (host-autoritativo) con un
    /// cooldown por jugador. Sin coste en alpha.
    /// </summary>
    [Title( "Medical Point" )]
    [Category( "Último Barrio — World" )]
    [Icon( "medical_services" )]
    public sealed class MedicalPoint : Component, IWorldInteractable, IInteractable
    {
        [Property] public float HealAmount { get; set; } = 50f;
        [Property] public float CooldownSeconds { get; set; } = 15f;
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        private readonly Dictionary<string, TimeSince> _cooldowns = new();

        public string GetInteractionPrompt( InteractionRequest request )
        {
            var key = request.Identity.CanonicalId;
            if ( _cooldowns.TryGetValue( key, out var since ) && since < CooldownSeconds )
                return "Punto médico (recargando...)";

            return "Punto médico — pulsa E para curarte";
        }

        public bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null )
                return false;

            return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
        }

        public void OnInteract( InteractionRequest request )
        {
            if ( IsProxy )
            {
                RequestHealOnHost( request.InteractorObject?.Id ?? Guid.Empty );
                return;
            }

            ProcessHeal( request.InteractorObject );
        }

        [Rpc.Host]
        private void RequestHealOnHost( Guid interactorObjectId )
        {
            var interactor = Scene.Directory.FindByGuid( interactorObjectId );
            ProcessHeal( interactor );
        }

        private void ProcessHeal( GameObject interactor )
        {
            if ( interactor is null || !Networking.IsHost )
                return;

            if ( Vector3.DistanceBetween( interactor.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
                return;

            var health = interactor.Components.GetInDescendantsOrSelf<HealthComponent>();
            if ( health is null || health.IsDead || health.Health >= health.MaxHealth )
                return;

            var key = WorldSnapshotService.ResolvePlayerKey( interactor );
            if ( _cooldowns.TryGetValue( key, out var since ) && since < CooldownSeconds )
                return;

            _cooldowns[key] = 0f;
            health.Heal( HealAmount );

            UI.PlayerFeedback.Push( "Te has curado en el punto médico" );
            Log.Info( $"UB.World PuntoMedico player={key}" );
        }
    }
}
