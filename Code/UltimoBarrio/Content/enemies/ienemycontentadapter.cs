namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Frontera de adaptador para enemigos de contenido.
	/// El core nuevo (integration/wizard-holy-grail) consumirá esta interfaz;
	/// el pack la implementa de forma autocontenida (EnemyContentHost).
	/// </summary>
	public interface IEnemyContentAdapter
	{
		EnemyArchetypeDefinition Definition { get; }
		float Health { get; }
		bool IsDead { get; }

		void TakeDamage( Content.ContentDamageEvent damageEvent );
		void SetTarget( Sandbox.GameObject target );
	}
}
