using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.World;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Validación estática del sistema de loot. Solo consulta tablas y
    /// registro: no genera ítems falsos.
    /// </summary>
    public static class LootTests
    {
        [ConCmd( "ub_test_loot" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando loot ===" );

            var passed = 0;
            var failed = 0;

            var tableIds = LootTables.TableIds.ToList();
            var expected = new[] { "StreetLoot", "ApartmentLoot", "ShopLoot", "MedicalLoot", "DangerousLoot" };

            foreach ( var id in expected )
            {
                if ( tableIds.Contains( id ) ) passed++;
                else { failed++; Log.Error( $"[UBTest] FAIL: falta la tabla '{id}'." ); }
            }

            var errors = LootTables.Validate();
            foreach ( var error in errors )
            {
                failed++;
                Log.Error( $"[UBTest] FAIL: {error}" );
            }
            if ( errors.Count == 0 ) passed++;

            // Todas las tablas tienen al menos una entrada con peso > 0.
            foreach ( var id in tableIds )
            {
                if ( LootTables.TryGet( id, out var entries ) && entries.Count > 0
                    && entries.Any( e => e.Weight > 0f ) )
                    passed++;
                else
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: tabla '{id}' vacía o sin pesos." );
                }
            }

            Log.Info( $"[UBTest] === LOOT: {passed} passed, {failed} failed ===" );
        }
    }
}
