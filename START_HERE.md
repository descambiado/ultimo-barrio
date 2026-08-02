# Empezar el proyecto

## Objetivo de la primera sesión

Terminar con:

- Proyecto vacío de s&box creado.
- Repositorio Git inicializado.
- Starter pack copiado.
- Editor abierto sin errores.
- MCP conectado.
- Escena `main.scene` creada.
- Primer commit etiquetado como punto estable.
- Issue inicial preparada.

No se implementan armas, economía ni IA antes de completar este bootstrap.

## 1. Crear el proyecto de s&box

Desde el editor:

1. `New Project`
2. `Game`
3. `Empty`
4. Nombre técnico: `ultimo_barrio`
5. Organización: la organización pública que vaya a mantener el paquete.

Evita espacios, tildes y nombres comerciales largos en identificadores internos.

## 2. Copiar este starter pack

Copia todos los archivos y carpetas a la raíz donde se encuentre el `.sbproj`.

La estructura debería empezar así:

```text
ultimo_barrio/
├── ultimo_barrio.sbproj
├── Assets/
├── Code/
├── Libraries/
├── docs/
├── prompts/
├── .github/
├── README.md
├── CLAUDE.md
├── AGENTS.md
└── STATE.md
```

`Libraries/` debe permanecer versionado. Las librerías de s&box se almacenan como código fuente dentro del proyecto.

## 3. Inicializar Git

```bash
git init
git branch -M main
git add .
git commit -m "chore: bootstrap ultimo barrio"
git tag bootstrap-v0.0.0
```

Después crea el repositorio público y añade el remoto:

```bash
git remote add origin <REMOTE>
git push -u origin main --tags
```

## 4. Protección mínima de `main`

Configura en GitHub:

- PR obligatorio.
- Al menos una revisión cuando haya colaboradores.
- Conversaciones resueltas antes del merge.
- Bloqueo de force-push.
- Eliminación automática de ramas tras merge.
- Squash merge como opción predeterminada.

Mientras solo exista un mantenedor, se permiten merges propios, pero deben seguir pasando por PR para conservar contexto.

## 5. Conectar un agente al MCP de s&box

En el editor:

1. `Editor → Preferences → MCP Server`
2. Verifica que esté activo.
3. Copia el comando de Claude Code o la URL local.
4. Ejecuta el agente desde la raíz del proyecto.

Comando habitual:

```bash
claude mcp add --transport http sbox http://127.0.0.1:7269/mcp
```

El agente debe poder:

- Leer la escena.
- Buscar assets.
- Crear o editar GameObjects.
- Entrar en Play Mode.
- Leer la consola.
- Sacar capturas.

## 6. Crear la escena inicial

Crea:

```text
Assets/scenes/main.scene
```

Solo debe contener inicialmente:

- Root `World`.
- Root `Systems`.
- Root `SpawnPoints`.
- Root `Debug`.
- Cámara o configuración mínima necesaria.
- Un suelo provisional.
- Un punto de spawn.

Guarda, entra en Play Mode y confirma que no hay excepciones.

## 7. Primer hito técnico

Abre el issue:

> **M0 — Boot estable y jugador visible**

Criterios:

- El proyecto abre sin errores.
- La escena principal carga.
- El jugador aparece en un spawn.
- Puede moverse y mirar.
- Dos clientes pueden entrar en una sesión local.
- La consola queda limpia.
- Se documenta el procedimiento exacto para repetir la prueba.

## 8. Entregar contexto a la IA

Pega o adjunta:

- `prompts/PROJECT_BOOTSTRAP_PROMPT.md`
- `CLAUDE.md`
- `AGENTS.md`
- `STATE.md`
- `docs/ARCHITECTURE.md`
- El issue concreto que debe resolver.

Nunca digas simplemente “haz el juego”. Cada agente recibe una tarea cerrada y verificable.

## 9. Cierre obligatorio de sesión

Antes de parar:

1. Compila y prueba.
2. Actualiza `STATE.md`.
3. Añade decisiones nuevas a un ADR.
4. Actualiza `CHANGELOG.md`.
5. Deja tres siguientes acciones concretas.
6. Haz commit.
7. Anota el último commit estable.

Esto permite retomar el proyecto semanas después sin reconstruir el contexto.
