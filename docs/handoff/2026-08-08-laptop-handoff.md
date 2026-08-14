# Ultimo Barrio - Laptop Handoff

_Fecha: 2026-08-08. Estado preservado al congelar la fábrica (handoff mode)._
_Este documento es la fuente única de verdad para continuar sin la conversación original._

## Repository

- remote: `https://github.com/descambiado/ultimo-barrio.git` (`origin`)
- branch estable: `spike/laptop-content-stack`
- HEAD estable: `e37ed82` (docs(handoff): preserve laptop runtime integration state)
- base original: `feat/holy-grail-foundation` @ `c9e5664` (antigua, ver Critical context)
- **PUBLICADO en GitHub** (2026-08-08): spike + agent/enemies + agent/building + agent/audio + agent/qa + checkpoint WIP; verificado con ls-remote (local == remoto en todas).

## Engine

- s&box build: `26.08.05`
- MCP endpoint: `http://127.0.0.1:7269/mcp` (JSON-RPC; helper local `workspace/.openclaw/tmp/mcp.ps1` + `mcp_params.json`, o `Invoke-RestMethod` inline)
- compile status: `local.ultimo_barrio` → 0 errores propios (2 warnings preexistentes del core antiguo: GameResourceAttribute obsolete en `ItemDefinition.cs`/`MovementProfile.cs`)
- tools MCP útiles: `editor_status` (IsCompiling/LastCompileSucceeded), `compile_status` (IsBuilding por compilador), `play_start/play_stop`, `read_console` (filter/minimumLevel), `list_scenes`, `open_scene` (integrado en spike como `0f16745`), `reload_active_scene_from_disk`, `asset_search`, `asset_info`, `find_packages`, `get_package`, `save_scene`, `camera_screenshot`

## Critical context

- **GitHub/main está atrasado** (`d5ed250`). No es la base de este trabajo.
- `feat/holy-grail-foundation` @ `c9e5664` es la base antigua de la que partió este portátil.
- Este portátil desarrolló **contenido portable** desde esa base: weapons, enemies (assets), fortification, audio, QA suite. Todo bajo `UltimoBarrio.Content.*` + `Assets/prefabs/content/**` + `Assets/scenes/spikes/**`, sin tocar el core viejo.
- El desarrollo más avanzado del JUEGO COMPLETO vive en otro equipo bajo `integration/wizard-holy-grail` y puede no estar publicado aún.
- **NO fusionar este branch directamente a main.** Portar selectivamente cuando `integration/wizard-holy-grail` esté disponible (orden y adapters en `docs/research/portable-runtime-bundle.md`).
- El contenido portable NO depende del core viejo; los adaptadores (`IWeaponContentAdapter`, `IDamageTarget`, `IEnemyContentAdapter`, `IFortificationContentAdapter`, `BuildStructureHost`) son el punto de unión con el core nuevo.

## Runtime matrix

| Dominio | Estado |
|---|---|
| Weapon USP | PASS (runtime, editor 26.08.05, play local) |
| Weapon Crowbar | PASS (runtime; crowbar01 engine content) |
| Weapon Knife | PASS (runtime; w_trenchknife/v_m9bayonet cloud) |
| Weapon Shotgun | PASS (runtime; w_spaghellim4/v_spaghellim4 cloud, delta 96 = 8 pellets × 12) |
| WeaponSuite combinada | PASS 4/4 (`[UBSuite] Suite run complete (4 PASS, 0 FAIL, 0 SKIP)`, 2026-08-08 14:42, rig-7) |
| Cold restart cloud | PASS (editor cerrado y relanzado; paquetes montados solos vía PackageReferences) |
| Enemy | **PARTIAL** - assets integrados (prefabs citizen reales + loot pickups + registry, worker B); gameplay/AI en rama `agent/enemies` @ `d4c0c93` **WIP sin validar** (EnemyContentDefinition/Perception/Attack/TestRig escritos, NO compilados/integrados) |
| Building | **PASS** - building_lab 9/9 PASS (19:00): WoodenBarricade lifecycle completo (preview invalid/blocked/valid, spawn, overlap, damage, repair, upgrade, destroy) + 8 restantes (registry+prefab+spawn+HP+modelo: reinforced_barricade, basic_door, reinforced_door, stash, workbench, generator, alarm, repair_station). Commits `1fff31c` + `b470365` |
| Vehicle | **PENDING** - sin trabajo entregado; rama `agent/vehicles` vacía; research previo en `docs/research/workers/vehicles.md`; falta elegir kit PRIMARY y validar loop |
| Audio | **STATIC ONLY** - banco integrado (15 SoundEvents en `Assets/sounds/content/` + dry_fire.wav MIT + PENDING 4: knife_swing/enemy_alert/enemy_attack/night_warning); **no compilado en editor** (pendiente validar .sound) |
| QA | ContentRuntimeSuite integrado y VALIDADO (ILabSuite + runner `[UBSuite]`; WeaponSuite refactorizado; stubs Enemy/Building/VehicleSuite) |
| Cloud | 9 PackageReferences en `ultimo_barrio.sbproj` (portable, montaje automático al abrir proyecto); `Cloud.Model("ident")` exige string literal (SB2000) |

## PackageReferences

Copia EXACTA de `ultimo_barrio.sbproj` (2026-08-08):

```json
"PackageReferences": [
  "facepunch.w_usp",
  "facepunch.v_usp",
  "facepunch.w_trenchknife",
  "facepunch.v_m9bayonet",
  "facepunch.w_spaghellim4",
  "facepunch.v_spaghellim4",
  "facepunch.ammobox12g",
  "facepunch.12g_shell",
  "facepunch.12gshellcasing"
]
```

`install_package` NO persiste la referencia (solo caché `.sbox/cloud`); la forma durable es PackageReferences.

## Validated commits

| SHA | Dominio | Estado | Qué contiene |
|---|---|---|---|
| `9839846` | weapons | RUNTIME VALIDATED | USP loop (equip/fire/trace/damage/ammo/reload/drop) |
| `f302efe` | docs | - | STATE weapon lab USP |
| `6913adb` | weapons | RUNTIME VALIDATED | Crowbar melee loop (crowbar01 engine) |
| `9fd7b32` | weapons+cloud | RUNTIME VALIDATED | Suite 4/4 + PackageReferences + prefabs reales |
| `7433f69` | docs | - | STATE suite 4/4 + registry knife/shotgun |
| `d2e1132` | QA | RUNTIME VALIDATED | Refactor WeaponTestRig → WeaponSuite (ILabSuite) |
| `9055be6` | QA | STATIC ONLY | Stubs EnemySuite/BuildingSuite/VehicleSuite |
| `b470365` | building | RUNTIME VALIDATED | building_lab 9/9 PASS: rig modo FullLifecycle + escena 9 tests |
| `1fff31c` | building | RUNTIME VALIDATED | WoodenBarricade lifecycle PASS (fix SetBalance + ResolveModelFromPrefab) |
| `0f16745` | infra | RUNTIME VALIDATED | open_scene MCP tool (desde checkpoint e871fe7) |
| `a8cbd03` | QA | - | scripts/labs/README (pipeline coordinador) |
| `21d199f` | QA | RUNTIME VALIDATED | ILabSuite + ContentRuntimeSuite runner ([UBSuite] PASS) |
| `494b21b` | building assets | STATIC ONLY | 9 prefabs fortificación modelos engine reales (4 proxy visual) |
| `fafb557` | building assets | - | asset-registry sección building |
| `7222416` | enemies assets | STATIC ONLY | 3 prefabs enemy citizen reales + tint por archetype |
| `8a71468` | enemies assets | STATIC ONLY | Loot pickups (12g, trench knife, crowbar) |
| `21dfb14` | enemies assets | - | registry verify enemy/loot |
| `7727d4b` | docs | - | Porting plan filas enemies (aplicado a tabla H) |
| `4249201` | building system | STATIC ONLY | BuildDefinition + FortificationContentRegistry (9 defs) |
| `87e176f` | building system | STATIC ONLY | BuildPlacementRules + BuildStructureHost + prefabs retype |
| `b79dea3` | building system | STATIC ONLY | BuildingTestRig + LabResourceFixture + building_lab.scene |
| `9d8eab0` | docs | - | STATE building data layer |
| `cc16de7` | audio | STATIC ONLY | Banco SoundEvents (15) + dry_fire.wav MIT |
| `dfdf543` | docs | - | Registry sounds + MIT attribution (HEAD estable) |

No integrados (ramas worker): `d4c0c93` (enemies WIP, agent/enemies), `e871fe7` (checkpoint building diagnostic WIP).

## Worker branches

| branch | HEAD | integrated? | pushed? | estado |
|---|---|---|---|---|
| `agent/enemies` | `d4c0c93` | no (solo assets de B: `7222416`/`8a71468`/`21dfb14` sí) | **sí** (push 2026-08-08) | WIP gameplay de A preservado (no compilado) |
| `agent/building` | `2120d2f` | sí (4 commits) | **sí** | sistema building integrado |
| `agent/audio` | `802c826` | sí (2 commits) | **sí** | banco de sonidos integrado |
| `agent/qa` | `76a04d9` | sí (4 commits) | **sí** | QA infra integrada |
| `agent/vehicles` | `9fd7b32` | - | no (sin trabajo único) | sin trabajo (worker E no entregó commits) |
| `checkpoint/laptop-turbo-wip-20260808` | `e871fe7` → `a8a5a56` | no | **sí** (e871fe7; a8a5a56 = DebugDump diagnóstico, también pusheado) | WIP diagnóstico building (open_scene + rig-2 + DebugDump) - PUEDE ESTAR ROTO |
| `spike/laptop-content-stack` | `e37ed82` | - | **sí** (push + verificación ls-remote 2026-08-08) | rama estable del portátil (con handoff) |
| `checkpoint/laptop-weapon-lab-frankenplayer` | `93dcf9e` | no | no | experimento Frankenstein descartado (no portar) |

## Known blockers

### NRE BuildingTestRig.CheckRegistryCoverage — RESUELTO (2026-08-08 18:56)

- **Causa raiz: assembly/hotload STALE, no bug del codigo.** El cold restart del editor (cerrar proceso sbox-dev + relanzar) hizo desaparecer el NRE: el static ctor de `FortificationContentRegistry` ejecutaba una DLL intermedia (keys correctas, values null — confirmado con DebugDump en el checkpoint). Con arbol estable + editor limpio: registry 9/9 y lifecycle completo PASS (building_lab 9/9 PASS 19:00).
- Fixes reales integrados (el DebugDump NO se porto): `buildingtestrig.cs` rig-2 `_fixture.SetBalance(Entry.FixtureBalance)` (el repair fallaba con balance 0) y `buildstructurehost.cs` `ResolveModelPath` + `ResolveModelFromPrefab` (`ResourceLibrary.Get<Model>` no resuelve assets @nosource del engine por ruta; con prefab fallback el upgrade cambia old_bench -> security_shutter_box_middle).
- **Leccion proceso**: al editar un `.scene` por fuera del editor, `reload_active_scene_from_disk` es obligatorio antes de play (el editor juega la escena en memoria).

### NRE en BuildingTestRig.CheckRegistryCoverage (building_lab runtime FAIL)

- archivo: `Code/UltimoBarrio/Content/Dev/BuildingTestRig.cs`
- método: `CheckRegistryCoverage()` (llamado desde `OnStart()`, línea ~80)
- línea aproximada: 104 (el `Log.Info` del `foreach ( var def in all )`)
- síntoma: `[BuildingLab] Registry: 9 definiciones` seguido de `NullReferenceException` en el primer elemento del foreach
- último diagnóstico (rig-2, en checkpoint `e871fe7`): el null-check confirma que **`FortificationContentRegistry.All` contiene una entrada null** a pesar de que el static ctor registra 9 factories que devuelven `new BuildDefinition {...}` (ninguna devuelve null; las 9 keys son no-null). Pendiente: determinar por qué `_order.Select(id => _definitions[id])` produce un null (sospecha: el assembly del juego corrió con un DLL intermedio; verificar primero con un play limpio tras recompilar todo, luego inspeccionar `_definitions`/`_order` en runtime).
- último cambio aplicado: null-check defensivo + `VERSION=rig-2` (solo en checkpoint, NO en spike)
- siguiente comprobación: 1) play limpio de `building_lab.scene` con el árbol estable (rig-1) para confirmar el NRE reproducible; 2) loguear `_order` y `_definitions.Keys` antes del foreach; 3) si el ctor estático es correcto, comprobar que no hay DOS assemblies cargados (DLL viejo en caché).

### open_scene (infra MCP, en checkpoint `e871fe7`)

- `Editor/UltimoBarrioMcpTools.cs` añade la tool MCP `open_scene` (integrado en spike como `0f16745`)`), validada (abrió building_lab y weapon_lab; ToolCount 53→54). Está SOLO en el checkpoint; cherry-pickear a spike si se quiere abrir escenas vía MCP (sin ella, el editor solo restaura la última escena abierta).

## Do not redo

- No rehacer weapon research (4/4 PASS cerrado).
- No inventar cloud idents (verificar SIEMPRE con `find_packages`/`get_package`; falsos confirmados: `facepunch.knife`, `facepunch.w_shotgun`, `facepunch.ammo_9mm`).
- No crear PlayerController propio (el manual es copia literal del template del engine: `Assets/prefabs/content/dev/lab_player_official.prefab`).
- No reinstalar packages manualmente (la forma durable es PackageReferences del sbproj).
- No asumir "wait 60 s" (leer compile con `IsBuilding=false` + `Success=true`; usar marcador `[LabBuild] VERSION`).
- No reescribir inventory/housing/persistence del portátil (prohibido tocar core viejo).
- No usar `get_game_object` con `includeComponentProperties` en objetos con List<> (cuelga el MCP).
- No portar el Frankenstein (`checkpoint/laptop-weapon-lab-frankenplayer` + `frankenplayer-experiment.patch` = experimento descartado).

## Resume exactly here

```bash
git clone https://github.com/descambiado/ultimo-barrio.git
cd ultimo-barrio
git fetch --all --prune
git switch spike/laptop-content-stack
```

1. Leer `docs/handoff/2026-08-08-laptop-handoff.md`, `STATE.md`, `docs/research/laptop-porting-plan.md`.
2. Primera escena: `Assets/scenes/spikes/weapon_lab.scene` (play → `[UBSuite]` 4 PASS = el stack sigue sano).
3. Primer test tras weapons: `Assets/scenes/spikes/building_lab.scene` (play → FAIL esperado: NRE CheckRegistryCoverage → seguir Known blockers).
4. Primer fichero a inspeccionar: `Code/UltimoBarrio/Content/Fortification/FortificationContentRegistry.cs` (static ctor + All) y `Code/UltimoBarrio/Content/Dev/BuildingTestRig.cs`.
5. Siguiente feature por orden: Saqueador (integrar `agent/enemies` d4c0c93 → compile → enemy_lab runtime) → wooden barricade → vehicle foundation → Bruto+Merodeador → fortification pack → audio compile → combined suite.
