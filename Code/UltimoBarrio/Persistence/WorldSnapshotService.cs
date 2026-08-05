// SPDX-License-Identifier: MPL-2.0

using System;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Combat;
using UltimoBarrio.Economy;
using UltimoBarrio.Fortification;
using UltimoBarrio.Missions;
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
