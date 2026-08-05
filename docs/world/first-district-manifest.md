# Manifiesto del primer distrito — thieves.rpdowntown3t

Inventario real de `Assets/scenes/ultimo_barrio_alpha.scene`, consultado en vivo vía
MCP (`find_game_objects`) el 2026-08-05, no inventado. Motor 26.07.22, escena activa
en el momento de la consulta.

**No se ha guardado ni movido nada en la escena para producir este documento** —
es una lectura, no una mutación. Cualquier colocación nueva se hace en editor y se
valida visualmente, según pide el bloque 7 del encargo.

## Lo que ya está colocado

### Sector_Residential — 6 apartamentos

| Apartamento | Path | Componentes | Stash |
|---|---|---|---|
| apartment-a01 | `UltimoBarrioWorldOverlay/Sector_Residential/apartment-a01` | `ApartmentComponent` | Sí — `Stash Anchor` (`StashComponent`+`InventoryComponent`) |
| apartment-a02 | mismo patrón | `ApartmentComponent` | Sí |
| apartment-a03 | mismo patrón | `ApartmentComponent` | Sí |
| apartment-a04 | mismo patrón | `ApartmentComponent` | Sí |
| apartment-a05 | mismo patrón | `ApartmentComponent` | Sí |
| apartment-a06 | mismo patrón | `ApartmentComponent` | Sí |

Los 6 apartamentos pasan la validación de `ApartmentRegistry.Build` en runtime
(confirmado por log de sesión: `ApartmentRegistry.Build: Found 6 components, 6 are
valid` / `Found 6 groups`) — `DoorReference`/`StashReference`/`SpawnReference` están
resueltos para los 6, no son huecos.

**A01/A02** (los dos primeros del recorrido, per encargo) están entre estos 6 —
sin distinción especial en el registro; el recorrido de primera vivienda puede
usar cualquiera de los 6 marcados como `Unclaimed`.

### Sector_Scrapyard — nodos de recursos

**54 `ResourceNode` + `WorldItemPickup`** ya colocados (nombrados "Chatarra N" /
"Chatarra 18 (N)" — el patrón de nombres sugiere clonado en editor desde una base
"Chatarra 18"). Cada uno resuelve un `ItemId` real vía `ResourceNode` (no cubos —
llevan `ModelRenderer` propio). Esto ya cubre el paso 2 del recorrido ("recoger
materiales con E") en términos de disponibilidad de puntos de recolección.

### Sector_Plaza — comercio

**1 `Trader`** ("Kiosko Comerciante") con `BoxCollider`+`ModelRenderer`.

### MapInstance — spawns del mapa base

**154 `info_player_start`** heredados de `thieves.rpdowntown3t` — spawns del propio
mapa Source 2, no específicos de Último Barrio. No se listan individualmente aquí
(son parte del mapa, no del contenido del proyecto); `ApartmentComponent.SpawnReference`
es la fuente de verdad para respawn de propietario, no estos puntos.

## Lo que falta colocar (existe como prefab, no como instancia en la escena)

Confirmado por consulta directa — cero resultados en la escena activa para estos
componentes, aunque los prefabs sí existen en `Assets/prefabs/`:

| Elemento pedido | Prefab disponible | Estado en escena |
|---|---|---|
| CraftingStation | `Assets/prefabs/world/ub_crafting_station.prefab` | **0 instancias** — no hay estación de crafteo colocada, así que el paso "fabricar apartment_door_kit" no tiene dónde ejecutarse todavía |
| BarricadeAnchor | `Assets/prefabs/world/pf_barricade_anchor.prefab` | **0 instancias** |
| MedicalPoint | `Assets/prefabs/world/pf_medical_point.prefab` | **0 instancias** |
| DangerZone | `Assets/prefabs/world/pf_danger_zone.prefab` | **0 instancias** |
| SafeZone | sin prefab dedicado localizado | **0 instancias** — no se encontró candidato en `Assets/prefabs/world/` |
| WindowDefense | `Assets/prefabs/world/pf_window_defense.prefab` | **0 instancias** |
| VehicleSpawn | — | No aplica todavía (vehículos fuera del recorrido principal, per encargo) |

**Esto es el bloqueador real, no documentado hasta ahora, para completar
físicamente el recorrido de primera vivienda**: sin una `CraftingStation` colocada
y alcanzable a pie desde el Sector_Scrapyard/Sector_Residential, el jugador no
tiene dónde fabricar `apartment_door_kit` (bloque E, ya implementado en código —
ver commit `a2d49eb`) aunque tenga los materiales.

## Siguiente acción concreta

1. Colocar al menos **una `CraftingStation`** (`ub_crafting_station.prefab`) entre
   `Sector_Scrapyard` y `Sector_Residential`, a distancia razonable de ambos —
   validar en editor con `camera_screenshot`/`editor_camera_screenshot`, no solo
   por coordenadas.
2. Colocar **una `BarricadeAnchor`** por ventana/puerta de al menos apartment-a01
   y apartment-a02 (los dos que cita el encargo), con `ApartmentId`/`AnchorId`
   coherentes con esos apartamentos.
3. `MedicalPoint`/`DangerZone`/`SafeZone`/`WindowDefense` quedan catalogados como
   pendientes — no bloquean el recorrido mínimo de vivienda, se colocan cuando se
   aborde el ciclo día/noche y raids en profundidad.
4. Cualquier colocación debe pasar por editor + captura visual antes de darse por
   válida, según [[sbox-runtime-proof]] — coordenadas puestas a mano en este
   documento sin pasar por el editor no cuentan como colocación real.
