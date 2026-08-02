# M1-01 — Primer apartamento reclamable

Estado: **PREPARADO, NO IMPLEMENTADO**

Esta especificación conserva el alcance previsto del primer apartamento para poder abrir el issue sin adelantar código, economía, muebles o defensa dentro de M0.

## Objetivo

Crear un apartamento provisional que pueda pasar de libre a reclamado mediante una decisión autoritativa del host y conservar su propietario en un guardado local versionado.

## Contrato mínimo

El componente del apartamento expondrá exactamente estos datos:

| Campo | Propósito |
|---|---|
| `ApartmentId` | Identificador estable y único de la unidad. |
| `OwnerId` | Identidad opaca y estable resuelta por el host. Vacía mientras esté libre. |
| `ClaimState` | Estado `Unclaimed` o `Claimed` en la primera versión. |
| `DoorReference` | Anchor de la futura puerta; no implementa cerradura todavía. |
| `StashReference` | Anchor del futuro alijo; no implementa inventario todavía. |
| `SpawnReference` | Punto autorizado de aparición o reaparición del propietario. |
| `SaveVersion` | Versión del registro persistido; empieza en `1`. |

Las referencias de escena no se serializan en el guardado. Se vuelven a resolver por `ApartmentId` cuando carga la escena.

## Flujo previsto

```text
Apartamento sin dueño
→ el jugador interactúa con el punto de claim
→ el cliente envía únicamente ApartmentId
→ el host valida identidad, distancia, disponibilidad y ausencia de otro claim
→ el host reserva la unidad contra solicitudes simultáneas
→ el host guarda un snapshot v1
→ solo después del guardado asigna OwnerId y replica el resultado
→ en la siguiente sesión el propietario reaparece en SpawnReference
```

El cliente nunca elige ni envía `OwnerId`. Dos peticiones concurrentes sobre el mismo apartamento deben producir exactamente un ganador.

## Archivos previstos

```text
Code/UltimoBarrio/Apartments/ApartmentClaimState.cs
Code/UltimoBarrio/Apartments/ApartmentComponent.cs
Code/UltimoBarrio/Apartments/ApartmentRegistry.cs
Code/UltimoBarrio/Apartments/ApartmentClaimService.cs
Code/UltimoBarrio/Apartments/ApartmentClaimResult.cs
Code/UltimoBarrio/Players/IPlayerIdentityProvider.cs
Code/UltimoBarrio/Persistence/IPersistenceProvider.cs
Code/UltimoBarrio/Persistence/SaveSnapshot.cs
Code/UltimoBarrio/Persistence/ApartmentSaveData.cs
Code/UltimoBarrio/Persistence/LocalPersistenceProvider.cs
Code/UltimoBarrio/Persistence/SaveMigrator.cs
Assets/scenes/main.scene
docs/testing/m1-01-apartment-claim.md
```

Los nombres concretos de RPC o atributos de red se elegirán únicamente después de comprobar la API instalada de s&box.

## Estrategia de guardado

- Definir `IPersistenceProvider` como límite reemplazable.
- Empezar con `LocalPersistenceProvider`; no introducir una base de datos externa.
- Guardar un snapshot del host con `SaveVersion`, save slot y registros de apartamentos.
- Serializar únicamente `ApartmentId`, `OwnerId`, `ClaimState` y `SaveVersion`.
- Mantener un único escritor y escribir primero a un temporal o mecanismo equivalente seguro.
- No confirmar el claim hasta completar el guardado.
- Preservar el último snapshot válido ante fallos.
- Rechazar una versión futura o un archivo corrupto sin sobrescribirlo.
- Migrar versiones antiguas mediante pasos explícitos.

## Riesgos de red y persistencia

- Usar como `OwnerId` un ID efímero de conexión o un nombre visible.
- Permitir que el cliente controle el propietario o el punto de respawn.
- Atravesar una operación asíncrona sin reservar el apartamento y aceptar dos ganadores.
- Replicar el claim antes de que el guardado sea válido.
- Ejecutar dos escrituras simultáneas sobre el snapshot.
- Duplicar `ApartmentId` al copiar un prefab.
- Serializar GameObjects o conexiones de red.
- Cargar ownership antes de registrar los apartamentos de la escena.
- Perder la propiedad cuando falta o está bloqueado `SpawnReference`.
- Entregar estado obsoleto a un cliente que se incorpora después del claim.

## Criterios de aceptación

- [ ] Existe el contrato con los siete campos definidos.
- [ ] `ApartmentId` vacío o duplicado bloquea el claim con un error claro.
- [ ] Las referencias de puerta, alijo y spawn se validan al registrar la unidad.
- [ ] El cliente solo envía `ApartmentId`; el host resuelve `OwnerId`.
- [ ] Un jugador fuera de rango no puede reclamar.
- [ ] Un jugador no puede reclamar una segunda unidad.
- [ ] Dos solicitudes simultáneas producen un único ganador.
- [ ] Un fallo de guardado no deja un propietario confirmado parcialmente.
- [ ] Reiniciar el host conserva propietario y estado.
- [ ] Desconectar y reconectar no libera la unidad.
- [ ] El propietario reaparece en `SpawnReference` o en un fallback seguro sin perder propiedad.
- [ ] Un segundo cliente recibe el estado actual y no puede alterarlo localmente.
- [ ] Runtime y editor compilan sin errores.
- [ ] Play Mode no genera excepciones.
- [ ] Las pruebas de claim, reinicio, reconexión y carrera quedan documentadas.

## Fuera de alcance

- Economía, muebles, mejoras o defensa.
- Contenido del alijo e inventario.
- Cerraduras, daño y reparaciones.
- Transferir, vender o liberar una propiedad.
- Varios apartamentos por jugador.
- Host migration.
- Backend externo o UI definitiva.
