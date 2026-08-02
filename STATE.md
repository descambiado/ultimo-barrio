# Estado operativo

> Este archivo es la fuente de verdad para reanudar el proyecto.

## Identidad

- Proyecto: Último Barrio
- Fase: Bootstrap
- Versión objetivo: `0.1.0-alpha`
- Rama estable: `main`
- Último commit estable: `PENDIENTE`
- Última prueba multijugador: `NO REALIZADA`

## Hito activo

### M0 — Boot estable y jugador visible

**Objetivo:** abrir el proyecto, cargar `main.scene`, mover un jugador y conectar dos clientes locales sin errores.

### Criterios de aceptación

- [ ] Proyecto creado mediante plantilla `Game → Empty`.
- [ ] Starter pack copiado.
- [ ] Repositorio Git público creado.
- [ ] `main.scene` guardada.
- [ ] Jugador local aparece correctamente.
- [ ] Movimiento y cámara funcionan.
- [ ] Segundo cliente puede unirse.
- [ ] Consola sin excepciones.
- [ ] Prueba reproducible documentada.

## Bloqueadores

- Proyecto real de s&box todavía no creado.
- Página/paquete `omniparadigm/weapons` pendiente de evaluación dentro del editor.
- Mapa inicial pendiente de selección.

## Siguientes tres acciones

1. Crear el proyecto vacío y copiar el starter pack.
2. Verificar MCP y escena principal.
3. Implementar el jugador mínimo y prueba de dos clientes.

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
