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
			Register( M4A1() );
			Register( Magnum() );
			Register( Mp5() );
			Register( M700() );
			Register( Colt1911() );
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

				FireSound = "sounds/content/weapons/usp_fire.sound", // VERIFIED banco (MIT importado, glock_shoot_01.wav)
				ReloadSound = "sounds/content/weapons/usp_reload_magin.sound", // VERIFIED (pistol_mag_in.wav)
				ReloadStartSound = "sounds/content/weapons/usp_reload_magout.sound", // VERIFIED (pistol_mag_out.wav)
				DeploySound = "sounds/content/weapons/usp_deploy.sound", // VERIFIED (foley_deploy_weapon_03.wav, generic)
				DryFireSound = "sounds/content/weapons/usp_dry.sound", // VERIFIED (dry_fire.wav MIT)
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
				VerificationNotes = "Verificado en editor 26.08.05 (2026-08-07): w_usp.vmdl, v_usp.vmdl (+v_usp.vanmgrph), usp_magazine(.empty), ammobox_9mm.vmdl, casing_scatter_9mm_01.vmdl. Paquetes facepunch instalados via install_package. Sonidos: 4 SoundEvents del banco VERIFICADOS 2026-08-08 (worker C audio-v2) -- fuentes MIT importadas de Facepunch/sandbox (glock_shoot_01, pistol_mag_in/out, foley_deploy_weapon_03, dry_fire); los .sound compilan en el editor y las rutas referenciadas son assets de proyecto (sounds/content/weapons/*)."
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

				FireSound = "sounds/content/weapons/crowbar_swing.sound", // VERIFIED banco (MIT swing_01/02.wav)
				ReloadSound = "",
				ReloadStartSound = "",
				DeploySound = "",
				MeleeHitSound = "sounds/content/weapons/crowbar_impact.sound", // VERIFIED banco (MIT crowbar_hit_01..04.wav)
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
				VerificationNotes = "crowbar01.vmdl verificado en el asset system (engine content, 2026-08-07). facepunch.w_crowbar NO existe como paquete instalable: descartado como primary. Sonidos: crowbar_swing + crowbar_impact VERIFICADOS 2026-08-08 (worker C audio-v2) -- fuentes MIT importadas de Facepunch/sandbox (swing_01/02.wav, crowbar_hit_01..04.wav)."
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
				// CloudViewId corregido 2026-08-15: estaba mezclado con v_m9bayonet
				// (arma DISTINTA de trenchknife dentro de la colección); ahora usa
				// el viewmodel real del Trench Knife, coherente con el world model.
				CloudWorldId = "facepunch.w_trenchknife", // world model Trench Knife (Facepunch)
				CloudViewId = "facepunch.v_trenchknife",  // viewmodel Trench Knife (Facepunch)
				WorldModel = "",
				WorldModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_knife_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "sounds/content/weapons/knife_swing.sound", // VERIFIED banco (MIT swing_01/02.wav, compartido con crowbar)
				ReloadSound = "",
				ReloadStartSound = "",
				DeploySound = "",
				MeleeHitSound = "sounds/content/impacts/melee_impact_flesh.sound", // VERIFIED banco (engine core BluntWeapon/flesh-1..4)
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
				VerificationNotes = "Cloud idents verificados en el backend (find_packages 2026-08-07): facepunch.w_trenchknife + facepunch.v_m9bayonet. Validado en runtime en el weapon_lab. Sonidos: knife_swing VERIFICADO 2026-08-08 (worker C audio-v2) -- melee swing MIT importado (swing_01/02.wav, compartido con crowbar; mismo fit acústico, pendiente de OK de diseño para un stab propio del paquete del cuchillo)."
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

				FireSound = "sounds/content/weapons/shotgun_fire.sound", // VERIFIED banco (MIT shotgun1_shoot1/2.wav)
				ReloadSound = "sounds/content/weapons/shotgun_reload.sound", // VERIFIED (shotgun_load.wav)
				ReloadStartSound = "sounds/content/weapons/shotgun_reload_start.sound", // VERIFIED (shotgun_cock.wav)
				DeploySound = "sounds/content/weapons/usp_deploy.sound", // VERIFIED (foley_deploy_weapon_03.wav, generic)
				DryFireSound = "sounds/content/weapons/usp_dry.sound", // VERIFIED (dry_fire.wav MIT, generic)
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
				VerificationNotes = "Cloud idents verificados en el backend (find_packages 2026-08-07): facepunch.w_spaghellim4 + facepunch.v_spaghellim4 (Spaghelli M4 12g). Munición real facepunch.ammobox12g / facepunch.12g_shell / facepunch.12gshellcasing + muzzle flash facepunch.shotgun_muzzleflash PENDING de cablear (data-only). AmmoType 'ammo_buckshot' = item id del core nuevo. Sonidos: 4 SoundEvents VERIFICADOS 2026-08-08 (worker C audio-v2) -- fuentes MIT importadas de Facepunch/sandbox (shotgun1_shoot1/2, shotgun_cock, shotgun_load, foley_deploy_weapon_03, dry_fire)."
			};
		}

		private static WeaponContentDefinition M4A1()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_m4a1",
				DisplayName = "M4A1",
				Category = WeaponContentCategory.Firearm,

				CloudWorldId = "facepunch.w_m4a1",
				CloudViewId = "facepunch.v_m4a1",
				WorldModel = "models/weapons/sbox_assault_m4a1/w_m4a1.vmdl", // VERIFIED (install_package 2026-08-15)
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_m4a1_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "sounds/content/weapons/usp_fire.sound", // PENDING sonido propio; reutiliza banco verificado
				ReloadSound = "sounds/content/weapons/usp_reload_magin.sound",
				ReloadStartSound = "sounds/content/weapons/usp_reload_magout.sound",
				DeploySound = "sounds/content/weapons/usp_deploy.sound",
				DryFireSound = "sounds/content/weapons/usp_dry.sound",
				MuzzleEffect = "",

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.6f,

				Damage = 18f,
				FireRate = 0.1f,
				MagazineSize = 30,
				ReloadTime = 2f,
				IsAutomatic = true,
				Pellets = 1,
				Range = 5000f,
				RecoilKick = 5f,
				MovementSpeedScale = 0.9f,
				AmmoType = "ammo_9mm",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "facepunch.w_m4a1 + facepunch.v_m4a1 instalados via install_package 2026-08-15. Sonidos y efecto de boca PENDING (reutiliza banco de la USP como placeholder verificado, no inventado)."
			};
		}

		private static WeaponContentDefinition Magnum()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_magnum",
				DisplayName = "Revólver Magnum",
				Category = WeaponContentCategory.Firearm,

				// facepunch.wmagnum es el único paquete oficial: solo world model,
				// sin viewmodel dedicado. Se reutiliza el mismo modelo para ambos.
				CloudWorldId = "facepunch.wmagnum",
				CloudViewId = "",
				WorldModel = "models/weapons/sbox_revolver_magnum/w_magnum.vmdl", // VERIFIED (install_package 2026-08-15)
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_magnum_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "sounds/content/weapons/usp_fire.sound", // PENDING sonido propio; reutiliza banco verificado
				ReloadSound = "sounds/content/weapons/usp_reload_magin.sound",
				ReloadStartSound = "sounds/content/weapons/usp_reload_magout.sound",
				DeploySound = "sounds/content/weapons/usp_deploy.sound",
				DryFireSound = "sounds/content/weapons/usp_dry.sound",
				MuzzleEffect = "",

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.6f,

				Damage = 45f,
				FireRate = 0.6f,
				MagazineSize = 6,
				ReloadTime = 2.5f,
				IsAutomatic = false,
				Pellets = 1,
				Range = 5000f,
				RecoilKick = 10f,
				MovementSpeedScale = 0.92f,
				AmmoType = "ammo_9mm",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "facepunch.wmagnum instalado via install_package 2026-08-15. Sin viewmodel oficial de Facepunch para el Magnum: el world model se reutiliza también como viewmodel. Sonidos PENDING (reutiliza banco de la USP como placeholder verificado)."
			};
		}

		private static WeaponContentDefinition Mp5()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_mp5",
				DisplayName = "MP5",
				Category = WeaponContentCategory.Firearm,

				CloudWorldId = "facepunch.w_mp5",
				CloudViewId = "facepunch.v_mp5",
				WorldModel = "models/weapons/sbox_smg_mp5/w_mp5.vmdl", // VERIFIED (install_package 2026-08-15)
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_mp5_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "sounds/content/weapons/usp_fire.sound", // PENDING sonido propio; reutiliza banco verificado
				ReloadSound = "sounds/content/weapons/usp_reload_magin.sound",
				ReloadStartSound = "sounds/content/weapons/usp_reload_magout.sound",
				DeploySound = "sounds/content/weapons/usp_deploy.sound",
				DryFireSound = "sounds/content/weapons/usp_dry.sound",
				MuzzleEffect = "",

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.5f,

				Damage = 12f,
				FireRate = 0.08f,
				MagazineSize = 25,
				ReloadTime = 1.8f,
				IsAutomatic = true,
				Pellets = 1,
				Range = 4000f,
				RecoilKick = 3f,
				MovementSpeedScale = 0.95f,
				AmmoType = "ammo_9mm",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "facepunch.w_mp5 + facepunch.v_mp5 instalados via install_package 2026-08-15. Sonidos y efecto de boca PENDING (reutiliza banco de la USP como placeholder verificado, no inventado)."
			};
		}

		private static WeaponContentDefinition M700()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_m700",
				DisplayName = "Rifle M700",
				Category = WeaponContentCategory.Firearm,

				CloudWorldId = "facepunch.w_m700",
				CloudViewId = "facepunch.v_m700",
				WorldModel = "models/weapons/sbox_sniper_m700/w_m700.vmdl", // VERIFIED (install_package 2026-08-15)
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_m700_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "sounds/content/weapons/usp_fire.sound", // PENDING sonido propio; reutiliza banco verificado
				ReloadSound = "sounds/content/weapons/usp_reload_magin.sound",
				ReloadStartSound = "sounds/content/weapons/usp_reload_magout.sound",
				DeploySound = "sounds/content/weapons/usp_deploy.sound",
				DryFireSound = "sounds/content/weapons/usp_dry.sound",
				MuzzleEffect = "",

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.8f,

				Damage = 80f,
				FireRate = 1.2f,
				MagazineSize = 5,
				ReloadTime = 3f,
				IsAutomatic = false,
				Pellets = 1,
				Range = 8000f,
				RecoilKick = 18f,
				MovementSpeedScale = 0.85f,
				AmmoType = "ammo_9mm",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "facepunch.w_m700 + facepunch.v_m700 instalados via install_package 2026-08-15 (rifle de cerrojo, francotirador de la colección oficial). Sonidos y efecto de boca PENDING (reutiliza banco de la USP como placeholder verificado, no inventado). Usa CitizenAnimationHelper.HoldTypes.Rifle: no existe holdtype dedicado 'Sniper' en el motor."
			};
		}

		private static WeaponContentDefinition Colt1911()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_1911",
				DisplayName = "Pistola 1911",
				Category = WeaponContentCategory.Firearm,

				// facepunch.w_1911 es el único paquete oficial: solo world model,
				// igual que el Magnum, sin viewmodel dedicado en la colección.
				CloudWorldId = "facepunch.w_1911",
				CloudViewId = "",
				WorldModel = "models/weapons/sbox_pistol_1911/w_1911.vmdl", // VERIFIED (install_package 2026-08-15)
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_1911_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "sounds/content/weapons/usp_fire.sound", // PENDING sonido propio; reutiliza banco verificado
				ReloadSound = "sounds/content/weapons/usp_reload_magin.sound",
				ReloadStartSound = "sounds/content/weapons/usp_reload_magout.sound",
				DeploySound = "sounds/content/weapons/usp_deploy.sound",
				DryFireSound = "sounds/content/weapons/usp_dry.sound",
				MuzzleEffect = "",

				AnimGraph = "",
				HoldTypeParam = "holdtype",
				DrawTime = 0.5f,

				Damage = 20f,
				FireRate = 0.3f,
				MagazineSize = 7,
				ReloadTime = 1.6f,
				IsAutomatic = false,
				Pellets = 1,
				Range = 5000f,
				RecoilKick = 5f,
				MovementSpeedScale = 0.95f,
				AmmoType = "ammo_9mm",
				EquipSlot = "Primary",

				AssetsVerified = true,
				VerificationNotes = "facepunch.w_1911 instalado via install_package 2026-08-15. Sin viewmodel oficial de Facepunch para el 1911 (igual que el Magnum): el world model se reutiliza también como viewmodel. Sonidos PENDING (reutiliza banco de la USP como placeholder verificado)."
			};
		}
	}
}
