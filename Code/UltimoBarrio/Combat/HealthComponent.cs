using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio;

namespace UltimoBarrio.Combat
{
    public class HealthComponent : Component, IDamageable
    {
        [Property] public float MaxHealth { get; set; } = 100f;
        
        [Sync] public float Health { get; private set; }
        
        public bool IsDead => Health <= 0;

        public Action<float, Vector3, Vector3, string> OnDamageTaken;
        public Action OnDeath;

        protected override void OnStart()
        {
            if (Networking.IsHost)
            {
                Health = MaxHealth;
            }
        }

        public void TakeDamage(DamageEvent damageEvent)
        {
            if (!Networking.IsHost) return; // Host validation
            if (IsDead) return;

            Health -= damageEvent.Amount;
            Health = MathF.Max(0, Health);
            
            RpcTakeDamageFeedback(damageEvent.Amount, damageEvent.Position, damageEvent.Force, damageEvent.AttackerId);

            if (Health <= 0)
            {
                RpcDie();
            }
        }

        [Rpc.Broadcast]
        private void RpcTakeDamageFeedback(float amount, Vector3 position, Vector3 force, string attackerId)
        {
            OnDamageTaken?.Invoke(amount, position, force, attackerId);
        }

        [Rpc.Broadcast]
        private void RpcDie()
        {
            OnDeath?.Invoke();
        }

        public void Respawn(Vector3 spawnPosition)
        {
            if (!Networking.IsHost) return;
            Health = MaxHealth;
            RpcRespawn(spawnPosition);
        }

        [Rpc.Broadcast]
        private void RpcRespawn(Vector3 spawnPosition)
        {
            Transform.Position = spawnPosition;
            Transform.Rotation = Rotation.Identity;
        }
    }
}
