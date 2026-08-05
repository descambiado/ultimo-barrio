---
name: ultimo-barrio-architect
description: Use whenever making a gameplay, content, or scope decision for Último Barrio - deciding what to build next, whether a feature fits the vision, how a system should behave, or what the first playable district must contain. This is the canonical design reference: map, fantasy, the day/night loop, apartment persistence, combat, NPCs, economy, missions, vehicles, multiplayer, and progression. Consult before proposing new systems, before reordering the roadmap, and before implementing anything that touches apartment ownership, crafting, combat, AI, or the economy.
---

# ultimo-barrio-architect

(Formerly `ultimo-barrio-gameplay`, renamed and expanded per the "toma el control"
full-stack integration pass to cover combat, NPCs, vehicles, multiplayer, and progression
alongside the original housing/crafting scope.)

Último Barrio es un survival urbano, de construcción y progresión persistente. Esta skill
es la fuente canónica de la visión de diseño — consúltala antes de decidir *qué* construir,
no solo *cómo*.

## Mapa

```
thieves.rpdowntown3t
```

Toda colocación de contenido (anclas de puerta, estaciones de crafteo, traders, puntos
médicos, zonas de peligro, spawns futuros de vehículo) se ancla a este mapa concreto — ver
`docs/world/first-district-manifest.md` para el inventario real de lo ya colocado, no
coordenadas inventadas.

## Fantasía

```
DÍA
saquear → comerciar → fabricar → conseguir vivienda → fortificar →
completar encargos → prepararse

NOCHE
cerrar el barrio → defender vivienda → combatir saqueadores/enemigos →
reparar → sobrevivir → conseguir loot y progreso
```

La ambientación es una ciudad mediterránea ficticia. No usar nacionalidades, etnias o
religiones reales como facciones enemigas (regla ya establecida en el CLAUDE.md raíz del
proyecto).

## Requisitos de juego

- Debe funcionar solo, multijugador, y con pocos jugadores — ninguna mecánica puede asumir
  que hay otro jugador conectado, ni romperse cuando solo hay uno.
- Con persistencia real, no efímera — el progreso sobrevive Stop/Play y reinicios.
- Cada jugador tiene estadísticas, vivienda, stash, mejoras y progresión **separadas**.
- La vivienda evoluciona físicamente y persiste — el nivel de mejora se ve en el mundo.

## Recorrido inicial obligatorio (acceptance path)

```
1. Aparecer sin vivienda
2. → recoger materiales con E
3. → verlos en inventario
4. → fabricar kit de puerta
5. → encontrar vivienda libre
6. → instalar la puerta
7. → reclamar la vivienda
8. → desbloquear stash
9. → fabricar barricada
10. → colocarla en ventana o puerta
11. → reparar y mejorar la vivienda
12. → guardar
13. → reiniciar
14. → recuperar propiedad, inventario y mejoras
```

Cada paso debe ser una acción física real del jugador — ver [[ultimo-barrio-runtime-proof]].

## Esquema de persistencia por vivienda

```
ApartmentId, OwnerPersistentId, CoOwners, ClaimedAt
DoorType, DoorLevel, DoorHealth, LockLevel
Windows, Barricades, BarricadeHealth
Furniture, CraftingStations, GeneratorLevel, PowerStored, AlarmLevel, StorageLevel
StashContents, DefenseScore, ComfortScore, UpgradeLevel, RepairState
LastRaidAt, RaidsSurvived, DamageTaken, ResourcesSpent, SpawnEnabled
```

El proyecto actual ya persiste un subconjunto real de esto (`ApartmentSaveData`,
`FortificationSaveData`: `UpgradeLevel`, `DoorHealth`, `DoorMaxHealth`, `Barricades`) —
ampliar el esquema es incremental sobre lo existente, no un rediseño desde cero. Cualquier
cambio debe verificar primero cómo migrar los saves ya en disco (precedente: commit
`b4281ac`, "migrate legacy apartment save without data loss" — migración incremental que
lee el formato viejo y reescribe al nuevo sin perder datos).

Cada estructura colocada (barricada, mejora): `StructureId`, `PrefabId`, `ApartmentId`,
`OwnerPersistentId`, `AnchorId`, `LocalPosition`, `LocalRotation`, `Level`, `Health`,
`MaxHealth`, `State`, `CreatedAt`, `UpdatedAt`.

## Niveles de mejora de vivienda

```
0 abandonada
1 puerta básica
2 ventanas tapiadas
3 puerta reforzada
4 almacenamiento y alarma
5 vivienda fortificada
```

Cada nivel debe verse físicamente en el mundo — un `UpgradeLevel` que solo cambia un número
sin representación visual no cuenta como implementado.

## Combate

Base: `Sandbox.BaseCombatWeapon` como patrón de referencia (el `BaseCombatWeapon` local ya
sigue esta forma — fire/reload/ammo pool/RPC — porque fue extraído de él en una sesión
anterior; no se revierte a la base nativa, se conserva el local). Primer bloque de armas:
puños, crowbar, cuchillo, USP, escopeta. Cada una necesita modelo real, viewmodel,
worldmodel, animación, input, daño host-authoritative, munición, recarga, drop, pickup con
E, hotbar, persistencia, audio — no una menos, no veinte más hasta que estas cinco
funcionen. Los prefabs actuales (`ub_usp.prefab`, `v_usp.prefab`, `ub_melee.prefab`,
`v_melee.prefab`, ~800-980 bytes cada uno) son placeholders — el trabajo de este bloque es
sustituir el asset, no reescribir la lógica ya construida.

## NPC y enemigos

Arquitectura ya existente y reutilizable: `AIBase` (NavMeshAgent + PerceptionComponent +
combate melee + loot al morir), `SaqueadorBrain` (looter con FSM completa), `VecinoBrain`
(civil pasivo que huye). Arquetipos que faltan, extendiendo `AIBase` con el mismo patrón:

```
Saqueador: rápido, poca vida, ataca jugador y ventanas — YA EXISTE (SaqueadorBrain)
Bruto: lento, mucha vida, prioriza puertas y barricadas — falta
Merodeador: detecta ruido, rodea, busca entradas abiertas — falta
```

`FeatureFlags.EnableAI`/`EnableRaids` están en `false` por defecto — el spawn y los raids
están apagados, no rotos. Empezar con 2 saqueadores + 1 bruto, escalar después — no
cincuenta enemigos desde el principio.

## Economía y misiones

Transacciones atómicas para comprar, vender, fabricar, reparar, mejorar — el
`CraftingService` ya implementa este patrón (validar → consumir con rollback parcial →
crear resultado → persistir → rollback total si algo falla); cualquier transacción nueva
sigue la misma forma. `MissionSystem.cs` ya modela `ObjectiveType.SurviveNight` — la
misión "Sobrevive a la primera noche" tiene dónde encajar sin diseño nuevo. Falta el panel
`MissionJournal` en la UI (el toggle en `UIShell` existe, el panel no).

## Vehículos

Catalogar, no integrar en el recorrido principal todavía. Candidato verificado:
`matekdev/sbox-arcade-car-physics` (MIT, ya multijugador-listo con el mismo modelo de
autoridad dueño-controla que el resto del proyecto). Primera integración futura: furgoneta
de saqueo con entrar/salir, maletero, combustible, daño, spawn persistente — como spike
aislado, no bloqueando ningún bloque anterior.

## Multijugador

Todo sistema nuevo es host-authoritative por defecto, siguiendo el patrón ya establecido en
`InventoryComponent`/`ApartmentClaimService`/`CraftingService`: el cliente propone, el host
valida y decide, con RPC (`[Rpc.Host]`/`[Rpc.Owner]`/`[Rpc.Broadcast]`) y rollback explícito
si un paso falla a mitad de una operación de varios pasos.

## Progresión

Por jugador, no compartida salvo donde el diseño lo pida explícitamente (robos, PvP,
raids). `OwnerPersistentId` es la clave de todo — nunca la sesión de red.
