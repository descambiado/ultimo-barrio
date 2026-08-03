using Sandbox;
using System;

namespace UltimoBarrio.Combat
{
    public class WeaponEquipper : Component
    {
        [Property] public GameObject WeaponHolder { get; set; }

        [Sync] public Guid CurrentWeaponId { get; set; }

        public void EquipWeapon(GameObject weaponPrefab)
        {
            if (!Networking.IsHost) return;

            // Despawn current if any
            if (CurrentWeaponId != Guid.Empty)
            {
                var oldWep = Scene.Directory.FindByGuid(CurrentWeaponId);
                if (oldWep != null)
                {
                    oldWep.Destroy();
                }
            }

            // Spawn new weapon
            var newWep = weaponPrefab.Clone();
            newWep.SetParent(WeaponHolder ?? GameObject);
            newWep.LocalPosition = Vector3.Zero;
            newWep.LocalRotation = Rotation.Identity;
            newWep.NetworkSpawn(Connection.Local);

            CurrentWeaponId = newWep.Id;
        }
    }
}
