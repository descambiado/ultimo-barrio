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
			// Tags para que los enemigos puedan percibir y atacar la estructura:
			//   "enemy_target"  → candidato para EnemyPerception.FindBestCandidate.
			//   "fortification" → preferencia del Bruto (StructureTag) sobre el jugador.
			GameObject.Tags.Add( "fortification" );
			GameObject.Tags.Add( "enemy_target" );

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

			Sound.Play( "sounds/content/fortification/repair.sound", position );
			return host;
		}

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( !Networking.IsHost || IsDead ) return;

			Health -= damageEvent.Amount;
			Health = MathF.Max( 0f, Health );

			Log.Info( $"[BuildHost] {Definition?.Id} damage {damageEvent.Amount:F0} (src '{damageEvent.SourceId}') → HP {Health:F0}" );

			RpcImpactSound( damageEvent.Position );

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
			RpcRepairSound();
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
				var model = ResolveModel() ?? ResolveModelFromPrefab();
				if ( model != null ) renderer.Model = model;
				else if ( !string.IsNullOrEmpty( Definition.Model ) )
				{
					Log.Warning( $"[BuildHost] {Definition.Id}: modelo no resuelto '{Definition.Model}' (ni ruta ni prefab) — se conserva el del prefab" );
				}
			}

			if ( Definition.Scale != 1f )
			{
				WorldScale = Definition.Scale;
			}
		}

		/// <summary>Modelo primario → fallback. Si nada resuelve, se conserva el modelo del prefab.</summary>
		private Model ResolveModel()
		{
			var model = ResolveModelPath( Definition.Model );
			if ( model != null ) return model;

			model = ResolveModelPath( Definition.ModelFallback );
			if ( model == null && !string.IsNullOrEmpty( Definition.Model ) )
			{
				Log.Warning( $"[BuildHost] {Definition.Id}: modelo no resuelto '{Definition.Model}' — se conserva el del prefab" );
			}
			return model;
		}

		/// <summary>Fallback: modelo ya resuelto por ident dentro del PREFAB destino (los assets del engine no resuelven por ruta).</summary>
		private Model ResolveModelFromPrefab()
		{
			if ( string.IsNullOrEmpty( Definition.Prefab ) ) return null;

			var prefabFile = ResourceLibrary.Get<PrefabFile>( Definition.Prefab );
			var scene = prefabFile == null ? null : SceneUtility.GetPrefabScene( prefabFile );
			var renderer = scene?.GetAllComponents<ModelRenderer>()?.FirstOrDefault();
			return renderer?.Model;
		}

		/// <summary>Resuelve una ruta de modelo tolerando la extensión .vmdl opcional (Get&lt;Model&gt; es estricto).</summary>
		private static Model ResolveModelPath( string path )
		{
			if ( string.IsNullOrEmpty( path ) ) return null;

			var model = ResourceLibrary.Get<Model>( path );
			if ( model == null && path.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase ) )
			{
				model = ResourceLibrary.Get<Model>( path[..^5] );
			}
			return model;
		}

		private string ResolvedModelName()
		{
			return Components.GetInChildrenOrSelf<ModelRenderer>()?.Model?.ResourceName ?? "none";
		}

		[Rpc.Broadcast]
		private void RpcImpactSound( Vector3 position )
		{
			Sound.Play( "sounds/content/fortification/barricade_impact.sound", position );
		}

		[Rpc.Broadcast]
		private void RpcRepairSound()
		{
			Sound.Play( "sounds/content/fortification/repair.sound", WorldPosition );
		}
	}
}
