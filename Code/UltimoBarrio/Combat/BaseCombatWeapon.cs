using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Combat
{
    public class BaseCombatWeapon : Component
    {
        [Property] public float BaseDamage { get; set; } = 15f;
        [Property] public float FireRate { get; set; } = 0.2f;
        [Property] public int MaxAmmo { get; set; } = 30;
        [Property] public float ReloadTime { get; set; } = 2.0f;
        [Property] public bool IsAutomatic { get; set; } = false;
        [Property] public bool FriendlyFire { get; set; } = false;

        [Sync] public int CurrentAmmo { get; protected set; }
        [Sync] public bool IsReloading { get; protected set; }

        private TimeSince _lastFired;
        private TimeUntil _reloadComplete;

        protected override void OnStart()
        {
            if (Networking.IsHost)
            {
                CurrentAmmo = MaxAmmo;
            }
        }

        protected override void OnUpdate()
        {
            if (!IsProxy)
            {
                HandleInput();
            }

            if (Networking.IsHost && IsReloading && _reloadComplete)
            {
                FinishReload();
            }
        }

        protected virtual void HandleInput()
        {
            if (IsReloading) return;

            bool wantToFire = IsAutomatic ? Input.Down("attack1") : Input.Pressed("attack1");
            if (wantToFire && _lastFired >= FireRate)
            {
                if (CurrentAmmo > 0)
                {
                    Fire();
                }
                else
                {
                    Reload();
                }
            }

            if (Input.Pressed("reload") && CurrentAmmo < MaxAmmo)
            {
                Reload();
            }
        }

        public void Fire()
        {
            _lastFired = 0f;
            
            if (Networking.IsHost)
            {
                CurrentAmmo--;
            }
            else
            {
                RpcRequestFire();
            }
            
            DoFireEffects();
            PerformTrace();
        }

        [Rpc.Owner]
        private void RpcRequestFire()
        {
            if (CurrentAmmo <= 0 || IsReloading) return;
            CurrentAmmo--;
        }

        public void Reload()
        {
            if (IsReloading || CurrentAmmo == MaxAmmo) return;

            if (Networking.IsHost)
            {
                StartReload();
            }
            else
            {
                RpcRequestReload();
            }
        }

        [Rpc.Owner]
        private void RpcRequestReload()
        {
            StartReload();
        }

        private void StartReload()
        {
            IsReloading = true;
            _reloadComplete = ReloadTime;
            RpcDoReloadEffects();
        }

        private void FinishReload()
        {
            IsReloading = false;
            CurrentAmmo = MaxAmmo;
        }

        [Rpc.Broadcast]
        protected virtual void DoFireEffects()
        {
            // Override for sounds/particles
            // Sound.Play("shoot_sound");
        }

        [Rpc.Broadcast]
        protected virtual void RpcDoReloadEffects()
        {
            // Override for reload sound
        }

        protected virtual void PerformTrace()
        {
            var ray = Scene.Camera.ScreenNormalToRay(0.5f);
            var tr = Scene.Trace.Ray(ray, 5000f)
                .IgnoreGameObjectHierarchy(GameObject.Root)
                .Run();

            if (tr.Hit)
            {
                var damageable = tr.GameObject.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>();
                if (damageable != null)
                {
                    var dmg = new DamageEvent
                    {
                        Amount = BaseDamage,
                        Position = tr.HitPosition,
                        Force = ray.Forward * 100f,
                        AttackerId = Connection.Local?.Id.ToString() ?? "",
                        WeaponId = GameObject.Name
                    };

                    if (Networking.IsHost)
                    {
                        damageable.TakeDamage(dmg);
                    }
                    else
                    {
                        RpcApplyDamage(tr.GameObject.Id, dmg.Amount, dmg.Position, dmg.Force, dmg.AttackerId, dmg.WeaponId);
                    }
                }
                
                DoHitEffects(tr.HitPosition, tr.Normal);
            }
        }

        [Rpc.Host]
        private void RpcApplyDamage(Guid hitObjectId, float damage, Vector3 position, Vector3 force, string attackerId, string weaponId)
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
                    // Check friendly fire later if needed, disabled by default per requirements
                    damageable.TakeDamage(dmg);
                }
            }
        }

        [Rpc.Broadcast]
        protected virtual void DoHitEffects(Vector3 position, Vector3 normal)
        {
            // Particles etc.
        }
    }
}
