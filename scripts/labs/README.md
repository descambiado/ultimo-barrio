# Laboratorios de spike + mini-suite runtime — nodo portátil (spike/laptop-content-stack)

Escenas aisladas para probar el contenido portable del paquete `Content`
sin tocar `ultimo_barrio_alpha.scene` ni el core antiguo. Incluye la
**mini-suite unificada** (`ContentRuntimeSuite`) que valida armas ya y tiene
contratos listos para enemy/building/vehicle.

> ⚠️ Escenas generadas por script (`python scripts/labs/generate_lab_scenes.py`).
> Si cambias el formato de escena del editor, re-genera (no edites a mano los GUIDs
> generados). El `WeaponTestRig` + su `CameraComponent` del weapon_lab se añadieron
> a mano (infra de test, ver commit del rig).

## Pipeline del coordinador (validación runtime)

El editor es un recurso SERIAL: solo el coordinador ejecuta `play_start`/`play_stop`/
`compile`. Los workers entregan commits; el coordinador valida. Ciclo por spike:

1. **Inspección estática** — el worker revisa el diff (`git show`), sin tocar el editor.
2. **Cherry-pick selectivo** — se trae SOLO lo necesario (nunca `add .` / merges / force).
3. **Compile (gate REAL)** — `editor_status`: `IsCompiling=false` Y
   `LastCompileSucceeded=true`, o `compile_status`: `IsBuilding=false` +
   `Success=true` + `Errors=0` en `local.ultimo_barrio`.
   ⚠️ **Nunca confiar en un Success viejo**: si `IsBuilding=true`, el Success mostrado
   es del build ANTERIOR. Si el marcador `[LabBuild] VERSION=<n>` en consola es viejo
   o ausente → el DLL no se actualizó → DIAGNOSTICAR (otra instancia sbox-dev, asset
   sin recompilar, tab vieja), no esperar a ciegas.
4. **Lab** — `play_stop` limpio si `IsPlaying`, luego `play_start` en la escena del lab.
5. **Autotest** — el rig ejecuta la suite solo; leer logs con `read_console` (filter).
6. **PASS/FAIL** — según logs deterministas `[WeaponLab] <Label> PASS/FAIL` y
   `[UBSuite] <Domain>.<Label> PASS|FAIL|SKIP`; 0 errores propios
   (los 2 warnings preexistentes del core antiguo se ignoran).
7. **Fix** — si FAIL: diagnosticar, arreglar, repetir desde el paso 3 (subir VERSION).
8. **Commit** — `feat(spike): ...` + STATE.md actualizado + SHA + evidencia.

Anti-falsificación: el daño/navegación/placement se prueban por la ruta REAL
(host → trace → `IDamageTarget` / NavMeshAgent / validación server), nunca llamando
internals del test. El rig solo sustituye input humano y target humano.

## Mini-suite unificada (UltimoBarrio.Content.Dev)

Infra QA, SOLO dev — nunca producción. Todo en `Code/UltimoBarrio/Content/Dev/`:

| Archivo | Rol |
|---|---|
| `ilabsuite.cs` | Contrato `ILabSuite` (Domain, Name, Initialize, Step, IsComplete, Result Pass/Fail/Skip + Detail) + `LabSuiteResult` |
| `contentruntimesuite.cs` | Registro estático `ContentRuntimeSuite.Register(...)` + runner `ContentRuntimeSuiteRunner` (Component) |
| `weaponsuite.cs` | `WeaponSuite` — lógica validada del rig (logs `[WeaponLab]` intactos) + tipos `WeaponTestEntry`/`WeaponTestType` |
| `enemysuite.cs` / `buildingsuite.cs` / `vehiclesuite.cs` | Contratos SKIP para workers A/C/E (implementar sin fabricar resultados) |
| `weapontestrig.cs` | Fixture de escena: cámara propia + TargetDummy + registra una `WeaponSuite` por entrada y crea el runner |

Flujo: el rig registra suites en `ContentRuntimeSuite` y crea
`ContentRuntimeSuiteRunner` (un solo runner activo por sesión). El runner ejecuta
cada suite en orden (Initialize → Step(dt) por frame) y emite una línea
machine-readable por suite:

```
[UBSuite] Weapon.USP PASS time=8.52s delta=15.0 state=complete
[UBSuite] Weapon.Crowbar PASS time=6.00s delta=35.0 state=complete
[UBSuite] Enemy.Saqueador SKIP time=0.00s delta=0.0 state=skip (contrato Worker A: ...)
[UBSuite] Suite run complete (2 PASS, 0 FAIL, 1 SKIP)
```

Resumen legacy al final del weapon_lab: `[WeaponLab] Suite complete (4/4 PASS)`.

Contrato para los workers A/C/E: implementar la suite del dominio con la ruta real
(ver docs en cada stub) y registrarla desde el rig de su lab. El runner es común.

## Escenas

| Escena | Qué probar | Suite |
|---|---|---|
| `Assets/scenes/spikes/weapon_lab.scene` | USP, palanca, cuchillo, escopeta (spawn, disparo/melee, daño a IDamageTarget, recarga, dry fire) | `WeaponSuite` (4 tests, rig-7) |
| `Assets/scenes/spikes/enemy_lab.scene` | Spawn de Saqueador/Bruto/Merodeador, NavMesh, persecución, ataque al dummy, daño, muerte, botín | `EnemySuite` (contrato Worker A) |
| `Assets/scenes/spikes/building_lab.scene` | Autotest BuildingTestRig (rig-1): preview inválido/bloqueado/overlap REJECTED, válido ACCEPTED, spawn, HP, daño (trace real), repair (consumo fixture), upgrade (madera→reforzada), destroy | `BuildingSuite` (contrato Worker C, sin input manual) |
| `Assets/scenes/spikes/vehicle_lab.scene` | Stub: rellenar prefabs cuando el research decida el paquete de vehículos (manifest bloque H) | `VehicleSuite` (contrato Worker E) |

## Cómo abrir

1. Abre s&box con el proyecto en la rama `spike/laptop-content-stack`.
2. En el editor, abre la escena de lab deseada y pulsa Play (el `NetworkHelper`
   spawnea al jugador en `Primary Spawn`).
3. Verifica en consola que no hay errores del proyecto.

## Qué esperar (primera pasada)

- **weapon_lab**: al pulsar Slot1..Slot4 se instancia el prefab del arma como
  hijo del jugador; los modelos usan los fallbacks verificados (los candidatos
  primarios están marcados ⚠️ PENDING_VERIFY en los registros). Con `AutoTest`
  activo, el rig ejecuta la mini-suite completo sin input (rig-7: `[LabBuild] VERSION=rig-7`).
- **enemy_lab**: usa `MapInstance thieves.rpdowntown3t` (verificado) para que
  NavMeshAgent tenga navmesh. Los enemigos persiguen al dummy (BuildStructureHost
  como IDamageTarget) y sueltan pickups al morir.
- **building_lab**: el BuildingTestRig valida el bucle completo por la ruta real
  (BuildPlacementRules → SpawnBuild → trace/IDamageTarget → Repair con
  LabResourceFixture → Upgrade → destroy). Autotest: `[LabBuild] VERSION` + `[BuildingLab] PASS`.

## Verificación pendiente (cuando el editor esté disponible)

- [ ] Compilación del proyecto con 0 errores (atención a `Scene.GetAllComponents<T>()`
      en los spawners Dev y a `NavMeshAgent` en `EnemyContentHost`).
- [ ] Carga de las 4 escenas sin errores de serialización.
- [ ] `models/dev/plane.vmdl` en los labs de suelo (usado por main.scene, pero confirmar).
- [ ] Cloud Browser: verificar modelos primarios ⚠️ de armas, enemigos y fortificaciones
      y actualizar `AssetsVerified` en los registros.
- [ ] Sonidos: crear los SoundEvent referenciados (`weapon.usp.fire`, etc.) o vaciar los campos.

## Contrato de portabilidad

Todo lo que hay bajo `Code/UltimoBarrio/Content/` y `Assets/prefabs/content/`
es autocontenido: no referencia `InventoryComponent`, `HeldItemController`,
`AIBase` ni `HealthComponent` del core antiguo. Para portar a
`integration/wizard-holy-grail` basta cherry-pick de:

- `Code/UltimoBarrio/Content/` (contratos + hosts)
- `Assets/prefabs/content/` (prefabs)
- `Assets/scenes/spikes/` (labs)
- `docs/research/laptop-content-integration-manifest.md` (decisión de stack)

Los bridges al core nuevo (daño, inventario, sonido, animación) están marcados
con `TODO(core nuevo)` en el código.
