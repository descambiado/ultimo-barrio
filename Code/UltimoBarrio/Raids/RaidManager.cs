using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.WorldTime;
using UltimoBarrio.Apartments;
using UltimoBarrio.Audio;

namespace UltimoBarrio.Raids
{
    /// <summary>
    /// Director de raids: de noche selecciona una vivienda reclamada (prioridad
    /// sobre las libres), spawna saqueadores con RaidTarget y resuelve el
    /// resultado (victoria si caen todos, derrota si expira el tiempo).
    /// </summary>
    public class RaidManager : Component
    {
        [Property] public WorldClock Clock { get; set; }
        [Property] public GameObject LooterPrefab { get; set; }
        [Property] public int MinLooters { get; set; } = 1;
        [Property] public int MaxLooters { get; set; } = 3;
        [Property] public GameObject SpawnPoint { get; set; }
        [Property] public float MaxRaidDuration { get; set; } = 180f;

        [Property, Sync( SyncFlags.FromHost )]
        public bool isRaidActive { get; set; } = false;

        [Property, Sync( SyncFlags.FromHost )]
        public float currentRaidTime { get; set; } = 0f;

        [Property, Sync( SyncFlags.FromHost )]
        public string currentRaidTargetId { get; private set; } = string.Empty;

        private readonly List<GameObject> activeLooters = new();

        protected override void OnStart()
        {
            if ( !Core.FeatureFlags.EnableRaids )
            {
                isRaidActive = false;
                currentRaidTime = 0f;
                currentRaidTargetId = string.Empty;
                Enabled = false; // Disable component entirely
                return;
            }

            if ( Clock != null && !IsProxy )
                Clock.OnPhaseChanged += HandlePhaseChanged;
        }

        protected override void OnDestroy()
        {
            if ( Clock != null )
                Clock.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged( TimePhase newPhase )
        {
            if ( IsProxy || !Networking.IsHost )
                return;

            if ( !Core.FeatureFlags.EnableRaids )
                return;

            if ( newPhase == TimePhase.Night )
                StartRaid();
            else if ( newPhase is TimePhase.Aftermath or TimePhase.Day && isRaidActive )
                EndRaid( false );
        }

        private void StartRaid()
        {
            if ( isRaidActive || !Networking.IsHost )
                return;

            isRaidActive = true;
            currentRaidTime = 0f;

            var target = FindRaidTarget();
            if ( target is null )
            {
                Log.Warning( "UB.Raid SinObjetivo — no hay viviendas ni anchors." );
                EndRaid( false );
                return;
            }

            currentRaidTargetId = target.Name;

            int numLooters = Random.Shared.Next( MinLooters, MaxLooters + 1 );
            for ( int i = 0; i < numLooters; i++ )
                SpawnLooter( target );

            var audio = Scene.GetAllComponents<UltimoBarrioAudioCatalog>().FirstOrDefault();
            audio?.PlayEvent( AudioEvent.RaidStart );

            RpcRaidAnnounce( $"¡RAID! Los saqueadores atacan {currentRaidTargetId}." );
            Log.Info( $"UB.Raid Iniciada target={currentRaidTargetId} looters={numLooters}" );
        }

        private GameObject FindRaidTarget()
        {
            var apartments = Scene.GetAllComponents<ApartmentComponent>().ToList();

            // Prioridad: viviendas reclamadas (con puerta/alijo válidos).
            var claimed = apartments
                .Where( a => a.ClaimState == ApartmentClaimState.Claimed )
                .ToList();

            var pool = claimed.Count > 0 ? claimed : apartments;
            if ( pool.Count == 0 )
                return null;

            var apartment = pool[Random.Shared.Next( 0, pool.Count )];

            var candidates = new List<GameObject>();
            if ( apartment.DoorReference.IsValid() ) candidates.Add( apartment.DoorReference );
            if ( apartment.StashReference.IsValid() ) candidates.Add( apartment.StashReference );
            if ( apartment.RaidTargetReference.IsValid() ) candidates.Add( apartment.RaidTargetReference );

            return candidates.Count > 0
                ? candidates[Random.Shared.Next( 0, candidates.Count )]
                : apartment.GameObject;
        }

        private void SpawnLooter( GameObject target )
        {
            if ( LooterPrefab is null || !LooterPrefab.IsValid() )
            {
                Log.Warning( "UB.Raid LooterPrefabAusente — revisa RaidManager." );
                return;
            }

            var spawn = SpawnPoint.IsValid() ? SpawnPoint : GameObject;
            var looter = LooterPrefab.Clone( spawn.WorldPosition, spawn.WorldRotation );
            looter.NetworkSpawn();
            activeLooters.Add( looter );

            var brain = looter.Components.Get<AI.SaqueadorBrain>( FindMode.EverythingInSelfAndDescendants );
            if ( brain is not null )
                brain.RaidTarget = target;
        }

        protected override void OnUpdate()
        {
            if ( IsProxy || !Networking.IsHost )
                return;

            if ( !isRaidActive || !Core.FeatureFlags.EnableRaids )
                return;

            currentRaidTime += Time.Delta;
            activeLooters.RemoveAll( l => l is null || !l.IsValid() );

            if ( activeLooters.Count == 0 )
                EndRaid( true );
            else if ( currentRaidTime >= MaxRaidDuration )
                EndRaid( false );
        }

        private void EndRaid( bool victory )
        {
            if ( !isRaidActive )
                return;

            isRaidActive = false;
            currentRaidTime = 0f;

            foreach ( var looter in activeLooters )
            {
                if ( looter is not null && looter.IsValid() )
                    looter.Destroy();
            }
            activeLooters.Clear();

            Log.Info( victory ? "UB.Raid Terminada victoria=true" : "UB.Raid Terminada victoria=false" );
            RpcRaidAnnounce( victory
                ? "La vivienda aguanta: saqueadores derrotados."
                : "Los saqueadores han hecho de las suyas esta noche." );
        }

        [Rpc.Broadcast]
        private void RpcRaidAnnounce( string message )
        {
            UI.PlayerFeedback.Push( message );
        }
    }
}
