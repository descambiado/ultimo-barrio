using Sandbox;
using System;

namespace UltimoBarrio.Combat
{
    [Title("Melee Weapon")]
    [Category("Último Barrio — Combat")]
    [Icon("pan_tool")]
    public class MeleeWeapon : BaseCombatWeapon
    {
        protected override void OnStart()
        {
            base.OnStart();
            Range = 80f;
            BaseDamage = 25f;
            FireRate = 0.5f;
            MaxAmmo = 1;
            CurrentAmmo = 1; // Melee doesn't use ammo realistically, but this bypasses reload logic
        }

        protected override void HandleInput()
        {
            if (Input.Pressed("attack1"))
            {
                // Bypass ammo and last fired check if we implement custom melee logic, 
                // but for now let's just use base functionality with ammo being refilled instantly
                CurrentAmmo = 1;
                base.HandleInput();
            }
        }

        [Rpc.Broadcast]
        protected override void DoFireEffects()
        {
            base.DoFireEffects();
            Log.Info("Melee swing!");
        }
    }
}
