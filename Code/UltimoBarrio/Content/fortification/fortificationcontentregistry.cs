using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>
	/// Catálogo del fortification content pack (portable). TODOS los objetos como DATA:
	/// una sola implementación (BuildStructureHost) + una entrada de datos por estructura.
	/// Orden de registro preservado (All); la barricada de madera va PRIMERA.
	/// Modelos REALES del engine verificados (asset-registry.yml, Worker D 2026-08-08).
	/// </summary>
	public static class FortificationContentRegistry
	{
		private static readonly Dictionary<string, BuildDefinition> _definitions = new();
		private static readonly List<string> _order = new();

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

		public static BuildDefinition Get( string id )
		{
			return id != null && _definitions.TryGetValue( id, out var def ) ? def : null;
		}

		public static IReadOnlyList<BuildDefinition> All => _order.Select( id => _definitions[id] ).ToList();

		private static void Register( BuildDefinition def )
		{
			_definitions[def.Id] = def;
			_order.Add( def.Id );
		}

		private static BuildDefinition BarricadeWood()
		{
			return new BuildDefinition
			{
				Id = "fort_barricade_wood",
				DisplayName = "Barricada de madera",
				Category = BuildCategory.Barricade,
				Prefab = "prefabs/content/fortification/fort_barricade_wood.prefab",
				Model = "models/sbox_props/benches/old_bench.vmdl", // VERIFIED (engine.sbox_props_old_bench)
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 150f,
				RepairAmount = 25f,
				RepairCost = 5,
				UpgradeTo = "fort_barricade_reinforced",
				AssetsVerified = true,
				VerificationNotes = "Modelo real del engine (asset-registry engine.sbox_props_old_bench). Upgrades a reinforced."
			};
		}

		private static BuildDefinition BarricadeReinforced()
		{
			return new BuildDefinition
			{
				Id = "fort_barricade_reinforced",
				DisplayName = "Barricada reforzada",
				Category = BuildCategory.Barricade,
				Prefab = "prefabs/content/fortification/fort_barricade_reinforced.prefab",
				Model = "models/sbox_props/security_shutter/security_shutter_box_middle.vmdl", // VERIFIED
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 400f,
				RepairAmount = 30f,
				RepairCost = 10,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "Panel metálico (caja de persiana de seguridad). Mejora de la barricada de madera."
			};
		}

		private static BuildDefinition DoorBasic()
		{
			return new BuildDefinition
			{
				Id = "fort_door_basic",
				DisplayName = "Puerta básica",
				Category = BuildCategory.Door,
				Prefab = "prefabs/content/fortification/fort_door_basic.prefab",
				Model = "models/sbox_props/security_shutter/security_shutter_curtain_128.vmdl", // VERIFIED (PROXY VISUAL)
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 200f,
				RepairAmount = 25f,
				RepairCost = 8,
				UpgradeTo = "fort_door_reinforced",
				AssetsVerified = true,
				VerificationNotes = "No hay puerta vmdl en el engine: proxy real de cortina de persiana 128u (asset-registry)."
			};
		}

		private static BuildDefinition DoorReinforced()
		{
			return new BuildDefinition
			{
				Id = "fort_door_reinforced",
				DisplayName = "Puerta reforzada",
				Category = BuildCategory.Door,
				Prefab = "prefabs/content/fortification/fort_door_reinforced.prefab",
				Model = "models/sbox_props/security_shutter/security_shutter_curtain_bottom.vmdl", // VERIFIED (PROXY VISUAL)
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 500f,
				RepairAmount = 30f,
				RepairCost = 15,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "Sección inferior de persiana con barra de bloqueo. Mejora de la puerta básica."
			};
		}

		private static BuildDefinition Stash()
		{
			return new BuildDefinition
			{
				Id = "fort_stash",
				DisplayName = "Alijo",
				Category = BuildCategory.Stash,
				Prefab = "prefabs/content/fortification/fort_stash.prefab",
				Model = "models/citizen_props/gritbin01_combined.vmdl", // VERIFIED
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 100f,
				RepairAmount = 20f,
				RepairCost = 5,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "Contenedor a granel (silo/arenero). El contenido del alijo lo gestiona el core nuevo."
			};
		}

		private static BuildDefinition Workbench()
		{
			return new BuildDefinition
			{
				Id = "fort_workbench",
				DisplayName = "Banco de trabajo",
				Category = BuildCategory.Workbench,
				Prefab = "prefabs/content/fortification/fort_workbench.prefab",
				Model = "models/citizen_props/oldoven.vmdl", // VERIFIED (PROXY VISUAL)
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 150f,
				RepairAmount = 20f,
				RepairCost = 6,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "No hay workbench vmdl: proxy real de estufa/forja industrial (asset-registry)."
			};
		}

		private static BuildDefinition Generator()
		{
			return new BuildDefinition
			{
				Id = "fort_generator",
				DisplayName = "Generador",
				Category = BuildCategory.Generator,
				Prefab = "prefabs/content/fortification/fort_generator.prefab",
				Model = "models/props/aircon_unit_wall/aircon_unit_medium_wall.vmdl", // VERIFIED (PROXY VISUAL)
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 80f,
				RepairAmount = 20f,
				RepairCost = 8,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "No hay generator vmdl: proxy real de unidad de aire acondicionado (asset-registry)."
			};
		}

		private static BuildDefinition Alarm()
		{
			return new BuildDefinition
			{
				Id = "fort_alarm",
				DisplayName = "Alarma",
				Category = BuildCategory.Alarm,
				Prefab = "prefabs/content/fortification/fort_alarm.prefab",
				Model = "models/sbox_props/intruder_alarm_2/intruder_alarm_2.vmdl", // VERIFIED
				ModelFallback = "",
				Scale = 0.5f,
				MaxHp = 50f,
				RepairAmount = 15f,
				RepairCost = 10,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "Sirena de intrusos real del engine. Avisa de raids en el core nuevo."
			};
		}

		private static BuildDefinition RepairStation()
		{
			return new BuildDefinition
			{
				Id = "fort_repair_station",
				DisplayName = "Estación de reparación",
				Category = BuildCategory.RepairStation,
				Prefab = "prefabs/content/fortification/fort_repair_station.prefab",
				Model = "models/props/mobile_masts/microwave_trans.vmdl", // VERIFIED (PROXY VISUAL)
				ModelFallback = "",
				Scale = 1f,
				MaxHp = 120f,
				RepairAmount = 25f,
				RepairCost = 6,
				UpgradeTo = "",
				AssetsVerified = true,
				VerificationNotes = "Proxy real de equipo de comunicaciones. Repara fortificaciones cercanas en el core nuevo."
			};
		}
	}
}
