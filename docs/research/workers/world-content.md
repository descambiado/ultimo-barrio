# Catálogo de Assets (World Content) — "Último Barrio" (s&box)

- **Fecha**: 2026-08-07
- **Worker**: world-content (investigación de assets con licencia clara)
- **Método**: 15 búsquedas web independientes (sbox.game, asset.party, GitHub Facepunch, docs oficiales, tiendas externas)
- **Contexto técnico**: rama base con API actual de s&box (GameObject/Component, prefabs JSON, paquetes cloud tipo `thieves.rpdowntown3t`). Los paquetes de sbox.game/asset.party se referencian con ID `autor.paquete` y se montan como dependencias; el código los enlaza vía `Sandbox.Package` (sbox.game/api/Sandbox.Package).
- **Convención de confianza**: `⚠️` = dato no verificable en esta sesión (revisar enlazando el paquete en el editor). No se inventan versiones.

---

## 0. Fuentes oficiales / infraestructura (aplican a varios bloques)

### 0.1 s&box Assets — colección oficial Facepunch
- **Name**: s&box assets (Facepunch official collection)
- **URL/package**: `https://sbox.game/facepunch/sboxassets` (paquete cloud: `facepunch.sboxassets`)
- **Exact revision/version**: ⚠️ no verificada (colección viva; se actualiza con cada release del editor)
- **Last update**: ⚠️ continua (colección mantenida por Facepunch)
- **License code**: Facepunch free-to-use ("free-to-use across any of your s&box projects however you'd like") — propietaria pero con licencia de uso libre dentro de proyectos s&box. No es CC0 ni redistribuible fuera de s&box.
- **License assets**: modelos/materiales propios de Facepunch (Rust/S&box era), sin atribución requerida
- **Dependencies**: ninguna (paquete raíz de Facepunch)
- **s&box API generation**: actual (paquete cloud nativo, prefabs JSON/GameObject)
- **What it solves**: bloque base de props urbanos/chatarra/barricadas: road barriers ("Barrier Rusty Corner In A Road Barrier"), Oil Drum, Metal Wheely Bin, Storage Box, Stone Wall, Stone Brick, Bitumen Roof Panels, Trench, pallets, etc. (vistos en snippets: "Metal Wheely Bin", "Oil Drum", "Explosive Stone Wall", "Trench", "Storage Box 1 Lid", "Bitumen Roof Panels", "Stone Brick Tile", "Barrier Rusty Corner In A Road Barrier")
- **Exact files/components**: assets bajo `Assets/` del paquete montado (modelos `.vmdl` + materiales `.vmat`); añadir como dependencia de paquete en el proyecto
- **Verdict**: **ADOPT** — es LA fuente primaria gratuita y oficial para chatarra, barricadas, vallas y props de calle
- **Confidence**: ALTA (verificado en 5+ resultados independientes; el listado exacto de modelos es ⚠️ parcial)

### 0.2 s&box Weapons — colección de armas Facepunch
- **Name**: s&box Weapons
- **URL/package**: `https://sbox.game/facepunch/sboxweapons` (`facepunch.sboxweapons`)
- **Exact revision/version**: ⚠️ no verificada
- **Last update**: ⚠️ (colección mantenida por Facepunch)
- **License code**: Facepunch free-to-use en proyectos s&box (propietaria, uso libre)
- **License assets**: modelos de armas y attachments hechos por Facepunch
- **Dependencies**: ninguna
- **s&box API generation**: actual
- **What it solves**: TODO el bloque de armas: "Weapon models and attachments made and set up by Facepunch. Magazines, Bullets and Modular Attachments are available separately"
- **Exact files/components**: modelos de armas, attachments modulares, magazines, bullets (bajo `Assets/` del paquete)
- **Verdict**: **ADOPT** — armas base listas para usar sin coste ni fricción de licencia
- **Confidence**: ALTA (descripción oficial verificada)

### 0.3 First-Person Weapons (doc oficial + paquete de brazos)
- **Name**: First-Person Weapons (ready-to-use assets) + ViewModel citizen arms
- **URL/package**: `https://sbox.game/dev/doc/assets/ready-to-use-assets/first-person-weapons` · paquete `facepunch.v_first_person_arms_citizen` (`https://sbox.game/facepunch/v_first_person_arms_citizen`)
- **Exact revision/version**: ⚠️ no verificada
- **Last update**: ⚠️
- **License code**: Facepunch free-to-use en proyectos s&box
- **License assets**: armas listas para usar y brazos/manos primera persona (citizen)
- **Dependencies**: Citizen rig
- **s&box API generation**: actual
- **What it solves**: viewmodels y armas FPS ya rigueadas (evita riggear brazos desde cero)
- **Exact files/components**: paquete `facepunch.v_first_person_arms_citizen` (brazos/manos); armas en doc oficial
- **Verdict**: **ADOPT**
- **Confidence**: ALTA (doc oficial + paquete oficial verificados)

### 0.4 Citizen Characters (modelo de personaje oficial)
- **Name**: Citizen Characters
- **URL/package**: `https://sbox.game/dev/doc/assets/ready-to-use-assets/citizen-characters`
- **Exact revision/version**: ⚠️ no verificada
- **Last update**: ⚠️
- **License code**: Facepunch free-to-use en proyectos s&box
- **License assets**: modelo Citizen (player model por defecto) + sistema de clothing
- **Dependencies**: ninguna (viene en el juego: `addons/citizen/Assets` según la doc)
- **s&box API generation**: actual
- **What it solves**: civiles Y enemigos reutilizando el mismo rig (reskin/color/materials sucios); Facepunch publica ~73 items de clothing en su org (`sbox.game/facepunch`, "1410 model, 637 material, 73 clothing")
- **Exact files/components**: `addons/citizen/Assets` (modelos, animaciones, clothing)
- **Verdict**: **ADOPT** (civiles) + **ADAPT** (enemigos: materiales sucios/tint sobre el mismo rig)
- **Confidence**: ALTA (doc oficial verificada)

### 0.5 Asset Packs — org comunitaria de packs CC0
- **Name**: Asset Packs (org "asset")
- **URL/package**: `https://sbox.game/asset`
- **Exact revision/version**: ⚠️ (org con múltiples packs; listar dentro del editor)
- **Last update**: ⚠️
- **License code**: **CC0** (org: "provide free original CC0 asset packs") — no afiliada a Facepunch
- **License assets**: packs originales de la comunidad bajo CC0 (redistribuibles)
- **Dependencies**: variables por pack
- **s&box API generation**: actual
- **What it solves**: huecos de contenido que Facepunch no cubre (muebles, herramientas, etc.) con licencia permisiva real
- **Exact files/components**: ⚠️ revisar packs individuales en el navegador cloud
- **Verdict**: **ADOPT** — primera parada para lo que no esté en las colecciones oficiales
- **Confidence**: ALTA (misión/CC0 verificados); contenido por pack ⚠️

### 0.6 asset.party (infraestructura de paquetes)
- **Name**: asset.party + API `Sandbox.Package`
- **URL/package**: `https://asset.party` · `https://sbox.game/api/Sandbox.Package`
- **Exact revision/version**: ⚠️ (catálogo vivo)
- **Last update**: ⚠️
- **License code**: **NO estandarizada** — la plataforma no exige licencia al subir (feature request abierta: github.com/Facepunch/sbox-public/issues/5130 "Add the ability to add licenses to assets uploaded to asset.party"). Verificar pack a pack con su autor.
- **License assets**: variable (cada autor pone sus términos; algunos CC0, muchos sin indicar)
- **Dependencies**: `Sandbox.Package` permite declarar dependencias a otros paquetes de Asset Party
- **s&box API generation**: actual
- **What it solves**: catálogo masivo de modelos/sonidos/mapas/juegos listos para montar en el editor
- **Exact files/components**: paquete → `Assets/` montado
- **Verdict**: **PATTERN** (infraestructura a usar) + **DISCARD** de cualquier pack sin licencia explícita para redistribución
- **Confidence**: ALTA (funcionamiento verificado); licencias individuales ⚠️

### 0.7 Marco legal de la plataforma
- **Name**: s&box EULA + acuerdo Valve (exportación)
- **URL/package**: `https://facepunch.com/legal/sbox/eula` · news: `https://sbox.game/news/update-26-03-25` (licencia con Valve firmada 26/03/25, permite exportar juegos standalone royalty-free)
- **Exact revision/version**: EULA vigente 2026 ⚠️ (leer antes de publicar)
- **License code**: EULA propietaria de Facepunch; exportación de juegos a Steam aprobada por Valve
- **What it solves**: confirma que un modo de juego s&box (y su exportación futura) es legal sin royalties; **no** da permiso para redistribuir assets de terceros
- **Verdict**: **PATTERN** — referencia legal obligatoria antes de publicar
- **Confidence**: ALTA (EULA y noticia verificadas)

### 0.8 Mapas existentes como base de escena
- **Name**: Rp Downtown (remake Three Thieves)
- **URL/package**: `https://sbox.game/thieves/rpdowntown3t/` → ID `thieves.rpdowntown3t` (confirmado como patrón de ID en physgun.com y usado por RP existentes)
- **Exact revision/version**: ⚠️ (versión 93645 vista en un resultado, no confirmada como la última)
- **Last update**: ⚠️
- **License code**: paquete propietario del autor (Three Thieves); uso como mapa base ⚠️ requiere OK del autor / términos del paquete
- **What it solves**: escenario urbano ya construido (calles, edificios) para ambientar "Último Barrio"
- **Verdict**: **ADOPT** como dependencia de mapa base si ya está en el proyecto (contexto actual), pero **DISCARD** como fuente de assets copiables a otros paquetes — no redistribuir sus modelos
- **Confidence**: MEDIA (existencia/ID verificados; términos del autor ⚠️)

---

## 1. Muebles

| Candidato | Licencia | Qué cubre | Veredicto |
|---|---|---|---|
| `facepunch.sboxassets` (0.1) | FP free-to-use | sillas/mesas/mobiliario urbano entre sus 1400+ modelos ("Wall 2 chair" visto en org FP) | **ADOPT** |
| Org CC0 `sbox.game/asset` (0.5) | CC0 | packs de muebles comunitarios (buscar "furniture"/"kitchen" en cloud browser) ⚠️ | **ADOPT** |
| Kenney Furniture Kit — `https://kenney.nl/assets/furniture-kit` | **CC0** (Kenney estándar) | muebles low-poly genéricos (sillones, mesas, camas, estanterías) en FBX/GLTF → importar a `.vmdl` vía ModelDoc | **ADAPT** (requiere import y retopología opcional; no nativo s&box) |
| Hello World Pack — `https://sbox.game/rwtrcsc/hello_world_pack` | ⚠️ propietaria del autor (rwtrcsc) | "comprehensive 3D asset pack... props, environmental" | **DISCARD** hasta confirmar licencia con autor |
| ArtStation Kitchen Furniture Mega Pack ($9.99) | propietaria comercial | 20k+ modelos (excesivo para un modo) | **DISCARD** (overkill y coste) |

**PRINCIPAL (Muebles)**: `facepunch.sboxassets` + org CC0 `sbox.game/asset`; si falta algo específico, Kenney Furniture Kit (CC0) importado.

---

## 2. Puertas / Barricadas

- **Puerta oficial**: `facepunch.door_single_dev` — `https://sbox.game/facepunch/door_single_dev` ("model updated" por Facepunch; usado en el tutorial oficial "Creating a Door" `https://sbox.game/learn/facepunch/creating-a-door`). Licencia FP free-to-use. **ADOPT** — ALTA.
- **Patrón de puerta jugable**: el tutorial "Creating a Door" (prefab door + hinge + collider) es el PATTERN para puertas con cerrojo/barricada. **ADOPT** (patrón de código/prefab).
- **Barricadas**: `facepunch.sboxassets` incluye "Barrier Rusty Corner In A Road Barrier", barriles, pallets, maderas → combinables como material de barricada física. **ADOPT** — ALTA.
- **Propkits**: `facepunch.sandbox` (`https://sbox.game/facepunch/sandbox`) — juego oficial Sandbox con propkits/prefabs de props y entidades ("Spawning props and entities from the cloud"); usar su set de propkits como referencia/prefabs base. **PATTERN** — ALTA.
- Itch.io "Modular barricaded door (chains), 73 prefabs" — ⚠️ Unity, licencia por ver. **DISCARD/ADAPT** — BAJA.

**PRINCIPAL (Puertas/barricadas)**: prefab puerta propio sobre `facepunch.door_single_dev` + propkits de `facepunch.sandbox` + props de barricada de `facepunch.sboxassets`.

---

## 3. Chatarra (scrap/junk)

- `facepunch.sboxassets`: Oil Drum, Metal Wheely Bin, Storage Box, Trench, barriles, maderas → núcleo del loot visual de chatarra. **ADOPT** — ALTA.
- Colección de props de Tom (@Facepunch) en Reddit (r/sandbox 14uv4zs): props nuevos añadidos al juego, incluye props originales y "scrapped/unreleased assets" — confirmación de que FP publica junk/industrial periódicamente en `facepunch.sboxassets`. **ADOPT** (seguir la colección) — MEDIA.
- lb3d.co "Fallout Style Junk Items" (17 props post-apocalípticos) — ⚠️ licencia por ver (sitio mixto). **ADAPT** si CC0 confirmada — BAJA/⚠️.
- **Fallback externo CC0**: Kenney "Industrial Kit"/"Prop Kit" (kenney.nl, CC0) y Quaternius (CC0) para latas, cajas, tuberías, importados a `.vmdl`. ⚠️ no verificados en esta sesión, marcar al usar. **ADAPT** — MEDIA.

**PRINCIPAL (Chatarra)**: `facepunch.sboxassets` (oil drum, wheely bin, storage box, pallets) + reutilizar props de mapas urbanos.

---

## 4. Herramientas

- **No hay pack oficial nativo específico** detectado en las búsquedas (⚠️ gap).
- Org CC0 `sbox.game/asset` (0.5): buscar packs con tags "tool"/"tools" ⚠️. **ADOPT condicional**.
- `asset.party`: búsqueda "tool" con filtro por autor y verificación de licencia manual (la plataforma no la estandariza). **PATTERN** ⚠️.
- Externo: packs de herramientas CC0 en itch.io (buscar "tools CC0 3D") / Kenney — import a s&box. **ADAPT** ⚠️.
- Nota: el Toolgun de `facepunch.sandbox` ya trae viewmodel de herramienta para el propio modo Sandbox (reutilizable como referencia visual).

**PRINCIPAL (Herramientas)**: búsqueda en org CC0 `sbox.game/asset` + `asset.party` con verificación manual de licencia; fallback import CC0 externo. ⚠️ gap confirmado.

---

## 5. Médico

- **No hay pack médico nativo oficial** detectado (⚠️ gap).
- Fab (antes UE Marketplace) "Hospital Props Pack" — `https://www.fab.com/listings/d27ec4aa-f015-422b-bad1-332f23ba2f29` (vendajes, toallas, manta, higiene) — licencia propietaria por compra (fab Standard License). **ADAPT** (import FBX→vmdl) — MEDIA.
- Sketchfab "Hospital Props Pack" (Studio Lab) — pago, licencia del autor. **ADAPT** ⚠️.
- iHappyStudios "Medical Pack" / SuperHive "Medical Pack (440+ low-poly)" — propietaria comercial. **ADAPT** ⚠️.
- Alternativa libre: muchos props médicos (camillas, botiquines, jeringas) existen en Sketchfab con licencias CC-BY/CC0 — filtrar por licencia y atribuir. **ADAPT** — MEDIA/⚠️.

**PRINCIPAL (Médico)**: filtrar Sketchfab/Fab por CC0/CC-BY (atribución) e importar; comprar solo si se necesita un set coherente (ej. Fab Hospital Props Pack). ⚠️ sin opción nativa.

---

## 6. Generadores / Luces

- **Luces**: entidades nativas del motor — Sandbox Mode incluye "lights, fog and NPCs" como entidades built-in (`https://sbox.game/news/update-26-01-07`). Usar componente de luz (PointLight/SpotLight) en prefabs; **no requiere assets externos**. **ADOPT** — ALTA.
- **Generadores**: sin pack oficial específico; modelos de generadores en `asset.party` (buscar "generator") ⚠️ y en packs industriales de `facepunch.sboxassets` (el "Storage Box"/"Trench" no son generadores; ⚠️ verificar). **ADAPT** — BAJA/⚠️.
- Org CC0 `sbox.game/asset`: probable pack de props industriales ⚠️.

**PRINCIPAL (Generadores/luces)**: luces = entidades nativas; generador = prop importado (CC0 de asset.party/org CC0) + prefab con emisivo y sonido.

---

## 7. Vallas

- `facepunch.sboxassets`: "Road Barrier", "Barrier Rusty Corner", "Stone Wall", "Stone Brick", "Bitumen Roof Panels" → vallas de obra y muros. **ADOPT** — ALTA.
- Valla metálica tipo "wire fence": ⚠️ no vista en los snippets; opciones: búsqueda en `asset.party` (fence) o import CC0 (Kenney "Fence Kit" no existe como tal; Quaternius tiene fences CC0). **ADAPT** ⚠️ — MEDIA.
- Nota Valve wiki: s&box puede montar Quake/GoldSrc/NS2 y usar sus props en el editor (developer.valvesoftware.com/wiki/S&box) — útil como referencia dev, **no** redistribuir en el modo (licencias GPL/GoldSrc incompatibles). **DISCARD** para shipping.

**PRINCIPAL (Vallas)**: `facepunch.sboxassets` (barriers + stone walls) y rejilla metálica CC0 importada si hace falta.

---

## 8. Vehículos

- **CAVC — Clearly A Vehicle Controller** — `https://sbox.game/clearly/cavc/` — framework de vehículos con coche demo en `Assets/prefabs`. Licencia ⚠️ (revisar paquete; frameworks publicados en sbox.game suelen permitir uso con el paquete montado). **ADOPT/PATTERN** — MEDIA/⚠️.
- **Car&Race (Vehicle Tool Example)** — `https://sbox.game/meteorlab/vehicle_tool_example` — ejemplo oficial-ish de conducción + editor de vehículos (abrir/cerrar editor, entrar/salir, voltear). **PATTERN** (referencia de implementación) — MEDIA/⚠️.
- Sandbox mode: "add wheels... full wheel physics" (r/sandbox + `facepunch.sandbox`) — patrón de ruedas sobre props. **PATTERN** — ALTA.
- Modelos de coches: importar CC0 (Kenney "Car Kit" ⚠️, Quaternius vehicles CC0) y adaptar con CAVC. **ADAPT** ⚠️.

**PRINCIPAL (Vehículos)**: CAVC como framework + coche demo; modelos CC0 externos importados. ⚠️ verificar términos del paquete CAVC.

---

## 9. Enemigos

- **Citizen (0.4) reskinnado** = vía rápida: mismo rig, materiales sucios/tint, variantes de ropa (73 clothing de FP). **ADOPT/ADAPT** — ALTA.
- **Zombie Mod [s&box]** — `https://sbox.game/zmp/zm_mod_test` — modo con infección, knockback y "object physics for building barricades": referencia de mecánicas y de enfoque de assets (barricadas con props físicos). Licencia del código ⚠️ (usar solo como referencia). **PATTERN** — MEDIA/⚠️.
- **Vandalite** — `https://sbox.game/cosmo/vndl` — shooter de zombis con "Models: PS1-style Makarov" — referencia visual/aesthetic para el modo. **PATTERN** (referencia estética) — MEDIA/⚠️.
- Sketchfab "Zombie Hand" (de un modo open source de s&box) — CC? ⚠️. **ADAPT** — BAJA/⚠️.
- Packs de zombies comerciales (SuperHive "Zombie Pack 19 modelos", Fab) — propietarias; **ADAPT** si se compran — ⚠️.
- **No** usar modelos de Quake/GoldSrc para shipping (licencias incompatibles con redistribución cerrada).

**PRINCIPAL (Enemigos)**: Citizen con materiales de "infectado" + animaciones del rig citizen; mecánicas de referencia de Zombie Mod. Para variantes visuales: packs CC0 de zombies (⚠️ verificar) o modelado simple.

---

## 10. Civiles

- **Citizen Characters (0.4)** — modelo por defecto con clothing system; civiles con variantes de ropa/color. **ADOPT** — ALTA.
- Facepunch org (`sbox.game/facepunch`): 1410 modelos, 637 materiales, **73 clothing** — usar clothing oficial para diferenciar civiles. **ADOPT** — ALTA.

**PRINCIPAL (Civiles)**: Citizen + clothing de Facepunch, variantes de color/material por facción.

---

## 11. Armas

- **`facepunch.sboxweapons` (0.2)** — modelos de armas + attachments + magazines + bullets, "made and set up by Facepunch". **ADOPT** — ALTA.
- **First-Person Weapons doc + `facepunch.v_first_person_arms_citizen` (0.3)** — viewmodels y brazos listos. **ADOPT** — ALTA.
- itch.io "CC0 Flat Guns / pack de armas 3D CC0" (`https://itch.io/game-assets/free/tag-first-person`) — CC0 pero 2D/low-poly; import necesario. **ADAPT** — MEDIA/⚠️.
- Tutorial "Custom Weapons on Viewmodels" (YouTube K-WNZ7bS6CM) — cómo sustituir las armas del viewmodel base de Sandbox. **PATTERN** — MEDIA/⚠️ (no es asset).

**PRINCIPAL (Armas)**: `facepunch.sboxweapons` (world models) + brazos citizen (viewmodels); attachments modulares ya incluidos.

---

## Recomendaciones (resumen PRINCIPAL por bloque)

| Bloque | Opción PRINCIPAL | Verdict | Confianza |
|---|---|---|---|
| Muebles | `facepunch.sboxassets` + org CC0 `sbox.game/asset` (+ Kenney CC0 import) | ADOPT/ADAPT | ALTA |
| Puertas/barricadas | `facepunch.door_single_dev` + prefab propio (tutorial Creating a Door) + propkits `facepunch.sandbox` | ADOPT | ALTA |
| Chatarra | `facepunch.sboxassets` (oil drum, wheely bin, storage box, pallets) | ADOPT | ALTA |
| Herramientas | org CC0 `sbox.game/asset` / asset.party (verificación manual de licencia) ⚠️ | ADOPT condicional | MEDIA/⚠️ |
| Médico | Sketchfab/Fab filtrado CC0/CC-BY (atribución) o compra Fab Hospital Props | ADAPT | MEDIA/⚠️ |
| Generadores/luces | Luces: entidades nativas (Sandbox Mode). Generador: prop importado ⚠️ | ADOPT/ADAPT | ALTA/MEDIA |
| Vallas | `facepunch.sboxassets` (road barrier, stone wall) + rejilla CC0 importada | ADOPT/ADAPT | ALTA/MEDIA |
| Vehículos | CAVC framework + coche demo; modelos CC0 importados ⚠️ términos | ADOPT/PATTERN | MEDIA/⚠️ |
| Enemigos | Citizen reskinnado (rig + materiales sucios); mecánicas ref. Zombie Mod | ADOPT/ADAPT | ALTA |
| Civiles | Citizen + 73 clothing de Facepunch | ADOPT | ALTA |
| Armas | `facepunch.sboxweapons` + `facepunch.v_first_person_arms_citizen` | ADOPT | ALTA |

## Reglas de oro para el repo

1. **Fuente primaria**: colecciones oficiales de Facepunch (sboxassets, sboxweapons, citizen, arms) — licencia FP free-to-use, cero fricción legal, nativas de la API actual (paquete cloud + prefabs JSON/GameObject).
2. **Licencias de terceros**: la plataforma (asset.party/sbox.game) NO estandariza licencias (issue #5130 abierto) → solo usar packs con licencia explícita (CC0/CC-BY con atribución) o autorización del autor por escrito. Todo lo que no se pueda redistribuir queda como dependencia de paquete (no se copia al repo).
3. **Import de assets externos** (Kenney, Quaternius, Sketchfab CC0, itch.io CC0): se convierten a `.vmdl`/`.vmat` en el editor (ModelDoc) y se guardan en `Assets/` del proyecto; registrar la licencia original y atribución en `docs/licenses/`.
4. **No redistribuir** modelos de Quake/GoldSrc/HL (solo montaje dev) ni assets de mapas tipo `thieves.rpdowntown3t` (propiedad de sus autores).
5. Gaps confirmados a cubrir con creación propia o compra: **médico**, **herramientas**, **generadores**, **vallas metálicas**.
