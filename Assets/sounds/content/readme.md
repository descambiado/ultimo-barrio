# Banco de sonidos — Último Barrio (content pack)

`Assets/sounds/content/**` — SoundEvents propios del proyecto (`sounds/content/...`).

**Rama:** `agent/audio-v2` — **HEAD base validado:** `46b3b01` — **Fecha:** 2026-08-08
**Workers:** F (banco inicial, 15 eventos) + C audio-v2 (validación editor, fix de rutas, 3 eventos nuevos).
**Método:** solo rutas reales verificadas (código oficial de Facepunch + disco del engine + repo oficial MIT).
**Prohibido inventar rutas** — lo no verificable está en `PENDING.md`.

## Reglas de uso

- Los `.sound` son wrappers JSON en el formato oficial del editor (verificado contra `facepunch/sandbox`,
  el addon `menu` del engine y los eventos compilados del core). **Verificado en editor 26.08.05: los 17
  `.sound` compilan (IsCompileFailed=false, IsCompiledAndUpToDate=true).**
- `Sounds[]` referencia archivos `.vsnd`. El motor resuelve `.vsnd` → `.vsnd_c` compilado (convención oficial).
  - **Armas y voces:** `.vsnd` de PROYECTO (`sounds/content/...`) compilados por el editor desde los `.wav`
    importados (MIT, byte-exactos del repo Facepunch/sandbox). Antes apuntaban a `weapons/...` (rutas del
    paquete sboxweapons) que **NO resuelven**: la colección `facepunch.sboxweapons` es solo modelos
    (verificado 2026-08-08 con get_package + asset_search → 0 vsnd en los paquetes montados).
  - **Impactos:** `.vsnd` del core del engine (`core/sounds/`), disponibles en cualquier proyecto s&box
    (verificados en el asset system 2026-08-08).
- Reproducción: `Sound.Play( "sounds/content/weapons/usp_fire.sound", WorldPosition )` (path completo con
  `.sound`, convención confirmada en el código oficial: `Sound.Play("sounds/flatline.sound")`).
  `Sound.Precache` antes de uso intensivo.
- No copiamos binarios salvo los importados MIT (lista en `THIRD_PARTY_NOTICES.md`).

## Tabla maestra — source / license / ident / SoundEvent / usage

Estado por sonido (2026-08-08, worker C): los 17 eventos compilan en el editor; rutas de `Sounds[]`
verificadas una a una contra el asset system. "LOADS" = evento compilado + refs resueltas.
PLAYS = validación runtime (coordinador, tras merge a spike).

| # | Evento del juego | SoundEvent (path en juego) | Fuente del sonido | Archivo(s) reales referenciados | License / autoría | Estado |
|---|---|---|---|---|---|---|
| 1 | USP fire | `sounds/content/weapons/usp_fire.sound` | Repo oficial `Facepunch/sandbox` (MIT), importado | `sounds/content/weapons/glock_shoot_01.vsnd` | MIT © Facepunch — atribución en THIRD_PARTY_NOTICES | VALID / LOADS (ref proyecto) |
| 2 | USP reload (extraer cargador) | `sounds/content/weapons/usp_reload_magout.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/pistol_mag_out.vsnd` | MIT © Facepunch | VALID / LOADS |
| 3 | USP reload (insertar cargador) | `sounds/content/weapons/usp_reload_magin.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/pistol_mag_in.vsnd` | MIT © Facepunch | VALID / LOADS |
| 4 | USP deploy | `sounds/content/weapons/usp_deploy.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/foley_deploy_weapon_03.vsnd` | MIT © Facepunch | VALID / LOADS |
| 5 | USP dry fire | `sounds/content/weapons/usp_dry.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/dry_fire.vsnd` | MIT © Facepunch | VALID / LOADS (verificado en asset system: dry_fire.vsnd resuelve) |
| 6 | Shotgun fire | `sounds/content/weapons/shotgun_fire.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/shotgun1_shoot1.vsnd`, `shotgun1_shoot2.vsnd` | MIT © Facepunch | VALID / LOADS |
| 7 | Shotgun reload (inicio: cock) | `sounds/content/weapons/shotgun_reload_start.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/shotgun_cock.vsnd` | MIT © Facepunch | VALID / LOADS |
| 8 | Shotgun reload (carga) | `sounds/content/weapons/shotgun_reload.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/shotgun_load.vsnd` | MIT © Facepunch | VALID / LOADS |
| 9 | Crowbar swing | `sounds/content/weapons/crowbar_swing.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/swing_01.vsnd`, `swing_02.vsnd` | MIT © Facepunch | VALID / LOADS |
| 10 | Crowbar impact | `sounds/content/weapons/crowbar_impact.sound` | Repo oficial (MIT), importado | `sounds/content/weapons/crowbar_hit_01..04.vsnd` | MIT © Facepunch | VALID / LOADS |
| 11 | Knife impact / ataque melee enemigo (flesh) | `sounds/content/impacts/melee_impact_flesh.sound` | Engine core (`core/sounds`) | `sounds/Impacts/Melee/BluntWeapon/flesh-1..4.vsnd` | Contenido del engine s&box (referencia, no redistribuido) | VALID / LOADS (refs en asset system) |
| 12 | Enemy hurt (bala) | `sounds/content/impacts/bullet_impact_flesh.sound` | Engine core | `sounds/Impacts/Bullets/flesh_bullet_impact-1..4.vsnd` | Contenido del engine s&box (referencia) | VALID / LOADS |
| 13 | Door impact | `sounds/content/fortification/door_impact.sound` | Engine core | `sounds/Impacts/Buildings/wood_gib-1..4.vsnd` | Contenido del engine s&box (referencia) | VALID / LOADS |
| 14 | Barricade impact | `sounds/content/fortification/barricade_impact.sound` | Engine core | `sounds/Impacts/Buildings/metal_gib-1..4.vsnd` + `wood_gib-1..4.vsnd` | Contenido del engine s&box (referencia) | VALID / LOADS |
| 15 | Repair (herramienta) | `sounds/content/fortification/repair.sound` | Engine core | `sounds/Physics/phys-impact-tool-1..4.vsnd` | Contenido del engine s&box (referencia) | VALID / LOADS |
| 16 | Enemy hurt (thud genérico) | `sounds/content/enemies/enemy_hurt.sound` | Engine core | `sounds/Physics/phys-impact-meat-1..4.vsnd` | Contenido del engine s&box (referencia) | VALID / LOADS |
| 17 | Enemy death (ragdoll) | `sounds/content/enemies/enemy_death.sound` | Engine core | `sounds/Physics/phys-impact-bone-001..004.vsnd` + `phys-impact-meat-1..4.vsnd` | Contenido del engine s&box (referencia) | VALID / LOADS |
| 18 | Knife swing (NUEVO worker C) | `sounds/content/weapons/knife_swing.sound` | Repo oficial (MIT), importado (comparte swing_01/02 con crowbar) | `sounds/content/weapons/swing_01.vsnd`, `swing_02.vsnd` | MIT © Facepunch | VALID / LOADS — fit acústico melee; OK de diseño pendiente para stab propio |
| 19 | Enemy alert (NUEVO worker C) | `sounds/content/enemies/enemy_alert.sound` | Repo oficial (MIT), darren scared (candidato documentado en PENDING.md) | `sounds/content/enemies/scared_01..06.vsnd` | MIT © Facepunch | VALID / LOADS — voz robótica; OK de diseño pendiente |
| 20 | Enemy attack (NUEVO worker C) | `sounds/content/enemies/enemy_attack.sound` | Repo oficial (MIT), darren scared | `sounds/content/enemies/scared_01..06.vsnd` | MIT © Facepunch | VALID / LOADS — voz robótica; OK de diseño pendiente |

## Fuentes de sonido (fuera del banco, disponibles por montaje o por el core)

| Path (verificado) | Qué es | Dónde vive |
|---|---|---|
| `sounds/Impacts/Melee/impact-melee-{cloth,concrete,dirt,flesh,glass,grass,metal,sand,snow,wood}.sound` | Eventos de impacto melee del engine por superficie (reproducción directa) | `core/sounds/Impacts/Melee/` (disco + asset system) |
| `sounds/Impacts/Bullets/impact-bullet-{...}.sound` | Eventos de impacto de bala del engine por superficie | `core/sounds/Impacts/Bullets/` (disco) |
| `sounds/footsteps/footstep-{...}.sound` | Pasos por superficie (los enemigos/player los disparan por `Surface`) | `core/sounds/footsteps/` (disco) |
| `sounds/effects/explosion/explosion_small.sound`, `sounds/effects/fire/fire_burn_loop01.sound`, `sounds/water/*` | Explosión / fuego / agua | `core/sounds/` (disco) |
| `sounds/ambience/electric-buzz-loop.vsnd`, `electric-buzz-2-loop.vsnd` | Único candidato del core tipo "alarma" (loop de zumbido); NO es sirena | `core/sounds/ambience/` (disco) |

## Convención de paths

- Paths de proyecto (`sounds/content/...`) → assets propios de `Assets/sounds/content/` (wavs importadas
  MIT + `.sound` wrappers). **Única dependencia de audio del pack.**
- Paths de engine (`sounds/Impacts/...`, `sounds/Physics/...`) → contenido base de s&box, disponible en
  todo proyecto (verificado en disco y en el asset system).
- ~~Paths de paquete (`weapons/...`)~~ → **descartados 2026-08-08**: la colección `facepunch.sboxweapons`
  no incluye sonidos (solo modelos); las rutas `weapons/glock/...` venían del gamemode sandbox y no
  resuelven en el asset system.

## Verificación en editor (MCP read-only, 2026-08-08, worker C)

1. `asset_search type=sound query=content projectOnly=true` → 17 eventos registrados. ✅
2. `asset_info` en `sounds/content/weapons/usp_fire.sound` → IsCompiled=true, IsCompileFailed=false. ✅
3. `asset_files` en cada `.sound` → `UnresolvedReferences`:
   - Antes del fix: `weapons/glock/glock_shoot_01.vsnd` etc. (9 eventos con refs rotas). ❌
   - Tras el fix (refs → `sounds/content/...`): sin unresolved (los `.wav` del proyecto se compilan a
     `.vsnd_c` automáticamente, mismo pipeline que `dry_fire.wav`, que ya resuelve). ✅
4. Refs a core verificadas en asset system: phys-impact-bone/meat/tool (4+4+4), metal_gib/wood_gib (4+4),
   flesh_bullet_impact (7), BluntWeapon flesh (40). ✅
5. `weapons/*` en asset system → Total 0 (confirmado: sin sonidos en paquetes montados). ❌ → motivó el fix.

Pendiente de validar tras merge a spike (coordinador): compilar en editor con los nuevos `.wav` presentes
y `asset_files` de los 20 eventos con 0 unresolved + un play test.
Ver `PENDING.md` para los huecos sin fuente verificada (night warning, stab propio del cuchillo).
