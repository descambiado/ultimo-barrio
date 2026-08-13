using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio
{
    public static class ItemRegistry
    {
        private static Dictionary<string, ItemDefinition> _fallbacks = new Dictionary<string, ItemDefinition>();

        static ItemRegistry()
        {
            InitializeFallbacks();
        }

        private static void InitializeFallbacks()
        {
            // Registering the minimum viable items requested by the user
            _fallbacks["chatarra"] = new ItemDefinition
            {
                ItemId = "chatarra", DisplayName = "Chatarra", Description = "Metal viejo",
                Category = ItemCategory.Resource, StackSize = 99, WorldPrefab = null // Uses pf_scrap_pickup by default in RequestDrop
            };
            
            _fallbacks["water"] = new ItemDefinition
            {
                ItemId = "water", DisplayName = "Agua", Description = "Agua purificada",
                Category = ItemCategory.Consumable, StackSize = 5, Usable = true
            };
            
            _fallbacks["medicine"] = new ItemDefinition
            {
                ItemId = "medicine", DisplayName = "Medicinas", Description = "Cura tus heridas",
                Category = ItemCategory.Consumable, StackSize = 5, Usable = true
            };

            _fallbacks["ammo_9mm"] = new ItemDefinition
            {
                ItemId = "ammo_9mm", DisplayName = "Mun. 9mm", Description = "Balas calibre 9mm",
                Category = ItemCategory.Ammo, StackSize = 120
            };

            _fallbacks["ammo_buckshot"] = new ItemDefinition
            {
                ItemId = "ammo_buckshot", DisplayName = "Cartuchos", Description = "Cartuchos calibre 12",
                Category = ItemCategory.Ammo, StackSize = 60
            };

            _fallbacks["weapon_usp"] = new ItemDefinition
            {
                ItemId = "weapon_usp", DisplayName = "Pistola USP", Description = "Pistola estándar",
                Category = ItemCategory.Firearm, StackSize = 1, AmmoType = "ammo_9mm",
                EquipSlot = "Primary",
                ViewModelPrefab = "prefabs/weapons/v_usp.prefab",
                WorldModelPrefab = "prefabs/weapons/ub_usp.prefab"
            };

            _fallbacks["weapon_crowbar"] = new ItemDefinition
            {
                ItemId = "weapon_crowbar", DisplayName = "Palanca", Description = "Para abrir cabezas",
                Category = ItemCategory.Melee, StackSize = 1, EquipSlot = "Melee",
                ViewModelPrefab = "prefabs/weapons/v_melee.prefab",
                WorldModelPrefab = "prefabs/weapons/ub_melee.prefab"
            };

            _fallbacks["weapon_shotgun"] = new ItemDefinition
            {
                ItemId = "weapon_shotgun", DisplayName = "Escopeta", Description = "12 gauge",
                Category = ItemCategory.Firearm, StackSize = 1, AmmoType = "ammo_buckshot", EquipSlot = "Primary",
                ViewModelPrefab = "prefabs/content/weapons/v_shotgun_content.prefab",
                WorldModelPrefab = "prefabs/content/weapons/w_shotgun_content.prefab"
            };

            _fallbacks["weapon_knife"] = new ItemDefinition
            {
                ItemId = "weapon_knife", DisplayName = "Cuchillo", Description = "Acero frío",
                Category = ItemCategory.Melee, StackSize = 1, EquipSlot = "Melee",
                ViewModelPrefab = "prefabs/content/weapons/v_knife_content.prefab",
                WorldModelPrefab = "prefabs/content/weapons/w_knife_content.prefab"
            };
        }

        public static ItemDefinition GetDefinition(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            // Priority: Real s&box assets
            var asset = ResourceLibrary.GetAll<ItemDefinition>().FirstOrDefault(x => x.ItemId == itemId);
            if (asset != null) return asset;

            // Fallback: Hardcoded
            if (_fallbacks.TryGetValue(itemId, out var fb)) return fb;

            return null;
        }
    }
}
