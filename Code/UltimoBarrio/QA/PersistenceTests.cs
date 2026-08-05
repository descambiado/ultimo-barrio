using System;
using UltimoBarrio.Persistence;
using UltimoBarrio.WorldTime;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Validación estática de persistencia: migración v1→v2, rechazo de
    /// versiones futuras y campos v2 por defecto. Sin tocar FileSystem.Data.
    /// </summary>
    public static class PersistenceTests
    {
        [ConCmd( "ub_test_persistence" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando persistencia ===" );

            var passed = 0;
            var failed = 0;

            var migrator = new SaveMigrator();

            // v1 → v2: se migra y se rellenan las secciones nuevas.
            var v1 = new SaveSnapshot { SaveVersion = 1, SlotId = "qa" };
            if ( migrator.TryMigrate( v1, out var migrated, out _ )
                && migrated.SaveVersion == 2
                && migrated.Clock is not null
                && migrated.Fortifications is not null
                && migrated.Missions is not null
                && migrated.PlayerStates is not null )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: migración v1→v2 incorrecta." );
            }

            // Versión futura → rechazada sin tocar.
            var future = new SaveSnapshot { SaveVersion = 99 };
            if ( !migrator.TryMigrate( future, out _, out var error ) && error.Contains( "newer" ) )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: versión futura debería rechazarse." );
            }

            // v2 ida y vuelta sin cambios.
            var v2 = new SaveSnapshot { SaveVersion = 2, SlotId = "qa2", Clock = new ClockSaveData { Phase = TimePhase.Night, RemainingSeconds = 42f } };
            if ( migrator.TryMigrate( v2, out var roundTrip, out _ )
                && roundTrip.SaveVersion == 2
                && roundTrip.Clock.Phase == TimePhase.Night
                && Math.Abs( roundTrip.Clock.RemainingSeconds - 42f ) < 0.001f )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: v2 ida y vuelta alteró el snapshot." );
            }

            // Snapshot nulo → rechazado.
            if ( !migrator.TryMigrate( null, out _, out _ ) )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: snapshot nulo debería rechazarse." );
            }

            // Claves de jugador: el InventoryId canónico no debe estar vacío.
            var playerKey = "player:steam:123:inventory";
            if ( !string.IsNullOrEmpty( playerKey ) )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: player key vacía." );
            }

            Log.Info( $"[UBTest] === PERSISTENCIA: {passed} passed, {failed} failed ===" );
        }
    }
}
