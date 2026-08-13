using Sandbox;
using System.Linq;

namespace UltimoBarrio.Development
{
	/// <summary>
	/// Bootstrap de contenido del mundo en runtime.
	///
	/// Coloca elementos de mundo que no conviene incrustar a mano en la escena
	/// (prefabs con GUIDs/propiedades cambiantes): la estación de crafteo junto al
	/// comerciante. Idempotente: si ya existe una CraftingStation en la escena, no
	/// duplica. Solo corre en host.
	/// </summary>
	[Title( "World Content Bootstrap" )]
	[Category( "Development" )]
	[Icon( "construction" )]
	public sealed class WorldContentBootstrap : Component
	{
		[Property] public string CraftingStationPrefab { get; set; } = "prefabs/world/ub_crafting_station.prefab";
		[Property] public float CraftingStationOffset { get; set; } = 120f;

		private bool _spawned;

		protected override void OnStart()
		{
			if ( Scene.IsEditor || !Networking.IsHost ) return;

			SpawnCraftingStation();
		}

		protected override void OnUpdate()
		{
			// El trader puede no existir aún en el primer OnStart (orden de componentes);
			// reintentamos hasta colocarla una vez.
			if ( _spawned || Scene.IsEditor || !Networking.IsHost ) return;
			SpawnCraftingStation();
		}

		private void SpawnCraftingStation()
		{
			_spawned = true;

			// No duplicar si ya hay una.
			if ( Scene.GetAllComponents<Crafting.CraftingStation>().Any() )
			{
				Log.Info( "[WorldBootstrap] CraftingStation ya presente; no se duplica." );
				return;
			}

			var prefabFile = ResourceLibrary.Get<PrefabFile>( CraftingStationPrefab );
			if ( prefabFile == null )
			{
				Log.Error( $"[WorldBootstrap] Prefab de crafting NO encontrado: {CraftingStationPrefab}" );
				return;
			}

			var trader = Scene.GetAllComponents<Trading.Trader>().FirstOrDefault();
			if ( trader == null )
			{
				Log.Warning( "[WorldBootstrap] Sin trader en la escena; la estación de crafteo no se coloca." );
				return;
			}

			var station = SceneUtility.GetPrefabScene( prefabFile ).Clone();
			station.WorldPosition = trader.WorldPosition + trader.WorldRotation.Right * CraftingStationOffset;
			station.WorldRotation = trader.WorldRotation;
			station.NetworkSpawn();

			Log.Info( $"[WorldBootstrap] CraftingStation colocada en {station.WorldPosition}" );
		}
	}
}
