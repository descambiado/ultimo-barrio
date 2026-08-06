# kurozael/sbox-inventory

- **Nombre:** sbox-inventory (Conna)
- **Fuente:** https://github.com/kurozael/sbox-inventory
- **Commit/tag/revisión:** `acbfc511d47e7703c6739f7d2c8d7cea0aee07a2` (confirmado tras clonar en `.research/vendor/sbox-inventory/`)
- **Última actualización:** 2026-01-23
- **Licencia:** MIT
- **Código fuente disponible:** Sí — `BaseInventory`, `InventoryItem`,
  `NetworkedInventory` y ejemplos de UI con drag-and-drop
- **Sistema que aporta:** Inventario tipo Tetris (grid 2D) con tamaños de ítem
  variables, apilado, sincronización automática entre clientes, host-autoritativo.
- **API de s&box utilizada:** No confirmada específicamente sin clonar.
- **Compatibilidad probable:** Media — el modelo de grid 2D es más complejo que lo
  que el diseño de Último Barrio pide (slots simples de stack, no colocación libre
  en grilla — ver `InventorySlot` local: `ItemId`/`Amount`/`AmmoInMag`, sin
  coordenadas de grid).
- **Archivos concretos útiles:** El patrón de sincronización host-autoritativa
  (`NetworkedInventory`) es la parte más transferible; el modelo de datos de grid no.
- **Dependencias:** Ninguna declarada más allá del motor.
- **Conflictos con nuestro proyecto:** El `InventoryComponent` local ya resuelve
  sincronización vía `[Sync] NetList<InventorySlot>` con transferencias
  host-validadas (`RequestTransfer`) y anti-cheat de distancia — adoptar el sistema
  de Conna significaría remodelar todo el esquema de datos (incluyendo el munición-
  por-slot ya persistido) sin ganancia clara para el diseño actual de slots simples.
- **Riesgo de networking:** Bajo si solo se toma el patrón, no el código.
- **Riesgo de persistencia:** Alto si se adoptara completo — migrar de slots planos
  a grid 2D rompería los saves de apartamento/inventario ya en disco.
- **Trabajo de integración:** Alto si se adoptara completo; bajo si solo se toma
  el patrón de sincronización como referencia (ya está esencialmente replicado en
  el `InventoryComponent` local).
- **Veredicto:** EXTRAER PATRÓN (parcial) — no adoptar el modelo de grid. El
  patrón de sincronización ya está efectivamente presente en el código local; esta
  ficha existe para dejar constancia de que se buscó y se comparó, no para marcar
  trabajo pendiente.
