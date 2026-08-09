using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Registro estático de suites de laboratorio (infra QA, SOLO dev).
	///
	/// Los rigs registran sus suites aquí (WeaponSuite, y en el futuro EnemySuite,
	/// BuildingSuite, VehicleSuite de los workers A/C/E); el runner las ejecuta en
	/// orden de registro y emite una línea machine-readable por suite:
	///   [UBSuite] &lt;Domain&gt;.&lt;Name&gt; PASS|FAIL|SKIP time=&lt;s&gt;s delta=&lt;v&gt; state=&lt;estado&gt;
	/// </summary>
	public static class ContentRuntimeSuite
	{
		private static readonly List<ILabSuite> _registered = new();

		public static IReadOnlyList<ILabSuite> Registered => _registered;

		public static void Register( ILabSuite suite )
		{
			if ( suite == null ) return;
			if ( !_registered.Contains( suite ) ) _registered.Add( suite );
		}

		public static void Unregister( ILabSuite suite )
		{
			_registered.Remove( suite );
		}

		public static void Clear()
		{
			_registered.Clear();
		}

		/// <summary>
		/// Guard: solo UN runner por sesión ejecuta el registro (evita duplicar
		/// logs si varios rigs crean runners en la misma escena).
		/// </summary>
		public static bool RunnerActive { get; set; }
	}

	/// <summary>
	/// Runner unificado de la mini-suite: itera las suites registradas en
	/// <see cref="ContentRuntimeSuite"/>, llama Initialize → Step(dt) cada frame y
	/// emite la salida machine-readable [UBSuite]. Infra de test, SOLO dev.
	///
	/// Anti-falsificación: el runner NUNCA fabrica resultados. Si Initialize o Step
	/// lanzan excepción, la suite se reporta FAIL con el error; el PASS solo proviene
	/// de ILabSuite.Result cuando la suite completó su ruta real (nunca un Success viejo).
	/// </summary>
	[Title( "Content Runtime Suite Runner" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "fact_check" )]
	public sealed class ContentRuntimeSuiteRunner : Component
	{
		private readonly List<ILabSuite> _suites = new();
		private readonly List<bool> _initialized = new();
		private int _current;
		private bool _finished;
		private int _passes;
		private int _fails;
		private int _skips;

		public bool IsFinished => _finished;
		public int PassCount => _passes;
		public int FailCount => _fails;
		public int SkipCount => _skips;

		protected override void OnStart()
		{
			if ( ContentRuntimeSuite.RunnerActive )
			{
				Log.Warning( "[UBSuite] Runner ya activo en la sesión; este runner se ignora" );
				Enabled = false;
				return;
			}

			ContentRuntimeSuite.RunnerActive = true;

			foreach ( var suite in ContentRuntimeSuite.Registered )
			{
				_suites.Add( suite );
				_initialized.Add( false );
			}

			if ( _suites.Count == 0 )
			{
				Log.Warning( "[UBSuite] No hay suites registradas en ContentRuntimeSuite" );
				ContentRuntimeSuite.RunnerActive = false;
				Enabled = false;
				return;
			}

			Log.Info( $"[UBSuite] Runner: {_suites.Count} suite(s) registrada(s)" );
		}

		protected override void OnDestroy()
		{
			ContentRuntimeSuite.RunnerActive = false;

			// Lifecycle del registro: las suites pertenecen a la sesión de juego que
			// las registró. Si sobreviven al play_stop/reload, el runner de la sesión
			// siguiente las re-ejecuta con referencias muertas y emite resultados
			// stale (IsComplete/Result viejos) → duplicación y FAILs fantasma.
			ContentRuntimeSuite.Clear();
		}

		protected override void OnUpdate()
		{
			if ( _finished ) return;

			if ( _current >= _suites.Count )
			{
				FinishRun();
				return;
			}

			var suite = _suites[_current];

			try
			{
				if ( !_initialized[_current] )
				{
					_initialized[_current] = true;
					suite.Initialize();
				}
				else if ( !suite.IsComplete )
				{
					suite.Step( Time.Delta );
				}
			}
			catch ( Exception e )
			{
				Log.Error( $"[UBSuite] {suite.Domain}.{suite.Name} EXCEPCIÓN: {e.Message}" );
				EmitResult( suite, LabSuiteResult.Fail( 0f, 0f, $"error:{e.Message}" ) );
				_current++;
				return;
			}

			if ( suite.IsComplete )
			{
				EmitResult( suite, suite.Result );
				_current++;
			}
		}

		private void EmitResult( ILabSuite suite, LabSuiteResult result )
		{
			switch ( result.Status )
			{
				case LabSuiteStatus.Pass: _passes++; break;
				case LabSuiteStatus.Fail: _fails++; break;
				case LabSuiteStatus.Skip: _skips++; break;
			}

			Log.Info( $"[UBSuite] {suite.Domain}.{suite.Name} {result.Status.ToString().ToUpperInvariant()} {result.Detail}" );
		}

		private void FinishRun()
		{
			_finished = true;
			Log.Info( $"[UBSuite] Suite run complete ({_passes} PASS, {_fails} FAIL, {_skips} SKIP)" );

			// El run ha consumido las suites: limpiar el registro para que la
			// próxima sesión arranque solo con sus propios rigs (4 suites, no 8).
			ContentRuntimeSuite.Clear();
		}
	}
}
