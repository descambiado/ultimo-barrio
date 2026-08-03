using Sandbox;
using System;

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

        [Authority]
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

        [Authority]
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

        [Broadcast]
        protected virtual void DoFireEffects()
        {
            // Override for sounds/particles
            // Sound.Play("shoot_sound");
        }

        [Broadcast]
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
                var damageable = tr.GameObject.Components.GetInAncestorsOrSelf<UltimoBarrio.IDamageable>();
                if (damageable != null)
                {
                    if (Networking.IsHost)
                    {
                        damageable.TakeDamage(BaseDamage, tr.HitPosition, ray.Forward * 100f, Guid.Empty);
                    }
                    else
                    {
                        RpcApplyDamage(tr.GameObject.Id, BaseDamage, tr.HitPosition, ray.Forward * 100f);
                    }
                }
                
                DoHitEffects(tr.HitPosition, tr.Normal);
            }
        }

        [Rpc.Host]
        private void RpcApplyDamage(Guid hitObjectId, float damage, Vector3 position, Vector3 force)
        {
            var hitObj = Scene.Directory.FindByGuid(hitObjectId);
            if (hitObj != null)
            {
                var damageable = hitObj.Components.GetInAncestorsOrSelf<UltimoBarrio.IDamageable>();
                if (damageable != null)
                {
                    // Check friendly fire later if needed, disabled by default per requirements
                    damageable.TakeDamage(damage, position, force, Guid.Empty);
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
