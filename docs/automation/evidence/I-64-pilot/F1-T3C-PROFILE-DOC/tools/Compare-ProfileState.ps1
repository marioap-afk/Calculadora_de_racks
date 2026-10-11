#Requires -Version 7.0
<#
.SYNOPSIS
  I-64 F1-T3C-PROFILE-DOC: DIFF y DISPOSITION entre dos capturas canonicas (BASELINE y POST) de Capture-ProfileState.ps1.

.DESCRIPTION
  Procedimiento: docs/initiatives/I-64-f1-t3c-profile-diff-procedure.md (secciones 5 a 8).
  No lee el registro ni AutoCAD: solo los dos archivos de captura y sus .sha256. Escribe un unico archivo NUEVO
  (-OutFile) y su .sha256; se niega a sobrescribir.

  Reglas, en orden (la primera que aplica gana):
    INVALID   R-INV-01 acad en ejecucion en una captura; R-INV-02 cambia la identidad del host, de la herramienta o del
              DLL candidato; R-INV-03 entradas ilegibles; R-INV-04 integridad (sha256, libro canonico, fases).
    FORBIDDEN R-F-01 perfil actual o conjunto de perfiles; R-F-02 SECURELOAD; R-F-03 TRUSTEDPATHS; R-F-04 PICKFIRST;
              R-F-05 Applications, InstalledApplications o Loaded (HKCU) y Applications (HKLM); R-F-06 raices
              ApplicationPlugins; R-F-07 variables de entorno persistentes con RackCad; R-F-08 estado de usuario de
              RackCad fuera de %APPDATA%\RackCad\logs.
    REPORT    R-R-01 clave o valor nuevo o cambiado que nombra RackCad, RACKPANEL o Workspace (salvo R-A-02);
              R-R-02 clave nueva con nombre GUID en el perfil o en FixedProfile; R-R-99 ubicacion no clasificada.
    ALLOWED   R-A-01 registro de RackLog (%APPDATA%\RackCad\logs); R-A-02 colocacion de ventanas RackCad existentes en la
              BASELINE (Dialogs\RackCad.UI.<no Workspace>); R-A-03 actividad normal de AutoCAD en el perfil o FixedProfile.

  Disposicion y codigo de salida: CLEAN 0; REPORT con -PaletteKeyAcceptance ACCEPTED -> REPORTED_ACCEPTED 11;
  REPORT sin esa aceptacion -> REPORT_REQUIRED_STOP 10; FORBIDDEN_DRIFT 20; INVALID 30; uso invalido 2.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Baseline,
    [Parameter(Mandatory = $true)][string]$Post,
    [Parameter(Mandatory = $true)][string]$OutFile,
    [ValidateSet('ACCEPTED', 'NOT_ACCEPTED', 'NOT_RECORDED')][string]$PaletteKeyAcceptance = 'NOT_RECORDED',
    [switch]$AllowDryRun
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Utf8 = [System.Text.UTF8Encoding]::new($false)

function Get-Sha256Hex([byte[]]$Bytes) {
    return [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

if ((Test-Path -LiteralPath $OutFile) -or (Test-Path -LiteralPath "$OutFile.sha256")) {
    Write-Output 'ERROR: el archivo de salida ya existe; la evidencia no se sobrescribe (codigo 2).'
    exit 2
}

$integrity = [System.Collections.Generic.List[string]]::new()

function Read-Capture([string]$Path, [string]$Role) {
    $info = [ordered]@{ Role = $Role; File = [System.IO.Path]::GetFileName($Path); Sha256 = $null; SidecarOk = $false; CanonicalOk = $false; SortedOk = $false; Phase = $null; CanonicalSha256 = $null; Ledger = @() }
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { $integrity.Add("$Role`: archivo ausente"); return $info }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $info.Sha256 = Get-Sha256Hex $bytes
    $side = "$Path.sha256"
    if (Test-Path -LiteralPath $side -PathType Leaf) {
        $first = ([System.IO.File]::ReadAllText($side).Trim() -split '\s+')[0]
        $info.SidecarOk = ($first -eq $info.Sha256)
    }
    if (-not $info.SidecarOk) { $integrity.Add("$Role`: el .sha256 no coincide o falta") }
    $doc = $Utf8.GetString($bytes) | ConvertFrom-Json
    if ($doc.Schema -ne 'rackcad-i64-profile-capture/v1') { $integrity.Add("$Role`: esquema desconocido") }
    $info.Phase = [string]$doc.Phase
    $info.CanonicalSha256 = [string]$doc.CanonicalSha256
    [string[]]$ledger = @($doc.Ledger)
    $info.Ledger = $ledger
    $recomputed = Get-Sha256Hex $Utf8.GetBytes([string]::Join("`n", $ledger) + "`n")
    $info.CanonicalOk = ($recomputed -eq $info.CanonicalSha256)
    if (-not $info.CanonicalOk) { $integrity.Add("$Role`: CanonicalSha256 no coincide con el libro") }
    [string[]]$sorted = $ledger.Clone()
    [System.Array]::Sort($sorted, [System.StringComparer]::Ordinal)
    $info.SortedOk = ([string]::Join("`n", $sorted) -ceq [string]::Join("`n", $ledger))
    if (-not $info.SortedOk) { $integrity.Add("$Role`: el libro no esta ordenado por ordinal") }
    return $info
}

$b = Read-Capture $Baseline 'BASELINE'
$p = Read-Capture $Post 'POST'
if ($AllowDryRun) {
    if ($b.Phase -ne 'DRYRUN' -or $p.Phase -ne 'DRYRUN') { $integrity.Add('-AllowDryRun exige dos capturas DRYRUN') }
} else {
    if ($b.Phase -ne 'BASELINE') { $integrity.Add('la primera captura no es BASELINE') }
    if ($p.Phase -ne 'POST') { $integrity.Add('la segunda captura no es POST') }
}

function ConvertTo-Map([string[]]$Ledger, [string]$Role) {
    $map = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    foreach ($l in $Ledger) {
        $f = $l.Split('|')
        $id = $null
        $payload = $null
        switch ($f[0]) {
            'F' { $id = [string]::Join('|', $f[0..($f.Length - 2)]); $payload = $f[$f.Length - 1] }
            'K' { $id = $l; $payload = 'present' }
            'V' { $id = "V|$($f[1])|$($f[2])"; $payload = [string]::Join('|', $f[3..($f.Length - 1)]) }
            'D' { $id = "D|$($f[1])|$($f[2])"; $payload = [string]::Join('|', $f[3..($f.Length - 1)]) }
            'P' { $id = "P|$($f[1])|$($f[2])"; $payload = $f[3] }
            'E' { $id = "E|$($f[1])|$($f[2])"; $payload = $f[3] }
        }
        if ($null -eq $id) { $integrity.Add("$Role`: linea de tipo desconocido"); continue }
        if ($map.ContainsKey($id)) { $integrity.Add("$Role`: identidad duplicada $id"); continue }
        $map[$id] = $payload
    }
    return , $map
}
$bm = ConvertTo-Map $b.Ledger 'BASELINE'
$pm = ConvertTo-Map $p.Ledger 'POST'

$GuidSegment = '^\{?[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\}?$'
$InvalidFacts = @('Schema', 'ToolSha256', 'AutoCADRelease', 'ProductKey', 'AcadExeFileVersion', 'AcadExeSha256', 'CandidateDll.FileName', 'CandidateDll.Sha256', 'AcadProcessCount', 'Unreadable')

function Get-Classification([string]$Id, [string]$Change) {
    $parts = $Id.Split('|')
    switch ($parts[0]) {
        'F' {
            $name = $parts[1]
            if ($InvalidFacts -contains $name) { return @('INVALID', 'R-INV-02') }
            if (@('CurrentProfile', 'ProfileCount', 'ProfileSet') -contains $name) { return @('FORBIDDEN', 'R-F-01') }
            if ($name -eq 'SECURELOAD') { return @('FORBIDDEN', 'R-F-02') }
            if ($name.StartsWith('TRUSTEDPATHS')) { return @('FORBIDDEN', 'R-F-03') }
            if ($name -eq 'PICKFIRST') { return @('FORBIDDEN', 'R-F-04') }
            if ($name -eq 'RootPresent') {
                $root = $parts[2]
                if ($root.StartsWith('PLUGINS_')) { return @('FORBIDDEN', 'R-F-06') }
                if ($root.StartsWith('RACKCAD_')) { return @('FORBIDDEN', 'R-F-08') }
                if ($root -match '\\(Applications|InstalledApplications|Loaded)$') { return @('FORBIDDEN', 'R-F-05') }
                return @('FORBIDDEN', 'R-F-01')
            }
            if ($name -like 'RackCadEnv*') { return @('FORBIDDEN', 'R-F-07') }
            return @('REPORT', 'R-R-99')
        }
        { $_ -eq 'K' -or $_ -eq 'V' } {
            $path = $parts[1]
            $name = if ($parts[0] -eq 'V') { $parts[2] } else { '' }
            if ($path -match '^HKCU:PRODUCT\\(Applications|InstalledApplications|Loaded)(\\|$)' -or $path -match '^HKLM:PRODUCT\\Applications(\\|$)') { return @('FORBIDDEN', 'R-F-05') }
            if ($path -eq 'HKCU:PRODUCT\Profiles') { return @('FORBIDDEN', 'R-F-01') }
            if ($parts[0] -eq 'V' -and $path -eq 'HKCU:PRODUCT\Profiles\<PROFILE>\Variables' -and $name -eq 'TRUSTEDPATHS') { return @('FORBIDDEN', 'R-F-03') }
            if ($parts[0] -eq 'V' -and $path -eq 'HKCU:PRODUCT\Profiles\<PROFILE>\Variables' -and $name -eq 'SECURELOAD') { return @('FORBIDDEN', 'R-F-02') }
            if ($parts[0] -eq 'V' -and $name -eq 'PICKFIRST') { return @('FORBIDDEN', 'R-F-04') }
            if (("$path\$name") -match '(?i)rackcad|rackpanel|workspace') {
                $m = [regex]::Match($path, '^(HKCU:PRODUCT\\Profiles\\<PROFILE>\\Dialogs\\RackCad\.UI\.(?!Workspace)[^\\]+)')
                if ($m.Success -and $bm.ContainsKey('K|' + $m.Groups[1].Value)) { return @('ALLOWED', 'R-A-02') }
                return @('REPORT', 'R-R-01')
            }
            if ($path -match '^HKCU:PRODUCT\\(Profiles\\<PROFILE>|FixedProfile)(\\|$)') {
                $segments = $path.Split('\')
                for ($i = 0; $i -lt $segments.Length; $i++) {
                    if ($segments[$i] -match $GuidSegment) {
                        $guidKey = 'K|' + [string]::Join('\', $segments[0..$i])
                        if (-not $bm.ContainsKey($guidKey)) { return @('REPORT', 'R-R-02') }
                    }
                }
                return @('ALLOWED', 'R-A-03')
            }
            return @('REPORT', 'R-R-99')
        }
        'D' {
            $root = $parts[1]
            $rel = $parts[2]
            if ($root -eq 'RACKCAD_APPDATA' -and $rel -match '^logs(/|$)') { return @('ALLOWED', 'R-A-01') }
            if ($root.StartsWith('RACKCAD_')) { return @('FORBIDDEN', 'R-F-08') }
            if ($root.StartsWith('PLUGINS_')) { return @('FORBIDDEN', 'R-F-06') }
            return @('REPORT', 'R-R-99')
        }
        'P' { return @('FORBIDDEN', 'R-F-06') }
        'E' { return @('FORBIDDEN', 'R-F-07') }
    }
    return @('REPORT', 'R-R-99')
}

$entries = [System.Collections.Generic.List[object]]::new()
$preconditions = [System.Collections.Generic.List[object]]::new()
foreach ($pair in @(@('BASELINE', $bm), @('POST', $pm))) {
    $m = $pair[1]
    if (-not $m.ContainsKey('F|AcadProcessCount') -or $m['F|AcadProcessCount'] -ne '0') {
        $preconditions.Add([ordered]@{ Capture = $pair[0]; Class = 'INVALID'; Rule = 'R-INV-01'; Detail = 'AcadProcessCount distinto de 0 o ausente' })
    }
    $unreadable = @($m.Keys | Where-Object { $m[$_] -match 'UNREADABLE' }).Count
    if ($unreadable -gt 0 -or ($m.ContainsKey('F|Unreadable') -and $m['F|Unreadable'] -ne '0')) {
        $preconditions.Add([ordered]@{ Capture = $pair[0]; Class = 'INVALID'; Rule = 'R-INV-03'; Detail = 'entradas ilegibles' })
    }
}
foreach ($msg in $integrity) { $preconditions.Add([ordered]@{ Capture = 'BOTH'; Class = 'INVALID'; Rule = 'R-INV-04'; Detail = $msg }) }

$ids = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($k in $bm.Keys) { [void]$ids.Add($k) }
foreach ($k in $pm.Keys) { [void]$ids.Add($k) }
foreach ($id in $ids) {
    $inB = $bm.ContainsKey($id)
    $inP = $pm.ContainsKey($id)
    $change = $null
    if ($inB -and -not $inP) { $change = 'REMOVED' }
    elseif ($inP -and -not $inB) { $change = 'ADDED' }
    elseif ($bm[$id] -cne $pm[$id]) { $change = 'CHANGED' }
    if (-not $change) { continue }
    $c = Get-Classification $id $change
    $entries.Add([ordered]@{
            Id = $id; Change = $change; Class = $c[0]; Rule = $c[1]
            Baseline = $(if ($inB) { $bm[$id] } else { $null }); Post = $(if ($inP) { $pm[$id] } else { $null })
        })
}

$classCount = [ordered]@{ INVALID = 0; FORBIDDEN = 0; REPORT = 0; ALLOWED = 0 }
foreach ($e in $entries) { $classCount[$e.Class]++ }
$classCount.INVALID += $preconditions.Count
$changeCount = [ordered]@{
    ADDED = @($entries | Where-Object { $_.Change -eq 'ADDED' }).Count
    REMOVED = @($entries | Where-Object { $_.Change -eq 'REMOVED' }).Count
    CHANGED = @($entries | Where-Object { $_.Change -eq 'CHANGED' }).Count
}
if ($classCount.INVALID -gt 0) { $disposition = 'INVALID'; $code = 30 }
elseif ($classCount.FORBIDDEN -gt 0) { $disposition = 'FORBIDDEN_DRIFT'; $code = 20 }
elseif ($classCount.REPORT -gt 0) {
    if ($PaletteKeyAcceptance -eq 'ACCEPTED') { $disposition = 'REPORTED_ACCEPTED'; $code = 11 } else { $disposition = 'REPORT_REQUIRED_STOP'; $code = 10 }
}
else { $disposition = 'CLEAN'; $code = 0 }

$out = [ordered]@{
    Schema = 'rackcad-i64-profile-diff/v1'
    Baseline = [ordered]@{ File = $b.File; Sha256 = $b.Sha256; Phase = $b.Phase; CanonicalSha256 = $b.CanonicalSha256; SidecarOk = $b.SidecarOk; CanonicalOk = $b.CanonicalOk; SortedOk = $b.SortedOk }
    Post = [ordered]@{ File = $p.File; Sha256 = $p.Sha256; Phase = $p.Phase; CanonicalSha256 = $p.CanonicalSha256; SidecarOk = $p.SidecarOk; CanonicalOk = $p.CanonicalOk; SortedOk = $p.SortedOk }
    PaletteKeyAcceptance = $PaletteKeyAcceptance
    Preconditions = $preconditions
    ChangeCounts = $changeCount
    ClassCounts = $classCount
    Disposition = $disposition
    ExitCode = $code
    Entries = $entries
}
$text = (ConvertTo-Json $out -Depth 6).Replace("`r`n", "`n") + "`n"
$bytes = $Utf8.GetBytes($text)
$stream = [System.IO.File]::Open($OutFile, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
$sideBytes = $Utf8.GetBytes((Get-Sha256Hex $bytes) + '  ' + [System.IO.Path]::GetFileName($OutFile) + "`n")
$stream = [System.IO.File]::Open("$OutFile.sha256", [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
try { $stream.Write($sideBytes, 0, $sideBytes.Length) } finally { $stream.Dispose() }

Write-Output ("DISPOSITION={0} code={1} added={2} removed={3} changed={4} invalid={5} forbidden={6} report={7} allowed={8}" -f `
        $disposition, $code, $changeCount.ADDED, $changeCount.REMOVED, $changeCount.CHANGED, $classCount.INVALID, $classCount.FORBIDDEN, $classCount.REPORT, $classCount.ALLOWED)
exit $code
