using Sandbox;
using System;
using System.Linq;

namespace UltimoBarrio.Combat
{
    public enum HeldItemState
    {
        Holstered,
        Equipping,
        Equipped,
        Using,
        Reloading,
        Dropping
    }

    public enum HeldItemSlot
    {
        None,
        Melee,
        Primary,
        Secondary
    }

    [Title("Held Item Controller")]
    [Category("Último Barrio — Combat")]
    [Icon("front_hand")]
    public sealed class HeldItemController : Component
    {
        [Property] public GameObject ViewmodelCamera { get; set; }
        [Property] public SkinnedModelRenderer WorldBodyRenderer { get; set; }
        
        [Property] public SkinnedModelRenderer ViewmodelArms { get; set; }
        [Property] public Model BaseHandsModel { get; set; }

        [Property] public GameObject MeleePrefab { get; set; }
        [Property] public GameObject PrimaryPrefab { get; set; }
        [Property] public GameObject SecondaryPrefab { get; set; }

        [Sync] public Guid ActiveWeaponId { get; set; }
        [Sync] public HeldItemState CurrentState { get; set; }
        [Sync] public HeldItemSlot CurrentSlot { get; set; }

        private BaseCombatWeapon _activeWeapon;
        private GameObject _currentViewmodel;
        
        private TimeSince _timeSinceStateChange;

        protected override void OnStart()
        {
            if (ViewmodelArms != null && BaseHandsModel != null)
            {
                ViewmodelArms.Model = BaseHandsModel;
            }
            
            EquipSlot(HeldItemSlot.None);
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            UpdateState();
            UpdateAnimations();
            HandleInput();
        }

        private void UpdateState()
        {
            switch (CurrentState)
            {
                case HeldItemState.Equipping:
                    if (_timeSinceStateChange > 0.5f)
                    {
                        CurrentState = HeldItemState.Equipped;
                    }
                    break;
                case HeldItemState.Dropping:
                    if (_timeSinceStateChange > 0.2f)
                    {
                        EquipSlot(HeldItemSlot.None);
                    }
                    break;
                case HeldItemState.Reloading:
                    if (_activeWeapon != null && !_activeWeapon.IsReloading)
                    {
                        CurrentState = HeldItemState.Equipped;
                    }
                    break;
            }
        }

        private void UpdateAnimations()
        {
            if (ViewmodelArms == null) return;
            
            var cc = Components.GetInAncestorsOrSelf<CharacterController>();
            if (cc != null)
            {
                float speed = cc.Velocity.Length;
                bool isGrounded = cc.IsOnGround;
                
                ViewmodelArms.Set("b_grounded", isGrounded);
                ViewmodelArms.Set("move_speed", speed);
                
                bool isSprinting = Input.Down("Run") && speed > 50f;
                ViewmodelArms.Set("b_sprint", isSprinting);
                
                ViewmodelArms.Set("walk_bob", speed > 10f && !isSprinting ? 1f : 0f);
            }
        }

        private void HandleInput()
        {
            if (CurrentState == HeldItemState.Equipping || CurrentState == HeldItemState.Dropping) return;

            if (Input.Pressed("Slot1"))
            {
                EquipSlot(HeldItemSlot.None);
            }
            else if (Input.Pressed("Slot2"))
            {
                EquipSlot(HeldItemSlot.Melee);
            }
            else if (Input.Pressed("Slot3"))
            {
                EquipSlot(HeldItemSlot.Primary);
            }
            else if (Input.Pressed("Slot4"))
            {
                EquipSlot(HeldItemSlot.Secondary);
            }
            else if (Input.Pressed("Drop"))
            {
                DropCurrentWeapon();
            }
            
            if (Input.Pressed("Jump") && ViewmodelArms != null)
            {
                ViewmodelArms.Set("b_jump", true);
            }

            if (Input.Pressed("attack1") && CurrentState == HeldItemState.Equipped)
            {
                if (_activeWeapon != null)
                {
                    _activeWeapon.Fire();
                    CurrentState = HeldItemState.Using;
                    _timeSinceStateChange = 0;
                }
                else
                {
                    if (ViewmodelArms != null)
                    {
                        ViewmodelArms.Set("b_attack", true);
                        ViewmodelArms.Set("fists_attack", true);
                    }
                }
            }

            if (CurrentState == HeldItemState.Using && _activeWeapon != null)
            {
                if (_timeSinceStateChange > _activeWeapon.FireRate)
                {
                    CurrentState = HeldItemState.Equipped;
                }
            }
            
            if (Input.Pressed("reload") && _activeWeapon != null && CurrentState == HeldItemState.Equipped)
            {
                _activeWeapon.Reload();
                CurrentState = HeldItemState.Reloading;
                _timeSinceStateChange = 0;
            }
        }

        public void EquipSlot(HeldItemSlot slot)
        {
            if (!Networking.IsHost)
            {
                RpcRequestEquip((int)slot);
                return;
            }
            DoEquip(slot);
        }

        [Rpc.Host]
        private void RpcRequestEquip(int slotInt)
        {
            DoEquip((HeldItemSlot)slotInt);
        }

        private void DoEquip(HeldItemSlot slot)
        {
            ClearCurrentWeapon();
            CurrentSlot = slot;

            if (slot == HeldItemSlot.None)
            {
                CurrentState = HeldItemState.Holstered;
                if (ViewmodelArms != null)
                {
                    ViewmodelArms.Enabled = true;
                    if (BaseHandsModel != null) ViewmodelArms.Model = BaseHandsModel;
                }
                return;
            }

            GameObject prefab = slot switch
            {
                HeldItemSlot.Melee => MeleePrefab,
                HeldItemSlot.Primary => PrimaryPrefab,
                HeldItemSlot.Secondary => SecondaryPrefab,
                _ => null
            };

            if (prefab == null)
            {
                EquipSlot(HeldItemSlot.None);
                return;
            }

            var newWep = prefab.Clone();
            
            if (WorldBodyRenderer != null)
            {
                newWep.SetParent(WorldBodyRenderer.GameObject);
                var boneT = WorldBodyRenderer.GetBoneObject("hold_R");
                if (boneT != null)
                {
                    newWep.SetParent(boneT);
                }
                else
                {
                    newWep.LocalPosition = Vector3.Zero;
                    newWep.LocalRotation = Rotation.Identity;
                }
            }
            else
            {
                newWep.SetParent(GameObject);
            }
            
            newWep.NetworkSpawn(Connection.Local);

            ActiveWeaponId = newWep.Id;
            _activeWeapon = newWep.Components.GetInDescendantsOrSelf<BaseCombatWeapon>();
            
            CurrentState = HeldItemState.Equipping;
            _timeSinceStateChange = 0;

            if (ViewmodelArms != null)
            {
                ViewmodelArms.Enabled = false;
            }
            
            RpcOnWeaponEquipped(ActiveWeaponId);
        }
        
        [Rpc.Broadcast]
        private void RpcOnWeaponEquipped(Guid weaponId)
        {
            if (IsProxy) return;
            
            if (_currentViewmodel != null)
            {
                _currentViewmodel.Destroy();
                _currentViewmodel = null;
            }
            
            var wepObj = Scene.Directory.FindByGuid(weaponId);
            if (wepObj != null)
            {
                // Spawn Viewmodel locally logic here
            }
        }

        private void DropCurrentWeapon()
        {
            if (CurrentSlot == HeldItemSlot.None) return;
            
            if (Networking.IsHost)
            {
                GameObject prefab = CurrentSlot switch
                {
                    HeldItemSlot.Melee => MeleePrefab,
                    HeldItemSlot.Primary => PrimaryPrefab,
                    HeldItemSlot.Secondary => SecondaryPrefab,
                    _ => null
                };
                
                if (prefab != null)
                {
                    var tr = Scene.Trace.Ray(WorldPosition + Vector3.Up * 50f, WorldPosition + WorldRotation.Forward * 50f)
                        .IgnoreGameObjectHierarchy(GameObject.Root)
                        .Run();
                        
                    var pickup = new GameObject();
                    pickup.WorldPosition = tr.Hit ? tr.HitPosition : tr.EndPosition;
                    var clone = prefab.Clone(pickup.WorldPosition);
                    clone.WorldPosition = pickup.WorldPosition;
                    
                    var phys = clone.Components.Get<Rigidbody>(FindMode.EnabledInSelfAndDescendants);
                    if (phys != null) phys.Velocity = WorldRotation.Forward * 200f;
                }
                
                ClearCurrentWeapon();
                CurrentState = HeldItemState.Dropping;
                _timeSinceStateChange = 0;
            }
            else
            {
                RpcRequestDrop();
            }
        }
        
        [Rpc.Host]
        private void RpcRequestDrop()
        {
            DropCurrentWeapon();
        }

        private void ClearCurrentWeapon()
        {
            if (ActiveWeaponId != Guid.Empty)
            {
                var oldWep = Scene.Directory.FindByGuid(ActiveWeaponId);
                oldWep?.Destroy();
                ActiveWeaponId = Guid.Empty;
                _activeWeapon = null;
            }
            
            if (_currentViewmodel != null)
            {
                _currentViewmodel.Destroy();
                _currentViewmodel = null;
            }
        }
    }
}
