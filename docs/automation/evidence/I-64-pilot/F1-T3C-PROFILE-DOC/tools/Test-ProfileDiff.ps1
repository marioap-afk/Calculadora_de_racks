#Requires -Version 7.0
<#
.SYNOPSIS
  I-64 F1-T3C-PROFILE-DOC: autoprueba de Compare-ProfileState.ps1 con capturas SINTETICAS (sin registro ni AutoCAD).

.DESCRIPTION
  Construye pares BASELINE/POST sinteticos en -WorkDir (que no debe existir; se crea) y comprueba, caso por caso, el
  codigo de salida y las reglas que aplica Compare-ProfileState.ps1. Escribe el resultado en -OutFile (nuevo) sin rutas
  ni marcas de tiempo, de modo que dos ejecuciones dan el mismo archivo. Codigo 0 si todos los casos pasan; 1 si no.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$WorkDir,
    [Parameter(Mandatory = $true)][string]$OutFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Utf8 = [System.Text.UTF8Encoding]::new($false)
$Compare = Join-Path $PSScriptRoot 'Compare-ProfileState.ps1'

function Get-TextSha([string]$Text) { return [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Utf8.GetBytes($Text))).ToLowerInvariant() }
function LfSha([string]$Path) { return Get-TextSha ([System.IO.File]::ReadAllText($Path).Replace("`r`n", "`n")) }

if (Test-Path -LiteralPath $WorkDir) { Write-Output 'ERROR: -WorkDir no debe existir.'; exit 2 }
if (Test-Path -LiteralPath $OutFile) { Write-Output 'ERROR: -OutFile ya existe.'; exit 2 }
[void](New-Item -ItemType Directory -Path $WorkDir)

$P = 'HKCU:PRODUCT\Profiles\<PROFILE>'
$Base = @(
    'F|Schema|rackcad-i64-profile-capture/v1', ('F|ToolSha256|' + (Get-TextSha 'tool')), 'F|AutoCADRelease|R25.0', 'F|ProductKey|ACAD-8101:409',
    'F|AcadProcessCount|0', 'F|AcadExeFileVersion|R25.0.171.0.0', ('F|AcadExeSha256|' + (Get-TextSha 'acad')),
    'F|CurrentProfile|<<Unnamed Profile>>', 'F|ProfileCount|1', 'F|ProfileSet|<<Unnamed Profile>>', 'F|SECURELOAD|1',
    'F|TRUSTEDPATHS.EntryCount|2', 'F|TRUSTEDPATHS.EmptySegments|0', ('F|TRUSTEDPATHS.Entry.001|' + (Get-TextSha 'a')), ('F|TRUSTEDPATHS.Entry.002|' + (Get-TextSha 'b')),
    ('F|TRUSTEDPATHS.ChainSha256|' + (Get-TextSha 'a;b')), 'F|PICKFIRST|ABSENT', 'F|CandidateDll.FileName|RackCad.Plugin.dll', ('F|CandidateDll.Sha256|' + (Get-TextSha 'dll')),
    'F|TRUSTEDPATHS.CoversCandidateDllFolder|false', 'F|RackCadEnvUser|0', 'F|RackCadEnvMachine|0', 'F|Unreadable|0',
    'F|RootPresent|RACKCAD_APPDATA|true', 'F|RootPresent|RACKCAD_LOCALAPPDATA|false', 'F|RootPresent|PLUGINS_APPDATA|true', 'F|RootPresent|PLUGINS_PROGRAMFILES|true',
    "K|$P", "K|$P\Variables", ("V|$P\Variables|SECURELOAD|String|1|" + (Get-TextSha '1')), ("V|$P\Variables|TRUSTEDPATHS|String|3|" + (Get-TextSha 'a;b')),
    "K|$P\Dialogs", "K|$P\Dialogs\RackCad.UI.Systems.Selective.RackSelectiveWindow",
    ("V|$P\Dialogs\RackCad.UI.Systems.Selective.RackSelectiveWindow|Placement|String|4|" + (Get-TextSha 'p1')),
    "K|$P\Drawing Window", ("V|$P\Drawing Window|WindowState|DWord|1|" + (Get-TextSha '3')),
    'K|HKCU:PRODUCT\FixedProfile', 'K|HKCU:PRODUCT\Applications', 'K|HKCU:PRODUCT\Applications\AcadVBA', 'K|HKCU:PRODUCT\Loaded', 'K|HKLM:PRODUCT\Applications',
    'D|RACKCAD_APPDATA|logs/|DIR|-', ('D|RACKCAD_APPDATA|logs/rackcad.log|100|' + (Get-TextSha 'log1')), ('D|RACKCAD_APPDATA|settings.json|50|' + (Get-TextSha 'set1')),
    'P|PLUGINS_PROGRAMFILES|AcVBA2025.Bundle|DIR'
)

function Write-Capture([string]$Path, [string]$Phase, [string[]]$Ledger, [string]$Tamper) {
    [string[]]$l = $Ledger.Clone()
    [System.Array]::Sort($l, [System.StringComparer]::Ordinal)
    $canon = Get-TextSha ([string]::Join("`n", $l) + "`n")
    if ($Tamper -eq 'canonical') { $l = @($l + 'K|HKCU:PRODUCT\FixedProfile\Injected'); [System.Array]::Sort($l, [System.StringComparer]::Ordinal) }
    $doc = [ordered]@{ Schema = 'rackcad-i64-profile-capture/v1'; Phase = $Phase; RunLabel = 'SELFTEST'; CanonicalSha256 = $canon; LedgerLineCount = $l.Length; Ledger = $l }
    $bytes = $Utf8.GetBytes((ConvertTo-Json $doc -Depth 4).Replace("`r`n", "`n") + "`n")
    [System.IO.File]::WriteAllBytes($Path, $bytes)
    $sha = [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    if ($Tamper -eq 'sidecar') { $sha = '0' * 64 }
    [System.IO.File]::WriteAllText("$Path.sha256", $sha + '  ' + [System.IO.Path]::GetFileName($Path) + "`n", $Utf8)
}

function Edit-Ledger([string[]]$Ledger, [string[]]$Remove, [string[]]$Add) {
    $out = [System.Collections.Generic.List[string]]::new()
    foreach ($x in $Ledger) { if ($Remove -notcontains $x) { $out.Add($x) } }
    foreach ($x in $Add) { $out.Add($x) }
    return , $out.ToArray()
}

$tp = "V|$P\Variables|TRUSTEDPATHS|String|3|" + (Get-TextSha 'a;b')
$Cases = @(
    @{ Id = 'T01-identical'; Remove = @(); Add = @(); Code = 0; Rules = @() }
    @{ Id = 'T02-trustedpaths-value'; Remove = @($tp, ('F|TRUSTEDPATHS.ChainSha256|' + (Get-TextSha 'a;b'))); Add = @(("V|$P\Variables|TRUSTEDPATHS|String|5|" + (Get-TextSha 'a;b;c')), ('F|TRUSTEDPATHS.ChainSha256|' + (Get-TextSha 'a;b;c'))); Code = 20; Rules = @('R-F-03') }
    @{ Id = 'T03-secureload'; Remove = @('F|SECURELOAD|1'); Add = @('F|SECURELOAD|0'); Code = 20; Rules = @('R-F-02') }
    @{ Id = 'T04-current-profile'; Remove = @('F|CurrentProfile|<<Unnamed Profile>>'); Add = @('F|CurrentProfile|SHA256:' + (Get-TextSha 'other')); Code = 20; Rules = @('R-F-01') }
    @{ Id = 'T05-application-registered'; Remove = @(); Add = @('K|HKCU:PRODUCT\Applications\RackCad', ('V|HKCU:PRODUCT\Applications\RackCad|LOADCTRLS|DWord|1|' + (Get-TextSha '2'))); Code = 20; Rules = @('R-F-05') }
    @{ Id = 'T06-bundle-installed'; Remove = @(); Add = @('P|PLUGINS_APPDATA|RackCad.bundle|DIR'); Code = 20; Rules = @('R-F-06') }
    @{ Id = 'T07-settings-changed'; Remove = @(('D|RACKCAD_APPDATA|settings.json|50|' + (Get-TextSha 'set1'))); Add = @(('D|RACKCAD_APPDATA|settings.json|51|' + (Get-TextSha 'set2'))); Code = 20; Rules = @('R-F-08') }
    @{ Id = 'T08-racklog-grows'; Remove = @(('D|RACKCAD_APPDATA|logs/rackcad.log|100|' + (Get-TextSha 'log1'))); Add = @(('D|RACKCAD_APPDATA|logs/rackcad.log|180|' + (Get-TextSha 'log2'))); Code = 0; Rules = @('R-A-01') }
    @{ Id = 'T09-palette-key'; Remove = @(); Add = @("K|$P\Dialogs\RackCad", ("V|$P\Dialogs\RackCad|Dock|DWord|1|" + (Get-TextSha '1'))); Code = 10; Rules = @('R-R-01') }
    @{ Id = 'T10-palette-key-accepted'; Remove = @(); Add = @("K|$P\Dialogs\RackCad", ("V|$P\Dialogs\RackCad|Dock|DWord|1|" + (Get-TextSha '1'))); Code = 11; Rules = @('R-R-01'); Accept = $true }
    @{ Id = 'T11-existing-window-placement'; Remove = @(("V|$P\Dialogs\RackCad.UI.Systems.Selective.RackSelectiveWindow|Placement|String|4|" + (Get-TextSha 'p1'))); Add = @(("V|$P\Dialogs\RackCad.UI.Systems.Selective.RackSelectiveWindow|Placement|String|4|" + (Get-TextSha 'p2'))); Code = 0; Rules = @('R-A-02') }
    @{ Id = 'T12-guid-key'; Remove = @(); Add = @("K|$P\Dialogs\{00000000-0000-0000-0000-000000000000}"); Code = 10; Rules = @('R-R-02') }
    @{ Id = 'T13-autocad-activity'; Remove = @(("V|$P\Drawing Window|WindowState|DWord|1|" + (Get-TextSha '3'))); Add = @(("V|$P\Drawing Window|WindowState|DWord|1|" + (Get-TextSha '1'))); Code = 0; Rules = @('R-A-03') }
    @{ Id = 'T14-acad-identity'; Remove = @(('F|AcadExeSha256|' + (Get-TextSha 'acad'))); Add = @(('F|AcadExeSha256|' + (Get-TextSha 'acad2'))); Code = 30; Rules = @('R-INV-02') }
    @{ Id = 'T15-sidecar-tampered'; Remove = @(); Add = @(); Code = 30; Rules = @('R-INV-04'); Tamper = 'sidecar' }
    @{ Id = 'T16-ledger-tampered'; Remove = @(); Add = @(); Code = 30; Rules = @('R-INV-04'); Tamper = 'canonical' }
    @{ Id = 'T17-persistent-env-var'; Remove = @('F|RackCadEnvUser|0'); Add = @('F|RackCadEnvUser|1', ('E|USER|RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT|' + (Get-TextSha '2'))); Code = 20; Rules = @('R-F-07') }
    @{ Id = 'T18-pickfirst'; Remove = @('F|PICKFIRST|ABSENT'); Add = @('F|PICKFIRST|0'); Code = 20; Rules = @('R-F-04') }
    @{ Id = 'T19-workspace-dialog'; Remove = @(); Add = @("K|$P\Dialogs\RackCad.UI.Workspace.WorkspacePanelView"); Code = 10; Rules = @('R-R-01') }
    @{ Id = 'T20-fixedprofile-rackcad'; Remove = @(); Add = @('K|HKCU:PRODUCT\FixedProfile\RackCad'); Code = 10; Rules = @('R-R-01') }
    @{ Id = 'T21-acad-running-post'; Remove = @('F|AcadProcessCount|0'); Add = @('F|AcadProcessCount|1'); Code = 30; Rules = @('R-INV-01') }
    @{ Id = 'T22-unreadable'; Remove = @('F|Unreadable|0'); Add = @('F|Unreadable|1', "V|$P\Variables|LISPSYS|UNREADABLE|0|UNREADABLE"); Code = 30; Rules = @('R-INV-03') }
    @{ Id = 'T23-new-window-type'; Remove = @(); Add = @("K|$P\Dialogs\RackCad.UI.RackListWindow"); Code = 10; Rules = @('R-R-01') }
    @{ Id = 'T24-localappdata-layout'; Remove = @('F|RootPresent|RACKCAD_LOCALAPPDATA|false'); Add = @('F|RootPresent|RACKCAD_LOCALAPPDATA|true', ('D|RACKCAD_LOCALAPPDATA|RackFrameConfigurator.layout|10|' + (Get-TextSha 'lay'))); Code = 20; Rules = @('R-F-08') }
    @{ Id = 'T25-loaded-entry'; Remove = @(); Add = @('K|HKCU:PRODUCT\Loaded\<APPDATA>/Autodesk/ApplicationPlugins/RackCad.bundle'); Code = 20; Rules = @('R-F-05') }
    @{ Id = 'T26-candidate-dll-changed'; Remove = @(('F|CandidateDll.Sha256|' + (Get-TextSha 'dll'))); Add = @(('F|CandidateDll.Sha256|' + (Get-TextSha 'dll2'))); Code = 30; Rules = @('R-INV-02') }
)

$results = [System.Collections.Generic.List[object]]::new()
$passed = 0
foreach ($c in $Cases) {
    $dir = Join-Path $WorkDir $c.Id
    [void](New-Item -ItemType Directory -Path $dir)
    $bf = Join-Path $dir 'baseline.json'
    $pf = Join-Path $dir 'post.json'
    $of = Join-Path $dir 'diff.json'
    Write-Capture $bf 'BASELINE' $Base ''
    $tamper = if ($c.ContainsKey('Tamper')) { $c.Tamper } else { '' }
    Write-Capture $pf 'POST' (Edit-Ledger $Base $c.Remove $c.Add) $tamper
    $argsList = @('-NoProfile', '-File', $Compare, '-Baseline', $bf, '-Post', $pf, '-OutFile', $of)
    if ($c.ContainsKey('Accept') -and $c.Accept) { $argsList += @('-PaletteKeyAcceptance', 'ACCEPTED') }
    $null = & pwsh @argsList
    $code = $LASTEXITCODE
    $diff = Get-Content -LiteralPath $of -Raw | ConvertFrom-Json
    $rules = [System.Collections.Generic.SortedSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($e in @($diff.Entries)) { if ($null -ne $e) { [void]$rules.Add([string]$e.Rule) } }
    foreach ($e in @($diff.Preconditions)) { if ($null -ne $e) { [void]$rules.Add([string]$e.Rule) } }
    $missing = @($c.Rules | Where-Object { -not $rules.Contains($_) })
    $ok = ($code -eq $c.Code) -and ($missing.Count -eq 0)
    if ($ok) { $passed++ }
    $results.Add([ordered]@{
            Id = $c.Id; ExpectedCode = $c.Code; ExpectedRules = $c.Rules; ActualCode = $code; ActualDisposition = [string]$diff.Disposition
            ActualRules = @($rules); Result = $(if ($ok) { 'PASS' } else { 'FAIL' })
        })
}

$out = [ordered]@{
    Schema = 'rackcad-i64-profile-diff-selftest/v1'
    CompareSha256 = LfSha $Compare
    SelfTestSha256 = LfSha $PSCommandPath
    Total = $Cases.Count
    Passed = $passed
    Result = $(if ($passed -eq $Cases.Count) { 'PASS' } else { 'FAIL' })
    Cases = $results
}
$bytes = $Utf8.GetBytes((ConvertTo-Json $out -Depth 5).Replace("`r`n", "`n") + "`n")
$stream = [System.IO.File]::Open($OutFile, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
try { $stream.Write($bytes, 0, $bytes.Length) } finally { $stream.Dispose() }
Write-Output ("SELFTEST {0}: {1}/{2}" -f $out.Result, $passed, $Cases.Count)
if ($passed -eq $Cases.Count) { exit 0 } else { exit 1 }
