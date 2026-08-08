## Estado Actual: Laptop Content Stack — weapon_lab validado (USP)

- **Rama Actual**: `spike/laptop-content-stack` (base `feat/holy-grail-foundation` @ c9e5664). Sin push.
- **Último Commit**: `9839846 feat(spike): validate portable USP weapon loop` (2026-08-07).
- **Errores de Compilación**: 0 (solo 2 warnings preexistentes del core antiguo: GameResourceAttribute obsolete en ItemDefinition.cs / MovementProfile.cs).
- **Arquitectura del lab (corregida)**: fuera el player/cámara Frankenstein. El autotest corre en un **Weapon Test Rig** fijo (GameObject independiente con CameraComponent main + TargetDummy en la línea de fuego, sin pawn). El player manual es el **PlayerController oficial** del engine (`lab_player_official.prefab`, copia literal del template, sin código custom) — pendiente de validar al final de los labs.

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

### Siguientes Pasos

1. Replicar patrón limpio: crowbar → knife → shotgun (mismo rig, cambiar prefabs/idents).
2. `enemy_lab` (Saqueador), `building_lab` (barricada de madera), `vehicle_lab`.
3. Prueba manual única al final de todos los labs (PlayerController oficial).

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
