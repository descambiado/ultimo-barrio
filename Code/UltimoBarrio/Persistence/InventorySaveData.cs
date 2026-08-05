// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;

namespace UltimoBarrio.Persistence;

public sealed class InventorySaveData
{
	public string InventoryId { get; set; } = string.Empty;
	public List<InventorySlotSaveData> Slots { get; set; } = [];
}

public sealed class InventorySlotSaveData
{
	public string ItemId { get; set; } = string.Empty;
	public int Amount { get; set; }

	/// <summary>Cargador del arma apilada en este slot (v2).</summary>
	public int AmmoInMag { get; set; }
}
