using Sandbox;
using System;
using System.Linq;

namespace UltimoBarrio
{
    public class InventorySlot
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }

        /// <summary>
        /// Munición actual del cargador del arma apilada en este slot.
        /// El inventario es la única fuente de verdad del cargador: sobrevive
        /// al cambio de slot, al drop/re-pickup y al guardado.
        /// </summary>
        public int AmmoInMag { get; set; }
    }

    public class InventoryComponent : Component, IInventory
    {
        [Property] public string InventoryId { get; set; } = string.Empty;
        [Property] public int MaxSlots { get; set; } = 24;
        [Property] public int HotbarSlots { get; set; } = 6;

        [Sync] public NetList<InventorySlot> Slots { get; set; } = new NetList<InventorySlot>();

        protected override void OnAwake()
        {
            if ( Slots.Count == 0 && !IsProxy )
            {
                for ( int i = 0; i < MaxSlots; i++ )
                    Slots.Add( new InventorySlot { ItemId = "", Amount = 0 } );
            }
        }

        public bool CanAdd( string itemId, int amount )
        {
            if ( string.IsNullOrEmpty( itemId ) || amount <= 0 )
                return false;

            var definition = ItemRegistry.GetDefinition( itemId );
            var stackSize = definition?.StackSize ?? 64;

            int remaining = amount;
            foreach ( var slot in Slots )
            {
                if ( slot.ItemId == itemId && slot.Amount < stackSize )
                    remaining -= stackSize - slot.Amount;
                if ( remaining <= 0 )
                    return true;
            }

            int freeSlots = Slots.Count( s => string.IsNullOrEmpty( s.ItemId ) );
            int neededSlots = ( remaining + stackSize - 1 ) / stackSize;
            return freeSlots >= neededSlots;
        }

        public bool TryAdd( string itemId, int amount )
        {
            if ( IsProxy ) return false;
            if ( !CanAdd( itemId, amount ) ) return false;

            var definition = ItemRegistry.GetDefinition( itemId );
            var stackSize = definition?.StackSize ?? 64;

            int remaining = amount;

            // 1) Apilar sobre stacks existentes del mismo ítem.
            foreach ( var slot in Slots )
            {
                if ( remaining <= 0 ) break;
                if ( slot.ItemId != itemId ) continue;

                int space = stackSize - slot.Amount;
                if ( space <= 0 ) continue;

                int toAdd = Math.Min( space, remaining );
                slot.Amount += toAdd;
                remaining -= toAdd;
            }

            // 2) Llenar slots vacíos.
            foreach ( var slot in Slots )
            {
                if ( remaining <= 0 ) break;
                if ( !string.IsNullOrEmpty( slot.ItemId ) ) continue;

                int toAdd = Math.Min( stackSize, remaining );
                slot.ItemId = itemId;
                slot.Amount = toAdd;
                remaining -= toAdd;
            }

            return remaining == 0;
        }

        /// <summary>
        /// Añade ítems devolviendo el slot donde terminó (usado por pickups
        /// para transferir el cargador del arma recogida). Devuelve null si falla.
        /// </summary>
        public InventorySlot AddItem( string itemId, int amount, int ammoInMag = 0 )
        {
            if ( !TryAdd( itemId, amount ) )
                return null;

            // El ítem terminó en el último slot no vacío de ese id.
            var definition = ItemRegistry.GetDefinition( itemId );
            if ( definition is not null && definition.IsWeapon && ammoInMag > 0 )
            {
                var slot = Slots.LastOrDefault( s => s.ItemId == itemId && s.Amount > 0 );
                if ( slot is not null )
                    slot.AmmoInMag = Math.Clamp( ammoInMag, 0, Math.Max( 1, definition.MagazineSize ) );
            }

            return Slots.LastOrDefault( s => s.ItemId == itemId && s.Amount > 0 );
        }

        public bool TryRemove( string itemId, int amount )
        {
            if ( IsProxy ) return false;

            if ( GetCount( itemId ) < amount )
                return false;

            int remaining = amount;
            for ( int i = Slots.Count - 1; i >= 0 && remaining > 0; i-- )
            {
                var slot = Slots[i];
                if ( slot.ItemId != itemId ) continue;

                int toRemove = Math.Min( slot.Amount, remaining );
                slot.Amount -= toRemove;
                remaining -= toRemove;

                if ( slot.Amount <= 0 )
                {
                    slot.ItemId = "";
                    slot.Amount = 0;
                    slot.AmmoInMag = 0;
                }
            }

            return remaining == 0;
        }

        public int GetCount( string itemId )
        {
            int total = 0;
            foreach ( var slot in Slots )
            {
                if ( slot.ItemId == itemId )
                    total += slot.Amount;
            }
            return total;
        }

        /// <summary>Peso total del inventario (penalización de movimiento).</summary>
        public float GetTotalWeight()
        {
            float total = 0f;
            foreach ( var slot in Slots )
            {
                if ( string.IsNullOrEmpty( slot.ItemId ) || slot.Amount <= 0 )
                    continue;

                var definition = ItemRegistry.GetDefinition( slot.ItemId );
                if ( definition is not null )
                    total += definition.Weight * slot.Amount;
            }
            return total;
        }

        [Rpc.Host]
        public void RequestTransfer( string itemId, int amount, Guid targetInventoryId )
        {
            var targetInv = Scene.GetAllComponents<InventoryComponent>()
                .FirstOrDefault( c => c.GameObject.Id == targetInventoryId );
            if ( targetInv == null ) return;

            // Autorización: el stash valida propietario; aquí solo se transfiere
            // si el destino acepta (atomicidad con reembolso).
            if ( TryRemove( itemId, amount ) )
            {
                if ( !targetInv.TryAdd( itemId, amount ) )
                {
                    // Rollback: devolver lo extraído.
                    TryAdd( itemId, amount );
                }
            }
        }

        [Rpc.Host]
        public void RequestDrop( string itemId, int amount )
        {
            if ( TryRemove( itemId, amount ) )
            {
                var pickup = ItemPickupFactory.SpawnPickup( Scene, itemId, amount, 0,
                    GameObject.WorldPosition + Vector3.Up * 50f + GameObject.WorldRotation.Forward * 50f );

                if ( pickup is null )
                {
                    // Rollback: no se pudo materializar el pickup.
                    TryAdd( itemId, amount );
                }
            }
        }
    }
}
