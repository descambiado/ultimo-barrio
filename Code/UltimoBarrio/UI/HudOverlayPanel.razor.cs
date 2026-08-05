
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio;
using UltimoBarrio.Combat;
using UltimoBarrio.Economy;
using UltimoBarrio.WorldTime;
using UltimoBarrio.Raids;
using UltimoBarrio.Apartments;
using UltimoBarrio.Players;

namespace UltimoBarrio.UI
{
    public partial class HudOverlayPanel : Panel
    {
        public GameObject PlayerObj { get; set; }

        // ── Fase / reloj ────────────────────────────────────────────────────
        public string CurrentPhaseName => Clock?.CurrentPhase.ToString().ToUpper() ?? "DAY";
        public string TimeRemaining
        {
            get
            {
                if ( Clock == null ) return "00:00";
                var t = TimeSpan.FromSeconds( Clock.TimeRemainingInPhase );
                return string.Format( "{0:D2}:{1:D2}", t.Minutes, t.Seconds );
            }
        }

        public bool IsRaidActive => Clock?.CurrentPhase == TimePhase.Night;
        public string RaidText => IsRaidActive ? "RAID EN CURSO" : "ZONA SEGURA";

        public string ApartmentText
        {
            get
            {
                var apts = PlayerObj?.Scene?.GetAllComponents<ApartmentComponent>();
                if ( apts != null )
                {
                    bool hasClaimed = apts.Any( a => a.ClaimState == ApartmentClaimState.Claimed );
                    return hasClaimed ? "APARTAMENTO: RECLAMADO" : "APARTAMENTO: SIN RECLAMAR";
                }
                return "APARTAMENTO: DESCONOCIDO";
            }
        }

        // ── Vida / resistencia ───────────────────────────────────────────────
        public string HealthText => MathF.Ceiling( HealthComp?.Health ?? 0 ).ToString();
        public float HealthPercent => MaxHealth > 0 ? ( HealthComp?.Health ?? 0f ) / MaxHealth : 0f;
        public float MaxHealth => HealthComp?.MaxHealth ?? 100f;

        public string StaminaText => MathF.Ceiling( StaminaComp?.CurrentStamina ?? 0 ).ToString();
        public float StaminaPercent => StaminaMax > 0 ? ( StaminaComp?.CurrentStamina ?? 0f ) / StaminaMax : 0f;
        public float StaminaMax => StaminaComp?.Profile?.MaxStamina ?? 100f;
        public bool IsExhausted => StaminaComp?.IsExhausted ?? false;

        // ── Dinero ───────────────────────────────────────────────────────────
        public string MoneyText => "$" + ( WalletComp?.Balance ?? 0 ).ToString();

        // ── Arma activa / munición ───────────────────────────────────────────
        public string ActiveItemText
        {
            get
            {
                var held = HeldItemCtrl;
                if ( held is null || string.IsNullOrEmpty( held.ActiveItemId ) )
                    return "MANOS VACÍAS";

                var def = ItemRegistry.GetDefinition( held.ActiveItemId );
                return def?.DisplayName ?? held.ActiveItemId;
            }
        }

        public bool HasWeapon => ActiveWeapon is not null;

        public string MagText => HasWeapon ? $"{ActiveWeapon.CurrentAmmo}" : "--";

        public string ReserveText
        {
            get
            {
                if ( ActiveWeapon is null || string.IsNullOrEmpty( ActiveWeapon.AmmoType ) )
                    return "";

                var inv = PlayerObj?.Components.Get<InventoryComponent>( FindMode.EverythingInSelfAndDescendants );
                int reserve = inv?.GetCount( ActiveWeapon.AmmoType ) ?? 0;
                return $"{reserve}";
            }
        }

        public string MaxMagText => HasWeapon ? $"{ActiveWeapon.MaxAmmo}" : "";

        public bool IsReloading => ActiveWeapon?.IsReloading ?? false;

        public bool LowMag => HasWeapon && ActiveWeapon.CurrentAmmo <= ActiveWeapon.MaxAmmo * 0.25f;

        // ── Feedback transitorio (pickup, compra, crafteo) ───────────────────
        public IReadOnlyList<string> FeedbackMessages => _feedback;

        private readonly List<string> _feedback = new();
        private readonly List<float> _feedbackAge = new();

        private WorldClock Clock => PlayerObj?.Scene?.GetAllComponents<WorldClock>().FirstOrDefault();
        private HealthComponent HealthComp => PlayerObj?.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndDescendants );
        private PlayerMovementModifier StaminaComp => PlayerObj?.Components.Get<PlayerMovementModifier>( FindMode.EverythingInSelfAndDescendants );
        private Wallet WalletComp => PlayerObj?.Components.Get<Wallet>( FindMode.EverythingInSelfAndDescendants );
        private HeldItemController HeldItemCtrl => PlayerObj?.Components.Get<HeldItemController>( FindMode.EverythingInSelfAndDescendants );
        private Combat.BaseCombatWeapon ActiveWeapon
        {
            get
            {
                var held = HeldItemCtrl;
                if ( held is null || held.ActiveWeaponId == Guid.Empty )
                    return null;

                var weaponObj = PlayerObj?.Scene?.Directory.FindByGuid( held.ActiveWeaponId );
                if ( weaponObj is null )
                    return null;

                return weaponObj.Components.GetInDescendantsOrSelf<Combat.BaseCombatWeapon>();
            }
        }

        protected override void OnAfterTreeRender( bool firstTime )
        {
            base.OnAfterTreeRender( firstTime );
            if ( firstTime )
                PlayerFeedback.OnFeedback += OnPlayerFeedback;
        }

        public override void OnDeleted()
        {
            PlayerFeedback.OnFeedback -= OnPlayerFeedback;
            base.OnDeleted();
        }

        private void OnPlayerFeedback( string message )
        {
            _feedback.Add( message );
            _feedbackAge.Add( 0f );
        }

        public override void Tick()
        {
            base.Tick();

            // Expirar feedback antiguo.
            for ( int i = _feedback.Count - 1; i >= 0; i-- )
            {
                _feedbackAge[i] += Time.Delta;
                if ( _feedbackAge[i] > 4f )
                {
                    _feedback.RemoveAt( i );
                    _feedbackAge.RemoveAt( i );
                }
            }

            StateHasChanged();
        }
    }
}
