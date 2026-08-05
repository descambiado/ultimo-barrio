using Sandbox;
using System;
using UltimoBarrio.Audio;

namespace UltimoBarrio.Combat
{
    /// <summary>
    /// USP completa: cargador persistente (vía inventario), hitscan, daño,
    /// cadencia, dry fire, recarga desde reserva, recoil + camera kick,
    /// muzzle flash, humo, casquillo, impactos, sonido interior/exterior,
    /// drop y repickup (los dos últimos vía ItemPickupFactory/HeldItemController).
    ///
    /// Los efectos visuales son procedurales (primitivas dev) porque el
    /// proyecto aún no tiene assets finales; el prefab ub_usp.prefab debe
    /// llevar este componente + ModelRenderer + BoxCollider (ver manifiesto).
    /// </summary>
    [Title( "USP Pistol" )]
    [Category( "Último Barrio — Combat" )]
    [Icon( "sports_martial_arts" )]
    public class USPPistol : BaseCombatWeapon
    {
        [Property] public float RecoilPitch { get; set; } = 1.5f;
        [Property] public float RecoilYaw { get; set; } = 0.6f;
        [Property] public float CameraKick { get; set; } = 2.2f;
        [Property] public float MuzzleFlashDuration { get; set; } = 0.05f;
        [Property] public string ShootSoundInterior { get; set; } = "weapon.usp.shoot";
        [Property] public string ShootSoundExterior { get; set; } = "weapon.usp.shoot.outdoor";
        [Property] public string DryFireSound { get; set; } = "weapon.usp.dry";
        [Property] public string ReloadSound { get; set; } = "weapon.usp.reload";
        [Property] public string ImpactSound { get; set; } = "impact.generic";

        private GameObject _muzzleFlash;
        private GameObject _muzzleSmoke;
        private GameObject _casing;
        private TimeSince _sinceMuzzleFlash;
        private TimeSince _sinceMuzzleSmoke;
        private float _kickCurrent;
        private float _kickTarget;
        private bool _initializedFx;

        protected override void OnStart()
        {
            base.OnStart();
            BaseDamage = 15f;
            FireRate = 0.25f;
            MaxAmmo = 12;
            ReloadTime = 1.5f;
            IsAutomatic = false;
            Range = 4000f;
            AmmoType = "ammo_9mm";
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Camera kick decay (propietario).
            if ( !IsProxy && _kickCurrent != 0f )
            {
                _kickCurrent = MathX.Lerp( _kickCurrent, 0f, Time.Delta * 10f );
                if ( Math.Abs( _kickCurrent ) < 0.02f )
                    _kickCurrent = 0f;
            }

            // Apagar el flash y el humo.
            if ( _muzzleFlash is not null && _muzzleFlash.Enabled && _sinceMuzzleFlash > MuzzleFlashDuration )
                _muzzleFlash.Enabled = false;

            if ( _muzzleSmoke is not null && _muzzleSmoke.Enabled && _sinceMuzzleSmoke > 0.4f )
                _muzzleSmoke.Enabled = false;
        }

        // ------------------------------------------------------------------
        // Disparo
        // ------------------------------------------------------------------

        [Rpc.Broadcast]
        protected override void DoFireEffects()
        {
            base.DoFireEffects();

            EnsureFxObjects();

            // Muzzle flash.
            if ( _muzzleFlash is not null )
            {
                _muzzleFlash.Enabled = true;
                _sinceMuzzleFlash = 0f;
            }

            // Humo.
            if ( _muzzleSmoke is not null )
            {
                _muzzleSmoke.Enabled = true;
                _sinceMuzzleSmoke = 0f;
            }

            // Casquillo.
            SpawnCasing();

            // Sonido interior/exterior.
            var soundName = AudioEnvironment.IsIndoor( Scene, WorldPosition )
                ? ShootSoundInterior
                : ShootSoundExterior;
            Sound.Play( soundName, WorldPosition );

            // Recoil visual del propietario.
            if ( !IsProxy && IsLocalOwner() )
            {
                ApplyRecoil();
            }

            // Ruido para la IA (investigación de disparos).
            WeaponNoise.Emit( WorldPosition, 2200f );
        }

        [Rpc.Broadcast]
        protected override void RpcDoReloadEffects()
        {
            base.RpcDoReloadEffects();
            Sound.Play( ReloadSound, WorldPosition );
        }

        [Rpc.Broadcast]
        protected override void OnDryFire()
        {
            base.OnDryFire();
            Sound.Play( DryFireSound, WorldPosition );
        }

        [Rpc.Broadcast]
        protected override void DoHitEffects( Vector3 position, Vector3 normal )
        {
            base.DoHitEffects( position, normal );

            // Marca de impacto procedural (pequeño disco oscuro).
            var impact = new GameObject( true, "ImpactMark" );
            impact.WorldPosition = position + normal * 1.5f;
            impact.WorldRotation = Rotation.LookAt( normal );

            var renderer = impact.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/dev/box.vmdl" );
            renderer.Tint = new Color( 0.1f, 0.1f, 0.12f, 0.8f );
            impact.LocalScale = new Vector3( 4f, 4f, 0.6f );

            Sound.Play( ImpactSound, position );

            _ = DestroyAfterAsync( impact, 8f );
        }

        // ------------------------------------------------------------------
        // Efectos procedurales
        // ------------------------------------------------------------------

        private void EnsureFxObjects()
        {
            if ( _initializedFx )
                return;
            _initializedFx = true;

            // Flash: caja diminuta brillante frente a la boca.
            _muzzleFlash = new GameObject( true, "MuzzleFlash" );
            _muzzleFlash.SetParent( GameObject );
            _muzzleFlash.LocalPosition = Vector3.Forward * 14f;
            var flashRenderer = _muzzleFlash.Components.Create<ModelRenderer>();
            flashRenderer.Model = Model.Load( "models/dev/box.vmdl" );
            flashRenderer.Tint = new Color( 1f, 0.9f, 0.5f, 1f );
            _muzzleFlash.LocalScale = new Vector3( 3f, 3f, 3f );
            _muzzleFlash.Enabled = false;

            // Humo: caja translúcida que se apaga a los 0.4s.
            _muzzleSmoke = new GameObject( true, "MuzzleSmoke" );
            _muzzleSmoke.SetParent( GameObject );
            _muzzleSmoke.LocalPosition = Vector3.Forward * 20f;
            var smokeRenderer = _muzzleSmoke.Components.Create<ModelRenderer>();
            smokeRenderer.Model = Model.Load( "models/dev/box.vmdl" );
            smokeRenderer.Tint = new Color( 0.7f, 0.7f, 0.72f, 0.35f );
            _muzzleSmoke.LocalScale = new Vector3( 6f, 6f, 6f );
            _muzzleSmoke.Enabled = false;
        }

        private void SpawnCasing()
        {
            if ( _casing is not null && _casing.IsValid() )
                _casing.Destroy();

            _casing = new GameObject( true, "ShellCasing" );
            _casing.WorldPosition = WorldPosition + Vector3.Up * 30f;

            var renderer = _casing.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/dev/box.vmdl" );
            renderer.Tint = new Color( 0.8f, 0.6f, 0.2f );
            _casing.LocalScale = new Vector3( 1.4f, 1.4f, 3f );

            var collider = _casing.Components.Create<BoxCollider>();
            collider.Scale = new Vector3( 1f, 1f, 1f );

            var body = _casing.Components.Create<Rigidbody>();
            body.Velocity = WorldRotation.Forward * 120f + Vector3.Up * 160f + Vector3.Right * 40f;

            _ = DestroyAfterAsync( _casing, 4f );
        }

        private static async System.Threading.Tasks.Task DestroyAfterAsync( GameObject go, float seconds )
        {
            await GameTask.DelayRealtimeSeconds( seconds );
            if ( go.IsValid() )
                go.Destroy();
        }

        private void ApplyRecoil()
        {
            var controller = Components.GetInAncestorsOrSelf<Sandbox.PlayerController>();
            if ( controller is not null )
            {
                var angles = controller.EyeAngles;
                angles.pitch -= RecoilPitch;
                angles.yaw += ( Random.Shared.NextSingle() * 2f - 1f ) * RecoilYaw;
                controller.EyeAngles = angles;
            }

            _kickTarget = CameraKick;
            _kickCurrent = _kickTarget;

            var camera = Scene.Camera;
            if ( camera is not null )
            {
                var localOffset = camera.LocalPosition.WithZ( camera.LocalPosition.z - CameraKick * 0.2f );
                camera.LocalPosition = localOffset;
            }
        }

        private bool IsLocalOwner()
        {
            var local = Connection.Local;
            if ( local is null )
                return true; // Solitario.

            return GameObject.Network.OwnerId == local.Id;
        }
    }

    /// <summary>Heurística interior/exterior para seleccionar el sonido del arma.</summary>
    public static class AudioEnvironment
    {
        public static bool IsIndoor( Scene scene, Vector3 position )
        {
            if ( scene is null )
                return false;

            // Trazo hacia arriba: si hay techo a menos de 350 unidades → interior.
            var tr = scene.Trace
                .Ray( position + Vector3.Up * 100f, position + Vector3.Up * 400f )
                .Run();

            return tr.Hit && tr.Distance < 350f;
        }
    }
}
