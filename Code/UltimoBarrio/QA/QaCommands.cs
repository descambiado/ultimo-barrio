using Sandbox;
using System.Linq;
using UltimoBarrio.Apartments;

using UltimoBarrio.Core;
using UltimoBarrio.WorldTime;
using UltimoBarrio.Economy;

namespace UltimoBarrio.QA
{
    public static class QaCommands
    {
        [ConCmd("ub_qa_reset_save")]
        public static void ResetQaSave()
        {
            if (!Networking.IsHost) return;
            Log.Info("[QA] Resetting QA save slot...");
            try 
            {
                if (FileSystem.Data.DirectoryExists("ultimo-barrio/saves/qa-slot"))
                {
                    FileSystem.Data.DeleteDirectory("ultimo-barrio/saves/qa-slot", true);
                    Log.Info("[QA] QA save slot cleared.");
                }
            } 
            catch (System.Exception e)
            {
                Log.Warning("[QA] Could not clear save slot: " + e.Message);
            }
        }

        [ConCmd("ub_qa_release_apartment")]
        public static void ReleaseApartment(string apartmentId)
        {
            if (!Networking.IsHost) return;
            var apt = Game.ActiveScene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == apartmentId);
            if (apt != null)
            {
                apt.ApplyState(null, ApartmentClaimState.Unclaimed);
                Log.Info($"[QA] Apartment {apartmentId} released.");
            }
        }

        [ConCmd("ub_qa_assign_apartment")]
        public static void AssignApartment(string apartmentId, string ownerId)
        {
            if (!Networking.IsHost) return;
            
            // Check 1 apartment per player rule
            var existing = Game.ActiveScene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.OwnerId == ownerId);
            if (existing != null)
            {
                Log.Warning($"[QA] Player {ownerId} already owns apartment {existing.ApartmentId}. Release it first.");
                return;
            }

            var apt = Game.ActiveScene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == apartmentId);
            if (apt != null && apt.ClaimState == ApartmentClaimState.Unclaimed)
            {
                apt.ApplyState(ownerId, ApartmentClaimState.Claimed);
                Log.Info($"[QA] Apartment {apartmentId} assigned to {ownerId}.");
            }
        }

        [ConCmd("ub_qa_clear_inventory")]
        public static void ClearInventory(string inventoryId)
        {
            if (!Networking.IsHost) return;
            var inv = Game.ActiveScene.GetAllComponents<InventoryComponent>().FirstOrDefault(i => i.InventoryId == inventoryId);
            if (inv != null)
            {
                inv.Slots.Clear();
                Log.Info($"[QA] Inventory {inventoryId} cleared.");
            }
        }

        [ConCmd("ub_qa_give_scrap")]
        public static void GiveScrap(string inventoryId, int amount)
        {
            if (!Networking.IsHost) return;
            var inv = Game.ActiveScene.GetAllComponents<InventoryComponent>().FirstOrDefault(i => i.InventoryId == inventoryId);
            if (inv != null)
            {
                var existing = inv.Slots.FirstOrDefault(s => s.ItemId == "chatarra");
                if (existing != null)
                {
                    existing.Amount += amount;
                }
                else
                {
                    inv.Slots.Add(new InventorySlot { ItemId = "chatarra", Amount = amount });
                }
                Log.Info($"[QA] Added {amount} chatarra to {inventoryId}.");
            }
        }

        /// <summary>
        /// Añade un ítem cualquiera al jugador local -- solo para desbloquear
        /// pruebas cuando la única fuente real de un recurso en el mundo se
        /// agota (p.ej. "Wood Pickup" en la Starter Resource Zone es un
        /// WorldItemPickup de un solo uso, sin ResourceNode/respawn, y ya se
        /// recolectó esta sesión). No sustituye a TryCraft/TryClaim/etc. --
        /// solo llena el inventario, exactamente como ub_qa_give_scrap ya
        /// hacía para chatarra.
        /// </summary>
        [ConCmd("ub_qa_give_item")]
        public static void GiveItem(string itemId, int amount)
        {
            if (!Networking.IsHost) return;
            var player = Game.ActiveScene.GetAllComponents<Players.PlayerMovementModifier>().FirstOrDefault()?.GameObject;
            var inv = player?.Components.Get<InventoryComponent>();
            if (inv is null) { Log.Error("--- ub_qa_give_item --- No player inventory found."); return; }
            inv.TryAdd(itemId, amount);
            Log.Info($"[QA] Added {amount} {itemId} to local player.");
        }

        [ConCmd("ub_qa_give_money")]
        public static void GiveMoney(int amount)
        {
            if (!Networking.IsHost) return;
            var comp = Game.ActiveScene.GetAllComponents<Wallet>().FirstOrDefault();
            if (comp != null)
            {
                comp.AddFunds(amount);
                Log.Info($"[QA] Added {amount} money to first wallet found.");
            }
        }

        [ConCmd("ub_qa_force_day")]
        public static void ForceDay()
        {
            if (!Networking.IsHost) return;
            var clock = Game.ActiveScene.GetAllComponents<WorldClock>().FirstOrDefault();
            if (clock != null)
            {
                clock.ForcePhase(TimePhase.Day);
                Log.Info("[QA] Forcing Day Phase...");
            }
        }

        [ConCmd("ub_qa_force_preparation")]
        public static void ForcePreparation()
        {
            if (!Networking.IsHost) return;
            var clock = Game.ActiveScene.GetAllComponents<WorldClock>().FirstOrDefault();
            if (clock != null)
            {
                clock.ForcePhase(TimePhase.Preparation);
                Log.Info("[QA] Forcing Preparation Phase...");
            }
        }

        [ConCmd("ub_qa_force_night")]
        public static void ForceNight()
        {
            if (!Networking.IsHost) return;
            var clock = Game.ActiveScene.GetAllComponents<WorldClock>().FirstOrDefault();
            if (clock != null)
            {
                clock.ForcePhase(TimePhase.Night);
                Log.Info("[QA] Forcing Night Phase...");
            }
        }

        [ConCmd("ub_qa_spawn_raider")]
        public static void SpawnRaider()
        {
            if (!Networking.IsHost) return;
            Log.Info("[QA] Spawning Raider...");
        }

        [ConCmd("ub_qa_assign_me")]
        public static void AssignToMe(string apartmentId)
        {
            if (!Networking.IsHost) return;
            var player = Game.ActiveScene.GetAllComponents<Sandbox.PlayerController>().FirstOrDefault()?.GameObject;
            if (player == null) return;
            var provider = Game.ActiveScene.GetAllComponents<IPlayerIdentityProvider>().FirstOrDefault();
            if (provider == null || !provider.TryResolve(player.Network.Owner, out var id)) return;

            var apt = Game.ActiveScene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == apartmentId);
            if (apt != null)
            {
                apt.ApplyState(id.CanonicalId, ApartmentClaimState.Claimed);
                Log.Info($"[QA] Apartment {apartmentId} assigned to REAL ID {id.CanonicalId}.");
            }
        }

        [ConCmd("ub_qa_test_stash")]
        public static void TestStash(string apartmentId)
        {
            if (!Networking.IsHost) return;
            var player = Game.ActiveScene.GetAllComponents<Sandbox.PlayerController>().FirstOrDefault()?.GameObject;
            if (player == null) return;
            
            var provider = Game.ActiveScene.GetAllComponents<IPlayerIdentityProvider>().FirstOrDefault();
            if (provider == null || !provider.TryResolve(player.Network.Owner, out var id)) return;

            var apt = Game.ActiveScene.GetAllComponents<ApartmentComponent>().FirstOrDefault(a => a.ApartmentId == apartmentId);
            var stash = Game.ActiveScene.GetAllComponents<UltimoBarrio.StashComponent>().FirstOrDefault(s => s.ApartmentId == apartmentId);
            if (apt == null || stash == null) return;

            var req = new InteractionRequest { Identity = id, InteractorObject = player };
            bool canInteract = stash.CanInteract(req);
            
            var inv = stash.Components.Get<UltimoBarrio.InventoryComponent>();
            Log.Info($"[QA_TEST] Target: {apartmentId}");
            Log.Info($"[QA_TEST] CanonicalId: {id.CanonicalId}");
            Log.Info($"[QA_TEST] OwnerId persistido: {apt.OwnerId}");
            Log.Info($"[QA_TEST] InventoryId del stash: {inv?.InventoryId}");
            Log.Info($"[QA_TEST] IsOwner: {id.CanonicalId == apt.OwnerId}");
            Log.Info($"[QA_TEST] CanInteract: {canInteract}");

            if (canInteract)
            {
                stash.OnInteract(req); // Abre la UI de verdad
                Log.Info($"[QA_TEST] Opened Stash UI for {id.CanonicalId}");
            }
        }
    }
}
