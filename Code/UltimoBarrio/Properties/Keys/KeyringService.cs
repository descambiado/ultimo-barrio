// SPDX-License-Identifier: MPL-2.0

using System.Linq;
using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Players;
using UltimoBarrio.Properties.Doors;

namespace UltimoBarrio.Properties.Keys;

/// <summary>
/// Servicio host-autoritativo de credenciales: entregar, revocar, duplicar y
/// forzar cambio de cerradura. Todas las operaciones administrativas
/// (entregar/revocar/rekey) exigen que el llamante sea PropertyComponent.
/// OwnerPersistentId — la propia credencial nunca es suficiente para
/// autorizarlas, solo para abrir la puerta físicamente.
/// </summary>
[Title( "Keyring Service" )]
[Category( "Último Barrio — Properties" )]
[Icon( "vpn_key" )]
public sealed class KeyringService : Component
{
	private IPlayerIdentityProvider _identityProvider;

	protected override void OnStart()
	{
		_identityProvider ??= new SteamPlayerIdentityProvider();
	}

	[Rpc.Host]
	public void RequestGrantAccess( GameObject targetPlayer, string propertyId, AccessLevel level, float durationSeconds = 0f )
	{
		if ( !Networking.IsHost || targetPlayer is null )
			return;

		if ( !TryResolveOwnerCaller( propertyId, out var property, out var reason ) )
		{
			Log.Warning( $"UB.Property GrantAccessRejected property={propertyId} reason={reason}" );
			return;
		}

		var door = ResolvePrimaryDoor( property );
		if ( door is null )
		{
			Log.Warning( $"UB.Property GrantAccessRejected property={propertyId} reason=NoDoorInstalled" );
			return;
		}

		var keyring = targetPlayer.Components.GetInDescendantsOrSelf<KeyringItem>() ?? targetPlayer.Components.GetOrCreate<KeyringItem>();

		var credential = new AccessCredential
		{
			PropertyId = propertyId,
			LockId = door.LockId,
			KeyRevision = door.KeyRevision,
			AccessLevel = level,
			IssuerPersistentId = property.OwnerPersistentId,
			ExpiresAt = durationSeconds > 0f ? Time.Now + durationSeconds : 0f
		};

		keyring.Grant( credential );
		Persistence.PersistenceBridge.RequestSave( "keyring_grant" );

		Log.Info( $"UB.Property AccessGranted property={propertyId} level={level} target={targetPlayer.Name}" );
	}

	[Rpc.Host]
	public void RequestRevokeAccess( GameObject targetPlayer, string propertyId )
	{
		if ( !Networking.IsHost || targetPlayer is null )
			return;

		if ( !TryResolveOwnerCaller( propertyId, out _, out var reason ) )
		{
			Log.Warning( $"UB.Property RevokeAccessRejected property={propertyId} reason={reason}" );
			return;
		}

		var keyring = targetPlayer.Components.GetInDescendantsOrSelf<KeyringItem>();
		var removed = keyring?.Revoke( propertyId ) ?? 0;

		if ( removed > 0 )
			Persistence.PersistenceBridge.RequestSave( "keyring_revoke" );

		Log.Info( $"UB.Property AccessRevoked property={propertyId} target={targetPlayer.Name} removed={removed}" );
	}

	/// <summary>Un residente/co-propietario con credencial vigente puede duplicarla para un invitado temporal — no exige ser el owner.</summary>
	[Rpc.Host]
	public void RequestDuplicateAccess( GameObject targetPlayer, string propertyId, float durationSeconds = 3600f )
	{
		if ( !Networking.IsHost || targetPlayer is null )
			return;

		if ( Rpc.Caller is null || !_identityProvider.TryResolve( Rpc.Caller, out var callerIdentity ) )
			return;

		var callerPlayer = FindPlayer( Rpc.Caller );
		var callerKeyring = callerPlayer?.Components.GetInDescendantsOrSelf<KeyringItem>();
		var source = callerKeyring?.FindCredential( propertyId );

		if ( source is null )
		{
			Log.Warning( $"UB.Property DuplicateAccessRejected property={propertyId} reason=CallerHasNoCredential" );
			return;
		}

		var copy = source.Clone( callerIdentity.CanonicalId );
		copy.AccessLevel = AccessLevel.Guest;
		copy.ExpiresAt = durationSeconds > 0f ? Time.Now + durationSeconds : 0f;

		var targetKeyring = targetPlayer.Components.GetInDescendantsOrSelf<KeyringItem>() ?? targetPlayer.Components.GetOrCreate<KeyringItem>();
		targetKeyring.Grant( copy );
		Persistence.PersistenceBridge.RequestSave( "keyring_duplicate" );

		Log.Info( $"UB.Property AccessDuplicated property={propertyId} target={targetPlayer.Name}" );
	}

	[Rpc.Host]
	public void RequestRekeyDoor( string propertyId )
	{
		if ( !Networking.IsHost )
			return;

		if ( !TryResolveOwnerCaller( propertyId, out var property, out var reason ) )
		{
			Log.Warning( $"UB.Property RekeyRejected property={propertyId} reason={reason}" );
			return;
		}

		var door = ResolvePrimaryDoor( property );
		if ( door is null )
			return;

		door.Rekey();

		foreach ( var keyring in Scene.GetAllComponents<KeyringItem>() )
			keyring.PruneStaleRevisions( propertyId, door.LockId, door.KeyRevision );

		Persistence.PersistenceBridge.RequestSave( "keyring_rekey" );
		Log.Info( $"UB.Property DoorRekeyed property={propertyId} revision={door.KeyRevision}" );
	}

	private bool TryResolveOwnerCaller( string propertyId, out PropertyComponent property, out string reason )
	{
		property = null;
		reason = string.Empty;

		if ( Rpc.Caller is null || !_identityProvider.TryResolve( Rpc.Caller, out var callerIdentity ) )
		{
			reason = "InvalidCaller";
			return false;
		}

		property = Scene.GetAllComponents<PropertyComponent>().FirstOrDefault( p => p.PropertyId == propertyId );
		if ( property is null )
		{
			reason = "InvalidProperty";
			return false;
		}

		if ( property.OwnerPersistentId != callerIdentity.CanonicalId )
		{
			reason = "CallerIsNotOwner";
			return false;
		}

		return true;
	}

	private static PropertyDoor ResolvePrimaryDoor( PropertyComponent property )
	{
		return Game.ActiveScene.GetAllComponents<DoorAnchor>()
			.Where( a => a.PropertyId == property.PropertyId && a.HasDoor )
			.Select( a => a.DoorReference )
			.FirstOrDefault();
	}

	private PlayerController FindPlayer( Connection connection )
	{
		return Scene.GetAllComponents<PlayerController>().FirstOrDefault(
			player => player.GameObject.Network.OwnerId == connection.Id );
	}
}
