// SPDX-License-Identifier: MPL-2.0

using System.Globalization;

namespace UltimoBarrio.Players;

public interface IPlayerIdentityProvider
{
	bool TryResolve( Connection connection, out string ownerId );
}

public sealed class SteamPlayerIdentityProvider : IPlayerIdentityProvider
{
	public bool TryResolve( Connection connection, out string ownerId )
	{
		ownerId = string.Empty;
		if ( connection is null )
			return false;

		var steamId = connection.SteamId.ValueUnsigned;
		if ( steamId == 0 )
			return false;

		ownerId = steamId.ToString( CultureInfo.InvariantCulture );
		return true;
	}
}
