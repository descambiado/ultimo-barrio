## Estado Actual: Barrio 01 Jugable — Gameplay Loop Cerrado

- **Rama Actual**: `spike/laptop-content-stack` (pushed a GitHub).
- **Último Commit**: `c4f0762 feat(build+hud): bind build to F (Flashlight action), show consumable name in HUD` (2026-08-13).
- **Errores de Compilación**: 0 (esperado — sin editor para verificar, pero los cambios son wiring de componentes, fixes de data y APIs ya usadas en el repo).
- **StartupScene**: `scenes/barrio_01.scene` (configurado en sbproj).

### BARRIO 01 — SISTEMAS JUGABLES

| Sistema | Estado | Notas |
|---|---|---|
| PlayerController | ✅ | Prefab oficial del engine + UbWeaponCarrier + HealthComponent + InventoryComponent + PlayerHud + PlayerInteractor + PlayerContentDamageBridge + MissionJournal + PlayerDeathHandler + BuildController |
| Armas (4) | ✅ | USP, Escopeta, Palanca, Cuchillo — equipar con slots 1-6; **la recarga consume munición del inventario** (ammo_9mm / ammo_buckshot); melee sin munición |
| Consumibles | ✅ | Agua (+25 HP) y Medicina (+50 HP) seleccionables en hotbar; attack1 los usa; host-validado por RPC con id del emisor (multijugador-safe) |
| Enemigos (3) | ✅ | Saqueador, Bruto, Merodeador — prefabs con NavMeshAgent + EnemyPerception + EnemyAttack + EnemyContentHost; las armas de fuego reportan ruido (ReportNoise) |
| Spawner nocturno | ✅ | NightEnemySpawner activa saqueadores en fase Night, los retira en Day; NavMesh de escena Enabled+IncludeStaticBodies (los enemigos navegan el barrio) |
| Ciclo día/noche | ✅ | WorldClock (Day 5m → Prep 1m → Night 3m → Aftermath 30s) + WorldTimeLighting ajusta luces |
| Loot físico | ✅ | LootPickupContent (recoger con E) + ResourceNode (recolección con respawn) + 57 chatarra a nivel del suelo |
| Apartamentos | ✅ | **5 apartamentos (a01-a05) a escala real a nivel del suelo**: muros 128u, portal 96u con collider trigger (el jugador entra caminando), stash y spawn interior; claim por portal |
| Comerciante | ✅ | Kiosko en la plaza (0,0,0) — único trader; compra/venta con Wallet + InventoryComponent; vende agua, medicina, 9mm, cartuchos 12G, USP |
| Inventario/HUD | ✅ | HotbarPanel (fallback UbWeaponCarrier), InventoryUI, HudOverlayPanel con objetivos de misión, HUD muestra consumible activo |
| Crafting | ✅ | WorldContentBootstrap coloca la estación junto al kiosko; 5 recetas con ítems reales (ammo_9mm, water, medicine, weapon_knife, weapon_usp) |
| Fortificación | ✅ | BuildController con tecla F (acción Flashlight) + `ub_build`; barricada de madera con coste 20 chatarra; tags fortification+enemy_target |
| Misiones | ✅ | Cadena "Primeros Pasos" completa: 8 objetivos, HUD de objetivos, recompensa al completar ($100 + 20x ammo_9mm), ReturnHome al respawnear en casa |
| Muerte/respawn | ✅ | PlayerDeathHandler: 5s → respawn en apartamento propio (si tiene) o SpawnPoint; notifica ReturnHome |
| Persistencia | ✅ | Autosave cada 60s (claims + stashes + economía por identidad estable); restauración al reconectar (respawn en casa + balance) |
| Raid Manager | ✅ | Clock auto-detect, looters con EnemyContentHost apuntando al portal del apartamento |
| Audio | ✅ | Banco de sonidos MIT (15 SoundEvents + WAVs importados); validación runtime pendiente en editor |

### FIXES APLICADOS (sesión 2026-08-13)

1. **Escena barrio_01 reescrita**: todo alineado a Z=0 (suelo flatgrass). Antes el contenido jugable flotaba a Z 341-425, los spawns a Z 390 y el plano de colisión a Z 388 (el jugador spawneaba 25+ unidades sobre el barrio).
2. **Apartamentos a escala real**: colisión correcta (BoxCollider.Scale=64), front walls abiertas (±80) y portal con collider trigger para entrar caminando. Añadidos a03/a04/a05 (5 total).
3. **Trader duplicado eliminado** (estaba dentro del apartamento a01; también desambigua WorldContentBootstrap).
4. **NetworkHelper**: PlayerPrefab → prefabs/player.prefab (formato prefab ref); SpawnPoints a Z=0.
5. **Consumibles**: HealthComponent.Heal + uso por attack1 (agua/medicina) en UbWeaponCarrier.
6. **Munición real**: FinishReload consume ammo del inventario; ammo_buckshot añadido al registry y al trader.
7. **Misiones**: fix BuyItem "ammo"→"ammo_9mm", recompensa ammo→ammo_9mm, ReturnHome cableado al respawn, recompensas aplicadas en CompleteMission.
8. **Crafting**: recetas reescritas con ítems reales (antes producían medicina/repair_kit/barricade inexistentes).
9. **Persistencia**: autosave periódico + economía persistida por PlayerIdentity (antes solo claims al reclamar; el wallet se perdía).
10. **QASprintRunner**: commands actualizados a UbWeaponCarrier; UbPlayerFix diag retirado de la escena.

### VALIDACIÓN PENDIENTE (editor s&box — usuario)

- **Compilación**: abrir el editor y verificar 0 errores (no hay .NET SDK local).
- **Runtime barrio_01**: Play → spawn del player en la plaza (Z=0), apartamentos visibles y reclamables entrando por el portal, stash, trader, crafting, construir con F, munición consumida al recargar, consumibles curan, misiones completables con recompensa, enemigos nocturnos navegan, muerte → respawn en casa, reinicio → claims/stash/economía restaurados.
- **Multiplayer**: spawn de 2+ jugadores, daño/loot/inventario/networking.
- **Vehículos**: kit `fieldguide.vehiclephysics` sin descargar localmente — bloqueado hasta abrir el proyecto en el editor (montaje automático). Pendiente: rellenar VehiclePrefabPath en vehicle_lab y confirmar nombres de input actions contra el README del kit.

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
