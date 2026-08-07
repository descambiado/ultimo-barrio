# Laboratorios de spike — nodo portátil (spike/laptop-content-stack)

Escenas aisladas para probar el contenido portable del paquete `Content`
sin tocar `ultimo_barrio_alpha.scene` ni el core antiguo.

> ⚠️ Generadas por script. Si cambias el formato de escena del editor,
> re-genera con `python scripts/labs/generate_lab_scenes.py` (no edites a mano
> los GUIDs generados).

## Escenas

| Escena | Qué probar | Teclas |
|---|---|---|
| `Assets/scenes/spikes/weapon_lab.scene` | USP, palanca, cuchillo, escopeta (spawn, disparo/melee, daño a IDamageTarget, recarga, dry fire) | Slot1..Slot4 equipan; attack1 dispara; R recarga; Drop suelta |
| `Assets/scenes/spikes/enemy_lab.scene` | Spawn de Saqueador/Bruto/Merodeador, NavMesh, persecución, ataque al dummy, daño, muerte, botín | Slot1..Slot3 spawnan enemigos en el marcador |
| `Assets/scenes/spikes/building_lab.scene` | Colocar 9 fortificaciones, daño, reparación, upgrade (madera→reforzada) | Slot1..Slot9 colocan; R repara la más cercana |
| `Assets/scenes/spikes/vehicle_lab.scene` | Stub: rellenar prefabs cuando el research decida el paquete de vehículos (manifest bloque H) | Slot1 spawna vehículo (pendiente) |

## Cómo abrir

1. Abre s&box con el proyecto en la rama `spike/laptop-content-stack`.
2. En el editor, abre la escena de lab deseada y pulsa Play (el `NetworkHelper`
   spawnea al jugador en `Primary Spawn`).
3. Verifica en consola que no hay errores del proyecto.

## Qué esperar (primera pasada)

- **weapon_lab**: al pulsar Slot1..Slot4 se instancia el prefab del arma como
  hijo del jugador; los modelos usan los fallbacks verificados (los candidatos
  primarios están marcados ⚠️ PENDING_VERIFY en los registros).
- **enemy_lab**: usa `MapInstance thieves.rpdowntown3t` (verificado) para que
  NavMeshAgent tenga navmesh. Los enemigos persiguen al dummy (FortificationContentHost
  como IDamageTarget) y sueltan pickups al morir.
- **building_lab**: las fortificaciones reciben daño de las armas del weapon_lab
  (mismo contrato IDamageTarget) y se pueden reparar con R.

## Verificación pendiente (cuando el editor esté disponible)

- [ ] Compilación del proyecto con 0 errores (atención a `Scene.GetAllComponents<T>()`
      en los spawners Dev y a `NavMeshAgent` en `EnemyContentHost`).
- [ ] Carga de las 4 escenas sin errores de serialización.
- [ ] `models/dev/plane.vmdl` en los labs de suelo (usado por main.scene, pero confirmar).
- [ ] Cloud Browser: verificar modelos primarios ⚠️ de armas, enemigos y fortificaciones
      y actualizar `AssetsVerified` en los registros.
- [ ] Sonidos: crear los SoundEvent referenciados (`weapon.usp.fire`, etc.) o vaciar los campos.

## Contrato de portabilidad

Todo lo que hay bajo `Code/UltimoBarrio/Content/` y `Assets/prefabs/content/`
es autocontenido: no referencia `InventoryComponent`, `HeldItemController`,
`AIBase` ni `HealthComponent` del core antiguo. Para portar a
`integration/wizard-holy-grail` basta cherry-pick de:

- `Code/UltimoBarrio/Content/` (contratos + hosts)
- `Assets/prefabs/content/` (prefabs)
- `Assets/scenes/spikes/` (labs)
- `docs/research/laptop-content-integration-manifest.md` (decisión de stack)

Los bridges al core nuevo (daño, inventario, sonido, animación) están marcados
con `TODO(core nuevo)` en el código.
