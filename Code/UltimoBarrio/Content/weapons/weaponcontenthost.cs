using Sandbox;
using System;

namespace UltimoBarrio.Content.Weapons
{
	/// <summary>
	/// Implementación autocontenida de IWeaponContentAdapter para el pack de contenido.
	///
	/// - No depende del core antiguo (ni InventoryComponent ni HeldItemController).
	/// - Autoridad de host: el daño se aplica en host; efectos en broadcast.
	/// - Daño dirigido a IDamageTarget (Content). El core nuevo mapeará ese contrato.
	/// - Los prefabs del pack referencian DefinitionId; el resto son datos.
	/// </summary>
	[Title( "Content Weapon Host" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "gps_fixed" )]
	public sealed class WeaponContentHost : Component, IWeaponContentAdapter
	{
		[Property] public string DefinitionId { get; set; } = "";
		[Property] public bool AutoHandleInput { get; set; } = true;

		public WeaponContentDefinition Definition { get; private set; }

		[Sync] public int CurrentAmmo { get; private set; }
		[Sync] public bool IsReloading { get; private set; }
		public bool CanFire => !IsReloading && ( IsMelee || CurrentAmmo > 0 );

		private bool IsMelee => Definition != null && Definition.Category == WeaponContentCategory.Melee;

		private TimeSince _lastFired;
		private TimeUntil _reloadComplete;

		protected override void OnStart()
		{
			Definition = WeaponContentRegistry.Get( DefinitionId );
			if ( Definition == null )
			{
				Log.Error( $"[Content.Weapon] DefinitionId '{DefinitionId}' no registrada en WeaponContentRegistry" );
				return;
			}

			ApplyDefinitionToRenderer();

			if ( Networking.IsHost && !IsMelee )
			{
				CurrentAmmo = Definition.MagazineSize;
			}
		}

		protected override void OnUpdate()
		{
			if ( Definition == null ) return;

			if ( !IsProxy && AutoHandleInput )
			{
				HandleInput();
			}

			if ( Networking.IsHost && IsReloading && _reloadComplete )
			{
				FinishReload();
			}
		}

		private void ApplyDefinitionToRenderer()
		{
			var renderer = Components.GetInChildrenOrSelf<ModelRenderer>();
			if ( renderer == null ) return;

			var model = ResolveModel();
			if ( model != null )
			{
				renderer.Model = model;
			}
		}

		/// <summary>Devuelve el modelo primario si está verificado; si no, el fallback.</summary>
		private Model ResolveModel()
		{
			if ( !string.IsNullOrEmpty( Definition.WorldModel ) && Definition.AssetsVerified )
			{
				return ResourceLibrary.Get<Model>( Definition.WorldModel );
			}

			if ( !string.IsNullOrEmpty( Definition.WorldModelFallback ) )
			{
				return ResourceLibrary.Get<Model>( Definition.WorldModelFallback );
			}

			return null;
		}

		private void HandleInput()
		{
			if ( IsReloading ) return;

			bool wantToFire = Definition.IsAutomatic ? Input.Down( "attack1" ) : Input.Pressed( "attack1" );
			if ( wantToFire && _lastFired >= Definition.FireRate )
			{
				if ( IsMelee || CurrentAmmo > 0 )
				{
					Fire();
				}
				else
				{
					DryFire();
				}
			}

			if ( !IsMelee && Input.Pressed( "reload" ) && CurrentAmmo < Definition.MagazineSize )
			{
				Reload();
			}
		}

		public void Fire()
		{
			if ( Definition == null ) return;
			_lastFired = 0f;

			if ( Networking.IsHost )
			{
				PerformHostFire();
			}
			else
			{
				RpcRequestFire();
			}

			RpcFireEffects();
		}

		public void DryFire()
		{
			_lastFired = 0f;
			RpcDryFireEffects();
		}

		public void Reload()
		{
			if ( Definition == null || IsReloading || IsMelee || CurrentAmmo >= Definition.MagazineSize ) return;

			if ( Networking.IsHost )
			{
				StartReload();
			}
			else
			{
				RpcRequestReload();
			}
		}

		[Rpc.Host]
		private void RpcRequestFire()
		{
			PerformHostFire();
		}

		[Rpc.Host]
		private void RpcRequestReload()
		{
			StartReload();
		}

		private void PerformHostFire()
		{
			if ( !IsMelee )
			{
				if ( CurrentAmmo <= 0 ) return;
				CurrentAmmo--;
			}

			PerformTrace();
		}

		private void StartReload()
		{
			IsReloading = true;
			_reloadComplete = Definition.ReloadTime;
		}

		private void FinishReload()
		{
			IsReloading = false;
			CurrentAmmo = Definition.MagazineSize;
			// Nota: el consumo de munición del inventario lo hará el adaptador del core nuevo.
		}

		private void PerformTrace()
		{
			var ray = Scene.Camera.ScreenNormalToRay( 0.5f );
			float range = IsMelee ? Definition.MeleeRange : Definition.Range;

			for ( int i = 0; i < Definition.Pellets; i++ )
			{
				var traceRay = ray;
				if ( Definition.Pellets > 1 )
				{
					// Dispersión simple para escopeta (data-only; el core nuevo hará spread configurable).
					var spread = Vector3.Random.WithZ( MathF.Abs( Vector3.Random.z ) ) * 40f;
					traceRay = Scene.Camera.ScreenNormalToRay( 0.5f + spread * 0.0001f );
				}

				var tr = Scene.Trace.Ray( traceRay, range )
					.IgnoreGameObjectHierarchy( GameObject.Root )
					.Run();

				if ( !tr.Hit ) continue;

				var target = tr.GameObject.Components.GetInAncestorsOrSelf<IDamageTarget>();
				if ( target != null && !target.IsDead )
				{
					target.TakeDamage( new ContentDamageEvent
					{
						Amount = Definition.Damage,
						Position = tr.HitPosition,
						Force = traceRay.Forward * 100f,
						SourceId = Definition.Id,
						AttackerId = Connection.Local?.Id.ToString() ?? ""
					} );
				}

				RpcHitEffects( tr.HitPosition, tr.Normal );
			}
		}

		[Rpc.Broadcast]
		private void RpcFireEffects()
		{
			if ( Definition == null ) return;

			if ( !string.IsNullOrEmpty( Definition.FireSound ) )
			{
				Sound.Play( Definition.FireSound, WorldPosition );
			}

			if ( !string.IsNullOrEmpty( Definition.MuzzleEffect ) )
			{
				// TODO(core nuevo): Scene.Particles / ParticleEffect desde MuzzleEffect.
			}
		}

		[Rpc.Broadcast]
		private void RpcDryFireEffects()
		{
			if ( Definition != null && !string.IsNullOrEmpty( Definition.DryFireSound ) )
			{
				Sound.Play( Definition.DryFireSound, WorldPosition );
			}
		}

		[Rpc.Broadcast]
		private void RpcHitEffects( Vector3 position, Vector3 normal )
		{
			// TODO(core nuevo): impact particles/sounds desde datos del pack.
		}
	}
}
