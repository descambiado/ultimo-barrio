using Sandbox;
using System;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>
	/// Autoridad de host para estructuras construibles del pack de contenido.
	/// - No depende del core antiguo (ni InventoryComponent ni PlayerInteractor).
	/// - Autoridad de host: spawn / damage / repair / upgrade / destroy se deciden en host.
	/// - HP expuesto vía IDamageTarget (Content) para la escalera de daño real.
	/// - Repair consume recursos a través de un delegado (LabResourceFixture del lab en
	///   el spike; el core nuevo conectará su inventario aquí — NO InventoryComponent).
	/// - Upgrade cambia DEFINICIÓN en caliente hacia UpgradeTo (modelo + MaxHp).
	/// </summary>
	[Title( "Content Build Structure Host" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "shield" )]
	public sealed class BuildStructureHost : Component, IFortificationContentAdapter, IDamageTarget
	{
		[RequireComponent] public BoxCollider Collider { get; set; }

		[Property] public string DefinitionId { get; set; } = "";

		public BuildDefinition Definition { get; private set; }

		[Sync] public float Health { get; private set; }
		public bool IsDead => Health <= 0f;

		protected override void OnStart()
		{
			Definition = FortificationContentRegistry.Get( DefinitionId );
			if ( Definition == null )
			{
				Log.Error( $"[BuildHost] DefinitionId '{DefinitionId}' no registrada en FortificationContentRegistry" );
				return;
			}

			ApplyDefinitionToRenderer();

			if ( Networking.IsHost )
			{
				Health = Definition.MaxHp;
			}

			Log.Info( $"[BuildHost] {Definition.Id} spawned (HP {Health:F0}/{Definition.MaxHp:F0}, model {ResolvedModelName()})" );
		}

		/// <summary>Autoridad de host: instancia el prefab del BuildDefinition y lo registra en red.</summary>
		public static BuildStructureHost SpawnBuild( BuildDefinition def, Vector3 position, Rotation rotation )
		{
			if ( !Networking.IsHost )
			{
				Log.Warning( "[BuildHost] SpawnBuild fuera del host ignorado" );
				return null;
			}

			if ( def == null )
			{
				Log.Error( "[BuildHost] SpawnBuild sin definición" );
				return null;
			}

			var prefabFile = ResourceLibrary.Get<PrefabFile>( def.Prefab );
			if ( prefabFile == null )
			{
				Log.Error( $"[BuildHost] SpawnBuild: prefab no encontrado '{def.Prefab}'" );
				return null;
			}

			var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
			if ( prefabScene == null )
			{
				Log.Error( $"[BuildHost] SpawnBuild: prefab scene inválida '{def.Prefab}'" );
				return null;
			}

			var go = prefabScene.Clone();
			go.WorldPosition = position;
			go.WorldRotation = rotation;
			go.NetworkSpawn( Connection.Local );

			var host = go.Components.Get<BuildStructureHost>();
			if ( host == null )
			{
				Log.Error( $"[BuildHost] SpawnBuild: '{def.Prefab}' no contiene BuildStructureHost" );
			}

			return host;
		}

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( !Networking.IsHost || IsDead ) return;

			Health -= damageEvent.Amount;
			Health = MathF.Max( 0f, Health );

			Log.Info( $"[BuildHost] {Definition?.Id} damage {damageEvent.Amount:F0} (src '{damageEvent.SourceId}') → HP {Health:F0}" );

			if ( Health <= 0f )
			{
				Log.Info( $"[BuildHost] {Definition?.Id} DESTROYED" );
				GameObject.Destroy();
			}
		}

		/// <summary>
		/// Repara en host consumiendo recursos vía tryConsume (devuelve false si no hay
		/// presupuesto o no es host). El spike inyecta el LabResourceFixture; el core
		/// nuevo conectará el inventario canónico por el mismo delegado.
		/// </summary>
		public bool Repair( float amount, Func<int, bool> tryConsume )
		{
			if ( !Networking.IsHost || Definition == null || IsDead ) return false;

			if ( tryConsume != null && !tryConsume( Definition.RepairCost ) )
			{
				Log.Info( $"[BuildHost] {Definition.Id} repair RECHAZADO (recursos insuficientes: {Definition.RepairCost})" );
				return false;
			}

			Health = MathF.Min( Definition.MaxHp, Health + amount );
			Log.Info( $"[BuildHost] {Definition.Id} repaired +{amount:F0} (coste {Definition.RepairCost}) → HP {Health:F0}/{Definition.MaxHp:F0}" );
			return true;
		}

		/// <summary>Mejora en caliente: cambia DEFINICIÓN (modelo + MaxHp) hacia UpgradeTo.</summary>
		public void Upgrade()
		{
			if ( !Networking.IsHost || Definition == null || IsDead ) return;
			if ( string.IsNullOrEmpty( Definition.UpgradeTo ) ) return;

			var next = FortificationContentRegistry.Get( Definition.UpgradeTo );
			if ( next == null )
			{
				Log.Error( $"[BuildHost] {Definition.Id} upgrade: '{Definition.UpgradeTo}' no registrada" );
				return;
			}

			string previousModel = ResolvedModelName();
			Definition = next;
			ApplyDefinitionToRenderer();
			Health = Definition.MaxHp;

			Log.Info( $"[BuildHost] {Definition.Id} UPGRADED (model {previousModel} → {ResolvedModelName()}) → HP {Health:F0}/{Definition.MaxHp:F0}" );
		}

		private void ApplyDefinitionToRenderer()
		{
			var renderer = Components.GetInChildrenOrSelf<ModelRenderer>();
			if ( renderer != null )
			{
				var model = ResolveModel();
				if ( model != null ) renderer.Model = model;
			}

			if ( Definition.Scale != 1f )
			{
				WorldScale = Definition.Scale;
			}
		}

		/// <summary>Modelo primario → fallback. Si nada resuelve, se conserva el modelo del prefab.</summary>
		private Model ResolveModel()
		{
			if ( !string.IsNullOrEmpty( Definition.Model ) )
			{
				var model = ResourceLibrary.Get<Model>( Definition.Model );
				if ( model != null ) return model;
			}

			if ( !string.IsNullOrEmpty( Definition.ModelFallback ) )
			{
				return ResourceLibrary.Get<Model>( Definition.ModelFallback );
			}

			return null;
		}

		private string ResolvedModelName()
		{
			return Components.GetInChildrenOrSelf<ModelRenderer>()?.Model?.ResourceName ?? "none";
		}
	}
}
