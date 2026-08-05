using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using UltimoBarrio;

namespace UltimoBarrio.UI
{
    public enum HudState
    {
        Gameplay,
        InventoryOnly,
        InventoryAndStash,
        Trader,
        Crafting,
        Dead
    }

    [Title("Player HUD")]
    [Category("Último Barrio")]
    [Icon("desktop_windows")]
    public sealed class PlayerHud : PanelComponent
    {
        private InteractionPromptPanel _promptPanel;
        private HudOverlayPanel _hudOverlay;
        private TraderUI _traderUI;
        
        public InventoryUI PlayerInvUI { get; private set; }
        public InventoryUI StashInvUI { get; private set; }
        public HotbarPanel Hotbar { get; private set; }
        public CraftingPanel CraftingUI { get; private set; }

        public HudState CurrentState { get; private set; } = HudState.Gameplay;

        protected override void OnStart()
        {
            if ( IsProxy ) return; // Only for local player

            Panel.Style.Width = Length.Percent(100);
            Panel.Style.Height = Length.Percent(100);
            Panel.Style.Position = PositionMode.Absolute;
            Panel.Style.PointerEvents = PointerEvents.None; // Let clicks pass through if not on a child

            _promptPanel = new InteractionPromptPanel();
            Panel.AddChild( _promptPanel );

            _hudOverlay = new HudOverlayPanel();
            _hudOverlay.PlayerObj = GameObject;
            Panel.AddChild( _hudOverlay );

            _traderUI = new TraderUI();
            _traderUI.PlayerObj = GameObject;
            Panel.AddChild( _traderUI );

            // Dynamically add the inventory UIs
            PlayerInvUI = GameObject.Components.Create<InventoryUI>();
            PlayerInvUI.Title = "Tu Mochila";

            StashInvUI = GameObject.Components.Create<InventoryUI>();
            StashInvUI.Title = "Alijo";
            
            Hotbar = GameObject.Components.Create<HotbarPanel>();
            Hotbar.TargetInventory = GameObject.Components.Get<InventoryComponent>();
            Hotbar.HeldItemCtrl = GameObject.Components.Get<HeldItemController>();

            CraftingUI = GameObject.Components.Create<CraftingPanel>();
            
            ChangeState(HudState.Gameplay);
        }

        public void ChangeState(HudState newState)
        {
            if (IsProxy) return;
            CurrentState = newState;
            
            // Toggle cursor
            bool showCursor = (newState != HudState.Gameplay && newState != HudState.Dead);
            // We can't directly lock mouse in UI easily without Scene.Camera or Input, 
            // but we can set panel pointer events
            Panel.Style.PointerEvents = showCursor ? PointerEvents.All : PointerEvents.None;
            
            if (showCursor)
            {
                Panel.Style.PointerEvents = PointerEvents.All;
            }
            
            // Manage UI visibility based on state
            if (PlayerInvUI.Panel != null)
                PlayerInvUI.Panel.Style.Display = (newState == HudState.InventoryOnly || newState == HudState.InventoryAndStash || newState == HudState.Trader) ? DisplayMode.Flex : DisplayMode.None;
                
            if (StashInvUI.Panel != null)
                StashInvUI.Panel.Style.Display = (newState == HudState.InventoryAndStash) ? DisplayMode.Flex : DisplayMode.None;
                
            if (_traderUI != null)
                _traderUI.Style.Display = (newState == HudState.Trader) ? DisplayMode.Flex : DisplayMode.None;
                
            if (CraftingUI != null && CraftingUI.Panel != null)
                CraftingUI.Panel.Style.Display = (newState == HudState.Crafting) ? DisplayMode.Flex : DisplayMode.None;
                
            if (Hotbar != null && Hotbar.Panel != null)
                Hotbar.Panel.Style.Display = (newState == HudState.Gameplay || newState == HudState.InventoryOnly || newState == HudState.InventoryAndStash) ? DisplayMode.Flex : DisplayMode.None;
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            // Apply styles if panels exist
            if (PlayerInvUI.Panel != null)
            {
                PlayerInvUI.Panel.Style.Position = PositionMode.Absolute;
                PlayerInvUI.Panel.Style.Left = Length.Percent(20);
                PlayerInvUI.Panel.Style.Top = Length.Percent(30);
                PlayerInvUI.Panel.Style.Width = 400;
                PlayerInvUI.Panel.Style.PointerEvents = PointerEvents.All;
            }

            if (StashInvUI.Panel != null)
            {
                StashInvUI.Panel.Style.Position = PositionMode.Absolute;
                StashInvUI.Panel.Style.Right = Length.Percent(20);
                StashInvUI.Panel.Style.Top = Length.Percent(30);
                StashInvUI.Panel.Style.Width = 400;
                StashInvUI.Panel.Style.PointerEvents = PointerEvents.All;
            }

            // Check distance for Stash and Trader
            if (CurrentState == HudState.InventoryAndStash && StashInvUI.TargetInventory != null)
            {
                var distance = (WorldPosition - StashInvUI.TargetInventory.WorldPosition).Length;
                if (distance > 200f) // Threshold slightly larger than interaction range
                {
                    StashInvUI.TargetInventory = null;
                    PlayerInvUI.TransferTarget = null;
                    ChangeState(HudState.Gameplay);
                }
            }
            
            if (CurrentState == HudState.Trader && _traderUI.TargetTrader != null)
            {
                var distance = (WorldPosition - _traderUI.TargetTrader.WorldPosition).Length;
                if (distance > 200f)
                {
                    ChangeState(HudState.Gameplay);
                }
            }

            // Distancia para la estación de crafteo.
            if (CurrentState == HudState.Crafting && CraftingUI.TargetStation != null)
            {
                var distance = (WorldPosition - CraftingUI.TargetStation.WorldPosition).Length;
                if (distance > 200f)
                {
                    CraftingUI.Close();
                    ChangeState(HudState.Gameplay);
                }
            }

            // Toggle inventory
            if (Input.Pressed("Score")) // TAB key usually
            {
                if (CurrentState == HudState.Gameplay)
                {
                    PlayerInvUI.TargetInventory = GameObject.Components.Get<InventoryComponent>();
                    PlayerInvUI.TransferTarget = null;
                    StashInvUI.TargetInventory = null;
                    ChangeState(HudState.InventoryOnly);
                }
                else
                {
                    ChangeState(HudState.Gameplay);
                }
            }
        }

        public void OpenTrader(UltimoBarrio.Trading.Trader trader)
        {
            if (IsProxy) return;
            _traderUI?.Open(trader);
            
            var playerInv = GameObject.Components.Get<InventoryComponent>();
            PlayerInvUI.TargetInventory = playerInv;
            PlayerInvUI.TransferTarget = null;
            
            ChangeState(HudState.Trader);
        }

        public void OpenStash(InventoryComponent stashInv)
        {
            if (IsProxy) return;
            var playerInv = GameObject.Components.Get<InventoryComponent>();
            
            PlayerInvUI.TargetInventory = playerInv;
            PlayerInvUI.TransferTarget = stashInv;

            StashInvUI.TargetInventory = stashInv;
            StashInvUI.TransferTarget = playerInv;

            ChangeState(HudState.InventoryAndStash);
        }

        public void OpenCrafting(Crafting.CraftingStation station)
        {
            if (IsProxy || station == null) return;

            CraftingUI.Open(station, GameObject.Components.Get<InventoryComponent>());
            ChangeState(HudState.Crafting);
        }

        protected override void OnDestroy()
        {
            if ( _promptPanel != null )
            {
                _promptPanel.Delete();
                _promptPanel = null;
            }
            if ( _hudOverlay != null )
            {
                _hudOverlay.Delete();
                _hudOverlay = null;
            }
            if ( _traderUI != null )
            {
                _traderUI.Delete();
                _traderUI = null;
            }
            if ( PlayerInvUI != null )
            {
                PlayerInvUI.Destroy();
                PlayerInvUI = null;
            }
            if ( StashInvUI != null )
            {
                StashInvUI.Destroy();
                StashInvUI = null;
            }
            if ( Hotbar != null )
            {
                Hotbar.Destroy();
                Hotbar = null;
            }
            if ( CraftingUI != null )
            {
                CraftingUI.Destroy();
                CraftingUI = null;
            }
        }

        public void ShowPrompt( string title, string subtitle = "" )
        {
            if ( IsProxy || _promptPanel == null ) return;
            _promptPanel.Show( title, subtitle );
        }

        public void HidePrompt()
        {
            if ( IsProxy || _promptPanel == null ) return;
            _promptPanel.Hide();
        }

        public void ShowMessage( string message )
        {
            if ( IsProxy || _promptPanel == null ) return;
            _promptPanel.Show( message, "" );
            
            var hideTask = async () =>
            {
                await GameTask.DelayRealtimeSeconds( 3f );
                if ( _promptPanel != null && _promptPanel.TitleText == message )
                    _promptPanel.Hide();
            };
            _ = hideTask();
        }
    }
}
