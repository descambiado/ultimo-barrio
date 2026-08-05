using Sandbox;
using System;

namespace UltimoBarrio.Players
{
    [Title("Player Camera Effects")]
    [Category("Último Barrio — Players")]
    public class PlayerCameraEffects : Component
    {
        public MovementProfile Profile { get; set; }
        public PlayerMovementModifier MovementModifier { get; set; }

        private float _bobTimer;
        private Vector3 _startLocalPosition;
        private Rotation _startLocalRotation;
        
        private float _currentLean;
        private float _targetLean;
        
        private float _currentDip;

        protected override void OnStart()
        {
            _startLocalPosition = LocalPosition;
            _startLocalRotation = LocalRotation;
        }

        protected override void OnUpdate()
        {
            if (Profile == null || MovementModifier == null) return;
            if (MovementModifier.Controller == null) return;

            var controller = MovementModifier.Controller;
            var velocity = controller.Velocity.WithZ(0);
            float speed = velocity.Length;
            bool isGrounded = controller.IsOnGround;

            // --- Bobbing ---
            float bobSpeed = Profile.BobSpeedWalk;
            float bobIntensity = Profile.BobIntensityWalk;

            if (Input.Down("duck"))
            {
                bobSpeed = Profile.BobSpeedDucked;
                bobIntensity = Profile.BobIntensityDucked;
            }
            else if (MovementModifier.IsSprinting)
            {
                bobSpeed = Profile.BobSpeedRun;
                bobIntensity = Profile.BobIntensityRun;
            }

            if (isGrounded && speed > 5f)
            {
                _bobTimer += Time.Delta * bobSpeed * (speed / Profile.WalkSpeed);
            }
            else
            {
                _bobTimer = MathX.Lerp(_bobTimer, 0, Time.Delta * 5f);
            }

            // In s&box, Z is up/down, Y is left/right
            float bobOffsetZ = MathF.Sin(_bobTimer) * bobIntensity;
            float bobOffsetY = MathF.Cos(_bobTimer * 0.5f) * bobIntensity * 0.5f;

            // --- Leaning ---
            _targetLean = 0f;
            if (Input.Down("lean_left")) _targetLean = Profile.LeanAngle; // Roll positive for left lean? Actually roll is X axis. 
            if (Input.Down("lean_right")) _targetLean = -Profile.LeanAngle;
            
            _currentLean = MathX.Lerp(_currentLean, _targetLean, Time.Delta * Profile.LeanSpeed);
            
            float leanRatio = Profile.LeanAngle > 0 ? _currentLean / Profile.LeanAngle : 0f;
            float leanOffsetY = leanRatio * Profile.LeanOffset;

            // --- Sway ---
            var lookInput = Input.AnalogLook;
            float swayX = -lookInput.yaw * Profile.WeaponSwayAmount;
            float swayY = lookInput.pitch * Profile.WeaponSwayAmount;
            
            swayX = MathX.Clamp(swayX, -5f, 5f);
            swayY = MathX.Clamp(swayY, -5f, 5f);

            var swayRot = Rotation.From(swayY, swayX, 0);

            // --- Landing Dip ---
            _currentDip = MathX.Lerp(_currentDip, 0, Time.Delta * Profile.LandingRecoverSpeed);

            // Apply transforms
            var targetPos = _startLocalPosition + new Vector3(0, bobOffsetY + leanOffsetY, bobOffsetZ - _currentDip);
            var targetRot = _startLocalRotation * Rotation.From(0, 0, _currentLean) * swayRot;

            LocalPosition = Vector3.Lerp(LocalPosition, targetPos, Time.Delta * 10f);
            LocalRotation = Rotation.Slerp(LocalRotation, targetRot, Time.Delta * 10f);
        }

        public void ApplyLandingDip()
        {
            if (Profile != null)
            {
                _currentDip = Profile.LandingCameraDip;
            }
        }
    }
}
