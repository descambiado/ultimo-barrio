# Manifiesto — Vertical slice A01/A02 sobre `thieves.rpdowntown3t`

> STATICALLY VERIFIED — este documento describe la integración en el editor
> (EDITOR VERIFICATION REQUIRED). No se modificaron posiciones de la escena
> existente; todo lo que requiere editor se coloca con estos prefabs.

## Mapa canónico

- `.sbproj`: `MapList: ["thieves.rpdowntown3t"]`, `MapSelect: Locked`.
- La escena `Assets/scenes/ultimo_barrio_alpha.scene` ya monta el mapa vía
  `Sandbox.MapInstance` + el overlay `UltimoBarrioWorldOverlay`. No mover
  nada del overlay existente.

## Regla de oro

Para cada elemento del barrio: **colocar el prefab, no mover la geometría
del mapa**. Los componentes se auto-resuelven por ApartmentId/nombre.

## Prefabs entregados

| Prefab | Componentes | Uso |
|---|---|---|
| `prefabs/weapons/ub_usp.prefab` | USPPistol, BoxCollider, ModelRenderer | Worldmodel de la USP (slot) |
| `prefabs/weapons/v_usp.prefab` | USPPistol, ModelRenderer | Viewmodel (fallback) |
| `prefabs/weapons/ub_melee.prefab` | MeleeWeapon, BoxCollider, ModelRenderer | Worldmodel melee |
| `prefabs/weapons/v_melee.prefab` | MeleeWeapon, ModelRenderer | Viewmodel melee |
| `prefabs/ai/saqueador.prefab` | NavMeshAgent, PerceptionComponent, SaqueadorBrain, CapsuleCollider, ModelRenderer | Hostil de raid/noche |
| `prefabs/ai/vecino.prefab` | NavMeshAgent, PerceptionComponent, VecinoBrain, CapsuleCollider, ModelRenderer | Civil |
| `prefabs/world/pf_loot_container.prefab` | LootContainer, InventoryComponent, BoxCollider, ModelRenderer | Contenedor de loot |
| `prefabs/world/pf_barricade_anchor.prefab` | BarricadeAnchor, BoxCollider, ModelRenderer | Punto de construcción |
| `prefabs/world/pf_enemy_spawn.prefab` | SpawnZone, BoxCollider(trigger) | Zona de spawn nocturno |
| `prefabs/world/pf_medical_point.prefab` | MedicalPoint, BoxCollider, ModelRenderer | Punto médico |
| `prefabs/world/pf_window_defense.prefab` | WindowDefensePoint, BoxCollider, ModelRenderer | Ventana de combate |
| `prefabs/world/pf_danger_zone.prefab` | DangerZone, BoxCollider(trigger) | Callejón peligroso |
| `prefabs/world/pf_shop_kiosk.prefab` | Trader, BoxCollider, ModelRenderer | Comercio pequeño |

## Colocación vertical slice (alrededor de A01 y A02)

| Elemento | GameObject padre | Prefab | Posición requerida | Rotación | Collider | Dependencias | Criterio de validación |
|---|---|---|---|---|---|---|---|
| Trader | `Sector_Plaza` (existente) | `pf_shop_kiosk` | junto al kiosco actual (sin solapar) | 0 | BoxCollider | Wallet/Inventory del jugador | Pulsa E → abre TraderUI |
| Estación de crafteo | `Sector_Plaza` | `ub_crafting_station` (ya existe) | a 8 m del kiosco | 0 | BoxCollider | PlayerInventory | Pulsa E → panel de crafteo |
| Punto médico | `Sector_Plaza` | `pf_medical_point` | a 5 m de la estación | 0 | BoxCollider | HealthComponent | E → cura con cooldown |
| Contenedor loot A01 | hijo de `apartment-a01` | `pf_loot_container` | interior A01 | 0 | BoxCollider | — | E → abre stash-like con loot |
| Contenedor loot A02 | hijo de `apartment-a02` | `pf_loot_container` | interior A02 | 0 | BoxCollider | — | ídem |
| Contenedor callejón | `Sector_Scrapyard` (extendido) | `pf_loot_container` (DangerousLoot) | en el callejón marcado | 0 | BoxCollider | — | E → loot peligroso |
| Anchor barricada A01 | hijo de `apartment-a01` | `pf_barricade_anchor` | puerta + ventana (2) | 0 | BoxCollider | ApartmentId=A01 | E con `barricade` → coloca |
| Anchor barricada A02 | hijo de `apartment-a02` | `pf_barricade_anchor` | puerta + ventana (2) | 0 | BoxCollider | ApartmentId=A02 | ídem |
| Ventana A01/A02 | hijo del apartamento, **nombre contiene "Window"** | `pf_window_defense` | bajo cada ventana | 0 | BoxCollider | ApartmentFortification auto-crea salud | Los saqueadores atacan la ventana |
| Zona spawn noche | `Sector_Scrapyard` | `pf_enemy_spawn` | 2 zonas | 0 | Trigger | HostilePrefab = `saqueador` (asignar) | De noche aparecen hostiles |
| Zona spawn raid | `Sector_Plaza` borde | `pf_enemy_spawn` | 1 zona | 0 | Trigger | HostilePrefab = `saqueador` | RaidManager usa SpawnPoint propio si existe |
| Callejón peligroso | `Sector_Scrapyard` | `pf_danger_zone` | cubriendo el callejón | 0 | Trigger | — | +2 hostiles de noche |
| Rutas de patrulla | hijo de cada NPC prefab o GO | `PatrolRoute` + hijos `wp` | waypoints visibles | — | — | NavMesh del mapa | El NPC recorre los wp |
| ApartmentFortification | hijo de `apartment-a01`/`a02` | componente directo (no prefab) | — | — | — | DoorReference = Claim Portal | Salud de puerta visible |

## Configuración de componentes en el editor

- **Saqueador/vecino**: asignar `HostilePrefab` en las SpawnZones; el
  `NavMeshAgent` requiere el NavMesh del mapa (`thieves.rpdowntown3t` debe
  tener navmesh horneada; si no, los NPC se quedan en Idle y se registra
  warning).
- **Trader**: precios por defecto (agua 10, medicina 20, munición 5,
  chatarra 2); el kiosco existente ya está en la escena.
- **ApartmentFortification**: `ApartmentId` = "apartment-a01"/"apartment-a02",
  `DoorReference` = el "Claim Portal" del apartamento.
- **LootContainer**: `LootTableId` = StreetLoot / ApartmentLoot / DangerousLoot.
- **HUD**: el prefab `player.prefab` ya incluye PlayerHud, HotbarPanel,
  HeldItemController, PlayerMovementModifier y Wallet — no añadir duplicados.

## Modelos y assets

- Placeholders dev (`models/dev/box.vmdl`, `models/sbox_props/...`) hasta
  tener assets finales. Los sonidos referenciados por el catálogo
  (`weapon.usp.shoot`, etc.) deben crearse como SoundEvents con esos nombres
  (EDT: añadir al AudioCatalog del proyecto).

## Validación final en editor (checklist)

1. Compilar: 0 errores / 0 avisos (Code/UltimoBarrio).
2. Play Mode: `ub_test_all` → todos los validadores en verde.
3. `ub_debug_spawn_apartment` no aplica (rama canónica): usar los anchors del
   overlay existente (6 apartamentos ya registrados).
4. A01/A02: reclamar, guardar en stash, reiniciar host → propietario y stash
   intactos (snapshot v2).
5. Noche forzada (`ub_qa_force_night`): hostiles en zonas, raid con
   saqueadores atacando puertas/ventanas, HUD de raid.
6. Dos clientes: hotbar, disparos, crafteo y stash sin desincronización.
