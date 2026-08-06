using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;

namespace UltimoBarrio.Fortification
{
    /// <summary>
    /// Fortificación de un apartamento: garantiza estructuras dañables en
    /// puerta y ventanas (auto-registro), gestiona mejoras de nivel y la
    /// reparación host-autoritativa con kits.
    /// </summary>
    [Title( "Apartment Fortification" )]
    [Category( "Último Barrio — Fortification" )]
    [Icon( "fort" )]
    public sealed class ApartmentFortification : Component
    {
        [Property] public string ApartmentId { get; set; } = "apartment-a01";
        [Property] public GameObject DoorReference { get; set; }
        [Property] public float BaseDoorMaxHealth { get; set; } = 250f;
        [Property] public float BaseWindowMaxHealth { get; set; } = 150f;

        [Sync( SyncFlags.FromHost )]
        public int UpgradeLevel { get; private set; }

        public DestructibleStructure DoorStructure { get; private set; }

        protected override void OnStart()
        {
            if ( Networking.IsHost )
            {
                ResolveDoorReference();
                EnsureDoorStructure();
                ApplyUpgradeBonuses();
            }
        }

        /// <summary>
        /// Resuelve DoorReference si no viene fijado en el editor — mismo patrón
        /// que ApartmentComponent.TryValidateConfiguration: busca el hijo con
        /// ApartmentClaimInteractable o cuyo nombre contenga "Claim"/"Door". Evita
        /// depender de wiring manual de referencias GameObject en la escena.
        /// </summary>
        private void ResolveDoorReference()
        {
            if ( DoorReference.IsValid() )
                return;

            DoorReference = GameObject.Components.GetAll<Apartments.ApartmentClaimInteractable>( FindMode.EverythingInSelfAndDescendants ).FirstOrDefault()?.GameObject
                ?? GameObject.Children.FirstOrDefault( c => c.Name.Contains( "Claim" ) || c.Name.Contains( "Door" ) );
        }

        /// <summary>Localiza/crea la estructura de la puerta sobre DoorReference.</summary>
        private void EnsureDoorStructure()
        {
            if ( DoorReference is null || !DoorReference.IsValid() )
                return;

            DoorStructure = DoorReference.Components.GetOrCreate<DestructibleStructure>();
            DoorStructure.StructureId = $"{ApartmentId}-door";
            DoorStructure.MaxHealth = BaseDoorMaxHealth;
            DoorStructure.SetHealth( BaseDoorMaxHealth );
        }

        private void ApplyUpgradeBonuses()
        {
            if ( DoorStructure is not null && DoorStructure.IsValid() )
            {
                DoorStructure.MaxHealth = BaseDoorMaxHealth * FortificationMath.MaxHealthMultiplier( UpgradeLevel );
                DoorStructure.SetHealth( Math.Min( DoorStructure.Health, DoorStructure.MaxHealth ) );
            }

            // Ventanas: estructuras hijas con nombre "Window".
            foreach ( var child in GameObject.Children )
            {
                if ( !child.Name.Contains( "Window", StringComparison.OrdinalIgnoreCase ) )
                    continue;

                var structure = child.Components.GetOrCreate<DestructibleStructure>();
                structure.StructureId = $"{ApartmentId}-{child.Name}";
                structure.MaxHealth = BaseWindowMaxHealth * FortificationMath.MaxHealthMultiplier( UpgradeLevel );
            }
        }

        /// <summary>Mejora de nivel (host): consume materiales y sube el bonus.</summary>
        public bool TryUpgrade( GameObject player )
        {
            if ( !Networking.IsHost || player is null )
                return false;

            if ( !FortificationMath.TryGetUpgradeCost( UpgradeLevel, out var wood, out var scrap, out var components ) )
                return false;

            var inventory = player.Components.GetInDescendantsOrSelf<InventoryComponent>();
            if ( inventory is null )
                return false;

            if ( inventory.GetCount( "wood" ) < wood
                || inventory.GetCount( "chatarra" ) < scrap
                || inventory.GetCount( "components" ) < components )
            {
                UI.PlayerFeedback.Push( "Materiales insuficientes para mejorar" );
                return false;
            }

            inventory.TryRemove( "wood", wood );
            inventory.TryRemove( "chatarra", scrap );
            inventory.TryRemove( "components", components );

            UpgradeLevel++;
            ApplyUpgradeBonuses();

            UI.PlayerFeedback.Push( $"Vivienda mejorada (nivel {UpgradeLevel})" );
            Persistence.PersistenceBridge.RequestSave( "upgrade" );
            Log.Info( $"UB.Fortification Mejora apartment={ApartmentId} nivel={UpgradeLevel}" );
            return true;
        }

        /// <summary>Restaura nivel y salud de puerta desde el snapshot (host).</summary>
        internal void RestoreLevel( int upgradeLevel, float doorHealth, float doorMaxHealth )
        {
            if ( !Networking.IsHost )
                return;

            UpgradeLevel = Math.Clamp( upgradeLevel, 0, FortificationMath.MaxUpgradeLevel );
            ApplyUpgradeBonuses();

            if ( DoorStructure is not null && DoorStructure.IsValid() && doorMaxHealth > 0f )
            {
                DoorStructure.MaxHealth = doorMaxHealth;
                DoorStructure.SetHealth( Math.Clamp( doorHealth, 0f, doorMaxHealth ) );
            }
        }
    }
}
