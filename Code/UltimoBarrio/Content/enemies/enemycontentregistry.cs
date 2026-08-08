using System.Collections.Generic;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Catálogo del enemy content pack (portable): Saqueador, Bruto, Merodeador.
	/// Mismos estados de assets que el pack de armas (PENDING_VERIFY / VERIFIED fallback).
	/// No conecta persistencia ni apartment system de la rama vieja.
	/// </summary>
	public static class EnemyContentRegistry
	{
		private static readonly Dictionary<string, EnemyArchetypeDefinition> _enemies = new();
		private static readonly Dictionary<string, LootTableDefinition> _lootTables = new();

		static EnemyContentRegistry()
		{
			RegisterEnemy( Saqueador() );
			RegisterEnemy( Bruto() );
			RegisterEnemy( Merodeador() );

			RegisterLoot( ChatarraTable() );
			RegisterLoot( BrutoTable() );
			RegisterLoot( MerodeadorTable() );
		}

		public static EnemyArchetypeDefinition GetEnemy( string id )
		{
			return id != null && _enemies.TryGetValue( id, out var def ) ? def : null;
		}

		public static LootTableDefinition GetLootTable( string id )
		{
			return id != null && _lootTables.TryGetValue( id, out var def ) ? def : null;
		}

		public static IEnumerable<EnemyArchetypeDefinition> AllEnemies => _enemies.Values;

		private static void RegisterEnemy( EnemyArchetypeDefinition def ) => _enemies[def.Id] = def;
		private static void RegisterLoot( LootTableDefinition def ) => _lootTables[def.Id] = def;

		private static EnemyArchetypeDefinition Saqueador()
		{
			return new EnemyArchetypeDefinition
			{
				Id = "ub_enemy_saqueador",
				DisplayName = "Saqueador",
				Model = "models/citizen/citizen.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
				Scale = 1f,
				MaxHealth = 100f,
				WalkSpeed = 220f,
				VisionRange = 2000f,
				VisionAngle = 100f,
				HearingRadius = 1400f,
				MemoryDuration = 5f,
				AttackRange = 100f,
				AttackDamage = 15f,
				AttackCooldown = 1.5f,
				TargetPriority = EnemyTargetPriority.Player,
				LootTableId = "loot_chatarra",
				AssetsVerified = false,
				VerificationNotes = "Modelo base ciudadano de Facepunch pendiente de confirmar en Cloud Browser. Fallback verificado."
			};
		}

		private static EnemyArchetypeDefinition Bruto()
		{
			return new EnemyArchetypeDefinition
			{
				Id = "ub_enemy_bruto",
				DisplayName = "Bruto",
				Model = "models/citizen/citizen.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
				Scale = 1.3f,
				MaxHealth = 250f,
				WalkSpeed = 150f,
				VisionRange = 1600f,
				VisionAngle = 90f,
				HearingRadius = 1200f,
				MemoryDuration = 6f,
				AttackRange = 110f,
				AttackDamage = 40f,
				AttackCooldown = 2f,
				TargetPriority = EnemyTargetPriority.Structures,
				StructureTag = "fortification",
				LootTableId = "loot_bruto",
				AssetsVerified = false,
				VerificationNotes = "Misma base ciudadana, escala 1.3. Modelo alternativo (skin Bruto) pendiente de investigación."
			};
		}

		private static EnemyArchetypeDefinition Merodeador()
		{
			return new EnemyArchetypeDefinition
			{
				Id = "ub_enemy_merodeador",
				DisplayName = "Merodeador",
				Model = "models/citizen/citizen.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
				Scale = 0.95f,
				MaxHealth = 60f,
				WalkSpeed = 320f,
				VisionRange = 2400f,
				VisionAngle = 120f,
				HearingRadius = 1600f,
				MemoryDuration = 4f,
				AttackRange = 80f,
				AttackDamage = 10f,
				AttackCooldown = 1f,
				TargetPriority = EnemyTargetPriority.Balanced,
				LootTableId = "loot_merodeador",
				AssetsVerified = false,
				VerificationNotes = "Arquetipo rápido y frágil; escala ligeramente reducida."
			};
		}

		// Los WorldPrefab apuntan a pickups FÍSICOS del pack (prefabs/content/enemies/*).
		// Llevan LootPickupContent (el componente del pack), así el rig puede contar
		// el botín y el core nuevo mapea ItemId → inventario.

		private static LootTableDefinition ChatarraTable()
		{
			return new LootTableDefinition
			{
				Id = "loot_chatarra",
				Entries = new List<LootEntry>
				{
					new LootEntry { ItemId = "chatarra", WorldPrefab = "prefabs/content/enemies/loot_scrap_content.prefab", Min = 1, Max = 3, Chance = 1f }
				}
			};
		}

		private static LootTableDefinition BrutoTable()
		{
			return new LootTableDefinition
			{
				Id = "loot_bruto",
				Entries = new List<LootEntry>
				{
					new LootEntry { ItemId = "chatarra", WorldPrefab = "prefabs/content/enemies/loot_scrap_content.prefab", Min = 2, Max = 5, Chance = 1f },
					new LootEntry { ItemId = "ammo_9mm", WorldPrefab = "prefabs/content/enemies/loot_supplies_content.prefab", Min = 1, Max = 2, Chance = 0.5f }
				}
			};
		}

		private static LootTableDefinition MerodeadorTable()
		{
			return new LootTableDefinition
			{
				Id = "loot_merodeador",
				Entries = new List<LootEntry>
				{
					new LootEntry { ItemId = "chatarra", WorldPrefab = "prefabs/content/enemies/loot_scrap_content.prefab", Min = 1, Max = 2, Chance = 0.8f },
					new LootEntry { ItemId = "ammo_9mm", WorldPrefab = "prefabs/content/enemies/loot_supplies_content.prefab", Min = 1, Max = 1, Chance = 0.3f }
				}
			};
		}
	}
}
