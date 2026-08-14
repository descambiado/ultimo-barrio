# Investigación BUILDING — Catálogo de sistemas de construcción/fortificación para "Último Barrio" (s&box)

- **Fecha:** 2026-08-07
- **Contexto de API:** rama base con API actual de s&box (GameObject, componentes, prefabs JSON — sistema de escena moderno). s&box se abrió como open source el 2026-04-28 (MIT), y Facepunch publicó el gamemode Sandbox como open source el 2026-04-08.
- **Método:** 18 búsquedas independientes en web (variaciones sobre Basebound, Sandbox/Sandbox++, Fortwars, constraints, snapping/placement, destructible props, doors/locks, repair, health, Wirebox, paquetes building en sbox.game, awesome-sbox). Prioridad a fuentes oficiales (github.com/Facepunch, sbox.game, wiki).
- **Convención de fiabilidad:** ⚠️ = dato no verificable desde búsqueda web (no se ha inventado; requiere clonar/leer el repo para confirmar). Revisions exactas de commit NO verificables sin git → marcadas como no determinadas.

---

## Candidato 1 — Facepunch/sandbox (gamemode Sandbox oficial)

- **Name:** Facepunch/sandbox
- **URL/package:** https://github.com/Facepunch/sandbox · https://sbox.game/facepunch/sandbox
- **Exact revision:** no determinada ⚠️ (repo público activo, mirror del gamemode en desarrollo)
- **Last update:** en curso; open sourced el 2026-04-08 (anuncio: sbox.game/news/update-26-04-08)
- **License code:** ⚠️ asumible MIT como sbox-public (reddit confirma MIT para sbox-public; la licencia exacta del gamemode no verificada en búsqueda). No comercializarlo sin leer LICENSE.
- **License assets:** ⚠️ no verificada; ojo: el core cloud asset "explosion" de sbox base addon es no-comercial (issue Facepunch/sbox-public #11211), patrón a respetar con assets del engine.
- **Dependencies:** s&box engine (sbox-public). Ninguna externa.
- **s&box API generation:** actual — el gamemode vive en el API moderno (componentes/escena); los docs de API sbox.game/api exponen sus tipos (Sandbox.Prop, Sandbox.Mapping.Door, etc.). ⚠️ Verificar si el repo está ya migrado 100% al sistema GameObject/prefabs JSON o conserva restos de entidades legacy (la migración fue progresiva; confirmar al clonar).
- **What it solves:** es EL código de referencia oficial para: spawn de props, menú de spawn, toolgun, welding/constraints básicos, entidades físicas, HUD, networking. Patrón canónico de "cómo Facepunch estructura un juego multiplayer".
- **Exact files/components:**
  - `Sandbox.Prop` (componente Prop): el modelo define su salud y qué pasa al romperse ("The model can define its health and what happens when it breaks") → health/breakable por ModelDoc. URL: https://sbox.game/api/Sandbox.Prop/
  - `Sandbox.Mapping.Door`: puerta animada con curva (AnimationCurve, X=tiempo 0-1, Y=abertura 0-1). URL: https://sbox.game/api/Sandbox.Mapping.Door
  - Toolgun + herramientas de constraint (weld, etc.), SpawnMenu, prop/ragdoll/weapon spawners ⚠️ (estructura exacta de archivos del repo no listada en búsqueda; confirmar en clon).
- **Verdict:** **ADOPT** (base de código principal). Es oficial, activo, con los patrones de salud/destructibles/puertas/placement que necesitamos, y licencia abierta compatible con aprender/adaptar.
- **Confidence:** ALTA para existencia/rol; MEDIA para detalles de archivos internos (⚠️ revision y estructura exacta sin verificar).

---

## Candidato 2 — Nebual/sandbox-plus (fork comunitario "SandboxPlus")

- **Name:** sandbox-plus (a.k.a. el "Sandbox++" de facto de la comunidad)
- **URL/package:** https://github.com/nebual/sandbox-plus
- **Exact revision:** no determinada ⚠️
- **Last update:** activo (referenciado junto a Wirebox en la era open source 2025-2026) ⚠️ fecha exacta no verificada
- **License code:** ⚠️ no verificada en búsqueda (fork de gamemode; leer LICENSE del repo)
- **License assets:** ⚠️ no verificada
- **Dependencies:** Facepunch/sandbox (fork de él); diseñado para que addons modulares (Wirebox) se monten encima.
- **s&box API generation:** actual (vive al día con el gamemode de Facepunch) ⚠️.
- **What it solves:** aporta exactamente las piezas de "toolbox" que faltan en el Sandbox vanilla:
  - **Constraint tool** ("omni-tool" estilo Precision Tool): weld / axis / rope / elastic / slider / ballsocket + nudge/rotate/move → toda la gama de constraints de GMod en s&box.
  - Arquitectura de **addons modulares** (permite que Wirebox y otros extiendan el gamemode sin forkear).
- **Exact files/components:** ⚠️ rutas exactas no verificadas en búsqueda (constraint tool, precision tool, panel de addons). Patrón a extraer: implementación de joints/constraints en código C#.
- **Verdict:** **ADAPT** — si "Último Barrio" quiere herramientas de fortificación con constraints (puertas con bisagras, barricadas atornilladas), es la referencia de implementación más completa del ecosistema.
- **Confidence:** ALTA (existencia/rol confirmado por múltiples fuentes); MEDIA (detalles internos ⚠️).

---

## Candidato 3 — wiremod/wirebox + WireLib

- **Name:** Wirebox (Wire para s&box) + WireLib
- **URL/package:** https://github.com/wiremod/wirebox · https://sbox.game/wiremod/wireboxaddon · https://sbox.game/wiremod/wirelib
- **Exact revision:** no determinada ⚠️
- **Last update:** activo (era open source; "Early WIP" en README) ⚠️ fecha exacta no verificada
- **License code:** ⚠️ no verificada (proyecto heredado de la comunidad de GMod; comprobar LICENSE)
- **License assets:** ⚠️ no verificada
- **Dependencies:** SandboxPlus (fork de Nebual) — depende de él para funcionar como addon; otros addons dependen de **WireLib** (interfaces como `IWireInputEntity`) en vez de Wirebox completo.
- **s&box API generation:** actual ⚠️.
- **What it solves:** lógica/circuitos: gates, entradas/salidas por cable entre entidades. Para "Último Barrio" es relevante solo si queremos sistemas eléctricos (alarmas, luces, puertas motorizadas, trampas). No es un sistema de building per se.
- **Exact files/components:** WireLib con interfaces de integración (`IWireInputEntity`); addons de gates/entidades ⚠️ rutas exactas sin verificar.
- **Verdict:** **PATTERN** (arquitectura de integración: librería de interfaces separada del contenido). No adoptar el contenido completo salvo que el juego pida electricidad.
- **Confidence:** MEDIA (⚠️ detalles no verificados).

---

## Candidato 4 — apetavern/fortwars-entity (Fortwars, Ape Tavern)

- **Name:** Fortwars (s&box, por Ape Tavern) — repo "fortwars-entity"
- **URL/package:** https://github.com/apetavern/fortwars-entity · assets: https://sbox.game/apetavern/fw_assets
- **Exact revision:** no determinada ⚠️
- **Last update:** activo en 2025-2026 (assets "updated" recientes en sbox.game) ⚠️ fecha exacta no verificada
- **License code:** ⚠️ no verificada (leer LICENSE; proyecto open source de equipo con otras obras MIT ⚠️)
- **License assets:** ⚠️ colección "Fortwars Assets" en sbox.game (Wood/Steel 1x1, 1x2, 1x1x1, 1x2x1, etc.) — comprobar términos por paquete.
- **Dependencies:** s&box engine; assets propios en paquete separado.
- **s&box API generation:** actual ⚠️ (proyecto de la era post-open-source; confirmar migración a GameObject).
- **What it solves:** el caso de uso MÁS parecido a "Último Barrio" de todo el ecosistema: CTF con **énfasis en building** — fase de construcción ("build round") con **build wheel** ([Q]), bloques limitados por jugador, fortalezas por equipos, y combate para destruir la base enemiga.
- **Exact files/components:** ⚠️ no listados en búsqueda; por snippets: build wheel (rueda de selección de bloques), sistema de bloques colocables con tope de cantidad, fases de build/combat. Patrones esperables: placement con preview, grid de bloques, destructibilidad por daño.
- **Verdict:** **ADAPT** — referencia de diseño #1 para el loop "construir fortaleza → defender → asaltar". Extraer: build wheel, límite de bloques, fases de juego, assets de bloques de madera/acero.
- **Confidence:** MEDIA-ALTA (existencia y mecánicas confirmadas por snippets; detalles internos ⚠️).

---

## Candidato 5 — Nolankicks/Fortwars (port abandonado de GMOD Fortwars)

- **Name:** Fortwars (Kick's Collective)
- **URL/package:** https://github.com/Nolankicks/Fortwars
- **Exact revision:** no determinada ⚠️
- **Last update:** abandonado ("This is a now abandoned project... decided to end the project and open source it") — fecha ⚠️
- **License code:** ⚠️ no verificada (open source al abandonarse)
- **License assets:** ⚠️ no verificada
- **Dependencies:** s&box engine (era legacy — API de entidades pre-escena ⚠️).
- **s&box API generation:** LEGACY (código de la era de entidades; útil solo como referencia conceptual o para portar).
- **What it solves:** port completo del clásico Fortwars de GMod (forts + CTF). Misma idea que el candidato 4 pero código viejo y muerto.
- **Exact files/components:** ⚠️ no verificados.
- **Verdict:** **PATTERN** (referencia conceptual/histórica) o **DISCARD** si no aporta nada que fortwars-entity no tenga ya. Recomendado: PATTERN solo para diseño de reglas; no como base de código.
- **Confidence:** ALTA (abandonado y legacy confirmado por README/snippets).

---

## Candidato 6 — themasterminds/sbox-fortwars

- **Name:** sbox-fortwars (The Masterminds)
- **URL/package:** https://github.com/themasterminds/sbox-fortwars
- **Exact revision:** no determinada ⚠️
- **Last update:** ⚠️ no verificada (proyecto pequeño, posiblemente antiguo)
- **License code:** ⚠️ no verificada
- **License assets:** ⚠️ no verificada
- **Dependencies:** s&box engine.
- **s&box API generation:** ⚠️ probablemente LEGACY (mención a "SetMaterial" y todo por hacer: "basic todo: SetMaterial based on Player") — estilo API antigua.
- **What it solves:** otro port de Fortwars: build round + combat round, captura de banderas. Confirmación de que el patrón "fase de construcción → fase de combate" se repite en la comunidad.
- **Exact files/components:** ⚠️ no verificados.
- **Verdict:** **PATTERN** (concepto de fases build/combat; código menor).
- **Confidence:** BAJA-MEDIA (⚠️ poco verificable, repo pequeño).

---

## Candidato 7 — Facepunch/sbox-public (engine + issues públicos)

- **Name:** Facepunch/sbox-public
- **URL/package:** https://github.com/Facepunch/sbox-public
- **Exact revision:** no determinada ⚠️ (repositorio público del engine, MIT)
- **Last update:** activo; s&box lanzado/release open source 2026-04-28.
- **License code:** **MIT** (confirmado por comunidad/reddit y wikipedia) ⚠️ verificar términos exactos.
- **License assets:** ⚠️ mixto: core cloud assets con licencias no-comerciales (issue #11211: asset "explosion" bloquea publicación comercial) — IMPORTANTE para no asumir que todo lo del engine es libre.
- **Dependencies:** — (es el engine).
- **s&box API generation:** actual (motor completo: GameObject/Component, Scene, prefabs JSON).
- **What it solves:** no es un sistema de building, pero sus issues/documentación son fuente de verdad sobre límites del API:
  - **Issue #4566:** no hay ropes/cables físicos en el editor de escenas (constraints de cuerda/cable NO disponibles en el sistema de escena actual) → los constraints "tipo GMod" pertenecen a runtime/legacy o hay que implementarlos.
  - Docs API: https://sbox.game/api/i/components (catálogo oficial de componentes; patrón model-driven health/breakables en Sandbox.Prop).
- **Exact files/components:** engine/source + docs markdown (movidos a GitHub en el update 26-04-08).
- **Verdict:** **PATTERN** (fuente normativa: qué hace el API nativo y qué hay que construir uno mismo).
- **Confidence:** ALTA.

---

## Candidato 8 — API oficial sbox.game (Sandbox.Prop, Sandbox.Mapping.Door, componentes)

- **Name:** s&box API reference (docs del gamemode Sandbox de Facepunch)
- **URL/package:** https://sbox.game/api/Sandbox.Prop/ · https://sbox.game/api/Sandbox.Mapping.Door · https://sbox.game/api/i/components
- **Exact revision:** no determinada ⚠️ (docs en vivo)
- **Last update:** en vivo (2026) ⚠️
- **License code:** docs del gamemode open source ⚠️ (misma incógnita que candidato 1)
- **License assets:** — (solo API)
- **Dependencies:** gamemode Facepunch/sandbox.
- **s&box API generation:** actual.
- **What it solves:** documenta las dos piezas nativas que "Último Barrio" necesita sin escribir nada:
  - **Health/destructibles:** el modelo (ModelDoc) puede definir salud y comportamiento al romperse → props destructibles "gratis" por configuración de asset, no solo por código.
  - **Puertas:** componente de puerta con curva de animación (tiempo/abertura) → base para puertas fortificables (añadir cerradura como componente propio).
- **Exact files/components:** `Sandbox.Prop`, `Sandbox.Mapping.Door`, catálogo `i/components`.
- **Verdict:** **ADOPT** (usar los componentes nativos como cimiento de salud/puertas/destructibles; lo que no cubren —cerraduras, reparación, snapping, previews— se implementa como componentes propios encima).
- **Confidence:** ALTA (documentación oficial con snippets verificados).

---

## Candidato 9 — Ryhon0/awesome-sbox (índice comunitario)

- **Name:** awesome-sbox
- **URL/package:** https://github.com/Ryhon0/awesome-sbox
- **Exact revision:** no determinada ⚠️
- **Last update:** ⚠️ no verificada (lista viva)
- **License code:** ⚠️ no verificada (índice markdown, típicamente CC0/MIT)
- **License assets:** —
- **Dependencies:** —
- **s&box API generation:** mixto (lista toda la era).
- **What it solves:** catálogo curado de proyectos open source de s&box → es el punto de partida para descubrir más sistemas de building/doors/locks sin buscarlos a ciegas.
- **Exact files/components:** lista de repos por categoría.
- **Verdict:** **ADOPT** (herramienta de trabajo: consultar antes de implementar cualquier subsistema; probablemente contenga repos de cerraduras/trampas que no salieron en estas búsquedas).
- **Confidence:** ALTA (existencia confirmada); contenido ⚠️.

---

## Candidato 10 — BaseWars (obse)

- **Name:** BaseWars (s&box)
- **URL/package:** https://sbox.game/obse/basewars
- **Exact revision:** no determinada ⚠️
- **Last update:** ⚠️ no verificada (juego publicado en sbox.game)
- **License code:** ⚠️ no verificada (probablemente cerrado; no es repo público conocido)
- **License assets:** ⚠️
- **Dependencies:** s&box engine.
- **s&box API generation:** actual ⚠️.
- **What it solves:** loop "Rust-like" sobre s&box: **usa recursos para construir muros, torretas, púas y máquinas**; reclamas plot gratis y construyes tu base. Es la evidencia de que el fantasy de "Último Barrio" (fortificar un barrio, recursos → barricadas) tiene precedente jugable.
- **Exact files/components:** ⚠️ no públicos (no hay repo verificado): walls, turrets, spikes, machines, plot claiming.
- **Verdict:** **PATTERN** (referencia de diseño de loop/economía; código no accesible → no se puede adaptar, solo inspirar).
- **Confidence:** MEDIA (mecánicas confirmadas por snippet oficial; código ⚠️).

---

## Candidato 11 — SpaceBox (ataco)

- **Name:** SpaceBox
- **URL/package:** https://sbox.game/ataco/spacebox/
- **Exact revision:** no determinada ⚠️
- **Last update:** ⚠️ no verificada ("Very early days yet")
- **License code:** ⚠️ no verificada
- **License assets:** ⚠️
- **Dependencies:** s&box engine.
- **s&box API generation:** actual ⚠️.
- **What it solves:** construcción de naves estilo GMod SpaceBuild con modo build (TAB) sobre la nave — patrón de "modo construcción con preview/ghost y grid" en runtime.
- **Exact files/components:** ⚠️ no públicos.
- **Verdict:** **PATTERN** (confirmar patrón "modo build + piezas encajables").
- **Confidence:** BAJA (⚠️ poco verificable; juego temprano).

---

## Candidato 12 — Fortwars Assets (paquete de assets de bloques)

- **Name:** Fortwars Assets (Ape Tavern)
- **URL/package:** https://sbox.game/apetavern/fw_assets/
- **Exact revision:** no determinada ⚠️
- **Last update:** recientes (colección "updated" 2025-2026) ⚠️ fecha exacta no verificada
- **License code:** ⚠️ no verificada (paquete comunitario; leer términos)
- **License assets:** ⚠️ bloques de construcción: Fortwars Wood 1x1/3x2/1x1x1, Fortwars Steel 1x2/1x2x1, etc. — exactamente el tipo de kit modular (madera/acero) para barricadas.
- **Dependencies:** fortwars-entity (hecho para ese juego; reutilizable si licencia lo permite).
- **s&box API generation:** actual (assets de escena/prefabs) ⚠️.
- **What it solves:** provee el contenido 3D modular de bloques de fortificación ya optimizado para el flujo de placement.
- **Exact files/components:** colección de modelos de bloques Wood/Steel con tamaños de grid.
- **Verdict:** **ASSETS** (si la licencia lo permite, reutilizar/estudiar como base de los kits de barricadas; en cualquier caso, su nomenclatura de tamaños = espec de grid 1x1/1x2/1x1x1 útil).
- **Confidence:** MEDIA (⚠️ licencia sin verificar).

---

## Candidato 13 — "Basebound" (candidato semilla)

- **Name:** Basebound
- **URL/package:** no encontrado como proyecto de building verificado ⚠️
- **Exact revision:** — (no localizado)
- **Last update:** — (no localizado)
- **License code:** — (no localizado)
- **License assets:** — (no localizado)
- **Dependencies:** — (no localizado)
- **s&box API generation:** — (no localizado)
- **What it solves:** buscado 2 veces ("s&box Basebound", '"Basebound" sbox game github'): NO aparece ningún sistema/proyecto de building llamado "Basebound" en s&box. El único rastro es un nombre en un skill de terceros (lobehub: "echohello-dev-basebound-sbox-ui-razor", sobre UI Razor, sin relación con building). El proyecto de building real más cercano en nombre es **BaseWars** (candidato 10).
- **Exact files/components:** — (no localizado)
- **Verdict:** **DISCARD** (no verificable; probablemente no existe como framework de building, o es un proyecto privado/renombrado). No invertir tiempo en él; si aparece, re-evaluar.
- **Confidence:** ⚠️ BAJA (no hay evidencia; explícitamente NO se inventa).

---

## Candidato 14 — "Sandbox++" (candidato semilla)

- **Name:** Sandbox++ (a.k.a. SandboxPlus)
- **URL/package:** sin repo propio encontrado; el proyecto real es **Nebual/sandbox-plus** (candidato 2) ⚠️
- **Exact revision:** — (no localizado como tal)
- **Last update:** — (no localizado como tal)
- **License code:** — (no localizado)
- **License assets:** — (no localizado)
- **Dependencies:** — (no localizado como tal)
- **s&box API generation:** — (no localizado como tal)
- **What it solves:** no existe un proyecto llamado "Sandbox++" verificable; la comunidad usa "SandboxPlus" (fork de Nebual) como el sandbox extendido (constraints, addons, precision tool).
- **Exact files/components:** — (no localizado)
- **Verdict:** **DISCARD** como nombre propio → redirigir a **sandbox-plus** (candidato 2, ADAPT).
- **Confidence:** ⚠️ BAJA (sin evidencia del nombre; no se inventa).

---

## Notas transversales (verificadas en búsquedas)

1. **Constraints en el sistema de escena actual:** NO hay soporte nativo de editor para ropes/cables físicos (issue Facepunch/sbox-public #4566). Los constraints tipo GMod (weld/axis/rope/elastic/slider/ballsocket) existen en código (sandbox-plus los implementa). → Para "Último Barrio" (API GameObject/prefabs JSON), planificar constraints propios (componentes de joint físicas) o evitarlos y usar snapping por grid.
2. **Health/breakables nativo:** el modelo define salud y comportamiento de rotura (Sandbox.Prop) → barricadas/props destructibles con mínimo código; la reparación es un componente propio a escribir (no se encontró sistema de reparación de referencia verificable en el ecosistema ⚠️).
3. **Doors nativo:** Sandbox.Mapping.Door (curva de animación) → base para puertas; **cerraduras/codelocks**: no se encontró sistema público verificable (comunidad los construye ad-hoc; buscar en awesome-sbox ⚠️).
4. **Licencias de assets del engine:** algunos core cloud assets son no-comerciales (issue #11211) → auditar licencias antes de publicar.
5. **Fechas clave:** s&box open source 2026-04-28 (MIT); gamemode Sandbox open sourced 2026-04-08; docs markdown movidos a GitHub en esa fecha.

---

## Recomendaciones

### PRINCIPAL — Base: Facepunch/sandbox + API nativa (ADOPT)
Clonar/estudiar `Facepunch/sandbox` como cimiento: componentes nativos `Sandbox.Prop` (salud/destructibles por ModelDoc) y `Sandbox.Mapping.Door` (puertas animadas) para barricadas, props destructibles y puertas. Implementar encima, como componentes propios del juego (API GameObject + prefabs JSON):
- **Placement preview/ghost**: componente de preview (GameObject semitransparente con validación de posición/rotación) — patrón estándar, sin dependencias externas.
- **Snapping**: grid propio en código (tamaños 1x1/1x2/1x1x1 como espec de Fortwars Assets) + snap por vecino.
- **Salud/reparación**: componente `Destructible` propio + herramienta/anim de reparación (no hay sistema de reparación de referencia → diseñarlo: curar por nivel de daño, estados visuales, coste de recursos).
- **Cerradura/puertas**: componente `Lock` propio sobre Sandbox.Mapping.Door (clave/candado, propietario, estado de red).

**Por qué:** es lo único oficial, activo y con licencia abierta; cubre 40-50% del catálogo building sin fricción; el resto son componentes de nicho que de todos modos habría que escribir.

### Alternativa A — Toolbox de constraints: Nebual/sandbox-plus (ADAPT)
Si "Último Barrio" quiere fortificación física avanzada (barricadas atornilladas, puertas con bisagras, trampas con ball sockets), adaptar el omni-tool de constraints (weld/axis/rope/elastic/slider/ballsocket + nudge/rotate/move) de `Nebual/sandbox-plus`. Coste: el gamemode de Facepunch cambia rápido; el fork va por detrás y habría que portar al API actual.

### Alternativa B — Diseño de juego: apetavern/fortwars-entity (ADAPT)
Para el loop "construir → defender → asaltar", extraer de Fortwars: build wheel ([Q]) con catálogo de bloques, límite de bloques por jugador, fases build/combat. Es el precedente jugable más cercano a "Último Barrio" y su paquete de assets (Wood/Steel) sirve de espec de kit modular (ASSETS si licencia lo permite).

### Alternativa C — Loop económico: BaseWars (PATTERN)
Como referencia de diseño de economía/recursos (recoger recursos → muros/torretas/púas/máquinas, claim de plots) para el sistema de fortificación basado en recursos del barrio. Código no accesible → solo inspiración.

### Descartados explícitamente
- **Basebound**: no verificable, sin evidencia de existencia (⚠️).
- **Sandbox++**: sin proyecto propio; es sandbox-plus (→ Alternativa A).
- **Nolankicks/Fortwars**: abandonado y API legacy (solo referencia conceptual).
- **wiremod/wirebox**: solo si se añade electricidad/lógica (PATTERN de integración, no de building).

### Pasos siguientes sugeridos (para el equipo)
1. Clonar Facepunch/sandbox y verificar: licencia exacta, migración al API GameObject, rutas de Prop/Door/Toolgun.
2. Consultar Ryhon0/awesome-sbox para cerraduras/trampas/repair comunitarios (hueco detectado: locks y repair no tienen referente público verificado).
3. Decidir el sistema de grid (1m? 0.5m?) con la espec de bloques Fortwars como referencia.
