# Investigación de Audio — Último Barrio (s&box)

- **Fecha**: 2026-08-07
- **Worker**: Investigador audio (subagente)
- **Contexto**: Rama base con API actual de s&box (GameObject, componentes de sonido, prefabs JSON). Modo de juego urbano (barrio) que necesita: disparos, recarga, pasos, enemigos, impactos, construcción, sirenas, ambiente.
- **Método**: 16 búsquedas web independientes (s&box sound system / SoundEvent / gunshot / footsteps / freesound CC0 / Sonniss / OpenGameArt / Kenney / itch.io / asset.party / sirenas CC0, etc.). Licencias verificadas por snippet/fuente oficial; lo no verificable se marca ⚠️ y **no se inventa**.

---

## 0. Contexto técnico: cómo se integra audio en s&box (PATTERN base)

- **SoundEvent (.sound)**: "Most sounds in s&box are defined as SoundEvent assets (.sound files). A SoundEvent bundles one or more audio files together with settings like volume range, pitch range, 3D distance, occlusion, and a selection mode." — https://sbox.game/dev/doc/sound/playing-sounds
- **API de código**: `Sound.Play( "sounds/explosion.sound", WorldPosition )`; devuelve `SoundHandle` para control (parar, pitch, etc.); `Sound.Precache` evita stutters; flag **UI** en el SoundEvent = sonido 2D plano (menús/HUD). Referencia API: https://sbox.game/api/Sandbox.Sound
- **Ambiente**: `Sandbox.Soundscape` — "used for environmental ambiance of a map by playing a set of random sounds at given intervals" (https://sbox.game/api/Sandbox.Soundscape).
- **Formatos**: wav/mp3/ogg editables como assets; el editor compila a `.vsnd_c` (s&box también usa internamente `.vsnd_c`). Formatos exactos soportados → verificar en el editor ⚠️.
- **Distribución**: los assets de s&box se publican/entregan vía **asset.party** (https://sbox.game/news/asset-system). Los assets oficiales de Facepunch ("s&box assets", https://sbox.game/facepunch/sboxassets) son "free-to-use across any of your s&box projects" — pero son **modelos/materiales**, no se confirma audio ⚠️.
- **Actualización relevante**: update 26.06.03 (https://sbox.game/news/update-26-06-03) — overhaul de propagación de sonido (simulación física). Conviene testear volúmenes/oclusión con esta build.

---

## 1. DISPAROS (armas de fuego)

### Candidato: The Free Firearm Sound Library
- **Name**: The Free Firearm Sound Library
- **URL/package**: https://opengameart.org/content/the-free-firearm-sound-library
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0 1.0 Universal
- **License assets**: CC0 — sin atribución, uso comercial libre, redistribuible
- **Dependencies**: ninguna
- **s&box API generation**: n/a (assets sueltos) → se importan como wav/mp3/ogg y se envuelven en SoundEvent (.sound)
- **What it solves**: Disparos (librería completa de armas de fuego; incluye vídeos instructivos)
- **Exact files/components**: ficheros de audio de armas de fuego (formato por confirmar al descargar ⚠️); se convierten a `.sound` por arma
- **Verdict**: **ADOPT** — CC0 puro, sin atribución, cubre la categoría principal del modo
- **Confidence**: **ALTA** (licencia CC0 verificada en la ficha de OGA)

### Candidato: CC0 Sound Effects (colección OGA)
- **Name**: CC0 Sound Effects (14 SFX)
- **URL/package**: https://opengameart.org/content/cc0-sound-effects
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0 1.0 Universal
- **License assets**: CC0, sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent por efecto
- **What it solves**: Disparos (firearm), pasos, "man hurt" (enemigos), beeps
- **Exact files/components**: 14 SFX (incluye "Free Firearm Sound Library" parcial, "Fantozzi's Footsteps", "Beep", "Man hurt")
- **Verdict**: **ASSETS** — descartable como fuente secundaria dentro de OGA
- **Confidence**: **ALTA** (licencia verificada)

### Candidato: Snake's Authentic Gun Sounds (+ SECOND pack)
- **Name**: Snake's Authentic Gun Sounds / Snake's SECOND Authentic Gun Sounds Pack
- **URL/package**: https://itch.io/game-assets/free/tag-gun/tag-sound-effects
- **Exact revision/version**: no verificado ⚠️
- **Last update**: no verificado ⚠️
- **License code**: depende del pack (itch.io; comúnmente CC0/CC-BY) **NO VERIFICADA** ⚠️
- **License assets**: revisar la página de cada pack antes de usar
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Disparos auténticos (armas reales grabadas)
- **Exact files/components**: packs de disparos por arma
- **Verdict**: **ADAPT** — calidad alta y gratis, pero exige verificación de licencia por pack antes de commitear
- **Confidence**: **MEDIA** (existencia confirmada; licencia no verificada ⚠️)

### Candidato: Freesound — tag firearm/gunshot (CC0)
- **Name**: Freesound (búsqueda por tag con filtro CC0)
- **URL/package**: https://freesound.org/browse/tags/firearm/ (y búsquedas "gunshot" con filtro CC0)
- **Exact revision/version**: por sonido (IDs individuales, p.ej. Glock17_Magazine.1 de Stoniac)
- **Last update**: continuo
- **License code**: **por sonido** — CC0, CC-BY, CC-BY-NC, Sampling+… filtrar por CC0/CC-BY
- **License assets**: CC0 = sin atribución; CC-BY = atribución en créditos
- **Dependencies**: cuenta gratuita para descargar (o API pública)
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Disparos, impactos, todo — banco colaborativo enorme
- **Exact files/components**: wav/flac por sonido; crear `sounds/weapons/*.sound`
- **Verdict**: **ASSETS** — fuente de relleno para huecos concretos, siempre con filtro de licencia
- **Confidence**: **ALTA** (mecánica de licencias por sonido confirmada con ejemplos CC0 reales)

### Candidato: Sonniss #GameAudioGDC (bundle anual gratuito)
- **Name**: Sonniss — GameAudioGDC / GDC Game Audio Bundle
- **URL/package**: https://sonniss.com/gameaudiogdc/ (edición anual: https://gdc.sonniss.com/)
- **Exact revision/version**: ediciones anuales (2024: 400GB+ según cobertura; tamaños varían por año) ⚠️
- **Last update**: anual (GDC)
- **License code**: **Licencia propietaria de uso libre (royalty-free)** — NO es CC0
- **License assets**: "licensed for media production (games, film, TV, interactive projects) only… Use for AI/ML training is strictly prohibited"; sin atribución, uso comercial ilimitado en proyectos (confirmado en reddit y página oficial de licencia: https://sonniss.com/gdc-bundle-license/)
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Disparos, recarga, impactos, explosiones, ambiente urbano, sirenas/alarmas, humanoides — cubre TODAS las categorías
- **Exact files/components**: miles de wav organizados por categoría; crear `sounds/*/*.sound` por evento
- **Verdict**: **ADOPT** — la mejor cobertura "todo en uno" con licencia clara para juegos (guardar copia de la licencia en el repo docs/)
- **Confidence**: **ALTA** (licencia y términos verificados en sonniss.com + reddit)

---

## 2. RECARGA y manejo de armas

### Candidato: 113 Gun Weapon SFX Pack — FIRE / RELOAD (itch.io)
- **Name**: 113 Gun Weapon SFX Pack FIRE RELOAD
- **URL/package**: https://itch.io/game-assets/tag-reload/tag-sound-effects
- **Exact revision/version**: no verificado ⚠️
- **Last update**: no verificado ⚠️
- **License code**: por pack — NO VERIFICADA ⚠️ (revisar ficha)
- **License assets**: idem
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Recarga (y disparos)
- **Exact files/components**: pack de SFX de disparo/recarga
- **Verdict**: **ADAPT** — candidato fuerte si la licencia es CC0/CC-BY; requiere verificación manual
- **Confidence**: **MEDIA** (existencia confirmada; licencia ⚠️)

### Candidato: Gun Handling Sound Pack (itch.io)
- **Name**: Gun Handling Sound Pack
- **URL/package**: https://itch.io/game-assets/tag-reload/tag-sound-effects
- **Exact revision/version**: no verificado ⚠️
- **Last update**: no verificado ⚠️
- **License code**: NO VERIFICADA ⚠️
- **License assets**: idem
- **Dependencies**: ninguna
- **s&box API generation**: n/a
- **What it solves**: Recarga y manejo (cock, drop, empty mag)
- **Exact files/components**: SFX de manejo de armas
- **Verdict**: **ADAPT** — mismo criterio que el anterior (verificar licencia)
- **Confidence**: **MEDIA** ⚠️

### Candidato: Sonniss GDC (recargas)
- **Name**: Sonniss #GameAudioGDC (sección weapons/reload)
- **URL/package**: https://sonniss.com/gameaudiogdc/
- **Exact revision/version**: anual
- **Last update**: anual
- **License code**: royalty-free para juegos (ver §1)
- **License assets**: sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Recarga y mecánicas de arma (además de todo lo demás)
- **Exact files/components**: wav por categoría
- **Verdict**: **ADOPT** — licencia ya verificada; sirve como base si los packs de itch no pasan el filtro
- **Confidence**: **ALTA**

---

## 3. PASOS

### Candidato: Fantozzi's Footsteps (Grass/Sand & Stone)
- **Name**: Fantozzi's Footsteps
- **URL/package**: https://opengameart.org/content/cc0-sound-effects (colección) / búsqueda OGA "Fantozzi footsteps"
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0 1.0 Universal
- **License assets**: CC0, sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent + selección por superficie
- **What it solves**: Pasos (hierba/arena y piedra — el barrio urbano necesitará además asfalto/hormigón de otras fuentes)
- **Exact files/components**: footsteps por material
- **Verdict**: **ASSETS** — útil para superficies blandas
- **Confidence**: **ALTA** (licencia verificada)

### Candidato: 100 CC0 SFX #2 (OGA)
- **Name**: 100 CC0 SFX #2
- **URL/package**: https://opengameart.org/content/100-cc0-sfx-2
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0 1.0 Universal
- **License assets**: CC0, sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Pasos ("various footstep"), golpes, puertas, loops de ambiente (máquina, obra/construction site, highway/street), aire
- **Exact files/components**: 100 SFX (wav/ogg)
- **Verdict**: **ADOPT** — un solo pack CC0 que cubre pasos + construcción + ambiente urbano básico
- **Confidence**: **ALTA** (licencia y contenido verificados)

### Candidato: Source Footsteps and Phys Sounds (addon s&box)
- **Name**: Source Footsteps and Phys Sounds — by impostor
- **URL/package**: https://sbox.game/impostor/source_footsteps (versión vista: /version/9813)
- **Exact revision/version**: 9813 (version page vista en búsqueda)
- **Last update**: no verificado ⚠️
- **License code**: no especificada en addon ⚠️ — reutiliza sonidos internos de s&box (`.vsnd_c` de `sounds/physics/...`)
- **License assets**: ⚠️ no redistribuir los .vsnd_c como propios; usar solo como referencia
- **Dependencies**: superficies correctamente configuradas en materiales (s&box Surface)
- **s&box API generation**: s&box nativo (SoundEvent/Sound.Play sobre superficies)
- **What it solves**: Pasos y sonidos físicos por material — **referencia de integración** (cómo enganchar pasos a superficies en s&box)
- **Exact files/components**: código del addon + rutas `sounds/physics/surfaces/*.vsnd_c`; patrón: play por `Surface` del material tocado
- **Verdict**: **PATTERN** — no es fuente de audio nueva, es el patrón de implementación de pasos por superficie
- **Confidence**: **MEDIA** (existencia y mecanismo confirmados; licencia del addon ⚠️)

---

## 4. ENEMIGOS (voces, daño, gruñidos)

### Candidato: Sound Effects Pack (OGA, CC0)
- **Name**: Sound Effects Pack
- **URL/package**: https://opengameart.org/content/sound-effects-pack
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0 1.0 Universal
- **License assets**: CC0, sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Enemigos — sonidos humanos (respiración, risa, tos), impactos metálicos/cerámicos/mates, pasos
- **Exact files/components**: SFX agrupados por tipo
- **Verdict**: **ADOPT** — base CC0 para voces/dolor de enemigos
- **Confidence**: **ALTA** (licencia verificada)

### Candidato: OGA CC0 Sound Effects — "Man hurt"
- **Name**: CC0 Sound Effects (item "Man hurt")
- **URL/package**: https://opengameart.org/content/cc0-sound-effects
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0
- **License assets**: CC0
- **Dependencies**: ninguna
- **s&box API generation**: n/a
- **What it solves**: Daño recibido por enemigos humanos
- **Exact files/components**: "Man hurt" SFX
- **Verdict**: **ASSETS** — complemento del pack anterior
- **Confidence**: **ALTA**

### Candidato: Freesound CC0 (impacto/piercing humano)
- **Name**: Freesound — p.ej. "Anime Sound Effect - Piercing impact / Stabbing" (Breviceps)
- **URL/package**: https://freesound.org/people/Breviceps/sounds/464839/
- **Exact revision/version**: sound ID 464839
- **Last update**: no verificado ⚠️
- **License code**: CC0 (declarado por el autor)
- **License assets**: CC0
- **Dependencies**: cuenta Freesound
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Impactos en cuerpo (arma blanca/flecha) — útil para melee de enemigos
- **Exact files/components**: wav/flac
- **Verdict**: **ASSETS** — caso de uso de Freesound: buscar por tag + filtrar CC0
- **Confidence**: **MEDIA** (declaración del autor en la ficha; revisar ficha final al descargar)

---

## 5. IMPACTOS (balas, físicos, explosiones)

### Candidato: Kenney — Impact Sounds
- **Name**: Kenney Impact Sounds
- **URL/package**: https://kenney.nl/assets/impact-sounds (categoría Audio: https://kenney.nl/assets/category:Audio)
- **Exact revision/version**: no especificada ⚠️ (pack estable)
- **Last update**: no verificado ⚠️
- **License code**: **CC0** (licencia Kenney = CC0; los assets "are available for free" sin atribución)
- **License assets**: CC0, sin atribución, uso comercial
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Impactos genéricos (golpes, choques) — base para bullet impacts + físicos
- **Exact files/components**: pack de SFX de impacto (wav)
- **Verdict**: **ADOPT** — CC0 sólido para impactos de bala/objeto
- **Confidence**: **ALTA** (licencia Kenney CC0 ampliamente documentada)

### Candidato: Sonniss GDC (impacts/explosions)
- **Name**: Sonniss #GameAudioGDC — sección impacts/explosions
- **URL/package**: https://sonniss.com/gameaudiogdc/
- **Exact revision/version**: anual
- **Last update**: anual
- **License code**: royalty-free para juegos (ver §1)
- **License assets**: sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Impactos de bala por material (hormigón, metal, tierra) y explosiones
- **Exact files/components**: wav por categoría
- **Verdict**: **ADOPT** — la fuente más completa para impactos por superficie
- **Confidence**: **ALTA**

### Candidato: 100 CC0 SFX #2 (impacts)
- **Name**: 100 CC0 SFX #2 — items "hit"/"impact"
- **URL/package**: https://opengameart.org/content/100-cc0-sfx-2
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0
- **License assets**: CC0
- **Dependencies**: ninguna
- **s&box API generation**: n/a
- **What it solves**: Golpes genéricos
- **Exact files/components**: SFX sueltos
- **Verdict**: **ASSETS** — complemento
- **Confidence**: **ALTA**

---

## 6. CONSTRUCCIÓN

### Candidato: 100 CC0 SFX #2 (construction site loops)
- **Name**: 100 CC0 SFX #2 — loops "construction site"
- **URL/package**: https://opengameart.org/content/100-cc0-sfx-2
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0 1.0 Universal
- **License assets**: CC0, sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent (con flag de loop) o Soundscape
- **What it solves**: Construcción (obra en curso: loops de máquinas/herramientas) + ambiente de obra
- **Exact files/components**: loops ambientales de obra
- **Verdict**: **ADOPT** — CC0 y encaja con la temática barrio/obra
- **Confidence**: **ALTA**

### Candidato: Sonniss GDC (construction/wood/tools)
- **Name**: Sonniss #GameAudioGDC — sección construction/tools
- **URL/package**: https://sonniss.com/gameaudiogdc/
- **Exact revision/version**: anual
- **Last update**: anual
- **License code**: royalty-free para juegos
- **License assets**: sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Construcción (martillos, taladros, maderas, metales) — cubre mecánicas de build del modo
- **Exact files/components**: wav por categoría
- **Verdict**: **ADOPT** — la más completa para mecánica de construcción por partes
- **Confidence**: **ALTA**

### Candidato: Freesound CC0 (construction)
- **Name**: Freesound — tag/búsqueda "construction" con filtro CC0
- **URL/package**: https://freesound.org/search/?q=construction (filtro licencia CC0)
- **Exact revision/version**: por sonido
- **Last update**: continuo
- **License code**: por sonido (filtrar CC0/CC-BY)
- **License assets**: según sonido
- **Dependencies**: cuenta Freesound
- **s&box API generation**: n/a
- **What it solves**: Relleno de huecos específicos (clavos, sierras…)
- **Exact files/components**: wav/flac
- **Verdict**: **ASSETS** — relleno puntual
- **Confidence**: **MEDIA** (mecánica confirmada; contenido depende de la búsqueda)

---

## 7. SIRENAS (policía / ambulancia / bomberos)

### Candidato: Pixabay Sound Effects (sirenas/firetruck)
- **Name**: Pixabay Sound Effects — siren / firetruck
- **URL/package**: https://pixabay.com/sound-effects/search/firetruck/ (y /search/siren/)
- **Exact revision/version**: por clip
- **Last update**: continuo
- **License code**: **Pixabay Content License** (royalty-free, "No attribution required")
- **License assets**: sin atribución, uso comercial permitido (no redistribuir sin procesar)
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent (loop para sirenas activas)
- **What it solves**: Sirenas de policía/bomberos/ambulancia — clave para eventos de barrio
- **Exact files/components**: MP3 por clip; convertir y crear `sounds/vehicles/siren_*.sound`
- **Verdict**: **ADOPT** — gratis, sin atribución, cubre sirenas directamente
- **Confidence**: **ALTA** (términos verificados en la página)

### Candidato: Mixkit — Siren Sound Effects
- **Name**: Mixkit Free Sound Effects — Siren
- **URL/package**: https://mixkit.co/free-sound-effects/siren/
- **Exact revision/version**: 17 clips listados
- **Last update**: no verificado ⚠️
- **License code**: Mixkit License (free, royalty-free para uso comercial)
- **License assets**: sin atribución requerida
- **Dependencies**: ninguna
- **s&box API generation**: n/a → SoundEvent
- **What it solves**: Sirenas (17 variantes)
- **Exact files/components**: MP3/WAV por clip
- **Verdict**: **ADOPT** — alternativa directa a Pixabay
- **Confidence**: **MEDIA** (términos generales Mixkit conocidos; ficha concreta ⚠️)

### Candidato: Freesound CC0 (siren)
- **Name**: Freesound — búsqueda "siren" filtro CC0
- **URL/package**: https://freesound.org/search/?q=siren
- **Exact revision/version**: por sonido
- **Last update**: continuo
- **License code**: por sonido (filtrar CC0)
- **License assets**: según sonido
- **Dependencies**: cuenta Freesound
- **s&box API generation**: n/a
- **What it solves**: Sirenas específicas (EE.UU./Europa) si se necesita variedad
- **Exact files/components**: wav/flac
- **Verdict**: **ASSETS** — variedad
- **Confidence**: **MEDIA**

---

## 8. AMBIENTE (barrio / ciudad)

### Candidato: Sonniss GDC (urban ambience)
- **Name**: Sonniss #GameAudioGDC — sección ambience/city
- **URL/package**: https://sonniss.com/gameaudiogdc/
- **Exact revision/version**: anual
- **Last update**: anual
- **License code**: royalty-free para juegos (ver §1)
- **License assets**: sin atribución
- **Dependencies**: ninguna
- **s&box API generation**: n/a → **Soundscape** (Sandbox.Soundscape) para capas ambientales
- **What it solves**: Ambiente urbano: tráfico, ciudad día/noche, barrio residencial, viento
- **Exact files/components**: loops de ambiente en wav
- **Verdict**: **ADOPT** — la fuente principal de ambiente
- **Confidence**: **ALTA**

### Candidato: 100 CC0 SFX #2 (ambient loops)
- **Name**: 100 CC0 SFX #2 — loops (ambient, machine, highway/street, flowing…)
- **URL/package**: https://opengameart.org/content/100-cc0-sfx-2
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: CC0
- **License assets**: CC0
- **Dependencies**: ninguna
- **s&box API generation**: n/a → Soundscape
- **What it solves**: Ambiente urbano básico (calle/carretera) y máquinas
- **Exact files/components**: loops
- **Verdict**: **ADOPT** — base CC0 de ambiente
- **Confidence**: **ALTA**

### Candidato: s&box Soundscape (API) — PATTERN
- **Name**: Sandbox.Soundscape (API s&box)
- **URL/package**: https://sbox.game/api/Sandbox.Soundscape
- **Exact revision/version**: API actual de la rama base
- **Last update**: vigente en la build actual (2026)
- **License code**: n/a (API del motor)
- **License assets**: n/a
- **Dependencies**: motor s&box
- **s&box API generation**: s&box nativa (GameObject/componente Soundscape o API de clase)
- **What it solves**: Reproducción de ambiente por zonas del mapa (conjunto de sonidos aleatorios a intervalos + loops constantes)
- **Exact files/components**: componente/API Soundscape + SoundEvents de ambiente
- **Verdict**: **PATTERN** — la forma correcta de montar el ambiente del barrio
- **Confidence**: **ALTA** (documentación oficial de la API)

### Candidato: s&box assets oficiales de Facepunch (audio ⚠️)
- **Name**: s&box assets (Facepunch) / ready-to-use assets
- **URL/package**: https://sbox.game/facepunch/sboxassets y https://sbox.game/dev/doc/assets/ready-to-use-assets/first-person-weapons
- **Exact revision/version**: no especificada ⚠️
- **Last update**: no verificado ⚠️
- **License code**: "free-to-use across any of your s&box projects" (uso en proyectos s&box)
- **License assets**: incluye modelos/materiales y assets de armas primera persona; **audio no confirmado** ⚠️
- **Dependencies**: proyecto s&box
- **s&box API generation**: s&box nativa
- **What it solves**: Si incluyen SoundEvents de armas, cubrirían disparos/recarga sin conversión ⚠️
- **Exact files/components**: paquete "s&box assets" en el Asset Browser (buscar carpeta `sounds/`)
- **Verdict**: **ASSETS/PATTERN** — revisar el paquete oficial en el editor antes de comprar/importar nada externo para armas
- **Confidence**: **MEDIA** (existencia confirmada; contenido de audio ⚠️ no verificado)

---

## Recomendaciones — opción PRINCIPAL por categoría

| Categoría | PRINCIPAL | Alternativas | Integración s&box |
|---|---|---|---|
| Disparos | **The Free Firearm Sound Library** (OGA, CC0) | Sonniss GDC; Snake's Authentic Gun Sounds (verificar licencia ⚠️); Freesound CC0 | `sounds/weapons/xxx.sound` + `Sound.Play` |
| Recarga | **Sonniss GDC** (licencia verificada) | 113 Gun Weapon SFX Pack (⚠️ licencia); Gun Handling (⚠️) | `sounds/weapons/reload_*.sound` |
| Pasos | **100 CC0 SFX #2** (OGA, CC0) + **PATTERN Source Footsteps** | Fantozzi's Footsteps; Sonniss | `Sound.Play` por `Surface` del material (ver addon impostor/source_footsteps) |
| Enemigos | **Sound Effects Pack** (OGA, CC0: humano/daño) | Freesound CC0 (Breviceps etc.); Sonniss | `sounds/enemies/*.sound` |
| Impactos | **Kenney Impact Sounds** (CC0) + **Sonniss** (por material) | 100 CC0 SFX #2 | `sounds/impacts/*.sound` (variantes por superficie) |
| Construcción | **Sonniss GDC** (tools/wood) + **100 CC0 SFX #2** (obra) | Freesound CC0 | loops con flag de loop o Soundscape |
| Sirenas | **Pixabay SFX** (sin atribución) | Mixkit; Freesound CC0 | `sounds/vehicles/siren_*.sound` (loop) |
| Ambiente | **Sonniss GDC** (urban) + **Soundscape API** | 100 CC0 SFX #2 (street/highway) | `Sandbox.Soundscape` por zona del mapa |

**Flujo recomendado (ASSETS → s&box)**:
1. Descargar Sonniss GDC + packs CC0 (OGA/Kenney/Pixabay) y guardar **copia de cada licencia** en `docs/research/workers/` o `docs/licenses/`.
2. Convertir a wav/ogg, colocarlos en `sounds/<categoria>/` del addon.
3. Crear un **SoundEvent (.sound)** por evento con variantes (selection mode random) y rangos de volumen/pitch; flag UI para HUD.
4. Reproducir con `Sound.Play( "sounds/weapons/pistol_shot.sound", pos )`, guardar `SoundHandle` para sirenas/loops; `Sound.Precache` en precarga.
5. Ambiente por zonas con `Sandbox.Soundscape`.
6. ⚠️ Verificar en el editor: formatos de audio soportados, y el contenido de audio del paquete oficial "s&box assets" (evita importar armas si Facepunch ya las trae).

**Riesgos/notas**:
- itch.io (Snake's, 113 Gun, Gun Handling) es gratis pero la licencia debe revisarse **por pack** antes de integrar ⚠️.
- Freesound: licencia **por sonido**; filtrar siempre CC0/CC-BY y guardar la ficha del sonido para atribución CC-BY.
- Sonniss: prohibido uso para entrenamiento de IA/ML; ok para el juego. No redistribuir los wav sueltos, solo dentro del addon.
- Pixabay: no redistribuir clips sin procesar fuera del juego.
- El addon impostor/source_footsteps no es fuente de audio redistribuible (usa .vsnd_c internos); usarlo como referencia de código.
