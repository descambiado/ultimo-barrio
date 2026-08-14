using Sandbox;
using System;

namespace UltimoBarrio.Content.Enemies
{
	/// <summary>
	/// Implementación autocontenida de IEnemyContentAdapter para el pack de contenido.
	///
	/// - Sin dependencias del core antiguo (ni AIBase, ni PerceptionComponent, ni HealthComponent).
	/// - NavMeshAgent y colisión los garantiza [RequireComponent].
	/// - Autoridad de host: daño y muerte en host; el resto se sincroniza con [Sync].
	/// - El botín se instancia como pickups de mundo (WorldPrefab) al morir.
	/// </summary>
	[Title( "Content Enemy Host" )]
	[Category( "Último Barrio — Content" )]
	[Icon( "skull" )]
	public sealed class EnemyContentHost : Component, IEnemyContentAdapter, IDamageTarget
	{
		[RequireComponent] public NavMeshAgent Agent { get; set; }
		[RequireComponent] public BoxCollider Collider { get; set; }

		[Property] public string DefinitionId { get; set; } = "";
		[Property] public GameObject Target { get; set; }

		public EnemyArchetypeDefinition Definition { get; private set; }

		[Sync] public float Health { get; private set; }
		public bool IsDead => Health <= 0f;

		private TimeSince _timeSinceAttack;
		private TimeSince _timeSinceSpawn;

		protected override void OnStart()
		{
			Definition = EnemyContentRegistry.GetEnemy( DefinitionId );
			if ( Definition == null )
			{
				Log.Error( $"[Content.Enemy] DefinitionId '{DefinitionId}' no registrada en EnemyContentRegistry" );
				return;
			}

			ApplyDefinitionToRenderer();

			if ( Networking.IsHost )
			{
				Health = Definition.MaxHealth;
			}

			_timeSinceSpawn = 0f;
		}

		protected override void OnUpdate()
		{
			if ( Definition == null || IsDead || IsProxy ) return;

			if ( _timeSinceSpawn < 1f ) return; // pequeño retardo de aparición

			TickBehavior();
		}

		private void TickBehavior()
		{
			if ( Target == null || !Target.IsValid() ) return;

			float distance = Vector3.DistanceBetween( WorldPosition, Target.WorldPosition );

			if ( distance > Definition.AttackRange )
			{
				Agent.MoveTo( Target.WorldPosition );
			}
			else
			{
				Agent.Stop();
				TryAttack();
			}
		}

		private void TryAttack()
		{
			if ( _timeSinceAttack < Definition.AttackCooldown ) return;

			_timeSinceAttack = 0f;
			RpcAttackEffects();

			var target = Target.Components.GetInAncestorsOrSelf<IDamageTarget>();
			if ( target != null && !target.IsDead )
			{
				target.TakeDamage( new ContentDamageEvent
				{
					Amount = Definition.AttackDamage,
					Position = WorldPosition,
					Force = ( Target.WorldPosition - WorldPosition ).Normal * 50f,
					SourceId = Definition.Id
				} );
			}
		}

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( !Networking.IsHost || IsDead ) return;

			Health -= damageEvent.Amount;
			Health = MathF.Max( 0f, Health );

			RpcDamageFeedback( damageEvent.Amount, damageEvent.Position, damageEvent.SourceId );

			if ( Health <= 0f )
			{
				Die();
			}
		}

		public void SetTarget( GameObject target )
		{
			Target = target;
		}

		private void Die()
		{
			if ( !Networking.IsHost ) return;

			SpawnLoot();
			RpcDeathEffects();
			GameObject.Destroy();
		}

		private void SpawnLoot()
		{
			var table = EnemyContentRegistry.GetLootTable( Definition?.LootTableId );
			if ( table == null ) return;

			int index = 0;
			foreach ( var entry in table.Entries )
			{
				if ( entry.Chance < 1f && Game.Random.Float( 0f, 1f ) > entry.Chance ) continue;

				int amount = Game.Random.Int( entry.Min, entry.Max );
				if ( string.IsNullOrEmpty( entry.WorldPrefab ) ) continue;

				for ( int i = 0; i < amount; i++ )
				{
					SpawnPickup( entry.WorldPrefab, index );
					index++;
				}
			}
		}

		private void SpawnPickup( string prefabPath, int index )
		{
			var prefabFile = ResourceLibrary.Get<PrefabFile>( prefabPath );
			if ( prefabFile == null ) return;

			var scene = SceneUtility.GetPrefabScene( prefabFile );
			if ( scene == null ) return;

			var pickup = scene.Clone();
			pickup.WorldPosition = WorldPosition + Vector3.Up * 20f + Vector3.Random.WithZ( 0f ) * 24f;
			pickup.NetworkSpawn( Connection.Local );
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
		private void RpcAttackEffects()
		{
			// TODO(core nuevo): animación de ataque y sonido desde datos del pack.
		}

		[Rpc.Broadcast]
		private void RpcDamageFeedback( float amount, Vector3 position, string sourceId )
		{
			// TODO(core nuevo): feedback visual (flash, sangre, sonido).
		}

		[Rpc.Broadcast]
		private void RpcDeathEffects()
		{
			// TODO(core nuevo): death anim / ragdoll desde datos del pack (CorpseLifetime).
		}
	}
}
