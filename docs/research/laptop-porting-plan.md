# Laptop Porting Plan — spike/laptop-content-stack → integration/wizard-holy-grail

**Fecha:** 2026-08-07
**Origen:** `spike/laptop-content-stack` @ **712f2c9** (10 commits sobre `feat/holy-grail-foundation` @ c9e5664 — base antigua)
**Destino:** `integration/wizard-holy-grail` (se publicará desde el sobremesa; no visible aún en remoto)
**Regla:** el portátil produce componentes portables (`UltimoBarrio.Content.*`). Este documento es la guía de portado cuando la rama destino aparezca en remoto.

---

## 1. Estado de la rama

- HEAD: `712f2c9` `feat(content): add production weapon asset pack`
- 10 commits, **sin push** (todo local en el portátil).
- Árbol limpio al cierre de cada fase; todo lo generado es portable por diseño (sin dependencias del core viejo excepto la API base del engine: Component/[Sync]/[Rpc], NavMeshAgent, ResourceLibrary, SceneUtility, NetworkSpawn).

## 2. Por commit

| # | SHA | Feature | Archivos | Dependencias | Supuestos de core | Conflictos esperados | Cherry-pick | Orden |
|---|---|---|---|---|---|---|---|---|
| 1 | `c1e1be9` | docs(research): refinar path del worldmodel USP | `Code/UltimoBarrio/Content/weapons/weaponcontentregistry.cs` (1 línea) | — | El path `sbox_pistol_usp` del worldmodel oficial | Si el core nuevo renombra el registry | **SÍ** (trivial) | 1 |
| 2 | `66609f4` | docs(research): catálogo del stack | 8 docs en `docs/research/workers/` | — | — | Ninguno (docs) | **SÍ** | 1 |
| 3 | `081f04c` | fix(content): casing canónico | 18 .cs de Content + `scripts/labs/README.md` | — | ⚠️ Índice git en minúsculas (`core.ignorecase=true`); C# no exige filename=classname, pero normalizar a PascalCase al portar | Si el core nuevo ya tiene `UltimoBarrio.Content.*` con otra convención | **SÍ** (con normalización de casing) | 2 |
| 4 | `0f14eca` | feat(spike): weapon lab | `Assets/scenes/spikes/weapon_lab.scene`, `scripts/labs/generate_lab_scenes.py` | Prefabs de armas (commit 9) | Scene JSON `__version 2`; spawners Dev en la escena | StartupScene / sbproj si el core nuevo cambia el arranque | **SÍ** | 4 |
| 5 | `c14ac4e` | feat(spike): enemy lab | `Assets/scenes/spikes/enemy_lab.scene` | Prefabs de enemigos (commit 8); `MapInstance thieves.rpdowntown3t` (dependencia de mapa, no se copia) | NavMesh en `thieves.rpdowntown3t` | El core ya tiene raids/Saqueador propio → decidir quién manda en la escena | **SÍ** | 5 |
| 6 | `d9cfdf9` | feat(spike): building lab | `Assets/scenes/spikes/building_lab.scene` | Prefabs de fortificación (commit 7) | — | Bajo (escena aislada) | **SÍ** | 6 |
| 7 | `ac552f3` | feat(spike): vehicle lab (stub) | `Assets/scenes/spikes/vehicle_lab.scene` | **Kit de vehículos aún sin PRIMARY** (Vehicle Physics Kit vs Vehicle Prototyping ⚠️) | El lab queda como stub hasta elegir kit | Bajo (escena vacía) | **SÍ** | 8 |
| 8 | `8eb4253` | feat(content): fortificación (9 prefabs) | `Assets/prefabs/content/fortification/*.prefab` | Hosts de fortificación (commit 3) | Fallbacks del engine (`crate01`, `metal_wheely_bin`, `cardboard_box_open`) ⚠️ PENDING_VERIFY | IDs de objeto vs housing/ownership del core | **SÍ** (JSON puro) | 3 |
| 9 | `1d2047f` | feat(content): enemigos (3 prefabs) | `Assets/prefabs/content/enemies/*.prefab` | Hosts de enemigos (commit 3) | Modelo Citizen ⚠️ (riesgo EULA T5); NavMeshAgent | Sistema de NPC/raids del core | **SÍ** (JSON puro) | 3 |
| 10 | `712f2c9` | feat(content): armas (8 prefabs) + gobernanza | `Assets/prefabs/content/weapons/*.prefab`, `Assets/asset-registry.yml`, `THIRD_PARTY_NOTICES.md` | Hosts de armas (commit 3) | Paths de armas oficiales ⚠️ PENDING_VERIFY | `asset-registry.yml`/`THIRD_PARTY_NOTICES.md` → **merge manual** (no overwrite); IDs de item vs `ItemRegistry` del core (`weapon_usp` ya existe allí) | **SÍ** (prefabs) / **MERGE** (gobernanza) | 3 |

## 3. Clasificación de archivos

### COPY DIRECT (sin tocar)
- `Assets/prefabs/content/**` (JSON puro; validar con Cloud Browser antes).
- `scripts/labs/generate_lab_scenes.py` + `scripts/labs/README.md`.
- `docs/research/workers/*.md`, `docs/research/laptop-content-integration-manifest.md`, `docs/research/native-inventory-migration-spike.md`, `docs/research/laptop-porting-plan.md`.
- `Assets/scenes/spikes/*.scene` (aisladas; no alteran el startup del juego).

### CHERRY-PICK (commit completo, con revisión)
- Contratos: `Code/UltimoBarrio/Content/IDamageTarget.cs`, `IWeaponContentAdapter.cs`, `IEnemyContentAdapter.cs`, `IFortificationContentAdapter.cs`.
- Definiciones + registries: `WeaponContentDefinition/Registry`, `EnemyArchetypeDefinition/Registry`, `LootTableDefinition`, `FortificationContentDefinition/Registry`.
- `scripts/labs/README.md`.

### ADAPT (portar y conectar al core nuevo)
- Hosts: `WeaponContentHost` (equipar vía API de armas de primera parte en vez de hotbar vieja), `EnemyContentHost` (al sistema de raids/NPC del core), `FortificationContentHost` (al ownership/housing del core; el host NO debe implementar ownership real).
- Dev spawners: `LabWeaponSpawner`, `LabEnemySpawner`, `LabBuildingSpawner`, `LabVehicleSpawner` (`Scene.GetAllComponents<T>` → adaptar si el core cambia convenciones).
- `weaponcontentregistry.cs`: unificar IDs con el `ItemRegistry` del core.
- Normalizar casing de paths (índice en minúsculas → convención del core).

### DO NOT PORT
- Nada en esta rama (todo se creó portable). Excepción: no portar los prefabs de armas/enemigos/fortificación **hasta** sustituir los fallbacks ⚠️ PENDING_VERIFY por assets reales verificados (o mantenerlos marcados como placeholders).

## 4. Orden de integración recomendado

1. **Docs** (commits 1-2): research + manifest + spike + este plan.
2. **Contratos** (`IDamageTarget`, adapters) — la frontera con el core.
3. **Definiciones + registries + prefabs** (commits 3, 8, 9, 10): merge de gobernanza primero (`asset-registry.yml`, `THIRD_PARTY_NOTICES.md`).
4. **Lab scenes + generador** (commits 4-6, 7 al final).
5. **Hosts adaptados** al core nuevo (armas → API de primera parte; enemigos → raids; fortificación → ownership).
6. **Validación en el sobremesa**: compile → Play → labs → QA (regla de bloques pequeños: un bloque → compile → editor → Play → evidence → commit).

## 5. Conflictos esperados (resumen)

| Área | Conflicto | Resolución |
|---|---|---|
| Items | `weapon_usp`/`ammo_9mm` ya existen en el `ItemRegistry` del core | Unificar IDs en el registry; los prefabs de content referencian por string |
| NPC | El core ya tiene `SaqueadorBrain`/raids | El `EnemyContentHost` se porta como *variante portable*; el raid del core manda |
| Inventory | El core vive sobre `InventoryComponent`; T1 = CANDIDATE ARCHITECTURE (spike) | No migrar hasta el spike; los content packs no tocan inventario |
| Ownership | El core implementa housing ownership | `FortificationContentHost` delega ownership a la interfaz del core (TODO marcado en código) |
| Gobernanza | `asset-registry.yml` + notices divergen | Merge manual conservando ambos historiales |
| Casing | Índice en minúsculas por `core.ignorecase` | Normalizar al portar; C# no depende del filename |

## 6. Verificación de portado

- `git cherry-pick` por commit en orden de la tabla; resolver conflictos con la tabla de la sección 5.
- Tras cada cherry-pick: compile en el editor del sobremesa (0 errores propios).
- Sustituir PENDING_VERIFY solo con assets vistos en Cloud Browser.
- Marcar en `asset-registry.yml` el identificador real de cada asset verificado.
