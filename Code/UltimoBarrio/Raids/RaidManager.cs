using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.WorldTime;
using UltimoBarrio.Apartments;
using UltimoBarrio.Content.Enemies;

namespace UltimoBarrio.Raids
{
    public class RaidManager : Component
    {
        [Property] public WorldClock Clock { get; set; }
        [Property] public GameObject LooterPrefab { get; set; }
        [Property] public int MinLooters { get; set; } = 1;
        [Property] public int MaxLooters { get; set; } = 3;
        [Property] public GameObject SpawnPoint { get; set; }
        [Property] public float MaxRaidDuration { get; set; } = 180f; 

        [Property, Sync(SyncFlags.FromHost)] public bool isRaidActive { get; set; } = false;
        [Property, Sync(SyncFlags.FromHost)] public float currentRaidTime { get; set; } = 0f;
        
        private List<GameObject> activeLooters = new List<GameObject>();

        protected override void OnStart()
        {
            if ( Clock == null )
            {
                Clock = Scene.GetAllComponents<WorldClock>().FirstOrDefault();
            }

            if ( Clock != null && !IsProxy )
            {
                Clock.OnPhaseChanged += HandlePhaseChanged;
            }
        }

        private void HandlePhaseChanged(TimePhase newPhase)
        {
            if (IsProxy) return;

            if (newPhase == TimePhase.Night)
            {
                StartRaid();
            }
            else if (newPhase == TimePhase.Aftermath)
            {
                EndRaid(false);
            }
        }

        private void StartRaid()
        {
            if (isRaidActive) return;
            isRaidActive = true;
            currentRaidTime = 0f;

            Log.Info("Raid Started!");

            var target = FindRaidTarget();

            int numLooters = Random.Shared.Next(MinLooters, MaxLooters + 1);
            for (int i = 0; i < numLooters; i++)
            {
                SpawnLooter(target);
            }
        }

        private GameObject FindRaidTarget()
        {
            var apartment = Scene.GetAllComponents<ApartmentComponent>().FirstOrDefault();
            if (apartment != null)
            {
                return Random.Shared.Next(0, 2) == 0 ? apartment.DoorReference : apartment.StashReference;
            }
            return null;
        }

        private void SpawnLooter( GameObject target )
        {
            if ( LooterPrefab == null || SpawnPoint == null ) return;

            var looter = LooterPrefab.Clone( SpawnPoint.WorldPosition, SpawnPoint.WorldRotation );
            looter.NetworkSpawn();
            activeLooters.Add( looter );

            // Intentar SaqueadorBrain (AI vieja) si existe
            var brain = looter.Components.Get<AI.SaqueadorBrain>();
            if ( brain != null )
            {
                brain.RaidTarget = target;
            }

            // Intentar EnemyContentHost (content pack nuevo) si existe
            var enemyHost = looter.Components.Get<Content.Enemies.EnemyContentHost>();
            if ( enemyHost != null && target != null )
            {
                enemyHost.SetTarget( target );
            }
        }

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            if (isRaidActive)
            {
                currentRaidTime += Time.Delta;
                activeLooters.RemoveAll(l => l == null || !l.IsValid);

                if (activeLooters.Count == 0)
                {
                    EndRaid(true); // Defeated all enemies
                }
                else if (currentRaidTime >= MaxRaidDuration)
                {
                    EndRaid(false); // Time out, enemies won or raid over
                }
            }
        }

        private void EndRaid(bool victory)
        {
            if (!isRaidActive) return;
            isRaidActive = false;

            Log.Info(victory ? "Raid Ended: Victory!" : "Raid Ended: Defeat!");

            foreach (var looter in activeLooters)
            {
                if (looter != null && looter.IsValid)
                {
                    looter.Destroy();
                }
            }
            activeLooters.Clear();
        }
    }
}
