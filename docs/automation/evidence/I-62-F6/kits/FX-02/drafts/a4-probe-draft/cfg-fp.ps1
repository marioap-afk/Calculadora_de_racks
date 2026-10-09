# Fingerprint of ~/.codex/config.toml for P-01: SHA-256, size, mtime and the SANITIZED section/key names (README §13.4). Never emits a value.
param([string]$Out)
$ErrorActionPreference = 'Stop'
$cfg = Join-Path $env:USERPROFILE '.codex\config.toml'
$names = [System.Collections.Generic.List[string]]::new()
$section = $null
foreach ($line in [System.IO.File]::ReadLines($cfg)) {
    $t = $line.Trim()
    if ($t -match '^\[\[?([^\]]+)\]\]?\s*(#.*)?$') { $section = $Matches[1].Trim(); $names.Add('[' + $section + ']'); continue }
    if ($t -match '^([A-Za-z0-9_\-]+|"[^"]*"|''[^'']*'')(\s*\.\s*([A-Za-z0-9_\-]+|"[^"]*"))*\s*=') {
        $key = ($t -split '=', 2)[0].Trim()
        $names.Add($(if ($section) { $section + '.' + $key } else { $key }))
    }
}
$sanitized = foreach ($n in $names) {
    if ($n.IndexOfAny([char[]]@('\', '/', ':')) -ge 0) { if ($n -match '^\[([^.\]]+)\.') { '[' + $Matches[1] + '.<redactado>]' } else { '<redactado>' } } else { $n }
}
$i = Get-Item -LiteralPath $cfg
[ordered]@{
    Path = '~/.codex/config.toml'; Sha256 = (Get-FileHash -LiteralPath $cfg -Algorithm SHA256).Hash; Length = $i.Length
    LastWriteTimeUtc = $i.LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ss.fffZ'); ObservedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    KeyNames = @($sanitized | Sort-Object -CaseSensitive)
} | ConvertTo-Json -Depth 4 | Set-Content -Path $Out -Encoding utf8NoBOM
(Get-FileHash -LiteralPath $cfg -Algorithm SHA256).Hash
