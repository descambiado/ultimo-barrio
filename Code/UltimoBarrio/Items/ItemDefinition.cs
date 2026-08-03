using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio
{
    [GameResource("Item Definition", "item", "Defines an item in Ultimo Barrio")]
    public class ItemDefinition : GameResource
    {
        public string ItemId { get; set; } = "unknown";
        public string Name { get; set; } = "Item Name";
        public string Description { get; set; } = "Item description";
        public Texture Icon { get; set; }
        public bool IsStackable { get; set; } = true;
        public int MaxStackSize { get; set; } = 64;
        public GameObject WorldPrefab { get; set; }
    }
}
