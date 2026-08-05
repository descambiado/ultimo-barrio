using UltimoBarrio.Core;
﻿using Sandbox;
using System;

namespace UltimoBarrio.Combat
{
    [Title("Melee Weapon")]
    [Category("Último Barrio — Combat")]
    [Icon("pan_tool")]
    public class MeleeWeapon : BaseCombatWeapon
    {
        [Property] public float StaminaCost { get; set; } = 10f;
        [Property] public float PushForce { get; set; } = 200f;
        [Property] public SoundEvent HitSound { get; set; }
        [Property] public SoundEvent MissSound { get; set; }
        [Property] public SoundEvent WallHitSound { get; set; }

        private TimeSince _timeSinceMeleeFire;

        protected override void OnStart()
        {
            base.OnStart();
            Range = 80f;
            BaseDamage = 25f;
            FireRate = 0.5f; // Cooldown
            MaxAmmo = 1;
            CurrentAmmo = 1; 
        }

        protected override void HandleInput()
        {
            if (IsReloading) return;

            bool wantToFire = IsAutomatic ? Input.Down("attack1") : Input.Pressed("attack1");
            
            // TODO: integrate with Stamina component if exists
            bool hasStamina = true; // Assume true for now

            if (wantToFire && hasStamina && _timeSinceMeleeFire >= FireRate)
            {
                _timeSinceMeleeFire = 0;
                
                // Manually trigger effects and trace to bypass ammo logic
                if (Networking.IsHost)
                {
                    CurrentAmmo = 1; // keep it at 1
                }
                
                DoFireEffects();
                PerformTrace();
            }
        }

        protected override void PerformTrace()
        {
            var ray = Scene.Camera.ScreenNormalToRay(0.5f);
            
            // Sphere cast or thick ray for melee is usually better
            var tr = Scene.Trace.Ray(ray, Range)
                .IgnoreGameObjectHierarchy(GameObject.Root)
                .Radius(15f)
                .Run();

            if (tr.Hit)
            {
                var isWall = tr.GameObject.Tags.Has("world") || tr.GameObject.Tags.Has("solid");
                
                if (isWall)
                {
                    DoWallHitEffects(tr.HitPosition, tr.Normal);
                    return; // Blocked by wall
                }

                var damageable = tr.GameObject.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>();
                if (damageable != null)
                {
                    var dmg = new DamageEvent
                    {
                        Amount = BaseDamage,
                        Position = tr.HitPosition,
                        Force = ray.Forward * PushForce, // Empuje
                        AttackerId = Connection.Local?.Id.ToString() ?? "",
                        WeaponId = GameObject.Name
                    };

                    if (Networking.IsHost)
                    {
                        damageable.TakeDamage(dmg);
                    }
                    else
                    {
                        RpcRequestMeleeDamage(tr.GameObject.Id, dmg.Amount, dmg.Position, dmg.Force, dmg.AttackerId, dmg.WeaponId);
                    }
                }
                
                DoHitEffects(tr.HitPosition, tr.Normal);
            }
            else
            {
                DoMissEffects();
            }
        }
        
        [Rpc.Host]
        private void RpcRequestMeleeDamage(Guid hitObjectId, float damage, Vector3 position, Vector3 force, string attackerId, string weaponId)
        {
            var hitObj = Scene.Directory.FindByGuid(hitObjectId);
            if (hitObj != null)
            {
                var damageable = hitObj.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>();
                if (damageable != null)
                {
                    var dmg = new DamageEvent
                    {
                        Amount = damage,
                        Position = position,
                        Force = force,
                        AttackerId = attackerId,
                        WeaponId = weaponId
                    };
                    damageable.TakeDamage(dmg);
                }
            }
        }

        [Rpc.Broadcast]
        protected override void DoHitEffects(Vector3 position, Vector3 normal)
        {
            base.DoHitEffects(position, normal);
            if (HitSound != null) Sound.Play(HitSound, position);
            Log.Info("Melee Hit!");
        }

        [Rpc.Broadcast]
        protected void DoWallHitEffects(Vector3 position, Vector3 normal)
        {
            if (WallHitSound != null) Sound.Play(WallHitSound, position);
            Log.Info("Melee Hit Wall!");
        }

        [Rpc.Broadcast]
        protected void DoMissEffects()
        {
            if (MissSound != null) Sound.Play(MissSound, WorldPosition);
            Log.Info("Melee Miss!");
        }
    }
}
