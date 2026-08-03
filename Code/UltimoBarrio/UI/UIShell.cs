using Sandbox;
using Sandbox.UI;
using System;

namespace UltimoBarrio.UI
{
    public enum ActiveScreen
    {
        None,
        MainMenu,
        PauseMenu,
        Inventory,
        Stash,
        Trader,
        MissionJournal,
        Map,
        Settings,
        DeathScreen,
        RaidSummary
    }

    [Title("UI Shell")]
    [Category("Último Barrio — UI")]
    [Icon("widgets")]
    public sealed class UIShell : PanelComponent
    {
        public static UIShell Local { get; private set; }

        public ActiveScreen CurrentScreen { get; private set; } = ActiveScreen.None;

        protected override void OnStart()
        {
            if (!IsProxy)
            {
                Local = this;
                if (Panel != null)
                {
                    Panel.Style.Width = Length.Percent(100);
                    Panel.Style.Height = Length.Percent(100);
                    Panel.Style.PointerEvents = PointerEvents.None;
                }
            }
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            // Escape key toggles PauseMenu
            if (Input.Pressed("Menu"))
            {
                if (CurrentScreen == ActiveScreen.None)
                {
                    OpenScreen(ActiveScreen.PauseMenu);
                }
                else if (CurrentScreen == ActiveScreen.PauseMenu)
                {
                    CloseScreen();
                }
            }

            // M key toggles MissionJournal
            if (Input.Pressed("Score") && Input.Down("Walk")) // Shift+TAB or M key
            {
                if (CurrentScreen == ActiveScreen.None)
                {
                    OpenScreen(ActiveScreen.MissionJournal);
                }
                else if (CurrentScreen == ActiveScreen.MissionJournal)
                {
                    CloseScreen();
                }
            }
        }

        public void OpenScreen(ActiveScreen screen)
        {
            if (IsProxy) return;
            CurrentScreen = screen;
            Log.Info($"[UIShell] Transitioned to screen: {screen}");

            if (Panel != null)
            {
                Panel.Style.PointerEvents = (screen == ActiveScreen.None) ? PointerEvents.None : PointerEvents.All;
            }
        }

        public void CloseScreen()
        {
            OpenScreen(ActiveScreen.None);
        }
    }
}
