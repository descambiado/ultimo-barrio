using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.World
{
    [Title("Resource Node")]
    [Category("Último Barrio — World")]
    [Icon("grid_view")]
    public class ResourceNode : Component
    {
        [Property] public string ItemId { get; set; } = "chatarra";
        [Property] public int Amount { get; set; } = 1;
        [Property] public float RespawnTime { get; set; } = 30f;

        /// <summary>
        /// Ítem que entrega la recolección. Vacío = ItemId; las variantes de
        /// chatarra (scrap_*) producen chatarra internamente por defecto.
        /// </summary>
        [Property] public string HarvestItemId { get; set; } = string.Empty;

        public string ResolvedHarvestItemId
            => !string.IsNullOrEmpty( HarvestItemId )
                ? HarvestItemId
                : ( ItemId.StartsWith( "scrap_", StringComparison.Ordinal ) ? "chatarra" : ItemId );

        [Sync(SyncFlags.FromHost)] public bool IsAvailable { get; private set; } = true;

        private TimeSince timeSinceCollected;

        protected override void OnStart()
        {
            IsAvailable = false;
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            if (!Networking.IsHost) return;

            if (!IsAvailable && timeSinceCollected > RespawnTime)
            {
                IsAvailable = true;
                RpcSetVisibility(true);
            }
        }

        public bool TryHarvest(GameObject harvester, out int harvestedAmount)
        {
            harvestedAmount = 0;
            if (!Networking.IsHost) return false;
            if (!IsAvailable) return false;

            IsAvailable = false;
            timeSinceCollected = 0f;
            harvestedAmount = Amount;
            RpcSetVisibility(false);
            return true;
        }

        [Rpc.Broadcast]
        private void RpcSetVisibility(bool visible)
        {
            var mr = Components.Get<ModelRenderer>();
            if (mr != null) mr.Enabled = visible;

            var col = Components.Get<Collider>();
            if (col != null) col.Enabled = visible;
        }
    }
}
