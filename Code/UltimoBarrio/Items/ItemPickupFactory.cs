using Sandbox;
using System;

namespace UltimoBarrio
{
    /// <summary>
    /// Fábrica única de pickups en el mundo. Todo drop (jugador, muerte, loot)
    /// pasa por aquí para que el repickup devuelva exactamente lo soltado
    /// (incluido el cargador del arma). Devuelve null si no se pudo materializar.
    /// </summary>
    public static class ItemPickupFactory
    {
        public static GameObject SpawnPickup(
            Scene scene,
            string itemId,
            int amount,
            int ammoInMag,
            Vector3 position,
            Vector3? spawnVelocity = null )
        {
            if ( scene is null || string.IsNullOrEmpty( itemId ) || amount <= 0 )
                return null;

            var definition = ItemRegistry.GetDefinition( itemId );
            if ( definition is null )
            {
                Log.Error( $"[ItemPickupFactory] Ítem desconocido '{itemId}'. Drop cancelado." );
                return null;
            }

            var prefabPath = ResolvePickupPrefab( definition );
            var prefabFile = ResourceLibrary.Get<PrefabFile>( prefabPath );
            GameObject pickup = null;

            if ( prefabFile is null )
            {
                Log.Warning($"[ItemPickupFactory] Prefab inválido para '{itemId}'. Spawn rechazado.");
                return null;
            }

            var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
            if ( prefabScene is null ) return null;
            
            // Validate model exist and not a dev placeholder
            var modelRenderer = prefabScene.Components.GetAll<ModelRenderer>(FindMode.EverythingInDescendants).FirstOrDefault();
            if (modelRenderer != null && modelRenderer.Model != null)
            {
                var modelName = modelRenderer.Model.Name.ToLowerInvariant();
                if (modelName.Contains("dev/box") || modelName.Contains("dev/sphere") || modelName.Contains("error.vmdl"))
                {
                    Log.Warning($"[ItemPickupFactory] Modelo dev placeholder '{modelName}' en '{itemId}'. Spawn rechazado.");
                    return null;
                }
            }

            // Validar MaxActive (límite provisional de 100 objetos dinámicos)
            if ( scene.GetAllComponents<WorldItemPickup>().Count() > 100 )
            {
                Log.Warning("[ItemPickupFactory] MaxActive alcanzado. Spawn rechazado.");
                return null;
            }

            // Validar posición con suelo
            var tr = scene.Trace.Ray( position + Vector3.Up * 10f, position + Vector3.Down * 500f ).Run();
            if ( !tr.Hit )
            {
                Log.Warning($"[ItemPickupFactory] Posición sin suelo para '{itemId}'. Spawn rechazado.");
                return null;
            }

            pickup = prefabScene.Clone();

            pickup.WorldPosition = position;

            var pickupComp = pickup.Components.Get<WorldItemPickup>( FindMode.EverythingInSelfAndDescendants );
            if ( pickupComp is null )
            {
                pickup.Destroy();
                return null;
            }

            pickupComp.ItemId = itemId;
            pickupComp.Amount = amount;
            pickupComp.AmmoInMag = ammoInMag;

            if ( Networking.IsActive )
                pickup.NetworkSpawn();

            if ( spawnVelocity.HasValue )
            {
                var body = pickup.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndDescendants );
                if ( body is not null )
                    body.Velocity = spawnVelocity.Value;
            }

            return pickup;
        }

        public static string ResolvePickupPrefab( ItemDefinition definition )
        {
            if ( definition is not null && !string.IsNullOrEmpty( definition.WorldPrefab ) )
                return definition.WorldPrefab;

            // Fallback genérico por categoría.
            switch ( definition?.Category )
            {
                case ItemCategory.Ammo:
                    return "prefabs/items/pf_ammo_9mm_pickup.prefab";
                case ItemCategory.Firearm:
                    return "prefabs/items/pf_usp_pickup.prefab";
                default:
                    return "prefabs/items/pf_scrap_pickup.prefab";
            }
        }
    }
}
