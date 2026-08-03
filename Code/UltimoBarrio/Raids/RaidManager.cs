using Sandbox;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.WorldTime;

namespace UltimoBarrio.Raids
{
    public class RaidManager : Component
    {
        [Property] public WorldClock Clock { get; set; }
        [Property] public GameObject LooterPrefab { get; set; }
        [Property] public int MinLooters { get; set; } = 1;
        [Property] public int MaxLooters { get; set; } = 3;
        [Property] public Transform SpawnPoint { get; set; }
        [Property] public float MaxRaidDuration { get; set; } = 180f; // Termina por tiempo

        private List<GameObject> activeLooters = new List<GameObject>();
        private bool isRaidActive = false;
        private float currentRaidTime = 0f;

        protected override void OnStart()
        {
            if (Clock != null)
            {
                Clock.OnPhaseChanged += HandlePhaseChanged;
            }
        }

        private void HandlePhaseChanged(TimePhase newPhase)
        {
            if (newPhase == TimePhase.Night)
            {
                StartRaid();
            }
            else if (newPhase == TimePhase.Aftermath)
            {
                EndRaid();
            }
        }

        private void StartRaid()
        {
            if (isRaidActive) return;
            isRaidActive = true;
            currentRaidTime = 0f;

            int numLooters = Game.Random.Int(MinLooters, MaxLooters);
            for (int i = 0; i < numLooters; i++)
            {
                SpawnLooter();
            }
        }

        private void SpawnLooter()
        {
            if (LooterPrefab == null || SpawnPoint == null) return;

            var looter = LooterPrefab.Clone(SpawnPoint.Position, SpawnPoint.Rotation);
            activeLooters.Add(looter);
        }

        protected override void OnUpdate()
        {
            if (isRaidActive)
            {
                currentRaidTime += Time.Delta;
                activeLooters.RemoveAll(l => l == null || !l.IsValid);

                if (activeLooters.Count == 0 || currentRaidTime >= MaxRaidDuration)
                {
                    EndRaid();
                }
            }
        }

        private void EndRaid()
        {
            isRaidActive = false;

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
