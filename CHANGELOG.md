# Changelog

El formato se inspira en Keep a Changelog y el proyecto utiliza versionado semántico cuando sea aplicable.

## [Unreleased]

### Added

- Visión inicial del juego.
- Arquitectura propuesta.
- Política de assets.
- Flujo de trabajo con agentes.
- Plan de una semana.
- Plantillas de issues y PR.
- Registro operativo para reanudar sesiones.
- Baseline Git local del starter pack con tag `bootstrap-v0.0.0`.
- Preflight M0 de solo lectura para Git, estructura del proyecto, escena y MCP.
- Handoffs únicos por tarea para conservar evidencia entre agentes.
- Proyecto s&box `Game → Empty` con manifiesto, escena mínima, ensamblados de
  runtime/editor y configuración inicial del proyecto.

### Changed

- El flujo paralelo reserva `STATE.md` y `CHANGELOG.md` al integrador para
  evitar conflictos entre agentes.
- El prompt de bootstrap limita cada ejecución a una tarea verificable de M0.
- El `.gitignore` sigue la política oficial de s&box: conserva fuentes y
  `ProjectSettings/`, y excluye estado local, proyectos generados y assets
  compilados.

## [0.0.0] - 2026-08-02

### Added

- Bootstrap documental del repositorio.
