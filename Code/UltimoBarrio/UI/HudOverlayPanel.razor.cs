
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using UltimoBarrio.Combat;
using UltimoBarrio.Economy;
using UltimoBarrio.WorldTime;
using UltimoBarrio.Raids;
using UltimoBarrio.Apartments;

namespace UltimoBarrio.UI
{
    public partial class HudOverlayPanel : Panel
    {
        public GameObject PlayerObj { get; set; }

        public string CurrentPhaseName => Clock?.CurrentPhase.ToString().ToUpper() ?? "DAY";
        public string TimeRemaining 
        {
            get
            {
                if (Clock == null) return "00:00";
                var t = TimeSpan.FromSeconds(Clock.TimeRemainingInPhase);
                return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            }
        }
        
        public bool IsRaidActive => Clock?.CurrentPhase == TimePhase.Night;
        public string RaidText => IsRaidActive ? "RAID EN CURSO" : "ZONA SEGURA";

        public string ApartmentText 
        {
            get 
            {
                var apts = PlayerObj?.Scene?.GetAllComponents<ApartmentComponent>();
                if (apts != null)
                {
                    bool hasClaimed = apts.Any(a => a.ClaimState == ApartmentClaimState.Claimed);
                    return hasClaimed ? "APARTAMENTO: RECLAMADO" : "APARTAMENTO: SIN RECLAMAR";
                }
                return "APARTAMENTO: DESCONOCIDO";
            }
        }

        public string HealthText => MathF.Ceiling(HealthComp?.Health ?? 0).ToString();
        
        public string AmmoText 
        {
            get 
            {
                var carrier = WeaponCarrier;
                if (carrier != null && !string.IsNullOrEmpty(carrier.ActiveItemId))
                {
                    return "LISTO";
                }
                return "--";
            }
        }

        public string MoneyText => "$" + (WalletComp?.Balance ?? 0).ToString();

        private WorldClock Clock => PlayerObj?.Scene?.GetAllComponents<WorldClock>().FirstOrDefault();
        private HealthComponent HealthComp => PlayerObj?.Components.Get<HealthComponent>(FindMode.EverythingInSelfAndDescendants);
        private Wallet WalletComp => PlayerObj?.Components.Get<Wallet>(FindMode.EverythingInSelfAndDescendants);
        private UbWeaponCarrier WeaponCarrier => PlayerObj?.Components.Get<UbWeaponCarrier>(FindMode.EverythingInSelfAndDescendants);
        
        public override void Tick()
        {
            base.Tick();
            StateHasChanged();
        }
    }
}

