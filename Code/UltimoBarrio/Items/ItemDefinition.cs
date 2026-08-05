using Sandbox;

namespace UltimoBarrio
{
    public enum ItemCategory
    {
        Resource,
        Consumable,
        Ammo,
        Melee,
        Firearm,
        Utility,
        Medical
    }

    /// <summary>
    /// Definición data-driven de un ítem. Fuente de verdad única: ItemRegistry
    /// resuelve GameResources del editor primero y el catálogo canónico de
    /// ItemCatalog después; nadie debe hardcodear ítems fuera de esos dos sitios.
    ///
    /// Los IDs canónicos de la vertical slice son:
    ///   chatarra, water, medicine, ammo_9mm, weapon_crowbar, weapon_usp
    /// más las variantes de chatarra (scrap_*), tela, madera, componentes,
    /// consumibles fabricados y armas de apoyo (weapon_knife).
    /// </summary>
    [GameResource("Item Definition", "item", "A data-driven definition of an inventory item.", Icon = "category")]
    public class ItemDefinition : GameResource
    {
        public string ItemId { get; set; } = "unknown";
        public string DisplayName { get; set; } = "Item Name";
        public string Description { get; set; } = "Item description";
        public string Icon { get; set; } // String path for simple UI icons or Texture
        public int StackSize { get; set; } = 64;
        public ItemCategory Category { get; set; } = ItemCategory.Resource;

        /// <summary>Slot de equipamiento: "Primary", "Melee", "Hand" o vacío (no equipable).</summary>
        public string EquipSlot { get; set; }

        /// <summary>Prefab del pickup que se suelta al hacer drop.</summary>
        public string WorldPrefab { get; set; }

        /// <summary>Prefab en primera persona (arma/sostenido del propietario).</summary>
        public string ViewModelPrefab { get; set; }

        /// <summary>Prefab en tercera persona (arma/sostenido para clientes remotos).</summary>
        public string WorldModelPrefab { get; set; }

        /// <summary>Tipo de munición que consume (armas de fuego), p. ej. "ammo_9mm".</summary>
        public string AmmoType { get; set; }

        /// <summary>Si es consumible usable con click izquierdo (agua, medicina, vendaje).</summary>
        public bool Usable { get; set; } = false;

        public bool Droppable { get; set; } = true;

        /// <summary>Peso conceptual por unidad; alimenta la penalización de movimiento.</summary>
        public float Weight { get; set; } = 1f;

        /// <summary>Salud restaurada al consumir (consumibles).</summary>
        public int ConsumeHeal { get; set; }

        /// <summary>Tiempo en segundos que tarda la acción de usar/consumir.</summary>
        public float UseTime { get; set; } = 1f;

        /// <summary>Tamaño de cargador (armas de fuego).</summary>
        public int MagazineSize { get; set; }

        /// <summary>Daño base (armas).</summary>
        public float Damage { get; set; }

        /// <summary>Cadencia (armas): segundos entre disparos.</summary>
        public float FireRate { get; set; } = 0.25f;

        /// <summary>Radio de alcance (melee).</summary>
        public float MeleeRange { get; set; } = 80f;

        public bool IsEquippable =>
            Category is ItemCategory.Melee or ItemCategory.Firearm
            || Usable
            || !string.IsNullOrEmpty( EquipSlot );

        public bool IsWeapon => Category is ItemCategory.Melee or ItemCategory.Firearm;
    }
}
