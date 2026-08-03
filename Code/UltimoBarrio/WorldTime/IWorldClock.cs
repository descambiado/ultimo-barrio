using Sandbox;
using System;

namespace UltimoBarrio.WorldTime
{
    public enum TimePhase
    {
        Day,
        Preparation,
        Night,
        Aftermath
    }

    public interface IWorldClock
    {
        TimePhase CurrentPhase { get; }
        float TimeRemainingInPhase { get; }
        event Action<TimePhase> OnPhaseChanged;
    }
}
