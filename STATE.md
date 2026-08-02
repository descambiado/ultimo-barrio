# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: Bootstrap
- Versión objetivo: `0.1.0-alpha`
- Rama estable: `main`
- Último commit estable: `PENDIENTE` (no existe todavía una build verificable)
- Baseline documental: `0298207` (`bootstrap-v0.0.0`, local)
- Última prueba multijugador: `NO REALIZADA`
- Build de s&box instalada: `24338653` (Steam, verificada el 2026-08-02)

## Hito activo

### M0 — Boot estable y jugador visible

**Objetivo:** abrir el proyecto, cargar `main.scene`, mover un jugador y conectar dos clientes locales sin errores.

### Criterios de aceptación

- [ ] Proyecto creado mediante plantilla `Game → Empty`.
- [x] Starter pack copiado.
- [ ] Repositorio Git público creado.
- [ ] `main.scene` guardada.
- [ ] Jugador local aparece correctamente.
- [ ] Movimiento y cámara funcionan.
- [ ] Segundo cliente puede unirse.
- [ ] Consola sin excepciones.
- [ ] Prueba reproducible documentada.

## Bloqueadores

- Proyecto real todavía no creado: faltan el `.sbproj`, `Code/` y
  `Assets/scenes/main.scene` generados o guardados desde el editor. s&box está
  instalado y abierto, pero el MCP `127.0.0.1:7269` no escucha sin un proyecto.
- Repositorio público, remoto `origin`, organización mantenedora, `CODEOWNERS`
  y canal privado de seguridad pendientes de configurar.

## Siguientes tres acciones

1. Crear `ultimo_barrio` mediante `Game → Empty` en una carpeta vacía temporal,
   con la organización real, y fusionar después los archivos generados sin
   sobrescribir el starter ni escribir el `.sbproj` a mano.
2. Configurar el remoto público y sustituir los placeholders de ownership y
   seguridad con datos reales del mantenedor.
3. Activar el MCP, ejecutar `scripts/check-m0-preflight.ps1` y abordar solo la
   primera tarea M0 no bloqueada.

## Decisiones vigentes

- Ambientación ficticia mediterránea.
- Solo-first y cooperativo.
- Host autoritativo.
- Persistencia desacoplada mediante interfaz.
- Armas oficiales como camino base.
- Dependencias externas registradas.
- No construir la Alpha alrededor de OmniParadigm hasta completar el spike.

## Riesgos actuales

- Querer añadir contenido antes de cerrar el bucle principal.
- Elegir un mapa demasiado grande.
- Mezclar persistencia local y multijugador sin una interfaz.
- Hacer IA compleja antes de tener objetivos físicos sencillos.
- Varias IAs modificando los mismos archivos.

## Registro de sesiones

| Fecha | Autor/agente | Rama | Resultado | Último commit estable |
|---|---|---|---|---|
| 2026-08-02 | Bootstrap documental | — | Starter pack generado | — |
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | Starter instalado, baseline Git local y preflight creados; s&box 24338653 instalado, pendiente crear el proyecto vía GUI | — |
