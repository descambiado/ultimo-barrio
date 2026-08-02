# Política de assets y dependencias

## Objetivo

Aprovechar el ecosistema de s&box sin convertir el proyecto en una colección frágil, ilegal o imposible de mantener.

## Regla de entrada

Ningún asset entra sin una fila en:

```text
Assets/asset-registry.yml
```

## Campos obligatorios

- `id`
- `name`
- `author`
- `source_url`
- `package_type`
- `license`
- `license_verified`
- `integration_mode`
- `version_or_checked_at`
- `used_by`
- `fallback`
- `notes`

## Modos de integración

### Reference

Se referencia el paquete remoto. Preferido para mapas y assets estables.

### Vendored library

Se instala en `Libraries/` y se versiona su código fuente. Mantener licencia y cambios documentados.

### Imported source

Solo con licencia que permita copia/modificación/publicación.

### Temporary prototype

Puede usarse en una rama de spike, pero no llegar a release sin verificación.

## Prohibido

- Ripear juegos comerciales.
- Migrar contenido de Garry's Mod sin derechos.
- Eliminar créditos.
- Suponer licencia por estar en el Workshop.
- Reempaquetar un mapa que prohíba modificaciones.
- Añadir un package ID solo desde memoria.
- Incluir assets generados por IA sin revisar derechos y coherencia.

## Armas

### Facepunch `sboxweapons`

Uso previsto:

- Viewmodels.
- Worldmodels.
- Brazos.
- Cargadores.
- Munición.
- Accesorios.
- Animaciones.

La lógica se mantendrá separada mediante componentes oficiales y adaptadores propios.

### OmniParadigm `weapons`

Estado: **candidate / pending verification**.

Completar `SPIKE-WEAPONS-001`:

1. Instalar en rama aislada.
2. Identificar tipo de paquete.
3. Localizar código fuente.
4. Leer licencia.
5. Enumerar dependencias.
6. Crear arma mínima.
7. Probar host y cliente.
8. Probar recarga.
9. Probar daño.
10. Probar join-in-progress.
11. Medir errores y hotload.
12. Documentar sustitución.

Resultado posible:

- Adoptar como dependencia.
- Extraer patrones permitidos.
- Usar solo assets.
- Rechazar.

## Mapas

La Alpha debe usar:

- Una manzana pequeña.
- Interiores accesibles.
- Ventanas y balcones.
- Pocas rutas legibles.
- NavMesh viable.
- Buen rendimiento.
- Permisos claros.

Si se usa un mapa comunitario, la lógica del juego debe existir en nuestra escena/prefabs, no incrustada de forma irreversible en el mapa.

## Sustitución progresiva

Cada dependencia importante tiene:

- Abstracción.
- Fallback.
- Issue de sustitución.
- Propietario.
- Prioridad.

El prototipo puede ser visualmente prestado. La identidad final debe migrar hacia assets propios o claramente licenciados.
