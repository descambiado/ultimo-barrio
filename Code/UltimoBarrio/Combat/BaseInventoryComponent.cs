using Sandbox;
using System.Collections.Generic;
using UltimoBarrio;

namespace UltimoBarrio.Combat
{
    public class BaseInventoryComponent : Component, IInventoryOwner
    {
        // Simple dictionary for items and amounts
        private Dictionary<string, int> _items = new Dictionary<string, int>();

        public bool CanAdd(string itemId, int amount)
        {
            return true; // Simplified for MVP
        }

        public bool TryAdd(string itemId, int amount)
        {
            if (!CanAdd(itemId, amount)) return false;
            
            if (_items.ContainsKey(itemId))
                _items[itemId] += amount;
            else
                _items[itemId] = amount;
                
            return true;
        }

        public bool TryRemove(string itemId, int amount)
        {
            if (!_items.ContainsKey(itemId) || _items[itemId] < amount)
                return false;
                
            _items[itemId] -= amount;
            if (_items[itemId] <= 0)
                _items.Remove(itemId);
                
            return true;
        }

        public int GetCount(string itemId)
        {
            if (_items.TryGetValue(itemId, out int count))
                return count;
            return 0;
        }
    }
}
