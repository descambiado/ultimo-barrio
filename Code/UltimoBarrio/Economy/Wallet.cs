using Sandbox;
using System;
using UltimoBarrio.Persistence;
using UltimoBarrio.Core;

namespace UltimoBarrio.Economy
{
    public sealed class Wallet : Component, IWallet
    {
        [Sync, Property]
        public int Balance { get; private set; }

        public string WalletId => GameObject.Id.ToString();

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

        public bool CanAfford(int amount) => Balance >= amount;

        public bool TryWithdraw(int amount) => TryRemoveFunds(amount);

        public void Deposit(int amount) => AddFunds(amount);
    }
}
