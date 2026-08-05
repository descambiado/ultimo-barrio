using System;

namespace UltimoBarrio.Combat
{
    /// <summary>
    /// Matemática pura de recarga: cuánta munición se mueve de la reserva al
    /// cargador y cuánta queda. Probable sin Scene.
    /// </summary>
    public static class ReloadMath
    {
        /// <summary>Devuelve (munición en cargador tras recargar, munición restante en reserva).</summary>
        public static (int Clip, int Reserve) Reload( int currentClip, int maxClip, int reserve )
        {
            if ( maxClip <= 0 )
                return ( Math.Max( 0, currentClip ), Math.Max( 0, reserve ) );

            int needed = maxClip - Math.Clamp( currentClip, 0, maxClip );
            int toTake = Math.Min( needed, Math.Max( 0, reserve ) );
            return ( Math.Clamp( currentClip, 0, maxClip ) + toTake, Math.Max( 0, reserve ) - toTake );
        }

        public static bool CanReload( int currentClip, int maxClip, int reserve )
            => currentClip < maxClip && reserve > 0;
    }
}
