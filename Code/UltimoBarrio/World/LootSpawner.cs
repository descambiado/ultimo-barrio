using Sandbox;
using System;

namespace UltimoBarrio.World
{
    /// <summary>
    /// Generador de loot: convierte una tabla en pickups o en el contenido de
    /// un contenedor. Único punto de entrada para loot procedural (spawning de
    /// zonas, drops de NPC, relleno de escena).
    /// </summary>
    public static class LootSpawner
    {
        /// <summary>Materializa un número de ítems de una tabla como pickups en una posición.</summary>
        public static int SpawnPickups( Scene scene, string tableId, Vector3 position, int count )
        {
            if ( scene is null || count <= 0 )
                return 0;

            int spawned = 0;
            for ( int i = 0; i < count; i++ )
            {
                var entry = LootTables.Roll( tableId );
                if ( entry is null )
                    continue;

                int amount = entry.MinAmount;
                if ( entry.MaxAmount > entry.MinAmount )
                    amount += (int)( Random.Shared.NextDouble() * ( entry.MaxAmount - entry.MinAmount + 1 ) );

                var offset = new Vector3(
                    (float)( Random.Shared.NextDouble() * 80f - 40f ),
                    (float)( Random.Shared.NextDouble() * 80f - 40f ),
                    20f );

                var pickup = ItemPickupFactory.SpawnPickup( scene, entry.ItemId, amount, 0, position + offset );
                if ( pickup is not null )
                    spawned++;
            }

            return spawned;
        }

        /// <summary>Rellena un contenedor con una tabla.</summary>
        public static bool FillContainer( LootContainer container )
        {
            if ( container is null )
                return false;

            container.FillFromTable();
            return true;
        }
    }
}
