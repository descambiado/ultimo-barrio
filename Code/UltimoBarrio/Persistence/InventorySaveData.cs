// SPDX-License-Identifier: MPL-2.0

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
}
