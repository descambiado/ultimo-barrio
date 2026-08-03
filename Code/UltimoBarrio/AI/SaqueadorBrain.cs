using Sandbox;
using System;

namespace UltimoBarrio.AI
{
    public class SaqueadorBrain : AIBase
    {
        public enum SaqueadorState { Idle, Patrol, Detect, Approach, Attack, Retreat }
        
        [Property] public SaqueadorState CurrentState { get; private set; } = SaqueadorState.Idle;
        
        private TimeSince timeInState;
        
        protected override void OnUpdate()
        {
            if (IsDead) return;
            
            switch (CurrentState)
            {
                case SaqueadorState.Idle:
                    if (timeInState > 2f) ChangeState(SaqueadorState.Patrol);
                    break;
                case SaqueadorState.Patrol:
                    if (Perception.CurrentTarget != null) ChangeState(SaqueadorState.Detect);
                    break;
                case SaqueadorState.Detect:
                    if (timeInState > 1f) ChangeState(SaqueadorState.Approach);
                    break;
                case SaqueadorState.Approach:
                    if (Perception.CurrentTarget != null && Vector3.DistanceBetween(Transform.Position, Perception.CurrentTarget.Transform.Position) < 100f)
                        ChangeState(SaqueadorState.Attack);
                    break;
                case SaqueadorState.Attack:
                    if (timeInState > 1f) ChangeState(SaqueadorState.Retreat);
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
    }
}
