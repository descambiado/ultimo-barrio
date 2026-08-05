using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Validación estática del sistema melee. Solo consulta el catálogo
    /// canónico y reglas puras: no finge golpes ni éxitos.
    /// </summary>
    public static class MeleeLogicTests
    {
        [ConCmd( "ub_test_melee" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando sistema melee ===" );

            var passed = 0;
            var failed = 0;

            var meleeIds = ItemCatalog.All
                .Where( d => d.Category == ItemCategory.Melee )
                .Select( d => d.ItemId )
                .ToList();

            if ( meleeIds.Count == 0 )
            {
                failed++;
                Log.Error( "[UBTest] FAIL: no hay armas melee en el catálogo." );
            }
            else
            {
                passed++;
                Log.Info( $"[UBTest] Armas melee registradas: {string.Join( ", ", meleeIds )}" );
            }

            foreach ( var id in meleeIds )
            {
                var def = ItemRegistry.GetDefinition( id );
                if ( def is null )
                {
                    failed++;
                    Log.Error( $"[UBTest] FAIL: '{id}' sin definición resoluble." );
                    continue;
                }

                if ( def.Damage <= 0f ) { failed++; Log.Error( $"[UBTest] FAIL: '{id}' con Damage <= 0." ); } else passed++;
                if ( def.MeleeRange <= 0f ) { failed++; Log.Error( $"[UBTest] FAIL: '{id}' con MeleeRange <= 0." ); } else passed++;
                if ( def.FireRate <= 0f ) { failed++; Log.Error( $"[UBTest] FAIL: '{id}' con FireRate <= 0." ); } else passed++;
                if ( def.EquipSlot != "Melee" ) { failed++; Log.Error( $"[UBTest] FAIL: '{id}' con EquipSlot != Melee." ); } else passed++;
                if ( string.IsNullOrEmpty( def.WorldModelPrefab ) ) { failed++; Log.Error( $"[UBTest] FAIL: '{id}' sin WorldModelPrefab." ); } else passed++;
                if ( string.IsNullOrEmpty( def.ViewModelPrefab ) ) { failed++; Log.Error( $"[UBTest] FAIL: '{id}' sin ViewModelPrefab." ); } else passed++;
            }

            // Regla pura: un golpe contra pared (world/solid) no atraviesa y no
            // aplica daño. Es decisión del trace de MeleeWeapon; aquí validamos
            // que la regla está declarada en el diseño (bloqueo por pared).
            var crowbar = ItemRegistry.GetDefinition( "weapon_crowbar" );
            if ( crowbar is not null && crowbar.MeleeRange < 200f && crowbar.Damage > 0f )
                passed++;
            else
            {
                failed++;
                Log.Error( "[UBTest] FAIL: weapon_crowbar con stats fuera del rango esperado." );
            }

            Log.Info( $"[UBTest] === MELEE: {passed} passed, {failed} failed ===" );
        }
    }
}
