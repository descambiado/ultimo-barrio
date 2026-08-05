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
        private float _defaultJumpSpeed = 300f;

        protected override void OnStart()
        {
            Controller = Components.GetInAncestorsOrSelf<Sandbox.PlayerController>();
            HeldItems = Components.GetInDescendantsOrSelf<HeldItemController>();

            if ( Controller is not null )
                _defaultJumpSpeed = Controller.JumpSpeed;

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

            CurrentStamina = StaminaMath.Step(
                CurrentStamina,
                Profile.MaxStamina,
                Profile.StaminaDrainRate,
                Profile.StaminaRegenRate,
                sprinting: trySprint,
                Time.Delta );

            IsSprinting = trySprint && CurrentStamina > 0f;

            // Salto con coste de stamina; agotado = sin salto.
            if (Input.Pressed("jump") && Controller.IsOnGround && !IsExhausted)
            {
                if (CurrentStamina >= Profile.JumpStaminaCost)
                {
                    CurrentStamina = System.Math.Clamp(CurrentStamina - Profile.JumpStaminaCost, 0f, Profile.MaxStamina);
                }
            }

            // Agotamiento: bloquea el salto y el sprint (el sprint ya se bloquea
            // arriba por IsExhausted; aquí cortamos el salto del PlayerController).
            Controller.JumpSpeed = IsExhausted ? 0f : _defaultJumpSpeed;

            // Peso real del inventario (única fuente: InventoryComponent).
            var inventory = Components.Get<InventoryComponent>( FindMode.EverythingInSelfAndDescendants );
            CurrentWeight = inventory?.GetTotalWeight() ?? 0f;

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
        }
    }
}
