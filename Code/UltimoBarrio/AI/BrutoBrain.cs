using Sandbox;
using System;
using System.Linq;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Cerebro del bruto: lento, mucha vida, prioriza estructuras (puertas y
    /// barricadas) sobre el jugador — no retrocede, machaca hasta destruir lo
    /// que le impide entrar. Idle → Patrol → Approach → Bash (estructura) /
    /// Attack (jugador si le bloquea el paso o le ataca primero).
    /// </summary>
    public class BrutoBrain : AIBase
    {
        public enum BrutoState { Idle, Patrol, Approach, Bash, Attack }

        [Property, Sync( SyncFlags.FromHost )]
        public BrutoState CurrentState { get; private set; } = BrutoState.Idle;

        [Property, Sync( SyncFlags.FromHost )]
        public GameObject RaidTarget { get; set; }

        private TimeSince _timeInState;
        private PatrolRoute _route;
        private int _waypointIndex;

        protected override void OnStart()
        {
            base.OnStart();

            // Bruto: el doble de vida que un saqueador base, la mitad de rápido.
            MaxHealth = MaxHealth <= 0f ? 220f : MaxHealth;
            Health = MaxHealth;
            AttackDamage = MathF.Max( AttackDamage, 22f );

            if ( Agent is not null && Agent.IsValid() )
                Agent.MaxSpeed = MathF.Min( Agent.MaxSpeed > 0f ? Agent.MaxSpeed : 120f, 120f );

            _route = Components.Get<PatrolRoute>( FindMode.EverythingInSelfAndDescendants );
            ChangeState( BrutoState.Idle );
        }

        protected override void OnUpdate()
        {
            if ( IsDead || IsProxy )
                return;

            if ( Agent is null || !Agent.IsValid() )
                return;

            var targets = Scene.GetAllComponents<Sandbox.PlayerController>();
            GameObject nearestPlayer = null;
            float nearestDistance = float.MaxValue;
            foreach ( var player in targets )
            {
                if ( !player.IsValid() )
                    continue;

                Perception.UpdateTarget( player.GameObject );
                if ( Perception.CurrentTarget is null )
                    continue;

                var d = Vector3.DistanceBetween( WorldPosition, Perception.CurrentTarget.WorldPosition );
                if ( d < nearestDistance )
                {
                    nearestDistance = d;
                    nearestPlayer = Perception.CurrentTarget;
                }
            }

            bool raidTargetIsStructure = RaidTarget.IsValid()
                && RaidTarget.Components.GetInAncestorsOrSelf<Core.IDamageable>() is not null;

            switch ( CurrentState )
            {
                case BrutoState.Idle:
                    if ( raidTargetIsStructure )
                        ChangeState( BrutoState.Approach );
                    else if ( nearestPlayer is not null )
                        ChangeState( BrutoState.Approach );
                    else if ( _timeInState > 3f )
                        ChangeState( _route is not null ? BrutoState.Patrol : BrutoState.Idle );
                    break;

                case BrutoState.Patrol:
                    if ( raidTargetIsStructure || nearestPlayer is not null )
                    {
                        ChangeState( BrutoState.Approach );
                        break;
                    }

                    PatrolStep();
                    break;

                case BrutoState.Approach:
                    // Prioridad: la estructura del asalto (puerta/barricada) sobre
                    // el jugador — un bruto no se distrae, busca entrar.
                    if ( raidTargetIsStructure )
                    {
                        Agent.MoveTo( RaidTarget.WorldPosition );

                        // Si el jugador se pone en medio y le ataca, responde.
                        if ( nearestPlayer is not null && nearestDistance <= AttackRange )
                        {
                            ChangeState( BrutoState.Attack );
                            break;
                        }

                        if ( Vector3.DistanceBetween( WorldPosition, RaidTarget.WorldPosition ) <= AttackRange )
                            ChangeState( BrutoState.Bash );
                        break;
                    }

                    if ( nearestPlayer is not null )
                    {
                        Agent.MoveTo( nearestPlayer.WorldPosition );
                        if ( nearestDistance <= AttackRange )
                            ChangeState( BrutoState.Attack );
                    }
                    else if ( _timeInState > 6f )
                    {
                        ChangeState( BrutoState.Idle );
                    }
                    break;

                case BrutoState.Bash:
                    if ( !raidTargetIsStructure )
                    {
                        ChangeState( nearestPlayer is not null ? BrutoState.Approach : BrutoState.Idle );
                        break;
                    }

                    TryAttack( RaidTarget );

                    if ( nearestPlayer is not null && nearestDistance <= AttackRange )
                    {
                        ChangeState( BrutoState.Attack );
                        break;
                    }

                    if ( Vector3.DistanceBetween( WorldPosition, RaidTarget.WorldPosition ) > AttackRange * 1.5f )
                        ChangeState( BrutoState.Approach );
                    break;

                case BrutoState.Attack:
                    if ( nearestPlayer is not null )
                    {
                        Agent.MoveTo( nearestPlayer.WorldPosition );
                        TryAttack( nearestPlayer );

                        if ( nearestDistance > AttackRange * 1.6f )
                            ChangeState( raidTargetIsStructure ? BrutoState.Approach : BrutoState.Idle );
                    }
                    else
                    {
                        ChangeState( raidTargetIsStructure ? BrutoState.Approach : BrutoState.Idle );
                    }
                    break;
            }
        }

        private void PatrolStep()
        {
            if ( _route is null )
                return;

            var waypoints = _route.ResolveWaypoints();
            if ( waypoints.Count == 0 )
                return;

            if ( _waypointIndex >= waypoints.Count )
                _waypointIndex = 0;

            var waypoint = waypoints[_waypointIndex];
            if ( waypoint is null || !waypoint.IsValid() )
            {
                _waypointIndex++;
                return;
            }

            Agent.MoveTo( waypoint.WorldPosition );

            if ( Vector3.DistanceBetween( WorldPosition, waypoint.WorldPosition ) < 80f )
            {
                _waypointIndex++;
                if ( _waypointIndex >= waypoints.Count )
                    _waypointIndex = 0;
            }
        }

        public void ChangeState( BrutoState newState )
        {
            if ( CurrentState == newState )
                return;

            CurrentState = newState;
            _timeInState = 0f;
        }
    }
}
