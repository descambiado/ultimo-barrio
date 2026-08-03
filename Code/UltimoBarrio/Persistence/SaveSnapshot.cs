// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Persistence;

public sealed class SaveSnapshot
{
	public const int CurrentVersion = 1;

	public int SaveVersion { get; set; } = CurrentVersion;

	public string SlotId { get; set; } = "default";

	public long Generation { get; set; }

	public string SavedAtUtc { get; set; } = string.Empty;

	public List<ApartmentSaveData> Apartments { get; set; } = [];
}
