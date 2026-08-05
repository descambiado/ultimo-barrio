using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Combat;
using UltimoBarrio.UI;

namespace UltimoBarrio
{
    public enum HeldItemState
    {
        Empty,
        Equipping,
        Equipped,
        Using,
        Aiming,
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

    /// <summary>
    /// Conecta el inventario (6 slots de hotbar) con el objeto sostenido.
    ///
    /// Reglas:
    ///  - Solo el host decide selección y equipamiento ([Rpc.Host]).
    ///  - El inventario es la fuente de verdad del cargador (slot.AmmoInMag);
    ///    el arma cacheada por slot NO se destruye al cambiar de slot, así el
    ///    cargador nunca se reinicia.
    ///  - Sin objeto → puños (FistsWeapon): idle, salto, aterrizaje y puñetazo.
    ///  - Consumibles (agua, medicina, vendaje) y recursos se sostienen en mano.
    ///  - El input de armas se bloquea mientras stash/trader/inventario/crafting
    ///    capturan la UI (PlayerHud.CurrentState != Gameplay).
    ///  - Los huesos de attach se buscan por nombre ("hold_R"), nunca por GUID.
    /// </summary>
    [Title( "Held Item Controller" )]
    [Category( "Último Barrio — Combat" )]
    [Icon( "front_hand" )]
    public sealed class HeldItemController : Component
    {
        [Property] public SkinnedModelRenderer WorldBodyRenderer { get; set; }
        [Property] public SkinnedModelRenderer ViewmodelArms { get; set; }
        [Property] public Model BaseHandsModel { get; set; }

        [Sync] public HeldItemState CurrentState { get; private set; } = HeldItemState.Empty;
        [Sync] public HeldItemSlot CurrentSlot { get; private set; } = HeldItemSlot.None;
        [Sync] public int SelectedHotbarSlot { get; private set; } = -1;
        [Sync] public string ActiveItemId { get; private set; } = string.Empty;
        [Sync] public Guid ActiveWeaponId { get; private set; }

        /// <summary>
        /// Caché host-side de objetos de arma por slot. El objeto vive mientras
        /// el ítem esté en el inventario: el cargador ([Sync]) persiste entre
        /// cambios de slot sin reiniciarse.
        /// </summary>
        private readonly Dictionary<int, GameObject> _slotWeapons = new();

        private FistsWeapon _fists;
        private GameObject _currentViewmodel;
        private TimeSince _timeSinceStateChange;
        private bool _wasGrounded = true;

        // ------------------------------------------------------------------
        // Ciclo de vida
        // ------------------------------------------------------------------

        protected override void OnStart()
        {
            if ( ViewmodelArms != null && BaseHandsModel != null )
                ViewmodelArms.Model = BaseHandsModel;

            _fists = Components.GetOrCreate<FistsWeapon>();
            SelectEmptyHands();

            // Al morir, soltar el objeto activo (host).
            var health = Components.GetInDescendantsOrSelf<HealthComponent>();
            if ( health is not null )
                health.OnDeath += OnOwnerDied;
        }

        protected override void OnDestroy()
        {
            var health = Components.GetInDescendantsOrSelf<HealthComponent>();
            if ( health is not null )
                health.OnDeath -= OnOwnerDied;
        }

        protected override void OnUpdate()
        {
            if ( IsProxy )
                return;

            UpdateState();
            UpdateAnimations();
            WriteBackMagazine();

            if ( !IsUiCapturingInput() )
                HandleInput();
        }

        private void UpdateState()
        {
            switch ( CurrentState )
            {
                case HeldItemState.Equipping:
                    if ( _timeSinceStateChange > 0.5f )
                        CurrentState = HeldItemState.Equipped;
                    break;

                case HeldItemState.Using:
                    if ( _timeSinceStateChange > 0.2f )
                        CurrentState = HeldItemState.Equipped;
                    break;

                case HeldItemState.Reloading:
                    var weapon = GetActiveWeaponComponent();
                    if ( weapon is null || !weapon.IsReloading )
                        CurrentState = HeldItemState.Equipped;
                    break;
            }
        }

        private void UpdateAnimations()
        {
            if ( ViewmodelArms == null )
                return;

            var cc = Components.GetInAncestorsOrSelf<CharacterController>();
            if ( cc == null )
                return;

            float speed = cc.Velocity.Length;
            bool isGrounded = cc.IsOnGround;

            ViewmodelArms.Set( "b_grounded", isGrounded );
            ViewmodelArms.Set( "move_speed", speed );

            bool isSprinting = Input.Down( "Run" ) && speed > 50f;
            ViewmodelArms.Set( "b_sprint", isSprinting );
            ViewmodelArms.Set( "walk_bob", speed > 10f && !isSprinting ? 1f : 0f );

            // Aterrizaje.
            if ( !_wasGrounded && isGrounded )
            {
                ViewmodelArms.Set( "b_jump", false );
                ViewmodelArms.Set( "b_land", true );
            }
            else if ( isGrounded )
            {
                ViewmodelArms.Set( "b_land", false );
            }
            _wasGrounded = isGrounded;

            if ( Input.Pressed( "Jump" ) )
                ViewmodelArms.Set( "b_jump", true );
        }

        // ------------------------------------------------------------------
        // Input
        // ------------------------------------------------------------------

        public bool IsUiCapturingInput()
        {
            var hud = Components.GetInDescendantsOrSelf<PlayerHud>();
            return hud is not null && hud.CurrentState != HudState.Gameplay;
        }

        private void HandleInput()
        {
            if ( CurrentState is HeldItemState.Equipping or HeldItemState.Dropping )
                return;

            var inv = Components.Get<InventoryComponent>();

            // Slots 1-6.
            if ( Input.Pressed( "Slot1" ) ) { SelectSlot( 0 ); return; }
            if ( Input.Pressed( "Slot2" ) ) { SelectSlot( 1 ); return; }
            if ( Input.Pressed( "Slot3" ) ) { SelectSlot( 2 ); return; }
            if ( Input.Pressed( "Slot4" ) ) { SelectSlot( 3 ); return; }
            if ( Input.Pressed( "Slot5" ) ) { SelectSlot( 4 ); return; }
            if ( Input.Pressed( "Slot6" ) ) { SelectSlot( 5 ); return; }

            // Rueda: anterior/siguiente.
            if ( inv is not null && inv.HotbarSlots > 0 )
            {
                float wheel = Input.MouseWheel.y;
                if ( wheel != 0f )
                {
                    int start = SelectedHotbarSlot < 0 ? 0 : SelectedHotbarSlot;
                    int next = ( start + ( wheel > 0 ? 1 : -1 ) + inv.HotbarSlots ) % inv.HotbarSlots;
                    SelectSlot( next );
                    return;
                }
            }

            // Soltar el objeto activo.
            if ( Input.Pressed( "Drop" ) )
            {
                DropActiveItem();
                return;
            }

            // Ataque / uso.
            if ( Input.Pressed( "attack1" ) && CurrentState is HeldItemState.Equipped or HeldItemState.Empty )
            {
                if ( CurrentSlot is HeldItemSlot.Primary or HeldItemSlot.Secondary
                    && GetActiveWeaponComponent() is { } ranged )
                {
                    ranged.Fire();
                    CurrentState = HeldItemState.Using;
                    _timeSinceStateChange = 0;
                }
                else if ( CurrentSlot == HeldItemSlot.Melee && GetActiveWeaponComponent() is MeleeWeapon melee )
                {
                    melee.TrySwing();
                    CurrentState = HeldItemState.Using;
                    _timeSinceStateChange = 0;
                }
                else if ( _fists is not null )
                {
                    // Puños (manos vacías).
                    _fists.TrySwing();
                    CurrentState = HeldItemState.Using;
                    _timeSinceStateChange = 0;
                    ViewmodelArms?.Set( "b_attack", true );
                    ViewmodelArms?.Set( "fists_attack", true );
                }
            }

            // Consumible en mano (agua, medicina, vendaje).
            if ( Input.Pressed( "attack1" ) && CurrentSlot == HeldItemSlot.None && !string.IsNullOrEmpty( ActiveItemId ) )
            {
                var def = ItemRegistry.GetDefinition( ActiveItemId );
                if ( def is not null && def.Usable )
                {
                    UseActiveConsumable();
                    return;
                }

                if ( def is not null && !def.Usable )
                {
                    // Recurso/municion: no se consume; aviso HUD.
                    var hud = Components.GetInDescendantsOrSelf<PlayerHud>();
                    hud?.ShowMessage( $"{def.DisplayName}: no se puede usar así" );
                }
            }

            // Recargar.
            if ( Input.Pressed( "reload" ) && GetActiveWeaponComponent() is { } weapon
                && CurrentState is HeldItemState.Equipped or HeldItemState.Using )
            {
                weapon.Reload();
                CurrentState = HeldItemState.Reloading;
                _timeSinceStateChange = 0;
            }

            // Apuntar (placeholder: estado Aiming, sin zoom por ahora).
            if ( Input.Down( "attack2" ) && CurrentSlot is HeldItemSlot.Primary or HeldItemSlot.Secondary )
            {
                if ( CurrentState == HeldItemState.Equipped )
                    CurrentState = HeldItemState.Aiming;
            }
            else if ( CurrentState == HeldItemState.Aiming )
            {
                CurrentState = HeldItemState.Equipped;
            }
        }

        // ------------------------------------------------------------------
        // Selección de slot
        // ------------------------------------------------------------------

        public void SelectSlot( int index )
        {
            if ( !Networking.IsHost )
            {
                RpcRequestSelectSlot( index );
                return;
            }

            DoSelectSlot( index );
        }

        [Rpc.Host]
        private void RpcRequestSelectSlot( int index )
        {
            DoSelectSlot( index );
        }

        private void DoSelectSlot( int index )
        {
            SelectedHotbarSlot = index;

            var inv = Components.Get<InventoryComponent>();
            if ( inv is null || index < 0 || index >= inv.HotbarSlots || index >= inv.Slots.Count )
            {
                SelectEmptyHands();
                return;
            }

            var slot = inv.Slots[index];
            if ( string.IsNullOrEmpty( slot.ItemId ) || slot.Amount <= 0 )
            {
                SelectEmptyHands();
                return;
            }

            var definition = ItemRegistry.GetDefinition( slot.ItemId );
            if ( definition is null )
            {
                Log.Error( $"[HeldItems] Ítem '{slot.ItemId}' sin definición; slot vacío." );
                SelectEmptyHands();
                return;
            }

            if ( definition.IsWeapon )
                EquipWeaponSlot( index, definition, slot );
            else
                EquipHeldItem( slot.ItemId, definition );
        }

        // ------------------------------------------------------------------
        // Equipamiento
        // ------------------------------------------------------------------

        private void EquipWeaponSlot( int index, ItemDefinition definition, InventorySlot slot )
        {
            if ( !_slotWeapons.TryGetValue( index, out var weaponGo ) || !weaponGo.IsValid() )
            {
                weaponGo = CreateWeaponObject( definition );
                if ( weaponGo is null )
                {
                    // Fallback: sin prefab de presentación no se equipa.
                    Log.Error( $"[HeldItems] No se pudo crear el arma '{definition.ItemId}' (prefab ausente)." );
                    SelectEmptyHands();
                    return;
                }

                _slotWeapons[index] = weaponGo;
            }

            ApplyMagazineFromSlot( weaponGo, slot, definition );

            foreach ( var pair in _slotWeapons )
            {
                if ( pair.Key != index )
                    pair.Value.Enabled = false;
            }
            weaponGo.Enabled = true;

            ActiveWeaponId = weaponGo.Id;
            ActiveItemId = slot.ItemId;
            CurrentSlot = definition.Category == ItemCategory.Melee ? HeldItemSlot.Melee : HeldItemSlot.Primary;
            CurrentState = HeldItemState.Equipping;
            _timeSinceStateChange = 0;

            if ( ViewmodelArms != null )
            {
                ViewmodelArms.Enabled = true;
                if ( BaseHandsModel != null )
                    ViewmodelArms.Model = BaseHandsModel;

                ViewmodelArms.Set( "holdtype", CurrentSlot == HeldItemSlot.Melee ? 4 : 2 );
            }

            RpcOnWeaponEquipped( ActiveWeaponId );
        }

        private void EquipHeldItem( string itemId, ItemDefinition definition )
        {
            HideAllWeapons();

            ActiveItemId = itemId;
            ActiveWeaponId = Guid.Empty;
            CurrentSlot = HeldItemSlot.None;
            CurrentState = HeldItemState.Equipped;
            _timeSinceStateChange = 0;

            if ( ViewmodelArms != null )
            {
                ViewmodelArms.Enabled = true;
                if ( BaseHandsModel != null )
                    ViewmodelArms.Model = BaseHandsModel;
                ViewmodelArms.Set( "holdtype", definition.Usable ? 1 : 0 );
            }
        }

        private void SelectEmptyHands()
        {
            HideAllWeapons();

            ActiveItemId = string.Empty;
            ActiveWeaponId = Guid.Empty;
            CurrentSlot = HeldItemSlot.None;
            CurrentState = HeldItemState.Empty;

            if ( ViewmodelArms != null )
            {
                ViewmodelArms.Enabled = true;
                if ( BaseHandsModel != null )
                    ViewmodelArms.Model = BaseHandsModel;
                ViewmodelArms.Set( "holdtype", 0 );
            }
        }

        private GameObject CreateWeaponObject( ItemDefinition definition )
        {
            if ( string.IsNullOrEmpty( definition.WorldModelPrefab ) )
                return null;

            var prefabFile = ResourceLibrary.Get<PrefabFile>( definition.WorldModelPrefab );
            if ( prefabFile is null )
                return null;

            var scene = SceneUtility.GetPrefabScene( prefabFile );
            if ( scene is null )
                return null;

            var newWeapon = scene.Clone();
            newWeapon.Name = $"{definition.ItemId} (slot)";

            // Estadísticas desde el registro canónico (fuente de verdad).
            var weaponComp = newWeapon.Components.GetInDescendantsOrSelf<BaseCombatWeapon>();
            weaponComp?.ApplyDefinitionStats( definition );

            // Attach por nombre de hueso (nunca por GUID).
            if ( WorldBodyRenderer is not null )
            {
                var bone = WorldBodyRenderer.GetBoneObject( "hold_R" );
                if ( bone is not null )
                    newWeapon.SetParent( bone );
                else
                    newWeapon.SetParent( WorldBodyRenderer.GameObject );

                newWeapon.LocalPosition = Vector3.Zero;
                newWeapon.LocalRotation = Rotation.Identity;
            }
            else
            {
                newWeapon.SetParent( GameObject );
            }

            if ( Networking.IsActive )
                newWeapon.NetworkSpawn( Connection.Local );

            newWeapon.Enabled = false;
            return newWeapon;
        }

        /// <summary>
        /// El inventario es la fuente de verdad del cargador. Al equipar se
        /// restaura el cargador del slot; cada frame se escribe de vuelta.
        /// </summary>
        private void ApplyMagazineFromSlot( GameObject weaponGo, InventorySlot slot, ItemDefinition definition )
        {
            var weapon = weaponGo.Components.GetInDescendantsOrSelf<BaseCombatWeapon>();
            if ( weapon is null )
                return;

            if ( definition.MagazineSize > 0 )
                weapon.MaxAmmo = definition.MagazineSize;

            int mag = slot.AmmoInMag > 0
                ? Math.Min( slot.AmmoInMag, weapon.MaxAmmo )
                : weapon.MaxAmmo;

            weapon.CurrentAmmo = mag;
        }

        private void WriteBackMagazine()
        {
            if ( !Networking.IsHost )
                return;

            if ( ActiveWeaponId == Guid.Empty || SelectedHotbarSlot < 0 )
                return;

            var inv = Components.Get<InventoryComponent>();
            if ( inv is null || SelectedHotbarSlot >= inv.Slots.Count )
                return;

            var slot = inv.Slots[SelectedHotbarSlot];
            if ( string.IsNullOrEmpty( slot.ItemId ) || slot.Amount <= 0 )
                return;

            var weapon = GetActiveWeaponComponent();
            if ( weapon is null )
                return;

            slot.AmmoInMag = weapon.CurrentAmmo;
        }

        private BaseCombatWeapon GetActiveWeaponComponent()
        {
            if ( ActiveWeaponId == Guid.Empty )
                return null;

            var weaponGo = Scene.Directory.FindByGuid( ActiveWeaponId );
            if ( weaponGo is null || !weaponGo.IsValid() )
                return null;

            return weaponGo.Components.GetInDescendantsOrSelf<BaseCombatWeapon>();
        }

        private void HideAllWeapons()
        {
            foreach ( var pair in _slotWeapons )
            {
                if ( pair.Value.IsValid() )
                    pair.Value.Enabled = false;
            }

            if ( _currentViewmodel is not null )
            {
                _currentViewmodel.Destroy();
                _currentViewmodel = null;
            }

            ActiveWeaponId = Guid.Empty;
        }

        private void RemoveSlotWeapon( int index )
        {
            if ( _slotWeapons.TryGetValue( index, out var weaponGo ) )
            {
                if ( weaponGo.IsValid() )
                    weaponGo.Destroy();
                _slotWeapons.Remove( index );
            }

            if ( _currentViewmodel is not null )
            {
                _currentViewmodel.Destroy();
                _currentViewmodel = null;
            }
        }

        // ------------------------------------------------------------------
        // Viewmodel (solo propietario)
        // ------------------------------------------------------------------

        [Rpc.Broadcast]
        private void RpcOnWeaponEquipped( Guid weaponId )
        {
            if ( IsProxy )
                return;

            if ( _currentViewmodel is not null )
            {
                _currentViewmodel.Destroy();
                _currentViewmodel = null;
            }

            if ( ViewmodelArms is null )
                return;

            var weaponObj = Scene.Directory.FindByGuid( weaponId );
            if ( weaponObj is null )
                return;

            var originalRenderer = weaponObj.Components.GetInDescendantsOrSelf<ModelRenderer>();
            if ( originalRenderer is null )
                return;

            _currentViewmodel = new GameObject( true, "ViewmodelWeapon" );
            _currentViewmodel.SetParent( ViewmodelArms.GameObject );

            var bone = ViewmodelArms.GetBoneObject( "hold_R" );
            if ( bone is not null )
                _currentViewmodel.SetParent( bone );
            else
            {
                _currentViewmodel.LocalPosition = Vector3.Zero;
                _currentViewmodel.LocalRotation = Rotation.Identity;
            }

            var viewRenderer = _currentViewmodel.Components.Create<ModelRenderer>();
            viewRenderer.Model = originalRenderer.Model;
            viewRenderer.Tint = originalRenderer.Tint;
        }

        // ------------------------------------------------------------------
        // Drop con rollback y repickup
        // ------------------------------------------------------------------

        public void DropActiveItem()
        {
            if ( string.IsNullOrEmpty( ActiveItemId ) )
                return;

            if ( !Networking.IsHost )
            {
                RpcRequestDrop();
                return;
            }

            DoDropActiveItem();
        }

        [Rpc.Host]
        private void RpcRequestDrop()
        {
            DoDropActiveItem();
        }

        private void DoDropActiveItem()
        {
            var inv = Components.Get<InventoryComponent>();
            if ( inv is null )
                return;

            int index = SelectedHotbarSlot;
            if ( index < 0 || index >= inv.Slots.Count )
                return;

            var slot = inv.Slots[index];
            if ( string.IsNullOrEmpty( slot.ItemId ) || slot.Amount <= 0 )
                return;

            var itemId = slot.ItemId;
            int mag = slot.AmmoInMag;

            if ( !inv.TryRemove( itemId, 1 ) )
                return;

            var pickup = ItemPickupFactory.SpawnPickup(
                Scene, itemId, 1, mag,
                GameObject.WorldPosition + Vector3.Up * 50f + WorldRotation.Forward * 50f,
                spawnVelocity: WorldRotation.Forward * 140f + Vector3.Up * 60f );

            if ( pickup is null )
            {
                // Rollback: no se pudo soltar.
                inv.TryAdd( itemId, 1 );
                return;
            }

            RemoveSlotWeapon( index );

            // Si quedan unidades del mismo ítem, re-equipar el slot; si no, manos vacías.
            if ( index < inv.Slots.Count && !string.IsNullOrEmpty( inv.Slots[index].ItemId ) && inv.Slots[index].Amount > 0 )
                DoSelectSlot( index );
            else
                SelectEmptyHands();
        }

        // ------------------------------------------------------------------
        // Consumibles
        // ------------------------------------------------------------------

        public void UseActiveConsumable()
        {
            if ( string.IsNullOrEmpty( ActiveItemId ) )
                return;

            if ( !Networking.IsHost )
            {
                RpcUseConsumable( ActiveItemId );
                return;
            }

            UseConsumableOnHost( ActiveItemId );
        }

        [Rpc.Host]
        private void RpcUseConsumable( string itemId )
        {
            UseConsumableOnHost( itemId );
        }

        private void UseConsumableOnHost( string itemId )
        {
            var definition = ItemRegistry.GetDefinition( itemId );
            if ( definition is null || !definition.Usable )
                return;

            var inv = Components.Get<InventoryComponent>();
            if ( inv is null || inv.GetCount( itemId ) <= 0 )
                return;

            // Efecto host-autoritativo: curar antes de consumir.
            var health = Components.GetInDescendantsOrSelf<HealthComponent>();
            if ( definition.ConsumeHeal > 0 )
            {
                if ( health is null || health.IsDead || health.Health >= health.MaxHealth )
                {
                    var hud = Components.GetInDescendantsOrSelf<PlayerHud>();
                    hud?.ShowMessage( "No necesitas curarte ahora" );
                    return;
                }

                if ( !inv.TryRemove( itemId, 1 ) )
                    return;

                health.Heal( definition.ConsumeHeal );
            }
            else
            {
                // Consumible sin cura (agua): consumir y dar feedback.
                if ( !inv.TryRemove( itemId, 1 ) )
                    return;
            }

            var audio = Scene.GetAllComponents<Audio.UltimoBarrioAudioCatalog>().FirstOrDefault();
            audio?.PlayEvent( Audio.AudioEvent.PickupWater, WorldPosition );

            var hud = Components.GetInDescendantsOrSelf<PlayerHud>();
            hud?.ShowMessage( $"Usado: {definition.DisplayName}" );

            CurrentState = HeldItemState.Using;
            _timeSinceStateChange = 0;
        }

        /// <summary>El jugador murió: soltar el objeto activo y deseleccionar.</summary>
        public void OnOwnerDied()
        {
            if ( !Networking.IsHost )
                return;

            if ( !string.IsNullOrEmpty( ActiveItemId ) && SelectedHotbarSlot >= 0 )
                DoDropActiveItem();
            else
                SelectEmptyHands();
        }

        /// <summary>Restaura la selección de hotbar guardada (host).</summary>
        internal void RestoreSelection( int slot )
        {
            if ( !Networking.IsHost )
                return;

            if ( slot >= 0 )
                DoSelectSlot( slot );
        }
    }
}
