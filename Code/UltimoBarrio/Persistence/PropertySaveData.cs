// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;
using UltimoBarrio.Properties;

namespace UltimoBarrio.Persistence;

/// <summary>
/// Snapshot persistente de PropertyComponent (v3). Convive con ApartmentSaveData
/// durante la migración — no la sustituye todavía. Ver PropertyClaimService.
/// </summary>
public sealed class PropertySaveData
{
	public string PropertyId { get; set; } = string.Empty;

	public PropertyType PropertyType { get; set; } = PropertyType.Rental;

	public string OwnerPersistentId { get; set; } = string.Empty;

	public string TenantPersistentId { get; set; } = string.Empty;

	public List<string> CoOwners { get; set; } = new();

	public List<string> Guests { get; set; } = new();

	public RentalState RentalState { get; set; } = RentalState.Vacant;

	public float NextRentAt { get; set; }

	public PropertyClaimState ClaimState { get; set; } = PropertyClaimState.Unclaimed;

	public int UpgradeLevel { get; set; }

	public int SecurityLevel { get; set; }

	public float DefenseScore { get; set; }

	public List<PropertyDoorSaveData> Doors { get; set; } = new();

	public int SaveVersion { get; set; } = SaveSnapshot.CurrentVersion;
}

public sealed class PropertyDoorSaveData
{
	public string AnchorId { get; set; } = string.Empty;

	public float Health { get; set; }

	public float MaxHealth { get; set; }

	public int UpgradeLevel { get; set; }

	public string LockId { get; set; } = string.Empty;

	public int KeyRevision { get; set; } = 1;

	public bool IsLocked { get; set; }
}
