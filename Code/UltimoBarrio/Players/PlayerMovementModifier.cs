using Sandbox;
using UltimoBarrio.Combat;
using System;

namespace UltimoBarrio.Players
{
    [Title("Player Movement Modifier")]
    [Category("Último Barrio — Players")]
    public class PlayerMovementModifier : Component
    {
        public Sandbox.PlayerController Controller { get; set; }
        public HeldItemController HeldItems { get; set; }
        
        [Property] public MovementProfile Profile { get; set; }

        public float CurrentStamina { get; private set; }
        public bool IsExhausted => CurrentStamina <= 0f;
        public bool IsSprinting { get; private set; }

        public float CurrentWeight { get; set; } = 0f; // For inventory integration later

        private bool _wasOnGround = true;

        protected override void OnStart()
        {
            Controller = Components.GetInAncestorsOrSelf<Sandbox.PlayerController>();
            HeldItems = Components.GetInDescendantsOrSelf<HeldItemController>();

            if (Profile != null)
            {
                CurrentStamina = Profile.MaxStamina;
            }

            // Ensure PlayerCameraEffects is present on the Camera
            var cam = Components.GetInDescendantsOrSelf<CameraComponent>();
            if (cam != null)
            {
                var camFx = cam.Components.GetOrCreate<PlayerCameraEffects>();
                camFx.Profile = Profile;
                camFx.MovementModifier = this;
            }
        }

        protected override void OnUpdate()
        {
            if (Controller == null || Profile == null) return;
            
            if (!IsProxy)
            {
                var eyeAngles = Controller.EyeAngles;
                eyeAngles.pitch += Input.AnalogLook.pitch;
                eyeAngles.yaw += Input.AnalogLook.yaw;
                eyeAngles.pitch = eyeAngles.pitch.Clamp(-89f, 89f);
                Controller.EyeAngles = eyeAngles;

                var cam = Components.GetInDescendantsOrSelf<CameraComponent>();
                if (cam != null)
                {
                    cam.WorldRotation = Controller.EyeAngles.ToRotation();
                    cam.LocalPosition = Vector3.Up * 64f;
                }
            }

            // Landing detection for Camera Effects
            if (!_wasOnGround && Controller.IsOnGround)
            {
                var cam = Components.GetInDescendantsOrSelf<CameraComponent>();
                var camFx = cam?.Components.Get<PlayerCameraEffects>();
                if (camFx != null) camFx.ApplyLandingDip();
            }
            _wasOnGround = Controller.IsOnGround;

            // Sprinting & Stamina
            bool trySprint = Input.Down("run") && !IsExhausted && Controller.Velocity.Length > 10f && !Input.Down("duck") && Controller.IsOnGround;
            
            if (trySprint)
            {
                IsSprinting = true;
                CurrentStamina -= Profile.StaminaDrainRate * Time.Delta;
                CurrentStamina = System.Math.Clamp(CurrentStamina, 0f, Profile.MaxStamina);
            }
            else
            {
                IsSprinting = false;
                CurrentStamina += Profile.StaminaRegenRate * Time.Delta;
                if (CurrentStamina > Profile.MaxStamina) CurrentStamina = Profile.MaxStamina;
            }

            // Jump Stamina logic (if we want to intercept, we'd need to check jump input or just deduct if we jumped)
            // But since PlayerController handles jump internally based on its own logic, we can just detect jump if we can
            // For now, if we detect an upward velocity burst while grounded was just lost, or intercept jump action.
            if (Input.Pressed("jump") && Controller.IsOnGround && !IsExhausted)
            {
                if (CurrentStamina >= Profile.JumpStaminaCost)
                {
                    CurrentStamina -= Profile.JumpStaminaCost;
                    // Actual jump is handled by PlayerController
                }
                else
                {
                    // Not enough stamina to jump - we might want to cancel the jump but PlayerController does it itself.
                    // We can temporarily set JumpSpeed to 0 if we don't want them to jump?
                    // Actually, modifying JumpSpeed dynamically:
                }
            }

            // Calculate Multipliers
            float weightRatio = MathX.Clamp(CurrentWeight / MathF.Max(1f, Profile.MaxWeight), 0f, 1f);
            float weightMult = 1f - (weightRatio * Profile.MaxWeightSpeedPenalty);
            
            float weaponMult = 1f;
            if (HeldItems != null)
            {
                if (HeldItems.CurrentSlot == HeldItemSlot.Primary) weaponMult = 0.95f;
                else if (HeldItems.CurrentSlot == HeldItemSlot.Melee) weaponMult = 1.05f;
            }

            float finalMult = weightMult * weaponMult;

            // Apply to Controller
            Controller.WalkSpeed = Profile.WalkSpeed * finalMult;
            Controller.RunSpeed = IsSprinting ? (Profile.RunSpeed * finalMult) : Controller.WalkSpeed;
            Controller.DuckedSpeed = Profile.DuckedSpeed * finalMult;

            Controller.AccelerationTime = Profile.AccelerationTime;
            Controller.DeaccelerationTime = Profile.DecelerationTime;
            
            // If exhausted, disable jumping or reduce speed
            // Since we can't easily disable jump if PlayerController handles it, we can just let stamina be a deterrent.
        }
    }
}
