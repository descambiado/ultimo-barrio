using Sandbox;
using System.Collections.Generic;

namespace UltimoBarrio.AI;

public enum UbNpcTaskStatus
{
	Running,
	Success,
	Failed,
	Interrupted
}

/// <summary>Small, reusable schedule/task runtime ported from DarkRP's NPC framework.</summary>
public abstract class UbNpcSchedule
{
	private readonly List<UbNpcTask> _tasks = new();
	private int _index;

	public GameObject Agent { get; private set; }
	public string DebugName => _index < _tasks.Count
		? $"{GetType().Name}/{_tasks[_index].GetType().Name}"
		: $"{GetType().Name}/(none)";

	public void Start( GameObject agent )
	{
		Agent = agent;
		_tasks.Clear();
		_index = 0;
		BuildTasks();
		if ( _tasks.Count > 0 ) _tasks[0].Start( this );
	}

	public UbNpcTaskStatus Tick()
	{
		if ( _index >= _tasks.Count ) return UbNpcTaskStatus.Success;
		if ( ShouldInterrupt() ) return UbNpcTaskStatus.Interrupted;

		var status = _tasks[_index].Tick();
		if ( status == UbNpcTaskStatus.Running ) return status;
		_tasks[_index].End();
		if ( status != UbNpcTaskStatus.Success ) return status;

		_index++;
		if ( _index >= _tasks.Count ) return UbNpcTaskStatus.Success;
		_tasks[_index].Start( this );
		return UbNpcTaskStatus.Running;
	}

	public void Stop()
	{
		if ( _index < _tasks.Count ) _tasks[_index].End();
		_index = 0;
		OnStopped();
	}

	protected abstract void BuildTasks();
	protected virtual bool ShouldInterrupt() => false;
	protected virtual void OnStopped() { }
	protected void AddTask( UbNpcTask task ) => _tasks.Add( task );
	internal void AddMoveTask( Vector3 target ) => AddTask( new UbNpcMoveTask( target ) );
}

public abstract class UbNpcTask
{
	protected UbNpcSchedule Schedule { get; private set; }
	protected GameObject Agent => Schedule.Agent;

	internal void Start( UbNpcSchedule schedule )
	{
		Schedule = schedule;
		OnStart();
	}

	internal UbNpcTaskStatus Tick() => OnTick();
	internal void End() => OnEnd();
	protected virtual void OnStart() { }
	protected abstract UbNpcTaskStatus OnTick();
	protected virtual void OnEnd() { }
}

/// <summary>Navigation task using the real NavMeshAgent on the NPC object.</summary>
public sealed class UbNpcMoveTask : UbNpcTask
{
	private readonly Vector3 _target;
	private NavMeshAgent _agent;

	public UbNpcMoveTask( Vector3 target ) => _target = target;

	protected override void OnStart()
	{
		_agent = Agent.Components.Get<NavMeshAgent>();
		_agent?.MoveTo( _target );
	}

	protected override UbNpcTaskStatus OnTick()
	{
		if ( _agent == null ) return UbNpcTaskStatus.Failed;
		if ( !_agent.IsNavigating ) return UbNpcTaskStatus.Success;
		return UbNpcTaskStatus.Running;
	}

	protected override void OnEnd() => _agent?.Stop();
}
