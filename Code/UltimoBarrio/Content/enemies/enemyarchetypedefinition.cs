namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Definición data-driven de un arquetipo de enemigo.
	/// Clase plana (datos puros) registrada en EnemyContentRegistry.
	///
	/// Convención de modelos igual que en armas:
	///   Model → candidato PRIMARIO (pendiente verificación).
	///   ModelFallback → ruta VERIFICADA que carga hoy.
	///   AssetsVerified=false → revisar con Cloud Browser antes de release.
	/// </summary>
	public sealed class EnemyArchetypeDefinition
	{
		public string Id { get; set; } = "";
		public string DisplayName { get; set; } = "";

		// Modelo / animación
		public string Model { get; set; } = "";
		public string ModelFallback { get; set; } = "";
		public string AnimGraph { get; set; } = "";
		public float Scale { get; set; } = 1f;

		// Combate
		public float MaxHealth { get; set; } = 100f;
		public float WalkSpeed { get; set; } = 200f;   // data-only: el adaptador del core nuevo lo aplica
		public float AttackRange { get; set; } = 100f;
		public float AttackDamage { get; set; } = 15f;
		public float AttackCooldown { get; set; } = 1.5f;
		public float PerceptionRange { get; set; } = 2000f;

		// Comportamiento
		public string LootTableId { get; set; } = "";
		public float CorpseLifetime { get; set; } = 0f; // 0 = destruir al morir (sin ragdoll aún)

		// Estado de verificación
		public bool AssetsVerified { get; set; } = false;
		public string VerificationNotes { get; set; } = "";
	}
}
