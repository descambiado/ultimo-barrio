using Sandbox;
using Sandbox.UI;
using System.Linq;
using UltimoBarrio;

namespace UltimoBarrio.UI
{
    public class HotbarPanel : PanelComponent
    {
        [Property] public InventoryComponent TargetInventory { get; set; }
        [Property] public HeldItemController HeldItemCtrl { get; set; }

        /// <summary>
        /// Fallback: si no hay HeldItemController (viejo), usar UbWeaponCarrier (nuevo,
        /// es el sistema de armas real en el player.prefab actual).
        /// </summary>
        private Combat.UbWeaponCarrier _weaponCarrier;

        private Panel _container;

        protected override void OnStart()
        {
            _weaponCarrier = Components.Get<Combat.UbWeaponCarrier>();
            if ( Panel != null )
            {
                Panel.Style.Position = PositionMode.Absolute;
                Panel.Style.Bottom = 20;
                Panel.Style.Left = 0;
                Panel.Style.Right = 0;
                Panel.Style.Display = DisplayMode.Flex;
                Panel.Style.JustifyContent = Justify.Center;
                Panel.Style.PointerEvents = PointerEvents.None;

                _container = Panel.AddChild<Panel>();
                _container.Style.Display = DisplayMode.Flex;
                _container.Style.FlexDirection = FlexDirection.Row;
                _container.Style.PointerEvents = PointerEvents.All;
            }
        }

        private int _lastHash = 0;

        protected override void OnUpdate()
        {
            if ( TargetInventory == null || _container == null ) return;

            int activeSlot = HeldItemCtrl != null ? HeldItemCtrl.SelectedHotbarSlot
                          : _weaponCarrier != null ? _weaponCarrier.SelectedSlot : -1;

            // Munición del arma activa (cargador + recarga) — solo disponible con el
            // sistema de armas viejo (HeldItemCtrl); UbWeaponCarrier no expone esto aquí.
            var activeWeapon = HeldItemCtrl != null ? ResolveActiveWeapon() : null;
            int mag = activeWeapon?.CurrentAmmo ?? 0;
            int maxMag = activeWeapon?.MaxAmmo ?? 0;
            bool reloading = activeWeapon?.IsReloading ?? false;

            int currentHash = System.HashCode.Combine( activeSlot, TargetInventory.HotbarSlots, mag, maxMag, reloading );
            for ( int i = 0; i < TargetInventory.HotbarSlots; i++ )
            {
                if ( i < TargetInventory.Slots.Count )
                {
                    var s = TargetInventory.Slots[i];
                    currentHash = System.HashCode.Combine( currentHash, s.ItemId, s.Amount, s.AmmoInMag );
                }
            }

            if ( _lastHash == currentHash && _container.ChildrenCount > 0 )
                return;

            _lastHash = currentHash;

            _container.DeleteChildren( true );

            for ( int i = 0; i < TargetInventory.HotbarSlots; i++ )
            {
                var slotPanel = _container.AddChild<Panel>();
                slotPanel.Style.Width = 64;
                slotPanel.Style.Height = 64;

                bool isSelected = ( i == activeSlot );
                slotPanel.Style.BackgroundColor = isSelected
                    ? new Color( 0.1f, 0.6f, 0.3f, 0.95f )
                    : new Color( 0.12f, 0.12f, 0.12f, 0.8f );

                slotPanel.Style.AlignItems = Align.Center;
                slotPanel.Style.JustifyContent = Justify.Center;
                slotPanel.Style.FlexDirection = FlexDirection.Column;
                slotPanel.Style.PointerEvents = PointerEvents.All;
                slotPanel.Style.Cursor = "pointer";

                var slotIndex = i;
                slotPanel.AddEventListener( "onclick", () =>
                {
                    if ( HeldItemCtrl != null )
                        HeldItemCtrl.SelectSlot( slotIndex );
                    else
                        _weaponCarrier?.SelectSlot( slotIndex );
                } );

                var numberLbl = slotPanel.AddChild<Label>();
                numberLbl.Text = ( i + 1 ).ToString();
                numberLbl.Style.Position = PositionMode.Absolute;
                numberLbl.Style.Top = 2;
                numberLbl.Style.Left = 4;
                numberLbl.Style.FontSize = 10;
                numberLbl.Style.FontColor = Color.White;

                if ( i < TargetInventory.Slots.Count )
                {
                    var slot = TargetInventory.Slots[i];
                    if ( !string.IsNullOrEmpty( slot.ItemId ) && slot.Amount > 0 )
                    {
                        var def = ItemRegistry.GetDefinition( slot.ItemId );
                        string displayName = def != null ? def.DisplayName : slot.ItemId;

                        // Icono provisional: inicial coloreada por categoría.
                        var iconLbl = slotPanel.AddChild<Label>();
                        iconLbl.Text = !string.IsNullOrEmpty( displayName ) ? displayName[..1].ToUpper() : "?";
                        iconLbl.Style.FontSize = 22;
                        iconLbl.Style.FontColor = CategoryColor( def );
                        iconLbl.Style.FontWeight = 800;

                        var nameLbl = slotPanel.AddChild<Label>();
                        nameLbl.Text = displayName;
                        nameLbl.Style.FontSize = 9;
                        nameLbl.Style.FontColor = Color.White;
                        nameLbl.Style.TextAlign = TextAlign.Center;

                        var amtLbl = slotPanel.AddChild<Label>();
                        amtLbl.Text = $"x{slot.Amount}";
                        amtLbl.Style.FontSize = 12;
                        amtLbl.Style.FontColor = new Color( 0.8f, 0.8f, 0.8f );
                        amtLbl.Style.FontWeight = 800;

                        // Cargador del arma en este slot (persistente).
                        if ( def is not null && def.IsWeapon && def.MagazineSize > 0 )
                        {
                            var magLbl = slotPanel.AddChild<Label>();
                            var displayedAmmo = slot.AmmoInMag >= 0 ? slot.AmmoInMag : def.MagazineSize;
                            magLbl.Text = $"{displayedAmmo}/{def.MagazineSize}";
                            magLbl.Style.FontSize = 10;
                            magLbl.Style.FontColor = displayedAmmo == 0 ? new Color( 1f, 0.3f, 0.3f ) : new Color( 0.9f, 0.9f, 0.5f );
                            magLbl.Style.FontWeight = 700;
                        }
                    }
                }
            }

            // Indicador de munición/recarga del arma activa (HUD inferior).
            if ( activeWeapon is not null && ( mag > 0 || reloading ) )
            {
                var statusLabel = _container.AddChild<Label>();
                statusLabel.Text = reloading
                    ? "RECARGANDO..."
                    : $"CARGA: {mag}/{maxMag}";
                statusLabel.Style.FontSize = 14;
                statusLabel.Style.FontColor = reloading ? new Color( 1f, 0.7f, 0.2f ) : Color.White;
                statusLabel.Style.FontWeight = 800;
                statusLabel.Style.Padding = 6;
            }
        }

        private Combat.BaseCombatWeapon ResolveActiveWeapon()
        {
            if ( HeldItemCtrl.ActiveWeaponId == System.Guid.Empty )
                return null;

            var weaponObj = HeldItemCtrl.Scene?.Directory.FindByGuid( HeldItemCtrl.ActiveWeaponId );
            if ( weaponObj == null )
                return null;

            return weaponObj.Components.GetInDescendantsOrSelf<Combat.BaseCombatWeapon>();
        }

        private static Color CategoryColor( ItemDefinition definition )
        {
            if ( definition is null )
                return Color.Gray;

            return definition.Category switch
            {
                ItemCategory.Firearm => new Color( 0.9f, 0.4f, 0.3f ),
                ItemCategory.Melee => new Color( 0.7f, 0.7f, 0.9f ),
                ItemCategory.Consumable => new Color( 0.3f, 0.9f, 0.4f ),
                ItemCategory.Ammo => new Color( 0.9f, 0.8f, 0.3f ),
                ItemCategory.Resource => new Color( 0.7f, 0.5f, 0.3f ),
                _ => Color.White
            };
        }
    }
}
