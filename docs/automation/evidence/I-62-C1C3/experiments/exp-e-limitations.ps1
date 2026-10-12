# Experimento (e): limites de C1 (y referencia a los de C3, medidos en exp-c).
# E1 escritura transitoria (crear y borrar: F4-T5 placeholder); E2 sobrescritura de un ignorado con fecha restaurada;
# E3 cambio de contenido de un rastreado con el mismo tamano y la fecha restaurada (git status es por stat);
# E4 granularidad de la fecha de escritura en NTFS; E5 coste del barrido en un arbol real (solo enumeracion,
# sin Git, sobre el worktree de I-62 de solo lectura) y rendimiento de SHA-256.
. "$PSScriptRoot\lib\c1c3-lib.ps1"
Initialize-IsolatedGit
$log = New-StepLog
$runs = [ordered]@{}
function Brief($d) { return @($d.Checkouts | Where-Object { $_.Role -ne 'UNIT' } | ForEach-Object { [ordered]@{ Role = $_.Role; Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded; IgnoredChanged = $_.IgnoredChanged; DirEntryChurn = $_.DirEntryChurn; TrackedHashChanged = $_.TrackedHashChanged; ChangeTimeOnly = $_.Sweep.ChangeTimeOnly; ModifiedFiles = $_.Sweep.ModifiedFiles } }) }

# ---- E1: crear y borrar en el checkout principal durante la ventana ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\E1-transient') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$cmd = "[IO.File]::WriteAllText('placeholder.txt', 'x'); Start-Sleep -Milliseconds 1500; [IO.File]::Delete('placeholder.txt')"
$r = Invoke-ChildPwsh -Command $cmd -Cwd $fx.Main
Add-Step $log 'E1 transient write' $cmd $fx.Main $r.ExitCode 'ok'
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
$d = Compare-RepoSnapshot $exit $entry
$runs['E1-transient-create-delete'] = [ordered]@{ Purpose = 'analogo de F4-T5 (placeholder.txt creado y borrado): git status y hashes no dejan rastro; la fecha del directorio si'; C1 = [ordered]@{ Overall = $d.Overall; PerCheckout = Brief $d } }

# ---- E2 y E3: contenido cambiado con el mismo tamano y la fecha restaurada ----
$fx = New-FixturePair -Dir (Join-Path $script:Root 'work\E2-E3-mtime-restored') -AutoCrlf
$roles = @{ $fx.Unit = 'UNIT' }
$ign = Join-Path $fx.Main 'bin\tool.dll'; New-Item -ItemType Directory -Force (Split-Path $ign) | Out-Null; [IO.File]::WriteAllText($ign, 'AAAAAAAA')
[IO.File]::SetLastWriteTimeUtc($ign, [DateTime]::UtcNow.AddMinutes(-10))
$tracked = Join-Path $fx.Main 'src\A.cs'
Start-Sleep -Milliseconds 1200   # fuera de la ventana "racy" de Git
& git -C $fx.Main status --porcelain | Out-Null   # deja el indice con stat fresco (fixture, no checkout real)
$exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles -WithChangeTime -HashAllTracked
$m1 = [IO.File]::GetLastWriteTimeUtc($ign); [IO.File]::WriteAllText($ign, 'BBBBBBBB'); [IO.File]::SetLastWriteTimeUtc($ign, $m1)
$orig = [IO.File]::ReadAllText($tracked); $mt = [IO.File]::GetLastWriteTimeUtc($tracked)
$mutated = $orig.Replace('class A', 'class Z'); [IO.File]::WriteAllText($tracked, $mutated); [IO.File]::SetLastWriteTimeUtc($tracked, $mt)
Add-Step $log 'E2/E3 same-size write + mtime restored' 'WriteAllText(bin\tool.dll, mismo tamano) + SetLastWriteTimeUtc; WriteAllText(src\A.cs, class A->class Z) + SetLastWriteTimeUtc' $fx.Main 0 ("len igual={0}" -f ($orig.Length -eq $mutated.Length))
$plainStatus = & git -C $fx.Main --no-optional-locks status --porcelain
$entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles -WithChangeTime -HashAllTracked
$d = Compare-RepoSnapshot $exit $entry
$runs['E2-E3-same-size-mtime-restored'] = [ordered]@{
    Purpose = 'sobrescritura con el mismo tamano y la fecha de escritura restaurada: invisible para git status (stat) y para el barrido de fechas; visible para el hash de todos los rastreados y para ChangeTime de NTFS'
    GitStatusAfter = @($plainStatus)
    C1 = [ordered]@{ Overall = $d.Overall; PerCheckout = Brief $d }
}

# ---- E4: granularidad de la fecha de escritura ----
$g = Join-Path $script:Root 'work\E4-granularity'; New-Item -ItemType Directory -Force $g | Out-Null
$f = Join-Path $g 'g.txt'; $deltas = @()
for ($i = 0; $i -lt 200; $i++) { [IO.File]::WriteAllText($f, "a$i"); $t1 = [IO.File]::GetLastWriteTimeUtc($f).Ticks; [IO.File]::WriteAllText($f, "b$i"); $t2 = [IO.File]::GetLastWriteTimeUtc($f).Ticks; $deltas += ($t2 - $t1) }
$fs = (Get-Volume -DriveLetter D).FileSystemType
$runs['E4-mtime-granularity'] = [ordered]@{ Purpose = 'dos escrituras seguidas del mismo archivo (N pares): diferencia de LastWriteTime en ticks de 100 ns'; FileSystem = $fs; Pairs = $deltas.Count; MinNonZeroDeltaTicks = (@($deltas | Where-Object { $_ -gt 0 }) | Measure-Object -Minimum).Minimum; MaxDeltaTicks = ($deltas | Measure-Object -Maximum).Maximum; EqualTimestampPairs = @($deltas | Where-Object { $_ -eq 0 }).Count; Note = 'pares con delta 0 = dos escrituras indistinguibles por fecha; C1 compara estado (hash/estado Git), no solo fecha' }

# ---- E5: coste del barrido sobre un arbol real (solo lectura, sin Git) y rendimiento de SHA-256 ----
$i62 = Join-Path $env:USERPROFILE '.codex\worktrees\architecture-portabilidad-coordinador-principal'
$sw = [Diagnostics.Stopwatch]::StartNew(); $sweep = Get-FsSweep -Path $i62; $sw.Stop()
$bytes = 0L; foreach ($v in $sweep.Files.Values) { $bytes += $v.Length }
$blob = Join-Path $script:Root 'work\E5-hash.bin'; $buf = [byte[]]::new(64MB); [Random]::new(1).NextBytes($buf); [IO.File]::WriteAllBytes($blob, $buf)
$sw2 = [Diagnostics.Stopwatch]::StartNew(); $null = Get-FileHash -LiteralPath $blob -Algorithm SHA256; $sw2.Stop()
Remove-Item -LiteralPath $blob
$runs['E5-cost'] = [ordered]@{
    Purpose = 'coste orientativo de un barrido completo (archivos y directorios, sin .git) sobre el worktree de I-62 (lectura) y del hash'
    Tree = 'worktree de I-62 (solo enumeracion de metadatos; sin Git; sin leer contenidos)'
    Files = $sweep.Files.Count; Dirs = $sweep.Dirs.Count; TotalBytes = $bytes; SweepMs = $sw.ElapsedMilliseconds
    Sha256MBps = [Math]::Round(64 / ($sw2.Elapsed.TotalSeconds), 1)
    ProjectedFullHashSeconds = [Math]::Round(($bytes / 1MB) / (64 / $sw2.Elapsed.TotalSeconds), 1)
}
Add-Step $log 'E5 sweep cost' 'Get-FsSweep <worktree de I-62> (solo metadatos)' $i62 0 ("{0} archivos, {1} ms" -f $sweep.Files.Count, $sw.ElapsedMilliseconds)

$result = [ordered]@{ Experiment = 'e-limitations'; Runs = $runs; Steps = $log }
$sha = Write-JsonResult (Join-Path $script:Root 'results\exp-e-limitations.json') $result
foreach ($k in $runs.Keys) { $x = $runs[$k]; if ($x.Contains('C1')) { '{0}: C1={1} main={2}' -f $k, $x.C1.Overall, (($x.C1.PerCheckout | Where-Object { $_.Role -eq 'MAIN' }).Classes -join ',') } }
"E4 equal pairs: $($runs['E4-mtime-granularity'].EqualTimestampPairs)/$($runs['E4-mtime-granularity'].Pairs) fs=$fs"
"E5 files=$($runs['E5-cost'].Files) dirs=$($runs['E5-cost'].Dirs) bytes=$($runs['E5-cost'].TotalBytes) ms=$($runs['E5-cost'].SweepMs) sha=$($runs['E5-cost'].Sha256MBps) MB/s"
"sha256 $sha"
