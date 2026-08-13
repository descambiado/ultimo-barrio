using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Apartments;

namespace UltimoBarrio.Combat
{
	/// <summary>
	/// Player Death Handler — cierra el bucle de muerte/respawn del jugador.
	///
	/// El HealthComponent ya emite OnDeath (broadcast) y tiene Respawn (host-only),
	/// pero nada los conectaba: el jugador moría y se quedaba muerto sin volver.
	/// Este componente:
	///   - en el host, se suscribe a OnDeath y programa un respawn tras un retardo;
	///   - busca el punto de respawn: el spawn del apartamento del jugador si lo
	///     tiene reclamado, o el spawn point del NetworkHelper como fallback.
	/// </summary>
	[Title( "Player Death Handler" )]
	[Category( "Último Barrio — Combat" )]
	[Icon( "healing" )]
	public sealed class PlayerDeathHandler : Component
	{
		[Property] public float RespawnDelay { get; set; } = 5f;

		private HealthComponent _health;
		private TimeSince _sinceDeath;
		private bool _dead;
		private bool _respawnAtHome;
		private Vector3 _respawnPosition;

		protected override void OnStart()
		{
			_health = Components.Get<HealthComponent>();
			if ( _health != null )
			{
				_health.OnDeath += HandleDeath;
			}
		}

		protected override void OnDestroy()
		{
			if ( _health != null )
			{
				_health.OnDeath -= HandleDeath;
			}
		}

		private void HandleDeath()
		{
			// OnDeath se emite por broadcast; solo el host gestiona el respawn.
			if ( !Networking.IsHost ) return;

			if ( _dead ) return;
			_dead = true;
			_sinceDeath = 0f;
			( _respawnPosition, _respawnAtHome ) = FindRespawnPosition();

			Log.Info( $"[DeathHandler] {GameObject.Name} murió. Respawn en {_respawnPosition} tras {RespawnDelay}s" );
		}

		protected override void OnUpdate()
		{
			if ( !Networking.IsHost || !_dead ) return;

			if ( _sinceDeath >= RespawnDelay )
			{
				_dead = false;
				_health?.Respawn( _respawnPosition );
				Log.Info( $"[DeathHandler] {GameObject.Name} reapareció en {_respawnPosition}" );

				// Objetivo de misión: "Regresa a casa" (solo si el respawn fue en el
				// apartamento reclamado del jugador).
				if ( _respawnAtHome )
				{
					Missions.MissionJournal.Local?.NotifyProgress( Missions.ObjectiveType.ReturnHome );
				}
			}
		}

		/// <summary>
		/// Punto de respawn: spawn del apartamento del jugador si lo tiene; si no,
		/// el primer SpawnPoint del NetworkHelper; si no, la posición actual.
		/// Devuelve (posición, esApartamentoPropio).
		/// </summary>
		private (Vector3 Position, bool IsHome) FindRespawnPosition()
		{
			var identity = Core.PlayerIdentity.FromGameObject( GameObject );

			// 1) Apartamento reclamado por este jugador.
			var apartment = Scene.GetAllComponents<ApartmentComponent>()
				.FirstOrDefault( a => a.ClaimState == ApartmentClaimState.Claimed
					&& !string.IsNullOrEmpty( a.OwnerId )
					&& a.OwnerId == identity.CanonicalId );

			if ( apartment != null && apartment.SpawnReference.IsValid() )
			{
				return ( apartment.SpawnReference.WorldPosition, true );
			}

			// 2) Spawn point del NetworkHelper.
			var helper = Scene.GetAllComponents<NetworkHelper>().FirstOrDefault();
			if ( helper != null && helper.SpawnPoints != null && helper.SpawnPoints.Count > 0 )
			{
				var first = helper.SpawnPoints.FirstOrDefault( p => p != null && p.IsValid() );
				if ( first != null )
				{
					return ( first.WorldPosition, false );
				}
			}

			// 3) Fallback: posición actual.
			return ( WorldPosition, false );
		}
	}
}
