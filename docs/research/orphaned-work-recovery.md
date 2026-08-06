# Inspección de trabajo "huérfano" — hallazgo: no hay nada huérfano

Fecha: 2026-08-07.

## Corrección de un error propio de esta sesión

En un checkpoint anterior de `docs/production/autonomous-progress.md` afirmé
que `e0bb0db` (Bruto/Merodeador) y `49595a4` (panel de misiones) habían
quedado en una rama previa al pivote de arquitectura de propiedades y nunca
se habían reaplicado. Esa afirmación era **falsa**, basada en una lectura
incorrecta del `git reflog` sin verificar con `git merge-base`. Verificación
real:

```
git merge-base --is-ancestor e0bb0db HEAD   → true
git merge-base --is-ancestor 49595a4 HEAD   → true
```

Ambos commits son ancestros directos de `HEAD` (`dac23f3` en el momento de
escribir esto). No hay nada que "portar" ni "extraer" — el código ya está en
la rama de trabajo, con commits posteriores construyendo encima:

- `aa059e6 fix(missions): wire MissionJournal onto the player, verify panel renders`
- `5733a22 test(qa): add persistence/AI-flag QA tooling; document Fase 11 root cause`

## Estado real por archivo/unidad

| Commit | Feature | Archivos | Dependencia arquitectónica | Decisión | Destino | Estado de validación |
|---|---|---|---|---|---|---|
| `e0bb0db` | BrutoBrain (lento, tanque, prioriza estructuras) | `Code/UltimoBarrio/AI/BrutoBrain.cs` | Extiende `AIBase`/`PerceptionComponent`, sistema de IA sin tocar por el pivote de propiedades | **YA INTEGRADO** (no requiere acción) | ya en `HEAD` | COMPILA. `FeatureFlags.EnableAI=false` — sin spawn/comportamiento verificado en motor |
| `e0bb0db` | MerodeadorBrain (patrulla, reacciona a ruido, busca puertas abiertas) | `Code/UltimoBarrio/AI/MerodeadorBrain.cs` | Igual que arriba, además usa `ApartmentDoorPolicy` (sistema de apartamentos original, no afectado por el pivote) | **YA INTEGRADO** | ya en `HEAD` | COMPILA. Mismo bloqueo de flag que Bruto |
| `49595a4`+`aa059e6` | Panel de diario de misiones | `Code/UltimoBarrio/UI/MissionJournalPanel.razor(.cs/.scss)`, `PlayerHud.cs`, `ProjectSettings/Input.config` (tecla J) | Ninguna — `MissionSystem`/`MissionJournal` no forman parte del sistema de vivienda | **YA INTEGRADO Y VERIFICADO PARCIALMENTE** | ya en `HEAD` | `aa059e6` confirma que el panel renderiza (colocado en `player.prefab`) — sigue pendiente una pasada con tecla J real para confirmar interacción completa |

## Por qué mi lectura anterior fue errónea (para no repetirla)

Comparé `git log --all --oneline | grep <hash>` (que encuentra el commit en
*cualquier* rama alcanzable, incluidos los `checkpoint/*`) con un
`git reflog` leído en el orden equivocado (asumí que `HEAD@{N}` más alto
significa "más reciente"; es al revés — `HEAD@{0}` es el más reciente).
Nunca corrí `git merge-base --is-ancestor` en ese momento, que es la única
comprobación que responde la pregunta real ("¿esto está en mi historia
actual?") sin ambigüedad. Corregido aquí y en `autonomous-progress.md`.

## Otros commits de IA/raids/misiones/armas — auditoría rápida (corregida)

Búsqueda de commits adicionales de armas/raids/misiones no cubiertos arriba:

```
git log --oneline --all -- Code/UltimoBarrio/AI/ Code/UltimoBarrio/Raids/ Code/UltimoBarrio/Missions/ Code/UltimoBarrio/Combat/
```

**Segunda corrección en esta misma pasada**: mi primera versión de este
documento afirmaba "Fase 12 (armas) sigue genuinamente sin empezar, no hay
nada que recuperar ahí" — también incorrecto, y por el mismo motivo (no
verifiqué ancestro antes de afirmar). `git merge-base --is-ancestor` confirma
que `a3c01c5` (feat(combat): complete USP foundation), `98700c7` (feat(melee):
add fists and crowbar), `bdac61b` (feat(weapons): add first USP
implementation), `5bce5fe` (feat(ai): add hostile NPC foundation) y otros ya
son ancestros de `HEAD`. El código de armas **ya existe y ya está
integrado**: `Code/UltimoBarrio/Combat/USPPistol.cs`, `FistsWeapon.cs`,
`MeleeWeapon.cs`, `BaseCombatWeapon.cs`, `HeldItemController.cs`,
`ReloadMath.cs`, `WeaponNoise.cs`.

Lo que sí sigue genuinamente sin empezar es la capa de **assets reales**: los
prefabs `ub_usp.prefab`/`v_usp.prefab`/`ub_melee.prefab`/`v_melee.prefab`
pesan 797-1056 bytes (placeholders sin modelo/animación real, confirmado por
tamaño de archivo) — la lógica de disparo/recarga/daño/munición ya está
escrita y compila, pero no hay worldmodel/viewmodel/animación/sonido reales
todavía. La Fase 12 es un problema de **assets + verificación en motor**, no
de arquitectura o lógica desde cero.

`SaqueadorBrain`/`RaidManager`/`AIBase`/`PerceptionComponent` (la base sobre
la que Bruto/Merodeador se construyen) llevan más tiempo en la rama y ya
estaban documentados como maduros en pasadas anteriores.

**Lección para el resto de esta sesión**: antes de afirmar "esto no existe"
o "esto es huérfano", correr `git log --oneline --all -- <path>` +
`git merge-base --is-ancestor <hash> HEAD` primero. Ya ha fallado dos veces
en la misma pasada por saltarse ese paso.

## Próxima acción real

No hay trabajo de "recuperación" pendiente. Lo que sigue pendiente es
**verificación en motor** de código ya integrado:

1. `FeatureFlags.EnableAI`/`EnableRaids` → `true` vía `ub_qa_toggle_ai` (ya
   existe, commit `5733a22`).
2. Colocar 1 `SpawnZone` de prueba con `SaqueadorBrain`+`BrutoBrain`+
   `MerodeadorBrain`, confirmar patrulla/detección/persecución/ataque con
   capturas + `read_console`.
3. Pulsar J en una sesión de Play real (o `ub_qa_physical_interact`-equivalente
   si no hay tecla J libre para el harness) y confirmar que
   `MissionJournalPanel` responde a interacción, no solo a renderizado.

Esto se hace en el orden que pidió el usuario (después de vivienda/BuildVolume/
distrito/armas), no antes.
