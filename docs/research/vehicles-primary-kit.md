# Vehicles — Kit PRIMARY: `fieldguide.vehiclephysics` (Vehicle Physics Kit)

- **Fecha:** 2026-08-08 (turbo session 2, Worker D — vehicle foundation)
- **Rama:** `agent/vehicles-v2` (base `spike/laptop-content-stack` @ `46b3b01`)
- **Método:** `find_packages`/`get_package` (MCP sbox backend, read-only) + sboxdb.dev + GitHub API. `web_search` del broker AutoGLM **caído** (broker_unauthorized durante toda la sesión) → la verificación de licencia se hizo por el backend oficial de sbox (más fuerte que una búsqueda web).

## Criterios verificados (editor 26.08.05)

| Criterio | **fieldguide.vehiclephysics** (PRIMARY) | clearly.cavc | matekdev/sbox-arcade-car-physics | bugge.vehicle_controller |
|---|---|---|---|---|
| License | **MIT** ✅ (texto oficial en `get_package`: *"MIT licensed. Fork it, ship it, no strings."* + confirmado en sboxdb) | ⚠️ no verificado | MIT ✅ (GitHub API `license.spdx_id`) | ⚠️ no verificado |
| Última actualización | **2026-07-31** ✅ (v337869 en sboxdb; el kit nació 2026-07-19) | 2025-03-08 (17 meses) | push 2025-09-14 (11 meses) | 2026-05-03 |
| Type | **library** (paquete cloud) | library | GitHub addon | library |
| API generación 26.08.05 | **actual** ✅ (descripción oficial: `CarDefinition` + `VehicleFactory.Spawn` + seam `CustomBodyBuilder`; GameObject/Component) | probablemente antigua | **ANTIGUA — verificada en source**: `SceneObject( Scene.SceneWorld, ... )`, `Input.AnalogMove`, `Input.Down( InputAction.JUMP )` → NO compila en 26.08.05 | no verificada |
| Source accesible desde este entorno | ❌ (library packages no instalables por `install_package` MCP — solo asset packages; sin mirror GitHub) | ❌ | ✅ (GitHub raw) | ❌ |
| Dependencia durable | **PackageReferences** (cloud) ✅ | PackageReferences | documentación del addon | PackageReferences |

## Decisión

**PRIMARY = `fieldguide.vehiclephysics`** (sbox Field Guide, Vehicle Physics Kit).

- Ident EXACTO verificado por `find_packages` + `get_package` (backend oficial, 2026-08-08).
- Cumple todos los criterios del task: licencia compatible (**MIT**), mantenido (2026-07-31, semanas antes del editor 26.08.05), API actual (raycast wheels, slip-curve, drivetrain, assists, chase cam; montaje vía `VehicleFactory`), sin arte requerido.
- Mantiene el veredicto **ADOPT** del research previo (`docs/research/workers/vehicles.md`).
- **Corrección al research**: *Vehicle Prototyping* (`fieldguide/vehicle_prototyping`) **YA NO EXISTE** en el backend (`find_packages` = 0 resultados; catálogo Field Guide = 5 paquetes: vehiclephysics, tips, placement, world_builder, daynight). El montaje/fábrica ahora vive dentro del propio kit (descripción oficial: spawn con `VehicleFactory.Spawn`). No hay que añadir un segundo paquete.

## Descartados (con por qué — timebox 30 min/kit)

1. **matekdev/sbox-arcade-car-physics** (GitHub, MIT): fuente leída íntegra (Vehicle.cs / Input / Physics / Axle / Wheel). Usa **API antigua** (`SceneObject(Scene.SceneWorld, ...)`, `Input.AnalogMove`, `Input.Down(InputAction.JUMP)`) — no compila contra 26.08.05. Descartado por API, no por licencia.
2. **clearly.cavc**: sin actividad desde 2025-03-08 y licencia sin verificar → no cumple "mantenido" ni "licencia".
3. **bugge.vehicle_controller**: licencia sin verificar.
4. **koncha.fosterz_cardemo** (ZCars): `type=game` (no library); se mantiene como PATTERN del research.
5. Modelos `facepunch.van_dev` / `facepunch.forklift_up_dev` (model, facepunch): candidatos a **visual del vehículo** para integración futura, no para física (fuera del alcance de esta foundation).

## Estrategia del rig frente a un kit cuyo source no es legible desde este entorno

Regla del pack: **nunca inventar API**. El `VehicleTestRig` usa SOLO API de engine verificada en 26.08.05 (XML `Sandbox.Engine.xml` + código de los addons base/tools/citizen + código ya validado del repo):

- **Spawn:** `ResourceLibrary.Get<PrefabFile>` + `SceneUtility.GetPrefabScene` + `Clone()` + `NetworkSpawn( Connection.Local )` — patrón ya validado en `LabVehicleSpawner`. El path del prefab es **propiedad configurable** (`VehiclePrefabPath`): el coordinador la rellena con un prefab del kit tras montar el paquete (el kit bundlea `vehiclephysics_demo` con su coche).
- **Simulación de input (sustituye al humano):** `Input.SetAction( string, bool )` — API de engine **verificada** en Sandbox.Engine.xml. Los **nombres** de las input actions del kit son **datos configurables** (propiedades del rig en la escena), NO API inventada: defaults `Forward` / `Brake` / `SteerLeft` / `SteerRight` / `Reverse` documentados como supuestos a confirmar contra el README del kit (Source/README) tras el montaje.
- **Enter/exit:** fixture DEV propio (`VehicleDriverFixture`) — `GameObject.Parent` / `SetParent` (verificado en `addons/tools/Code`). El kit no cubre enter/exit (hueco confirmado en research) → el attach/detach es el flujo probado.
- **Anti-falsificación:** PASS solo con deltas reales: posición (throttle/reverse), yaw (steering), velocidad por ventanas de posición (brake). Sin física propia, sin fijar posiciones, sin llamadas a internals.

## Dependencia durable

- `ultimo_barrio.sbproj` → `PackageReferences` += `"fieldguide.vehiclephysics"` (ident verificado; commit aparte).
- ⚠️ **Merge manual** con el sbproj del core (regla del porting plan, sección 5).
- NOTA técnica: `install_package` (MCP) **falla** para library packages ("is it a valid asset package ident" — solo asset packages). El montaje durable de un library es exclusivamente vía `PackageReferences` (el editor lo compila al abrir el proyecto).

## Pendientes del coordinador (validación runtime)

1. Merge del sbproj (con `fieldguide.vehiclephysics`) → abrir editor → el paquete se monta y su código compila.
2. Leer `Source/README` del kit (visible al montar) → **confirmar los nombres de input actions** y ajustar las propiedades del rig en `vehicle_lab.scene` si difieren de los defaults.
3. Rellenar `VehiclePrefabPath` con un prefab de vehículo del kit (guardar el coche de `vehiclephysics_demo` como prefab o usar el path del paquete).
4. `Play vehicle_lab` → `[LabBuild] VERSION=rig-1` + secuencia `[VehicleLab]` spawn → components → enter → throttle → steer → brake → reverse → exit → `Suite complete (8/8 PASS)`.

## Evidencia

- `find_packages "vehicle physics"` (MCP, 2026-08-08): `fieldguide.vehiclephysics`, library, org "sbox Field Guide", Updated 2026-07-31.
- `get_package fieldguide.vehiclephysics` (MCP): descripción oficial con "MIT licensed. Fork it, ship it, no strings."; Created 2026-07-19, Updated 2026-07-31, Public, no Archived.
- sboxdb.dev/package/fieldguide/vehiclephysics: versiones 337869 (latest) / 313829 / 312377 / 312302 / 312281; mismo texto MIT.
- GitHub API: `matekdev/sbox-arcade-car-physics` license=MIT, pushed 2025-09-14 (descartado por API antigua).
- SHAs de esta rama: ver git log `agent/vehicles-v2`.
