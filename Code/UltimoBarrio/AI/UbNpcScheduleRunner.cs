using Sandbox;

namespace UltimoBarrio.AI;

/// <summary>
/// Component boundary for NPC schedules. Concrete brains choose schedules; this
/// runner owns cancellation, debug state and one active schedule at a time.
/// </summary>
[Title( "Último Barrio NPC Schedule Runner" )]
[Category( "Último Barrio — AI" )]
public sealed class UbNpcScheduleRunner : Component
{
	public UbNpcSchedule ActiveSchedule { get; private set; }
	public string ActiveScheduleDebug => ActiveSchedule?.DebugName ?? "(idle)";

	public void SetSchedule( UbNpcSchedule schedule )
	{
		if ( !Networking.IsHost || schedule == null ) return;
		ActiveSchedule?.Stop();
		ActiveSchedule = schedule;
		ActiveSchedule.Start( GameObject );
	}

	public void ClearSchedule()
	{
		if ( !Networking.IsHost ) return;
		ActiveSchedule?.Stop();
		ActiveSchedule = null;
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost || ActiveSchedule == null ) return;

		var status = ActiveSchedule.Tick();
		if ( status is UbNpcTaskStatus.Success or UbNpcTaskStatus.Failed or UbNpcTaskStatus.Interrupted )
		{
			ActiveSchedule.Stop();
			ActiveSchedule = null;
		}
	}

	protected override void OnDisabled()
	{
		ActiveSchedule?.Stop();
		ActiveSchedule = null;
		base.OnDisabled();
	}
}
