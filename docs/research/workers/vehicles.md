# Investigación: Ecosistema de Vehículos en s&box — "Último Barrio"

- **Fecha:** 2026-08-07
- **Worker:** VEHÍCULOS (subagente de investigación)
- **Contexto del repo:** rama base con API actual de s&box (GameObject, componentes, prefabs JSON)
- **Necesidades del juego:** física de vehículo, enter/exit, maletero (inventario), daño
- **Método:** 17 búsquedas independientes con `web_search` (variaciones de: vehicle physics kit, vehicle-prototyping, arcade car physics, enter/exit, trunk inventory, vehicle damage, packages en sbox.game, repos Facepunch/community). Fuentes priorizadas: sbox.game (incl. sboxdb.dev), github.com, wiki/docs oficiales. **No se clonaron repos ni se abrió código completo** → todo lo no verificable está marcado ⚠️. No se inventó nada.
- **Hallazgo clave del ecosistema:** en 2025-2026 s&box pasó a API nueva (GameObject/Component, prefabs JSON) y el ecosistema de vehículos se concentra en paquetes **open-source publicados en sbox.game** (código visible en la propia web del paquete) + algunos repos de GitHub. No hay un sistema de vehículos oficial "built-in" completo (física+enter/exit+trunk+daño) en un solo paquete; el campo está dividido entre kits de física y frameworks.

---

## Candidatos

### 1. Vehicle Physics Kit (por sbox Field Guide)

- **Name:** Vehicle Physics Kit
- **URL/package:** https://sbox.game/fieldguide/vehiclephysics (código fuente navegable: `source?file=Code/Assembly.cs`, `source?file=Code/WheelVisual.cs`, `source?file=.obj/__compiler_extra.cs`)
- **Exact revision:** no verificada en esta sesión (paquete "Released"; sboxdb.dev lo rastrea, revisión exacta ⚠️)
- **Last update:** activo en 2026 (el paquete hermano Vehicle Prototyping tiene versión del 2026-07-22, ver candidato 2) ⚠️
- **License code:** no verificado ⚠️ (paquete de sbox de "sbox Field Guide", tradicionalmente open-source en el perfil de Field Guide)
- **License assets:** sin arte — "No art required" (los assets son primitivos generados en código, no hay arte comercial que bloquee)
- **Dependencies:** ninguno reportado en snippets; se integra con VehicleFactory/Vehicle Prototyping (mismo autor)
- **s&box API generation:** **actual (nueva)** — componentes estilo `WheelVisual` (componente que mueve una malla de rueda desde estado `VehicleWheel`), estados de rueda, ensamblado por `VehicleFactory` (patrón GameObject/Component) — Confianza MEDIA (por inspección de rutas/archivos fuente en snippets)
- **What it solves:** física de vehículo con ruedas raycast: curvas de deslizamiento de neumático (slip-curve tires), drivetrain completo, ayudas de conducción (assists) y cámara de persecución (chase cam). Sin necesidad de arte.
- **Exact files/components (visto en snippets):** `Code/Assembly.cs`, `Code/WheelVisual.cs` (visual de rueda desde estado `VehicleWheel`), sistema de ruedas raycast, `VehicleFactory` (hook que construye la física). Enter/exit, maletero y daño: **no cubiertos** por este paquete (el kit es de física).
- **Verdict:** **ADOPT** — es el candidato principal para la capa de *física* (raycast wheels, drivetrain, assists, cámara) sobre API nueva, activo y sin dependencia de arte. No cubre enter/exit, maletero ni daño → hay que complementarlo.
- **Confidence:** ALTA (existencia, propósito y enfoque verificados por múltiples snippets oficiales sbox.game); detalles de licencia/revisión ⚠️

### 2. Vehicle Prototyping (por sbox Field Guide)

- **Name:** Vehicle Prototyping
- **URL/package:** https://sbox.game/fieldguide/vehicle_prototyping (fuente: `source?file=Vehicle/Parts/PartKitFactory.cs`)
- **Exact revision:** **v313827** (sboxdb.dev, https://sboxdb.dev/package/fieldguide/vehicle_prototyping/version/313827)
- **Last update:** 2026-07-22 (versión 313827, 412 archivos, 5.2 MB)
- **License code:** no verificado ⚠️ (mismo autor que Vehicle Physics Kit)
- **License assets:** assets procedimentales/primitivos de prototipado (sin arte comercial)
- **Dependencies:** Vehicle Physics Kit (mismo autor; "Hooks into VehicleFactory to let the Vehicle Physics Kit build physics and primitive visuals")
- **s&box API generation:** **actual (nueva)** — `VehicleFactory`, `PartKitFactory` (fábrica de piezas), reubicación del asiento de conductor → patrón componentes/prefabs actual — Confianza MEDIA
- **What it solves:** prototipado rápido de vehículos: construye la física y los visuales primitivos del vehículo desde piezas, reubica el asiento del conductor, y hace auditoría/logs ("logs audits"). Es el flujo de trabajo para montar un vehículo desde código sin modelar.
- **Exact files/components:** `Vehicle/Parts/PartKitFactory.cs` (fábrica de piezas), hook sobre `VehicleFactory`, lógica de posición del driver seat. No cubre maletero ni daño.
- **Verdict:** **ADOPT** (complemento del candidato 1) — acelera el montaje de vehículos de "Último Barrio" con prefabs generados en código y asiento de conductor.
- **Confidence:** ALTA (versión, fecha y hook verificado vía sboxdb.dev + snippet oficial)

### 3. CAVC — Clearly A Vehicle Controller

- **Name:** CAVC - Clearly A Vehicle Controller
- **URL/package:** https://sbox.game/clearly/cavc/ (el snippet menciona página de GitHub: "https://github...." truncado ⚠️)
- **Exact revision:** no verificada ⚠️
- **Last update:** no verificado ⚠️
- **License code:** no verificado ⚠️
- **License assets:** incluye "a demo car to try out" (assets de demo ⚠️)
- **Dependencies:** no verificadas ⚠️
- **s&box API generation:** presumiblemente actual (paquete publicado en sbox.game) ⚠️
- **What it solves:** "framework para añadir coches y otros vehículos a tu juego" — enfoque más amplio que un kit de física (framework de vehículos). Con coche demo.
- **Exact files/components:** no verificados en snippets ⚠️ (candidato a revisar en profundidad: puede cubrir enter/exit/seat genérico)
- **Verdict:** **ADAPT / verificar** — si su framework incluye asientos/enter-exit genéricos y licencia permisiva, es el complemento natural para la capa de interacción. Requiere inspección manual antes de decidir.
- **Confidence:** MEDIA (existencia verificada; contenido/licencia ⚠️)

### 4. sbox-arcade-car-physics (port de matekdev)

- **Name:** Arcade Car Physics (s&box port)
- **URL/package:** https://github.com/matekdev/sbox-arcade-car-physics
- **Exact revision:** no verificada (sin clonar) ⚠️
- **Last update:** no verificado ⚠️
- **License code:** no verificado en snippets ⚠️ (port del proyecto MIT-ish de SergeyMakeev, pero no confirmado para el fork)
- **License assets:** assets del coche demo incluidos ⚠️
- **Dependencies:** ninguna reportada; port de https://github.com/SergeyMakeev/ArcadeCarPhysics (Unity)
- **s&box API generation:** actual (port para s&box con multijugador; "setup to work in multiplayer") ⚠️ (MEDIA)
- **What it solves:** física arcade de coche (estilo GTA/Rocket League/Flatout según el original) con lógica para posicionar/rotar las ruedas y soporte multijugador.
- **Exact files/components:** lógica de posición/rotación de ruedas (visto en snippet); resto ⚠️
- **Verdict:** **ADAPT** (candidato si se quiere un feeling arcade más simple que el kit de Field Guide; habría que portar/verificar contra la API actual) — también útil como **PATTERN** de referencia.
- **Confidence:** MEDIA (existencia verificada; licencia/revisión ⚠️)

### 5. ZCars Vehicle Physics Demo (FosterZ / Koncha)

- **Name:** ZCars Vehicle Physics Demo
- **URL/package:** https://sbox.game/koncha/fosterz_cardemo
- **Exact revision:** no verificada ⚠️
- **Last update:** no verificado ⚠️
- **License code:** "open-source" (según snippet oficial) ⚠️ (licencia concreta no indicada)
- **License assets:** demo técnica (assets ⚠️)
- **Dependencies:** ninguna reportada
- **s&box API generation:** actual (publicado en sbox.game) ⚠️
- **What it solves:** simulación realista de vehículos terrestres (física orientada a realismo); el proyecto se publicará también para Garry's Mod. Es la opción "realista" frente a la arcade.
- **Exact files/components:** no verificados ⚠️
- **Verdict:** **PATTERN / monitorizar** — referencia de física realista en s&box; esperar a la release de s&box para valorar ADAPT.
- **Confidence:** MEDIA (existencia y enfoque verificados; detalles ⚠️)

### 6. Togg Sedan Car Addon (sbox-community)

- **Name:** S&box Togg Sedan Car Addon
- **URL/package:** https://github.com/sbox-community/sbox-togg-sedan-car-addon · https://sbox.game/sboxcommunity/toggsedan
- **Exact revision:** no verificada ⚠️
- **Last update:** no verificado ⚠️ ("Experimental")
- **License code:** no verificado ⚠️
- **License assets:** modelo Togg Sedan (assets de coche real, cuidado con el uso comercial ⚠️)
- **Dependencies:** no verificadas ⚠️
- **s&box API generation:** presumiblemente actual ⚠️
- **What it solves:** ejemplo de addon de coche concreto en s&box (comunidad). Faltan features: sonidos, luces, partículas y "driver based things".
- **Exact files/components:** prefab de coche + configuración de vehículo (no detallado ⚠️)
- **Verdict:** **ASSETS / referencia** — sirve como ejemplo real de cómo empaquetar un coche (modelo + componentes) en la API actual, no como base de física.
- **Confidence:** MEDIA (existencia verificada; calidad/estado ⚠️)

### 7. API oficial de s&box (docs) — GameObject, DamageInfo, Storage

- **Name:** API y docs oficiales de s&box (referencia)
- **URL/package:**
  - https://sbox.game/dev/api/Sandbox.GameObject (GameObject, componentes, prefabs)
  - https://sbox.game/api/Sandbox.DamageInfo (sistema de daño oficial: "Describes the damage that should be done to something... class derivable para sistemas propios")
  - https://sbox.game/dev/doc/systems/storage-ugc/ (Storage UGC: gestión de contenido/save, base para inventario persistente)
  - https://sbox.game/dev/doc/assets/
- **Exact revision:** docs en vivo (sin versionado ⚠️)
- **Last update:** en curso (docs oficiales)
- **License code:** documentación oficial de la API del motor (MIT engine, ver candidato 8)
- **License assets:** n/a (docs)
- **Dependencies:** n/a
- **s&box API generation:** **actual** — es la definición de la generación actual (GameObject/Component)
- **What it solves:** patrón oficial para las capas que los kits no cubren: **daño** (`DamageInfo` derivable para daño de vehículo), **trunk/inventario** (storage + componente de inventario propio en el prefab del vehículo) y **enter/exit** (componentes en el asiento + parenteo del jugador).
- **Exact files/components:** `Sandbox.GameObject` (núcleo de la API nueva), `Sandbox.DamageInfo` (daño), Storage UGC (persistencia). No hay componente oficial de "trunk" ni "vehicle seat" verificado en estas búsquedas ⚠️.
- **Verdict:** **PATTERN** — base obligatoria para implementar daño/maletero/enter-exit propios sobre la API actual.
- **Confidence:** ALTA (URLs oficiales verificadas)

### 8. Facepunch/sbox-public (repo oficial del motor)

- **Name:** sbox-public (Facepunch)
- **URL/package:** https://github.com/Facepunch/sbox-public
- **Exact revision:** repo vivo, sin revisión fija ⚠️
- **Last update:** activo (issues/API en curso)
- **License code:** **MIT** (según reportes de comunidad de que el motor pasó a open-source con licencia MIT; no verificado en esta sesión ⚠️)
- **License assets:** n/a (repositorio de API/issues/ejemplos)
- **Dependencies:** n/a
- **s&box API generation:** actual (define la API nueva; issues sobre prefabs/componentes: ej. issue #5814 "Components randomly get removed from prefabs", #3867 sobre referencias en prefabs)
- **What it solves:** fuente de verdad de la API actual de s&box + reportes de bugs conocidos (importante: hay issues abiertos sobre estabilidad de prefabs/componentes que afectan a vehículos).
- **Exact files/components:** n/a (referencia)
- **Verdict:** **PATTERN / referencia** — consultar para migrar a la API actual y conocer bugs de prefabs.
- **Confidence:** ALTA (repo oficial); licencia ⚠️

### 9. (Referencia externa) ArcadeCarPhysics original — SergeyMakeev

- **Name:** ArcadeCarPhysics (Unity3D, original)
- **URL/package:** https://github.com/SergeyMakeev/ArcadeCarPhysics
- **Exact revision / Last update / License code / License assets / Dependencies / API:** proyecto Unity; "can be used for games like GTA, Rocket League or Flatout" ⚠️ (fuera de s&box; no aplica directamente)
- **What it solves:** física arcade de referencia que inspiró el port de matekdev (candidato 4)
- **Verdict:** **PATTERN** (diseño de física arcade consultable, no portable directo)
- **Confidence:** ALTA (existencia verificada)

---

## Recomendaciones

**OPCIÓN PRINCIPAL — ADOPT:** **Vehicle Physics Kit + Vehicle Prototyping (sbox Field Guide)**
- Capa de *física* (ruedas raycast, slip-curve, drivetrain, assists, chase cam) + capa de *montaje/prototipado* (VehicleFactory, PartKitFactory, asiento de conductor) sobre la API actual de GameObject/Component. Activos en 2026 (v313827, 2026-07-22), sin arte requerido, fuente visible en sbox.game.
- **Cobertura de las 4 necesidades:** física ✅ (kit) · enter/exit parcial (asiento del conductor en prototyping; el enter/exit completo hay que implementarlo o tomarlo de CAVC) · maletero ❌ (implementar con componente propio en el prefab + Storage UGC como patrón) · daño ❌ (implementar derivando de `Sandbox.DamageInfo`).
- **Acción previa (verificar ⚠️):** confirmar licencia del paquete y revisión exacta en sboxdb.dev, e inspeccionar el source online antes de integrar.

**Alternativa 1 — ADAPT/verificar:** **CAVC — Clearly A Vehicle Controller** (sbox.game/clearly/cavc)
- Framework de vehículos más amplio ("coches y otros vehículos") con coche demo. Si su modelo de asientos/enter-exit es genérico y la licencia permite, es el complemento ideal para la interacción. Requiere inspección del repo/GitHub (URL truncada en la búsqueda) y de su estado frente a la API actual.

**Alternativa 2 — ADAPT:** **matekdev/sbox-arcade-car-physics**
- Física arcade (feeling GTA/Flatout) con soporte multijugador, en GitHub. Buena si se prefiere un handling sencillo sobre el kit realista; verificar licencia y actualidad del port contra la API nueva.

**Alternativa 3 — PATTERN/monitorizar:** **ZCars (FosterZ)**
- Física realista open-source de referencia; cuando publique su versión s&box, reevaluar como base de física realista. Mientras tanto, usar como referencia de diseño.

**Complementos transversales (PATTERN):** docs oficiales — `Sandbox.GameObject` (API actual), `Sandbox.DamageInfo` (daño), Storage UGC (persistencia del maletero) y `Facepunch/sbox-public` (bugs conocidos de prefabs: #5814, #3867). **sbox-community/sbox-togg-sedan-car-addon** como ejemplo ASSETS de empaquetado de un coche concreto en s&box.

**Huecos del ecosistema (oportunidad de diseño propio):** ningún paquete encontrado cubre *maletero/inventario* ni *daño de vehículo* de forma empaquetada → son implementación propia (patrones oficiales arriba). Enter/exit depende del framework elegido (Field Guide expone el asiento; CAVC podría dar el flujo completo).
