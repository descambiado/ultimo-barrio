using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Audio;

namespace UltimoBarrio.WorldTime
{
    /// <summary>
    /// Ciclo del mundo: Día → Atardecer (Preparation) → Noche → Amanecer
    /// (Aftermath). El host avanza las fases ([Sync]) y todos los clientes
    /// suavizan la iluminación (DirectionalLight) según LightLevel.
    /// </summary>
    [Title( "World Clock" )]
    [Category( "Último Barrio - World" )]
    [Icon( "schedule" )]
    public class WorldClock : Component, IWorldClock
    {
        [Property] public float DayDuration { get; set; } = 300f;
        [Property] public float PreparationDuration { get; set; } = 60f;
        [Property] public float NightDuration { get; set; } = 180f;
        [Property] public float AftermathDuration { get; set; } = 30f;

        /// <summary>Ajusta la luz direccional de la escena según la fase.</summary>
        [Property] public bool ControlLighting { get; set; } = true;

        /// <summary>Segundos que tarda la transición de luz entre fases.</summary>
        [Property] public float LightingTransitionSeconds { get; set; } = 20f;

        [Sync] public TimePhase CurrentPhase { get; private set; } = TimePhase.Day;
        [Sync] public float TimeRemainingInPhase { get; private set; }
        [Sync] public float LightLevel { get; private set; } = 1f;

        public event Action<TimePhase> OnPhaseChanged;

        private DirectionalLight _sceneLight;
        private float _targetLightLevel = 1f;

        protected override void OnStart()
        {
            if ( !IsProxy )
                SetPhase( TimePhase.Day );
        }

        protected override void OnUpdate()
        {
            if ( Scene.IsEditor )
                return;

            if ( !IsProxy )
            {
                TimeRemainingInPhase -= Time.Delta;
                if ( TimeRemainingInPhase <= 0 )
                    AdvancePhase();

                // Transición de luz suave.
                if ( Math.Abs( LightLevel - _targetLightLevel ) > 0.01f )
                {
                    LightLevel = MathX.Lerp( LightLevel, _targetLightLevel, Time.Delta / Math.Max( 0.1f, LightingTransitionSeconds ) );
                }
            }

            UpdateLighting();
        }

        private void UpdateLighting()
        {
            if ( !ControlLighting )
                return;

            if ( _sceneLight is null || !_sceneLight.IsValid() )
            {
                _sceneLight = Scene.GetAllComponents<DirectionalLight>().FirstOrDefault();
                if ( _sceneLight is null )
                    return;
            }

            var level = MathX.Clamp( LightLevel, 0f, 1f );
            var targetColor = _dayColor * level;
            _sceneLight.LightColor = Color.Lerp( _sceneLight.LightColor, targetColor, Time.Delta * 3f );

            var skyTarget = _daySkyColor * MathX.Lerp( 0.5f, 1f, level );
            _sceneLight.SkyColor = Color.Lerp( _sceneLight.SkyColor, skyTarget, Time.Delta * 3f );
        }

        private static readonly Color _dayColor = new( 0.94f, 0.98f, 1f );
        private static readonly Color _daySkyColor = new( 0.25f, 0.32f, 0.35f );

        private void AdvancePhase()
        {
            switch ( CurrentPhase )
            {
                case TimePhase.Day:
                    SetPhase( TimePhase.Preparation );
                    break;
                case TimePhase.Preparation:
                    SetPhase( TimePhase.Night );
                    RpcNightStarted();
                    break;
                case TimePhase.Night:
                    SetPhase( TimePhase.Aftermath );
                    // El jugador sobrevivió a la noche (transición Night → Aftermath).
                    Missions.MissionJournal.Local?.NotifyProgress( Missions.ObjectiveType.SurviveNight, "", 1 );
                    break;
                case TimePhase.Aftermath:
                    SetPhase( TimePhase.Day );
                    break;
            }
        }

        public void ForcePhase( TimePhase phase )
        {
            if ( !IsProxy )
                SetPhase( phase );
        }

        /// <summary>Restaura el estado del reloj desde el snapshot (host).</summary>
        internal void RestoreState( TimePhase phase, float remainingSeconds, float lightLevel )
        {
            if ( IsProxy )
                return;

            CurrentPhase = phase;
            TimeRemainingInPhase = Math.Max( 0f, remainingSeconds );
            _targetLightLevel = MathX.Clamp( lightLevel, 0f, 1f );
            LightLevel = _targetLightLevel;

            Log.Info( $"UB.WorldClock Restaurada fase={phase} restante={TimeRemainingInPhase}" );
            OnPhaseChanged?.Invoke( CurrentPhase );
        }

        private void SetPhase( TimePhase newPhase )
        {
            CurrentPhase = newPhase;

            switch ( newPhase )
            {
                case TimePhase.Day:
                    TimeRemainingInPhase = DayDuration;
                    _targetLightLevel = 1f;
                    break;
                case TimePhase.Preparation:
                    TimeRemainingInPhase = PreparationDuration;
                    _targetLightLevel = 0.65f;
                    break;
                case TimePhase.Night:
                    TimeRemainingInPhase = NightDuration;
                    _targetLightLevel = 0.12f;
                    break;
                case TimePhase.Aftermath:
                    TimeRemainingInPhase = AftermathDuration;
                    _targetLightLevel = 0.45f;
                    break;
            }

            Log.Info( $"UB.WorldClock Fase={newPhase} duración={TimeRemainingInPhase}" );
            OnPhaseChanged?.Invoke( CurrentPhase );
        }

        [Rpc.Broadcast]
        private void RpcNightStarted()
        {
            // No hay ningún asset .sound real en el proyecto todavía (0 archivos
            // bajo Assets/, confirmado por búsqueda) -- "raid.siren" nunca ha
            // apuntado a un sonido real, solo al nombre de evento aspiracional.
            // Omitido hasta que exista un asset de audio real registrado, en vez
            // de dejar el warning del motor en cada inicio de noche.
        }
    }
}
