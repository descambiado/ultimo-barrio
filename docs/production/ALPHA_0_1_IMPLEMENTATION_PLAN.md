# Alpha 0.1 Runtime Repair Plan

Este plan detalla las acciones correctivas necesarias para transformar el build "verde" en una experiencia Alpha 0.1 completamente funcional en runtime.

## Open Questions
- Debido a que mi entorno MCP no tiene acceso directo a S&box Editor en modo interactivo (`main.scene` Play Mode), ejecutarÃ© verificaciones estÃ¡ticas profundas en la escena y en el cÃ³digo. GenerarÃ© las escenas de prueba `test_spanish_text.scene` y actualizarÃ© `main.scene` mediante manipulaciÃ³n directa de JSON/S&box properties y scripts. Â¿EstÃ¡s de acuerdo con este enfoque puramente estÃ¡tico + scripts QA para resolver los Gates?

## Proposed Changes

### 1. Limpieza de Repositorio y CodificaciÃ³n
- [DELETE] `refactor_contracts.py`, `fix_compile.py`, `fix_compile2.py` (si existen).
- [MODIFY] `docs/production/evidence/encoding-audit.md` generado.
- [MODIFY] Todos los `.cs`, `.razor`, `.md` con errores de codificaciÃ³n serÃ¡n reescritos a UTF-8 (sin BOM).

### 2. Identidad y Persistencia Determinista
- [MODIFY] `InventoryComponent.cs`: Eliminar `Guid.NewGuid().ToString()` al generar identificadores de inventario.
  - Los jugadores usarÃ¡n `player:{PlayerId}:inventory`.
  - Los alijos usarÃ¡n `{ApartmentId}:stash`.
  - Los mercaderes usarÃ¡n `{TraderId}:stock`.
- [MODIFY] `PlayerIdentityProvider`: Mantener una API limpia que exponga el `PlayerId` de manera segura, sin parsear strings no validados.

### 3. Autoridad y RPCs (Host-Authoritative)
- [MODIFY] `ApartmentClaimService.cs`: Asegurar que `RequestClaim` valida distancia y pertenencia en el host.
- [MODIFY] `InventoryComponent.cs`: Convertir acciones de inventario (`RequestTransfer`, `RequestDrop`) a validaciÃ³n estricta del host (sin confiar en el cliente).
- [MODIFY] `Trader.cs` / `TraderUI.razor.cs`: Las compras y ventas (`BuyItem`, `SellItem`) serÃ¡n autoritativas en el host.
- [MODIFY] `BaseCombatWeapon.cs`: Validar que el daÃ±o es propagado desde el host hacia los clientes, y no al revÃ©s.

### 4. Cadena de InteracciÃ³n y Estados de UI
- [MODIFY] `PlayerInteractor.cs`: Corregir el ciclo completo del raycast.
  - Si la distancia > lÃ­mite, anular la interacciÃ³n.
  - Al alejarse, limpiar referencias activas.
- [MODIFY] `PlayerHud.razor` (y relativos):
  - Implementar la mÃ¡quina de estados: `Gameplay`, `InventoryOnly`, `InventoryAndStash`, `Trader`, `Dead`.
  - Iniciar cerrado (Gameplay).
  - Bloquear / liberar el movimiento/cursor adecuadamente.

### 5. AuditorÃ­a de Escenas
- [MODIFY] `main.scene`: 
  - Validar y purgar singletons duplicados (`NetworkHelper`, `WorldClock`, etc.).
  - Asegurar la presencia fÃ­sica de `apartment-a01`, `apartment-a02`, `trader-neighborhood`.
- [NEW] `test_spanish_text.scene`: CreaciÃ³n de la escena de prueba para validar glifos y tipografÃ­a.

## Verification Plan

### Automated Tests (Static QA)
- Auditar referencias a `Guid.Parse` y `Rpc.Owner` en acciones crÃ­ticas.
- Verificar el contenido de `main.scene` usando parseo JSON para confirmar Singletons y Prefabs.

### Manual Verification (User)
- Al completar los Gates 1 a 8 de este plan, el usuario deberÃ¡ iniciar S&box y cargar `test_spanish_text.scene` y `main.scene` para validar visualmente la codificaciÃ³n y la jugabilidad del hito Alpha 0.1, de acuerdo a la matriz actualizada.
