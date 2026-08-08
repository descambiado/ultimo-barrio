using System.Collections.Generic;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Tabla de botín data-driven. Los WorldPrefab apuntan a pickups FÍSICOS del pack
	/// (p. ej. prefabs/content/enemies/loot_scrap_content.prefab); el core nuevo
	/// mapeará ItemId a inventario, pero el pack nunca lo toca directamente.
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
