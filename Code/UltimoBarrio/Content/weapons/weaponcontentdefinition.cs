namespace UltimoBarrio.Content.Weapons
{
	public enum WeaponContentCategory
	{
		Firearm,
		Melee
	}

	/// <summary>
	/// Definición data-driven de un arma de contenido.
	///
	/// Clase plana, sin dependencias de escena ni de red: solo datos.
	/// Se registra en WeaponContentRegistry y (en el futuro) se puede convertir
	/// a GameResource sin cambiar el contrato.
	///
	/// Convención de modelos:
	///   WorldModel  → candidato PRIMARIO (puede no estar verificado aún).
	///   WorldModelFallback → ruta VERIFICADA que carga sin errores hoy.
	///   AssetsVerified=false → revisar con Cloud Browser antes de cualquier release.
	/// </summary>
	public sealed class WeaponContentDefinition
	{
		public string Id { get; set; } = "";
		public string DisplayName { get; set; } = "";
		public WeaponContentCategory Category { get; set; } = WeaponContentCategory.Firearm;

		// Modelos
		public string WorldModel { get; set; } = "";
		public string WorldModelFallback { get; set; } = "";
		public string ViewModel { get; set; } = "";
		public string AmmoModel { get; set; } = "";
		public string CasingModel { get; set; } = "";

		// Sonido / efectos (nombres de SoundEvent; vacío = silencio hasta verificar)
		public string FireSound { get; set; } = "";
		public string ReloadSound { get; set; } = "";
		public string DryFireSound { get; set; } = "";
		public string MuzzleEffect { get; set; } = ""; // ruta .vpcf

		// Animación
		public string AnimGraph { get; set; } = "";
		public string HoldTypeParam { get; set; } = "holdtype";
		public float DrawTime { get; set; } = 0.5f;

		// Gameplay
		public float Damage { get; set; } = 15f;
		public float FireRate { get; set; } = 0.25f;
		public int MagazineSize { get; set; } = 12;
		public float ReloadTime { get; set; } = 1.5f;
		public bool IsAutomatic { get; set; } = false;
		public int Pellets { get; set; } = 1;          // >1 para escopetas
		public float MeleeRange { get; set; } = 90f;   // usado solo por Category==Melee
		public float Range { get; set; } = 5000f;
		public float RecoilKick { get; set; } = 0f;    // lo consume el adaptador del core nuevo
		public float MovementSpeedScale { get; set; } = 1f;
		public string AmmoType { get; set; } = "";     // item id de munición (mapeado luego)
		public string EquipSlot { get; set; } = "Primary";

		// Estado de verificación de assets
		public bool AssetsVerified { get; set; } = false;
		public string VerificationNotes { get; set; } = "";
	}
}
