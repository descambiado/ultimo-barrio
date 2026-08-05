using System;

namespace UltimoBarrio.Combat
{
    /// <summary>
    /// Bus de ruido de armas: cada disparo emite un evento posicional que la IA
    /// (PerceptionComponent) escucha para investigar. Separado del audio para
    /// que los NPC no dependan de assets de sonido.
    /// </summary>
    public static class WeaponNoise
    {
        public static event Action<Vector3, float> OnNoise;

        public static void Emit( Vector3 position, float radius )
        {
            OnNoise?.Invoke( position, radius );
        }
    }
}
