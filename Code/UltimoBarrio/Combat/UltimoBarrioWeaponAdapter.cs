using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio.Combat;

namespace UltimoBarrio.Combat
{
    [Title("Ultimo Barrio Weapon Adapter")]
    [Category("Último Barrio — Combat")]
    [Icon("shield")]
    public class UltimoBarrioWeaponAdapter : Component
    {
        [Property] public string WeaponId { get; set; } = "weapon_pistol";
        [Property] public string AmmoItemId { get; set; } = "ammo";
        [Property] public int ClipSize { get; set; } = 12;
        [Property] public float DamagePerShot { get; set; } = 25f;

        [Sync(SyncFlags.FromHost)] public int AmmoInClip { get; private set; }
        
        protected override void OnStart()
        {
            if (Networking.IsHost)
            {
                AmmoInClip = ClipSize;
            }
        }

        public bool CanShoot()
        {
            return AmmoInClip > 0;
        }

        public void ConsumeAmmo()
        {
            if (!Networking.IsHost) return;
            if (AmmoInClip > 0) AmmoInClip--;
        }

        public bool TryReload(InventoryComponent inventory)
        {
            if (!Networking.IsHost) return false;
            if (inventory == null) return false;

            int needed = ClipSize - AmmoInClip;
            if (needed <= 0) return false;

            int available = inventory.GetCount(AmmoItemId);
            if (available <= 0) return false;

            int toLoad = Math.Min(needed, available);
            if (inventory.TryRemove(AmmoItemId, toLoad))
            {
                AmmoInClip += toLoad;
                Log.Info($"[WeaponAdapter] Reloaded {toLoad} {AmmoItemId}. Ammo now: {AmmoInClip}/{ClipSize}");
                return true;
            }
            return false;
        }

        public void FireHitscan(Vector3 origin, Vector3 direction, string attackerId)
        {
            if (!CanShoot()) return;

            if (Networking.IsHost)
            {
                ConsumeAmmo();
                var tr = Scene.Trace.Ray(origin, origin + direction * 3000f)
                    .IgnoreGameObjectHierarchy(GameObject)
                    .Run();

                if (tr.Hit && tr.GameObject != null)
                {
                    var damageable = tr.GameObject.Components.Get<UltimoBarrio.Core.IDamageable>();
                    if (damageable != null)
                    {
                        var dmgEvent = new DamageEvent
                        {
                            Amount = DamagePerShot,
                            Position = tr.EndPosition,
                            Force = direction * 500f,
                            AttackerId = attackerId,
                            WeaponId = WeaponId
                        };
                        damageable.TakeDamage(dmgEvent);
                        Log.Info($"[WeaponAdapter] Hit {tr.GameObject.Name} for {DamagePerShot} damage!");
                    }
                }
            }
        }
    }
}
