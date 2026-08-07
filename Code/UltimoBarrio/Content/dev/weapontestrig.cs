using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Content.Weapons;

namespace UltimoBarrio.Content.Dev
{
	public enum WeaponTestType
	{
		Firearm,
		Melee
	}

	/// <summary>Configuración de un test de arma dentro de la suite del rig.</summary>
	public sealed class WeaponTestEntry
	{
		public string Label { get; set; } = "";
		public string WeaponId { get; set; } = "";        // opcional: valida que el host cargó esta definición
		public WeaponTestType TestType { get; set; } = WeaponTestType.Firearm;
		public string WorldPrefab { get; set; } = "";
		public string ViewPrefab { get; set; } = "";
		public float TargetDistance { get; set; } = 200f; // el dummy se coloca en la línea de fuego a esta distancia
		public float ExpectedDamage { get; set; } = 15f;
		public int ClipSize { get; set; } = 12;
		public bool UsesAmmo { get; set; } = true;
	}

	/// <summary>
	/// Weapon Test Rig — fixture automatizado para validar el bucle de armas portable.
	/// SOLO dev. NO usa pawn: el rig tiene su propia cámara y coloca el TargetDummy
	/// directamente en la línea de fuego, así el autotest no depende de movimiento,
	/// input, gravedad ni convenciones de ángulos.
	///
	/// El daño recorre SIEMPRE el camino real:
	///   WeaponContentHost.Fire → trace (Scene.Camera) → IDamageTarget → damage
	/// El rig solo lee la salud del dummy para reportar (no falsifica PASS).
	///
	/// La suite es data-driven (List of WeaponTestEntry): FIREARM (equip → fire x3 →
	/// reload → fire → drop) y MELEE (equip → strike → cooldown → strike → drop).
	/// Los únicos switches por arma viven en el resolver de Cloud literals (el compiler
	/// exige string literal en Cloud.Model), nunca en la lógica del test.
	/// </summary>
	[Title( "Weapon Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "science" )]
	public sealed class WeaponTestRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-4";

		[Property] public bool AutoTest { get; set; } = true;
		[Property] public List<WeaponTestEntry> Tests { get; set; } = new();

		private CameraComponent _camera;
		private GameObject _weapon;
		private GameObject _viewmodel;
		private GameObject _dummy;
		private LabDamageDummy _dummyDamage;
		private WeaponContentHost _host;
		private int _testIndex;
		private int _step;
		private TimeSince _timer;
		private bool _fail;
		private bool _loggedEquipped;
		private bool _anyHit;
		private float _firstDelta;
		private float _firstHitFrom;
		private int _fails;

		private WeaponTestEntry Entry => _testIndex < Tests.Count ? Tests[_testIndex] : null;

		protected override void OnStart()
		{
			Log.Info( $"[LabBuild] VERSION={Version}" );

			_camera = Components.Get<CameraComponent>( true );
			if ( _camera == null )
			{
				Log.Error( "[WeaponLab] rig sin CameraComponent" );
				_fail = true;
				return;
			}
			_camera.IsMainCamera = true;
			_camera.Priority = 10;

			CreateTargetDummy();

			if ( Tests.Count == 0 )
			{
				Log.Error( "[WeaponLab] suite vacía (Tests sin configurar)" );
				_fail = true;
				return;
			}

			Log.Info( $"[WeaponLab] Suite: {Tests.Count} tests ({string.Join( ", ", Tests.ConvertAll( t => t.Label ) )})" );
			StartTest( 0 );
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

		private void StartTest( int index )
		{
			_testIndex = index;
			_step = 0;
			_timer = 0f;
			_anyHit = false;
			_firstDelta = 0f;
			_firstHitFrom = 0f;
			_loggedEquipped = false;
			_fail = false;

			var entry = Entry;
			Log.Info( $"[WeaponLab] === Test {index + 1}/{Tests.Count}: {entry.Label} ({entry.TestType}) ===" );

			// Dummy en la línea de fuego, collider centrado a la altura del ray.
			_dummy.WorldPosition = _camera.WorldPosition + _camera.WorldRotation.Forward * entry.TargetDistance;
			_dummyDamage.ResetHealth();
			Log.Info( $"[WeaponLab] TargetDummy en {_dummy.WorldPosition}" );

			Equip( entry );
		}

		private void Equip( WeaponTestEntry entry )
		{
			var wPrefab = ResourceLibrary.Get<PrefabFile>( entry.WorldPrefab );
			if ( wPrefab == null )
			{
				Log.Error( $"[WeaponLab] {entry.Label} world prefab NO encontrado: {entry.WorldPrefab}" );
				_fail = true;
				return;
			}

			var weapon = SceneUtility.GetPrefabScene( wPrefab ).Clone();
			weapon.SetParent( GameObject ); // WeaponMount = el propio rig
			weapon.LocalPosition = Vector3.Zero;
			weapon.LocalRotation = Rotation.Identity;
			weapon.NetworkSpawn( Connection.Local );
			_weapon = weapon;
			_host = weapon.Components.Get<WeaponContentHost>();
			Log.Info( $"[WeaponLab] {entry.Label} asset world OK" );

			var vPrefab = ResourceLibrary.Get<PrefabFile>( entry.ViewPrefab );
			if ( vPrefab == null )
			{
				Log.Error( $"[WeaponLab] {entry.Label} view prefab NO encontrado: {entry.ViewPrefab}" );
				_fail = true;
				return;
			}

			var viewmodel = SceneUtility.GetPrefabScene( vPrefab ).Clone();
			viewmodel.SetParent( _camera.GameObject );
			viewmodel.LocalPosition = Vector3.Zero;
			viewmodel.LocalRotation = Rotation.Identity;
			viewmodel.NetworkSpawn( Connection.Local );
			_viewmodel = viewmodel;

			// Viewmodel via Cloud ident cuando existe (ver ResolveViewCloudModel):
			// el prefab del pack mantiene su modelo como fallback.
			var vmRenderer = viewmodel.Components.Get<ModelRenderer>();
			if ( vmRenderer != null )
			{
				var vmModel = ResolveViewCloudModel( entry.ViewPrefab );
				if ( vmModel != null ) vmRenderer.Model = vmModel;
			}
			Log.Info( $"[WeaponLab] {entry.Label} asset view OK" );

			if ( _host != null )
			{
				Log.Info( $"[WeaponLab] {entry.Label} Equipped" );
			}
			else
			{
				Log.Error( $"[WeaponLab] {entry.Label} WeaponContentHost no encontrado en el worldmodel" );
				_fail = true;
			}
		}

		protected override void OnUpdate()
		{
			if ( _camera == null || !AutoTest ) return;

			// El OnStart del host del arma corre tras el frame de creación; logueamos
			// el ammo real un frame después para que 'Equipped' sea determinista.
			var entry = Entry;
			if ( entry == null ) return;

			if ( !_loggedEquipped && _host != null )
			{
				// El OnStart del host corre tras el frame de creación: aquí la definición
				// y el ammo ya son los reales (logs deterministas + validación de WeaponId).
				string def = _host.Definition?.Id ?? "NULL";
				Log.Info( $"[WeaponLab] {entry.Label} Equipped def={def}" );
				if ( !string.IsNullOrEmpty( entry.WeaponId ) && def != entry.WeaponId )
				{
					Log.Error( $"[WeaponLab] {entry.Label} definición inesperada: {def} (esperada {entry.WeaponId})" );
					_fail = true;
				}
				if ( entry.UsesAmmo )
				{
					Log.Info( $"[WeaponLab] {entry.Label} Equipped ammo={_host.CurrentAmmo}" );
				}
				_loggedEquipped = true;
			}

			RunTest( entry );
		}

		private void RunTest( WeaponTestEntry entry )
		{
			if ( entry.TestType == WeaponTestType.Firearm )
			{
				RunFirearmTest( entry );
			}
			else
			{
				RunMeleeTest( entry );
			}
		}

		private void RunFirearmTest( WeaponTestEntry entry )
		{
			switch ( _step )
			{
				case 0:
					Log.Info( $"[WeaponLab] {entry.Label} Autotest iniciado" );
					_timer = 0f;
					_step = 1;
					break;
				case 1:
					if ( _timer >= 1f )
					{
						FireTimes( entry, 3 );
						_timer = 0f;
						_step = 2;
					}
					break;
				case 2:
					if ( _timer >= 3f )
					{
						Reload( entry );
						_timer = 0f;
						_step = 3;
					}
					break;
				case 3:
					if ( _timer >= 5.5f )
					{
						FireTimes( entry, 1 );
						_timer = 0f;
						_step = 4;
					}
					break;
				case 4:
					if ( _timer >= 7.5f )
					{
						Drop( entry );
						_timer = 0f;
						_step = 5;
					}
					break;
				case 5:
					if ( _timer >= 8.5f )
					{
						FinishTest( entry );
					}
					break;
			}
		}

		private void RunMeleeTest( WeaponTestEntry entry )
		{
			switch ( _step )
			{
				case 0:
					Log.Info( $"[WeaponLab] {entry.Label} Autotest iniciado" );
					_timer = 0f;
					_step = 1;
					break;
				case 1:
					if ( _timer >= 1f )
					{
						Strike( entry );
						_timer = 0f;
						_step = 2;
					}
					break;
				case 2:
					if ( _timer >= 3f ) // cooldown real (FireRate 0.7s) con margen
					{
						Strike( entry );
						_timer = 0f;
						_step = 3;
					}
					break;
				case 3:
					if ( _timer >= 5f )
					{
						Drop( entry );
						_timer = 0f;
						_step = 4;
					}
					break;
				case 4:
					if ( _timer >= 6f )
					{
						FinishTest( entry );
					}
					break;
			}
		}

		private void FireTimes( WeaponTestEntry entry, int count )
		{
			for ( int i = 0; i < count; i++ )
			{
				if ( _host == null || _dummyDamage == null )
				{
					Log.Error( $"[WeaponLab] {entry.Label} rig incompleto (host o dummy null)" );
					_fail = true;
					return;
				}

				float before = _dummyDamage.Health;
				int ammoBefore = _host.CurrentAmmo;

				_host.Fire();

				float after = _dummyDamage.Health;
				Log.Info( $"[WeaponLab] {entry.Label} Fired ammo={ammoBefore}->{_host.CurrentAmmo}" );

				RecordHit( entry, before, after );
			}
		}

		private void Strike( WeaponTestEntry entry )
		{
			if ( _host == null || _dummyDamage == null )
			{
				Log.Error( $"[WeaponLab] {entry.Label} rig incompleto (host o dummy null)" );
				_fail = true;
				return;
			}

			float before = _dummyDamage.Health;
			_host.Fire();
			float after = _dummyDamage.Health;
			Log.Info( $"[WeaponLab] {entry.Label} Melee strike" );

			RecordHit( entry, before, after );
		}

		private void RecordHit( WeaponTestEntry entry, float before, float after )
		{
			if ( after < before )
			{
				float delta = before - after;
				Log.Info( $"[WeaponLab] {entry.Label} Trace hit=TargetDummy" );
				Log.Info( $"[WeaponLab] {entry.Label} Damage before={before:F0} after={after:F0} (delta {delta:F1})" );
				_anyHit = true;
				if ( _firstDelta <= 0f )
				{
					_firstDelta = delta;
					_firstHitFrom = before;
				}
			}
			else
			{
				Log.Info( $"[WeaponLab] {entry.Label} Trace hit=MISS" );
			}
		}

		private void Reload( WeaponTestEntry entry )
		{
			_host?.Reload();
			Log.Info( $"[WeaponLab] {entry.Label} Reloaded" );
		}

		private void Drop( WeaponTestEntry entry )
		{
			if ( _weapon != null )
			{
				_weapon.SetParent( null );
				_weapon.WorldPosition = _camera.WorldPosition + _camera.WorldRotation.Forward * 80f;
				_weapon = null;
			}
			_viewmodel?.Destroy();
			_viewmodel = null;
			_host = null;
			Log.Info( $"[WeaponLab] {entry.Label} Dropped" );
		}

		private void FinishTest( WeaponTestEntry entry )
		{
			bool damageOk = _anyHit && MathF.Abs( _firstDelta - entry.ExpectedDamage ) < 0.5f;
			bool pass = damageOk && !_fail;
			if ( !pass ) _fails++;

			Log.Info( pass ? $"[WeaponLab] {entry.Label} PASS" : $"[WeaponLab] {entry.Label} FAIL" );

			_testIndex++;
			if ( _testIndex < Tests.Count )
			{
				StartTest( _testIndex );
			}
			else
			{
				Log.Info( $"[WeaponLab] Suite complete ({Tests.Count - _fails}/{Tests.Count} PASS)" );
			}
		}

		/// <summary>
		/// Cloud.Model() exige string literal en el call site (CloudAssetProvider de
		/// compile-time). Mapeo prefab→ident para los labs con cloud asset real;
		/// devuelve null para el resto (el renderer mantiene su modelo del prefab).
		/// </summary>
		private Model ResolveViewCloudModel( string viewPrefab )
		{
			switch ( viewPrefab )
			{
				case "prefabs/content/weapons/v_usp_content.prefab":
					return Cloud.Model( "facepunch.v_usp" );
				default:
					return null;
			}
		}
	}
}
