using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UltimoBarrio.Content.Enemies;
using UltimoBarrio.WorldTime;

namespace UltimoBarrio.Npcs;

/// <summary>
/// Host-owned bridge between the world clock and the validated enemy prefabs.
///
/// This is deliberately opt-in: every spawn-related property defaults to its
/// inert value, and the director also honours the existing AI/Raid feature
/// flags. Adding it to a scene therefore cannot create NPCs by itself.
/// </summary>
[Title( "NPC Population Director" )]
[Category( "Último Barrio — NPCs" )]
[Icon( "groups" )]
public sealed class NpcPopulationDirector : Component
{
	private const int AbsolutePopulationLimit = 8;
	private const int AbsoluteRaidLimit = 8;

	[Property] public WorldClock Clock { get; set; }
	[Property] public List<GameObject> SpawnPoints { get; set; } = new();

	/// <summary>Must be enabled explicitly in the scene; false keeps this component inert.</summary>
	[Property] public bool EnableNightPopulation { get; set; }
	[Property] public string PopulationPrefabPath { get; set; } = "";
	[Property] public int PopulationCap { get; set; }
	[Property] public float PopulationSpawnInterval { get; set; } = 8f;

	/// <summary>Starts one configured raid on the Night transition when explicitly enabled.</summary>
	[Property] public bool StartRaidOnNight { get; set; }
	[Property] public string RaidPrefabPath { get; set; } = "";
	[Property] public int RaidSize { get; set; }
	[Property] public float RaidDuration { get; set; } = 180f;
	[Property] public GameObject RaidTarget { get; set; }

	[Sync] public int CurrentPopulation { get; private set; }
	[Sync] public bool IsRaidActive { get; private set; }

	private readonly List<GameObject> _population = new();
	private readonly List<GameObject> _raidMembers = new();
	private bool _nightActive;
	private bool _populationConfigWarningLogged;
	private bool _raidConfigWarningLogged;
	private TimeSince _sincePopulationSpawn;
	private TimeSince _sinceRaidStarted;

	protected override void OnStart()
	{
		if ( !Networking.IsHost )
			return;

		Clock ??= Scene.GetAllComponents<WorldClock>().FirstOrDefault();
		if ( Clock == null )
		{
			Log.Warning( "[UB.Npcs] Director sin WorldClock: permanece inactivo." );
			return;
		}

		Clock.OnPhaseChanged += HandlePhaseChanged;
		_nightActive = Clock.CurrentPhase == TimePhase.Night;
		_sincePopulationSpawn = 0f;
	}

	protected override void OnDestroy()
	{
		if ( Clock != null )
			Clock.OnPhaseChanged -= HandlePhaseChanged;
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost || !_nightActive )
			return;

		PruneInvalidReferences();

		if ( IsRaidActive )
		{
			if ( !RaidTarget.IsValid() || _sinceRaidStarted >= MathF.Max( 1f, RaidDuration ) )
				EndRaid( "objetivo ausente o duración agotada" );
		}

		if ( !EnableNightPopulation || !Core.FeatureFlags.EnableAI )
			return;

		var cap = Math.Clamp( PopulationCap, 0, AbsolutePopulationLimit );
		if ( cap == 0 || _population.Count >= cap )
			return;

		if ( _sincePopulationSpawn < MathF.Max( 0.5f, PopulationSpawnInterval ) )
			return;

		if ( TrySpawn( PopulationPrefabPath, out var npc, ref _populationConfigWarningLogged ) )
		{
			_population.Add( npc );
			_sincePopulationSpawn = 0f;
			CurrentPopulation = _population.Count;
			Log.Info( $"[UB.Npcs] PopulationSpawned count={CurrentPopulation}/{cap} prefab='{PopulationPrefabPath}'" );
		}
	}

	/// <summary>
	/// Explicit host call for future raid flow. It cannot start outside the night,
	/// with no valid target, with an empty prefab path, or when raids are disabled.
	/// </summary>
	public bool TryStartRaid()
	{
		if ( !Networking.IsHost || !_nightActive || IsRaidActive || !Core.FeatureFlags.EnableRaids )
			return false;

		if ( !RaidTarget.IsValid() )
		{
			Log.Warning( "[UB.Npcs] Raid no iniciada: RaidTarget no está configurado." );
			return false;
		}

		var size = Math.Clamp( RaidSize, 0, AbsoluteRaidLimit );
		if ( size == 0 )
			return false;

		for ( var index = 0; index < size; index++ )
		{
			if ( !TrySpawn( RaidPrefabPath, out var npc, ref _raidConfigWarningLogged ) )
				break;

			var enemy = npc.Components.Get<EnemyContentHost>( FindMode.EverythingInSelfAndDescendants );
			enemy?.SetTarget( RaidTarget );
			_raidMembers.Add( npc );
		}

		if ( _raidMembers.Count == 0 )
			return false;

		IsRaidActive = true;
		_sinceRaidStarted = 0f;
		Log.Info( $"[UB.Npcs] RaidStarted members={_raidMembers.Count} target='{RaidTarget.Name}'" );
		return true;
	}

	private void HandlePhaseChanged( TimePhase phase )
	{
		if ( !Networking.IsHost )
			return;

		_nightActive = phase == TimePhase.Night;
		if ( _nightActive )
		{
			_sincePopulationSpawn = 0f;
			if ( StartRaidOnNight )
				TryStartRaid();
			return;
		}

		DespawnAll( _population );
		CurrentPopulation = 0;
		EndRaid( $"fase {phase}" );
	}

	private bool TrySpawn( string prefabPath, out GameObject npc, ref bool warningLogged )
	{
		npc = null;
		if ( string.IsNullOrWhiteSpace( prefabPath ) )
		{
			LogConfigWarningOnce( ref warningLogged, "prefab no configurado" );
			return false;
		}

		var points = SpawnPoints.Where( point => point.IsValid() ).ToList();
		if ( points.Count == 0 )
		{
			LogConfigWarningOnce( ref warningLogged, "sin SpawnPoints válidos" );
			return false;
		}

		var prefab = ResourceLibrary.Get<PrefabFile>( prefabPath );
		if ( prefab == null )
		{
			LogConfigWarningOnce( ref warningLogged, $"prefab no encontrado '{prefabPath}'" );
			return false;
		}

		var point = points[Game.Random.Int( 0, points.Count - 1 )];
		npc = SceneUtility.GetPrefabScene( prefab ).Clone();
		npc.WorldPosition = point.WorldPosition;
		npc.WorldRotation = point.WorldRotation;
		npc.NetworkSpawn( Connection.Local );
		return true;
	}

	private static void LogConfigWarningOnce( ref bool warningLogged, string reason )
	{
		if ( warningLogged )
			return;

		warningLogged = true;
		Log.Warning( $"[UB.Npcs] Director inactivo para esta ruta: {reason}." );
	}

	private void PruneInvalidReferences()
	{
		_population.RemoveAll( npc => !npc.IsValid() );
		_raidMembers.RemoveAll( npc => !npc.IsValid() );
		CurrentPopulation = _population.Count;

		if ( IsRaidActive && _raidMembers.Count == 0 )
			EndRaid( "todos los miembros fueron retirados" );
	}

	private void EndRaid( string reason )
	{
		if ( !IsRaidActive && _raidMembers.Count == 0 )
			return;

		DespawnAll( _raidMembers );
		IsRaidActive = false;
		Log.Info( $"[UB.Npcs] RaidEnded reason={reason}" );
	}

	private static void DespawnAll( List<GameObject> npcs )
	{
		foreach ( var npc in npcs )
		{
			if ( npc.IsValid() )
				npc.Destroy();
		}

		npcs.Clear();
	}
}
