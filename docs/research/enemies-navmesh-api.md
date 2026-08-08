# NavMesh en s&box — API oficial verificada en el engine instalado

> Worker B — enemy nav/runtime fixtures (ramas `agent/enemies-nav` / `agent/enemies-v2`).
> Fuente: XML de documentación del engine instalado
> (`C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Engine.xml`)
> y código del editor (`addons\tools\Code\Scene\ObjectTool\NavMeshTool*.cs`,
> `addons\tools\Code\Scene\Tools\Component\NavMeshLinkTool.cs`).
> Engine usa **DotRecast** (port C# de Recast/Detour) para generación y pathfinding.

## 1. Cómo se provee un NavMesh en escena (formato exacto del scene JSON)

El NavMesh **no es un componente**: es una sección `NavMesh` dentro de
`SceneProperties` del `.scene` (serie `ResourceVersion`/`__version` 3).

```json
"SceneProperties": {
  "NetworkInterpolation": true,
  "TimeScale": 1,
  "WantsSystemScene": true,
  "Metadata": {},
  "NavMesh": {
    "Enabled": true,              // ← runtime: genera tiles desde la física
    "IncludeStaticBodies": true,  // ← el floor estático ES la superficie
    "IncludeKeyframedBodies": true,
    "EditorAutoUpdate": false,    // solo editor: rebuild automático
    "AgentHeight": 64,
    "AgentRadius": 16,
    "AgentStepSize": 18,
    "AgentMaxSlope": 40,
    "ExcludedBodies": "",
    "IncludedBodies": "",
    "CustomBounds": false
  }
}
```

- Con `Enabled=true`, el runtime genera el NavMesh **por tiles incrementales** a
  partir de los physics bodies de la escena (`IncludeStaticBodies`/`IncludeKeyframedBodies`).
- La superficie caminable en el lab es un `Sandbox.MeshComponent` cuádruple
  (`Collision: "Mesh"`, `Static: true`) — el mismo patrón de la escena plantilla
  `sandbox.addon/Assets/scenes/testing_scene.scene` ("Block"). Un `PlaneCollider`
  infinito es arriesgado para el horneado (bounds ilimitados); el quad acotado es
  determinista.
- `NavMeshArea` (componente, `IsBlocker=true`) recorta zonas del NavMesh y
  `NavMeshLink` conecta polígonos (escaleras/saltos) — no necesarios en el lab plano.
- Pre-bake opcional: el editor escribe un navdata file por escena
  (`NavMesh.BakeData(Scene, ...)` / `LoadFromBakedData(byte[])`) — aceleración,
  no requisito: la generación runtime cubre el caso.

## 2. API runtime oficial (namespace `Sandbox.Navigation`)

Clase estática `Sandbox.Navigation.NavMesh` (el acceso `Scene.NavMesh` del editor
es wrapper del editor; en juego se usa la clase):

| Miembro | Firma / uso | Significado |
|---|---|---|
| `IsEnabled` | `bool` | NavMesh habilitado para la escena actual |
| `IsGenerating` | `bool` | tiles en generación (esperar antes de pathfind) |
| `IsDirty` / `SetDirty()` | — | marca rebuild |
| `AgentHeight/Radius/StepSize/MaxSlope` | `float` | parámetros de horneado (idem SceneProperties) |
| `IncludeStaticBodies` / `IncludeKeyframedBodies` | `bool` | fuentes de geometría |
| `ExcludedBodies` / `IncludedBodies` | `string` | filtros por body |
| `CustomBounds` / `Bounds` | `bool` / `BBox` | volumen de horneado |
| `GenerateTile(PhysicsWorld, Vector3)` / `GenerateTiles(PhysicsWorld, BBox)` | — | generación síncrona |
| `RequestTileGeneration(Vector3)` / `RequestTilesGeneration(BBox)` | — | generación incremental (fire-and-forget) |
| `UnloadTile/UnloadTiles` | — | descarte |
| `CalculatePath(CalculatePathRequest)` | `NavMeshPath` | pathfinding (mismo algoritmo que NavMeshAgent) |
| `GetRandomPoint(BBox)` / `GetRandomPoint(Vector3, float)` | `Vector3?` | punto aleatorio sobre el mesh |
| `BakeData(Scene, Action<int,int>, CancellationToken)` / `BakeDataToBytes` / `LoadFromBakedData` | — | bake a fichero |
| `DrawMesh` | `bool` | debug draw |

`CalculatePathRequest` (struct): `Start` (cerca del mesh), `Target`, `Agent` (opcional:
usa su configuración). `NavMeshPath`: `Status` (`NavMeshPathStatus.StartNotFound |
TargetNotFound | PathNotFound | Partial | Complete`), `IsValid`, `Points` (lista de
`NavMeshPathPoint` con `Position`), `Polygons` (interno).

## 3. NavMeshAgent (componente `Sandbox.NavMeshAgent`)

`[RequireComponent]` en `EnemyContentHost` — se autoañade en el prefab al clonar.

| Miembro | Uso |
|---|---|
| `MoveTo(Vector3)` | navegar a una posición |
| `Stop()` | detenerse |
| `SetPath(NavMeshPath)` / `GetPath()` | path precalculado |
| `IsNavigating` | true mientras navega |
| `AgentPosition` | posición del agente (aunque `UpdatePosition=false`) |
| `TargetPosition` | target actual del agente |
| `WishVelocity` | velocidad deseada (movimiento real) |
| `Acceleration` | aceleración máx. |
| `UpdatePosition` / `UpdateRotation` | seguir pos/rot del GameObject (default true) |
| `AllowedAreas` / `ForbiddenAreas` / `AllowDefaultArea` | restricción de áreas |
| `AutoTraverseLinks` / `CompleteLinkTraversal` / `LinkEnter` / `LinkExit` | links |
| `Separation` | separación entre agentes |
| `GetLookAhead(float)` | punto a distancia en el path |

No existe `MaxSpeed` documentado en esta versión (el movimiento usa el default del
engine; `WalkSpeed` del arquetipo es data-only hasta el adaptador del core nuevo).

`NavMeshGameSystem` actualiza en paralelo el grounding de los agentes
(`FindPhysicsGroundZ`) en PrePhysicsStep: los agentes se asientan sobre el mesh solo.

## 4. Integración en el enemy_lab (fixtures Worker B)

- `enemy_lab.scene`: `SceneProperties.NavMesh.Enabled=true` + floor quad estático
  (`Sandbox.MeshComponent`, Collision Mesh, Static) + `Enemy Spawn Marker`
  (96,0,8) + `TestTarget` fijo (crate + BoxCollider estático + `LabDamageDummy`
  200 HP, en 768,0,60) + `Loot Observation Point` con `LabLootObserver`.
- `EnemyNavSuite` (ILabSuite): valida el escenario con la ruta real —
  `NavMesh.CalculatePath` (PathCheck Complete) → probe `NavMeshAgent` real con
  descenso t0/t1/t2 → prefab enemigo (host → agent) → kill por `IDamageTarget`
  → observación física del loot. Anti-teleport: salto por frame < 120u,
  recorrido acumulado ≥ 70% de la recta, t0 > t1 > t2 estricto.
- Evidencia: logs `[EnemyLab]` + línea machine-readable `[UBSuite] Enemy.<Label>`.

## 5. Referencias

- `Sandbox.Engine.xml` → `T:Sandbox.NavMeshAgent`, `T:Sandbox.Navigation.NavMesh`,
  `T:Sandbox.Navigation.NavMeshPath`, `T:Sandbox.Navigation.CalculatePathRequest`,
  `T:Sandbox.NavMeshArea`, `T:Sandbox.NavMeshLink`, `T:Sandbox.NavMeshGameSystem`.
- Editor: `addons/tools/Code/Scene/ObjectTool/NavMeshTool.SubTools.cs` (Bake +
  Path Tester + Bounds), `NavMeshLinkTool.cs` (`GetClosestPoint`).
- Plantilla engine: `templates/sandbox.addon/Assets/scenes/testing_scene.scene`
  (formato SceneProperties.NavMesh + floor quad).
