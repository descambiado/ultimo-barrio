using Sandbox;
using System;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Spawner de laboratorio para el vehicle_lab.
	/// SOLO dev. Los prefabs de vehículo se rellenan cuando el research de vehículos
	/// decida el paquete (Vehicle Physics Kit u otro). Tecla Slot1 → spawn del vehículo 0.
	/// </summary>
	[Title( "Lab Vehicle Spawner" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class LabVehicleSpawner : Component
	{
		// PENDING: rellenar tras decisión de research (docs/research/laptop-content-integration-manifest.md, bloque H).
		private static readonly string[] VehiclePrefabs = { };

		[Property] public GameObject SpawnMarker { get; set; }

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			if ( Input.Pressed( "Slot1" ) )
			{
				if ( VehiclePrefabs.Length == 0 )
				{
					Log.Info( "[Lab] Vehicle lab: sin paquete de vehículos decidido aún (ver manifest bloque H)." );
					return;
				}

				SpawnVehicle( 0 );
			}
		}

		private void SpawnVehicle( int index )
		{
			if ( index < 0 || index >= VehiclePrefabs.Length ) return;

			var prefabFile = ResourceLibrary.Get<PrefabFile>( VehiclePrefabs[index] );
			if ( prefabFile == null )
			{
				Log.Error( $"[Lab] Prefab no encontrado: {VehiclePrefabs[index]}" );
				return;
			}

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null ) return;

			var vehicle = scene.Clone();
			vehicle.WorldPosition = SpawnMarker != null ? SpawnMarker.WorldPosition : WorldPosition + Vector3.Up * 40f;
			vehicle.NetworkSpawn( Connection.Local );
		}
	}
}
