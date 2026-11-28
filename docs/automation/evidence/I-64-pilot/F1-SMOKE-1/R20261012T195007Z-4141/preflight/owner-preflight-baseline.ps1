#Requires -Version 7.0
<#
  I-64 Smoke-1, RunId R20261012T195007Z-4141: preflight + BASELINE, run by the OWNER with AutoCAD closed.
  Order of the Coordinator authorization: acad = 0; clean tree and exact Candidate; T3C tool hashes; self-test 26/26;
  SECURELOAD read read-only (reg query, independent of the capture); T3C BASELINE (exact command of T3C section 3).
  Read-only on the registry. Writes only inside the run directory. Stops at the first failure (no AutoCAD).
#>
param(
    [Parameter(Mandatory = $true)][string]$DrawingA,
    [Parameter(Mandatory = $true)][string]$DrawingB,
    [Parameter(Mandatory = $true)][string]$DrawingC
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RunId = 'R20261012T195007Z-4141'
$Candidate = '4d79e91036acdaa3e62b7b54677f394593f581a0'
$DllSha = 'E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512'
$AcadSha = '2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422'
$TrustedSha = 'a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498'
$Profile = '<<Unnamed Profile>>'
$FixtureSha = '03e5606c1e311f700640125b892fe09bf408dd5642115ebcb433b57474ccf165'
$Tools = [ordered]@{
    'Capture-ProfileState.ps1' = '750a9097d4d84a75ac1787a7bf40032c44556c64399182aac0b95694b48226e1'
    'Compare-ProfileState.ps1' = 'de642515371b19a59230940ef9659f3d84413f66c8831be201abf55fa73cbd65'
    'Test-ProfileDiff.ps1'     = 'd0bb54be83291e060aeaf4d2ec792743d70fc0f621202a1e24ee2fb41de94d8b'
}
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$prep = Join-Path $wt 'artifacts\orchestration\I-64\F1-SMOKE-1-PREP'
$run = Join-Path $wt "artifacts\orchestration\I-64\F1-SMOKE-1\$RunId"
$toolDir = Join-Path $prep 't3c\tools'
$log = Join-Path $run 'preflight\preflight-owner.txt'
$Utf8 = [System.Text.UTF8Encoding]::new($false)
function Say([string]$t) { Write-Output $t; Add-Content -LiteralPath $log -Value $t -Encoding utf8NoBOM }
function Stop-Here([string]$why) { Say "STOP: $why"; Say 'No se lanza AutoCAD. Avisa a la sesion I-64.'; exit 1 }
function LfSha([string]$path) { [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Utf8.GetBytes([System.IO.File]::ReadAllText($path).Replace("`r`n", "`n")))).ToLowerInvariant() }
function AcadCount { @(Get-Process -Name acad -ErrorAction SilentlyContinue).Count }

if (Test-Path -LiteralPath $log) { Write-Output 'STOP: el preflight de este RunId ya se ejecuto (no se repite).'; exit 1 }
Say "preflight-owner $RunId inicio UTC $([DateTime]::UtcNow.ToString('o'))"

# 1. acad = 0
$n = AcadCount; Say "1 acad.exe count = $n"; if ($n -ne 0) { Stop-Here 'hay AutoCAD abierto' }

# 2. arbol limpio y Candidato exacto
$head = (git -C $wt rev-parse HEAD).Trim(); $porc = @(git -C $wt status --porcelain).Count
$prod = @(git -C $wt diff --stat e0205b5a9f000bbd44aa1f7556d30f06411c3db7 HEAD -- src tests).Count
$dll = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll'
$dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
Say "2 HEAD=$head porcelain=$porc productDiffLines=$prod CandidateDll=$dllHash"
if ($head -ne $Candidate -or $porc -ne 0 -or $prod -ne 0 -or $dllHash -ne $DllSha) { Stop-Here 'el Candidato no es el autorizado' }

# 3. herramientas T3C con su SHA-256 aprobado
foreach ($t in $Tools.Keys) { $h = LfSha (Join-Path $toolDir $t); Say "3 $t $h"; if ($h -ne $Tools[$t]) { Stop-Here "$t no tiene su SHA-256 aprobado" } }

# 4. autoprueba 26/26
$st = Join-Path $run 'preflight\selftest-result.txt'
$out = & pwsh -NoProfile -File (Join-Path $toolDir 'Test-ProfileDiff.ps1') -WorkDir (Join-Path $run 'preflight\selftest-work') -OutFile $st
$code = $LASTEXITCODE; Say "4 selftest: $out (codigo $code) resultado SHA-256 $((Get-FileHash -LiteralPath $st -Algorithm SHA256).Hash)"
if ($code -ne 0 -or "$out" -notmatch '26/26') { Stop-Here 'la autoprueba no da 26/26' }

# 5. dibujos de prueba: SHA-256 antes del smoke (PR-07 y la autorizacion: SHA del dibujo D antes del lanzamiento)
$d = Join-Path $prep 'fixtures\I64-SMOKE1-FIXTURES-D.dxf'
$dh = (Get-FileHash -LiteralPath $d -Algorithm SHA256).Hash.ToLowerInvariant(); Say "5 D I64-SMOKE1-FIXTURES-D.dxf $dh"
if ($dh -ne $FixtureSha) { Stop-Here 'el dibujo D no tiene el SHA-256 preparado' }
foreach ($pair in @(@('A', $DrawingA), @('B', $DrawingB), @('C', $DrawingC))) {
    if (-not (Test-Path -LiteralPath $pair[1] -PathType Leaf)) { Stop-Here "no existe el dibujo $($pair[0])" }
    Say ("5 {0} {1} {2}" -f $pair[0], (Split-Path $pair[1] -Leaf), (Get-FileHash -LiteralPath $pair[1] -Algorithm SHA256).Hash.ToLowerInvariant())
}
$lib = (Get-Content -LiteralPath (Join-Path $env:APPDATA 'RackCad\settings.json') -Raw | ConvertFrom-Json).BlockLibraryPath
Say ("5 biblioteca {0} {1}" -f (Split-Path $lib -Leaf), (Get-FileHash -LiteralPath $lib -Algorithm SHA256).Hash)

# 6. SECURELOAD actual, solo lectura, con reg query (independiente de la captura)
$n = AcadCount; Say "6 acad.exe count = $n"; if ($n -ne 0) { Stop-Here 'hay AutoCAD abierto' }
$profKey = 'HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles'
$profOut = (reg query $profKey /ve 2>&1 | Out-String).TrimEnd(); Say "6 reg query $profKey /ve"; Say $profOut
$active = if ($profOut -match '\(\S+\)\s+REG_SZ\s+(.+)') { $Matches[1].Trim() } else { '' }
if ($active -ne $Profile) { Stop-Here "perfil activo '$active' distinto de $Profile" }
$varKey = "$profKey\$Profile\Variables"
$slOut = (reg query $varKey /v SECURELOAD 2>&1 | Out-String).TrimEnd(); Say "6 reg query $varKey /v SECURELOAD"; Say $slOut
$slCode = $LASTEXITCODE
$sl = if ($slOut -match 'SECURELOAD\s+REG_DWORD\s+0x([0-9a-fA-F]+)') { [Convert]::ToUInt32($Matches[1], 16).ToString([System.Globalization.CultureInfo]::InvariantCulture) }
      elseif ($slOut -match 'SECURELOAD\s+REG_\w+\s+(\S.*)') { $Matches[1].Trim() } elseif ($slCode -ne 0) { 'ABSENT' } else { '' }
if ($sl -eq '') { Stop-Here 'no se pudo leer SECURELOAD' }
$slFile = Join-Path $run 'preflight\SECURELOAD_BASELINE_CURRENT.txt'
Set-Content -LiteralPath $slFile -Value ("SECURELOAD_BASELINE_CURRENT=$sl`nSource=reg query $varKey /v SECURELOAD (solo lectura; perfil activo $active)`nReadUtc=$([DateTime]::UtcNow.ToString('o'))") -Encoding utf8NoBOM
Say "6 SECURELOAD_BASELINE_CURRENT = $sl"

# 7. BASELINE (orden exacta de T3C seccion 3)
$n = AcadCount; Say "7 acad.exe count = $n"; if ($n -ne 0) { Stop-Here 'hay AutoCAD abierto' }
$cap = Join-Path $toolDir 'Capture-ProfileState.ps1'
$cons = Join-Path $run 'console\baseline.txt'
Set-Content -LiteralPath $cons -Value ("pwsh -NoProfile -File <prep>\t3c\tools\Capture-ProfileState.ps1 -Phase BASELINE -OutDir <run>\profile -RunLabel $RunId -CandidateDll <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll -ExpectTrustedPathsSha256 $TrustedSha -ExpectSecureLoad $sl -ExpectProfile '$Profile' -ExpectAcadSha256 $AcadSha") -Encoding utf8NoBOM
$bout = & pwsh -NoProfile -File $cap -Phase BASELINE -OutDir (Join-Path $run 'profile') -RunLabel $RunId -CandidateDll $dll `
    -ExpectTrustedPathsSha256 $TrustedSha -ExpectSecureLoad $sl -ExpectProfile $Profile -ExpectAcadSha256 $AcadSha
$bcode = $LASTEXITCODE
Add-Content -LiteralPath $cons -Value ($bout -join "`n") -Encoding utf8NoBOM
Add-Content -LiteralPath $cons -Value "exit=$bcode" -Encoding utf8NoBOM
Say "7 BASELINE codigo $bcode"; $bout | ForEach-Object { Say "  $_" }
if ($bcode -ne 0) { Stop-Here "la BASELINE devolvio el codigo $bcode" }
Say "OK: preflight y BASELINE correctos. NO abras AutoCAD hasta que la sesion I-64 confirme la BASELINE."
