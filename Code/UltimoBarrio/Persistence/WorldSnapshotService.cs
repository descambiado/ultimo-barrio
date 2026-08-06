// SPDX-License-Identifier: MPL-2.0

using System;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Combat;
using UltimoBarrio.Economy;
using UltimoBarrio.Fortification;
using UltimoBarrio.Missions;
using UltimoBarrio.Properties;
using UltimoBarrio.Properties.Doors;
using UltimoBarrio.Properties.Keys;
using UltimoBarrio.WorldTime;

namespace UltimoBarrio.Persistence;

/// <summary>
/// Captura y aplica las secciones del snapshot que no dependen del registro
/// de apartamentos: economía, reloj, fortificación, misiones y selección de
/// hotbar. Todo host-side; se llama desde ApartmentClaimService al guardar y
/// al cargar.
/// </summary>
public static class WorldSnapshotService
{
    // ── Captura ────────────────────────────────────────────────────────────

    public static void Capture( SaveSnapshot snapshot, Scene scene )
    {
        if ( snapshot is null || scene is null )
            return;

        CaptureEconomy( snapshot, scene );
        CaptureClock( snapshot, scene );
        CaptureFortifications( snapshot, scene );
        CaptureMissions( snapshot, scene );
        CapturePlayerStates( snapshot, scene );
        CaptureProperties( snapshot, scene );
        CaptureKeyrings( snapshot, scene );
    }

    private static void CaptureKeyrings( SaveSnapshot snapshot, Scene scene )
    {
        snapshot.Keyrings ??= [];

        foreach ( var keyring in scene.GetAllComponents<KeyringItem>() )
        {
            var playerKey = ResolvePlayerKey( keyring.GameObject );
            if ( string.IsNullOrEmpty( playerKey ) || keyring.Credentials.Count == 0 )
                continue;

            var data = new KeyringSaveData { PlayerKey = playerKey };
            foreach ( var credential in keyring.Credentials )
            {
                data.Credentials.Add( new AccessCredentialSaveData
                {
                    PropertyId = credential.PropertyId,
                    LockId = credential.LockId,
                    KeyRevision = credential.KeyRevision,
                    AccessLevel = credential.AccessLevel,
                    IssuerPersistentId = credential.IssuerPersistentId,
                    ExpiresAt = credential.ExpiresAt,
                    Stealable = credential.Stealable
                } );
            }

            var existing = snapshot.Keyrings.FirstOrDefault( k => k.PlayerKey == playerKey );
            if ( existing is not null )
                snapshot.Keyrings.Remove( existing );

            snapshot.Keyrings.Add( data );
        }
    }

    private static void CaptureProperties( SaveSnapshot snapshot, Scene scene )
    {
        snapshot.Properties ??= [];

        foreach ( var property in scene.GetAllComponents<PropertyComponent>() )
        {
            if ( string.IsNullOrEmpty( property.PropertyId ) )
                continue;

            var data = new PropertySaveData
            {
                PropertyId = property.PropertyId,
                PropertyType = property.PropertyType,
                OwnerPersistentId = property.OwnerPersistentId,
                TenantPersistentId = property.TenantPersistentId,
                CoOwners = property.CoOwners.ToList(),
                Guests = property.Guests.ToList(),
                RentalState = property.RentalState,
                NextRentAt = property.NextRentAt,
                ClaimState = property.ClaimState,
                UpgradeLevel = property.UpgradeLevel,
                SecurityLevel = property.SecurityLevel,
                DefenseScore = property.DefenseScore
            };

            foreach ( var anchor in scene.GetAllComponents<DoorAnchor>().Where( a => a.PropertyId == property.PropertyId ) )
            {
                if ( !anchor.HasDoor || !anchor.DoorReference.IsValid() )
                    continue;

                var door = anchor.DoorReference;
                data.Doors.Add( new PropertyDoorSaveData
                {
                    AnchorId = anchor.AnchorId,
                    Health = door.Health,
                    MaxHealth = door.MaxHealth,
                    UpgradeLevel = door.UpgradeLevel,
                    LockId = door.LockId,
                    KeyRevision = door.KeyRevision,
                    IsLocked = door.IsLocked
                } );
            }

            var existing = snapshot.Properties.FirstOrDefault( p => p.PropertyId == property.PropertyId );
            if ( existing is not null )
                snapshot.Properties.Remove( existing );

            snapshot.Properties.Add( data );
        }
    }

    private static void CaptureEconomy( SaveSnapshot snapshot, Scene scene )
    {
        snapshot.PlayersEconomy ??= [];

        foreach ( var wallet in scene.GetAllComponents<Wallet>() )
        {
            var playerKey = ResolvePlayerKey( wallet.GameObject );
            if ( string.IsNullOrEmpty( playerKey ) )
                continue;

            var existing = snapshot.PlayersEconomy.FirstOrDefault( e => e.PlayerId == playerKey );
            if ( existing is not null )
                existing.Balance = wallet.Balance;
            else
                snapshot.PlayersEconomy.Add( new PlayerEconomySaveData { PlayerId = playerKey, Balance = wallet.Balance } );
        }
    }

    private static void CaptureClock( SaveSnapshot snapshot, Scene scene )
    {
        var clock = scene.GetAllComponents<WorldClock>().FirstOrDefault();
        if ( clock is null )
            return;

        snapshot.Clock = new ClockSaveData
        {
            Phase = clock.CurrentPhase,
            RemainingSeconds = clock.TimeRemainingInPhase,
            LightLevel = clock.LightLevel
        };
    }

    private static void CaptureFortifications( SaveSnapshot snapshot, Scene scene )
    {
        snapshot.Fortifications ??= [];

        foreach ( var fortification in scene.GetAllComponents<ApartmentFortification>() )
        {
            var data = new FortificationSaveData
            {
                ApartmentId = fortification.ApartmentId,
                UpgradeLevel = fortification.UpgradeLevel,
                DoorHealth = fortification.DoorStructure.IsValid() ? fortification.DoorStructure.Health : 0f,
                DoorMaxHealth = fortification.DoorStructure.IsValid() ? fortification.DoorStructure.MaxHealth : 0f
            };

            foreach ( var anchor in scene.GetAllComponents<BarricadeAnchor>().Where( a => a.ApartmentId == fortification.ApartmentId ) )
            {
                if ( !anchor.HasBarricade || !anchor.BarricadeReference.IsValid() )
                    continue;

                data.Barricades.Add( new BarricadeSaveData
                {
                    AnchorId = anchor.AnchorId,
                    Health = anchor.BarricadeReference.Health,
                    MaxHealth = anchor.BarricadeReference.MaxHealth
                } );
            }

            snapshot.Fortifications.Add( data );
        }
    }

    private static void CaptureMissions( SaveSnapshot snapshot, Scene scene )
    {
        snapshot.Missions ??= [];

        var journal = scene.GetAllComponents<MissionJournal>().FirstOrDefault();
        if ( journal is null )
            return;

        foreach ( var mission in journal.ActiveMissions )
        {
            var data = new MissionSaveData { MissionId = mission.MissionId };
            foreach ( var objective in mission.Objectives )
            {
                data.Objectives.Add( new ObjectiveSaveData
                {
                    Id = objective.Id,
                    Progress = objective.CurrentProgress,
                    Completed = objective.IsCompleted
                } );
            }
            snapshot.Missions.Add( data );
        }
    }

    private static void CapturePlayerStates( SaveSnapshot snapshot, Scene scene )
    {
        snapshot.PlayerStates ??= [];

        foreach ( var held in scene.GetAllComponents<HeldItemController>() )
        {
            var playerKey = ResolvePlayerKey( held.GameObject );
            if ( string.IsNullOrEmpty( playerKey ) )
                continue;

            snapshot.PlayerStates.Add( new PlayerStateSaveData
            {
                PlayerKey = playerKey,
                SelectedHotbarSlot = held.SelectedHotbarSlot
            } );
        }
    }

    // ── Aplicación ─────────────────────────────────────────────────────────

    public static void Apply( SaveSnapshot snapshot, Scene scene )
    {
        if ( snapshot is null || scene is null )
            return;

        ApplyEconomy( snapshot, scene );
        ApplyClock( snapshot, scene );
        ApplyFortifications( snapshot, scene );
        ApplyMissions( snapshot, scene );
        ApplyPlayerStates( snapshot, scene );
        ApplyProperties( snapshot, scene );
        ApplyKeyrings( snapshot, scene );
    }

    private static void ApplyKeyrings( SaveSnapshot snapshot, Scene scene )
    {
        if ( snapshot.Keyrings is null )
            return;

        foreach ( var data in snapshot.Keyrings )
        {
            var keyring = scene.GetAllComponents<KeyringItem>()
                .FirstOrDefault( k => ResolvePlayerKey( k.GameObject ) == data.PlayerKey );

            if ( keyring is null )
                continue;

            keyring.Credentials.Clear();
            foreach ( var credentialData in data.Credentials ?? [] )
            {
                keyring.Credentials.Add( new AccessCredential
                {
                    PropertyId = credentialData.PropertyId,
                    LockId = credentialData.LockId,
                    KeyRevision = credentialData.KeyRevision,
                    AccessLevel = credentialData.AccessLevel,
                    IssuerPersistentId = credentialData.IssuerPersistentId,
                    ExpiresAt = credentialData.ExpiresAt,
                    Stealable = credentialData.Stealable
                } );
            }
        }
    }

    private static void ApplyProperties( SaveSnapshot snapshot, Scene scene )
    {
        if ( snapshot.Properties is null )
            return;

        var sceneProperties = scene.GetAllComponents<PropertyComponent>()
            .Where( p => !string.IsNullOrEmpty( p.PropertyId ) )
            .ToDictionary( p => p.PropertyId );

        foreach ( var data in snapshot.Properties )
        {
            if ( !sceneProperties.TryGetValue( data.PropertyId, out var property ) )
                continue;

            property.ApplyOwnership( data.OwnerPersistentId, data.ClaimState );
            property.ApplyTenancy( data.TenantPersistentId, data.RentalState, data.NextRentAt );
            property.ApplyProgression( data.UpgradeLevel, data.SecurityLevel, data.DefenseScore );

            property.CoOwners.Clear();
            foreach ( var coOwner in data.CoOwners ?? [] )
                property.CoOwners.Add( coOwner );

            property.Guests.Clear();
            foreach ( var guest in data.Guests ?? [] )
                property.Guests.Add( guest );

            foreach ( var doorData in data.Doors ?? [] )
            {
                var anchor = scene.GetAllComponents<DoorAnchor>()
                    .FirstOrDefault( a => a.PropertyId == data.PropertyId && a.AnchorId == doorData.AnchorId );

                anchor?.RestoreDoor( doorData.Health, doorData.MaxHealth, doorData.UpgradeLevel, doorData.LockId, doorData.KeyRevision, doorData.IsLocked );
            }
        }
    }

    private static void ApplyEconomy( SaveSnapshot snapshot, Scene scene )
    {
        foreach ( var wallet in scene.GetAllComponents<Wallet>() )
        {
            var playerKey = ResolvePlayerKey( wallet.GameObject );
            if ( string.IsNullOrEmpty( playerKey ) )
                continue;

            var saved = snapshot.PlayersEconomy?.FirstOrDefault( e => e.PlayerId == playerKey );
            if ( saved is not null )
                wallet.LoadData( saved.Balance );
        }
    }

    private static void ApplyClock( SaveSnapshot snapshot, Scene scene )
    {
        var clock = scene.GetAllComponents<WorldClock>().FirstOrDefault();
        if ( clock is null || snapshot.Clock is null )
            return;

        clock.RestoreState( snapshot.Clock.Phase, snapshot.Clock.RemainingSeconds, snapshot.Clock.LightLevel );
    }

    private static void ApplyFortifications( SaveSnapshot snapshot, Scene scene )
    {
        foreach ( var data in snapshot.Fortifications ?? [] )
        {
            var fortification = scene.GetAllComponents<ApartmentFortification>()
                .FirstOrDefault( f => f.ApartmentId == data.ApartmentId );

            if ( fortification is null )
                continue;

            fortification.RestoreLevel( data.UpgradeLevel, data.DoorHealth, data.DoorMaxHealth );

            foreach ( var barricade in data.Barricades ?? [] )
            {
                var anchor = scene.GetAllComponents<BarricadeAnchor>()
                    .FirstOrDefault( a => a.ApartmentId == data.ApartmentId && a.AnchorId == barricade.AnchorId );

                anchor?.RestoreBarricade( barricade.Health, barricade.MaxHealth );
            }
        }
    }

    private static void ApplyMissions( SaveSnapshot snapshot, Scene scene )
    {
        var journal = scene.GetAllComponents<MissionJournal>().FirstOrDefault();
        if ( journal is null )
            return;

        journal.Restore( snapshot.Missions );
    }

    private static void ApplyPlayerStates( SaveSnapshot snapshot, Scene scene )
    {
        foreach ( var held in scene.GetAllComponents<HeldItemController>() )
        {
            var playerKey = ResolvePlayerKey( held.GameObject );
            if ( string.IsNullOrEmpty( playerKey ) )
                continue;

            var saved = snapshot.PlayerStates?.FirstOrDefault( p => p.PlayerKey == playerKey );
            if ( saved is not null )
                held.RestoreSelection( saved.SelectedHotbarSlot );
        }
    }

    // ── Utilidades ─────────────────────────────────────────────────────────

    /// <summary>Clave estable del jugador: el InventoryId canónico de su pawn.</summary>
    public static string ResolvePlayerKey( GameObject playerGo )
    {
        var inventory = playerGo?.Components.GetInDescendantsOrSelf<InventoryComponent>();
        return inventory is not null && !string.IsNullOrEmpty( inventory.InventoryId )
            ? inventory.InventoryId
            : playerGo?.Id.ToString() ?? "";
    }
}
