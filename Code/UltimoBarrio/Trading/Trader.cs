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
        }

        [Rpc.Owner]
        public void BuyItem(GameObject buyer, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (buyer == null) return;
            
            var wallet = buyer.GetComponent<Wallet>();
            var inventory = buyer.GetComponent<IInventory>();

            if (wallet == null || inventory == null) return;

            int price = 0;
            if (itemId == "water") price = WaterPrice;
            else if (itemId == "medicine") price = MedicinePrice;
            else if (itemId == "ammo") price = AmmoPrice;
            else return;

            int totalCost = price * amount;
            if (wallet.Balance >= totalCost)
            {
                if (inventory.TryAdd(itemId, amount))
                {
                    wallet.TryRemoveFunds(totalCost);
                }
            }
        }

        [Rpc.Host]
        public void SellItem(GameObject seller, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (seller == null) return;

            var wallet = seller.GetComponent<Wallet>();
            var inventory = seller.GetComponent<IInventory>();

            if (wallet == null || inventory == null) return;

            if (itemId != "scrap") return;

            int totalPrice = ScrapSellPrice * amount;

            if (inventory.TryRemove(itemId, amount))
            {
                wallet.AddFunds(totalPrice);
            }
        }
    }
}
