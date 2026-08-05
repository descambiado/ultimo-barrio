using System;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Lógica pura de estructuras fortificables (probable sin Scene):
    /// daño clampado, reparación y costes de mejora por nivel.
    /// </summary>
    public static class FortificationMath
    {
        public static float ApplyDamage( float health, float amount, float maxHealth )
        {
            return Math.Clamp( health - amount, 0f, Math.Max( 0f, maxHealth ) );
        }

        public static float ApplyRepair( float health, float amount, float maxHealth )
        {
            return Math.Clamp( health + amount, 0f, Math.Max( 0f, maxHealth ) );
        }

        /// <summary>Coste de mejora por nivel (1..3): madera, chatarra, componentes.</summary>
        public static bool TryGetUpgradeCost( int currentLevel, out int wood, out int scrap, out int components )
        {
            wood = 0;
            scrap = 0;
            components = 0;

            int nextLevel = currentLevel + 1;
            switch ( nextLevel )
            {
                case 1:
                    wood = 5; scrap = 5; components = 0;
                    return true;
                case 2:
                    wood = 10; scrap = 10; components = 3;
                    return true;
                case 3:
                    wood = 15; scrap = 15; components = 5;
                    return true;
                default:
                    return false; // Nivel máximo.
            }
        }

        public const int MaxUpgradeLevel = 3;

        /// <summary>Bonus de salud máxima por nivel de mejora (multiplicador).</summary>
        public static float MaxHealthMultiplier( int upgradeLevel )
            => 1f + 0.25f * Math.Clamp( upgradeLevel, 0, MaxUpgradeLevel );
    }
}
