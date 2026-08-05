using UltimoBarrio.Core;
using Sandbox;
using System;
using UltimoBarrio.Players;
using UltimoBarrio.UI;

namespace UltimoBarrio.Combat
{
    [Title( "Melee Weapon" )]
    [Category( "Último Barrio — Combat" )]
    [Icon( "pan_tool" )]
    public class MeleeWeapon : BaseCombatWeapon
    {
        [Property] public float StaminaCost { get; set; } = 10f;
        [Property] public float PushForce { get; set; } = 200f;
        [Property] public SoundEvent HitSound { get; set; }
        [Property] public SoundEvent MissSound { get; set; }
        [Property] public SoundEvent WallHitSound { get; set; }

        private TimeSince _timeSinceMeleeFire;

        protected override void OnStart()
        {
            base.OnStart();
            Range = 80f;
            BaseDamage = 25f;
            FireRate = 0.5f;
            MaxAmmo = 1;
            CurrentAmmo = 1;
        }

        protected override void HandleInput()
        {
            if ( IsReloading )
                return;

            if ( IsUiCapturingInput() )
                return;

            bool wantToFire = Input.Pressed( "attack1" );
            if ( wantToFire )
                TrySwing();
        }

        /// <summary>
        /// Golpe melee invocable desde el HeldItemController (puños) o desde el
        /// input del arma. Valida stamina y cooldown, traza y aplica daño.
        /// Flujo: input → HeldItemController → MeleeWeapon → trace → HealthComponent.
        /// </summary>
        public bool TrySwing()
        {
            if ( IsProxy )
                return false;

            if ( _timeSinceMeleeFire < FireRate )
                return false;

            var movement = Components.GetInAncestorsOrSelf<PlayerMovementModifier>();
            if ( movement is not null && movement.CurrentStamina < StaminaCost )
                return false;

            if ( movement is not null )
                movement.CurrentStamina = Math.Max( 0f, movement.CurrentStamina - StaminaCost );

            _timeSinceMeleeFire = 0;
            PerformTrace();
            return true;
        }

        protected override void PerformTrace()
        {
            var ray = Scene.Camera.ScreenNormalToRay( 0.5f );

            var tr = Scene.Trace.Ray( ray, Range )
                .IgnoreGameObjectHierarchy( GameObject.Root )
                .Radius( 15f )
                .Run();

            if ( tr.Hit )
            {
                var isWall = tr.GameObject.Tags.Has( "world" ) || tr.GameObject.Tags.Has( "solid" );

                if ( isWall )
                {
                    DoWallHitEffects( tr.HitPosition, tr.Normal );
                    return; // Bloqueado por pared.
                }

                var damageable = tr.GameObject.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>();
                if ( damageable != null )
                {
                    var dmg = new DamageEvent
                    {
                        Amount = BaseDamage,
                        Position = tr.HitPosition,
                        Force = ray.Forward * PushForce,
                        AttackerId = Connection.Local?.Id.ToString() ?? "",
                        WeaponId = GameObject.Name
                    };

                    if ( Networking.IsHost )
                    {
                        damageable.TakeDamage( dmg );
                    }
                    else
                    {
                        RpcRequestMeleeDamage( tr.GameObject.Id, dmg.Amount, dmg.Position, dmg.Force, dmg.AttackerId, dmg.WeaponId );
                    }
                }

                DoHitEffects( tr.HitPosition, tr.Normal );
            }
            else
            {
                DoMissEffects();
            }
        }

        [Rpc.Host]
        private void RpcRequestMeleeDamage( Guid hitObjectId, float damage, Vector3 position, Vector3 force, string attackerId, string weaponId )
        {
            var hitObj = Scene.Directory.FindByGuid( hitObjectId );
            if ( hitObj != null )
            {
                var damageable = hitObj.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>();
                if ( damageable != null )
                {
                    var dmg = new DamageEvent
                    {
                        Amount = damage,
                        Position = position,
                        Force = force,
                        AttackerId = attackerId,
                        WeaponId = weaponId
                    };
                    damageable.TakeDamage( dmg );
                }
            }
        }

        [Rpc.Broadcast]
        protected override void DoHitEffects( Vector3 position, Vector3 normal )
        {
            base.DoHitEffects( position, normal );
            if ( HitSound != null )
                Sound.Play( HitSound, position );
        }

        [Rpc.Broadcast]
        protected void DoWallHitEffects( Vector3 position, Vector3 normal )
        {
            if ( WallHitSound != null )
                Sound.Play( WallHitSound, position );
        }

        [Rpc.Broadcast]
        protected void DoMissEffects()
        {
            if ( MissSound != null )
                Sound.Play( MissSound, WorldPosition );
        }
    }
}
