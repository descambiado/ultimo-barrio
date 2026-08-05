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

        /// <summary>
        /// Asienta al jugador sobre el suelo más cercano bajo sus pies. El origen de
        /// Sandbox.PlayerController está en los pies, así que trazamos desde poco por
        /// encima (no desde 500 unidades: eso capturaba techos y azoteas) hacia abajo.
        /// Devuelve false si todavía no hay geometría — MapInstance carga en diferido.
        /// </summary>
        private bool TryGroundPlayer( Vector3 pos )
        {
            var tr = Scene.Trace.Ray( pos + Vector3.Up * 64f, pos + Vector3.Down * 2048f )
                .IgnoreGameObjectHierarchy( GameObject )
                .Run();

            if ( !tr.Hit ) return false;

            GameObject.WorldPosition = tr.EndPosition + Vector3.Up * 4f;
            return true;
        }

        private void ValidateSpawn()
        {
            // Sólo el host es autoritativo sobre la posición; los proxies se limitan
            // a replicarla, así que no tienen nada que validar.
            if ( !Networking.IsHost )
            {
                _spawnValidated = true;
                return;
            }

            if ( TryGroundPlayer( GameObject.WorldPosition ) )
            {
                _spawnValidated = true;
                Log.Info( $"Player grounded at {GameObject.WorldPosition}" );
                return;
            }

            foreach ( var p in Scene.GetAllComponents<SpawnPoint>() )
            {
                if ( !TryGroundPlayer( p.GameObject.WorldPosition ) ) continue;

                _spawnValidated = true;
                Log.Info( $"Fallback spawn selected: {GameObject.WorldPosition}" );
                return;
            }

            // Sin suelo todavía (mapa aún cargando). No inventamos una posición:
            // OnUpdate reintentará mientras el jugador no haya caído del mundo.
            Log.Warning( "No ground found yet for player spawn; will retry." );
        }

        public float CurrentWeight { get; set; } = 0f; // For inventory integration later

        private bool _spawnValidated;
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
        }

        protected override void OnUpdate()
        {
            // Este componente NO toca cámara ni orientación: el look, el pitch y el
            // conmutador primera/tercera persona son propiedad de Sandbox.PlayerController
            // (UseLookControls / UseCameraControls / ToggleCameraModeButton), que actúa
            // como ICameraModifier sobre la cámara principal de la escena.
            if ( !_spawnValidated || GameObject.WorldPosition.z < -2000f )
            {
                ValidateSpawn();
                if ( !_spawnValidated ) return;
            }

            if (Controller == null || Profile == null) return;

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
