# Open Source Adoption Matrix — Último Barrio (Alpha 0.1)

Documentación de repositorios de código abierto evaluados para su integración, adaptación o referencia en el desarrollo de la Alpha 0.1 de *Último Barrio*.

---

## Matriz de Evaluación

| Repositorio | Versión / Commit | Licencia | Propósito Evaluado | Compatibilidad s&box | Decisión | Modificaciones / Atribución Necesaria |
|---|---|---|---|---|---|---|
| **Facepunch/sandbox** | `main` (2024-2026) | MIT | Patrones de Player Lifecycle, Network Spawning, Camera/Input, Connection Management | Alta (Oficial) | **Adaptar** | Extraer patrones de spawning, RPCs de posición e interfaz `Connection`. Conservar copyright Facepunch. |
| **timmybo5/simple-weapon-base** | `v1.2.0` | MIT | Base de armas (Hitscan, recarga, munición, equipar, animaciones) | Alta | **Adaptar (Vendor Controlado)** | Crear `UltimoBarrioWeaponAdapter` para vincular con `InventoryComponent`, `Wallet` y `DamageEvent`. Preservar licencias. |
| **Nebual/sandbox-plus** | `master` | MIT | Eventos modulares, permisos de interacción y gestión de ownership | Media | **Solo referencia** | Utilizar como referencia conceptual para `IWorldInteractable` y permisos de apartamentos. No importar herramientas Sandbox. |
| **Facepunch/sbox-public** | `main` | MIT / Public Code | Ejemplos oficiales de UI en Razor, SCSS y componentes de físicas/transformaciones | Alta (Oficial) | **Adaptar** | Referencia de estructuras Razor/SCSS y patrones de componentes `PanelComponent`. |
| **Facepunch/sbox-docs** | `main` | CC-BY-4.0 | Documentación oficial de API (`Game.ActiveScene`, `Connection.All`, `[Rpc.Host]`, `[Sync]`) | Alta (Oficial) | **Solo referencia** | Guía de sintaxis y mejores prácticas de s&box Scene System. |

---

## Detalle por Repositorio

### 1. Facepunch/sandbox
- **URL**: `https://github.com/Facepunch/sandbox`
- **Licencia**: MIT
- **Uso**: Referencia estándar para el ciclo de vida del jugador, respawn en `SpawnPoint`, delegación de `Connection.All` e identidad de red.
- **Estrategia**: Adaptación directa en `PlayerController` y `PlayerInteractor`.

### 2. timmybo5/simple-weapon-base
- **URL**: `https://github.com/timmybo5/simple-weapon-base`
- **Licencia**: MIT
- **Uso**: Sistema base de armas para disparo hitscan, gestión de cargador y feedback.
- **Estrategia**: Adaptar mediante `UltimoBarrioWeaponAdapter` que conecta el inventario de s&box con el balance de munición y `HealthComponent.TakeDamage`.

### 3. Nebual/sandbox-plus
- **URL**: `https://github.com/Nebual/sandbox-plus`
- **Licencia**: MIT
- **Uso**: Referencia para arquitecturas de interacción y despacho de eventos de red.
- **Estrategia**: Solo referencia para la separación estricta de `IWorldInteractable`.

---

## Compromisos de Licencia
- No se copia código sin licencia explícita.
- Toda adaptación de repositorios MIT mantiene el aviso de copyright original en `docs/production/THIRD_PARTY_NOTICES.md`.
- No se introducen dependencias monolíticas externas que modifiquen el `ultimo_barrio.csproj`.
