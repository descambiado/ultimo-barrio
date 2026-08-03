using Sandbox;
using System;

namespace UltimoBarrio.Combat
{
    public class PistolWeapon : BaseCombatWeapon
    {
        protected override void OnStart()
        {
            base.OnStart();
            BaseDamage = 15f;
            FireRate = 0.25f;
            MaxAmmo = 12;
            ReloadTime = 1.5f;
            IsAutomatic = false;
        }

        [Broadcast]
        protected override void DoFireEffects()
        {
            base.DoFireEffects();
            // Implement pistol specific effects
            Log.Info("Pistol fired");
        }
    }
}
