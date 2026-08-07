using Sandbox;
using System;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>
	/// Implementación autocontenida de IFortificationContentAdapter.
	/// Salud, daño (IDamageTarget), reparación y mejora (upgrade a otro prefab).
	/// Sin dependencias del core antiguo. Host-authoritative.
	/// </summary>
	[Title( "Content Fortification Host" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "shield" )]
	public sealed class FortificationContentHost : Component, IFortificationContentAdapter, IDamageTarget
	{
		[RequireComponent] public BoxCollider Collider { get; set; }

		[Property] public string DefinitionId { get; set; } = "";

		public FortificationContentDefinition Definition { get; private set; }

		[Sync] public float Health { get; private set; }
		public bool IsDead => Health <= 0f;

		protected override void OnStart()
		{
			Definition = FortificationContentRegistry.Get( DefinitionId );
			if ( Definition == null )
			{
				Log.Error( $"[Content.Fortification] DefinitionId '{DefinitionId}' no registrada en FortificationContentRegistry" );
				return;
			}

			ApplyDefinitionToRenderer();

			if ( Networking.IsHost )
			{
				Health = Definition.MaxHealth;
			}
		}

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( !Networking.IsHost || IsDead ) return;

			Health -= damageEvent.Amount;
			Health = MathF.Max( 0f, Health );

			RpcDamageFeedback( damageEvent.Amount, damageEvent.Position );

			if ( Health <= 0f )
			{
				RpcDestroyEffects();
				GameObject.Destroy();
			}
		}

		public void Repair( float amount )
		{
			if ( !Networking.IsHost || Definition == null || IsDead ) return;

			Health = MathF.Min( Definition.MaxHealth, Health + amount );
			RpcRepairEffects();
		}

		public void Upgrade()
		{
			if ( !Networking.IsHost || Definition == null || IsDead ) return;
			if ( string.IsNullOrEmpty( Definition.UpgradePrefab ) ) return;

			var prefabFile = ResourceLibrary.Get<PrefabFile>( Definition.UpgradePrefab );
			if ( prefabFile == null ) return;

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null ) return;

			var upgraded = scene.Clone();
			upgraded.WorldPosition = WorldPosition;
			upgraded.WorldRotation = WorldRotation;
			upgraded.NetworkSpawn( Connection.Local );

			GameObject.Destroy();
		}

		private void ApplyDefinitionToRenderer()
		{
			var renderer = Components.GetInChildrenOrSelf<ModelRenderer>();
			if ( renderer != null )
			{
				var model = !string.IsNullOrEmpty( Definition.Model ) && Definition.AssetsVerified
					? ResourceLibrary.Get<Model>( Definition.Model )
					: ResourceLibrary.Get<Model>( Definition.ModelFallback );

				if ( model != null ) renderer.Model = model;
			}

			if ( Definition.Scale != 1f )
			{
				WorldScale = Definition.Scale;
			}
		}

		[Rpc.Broadcast]
		private void RpcDamageFeedback( float amount, Vector3 position )
		{
			// TODO(core nuevo): feedback visual / sonoro desde datos del pack.
		}

		[Rpc.Broadcast]
		private void RpcRepairEffects()
		{
			// TODO(core nuevo): partículas/sonido de reparación.
		}

		[Rpc.Broadcast]
		private void RpcDestroyEffects()
		{
			// TODO(core nuevo): sonido de destrucción desde datos del pack.
		}
	}
}
