# Sistema canónico de vivienda — decisión de arquitectura

Fecha: 2026-08-07. Rama: `integration/wizard-holy-grail`.

## Contexto

El repositorio tiene actualmente dos sistemas de vivienda que se solapan:

1. **`ApartmentComponent` / `ApartmentClaimInteractable` / `ApartmentClaimService`**
   (original). 6 apartamentos fijos codificados en la escena
   (`apartment-a01`..`apartment-a06`). Acceso por `OwnerId` directo (string,
   sin credencial). Sin soporte de alquiler, sin invitados, sin co-owners.
   Verificado físicamente en esta sesión de punta a punta: craft → claim →
   puerta (`ApartmentDoorPolicy`) → stash → respawn → abandon → persistencia
   Stop/Play. Ver `docs/production/autonomous-progress.md`.

2. **`PropertyComponent` / `PropertyClaimService` / `Properties/Doors/*` /
   `Properties/Keys/*`** (más nuevo, del pivote de arquitectura). Modelo
   general: `PropertyType` (Rental, AbandonedShell, Garage, Shop, BuildPlot,
   GroupBase — ver `PropertyComponent.cs`), `OwnerPersistentId`,
   `TenantPersistentId`, `CoOwners`, `Guests` (listas reales, no solo un
   owner), `AccessCredential`/`KeyringItem` (rekey, revocar, duplicar),
   `PropertyDoor` sobre `Sandbox.Mapping.Door` real. Código completo,
   compila, **0 instancias en la escena real** — nunca se ha visto un frame
   de este sistema funcionando.

`ApartmentClaimService` ya implementa `IPropertyAccessPolicy` como adapter
(Tarea #21, completada) — el proyecto ya venía empujando hacia
`PropertyComponent` como destino, no como un experimento paralelo abandonado.

## Decisión: **Opción A — migrar a la arquitectura Property**

`apartment-a01` y `apartment-a02` migran a `PropertyComponent` +
`DoorAnchor`/`PropertyDoor` + `KeyringService`. `apartment-a03`..`a06`
permanecen en `ApartmentComponent` hasta que se migren en una pasada
posterior (no se tocan en este bloque — siguen sirviendo de red de
seguridad si la migración de a01/a02 revela un problema).

### Por qué no la opción B (adaptar Property hacia Apartment)

- `ApartmentComponent` no tiene invitados, co-owners, ni alquiler — construir
  eso encima de un modelo pensado para 6 fixtures fijas sería reinventar
  `PropertyComponent` con otro nombre.
- El propio plan de distrito (`docs/world/first-district-manifest.md` y la
  lista de tareas #26-28) ya asume "4 Rental + 4 AbandonedShell + 1 Garage +
  1 Shop + 1 BuildPlot + 1 GroupBase" — tipos que solo `PropertyType` modela.
- BuildVolume/fortificación (sección 7 del encargo) necesita autorización por
  jugador (dueño puede construir, invitado quizá no, otra propiedad nunca) —
  `PropertyComponent.CoOwners`/`Guests`/`OwnerPersistentId` ya da esa forma;
  `ApartmentComponent.OwnerId` (un solo string) no.
- El propio encargo de esta pasada (sección 4) pide autorar `DoorAnchor` en
  apartment-a01/a02 — terminología y componente de `PropertyComponent`, no de
  `ApartmentComponent`. Construir sobre el sistema viejo iría contra la
  instrucción explícita del usuario.

### Por qué no migrar los 6 apartamentos de golpe

Regla dura del proyecto: "confirma que el cambio cabe en una sola PR". Migrar
los 6 a la vez sin haber visto `PropertyComponent` funcionar ni una sola vez
en el motor sería repetir exactamente el patrón de riesgo que ya causó bugs
reales esta sesión (persistencia rota, recogida con E rota, salud de
barricada en 0) — cambios grandes sin verificación intermedia. Se migran 2
como prueba real, se deja el resto en el sistema ya probado como red de
seguridad, y solo se completa la migración de a03-a06 en una pasada futura
una vez a01/a02 hayan sobrevivido varios ciclos Stop/Play y al menos una
noche de raid.

## Plan de migración para a01/a02

1. Añadir a cada GameObject `apartment-a01`/`apartment-a02` (o a un
   GameObject hijo nuevo, sin borrar el `ApartmentComponent` existente
   todavía): `PropertyComponent` (`PropertyType.AbandonedShell`, ya que el
   flujo real verificado es craft-kit → claim, no alquiler), `DoorAnchor`
   sobre la puerta física ya existente, `ClaimCabinetAnchor` cerca de la
   puerta, `RespawnAnchor` reutilizando la posición del actual "Owner Spawn
   Anchor".
2. Verificar con el harness genérico (`ub_qa_physical_interact`) el flujo
   completo sobre el sistema Property: craft kit real → instalar puerta →
   instalar armario → `PropertyClaimService.TryClaimAbandonedShell` real →
   rekey → `KeyringService` credencial → stash → respawn → Stop/Play.
3. **Solo cuando ese recorrido pase entero, sin atajos, en a01**: desactivar
   (no borrar todavía) el `ApartmentClaimInteractable`/`ApartmentComponent`
   de a01, repetir en a02.
4. `ApartmentComponent`/`ApartmentClaimInteractable`/`ApartmentDoorPolicy`
   **no se eliminan del código** en este bloque — siguen sirviendo a
   a03-a06. Se marcan como "sistema legacy, en migración" en un comentario
   de cabecera, no se tocan más allá de eso.
5. Actualizar `docs/production/autonomous-progress.md` con el resultado real
   (no solo "IMPLEMENTADO") antes de tocar a03-a06 en una pasada futura.

## Regla para todo trabajo nuevo a partir de ahora

Cualquier vivienda nueva que se autore en el distrito (garages, tiendas,
BuildPlots, GroupBase) usa `PropertyComponent` desde el principio —
`ApartmentComponent` no vuelve a instanciarse en escena nueva. Es legacy en
proceso de retirada, no una alternativa vigente.
