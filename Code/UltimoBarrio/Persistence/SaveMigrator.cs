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
			
			if (snapshot.Apartments != null)
			{
				foreach (var apt in snapshot.Apartments)
				{
					if (apt.SaveVersion == 1) apt.SaveVersion = 2;
					if (!string.IsNullOrEmpty(apt.OwnerId) && ulong.TryParse(apt.OwnerId, out _))
					{
						apt.OwnerId = $"steam:{apt.OwnerId}";
					}
				}
			}

			snapshot.SaveVersion = 2;
		}

		if ( snapshot.SaveVersion == 2 )
		{
			// v2 → v3: se añadió el sistema general de propiedades (Properties, Keyrings).
			// Apartments/ApartmentClaimService siguen siendo la fuente de verdad de los
			// 6 fixtures durante la migración — esto solo garantiza que las listas nuevas
			// existan, no reescribe nada de lo ya guardado. TryValidateSnapshot exige que
			// cada ApartmentSaveData.SaveVersion coincida con CurrentVersion, igual que ya
			// hacía el bloque v1→v2 de arriba — sin esto, todo save v2 real (con
			// apartamentos guardados) se rechaza como "invalid or duplicated" al cargar.
			snapshot.Properties ??= [];
			snapshot.Keyrings ??= [];

			if ( snapshot.Apartments != null )
			{
				foreach ( var apt in snapshot.Apartments )
				{
					if ( apt.SaveVersion == 2 ) apt.SaveVersion = 3;
				}
			}

			snapshot.SaveVersion = 3;
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
