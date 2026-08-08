using Sandbox;
using System;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Dummy de daño para los labs de contenido (SOLO dev).
	/// Implementa IDamageTarget del paquete portable y loguea cada impacto
	/// para verificar la escalera DAMAGES sin tocar el core viejo.
	/// </summary>
	[Title( "Lab Damage Dummy" )]
	[Category( "Último Barrio — Content (Dev)" )]
	public sealed class LabDamageDummy : Component, IDamageTarget
	{
		[Property] public float MaxHealth { get; set; } = 100f;

		/// <summary>Prefijo de los logs de impacto (cada rig usa el suyo: [WeaponLab], [EnemyLab]...).</summary>
		[Property] public string LogPrefix { get; set; } = "[Lab]";

		public float Health { get; private set; } = 100f;
		public bool IsDead => Health <= 0f;

		/// <summary>Devuelve el dummy a su salud máxima (uso del rig entre tests).</summary>
		public void ResetHealth()
		{
			Health = MaxHealth;
		}

		protected override void OnStart()
		{
			Health = MaxHealth;
		}

		public void TakeDamage( ContentDamageEvent damageEvent )
		{
			if ( IsDead ) return;

			Health -= damageEvent.Amount;
			Health = MathF.Max( 0f, Health );

			Log.Info( $"{LogPrefix} Dummy recibió {damageEvent.Amount:F1} de '{damageEvent.SourceId}' → HP {Health:F1}/{MaxHealth:F0}" );

			if ( Health <= 0f )
			{
				Log.Info( $"{LogPrefix} Dummy destruido (HP 0)" );
			}
		}
	}
}
