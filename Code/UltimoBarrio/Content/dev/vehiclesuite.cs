using Sandbox;
using System;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// VehicleLabAdapter — contrato DEV mínimo que el rig de vehículos implementa sobre el kit
	/// EXTERNO (Vehicle Physics Kit u otro; decisión de research, manifest bloque H).
	///
	/// La suite NUNCA toca el kit directamente: este adapter es el puente dev (lo construye el
	/// rig como Componente sobre el prefab del vehículo). Clase abstracta (no interfaz) para que
	/// Scene.GetAllComponents&lt;VehicleLabAdapter&gt;() resuelva sin ambigüedad de constraints.
	/// </summary>
	public abstract class VehicleLabAdapter : Component
	{
		public abstract bool IsOccupied { get; }
		public abstract float Speed { get; }
		public abstract Vector3 Position { get; }
		public abstract Rotation Rotation { get; }

		public abstract bool CanEnter { get; }
		public abstract bool Enter();
		public abstract void Exit();

		/// <summary>Aceleración/retroceso, -1..1 (ruta real del kit).</summary>
		public abstract void SetThrottle( float axis );
		/// <summary>Dirección, -1..1 (ruta real del kit).</summary>
		public abstract void SetSteer( float axis );
		public abstract void SetBrake( bool on );
	}

	/// <summary>Configuración de un test de vehículo dentro de la suite del rig (data-driven).</summary>
	public sealed class VehicleTestEntry
	{
		public string Label { get; set; } = "";
		public float ThrottleSeconds { get; set; } = 2f;
		public float MinPositionDelta { get; set; } = 100f;
		public float SteerSeconds { get; set; } = 1.5f;
		public float SteerAmount { get; set; } = 1f;
		public float MinYawDelta { get; set; } = 15f;
		public float BrakeSeconds { get; set; } = 2f;
		public float MaxSpeedAfterBrake { get; set; } = 10f;
		public float ReverseSeconds { get; set; } = 1.5f;
		public float MinReverseDelta { get; set; } = 50f;
	}

	/// <summary>
	/// VehicleSuite — suite ILabSuite del dominio Vehicle (CONTRATO foundation, infra QA, SOLO dev).
	///
	/// Contrato canónico (pasos con nombre canónico y criterio PASS; el rig los emite en [VehicleLab]):
	///
	///   1. Spawn    — VehicleLabAdapter presente y vehículo válido (el rig spawnea el prefab del kit)
	///   2. Enter    — CanEnter → Enter() → IsOccupied (ruta real del kit)
	///   3. Throttle — throttle real durante ThrottleSeconds → Δposición ≥ MinPositionDelta
	///   4. Steer    — steer real durante SteerSeconds → Δyaw ≥ MinYawDelta
	///   5. Brake    — brake real durante BrakeSeconds → Speed ≤ MaxSpeedAfterBrake
	///   6. Reverse  — throttle negativo real → Δposición ≥ MinReverseDelta
	///   7. Exit     — Exit() → !IsOccupied
	///
	/// Anti-falsificación: el movimiento se mide por la ruta REAL del kit (posición/rotación/velocidad
	/// tras cada input); la suite NUNCA fija posiciones ni llama internals del kit. El adapter es el
	/// único punto de contacto (el rig lo implementa sobre el kit elegido).
	///
	/// Estado actual: el kit EXTERNO aún no está decidido (manifest bloque H) → la suite emite SKIP
	/// honesto hasta que el rig integre un VehicleLabAdapter en la escena. El contrato queda
	/// ejecutable sin cambios de código: basta spawnear el vehículo con su adapter.
	///
	/// Registro (contrato del rig): el rig crea la suite con la escena y la registra en
	/// ContentRuntimeSuite; la suite localiza el adapter con Scene.GetAllComponents&lt;VehicleLabAdapter&gt;().
	/// </summary>
	public sealed class VehicleSuite : ILabSuite
	{
		/// <summary>Pasos canónicos del contrato foundation (orden exacto; el rig los emite en [VehicleLab]).</summary>
		public enum VehicleStep
		{
			Spawn,
			Enter,
			Throttle,
			Steer,
			Brake,
			Reverse,
			Exit
		}

		public string Domain => "Vehicle";
		public string Name => _entry.Label;
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private readonly VehicleTestEntry _entry;
		private readonly Scene _scene;
		private readonly VehicleLabAdapter _adapterOverride;

		private VehicleLabAdapter _adapter;
		private VehicleStep _step;
		private TimeSince _timer;
		private float _elapsed;
		private float _positionDelta;
		private Vector3 _startPos;
		private float _yawBeforeSteer;
		private float _yawAfterSteer;
		private Vector3 _reverseStartPos;
		private bool _throttleApplied;
		private bool _steerApplied;
		private bool _brakeApplied;
		private bool _reverseApplied;

		public VehicleSuite( VehicleTestEntry entry, Scene scene, VehicleLabAdapter adapter = null )
		{
			_entry = entry ?? throw new ArgumentNullException( nameof( entry ) );
			_scene = scene ?? throw new ArgumentNullException( nameof( scene ) );
			_adapterOverride = adapter;
		}

		public void Initialize()
		{
			_step = VehicleStep.Spawn;
			_timer = 0f;
			_elapsed = 0f;
			_positionDelta = 0f;
			_throttleApplied = false;
			_steerApplied = false;
			_brakeApplied = false;
			_reverseApplied = false;

			// SKIP honesto: el kit de vehículos aún no está integrado en la sesión.
			_adapter = _adapterOverride;
			if ( _adapter == null )
			{
				foreach ( var a in _scene.GetAllComponents<VehicleLabAdapter>() )
				{
					_adapter = a;
					break;
				}
			}

			if ( _adapter == null )
			{
				Skip( "kit de vehículos no integrado (VehicleLabAdapter ausente — contrato Worker D, manifest bloque H)" );
				return;
			}

			Log.Info( $"[VehicleLab] === Suite: {_entry.Label} — contrato foundation (7 pasos) ===" );
		}

		public void Step( float dt )
		{
			if ( IsComplete ) return;

			_elapsed += dt;
			if ( _elapsed > 60f )
			{
				Fail( "timeout 60s (contrato foundation incompleto)" );
				return;
			}

			switch ( _step )
			{
				case VehicleStep.Spawn: if ( _timer >= 0.5f ) RunSpawn(); break;
				case VehicleStep.Enter: if ( _timer >= 0.3f ) RunEnter(); break;
				case VehicleStep.Throttle: RunThrottle(); break;
				case VehicleStep.Steer: RunSteer(); break;
				case VehicleStep.Brake: RunBrake(); break;
				case VehicleStep.Reverse: RunReverse(); break;
				case VehicleStep.Exit: if ( _timer >= 0.3f ) RunExit(); break;
			}
		}

		// ---------- Pasos canónicos ----------

		private void RunSpawn()
		{
			if ( _adapter == null || !_adapter.GameObject.IsValid )
			{
				Fail( "Spawn: vehículo/adapter no válido (el rig debe spawnear el prefab del kit)" );
				return;
			}

			_startPos = _adapter.Position;
			Log.Info( $"[VehicleLab] {_entry.Label} Spawn PASS pos={_startPos}" );
			NextStep();
		}

		private void RunEnter()
		{
			if ( !_adapter.CanEnter )
			{
				Fail( "Enter: CanEnter=false (ruta real del kit)" );
				return;
			}
			if ( !_adapter.Enter() || !_adapter.IsOccupied )
			{
				Fail( "Enter: Enter() no dejó el vehículo ocupado" );
				return;
			}

			Log.Info( $"[VehicleLab] {_entry.Label} Enter PASS" );
			NextStep();
		}

		private void RunThrottle()
		{
			if ( !_throttleApplied )
			{
				_startPos = _adapter.Position;
				_adapter.SetThrottle( 1f );
				_throttleApplied = true;
				_timer = 0f;
				return;
			}

			if ( _timer < _entry.ThrottleSeconds ) return;

			_adapter.SetThrottle( 0f );
			_positionDelta = Vector3.DistanceBetween( _startPos, _adapter.Position );
			bool pass = _positionDelta >= _entry.MinPositionDelta;
			Log.Info( $"[VehicleLab] {_entry.Label} Throttle Δpos={_positionDelta:F1}u ≥ {_entry.MinPositionDelta:F0} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Throttle: Δpos {_positionDelta:F1} < {_entry.MinPositionDelta:F0} (sin movimiento real)" ); return; }

			_yawBeforeSteer = _adapter.Rotation.Yaw();
			NextStep();
		}

		private void RunSteer()
		{
			if ( !_steerApplied )
			{
				_adapter.SetThrottle( 0.5f );
				_adapter.SetSteer( _entry.SteerAmount );
				_steerApplied = true;
				_timer = 0f;
				return;
			}

			if ( _timer < _entry.SteerSeconds ) return;

			_adapter.SetThrottle( 0f );
			_adapter.SetSteer( 0f );
			_yawAfterSteer = _adapter.Rotation.Yaw();
			float yawDelta = AngleDelta( _yawBeforeSteer, _yawAfterSteer );
			bool pass = yawDelta >= _entry.MinYawDelta;
			Log.Info( $"[VehicleLab] {_entry.Label} Steer Δyaw={yawDelta:F1}° ≥ {_entry.MinYawDelta:F0} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Steer: Δyaw {yawDelta:F1} < {_entry.MinYawDelta:F0} (sin giro real)" ); return; }

			_reverseStartPos = _adapter.Position;
			NextStep();
		}

		private void RunBrake()
		{
			if ( !_brakeApplied )
			{
				_adapter.SetBrake( true );
				_brakeApplied = true;
				_timer = 0f;
				return;
			}

			if ( _timer < _entry.BrakeSeconds ) return;

			_adapter.SetBrake( false );
			float speed = _adapter.Speed;
			bool pass = speed <= _entry.MaxSpeedAfterBrake;
			Log.Info( $"[VehicleLab] {_entry.Label} Brake speed={speed:F1}u/s ≤ {_entry.MaxSpeedAfterBrake:F0} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Brake: speed {speed:F1} > {_entry.MaxSpeedAfterBrake:F0}" ); return; }

			NextStep();
		}

		private void RunReverse()
		{
			if ( !_reverseApplied )
			{
				_reverseStartPos = _adapter.Position;
				_adapter.SetThrottle( -1f );
				_reverseApplied = true;
				_timer = 0f;
				return;
			}

			if ( _timer < _entry.ReverseSeconds ) return;

			_adapter.SetThrottle( 0f );
			float delta = Vector3.DistanceBetween( _reverseStartPos, _adapter.Position );
			bool pass = delta >= _entry.MinReverseDelta;
			Log.Info( $"[VehicleLab] {_entry.Label} Reverse Δpos={delta:F1}u ≥ {_entry.MinReverseDelta:F0} {(pass ? "PASS" : "FAIL")}" );
			if ( !pass ) { Fail( $"Reverse: Δpos {delta:F1} < {_entry.MinReverseDelta:F0}" ); return; }

			NextStep();
		}

		private void RunExit()
		{
			_adapter.Exit();
			if ( _adapter.IsOccupied )
			{
				Fail( "Exit: el vehículo sigue ocupado tras Exit()" );
				return;
			}

			Log.Info( $"[VehicleLab] {_entry.Label} Exit PASS" );
			Finish();
		}

		// ---------- Helpers ----------

		private void NextStep()
		{
			_step++;
			_timer = 0f;
		}

		private void Skip( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Skip( reason );
		}

		private void Fail( string reason )
		{
			IsComplete = true;
			Result = LabSuiteResult.Fail( _elapsed, _positionDelta, $"fail:{reason}" );
			Log.Error( $"[VehicleLab] {_entry.Label} FAIL ({reason})" );
		}

		private void Finish()
		{
			Log.Info( $"[VehicleLab] {_entry.Label} PASS (contrato foundation 7/7)" );
			IsComplete = true;
			Result = LabSuiteResult.Pass( _elapsed, _positionDelta, "complete" );
		}

		private static float AngleDelta( float a, float b )
		{
			float d = MathF.Abs( a - b );
			return d > 180f ? 360f - d : d;
		}
	}
}
