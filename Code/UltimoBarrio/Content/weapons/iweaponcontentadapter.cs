namespace UltimoBarrio.Content.Weapons
{
	/// <summary>
	/// Frontera de adaptador para armas de contenido.
	///
	/// El gameplay del core nuevo (integration/wizard-holy-grail) consumirá esta
	/// interfaz; el pack de contenido solo la implementa de forma autocontenida
	/// (WeaponContentHost). Así el pack se porta con cherry-pick sin arrastrar
	/// el InventoryComponent / HeldItemController antiguos.
	/// </summary>
	public interface IWeaponContentAdapter
	{
		WeaponContentDefinition Definition { get; }
		int CurrentAmmo { get; }
		bool IsReloading { get; }
		bool CanFire { get; }

		void Fire();
		void Reload();
		void DryFire();
	}
}
