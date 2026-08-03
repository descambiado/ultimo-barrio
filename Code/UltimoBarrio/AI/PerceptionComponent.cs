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
            
            var dirToTarget = (target.Transform.Position - Transform.Position).Normal;
            var distToTarget = Vector3.DistanceBetween(target.Transform.Position, Transform.Position);

            if (distToTarget > VisionRange) return false;

            var angle = Vector3.GetAngle(Transform.Rotation.Forward, dirToTarget);
            if (angle > FieldOfView * 0.5f) return false;

            var tr = Scene.Trace.Ray(Transform.Position + Vector3.Up * 50f, target.Transform.Position + Vector3.Up * 50f)
                .IgnoreGameObjectHierarchy(GameObject)
                .Run();

            return tr.Hit && tr.GameObject == target;
        }

        public void HearSound(Vector3 position, float volume)
        {
            if (Vector3.DistanceBetween(position, Transform.Position) <= HearingRange * volume)
            {
                UpdateMemory(position);
            }
        }

        public void UpdateTarget(GameObject target)
        {
            if (CanSee(target))
            {
                CurrentTarget = target;
                UpdateMemory(target.Transform.Position);
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
