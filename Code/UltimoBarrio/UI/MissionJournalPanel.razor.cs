using Sandbox;
using Sandbox.UI;

namespace UltimoBarrio.UI
{
    public partial class MissionJournalPanel : Panel
    {
        public Missions.MissionJournal Journal { get; set; }
        public bool IsOpen { get; set; }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public override void Tick()
        {
            base.Tick();

            // MissionJournal es un singleton por cliente que fija .Local en su
            // propio OnStart — el orden respecto a PlayerHud.OnStart no está
            // garantizado, así que se refresca aquí en vez de capturarlo una
            // sola vez al crear el panel.
            Journal ??= Missions.MissionJournal.Local;

            StateHasChanged();
        }
    }
}
