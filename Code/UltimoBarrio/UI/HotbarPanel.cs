using Sandbox;
using Sandbox.UI;

namespace UltimoBarrio.UI
{
    public class HotbarPanel : PanelComponent
    {
        [Property] public InventoryComponent TargetInventory { get; set; }
        [Property] public Combat.HeldItemController HeldItemCtrl { get; set; }
        
        /// <summary>
        /// Fallback: si no hay HeldItemController (viejo), usar UbWeaponCarrier (nuevo).
        /// </summary>
        private Combat.UbWeaponCarrier _weaponCarrier;
        
        private Panel _container;

        protected override void OnStart()
        {
            _weaponCarrier = Components.Get<Combat.UbWeaponCarrier>();
            if (Panel != null)
            {
                Panel.Style.Position = PositionMode.Absolute;
                Panel.Style.Bottom = 20;
                Panel.Style.Left = Length.Percent(50);
                Panel.Style.Width = Length.Percent(100);
                // Center transform is not supported directly in this simple CSS engine sometimes, so flex box it:
                Panel.Style.Left = 0;
                Panel.Style.Display = DisplayMode.Flex;
                Panel.Style.JustifyContent = Justify.Center;
                Panel.Style.PointerEvents = PointerEvents.None;

                _container = Panel.AddChild<Panel>();
                _container.Style.Display = DisplayMode.Flex;
                _container.Style.FlexDirection = FlexDirection.Row;
                // _container.Style.Gap = 8;
                _container.Style.PointerEvents = PointerEvents.All;
            }
        }

        private int _lastHash = 0;
        private int _lastSelected = -1;

        protected override void OnUpdate()
        {
            if ( TargetInventory == null || _container == null ) return;
            
            int activeSlot = HeldItemCtrl != null ? HeldItemCtrl.SelectedHotbarSlot
                          : _weaponCarrier != null ? _weaponCarrier.SelectedSlot : -1;
            
            int currentHash = System.HashCode.Combine(activeSlot, TargetInventory.HotbarSlots);
            for(int i = 0; i < TargetInventory.HotbarSlots; i++)
            {
                if (i < TargetInventory.Slots.Count)
                {
                    var s = TargetInventory.Slots[i];
                    currentHash = System.HashCode.Combine(currentHash, s.ItemId, s.Amount);
                }
            }

            if (_lastHash == currentHash && _container.ChildrenCount > 0) return;
            _lastHash = currentHash;
            _lastSelected = activeSlot;

            _container.DeleteChildren(true);
            
            for (int i = 0; i < TargetInventory.HotbarSlots; i++)
            {
                var slotPanel = _container.AddChild<Panel>();
                slotPanel.Style.Width = 64;
                slotPanel.Style.Height = 64;
                
                bool isSelected = (i == activeSlot);
                slotPanel.Style.BackgroundColor = isSelected ? new Color(0.1f, 0.6f, 0.3f, 0.9f) : new Color(0.12f, 0.12f, 0.12f, 0.8f);
                // slotPanel.Style.Border = new Border(Length.Pixels(2), BorderStyle.Solid, isSelected ? Color.White : new Color(0.3f, 0.3f, 0.3f, 1f));
                
                slotPanel.Style.AlignItems = Align.Center;
                slotPanel.Style.JustifyContent = Justify.Center;
                slotPanel.Style.FlexDirection = FlexDirection.Column;
                
                var numberLbl = slotPanel.AddChild<Label>();
                numberLbl.Text = (i + 1).ToString();
                numberLbl.Style.Position = PositionMode.Absolute;
                numberLbl.Style.Top = 2;
                numberLbl.Style.Left = 4;
                numberLbl.Style.FontSize = 10;
                numberLbl.Style.FontColor = Color.White;

                if (i < TargetInventory.Slots.Count)
                {
                    var slot = TargetInventory.Slots[i];
                    if (!string.IsNullOrEmpty(slot.ItemId) && slot.Amount > 0)
                    {
                        var def = ItemRegistry.GetDefinition(slot.ItemId);
                        string displayName = def != null ? def.DisplayName : slot.ItemId;
                        
                        var nameLbl = slotPanel.AddChild<Label>();
                        nameLbl.Text = displayName;
                        nameLbl.Style.FontSize = 10;
                        nameLbl.Style.FontColor = Color.White;
                        nameLbl.Style.TextAlign = TextAlign.Center;
                        
                        var amtLbl = slotPanel.AddChild<Label>();
                        amtLbl.Text = $"x{slot.Amount}";
                        amtLbl.Style.FontSize = 12;
                        amtLbl.Style.FontColor = new Color(0.8f, 0.8f, 0.8f);
                        amtLbl.Style.FontWeight = 800;
                    }
                }
            }
        }
    }
}
