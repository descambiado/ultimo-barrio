# Último Barrio

> De día mantienes el barrio vivo. De noche proteges tu casa.

**Último Barrio** es un survival urbano persistente para s&box, jugable en solitario y en cooperativo. Cada jugador conserva su personaje, apartamento, alijo, relaciones y mejoras. Durante el día comercia, trabaja, oculta recursos y prepara defensas bajo vigilancia. Durante la noche, grupos hostiles intentan registrar, saquear, capturar vecinos o tomar infraestructuras.

El proyecto utiliza una **ciudad fronteriza mediterránea ficticia**. Puede inspirarse en arquitectura, resiliencia civil y economías clandestinas reales, pero no convierte nacionalidades, etnias o religiones existentes en facciones enemigas.

## Estado

**Pre-alpha / diseño y bootstrap técnico.**

El primer objetivo no es una ciudad completa. Es una manzana pequeña e inolvidable que demuestre todos los pilares:

- Un apartamento persistente.
- Un ciclo día, preparación, noche y amanecer.
- Comercio legal y clandestino.
- Civiles autónomos.
- Un asalto nocturno con objetivos físicos.
- Consecuencias persistentes sin borrar horas de progreso.
- Juego funcional con una sola persona.
- Cooperativo drop-in para 1–4 jugadores.

## Principios

1. **La casa importa.** Las mejoras deben existir físicamente, no solo como números.
2. **El enemigo tiene objetivos.** Roba, registra, rompe, captura y se retira; no corre siempre hasta morir.
3. **Solo primero, cooperativo siempre.** Los NPC ocupan los huecos que no cubren jugadores.
4. **Consecuencias, no wipes.** Perder una noche cambia el barrio, pero no elimina la cuenta.
5. **Sistemas antes que contenido.** Pocas piezas que interactúan producen más historias que cien objetos aislados.
6. **Autoridad del host.** Dinero, daño, botín, guardado, IA y construcción se validan en servidor.
7. **Dependencias controladas.** Todo asset o librería queda registrado con autor, origen, versión y licencia.
8. **Proyecto reanudable.** Cada sesión termina actualizando `STATE.md`.

## Primera versión jugable

Duración prevista de una sesión: **18–22 minutos**.

- 1 manzana urbana.
- 1 apartamento reclamable por jugador.
- 2–4 hogares civiles controlados por IA.
- 1 comerciante.
- 1 patrulla diurna.
- 1 encargo clandestino.
- 1 asalto nocturno.
- 3 roles hostiles: explorador, asaltante y saqueador.
- Puertas y ventanas dañables.
- Escondite persistente.
- 5 mejoras domésticas.
- Guardado local versionado.
- 1–4 jugadores.

Consulta:

- [`START_HERE.md`](START_HERE.md)
- [`STATE.md`](STATE.md)
- [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- [`docs/WEEK_ONE_PLAN.md`](docs/WEEK_ONE_PLAN.md)
- [`CLAUDE.md`](CLAUDE.md)
- [`AGENTS.md`](AGENTS.md)

## Inicio rápido

1. Instala s&box y el editor desde Steam.
2. Crea un proyecto **Game → Empty** llamado `ultimo_barrio`.
3. Copia este starter pack a la raíz del proyecto.
4. Inicializa Git y crea el primer commit.
5. Abre el proyecto en el editor.
6. Activa o verifica el MCP del editor.
7. Entrega a Claude Code, ChatGPT/Codex o Cursor el archivo `prompts/PROJECT_BOOTSTRAP_PROMPT.md`.
8. Implementa únicamente el hito activo de `STATE.md`.

## Dependencias de armas

Estrategia inicial:

- **Lógica:** componentes oficiales de inventario y `BaseCombatWeapon`.
- **Visuales:** colección oficial `facepunch/sboxweapons`.
- **OmniParadigm Weapons:** candidato opcional sujeto a evaluación de fuente, licencia, autoridad de red, mantenimiento y dependencias.
- Nunca bloquear el núcleo del juego detrás de un paquete no verificado.

## Open source

Código original bajo **Mozilla Public License 2.0**. La intención es que las modificaciones distribuidas de los archivos cubiertos permanezcan abiertas, sin impedir módulos independientes con otras licencias.

Los assets de terceros conservan sus propias licencias y **no quedan relicenciados** por este repositorio. Consulta `THIRD_PARTY_NOTICES.md` y `Assets/asset-registry.yml`.

## Contribuir

Lee [`CONTRIBUTING.md`](CONTRIBUTING.md). Cada PR debe:

- Resolver una tarea concreta.
- Mantener el proyecto compilando.
- Incluir pasos de prueba.
- Actualizar documentación y estado cuando corresponda.
- Registrar cualquier nueva dependencia.
- No introducir contenido extraído de juegos comerciales o Garry's Mod sin permiso verificable.

## Nombre

`Último Barrio` es un nombre de trabajo. La arquitectura, documentación y organización no dependen del nombre comercial definitivo.
