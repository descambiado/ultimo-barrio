using System.Collections.Generic;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>
	/// Catálogo del fortification content pack (portable).
	/// Objetivo: dejar atrás cubos y placeholders con modelos/prefabs reales.
	/// Mismos estados de assets que el resto de packs (PENDING_VERIFY / VERIFIED fallback).
	/// </summary>
	public static class FortificationContentRegistry
	{
		private static readonly Dictionary<string, FortificationContentDefinition> _definitions = new();

		static FortificationContentRegistry()
		{
			Register( BarricadeWood() );
			Register( BarricadeReinforced() );
			Register( DoorBasic() );
			Register( DoorReinforced() );
			Register( Stash() );
			Register( Workbench() );
			Register( Generator() );
			Register( Alarm() );
			Register( RepairStation() );
		}

		public static FortificationContentDefinition Get( string id )
		{
			return id != null && _definitions.TryGetValue( id, out var def ) ? def : null;
		}

		public static IEnumerable<FortificationContentDefinition> All => _definitions.Values;

		private static void Register( FortificationContentDefinition def ) => _definitions[def.Id] = def;

		private static FortificationContentDefinition BarricadeWood()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_barricade_wood",
				DisplayName = "Barricada de madera",
				Type = FortificationContentType.Barricade,
				Model = "models/sbox_props/wooden_barricade/wooden_barricade.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 150f,
				RepairAmount = 25f,
				RepairCost = 5,
				UpgradePrefab = "prefabs/content/fortification/fort_barricade_reinforced.prefab",
				SnapType = FortificationSnapType.Any,
				BuildSound = "build.wood.place",
				DamageSound = "build.wood.hit",
				DestroySound = "build.wood.break",
				AssetsVerified = false,
				VerificationNotes = "Buscar modelo de barricada de madera legal (asset store / research). Fallback verificado."
			};
		}

		private static FortificationContentDefinition BarricadeReinforced()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_barricade_reinforced",
				DisplayName = "Barricada reforzada",
				Type = FortificationContentType.Barricade,
				Model = "models/sbox_props/metal_barricade/metal_barricade.vmdl", // PENDING_VERIFY
				ModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 400f,
				RepairAmount = 30f,
				RepairCost = 10,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Any,
				BuildSound = "build.metal.place",
				DamageSound = "build.metal.hit",
				DestroySound = "build.metal.break",
				AssetsVerified = false,
				VerificationNotes = "Mejora de la barricada de madera. Modelo metálico pendiente de verificar."
			};
		}

		private static FortificationContentDefinition DoorBasic()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_door_basic",
				DisplayName = "Puerta de apartamento",
				Type = FortificationContentType.Door,
				Model = "models/sbox_props/wooden_door/wooden_door.vmdl", // PENDING_VERIFY (falló en sprint previo)
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 200f,
				RepairAmount = 25f,
				RepairCost = 8,
				UpgradePrefab = "prefabs/content/fortification/fort_door_reinforced.prefab",
				SnapType = FortificationSnapType.Wall,
				BuildSound = "build.door.place",
				DamageSound = "build.wood.hit",
				DestroySound = "build.wood.break",
				AssetsVerified = false,
				VerificationNotes = "OJO: models/sbox_props/wooden_door/wooden_door.vmdl NO existe en el engine (sprint previo lo confirmó). Buscar puerta real."
			};
		}

		private static FortificationContentDefinition DoorReinforced()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_door_reinforced",
				DisplayName = "Puerta reforzada",
				Type = FortificationContentType.Door,
				Model = "models/sbox_props/metal_door/metal_door.vmdl", // PENDING_VERIFY
				ModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 500f,
				RepairAmount = 30f,
				RepairCost = 15,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Wall,
				BuildSound = "build.metal.place",
				DamageSound = "build.metal.hit",
				DestroySound = "build.metal.break",
				AssetsVerified = false,
				VerificationNotes = "Mejora de puerta básica."
			};
		}

		private static FortificationContentDefinition Stash()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_stash",
				DisplayName = "Alijo",
				Type = FortificationContentType.Stash,
				Model = "models/sbox_props/cardboard_box/cardboard_box_open.vmdl", // VERIFIED
				ModelFallback = "models/sbox_props/cardboard_box/cardboard_box_open.vmdl",
				Scale = 1f,
				MaxHealth = 100f,
				RepairAmount = 20f,
				RepairCost = 5,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Floor,
				BuildSound = "build.stash.place",
				DamageSound = "build.cardboard.hit",
				DestroySound = "build.cardboard.break",
				AssetsVerified = true,
				VerificationNotes = "Modelo ya verificado en el sprint previo. El contenido del alijo lo gestiona el core nuevo."
			};
		}

		private static FortificationContentDefinition Workbench()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_workbench",
				DisplayName = "Banco de trabajo",
				Type = FortificationContentType.Workbench,
				Model = "models/sbox_props/workbench/workbench.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 150f,
				RepairAmount = 20f,
				RepairCost = 6,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Floor,
				BuildSound = "build.workbench.place",
				DamageSound = "build.wood.hit",
				DestroySound = "build.wood.break",
				AssetsVerified = false,
				VerificationNotes = "Para crafting/mejoras. Buscar modelo real."
			};
		}

		private static FortificationContentDefinition Generator()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_generator",
				DisplayName = "Generador",
				Type = FortificationContentType.Generator,
				Model = "models/sbox_props/generator/generator.vmdl", // PENDING_VERIFY
				ModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 80f,
				RepairAmount = 20f,
				RepairCost = 8,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Floor,
				BuildSound = "build.generator.place",
				DamageSound = "build.metal.hit",
				DestroySound = "build.metal.break",
				AssetsVerified = false,
				VerificationNotes = "Energía del apartamento (luces/alarma) en el core nuevo."
			};
		}

		private static FortificationContentDefinition Alarm()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_alarm",
				DisplayName = "Alarma",
				Type = FortificationContentType.Alarm,
				Model = "models/sbox_props/alarm/alarm.vmdl", // PENDING_VERIFY
				ModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				Scale = 0.5f,
				MaxHealth = 50f,
				RepairAmount = 15f,
				RepairCost = 10,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Wall,
				BuildSound = "build.alarm.place",
				DamageSound = "build.electric.hit",
				DestroySound = "build.electric.break",
				AssetsVerified = false,
				VerificationNotes = "Avisa de raids en el core nuevo. Sonido de sirena pendiente (research audio)."
			};
		}

		private static FortificationContentDefinition RepairStation()
		{
			return new FortificationContentDefinition
			{
				Id = "fort_repair_station",
				DisplayName = "Estación de reparación",
				Type = FortificationContentType.RepairStation,
				Model = "models/sbox_props/repair_station/repair_station.vmdl", // PENDING_VERIFY
				ModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				Scale = 1f,
				MaxHealth = 120f,
				RepairAmount = 25f,
				RepairCost = 6,
				UpgradePrefab = "",
				SnapType = FortificationSnapType.Floor,
				BuildSound = "build.repair.place",
				DamageSound = "build.metal.hit",
				DestroySound = "build.metal.break",
				AssetsVerified = false,
				VerificationNotes = "Repara fortificaciones cercanas consumiendo recursos (core nuevo)."
			};
		}
	}
}
