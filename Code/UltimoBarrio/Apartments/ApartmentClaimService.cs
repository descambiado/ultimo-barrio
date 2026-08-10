// SPDX-License-Identifier: MPL-2.0

using System;
using UltimoBarrio.Persistence;
using UltimoBarrio.Players;
using UltimoBarrio.Core;

namespace UltimoBarrio.Apartments;

[Title( "Apartment Claim Service" )]
[Category( "Ultimo Barrio" )]
[Icon( "real_estate_agent" )]
public sealed class ApartmentClaimService : Component, Component.INetworkListener, IApartmentAccessPolicy
{
	[Property] public string SaveSlotId { get; set; } = "prototype";

	/// <summary>
	/// El radio mÃ¡ximo (en unidades) desde el cual el jugador puede reclamar el apartamento.
	/// Evaluado desde la posiciÃ³n del <see cref="ApartmentComponent.SpawnReference"/>.
	/// </summary>
	[Property] public float ClaimDistance { get; set; } = 150f;

	private readonly object _claimGate = new();
	private readonly HashSet<string> _apartmentsInProgress = new( StringComparer.Ordinal );
	private readonly HashSet<string> _ownersInProgress = new( StringComparer.Ordinal );
	private readonly List<Connection> _pendingRespawns = [];

	private ApartmentRegistry _registry;
	private IPersistenceProvider _persistence;
	private IPlayerIdentityProvider _identityProvider;
	private bool _initializationAttempted;
	private bool _isReady;

	protected override void OnStart()
	{
		if ( Scene.IsEditor )
			return;

		if ( !TryInitialize() )
		{
			// Log...
		}
	}

	protected override void OnUpdate()
	{
		if ( Scene.IsEditor || !Networking.IsHost )
			return;

		TryInitialize();
		ProcessPendingRespawns();
	}

	public void OnActive( Connection connection )
	{
		if ( !Networking.IsHost || connection is null )
			return;

		if ( !_pendingRespawns.Contains( connection ) )
			_pendingRespawns.Add( connection );
	}

	public void OnDisconnected( Connection connection )
	{
		_pendingRespawns.Remove( connection );
	}

	[Rpc.Host]
	public void RequestClaim( string apartmentId )
	{
		var result = TryClaim( Rpc.Caller, apartmentId );
		var knownApartmentId = _registry is not null && _registry.TryGet( apartmentId, out var apartment )
			? apartment.ApartmentId
			: "<invalid>";

		if ( result.Succeeded )
		{
			Log.Info( $"UB.Apartment ClaimSucceeded apartment={knownApartmentId}" );
			return;
		}

		Log.Warning( $"UB.Apartment ClaimRejected apartment={knownApartmentId} reason={result.Failure}" );
	}

	internal ApartmentClaimResult TryClaim( Connection caller, string apartmentId )
	{
		if ( !Networking.IsHost || !TryInitialize() )
		{
			return ApartmentClaimResult.Rejected(
				ApartmentClaimFailure.ServiceNotReady,
				"The claim service is not ready." );
		}

		if ( caller is null || !caller.IsActive || !_identityProvider.TryResolve( caller, out var ownerIdentity ) )
		{
			return ApartmentClaimResult.Rejected(
				ApartmentClaimFailure.InvalidCaller,
				"The caller does not have a stable host-resolved identity." );
		}

		if ( !_registry.TryGet( apartmentId, out var apartment ) )
		{
			return ApartmentClaimResult.Rejected(
				ApartmentClaimFailure.InvalidApartment,
				"The requested apartment is not registered." );
		}

		var player = FindPlayer( caller );
		if ( !player.IsValid() )
		{
			return ApartmentClaimResult.Rejected(
				ApartmentClaimFailure.PlayerNotFound,
				"The host could not resolve the caller's player." );
		}

		var distanceSquared = (player.WorldPosition - apartment.DoorReference.WorldPosition).LengthSquared;
		if ( distanceSquared > ClaimDistance * ClaimDistance )
		{
			return ApartmentClaimResult.Rejected(
				ApartmentClaimFailure.OutOfRange,
				"The host-resolved player is outside claim range." );
		}

		lock ( _claimGate )
		{
			if ( _apartmentsInProgress.Contains( apartment.ApartmentId )
				|| _ownersInProgress.Contains( ownerIdentity.CanonicalId ) )
			{
				return ApartmentClaimResult.Rejected(
					ApartmentClaimFailure.ClaimInProgress,
					"A claim for this apartment or player is already in progress." );
			}

			if ( apartment.ClaimState != ApartmentClaimState.Unclaimed )
			{
				return ApartmentClaimResult.Rejected(
					ApartmentClaimFailure.ApartmentUnavailable,
					"The apartment is no longer available." );
			}

			if ( _registry.FindByOwner( ownerIdentity.CanonicalId ).IsValid() )
			{
				return ApartmentClaimResult.Rejected(
					ApartmentClaimFailure.PlayerAlreadyOwnsApartment,
					"The player already owns an apartment." );
			}

			_apartmentsInProgress.Add( apartment.ApartmentId );
			_ownersInProgress.Add( ownerIdentity.CanonicalId );

			try
			{
				var candidate = _registry.CreateSnapshot( SaveSlotId, apartment.ApartmentId, ownerIdentity.CanonicalId );
				var saveResult = _persistence.Save( candidate );
				if ( !saveResult.Succeeded )
				{
					return ApartmentClaimResult.Rejected(
						ApartmentClaimFailure.PersistenceFailed,
						saveResult.Error );
				}

				apartment.ApplyState( ownerIdentity.CanonicalId, ApartmentClaimState.Claimed );
				return ApartmentClaimResult.Success();
			}
			finally
			{
				_apartmentsInProgress.Remove( apartment.ApartmentId );
				_ownersInProgress.Remove( ownerIdentity.CanonicalId );
			}
		}
	}

	private bool TryInitialize()
	{
		if ( _isReady )
			return true;

		if ( _initializationAttempted || !Networking.IsActive || !Networking.IsHost )
			return false;

		_initializationAttempted = true;
		_registry = ApartmentRegistry.Build( Scene.GetAllComponents<ApartmentComponent>() );
		if ( !_registry.IsValid )
		{
			foreach ( var error in _registry.Errors )
				Log.Error( $"UB.Apartment RegistryInvalid reason={error}" );

			return false;
		}

		_persistence ??= new LocalPersistenceProvider( FileSystem.Data );
		_identityProvider ??= new SteamPlayerIdentityProvider();

		var loadResult = _persistence.Load( SaveSlotId );
		if ( !loadResult.Succeeded )
		{
			Log.Error( $"UB.Apartment SaveLoadFailed status={loadResult.Status}" );
			return false;
		}

		if ( loadResult.Snapshot is not null )
		{
			// RestauraciÃ³n segura: solo se aplica estado de viviendas que existen en ESTA escena.
			// ApplySnapshot ya ignora apartmentIds ausentes; aquÃ­ solo informamos de los descartados
			// para no teletransportar al jugador a posiciones de un mapa/versiÃ³n anterior.
			var stale = loadResult.Snapshot.Apartments
				.Where( saved => !_registry.TryGet( saved.ApartmentId, out _ ) )
				.Select( saved => saved.ApartmentId )
				.ToList();
			if ( stale.Count > 0 )
			{
				Log.Warning( $"UB.Apartment SaveIgnoredStale apartments={string.Join( ",", stale )} — no existen en esta escena; no se restauran." );
			}

			_registry.ApplySnapshot( loadResult.Snapshot );
		}

		_isReady = true;
		Log.Info( $"UB.Apartment ServiceReady apartments={_registry.Apartments.Count} loaded={loadResult.Status == PersistenceLoadStatus.Loaded}" );
		return true;
	}

	private PlayerController FindPlayer( Connection connection )
	{
		return Scene.GetAllComponents<PlayerController>().FirstOrDefault(
			player => player.GameObject.Network.OwnerId == connection.Id );
	}

	private void ProcessPendingRespawns()
	{
		if ( !_isReady || _pendingRespawns.Count == 0 )
			return;

		foreach ( var connection in _pendingRespawns.ToList() )
		{
			if ( connection is null || !connection.IsActive )
			{
				_pendingRespawns.Remove( connection );
				continue;
			}

			if ( !_identityProvider.TryResolve( connection, out var ownerIdentity ) )
			{
				_pendingRespawns.Remove( connection );
				continue;
			}

			var apartment = _registry.FindByOwner( ownerIdentity.CanonicalId );
			if ( !apartment.IsValid() )
			{
				_pendingRespawns.Remove( connection );
				continue;
			}

			if ( !apartment.SpawnReference.IsValid() )
			{
				_pendingRespawns.Remove( connection );
				Log.Warning( $"UB.Apartment OwnerRespawnFallback apartment={apartment.ApartmentId} reason=spawn_reference_missing" );
				continue;
			}

			var player = FindPlayer( connection );
			if ( !player.IsValid() )
				continue;

			player.GameObject.WorldTransform = apartment.SpawnReference.WorldTransform.WithScale( 1.0f );
			_pendingRespawns.Remove( connection );
			Log.Info( $"UB.Apartment OwnerRespawned apartment={apartment.ApartmentId}" );
		}
	}

	public bool CanEnter( string apartmentId, string playerId )
	{
		return CheckAccess( apartmentId, playerId );
	}

	public bool CanAccessStash( string apartmentId, string playerId )
	{
		return CheckAccess( apartmentId, playerId );
	}

	private bool CheckAccess( string apartmentId, string playerId )
	{
		if ( _registry == null || !_registry.TryGet( apartmentId, out var apartment ) ) return true;
		if ( apartment.ClaimState != ApartmentClaimState.Claimed ) return true;
		
		// playerId is the domain PlayerId (e.g. SteamId or QA ID)
		return apartment.OwnerId == playerId;
	}
}
