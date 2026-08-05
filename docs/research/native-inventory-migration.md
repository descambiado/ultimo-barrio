# Auditoría: arquitectura local vs sistemas nativos de s&box

Comparación de `InventoryComponent`/`ItemRegistry`/`HeldItemController`/
`WorldItemPickup`/`HotbarPanel` contra `Sandbox.BaseInventoryComponent`,
`Sandbox.BaseInventoryItem`, `Sandbox.BaseCombatWeapon` y el patrón
`PickupBehaviour.Use` del motor 26.07.22 instalado.

## Resumen ejecutivo

**No se recomienda una migración a los tipos base nativos.** La arquitectura local
ya es host-autoritativa, ya tiene un pipeline de interacción con prompts
(`IWorldInteractable` → `PlayerInteractor` → `Input.Pressed("Use")`), y ya persiste
munición por slot de forma que sobrevive a equipar/soltar/recoger. Sustituir esto
por `BaseInventoryComponent`/`BaseCombatWeapon` sería reescribir un sistema que
funciona por uno que tendría que volver a aprender los mismos requisitos concretos
del proyecto (stash por apartamento, transferencias con anti-cheat de distancia,
cargador persistente), sin evidencia de que resuelva algo que falte.

Este es exactamente el caso que [[sbox-reuse-first]] contempla como salida válida:
la búsqueda no encontró un vacío que llenar, encontró un sistema ya construido que
cubre el mismo terreno. La migración recomendada es **selectiva y de patrón**, no
de reemplazo.

## Hallazgo importante: la recogida con E ya existe

El bloque de trabajo original asumía que había que "arreglar recogida real con E".
Auditando el código actual, **el pipeline ya existe y es real, no un placeholder**:

```
PlayerInteractor.OnUpdate()
  → Scene.Trace.Ray(...) hacia donde mira PlayerController.EyeAngles
  → busca IWorldInteractable en el objeto golpeado
  → muestra prompt vía PlayerHud
  → Input.Pressed("Use") → interactable.OnInteract(req)

WorldItemPickup.OnInteract()
  → si es proxy, RPC a host (RequestPickupOnHost)
  → si es host, ProcessPickup(): valida distancia, resuelve InventoryComponent
    del interactor, gestiona ResourceNode (recolectables) o item plano,
    inventory.AddItem(...), destruye el pickup, notifica por RPC broadcast
```

Esto ya cumple el criterio de [[sbox-runtime-proof]] de "input físico real, no
comando QA" — el flujo entero pasa por `Input.Pressed`, no por un `ConCmd`. Lo que
falta **no es construir el sistema**, es:

1. Verificarlo físicamente en juego (ver sección de verificación abajo).
2. Confirmar que los materiales iniciales (`scrap_metal`, `wood`, `cloth`,
   `components`, `water`, `medicine`, `ammo_9mm`) tienen `WorldItemPickup`
   colocables en el mapa con modelos reales, no cubos.

## Comparación componente por componente

### InventoryComponent vs Sandbox.BaseInventoryComponent

| | Local (`InventoryComponent`) | Nativo (`BaseInventoryComponent`) |
|---|---|---|
| Modelo de datos | `NetList<InventorySlot>` — `ItemId`/`Amount`/`AmmoInMag` | Lista de `BaseInventoryItem`, con `SlotOrder`/`PreferredSlot` |
| Autoridad | `[Rpc.Host]` en `RequestTransfer`/`RequestDrop`, `TryAdd`/`TryRemove` bloqueados a `IsProxy` | Pickup/holster nativo, no expone el mismo modelo de transferencia entre contenedores |
| Transferencias entre contenedores | Sí — `RequestTransfer` con validación de distancia y política de acceso a stash (`IApartmentAccessPolicy`) | No es su caso de uso — pensado para inventario de un solo jugador (armas/objetos que se llevan), no para stashes compartidos con reglas de propiedad |
| Persistencia de munición por slot | Sí — `AmmoInMag` sobrevive cambio de slot/drop/pickup | No aplica directamente — el pooling de munición nativo (`GetAmmo`) es compartido por tipo de munición, no por instancia de arma en un slot concreto |
| Peso/penalización de movimiento | Sí — `GetTotalWeight()` alimenta `PlayerMovementModifier` | No — no forma parte de `BaseInventoryComponent` |

**Veredicto: conservar el local.** Ninguna de las necesidades específicas del
proyecto (stash compartido con permisos, cargador persistente por slot, peso) las
resuelve el tipo nativo sin construir por encima una capa equivalente a lo que ya
existe.

### HeldItemController vs BaseCombatWeapon + BaseInventoryComponent equip flow

El `HeldItemController` local ya hace lo que el flujo nativo de equip/holster
haría: cachea el `GameObject` del arma por slot host-side, restaura el cargador
desde el inventario al equipar, bloquea input mientras la UI captura, gestiona
puños como fallback. No hay hueco que un tipo nativo llene aquí sin duplicar esta
lógica.

**Veredicto: conservar el local.**

### WorldItemPickup vs PickupBehaviour.Use

`PickupBehaviour.Use` (enum de `BaseInventoryComponent`) es la marca nativa de
"este pickup requiere una acción de uso, no solo tocarlo". El `WorldItemPickup`
local implementa exactamente ese comportamiento (`Use`, no `Touch`) a mano, con
prompt dinámico según si es arma/consumible/recurso.

**Veredicto: conservar el local**, ya replica el comportamiento `Use` sin
depender del tipo base. No hay razón para adoptar `BaseInventoryComponent` solo
para heredar el enum `PickupBehaviour` cuando el comportamiento ya está
implementado.

### HotbarPanel

No tiene equivalente nativo directo (`BaseInventoryComponent` no impone una UI de
hotbar). Se mantiene sin cambios — ver el fix de esta sesión sobre
`Mouse.Visibility` que corrigió que la hotbar capturara el cursor permanentemente.

### BaseCombatWeapon local vs Sandbox.BaseCombatWeapon nativo

La forma es casi idéntica a propósito: `Fire`/`Reload`/`CurrentAmmo`/`IsReloading`,
RPC de disparo (`Rpc.Owner`/`Rpc.Host`), traza de bala, aplicación de daño vía
`IDamageable`. Esto sugiere que el `BaseCombatWeapon` local ya se diseñó tomando el
patrón nativo como referencia (extracción de patrón exitosa de una sesión
anterior), solo que con el pooling de munición resuelto contra
`InventoryComponent.TryRemove(AmmoType, ...)` en vez del pooling nativo por
`BaseInventoryComponent.GetAmmo()`.

**Veredicto: conservar el local**, sin acción — es ya el resultado correcto de
haber extraído el patrón nativo anteriormente, no necesita revertirse a la base.

## Qué código local sobra

Ninguno de los componentes auditados sobra. No se identificó duplicación entre
sistemas locales que resuelven el mismo problema dos veces — el catálogo de
reutilización (parte 1) tampoco encontró un tipo nativo cuya adopción elimine
código local sin pérdida de funcionalidad requerida.

## Qué necesita un adapter temporal

Nada, por la razón anterior: no hay una migración de tipo A→B en marcha, así que no
hay una capa de compatibilidad que mantener durante una transición.

## Cómo evitar perder saves

No aplica en este momento — no se está tocando el esquema de `InventorySlot` ni de
`ApartmentComponent`. Si en el futuro un candidato justifica adoptar un modelo de
datos distinto (por ejemplo, pasar a inventario tipo grid si el diseño cambia),
la migración debe seguir el patrón ya usado en el proyecto para casos similares —
ver `Code/UltimoBarrio/Persistence/AutoSaveManager.cs` y el histórico de
"migrate legacy apartment save without data loss" (`b4281ac`) como precedente de
cómo se hizo antes: migración incremental que lee el formato viejo y lo reescribe
al nuevo sin perder datos, nunca un reset de save.

## Cómo conectar hotbar al inventario nativo

No aplica — la hotbar ya está conectada al `InventoryComponent` local
(`HotbarPanel.TargetInventory`), que es la fuente de verdad correcta dado que no se
está migrando el modelo de inventario.

## Cómo recoger realmente con E

Ya funciona por diseño (ver hallazgo arriba) — pendiente de **verificación física**,
no de implementación. Ver sección siguiente.

## Cómo transferir entre jugador y stash

Ya implementado: `InventoryComponent.RequestTransfer` con `[Rpc.Host]`, validación
de distancia (400 unidades) y política de acceso vía `IApartmentAccessPolicy`
(`StashComponent`). Pendiente de verificación física del flujo UI completo
(abrir stash con E → arrastrar/transferir en `InventoryUI` → confirmar en ambos
inventarios).

## Cómo convertir armas en tipos nativos reales

No se recomienda por ahora (ver comparación `BaseCombatWeapon` arriba) — el local
ya sigue el mismo patrón de diseño. Cuando se aborde el bloque I (armas después del
loop de vivienda), revisar `timmybo5/simple-weapon-base`
(`.research/timmybo5-simple-weapon-base.md`) para ideas de sway/aim/attachments
sobre la base local existente, no para reemplazarla.

## Verificación pendiente (bloque D)

Siguiendo [[sbox-runtime-proof]], antes de declarar "recogida con E" en estado
FUNCIONA EN RUNTIME:

1. Colocar al menos un `WorldItemPickup` real en el mapa (`scrap_metal` con modelo
   verificado, no cubo) — ver `docs/world/first-district-manifest.md`.
2. En Play Mode, mirar el pickup, confirmar que aparece el prompt "Pulsa E".
3. Pulsar E físicamente, confirmar que el inventario cambia en el HUD y el objeto
   desaparece del mundo.
4. Confirmar que el host rechaza la recogida si la distancia excede
   `MaxInteractionDistance` (probar alejándose antes de pulsar E).
