// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;

namespace UltimoBarrio.Persistence;

public sealed class MissionSaveData
{
    public string MissionId { get; set; } = string.Empty;
    public List<ObjectiveSaveData> Objectives { get; set; } = [];
    public int SaveVersion { get; set; } = 1;
}

public sealed class ObjectiveSaveData
{
    public string Id { get; set; } = string.Empty;
    public int Progress { get; set; }
    public bool Completed { get; set; }
}
