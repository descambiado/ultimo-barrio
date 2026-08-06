// SPDX-License-Identifier: MPL-2.0

using System;
using System.Linq;
using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Players;
using UltimoBarrio.Properties.Keys;

namespace UltimoBarrio.Properties.Doors;

/// <summary>
/// Puerta física de una propiedad. Usa Sandbox.Mapping.Door como comportamiento
/// físico base (abrir/cerrar/animación/sonido) — esto solo añade salud,
/// mejora, daño/reparación/breach y delega el estado de cerradura a
/// DoorLockComponent (mismo GameObject).
/// </summary>
[Title( "Property Door" )]
[Category( "Último Barrio — Properties" )]
[Icon( "door_front" )]
public sealed class PropertyDoor : Component, IDamageable, IWorldInteractable, IInteractable
{
	[Property] public string PropertyId { get; set; } = string.Empty;

	[Property] public string AnchorId { get; set; } = string.Empty;

	[Property] public string DoorId { get; set; } = "wooden_door";

	[Sync( SyncFlags.FromHost )] public float Health { get; private set; } = 250f;

	[Sync( SyncFlags.FromHost )] public float MaxHealth { get; private set; } = 250f;

	[Sync( SyncFlags.FromHost )] public int UpgradeLevel { get; private set; }

	public bool IsDead => Health <= 0f;

	public bool IsLocked => _lock?.IsLocked ?? false;

	public string LockId => _lock?.LockId ?? string.Empty;

	public int KeyRevision => _lock?.KeyRevision ?? 0;

	private Sandbox.Mapping.Door _physicalDoor;
	private DoorLockComponent _lock;

	protected override void OnStart()
	{
		_physicalDoor = Components.Get<Sandbox.Mapping.Door>();
		_lock = Components.GetOrCreate<DoorLockComponent>();

		if ( Health <= 0f )
			Health = MaxHealth;
	}

	public void Open() => _physicalDoor?.Open();

	public void Close() => _physicalDoor?.Close();

	public void Toggle() => _physicalDoor?.Toggle();

	internal void SetLocked( bool locked ) => _lock?.SetLocked( locked );

	internal void Rekey() => _lock?.Rekey();

	// ── IWorldInteractable ─────────────────────────────────────────────────
	// E: si está bloqueada, exige acceso (owner/co-owner/tenant/guest directo
	// en PropertyComponent, o una AccessCredential vigente en el llavero del
	// interactor) y desbloquea antes de abrir; si ya está desbloqueada, abre o
	// cierra libremente — igual que una puerta real una vez tiene la llave echada.

	public string GetInteractionPrompt( InteractionRequest request )
	{
		if ( !IsLocked )
			return "Abrir/cerrar puerta";

		return HasAccess( request.InteractorObject, AccessLevel.Guest )
			? "Desbloquear y abrir"
			: "Puerta bloqueada";
	}

	public bool CanInteract( InteractionRequest request ) => request.InteractorObject is not null;

	public void OnInteract( InteractionRequest request )
	{
		if ( IsProxy )
		{
			RequestInteractOnHost( request.InteractorObject?.Id ?? Guid.Empty );
			return;
		}

		ProcessInteract( request.InteractorObject );
	}

	[Rpc.Host]
	private void RequestInteractOnHost( Guid interactorObjectId )
	{
		var interactor = Scene.Directory.FindByGuid( interactorObjectId );
		ProcessInteract( interactor );
	}

	private void ProcessInteract( GameObject interactor )
	{
		if ( interactor is null || !Networking.IsHost )
			return;

		if ( !IsLocked )
		{
			Toggle();
			return;
		}

		if ( !HasAccess( interactor, AccessLevel.Guest ) )
		{
			UI.PlayerFeedback.Push( "No tienes acceso a esta propiedad" );
			return;
		}

		SetLocked( false );
		Open();
	}

	private bool HasAccess( GameObject interactor, AccessLevel minLevel )
	{
		if ( interactor is null )
			return false;

		var property = Scene.GetAllComponents<PropertyComponent>().FirstOrDefault( p => p.PropertyId == PropertyId );
		if ( property is null )
			return false;

		var identity = PlayerIdentity.FromGameObject( interactor );
		if ( identity.IsValid )
		{
			if ( property.OwnerPersistentId == identity.CanonicalId || property.TenantPersistentId == identity.CanonicalId )
				return true;

			if ( property.CoOwners.Contains( identity.CanonicalId ) || property.Guests.Contains( identity.CanonicalId ) )
				return true;
		}

		var keyring = interactor.Components.GetInDescendantsOrSelf<KeyringItem>();
		return keyring is not null && keyring.HasAccess( PropertyId, LockId, KeyRevision, minLevel );
	}

	public void TakeDamage( DamageEvent damageEvent )
	{
		if ( !Networking.IsHost || IsDead )
			return;

		Health = MathF.Max( 0f, Health - damageEvent.Amount );

		if ( IsDead )
			Breach();
	}

	public void Repair( float amount )
	{
		if ( !Networking.IsHost || amount <= 0f )
			return;

		Health = MathF.Min( MaxHealth, Health + amount );
	}

	public bool TryUpgrade( DoorDefinition definition )
	{
		if ( !Networking.IsHost || definition is null )
			return false;

		if ( UpgradeLevel >= definition.MaxUpgradeLevel )
			return false;

		UpgradeLevel++;
		var healthDelta = Health - MaxHealth;
		MaxHealth = definition.MaxHealth + definition.UpgradeHealthBonus * UpgradeLevel;
		Health = Math.Clamp( MaxHealth + healthDelta, 1f, MaxHealth );
		return true;
	}

	/// <summary>Forzar apertura y desbloqueo tras alcanzar Health 0 (raid/asalto). No destruye el GameObject — la puerta sigue reparable.</summary>
	private void Breach()
	{
		_physicalDoor?.Open();
		SetLocked( false );
		Log.Info( $"UB.Property DoorBreached property={PropertyId} anchor={AnchorId}" );
	}

	/// <summary>Restaura el estado guardado (host, sin disparar Breach aunque Health llegue a 0 desde disco).</summary>
	internal void RestoreState( float health, float maxHealth, int upgradeLevel, string lockId, int keyRevision, bool isLocked )
	{
		Health = health;
		MaxHealth = maxHealth > 0f ? maxHealth : MaxHealth;
		UpgradeLevel = upgradeLevel;
		_lock ??= Components.GetOrCreate<DoorLockComponent>();
		_lock.RestoreLock( lockId, keyRevision, isLocked );
	}
}
