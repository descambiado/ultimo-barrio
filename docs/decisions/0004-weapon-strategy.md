# ADR-0004 — Estrategia de armas

- Estado: Proposed
- Fecha: 2026-08-02

## Decisión propuesta

Usar lógica oficial basada en `BaseCombatWeapon` y assets oficiales de Facepunch. Evaluar OmniParadigm en un spike aislado.

## Razón

Las armas son una dependencia secundaria. Apartamentos, IA, persistencia y bucle no deben quedar acoplados a un paquete no verificado.

## Criterio de cambio

Adoptar OmniParadigm solo si supera al camino oficial en velocidad, mantenimiento y extensibilidad sin crear lock-in.
