// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Properties;

/// <summary>Estado de ocupación general, independiente del tipo de propiedad.</summary>
public enum PropertyClaimState
{
	Unclaimed = 0,
	Claimed = 1
}

/// <summary>Estado específico del ciclo de alquiler (solo aplica a PropertyType.Rental).</summary>
public enum RentalState
{
	Vacant = 0,
	Rented = 1,
	GracePeriod = 2,
	Evicted = 3
}
