using Sandbox;
using System;
using UltimoBarrio.Persistence;

namespace UltimoBarrio.Economy
{
    public sealed class Wallet : Component
    {
        [Sync, Property]
        public int Balance { get; private set; }

        public void AddFunds(int amount)
        {
            if (amount < 0) return;
            if (!Networking.IsHost) return;
            
            Balance += amount;
        }

        public bool TryRemoveFunds(int amount)
        {
            if (amount < 0) return false;
            if (!Networking.IsHost) return false;

            if (Balance >= amount)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }

        public void LoadData(int savedBalance)
        {
            if (!Networking.IsHost) return;
            Balance = savedBalance;
        }
    }
}
