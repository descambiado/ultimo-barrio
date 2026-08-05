using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio.World;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Base reutilizable de NPC hostil: NavMeshAgent + percepción + combate
    /// melee (rango/daño/cooldown) + loot al morir.
    /// </summary>
    public abstract class AIBase : Component, IDamageable
    {
        [RequireComponent] public NavMeshAgent Agent { get; set; }
        [RequireComponent] public PerceptionComponent Perception { get; set; }

        [Property] public float MaxHealth { get; set; } = 100f;
        [Property] public float AttackRange { get; set; } = 100f;
        [Property] public float AttackDamage { get; set; } = 10f;
        [Property] public float AttackCooldown { get; set; } = 1.2f;
        [Property] public bool DropsLootOnDeath { get; set; } = true;
        [Property] public string DeathLootTable { get; set; } = "StreetLoot";

        public float Health { get; protected set; }
        public bool IsDead => Health <= 0;

        private TimeSince _timeSinceAttack;

        protected override void OnStart()
        {
            Health = MaxHealth;
        }

        public virtual void TakeDamage( DamageEvent damageEvent )
        {
            if ( IsDead || !Networking.IsHost )
                return;

            Health -= damageEvent.Amount;
            if ( Health <= 0 )
                Die();
        }

        /// <summary>Ataque melee contra un objetivo dentro de rango.</summary>
        public bool TryAttack( GameObject target )
        {
            if ( target is null || !target.IsValid() || IsDead )
                return false;

            if ( _timeSinceAttack < AttackCooldown )
                return false;

            if ( Vector3.DistanceBetween( WorldPosition, target.WorldPosition ) > AttackRange )
                return false;

            _timeSinceAttack = 0f;

            var damageable = target.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>();
            if ( damageable != null )
            {
                var dmg = new DamageEvent
                {
                    Amount = AttackDamage,
                    Position = target.WorldPosition,
                    Force = ( target.WorldPosition - WorldPosition ).Normal * 150f,
                    AttackerId = GameObject.Name,
                    WeaponId = "npc_melee"
                };

                damageable.TakeDamage( dmg );
                return true;
            }

            return false;
        }

        protected virtual void Die()
        {
            if ( Networking.IsHost && DropsLootOnDeath )
            {
                int drops = 1 + (int)( Random.Shared.NextDouble() * 2f );
                LootSpawner.SpawnPickups( Scene, DeathLootTable, WorldPosition + Vector3.Up * 20f, drops );
            }

            GameObject.Destroy();
        }
    }
}
