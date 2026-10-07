# Runs the F4 production state/v2 validator (RackCad.Tests.dll, StateV2Validator) on real fixture points read from Git.
# Usage: pwsh -File run_validator.ps1 -Dll <RackCad.Tests.dll> -Repo <fixture-origin.git> -Prev <commit> -Next <commit> -Out <json>
param([string]$Dll, [string]$Repo, [string]$Prev, [string]$Next, [string]$Out)
$ErrorActionPreference = 'Stop'
Add-Type -Path $Dll
$statePath = 'docs/automation/state/FX-U1.yml'
function Point([string]$commit) {
    $text = (& git --git-dir=$Repo show "${commit}:$statePath" | Out-String)
    $raw = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::UTF8.GetBytes($text))
    # read exactly the stored bytes (git show via Out-String may add a trailing newline; re-read through cat-file for fidelity)
    $psi = [System.Diagnostics.ProcessStartInfo]::new('git', "--git-dir=`"$Repo`" cat-file blob ${commit}:$statePath")
    $psi.RedirectStandardOutput = $true; $psi.UseShellExecute = $false
    $p = [System.Diagnostics.Process]::Start($psi); $ms = [System.IO.MemoryStream]::new(); $p.StandardOutput.BaseStream.CopyTo($ms); $p.WaitForExit()
    $text = [System.Text.Encoding]::UTF8.GetString($ms.ToArray())
    $state = [RackCad.Tests.YamlSubset]::Read($text, [RackCad.Tests.YamlReadMode]::Strict)
    $tree = [RackCad.Tests.GitCommitStateTree]::new($Repo, $commit)
    return [RackCad.Tests.StatePoint]::new($state, $tree)
}
function Rows($violations) { @($violations | ForEach-Object { [ordered]@{ Invariant = $_.Invariant; Clause = $_.Clause; Message = $_.Message } }) }
$v = [RackCad.Tests.StateV2Validator]::new($null)
$git = [RackCad.Tests.GitProcessHistory]::new($Repo)
$p = Point $Prev
$n = Point $Next
$res = [ordered]@{
    Validator = 'RackCad.Tests.StateV2Validator (F4 production validator)'
    DllSha256 = (Get-FileHash -LiteralPath $Dll -Algorithm SHA256).Hash
    Repo = 'D:/r62-fixture/fixture-origin.git'
    Prev = $Prev; Next = $Next
    FilePrev = Rows ($v.ValidateFile($p))
    FileNext = Rows ($v.ValidateFile($n))
    Pair = Rows ($v.ValidatePair($p, $n, $null))
    HistoryNext = Rows ($v.ValidateHistory($n, $git, $Next))
    PairHistory = Rows ($v.ValidatePairHistory($p, $Prev, $n, $Next, $git, $statePath))
    B1History = Rows ($v.ValidateB1History($p, $n, $git, $Next))
}
$res | ConvertTo-Json -Depth 6 | Set-Content -Path $Out -Encoding utf8NoBOM
$res.Keys | Where-Object { $res[$_] -is [array] } | ForEach-Object { '{0}: {1}' -f $_, (@($res[$_]) | ForEach-Object { $_.Invariant + ' ' + $_.Message }) -join ' | ' }
