using Sandbox;
using System;
using UltimoBarrio.Core;
using UltimoBarrio.UI;
using UltimoBarrio.Apartments;

namespace UltimoBarrio
{
    [Title("Player Interactor")]
    [Category("Último Barrio")]
    [Icon("pan_tool")]
    public sealed class PlayerInteractor : Component
    {
        [Property] public float InteractionRange { get; set; } = 150f;
        
        private PlayerHud _hud;

        protected override void OnStart()
        {
            _hud = Components.Get<PlayerHud>();
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            var rayPos = Transform.Position + Vector3.Up * 64f;
            var rayDir = Transform.Rotation.Forward;

            var pc = Components.Get<Sandbox.PlayerController>();
            if (pc != null)
            {
                rayDir = pc.EyeAngles.Forward;
            }
            else
            {
                var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
                if (cam != null)
                {
                    rayPos = cam.Transform.Position;
                    rayDir = cam.Transform.Rotation.Forward;
                }
            }

            var tr = Scene.Trace.Ray(rayPos, rayPos + rayDir * InteractionRange)
                .IgnoreGameObjectHierarchy(GameObject)
                .Run();

            if (Input.Pressed("Use"))
            {
                Log.Info($"[Interact] Player pressed E. Ray hit: {tr.Hit}, Object: {tr.GameObject?.Name ?? "none"} at {tr.EndPosition}");
                if (tr.GameObject != null)
                {
                    var allComps = string.Join(", ", tr.GameObject.Components.GetAll().Select(c => c.GetType().Name));
                    Log.Info($"[Interact] Components on hit object: {allComps}");
                }
            }

            if (tr.Hit && tr.GameObject != null)
            {
                var claimable = tr.GameObject.Components.Get<ApartmentClaimInteractable>();
                if (claimable != null)
                {
                    var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == claimable.ApartmentId);
                    if (apt != null)
                    {
                        if (apt.ClaimState == ApartmentClaimState.Unclaimed)
                        {
                            _hud?.ShowPrompt("Este piso está disponible", "Pulsa E para reclamarlo");
                        }
                        else
                        {
                            _hud?.ShowPrompt("Este piso ya tiene dueño", "");
                        }

                        if (Input.Pressed("Use"))
                        {
                            var pressable = claimable as Component.IPressable;
                            if (pressable.CanPress(new Component.IPressable.Event()))
                            {
                                pressable.Press(new Component.IPressable.Event());
                            }
                        }
                    }
                    return;
                }

                var stashInv = tr.GameObject.Components.Get<InventoryComponent>();
                if (stashInv != null)
                {
                    // For stash, we should only open if we are the owner or it's public.
                    // A simple check for now: find parent ApartmentComponent and check owner.
                    var apt = tr.GameObject.Components.GetInAncestorsOrSelf<ApartmentComponent>();
                    bool canOpen = apt == null || apt.OwnerId == Game.SteamId.ToString();
                    
                    if (canOpen)
                    {
                        _hud?.ShowPrompt("Pulsa E para abrir el alijo", "");
                        if (Input.Pressed("Use"))
                        {
                            _hud?.OpenStash(stashInv);
                        }
                    }
                    else
                    {
                        _hud?.ShowPrompt("No puedes abrir este alijo", "");
                    }
                    return;
                }
                
                var pickup = tr.GameObject.Components.Get<WorldItemPickup>();
                if (pickup != null)
                {
                    var req = new InteractionRequest { InteractorId = Network.OwnerId.ToString(), InteractorObject = GameObject };
                    _hud?.ShowPrompt(pickup.GetComponent<IInteractable>().GetInteractionPrompt(req), "Pulsa E");
                    if (Input.Pressed("Use"))
                    {
                        var interactable = pickup.GetComponent<IInteractable>();
                        if (interactable != null && interactable.CanInteract(req))
                        {
                            interactable.OnInteract(req);
                            _hud?.ShowMessage("Objeto recogido");
                        }
                    }
                    return;
                }
            }

            _hud?.HidePrompt();
        }
    }
}
