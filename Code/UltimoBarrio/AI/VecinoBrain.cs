using Sandbox;
using System;

namespace UltimoBarrio.AI
{
    public class VecinoBrain : AIBase
    {
        public enum VecinoState { Idle, Patrol, Investigate, Flee, ReturnHome }
        
        [Property, Sync(SyncFlags.FromHost)] public VecinoState CurrentState { get; private set; } = VecinoState.Idle;
        [Property, Sync(SyncFlags.FromHost)] public GameObject Home { get; set; }
        
        private TimeSince timeInState;
        
        protected override void OnUpdate()
        {
            if (IsDead) return;
            if (IsProxy) return;
            
            switch (CurrentState)
            {
                case VecinoState.Idle:
                    if (timeInState > 3f) ChangeState(VecinoState.Patrol);
                    break;
                case VecinoState.Patrol:
                    if (Agent != null && Agent.Velocity.Length < 1f && timeInState > 1f) ChangeState(VecinoState.Idle);
                    break;
                case VecinoState.Investigate:
                    if (timeInState > 5f) ChangeState(VecinoState.ReturnHome);
                    break;
                case VecinoState.Flee:
                    if (timeInState > 10f) ChangeState(VecinoState.ReturnHome);
                    break;
                case VecinoState.ReturnHome:
                    if (Home != null && Vector3.DistanceBetween(WorldPosition, Home.WorldPosition) < 50f)
                        ChangeState(VecinoState.Idle);
                    break;
            }
        }
        
        public void ChangeState(VecinoState newState)
        {
            CurrentState = newState;
            timeInState = 0f;
        }
    }
}
