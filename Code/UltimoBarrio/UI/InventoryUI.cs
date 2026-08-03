using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace UltimoBarrio.UI
{
    public class InventoryUI : PanelComponent
    {
        [Property] public InventoryComponent TargetInventory { get; set; }
        
        private Panel inventoryContainer;

        protected override void OnStart()
        {
            if (Panel != null)
            {
                Panel.Style.Display = DisplayMode.Flex;
                Panel.Style.FlexDirection = FlexDirection.Column;
                Panel.Style.BackgroundColor = Color.Black.WithAlpha(0.8f);
                Panel.Style.Padding = 20;

                Panel.AddChild(new Label { Text = "Inventory", Style = { FontSize = 24, Color = Color.White } });
                
                inventoryContainer = Panel.AddChild<Panel>();
                inventoryContainer.Style.Display = DisplayMode.Flex;
                inventoryContainer.Style.FlexWrap = Wrap.Wrap;
            }
        }

        protected override void OnUpdate()
        {
            if (TargetInventory == null || inventoryContainer == null) return;
            
            // Rebuild UI simply for now
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

                if (!string.IsNullOrEmpty(slot.ItemId))
                {
                    slotPanel.AddChild(new Label { Text = $"{slot.ItemId}\nx{slot.Amount}", Style = { FontSize = 12, Color = Color.White, TextAlign = TextAlign.Center } });
                }
            }
        }
    }
}
