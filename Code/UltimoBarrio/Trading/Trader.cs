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
            else if (itemId == "ammo_buckshot") { price = AmmoPrice; }
            else if (itemId == "weapon_usp") { price = 100; }
            else 
            {
                Log.Info($"[Trader] Invalid item: {itemId}");
                return;
            }

            var result = TradeTransactionService.TryBuy( inventory, wallet, itemId, amount, price );
            if ( result.Succeeded )
            {
                Log.Info($"[Trader] {buyer.Name} bought {result.Amount} {result.ItemId} for {result.Value}. Balance now: ${wallet.Balance}");
                Missions.MissionJournal.Local?.NotifyProgress(Missions.ObjectiveType.BuyItem, result.ItemId, result.Amount);
                return;
            }

            Log.Info($"[Trader] Buy rejected ({result.Code}) for {buyer.Name}: {itemId} x{amount}.");
        }

        [Rpc.Host]
        public void SellItem(GameObject seller, string itemId, int amount = 1)
        {
            if (!Networking.IsHost) return;
            if (seller == null) return;

            var wallet = seller.Components.Get<IWallet>();
            var inventory = seller.Components.Get<IInventory>();

            if (wallet == null || inventory == null) return;

            var targetItem = itemId == "scrap" ? "scrap" : itemId == "chatarra" ? "chatarra" : string.Empty;
			if ( string.IsNullOrEmpty( targetItem ) )
            {
                Log.Info($"[Trader] Invalid sell item: {itemId}");
                return;
            }

            var result = TradeTransactionService.TrySell( inventory, wallet, targetItem, amount, ScrapSellPrice );
            if ( result.Succeeded )
            {
                Log.Info($"[Trader] {seller.Name} sold {result.Amount} {result.ItemId} for ${result.Value}. Balance now: ${wallet.Balance}");
                Missions.MissionJournal.Local?.NotifyProgress(Missions.ObjectiveType.SellToTrader, "chatarra", result.Amount);
                return;
            }

            Log.Info($"[Trader] Sell rejected ({result.Code}) for {seller.Name}: {targetItem} x{amount}.");
        }
    }
}
