# Experimento (a): clase de escritura por ruta relativa (I-65 F4-T4, llamada 25) en un par desechable
# "checkout principal + worktree", deteccion por C1 (fotografia salida/entrada), senal de C2 (primer hecho de cwd)
# y prevencion arrancando el hijo con cwd = worktree (C2 preventivo) o con cwd = directorio trampa vacio.
. "$PSScriptRoot\lib\c1c3-lib.ps1"
Initialize-IsolatedGit
$log = New-StepLog
$worker = Join-Path $PSScriptRoot 'worker-call-relative.ps1'
$runs = [ordered]@{}

function Invoke-WorkerCall($Fx, [string]$Cwd, [string]$Mode, [string]$Expected) {
    $al = @('-NoProfile', '-NonInteractive', '-File', $worker, '-TargetDir', $Fx.Unit, '-ExpectedCwd', $Expected, '-Mode', $Mode)
    $r = Invoke-Child -Exe $script:PwshExe -ArgList $al -Cwd $Cwd
    Add-Step $log "worker-call ($Mode)" ("pwsh -NoProfile -File worker-call-relative.ps1 -TargetDir {0} -ExpectedCwd {2} -Mode {1}" -f $Fx.Unit, $Mode, $Expected) $Cwd $r.ExitCode ($r.Stdout.Trim())
    $j = $null; try { $j = $r.Stdout.Trim() | ConvertFrom-Json } catch {}
    return [ordered]@{ ExitCode = $r.ExitCode; Output = $j; Stderr = $r.Stderr.Trim() }
}
function P($o, [string]$n) { if ($null -eq $o) { return $null }; $pp = $o.PSObject.Properties[$n]; if ($pp) { return $pp.Value } else { return $null } }
function Get-BomFlag([string]$f) { if (-not (Test-Path -LiteralPath $f)) { return 'ABSENT' }; $b = [IO.File]::ReadAllBytes($f); return ($b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF) }

$cases = @(
    @{ Id = 'A1-hazard-cwd-main'; CwdRole = 'Main'; Mode = 'EditSln'; Purpose = 'reproduce la clase de F4-T4: hijo con cwd = checkout principal, Set-Location al worktree, [IO.File] relativo' },
    @{ Id = 'A2-prevent-cwd-worktree'; CwdRole = 'Unit'; Mode = 'EditSln'; Purpose = 'C2 preventivo: el transporte arranca el hijo con cwd = worktree de la unidad' },
    @{ Id = 'A3a-trap-cwd-empty-edit'; CwdRole = 'Trap'; Mode = 'EditSln'; Purpose = 'C2 variante directorio vacio: la edicion relativa falla (no hay RackCad.sln en la trampa)' },
    @{ Id = 'A3b-trap-cwd-empty-create'; CwdRole = 'Trap'; Mode = 'CreateNew'; Purpose = 'C2 variante directorio vacio: la creacion relativa cae en la trampa (detectable: trampa no vacia)' },
    @{ Id = 'A4-cmdlet-cwd-main'; CwdRole = 'Main'; Mode = 'CmdletEdit'; Purpose = 'contraste: con cmdlets de PowerShell la ruta relativa resuelve contra la ubicacion de PowerShell' }
)

foreach ($c in $cases) {
    $fx = New-FixturePair -Dir (Join-Path $script:Root ("work\{0}" -f $c.Id)) -AutoCrlf
    Add-Step $log 'fixture' ('git init + 2x git worktree add (autocrlf=true) en ' + $fx.Root) $fx.Root 0 'ok'
    $trap = Join-Path $fx.Root 'trap-empty'; New-Item -ItemType Directory -Force $trap | Out-Null
    $cwd = switch ($c.CwdRole) { 'Main' { $fx.Main } 'Unit' { $fx.Unit } 'Trap' { $trap } }
    $roles = @{ $fx.Unit = 'UNIT' }
    $bomBefore = [ordered]@{ Main = Get-BomFlag (Join-Path $fx.Main 'RackCad.sln'); Unit = Get-BomFlag (Join-Path $fx.Unit 'RackCad.sln') }
    $exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
    Add-Step $log 'C1 exit snapshot' 'git --no-optional-locks status --porcelain=v1 -z --untracked-files=all + hashes + barrido de fechas (todos los checkouts de git worktree list)' $fx.Unit 0 ('{0} checkouts' -f $exit.Checkouts.Count)
    $expected = if ($c.CwdRole -eq 'Trap') { $trap } else { $fx.Unit }
    $call = Invoke-WorkerCall $fx $cwd $c.Mode $expected
    $entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
    $diff = Compare-RepoSnapshot $exit $entry
    Add-Step $log 'C1 entry snapshot + diff' 'Compare-RepoSnapshot exit entry' $fx.Unit 0 $diff.Overall
    $trapEntries = @(Get-ChildItem -LiteralPath $trap -Force | ForEach-Object { $_.Name })
    $runs[$c.Id] = [ordered]@{
        Purpose = $c.Purpose
        ChildCwd = $cwd
        ExpectedWorkerCwd = $expected
        WorkerCall = $call
        C2 = [ordered]@{ FirstFactProcessCwd = (P (P $call.Output 'FirstFact') 'ProcessCwd'); Verdict = (P (P $call.Output 'FirstFact') 'C2') }
        PowerShellVsDotNet = [ordered]@{ PSLocationAfterSetLocation = (P (P $call.Output 'AfterSetLocation') 'PSLocation'); ProcessCwdAfterSetLocation = (P (P $call.Output 'AfterSetLocation') 'ProcessCwd'); ResolvedByDotNet = (P $call.Output 'ResolvedByDotNet'); ResolvedByPowerShell = (P $call.Output 'ResolvedByPowerShell'); Wrote = (P $call.Output 'Wrote'); Error = (P $call.Output 'Error') }
        BomBefore = $bomBefore
        BomAfter = [ordered]@{ Main = Get-BomFlag (Join-Path $fx.Main 'RackCad.sln'); Unit = Get-BomFlag (Join-Path $fx.Unit 'RackCad.sln') }
        TrapEntriesAfter = $trapEntries
        C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = @($diff.Checkouts | ForEach-Object { [ordered]@{ Path = $_.Path; Role = $_.Role; Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded; HashChanged = $_.HashChanged; IgnoredChanged = $_.IgnoredChanged; DirEntryChurn = $_.DirEntryChurn } }) }
        ExitSnapshot = Get-SnapshotSummary $exit
        EntrySnapshot = Get-SnapshotSummary $entry
    }
}

# Bash (Git for Windows): cd cambia el directorio de proceso; la ruta relativa resuelve en el worktree.
$gitDir = Split-Path (Split-Path (Get-Command git).Source)
$bash = Join-Path $gitDir 'bin\bash.exe'
if (Test-Path -LiteralPath $bash) {
    $fx = New-FixturePair -Dir (Join-Path $script:Root 'work\A5-bash-cwd-main') -AutoCrlf
    $roles = @{ $fx.Unit = 'UNIT' }
    $exit = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
    $unixUnit = $fx.Unit.Replace('\', '/')
    $cmd = "cd '$unixUnit' && printf 'AddedByWorker = 1\r\n' >> RackCad.sln && pwd -W"
    $r = Invoke-Child -Exe $bash -ArgList @('-c', $cmd) -Cwd $fx.Main
    Add-Step $log 'bash worker-call' ("bash -c `"$cmd`"") $fx.Main $r.ExitCode ($r.Stdout.Trim())
    $entry = Get-RepoSnapshot -AnyCheckout $fx.Unit -Roles $roles
    $diff = Compare-RepoSnapshot $exit $entry
    $runs['A5-bash-cwd-main'] = [ordered]@{
        Purpose = 'contraste: en Bash, cd cambia el directorio del proceso; la escritura relativa cae en el worktree'
        ChildCwd = $fx.Main; BashPwdAfterCd = $r.Stdout.Trim(); ExitCode = $r.ExitCode
        C1 = [ordered]@{ Overall = $diff.Overall; PerCheckout = @($diff.Checkouts | ForEach-Object { [ordered]@{ Path = $_.Path; Role = $_.Role; Classes = $_.Classes; Disposition = $_.Disposition; StatusAdded = $_.StatusAdded } }) }
    }
}

$result = [ordered]@{
    Experiment = 'a-relative-path-write'
    Host = [ordered]@{ Pwsh = $PSVersionTable.PSVersion.ToString(); Git = (& git --version); Os = [Environment]::OSVersion.VersionString }
    Runs = $runs
    Steps = $log
}
$sha = Write-JsonResult (Join-Path $script:Root 'results\exp-a-relative-write.json') $result
foreach ($k in $runs.Keys) { $x = $runs[$k]; if ($x.Contains('C2')) { '{0}: C1={1} C2={2} wrote={3} err={4}' -f $k, $x.C1.Overall, $x.C2.Verdict, (ConvertTo-Sanitized $x.PowerShellVsDotNet.Wrote), $x.PowerShellVsDotNet.Error } else { '{0}: C1={1}' -f $k, $x.C1.Overall } }
"sha256 $sha"
