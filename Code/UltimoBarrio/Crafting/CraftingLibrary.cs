using System.Collections.Generic;

namespace UltimoBarrio.Crafting
{
    /// <summary>
    /// Catálogo de recetas de la vertical slice. Data-driven por código; las
    /// recetas del editor (GameResource) pueden añadirse sin tocar esto.
    /// Sin explosivos en esta fase.
    /// </summary>
    public static class CraftingLibrary
    {
        private static readonly List<CraftingRecipe> _recipes;

        static CraftingLibrary()
        {
            _recipes = new List<CraftingRecipe>
            {
                new()
                {
                    RecipeId = "craft_ammo_9mm",
                    DisplayName = "Munición 9mm",
                    Description = "Chatarra x5 → Munición 9mm x12",
                    Ingredients = { new CraftingIngredient { ItemId = "chatarra", Amount = 5 } },
                    Result = new CraftingResult { ItemId = "ammo_9mm", Amount = 12 },
                    CraftTime = 3f
                },
                new()
                {
                    RecipeId = "craft_bandage",
                    DisplayName = "Vendaje",
                    Description = "Medicina + Tela → Vendaje",
                    Ingredients =
                    {
                        new CraftingIngredient { ItemId = "medicine", Amount = 1 },
                        new CraftingIngredient { ItemId = "cloth", Amount = 1 }
                    },
                    Result = new CraftingResult { ItemId = "bandage", Amount = 1 },
                    CraftTime = 2f
                },
                new()
                {
                    RecipeId = "craft_repair_kit",
                    DisplayName = "Kit de reparación",
                    Description = "Chatarra + Componentes → Kit de reparación",
                    Ingredients =
                    {
                        new CraftingIngredient { ItemId = "chatarra", Amount = 5 },
                        new CraftingIngredient { ItemId = "components", Amount = 2 }
                    },
                    Result = new CraftingResult { ItemId = "repair_kit", Amount = 1 },
                    CraftTime = 4f
                },
                new()
                {
                    RecipeId = "craft_barricade",
                    DisplayName = "Barricada",
                    Description = "Madera + Chatarra → Barricada",
                    Ingredients =
                    {
                        new CraftingIngredient { ItemId = "wood", Amount = 4 },
                        new CraftingIngredient { ItemId = "chatarra", Amount = 3 }
                    },
                    Result = new CraftingResult { ItemId = "barricade", Amount = 1 },
                    CraftTime = 5f
                },
                new()
                {
                    RecipeId = "craft_crowbar",
                    DisplayName = "Palanca",
                    Description = "Chatarra + Piezas → Palanca",
                    Ingredients =
                    {
                        new CraftingIngredient { ItemId = "chatarra", Amount = 10 },
                        new CraftingIngredient { ItemId = "scrap_parts", Amount = 3 }
                    },
                    Result = new CraftingResult { ItemId = "weapon_crowbar", Amount = 1 },
                    CraftTime = 6f
                }
            };
        }

        public static IReadOnlyList<CraftingRecipe> AllRecipes => _recipes;

        public static CraftingRecipe Get( string recipeId )
        {
            if ( string.IsNullOrEmpty( recipeId ) )
                return null;

            foreach ( var recipe in _recipes )
            {
                if ( recipe.RecipeId == recipeId )
                    return recipe;
            }

            return null;
        }
    }
}
