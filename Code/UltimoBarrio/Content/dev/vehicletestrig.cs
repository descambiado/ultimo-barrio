using Sandbox;
using System;
using System.Linq;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Vehicle Test Rig â€” fixture automatizado del vehicle_lab (SOLO dev).
	/// Sustituye input humano y conductor humano; el movimiento recorre SIEMPRE la
	/// ruta real del kit externo PRIMARY (fieldguide.vehiclephysics):
	///
	///   spawn (prefab del kit) â†’ components vÃ¡lidos â†’ driver fixture entra (attach)
	///   â†’ throttle â†’ steering (yaw) â†’ brake (reduce velocidad) â†’ reverse (signo)
	///   â†’ exit (detach) â†’ PASS
	///
	/// Anti-falsificaciÃ³n: PASS solo por deltas reales (posiciÃ³n / yaw / velocidad por
	/// ventanas de posiciÃ³n). Sin fÃ­sica propia, sin fijar posiciones, sin internals.
	///
	/// El kit se conduce simulando sus input actions con Input.SetAction(string, bool)
	/// (API de engine verificada 26.08.05). Los NOMBRES de action son propiedades
	/// configurables en la escena (NO API inventada): confirmar contra el README del
	/// kit tras montar fieldguide.vehiclephysics y ajustarlos si difieren.
	/// </summary>
	[Title( "Vehicle Test Rig" )]
	[Category( "Ãšltimo Barrio â€” Content (Dev)" )]
	[Icon( "directions_car" )]
	public sealed class VehicleTestRig : Component
	{
		// Sube este nÃºmero en cada cambio de cÃ³digo relevante para verificar
		// que la sesiÃ³n de juego carga el assembly nuevo (detecciÃ³n de hotload atrasado).
		public const string Version = "rig-1";

		[Property] public bool AutoTest { get; set; } = true;

		/// <summary>
		/// Prefab del vehÃ­culo del kit (rellenar tras montar fieldguide.vehiclephysics:
		/// guardar el coche de vehiclephysics_demo como prefab o usar el path del paquete).
		/// </summary>
		[Property] public string VehiclePrefabPath { get; set; } = "";

		[Property] public GameObject SpawnMarker { get; set; }

		// Input actions del kit (data-driven; verificar nombres en el README del kit).
		[Property] public string ThrottleAction { get; set; } = "Forward";
		[Property] public string BrakeAction { get; set; } = "Brake";
		[Property] public string SteerLeftAction { get; set; } = "SteerLeft";
		[Property] public string SteerRightAction { get; set; } = "SteerRight";
		[Property] public string ReverseAction { get; set; } = "Reverse";

		// Umbrales de PASS (unidades s&box â‰ˆ cm).
		[Property] public float MinThrottleDelta { get; set; } = 25f;
		[Property] public float MinSteerYawDelta { get; set; } = 10f; // grados
		[Property] public float BrakeSpeedReduction { get; set; } = 0.5f; // v_after < v_before * este factor
		[Property] public float MinReverseDelta { get; set; } = 15f;

		private const int TotalSteps = 8; // spawn, components, enter, throttle, steer, brake, reverse, exit

		private CameraComponent _camera;
		private VehicleDriverFixture _driver;
		private GameObject _vehicle;
		private VehicleSuite _suite;

		private int _step;
		private TimeSince _timer;
		private int _phase;
		private TimeSince _phaseTimer;
		private int _fails;

		// Muestras de las pruebas.
		private Vector3 _sampleStart;
		private float _sampleYaw;
		private Vector3 _sampleA;
		private Vector3 _sampleB;
		private float _speedBefore;
		private float _speedAfter;
		private float _runTime;

		private bool HasVehicle => _vehicle != null && _vehicle.IsValid();

		protected override void OnStart()
		{
			Log.Info( $"[LabBuild] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Warning( "[VehicleLab] rig sin CameraComponent (la suite sigue, sin vista de cÃ¡mara)" );
			}
			else
			{
				_camera.IsMainCamera = true;
				_camera.Priority = 10;
			}

			// Suite reportera unificada con la infra QA ([UBSuite]).
			_suite = new VehicleSuite( "VehicleLab" );
			ContentRuntimeSuite.Register( _suite );
			Components.Create<ContentRuntimeSuiteRunner>();

			if ( !AutoTest ) return;

			Log.Info( "[VehicleLab] Suite: spawn â†’ components â†’ enter â†’ throttle â†’ steer â†’ brake â†’ reverse â†’ exit" );
		}

		protected override void OnUpdate()
		{
			if ( !AutoTest ) return;

			_runTime += Time.Delta;

			switch ( _step )
			{
				case 0: if ( _timer >= 1.0f ) RunSpawn(); break;
				case 1: if ( _timer >= 1.0f ) RunComponents(); break;      // 1s extra: el vehÃ­culo se asienta en el suelo
				case 2: if ( _timer >= 0.6f ) RunEnter(); break;
				case 3: RunThrottle(); break;                               // multi-fase
				case 4: RunSteer(); break;
				case 5: RunBrake(); break;
				case 6: RunReverse(); break;
				case 7: if ( _timer >= 0.6f ) RunExit(); break;
				case 8: if ( _timer >= 0.8f ) FinishSuite(); break;
			}
		}

		protected override void OnDestroy()
		{
			ReleaseAllInputs();
			if ( _suite != null ) ContentRuntimeSuite.Unregister( _suite );
		}

		private void NextStep()
		{
			_step++;
			_timer = 0f;
			_phase = 0;
			_phaseTimer = 0f;
		}

		private void SetAction( string name, bool down )
		{
			if ( !string.IsNullOrWhiteSpace( name ) ) Input.SetAction( name, down );
		}

		private void ReleaseAllInputs()
		{
			SetAction( ThrottleAction, false );
			SetAction( BrakeAction, false );
			SetAction( SteerLeftAction, false );
			SetAction( SteerRightAction, false );
			SetAction( ReverseAction, false );
		}

		private void MarkFail()
		{
			_fails++;
		}

		private void GuardSpawned()
		{
			if ( HasVehicle ) return;
			Log.Error( "[VehicleLab] vehÃ­culo no disponible (Â¿fallÃ³ el spawn?)" );
			MarkFail();
			NextStep();
		}

		// ---------------------------------------------------------------- spawn
		private void RunSpawn()
		{
			ReleaseAllInputs();

			if ( string.IsNullOrWhiteSpace( VehiclePrefabPath ) )
			{
				Log.Error( "[VehicleLab] Spawn FAIL: VehiclePrefabPath vacÃ­o â€” configurar un prefab del kit fieldguide.vehiclephysics tras montar el paquete" );
				MarkFail();
				_step = 8;
				_timer = 0f;
				return;
			}

			var prefabFile = ResourceLibrary.Get<PrefabFile>( VehiclePrefabPath );
			if ( prefabFile == null )
			{
				Log.Error( $"[VehicleLab] Spawn FAIL: PrefabFile no encontrado: {VehiclePrefabPath}" );
				MarkFail();
				_step = 8;
				_timer = 0f;
				return;
			}

			var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
			if ( prefabScene == null )
			{
				Log.Error( $"[VehicleLab] Spawn FAIL: GetPrefabScene null para {VehiclePrefabPath}" );
				MarkFail();
				_step = 8;
				_timer = 0f;
				return;
			}

			var spawnBase = SpawnMarker != null ? SpawnMarker.WorldPosition : WorldPosition;
			_vehicle = prefabScene.Clone();
			_vehicle.WorldPosition = spawnBase + Vector3.Up * 60f; // cae al suelo (floor z=0 del lab)
			_vehicle.NetworkSpawn( Connection.Local );

			Log.Info( $"[VehicleLab] Spawn OK: {VehiclePrefabPath} en {spawnBase} (root '{_vehicle.Name}')" );
			NextStep();
		}

		// ------------------------------------------------------- components vÃ¡lidos
		private void RunComponents()
		{
			GuardSpawned();
			if ( !HasVehicle ) return;

			var all = _vehicle.Components.GetAll().Where( c => c.IsValid() ).ToArray();
			foreach ( var c in all )
			{
				Log.Info( $"[VehicleLab] Component: {c.GetType().Name}" );
			}

			var rigidbody = _vehicle.Components.GetInChildrenOrSelf<Rigidbody>();
			var renderer = _vehicle.Components.GetInChildrenOrSelf<ModelRenderer>();

			bool pass = rigidbody.IsValid() && renderer.IsValid() && all.Length >= 3;
			string rbOk = rigidbody.IsValid() ? "sÃ­" : "NO";
			string mrOk = renderer.IsValid() ? "sÃ­" : "NO";
			string verdict = pass ? "PASS" : "FAIL";
			Log.Info( $"[VehicleLab] Components: {all.Length} total, Rigidbody={rbOk}, ModelRenderer={mrOk} â†’ {verdict}" );
			if ( !pass ) MarkFail();
			NextStep();
		}

		// ------------------------------------------------------------- enter/attach
		private void RunEnter()
		{
			GuardSpawned();
			if ( !HasVehicle ) return;

			var driverGo = new GameObject( true, "LabDriverFixture" );
			driverGo.WorldPosition = _vehicle.WorldPosition + Vector3.Up * 120f;

			var renderer = driverGo.Components.Create<ModelRenderer>();
			renderer.Model = ResourceLibrary.Get<Model>( "models/citizen_props/crate01.vmdl" );

			_driver = driverGo.Components.Create<VehicleDriverFixture>();

			bool attached = _driver.Enter( _vehicle, new Vector3( 0f, 0f, 40f ) );
			string parentName = _driver.GameObject.Parent?.Name ?? "null";
			Log.Info( attached
				? $"[VehicleLab] Enter attach PASS (driver en asiento, parent={parentName})"
				: "[VehicleLab] Enter attach FAIL (no se pudo parentear el driver al vehÃ­culo)" );
			if ( !attached ) MarkFail();
			NextStep();
		}

		// --------------------------------------------------------------- throttle
		private void RunThrottle()
		{
			switch ( _phase )
			{
				case 0:
					ReleaseAllInputs();
					_phase = 1;
					_phaseTimer = 0f;
					break;
				case 1: // asentar 0.5s y tomar posiciÃ³n inicial
					if ( _phaseTimer >= 0.5f )
					{
						_sampleStart = HasVehicle ? _vehicle.WorldPosition : Vector3.Zero;
						_phase = 2;
						_phaseTimer = 0f;
						SetAction( ThrottleAction, true );
					}
					break;
				case 2: // mantener throttle 1.6s y medir delta real
					if ( _phaseTimer >= 1.6f )
					{
						SetAction( ThrottleAction, false );
						float delta = HasVehicle ? ( _vehicle.WorldPosition - _sampleStart ).Length : 0f;
						bool pass = delta >= MinThrottleDelta;
						string verdict = pass ? "PASS" : "FAIL";
						Log.Info( $"[VehicleLab] Throttle: movido {delta:F1}u (umbral {MinThrottleDelta:F0}u) â†’ {verdict}" );
						if ( !pass ) MarkFail();
						NextStep();
					}
					break;
			}
		}

		// -------------------------------------------------------------- steering
		private void RunSteer()
		{
			switch ( _phase )
			{
				case 0:
					ReleaseAllInputs();
					_phase = 1;
					_phaseTimer = 0f;
					break;
				case 1: // asentar 0.4s, tomar yaw inicial, acelerar + girar a la derecha
					if ( _phaseTimer >= 0.4f )
					{
						_sampleYaw = HasVehicle ? _vehicle.WorldRotation.Yaw() : 0f;
						_phase = 2;
						_phaseTimer = 0f;
						SetAction( ThrottleAction, true );
						SetAction( SteerRightAction, true );
					}
					break;
				case 2: // mantener 1.8s y medir cambio de heading
					if ( _phaseTimer >= 1.8f )
					{
						SetAction( SteerRightAction, false );
						SetAction( ThrottleAction, false );
						float yawNow = HasVehicle ? _vehicle.WorldRotation.Yaw() : _sampleYaw;
						// Diferencia de yaw envuelta a [-180, 180] (MathF puro, sin dependencias de MathX).
						float raw = yawNow - _sampleYaw;
						float wrapped = ( ( raw + 180f ) % 360f + 360f ) % 360f - 180f;
						float dy = MathF.Abs( wrapped );
						bool pass = dy >= MinSteerYawDelta;
						string verdict = pass ? "PASS" : "FAIL";
						Log.Info( $"[VehicleLab] Steering: yaw {_sampleYaw:F1}Â° â†’ {yawNow:F1}Â° (Î” {dy:F1}Â°, umbral {MinSteerYawDelta:F0}Â°) â†’ {verdict}" );
						if ( !pass ) MarkFail();
						NextStep();
					}
					break;
			}
		}

		// ----------------------------------------------------------------- brake
		private void RunBrake()
		{
			switch ( _phase )
			{
				case 0:
					ReleaseAllInputs();
					_phase = 1;
					_phaseTimer = 0f;
					break;
				case 1: // acelerar 1.8s para coger velocidad
					if ( _phaseTimer >= 0.4f )
					{
						SetAction( ThrottleAction, true );
						_phase = 2;
						_phaseTimer = 0f;
					}
					break;
				case 2:
					if ( _phaseTimer >= 1.8f )
					{
						SetAction( ThrottleAction, false );
						_sampleA = HasVehicle ? _vehicle.WorldPosition : Vector3.Zero;
						_phase = 3;
						_phaseTimer = 0f;
					}
					break;
				case 3: // ventana de velocidad antes de frenar (0.5s)
					if ( _phaseTimer >= 0.5f )
					{
						_sampleB = HasVehicle ? _vehicle.WorldPosition : _sampleA;
						_speedBefore = ( _sampleB - _sampleA ).Length / 0.5f;
						SetAction( BrakeAction, true );
						_phase = 4;
						_phaseTimer = 0f;
					}
					break;
				case 4: // frenar 1.2s
					if ( _phaseTimer >= 1.2f )
					{
						SetAction( BrakeAction, false );
						_sampleA = HasVehicle ? _vehicle.WorldPosition : _sampleB;
						_phase = 5;
						_phaseTimer = 0f;
					}
					break;
				case 5: // ventana de velocidad tras frenar (0.5s)
					if ( _phaseTimer >= 0.5f )
					{
						_sampleB = HasVehicle ? _vehicle.WorldPosition : _sampleA;
						_speedAfter = ( _sampleB - _sampleA ).Length / 0.5f;
						bool pass = _speedAfter < _speedBefore * BrakeSpeedReduction;
						string verdict = pass ? "PASS" : "FAIL";
						Log.Info( $"[VehicleLab] Brake: v {_speedBefore:F1}u/s â†’ {_speedAfter:F1}u/s (factor {BrakeSpeedReduction:F2}) â†’ {verdict}" );
						if ( !pass ) MarkFail();
						NextStep();
					}
					break;
			}
		}

		// --------------------------------------------------------------- reverse
		private void RunReverse()
		{
			switch ( _phase )
			{
				case 0:
					ReleaseAllInputs();
					_phase = 1;
					_phaseTimer = 0f;
					break;
				case 1: // asentar 0.4s, tomar posiciÃ³n+rotaciÃ³n, marcha atrÃ¡s
					if ( _phaseTimer >= 0.4f )
					{
						_sampleStart = HasVehicle ? _vehicle.WorldPosition : Vector3.Zero;
						_sampleYaw = HasVehicle ? _vehicle.WorldRotation.Yaw() : 0f;
						_phase = 2;
						_phaseTimer = 0f;
						SetAction( ReverseAction, true );
					}
					break;
				case 2: // mantener 1.6s y medir movimiento con signo
					if ( _phaseTimer >= 1.6f )
					{
						SetAction( ReverseAction, false );
						Vector3 delta = HasVehicle ? _vehicle.WorldPosition - _sampleStart : Vector3.Zero;
						Vector3 forward = Rotation.FromYaw( _sampleYaw ).Forward;
						float along = Vector3.Dot( delta, forward );
						float moved = delta.Length;
						bool movedBackward = along < 0f;
						bool pass = movedBackward && moved >= MinReverseDelta;
						string signText = movedBackward ? "negativo/OK" : "positivo/NO";
						string verdict = pass ? "PASS" : "FAIL";
						Log.Info( $"[VehicleLab] Reverse: Î” {moved:F1}u, dot(Î”,forward)={along:F1} (signo {signText}, umbral {MinReverseDelta:F0}u) â†’ {verdict}" );
						if ( !pass ) MarkFail();
						NextStep();
					}
					break;
			}
		}

		// -------------------------------------------------------------- exit/detach
		private void RunExit()
		{
			if ( _driver == null )
			{
				Log.Error( "[VehicleLab] Exit FAIL: no hay driver que desmontar" );
				MarkFail();
				NextStep();
				return;
			}

			bool detached = _driver.Exit();
			bool pass = detached && _driver.GameObject.Parent == null;
			Log.Info( pass
				? "[VehicleLab] Exit detach PASS (driver fuera del vehÃ­culo)"
				: "[VehicleLab] Exit detach FAIL (el driver sigue montado)" );
			if ( !pass ) MarkFail();
			NextStep();
		}

		// ---------------------------------------------------------------- finish
		private void FinishSuite()
		{
			ReleaseAllInputs();
			_driver?.GameObject.Destroy();

			int passed = TotalSteps - _fails;
			bool pass = _fails == 0;

			Log.Info( $"[VehicleLab] Suite complete ({passed}/{TotalSteps} PASS)" );

			var result = pass
				? LabSuiteResult.Pass( _runTime, passed, "vehicle foundation fieldguide.vehiclephysics" )
				: LabSuiteResult.Fail( _runTime, _fails, $"fails:{_fails}" );
			_suite?.Complete( result );

			Enabled = false;
		}
	}
}
