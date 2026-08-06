// SPDX-License-Identifier: MPL-2.0

using System.Linq;
using Sandbox;

namespace UltimoBarrio.Properties.Keys;

/// <summary>
/// Llavero del jugador: un único objeto de inventario ("keyring") respaldado
/// por este componente host-autoritativo, que guarda las AccessCredential
/// reales. Mismo patrón que Wallet (componente en el jugador, gateado por si
/// el ítem sigue en su inventario) — evita inventar un slot de inventario con
/// payload complejo.
///
/// PENDIENTE AL REANUDAR CON EDITOR: a diferencia de Wallet/HeldItemController,
/// este componente NO está todavía en player.prefab (no se pudo editar sin
/// MCP vivo) — es el mismo patrón de bug ya encontrado 4 veces esta sesión
/// (componente escrito y enganchado a call sites reales pero nunca colocado).
/// KeyringService.RequestGrantAccess lo crea on-demand vía GetOrCreate, así
/// que un jugador que reciba una credencial SÍ lo tendrá — pero
/// WorldSnapshotService.ApplyKeyrings no puede restaurar un llavero guardado
/// en un jugador que aún no lo tiene al cargar. Añadir KeyringItem a
/// player.prefab (junto a Wallet) en cuanto el editor responda, antes de dar
/// la Tarea #23 por verificada.
/// </summary>
[Title( "Keyring" )]
[Category( "Último Barrio — Properties" )]
[Icon( "key" )]
public sealed class KeyringItem : Component
{
	public const string ItemId = "keyring";

	[Sync] public NetList<AccessCredential> Credentials { get; set; } = new();

	private bool HasKeyringItem()
	{
		var inventory = Components.GetInDescendantsOrSelf<InventoryComponent>();
		return inventory is not null && inventory.GetCount( ItemId ) > 0;
	}

	/// <summary>¿Tiene una credencial válida (cerradura actual, sin caducar, nivel suficiente) para esta puerta?</summary>
	public bool HasAccess( string propertyId, string lockId, int currentKeyRevision, AccessLevel minLevel )
	{
		if ( !HasKeyringItem() )
			return false;

		var now = Time.Now;
		return Credentials.Any( c =>
			c.PropertyId == propertyId &&
			c.LockId == lockId &&
			c.KeyRevision == currentKeyRevision &&
			c.AccessLevel >= minLevel &&
			!c.IsExpired( now ) );
	}

	internal void Grant( AccessCredential credential )
	{
		if ( credential is null )
			return;

		// Sustituir cualquier credencial previa para la misma propiedad (evita duplicados/llaves fantasma).
		var stale = Credentials.Where( c => c.PropertyId == credential.PropertyId ).ToList();
		foreach ( var s in stale )
			Credentials.Remove( s );

		Credentials.Add( credential );
	}

	internal int Revoke( string propertyId )
	{
		var toRemove = Credentials.Where( c => c.PropertyId == propertyId ).ToList();
		foreach ( var c in toRemove )
			Credentials.Remove( c );

		return toRemove.Count;
	}

	/// <summary>Invalida en el acto cualquier credencial de esta propiedad que quedó desincronizada tras un cambio de cerradura.</summary>
	internal int PruneStaleRevisions( string propertyId, string currentLockId, int currentKeyRevision )
	{
		var stale = Credentials.Where( c => c.PropertyId == propertyId && (c.LockId != currentLockId || c.KeyRevision != currentKeyRevision) ).ToList();
		foreach ( var c in stale )
			Credentials.Remove( c );

		return stale.Count;
	}

	public AccessCredential FindCredential( string propertyId )
	{
		return Credentials.FirstOrDefault( c => c.PropertyId == propertyId );
	}
}
