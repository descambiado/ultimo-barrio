using Sandbox;

namespace UltimoBarrio.Content
{
	/// <summary>
	/// Evento de daño mínimo y autocontenido del paquete Content.
	/// No depende de UltimoBarrio.Core (core antiguo de la rama feat/holy-grail-foundation).
	/// Cuando se publique integration/wizard-holy-grail, un bridge de pocas líneas
	/// mapeará IDamageTarget → el contrato de daño del core nuevo.
	/// </summary>
	public struct ContentDamageEvent
	{
		public float Amount;
		public Vector3 Position;
		public Vector3 Force;
		public string SourceId;    // weapon/enemy definition id
		public string AttackerId;  // connection id si existe
	}

	/// <summary>
	/// Contrato mínimo para que cualquier objeto de contenido reciba daño
	/// (enemigos, fortificaciones, dummies de laboratorio).
	/// </summary>
	public interface IDamageTarget
	{
		void TakeDamage( ContentDamageEvent damageEvent );
		bool IsDead { get; }
	}
}
