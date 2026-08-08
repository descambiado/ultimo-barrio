using Sandbox;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Pickup de botín FÍSICO del enemy content pack (objeto de mundo).
	/// El host lo instancia al morir; el jugador lo coge como objeto de mundo
	/// (el core nuevo decidirá cómo entra al inventario). NUNCA inventario canónico.
	/// ItemId/Amount son strings opacos para el mapeo del core nuevo.
	/// </summary>
	[Title( "Loot Pickup Content" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "inventory_2" )]
	public sealed class LootPickupContent : Component
	{
		[Property] public string ItemId { get; set; } = "";
		[Property] public int Amount { get; set; } = 1;

		protected override void OnStart()
		{
			GameObject.Tags.Add( "loot_pickup" );
		}
	}
}
