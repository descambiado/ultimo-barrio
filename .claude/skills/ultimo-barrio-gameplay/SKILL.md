---
name: ultimo-barrio-gameplay
description: Use whenever making a gameplay, content, or scope decision for Último Barrio - deciding what to build next, whether a feature fits the vision, how a system should behave, or what the first playable loop must contain. This is the canonical design reference: map, fantasy, the mandatory first-playthrough path, and the per-player apartment persistence schema. Consult before proposing new systems, before reordering the roadmap, and before implementing anything that touches apartment ownership, crafting, or the day/night loop.
---

# ultimo-barrio-gameplay

Último Barrio es un survival urbano persistente para s&box. Esta skill es la fuente
canónica de la visión de diseño — consúltala antes de decidir *qué* construir, no
solo *cómo*.

## Mapa

```
thieves.rpdowntown3t
```

Toda colocación de contenido (anclas de puerta, estaciones de crafteo, traders,
puntos médicos, zonas de peligro) se ancla a este mapa concreto. Ver
[[sbox-runtime-proof]] para el manifiesto de distrito con posiciones reales
verificadas en editor, no coordenadas inventadas.

## Fantasía

De día: recoger, comerciar, fabricar, ocupar y reforzar viviendas.
De noche: defender el barrio y la casa frente a saqueadores y robos.

La ambientación es una ciudad mediterránea ficticia. No usar nacionalidades, etnias
o religiones reales como facciones enemigas (regla ya establecida en el CLAUDE.md
raíz del proyecto — se repite aquí porque toca directamente el diseño de facciones).

## Requisitos de juego

- Funcional en solitario **y** en multijugador — ninguna mecánica puede asumir que
  hay un único jugador conectado, ni romperse cuando solo hay uno.
- Cada jugador tiene estadísticas, vivienda, stash, mejoras y progresión
  **separadas** — el estado de un jugador nunca debe filtrarse al de otro salvo
  donde el diseño lo pida explícitamente (robos, PvP, raids).
- La vivienda evoluciona físicamente y persiste — el nivel de mejora se ve en el
  mundo, no solo en un stat interno.

## Recorrido inicial obligatorio (acceptance path)

Este es el camino mínimo que define "el juego funciona". Cualquier feature nueva se
evalúa contra si ayuda a completar este recorrido o es prematura:

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

Cada paso debe ser una acción física real del jugador (input real, UI real, host
autoritativo validando), nunca un comando QA simulando el resultado — ver
[[sbox-runtime-proof]] para la disciplina de verificación que aplica a cada paso.

## Esquema de persistencia por vivienda

La vivienda persiste por `OwnerPersistentId`, no por sesión. Campos mínimos:

```
ApartmentId
OwnerPersistentId
DoorLevel
DoorHealth
WindowBarricades
BarricadeHealth
StashContents
UpgradeLevel
DefenseScore
LastRaidAt
RepairState
```

Cualquier cambio al modelo de datos de apartamentos existente (`ApartmentClaimService`,
componentes relacionados) debe verificar primero cómo migrar los saves ya en disco —
ver `docs/research/native-inventory-migration.md` para el patrón de migración
incremental y reversible, y nunca perder progreso de jugador por un cambio de esquema.

## Niveles de mejora de vivienda

```
Nivel 0: vivienda abandonada
Nivel 1: puerta básica
Nivel 2: puerta reforzada + barricadas
Nivel 3: puerta blindada + mayor resistencia y almacenamiento
```

Cada nivel debe verse físicamente en el mundo — un `UpgradeLevel` que solo cambia un
número sin representación visual no cuenta como implementado.

## Orden de prioridad de contenido

No es una sugerencia — es el orden en que el trabajo real avanza, y por qué:

```
1. Recogida real con E (la base de toda interacción de item)
2. Crafting de door kit (primer sistema de crafteo, valida el patrón general)
3. Reclamar primera vivienda instalando la puerta (valida ownership + persistencia)
4. Barricadas y mejora de vivienda (extiende el mismo patrón de anclas/instalación)
5. Persistencia y prueba de reinicio (cierra el loop — sin esto nada de lo anterior
   "cuenta")
6. Solo después: sustituir armas-cubo por sistema de armas real
```

Armas, IA, raids, y vehículos son deliberadamente posteriores al loop de vivienda —
no porque sean menos importantes al final, sino porque el loop de vivienda es lo que
prueba que ownership, persistencia, crafteo e interacción funcionan de verdad. Meter
armas antes de eso construye combate sobre una base no verificada.

## Materiales iniciales

```
scrap_metal, wood, cloth, components, water, medicine, ammo_9mm
```

Deben usar modelos reales verificados vía Cloud Browser o assets licenciados — nunca
cubos ni modelos dev/error como contenido "terminado" (ver [[sbox-reuse-first]]
reglas duras).

## Recetas iniciales

```
apartment_door_kit:   wood x8, scrap_metal x6, components x2
window_barricade_kit: wood x6, scrap_metal x3
```

## Armas del primer bloque

```
puños, crowbar, cuchillo, USP, escopeta
```

Cinco armas funcionando completamente (viewmodel, worldmodel, animación, input,
daño host-authoritative, munición, recarga, drop, pickup, hotbar, persistencia,
audio) antes de importar una sola arma más allá de estas cinco.
