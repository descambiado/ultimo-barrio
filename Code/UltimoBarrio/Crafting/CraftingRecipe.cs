using System.Collections.Generic;

namespace UltimoBarrio.Crafting
{
    /// <summary>Ingrediente de una receta: id canónico + cantidad.</summary>
    public sealed class CraftingIngredient
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; } = 1;
    }

    /// <summary>Resultado de una receta: id canónico + cantidad.</summary>
    public sealed class CraftingResult
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; } = 1;
    }

    /// <summary>
    /// Receta data-driven de fabricación. Los ids de ingredientes y resultado
    /// deben existir en ItemRegistry (validación estática en QA).
    /// </summary>
    public sealed class CraftingRecipe
    {
        public string RecipeId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<CraftingIngredient> Ingredients { get; set; } = new();
        public CraftingResult Result { get; set; } = new();

        /// <summary>Tiempo en segundos que tarda la fabricación (host).</summary>
        public float CraftTime { get; set; } = 2f;

        public bool IsValid =>
            !string.IsNullOrEmpty( RecipeId )
            && Result is not null && !string.IsNullOrEmpty( Result.ItemId ) && Result.Amount > 0
            && Ingredients is not null && Ingredients.Count > 0;
    }
}
