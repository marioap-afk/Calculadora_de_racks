# Experimento (f): canarios por cesion (K-C1, K-C2) y controles negativos de diseno sobre el validador del relevo
# (mutacion del registro de entrada; roles no derivados). Todo sobre fixtures desechables.
. "$PSScriptRoot\lib\c1c3-lib.ps1"
Initialize-IsolatedGit
$log = New-StepLog
$runs = [ordered]@{}

# ---- K-C1: canario del instrumento de fotografias (oraculo exacto) ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\F1-canary-c1') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$sw = [Diagnostics.Stopwatch]::StartNew()
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$canaryName = 'C1-CANARY-' + [guid]::NewGuid().ToString('N').Substring(0, 8) + '.txt'
[IO.File]::WriteAllText((Join-Path $fx.Main $canaryName), 'canary')
[IO.File]::AppendAllText((Join-Path $fx.Sibling 'src\A.cs'), "// canary`r`n")
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$d = Compare-RepoSnapshot $exit $entry
$sw.Stop()
$main = $d.Checkouts | Where-Object { $_.Role -eq 'MAIN' }; $sib = $d.Checkouts | Where-Object { $_.Role -eq 'QUIESCENT_SIBLING' }
$expectedMain = @("?? $canaryName"); $expectedSib = @(' M src/A.cs')
$pass = (($main.StatusAdded -join '|') -eq ($expectedMain -join '|')) -and (($sib.StatusAdded -join '|') -eq ($expectedSib -join '|')) -and $d.Overall -eq 'STOP'
# negativo del canario: sin escritura -> NONE
$e2 = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles; $e3 = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$dNone = Compare-RepoSnapshot $e2 $e3
$runs['K-C1-canary'] = [ordered]@{
    Purpose = 'antes de cada cesion: el instrumento detecta exactamente una escritura canario en un fixture (nunca en el checkout real) y no detecta nada sin escritura'
    Expected = [ordered]@{ Main = $expectedMain; Sibling = $expectedSib; Overall = 'STOP' }
    Observed = [ordered]@{ Main = $main.StatusAdded; Sibling = $sib.StatusAdded; Overall = $d.Overall }
    NoWriteOverall = $dNone.Overall
    Result = $(if ($pass -and $dNone.Overall -eq 'NONE') { 'PASS' } else { 'FAIL' })
    DurationMs = $sw.ElapsedMilliseconds
}
Add-Step $log 'K-C1' 'fixture + exit + escritura canario (main, hermano) + entry + oraculo exacto' $fx.Root 0 $runs['K-C1-canary'].Result

# ---- NC-C1a: registro de entrada mutado (inyecta un cambio en el checkout principal) ----
$base = Compare-RepoSnapshot $e2 $e3
$mut = $e3 | ConvertTo-Json -Depth 30 | ConvertFrom-Json -AsHashtable
$mm = $mut.Checkouts | Where-Object { $_.Role -eq 'MAIN' }
$mm.Status = @(' M RackCad.sln'); $mm.StatusHashes = [ordered]@{ 'RackCad.sln' = ('0' * 64) }
# rehidratar diccionarios ordenados que espera la comparacion
foreach ($c in $mut.Checkouts) { foreach ($k in 'Files', 'Dirs') { $sd = [System.Collections.Generic.SortedDictionary[string, object]]::new([StringComparer]::OrdinalIgnoreCase); foreach ($kv in $c.Sweep[$k].GetEnumerator()) { $sd[$kv.Key] = $kv.Value }; $c.Sweep[$k] = $sd } }
$dm = Compare-RepoSnapshot $e2 $mut
$mmain = $dm.Checkouts | Where-Object { $_.Role -eq 'MAIN' }
$others = @($dm.Checkouts | Where-Object { $_.Role -ne 'MAIN' } | ForEach-Object { $_.Disposition })
$baseOthers = @($base.Checkouts | Where-Object { $_.Role -ne 'MAIN' } | ForEach-Object { $_.Disposition })
$runs['NC-C1a-mutated-entry'] = [ordered]@{
    Purpose = 'control negativo del validador: la entrada mutada con un cambio en el checkout principal debe dar STOP con esa ruta; el resto, igual que la comparacion real'
    Oracle = 'Overall=STOP; MAIN.StatusAdded=[ M RackCad.sln]; demas checkouts con la misma disposicion que la comparacion real'
    Observed = [ordered]@{ Overall = $dm.Overall; MainStatusAdded = $mmain.StatusAdded; OthersDisposition = $others; BaselineOthersDisposition = $baseOthers }
    Result = $(if ($dm.Overall -eq 'STOP' -and ($mmain.StatusAdded -join '|') -eq ' M RackCad.sln' -and ($others -join '|') -eq ($baseOthers -join '|')) { 'PASS' } else { 'FAIL' })
}

# ---- NC-C1b: checkout ausente en la entrada ----
$mut2 = [ordered]@{ TakenUtc = $e3.TakenUtc; Checkouts = @($e3.Checkouts | Where-Object { $_.Role -ne 'QUIESCENT_SIBLING' }) }
$dm2 = Compare-RepoSnapshot $e2 $mut2
$runs['NC-C1b-checkout-missing'] = [ordered]@{ Purpose = 'un checkout de la salida falta en la entrada (borrado o no fotografiado)'; Observed = [ordered]@{ Overall = $dm2.Overall; Classes = @($dm2.Checkouts | ForEach-Object { "$($_.Role):$($_.Classes -join ',')" }) }; Result = $(if ($dm2.Overall -eq 'STOP') { 'PASS' } else { 'FAIL' }) }

# ---- NC-C1c: roles no derivados (el registro llama UNIT al checkout principal) ----
function Get-DerivedRoles([string]$AnyCheckout, [string]$ExpectedWorktree, [string[]]$DeclaredConcurrent) {
    $m = [ordered]@{}
    foreach ($c in (Get-CheckoutList $AnyCheckout)) { $m[$c.Path] = if ($c.Path -ieq $ExpectedWorktree) { 'UNIT' } elseif ($c.IsMainWorkingTree) { 'MAIN' } elseif ($DeclaredConcurrent -contains $c.Path) { 'CONCURRENT_SIBLING' } else { 'QUIESCENT_SIBLING' } }
    return $m
}
$derived = Get-DerivedRoles $fx.Unit $fx.Unit @()
$recorded = [ordered]@{}; foreach ($c in $e2.Checkouts) { $recorded[$c.Path] = $c.Role }
$recorded[$fx.Main] = 'UNIT'
$mismatch = @($derived.Keys | Where-Object { $derived[$_] -ne $recorded[$_] })
$runs['NC-C1c-role-not-derived'] = [ordered]@{ Purpose = 'los roles se derivan de git worktree list y de ExpectedWorktree de la delegacion; un rol registrado distinto invalida el registro'; Derived = $derived; RecordedMutated = $recorded; Mismatch = $mismatch; Result = $(if ($mismatch.Count -gt 0) { 'PASS (registro invalido)' } else { 'FAIL' }) }

# ---- K-C2: el transporte fija el directorio de proceso; comprobacion antes de ceder ----
$k2 = foreach ($case in @(@{ Id = 'cwd=worktree'; Cwd = $fx.Unit }, @{ Id = 'cwd=main'; Cwd = $fx.Main })) {
    $r = Invoke-ChildPwsh -Command '[Environment]::CurrentDirectory' -Cwd $case.Cwd
    $obs = $r.Stdout.Trim()
    [ordered]@{ Case = $case.Id; Observed = $obs; Expected = $fx.Unit; Verdict = $(if ($obs -ieq $fx.Unit) { 'PASS' } else { 'STOP_BEFORE_INVOCATION' }) }
}
$runs['K-C2-transport-cwd'] = [ordered]@{ Purpose = 'canario de C2: un hijo lanzado por el transporte informa su Environment.CurrentDirectory; distinto del worktree = no se invoca'; Cases = @($k2) }

$result = [ordered]@{ Experiment = 'f-canaries-and-negatives'; Runs = $runs; Steps = $log }
$sha = Write-JsonResult (Join-Path $script:Root 'results\exp-f-canaries-and-negatives.json') $result
foreach ($k in $runs.Keys) { $x = $runs[$k]; if ($x.Contains('Result')) { "{0}: {1}" -f $k, $x.Result } else { "{0}: {1}" -f $k, (($x.Cases | ForEach-Object { "$($_.Case)=$($_.Verdict)" }) -join ' ') } }
"sha256 $sha"
