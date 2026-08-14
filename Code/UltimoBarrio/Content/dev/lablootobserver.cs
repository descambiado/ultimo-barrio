using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Content.Enemies;

namespace UltimoBarrio.Content.Dev
{
	/// <summary>
	/// Observador de loot del enemy_lab (SOLO dev).
	///
	/// Punto de observación física: componente situado en el GameObject
	/// "Loot Observation Point" de la escena. Cuando la suite lo activa
	/// (StartObserving), cuenta los pickups REALES que aparecen en el mundo
	/// dentro de un radio del ancla durante una ventana de tiempo.
	///
	/// Anti-falsificación: el observer NO fabrica loot. Solo observa los
	/// pickups que el EnemyContentHost.SpawnLoot instancia por la ruta real
	/// (loot table → WorldPrefab → clone → NetworkSpawn). La detección usa el
	/// componente LootPickupContent del pack (solo LECTURA; cuando el core
	/// nuevo sustituya el pickup, este observer dev se actualiza).
	/// </summary>
	[Title( "Lab Loot Observer" )]
	[Category( "Ultimo Barrio - Content (Dev)" )]
	[Icon( "visibility" )]
	public sealed class LabLootObserver : Component
	{
		[Property] public GameObject Anchor { get; set; }
		[Property] public float Radius { get; set; } = 250f;
		[Property] public float ObserveSeconds { get; set; } = 3f;

		public bool IsObserving { get; private set; }
		public int ObservedCount { get; private set; }
		public IReadOnlyList<string> ObservedItemIds => _observedItemIds;

		private readonly List<string> _observedItemIds = new();
		private readonly HashSet<GameObject> _counted = new();
		private TimeSince _sinceStart;

		/// <summary>Abre la ventana de observación (resetea el contador).</summary>
		public void StartObserving()
		{
			_observedItemIds.Clear();
			_counted.Clear();
			ObservedCount = 0;
			_sinceStart = 0f;
			IsObserving = true;
			Log.Info( $"[EnemyLab] LootObserver window open ({ObserveSeconds:F1}s, radio {Radius:F0}u)" );
		}

		public void StopObserving()
		{
			if ( !IsObserving ) return;
			IsObserving = false;
			Log.Info( $"[EnemyLab] LootObserver window closed (count={ObservedCount})" );
		}

		protected override void OnUpdate()
		{
			if ( !IsObserving || IsProxy ) return;

			if ( _sinceStart >= ObserveSeconds )
			{
				StopObserving();
				return;
			}

			var anchor = Anchor != null && Anchor.IsValid() ? Anchor : GameObject;
			var anchorPos = anchor.WorldPosition;

			foreach ( var pickup in Scene.GetAllComponents<LootPickupContent>() )
			{
				var go = pickup.GameObject;
				if ( go == null || !go.IsValid() || _counted.Contains( go ) ) continue;

				if ( Vector3.DistanceBetween( go.WorldPosition, anchorPos ) <= Radius )
				{
					_counted.Add( go );
					ObservedCount++;
					_observedItemIds.Add( string.IsNullOrEmpty( pickup.ItemId ) ? "?" : pickup.ItemId );
					Log.Info( $"[EnemyLab] LootObserved item={pickup.ItemId ?? "?"} pos={go.WorldPosition} n={ObservedCount}" );
				}
			}
		}
	}
}
