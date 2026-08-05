# Nebual/sandbox-plus

- **Nombre:** sandbox-plus
- **Fuente:** https://github.com/Nebual/sandbox-plus
- **Commit/tag/revisión:** main @ 741 commits (SHA no fijado en esta pasada)
- **Última actualización:** no confirmada — algunos addons marcados "outdated" en
  el propio README, indicando desarrollo activo mudado a otras partes del proyecto
  más que abandono total.
- **Licencia:** MIT
- **Código fuente disponible:** Sí
- **Sistema que aporta:** Fork comunitario del gamemode Sandbox de Facepunch con
  tool de constraints al estilo GMod (weld/axis/rope/elastic/slider/ballsocket),
  spawnmenu de formas dinámicas (DynShapes), sistema de eventos extendido
  (`entity.spawned`, `undo.add`).
- **API de s&box utilizada:** Basado en el gamemode Sandbox oficial reescrito para
  el sistema de escenas — API relativamente actual dado el uso activo declarado.
- **Compatibilidad probable:** Media-Alta para el patrón de interacción de
  constraints (selección de ancla, preview, confirmación) — no para el gamemode
  completo, que no aplica al diseño de Último Barrio.
- **Archivos concretos útiles:** El tool de constraints como referencia de UX
  (herramienta selecciona punto → preview válido/inválido → confirma) — el patrón
  que necesita la colocación de barricadas (ver `ultimo-barrio-gameplay` paso de
  "seleccionar kit → apuntar a BarricadeAnchor → preview → colocar").
- **Dependencias:** Ninguna declarada relevante.
- **Conflictos con nuestro proyecto:** El gamemode completo (spawnmenu, addons
  tipo Wirebox) no tiene cabida en Último Barrio — solo el patrón de interacción
  de la constraint tool es relevante, no el sistema.
- **Riesgo de networking:** Bajo si solo se extrae el patrón de UX.
- **Riesgo de persistencia:** Ninguno — no se toca su modelo de datos.
- **Trabajo de integración:** Ninguno directo — sirve como referencia de diseño
  para implementar la colocación de barricadas localmente.
- **Veredicto:** EXTRAER PATRÓN — usar como referencia de UX para el flujo de
  colocación de barricadas (bloque G), implementado de cero contra
  `BarricadeAnchor` local, no como dependencia.
