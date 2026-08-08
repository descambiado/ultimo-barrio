# PENDING — Sonidos sin fuente legal verificada (NO inventar rutas)

Estos eventos NO tienen aún un `.sound` en el banco. **Prohibido inventar paths** para ellos;
cuando el editor/Cloud Browser esté disponible (o se descargue y verifique una fuente legal), se
crean y se mueven a la tabla principal del README.

## 1. Knife swing — RESUELTO (worker C, 2026-08-08)

`knife_swing.sound` creado en `Assets/sounds/content/weapons/`, usando los melee swings oficiales
MIT importados (`swing_01/02.wav`, compartidos con el crowbar — mismo fit acústico de balanceo).
- **Estado:** RESUELTO como swing melee verificado; pendiente SOLO de OK de diseño si se quiere un
  "stab" distintivo del paquete del cuchillo (`facepunch.w_trenchknife` / `facepunch.v_m9bayonet`
  siguen siendo paquetes SOLO modelos: verificado que no traen sonidos).

## 2. Knife impact — RESUELTO por mapping (worker F/C)

Impacto en carne → `sounds/content/impacts/melee_impact_flesh.sound` (engine core verificado, #11 del
README) y cableado en `WeaponContentDefinition.MeleeHitSound` del cuchillo (worker C).

## 3. Enemy alert / enemy attack — RESUELTO con candidato documentado (worker C, 2026-08-08)

`enemy_alert.sound` + `enemy_attack.sound` creados en `Assets/sounds/content/enemies/` usando los
sonidos del NPC "darren" del repo oficial `Facepunch/sandbox` (MIT, importados byte-exactos:
`scared_01..06.wav` → `sounds/content/enemies/scared_*.vsnd`).
- **OJO:** es voz robótica/dispositivo, NO humana. `pending.md` (worker F) ya lo documentaba como
  "alternativa oficial MIT verificada solo si el diseño acepta voz robótica". Se ha importado porque
  era la fuente verificada disponible; **queda pendiente el OK de diseño** para saqueadores humanos.
- Si el diseño lo rechaza: candidatos CC0 externos siguen documentados (OGA "Sound Effects Pack"
  CC0, Freesound CC0 filtrado por sonido).

## 4. Night warning (aviso nocturno / sirena) — SIGUE PENDING (verificado 2026-08-08)

- **Verificación nueva (worker C):** NO existe sirena/alarma en (a) el core del engine
  (`core/sounds/` completo: 0 hits para siren|alarm|klaxon|warning en audio), (b) los paquetes
  montados (0 vsnd en paquetes facepunch), (c) el repo oficial `Facepunch/sandbox` (0 hits).
- Único candidato del core: `sounds/ambience/electric-buzz-loop.vsnd` / `electric-buzz-2-loop.vsnd`
  (zumbido eléctrico en loop). NO se ha usado: es un loop de ambience, no una sirena one-shot, y
  encajaría mal como "aviso nocturno" sin OK de diseño.
- Candidatos legales documentados (research previo, `docs/research/workers/sound.md` §7):
  - Pixabay SFX (Pixabay Content License, sin atribución): https://pixabay.com/sound-effects/search/siren/
  - Mixkit Free Sound Effects (royalty-free): https://mixkit.co/free-sound-effects/siren/
  - Freesound tag "siren" filtro CC0 (por sonido).
  - Regla: NO Pixabay/Mixkit "random" si aparece una fuente oficial primero; re-verificar al integrar.

## 5. USP dry fire — resuelto (worker F)

`usp_dry.sound` creado importando `dry_fire.wav` (MIT oficial). Queda como registro del hueco cerrado.

## Regla de cierre

Un ítem PENDING solo se mueve a la tabla principal cuando: (a) path verificado en Cloud Browser/editor,
o (b) archivo descargado de fuente con licencia verificada (CC0/CC-BY/MIT) + ficha de licencia guardada.
Nada de esto entra en build pública sin pasar por `Assets/asset-registry.yml`.
