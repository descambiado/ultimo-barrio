// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Economy;
using UltimoBarrio.Players;
using UltimoBarrio.Properties.Doors;
using UltimoBarrio.Properties.Keys;

namespace UltimoBarrio.Properties;

/// <summary>
/// Servicio de reclamación/alquiler nativo del sistema general de propiedades.
/// Cubre PropertyComponent (Rental, AbandonedShell, ...) — los 6 apartamentos
/// fixture (A01-A06) siguen bajo ApartmentClaimService mientras dura la
/// migración (ver su implementación adapter de IPropertyAccessPolicy).
/// El commit de una reclamación/alquiler exitoso fuerza un guardado síncrono
/// vía ApartmentClaimService.TrySaveNow() (dueño actual del proveedor de
/// persistencia) para no dejar la transacción a merced del autosave periódico;
/// si no hay ninguno en la escena, cae a PersistenceBridge.RequestSave (async).
/// </summary>
[Title( "Property Claim Service" )]
[Category( "Último Barrio" )]
[Icon( "real_estate_agent" )]
public sealed class PropertyClaimService : Component, IPropertyAccessPolicy
{
	[Property] public float ClaimDistance { get; set; } = 150f;

	private readonly object _claimGate = new();
	private readonly HashSet<string> _propertiesInProgress = new( StringComparer.Ordinal );
	private readonly HashSet<string> _actorsInProgress = new( StringComparer.Ordinal );

	private Dictionary<string, PropertyComponent> _registry;
	private IPlayerIdentityProvider _identityProvider;
	private bool _isReady;

	public bool IsReady => _isReady;

	protected override void OnStart()
	{
		if ( Scene.IsEditor )
			return;

		if ( !Networking.IsHost )
			return;

		BuildRegistry();
	}

	private void BuildRegistry()
	{
		_identityProvider ??= new SteamPlayerIdentityProvider();

		_registry = new Dictionary<string, PropertyComponent>( StringComparer.Ordinal );
		foreach ( var property in Scene.GetAllComponents<PropertyComponent>() )
		{
			if ( string.IsNullOrEmpty( property.PropertyId ) )
			{
				Log.Warning( $"UB.Property SkippedEmptyId gameObject={property.GameObject.Name}" );
				continue;
			}

			if ( _registry.ContainsKey( property.PropertyId ) )
			{
				Log.Error( $"UB.Property DuplicatePropertyId id={property.PropertyId}" );
				continue;
			}

			_registry[property.PropertyId] = property;
		}

		_isReady = true;
		Log.Info( $"UB.Property ServiceReady count={_registry.Count}" );
	}

	[Rpc.Host]
	public void RequestClaimAbandonedShell( string propertyId )
	{
		var result = TryClaimAbandonedShell( Rpc.Caller, propertyId );
		if ( result.Succeeded )
		{
			Log.Info( $"UB.Property ClaimSucceeded property={propertyId}" );

			var journal = Scene.GetAllComponents<Missions.MissionJournal>().FirstOrDefault();
			journal?.NotifyProgress( Missions.ObjectiveType.ClaimApartment, propertyId );
			return;
		}

		Log.Warning( $"UB.Property ClaimRejected property={propertyId} reason={result.Failure}" );
	}

	[Rpc.Host]
	public void RequestRentProperty( string propertyId )
	{
		var result = TryRentProperty( Rpc.Caller, propertyId );
		if ( result.Succeeded )
		{
			Log.Info( $"UB.Property RentSucceeded property={propertyId}" );
			return;
		}

		Log.Warning( $"UB.Property RentRejected property={propertyId} reason={result.Failure}" );
	}

	internal PropertyClaimResult TryClaimAbandonedShell( Connection caller, string propertyId )
	{
		if ( !Networking.IsHost || !EnsureReady() )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.ServiceNotReady, "The property service is not ready." );

		if ( caller is null || !caller.IsActive || !_identityProvider.TryResolve( caller, out var claimantIdentity ) )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.InvalidCaller, "The caller does not have a stable host-resolved identity." );

		if ( !_registry.TryGetValue( propertyId, out var property ) )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.InvalidProperty, "The requested property is not registered." );

		if ( property.PropertyType != PropertyType.AbandonedShell )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.WrongPropertyType, "This property is not an abandoned shell." );

		var player = FindPlayer( caller );
		if ( !player.IsValid() )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.PlayerNotFound, "The host could not resolve the caller's player." );

		var anchor = property.RespawnAnchor.IsValid() ? property.RespawnAnchor : property.GameObject;
		var distanceSquared = (player.WorldPosition - anchor.WorldPosition).LengthSquared;
		if ( distanceSquared > ClaimDistance * ClaimDistance )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.OutOfRange, "The host-resolved player is outside claim range." );

		// El kit de puerta ya se consumió al instalar la puerta física (DoorAnchor.ProcessInstall) —
		// el claim en sí no consume nada, solo exige que puerta y armario ya estén instalados.
		var door = Game.ActiveScene.GetAllComponents<DoorAnchor>()
			.Where( a => a.PropertyId == propertyId && a.HasDoor )
			.Select( a => a.DoorReference )
			.FirstOrDefault();
		if ( door is null )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.DoorNotInstalled, "No door has been installed on this property yet." );

		var cabinetAnchor = Game.ActiveScene.GetAllComponents<ClaimCabinetAnchor>()
			.FirstOrDefault( a => a.PropertyId == propertyId && a.HasCabinet );
		if ( cabinetAnchor is null )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.CabinetNotInstalled, "No claim cabinet has been installed on this property yet." );

		lock ( _claimGate )
		{
			if ( _propertiesInProgress.Contains( propertyId ) || _actorsInProgress.Contains( claimantIdentity.CanonicalId ) )
				return PropertyClaimResult.Rejected( PropertyClaimFailure.ClaimInProgress, "A claim for this property or player is already in progress." );

			if ( property.ClaimState != PropertyClaimState.Unclaimed )
				return PropertyClaimResult.Rejected( PropertyClaimFailure.PropertyUnavailable, "The property is no longer available." );

			_propertiesInProgress.Add( propertyId );
			_actorsInProgress.Add( claimantIdentity.CanonicalId );

			try
			{
				property.ApplyOwnership( claimantIdentity.CanonicalId, PropertyClaimState.Claimed );

				// Puerta y cerradura: rekey para el nuevo propietario — invalida cualquier
				// credencial que hubiera quedado de antes del reclamo.
				door.Rekey();
				door.SetLocked( true );

				// Habilitar build volume, stash y respawn ahora que la propiedad tiene dueño.
				if ( property.BuildVolume.IsValid() ) property.BuildVolume.Enabled = true;
				if ( property.Stash.IsValid() ) property.Stash.Enabled = true;
				if ( property.RespawnAnchor.IsValid() ) property.RespawnAnchor.Enabled = true;

				if ( !CommitSave() )
				{
					// Rollback completo: revertir el estado en memoria (la puerta/armario siguen
					// instalados — reclamarlos de nuevo no requiere gastar otro kit).
					property.ApplyOwnership( string.Empty, PropertyClaimState.Unclaimed );
					if ( property.BuildVolume.IsValid() ) property.BuildVolume.Enabled = false;
					if ( property.Stash.IsValid() ) property.Stash.Enabled = false;
					if ( property.RespawnAnchor.IsValid() ) property.RespawnAnchor.Enabled = false;
					return PropertyClaimResult.Rejected( PropertyClaimFailure.PersistenceFailed, "Failed to persist the claim." );
				}

				return PropertyClaimResult.Success( "Property claimed." );
			}
			finally
			{
				_propertiesInProgress.Remove( propertyId );
				_actorsInProgress.Remove( claimantIdentity.CanonicalId );
			}
		}
	}

	internal PropertyClaimResult TryRentProperty( Connection caller, string propertyId )
	{
		if ( !Networking.IsHost || !EnsureReady() )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.ServiceNotReady, "The property service is not ready." );

		if ( caller is null || !caller.IsActive || !_identityProvider.TryResolve( caller, out var tenantIdentity ) )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.InvalidCaller, "The caller does not have a stable host-resolved identity." );

		if ( !_registry.TryGetValue( propertyId, out var property ) )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.InvalidProperty, "The requested property is not registered." );

		if ( property.PropertyType != PropertyType.Rental )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.WrongPropertyType, "This property is not for rent." );

		var player = FindPlayer( caller );
		if ( !player.IsValid() )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.PlayerNotFound, "The host could not resolve the caller's player." );

		var wallet = player.GameObject.Components.GetInDescendantsOrSelf<Wallet>();
		if ( wallet is null )
			return PropertyClaimResult.Rejected( PropertyClaimFailure.PlayerNotFound, "The player has no wallet." );

		var totalCost = property.RentPrice + property.Deposit;

		lock ( _claimGate )
		{
			if ( _propertiesInProgress.Contains( propertyId ) )
				return PropertyClaimResult.Rejected( PropertyClaimFailure.ClaimInProgress, "A rental transaction for this property is already in progress." );

			if ( property.RentalState != RentalState.Vacant )
				return PropertyClaimResult.Rejected( PropertyClaimFailure.PropertyUnavailable, "The property is not vacant." );

			if ( !wallet.CanAfford( totalCost ) )
				return PropertyClaimResult.Rejected( PropertyClaimFailure.InsufficientFunds, $"Cannot afford deposit+rent ({totalCost})." );

			_propertiesInProgress.Add( propertyId );

			try
			{
				if ( !wallet.TryWithdraw( totalCost ) )
					return PropertyClaimResult.Rejected( PropertyClaimFailure.InsufficientFunds, "Withdrawal failed inside the claim gate." );

				// El periodo de gracia/renovación real (Tarea #24) decide cómo se recalcula NextRentAt;
				// aquí solo se fija el primer vencimiento a un ciclo de juego completo.
				const float firstRentCycleSeconds = 1800f;
				property.ApplyTenancy( tenantIdentity.CanonicalId, RentalState.Rented, firstRentCycleSeconds );
				IssueResidentCredential( property, player.GameObject, tenantIdentity.CanonicalId );

				if ( !CommitSave() )
				{
					wallet.Deposit( totalCost );
					property.ApplyTenancy( string.Empty, RentalState.Vacant, 0f );
					player.GameObject.Components.GetInDescendantsOrSelf<KeyringItem>()?.Revoke( propertyId );
					return PropertyClaimResult.Rejected( PropertyClaimFailure.PersistenceFailed, "Failed to persist the rental." );
				}

				return PropertyClaimResult.Success( "Property rented." );
			}
			finally
			{
				_propertiesInProgress.Remove( propertyId );
			}
		}
	}

	/// <summary>
	/// Otorga una credencial Resident para el arrendatario. No es la única vía
	/// de acceso (PropertyDoor.HasAccess ya concede paso directo a
	/// TenantPersistentId sin credencial) — esto es fidelidad al flujo del
	/// spec ("emitir credencial") y deja al inquilino con un ítem físico de
	/// llave, útil si algún día se separa el registro de tenancy del acceso
	/// físico. Si la propiedad aún no tiene puerta instalada, no falla el
	/// alquiler — simplemente no hay nada que abrir todavía.
	/// </summary>
	private static void IssueResidentCredential( PropertyComponent property, GameObject tenant, string tenantId )
	{
		var door = Game.ActiveScene.GetAllComponents<DoorAnchor>()
			.Where( a => a.PropertyId == property.PropertyId && a.HasDoor )
			.Select( a => a.DoorReference )
			.FirstOrDefault();

		if ( door is null )
			return;

		var keyring = tenant.Components.GetInDescendantsOrSelf<KeyringItem>() ?? tenant.Components.GetOrCreate<KeyringItem>();
		keyring.Grant( new AccessCredential
		{
			PropertyId = property.PropertyId,
			LockId = door.LockId,
			KeyRevision = door.KeyRevision,
			AccessLevel = AccessLevel.Resident,
			IssuerPersistentId = tenantId
		} );
	}

	private bool CommitSave()
	{
		var apartmentService = Scene.GetAllComponents<Apartments.ApartmentClaimService>().FirstOrDefault();
		if ( apartmentService is not null && apartmentService.IsReady )
			return apartmentService.TrySaveNow().Succeeded;

		Persistence.PersistenceBridge.RequestSave( "property_claim" );
		return true;
	}

	private bool EnsureReady()
	{
		if ( _isReady )
			return true;

		BuildRegistry();
		return _isReady;
	}

	private PlayerController FindPlayer( Connection connection )
	{
		return Scene.GetAllComponents<PlayerController>().FirstOrDefault(
			player => player.GameObject.Network.OwnerId == connection.Id );
	}

	// ── IPropertyAccessPolicy ──────────────────────────────────────────────

	public bool CanEnterProperty( string propertyId, string playerId ) => CheckAccess( propertyId, playerId );

	public bool CanAccessPropertyStash( string propertyId, string playerId ) => CheckAccess( propertyId, playerId );

	private bool CheckAccess( string propertyId, string playerId )
	{
		if ( _registry == null || !_registry.TryGetValue( propertyId, out var property ) )
			return true;

		var isOccupied = property.ClaimState == PropertyClaimState.Claimed || property.RentalState == RentalState.Rented;
		if ( !isOccupied )
			return true;

		if ( property.OwnerPersistentId == playerId || property.TenantPersistentId == playerId )
			return true;

		return property.CoOwners.Contains( playerId ) || property.Guests.Contains( playerId );
	}
}
