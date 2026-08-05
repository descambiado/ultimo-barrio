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

        public void ConsumeStamina( float amount )
        {
            if ( amount <= 0 || Profile == null ) return;
            CurrentStamina = System.Math.Clamp( CurrentStamina - amount, 0f, Profile.MaxStamina );
        }

        private void ValidateSpawn()
        {
            if ( !Networking.IsHost ) return;
            
            var pos = GameObject.WorldPosition;
            // Trace desde muy alto para no quedarse atrapado bajo el mapa
            var tr = Scene.Trace.Ray( pos.WithZ(pos.z + 500f), pos.WithZ(pos.z - 2000f) )
                .IgnoreGameObjectHierarchy( GameObject )
                .Run();

            if ( tr.Hit )
            {
                // Mueve el origen al suelo + 5 unidades. Si el collider está centrado, podría necesitar más altura.
                // S&box PlayerController suele tener el origen abajo, pero si cae, lo subimos 10f.
                GameObject.WorldPosition = tr.EndPosition + Vector3.Up * 10f;
                Log.Info($"Player spawned at valid floor: {GameObject.WorldPosition}");
            }
            else
            {
                Log.Warning($"Invalid spawn position {pos}. Searching for fallback SpawnPoint...");
                var points = Scene.GetAllComponents<SpawnPoint>();
                foreach (var p in points)
                {
                    var ptr = Scene.Trace.Ray( p.GameObject.WorldPosition.WithZ(p.GameObject.WorldPosition.z + 500f), p.GameObject.WorldPosition.WithZ(p.GameObject.WorldPosition.z - 2000f) ).Run();
                    if ( ptr.Hit )
                    {
                        GameObject.WorldPosition = ptr.EndPosition + Vector3.Up * 10f;
                        Log.Info($"Fallback spawn selected: {GameObject.WorldPosition}");
                        return;
                    }
                }
                
                // Absolute fallback to a high point if map is missing entirely
                GameObject.WorldPosition = new Vector3(0, 0, 1000f);
            }
        }

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

            ValidateSpawn();

            // Fix Camera Hierarchy if Camera is on the root object
            var rootCam = Components.Get<CameraComponent>();
            if (rootCam != null)
            {
                var camGo = new GameObject(true, "HeadCamera");
                camGo.SetParent(this.GameObject);
                camGo.LocalPosition = Vector3.Up * 64f;
                var newCam = camGo.Components.Create<CameraComponent>();
                newCam.FieldOfView = rootCam.FieldOfView;
                rootCam.Destroy();
            }

            // Desactivamos PlayerCameraEffects temporalmente porque interfiere con 
            // el sistema nativo de tercera persona de Sandbox.PlayerController.
        }

        protected override void OnUpdate()
        {
            if ( GameObject.WorldPosition.z < -2000f )
            {
                Log.Warning("Player fell below KillZ! Recovering...");
                ValidateSpawn();
                return;
            }

            if (Controller == null || Profile == null) return;
            
            if (!IsProxy)
            {
                var eyeAngles = Controller.EyeAngles;
                eyeAngles.pitch += Input.AnalogLook.pitch;
                eyeAngles.yaw += Input.AnalogLook.yaw;
                eyeAngles.pitch = eyeAngles.pitch.Clamp(-89f, 89f);
                Controller.EyeAngles = eyeAngles;
                
                // Ya no forzamos cam.LocalPosition = Vector3.Up * 64f;
                // Dejamos que Sandbox.PlayerController mueva el objeto de la cámara.
            }

            // Landing detection
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
