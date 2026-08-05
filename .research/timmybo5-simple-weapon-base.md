# timmybo5/simple-weapon-base

- **Nombre:** Simple Weapon Base (SWB)
- **Fuente:** https://github.com/timmybo5/simple-weapon-base
- **Commit/tag/revisión:** master @ 619 commits (SHA no fijado en esta pasada)
- **Última actualización:** no confirmada en esta pasada
- **Licencia:** MIT (confirmado en el footer del repo)
- **Código fuente disponible:** Sí, completo
- **Sistema que aporta:** Base de armas — hitscan y físico (balístico), recarga por
  cargador y por cartucho, animaciones (andar/agachado/correr/saltar/idle/sway/aim),
  soporte de mando con aim assist, sistema de attachments apilables con menú de
  personalización, HUD con retícula dinámica, scope 2D para francotirador, editor de
  offsets de modelo. Deliberadamente sin gestión de inventario.
- **API de s&box utilizada:** No especificada explícitamente en el fetch — requiere
  clonar y revisar imports para confirmar contra el motor 26.07.22 instalado antes
  de cualquier adopción real.
- **Compatibilidad probable:** Media — sin inventario propio, tendría que conectarse
  al `InventoryComponent`/`ItemRegistry` local, que no es su modelo de datos nativo.
- **Archivos concretos útiles:** Pendiente de identificar tras clonar — candidatos
  probables: lógica de sway/aim, sistema de attachments, offset editor tool.
- **Dependencias:** Ninguna declarada relevante más allá del motor.
- **Conflictos con nuestro proyecto:** Su modelo de progresión de attachments y su
  propio sistema de slots no coincide con `ItemDefinition`/`InventorySlot.AmmoInMag`
  (el proyecto ya tiene persistencia de cargador por slot que SWB no necesita
  resolver).
- **Riesgo de networking:** Bajo — probablemente asume el mismo modelo
  host-autoritativo estándar de s&box, pero no verificado.
- **Riesgo de persistencia:** Ninguno relevante — SWB no persiste inventario.
- **Trabajo de integración:** Medio-Alto si se adopta para el bloque I (armas) —
  requeriría adaptar attach points, sway, y magazine handling al `BaseCombatWeapon`
  local en vez de reemplazarlo.
- **Veredicto:** EXTRAER PATRÓN — no adoptar como base completa (el proyecto ya
  tiene su propio `Combat.BaseCombatWeapon` funcional con fire/reload/ammo pool/RPC).
  Revisar de nuevo con ficha completa cuando se aborde el bloque I para tomar ideas
  concretas de sway/aim/attachments, no el sistema entero.
