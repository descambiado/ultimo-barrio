using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Core;

namespace UltimoBarrio.Combat
{
	/// <summary>
	/// Weapon Carrier — capa de juego que conecta el inventario/hotbar del jugador
	/// con las armas del pack de contenido (WeaponContentHost).
	///
	/// REUSE > BUILD: no reinventa viewmodels, reload, ammo, ni networking. El arma
	/// clonada (prefab w_*_content) trae WeaponContentHost con AutoHandleInput=true,
	/// que ya gestiona fire/reload/dry-fire/sonidos/daño por IDamageTarget. Este
	/// componente solo:
	///   - lee el slot del hotbar (1..6) del InventoryComponent,
	///   - instancia el arma world (hija del jugador) y el viewmodel (hija de la cámara),
	///   - holstera al cambiar a un slot vacío,
	///   - suelta el arma actual con la tecla de drop.
	///
	/// Sustituye a HeldItemController en el prefab del jugador (que apuntaba a la
	/// pila vieja BaseCombatWeapon, incompatible con los enemigos del content pack).
	/// </summary>
	[Title( "Weapon Carrier" )]
	[Category( "Ultimo Barrio — Combat" )]
	[Icon( "gps_fixed" )]
	public sealed class UbWeaponCarrier : Component
	{
		// itemId (inventario) → (world prefab, view prefab) del pack verificado.
		private static readonly Dictionary<string, (string World, string View)> WeaponPrefabs = new()
		{
			["weapon_usp"] = ("prefabs/content/weapons/w_usp_content.prefab", "prefabs/content/weapons/v_usp_content.prefab"),
			["weapon_shotgun"] = ("prefabs/content/weapons/w_shotgun_content.prefab", "prefabs/content/weapons/v_shotgun_content.prefab"),
			["weapon_crowbar"] = ("prefabs/content/weapons/w_crowbar_content.prefab", "prefabs/content/weapons/v_crowbar_content.prefab"),
			["weapon_knife"] = ("prefabs/content/weapons/w_knife_content.prefab", "prefabs/content/weapons/v_knife_content.prefab")
		};

		[Property] public int HotbarSlots { get; set; } = 6;

		[Sync] public int SelectedSlot { get; private set; } = -1;
		[Sync] public string ActiveItemId { get; private set; } = "";

		private GameObject _weapon;
		private GameObject _viewmodel;

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			for ( int i = 0; i < HotbarSlots; i++ )
			{
				if ( Input.Pressed( $"Slot{i + 1}" ) )
				{
					Log.Info( $"[WeaponCarrier] hotbar slot={i}" );
					SelectSlot( i );
					break;
				}
			}

			if ( Input.Pressed( "Drop" ) )
			{
				DropCurrent();
			}
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
			if ( def == null || ( def.Category != ItemCategory.Firearm && def.Category != ItemCategory.Melee ) )
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

			var vFile = ResourceLibrary.Get<PrefabFile>( viewPrefab );
			if ( vFile == null )
			{
				Log.Error( $"[WeaponCarrier] view prefab NO encontrado: {viewPrefab}" );
				return;
			}

			var cam = Components.Get<CameraComponent>() ?? Scene.GetAllComponents<CameraComponent>().FirstOrDefault( c => c.IsMainCamera );
			if ( cam == null )
			{
				Log.Warning( "[WeaponCarrier] sin CameraComponent: arma sin viewmodel" );
				return;
			}

			_viewmodel = SceneUtility.GetPrefabScene( vFile ).Clone();
			_viewmodel.SetParent( cam.GameObject );
			_viewmodel.LocalPosition = Vector3.Zero;
			_viewmodel.LocalRotation = Rotation.Identity;
			_viewmodel.NetworkSpawn( Connection.Local );

			Log.Info( $"[WeaponCarrier] equipada {itemId} (slot {SelectedSlot})" );
		}

		private void Holster()
		{
			ClearEquipped();
			SelectedSlot = -1;
			ActiveItemId = "";
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
