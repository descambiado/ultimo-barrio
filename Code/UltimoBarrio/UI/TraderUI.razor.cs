
using Sandbox;
using Sandbox.UI;
using System.Linq;
using UltimoBarrio.Trading;
using UltimoBarrio.Economy;
using UltimoBarrio.Core;

namespace UltimoBarrio.UI
{
    public partial class TraderUI : Panel
    {
        public Trader TargetTrader { get; set; }
        public bool IsOpen => TargetTrader != null;
        public GameObject PlayerObj { get; set; }

        public void Open(Trader trader)
        {
            TargetTrader = trader;
        }

        public void Close()
        {
            TargetTrader = null;
        }

        public void Buy(string itemId)
        {
            if (TargetTrader == null || PlayerObj == null) return;
            TargetTrader.BuyItem(PlayerObj, itemId, 1);
        }

        public void Sell(string itemId)
        {
            if (TargetTrader == null || PlayerObj == null) return;
            // Get player inventory to count how many to sell
            var inv = PlayerObj.Components.Get<IInventory>();
            if (inv != null)
            {
                var amount = inv.GetCount(itemId);
                if (amount > 0)
                {
                    TargetTrader.SellItem(PlayerObj, itemId, amount);
                }
            }
        }
        
        public override void Tick()
        {
            base.Tick();
            
            if (IsOpen && TargetTrader != null && PlayerObj != null)
            {
                // Auto close if too far
                var distance = (TargetTrader.WorldPosition - PlayerObj.WorldPosition).Length;
                if (distance > 200f)
                {
                    Close();
                }
            }

            StateHasChanged();
        }
    }
}

