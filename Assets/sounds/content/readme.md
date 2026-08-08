# Banco de sonidos — Último Barrio (content pack)

`Assets/sounds/content/**` — SoundEvents propios del proyecto (`sounds/content/...`).

**Rama:** `agent/audio` · **HEAD base validado:** `9fd7b32` · **Fecha:** 2026-08-08
**Worker:** F — Audio. **Método:** solo rutas reales verificadas (código oficial de Facepunch + disco del engine + repo oficial MIT). **Prohibido inventar rutas** — lo no verificable está en `PENDING.md`.

## Reglas de uso

- Los `.sound` son wrappers JSON en el formato oficial del editor (verificado contra `facepunch/sandbox` y los eventos compilados del engine).
- `Sounds[]` referencia archivos `.vsnd` reales. El motor resuelve `.vsnd` → `.vsnd_c` compilado (convención oficial; verificado en el formato interno de `impact-melee-wood.sound_c` del core).
- Los `.vsnd` de armas viven en los paquetes de la colección `facepunch.sboxweapons` (montados vía `PackageReferences` del sbproj) y en el contenido base del engine. Los `.vsnd` de impactos viven en el core del engine (`core/sounds/`), disponibles en cualquier proyecto s&box.
- Reproducción: `Sound.Play( "sounds/content/weapons/usp_fire.sound", WorldPosition )`. `Sound.Precache` antes de uso intensivo.
- No copiamos binarios salvo `dry_fire.wav` (único hueco sin referencia montada; importado del repo oficial MIT de Facepunch con aviso en `THIRD_PARTY_NOTICES.md`).

## Tabla maestra — source / license / ident / SoundEvent / usage

| # | Evento del juego | SoundEvent (path en juego) | Fuente del sonido | Archivo(s) reales referenciados | License / autoría | Evidencia |
|---|---|---|---|---|---|---|
| 1 | USP fire | `sounds/content/weapons/usp_fire.sound` | `facepunch.sboxweapons` (colección oficial) | `weapons/glock/glock_shoot_01.vsnd` | Facepunch oficial, free-to-use en proyectos s&box | El gamemode oficial usa este sonido para USP/glock/1911 — `Facepunch/sandbox` `Assets/weapons/Glock/glock_shoot.sound`; commit `1b77a36f` ("USP shoot sound was too quiet") edita ese mismo evento |
| 2 | USP reload (extraer cargador) | `sounds/content/weapons/usp_reload_magout.sound` | `facepunch.sboxweapons` | `weapons/glock/reload/pistol_mag_out.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Glock/reload/pistol_mag_out.sound` |
| 3 | USP reload (insertar cargador) | `sounds/content/weapons/usp_reload_magin.sound` | `facepunch.sboxweapons` | `weapons/glock/reload/pistol_mag_in.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Glock/reload/pistol_mag_in.sound` |
| 4 | USP deploy | `sounds/content/weapons/usp_deploy.sound` | `facepunch.sboxweapons` | `weapons/common/foley/foley_deploy_weapon_03.vsnd` | Facepunch oficial | Usado por `colt1911_vm.prefab` y `glock_vm.prefab` del gamemode oficial |
| 5 | USP dry fire | `sounds/content/weapons/usp_dry.sound` | Repo oficial `Facepunch/sandbox` (MIT), importado: `Assets/sounds/content/weapons/dry_fire.wav` → `sounds/content/weapons/dry_fire.vsnd` (compilado por el editor) | `sounds/content/weapons/dry_fire.vsnd` (proyecto) | MIT © 2026 Facepunch — atribución en `THIRD_PARTY_NOTICES.md` | `Facepunch/sandbox` `Assets/sounds/dry_fire.sound` + `Assets/sounds/dry_fire.wav` (36 KB) |
| 6 | Shotgun fire | `sounds/content/weapons/shotgun_fire.sound` | `facepunch.sboxweapons` | `weapons/shotgun/sounds/shotgun1_shoot1.vsnd`, `shotgun1_shoot2.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Shotgun/sounds/shotgun_shoot.sound` (Vol 0.3, la escopeta del proyecto es la Spaghelli M4 montada vía `w_spaghellim4`/`v_spaghellim4`) |
| 7 | Shotgun reload (inicio: cock) | `sounds/content/weapons/shotgun_reload_start.sound` | `facepunch.sboxweapons` | `weapons/shotgun/sounds/shotgun_cock.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Shotgun/sounds/shotgun_cock.sound` |
| 8 | Shotgun reload (carga) | `sounds/content/weapons/shotgun_reload.sound` | `facepunch.sboxweapons` | `weapons/shotgun/sounds/shotgun_load.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Shotgun/sounds/shotgun_load.sound` (Vol 0.5, pitch 0.9–1.1) |
| 9 | Crowbar swing | `sounds/content/weapons/crowbar_swing.sound` | `facepunch.sboxweapons` | `weapons/crowbar/sounds/swing_01.vsnd`, `swing_02.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Crowbar/sounds/crowbar.swing.sound` + `crowbar.prefab` (`AttackSound`) |
| 10 | Crowbar impact | `sounds/content/weapons/crowbar_impact.sound` | `facepunch.sboxweapons` | `weapons/crowbar/sounds/crowbar_hit_01..04.vsnd` | Facepunch oficial | `Facepunch/sandbox` `Assets/weapons/Crowbar/sounds/crowbar.hit.sound` + `crowbar.prefab` (`HitSound`) |
| 11 | Knife impact / ataque melee enemigo (flesh) | `sounds/content/impacts/melee_impact_flesh.sound` | Engine core (`core/sounds`) | `sounds/Impacts/Melee/BluntWeapon/flesh-1..4.vsnd` | Contenido del engine s&box (referencia, no redistribuido) | Archivos verificados en disco: `core/sounds/Impacts/Melee/BluntWeapon/flesh-1.wav/.vsnd_c`; el propio evento del engine `impact-melee-flesh.sound_c` los referencia igual |
| 12 | Enemy hurt (bala) | `sounds/content/impacts/bullet_impact_flesh.sound` | Engine core | `sounds/Impacts/Bullets/flesh_bullet_impact-1..4.vsnd` | Contenido del engine s&box (referencia) | Verificado en disco: `core/sounds/Impacts/Bullets/flesh_bullet_impact-*.wav/.vsnd_c`; evento del engine `impact-bullet-flesh.sound_c` |
| 13 | Door impact | `sounds/content/fortification/door_impact.sound` | Engine core | `sounds/Impacts/Buildings/wood_gib-1..4.vsnd` | Contenido del engine s&box (referencia) | Verificado en disco: `core/sounds/Impacts/Buildings/wood_gib-*.wav/.vsnd_c` (impacto de escombros de madera; puerta de madera del barrio). Alternativa metal: usar `barricade_impact` |
| 14 | Barricade impact | `sounds/content/fortification/barricade_impact.sound` | Engine core | `sounds/Impacts/Buildings/metal_gib-1..4.vsnd` + `wood_gib-1..4.vsnd` | Contenido del engine s&box (referencia) | Verificado en disco (metal y madera, mix random) |
| 15 | Repair (herramienta) | `sounds/content/fortification/repair.sound` | Engine core | `sounds/Physics/phys-impact-tool-1..4.vsnd` | Contenido del engine s&box (referencia) | Verificado en disco: `core/sounds/Physics/phys-impact-tool-*.wav/.vsnd_c` |
| 16 | Enemy hurt (thud genérico) | `sounds/content/enemies/enemy_hurt.sound` | Engine core | `sounds/Physics/phys-impact-meat-1..4.vsnd` | Contenido del engine s&box (referencia) | Verificado en disco: `core/sounds/Physics/phys-impact-meat-*.wav/.vsnd_c` |
| 17 | Enemy death (ragdoll) | `sounds/content/enemies/enemy_death.sound` | Engine core | `sounds/Physics/phys-impact-bone-001..004.vsnd` + `phys-impact-meat-1..4.vsnd` | Contenido del engine s&box (referencia) | Verificado en disco |

## Fuentes de sonido (fuera del banco, disponibles por montaje)

| Path (verificado) | Qué es | Dónde vive |
|---|---|---|
| `sounds/Impacts/Melee/impact-melee-{cloth,concrete,dirt,flesh,glass,grass,metal,sand,snow,wood}.sound` | Eventos de impacto melee del engine por superficie (reproducción directa) | `core/sounds/Impacts/Melee/` (disco, `.sound_c`) |
| `sounds/Impacts/Bullets/impact-bullet-{cloth,concrete,dirt,flesh,foliage,generic,glass,metal,plaster,plastic,sand,snow,water,wood}.sound` | Eventos de impacto de bala del engine por superficie | `core/sounds/Impacts/Bullets/` (disco) |
| `sounds/footsteps/footstep-{concrete,wood,metal,grass,dirt,sand,snow,gravel,stones,glass,cloth,forest}.sound` | Pasos por superficie (motor; los enemigos/player los disparan por `Surface`) | `core/sounds/footsteps/` (disco) |
| `sounds/effects/explosion/explosion_small.sound`, `sounds/effects/fire/fire_burn_loop01.sound`, `sounds/water/water_splash_medium.sound`, `sounds/water/water_bullet_impact.sound` | Explosión / fuego / agua | `core/sounds/` (disco) |
| `weapons/shotgun/sounds/shotgun_shoot.sound` (evento completo del paquete) | Alternativa oficial directa al wrapper #6 | Paquete montado `facepunch.sboxweapons` |
| `weapons/crowbar/sounds/crowbar.swing.sound` / `crowbar.hit.sound` | Eventos completos oficiales del paquete (alternativa a los wrappers #9/#10) | Paquete montado `facepunch.sboxweapons` |

## Convención de paths

- Paths de paquete (`weapons/...`) → resuelven porque los paquetes de la colección `facepunch.sboxweapons` están en `PackageReferences` del sbproj (mismo mecanismo que los modelos `models/weapons/sbox_pistol_usp/w_usp.vmdl`).
- Paths de engine (`sounds/Impacts/...`, `sounds/Physics/...`) → contenido base de s&box, disponible en todo proyecto (verificado en disco en `core/sounds/`).
- Paths de proyecto (`sounds/content/...`) → assets propios de `Assets/sounds/content/`.

## Verificación pendiente en editor (Cloud Browser)

1. Confirmar que cada `.sound` compila y resuelve (abrir proyecto → asset system → `asset_search`).
2. Confirmar `weapons/glock/glock_shoot_01.vsnd` etc. visibles en el asset browser con los paquetes montados (lista de `PackageReferences` actual).
3. `dry_fire.wav` → el editor compila `sounds/content/weapons/dry_fire.vsnd_c` automáticamente.
4. Ver `PENDING.md` para los huecos sin fuente verificada (knife swing, voces de enemigo, night warning).
