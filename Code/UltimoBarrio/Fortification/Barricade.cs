using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Barricada colocada sobre un BarricadeAnchor. Bloquea el paso y tiene
    /// salud propia; al destruirse desaparece (los materiales no se devuelven).
    /// </summary>
    [Title( "Barricade" )]
    [Category( "Último Barrio — Fortification" )]
    [Icon( "construction" )]
    public sealed class Barricade : DestructibleStructure
    {
        [Property] public string ApartmentId { get; set; } = string.Empty;
        [Property] public string AnchorId { get; set; } = string.Empty;

        /// <summary>Ítem de kit colocado (p.ej. "wooden_barricade_kit") -- se usa para
        /// saber qué receta de crafteo reembolsar parcialmente al desmontar.</summary>
        [Property] public string PlacedKitItemId { get; set; } = string.Empty;

        // Sin override de OnStart a propósito: BarricadeAnchor.ProcessPlace y
        // RestoreBarricade ya fijan MaxHealth/Health explícitamente según el
        // tier colocado (wooden_barricade_kit=150, reinforced_barricade_kit=300) justo
        // tras Create<Barricade>(). Un OnStart que hardcodee MaxHealth=150 aquí
        // pisaría ese valor cuando el motor por fin ejecute OnStart (no es
        // síncrono con Create<T>()), bajando de nivel cualquier barricada
        // reforzada en cuanto arrancara.

        /// <summary>
        /// A salud completa no hay nada que reparar -- en vez de simplemente
        /// rechazar la interacción (como hace la base), se ofrece desmontar. Un
        /// segundo E sobre una barricada dañada sigue reparando (comportamiento
        /// de la base intacto).
        /// </summary>
        public override string GetInteractionPrompt( InteractionRequest request )
        {
            if ( IsDestroyed )
                return "Estructura destruida";

            if ( Health >= MaxHealth )
                return "Desmontar barricada (E) — recuperas parte de los materiales";

            return base.GetInteractionPrompt( request );
        }

        public override bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null || IsDestroyed )
                return false;

            if ( Health >= MaxHealth )
                return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= 200f;

            return base.CanInteract( request );
        }

        public override void OnInteract( InteractionRequest request )
        {
            if ( Health >= MaxHealth )
            {
                if ( IsProxy )
                {
                    RequestDismantleOnHost( request.InteractorObject?.Id ?? Guid.Empty );
                    return;
                }

                ProcessDismantle( request.InteractorObject );
                return;
            }

            base.OnInteract( request );
        }

        [Rpc.Host]
        private void RequestDismantleOnHost( Guid interactorObjectId )
        {
            var interactor = Scene.Directory.FindByGuid( interactorObjectId );
            ProcessDismantle( interactor );
        }

        private void ProcessDismantle( GameObject interactor )
        {
            if ( interactor is null || !Networking.IsHost )
                return;

            if ( !Building.StructureAuthorization.CanBuild( Scene, interactor, ApartmentId ) )
            {
                UI.PlayerFeedback.Push( "Solo el propietario puede desmontar" );
                return;
            }

            var inventory = interactor.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory is null )
                return;

            var recipeId = PlacedKitItemId switch
            {
                "wooden_barricade_kit" => "craft_wooden_barricade_kit",
                "reinforced_barricade_kit" => "craft_reinforced_barricade_kit",
                _ => null
            };
            var recipe = recipeId is not null ? Crafting.CraftingLibrary.Get( recipeId ) : null;

            if ( recipe is not null )
            {
                foreach ( var ingredient in recipe.Ingredients )
                {
                    var refund = (int)MathF.Floor( ingredient.Amount * BarricadeAnchor.DismantleRefundFraction );
                    if ( refund > 0 )
                        inventory.TryAdd( ingredient.ItemId, refund );
                }
            }

            UI.PlayerFeedback.Push( "Barricada desmontada" );
            OnStructureDestroyed();
        }

        protected override void OnStructureDestroyed()
        {
            // Notificar al anchor para que quede libre.
            var anchor = Scene.GetAllComponents<BarricadeAnchor>()
                .FirstOrDefault( a => a.AnchorId == AnchorId && a.ApartmentId == ApartmentId );
            if ( anchor is not null )
                anchor.OnBarricadeDestroyed();

            Persistence.PersistenceBridge.RequestSave();
            GameObject.Destroy();
        }
    }
}
