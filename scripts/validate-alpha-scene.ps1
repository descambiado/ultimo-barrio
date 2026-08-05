#!/usr/bin/env pwsh
# validate-alpha-scene.ps1
# Scene integrity validator for Ultimo Barrio Alpha
# Exit code != 0 means validation failed

param(
    [string[]]$Files = @(
        "Assets/scenes/ultimo_barrio_alpha.scene",
        "Assets/prefabs/player.prefab",
        "Assets/prefabs/weapons/ub_usp.prefab",
        "Assets/prefabs/items/pf_usp_pickup.prefab",
        "Assets/prefabs/items/pf_ammo_9mm_pickup.prefab"
    )
)

$ErrorActionPreference = "Continue"
$failCount = 0

function Fail {
    param([string]$msg)
    Write-Host "FAIL: $msg" -ForegroundColor Red
    $script:failCount++
}

function Pass {
    param([string]$msg)
    Write-Host "PASS: $msg" -ForegroundColor Green
}

function Warn {
    param([string]$msg)
    Write-Host "WARN: $msg" -ForegroundColor Yellow
}

Write-Host "=== Ultimo Barrio Alpha Scene Validator ===" -ForegroundColor Cyan

# --- 1. File existence ---
foreach ($f in $Files) {
    if (Test-Path $f) {
        Pass "File exists: $f"
    } else {
        Fail "File not found: $f"
    }
}

# --- 2. Load and parse the main alpha scene ---
$scenePath = "Assets/scenes/ultimo_barrio_alpha.scene"
$sceneRaw = Get-Content $scenePath -Raw -Encoding utf8
$sceneJson = $sceneRaw | ConvertFrom-Json

# --- 3. Banned dev models ---
$devModelPattern = 'models/dev/box\.vmdl'
$devMatches = [regex]::Matches($sceneRaw, $devModelPattern)
if ($devMatches.Count -gt 0) {
    Fail "Scene contains $($devMatches.Count) references to models/dev/box.vmdl"
} else {
    Pass "No dev/box.vmdl models in scene"
}

# Check all prefab files too
foreach ($f in $Files | Where-Object { $_ -like "*.prefab" }) {
    $content = Get-Content $f -Raw -Encoding utf8
    if ($content -match 'models/dev/box\.vmdl') {
        Fail "Prefab $f contains models/dev/box.vmdl"
    } else {
        Pass "No dev/box in prefab: $f"
    }
}

# --- 4. GUID duplicates in scene ---
$guidPattern = '"__guid":\s*"([^"]+)"'
$guids = [regex]::Matches($sceneRaw, $guidPattern) | ForEach-Object { $_.Groups[1].Value }
$dupGuids = $guids | Group-Object | Where-Object { $_.Count -gt 1 }
if ($dupGuids) {
    foreach ($d in $dupGuids) {
        Fail "Duplicate GUID found: $($d.Name) (x$($d.Count))"
    }
} else {
    Pass "No duplicate GUIDs in scene ($($guids.Count) total)"
}

# --- 5. ApartmentId duplicates ---
$aptIdPattern = '"ApartmentId":\s*"([^"]+)"'
$aptIds = [regex]::Matches($sceneRaw, $aptIdPattern) | ForEach-Object { $_.Groups[1].Value }
$aptIdGroups = $aptIds | Group-Object
$dupAptIds = $aptIdGroups | Where-Object { $_.Count -gt 2 } # allow 2: component + door policy
foreach ($d in $dupAptIds) {
    if ($d.Count -gt 4) {
        Fail "ApartmentId '$($d.Name)' appears $($d.Count) times (expected at most 4)"
    }
}
if (-not ($aptIdGroups | Where-Object { $_.Count -gt 4 })) {
    Pass "ApartmentId distribution looks normal ($($aptIds.Count) references, $($aptIdGroups.Count) unique)"
}

# --- 6. InventoryId duplicates ---
$invIdPattern = '"InventoryId":\s*"([^"]+)"'
$invIds = [regex]::Matches($sceneRaw, $invIdPattern) | ForEach-Object { $_.Groups[1].Value }
$dupInvIds = $invIds | Group-Object | Where-Object { $_.Count -gt 1 }
foreach ($d in $dupInvIds) {
    Fail "Duplicate InventoryId: '$($d.Name)'"
}
if (-not $dupInvIds) {
    Pass "No duplicate InventoryIds ($($invIds.Count) total)"
}

# --- 7. Missing model references (empty model) ---
$emptyModelPattern = '"Model":\s*""'
$emptyModels = [regex]::Matches($sceneRaw, $emptyModelPattern)
if ($emptyModels.Count -gt 0) {
    Fail "Scene contains $($emptyModels.Count) empty Model references"
} else {
    Pass "No empty Model references"
}

# --- 8. Previously-broken models now corrected ---
$brokenModels = @(
    "models/sbox_props/wooden_door/wooden_door.vmdl",
    "models/sbox_props/plastic_crate/plastic_crate.vmdl",
    "models/sbox_props/cash_register/cash_register.vmdl"
)
foreach ($bm in $brokenModels) {
    if ($sceneRaw -match [regex]::Escape($bm)) {
        Fail "Scene still references broken model: $bm"
    } else {
        Pass "Broken model removed from scene: $bm"
    }
}

# --- 9. USP prefab validation ---
$uspPrefab = Get-Content "Assets/prefabs/weapons/ub_usp.prefab" -Raw
if ($uspPrefab -match 'models/dev/box\.vmdl') {
    Fail "USP prefab still has dev/box.vmdl"
} else {
    Pass "USP prefab does not use dev/box.vmdl"
}
if ($uspPrefab -match '"__type":\s*"Sandbox.ModelRenderer"') {
    Pass "USP prefab has ModelRenderer"
} else {
    Fail "USP prefab missing ModelRenderer"
}
if ($uspPrefab -match '"__type":\s*"Sandbox.BoxCollider"') {
    Pass "USP prefab has BoxCollider"
} else {
    Fail "USP prefab missing BoxCollider"
}
if ($uspPrefab -match 'UltimoBarrio.Combat.UltimoBarrioWeaponAdapter') {
    Pass "USP prefab has WeaponAdapter"
} else {
    Fail "USP prefab missing WeaponAdapter"
}

# --- 10. Pickup prefab validation ---
$pickupFiles = @(
    @{ Path = "Assets/prefabs/items/pf_usp_pickup.prefab"; ExpectedItemId = "weapon_usp" },
    @{ Path = "Assets/prefabs/items/pf_ammo_9mm_pickup.prefab"; ExpectedItemId = "ammo_9mm" },
    @{ Path = "Assets/prefabs/items/pf_scrap_pickup.prefab"; ExpectedItemId = "chatarra" }
)
foreach ($pf in $pickupFiles) {
    if (Test-Path $pf.Path) {
        $content = Get-Content $pf.Path -Raw
        if ($content -match [regex]::Escape('"ItemId": "' + $pf.ExpectedItemId + '"')) {
            Pass "Pickup $($pf.Path) has correct ItemId: $($pf.ExpectedItemId)"
        } else {
            Fail "Pickup $($pf.Path) missing ItemId: $($pf.ExpectedItemId)"
        }
    } else {
        Fail "Pickup prefab missing: $($pf.Path)"
    }
}

# --- 11. Player prefab has InventoryComponent ---
$playerPrefab = Get-Content "Assets/prefabs/player.prefab" -Raw
if ($playerPrefab -match 'UltimoBarrio.InventoryComponent') {
    Pass "Player prefab has InventoryComponent"
} else {
    Fail "Player prefab missing InventoryComponent"
}
if ($playerPrefab -match 'UltimoBarrio.Economy.Wallet') {
    Pass "Player prefab has Wallet"
} else {
    Warn "Player prefab: Wallet component not found (may be optional)"
}

# --- 12. Scene has MapInstance ---
if ($sceneRaw -match 'Sandbox.MapInstance') {
    Pass "Scene has MapInstance"
} else {
    Fail "Scene missing MapInstance"
}

# --- 13. Summary ---
Write-Host ""
Write-Host "=== Validation Summary ===" -ForegroundColor Cyan
if ($failCount -eq 0) {
    Write-Host "ALL CHECKS PASSED" -ForegroundColor Green
    exit 0
} else {
    Write-Host "$failCount CHECK(S) FAILED" -ForegroundColor Red
    exit 1
}
