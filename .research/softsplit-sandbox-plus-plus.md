# Softsplit/sandbox-plus-plus

- **Nombre:** sandbox-plus-plus
- **URL:** https://github.com/Softsplit/sandbox-plus-plus
- **Revisión exacta:** `979990901e3d016b6f92978a36ad5189557f8658`
- **Fecha:** 2026-06-28
- **Licencia de código:** **GPL-3.0** (confirmado, texto completo de la GNU GPL v3 en
  `LICENSE`)
- **Licencia de assets:** No verificada por separado — dado el veredicto (no se copia
  código), no es relevante para esta pasada.
- **Atribución:** N/A — no se copia código de este repositorio.
- **Código fuente disponible:** Sí, completo (clonado en
  `.research/vendor/sandbox-plus-plus/`)
- **Sistema que aporta:** Herramientas de construcción/placement estilo GMod — constraints,
  duplicador, rotación, snapping, undo, patrones de permisos.
- **Dependencias:** N/A — no se adopta código.
- **Archivos concretos útiles:** Ninguno para copia directa. Como **referencia de lectura**
  para el flujo UX (herramienta selecciona ancla → preview → confirma → snap/rotate) que
  necesita la colocación de barricadas.
- **Riesgos:** **Alto si se copia código** — GPL-3.0 es copyleft: cualquier código derivado
  distribuido tendría que relicenciarse bajo GPL-3.0 también, lo cual no es una decisión
  que se pueda tomar implícitamente en un commit de gameplay. No confundir con
  `Nebual/sandbox-plus` (repo distinto, también MIT, ya fichado por separado en
  `.research/nebual-sandbox-plus.md`).
- **Veredicto:** EXTRAER PATRÓN únicamente — **no copiar ni una línea de código**. Usar solo
  como referencia de diseño de interacción para el Bloque C (barricadas/mejoras),
  reimplementado desde cero contra `BarricadeAnchor` local.
