// SPDX-License-Identifier: MPL-2.0

using UltimoBarrio.Persistence;

namespace UltimoBarrio.Apartments;

[Title( "Último Barrio Apartment" )]
[Category( "Último Barrio" )]
[Icon( "apartment" )]
public sealed class ApartmentComponent : Component
{
	[Property] public string ApartmentId { get; set; } = "apartment-a01";

	[Sync( SyncFlags.FromHost )]
	public string OwnerId { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )]
	public ApartmentClaimState ClaimState { get; private set; } = ApartmentClaimState.Unclaimed;

	[Property] public GameObject DoorReference { get; set; }

	[Property] public GameObject StashReference { get; set; }

	[Property] public GameObject SpawnReference { get; set; }

	[Property] public int SaveVersion { get; set; } = SaveSnapshot.CurrentVersion;

	internal bool TryValidateConfiguration( out string error )
	{
		if ( string.IsNullOrWhiteSpace( ApartmentId ) )
		{
			error = "ApartmentId is empty.";
			return false;
		}

		if ( !DoorReference.IsValid() )
		{
			error = $"Apartment '{ApartmentId}' is missing DoorReference.";
			return false;
		}

		if ( !StashReference.IsValid() )
		{
			error = $"Apartment '{ApartmentId}' is missing StashReference.";
			return false;
		}

		if ( !SpawnReference.IsValid() )
		{
			error = $"Apartment '{ApartmentId}' is missing SpawnReference.";
			return false;
		}

		if ( SaveVersion != SaveSnapshot.CurrentVersion )
		{
			error = $"Apartment '{ApartmentId}' has unsupported SaveVersion {SaveVersion}.";
			return false;
		}

		error = string.Empty;
		return true;
	}

	internal void ApplyState( string ownerId, ApartmentClaimState claimState )
	{
		OwnerId = ownerId ?? string.Empty;
		ClaimState = claimState;
	}

	internal ApartmentSaveData ToSaveData( string ownerOverride = null )
	{
		var ownerId = ownerOverride ?? OwnerId;
		var claimState = string.IsNullOrEmpty( ownerId )
			? ApartmentClaimState.Unclaimed
			: ApartmentClaimState.Claimed;

		return new ApartmentSaveData
		{
			ApartmentId = ApartmentId,
			OwnerId = ownerId,
			ClaimState = claimState,
			SaveVersion = SaveVersion
		};
	}
}
