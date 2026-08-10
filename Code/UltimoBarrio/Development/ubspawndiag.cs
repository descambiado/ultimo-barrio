using Sandbox;
using System.Linq;

namespace UltimoBarrio.Development;

/// <summary>
/// DIAGNÓSTICO TEMPORAL del spawn (paso 01 del vertical slice).
/// Reporta estado del MapInstance (IsLoaded/Bounds), trace vertical profundo
/// desde Spawn A, y posicion/velocidad/grounded cada 0.5s. Se ELIMINA tras
/// el A/B test de 60s.
/// </summary>
public sealed class UbSpawnDiag : Component
{
	[Property] public GameObject SpawnPoint { get; set; }

	private TimeSince _sinceLog;
	private int _ticks;

	protected override void OnStart()
	{
		if ( Scene.IsEditor || !Networking.IsHost ) return;

		var map = Game.ActiveScene.GetAllComponents<MapInstance>().FirstOrDefault();
		Log.Info( $"[SpawnDiag] MAPINSTANCE valid={map.IsValid()} loaded={( map.IsValid() ? map.IsLoaded.ToString() : "n/a" )} bounds={( map.IsValid() ? map.Bounds.ToString() : "n/a" )}" );

		// Trace vertical profundo desde Spawn A: +500u arriba -> -5000u abajo
		var a = SpawnPoint.IsValid() ? SpawnPoint.WorldPosition : new Vector3( -100, -100, 10 );
		var tr = Scene.Trace.Ray( a + Vector3.Up * 500f, a + Vector3.Down * 5000f )
			.UseHitboxes( false )
			.Run();
		Log.Info( $"[SpawnDiag] TRACE_DEEP from={a} hit={tr.Hit} go={tr.GameObject?.Name} pos={tr.EndPosition} normal={tr.Normal} dist={tr.Distance:0.0} comp={tr.Component?.GetType().Name}" );

		Log.Info( $"[SpawnDiag] SPAWN_A={a}" );
	}

	protected override void OnUpdate()
	{
		if ( Scene.IsEditor || !Networking.IsHost ) return;
		if ( _sinceLog < 0.5f ) return;
		_sinceLog = 0;

		var player = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault();
		if ( !player.IsValid() )
		{
			Log.Info( $"[SpawnDiag] t+{Time.Now:0.0} NO_PLAYER" );
			return;
		}

		var pos = player.WorldPosition;
		var vel = player.Velocity;
		var health = player.GameObject.Components.Get<UltimoBarrio.Combat.HealthComponent>();
		var cc = player.GameObject.Components.GetInDescendantsOrSelf<CharacterController>();

		_ticks++;
		Log.Info( $"[SpawnDiag] t+{Time.Now:0.0} pos={pos} vel={vel} grounded={( cc?.IsOnGround.ToString() ?? "n/a" )} ground={( cc?.GroundObject?.Name ?? "n/a" )} hp={( health?.Health.ToString() ?? "n/a" )} dSpawn={( pos - ( SpawnPoint.IsValid() ? SpawnPoint.WorldPosition : new Vector3( -100, -100, 10 ) ) ).Length:0.0}" );
		if ( _ticks >= 130 ) Enabled = false; // ~65s de observacion
	}
}
