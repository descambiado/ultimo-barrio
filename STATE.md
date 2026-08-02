# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: Bootstrap
- Versión objetivo: `0.1.0-alpha`
- Rama activa: `feat/m0-bootstrap`
- Rama estable: `main`
- Último commit de implementación estable: `2ba1925` (`feat(scene): add first boot scene`)
- Baseline etiquetado: `0298207` (`bootstrap-v0.0.0`, solo local por ahora)
- Licencia: Mozilla Public License 2.0 (`SPDX-License-Identifier: MPL-2.0`)
- Build de s&box: `26.07.22`, Steam BuildID `24338653`

## GitHub

- Repositorio público: **PENDIENTE**; GitHub CLI conserva una sesión expirada y todavía no hay autorización válida.
- URL: **PENDIENTE**; no se inventa hasta obtener el owner mediante `gh api user --jq .login`.
- Remoto `origin`: **NO CONFIGURADO**.
- Ramas remotas: **NINGUNA PUBLICADA**.
- Tags remotos: **NINGUNO PUBLICADO**.
- Draft PR: **PENDIENTE**.
- Issues: **PENDIENTES** (`M0-03`, `M0-04`, `M1-01` y los tres spikes iniciales).

## Hito activo

### M0-03 — `main.scene` y primer boot jugable

**Objetivo:** guardar la primera escena real, mostrar un jugador provisional y verificar movimiento, cámara y Play Mode sin excepciones.

### Criterios de aceptación

- [x] Existe `Assets/scenes/main.scene`.
- [x] La escena contiene los roots `World`, `Systems`, `SpawnPoints` y `Debug`.
- [x] Existe un suelo provisional.
- [x] Existe un punto de aparición.
- [x] El jugador provisional aparece en Play Mode.
- [ ] Movimiento y cámara funcionan. La cámara en tercera persona renderiza, pero todavía no se ha inyectado ni observado input de movimiento.
- [x] Play Mode inicia y se detiene sin excepciones de proyecto.
- [x] La consola no contiene entradas de nivel `Error` durante la prueba.
- [x] La prueba parcial queda documentada en `docs/testing/m0-03-first-boot.md`.

M0-03 permanece **EN PROGRESO** porque el movimiento todavía no está probado.

## Estado técnico comprobado

- Escena activa: `scenes/main.scene` según el registro de escenas del MCP.
- Cambios sin guardar en la escena: no.
- Jerarquía serializada: exactamente cuatro roots, `World`, `Systems`, `SpawnPoints` y `Debug`. El quinto objeto que expone el MCP es `editor_camera`, transitorio y no guardado en `main.scene`.
- Mundo provisional: plano con collider estático, luz direccional y skybox 2D.
- Jugador: instancia del prefab oficial `templates/gameobject/player controller.prefab`.
- Cámara: `CameraComponent` principal y cámara en tercera persona del prefab; el render se comprobó visualmente.
- Compilación: 10 compiladores correctos; `local.ultimo_barrio` y `local.ultimo_barrio.editor` con 0 errores y 0 avisos.
- Consola: 0 entradas de nivel `Error` en 47 entradas almacenadas en la lectura posterior. Hay 11 avisos globales del engine/templates; ninguno menciona `ultimo_barrio` ni `main.scene`.
- Play Mode: **PROBADO PARCIALMENTE**; arrancó en `main.scene`, mostró al jugador y se detuvo correctamente.
- Movimiento: **NO VERIFICADO**; el MCP disponible no expone teclado o ratón.
- Prueba multijugador: **NO REALIZADA**; no se inició una segunda instancia.
- Captura de evidencia: `docs/media/first-boot.png`, 1280×720, captura real de Play Mode; no demuestra movimiento.

## M1-01 preparado, no implementado

El contrato previsto para el primer apartamento reclamable queda preparado para su issue:

- `ApartmentId`
- `OwnerId`
- `ClaimState`
- `DoorReference`
- `StashReference`
- `SpawnReference`
- `SaveVersion`

El flujo será autoritativo en host y la persistencia se aislará tras una interfaz con snapshots versionados. No existe todavía código, apartamento, claim ni guardado.

## Bloqueadores reales

1. GitHub CLI no tiene una autorización válida. Crear el repositorio, publicar refs, abrir el PR y crear issues requiere completar `gh auth login` en el navegador.
2. El MCP de s&box no inyecta entrada de teclado o ratón. La aparición y el render de cámara están probados, pero el movimiento necesita una interacción real en la ventana de Play Mode.
3. El MCP no expone lanzamiento/unión/control de una segunda instancia. M0-04 requiere una prueba manual o una capacidad adicional del editor.

## Siguientes tres acciones

1. Completar la autorización web de GitHub CLI, obtener el owner real y publicar `main`, `feat/m0-bootstrap` y `bootstrap-v0.0.0` sin force-push.
2. Entrar en Play Mode y comprobar físicamente movimiento con WASD y control de cámara; registrar el resultado y cualquier entrada de consola.
3. Tras cerrar M0-03, configurar una sesión host y una segunda instancia para M0-04; solo después comenzar los anchors del primer apartamento.

## Decisiones vigentes

- Ambientación ficticia mediterránea.
- Solo-first y cooperativo.
- Host autoritativo.
- Persistencia desacoplada mediante interfaz.
- Armas oficiales como camino base.
- Dependencias externas registradas.
- No construir la Alpha alrededor de OmniParadigm hasta completar el spike.
- No reescribir `bootstrap-v0.0.0` ni el historial existente.

## Riesgos actuales

- Confundir una captura estática con una prueba de movimiento.
- Añadir red o apartamentos antes de cerrar el boot local.
- Usar una identidad efímera de conexión como `OwnerId`.
- Confirmar un claim antes de persistirlo o permitir dos ganadores concurrentes.
- Elegir un mapa demasiado grande antes del spike de la primera manzana.
- El manifiesto conserva valores provisionales de plantilla (`Org: local`, 64 jugadores y `facepunch.flatgrass`) que deben revisarse antes de una prueba multijugador pública.

## Registro de sesiones

| Fecha | Autor/agente | Rama | Resultado | Último commit estable |
|---|---|---|---|---|
| 2026-08-02 | Bootstrap documental | — | Starter pack generado | — |
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | Starter instalado, baseline Git local y preflight creados | `4441876` |
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | Proyecto Empty integrado; compilación y escena mínima validadas | `a3084b0` |
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | README y medios reales preparados; `main.scene` creada y primer boot parcial validado | `2ba1925` |
