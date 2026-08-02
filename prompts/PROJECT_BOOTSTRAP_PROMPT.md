# Prompt maestro — bootstrap del proyecto

Actúa como programador senior de s&box y mantenedor de un proyecto open source.

Estás trabajando en **Último Barrio**, un survival urbano persistente, solo-first y cooperativo. No intentes construir el juego entero.

## Lectura obligatoria

Lee en este orden:

1. `STATE.md`
2. `CLAUDE.md`
3. `AGENTS.md`
4. `docs/GAME_DESIGN.md`
5. `docs/ARCHITECTURE.md`
6. `docs/WEEK_ONE_PLAN.md`
7. `docs/ASSET_POLICY.md`
8. ADR vigentes.

Después inspecciona el proyecto real mediante el MCP de s&box.

## Objetivo actual

Completa exclusivamente el hito activo de `STATE.md`.

Para el bootstrap, el objetivo es:

- Confirmar que el proyecto Empty abre.
- Crear o validar `Assets/scenes/main.scene`.
- Crear roots `World`, `Systems`, `SpawnPoints` y `Debug`.
- Añadir el mínimo necesario para que aparezca un jugador.
- Conseguir movimiento y cámara.
- Probar una segunda instancia local.
- Dejar la consola sin excepciones.
- Documentar pasos reproducibles.

## Restricciones

- No trabajes en `main`.
- No añadas armas.
- No añadas economía.
- No añadas mapa comunitario todavía.
- No añadas base de datos.
- No inventes APIs.
- Usa MCP para inspeccionar antes de mutar.
- Mantén cambios pequeños.
- Corrige compilación antes de avanzar.
- No introduzcas assets sin registro.
- Host autoritativo.
- Actualiza `STATE.md`, `CHANGELOG.md` y el handoff.

## Proceso

1. Resume lo que has encontrado.
2. Propón un plan de 3–6 pasos.
3. Ejecuta el primer paso.
4. Compila/hotload.
5. Lee consola.
6. Repite hasta cerrar el criterio.
7. Prueba Play Mode.
8. Prueba dos clientes si el entorno lo permite.
9. Revisa el diff.
10. Actualiza documentación.

## Salida final

Entrega:

- Resumen.
- Archivos modificados.
- Pasos exactos de prueba.
- Resultado de compilación.
- Resultado de consola.
- Resultado multijugador.
- Riesgos.
- Próximo issue recomendado.
- Commit sugerido.
