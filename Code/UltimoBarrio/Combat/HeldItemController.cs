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
        [Sync] public int SelectedHotbarSlot { get; set; } = -1;
        [Sync] public string ActiveItemId { get; set; } = string.Empty;

        private BaseCombatWeapon _activeWeapon;
        private GameObject _currentViewmodel;
        
        private TimeSince _timeSinceStateChange;

        protected override void OnStart()
        {
            if (ViewmodelArms != null && BaseHandsModel != null)
            {
                ViewmodelArms.Model = BaseHandsModel;
            }
            
            DoEquip(HeldItemSlot.None, null, string.Empty);
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
                        DoEquip(HeldItemSlot.None, null, string.Empty);
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

            if (Input.Pressed("Slot1")) SelectSlot(0);
            else if (Input.Pressed("Slot2")) SelectSlot(1);
            else if (Input.Pressed("Slot3")) SelectSlot(2);
            else if (Input.Pressed("Slot4")) SelectSlot(3);
            else if (Input.Pressed("Slot5")) SelectSlot(4);
            else if (Input.Pressed("Slot6")) SelectSlot(5);
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

        public void SelectSlot(int index)
        {
            if (!Networking.IsHost)
            {
                RpcRequestSelectSlot(index);
                return;
            }
            DoSelectSlot(index);
        }

        [Rpc.Host]
        private void RpcRequestSelectSlot(int index)
        {
            DoSelectSlot(index);
        }

        private void DoSelectSlot(int index)
        {
            SelectedHotbarSlot = index;
            
            var inv = Components.Get<InventoryComponent>();
            if (inv == null || index < 0 || index >= inv.HotbarSlots || index >= inv.Slots.Count)
            {
                DoEquip(HeldItemSlot.None, null, string.Empty);
                return;
            }

            var slot = inv.Slots[index];
            if (string.IsNullOrEmpty(slot.ItemId) || slot.Amount <= 0)
            {
                DoEquip(HeldItemSlot.None, null, string.Empty);
                return;
            }

            var def = ItemRegistry.GetDefinition(slot.ItemId);
            if (def == null || (def.Category != ItemCategory.Melee && def.Category != ItemCategory.Firearm))
            {
                DoEquip(HeldItemSlot.None, null, string.Empty);
                return;
            }

            // Fallbacks to properties if WorldPrefab is null (for backward compat with agent's old code)
            GameObject prefabToSpawn = def.WorldPrefab;
            HeldItemSlot slotType = HeldItemSlot.Primary;

            if (def.Category == ItemCategory.Melee)
            {
                slotType = HeldItemSlot.Melee;
                if (prefabToSpawn == null) prefabToSpawn = MeleePrefab;
            }
            else if (def.Category == ItemCategory.Firearm)
            {
                slotType = HeldItemSlot.Primary;
                if (prefabToSpawn == null) prefabToSpawn = PrimaryPrefab;
            }

            DoEquip(slotType, prefabToSpawn, slot.ItemId);
        }

        private void DoEquip(HeldItemSlot slot, GameObject prefab, string itemId)
        {
            ClearCurrentWeapon();
            CurrentSlot = slot;
            ActiveItemId = itemId;

            if (slot == HeldItemSlot.None || prefab == null)
            {
                CurrentState = HeldItemState.Holstered;
                if (ViewmodelArms != null)
                {
                    ViewmodelArms.Enabled = true;
                    if (BaseHandsModel != null) ViewmodelArms.Model = BaseHandsModel;
                    ViewmodelArms.Set("holdtype", 0); // Fists/Empty
                }
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
                ViewmodelArms.Enabled = true;
                if (CurrentSlot == HeldItemSlot.Melee) ViewmodelArms.Set("holdtype", 4);
                else if (CurrentSlot == HeldItemSlot.Primary) ViewmodelArms.Set("holdtype", 2);
                else if (CurrentSlot == HeldItemSlot.Secondary) ViewmodelArms.Set("holdtype", 1);
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
                var originalRenderer = wepObj.Components.GetInDescendantsOrSelf<ModelRenderer>();
                if (originalRenderer != null && ViewmodelArms != null)
                {
                    _currentViewmodel = new GameObject(true, "ViewmodelWeapon");
                    _currentViewmodel.SetParent(ViewmodelArms.GameObject);
                    
                    var bone = ViewmodelArms.GetBoneObject("hold_R");
                    if (bone != null) _currentViewmodel.SetParent(bone);
                    else
                    {
                        _currentViewmodel.LocalPosition = Vector3.Zero;
                        _currentViewmodel.LocalRotation = Rotation.Identity;
                    }

                    var newRenderer = _currentViewmodel.Components.Create<ModelRenderer>();
                    newRenderer.Model = originalRenderer.Model;
                }
            }
        }

        private void DropCurrentWeapon()
        {
            if (CurrentSlot == HeldItemSlot.None || string.IsNullOrEmpty(ActiveItemId)) return;
            
            if (Networking.IsHost)
            {
                var inv = Components.Get<InventoryComponent>();
                if (inv != null)
                {
                    // Call the inventory to handle dropping
                    inv.RequestDrop(ActiveItemId, 1);
                }
                
                ClearCurrentWeapon();
                CurrentState = HeldItemState.Dropping;
                _timeSinceStateChange = 0;
                SelectedHotbarSlot = -1; // Deselect
                ActiveItemId = string.Empty;
                
                // Return to empty hands
                DoEquip(HeldItemSlot.None, null, string.Empty);
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
