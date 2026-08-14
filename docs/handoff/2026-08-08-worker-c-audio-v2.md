# Worker C — Audio Runtime (turbo session 2) — Handoff

_Fecha: 2026-08-08 (tarde). Rama: `agent/audio-v2` (desde `spike/laptop-content-stack` @ `46b3b01`)._
_Validación runtime: coordinador (tras merge a spike). Editor serial respetado: nunca play/compile._

## Resultado

El banco de sonidos **YA compilaba y cargaba en el editor** (los 17 .sound del worker F están
registrados en el asset system, `IsCompiledAndUpToDate=true`). El problema real era de **rutas**:
9 de 15 eventos referenciaban `weapons/...` (rutas del gamemode sandbox) que NO resuelven —
la colección `facepunch.sboxweapons` es **solo modelos** (verificado con `get_package` +
`asset_search`: 0 vsnd en paquetes montados). Consola mostraba `ERROR_FILEOPEN: File not found`
para `weapons/glock/glock_shoot_01.vsnd_c` etc. → esos eventos NO habrían sonado.

**Fix:** los 9 eventos rotos ahora referencian wavs MIT byte-exactos importados del repo oficial
`Facepunch/sandbox` (mismo pipeline que `dry_fire.wav`), compilados por el editor a
`sounds/content/weapons/*.vsnd` (proyecto, siempre resuelven). +3 eventos nuevos.

## Validación MCP read-only (editor 26.08.05, spike worktree con copia temporal, luego revertida)

- 20/20 `.sound`: `IsCompiled=true`, `IsCompiledAndUpToDate=true`, `IsCompileFailed=false`.
- 20/20: `asset_files` → `UnresolvedReferences=[]` (0).
- 20 vsnd nuevos compilados on-demand sin errores; 0 warnings nuevos en consola.
- Evidencia del bug pre-fix en consola (18:50): `ERROR_FILEOPEN weapons/glock/glock_shoot_01.vsnd_c`.
- La copia temporal en `ultimo-barrio` fue revertida (`git checkout` + `git clean`): worktree limpio.

## Entregables (commits en `agent/audio-v2`, push -u origin OK)

| Commit | SHA | Contenido |
|---|---|---|
| dd8e914 | `dd8e914acfaa0ad11ae43816b1b58ef5a56fa89f` | import 20 MIT wavs (14 armas + 6 darren) |
| 7c9b4a3 | `7c9b4a3aeb9ba65c36f41537c822acc6900a8135` | fix 9 SoundEvents rotos + 3 nuevos (knife_swing, enemy_alert, enemy_attack) |
| 0b88201 | `0b88201795fcd0462f820c67f3a417dfd611b959` | WeaponContentDefinition: campos DeploySound/ReloadStartSound/MeleeHitSound + wiring |
| 1542ed6 | `1542ed6db4a8f6c2117d9c0393538673b71fcca3` | docs: registry + notices + readme + pending (+fix YAML pre-existente) |

SHA-256 (16 primeros) de los 20 `.sound` y 21 `.wav`: ver tabla en el cuerpo del reporte del worker C.
Git blob SHAs disponibles con `git rev-parse <commit>:<path>`.

## Estado por sonido (tabla)

| Evento | Estado |
|---|---|
| usp_fire, usp_reload_magout, usp_reload_magin, usp_deploy, usp_dry | VALID SOUND EVENT / LOADS (refs proyecto, MIT) |
| shotgun_fire, shotgun_reload, shotgun_reload_start | VALID / LOADS (MIT) |
| crowbar_swing, crowbar_impact, knife_swing | VALID / LOADS (MIT) |
| enemy_alert, enemy_attack | VALID / LOADS (MIT darren; voz robótica — **OK de diseño pendiente**) |
| enemy_death, enemy_hurt, barricade_impact, door_impact, repair, bullet_impact_flesh, melee_impact_flesh | VALID / LOADS (engine core, sin cambios) |
| night_warning | PENDING — **no existe sirena** en core ni en repo MIT (verificado); único candidato del core `electric-buzz-loop` (loop de ambience, descartado). Fuentes CC0 externas documentadas en pending.md |

## Bloqueos / para el coordinador

1. **PLAYS** sin validar (editor serial): tras merge a spike, play test + `Sound.Play("sounds/content/weapons/usp_fire.sound")`.
2. **darren (enemy_alert/attack): OK de diseño pendiente** (voz robótica, no humana). Reemplazable por voces CC0.
3. **knife_swing** comparte swing_01/02 con el crowbar (mismo fit acústico); OK de diseño para stab propio del paquete del cuchillo pendiente.
4. **night_warning**: sin fuente verificada en engine/repo MIT; NO se creó el evento (regla: no randomizar fuentes).
5. `facepunch.sboxweapons-sounds` (entrada vieja del registry) fue sustituida por `facepunch.sandbox-game-weaponsounds` — el sbproj no necesita cambios (los sonidos ya no dependen de paquetes).
6. YAML del registry: corregí colons sin quotes pre-existentes (líneas 119, 215, 245, 314, 377, 397, 416, 496, 513) — el archivo ahora parsea (27 assets).

## Notas de portabilidad (guard)

- Solo `Assets/sounds/content/**` + `Code/UltimoBarrio/Content/weapons/*` + docs. Sin tocar core/scenes.
- `.sound` no referencian NINGÚN path de paquete cloud (solo proyecto + engine core) → 0 dependencias nuevas.
- Reproducción: `Sound.Play("sounds/content/.../x.sound", pos)` — path completo con `.sound` (convención del código oficial, ej. `Sound.Play("sounds/flatline.sound")`).
