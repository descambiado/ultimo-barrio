// SPDX-License-Identifier: MPL-2.0

using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Players;

/// <summary>
/// Evento inmutable emitido por <see cref="UbPlayerSession"/> en el host.
/// Los consumidores pueden usarlo para telemetría, restauración o guardado sin
/// acoplarse al prefab concreto del jugador.
/// </summary>
public readonly record struct PlayerSessionLifecycle(
	PlayerIdentity Identity,
	Guid SessionId,
	string DisplayName,
	PlayerSessionLifecycleKind Kind,
	string Reason );

public enum PlayerSessionLifecycleKind
{
	Joined,
	Leaving,
	PersistenceRequested,
	PersistenceRestored
}
