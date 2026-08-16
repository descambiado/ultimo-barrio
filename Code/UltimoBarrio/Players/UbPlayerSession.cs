// SPDX-License-Identifier: MPL-2.0

using System;
using Sandbox;
using UltimoBarrio.Core;
using UltimoBarrio.Persistence;

namespace UltimoBarrio.Players;

/// <summary>
/// Identidad y ciclo de vida canónicos de un pawn de jugador.
///
/// La identidad persistente procede siempre de Network.Owner (Steam), nunca de
/// GameObject.Id ni de Connection.Local. Sólo el host inicializa y publica los
/// estados; los clientes reciben una vista de sólo lectura sincronizada.
///
/// No serializa snapshots ni abre proveedores de guardado. Cuando necesita un
/// guardado, delega en PersistenceBridge, cuyo escritor sigue siendo el dueño
/// único de la persistencia de mundo.
/// </summary>
[Title( "Player Session" )]
[Category( "Último Barrio — Players" )]
[Icon( "person" )]
public sealed class UbPlayerSession : Component
{
	[Sync( SyncFlags.FromHost )]
	public string PersistentId { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )]
	public string DisplayName { get; private set; } = string.Empty;

	[Sync( SyncFlags.FromHost )]
	public Guid SessionId { get; private set; }

	[Sync( SyncFlags.FromHost )]
	public bool IsConnected { get; private set; }

	/// <summary>Identidad estable apta para propiedad, inventario y guardado.</summary>
	public PlayerIdentity Identity => new( PersistentId );

	public bool HasStableIdentity => Identity.IsValid;

	/// <summary>
	/// Hooks de proceso host-side. Se usan en vez de que cada sistema sondee
	/// Connection.Local o implemente su propio listener de conexión.
	/// </summary>
	public static event Action<PlayerSessionLifecycle> LifecycleChanged;

	private bool _joinPublished;
	private bool _missingOwnerWarningPublished;

	protected override void OnStart()
	{
		TryInitializeOnHost();
	}

	protected override void OnUpdate()
	{
		// NetworkSpawn puede construir el componente antes de que Owner se haya
		// replicado. Reintentamos de forma silenciosa hasta poder usar la conexión
		// real, sin inventar IDs temporales.
		if ( !_joinPublished )
			TryInitializeOnHost();
	}

	protected override void OnDestroy()
	{
		if ( !Networking.IsHost || !_joinPublished )
			return;

		IsConnected = false;
		Publish( PlayerSessionLifecycleKind.Leaving, "pawn_destroyed" );
		RequestPersistence( "player_leave" );
	}

	/// <summary>
	/// Solicita una captura al escritor de persistencia existente. No guarda datos
	/// propios ni duplica el contrato de PersistenceBridge.
	/// </summary>
	public void RequestPersistence( string reason = "player_session" )
	{
		if ( !Networking.IsHost || !_joinPublished )
			return;

		var normalizedReason = string.IsNullOrWhiteSpace( reason ) ? "player_session" : reason.Trim();
		Publish( PlayerSessionLifecycleKind.PersistenceRequested, normalizedReason );
		PersistenceBridge.RequestSave( $"player:{PersistentId}:{normalizedReason}" );
	}

	/// <summary>
	/// Punto de integración para el sistema que aplique un SaveSnapshot a este
	/// pawn. El componente no decide qué se restaura: sólo publica que el estado
	/// ya fue aplicado correctamente.
	/// </summary>
	public void NotifyPersistenceRestored()
	{
		if ( !Networking.IsHost || !_joinPublished )
			return;

		Publish( PlayerSessionLifecycleKind.PersistenceRestored, "snapshot_applied" );
	}

	private void TryInitializeOnHost()
	{
		if ( !Networking.IsHost || _joinPublished )
			return;

		var owner = GameObject.Network.Owner;
		if ( owner is null || !owner.IsActive )
		{
			if ( !_missingOwnerWarningPublished )
			{
				_missingOwnerWarningPublished = true;
				Log.Warning( $"UB.PlayerSession AwaitingOwner object={GameObject.Name}" );
			}
			return;
		}

		var identity = PlayerIdentity.FromConnection( owner );
		if ( !identity.IsValid )
		{
			Log.Warning( $"UB.PlayerSession InvalidOwnerIdentity connection={owner.Id}" );
			return;
		}

		PersistentId = identity.CanonicalId;
		DisplayName = string.IsNullOrWhiteSpace( owner.DisplayName ) ? PersistentId : owner.DisplayName;
		SessionId = Guid.NewGuid();
		IsConnected = true;
		_joinPublished = true;
		Publish( PlayerSessionLifecycleKind.Joined, "network_owner_ready" );
	}

	private void Publish( PlayerSessionLifecycleKind kind, string reason )
	{
		LifecycleChanged?.Invoke( new PlayerSessionLifecycle( Identity, SessionId, DisplayName, kind, reason ) );
	}
}
