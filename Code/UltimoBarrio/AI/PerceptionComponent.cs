using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.AI
{
    public class PerceptionComponent : Component
    {
        [Property] public float VisionRange { get; set; } = 2000f;
        [Property] public float FieldOfView { get; set; } = 90f;
        [Property] public float HearingRange { get; set; } = 1500f;
        [Property] public float MemoryDuration { get; set; } = 5f;

        public Vector3? LastKnownTargetPosition { get; private set; }
        private TimeSince timeSinceLastSeenTarget;

        public GameObject CurrentTarget { get; private set; }

        public bool CanSee(GameObject target)
        {
            if (target == null) return false;
            
            var dirToTarget = (target.WorldPosition - WorldPosition).Normal;
            var distToTarget = Vector3.DistanceBetween(target.WorldPosition, WorldPosition);

            if (distToTarget > VisionRange) return false;

            var angle = Vector3.GetAngle(WorldRotation.Forward, dirToTarget);
            if (angle > FieldOfView * 0.5f) return false;

            var tr = Scene.Trace.Ray(WorldPosition + Vector3.Up * 50f, target.WorldPosition + Vector3.Up * 50f)
                .IgnoreGameObjectHierarchy(GameObject)
                .Run();

            return tr.Hit && tr.GameObject == target;
        }

        public void HearSound(Vector3 position, float volume)
        {
            if (Vector3.DistanceBetween(position, WorldPosition) <= HearingRange * volume)
            {
                UpdateMemory(position);
            }
        }

        public void UpdateTarget(GameObject target)
        {
            if (CanSee(target))
            {
                CurrentTarget = target;
                UpdateMemory(target.WorldPosition);
            }
            else
            {
                if (CurrentTarget == target)
                {
                    CurrentTarget = null;
                }
            }

            if (timeSinceLastSeenTarget > MemoryDuration)
            {
                LastKnownTargetPosition = null;
            }
        }

        private void UpdateMemory(Vector3 pos)
        {
            LastKnownTargetPosition = pos;
            timeSinceLastSeenTarget = 0f;
        }
    }
}
