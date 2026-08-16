# Handoff — vestuario runtime del Saqueador

- Rama: `fix-enemy-clothing-runtime`
- Commit: `c05d351 fix(enemies): apply looter clothing after model readiness`
- Código: `EnemyContentHost` enlaza `Dresser.BodyTarget` al `SkinnedModelRenderer`, fuerza `Manual`, resuelve tres prendas y reintenta durante una ventana acotada.
- Escena: la instancia `Saqueador Inicial` de `Assets/scenes/barrio_01.scene` necesitaba un componente `Sandbox.Dresser`; se añadió desde el editor y se guardó.
- Verificación runtime (s&box 26.08.05): `[Content.Enemy] ClothingApplied: ub_enemy_saqueador (3 prendas)`.
- Compilación MCP: `LastCompileSucceeded=true`, `LastCompileErrors=0`.
- Play Mode: iniciado y detenido sin errores nuevos de `EnemyContentHost`.

El árbol conserva otros cambios de combate/QA previos sin mezclar en este commit.
