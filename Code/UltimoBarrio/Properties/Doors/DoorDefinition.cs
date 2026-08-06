// SPDX-License-Identifier: MPL-2.0

using Sandbox;

namespace UltimoBarrio.Properties.Doors;

/// <summary>
/// Definición data-driven de un tipo de puerta instalable (wooden_door,
/// reinforced_door, ...). Mismo patrón que ItemDefinition/MovementProfile.
/// </summary>
#pragma warning disable CS0618
[GameResource( "ultimo_barrio/door_def", "ubdoor", "Define un tipo de puerta instalable en una propiedad." )]
#pragma warning restore CS0618
public class DoorDefinition : GameResource
{
	public string DoorId { get; set; } = "wooden_door";

	public string DisplayName { get; set; } = "Puerta de madera";

	public float MaxHealth { get; set; } = 250f;

	/// <summary>Bonus de MaxHealth aplicado por nivel de mejora (misma proporción que ApartmentFortification: 250→312.5 en nivel 1).</summary>
	public float UpgradeHealthBonus { get; set; } = 62.5f;

	public int MaxUpgradeLevel { get; set; } = 3;

	/// <summary>Ítem de inventario que se consume al instalar esta puerta.</summary>
	public string RequiredItemId { get; set; } = "apartment_door_kit";

	/// <summary>Ruta del prefab de mundo, si difiere del genérico.</summary>
	public string WorldPrefab { get; set; } = string.Empty;
}
