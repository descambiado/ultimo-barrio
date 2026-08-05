using Sandbox;
using System;

namespace UltimoBarrio.Combat
{
    [Title("USP Pistol")]
    [Category("Último Barrio — Combat")]
    [Icon("sports_martial_arts")]
    public class USPPistol : BaseCombatWeapon
    {
        // TODO: worldmodel, viewmodel, brazos, collider, ammo_9mm, fire, reload, dry fire, muzzle flash, light flash, smoke, shell casing, impact, recoil, camera kick, sound, damage, drop, repickup.
        
        protected override void OnStart()
        {
            base.OnStart();
            BaseDamage = 15f;
            FireRate = 0.25f;
            MaxAmmo = 12;
            ReloadTime = 1.5f;
            IsAutomatic = false;
        }

        [Rpc.Broadcast]
        protected override void DoFireEffects()
        {
            base.DoFireEffects();
            Log.Info("USP Pistol Fired - TODO: Muzzle flash, sound, recoil");
        }
        
        [Rpc.Broadcast]
        protected override void RpcDoReloadEffects()
        {
            base.RpcDoReloadEffects();
            Log.Info("USP Pistol Reloading - TODO: Reload sound, animation");
        }
    }
}
