// SPDX-License-Identifier: MPL-2.0

using Sandbox;

namespace UltimoBarrio.Properties.Doors;

/// <summary>
/// Estado de cerradura de una PropertyDoor. La validación de acceso real
/// (quién puede abrir/cambiar la cerradura) la resuelve KeyringItem/
/// AccessCredential (Tarea #23) contra PropertyComponent server-side — este
/// componente solo guarda LockId/KeyRevision y refleja el bool físico en
/// Sandbox.Mapping.Door.
/// </summary>
[Title( "Property Door Lock" )]
[Category( "Último Barrio — Properties" )]
[Icon( "lock" )]
public sealed class DoorLockComponent : Component
{
	[Sync( SyncFlags.FromHost )] public string LockId { get; private set; } = string.Empty;

	/// <summary>Se incrementa en cada cambio de cerradura; invalida cualquier AccessCredential con una KeyRevision anterior.</summary>
	[Sync( SyncFlags.FromHost )] public int KeyRevision { get; private set; } = 1;

	[Sync( SyncFlags.FromHost )] public bool IsLocked { get; private set; }

	private Sandbox.Mapping.Door _physicalDoor;

	protected override void OnStart()
	{
		if ( string.IsNullOrEmpty( LockId ) )
			LockId = $"lock:{GameObject.Id}";

		_physicalDoor = Components.Get<Sandbox.Mapping.Door>();
		ApplyPhysicalState();
	}

	private void ApplyPhysicalState()
	{
		if ( _physicalDoor.IsValid() )
			_physicalDoor.IsLocked = IsLocked;
	}

	internal void SetLocked( bool locked )
	{
		IsLocked = locked;
		ApplyPhysicalState();
	}

	/// <summary>Cambia la cerradura: nuevo LockId, incrementa KeyRevision. Cualquier llave/credencial anterior deja de servir.</summary>
	internal void Rekey()
	{
		LockId = $"lock:{GameObject.Id}:{KeyRevision + 1}";
		KeyRevision++;
	}

	internal void RestoreLock( string lockId, int keyRevision, bool isLocked )
	{
		if ( !string.IsNullOrEmpty( lockId ) )
			LockId = lockId;

		if ( keyRevision > 0 )
			KeyRevision = keyRevision;

		IsLocked = isLocked;
		ApplyPhysicalState();
	}
}
