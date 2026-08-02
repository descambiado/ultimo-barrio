# Flujo de trabajo con agentes

## Objetivo

Usar agentes para acelerar trabajo verificable, no para generar miles de líneas sin control.

## Roles

### Integrador

- Mantiene arquitectura.
- Divide issues.
- Revisa PR.
- Resuelve conflictos.
- Protege `main`.
- Decide alcance.

### Player/Network Agent

- Jugador.
- Input.
- Cámara.
- Sesiones.
- Ownership.
- RPC.
- Join-in-progress.

### World/Persistence Agent

- Fases.
- Apartamentos.
- Guardado.
- Migraciones.
- Economía.

### AI/Raid Agent

- Percepción.
- Utility.
- Navegación.
- Director.
- Objetivos.

### UI/Audio Agent

- HUD.
- Tutorial.
- Feedback.
- Audio.
- Accesibilidad.

### QA/Performance Agent

- Pruebas.
- Consola.
- Reproducciones.
- Métricas.
- Exploits.
- Regresiones.

## Worktrees

Ejemplo:

```bash
git worktree add ../ub-player -b feat/player-m0
git worktree add ../ub-world -b feat/world-m0
git worktree add ../ub-ai -b feat/ai-spike
git worktree add ../ub-qa -b test/m0
```

Cada worktree abre su propia rama. Evitar abrir dos copias del editor sobre el mismo directorio.

## Unidad de trabajo

Una tarea ideal:

- 1 objetivo.
- 1 responsable.
- 1–5 archivos principales.
- 1 prueba clara.
- Menos de una jornada.
- Rollback sencillo.

## Handoff

Cada sesión deja:

- Commit.
- Estado de compilación.
- Pasos de prueba.
- Captura o logs.
- Riesgos.
- Próxima acción.
- `STATE.md` actualizado.

## Prompts

No usar:

> Haz todo el juego.

Usar:

> Implementa el issue M0-03. Lee los archivos indicados. Inspecciona la escena con MCP. No edites fuera de X. La prueba termina cuando Y. Actualiza STATE.md.

## Herramientas MCP propias futuras

- `validate_main_scene`
- `list_networked_objects`
- `create_apartment_shell`
- `create_breach_point`
- `validate_asset_registry`
- `run_two_client_smoke_test`
- `capture_gameplay_screenshot`
- `report_console_errors`

Los nombres se consideran API pública para agentes; no renombrarlos sin migración.

## Control de calidad

Después de cada cambio:

1. Hotload.
2. Consola.
3. Play Mode.
4. Prueba del criterio.
5. Segundo cliente si toca red.
6. Guardar/cargar si toca persistencia.
7. Captura.
8. Diff.
9. Commit.

## Recuperar contexto semanas después

Pegar `prompts/RESUME_PROJECT_PROMPT.md` y permitir al agente leer:

- `STATE.md`
- Últimos commits.
- Issues abiertos.
- ADR.
- Changelog.
- Consola actual.

El agente debe proponer una sola siguiente tarea, no un rediseño general.
