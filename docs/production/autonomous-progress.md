# Progreso de ejecución autónoma — Último Barrio

## VERIFICADO EN MOTOR (2026-08-06, editor recuperado) — leer esto primero

El editor volvió a responder. Se cerró el checklist de reanudación de la
sección "SISTEMA DE PROPIEDADES" de abajo y se verificó físicamente con
`ub_qa_test_property_claim`, `ub_qa_test_property_credential`,
`ub_qa_test_property_rent` y ciclos reales `play_stop`/`play_start`
(commit `c6e5d14`).

**Dos bugs reales encontrados y arreglados, ninguno detectable con dotnet
build**: (1) `SaveMigrator` migraba `SaveSnapshot.SaveVersion` 2→3 pero no
`ApartmentSaveData.SaveVersion` de cada registro — `TryValidateSnapshot`
rechazaba TODO save real como corrupto, bloqueando toda persistencia, no solo
la de propiedades. (2) `LocalPersistenceProvider.CloneForSave` (lo que
realmente se escribe a disco) nunca copiaba Clock/Fortifications/Missions/
PlayerStates — bug preexistente a esta sesión, invisible hasta ahora.

**Confirmado funcionando de verdad**: reclamo de AbandonedShell (instalar
puerta → instalar armario → claim atómico → rekey automático), alquiler de
Rental (pagar → tenant → credencial), y las 4 reglas de Cliente B del spec
(sin credencial deniega, con credencial abre, revocar deniega, rekey invalida
la llave vieja) — todo vía los gateways reales, con estado de
`property-shell-01`/`property-rental-01` sobreviviendo Stop→Play
byte a byte. `ub_test_all`: 36/36 sin fallos.

**Gap conocido, no perseguido más**: la lista de credenciales del llavero no
sobrevive Stop→Play (probablemente `ApplyKeyrings` corre antes de que
`PlayerInteractor.OnStart` fije el `InventoryId` canónico del jugador — mismo
patrón de bug de orden-de-init ya visto varias veces esta sesión). No bloquea
a jugadores reales: el acceso directo owner/tenant sobre PropertyComponent sí
persiste y es la vía principal.

**Pendiente real** (Tarea #26+): BuildVolume/construcción, Property
Authoring Tool, mapear el primer distrito (11 propiedades reales, no las 2
fixtures de prueba en `-2200,1800/2000,216`), y solo después retomar
armas/IA/economía/vehículo per la instrucción original del usuario.

---

Última actualización: 2026-08-06, pivote de arquitectura pedido por el usuario:
"Antes de continuar con armas, IA o el ciclo nocturno, corrige la arquitectura de
vivienda." Sustituye el modelo de 6 ApartmentComponent hardcodeados por un
sistema general de propiedades (alquiler, habitáculos abandonados reclamables,
parcelas, llaves/permisos, fortificación). Ver "SISTEMA DE PROPIEDADES (V1,
en curso)" más abajo para el estado exacto. El resto del documento (fases 1-11
del housing loop original) sigue vigente como historial — no se ha borrado nada
de lo ya verificado, ver regla "A01-A06 como fixtures de migración".

## SISTEMA DE PROPIEDADES (V1, en curso) — leer esto primero al reanudar

Rama: `integration/wizard-holy-grail`. Commits de esta pasada, en orden:
`cf197c8` (scene: referencias de puerta/stash/spawn ya verificadas antes del
cuelgue del editor, solo faltaba commitear), `9239434` (fundación: PropertyComponent,
PropertySaveData v3, WorldSnapshotService, adapter, PropertyClaimService,
PropertyDoor/DoorAnchor/DoorDefinition/DoorLockComponent sobre Sandbox.Mapping.Door),
`1d08e2f` (Keyring/AccessCredential + PropertyDoor interactuable).

**Todo lo de abajo es solo COMPILA (`dotnet build` 0 errores/0 warnings tras
cada paso) — cero verificación en motor.** El editor de sbox se cerró por
completo a mitad de la Fase 11 anterior (no solo colgado: el proceso ya no
aparece en `Get-Process` y el puerto 7269 no acepta conexiones) y no ha vuelto
a responder en ningún momento de esta pasada pese a comprobarlo repetidamente.
No hay forma de reabrir su GUI de forma autónoma sin que sea una acción
invasiva sobre el escritorio del usuario, así que no se ha intentado.

### Decisión de alcance para esta pasada

La petición completa (11 secciones: PropertyComponent, puertas, llaves,
alquiler, habitáculos abandonados, construcción modular, herramienta de
autoría, primer distrito, migración, pruebas) es multi-sesión por tamaño real.
Regla dura del proyecto: *"Confirma que el cambio cabe en una sola PR"* — así
que en vez de intentarlo todo de una vez sin poder probar nada, se secuenció en
pasos pequeños, cada uno compilando en verde antes de seguir al siguiente, con
commits atómicos separados. Se completaron las secciones 2 (modelo de datos),
3 (puertas sobre `Sandbox.Mapping.Door`, API real confirmada por compilación de
prueba — no adivinada), 4 (llaves/credenciales) y la mitad segura de 1/10
(adapter aditivo, sin tocar la lógica ya probada de `ApartmentClaimService`).

### Por qué no se tocó `ApartmentClaimService` por dentro

El plan pide *"ApartmentClaimService como adapter temporal"*. Reescribir su
lógica interna de claim/save (ya verificada físicamente esta sesión: recogida,
crafting, puerta, barricadas) para que delegue de verdad en
`PropertyClaimService` es exactamente el tipo de cambio de comportamiento que,
en esta misma sesión, ha escondido bugs reales en cada ocasión anterior
(recogida con E, salud de barricada al nacer, guardado nunca disparado) — y
esos bugs solo se encontraron con Play Mode real, nunca con build limpio. Sin
capacidad de probar Stop/Play ahora mismo, arriesgar la única vivienda que ya
funciona no es razonable. En su lugar: `ApartmentClaimService` implementa
`IPropertyAccessPolicy` como delegador puro hacia su propia lógica intacta —
es un adapter real, pero de solo lectura sobre lo ya probado, cero riesgo de
regresión. La reescritura interna real queda para cuando haya editor vivo.

### Hecho (build-verified, no engine-verified)

- **PropertyComponent** (`Code/UltimoBarrio/Properties/PropertyComponent.cs`):
  entidad canónica completa según el spec — ownership, tenancy, co-owners/
  guests, rental state, claim state, anchors de autoría (puertas, ventanas,
  build volume, claim cabinet, stash, respawn), progresión (upgrade/security/
  defense).
- **PropertySaveData + SaveSnapshot v3**: nueva sección `Properties` (con
  `Doors` anidado) y `Keyrings`, compatibles con saves antiguos (deserializan
  con listas vacías). `WorldSnapshotService.Capture/Apply` las escribe/lee
  automáticamente — reutiliza el pipeline de guardado ya arreglado esta
  sesión (mismo punto donde vive el fix de `AutoSaveManager`, ver más abajo).
- **PropertyClaimService**: servicio nativo nuevo para propiedades reales
  (no las 6 fixture). Claim atómico de `AbandonedShell` (consume
  `apartment_door_kit`, con rollback) y alquiler de `Rental` (retira depósito+
  renta del `Wallet`, con rollback), mismo patrón de `_claimGate`/rollback que
  `ApartmentClaimService.TryClaim` ya tiene probado. Fuerza guardado síncrono
  vía `ApartmentClaimService.TrySaveNow()` en el momento del éxito, no solo un
  `RequestSave` async.
- **PropertyDoor/DoorAnchor/DoorDefinition/DoorLockComponent**
  (`Code/UltimoBarrio/Properties/Doors/`): comportamiento físico sobre
  `Sandbox.Mapping.Door` — **confirmado real por compilación de prueba contra
  el DLL del motor** (no por strings/reflexión, que resultó poco fiable en
  este entorno): `IsLocked` (get/set), `Open()/Close()/Toggle()`, `State`
  (`DoorState.Open/Closed/Opening/Closing`), `LinkedDoor`, sonidos. Salud/
  mejora/daño/reparación/breach son capa propia (mismo modelo que
  `ApartmentFortification`). `PropertyDoor` ahora es `IWorldInteractable`: con
  cerradura, exige owner/tenant/co-owner/guest directo o una
  `AccessCredential` vigente antes de desbloquear y abrir; sin cerradura, abre
  y cierra libre para cualquiera.
- **Keyring/AccessCredential** (`Code/UltimoBarrio/Properties/Keys/`):
  `AccessCredential` (PropertyId/LockId/KeyRevision/AccessLevel/Issuer/
  Expiry/Stealable), `KeyringItem` (componente host, ítem "keyring" ya en
  `ItemCatalog`), `KeyringService` (RPCs host: entregar/revocar/duplicar/
  cambiar cerradura — entregar/revocar/rekey exigen ser
  `PropertyComponent.OwnerPersistentId` real, nunca basta con la credencial
  misma; duplicar exige tener ya una credencial válida, no ser el owner).

### Gaps conocidos y explícitamente documentados (no ocultos)

1. **`AutoSaveManager` no está guardado en el `.scene` en disco.** Se añadió
   vía MCP `add_component` a "Apartment Claims" (`5d6c2a82-51f7-4ff1-af79-5d9cbd48d512`)
   justo antes de que el editor se cerrara del todo — el `save_scene` que lo
   habría persistido nunca llegó a ejecutarse. **Casi seguro que se perdió**
   (el editor no solo colgó, el proceso murió). Repetir el `add_component` en
   cuanto el editor responda, ANTES de re-probar persistencia — sin esto, todo
   lo de esta pasada (Properties, Doors, Keyrings) tampoco se autoguardará por
   la misma razón raíz que rompía la Fase 11 original.
2. **`KeyringItem` no está en `player.prefab`.** Mismo patrón de bug ya
   encontrado 4 veces esta sesión (componente escrito y enganchado a call
   sites reales pero nunca colocado). Se crea on-demand vía `GetOrCreate` en
   el primer `RequestGrantAccess`, así que otorgar acceso SÍ funciona en una
   sesión en curso — pero `ApplyKeyrings` no puede restaurar un llavero
   guardado en un jugador que aún no lo tiene al cargar la escena. Añadirlo
   junto a `Wallet` en `player.prefab`.
3. **Nada de esto se ha visto en pantalla.** Ni una puerta física, ni un
   claim de `AbandonedShell`, ni un alquiler, ni una credencial otorgada. Es
   arquitectura real y compilable, no arquitectura probada.

### Tercera pasada (mismo bloqueo, más trabajo solo-código completado)

Editor confirmado seguir sin proceso ni puerto abierto en esta comprobación
también. En vez de seguir sondeando, se avanzó con lo que sí es seguro sin
motor: **Tarea #24 (alquiler) y Tarea #25 (ClaimCabinet) completas**, commits
`14089d2` y `50fc5bd`. Resumen:

- `RentSign` (interactuable físico, dispara `RequestRentProperty`) +
  `RentalService` (tick cada 5s: `Rented`→`GracePeriod`→desahucio si no se
  paga; `RequestRenewRental`/`RequestAbandonRental` con reembolso parcial de
  depósito). `TryRentProperty` ahora también emite una credencial Resident
  (aunque el acceso directo por `TenantPersistentId` ya funciona sin ella —
  la credencial es fidelidad al spec, no la única vía).
- `ClaimCabinetAnchor`/`ClaimCabinetComponent` + ítem `claim_cabinet` y su
  receta. **Corregido un error de modelado real**: `TryClaimAbandonedShell`
  consumía `apartment_door_kit` él mismo, duplicando lo que `DoorAnchor.
  ProcessInstall` ya hace como paso separado anterior — ahora el claim no
  consume nada, exige que puerta Y armario ya estén instalados
  (`DoorNotInstalled`/`CabinetNotInstalled` si no), y al reclamar hace rekey
  de la puerta (invalida credenciales previas al reclamo) y habilita
  BuildVolume/Stash/RespawnAnchor.

**Parada deliberada aquí, no por agotar ideas.** La Tarea #26 (BuildVolume +
12 piezas: preview, snapping, validación de colisión) es fundamentalmente
distinta a todo lo anterior — placement/preview/snapping es lógica espacial
que depende de feedback visual real para no estar simplemente equivocada; el
resto del sistema (máquinas de estado de claim/alquiler/credenciales) se pudo
razonar con precisión sin motor porque es lógica de datos, no geometría. Sin
poder ver un solo frame, seguir apilando código espacial no verificable tiene
más riesgo de arquitectura torcida que valor real. Mismos gaps pendientes que
antes (`AutoSaveManager` y `KeyringItem` en player.prefab).

### Segunda comprobación (30 min después, vía wakeup programado)

`sbox-dev` sigue sin proceso (`Get-Process` vacío) y el puerto 7269 sigue sin
aceptar conexiones (`Test-NetConnection` = False). Confirma que no es un
cuelgue transitorio: el editor necesita que el usuario lo reabra manualmente.
Se detiene el sondeo automático aquí — no tiene sentido seguir comprobando
cada 30 min sin que el usuario sepa que hace falta su acción. Cuando el
usuario reabra el proyecto en el editor, retomar directamente por la lista de
abajo, sin reinvestigar nada.

### Siguiente acción exacta al reanudar (en orden)

1. `editor_status` — si responde, seguir; si no, otra ronda de espera/backoff
   antes de asumir que necesita un reinicio manual.
2. Repetir `add_component id=5d6c2a82-51f7-4ff1-af79-5d9cbd48d512 type=AutoSaveManager`
   (gap #1) y `save_scene`.
3. Añadir `KeyringItem` a `player.prefab` junto a `Wallet` (gap #2), guardar.
4. Colocar en la escena, vía MCP, al menos: 1 `PropertyComponent` tipo
   `AbandonedShell` con su `DoorAnchor` (para probar Tarea #25/claim real por
   primera vez) y 1 `PropertyComponent` tipo `Rental` con una `PropertyDoor`
   ya instalada (para probar Tarea #22/23: abrir con credencial, denegar sin
   ella, rekey invalida). Esto es autoría mínima de prueba, no el primer
   distrito completo (Tarea #28, con sus 11 propiedades, sigue pendiente y es
   más grande).
5. Recorrido físico real: fabricar `apartment_door_kit` → instalar en el
   `DoorAnchor` del AbandonedShell → colocar `ClaimCabinet` (Tarea #25,
   pendiente de implementar) → reclamar → `KeyringService.RequestGrantAccess`
   a un segundo jugador de prueba → confirmar que abre con credencial y no sin
   ella → `RequestRekeyDoor` → confirmar que la credencial vieja ya no sirve →
   Stop/Play → confirmar que todo (incluida la propiedad, la puerta y el
   llavero) sobrevive.
6. Solo tras cerrar 1-5: continuar con Tarea #24 (flujo de alquiler completo:
   `RentSign` interactable, renovación, impago, desahucio — el primitivo
   `TryRentProperty` ya existe pero el flujo de UI/cartel no), Tarea #26
   (BuildVolume + colección de 12 piezas), Tarea #27 (Property Authoring
   Tool), Tarea #28 (mapear el primer distrito real: 4 Rental + 4
   AbandonedShell + 1 Garage + 1 Shop + 1 BuildPlot + 1 GroupBase).
7. Solo entonces retomar armas/IA nocturna/economía-misiones/vehículo (las
   fases originales 12-15), como pidió el usuario explícitamente al abrir
   este bloque: *"Antes de continuar con armas, IA o el ciclo nocturno,
   corrige la arquitectura de vivienda."*

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
