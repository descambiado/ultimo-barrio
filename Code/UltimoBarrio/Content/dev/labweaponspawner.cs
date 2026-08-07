using Sandbox;
using System;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Spawner de laboratorio para el weapon_lab.
	/// SOLO dev: no forma parte del contenido portable de producción.
	/// Teclas: Slot1..Slot4 → USP, Palanca, Cuchillo, Escopeta (se equipan en el jugador local).
	/// </summary>
	[Title( "Lab Weapon Spawner" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class LabWeaponSpawner : Component
	{
		private static readonly string[] WeaponPrefabs =
		{
			"prefabs/content/weapons/w_usp_content.prefab",
			"prefabs/content/weapons/w_crowbar_content.prefab",
			"prefabs/content/weapons/w_knife_content.prefab",
			"prefabs/content/weapons/w_shotgun_content.prefab"
		};

		private GameObject _currentWeapon;

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			if ( Input.Pressed( "Slot1" ) ) SpawnWeapon( 0 );
			else if ( Input.Pressed( "Slot2" ) ) SpawnWeapon( 1 );
			else if ( Input.Pressed( "Slot3" ) ) SpawnWeapon( 2 );
			else if ( Input.Pressed( "Slot4" ) ) SpawnWeapon( 3 );
			else if ( Input.Pressed( "Drop" ) ) ClearWeapon();
		}

		private void SpawnWeapon( int index )
		{
			if ( index < 0 || index >= WeaponPrefabs.Length ) return;

			ClearWeapon();

			var prefabFile = ResourceLibrary.Get<PrefabFile>( WeaponPrefabs[index] );
			if ( prefabFile == null )
			{
				Log.Error( $"[Lab] Prefab no encontrado: {WeaponPrefabs[index]}" );
				return;
			}

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null ) return;

			var weapon = scene.Clone();
			var parent = FindLocalPlayerBody();
			if ( parent != null )
			{
				weapon.SetParent( parent );
				weapon.LocalPosition = Vector3.Zero;
				weapon.LocalRotation = Rotation.Identity;
			}
			else
			{
				weapon.WorldPosition = WorldPosition + Vector3.Up * 40f;
			}

			weapon.NetworkSpawn( Connection.Local );
			_currentWeapon = weapon;

			Log.Info( $"[Lab] Arma equipada: {WeaponPrefabs[index]}" );
		}

		private void ClearWeapon()
		{
			_currentWeapon?.Destroy();
			_currentWeapon = null;
		}

		private GameObject FindLocalPlayerBody()
		{
			// Lab de un jugador: el primer CharacterController es el jugador local.
			foreach ( var controller in Scene.GetAllComponents<CharacterController>() )
			{
				return controller.GameObject;
			}
			return null;
		}
	}
}
