using Sandbox;
using System;

namespace UltimoBarrio.WorldTime
{
    public class WorldClock : Component, IWorldClock
    {
        [Property] public float DayDuration { get; set; } = 300f; // 5m
        [Property] public float PreparationDuration { get; set; } = 60f; // 1m
        [Property] public float NightDuration { get; set; } = 180f; // 3m
        [Property] public float AftermathDuration { get; set; } = 30f; // 30s

        [Sync] public TimePhase CurrentPhase { get; private set; } = TimePhase.Day;
        [Sync] public float TimeRemainingInPhase { get; private set; }
        public event Action<TimePhase> OnPhaseChanged;

        protected override void OnStart()
        {
            if (!IsProxy) SetPhase(TimePhase.Day);
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            TimeRemainingInPhase -= Time.Delta;

            if (TimeRemainingInPhase <= 0)
            {
                AdvancePhase();
            }
        }

        private void AdvancePhase()
        {
            switch (CurrentPhase)
            {
                case TimePhase.Day:
                    SetPhase(TimePhase.Preparation);
                    break;
                case TimePhase.Preparation:
                    SetPhase(TimePhase.Night);
                    break;
                case TimePhase.Night:
                    SetPhase(TimePhase.Aftermath);
                    // El jugador sobrevivió a la noche (transición Night → Aftermath).
                    Missions.MissionJournal.Local?.NotifyProgress( Missions.ObjectiveType.SurviveNight, "", 1 );
                    break;
                case TimePhase.Aftermath:
                    SetPhase(TimePhase.Day);
                    break;
            }
        }

        public void ForcePhase(TimePhase phase) { if(!IsProxy) SetPhase(phase); }
        private void SetPhase(TimePhase newPhase)
        {
            CurrentPhase = newPhase;

            switch (newPhase)
            {
                case TimePhase.Day:
                    TimeRemainingInPhase = DayDuration;
                    break;
                case TimePhase.Preparation:
                    TimeRemainingInPhase = PreparationDuration;
                    break;
                case TimePhase.Night:
                    TimeRemainingInPhase = NightDuration;
                    break;
                case TimePhase.Aftermath:
                    TimeRemainingInPhase = AftermathDuration;
                    break;
            }

            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }
}
