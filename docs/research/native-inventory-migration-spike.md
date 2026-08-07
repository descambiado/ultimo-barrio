# Spike de migración: `InventoryComponent` (rama) → `Sandbox.Inventory` (primera parte)

**Estado:** SPIKE ABIERTO — **no implementar la migración en esta rama** (`spike/laptop-content-stack`).
**Fecha:** 2026-08-07
**Base:** `feat/holy-grail-foundation` @ c9e5664. El core del otro PC ya implementa sobre el `InventoryComponent` actual: pickup, stack, drop/repickup, stash, crafting, persistencia y housing.
**Objetivo:** comparar el inventario actual contra la API nativa de s&box (`Sandbox.Inventory`, update 26.07.08) para que el core nuevo (`integration/wizard-holy-grail`) decida con datos — no por inercia ni por moda — y con estrategia de rollback.

---

## 1. Qué hay hoy (`InventoryComponent` de la rama)

Fuente: `Code/UltimoBarrio/Inventory/InventoryComponent.cs` (leído completo, commit base c9e5664).

- `InventorySlot { ItemId, Amount }` — slot = item + cantidad (stacking aritmético).
- `[Sync] NetList<InventorySlot> Slots` — sincronización de red por lista de slots.
- `MaxSlots = 24`, `HotbarSlots = 6` (properties).
- API: `CanAdd`, `TryAdd`, `TryRemove`, `GetCount` — solo el host muta (`IsProxy` bloquea en cliente).
- `[Rpc.Host] RequestTransfer(itemId, amount, targetInventoryId)` — mover entre inventarios (stash/cofre): `Scene.GetAllComponents<InventoryComponent>()` + validación de distancia/ownership comentada (TODO).
- `[Rpc.Host] RequestDrop(itemId, amount)` — spawn de pickup prefab (`ResourceLibrary.Get<PrefabFile>` + `SceneUtility.GetPrefabScene().Clone()` + `NetworkSpawn`), mapeo itemId→prefab hardcodeado (weapon_usp, ammo_9mm, scrap).
- Dependencias: interfaz `IInventory` (contrato), `WorldItemPickup`, prefabs `prefabs/items/pf_*_pickup.prefab`.

## 2. Qué ofrece `Sandbox.Inventory` (primera parte, update 26.07.08)

Fuente: `docs/research/workers/inventory-ui.md` §1 y §6 (sbdm). ⚠️ **Firma exacta de la API no inspeccionada** — requiere editor abierto o sbox.game/api; todo lo no confirmado va marcado ⚠️.

- Componente de inventario basado en slots de primera parte; items como **child GameObjects**.
- **Host-authoritative**: los clientes solicitan, el host valida.
- Rastrea el item **activo** y lo habilita/deshabilita al cambiar de slot (modelo oficial de hotbar/equipación).
- Items basados en `Sandbox.BaseInventoryItem`.
- UI de referencia oficial: `UI/Inventory.razor` del sbdm (hasta 5 slots, hover, activo/anterior, input de cambio de slot).

## 3. Comparación

| Dimensión | `InventoryComponent` (rama) | `Sandbox.Inventory` (nativa) | Impacto |
|---|---|---|---|
| **Features** | slots + stacking + transfer + drop | slots + stacking + active item + equipación | La nativa cubre el núcleo; stash/crafting/persistencia/housing siguen siendo propios igualmente |
| **Network authority** | host muta + `[Sync] NetList` | host-authoritative nativo (clientes solicitan) | ✅ Mejora: validación central en el framework |
| **Slots** | 24 fijos (property) | slot-based nativo; capacidad configurable ⚠️ | Bajo |
| **Stacking** | aritmético por slot (ItemId+Amount) | nativo ⚠️ (semántica a confirmar) | Bajo-medio |
| **Containers (stash)** | manual: `RequestTransfer` por GUID de inventario | ⚠️ no confirmado si el API cubre contenedores; si no → capa propia | Medio |
| **Ammo** | ítem `ammo_9mm` como stack desvinculado del arma | API de munición de primera parte (mismo update) | Medio: re-mapear ammo a la API de munición ⚠️ |
| **Active item** | no existe aquí (el core lo resuelve aparte: `HeldItemController`) | nativo (activa/desactiva al cambiar slot) | ✅ Elimina `HeldItemController` para armas |
| **Crafting bridge** | el core consume `IInventory` (TryAdd/TryRemove) | requiere envolver la API nativa en un adapter | Medio |
| **Save migration** | persistencia propia v1 (JSON) sobre slots | ⚠️ la nativa no incluye persistencia → hay que migrar el formato de save | **Alto** si el save está acoplado a `InventorySlot` |
| **Hotbar bridge** | `HotbarSlots = 6` (property sin lógica) | nativo + UI de referencia (sbdm) | Bajo: rehacer UI sobre API nativa |
| **Multiplayer** | `NetList` por slots | host-authoritative + child GameObjects networked | Medio: revisar proxying de items |
| **Migration cost** | — | — | **ALTO**: toca pickup/stash/crafting/persistencia/housing del core del otro PC; no es un swap local |
| **Rollback strategy** | — | — | Mantener `IInventory` como frontera: el core puede implementar la interfaz sobre la API nativa y volver al componente actual si el spike falla (feature-flag) |

## 4. Huecos que la API nativa NO cubre (siguen siendo propios del core)

- Persistencia (save/load del mundo).
- Stash / housing ownership.
- Crafting (consumo/producción).
- Loot tables (el core ya tiene las suyas; los content packs definen las suyas aparte).
- Monedero / economía.

## 5. Criterios de decisión para el core nuevo (cuando haya editor en el sobremesa)

1. Comprobar la firma real de `Sandbox.Inventory` (sbox.game/api o editor abierto).
2. Verificar si la API nativa cubre contenedores (stash) y munición.
3. Prototipo mínimo: pickup → stack → hotbar → drop → stash con la API nativa, con el core actual detrás de `IInventory`.
4. Si el prototipo cubre pickup/stack/drop/hotbar/stash sin fricción → migrar. Si no → seguir con `InventoryComponent` y envolver la API nativa solo para equipación de armas (active item).
5. Rollback: feature-flag `USE_NATIVE_INVENTORY`; ambas implementaciones detrás de `IInventory`.

**Regla de esta rama:** no implementar nada de esto aquí. Solo documentar y portar el documento.
