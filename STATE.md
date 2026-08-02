# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: Bootstrap
- Versión objetivo: `0.1.0-alpha`
- Rama activa: `feat/m0-bootstrap`
- Rama estable: `main`
- Último commit de implementación estable: `2ba1925` (`feat(scene): add first boot scene`)
- Baseline etiquetado: `0298207` (`bootstrap-v0.0.0`, publicado sin reescribir)
- Licencia: Mozilla Public License 2.0 (`SPDX-License-Identifier: MPL-2.0`)
- Build de s&box: `26.07.22`, Steam BuildID `24338653`

## GitHub

- Owner real: `descambiado`, confirmado por `gh api user --jq .login`.
- Repositorio público: https://github.com/descambiado/ultimo-barrio
- Remoto `origin`: `https://github.com/descambiado/ultimo-barrio.git`.
- Ramas remotas: `main` y `feat/m0-bootstrap`.
- Tag remoto: `bootstrap-v0.0.0` → `0298207`.
- Rama principal: `main`.
- Issues activados; wiki y Discussions desactivadas.
- GitHub Pages: no configurado.
- Releases: ninguna.
- Draft PR: [#7 — chore: bootstrap real del proyecto s&box](https://github.com/descambiado/ultimo-barrio/pull/7).
- Issues:
  - [#1 — M0-03: crear `main.scene` y validar el primer boot jugable](https://github.com/descambiado/ultimo-barrio/issues/1)
  - [#2 — M0-04: validar sesión local con dos clientes](https://github.com/descambiado/ultimo-barrio/issues/2)
  - [#3 — M1-01: crear apartamento reclamable](https://github.com/descambiado/ultimo-barrio/issues/3)
  - [#4 — SPIKE-MAP-001](https://github.com/descambiado/ultimo-barrio/issues/4)
  - [#5 — SPIKE-WEAPONS-001](https://github.com/descambiado/ultimo-barrio/issues/5)
  - [#6 — SPIKE-SAVE-001](https://github.com/descambiado/ultimo-barrio/issues/6)

## Hitos completados

### M0-02 — Publicación inicial

- [x] Repositorio público creado.
- [x] `origin` configurado.
- [x] `main` publicada.
- [x] `feat/m0-bootstrap` publicada.
- [x] `bootstrap-v0.0.0` publicado sin reescribir.
- [x] Draft PR abierta.
- [x] Issues iniciales creados.
- [x] README humano y medios reales publicados en la rama de trabajo.

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
- Red en la escena: no existe `NetworkHelper`; el `Player` actual es una instancia directa y no un spawn por conexión.
- Camino oficial de dos clientes: verificado en la instalación local como `sbox.exe -joinlocal +instanceid <N>` después de iniciar hosting. No se ejecutó.
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

1. El MCP de s&box no inyecta entrada de teclado o ratón. La aparición y el render de cámara están probados, pero el movimiento necesita una interacción real en la ventana de Play Mode.
2. El MCP no expone el menú `Start Hosting`, input independiente ni la consola del segundo proceso. El lanzamiento oficial de otra instancia está identificado, pero M0-04 aún requiere configurar `NetworkHelper`, evitar duplicar el Player directo y validar ambos clientes.

## Siguientes tres acciones

1. Entrar en Play Mode y comprobar físicamente movimiento con WASD y control de cámara; registrar el resultado y cualquier entrada de consola en el issue #1.
2. Tras cerrar M0-03, sustituir el Player directo por spawn autoritativo mediante `NetworkHelper`, iniciar hosting y lanzar el segundo cliente con el comando oficial verificado.
3. Cuando M0-04 tenga evidencia de dos clientes, comenzar los anchors del apartamento definidos en el issue #3.

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
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | M0-02 publicado: repositorio público, refs, draft PR e issues iniciales | `a8fbd90` |
