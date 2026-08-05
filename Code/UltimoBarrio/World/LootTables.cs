using System;
using System.Collections.Generic;

namespace UltimoBarrio.World
{
    /// <summary>Entrada ponderada de una tabla de loot.</summary>
    public sealed class LootEntry
    {
        public string ItemId { get; set; } = "chatarra";
        public int MinAmount { get; set; } = 1;
        public int MaxAmount { get; set; } = 1;
        public float Weight { get; set; } = 1f;
    }

    /// <summary>
    /// Tablas de loot de la vertical slice. Separadas de los pickups sueltos:
    /// las tablas alimentan contenedores (LootContainer) y el generador
    /// (LootSpawner). Todos los ids deben existir en ItemRegistry.
    /// </summary>
    public static class LootTables
    {
        private static readonly Dictionary<string, List<LootEntry>> _tables;

        static LootTables()
        {
            _tables = new Dictionary<string, List<LootEntry>>( StringComparer.Ordinal );

            _tables["StreetLoot"] = new List<LootEntry>
            {
                new() { ItemId = "chatarra", MinAmount = 1, MaxAmount = 3, Weight = 10f },
                new() { ItemId = "scrap_metal", MinAmount = 1, MaxAmount = 2, Weight = 6f },
                new() { ItemId = "scrap_parts", MinAmount = 1, MaxAmount = 2, Weight = 5f },
                new() { ItemId = "scrap_cable", MinAmount = 1, MaxAmount = 2, Weight = 5f },
                new() { ItemId = "cloth", MinAmount = 1, MaxAmount = 2, Weight = 4f },
                new() { ItemId = "wood", MinAmount = 1, MaxAmount = 2, Weight = 4f },
                new() { ItemId = "water", MinAmount = 1, MaxAmount = 1, Weight = 3f }
            };

            _tables["ApartmentLoot"] = new List<LootEntry>
            {
                new() { ItemId = "chatarra", MinAmount = 1, MaxAmount = 2, Weight = 8f },
                new() { ItemId = "cloth", MinAmount = 1, MaxAmount = 2, Weight = 6f },
                new() { ItemId = "wood", MinAmount = 1, MaxAmount = 2, Weight = 5f },
                new() { ItemId = "food", MinAmount = 1, MaxAmount = 1, Weight = 5f },
                new() { ItemId = "medicine", MinAmount = 1, MaxAmount = 1, Weight = 2f },
                new() { ItemId = "components", MinAmount = 1, MaxAmount = 1, Weight = 3f }
            };

            _tables["ShopLoot"] = new List<LootEntry>
            {
                new() { ItemId = "chatarra", MinAmount = 2, MaxAmount = 5, Weight = 8f },
                new() { ItemId = "scrap_electronics", MinAmount = 1, MaxAmount = 2, Weight = 5f },
                new() { ItemId = "scrap_tools", MinAmount = 1, MaxAmount = 1, Weight = 4f },
                new() { ItemId = "components", MinAmount = 1, MaxAmount = 2, Weight = 5f },
                new() { ItemId = "ammo_9mm", MinAmount = 6, MaxAmount = 12, Weight = 4f },
                new() { ItemId = "water", MinAmount = 1, MaxAmount = 2, Weight = 4f }
            };

            _tables["MedicalLoot"] = new List<LootEntry>
            {
                new() { ItemId = "medicine", MinAmount = 1, MaxAmount = 2, Weight = 8f },
                new() { ItemId = "bandage", MinAmount = 1, MaxAmount = 2, Weight = 8f },
                new() { ItemId = "water", MinAmount = 1, MaxAmount = 1, Weight = 5f },
                new() { ItemId = "cloth", MinAmount = 1, MaxAmount = 2, Weight = 5f }
            };

            _tables["DangerousLoot"] = new List<LootEntry>
            {
                new() { ItemId = "ammo_9mm", MinAmount = 6, MaxAmount = 18, Weight = 8f },
                new() { ItemId = "scrap_electronics", MinAmount = 1, MaxAmount = 3, Weight = 6f },
                new() { ItemId = "components", MinAmount = 1, MaxAmount = 2, Weight = 6f },
                new() { ItemId = "medicine", MinAmount = 1, MaxAmount = 2, Weight = 4f },
                new() { ItemId = "weapon_knife", MinAmount = 1, MaxAmount = 1, Weight = 2f },
                new() { ItemId = "repair_kit", MinAmount = 1, MaxAmount = 1, Weight = 2f }
            };
        }

        public static IReadOnlyCollection<string> TableIds => _tables.Keys;

        public static bool TryGet( string tableId, out IReadOnlyList<LootEntry> entries )
        {
            entries = null;
            if ( string.IsNullOrEmpty( tableId ) || !_tables.TryGetValue( tableId, out var list ) )
                return false;

            entries = list;
            return true;
        }

        /// <summary>Rueda una entrada de la tabla por peso.</summary>
        public static LootEntry Roll( string tableId )
        {
            if ( !TryGet( tableId, out var entries ) || entries.Count == 0 )
                return null;

            float totalWeight = 0f;
            foreach ( var entry in entries )
                totalWeight += Math.Max( 0f, entry.Weight );

            if ( totalWeight <= 0f )
                return entries[0];

            float roll = (float)( Random.Shared.NextDouble() * totalWeight );
            float cumulative = 0f;
            foreach ( var entry in entries )
            {
                cumulative += Math.Max( 0f, entry.Weight );
                if ( roll <= cumulative )
                    return entry;
            }

            return entries[^1];
        }

        /// <summary>Valida que todas las tablas referencian ítems existentes.</summary>
        public static List<string> Validate()
        {
            var errors = new List<string>();
            var referenced = new List<string>();

            foreach ( var table in _tables.Values )
            {
                foreach ( var entry in table )
                {
                    referenced.Add( entry.ItemId );

                    if ( entry.MinAmount < 1 || entry.MaxAmount < entry.MinAmount )
                        errors.Add( $"Tabla loot: '{entry.ItemId}' con rango inválido ({entry.MinAmount}..{entry.MaxAmount})." );
                }
            }

            errors.AddRange( ItemRegistry.ValidateReferences( referenced ) );
            return errors;
        }
    }
}
