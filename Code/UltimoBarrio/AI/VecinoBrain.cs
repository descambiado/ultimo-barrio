using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Cerebro del vecino (civil): patrulla tranquilo, huye del ruido de
    /// disparos y vuelve a casa. No ataca.
    /// </summary>
    public class VecinoBrain : AIBase
    {
        public enum VecinoState { Idle, Patrol, Investigate, Flee, ReturnHome }

        [Property, Sync( SyncFlags.FromHost )]
        public VecinoState CurrentState { get; private set; } = VecinoState.Idle;

        [Property, Sync( SyncFlags.FromHost )]
        public GameObject Home { get; set; }

        private TimeSince _timeInState;
        private PatrolRoute _route;
        private int _waypointIndex;
        private Vector3 _fleeDirection;

        protected override void OnStart()
        {
            base.OnStart();
            AttackDamage = 0f; // No ataca.
            _route = Components.Get<PatrolRoute>( FindMode.EverythingInSelfAndDescendants );
            ChangeState( VecinoState.Idle );
        }

        protected override void OnUpdate()
        {
            if ( IsDead || IsProxy )
                return;

            if ( Agent is null || !Agent.IsValid() )
                return;

            switch ( CurrentState )
            {
                case VecinoState.Idle:
                    if ( _timeInState > 3f )
                        ChangeState( _route is not null ? VecinoState.Patrol : VecinoState.Idle );

                    if ( Perception.InvestigatePosition.HasValue )
                        ChangeState( VecinoState.Flee );
                    break;

                case VecinoState.Patrol:
                    if ( Perception.InvestigatePosition.HasValue )
                    {
                        ChangeState( VecinoState.Flee );
                        break;
                    }

                    PatrolStep();
                    break;

                case VecinoState.Investigate:
                    if ( Perception.InvestigatePosition.HasValue )
                    {
                        Agent.MoveTo( Perception.InvestigatePosition.Value );
                        if ( Vector3.DistanceBetween( WorldPosition, Perception.InvestigatePosition.Value ) < 80f || _timeInState > 4f )
                            ChangeState( VecinoState.ReturnHome );
                    }
                    else
                    {
                        ChangeState( VecinoState.ReturnHome );
                    }
                    break;

                case VecinoState.Flee:
                    _fleeDirection = Perception.InvestigatePosition.HasValue
                        ? ( WorldPosition - Perception.InvestigatePosition.Value ).Normal
                        : _fleeDirection;

                    Agent.MoveTo( WorldPosition + _fleeDirection * 300f );

                    if ( _timeInState > 8f || !Perception.InvestigatePosition.HasValue )
                        ChangeState( VecinoState.ReturnHome );
                    break;

                case VecinoState.ReturnHome:
                    if ( Home.IsValid() )
                    {
                        Agent.MoveTo( Home.WorldPosition );
                        if ( Vector3.DistanceBetween( WorldPosition, Home.WorldPosition ) < 60f )
                            ChangeState( VecinoState.Idle );
                    }
                    else
                    {
                        ChangeState( VecinoState.Idle );
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

        public void ChangeState( VecinoState newState )
        {
            if ( CurrentState == newState )
                return;

            CurrentState = newState;
            _timeInState = 0f;
        }
    }
}
