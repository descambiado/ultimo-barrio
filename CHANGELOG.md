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
- Portada humana del proyecto con una captura real del editor y un diagrama
  original del ciclo de juego.
- Escena `Assets/scenes/main.scene` con roots `World`, `Systems`,
  `SpawnPoints` y `Debug`, suelo provisional, iluminación, cielo y punto de
  aparición.
- Prefab oficial `Player Controller` y cámara provisional para el primer boot
  local.
- Captura real `docs/media/first-boot.png` con el jugador visible durante Play
  Mode.
- Especificación previa de M1-01 con contrato, autoridad del host, riesgos de
  red y estrategia de guardado versionado; no incluye implementación.
- Repositorio público `descambiado/ultimo-barrio` con `main`,
  `feat/m0-bootstrap` y el tag histórico `bootstrap-v0.0.0` publicados.
- Draft PR `#7` para el bootstrap y seis issues iniciales para M0-03, M0-04,
  M1-01 y los spikes de mapa, armas y guardado.
- Spawn de jugador por conexión mediante el componente oficial
  `Sandbox.NetworkHelper`, sin Player directo duplicado en la escena.
- Prueba local real con editor host y segundo `sbox.exe`: dos jugadores,
  movimiento/cámara independientes y desconexión verificada.

### Changed

- El flujo paralelo reserva `STATE.md` y `CHANGELOG.md` al integrador para
  evitar conflictos entre agentes.
- El prompt de bootstrap limita cada ejecución a una tarea verificable de M0.
- El `.gitignore` sigue la política oficial de s&box: conserva fuentes y
  `ProjectSettings/`, y excluye estado local, proyectos generados y assets
  compilados.
- El proyecto usa `scenes/main.scene` como escena de inicio.
- El primer Play Mode de `main.scene` inició y se detuvo con compilación limpia
  (0 errores y 0 avisos) y sin errores de consola. La prueba manual del
  2026-08-03 valida aparición, WASD, salto, ratón y cambio entre tercera y
  primera persona. La sesión con dos clientes sigue pendiente.
- Los spikes de mapa y guardado tienen resultados de investigación publicados:
  blockout propio y pequeño para la primera manzana, y persistencia local
  host-only mediante generaciones inmutables sobre `FileSystem.Data`.
- M0-04 queda funcionalmente validado. El smoke test conserva como seguimientos
  el spawn solapado observado y los errores de recursos del cliente dev.

## [0.0.0] - 2026-08-02

### Added

- Bootstrap documental del repositorio.
