#Requires -Version 7.0
<#
  I-64 Smoke-NAV, RunId R20261110T073451Z-e221 (Candidato 003be859, producto 82bd49b6):
  preflight + capturas de host + BASELINE, ejecutado por la SESION (operador autorizado bajo OPERATOR_EXCEPTION) con AutoCAD cerrado. Orden de la autorizacion del Coordinator: acad = 0 y RAM Elements = 0; arbol limpio, HEAD = 1917bc19 y
  Candidato probado (sin delta en src/, tests/ ni archivos de proyecto desde el Candidato; identidad del Candidato estampada en el DLL; SHA exacto); SHA-256 de las herramientas T3C; autoprueba 26/26; SHA-256 de los dibujos; SECURELOAD leido en solo lectura (reg query,
  independiente de la captura) = SECURELOAD_BASELINE_CURRENT; captura de solo lectura de AssemblyMap, CoreUser y del historial de NETLOAD (HOST-EXTRAS BEFORE);
  BASELINE de T3C (orden exacta de T3C seccion 3), que vuelve a leer SECURELOAD por su cuenta y para si no coincide.
  Solo lectura en el registro. Escribe solo dentro del directorio del RunId. Para en el primer fallo (sin AutoCAD).
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RunId = 'R20261110T073451Z-e221'
$Candidate = '003be859002901934754ba54b9fad38a761c32ca'
$DllSha = '63A6ABAAE99A9A8BC379C8087A26F9ADE95792F0FDC660947FD237C805F6A2BC'
$HeadExpected = '003be859002901934754ba54b9fad38a761c32ca'
$AcadSha = '2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422'
$TrustedSha = 'a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498'
$Profile = '<<Unnamed Profile>>'
$FixtureN = '83a633adae534666849cdce82a5904ef98e2ec59123384bbc072b8282a068063'
$FixtureN2 = 'd43f7e33e2ac3700b0d55a7ad5ba6e97e7c51304920b73acfbfc6f008b5e0a40'
$Tools = [ordered]@{
    'Capture-ProfileState.ps1' = '750a9097d4d84a75ac1787a7bf40032c44556c64399182aac0b95694b48226e1'
    'Compare-ProfileState.ps1' = 'de642515371b19a59230940ef9659f3d84413f66c8831be201abf55fa73cbd65'
    'Test-ProfileDiff.ps1'     = 'd0bb54be83291e060aeaf4d2ec792743d70fc0f621202a1e24ee2fb41de94d8b'
}
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$prep = Join-Path $wt 'artifacts\orchestration\I-64\F1-SMOKE-1-PREP'
$run = Join-Path $wt "artifacts\orchestration\I-64\F2-SMOKE-NAV\$RunId"
$toolDir = Join-Path $prep 't3c\tools'
$log = Join-Path $run 'preflight\preflight-owner.txt'
$Utf8 = [System.Text.UTF8Encoding]::new($false)
function Say([string]$t) { Write-Output $t; Add-Content -LiteralPath $log -Value $t -Encoding utf8NoBOM }
function Stop-Here([string]$why) { Say "STOP: $why"; Say 'No se abre AutoCAD (STOP antes de AutoCAD).'; exit 1 }
function LfSha([string]$path) { [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Utf8.GetBytes([System.IO.File]::ReadAllText($path).Replace("`r`n", "`n")))).ToLowerInvariant() }
function AcadCount { @(Get-Process -Name acad -ErrorAction SilentlyContinue).Count }
function RamCount { @(Get-Process -Name RAMElements -ErrorAction SilentlyContinue).Count }
function CheckProcs([string]$step) {
    $a = AcadCount; $r = RamCount; Say "$step acad.exe count = $a; RAMElements.exe count = $r"
    if ($a -ne 0) { Stop-Here 'hay AutoCAD abierto' }
    if ($r -ne 0) { Stop-Here 'hay RAM Elements abierto' }
}

if (-not (Test-Path -LiteralPath $run -PathType Container)) { Write-Output 'ERROR: no existe el directorio de este RunId.'; exit 2 }
if (Test-Path -LiteralPath $log) { Write-Output 'STOP: el preflight de este RunId ya se ejecuto (no se repite).'; exit 1 }
Say "preflight-owner $RunId inicio UTC $([DateTime]::UtcNow.ToString('o'))"

# 1. acad = 0 y RAM Elements = 0 (precondicion de la autorizacion; si alguno sigue abierto NO se toma la BASELINE)
CheckProcs '1'

# 2. arbol limpio; HEAD = Candidato
$head = (git -C $wt rev-parse HEAD).Trim(); $porc = @(git -C $wt status --porcelain).Count
git -C $wt merge-base --is-ancestor $Candidate HEAD; $anc = ($LASTEXITCODE -eq 0)
$dSrc = @(git -C $wt diff --name-only $Candidate HEAD -- src).Count
$dTests = @(git -C $wt diff --name-only $Candidate HEAD -- tests).Count
$dProj = @(git -C $wt diff --name-only $Candidate HEAD -- '*.csproj' '*.props' '*.targets' '*.sln' 'global.json' 'nuget.config' 'NuGet.config' 'Directory.*').Count
$dOther = @(git -C $wt diff --name-only $Candidate HEAD | Where-Object { $_ -notlike 'docs/automation/*' }).Count
$dll = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll'
$dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
$pv = (Get-Item -LiteralPath $dll).VersionInfo.ProductVersion
Say "2 HEAD=$head (esperado $HeadExpected) porcelain=$porc CandidateAncestor=$anc deltaSrc=$dSrc deltaTests=$dTests deltaProjectFiles=$dProj deltaOutsideDocsAutomation=$dOther"
Say "2 CandidateDll sha256=$dllHash ProductVersion=$pv"
if ($head -ne $HeadExpected -or $porc -ne 0 -or -not $anc -or $dSrc -ne 0 -or $dTests -ne 0 -or $dProj -ne 0 -or $dOther -ne 0) { Stop-Here 'el arbol o la rama no son los autorizados' }
if ($dllHash -ne $DllSha -or $pv -notlike "*+$Candidate") { Stop-Here 'el DLL no es el del Candidato autorizado (SHA o identidad estampada)' }

# 3. herramientas T3C con su SHA-256 aprobado
foreach ($t in $Tools.Keys) { $h = LfSha (Join-Path $toolDir $t); Say "3 $t $h"; if ($h -ne $Tools[$t]) { Stop-Here "$t no tiene su SHA-256 aprobado" } }

# 4. autoprueba 26/26
$st = Join-Path $run 'preflight\selftest-result.txt'
$out = & pwsh -NoProfile -File (Join-Path $toolDir 'Test-ProfileDiff.ps1') -WorkDir (Join-Path $run 'preflight\selftest-work') -OutFile $st
$code = $LASTEXITCODE; Say "4 selftest: $out (codigo $code) resultado SHA-256 $((Get-FileHash -LiteralPath $st -Algorithm SHA256).Hash)"
if ($code -ne 0 -or "$out" -notmatch '26/26') { Stop-Here 'la autoprueba no da 26/26' }

# 5. dibujos de prueba: SHA-256 antes del hito
foreach ($pair in @(@('I64-SNAV-N.dxf', $FixtureN), @('I64-SNAV-N2.dxf', $FixtureN2))) {
    $f = Join-Path $run ('fixtures\' + $pair[0]); $h = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash.ToLowerInvariant(); Say ("5 {0} {1}" -f $pair[0], $h)
    if ($h -ne $pair[1]) { Stop-Here "el dibujo $($pair[0]) no tiene el SHA-256 preparado" }
}
$lib = (Get-Content -LiteralPath (Join-Path $env:APPDATA 'RackCad\settings.json') -Raw | ConvertFrom-Json).BlockLibraryPath
Say ("5 biblioteca {0} {1}" -f (Split-Path $lib -Leaf), (Get-FileHash -LiteralPath $lib -Algorithm SHA256).Hash)

# 6. SECURELOAD actual, solo lectura, con reg query (lectura del Owner, independiente de la captura)
CheckProcs '6'
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

# 6b. HOST-EXTRAS BEFORE: AssemblyMap, CoreUser e historial de NETLOAD, solo lectura (orden del Coordinator: antes de la BASELINE)
CheckProcs '6b'
$hx = & pwsh -NoProfile -File (Join-Path $run 'kit\Capture-HostExtras.ps1') -Phase BEFORE -OutFile (Join-Path $run 'host-extras\host-extras-BEFORE.json')
$hxCode = $LASTEXITCODE; Say "6b HOST-EXTRAS BEFORE codigo $hxCode"; $hx | ForEach-Object { Say "  $_" }
if ($hxCode -ne 0) { Stop-Here "la captura HOST-EXTRAS BEFORE devolvio el codigo $hxCode" }

# 7. BASELINE (orden exacta de T3C seccion 3)
CheckProcs '7'
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
Say "OK: preflight, HOST-EXTRAS BEFORE y BASELINE correctos. AutoCAD solo se abre tras verificar la BASELINE y con la guarda PG-1a (acad = 0)."
