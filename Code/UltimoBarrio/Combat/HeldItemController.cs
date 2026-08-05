using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Inventory;

namespace UltimoBarrio.Combat
{
    public enum HeldItemType
    {
        None,
        Melee,
        Pistol
    }

    [Title("Held Item Controller")]
    [Category("Último Barrio — Combat")]
    [Icon("front_hand")]
    public sealed class HeldItemController : Component
    {
        [Property] public GameObject HandBone { get; set; }
        
        [Property] public GameObject MeleePrefab { get; set; }
        [Property] public GameObject PistolPrefab { get; set; }

        [Sync] public Guid ActiveWeaponId { get; set; }
        [Sync] public HeldItemType CurrentType { get; set; }

        private BaseCombatWeapon _activeWeapon;

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.Pressed("Slot1"))
            {
                if (CurrentType != HeldItemType.Melee)
                    EquipWeapon(HeldItemType.Melee);
            }
            else if (Input.Pressed("Slot2"))
            {
                var inv = Components.GetInAncestorsOrSelf<UltimoBarrioPlayerInventory>();
                if (inv != null && inv.GetCount("weapon_usp") > 0)
                {
                    if (CurrentType != HeldItemType.Pistol)
                        EquipWeapon(HeldItemType.Pistol);
                }
            }
            else if (Input.Pressed("Drop"))
            {
                if (CurrentType != HeldItemType.None)
                    EquipWeapon(HeldItemType.None);
            }
        }

        public void EquipWeapon(HeldItemType type)
        {
            if (!Networking.IsHost)
            {
                RpcRequestEquip((int)type);
                return;
            }
            DoEquip(type);
        }

        [Rpc.Host]
        private void RpcRequestEquip(int typeInt)
        {
            DoEquip((HeldItemType)typeInt);
        }

        private void DoEquip(HeldItemType type)
        {
            if (ActiveWeaponId != Guid.Empty)
            {
                var oldWep = Scene.Directory.FindByGuid(ActiveWeaponId);
                oldWep?.Destroy();
            }

            CurrentType = type;
            GameObject prefab = type switch
            {
                HeldItemType.Melee => MeleePrefab,
                HeldItemType.Pistol => PistolPrefab,
                _ => null
            };

            if (prefab == null)
            {
                ActiveWeaponId = Guid.Empty;
                _activeWeapon = null;
                return;
            }

            var newWep = prefab.Clone();
            newWep.SetParent(HandBone ?? GameObject);
            newWep.LocalPosition = Vector3.Zero;
            newWep.LocalRotation = Rotation.Identity;
            newWep.NetworkSpawn(Connection.Local);

            ActiveWeaponId = newWep.Id;
            _activeWeapon = newWep.Components.GetInDescendantsOrSelf<BaseCombatWeapon>();
            Log.Info($"[HeldItem] Equipped {type}");
        }
    }
}
