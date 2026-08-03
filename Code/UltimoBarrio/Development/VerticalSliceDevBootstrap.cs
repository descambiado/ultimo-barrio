using Sandbox;
using System.Linq;

namespace UltimoBarrio.Development
{
	[Title( "Vertical Slice Dev Bootstrap" )]
	[Category( "Development" )]
	[Icon( "bug_report" )]
	public sealed class VerticalSliceDevBootstrap : Component
	{
		[Property] public bool EnabledInDevelopment { get; set; } = true;

		// We can add references to Prefabs here later to instantiate them
		// [Property] public GameObjectPrefab TraderPrefab { get; set; }

		protected override void OnStart()
		{
			base.OnStart();

			if ( !EnabledInDevelopment )
				return;

			// Dev bootstrap logic here (e.g., spawning test traders, setting time of day).
			// Do NOT clone apartments dynamically. They must be physically present in the scene.
		}
	}
}
