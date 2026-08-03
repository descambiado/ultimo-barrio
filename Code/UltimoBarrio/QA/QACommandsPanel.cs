using Sandbox;
using System;
using System.Linq;

namespace UltimoBarrio.QA
{
    /// <summary>
    /// Comandos de consola para QA. Escríbelos en la consola de s&box (tilde ~).
    /// qa_inv         — muestra el inventario del jugador local
    /// qa_give_scrap  — añade 10 chatarra al inventario
    /// qa_give_money  — añade 100 al wallet
    /// qa_apts        — muestra el estado de todos los apartamentos
    /// qa_force_night — fuerza la fase Noche en WorldClock
    /// qa_force_day   — fuerza la fase Día en WorldClock
    /// qa_release_apt — libera el apartamento del jugador local (host only)
    /// qa_release_all — libera TODOS los apartamentos (host only)
    /// </summary>
    [Title("QA Commands")]
    [Category("Último Barrio — QA")]
    [Icon("bug_report")]
    public sealed class QACommandsPanel : Component
    {
        // ── qa_inv ────────────────────────────────────────────────────────────
        [ConCmd("qa_inv")]
        public static void CmdInventory()
        {
            var player = FindLocalPlayer();
            if (player == null) { Log.Warning("[QA] No se encontró el jugador local."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            if (inv == null) { Log.Warning("[QA] Sin InventoryComponent."); return; }

            Log.Info($"[QA] InventoryId: {inv.InventoryId}");
            int filled = 0;
            foreach (var slot in inv.Slots.Where(s => !string.IsNullOrEmpty(s.ItemId)))
            {
                Log.Info($"  [{slot.ItemId}] x{slot.Amount}");
                filled++;
            }
            if (filled == 0) Log.Info("  (inventario vacío)");
        }

        // ── qa_give_scrap ─────────────────────────────────────────────────────
        [ConCmd("qa_give_scrap")]
        public static void CmdGiveScrap()
        {
            var player = FindLocalPlayer();
            if (player == null) { Log.Warning("[QA] No se encontró el jugador local."); return; }

            var inv = player.Components.Get<InventoryComponent>();
            if (inv == null) { Log.Warning("[QA] Sin InventoryComponent."); return; }

            bool ok = inv.TryAdd("chatarra", 10);
            Log.Info($"[QA] Give 10 chatarra → {(ok ? "OK" : "FALLIDO (sin espacio o proxy)")}");
        }

        // ── qa_give_money ─────────────────────────────────────────────────────
        [ConCmd("qa_give_money")]
        public static void CmdGiveMoney()
        {
            var player = FindLocalPlayer();
            if (player == null) { Log.Warning("[QA] No se encontró el jugador local."); return; }

            var wallet = player.Components.Get<Economy.Wallet>();
            if (wallet == null) { Log.Warning("[QA] Sin Wallet."); return; }

            wallet.Deposit(100);
            Log.Info($"[QA] Añadidos 100. Saldo: {wallet.Balance}");
        }

        // ── qa_apts ───────────────────────────────────────────────────────────
        [ConCmd("qa_apts")]
        public static void CmdApartments()
        {
            var scene = Game.ActiveScene;
            if (scene == null) { Log.Warning("[QA] Sin escena activa."); return; }

            var apts = scene.GetAllComponents<Apartments.ApartmentComponent>();
            foreach (var apt in apts)
            {
                string owner = string.IsNullOrEmpty(apt.OwnerId) ? "(libre)" : apt.OwnerId;
                Log.Info($"[QA] {apt.ApartmentId} | {apt.ClaimState} | Owner: {owner}");
            }
        }

        // ── qa_force_night ────────────────────────────────────────────────────
        [ConCmd("qa_force_night")]
        public static void CmdForceNight()
        {
            var scene = Game.ActiveScene;
            if (scene == null) { Log.Warning("[QA] Sin escena activa."); return; }

            var clock = scene.GetAllComponents<WorldTime.WorldClock>().FirstOrDefault();
            if (clock == null) { Log.Warning("[QA] WorldClock no encontrado."); return; }

            clock.ForcePhase(WorldTime.TimePhase.Night);
            Log.Info("[QA] WorldClock → Night");
        }

        // ── qa_force_day ──────────────────────────────────────────────────────
        [ConCmd("qa_force_day")]
        public static void CmdForceDay()
        {
            var scene = Game.ActiveScene;
            if (scene == null) { Log.Warning("[QA] Sin escena activa."); return; }

            var clock = scene.GetAllComponents<WorldTime.WorldClock>().FirstOrDefault();
            if (clock == null) { Log.Warning("[QA] WorldClock no encontrado."); return; }

            clock.ForcePhase(WorldTime.TimePhase.Day);
            Log.Info("[QA] WorldClock → Day");
        }

        // ── qa_release_apt ────────────────────────────────────────────────────
        [ConCmd("qa_release_apt")]
        public static void CmdReleaseApartment()
        {
            if (!Networking.IsHost) { Log.Warning("[QA] Solo el host puede liberar apartamentos."); return; }

            var scene = Game.ActiveScene;
            if (scene == null) { Log.Warning("[QA] Sin escena activa."); return; }

            var steamId = Game.SteamId.ToString();
            var apt = scene.GetAllComponents<Apartments.ApartmentComponent>()
                          .FirstOrDefault(a => a.OwnerId == steamId);

            if (apt == null)
            {
                Log.Warning($"[QA] No se encontró apartamento para SteamId {steamId}");
                return;
            }

            apt.ApplyState(string.Empty, Apartments.ApartmentClaimState.Unclaimed);
            Log.Info($"[QA] {apt.ApartmentId} liberado.");
        }

        // ── qa_release_all ────────────────────────────────────────────────────
        [ConCmd("qa_release_all")]
        public static void CmdReleaseAllApartments()
        {
            if (!Networking.IsHost) { Log.Warning("[QA] Solo el host puede liberar apartamentos."); return; }

            var scene = Game.ActiveScene;
            if (scene == null) { Log.Warning("[QA] Sin escena activa."); return; }

            var apts = scene.GetAllComponents<Apartments.ApartmentComponent>();
            foreach (var apt in apts)
            {
                apt.ApplyState(string.Empty, Apartments.ApartmentClaimState.Unclaimed);
            }
            Log.Info("[QA] Todos los apartamentos han sido liberados.");
        }

        // ── helper ────────────────────────────────────────────────────────────
        private static GameObject FindLocalPlayer()
        {
            var scene = Game.ActiveScene;
            if (scene == null) return null;

            return scene.GetAllComponents<PlayerController>()
                        .FirstOrDefault(p => !p.IsProxy)
                        ?.GameObject;
        }
    }
}
