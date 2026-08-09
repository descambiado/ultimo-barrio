using Sandbox;
using System.Linq;

namespace UltimoBarrio.WorldTime
{
	/// <summary>
	/// World Time Lighting — hace VISIBLE el ciclo día/noche del WorldClock.
	///
	/// Sin middleware nuevo: ajusta las luces que ya existen en la escena (incluidas
	/// las del mapa montado por MapInstance) según la fase del reloj:
	///   - DirectionalLight.LightColor (API confirmada: addons/tools AssetPreview)
	///   - SceneWorld.AmbientLightColor (API confirmada: addons/tools AssetPreview)
	/// El HUD ya muestra la fase; esto añade el cambio del mundo.
	/// </summary>
	[Title( "World Time Lighting" )]
	[Category( "Ultimo Barrio — World Time" )]
	[Icon( "wb_twilight" )]
	public sealed class WorldTimeLighting : Component
	{
		[Property] public WorldClock Clock { get; set; }

		[Property] public Color DayColor { get; set; } = new Color( 1f, 0.95f, 0.85f );
		[Property] public Color PreparationColor { get; set; } = new Color( 1f, 0.6f, 0.35f );
		[Property] public Color NightColor { get; set; } = new Color( 0.25f, 0.3f, 0.55f );
		[Property] public Color AftermathColor { get; set; } = new Color( 0.6f, 0.6f, 0.75f );

		[Property] public Color DayAmbient { get; set; } = new Color( 0.5f, 0.52f, 0.55f );
		[Property] public Color NightAmbient { get; set; } = new Color( 0.07f, 0.09f, 0.17f );

		protected override void OnStart()
		{
			if ( Clock == null )
			{
				Clock = Scene.GetAllComponents<WorldClock>().FirstOrDefault();
			}

			if ( Clock != null )
			{
				Clock.OnPhaseChanged += HandlePhase;
			}

			HandlePhase( Clock != null ? Clock.CurrentPhase : TimePhase.Day );
		}

		protected override void OnDestroy()
		{
			if ( Clock != null )
			{
				Clock.OnPhaseChanged -= HandlePhase;
			}
		}

		private void HandlePhase( TimePhase phase )
		{
			Color color = phase switch
			{
				TimePhase.Day => DayColor,
				TimePhase.Preparation => PreparationColor,
				TimePhase.Night => NightColor,
				_ => AftermathColor
			};

			Color ambient = phase switch
			{
				TimePhase.Day => DayAmbient,
				TimePhase.Preparation => new Color( 0.3f, 0.22f, 0.16f ),
				TimePhase.Night => NightAmbient,
				_ => new Color( 0.28f, 0.28f, 0.35f )
			};

			int adjusted = 0;
			foreach ( var sun in Scene.GetAllComponents<DirectionalLight>() )
			{
				sun.LightColor = color;
				adjusted++;
			}

			Scene.SceneWorld.AmbientLightColor = ambient;

			Log.Info( $"[WorldTime] Fase {phase}: luz {color} ambiente {ambient} ({adjusted} direccionales ajustadas)" );
		}
	}
}
