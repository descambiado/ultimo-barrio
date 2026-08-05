using System;

namespace UltimoBarrio.Players
{
    /// <summary>
    /// Lógica pura de resistencia (probable sin Scene): drena durante sprint,
    /// regenera en reposo y siempre se mantiene en [0, Max]. Agotado (0) no
    /// puede sprintar ni saltar (el consumidor lo decide).
    /// </summary>
    public static class StaminaMath
    {
        public static float Step( float current, float max, float drainRate, float regenRate, bool sprinting, float deltaTime )
        {
            if ( max <= 0f || deltaTime <= 0f )
                return Math.Clamp( current, 0f, Math.Max( 0f, max ) );

            if ( sprinting )
                return Math.Clamp( current - drainRate * deltaTime, 0f, max );

            return Math.Clamp( current + regenRate * deltaTime, 0f, max );
        }

        public static bool CanSprint( float current ) => current > 0f;

        public static bool CanJump( float current, float jumpCost ) => current >= jumpCost;
    }
}
