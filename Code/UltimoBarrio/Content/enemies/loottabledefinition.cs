using System.Collections.Generic;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Tabla de botín data-driven. Los WorldPrefab apuntan a prefabs de pickup
	/// (p. ej. prefabs/items/pf_scrap_pickup.prefab) que el core nuevo resolverá.
	/// Los ItemId son strings opacos: el mapeo a inventario lo decide el core nuevo.
	/// </summary>
	public sealed class LootTableDefinition
	{
		public string Id { get; set; } = "";
		public List<LootEntry> Entries { get; set; } = new();
	}

	public sealed class LootEntry
	{
		public string ItemId { get; set; } = "";
		public string WorldPrefab { get; set; } = "";
		public int Min { get; set; } = 1;
		public int Max { get; set; } = 1;
		public float Chance { get; set; } = 1f; // 0..1
	}
}
