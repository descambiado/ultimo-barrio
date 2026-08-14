// touch rebuild 2026-08-10 00:50 map-sync
using Sandbox;
using System.Linq;

namespace UltimoBarrio.World;

/// <summary>
/// Autoridad única de spawn inicial. barrio_01 monta un MapInstance
/// (facepunch.flatgrass) como world substrate; el mapa carga de forma
/// asíncrona, así que el spawn espera a OnMapLoaded / IsLoaded antes de
/// clonar el prefab del jugador en el SpawnPoint local. Nunca duplica.
/// </summary>
public sealed class UbPlayerSpawner : Component
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	private MapInstance _map;
	private bool _spawned;

	protected override void OnStart()
	{
		if ( !Networking.IsHost ) return;
		if ( PlayerPrefab is null || SpawnPoint is null )
		{
			Log.Warning( "[UBSpawn] PlayerPrefab o SpawnPoint sin asignar" );
			return;
		}

		_map = Game.ActiveScene.GetAllComponents<MapInstance>().FirstOrDefault();
		if ( !_map.IsValid() )
		{
			// Sin MapInstance: comportamiento legacy (spawn directo).
			SpawnPlayer();
			return;
		}

		if ( _map.IsLoaded )
		{
			Log.Info( "[UBSpawn] map loaded (ya cargado en OnStart)" );
			SpawnPlayer();
			return;
		}

		Log.Info( "[UBSpawn] waiting for map" );
		_map.OnMapLoaded += OnMapLoaded;
	}

	private void OnMapLoaded()
	{
		Log.Info( "[UBSpawn] map loaded" );
		if ( _map.IsValid() ) _map.OnMapLoaded -= OnMapLoaded;
		SpawnPlayer();
	}

	private void SpawnPlayer()
	{
		if ( _spawned ) return;
		_spawned = true;

		// Si ya existe un jugador (NetworkHelper u otro spawner), no duplicar.
		var existing = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault();
		if ( existing.IsValid() )
		{
			Log.Info( $"[UBSpawn] player already present, skip spawn ({existing.GameObject.Name})" );
			return;
		}

		var go = PlayerPrefab.Clone( SpawnPoint.WorldPosition, SpawnPoint.WorldRotation );
		go.NetworkSpawn( Connection.Local );
		go.Name = $"Player - {Connection.Local?.DisplayName ?? "Host"}";
		Log.Info( $"[UBSpawn] spawning player at {SpawnPoint.WorldPosition}" );
	}
}
