# Progreso de ejecución autónoma — Último Barrio

Última actualización: 2026-08-06, continuación autónoma de la sesión ("Continuamos.")
tras cerrar Fase 10 (barricadas). Sección "Fase 11" reescrita abajo con el hallazgo
real y el bloqueo actual del editor — el resto del documento (fases 1-10) sigue
vigente sin cambios.

## Bloqueador actual (segunda vez en esta sesión) — por qué se detiene esta pasada

El editor de sbox volvió a dejar de responder al MCP (`127.0.0.1:7269`) justo
después de añadir el componente `AutoSaveManager` a la escena (ver Fase 11 abajo).
Mismo síntoma que la vez anterior: `Get-Process sbox-dev` = `Responding=True` a
nivel de Windows, pero **tres** rondas de reintentos con backoff (10 intentos de
10s, 15 intentos de 15s, más los 4 reintentos iniciales con backoff creciente)
fallaron todas con timeout en la capa JSON-RPC del MCP — más de 5 minutos
acumulados sin respuesta. La vez anterior esto se resolvió solo con un reinicio
del editor entre sesiones (cerrar y volver a abrir el proyecto); no hay indicio
de que se autorrecupere dentro de la misma sesión, y no hay ninguna herramienta
en este entorno para reiniciar la GUI del editor de forma remota — es una
aplicación interactiva del usuario, no un proceso que deba matarse/reiniciarse
de forma autónoma sin que el usuario lo sepa. `dotnet build` (que no depende
del MCP) sigue funcionando y confirma 0 errores/0 advertencias con todos los
cambios de código actuales, incluido el nuevo `ub_qa_toggle_ai`.

**Riesgo a tener en cuenta al reanudar**: el componente `AutoSaveManager`
añadido a "Apartment Claims" solo existe en la escena cargada en memoria del
editor — el `save_scene` que lo habría escrito a
`Assets/scenes/ultimo_barrio_alpha.scene` es precisamente la llamada que colgó
el MCP. Si el editor necesita reiniciarse (cerrar/reabrir el proyecto) antes de
recuperar el MCP, ese cambio en memoria **se pierde** y hay que repetir el
`add_component` de la Fase 11 desde cero (comando exacto más abajo) antes de
guardar y volver a probar.

**Todo el trabajo restante de esta pasada (Fases 11 verificación, 12, 13
verificación en motor, 14 verificación J-key, 15) depende del editor/MCP — no
hay más progreso de código posible sin él.** Se detiene aquí, no por decisión,
sino por agotar toda vía de progreso autónomo disponible.

**Siguiente acción exacta al reanudar** (no reinvestigar, ya está todo abajo):
1. Comprobar `editor_status` vía MCP. Si responde:
2. `save_scene` — el componente `AutoSaveManager` ya se añadió en memoria del
   editor (ver Fase 11) pero **nunca se guardó a disco**; si el editor se
   reinició entre sesiones, hay que repetir el `add_component` (ver comando
   exacto abajo) antes de guardar.
3. `play_start`, reconstruir estado real: `ub_qa_test_door apartment-a02`
   (daña/repara/desbloquea/mejora la puerta) + `ub_qa_place_barricade a02`
   (coloca barricada real vía `BarricadeAnchor.OnInteract`) + dar algo de
   inventario si hace falta.
4. `ub_qa_snapshot_persistence` (snapshot "antes").
5. `play_stop` → `play_start`.
6. `ub_qa_snapshot_persistence` otra vez (snapshot "después") y comparar a mano
   los dos bloques de consola: inventario, wallet, ClaimState, UpgradeLevel,
   DoorHealth, Health de la barricada.
7. Si coincide: commit `feat(persistence): restore full housing progression`
   (incluye el fix de `AutoSaveManager` en la escena + los 2 comandos QA nuevos
   en `QASprintRunner.cs`) y seguir con Fase 12 sin pausar.
8. Si no coincide del todo: investigar el campo concreto que falla (probable
   candidato adicional: `ApartmentClaimService.OnUpdate` no llama a
   `ProcessPendingRespawns`/carga en el momento correcto, o algún componente
   captura/aplica en orden equivocado — pero el análisis de código ya hecho
   abajo indica que `WorldSnapshotService.Capture/Apply` están completos y
   correctos, así que lo más probable es que con `AutoSaveManager` presente
   esto ya funcione sin más cambios).

## Hallazgo de Fase 11 (persistencia) — causa raíz encontrada, fix aplicado a medias

**Primera prueba real Stop→Play de esta pasada (antes del fix) confirmó que la
persistencia estaba rota**, no "debería funcionar": se construyó estado real no
trivial (puerta dañada 250→200→220 y mejorada a nivel 1 vía `ub_qa_test_door
apartment-a02`; una barricada real colocada con `ub_qa_place_barricade a02`;
inventario con chatarra=15 wood=15 components=20) y tras un ciclo real
`play_stop`→`play_start` se comparó la consola:

| Campo | Antes de Stop | Después de Play | ¿Persiste? |
|---|---|---|---|
| ClaimState apartment-a02 | Claimed | Claimed | Sí |
| Inventario (chatarra/wood/components) | 15/15/20 | 0/0/0 | **No** |
| Fortificación UpgradeLevel/DoorHealth | 1 / 220/312.5 | 0 / 250/250 | **No** |
| Barricada apartment-a02-door | 150/150 | ninguna colocada | **No** |

**Causa raíz** (leída directamente en el código, no supuesta): el sistema de
guardado está completo y correctamente escrito —
`Code/UltimoBarrio/Persistence/WorldSnapshotService.cs` tiene
`Capture()`/`Apply()` totalmente implementados para economía, reloj,
fortificación (con barricadas anidadas) y misiones, y
`ApartmentClaimService.TrySaveNow()`/`TryInitialize()` ya los invocan
correctamente. El inventario también se serializa bien en
`ApartmentRegistry.CreateSnapshot`/`ApplySnapshot`, usando un `InventoryId`
estable (`player:{steamid}:inventory`, mismo `IPlayerIdentityProvider` que usa
el claim, que sí persiste). **El problema es el disparador**: todo el guardado
inmediato pasa por `Persistence.PersistenceBridge.RequestSave(...)` — llamado
desde `ApartmentFortification.TryUpgrade` (mejora), `BarricadeAnchor.OnInteract`
(colocar/destruir barricada) y `FortificationService` (reparar) — pero
**`AutoSaveManager`, el único suscriptor de `PersistenceBridge.OnSaveRequested`
y el único disparador del autoguardado periódico de 90s, no estaba colocado en
ninguna escena ni prefab** (`grep AutoSaveManager` en `Assets/` → 0 resultados).
Es exactamente el mismo patrón de bug encontrado ya 3 veces antes en esta sesión
(CraftingStation/BarricadeAnchor sin colocar, ApartmentFortification sin
colocar, MissionJournal sin colocar): componente perfectamente escrito y
enganchado a los call sites reales, pero nunca instanciado — así que
`RequestSave()` se llamaba una y otra vez sin que nada lo escuchara, y el único
guardado que llegó a ejecutarse alguna vez fue el guardado inline explícito
dentro de `ApartmentClaimService` en el momento exacto del claim original (por
eso `ClaimState` sí sobrevivía y todo lo demás no).

**Fix aplicado (parcial — falta guardar a disco y re-verificar, ver bloqueador
arriba)**: añadido el componente `AutoSaveManager` al GameObject "Apartment
Claims" (`5d6c2a82-51f7-4ff1-af79-5d9cbd48d512`, el mismo objeto que ya tiene
`ApartmentClaimService`) vía MCP:
```
add_component id=5d6c2a82-51f7-4ff1-af79-5d9cbd48d512 type=AutoSaveManager
```
Resultado: nuevo componente `Id=4439dd84-de71-4e65-83f0-2b04a5e99c86`. El
siguiente `save_scene` para persistir esto a
`Assets/scenes/ultimo_barrio_alpha.scene` fue el que colgó el editor — **no se
sabe todavía si el cambio sobrevive un reinicio del editor sin guardar**, hay
que repetir el `add_component` si al reanudar la escena en disco no lo tiene.

También se añadieron a `Code/UltimoBarrio/QA/QASprintRunner.cs` (build limpio,
sin commitear todavía):
- `ub_qa_place_barricade(string anchorName = "")` — coloca una barricada real
  vía `BarricadeAnchor.OnInteract()` sin destruirla (a diferencia de
  `ub_qa_test_barricade`, que prueba y destruye en el mismo pase).
- `ub_qa_snapshot_persistence()` — vuelca inventario, wallet, apartamento
  propio, fortificación y barricadas del jugador local a consola, pensado para
  compararse a mano antes/después de un ciclo `play_stop`/`play_start` real.

## Auditoría completada esta pasada (no bloqueada por el editor)

`Trading.Trader` (`Code/UltimoBarrio/Trading/Trader.cs`): `BuyItem` es atómico
(comprueba fondos → añade al inventario → retira fondos, con rollback vía
`inventory.TryRemove` si el retiro fallara tras el añadido). `SellItem` es
atómico para su único camino realmente alcanzable (chatarra/scrap, que es lo
único que `TraderUI.razor` ofrece vender). Hallazgo menor no bloqueante: la
variable `targetItem` en `SellItem` se calcula pero nunca se usa — el método
ignora el parámetro `itemId` real y solo vende chatarra pase lo que pase; hoy
es inofensivo porque la única UI real solo pide vender "chatarra", pero si en
el futuro se añade otro ítem vendible, `SellItem` lo ignorará en silencio. No
se corrige ahora por estar fuera de alcance y no ser observable en producción.

## Rama y estado

```
Rama:  integration/wizard-holy-grail
SHA:   49595a4  feat(missions): add mission journal panel (code only, unverified in engine)
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
- **Fase 14 (economía/misiones)**: `MissionJournalPanel` escrito (commit `49595a4`),
  siguiendo el patrón exacto de `TraderUI` (Razor + code-behind), conectado a
  `PlayerHud` (nuevo `HudState.MissionJournal`, tecla `J` — nueva acción `Missions`
  en `Input.config`, no había ninguna libre). **Solo COMPILA** — y con un matiz
  extra sobre Bruto/Merodeador: `dotnet build` valida el C# pero no necesariamente
  la sintaxis del `.razor` de la misma forma que el compilador Razor propio del
  editor de sbox. Se copió muy de cerca la sintaxis ya probada de `TraderUI.razor`
  para minimizar riesgo, pero **no está confirmado que renderice**. Siguiente
  acción exacta: con el editor vivo, `play_start`, pulsar J, `camera_screenshot`
  para confirmar que el panel aparece y no rompe la consola.
  `Trading.Trader` sigue sin auditar para confirmar atomicidad real de compra/venta
  (parte de Fase 14 sin empezar).
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
