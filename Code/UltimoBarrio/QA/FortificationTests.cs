using UltimoBarrio.Fortification;

namespace UltimoBarrio.QA
{
    /// <summary>Validación estática de fortificación (lógica pura).</summary>
    public static class FortificationTests
    {
        [ConCmd( "ub_test_fortification" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando fortificación ===" );

            var passed = 0;
            var failed = 0;

            // Daño clampado: nunca baja de 0.
            var damaged = FortificationMath.ApplyDamage( 50f, 200f, 200f );
            if ( damaged == 0f ) passed++;
            else { failed++; Log.Error( $"[UBTest] FAIL: daño debería clamar a 0 (era {damaged})." ); }

            // Reparación clampada: nunca supera Max.
            var repaired = FortificationMath.ApplyRepair( 190f, 100f, 200f );
            if ( repaired == 200f ) passed++;
            else { failed++; Log.Error( $"[UBTest] FAIL: reparación debería clamar a Max (era {repaired})." ); }

            // Costes de mejora por nivel.
            if ( FortificationMath.TryGetUpgradeCost( 0, out var w1, out var s1, out var c1 ) && w1 == 5 && s1 == 5 && c1 == 0 ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: coste nivel 1 incorrecto." ); }

            if ( FortificationMath.TryGetUpgradeCost( 3, out _, out _, out _ ) == false ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: nivel 4 no debería existir." ); }

            // Multiplicador de salud por nivel.
            if ( FortificationMath.MaxHealthMultiplier( 0 ) == 1f
                && FortificationMath.MaxHealthMultiplier( 3 ) == 1.75f
                && FortificationMath.MaxHealthMultiplier( 9 ) == 1.75f )
                passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: multiplicador de salud incorrecto." ); }

            Log.Info( $"[UBTest] === FORTIFICACIÓN: {passed} passed, {failed} failed ===" );
        }
    }
}
