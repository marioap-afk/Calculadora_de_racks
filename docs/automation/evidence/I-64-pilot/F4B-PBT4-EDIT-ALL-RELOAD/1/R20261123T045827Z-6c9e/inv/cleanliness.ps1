param([string]$Label)
$ErrorActionPreference = 'Continue'
$layout = Join-Path (Join-Path $env:<datos-locales-del-usuario> 'RackCad') 'RackFrameConfigurator.layout'
if (Test-Path -LiteralPath $layout) {
    $h = (Get-FileHash -LiteralPath $layout -Algorithm SHA256).Hash
    $m = (Get-Item -LiteralPath $layout).LastWriteTimeUtc.ToString('o')
    "layout: sha256=$h mtimeUtc=$m"
} else { 'layout: ausente' }

$settingsPath = Join-Path (Join-Path <datos-de-aplicacion-del-usuario> 'RackCad') 'settings.json'
$lib = $null
if (Test-Path -LiteralPath $settingsPath) {
    try { $s = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json; if ($s.DesignLibraryPath -and $s.DesignLibraryPath.Trim().Length -gt 0) { $lib = $s.DesignLibraryPath } } catch { }
}
if (-not $lib) { $lib = Join-Path (Join-Path <datos-de-aplicacion-del-usuario> 'RackCad') 'Designs' }
if (Test-Path -LiteralPath $lib) {
    $d = (Get-Item -LiteralPath $lib).LastWriteTimeUtc.ToString('o')
    $root = (Get-Item -LiteralPath $lib).FullName
    $items = Get-ChildItem -LiteralPath $lib -Recurse -Force | ForEach-Object { $_.FullName.Substring($root.Length) + '|' + $_.Length + '|' + $_.LastWriteTimeUtc.ToString('o') } | Sort-Object -CaseSensitive
    $text = ($items -join "`n")
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    $sha = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData($bytes)).Replace('-', '')
    "biblioteca: dirMtimeUtc=$d entries=$(@($items).Count) listingSha256=$sha"
} else { 'biblioteca: ausente' }
# settings.json (informativo): su hash y fecha
if (Test-Path -LiteralPath $settingsPath) {
    "settings.json: sha256=$((Get-FileHash -LiteralPath $settingsPath -Algorithm SHA256).Hash) mtimeUtc=$((Get-Item -LiteralPath $settingsPath).LastWriteTimeUtc.ToString('o'))"
} else { 'settings.json: ausente' }
