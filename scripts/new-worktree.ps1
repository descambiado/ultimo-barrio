param(
  [Parameter(Mandatory = $true)]
  [string]$Branch,

  [Parameter(Mandatory = $true)]
  [string]$Path
)

$ErrorActionPreference = "Stop"

git fetch origin
git worktree add $Path -b $Branch origin/main

Write-Host "Created worktree $Path on $Branch"
