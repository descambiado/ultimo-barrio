using Sandbox;
using System;
using System.Linq;
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
            
            // Set deterministic inventory ID
            var inv = Components.Get<InventoryComponent>();
            if (inv != null && GameObject.Network.OwnerId != Guid.Empty)
            {
                var identityProvider = Scene.GetAllComponents<IPlayerIdentityProvider>().FirstOrDefault();
                if (identityProvider != null)
                {
                    var connection = Connection.All.FirstOrDefault(c => c.Id == GameObject.Network.OwnerId);
                    if (connection != null && identityProvider.TryResolve(connection, out var ownerId))
                    {
                        inv.InventoryId = $"player:{ownerId}:inventory";
                    }
                    else
                    {
                        inv.InventoryId = $"player:{GameObject.Network.OwnerId}:inventory";
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;
            
            // Only interact if in Gameplay state
            if (_hud != null && _hud.CurrentState != HudState.Gameplay)
            {
                _hud?.HidePrompt();
                return;
            }

            var rayPos = WorldPosition + Vector3.Up * 64f;
            var rayDir = WorldRotation.Forward;

            var pc = Components.Get<Sandbox.PlayerController>();
            if (pc != null) rayDir = pc.EyeAngles.Forward;
            else
            {
                var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
                if (cam != null)
                {
                    rayPos = cam.WorldPosition;
                    rayDir = cam.WorldRotation.Forward;
                }
            }

            var tr = Scene.Trace.Ray(rayPos, rayPos + rayDir * InteractionRange)
                .IgnoreGameObjectHierarchy(GameObject)
                .Run();

            if (tr.Hit && tr.GameObject != null)
            {
                float dist = (tr.EndPosition - rayPos).Length;

                if (Input.Pressed("Use"))
                {
                    Log.Info($"[Interact] Target: {tr.GameObject.Name}, Distance: {dist}");
                }

                // 1. Apartment
                var claimable = tr.GameObject.Components.Get<ApartmentClaimInteractable>();
                if (claimable != null)
                {
                    var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == claimable.ApartmentId);
                    if (apt != null)
                    {
                        string prompt = apt.ClaimState == ApartmentClaimState.Unclaimed ? "Este piso está disponible" : "Este piso ya tiene dueño";
                        _hud?.ShowPrompt(prompt, apt.ClaimState == ApartmentClaimState.Unclaimed ? "Pulsa E para reclamarlo" : "");

                        if (Input.Pressed("Use"))
                        {
                            Log.Info($"[Interact] Type: ApartmentClaimInteractable, Prompt: {prompt}, CanInteract: True");
                            Log.Info("[Interact] Sending RPC to Host...");
                            var pressable = claimable as Component.IPressable;
                            if (pressable.CanPress(new Component.IPressable.Event()))
                            {
                                pressable.Press(new Component.IPressable.Event());
                            }
                        }
                    }
                    return;
                }

                // 2. Stash
                var stashInv = tr.GameObject.Components.Get<InventoryComponent>();
                if (stashInv != null)
                {
                    var apt = tr.GameObject.Components.GetInAncestorsOrSelf<ApartmentComponent>();
                    bool canOpen = apt == null || apt.OwnerId == Game.SteamId.ToString();
                    
                    if (canOpen)
                    {
                        _hud?.ShowPrompt("Pulsa E para abrir el alijo", "");
                        if (Input.Pressed("Use"))
                        {
                            Log.Info("[Interact] Type: Stash, Prompt: Abrir alijo, CanInteract: True");
                            _hud?.OpenStash(stashInv);
                        }
                    }
                    else
                    {
                        _hud?.ShowPrompt("No puedes acceder a este apartamento", "");
                        if (Input.Pressed("Use"))
                        {
                            Log.Info("[Interact] Type: Stash, Prompt: Denegado, CanInteract: False");
                        }
                    }
                    return;
                }
                
                // 3. Trader
                var trader = tr.GameObject.Components.Get<UltimoBarrio.Trading.Trader>();
                if (trader != null)
                {
                    _hud?.ShowPrompt("Comerciante", "Pulsa E");
                    if (Input.Pressed("Use"))
                    {
                        Log.Info("[Interact] Type: Trader, Prompt: Comerciante, CanInteract: True");
                        _hud?.OpenTrader(trader);
                    }
                    return;
                }

                // 4. IInteractable (Pickups, etc)
                var interactable = tr.GameObject.Components.Get<IInteractable>();
                if (interactable != null)
                {
                    var req = new InteractionRequest { InteractorId = GameObject.Network.OwnerId.ToString(), InteractorObject = GameObject };
                    string prompt = interactable.GetInteractionPrompt(req);
                    _hud?.ShowPrompt(prompt, "Pulsa E");
                    
                    if (Input.Pressed("Use"))
                    {
                        bool can = interactable.CanInteract(req);
                        Log.Info($"[Interact] Type: IInteractable, Prompt: {prompt}, CanInteract: {can}");
                        if (can)
                        {
                            Log.Info("[Interact] Sending RPC/Call to Host...");
                            interactable.OnInteract(req);
                        }
                    }
                    return;
                }
            }

            _hud?.HidePrompt();
        }
    }
}
