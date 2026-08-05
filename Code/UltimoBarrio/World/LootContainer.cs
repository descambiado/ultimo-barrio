using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;

namespace UltimoBarrio.World
{
    /// <summary>
    /// Contenedor de loot (versus pickup suelto): se rellena en el host al
    /// arrancar con una tabla y se abre como alijo (UI de stash). Los
    /// contenidos se replican con [Sync] desde el host.
    /// </summary>
    [Title( "Loot Container" )]
    [Category( "Último Barrio — World" )]
    [Icon( "inventory_2" )]
    public class LootContainer : Component, IWorldContainer, IWorldInteractable, IInteractable
    {
        [Property] public string LootTableId { get; set; } = "StreetLoot";
        [Property] public int MinItems { get; set; } = 2;
        [Property] public int MaxItems { get; set; } = 4;
        [Property] public float MaxInteractionDistance { get; set; } = 200f;

        [RequireComponent] public InventoryComponent Inventory { get; set; }

        private bool _filled;

        protected override void OnStart()
        {
            if ( Networking.IsHost && !_filled )
                FillFromTable();
        }

        /// <summary>Rellena el contenedor con la tabla configurada (host).</summary>
        public void FillFromTable()
        {
            if ( Inventory is null || !Networking.IsHost )
                return;

            _filled = true;
            Inventory.Slots.Clear();
            for ( int i = 0; i < Inventory.MaxSlots; i++ )
                Inventory.Slots.Add( new InventorySlot { ItemId = "", Amount = 0 } );

            int count = Math.Max( MinItems, MinItems + (int)( Random.Shared.NextDouble() * Math.Max( 1, MaxItems - MinItems + 1 ) ) );

            for ( int i = 0; i < count; i++ )
            {
                var entry = LootTables.Roll( LootTableId );
                if ( entry is null )
                    continue;

                int amount = entry.MinAmount;
                if ( entry.MaxAmount > entry.MinAmount )
                    amount += (int)( Random.Shared.NextDouble() * ( entry.MaxAmount - entry.MinAmount + 1 ) );

                Inventory.TryAdd( entry.ItemId, amount );
            }

            Log.Info( $"[Loot] Contenedor '{GameObject.Name}' rellenado desde {LootTableId} ({count} ítems)." );
        }

        public InventoryComponent GetContainerInventory() => Inventory;

        public string GetInteractionPrompt( InteractionRequest request )
        {
            var def = !string.IsNullOrEmpty( LootTableId ) ? LootTableId : "contenedor";
            return $"Registrar contenedor ({def})";
        }

        public bool CanInteract( InteractionRequest request )
        {
            if ( request.InteractorObject == null ) return false;
            return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
        }

        public void OnInteract( InteractionRequest request )
        {
            var hud = request.InteractorObject?.Components.GetInDescendantsOrSelf<UI.PlayerHud>();
            if ( hud is not null && Inventory is not null )
                hud.OpenStash( Inventory );
        }
    }
}
