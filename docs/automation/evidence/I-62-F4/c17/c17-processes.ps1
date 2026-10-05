# I-62 F4, C-17 (reproducible control): the process check of README §3.2 / AUTOMATION_PLAN 16.4 on REAL processes of this host.
# Positive: a pwsh whose command line names the worktree is classified `participant`. Negative: an ephemeral process is not alive after the re-read.
# The classification is the production function RackCad.Tests.ProcessFacts (loaded from the built test assembly). Only PIDs, names and classes are
# recorded: never a command line (it could carry another program's data).
# Usage (PowerShell 7, from the worktree, after `dotnet build tests/RackCad.Tests`): pwsh -NoProfile -File c17-processes.ps1 <out.json>
param([Parameter(Mandatory = $true)][string]$Out)
$ErrorActionPreference = 'Stop'
$wt = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..\..')).Path
[void][System.Reflection.Assembly]::LoadFrom((Join-Path $wt 'tests\RackCad.Tests\bin\Debug\net8.0\RackCad.Tests.dll'))

function Snapshot {
    $rows = [System.Collections.Generic.List[RackCad.Tests.ProcessRow]]::new()
    foreach ($p in Get-CimInstance Win32_Process) {
        $rows.Add([RackCad.Tests.ProcessRow]::new([int]$p.ProcessId, [int]$p.ParentProcessId, [string]$p.Name, $p.CommandLine, $p.CreationDate.ToUniversalTime()))
    }
    return , $rows
}

$participant = Start-Process pwsh -ArgumentList '-NoProfile', '-Command', "Start-Sleep -Seconds 120 # $wt" -PassThru -WindowStyle Hidden
$ephemeral = Start-Process pwsh -ArgumentList '-NoProfile', '-Command', 'exit 0' -PassThru -WindowStyle Hidden
$ephemeral.WaitForExit()
Start-Sleep -Seconds 2
try {
    $first = Snapshot
    $classes = [RackCad.Tests.ProcessFacts]::Classify($first, $PID, $wt)
    $mine = @($classes | Where-Object { $_.Process.Pid -eq $participant.Id })
    $again = Snapshot
    $ephemeralAlive = @($again | Where-Object { $_.Pid -eq $ephemeral.Id -and $_.CreationUtc -ge $ephemeral.StartTime.ToUniversalTime().AddSeconds(-1) }).Count -gt 0
    $own = @($classes | Where-Object { $_.Classification -eq 'own-tree' } | ForEach-Object { $_.Process.Pid })
    $stop = [RackCad.Tests.ProcessFacts]::StopCauses($classes, [int[]]@($participant.Id), $null, $null)
    $byClass = @{}
    foreach ($c in $classes) { $byClass[$c.Classification] = 1 + [int]$byClass[$c.Classification] }
    $result = [ordered]@{
        Control         = 'C-17'
        Host            = 'this workstation (Windows, PowerShell ' + $PSVersionTable.PSVersion.ToString() + ')'
        Positive        = [ordered]@{ Pid = $participant.Id; Name = 'pwsh.exe'; Classification = ($mine | ForEach-Object { $_.Classification }) -join ','; Pass = ($mine.Count -eq 1 -and $mine[0].Classification -eq 'participant') }
        Ephemeral       = [ordered]@{ Pid = $ephemeral.Id; ExitCode = $ephemeral.ExitCode; AliveAfterReread = $ephemeralAlive; Pass = (-not $ephemeralAlive) }
        OwnTreeContainsSelf = ($own -contains $PID)
        ClassCounts     = $byClass
        LaunchedTreeExcludedFromStop = (-not (@($stop) | Where-Object { $_ -like "*($($participant.Id))*" }))
    }
    $result.Pass = $result.Positive.Pass -and $result.Ephemeral.Pass -and $result.OwnTreeContainsSelf -and $result.LaunchedTreeExcludedFromStop
    ($result | ConvertTo-Json -Depth 5) + "`n" | Set-Content -Path $Out -Encoding utf8NoBOM -NoNewline
    $result | ConvertTo-Json -Depth 5
}
finally {
    Stop-Process -Id $participant.Id -Force -ErrorAction SilentlyContinue
}
