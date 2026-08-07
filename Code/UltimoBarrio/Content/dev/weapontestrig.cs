using Sandbox;
using System;
using UltimoBarrio.Content.Weapons;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Weapon Test Rig — fixture automatizado para validar el bucle de armas portable.
	/// SOLO dev. NO usa pawn: el rig tiene su propia cámara y coloca el TargetDummy
	/// directamente en la línea de fuego, así el autotest no depende de movimiento,
	/// input, gravedad ni convenciones de ángulos.
	///
	/// El daño recorre SIEMPRE el camino real:
	///   WeaponContentHost.Fire() → trace (Scene.Camera) → IDamageTarget → damage
	/// El rig solo lee la salud del dummy para reportar (no falsifica PASS).
	///
	/// Logs deterministas: [LabBuild] VERSION, [WeaponLab] ...
	/// </summary>
	[Title( "Weapon Test Rig" )]
	[Category( "Último Barrio — Content (Dev)" )]
	[Icon( "science" )]
	public sealed class WeaponTestRig : Component
	{
		// Sube este número en cada cambio de código relevante para verificar
		// que la sesión de juego carga el assembly nuevo (detección de hotload atrasado).
		public const string Version = "rig-2";

		[Property] public float TargetDistance { get; set; } = 200f;
		[Property] public bool AutoTest { get; set; } = true;

		[Property] public string WorldPrefab { get; set; } = "prefabs/content/weapons/w_usp_content.prefab";
		[Property] public string ViewPrefab { get; set; } = "prefabs/content/weapons/v_usp_content.prefab";

		private CameraComponent _camera;
		private GameObject _weapon;
		private GameObject _viewmodel;
		private GameObject _dummy;
		private LabDamageDummy _dummyDamage;
		private WeaponContentHost _host;
		private TimeSince _timer;
		private int _step;
		private bool _fail;
		private bool _loggedEquipped;

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
			EquipWeapon();
		}

		private void CreateTargetDummy()
		{
			var dummy = new GameObject( true, "TargetDummy" );
			// Línea de fuego: mismo origen y dirección que el trace del host (Scene.Camera).
			dummy.WorldPosition = _camera.WorldPosition + _camera.WorldRotation.Forward * TargetDistance;
			dummy.WorldRotation = Rotation.Identity;

			var renderer = dummy.Components.Create<ModelRenderer>();
			renderer.Model = ResourceLibrary.Get<Model>( "models/citizen_props/crate01.vmdl" );

			var collider = dummy.Components.Create<BoxCollider>();
			collider.Scale = new Vector3( 40f, 40f, 40f );
			collider.Static = true;

			_dummyDamage = dummy.Components.Create<LabDamageDummy>();
			_dummyDamage.MaxHealth = 100f;

			_dummy = dummy;
			Log.Info( $"[WeaponLab] TargetDummy en {dummy.WorldPosition}" );
		}

		private void EquipWeapon()
		{
			var wPrefab = ResourceLibrary.Get<PrefabFile>( WorldPrefab );
			if ( wPrefab == null )
			{
				Log.Error( "[WeaponLab] world prefab NO encontrado" );
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
			Log.Info( "[WeaponLab] USP asset world OK" );

			var vPrefab = ResourceLibrary.Get<PrefabFile>( ViewPrefab );
			if ( vPrefab == null )
			{
				Log.Error( "[WeaponLab] view prefab NO encontrado" );
				_fail = true;
				return;
			}

			var viewmodel = SceneUtility.GetPrefabScene( vPrefab ).Clone();
			viewmodel.SetParent( _camera.GameObject );
			viewmodel.LocalPosition = Vector3.Zero;
			viewmodel.LocalRotation = Rotation.Identity;
			viewmodel.NetworkSpawn( Connection.Local );
			_viewmodel = viewmodel;

			// Viewmodel via Cloud ident: el prefab del pack mantiene su ruta como fallback,
			// pero el fixture resuelve el modelo real del paquete (persistente sin install manual).
			var vmRenderer = viewmodel.Components.Get<ModelRenderer>();
			if ( vmRenderer != null )
			{
				var vmModel = ResolveViewCloudModel();
				if ( vmModel != null ) vmRenderer.Model = vmModel;
			}
			Log.Info( "[WeaponLab] USP asset view OK" );

			if ( _host != null )
			{
				Log.Info( "[WeaponLab] Equipped" );
			}
			else
			{
				Log.Error( "[WeaponLab] WeaponContentHost no encontrado en el worldmodel" );
				_fail = true;
			}
		}

		protected override void OnUpdate()
		{
			if ( _camera == null || _fail ) return;

			// El OnStart del host del arma corre tras el frame de creación; logueamos
			// el ammo real un frame después para que 'Equipped' sea determinista.
			if ( !_loggedEquipped && _host != null )
			{
				Log.Info( $"[WeaponLab] Equipped ammo={_host.CurrentAmmo}" );
				_loggedEquipped = true;
			}

			if ( !AutoTest ) return;
			RunAutoTest();
		}

		private void RunAutoTest()
		{
			switch ( _step )
			{
				case 0:
					_timer = 0f;
					_step = 1;
					Log.Info( "[WeaponLab] Autotest iniciado" );
					break;
				case 1:
					if ( _timer >= 1f )
					{
						FireTimes( 3 );
						_timer = 0f;
						_step = 2;
					}
					break;
				case 2:
					if ( _timer >= 2f )
					{
						Reload();
						_timer = 0f;
						_step = 3;
					}
					break;
				case 3:
					if ( _timer >= 2.5f )
					{
						FireTimes( 1 );
						_timer = 0f;
						_step = 4;
					}
					break;
				case 4:
					if ( _timer >= 2f )
					{
						Drop();
						_timer = 0f;
						_step = 5;
					}
					break;
				case 5:
					if ( _timer >= 1f )
					{
						Finish();
						_step = 6;
					}
					break;
			}
		}

		private void FireTimes( int count )
		{
			for ( int i = 0; i < count; i++ )
			{
				if ( _host == null || _dummyDamage == null )
				{
					Log.Error( "[WeaponLab] rig incompleto (host o dummy null)" );
					_fail = true;
					return;
				}

				float before = _dummyDamage.Health;
				int ammoBefore = _host.CurrentAmmo;

				_host.Fire();

				float after = _dummyDamage.Health;
				Log.Info( $"[WeaponLab] Fired ammo={ammoBefore}->{_host.CurrentAmmo}" );

				if ( after < before )
				{
					Log.Info( "[WeaponLab] Trace hit=TargetDummy" );
					Log.Info( $"[WeaponLab] Damage before={before:F0} after={after:F0}" );
				}
				else
				{
					Log.Info( "[WeaponLab] Trace hit=MISS" );
				}
			}
		}

		private void Reload()
		{
			_host?.Reload();
			Log.Info( "[WeaponLab] Reloaded" );
		}

		private void Drop()
		{
			if ( _weapon != null )
			{
				_weapon.SetParent( null );
				_weapon.WorldPosition = _camera.WorldPosition + _camera.WorldRotation.Forward * 80f;
				_weapon = null;
			}
			_viewmodel?.Destroy();
			_viewmodel = null;
			Log.Info( "[WeaponLab] Dropped" );
		}

		/// <summary>
		/// Cloud.Model() exige string literal en el call site (CloudAssetProvider de
		/// compile-time). Mapeo prefab→ident para los labs conocidos; devuelve null
		/// para prefabs sin ident cloud (el renderer mantiene su ruta del prefab).
		/// </summary>
		private Model ResolveViewCloudModel()
		{
			switch ( ViewPrefab )
			{
				case "prefabs/content/weapons/v_usp_content.prefab":
					return Cloud.Model( "facepunch.v_usp" );
				default:
					return null;
			}
		}

		private void Finish()
		{
			bool hit = _dummyDamage != null && _dummyDamage.Health < 100f;
			bool ammoOk = _host == null || _host.CurrentAmmo >= 0;
			bool pass = hit && ammoOk && !_fail;

			Log.Info( pass ? "[WeaponLab] PASS" : "[WeaponLab] FAIL" );
		}
	}
}
