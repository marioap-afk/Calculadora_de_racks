# I-62 F6 Owner-decision preflight: PASSIVE, read-only host facts for OD-2, OD-3, OD-4, OD-5 and OD-7.
# Never executes codex.exe or claude.exe, never reads auth.json or any credential file, never prints a config value, changes nothing.
param([string]$Out)
$ErrorActionPreference = 'Stop'
$home_ = $env:USERPROFILE

function FileFacts([string]$path) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return [ordered]@{ Path = $path.Replace($home_, '~'); Exists = $false } }
    $i = Get-Item -LiteralPath $path
    $v = $null
    try { $v = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path) } catch { }
    [ordered]@{
        Path = $path.Replace($home_, '~'); Exists = $true; Sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        Length = $i.Length; LastWriteTimeUtc = $i.LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ssZ')
        FileVersion = $(if ($v) { $v.FileVersion } else { $null }); ProductVersion = $(if ($v) { $v.ProductVersion } else { $null })
    }
}

# ---- OD-2: ~/.codex/config.toml fingerprint (SHA-256 + sanitized section/key NAMES; values are never emitted)
$cfg = Join-Path $home_ '.codex\config.toml'
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
    if ($n.IndexOfAny([char[]]@('\', '/', ':')) -ge 0) {
        if ($n -match '^\[([^.\]]+)\.') { '[' + $Matches[1] + '.<redactado>]' } else { '<redactado>' }
    } else { $n }
}
$sorted = @($sanitized | Sort-Object -CaseSensitive)
$cfgItem = Get-Item -LiteralPath $cfg
$fingerprint = [ordered]@{
    Path = '~/.codex/config.toml'; Sha256 = (Get-FileHash -LiteralPath $cfg -Algorithm SHA256).Hash
    Length = $cfgItem.Length; LastWriteTimeUtc = $cfgItem.LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ssZ')
    KeyNameCount = $sorted.Count; RedactedCount = @($sorted | Where-Object { $_ -like '*<redactado>*' }).Count
    TopLevelSections = @($sorted | Where-Object { $_ -like '`[*' } | ForEach-Object { ($_.Trim('[', ']') -split '\.')[0] } | Sort-Object -Unique)
    TopLevelKeys = @($sorted | Where-Object { $_ -notlike '`[*' -and $_ -notlike '*.*' })
    SandboxRelatedNames = @($sorted | Where-Object { $_ -match '(?i)sandbox|approval|windows' })
    KeyNames = $sorted
}

# ---- Codex binaries and app (metadata and hashes only; never executed)
$pkgs = @(Get-AppxPackage -Name 'OpenAI.Codex*' -ErrorAction SilentlyContinue | ForEach-Object { [ordered]@{ Name = $_.Name; Version = $_.Version.ToString(); InstallDate = $null } })
$codexPaths = [System.Collections.Generic.List[string]]::new()
foreach ($p in Get-AppxPackage -Name 'OpenAI.Codex*' -ErrorAction SilentlyContinue) { $codexPaths.Add((Join-Path $p.InstallLocation 'app\resources\codex.exe')) }
$codexPaths.Add((Join-Path $home_ '.codex\plugins\.plugin-appserver\codex.exe'))
$codexPaths.Add((Join-Path $home_ '.codex\.sandbox-bin\codex.exe'))
if (Test-Path (Join-Path $home_ '.codex\bin')) { Get-ChildItem (Join-Path $home_ '.codex\bin') -Recurse -Filter 'codex*.exe' -ErrorAction SilentlyContinue | ForEach-Object { $codexPaths.Add($_.FullName) } }
$codexBinaries = @($codexPaths | Select-Object -Unique | ForEach-Object { FileFacts $_ })
$whereCodex = (& where.exe codex 2>$null) -join ';'
$sandboxBin = Join-Path $home_ '.codex\.sandbox-bin'
$sandboxDir = if (Test-Path $sandboxBin) { @(Get-ChildItem $sandboxBin -File | ForEach-Object { [ordered]@{ Name = $_.Name; LastWriteTimeUtc = $_.LastWriteTimeUtc.ToString('yyyy-MM-ddTHH:mm:ssZ') } }) } else { @() }

# ---- Claude CLI (metadata only; never executed, never authenticated; credentials never touched)
$claudeLocal = Join-Path $home_ '.local\bin\claude.exe'
$whereClaude = (& where.exe claude 2>$null) -join ';'

# ---- Processes (names only)
$procs = @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match '^(codex|claude)' } | Select-Object -ExpandProperty ProcessName | Sort-Object -Unique)

$result = [ordered]@{
    Purpose = 'I-62 F6 Owner-decision preflight: passive read-only facts (OD-2, OD-3, OD-4, OD-5, OD-7)'
    ObservedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    Host = [ordered]@{ OS = [System.Environment]::OSVersion.VersionString; PowerShell = $PSVersionTable.PSVersion.ToString() }
    CodexConfigFingerprint = $fingerprint
    CodexApp = $pkgs
    CodexBinaries = $codexBinaries
    CodexOnPath = $(if ($whereCodex) { $whereCodex.Replace($home_, '~') } else { 'NO' })
    CodexSandboxBin = $sandboxDir
    ClaudeCli = [ordered]@{ OnPath = $(if ($whereClaude) { $whereClaude.Replace($home_, '~') } else { 'NO' }); LocalBinary = (FileFacts $claudeLocal); AuthState = 'UNKNOWN (observing it requires invoking the CLI; credentials are never read)' }
    ProcessNames = $procs
    NeverDone = @('codex.exe or claude.exe executed', 'auth.json or any credential file read', 'a config value emitted', 'any file, setting, remote or permission changed')
}
$result | ConvertTo-Json -Depth 6 | Set-Content -Path $Out -Encoding utf8NoBOM
'ok ' + $result.ObservedUtc
