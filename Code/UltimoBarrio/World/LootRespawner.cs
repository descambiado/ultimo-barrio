using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.World
{
    [Title("Loot Respawner")]
    [Category("Último Barrio — World")]
    [Icon("refresh")]
    public class LootRespawner : Component
    {
        [Property] public float IntervalSeconds { get; set; } = 45f;

        private TimeSince timeSinceLastCheck;

        protected override void OnUpdate()
        {
            if (!Networking.IsHost) return;

            if (timeSinceLastCheck > IntervalSeconds)
            {
                timeSinceLastCheck = 0f;
                RespawnWorldNodes();
            }
        }

        private void RespawnWorldNodes()
        {
            var nodes = Scene.GetAllComponents<ResourceNode>();
            int count = 0;
            foreach (var node in nodes)
            {
                if (!node.IsAvailable)
                {
                    count++;
                }
            }
            if (count > 0)
            {
                Log.Info($"[LootRespawner] Managing {count} harvested nodes on host.");
            }
        }
    }
}
