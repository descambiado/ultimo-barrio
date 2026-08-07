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

				WorldModel = "facepunch.w_usp", // PENDING_VERIFY
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_usp_content.prefab",
				AmmoModel = "facepunch.ammo_9mm", // PENDING_VERIFY
				CasingModel = "",                 // PENDING_VERIFY (casquillo 9mm)

				FireSound = "weapon.usp.fire",    // PENDING_VERIFY (SoundEvent a crear)
				ReloadSound = "weapon.usp.reload",
				DryFireSound = "weapon.usp.dryfire",
				MuzzleEffect = "",                // PENDING_VERIFY (.vpcf)

				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
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

				AssetsVerified = false,
				VerificationNotes = "WorldModel 'facepunch.w_usp': confirmar ruta montada del paquete sboxweapons en Cloud Browser. Fallback metal_wheely_bin ya verificado."
			};
		}

		private static WeaponContentDefinition Crowbar()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_crowbar",
				DisplayName = "Palanca",
				Category = WeaponContentCategory.Melee,

				WorldModel = "facepunch.w_crowbar", // PENDING_VERIFY
				WorldModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_crowbar_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "weapon.crowbar.swing",
				ReloadSound = "",
				DryFireSound = "",
				MuzzleEffect = "",

				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
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

				AssetsVerified = false,
				VerificationNotes = "Buscar modelo de palanca legal (sboxweapons u oficial). Fallback crate01 verificado."
			};
		}

		private static WeaponContentDefinition Knife()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_knife",
				DisplayName = "Cuchillo",
				Category = WeaponContentCategory.Melee,

				WorldModel = "facepunch.knife", // PENDING_VERIFY
				WorldModelFallback = "models/citizen_props/crate01.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_knife_content.prefab",
				AmmoModel = "",
				CasingModel = "",

				FireSound = "weapon.knife.swing",
				ReloadSound = "",
				DryFireSound = "",
				MuzzleEffect = "",

				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
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

				AssetsVerified = false,
				VerificationNotes = "Buscar modelo de cuchillo legal. Fallback crate01 verificado."
			};
		}

		private static WeaponContentDefinition Shotgun()
		{
			return new WeaponContentDefinition
			{
				Id = "ub_weapon_shotgun",
				DisplayName = "Escopeta",
				Category = WeaponContentCategory.Firearm,

				WorldModel = "facepunch.w_shotgun", // PENDING_VERIFY
				WorldModelFallback = "models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl", // VERIFIED
				ViewModel = "prefabs/content/weapons/v_shotgun_content.prefab",
				AmmoModel = "facepunch.ammo_buckshot", // PENDING_VERIFY
				CasingModel = "",

				FireSound = "weapon.shotgun.fire",
				ReloadSound = "weapon.shotgun.reload",
				DryFireSound = "weapon.shotgun.dryfire",
				MuzzleEffect = "",

				AnimGraph = "models/citizen/citizen.animgraph", // PENDING_VERIFY
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

				AssetsVerified = false,
				VerificationNotes = "AmmoType 'ammo_buckshot' es un item id nuevo (definirlo en el core nuevo). Fallback verificado."
			};
		}
	}
}
