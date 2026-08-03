# M1-01 — apartamento reclamable

Estado: **IMPLEMENTADO PARCIALMENTE, NO VALIDADO**

Fecha de trabajo: 2026-08-03

## Superficie preparada

- `ApartmentComponent` conserva los siete datos acordados.
- `ApartmentClaimInteractable` envía únicamente `ApartmentId`.
- `ApartmentClaimService` resuelve caller, identidad, jugador y distancia en
  host.
- La reserva cubre simultáneamente `ApartmentId` y `OwnerId`.
- El estado solo se aplica después de que el proveedor confirme el snapshot.
- `LocalPersistenceProvider` escribe generaciones nuevas, guarda el payload
  con CRC64 y relee el archivo antes de confirmar.
- Una versión futura válida bloquea carga y posteriores escrituras.
- `Prototype Apartment A01` incluye portal, alijo provisional y anchor de
  reaparición. El anchor no tiene `SpawnPoint` para que invitados no aparezcan
  dentro del apartamento.

## Evidencia obtenida

- El compilador live aceptó runtime y editor con 0 errores y 0 avisos después
  de implementar el código.
- La escena fue creada y guardada mediante MCP.
- El archivo `Assets/scenes/main.scene` contiene referencias serializadas a
  puerta, alijo y spawn.

## Intento que no cuenta como aceptación

La primera ejecución de Play Mode utilizó la copia antigua de la escena que el
editor mantenía en memoria. Esa copia aún tenía las tres referencias a `null`,
por lo que el registro falló cerrado con `missing DoorReference` y el claim fue
rechazado como `ServiceNotReady`. Este resultado demuestra la validación de
configuración, pero no demuestra un claim funcional.

Al intentar enlazar y guardar las referencias dentro del editor, el endpoint
MCP quedó ocupado. El proceso de s&box continuó respondiendo. Para preservar la
versión correcta se debe recargar la escena desde disco y no sobrescribirla con
la copia antigua en memoria.

## Pruebas pendientes

- [ ] Confirmar por MCP las tres referencias después de recargar.
- [ ] Compilación actual con 0 errores y 0 avisos.
- [ ] Claim dentro de rango.
- [ ] Rechazo fuera de rango.
- [ ] Segundo claim del mismo propietario rechazado.
- [ ] Fallo de guardado sin estado parcial.
- [ ] Reinicio conserva propietario y estado.
- [ ] Propietario reaparece en el anchor o usa fallback sin perder propiedad.
- [ ] Carrera produce un único ganador.
- [ ] Reconexión conserva propiedad.
- [ ] Cliente tardío recibe el estado y no puede modificarlo.
- [ ] Consola host limpia para el hito.
- [ ] Consola cliente revisada.

No se registran IDs personales ni nombres de perfil en este documento.
