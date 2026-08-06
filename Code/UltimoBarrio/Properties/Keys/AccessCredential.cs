// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Properties.Keys;

public enum AccessLevel
{
	Guest = 0,
	Resident = 1,
	CoOwner = 2,
	Owner = 3
}

/// <summary>
/// Credencial de acceso a una PropertyDoor concreta. No es la fuente de
/// verdad del ownership — PropertyComponent.OwnerPersistentId lo es. Una
/// credencial solo abre físicamente si su LockId/KeyRevision coinciden con
/// los actuales de la puerta (un cambio de cerradura invalida todas las
/// anteriores automáticamente).
/// </summary>
public sealed class AccessCredential
{
	public string PropertyId { get; set; } = string.Empty;

	public string LockId { get; set; } = string.Empty;

	public int KeyRevision { get; set; } = 1;

	public AccessLevel AccessLevel { get; set; } = AccessLevel.Guest;

	public string IssuerPersistentId { get; set; } = string.Empty;

	/// <summary>Marca de Time.Now a partir de la cual la credencial deja de servir. 0 = sin caducidad (residente/propietario). Los invitados temporales sí la usan.</summary>
	public float ExpiresAt { get; set; }

	/// <summary>Si es true, esta credencial puede robarse (ítem "keyring" droppable/lootable) — opt-in explícito, no el comportamiento por defecto.</summary>
	public bool Stealable { get; set; }

	public bool IsExpired( float now ) => ExpiresAt > 0f && now >= ExpiresAt;

	public AccessCredential Clone( string newIssuerId = null )
	{
		return new AccessCredential
		{
			PropertyId = PropertyId,
			LockId = LockId,
			KeyRevision = KeyRevision,
			AccessLevel = AccessLevel,
			IssuerPersistentId = newIssuerId ?? IssuerPersistentId,
			ExpiresAt = ExpiresAt,
			Stealable = Stealable
		};
	}
}
