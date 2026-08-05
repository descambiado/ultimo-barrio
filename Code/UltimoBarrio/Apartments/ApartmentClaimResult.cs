// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Apartments;

public enum ApartmentClaimFailure
{
	None = 0,
	ServiceNotReady,
	InvalidApartment,
	InvalidCaller,
	PlayerNotFound,
	OutOfRange,
	ApartmentUnavailable,
	PlayerAlreadyOwnsApartment,
	ClaimInProgress,
	MissingDoorKit,
	PersistenceFailed
}

public readonly record struct ApartmentClaimResult(
	bool Succeeded,
	ApartmentClaimFailure Failure,
	string Message )
{
	public static ApartmentClaimResult Success()
	{
		return new( true, ApartmentClaimFailure.None, "Apartment claimed." );
	}

	public static ApartmentClaimResult Rejected( ApartmentClaimFailure failure, string message )
	{
		return new( false, failure, message );
	}
}
