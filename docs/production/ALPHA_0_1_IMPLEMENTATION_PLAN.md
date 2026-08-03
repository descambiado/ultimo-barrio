# Alpha 0.1 Integration Plan

El objetivo es transicionar el proyecto desde el estado de Spike funcional (M1.5) hacia una Alpha 0.1 jugable, modular y medible, respetando la regla inquebrantable de **una vivienda por jugador** en producción, y utilizando una interfaz QA para pruebas locales de múltiples viviendas.

## User Review Required
> [!IMPORTANT]
> Se crearán múltiples subagentes para paralelizar el trabajo en diferentes "carriles".
> Requiero tu confirmación antes de disparar la creación masiva de ramas y el trabajo de los subagentes paralelos, para asegurar que la arquitectura de integración y los contratos están acordes a tus expectativas.

## Proposed Changes

### 1. Control del Proyecto & QA
- **Ramas:** Se creará la rama base `feat/alpha-0.1-integration` a partir del HEAD actual.
- **Documentación:** Se generarán los documentos solicitados en `docs/production/` (`ALPHA_0_1_ROADMAP.md`, `SYSTEM_MATRIX.md`, `INTEGRATION_CONTRACTS.md`, `STYLE_BIBLE.md`, `QA_PLAYBOOK.md`).
- **QA Panel/Console:** Implementaré herramientas exclusivas de QA para forzar asignaciones de A02 sin romper la regla global, manipular fases del día, y generar recursos/enemigos bajo demanda.

### 2. Definición de Contratos (Contracts)
Definiremos e implementaremos las interfaces unificadoras:
- `IInteractable`, `IPlayerIdentityProvider`
- `IInventory`, `IInventoryContainer`, `IApartmentAccessPolicy`
- `IPersistenceProvider`, `IWallet`, `IDamageable`, `IWorldClock`, `IRaidTarget`

### 3. Delegación a Subagentes (Carriles)
Lanzaré subagentes para trabajar en ramas derivadas de `feat/alpha-0.1-integration`:
- **Carril A (Foundation & QA)**: Rama `feat/alpha-foundation`. Responsable de QA, Feature Flags, IDs deterministas.
- **Carril B (Viviendas & Inventario)**: Rama `feat/alpha-housing-inventory`. Responsable de A01/A02 físicos, mochilas, stash, shift-clic.
- **Carril C (Economía)**: Rama `feat/alpha-economy`. Responsable del Wallet y Trader.
- **Carril D (Combate)**: Rama `feat/alpha-combat`. Responsable de la pistola, munición, daño y salud.
- **Carril E (IA & Raid)**: Rama `feat/alpha-ai-raid`. Responsable del saqueador y sistema de raid.
- **Carril F (Mundo & Presentación)**: Rama `feat/alpha-presentation`. Responsable de la UI y los estilos visuales (sin sci-fi, luz de sodio, etc).

### 4. Integración y WorldClock
Como integrador, supervisaré el trabajo, implementaré el `WorldClock` (Día, Preparación, Noche, Consecuencias) y consolidaré los resultados progresivamente en `feat/alpha-0.1-integration`. Validaré que ningún subagente modifique `main.scene`.

## Verification Plan

### Automated Tests
- Compilación de cada rama antes del merge (`dotnet build`).
- Verificación de la serialización del QA slot.

### Manual Verification
- Validar el loop jugable utilizando las herramientas de QA para forzar fases (Noche, Raid).
- Verificar que el progreso completo (Inventario, Vivienda, Dinero) se mantiene al guardar y cargar.
- Con dos clientes: verificar la independencia de stashes, carteras y la correcta replicación del combate e IA.
