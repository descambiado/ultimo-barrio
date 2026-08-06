// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Properties;

public enum PropertyClaimFailure
{
	None = 0,
	ServiceNotReady,
	InvalidProperty,
	InvalidCaller,
	PlayerNotFound,
	OutOfRange,
	PropertyUnavailable,
	WrongPropertyType,
	MissingDoorKit,
	DoorNotInstalled,
	CabinetNotInstalled,
	InsufficientFunds,
	ClaimInProgress,
	PersistenceFailed
}

public readonly record struct PropertyClaimResult(
	bool Succeeded,
	PropertyClaimFailure Failure,
	string Message )
{
	public static PropertyClaimResult Success( string message = "OK" )
	{
		return new( true, PropertyClaimFailure.None, message );
	}

	public static PropertyClaimResult Rejected( PropertyClaimFailure failure, string message )
	{
		return new( false, failure, message );
	}
}
