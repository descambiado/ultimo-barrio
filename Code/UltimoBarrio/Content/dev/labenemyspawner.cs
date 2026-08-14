using Sandbox;
using System;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Spawner de laboratorio para el enemy_lab.
	/// SOLO dev. Teclas: Slot1..Slot3 → Saqueador, Bruto, Merodeador en el marcador.
	/// El objetivo (dummy o jugador) se asigna si existe.
	/// </summary>
	[Title( "Lab Enemy Spawner" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class LabEnemySpawner : Component
	{
		private static readonly string[] EnemyPrefabs =
		{
			"prefabs/content/enemies/enemy_saqueador.prefab",
			"prefabs/content/enemies/enemy_bruto.prefab",
			"prefabs/content/enemies/enemy_merodeador.prefab"
		};

		[Property] public GameObject SpawnMarker { get; set; }
		[Property] public GameObject TargetDummy { get; set; }

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			if ( Input.Pressed( "Slot1" ) ) SpawnEnemy( 0 );
			else if ( Input.Pressed( "Slot2" ) ) SpawnEnemy( 1 );
			else if ( Input.Pressed( "Slot3" ) ) SpawnEnemy( 2 );
		}

		private void SpawnEnemy( int index )
		{
			if ( index < 0 || index >= EnemyPrefabs.Length ) return;

			var prefabFile = ResourceLibrary.Get<PrefabFile>( EnemyPrefabs[index] );
			if ( prefabFile == null )
			{
				Log.Error( $"[Lab] Prefab no encontrado: {EnemyPrefabs[index]}" );
				return;
			}

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null ) return;

			var enemy = scene.Clone();
			enemy.WorldPosition = SpawnMarker != null ? SpawnMarker.WorldPosition : WorldPosition + Vector3.Up * 40f;
			enemy.NetworkSpawn( Connection.Local );

			var host = enemy.Components.GetInDescendantsOrSelf<Content.Enemies.EnemyContentHost>();
			if ( host != null )
			{
				var target = TargetDummy ?? FindLocalPlayer();
				if ( target != null ) host.SetTarget( target );
			}

			Log.Info( $"[Lab] Enemigo spawnado: {EnemyPrefabs[index]}" );
		}

		private GameObject FindLocalPlayer()
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
