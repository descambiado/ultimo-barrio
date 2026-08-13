using Sandbox;
using System;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Enemy Content Host — implementación autocontenida de IEnemyContentAdapter e IDamageTarget.
	///
	/// UN brain data-driven (no tres copiados): la definición (EnemyContentRegistry)
	/// decide stats, percepción (EnemyPerception), ataque (EnemyAttack) y navegación.
	/// - Navegación con NavMeshAgent REAL (prohibido teleport).
	/// - Daño de entrada y salida por la ruta real (ContentDamageEvent / IDamageTarget).
	/// - Muerte con loot FÍSICO: instancia pickups como objetos de mundo, NUNCA
	///   inventario canónico (eso lo mapeará el core nuevo vía IEnemyContentAdapter).
	/// - Autoridad de host: daño y muerte en host; estado sincronizado con [Sync].
	/// </summary>
	[Title( "Content Enemy Host" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "skull" )]
	public sealed class EnemyContentHost : Component, IEnemyContentAdapter, IDamageTarget
	{
		[RequireComponent] public NavMeshAgent Agent { get; set; }
		[RequireComponent] public EnemyPerception Perception { get; set; }
		[RequireComponent] public EnemyAttack Attack { get; set; }

		[Property] public string DefinitionId { get; set; } = "";
		[Property] public GameObject Target { get; set; }

		public EnemyArchetypeDefinition Definition { get; private set; }

		[Sync] public float Health { get; private set; }
		public bool IsDead => Health <= 0f;

		// Idempotencia de la transicion de muerte (separada de IsDead: Health<=0 es
		// estado de salud; _deathHandled garantiza que Die() corre exactamente 1 vez).
		private bool _deathHandled;

		// Lecturas para el rig / core nuevo
		public bool IsTargetAcquired => Perception?.CurrentTarget != null;
		public Vector3? LastKnownPosition => Perception?.LastKnownPosition;
		public float DistanceToTarget => Target != null && Target.IsValid()
			? Vector3.DistanceBetween( WorldPosition, Target.WorldPosition )
			: -1f;

		private TimeSince _timeSinceSpawn;
		private bool _configApplied;
		private float _refreshPathTimer;

		protected override void OnStart()
		{
			Definition = EnemyContentRegistry.GetEnemy( DefinitionId );
			if ( Definition == null )
			{
				Log.Error( $"[Content.Enemy] DefinitionId '{DefinitionId}' no registrada en EnemyContentRegistry" );
				return;
			}

			if ( Networking.IsHost )
			{
				Health = Definition.MaxHealth;
			}

			_timeSinceSpawn = 0f;
		}

		protected override void OnUpdate()
		{
			if ( Definition == null || IsProxy ) return;

			// El OnStart del host corre tras el frame de creación: aplicamos la
			// definición en el primer OnUpdate para que los logs sean deterministas.
			if ( !_configApplied )
			{
				ApplyDefinition();
			}

			if ( IsDead || _timeSinceSpawn < 0.4f ) return; // retardo de aparición (el agente se asienta en el navmesh)

			Perception.Tick( Target );

			if ( Perception.CurrentTarget != null && Perception.CurrentTarget.IsValid() )
			{
				ChaseOrAttack( Perception.CurrentTarget );
			}
			else
			{
				Agent.Stop();
			}
		}

		private void ApplyDefinition()
		{
			_configApplied = true;

			Perception.Configure( Definition );
			Attack.Configure( Definition );
			Agent.MaxSpeed = Definition.WalkSpeed;

			ApplyDefinitionToRenderer();

			Log.Info( $"[Content.Enemy] {Definition.Id} '{Definition.DisplayName}' | HP {Definition.MaxHealth} | speed {Definition.WalkSpeed} | vision {Definition.VisionRange}u/{Definition.VisionAngle}° | hearing {Definition.HearingRadius}u | dmg {Definition.AttackDamage} | cooldown {Definition.AttackCooldown}s | priority {Definition.TargetPriority} | loot '{Definition.LootTableId}'" );
		}

		private void ChaseOrAttack( GameObject target )
		{
			float distance = Vector3.DistanceBetween( WorldPosition, target.WorldPosition );

			if ( distance > Definition.AttackRange )
			{
				// Navegación REAL por NavMeshAgent: nunca teleport.
				_refreshPathTimer -= Time.Delta;
				if ( !Agent.IsNavigating || _refreshPathTimer <= 0f )
				{
					Agent.MoveTo( target.WorldPosition );
					_refreshPathTimer = 0.5f;
				}
			}
			else
			{
				Agent.Stop();
				var damageTarget = target.Components.GetInAncestorsOrSelf<IDamageTarget>();
				Attack.TryAttack( damageTarget, target.WorldPosition, Definition.Id );
			}
		}

		// --- Contrato de daño (ruta real) ---

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( !Networking.IsHost || _deathHandled ) return;

			Health = MathF.Max( 0f, Health - damageEvent.Amount );
			Log.Info( $"[Content.Enemy] {Definition?.Id} recibió {damageEvent.Amount:F0} de '{damageEvent.SourceId}' → HP {Health:F0}/{Definition?.MaxHealth:F0}" );

			RpcDamageFeedback( damageEvent.Amount, damageEvent.Position, damageEvent.SourceId );

			if ( Health <= 0f )
			{
				Die();
			}
		}

		private void Die()
		{
			if ( !Networking.IsHost || _deathHandled ) return;

			_deathHandled = true;
			Health = 0f;
			Log.Info( $"[Content.Enemy] {Definition?.Id} murió" );

			SpawnLoot();
			RpcDeathEffects();
			GameObject.Destroy();
		}

		// --- Loot físico (pickups de mundo; sin inventario canónico) ---

		private void SpawnLoot()
		{
			var table = EnemyContentRegistry.GetLootTable( Definition?.LootTableId );
			if ( table == null )
			{
				Log.Info( $"[Content.Enemy] {Definition?.Id} sin loot table '{Definition?.LootTableId}'" );
				return;
			}

			int spawned = 0;
			foreach ( var entry in table.Entries )
			{
				if ( entry.Chance < 1f && Game.Random.Float( 0f, 1f ) > entry.Chance ) continue;
				if ( string.IsNullOrEmpty( entry.WorldPrefab ) ) continue;

				int amount = Game.Random.Int( entry.Min, entry.Max );
				for ( int i = 0; i < amount; i++ )
				{
					if ( SpawnPickup( entry.WorldPrefab, entry.ItemId ) ) spawned++;
				}
			}

			Log.Info( $"[Content.Enemy] {Definition?.Id} soltó {spawned} pickups físicos" );
		}

		private bool SpawnPickup( string prefabPath, string itemId )
		{
			var prefabFile = ResourceLibrary.Get<PrefabFile>( prefabPath );
			if ( prefabFile == null )
			{
				Log.Error( $"[Content.Enemy] Loot prefab NO encontrado: {prefabPath}" );
				return false;
			}

			var pickup = SceneUtility.GetPrefabScene( prefabFile ).Clone();
			pickup.WorldPosition = WorldPosition + Vector3.Up * 30f + Vector3.Random.WithZ( 0f ) * 24f;
			pickup.NetworkSpawn( Connection.Local );

			var loot = pickup.Components.Get<LootPickupContent>();
			if ( loot != null )
			{
				loot.ItemId = itemId;
				loot.Amount = 1;
			}

			Log.Info( $"[Content.Enemy] Loot pickup '{itemId}' en {pickup.WorldPosition}" );
			return true;
		}

		// --- IEnemyContentAdapter ---

		public void SetTarget( GameObject target )
		{
			Target = target;
		}

		public void ReportNoise( Vector3 position, float volume )
		{
			if ( !Networking.IsHost ) return;
			Perception.ReportNoise( position, volume );
		}

		// --- Renderer ---

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

		private Model ResolveModel()
		{
			// Primario → fallback verificado (igual que armas).
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

		[Rpc.Broadcast]
		private void RpcDamageFeedback( float amount, Vector3 position, string sourceId )
		{
			Sound.Play( "sounds/content/enemies/enemy_hurt.sound", position );
		}

		[Rpc.Broadcast]
		private void RpcDeathEffects()
		{
			Sound.Play( "sounds/content/enemies/enemy_death.sound", WorldPosition );
		}
	}
}
