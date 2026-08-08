using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Enemy Nav Rig - fixture de escena del enemy_lab (SOLO dev, Worker B).
	///
	/// Infraestructura del escenario de navegación: cámara propia (sin pawn),
	/// referencias a los fixtures (SpawnMarker / TestTarget / LootObservationPoint)
	/// y construcción de una EnemyNavSuite por entrada (Tests data-driven).
	/// Las suites se registran en ContentRuntimeSuite y las ejecuta el runner
	/// unificado (ContentRuntimeSuiteRunner); el resumen legacy [EnemyLab] se
	/// emite cuando el runner termina.
	///
	/// Dirección de dependencias (guard de portabilidad): rig → host, nunca al revés.
	/// El rig solo sustituye input humano y target humano; la navegación la hace el
	/// NavMeshAgent real y el daño recorre IDamageTarget (anti-falsificación).
	/// </summary>
	[Title( "Lab Enemy Nav Rig" )]
	[Category( "Ultimo Barrio - Content (Dev)" )]
	[Icon( "navigation" )]
	public sealed class LabEnemyNavRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-1";

		[Property] public bool AutoTest { get; set; } = true;
		[Property] public GameObject SpawnMarker { get; set; }
		[Property] public GameObject TestTarget { get; set; }
		[Property] public GameObject LootObservationPoint { get; set; }
		[Property] public List<EnemyNavTestEntry> Tests { get; set; } = new();

		private CameraComponent _camera;
		private readonly List<EnemyNavSuite> _suites = new();
		private ContentRuntimeSuiteRunner _runner;
		private bool _summaryLogged;

		protected override void OnStart()
		{
			Log.Info( $"[LabEnemy] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[EnemyLab] rig sin CameraComponent" );
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			if ( SpawnMarker == null || !SpawnMarker.IsValid() || TestTarget == null || !TestTarget.IsValid() )
			{
				Log.Error( "[EnemyLab] fixtures incompletas (SpawnMarker / TestTarget)" );
				return;
			}

			// El punto de observación debe tener LabLootObserver (se autocrea si falta).
			if ( LootObservationPoint != null && LootObservationPoint.IsValid() )
			{
				if ( LootObservationPoint.Components.Get<LabLootObserver>( true ) == null )
				{
					var observer = LootObservationPoint.Components.Create<LabLootObserver>();
					observer.Anchor = LootObservationPoint;
					Log.Info( "[EnemyLab] LabLootObserver autocreado en LootObservationPoint" );
				}
			}
			else
			{
				Log.Warning( "[EnemyLab] sin LootObservationPoint — la fase loot quedará SKIP" );
			}

			if ( Tests.Count == 0 )
			{
				Log.Error( "[EnemyLab] suite vacía (Tests sin configurar)" );
				return;
			}

			Log.Info( $"[EnemyLab] Suite: {Tests.Count} tests ({string.Join( ", ", Tests.ConvertAll( t => t.Label ) )})" );

			if ( !AutoTest ) return;

			foreach ( var entry in Tests )
			{
				var observer = LootObservationPoint != null && LootObservationPoint.IsValid()
					? LootObservationPoint.Components.Get<LabLootObserver>( true )
					: null;

				var suite = new EnemyNavSuite( entry, SpawnMarker, TestTarget, observer );
				_suites.Add( suite );
				ContentRuntimeSuite.Register( suite );
			}

			_runner = Components.Create<ContentRuntimeSuiteRunner>();
		}

		protected override void OnUpdate()
		{
			// Resumen legacy [EnemyLab] cuando el runner termina (misma salida que weapon_lab).
			if ( _summaryLogged || _runner == null || !_runner.IsFinished ) return;

			_summaryLogged = true;
			int pass = _suites.Count( s => s.Result.Status == LabSuiteStatus.Pass );
			int fail = _suites.Count( s => s.Result.Status == LabSuiteStatus.Fail );
			Log.Info( $"[EnemyLab] Suite complete ({pass}/{_suites.Count} PASS, {fail} FAIL)" );
		}
	}
}
