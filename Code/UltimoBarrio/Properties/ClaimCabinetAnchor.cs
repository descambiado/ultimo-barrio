// SPDX-License-Identifier: MPL-2.0

using System;
using Sandbox;
using UltimoBarrio.Core;

namespace UltimoBarrio.Properties;

/// <summary>
/// Punto de instalación del armario de reclamo (Property Authoring Tool,
/// Tarea #27). El jugador debe instalar primero la puerta (DoorAnchor) y
/// luego el armario aquí antes de que PropertyClaimService.TryClaimAbandonedShell
/// acepte el reclamo — mismo patrón host-autoritativo que DoorAnchor/BarricadeAnchor.
/// </summary>
[Title( "Claim Cabinet Anchor" )]
[Category( "Último Barrio — Properties" )]
[Icon( "anchor" )]
public sealed class ClaimCabinetAnchor : Component, IWorldInteractable, IInteractable
{
	public const string CabinetItemId = "claim_cabinet";

	[Property] public string PropertyId { get; set; } = string.Empty;

	[Property] public float MaxInteractionDistance { get; set; } = 150f;

	public bool HasCabinet => _cabinet.IsValid();

	public ClaimCabinetComponent CabinetReference => _cabinet;

	private ClaimCabinetComponent _cabinet;

	public string GetInteractionPrompt( InteractionRequest request )
	{
		if ( HasCabinet )
			return "Armario ya instalado";

		var inv = request.InteractorObject?.Components.GetInDescendantsOrSelf<InventoryComponent>();
		var count = inv?.GetCount( CabinetItemId ) ?? 0;

		return count > 0
			? $"Instalar armario de reclamo (tienes {count})"
			: "Instalar armario de reclamo (no llevas ninguno)";
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
			RequestInstallOnHost( request.InteractorObject?.Id ?? Guid.Empty );
			return;
		}

		ProcessInstall( request.InteractorObject );
	}

	[Rpc.Host]
	private void RequestInstallOnHost( Guid interactorObjectId )
	{
		var interactor = Scene.Directory.FindByGuid( interactorObjectId );
		ProcessInstall( interactor );
	}

	private void ProcessInstall( GameObject interactor )
	{
		if ( interactor is null || !Networking.IsHost || HasCabinet )
			return;

		if ( Vector3.DistanceBetween( interactor.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
			return;

		var inventory = interactor.Components.GetInDescendantsOrSelf<InventoryComponent>();
		if ( inventory is null || !inventory.TryRemove( CabinetItemId, 1 ) )
			return;

		_cabinet = SpawnCabinet();

		Persistence.PersistenceBridge.RequestSave();
		UI.PlayerFeedback.Push( "Armario de reclamo instalado" );
		Log.Info( $"UB.Property CabinetInstalled property={PropertyId}" );
	}

	private ClaimCabinetComponent SpawnCabinet()
	{
		var cabinetGo = new GameObject( true, $"ClaimCabinet_{PropertyId}" )
		{
			Parent = GameObject.Parent ?? GameObject,
			WorldPosition = GameObject.WorldPosition,
			WorldRotation = GameObject.WorldRotation
		};

		cabinetGo.Components.Create<BoxCollider>();
		var cabinet = cabinetGo.Components.Create<ClaimCabinetComponent>();
		cabinet.PropertyId = PropertyId;

		if ( Networking.IsActive )
			cabinetGo.NetworkSpawn();

		return cabinet;
	}

	/// <summary>Restaura un armario guardado (host, sin consumir el ítem).</summary>
	internal void RestoreCabinet()
	{
		if ( !Networking.IsHost || HasCabinet )
			return;

		_cabinet = SpawnCabinet();
	}
}
