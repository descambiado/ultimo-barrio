using Sandbox;
using System.Linq;

namespace UltimoBarrio.Development;

/// <summary>
/// SPAWN GATE (temporal). Observación one-shot que espera al estado real de
/// juego (scene cargada + networking activo + conexión local) antes de censar
/// Game.ActiveScene. Decide CASO A/B/C del NetworkHelper y sigue al runtime
/// player 60s (t=0,5,15,30,45,60). Se ELIMINA tras Spawn/Move PASS.
/// </summary>
public sealed class UbSceneCensus : Component
{
	[Property] public GameObject SpawnPoint { get; set; }
	[Property] public GameObject PlayerPrefab { get; set; }

	private enum Phase { Waiting, ReadyCensus, PlayerCheck, Tracking }
	private Phase _phase = Phase.Waiting;
	private TimeSince _sinceStart;
	private TimeSince _sinceLog;
	private TimeSince _sinceReady;
	private PlayerController _player;

	protected override void OnStart()
	{
		if ( Scene.IsEditor || !Networking.IsHost ) return;
		_sinceStart = 0;
	}

	private bool Ready()
	{
		return !Game.ActiveScene.IsLoading
			&& Networking.IsActive
			&& Connection.Local is { IsActive: true }
			&& Connection.All.Any();
	}

	protected override void OnUpdate()
	{
		if ( Scene.IsEditor || !Networking.IsHost ) return;

		switch ( _phase )
		{
			case Phase.Waiting:
			{
				if ( _sinceLog > 1f )
				{
					_sinceLog = 0;
					Log.Info( $"[SpawnGate] sceneLoading={Game.ActiveScene.IsLoading} networkingActive={Networking.IsActive} networkingHost={Networking.IsHost} localActive={( Connection.Local?.IsActive.ToString() ?? "null" )} connections={Connection.All.Count()}" );
				}
				if ( Ready() )
				{
					Log.Info( "[SpawnGate] READY" );
					_sinceReady = 0;
					_phase = Phase.ReadyCensus;
					DoReadyCensus();
				}
				else if ( _sinceStart > 15f )
				{
					Log.Info( "[SpawnGate] TIMEOUT 15s (forzando censo)" );
					_sinceReady = 0;
					_phase = Phase.ReadyCensus;
					DoReadyCensus();
				}
				break;
			}
			case Phase.ReadyCensus:
			{
				if ( _sinceReady > 2f )
				{
					_phase = Phase.PlayerCheck;
				}
				break;
			}
			case Phase.PlayerCheck:
			{
				var players = Game.ActiveScene.GetAllComponents<PlayerController>().ToList();
				if ( players.Count == 1 )
				{
					_player = players[0];
					Log.Info( "[SpawnGate] CASO A: 1 player runtime" );
					DiagnosePlayer( _player );
					_sinceLog = 0;
					_phase = Phase.Tracking;
				}
				else if ( players.Count > 1 )
				{
					Log.Info( $"[SpawnGate] CASO C: {players.Count} players — duplicidad real" );
					foreach ( var p in players )
						Log.Info( $"[SpawnGate]   player {p.GameObject.Name} scene={p.GameObject.Scene.Source?.ResourcePath ?? "(sin)"} owner={( p.GameObject.Network?.Owner?.Id.ToString() ?? "null" )}" );
					_player = players[0];
					_sinceLog = 0;
					_phase = Phase.Tracking;
				}
				else if ( _sinceReady > 5f )
				{
					Log.Info( "[SpawnGate] CASO B: 0 players tras READY+5s" );
					DoNHConfig();
					_sinceLog = 0;
					_phase = Phase.Tracking;
				}
				break;
			}
			case Phase.Tracking:
			{
				if ( _sinceLog < 1f ) return;
				_sinceLog = 0;
				LogPlayer();
				break;
			}
		}
	}

	private void DoReadyCensus()
	{
		var scene = Game.ActiveScene;
		Log.Info( "[SpawnGate] === CENSO READY ===" );
		Log.Info( $"[SpawnGate] scene={scene.Source?.ResourcePath ?? "(sin)"} loading={scene.IsLoading} nh={scene.GetAllComponents<NetworkHelper>().Count()} players={scene.GetAllComponents<PlayerController>().Count()} health={scene.GetAllComponents<UltimoBarrio.Combat.HealthComponent>().Count()} inv={scene.GetAllComponents<UltimoBarrio.InventoryComponent>().Count()} weapons={scene.GetAllComponents<UltimoBarrio.Combat.UbWeaponCarrier>().Count()} spawnpoints={scene.GetAllComponents<SpawnPoint>().Count()}" );
		foreach ( var nh in scene.GetAllComponents<NetworkHelper>() )
		{
			var sp = nh.SpawnPoints;
			Log.Info( $"[SpawnGate] NH enabled={nh.Enabled} startServer={nh.StartServer} prefabValid={( nh.PlayerPrefab?.IsValid() ?? false )} prefab={nh.PlayerPrefab?.Name} spawns={( sp is null ? "null" : sp.Count.ToString() )}" );
			if ( sp is { Count: > 0 } )
				foreach ( var g in sp.Where( x => x.IsValid() ) )
					Log.Info( $"[SpawnGate]   NH.spawn {g.Name} pos={g.WorldPosition}" );
		}
		foreach ( var c in Connection.All )
		{
			Log.Info( $"[SpawnGate] conn id={c.Id} active={c.IsActive} host={c.IsHost} connecting={c.IsConnecting}" );
		}
	}

	private void DoNHConfig()
	{
		Log.Info( "[SpawnGate] NH config check:" );
		foreach ( var nh in Game.ActiveScene.GetAllComponents<NetworkHelper>() )
		{
			Log.Info( $"[SpawnGate]   NH enabled={nh.Enabled} startServer={nh.StartServer} prefabValid={( nh.PlayerPrefab?.IsValid() ?? false )} spawns={( nh.SpawnPoints is null ? "null" : nh.SpawnPoints.Count.ToString() )}" );
		}
		Log.Info( $"[SpawnGate]   Connection.Local active={( Connection.Local?.IsActive.ToString() ?? "null" )} count={Connection.All.Count()}" );
	}

	private void DiagnosePlayer( PlayerController pc )
	{
		var go = pc.GameObject;
		var rb = go.Components.Get<Rigidbody>();
		var owner = go.Network?.Owner?.Id.ToString() ?? "(null)";
		Log.Info( $"[SpawnGate] PLAYER go={go.Name} sceneIsActive={go.Scene == Game.ActiveScene} prefabScene={go.Scene is PrefabScene} owner={owner} isProxy={go.IsProxy} pos={go.WorldPosition}" );
		Log.Info( $"[SpawnGate]   PC.Velocity={pc.Velocity} RB.Velocity={( rb.IsValid() ? rb.Velocity.ToString() : "n/a" )} onGround={pc.IsOnGround} ground={( pc.GroundObject?.Name ?? "(null)" )}" );
		Log.Info( $"[SpawnGate]   bodyValid={( pc.Body?.IsValid() ?? false )} bodyCollider={( pc.BodyCollider?.IsValid() ?? false )} feetCollider={( pc.FeetCollider?.IsValid() ?? false )}" );

		var a = SpawnPoint.IsValid() ? SpawnPoint.WorldPosition : go.WorldPosition;
		var from = a + Vector3.Up * 500f;
		var to = a + Vector3.Down * 5000f;
		var tr = go.Scene.Trace.Ray( from, to ).UseHitboxes( false ).Run();
		Log.Info( $"[SpawnGate] TRACE(playerScene) hit={tr.Hit} go={tr.GameObject?.Name} comp={tr.Component?.GetType().Name} pos={tr.EndPosition}" );

		foreach ( var col in go.Scene.GetAllComponents<BoxCollider>() )
		{
			if ( col.GameObject.Name != "World" ) continue;
			var max = col.GameObject.WorldPosition + col.Center + col.Scale * 0.5f;
			bool sameScene = col.GameObject.Scene == go.Scene;
			bool samePw = col.GameObject.Scene.PhysicsWorld == go.Scene.PhysicsWorld;
			Log.Info( $"[SpawnGate] FLOOR topZ={max.z} floor.Scene==player.Scene: {sameScene} | floor.PW==player.PW: {samePw}" );
		}
	}

	private void LogPlayer()
	{
		if ( _player is null || !_player.IsValid() )
		{
			var p = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault();
			if ( !p.IsValid() )
			{
				Log.Info( $"[SpawnGate] t+{Time.Now:0.0} NO_PLAYER" );
				return;
			}
			_player = p;
		}
		var go = _player.GameObject;
		var rb = go.Components.Get<Rigidbody>();
		Log.Info( $"[SpawnGate] t+{Time.Now:0.0} pos={go.WorldPosition} vel={_player.Velocity} rbvel={( rb.IsValid() ? rb.Velocity.ToString() : "n/a" )} grounded={_player.IsOnGround} ground={( _player.GroundObject?.Name ?? "(null)" )}" );
	}
}
