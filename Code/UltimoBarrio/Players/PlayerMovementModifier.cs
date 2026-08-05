using Sandbox;
using UltimoBarrio.Combat;

namespace UltimoBarrio.Players
{
    [Title("Player Movement Modifier")]
    [Category("Último Barrio — Players")]
    public class PlayerMovementModifier : Component
    {
        // Require components to be on the same GameObject or find them
        public Sandbox.PlayerController Controller { get; set; }
        public HeldItemController HeldItems { get; set; }
        
        private float _defaultSpeed = 0f;

        protected override void OnStart()
        {
            Controller = Components.GetInAncestorsOrSelf<Sandbox.PlayerController>();
            HeldItems = Components.GetInDescendantsOrSelf<HeldItemController>();

            if (Controller != null)
                _defaultSpeed = Controller.WalkSpeed;
        }

        protected override void OnUpdate()
        {
            if (Controller == null || HeldItems == null) return;
            if (_defaultSpeed == 0f) _defaultSpeed = Controller.WalkSpeed;
            
            if (HeldItems.CurrentType == HeldItemType.Pistol)
            {
                Controller.WalkSpeed = _defaultSpeed * 0.85f; 
            }
            else if (HeldItems.CurrentType == HeldItemType.Melee)
            {
                Controller.WalkSpeed = _defaultSpeed * 1.05f;
            }
            else
            {
                Controller.WalkSpeed = _defaultSpeed;
            }
        }
    }
}
