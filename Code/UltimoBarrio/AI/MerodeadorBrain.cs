using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Apartments;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Cerebro del merodeador: detecta ruido, rodea el objetivo buscando una
    /// entrada abierta (puerta sin bloquear) en vez de asaltar de frente, y solo
    /// ataca si encuentra al jugador o agota el tiempo de búsqueda. Rápido para
    /// investigar, pero no tan directo como el saqueador — y sí retrocede si
    /// las cosas se ponen feas (a diferencia del bruto).
    /// Idle → Patrol → Investigate → Circle (busca puerta abierta) →
    /// Approach → Attack → Retreat → Idle.
    /// </summary>
    public class MerodeadorBrain : AIBase
    {
        public enum MerodeadorState { Idle, Patrol, Investigate, Circle, Approach, Attack, Retreat }

        [Property, Sync( SyncFlags.FromHost )]
        public MerodeadorState CurrentState { get; private set; } = MerodeadorState.Idle;

        [Property, Sync( SyncFlags.FromHost )]
        public GameObject RaidTarget { get; set; }

        /// <summary>Radio en el que busca puertas de apartamento sin bloquear.</summary>
        [Property] public float DoorSearchRadius { get; set; } = 600f;

        [Property] public float CircleTimeout { get; set; } = 6f;

        private TimeSince _timeInState;
        private PatrolRoute _route;
        private int _waypointIndex;
        private Vector3 _retreatPosition;
        private GameObject _foundOpenDoor;

        protected override void OnStart()
        {
            base.OnStart();
            _route = Components.Get<PatrolRoute>( FindMode.EverythingInSelfAndDescendants );
            _retreatPosition = WorldPosition;
            ChangeState( MerodeadorState.Idle );
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

            switch ( CurrentState )
            {
                case MerodeadorState.Idle:
                    if ( nearestPlayer is not null )
                        ChangeState( MerodeadorState.Approach );
                    else if ( Perception.InvestigatePosition.HasValue )
                        ChangeState( MerodeadorState.Investigate );
                    else if ( _timeInState > 2f )
                        ChangeState( _route is not null ? MerodeadorState.Patrol : MerodeadorState.Idle );
                    break;

                case MerodeadorState.Patrol:
                    if ( nearestPlayer is not null )
                    {
                        ChangeState( MerodeadorState.Approach );
                        break;
                    }

                    if ( Perception.InvestigatePosition.HasValue )
                    {
                        ChangeState( MerodeadorState.Investigate );
                        break;
                    }

                    PatrolStep();
                    break;

                case MerodeadorState.Investigate:
                    var dest = Perception.InvestigatePosition ?? ( RaidTarget.IsValid() ? RaidTarget.WorldPosition : (Vector3?)null );
                    if ( !dest.HasValue )
                    {
                        ChangeState( MerodeadorState.Idle );
                        break;
                    }

                    Agent.MoveTo( dest.Value );

                    if ( nearestPlayer is not null )
                    {
                        ChangeState( MerodeadorState.Approach );
                        break;
                    }

                    if ( Vector3.DistanceBetween( WorldPosition, dest.Value ) < 150f || _timeInState > 5f )
                        ChangeState( MerodeadorState.Circle );
                    break;

                case MerodeadorState.Circle:
                    // Busca una puerta de apartamento reclamado que no esté
                    // bloqueada — el rasgo distintivo del merodeador frente al
                    // saqueador (asalto directo) y al bruto (machaca la puerta).
                    if ( nearestPlayer is not null )
                    {
                        ChangeState( MerodeadorState.Approach );
                        break;
                    }

                    if ( _foundOpenDoor is null || !_foundOpenDoor.IsValid() )
                        _foundOpenDoor = FindNearestOpenDoor();

                    if ( _foundOpenDoor is not null && _foundOpenDoor.IsValid() )
                    {
                        Agent.MoveTo( _foundOpenDoor.WorldPosition );

                        if ( Vector3.DistanceBetween( WorldPosition, _foundOpenDoor.WorldPosition ) < 80f )
                        {
                            // Entrada encontrada: por ahora se limita a investigar
                            // dentro — atacar al jugador si lo encuentra allí.
                            ChangeState( MerodeadorState.Investigate );
                        }
                        break;
                    }

                    if ( _timeInState > CircleTimeout )
                    {
                        // No hay puerta abierta a la vista: se retira, no fuerza la entrada.
                        ChangeState( MerodeadorState.Retreat );
                    }
                    break;

                case MerodeadorState.Approach:
                    if ( nearestPlayer is not null )
                    {
                        Agent.MoveTo( nearestPlayer.WorldPosition );
                        if ( nearestDistance <= AttackRange )
                            ChangeState( MerodeadorState.Attack );
                    }
                    else if ( _timeInState > 5f )
                    {
                        ChangeState( Perception.InvestigatePosition.HasValue ? MerodeadorState.Investigate : MerodeadorState.Idle );
                    }
                    break;

                case MerodeadorState.Attack:
                    if ( nearestPlayer is not null )
                    {
                        TryAttack( nearestPlayer );

                        if ( nearestDistance > AttackRange * 1.6f )
                            ChangeState( MerodeadorState.Approach );
                        else if ( _timeInState > 5f )
                            ChangeState( MerodeadorState.Retreat );
                    }
                    else
                    {
                        ChangeState( MerodeadorState.Idle );
                    }
                    break;

                case MerodeadorState.Retreat:
                    _foundOpenDoor = null;

                    if ( Vector3.DistanceBetween( WorldPosition, _retreatPosition ) > 50f )
                        Agent.MoveTo( _retreatPosition );
                    else if ( _timeInState > 3f )
                        ChangeState( MerodeadorState.Idle );
                    break;
            }
        }

        /// <summary>
        /// Busca la puerta de apartamento reclamado más cercana cuyo
        /// ApartmentDoorPolicy no esté bloqueando (IsLocked=false), dentro de
        /// DoorSearchRadius.
        /// </summary>
        private GameObject FindNearestOpenDoor()
        {
            return Scene.GetAllComponents<ApartmentDoorPolicy>()
                .Where( d => !d.IsLocked && Vector3.DistanceBetween( d.WorldPosition, WorldPosition ) <= DoorSearchRadius )
                .OrderBy( d => Vector3.DistanceBetween( d.WorldPosition, WorldPosition ) )
                .Select( d => d.GameObject )
                .FirstOrDefault();
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

        public void ChangeState( MerodeadorState newState )
        {
            if ( CurrentState == newState )
                return;

            CurrentState = newState;
            _timeInState = 0f;
        }
    }
}
