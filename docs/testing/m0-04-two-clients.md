# M0-04 — Validación local con dos clientes

- Fecha: 2026-08-03
- Rama: `feat/m0-bootstrap`
- Build de s&box: `26.07.22` (Steam BuildID `24338653`)
- Plataforma: editor `sbox-dev.exe` + cliente independiente `sbox.exe`

## Preparación

La escena dejó de contener un Player directo. `Systems/Network` usa el
componente oficial `Sandbox.NetworkHelper` con:

```text
StartServer = true
PlayerPrefab = templates/gameobject/player controller.prefab
SpawnPoints = null
```

Con `SpawnPoints = null`, el helper oficial utiliza los componentes
`SpawnPoint` existentes en la escena. En esta prueba solo existía
`SpawnPoints/Primary Spawn`.

## Ejecución

1. Se inició Play Mode sobre `main.scene`.
2. El helper creó la sesión y generó exactamente un jugador para el host.
3. Se lanzó una segunda instancia con el mismo flujo que usa el editor:

   ```powershell
   sbox.exe -joinlocal +instanceid 1 -sw -720
   ```

4. El host registró conexión y alta de la segunda instancia.
5. La escena runtime expuso exactamente dos jugadores distintos, ambos con
   `PlayerController`, `Rigidbody`, los tres modos de movimiento y `Dresser`.
6. El operador comprobó en ambas ventanas movimiento y cámara independientes.
   La segunda instancia cargó el avatar del perfil local.
7. Se cerró la segunda ventana. El host registró salida y desconexión; su
   GameObject desapareció y quedó exactamente el jugador del host.
8. Se detuvo Play Mode y la escena volvió a edición sin cambios sin guardar.

No se registran en este documento nombres de perfil, Steam IDs ni rutas de
usuario.

## Resultados

| Comprobación | Resultado | Evidencia |
|---|---|---|
| Host inicia sesión | PASS | Play Mode creó escena runtime y un jugador host. |
| Segundo cliente conecta | PASS | Log del host: connecting, joined y connected. |
| Dos jugadores sin duplicado | PASS | MCP contó exactamente dos objetos `Player - …`. |
| Ownership práctico | PASS | Cada ventana controló únicamente su jugador. |
| Movimiento independiente | PASS | Comprobación manual en ambas ventanas. |
| Cámara independiente | PASS | Comprobación manual en ambas ventanas. |
| Avatar del cliente | PASS | La segunda instancia mostró el avatar del perfil local. |
| Desconexión | PASS | Log `left`/`disconnected`; conteo volvió de dos a uno. |
| Consola del host | PASS | 0 entradas `Error` entre 64 entradas almacenadas. |
| Log del cliente revisado | PASS CON INCIDENCIAS | La sesión funcionó, pero `sbox.log` contiene errores de recursos descritos abajo. |

## Incidencias observadas

### Spawn solapado

La segunda instancia apareció en el aire antes de poder moverse con normalidad.
La causa más probable es que ambos jugadores compartieran el único spawn y sus
colliders se separaran físicamente. No se capturó telemetría suficiente para
declarar la causa como demostrada.

M0-04 valida red, ownership práctico, input y desconexión, pero el siguiente
smoke test debe añadir asignación de spawn sin solapamiento y repetir la unión.

### Recursos informados por el cliente

El log del cliente independiente registró errores de recursos ausentes del
engine/Base Library y comprobaciones de `scenes/minimal.scene_c` y
`scenes/main.scene_c`. A pesar de ello, el cliente cargó la sesión replicada,
mostró avatar y respondió a movimiento/cámara.

No se declara la consola del cliente como limpia. Antes de una build pública se
debe repetir la prueba desde paquete compilado y clasificar cuáles entradas son
ruido del entorno dev y cuáles requieren empaquetado adicional.

## Repetir la prueba

1. Abrir `ultimo_barrio.sbproj` con `scenes/main.scene` activa.
2. Confirmar `Systems/Network` y ausencia de Player directo en la escena fuente.
3. Iniciar Play Mode y esperar al jugador host.
4. Desde la instalación de s&box ejecutar:

   ```powershell
   .\sbox.exe -joinlocal +instanceid 1 -sw -720
   ```

5. Confirmar dos jugadores distintos en el host.
6. Mover y girar cámara en cada ventana; comprobar que la otra conexión no
   recibe ese input.
7. Cerrar solo la instancia `sbox.exe`.
8. Confirmar que el host elimina el jugador remoto y continúa funcionando.
9. Revisar por separado la consola del editor y `logs/sbox.log`.
10. Detener Play Mode y comprobar que la escena fuente sigue guardada.

## Resultado del hito

M0-04 está **COMPLETADO** funcionalmente. Quedan dos seguimientos explícitos
para el smoke test: spawn sin solapamiento y clasificación de errores de
recursos del cliente independiente.
