# Estrategia open source

## Objetivos

- Poder pausar y retomar.
- Atraer programadores y diseñadores.
- Convertir bugs en issues reproducibles.
- Evitar una base privada imposible de mantener.
- Crear reputación técnica alrededor del proyecto.
- Permitir servidores y forks sin perder mejoras importantes.

## Licencia propuesta

**MPL-2.0 para el código original.**

Motivo:

- Mantiene abiertas las modificaciones de los archivos cubiertos.
- Permite combinar el proyecto con módulos independientes bajo otras licencias.
- Permite estudiar, modificar y redistribuir.
- No relicencia assets externos.

Antes de una release comercial importante puede evaluarse:

- Mantener MPL.
- Dual licensing.
- Excepciones para partners.
- Separar SDK MIT de juego MPL.

## Gobernanza inicial

### Maintainer

Decide:

- Dirección.
- Releases.
- Seguridad.
- Marca.
- Merge.
- Licencias.

### Contributors

Trabajan mediante issues y PR.

### Trusted maintainers

Obtienen ownership por área después de contribuciones sostenidas.

## Transparencia

Publicar:

- Roadmap.
- Changelog.
- ADR.
- Métricas de rendimiento.
- Dependencias.
- Bugs conocidos.
- Issues para principiantes.

No publicar:

- Secretos.
- Exploits activos.
- Datos de jugadores.
- Claves.
- Moderación privada.

## Releases

```text
0.0.x bootstrap
0.1.0 first playable alpha
0.2.0 neighborhood persistence
0.3.0 adaptive raids
0.4.0 public multiplayer test
```

Cadencia aspiracional:

- Commits continuos.
- Build jugable semanal.
- Changelog por release.
- Vídeo corto por hito.
