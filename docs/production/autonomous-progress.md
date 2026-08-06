# Progreso de ejecución autónoma — Último Barrio

Última actualización: 2026-08-06, durante la sesión de "toma el control" tras el
reporte del usuario de que E no recogía nada.

## Rama y estado

```
Rama:  integration/wizard-holy-grail
SHA:   e0bb0db  feat(ai): add Bruto and Merodeador enemy archetypes (code only, unverified in engine)
```

Checkpoints creados en esta sesión (no cambiar a ellos, son red de seguridad):
- `checkpoint/pre-full-stack-integration`
- `checkpoint/pre-autonomous-gameplay-completion`

## Bloqueador actual — por qué se detiene esta pasada

El editor de sbox (`sbox-dev`, PID vivo, `Responding=True` a nivel de Windows) dejó
de responder al servidor MCP (`127.0.0.1:7269`) tras varios ciclos de
`play_stop`/`play_start`/`console_command`. Cuatro reintentos con timeouts
crecientes (inmediato, 5s, 15s, 60s) fallaron todos con
`Se excedió el tiempo de espera de la operación`. El puerto sigue abierto
(`Test-NetConnection` = True), pero el proceso no contesta ninguna petición HTTP.
No es una decisión de parar — es un bloqueo externo real. Todo lo de más abajo
marcado como "verificado" lo fue **antes** de este cuelgue, con evidencia de
consola real, no supuesta.

**Siguiente acción exacta al reanudar:** comprobar si `sbox-dev` sigue vivo
(`Get-Process sbox-dev`), probar `editor_status` vía MCP; si sigue sin responder,
reiniciar el editor (cerrar y volver a abrir el proyecto) antes de continuar. No
hace falta reinvestigar nada de lo ya verificado — seguir directamente por
**Fase 11** (ver abajo).

## Causa raíz encontrada y arreglada (el motivo real de este bloque de trabajo)

El usuario reportó que E no recogía nada pese a que `ub_test_all`, el build limpio
y las capturas de `CraftingStation`/`BarricadeAnchor` de la pasada anterior no lo
demostraban. Traza completa de la cadena de interacción con logging temporal
(`UB.Interact ...`), confirmado en vivo vía MCP:

`ResourceNode.OnStart()` fijaba `IsAvailable=false` y `Enabled=false`
incondicionalmente en **todos** los nodos de recurso — y como un componente
deshabilitado nunca ejecuta su propio `OnUpdate` (la única lógica que reactiva
`IsAvailable` tras `RespawnTime`), ningún nodo volvía a estar disponible jamás.
`TryHarvest` rechazaba silenciosamente cada intento para siempre. El prompt
aparecía, `CanInteract` era `true`, `OnInteract` se disparaba — pero nada llegaba
nunca al inventario. Arreglado eliminando el `OnStart` (commit `edbf2d4`).

## Fases completadas y verificadas (con evidencia real, no QA que fabrica el resultado)

| Fase | Estado | Verificación |
|---|---|---|
| Recogida con E | **FUNCIONA** | `ub_qa_test_pickup` (invoca `WorldItemPickup.OnInteract`, el mismo gateway que `PlayerInteractor`): delta=3 confirmado, reproducido en 2 sesiones de Play frescas |
| Inventario/hotbar (stack/drop/repickup/consumo) | **FUNCIONA** | `ub_qa_test_drop_repickup`: drop 2→1, repickup 1→2 (restaurado), agua consumida 1→0 vía `HeldItemController.UseActiveConsumable` real. Bug de validación de `water` corregido (el validador exigía curación a todo consumible, contradiciendo el propio código de consumo) |
| Crafting (atómico, con rollback) | **FUNCIONA** | `ub_qa_test_craft` vía `CraftingStation.RequestCraft` real: rechazo sin ingredientes (sin mutación), éxito con ingredientes (consumo exacto: wood 8→0, scrap 6→0, components 2→0, kit 0→1). Añadidas las 2 recetas que faltaban: `craft_reinforced_barricade_kit`, `craft_reinforced_door_upgrade` |
| Primera vivienda / puerta física | **FUNCIONA** | `ApartmentFortification` (vida/daño/reparación/mejora de la puerta) **no estaba colocado en ningún apartamento** — añadido a los 6, con auto-resolución de `DoorReference` (el MCP no puede serializar referencias GameObject al añadir componentes). Bug de enrutamiento en `PlayerInteractor` arreglado: una vez reclamado, E siempre volvía a intentar reclamar en vez de abrir/cerrar la puerta. `ApartmentDoorPolicy.IsLocked` ahora bloquea físicamente de verdad (Collider.IsTrigger). Verificado con `ub_qa_test_door`: salud 250→200 (daño)→220 (reparación), bloqueo real confirmado, toggle abre/cierra confirmado, `TryUpgrade` nivel 0→1 maxHealth 250→312.5 |
| Barricadas y mejoras | **FUNCIONA** | Bug real: `Barricade` nacía con `Health=0/200` (no `150/150`) porque `Component.OnStart()` no es síncrono tras `Create<T>()` — con `Health<=0`, `IsDestroyed` ya era `true` y cualquier daño real se descartaba en silencio. Arreglado inicializando `MaxHealth`/`Health` explícitamente en `ProcessPlace`. Extendido a niveles (`barricade`=150, `reinforced_barricade_kit`=300) — el kit reforzado tenía receta pero no había dónde colocarlo. Verificado con `ub_qa_test_barricade` para ambos niveles: colocación, daño, reparación, destrucción (libera el anchor) |

## Pendiente — bloqueado por el editor, no por falta de trabajo

- **Fase 11 (persistencia)**: necesita Stop/Play real en el editor. Hay buena señal
  indirecta — `OwnerRespawned apartment=apartment-a02` se repitió en cada reinicio
  de Play Mode de esta sesión, sugiriendo que el claim ya sobrevive reinicios — pero
  **no está formalmente probado** (inventario/wallet/door health/barricadas/nivel de
  mejora tras Stop→Play con aserciones explícitas). Siguiente acción: escribir
  `ub_qa_test_persistence` que capture snapshot antes de Stop, y lo compare después
  de Play.
- **Fase 12 (armas reales)**: sin empezar. Los prefabs `ub_usp.prefab`/`v_usp.prefab`/
  `ub_melee.prefab`/`v_melee.prefab` siguen siendo placeholders (~800-980 bytes).
  Necesita `find_packages`/Library Manager (MCP `package` toolset) para localizar
  `facepunch/sboxweapons` real — bloqueado por el mismo cuelgue del editor.
- **Fase 13 (enemigos nocturnos)**: `BrutoBrain`/`MerodeadorBrain` escritos
  (commit `e0bb0db`), **solo COMPILA** — 0 errores en `dotnet build`, cero
  verificación en motor (el editor llevaba caído desde mitad de Fase 10). No se
  activaron `FeatureFlags.EnableAI`/`EnableRaids` a propósito: hacerlo sin poder
  probar spawn/comportamiento en Play Mode sería exactamente el mismo error que
  causó el bug de recogida con E de esta sesión. Siguiente acción exacta: cuando
  el editor responda, activar los dos flags, colocar un `SpawnZone` de prueba con
  1 `BrutoBrain` + 1 `MerodeadorBrain` + el `SaqueadorBrain` ya existente, y
  verificar con capturas + `read_console` que patrullan/detectan/persiguen/atacan
  antes de dar la fase por buena.
- **Fase 14 (economía/misiones)**: sin empezar. `MissionJournal` UI no existe
  (`UIShell.OpenScreen(ActiveScreen.MissionJournal)` cambia de estado pero no hay
  panel). `MissionSystem.cs` ya modela `ObjectiveType.SurviveNight`. `Trading.Trader`
  no auditado todavía para confirmar atomicidad real de compra/venta.
- **Fase 15 (vehículo)**: sin empezar, spike aislado con
  `matekdev/sbox-arcade-car-physics` (ya fichado, MIT, verificado).

## Archivo exacto en desarrollo al cortar

Ninguno a medias — cada fase se cerró con commit limpio y build en 0 errores antes
de que el editor dejara de responder. El último commit (`0ff94c0`) es autocontenido
y correcto.

## Reglas que siguen aplicando al reanudar

- No usar `git add .`, `reset --hard`, `restore .`, `clean`, force push, merge a
  main.
- Rama operativa sigue siendo `integration/wizard-holy-grail`.
- Los QA (`ub_qa_test_*`) consultan/ejercitan el gateway público real
  ([[ultimo-barrio-runtime-proof]]) — no fabrican el resultado. Seguir ese patrón
  para `ub_qa_test_persistence` y cualquier test nuevo.
- No reabrir la migración a `Sandbox.BaseInventoryComponent` — decisión ya tomada
  con el usuario, con evidencia de código muerto de un intento previo abandonado.
