# Performance Budget & Rules — Alpha 0.1

## Métrica Objetivo
- **FPS Objetivo**: 60 FPS estables con 2 clientes conectados y 1 raid activo.
- **Frame Time CPU**: < 10 ms
- **Frame Time GPU**: < 8 ms

## Reglas de Red y Optimización
1. **Network Transmit**: Desactivar `AlwaysTransmit` para entidades dinámicas no críticas fuera del área de visión.
2. **Sin Búsquedas por Frame**: Prohibido el uso de `Scene.GetAllComponents` en `OnUpdate`. Cachear referencias en `OnStart`.
3. **Audio 3D Culling**: Sonidos ambientales y de mundo con límites de atenuación `MaxDistance` ajustados.
4. **Colisiones Estáticas**: Usar `Static = true` en todo prop o estructura decorativa que no se mueva.
