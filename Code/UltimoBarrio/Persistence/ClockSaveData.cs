// SPDX-License-Identifier: MPL-2.0

using UltimoBarrio.WorldTime;

namespace UltimoBarrio.Persistence;

public sealed class ClockSaveData
{
    public TimePhase Phase { get; set; } = TimePhase.Day;
    public float RemainingSeconds { get; set; }
    public float LightLevel { get; set; } = 1f;
    public int SaveVersion { get; set; } = 1;
}
