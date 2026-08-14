// UbWeaponCarrier — connects hotbar inventory to weapon prefabs
using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Core;

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
			["weapon_knife"] = ("prefabs/content/weapons/w_knife.prefab", "prefabs/content/weapons/v_knife.prefab")
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
		}

		private void DropCurrent()
		{
			if ( string.IsNullOrEmpty( ActiveItemId ) ) return;

			var inv = Components.Get<InventoryComponent>();
			Log.Info( $"[WeaponCarrier] drop {ActiveItemId}" );
			inv?.RequestDrop( ActiveItemId, 1 );
			Holster();
		}
	}
}