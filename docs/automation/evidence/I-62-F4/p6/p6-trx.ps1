# I-62 F4, P6 on the REAL TRX of the Full (Core + UI) of the implementation SHA: the production helper TrxReading.Read (tests/RackCad.Tests/I62/TrxReading.cs)
# loaded from the built test assembly, with the declared Skip methods of each suite's sources. Facts only, no verdict; the TRX bytes are not copied,
# only their SHA-256 and the normalized reading.
# Usage: pwsh -NoProfile -File p6-trx.ps1 <worktree> <core.trx> <ui.trx> <out.json> <implementation sha>
param([string]$Worktree, [string]$CoreTrx, [string]$UiTrx, [string]$Out, [string]$Sha)
$ErrorActionPreference = 'Stop'
$asm = [System.Reflection.Assembly]::LoadFrom((Join-Path $Worktree 'tests/RackCad.Tests/bin/Debug/net8.0/RackCad.Tests.dll'))
$trx = $asm.GetType('RackCad.Tests.TrxReading', $true)
$read = $trx.GetMethod('Read')
$declared = $trx.GetMethod('DeclaredSkipMethods')

function Read-Suite([string]$path, [string]$sourceDir) {
    $sources = [System.Collections.Generic.List[string]]::new()
    Get-ChildItem -Path (Join-Path $Worktree $sourceDir) -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
        ForEach-Object { $sources.Add([System.IO.File]::ReadAllText($_.FullName)) }
    $skips = $declared.Invoke($null, @(, [System.Collections.Generic.IEnumerable[string]]$sources))
    $facts = $read.Invoke($null, @([System.IO.File]::ReadAllBytes($path), $skips))
    [ordered]@{
        Trx = [System.IO.Path]::GetFileName($path)
        Sha256 = $facts.Sha256
        Status = $facts.Status
        Counters = $facts.Counters
        Outcomes = $facts.Outcomes
        Passed = $facts.Passed
        Failed = $facts.Failed
        Skipped = @($facts.Skipped | ForEach-Object { [ordered]@{ Test = $_.Test; Reason = $_.Reason } })
        DeclaredSkipMethods = @($skips | Sort-Object)
        UnexplainedNotExecuted = @($facts.UnexplainedNotExecuted | ForEach-Object { $_.Test })
        UnexpectedOutcomes = @($facts.UnexpectedOutcomes)
        Failures = @($facts.Failures)
        Notes = @($facts.Notes)
    }
}

$result = [ordered]@{
    Helper = 'TrxReading.Read (P6)'
    ImplementationSha = $Sha
    Core = Read-Suite $CoreTrx 'tests/RackCad.Tests'
    UI = Read-Suite $UiTrx 'tests/RackCad.UI.Tests'
}
$result | ConvertTo-Json -Depth 6 | Set-Content -Path $Out -Encoding utf8NoBOM
"Core: " + $result.Core.Status + " " + $result.Core.Passed + "/" + $result.Core.Failed + " skips " + $result.Core.Skipped.Count
"UI: " + $result.UI.Status + " " + $result.UI.Passed + "/" + $result.UI.Failed + " skips " + $result.UI.Skipped.Count
