// UbWeaponCarrier — connects hotbar inventory to weapon prefabs
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Collections.Generic;
using UltimoBarrio.Core;
using UltimoBarrio.Content.Weapons;
using UltimoBarrio.Components;

namespace UltimoBarrio.Combat
{
	[Title( "Weapon Carrier" )]
	[Category( "Ultimo Barrio — Combat" )]
	[Icon( "gps_fixed" )]
	public sealed class UbWeaponCarrier : Component
	{
		private static readonly Dictionary<string, (string World, string View)> WeaponPrefabs = new()
		{
			["weapon_usp"] = ("prefabs/content/weapons/w_usp_content.prefab", "prefabs/content/weapons/v_usp_content.prefab"),
			["weapon_shotgun"] = ("prefabs/content/weapons/w_shotgun_content.prefab", "prefabs/content/weapons/v_shotgun_content.prefab"),
			["weapon_crowbar"] = ("prefabs/content/weapons/w_crowbar_content.prefab", "prefabs/content/weapons/v_crowbar_content.prefab"),
			["weapon_knife"] = ("prefabs/content/weapons/w_knife_content.prefab", "prefabs/content/weapons/v_knife_content.prefab"),
			["weapon_m4a1"] = ("prefabs/content/weapons/w_m4a1_content.prefab", "prefabs/content/weapons/v_m4a1_content.prefab"),
			["weapon_magnum"] = ("prefabs/content/weapons/w_magnum_content.prefab", "prefabs/content/weapons/v_magnum_content.prefab"),
			["weapon_mp5"] = ("prefabs/content/weapons/w_mp5_content.prefab", "prefabs/content/weapons/v_mp5_content.prefab"),
			["weapon_m700"] = ("prefabs/content/weapons/w_m700_content.prefab", "prefabs/content/weapons/v_m700_content.prefab"),
			["weapon_1911"] = ("prefabs/content/weapons/w_1911_content.prefab", "prefabs/content/weapons/v_1911_content.prefab")
		};

		// CitizenAnimationHelper.HoldType anima la pose de mano/brazo del citizen
		// para que coincida con el tipo de arma; sin esto el modelo cuelga con la
		// bind pose por defecto (aparece flotando en pies/cabeza en vez de en la mano).
		private static readonly Dictionary<string, CitizenAnimationHelper.HoldTypes> HoldTypeByWeapon = new()
		{
			["weapon_usp"] = CitizenAnimationHelper.HoldTypes.Pistol,
			["weapon_magnum"] = CitizenAnimationHelper.HoldTypes.Pistol,
			["weapon_shotgun"] = CitizenAnimationHelper.HoldTypes.Shotgun,
			["weapon_mp5"] = CitizenAnimationHelper.HoldTypes.Rifle,
			["weapon_m4a1"] = CitizenAnimationHelper.HoldTypes.Rifle,
			["weapon_crowbar"] = CitizenAnimationHelper.HoldTypes.Swing,
			["weapon_knife"] = CitizenAnimationHelper.HoldTypes.HoldItem,
			["weapon_m700"] = CitizenAnimationHelper.HoldTypes.Rifle,
			["weapon_1911"] = CitizenAnimationHelper.HoldTypes.Pistol
		};

		[Property] public int HotbarSlots { get; set; } = 6;

		[Sync] public int SelectedSlot { get; private set; } = -1;
		[Sync] public string ActiveItemId { get; private set; } = "";

		private GameObject _weapon;
		private GameObject _viewmodel;
		private string _pendingItemId;
		private string _pendingWorld;
		private string _pendingView;

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			for ( int i = 0; i < HotbarSlots; i++ )
			{
				if ( Input.Pressed( $"Slot{i + 1}" ) )
				{
					SelectSlot( i );
					break;
				}
			}

			if ( Input.Pressed( "Drop" ) )
			{
				DropCurrent();
			}

			var wheel = Input.MouseWheel.y;
			if ( wheel > 0f || wheel < 0f )
			{
				var inv = Components.Get<InventoryComponent>();
				var count = inv?.HotbarSlots ?? HotbarSlots;
				var next = (SelectedSlot + (wheel > 0f ? -1 : 1) + count) % count;
				SelectSlot( next );
			}

			if ( !string.IsNullOrEmpty( _pendingItemId ) )
			{
				var camHost = FindCameraHost();
				var id = _pendingItemId;
				var w = _pendingWorld;
				var v = _pendingView;
				_pendingItemId = null;
				CreateViewmodel( id, v, camHost );
			}

			if ( IsConsumableActive && Input.Pressed( "attack1" ) )
			{
				UseActiveConsumable();
			}

			UpdateViewmodelPresentation();
		}

		// --- Presentación del viewmodel (sway, bob, apuntado, ocultar en 3ª persona) ---
		// Patrón adaptado de sousou63/DarkRP (MIT) ViewModel.UpdateAnimation/ApplyInertia
		// — mismos nombres de parámetro porque el Arms model usa el animgraph estándar
		// del citizen (ver VerificationNotes de cada arma: "skeleton"=1, arms citizen).
		// Vive aquí (no en WeaponContentHost) porque _viewmodel es el único sitio que
		// referencia esa instancia — v_*.prefab no lleva WeaponContentHost, solo w_*.
		private Vector2 _lastAimAngles;
		private Vector2 _aimInertia;
		private bool _swayFirstFrame = true;
		private const float SwayScale = 2f;

		private void UpdateViewmodelPresentation()
		{
			if ( _viewmodel == null || !_viewmodel.IsValid() ) return;

			var weaponRenderer = _viewmodel.Components.Get<SkinnedModelRenderer>();
			if ( !weaponRenderer.IsValid() ) return;

			var player = Components.Get<PlayerController>();
			if ( !player.IsValid() ) return;

			// El viewmodel cuelga de la cámara (CreateViewmodel), así que en tercera
			// persona seguiría pegado al objetivo tapando la vista si no lo ocultamos —
			// el motor solo aleja la cámara, no sabe nada de nuestro viewmodel. Se ocultan
			// TODOS los renderers (arma + Arms bonemerged), no solo el del arma, para no
			// dejar brazos flotando sin cuerpo.
			bool shouldShow = !player.ThirdPerson;
			foreach ( var renderer in _viewmodel.Components.GetAll<SkinnedModelRenderer>( FindMode.EverythingInSelfAndDescendants ) )
			{
				if ( renderer.Enabled != shouldShow ) renderer.Enabled = shouldShow;
			}
			if ( !shouldShow ) return;

			var rot = Scene.Camera.WorldRotation.Angles();

			if ( _swayFirstFrame )
			{
				_lastAimAngles = new Vector2( rot.pitch, rot.yaw );
				_aimInertia = Vector2.Zero;
				_swayFirstFrame = false;
			}

			_aimInertia = new Vector2(
				Angles.NormalizeAngle( rot.pitch - _lastAimAngles.x ),
				Angles.NormalizeAngle( _lastAimAngles.y - rot.yaw ) );
			_lastAimAngles = new Vector2( rot.pitch, rot.yaw );

			weaponRenderer.Set( "aim_body_pitch", rot.pitch );
			weaponRenderer.Set( "aim_pitch_inertia", _aimInertia.x * SwayScale );
			weaponRenderer.Set( "aim_body_yaw", rot.yaw );
			weaponRenderer.Set( "aim_yaw_inertia", _aimInertia.y * SwayScale );
			weaponRenderer.Set( "b_grounded", player.IsOnGround );

			var velocity = player.Velocity;
			var forward = Scene.Camera.WorldRotation.Forward.Dot( velocity );
			var sideward = Scene.Camera.WorldRotation.Right.Dot( velocity );
			var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

			weaponRenderer.Set( "move_direction", angle );
			weaponRenderer.Set( "move_speed", velocity.Length );
			weaponRenderer.Set( "move_groundspeed", velocity.WithZ( 0f ).Length );
			weaponRenderer.Set( "move_x", forward );
			weaponRenderer.Set( "move_y", sideward );
			weaponRenderer.Set( "move_z", velocity.z );

			// Apuntado: el estado real vive en el world model (única instancia con
			// WeaponContentHost); el viewmodel solo refleja el animgraph.
			var isAiming = _weapon != null && _weapon.IsValid()
				&& ( _weapon.Components.GetInDescendantsOrSelf<IUbWeaponRuntime>()?.IsAiming ?? false );
			weaponRenderer.Set( "ironsights", isAiming ? 1 : 0 );
		}

		private GameObject FindCameraHost()
		{
			// Scene.Camera is a CameraComponent, not a GameObject.
			// Get its parent Transform's GameObject, or fall back to self.
			var sceneCam = Scene.Camera;
			if ( sceneCam != null ) return sceneCam.Transform.GameObject;
			return GameObject;
		}

		public void SelectSlot( int index )
		{
			if ( !Networking.IsHost )
			{
				RpcSelectSlot( index );
				return;
			}
			DoSelectSlot( index );
		}

		[Rpc.Host]
		private void RpcSelectSlot( int index )
		{
			DoSelectSlot( index );
		}

		private void DoSelectSlot( int index )
		{
			SelectedSlot = index;

			var inv = Components.Get<InventoryComponent>();
			if ( inv == null || index < 0 || index >= inv.Slots.Count )
			{
				Holster();
				return;
			}

			var slot = inv.Slots[index];
			if ( slot == null || string.IsNullOrEmpty( slot.ItemId ) || slot.Amount <= 0 )
			{
				Holster();
				return;
			}

			var def = ItemRegistry.GetDefinition( slot.ItemId );
			if ( def == null )
			{
				Holster();
				return;
			}

			if ( def.Category == ItemCategory.Consumable && def.Usable )
			{
				ClearEquipped();
				ActiveItemId = slot.ItemId;
				Log.Info( $"[WeaponCarrier] consumible seleccionado {slot.ItemId} (slot {index})" );
				return;
			}

			if ( def.Category != ItemCategory.Firearm && def.Category != ItemCategory.Melee )
			{
				Holster();
				return;
			}

			if ( !WeaponPrefabs.TryGetValue( slot.ItemId, out var paths ) )
			{
				Holster();
				return;
			}

			Equip( slot.ItemId, paths.World, paths.View );
		}

		private void Equip( string itemId, string worldPrefab, string viewPrefab )
		{
			ClearEquipped();
			ActiveItemId = itemId;

			var anim = Components.GetInDescendantsOrSelf<CitizenAnimationHelper>();
			if ( anim != null )
			{
				anim.HoldType = HoldTypeByWeapon.TryGetValue( itemId, out var holdType )
					? holdType
					: CitizenAnimationHelper.HoldTypes.HoldItem;
			}

			SpawnWorldWeapon( worldPrefab );

			var camHost = FindCameraHost();
			if ( camHost == null )
			{
				_pendingItemId = itemId;
				_pendingWorld = worldPrefab;
				_pendingView = viewPrefab;
				return;
			}

			CreateViewmodel( itemId, viewPrefab, camHost );
		}

		private void CreateViewmodel( string itemId, string viewPrefab, GameObject cameraHost )
		{
			var vFile = ResourceLibrary.Get<PrefabFile>( viewPrefab );
			if ( vFile == null )
			{
				Log.Error( $"[WeaponCarrier] view prefab NO encontrado: {viewPrefab}" );
				return;
			}

			_viewmodel = SceneUtility.GetPrefabScene( vFile ).Clone();
			_viewmodel.SetParent( cameraHost );
			_viewmodel.LocalPosition = Vector3.Zero;
			_viewmodel.LocalRotation = Rotation.Identity;

			// Los brazos citizen bonemergeados (Arms, hijo del prefab) necesitan que
			// el arma sepa que está usando ese esqueleto en vez del humano por
			// defecto: parámetro "skeleton" del animgraph (0=humano, 1=citizen),
			// documentado en sbox.game/dev/doc first-person-weapons.
			var weaponSkinned = _viewmodel.Components.Get<SkinnedModelRenderer>();
			weaponSkinned?.Set( "skeleton", 1 );

			_viewmodel.NetworkSpawn( Connection.Local );

			Log.Info( $"[WeaponCarrier] equipada {itemId} (slot {SelectedSlot})" );
		}

		private void SpawnWorldWeapon( string worldPrefab )
		{
			var wFile = ResourceLibrary.Get<PrefabFile>( worldPrefab );
			if ( wFile == null )
			{
				Log.Error( $"[WeaponCarrier] world prefab NO encontrado: {worldPrefab}" );
				return;
			}

			_weapon = SceneUtility.GetPrefabScene( wFile ).Clone();
			_weapon.SetParent( GameObject );
			_weapon.LocalPosition = Vector3.Zero;
			_weapon.LocalRotation = Rotation.Identity;

			// Sin esto el arma queda flotando en el origen del jugador en vez de
			// en la mano. hold_R (confirmado vía ub_qa_list_bones contra el
			// esqueleto real del citizen: hand_L, hold_L, hand_R, hold_R + IK) es
			// el punto pensado para sujetar objetos. Requiere CreateBoneObjects=true
			// en el SkinnedModelRenderer del cuerpo (player.prefab) — sin eso
			// GetBoneObject devuelve null pase lo que pase.
			var bodyRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>();
			var hand = bodyRenderer?.GetBoneObject( "hold_R" ) ?? bodyRenderer?.GetBoneObject( "hand_R" );
			if ( hand != null && hand.IsValid() )
			{
				_weapon.SetParent( hand );
				_weapon.LocalPosition = Vector3.Zero;
				_weapon.LocalRotation = Rotation.Identity;
			}

			_weapon.NetworkSpawn( Connection.Local );
		}

		private void Holster()
		{
			ClearEquipped();
			SelectedSlot = -1;
			ActiveItemId = "";
			_pendingItemId = null;
		}

		private bool IsConsumableActive
		{
			get
			{
				if ( string.IsNullOrEmpty( ActiveItemId ) ) return false;
				var def = ItemRegistry.GetDefinition( ActiveItemId );
				return def != null && def.Category == ItemCategory.Consumable && def.Usable;
			}
		}

		private void UseActiveConsumable()
		{
			var itemId = ActiveItemId;
			if ( string.IsNullOrEmpty( itemId ) ) return;

			if ( !Networking.IsHost )
			{
				RpcUseConsumable( GameObject.Id, itemId );
				return;
			}

			ApplyConsumable( itemId );
		}

		[Rpc.Host]
		private void RpcUseConsumable( Guid playerId, string itemId )
		{
			var go = Scene.Directory.FindByGuid( playerId );
			var carrier = go?.Components.Get<UbWeaponCarrier>();
			if ( carrier != null && carrier.ActiveItemId == itemId )
			{
				carrier.ApplyConsumable( itemId );
			}
		}

		private void ApplyConsumable( string itemId )
		{
			var inv = Components.Get<InventoryComponent>();
			var health = Components.Get<HealthComponent>();
			if ( inv == null || health == null ) return;
			if ( health.IsDead ) return;

			if ( !inv.TryRemove( itemId, 1 ) )
			{
				Log.Info( $"[WeaponCarrier] sin {itemId} en el inventario" );
				Holster();
				return;
			}

			float heal = itemId == "medicine" ? 50f : 25f;
			health.Heal( heal );

			var def = ItemRegistry.GetDefinition( itemId );
			Log.Info( $"[WeaponCarrier] usado {itemId} (+{heal:F0} HP)" );
			RpcConsumableFeedback( itemId, heal, def?.DisplayName ?? itemId );

			var slot = SelectedSlot >= 0 && SelectedSlot < inv.Slots.Count ? inv.Slots[SelectedSlot] : null;
			if ( slot == null || string.IsNullOrEmpty( slot.ItemId ) || slot.Amount <= 0 )
			{
				Holster();
			}
		}

		[Rpc.Broadcast]
		private void RpcConsumableFeedback( string itemId, float heal, string displayName )
		{
			Log.Info( $"[Consumible] {displayName}: +{heal:F0} HP" );
		}

		private void ClearEquipped()
		{
			if ( _weapon != null && _weapon.IsValid() ) _weapon.Destroy();
			if ( _viewmodel != null && _viewmodel.IsValid() ) _viewmodel.Destroy();
			_weapon = null;
			_viewmodel = null;

			var anim = Components.GetInDescendantsOrSelf<CitizenAnimationHelper>();
			if ( anim != null ) anim.HoldType = CitizenAnimationHelper.HoldTypes.None;
		}

		private void DropCurrent()
		{
			if ( string.IsNullOrEmpty( ActiveItemId ) ) return;

			var itemId = ActiveItemId;
			var ammoInMag = _weapon?.Components.GetInDescendantsOrSelf<WeaponContentHost>()?.CurrentAmmo ?? 0;

			if ( Networking.IsHost )
			{
				DropCurrentOnHost( itemId, ammoInMag );
			}
			else
			{
				RpcRequestDropCurrent( itemId, ammoInMag );
			}
		}

		[Rpc.Host]
		private void RpcRequestDropCurrent( string itemId, int ammoInMag )
		{
			DropCurrentOnHost( itemId, ammoInMag );
		}

		private void DropCurrentOnHost( string itemId, int ammoInMag )
		{
			var inventory = Components.Get<InventoryComponent>();
			var definition = ItemRegistry.GetDefinition( itemId );
			if ( inventory == null || definition == null || !definition.Droppable || inventory.GetCount( itemId ) < 1 )
				return;

			// Crear primero el pickup y retirar después el inventario evita pérdidas si
			// falla una validación de suelo, prefab o límite de objetos activos.
			var position = WorldPosition + Vector3.Up * 42f + WorldRotation.Forward * 52f;
			var velocity = WorldRotation.Forward * 120f + Vector3.Up * 50f;
			var pickup = ItemPickupFactory.SpawnPickup( Scene, itemId, 1, ammoInMag, position, velocity );
			if ( pickup == null )
			{
				Log.Warning( $"[WeaponCarrier] Drop rechazado para '{itemId}': no se materializó pickup" );
				return;
			}

			if ( !inventory.TryRemove( itemId, 1 ) )
			{
				pickup.Destroy();
				return;
			}

			Log.Info( $"[WeaponCarrier] drop {itemId} ammo={ammoInMag}" );
			RpcDropConfirmed( GameObject.Id, itemId );
		}

		[Rpc.Broadcast]
		private void RpcDropConfirmed( Guid ownerObjectId, string itemId )
		{
			if ( GameObject.Id != ownerObjectId || ActiveItemId != itemId ) return;
			Holster();
		}
	}
}
