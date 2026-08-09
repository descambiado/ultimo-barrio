using Sandbox;
using System;
using UltimoBarrio.Core;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Pickup de botín FÍSICO del enemy content pack (objeto de mundo).
	///
	/// El host lo instancia al morir el enemigo. Es interactuable (IWorldInteractable):
	/// el jugador lo recoge con E y el ItemId/Amount entran a su InventoryComponent
	/// (ruta real del inventario del juego). ItemId es un item del ItemRegistry
	/// ("chatarra", "ammo_9mm", ...).
	/// </summary>
	[Title( "Loot Pickup Content" )]
	[Category( "Ultimo Barrio — Content" )]
	[Icon( "inventory_2" )]
	public sealed class LootPickupContent : Component, IWorldInteractable
	{
		[Property] public string ItemId { get; set; } = "";
		[Property] public int Amount { get; set; } = 1;
		[Property] public float MaxInteractionDistance { get; set; } = 200f;

		protected override void OnStart()
		{
			GameObject.Tags.Add( "loot_pickup" );
		}

		public string GetInteractionPrompt( InteractionRequest request )
		{
			return $"Recoger {ItemId} (x{Amount})";
		}

		public bool CanInteract( InteractionRequest request )
		{
			return request.InteractorObject != null
				&& Vector3.DistanceBetween( request.InteractorObject.WorldPosition, GameObject.WorldPosition ) <= MaxInteractionDistance;
		}

		public void OnInteract( InteractionRequest request )
		{
			if ( IsProxy )
			{
				RequestPickupOnHost( request.InteractorObject?.Id ?? Guid.Empty );
			}
			else
			{
				ProcessPickup( request.InteractorObject );
			}
		}

		[Rpc.Host]
		private void RequestPickupOnHost( Guid interactorObjectId )
		{
			var interactorGo = Scene.Directory.FindByGuid( interactorObjectId );
			ProcessPickup( interactorGo );
		}

		private void ProcessPickup( GameObject interactorGo )
		{
			if ( interactorGo == null || !interactorGo.IsValid() ) return;

			if ( Vector3.DistanceBetween( interactorGo.WorldPosition, GameObject.WorldPosition ) > MaxInteractionDistance )
			{
				Log.Warning( $"[Loot] {interactorGo.Name} intentó recoger desde demasiado lejos." );
				return;
			}

			var inventory = interactorGo.Components.Get<InventoryComponent>();
			if ( inventory != null && inventory.TryAdd( ItemId, Amount ) )
			{
				Log.Info( $"[Loot] {interactorGo.Name} recogió {Amount}x{ItemId}" );
				GameObject.Destroy();
			}
		}
	}
}
