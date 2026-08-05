// SPDX-License-Identifier: MPL-2.0

using System.Globalization;

using UltimoBarrio.Core;

namespace UltimoBarrio.Players;

public sealed class SteamPlayerIdentityProvider : IPlayerIdentityProvider
{
	public bool TryResolve( Connection connection, out PlayerIdentity identity )
	{
		if ( connection is null || !connection.IsActive )
		{
			identity = default;
			return false;
		}

		identity = PlayerIdentity.FromConnection( connection );
		return true;
	}
}
