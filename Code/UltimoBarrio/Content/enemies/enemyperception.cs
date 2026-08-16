using Sandbox;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Percepción del enemigo (componente separado, data-driven).
	/// - Visión: distancia + ángulo (cono) + línea de visión real (trace con hitboxes).
	/// - Oído: radio; el core nuevo reportará ruidos (disparos/pasos) vía
	///   IEnemyContentAdapter.ReportNoise y aquí se recuerda la posición.
	/// - Memoria: última posición conocida con duración configurable.
	///
	/// No depende del core antiguo (ni UltimoBarrio.AI.PerceptionComponent ni
	/// AIBase): todo el estado sale de EnemyArchetypeDefinition.
	/// </summary>
	[Title( "Content Enemy Perception" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "visibility" )]
	public sealed class EnemyPerception : Component
	{
		public EnemyArchetypeDefinition Definition { get; private set; }

		/// <summary>Objetivo actualmente percibido (o null).</summary>
		public GameObject CurrentTarget { get; private set; }

		/// <summary>Última posición conocida del objetivo (visión u oído).</summary>
		public Vector3? LastKnownPosition { get; private set; }
		public bool HasRecentMemory => LastKnownPosition.HasValue
			&& _timeSinceLastSeen <= ( Definition?.MemoryDuration ?? 0f );

		private TimeSince _timeSinceLastSeen;
		private TimeSince _timeSinceScan;

		/// <summary>El host configura la percepción desde la definición al cargar.</summary>
		public void Configure( EnemyArchetypeDefinition definition )
		{
			Definition = definition;
		}

		/// <summary>
		/// Tick de percepción (solo host). Si hay objetivo forzado (rig/core nuevo) se
		/// evalúa contra él; si no, se escanea el escenario por candidatos IDamageTarget
		/// con tag "enemy_target", ordenados según TargetPriority de la definición.
		/// </summary>
		public void Tick( GameObject forcedTarget )
		{
			if ( Definition == null ) return;

			if ( forcedTarget != null && forcedTarget.IsValid() )
			{
				if ( CanSee( forcedTarget ) )
				{
					Acquire( forcedTarget );
				}
				else if ( CurrentTarget == forcedTarget && _timeSinceLastSeen > Definition.MemoryDuration )
				{
					CurrentTarget = null;
				}
				return;
			}

			if ( _timeSinceScan < 0.2f ) return; // escaneo de candidatos a 5Hz
			_timeSinceScan = 0f;

			var candidate = FindBestCandidate();
			if ( candidate != null )
			{
				Acquire( candidate );
			}
			else if ( CurrentTarget != null && _timeSinceLastSeen > Definition.MemoryDuration )
			{
				CurrentTarget = null;
			}
		}

		/// <summary>
		/// ¿Ve el objetivo ahora mismo? Distancia ≤ VisionRange, ángulo ≤ VisionAngle/2
		/// y trace real con línea de visión: el objetivo (o su jerarquía) BLOQUEA el
		/// rayo; si algo intermedio lo bloquea, no hay visión.
		/// </summary>
		public bool CanSee( GameObject target )
		{
			if ( target == null || !target.IsValid() || Definition == null ) return false;

			var eyePos = WorldPosition + Vector3.Up * 50f;
			var targetPos = target.WorldPosition + Vector3.Up * 40f;

			float distance = Vector3.DistanceBetween( eyePos, targetPos );
			if ( distance > Definition.VisionRange ) return false;

			var dirToTarget = ( targetPos - eyePos ).Normal;
			float angle = Vector3.GetAngle( WorldRotation.Forward, dirToTarget );
			if ( angle > Definition.VisionAngle * 0.5f ) return false;

			var tr = Scene.Trace.Ray( eyePos, targetPos )
				.IgnoreGameObjectHierarchy( GameObject.Root ) // el propio enemigo no se bloquea a sí mismo
				.UseHitboxes( true )
				.Run();

			if ( !tr.Hit || tr.GameObject == null ) return false;

			return tr.GameObject == target
				|| tr.GameObject.Root == target.Root
				|| tr.GameObject.IsDescendant( target );
		}

		/// <summary>
		/// Oído: si el ruido cae dentro del radio se recuerda la posición aunque no haya
		/// línea de visión. El volumen escala el radio efectivo (1 = radio completo).
		/// </summary>
		public void ReportNoise( Vector3 position, float volume )
		{
			if ( Definition == null ) return;

			if ( Vector3.DistanceBetween( WorldPosition, position ) <= Definition.HearingRadius * volume )
			{
				Remember( position );
			}
		}

		/// <summary>Descarta el objetivo actual (sin limpiar la memoria).</summary>
		public void ForgetTarget()
		{
			CurrentTarget = null;
		}

		public void ForgetMemory()
		{
			CurrentTarget = null;
			LastKnownPosition = null;
		}

		private void Acquire( GameObject target )
		{
			CurrentTarget = target;
			Remember( target.WorldPosition );
		}

		private void Remember( Vector3 position )
		{
			LastKnownPosition = position;
			_timeSinceLastSeen = 0f;
		}

		/// <summary>
		/// Candidato más cercano visible con tag "enemy_target". Si TargetPriority ==
		/// Structures, los candidatos con Definition.StructureTag ganan a cualquier
		/// distancia razonable (score −100000). En el lab solo hay un candidato; el
		/// core nuevo alimentará objetivos explícitos vía SetTarget.
		/// </summary>
		private GameObject FindBestCandidate()
		{
			GameObject best = null;
			float bestScore = float.MaxValue;
			bool preferStructures = Definition.TargetPriority == EnemyTargetPriority.Structures;

			foreach ( var component in Scene.GetAllComponents<Component>() )
			{
				var go = component.GameObject;
				if ( go == GameObject || !go.Tags.Has( "enemy_target" ) ) continue;
				if ( go.Components.GetInAncestorsOrSelf<IDamageTarget>() == null ) continue;
				if ( !CanSee( go ) ) continue;

				float distance = Vector3.DistanceBetween( WorldPosition, go.WorldPosition );
				float score = distance;
				if ( preferStructures && !string.IsNullOrEmpty( Definition.StructureTag ) && go.Tags.Has( Definition.StructureTag ) )
				{
					score -= 100000f;
				}

				if ( score < bestScore )
				{
					bestScore = score;
					best = go;
				}
			}

			return best;
		}
	}
}
