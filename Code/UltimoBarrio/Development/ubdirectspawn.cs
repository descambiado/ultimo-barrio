using Sandbox;

namespace UltimoBarrio.Development;

/// <summary>
/// TEST DEFINITIVO: Rigidbody + CapsuleCollider (SIN PlayerController).
/// Si cae y aterriza en el suelo → el PlayerController es el culpable.
/// Si atraviesa → la física del engine no colisiona con nuestro suelo.
/// </summary>
public sealed class UbDirectSpawn : Component
{
	[Property] public GameObject SpawnPoint { get; set; }

	protected override void OnStart()
	{
		if ( !Networking.IsHost ) return;
		var pos = SpawnPoint.IsValid() ? SpawnPoint.WorldPosition : new Vector3( -100, -100, 400 );

		// GO simple: Rigidbody + CapsuleCollider
		var go = new GameObject();
		go.Name = "TestBody";
		go.WorldPosition = pos;
		go.Tags.Add( "player" );

		var rb = go.AddComponent<Rigidbody>();
		rb.Gravity = true;
		rb.MotionEnabled = false;
		rb.MassOverride = 80f;

		var col = go.AddComponent<CapsuleCollider>();
		col.Start = new Vector3( 0, 0, 0 );
		col.End = new Vector3( 0, 0, 72 );
		col.Radius = 16f;
		col.IsTrigger = false;

		Log.Info( $"[DirectSpawn] TestBody creado en {go.WorldPosition} scene={go.Scene.Source?.ResourcePath ?? "(sin)"}" );
		Log.Info( $"[DirectSpawn] rb bodyValid={(rb.PhysicsBody?.IsValid() ?? false)} col={col.IsTrigger}" );
	}

	protected override void OnUpdate()
	{
		var go = Scene.GetAllObjects( true ).FirstOrDefault( x => x.Name == "TestBody" );
		if ( !go.IsValid() ) return;
		var rb = go.Components.Get<Rigidbody>();
		var col = go.Components.Get<CapsuleCollider>();
		Log.Info( $"[DirectSpawn] t+{Time.Now:0.0} pos={go.WorldPosition} vel={(rb.IsValid()?rb.Velocity.ToString():"n/a")}" );
	}
}
