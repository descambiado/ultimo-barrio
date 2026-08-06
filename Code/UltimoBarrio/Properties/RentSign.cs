// SPDX-License-Identifier: MPL-2.0

using System.Linq;
using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Properties;

/// <summary>
/// Cartel de alquiler: E ejecuta directamente el pago de depósito+renta vía
/// PropertyClaimService.RequestRentProperty. El "panel de propiedad" completo
/// del spec (Razor, como TraderUI/CraftingPanel) se deja para cuando el
/// editor esté vivo — dotnet build no valida markup .razor de la misma forma
/// que el compilador propio del editor de sbox, y ya hay un precedente esta
/// sesión de un panel "solo compila" que resultó tener un gap de wiring real
/// hasta verse en pantalla (MissionJournalPanel). Esto entrega la transacción
/// real sin ese riesgo: feedback por UI.PlayerFeedback, no un panel nuevo.
/// </summary>
[Title( "Rent Sign" )]
[Category( "Último Barrio — Properties" )]
[Icon( "sell" )]
public sealed class RentSign : Component, IWorldInteractable, IInteractable
{
	[Property] public string PropertyId { get; set; } = string.Empty;

	[Property] public float MaxInteractionDistance { get; set; } = 150f;

	private PropertyComponent ResolveProperty()
	{
		return Scene.GetAllComponents<PropertyComponent>().FirstOrDefault( p => p.PropertyId == PropertyId );
	}

	public string GetInteractionPrompt( InteractionRequest request )
	{
		var property = ResolveProperty();
		if ( property is null )
			return "Cartel de alquiler (propiedad no encontrada)";

		return property.RentalState switch
		{
			RentalState.Vacant => $"Alquilar — depósito ${property.Deposit} + renta ${property.RentPrice}",
			RentalState.Rented => "Ya alquilada",
			RentalState.GracePeriod => "Alquilada (pago pendiente)",
			_ => "Alquiler no disponible"
		};
	}

	public bool CanInteract( InteractionRequest request )
	{
		if ( request.InteractorObject == null )
			return false;

		return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
	}

	public void OnInteract( InteractionRequest request )
	{
		var claimService = Scene.GetAllComponents<PropertyClaimService>().FirstOrDefault();
		claimService?.RequestRentProperty( PropertyId );
	}
}
