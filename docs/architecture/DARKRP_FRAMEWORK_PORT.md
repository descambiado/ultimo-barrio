# Port del framework DarkRP

Este documento cambia el alcance de la integración: DarkRP se utiliza como
framework de referencia y fuente MIT, no solo como colección de patrones.

## Objetivo

Portar las capacidades estructurales que hacen que DarkRP sea un framework
jugable completo, adaptándolas a la autoridad host y a los dominios de Último
Barrio. No se copiarán nombres de dominio de DarkRP (jobs, policías, dinero o
administración) cuando contradigan el diseño del barrio.

## Superficies que se portan

1. **Runtime y control de partida** — `GameLoop`, spawn, límites, eventos y
   ciclo de conexión.
2. **Base de objetos transportables** — carryable world/view model, ownership,
   attach/detach, drop y limpieza.
3. **Framework de armas** — `BaseWeapon`, munición, recarga, ironsights,
   proyectiles, animaciones, recoil y efectos.
4. **Interacción física** — props, constraints, tool actions y undo, reducidos
   al conjunto permitido por la supervivencia del barrio.
5. **NPC completo** — senses, navegación, schedules, tasks, layers, combate,
   patrulla, habla y estados de vida.
6. **Jugador y cámara** — cámara, inventario equipado, daño, muerte,
   observador, persistencia y controles.
7. **UI y economía física** — inventario, hotbar, pickups, tiendas y dinero,
   reinterpretados como alijo, recursos y comercio.
8. **Red y persistencia** — host authority, RPCs, ownership, snapshots y
   guardados versionados.

## Inventario completo de origen

El árbol que se debe revisar y portar es el siguiente; ninguna carpeta queda
fuera por defecto:

```text
Code/
├─ Cleanup       ├─ Components   ├─ Economy      ├─ FreeCam
├─ Game          ├─ GameLoop     ├─ Items        ├─ Jobs
├─ Map           ├─ Npcs         ├─ Player       ├─ Save
├─ Spawner       ├─ UI           ├─ Utility      └─ Weapons

Assets/
├─ ammotype      ├─ entities     ├─ fonts        ├─ hydraulics
├─ jobs          ├─ materials     ├─ models       ├─ prefabs
├─ scenes        ├─ shaders       ├─ sounds       ├─ surface
├─ textures      ├─ thrusters     ├─ UI           └─ weapons
```

La revisión se hará archivo por archivo para identificar dependencias de
DarkRP (jobs, policía, dinero, administración) y sustituirlas por las
equivalentes de Último Barrio (hogares, sospecha, recursos, apartamentos y
asaltos). Los assets se importarán solo cuando su licencia, origen y
dependencias estén registrados en `Assets/asset-registry.yml`.

## Orden de integración

```text
FrameworkKernel
  ├─ Carryable + ownership
  ├─ WeaponBase + WeaponModel + IronSights
  ├─ Player runtime + inventory/equipment
  ├─ NPC schedules/tasks/layers
  ├─ Interaction/tools/undo
  ├─ UI + economy adapters
  └─ Apartment/raid/game-flow integration
```

Cada bloque se compila y se prueba dentro del proyecto antes de activar el
siguiente, pero el diseño se mantiene como un único port, no como sistemas
aislados incompatibles.

## Fuentes MIT revisadas

- `DarkRP/Code/GameLoop/`
- `DarkRP/Code/Game/Weapon/`
- `DarkRP/Code/Npcs/`
- `DarkRP/Code/Player/`
- `DarkRP/Code/UI/`
- `DarkRP/Code/Items/`
- `DarkRP/Code/Weapons/`

La atribución y la licencia se conservan en `THIRD_PARTY_NOTICES.md`. El port
no incorpora el sistema de jobs policiales ni decisiones de autoridad del
framework original que entren en conflicto con Último Barrio.

## Estado inicial

- Recoil, ADS, viewmodel presentation, efectos y wander ya están integrados en
  la primera capa experimental.
- Falta consolidar el núcleo común para que las próximas armas, NPCs,
  interacción y persistencia dependan de las mismas abstracciones.
- El siguiente bloque de implementación es `FrameworkKernel + BaseCarryable +
  BaseWeapon/IronSights`, antes de ampliar el árbol de NPC.
