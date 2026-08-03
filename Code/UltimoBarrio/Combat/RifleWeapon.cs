using Sandbox;
using System;

namespace UltimoBarrio.Combat
{
    public class RifleWeapon : BaseCombatWeapon
    {
        protected override void OnStart()
        {
            base.OnStart();
            BaseDamage = 25f;
            FireRate = 0.1f;
            MaxAmmo = 30;
            ReloadTime = 2.5f;
            IsAutomatic = true;
        }

        [Rpc.Broadcast]
        protected override void DoFireEffects()
        {
            base.DoFireEffects();
            // Implement rifle specific effects
            Log.Info("Rifle fired");
        }
    }
}
