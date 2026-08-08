# Laptop Porting Plan — spike/laptop-content-stack → integration/wizard-holy-grail

**Fecha:** 2026-08-08 (audit H)
**Origen:** `spike/laptop-content-stack` @ **7433f69** (18 commits sobre `feat/holy-grail-foundation` @ c9e5664 — base antigua)
**Destino:** `integration/wizard-holy-grail` (se publicará desde el sobremesa; no visible aún en remoto)
**Regla:** el portátil produce componentes portables (`UltimoBarrio.Content.*`). Este documento es la guía de portado cuando la rama destino aparezca en remoto.
**Auditoría:** Worker H (portability/audit). Verificado: 0 archivos de core viejo tocados en los 18 commits (`git diff c9e5664..7433f69 -- Code/UltimoBarrio/{Inventory,Players,Housing,Persistence,Missions,Raid} Assets/scenes/ultimo_barrio_alpha.scene Assets/prefabs/player.prefab` = vacío). Bundle runtime: `docs/research/portable-runtime-bundle.md`.

---

## 1. Estado de la rama

- HEAD: `7433f69` `docs(state): weapon lab suite 4/4 runtime validated, registry knife/shotgun verified`
- 18 commits, **sin push** (todo local en el portátil).
- Suite de armas **RUNTIME VALIDATED 4/4** (USP, crowbar, cuchillo, escopeta): daño real `Fire → trace → IDamageTarget`, 0 errores runtime.
- Dependencias cloud **persistidas** en `ultimo_barrio.sbproj` → `PackageReferences` (9 idents; montaje automático al abrir proyecto).
- Dependencias de engine: `Component/[Sync]/[Rpc]`, `Cloud.Model()` (string literal — constraint SB2000), `ResourceLibrary`, `Scene.Camera` trace, `NavMeshAgent` (enemigos), `GameObject.Clone`, `TimeSince/TimeUntil`, `Networking`.

## 2. Por commit (los 18; filas nuevas del audit H marcadas con ⚠️)

| # | SHA | Dominio | Archivos | Deps engine | Cloud pkgs | Deps core viejo | Portable direct? | Adapter? | Conflictos esperados |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `712f2c9` | content: armas | `Assets/prefabs/content/weapons/*.prefab` (8), `Assets/asset-registry.yml`, `THIRD_PARTY_NOTICES.md` | ModelRenderer, prefab JSON | — | — | **SÍ** (JSON puro) | no | `asset-registry.yml`/notices → merge manual; IDs vs `ItemRegistry` core (`weapon_usp` ya existe) |
| 2 | `1d2047f` | content: enemigos | `Assets/prefabs/content/enemies/*.prefab` (3: saqueador/bruto/merodeador) | ModelRenderer, prefab JSON | — | — | **SÍ** | no (host sí, ver #17) | Modelo Citizen ⚠️ EULA T5; raids/NPC del core manda |
| 3 | `8eb4253` | content: fortificación | `Assets/prefabs/content/fortification/*.prefab` (9) | ModelRenderer, prefab JSON | — | — | **SÍ** | no (host sí) | IDs vs housing/ownership del core |
| 3b ⚠️ | `6c19f14` (worker D) | content: fortificación — modelos REALES engine | 9 prefabs actualizados: barricade_wood→`models/sbox_props/benches/old_bench.vmdl`, barricade_reinforced→`security_shutter_box_middle`, door_basic→`security_shutter_curtain_128` (PROXY: no hay puertas vmdl en engine, solo `maps/prefabs/doors/*.vmap`), door_reinforced→`security_shutter_curtain_bottom` (PROXY), stash→`citizen_props/gritbin01_combined`, workbench→`citizen_props/oldoven` (PROXY), generator→`props/aircon_unit_wall/aircon_unit_medium_wall` (PROXY), alarm→`sbox_props/intruder_alarm_2` (directa), repair_station→`props/mobile_masts/microwave_trans` (PROXY) | asset_search + asset_info IsCompiled | — | — | **SÍ** (JSON puro) | no | sustituir los 5 PROXY visuales en integración (puertas: componente PrefabFile con .vmap) |
| 4 | `0f14eca` | spike: weapon lab | `Assets/scenes/spikes/weapon_lab.scene`, `scripts/labs/generate_lab_scenes.py` | prefab JSON `__version 2` | — | — | **SÍ** | no | StartupScene/sbproj si el core cambia el arranque |
| 5 | `c14ac4e` | spike: enemy lab | `Assets/scenes/spikes/enemy_lab.scene` | NavMesh (mapa `thieves.rpdowntown3t`, no se copia) | — | — | **SÍ** | no | raids/Saqueador del core → decidir quién manda |
| 6 | `d9cfdf9` | spike: building lab | `Assets/scenes/spikes/building_lab.scene` | — | — | — | **SÍ** | no | bajo (escena aislada) |
| 7 | `ac552f3` | spike: vehicle lab (stub) | `Assets/scenes/spikes/vehicle_lab.scene` | — | — | — | **SÍ** | no | bajo; kit de vehículos aún sin PRIMARY |
| 8 | `081f04c` | fix(content): casing | 18 .cs de Content + `scripts/labs/README.md` | — | — | — | **SÍ** (normalizar PascalCase) | no | índice en minúsculas (`core.ignorecase=true`) |
| 9 | `66609f4` | docs: catálogo stack | `docs/research/workers/*.md` (8) | — | — | — | **SÍ** | no | ninguno (docs) |
| 10 | `c1e1be9` | docs: worldmodel USP | `Code/UltimoBarrio/Content/weapons/weaponcontentregistry.cs` (1 línea) | — | — | — | **SÍ** (trivial) | no | si el core renombra el registry |
| 11 | `7827872` | docs: verdicts research | `docs/research/laptop-content-integration-manifest.md` | — | — | — | **SÍ** | no | ninguno (docs) |
| 12 | `358585f` | docs: inventory spike | `docs/research/native-inventory-migration-spike.md` | — | — | — | **SÍ** | no | ninguno (docs) |
| 13 ⚠️ | `93dcf9e` | docs: porting plan | `docs/research/laptop-porting-plan.md` | — | — | — | **SÍ** | no | ninguno (docs; este plan) |
| 14 ⚠️ | `9839846` | feat(spike): USP validated | `weaponcontenthost.cs` (+cloud resolver), `weaponcontentdefinition.cs` (+Cloud*Id), `weaponcontentregistry.cs`, `dev/weapontestrig.cs` (nuevo rig-2, sin pawn), `dev/labdamagedummy.cs`, `dev/lab_player_official.prefab` (template engine literal), `weapon_lab.scene` (+rig/dummy), `v_usp_content.prefab`, `w_usp_content.prefab`, `asset-registry.yml` | `Cloud.Model()` literals, `Scene.Camera` trace, `Component`, `[Sync]/[Rpc]`, `Rigidbody`/`SkinnedModelRenderer` | facepunch.w_usp, facepunch.v_usp (runtime, vía Cloud.Model; aún NO persistidas en sbproj) | **ninguna** | **SÍ** | **SÍ** — `IWeaponContentAdapter` consumido por el core nuevo (equip/fire/reload); mapeo ident→literal en `ResolveCloudModel()` | `ub_weapon_usp` vs ItemRegistry core; sbproj sin PackageReferences aún (llega en #17) |
| 15 ⚠️ | `f302efe` | docs(state): USP validado | `STATE.md` | — | — | — | **SÍ** (docs) | no | STATE.md → merge manual (lo tocan otros workers/coordinador) |
| 16 ⚠️ | `6913adb` | feat(spike): crowbar validated | `weapontestrig.cs` (rig-6, suite data-driven `List<WeaponTestEntry>`), `labdamagedummy.cs` (+6), `weaponcontentregistry.cs` (+crowbar), `weapon_lab.scene`, `v/w_crowbar_content.prefab` | `crowbar01.vmdl` = engine content (sin cloud) | — | **ninguna** | **SÍ** | **SÍ** — melee sin SoundEvents (campos vacíos); daño vía IDamageTarget | facepunch.w_crowbar NO existe (descartado); fallback crate01 |
| 17 ⚠️ | `9fd7b32` | feat(spike): suite 4/4 + cloud persistida | `weaponcontentregistry.cs` (knife/shotgun: idents backend-verificados), `weaponcontenthost.cs` (+13), `weapontestrig.cs` (+28), `weapon_lab.scene`, `v/w_knife_content.prefab`, `v/w_shotgun_content.prefab`, **`ultimo_barrio.sbproj` (PackageReferences: 9 idents)** | igual #14; `Cloud.Model` knife/shotgun | **facepunch.w_usp, v_usp, w_trenchknife, v_m9bayonet, w_spaghellim4, v_spaghellim4, ammobox12g, 12g_shell, 12gshellcasing** (persistidas en sbproj) | **ninguna** | **SÍ** | **SÍ** — hosts ya implementan `IWeaponContentAdapter`/`IDamageTarget` | **sbproj = merge manual** (lista PackageReferences debe fusionarse con la del core, no overwrite); `AmmoType "ammo_buckshot"` = item id del core nuevo |
| 18 ⚠️ | `7433f69` | docs(state): suite 4/4 | `STATE.md`, `asset-registry.yml` (+ident verificados knife/shotgun) | — | — | — | **SÍ** (docs/data) | no | `asset-registry.yml` → merge manual (no overwrite); STATE.md → merge manual |

**Nota:** ramas `agent/*` (enemies/building/vehicles/audio/qa) sin commits nuevos a fecha del audit — todas en `9fd7b32`.

## 3. Clasificación de commits antiguos (pre-spike)

`e0bb0db` y `49595a4` **NO existen en el object DB local** (repo original del portátil; el spike se re-commitó desde working tree). Clasificación por CONTENIDO localizado en commits equivalentes de `feat/holy-grail-foundation`:

| SHA (orig. portátil) | Sistema | Contenido localizado en este repo | Clasificación | Nota |
|---|---|---|---|---|
| `e0bb0db` | Bruto/Merodeador | No hay código viejo con esos nombres (`git grep -i bruto\|merodeador c9e5664 -- Code` = vacío). Arquetipos equivalentes viejos: `AI/SaqueadorBrain.cs`, `AI/VecinoBrain.cs`, `AI/AIBase.cs`, `AI/PerceptionComponent.cs` (commits `8b48b00`, `548f07a`) | **DISCARD** (código: raids/Saqueador ya existen en el core nuevo) + **PORT DATA** (arquetipos saqueador/bruto/merodeador ya portados como `EnemyArchetypeDefinition` + prefabs content, commit `1d2047f`) + **PORT LOGIC** (state machine Idle/Patrol/Investigate/Detect/Attack/Retreat → patrón para el `EnemyContentHost` del core, vía `IEnemyContentAdapter`) | Los prefabs nuevos son la forma portable; el core manda en raids |
| `49595a4` | Mission journal | `Missions/MissionSystem.cs` + `MissionDefinition/ObjectiveType/Reward` (commit `49306c4` "data-driven MissionSystem + quest chain") | **DISCARD** (sistema: Missions/* es core viejo prohibido; el core nuevo tiene misiones) + **PORT DATA** (estructura de quest chain/objetivos/recompensas si el core quiere el tutorial) | No duplicar: exponer datos + contrato, no implementación |

### Weapons / Raids / AI / Economy antiguos (`git log --all`)

| Commit | Sistema | Clasificación | Nota |
|---|---|---|---|
| `28c5477` raids: night cycle + primer raid | **PORT LOGIC** (concepto night cycle/raid) / DISCARD de código | El core nuevo tiene raids; portar el patrón, no el sistema |
| `8b48b00` AI: Vecino/Saqueador vslice | **PORT LOGIC** | Base del `EnemyContentHost` futuro |
| `548f07a` V1 AI + Raid systems | **PORT LOGIC** / DISCARD | Superseded por core nuevo |
| `b13092d` economy/trading vslice | **DISCARD** + **PORT DATA** | Wallet/trader = core nuevo; portar tuning de precios (agua 10, medicina 20, munición 5, scrap venta 2, weapon_usp 100) |
| `7d362d1` / `60b212c` trader transactions | **DISCARD** | Core nuevo |
| `06147c7` vslice inventory/stash | **DISCARD** | Inventory = core (T1 CANDIDATE ARCHITECTURE, spike `358585f`) |
| `00797fe` / `635dc43` combat basics (base weapon/health/inventory) | **DISCARD** | Superseded por `WeaponContentHost` + core nuevo |
| `4442ed6` / `7c1ab19` HeldItem architecture / HeldItemController+MeleeWeapon | **DISCARD** | Reemplazado por content host + API de armas del core nuevo |
| `faaa241` / `bdac61b` USP weapon loops | **PORT DATA** (idents verificados w_usp/v_usp/ammo → ya en registry del pack) / DISCARD de código | Los idents cloud se reutilizan en `ub_weapon_usp` |
| `898978e` ResourceNode, LootRespawner, UltimoBarrioWeaponAdapter | **PORT LOGIC** (loot respawn/world nodes) | WeaponAdapter superseded por `IWeaponContentAdapter` |
| `49306c4` MissionSystem + UIShell | **DISCARD** + **PORT DATA** (quest chain) | Ver fila 49595a4 |
| `2e93338` / `c9e5664` inventory↔weapons wiring | **DISCARD** | Core |
| `38c85d2` engine inventory adoption | **DISCARD** | Core |
| `a00ed68` housing/inventory A01/A02 | **DISCARD** | Housing = core |
| `1a59b91` / `0987e5c` scene assembly | **DISCARD** | Escenas viejas |
| `3babf43`/`141630b`/`eb1a912`/`0322629` (merges alpha) | **DISCARD** | Artefactos de merge |

**Regla general:** el pack NO porta código de core viejo (InventoryComponent, PlayerInteractor, Housing, Persistence, Missions, Raid, Wallet). Solo porta: datos (definiciones/precios/quest data), patrones de lógica (AI states, loot respawn) y el contenido ya portable (prefabs/registries/hosts en `UltimoBarrio.Content.*`).

## 4. Orden de integración recomendado (actualizado con la suite validada)

1. **Docs** (commits 1-2, 9-13): research + manifest + spike + este plan.
2. **Contratos**: `IDamageTarget` (+`ContentDamageEvent`), `IWeaponContentAdapter`, `IEnemyContentAdapter`, `IFortificationContentAdapter` — la frontera con el core.
3. **Gobernanza**: merge manual de `asset-registry.yml`, `THIRD_PARTY_NOTICES.md`, `STATE.md` (conservar ambos historiales).
4. **PackageReferences en el sbproj del core** (9 idents cloud, commit 17) — ANTES de los prefabs para que el cloud monte solo.
5. **Definiciones + registries + prefabs** (commits 1-3, 8, 14, 16, 17).
6. **Lab scenes + generador** (commits 4-7).
7. **Hosts adaptados** al core nuevo: `WeaponContentHost` (equip/fire/reload vía la API de armas del core; mapear `ub_weapon_*` → ItemRegistry), `EnemyContentHost` (raids del core manda), `FortificationContentHost` (ownership del core).
8. **Dev rig** (`weapontestrig`, `labdamagedummy`, `lab_player_official`) — SOLO dev, último.
9. **Validación en el sobremesa**: compile → Play `weapon_lab` → suite 4/4 PASS → evidence → commit (bloques pequeños).

## 5. Conflictos esperados (resumen)

| Área | Conflicto | Resolución |
|---|---|---|
| Items | `weapon_usp`/`ammo_9mm` ya existen en el `ItemRegistry` del core; `ammo_buckshot`/`ammo_12g` son ids del core nuevo | Unificar IDs en el registry; prefabs referencian por string |
| sbproj | `PackageReferences` (9 cloud) vs lista del core nuevo | **Merge manual** de la lista, no overwrite |
| NPC | El core ya tiene `SaqueadorBrain`/raids | `EnemyContentHost` = variante portable; el raid del core manda |
| Inventory | El core vive sobre `InventoryComponent`; T1 = CANDIDATE ARCHITECTURE (spike) | No migrar hasta el spike; content packs no tocan inventario |
| Ownership | El core implementa housing ownership | `FortificationContentHost` delega a la interfaz del core (TODO en código) |
| Gobernanza | `asset-registry.yml` + notices + STATE.md divergen | Merge manual conservando ambos historiales |
| Casing | Índice en minúsculas (`core.ignorecase=true`) | Normalizar al portar; C# no depende del filename |
| Cloud literals | `Cloud.Model()` exige string literal (SB2000) | Mapeo ident→literal en `ResolveCloudModel()`; NO mover a datos |

## 6. Verificación de portado

- `git cherry-pick` por commit en orden de la tabla; resolver conflictos con la sección 5.
- Tras cada cherry-pick: compile en el editor del sobremesa (0 errores propios).
- Suite de armas: Play `weapon_lab` → 4/4 PASS (delta ≥ ExpectedDamage; rig no falsifica PASS).
- Sustituir PENDING_VERIFY (enemigos: modelo Citizen ⚠️ EULA T5) solo con assets vistos en Cloud Browser.
- Marcar en `asset-registry.yml` el identificador real de cada asset verificado.
