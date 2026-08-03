# Handoff: Economy and Trading (Agente F)

## Resumen
Se ha implementado la base para la economía (moneda provisional) y el comercio del juego mediante los componentes Wallet y Trader.

## Archivos modificados
- Creado: Code/UltimoBarrio/Economy/Wallet.cs (Componente de moneda)
- Creado: Code/UltimoBarrio/Trading/Trader.cs (Componente de comerciante)
- Creado: Code/UltimoBarrio/Persistence/PlayerEconomySaveData.cs (Clase de guardado)
- Creado: Assets/scenes/test_economy.scene (Escena de prueba clonando minimal.scene)
- Modificado: Code/UltimoBarrio/Persistence/SaveSnapshot.cs (Añadido lista de economía)
- Modificado: Code/UltimoBarrio/Persistence/LocalPersistenceProvider.cs (Añadida persistencia y validación)

## Cómo probar
1. Abre 	est_economy.scene en el editor s&box.
2. Añade los componentes Wallet, Trader e IInventoryOwner a GameObjects para simular un escenario.
3. Configura los precios en el inspector del Trader.
4. Utiliza llamadas directas o UI conectada a los métodos BuyItem("water"/"medicine"/"ammo") y SellItem("scrap") para observar el flujo monetario y de ítems.

## Resultado de compilación
El código asume dependencias correctas del entorno s&box. Al no disponer del engine build (csproj faltante o interno), la revisión sintáctica en C# confirma que se utilizan las interfaces preexistentes (ej. IInventoryOwner) correctamente.

## Riesgos
- Falta de UI conectada; los métodos de compra/venta asumen que se les llama desde un script o sistema de Interacción futuro.
- El deserializador de s&box JSON necesita tolerar que PlayersEconomy pudiese estar vacío en guardados antiguos (lo hemos seteado a [] por defecto).

## Trabajo pendiente
- Conectar Trader e IInteractable con la interfaz de usuario.
- Testear exhaustivamente la carga/guardado de PlayersEconomy con el LocalPersistenceProvider en entorno real de s&box.
