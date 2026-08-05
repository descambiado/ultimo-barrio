// SPDX-License-Identifier: MPL-2.0

using System;

namespace UltimoBarrio.Persistence;

public sealed class SaveMigrator
{
	public bool TryMigrate( SaveSnapshot snapshot, out SaveSnapshot migrated, out string error )
	{
		migrated = null;

		if ( snapshot is null )
		{
			error = "Snapshot is null.";
			return false;
		}

		if ( snapshot.SaveVersion > SaveSnapshot.CurrentVersion )
		{
			error = $"Snapshot version {snapshot.SaveVersion} is newer than supported version {SaveSnapshot.CurrentVersion}.";
			return false;
		}

		// Migraciones explícitas, una por versión.
		if ( snapshot.SaveVersion == 1 )
		{
			// v1 → v2: se añadieron Clock, Fortifications, Missions y PlayerStates.
			snapshot.Clock ??= new ClockSaveData();
			snapshot.Fortifications ??= [];
			snapshot.Missions ??= [];
			snapshot.PlayerStates ??= [];
			snapshot.SaveVersion = 2;
		}

		if ( snapshot.SaveVersion < SaveSnapshot.CurrentVersion )
		{
			error = $"No migration path exists from version {snapshot.SaveVersion} to {SaveSnapshot.CurrentVersion}.";
			return false;
		}

		migrated = snapshot;
		error = string.Empty;
		return true;
	}
}
