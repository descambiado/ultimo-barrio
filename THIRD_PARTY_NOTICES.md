# Third-party notices

Este repositorio contiene o referencia contenido de terceros. Cada elemento conserva su licencia original.

La lista operativa se mantiene en:

```text
Assets/asset-registry.yml
```

## Reglas

- La licencia del código original del proyecto no relicencia assets externos.
- Los paquetes deben consumirse como referencia/dependencia cuando sea posible.
- Las librerías copiadas a `Libraries/` mantienen sus avisos.
- No eliminar autoría, LICENSE, NOTICE o metadatos.
- No publicar fuentes o binarios de terceros cuando su licencia no lo permita.
- Nada de lo marcado `PENDING_VERIFY` en `Assets/asset-registry.yml` entra en una build pública sin verificación previa.

## Registros iniciales

### Facepunch — s&box Weapons

Colección oficial de modelos, viewmodels, cargadores, munición y accesorios. La integración exacta y los términos aplicables deben verificarse dentro del editor y la documentación del paquete antes de publicar.

### OmniParadigm — Weapons

Paquete propuesto por el equipo. Estado: pendiente de spike técnico y legal. No se considera dependencia obligatoria hasta completar la evaluación.

## Paquete de contenido — spike/laptop-content-stack

Referencias candidatas del content pack portable (rama `spike/laptop-content-stack`).
Todas ellas están **pendientes de verificación** (licencia y ruta exacta) con Cloud Browser
antes de cualquier uso en build pública. Los prefabs del pack usan hoy únicamente
fallbacks verificados del contenido oficial del engine (`models/dev/*`, `models/citizen_props/*`, `models/sbox_props/*`).

| Candidato | Uso previsto | Estado |
|---|---|---|
| `facepunch.w_usp` | Worldmodel USP | PENDING_VERIFY — confirmar ruta montada y términos del paquete sboxweapons |
| `facepunch.w_crowbar` / palanca | Worldmodel melee | PENDING_VERIFY — confirmar existencia y licencia |
| `facepunch.knife` | Worldmodel cuchillo | PENDING_VERIFY — confirmar existencia y licencia |
| `facepunch.w_shotgun` | Worldmodel escopeta | PENDING_VERIFY — confirmar existencia y licencia |
| `facepunch.ammo_9mm` / `ammo_buckshot` | Modelos de munición | PENDING_VERIFY |
| `models/citizen/citizen.vmdl` + `.animgraph` | Modelo base de enemigos (Saqueador/Bruto/Merodeador) | PENDING_VERIFY — modelo estándar de Facepunch; confirmar ruta |
| Modelos de barricada/puerta/banco/generador/alarma | Fortificaciones | PENDING_VERIFY — sustituir cubos; investigar en asset store |
| SoundEvents (`weapon.usp.fire`, `build.wood.place`, etc.) | Audio del pack | PENDING_VERIFY — crear graphs propios o licenciar CC0/CC-BY |

Modelos ya verificados en el engine y usados como fallback hoy:

- `models/dev/plane.vmdl`, `models/dev/box.vmdl`
- `models/citizen_props/crate01.vmdl`
- `models/sbox_props/cardboard_box/cardboard_box_open.vmdl`
- `models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl`

## Audio importado — Facepunch/sandbox (MIT)

Los siguientes archivos de audio se importan del repositorio oficial `Facepunch/sandbox`
(https://github.com/Facepunch/sandbox), licencia MIT, y se copian byte-exactos
(verificados por tamaño vía GitHub API el 2026-08-08). Se conserva el aviso completo de
licencia en `docs/licenses/MIT-Facepunch-sandbox.txt`. Cada `.sound` del banco que los
referencia los compila el editor a `.vsnd_c` como assets de proyecto
(`sounds/content/...`), sin dependencia de paquetes cloud.

### Armas (Assets/sounds/content/weapons/)

| Archivo local | Origen en el repo (main) | Uso |
|---|---|---|
| `glock_shoot_01.wav` | `Assets/weapons/Glock/glock_shoot_01.wav` | `usp_fire.sound` (la USP usa este sonido; ver commit 1b77a36f) |
| `pistol_mag_in.wav` | `Assets/weapons/Glock/reload/pistol_mag_in.wav` | `usp_reload_magin.sound` |
| `pistol_mag_out.wav` | `Assets/weapons/Glock/reload/pistol_mag_out.wav` | `usp_reload_magout.sound` |
| `foley_deploy_weapon_03.wav` | `Assets/weapons/Common/Foley/foley_deploy_weapon_03.wav` | `usp_deploy.sound` (deploy genérico) |
| `shotgun1_shoot1.wav` | `Assets/weapons/Shotgun/sounds/Shotgun1_Shoot1.wav` | `shotgun_fire.sound` |
| `shotgun1_shoot2.wav` | `Assets/weapons/Shotgun/sounds/Shotgun1_Shoot2.wav` | `shotgun_fire.sound` |
| `shotgun_cock.wav` | `Assets/weapons/Shotgun/sounds/shotgun_cock.wav` | `shotgun_reload_start.sound` |
| `shotgun_load.wav` | `Assets/weapons/Shotgun/sounds/shotgun_load.wav` | `shotgun_reload.sound` |
| `swing_01.wav` | `Assets/weapons/Crowbar/sounds/swing_01.wav` | `crowbar_swing.sound`, `knife_swing.sound` |
| `swing_02.wav` | `Assets/weapons/Crowbar/sounds/swing_02.wav` | `crowbar_swing.sound`, `knife_swing.sound` |
| `crowbar_hit_01.wav` | `Assets/weapons/Crowbar/sounds/crowbar_hit_01.wav` | `crowbar_impact.sound` |
| `crowbar_hit_02.wav` | `Assets/weapons/Crowbar/sounds/crowbar_hit_02.wav` | `crowbar_impact.sound` |
| `crowbar_hit_03.wav` | `Assets/weapons/Crowbar/sounds/crowbar_hit_03.wav` | `crowbar_impact.sound` |
| `crowbar_hit_04.wav` | `Assets/weapons/Crowbar/sounds/crowbar_hit_04.wav` | `crowbar_impact.sound` |

### Enemigos (Assets/sounds/content/enemies/)

| Archivo local | Origen en el repo (main) | Uso |
|---|---|---|
| `scared_01.wav` … `scared_06.wav` | `Assets/sounds/npc/darren/scared/scared_0X.wav` | `enemy_alert.sound`, `enemy_attack.sound` |

Nota: los sonidos del NPC darren son voces robóticas/dispositivo (no humanas); se usan como
candidato documentado para alerta/ataque de enemigos y quedan sujetos a OK de diseño
(ver `Assets/sounds/content/pending.md`).

## sousou63 — DarkRP 2 (MIT)

Repositorio: https://github.com/sousou63/DarkRP — licencia MIT.

Se reutilizan patrones y código de su framework de armas, presentación y NPCs. La
licencia MIT exige conservar el aviso de copyright y la nota de permiso; el archivo
`LICENSE` original se conserva junto a cualquier fichero portado.

Adoptado hasta ahora:

| Elemento | Origen en DarkRP | Uso en Último Barrio |
|---|---|---|
| Parámetros de animgraph `b_attack` / `b_reload` sobre `PlayerController.Renderer` | `Code/Game/Weapon/BaseBulletWeapon/BaseBulletWeapon.cs`, `BaseWeapon/BaseWeapon.Reloading.cs` | `Code/UltimoBarrio/Content/combateffects.cs` (`PlayAttackAnimation`, `PlayReloadAnimation`) |

Pendiente de evaluación para adopción posterior: `Code/Game/Weapon/` (BaseWeapon,
BaseBulletWeapon, IronSightsWeapon, MeleeWeapon, WeaponModel/ViewModel) y `Code/Npcs/`
(árbol de comportamiento: schedules, tasks, layers, combat, speech).
