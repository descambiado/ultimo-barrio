using Sandbox;

namespace UltimoBarrio.Components;

/// <summary>
/// Host-only lifecycle policy for temporary world objects such as dropped
/// items, loot and transient props. It is opt-in: adding this component to a
/// prefab does not change the authority of the item's own interaction code.
/// </summary>
[Title( "Último Barrio Timed World Cleanup" )]
[Category( "Último Barrio — Framework" )]
public sealed class TimedWorldCleanup : Component
{
	/// <summary>Zero disables automatic expiry; explicit scheduled cleanup still works.</summary>
	[Property, Range( 0f, 7200f ), Step( 1f )] public float LifetimeSeconds { get; set; } = 300f;
	[Property] public bool PreserveWhileHeld { get; set; } = true;
	[Property] public bool PreserveWhileOwned { get; set; }

	[Sync] public bool IsCleanupScheduled { get; private set; }
	[Sync] public string ScheduledReason { get; private set; } = string.Empty;

	private TimeSince _timeAlive;
	private TimeUntil _scheduledCleanup;

	protected override void OnStart()
	{
		if ( Networking.IsHost ) _timeAlive = 0f;
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost || ShouldPreserve() ) return;

		if ( IsCleanupScheduled )
		{
			if ( !_scheduledCleanup ) DestroyFromHost( ScheduledReason );
			return;
		}

		if ( LifetimeSeconds > 0f && _timeAlive >= LifetimeSeconds )
		{
			DestroyFromHost( "lifetime-expired" );
		}
	}

	/// <summary>Schedules removal after a host-validated delay.</summary>
	public bool TryScheduleCleanup( float delaySeconds, string reason )
	{
		if ( !Networking.IsHost || delaySeconds < 0f ) return false;

		IsCleanupScheduled = true;
		ScheduledReason = string.IsNullOrWhiteSpace( reason ) ? "scheduled" : reason;
		_scheduledCleanup = delaySeconds;
		return true;
	}

	public bool TryCancelScheduledCleanup()
	{
		if ( !Networking.IsHost ) return false;

		IsCleanupScheduled = false;
		ScheduledReason = string.Empty;
		return true;
	}

	private bool ShouldPreserve()
	{
		if ( PreserveWhileHeld )
		{
			var carryable = Components.GetInDescendantsOrSelf<UbCarryableComponent>();
			if ( carryable is not null && carryable.IsHeld ) return true;
		}

		if ( PreserveWhileOwned )
		{
			var ownership = Components.GetInDescendantsOrSelf<WorldObjectOwnership>();
			if ( ownership is not null && ownership.HasOwner ) return true;
		}

		return false;
	}

	private void DestroyFromHost( string reason )
	{
		if ( !Networking.IsHost || !GameObject.IsValid() ) return;

		Log.Info( $"[UB.Cleanup] destroyed '{GameObject.Name}' ({reason})." );
		GameObject.Destroy();
	}
}
