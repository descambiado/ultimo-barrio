// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;
using UltimoBarrio.Properties.Keys;

namespace UltimoBarrio.Persistence;

/// <summary>
/// Llavero guardado de un jugador (v3), keyeado por su InventoryId canónico
/// (mismo patrón que PlayerStateSaveData). ExpiresAt es Time.Now del proceso:
/// sobrevive un ciclo Stop/Play dentro de la misma sesión del editor, no un
/// reinicio completo del motor — aceptable para invitados temporales.
/// </summary>
public sealed class KeyringSaveData
{
	public string PlayerKey { get; set; } = string.Empty;

	public List<AccessCredentialSaveData> Credentials { get; set; } = new();
}

public sealed class AccessCredentialSaveData
{
	public string PropertyId { get; set; } = string.Empty;

	public string LockId { get; set; } = string.Empty;

	public int KeyRevision { get; set; } = 1;

	public AccessLevel AccessLevel { get; set; } = AccessLevel.Guest;

	public string IssuerPersistentId { get; set; } = string.Empty;

	public float ExpiresAt { get; set; }

	public bool Stealable { get; set; }
}
