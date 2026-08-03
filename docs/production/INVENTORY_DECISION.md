# Decision Record — Sistema de Inventario (Alpha 0.1)

## Contexto
Se ha evaluado la API de inventario oficial de s&box (`BaseInventoryComponent`) en comparación con nuestra implementación determinista `InventoryComponent`.

## Criterios Evaluados
1. **Multiplayer / Réplica en Red**: Requisito de inventarios separados por SteamId con validación autoritativa en el Host.
2. **Transferencias Mochila <-> Alijo**: Operación atómica de transferencia entre inventarios (`RequestTransfer`).
3. **Stacks y Slots**: Soporte para slots fijos, stacking por `ItemId` y `Amount`.
4. **Persistencia**: Serialización limpia a JSON con CRC64 e IDs deterministas (`player:{steamId}:inventory` / `apartment-a0X:stash`).

## Decisión
**Mantener y Adaptar nuestro `InventoryComponent` determinista.**

### Justificación
- `InventoryComponent` proporciona sincronización autoritativa limpia en s&box Scene System.
- Ofrece separación estricta: `InventoryComponent` **no implementa `IWorldInteractable`**, impidiendo estructuralmente que mirar a otro jugador exponga su almacenamiento.
- `StashComponent` envuelve `InventoryComponent` para exponer la interfaz `IWorldContainer` e `IWorldInteractable` únicamente en contenedores físicos del mundo.
