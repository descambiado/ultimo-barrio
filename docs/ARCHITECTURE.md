# Arquitectura técnica

## Objetivo

Construir una base que permita añadir sistemas semanalmente sin acoplar:

- Armas.
- Apartamentos.
- Economía.
- IA.
- Persistencia.
- Networking.
- UI.
- Assets.

## Principios

1. Host autoritativo.
2. Composición sobre jerarquías profundas.
3. Definiciones data-driven.
4. Interfaces en límites reemplazables.
5. Guardados versionados.
6. Simulación por niveles de detalle.
7. Dependencias externas encapsuladas.
8. Métricas y logs desde el principio.
9. Features verticales pequeñas.
10. Estado reanudable documentado.

## Capas

```text
Presentation
    UI, audio, viewmodels, feedback
Application
    casos de uso, flujo de fases, comandos
Domain
    apartamento, inventario, sospecha, relaciones, raid
Infrastructure
    s&box, red, filesystem, assets, servicios externos
```

El dominio no debe depender de un arma concreta ni de un mapa concreto.

## Estructura propuesta

```text
Code/UltimoBarrio/
├── Core/
│   ├── GamePhase.cs
│   ├── WorldClock.cs
│   ├── DistrictState.cs
│   └── Result.cs
├── Players/
├── Apartments/
├── Inventory/
├── Economy/
├── Suspicion/
├── Civilians/
├── Raids/
├── Combat/
├── Fortifications/
├── Persistence/
├── Networking/
├── UI/
└── Diagnostics/

Assets/
├── scenes/
├── prefabs/
├── data/
├── ui/
├── audio/
└── asset-registry.yml
```

## Límites reemplazables

```csharp
public interface IPersistenceProvider
{
	Task<SaveSnapshot?> LoadAsync( string profileId, CancellationToken cancellationToken );
	Task SaveAsync( SaveSnapshot snapshot, CancellationToken cancellationToken );
}

public interface IRaidDirector
{
	RaidPlan CreatePlan( RaidContext context );
}

public interface ITimeSource
{
	double Now { get; }
}

public interface IWeaponAdapter
{
	string WeaponId { get; }
	bool CanUse( WeaponUseContext context );
}
```

Las firmas exactas se adaptarán al proyecto real; el propósito es evitar dependencias directas entre sistemas.

## Autoridad de red

El host decide:

- Propiedad de apartamentos.
- Inventario persistente.
- Comercio.
- Recompensas.
- Sospecha.
- Daño final.
- Muerte.
- Colocación.
- Objetivos IA.
- Botín.
- Cambio de fase.
- Guardado.

El cliente solicita una intención:

```text
RequestTrade
RequestInteract
RequestPlaceFortification
RequestUseWeapon
RequestMoveItem
```

El host valida y replica el resultado.

## Estados sincronizados

Sincronizar únicamente lo necesario:

- Fase.
- Tiempo restante.
- Estado visible de puertas y ventanas.
- Vida.
- Objetivo público del raid.
- Inventario equipado.
- Feedback de interacción.

No sincronizar:

- Datos completos de guardado.
- Inventarios ajenos no visibles.
- Memoria interna completa de IA.
- Cálculos que el cliente pueda derivar de un estado estable.

## Persistencia

### Versión inicial

`LocalPersistenceProvider` sobre almacenamiento permitido por s&box.

### Futuro

`ApiPersistenceProvider` para servidor dedicado.

### Save snapshot

```text
SaveVersion
Profile
Apartment
Inventory
Relationships
DistrictProjects
WorldConsequences
Timestamp
Checksum/validation metadata
```

### Migraciones

Cada cambio de esquema incrementa `SaveVersion`.

```text
v1 → v2 → v3
```

Nunca modificar silenciosamente un snapshot sin una migración.

## Simulación LOD

### Full

Cerca del jugador:

- GameObject.
- Animación.
- NavMesh.
- Percepción.
- Combate.
- Física necesaria.

### Simplified

En la manzana pero fuera de interés:

- Posición aproximada.
- Destino.
- Acción.
- Salud.
- Inventario resumido.
- Tick reducido.

### Abstract

Fuera del área activa:

- Estado lógico.
- Evento programado.
- Recursos.
- Resultado probabilístico determinista.

## IA

Separar:

- Perception.
- Memory.
- Utility scoring.
- Navigation.
- Action execution.
- Squad coordination.
- Raid planning.

El director decide **qué quiere conseguir el grupo**. Cada agente decide **cómo ejecutar su parte**.

## Armas

### Camino base

- `BaseCombatWeapon` para mecánica.
- Assets Facepunch para viewmodel/worldmodel.
- Adaptador propio para que inventario, persistencia y daño no dependan del prefab.

### OmniParadigm

Debe pasar un spike:

- Fuente disponible.
- Licencia clara.
- Tipo de paquete.
- Dependencias.
- Autoridad de red.
- Compatibilidad con inventario.
- Daño extensible.
- Hooks de animación.
- Rendimiento.
- Mantenimiento.
- Facilidad de sustitución.

## Presupuestos Alpha

Valores iniciales, sujetos a profiling:

- 1–4 jugadores.
- 8 hostiles activos.
- 4–8 civiles activos.
- 30 fortificaciones globales.
- 1 objeto físico transportado por jugador.
- 1 manzana.
- Sin tráfico.
- Sin destrucción universal.
- Sin más de una simulación de raid activa.

## Observabilidad

Añadir categorías de log:

```text
UB.GameFlow
UB.Network
UB.Save
UB.Apartment
UB.AI
UB.Raid
UB.Economy
UB.Asset
```

Cada error debe indicar:

- Qué falló.
- Identificador.
- Estado relevante.
- Acción sugerida.

## Definition of Done técnica

- Compila.
- Consola limpia.
- Host authority revisada.
- Save compatibility revisada.
- Join-in-progress revisado si aplica.
- Prueba en solitario.
- Prueba con dos clientes.
- Perfilado básico.
- Docs y `STATE.md` actualizados.
