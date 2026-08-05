using System;
using System.Threading;
using UltimoBarrio.Persistence;

namespace UltimoBarrio.Crafting
{
    public enum CraftResult
    {
        Success,
        NotHost,
        InvalidInventory,
        InvalidRecipe,
        OutOfRange,
        MissingIngredients,
        InventoryFull,
        PersistFailed
    }

    /// <summary>
    /// Servicio de fabricación host-autoritativo y atómico:
    ///   1. validar distancia e ingredientes
    ///   2. reservar (gate + consumo)
    ///   3. crear el resultado
    ///   4. persistir
    ///   5. rollback completo si cualquier paso falla
    /// El cliente solo envía la receta; el host decide.
    /// </summary>
    public sealed class CraftingService
    {
        private readonly object _gate = new();

        public CraftResult TryCraft(
            InventoryComponent inventory,
            Vector3 playerPosition,
            Vector3 stationPosition,
            float maxDistance,
            CraftingRecipe recipe,
            Func<PersistenceSaveResult> persist = null )
        {
            if ( !Networking.IsHost )
                return CraftResult.NotHost;

            if ( inventory is null || inventory.IsProxy )
                return CraftResult.InvalidInventory;

            if ( Vector3.DistanceBetween( playerPosition, stationPosition ) > maxDistance )
                return CraftResult.OutOfRange;

            if ( recipe is null || !recipe.IsValid )
                return CraftResult.InvalidRecipe;

            lock ( _gate )
            {
                // 1. Validar ingredientes (sin mutar).
                foreach ( var ingredient in recipe.Ingredients )
                {
                    if ( inventory.GetCount( ingredient.ItemId ) < ingredient.Amount )
                        return CraftResult.MissingIngredients;
                }

                // 2. Consumir ingredientes con rollback parcial.
                var consumed = 0;
                try
                {
                    for ( int i = 0; i < recipe.Ingredients.Count; i++ )
                    {
                        if ( !inventory.TryRemove( recipe.Ingredients[i].ItemId, recipe.Ingredients[i].Amount ) )
                        {
                            RollbackConsumed( inventory, recipe, consumed );
                            return CraftResult.MissingIngredients;
                        }

                        consumed++;
                    }

                    // 3. Crear resultado; si falla, rollback total.
                    if ( !inventory.TryAdd( recipe.Result.ItemId, recipe.Result.Amount ) )
                    {
                        RollbackConsumed( inventory, recipe, consumed );
                        return CraftResult.InventoryFull;
                    }
                }
                catch ( Exception ex )
                {
                    RollbackConsumed( inventory, recipe, consumed );
                    Log.Error( $"[Crafting] Excepción: {ex}" );
                    return CraftResult.InvalidInventory;
                }

                // 4. Persistir; si falla, rollback completo.
                if ( persist is not null )
                {
                    var saveResult = persist();
                    if ( !saveResult.Succeeded )
                    {
                        // Rollback del resultado y de los ingredientes.
                        inventory.TryRemove( recipe.Result.ItemId, recipe.Result.Amount );
                        RollbackConsumed( inventory, recipe, consumed );
                        return CraftResult.PersistFailed;
                    }
                }

                return CraftResult.Success;
            }
        }

        private static void RollbackConsumed( InventoryComponent inventory, CraftingRecipe recipe, int consumedCount )
        {
            for ( int i = 0; i < consumedCount; i++ )
                inventory.TryAdd( recipe.Ingredients[i].ItemId, recipe.Ingredients[i].Amount );
        }
    }
}
