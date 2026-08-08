# Third-party notices

Este repositorio contiene o referencia contenido de terceros. Cada elemento conserva su licencia original.

La lista operativa se mantiene en:

```text
Assets/asset-registry.yml
```

## Reglas

- La licencia del código original del proyecto no relicencia assets externos.
- Los paquetes deben consumirse como referencia/dependencia cuando sea posible.
- Las librerías copiadas a `Libraries/` mantienen sus avisos.
- No eliminar autoría, LICENSE, NOTICE o metadatos.
- No publicar fuentes o binarios de terceros cuando su licencia no lo permita.
- Nada de lo marcado `PENDING_VERIFY` en `Assets/asset-registry.yml` entra en una build pública sin verificación previa.

## Registros iniciales

### Facepunch — s&box Weapons

Colección oficial de modelos, viewmodels, cargadores, munición y accesorios. La integración exacta y los términos aplicables deben verificarse dentro del editor y la documentación del paquete antes de publicar.

### OmniParadigm — Weapons

Paquete propuesto por el equipo. Estado: pendiente de spike técnico y legal. No se considera dependencia obligatoria hasta completar la evaluación.

## Paquete de contenido — spike/laptop-content-stack

Referencias candidatas del content pack portable (rama `spike/laptop-content-stack`).
Todas ellas están **pendientes de verificación** (licencia y ruta exacta) con Cloud Browser
antes de cualquier uso en build pública. Los prefabs del pack usan hoy únicamente
fallbacks verificados del contenido oficial del engine (`models/dev/*`, `models/citizen_props/*`, `models/sbox_props/*`).

| Candidato | Uso previsto | Estado |
|---|---|---|
| `facepunch.w_usp` | Worldmodel USP | PENDING_VERIFY — confirmar ruta montada y términos del paquete sboxweapons |
| `facepunch.w_crowbar` / palanca | Worldmodel melee | PENDING_VERIFY — confirmar existencia y licencia |
| `facepunch.knife` | Worldmodel cuchillo | PENDING_VERIFY — confirmar existencia y licencia |
| `facepunch.w_shotgun` | Worldmodel escopeta | PENDING_VERIFY — confirmar existencia y licencia |
| `facepunch.ammo_9mm` / `ammo_buckshot` | Modelos de munición | PENDING_VERIFY |
| `models/citizen/citizen.vmdl` + `.animgraph` | Modelo base de enemigos (Saqueador/Bruto/Merodeador) | PENDING_VERIFY — modelo estándar de Facepunch; confirmar ruta |
| Modelos de barricada/puerta/banco/generador/alarma | Fortificaciones | PENDING_VERIFY — sustituir cubos; investigar en asset store |
| SoundEvents (`weapon.usp.fire`, `build.wood.place`, etc.) | Audio del pack | PENDING_VERIFY — crear graphs propios o licenciar CC0/CC-BY |

Modelos ya verificados en el engine y usados como fallback hoy:

- `models/dev/plane.vmdl`, `models/dev/box.vmdl`
- `models/citizen_props/crate01.vmdl`
- `models/sbox_props/cardboard_box/cardboard_box_open.vmdl`
- `models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl`

## Audio importado — Facepunch/sandbox (MIT)

`Assets/sounds/content/weapons/dry_fire.wav` se importa del repositorio oficial `Facepunch/sandbox`
(https://github.com/Facepunch/sandbox, archivo `Assets/sounds/dry_fire.wav`), licencia MIT.

MIT License — Copyright (c) 2026 Facepunch. Se conserva el aviso completo de licencia en
`docs/licenses/MIT-Facepunch-sandbox.txt`. Uso: SoundEvent `sounds/content/weapons/usp_dry.sound`.
