// SPDX-License-Identifier: MPL-2.0

using System;
using UltimoBarrio.Persistence;

namespace UltimoBarrio.Apartments;

public sealed class ApartmentRegistry
{
	private readonly Dictionary<string, ApartmentComponent> _apartments;

	private ApartmentRegistry(
		Dictionary<string, ApartmentComponent> apartments,
		IReadOnlyList<string> errors )
	{
		_apartments = apartments;
		Errors = errors;
	}

	public IReadOnlyCollection<ApartmentComponent> Apartments => _apartments.Values;

	public IReadOnlyList<string> Errors { get; }

	public bool IsValid => Errors.Count == 0;

	public static ApartmentRegistry Build( IEnumerable<ApartmentComponent> components )
	{
		var source = components.Where( component => component.IsValid() ).ToList();
		var errors = new List<string>();
		var apartments = new Dictionary<string, ApartmentComponent>( StringComparer.Ordinal );

		var grouped = source
			.Where( component => !string.IsNullOrWhiteSpace( component.ApartmentId ) )
			.GroupBy( component => component.ApartmentId.Trim(), StringComparer.Ordinal );

		foreach ( var component in source.Where( component => string.IsNullOrWhiteSpace( component.ApartmentId ) ) )
		{
			errors.Add( $"Apartment on '{component.GameObject.Name}' has an empty ApartmentId." );
		}

		foreach ( var group in grouped )
		{
			var matches = group.ToList();
			if ( matches.Count != 1 )
			{
				errors.Add( $"ApartmentId '{group.Key}' is duplicated {matches.Count} times." );
				continue;
			}

			var apartment = matches[0];
			apartment.ApartmentId = group.Key;

			if ( !apartment.TryValidateConfiguration( out var error ) )
			{
				errors.Add( error );
				continue;
			}

			apartments.Add( group.Key, apartment );
		}

		return new ApartmentRegistry( apartments, errors );
	}

	public bool TryGet( string apartmentId, out ApartmentComponent apartment )
	{
		apartment = null;
		return !string.IsNullOrWhiteSpace( apartmentId )
			&& _apartments.TryGetValue( apartmentId.Trim(), out apartment );
	}

	public ApartmentComponent FindByOwner( string ownerId )
	{
		if ( string.IsNullOrWhiteSpace( ownerId ) )
			return null;

		return _apartments.Values.FirstOrDefault(
			apartment => string.Equals( apartment.OwnerId, ownerId, StringComparison.Ordinal ) );
	}

	public SaveSnapshot CreateSnapshot( string slotId, string claimedApartmentId, string ownerId )
	{
		return new SaveSnapshot
		{
			SaveVersion = SaveSnapshot.CurrentVersion,
			SlotId = slotId,
			Apartments = _apartments.Values
				.OrderBy( apartment => apartment.ApartmentId, StringComparer.Ordinal )
				.Select( apartment => apartment.ToSaveData(
					string.Equals( apartment.ApartmentId, claimedApartmentId, StringComparison.Ordinal )
						? ownerId
						: null ) )
				.ToList()
		};
	}

	public void ApplySnapshot( SaveSnapshot snapshot )
	{
		if ( snapshot?.Apartments is null )
			return;

		foreach ( var savedApartment in snapshot.Apartments )
		{
			if ( !_apartments.TryGetValue( savedApartment.ApartmentId, out var apartment ) )
				continue;

			apartment.ApplyState( savedApartment.OwnerId, savedApartment.ClaimState );
		}
	}
}
