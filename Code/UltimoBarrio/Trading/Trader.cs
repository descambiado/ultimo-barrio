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
            // Usually opens UI
            Log.Info($"[Trader] Interaction initiated by {request.InteractorId}");
        }

        [Rpc.Host]
        public void BuyItem(GameObject buyer, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (buyer == null) return;
            
            var wallet = buyer.GetComponent<IWallet>();
            var inventory = buyer.GetComponent<IInventory>();

            if (wallet == null || inventory == null) 
            {
                Log.Info($"[Trader] {buyer.Name} lacks wallet or inventory.");
                return;
            }

            int price = 0;
            if (itemId == "water") price = WaterPrice;
            else if (itemId == "medicine") price = MedicinePrice;
            else if (itemId == "ammo") price = AmmoPrice;
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
                        Log.Info($"[Trader] {buyer.Name} bought {amount} {itemId} for {totalCost}.");
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
                Log.Info($"[Trader] {buyer.Name} cannot afford {totalCost} (Balance: {wallet.Balance}).");
            }
        }

        [Rpc.Host]
        public void SellItem(GameObject seller, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (seller == null) return;

            var wallet = seller.GetComponent<IWallet>();
            var inventory = seller.GetComponent<IInventory>();

            if (wallet == null || inventory == null) return;

            if (itemId != "scrap") 
            {
                Log.Info($"[Trader] Can only sell scrap.");
                return;
            }

            int totalPrice = ScrapSellPrice * amount;

            if (inventory.GetCount(itemId) >= amount)
            {
                if (inventory.TryRemove(itemId, amount))
                {
                    wallet.Deposit(totalPrice);
                    Log.Info($"[Trader] {seller.Name} sold {amount} {itemId} for {totalPrice}.");
                }
            }
            else
            {
                Log.Info($"[Trader] {seller.Name} does not have enough {itemId} to sell.");
            }
        }
    }
}
