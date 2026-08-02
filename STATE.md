# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: Bootstrap
- Versión objetivo: `0.1.0-alpha`
- Rama activa: `feat/m0-bootstrap`
- Rama estable: `main`
- Último commit de implementación estable: `2ba1925` (`feat(scene): add first boot scene`)
- Último commit operativo publicado: `9417320` (`docs: record M0-02 publication`)
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
  - [#1 — M0-03: crear `main.scene` y validar el primer boot jugable](https://github.com/descambiado/ultimo-barrio/issues/1) — cerrado.
  - [#2 — M0-04: validar sesión local con dos clientes](https://github.com/descambiado/ultimo-barrio/issues/2)
  - [#3 — M1-01: crear apartamento reclamable](https://github.com/descambiado/ultimo-barrio/issues/3)
  - [#4 — SPIKE-MAP-001](https://github.com/descambiado/ultimo-barrio/issues/4) — cerrado.
  - [#5 — SPIKE-WEAPONS-001](https://github.com/descambiado/ultimo-barrio/issues/5)
  - [#6 — SPIKE-SAVE-001](https://github.com/descambiado/ultimo-barrio/issues/6)
- Resultados de spikes publicados:
  - [Mapa](https://github.com/descambiado/ultimo-barrio/issues/4#issuecomment-5160604969): blockout propio y pequeño mediante Scene Mapping.
  - [Armas](https://github.com/descambiado/ultimo-barrio/issues/5#issuecomment-5160641939): `BaseCombatWeapon`, validación host y assets CC0 individuales; prueba pendiente.
  - [Guardado](https://github.com/descambiado/ultimo-barrio/issues/6#issuecomment-5160609553): `FileSystem.Data`, generaciones inmutables y escritor único host; implementación pendiente.

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

### M0-03 — `main.scene` y primer boot jugable

- [x] `Assets/scenes/main.scene` creada con los cuatro roots acordados.
- [x] Suelo, iluminación, spawn, jugador provisional y cámara presentes.
- [x] Play Mode inicia y termina sin excepciones del proyecto.
- [x] Movimiento, salto, ratón y cambio entre tercera y primera persona
  comprobados manualmente el 2026-08-03.
- [x] Compilación y consola del proyecto limpias tras la prueba.

## Hito activo

### M0-04 — sesión local con dos clientes

**Objetivo:** sustituir el Player directo por spawn de red autoritativo y validar
host y segundo cliente en una sesión local.

### Criterios de aceptación

- [ ] El host crea una sesión local.
- [ ] Una segunda instancia se conecta.
- [ ] Existen dos jugadores distintos.
- [ ] Ownership y movimiento independiente funcionan.
- [ ] Ambas cámaras funcionan.
- [ ] La desconexión del segundo cliente es limpia.
- [ ] Consolas de host y cliente sin errores del proyecto.
- [ ] Procedimiento y evidencia documentados.

M0-04 está **EN PROGRESO**. La ruta oficial de lanzamiento está verificada,
pero la escena todavía no contiene `NetworkHelper` ni spawn por conexión.

## Estado técnico comprobado

- Escena activa: `scenes/main.scene` según el registro de escenas del MCP.
- Cambios sin guardar en la escena: no.
- Jerarquía serializada: exactamente cuatro roots, `World`, `Systems`, `SpawnPoints` y `Debug`. El quinto objeto que expone el MCP es `editor_camera`, transitorio y no guardado en `main.scene`.
- Mundo provisional: plano con collider estático, luz direccional y skybox 2D.
- Jugador: instancia del prefab oficial `templates/gameobject/player controller.prefab`.
- Cámara: `CameraComponent` principal y controles oficiales del prefab; se
  comprobaron tercera persona, primera persona y control con ratón.
- Compilación: 10 compiladores correctos; `local.ultimo_barrio` y `local.ultimo_barrio.editor` con 0 errores y 0 avisos.
- Consola posterior a la prueba: ninguna entrada `Error` y ninguna entrada
  `Warn` filtrada por `ultimo_barrio` entre 53 entradas almacenadas. Los avisos
  globales del engine/editor no pertenecen al proyecto.
- Play Mode local: **PROBADO**; arrancó en `main.scene`, mostró al jugador,
  recibió input real y se detuvo correctamente.
- Movimiento: **VERIFICADO MANUALMENTE** el 2026-08-03 con WASD, salto y ratón.
  También se comprobó el cambio entre tercera y primera persona. El MCP solo
  expone el transform serializado, por lo que la evidencia de input es la
  observación directa del operador, acompañada de la lectura posterior de
  compilación y consola.
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

- Ninguno para M0-03; el hito está completado.
- Para M0-04, el MCP no expone el menú `Start Hosting`, input independiente ni
  la consola del segundo proceso. Esto no bloquea todavía la implementación:
  falta configurar `NetworkHelper`, evitar duplicar el Player directo e
  intentar el lanzamiento oficial de la segunda instancia.

## Siguientes tres acciones

1. Sustituir el Player directo por spawn autoritativo mediante `NetworkHelper`.
2. Iniciar hosting y lanzar el segundo cliente con `sbox.exe -joinlocal +instanceid <N>`.
3. Verificar ownership, movimiento/cámara independientes, desconexión y las dos consolas; después cerrar M0-04.

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
| 2026-08-03 | Codex + prueba manual | `feat/m0-bootstrap` | M0-03 validado: WASD, salto, ratón, primera/tercera persona y consola del proyecto limpia | `test(player): verify movement and camera input` |
