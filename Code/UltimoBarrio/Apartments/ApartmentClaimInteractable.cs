// SPDX-License-Identifier: MPL-2.0

using System;

namespace UltimoBarrio.Apartments;

[Title( "Apartment Claim Interactable" )]
[Category( "Último Barrio" )]
[Icon( "touch_app" )]
public sealed class ApartmentClaimInteractable : Component, Component.IPressable
{
	[Property] public string ApartmentId { get; set; } = "apartment-a01";

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
