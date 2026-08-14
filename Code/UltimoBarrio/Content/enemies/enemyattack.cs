using Sandbox;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Ataque melee del enemigo (componente separado, data-driven).
	/// El daño recorre SIEMPRE la ruta real: ContentDamageEvent → IDamageTarget.
	/// Sin llamadas a internals: si el objetivo no tiene IDamageTarget, no hay daño.
	/// </summary>
	[Title( "Content Enemy Attack" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "swords" )]
	public sealed class EnemyAttack : Component
	{
		public EnemyContentDefinition Definition { get; private set; }

		/// <summary>Tiempo desde el último golpe (lectura del rig para logs deterministas).</summary>
		public TimeSince TimeSinceLastAttack { get; private set; }

		public bool HasAttacked => TimeSinceLastAttack < ( Definition?.AttackCooldown ?? 0f );

		/// <summary>El host configura el ataque desde la definición al cargar.</summary>
		public void Configure( EnemyContentDefinition definition )
		{
			Definition = definition;
		}

		/// <summary>
		/// Intenta golpear: respeta cooldown y rango reales, aplica el daño por la
		/// ruta IDamageTarget. Devuelve true solo si el golpe se ejecutó.
		/// </summary>
		public bool TryAttack( IDamageTarget target, Vector3 targetPosition, string sourceId )
		{
			if ( Definition == null || target == null || target.IsDead ) return false;
			if ( TimeSinceLastAttack < Definition.AttackCooldown ) return false;

			float distance = Vector3.DistanceBetween( WorldPosition, targetPosition );
			if ( distance > Definition.AttackRange ) return false;

			TimeSinceLastAttack = 0f;

			target.TakeDamage( new ContentDamageEvent
			{
				Amount = Definition.AttackDamage,
				Position = WorldPosition,
				Force = ( targetPosition - WorldPosition ).Normal * 100f,
				SourceId = sourceId
			} );

			return true;
		}
	}
}
