# Backlog inicial

## Epic M0 — Bootstrap

- [x] M0-00 Instalar starter, crear baseline Git local y preflight agentic.
- [x] M0-01 Crear proyecto Empty.
- [ ] M0-02 Inicializar repositorio y remoto.
- [ ] M0-03 Crear `main.scene`.
- [ ] M0-04 Jugador, movimiento y cámara.
- [ ] M0-05 Segunda instancia local.
- [ ] M0-06 Smoke test documentado.

### Progreso comprobado — 2026-08-02

- M0-02 sigue pendiente: no existe `origin` porque la autenticación de GitHub
  CLI está expirada.
- M0-03 tiene una implementación parcial verificable: `Assets/scenes/main.scene`
  contiene `World`, `Systems`, `SpawnPoints` y `Debug`, además de suelo, luz,
  cielo, punto de aparición, el prefab oficial `Player Controller` y cámara.
- El primer Play Mode local inició y se detuvo sin errores de consola. El
  jugador fue visible y el render de cámara quedó registrado en
  `docs/media/first-boot.png`.
- M0-03 y M0-04 permanecen abiertos porque el movimiento no se ha probado con
  input real; el MCP disponible no puede inyectarlo.
- M0-05 permanece abierto: no se ejecutó una segunda instancia porque el MCP
  disponible no ofrece control de un segundo cliente.

## Epic M1 — Apartamento

- [ ] M1-01 Definición de apartamento.
- Especificación previa de M1-01: `docs/planning/m1-01-apartment-claim.md`.
  No existe implementación todavía.
- [ ] M1-02 Claim host-authoritative.
- [ ] M1-03 Puerta interactiva.
- [ ] M1-04 Contenedor de alijo.
- [ ] M1-05 Save snapshot v1.
- [ ] M1-06 Load y recuperación de error.

## Epic M2 — Ciclo

- [ ] M2-01 Game phases.
- [ ] M2-02 Reloj sincronizado.
- [ ] M2-03 Amanecer.
- [ ] M2-04 Día.
- [ ] M2-05 Preparación.
- [ ] M2-06 Noche.
- [ ] M2-07 Resultado.

## Epic M3 — Economía y sospecha

- [ ] M3-01 Items data-driven.
- [ ] M3-02 Comerciante.
- [ ] M3-03 Compra/venta.
- [ ] M3-04 Contrabando.
- [ ] M3-05 Inspección.
- [ ] M3-06 Sospecha.
- [ ] M3-07 Encargo clandestino.

## Epic M4 — Fortificación

- [ ] M4-01 Integridad de puerta.
- [ ] M4-02 Integridad de ventana.
- [ ] M4-03 Reparación.
- [ ] M4-04 Persiana/tablones.
- [ ] M4-05 Escondite.
- [ ] M4-06 Validación de colocación.

## Epic M5 — Raid

- [ ] M5-01 Raid objective.
- [ ] M5-02 Director.
- [ ] M5-03 Percepción.
- [ ] M5-04 Explorador.
- [ ] M5-05 Asaltante.
- [ ] M5-06 Saqueador.
- [ ] M5-07 Escape con botín.
- [ ] M5-08 Resultado persistente.

## Spikes

- [ ] SPIKE-WEAPONS-001 Comparar armas oficiales y OmniParadigm.
- [ ] SPIKE-MAP-001 Seleccionar manzana.
- [ ] SPIKE-SAVE-001 Guardado local.
- [ ] SPIKE-AI-001 Saqueador vertical.
