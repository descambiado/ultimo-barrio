using Sandbox;
using Sandbox.UI;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Combat;
using UltimoBarrio.Inventory;
using UltimoBarrio.Players;
using UltimoBarrio.Economy;
using UltimoBarrio.Core;

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
    }
}
