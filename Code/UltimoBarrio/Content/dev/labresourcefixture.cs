using Sandbox;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Fixture de recursos para los labs (SOLO dev). Sustituye el inventario canónico
	/// (que aún no existe) en la ruta de reparación: BuildStructureHost.Repair consume
	/// vía delegado (Func&lt;int,bool&gt;). El core nuevo conectará su inventario real por
	/// el mismo delegado.
	/// </summary>
	[Title( "Lab Resource Fixture" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class LabResourceFixture : Component
	{
		public int Balance { get; private set; }

		public void SetBalance( int amount ) => Balance = amount;

		public bool TryConsume( int amount )
		{
			if ( amount < 0 || Balance < amount ) return false;

			Balance -= amount;
			Log.Info( $"[BuildingLab] fixture consumed {amount} → balance {Balance}" );
			return true;
		}
	}
}
