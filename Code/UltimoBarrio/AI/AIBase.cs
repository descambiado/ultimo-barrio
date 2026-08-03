using Sandbox;
using System;

namespace UltimoBarrio.AI
{
    public abstract class AIBase : Component, IDamageable
    {
        [RequireComponent] public NavMeshAgent Agent { get; set; }
        [RequireComponent] public PerceptionComponent Perception { get; set; }
        
        [Property] public float MaxHealth { get; set; } = 100f;
        public float Health { get; protected set; }
        public bool IsDead => Health <= 0;
        
        protected override void OnStart()
        {
            Health = MaxHealth;
        }
        
        public virtual void TakeDamage(float amount, Vector3 position, Vector3 force, Guid attackerId)
        {
            if (IsDead) return;
            Health -= amount;
            if (Health <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            GameObject.Destroy();
        }
    }
}
