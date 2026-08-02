# Política de seguridad

## Reportar una vulnerabilidad

No abras un issue público cuando el problema permita:

- Duplicar dinero o inventario.
- Ejecutar acciones de host desde cliente.
- Escribir o leer datos persistentes ajenos.
- Saltarse validaciones de construcción.
- Forzar spawns o daño.
- Bloquear un servidor.
- Acceder a secretos o endpoints privados.

Contacta de forma privada con los mantenedores mediante el canal indicado en el perfil de la organización. Incluye:

- Versión o commit.
- Pasos mínimos.
- Impacto.
- Prueba de concepto no destructiva.
- Mitigación propuesta.

## Reglas técnicas

- El cliente nunca decide recompensas, daño final, propiedad o guardado.
- Toda entrada de red se valida.
- Los identificadores persistentes no se confían al cliente.
- No se incluyen secretos en el repositorio.
- Los endpoints externos deben tener autenticación, rate limiting y logs.
- Las herramientas MCP que muten escenas deben requerir confirmación del cliente.
