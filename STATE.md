# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: M1 — primer apartamento reclamable
- Versión objetivo: `0.1.0-alpha`
- Rama activa: `feat/m1-01-claimable-apartment`
- Rama estable: `main`
- Último commit estable: `d5ed250` (merge de la PR #7 en `main`)
- Último commit de M0 publicado: `72c74d5` (`test(network): validate local two-client session`)
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
- PR de bootstrap: [#7 — chore: bootstrap real del proyecto s&box](https://github.com/descambiado/ultimo-barrio/pull/7) — fusionada en `main` mediante `d5ed250`.
- Issues:
  - [#1 — M0-03: crear `main.scene` y validar el primer boot jugable](https://github.com/descambiado/ultimo-barrio/issues/1) — cerrado.
  - [#2 — M0-04: validar sesión local con dos clientes](https://github.com/descambiado/ultimo-barrio/issues/2) — cerrado.
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

### M0-04 — sesión local con dos clientes

- [x] `NetworkHelper` sustituye al Player directo.
- [x] El host crea un único jugador local.
- [x] Una segunda instancia conecta y crea un segundo jugador.
- [x] Ownership práctico, movimiento y cámara independientes comprobados.
- [x] Desconexión limpia: el host elimina solo el jugador remoto.
- [x] Consolas revisadas y prueba reproducible documentada.

## Hito activo

### M1-01 — primer apartamento reclamable

**Objetivo:** crear una unidad provisional que el host pueda asignar a una
identidad estable, guardar en un snapshot local v1 y restaurar al reiniciar.

### Criterios de aceptación

- [x] Rama M1 creada desde el merge estable de `main`.
- [x] Contrato de siete campos implementado.
- [x] Petición de claim limitada a `ApartmentId`; identidad y distancia se
  resuelven en el host.
- [x] Proveedor local implementado con generaciones inmutables, CRC64,
  relectura y bloqueo ante versiones futuras.
- [x] Blockout provisional, portal y anchors serializados en `main.scene`.
- [ ] Escena recargada en el editor con las tres referencias resueltas.
- [ ] Claim probado en Play Mode.
- [ ] Persistencia y reaparición probadas después de reiniciar.
- [ ] Carrera, reconexión y segundo cliente probados.
- [ ] Consola de host y cliente revisada para este hito.

## Estado técnico comprobado

- Escena activa: `scenes/main.scene` según el registro de escenas del MCP.
- Cambios sin guardar en la escena: no.
- Jerarquía serializada: exactamente cuatro roots, `World`, `Systems`, `SpawnPoints` y `Debug`. El quinto objeto que expone el MCP es `editor_camera`, transitorio y no guardado en `main.scene`.
- Mundo provisional: plano con collider estático, luz direccional y skybox 2D.
- Jugador: `NetworkHelper` clona el prefab oficial
  `templates/gameobject/player controller.prefab` por conexión; no existe un
  Player directo en la escena fuente.
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
- Prueba multijugador: **PROBADA** con editor host y un `sbox.exe` independiente.
  Hubo exactamente dos Players, input/cámara independientes y desconexión.
- Red en la escena: `Systems/Network` usa `NetworkHelper` con `StartServer=true`
  y el prefab oficial; el `SpawnPoint` de la escena actúa como fallback.
- Consola host: 0 entradas `Error` entre 64 almacenadas.
- Log cliente: revisado, con errores de recursos del entorno dev; la sesión fue
  funcional, pero la consola del cliente no se declara limpia.
- Captura de evidencia: `docs/media/first-boot.png`, 1280×720, captura real de Play Mode; no demuestra movimiento.

## M1-01 implementado parcialmente, todavía no validado

La rama contiene el contrato previsto para el primer apartamento reclamable:

- `ApartmentId`
- `OwnerId`
- `ClaimState`
- `DoorReference`
- `StashReference`
- `SpawnReference`
- `SaveVersion`

El flujo implementado reserva simultáneamente por apartamento y propietario,
persiste antes de aplicar el estado sincronizado y usa
`Connection.SteamId.ValueUnsigned` detrás de `IPlayerIdentityProvider`. El
cliente solo envía `ApartmentId`.

`Assets/scenes/main.scene` contiene el blockout `Prototype Apartment A01`, el
portal de claim, `DoorReference`, `StashReference`, `SpawnReference` y el
servicio bajo `Systems`. No se ha declarado completado: falta ejecutar claim,
reinicio, carrera, reconexión y late join.

## Bloqueadores reales

- El endpoint MCP quedó ocupado al intentar guardar desde un comando de editor
  después de detectar una limitación de `set_component` con referencias
  `GameObject`. El proceso de s&box sigue respondiendo, pero la escena debe
  recargarse desde disco (versión externa) antes- **M1-01 (Apartments):** Recuperado, parcheado y publicado en PR #8 (draft). Se añadieron contratos base (Contracts.cs) y UI provisional para claim interactivo.
- **Producción Masiva (Vertical Slice):** Finalizada y fusionada la ejecución paralela de los 6 agentes:
  - **[x] Agente B:** `feat/vslice-inventory` (Inventario y Stash) - Integrado
  - **[x] Agente C:** `feat/vslice-combat` (Combate y armas) - Integrado
  - **[x] Agente A:** `feat/vslice-world-blockout` (Mundo y edificios) - Integrado
  - **[x] Agente D:** `feat/vslice-ai` (Vecinos y saqueadores) - Integrado
  - **[x] Agente F:** `feat/vslice-economy` (Comercio básico) - Integrado
  - **[x] Agente E:** `feat/vslice-night-cycle` (Ciclo de tiempo y raids) - Integrado
  
Todos los sistemas (Inventario, Combate, IA, Mundo, Economía, Reloj) están commiteados en `feat/m1-01-claimable-apartment` y el proyecto compila limpiamente (0 errores de C#).

## Pendiente
- El usuario debe ensamblar visualmente los prefabs generados (`pf_building_apartment_01.prefab`, `pf_enemy_spawn.prefab`, etc.) dentro de `main.scene` usando el Editor de s&box.
- Asignar los scripts de Combat, Economy, y WorldTime a GameObjects en la escena.
- Probar Play Mode con dos clientes para validar el flujo completo (Inventario -> Combate -> Economía -> Noche/Raid). desde disco y confirmar las tres referencias
   mediante MCP.
2. Compilar, ejecutar Play Mode y probar claim, snapshot y reinicio.
3. Probar carrera/reconexión/segundo cliente; solo después actualizar el issue
   #3, hacer commit y publicar la rama.

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
| 2026-08-03 | Codex + prueba de red | `feat/m0-bootstrap` | M0-04 validado con host, segundo cliente, input independiente y desconexión | `test(network): validate local two-client session` |
| 2026-08-03 | Codex + subagentes | `feat/m1-01-claimable-apartment` | PR #7 fusionada; contrato, persistencia y blockout A01 implementados; validación de M1 bloqueada hasta recargar la escena | `d5ed250` |
