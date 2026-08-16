using Sandbox;

namespace UltimoBarrio.Content
{
	/// <summary>
	/// Efectos de combate apoyados en el sistema Surface NATIVO del motor.
	///
	/// El motor ya publica, por material, todo lo que hacía falta y que hasta ahora
	/// no usábamos (de ahí que disparar no mostrara nada):
	///   surfaces/&lt;material&gt;.surface
	///     PrefabCollection.BulletImpact       → partícula de impacto
	///     PrefabCollection.BulletImpactDecal  → marca persistente
	///     SoundCollection.Bullet              → sonido del impacto
	/// y en concreto surfaces/flesh.surface apunta a prefabs/surface/flesh_bullet.prefab,
	/// es decir la SANGRE ya viene resuelta por el motor al disparar a carne.
	///
	/// Usar tr.Surface en vez de un switch por material nuestro significa que
	/// cualquier material del mapa (ladrillo, cristal, metal, agua, hierba...) da el
	/// impacto correcto sin tocar código, y que los packs de la colección oficial
	/// (facepunch.impactX) encajan solos.
	/// </summary>
	public static class CombatEffects
	{
		/// <summary>Traza de diagnóstico (ub_qa_fx_debug) para verificar la resolución por material.</summary>
		public static bool DebugLog { get; set; }


		/// <summary>
		/// Impacto de bala completo (partícula + decal + sonido) resuelto por el
		/// material impactado. Cliente: llamar dentro de una Rpc.Broadcast.
		///
		/// Recibe el Surface ya resuelto (y no el SceneTraceResult) porque el trace
		/// ocurre en quien dispara y el efecto se reproduce en todos los clientes:
		/// por la RPC solo viaja la ruta del material, que cada cliente vuelve a
		/// resolver contra su propia ResourceLibrary.
		/// </summary>
		public static void SpawnBulletImpact( Surface surface, Vector3 position, Vector3 normal )
		{
			if ( surface is null ) return;

			// Orientación estándar de impacto: mirando hacia fuera de la superficie.
			var transform = new Transform( position, Rotation.LookAt( normal ) );

			// PrefabCollection/SoundCollection son structs del motor (no admiten ?.).
			// Ojo: estos GameObject son referencias a PREFAB (no están en la escena),
			// así que IsValid() no sirve como guarda — hay que comprobar null.
			var impact = surface.PrefabCollection.BulletImpact;
			if ( impact is not null )
			{
				impact.Clone( transform );
			}

			var decal = surface.PrefabCollection.BulletImpactDecal;
			if ( decal is not null )
			{
				decal.Clone( transform );
			}

			if ( DebugLog )
			{
				Log.Info( $"[CombatEffects] surface='{surface.ResourceName}' impact={( impact is null ? "null" : "ok" )} decal={( decal is null ? "null" : "ok" )} sound={( surface.SoundCollection.Bullet is null ? "null" : "ok" )} pos={position}" );
			}

			var sound = surface.SoundCollection.Bullet;
			if ( sound is not null )
			{
				Sound.Play( sound, position );
			}
		}

		/// <summary>
		/// Dispara la animación de ataque en tercera persona del portador.
		///
		/// El animgraph del citizen expone "b_attack"/"b_reload"; CitizenAnimationHelper
		/// NO tiene trigger para esto (solo TriggerJump/TriggerDeploy), así que hay que
		/// escribir el parámetro directamente sobre el SkinnedModelRenderer del cuerpo,
		/// que es lo que expone PlayerController.Renderer.
		/// (Patrón tomado de sousou63/DarkRP, MIT — ver THIRD_PARTY_NOTICES.)
		/// </summary>
		public static void PlayAttackAnimation( GameObject weapon )
		{
			SetBodyParam( weapon, "b_attack" );
		}

		/// <summary>Animación de recarga en tercera persona del portador.</summary>
		public static void PlayReloadAnimation( GameObject weapon )
		{
			SetBodyParam( weapon, "b_reload" );
		}

		private static void SetBodyParam( GameObject weapon, string param )
		{
			if ( !weapon.IsValid() ) return;

			var controller = weapon.Root.Components.Get<PlayerController>();
			var renderer = controller?.Renderer;
			if ( !renderer.IsValid() ) return;

			renderer.Set( param, true );
		}

		/// <summary>
		/// Fogonazo en la boca del arma. Usa el attachment "muzzle" del modelo real
		/// si existe (los modelos de facepunch.sboxweapons lo traen); si no, cae al
		/// origen del renderer para no quedarse sin efecto.
		/// </summary>
		public static void SpawnMuzzleFlash( GameObject weapon, string muzzlePrefabPath )
		{
			if ( !weapon.IsValid() || string.IsNullOrEmpty( muzzlePrefabPath ) ) return;

			var prefab = ResourceLibrary.Get<PrefabFile>( muzzlePrefabPath );
			if ( prefab is null ) return;

			var transform = weapon.WorldTransform;

			var renderer = weapon.Components.GetInDescendantsOrSelf<SkinnedModelRenderer>();
			if ( renderer.IsValid() )
			{
				var attachment = renderer.GetAttachment( "muzzle" );
				if ( attachment.HasValue )
				{
					transform = attachment.Value;
				}
			}

			var flash = SceneUtility.GetPrefabScene( prefab ).Clone( transform );

			// El fogonazo acompaña al arma mientras dura (evita que se quede flotando
			// si el jugador gira en el mismo frame).
			flash.SetParent( weapon, true );
		}
	}
}
