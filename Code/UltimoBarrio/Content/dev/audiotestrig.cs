using Sandbox;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Audio Test Rig — fixture de escena del dominio Audio (SOLO dev, Worker F).
	///
	/// Registra la AudioSuite del contrato del banco de sonido
	/// (ContentRuntimeSuite.Register) y delega la ejecución al runner unificado.
	/// La suite valida por la ruta real: ResourceLibrary.Get&lt;SoundEvent&gt;
	/// → Sound.Play → agregado sin missing assets. El rig NO investiga sonidos:
	/// solo ejecuta el contrato sobre el banco integrado.
	/// </summary>
	[Title( "Audio Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "volume_up" )]
	public sealed class AudioTestRig : Component
	{
		// Sube este número en cada cambio de código relevante (detección de hotload atrasado).
		public const string Version = "rig-1";

		[Property] public bool AutoTest { get; set; } = true;

		/// <summary>Posición de reproducción 3D de los eventos posicionales.</summary>
		[Property] public Vector3 PlayPosition { get; set; } = new Vector3( 0f, 0f, 60f );

		private CameraComponent _camera;
		private ContentRuntimeSuiteRunner _runner;

		protected override void OnStart()
		{
			Log.Info( $"[LabAudio] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[AudioLab] rig sin CameraComponent" );
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			if ( !AutoTest ) return;

			ContentRuntimeSuite.Register( new AudioSuite( "Core", AudioSuite.ContentBankDefault(), PlayPosition ) );
			_runner = Components.Create<ContentRuntimeSuiteRunner>();
		}

		protected override void OnUpdate()
		{
			// El runner emite el resumen ([UBSuite] Suite run complete); nada que hacer aquí.
		}
	}
}
