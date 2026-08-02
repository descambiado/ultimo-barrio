# Backlog inicial

## Epic M0 — Bootstrap

- [x] M0-00 Instalar starter, crear baseline Git local y preflight agentic.
- [x] M0-01 Crear proyecto Empty.
- [x] M0-02 Publicar repositorio, ramas, tag, issues y draft PR.
- [x] [M0-03](https://github.com/descambiado/ultimo-barrio/issues/1) Crear `main.scene` y validar el primer boot jugable.
- [ ] [M0-04](https://github.com/descambiado/ultimo-barrio/issues/2) Validar sesión local con dos clientes.
- [ ] M0-05 Completar el smoke test reproducible de M0.

### Progreso comprobado — 2026-08-03

- M0-02 está completado: el repositorio público, `origin`, `main`,
  `feat/m0-bootstrap`, `bootstrap-v0.0.0`, los seis issues iniciales y la draft
  PR están publicados.
- M0-03 está completado: `Assets/scenes/main.scene`
  contiene `World`, `Systems`, `SpawnPoints` y `Debug`, además de suelo, luz,
  cielo, punto de aparición, el prefab oficial `Player Controller` y cámara.
- Play Mode local inició y se detuvo sin errores del proyecto. El jugador fue
  visible, el render quedó registrado en `docs/media/first-boot.png` y el
  2026-08-03 se comprobaron manualmente WASD, salto, ratón y cambio entre
  tercera y primera persona.
- M0-04 permanece abierto: no se ejecutó una segunda instancia. El camino
  oficial está identificado, pero falta configurar el spawn de red y validar
  ambos clientes.

## Epic M1 — Apartamento

- [ ] [M1-01](https://github.com/descambiado/ultimo-barrio/issues/3) Crear apartamento reclamable.
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

- [ ] [SPIKE-WEAPONS-001](https://github.com/descambiado/ultimo-barrio/issues/5) Comparar armas oficiales y OmniParadigm. Investigación terminada; falta prueba aislada.
- [x] [SPIKE-MAP-001](https://github.com/descambiado/ultimo-barrio/issues/4) Seleccionar la primera manzana. Resultado publicado: blockout propio y pequeño mediante Scene Mapping.
- [ ] [SPIKE-SAVE-001](https://github.com/descambiado/ultimo-barrio/issues/6) Validar guardado local versionado. Investigación terminada; falta implementación y matriz de recuperación.
- [ ] SPIKE-AI-001 Saqueador vertical.
