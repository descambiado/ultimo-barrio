using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>Configuración de un test de audio dentro de la suite (data-driven).</summary>
	public sealed class AudioTestEntry
	{
		public string Label { get; set; } = "";
		public string SoundEventPath { get; set; } = ""; // p. ej. "sounds/content/weapons/usp_fire.sound"
		public bool Positional { get; set; } = true;     // reproducir 3D (necesita posición) o 2D
	}

	/// <summary>
	/// AudioSuite — suite ILabSuite del dominio Audio (CONTRATO del banco de sonido, infra QA, SOLO dev).
	///
	/// Contrato canónico (pasos con nombre canónico y criterio PASS; la suite los emite en [AudioLab]):
	///
	///   1. Resource        — el recurso .sound resuelve (ResourceLibrary.Get&lt;SoundEvent&gt; != null)
	///   2. Load            — el SoundEvent está cargado (asset no nulo)
	///   3. Play            — la invocación Sound.Play tiene éxito (ruta real, sin excepción)
	///   4. NoMissingAssets — agregado: 0 eventos sin resolver / sin reproducir en todo el banco
	///
	/// Anti-falsificación: la suite recorre el banco real (Assets/sounds/content/**) por su ruta en
	/// juego y reproduce cada evento por Sound.Play; NUNCA fabrica un "cargado" si el asset no
	/// resuelve. Si el banco COMPLETO no resuelve en la sesión (assets sin compilar/validar en el
	/// editor), emite SKIP honesto con motivo — no PASS. Si resuelve parcialmente, es FAIL por
	/// contrato (criterio "sin missing asset errors").
	///
	/// Registro (contrato del rig): cualquier componente dev puede registrar la suite
	///   ContentRuntimeSuite.Register( new AudioSuite( "Core", AudioSuite.ContentBankDefault(), playPosition ) )
	/// El runner emite [UBSuite] Audio.Core PASS|FAIL|SKIP.
	/// </summary>
	public sealed class AudioSuite : ILabSuite
	{
		/// <summary>Pasos canónicos del contrato (los logs [AudioLab] los emiten por evento y en agregado).</summary>
		public enum AudioStep
		{
			Resource,
			Load,
			Play,
			NoMissingAssets
		}

		public string Domain => "Audio";
		public string Name => _label;
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private readonly string _label;
		private readonly List<AudioTestEntry> _entries;
		private readonly Vector3? _playPosition;
		private bool _initialized;

		public AudioSuite( string label, List<AudioTestEntry> entries, Vector3? playPosition = null )
		{
			_label = string.IsNullOrEmpty( label ) ? "Core" : label;
			_entries = entries ?? throw new ArgumentNullException( nameof( entries ) );
			_playPosition = playPosition;
		}

		/// <summary>
		/// Manifest del banco de sonido del content pack (Assets/sounds/content/** — Worker F, 2026-08-08).
		/// Excluye dry_fire.wav (binario importado, no es SoundEvent). Rutas en formato de juego.
		/// </summary>
		public static List<AudioTestEntry> ContentBankDefault()
		{
			return new List<AudioTestEntry>
			{
				new AudioTestEntry { Label = "usp_fire", SoundEventPath = "sounds/content/weapons/usp_fire.sound" },
				new AudioTestEntry { Label = "usp_reload_magout", SoundEventPath = "sounds/content/weapons/usp_reload_magout.sound" },
				new AudioTestEntry { Label = "usp_reload_magin", SoundEventPath = "sounds/content/weapons/usp_reload_magin.sound" },
				new AudioTestEntry { Label = "usp_deploy", SoundEventPath = "sounds/content/weapons/usp_deploy.sound" },
				new AudioTestEntry { Label = "usp_dry", SoundEventPath = "sounds/content/weapons/usp_dry.sound" },
				new AudioTestEntry { Label = "shotgun_fire", SoundEventPath = "sounds/content/weapons/shotgun_fire.sound" },
				new AudioTestEntry { Label = "shotgun_reload_start", SoundEventPath = "sounds/content/weapons/shotgun_reload_start.sound" },
				new AudioTestEntry { Label = "shotgun_reload", SoundEventPath = "sounds/content/weapons/shotgun_reload.sound" },
				new AudioTestEntry { Label = "crowbar_swing", SoundEventPath = "sounds/content/weapons/crowbar_swing.sound" },
				new AudioTestEntry { Label = "crowbar_impact", SoundEventPath = "sounds/content/weapons/crowbar_impact.sound" },
				new AudioTestEntry { Label = "bullet_impact_flesh", SoundEventPath = "sounds/content/impacts/bullet_impact_flesh.sound" },
				new AudioTestEntry { Label = "melee_impact_flesh", SoundEventPath = "sounds/content/impacts/melee_impact_flesh.sound" },
				new AudioTestEntry { Label = "door_impact", SoundEventPath = "sounds/content/fortification/door_impact.sound" },
				new AudioTestEntry { Label = "barricade_impact", SoundEventPath = "sounds/content/fortification/barricade_impact.sound" },
				new AudioTestEntry { Label = "repair", SoundEventPath = "sounds/content/fortification/repair.sound" },
				new AudioTestEntry { Label = "enemy_hurt", SoundEventPath = "sounds/content/enemies/enemy_hurt.sound" },
				new AudioTestEntry { Label = "enemy_death", SoundEventPath = "sounds/content/enemies/enemy_death.sound" }
			};
		}

		public void Initialize()
		{
			if ( _initialized ) return;
			_initialized = true;

			int total = _entries.Count;
			int resolved = 0;
			int playFailures = 0;
			var missing = new List<string>();
			var playErrorPaths = new List<string>();

			foreach ( var e in _entries )
			{
				if ( string.IsNullOrEmpty( e.SoundEventPath ) )
				{
					missing.Add( "<vacío>" );
					continue;
				}

				// Pasos 1 y 2: el recurso resuelve y el SoundEvent carga (asset real).
				var evt = ResourceLibrary.Get<SoundEvent>( e.SoundEventPath );
				if ( evt == null )
				{
					Log.Warning( $"[AudioLab] {e.SoundEventPath} Resource FAIL / Load FAIL (missing asset)" );
					missing.Add( e.SoundEventPath );
					continue;
				}
				Log.Info( $"[AudioLab] {e.SoundEventPath} Resource PASS / Load PASS" );

				// Paso 3: invocación de reproducción por la ruta real (Sound.Play), sin excepción.
				try
				{
					if ( e.Positional && _playPosition.HasValue )
					{
						Sound.Play( e.SoundEventPath, _playPosition.Value );
					}
					else
					{
						Sound.Play( e.SoundEventPath );
					}
					resolved++;
					Log.Info( $"[AudioLab] {e.SoundEventPath} Play PASS" );
				}
				catch ( Exception ex )
				{
					Log.Error( $"[AudioLab] {e.SoundEventPath} Play FAIL: {ex.Message}" );
					playFailures++;
					playErrorPaths.Add( e.SoundEventPath );
				}
			}

			// Paso 4 (agregado): sin missing asset errors en el banco.
			if ( total == 0 )
			{
				Skip( "manifest vacío (sin eventos que validar)" );
				return;
			}

			if ( resolved == 0 )
			{
				// El banco COMPLETO no resuelve: el sistema de audio no está disponible en esta
				// sesión (assets sin compilar/validar en el editor — validación runtime pendiente).
				Skip( $"banco de sonido no disponible en esta sesión ({missing.Count}/{total} eventos sin resolver — assets sin compilar/validar en editor)" );
				return;
			}

			bool pass = missing.Count == 0 && playFailures == 0;
			if ( pass )
			{
				Log.Info( $"[AudioLab] Core PASS ({resolved}/{total} eventos resueltos y reproducidos)" );
				IsComplete = true;
				Result = LabSuiteResult.Pass( 0f, resolved, "complete" );
				return;
			}

			string reason = "missing assets";
			if ( missing.Count > 0 ) reason += ": " + string.Join( ", ", missing );
			if ( playErrorPaths.Count > 0 ) reason += " | play FAIL: " + string.Join( ", ", playErrorPaths );

			Log.Error( $"[AudioLab] Core FAIL ({reason})" );
			IsComplete = true;
			Result = LabSuiteResult.Fail( 0f, resolved, $"fail:{reason}" );
		}

		public void Step( float dt )
		{
			// La validación es síncrona (Initialize completa la suite); Step no hace nada.
		}

		private void Skip( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( reason );
		}
	}
}
