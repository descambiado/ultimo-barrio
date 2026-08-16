using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Components;

/// <summary>
/// Host-authoritative ownership record for a world object.
///
/// This component deliberately stores a durable player identity instead of a
/// client-supplied network id. Systems such as drops, fortifications and future
/// apartment props can use the same ownership boundary without giving a client
/// authority to decide the result.
/// </summary>
[Title( "Último Barrio World Object Ownership" )]
[Category( "Último Barrio — Framework" )]
public sealed class WorldObjectOwnership : Component
{
	[Property] public bool AllowTransfer { get; set; }

	[Sync] public string OwnerId { get; private set; } = string.Empty;

	public bool HasOwner => !string.IsNullOrWhiteSpace( OwnerId );

	public bool IsOwnedBy( GameObject candidate )
	{
		if ( !HasOwner || !candidate.IsValid() ) return false;

		var identity = PlayerIdentity.FromGameObject( candidate );
		return identity.IsValid && identity.CanonicalId == OwnerId;
	}

	/// <summary>
	/// Assigns an owner after the calling gameplay service has already validated
	/// its own range, inventory or property rule. This method can only mutate
	/// state on the host.
	/// </summary>
	public bool TryAssignOwner( GameObject candidate )
	{
		if ( !Networking.IsHost || !candidate.IsValid() ) return false;

		var identity = PlayerIdentity.FromGameObject( candidate );
		if ( !identity.IsValid )
		{
			Log.Warning( $"[UB.Cleanup] ownership rejected: '{GameObject.Name}' has no valid player identity." );
			return false;
		}

		if ( HasOwner && OwnerId != identity.CanonicalId && !AllowTransfer )
		{
			Log.Warning( $"[UB.Cleanup] ownership transfer rejected: '{GameObject.Name}' is already owned." );
			return false;
		}

		OwnerId = identity.CanonicalId;
		return true;
	}

	public bool TryReleaseOwner( GameObject requester )
	{
		if ( !Networking.IsHost || !IsOwnedBy( requester ) ) return false;

		OwnerId = string.Empty;
		return true;
	}

	/// <summary>
	/// Reserved for host-side cleanup, destruction and administrative services.
	/// Gameplay callers should normally use <see cref="TryReleaseOwner"/>.
	/// </summary>
	public bool ClearOwnerFromHost()
	{
		if ( !Networking.IsHost ) return false;

		OwnerId = string.Empty;
		return true;
	}
}
