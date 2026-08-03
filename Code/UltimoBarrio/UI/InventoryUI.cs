using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace UltimoBarrio.UI
{
    public class InventoryUI : PanelComponent
    {
        [Property] public InventoryComponent TargetInventory { get; set; }
        [Property] public InventoryComponent TransferTarget { get; set; }
        [Property] public string Title { get; set; } = "Inventory";
        
        private Panel inventoryContainer;
        private Label titleLabel;

        protected override void OnStart()
        {
            if (Panel != null)
            {
                Panel.Style.Display = DisplayMode.Flex;
                Panel.Style.FlexDirection = FlexDirection.Column;
                Panel.Style.BackgroundColor = Color.Black.WithAlpha(0.8f);
                Panel.Style.Padding = 20;
                Panel.Style.PointerEvents = PointerEvents.All;

                titleLabel = Panel.AddChild<Label>();
                titleLabel.Text = Title;
                titleLabel.Style.FontSize = 24;
                titleLabel.Style.FontColor = Color.White;
                
                inventoryContainer = Panel.AddChild<Panel>();
                inventoryContainer.Style.Display = DisplayMode.Flex;
                inventoryContainer.Style.FlexWrap = Wrap.Wrap;
            }
        }

        private int _lastHash = 0;

        protected override void OnUpdate()
        {
            if (TargetInventory == null || inventoryContainer == null) return;
            
            titleLabel.Text = Title;

            int currentHash = 0;
            foreach (var slot in TargetInventory.Slots)
            {
                currentHash = System.HashCode.Combine(currentHash, slot.ItemId, slot.Amount);
            }

            if (_lastHash == currentHash && inventoryContainer.ChildrenCount > 0) return;
            _lastHash = currentHash;

            inventoryContainer.DeleteChildren(true);
            foreach (var slot in TargetInventory.Slots)
            {
                var slotPanel = inventoryContainer.AddChild<Panel>();
                slotPanel.Style.Width = 64;
                slotPanel.Style.Height = 64;
                slotPanel.Style.Margin = 4;
                slotPanel.Style.BackgroundColor = Color.Gray.WithAlpha(0.5f);
                slotPanel.Style.AlignItems = Align.Center;
                slotPanel.Style.JustifyContent = Justify.Center;
                slotPanel.Style.Cursor = "pointer";
                slotPanel.Style.PointerEvents = PointerEvents.All;

                var itemId = slot.ItemId; // copy for closure
                var amount = slot.Amount;

                if (!string.IsNullOrEmpty(itemId))
                {
                    slotPanel.AddChild(new Label { Text = $"{itemId}\nx{amount}", Style = { FontSize = 12, FontColor = Color.White, TextAlign = TextAlign.Center, PointerEvents = PointerEvents.None } });
                }

                slotPanel.AddEventListener("onclick", (e) =>
                {
                    if (string.IsNullOrEmpty(itemId) || TransferTarget == null || amount <= 0) return;
                    
                    bool shift = Input.Down("Run"); // Usually Shift
                    int transferAmount = shift ? amount : 1;

                    TargetInventory.RequestTransfer(itemId, transferAmount, TransferTarget.GameObject.Id);
                });
            }
        }
    }
}
