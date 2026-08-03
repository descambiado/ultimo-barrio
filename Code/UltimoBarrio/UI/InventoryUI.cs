
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
                Panel.Style.Display = DisplayMode.None; // Hidden by default — PlayerHud.ChangeState controls this
                Panel.Style.FlexDirection = FlexDirection.Column;
                Panel.Style.BackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.9f); // Charcoal
                Panel.Style.Padding = 20;
                Panel.Style.PointerEvents = PointerEvents.All;
                // Panel.Style.BorderTop = new Border(Length.Pixels(4), BorderStyle.Solid, new Color(0.97f, 0.97f, 1f)); // Off-white
                // Panel.Style.BorderRadius = 4;
                // Panel.Style.BoxShadow = new BoxShadow(2, 2, 8, 0, new Color(0, 0, 0, 0.8f));

                titleLabel = Panel.AddChild<Label>();
                titleLabel.Text = Title;
                titleLabel.Style.FontSize = 24;
                titleLabel.Style.FontWeight = 800;
                titleLabel.Style.FontColor = new Color(0.97f, 0.97f, 1f); // Off-white
                titleLabel.Style.MarginBottom = 16;
                titleLabel.Style.FontFamily = "Poppins";
                // titleLabel.Style.TextShadow = new TextShadow(1, 1, 2, new Color(0,0,0,0.8f));
                
                inventoryContainer = Panel.AddChild<Panel>();
                inventoryContainer.Style.Display = DisplayMode.Flex;
                inventoryContainer.Style.FlexWrap = Wrap.Wrap;
                // inventoryContainer.Style.Gap = 8;
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
                slotPanel.Style.BackgroundColor = new Color(0.16f, 0.16f, 0.2f, 0.9f); // Night blue tint
                // slotPanel.Style.BorderLeft = new Border(Length.Pixels(3), BorderStyle.Solid, new Color(0f, 0.41f, 0.3f)); // Persiana green
                slotPanel.Style.AlignItems = Align.Center;
                slotPanel.Style.JustifyContent = Justify.Center;
                slotPanel.Style.Cursor = "pointer";
                slotPanel.Style.PointerEvents = PointerEvents.All;
                // slotPanel.Style.BorderRadius = 2;

                var itemId = slot.ItemId; // copy for closure
                var amount = slot.Amount;

                if (!string.IsNullOrEmpty(itemId))
                {
                    slotPanel.AddChild(new Label { Text = $"{itemId}\nx{amount}", Style = { FontSize = 12, FontColor = Color.White, TextAlign = TextAlign.Center, PointerEvents = PointerEvents.None, FontFamily = "Poppins" } });
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

