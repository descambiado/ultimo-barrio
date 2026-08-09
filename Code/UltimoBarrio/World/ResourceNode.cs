using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.World
{
	[Title( "Resource Node" )]
	[Category( "Ultimo Barrio — World" )]
	[Icon( "grid_view" )]
	public class ResourceNode : Component, IWorldInteractable
	{
		[Property] public string ItemId { get; set; } = "chatarra";
		[Property] public int Amount { get; set; } = 1;
		[Property] public float RespawnTime { get; set; } = 30f;

		[Sync( SyncFlags.FromHost )] public bool IsAvailable { get; private set; } = true;

		private TimeSince timeSinceCollected;

		protected override void OnStart()
		{
			if ( Networking.IsHost )
			{
				IsAvailable = true;
			}
		}

		protected override void OnUpdate()
		{
			if ( !Networking.IsHost ) return;

			if ( !IsAvailable && timeSinceCollected > RespawnTime )
			{
				IsAvailable = true;
				RpcSetVisibility( true );
			}
		}

		public bool TryHarvest( GameObject harvester, out int harvestedAmount )
		{
			harvestedAmount = 0;
			if ( !Networking.IsHost ) return false;
			if ( !IsAvailable ) return false;

			IsAvailable = false;
			timeSinceCollected = 0f;
			harvestedAmount = Amount;
			RpcSetVisibility( false );
			return true;
		}

		// --- IWorldInteractable: el jugador recolecta con E (ruta real del interactor) ---

		public string GetInteractionPrompt( InteractionRequest request )
		{
			return IsAvailable ? $"Recolectar {ItemId} (x{Amount})" : "Agotado (respawn pendiente)";
		}

		public bool CanInteract( InteractionRequest request )
		{
			return request.InteractorObject != null && IsAvailable;
		}

		public void OnInteract( InteractionRequest request )
		{
			if ( IsProxy )
			{
				RequestHarvestOnHost( request.InteractorObject?.Id ?? Guid.Empty );
				return;
			}

			ProcessHarvest( request.InteractorObject );
		}

		[Rpc.Host]
		private void RequestHarvestOnHost( Guid interactorObjectId )
		{
			var interactorGo = Scene.Directory.FindByGuid( interactorObjectId );
			ProcessHarvest( interactorGo );
		}

		private void ProcessHarvest( GameObject interactorGo )
		{
			if ( interactorGo == null || !interactorGo.IsValid() ) return;

			if ( TryHarvest( interactorGo, out int harvestedAmount ) )
			{
				var inventory = interactorGo.Components.Get<InventoryComponent>();
				if ( inventory != null && inventory.TryAdd( ItemId, harvestedAmount ) )
				{
					Log.Info( $"[ResourceNode] {interactorGo.Name} recolectó {harvestedAmount}x{ItemId}" );
				}
			}
		}

		[Rpc.Broadcast]
		private void RpcSetVisibility( bool visible )
		{
			var mr = Components.Get<ModelRenderer>();
			if ( mr != null ) mr.Enabled = visible;

			var col = Components.Get<Collider>();
			if ( col != null ) col.Enabled = visible;
		}
	}
}
