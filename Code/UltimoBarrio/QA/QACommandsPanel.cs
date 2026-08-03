using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Core;
using UltimoBarrio.Apartments;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Panel de QA — actívalo añadiendo el componente al player prefab
    /// o como un GO en la escena con ScreenPanel.
    /// Teclas de debug: F1=Reset save, F2=Give chatarra, F3=Give money,
    /// F4=Force Night, F5=Release my apartment.
    /// </summary>
    [Title("QA Commands Panel")]
    [Category("Último Barrio — QA")]
    [Icon("bug_report")]
    public sealed class QACommandsPanel : Component
    {
        [Property] public bool ShowPanel { get; set; } = false;

        protected override void OnUpdate()
        {
            if (IsProxy) return;

            // F1 — Reset QA save
            if (Input.Pressed("F1"))
            {
                Log.Info("[QA] F1 → Reset QA save slot");
                var claimService = Scene.GetAllComponents<ApartmentClaimService>().FirstOrDefault();
                if (claimService != null)
                {
                    Log.Info("[QA] ApartmentClaimService found — cannot reset save in runtime, delete Data/prototype.json manually");
                }
                else
                {
                    Log.Warning("[QA] ApartmentClaimService not found in scene");
                }
            }

            // F2 — Give 10 chatarra
            if (Input.Pressed("F2"))
            {
                Log.Info("[QA] F2 → Give 10 chatarra");
                var inv = Components.Get<InventoryComponent>();
                if (inv != null)
                {
                    bool ok = inv.TryAdd("chatarra", 10);
                    Log.Info($"[QA] TryAdd chatarra(10) = {ok}. Slots used: {inv.Slots.Count(s => !string.IsNullOrEmpty(s.ItemId))}");
                }
                else
                {
                    Log.Warning("[QA] No InventoryComponent on local player");
                }
            }

            // F3 — Give 100 money
            if (Input.Pressed("F3"))
            {
                Log.Info("[QA] F3 → Give 100 dinero");
                var wallet = Components.Get<UltimoBarrio.Economy.Wallet>();
                if (wallet != null)
                {
                    wallet.Deposit(100);
                    Log.Info($"[QA] Wallet balance now: {wallet.Balance}");
                }
                else
                {
                    Log.Warning("[QA] No Wallet on local player");
                }
            }

            // F4 — Force Night on WorldClock
            if (Input.Pressed("F4"))
            {
                Log.Info("[QA] F4 → Force Night phase");
                var clock = Scene.GetAllComponents<WorldTime.WorldClock>().FirstOrDefault();
                if (clock != null)
                {
                    clock.ForcePhase(UltimoBarrio.WorldTime.TimePhase.Night);
                    Log.Info("[QA] WorldClock forced to Night");
                }
                else
                {
                    Log.Warning("[QA] WorldClock not found");
                }
            }

            // F5 — Print inventory state
            if (Input.Pressed("F5"))
            {
                Log.Info("[QA] F5 → Inventory dump");
                var inv = Components.Get<InventoryComponent>();
                if (inv != null)
                {
                    Log.Info($"[QA] InventoryId: {inv.InventoryId}");
                    foreach (var slot in inv.Slots.Where(s => !string.IsNullOrEmpty(s.ItemId)))
                    {
                        Log.Info($"  [{slot.ItemId}] x{slot.Amount}");
                    }
                }
            }

            // F6 — Print apartment states
            if (Input.Pressed("F6"))
            {
                Log.Info("[QA] F6 → Apartment state dump");
                foreach (var apt in Scene.GetAllComponents<ApartmentComponent>())
                {
                    Log.Info($"  Apt: {apt.ApartmentId} | State: {apt.ClaimState} | Owner: {(string.IsNullOrEmpty(apt.OwnerId) ? "(none)" : apt.OwnerId)}");
                }
            }
        }
    }
}
