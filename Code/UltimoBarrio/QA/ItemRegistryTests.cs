using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Crafting;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Validadores estáticos del registro canónico de ítems.
    /// Solo consultan estado: no fabrican éxitos ni mutan la escena.
    /// </summary>
    public static class ItemRegistryTests
    {
        [ConCmd( "ub_test_items" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando ItemRegistry ===" );

            var passed = 0;
            var failed = 0;

            var canonical = new[]
            {
                "chatarra", "water", "medicine", "ammo_9mm", "weapon_crowbar", "weapon_usp",
                "scrap_metal", "scrap_electronics", "scrap_tools", "scrap_parts", "scrap_cable",
                "cloth", "wood", "components", "bandage", "repair_kit", "wooden_barricade_kit", "weapon_knife"
            };

            foreach ( var id in canonical )
            {
                if ( ItemRegistry.Exists( id ) )
                {
                    passed++;
                }
                else
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: item canónico ausente '{id}'" );
                }
            }

            var catalogErrors = ItemRegistry.ValidateCatalog();
            foreach ( var error in catalogErrors )
            {
                failed++;
                Log.Error( $"[UBTest] FAIL: {error}" );
            }
            if ( catalogErrors.Count == 0 )
                passed++;

            // Duplicados: registrar dos veces el mismo id debe fallar en ValidateReferences.
            var refErrors = ItemRegistry.ValidateReferences( new[] { "chatarra", "chatarra", "no_existe" } );
            var duplicateHandled = refErrors.Count( e => e.Contains( "no_existe" ) ) == 1;
            if ( duplicateHandled )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: ValidateReferences no detectó la referencia rota." );
            }

            // Armas con presentación.
            var usp = ItemRegistry.GetDefinition( "weapon_usp" );
            if ( usp is not null && usp.IsWeapon && !string.IsNullOrEmpty( usp.WorldModelPrefab )
                && !string.IsNullOrEmpty( usp.ViewModelPrefab ) && usp.AmmoType == "ammo_9mm" )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: weapon_usp no tiene presentación completa o AmmoType." );
            }

            // Referencias de las recetas de crafting (si las recetas existen).
            try
            {
                var recipeIds = CraftingLibrary.AllRecipes
                    .SelectMany( r => r.Ingredients.Select( i => i.ItemId ) )
                    .Concat( CraftingLibrary.AllRecipes.Select( r => r.Result.ItemId ) );
                var brokenRecipes = ItemRegistry.ValidateReferences( recipeIds );
                foreach ( var broken in brokenRecipes )
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: receta rota → {broken}" );
                }
                if ( brokenRecipes.Count == 0 )
                    passed++;
            }
            catch ( Exception ex )
            {
                failed++;
                Log.Error( $"[UBTest] FAIL: no se pudo validar recetas ({ex.Message})" );
            }

            Log.Info( $"[UBTest] === ITEMS: {passed} passed, {failed} failed ===" );
        }
    }
}
