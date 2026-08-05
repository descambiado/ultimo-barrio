using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Combat
{
    /// <summary>
    /// Puños: arma melee base que vive en el pawn y solo se activa cuando las
    /// manos están vacías. El HeldItemController invoca TrySwing directamente
    /// (flujo: input → HeldItemController → MeleeWeapon → trace → HealthComponent).
    /// </summary>
    [Title( "Fists" )]
    [Category( "Último Barrio — Combat" )]
    [Icon( "sports_martial_arts" )]
    public sealed class FistsWeapon : MeleeWeapon
    {
        public FistsWeapon()
        {
            BaseDamage = 6f;
            FireRate = 0.4f;
            Range = 70f;
            StaminaCost = 5f;
            PushForce = 120f;
        }

        protected override void HandleInput()
        {
            // Los puños NO escuchan input propio: los dispara el HeldItemController
            // cuando el slot seleccionado está vacío.
        }
    }
}
