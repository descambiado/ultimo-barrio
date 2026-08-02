# Spikes técnicos

## SPIKE-WEAPONS-001 — Base de armas

### Pregunta

¿Debemos usar la implementación de OmniParadigm, los componentes oficiales o una combinación?

### Ramas

```text
spike/weapons-facepunch
spike/weapons-omniparadigm
```

### Prueba mínima

- Pistola.
- Viewmodel.
- Worldmodel.
- Disparo.
- Munición.
- Recarga.
- Daño validado por host.
- Cambio de arma.
- Join-in-progress.
- Hotload.
- Sin errores de consola.

### Matriz

| Criterio | Oficial | OmniParadigm |
|---|---:|---:|
| Fuente visible | Verificar | Verificar |
| Licencia | Verificar | Verificar |
| Autoridad de red | Probar | Probar |
| Integración inventario | Probar | Probar |
| Animaciones | Sí, assets disponibles | Verificar |
| Dependencias | Bajas esperadas | Verificar |
| Sustituible | Alta | Verificar |
| Mantenimiento | Oficial | Verificar |

### Decisión predeterminada

Usar componentes oficiales y assets Facepunch salvo evidencia fuerte a favor de OmniParadigm.

## SPIKE-MAP-001 — Manzana inicial

Evaluar máximo tres mapas. Criterios:

- Interiores.
- Balcones.
- Ventanas.
- NavMesh.
- Tamaño.
- Licencia.
- Rendimiento.
- Identidad.
- Posibilidad de sustituir.

## SPIKE-SAVE-001 — Guardado local

Probar:

- Perfil nuevo.
- Save.
- Load.
- Archivo corrupto.
- Versión antigua.
- Dos perfiles.
- Host/client.
- Reinicio.

## SPIKE-AI-001 — Saqueador

Demostrar:

- Selecciona contenedor.
- Navega.
- Abre/brecha.
- Recoge objeto.
- Cambia de objetivo si bloqueado.
- Trata de salir.
- Suelta botín al morir.
