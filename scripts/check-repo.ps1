$ErrorActionPreference = "Stop"

$required = @(
  "README.md",
  "STATE.md",
  "CLAUDE.md",
  "AGENTS.md",
  "docs/GAME_DESIGN.md",
  "docs/ARCHITECTURE.md",
  "docs/ASSET_POLICY.md",
  "Assets/asset-registry.yml"
)

$missing = @()

foreach ($path in $required) {
  if (-not (Test-Path $path)) {
    $missing += $path
  }
}

if ($missing.Count -gt 0) {
  Write-Error ("Missing required files:`n- " + ($missing -join "`n- "))
}

Write-Host "Repository structure OK."
