using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio
{
    /// <summary>
    /// Catálogo canónico de ítems (fuente única de verdad junto con los
    /// GameResources del editor). ItemRegistry consulta primero los recursos
    /// del editor y luego este catálogo. Todo id referenciado por crafting,
    /// loot o trader debe existir aquí o en un GameResource.
    ///
    /// Los ids NO cambian nunca: se persisten en los snapshots.
    /// </summary>
    public static class ItemCatalog
    {
        private static readonly Dictionary<string, ItemDefinition> _definitions;

        static ItemCatalog()
        {
            _definitions = new Dictionary<string, ItemDefinition>( StringComparer.Ordinal );

            // ── Recursos ─────────────────────────────────────────────────────
            Add( "chatarra", "Chatarra", "Metal viejo y utilizable", ItemCategory.Resource, 99, weight: 1f );
            Add( "scrap_metal", "Chatarra metálica", "Planchas de metal recicladas", ItemCategory.Resource, 50, weight: 2f );
            Add( "scrap_electronics", "Electrónica", "Circuitos y piezas eléctricas", ItemCategory.Resource, 50, weight: 0.5f );
            Add( "scrap_tools", "Herramientas", "Útiles de taller", ItemCategory.Resource, 30, weight: 1.5f );
            Add( "scrap_parts", "Piezas", "Recambios variados", ItemCategory.Resource, 50, weight: 1f );
            Add( "scrap_cable", "Cable", "Cableado y conductores", ItemCategory.Resource, 60, weight: 0.5f );
            Add( "cloth", "Tela", "Retales y tejidos", ItemCategory.Resource, 40, weight: 0.3f );
            Add( "wood", "Madera", "Tablones y listones", ItemCategory.Resource, 40, weight: 2.5f );
            Add( "components", "Componentes", "Piezas de precisión", ItemCategory.Resource, 30, weight: 0.8f );

            // ── Consumibles ──────────────────────────────────────────────────
            AddConsumable( "water", "Agua", "Agua purificada", 5, heal: 0, weight: 1f );
            AddConsumable( "medicine", "Medicinas", "Cura tus heridas", 5, heal: 25, weight: 0.5f );
            AddConsumable( "bandage", "Vendaje", "Cura heridas ligeras", 8, heal: 15, weight: 0.2f );
            AddConsumable( "food", "Comida", "Alimento envasado", 6, heal: 10, weight: 0.8f );

            // ── Munición ─────────────────────────────────────────────────────
            Add( "ammo_9mm", "Mun. 9mm", "Balas calibre 9mm", ItemCategory.Ammo, 120, weight: 0.05f );

            // ── Consumibles fabricados / utilidades ──────────────────────────
            Add( "repair_kit", "Kit de reparación", "Repara puertas, ventanas y barricadas", ItemCategory.Utility, 5, weight: 3f, usable: false );
            Add( "barricade", "Barricada", "Refuerzo colocable para puertas y ventanas", ItemCategory.Utility, 10, weight: 8f );

            // ── Armas ────────────────────────────────────────────────────────
            AddWeapon( "weapon_usp", "Pistola USP", "Pistola estándar", ItemCategory.Firearm,
                ammoType: "ammo_9mm", magazine: 12, damage: 15f, fireRate: 0.25f,
                viewModel: "prefabs/weapons/v_usp.prefab", worldModel: "prefabs/weapons/ub_usp.prefab",
                worldPickup: "prefabs/items/pf_usp_pickup.prefab" );

            AddWeapon( "weapon_crowbar", "Palanca", "Para abrir cabezas", ItemCategory.Melee,
                magazine: 0, damage: 25f, fireRate: 0.5f, meleeRange: 80f,
                viewModel: "prefabs/weapons/v_melee.prefab", worldModel: "prefabs/weapons/ub_melee.prefab",
                worldPickup: "prefabs/items/pf_scrap_tools.prefab" );

            AddWeapon( "weapon_knife", "Cuchillo", "Arma blanca rápida", ItemCategory.Melee,
                magazine: 0, damage: 18f, fireRate: 0.35f, meleeRange: 70f,
                viewModel: "prefabs/weapons/v_melee.prefab", worldModel: "prefabs/weapons/ub_melee.prefab",
                worldPickup: "prefabs/items/pf_scrap_tools.prefab" );
        }

        private static void Add(
            string itemId,
            string displayName,
            string description,
            ItemCategory category,
            int stackSize,
            float weight,
            bool usable = false,
            string equipSlot = null,
            string worldPrefab = null )
        {
            _definitions[itemId] = new ItemDefinition
            {
                ItemId = itemId,
                DisplayName = displayName,
                Description = description,
                Category = category,
                StackSize = stackSize,
                Weight = weight,
                Usable = usable,
                EquipSlot = equipSlot ?? (category is ItemCategory.Melee or ItemCategory.Firearm ? "Primary" : ""),
                WorldPrefab = worldPrefab ?? "prefabs/items/pf_scrap_pickup.prefab",
                Droppable = true
            };
        }

        private static void AddConsumable( string itemId, string displayName, string description, int stackSize, int heal, float weight )
        {
            _definitions[itemId] = new ItemDefinition
            {
                ItemId = itemId,
                DisplayName = displayName,
                Description = description,
                Category = ItemCategory.Consumable,
                StackSize = stackSize,
                Weight = weight,
                Usable = true,
                ConsumeHeal = heal,
                EquipSlot = "Hand",
                WorldPrefab = "prefabs/items/pf_scrap_pickup.prefab",
                Droppable = true
            };
        }

        private static void AddWeapon(
            string itemId, string displayName, string description, ItemCategory category,
            string ammoType, int magazine, float damage, float fireRate, float meleeRange = 0f,
            string viewModel = null, string worldModel = null, string worldPickup = null )
        {
            _definitions[itemId] = new ItemDefinition
            {
                ItemId = itemId,
                DisplayName = displayName,
                Description = description,
                Category = category,
                StackSize = 1,
                Weight = category == ItemCategory.Firearm ? 3f : 2f,
                Usable = false,
                EquipSlot = category == ItemCategory.Melee ? "Melee" : "Primary",
                AmmoType = ammoType,
                MagazineSize = magazine,
                Damage = damage,
                FireRate = fireRate,
                MeleeRange = meleeRange,
                ViewModelPrefab = viewModel,
                WorldModelPrefab = worldModel,
                WorldPrefab = worldPickup ?? "prefabs/items/pf_scrap_pickup.prefab",
                Droppable = true
            };
        }

        public static IReadOnlyCollection<ItemDefinition> All => _definitions.Values;

        public static bool TryGet( string itemId, out ItemDefinition definition )
        {
            definition = null;
            return !string.IsNullOrEmpty( itemId ) && _definitions.TryGetValue( itemId, out definition );
        }

        /// <summary>
        /// Valida el catálogo canónico: ids vacíos, ids duplicados, armas sin
        /// presentación (worldmodel/viewmodel) y consumibles sin efecto.
        /// Devuelve una lista vacía si todo es válido.
        /// </summary>
        public static List<string> Validate()
        {
            var errors = new List<string>();

            foreach ( var pair in _definitions )
            {
                var definition = pair.Value;

                if ( string.IsNullOrWhiteSpace( definition.ItemId ) )
                {
                    errors.Add( "Un item del catálogo tiene ItemId vacío." );
                    continue;
                }

                if ( !string.Equals( definition.ItemId, pair.Key, StringComparison.Ordinal ) )
                {
                    errors.Add( $"Ítem '{definition.ItemId}' registrado bajo otra clave '{pair.Key}'." );
                }

                if ( definition.Category is ItemCategory.Melee or ItemCategory.Firearm )
                {
                    if ( string.IsNullOrEmpty( definition.WorldModelPrefab ) )
                        errors.Add( $"Arma '{definition.ItemId}' sin WorldModelPrefab (presentación en tercera persona)." );

                    if ( string.IsNullOrEmpty( definition.ViewModelPrefab ) )
                        errors.Add( $"Arma '{definition.ItemId}' sin ViewModelPrefab (presentación en primera persona)." );

                    if ( definition.Category == ItemCategory.Firearm && string.IsNullOrEmpty( definition.AmmoType ) )
                        errors.Add( $"Arma de fuego '{definition.ItemId}' sin AmmoType." );
                }

                if ( definition.Usable && definition.Category == ItemCategory.Consumable && definition.ConsumeHeal <= 0 )
                    errors.Add( $"Consumible '{definition.ItemId}' sin efecto de curación (ConsumeHeal <= 0)." );
            }

            return errors;
        }
    }
}
