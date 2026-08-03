// SPDX-License-Identifier: MPL-2.0

namespace UltimoBarrio.Persistence;

public interface IPersistenceProvider
{
	bool WritesBlocked { get; }

	PersistenceLoadResult Load( string slotId );

	PersistenceSaveResult Save( SaveSnapshot snapshot );
}

public enum PersistenceLoadStatus
{
	Missing = 0,
	Loaded,
	Corrupt,
	UnsupportedFutureVersion,
	InvalidSlot
}

public readonly record struct PersistenceLoadResult(
	PersistenceLoadStatus Status,
	SaveSnapshot Snapshot,
	string Error )
{
	public bool Succeeded => Status is PersistenceLoadStatus.Missing or PersistenceLoadStatus.Loaded;

	public static PersistenceLoadResult Empty()
	{
		return new( PersistenceLoadStatus.Missing, null, string.Empty );
	}

	public static PersistenceLoadResult Loaded( SaveSnapshot snapshot )
	{
		return new( PersistenceLoadStatus.Loaded, snapshot, string.Empty );
	}

	public static PersistenceLoadResult Failed( PersistenceLoadStatus status, string error )
	{
		return new( status, null, error );
	}
}

public readonly record struct PersistenceSaveResult(
	bool Succeeded,
	SaveSnapshot Snapshot,
	string Error )
{
	public static PersistenceSaveResult Saved( SaveSnapshot snapshot )
	{
		return new( true, snapshot, string.Empty );
	}

	public static PersistenceSaveResult Failed( string error )
	{
		return new( false, null, error );
	}
}
