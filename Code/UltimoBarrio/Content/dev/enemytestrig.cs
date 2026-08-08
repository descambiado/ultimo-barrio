using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Enemy Test Rig — fixture de escena del dominio Enemy (SOLO dev).
	/// NO usa pawn: el rig tiene su propia cámara, crea el TargetDummy (sobre el
	/// DummyMarker) y spawnea cada enemigo en el SpawnMarker; el dummy sustituye
	/// al target humano y el rig al input humano.
	///
	/// El rig construye una EnemySuite por entrada (Tests data-driven), las
	/// registra en ContentRuntimeSuite y delega la ejecución al runner unificado
	/// (ContentRuntimeSuiteRunner). Toda la lógica de test vive en EnemySuite
	/// (ILabSuite); aquí queda solo la infraestructura de escena: cámara, dummy
	/// y registro. Los logs [EnemyLab] y la escalera de evidencia son los del rig
	/// portado (spawn → NavMeshAgent real → detección → t0/t1/t2 → ataque
	/// IDamageTarget → daño recibido → muerte → loot físico → PASS).
	/// </summary>
	[Title( "Enemy Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "science" )]
	public sealed class EnemyTestRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-1";

		[Property] public bool AutoTest { get; set; } = true;
		[Property] public List<EnemyTestEntry> Tests { get; set; } = new();
		[Property] public GameObject SpawnMarker { get; set; } // posición de spawn del enemigo (sobre NavMesh)
		[Property] public GameObject DummyMarker { get; set; } // posición del TargetDummy (sobre NavMesh)

		private CameraComponent _camera;
		private GameObject _dummy;
		private LabDamageDummy _dummyDamage;
		private readonly List<EnemySuite> _suites = new();
		private ContentRuntimeSuiteRunner _runner;
		private bool _summaryLogged;

		protected override void OnStart()
		{
			Log.Info( $"[LabBuild] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[EnemyLab] rig sin CameraComponent" );
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			if ( SpawnMarker == null || DummyMarker == null )
			{
				Log.Error( "[EnemyLab] rig sin SpawnMarker/DummyMarker (coloca ambos sobre NavMesh en el editor)" );
				return;
			}

			// Cámara apuntando al área de pruebas (sin jugador humano). El framing es
			// aproximado; la evidencia real son los logs [EnemyLab]/[UBSuite].
			_camera.WorldRotation = Rotation.From( -17f, -90f, 0f );

			CreateTargetDummy();

			if ( Tests.Count == 0 )
			{
				Log.Error( "[EnemyLab] suite vacía (Tests sin configurar)" );
				return;
			}

			Log.Info( $"[EnemyLab] Suite: {Tests.Count} tests ({string.Join( ", ", Tests.ConvertAll( t => t.Label ) )})" );

			if ( !AutoTest ) return;

			// Una suite por entrada; el runner las ejecuta en orden de registro.
			for ( int i = 0; i < Tests.Count; i++ )
			{
				var suite = new EnemySuite( Tests[i], i + 1, Tests.Count, _dummy, _dummyDamage, SpawnMarker, DummyMarker );
				_suites.Add( suite );
				ContentRuntimeSuite.Register( suite );
			}

			_runner = Components.Create<ContentRuntimeSuiteRunner>();
		}

		private void CreateTargetDummy()
		{
			var dummy = new GameObject( true, "TargetDummy" );
			dummy.WorldPosition = DummyMarker.WorldPosition;
			dummy.WorldRotation = Rotation.Identity;
			dummy.Tags.Add( "enemy_target" );

			var renderer = dummy.Components.Create<ModelRenderer>();
			renderer.Model = ResourceLibrary.Get<Model>( "models/citizen_props/crate01.vmdl" );

			var collider = dummy.Components.Create<BoxCollider>();
			collider.Scale = new Vector3( 60f, 60f, 90f );
			collider.Static = true;

			_dummyDamage = dummy.Components.Create<LabDamageDummy>();
			_dummyDamage.LogPrefix = "[EnemyLab]";
			_dummyDamage.MaxHealth = 200f;
			_dummy = dummy;
		}

		protected override void OnUpdate()
		{
			// Resumen legacy [EnemyLab] cuando el runner termina (misma salida que el rig portado).
			if ( _summaryLogged || _runner == null || !_runner.IsFinished ) return;

			_summaryLogged = true;
			int pass = _suites.Count( s => s.Result.Status == LabSuiteStatus.Pass );
			Log.Info( $"[EnemyLab] Suite complete ({pass}/{_suites.Count} PASS)" );
		}
	}
}
