// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;
using Sandbox;

namespace UltimoBarrio.Properties;

/// <summary>
/// Entidad canónica del sistema general de propiedades: sustituye gradualmente
/// a ApartmentComponent. Soporta alquiler, habitáculos abandonados reclamables,
/// parcelas de construcción y bases de grupo bajo un único modelo de datos.
/// Ver PropertyClaimService (adapter) y docs/decisions para el plan de migración.
/// </summary>
[Title( "Último Barrio Property" )]
[Category( "Último Barrio" )]
[Icon( "home_work" )]
public sealed class PropertyComponent : Component
{
	[Property] public string PropertyId { get; set; } = string.Empty;

	[Property] public PropertyType PropertyType { get; set; } = PropertyType.Rental;

	[Property] public string DisplayName { get; set; } = string.Empty;

	[Property] public BBox Bounds { get; set; } = BBox.FromPositionAndSize( Vector3.Zero, Vector3.One * 256f );

	// ── Ownership / tenancy ───────────────────────────────────────────────

	[Sync( SyncFlags.FromHost )] public string OwnerPersistentId { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )] public string TenantPersistentId { get; private set; } = string.Empty;

	[Sync] public NetList<string> CoOwners { get; set; } = new();

	[Sync] public NetList<string> Guests { get; set; } = new();

	// ── Rental ─────────────────────────────────────────────────────────────

	[Sync( SyncFlags.FromHost )] public RentalState RentalState { get; private set; } = RentalState.Vacant;

	[Property] public int RentPrice { get; set; } = 50;

	[Property] public int Deposit { get; set; } = 25;

	/// <summary>Segundos de tiempo de juego restantes hasta el próximo cobro. Lo gestiona el flujo de alquiler (Tarea #24).</summary>
	[Sync( SyncFlags.FromHost )] public float NextRentAt { get; private set; }

	// ── Claim (habitáculos abandonados, parcelas, etc.) ──────────────────

	[Sync( SyncFlags.FromHost )] public PropertyClaimState ClaimState { get; private set; } = PropertyClaimState.Unclaimed;

	// ── Anclajes de autoría (Property Authoring Tool, Tarea #27) ─────────

	[Property] public List<GameObject> DoorSlots { get; set; } = new();

	[Property] public List<GameObject> WindowSlots { get; set; } = new();

	[Property] public GameObject BuildVolume { get; set; }

	[Property] public GameObject ClaimCabinet { get; set; }

	[Property] public GameObject Stash { get; set; }

	[Property] public GameObject RespawnAnchor { get; set; }

	/// <summary>Referencias de autoría a estructuras construidas (Tarea #26). No es la fuente de verdad en runtime — las estructuras se consultan por PropertyId, igual que BarricadeAnchor.</summary>
	[Property] public List<GameObject> Structures { get; set; } = new();

	// ── Fortificación / progresión ────────────────────────────────────────

	[Sync( SyncFlags.FromHost )] public int UpgradeLevel { get; private set; }

	[Sync( SyncFlags.FromHost )] public int SecurityLevel { get; private set; }

	[Sync( SyncFlags.FromHost )] public float DefenseScore { get; private set; }

	internal void ApplyOwnership( string ownerId, PropertyClaimState claimState )
	{
		OwnerPersistentId = ownerId ?? string.Empty;
		ClaimState = claimState;
	}

	internal void ApplyTenancy( string tenantId, RentalState rentalState, float nextRentAt )
	{
		TenantPersistentId = tenantId ?? string.Empty;
		RentalState = rentalState;
		NextRentAt = nextRentAt;
	}

	internal void ApplyProgression( int upgradeLevel, int securityLevel, float defenseScore )
	{
		UpgradeLevel = upgradeLevel;
		SecurityLevel = securityLevel;
		DefenseScore = defenseScore;
	}

	internal bool TryValidateConfiguration( out string error )
	{
		if ( string.IsNullOrWhiteSpace( PropertyId ) )
		{
			error = "PropertyId is empty.";
			return false;
		}

		error = string.Empty;
		return true;
	}
}
