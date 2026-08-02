# Handoff de sesión

## Contexto

- Fecha: 2026-08-02
- Agente/persona: Codex (bootstrap local)
- Rama: `feat/m0-bootstrap`
- Issue: M0-00 — instalar starter, crear baseline Git local y preflight agentic
- Commit de implementación: `4441876` (`chore: harden agentic m0 bootstrap`)
- Baseline documental previo: `0298207` (`chore: bootstrap ultimo barrio`), etiquetado como `bootstrap-v0.0.0`

## Resultado

- Qué se completó: se copió el starter pack y se fijó el baseline `0298207`/`bootstrap-v0.0.0`; además, `4441876` añadió un preflight M0 reproducible y corrigió el flujo paralelo para que solo el integrador consolide `STATE.md` y `CHANGELOG.md`.
- Qué no se completó: no se creó el proyecto real de s&box. No hay remoto Git configurado y, por tanto, no se publicó el repositorio. Tampoco existen archivos `*.sbproj`, escenas `*.scene` ni código bajo `Code/`.
- Estado de compilación: **NO EJECUTADA**. No existe un archivo `*.sbproj` que se pueda compilar.
- Estado de Play Mode: **NO EJECUTADO**. No hay proyecto ni escena que abrir en el editor.
- Prueba multijugador: **NO EJECUTADA**. No se iniciaron host ni segundo cliente.
- Prueba de guardado: **NO EJECUTADA**. No existe todavía una implementación de persistencia.

## Archivos principales

- Starter documental presente en la raíz del repositorio (`AGENTS.md`, `STATE.md`, `README.md`, `BACKLOG.md`, documentación, scripts y registro de assets).
- `scripts/check-m0-preflight.ps1`: diagnóstico de solo lectura para Git, proyecto, estructura, escena y MCP.
- `docs/handoffs/2026-08-02-m0-bootstrap.md`: este registro de reanudación.
- No existen todavía `*.sbproj`, `*.scene` ni archivos dentro de `Code/`.

## Decisiones

- Mantener `0298207` y `bootstrap-v0.0.0` como baseline local reproducible del starter.
- Continuar el trabajo de M0 en `feat/m0-bootstrap`; no trabajar directamente en `main`.
- No afirmar compatibilidad con s&box hasta instalarlo, crear el proyecto real y validar la escena mediante el editor/MCP.

## Bugs conocidos

- s&box terminó de instalarse correctamente desde Steam con `BuildID 24338653`, `SizeOnDisk 23090206734` y tres depots instalados. Se iniciaron `sbox-launcher` y `sbox`, pero todavía no se ha creado ni abierto el proyecto de Último Barrio.
- El MCP esperado en `127.0.0.1:7269` no está activo; no existe un proyecto abierto en el editor que exponga ese endpoint.
- El repositorio no tiene remoto configurado (`git remote -v` no devuelve entradas).
- Al faltar `*.sbproj`, `*.scene` y `Code/`, no se puede validar compilación, Play Mode ni multijugador.

## Próximas tres acciones

1. En el launcher de s&box, crear `Game → Empty` con identificador `ultimo_barrio`, la organización real y una carpeta vacía temporal como `C:\Users\davyd\AppData\Local\Temp\ultimo-barrio-sbox-seed`. No usar directamente la raíz del repo, porque la plantilla oficial intentaría copiar `.editorconfig` sobre el archivo del starter.
2. Inspeccionar la salida generada y fusionar en esta rama el `.sbproj`, `Code/`, `Editor/` y los archivos nuevos de `Assets/` sin sobrescribir a ciegas `.editorconfig` ni `Assets/asset-registry.yml`. Abrir el `.sbproj` resultante y ejecutar `scripts/check-m0-preflight.ps1`.
3. Con el proyecto abierto, confirmar el MCP en `127.0.0.1:7269`, inspeccionar la escena real y abordar únicamente M0-03: crear/guardar `main.scene` con los cuatro roots exigidos. Compilar, entrar en Play Mode, leer la consola y registrar evidencia antes de pasar al jugador o al segundo cliente.

## Cómo reproducir

1. Abrir PowerShell en la raíz del repositorio y ejecutar `git branch --show-current`, `git rev-parse --short bootstrap-v0.0.0` y `git show-ref --verify refs/tags/bootstrap-v0.0.0`. La rama debe ser `feat/m0-bootstrap` y el tag debe resolver a `0298207`, independientemente de que `HEAD` avance al integrar esta sesión.
2. Ejecutar `git remote -v` y `rg --files -g '*.sbproj' -g '*.scene' -g 'Code/**'`. En esta sesión ambos comandos no produjeron entradas.
3. Ejecutar `Get-NetTCPConnection -LocalAddress 127.0.0.1 -LocalPort 7269 -State Listen -ErrorAction SilentlyContinue`. En esta sesión no devolvió ningún listener. Para no exponer identificadores de cuenta, consultar el manifiesto con `Select-String -Path 'C:\Program Files (x86)\Steam\steamapps\appmanifest_590830.acf' -Pattern '"(StateFlags|SizeOnDisk|buildid|TargetBuildID|BytesToDownload|BytesDownloaded|BytesToStage|BytesStaged|InstalledDepots)"'` en lugar de volcarlo completo. El estado final mostró `buildid 24338653`, `SizeOnDisk 23090206734`, descarga completa y tres depots instalados.

## Evidencias

- Captura: no disponible; Play Mode no se ejecutó.
- Log: no hay log de compilación ni de consola del proyecto porque el proyecto real aún no existe. El arranque del menú de s&box quedó fuera de la validación M0.
- Métrica: 0 compilaciones, 0 sesiones de Play Mode y 0 pruebas multijugador ejecutadas durante esta sesión.
