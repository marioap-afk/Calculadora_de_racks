# Experimento (d): falsos positivos de C1 y como se filtran: refresco del indice de Git, temporales de editores,
# salidas de compilacion, actividad compartida de .git, estabilidad, actividad del Owner. Ruido residual declarado.
. "$PSScriptRoot\lib\c1c3-lib.ps1"
Initialize-IsolatedGit
$log = New-StepLog
$runs = [ordered]@{}
function FileState([string]$p) { $i = [IO.FileInfo]::new($p); return [ordered]@{ MtimeTicks = $i.LastWriteTimeUtc.Ticks; Sha256 = (Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash.Substring(0, 16) } }
function Brief($d) { return @($d.Checkouts | ForEach-Object { [ordered]@{ Role = $_.Role; Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded; IgnoredChanged = $_.IgnoredChanged; TouchOnly = $_.TouchOnly; DirEntryChurn = $_.DirEntryChurn; NoiseAllowlisted = $_.NoiseAllowlisted } }) }

# ---- D1: refresco del indice ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\D1-index-refresh') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$idx = Join-Path $fx.Main '.git\index'
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$a = Join-Path $fx.Main 'src\A.cs'; [IO.File]::SetLastWriteTimeUtc($a, [IO.File]::GetLastWriteTimeUtc($a).AddSeconds(30))
Add-Step $log 'D1 touch' '[IO.File]::SetLastWriteTimeUtc(main\src\A.cs, +30 s) (mismo contenido)' $fx.Main 0 'ok'
$i0 = FileState $idx
$s1 = & git -C $fx.Main --no-optional-locks status --porcelain; $i1 = FileState $idx
Add-Step $log 'D1 status --no-optional-locks' 'git -C main --no-optional-locks status --porcelain' $fx.Main $LASTEXITCODE ("status='{0}' indexChanged={1}" -f ($s1 -join ','), ($i0.MtimeTicks -ne $i1.MtimeTicks -or $i0.Sha256 -ne $i1.Sha256))
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$i2 = FileState $idx
$s2 = & git -C $fx.Main status --porcelain; $i3 = FileState $idx
Add-Step $log 'D1 status (con bloqueos opcionales)' 'git -C main status --porcelain' $fx.Main $LASTEXITCODE ("status='{0}' indexChanged={1}" -f ($s2 -join ','), ($i2.MtimeTicks -ne $i3.MtimeTicks -or $i2.Sha256 -ne $i3.Sha256))
$diff = Compare-RepoSnapshot $exit $entry
$runs['D1-index-refresh-and-touch'] = [ordered]@{
    Purpose = 'git status sin --no-optional-locks reescribe .git/index del checkout inspeccionado; con la opcion no escribe. Un toque de fecha sin cambio de contenido = TOUCH_ONLY'
    IndexAfterNoOptionalLocks = [ordered]@{ Changed = ($i0.MtimeTicks -ne $i1.MtimeTicks -or $i0.Sha256 -ne $i1.Sha256) }
    IndexAfterPlainStatus = [ordered]@{ Changed = ($i2.MtimeTicks -ne $i3.MtimeTicks -or $i2.Sha256 -ne $i3.Sha256) }
    StatusOutputs = [ordered]@{ NoOptionalLocks = @($s1); Plain = @($s2) }
    C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = Brief $diff }
}

# ---- D2: temporales de editores y de AutoCAD en el checkout principal ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\D2-editor-temp') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$temps = @('RackCad.sln~', '.~lock.notas.odt#', 'src\.A.cs.swp', '~$plano.docx', '.vs\RackCad\v17\.suo', 'plano.dwl', 'plano.dwl2', 'plano.bak', 'src\A.cs.orig')
foreach ($t in $temps) { $p = Join-Path $fx.Main $t; New-Item -ItemType Directory -Force (Split-Path $p) | Out-Null; [IO.File]::WriteAllText($p, 'tmp') }
Add-Step $log 'D2 editor temp files' ('crear en main: ' + ($temps -join ', ')) $fx.Main 0 'ok'
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$diff = Compare-RepoSnapshot $exit $entry
$m = $diff.Checkouts | Where-Object { $_.Role -eq 'MAIN' }
$runs['D2-editor-temp-files'] = [ordered]@{
    Purpose = 'temporales tipicos: unos ignorados por .gitignore, otros en la lista de ruido declarada, otros sin filtro'
    NoisePatterns = $script:DefaultNoise
    C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = Brief $diff }
    ResidualNoiseNotFiltered = @(@($m.StatusAdded | ForEach-Object { $_.Substring(3) }) + @($m.IgnoredChanged) | Where-Object { $m.NoiseAllowlisted -notcontains $_ })
}

# ---- D3: salidas de compilacion (ignoradas) ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\D3-build-outputs') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
foreach ($t in @('src\bin\Debug\net8.0\RackCad.Core.dll', 'src\obj\project.assets.json', 'src\obj\Debug\net8.0\RackCad.Core.AssemblyInfo.cs')) { $p = Join-Path $fx.Main $t; New-Item -ItemType Directory -Force (Split-Path $p) | Out-Null; [IO.File]::WriteAllText($p, 'build') }
$st = & git -C $fx.Main --no-optional-locks status --porcelain --untracked-files=all
$stIgn = & git -C $fx.Main --no-optional-locks status --porcelain --untracked-files=all --ignored=matching
Add-Step $log 'D3 build outputs' 'crear bin/obj en main; git status --porcelain --untracked-files=all [--ignored=matching]' $fx.Main 0 ("sin --ignored: {0} lineas; con --ignored: {1}" -f @($st).Count, @($stIgn).Count)
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$diff = Compare-RepoSnapshot $exit $entry
$runs['D3-build-outputs'] = [ordered]@{
    Purpose = 'escrituras en rutas ignoradas: git status (sin --ignored) no las ve; el barrido de fechas si'
    StatusWithoutIgnored = @($st); StatusWithIgnoredMatching = @($stIgn)
    C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = Brief $diff }
}

# ---- D4: actividad compartida en .git por un commit del worktree de la unidad ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\D4-shared-git') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$gitDir = Join-Path $fx.Main '.git'
$before = @{}; Get-ChildItem -LiteralPath $gitDir -Recurse -File -Force | ForEach-Object { $before[$_.FullName] = $_.LastWriteTimeUtc.Ticks }
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
[IO.File]::WriteAllText((Join-Path $fx.Unit 'src\B.cs'), "class B { }`r`n"); G -C $fx.Unit add -A | Out-Null; G -C $fx.Unit commit -q -m 'commit del worker en su worktree' | Out-Null
Add-Step $log 'D4 worker commit in unit worktree' 'git -C wt-unit add -A; git -C wt-unit commit' $fx.Unit 0 'ok'
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$after = @{}; Get-ChildItem -LiteralPath $gitDir -Recurse -File -Force | ForEach-Object { $after[$_.FullName] = $_.LastWriteTimeUtc.Ticks }
$gitChanged = @($after.Keys | Where-Object { -not $before.ContainsKey($_) -or $before[$_] -ne $after[$_] } | ForEach-Object { [IO.Path]::GetRelativePath($fx.Main, $_).Replace('\', '/') } | Sort-Object)
$diff = Compare-RepoSnapshot $exit $entry
$runs['D4-shared-git-activity'] = [ordered]@{
    Purpose = 'un commit legitimo en el worktree de la unidad escribe en el .git compartido (dentro del checkout principal); el barrido excluye .git'
    MainGitDirFilesChanged = $gitChanged
    C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = Brief $diff }
}

# ---- D5: estabilidad (dos fotografias sin actividad) y D6: compilacion en el worktree propio ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\D5-stability') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$s1 = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles; $s2 = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$d5 = Compare-RepoSnapshot $s1 $s2
foreach ($t in @('src\bin\Debug\x.dll', 'src\obj\y.json')) { $p = Join-Path $fx.Unit $t; New-Item -ItemType Directory -Force (Split-Path $p) | Out-Null; [IO.File]::WriteAllText($p, 'b') }
$s3 = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$d6 = Compare-RepoSnapshot $s2 $s3
$runs['D5-stability'] = [ordered]@{ Purpose = 'dos fotografias seguidas sin actividad'; C1 = [ordered]@{ Overall = $d5.Overall; PerCheckout = Brief $d5 }; DurationsMs = @($s1.Checkouts | ForEach-Object { $_.DurationMs }) }
$runs['D6-build-in-own-worktree'] = [ordered]@{ Purpose = 'bin/obj en el worktree propio del Worker: no es materia de C1'; C1 = [ordered]@{ Overall = $d6.Overall; PerCheckout = Brief $d6 } }

# ---- D7: el Owner edita su checkout durante la cesion ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\D7-owner-activity') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
[IO.File]::AppendAllText((Join-Path $fx.Main 'docs\x.md'), "nota del Owner`r`n")
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$diff = Compare-RepoSnapshot $exit $entry
$runs['D7-owner-activity'] = [ordered]@{ Purpose = 'una edicion legitima del Owner en su checkout durante la ventana es indistinguible de una escritura del Worker'; C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = Brief $diff } }

$result = [ordered]@{ Experiment = 'd-false-positives'; Runs = $runs; Steps = $log }
$sha = Write-JsonResult (Join-Path $script:Root 'results\exp-d-false-positives.json') $result
foreach ($k in $runs.Keys) { '{0}: C1={1} main={2}' -f $k, $runs[$k].C1.Overall, (($runs[$k].C1.PerCheckout | Where-Object { $_.Role -eq 'MAIN' }).Classes -join ',') }
"sha256 $sha"
