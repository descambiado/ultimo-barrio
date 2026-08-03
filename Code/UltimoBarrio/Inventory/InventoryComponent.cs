using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio
{
    public class InventorySlot
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }
    }

    public class InventoryComponent : Component, IInventoryOwner
    {
        [Property] public int MaxSlots { get; set; } = 24;
        [Property] public int HotbarSlots { get; set; } = 6;
        
        [Sync] public NetList<InventorySlot> Slots { get; set; } = new NetList<InventorySlot>();

        protected override void OnAwake()
        {
            if (Slots.Count == 0 && !IsProxy)
            {
                for(int i=0; i<MaxSlots; i++) Slots.Add(new InventorySlot { ItemId = "", Amount = 0 });
            }
        }

        public bool CanAdd(string itemId, int amount)
        {
            return true; // Simplification for now
        }

        public bool TryAdd(string itemId, int amount)
        {
            if (IsProxy) return false;
            // Find existing stack
            var existing = Slots.FirstOrDefault(s => s.ItemId == itemId);
            if (existing != null)
            {
                existing.Amount += amount;
                return true;
            }
            // Find empty slot
            var empty = Slots.FirstOrDefault(s => string.IsNullOrEmpty(s.ItemId));
            if (empty != null)
            {
                empty.ItemId = itemId;
                empty.Amount = amount;
                return true;
            }
            return false;
        }

        public bool TryRemove(string itemId, int amount)
        {
            if (IsProxy) return false;
            var existing = Slots.FirstOrDefault(s => s.ItemId == itemId);
            if (existing != null && existing.Amount >= amount)
            {
                existing.Amount -= amount;
                if (existing.Amount <= 0)
                {
                    existing.ItemId = "";
                    existing.Amount = 0;
                }
                return true;
            }
            return false;
        }

        public int GetCount(string itemId)
        {
            var existing = Slots.FirstOrDefault(s => s.ItemId == itemId);
            return existing?.Amount ?? 0;
        }
    }
}
