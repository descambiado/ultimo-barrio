// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;

namespace UltimoBarrio.Persistence;

public sealed class SaveSnapshot
{
	public const int CurrentVersion = 2;

	public int SaveVersion { get; set; } = CurrentVersion;

	public string SlotId { get; set; } = "default";

	public long Generation { get; set; }

	public string SavedAtUtc { get; set; } = string.Empty;

	public List<ApartmentSaveData> Apartments { get; set; } = [];
	public List<PlayerEconomySaveData> PlayersEconomy { get; set; } = [];
	public List<InventorySaveData> Inventories { get; set; } = [];

	/// <summary>Estado del ciclo del mundo (v2).</summary>
	public ClockSaveData Clock { get; set; }

	/// <summary>Fortificación por apartamento (v2): mejoras, salud de puerta y barricadas.</summary>
	public List<FortificationSaveData> Fortifications { get; set; } = [];

	/// <summary>Misiones activas y progreso (v2).</summary>
	public List<MissionSaveData> Missions { get; set; } = [];

	/// <summary>Selección de hotbar por jugador (v2).</summary>
	public List<PlayerStateSaveData> PlayerStates { get; set; } = [];
}
