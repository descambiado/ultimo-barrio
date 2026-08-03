# Roadmap Alpha 0.1

## Objetivo de Ejecución
Construir una Alpha 0.1 jugable, estilizada y medible.

## Loop Jugable Objetivo
1. Entrar al barrio
2. Reclamar vivienda
3. Recoger recursos
4. Usar inventario y stash
5. Vender chatarra
6. Comprar recursos
7. Conseguir una pistola
8. Pasar de día a noche
9. Recibir un ataque
10. Defender la vivienda
11. Guardar el progreso

## Carriles de Desarrollo Paralelo
- **Carril A**: Foundation y QA
- **Carril B**: Viviendas, inventario y stash
- **Carril C**: Economía
- **Carril D**: Combate
- **Carril E**: IA y raid
- **Carril F**: Mundo, UI y estilo

## Reglas de Integración
- Integración secuencial con verificación de cada paso.
- El integrador revisa interfaces y ownership antes del merge.
- Ningún subagente modifica main.scene.
