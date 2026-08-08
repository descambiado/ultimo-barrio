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

---

## Enemigos (rama `agent/enemies`, pendiente de validación runtime por el coordinador)

- **Arquitectura data-driven, UN brain**: `EnemyContentDefinition` (HP/speed/visión rango+ángulo/oído/rango+daño+cooldown de ataque/prioridad de estructura/loot) → `EnemyContentRegistry` (Saqueador rápido-vida media-baja-prioriza player, Bruto lento-mucha vida-alto daño estructural, Merodeador movilidad media-percepción superior) → `EnemyContentHost` (NavMeshAgent REAL, percepción y ataque como componentes separados `EnemyPerception`/`EnemyAttack`, daño por `IDamageTarget`, muerte con loot FÍSICO `LootPickupContent` — sin inventario canónico). Contrato de integración futura: `IEnemyContentAdapter`.
- **Rig**: `EnemyTestRig` (UltimoBarrio.Content.Dev) con suite data-driven (Saqueador/Bruto/Merodeador). Acceptance por test: spawn → NavMeshAgent válido → detecta target (visión + sub-check oído) → distancia disminuye (t0→t1) → llega (t2) → ataca → target pierde HP → enemigo recibe daño → muere → loot físico → PASS. Logs `[EnemyLab]`, marcador `[LabBuild] VERSION=rig-1`.
- **Escena**: `Assets/scenes/spikes/enemy_lab.scene` — `MapInstance facepunch.flatgrass` (NavMesh real; en el sbproj `MapList`), NetworkHelper `StartServer` SIN `PlayerPrefab` (autotest), rig con cámara propia, SpawnMarker (100,0,0) + DummyMarker (1100,0,64) sobre NavMesh. Sin jugador humano, sin PlayerController.
- **API NavMesh investigada en el engine** (Sandbox.Engine.xml + tools): `Sandbox.Navigation.NavMesh` (`IsEnabled/IsGenerating/IsDirty`, `RequestTileGeneration(Vector3)` incremental, `GetRandomPoint(Vector3,float)`, `SetDirty`/`Generate(PhysicsWorld)`) y `Sandbox.NavMeshAgent` (`MoveTo/Stop/IsNavigating/AgentPosition/MaxSpeed`). El host usa solo `NavMeshAgent` (documentado); el rig usa `Scene.NavMesh` (evidencia: `get_NavMesh` en Sandbox.Engine.dll + uso en tools) aislado en `LogNavMeshDiagnostics()` — si no compila en runtime, comentar ese método y el fallo pasa a ser comportamental.
- **Pendiente para el coordinador**:
  1. Validar runtime del lab (el rig mide todo; si el NavMesh de flatgrass no está baked, mover SpawnMarker/DummyMarker o cambiar de mapa — los logs `[EnemyLab] ⚠️ ... NO está sobre NavMesh` lo indican).
  2. Borrar legacy muerto: `Code/UltimoBarrio/Content/enemies/enemyarchetypedefinition.cs` y `Code/UltimoBarrio/Content/dev/labenemyspawner.cs` (sustituidos por `EnemyContentDefinition`/`EnemyTestRig`; el borrado de archivos fue denegado por el Safety Guard en la sesión del worker).
  3. `scripts/labs/generate_lab_scenes.py` está desactualizado (generaría escenas con `PlayerPrefab` + `LabEnemySpawner`); las escenas reales se escriben a mano, no regenerar con él sin actualizarlo.
