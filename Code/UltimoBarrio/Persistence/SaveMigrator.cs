// SPDX-License-Identifier: MPL-2.0

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
