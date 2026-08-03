# GUÍA DE VALIDACIÓN MANUAL — Alpha 0.1
# Último Barrio — feat/alpha-0.1-integration
# Fecha: 2026-08-03

## PREPARACIÓN

1. Abre s&box
2. Carga la escena: `Assets/scenes/main.scene`
3. Asegúrate de que el proyecto compila en s&box (ya compila con dotnet build: 0 errores / 0 warnings)
4. El jugador tiene QACommandsPanel en el prefab (añadirlo manualmente en el editor si no aparece)

## GATE 1 — UN JUGADOR

### Test G1-A: Spawn y HUD
**Acción**: Pulsa Play → Entra como host
**Esperado en consola**: 
  - `UB.Apartment ServiceReady apartments=2`
**Esperado en pantalla**:
  - HUD vacío (sin cartel de interacción hasta que mires algo)
  - TAB abre el inventario (panel "Tu Mochila")
  - TAB de nuevo lo cierra

### Test G1-B: Reclamar A01
**Acción**: Acércate al "Claim Portal" de apartment-a01 (es el cubo con ApartmentClaimInteractable)
**Esperado en pantalla**: 
  - Aparece "Este piso está disponible | Pulsa E para reclamarlo"
**Acción**: Pulsa E
**Esperado en consola**:
  - `[Interact] Type: ApartmentClaimInteractable, Prompt: Este piso está disponible, CanInteract: True`
  - `[Interact] Sending RPC to Host...`
  - `UB.Apartment ClaimSucceeded apartment=apartment-a01`

### Test G1-C: No reclamar A02 (mismo jugador)
**Acción**: Ve al Claim Portal de apartment-a02, pulsa E
**Esperado en consola**:
  - `UB.Apartment ClaimRejected apartment=apartment-a02 reason=PlayerAlreadyOwnsApartment`

### Test G1-D: Recoger chatarra
**Acción**: Acércate al objeto "Chatarra A02" (posición 150,150,50), pulsa E
**Esperado en pantalla**:
  - Aparece "Recoger chatarra (x5) | Pulsa E"
**Acción**: Pulsa E
**Esperado**: Objeto desaparece, chatarra en inventario
**Verificar con F5** (QA dump): `[chatarra] x5`

### Test G1-E: Abrir alijo (stash)
**Acción**: Acércate al cubo verde en apartment-a01 (Stash Anchor), pulsa E
**Esperado en pantalla**:
  - "Pulsa E para abrir el alijo"
**Acción**: Pulsa E
**Esperado**: Se abren dos paneles (Tu Mochila izquierda, Alijo derecha)
**Acción**: Transfiere chatarra del inventario al alijo haciendo click en los items

### Test G1-F: Distancia del alijo
**Acción**: Con el alijo abierto, aléjate > 200 unidades
**Esperado**: El panel del alijo se cierra automáticamente

### Test G1-G: TAB cierra inventario
**Acción**: Abre inventario con TAB, pulsa TAB de nuevo
**Esperado**: Se cierra

## GATE 2 — DOS JUGADORES (necesitas otro cliente)

### Test G2-A: Segundo cliente conecta
**Acción**: Abre segundo cliente, conéctate al host (IP local)
**Esperado**: El segundo jugador aparece en la escena

### Test G2-B: Segundo jugador no puede reclamar A01
**Acción**: Desde el segundo cliente, acércate a Claim Portal de A01
**Esperado en pantalla**: "Este piso ya tiene dueño"

### Test G2-C: Segundo jugador puede reclamar A02
**Acción**: Desde el segundo cliente, acércate a Claim Portal de A02, pulsa E
**Esperado**: `UB.Apartment ClaimSucceeded apartment=apartment-a02`

### Test G2-D: Segundo jugador no puede abrir el alijo de A01
**Acción**: Desde el segundo cliente, acércate al Stash de A01, pulsa E
**Esperado en pantalla**: "No puedes acceder a este apartamento"

## TECLAS DE DEBUG (QACommandsPanel)

| Tecla | Acción |
|-------|--------|
| F2    | +10 chatarra al inventario |
| F3    | +100 dinero al wallet |
| F4    | Fuerza fase Night en WorldClock |
| F5    | Dump inventario en consola |
| F6    | Dump estado de apartamentos en consola |

## QUÉ INFORMAR

Cuando hagas cada test, dime:
1. ✅ Pasó / ❌ Falló / ⚠️ Parcial
2. Copia el texto de consola si hay error
3. Screenshot si el bug es visual

El integrador (yo) aplico el fix inmediatamente y te pido que repitas.
