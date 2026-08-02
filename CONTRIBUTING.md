# Contribuir a Último Barrio

## Antes de empezar

Lee, en este orden:

1. `README.md`
2. `STATE.md`
3. `docs/GAME_DESIGN.md`
4. `docs/ARCHITECTURE.md`
5. `docs/ASSET_POLICY.md`
6. `AGENTS.md`

## Flujo

1. Busca o crea un issue.
2. Comenta que vas a trabajarlo.
3. Crea una rama:
   - `feat/<nombre>`
   - `fix/<nombre>`
   - `docs/<nombre>`
   - `test/<nombre>`
   - `chore/<nombre>`
4. Mantén el cambio pequeño.
5. Prueba en el editor.
6. Actualiza documentación.
7. Abre PR usando la plantilla.

## Commits

Formato recomendado:

```text
tipo(área): descripción
```

Ejemplos:

```text
feat(apartment): add claimable apartment state
fix(network): validate build placement on host
docs(ai): document raid utility scoring
test(save): add save migration fixture
```

Tipos:

- `feat`
- `fix`
- `docs`
- `test`
- `refactor`
- `perf`
- `chore`

## Definition of Done

Una tarea no está terminada hasta que:

- Compila.
- La consola no muestra errores nuevos.
- Existe una prueba manual reproducible.
- Autoridad de red revisada.
- Guardado/migración revisados si toca datos persistentes.
- Asset registry actualizado si se añadió contenido.
- Documentación actualizada.
- `STATE.md` actualizado si cambia el hito activo.
- PR explica riesgos y rollback.

## Dependencias

No añadas un paquete porque “funciona en mi máquina”. Incluye:

- Identificador.
- Autor.
- URL.
- Tipo de paquete.
- Licencia.
- Versión o fecha comprobada.
- Archivos que lo usan.
- Alternativa si desaparece.
- Resultado del spike técnico.

## Contenido sensible

La ambientación es ficticia. No se aceptan:

- Facciones enemigas definidas por etnia, nacionalidad o religión real.
- Propaganda política real.
- Símbolos extremistas usados como decoración glorificadora.
- Assets extraídos sin permiso.
- Reproducciones exactas de conflictos o víctimas identificables.

## Seguridad

No publiques exploits funcionales contra servidores. Sigue `SECURITY.md`.
