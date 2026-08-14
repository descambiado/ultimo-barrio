using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Combat
{
    public class HealthComponent : Component, IDamageable
    {
        [Property] public float MaxHealth { get; set; } = 100f;

        [Sync] public float Health { get; private set; }

        public bool IsDead => Health <= 0;

        public Action<float, Vector3, Vector3, string> OnDamageTaken;
        public Action OnDeath;
        public Action<float> OnHealed;

        protected override void OnStart()
        {
            if ( Networking.IsHost )
            {
                Health = MaxHealth;
            }
        }

        public void TakeDamage( DamageEvent damageEvent )
        {
            if ( !Networking.IsHost ) return; // Validación host.
            if ( IsDead ) return;

            var before = Health;
            Health -= damageEvent.Amount;
            Health = MathF.Max( 0, Health );

            Log.Info( $"[Damage] source={damageEvent.WeaponId} before={before} damage={damageEvent.Amount} after={Health}" );

            RpcTakeDamageFeedback( damageEvent.Amount, damageEvent.Position, damageEvent.Force, damageEvent.AttackerId );

            if ( Health <= 0 )
            {
                RpcDie();
            }
        }

        /// <summary>
        /// Cura al objetivo (host-only). Clamp a MaxHealth. No revive (no-op si IsDead).
        /// </summary>
        public void Heal(float amount)
        {
            if (!Networking.IsHost) return;
            if (IsDead || amount <= 0) return;

            var before = Health;
            Health = MathF.Min(MaxHealth, Health + amount);
            var applied = Health - before;

            if (applied > 0)
            {
                Log.Info($"[Heal] healed {applied:F0} HP ({before:F0} → {Health:F0})");
                RpcHealFeedback(applied);
            }
        }

        [Rpc.Broadcast]
        private void RpcHealFeedback(float amount)
        {
            OnHealed?.Invoke(amount);
        }

        [Rpc.Broadcast]
        private void RpcTakeDamageFeedback( float amount, Vector3 position, Vector3 force, string attackerId )
        {
            OnDamageTaken?.Invoke( amount, position, force, attackerId );
        }

        [Rpc.Broadcast]
        private void RpcDie()
        {
            OnDeath?.Invoke();
        }

        public void Respawn( Vector3 spawnPosition )
        {
            if ( !Networking.IsHost ) return;
            Health = MaxHealth;
            RpcRespawn( spawnPosition );
        }

        [Rpc.Broadcast]
        private void RpcRespawn( Vector3 spawnPosition )
        {
            WorldPosition = spawnPosition;
            WorldRotation = Rotation.Identity;
        }
    }
}
