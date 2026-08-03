using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Core;

namespace UltimoBarrio.Inventory
{
    public class UltimoBarrioInventoryItem
    {
        public string ItemId { get; set; } = string.Empty;
        public int Amount { get; set; } = 1;
        public int MaxStack { get; set; } = 50;
        public string IconPath { get; set; } = string.Empty;
    }

    [Title("Ultimo Barrio Player Inventory")]
    [Category("Último Barrio — Inventory")]
    [Icon("inventory")]
    public class UltimoBarrioPlayerInventory : Component
    {
        [Property] public int MaxSlots { get; set; } = 24;
        [Property] public string InventoryId { get; set; } = string.Empty;

        [RequireComponent] public InventoryComponent CoreInventory { get; set; }

        protected override void OnStart()
        {
            if (CoreInventory != null && string.IsNullOrEmpty(CoreInventory.InventoryId))
            {
                var connId = GameObject.Network.OwnerId;
                CoreInventory.InventoryId = $"player:{connId}:inventory";
            }
        }

        public bool TryAdd(string itemId, int amount = 1)
        {
            if (CoreInventory == null) return false;
            return CoreInventory.TryAdd(itemId, amount);
        }

        public bool TryRemove(string itemId, int amount = 1)
        {
            if (CoreInventory == null) return false;
            return CoreInventory.TryRemove(itemId, amount);
        }

        public int GetCount(string itemId)
        {
            return CoreInventory?.GetCount(itemId) ?? 0;
        }
    }

    [Title("Ultimo Barrio Stash Inventory")]
    [Category("Último Barrio — Inventory")]
    [Icon("archive")]
    public class UltimoBarrioStashInventory : Component
    {
        [Property] public int MaxSlots { get; set; } = 24;
        [Property] public string ApartmentId { get; set; } = "apartment-a01";

        [RequireComponent] public InventoryComponent CoreInventory { get; set; }

        protected override void OnStart()
        {
            if (CoreInventory != null && string.IsNullOrEmpty(CoreInventory.InventoryId))
            {
                CoreInventory.InventoryId = $"{ApartmentId}:stash";
            }
        }
    }
}
