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
        
        [Property] public float BaseWalkSpeed { get; set; } = 110f;
        [Property] public float BaseRunSpeed { get; set; } = 320f;
        [Property] public float BaseDuckedSpeed { get; set; } = 70f;
        [Property] public float MaxStamina { get; set; } = 100f;
        [Property] public float StaminaDrain { get; set; } = 20f;
        [Property] public float StaminaRegen { get; set; } = 15f;

        public float CurrentStamina { get; private set; }
        public bool IsExhausted => CurrentStamina <= 0f;

        protected override void OnStart()
        {
            Controller = Components.GetInAncestorsOrSelf<Sandbox.PlayerController>();
            HeldItems = Components.GetInDescendantsOrSelf<HeldItemController>();
            CurrentStamina = MaxStamina;

            if (Controller != null)
            {
                Controller.AccelerationTime = 0.15f;
                Controller.DeaccelerationTime = 0.1f;
            }
        }

        protected override void OnUpdate()
        {
            if (Controller == null) return;
            
            bool isSprinting = Input.Down("run") && !IsExhausted && Controller.Velocity.Length > 10f;
            if (isSprinting)
            {
                CurrentStamina -= StaminaDrain * Time.Delta;
                if (CurrentStamina < 0f) CurrentStamina = 0f;
            }
            else
            {
                CurrentStamina += StaminaRegen * Time.Delta;
                if (CurrentStamina > MaxStamina) CurrentStamina = MaxStamina;
            }

            float weaponMult = 1f;
            if (HeldItems != null)
            {
                if (HeldItems.CurrentType == HeldItemType.Pistol) weaponMult = 0.9f;
                else if (HeldItems.CurrentType == HeldItemType.Melee) weaponMult = 1.05f;
            }

            Controller.WalkSpeed = BaseWalkSpeed * weaponMult;
            Controller.RunSpeed = isSprinting ? (BaseRunSpeed * weaponMult) : Controller.WalkSpeed;
            Controller.DuckedSpeed = BaseDuckedSpeed * weaponMult;

            // Simple Lean/Sway would be on the camera, so we skip complex math here for brevity, 
            // but the movement dynamics are now handled with stamina.
        }
    }
}
