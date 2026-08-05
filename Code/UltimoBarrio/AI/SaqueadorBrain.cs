using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Cerebro del asaltante (Saqueador):
    /// Idle → Patrol (rutas) → Investigate (ruido/objetivo de raid) →
    /// Detect → Approach → Attack (melee) → Retreat → Idle.
    /// Por la noche se spawna desde SpawnZones o por RaidManager.
    /// </summary>
    public class SaqueadorBrain : AIBase
    {
        public enum SaqueadorState { Idle, Patrol, Investigate, Detect, Approach, Attack, Retreat }

        [Property, Sync( SyncFlags.FromHost )]
        public SaqueadorState CurrentState { get; private set; } = SaqueadorState.Idle;

        [Property, Sync( SyncFlags.FromHost )]
        public GameObject RaidTarget { get; set; }

        private TimeSince _timeInState;
        private PatrolRoute _route;
        private int _waypointIndex;
        private Vector3 _retreatPosition;

        protected override void OnStart()
        {
            base.OnStart();
            _route = Components.Get<PatrolRoute>( FindMode.EverythingInSelfAndDescendants );
            _retreatPosition = WorldPosition;
            ChangeState( SaqueadorState.Idle );
        }

        protected override void OnUpdate()
        {
            if ( IsDead || IsProxy )
                return;

            if ( Agent is null || !Agent.IsValid() )
            {
                // Sin NavMesh no se puede mover; el FSM se mantiene en Idle.
                return;
            }

            // Percepción.
            var targets = Scene.GetAllComponents<Sandbox.PlayerController>();
            GameObject nearestTarget = null;
            float nearestDistance = float.MaxValue;
            foreach ( var player in targets )
            {
                if ( !player.IsValid() )
                    continue;

                Perception.UpdateTarget( player.GameObject );
                if ( Perception.CurrentTarget is not null )
                {
                    var d = Vector3.DistanceBetween( WorldPosition, Perception.CurrentTarget.WorldPosition );
                    if ( d < nearestDistance )
                    {
                        nearestDistance = d;
                        nearestTarget = Perception.CurrentTarget;
                    }
                }
            }

            switch ( CurrentState )
            {
                case SaqueadorState.Idle:
                    if ( nearestTarget is not null )
                        ChangeState( SaqueadorState.Detect );
                    else if ( Perception.InvestigatePosition.HasValue )
                        ChangeState( SaqueadorState.Investigate );
                    else if ( _timeInState > 2f )
                        ChangeState( _route is not null ? SaqueadorState.Patrol : SaqueadorState.Idle );
                    break;

                case SaqueadorState.Patrol:
                    if ( nearestTarget is not null )
                    {
                        ChangeState( SaqueadorState.Detect );
                        break;
                    }

                    if ( Perception.InvestigatePosition.HasValue )
                    {
                        ChangeState( SaqueadorState.Investigate );
                        break;
                    }

                    if ( RaidTarget.IsValid() )
                    {
                        Agent.MoveTo( RaidTarget.WorldPosition );
                        if ( Vector3.DistanceBetween( WorldPosition, RaidTarget.WorldPosition ) < 150f )
                            ChangeState( SaqueadorState.Investigate );
                        break;
                    }

                    PatrolStep();
                    break;

                case SaqueadorState.Investigate:
                    var destination = Perception.InvestigatePosition ?? ( RaidTarget.IsValid() ? RaidTarget.WorldPosition : (Vector3?)null );
                    if ( destination.HasValue )
                    {
                        Agent.MoveTo( destination.Value );

                        // Si el objetivo del raid es una estructura (puerta/alijo),
                        // pasar a atacarla al llegar.
                        if ( RaidTarget.IsValid()
                            && Vector3.DistanceBetween( WorldPosition, RaidTarget.WorldPosition ) <= AttackRange
                            && RaidTarget.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>() is not null )
                        {
                            ChangeState( SaqueadorState.Attack );
                            break;
                        }

                        if ( Vector3.DistanceBetween( WorldPosition, destination.Value ) < 80f || _timeInState > 6f )
                            ChangeState( nearestTarget is not null ? SaqueadorState.Detect : SaqueadorState.Idle );
                    }
                    else
                    {
                        ChangeState( SaqueadorState.Idle );
                    }

                    if ( nearestTarget is not null )
                        ChangeState( SaqueadorState.Detect );
                    break;

                case SaqueadorState.Detect:
                    if ( _timeInState > 1f )
                        ChangeState( SaqueadorState.Approach );
                    break;

                case SaqueadorState.Approach:
                    if ( nearestTarget is not null )
                    {
                        Agent.MoveTo( nearestTarget.WorldPosition );

                        if ( Vector3.DistanceBetween( WorldPosition, nearestTarget.WorldPosition ) <= AttackRange )
                            ChangeState( SaqueadorState.Attack );
                    }
                    else if ( _timeInState > 5f )
                    {
                        ChangeState( Perception.InvestigatePosition.HasValue ? SaqueadorState.Investigate : SaqueadorState.Idle );
                    }
                    break;

                case SaqueadorState.Attack:
                    if ( nearestTarget is not null )
                    {
                        TryAttack( nearestTarget );

                        if ( Vector3.DistanceBetween( WorldPosition, nearestTarget.WorldPosition ) > AttackRange * 1.6f )
                            ChangeState( SaqueadorState.Approach );
                        else if ( _timeInState > 8f )
                            ChangeState( SaqueadorState.Retreat );
                    }
                    else if ( RaidTarget.IsValid()
                        && RaidTarget.Components.GetInAncestorsOrSelf<UltimoBarrio.Core.IDamageable>() is not null )
                    {
                        // Asalto a la estructura (puerta/alijo).
                        TryAttack( RaidTarget );

                        if ( Vector3.DistanceBetween( WorldPosition, RaidTarget.WorldPosition ) > AttackRange * 1.6f )
                            ChangeState( SaqueadorState.Approach );
                        else if ( _timeInState > 8f )
                            ChangeState( SaqueadorState.Retreat );
                    }
                    else
                    {
                        ChangeState( Perception.InvestigatePosition.HasValue ? SaqueadorState.Investigate : SaqueadorState.Idle );
                    }
                    break;

                case SaqueadorState.Retreat:
                    if ( Vector3.DistanceBetween( WorldPosition, _retreatPosition ) > 50f )
                        Agent.MoveTo( _retreatPosition );
                    else if ( _timeInState > 4f )
                        ChangeState( SaqueadorState.Idle );
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

                // Pequeña pausa en cada waypoint.
                _timeInState = 0f;
            }
        }

        public void ChangeState( SaqueadorState newState )
        {
            if ( CurrentState == newState )
                return;

            CurrentState = newState;
            _timeInState = 0f;
        }
    }
}
