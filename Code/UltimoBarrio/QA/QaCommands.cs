using Sandbox;
using System.Linq;
using UltimoBarrio.Apartments;
using UltimoBarrio.Inventory;
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
            if (!Game.IsServer) return;
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
            if (!Game.IsServer) return;
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
            if (!Game.IsServer) return;
            
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
            if (!Game.IsServer) return;
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
            if (!Game.IsServer) return;
            var inv = Game.ActiveScene.GetAllComponents<InventoryComponent>().FirstOrDefault(i => i.InventoryId == inventoryId);
            if (inv != null)
            {
                var existing = inv.Slots.FirstOrDefault(s => s.ItemId == "item_scrap");
                if (existing != null)
                {
                    existing.Amount += amount;
                }
                else
                {
                    inv.Slots.Add(new InventorySlot { ItemId = "item_scrap", Amount = amount });
                }
                Log.Info($"[QA] Added {amount} scrap to {inventoryId}.");
            }
        }

        [ConCmd("ub_qa_give_money")]
        public static void GiveMoney(string playerId, int amount)
        {
            if (!Game.IsServer) return;
            var comp = Game.ActiveScene.GetAllComponents<PlayerEconomyComponent>().FirstOrDefault(c => c.PlayerId == playerId);
            if (comp != null)
            {
                comp.Balance += amount;
                Log.Info($"[QA] Added {amount} money to player {playerId}.");
            }
        }

        [ConCmd("ub_qa_force_day")]
        public static void ForceDay()
        {
            if (!Game.IsServer) return;
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
            if (!Game.IsServer) return;
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
            if (!Game.IsServer) return;
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
            if (!Game.IsServer) return;
            Log.Info("[QA] Spawning Raider...");
        }
    }
}
