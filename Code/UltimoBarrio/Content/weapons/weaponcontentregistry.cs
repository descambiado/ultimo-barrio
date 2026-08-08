using System.Collections.Generic;

namespace UltimoBarrio.Content.Weapons
{
	/// <summary>
	/// Catálogo del weapon content pack (portable).
	///
	/// Estados de assets:
	///   ⚠️ VERIFIED_FALLBACK → ruta confirmada que carga en el engine (usada en prefabs hoy).
	///   ⚠️ PENDING_VERIFY    → candidato primario; comprobar con Cloud Browser
	///                           cuando el editor esté disponible y actualizar.
	///
	/// Regla: nada de este pack toca el InventoryComponent / HeldItemController viejos.
	/// El gameplay se conectará vía IWeaponContentAdapter al core nuevo.
	/// </summary>
	public static class WeaponContentRegistry
	{
		private static readonly Dictionary<string, WeaponContentDefinition> _definitions = new();

		static WeaponContentRegistry()
		{
			Register( Usp() );
			Register( Crowbar() );
			Register( Knife() );
			Register( Shotgun() );
		}

		public static WeaponContentDefinition Get( string id )
		{
			if ( string.IsNullOrEmpty( id ) ) return null;
			return _definitions.TryGetValue( id, out var def ) ? def : null;
		}

		public static IEnumerable<WeaponContentDefinition> All => _definitions.Values;

		private static void Register( WeaponContentDefinition def )
		{
			_definitions[def.Id] = def;
		}

		private static WeaponContentDefinition Usp()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_usp",
				DisplayName = "USP",
				Category = WeaponContentCategory.Firearm,

				CloudWorldId = "facepunch.w_usp", // resuelve via Cloud.Model() (paquete facepunch)
				CloudViewId = "facepunch.v_usp",
				WorldModel = "models/weapons/sbox_pistol_usp/w_usp.vmdl", // ruta local del paquete (fallback)
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_usp_content.prefab", // viewmodel con modelo real v_usp.vmdl
				AmmoModel = "models/weapons/sbox_ammo/9mm_ammobox/ammobox_9mm.vmdl", // VERIFIED (facepunch.ammobox9mm)
				CasingModel = "models/props/casings/casing_scatter_9mm_01.vmdl", // VERIFIED (facepunch.casingscatter9mm01)

				FireSound = "", // PENDING (SoundEvent real tras verificar audio; no usar nombres inventados)
				ReloadSound = "",
				DryFireSound = "",
				MuzzleEffect = "", // PENDING (.vpcf)

				AnimGraph = "models/citizen/citizen.vmdl", // animgraph embebido en el modelo (citizen.vmdl VERIFIED)
				HoldTypeParam = "holdtype",
				DrawTime = 0.5f,

				Damage = 15f,
				FireRate = 0.25f,
				MagazineSize = 12,
				ReloadTime = 1.5f,
				IsAutomatic = false,
				Pellets = 1,
				Range = 5000f,
				RecoilKick = 4f,
				MovementSpeedScale = 0.95f,
				AmmoType = "ammo_9mm",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "Verificado en editor 26.08.05 (2026-08-07): w_usp.vmdl, v_usp.vmdl (+v_usp.vanmgrph), usp_magazine(.empty), ammobox_9mm.vmdl, casing_scatter_9mm_01.vmdl. Paquetes facepunch instalados via install_package. Sonidos PENDIENTES (SoundEvents a crear; campos vacios para no inventar nombres)."
			};
		}

		private static WeaponContentDefinition Crowbar()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_crowbar",
				DisplayName = "Palanca",
				Category = WeaponContentCategory.Melee,

				// Sin cloud ident: facepunch.w_crowbar NO existe en el asset system.
				// Engine content (siempre disponible, sin montaje manual): crowbar01.vmdl.
				CloudWorldId = "",
				CloudViewId = "",
				WorldModel = "models/citizen_props/crowbar01.vmdl", // VERIFIED (engine content, 2026-08-07)
				WorldModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_crowbar_content.prefab", // crowbar01 en cámara (no hay viewmodel de palanca específico)
				AmmoModel = "",
				CasingModel = "",

				FireSound = "", // no existe SoundEvent de melee en el asset system (verificado 2026-08-07)
				ReloadSound = "",
				DryFireSound = "",
				MuzzleEffect = "",

				AnimGraph = "", // prop sin rig: no aplica animgraph
				HoldTypeParam = "holdtype",
				DrawTime = 0.4f,

				Damage = 35f,
				FireRate = 0.7f,
				MagazineSize = 0,
				ReloadTime = 0f,
				IsAutomatic = false,
				Pellets = 1,
				MeleeRange = 90f,
				Range = 90f,
				RecoilKick = 0f,
				MovementSpeedScale = 1f,
				AmmoType = "",
				EquipSlot = "Melee",

				AssetsVerified = true,
				VerificationNotes = "crowbar01.vmdl verificado en el asset system (engine content, 2026-08-07). facepunch.w_crowbar NO existe: descartado. Sin SoundEvents melee disponibles: campos de sonido vacíos."
			};
		}

		private static WeaponContentDefinition Knife()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_knife",
				DisplayName = "Cuchillo",
				Category = WeaponContentCategory.Melee,

				// Idents verificados en el backend (find_packages, 2026-08-07):
				//   facepunch.knife NO existe (descartado, era inventado).
				CloudWorldId = "facepunch.w_trenchknife", // world model Trench Knife (Facepunch)
				CloudViewId = "facepunch.v_m9bayonet",    // viewmodel melee (Facepunch)
				WorldModel = "",
				WorldModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_knife_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "", // no hay SoundEvent melee en el asset system
				ReloadSound = "",
				DryFireSound = "",
				MuzzleEffect = "",

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.3f,

				Damage = 20f,
				FireRate = 0.35f,
				MagazineSize = 0,
				ReloadTime = 0f,
				IsAutomatic = false,
				Pellets = 1,
				MeleeRange = 75f,
				Range = 75f,
				RecoilKick = 0f,
				MovementSpeedScale = 1.05f,
				AmmoType = "",
				EquipSlot = "Melee",

				AssetsVerified = true,
				VerificationNotes = "Cloud idents verificados en el backend (find_packages 2026-08-07): facepunch.w_trenchknife + facepunch.v_m9bayonet. Validado en runtime en el weapon_lab."
			};
		}

		private static WeaponContentDefinition Shotgun()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_shotgun",
				DisplayName = "Escopeta",
				Category = WeaponContentCategory.Firearm,

				// Idents verificados en el backend (find_packages, 2026-08-07):
				//   facepunch.w_shotgun NO existe (descartado, era inventado).
				//   La escopeta real de Facepunch es la Spaghelli M4 (12 gauge, benelli).
				CloudWorldId = "facepunch.w_spaghellim4", // world model Spaghelli M4
				CloudViewId = "facepunch.v_spaghellim4",  // viewmodel Spaghelli M4
				WorldModel = "",
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_shotgun_content.prefab",
				AmmoModel = "models/weapons/sbox_ammo/9mm_ammobox/ammobox_9mm.vmdl", // VERIFIED (placeholder de munición; real: facepunch.ammobox12g PENDING)
				CasingModel = "models/props/casings/casing_scatter_9mm_01.vmdl", // VERIFIED (placeholder; real: facepunch.12gshellcasing PENDING)

				FireSound = "", // sin SoundEvent de escopeta verificado
				ReloadSound = "",
				DryFireSound = "",
				MuzzleEffect = "", // real: facepunch.shotgun_muzzleflash (prefab) PENDING

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.7f,

				Damage = 12f,
				FireRate = 0.9f,
				MagazineSize = 6,
				ReloadTime = 2.5f,
				IsAutomatic = false,
				Pellets = 8,
				Range = 3500f,
				RecoilKick = 12f,
				MovementSpeedScale = 0.9f,
				AmmoType = "ammo_buckshot",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "Cloud idents verificados en el backend (find_packages 2026-08-07): facepunch.w_spaghellim4 + facepunch.v_spaghellim4 (Spaghelli M4 12g). Munición real facepunch.ammobox12g / facepunch.12g_shell / facepunch.12gshellcasing + muzzle flash facepunch.shotgun_muzzleflash PENDING de cablear (data-only). AmmoType 'ammo_buckshot' = item id del core nuevo."
			};
		}
	}
}
