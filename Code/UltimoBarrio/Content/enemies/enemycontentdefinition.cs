namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Prioridad de selección de objetivo (data-driven). Un único brain (EnemyContentHost)
	/// se comporta distinto según la definición; el core nuevo alimenta candidatos
	/// vía IEnemyContentAdapter y el host los ordena con esta prioridad.
	/// </summary>
	public enum EnemyTargetPriority
	{
		Player,     // Saqueador: persigue al jugador por encima de todo.
		Structures, // Bruto: prefiere fortificaciones/estructuras (ver StructureTag).
		Balanced    // Merodeador: evalúa por proximidad y entradas vulnerables.
	}

	/// <summary>
	/// Definición data-driven de un arquetipo de enemigo (clase plana, sin dependencias
	/// de escena ni de red). Se registra en EnemyContentRegistry.
	///
	/// Convención de modelos (idéntica al pack de armas):
	///   Model → candidato PRIMARIO (PENDING_VERIFY).
	///   ModelFallback → ruta VERIFICADA que carga hoy.
	///   AssetsVerified=false → revisar con Cloud Browser antes de release.
	/// </summary>
	public sealed class EnemyContentDefinition
	{
		public string Id { get; set; } = "";
		public string DisplayName { get; set; } = "";

		// Modelo / animación
		public string Model { get; set; } = "";
		public string ModelFallback { get; set; } = "";
		public string AnimGraph { get; set; } = "";
		public float Scale { get; set; } = 1f;

		// Vitalidad
		public float MaxHealth { get; set; } = 100f;

		// Movimiento (NavMeshAgent REAL; lo aplica EnemyContentHost.ApplyDefinition)
		public float WalkSpeed { get; set; } = 220f;

		// Percepción (EnemyPerception)
		public float VisionRange { get; set; } = 2000f;
		public float VisionAngle { get; set; } = 100f;   // grados (cono completo; ángulo = FOV)
		public float HearingRadius { get; set; } = 1400f;
		public float MemoryDuration { get; set; } = 5f;  // recuerdo de la última posición conocida

		// Ataque melee (EnemyAttack)
		public float AttackRange { get; set; } = 110f;
		public float AttackDamage { get; set; } = 15f;
		public float AttackCooldown { get; set; } = 1.5f;

		// Comportamiento / estructura
		public EnemyTargetPriority TargetPriority { get; set; } = EnemyTargetPriority.Player;
		public string StructureTag { get; set; } = "";   // tag de estructuras preferidas (core nuevo)

		// Botín (tabla en EnemyContentRegistry; pickups físicos de mundo)
		public string LootTableId { get; set; } = "";

		// Estado de verificación de assets
		public bool AssetsVerified { get; set; } = false;
		public string VerificationNotes { get; set; } = "";
	}
}
