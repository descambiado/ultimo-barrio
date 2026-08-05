using Sandbox;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Combat;
using UltimoBarrio.Inventory;
using UltimoBarrio.Players;
using UltimoBarrio.Economy;

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
            if (p.CurrentWeapon != null)
            {
                Log.Info($"CurrentWeapon: {p.CurrentWeapon.GameObject.Name}");
                Log.Info($"ActiveViewModel: {p.CurrentWeapon.Components.Get<Sandbox.SkinnedModelRenderer>(FindMode.EverythingInDescendants) != null}");
                Log.Info($"ActiveWorldModel: {p.CurrentWeapon.Components.Get<Sandbox.ModelRenderer>(FindMode.EverythingInDescendants) != null}");
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
            if (p == null || p.CurrentWeapon == null) { Log.Error("No HeldItemController or Weapon found."); return; }

            Log.Info("--- ub_qa_weapon_state ---");
            if (p.CurrentWeapon is FirearmWeapon fw)
            {
                Log.Info($"CurrentAmmo: {fw.CurrentAmmo}");
                Log.Info($"ReserveAmmo: {fw.ReserveAmmo}");
                Log.Info($"CurrentState: {(fw.IsReloading ? "Reloading" : "Idle/Firing")}");
            }
            else if (p.CurrentWeapon is MeleeWeapon mw)
            {
                Log.Info($"MeleeWeapon: {mw.GameObject.Name}");
                Log.Info($"Damage: {mw.BaseDamage}");
                Log.Info($"CurrentState: Idle/Attacking");
            }
        }

        [ConCmd("ub_qa_spawn_dummy")]
        public static void SpawnDummy()
        {
            var player = Game.ActiveScene.GetAllComponents<PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            if (player == null) { Log.Error("Player not found"); return; }

            var dummyGo = new GameObject(true, "QA_CombatDummy");
            dummyGo.Transform.Position = player.Transform.Position + player.Transform.Rotation.Forward * 100f + Vector3.Up * 10f;
            
            var model = dummyGo.Components.Create<ModelRenderer>();
            model.Model = Model.Load("models/citizen/citizen.vmdl");
            
            var collider = dummyGo.Components.Create<BoxCollider>();
            collider.Scale = new Vector3(20, 20, 72);
            collider.Center = new Vector3(0, 0, 36);

            var hc = dummyGo.Components.Create<HealthComponent>();
            hc.MaxHealth = 100f;
            hc.CurrentHealth = 100f;

            Log.Info($"Dummy spawned at {dummyGo.Transform.Position} with {hc.CurrentHealth} HP.");
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
                Log.Info($"Dummy HP: {hc.CurrentHealth} / {hc.MaxHealth}");
            }
        }

        [ConCmd("ub_qa_stash_state")]
        public static void CheckStashState()
        {
            var interactor = Game.ActiveScene.GetAllComponents<PlayerInteractor>().FirstOrDefault();
            if (interactor == null) { Log.Error("PlayerInteractor not found."); return; }

            var tr = Game.ActiveScene.Trace.Ray(interactor.GameObject.Transform.Position + Vector3.Up * 64f, interactor.GameObject.Transform.Position + Vector3.Up * 64f + interactor.GameObject.Transform.Rotation.Forward * 150f).Run();
            
            Log.Info("--- ub_qa_stash_state ---");
            if (tr.Hit && tr.GameObject != null)
            {
                var stash = tr.GameObject.Components.GetInAncestorsOrSelf<StashComponent>();
                if (stash != null)
                {
                    Log.Info($"CanonicalId: {PlayerIdentity.Local.CanonicalId}");
                    Log.Info($"OwnerId: {stash.Apartment.OwnerId}");
                    Log.Info($"CanInteract: {stash.CanInteract(interactor.GameObject)}");
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
            Log.Info($"crouch activo: {mod.IsCrouching}");
            Log.Info($"lean offset: {cam.CurrentLeanOffset}");
            Log.Info($"camera local position: {cam.GameObject.Transform.LocalPosition}");
        }
    }
}
