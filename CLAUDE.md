# Claude Code — contexto del repositorio

## Proyecto

Último Barrio es un survival urbano persistente para s&box. De día, el jugador trabaja, comercia y oculta recursos. De noche, protege su apartamento y el barrio de grupos hostiles con objetivos de saqueo, registro o captura.

La ambientación es una ciudad mediterránea ficticia. No uses nacionalidades, etnias o religiones reales como facciones enemigas.

## Fuente de verdad

- Estado actual: `STATE.md`
- Diseño: `docs/GAME_DESIGN.md`
- Arquitectura: `docs/ARCHITECTURE.md`
- Alcance semanal: `docs/WEEK_ONE_PLAN.md`
- Assets: `docs/ASSET_POLICY.md`
- Decisiones: `docs/decisions/`

## Antes de editar

- Lee el issue.
- Inspecciona el proyecto con MCP.
- Comprueba errores existentes.
- Identifica la prueba de aceptación.
- Confirma que el cambio cabe en una sola PR.

## Reglas duras

- No trabajar en `main`.
- Host autoritativo.
- No paquetes sin `Assets/asset-registry.yml`.
- No APIs inventadas.
- No persistencia externa antes de que la local funcione.
- No IA de lenguaje en nube para comportamiento base.
- No armas antes de completar el spike de integración.
- Mantén el juego jugable en solitario.
- Mantén el proyecto compilando después de cada paso.
- Actualiza `STATE.md` al finalizar.

## MCP de s&box

El editor expone normalmente:

```text
http://127.0.0.1:7269/mcp
```

Utiliza el MCP para:

- Inspeccionar escenas y GameObjects.
- Buscar assets.
- Crear o editar objetos.
- Ejecutar Play Mode.
- Leer consola.
- Tomar capturas.

Prefiere herramientas read-only antes de mutaciones. Realiza cambios reversibles.

## Armas

Camino base:

- Mecánicas con `BaseCombatWeapon`.
- Visuales con la colección oficial `facepunch/sboxweapons`.
- `omniparadigm/weapons` solo después de completar `SPIKE-WEAPONS-001`.

No acoples inventario, daño o guardado a una implementación visual.

## Formato de respuesta final

```text
Resumen:
Archivos:
Prueba:
Compilación:
Capturas/logs:
Riesgos:
Siguiente acción:
```
