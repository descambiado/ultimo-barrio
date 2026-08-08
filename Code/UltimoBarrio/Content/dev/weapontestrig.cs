using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Weapon Test Rig — fixture de escena del dominio Weapon (SOLO dev).
	/// NO usa pawn: el rig tiene su propia cámara y coloca el TargetDummy
	/// directamente en la línea de fuego, así el autotest no depende de movimiento,
	/// input, gravedad ni convenciones de ángulos.
	///
	/// El rig construye una WeaponSuite por entrada (Tests data-driven), las
	/// registra en ContentRuntimeSuite y delega la ejecución al runner unificado
	/// (ContentRuntimeSuiteRunner). Toda la lógica de test vive en WeaponSuite
	/// (ILabSuite); aquí queda solo la infraestructura de escena: cámara, dummy
	/// y registro. Los logs [WeaponLab] y la escalera de evidencia no cambian
	/// respecto a rig-6 (validado 4/4 PASS).
	/// </summary>
	[Title( "Weapon Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "science" )]
	public sealed class WeaponTestRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-7";

		[Property] public bool AutoTest { get; set; } = true;
		[Property] public List<WeaponTestEntry> Tests { get; set; } = new();

		private CameraComponent _camera;
		private GameObject _dummy;
		private LabDamageDummy _dummyDamage;
		private readonly List<WeaponSuite> _suites = new();
		private ContentRuntimeSuiteRunner _runner;
		private bool _summaryLogged;

		protected override void OnStart()
		{
			Log.Info( $"[LabBuild] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[WeaponLab] rig sin CameraComponent" );
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			CreateTargetDummy();

			if ( Tests.Count == 0 )
			{
				Log.Error( "[WeaponLab] suite vacía (Tests sin configurar)" );
				return;
			}

			Log.Info( $"[WeaponLab] Suite: {Tests.Count} tests ({string.Join( ", ", Tests.ConvertAll( t => t.Label ) )})" );

			if ( !AutoTest ) return;

			// Una suite por entrada; el runner las ejecuta en orden de registro.
			for ( int i = 0; i < Tests.Count; i++ )
			{
				var suite = new WeaponSuite( Tests[i], i + 1, Tests.Count, _camera, GameObject, _dummy, _dummyDamage );
				_suites.Add( suite );
				ContentRuntimeSuite.Register( suite );
			}

			_runner = Components.Create<ContentRuntimeSuiteRunner>();
		}

		private void CreateTargetDummy()
		{
			var dummy = new GameObject( true, "TargetDummy" );
			dummy.WorldRotation = Rotation.Identity;

			var renderer = dummy.Components.Create<ModelRenderer>();
			renderer.Model = ResourceLibrary.Get<Model>( "models/citizen_props/crate01.vmdl" );

			var collider = dummy.Components.Create<BoxCollider>();
			collider.Scale = new Vector3( 40f, 40f, 40f );
			collider.Static = true;

			_dummyDamage = dummy.Components.Create<LabDamageDummy>();
			_dummyDamage.MaxHealth = 100f;
			_dummy = dummy;
		}

		protected override void OnUpdate()
		{
			// Resumen legacy [WeaponLab] cuando el runner termina (misma salida que rig-6).
			if ( _summaryLogged || _runner == null || !_runner.IsFinished ) return;

			_summaryLogged = true;
			int pass = _suites.Count( s => s.Result.Status == LabSuiteStatus.Pass );
			Log.Info( $"[WeaponLab] Suite complete ({pass}/{_suites.Count} PASS)" );
		}
	}
}
