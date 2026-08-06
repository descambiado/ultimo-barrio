using Sandbox;
using Sandbox.UI;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Combat;
using UltimoBarrio.Inventory;
using UltimoBarrio.Players;
using UltimoBarrio.Economy;
using UltimoBarrio.Core;
using UltimoBarrio.UI;
using UltimoBarrio.Properties;
using UltimoBarrio.Properties.Doors;
using UltimoBarrio.Properties.Keys;

namespace UltimoBarrio.QA
{
    public static class QASprintRunner
    {
        [ConCmd("ub_qa_held_state")]
        public static void CheckHeldState()
        {
            var p = Game.ActiveScene.GetAllComponents<HeldItemController>().FirstOrDefault(x => x.GameObject.Root.Name.Contains("Player", System.StringComparison.OrdinalIgnoreCase) || x.GameObject.Root.Name.Contains("Descambiado", System.StringComparison.OrdinalIgnoreCase));
            if (p == null) { Log.Error("No HeldItemController found on Player."); return; }

            Log.Info("--- ub_qa_held_state ---");
            Log.Info($"CurrentSlot: {p.CurrentSlot}");
            var currentWeapon = p.GameObject.Components.GetAll<Component>(FindMode.EverythingInDescendants).FirstOrDefault(c => c.GetType().Name.Contains("Weapon"));
            if (currentWeapon != null)
            {
                Log.Info($"CurrentWeapon: {currentWeapon.GameObject.Name}");
            }
            else
            {
                Log.Info("CurrentWeapon: None (Hands visible)");
            }
        }

        [ConCmd("ub_qa_weapon_state")]
        public static void CheckWeaponState()
        {
            var p = Game.ActiveScene.GetAllComponents<HeldItemController>().FirstOrDefault(x => x.GameObject.Root.Name.Contains("Player", System.StringComparison.OrdinalIgnoreCase) || x.GameObject.Root.Name.Contains("Descambiado", System.StringComparison.OrdinalIgnoreCase));
            if (p == null) { Log.Error("No HeldItemController or Weapon found."); return; }
            var currentWeapon = p.GameObject.Components.GetAll<Component>(FindMode.EverythingInDescendants).FirstOrDefault(c => c.GetType().Name.Contains("Weapon"));

            Log.Info("--- ub_qa_weapon_state ---");
            if (currentWeapon != null)
            {
                Log.Info($"WeaponActive: {currentWeapon.GameObject.Name}");
            }
        }

        [ConCmd("ub_qa_spawn_dummy")]
        public static void SpawnDummy()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player == null) { Log.Error("Player not found"); return; }

            var dummyGo = new GameObject(true, "QA_CombatDummy");
            dummyGo.WorldPosition = player.WorldPosition + player.WorldRotation.Forward * 100f + Vector3.Up * 10f;
            
            var model = dummyGo.Components.Create<ModelRenderer>();
            model.Model = Model.Load("models/citizen/citizen.vmdl");
            
            var collider = dummyGo.Components.Create<BoxCollider>();
            collider.Scale = new Vector3(20, 20, 72);
            collider.Center = new Vector3(0, 0, 36);

            var hc = dummyGo.Components.Create<HealthComponent>();
            hc.MaxHealth = 100f;
            
            Log.Info($"Dummy spawned at {dummyGo.WorldPosition} with {hc.Health} HP.");
        }

        [ConCmd("ub_qa_dummy_state")]
        public static void CheckDummyState()
        {
            var dummyGo = Game.ActiveScene.GetAllObjects(true).FirstOrDefault(x => x.Name == "QA_CombatDummy");
            if (dummyGo == null) { Log.Error("Dummy not found."); return; }

            var hc = dummyGo.Components.Get<HealthComponent>();
            if (hc != null)
            {
                Log.Info("--- ub_qa_dummy_state ---");
                Log.Info($"Dummy HP: {hc.Health} / {hc.MaxHealth}");
            }
        }

        [ConCmd("ub_qa_stash_state")]
        public static void CheckStashState()
        {
            var interactor = Game.ActiveScene.GetAllComponents<PlayerInteractor>().FirstOrDefault();
            if (interactor == null) { Log.Error("PlayerInteractor not found."); return; }

            var tr = Game.ActiveScene.Trace.Ray(interactor.GameObject.WorldPosition + Vector3.Up * 64f, interactor.GameObject.WorldPosition + Vector3.Up * 64f + interactor.GameObject.WorldRotation.Forward * 150f)
                .IgnoreGameObjectHierarchy(interactor.GameObject)
                .Run();
            
            Log.Info("--- ub_qa_stash_state ---");
            if (tr.Hit && tr.GameObject != null)
            {
                var stash = tr.GameObject.Components.GetInAncestorsOrSelf<StashComponent>();
                if (stash != null)
                {
                    Log.Info($"OwnerId (Apartment): {stash.ApartmentId}");
                    Log.Info($"ExternalInventoryId: {stash.InventoryId}");
                }
                else
                {
                    Log.Info("Looking at: " + tr.GameObject.Name + " (No StashComponent)");
                }
            }
            else
            {
                Log.Info("Not looking at anything in range.");
            }
        }

        [ConCmd("ub_qa_movement_state")]
        public static void CheckMovementState()
        {
            var scene = Game.ActiveScene;
            var mod = scene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault();
            if (mod == null) { Log.Error("PlayerMovementModifier not found."); return; }

            Log.Info("--- ub_qa_movement_state ---");
            Log.Info($"stamina actual: {mod.CurrentStamina}");
            Log.Info($"IsExhausted: {mod.IsExhausted}");
            Log.Info($"IsSprinting: {mod.IsSprinting}");
        }

        /// <summary>
        /// Estado real de cámara y controlador. Sandbox.PlayerController es un
        /// ICameraModifier: la cámara principal vive en la escena y el controlador
        /// le compone la vista cada frame, por eso el transform del GameObject y la
        /// vista compuesta (View) no coinciden y hay que registrar ambos.
        /// </summary>
        [ConCmd("ub_qa_camera_state")]
        public static void CheckCameraState()
        {
            var scene = Game.ActiveScene;
            var controller = scene.GetAllComponents<Sandbox.PlayerController>().FirstOrDefault();
            var cameras = scene.GetAllComponents<CameraComponent>().Where(c => c.Active).ToList();

            Log.Info("--- ub_qa_camera_state ---");
            Log.Info($"Active CameraComponent count: {cameras.Count}");

            foreach (var c in cameras)
            {
                Log.Info($"  camera '{c.GameObject.Name}' parent='{c.GameObject.Parent?.Name ?? "<scene root>"}' IsMainCamera={c.IsMainCamera}");
            }

            if (controller == null) { Log.Error("No PlayerController in scene."); return; }

            var player = controller.GameObject;
            Log.Info($"Player.WorldPosition: {player.WorldPosition}");
            Log.Info($"Player.WorldRotation: {player.WorldRotation.Angles()}");
            Log.Info($"Controller.EyeAngles: {controller.EyeAngles}");
            Log.Info($"Controller.EyePosition: {controller.EyePosition}");
            Log.Info($"Current camera mode: {(controller.ThirdPerson ? "ThirdPerson" : "FirstPerson")}");
            Log.Info($"UseCameraControls={controller.UseCameraControls} UseLookControls={controller.UseLookControls} CameraOffset={controller.CameraOffset}");

            var main = scene.Camera;
            if (main == null) { Log.Error("Scene.Camera is null."); return; }

            Log.Info($"Camera GameObject: '{main.GameObject.Name}' parent='{main.GameObject.Parent?.Name ?? "<scene root>"}'");
            Log.Info($"Camera.WorldPosition: {main.WorldPosition}");
            Log.Info($"Camera.WorldRotation: {main.WorldRotation.Angles()}");
            Log.Info($"Camera.View.Position (compuesta): {main.View.Position}");
            Log.Info($"Camera.View.Rotation (compuesta): {main.View.Rotation.Angles()}");
            Log.Info($"Camera.View.Forward: {main.View.Rotation.Forward}");

            // Base de movimiento que Sandbox.PlayerController usa para convertir
            // Input.AnalogMove en WishVelocity: los ejes de EyeAngles sin pitch.
            var moveBasis = controller.EyeAngles.WithPitch(0f).ToRotation();
            Log.Info($"Move basis Forward (W): {moveBasis.Forward}");
            Log.Info($"Move basis Right (D): {moveBasis.Right}");
            Log.Info($"dot(W, camForward.xy): {Vector3.Dot(moveBasis.Forward, main.View.Rotation.Forward.WithZ(0).Normal)}");
        }

        /// <summary>
        /// QA: estado del cursor y del look. Si el cursor está visible, el motor no
        /// entrega Input.AnalogLook y la cámara se queda clavada aunque WASD funcione.
        /// </summary>
        [ConCmd("ub_qa_input_state")]
        public static void CheckInputState()
        {
            Log.Info("--- ub_qa_input_state ---");
            Log.Info($"Mouse.Visibility: {Mouse.Visibility}");
            Log.Info($"Mouse.Active: {Mouse.Active}");
            Log.Info($"Input.MouseCursorVisible: {Input.MouseCursorVisible}");
            Log.Info($"Input.Suppressed: {Input.Suppressed}");
            Log.Info($"Input.AnalogLook: {Input.AnalogLook}");
            Log.Info($"Input.AnalogMove: {Input.AnalogMove}");

            var hud = Game.ActiveScene.GetAllComponents<UltimoBarrio.UI.PlayerHud>().FirstOrDefault();
            Log.Info($"PlayerHud.CurrentState: {(hud == null ? "<no hud>" : hud.CurrentState.ToString())}");

            foreach (var pc in Game.ActiveScene.GetAllComponents<PanelComponent>())
            {
                var p = pc.Panel;
                if (p == null) { Log.Info($"  panel {pc.GetType().Name}: <null>"); continue; }
                Log.Info($"  panel {pc.GetType().Name}: display={p.ComputedStyle?.Display} pointerEvents={p.ComputedStyle?.PointerEvents}");
            }
        }

        /// <summary>QA: fija EyeAngles para comprobar que la cámara sigue al look.</summary>
        [ConCmd("ub_qa_look")]
        public static void SetLook(float yaw, float pitch)
        {
            var controller = Game.ActiveScene.GetAllComponents<Sandbox.PlayerController>().FirstOrDefault();
            if (controller == null) { Log.Error("No PlayerController in scene."); return; }

            controller.EyeAngles = new Angles(pitch, yaw, 0f);
            Log.Info($"--- ub_qa_look --- EyeAngles set to {controller.EyeAngles}");
        }

        /// <summary>QA: fuerza el modo de cámara para capturar evidencia sin pulsar la tecla View.</summary>
        [ConCmd("ub_qa_camera_mode")]
        public static void SetCameraMode(int thirdPerson)
        {
            var controller = Game.ActiveScene.GetAllComponents<Sandbox.PlayerController>().FirstOrDefault();
            if (controller == null) { Log.Error("No PlayerController in scene."); return; }

            controller.ThirdPerson = thirdPerson != 0;
            Log.Info($"--- ub_qa_camera_mode --- ThirdPerson={controller.ThirdPerson}");
        }
        [ConCmd("ub_qa_run_preflight")]
        public static void RunPreflight()
        {
            Log.Info("--- ub_qa_run_preflight ---");
            
            // 1. Give Items
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player != null)
            {
                var inv = player.Components.Get<InventoryComponent>();
                if (inv != null)
                {
                    inv.TryAdd("weapon_crowbar", 1);
                    inv.TryAdd("weapon_usp", 1);
                    inv.TryAdd("ammo_9mm", 24);
                    inv.TryAdd("chatarra", 5);
                }
            }
            
            // 2. Dump Player
            var hud = Game.ActiveScene.GetAllComponents<UI.PlayerHud>().FirstOrDefault();
            var invComp = player?.Components.Get<InventoryComponent>();
            var held = player?.Components.Get<HeldItemController>();

            Log.Info($"PlayerHud presente: {hud != null}");
            Log.Info($"HotbarPanel creado: {hud?.Hotbar != null}");
            Log.Info($"InventoryComponent presente: {invComp != null}");
            Log.Info($"HeldItemController presente: {held != null}");
            Log.Info($"ItemRegistry disponible: {ItemRegistry.GetDefinition("chatarra") != null}");
            Log.Info($"SelectedSlot: {held?.SelectedHotbarSlot}");
            Log.Info($"SelectedItemId: {held?.ActiveItemId}");
            
            var wpn = held?.GameObject.Components.GetAll<Component>(FindMode.EverythingInDescendants).FirstOrDefault(c => c.GetType().Name.Contains("Weapon"));
            Log.Info($"ActiveViewModel: {wpn != null}");
            Log.Info($"ActiveWorldModel: {wpn != null}");

            // 3. Dump Registry
            var items = new[] { "chatarra", "water", "medicine", "ammo_9mm", "weapon_crowbar", "weapon_usp" };
            foreach (var id in items)
            {
                var def = ItemRegistry.GetDefinition(id);
                if (def != null)
                {
                    Log.Info($"ItemId: {def.ItemId}");
                    Log.Info($"Category: {def.Category}");
                    Log.Info($"EquipSlot: {def.EquipSlot}");
                    Log.Info($"WorldPrefab: {def.WorldPrefab}");
                    Log.Info($"ViewModelPrefab: {def.ViewModelPrefab}");
                    Log.Info($"WorldModelPrefab: {def.WorldModelPrefab}");
                    Log.Info($"Droppable: {def.Droppable}");
                }
            }
        }

        /// <summary>
        /// Diagnóstico: invoca el mismo gateway público (IWorldInteractable.OnInteract)
        /// que PlayerInteractor llama al pulsar E, sobre el ResourceNode/pickup más
        /// cercano al jugador. Esto NO valida el input físico (eso ya lo prueba el
        /// propio jugador) — solo aísla si el fallo está en la cadena de interacción
        /// o después de ella (host, inventario, etc).
        /// </summary>
        [ConCmd("ub_qa_test_pickup")]
        public static void TestNearestPickup()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_test_pickup --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            if (inv is null) { Log.Error("--- ub_qa_test_pickup --- Player has no InventoryComponent."); return; }

            var pickup = Game.ActiveScene.GetAllComponents<UltimoBarrio.WorldItemPickup>()
                .Where(p => p.GameObject.IsValid())
                .OrderBy(p => Vector3.DistanceBetween(p.WorldPosition, player.WorldPosition))
                .FirstOrDefault();

            if (pickup is null) { Log.Error("--- ub_qa_test_pickup --- No WorldItemPickup found in scene."); return; }

            // Acercamos al jugador como si hubiera caminado hasta el pickup — no
            // fabrica el resultado de la interacción, solo el requisito de distancia
            // que CanInteract comprobará de todas formas.
            player.WorldPosition = pickup.WorldPosition + Vector3.Up * 10f;

            var before = inv.GetCount(pickup.ItemId);
            Log.Info($"--- ub_qa_test_pickup --- target={pickup.GameObject.Name} itemId={pickup.ItemId} distance={Vector3.DistanceBetween(pickup.WorldPosition, player.WorldPosition):F1} countBefore={before}");

            var req = new InteractionRequest
            {
                Identity = PlayerIdentity.FromGameObject(player),
                InteractorObject = player
            };

            bool can = pickup.CanInteract(req);
            Log.Info($"--- ub_qa_test_pickup --- CanInteract={can}");
            if (!can) return;

            pickup.OnInteract(req);

            var after = inv.GetCount(pickup.ItemId);
            Log.Info($"--- ub_qa_test_pickup --- countAfter={after} delta={after - before} pickupStillValid={pickup.GameObject.IsValid()}");
        }

        /// <summary>
        /// Diagnóstico: ejercita drop -> spawn de pickup real -> re-recogida por el
        /// mismo gateway OnInteract, y el consumo de un consumible sin curación
        /// (agua). Usa InventoryComponent.RequestDrop y HeldItemController
        /// (los gateways reales), no atajos que fabriquen el resultado.
        /// </summary>
        [ConCmd("ub_qa_test_drop_repickup")]
        public static void TestDropRepickup()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_test_drop_repickup --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            if (inv is null) { Log.Error("--- ub_qa_test_drop_repickup --- No InventoryComponent."); return; }

            // Setup: aseguramos que hay algo que soltar (no es lo que se está probando).
            if (inv.GetCount("chatarra") < 2)
                inv.TryAdd("chatarra", 2);

            var beforeDrop = inv.GetCount("chatarra");
            inv.RequestDrop("chatarra", 1);

            var afterDrop = inv.GetCount("chatarra");
            Log.Info($"--- ub_qa_test_drop_repickup --- drop beforeDrop={beforeDrop} afterDrop={afterDrop}");

            var dropped = Game.ActiveScene.GetAllComponents<UltimoBarrio.WorldItemPickup>()
                .Where(p => p.GameObject.IsValid() && p.ItemId == "chatarra")
                .OrderBy(p => Vector3.DistanceBetween(p.WorldPosition, player.WorldPosition))
                .FirstOrDefault();

            if (dropped is null) { Log.Error("--- ub_qa_test_drop_repickup --- No dropped pickup found nearby."); return; }

            player.WorldPosition = dropped.WorldPosition + Vector3.Up * 10f;

            var req = new InteractionRequest { Identity = PlayerIdentity.FromGameObject(player), InteractorObject = player };
            bool can = dropped.CanInteract(req);
            dropped.OnInteract(req);

            var afterRepickup = inv.GetCount("chatarra");
            Log.Info($"--- ub_qa_test_drop_repickup --- repickup can={can} afterRepickup={afterRepickup} restored={afterRepickup == beforeDrop}");

            // Consumible sin curación (agua) por el gateway real de HeldItemController.
            var held = player.Components.Get<HeldItemController>();
            if (held is null) { Log.Error("--- ub_qa_test_drop_repickup --- No HeldItemController."); return; }

            inv.TryAdd("water", 1);
            var waterBefore = inv.GetCount("water");

            var health = player.Components.GetInDescendantsOrSelf<Combat.HealthComponent>();
            if (health is not null && health.Health >= health.MaxHealth)
                health.TakeDamage(new DamageEvent { Amount = 10f });

            // UseActiveConsumable usa ActiveItemId; forzamos la selección del slot con agua.
            var waterSlotIndex = inv.Slots.ToList().FindIndex(s => s.ItemId == "water" && s.Amount > 0);
            if (waterSlotIndex >= 0)
            {
                held.SelectSlot(waterSlotIndex);
                held.UseActiveConsumable();
            }

            var waterAfter = inv.GetCount("water");
            Log.Info($"--- ub_qa_test_drop_repickup --- water waterBefore={waterBefore} waterAfter={waterAfter} consumed={waterAfter < waterBefore}");
        }

        /// <summary>
        /// Diagnóstico: ejercita CraftingStation.RequestCraft (el mismo gateway
        /// [Rpc.Host] que la UI real invoca al pulsar "Fabricar") para la receta
        /// del kit de puerta, dos veces: una con ingredientes suficientes (debe
        /// consumir atómicamente y añadir el resultado) y otra sin ellos (debe
        /// rechazar sin tocar el inventario — rollback implícito al no mutar nada).
        /// </summary>
        [ConCmd("ub_qa_test_craft")]
        public static void TestCraft()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_test_craft --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            if (inv is null) { Log.Error("--- ub_qa_test_craft --- No InventoryComponent."); return; }

            var station = Game.ActiveScene.GetAllComponents<Crafting.CraftingStation>().FirstOrDefault();
            if (station is null) { Log.Error("--- ub_qa_test_craft --- No CraftingStation in scene."); return; }

            // Caso B primero (sin ingredientes) para no contaminarlo con el setup del caso A.
            inv.TryRemove("wood", inv.GetCount("wood"));
            inv.TryRemove("scrap_metal", inv.GetCount("scrap_metal"));
            inv.TryRemove("components", inv.GetCount("components"));
            inv.TryRemove("apartment_door_kit", inv.GetCount("apartment_door_kit"));

            player.WorldPosition = station.WorldPosition;

            var beforeFail = inv.GetCount("apartment_door_kit");
            station.RequestCraft(player.Id, "craft_apartment_door_kit");
            var afterFail = inv.GetCount("apartment_door_kit");
            Log.Info($"--- ub_qa_test_craft --- sin ingredientes: kitBefore={beforeFail} kitAfter={afterFail} rechazadoCorrectamente={afterFail == beforeFail}");

            // Caso A: con ingredientes suficientes.
            inv.TryAdd("wood", 8);
            inv.TryAdd("scrap_metal", 6);
            inv.TryAdd("components", 2);

            var woodBefore = inv.GetCount("wood");
            var scrapBefore = inv.GetCount("scrap_metal");
            var compBefore = inv.GetCount("components");
            var kitBefore = inv.GetCount("apartment_door_kit");

            station.RequestCraft(player.Id, "craft_apartment_door_kit");

            var woodAfter = inv.GetCount("wood");
            var scrapAfter = inv.GetCount("scrap_metal");
            var compAfter = inv.GetCount("components");
            var kitAfter = inv.GetCount("apartment_door_kit");

            Log.Info($"--- ub_qa_test_craft --- con ingredientes: wood {woodBefore}->{woodAfter} scrap {scrapBefore}->{scrapAfter} components {compBefore}->{compAfter} kit {kitBefore}->{kitAfter}");
            Log.Info($"--- ub_qa_test_craft --- exito={kitAfter > kitBefore && woodAfter == woodBefore - 8 && scrapAfter == scrapBefore - 6 && compAfter == compBefore - 2}");
        }

        /// <summary>
        /// Diagnóstico: la puerta física de un apartamento — DestructibleStructure
        /// autocreada por ApartmentFortification sobre la Claim Portal, daño,
        /// reparación y el bloqueo físico real de ApartmentDoorPolicy (Collider
        /// IsTrigger). No sustituye la reclamación real (ya probada por
        /// ApartmentClaimService + ub_test_all); aísla si la puerta resultante
        /// tiene vida/daño/reparación/bloqueo reales.
        /// </summary>
        [ConCmd("ub_qa_test_door")]
        public static void TestDoor(string apartmentId = "apartment-a01")
        {
            var apt = Game.ActiveScene.GetAllComponents<Apartments.ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == apartmentId);
            if (apt is null) { Log.Error($"--- ub_qa_test_door --- Apartamento '{apartmentId}' no encontrado."); return; }

            var fort = apt.GameObject.Components.Get<Fortification.ApartmentFortification>();
            Log.Info($"--- ub_qa_test_door --- ApartmentFortification presente: {fort != null}");
            if (fort is null) return;

            var doorStructure = fort.DoorStructure;
            Log.Info($"--- ub_qa_test_door --- DoorReference resuelto: {fort.DoorReference?.Name} DoorStructure: {doorStructure != null}");
            if (doorStructure is null) return;

            Log.Info($"--- ub_qa_test_door --- Health inicial: {doorStructure.Health}/{doorStructure.MaxHealth}");

            doorStructure.TakeDamage(new DamageEvent { Amount = 50f, Position = doorStructure.WorldPosition });
            Log.Info($"--- ub_qa_test_door --- Tras 50 de daño: {doorStructure.Health}/{doorStructure.MaxHealth} dañoAplicado={doorStructure.Health < doorStructure.MaxHealth}");

            doorStructure.Repair(20f);
            Log.Info($"--- ub_qa_test_door --- Tras reparar 20: {doorStructure.Health}/{doorStructure.MaxHealth}");

            var doorPolicy = fort.DoorReference?.Components.Get<Apartments.ApartmentDoorPolicy>();
            var col = fort.DoorReference?.Components.Get<Collider>();
            if (doorPolicy is null || col is null) { Log.Error("--- ub_qa_test_door --- Falta ApartmentDoorPolicy o Collider."); return; }

            Log.Info($"--- ub_qa_test_door --- IsLocked={doorPolicy.IsLocked} Collider.IsTrigger={col.IsTrigger} (bloqueando={!col.IsTrigger})");

            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is not null)
            {
                var req = new InteractionRequest { Identity = PlayerIdentity.FromGameObject(player), InteractorObject = player };
                bool canBeforeOwnership = doorPolicy.CanInteract(req);
                Log.Info($"--- ub_qa_test_door --- CanInteract (jugador de prueba, probablemente no propietario)={canBeforeOwnership}");

                // Si el apartamento sigue libre, reclamarlo de verdad (kit real +
                // gateway real IPressable.Press) para poder probar el resto del
                // ciclo (toggle de puerta, upgrade) como propietario real.
                if (apt.ClaimState == Apartments.ApartmentClaimState.Unclaimed)
                {
                    var invClaim = player.Components.Get<InventoryComponent>();
                    invClaim?.TryAdd("apartment_door_kit", 1);
                    player.WorldPosition = fort.DoorReference.WorldPosition;

                    var claimInteractable = fort.DoorReference.Components.Get<Apartments.ApartmentClaimInteractable>();
                    var pressable = claimInteractable as Component.IPressable;
                    pressable?.Press(new Component.IPressable.Event());
                    Log.Info($"--- ub_qa_test_door --- Claim intentado: ClaimState={apt.ClaimState} OwnerId={apt.OwnerId}");
                }

                if (apt.OwnerId == req.Identity.CanonicalId)
                {
                    doorPolicy.OnInteract(req);
                    Log.Info($"--- ub_qa_test_door --- Tras toggle: IsLocked={doorPolicy.IsLocked} Collider.IsTrigger={col.IsTrigger}");

                    var maxHealthBefore = doorStructure.MaxHealth;
                    var levelBefore = fort.UpgradeLevel;
                    var invUp = player.Components.Get<InventoryComponent>();
                    invUp?.TryAdd("wood", 20);
                    invUp?.TryAdd("chatarra", 20);
                    invUp?.TryAdd("components", 20);
                    bool upgraded = fort.TryUpgrade(player);
                    Log.Info($"--- ub_qa_test_door --- TryUpgrade={upgraded} nivel {levelBefore}->{fort.UpgradeLevel} maxHealth {maxHealthBefore}->{doorStructure.MaxHealth}");
                }
            }
        }

        /// <summary>
        /// Diagnóstico: coloca una barricada real (BarricadeAnchor.OnInteract, el
        /// mismo gateway que E) en el anchor más cercano, la daña, la repara y
        /// confirma que destruirla libera el anchor.
        /// </summary>
        [ConCmd("ub_qa_test_barricade")]
        public static void TestBarricade()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_test_barricade --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            if (inv is null) { Log.Error("--- ub_qa_test_barricade --- No InventoryComponent."); return; }

            var anchors = Game.ActiveScene.GetAllComponents<Fortification.BarricadeAnchor>().ToList();
            var anchor = anchors.FirstOrDefault(a => !a.HasBarricade);
            if (anchor is null) { Log.Error("--- ub_qa_test_barricade --- No free BarricadeAnchor in scene."); return; }

            player.WorldPosition = anchor.WorldPosition;
            inv.TryAdd("reinforced_barricade_kit", 1);

            var req = new InteractionRequest { Identity = PlayerIdentity.FromGameObject(player), InteractorObject = player };
            Log.Info($"--- ub_qa_test_barricade --- anchor={anchor.GameObject.Name} HasBarricadeAntes={anchor.HasBarricade} CanInteract={anchor.CanInteract(req)}");

            anchor.OnInteract(req);
            Log.Info($"--- ub_qa_test_barricade --- Tras colocar: HasBarricade={anchor.HasBarricade}");
            if (!anchor.HasBarricade) return;

            var barricade = anchor.BarricadeReference;
            Log.Info($"--- ub_qa_test_barricade --- Health inicial: {barricade.Health}/{barricade.MaxHealth}");

            barricade.TakeDamage(new DamageEvent { Amount = 60f, Position = barricade.WorldPosition });
            Log.Info($"--- ub_qa_test_barricade --- Tras 60 de daño: {barricade.Health}/{barricade.MaxHealth}");

            barricade.Repair(10f);
            Log.Info($"--- ub_qa_test_barricade --- Tras reparar 10: {barricade.Health}/{barricade.MaxHealth}");

            barricade.TakeDamage(new DamageEvent { Amount = 500f, Position = barricade.WorldPosition });
            Log.Info($"--- ub_qa_test_barricade --- Tras daño letal: destruida={barricade.IsDestroyed} anchorLibre={!anchor.HasBarricade}");
        }

        /// <summary>
        /// Diagnóstico: fuerza HudState.MissionJournal para confirmar que
        /// MissionJournalPanel.razor carga y renderiza de verdad (CARGA/VISIBLE) —
        /// escrito a ciegas mientras el editor no respondía. No sustituye pulsar J
        /// físicamente (eso prueba INTERACTUABLE), solo aísla si el Razor en sí
        /// tiene un fallo de sintaxis/enlace que dotnet build no detecta.
        /// </summary>
        [ConCmd("ub_qa_test_missionjournal")]
        public static void TestMissionJournal()
        {
            var hud = Game.ActiveScene.GetAllComponents<PlayerHud>().FirstOrDefault();
            if (hud is null) { Log.Error("--- ub_qa_test_missionjournal --- No PlayerHud found."); return; }

            Log.Info($"--- ub_qa_test_missionjournal --- Estado antes: {hud.CurrentState}");
            hud.ChangeState(HudState.MissionJournal);
            Log.Info($"--- ub_qa_test_missionjournal --- Estado tras ChangeState: {hud.CurrentState}");

            var journal = Missions.MissionJournal.Local;
            Log.Info($"--- ub_qa_test_missionjournal --- MissionJournal.Local presente: {journal != null} misionesActivas: {journal?.ActiveMissions.Count ?? -1}");
        }

        /// <summary>
        /// Coloca una barricada real en el anchor indicado y la deja intacta
        /// (a diferencia de ub_qa_test_barricade, que la destruye al final para
        /// probar el ciclo completo). Para preparar estado real que verificar
        /// tras un Stop/Play.
        /// </summary>
        [ConCmd("ub_qa_place_barricade")]
        public static void PlaceBarricadeKeepAlive(string anchorName = "")
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_place_barricade --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            var anchor = Game.ActiveScene.GetAllComponents<Fortification.BarricadeAnchor>()
                .FirstOrDefault(a => !a.HasBarricade && (string.IsNullOrEmpty(anchorName) || a.GameObject.Name.Contains(anchorName)));

            if (anchor is null) { Log.Error("--- ub_qa_place_barricade --- No free anchor matching."); return; }

            player.WorldPosition = anchor.WorldPosition;
            inv?.TryAdd("barricade", 1);

            var req = new InteractionRequest { Identity = PlayerIdentity.FromGameObject(player), InteractorObject = player };
            anchor.OnInteract(req);

            Log.Info($"--- ub_qa_place_barricade --- anchor={anchor.GameObject.Name} HasBarricade={anchor.HasBarricade} Health={anchor.BarricadeReference?.Health}/{anchor.BarricadeReference?.MaxHealth}");
        }

        /// <summary>
        /// Diagnóstico de persistencia: vuelca el estado completo del jugador y de
        /// su vivienda. Se ejecuta una vez antes de Stop y otra vez después de
        /// Play — la consola de la sesión persiste entre reinicios de Play (no del
        /// editor), así que ambas líneas quedan una junto a otra para comparar a
        /// mano. No mide nada por sí solo: es una fotografía, la prueba real es
        /// Stop→Play entre dos llamadas y diferenciar el texto.
        /// </summary>
        [ConCmd("ub_qa_snapshot_persistence")]
        public static void SnapshotPersistence()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_snapshot_persistence --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            var wallet = player.Components.Get<Economy.Wallet>();

            var apt = Game.ActiveScene.GetAllComponents<Apartments.ApartmentComponent>()
                .FirstOrDefault(a => a.OwnerId == PlayerIdentity.FromGameObject(player).CanonicalId);

            Log.Info("=== SNAPSHOT PERSISTENCIA ===");
            Log.Info($"Inventario: chatarra={inv?.GetCount("chatarra")} wood={inv?.GetCount("wood")} scrap_metal={inv?.GetCount("scrap_metal")} components={inv?.GetCount("components")} apartment_door_kit={inv?.GetCount("apartment_door_kit")}");
            Log.Info($"Wallet: {wallet?.Balance}");

            if (apt is null)
            {
                Log.Info("Apartamento propio: ninguno.");
                return;
            }

            var fort = apt.GameObject.Components.Get<Fortification.ApartmentFortification>();
            Log.Info($"Apartamento propio: {apt.ApartmentId} ClaimState={apt.ClaimState}");
            Log.Info($"Fortificación: UpgradeLevel={fort?.UpgradeLevel} DoorHealth={fort?.DoorStructure?.Health}/{fort?.DoorStructure?.MaxHealth}");

            var anchors = apt.GameObject.Components.GetAll<Fortification.BarricadeAnchor>(FindMode.EverythingInSelfAndDescendants).ToList();
            foreach (var anchor in anchors)
            {
                if (anchor.HasBarricade)
                    Log.Info($"Barricada {anchor.AnchorId}: Health={anchor.BarricadeReference.Health}/{anchor.BarricadeReference.MaxHealth}");
                else
                    Log.Info($"Barricada {anchor.AnchorId}: ninguna colocada.");
            }
        }

        /// <summary>
        /// Activa/desactiva IA y raids para pruebas aisladas de Fase 13. Solo cambia
        /// el flag en memoria del proceso actual — no persiste, no toca el default
        /// de FeatureFlags.cs.
        /// </summary>
        [ConCmd("ub_qa_toggle_ai")]
        public static void ToggleAi(bool enableAi = true, bool enableRaids = true)
        {
            Core.FeatureFlags.EnableAI = enableAi;
            Core.FeatureFlags.EnableRaids = enableRaids;
            Log.Info($"--- ub_qa_toggle_ai --- EnableAI={Core.FeatureFlags.EnableAI} EnableRaids={Core.FeatureFlags.EnableRaids}");
        }

        /// <summary>
        /// Recorrido de habitáculo abandonado: fabricar->instalar puerta->instalar
        /// armario->claim atómico, vía los gateways reales (DoorAnchor.OnInteract,
        /// ClaimCabinetAnchor.OnInteract, PropertyClaimService.TryClaimAbandonedShell).
        /// </summary>
        [ConCmd("ub_qa_test_property_claim")]
        public static void TestPropertyClaim(string propertyId = "property-shell-01")
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_test_property_claim --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            inv?.TryAdd("apartment_door_kit", 1);
            inv?.TryAdd("claim_cabinet", 1);

            var doorAnchor = Game.ActiveScene.GetAllComponents<DoorAnchor>().FirstOrDefault(a => a.PropertyId == propertyId);
            var cabinetAnchor = Game.ActiveScene.GetAllComponents<ClaimCabinetAnchor>().FirstOrDefault(a => a.PropertyId == propertyId);
            var property = Game.ActiveScene.GetAllComponents<PropertyComponent>().FirstOrDefault(p => p.PropertyId == propertyId);
            if (doorAnchor is null || cabinetAnchor is null || property is null)
            {
                Log.Error("--- ub_qa_test_property_claim --- Fixture anchors/property not found.");
                return;
            }

            var req = new InteractionRequest { Identity = PlayerIdentity.FromGameObject(player), InteractorObject = player };

            player.WorldPosition = doorAnchor.WorldPosition;
            doorAnchor.OnInteract(req);
            Log.Info($"--- ub_qa_test_property_claim --- Puerta instalada: {doorAnchor.HasDoor}");

            player.WorldPosition = cabinetAnchor.WorldPosition;
            cabinetAnchor.OnInteract(req);
            Log.Info($"--- ub_qa_test_property_claim --- Armario instalado: {cabinetAnchor.HasCabinet}");

            player.WorldPosition = property.RespawnAnchor.IsValid() ? property.RespawnAnchor.WorldPosition : property.WorldPosition;

            var claimService = Game.ActiveScene.GetAllComponents<PropertyClaimService>().FirstOrDefault();
            if (claimService is null) { Log.Error("--- ub_qa_test_property_claim --- No PropertyClaimService found."); return; }

            var connLocal = Connection.Local;
            var resolvedPlayer = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault(p => p.GameObject.Network.OwnerId == connLocal.Id);
            Log.Info($"--- ub_qa_test_property_claim --- DIAG qaPlayer.Pos={player.WorldPosition} qaPlayer.Id={player.Id} connLocal.Id={connLocal?.Id} resolvedPlayer={resolvedPlayer?.GameObject.Id} resolvedPlayer.Pos={resolvedPlayer?.WorldPosition} property.Pos={property.WorldPosition}");

            var result = claimService.TryClaimAbandonedShell(Connection.Local, propertyId);
            Log.Info($"--- ub_qa_test_property_claim --- Claim: Succeeded={result.Succeeded} Failure={result.Failure} Message={result.Message}");
            Log.Info($"--- ub_qa_test_property_claim --- Propiedad: ClaimState={property.ClaimState} Owner={property.OwnerPersistentId}");

            var door = doorAnchor.DoorReference;
            Log.Info($"--- ub_qa_test_property_claim --- Puerta tras claim: IsLocked={door?.IsLocked} LockId={door?.LockId} KeyRevision={door?.KeyRevision}");
            Log.Info($"--- ub_qa_test_property_claim --- BuildVolume habilitado: {property.BuildVolume?.Enabled} Stash habilitado: {property.Stash?.Enabled}");
        }

        /// <summary>
        /// Recorrido de credenciales sobre el habitáculo ya reclamado por
        /// ub_qa_test_property_claim: otorgar, aislar el camino de la credencial
        /// (limpiando el match directo de owner), revocar, y cambio de cerradura
        /// invalidando la credencial vieja. Todo vía los gateways reales
        /// (KeyringService RPCs, PropertyDoor.OnInteract, KeyringItem.HasAccess).
        /// </summary>
        [ConCmd("ub_qa_test_property_credential")]
        public static void TestPropertyCredential(string propertyId = "property-shell-01")
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_test_property_credential --- No player found."); return; }

            var property = Game.ActiveScene.GetAllComponents<PropertyComponent>().FirstOrDefault(p => p.PropertyId == propertyId);
            var door = Game.ActiveScene.GetAllComponents<PropertyDoor>().FirstOrDefault(d => d.PropertyId == propertyId);
            var keyringService = Game.ActiveScene.GetAllComponents<KeyringService>().FirstOrDefault();
            var keyring = player.Components.Get<KeyringItem>() ?? player.Components.GetOrCreate<KeyringItem>();
            if (property is null || door is null || keyringService is null)
            {
                Log.Error("--- ub_qa_test_property_credential --- Fixture/service not found.");
                return;
            }

            var identity = PlayerIdentity.FromGameObject(player);
            if (property.OwnerPersistentId != identity.CanonicalId)
            {
                Log.Error($"--- ub_qa_test_property_credential --- El jugador no es owner de {propertyId} (ejecuta ub_qa_test_property_claim primero).");
                return;
            }

            var req = new InteractionRequest { Identity = identity, InteractorObject = player };
            var ownerId = property.OwnerPersistentId;

            // KeyringItem.HasAccess exige llevar el ítem físico "keyring" en el inventario
            // (mismo patrón que Wallet) -- sin él, ninguna credencial sirve aunque exista.
            var inv = player.Components.Get<InventoryComponent>();
            inv?.TryAdd("keyring", 1);

            keyringService.RequestGrantAccess(player, propertyId, AccessLevel.Guest, 0f);
            Log.Info($"--- ub_qa_test_property_credential --- Credencial otorgada: {keyring.FindCredential(propertyId) != null}");

            // Aislar el camino de la credencial: quitar el match directo de owner
            // para confirmar que la credencial por sí sola abre la puerta.
            property.ApplyOwnership(string.Empty, PropertyClaimState.Claimed);
            door.SetLocked(true);
            door.OnInteract(req);
            Log.Info($"--- ub_qa_test_property_credential --- Solo credencial (sin match directo): IsLocked={door.IsLocked} (esperado false)");

            // Sin credencial ni match directo: debe denegar.
            property.ApplyOwnership(string.Empty, PropertyClaimState.Claimed);
            keyring.Revoke(propertyId);
            door.SetLocked(true);
            door.OnInteract(req);
            Log.Info($"--- ub_qa_test_property_credential --- Sin credencial ni match directo: IsLocked={door.IsLocked} (esperado true)");

            // Restaurar ownership para poder ejercer las RPCs de owner (grant/rekey).
            property.ApplyOwnership(ownerId, PropertyClaimState.Claimed);

            keyringService.RequestGrantAccess(player, propertyId, AccessLevel.Guest, 0f);
            var oldLockId = door.LockId;
            var oldKeyRevision = door.KeyRevision;
            Log.Info($"--- ub_qa_test_property_credential --- Credencial re-otorgada, cerradura actual: LockId={oldLockId} KeyRevision={oldKeyRevision}");

            keyringService.RequestRekeyDoor(propertyId);
            Log.Info($"--- ub_qa_test_property_credential --- Tras rekey: LockId={door.LockId} KeyRevision={door.KeyRevision} (debe cambiar)");

            // Aislar de nuevo el camino de la credencial para confirmar que la vieja ya no sirve
            // (PruneStaleRevisions ya la habrá quitado del llavero — RequestRekeyDoor lo hace solo).
            property.ApplyOwnership(string.Empty, PropertyClaimState.Claimed);
            door.SetLocked(true);
            door.OnInteract(req);
            Log.Info($"--- ub_qa_test_property_credential --- Credencial vieja tras rekey (sin match directo): IsLocked={door.IsLocked} (esperado true, la RequestRekeyDoor ya podó la credencial vieja)");

            property.ApplyOwnership(ownerId, PropertyClaimState.Claimed);
            Log.Info($"--- ub_qa_test_property_credential --- Estado final restaurado: Owner={property.OwnerPersistentId}");
        }

        [ConCmd("ub_qa_test_property_rent")]
        public static void TestPropertyRent(string propertyId = "property-rental-01")
        {
            var claimService = Game.ActiveScene.GetAllComponents<PropertyClaimService>().FirstOrDefault();
            if (claimService is null) { Log.Error("--- ub_qa_test_property_rent --- No PropertyClaimService found."); return; }

            var result = claimService.TryRentProperty(Connection.Local, propertyId);
            Log.Info($"--- ub_qa_test_property_rent --- Rent: Succeeded={result.Succeeded} Failure={result.Failure} Message={result.Message}");
        }

        [ConCmd("ub_qa_test_property_abandon_rental")]
        public static void TestPropertyAbandonRental(string propertyId = "property-rental-01")
        {
            var rentalService = Game.ActiveScene.GetAllComponents<RentalService>().FirstOrDefault();
            if (rentalService is null) { Log.Error("--- ub_qa_test_property_abandon_rental --- No RentalService found."); return; }
            rentalService.RequestAbandonRental(propertyId);
        }

        /// <summary>
        /// Fuerza el guardado ya mismo llamando a ApartmentClaimService.TrySaveNow() --
        /// el mismo método que AutoSaveManager ya llama cada 90s o al recibir un
        /// PersistenceBridge.RequestSave real. Solo evita esperar el intervalo
        /// completo en las pruebas; no fabrica el resultado del guardado.
        /// </summary>
        [ConCmd("ub_qa_force_save")]
        public static void ForceSave()
        {
            var claimService = Game.ActiveScene.GetAllComponents<ApartmentClaimService>().FirstOrDefault();
            if (claimService is null) { Log.Error("--- ub_qa_force_save --- No ApartmentClaimService found."); return; }

            var result = claimService.TrySaveNow();
            Log.Info($"--- ub_qa_force_save --- Succeeded={result.Succeeded} Error={result.Error}");
        }

        /// <summary>
        /// Llama a RequestAbandonApartment() -- la RPC de producción real, sin
        /// UI todavía (no aporta atajo: es exactamente el método que un botón de
        /// "renunciar" llamaría). Existe para poder liberar una vivienda de
        /// prueba y volver a ejercitar el flujo de reclamo completo en la misma
        /// sesión de Play, sin tocar OwnerId/ClaimState directamente.
        /// </summary>
        [ConCmd("ub_qa_request_abandon")]
        public static void RequestAbandon()
        {
            var claimService = Game.ActiveScene.GetAllComponents<ApartmentClaimService>().FirstOrDefault();
            if (claimService is null) { Log.Error("--- ub_qa_request_abandon --- No ApartmentClaimService found."); return; }
            claimService.RequestAbandonApartment();
        }

        [ConCmd("ub_qa_snapshot_properties")]
        public static void SnapshotProperties()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            var keyring = player?.Components.Get<KeyringItem>();

            Log.Info("=== SNAPSHOT PROPERTIES ===");
            foreach (var property in Game.ActiveScene.GetAllComponents<PropertyComponent>().OrderBy(p => p.PropertyId))
            {
                Log.Info($"{property.PropertyId}: Type={property.PropertyType} ClaimState={property.ClaimState} Owner={property.OwnerPersistentId} Tenant={property.TenantPersistentId} RentalState={property.RentalState}");

                var doorAnchor = Game.ActiveScene.GetAllComponents<DoorAnchor>().FirstOrDefault(a => a.PropertyId == property.PropertyId);
                if (doorAnchor != null)
                {
                    var d = doorAnchor.DoorReference;
                    Log.Info($"  Puerta {doorAnchor.AnchorId}: HasDoor={doorAnchor.HasDoor} IsLocked={d?.IsLocked} LockId={d?.LockId} KeyRevision={d?.KeyRevision} Health={d?.Health}/{d?.MaxHealth}");
                }

                var cabinetAnchor = Game.ActiveScene.GetAllComponents<ClaimCabinetAnchor>().FirstOrDefault(a => a.PropertyId == property.PropertyId);
                if (cabinetAnchor != null)
                    Log.Info($"  Armario: HasCabinet={cabinetAnchor.HasCabinet}");
            }

            Log.Info($"Llavero del jugador: {keyring?.Credentials.Count ?? 0} credenciales");
            foreach (var c in keyring?.Credentials ?? Enumerable.Empty<AccessCredential>())
                Log.Info($"  Credencial: Property={c.PropertyId} Level={c.AccessLevel} LockId={c.LockId} KeyRevision={c.KeyRevision}");
        }

        /// <summary>
        /// Sitúa al jugador junto al pickup real (por ItemId) y lo orienta para mirarlo,
        /// luego dispara PlayerInteractor.DebugForceUseAttempt() -- el mismo trace +
        /// resolución de IWorldInteractable + CanInteract + OnInteract que ejecuta un E
        /// real, sustituyendo únicamente el evento de teclado (imposible de simular
        /// desde MCP). No llama a AddItem/OnInteract directamente.
        /// </summary>
        [ConCmd("ub_qa_physical_pickup_test")]
        public static void PhysicalPickupTest(string itemId)
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_physical_pickup_test --- No player found."); return; }

            var pickup = Game.ActiveScene.GetAllComponents<WorldItemPickup>().FirstOrDefault(p => p.ItemId == itemId);
            if (pickup is null) { Log.Error($"--- ub_qa_physical_pickup_test --- No WorldItemPickup with ItemId={itemId} found."); return; }

            var interactor = player.Components.Get<PlayerInteractor>();
            if (interactor is null) { Log.Error("--- ub_qa_physical_pickup_test --- Player has no PlayerInteractor."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            var before = inv?.GetCount(itemId) ?? -1;

            // Colocar al jugador a 80u del pickup y mirarlo -- equivale a "acercarse y mirar madera".
            // ProcessInteraction usa PlayerController.EyeAngles.Forward (no WorldRotation) cuando
            // hay un PlayerController -- si solo se fija WorldRotation, el trace sigue apuntando
            // hacia donde miraba antes (bug real de esta prueba detectado al primer intento: el
            // trace falló con hit=False porque EyeAngles no se había tocado).
            var toPickup = (pickup.WorldPosition - player.WorldPosition).Normal;
            player.WorldPosition = pickup.WorldPosition - toPickup * 80f;
            // El trace parte de WorldPosition + Up*64 (altura de ojos), no de la base del jugador --
            // mirar desde la base subestimaba el pitch y el trace pasaba por encima del pickup.
            var eyePos = player.WorldPosition + Vector3.Up * 64f;
            var lookDir = (pickup.WorldPosition - eyePos).Normal;
            var controller = player.Components.Get<Sandbox.PlayerController>();
            if (controller != null)
                controller.EyeAngles = Rotation.LookAt(lookDir).Angles();
            else
                player.WorldRotation = Rotation.LookAt(lookDir);

            Log.Info($"--- ub_qa_physical_pickup_test --- item={itemId} playerPos={player.WorldPosition} pickupPos={pickup.WorldPosition} Before={itemId}:{before}");

            interactor.DebugForceUseAttempt();

            var after = inv?.GetCount(itemId) ?? -1;
            var pickupStillExists = Game.ActiveScene.GetAllComponents<WorldItemPickup>().Any(p => p == pickup);
            Log.Info($"--- ub_qa_physical_pickup_test --- After={itemId}:{after} PickupDestroyed={!pickupStillExists}");
        }

        /// <summary>
        /// Selecciona el slot de hotbar del ítem (mismo gateway público que
        /// HeldItemController.SelectSlot llama al pulsar Slot1-6) y consume vía
        /// UseActiveConsumable() -- el mismo método que dispara Input.Pressed("attack1")
        /// en producción. No fabrica el consumo llamando a inventory.TryRemove directo.
        /// </summary>
        [ConCmd("ub_qa_physical_consume_test")]
        public static void PhysicalConsumeTest(string itemId)
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_physical_consume_test --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            var held = player.Components.Get<HeldItemController>();
            if (inv is null || held is null) { Log.Error("--- ub_qa_physical_consume_test --- Missing InventoryComponent/HeldItemController."); return; }

            var slotIndex = inv.Slots.ToList().FindIndex(s => s.ItemId == itemId);
            if (slotIndex < 0) { Log.Error($"--- ub_qa_physical_consume_test --- {itemId} not found in inventory."); return; }

            var before = inv.GetCount(itemId);
            held.SelectSlot(slotIndex);
            Log.Info($"--- ub_qa_physical_consume_test --- item={itemId} slot={slotIndex} ActiveItemId={held.ActiveItemId} Before={itemId}:{before}");

            held.UseActiveConsumable();

            var after = inv.GetCount(itemId);
            Log.Info($"--- ub_qa_physical_consume_test --- After={itemId}:{after} Consumed={after < before}");
        }

        /// <summary>
        /// Selecciona el slot y llama a DropActiveItem() -- el mismo método que
        /// dispara Input.Pressed("Drop") en producción.
        /// </summary>
        [ConCmd("ub_qa_physical_drop_test")]
        public static void PhysicalDropTest(string itemId)
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_physical_drop_test --- No player found."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            var held = player.Components.Get<HeldItemController>();
            if (inv is null || held is null) { Log.Error("--- ub_qa_physical_drop_test --- Missing InventoryComponent/HeldItemController."); return; }

            var slotIndex = inv.Slots.ToList().FindIndex(s => s.ItemId == itemId);
            if (slotIndex < 0) { Log.Error($"--- ub_qa_physical_drop_test --- {itemId} not found in inventory."); return; }

            var before = inv.GetCount(itemId);
            held.SelectSlot(slotIndex);
            Log.Info($"--- ub_qa_physical_drop_test --- item={itemId} slot={slotIndex} Before={itemId}:{before}");

            var worldPickupsBefore = Game.ActiveScene.GetAllComponents<WorldItemPickup>().Count(p => p.ItemId == itemId);
            held.DropActiveItem();

            var after = inv.GetCount(itemId);
            var worldPickupsAfter = Game.ActiveScene.GetAllComponents<WorldItemPickup>().Count(p => p.ItemId == itemId);
            Log.Info($"--- ub_qa_physical_drop_test --- After={itemId}:{after} WorldPickups {worldPickupsBefore}->{worldPickupsAfter}");
        }

        /// <summary>
        /// Generaliza ub_qa_physical_pickup_test a CUALQUIER IWorldInteractable por
        /// nombre de GameObject (puerta, armario, estación de crafteo, ...), no solo
        /// pickups -- así no hace falta un DebugForceX distinto por sistema. Posiciona
        /// y orienta al jugador (equivalente a "acercarse y mirar") y dispara
        /// PlayerInteractor.DebugForceUseAttempt(), el mismo trace+CanInteract+OnInteract
        /// real que un E físico.
        /// </summary>
        [ConCmd("ub_qa_physical_interact")]
        public static void PhysicalInteract(string targetName)
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_physical_interact --- No player found."); return; }

            GameObject target = null;
            if (System.Guid.TryParse(targetName, out var targetGuid))
                target = Game.ActiveScene.Directory.FindByGuid(targetGuid);

            target ??= Game.ActiveScene.GetAllComponents<Component>()
                .Select(c => c.GameObject)
                .FirstOrDefault(go => go.Name.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));

            if (target is null) { Log.Error($"--- ub_qa_physical_interact --- No GameObject matching '{targetName}' found."); return; }

            var interactor = player.Components.Get<PlayerInteractor>();
            if (interactor is null) { Log.Error("--- ub_qa_physical_interact --- Player has no PlayerInteractor."); return; }

            // Solo se usa el plano horizontal para calcular el punto de aproximación --
            // usar la dirección 3D completa (incluyendo Z) puede dejar al jugador muy
            // por encima o por debajo del objetivo si la posición previa del jugador
            // tenía una altura muy distinta, lo que inclina la cámara hacia el suelo
            // y el trace impacta el terreno antes de llegar al objetivo real.
            var toTargetFlat = (target.WorldPosition - player.WorldPosition).WithZ(0).Normal;
            var approach = target.WorldPosition - toTargetFlat * 80f;
            player.WorldPosition = approach.WithZ(target.WorldPosition.z);

            var eyePos = player.WorldPosition + Vector3.Up * 64f;
            var lookDir = (target.WorldPosition - eyePos).Normal;
            var controller = player.Components.Get<Sandbox.PlayerController>();
            if (controller != null)
                controller.EyeAngles = Rotation.LookAt(lookDir).Angles();
            else
                player.WorldRotation = Rotation.LookAt(lookDir);

            Log.Info($"--- ub_qa_physical_interact --- target={targetName} playerPos={player.WorldPosition} targetPos={target.WorldPosition}");

            interactor.DebugForceUseAttempt();
        }

        /// <summary>
        /// Llama a CraftingStation.RequestCraft() -- el mismo método exacto que el
        /// botón "Fabricar" de CraftingPanel invoca en producción (CraftingPanel.cs
        /// línea ~127). No fabrica el resultado: la propia estación valida
        /// ingredientes, reserva, consume y entrega de forma atómica.
        /// </summary>
        [ConCmd("ub_qa_physical_craft")]
        public static void PhysicalCraft(string recipeId)
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player is null) { Log.Error("--- ub_qa_physical_craft --- No player found."); return; }

            var station = Game.ActiveScene.GetAllComponents<Crafting.CraftingStation>().FirstOrDefault();
            if (station is null) { Log.Error("--- ub_qa_physical_craft --- No CraftingStation found."); return; }

            var recipe = station.GetRecipe(recipeId);
            if (recipe is null) { Log.Error($"--- ub_qa_physical_craft --- Recipe {recipeId} not on this station."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            var before = inv?.GetCount(recipe.Result.ItemId) ?? -1;
            var ingredientsBefore = string.Join(", ", recipe.Ingredients.Select(i => $"{i.ItemId}:{inv?.GetCount(i.ItemId)}"));
            Log.Info($"--- ub_qa_physical_craft --- recipe={recipeId} Before result={recipe.Result.ItemId}:{before} ingredients=[{ingredientsBefore}]");

            station.RequestCraft(player.Id, recipeId);

            var after = inv?.GetCount(recipe.Result.ItemId) ?? -1;
            var ingredientsAfter = string.Join(", ", recipe.Ingredients.Select(i => $"{i.ItemId}:{inv?.GetCount(i.ItemId)}"));
            Log.Info($"--- ub_qa_physical_craft --- After result={recipe.Result.ItemId}:{after} ingredients=[{ingredientsAfter}]");
        }
    }
}
