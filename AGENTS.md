# Instrucciones para agentes

Estas instrucciones se aplican a Claude Code, ChatGPT/Codex, Cursor, Cline y agentes equivalentes.

## Lectura obligatoria

Antes de modificar nada:

1. `STATE.md`
2. `docs/ARCHITECTURE.md`
3. `docs/GAME_DESIGN.md`
4. `docs/ASSET_POLICY.md`
5. El issue asignado.
6. Los archivos directamente afectados.

## Regla principal

**Implementa una única tarea verificable. No intentes completar el juego entero.**

## Prohibiciones

- No trabajar directamente en `main`.
- No inventar APIs de s&box.
- No añadir paquetes sin registrar.
- No copiar assets de Garry's Mod o juegos comerciales.
- No introducir una base de datos externa durante M0–M2.
- No hacer que el cliente decida dinero, daño, botín, propiedad o guardado.
- No cambiar arquitectura global sin ADR.
- No reescribir archivos fuera del alcance.
- No iniciar un segundo sistema si el proyecto no compila.
- No dejar TODOs vagos sin issue asociado.

## Uso del MCP

Cuando el editor esté disponible:

1. Inspecciona la escena real.
2. Busca componentes y assets existentes.
3. Haz cambios pequeños.
4. Entra en Play Mode.
5. Lee la consola.
6. Captura una imagen cuando valide el resultado.
7. Corrige errores antes de continuar.

Nunca deduzcas la escena exclusivamente desde archivos si el MCP puede inspeccionarla.

## Trabajo paralelo

Cada agente posee un área:

- Player/network.
- Game flow/persistence.
- Apartments/economy.
- Civilians/raids.
- UI/audio.
- QA/performance.

Dos agentes no deben editar el mismo archivo simultáneamente.

Antes de delegar, el integrador asigna propietarios de escritura por archivo o
carpeta. Durante trabajo paralelo:

- `STATE.md` y `CHANGELOG.md` pertenecen al integrador.
- Cada subagente escribe su resultado en un handoff único bajo
  `docs/handoffs/` si necesita persistir contexto.
- Un subagente solo modifica archivos compartidos cuando el integrador se los
  asigna explícitamente.
- El integrador revisa los diffs y consolida estado y changelog al final.

## Diseño de código

- Composición sobre herencia profunda.
- Configuración data-driven cuando aporte valor.
- Interfaces en límites reemplazables.
- Nombres explícitos.
- Métodos cortos.
- Estados de red mínimos.
- Guardados versionados.
- Random determinista cuando afecte simulación compartida.
- Logs accionables, no spam.

## Cierre de tarea

Responde con:

1. Resumen.
2. Archivos modificados.
3. Cómo probar.
4. Resultado de compilación.
5. Riesgos.
6. Trabajo pendiente.
7. Handoff de la tarea o actualización de `STATE.md` si eres el integrador.

Si no pudiste verificar algo, dilo claramente.
