using Sandbox;
using System;
using UltimoBarrio.Combat;
using UltimoBarrio.Core;

namespace UltimoBarrio.Content
{
	/// <summary>
	/// Player Content Damage Bridge — convierte al jugador en un IDamageTarget del
	/// content pack (la ruta de daño real de los enemigos del lab).
	///
	/// Los enemigos (EnemyContentHost/EnemyAttack) aplican daño SOLO por
	/// ContentDamageEvent → IDamageTarget, y su percepción detecta candidatos con
	/// tag "enemy_target". Este bridge:
	///   - añade el tag "enemy_target" al jugador (visible/atacable),
	///   - traduce ContentDamageEvent → DamageEvent del HealthComponent del jugador.
	///
	/// Sin esto, los enemigos del lab no pueden ver ni dañar al jugador real.
	/// </summary>
	[Title( "Player Content Damage Bridge" )]
	[Category( "Ultimo Barrio — Content" )]
	[Icon( "shield" )]
	public sealed class PlayerContentDamageBridge : Component, IDamageTarget
	{
		public bool IsDead => _health != null && _health.IsDead;

		private HealthComponent _health;

		protected override void OnStart()
		{
			GameObject.Tags.Add( "enemy_target" );
			_health = Components.Get<HealthComponent>();
		}

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( !Networking.IsHost || _health == null || _health.IsDead ) return;

			_health.TakeDamage( new DamageEvent
			{
				Amount = damageEvent.Amount,
				Position = damageEvent.Position,
				Force = damageEvent.Force,
				AttackerId = damageEvent.AttackerId,
				WeaponId = damageEvent.SourceId
			} );
		}
	}
}
