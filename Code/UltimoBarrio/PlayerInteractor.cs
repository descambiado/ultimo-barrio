using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;
using UltimoBarrio.UI;
using UltimoBarrio.Apartments;
using UltimoBarrio.Players;

namespace UltimoBarrio
{
    [Title("Player Interactor")]
    [Category("Último Barrio")]
    [Icon("pan_tool")]
    public sealed class PlayerInteractor : Component
    {
        [Property] public float InteractionRange { get; set; } = 150f;

        private PlayerHud _hud;
        private static int _pickupAttemptCounter;

        protected override void OnStart()
        {
            _hud = Components.Get<PlayerHud>();
            
            // Set deterministic inventory ID.
            // SteamPlayerIdentityProvider es una clase simple, no un Component -- nunca
            // se registra en la escena (todos los demás servicios ya la instancian
            // localmente por la misma razón: ApartmentClaimService, PropertyClaimService,
            // RentalService, KeyringService). Buscarla con Scene.GetAllComponents<>()
            // devolvía null siempre, así que este bloque nunca llegaba a fijar InventoryId
            // -- causa raíz real de por qué las credenciales del llavero (y cualquier otro
            // estado indexado por InventoryId del jugador local) no sobrevivían Stop/Play.
            var inv = Components.Get<InventoryComponent>();
            if (inv != null && GameObject.Network.OwnerId != Guid.Empty)
            {
                IPlayerIdentityProvider identityProvider = new SteamPlayerIdentityProvider();
                var connection = Connection.All.FirstOrDefault(c => c.Id == GameObject.Network.OwnerId);
                if (connection != null && identityProvider.TryResolve(connection, out var ownerId))
                {
                    inv.InventoryId = $"player:{ownerId}:inventory";
                }
                else
                {
                    inv.InventoryId = $"player:{GameObject.Network.OwnerId}:inventory";
                }

                // Condición explícita (no delay): jugador válido (this OnStart ya corriendo)
                // + InventoryId recién fijado (no vacío) + KeyringItem disponible en este
                // GameObject + save realmente cargado (TryReapplyPlayerState no hace nada
                // si _loadedSnapshot es null). Reaplica llavero/economía/hotbar para este
                // jugador en concreto por si ApartmentClaimService.OnStart ya había corrido
                // el Apply original antes de que este InventoryId existiera.
                if (!string.IsNullOrEmpty(inv.InventoryId) && Components.Get<Properties.Keys.KeyringItem>() != null)
                {
                    var claimService = Scene.GetAllComponents<ApartmentClaimService>().FirstOrDefault();
                    claimService?.TryReapplyPlayerState();
                }
            }
        }

        protected override void OnUpdate()
        {
            var usePressed = Input.Pressed("Use");

            // Se loguea solo cuando ocurre de verdad (nunca por frame) -- evidencia de que
            // el evento físico de teclado llegó realmente a este componente, para la única
            // prueba manual pendiente (mirar pickup -> pulsar E una vez).
            if (usePressed)
                Log.Info($"UB.Input UsePressed Player={GameObject.Id} State={_hud?.CurrentState} Cursor={Mouse.Visibility}");

            ProcessInteraction(usePressed);
        }

        /// <summary>
        /// HERRAMIENTA DE DESARROLLO/QA — nunca la llama código de producción, solo
        /// QASprintRunner. Sustituye ÚNICAMENTE el evento físico de teclado (imposible
        /// de simular desde MCP, confirmado sin herramienta de input en el toolset) y
        /// reejecuta EXACTAMENTE ProcessInteraction — el mismo trace, la misma
        /// resolución de IWorldInteractable, el mismo CanInteract, la misma RPC al
        /// host, el mismo networking. No añade ítems, no toca wallet, no reclama, no
        /// instala nada por su cuenta, no se salta CanInteract. Genérico a propósito:
        /// sirve para pickups, puertas, armarios, estaciones de crafteo o cualquier
        /// IWorldInteractable con solo apuntar al jugador hacia el objetivo antes de
        /// llamarlo — no crear DebugForceCraft/DebugForceClaim/DebugForceDoor por
        /// separado, este único método ya cubre todos esos casos porque toda la
        /// lógica específica por tipo vive en ProcessInteraction, no aquí.
        /// </summary>
        public void DebugForceUseAttempt()
        {
            ProcessInteraction(true);
        }

        /// <summary>
        /// Punto único de resolución de interacción (trace + prompt + press).
        /// OnUpdate lo llama cada frame con el estado real de pressed.
        /// DebugForceUseAttempt (instrumentación temporal, UB.Pickup) lo llama con
        /// pressed=true para poder probar la cadena completa sin poder simular una
        /// pulsación de teclado real desde MCP — usa exactamente el mismo trace,
        /// la misma resolución de IWorldInteractable y el mismo CanInteract/OnInteract
        /// que un E real, no un atajo que fabrique el resultado.
        /// </summary>
        private void ProcessInteraction(bool pressed)
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

            if (pressed)
            {
                Log.Info($"UB.Pickup Trace camPos={rayPos} camFwd={rayDir} start={rayPos} end={rayPos + rayDir * InteractionRange} " +
                    $"hit={tr.Hit} hitObject={tr.GameObject?.Name ?? "none"} hitCollider={tr.Shape?.Collider?.GetType().Name ?? "none"} " +
                    $"distance={tr.Distance} parent={tr.GameObject?.Parent?.Name ?? "none"}");
            }

            if (tr.Hit && tr.GameObject != null)
            {
                // Only query for IWorldInteractable components
                var interactable = InteractionResolver.Find( tr.GameObject );
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

                                    if (pressed && doorCan)
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

                            if (pressed)
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
                        if (pressed)
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
                        if (pressed)
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

                        if (pressed)
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
                    bool can = InteractionResolver.CanUse( interactable, req );
                    _hud?.ShowPrompt(defaultPrompt, can ? "Pulsa E" : "");

                    if (pressed)
                    {
                        var attemptId = ++_pickupAttemptCounter;
                        var reason = can ? "OK" : "CanInteract=false (ver distancia/condición en el propio IWorldInteractable)";
                        Log.Info($"UB.Pickup Attempt={attemptId} InputPressed action=Use");
                        Log.Info($"UB.Pickup Attempt={attemptId} Hit={tr.GameObject.Name}");
                        Log.Info($"UB.Pickup Attempt={attemptId} Interactable={interactable.GetType().Name}");
                        Log.Info($"UB.Pickup Attempt={attemptId} CanInteract={can} Reason={reason}");

                        if (can)
                        {
                            if (interactable is WorldItemPickup pickup)
                                pickup.DebugAttemptId = attemptId;

                            InteractionResolver.TryUse( interactable, req );
                        }
                    }
                    return;
                }
            }

            _hud?.HidePrompt();
        }
    }
}
