using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio
{
    public enum ItemCategory
    {
        Resource,
        Consumable,
        Ammo,
        Melee,
        Firearm
    }

    [GameResource("Item Definition", "item", "A data-driven definition of an inventory item.", Icon = "category")]
    public class ItemDefinition : GameResource
    {
        public string ItemId { get; set; } = "unknown";
        public string DisplayName { get; set; } = "Item Name";
        public string Description { get; set; } = "Item description";
        public string Icon { get; set; } // Using string path for simple UI icons or Texture
        public int StackSize { get; set; } = 64;
        public ItemCategory Category { get; set; } = ItemCategory.Resource;
        public string EquipSlot { get; set; } // Primary, Melee, etc.
        public GameObject WorldPrefab { get; set; }
        public GameObject ViewModelPrefab { get; set; }
        public GameObject WorldModelPrefab { get; set; }
        public string AmmoType { get; set; } // For firearms
        public bool Usable { get; set; } = false;
        public bool Droppable { get; set; } = true;
    }
}
