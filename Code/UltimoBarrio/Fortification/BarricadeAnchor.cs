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

        public Barricade BarricadeReference => _barricade;

        private Barricade _barricade;

        /// <summary>Tiers colocables aquí, en orden de preferencia (mejor primero).</summary>
        private static readonly (string ItemId, float MaxHealth)[] _tiers =
        {
            ( "reinforced_barricade_kit", 300f ),
            ( "barricade", 150f )
        };

        public string GetInteractionPrompt( InteractionRequest request )
        {
            if ( HasBarricade )
                return "Barricada colocada (revisa su salud)";

            var inv = request.InteractorObject?.Components.GetInDescendantsOrSelf<InventoryComponent>();
            foreach ( var (itemId, _) in _tiers )
            {
                int count = inv?.GetCount( itemId ) ?? 0;
                if ( count > 0 )
                {
                    var def = ItemRegistry.GetDefinition( itemId );
                    return $"Colocar {def?.DisplayName ?? itemId} (tienes {count})";
                }
            }

            return "Barricada (no llevas ninguna)";
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
            if ( inventory is null )
                return;

            string placedItemId = null;
            float maxHealth = 150f;
            foreach ( var (itemId, tierHealth) in _tiers )
            {
                if ( inventory.GetCount( itemId ) < 1 )
                    continue;

                if ( !inventory.TryRemove( itemId, 1 ) )
                    continue;

                placedItemId = itemId;
                maxHealth = tierHealth;
                break;
            }

            if ( placedItemId is null )
                return;

            var barricadeGo = new GameObject( true, $"Barricade_{AnchorId}" );
            barricadeGo.Parent = GameObject.Parent ?? GameObject;
            barricadeGo.WorldPosition = GameObject.WorldPosition;
            barricadeGo.WorldRotation = GameObject.WorldRotation;

            var renderer = barricadeGo.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/dev/box.vmdl" );
            renderer.Tint = placedItemId == "reinforced_barricade_kit" ? new Color( 0.5f, 0.5f, 0.55f ) : new Color( 0.45f, 0.3f, 0.15f );
            barricadeGo.LocalScale = new Vector3( 40f, 10f, 70f );

            var collider = barricadeGo.Components.Create<BoxCollider>();

            var barricade = barricadeGo.Components.Create<Barricade>();
            barricade.ApartmentId = ApartmentId;
            barricade.AnchorId = AnchorId;

            // Component.OnStart() no se garantiza síncrono tras Create<T>() (puede
            // diferirse al siguiente tick) — sin esto la barricada nace con
            // Health=0/MaxHealth=200 (los valores por defecto de
            // DestructibleStructure) hasta que OnStart corre, y con Health<=0,
            // IsDestroyed ya es true: cualquier daño real en esa ventana no hace
            // nada porque TakeDamage descarta las estructuras ya destruidas.
            barricade.MaxHealth = maxHealth;
            barricade.SetHealth( maxHealth );

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

        /// <summary>Restaura una barricada guardada (host, sin consumir ítem).</summary>
        internal void RestoreBarricade( float health, float maxHealth )
        {
            if ( !Networking.IsHost || HasBarricade || health <= 0f )
                return;

            var barricadeGo = new GameObject( true, $"Barricade_{AnchorId}" );
            barricadeGo.Parent = GameObject.Parent ?? GameObject;
            barricadeGo.WorldPosition = GameObject.WorldPosition;
            barricadeGo.WorldRotation = GameObject.WorldRotation;

            var renderer = barricadeGo.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/dev/box.vmdl" );
            renderer.Tint = new Color( 0.45f, 0.3f, 0.15f );
            barricadeGo.LocalScale = new Vector3( 40f, 10f, 70f );

            barricadeGo.Components.Create<BoxCollider>();

            var barricade = barricadeGo.Components.Create<Barricade>();
            barricade.ApartmentId = ApartmentId;
            barricade.AnchorId = AnchorId;
            barricade.MaxHealth = maxHealth > 0f ? maxHealth : 150f;
            barricade.SetHealth( health );

            if ( Networking.IsActive )
                barricadeGo.NetworkSpawn();

            _barricade = barricade;
        }
    }
}
