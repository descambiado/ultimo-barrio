# Registro de código de terceros

Este documento cubre **código** de terceros (candidatos investigados, adoptados,
adaptados o descartados). Los **assets** (modelos, materiales, sonidos) tienen su propio
registro operativo en `Assets/asset-registry.yml` y su resumen en
`THIRD_PARTY_NOTICES.md` — no se duplican aquí.

Cada candidato tiene una ficha completa en `.research/<slug>.md` con el formato definido
por [[sbox-license-auditor]]. Este documento es el índice legal — qué se puede citar,
qué no, y por qué — no repite el contenido completo de cada ficha.

## Adoptado / adaptado en el código del proyecto

Ninguno todavía. Cuando un candidato pase de ADOPTAR/ADAPTAR en su ficha a código real
dentro de `Code/`/`Assets/`/`Libraries/`, se añade aquí una entrada con:
- Qué archivo(s) locales incorporan el patrón/código
- De qué candidato viene (enlace a la ficha)
- Bajo qué licencia se distribuye Último Barrio esa porción (si la licencia de origen lo
  exige — ver la entrada GPL-3.0 abajo)

## Candidatos investigados — sin código copiado, solo patrón o referencia

| Candidato | Licencia | Ficha | Qué se puede usar |
|---|---|---|---|
| `Nebual/sandbox-plus` | MIT | `.research/nebual-sandbox-plus.md` | Patrón de UX de constraints — no se copia código, MIT lo permitiría pero no ha hecho falta |
| `timmybo5/simple-weapon-base` | MIT | `.research/timmybo5-simple-weapon-base.md` | Patrón de sway/aim/attachments para el Bloque D (armas) — pendiente |
| `matekdev/sbox-arcade-car-physics` | MIT | `.research/matekdev-arcade-car-physics.md` | Candidato ADAPTAR para el spike de vehículos (Bloque G) — pendiente |
| `kurozael/sbox-inventory` | MIT | `.research/kurozael-sbox-inventory.md` | Patrón de sincronización — ya replicado localmente, sin acción pendiente |
| `echohello-dev/basebound` | MIT | `.research/echohello-basebound.md` | Candidato ADAPTAR para contratos/raids con warrant — pendiente de auditoría de archivos concretos |

## Exclusiones explícitas — no citar, no copiar, no clonar de nuevo

| Candidato | Licencia | Ficha | Motivo de exclusión |
|---|---|---|---|
| `dxura/dxrp` | **Propietaria, todos los derechos reservados** | `.research/dxura-dxrp.md` | Prohíbe explícitamente redistribución y uso comercial sin permiso escrito. **No clonado.** Ni siquiera como referencia de patrón — la licencia no distingue entre copia de código y aprendizaje de patrón. |
| `Softsplit/sandbox-plus-plus` | **GPL-3.0** | `.research/softsplit-sandbox-plus-plus.md` | Copyleft: cualquier código derivado distribuido tendría que relicenciarse bajo GPL-3.0, algo que este proyecto no ha decidido hacer. **Solo lectura para entender el flujo UX de constraints/placement — cero líneas de código copiadas.** No confundir con `Nebual/sandbox-plus` (MIT, repo distinto, sí utilizable). |
| `apetavern/sbox-fortwars` | **Ninguna (confirmado tras clonar)** | `.research/apetavern-sbox-fortwars.md` | Sin archivo LICENSE = todos los derechos reservados por defecto. Repo archivado desde 2024-05-30, último commit de 2022. |
| `Facepunch/sbox-hc1` | **Ninguna** (404 en LICENSE) | `.research/facepunch-sbox-hc1.md` | Repositorio público de Facepunch sin licencia explícita. Válido como lectura para entender patrones actuales del propio motor; no como fuente de copia. |

## Regla operativa

Antes de que cualquier línea de código de un candidato externo entre en `Code/`,
`Assets/`, o `Libraries/`:

1. Su ficha en `.research/` debe tener veredicto ADOPTAR o ADAPTAR (no EXTRAER PATRÓN,
   SOLO ASSETS, REFERENCIA, ni DESCARTAR).
2. Su licencia debe permitir explícitamente la redistribución en un proyecto que puede
   monetizarse — GPL-family requiere una decisión de relicenciar que el equipo no ha
   tomado, así que ADOPTAR/ADAPTAR no aplica a código GPL sin esa decisión explícita.
3. La integración sigue [[sbox-git-safety]] — checkpoint branch antes de integrar, rama
   aislada por dependencia, atribución en comentario o entrada en este documento.
