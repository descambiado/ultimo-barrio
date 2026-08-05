using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Punto de construcción: un anchor del editor donde el propietario puede
    /// colocar una barricada (ítem "barricade"). Host-autoritativo vía RPC.
    /// </summary>
    [Title( "Barricade Anchor" )]
    [Category( "Último Barrio — Fortification" )]
    [Icon( "anchor" )]
    public sealed class BarricadeAnchor : Component, IWorldInteractable, IInteractable
    {
        [Property] public string ApartmentId { get; set; } = string.Empty;
        [Property] public string AnchorId { get; set; } = string.Empty;
        [Property] public float MaxInteractionDistance { get; set; } = 150f;

        public bool HasBarricade => _barricade.IsValid();

        private Barricade _barricade;

        public string GetInteractionPrompt( InteractionRequest request )
        {
            if ( HasBarricade )
                return "Barricada colocada (revisa su salud)";

            int count = request.InteractorObject?.Components.GetInDescendantsOrSelf<InventoryComponent>()?.GetCount( "barricade" ) ?? 0;
            return count > 0 ? $"Colocar barricada (tienes {count})" : "Barricada (no llevas ninguna)";
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
                RequestPlaceOnHost( request.InteractorObject?.Id ?? Guid.Empty );
                return;
            }

            ProcessPlace( request.InteractorObject );
        }

        [Rpc.Host]
        private void RequestPlaceOnHost( Guid interactorObjectId )
        {
            var interactor = Scene.Directory.FindByGuid( interactorObjectId );
            ProcessPlace( interactor );
        }

        private void ProcessPlace( GameObject interactor )
        {
            if ( interactor is null || !Networking.IsHost )
                return;

            // Distancia en servidor.
            if ( Vector3.DistanceBetween( interactor.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
                return;

            if ( HasBarricade )
                return;

            var inventory = interactor.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory is null || inventory.GetCount( "barricade" ) < 1 )
                return;

            // Consumir el ítem; si el spawn falla, rollback.
            if ( !inventory.TryRemove( "barricade", 1 ) )
                return;

            var barricadeGo = new GameObject( true, $"Barricade_{AnchorId}" );
            barricadeGo.Parent = GameObject.Parent ?? GameObject;
            barricadeGo.WorldPosition = GameObject.WorldPosition;
            barricadeGo.WorldRotation = GameObject.WorldRotation;

            var renderer = barricadeGo.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/dev/box.vmdl" );
            renderer.Tint = new Color( 0.45f, 0.3f, 0.15f );
            barricadeGo.LocalScale = new Vector3( 40f, 10f, 70f );

            var collider = barricadeGo.Components.Create<BoxCollider>();

            var barricade = barricadeGo.Components.Create<Barricade>();
            barricade.ApartmentId = ApartmentId;
            barricade.AnchorId = AnchorId;

            if ( Networking.IsActive )
                barricadeGo.NetworkSpawn();

            _barricade = barricade;

            // Persistir la colocación.
            Persistence.PersistenceBridge.RequestSave();

            UI.PlayerFeedback.Push( "Barricada colocada" );
            Log.Info( $"UB.Fortification BarricadaColocada apartment={ApartmentId} anchor={AnchorId}" );
        }

        internal void OnBarricadeDestroyed()
        {
            _barricade = null;
            UI.PlayerFeedback.Push( "¡Tu barricada fue destruida!" );
            Persistence.PersistenceBridge.RequestSave();
        }
    }
}
