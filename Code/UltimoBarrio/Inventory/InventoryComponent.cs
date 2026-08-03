using Sandbox;
using System;
using UltimoBarrio.Core;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio
{
    public class InventorySlot
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }
    }

    public class InventoryComponent : Component, IInventory
    {
        [Property] public string InventoryId { get; set; } = string.Empty;
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

        [Rpc.Host]
        public void RequestTransfer(string itemId, int amount, Guid targetInventoryId)
        {
            var targetInv = Scene.GetAllComponents<InventoryComponent>().FirstOrDefault(c => c.GameObject.Id == targetInventoryId);
            if (targetInv == null) return;

            // Optional: Validate distance or ownership here if needed
            
            if (TryRemove(itemId, amount))
            {
                if (!targetInv.TryAdd(itemId, amount))
                {
                    // Refund if add failed
                    TryAdd(itemId, amount);
                }
            }
        }
    }

        [Rpc.Host]
        public void RequestDrop(string itemId, int amount)
        {
            if (TryRemove(itemId, amount))
            {
                var prefabPath = "prefabs/items/pf_scrap_pickup.prefab"; // Default to scrap for now
                if (itemId != "scrap" && itemId != "chatarra")
                {
                    // For demo purposes, we will use scrap pickup for all dropped items, or instantiate dynamic if possible
                    prefabPath = "prefabs/items/pf_scrap_pickup.prefab"; 
                }

                var prefab = Scene.Directory.FindPrefab(prefabPath); // Wait, s&box API is different. 
                // Wait, it is SceneUtility.GetPrefabScene() or similar.
                
                // s&box way to spawn prefab:
                var obj = SceneUtility.GetPrefabScene(prefabPath)?.Clone();
                if (obj != null)
                {
                    obj.WorldPosition = GameObject.WorldPosition + Vector3.Up * 50f + GameObject.WorldRotation.Forward * 50f;
                    var pickup = obj.Components.Get<WorldItemPickup>();
                    if (pickup != null)
                    {
                        pickup.ItemId = itemId;
                        pickup.Amount = amount;
                    }
                    obj.NetworkSpawn();
                }
            }
        }
}
