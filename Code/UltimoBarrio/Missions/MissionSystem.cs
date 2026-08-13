using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.Missions
{
    public enum ObjectiveType
    {
        ClaimApartment,
        CollectItem,
        StoreInStash,
        SellToTrader,
        BuyItem,
        ObtainWeapon,
        ReturnHome,
        SurviveNight
    }

    public struct MissionObjective
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public ObjectiveType Type { get; set; }
        public string TargetId { get; set; }
        public int TargetAmount { get; set; }
        public int CurrentProgress { get; set; }
        public bool IsCompleted { get; set; }
    }

    public struct MissionReward
    {
        public int Money { get; set; }
        public string ItemId { get; set; }
        public int ItemAmount { get; set; }
    }

    public sealed class MissionDefinition
    {
        public string MissionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<MissionObjective> Objectives { get; set; } = new();
        public MissionReward Reward { get; set; }
    }

    [Title("Mission Journal")]
    [Category("Último Barrio — Missions")]
    [Icon("assignment")]
    public sealed class MissionJournal : Component
    {
        public static MissionJournal Local { get; private set; }

        public List<MissionDefinition> ActiveMissions { get; private set; } = new();
        public List<string> CompletedMissionIds { get; private set; } = new();

        protected override void OnStart()
        {
            if (!IsProxy)
            {
                Local = this;
                InitFirstQuestChain();
            }
        }

        public void InitFirstQuestChain()
        {
            var alphaChain = new MissionDefinition
            {
                MissionId = "m_alpha_first_steps",
                Title = "Primeros Pasos en El Barrio",
                Description = "Establécete en el barrio, asegura recursos y prepárate para la noche.",
                Reward = new MissionReward { Money = 100, ItemId = "ammo", ItemAmount = 20 },
                Objectives = new List<MissionObjective>
                {
                    new MissionObjective { Id = "obj_1", Description = "1. Reclama una vivienda", Type = ObjectiveType.ClaimApartment, TargetAmount = 1 },
                    new MissionObjective { Id = "obj_2", Description = "2. Recoge 10 de chatarra", Type = ObjectiveType.CollectItem, TargetId = "chatarra", TargetAmount = 10 },
                    new MissionObjective { Id = "obj_3", Description = "3. Guarda 5 en el alijo", Type = ObjectiveType.StoreInStash, TargetId = "chatarra", TargetAmount = 5 },
                    new MissionObjective { Id = "obj_4", Description = "4. Vende 5 al comerciante", Type = ObjectiveType.SellToTrader, TargetId = "chatarra", TargetAmount = 5 },
                    new MissionObjective { Id = "obj_5", Description = "5. Compra munición", Type = ObjectiveType.BuyItem, TargetId = "ammo", TargetAmount = 1 },
                    new MissionObjective { Id = "obj_6", Description = "6. Consigue una pistola", Type = ObjectiveType.ObtainWeapon, TargetId = "weapon_usp", TargetAmount = 1 },
                    new MissionObjective { Id = "obj_7", Description = "7. Regresa a casa", Type = ObjectiveType.ReturnHome, TargetAmount = 1 },
                    new MissionObjective { Id = "obj_8", Description = "8. Sobrevive a la primera noche", Type = ObjectiveType.SurviveNight, TargetAmount = 1 }
                }
            };

            ActiveMissions.Add(alphaChain);
            Log.Info("[Missions] First quest chain initialized: " + alphaChain.Title);
        }

        public void NotifyProgress(ObjectiveType type, string targetId = "", int amount = 1)
        {
            if (IsProxy) return;

            foreach (var mission in ActiveMissions.ToList())
            {
                for (int i = 0; i < mission.Objectives.Count; i++)
                {
                    var obj = mission.Objectives[i];
                    if (!obj.IsCompleted && obj.Type == type)
                    {
                        if (string.IsNullOrEmpty(obj.TargetId) || string.Equals(obj.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                        {
                            obj.CurrentProgress += amount;
                            if (obj.CurrentProgress >= obj.TargetAmount)
                            {
                                obj.CurrentProgress = obj.TargetAmount;
                                obj.IsCompleted = true;
                                Log.Info($"[Missions] Objective Completed: {obj.Description}");
                            }
                            mission.Objectives[i] = obj;
                        }
                    }
                }

                if ( mission.Objectives.All( o => o.IsCompleted ) )
                {
                    CompleteMission( mission );
                }
            }
        }

        /// <summary>
        /// Marca la misión como completada y aplica la recompensa (dinero + ítems)
        /// al jugador local. Idempotente por MissionId.
        /// </summary>
        private void CompleteMission( MissionDefinition mission )
        {
            if ( CompletedMissionIds.Contains( mission.MissionId ) ) return;

            CompletedMissionIds.Add( mission.MissionId );
            ActiveMissions.Remove( mission );

            var wallet = Components.Get<Economy.Wallet>();
            if ( wallet != null && mission.Reward.Money > 0 )
            {
                wallet.Deposit( mission.Reward.Money );
            }

            var inventory = Components.Get<InventoryComponent>();
            if ( inventory != null && !string.IsNullOrEmpty( mission.Reward.ItemId ) && mission.Reward.ItemAmount > 0 )
            {
                inventory.TryAdd( mission.Reward.ItemId, mission.Reward.ItemAmount );
            }

            Log.Info( $"[Missions] Mission Completed: {mission.Title} (+${mission.Reward.Money}, +{mission.Reward.ItemAmount}x{mission.Reward.ItemId})" );
        }
    }
}
