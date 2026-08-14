using Sandbox;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Frontera de adaptador para enemigos de contenido.
	/// El core nuevo (integration/wizard-holy-grail) consumirá esta interfaz;
	/// el pack la implementa de forma autocontenida (EnemyContentHost).
	/// El contrato NO toca inventario canónico ni el core viejo: solo expone
	/// datos del enemigo, daño por ruta real y objetivos por referencia.
	/// </summary>
	public interface IEnemyContentAdapter
	{
		/// <summary>Definición data-driven del arquetipo (stats, percepción, loot).</summary>
		EnemyContentDefinition Definition { get; }

		float Health { get; }
		bool IsDead { get; }

		/// <summary>¿Tiene objetivo percibido ahora mismo? (lectura para el core/rig)</summary>
		bool IsTargetAcquired { get; }

		void TakeDamage( Content.ContentDamageEvent damageEvent );

		/// <summary>Asigna objetivo explícito (jugador, estructura, dummy de lab).</summary>
		void SetTarget( Sandbox.GameObject target );

		/// <summary>
		/// Oído: el core nuevo reporta ruidos (disparos, pasos, explosiones);
		/// el enemigo los procesa según su HearingRadius.
		/// </summary>
		void ReportNoise( Vector3 position, float volume );
	}
}
