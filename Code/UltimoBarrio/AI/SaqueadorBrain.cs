using Sandbox;
using System;

namespace UltimoBarrio.AI
{
    public class SaqueadorBrain : AIBase
    {
        public enum SaqueadorState { Idle, Patrol, Investigate, Detect, Approach, Attack, Retreat }
        
        [Property, Sync(SyncFlags.FromHost)] public SaqueadorState CurrentState { get; private set; } = SaqueadorState.Idle;
        
        [Property, Sync(SyncFlags.FromHost)] public GameObject RaidTarget { get; set; }
        
        private TimeSince timeInState;
        
        protected override void OnStart()
        {
            base.OnStart();
            ChangeState(SaqueadorState.Idle);
        }

        protected override void OnUpdate()
        {
            if (IsDead) return;
            
            if (IsProxy) return;

            switch (CurrentState)
            {
                case SaqueadorState.Idle:
                    if (timeInState > 2f) ChangeState(SaqueadorState.Patrol);
                    break;
                case SaqueadorState.Patrol:
                    if (Perception.CurrentTarget != null) ChangeState(SaqueadorState.Detect);
                    else if (RaidTarget != null && timeInState > 2f) ChangeState(SaqueadorState.Investigate);
                    break;
                case SaqueadorState.Investigate:
                    if (RaidTarget != null)
                    {
                        if (Agent != null)
                        {
                            Agent.MoveTo(RaidTarget.Transform.Position);
                        }
                        if (Vector3.DistanceBetween(Transform.Position, RaidTarget.Transform.Position) < 150f)
                        {
                            ChangeState(SaqueadorState.Attack);
                        }
                    }
                    if (Perception.CurrentTarget != null) ChangeState(SaqueadorState.Detect);
                    break;
                case SaqueadorState.Detect:
                    if (timeInState > 1f) ChangeState(SaqueadorState.Approach);
                    break;
                case SaqueadorState.Approach:
                    if (Perception.CurrentTarget != null)
                    {
                        if (Agent != null)
                        {
                            Agent.MoveTo(Perception.CurrentTarget.Transform.Position);
                        }
                        if (Vector3.DistanceBetween(Transform.Position, Perception.CurrentTarget.Transform.Position) < 100f)
                        {
                            ChangeState(SaqueadorState.Attack);
                        }
                    }
                    else if (timeInState > 5f)
                    {
                        ChangeState(SaqueadorState.Investigate);
                    }
                    break;
                case SaqueadorState.Attack:
                    if (timeInState > 3f) ChangeState(SaqueadorState.Retreat);
                    break;
                case SaqueadorState.Retreat:
                    if (timeInState > 5f) ChangeState(SaqueadorState.Idle);
                    break;
            }
        }
        
        public void ChangeState(SaqueadorState newState)
        {
            CurrentState = newState;
            timeInState = 0f;
        }

        protected override void Die()
        {
            base.Die();
        }
    }
}
