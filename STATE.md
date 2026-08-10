## Estado Actual: Barrio 01 Jugable — Integration Fixes Applied

- **Rama Actual**: `spike/laptop-content-stack` (pushed a GitHub).
- **Último Commit**: `8366498 fix(trader): use Components.Get<T>() instead of GetComponent<T>()` (2026-08-10).
- **Commit anterior**: `1c60dc3 fix(integration): wire all gameplay systems for playable barrio_01` (2026-08-10).
- **Errores de Compilación**: 0 (esperado — sin editor para verificar, pero los cambios son solo correcciones de API y wiring de componentes).
- **StartupScene**: `scenes/barrio_01.scene` (configurado en sbproj).

### BARRIO 01 — SISTEMAS JUGABLES

| Sistema | Estado | Notas |
|---|---|---|
| PlayerController | ✅ | Prefab oficial del engine + UbWeaponCarrier + HealthComponent + InventoryComponent + PlayerHud + PlayerInteractor + PlayerContentDamageBridge |
| Armas (4) | ✅ | USP, Escopeta, Palanca, Cuchillo — equipar con slots 1-6, disparar/recargar/soltar via WeaponContentHost |
| Enemigos (3) | ✅ | Saqueador, Bruto, Merodeador — prefabs con NavMeshAgent + EnemyPerception + EnemyAttack + EnemyContentHost |
| Spawner nocturno | ✅ | NightEnemySpawner activa saqueadores en fase Night, los retira en Day |
| Ciclo día/noche | ✅ | WorldClock (Day 5m → Prep 1m → Night 3m → Aftermath 30s) + WorldTimeLighting ajusta luces |
| Loot físico | ✅ | LootPickupContent (recoger con E) + ResourceNode (recolección con respawn) |
| Apartamentos | ✅ | 5 apartamentos reclamables con stash + ApartmentClaimInteractable |
| Comerciante | ✅ | Trader NPC — comprar/vender con Wallet + InventoryComponent |
| Inventario/HUD | ✅ | HotbarPanel con fallback a UbWeaponCarrier, InventoryUI, HudOverlayPanel |
| Fortificación | ✅ | 9 objetos (barricada madera/reforzada, puertas, stash, workbench, etc.) |
| Audio | ✅ | Banco de sonidos MIT (15 SoundEvents + WAVs importados) |
| Raid Manager | ✅ | Clock auto-detect, soporte EnemyContentHost para looters |

### FIXES APLICADOS (2026-08-10)

1. **HotbarPanel**: fallback a `UbWeaponCarrier` cuando `HeldItemController` es null (el player usa el sistema nuevo, no el viejo).
2. **Prefabs de enemigos**: añadidos `NavMeshAgent` + `EnemyPerception` + `EnemyAttack` (faltaban en los 3 prefabs — `EnemyContentHost` los requiere).
3. **RaidManager**: auto-busca `WorldClock` como los demás sistemas; soporta tanto `SaqueadorBrain` (viejo) como `EnemyContentHost` (nuevo) para looter targets.
4. **UbPlayerFix**: eliminado log spam por frame (spameaba console cada frame).
5. **InventoryComponent.RequestDrop**: añadidos `weapon_crowbar` y `weapon_shotgun` al mapa de prefab paths.
6. **Trader**: `GetComponent<T>()` → `Components.Get<T>()` (API correcta de s&box).
7. **PlayerHud**: comentario aclarador de que `HeldItemCtrl` puede ser null.

### ARQUITECTURA

- **Content pack portable** (`UltimoBarrio.Content.*`): weapons, enemies, fortification, audio — sin tocar core viejo.
- **Adaptadores**: `IDamageTarget`, `IWeaponContentAdapter`, `IEnemyContentAdapter`, `IFortificationContentAdapter` — punto de unión con core nuevo.
- **Gameplay layer**: `UbWeaponCarrier` (hotbar → content pack), `PlayerContentDamageBridge` (player = IDamageTarget), `NightEnemySpawner` (lifecycle enemigos), `WorldTimeLighting` (luz por fase).
- **Escena barrio_01**: mapa `facepunch.flatgrass`, 5 apartamentos, trader, 3 armas pickup, 3+ loot nodes, 3 enemy spawns, NavMesh ON, WorldClock 5min día.

### VALIDACIÓN PENDIENTE

- **Compilación**: abrir el editor s&box y verificar 0 errores (no hay .NET SDK local).
- **Runtime**: Play `barrio_01.scene` → verificar spawn del player, armas equipables, enemigos aparecen de noche, loot recolectable, ciclo día/noche visible, trader funcional.
- **Multiplayer**: verificar spawn de 2+ jugadores, networking de daño/loot/inventario.

### WEAPON LAB SUITE 4/4 PASS (editor 26.08.05, play local, 2026-08-08, prefabs definitivos)

```
[WeaponLab] USP PASS      (w_usp/v_usp, delta 15 x4, ammo/reload/drop)
[WeaponLab] Crowbar PASS  (crowbar01 engine, delta 35 x2)
[WeaponLab] Knife PASS    (w_trenchknife/v_m9bayonet cloud, delta 20 x2)
[WeaponLab] Shotgun PASS  (w_spaghellim4/v_spaghellim4 cloud, delta 96 = 8 pellets x 12)
[WeaponLab] Suite complete (4/4 PASS)
```

- Daño por ruta real `Fire → PerformTrace (Scene.Camera) → IDamageTarget`; rig solo sustituye input humano.
- Rig data-driven rig-6: `List<WeaponTestEntry>` (Label/WeaponId/TestType/WorldPrefab/ViewPrefab/TargetDistance/ExpectedDamage/ClipSize/UsesAmmo). PASS = delta >= ExpectedDamage (tolera spread de pellets).
- Cloud portability: `PackageReferences` en sbproj (List<string> de idents, formato confirmado en codigo del engine); montaje automatico al abrir proyecto; cold restart PASS (2026-08-07).
- Prefabs del pack con modelos reales como fallback autocontenido (w_knife=w_trenchknife, w_shotgun/v_shotgun=spaghellim4, v_knife=crowbar01).

### USP RUNTIME VALIDATED (editor 26.08.05, play local, 2026-08-07)

```
[LabBuild] VERSION=rig-2                      ← hotload verificado sin esperas
[WeaponLab] USP asset world OK / view OK
[WeaponLab] Equipped ammo=12
[WeaponLab] Fired ammo=12→11 / 11→10 / 10→9   ← Trace hit=TargetDummy, Damage 100→85→70→55 (15/impacto)
[WeaponLab] Reloaded                          ← ammo 12
[WeaponLab] Fired ammo=12→11                  ← Damage 55→40
[WeaponLab] Dropped
[WeaponLab] PASS
```

El daño recorre el camino real: `WeaponContentHost.Fire → PerformTrace (Scene.Camera) → IDamageTarget → damage`. El rig solo lee la salud del dummy (no falsifica PASS).

### Cloud assets (dependencia persistente resuelta)

- `Cloud.Model("facepunch.w_usp")` / `Cloud.Model("facepunch.v_usp")` — API oficial, exige **string literal** en el call site (constraint SB2000 del CloudAssetProvider): mapeo ident→literal en `ResolveCloudModel()` (host) y `ResolveViewCloudModel()` (rig).
- La ruta montada (`models/weapons/sbox_pistol_usp/...`) queda solo como fallback.
- `Assets/asset-registry.yml` actualizado con cloud ident / asset type / source / license / uso / estado verificado.
- Pendiente de confirmar: abrir proyecto nuevo sin install manual → USP resuelve (el camino Cloud.Model no depende del montaje, pero la primera apertura tras borrar caché lo demostrará).

### Lección del "wait 60 seconds"

NO es una regla del engine: era leer `compile_status` con un `Success` del build anterior mientras `IsBuilding=true`. Proceso correcto: guardar → esperar `IsBuilding=false` + `Success=true` (hotload en segundos) → play_start → verificar `[LabBuild] VERSION`. Confirmado: rig-1→rig-2 con play inmediato y marcador correcto.

### Evidencia visual

- `.openclaw/tmp/lab_rig_camera.png` — cámara del rig (viewmodel + TargetDummy en línea de fuego).
- `.openclaw/tmp/lab_rig_world.png` — vista del editor (rig + worldmodel + dummy).
- Vision model caído (400) → capturas sin analizar; revisión manual al final de labs.

### Archivos experimentales descartados (preservados en checkpoint)

- `checkpoint/laptop-weapon-lab-frankenplayer` + `frankenplayer-experiment.patch` (fuera del repo) conservan LabPlayerController, lab_player/lab_player2, LabWeaponSpawner. No portados al content pack.

### Siguientes Pasos (TURBO MODE)

1. Workers en paralelo: enemies (A/B), building (C/D), vehicles (E), audio (F), qa (G), portability (H) — worktrees wt-* ramas agent/*.
2. Pipeline del coordinador: Saqueador → wooden barricade → vehicle foundation → Bruto+Merodeador → fortification pack → audio → combined suite.
3. Prueba manual única al final de todos los labs (PlayerController oficial).

### HANDOFF 2026-08-08 (fábrica congelada — ver docs/handoff/2026-08-08-laptop-handoff.md)

- HEAD estable: `dfdf543` (spike/laptop-content-stack). Compile 0 errores. Pendiente de push a GitHub.
- Integrado y compilando: QA suite (G), building assets+system (D+C, building_lab runtime FAIL), enemies assets (B), audio (F, sin compilar).
- WeaponSuite 4/4 PASS validada tras refactor de G (14:42).
- WIP preservados: `agent/enemies` @ `d4c0c93` (sistema de enemigos de A, sin validar), `checkpoint/laptop-turbo-wip-20260808` @ `e871fe7` (diagnóstico NRE building + tool MCP open_scene).
- `agent/vehicles` sin trabajo (worker E no entregó).
- NRE actual: `BuildingTestRig.CheckRegistryCoverage` línea ~104 — `FortificationContentRegistry.All` contiene un null; diagnóstico en curso (ver handoff → Known blockers).

### BUILDING SPIKE — agent/building (Worker C)

- **Rama**: `agent/building`. Base HEAD validado `9fd7b32`; el coordinador avanzó con
  `6c19f14` (prefabs con modelos reales) y `476e4bf` (asset-registry building) antes
  del spike — se trabajó sobre `476e4bf`.
- **Sistema portable data-driven** (sin propiedad/apartamentos ni inventario canónico):
  - `BuildDefinition` (id/displayName/category/prefab/maxHp/repairAmount/repairCost/upgradeTo/model+fallback).
  - `BuildPlacementRules` (server): rango builder, solapamiento, ground check, volumen
    de prueba — trazas reales del scene.
  - `BuildStructureHost`: autoridad host — spawn (prefab), HP (IDamageTarget), damage,
    repair (consumo vía delegado → `LabResourceFixture` del lab, NO InventoryComponent),
    upgrade (cambio de definición: modelo + HP), destroy.
  - `FortificationContentRegistry`: 9 objetos como DATA (wooden_barricade primero;
    reinforced_barricade, doors, stash, workbench, generator, alarm, repair_station)
    con los modelos reales del engine verificados en asset-registry.
  - Rig: `BuildingTestRig` (UltimoBarrio.Content.Dev) + `building_lab.scene`
    (NetworkHelper StartServer SIN PlayerPrefab): preview inválido REJECTED →
    bloqueado REJECTED → válido ACCEPTED → rotación → spawn → HP → daño (trace real)
    → repair (consumo fixture) → upgrade (modelo reforzado) → destroy → PASS.
    Logs `[BuildingLab]` + `[LabBuild] VERSION=rig-1`.
- **Commits** (agent/building): `ffa08bd` data layer · `cc249e9` rules+host+retype ·
  `63bc700` rig+scene · docs STATE.
- **Nota cross-domain**: `enemy_lab.scene` (dominio enemigos) referenciaba
  `FortificationContentHost` como dummy objetivo → `__type` actualizado a
  `BuildStructureHost` (cambio mecánico de 1 línea, requerido por el rename).

### VEHICLES SPIKE - agent/vehicles-v2 (Worker D)

- **Rama**: `agent/vehicles-v2`. Base HEAD `46b3b01` (spike/laptop-content-stack).
- **PRIMARY kit**: `fieldguide.vehiclephysics` (Vehicle Physics Kit, sbox Field Guide) -
  **MIT verificado** (get_package MCP 2026-08-08: "MIT licensed. Fork it, ship it, no
  strings."; sboxdb v337869, updated 2026-07-31). Ident exacto verificado por
  find_packages. Decision detallada: `docs/research/vehicles-primary-kit.md`.
- **Correccion al research**: `fieldguide.vehicle_prototyping` YA NO EXISTE en el
  backend (find_packages = 0); el montaje (VehicleFactory) vive en el propio kit.
- **Descartados**: matekdev/sbox-arcade-car-physics (API antigua verificada en source:
  SceneObject(SceneWorld), Input.AnalogMove - no compila en 26.08.05, aunque MIT);
  clearly.cavc (2025-03-08, licencia sin verificar); bugge.vehicle_controller (sin
  licencia verificada).
- **Entregado** (STATIC ONLY, pendiente de validacion runtime del coordinador):
  - `VehicleTestRig` (UltimoBarrio.Content.Dev): spawn (prefab del kit por propiedad
    VehiclePrefabPath) → components validos (Rigidbody+ModelRenderer+listado real de
    componentes) → driver fixture entra (attach via GameObject.Parent) → throttle
    (delta de posicion real) → steering (cambio de yaw real) → brake (reduccion de
    velocidad por ventanas de posicion) → reverse (signo dot(Δ,forward) < 0) → exit
    (detach) → `[VehicleLab] Suite complete (8/8 PASS)`.
  - `VehicleDriverFixture` (Dev): enter/exit por ruta real de parenteo; sin fisica propia.
  - `VehicleSuite`: reporter ILabSuite integrado con el runner QA ([UBSuite]).
  - `vehicle_lab.scene` actualizada: NetworkHelper StartServer SIN PlayerPrefab
    (autotest, sin pawn) + rig con CameraComponent (IsMainCamera, Priority 10).
  - `ultimo_barrio.sbproj`: PackageReferences += "fieldguide.vehiclephysics".
  - `assets/asset-registry.yml`: entrada fieldguide/vehiclephysics VERIFIED (MIT).
  - Logs: `[LabBuild] VERSION=rig-1` + `[VehicleLab]`.
- **Anti-falsificacion**: PASS solo por deltas reales (posicion/yaw/velocidad);
  input simulado con `Input.SetAction(string,bool)` (API engine verificada 26.08.05);
  NOMBRES de input actions del kit = propiedades configurables en la escena (defaults
  Forward/Brake/SteerLeft/SteerRight/Reverse), pendientes de confirmar contra el
  README del kit tras montarlo.
- **Pendiente coordinador (runtime)**: merge sbproj → montar paquete → confirmar
  nombres de input actions en el README del kit → rellenar VehiclePrefabPath con un
  prefab del kit → Play vehicle_lab → 8/8 PASS.
