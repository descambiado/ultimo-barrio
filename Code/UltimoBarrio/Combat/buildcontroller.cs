using Sandbox;
using System;
using System.Linq;
using UltimoBarrio.Content.Fortification;

namespace UltimoBarrio.Combat
{
	/// <summary>
	/// Build Controller — conecta el sistema de fortificación (validado en lab, 9/9)
	/// al gameplay real del jugador.
	///
	/// Host-authoritative: el cliente manda la intención (raycast local) por RPC; el
	/// host valida posición con BuildPlacementRules, descuenta el coste de "chatarra"
	/// del inventario del jugador y llama a BuildStructureHost.SpawnBuild.
	///
	/// Interacción (alpha):
	///   - Tecla F (acción estándar "Flashlight", sin uso en el juego) → intenta
	///     colocar la barricada de madera apuntando al suelo frente al jugador.
	///   - Comando de consola `ub_build` como alternativa.
	///   - Sin vista previa por ahora: la barricada aparece directamente si la
	///     posición es válida y hay recursos.
	/// </summary>
	[Title( "Build Controller" )]
	[Category( "Último Barrio — Combat" )]
	[Icon( "construction" )]
	public sealed class BuildController : Component
	{
		[Property] public string BuildDefinitionId { get; set; } = "fort_barricade_wood";
		[Property] public float PlacementRange { get; set; } = 250f;

		/// <summary>
		/// Acción de input para construir. "Build" NO es una acción por defecto de s&box;
		/// usamos "Flashlight" (tecla F, estándar del engine y sin usar en el juego).
		/// También disponible el comando de consola `ub_build`.
		/// </summary>
		[Property] public string BuildInputAction { get; set; } = "Flashlight";

		[ConCmd( "ub_build" )]
		public static void CmdBuild()
		{
			var controller = Game.ActiveScene.GetAllComponents<BuildController>()
				.FirstOrDefault( c => !c.IsProxy );
			if ( controller == null )
			{
				Log.Warning( "[BuildController] No se encontró el BuildController del jugador local." );
				return;
			}
			controller.RequestBuild();
		}

		protected override void OnUpdate()
		{
			if ( IsProxy ) return;

			if ( !string.IsNullOrEmpty( BuildInputAction ) && Input.Pressed( BuildInputAction ) )
			{
				RequestBuild();
			}
		}

		private void RequestBuild()
		{
			var (origin, forward) = GetAim();
			if ( !Networking.IsHost )
			{
				RpcRequestBuild( origin, forward );
				return;
			}

			TryBuildOnHost( origin, forward );
		}

		/// <summary>Origen y dirección de mira (cámara, o desde el ojo si no hay cámara).</summary>
		private (Vector3 Origin, Vector3 Forward) GetAim()
		{
			var cam = Components.Get<CameraComponent>()
				?? Scene.GetAllComponents<CameraComponent>().FirstOrDefault( c => c.IsMainCamera );

			if ( cam != null )
			{
				var ray = cam.ScreenNormalToRay( 0.5f );
				return (ray.Position, ray.Forward);
			}

			return (WorldPosition + Vector3.Up * 64f, WorldRotation.Forward);
		}

		[Rpc.Host]
		private void RpcRequestBuild( Vector3 origin, Vector3 direction )
		{
			TryBuildOnHost( origin, direction );
		}

		private void TryBuildOnHost( Vector3 origin, Vector3 direction )
		{
			if ( !Networking.IsHost ) return;

			var def = FortificationContentRegistry.Get( BuildDefinitionId );
			if ( def == null )
			{
				Log.Error( $"[BuildController] Definición no registrada: {BuildDefinitionId}" );
				return;
			}

			// Raycast para el punto de colocación.
			var tr = Scene.Trace.Ray( origin, origin + direction * PlacementRange )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.Run();

			if ( !tr.Hit )
			{
				Log.Info( "[BuildController] Sin punto de colocación (sin impacto)." );
				return;
			}

			var position = tr.HitPosition;
			// El cuerpo del player rota solo en yaw (el pitch vive en los ojos), así que
			// WorldRotation ya es la orientación upright correcta para la barricada.
			var rotation = WorldRotation;

			// Validación server con las reglas reales del pack.
			var existing = Scene.GetAllComponents<BuildStructureHost>();
			var result = BuildPlacementRules.Validate( Scene, def, position, rotation, WorldPosition, existing );
			if ( result != BuildPlacementResult.Valid )
			{
				Log.Info( $"[BuildController] Colocación RECHAZADA: {result}" );
				return;
			}

			// Coste de construcción (chatarra) del inventario del jugador.
			var inv = Components.Get<InventoryComponent>();
			if ( inv != null )
			{
				if ( def.BuildCost > 0 && inv.GetCount( "chatarra" ) < def.BuildCost )
				{
					Log.Info( $"[BuildController] Chatarra insuficiente (necesitas {def.BuildCost})." );
					return;
				}

				if ( def.BuildCost > 0 )
				{
					inv.TryRemove( "chatarra", def.BuildCost );
				}
			}

			var host = BuildStructureHost.SpawnBuild( def, position, rotation );
			Log.Info( host != null
				? $"[BuildController] {def.DisplayName} construida en {position} (coste {def.BuildCost} chatarra)."
				: "[BuildController] Falló el spawn de la estructura." );
		}
	}
}
