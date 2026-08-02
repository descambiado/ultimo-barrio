# Game Design Document

## 1. High concept

**Último Barrio** es un survival urbano persistente donde el jugador intenta mantener su vida, vivienda y comunidad dentro de una ciudad fronteriza ficticia sometida a control y asaltos recurrentes.

Durante el día:

- Trabaja.
- Comercia.
- Oculta mercancía.
- Repara.
- Explora.
- Observa patrullas.
- Ayuda o traiciona vecinos.
- Prepara defensas.

Durante la noche:

- Defiende su apartamento.
- Ayuda a otros hogares.
- Protege infraestructuras.
- Oculta el alijo.
- Evacua.
- Resiste registros y saqueos.
- Decide qué está dispuesto a perder.

El juego debe producir historias del tipo:

> “Apagué el piso para que pasaran de largo, pero escuché al vecino pedir ayuda. Crucé por el balcón, perdí medicinas y salvé a su familia. A la mañana siguiente el taller me reparó la puerta gratis.”

## 2. Fantasía del jugador

El jugador no es un héroe militar predestinado. Es una persona del barrio que progresivamente se convierte en:

- Superviviente.
- Comerciante.
- Técnico.
- Médico.
- Vigilante.
- Contrabandista.
- Organizador comunitario.

La fantasía no es eliminar infinitos enemigos. Es **hacer que una vivienda vulnerable termine formando parte de una comunidad difícil de quebrar**.

## 3. Ambientación

- Ciudad mediterránea ficticia.
- Bloques de viviendas, balcones, patios, comercios pequeños y azoteas conectables.
- Arquitectura densa y vertical.
- Facciones inventadas.
- Conflicto tratado desde la supervivencia civil.
- Sin reconstruir literalmente una invasión real.
- Sin asociar enemistad a colectivos existentes.

## 4. Pilares

### 4.1 Hogar persistente

El apartamento conserva:

- Integridad.
- Puertas y ventanas.
- Alijo.
- Mobiliario funcional.
- Mejoras.
- Agua y energía.
- Daños.
- Objetos personales.
- Rutas hacia balcones o azoteas.

Las mejoras son visibles y ocupan espacio.

### 4.2 Economía clandestina física

Las transacciones deben generar situaciones:

- Esconder medicinas dentro de una caja.
- Cruzar un control.
- Transportar una batería pesada.
- Elegir entre un comprador rico y una clínica.
- Guardar stock fuera del inventario declarado.

### 4.3 Asaltos con propósito

Los grupos hostiles persiguen objetivos:

- Registrar.
- Robar.
- Capturar.
- Romper una infraestructura.
- Tomar una posición.
- Castigar actividad sospechosa.
- Retirarse con botín.

### 4.4 Barrio autónomo

Con un jugador, el barrio sigue funcionando mediante NPC. Al entrar personas, sustituyen hogares o roles simulados sin romper el ciclo.

### 4.5 Consecuencias recuperables

Una derrota puede:

- Dañar una mejora.
- Robar parte del alijo.
- Herir al personaje.
- Cerrar una tienda.
- Detener a un contacto.
- Ocupar temporalmente una planta.
- Aumentar precios.
- Bloquear una ruta.

No elimina permanentemente al personaje ni destruye todo el progreso.

## 5. Bucle principal

```text
Amanecer
→ evaluar daños y necesidades
→ trabajar/comerciar/explorar
→ aumentar o reducir sospecha
→ preparar apartamento y barrio
→ asalto nocturno
→ calcular pérdidas, relaciones y progreso
→ guardar
→ nuevo amanecer
```

Duración inicial:

- Amanecer: 1 minuto.
- Día: 8 minutos.
- Preparación: 2 minutos.
- Noche: 7 minutos.
- Resultado: 1 minuto.

Total: 19 minutos aproximadamente.

## 6. Apartamento

### Acciones básicas

- Abrir/cerrar.
- Bloquear.
- Reparar.
- Mover objetos permitidos.
- Guardar/retirar.
- Ocultar.
- Apagar luces.
- Observar por ventana.
- Disparar desde cobertura.
- Cruzar por balcón preparado.

### Mejoras Alpha

1. Puerta reforzada.
2. Persiana interior.
3. Compartimento oculto.
4. Depósito de agua.
5. Radio de avisos.

### Estado mínimo

```text
ApartmentId
OwnerId
Integrity
DoorState
WindowStates
PowerState
WaterState
Stash
InstalledUpgrades
LastRaidDamage
SaveVersion
```

## 7. Civiles

Cada hogar define:

- Residentes.
- Profesión.
- Recursos.
- Necesidades.
- Confianza.
- Miedo.
- Lealtad.
- Rutina.
- Capacidad defensiva.
- Relación con otros hogares.

### Primeros arquetipos

- Mecánico.
- Enfermera.
- Comerciante.
- Anciano observador.

### Comportamiento

Durante el día:

- Trabajar.
- Comprar.
- Transportar.
- Buscar.
- Compartir rumores.
- Esconderse ante patrullas.

Durante la noche:

- Atrincherarse.
- Defender.
- Curar.
- Pedir ayuda.
- Evacuar.
- Abandonar una posición.
- Proteger a otro residente.

## 8. IA hostil

### Roles Alpha

#### Explorador

- Observa.
- Detecta luces y accesos.
- Marca brechas.
- Evita combate directo.

#### Asaltante

- Presiona defensores.
- Cubre calles.
- Avanza hacia objetivos.

#### Saqueador

- Busca contenedores.
- Recoge botín.
- Prioriza escapar.

### Percepción

- Línea de visión.
- Sonidos.
- Luces.
- Puertas abiertas.
- Disparos recientes.
- Última posición conocida.
- Marcas de exploradores.

### Decisión

Sistema de utilidad:

```text
Score(action) =
  objective_relevance
+ urgency
+ personality
+ squad_order
- risk
- distance
- injury_penalty
```

No usar un LLM para comportamiento minuto a minuto.

## 9. Sospecha

La sospecha conecta día y noche.

Aumenta por:

- Ser visto con contrabando.
- Correr cerca de controles.
- Entrar en zonas restringidas.
- Disparos.
- Luces o actividad nocturna.
- Comercios denunciados.
- Repetir rutas.

Disminuye por:

- Trabajo legal.
- Ocultar pruebas.
- Sobornos o favores.
- Cambiar rutas.
- Mantener perfil bajo.

La sospecha modifica el objetivo nocturno y las patrullas futuras.

## 10. Solo y cooperativo

### Solitario

- NPC cubren funciones comunitarias.
- Asaltos se ajustan a capacidad real.
- El jugador puede ordenar tareas básicas a aliados.
- No se exige conectividad externa.

### Cooperativo

- 1–4 jugadores Alpha.
- Cada persona reclama vivienda.
- Drop-in durante el día o amanecer.
- Un jugador desconectado no pierde su casa.
- El host valida estado y guardado.
- Los roles humanos reemplazan funciones NPC cuando conviene.

## 11. Progresión

### Personaje

Desbloquea capacidades, no solo daño:

- Reparar más rápido.
- Detectar compartimentos.
- Negociar rutas.
- Curar heridas complejas.
- Fabricar mejores cierres.
- Leer patrones de patrulla.

### Vivienda

Pasa de refugio básico a:

- Taller.
- Clínica.
- Almacén.
- Puesto de radio.
- Mirador.
- Nodo de red de balcones.

### Barrio

Proyectos:

- Generador.
- Clínica.
- Cocina común.
- Taller.
- Red de azoteas.
- Mercado clandestino.
- Puesto de vigilancia.

## 12. Retención saludable

Motivos para volver:

- Ver cómo cambia la vivienda.
- Relaciones.
- Nuevos rumores.
- Proyectos del barrio.
- Objetivos semanales.
- Variaciones de asalto.
- Especialización.
- Historias emergentes.

No usar:

- Pérdidas destructivas por no conectarse.
- Castigos diarios obligatorios.
- Monetización pay-to-win.
- Energía artificial que limite jugar.

## 13. Alpha 0.1 — criterios

La Alpha demuestra el concepto cuando:

- El jugador reclama una vivienda.
- Guarda y recupera objetos tras reiniciar.
- Completa una entrega clandestina.
- Una patrulla puede detectar contrabando.
- Puede reforzar una puerta o ventana.
- Un grupo hostil selecciona un objetivo.
- Al menos un saqueador recoge un objeto y trata de escapar.
- El amanecer persiste daños y relaciones.
- Funciona con un jugador.
- Funciona con dos clientes.
- La sesión completa puede entenderse sin explicación externa.
