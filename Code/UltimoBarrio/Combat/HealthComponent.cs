using Sandbox;
using System;
using UltimoBarrio;

namespace UltimoBarrio.Combat
{
    public class HealthComponent : Component, UltimoBarrio.IDamageable
    {
        [Property] public float MaxHealth { get; set; } = 100f;
        
        [Sync] public float Health { get; private set; }
        
        public bool IsDead => Health <= 0;

        public Action<float, Vector3, Vector3, Guid> OnDamageTaken;
        public Action OnDeath;

        protected override void OnStart()
        {
            if (Networking.IsHost)
            {
                Health = MaxHealth;
            }
        }

        public void TakeDamage(float amount, Vector3 position, Vector3 force, Guid attackerId)
        {
            if (!Networking.IsHost) return; // Host validation
            if (IsDead) return;

            Health -= amount;
            Health = MathF.Max(0, Health);
            
            RpcTakeDamageFeedback(amount, position, force, attackerId);

            if (Health <= 0)
            {
                RpcDie();
            }
        }

        [Broadcast]
        private void RpcTakeDamageFeedback(float amount, Vector3 position, Vector3 force, Guid attackerId)
        {
            OnDamageTaken?.Invoke(amount, position, force, attackerId);
        }

        [Broadcast]
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

        [Broadcast]
        private void RpcRespawn(Vector3 spawnPosition)
        {
            Transform.Position = spawnPosition;
            Transform.Rotation = Rotation.Identity;
        }
    }
}
