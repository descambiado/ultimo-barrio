// SPDX-License-Identifier: MPL-2.0

using System;
using System.Linq;
using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Apartments;

[Title( "Apartment Claim Interactable" )]
[Category( "Último Barrio" )]
[Icon( "touch_app" )]
public sealed class ApartmentClaimInteractable : Component, Component.IPressable, IWorldInteractable, IInteractable
{
	[Property] public string ApartmentId { get; set; } = "apartment-a01";

	public string GetInteractionPrompt( InteractionRequest request )
	{
		var apt = FindApartment();
		if ( apt?.ClaimState != ApartmentClaimState.Unclaimed )
			return "Este piso ya tiene dueño";

		return HasDoorKit( request ) ? "Este piso está disponible" : "Necesitas un kit de puerta para reclamarlo";
	}

	public bool CanInteract( InteractionRequest request )
	{
		return FindApartment()?.ClaimState == ApartmentClaimState.Unclaimed && HasDoorKit( request );
	}

	/// <summary>
	/// Comprobación de cliente solo para UX (mostrar el prompt correcto); la
	/// autorización real y el consumo del kit ocurren en
	/// ApartmentClaimService.TryClaim, en el host.
	/// </summary>
	private static bool HasDoorKit( InteractionRequest request )
	{
		var inventory = request.InteractorObject?.Components.GetInDescendantsOrSelf<InventoryComponent>();
		return inventory is not null && inventory.GetCount( ApartmentClaimService.DoorKitItemId ) >= 1;
	}

	public void OnInteract( InteractionRequest request )
	{
		var pressable = this as Component.IPressable;
		pressable?.Press( new Component.IPressable.Event() );
	}

	bool IPressable.CanPress( IPressable.Event pressEvent )
	{
		return FindApartment()?.ClaimState == ApartmentClaimState.Unclaimed;
	}

	bool IPressable.Press( IPressable.Event pressEvent )
	{
		var service = Scene.GetAllComponents<ApartmentClaimService>().FirstOrDefault();
		if ( !service.IsValid() )
		{
			Log.Warning( $"UB.Apartment ClaimRejected apartment={ApartmentId} reason=service_missing" );
			return false;
		}

		service.RequestClaim( ApartmentId );
		return true;
	}

	private ApartmentComponent FindApartment()
	{
		return Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(
			apartment => string.Equals( apartment.ApartmentId, ApartmentId, StringComparison.Ordinal ) );
	}
}
