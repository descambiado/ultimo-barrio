using UltimoBarrio.Players;

namespace UltimoBarrio.QA
{
    /// <summary>Validación estática del movimiento táctico (lógica pura).</summary>
    public static class MovementTests
    {
        [ConCmd( "ub_test_movement" )]
        public static void Run()
        {
            Log.Info( "[UBTest] === Validando movimiento ===" );

            var passed = 0;
            var failed = 0;

            // Stamina clamp: sprint agota hasta 0, nunca negativo.
            var drained = StaminaMath.Step( 10f, 100f, 20f, 15f, sprinting: true, 1f );
            if ( drained == 0f ) passed++;
            else { failed++; Log.Error( $"[UBTest] FAIL: stamina sprint debería llegar a 0 (era {drained})." ); }

            // Regeneración nunca supera Max.
            var regen = StaminaMath.Step( 99f, 100f, 20f, 15f, sprinting: false, 10f );
            if ( regen == 100f ) passed++;
            else { failed++; Log.Error( $"[UBTest] FAIL: stamina regen debería clamar a 100 (era {regen})." ); }

            // Clamp superior en sprint imposible: entrada ya por encima.
            var over = StaminaMath.Step( 120f, 100f, 20f, 15f, sprinting: true, 0.1f );
            if ( over <= 100f ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: stamina nunca supera Max." ); }

            // Agotado → no puede sprintar.
            if ( !StaminaMath.CanSprint( 0f ) ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: CanSprint(0) debería ser false." ); }

            // Salto requiere coste.
            if ( StaminaMath.CanJump( 10f, 15f ) == false && StaminaMath.CanJump( 20f, 15f ) == true ) passed++;
            else { failed++; Log.Error( "[UBTest] FAIL: CanJump no respeta el coste." ); }

            Log.Info( $"[UBTest] === MOVIMIENTO: {passed} passed, {failed} failed ===" );
        }
    }
}
