using Sandbox;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>Categorías de estructura construible del pack de contenido.</summary>
	public enum BuildCategory
	{
		Barricade,
		Door,
		Stash,
		Workbench,
		Generator,
		Alarm,
		RepairStation
	}

	/// <summary>
	/// Definición data-driven de un build (fortificación).
	/// Clase plana registrada en FortificationContentRegistry; los prefabs del pack
	/// solo referencian DefinitionId y el resto son datos.
	/// Model = ruta REAL verificada (asset-registry.yml); ModelFallback = ruta de
	/// reserva para assets pendientes de verificar (vacío = sin fallback).
	/// </summary>
	public sealed class BuildDefinition
	{
		public string Id { get; set; } = "";
		public string DisplayName { get; set; } = "";
		public BuildCategory Category { get; set; } = BuildCategory.Barricade;

		public string Prefab { get; set; } = "";        // ruta prefab para el spawn (BuildStructureHost.SpawnBuild)
		public string Model { get; set; } = "";         // modelo primario (verificado en asset-registry.yml)
		public string ModelFallback { get; set; } = ""; // ruta de reserva (vacío = sin fallback)

		public float MaxHp { get; set; } = 100f;
		public float RepairAmount { get; set; } = 25f; // HP restaurado por uso de reparación
		public int RepairCost { get; set; } = 5;       // unidades de recurso por reparación
		public string UpgradeTo { get; set; } = "";    // id del BuildDefinition de la mejora (vacío = sin mejora)

		public float Scale { get; set; } = 1f;
		public Vector2 Footprint { get; set; } = new( 40f, 40f ); // huella en unidades (overlap + volumen de prueba)

		public bool AssetsVerified { get; set; } = false;
		public string VerificationNotes { get; set; } = "";
	}
}
