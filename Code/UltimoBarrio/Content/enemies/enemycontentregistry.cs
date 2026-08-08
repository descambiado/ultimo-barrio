using System.Collections.Generic;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Catálogo del enemy content pack (portable): Saqueador, Bruto, Merodeador.
	/// Mismos estados de assets que el pack de armas (PENDING_VERIFY / VERIFIED fallback).
	/// No conecta persistencia, apartamentos ni raids del core viejo.
	/// </summary>
	public static class EnemyContentRegistry
	{
		private static readonly Dictionary<string, EnemyContentDefinition> _enemies = new();
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

		public static EnemyContentDefinition GetEnemy( string id )
		{
			if ( string.IsNullOrEmpty( id ) ) return null;
			return _enemies.TryGetValue( id, out var def ) ? def : null;
		}

		public static LootTableDefinition GetLootTable( string id )
		{
			if ( string.IsNullOrEmpty( id ) ) return null;
			return _lootTables.TryGetValue( id, out var def ) ? def : null;
		}

		public static IEnumerable<EnemyContentDefinition> AllEnemies => _enemies.Values;

		private static void RegisterEnemy( EnemyContentDefinition def ) => _enemies[def.Id] = def;
		private static void RegisterLoot( LootTableDefinition def ) => _lootTables[def.Id] = def;

		/// <summary>Saqueador: rápido, vida media-baja, prioriza al jugador.</summary>
		private static EnemyContentDefinition Saqueador()
		{
			return new EnemyContentDefinition
			{
				Id = "ub_enemy_saqueador",
				DisplayName = "Saqueador",
				Model = "models/citizen/citizen.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
				Scale = 1f,
				MaxHealth = 100f,
				WalkSpeed = 260f,
				VisionRange = 2000f,
				VisionAngle = 100f,
				HearingRadius = 1400f,
				MemoryDuration = 5f,
				AttackRange = 110f,
				AttackDamage = 15f,
				AttackCooldown = 1.2f,
				TargetPriority = EnemyTargetPriority.Player,
				StructureTag = "fortification",
				LootTableId = "loot_chatarra",
				AssetsVerified = false,
				VerificationNotes = "Modelo base ciudadano de Facepunch pendiente de confirmar en Cloud Browser. Fallback verificado."
			};
		}

		/// <summary>Bruto: lento, mucha vida, alto daño estructural (prioriza fortificaciones).</summary>
		private static EnemyContentDefinition Bruto()
		{
			return new EnemyContentDefinition
			{
				Id = "ub_enemy_bruto",
				DisplayName = "Bruto",
				Model = "models/citizen/citizen.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
				Scale = 1.3f,
				MaxHealth = 300f,
				WalkSpeed = 130f,
				VisionRange = 1600f,
				VisionAngle = 80f,
				HearingRadius = 1200f,
				MemoryDuration = 6f,
				AttackRange = 130f,
				AttackDamage = 40f,
				AttackCooldown = 2.2f,
				TargetPriority = EnemyTargetPriority.Structures,
				StructureTag = "fortification",
				LootTableId = "loot_bruto",
				AssetsVerified = false,
				VerificationNotes = "Misma base ciudadana, escala 1.3. Modelo alternativo (skin Bruto) pendiente de investigación."
			};
		}

		/// <summary>Merodeador: movilidad media-alta, percepción superior, busca entradas vulnerables.</summary>
		private static EnemyContentDefinition Merodeador()
		{
			return new EnemyContentDefinition
			{
				Id = "ub_enemy_merodeador",
				DisplayName = "Merodeador",
				Model = "models/citizen/citizen.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
				Scale = 0.95f,
				MaxHealth = 60f,
				WalkSpeed = 300f,
				VisionRange = 2600f,
				VisionAngle = 140f,
				HearingRadius = 2000f,
				MemoryDuration = 4f,
				AttackRange = 90f,
				AttackDamage = 10f,
				AttackCooldown = 0.9f,
				TargetPriority = EnemyTargetPriority.Balanced,
				StructureTag = "fortification",
				LootTableId = "loot_merodeador",
				AssetsVerified = false,
				VerificationNotes = "Arquetipo rápido y frágil; visión/ángulo y oído superiores (percepción superior)."
			};
		}

		/// <summary>
		/// Tablas de loot: WorldPrefab apuntan a pickups FÍSICOS del pack
		/// (prefabs/content/enemies/). Los ItemId son strings opacos: el mapeo a
		/// inventario lo decide el core nuevo.
		/// </summary>
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
					new LootEntry { ItemId = "chatarra", WorldPrefab = "prefabs/content/enemies/loot_scrap_content.prefab", Min = 2, Max = 4, Chance = 1f },
					new LootEntry { ItemId = "suministros", WorldPrefab = "prefabs/content/enemies/loot_supplies_content.prefab", Min = 1, Max = 2, Chance = 0.6f }
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
					new LootEntry { ItemId = "chatarra", WorldPrefab = "prefabs/content/enemies/loot_scrap_content.prefab", Min = 1, Max = 2, Chance = 1f },
					new LootEntry { ItemId = "suministros", WorldPrefab = "prefabs/content/enemies/loot_supplies_content.prefab", Min = 1, Max = 1, Chance = 0.25f }
				}
			};
		}
	}
}
