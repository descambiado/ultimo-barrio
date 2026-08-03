$patterns = 'Ã.|Â.|â€™|â€œ|â€|â€“|â€”|â€¦|â€¢|â‚¬|ðŸ|Ð.'

$files = Get-ChildItem Code,Assets,docs -Recurse -File |
  Where-Object {
    $_.Extension -in '.cs','.razor','.scss','.css','.json','.scene','.prefab','.md','.txt'
  }

$results = $files |
  Select-String -Pattern $patterns |
  Select-Object Path, LineNumber, Line

New-Item -ItemType Directory -Force -Path "docs\production\evidence" -ErrorAction SilentlyContinue | Out-Null
$results | Out-File "docs\production\evidence\encoding-audit.md" -Encoding utf8
