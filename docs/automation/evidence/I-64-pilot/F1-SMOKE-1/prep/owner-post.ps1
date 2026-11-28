#Requires -Version 7.0
<#
  I-64 Smoke-1, RunId R20261013T011654Z-184b: POST (T3C seccion 4), run by the OWNER after AutoCAD is fully closed.
  Same Candidate DLL and the same expectations as the BASELINE (profile, acad.exe, TRUSTEDPATHS chain, SECURELOAD_BASELINE_CURRENT).
  Read-only on the registry. Writes only the POST files and console\post.txt in the run directory. Never forces AutoCAD to close.
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RunId = 'R20261013T011654Z-184b'
$AcadSha = '2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422'
$TrustedSha = 'a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498'
$Profile = '<<Unnamed Profile>>'
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$run = Join-Path $wt "artifacts\orchestration\I-64\F1-SMOKE-1\$RunId"
$cap = Join-Path $wt 'artifacts\orchestration\I-64\F1-SMOKE-1-PREP\t3c\tools\Capture-ProfileState.ps1'
$dll = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll'
$cons = Join-Path $run 'console\post.txt'

$n = @(Get-Process -Name acad -ErrorAction SilentlyContinue).Count
if ($n -ne 0) { Write-Output "AutoCAD sigue abierto ($n proceso). Cierralo del todo sin guardar y vuelve a ejecutar; no se escribio nada."; exit 3 }
if (Test-Path -LiteralPath $cons) { Write-Output 'STOP: el POST de este RunId ya se tomo (no se repite).'; exit 1 }
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
