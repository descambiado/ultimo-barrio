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

            if ( prefabFile is not null )
            {
                var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
                if ( prefabScene is not null )
                    pickup = prefabScene.Clone();
            }

            if ( pickup is null )
            {
                // Fallback: pickup procedural sin prefab.
                pickup = new GameObject( true, $"{itemId} pickup" );
                pickup.WorldPosition = position;

                var collider = pickup.Components.Create<BoxCollider>();
                collider.Scale = new Vector3( 12, 12, 12 );

                var renderer = pickup.Components.Create<ModelRenderer>();
                renderer.Model = Model.Load( "models/dev/box.vmdl" );
                renderer.Tint = definition.Category == ItemCategory.Firearm
                    ? new Color( 0.25f, 0.25f, 0.3f )
                    : new Color( 0.5f, 0.45f, 0.3f );

                pickup.Components.Create<WorldItemPickup>();
            }

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
