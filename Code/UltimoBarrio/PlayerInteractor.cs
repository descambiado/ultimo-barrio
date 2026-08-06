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
                // Only query for IWorldInteractable components
                var interactable = tr.GameObject.Components.Get<IWorldInteractable>();
                if (interactable != null)
                {
                    var req = new InteractionRequest
                    {
                        Identity = PlayerIdentity.FromGameObject(GameObject),
                        InteractorObject = GameObject
                    };

                    // Special handling for Apartment Claim
                    var claimable = interactable as ApartmentClaimInteractable;
                    if (claimable != null)
                    {
                        var apt = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == claimable.ApartmentId);
                        if (apt != null)
                        {
                            // Ya reclamada: la puerta pasa a comportarse como puerta real
                            // (ApartmentDoorPolicy, abrir/cerrar), no como portal de claim.
                            // Sin esto, Get<IWorldInteractable>() siempre resolvía a
                            // ApartmentClaimInteractable (mismo GameObject, cast primero)
                            // y el propietario nunca podía llegar a abrir/cerrar su puerta.
                            if (apt.ClaimState != ApartmentClaimState.Unclaimed)
                            {
                                var doorPolicy = tr.GameObject.Components.Get<Apartments.ApartmentDoorPolicy>();
                                if (doorPolicy != null)
                                {
                                    string doorPrompt = doorPolicy.GetInteractionPrompt(req);
                                    bool doorCan = doorPolicy.CanInteract(req);
                                    _hud?.ShowPrompt(doorPrompt, doorCan ? "Pulsa E" : "");

                                    if (Input.Pressed("Use") && doorCan)
                                    {
                                        Log.Info($"[Interact] DoorPolicy: {claimable.ApartmentId}");
                                        doorPolicy.OnInteract(req);
                                    }
                                    return;
                                }

                                _hud?.ShowPrompt("Este piso ya tiene dueño", "");
                                return;
                            }

                            _hud?.ShowPrompt("Este piso está disponible", "Pulsa E para reclamarlo");

                            if (Input.Pressed("Use"))
                            {
                                Log.Info($"[Interact] Claimable: {claimable.ApartmentId}");
                                var pressable = claimable as Component.IPressable;
                                pressable?.Press(new Component.IPressable.Event());
                            }
                        }
                        return;
                    }

                    // Special handling for Trader
                    var trader = interactable as Trading.Trader;
                    if (trader != null)
                    {
                        _hud?.ShowPrompt("Comerciante", "Pulsa E para comerciar");
                        if (Input.Pressed("Use"))
                        {
                            Log.Info($"[Interact] Opening Trader");
                            _hud?.OpenTrader(trader);
                        }
                        return;
                    }

                    // Special handling for CraftingStation
                    var crafting = interactable as Crafting.CraftingStation;
                    if (crafting != null)
                    {
                        _hud?.ShowPrompt("Estación de crafteo", "Pulsa E para fabricar");
                        if (Input.Pressed("Use"))
                        {
                            Log.Info($"[Interact] Opening Crafting Station");
                            _hud?.OpenCrafting(crafting);
                        }
                        return;
                    }

                    // Special handling for World Container (Stash)
                    var container = interactable as IWorldContainer;
                    if (container != null)
                    {
                        string prompt = interactable.GetInteractionPrompt(req);
                        bool canInteract = interactable.CanInteract(req);
                        _hud?.ShowPrompt(prompt, canInteract ? "Pulsa E" : "");

                        if (Input.Pressed("Use"))
                        {
                            Log.Info($"[Interact] Stash/Container interact, canInteract={canInteract}");
                            if (canInteract)
                            {
                                var inv = container.GetContainerInventory();
                                if (inv != null)
                                {
                                    _hud?.OpenStash(inv);
                                }
                            }
                        }
                        return;
                    }

                    // Default IWorldInteractable (Pickups, etc)
                    string defaultPrompt = interactable.GetInteractionPrompt(req);
                    bool can = interactable.CanInteract(req);
                    _hud?.ShowPrompt(defaultPrompt, can ? "Pulsa E" : "");

                    if (Input.Pressed("Use"))
                    {
                        Log.Info($"[Interact] Direct IWorldInteractable: {defaultPrompt}, Can={can}");
                        if (can)
                        {
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
