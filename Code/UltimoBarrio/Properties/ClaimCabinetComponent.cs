// SPDX-License-Identifier: MPL-2.0

using System.Linq;
using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Properties;

/// <summary>
/// Marcador físico de que un AbandonedShell fue formalmente reclamado. Punto
/// de interacción futuro para el panel de permisos/mantenimiento/estructuras
/// (Tarea #27, Property Authoring Tool + UI real) — de momento solo muestra
/// un resumen por consola/feedback, sin panel Razor nuevo sin verificar.
/// </summary>
[Title( "Claim Cabinet" )]
[Category( "Último Barrio — Properties" )]
[Icon( "inventory_2" )]
public sealed class ClaimCabinetComponent : Component, IWorldInteractable, IInteractable
{
	[Property] public string PropertyId { get; set; } = string.Empty;

	public string GetInteractionPrompt( InteractionRequest request ) => "Armario de reclamo";

	public bool CanInteract( InteractionRequest request ) => true;

	public void OnInteract( InteractionRequest request )
	{
		var property = Scene.GetAllComponents<PropertyComponent>().FirstOrDefault( p => p.PropertyId == PropertyId );
		if ( property is null )
			return;

		UI.PlayerFeedback.Push(
			$"{property.DisplayName}: nivel {property.UpgradeLevel}, seguridad {property.SecurityLevel}, defensa {property.DefenseScore:0}" );
	}
}
