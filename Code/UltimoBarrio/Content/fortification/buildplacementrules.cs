using Sandbox;
using System;
using System.Collections.Generic;

namespace UltimoBarrio.Content.Fortification
{
	/// <summary>Resultado de la validación de placement (server).</summary>
	public enum BuildPlacementResult
	{
		Valid,
		InvalidDefinition,
		NotHost,
		OutOfRange,
		OverlapsBuild,
		NoGround,
		BlockedVolume
	}

	/// <summary>
	/// Validación SERVER del transform de un build (posición + rotación).
	/// No toca internals: el lab la ejercita con un fixture de "jugador" (posición
	/// del builder) y el estado real de la escena (BuildStructureHost existentes +
	/// trazas del scene). Orden de checks: definición → host → rango → solapamiento
	/// → ground → volumen.
	/// </summary>
	public static class BuildPlacementRules
	{
		public const float MaxBuildRange = 250f;   // unidades desde el builder
		public const float GroundProbeHeight = 400f;
		public const float GroundTolerance = 35f;
		public const float BuildHalfHeight = 60f;  // media altura del volumen de prueba

		public static BuildPlacementResult Validate( Scene scene, BuildDefinition def, Vector3 position, Rotation rotation, Vector3 builderOrigin, IEnumerable<BuildStructureHost> existingBuilds )
		{
			if ( def == null ) return BuildPlacementResult.InvalidDefinition;
			if ( !Networking.IsHost ) return BuildPlacementResult.NotHost;

			// 1) Rango del jugador/builder.
			if ( Vector3.DistanceBetween( builderOrigin, position ) > MaxBuildRange )
			{
				return BuildPlacementResult.OutOfRange;
			}

			// 2) Solapamiento con otros builds (AABB horizontal por huella).
			if ( OverlapsExisting( position, def, existingBuilds ) )
			{
				return BuildPlacementResult.OverlapsBuild;
			}

			// 3) Ground check: el suelo debe existir bajo el build y estar cerca.
			if ( !HasGround( scene, position ) )
			{
				return BuildPlacementResult.NoGround;
			}

			// 4) Volumen de prueba: el espacio del build debe estar libre de sólidos.
			if ( VolumeBlocked( scene, position, def.Footprint ) )
			{
				return BuildPlacementResult.BlockedVolume;
			}

			return BuildPlacementResult.Valid;
		}

		private static bool OverlapsExisting( Vector3 position, BuildDefinition def, IEnumerable<BuildStructureHost> existingBuilds )
		{
			if ( existingBuilds == null ) return false;

			float thisHalf = def.Footprint.Length * 0.5f;

			foreach ( var other in existingBuilds )
			{
				if ( other == null ) continue;
				var otherGo = other.GameObject;
				if ( otherGo == null || !otherGo.IsValid || other.IsDead ) continue;

				var otherDef = other.Definition;
				float otherHalf = otherDef != null ? otherDef.Footprint.Length * 0.5f : thisHalf;

				var delta = other.WorldPosition - position;
				delta.z = 0f;

				if ( delta.Length < thisHalf + otherHalf )
				{
					return true;
				}
			}

			return false;
		}

		private static bool HasGround( Scene scene, Vector3 position )
		{
			var start = position + Vector3.Up * GroundProbeHeight;
			var end = position - Vector3.Up * GroundProbeHeight;

			var tr = scene.Trace.Ray( start, end ).Run();
			if ( !tr.Hit ) return false;
			if ( tr.Normal.Dot( Vector3.Up ) < 0.7f ) return false;

			float dz = position.z - tr.HitPosition.z;
			return dz >= -GroundTolerance && dz <= GroundTolerance;
		}

		private static bool VolumeBlocked( Scene scene, Vector3 position, Vector2 footprint )
		{
			var halfExtents = new Vector3( footprint.x * 0.5f, footprint.y * 0.5f, 20f );

			// Barrido vertical de caja (axis-aligned): desde encima del volumen hasta
			// justo por encima del suelo. El fondo de la caja nunca cruza el plano del
			// suelo, así el suelo no bloquea y solo los sólidos del volumen impactan.
			float startZ = position.z + BuildHalfHeight + 80f;
			float endZ = MathF.Max( position.z - 33f, 21f );

			var tr = scene.Trace.Box( halfExtents, new Vector3( position.x, position.y, startZ ), new Vector3( position.x, position.y, endZ ) ).Run();
			return tr.Hit;
		}
	}
}
