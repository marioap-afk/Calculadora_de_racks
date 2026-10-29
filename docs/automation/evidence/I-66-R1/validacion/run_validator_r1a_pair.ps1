# I-66 R1a: runs the F4 production state/v2 validator (RackCad.Tests.dll, StateV2Validator) on two consecutive shadow points p -> n read from Git.
# Derived from docs/automation/evidence/I-62-F6/FX-U1-chain/run_validator.ps1 of I-62 (same calls: file of both points, pair, history of n, pair history
# and B1 history), with the state path as a parameter and negative controls read from a JSON file: each one replaces a literal text of n in memory and
# must be caught by its invariants (scope File = ValidateFile of the mutated n; scope Pair = ValidatePair of p and the mutated n).
# Usage: pwsh -File run_validator_r1a_pair.ps1 -Dll <RackCad.Tests.dll> -Repo <repo> -Prev <commit> -Next <commit> -StatePath <path> -Negatives <json> -Out <json>
param([string]$Dll, [string]$Repo, [string]$Prev, [string]$Next, [string]$StatePath, [string]$Negatives, [string]$Out)
$ErrorActionPreference = 'Stop'
Add-Type -Path $Dll
function Blob([string]$commit, [string]$path) {
    $psi = [System.Diagnostics.ProcessStartInfo]::new('git')
    foreach ($a in @('-C', $Repo, '-c', 'core.autocrlf=false', 'cat-file', 'blob', "${commit}:$path")) { $psi.ArgumentList.Add($a) }
    $psi.RedirectStandardOutput = $true; $psi.UseShellExecute = $false
    $p = [System.Diagnostics.Process]::Start($psi); $ms = [System.IO.MemoryStream]::new(); $p.StandardOutput.BaseStream.CopyTo($ms); $p.WaitForExit()
    if ($p.ExitCode -ne 0) { throw "git cat-file failed for ${commit}:$path" }
    return [System.Text.Encoding]::UTF8.GetString($ms.ToArray())
}
function Point([string]$commit, [string]$text) {
    return [RackCad.Tests.StatePoint]::new([RackCad.Tests.YamlSubset]::Read($text, [RackCad.Tests.YamlReadMode]::Strict), [RackCad.Tests.GitCommitStateTree]::new($Repo, $commit))
}
function Rows($violations) { @($violations | ForEach-Object { [ordered]@{ Invariant = $_.Invariant; Clause = $_.Clause; Message = $_.Message } }) }
function Sha256([string]$text) { [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($text))).Replace('-', '').ToLowerInvariant() }
$v = [RackCad.Tests.StateV2Validator]::new($null)
$git = [RackCad.Tests.GitProcessHistory]::new($Repo)
$pText = Blob $Prev $StatePath
$nText = Blob $Next $StatePath
$p = Point $Prev $pText
$n = Point $Next $nText
$res = [ordered]@{
    Validator = 'RackCad.Tests.StateV2Validator (F4 production validator, I-62)'
    DllSha256 = (Get-FileHash -LiteralPath $Dll -Algorithm SHA256).Hash
    Repo = 'origin of I-66 (local worktree of fix/placedclone-copia-anotaciones)'
    StatePath = $StatePath
    Prev = $Prev; PrevPoint = $p.State['custody']['point']; PrevRecordVersion = $p.State['custody']['record_version']; PrevStateBlobSha256 = Sha256 $pText
    Next = $Next; NextPoint = $n.State['custody']['point']; NextRecordVersion = $n.State['custody']['record_version']; NextStateBlobSha256 = Sha256 $nText
    CanonicalForm = ([RackCad.Tests.YamlSubset]::Write($n.State) -eq $nText)
    FilePrev = Rows ($v.ValidateFile($p))
    FileNext = Rows ($v.ValidateFile($n))
    Pair = Rows ($v.ValidatePair($p, $n, $null))
    HistoryNext = Rows ($v.ValidateHistory($n, $git, $Next))
    PairHistory = Rows ($v.ValidatePairHistory($p, $Prev, $n, $Next, $git, $StatePath))
    B1History = Rows ($v.ValidateB1History($p, $n, $git, $Next))
}
$neg = foreach ($c in (Get-Content -LiteralPath $Negatives -Raw -Encoding utf8 | ConvertFrom-Json)) {
    $mText = $nText.Replace([string]$c.Find, [string]$c.Replace)
    if ($mText -eq $nText) { throw "negative $($c.Case) did not change the text" }
    $m = Point $Next $mText
    $found = if ($c.Scope -eq 'Pair') { $v.ValidatePair($p, $m, $null) } else { $v.ValidateFile($m) }
    $got = @($found | ForEach-Object { $_.Invariant } | Sort-Object -Unique)
    [ordered]@{ Case = $c.Case; Scope = $c.Scope; Expected = @($c.Expect); Observed = $got
                Caught = (@($c.Expect | Where-Object { $got -contains $_ }).Count -eq @($c.Expect).Count) }
}
$res.Negatives = @($neg)
$clean = (@('FilePrev', 'FileNext', 'Pair', 'HistoryNext', 'PairHistory', 'B1History') | Where-Object { @($res[$_]).Count -gt 0 }).Count -eq 0
$res.Verdict = $(if ($clean -and $res.CanonicalForm -and @($neg | Where-Object { -not $_.Caught }).Count -eq 0) { 'PASS' } else { 'FAIL' })
$res | ConvertTo-Json -Depth 6 | Set-Content -Path $Out -Encoding utf8NoBOM
'{0} canonical={1} | ' -f $res.Verdict, $res.CanonicalForm + ((@('FilePrev', 'FileNext', 'Pair', 'HistoryNext', 'PairHistory', 'B1History') | ForEach-Object { '{0}={1}[{2}]' -f $_, @($res[$_]).Count, ((@($res[$_]) | ForEach-Object { $_.Invariant }) -join ',') }) -join ' ') + ' | ' + (($neg | ForEach-Object { $_.Case + ':' + $_.Caught + '[' + ($_.Observed -join ',') + ']' }) -join ' | ')
