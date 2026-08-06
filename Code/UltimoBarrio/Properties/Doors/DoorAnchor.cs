// SPDX-License-Identifier: MPL-2.0

using System.Linq;
using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Properties.Doors;

/// <summary>
/// Punto de instalación de puerta del editor (Property Authoring Tool, Tarea
/// #27) donde el propietario de un AbandonedShell puede instalar la puerta
/// fabricada (apartment_door_kit). Host-autoritativo vía RPC — mismo patrón
/// que BarricadeAnchor.
/// </summary>
[Title( "Property Door Anchor" )]
[Category( "Último Barrio — Properties" )]
[Icon( "anchor" )]
public sealed class DoorAnchor : Component, IWorldInteractable, IInteractable
{
	[Property] public string PropertyId { get; set; } = string.Empty;

	[Property] public string AnchorId { get; set; } = string.Empty;

	[Property] public string DoorId { get; set; } = "wooden_door";

	[Property] public float MaxInteractionDistance { get; set; } = 150f;

	public bool HasDoor => _door.IsValid();

	public PropertyDoor DoorReference => _door;

	private PropertyDoor _door;
	private DoorDefinition _definition;

	private DoorDefinition ResolveDefinition()
	{
		return _definition ??= ResourceLibrary.GetAll<DoorDefinition>().FirstOrDefault( d => d.DoorId == DoorId );
	}

	public string GetInteractionPrompt( InteractionRequest request )
	{
		if ( HasDoor )
			return "Puerta instalada";

		var def = ResolveDefinition();
		var requiredItem = def?.RequiredItemId ?? "apartment_door_kit";
		var inv = request.InteractorObject?.Components.GetInDescendantsOrSelf<InventoryComponent>();
		var count = inv?.GetCount( requiredItem ) ?? 0;

		return count > 0
			? $"Instalar puerta (tienes {count})"
			: "Instalar puerta (no llevas el kit)";
	}

	public bool CanInteract( InteractionRequest request )
	{
		if ( request.InteractorObject == null )
			return false;

		return Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
	}

	public void OnInteract( InteractionRequest request )
	{
		if ( IsProxy )
		{
			RequestInstallOnHost( request.InteractorObject?.Id ?? System.Guid.Empty );
			return;
		}

		ProcessInstall( request.InteractorObject );
	}

	[Rpc.Host]
	private void RequestInstallOnHost( System.Guid interactorObjectId )
	{
		var interactor = Scene.Directory.FindByGuid( interactorObjectId );
		ProcessInstall( interactor );
	}

	private void ProcessInstall( GameObject interactor )
	{
		if ( interactor is null || !Networking.IsHost || HasDoor )
			return;

		if ( Vector3.DistanceBetween( interactor.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
			return;

		var inventory = interactor.Components.GetInDescendantsOrSelf<InventoryComponent>();
		var def = ResolveDefinition();
		var requiredItem = def?.RequiredItemId ?? "apartment_door_kit";

		if ( inventory is null || !inventory.TryRemove( requiredItem, 1 ) )
			return;

		var door = SpawnDoor( def );
		door.RestoreState( def?.MaxHealth ?? 250f, def?.MaxHealth ?? 250f, 0, string.Empty, 1, false );
		_door = door;

		Persistence.PersistenceBridge.RequestSave();

		UI.PlayerFeedback.Push( "Puerta instalada" );
		Log.Info( $"UB.Property DoorInstalled property={PropertyId} anchor={AnchorId}" );
	}

	private PropertyDoor SpawnDoor( DoorDefinition def )
	{
		var doorGo = new GameObject( true, $"PropertyDoor_{AnchorId}" )
		{
			Parent = GameObject.Parent ?? GameObject,
			WorldPosition = GameObject.WorldPosition,
			WorldRotation = GameObject.WorldRotation
		};

		doorGo.Components.Create<BoxCollider>();
		doorGo.Components.Create<Sandbox.Mapping.Door>();

		var door = doorGo.Components.Create<PropertyDoor>();
		door.PropertyId = PropertyId;
		door.AnchorId = AnchorId;
		door.DoorId = DoorId;

		if ( Networking.IsActive )
			doorGo.NetworkSpawn();

		return door;
	}

	/// <summary>Restaura una puerta guardada (host, sin consumir el kit).</summary>
	internal void RestoreDoor( float health, float maxHealth, int upgradeLevel, string lockId, int keyRevision, bool isLocked )
	{
		if ( !Networking.IsHost || HasDoor || health <= 0f )
			return;

		var def = ResolveDefinition();
		var door = SpawnDoor( def );
		door.RestoreState( health, maxHealth, upgradeLevel, lockId, keyRevision, isLocked );
		_door = door;
	}
}
