using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio;
using UltimoBarrio.Economy;

namespace UltimoBarrio.Trading
{
    public sealed class Trader : Component, IInteractable
    {
        [Property] public int WaterPrice { get; set; } = 10;
        [Property] public int MedicinePrice { get; set; } = 20;
        [Property] public int AmmoPrice { get; set; } = 5;
        [Property] public int ScrapSellPrice { get; set; } = 2;

        public string GetInteractionPrompt(InteractionRequest request) => "Interactuar con el Comerciante";
        
        public bool CanInteract(InteractionRequest request) => true;

        public void OnInteract(InteractionRequest request)
        {
            Log.Info($"[Trader] Interaction initiated by {request.Identity.CanonicalId}");
        }

        [Rpc.Host]
        public void BuyItem(GameObject buyer, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (buyer == null) return;
            
            var wallet = buyer.Components.Get<IWallet>();
            var inventory = buyer.Components.Get<IInventory>();

            if (wallet == null || inventory == null) 
            {
                Log.Info($"[Trader] {buyer.Name} lacks wallet or inventory.");
                return;
            }

            int price = 0;
            if (itemId == "water") price = WaterPrice;
            else if (itemId == "medicine") price = MedicinePrice;
            else if (itemId == "ammo_9mm" || itemId == "ammo") { price = AmmoPrice; itemId = "ammo_9mm"; }
            else if (itemId == "weapon_usp") { price = 100; }
            else 
            {
                Log.Info($"[Trader] Invalid item: {itemId}");
                return;
            }

            int totalCost = price * amount;
            if (wallet.CanAfford(totalCost))
            {
                if (inventory.CanAdd(itemId, amount) && inventory.TryAdd(itemId, amount))
                {
                    if (wallet.TryWithdraw(totalCost))
                    {
                        Log.Info($"[Trader] {buyer.Name} bought {amount} {itemId} for {totalCost}. Balance now: ${wallet.Balance}");
                        Missions.MissionJournal.Local?.NotifyProgress(Missions.ObjectiveType.BuyItem, itemId, amount);
                    }
                    else 
                    {
                        inventory.TryRemove(itemId, amount);
                    }
                }
                else
                {
                    Log.Info($"[Trader] {buyer.Name} has no inventory space for {amount} {itemId}.");
                }
            }
            else 
            {
                Log.Info($"[Trader] {buyer.Name} cannot afford {totalCost} (Balance: ${wallet.Balance}).");
            }
        }

        [Rpc.Host]
        public void SellItem(GameObject seller, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (seller == null) return;

            var wallet = seller.Components.Get<IWallet>();
            var inventory = seller.Components.Get<IInventory>();

            if (wallet == null || inventory == null) return;

            string targetItem = (itemId == "chatarra" || itemId == "scrap") ? "chatarra" : itemId;
            int available = inventory.GetCount("chatarra");
            if (available == 0) available = inventory.GetCount("scrap");

            if (available <= 0)
            {
                Log.Info($"[Trader] {seller.Name} has no scrap to sell.");
                return;
            }

            int actualAmount = Math.Min(amount, available);
            int totalPrice = ScrapSellPrice * actualAmount;

            if (inventory.TryRemove("chatarra", actualAmount) || inventory.TryRemove("scrap", actualAmount))
            {
                wallet.Deposit(totalPrice);
                Log.Info($"[Trader] {seller.Name} sold {actualAmount} scrap for ${totalPrice}. Balance now: ${wallet.Balance}");
                Missions.MissionJournal.Local?.NotifyProgress(Missions.ObjectiveType.SellToTrader, "chatarra", actualAmount);
            }
        }
    }
}
