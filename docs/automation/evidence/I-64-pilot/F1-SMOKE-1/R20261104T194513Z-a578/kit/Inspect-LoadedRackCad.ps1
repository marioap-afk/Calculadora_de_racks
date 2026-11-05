#Requires -Version 7.0
<#
  I-64 Smoke-1, RunId R20261104T194513Z-a578: read-only inspection of the AutoCAD process right after the Candidate NETLOAD (order of the
  Coordinator). Run by the SESSION, never by AutoCAD. Lists the modules of the single acad.exe, keeps every RackCad* module and proves the
  exact path and SHA-256 of RackCad.Plugin.dll. Reads files and the process module list only; writes only OutFile.
  Verdict PASS only if: exactly one acad.exe; exactly one RackCad.Plugin.dll module; its path is the Candidate DLL of the I-64 worktree;
  its SHA-256 is the required one. Anything else: STOP (do not continue Smoke-1).
#>
param([Parameter(Mandatory = $true)][string]$OutFile)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Required = '3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0'
$Expected = [ordered]@{
    'RackCad.Plugin.dll'      = '3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0'
    'RackCad.Application.dll' = '12B8E237F70596FA2F8030D66E51CC54F410A8906CB78B61A4056873058E27B3'
    'RackCad.Domain.dll'      = 'BFAA59433E5681A0F1453A765AE37B50ED2381993AFDE82628CCBDA7F8773959'
    'RackCad.UI.dll'          = '363AE9FE436AAE7A1DAE7E5394D86D7EF0C6C6D137943BD594BC15340CBC76E1'
}
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$bin = [System.IO.Path]::GetFullPath((Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows'))
$lines = [System.Collections.Generic.List[string]]::new()
function Say([string]$t) { $lines.Add($t); Write-Output $t }
function Redact([string]$p) { $p.Replace($wt, '<worktree>').Replace($env:USERPROFILE, '<USERPROFILE>') }

Say "inspect-loaded-rackcad UTC $([DateTime]::UtcNow.ToString('o'))"
$procs = @(Get-Process -Name acad -ErrorAction SilentlyContinue)
Say "acad.exe count = $($procs.Count)"
$verdict = 'PASS'; $why = [System.Collections.Generic.List[string]]::new()
if ($procs.Count -ne 1) { $verdict = 'STOP'; $why.Add("acad.exe count = $($procs.Count) (debe ser 1)") }
else {
    $p = $procs[0]
    Say "acad pid = $($p.Id); start UTC = $($p.StartTime.ToUniversalTime().ToString('o'))"
    $mods = @($p.Modules | Where-Object { $_.ModuleName -like 'RackCad*' } | Sort-Object FileName)
    Say "RackCad modules = $($mods.Count)"
    foreach ($m in $mods) {
        $h = (Get-FileHash -LiteralPath $m.FileName -Algorithm SHA256).Hash
        $dir = [System.IO.Path]::GetDirectoryName([System.IO.Path]::GetFullPath($m.FileName))
        $inBin = [string]::Equals($dir, $bin, [StringComparison]::OrdinalIgnoreCase)
        $exp = if ($Expected.Contains($m.ModuleName)) { $Expected[$m.ModuleName] } else { '' }
        Say ("  {0} | {1} | sha256={2} | candidateBin={3} | expected={4}" -f $m.ModuleName, (Redact $m.FileName), $h, $inBin, $(if ($exp -eq '') { 'n/a' } elseif ($h -eq $exp) { 'MATCH' } else { 'MISMATCH' }))
        if ($exp -ne '' -and $h -ne $exp) { $verdict = 'STOP'; $why.Add("$($m.ModuleName) SHA-256 distinto del Candidato") }
        if (-not $inBin) { $verdict = 'STOP'; $why.Add("$($m.ModuleName) cargado fuera del bin del Candidato") }
    }
    $plugins = @($mods | Where-Object { $_.ModuleName -ieq 'RackCad.Plugin.dll' })
    if ($plugins.Count -ne 1) { $verdict = 'STOP'; $why.Add("RackCad.Plugin.dll cargado $($plugins.Count) veces (debe ser 1)") }
    elseif ((Get-FileHash -LiteralPath $plugins[0].FileName -Algorithm SHA256).Hash -ne $Required) { $verdict = 'STOP'; $why.Add('RackCad.Plugin.dll no tiene el SHA-256 exigido') }
    else { Say ("LOADED RackCad.Plugin.dll path = {0}; sha256 = {1}" -f (Redact $plugins[0].FileName), $Required) }
}
Say "VERDICT = $verdict"
foreach ($w in $why) { Say "  motivo: $w" }
if (-not (Test-Path -LiteralPath $OutFile)) { [System.IO.File]::WriteAllLines($OutFile, $lines, [System.Text.UTF8Encoding]::new($false)) }
if ($verdict -eq 'PASS') { exit 0 } else { exit 1 }
