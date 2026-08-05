using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Core;

namespace UltimoBarrio.Crafting
{
    /// <summary>
    /// Estación de crafteo: abre el panel de fabricación. Todo el crafteo es
    /// host-autoritativo vía CraftingService; el cliente solo envía la receta.
    /// </summary>
    [Title( "Crafting Station" )]
    [Category( "Último Barrio — Crafting" )]
    [Icon( "construction" )]
    public sealed class CraftingStation : Component, IInteractable
    {
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        /// <summary>Recetas propias del editor (si vacío, se usa CraftingLibrary).</summary>
        [Property] public List<CraftingRecipe> Recipes { get; set; } = new();

        public IReadOnlyList<CraftingRecipe> AvailableRecipes
            => Recipes is { Count: > 0 } ? Recipes : CraftingLibrary.AllRecipes;

        public CraftingRecipe GetRecipe( string recipeId )
        {
            foreach ( var recipe in AvailableRecipes )
            {
                if ( recipe.RecipeId == recipeId )
                    return recipe;
            }

            return null;
        }

        public string GetInteractionPrompt( InteractionRequest request )
            => "Interactuar con la estación de crafteo";

        public bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null ) return false;
            return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
        }

        public void OnInteract( InteractionRequest request )
        {
            // Abre la UI en el cliente que interactúa.
            var hud = request.InteractorObject?.Components.GetInDescendantsOrSelf<UI.PlayerHud>();
            if ( hud is not null )
                hud.OpenCrafting( this );
        }

        /// <summary>El cliente solicita fabricar una receta; el host decide.</summary>
        [Rpc.Host]
        public void RequestCraft( Guid crafterObjectId, string recipeId )
        {
            if ( !Networking.IsHost )
                return;

            var crafter = Scene.Directory.FindByGuid( crafterObjectId );
            if ( crafter is null )
                return;

            var inventory = crafter.Components.GetInDescendantsOrSelf<InventoryComponent>();
            var recipe = GetRecipe( recipeId );

            var result = new CraftingService().TryCraft(
                inventory,
                playerPosition: crafter.WorldPosition,
                stationPosition: WorldPosition,
                maxDistance: MaxInteractionDistance,
                recipe: recipe,
                persist: null );

            var message = CraftingFeedback.GetMessage( result, recipe );
            Log.Info( $"[Crafting] resultado={result} receta={recipeId}" );
            NotifyCraftResult( message, result == CraftResult.Success );
        }

        [Rpc.Broadcast]
        private void NotifyCraftResult( string message, bool success )
        {
            UI.PlayerFeedback.Push( message );
        }
    }

    /// <summary>Traducción de resultados de crafteo a mensajes de feedback.</summary>
    public static class CraftingFeedback
    {
        public static string GetMessage( CraftResult result, CraftingRecipe recipe )
        {
            var name = recipe?.DisplayName ?? "receta";

            switch ( result )
            {
                case CraftResult.Success:
                    return $"Fabricado: {name}";
                case CraftResult.MissingIngredients:
                    return $"Faltan ingredientes para {name}";
                case CraftResult.InventoryFull:
                    return "Inventario lleno: no cabe el resultado";
                case CraftResult.OutOfRange:
                    return "Estás demasiado lejos de la estación";
                case CraftResult.PersistFailed:
                    return "El guardado falló: crafteo cancelado";
                default:
                    return $"No se pudo fabricar {name}";
            }
        }
    }
}
