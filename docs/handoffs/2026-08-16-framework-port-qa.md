# QA — Framework port (estado actual)

Fecha: 2026-08-16
Rama inspeccionada: `feat/darkrp-framework-port`
Alcance: revisión de los commits `cccbe6c..69a4d46`. Este handoff no modifica código, escenas ni `STATE.md`.

## Entorno y evidencia base

- Editor s&box 26.08.05; proyecto `ultimo_barrio`.
- Antes de la prueba, `barrio_01` era la escena activa y no tenía cambios sin guardar.
- `LastCompileSucceeded=true`, `LastCompileErrors=0`.
- Play Mode de `barrio_01` inició y se detuvo correctamente durante esta revisión.
- `read_console` con mínimo `Error` no devolvió errores. El buffer conserva warnings históricos de compilaciones ya corregidas, además de warnings de fuente (`Arial`/`Verdana`) y de caché del engine; no deben interpretarse como fallos del arranque actual.

## Matriz de pruebas

| Bloque | Evidencia comprobada | Estado | Prueba funcional pendiente |
| --- | --- | --- | --- |
| Carryable / weapon framework | `UbCarryableComponent` restringe equipar/soltar al host y sincroniza `IsHeld`/`IsDropped`; `UbWeaponFrameworkComponent` contiene cargador, cooldown, recarga y ADS host-authoritative. | Parcial: contrato compila. | Instanciar un prefab real que derive de esta base; comprobar equipar, soltar, recoger y muerte con host y cliente. No hay referencias de prefab/escena a estas dos clases en la búsqueda actual. |
| Armas reales | `WeaponContentHost` implementa `IUbWeaponRuntime`; `UbWeaponCarrier` consulta esa interfaz para la presentación de ADS. El jugador generado en Play Mode incluye `UbWeaponCarrier`, `InventoryComponent` y `PlayerInteractor`. | Parcial: límite de runtime conectado. | Equipar cada arma, disparar, recargar, apuntar y cambiar 1ª/3ª persona con input real; confirmar que no hay doble disparo ni desincronización de cargador. |
| Schedule NPC | `Saqueador Inicial` contiene `EnemyContentHost`, `NavMeshAgent`, `EnemyPerception`, `EnemyAttack`, `Dresser` y `UbNpcScheduleRunner` en Play Mode. La consola registra `ClothingApplied` y varios `Schedule=Wander` con destinos distintos. | Aprobado para bootstrap/wander. | Provocar y comprobar `Investigate` y `Engage` contra un jugador, pérdida de objetivo e interrupción/limpieza del schedule; validar en segundo cliente. |
| GameLoop / persistencia | `WorldClock.SetPhase()` pide `PersistenceBridge.RequestSave("phase:<fase>")` solo en host. La consola registra inicialización del reloj y restauraciones de estado anteriores. | Parcial: cableado y arranque comprobados. | Forzar las cuatro transiciones, reiniciar la sesión después de cada una y verificar fase/restante restaurados; comprobar qué ocurre si el guardado falla. |
| Items / drop | `UbWeaponCarrier.DropCurrentOnHost()` materializa el pickup antes de retirar del inventario, conserva `AmmoInMag`, hace rollback si falla y solo confirma después. Existe el comando QA `ub_qa_test_drop_repickup`, pero no se ejecutó en esta revisión por ser QA de solo lectura. | Revisión estática aprobada; sin runtime nuevo. | Soltar y recoger una arma con cargador parcial en host y cliente; probar fallo deliberado de prefab/espacio y verificar rollback. |
| Interaction resolver | `PlayerInteractor` usa `InteractionResolver.Find`, `CanUse` y `TryUse` en la ruta por defecto; resuelve componentes en ancestros. | Parcial. | Probar un collider hijo de pickup, puerta, alijo, trader y estación. Claim, trader, crafting y contenedores conservan rutas especiales directas en `PlayerInteractor`, por lo que todavía no pasan todos por `TryUse`. |

## Hallazgos y riesgos

1. **Adopción incompleta de la base carryable.** La búsqueda no encontró usos de `UbCarryableComponent` ni `UbWeaponFrameworkComponent` fuera de su definición y `TimedWorldCleanup`; las armas actuales cruzan el límite ligero `IUbWeaponRuntime`, no heredan todavía de la base común. No presentar este bloque como migración completa hasta conectar al menos un prefab/arma real.
2. **Cobertura de interacción desigual.** El resolver común protege la ruta por defecto, pero las ramas especiales de claim, trader, crafting y contenedor siguen validando/ejecutando por su propia ruta. Debe decidirse si se centraliza realmente o se documentan esas excepciones como política explícita.
3. **No hay evidencia de dos clientes en esta pasada.** Autoridad de host está presente en los métodos inspeccionados, pero ownership, sincronización de cargador, drop y cancelación de schedules requieren host+cliente.
4. **Console hygiene.** El arranque probado no emitió errores, pero el buffer conserva warnings de fuente y caché, y errores de compilación históricos. Antes de marcar una versión candidata conviene limpiar o acotar la lectura de consola por ventana de sesión.

## Siguiente secuencia de QA recomendada

1. Añadir una prueba automatizada/escena de integración para un arma que use de verdad `UbWeaponFrameworkComponent` o decidir retirar la base no adoptada.
2. Ejecutar `ub_qa_test_drop_repickup` y una prueba específica de arma con cargador parcial, preferiblemente con host y segundo cliente.
3. Crear una situación reproducible de percepción para verificar `Wander → Investigate → Engage → Wander` y su réplica de red.
4. Forzar cada fase del reloj y reiniciar para demostrar persistencia de transición, no solo la llamada de guardado.
5. Ejercitar colliders hijo en todos los tipos de interacción y migrar/documentar las ramas especiales.
