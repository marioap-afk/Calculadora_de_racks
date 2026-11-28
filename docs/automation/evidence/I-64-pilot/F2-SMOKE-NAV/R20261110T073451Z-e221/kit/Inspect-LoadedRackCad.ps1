#Requires -Version 7.0
<#
  I-64 Smoke-1: inspeccion de SOLO LECTURA del AutoCAD autorizado justo despues del NETLOAD del Candidato (SM-00d1), LIGADA A
  AUTHORIZED_AUTOCAD_PID (BORRADOR propuesto tras el STOP de R20261104T194513Z-a578; sustituye a la inspeccion por nombre de proceso
  del kit de a578 solo si el Coordinator lo autoriza). La ejecuta la SESION, nunca AutoCAD. Lee la lista de procesos, los modulos de
  la PID autorizada y los archivos de esos modulos; escribe solo OutFile.
  VERDICT PASS solo si: el registro AUTHORIZED_AUTOCAD_PID existe; hay exactamente un proceso con ese nombre y es la misma instancia
  (PID, ticks UTC de inicio y ruta); en ESA PID hay exactamente un modulo RackCad.Plugin.dll, en el bin del Candidato y con el SHA-256
  exigido; y cada modulo RackCad* de esa PID esta en el bin del Candidato con su SHA-256 esperado. Cualquier otra cosa: STOP.
#>
param(
    [Parameter(Mandatory = $true)][string]$OutFile,
    [Parameter(Mandatory = $true)][string]$ProcessDir,
    [Parameter(Mandatory = $true)][string]$CandidateBin,
    [string]$RequiredPluginSha256 = '3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Expected = [ordered]@{
    'RackCad.Plugin.dll'      = $RequiredPluginSha256
    'RackCad.Application.dll' = '72473408A04648F5B14E71D8787B9DBCE8685B1AA8B8A5CCD72E9E38C17254A3'
    'RackCad.Domain.dll'      = '8C2B0553E940A279960557FD4AED3EF3A083045A6A3C53D6727F966528C78771'
    'RackCad.UI.dll'          = 'D2B02110E8AC4F6E57671BA8199B34B5A607A108716F587F6B447353323CC9AB'
}
$bin = [System.IO.Path]::GetFullPath($CandidateBin)
$lines = [System.Collections.Generic.List[string]]::new()
function Say([string]$t) { $lines.Add($t); Write-Output $t }
function Redact([string]$p) { $p.Replace($bin, '<candidate-bin>').Replace($env:USERPROFILE, '<USERPROFILE>') }
$verdict = 'PASS'; $why = [System.Collections.Generic.List[string]]::new()
Say "inspect-loaded-rackcad (ligada a la PID) UTC $([DateTime]::UtcNow.ToString('o'))"
$regFile = Join-Path $ProcessDir 'authorized-autocad-pid.json'
if (-not (Test-Path -LiteralPath $regFile)) { $verdict = 'STOP'; $why.Add('no hay AUTHORIZED_AUTOCAD_PID registrado') }
else {
    $a = Get-Content -LiteralPath $regFile -Raw -Encoding UTF8 | ConvertFrom-Json
    $procs = @(Get-Process -Name $a.ProcessName -ErrorAction SilentlyContinue)
    Say "AUTHORIZED_AUTOCAD_PID = $($a.Pid); $($a.ProcessName) count = $($procs.Count)"
    $p = $procs | Where-Object { $_.Id -eq [int]$a.Pid } | Select-Object -First 1
    if ($procs.Count -ne 1) { $verdict = 'STOP'; $why.Add("$($a.ProcessName) count = $($procs.Count) (debe ser 1)") }
    if (-not $p) { $verdict = 'STOP'; $why.Add("la PID autorizada $($a.Pid) no existe") }
    elseif ([long]$p.StartTime.ToUniversalTime().Ticks -ne [long]$a.StartUtcTicks -or [string]$p.Path -ne [string]$a.ExecutablePath) {
        $verdict = 'STOP'; $why.Add("la PID $($a.Pid) no es la misma instancia (inicio o ruta distintos)")
    }
    else {
        $mods = @($p.Modules | Where-Object { $_.ModuleName -like 'RackCad*' } | Sort-Object FileName)
        Say "RackCad modules en la PID $($p.Id) = $($mods.Count)"
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
        if ($plugins.Count -ne 1) { $verdict = 'STOP'; $why.Add("RackCad.Plugin.dll cargado $($plugins.Count) veces en la PID autorizada (debe ser 1)") }
        elseif ((Get-FileHash -LiteralPath $plugins[0].FileName -Algorithm SHA256).Hash -ne $RequiredPluginSha256) { $verdict = 'STOP'; $why.Add('RackCad.Plugin.dll no tiene el SHA-256 exigido') }
        else { Say ("LOADED en la PID {0}: RackCad.Plugin.dll path = {1}; sha256 = {2}" -f $p.Id, (Redact $plugins[0].FileName), $RequiredPluginSha256) }
    }
}
Say "VERDICT = $verdict"
foreach ($w in $why) { Say "  motivo: $w" }
if (-not (Test-Path -LiteralPath $OutFile)) { [System.IO.File]::WriteAllLines($OutFile, $lines, [System.Text.UTF8Encoding]::new($false)) }
if ($verdict -eq 'PASS') { exit 0 } else { exit 1 }
