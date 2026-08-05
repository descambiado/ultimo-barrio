using Sandbox;
using System;

namespace UltimoBarrio.Core
{
    /// <summary>
    /// Represents a unique identifier for a player within a session.
    /// Canonical IDs take the form: "steam:76561198..." or "bot:1".
    /// </summary>
    public struct PlayerIdentity : IEquatable<PlayerIdentity>
    {
        public string CanonicalId { get; private set; }

        public bool IsValid => !string.IsNullOrEmpty( CanonicalId );

        public PlayerIdentity( string canonicalId )
        {
            CanonicalId = canonicalId;
        }

        public static PlayerIdentity FromSteamId( ulong steamId )
        {
            return new PlayerIdentity( $"steam:{steamId}" );
        }

        public static PlayerIdentity FromConnection( Connection connection )
        {
            if ( connection == null ) return new PlayerIdentity( string.Empty );
            return FromSteamId( connection.SteamId );
        }

        public static PlayerIdentity FromGameObject( GameObject playerGo )
        {
            if ( playerGo == null || playerGo.Network.Owner == null ) return new PlayerIdentity( string.Empty );
            return FromSteamId( playerGo.Network.Owner.SteamId );
        }

        public bool Equals( PlayerIdentity other )
        {
            return CanonicalId == other.CanonicalId;
        }

        public override bool Equals( object obj )
        {
            return obj is PlayerIdentity other && Equals( other );
        }

        public override int GetHashCode()
        {
            return CanonicalId?.GetHashCode() ?? 0;
        }

        public static bool operator ==( PlayerIdentity left, PlayerIdentity right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( PlayerIdentity left, PlayerIdentity right )
        {
            return !left.Equals( right );
        }

        public override string ToString()
        {
            return CanonicalId;
        }
    }
}
