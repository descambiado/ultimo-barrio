# Investigación NPC/AI en s&box — "Último Barrio"

**Fecha**: 2026-08-07
**Contexto**: La rama base de `ultimo-barrio` usa la API actual de s&box (sistema de escenas basado en GameObject + Component, prefabs JSON, NavMesh). Se buscan candidatos para enemigos humanos (Saqueador, Bruto, Merodeador) y NPCs.
**Fuentes**: 12 búsquedas web independientes (web_search, provider autoglm), priorizando fuentes oficiales (sbox.game, github.com/Facepunch, news oficiales).

> Nota de fiabilidad: el ecosistema s&box está en plena transición (motor open-source bajo MIT desde nov-2025, Sandbox gamemode open-source desde abr-2026, API de escenas en evolución). Varios datos (revisiones exactas, licencias de assets de paquetes) no son verificables vía búsqueda; se marcan con ⚠️ y **no se inventan**.

---

## 1. Sandbox gamemode oficial de Facepunch (open source)

- **Name**: Sandbox (official Facepunch gamemode)
- **URL/package**: https://sbox.game/facepunch/sandbox (código fuente navegable: `https://sbox.game/facepunch/sandbox/source?file=Npcs%252FNpc.Disposition.cs`) · repo GitHub anunciado en https://sbox.game/news/update-26-04-08 ("Sandbox: Open Source") — URL exacta del repo ⚠️ no verificada en búsqueda
- **Exact revision**: ⚠️ no verificable (página del juego: "updated Today"; gamemode abierto el 2026-04-08)
- **Last update**: activo (abril 2026 en adelante)
- **License code**: open source (probable MIT como el motor; ⚠️ texto exacto de licencia del gamemode no verificado)
- **License assets**: los assets del Sandbox (incluido el personaje Citizen) están bajo EULA de s&box; según discusión en r/sandbox, Facepunch indica que **no se puede usar el personaje por defecto en juegos standalone publicados** ⚠️ — verificar EULA antes de usarlo
- **Dependencies**: motor s&box (Source 2)
- **s&box API generation**: actual (scene/GameObject)
- **What it solves**: es LA implementación de referencia oficial de NPCs en la API actual. Incluye NPCs con disposición/enemistad (Npc.Disposition.cs), spawn desde menú de herramientas, animación, muerte y ragdoll en el contexto del gamemode.
- **Exact files/components**: `Npcs/Npc.Disposition.cs` (disposiciones: friendly/hostile...), `Npcs/Npc.cs` (componente base, ⚠️ nombre verificable por patrón de la URL), sistema de "props" del Sandbox, integración con NavMeshAgent, modelo Citizen + Animgraph.
- **Verdict**: **ADOPT** — referencia principal: es código oficial, actual, open source y resuelve exactamente el problema (NPC humanoide con NavMesh + disposición hostil + animación + ragdoll). Copiar/adaptar componentes directamente.
- **Confidence**: ALTA (existencia y open-source verificados) / ⚠️ en revisiones exactas y licencia del gamemode.

---

## 2. NavMeshAgent (componente oficial del motor)

- **Name**: NavMeshAgent
- **URL/package**: https://sbox.game/dev/doc/gameplay/navigation/navmesh-agent
- **Exact revision**: ⚠️ no aplicable/verificable (API del motor)
- **Last update**: doc oficial vigente (2026)
- **License code**: MIT (motor s&box open source, ver #3)
- **License assets**: N/A
- **Dependencies**: NavMesh (baked en escena), componente en GameObject
- **s&box API generation**: actual (scene/GameObject — "implemented as a component. When you add it to your GameObject it can take over control of the position and rotation")
- **What it solves**: pathfinding de agentes sobre NavMesh; movimiento y rotación del NPC hacia objetivos.
- **Exact files/components**: componente `NavMeshAgent` (props: velocidad, aceleración, radio, parada, etc.), `NavMesh` para bakeado, rutas de patrulla (waypoints). Es la pieza de locomoción para Saqueador/Bruto/Merodeador.
- **Verdict**: **ADOPT** — es el componente oficial de navegación en la API actual; la rama base ya asume NavMesh. Sin alternativa seria.
- **Confidence**: ALTA.

---

## 3. Facepunch/sbox-public (motor s&box, open source)

- **Name**: sbox-public (engine source)
- **URL/package**: https://github.com/Facepunch/sbox-public · anuncio: https://sbox.game/news/update-25-11-26 ("s&box is now open source under MIT license")
- **Exact revision**: ⚠️ HEAD en movimiento; sin tag verificado
- **Last update**: activo (2026)
- **License code**: **MIT**
- **License assets**: ⚠️ los assets del motor/juegos no están cubiertos por el MIT del código (EULA s&box)
- **Dependencies**: .NET, Source 2 (Valve)
- **s&box API generation**: actual
- **What it solves**: acceso al código fuente del motor: clases base `Component`, `GameObject`, `ModelPhysics`, `Ragdoll`, `AnimationGraph`, `NavMeshAgent`, etc. Permite entender el contrato exacto de la API.
- **Exact files/components**: ⚠️ no enumerados en búsqueda; útil como fuente de verdad para la API de componentes. Issues relevantes: #11057 (petición de componentes NPC estándar), #2576 (animgraph pathing/motors).
- **Verdict**: **ADOPT** (como referencia de API, no como dependencia) — resolver dudas de contrato de componentes mirando el source.
- **Confidence**: ALTA (licencia MIT verificada por dos fuentes: news oficial + Reddit).

---

## 4. Facepunch/sbox-docs (documentación oficial)

- **Name**: sbox-docs
- **URL/package**: https://github.com/Facepunch/sbox-docs · docs en https://sbox.game/dev/doc
- **Exact revision**: ⚠️ no verificada
- **Last update**: activo (2026)
- **License code**: ⚠️ probable MIT (repos Facepunch open source); no verificado en búsqueda
- **License assets**: N/A
- **Dependencies**: ninguna
- **s&box API generation**: actual
- **What it solves**: documentación editable vía PR. Secciones clave: https://sbox.game/dev/doc/gameplay/navigation/navmesh-agent, https://sbox.game/dev/doc/scene/ (GameObjectSystem), https://sbox.game/dev/doc/animation/ ("Valve's Animgraph system… node-based animation state machine"), https://sbox.game/dev/doc/assets/ready-to-use-assets/citizen-characters.
- **Exact files/components**: docs de `Scene`, `NavMeshAgent`, `Animation`, `Citizen Characters`.
- **Verdict**: **ADOPT** — referencia normativa para la rama base.
- **Confidence**: ALTA (existencia) / ⚠️ licencia.

---

## 5. NPC Zombie / NPC Zombie Horde (Gvarados)

- **Name**: NPC Zombie (y su evolución "NPC Zombie Horde")
- **URL/package**: https://sbox.game/gvar/npc_zombie · site autor: https://www.gvar.net/
- **Exact revision**: ⚠️ no verificable (paquete de sbox.game, sin tags públicos)
- **Last update**: ⚠️ desconocida; actividad pública del autor ~2022 (devlogs de "NPC Zombie Horde")
- **License code**: ⚠️ no declarada (paquete sbox.game bajo términos de s&box)
- **License assets**: ⚠️ no declarada
- **Dependencies**: s&box; probablemente API legacy/entity en su versión original ⚠️
- **s&box API generation**: ⚠️ posiblemente legacy (2022); puede requerir migración a scene/GameObject
- **What it solves**: implementación de zombis: perseguir al jugador, horda, armadura (tougher armored zombies), incapacitación. La descripción del paquete admite límites: "NPCs can't detect gunshots or player..." (sin percepción de sonido) — útil para ver qué NO hace.
- **Exact files/components**: ⚠️ no accesibles en búsqueda; componentes de NPC zombi (movimiento hacia jugador, ataque melee, health/armor, death).
- **Verdict**: **ADAPT** (patrón de comportamiento de horda/melee, si el código sigue siendo accesible y migrable) — en caso de API legacy, usar solo como referencia de diseño → **PATTERN**. No es código oficial.
- **Confidence**: MEDIA (existencia verificada; API/licencia ⚠️).

---

## 6. SbokuBot (framework de NPCs shooter)

- **Name**: SbokuBot
- **URL/package**: https://sbox.game/righty/sbokubot
- **Exact revision**: ⚠️ no verificable
- **Last update**: ⚠️ desconocida
- **License code**: ⚠️ no declarada
- **License assets**: ⚠️ no declarada
- **Dependencies**: s&box
- **s&box API generation**: ⚠️ sin confirmar (probable actual si está mantenido; no verificado)
- **What it solves**: "flexible framework for creating shooter NPCs in S&Box. You can use the default AI or extend it with your own logic. This library has built-in..." (percepción/combate a distancia, según descripción truncada ⚠️). Relevante para **Merodeador** (enemigo con arma/alcance).
- **Exact files/components**: ⚠️ no enumerados en búsqueda (AI por defecto, puntos de extensión).
- **Verdict**: **ADAPT** — candidato a base para NPCs armados (Merodeador) si su API es actual; evaluar en local antes de comprometerse.
- **Confidence**: MEDIA (existencia y propósito verificados; contenido técnico ⚠️).

---

## 7. AI Unit Control (framework NPC, "Better NPCs")

- **Name**: AI Unit Control
- **URL/package**: https://sbox.game/yourlstcomrade/ai_unit_control (news: "An (In Depth) Introduction to Better NPCs")
- **Exact revision**: ⚠️ no verificable
- **Last update**: ⚠️ desconocida
- **License code**: ⚠️ no declarada
- **License assets**: ⚠️ no declarada
- **Dependencies**: s&box
- **s&box API generation**: ⚠️ sin confirmar (news cita componentes y triggers → sugiere API de escenas moderna; no verificado)
- **What it solves**: percepción y decisiones de NPCs: "NPC persistence, a decision component. NPCs will either use triggers to find all NPCs in an area then filter them out with a line-of-sight..." → sistema de percepción (LOS) + persistencia + componente de decisión. Es lo más cercano a un "perception system" encontrado.
- **Exact files/components**: componente de decisión, detección por triggers + line-of-sight, persistencia de NPCs.
- **Verdict**: **ADAPT** — el sistema de percepción (LOS + triggers) es directamente aplicable a los tres enemigos; el componente de decisión es patrón para estados (patrullar → perseguir → atacar).
- **Confidence**: MEDIA (descripción verificada; internals ⚠️).

---

## 8. Shrimple Ragdolls

- **Name**: Shrimple Ragdolls
- **URL/package**: https://sbox.game/fish/shrimple_ragdolls · mirror: https://sbox.grimtech.co.uk/fish/shrimple_ragdolls/
- **Exact revision**: ⚠️ no verificable
- **Last update**: ⚠️ desconocida
- **License code**: ⚠️ no declarada
- **License assets**: ⚠️ no declarada
- **Dependencies**: s&box (ModelPhysics)
- **s&box API generation**: actual (envuelve `ModelPhysics`, componente de la API de escenas actual)
- **What it solves**: "A ModelPhysics wrapper that expands its features and lets you easily switch to different ragdoll modes. Enabled: The default mode we all know as 'ragdoll'..." → modos de ragdoll configurables sobre ModelPhysics (útil para muerte de Saqueador/Bruto).
- **Exact files/components**: wrapper de `ModelPhysics`, modos de ragdoll (enabled/toggle, etc.).
- **Verdict**: **ADAPT** — si el ragdoll nativo del motor (ver #9) no basta, este wrapper añade modos; es pequeño y fácil de portar como patrón.
- **Confidence**: MEDIA-ALTA (descripción verificada; código ⚠️).

---

## 9. Ragdoll component del motor (WIP oficial)

- **Name**: Ragdoll (engine component)
- **URL/package**: https://sbox.game/news/october-update-133be3ec ("The ragdoll component is a work in progress component that creates rigid body and joint components as child game objects") · contexto histórico: https://sbox.game/news/jan2021 ("Ragdolls")
- **Exact revision**: ⚠️ WIP, sin versión
- **Last update**: activo (news oct-2025, componente en desarrollo)
- **License code**: MIT (motor, ver #3)
- **License assets**: N/A
- **Dependencies**: motor (RigidBody, Joint, ModelPhysics)
- **s&box API generation**: actual
- **What it solves**: ragdoll procedural oficial: genera rigid bodies y joints como GameObjects hijo. Nota: fix relacionado en https://sbox.game/news/update-26-05-13 ("Fixed ModelPhysics created components not being available immediately after creation").
- **Exact files/components**: componente `Ragdoll`, `ModelPhysics`, `RigidBody`, `Joint`.
- **Verdict**: **ADOPT** — usar el componente oficial (aunque WIP) antes que librerías de terceros; Shrimple como reserva.
- **Confidence**: ALTA (existencia y comportamiento verificados) / ⚠️ estabilidad por ser WIP.

---

## 10. Citizen Characters (modelo humano reutilizable)

- **Name**: Citizen Characters
- **URL/package**: https://sbox.game/dev/doc/assets/ready-to-use-assets/citizen-characters
- **Exact revision**: ⚠️ no aplicable (assets del motor)
- **Last update**: doc vigente (2026)
- **License code**: código MIT; **assets bajo EULA s&box** ⚠️ — la discusión en r/sandbox (post "The official Sandbox mode by Facepunch is now open…") señala que Facepunch dice que **no se puede usar el personaje por defecto en juegos standalone publicados** → verificar restricción exacta antes de publicar "Último Barrio" standalone
- **License assets**: EULA Facepunch (ver arriba)
- **Dependencies**: motor
- **s&box API generation**: actual
- **What it solves**: modelo humano base de Facepunch con source files (VMDLs, FBXs) incluidos → base perfecta para Saqueador/Bruto/Merodeador (re-vestir, re-texturizar, variantes de cuerpo) y para el Animgraph humanoide. Es la vía más rápida a enemigos humanos animados legalmente dentro de s&box.
- **Exact files/components**: VMDL (modelo), FBX (source), animgraph del Citizen, skins/outfits.
- **Verdict**: **ASSETS** — usar como base de enemigos humanos dentro de s&box; confirmar con EULA si el juego sale de la plataforma.
- **Confidence**: ALTA (existencia) / ⚠️ restricción de uso standalone.

---

## 11. Animgraph (sistema de animación de Valve en s&box)

- **Name**: Animgraph / AnimationGraph
- **URL/package**: https://sbox.game/dev/doc/animation/ · API: https://sbox.game/api/Sandbox.AnimationGraph · tutorial oficial de skeletal animation: https://sbox.game/dev/doc/movie-maker/skeletal-animation · issue: https://github.com/Facepunch/sbox-public/issues/2576
- **Exact revision**: ⚠️ API del motor
- **Last update**: activo (2026)
- **License code**: MIT (motor)
- **License assets**: N/A
- **Dependencies**: modelo con esqueleto compatible
- **s&box API generation**: actual
- **What it solves**: máquina de estados de animación node-based: caminar/correr (blend), ataque melee, hurt, death. Con los nuevos playermodels animados de 2025-2026 ("The New S&box Animations…") hay animaciones humanoides listas para reutilizar.
- **Exact files/components**: `AnimationGraph` (componente/asset), parámetros de blend, estados de animación; issue #2576 documenta pathing/motors del animgraph.
- **Verdict**: **ADOPT** — sistema oficial de animación de personajes; necesario para los tres enemigos (melee attack anim, locomotion blend).
- **Confidence**: ALTA.

---

## 12. Scene system (GameObject/Component) — documentación base

- **Name**: Scene system / GameObjectSystem
- **URL/package**: https://sbox.game/dev/doc/scene/ · guía comunidad: https://steamcommunity.com/sharedfiles/filedetails/?id=3595903475
- **Exact revision**: ⚠️ API del motor
- **Last update**: activo (2026)
- **License code**: MIT (motor)
- **License assets**: N/A
- **Dependencies**: motor
- **s&box API generation**: actual
- **What it solves**: contrato base de la API actual (GameObjects + Components + Systems) sobre el que va todo lo demás; la rama base de Último Barrio ya usa esto.
- **Exact files/components**: `GameObject`, `Component`, `GameObjectSystem`, prefabs `.scene`/JSON.
- **Verdict**: **ADOPT** (ya es la base del proyecto).
- **Confidence**: ALTA.

---

## 13. Cut Them Down (juego de hordas melee, 3rd person)

- **Name**: Cut Them Down
- **URL/package**: https://sbox.game/struggler/cut_them_down
- **Exact revision**: ⚠️ no verificable
- **Last update**: ⚠️ (news "UPDATE #2 - New Enemies!"; fecha no verificada)
- **License code**: ⚠️ no declarada
- **License assets**: ⚠️ no declarada
- **Dependencies**: s&box
- **s&box API generation**: ⚠️ sin confirmar
- **What it solves**: "Tear through hordes of enemies in a 3rd person action survivors-like" → arquetipo de combate melee contra hordas en 3ª persona: patrones de spawn de oleadas, enemigos melee que rodean al jugador. Muy relevante como referencia de diseño para Saqueador/Bruto y oleadas.
- **Exact files/components**: ⚠️ no accesibles en búsqueda.
- **Verdict**: **PATTERN** — referencia de diseño de hordas/melee; no código aprovechable salvo acceso directo.
- **Confidence**: MEDIA (existencia verificada; internals ⚠️).

---

## 14. Zombie Mod [s&box]

- **Name**: Zombie Mod
- **URL/package**: https://sbox.game/zmp/zm_mod_test
- **Exact revision**: ⚠️ no verificable
- **Last update**: ⚠️ desconocida
- **License code**: ⚠️ no declarada
- **License assets**: ⚠️ no declarada
- **Dependencies**: s&box
- **s&box API generation**: ⚠️ sin confirmar
- **What it solves**: "infection by zombie hit, knockback from gunfire, object physics for building barricades" → mecánicas de infección, knockback y barricadas; útil como patrón de reglas de combate cuerpo a cuerpo (hit → infección/daño) y respuesta a impacto.
- **Exact files/components**: ⚠️ no accesibles en búsqueda.
- **Verdict**: **PATTERN** — mecánicas de reglas, no componentes reutilizables directos.
- **Confidence**: MEDIA.

---

## 15. "In This House" (candidato semilla)

- **Name**: In This House (s&box)
- **URL/package**: ⚠️ no localizado en búsquedas (ni sbox.game ni github)
- **Exact revision**: ⚠️ N/A
- **Last update**: ⚠️ N/A
- **License code**: ⚠️ N/A
- **License assets**: ⚠️ N/A
- **Dependencies**: ⚠️ N/A
- **s&box API generation**: ⚠️ N/A
- **What it solves**: ⚠️ no verificable; no se encontraron referencias fiables en esta sesión.
- **Exact files/components**: ⚠️ N/A
- **Verdict**: **DISCARD** (hasta que se aporte URL verificable) — no inventar datos.
- **Confidence**: BAJA (sin datos).

---

## 16. Contexto general / estado del ecosistema (referencias transversales)

- **Reddit "The current state of Sandbox NPC creation"** (https://www.reddit.com/r/sandbox/comments/1t674i6/): "There is no standardized way to create NPCs for any game. There is no interchangeability between each…" → no existe un estándar de NPCs; cada juego construye el suyo. Confirma la estrategia: montar un framework propio sobre componentes oficiales (NavMeshAgent + AnimationGraph + Ragdoll), tomando patrones de los paquetes listados.
- **Issue sbox-public #11057** (https://github.com/Facepunch/sbox-public/issues/11057): "There are no standardized components for Sandbox mode NPC's" → misma conclusión desde Facepunch issues.
- **s&box Explorer** (https://sbox.grimtech.co.uk/): buscador de terceros con tag `npcs` (API v1) — útil para descubrir más paquetes de NPCs.
- **Tutoriales de referencia**: "How to make moving NPCs and make them Ragdoll in S&box" (YouTube, APhaohonuCU); "S&box Tutorial: Introduction to Animgraph" (Q3wTr9dLYiQ); "S&box Tutorial: Ragdoll Workflow (Playermodel Guide #3)" (DyP6FM40hTo).
- **Veredicto colectivo**: **PATTERN** (contexto, no candidato directo).

---

# Recomendaciones

**PRINCIPAL — ADOPT: Sandbox oficial de Facepunch (open source) como base de código para los NPCs.**
Construir Saqueador/Bruto/Merodeador como componentes propios (`Npc.cs` adaptado + disposición hostil como `Npc.Disposition.cs`) sobre los componentes oficiales del motor:
- **Locomoción**: `NavMeshAgent` (pathfinding, patrulla, chase)
- **Animación**: `AnimationGraph` (blend caminar/correr + animación de ataque melee + hurt/death; usar animaciones del Citizen)
- **Muerte**: componente `Ragdoll` oficial del motor (WIP) + `ModelPhysics`
- **Assets**: modelo `Citizen Characters` re-vestido (verificar EULA para publicación standalone)
Todo esto es MIT (código), API actual, con soporte y docs oficiales. Es el único camino con garantías a medio plazo.

**Alternativa 1 — ADAPT: SbokuBot** (https://sbox.game/righty/sbokubot)
Framework de NPCs con armas (shooter AI) con AI por defecto y puntos de extensión → base para el **Merodeador** (enemigo armado a distancia) y para la variante armada del Saqueador. Verificar en local que usa la API de escenas actual.

**Alternativa 2 — ADAPT: AI Unit Control** (https://sbox.game/yourlstcomrade/ai_unit_control)
Sistema de percepción (triggers + line-of-sight), persistencia y componente de decisión → el "cerebro" de estados (patrullar/perseguir/atacar/retirarse) para los tres enemigos; complementa a la opción principal en lugar de sustituirla.

**Alternativa 3 — ADAPT/PATTERN: Shrimple Ragdolls + Cut Them Down / NPC Zombie (Gvarados)**
- Shrimple Ragdolls como reserva de modos de ragdoll si el componente oficial WIP falla.
- Cut Them Down (hordas melee 3ª persona) y NPC Zombie Horde (Gvarados) como patrones de diseño de horda/melee (oleadas, rodeo, armadura), no como código a copiar (API ⚠️ posiblemente legacy, licencias no declaradas).

**Descartados**: "In This House" (no verificable en esta sesión); paquetes sin licencia declarada para ASSETS directos.

**Riesgos a vigilar**: (1) licencia de los assets del Citizen para publicación standalone; (2) estabilidad del componente Ragdoll (WIP); (3) los paquetes de terceros pueden estar en API legacy — validar en local antes de adoptar; (4) s&box no ofrece componentes NPC estándar (issue #11057) → el framework propio es inevitable.
