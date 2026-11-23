# Prueba local, con archivos falsos, de los bloques 13, 13b, 8 (solo su parte de manifiestos), 10 y 15 de smoke-run-v2.md.
# No toca la carpeta de trabajo del Smoke-PB, ningun original real ni AutoCAD. Cada bloque se ejecuta en un pwsh nuevo,
# tal cual salvo las sustituciones de abajo. Los archivos falsos van FUERA de la carpeta del paquete (dos niveles arriba
# de este guion), para que la emision no los barra.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$doc = Join-Path (Split-Path -Parent $here) 'smoke-run-v2.md'
$T = Join-Path (Split-Path -Parent (Split-Path -Parent $here)) ('_probe_sr2\t' + [DateTime]::UtcNow.ToString('HHmmss'))
New-Item -ItemType Directory -Path $T -Force | Out-Null
$root = Join-Path $T 'spb'; $orig = Join-Path $T 'orig'
foreach ($d in $orig, $root, "$root\smoke", "$root\smoke\r1", "$root\respaldo") { New-Item -ItemType Directory -Path $d | Out-Null }
# Originales falsos
$roles = @(@('D5-02'), @('D5-01'), @('D1'), @('D4'), @('D2', 'D3b', 'D5-04'), @('D5-05'), @('D3a', 'D5-03'))
$sel = @()
for ($i = 0; $i -lt 7; $i++) {
  $f = Join-Path $orig ("o$i.dwg"); [IO.File]::WriteAllText($f, "falso $i")
  $h = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash
  if ($i -eq 3) { $h = '0' * 64 }   # una entrada con el SHA-256 de la lista distinto
  $sel += [pscustomobject]@{ Source = $f; Sha256 = $h; Roles = $roles[$i]; Exists = $true }
}
$d1 = Join-Path $orig 'o2.dwg'
$list = Join-Path $T 'lista.local.json'
ConvertTo-Json -InputObject ([pscustomobject]@{ D1 = [pscustomobject]@{ Source = $d1; Sha256 = 'x'; Exists = $true }; Selection = $sel }) -Depth 5 | Out-File -LiteralPath $list -Encoding utf8
$listSha = (Get-FileHash -LiteralPath $list -Algorithm SHA256).Hash
$hd1 = (Get-FileHash -LiteralPath $d1 -Algorithm SHA256).Hash
Copy-Item -LiteralPath $d1 -Destination "$root\respaldo\D1-respaldo.dwg"
Copy-Item -LiteralPath $d1 -Destination "$root\smoke\r1\c01.dwg"
[pscustomobject]@{ original_sha256 = $hd1; respaldo = 'D1-respaldo.dwg'; copias = @([pscustomobject]@{ copia = 'c01.dwg'; sha256 = $hd1 }) } | ConvertTo-Json -Depth 3 | Out-File -LiteralPath "$root\smoke\copias.json" -Encoding utf8
# Un repositorio git falso para el bloque 10
$wt = Join-Path $T 'wt'; New-Item -ItemType Directory -Path $wt | Out-Null
& git -C $wt init -q; & git -C $wt -c user.email=t@t -c user.name=t commit -q --allow-empty -m base
$cand = (& git -C $wt rev-parse HEAD).Trim()
& git -C $wt -c user.email=t@t -c user.name=t commit -q --allow-empty -m emision
$logs = Join-Path $T 'logs'; New-Item -ItemType Directory -Path $logs | Out-Null
Set-Content -LiteralPath (Join-Path $logs 'rackcad-20261123.log') -Value @('INFO uno', 'ERROR dos', 'INFO tres')
# Los bloques
$lines = Get-Content -LiteralPath $doc -Encoding utf8
$blocks = @(); $cur = $null
foreach ($ln in $lines) {
  $s = $ln.Trim()
  if ($null -eq $cur -and $s -eq '```powershell') { $cur = New-Object System.Collections.Generic.List[string]; continue }
  if ($null -ne $cur -and $s -eq '```') { $blocks += ($cur -join "`n"); $cur = $null; continue }
  if ($null -ne $cur) { $cur.Add(($ln -replace '^   ', '')) }
}
function Get-Block([string]$mark) { $b = @($blocks | Where-Object { $_.Contains($mark) }); if ($b.Count -ne 1) { throw "marca ${mark}: $($b.Count) bloques" }; $b[0] }
function Invoke-Block([string]$name, [string]$text) {
  $text = $text.Replace("'<carpeta-del-Smoke-PB>'", "'$root\smoke'").Replace("'<raiz-de-trabajo>'", "'$root'").Replace('DD3613CB94CD4E19712D0FE2687CBDBDED2500C2AA8C4AD450D8556A26D3F2C1', $listSha)
  $text = $text.Replace("'<lista-local-de-la-seleccion>'", "'$list'").Replace("'<original-D1>'", "'$d1'").Replace("'<worktree>'", "'$wt'").Replace("'ebd304a3773495fa9071de6611936c4c01a7b06c'", "'$cand'")
  $text = $text.Replace("'<carpeta-de-registros-de-RackCad>'", "'$logs'").Replace("'<settings-de-P-05>'", "'NO-EXISTE'").Replace("'<eNN-con-estilo>'", "'e02'")
  $text = $text.Replace("'7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'", "'$hd1'")
  $f = Join-Path $T "$name.ps1"; Set-Content -LiteralPath $f -Value $text -Encoding utf8
  $out = & pwsh -NoProfile -File $f 2>&1; $rc = $LASTEXITCODE
  "===== $name rc=$rc"; $out | ForEach-Object { "  $_" }
}
Invoke-Block 'b13' (Get-Block '$s2 = @{')
Invoke-Block 'b13-otra-vez' (Get-Block '$s2 = @{')
"estilo.json:"; Get-Content -LiteralPath "$root\smoke\estilo\estilo.json" -Raw
Invoke-Block 'b13b' (Get-Block "'<eNN-con-estilo>'")
# El bloque 8, sin la parte de AutoCAD: se corta antes de leer el registro de la instancia
$b8 = Get-Block "'<anterior>'"
$b8 = $b8.Substring(0, $b8.IndexOf('$recFile')) + '$info | Format-List | Out-String' + "`nexit 0"
foreach ($pair in @(@('c01', 'e01'), @('e05', 's01'), @('s02', 'c01'), @('e01', 'e01'), @('c01', 'e09'))) {
  Invoke-Block ("b8-" + $pair[0] + '-' + $pair[1]) ($b8.Replace("'<anterior>'", "'$($pair[0])'").Replace("'<siguiente>'", "'$($pair[1])'"))
}
Invoke-Block 'b10' (Get-Block 'estilo.json: no existe')
# Un original cambiado: el bloque 10 tiene que parar
Add-Content -LiteralPath (Join-Path $orig 'o0.dwg') -Value 'cambio'
Invoke-Block 'b10-original-cambiado' (Get-Block 'estilo.json: no existe')
Invoke-Block 'b15' (Get-Block "'<settings-de-P-05>'")
"carpeta de la prueba: $T"
exit 0
