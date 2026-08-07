# Research Worker: WEAPONS — Catálogo de candidatos para el stack de armas de "Último Barrio"

**Fecha:** 2026-08-07 · **Worker:** investigador de armas (subagente)
**Contexto:** rama base de "Último Barrio" sobre la API actual de s&box (escenas GameObject/Component, prefabs JSON, asset store de Facepunch, paquetes tipo `facepunch/sboxweapons`, `thieves.rpdowntown3t`).
**Método:** 14 búsquedas web independientes (GitHub, sbox.game, sbox.game/dev/doc, Reddit r/sandbox, YouTube). Prioridad a fuentes primarias (Facepunch, sbox.game, wiki oficial). Los datos no verificables se marcan ⚠️ y NO se inventan.

---

## 1. facepunch/sboxweapons (colección oficial de armas de Facepunch)

- **Name**: s&box Weapons (colección oficial Facepunch, paquete `facepunch/sboxweapons`)
- **URL/package**: https://sbox.game/facepunch/sboxweapons (espejo de stats: https://sbox.grimtech.co.uk/facepunch/sboxweapons/)
- **Exact revision**: ⚠️ no publicada en resultados; la versión la gestiona el cliente de s&box ("update via package"). Verificar en la página del paquete.
- **Last update**: ⚠️ no verificable por búsqueda (Facepunch la mantiene activamente; hay issues de sbox-issues abiertos sobre bone merging con estas armas).
- **License code**: n/a (es paquete de assets, no código fuente abierto) ⚠️ términos de uso de contenido de Facepunch, no detallados en la búsqueda.
- **License assets**: contenido oficial de Facepunch para uso dentro de s&box (igual que el contenido del engine). ⚠️ Confirmar los términos exactos de redistribución en la página del paquete antes de publicar.
- **Dependencies**: engine de s&box (bootstrap); "Magazines, Bullets and Modular Attachments are available separately" (paquetes separados).
- **s&box API generation**: API actual. Los modelos se consumen como assets (`.vmdl`, animgraphs) dentro del sistema de escenas actual.
- **What it solves**: da modelos de armas y attachments oficiales y de calidad AAA sin coste, con viewmodels/worldmodels ya configurados (hold bones) y un sistema modular de attachments/magazines/bullets.
- **Exact files/components**: modelos de armas (p.ej. `models/weapons/sbox_pistol_usp/v_usp.vmat_c`, `w_usp.vmat_c`, MP5, shotgun) ⚠️ lista completa no verificada; attachments modulares; magazines/bullets en paquetes separados.
- **Verdict**: ASSETS — la opción de assets más segura legalmente (oficial de Facepunch), pero no incluye base de código de armas (hay que escribir los componentes o usar una base de código).
- **Confidence**: ALTA (fuente oficial sbox.game) para existencia/contenido; MEDIA para el detalle de licencias.

---

## 2. Facepunch — First-Person Weapons (doc oficial de assets listos para usar)

- **Name**: First-Person Weapons (documentación oficial de assets, paquete de viewmodels/arms)
- **URL/package**: https://sbox.game/dev/doc/assets/ready-to-use-assets/first-person-weapons
- **Exact revision**: ⚠️ doc viva del repo Facepunch/sbox-docs; sin revisión pinneada.
- **Last update**: ⚠️ continua (docs sincronizadas con Facepunch/sbox-docs).
- **License code**: n/a (documentación + assets oficiales).
- **License assets**: contenido oficial de Facepunch, de uso libre dentro de s&box ⚠️ mismos términos que el contenido del engine.
- **Dependencies**: s&box base; los weapons assets requieren el animgraph "punching" para brazos desnudos/melee.
- **s&box API generation**: API actual (assets consumibles por el sistema de escenas actual; los viewmodels usan el sistema de animación por animgraphs y subgraphs).
- **What it solves**: resuelve el problema de viewmodels/arms en primera persona: brazos FP ya hechos con animgraph "punching" (melee a mano desnuda), huecos de armas con IK bones bajo `weapon_root` (uno por mano), y animación de armas FP lista.
- **Exact files/components**: animgraph "punching" para melee/brazos; subgraphs de animación; brazos FP; dos IK bones bajo `weapon_root`; guía de tiempos de swing para melee (swings cada 400–450 ms recomendados); triggers de ADS ("aim down sights" stance) y recoil escalonado (blend hacia 1 en fuego continuo).
- **Verdict**: PATTERN + ASSETS — documentación oficial canónica y assets FP gratuitos; la referencia correcta para implementar viewmodel, ADS y recoil.
- **Confidence**: ALTA (fuente oficial).

---

## 3. Facepunch Sandbox gamemode — código de armas base (w_usp, WeaponBase)

- **Name**: Sandbox gamemode de Facepunch (contiene `WeaponBase` y las armas `w_usp`, `w_mp5`, etc.)
- **URL/package**: https://sbox.game/facepunch/sandbox (página del juego); código fuente en GitHub ⚠️ repo presumiblemente `github.com/Facepunch/sandbox` (la comunidad lo confirma: "fork the sandbox gamemode from GitHub", r/sandbox); verificar URL exacta.
- **Exact revision**: ⚠️ no verificable por búsqueda; la página de versión vista: https://sbox.game/facepunch/sandbox/version/24874.
- **Last update**: ⚠️ mantenido activamente por Facepunch (es su gamemode de referencia).
- **License code**: ⚠️ MIT presumiblemente (todos los repos públicos de Facepunch s&box son MIT, p.ej. sbox-public); verificar en el repo del sandbox.
- **License assets**: contenido oficial de Facepunch, uso dentro de s&box. ⚠️ Ojo: la comunidad reporta que Facepunch restringe usar el *personaje* default en juegos standalone publicados (r/gmod); verificar qué aplica a las armas.
- **Dependencies**: engine s&box; contenido de `facepunch/sboxweapons` para los modelos.
- **s&box API generation**: API actual — el sandbox actual es scene/GameObject-based y es la base de la que salió la API moderna.
- **What it solves**: código de referencia de una WeaponBase completa y funcional (disparo, reload, viewmodel, deploy) mantenida por Facepunch; exactamente lo que hace falta para adaptar a "Último Barrio".
- **Exact files/components**: `WeaponBase` (componente base), armas concretas (`w_usp`, `w_mp5`, shotgun ⚠️ lista exacta no verificada), archivos de modelos `models/weapons/sbox_pistol_usp/v_usp.vmat_c` y `w_usp.vmat_c`.
- **Verdict**: ADAPT — es el código oficial de referencia; hay que extraer/adaptar la WeaponBase al proyecto (no usar el gamemode entero).
- **Confidence**: ALTA para existencia y contenido (sbox.game oficial + confirmación comunitaria); MEDIA para la URL exacta del repo y licencia.

---

## 4. timmybo5/simple-weapon-base (SWB)

- **Name**: Simple Weapon Base (S&Box) — comunidad de timmybo5
- **URL/package**: https://github.com/timmybo5/simple-weapon-base · demo: https://sbox.game/swb y https://sbox.game/swb/demo · esqueleto (solo lo necesario): https://github.com/timmybo5/swb-skeleton
- **Exact revision**: ⚠️ no verificable por búsqueda (revisar commits/tags en GitHub).
- **Last update**: ⚠️ activo — la demo de sbox.game "SWB" muestra horas de juego recientes y vídeos de hace ~2 meses; la descripción fue actualizada ("community managed").
- **License code**: ⚠️ no verificada en búsqueda; el README indica "licensed weapon models are included in the base" (los modelos tienen su propia licencia — verificar cada modelo).
- **License assets**: ⚠️ los modelos incluidos están licenciados (licencia de los modelos distinta del código); verificar en el repo.
- **Dependencies**: sin dependencias externas conocidas; es drop-in. (Hay una versión "skeleton" para arrastrar al gamemode.)
- **s&box API generation**: API actual — base de componentes moderna, configurable, con sistema de personalización de armas en juego (menú con Q).
- **What it solves**: base de armas "simple de usar pero muy configurable": automáticas, semiauto, shotgun, melee, burst, proyectiles, zoom/ADS, recoil y más; incluye modelos de armas y viewmodels; soporta reemplazar viewmodels fácilmente (usado por TTT de s&box).
- **Exact files/components**: `WeaponBase` y clases de armas por tipo (semi/full auto, shotgun, melee, burst, projectile); viewmodels reemplazables; menú de personalización en juego. ⚠️ nombres exactos de archivos por verificar en el repo.
- **Verdict**: ADOPT (con adaptación) — la base de armas comunitaria más mantenida y completa; verificar licencias de código y de los modelos incluidos antes de integrar.
- **Confidence**: ALTA para existencia/estado (GitHub + sbox.game + menciones en TTT); MEDIA para licencias.

---

## 5. Ryhon0/RWB (Ryhon's Weapon Base)

- **Name**: RWB — Ryhon's Weapon Base for S&Box
- **URL/package**: https://github.com/Ryhon0/RWB
- **Exact revision**: ⚠️ no verificable por búsqueda (ver commits en GitHub).
- **Last update**: ⚠️ no verificable; listada en awesome-sbox (mantenida por Ryhon0, mantenedor también de awesome-sbox).
- **License code**: ⚠️ no verificada en búsqueda.
- **License assets**: n/a (base de código; sin modelos propios ⚠️).
- **Dependencies**: ninguna conocida (se copia a `code/Weapons/Base/`).
- **s&box API generation**: API actual (código para el sistema de componentes moderno ⚠️ confirmar en repo que no usa APIs legacy).
- **What it solves**: base de armas ligera con soporte para semiauto, full-auto, shotguns, melee, burst fire y proyectiles "al mismo tiempo" — útil como segunda fuente de referencia si SWB no encaja.
- **Exact files/components**: estructura `Weapons/Base/` con la clase base y armas de ejemplo ⚠️ nombres exactos por verificar.
- **Verdict**: ADAPT — alternativa ligera de referencia (pattern de implementación); sin assets propios.
- **Confidence**: MEDIA (fuente primaria GitHub + listada en awesome-sbox; detalles internos no verificados).

---

## 6. Ryhon0/awesome-sbox (catálogo)

- **Name**: awesome-sbox — lista curada de proyectos open-source de S&Box
- **URL/package**: https://github.com/Ryhon0/awesome-sbox
- **Exact revision**: ⚠️ viva (no pinneada).
- **Last update**: ⚠️ mantenida (autor activo en el ecosistema).
- **License code**: n/a (es una lista).
- **License assets**: n/a.
- **Dependencies**: n/a.
- **s&box API generation**: n/a (índice).
- **What it solves**: descubre bases de armas y otros frameworks; incluye sección de "Facepunch Games" y weapon bases (SWB, RWB, etc.).
- **Exact files/components**: n/a.
- **Verdict**: PATTERN — usar como fuente de descubrimiento continua y para verificar qué bases están vivas.
- **Confidence**: ALTA (GitHub, fuente curada).

---

## 7. Facepunch/sbox-public (engine open source, contexto)

- **Name**: s&box engine (open source)
- **URL/package**: https://github.com/Facepunch/sbox-public · news: https://sbox.game/news/update-25-11-26 · https://sbox.game/news/update-26-04-08
- **Exact revision**: ⚠️ HEAD del repo (rama master); sin tag verificado.
- **Last update**: 2025-11-26 (open-sourced, MIT) y sigue activo (update-26-04-08, "Sandbox: Open Source").
- **License code**: MIT (confirmado por el repo y comunicados oficiales).
- **License assets**: los assets del engine quedan sujetos a los términos de Facepunch (el código es MIT, no necesariamente el contenido) ⚠️.
- **Dependencies**: n/a (es el engine).
- **s&box API generation**: API actual por definición (escenas GameObject/Component).
- **What it solves**: contexto: al ser open source se puede consultar la implementación real del engine (scene system, animación, audio) para depurar la integración de armas; no aporta armas en sí.
- **Exact files/components**: n/a para armas; útil para API de bajo nivel (Scene, Component, Sound, etc.).
- **Verdict**: PATTERN — referencia de implementación del engine, no un candidato de armas.
- **Confidence**: ALTA (fuente oficial).

---

## 8. thieves/rpdowntown3t (contexto de paquete base del proyecto)

- **Name**: Rp Downtown (3T) — remake del mapa Downtown de Three Thieves
- **URL/package**: https://sbox.game/thieves/rpdowntown3t · compact: https://sbox.game/thieves/rpdowntown3tcompact · performance: https://sbox.game/thieves/rpdowntown3tse
- **Exact revision**: versiones vistas: 90682, 93645, 93735 ⚠️ (la última publicada no confirmada).
- **Last update**: ⚠️ Three Thieves sigue publicando updates (dev build en sbox.game/thieves/downtown3tdev).
- **License code**: n/a (paquete de mapa/juego).
- **License assets**: contenido de Three Thieves, distribuido vía sbox.game ⚠️ términos del paquete.
- **Dependencies**: engine s&box.
- **s&box API generation**: n/a (mapa `.vpk`), pero es el ejemplo de paquete base que el repo ya usa (formato `organizacion/paquete` de sbox.game).
- **What it solves**: es el paquete de referencia del repo para el mundo del juego (mapa urbano), no armas — sirve para validar el formato de dependencias y el flujo de publicación.
- **Exact files/components**: `maps/downtown/rp_downtown_3t_v1.vpk` (11.6 MB), materiales, skyboxes.
- **Verdict**: DISCARD (para el stack de armas) — no aporta armas; útil solo como referencia de formato de paquete.
- **Confidence**: ALTA (sbox.game oficial) en cuanto a existencia/formato.

---

## 9. omniparadigm/weapons (candidato semilla)

- **Name**: omniparadigm/weapons (candidato semilla del stack)
- **URL/package**: ⚠️ NO ENCONTRADO. Dos búsquedas independientes ("omniparadigm weapons sbox github" y "omniparadigm sbox") devolvieron 0 resultados. No hay evidencia pública de un repo `omniparadigm/weapons`.
- **Exact revision**: — (no localizable).
- **Last update**: — (no localizable).
- **License code**: — (no localizable).
- **License assets**: — (no localizable).
- **Dependencies**: — (no localizable).
- **s&box API generation**: — (no localizable).
- **What it solves**: desconocido; probablemente confundido con otra base (p.ej. "Omni" no es un autor conocido en el ecosistema de weapon bases que aparezca en awesome-sbox).
- **Exact files/components**: —.
- **Verdict**: DISCARD — no verificable; NO inventar datos. Si el equipo tiene un enlace interno, reintentar la verificación manualmente.
- **Confidence**: BAJA (0 resultados en 2 búsquedas).

---

## 10. TTTReborn/tttreborn (referencia de armas TTT-style)

- **Name**: TTT Reborn — Trouble in Terry's Town (s&box)
- **URL/package**: https://github.com/TTTReborn/tttreborn · juego: https://sbox.game/ (TTT para s&box)
- **Exact revision**: ⚠️ no verificable por búsqueda.
- **Last update**: ⚠️ proyecto vivo (spiritual successor de TTT para s&box).
- **License code**: ⚠️ no verificada en búsqueda (repo GitHub público).
- **License assets**: ⚠️ no verificada.
- **Dependencies**: s&box; usa/reemplaza viewmodels de Simple Weapon Base (devblog #6: "with simple weapon base implemented it's extremely easy to replace any view models").
- **s&box API generation**: API actual (proyecto s&box moderno).
- **What it solves**: referencia de diseño de armas para un modo social/deducción ambientado en ciudad (muy afín a "Último Barrio"): gestión de armas, roles y combate moderado.
- **Exact files/components**: ⚠️ componentes de armas por verificar en el repo (weapons por rol, sistema de compra, etc.).
- **Verdict**: ADAPT — referencia de diseño y de integración con SWB, no para copiar.
- **Confidence**: MEDIA (GitHub + devblog oficial de TTT Reborn).

---

## 11. Disponibilidad legal de modelos: USP, crowbar, knife, shotgun (worldmodel + viewmodel + sonidos)

### USP (pistola)
- **Oficial Facepunch**: `sbox_pistol_usp` con `v_usp` (viewmodel) y `w_usp` (worldmodel) — dentro de `facepunch/sboxweapons` / gamemode sandbox (verificado en sbox.game/facepunch/sandbox/version/24874: `models/weapons/sbox_pistol_usp/v_usp.vmat_c`, `w_usp.vmat_c`). **Veredicto: ASSETS, legal y gratis** dentro de s&box. ⚠️ Confirmar términos de uso en juegos standalone publicados.
- **Confidence**: ALTA para existencia; MEDIA para términos de redistribución.

### Shotgun
- **Oficial Facepunch**: el collection `sboxweapons` incluye shotgun (Gary añadió pistol, SMG y shotgun como armas default; la colección oficial las modela y configura) ⚠️ el modelo exacto no se listó en los resultados. **Veredicto: ASSETS**. Confianza MEDIA ⚠️.
- Alternativa genérica: sketchfab "Firearm Asset Pack (10 guns plus knife)" (⚠️ revisar licencia CC del autor).

### Crowbar (palanca)
- **No confirmado como asset oficial de Facepunch** en las búsquedas ⚠️ (el sandbox tiene melee/herramientas; la palanca clásica no se confirmó). 
- Alternativas gratuitas: "Free Crowbar 3D Model" de VOiD1 Gaming en itch.io (void1gaming.itch.io/free-3d-assets-collection) ⚠️ verificar licencia (habitualmente CC0/CC-BY); varios packs de melee low-poly en sketchfab. Para viewmodel habrá que generarlo o adaptarlo (el sistema FP de Facepunch da brazos + animgraph "punching" reutilizable).
- **Veredicto: ASSETS (con verificación de licencia)**. Confianza BAJA-MEDIA.

### Knife/cuchillo
- **No confirmado como asset oficial de Facepunch** ⚠️.
- Alternativas: "Weapons Pack – Melee Weapons" de PolyOne en sketchfab (32 melee low-poly, incluye cuchillos) ⚠️ licencia CC por verificar; packs en itch.io (tag gun/melee). El animgraph "punching" + brazos oficiales sirven como base de animación melee.
- **Veredicto: ASSETS (con verificación de licencia)**. Confianza BAJA-MEDIA.

### Sonidos y efectos
- **No confirmado** que el paquete oficial `sboxweapons` incluya sonidos (la página menciona "Magazines, Bullets and Modular Attachments available separately"; no sonidos) ⚠️.
- Alternativas: packs de gunshot SFX en itch.io (p.ej. 774 sonidos por ~$9.99, tag `gun` + `sound-effects`); sonidos gratuitos tipo epicstockmedia (⚠️ licencias por verificar); asset.party de s&box como host para subir los propios.
- **Veredicto: compra/CC-BY verificado + subir a asset.party**. Confianza MEDIA.

---

## Recomendaciones

### OPCIÓN PRINCIPAL: timmybo5/simple-weapon-base (SWB) + assets oficiales Facepunch
- **Base de código**: `timmybo5/simple-weapon-base` (o el esqueleto `swb-skeleton` para integrarlo limpio). Cubre semiauto/full-auto/shotgun/melee/burst/proyectiles, zoom/ADS, recoil y viewmodels reemplazables — exactamente el abanico de "Último Barrio" (pistola, escopeta, cuchillo/palanca melee).
- **Assets**: consumir `facepunch/sboxweapons` (USP, shotgun, viewmodels/worldmodels, attachments) + brazos FP y animgraph "punching" oficiales para melee.
- **Riesgo a gestionar**: verificar las licencias de los modelos incluidos en SWB y los términos de Facepunch para juegos standalone publicados; mantener SWB como submodule/dependencia fija (revisión pinneada) para evitar breakages de API.

### Alternativa A (mínima dependencia de terceros): ADAPTAR la WeaponBase del Sandbox de Facepunch
- Extraer `WeaponBase` + `w_usp`/shotgun del gamemode oficial (github.com/Facepunch/sandbox ⚠️ URL a confirmar) y adaptarla al proyecto. Cero dependencias comunitarias, código 100 % oficial (MIT presumible), pero requiere más trabajo de integración y de implementar melee/ADS por nuestra cuenta. Assets igualmente de `facepunch/sboxweapons`.

### Alternativa B (referencia ligera): Ryhon0/RWB
- Si SWB no encaja por licencias o tamaño, RWB ofrece una base mínima con los mismos tipos de fuego (incl. melee y proyectiles) como referencia de implementación. Sin assets propios; combinar con `facepunch/sboxweapons`.

### Alternativa C (ASSETS-only): stack oficial Facepunch + asset store
- **Solo assets**: `facepunch/sboxweapons` (USP/shotgun + attachments) + brazos FP/"punching" animgraph (doc oficial) + modelos melee de terceros con licencia CC verificada (crowbar: VOiD1 Gaming/itch.io; knife: PolyOne/sketchfab ⚠️ verificar licencias) + SFX de itch.io (gunshot packs) subidos a asset.party. El código de armas se escribe a medida (más trabajo, pero 100 % control de licencias y estética propia).
- Indicada si el equipo quiere evitar cualquier dependencia comunitaria o tiene requisitos de licencia estrictos para publicación.

### Notas finales
- Verificar siempre en el repo real: revisiones exactas (commit/tag), licencias (LICENSE file) y que la base compile contra la build actual del engine (la API de s&box cambia rápido; fijar versiones del engine y de los paquetes).
- El sistema oficial de viewmodels (brazos FP + animgraph "punching" + triggers de ADS/recoil) es la referencia canónica de cómo se animan las armas en la API actual; cualquier base (SWB, RWB o propia) debería respetarlo.
- `omniparadigm/weapons` NO existe verificable públicamente → descartado del stack hasta tener el enlace real.
