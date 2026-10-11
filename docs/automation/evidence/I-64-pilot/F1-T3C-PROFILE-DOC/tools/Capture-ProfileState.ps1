#Requires -Version 7.0
<#
.SYNOPSIS
  I-64 F1-T3C-PROFILE-DOC: captura de SOLO LECTURA del estado de perfil de AutoCAD 2025 relevante para RackCad.

.DESCRIPTION
  Procedimiento: docs/initiatives/I-64-f1-t3c-profile-diff-procedure.md.
  Fases: BASELINE (antes de abrir AutoCAD para Smoke-1), POST (despues de cerrarlo) y DRYRUN (validacion; nunca es
  una linea base).

  Lee, y nunca escribe:
    - HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles: valor por defecto (perfil actual) y nombres de perfiles;
    - el subarbol completo del perfil actual (Profiles\<perfil>), con TRUSTEDPATHS, SECURELOAD y Dialogs;
    - FixedProfile, Applications, InstalledApplications y Loaded de la misma clave de producto (HKCU);
    - HKLM\SOFTWARE\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Applications y el valor AcadLocation;
    - acad.exe (FileVersion y SHA-256);
    - %APPDATA%\RackCad y %LOCALAPPDATA%\RackCad (rutas relativas, tamano y SHA-256 de cada archivo);
    - los cuatro directorios ApplicationPlugins de Autodesk (nombres de primer nivel);
    - nombres de variables de entorno de usuario y de maquina que contengan "RackCad".
  Las claves del registro se abren con OpenSubKey(ruta, $false): solo lectura.

  Escribe solo tres archivos NUEVOS en -OutDir (se niega a sobrescribir):
    profile-capture-<FASE>.json         canonico y redactado (versionable tras revision);
    profile-capture-<FASE>.json.sha256  su SHA-256;
    profile-capture-<FASE>.local.json   companero local con los datos en claro; NUNCA se versiona.

  Codigos de salida: 0 captura completa; 2 uso o destino invalido; 3 hay un proceso acad en ejecucion (no captura);
  4 falta la clave de producto de AutoCAD 2025 (no captura); 5 alguna expectativa -Expect* no coincide (archivo
  escrito); 6 hay entradas ilegibles (archivo escrito; la captura es INVALID).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidateSet('BASELINE', 'POST', 'DRYRUN')][string]$Phase,
    [Parameter(Mandatory = $true)][string]$OutDir,
    [string]$RunLabel = '',
    [string]$CandidateDll = '',
    [string]$ExpectTrustedPathsSha256 = '',
    [string]$ExpectSecureLoad = '',
    [string]$ExpectProfile = '',
    [string]$ExpectAcadSha256 = ''
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$Release = 'R25.0'
$ProductKey = 'ACAD-8101:409'
$DefaultProfile = '<<Unnamed Profile>>'
$HkcuProductPath = "Software\Autodesk\AutoCAD\$Release\$ProductKey"
$HklmProductPath = "SOFTWARE\Autodesk\AutoCAD\$Release\$ProductKey"
$Utf8 = [System.Text.UTF8Encoding]::new($false)
$Invariant = [System.Globalization.CultureInfo]::InvariantCulture

function Get-Sha256Hex([byte[]]$Bytes) {
    return [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-TextSha256([string]$Text) { return Get-Sha256Hex $Utf8.GetBytes($Text) }

function ConvertTo-Field([string]$Text) {
    if ($null -eq $Text) { return '' }
    return $Text.Replace('%', '%25').Replace('|', '%7C').Replace("`n", '%0A').Replace("`r", '%0D')
}

# --- Redaccion: rutas locales y nombres de usuario y de maquina -> marcadores ---
$PathTokens = [System.Collections.Generic.List[object]]::new()
function Add-PathToken([string]$Value, [string]$Token) {
    if ([string]::IsNullOrWhiteSpace($Value)) { return }
    $trimmed = $Value.TrimEnd('\', '/')
    $PathTokens.Add(@($trimmed, $Token))
    $PathTokens.Add(@($trimmed.Replace('\', '/'), $Token))
}
Add-PathToken ${env:ProgramFiles(x86)} '<PROGRAMFILESX86>'
Add-PathToken $env:ProgramFiles '<PROGRAMFILES>'
Add-PathToken $env:ProgramData '<PROGRAMDATA>'
Add-PathToken $env:LOCALAPPDATA '<LOCALAPPDATA>'
Add-PathToken $env:APPDATA '<APPDATA>'
Add-PathToken $env:USERPROFILE '<USERPROFILE>'
$SortedPathTokens = @($PathTokens | Sort-Object -Property @{ Expression = { $_[0].Length }; Descending = $true })
$WordTokens = @()
if ($env:USERNAME -and $env:USERNAME.Length -ge 3) { $WordTokens += , @($env:USERNAME, '<USER>') }
if ($env:COMPUTERNAME -and $env:COMPUTERNAME.Length -ge 3) { $WordTokens += , @($env:COMPUTERNAME, '<MACHINE>') }
$ResidualPath = '(?i)((^|[^A-Za-z0-9])[A-Za-z]:[\\/])|(\\\\[^\\])|((^|[\\/])Users[\\/])'

function Get-Redacted([string]$Text) {
    if ([string]::IsNullOrEmpty($Text)) { return $Text }
    $r = $Text
    foreach ($t in $SortedPathTokens) {
        $r = [regex]::Replace($r, [regex]::Escape($t[0]), $t[1], [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    }
    foreach ($w in $WordTokens) {
        $r = [regex]::Replace($r, '(?<![A-Za-z0-9])' + [regex]::Escape($w[0]) + '(?![A-Za-z0-9])', $w[1],
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    }
    if ($r -match $ResidualPath) { return '<REDACTED:' + (Get-TextSha256 $Text).Substring(0, 12) + '>' }
    return $r
}

function Get-ProfileLabel([string]$Name) {
    if ($Name -eq $DefaultProfile) { return $Name }
    return 'SHA256:' + (Get-TextSha256 $Name)
}

# --- Precondiciones ---
$acadCount = @(Get-Process -Name 'acad' -ErrorAction SilentlyContinue).Count
if ($acadCount -ne 0) {
    Write-Output "STOP: hay $acadCount proceso(s) acad en ejecucion; no se captura (codigo 3)."
    exit 3
}
if (-not (Test-Path -LiteralPath $OutDir -PathType Container)) {
    Write-Output 'ERROR: -OutDir debe existir y ser un directorio (codigo 2).'
    exit 2
}
$OutFull = [System.IO.Path]::GetFullPath($OutDir).TrimEnd('\') + '\'
$GuardRoots = @(
    (Join-Path $env:APPDATA 'RackCad'), (Join-Path $env:LOCALAPPDATA 'RackCad'),
    (Join-Path $env:APPDATA 'Autodesk\ApplicationPlugins'), (Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins'),
    (Join-Path $env:ProgramFiles 'Autodesk\ApplicationPlugins'), (Join-Path ${env:ProgramFiles(x86)} 'Autodesk\ApplicationPlugins'))
foreach ($g in $GuardRoots) {
    $gf = [System.IO.Path]::GetFullPath($g).TrimEnd('\') + '\'
    if ($OutFull.StartsWith($gf, [System.StringComparison]::OrdinalIgnoreCase)) {
        Write-Output 'ERROR: -OutDir no puede estar dentro de una raiz capturada (codigo 2).'
        exit 2
    }
}
$BaseName = "profile-capture-$Phase"
$CanonicalPath = Join-Path $OutDir "$BaseName.json"
$SidecarPath = "$CanonicalPath.sha256"
$LocalPath = Join-Path $OutDir "$BaseName.local.json"
foreach ($p in @($CanonicalPath, $SidecarPath, $LocalPath)) {
    if (Test-Path -LiteralPath $p) {
        Write-Output 'ERROR: el archivo de salida ya existe; la evidencia no se sobrescribe (codigo 2).'
        exit 2
    }
}

$Hkcu = [Microsoft.Win32.Registry]::CurrentUser
$Hklm = [Microsoft.Win32.Registry]::LocalMachine
$productKeyHandle = $Hkcu.OpenSubKey($HkcuProductPath, $false)
if ($null -eq $productKeyHandle) {
    Write-Output 'STOP: falta la clave de producto de AutoCAD 2025 en HKCU; no se captura (codigo 4).'
    exit 4
}
$productKeyHandle.Dispose()

$Lines = [System.Collections.Generic.List[string]]::new()
$LocalValues = [System.Collections.Generic.List[object]]::new()
$LocalFiles = [System.Collections.Generic.List[object]]::new()
$FileTimes = [ordered]@{}
$script:Unreadable = 0

function Read-RegistryValue($Key, [string]$Name) {
    $kind = $Key.GetValueKind($Name)
    $v = $Key.GetValue($Name, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
    $text = ''
    $bytes = [byte[]]@()
    switch ($kind.ToString()) {
        'String' { $text = [string]$v; $bytes = $Utf8.GetBytes($text) }
        'ExpandString' { $text = [string]$v; $bytes = $Utf8.GetBytes($text) }
        'MultiString' { $text = [string]::Join("`n", [string[]]$v); $bytes = $Utf8.GetBytes($text) }
        'DWord' { $text = [System.BitConverter]::ToUInt32([System.BitConverter]::GetBytes([int]$v), 0).ToString($Invariant); $bytes = $Utf8.GetBytes($text) }
        'QWord' { $text = [System.BitConverter]::ToUInt64([System.BitConverter]::GetBytes([long]$v), 0).ToString($Invariant); $bytes = $Utf8.GetBytes($text) }
        default {
            if ($v -is [byte[]]) { $bytes = [byte[]]$v; $text = [System.Convert]::ToHexString($bytes).ToLowerInvariant() }
            elseif ($null -ne $v) { $text = [string]$v; $bytes = $Utf8.GetBytes($text) }
        }
    }
    return [pscustomobject]@{ Kind = $kind.ToString(); Bytes = $bytes; Text = $text }
}

function Add-RegistryTree($Hive, [string]$HivePrefix, [string]$Path, [string]$Display, [bool]$Recurse) {
    $key = $Hive.OpenSubKey($Path, $false)
    if ($null -eq $key) {
        $Lines.Add('F|RootPresent|' + (ConvertTo-Field $Display) + '|false')
        return
    }
    try {
        $redKey = ConvertTo-Field (Get-Redacted $Display)
        $Lines.Add('K|' + $redKey)
        foreach ($name in $key.GetValueNames()) {
            $shownName = if ($name -eq '') { '(default)' } else { $name }
            try {
                $r = Read-RegistryValue $key $name
                $sha = Get-Sha256Hex $r.Bytes
                $line = 'V|' + $redKey + '|' + (ConvertTo-Field (Get-Redacted $shownName)) + '|' + $r.Kind + '|' + $r.Bytes.Length + '|' + $sha
                $Lines.Add($line)
                $LocalValues.Add([ordered]@{ Line = $line; ClearPath = "$HivePrefix\$Path"; ClearName = $shownName; Kind = $r.Kind; Data = $r.Text })
            } catch {
                $script:Unreadable++
                $Lines.Add('V|' + $redKey + '|' + (ConvertTo-Field (Get-Redacted $shownName)) + '|UNREADABLE|0|UNREADABLE')
            }
        }
        foreach ($sub in $key.GetSubKeyNames()) {
            if ($Recurse) { Add-RegistryTree $Hive $HivePrefix "$Path\$sub" "$Display\$sub" $true }
        }
    } finally {
        $key.Dispose()
    }
}

function Add-FileTree([string]$Root, [string]$Token) {
    if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
        $Lines.Add("F|RootPresent|$Token|false")
        return
    }
    $Lines.Add("F|RootPresent|$Token|true")
    $rootFull = [System.IO.Path]::GetFullPath($Root).TrimEnd('\') + '\'
    foreach ($item in Get-ChildItem -LiteralPath $Root -Recurse -Force) {
        $rel = $item.FullName.Substring($rootFull.Length).Replace('\', '/')
        $relField = ConvertTo-Field (Get-Redacted $rel)
        if ($item.PSIsContainer) {
            $Lines.Add("D|$Token|$relField/|DIR|-")
            continue
        }
        try {
            $sha = (Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            $Lines.Add("D|$Token|$relField|$($item.Length)|$sha")
        } catch {
            $script:Unreadable++
            $Lines.Add("D|$Token|$relField|UNREADABLE|UNREADABLE")
        }
        $FileTimes["$Token/$relField"] = $item.LastWriteTimeUtc.ToString('o', $Invariant)
        $LocalFiles.Add([ordered]@{ Token = $Token; ClearPath = $item.FullName; Bytes = $item.Length; LastWriteUtc = $item.LastWriteTimeUtc.ToString('o', $Invariant) })
    }
}

# --- Identidad de la herramienta (contenido con LF, igual al blob de Git) ---
$toolText = [System.IO.File]::ReadAllText($PSCommandPath).Replace("`r`n", "`n")
$Lines.Add('F|Schema|rackcad-i64-profile-capture/v1')
$Lines.Add('F|ToolSha256|' + (Get-TextSha256 $toolText))
$Lines.Add("F|AutoCADRelease|$Release")
$Lines.Add("F|ProductKey|$ProductKey")
$Lines.Add("F|AcadProcessCount|$acadCount")

# --- Perfil actual y conjunto de perfiles ---
$profilesKey = $Hkcu.OpenSubKey("$HkcuProductPath\Profiles", $false)
$currentProfile = ''
try {
    $currentProfile = [string]$profilesKey.GetValue('')
    $profileNames = [string[]]$profilesKey.GetSubKeyNames()
    [System.Array]::Sort($profileNames, [System.StringComparer]::Ordinal)
    $Lines.Add('F|CurrentProfile|' + (ConvertTo-Field (Get-ProfileLabel $currentProfile)))
    $Lines.Add('F|ProfileCount|' + $profileNames.Length)
    $labels = @($profileNames | ForEach-Object { Get-ProfileLabel $_ })
    [string[]]$labelArr = $labels
    [System.Array]::Sort($labelArr, [System.StringComparer]::Ordinal)
    $Lines.Add('F|ProfileSet|' + (ConvertTo-Field ([string]::Join(';', $labelArr))))
    foreach ($name in $profilesKey.GetValueNames()) {
        if ($name -eq '') { continue }
        $r = Read-RegistryValue $profilesKey $name
        $Lines.Add('V|HKCU:PRODUCT\Profiles|' + (ConvertTo-Field (Get-Redacted $name)) + '|' + $r.Kind + '|' + $r.Bytes.Length + '|' + (Get-Sha256Hex $r.Bytes))
    }
} finally {
    $profilesKey.Dispose()
}
if ([string]::IsNullOrEmpty($currentProfile)) {
    Write-Output 'STOP: el perfil actual no se puede leer; no se captura (codigo 4).'
    exit 4
}

# --- Arboles del registro ---
Add-RegistryTree $Hkcu 'HKCU' "$HkcuProductPath\Profiles\$currentProfile" 'HKCU:PRODUCT\Profiles\<PROFILE>' $true
Add-RegistryTree $Hkcu 'HKCU' "$HkcuProductPath\FixedProfile" 'HKCU:PRODUCT\FixedProfile' $true
Add-RegistryTree $Hkcu 'HKCU' "$HkcuProductPath\Applications" 'HKCU:PRODUCT\Applications' $true
Add-RegistryTree $Hkcu 'HKCU' "$HkcuProductPath\InstalledApplications" 'HKCU:PRODUCT\InstalledApplications' $true
Add-RegistryTree $Hkcu 'HKCU' "$HkcuProductPath\Loaded" 'HKCU:PRODUCT\Loaded' $true
Add-RegistryTree $Hklm 'HKLM' "$HklmProductPath\Applications" 'HKLM:PRODUCT\Applications' $true

# --- Hechos de seguridad del perfil (I-52: valor exacto, UTF-8 sin BOM, sin salto final; entradas separadas por ';') ---
$variables = $Hkcu.OpenSubKey("$HkcuProductPath\Profiles\$currentProfile\Variables", $false)
$secureLoad = 'ABSENT'
$trusted = $null
try {
    if ($null -ne $variables) {
        if ($variables.GetValueNames() -contains 'SECURELOAD') { $secureLoad = (Read-RegistryValue $variables 'SECURELOAD').Text }
        if ($variables.GetValueNames() -contains 'TRUSTEDPATHS') { $trusted = (Read-RegistryValue $variables 'TRUSTEDPATHS').Text }
    }
} finally {
    if ($null -ne $variables) { $variables.Dispose() }
}
$Lines.Add('F|SECURELOAD|' + (ConvertTo-Field $secureLoad))
$trustedChainSha = 'ABSENT'
$trustedEntries = @()
if ($null -ne $trusted) {
    $trustedChainSha = Get-TextSha256 $trusted
    $segments = $trusted.Split(';')
    $trustedEntries = @($segments | Where-Object { $_ -ne '' })
    $Lines.Add('F|TRUSTEDPATHS.EntryCount|' + $trustedEntries.Count)
    $Lines.Add('F|TRUSTEDPATHS.EmptySegments|' + @($segments | Where-Object { $_ -eq '' }).Count)
    for ($i = 0; $i -lt $trustedEntries.Count; $i++) {
        $Lines.Add('F|TRUSTEDPATHS.Entry.' + ($i + 1).ToString('000', $Invariant) + '|' + (Get-TextSha256 $trustedEntries[$i]))
    }
}
$Lines.Add("F|TRUSTEDPATHS.ChainSha256|$trustedChainSha")

$pickfirst = @($LocalValues | Where-Object { $_.ClearName -eq 'PICKFIRST' })
$Lines.Add('F|PICKFIRST|' + $(if ($pickfirst.Count -eq 0) { 'ABSENT' } else { ConvertTo-Field ([string]::Join(';', @($pickfirst | ForEach-Object { $_.Data }))) }))

# --- DLL candidato (identidad del DLL que el Owner cargara en el smoke) ---
if ($CandidateDll) {
    if (-not (Test-Path -LiteralPath $CandidateDll -PathType Leaf)) {
        Write-Output 'ERROR: -CandidateDll no existe (codigo 2).'
        exit 2
    }
    $dllFull = [System.IO.Path]::GetFullPath($CandidateDll)
    $Lines.Add('F|CandidateDll.FileName|' + (ConvertTo-Field ([System.IO.Path]::GetFileName($dllFull))))
    $Lines.Add('F|CandidateDll.Sha256|' + (Get-FileHash -LiteralPath $dllFull -Algorithm SHA256).Hash.ToLowerInvariant())
    $dllDir = [System.IO.Path]::GetDirectoryName($dllFull).TrimEnd('\')
    $inTrusted = @($trustedEntries | Where-Object {
            $e = $_.TrimEnd('\')
            $base = if ($e.EndsWith('\...')) { $e.Substring(0, $e.Length - 4) } else { $e }
            ($e -ieq $dllDir) -or ($e.EndsWith('\...') -and ($dllDir -ieq $base -or $dllDir.StartsWith($base + '\', [System.StringComparison]::OrdinalIgnoreCase)))
        }).Count -gt 0
    $Lines.Add('F|TRUSTEDPATHS.CoversCandidateDllFolder|' + $inTrusted.ToString().ToLowerInvariant())
} else {
    $Lines.Add('F|CandidateDll.Sha256|NOT_PROVIDED')
}

# --- Identidad de acad.exe ---
$acadSha = 'UNKNOWN'
$acadVersion = 'UNKNOWN'
$hklmProduct = $Hklm.OpenSubKey($HklmProductPath, $false)
if ($null -ne $hklmProduct) {
    try {
        $loc = [string]$hklmProduct.GetValue('AcadLocation')
        if ($loc) {
            $exe = Join-Path $loc 'acad.exe'
            if (Test-Path -LiteralPath $exe -PathType Leaf) {
                $acadVersion = (Get-Item -LiteralPath $exe).VersionInfo.FileVersion
                $acadSha = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash.ToLowerInvariant()
            }
        }
    } finally {
        $hklmProduct.Dispose()
    }
}
$Lines.Add('F|AcadExeFileVersion|' + (ConvertTo-Field $acadVersion))
$Lines.Add("F|AcadExeSha256|$acadSha")

# --- Estado de usuario de RackCad y raices de plugins ---
Add-FileTree (Join-Path $env:APPDATA 'RackCad') 'RACKCAD_APPDATA'
Add-FileTree (Join-Path $env:LOCALAPPDATA 'RackCad') 'RACKCAD_LOCALAPPDATA'
$pluginRoots = [ordered]@{
    PLUGINS_APPDATA         = (Join-Path $env:APPDATA 'Autodesk\ApplicationPlugins')
    PLUGINS_PROGRAMDATA     = (Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins')
    PLUGINS_PROGRAMFILES    = (Join-Path $env:ProgramFiles 'Autodesk\ApplicationPlugins')
    PLUGINS_PROGRAMFILESX86 = (Join-Path ${env:ProgramFiles(x86)} 'Autodesk\ApplicationPlugins')
}
foreach ($token in $pluginRoots.Keys) {
    $root = $pluginRoots[$token]
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        $Lines.Add("F|RootPresent|$token|false")
        continue
    }
    $Lines.Add("F|RootPresent|$token|true")
    foreach ($item in Get-ChildItem -LiteralPath $root -Force) {
        $kind = if ($item.PSIsContainer) { 'DIR' } else { 'FILE' }
        $Lines.Add("P|$token|" + (ConvertTo-Field (Get-Redacted $item.Name)) + "|$kind")
        if ($item.Name -match '(?i)rackcad') { Add-FileTree $item.FullName ("$token/" + $item.Name) }
    }
}

# --- Variables de entorno persistentes con RackCad en el nombre ---
$envCounts = [ordered]@{ USER = 0; MACHINE = 0 }
$envKeys = [ordered]@{ USER = @($Hkcu, 'Environment'); MACHINE = @($Hklm, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment') }
foreach ($scope in $envKeys.Keys) {
    $k = $envKeys[$scope][0].OpenSubKey($envKeys[$scope][1], $false)
    if ($null -eq $k) { continue }
    try {
        foreach ($name in $k.GetValueNames()) {
            if ($name -notmatch '(?i)rackcad') { continue }
            $r = Read-RegistryValue $k $name
            $Lines.Add("E|$scope|" + (ConvertTo-Field $name) + '|' + (Get-Sha256Hex $r.Bytes))
            $envCounts[$scope]++
        }
    } finally {
        $k.Dispose()
    }
}
$Lines.Add('F|RackCadEnvUser|' + $envCounts.USER)
$Lines.Add('F|RackCadEnvMachine|' + $envCounts.MACHINE)
$Lines.Add('F|Unreadable|' + $script:Unreadable)

# --- Libro canonico: lineas ordenadas por ordinal, UTF-8 sin BOM, LF ---
[string[]]$ledger = $Lines.ToArray()
[System.Array]::Sort($ledger, [System.StringComparer]::Ordinal)
$canonicalText = [string]::Join("`n", $ledger) + "`n"
$canonicalSha = Get-TextSha256 $canonicalText

# --- Comprobaciones previas al smoke (no forman parte del libro) ---
$checks = [ordered]@{}
$mismatch = $false
function Add-Check([string]$Name, [string]$Expected, [string]$Actual) {
    if (-not $Expected) { return }
    $ok = [string]::Equals($Expected, $Actual, [System.StringComparison]::OrdinalIgnoreCase)
    $checks[$Name] = [ordered]@{ Expected = $Expected; Actual = $Actual; Result = $(if ($ok) { 'MATCH' } else { 'MISMATCH' }) }
    if (-not $ok) { $script:mismatch = $true }
}
Add-Check 'TRUSTEDPATHS.ChainSha256' $ExpectTrustedPathsSha256 $trustedChainSha
Add-Check 'SECURELOAD' $ExpectSecureLoad $secureLoad
Add-Check 'CurrentProfile' $ExpectProfile (Get-ProfileLabel $currentProfile)
Add-Check 'AcadExeSha256' $ExpectAcadSha256 $acadSha

$facts = [ordered]@{}
foreach ($l in $ledger) {
    if (-not $l.StartsWith('F|')) { continue }
    $f = $l.Split('|')
    $facts[[string]::Join('|', $f[1..($f.Length - 2)])] = $f[$f.Length - 1]
}
$counts = [ordered]@{
    Keys = @($ledger | Where-Object { $_.StartsWith('K|') }).Count
    Values = @($ledger | Where-Object { $_.StartsWith('V|') }).Count
    FileEntries = @($ledger | Where-Object { $_.StartsWith('D|') }).Count
    PluginEntries = @($ledger | Where-Object { $_.StartsWith('P|') }).Count
    EnvironmentEntries = @($ledger | Where-Object { $_.StartsWith('E|') }).Count
    Unreadable = $script:Unreadable
}

function Write-NewFile([string]$Path, [string]$Text) {
    $bytes = $Utf8.GetBytes($Text)
    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
    return Get-Sha256Hex $bytes
}

$local = [ordered]@{
    Schema = 'rackcad-i64-profile-capture-local/v1'
    Warning = 'COMPANERO LOCAL CON DATOS EN CLARO: NO SE VERSIONA NI SE COMPARTE FUERA DE LA ESTACION'
    Phase = $Phase
    CanonicalSha256 = $canonicalSha
    Values = $LocalValues
    Files = $LocalFiles
}
$localSha = Write-NewFile $LocalPath ((ConvertTo-Json $local -Depth 8).Replace("`r`n", "`n") + "`n")

$doc = [ordered]@{
    Schema = 'rackcad-i64-profile-capture/v1'
    Phase = $Phase
    RunLabel = (Get-Redacted $RunLabel)
    CanonicalSha256 = $canonicalSha
    LedgerLineCount = $ledger.Length
    Facts = $facts
    Counts = $counts
    PreSmokeChecks = $checks
    Ledger = $ledger
    Ephemeral = [ordered]@{
        Note = 'campos efimeros: no forman parte del libro canonico ni de CanonicalSha256'
        CapturedUtc = [System.DateTime]::UtcNow.ToString('o', $Invariant)
        PSVersion = $PSVersionTable.PSVersion.ToString()
        FileLastWriteUtc = $FileTimes
    }
    LocalCompanion = [ordered]@{ FileName = [System.IO.Path]::GetFileName($LocalPath); Sha256 = $localSha; Versioned = $false }
}
$docSha = Write-NewFile $CanonicalPath ((ConvertTo-Json $doc -Depth 8).Replace("`r`n", "`n") + "`n")
[void](Write-NewFile $SidecarPath ($docSha + '  ' + [System.IO.Path]::GetFileName($CanonicalPath) + "`n"))

Write-Output ("{0}: CanonicalSha256={1} lines={2} keys={3} values={4} files={5} plugins={6} env={7} unreadable={8}" -f `
        $Phase, $canonicalSha, $ledger.Length, $counts.Keys, $counts.Values, $counts.FileEntries, $counts.PluginEntries, $counts.EnvironmentEntries, $counts.Unreadable)
if ($script:Unreadable -gt 0) { exit 6 }
if ($mismatch) { exit 5 }
exit 0
