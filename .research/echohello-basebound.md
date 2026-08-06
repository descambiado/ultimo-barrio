# echohello-dev/basebound

- **Nombre:** Basebound
- **URL:** https://github.com/echohello-dev/basebound
- **Revisión exacta:** `eb5c7bd0bcfacf8fd76e9aa26b2edb80b4986601`
- **Fecha:** 2026-07-24
- **Licencia de código:** MIT (confirmado, `LICENSE` en la raíz del repo clonado)
- **Licencia de assets:** No verificada por separado en esta pasada — revisar antes de
  tomar cualquier modelo/material, la licencia de código no cubre automáticamente los
  assets incluidos.
- **Atribución:** MIT estándar — mantener el aviso de copyright `(c) 2026 echoHello` en
  cualquier archivo derivado.
- **Código fuente disponible:** Sí, completo (clonado en `.research/vendor/basebound/`)
- **Sistema que aporta:** Base building estilo BaseWars — colocación de estructuras,
  ownership, economía activa/idle, raids con sistema de "warrant", contratos.
- **Dependencias:** No auditadas en profundidad esta pasada — pendiente antes de adoptar
  ningún archivo concreto.
- **Archivos concretos útiles:** Pendiente de identificar tras lectura dirigida — el
  candidato más prometedor para el patrón de "raids con objetivo" y "contratos", que
  Último Barrio todavía no tiene modelado (el proyecto ya tiene `RaidManager`/
  `IRaidParticipant` propios; comparar antes de tomar nada).
- **Riesgos:** Ninguno de licencia (MIT limpio). Riesgo de arquitectura: su modelo de
  ownership puede no coincidir con `OwnerPersistentId` local — verificar antes de adaptar
  cualquier pieza de persistencia.
- **Veredicto:** ADAPTAR (pendiente de auditoría de archivos concretos) — clonado y
  licencia verificada; la extracción de patrones específicos (contratos, warrant de raid)
  queda para cuando se aborde el Bloque E/F en profundidad, no bloquea el trabajo actual.
