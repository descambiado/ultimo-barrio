# Content Integration Manifest — nodo portátil (spike/laptop-content-stack)

**Fecha:** 2026-08-07
**Rama:** `spike/laptop-content-stack` (base: `feat/holy-grail-foundation` @ c9e5664 — **base antigua**)
**Propósito:** decidir el stack de contenido definitivo de Último Barrio. Todo lo que se elija aquí debe ser **portable por cherry-pick / copia selectiva** sobre `integration/wizard-holy-grail` cuando se publique. Nada de este documento asume que el STATE.md remoto refleje el estado real del juego.
**Fuentes:** 7 informes de investigación en `docs/research/workers/` (armas, NPC/AI, building, inventario/UI, vehículos, mundo/assets, audio) — ~110 búsquedas. Cada candidato tiene su ficha completa (revisión, licencias, dependencias, API generation) en esos archivos; aquí solo las decisiones.

**Correcciones de investigación (2026-08-07):** tres candidatos marcados DISCARD por "no verificables" se han reinvestigado contra la API de GitHub (URL exacta, rama por defecto, último push, LICENSE, árbol de archivos). Resultado: `echohello-dev/basebound` = **ADAPT (referencia), MIT**; `Softsplit/sandbox-plus-plus` = **EXTRACT PATTERN, GPL-3.0** (el "Sandbox++" real); `Small-Fish-Dev/In-This-House` = **EXTRACT PATTERN, MIT, API legacy**. Celdas corregidas en §B y §C.

---

## §0 — Decisiones transversales (afectan a todo el stack)

| # | Decisión | Evidencia / fuente | Implicación |
|---|---|---|---|
| T1 | **La API de inventario/arma/municiones de primera parte (update 26.07.08) es el cimiento del inventario nuevo.** No reimplementar sincronización de slots. | [inventory-ui.md](workers/inventory-ui.md) §1 — sbox.game/news/update-26-07-08 | **CANDIDATE ARCHITECTURE — REQUIRES MIGRATION SPIKE:** `Sandbox.Inventory` / `BaseInventoryItem` (update 26.07.08) es candidata a sustituir al `InventoryComponent` de la rama, pero la migración NO está decidida: requiere spike de comparación (`docs/research/native-inventory-migration-spike.md`) antes de tocar pickup/stack/drop/stash/crafting/persistencia/housing del core. **No tocar el inventario viejo en este nodo.** |
| T2 | **El gamemode Sandbox de Facepunch es open source (2026-04-08) y el motor es MIT (2025-11-26).** Es la referencia de código oficial: WeaponBase, Npc, Prop, Mapping.Door, Toolgun. | [weapons.md](workers/weapons.md) §3, [npc-ai.md](workers/npc-ai.md) §1, [building.md](workers/building.md) §1 | Antes de escribir cualquier sistema, consultar el código oficial. Clonar `Facepunch/sandbox` y `Facepunch/sbox-public`. |
| T3 | **Colecciones oficiales de assets Facepunch = fuente primaria de contenido** (uso libre declarado en proyectos s&box): `facepunch.sboxassets` (props urbanos), `facepunch.sboxweapons` (armas+attachments), `facepunch.v_first_person_arms_citizen` (brazos FP), Citizen Characters (rig humano + 73 clothing), `facepunch.door_single_dev`. | [world-content.md](workers/world-content.md) §0 | Cero fricción legal para el contenido del barrio. Registrar cada paquete en `Assets/asset-registry.yml`. |
| T4 | **asset.party/sbox.game NO estandariza licencias** (issue sbox-public #5130). Solo usar packs con licencia explícita (CC0/CC-BY) o autorización escrita. | [world-content.md](workers/world-content.md) §0.6 | Regla dura: un pack sin licencia clara = DISCARD. |
| T5 | **Riesgo legal del modelo Citizen en publicación standalone** (restricción reportada por la comunidad, ⚠️ sin verificar en EULA): re-vestir/re-texturizar o sustituir antes de exportar fuera de s&box. | [npc-ai.md](workers/npc-ai.md) §10, [weapons.md](workers/weapons.md) §11 | Plan B de enemigos/civiles: modelo alternativo CC0 con rig compatible. |
| T6 | **Assets del engine no son todos libres**: algunos core cloud assets son no-comerciales (issue sbox-public #11211). Auditar antes de publicar. | [building.md](workers/building.md) notas transversales | Revisar licencias de cada asset del engine que se referencie. |
| T7 | **No redistribuir** modelos de Quake/GoldSrc/HL, ni assets de mapas (`thieves.rpdowntown3t` es propiedad de Three Thieves; usar solo como dependencia de mapa). | [world-content.md](workers/world-content.md) §7, §0.8 | Lo externo se consume como dependencia de paquete; nunca se copia al repo. |
| T8 | **Sistemas de nicho sin referente público** (cerraduras, reparación, maletero, daño de vehículo, grid de construcción): implementación propia sobre componentes oficiales. | [building.md](workers/building.md) notas, [vehicles.md](workers/vehicles.md) huecos | Son oportunidad de diseño propio, no dependencias. |

---

## §A — WEAPONS

### Candidatos (ficha completa en [weapons.md](workers/weapons.md))

| Candidato | Verdict | Confianza |
|---|---|---|
| facepunch/sboxweapons (assets oficiales) | ASSETS | ALTA |
| First-Person Weapons doc + brazos FP oficiales (animgraph "punching", ADS, recoil) | PATTERN + ASSETS | ALTA |
| WeaponBase del Sandbox oficial de Facepunch (w_usp, w_mp5, shotgun) | ADAPT | ALTA |
| timmybo5/simple-weapon-base (SWB) + swb-skeleton | ADOPT (verificar licencias) | ALTA |
| Ryhon0/RWB | ADAPT | MEDIA |
| TTT Reborn (referencia integración SWB) | ADAPT | MEDIA |
| omniparadigm/weapons | **DISCARD** (no existe públicamente verificable) | BAJA |
| Modelos: USP ✅ oficial (`sbox_pistol_usp`); shotgun ⚠️ (oficial en sboxweapons, confirmar); crowbar/knife ⚠️ (no confirmados oficiales; CC0 itch/sketchfab a verificar) | ASSETS (con verificación) | ALTA/MEDIA |

### Decisión

- **PRIMARY:** API de armas de primera parte (T1) + assets `facepunch.sboxweapons` (worldmodels USP/shotgun) + brazos FP oficiales (viewmodels). Código propio de gameplay detrás de `IWeaponContentAdapter` (`Code/UltimoBarrio/Content/Weapons/`).
- **ALTERNATIVA 1:** SWB (timmybo5) como base de comportamiento (semi/full-auto, shotgun, melee, ADS, recoil) — solo tras verificar licencias de código y de sus modelos incluidos.
- **ALTERNATIVA 2:** WeaponBase oficial del Sandbox adaptada (cero dependencias comunitarias; más trabajo de melee/ADS).
- **ALTERNATIVA 3 (ASSETS-only):** stack oficial Facepunch + melee CC0 verificado (crowbar: VOiD1 Gaming/itch ⚠️; knife: PolyOne/sketchfab ⚠️) + SFX CC0; gameplay a medida.
- **REJECT:** omniparadigm/weapons (no localizable), frameworks legacy.

**Qué portar:** `Code/UltimoBarrio/Content/Weapons/*` (definiciones + host + adapter) y `Assets/prefabs/content/weapons/*` — ya creados en esta rama.

---

## §B — NPCs / ENEMIGOS

### Candidatos (ficha completa en [npc-ai.md](workers/npc-ai.md))

| Candidato | Verdict | Confianza |
|---|---|---|
| Sandbox oficial de Facepunch (Npc.cs, Npc.Disposition.cs, spawn, ragdoll) | ADOPT | ALTA |
| NavMeshAgent (motor) | ADOPT | ALTA |
| AnimationGraph / Animgraph (motor) | ADOPT | ALTA |
| Ragdoll component (motor, WIP) + ModelPhysics | ADOPT (vigilar WIP) | ALTA |
| Citizen Characters (rig humano + animaciones) | ASSETS | ALTA |
| SbokuBot (framework NPCs armados — Merodeador) | ADAPT | MEDIA |
| AI Unit Control (percepción LOS + decisión + persistencia) | ADAPT | MEDIA |
| Shrimple Ragdolls (modos de ragdoll, reserva) | ADAPT | MEDIA |
| NPC Zombie / Zombie Horde (Gvarados), Cut Them Down, Zombie Mod | PATTERN (diseño horda/melee) | MEDIA |
| In This House (`Small-Fish-Dev/In-This-House`) | **EXTRACT PATTERN** — MIT, branch `main`, último push 2025-02-24. API **legacy** (jam 2023, era entity pre-scene). Archivos útiles: `Entities/NPC/NPC.Navigation.cs` (A* sobre grid propio con retrace y LOS directa), `Entities/NPC/NPC.Controller.cs`, `Entities/Loot/` (LootSpawner, LootPresets, LootRarity, LootContainer), `Components/LockedComponent.cs`, `Entities/Door.cs`. Patrones transferibles (pathfinding por celdas, loot data-driven, puerta con candado); el código NO se porta (API antigua). | MEDIA-ALTA |

### Decisión

- **PRIMARY:** framework propio de enemigos sobre componentes oficiales: `NavMeshAgent` (locomoción) + `AnimationGraph` (blend + ataque melee + hurt/death) + `Ragdoll`/`ModelPhysics` (muerte) + modelo Citizen re-vestido (Saqueador/Bruto/Merodeador = variantes de materiales/escala). Código de referencia: Npc.cs del Sandbox oficial.
- **ALTERNATIVA 1:** SbokuBot para el Merodeador armado (percepción/combate a distancia).
- **ALTERNATIVA 2:** AI Unit Control como cerebro de estados (patrullar/perseguir/atacar/retirarse) si la nuestra se queda corta.
- **ALTERNATIVA 3:** Shrimple Ragdolls si el Ragdoll oficial WIP falla en la build objetivo.
- **REJECT:** packs sin licencia declarada. ~~In This House~~ → corregido arriba: MIT, EXTRACT PATTERN (A* grid propio + loot system).

**Qué portar:** `Code/UltimoBarrio/Content/Enemies/*` (arquetipos + loot tables + host con NavMeshAgent) y `Assets/prefabs/content/enemies/*` — ya creados. `IEnemyContentAdapter` es la frontera con el core nuevo.

---

## §C — BUILDING / FORTIFICACIÓN

### Candidatos (ficha completa en [building.md](workers/building.md))

| Candidato | Verdict | Confianza |
|---|---|---|
| Sandbox oficial: `Sandbox.Prop` (salud/destructible por ModelDoc) + `Sandbox.Mapping.Door` (puerta animada con curva) | ADOPT | ALTA |
| Nebual/sandbox-plus (omni-tool de constraints: weld/axis/rope/elastic/slider/ballsocket) | ADAPT | ALTA |
| apetavern/fortwars-entity (build wheel, fases build/combat, límite de bloques) | ADAPT (diseño) | MEDIA-ALTA |
| Fortwars Assets (kit modular Wood/Steel 1x1/1x2/1x1x1 — espec de grid) | ASSETS (si licencia) / PATTERN (espec) | MEDIA |
| BaseWars (economía recursos → muros/torretas) | PATTERN (diseño) | MEDIA |
| wiremod/wirebox + WireLib (electricidad/lógica) | PATTERN (solo si hace falta electricidad) | MEDIA |
| Nolankicks/Fortwars, themasterminds/sbox-fortwars | PATTERN (legacy) | BAJA |
| Basebound (`echohello-dev/basebound`) | **ADAPT (referencia)** — MIT, branch `main`, push 2026-07-24 (último commit: "chore: add license"). Plantilla de gamemode sobre API scene ACTUAL: `Code/Player.cs`, `Code/MyComponent.cs`, `Assets/scenes/minimal.scene`, `Docs/{architecture,networking,gameplay,setup}.md` y **skills de agente** (`.github/skills/`: sbox-gamemode-dev, sbox-triggers-collisions, sbox-ui-razor). Valor como scaffold y workflow agentizado; código de gameplay mínimo (5 .cs). | MEDIA-ALTA |
| Softsplit/sandbox-plus-plus (el "Sandbox++" real, sucesor comunitario de GMod Sandbox) | **EXTRACT PATTERN** — **GPL-3.0** (copyleft: NO compatible con MPL-2.0 del repo → no copiar código), branch `main`, push 2026-06-28, 2.620 entradas, API scene ACTUAL. Arquitectura de referencia: `Code/Game/Weapon/` (BaseWeapon, BaseBulletWeapon, BaseCarryable con ViewModel/WorldModel, MeleeWeapon, IronSightsWeapon, AmmoInventory/AmmoResource), `Code/Components/Ownable.cs`, `IPhysgunEvent/IToolgunEvent`, `Code/GameLoop/LimitsSystem.cs`, prefabs de entidades. Distinto de Nebual/sandbox-plus (que sigue como ADAPT). | ALTA |

### Decisión

- **PRIMARY:** cimiento oficial (`Sandbox.Prop` salud/destructibles + `Sandbox.Mapping.Door`) + componentes propios encima: placement preview/ghost, snapping por grid propio (espec 1x1/1x2/1x1x1), cerradura (`Lock`), reparación (estados visuales + coste de recursos), upgrade (madera→reforzada, ya implementado en `FortificationContentHost.Upgrade()`).
- **ALTERNATIVA 1:** sandbox-plus para constraints físicos (barricadas atornilladas, bisagras) — portar al API actual.
- **ALTERNATIVA 2:** patrón build wheel de Fortwars para el menú de construcción.
- **REJECT:** ports legacy abandonados. ~~Basebound / Sandbox++~~ → corregidos arriba: Basebound = ADAPT referencia (MIT); Softsplit/sandbox-plus-plus = EXTRACT PATTERN (GPL-3.0, solo estudiar arquitectura, sin importar su gamemode).

**Qué portar:** `Code/UltimoBarrio/Content/Fortification/*` (definiciones de 9 objetos + host con salud/reparación/upgrade) y `Assets/prefabs/content/fortification/*` — ya creados.

---

## §D — HOUSING ASSETS (muebles, puertas, barricadas, alijo, banco, generador, alarma)

Fuente: [world-content.md](workers/world-content.md) §1-2, §6.

| Bloque | PRINCIPAL | Alternativas | Confianza |
|---|---|---|---|
| Muebles | `facepunch.sboxassets` + org CC0 `sbox.game/asset` | Kenney Furniture Kit (CC0, import) | ALTA |
| Puertas | `facepunch.door_single_dev` + prefab propio (tutorial oficial "Creating a Door") | propkits de `facepunch.sandbox` | ALTA |
| Barricadas | `facepunch.sboxassets` (road barriers, pallets, oil drum) | bloques Fortwars Wood/Steel (si licencia) | ALTA |
| Alijo (stash) | props de `facepunch.sboxassets` (storage box) — hoy `cardboard_box_open` (verificado) | org CC0 | ALTA |
| Banco de trabajo | org CC0 `sbox.game/asset` / asset.party (verificación manual) ⚠️ | import CC0 (Kenney/Quaternius) | MEDIA ⚠️ |
| Generador | prop importado CC0 ⚠️ + prefab con emisivo/sonido | `facepunch.sboxassets` (verificar si hay industrial) | BAJA ⚠️ |
| Alarma | prop pequeño CC0 ⚠️ + sonido sirenas (§I) | — | BAJA ⚠️ |

**Regla:** el contenido del pack de fortificación de esta rama usa fallbacks verificados; los candidatos ⚠️ se confirman con Cloud Browser y se actualizan `AssetsVerified` en los registros.

---

## §E — LOOT (chatarra, recursos, ítems)

Fuente: [world-content.md](workers/world-content.md) §3.

- **PRIMARY:** `facepunch.sboxassets` (oil drum, metal wheely bin, storage box, pallets, trench) como loot visual; pickups propios (ya existen `pf_scrap_*.prefab` en la rama).
- **ALTERNATIVAS:** org CC0 `sbox.game/asset`; Kenney Industrial Kit / Quaternius (CC0, import).
- **Loot tables** ya definidas en `EnemyContentRegistry` (chatarra/ammo/medicine) — los ItemId son strings opacos mapeados por el core nuevo.
- **REJECT:** "Fallout Style Junk" de lb3d.co (licencia sin verificar), props de mapas de terceros.

---

## §F — CRAFTING

Fuente: [inventory-ui.md](workers/inventory-ui.md) §2, [world-content.md](workers/world-content.md) §6.

- **PRIMARY:** sistema propio de crafting sobre la API de inventario de primera parte (T1), con estación `fort_workbench` como contenedor/ancla. Recetas data-driven (patrón ya iniciado: `ItemDefinition`/registros de contenido).
- **ALTERNATIVA:** Modular Inventory System (llad) — crafting + chests + loot tables "todo toggleable"; verificar licencia, mantenimiento y compatibilidad con la API first-party (posterior al paquete).
- **PATTERN:** UI de crafting con SUI Designer (ver §G); flujo atómico (validar recursos → consumir → producir) como el "crafting atómico" ya trabajado en wizard-holy-grail.
- **REJECT:** sistemas legacy de grid (kurozael/sbox-inventory) como código; solo patrón Tetris si el diseño lo pide.

---

## §G — UI / INVENTARIO

Fuente: [inventory-ui.md](workers/inventory-ui.md).

| Candidato | Verdict | Confianza |
|---|---|---|
| **API de inventario de primera parte (update 26.07.08)** | **ADOPT (núcleo)** | ALTA |
| SUI Designer (KiKoZl1, MIT; InventoryGrid/InventorySlot/Hotbar) | ADOPT (herramienta) | ALTA |
| VirtualGrid (render de grids grandes, oficial) | ADOPT | ALTA |
| UI/Inventory.razor de facepunch/sbdm (hotbar + slot switching) | PATTERN | ALTA |
| Modular Inventory System (llad) | ADAPT | MEDIA ⚠️ |
| kurozael/sbox-inventory (Tetris) | PATTERN (solo si grid Tarkov) | MEDIA ⚠️ |
| Razor Designer (xaz/sklmr) | ADAPT (alternativa a SUI) | BAJA-MEDIA |
| Sandbox.Services.Inventory (Steam) | DISCARD para gameplay | ALTA |

### Decisión

- **PRIMARY:** inventario de primera parte (host-authoritative, items como child GameObjects) + UI Razor propia diseñada con **SUI Designer** (MIT) + **VirtualGrid** para grids grandes + patrón hotbar de sbdm.
- **ALTERNATIVA:** Modular Inventory System (llad) si se quiere crafting/chests/loot out-of-the-box.
- **REJECT:** inventario de Steam como gameplay; grids legacy como código.

---

## §H — VEHÍCULOS

Fuente: [vehicles.md](workers/vehicles.md).

| Candidato | Verdict | Confianza |
|---|---|---|
| Vehicle Physics Kit (Field Guide; ruedas raycast, slip-curve, drivetrain, chase cam) | ADOPT (física) | ALTA |
| Vehicle Prototyping (Field Guide; v313827, 2026-07-22; VehicleFactory/PartKitFactory) | ADOPT (montaje) | ALTA |
| CAVC — Clearly A Vehicle Controller (framework + coche demo) | ADAPT (verificar enter/exit + licencia) | MEDIA ⚠️ |
| matekdev/sbox-arcade-car-physics | ADAPT (feeling arcade) | MEDIA |
| ZCars (FosterZ) | PATTERN (física realista, monitorizar) | MEDIA |
| sbox-community/sbox-togg-sedan-car-addon | ASSETS/referencia (cuidado uso comercial del modelo) | MEDIA |
| API oficial: `Sandbox.DamageInfo` (daño), Storage UGC (persistencia maletero) | PATTERN | ALTA |

### Decisión

- **PRIMARY:** Vehicle Physics Kit + Vehicle Prototyping (física + montaje). **Enter/exit** parcial (asiento de conductor) → completar con componente propio; **maletero** y **daño** = implementación propia (Storage UGC + `Sandbox.DamageInfo`), huecos confirmados del ecosistema.
- **ALTERNATIVA:** CAVC si su framework de asientos/enter-exit es genérico y la licencia lo permite.
- **REJECT:** nada de esto entra en esta rama todavía — `LabVehicleSpawner` queda como stub hasta decidir paquete con licencia verificada.

---

## §I — AUDIO

Fuente: [sound.md](workers/sound.md). Integración: SoundEvent (.sound) + `Sound.Play` + `Sandbox.Soundscape` para ambiente.

| Categoría | PRINCIPAL | Alternativas | Confianza |
|---|---|---|---|
| Disparos | The Free Firearm Sound Library (OGA, **CC0**) | Sonniss GDC; Freesound CC0 | ALTA |
| Recarga | Sonniss GDC (royalty-free juegos) | packs itch (⚠️ licencia por pack) | ALTA |
| Pasos | 100 CC0 SFX #2 (OGA) + patrón "Source Footsteps" (por Surface) | Fantozzi's Footsteps | ALTA |
| Enemigos (voces/daño) | Sound Effects Pack (OGA, CC0) | Freesound CC0 | ALTA |
| Impactos | Kenney Impact Sounds (CC0) + Sonniss (por material) | 100 CC0 SFX #2 | ALTA |
| Construcción | Sonniss GDC (tools/wood) + 100 CC0 SFX #2 (obra) | Freesound CC0 | ALTA |
| Sirenas | Pixabay SFX (sin atribución) | Mixkit; Freesound CC0 | ALTA |
| Ambiente | Sonniss GDC (urban) + **Soundscape** | 100 CC0 SFX #2 | ALTA |

**Regla:** guardar copia de cada licencia en `docs/licenses/`; Sonniss no permite uso para IA/ML y no redistribuir wav sueltos; Pixabay no redistribuir sin procesar; Freesound filtrar CC0/CC-BY y guardar ficha para atribución.

---

## §J — ENVIRONMENT (vallas, luces, ambiente urbano)

Fuente: [world-content.md](workers/world-content.md) §6-7, [sound.md](workers/sound.md) §8.

- **Luces:** entidades nativas del motor (PointLight/SpotLight) — sin assets externos. **ADOPT** (ALTA).
- **Vallas/muros:** `facepunch.sboxassets` (road barrier, stone wall, stone brick, bitumen roof). Rejilla metálica: import CC0 si hace falta ⚠️. **ADOPT/ADAPT** (ALTA).
- **Ambiente sonoro:** Sonniss GDC (urban) + 100 CC0 SFX #2 + `Sandbox.Soundscape` por zona (día/noche). **ADOPT** (ALTA).
- **Mapa:** `thieves.rpdowntown3t` sigue como dependencia de mapa (no copiar sus assets). Alternativa de menor tamaño: `rpdowntown3tcompact` / `rpdowntown3tse`.
- **REJECT:** montar Quake/GoldSrc/NS2 y redistribuir sus props (licencias incompatibles).

---

## §K — Mapa de portabilidad hacia integration/wizard-holy-grail

Cuando se publique la rama canónica, de ESTA rama se portará **solo**:

1. `Code/UltimoBarrio/Content/` — contratos (`IDamageTarget`, `IWeaponContentAdapter`, `IEnemyContentAdapter`, `IFortificationContentAdapter`) + registros de definiciones + hosts autocontenidos. **Bridges al core nuevo marcados `TODO(core nuevo)`** (daño → contrato del core; inventario → API first-party T1; sonido → SoundEvents; animación → AnimationGraph).
2. `Assets/prefabs/content/` — 20 prefabs (armas, enemigos, fortificaciones) con fallbacks verificados.
3. `Assets/scenes/spikes/` — 4 labs (weapon/enemy/building/vehicle) para validación aislada.
4. `docs/research/` — este manifest + los 7 informes (decisión de stack).
5. `Assets/asset-registry.yml` + `THIRD_PARTY_NOTICES.md` — gobernanza de los nuevos candidatos.

**NO se portan**: Dev spawners tal cual (se reescriben contra el core nuevo si hace falta), ni ningún cambio a `main.scene`/`ultimo_barrio_alpha.scene` (no se tocaron), ni sistemas del core viejo (no se tocaron).

## §L — Pendientes de verificación (cuando el editor esté disponible)

1. Compilar `spike/laptop-content-stack` con 0 errores (atención a `Scene.GetAllComponents<T>()` en Dev y `NavMeshAgent`/`[RequireComponent]` en hosts).
2. Cargar las 4 escenas de spike sin errores de serialización.
3. **Cloud Browser:** verificar modelos primarios ⚠️ (USP/shotgun en sboxweapons — ruta canónica reportada `models/weapons/sbox_pistol_usp/w_usp.vmdl`; crowbar/knife CC0; citizen model + animgraph; door_single_dev; barricada; generador; alarma) y actualizar `AssetsVerified`.
4. Confirmar en editor: API de inventario de primera parte (T1), `Sandbox.Prop` health/breakable, `Sandbox.Mapping.Door`, componentes Ragdoll/ModelPhysics.
5. Verificar licencias: SWB (código + modelos), CAVC, llad Modular Inventory, packs de itch ⚠️; guardar licencias en `docs/licenses/`.
6. Confirmar restricción EULA del modelo Citizen para standalone (T5) y estado de los assets no-comerciales del engine (T6).
7. Crear los SoundEvents del pack (`sounds/weapons/*.sound` etc.) con las fuentes CC0/Sonniss elegidas.

## Fuentes

- [weapons.md](workers/weapons.md) — 14 búsquedas · [npc-ai.md](workers/npc-ai.md) — 12 · [building.md](workers/building.md) — 18 · [inventory-ui.md](workers/inventory-ui.md) — 15 · [vehicles.md](workers/vehicles.md) — 17 · [world-content.md](workers/world-content.md) — 15 · [sound.md](workers/sound.md) — 16.
- Convención: ⚠️ = dato no verificado por búsqueda (revisar en editor/Cloud Browser); ninguna revisión ni licencia se ha inventado.
