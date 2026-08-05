using Sandbox;
using System;
using UltimoBarrio.Combat;

namespace UltimoBarrio.AI
{
    /// <summary>
    /// Percepción de un NPC: visión (FOV + línea de vista), oído (ruido de
    /// disparos vía WeaponNoise) y memoria de última posición conocida.
    /// El ruido mueve al NPC a investigar (posición de investigación).
    /// </summary>
    public class PerceptionComponent : Component
    {
        [Property] public float VisionRange { get; set; } = 2000f;
        [Property] public float FieldOfView { get; set; } = 90f;
        [Property] public float HearingRange { get; set; } = 1500f;
        [Property] public float MemoryDuration { get; set; } = 6f;

        public GameObject CurrentTarget { get; private set; }

        /// <summary>Última posición conocida del objetivo (memoria).</summary>
        public Vector3? LastKnownTargetPosition { get; private set; }

        /// <summary>Posición del último ruido o indicio a investigar.</summary>
        public Vector3? InvestigatePosition { get; private set; }

        public bool IsAlerted => CurrentTarget is not null || InvestigatePosition.HasValue;

        private TimeSince _timeSinceLastSeen;

        protected override void OnStart()
        {
            WeaponNoise.OnNoise += HandleWeaponNoise;
        }

        protected override void OnDestroy()
        {
            WeaponNoise.OnNoise -= HandleWeaponNoise;
        }

        protected override void OnUpdate()
        {
            if ( IsProxy )
                return;

            // Re-evaluar visión del objetivo actual.
            if ( CurrentTarget is not null && CurrentTarget.IsValid() )
                UpdateTarget( CurrentTarget );

            // Olvidar memoria agotada.
            if ( _timeSinceLastSeen > MemoryDuration )
            {
                LastKnownTargetPosition = null;
                InvestigatePosition = null;
            }
        }

        private void HandleWeaponNoise( Vector3 position, float radius )
        {
            if ( IsProxy )
                return;

            if ( Vector3.DistanceBetween( position, WorldPosition ) <= radius )
            {
                InvestigatePosition = position;
                _timeSinceLastSeen = 0f;
            }
        }

        public bool CanSee( GameObject target )
        {
            if ( target == null || !target.IsValid() )
                return false;

            var dirToTarget = ( target.WorldPosition - WorldPosition ).Normal;
            var distToTarget = Vector3.DistanceBetween( target.WorldPosition, WorldPosition );

            if ( distToTarget > VisionRange )
                return false;

            var angle = Vector3.GetAngle( WorldRotation.Forward, dirToTarget );
            if ( angle > FieldOfView * 0.5f )
                return false;

            var tr = Scene.Trace.Ray( WorldPosition + Vector3.Up * 50f, target.WorldPosition + Vector3.Up * 50f )
                .IgnoreGameObjectHierarchy( GameObject )
                .Run();

            return tr.Hit && tr.GameObject == target;
        }

        public void HearSound( Vector3 position, float volume )
        {
            if ( Vector3.DistanceBetween( position, WorldPosition ) <= HearingRange * volume )
            {
                InvestigatePosition = position;
                _timeSinceLastSeen = 0f;
            }
        }

        public void UpdateTarget( GameObject target )
        {
            if ( target is null || !target.IsValid() )
                return;

            if ( CanSee( target ) )
            {
                CurrentTarget = target;
                LastKnownTargetPosition = target.WorldPosition;
                _timeSinceLastSeen = 0f;
            }
            else
            {
                if ( CurrentTarget == target )
                    CurrentTarget = null;
            }
        }
    }
}
