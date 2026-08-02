# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: Bootstrap
- Versión objetivo: `0.1.0-alpha`
- Rama estable: `main`
- Último commit estable: `a3084b0` (`chore: integrate sbox empty project`)
- Baseline documental: `0298207` (`bootstrap-v0.0.0`, local)
- Última prueba multijugador: `NO REALIZADA`
- Build de s&box instalada: `24338653` (Steam, verificada el 2026-08-02)

## Hito activo

### M0 — Boot estable y jugador visible

**Objetivo:** abrir el proyecto, cargar `main.scene`, mover un jugador y conectar dos clientes locales sin errores.

### Criterios de aceptación

- [x] Proyecto creado mediante plantilla `Game → Empty`.
- [x] Starter pack copiado.
- [ ] Repositorio Git público creado.
- [ ] `main.scene` guardada.
- [ ] Jugador local aparece correctamente.
- [ ] Movimiento y cámara funcionan.
- [ ] Segundo cliente puede unirse.
- [ ] Consola sin excepciones.
- [ ] Prueba reproducible documentada.

## Bloqueadores

- `Assets/scenes/main.scene` todavía no existe; la escena `minimal.scene` de la
  plantilla abre y compila, pero M0-03 sigue pendiente.
- Repositorio público, remoto `origin`, organización mantenedora, `CODEOWNERS`
  y canal privado de seguridad pendientes de configurar.

## Siguientes tres acciones

1. Configurar el remoto público y sustituir los placeholders de ownership y
   seguridad con datos reales del mantenedor.
2. Abordar solo M0-03: crear y guardar `Assets/scenes/main.scene` mediante el
   editor/MCP con la jerarquía base definida en la arquitectura.
3. Tras validar M0-03, abordar M0-04: jugador, movimiento y cámara.

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
- El manifiesto conserva valores de la plantilla (`Org: local`, 64 jugadores y
  `facepunch.flatgrass`) que deben revisarse antes de publicar o probar red.

## Registro de sesiones

| Fecha | Autor/agente | Rama | Resultado | Último commit estable |
|---|---|---|---|---|
| 2026-08-02 | Bootstrap documental | — | Starter pack generado | — |
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | Starter instalado, baseline Git local y preflight creados; s&box 24338653 instalado, pendiente crear el proyecto vía GUI | — |
| 2026-08-02 | Codex + subagentes | `feat/m0-bootstrap` | Proyecto Empty integrado; compilación de runtime y editor limpia; MCP y escena mínima validados | `a3084b0` |
