// SPDX-License-Identifier: MPL-2.0

using System.Linq;
using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Economy;
using UltimoBarrio.Players;
using UltimoBarrio.Properties.Keys;

namespace UltimoBarrio.Properties;

/// <summary>
/// Ciclo de vida del alquiler tras el "rent-now" inicial de
/// PropertyClaimService: renovación, impago → periodo de gracia → desahucio,
/// y abandono voluntario con devolución parcial de depósito. Host-only,
/// tick periódico igual que AutoSaveManager/WorldClock.
/// </summary>
[Title( "Rental Service" )]
[Category( "Último Barrio — Properties" )]
[Icon( "event_repeat" )]
public sealed class RentalService : Component
{
	/// <summary>Duración de un ciclo de renta y del periodo de gracia, en segundos de Time.Now.</summary>
	[Property] public float RentCycleSeconds { get; set; } = 1800f;

	[Property] public float GracePeriodSeconds { get; set; } = 300f;

	/// <summary>Fracción del depósito devuelta al abandonar voluntariamente (el resto cubre desgaste).</summary>
	[Property] public float VoluntaryAbandonRefundFraction { get; set; } = 0.5f;

	private TimeSince _timeSinceTick;
	private IPlayerIdentityProvider _identityProvider;

	protected override void OnStart()
	{
		_identityProvider ??= new SteamPlayerIdentityProvider();
		_timeSinceTick = 0f;
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( _timeSinceTick < 5f )
			return;

		_timeSinceTick = 0f;
		TickAllRentals();
	}

	private void TickAllRentals()
	{
		var now = Time.Now;

		foreach ( var property in Scene.GetAllComponents<PropertyComponent>().Where( p => p.PropertyType == PropertyType.Rental ) )
		{
			if ( property.NextRentAt <= 0f || property.NextRentAt > now )
				continue;

			if ( property.RentalState == RentalState.Rented )
			{
				property.ApplyTenancy( property.TenantPersistentId, RentalState.GracePeriod, now + GracePeriodSeconds );
				Log.Info( $"UB.Property GracePeriodStarted property={property.PropertyId}" );
				Persistence.PersistenceBridge.RequestSave( "rental_grace" );
			}
			else if ( property.RentalState == RentalState.GracePeriod )
			{
				Evict( property );
			}
		}
	}

	[Rpc.Host]
	public void RequestRenewRental( string propertyId )
	{
		if ( !Networking.IsHost )
			return;

		var property = Scene.GetAllComponents<PropertyComponent>().FirstOrDefault( p => p.PropertyId == propertyId );
		if ( property is null || property.PropertyType != PropertyType.Rental )
			return;

		if ( Rpc.Caller is null || !_identityProvider.TryResolve( Rpc.Caller, out var identity ) )
			return;

		if ( property.TenantPersistentId != identity.CanonicalId )
		{
			Log.Warning( $"UB.Property RenewRejected property={propertyId} reason=CallerIsNotTenant" );
			return;
		}

		var player = FindPlayer( Rpc.Caller );
		var wallet = player?.GameObject.Components.GetInDescendantsOrSelf<Wallet>();
		if ( wallet is null || !wallet.TryWithdraw( property.RentPrice ) )
		{
			UI.PlayerFeedback.Push( "No tienes fondos para renovar el alquiler" );
			return;
		}

		property.ApplyTenancy( property.TenantPersistentId, RentalState.Rented, Time.Now + RentCycleSeconds );
		Persistence.PersistenceBridge.RequestSave( "rental_renew" );
		UI.PlayerFeedback.Push( "Alquiler renovado" );
		Log.Info( $"UB.Property RentalRenewed property={propertyId}" );
	}

	[Rpc.Host]
	public void RequestAbandonRental( string propertyId )
	{
		if ( !Networking.IsHost )
			return;

		var property = Scene.GetAllComponents<PropertyComponent>().FirstOrDefault( p => p.PropertyId == propertyId );
		if ( property is null || property.PropertyType != PropertyType.Rental )
			return;

		if ( Rpc.Caller is null || !_identityProvider.TryResolve( Rpc.Caller, out var identity ) )
			return;

		if ( property.TenantPersistentId != identity.CanonicalId )
		{
			Log.Warning( $"UB.Property AbandonRejected property={propertyId} reason=CallerIsNotTenant" );
			return;
		}

		var player = FindPlayer( Rpc.Caller );
		var wallet = player?.GameObject.Components.GetInDescendantsOrSelf<Wallet>();
		var refund = (int)(property.Deposit * VoluntaryAbandonRefundFraction);
		wallet?.Deposit( refund );

		ClearTenancy( property );
		Persistence.PersistenceBridge.RequestSave( "rental_abandon" );
		UI.PlayerFeedback.Push( $"Abandonaste el alquiler — reembolso ${refund}" );
		Log.Info( $"UB.Property RentalAbandoned property={propertyId} refund={refund}" );
	}

	private void Evict( PropertyComponent property )
	{
		var evictedTenant = property.TenantPersistentId;
		ClearTenancy( property );
		Persistence.PersistenceBridge.RequestSave( "rental_evict" );
		Log.Info( $"UB.Property TenantEvicted property={property.PropertyId} tenant={evictedTenant}" );
	}

	private void ClearTenancy( PropertyComponent property )
	{
		property.ApplyTenancy( string.Empty, RentalState.Vacant, 0f );

		foreach ( var keyring in Scene.GetAllComponents<KeyringItem>() )
			keyring.Revoke( property.PropertyId );
	}

	private PlayerController FindPlayer( Connection connection )
	{
		return Scene.GetAllComponents<PlayerController>().FirstOrDefault(
			player => player.GameObject.Network.OwnerId == connection.Id );
	}
}
