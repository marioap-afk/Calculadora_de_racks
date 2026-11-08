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
    [string]$RequiredPluginSha256 = '0C0F2FDE0987C489BABAF2ACB9A46936117D044EAF45C706A303E6B5B1C56C0B'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Expected = [ordered]@{
    'RackCad.Plugin.dll'      = $RequiredPluginSha256
    'RackCad.Application.dll' = '12A7C29A851105F0A2CC067EC899CDB4BC98325FA1DC57845D89ED974BBE7558'
    'RackCad.Domain.dll'      = '24EA6643CAD865D6F99043AD13F7BAA11CD5257CD9EE27DCB83D9DE46CD79507'
    'RackCad.UI.dll'          = 'E49AC0A6C4336A7E73EF7C5EC649DDCAC278FF10778C6861D9BF85C5C02D9826'
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
