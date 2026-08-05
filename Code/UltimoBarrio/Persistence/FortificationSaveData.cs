// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;

namespace UltimoBarrio.Persistence;

public sealed class FortificationSaveData
{
    public string ApartmentId { get; set; } = string.Empty;
    public int UpgradeLevel { get; set; }
    public float DoorHealth { get; set; }
    public float DoorMaxHealth { get; set; }
    public List<BarricadeSaveData> Barricades { get; set; } = [];
    public int SaveVersion { get; set; } = 1;
}

public sealed class BarricadeSaveData
{
    public string AnchorId { get; set; } = string.Empty;
    public float Health { get; set; }
    public float MaxHealth { get; set; }
}
