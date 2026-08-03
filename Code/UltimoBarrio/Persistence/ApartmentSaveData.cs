// SPDX-License-Identifier: MPL-2.0

using UltimoBarrio.Apartments;

namespace UltimoBarrio.Persistence;

public sealed class ApartmentSaveData
{
	public string ApartmentId { get; set; } = string.Empty;

	public string OwnerId { get; set; } = string.Empty;

	public ApartmentClaimState ClaimState { get; set; } = ApartmentClaimState.Unclaimed;

	public int SaveVersion { get; set; } = SaveSnapshot.CurrentVersion;
}
