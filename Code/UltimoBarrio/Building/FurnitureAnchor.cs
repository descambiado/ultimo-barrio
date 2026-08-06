using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio.Inventory;

namespace UltimoBarrio.Building
{
    /// <summary>
    /// Punto de construcción para muebles funcionales (de momento: un cofre de
    /// almacenaje adicional). Mismo patrón que BarricadeAnchor -- colocación
    /// host-autoritativa vía RPC, distancia verificada en servidor, ítem
    /// consumido atómicamente.
    /// </summary>
    [Title( "Furniture Anchor" )]
    [Category( "Último Barrio — Building" )]
    [Icon( "chair" )]
    public sealed class FurnitureAnchor : Component, IWorldInteractable, IInteractable
    {
        [Property] public string ApartmentId { get; set; } = string.Empty;
        [Property] public string AnchorId { get; set; } = string.Empty;
        [Property] public float MaxInteractionDistance { get; set; } = 150f;

        public const string KitItemId = "storage_crate_kit";

        public bool HasFurniture => _furnitureGo.IsValid();

        private GameObject _furnitureGo;

        public string GetInteractionPrompt( InteractionRequest request )
        {
            if ( HasFurniture )
                return "Cofre de almacenaje colocado";

            var inv = request.InteractorObject?.Components.GetInDescendantsOrSelf<InventoryComponent>();
            int count = inv?.GetCount( KitItemId ) ?? 0;
            return count > 0 ? $"Colocar cofre de almacenaje (tienes {count})" : "Cofre de almacenaje (no llevas ninguno)";
        }

        public bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null || HasFurniture )
                return false;

            if ( !StructureAuthorization.CanBuild( Scene, request.InteractorObject, ApartmentId ) )
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
            if ( interactor is null || !Networking.IsHost || HasFurniture )
                return;

            if ( Vector3.DistanceBetween( interactor.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
                return;

            if ( !StructureAuthorization.CanBuild( Scene, interactor, ApartmentId ) )
                return;

            var inventory = interactor.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory is null || !inventory.TryRemove( KitItemId, 1 ) )
                return;

            var furnitureGo = new GameObject( true, $"Furniture_{AnchorId}" );
            furnitureGo.Parent = GameObject.Parent ?? GameObject;
            furnitureGo.WorldPosition = GameObject.WorldPosition;
            furnitureGo.WorldRotation = GameObject.WorldRotation;

            var renderer = furnitureGo.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/sbox_props/cardboard_box/cardboard_box_open.vmdl" );
            furnitureGo.LocalScale = new Vector3( 1.5f, 1.5f, 1.5f );

            furnitureGo.Components.Create<BoxCollider>();

            var stash = furnitureGo.Components.Create<StashComponent>();
            stash.ApartmentId = ApartmentId;
            stash.MaxSlots = 12;

            if ( Networking.IsActive )
                furnitureGo.NetworkSpawn();

            _furnitureGo = furnitureGo;

            Persistence.PersistenceBridge.RequestSave();
            UI.PlayerFeedback.Push( "Cofre colocado" );
            Log.Info( $"UB.Building FurnitureColocado apartment={ApartmentId} anchor={AnchorId}" );
        }
    }
}
