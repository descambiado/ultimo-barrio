# PENDING — Sonidos sin fuente legal verificada (NO inventar rutas)

Estos eventos NO tienen aún un `.sound` en el banco. **Prohibido inventar paths** para ellos;
cuando el editor/Cloud Browser esté disponible (o se descargue y verifique una fuente legal), se
crean y se mueven a la tabla principal del README.

## 1. Knife swing (cuchillo — balanceo)

- **Estado:** PENDING (path de sonido sin verificar).
- **Contexto:** los paquetes `facepunch.w_trenchknife` / `facepunch.v_m9bayonet` están montados
  (verificados para modelos), y con casi total seguridad traen sonidos, pero **ningún código oficial
  público referencia sus rutas de sonido** (no hay prefab de cuchillo en `Facepunch/sandbox`;
  búsqueda en repos oficiales Facepunch sin hits).
- **Verificación necesaria:** Cloud Browser en el editor con los paquetes montados → buscar
  `weapons/*knife*` / `weapons/*bayonet*` en el asset browser → anotar el `.sound`/`.vsnd` real.
- **Interino sugerido (requiere OK de diseño):** usar `sounds/content/weapons/crowbar_swing.sound`
  (swing melee oficial Facepunch, verificado) mientras no exista el propio del cuchillo.

## 2. Knife impact (cuchillo — impacto)

- **Estado:** RESUELTO parcialmente por mapping: impacto en carne → `sounds/content/impacts/melee_impact_flesh.sound`
  (engine core verificado, #11 del README). Impacto en props/metal → usar el evento del engine
  `sounds/Impacts/Melee/impact-melee-metal.sound` (reproducción directa) o envolver
  `sounds/Impacts/Melee/BluntWeapon/metal-1..4.vsnd`.
- Si se quiere un "stab" distintivo del paquete del cuchillo → misma verificación que #1.

## 3. Enemy alert / enemy attack (voces de enemigo)

- **Estado:** PENDING. No hay contenido oficial de voz humana en los paquetes montados ni en el core.
- **Candidatos legales documentados (research previo, `docs/research/workers/sound.md` §4):**
  - OGA "Sound Effects Pack" (CC0): https://opengameart.org/content/sound-effects-pack — gruñidos/dolor humanos.
  - OGA "CC0 Sound Effects" ítem "Man hurt" (CC0): https://opengameart.org/content/cc0-sound-effects.
  - Freesound tag firearm/voice con filtro CC0 (licencia por sonido, guardar ficha).
  - **Alternativa oficial MIT verificada (solo si el diseño acepta voz robótica/dispositivo):**
    los sonidos del NPC "darren" del repo `Facepunch/sandbox` (MIT, `.wav` en el repo):
    `Assets/sounds/npc/darren/{follow,idle,scared,stay,stuck}/**` y eventos `scared.sound` (6 variantes).
    Importables como `dry_fire.wav` con atribución MIT. NO es voz humana → no sirve para saqueadores
    sin aprobación de diseño.
- **Interino técnico:** el impacto del ataque melee enemigo ya suena (melee_impact_flesh); solo falta la voz.

## 4. Night warning (aviso nocturno / sirena)

- **Estado:** PENDING. No hay sirena/alarma oficial en paquetes montados ni core.
- **Candidatos legales documentados (research previo, `sound.md` §7):**
  - Pixabay SFX (Pixabay Content License, sin atribución): https://pixabay.com/sound-effects/search/siren/
  - Mixkit Free Sound Effects (royalty-free): https://mixkit.co/free-sound-effects/siren/
  - Freesound tag "siren" filtro CC0 (por sonido).
  - Regla: NO Pixabay/Mixkit "random" si aparece una fuente oficial primero; re-verificar al integrar.

## 5. USP dry fire — resuelto

`usp_dry.sound` creado importando `dry_fire.wav` (MIT oficial). Antes de este hallazgo estaba PENDING;
queda documentado aquí como registro del hueco cerrado.

## Regla de cierre

Un ítem PENDING solo se mueve a la tabla principal cuando: (a) path verificado en Cloud Browser/editor,
o (b) archivo descargado de fuente con licencia verificada (CC0/CC-BY/MIT) + ficha de licencia guardada.
Nada de esto entra en build pública sin pasar por `Assets/asset-registry.yml`.
