using Sandbox;

namespace UltimoBarrio.Components;

/// <summary>
/// Framework-level weapon state shared by content adapters. It intentionally
/// contains no damage or inventory policy: those remain host-side services.
/// The API mirrors the useful DarkRP BaseWeapon/IronSights contracts while
/// keeping the project's existing WeaponContentHost replaceable.
/// </summary>
[Title( "Último Barrio Weapon Framework" )]
[Category( "Último Barrio — Framework" )]
public class UbWeaponFrameworkComponent : UbCarryableComponent
{
	[Property] public bool UsesAmmo { get; set; } = true;
	[Property] public int MagazineSize { get; set; } = 12;
	[Property] public float FireInterval { get; set; } = 0.25f;
	[Property] public float ReloadDuration { get; set; } = 1.5f;
	[Property] public float IronSightsFireScale { get; set; } = 0.5f;

	[Sync] public int MagazineAmmo { get; protected set; }
	[Sync] public bool IsReloading { get; protected set; }
	[Sync] public bool IsAiming { get; protected set; }

	private TimeUntil _nextShot;
	private TimeUntil _reloadDone;

	protected override void OnStart()
	{
		if ( Networking.IsHost ) MagazineAmmo = MagazineSize;
	}

	public bool CanPrimaryAttack()
	{
		if ( IsReloading || _nextShot > 0 ) return false;
		return !UsesAmmo || MagazineAmmo > 0;
	}

	public bool TryPrimaryAttack()
	{
		if ( !Networking.IsHost || !CanPrimaryAttack() ) return false;

		if ( UsesAmmo ) MagazineAmmo--;
		_nextShot = FireInterval;
		OnPrimaryAttack();
		return true;
	}

	public bool TryStartReload()
	{
		if ( !Networking.IsHost || !UsesAmmo || IsReloading || MagazineAmmo >= MagazineSize ) return false;

		IsReloading = true;
		_reloadDone = ReloadDuration;
		OnReloadStarted();
		return true;
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost || !IsReloading || !_reloadDone ) return;

		IsReloading = false;
		MagazineAmmo = MagazineSize;
		OnReloadFinished();
	}

	public void SetAiming( bool aiming )
	{
		if ( !IsHeld || !Networking.IsHost ) return;
		IsAiming = aiming;
	}

	protected virtual void OnPrimaryAttack() { }
	protected virtual void OnReloadStarted() { }
	protected virtual void OnReloadFinished() { }
}
