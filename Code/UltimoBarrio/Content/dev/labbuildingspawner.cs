using Sandbox;
using System;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Spawner de laboratorio para el building_lab.
	/// SOLO dev. Teclas:
	///   Slot1..Slot9 → barricada madera, barricada reforzada, puerta básica, puerta reforzada,
	///                  alijo, banco de trabajo, generador, alarma, estación de reparación.
	///   R → repara la fortificación más cercana (dentro de 200 unidades).
	/// </summary>
	[Title( "Lab Building Spawner" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class LabBuildingSpawner : Component
	{
		private static readonly string[] FortificationPrefabs =
		{
			"prefabs/content/fortification/fort_barricade_wood.prefab",
			"prefabs/content/fortification/fort_barricade_reinforced.prefab",
			"prefabs/content/fortification/fort_door_basic.prefab",
			"prefabs/content/fortification/fort_door_reinforced.prefab",
			"prefabs/content/fortification/fort_stash.prefab",
			"prefabs/content/fortification/fort_workbench.prefab",
			"prefabs/content/fortification/fort_generator.prefab",
			"prefabs/content/fortification/fort_alarm.prefab",
			"prefabs/content/fortification/fort_repair_station.prefab"
		};

		[Property] public float PlacementDistance { get; set; } = 120f;

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			for ( int i = 0; i < FortificationPrefabs.Length; i++ )
			{
				if ( Input.Pressed( $"Slot{i + 1}" ) )
				{
					SpawnFortification( i );
					return;
				}
			}

			if ( Input.Pressed( "reload" ) )
			{
				RepairNearest();
			}
		}

		private void SpawnFortification( int index )
		{
			if ( index < 0 || index >= FortificationPrefabs.Length ) return;

			var prefabFile = ResourceLibrary.Get<PrefabFile>( FortificationPrefabs[index] );
			if ( prefabFile == null )
			{
				Log.Error( $"[Lab] Prefab no encontrado: {FortificationPrefabs[index]}" );
				return;
			}

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null ) return;

			var obj = scene.Clone();
			obj.WorldPosition = EyePosition() + EyeRotation().Forward * PlacementDistance;
			obj.WorldRotation = Rotation.FromYaw( EyeRotation().Yaw() );
			obj.NetworkSpawn( Connection.Local );

			Log.Info( $"[Lab] Fortificación colocada: {FortificationPrefabs[index]}" );
		}

		private void RepairNearest()
		{
			Fortification.FortificationContentHost nearest = null;
			float nearestDist = 200f;

			foreach ( var host in Scene.GetAllComponents<Fortification.FortificationContentHost>() )
			{
				float dist = Vector3.DistanceBetween( host.WorldPosition, WorldPosition );
				if ( dist < nearestDist )
				{
					nearestDist = dist;
					nearest = host;
				}
			}

			if ( nearest == null ) return;

			nearest.Repair( nearest.Definition?.RepairAmount ?? 25f );
			Log.Info( $"[Lab] Reparada fortificación {nearest.DefinitionId} (+{nearest.Definition?.RepairAmount ?? 25f})" );
		}

		private Vector3 EyePosition()
		{
			var camera = Scene.Camera;
			return camera != null ? camera.WorldPosition : WorldPosition + Vector3.Up * 60f;
		}

		private Rotation EyeRotation()
		{
			var camera = Scene.Camera;
			return camera != null ? camera.WorldRotation : WorldRotation;
		}
	}
}
