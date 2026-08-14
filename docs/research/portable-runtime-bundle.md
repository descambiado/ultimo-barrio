# Portable Runtime Bundle — weapon_lab suite (validada en runtime)

**Fuente:** `spike/laptop-content-stack` @ `7433f69`. Bundle mínimo validado: armas 4/4 PASS (editor 26.08.05, 2026-08-08). Daño por ruta real `Fire → trace → IDamageTarget`.

## Validated SHAs

| SHA | Contenido | Estado |
|---|---|---|
| `9839846` | Bucle USP portable validado (rig sin pawn, daño real) | ✅ validated |
| `6913adb` | Bucle melee crowbar validado (engine content) | ✅ validated |
| `9fd7b32` | Suite 4/4: USP, crowbar, knife (w_trenchknife/v_m9bayonet), shotgun (w_spaghellim4/v_spaghellim4); cloud persistida en sbproj | ✅ validated |
| `7433f69` | Estado final: idents knife/shotgun verificados en backend; asset-registry al día | ✅ validated |

Docs de soporte (no runtime): `93dcf9e` (plan), `f302efe` (estado USP).

## Required PackageReferences

En `ultimo_barrio.sbproj` → `PackageReferences` (añadidas en `9fd7b32`; montaje automático al abrir proyecto; `install_package` NO persiste):

```
facepunch.w_usp
facepunch.v_usp
facepunch.w_trenchknife
facepunch.v_m9bayonet
facepunch.w_spaghellim4
facepunch.v_spaghellim4
facepunch.ammobox12g
facepunch.12g_shell
facepunch.12gshellcasing
```

Descartados (no existen en backend): `facepunch.knife`, `facepunch.w_shotgun`, `facepunch.w_crowbar`, `facepunch.ammo_9mm` (como paquete instalable).

## Required Assets

Prefabs del pack (autocontenidos; el cloud reemplaza en runtime, el modelo del prefab es fallback real):

```
Assets/prefabs/content/weapons/w_usp_content.prefab
Assets/prefabs/content/weapons/v_usp_content.prefab
Assets/prefabs/content/weapons/w_crowbar_content.prefab
Assets/prefabs/content/weapons/v_crowbar_content.prefab
Assets/prefabs/content/weapons/w_knife_content.prefab
Assets/prefabs/content/weapons/v_knife_content.prefab
Assets/prefabs/content/weapons/w_shotgun_content.prefab
Assets/prefabs/content/weapons/v_shotgun_content.prefab
Assets/prefabs/content/dev/lab_player_official.prefab   (dev: template PlayerController del engine, literal)
```

Escena + código del bundle:

```
Assets/scenes/spikes/weapon_lab.scene
Code/UltimoBarrio/Content/idamagetarget.cs
Code/UltimoBarrio/Content/weapons/iweaponcontentadapter.cs
Code/UltimoBarrio/Content/weapons/weaponcontentdefinition.cs
Code/UltimoBarrio/Content/weapons/weaponcontentregistry.cs
Code/UltimoBarrio/Content/weapons/weaponcontenthost.cs
Code/UltimoBarrio/Content/dev/weapontestrig.cs            (dev, rig-6)
Code/UltimoBarrio/Content/dev/labdamagedummy.cs           (dev)
```

Fallbacks engine (NO se copian; existen en el asset system):
`crowbar01.vmdl`, `models/citizen_props/crate01.vmdl`, `models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl`.

Fuera del bundle (no validados en runtime): enemigos (`1d2047f`, modelo Citizen ⚠️ PENDING_VERIFY/EULA), fortificación (`8eb4253`), labs building/enemy/vehicle.

## Integration Order

1. **Contratos**: `IDamageTarget` + `ContentDamageEvent`, `IWeaponContentAdapter`.
2. **PackageReferences** (9 idents) en el sbproj del core — antes que los prefabs.
3. **Definiciones + registry** (`weaponcontentdefinition.cs`, `weaponcontentregistry.cs`) — unificar `ub_weapon_*` con el ItemRegistry del core.
4. **Prefabs** (8 armas) + fallbacks.
5. **Host** (`weaponcontenthost.cs`) — conectar equip/fire/reload a la API de armas del core.
6. **Escena + dev rig** (`weapon_lab.scene`, `weapontestrig.cs`, `labdamagedummy.cs`, `lab_player_official.prefab`) — último.
7. **Validar**: compile → Play `weapon_lab` → suite 4/4 PASS (delta ≥ ExpectedDamage).

## Adapters Required

| Adapter | Estado | Unión con el core nuevo |
|---|---|---|
| `IWeaponContentAdapter` | implementado por `WeaponContentHost` | El core consume equip/fire/reload; mapeo ident→literal cloud en `ResolveCloudModel()` |
| `IDamageTarget` (`ContentDamageEvent`) | implementado por dummy + enemigos/fortificación | Bridge de pocas líneas → contrato de daño del core |
| Registry IDs | `ub_weapon_usp/crowbar/knife/shotgun`, `AmmoType: ammo_buckshot/ammo_9mm` | Unificar con ItemRegistry del core; prefabs referencian por string |

No duplicar (ya los tiene el core nuevo): wallet, misiones, raid, inventory, housing/ownership.
