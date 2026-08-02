<#
.SYNOPSIS
Runs a read-only M0 readiness check from the repository root.

.PARAMETER McpTimeoutMilliseconds
Maximum time to wait for the local s&box MCP TCP endpoint.
#>
[CmdletBinding()]
param(
  [ValidateRange(100, 5000)]
  [int]$McpTimeoutMilliseconds = 500
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath "..")).Path
$results = @()

function Add-CheckResult {
  param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("PASS", "PENDING", "FAIL")]
    [string]$Status,

    [Parameter(Mandatory = $true)]
    [string]$Name,

    [Parameter(Mandatory = $true)]
    [string]$Detail,

    [string]$Action = ""
  )

  $script:results += [pscustomobject]@{
    Status = $Status
    Name = $Name
    Detail = $Detail
    Action = $Action
  }
}

function Invoke-GitReadOnly {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$ArgumentList
  )

  $previousErrorActionPreference = $ErrorActionPreference

  try {
    $ErrorActionPreference = "SilentlyContinue"
    $output = @(& $script:gitExecutable -C $script:repoRoot @ArgumentList 2>$null)
    $exitCode = $LASTEXITCODE
  }
  finally {
    $ErrorActionPreference = $previousErrorActionPreference
  }

  return [pscustomobject]@{
    ExitCode = $exitCode
    Output = (($output | ForEach-Object { "$_" }) -join [Environment]::NewLine).Trim()
  }
}

function Test-TcpEndpoint {
  param(
    [Parameter(Mandatory = $true)]
    [string]$HostName,

    [Parameter(Mandatory = $true)]
    [int]$Port,

    [Parameter(Mandatory = $true)]
    [int]$TimeoutMilliseconds
  )

  $client = New-Object System.Net.Sockets.TcpClient
  $waitHandle = $null

  try {
    $asyncResult = $client.BeginConnect($HostName, $Port, $null, $null)
    $waitHandle = $asyncResult.AsyncWaitHandle

    if (-not $waitHandle.WaitOne($TimeoutMilliseconds, $false)) {
      return $false
    }

    $client.EndConnect($asyncResult)
    return $client.Connected
  }
  catch {
    return $false
  }
  finally {
    if ($null -ne $waitHandle) {
      $waitHandle.Close()
    }

    $client.Close()
  }
}

$gitCommand = Get-Command -Name "git" -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
$isExpectedGitRoot = $false

if ($null -eq $gitCommand) {
  Add-CheckResult -Status "FAIL" -Name "Repositorio Git" -Detail "git no esta disponible." -Action "Instala Git y vuelve a ejecutar el preflight."
}
else {
  $gitExecutable = $gitCommand.Source
  $rootResult = Invoke-GitReadOnly -ArgumentList @("rev-parse", "--show-toplevel")

  if ($rootResult.ExitCode -ne 0 -or [string]::IsNullOrWhiteSpace($rootResult.Output)) {
    Add-CheckResult -Status "FAIL" -Name "Repositorio Git" -Detail "La raiz calculada no pertenece a un repositorio Git." -Action "Inicializa o recupera el repositorio en $repoRoot."
  }
  else {
    $pathSeparators = [char[]]@([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $gitRoot = [System.IO.Path]::GetFullPath($rootResult.Output).TrimEnd($pathSeparators)
    $expectedRoot = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd($pathSeparators)

    if ($gitRoot -ieq $expectedRoot) {
      $isExpectedGitRoot = $true
      Add-CheckResult -Status "PASS" -Name "Repositorio Git" -Detail "La raiz Git coincide con $repoRoot."
    }
    else {
      Add-CheckResult -Status "FAIL" -Name "Repositorio Git" -Detail "La raiz Git detectada es $gitRoot." -Action "Ejecuta este script desde el checkout del proyecto, no desde un subdirectorio de otro repositorio."
    }
  }
}

if ($isExpectedGitRoot) {
  $branchResult = Invoke-GitReadOnly -ArgumentList @("symbolic-ref", "--short", "-q", "HEAD")

  if ($branchResult.ExitCode -ne 0 -or [string]::IsNullOrWhiteSpace($branchResult.Output)) {
    Add-CheckResult -Status "FAIL" -Name "Rama Git" -Detail "HEAD esta separado (detached) o la rama no se pudo determinar." -Action "Cambia a una rama de trabajo antes de continuar."
  }
  elseif ($branchResult.Output -eq "main") {
    Add-CheckResult -Status "FAIL" -Name "Rama Git" -Detail "La rama actual es main; el trabajo directo en main esta prohibido." -Action "Crea o cambia a una rama de tarea."
  }
  else {
    Add-CheckResult -Status "PASS" -Name "Rama Git" -Detail "Rama de trabajo: $($branchResult.Output)."
  }

  $worktreeResult = Invoke-GitReadOnly -ArgumentList @("status", "--porcelain")

  if ($worktreeResult.ExitCode -ne 0) {
    Add-CheckResult -Status "FAIL" -Name "Arbol de trabajo" -Detail "No se pudo leer el estado del repositorio." -Action "Corrige Git antes de continuar."
  }
  elseif ([string]::IsNullOrWhiteSpace($worktreeResult.Output)) {
    Add-CheckResult -Status "PASS" -Name "Arbol de trabajo" -Detail "El arbol Git esta limpio."
  }
  else {
    Add-CheckResult -Status "PENDING" -Name "Arbol de trabajo" -Detail "Hay cambios locales sin consolidar." -Action "Revisa, valida y agrupa los cambios reales en un commit antes del handoff."
  }

  $bootstrapTagResult = Invoke-GitReadOnly -ArgumentList @("show-ref", "--verify", "--quiet", "refs/tags/bootstrap-v0.0.0")

  if ($bootstrapTagResult.ExitCode -eq 0) {
    Add-CheckResult -Status "PASS" -Name "Tag bootstrap local" -Detail "bootstrap-v0.0.0 existe localmente."
  }
  else {
    Add-CheckResult -Status "FAIL" -Name "Tag bootstrap local" -Detail "bootstrap-v0.0.0 no existe localmente." -Action "Recupera el tag original sin reescribirlo."
  }

  $originResult = Invoke-GitReadOnly -ArgumentList @("remote", "get-url", "origin")

  if ($originResult.ExitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($originResult.Output)) {
    Add-CheckResult -Status "PASS" -Name "Remoto origin" -Detail "El remoto origin esta configurado."

    $requiredRemoteRefs = @(
      [pscustomobject]@{ Name = "Rama remota main"; Ref = "refs/heads/main" },
      [pscustomobject]@{ Name = "Rama remota feat/m0-bootstrap"; Ref = "refs/heads/feat/m0-bootstrap" },
      [pscustomobject]@{ Name = "Tag bootstrap remoto"; Ref = "refs/tags/bootstrap-v0.0.0" }
    )

    foreach ($requiredRemoteRef in $requiredRemoteRefs) {
      $remoteRefResult = Invoke-GitReadOnly -ArgumentList @("ls-remote", "--exit-code", "origin", $requiredRemoteRef.Ref)

      if ($remoteRefResult.ExitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($remoteRefResult.Output)) {
        Add-CheckResult -Status "PASS" -Name $requiredRemoteRef.Name -Detail "$($requiredRemoteRef.Ref) existe en origin."
      }
      else {
        Add-CheckResult -Status "PENDING" -Name $requiredRemoteRef.Name -Detail "$($requiredRemoteRef.Ref) no se pudo verificar en origin." -Action "Publica la referencia sin force-push y repite el preflight."
      }
    }
  }
  else {
    Add-CheckResult -Status "PENDING" -Name "Remoto origin" -Detail "No hay un remoto origin configurado." -Action "Configura origin con la URL del repositorio del proyecto."

    foreach ($pendingRemoteRef in @("refs/heads/main", "refs/heads/feat/m0-bootstrap", "refs/tags/bootstrap-v0.0.0")) {
      Add-CheckResult -Status "PENDING" -Name "Referencia remota $pendingRemoteRef" -Detail "No se puede verificar sin origin." -Action "Crea el repositorio, configura origin y publica las referencias."
    }
  }
}
else {
  Add-CheckResult -Status "PENDING" -Name "Rama Git" -Detail "No se puede validar la rama hasta corregir la raiz Git." -Action "Corrige primero el chequeo Repositorio Git."
  Add-CheckResult -Status "PENDING" -Name "Arbol de trabajo" -Detail "No se puede validar el arbol hasta corregir la raiz Git." -Action "Corrige primero el chequeo Repositorio Git."
  Add-CheckResult -Status "PENDING" -Name "Tag bootstrap local" -Detail "No se puede validar el tag hasta corregir la raiz Git." -Action "Corrige primero el chequeo Repositorio Git."
  Add-CheckResult -Status "PENDING" -Name "Remoto origin" -Detail "No se puede validar origin hasta corregir la raiz Git." -Action "Corrige primero el chequeo Repositorio Git."
}

$licensePath = Join-Path -Path $repoRoot -ChildPath "LICENSE"

if (-not (Test-Path -LiteralPath $licensePath -PathType Leaf)) {
  Add-CheckResult -Status "FAIL" -Name "Licencia MPL-2.0" -Detail "LICENSE no existe." -Action "Recupera el archivo MPL-2.0 versionado sin alterar el historial."
}
else {
  $licenseText = Get-Content -LiteralPath $licensePath -Raw -ErrorAction Stop

  if ($licenseText -match "Mozilla Public License Version 2\.0") {
    Add-CheckResult -Status "PASS" -Name "Licencia MPL-2.0" -Detail "LICENSE contiene Mozilla Public License Version 2.0."
  }
  else {
    Add-CheckResult -Status "FAIL" -Name "Licencia MPL-2.0" -Detail "LICENSE no contiene el texto esperado de MPL-2.0." -Action "Restaura la licencia acordada sin reescribir tags ni commits."
  }
}

if ($isExpectedGitRoot) {
  $agplReferenceResult = Invoke-GitReadOnly -ArgumentList @(
    "grep", "-n", "-i", "-E", "AGPL|GNU Affero", "--",
    "*.md", "*.yml", "*.yaml", "*.json", "*.cs", "*.csproj", "*.props", "*.targets", "*.sbproj", "*.txt"
  )

  if ($agplReferenceResult.ExitCode -eq 1) {
    Add-CheckResult -Status "PASS" -Name "Referencias de licencia" -Detail "No hay referencias AGPL en documentacion, manifiestos o codigo versionados."
  }
  elseif ($agplReferenceResult.ExitCode -eq 0) {
    Add-CheckResult -Status "FAIL" -Name "Referencias de licencia" -Detail "Hay referencias AGPL en documentacion, manifiestos o codigo versionados." -Action "Corrige esas referencias a Mozilla Public License 2.0 y SPDX MPL-2.0."
  }
  else {
    Add-CheckResult -Status "FAIL" -Name "Referencias de licencia" -Detail "No se pudo buscar referencias AGPL con git grep." -Action "Revisa Git y repite el preflight."
  }
}

try {
  $projectFiles = @(Get-ChildItem -LiteralPath $repoRoot -Filter "*.sbproj" -File -ErrorAction Stop)

  if ($projectFiles.Count -eq 1) {
    Add-CheckResult -Status "PASS" -Name "Proyecto s&box" -Detail "Proyecto raiz: $($projectFiles[0].Name)."

    try {
      $projectManifest = Get-Content -LiteralPath $projectFiles[0].FullName -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop

      if ($projectManifest.Metadata.StartupScene -eq "scenes/main.scene") {
        Add-CheckResult -Status "PASS" -Name "Escena de inicio" -Detail "StartupScene apunta a scenes/main.scene."
      }
      else {
        Add-CheckResult -Status "PENDING" -Name "Escena de inicio" -Detail "StartupScene apunta a '$($projectManifest.Metadata.StartupScene)'." -Action "Configura scenes/main.scene como escena de inicio y valida el boot."
      }
    }
    catch {
      Add-CheckResult -Status "FAIL" -Name "Escena de inicio" -Detail "No se pudo leer el manifiesto del proyecto: $($_.Exception.Message)" -Action "Corrige el JSON del .sbproj antes de continuar."
    }
  }
  elseif ($projectFiles.Count -eq 0) {
    Add-CheckResult -Status "PENDING" -Name "Proyecto s&box" -Detail "No existe ningun .sbproj en la raiz." -Action "Crea Game - Empty en una carpeta vacia y fusiona los archivos generados aqui sin sobrescribir el starter."
  }
  else {
    Add-CheckResult -Status "FAIL" -Name "Proyecto s&box" -Detail "Hay $($projectFiles.Count) archivos .sbproj en la raiz: $($projectFiles.Name -join ', ')." -Action "Conserva exactamente un proyecto .sbproj en la raiz."
  }
}
catch {
  Add-CheckResult -Status "FAIL" -Name "Proyecto s&box" -Detail "No se pudo enumerar la raiz: $($_.Exception.Message)" -Action "Revisa los permisos del checkout."
}

foreach ($directoryName in @("Assets", "Code")) {
  $directoryPath = Join-Path -Path $repoRoot -ChildPath $directoryName

  if (Test-Path -LiteralPath $directoryPath -PathType Container) {
    Add-CheckResult -Status "PASS" -Name "Directorio $directoryName" -Detail "$directoryName/ existe."
  }
  elseif (Test-Path -LiteralPath $directoryPath) {
    Add-CheckResult -Status "FAIL" -Name "Directorio $directoryName" -Detail "$directoryName existe, pero no es un directorio." -Action "Corrige la colision de ruta y crea $directoryName/."
  }
  else {
    Add-CheckResult -Status "PENDING" -Name "Directorio $directoryName" -Detail "$directoryName/ no existe." -Action "Crea o copia el directorio $directoryName/ del proyecto s&box."
  }
}

$librariesPath = Join-Path -Path $repoRoot -ChildPath "Libraries"

if (Test-Path -LiteralPath $librariesPath -PathType Container) {
  Add-CheckResult -Status "PASS" -Name "Directorio Libraries" -Detail "Libraries/ existe y debe permanecer versionado."
}
elseif (Test-Path -LiteralPath $librariesPath) {
  Add-CheckResult -Status "FAIL" -Name "Directorio Libraries" -Detail "Libraries existe, pero no es un directorio." -Action "Corrige la colision antes de instalar librerias."
}
else {
  Add-CheckResult -Status "PASS" -Name "Directorio Libraries" -Detail "No hay librerias instaladas; Game - Empty no crea Libraries/ hasta que se necesita."
}

$scenePath = Join-Path -Path $repoRoot -ChildPath "Assets\scenes\main.scene"

if (Test-Path -LiteralPath $scenePath -PathType Leaf) {
  Add-CheckResult -Status "PASS" -Name "Escena principal" -Detail "Assets/scenes/main.scene existe."
}
elseif (Test-Path -LiteralPath $scenePath) {
  Add-CheckResult -Status "FAIL" -Name "Escena principal" -Detail "Assets/scenes/main.scene existe, pero no es un archivo." -Action "Guarda main.scene como archivo en Assets/scenes/."
}
else {
  Add-CheckResult -Status "PENDING" -Name "Escena principal" -Detail "Assets/scenes/main.scene no existe." -Action "Crea y guarda la escena principal desde el editor s&box."
}

if (Test-TcpEndpoint -HostName "127.0.0.1" -Port 7269 -TimeoutMilliseconds $McpTimeoutMilliseconds) {
  Add-CheckResult -Status "PASS" -Name "MCP TCP" -Detail "127.0.0.1:7269 acepta conexiones TCP."
}
else {
  Add-CheckResult -Status "PENDING" -Name "MCP TCP" -Detail "127.0.0.1:7269 no respondio en $McpTimeoutMilliseconds ms." -Action "Abre s&box, activa el MCP y vuelve a ejecutar el preflight."
}

Write-Output "M0 preflight (solo lectura)"
Write-Output "Raiz: $repoRoot"
Write-Output ""

foreach ($result in $results) {
  Write-Output ("[{0}] {1}: {2}" -f $result.Status, $result.Name, $result.Detail)

  if (-not [string]::IsNullOrWhiteSpace($result.Action)) {
    Write-Output ("       Accion: {0}" -f $result.Action)
  }
}

$passCount = @($results | Where-Object { $_.Status -eq "PASS" }).Count
$pendingCount = @($results | Where-Object { $_.Status -eq "PENDING" }).Count
$failCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count

Write-Output ""
Write-Output ("Resumen: PASS={0} PENDING={1} FAIL={2}" -f $passCount, $pendingCount, $failCount)

if ($pendingCount -eq 0 -and $failCount -eq 0) {
  Write-Output "READY: M0 tiene todos los prerrequisitos comprobables."
  exit 0
}

Write-Output "NOT READY: resuelve los elementos PENDING/FAIL y repite el preflight."
exit 1
