# matekdev/sbox-arcade-car-physics

- **Nombre:** s&box Arcade Car Physics
- **Fuente:** https://github.com/matekdev/sbox-arcade-car-physics
- **Commit/tag/revisión:** main @ 4 commits (SHA no fijado en esta pasada)
- **Última actualización:** no confirmada en esta pasada
- **Licencia:** MIT
- **Código fuente disponible:** Sí
- **Sistema que aporta:** Física de vehículo arcade — simulación de raycast/física de
  vehículo, listo para multijugador de fábrica (el dueño tiene autoridad completa
  sobre el coche, las ruedas se replican visualmente a todos los clientes), soporte
  de gizmo en editor, manejo de input propio.
- **API de s&box utilizada:** Framework de física de vehículos, redes, input —
  específicos no confirmados sin clonar.
- **Compatibilidad probable:** Alta — el modelo de autoridad (dueño controla,
  visuales replicados) coincide exactamente con el patrón host-authoritative que ya
  usa el resto del proyecto (armas, inventario).
- **Archivos concretos útiles:** Pendiente — todo el sistema es candidato dado que
  es pequeño (4 commits, alcance acotado).
- **Dependencias:** Ninguna declarada más allá del motor.
- **Conflictos con nuestro proyecto:** Ninguno identificado — los vehículos no
  tocan inventario/apartamentos, área nueva.
- **Riesgo de networking:** Bajo — ya diseñado para multijugador con el mismo
  modelo de autoridad del proyecto.
- **Riesgo de persistencia:** Ninguno todavía relevante (vehículos no forman parte
  del recorrido de vivienda ni tienen requisito de persistencia definido aún).
- **Trabajo de integración:** Bajo-Medio si se adopta — punto de partida directo
  para el spike de vehículos.
- **Veredicto:** ADAPTAR — candidato principal para el spike aislado de vehículos.
  **No se integra en este bloque de trabajo** (regla explícita: catalogar
  vehículos, preparar spike aparte, no meterlos en el recorrido principal).
