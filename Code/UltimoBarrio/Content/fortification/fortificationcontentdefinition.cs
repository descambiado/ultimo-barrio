using Sandbox;

namespace UltimoBarrio.Content.Fortification
{
	public enum FortificationContentType
	{
		Barricade,
		Door,
		Stash,
		Workbench,
		Generator,
		Alarm,
		RepairStation
	}

	public enum FortificationSnapType
	{
		Any,
		Floor,
		Wall
	}

	/// <summary>
	/// Definición data-driven de una fortificación de contenido.
	/// Clase plana registrada en FortificationContentRegistry.
	/// Model → candidato PRIMARIO (pendiente verificación); ModelFallback → ruta VERIFICADA.
	/// </summary>
	public sealed class FortificationContentDefinition
	{
		public string Id { get; set; } = "";
		public string DisplayName { get; set; } = "";
		public FortificationContentType Type { get; set; } = FortificationContentType.Barricade;

		public string Model { get; set; } = "";
		public string ModelFallback { get; set; } = "";
		public float Scale { get; set; } = 1f;

		public float MaxHealth { get; set; } = 100f;
		public float RepairAmount { get; set; } = 25f;   // por uso de reparación
		public int RepairCost { get; set; } = 5;          // unidades de recurso (mapeado luego)
		public string UpgradePrefab { get; set; } = "";   // ruta prefab de la mejora (vacío = sin mejora)

		public FortificationSnapType SnapType { get; set; } = FortificationSnapType.Any;
		public Vector2 Footprint { get; set; } = new( 40f, 40f ); // huella en unidades (para placement preview)

		public string BuildSound { get; set; } = "";
		public string DamageSound { get; set; } = "";
		public string DestroySound { get; set; } = "";

		public bool AssetsVerified { get; set; } = false;
		public string VerificationNotes { get; set; } = "";
	}
}
