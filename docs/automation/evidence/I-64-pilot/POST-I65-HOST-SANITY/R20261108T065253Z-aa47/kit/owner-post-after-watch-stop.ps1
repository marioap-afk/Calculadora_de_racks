#Requires -Version 7.0
<#
  I-64 POST-I65 HOST SANITY CHECK, RunId R20261108T065253Z-aa47: HOST-EXTRAS AFTER y POST (T3C seccion 4), ejecutado por la SESION
  con AutoCAD cerrado del todo. Guarda PG-6: exige process\closed.ok y una bitacora de vigilancia sin STOP. Orden de la autorizacion: acad = 0; captura de AssemblyMap y CoreUser otra vez; despues el POST de T3C con el mismo
  DLL y las mismas expectativas que la BASELINE (perfil, acad.exe, cadena TRUSTEDPATHS, SECURELOAD_BASELINE_CURRENT).
  Solo lectura en el registro. Escribe solo en el directorio del RunId. Nunca fuerza el cierre de AutoCAD.
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RunId = 'R20261108T065253Z-aa47'
$AcadSha = '2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422'
$TrustedSha = 'a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498'
$Profile = '<<Unnamed Profile>>'
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$run = Join-Path $wt "artifacts\orchestration\I-64\POST-I65-HOST-SANITY\$RunId"
$cap = Join-Path $wt 'artifacts\orchestration\I-64\F1-SMOKE-1-PREP\t3c\tools\Capture-ProfileState.ps1'
$dll = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll'
$cons = Join-Path $run 'console\post.txt'

$n = @(Get-Process -Name acad -ErrorAction SilentlyContinue).Count
if ($n -ne 0) { Write-Output "AutoCAD sigue abierto ($n proceso). Cierralo del todo sin guardar y vuelve a ejecutar; no se escribio nada."; exit 3 }
if (Test-Path -LiteralPath $cons) { Write-Output 'STOP: el POST de este RunId ya se tomo (no se repite).'; exit 1 }
# PG-6: cierre normal de la PID autorizada (closed.ok) y vigilancia sin STOP antes de capturar
$proc = Join-Path $run 'process'
if (-not (Test-Path -LiteralPath (Join-Path $proc 'closed.ok'))) { Write-Output 'STOP: falta process\closed.ok (PG-5); no se toma el POST.'; exit 1 }
$wl = Join-Path $proc 'acad-watch.log'
if (-not (Test-Path -LiteralPath $wl)) { Write-Output 'STOP: falta process\acad-watch.log; no se toma el POST.'; exit 1 }
$stops = @(Get-Content -LiteralPath $wl -Encoding UTF8 | Where-Object { $_ -match 'STOP' })
if ($stops.Count -ne 0) { Write-Output "PG-6 NO APLICADA (desviacion declarada, procedimiento aa47: POST y DIFF tambien tras un STOP): la bitacora tiene $($stops.Count) linea(s) STOP, conservadas sin cambios:"; $stops | ForEach-Object { Write-Output "  $_" } }
"PG-6 closed.ok presente; acad-watch.log presente (lineas STOP listadas arriba)"
"acad.exe count = $n; RAMElements.exe count = $(@(Get-Process -Name RAMElements -ErrorAction SilentlyContinue).Count)"

# 1. HOST-EXTRAS AFTER (AssemblyMap y CoreUser, solo lectura), antes del POST de T3C
$hxFile = Join-Path $run 'host-extras\host-extras-AFTER.json'
if (-not (Test-Path -LiteralPath $hxFile)) {
    & pwsh -NoProfile -File (Join-Path $run 'kit\Capture-HostExtras.ps1') -Phase AFTER -OutFile $hxFile
    $hxCode = $LASTEXITCODE
    "HOST-EXTRAS AFTER codigo $hxCode"
    if ($hxCode -ne 0) { 'STOP: la captura HOST-EXTRAS AFTER fallo; no se toma el POST. Avisa a la sesion I-64.'; exit 1 }
}

# 2. POST de T3C
$slLine = Get-Content -LiteralPath (Join-Path $run 'preflight\SECURELOAD_BASELINE_CURRENT.txt') | Where-Object { $_ -like 'SECURELOAD_BASELINE_CURRENT=*' }
$sl = $slLine.Substring('SECURELOAD_BASELINE_CURRENT='.Length)
Set-Content -LiteralPath $cons -Value ("pwsh -NoProfile -File <prep>\t3c\tools\Capture-ProfileState.ps1 -Phase POST -OutDir <run>\profile -RunLabel $RunId -CandidateDll <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -ExpectTrustedPathsSha256 $TrustedSha -ExpectSecureLoad $sl -ExpectProfile '$Profile' -ExpectAcadSha256 $AcadSha") -Encoding utf8NoBOM
$out = & pwsh -NoProfile -File $cap -Phase POST -OutDir (Join-Path $run 'profile') -RunLabel $RunId -CandidateDll $dll `
    -ExpectTrustedPathsSha256 $TrustedSha -ExpectSecureLoad $sl -ExpectProfile $Profile -ExpectAcadSha256 $AcadSha
$code = $LASTEXITCODE
Add-Content -LiteralPath $cons -Value ($out -join "`n") -Encoding utf8NoBOM
Add-Content -LiteralPath $cons -Value "exit=$code" -Encoding utf8NoBOM
$out
"POST codigo $code (0 o 5: la sesion I-64 sigue con el DIFF; 2, 3, 4 o 6: POST INVALID)"
