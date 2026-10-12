# Experimento (b): escritura en un worktree hermano (relativa con '..' y absoluta) con el hijo YA en cwd = worktree
# (C2 aplicado): C2 no la impide; C1 la detecta en la fotografia del hermano. Caso B2: ruta construida desde el
# checkout principal que vuelve al worktree propio (analogo de F4-T1 '..\..\..\Users\...'): C1 no la marca (destino
# dentro del alcance); solo una senal de higiene de rutas.
. "$PSScriptRoot\lib\c1c3-lib.ps1"
Initialize-IsolatedGit
$log = New-StepLog
$runs = [ordered]@{}

function Test-PathHygiene([string]$Raw, [string]$Worktree, [string[]]$OtherCheckouts) {
    # Senal (no STOP): ruta cruda que pasa por otro checkout o sube con '..' fuera de la raiz del worktree
    $full = [IO.Path]::GetFullPath($Raw)
    $inWorktree = $full.StartsWith($Worktree.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)
    $viaOther = @($OtherCheckouts | Where-Object { $Raw.StartsWith($_.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase) })
    $hasDotDot = $Raw -match '(^|[\\/])\.\.([\\/]|$)'
    $signal = if (-not $inWorktree) { 'OUTSIDE_WORKTREE' } elseif ($viaOther.Count -gt 0) { 'PATH_VIA_OTHER_CHECKOUT' } elseif ($hasDotDot) { 'DOTDOT_SEGMENT' } else { 'NONE' }
    return [ordered]@{ Raw = $Raw; Normalized = $full; InWorktree = $inWorktree; Signal = $signal }
}

# ---- B1: escritura en el hermano ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\B1-sibling') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$sibAbs = Join-Path $fx.Sibling 'notes.txt'
$cmd = @"
`$f = [ordered]@{ ProcessCwd = [Environment]::CurrentDirectory }
[IO.File]::AppendAllText('..\wt-sibling\src\A.cs', '// tocado por el worker' + [char]13 + [char]10)
[IO.File]::WriteAllText('$sibAbs', 'nota')
`$f | ConvertTo-Json -Compress
"@
$r = Invoke-ChildPwsh -Command $cmd -Cwd $fx.Unit
Add-Step $log 'B1 worker-call' $cmd $fx.Unit $r.ExitCode ($r.Stdout.Trim())
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$diff = Compare-RepoSnapshot $exit $entry
$cwdFact = ($r.Stdout.Trim() | ConvertFrom-Json).ProcessCwd
$runs['B1-sibling-write'] = [ordered]@{
    Purpose = 'escritura relativa con .. y absoluta en un worktree hermano, con cwd = worktree de la unidad'
    C2 = [ordered]@{ FirstFactProcessCwd = $cwdFact; Verdict = $(if ($cwdFact -ieq $fx.Unit) { 'PASS' } else { 'STOP_C2_CWD_MISMATCH' }) }
    C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = @($diff.Checkouts | ForEach-Object { [ordered]@{ Path = $_.Path; Role = $_.Role; Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded } }) }
    PathHygiene = @(
        (Test-PathHygiene (Join-Path $fx.Unit '..\wt-sibling\src\A.cs') $fx.Unit @($fx.Main, $fx.Sibling)),
        (Test-PathHygiene $sibAbs $fx.Unit @($fx.Main, $fx.Sibling))
    )
}
# Variante: el mismo hermano declarado como CONCURRENT_SIBLING (otra sesion activa) -> atribucion requerida
$exitC = $exit; foreach ($c in $exitC.Checkouts) { if ($c.Path -eq $fx.Sibling) { $c.Role = 'CONCURRENT_SIBLING' } }
$diffC = Compare-RepoSnapshot $exitC $entry
$runs['B1c-sibling-declared-concurrent'] = [ordered]@{
    Purpose = 'mismo cambio con el hermano declarado concurrente: la disposicion pasa a ATTRIBUTION_REQUIRED'
    C1 = [ordered]@{ Overall = $diffC.Overall; Sibling = @($diffC.Checkouts | Where-Object { $_.Path -eq $fx.Sibling } | ForEach-Object { [ordered]@{ Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded } }) }
}

# ---- B2: ruta construida desde el checkout principal que vuelve al worktree propio (F4-T1) ----
$fx2 = New-FixturePair -Dir (Join-Path $script:Root 'work\B2-dotdot-into-own') -AutoCrlf
$roles2 = @{ $fx2.Unit = 'UNIT' }
$exit2 = Get-RepoSnapshot -AnyCheckout $fx2.Unit -Roles $roles2
$raw = Join-Path $fx2.Main '..\wt-unit\docs\x.md'
$cmd2 = "[IO.File]::AppendAllText('$raw', 'linea' + [char]13 + [char]10); [Environment]::CurrentDirectory"
$r2 = Invoke-ChildPwsh -Command $cmd2 -Cwd $fx2.Unit
Add-Step $log 'B2 worker-call' $cmd2 $fx2.Unit $r2.ExitCode ($r2.Stdout.Trim())
$entry2 = Get-RepoSnapshot -AnyCheckout $fx2.Unit -Roles $roles2
$diff2 = Compare-RepoSnapshot $exit2 $entry2
$runs['B2-path-via-main-into-own-worktree'] = [ordered]@{
    Purpose = 'analogo de F4-T1: ruta que sube desde el checkout principal y vuelve al worktree propio'
    C1 = [ordered]@{ Overall = $diff2.Overall; PerCheckout = @($diff2.Checkouts | ForEach-Object { [ordered]@{ Path = $_.Path; Role = $_.Role; Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded } }) }
    PathHygiene = Test-PathHygiene $raw $fx2.Unit @($fx2.Main, $fx2.Sibling)
}

$result = [ordered]@{ Experiment = 'b-sibling-write'; Runs = $runs; Steps = $log }
$sha = Write-JsonResult (Join-Path $script:Root 'results\exp-b-sibling-write.json') $result
foreach ($k in $runs.Keys) { '{0}: C1={1}' -f $k, $runs[$k].C1.Overall }
"sha256 $sha"
