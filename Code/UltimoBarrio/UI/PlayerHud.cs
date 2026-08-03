using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace UltimoBarrio.UI
{
    [Title("Player HUD")]
    [Category("sltimo Barrio")]
    [Icon("desktop_windows")]
    public sealed class PlayerHud : PanelComponent
    {
        private InteractionPromptPanel _promptPanel;
        
        public InventoryUI PlayerInvUI { get; private set; }
        public InventoryUI StashInvUI { get; private set; }

        private bool _isInventoryOpen = false;

        protected override void OnStart()
        {
            if ( IsProxy ) return; // Only for local player

            Panel.Style.Width = Length.Percent(100);
            Panel.Style.Height = Length.Percent(100);
            Panel.Style.Position = PositionMode.Absolute;
            Panel.Style.PointerEvents = PointerEvents.None; // Let clicks pass through if not on a child

            _promptPanel = new InteractionPromptPanel();
            Panel.AddChild( _promptPanel );

            // Dynamically add the inventory UIs
            PlayerInvUI = GameObject.Components.Create<InventoryUI>();
            PlayerInvUI.Title = "Tu Mochila";

            StashInvUI = GameObject.Components.Create<InventoryUI>();
            StashInvUI.Title = "Alijo";
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
                PlayerInvUI.Panel.Style.Display = _isInventoryOpen ? DisplayMode.Flex : DisplayMode.None;
            }

            if (StashInvUI.Panel != null)
            {
                StashInvUI.Panel.Style.Position = PositionMode.Absolute;
                StashInvUI.Panel.Style.Right = Length.Percent(20);
                StashInvUI.Panel.Style.Top = Length.Percent(30);
                StashInvUI.Panel.Style.Width = 400;
                StashInvUI.Panel.Style.PointerEvents = PointerEvents.All;

                // Check distance
                if (StashInvUI.TargetInventory != null)
                {
                    var distance = (Transform.Position - StashInvUI.TargetInventory.Transform.Position).Length;
                    if (distance > 200f) // Threshold slightly larger than interaction range
                    {
                        StashInvUI.TargetInventory = null;
                        PlayerInvUI.TransferTarget = null;
                    }
                }

                // Only show stash if we have a valid target
                bool showStash = _isInventoryOpen && StashInvUI.TargetInventory != null && PlayerInvUI.TransferTarget != null;
                StashInvUI.Panel.Style.Display = showStash ? DisplayMode.Flex : DisplayMode.None;
            }

            // Toggle inventory
            if (Input.Pressed("Score")) // TAB key usually
            {
                _isInventoryOpen = !_isInventoryOpen;
                if (_isInventoryOpen)
                {
                    PlayerInvUI.TargetInventory = GameObject.Components.Get<InventoryComponent>();
                    // When pressing TAB normally, we clear the transfer target so the stash hides
                    PlayerInvUI.TransferTarget = null;
                    StashInvUI.TargetInventory = null;
                }
            }
        }

        public void OpenStash(InventoryComponent stashInv)
        {
            if (IsProxy) return;
            var playerInv = GameObject.Components.Get<InventoryComponent>();
            
            PlayerInvUI.TargetInventory = playerInv;
            PlayerInvUI.TransferTarget = stashInv;

            StashInvUI.TargetInventory = stashInv;
            StashInvUI.TransferTarget = playerInv;

            _isInventoryOpen = true;
        }

        protected override void OnDestroy()
        {
            if ( _promptPanel != null )
            {
                _promptPanel.Delete();
                _promptPanel = null;
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
