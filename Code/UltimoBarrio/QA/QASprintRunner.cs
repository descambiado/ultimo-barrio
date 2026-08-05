using Sandbox;
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
            var mod = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault();
            var cam = Game.ActiveScene.GetAllComponents<PlayerCameraEffects>().FirstOrDefault();
            if (mod == null || cam == null) { Log.Error("Movement/Camera not found."); return; }

            Log.Info("--- ub_qa_movement_state ---");
            Log.Info($"stamina actual: {mod.CurrentStamina}");
            Log.Info($"IsExhausted: {mod.IsExhausted}");
            Log.Info($"camera local position: {cam.GameObject.LocalPosition}");
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
    }
}
