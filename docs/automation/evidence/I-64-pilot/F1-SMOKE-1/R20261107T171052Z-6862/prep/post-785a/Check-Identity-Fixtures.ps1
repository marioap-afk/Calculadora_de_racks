#Requires -Version 7.0
# I-64 preparacion tras 785a. Solo lectura: identidad de dibujos, biblioteca y herramientas frente a 785a; delta de la rama; MBUTTONPAN del perfil.
# Escribe OutFile sin rutas locales ni nombres de archivos ajenos. No toca el registro, el perfil ni los dibujos.
param([Parameter(Mandatory = $true)][string]$OutFile)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$orch = Join-Path $wt 'artifacts\orchestration\I-64'
$kit = Join-Path $orch 'F1-SMOKE-1\R20261105T050250Z-785a\kit'
$pg = Join-Path $orch 'F1-SMOKE-1-RETRY-PREP\host-recheck-after-a578\process-guard'
$candidate = 'cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6'
$out = [System.Collections.Generic.List[string]]::new()
function Say([string]$t) { $out.Add($t); Write-Output $t }
function Sha([string]$p) { (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash }
Say "identidad UTC $([DateTime]::UtcNow.ToString('o')); acad = $(@(Get-Process -Name acad -ErrorAction SilentlyContinue).Count); RAM = $(@(Get-Process -Name RAMElements -ErrorAction SilentlyContinue).Count)"

# Dibujos y biblioteca, con los SHA de 785a (SM-41)
$dir = '<unidad-local>\Dibujos'
$lib = (Get-Content -LiteralPath (Join-Path $env:APPDATA 'RackCad\settings.json') -Raw | ConvertFrom-Json).BlockLibraryPath
$items = [ordered]@{
    A = @((Join-Path $dir 'Dibujo A.dwg'), '90750E8B', '4852')
    B = @((Join-Path $dir 'Dibujo B.dwg'), 'FB9B97F9', '682B')
    C = @((Join-Path $dir 'Dibujo C.dwg'), 'EDE7CDB3', 'A53C')
    D = @((Join-Path $orch 'F1-SMOKE-1-PREP\fixtures\I64-SMOKE1-FIXTURES-D.dxf'), '03E5606C', 'F165')
    biblioteca = @($lib, 'B4CA2248', '60E8')
}
foreach ($k in $items.Keys) {
    $p = $items[$k][0]; $it = Get-Item -LiteralPath $p; $h = Sha $p
    $ok = $h.StartsWith($items[$k][1]) -and $h.EndsWith($items[$k][2])
    Say "dibujo $k sha256 $h esperado $($items[$k][1])...$($items[$k][2]) $(if ($ok) { 'IGUAL' } else { 'DISTINTO' }) LastWriteUtc $($it.LastWriteTimeUtc.ToString('o'))"
}
foreach ($d in @($dir, (Split-Path $items.D[0]))) {
    $side = @(Get-ChildItem -LiteralPath $d -File | Where-Object { $_.Extension -in '.bak', '.sv$', '.dwl', '.dwl2' })
    Say "archivos laterales (.bak/.sv`$/.dwl) en la carpeta de $(if ($d -eq $dir) { 'A, B y C' } else { 'D' }) = $($side.Count)"
}

# Delta de la rama frente al Candidato
$head = (git -C $wt rev-parse HEAD).Trim()
git -C $wt merge-base --is-ancestor $candidate HEAD; $anc = ($LASTEXITCODE -eq 0)
$all = @(git -C $wt diff --name-only $candidate HEAD)
$outside = @($all | Where-Object { $_ -notlike 'docs/automation/*' })
Say "rama HEAD=$head CandidatoAncestro=$anc archivos distintos=$($all.Count) fuera de docs/automation=$($outside.Count)"
foreach ($o in $outside) { Say "  fuera de docs/automation: $o" }

# Herramientas de la ocurrencia: kit de 785a frente a process-guard
foreach ($f in 'Test-AcadProcessGuard.ps1', 'Inspect-LoadedRackCad.ps1', 'Save-ScreenRegion.ps1') {
    Say "guarda $f sha256 $(Sha (Join-Path $pg $f)) igual al kit de 785a = $((Sha (Join-Path $pg $f)) -eq (Sha (Join-Path $kit $f)))"
}

# MBUTTONPAN en el perfil activo (solo lectura)
$prof = 'Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles\<<Unnamed Profile>>\Variables'
$vk = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($prof, $false)
$mb = if ($vk) { $vk.GetValue('MBUTTONPAN', '<no almacenado en Variables>') } else { '<clave no encontrada>' }
if ($vk) { $vk.Close() }
Say "MBUTTONPAN en Variables del perfil = $mb"
if (-not (Test-Path -LiteralPath $OutFile)) { [System.IO.File]::WriteAllLines($OutFile, $out, [System.Text.UTF8Encoding]::new($false)) } else { Write-Output 'OutFile ya existe: no se sobrescribe' }
