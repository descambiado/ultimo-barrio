// SPDX-License-Identifier: MPL-2.0

using System;
using System.Globalization;

namespace UltimoBarrio.Persistence;

public sealed class LocalPersistenceProvider : IPersistenceProvider
{
	private const string SaveRoot = "ultimo-barrio/saves";
	private const string SnapshotPrefix = "snapshot-";
	private const string SnapshotSuffix = ".json";

	private readonly BaseFileSystem _fileSystem;
	private readonly SaveMigrator _migrator = new();
	private readonly object _ioGate = new();
	private readonly HashSet<string> _loadedSlots = new( StringComparer.Ordinal );
	private readonly HashSet<string> _blockedSlots = new( StringComparer.Ordinal );

	public LocalPersistenceProvider( BaseFileSystem fileSystem )
	{
		_fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );
	}

	public bool WritesBlocked
	{
		get
		{
			lock ( _ioGate )
			{
				return _blockedSlots.Count > 0;
			}
		}
	}

	public PersistenceLoadResult Load( string slotId )
	{
		lock ( _ioGate )
		{
			return LoadLocked( slotId );
		}
	}

	private PersistenceLoadResult LoadLocked( string slotId )
	{
		if ( !TryGetSlot( slotId, out var canonicalSlotId, out var directory ) )
		{
			return PersistenceLoadResult.Failed(
				PersistenceLoadStatus.InvalidSlot,
				"The save slot must already be lowercase ASCII without surrounding whitespace." );
		}

		try
		{
			var files = FindSnapshots( directory ).ToList();
			if ( files.Count == 0 )
			{
				_loadedSlots.Add( canonicalSlotId );
				return PersistenceLoadResult.Empty();
			}

			foreach ( var file in files.OrderByDescending( candidate => candidate.Generation ) )
			{
				var envelopeResult = TryReadEnvelope( file.Path, file.Generation, canonicalSlotId );
				if ( envelopeResult.Status == EnvelopeReadStatus.FutureVersion )
				{
					_blockedSlots.Add( canonicalSlotId );
					return PersistenceLoadResult.Failed(
						PersistenceLoadStatus.UnsupportedFutureVersion,
						"A newer save version exists. Loading and saving are blocked." );
				}

				if ( envelopeResult.Status != EnvelopeReadStatus.Valid )
					continue;

				_loadedSlots.Add( canonicalSlotId );
				return PersistenceLoadResult.Loaded( envelopeResult.Snapshot );
			}

			return PersistenceLoadResult.Failed(
				PersistenceLoadStatus.Corrupt,
				"No valid snapshot generation could be loaded." );
		}
		catch ( Exception exception )
		{
			return PersistenceLoadResult.Failed(
				PersistenceLoadStatus.Corrupt,
				$"Snapshot enumeration failed ({exception.GetType().Name})." );
		}
	}

	public PersistenceSaveResult Save( SaveSnapshot snapshot )
	{
		lock ( _ioGate )
		{
			return SaveLocked( snapshot );
		}
	}

	private PersistenceSaveResult SaveLocked( SaveSnapshot snapshot )
	{
		if ( snapshot is null || !TryGetSlot( snapshot.SlotId, out var canonicalSlotId, out var directory ) )
			return PersistenceSaveResult.Failed( "The snapshot or save slot is invalid." );

		if ( _blockedSlots.Contains( canonicalSlotId ) )
			return PersistenceSaveResult.Failed( "Saving is blocked by a newer save version." );

		if ( !_loadedSlots.Contains( canonicalSlotId ) )
			return PersistenceSaveResult.Failed( "The save slot must be loaded before its first write." );

		if ( !TryValidateSnapshot( snapshot, canonicalSlotId, out var validationError ) )
			return PersistenceSaveResult.Failed( validationError );

		try
		{
			_fileSystem.CreateDirectory( directory );

			var nextGeneration = FindSnapshots( directory )
				.Select( file => file.Generation )
				.DefaultIfEmpty( 0L )
				.Max();

			if ( nextGeneration == long.MaxValue )
				return PersistenceSaveResult.Failed( "The snapshot generation limit was reached." );

			nextGeneration++;

			var savedSnapshot = CloneForSave( snapshot, nextGeneration );
			var payloadJson = Json.Serialize( savedSnapshot );
			var envelope = new SaveEnvelope
			{
				EnvelopeVersion = SaveEnvelope.CurrentVersion,
				SaveVersion = savedSnapshot.SaveVersion,
				Generation = savedSnapshot.Generation,
				SlotId = savedSnapshot.SlotId,
				PayloadJson = payloadJson
			};
			envelope.ContentCrc64 = CalculateContentCrc( envelope );

			var path = $"{directory}/{SnapshotPrefix}{nextGeneration:D20}{SnapshotSuffix}";
			if ( _fileSystem.FileExists( path ) )
				return PersistenceSaveResult.Failed( "The next immutable snapshot generation already exists." );

			_fileSystem.WriteJson( path, envelope );

			var readBack = TryReadEnvelope( path, nextGeneration, canonicalSlotId );
			if ( readBack.Status != EnvelopeReadStatus.Valid )
				return PersistenceSaveResult.Failed( "The new snapshot failed read-back validation." );

			return PersistenceSaveResult.Saved( readBack.Snapshot );
		}
		catch ( Exception exception )
		{
			return PersistenceSaveResult.Failed( $"Snapshot write failed ({exception.GetType().Name})." );
		}
	}

	private EnvelopeReadResult TryReadEnvelope( string path, long expectedGeneration, string expectedSlotId )
	{
		try
		{
			var envelopeJson = _fileSystem.ReadAllText( path );
			if ( string.IsNullOrWhiteSpace( envelopeJson )
				|| !Json.TryDeserialize<SaveEnvelope>( envelopeJson, out var envelope )
				|| envelope is null )
			{
				return EnvelopeReadResult.Invalid();
			}

			if ( envelope.Generation != expectedGeneration
				|| !string.Equals( envelope.SlotId, expectedSlotId, StringComparison.Ordinal )
				|| string.IsNullOrEmpty( envelope.PayloadJson )
				|| CalculateContentCrc( envelope ) != envelope.ContentCrc64 )
			{
				return EnvelopeReadResult.Invalid();
			}

			if ( envelope.EnvelopeVersion > SaveEnvelope.CurrentVersion
				|| envelope.SaveVersion > SaveSnapshot.CurrentVersion )
			{
				return EnvelopeReadResult.FutureVersion();
			}

			if ( envelope.EnvelopeVersion != SaveEnvelope.CurrentVersion
				|| !Json.TryDeserialize<SaveSnapshot>( envelope.PayloadJson, out var snapshot )
				|| snapshot is null )
			{
				return EnvelopeReadResult.Invalid();
			}

			if ( snapshot.SaveVersion > SaveSnapshot.CurrentVersion )
				return EnvelopeReadResult.FutureVersion();

			if ( envelope.SaveVersion != snapshot.SaveVersion
				|| !_migrator.TryMigrate( snapshot, out var migrated, out _ )
				|| migrated.Generation != expectedGeneration
				|| !TryValidateSnapshot( migrated, expectedSlotId, out _ ) )
			{
				return EnvelopeReadResult.Invalid();
			}

			return EnvelopeReadResult.Valid( migrated );
		}
		catch ( Exception )
		{
			return EnvelopeReadResult.Invalid();
		}
	}

	private IEnumerable<SnapshotFile> FindSnapshots( string directory )
	{
		foreach ( var candidate in _fileSystem.FindFile( directory, $"{SnapshotPrefix}*{SnapshotSuffix}", false ) )
		{
			if ( !TryParseGeneration( candidate, out var generation ) )
				continue;

			var normalized = candidate.Replace( '\\', '/' );
			var path = normalized.Contains( '/' ) ? normalized : $"{directory}/{normalized}";
			yield return new SnapshotFile( path, generation );
		}
	}

	private static bool TryParseGeneration( string path, out long generation )
	{
		generation = 0;
		if ( string.IsNullOrWhiteSpace( path ) )
			return false;

		var fileName = path.Replace( '\\', '/' ).Split( '/' ).Last();
		if ( !fileName.StartsWith( SnapshotPrefix, StringComparison.Ordinal )
			|| !fileName.EndsWith( SnapshotSuffix, StringComparison.Ordinal ) )
		{
			return false;
		}

		var generationText = fileName[SnapshotPrefix.Length..^SnapshotSuffix.Length];
		return long.TryParse(
			generationText,
			NumberStyles.None,
			CultureInfo.InvariantCulture,
			out generation ) && generation > 0;
	}

	private static bool TryGetSlot(
		string slotId,
		out string canonicalSlotId,
		out string directory )
	{
		canonicalSlotId = string.Empty;
		directory = string.Empty;
		if ( string.IsNullOrEmpty( slotId ) || slotId.Length > 48 )
			return false;

		if ( !string.Equals( slotId, slotId.Trim(), StringComparison.Ordinal )
			|| !string.Equals( slotId, slotId.ToLowerInvariant(), StringComparison.Ordinal )
			|| slotId.Any( character =>
				!((character >= 'a' && character <= 'z')
					|| (character >= '0' && character <= '9')
					|| character is '-' or '_') ) )
		{
			return false;
		}

		canonicalSlotId = slotId;
		directory = $"{SaveRoot}/{canonicalSlotId}";
		return true;
	}

	private static ulong CalculateContentCrc( SaveEnvelope envelope )
	{
		var content = string.Concat(
			envelope.EnvelopeVersion.ToString( CultureInfo.InvariantCulture ), "\n",
			envelope.SaveVersion.ToString( CultureInfo.InvariantCulture ), "\n",
			envelope.Generation.ToString( CultureInfo.InvariantCulture ), "\n",
			envelope.SlotId, "\n",
			envelope.PayloadJson );

		return Sandbox.Utility.Crc64.FromString( content );
	}

	private static bool TryValidateSnapshot(
		SaveSnapshot snapshot,
		string expectedSlotId,
		out string error )
	{
		if ( snapshot.SaveVersion != SaveSnapshot.CurrentVersion
			|| snapshot.Generation < 0
			|| !string.Equals( snapshot.SlotId, expectedSlotId, StringComparison.Ordinal )
			|| snapshot.Apartments is null )
		{
			error = "Snapshot metadata is invalid.";
			return false;
		}

		var apartmentIds = new HashSet<string>( StringComparer.Ordinal );
		var ownerIds = new HashSet<string>( StringComparer.Ordinal );

		foreach ( var apartment in snapshot.Apartments )
		{
			if ( apartment is null
				|| string.IsNullOrWhiteSpace( apartment.ApartmentId )
				|| !apartmentIds.Add( apartment.ApartmentId )
				|| apartment.SaveVersion != SaveSnapshot.CurrentVersion
				|| !Enum.IsDefined( apartment.ClaimState ) )
			{
				error = "An apartment record is invalid or duplicated.";
				return false;
			}

			var hasOwner = !string.IsNullOrWhiteSpace( apartment.OwnerId );
			if ( hasOwner != (apartment.ClaimState == Apartments.ApartmentClaimState.Claimed)
				|| (hasOwner && !ownerIds.Add( apartment.OwnerId )) )
			{
				error = "Apartment ownership state is inconsistent or duplicated.";
				return false;
			}
		}

		error = string.Empty;
		return true;
	}

	private static SaveSnapshot CloneForSave( SaveSnapshot source, long generation )
	{
		return new SaveSnapshot
		{
			SaveVersion = source.SaveVersion,
			SlotId = source.SlotId,
			Generation = generation,
			SavedAtUtc = DateTimeOffset.UtcNow.ToString( "O", CultureInfo.InvariantCulture ),
			Apartments = source.Apartments.Select( apartment => new ApartmentSaveData
			{
				ApartmentId = apartment.ApartmentId,
				OwnerId = apartment.OwnerId,
				ClaimState = apartment.ClaimState,
				SaveVersion = apartment.SaveVersion
			} ).ToList()
		};
	}

	private readonly record struct SnapshotFile( string Path, long Generation );

	private enum EnvelopeReadStatus
	{
		Invalid = 0,
		Valid,
		FutureVersion
	}

	private readonly record struct EnvelopeReadResult(
		EnvelopeReadStatus Status,
		SaveSnapshot Snapshot )
	{
		public static EnvelopeReadResult Invalid() => new( EnvelopeReadStatus.Invalid, null );

		public static EnvelopeReadResult Valid( SaveSnapshot snapshot ) => new( EnvelopeReadStatus.Valid, snapshot );

		public static EnvelopeReadResult FutureVersion() => new( EnvelopeReadStatus.FutureVersion, null );
	}
}
