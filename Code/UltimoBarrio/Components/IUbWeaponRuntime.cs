using System;

namespace UltimoBarrio.Components;

/// <summary>
/// Stable boundary consumed by UbWeaponCarrier and the UI: the real weapon
/// state (WeaponContentHost) implements this so callers don't couple to its
/// concrete type. This is the only piece of the framework-kernel scaffolding
/// that ended up load-bearing — the parallel UbCarryableComponent/
/// UbWeaponFrameworkComponent base classes were removed (2026-08-16): nothing
/// derived from them, WeaponContentHost already covers the same ground and is
/// the one tested, working weapon system.
/// </summary>
public interface IUbWeaponRuntime
{
	bool IsAiming { get; }
	bool IsReloading { get; }
	int CurrentAmmo { get; }
	int MagazineCapacity { get; }
	bool CanFire { get; }

	/// <summary>
	/// Raised after a replicated runtime value changes. Consumers can update a
	/// durable snapshot, but must never invent a second magazine state.
	/// </summary>
	event Action RuntimeStateChanged;

	/// <summary>Host-only restoration from the active inventory slot.</summary>
	void RestoreAmmo( int amount );
}
