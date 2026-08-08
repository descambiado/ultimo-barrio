using Sandbox;
using System;
using UltimoBarrio.Content.Weapons;

namespace UltimoBarrio.Content.Dev
{
	public enum WeaponTestType
	{
		Firearm,
		Melee
	}

	/// <summary>Configuración de un test de arma dentro de la suite del rig (data-driven, serializada en la escena).</summary>
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
	/// WeaponSuite — suite ILabSuite del dominio Weapon (infra QA, SOLO dev).
	///
	/// Porta la lógica validada del WeaponTestRig (rig-6 → rig-7, 4/4 PASS) SIN
	/// cambiar los logs [WeaponLab] ni la escalera de evidencia:
	///   Equip → Fired/Melee strike → Trace hit → Damage (before/after/delta) →
	///   Reload → Drop → PASS/FAIL
	///
	/// Anti-falsificación: el daño recorre SIEMPRE la ruta real
	///   WeaponContentHost.Fire → trace (Scene.Camera) → IDamageTarget.TakeDamage
	/// La suite solo sustituye input humano (Fire/Reload) y target humano (dummy en
	/// la línea de fuego). Nunca llama internals del arma ni del dummy para fabricar
	/// el resultado; el dummy solo se lee y se resetea con su método público.
	///
	/// El rig de escena (WeaponTestRig) construye una instancia por entrada y la
	/// registra en ContentRuntimeSuite; el runner unificado ejecuta Initialize/Step
	/// y emite [UBSuite] Weapon.&lt;Label&gt; PASS|FAIL.
	/// </summary>
	public sealed class WeaponSuite : ILabSuite
	{
		public string Domain => "Weapon";
		public string Name => _entry.Label;
		public bool IsComplete { get; private set; }
		public LabSuiteResult Result { get; private set; }

		private readonly WeaponTestEntry _entry;
		private readonly int _testNumber;
		private readonly int _testTotal;
		private readonly CameraComponent _camera;
		private readonly GameObject _mount;   // WeaponMount: el GameObject del rig
		private readonly GameObject _dummy;
		private readonly LabDamageDummy _dummyDamage;

		private GameObject _weapon;
		private GameObject _viewmodel;
		private WeaponContentHost _host;
		private int _step;
		private TimeSince _timer;
		private bool _fail;
		private string _failReason = "";
		private bool _loggedEquipped;
		private bool _anyHit;
		private float _firstDelta;
		private float _firstHitFrom;
		private float _elapsed;
		private bool _initialized;

		public WeaponSuite( WeaponTestEntry entry, int testNumber, int testTotal, CameraComponent camera, GameObject mount, GameObject dummy, LabDamageDummy dummyDamage )
		{
			_entry = entry ?? throw new ArgumentNullException( nameof( entry ) );
			_testNumber = testNumber;
			_testTotal = testTotal;
			_camera = camera;
			_mount = mount;
			_dummy = dummy;
			_dummyDamage = dummyDamage;
		}

		public void Initialize()
		{
			if ( _initialized ) return;
			_initialized = true;

			_step = 0;
			_timer = 0f;
			_anyHit = false;
			_firstDelta = 0f;
			_firstHitFrom = 0f;
			_loggedEquipped = false;
			_fail = false;
			_failReason = "";
			_elapsed = 0f;

			Log.Info( $"[WeaponLab] === Test {_testNumber}/{_testTotal}: {_entry.Label} ({_entry.TestType}) ===" );

			// Dummy en la línea de fuego, collider centrado a la altura del ray.
			_dummy.WorldPosition = _camera.WorldPosition + _camera.WorldRotation.Forward * _entry.TargetDistance;
			_dummyDamage.ResetHealth();
			Log.Info( $"[WeaponLab] TargetDummy en {_dummy.WorldPosition}" );

			Equip( _entry );
		}

		public void Step( float dt )
		{
			_elapsed += dt;

			// El OnStart del host del arma corre tras el frame de creación; logueamos
			// el ammo real un frame después para que 'Equipped' sea determinista.
			if ( !_loggedEquipped && _host != null )
			{
				string def = _host.Definition?.Id ?? "NULL";
				Log.Info( $"[WeaponLab] {_entry.Label} Equipped def={def}" );
				if ( !string.IsNullOrEmpty( _entry.WeaponId ) && def != _entry.WeaponId )
				{
					Log.Error( $"[WeaponLab] {_entry.Label} definición inesperada: {def} (esperada {_entry.WeaponId})" );
					_fail = true;
					_failReason = "definición inesperada";
				}
				if ( _entry.UsesAmmo )
				{
					Log.Info( $"[WeaponLab] {_entry.Label} Equipped ammo={_host.CurrentAmmo}" );
				}

				// Evidencia del modelo REAL cargado (cloud ident resuelto vs fallback).
				var wmRenderer = _weapon.Components.GetInChildrenOrSelf<ModelRenderer>();
				if ( wmRenderer?.Model != null )
				{
					Log.Info( $"[WeaponLab] {_entry.Label} world model = {wmRenderer.Model.ResourceName}" );
				}

				_loggedEquipped = true;
			}

			RunTest( _entry );
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
						FinishTest();
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
						FinishTest();
					}
					break;
			}
		}

		private void Equip( WeaponTestEntry entry )
		{
			var wPrefab = ResourceLibrary.Get<PrefabFile>( entry.WorldPrefab );
			if ( wPrefab == null )
			{
				Log.Error( $"[WeaponLab] {entry.Label} world prefab NO encontrado: {entry.WorldPrefab}" );
				_fail = true;
				_failReason = "world prefab NO encontrado";
				return;
			}

			var weapon = SceneUtility.GetPrefabScene( wPrefab ).Clone();
			weapon.SetParent( _mount ); // WeaponMount = el propio rig
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
				_failReason = "view prefab NO encontrado";
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
			if ( vmRenderer?.Model != null )
			{
				Log.Info( $"[WeaponLab] {entry.Label} view model = {vmRenderer.Model.ResourceName}" );
			}

			if ( _host != null )
			{
				Log.Info( $"[WeaponLab] {entry.Label} Equipped" );
			}
			else
			{
				Log.Error( $"[WeaponLab] {entry.Label} WeaponContentHost no encontrado en el worldmodel" );
				_fail = true;
				_failReason = "WeaponContentHost no encontrado";
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
					_failReason = "rig incompleto (host o dummy null)";
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
				_failReason = "rig incompleto (host o dummy null)";
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

		private void FinishTest()
		{
			// Daño mínimo esperado: >= ExpectedDamage (tolerancia). Para escopeta con
			// pellets y spread aleatorio, el delta total varía; el primer hit debe
			// alcanzar al menos el daño de un pellet.
			bool damageOk = _anyHit && _firstDelta >= _entry.ExpectedDamage - 0.5f;
			bool pass = damageOk && !_fail;
			if ( !pass && string.IsNullOrEmpty( _failReason ) )
			{
				_failReason = damageOk ? "fail flag" : "daño insuficiente";
			}

			Log.Info( pass ? $"[WeaponLab] {_entry.Label} PASS" : $"[WeaponLab] {_entry.Label} FAIL" );

			IsComplete = true;
			Result = pass
				? LabSuiteResult.Pass( _elapsed, _firstDelta, "complete" )
				: LabSuiteResult.Fail( _elapsed, _firstDelta, $"fail:{_failReason}" );
		}

		/// <summary>
		/// Cloud.Model() exige string literal en el call site (CloudAssetProvider de
		/// compile-time). Mapeo prefab→ident para los labs con cloud asset real;
		/// devuelve null para el resto (el renderer mantiene su modelo del prefab).
		/// Idents verificados en el backend (find_packages, 2026-08-07).
		/// </summary>
		private Model ResolveViewCloudModel( string viewPrefab )
		{
			switch ( viewPrefab )
			{
				case "prefabs/content/weapons/v_usp_content.prefab":
					return Cloud.Model( "facepunch.v_usp" );
				case "prefabs/content/weapons/v_knife_content.prefab":
					return Cloud.Model( "facepunch.v_m9bayonet" );
				case "prefabs/content/weapons/v_shotgun_content.prefab":
					return Cloud.Model( "facepunch.v_spaghellim4" );
				default:
					return null;
			}
		}
	}
}
