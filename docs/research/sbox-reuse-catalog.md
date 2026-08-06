# Catálogo de reutilización — s&box / Último Barrio

Investigación realizada siguiendo [[sbox-integration-factory]] y [[sbox-license-auditor]].
Motor instalado: **26.07.22** (`C:\Program Files (x86)\Steam\steamapps\common\sbox`).
Fecha de investigación: 2026-08-05.

Las fichas completas de candidatos de código externo viven en `.research/` (no se
comitea el código de los candidatos, solo la ficha). Este documento es el índice y
resumen de decisión.

## Parte 1 — API nativa instalada (motor 26.07.22)

Confirmado por grep directo sobre `Sandbox.Engine.xml` / `Sandbox.System.xml` /
`Sandbox.Tools.xml` — no por memoria de una versión anterior del motor.

| Tipo nativo | Existe | Aporta | Uso recomendado en Último Barrio |
|---|---|---|---|
| `Sandbox.PlayerController` | Sí | Movimiento, mirada, cámara 1ª/3ª persona | **Ya integrado** — ver el fix de cámara/movimiento de esta sesión. Único propietario de look/move/camera. |
| `Sandbox.BaseInventoryComponent` | Sí | Inventario por slots basado en `BaseInventoryItem`, pickup por Touch/Use, pooling de munición compartido (`GetAmmo`) | Ver `docs/research/native-inventory-migration.md` — **no se adopta**, el `InventoryComponent` local ya cubre esto con requisitos propios (stash, transferencias, persistencia por apartamento) |
| `Sandbox.BaseInventoryItem` | Sí | Slot individual, `SlotOrder`, `PreferredSlot`, `ShouldAvoid` | Mismo veredicto — patrón de prioridad de slot es útil como referencia, no como base |
| `Sandbox.BaseCombatWeapon` | Sí | Fire/Reload/ammo pool, `ShootBullet`, `BulletTrace`, efectos de impacto en red | El `Combat.BaseCombatWeapon` local ya replica esta forma (fire/reload/ammo/RPC) casi 1:1 — ver migración |
| `Sandbox.BaseWeaponModel` | Sí | Modelo de arma vinculado al inventario | No usado — el `HeldItemController` local resuelve esto por bone attach + prefab |
| `Sandbox.Physics.HingeJoint` / `HingeJointBuilder` | Sí | Bisagra física nativa | **Candidato fuerte para puertas físicas** — ver sección Puertas/Barricadas abajo |
| `Sandbox.Physics.FixedJoint`, `SliderJoint`, `SpringJoint`, `BallSocketJoint` | Sí | Otros joints físicos | Candidatos para barricadas atornillables (FixedJoint) o mecanismos deslizantes |
| `Sandbox.NavMeshAgent` / `NavMeshLink` / `NavMeshArea` / `NavMeshGameSystem` | Sí | Navegación nativa para NPCs | Relevante para IA de saqueadores (fuera de alcance de este bloque, catalogado para después) |
| `IUse` / `Interactable` (interfaz genérica de interacción) | **No encontrado** con ese nombre exacto en el XML | — | El proyecto ya tiene su propio `IWorldInteractable`/`PlayerInteractor` — ver migración, no hace falta nativo |
| `Sandbox.Mapping.Door` | **No encontrado** | — | No existe un componente de puerta nativo genérico; las puertas del mapa son geometría del propio `.vmap`, no un componente reutilizable para puertas de apartamento nuevas |

### Plantillas y muestras oficiales instaladas

```
C:\Program Files (x86)\Steam\steamapps\common\sbox\templates\game.playercontroller\
C:\Program Files (x86)\Steam\steamapps\common\sbox\samples\sweeper\
```

`game.playercontroller` ya se usó como referencia para el fix de cámara de esta
sesión (confirma la jerarquía correcta: cámara suelta en la escena, controlador como
único `ICameraModifier`). No contiene inventario ni armas — es un ejemplo de
movimiento puro.

## Parte 2 — Repositorios comunitarios/oficiales investigados

Ficha resumida por candidato. Fichas completas con el formato de
[[sbox-license-auditor]] en `.research/<slug>.md`. Los repos con clon local viven en
`.research/vendor/` (gitignored, solo para inspección — ver [[sbox-integration-factory]]).

| Candidato | Fuente | Licencia | Veredicto | Por qué |
|---|---|---|---|---|
| Facepunch sbox-hc1 | github.com/Facepunch/sbox-hc1 | **Sin LICENSE** (404 en `/blob/main/LICENSE`) → All Rights Reserved por defecto | **REFERENCIA** (no ADOPTAR/ADAPTAR) | FPS completo de Facepunch, útil para leer patrones de arma/red actuales, pero sin licencia no se puede copiar código. |
| timmybo5/simple-weapon-base | github.com/timmybo5/simple-weapon-base | MIT | **EXTRAER PATRÓN** | Weapon base madura (hitscan+físico, sway, attachments, offset editor) pero deliberadamente sin inventario — no encaja con el modelo de datos local. Útil para sway/aim/attachment en el Bloque D, no ahora. |
| matekdev/sbox-arcade-car-physics | github.com/matekdev/sbox-arcade-car-physics | MIT | **ADAPTAR (spike futuro)** | Multijugador listo de fábrica, autoridad del dueño sobre el coche. Candidato principal para el spike de vehículos (Bloque G) — no se integra en este bloque. |
| kurozael/sbox-inventory | github.com/kurozael/sbox-inventory | MIT | **EXTRAER PATRÓN (parcial)** | Inventario tipo grid — más complejo de lo que Último Barrio necesita. El patrón de sincronización host-autoritativa ya está efectivamente presente en `InventoryComponent` local. |
| Nebual/sandbox-plus | github.com/Nebual/sandbox-plus | MIT | **EXTRAER PATRÓN** | Fork del gamemode Sandbox con tool de constraints. Referencia de UX para colocación de barricadas — no se adopta el addon completo. |
| echohello-dev/basebound | github.com/echohello-dev/basebound | MIT | **ADAPTAR (pendiente de archivos concretos)** | Base building con economía activa/idle, raids con warrant, contratos. Ficha: `.research/echohello-basebound.md`. |
| Softsplit/sandbox-plus-plus | github.com/Softsplit/sandbox-plus-plus | **GPL-3.0** | **EXTRAER PATRÓN únicamente — no copiar código** | Copyleft: código derivado tendría que relicenciarse bajo GPL-3.0. Solo referencia de UX para constraints/placement. Ficha: `.research/softsplit-sandbox-plus-plus.md`. **No confundir con `Nebual/sandbox-plus` (MIT, repo distinto).** |
| apetavern/sbox-fortwars | github.com/apetavern/sbox-fortwars | **Sin LICENSE** (confirmado tras clonar) | **DESCARTAR** | Archivado desde 2024-05-30, commit más reciente de 2022, sin archivo LICENSE. Ficha: `.research/apetavern-sbox-fortwars.md`. Si se necesita referencia de fortificación, auditar `Nolankicks/Fortwars` por separado. |
| dxura/dxrp | github.com/dxura/dxrp | **Propietario, "todos los derechos reservados"** | **DESCARTAR — no clonar** | Prohíbe explícitamente redistribución y uso comercial sin permiso escrito. Ficha: `.research/dxura-dxrp.md` (verificación de licencia únicamente, sin clon). |

### Puertas y barricadas — decisión de arquitectura

No existe un componente `Door` nativo reutilizable ni un repositorio candidato con
licencia clara que resuelva esto directamente. Decisión: **EXTRAER PATRÓN** —
construir la puerta de apartamento como:

- Geometría/anclaje: un `GameObject` hijo del apartamento en una posición fija
  (`DoorAnchor`, ya referenciado por el manifiesto de distrito), no una puerta física
  articulada con `HingeJoint` en este bloque — el requisito real es "instalada / no
  instalada" con niveles de daño, no una puerta que se abre y cierra físicamente.
  `HingeJoint` queda catalogado para si en el futuro se pide que la puerta se pueda
  abatir físicamente.
- Estado (`DoorLevel`, `DoorHealth`) vive en el esquema de persistencia ya definido
  en [[ultimo-barrio-architect]], no en un componente físico separado.

## Parte 3 — Búsquedas que no dieron candidato

Documentado explícitamente porque una búsqueda vacía es un resultado válido, no un
fallo (ver [[sbox-integration-factory]] — "cuándo esta skill dice escribir código nuevo de
todas formas"):

- **Sistema de puertas nativo genérico**: no existe `Sandbox.Mapping.Door` ni
  equivalente reutilizable para puertas de apartamento instalables por el jugador.
- **`IUse`/`Interactable` nativo genérico**: no se encontró una interfaz de
  interacción de uso general en el XML del motor instalado bajo ese nombre — el
  patrón de interacción del proyecto (`IWorldInteractable`) es una construcción
  local legítima, no una que debería haberse basado en un tipo nativo que no existe.

## Próximos pasos

1. Completar fichas pendientes en `.research/` para Fortwars antes de tocar
   fortificación en profundidad (bloque G).
2. Cuando se aborde el bloque I (armas), volver a `timmybo5/simple-weapon-base` con
   una ficha completa antes de decidir adoptar/adaptar/descartar su sistema de sway
   y attachments.
3. El spike de vehículos (fuera del recorrido principal) usa
   `matekdev/sbox-arcade-car-physics` como punto de partida cuando se programe.
