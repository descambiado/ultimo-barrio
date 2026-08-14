# Investigación INVENTARIO/UI — s&box (Último Barrio)

**Fecha:** 2026-08-07 · **Worker:** investigación inventario/UI · **Contexto:** rama base con API actual de s&box (GameObject, componentes, prefabs JSON, Razor UI). Objetivo: catálogo para inventario, grids, UI de loot/crafting/trader.

**Hallazgo crítico:** en el update **26.07.08** (julio 2026) Facepunch añadió **API de inventario, arma y munición de primera parte** ("First-party inventory, weapon, ammo, camera shake, camera modifier, tracer, and scene anchor APIs" — https://sbox.game/news/update-26-07-08). Esto cambia la estrategia: hoy existe un sistema oficial de inventario por slots sobre el componente actual, y los paquetes de comunidad mayoritariamente anteriores a este cambio deben revisarse contra él.

---

## 1. First-party s&box Inventory / Weapon / Ammo API (oficial)

- **Name:** s&box Inventory API (primera parte) — `Sandbox.Inventory` (componentes)
- **URL/package:** https://sbox.game/api/i/components · https://sbox.game/news/update-26-07-08 · docs: https://sbox.game/dev/doc (sección "Inventory & Weapons" 🎒Inventory / 🔫Weapons)
- **Exact revision:** no determinable (API generada del código fuente en vivo; update 26.07.08)
- **Last update:** 2026-07-08 (update semanal oficial)
- **License code:** propietario, incluido en la engine de s&box (uso gratuito dentro del ecosistema s&box)
- **License assets:** n/d
- **Dependencies:** ninguna externa; parte del framework `Sandbox`
- **s&box API generation:** **ACTUAL** (GameObject/Component, .NET 10)
- **What it solves:** inventario de slots de primera parte, host-autoritativo, con items como GameObjects hijo; el inventario rastrea cuál está activo y lo habilita/deshabilita al cambiar de slot. Modelo oficial para hotbar/equipación de armas y objetos. Sustituye la necesidad de implementar la sincronización de inventario a mano.
- **Exact files/components:** componente de inventario basado en slots de `Sandbox.BaseInventoryItem`; items almacenados como child GameObjects; "Host authoritative - clients request…" (cambios validados por host). Ver también docs oficiales de UI para render (VirtualGrid, Razor).
- **Verdict:** **ADOPT** — es la solución oficial sobre la API actual de la rama base; usar como núcleo del inventario de Último Barrio (hotbar + equipación). Complementar con UI propia en Razor (PATTERN del sbdm de Facepunch, ver §6).
- **Confidence:** **ALTA** (fuente oficial verificada vía sbox.game/news y sbox.game/api). Detalles de firma exacta de la API no inspeccionados → ⚠️ en cuanto a nombres de clases/métodos exactos.

---

## 2. Modular Inventory System (llad)

- **Name:** Modular Inventory System
- **URL/package:** https://sbox.game/llad/modularinventorysystem (package en sbox.game; source view: `.../source?file=InventorySlot.cs`)
- **Exact revision:** no verificable ⚠️
- **Last update:** no verificable ⚠️
- **License code:** no verificable desde la búsqueda ⚠️ (pendiente mirar ficha del package en sbox.game)
- **License assets:** no verificable ⚠️
- **Dependencies:** no verificadas ⚠️
- **s&box API generation:** probablemente **ACTUAL** (alojado en sbox.game con source view de fichero C#), no confirmado ⚠️
- **What it solves:** exactamente nuestro alcance: "A modular, drop-in inventory system with hotbar, equipment, crafting, chests, and loot tables. Every feature is toggleable." Es el candidato comunitario más cercano a las necesidades de Último Barrio (inventario + loot + crafting + chests).
- **Exact files/components:** `InventorySlot.cs` visible; paquete modular con hotbar, equipment slots, crafting, chests, loot tables, cada feature desactivable (nombres de ficheros restantes no inspeccionados ⚠️).
- **Verdict:** **ADAPT** — candidato fuerte como base para inventario + crafting + loot; requiere verificación de licencia, estado de mantenimiento y compatibilidad con la API de inventario de primera parte (que es posterior). Si su capa de red usa la API legacy, adaptar.
- **Confidence:** **MEDIA** ⚠️ (solo snippet del buscador; sin inspección de repo/licencia).

---

## 3. Tetris Inventory — kurozael/sbox-inventory

- **Name:** Tetris Inventory (sbox-inventory)
- **URL/package:** https://sbox.game/conna/inventory · GitHub: https://github.com/kurozael/sbox-inventory (autor: Conna Wiles / kurozael)
- **Exact revision:** no verificable ⚠️
- **Last update:** no verificable ⚠️ (repo antiguo; la página del package sigue en pie)
- **License code:** no verificable ⚠️
- **License assets:** n/d
- **Dependencies:** ninguna conocida
- **s&box API generation:** **LEGACY** presumible (creado en la era previa al sistema de componentes actual; requiere migración) ⚠️
- **What it solves:** inventario estilo Tetris (grid con items de tamaño variable) con sincronización de red. Útil si Último Barrio quiere grid tipo Escape from Tarkov en lugar de slots uniformes.
- **Exact files/components:** repo `sbox-inventory` con lógica de grid y sync de red; página de package con "usage and examples" (no inspeccionados ⚠️).
- **Verdict:** **PATTERN** — más que adoptar el código (API legacy), extraer el patrón de grid tetris (tamaños por item, detección de espacio) y reimplementarlo sobre la API actual; solo si el diseño del juego pide grids complejos. Si la UI se queda en slots uniformes → **DISCARD**.
- **Confidence:** **MEDIA** ⚠️ (existencia verificada; API generation y licencia inferidas, no confirmadas).

---

## 4. SUI Designer — KiKoZl1/sbox-ui-designer

- **Name:** Sbox UI Designer (SUI Designer)
- **URL/package:** https://sbox.game/kikozl/sbox_ui_designer · GitHub: https://github.com/KiKoZl1/sbox-ui-designer · docs: https://kikozl1.github.io/sbox-ui-designer/
- **Exact revision:** no verificable ⚠️ (repo activo con CHANGELOG.md en `main`)
- **Last update:** reciente (activo en 2026; post de Reddit r/sandbox reciente, CHANGELOG en `main`) ⚠️
- **License code:** **MIT** (indicado en la ficha de sbox.game: "License: MIT") — código del editor/tool, no del juego
- **License assets:** n/d (genera código, no assets)
- **Dependencies:** s&box editor (tool que corre dentro del editor); genera `.razor` + `.razor.scss` nativos
- **s&box API generation:** **ACTUAL** (genera Razor + SCSS nativo del sistema de UI actual)
- **What it solves:** edición visual de UI tipo UMG dentro del editor de s&box; WYSIWYG sobre documentos `.sui`; genera Razor + SCSS limpios; preview con "Test in Play". Incluye elementos específicos de inventario: Panel, Text, Image, Grid, **InventoryGrid**, **InventorySlot**, **Hotbar**, inventory screen, death modal.
- **Exact files/components:** `.sui` como fuente; generación de `.razor` y `.razor.scss`; 16 tipos de elemento incl. InventoryGrid / InventorySlot / Hotbar.
- **Verdict:** **ADOPT** (herramienta) — acelera enormemente el diseño de grids, slots y hotbar en Razor/SCSS nativo de la API actual, licencia MIT. No es un sistema de inventario: se combina con el API de primera parte (§1) o el paquete de llad (§2).
- **Confidence:** **ALTA** (existencia, licencia MIT y generación Razor/SCSS verificadas vía sbox.game + GitHub). Revisión exacta concreta ⚠️.

---

## 5. Razor Designer (xaz / sklmr)

- **Name:** Razor Designer
- **URL/package:** https://sbox.game/xaz/razordesigner → movido a https://sbox.game/sklmr/razordesigner
- **Exact revision:** no verificable ⚠️
- **Last update:** no verificable ⚠️
- **License code:** no verificable ⚠️
- **License assets:** n/d
- **Dependencies:** s&box editor
- **s&box API generation:** ACTUAL presumible (edita Razor) ⚠️
- **What it solves:** editor de UI Drag & Drop para Razor; alternativa directa a SUI Designer.
- **Exact files/components:** editor visual de documentos Razor (componentes no detallados ⚠️).
- **Verdict:** **ADAPT** (alternativa) — segundo editor visual de UI; evaluar frente a SUI Designer (éste documenta explícitamente InventoryGrid/InventorySlot/Hotbar, así que SUI parece más específico para nuestro caso).
- **Confidence:** **BAJA-MEDIA** ⚠️ (solo ficha de sbox.game; no inspeccionado).

---

## 6. Facepunch/sbdm — UI/Inventory.razor (referencia oficial)

- **Name:** s&box Deathmatch (sbdm) — componente de UI de inventario
- **URL/package:** https://sbox.game/facepunch/sbdm/source?file=UI%2FInventory.razor
- **Exact revision:** no verificable (código en vivo del juego oficial Facepunch) ⚠️
- **Last update:** en mantenimiento continuo por Facepunch ⚠️
- **License code:** código del juego oficial de Facepunch en sbox.game; consultar términos del package ⚠️
- **License assets:** n/d
- **Dependencies:** framework Sandbox + API de componentes de Facepunch
- **s&box API generation:** **ACTUAL** (Razor UI + componente de inventario de primera parte)
- **What it solves:** referencia canónica de cómo renderizar un hotbar de inventario: "renders up to 5 inventory slots, tracks hovered/active/previous carryable items, handles input for slot switching".
- **Exact files/components:** `UI/Inventory.razor` (slots, hover, activo/anterior, input de cambio de slot); acompañado del componente de inventario host-authoritativo del juego.
- **Verdict:** **PATTERN** — usar como plantilla oficial de hotbar + slot switching sobre la API actual; copiar patrones de binding Razor → componente de inventario.
- **Confidence:** **ALTA** (existencia y descripción verificadas en sbox.game). Detalle de código no leído ⚠️.

---

## 7. VirtualGrid (documentación oficial de UI)

- **Name:** VirtualGrid
- **URL/package:** https://sbox.game/dev/doc/ui/virtualgrid
- **Exact revision:** n/a (documentación oficial viva)
- **Last update:** n/a ⚠️
- **License code:** n/a (docs oficiales)
- **License assets:** n/d
- **Dependencies:** framework Sandbox (Panel de UI)
- **s&box API generation:** **ACTUAL**
- **What it solves:** "VirtualGrid is a Panel that allows you to create a grid of items virtually… if you have 1 million items, it won't render [todos]". Renderizado de grids de inventario grandes sin penalización de rendimiento.
- **Exact files/components:** clase `VirtualGrid` (Panel virtualizado para grids de items).
- **Verdict:** **ADOPT** (patrón oficial) — para la grilla de inventario/loot de Último Barrio si los contenedores pueden crecer; base oficial para cualquier grid.
- **Confidence:** **ALTA** (docs oficiales).

---

## 8. Sandbox.Services.Inventory (Steam Inventory)

- **Name:** Sandbox.Services.Inventory
- **URL/package:** https://sbox.game/api/Sandbox.Services.Inventory
- **Exact revision:** n/a (API generada viva)
- **Last update:** n/a ⚠️
- **License code:** propietario (framework)
- **License assets:** n/d
- **Dependencies:** Steam Inventory
- **s&box API generation:** **ACTUAL**
- **What it solves:** acceso al inventario de Steam del usuario (items/cosméticos del ecosistema s&box), NO inventario de gameplay. Incluye la función de guardar/comprobar ítems del usuario.
- **Exact files/components:** clase de servicio estática `Sandbox.Services.Inventory`.
- **Verdict:** **DISCARD** para el gameplay de Último Barrio (es inventario de plataforma/Steam, no de mundo); **PATTERN** solo si más adelante se monetiza con items de Steam.
- **Confidence:** **ALTA** (documentación oficial del API).

---

## 9. sboxcool.com — "Inventory System for s&box Games" (Network Storage)

- **Name:** Inventory System code example (Network Storage v3)
- **URL/package:** https://sboxcool.com/wiki/network-storage-v3/code-examples/inventory-system
- **Exact revision:** no verificable ⚠️
- **Last update:** no verificable ⚠️ (wiki de terceros)
- **License code:** ejemplo de código, sin licencia declarada ⚠️
- **License assets:** n/d
- **Dependencies:** Network Storage (persistencia server-authoritative)
- **s&box API generation:** ACTUAL presumible (Network Storage v3) ⚠️
- **What it solves:** patrón de inventario stackable server-authoritative con persistencia: "store item quantities on the player row, store item definitions in Game Values, validate purchases in a workflow, then mutate gold…". Diseñado para survival/RPG/shops — encaja con trader de Último Barrio.
- **Exact files/components:** workflow de 4 pasos (cantidades por jugador, definiciones en Game Values, validación de compras, mutación de moneda).
- **Verdict:** **PATTERN** — adoptar el patrón de persistencia y validación de compras/ventas para el trader; no como librería.
- **Confidence:** **MEDIA** ⚠️ (wiki de terceros, no inspeccionada a fondo).

---

## 10. alessandrobelli.it — "One box, one item: building a simple inventory in s&box"

- **Name:** Blog post: inventario simple en s&box (Alessandro Belli)
- **URL/package:** https://alessandrobelli.it/one-box-one-item-building-deliberately-simple-inventory-in-s-box/
- **Exact revision:** n/a (artículo)
- **Last update:** reciente (post-2025, API de componentes) ⚠️
- **License code:** texto/ejemplo de blog; código mostrado sin licencia ⚠️
- **License assets:** n/d
- **Dependencies:** framework Sandbox
- **s&box API generation:** **ACTUAL** (habla de "a Component is a class you attach to a GameObject, and [Property] exposes a field in the editor")
- **What it solves:** defensa y código real de un inventario deliberadamente simple: `List<InventorySlot>` de longitud fija, un slot = un item + contador, stacking aritmético, sin grid Tetris. Útil como antídoto de scope para un modo de juego.
- **Exact files/components:** componente de inventario con `[Property]`, `List<InventorySlot>`; ejemplo completo en el post.
- **Verdict:** **PATTERN** — referencia de diseño (scope control) y de cómo escribir componentes contra la API actual; alineado con el enfoque "un cajón, un item, stackable" si el diseño de Último Barrio no exige grid Tetris.
- **Confidence:** **ALTA** (post con código real sobre API de componentes).

---

## 11. sbox-ui-razor (skill para agentes) — echohello-dev/basebound

- **Name:** sbox-ui-razor skill (Skills Marketplace)
- **URL/package:** https://lobehub.com/skills/echohello-dev-basebound-sbox-ui-razor (base: echohello-dev/basebound)
- **Exact revision:** no verificable ⚠️
- **Last update:** no verificable ⚠️
- **License code:** no verificable ⚠️
- **License assets:** n/d
- **Dependencies:** razonador/agente (Claude u otro) + s&box
- **s&box API generation:** ACTUAL (Razor UI)
- **What it solves:** skill para que un agente construya UI de s&box en Razor (HUD, menús, healthbars, **inventario con drag/drop**) sin caer en patrones de Unity.
- **Exact files/components:** conocimiento procedural para generar Razor; incluye drag/drop inventory.
- **Verdict:** **PATTERN/ASSETS** — útil para el flujo de desarrollo con agentes (junto a claude-sbox, §13), no es un sistema.
- **Confidence:** **MEDIA** ⚠️.

---

## 12. Ryhon0/awesome-sbox (índice de proyectos open-source)

- **Name:** awesome-sbox
- **URL/package:** https://github.com/Ryhon0/awesome-sbox
- **Exact revision:** n/a — **archivado por el autor**
- **Last update:** archivado (sin mantenimiento) ⚠️
- **License code:** n/a (lista de enlaces)
- **License assets:** n/a
- **Dependencies:** n/a
- **s&box API generation:** mixta (lista histórica)
- **What it solves:** índice de proyectos s&box open-source (código, materiales, modelos gratis) — descubrimiento de alternativas de inventario/UI no cubiertas aquí.
- **Exact files/components:** README con enlaces categorizados.
- **Verdict:** **DISCARD** como candidato directo; conservar como índice de descubrimiento. Ojo: muchos enlaces apuntan a código legacy.
- **Confidence:** **ALTA** (archivado confirmado).

---

## 13. Facepunch/sboxassets (assets oficiales gratuitos)

- **Name:** s&box assets (Facepunch)
- **URL/package:** https://sbox.game/facepunch/sboxassets
- **Exact revision:** n/a ⚠️
- **Last update:** mantenido por Facepunch ⚠️
- **License code:** n/a
- **License assets:** "free-to-use across any of your s&box projects" (uso libre declarado en la colección)
- **Dependencies:** n/a
- **s&box API generation:** n/a (assets)
- **What it solves:** modelos y materiales oficiales (incl. sección "Inventory & Weapons" en https://sbox.game/dev/doc/assets/ y "First-Person Weapons") para prototipar items, armas y la UI de Último Barrio sin crear assets.
- **Exact files/components:** colección de modelos/materiales; "First-person weapons" listos para usar (bonemerge de brazos) con contador de munición.
- **Verdict:** **ASSETS** — usar para prototipado inmediato de ítems/armas.
- **Confidence:** **ALTA** (declaración oficial de uso libre).

---

## 14. S&Box Development Guide — SubZero Studios (guía Steam)

- **Name:** S&Box Development Guide (SubZero Studios)
- **URL/package:** https://steamcommunity.com/sharedfiles/filedetails/?id=3595903475
- **Exact revision:** n/a (guía viva) ⚠️
- **Last update:** 2026 ⚠️
- **License code:** texto de guía, sin licencia de código ⚠️
- **License assets:** n/d
- **Dependencies:** n/a
- **s&box API generation:** **ACTUAL** (describe Panel tree, flexbox, style tokens Goo.Tokens, drag & drop, .scene/.prefab JSON)
- **What it solves:** guía comunitaria de desarrollo: UI = árbol de Panels + stylesheets flexbox, drag and drop, tokens de estilo; contexto general del sistema actual (útil para que el equipo no mezcle APIs).
- **Exact files/components:** conceptos (Panel, stylesheets, drag & drop, Goo.Tokens) más que componentes concretos.
- **Verdict:** **PATTERN** — referencia de contexto para el equipo; no aporta sistema de inventario.
- **Confidence:** **MEDIA** ⚠️ (guía de terceros).

---

## 15. gavogavogavo/claude-sbox (skill de código s&box para agentes)

- **Name:** claude-sbox
- **URL/package:** https://github.com/gavogavogavo/claude-sbox
- **Exact revision:** no verificable ⚠️
- **Last update:** no verificable ⚠️
- **License code:** no verificable ⚠️
- **License assets:** n/d
- **Dependencies:** Claude Code + s&box
- **s&box API generation:** **ACTUAL** (C# components, Razor UI, physics, networking)
- **What it solves:** skill que enseña a Claude a escribir código s&box idiomático (componentes C#, Razor UI, networking) evitando patrones de Unity — mejora la calidad de código generado por agentes en el repo.
- **Exact files/components:** instrucciones procedurales del skill (componentes C#, Razor UI).
- **Verdict:** **PATTERN/ASSETS** — adoptar en el flujo de desarrollo agentizado de Último Barrio (complementa a §11).
- **Confidence:** **MEDIA** ⚠️.

---

## Recomendaciones

### PRINCIPAL: API de inventario de primera parte + UI Razor propia con SUI Designer
1. **Núcleo de gameplay:** usar el **inventario de slots de primera parte de s&box** (update 26.07.08, host-authoritativo, items como child GameObjects) como base de hotbar/equipación de Último Barrio. Es la opción oficial sobre la API actual de la rama base (GameObject/componentes), sin dependencias de terceros ni riesgo de licencia.
2. **UI:** diseñar grids/slots/hotbar con **SUI Designer (MIT)** → genera Razor + SCSS nativos (InventoryGrid, InventorySlot, Hotbar ya incluidos); renderizar grids grandes con **VirtualGrid**; usar **UI/Inventory.razor de Facepunch sbdm** como plantilla de hotbar con slot-switching.
3. **Assets:** prototipar items/armas con **sboxassets** (uso libre declarado).

### Alternativas
- **A1 — Modular Inventory System (llad) como capa rica:** si se necesita crafting + chests + loot tables "out of the box", evaluar este package (todo toggleable) **antes** de escribir el sistema propio; verificar licencia, mantenimiento y que no use API legacy. Verdict: ADAPT.
- **A2 — Grid tipo Tetris (kurozael/sbox-inventory) como patrón:** solo si el diseño pide grids con items de tamaño variable (estilo Tarkov); reimplementar el patrón sobre la API actual en lugar de portar el código (repo de era legacy). Verdict: PATTERN.
- **A3 — Persistencia y trader con Network Storage (sboxcool):** para economía/trader con validación de compras y moneda persistente, aplicar el patrón de 4 pasos (cantidades por jugador en Network Storage, definiciones en Game Values, validación, mutación). Verdict: PATTERN.

### Riesgos / pendientes de verificación
- Revisar la firma exacta de la API de inventario de primera parte (sbox.game/api/i/components) antes de diseñar el modelo de items.
- Verificar licencia y API generation de **Modular Inventory System (llad)** en la ficha del package.
- Confirmar estado de mantenimiento de **kurozael/sbox-inventory** y de **SUI Designer** (CHANGELOG en `main` sugiere actividad reciente).
- El sandbox mode oficial (Tony) estrenó inventario slot-based sobre esta misma API: seguir sus notas de release como fuente de patrones.

*Nota de confianza: todo lo marcado ⚠️ no fue verificable con las búsquedas realizadas (no se inspeccionaron repos en profundidad por límite de llamadas); no se han inventado revisiones, fechas ni licencias.*
