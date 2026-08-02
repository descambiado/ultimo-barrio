# Handoff de sesión

## Contexto

- Fecha: 2026-08-02
- Agente/persona: Codex + subagentes
- Rama: `feat/m0-bootstrap`
- Issue: M0-01 — crear proyecto Empty
- Commit de implementación: `a3084b0` (`chore: integrate sbox empty project`)

## Resultado

- Qué se completó: se creó `ultimo_barrio` desde la plantilla oficial
  `Game → Empty`, se integraron únicamente sus fuentes y configuraciones
  reproducibles y se abrió el proyecto resultante desde la raíz del repositorio.
- Qué no se completó: no se configuró `origin` ni se creó `main.scene`; son las
  tareas M0-02 y M0-03. No se modificó la escena mínima de la plantilla.
- Estado de compilación: **CORRECTA**. `local.ultimo_barrio` y
  `local.ultimo_barrio.editor` indican `Success=true`, 0 errores y 0 avisos.
- Estado de Play Mode: **NO EJECUTADO**. M0-01 valida creación, apertura y
  compilación; el smoke test de juego corresponde a la escena principal.
- Prueba multijugador: **NO EJECUTADA**. Pendiente de M0-05.
- Prueba de guardado: **NO EJECUTADA**. Fuera del alcance de M0-01.

## Archivos principales

- `ultimo_barrio.sbproj`: manifiesto generado por s&box.
- `Assets/scenes/minimal.scene`: escena editable de la plantilla.
- `Code/Assembly.cs` y `Editor/Assembly.cs`: ensamblados fuente iniciales.
- `ProjectSettings/*.config`: configuración inicial versionada.
- `.gitignore`: política oficial para excluir estado local y artefactos
  generados sin ocultar fuentes, settings ni librerías vendorizadas.

## Decisiones

- Versionar `.sbproj`, `Assets/`, `Code/`, `Editor/`, `ProjectSettings/` y el
  contenido futuro de `Libraries/`.
- Excluir `.sbox/`, `.vscode/`, proyectos C# generados, `*.slnx`,
  `Properties/launchSettings.json` y assets compilados `*.*_c`, conservando la
  excepción oficial `!*.shader_c`.
- Mantener M0-01 atómico: no crear todavía roots, sistemas ni jugador.

## Bugs conocidos

- `origin` no está configurado y `Assets/scenes/main.scene` no existe; el
  preflight los informa como los dos únicos pendientes, sin fallos.
- El manifiesto conserva valores de plantilla: `Org: local`, máximo de 64
  jugadores y `facepunch.flatgrass`. Deben revisarse antes de publicar y de la
  validación multijugador 1–4.
- La consola contiene avisos globales de recursos stock del motor, pero no
  devuelve entradas de nivel Error y los compiladores del proyecto tienen 0
  avisos.

## Próximas tres acciones

1. Completar M0-02 configurando el repositorio público, `origin`, ownership y
   el canal privado de seguridad.
2. Completar solo M0-03 creando y guardando `Assets/scenes/main.scene` mediante
   el editor/MCP.
3. Tras validar M0-03, abordar M0-04: jugador, movimiento y cámara.

## Cómo reproducir

1. Abrir `ultimo_barrio.sbproj` desde la raíz del repositorio con s&box y
   comprobar que el MCP responde en `http://127.0.0.1:7269/mcp`.
2. Ejecutar
   `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-repo.ps1`;
   debe mostrar `Repository structure OK.`
3. Ejecutar
   `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/check-m0-preflight.ps1`;
   mientras M0-02/M0-03 sigan pendientes debe resumir
   `PASS=7 PENDING=2 FAIL=0`.

## Evidencias

- Editor: `scenes/minimal.scene` activa, 9 objetos raíz y ningún cambio sin
  guardar.
- Compilación: runtime y editor, 0 errores y 0 avisos.
- Consola: ninguna entrada de nivel Error entre 38 entradas almacenadas.
- Git: `git diff --cached --check` limpio antes del commit de implementación.
