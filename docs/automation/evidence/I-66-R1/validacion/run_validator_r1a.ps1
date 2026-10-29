# I-66 R1a, AT-2: runs the F4 production state/v2 validator (RackCad.Tests.dll, StateV2Validator) on the shadow BOOTSTRAP point read from Git.
# Derived from docs/automation/evidence/I-62-F6/FX-U1-chain/run_validator.ps1 of I-62: same calls, with the state path as a parameter, one point
# (the first one: no pair) and negative controls that mutate a copy of the real point in memory and must each be caught by their invariant.
# Usage: pwsh -File run_validator_r1a.ps1 -Dll <RackCad.Tests.dll> -Repo <repo> -Commit <commit> -StatePath <path> -Out <json>
param([string]$Dll, [string]$Repo, [string]$Commit, [string]$StatePath, [string]$Out)
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
function Rows($violations) { @($violations | ForEach-Object { [ordered]@{ Invariant = $_.Invariant; Clause = $_.Clause; Message = $_.Message } }) }
$text = Blob $Commit $StatePath
$tree = [RackCad.Tests.GitCommitStateTree]::new($Repo, $Commit)
$v = [RackCad.Tests.StateV2Validator]::new($null)
$git = [RackCad.Tests.GitProcessHistory]::new($Repo)
$point = [RackCad.Tests.StatePoint]::new([RackCad.Tests.YamlSubset]::Read($text, [RackCad.Tests.YamlReadMode]::Strict), $tree)
$canonical = ([RackCad.Tests.YamlSubset]::Write($point.State) -eq $text)
$file = Rows ($v.ValidateFile($point))
$hist = Rows ($v.ValidateHistory($point, $git, $Commit))
$st = $point.State
$bindingBlob = $st['custody']['principal']['binding']['blob']
$flipped = $bindingBlob.Substring(0, 39) + $(if ($bindingBlob[39] -eq '0') { '1' } else { '0' })
$negatives = @(
    @{ Case = 'NEG-1 blob del binding alterado'; Expect = @('I-S13', 'I-S14'); Text = $text.Replace("blob: $bindingBlob", "blob: $flipped") },
    @{ Case = 'NEG-2 aceptación del Principal ACCEPTED en BOOTSTRAP'; Expect = @('I-S01', 'I-S16')
       Text = $text.Replace("    acceptance:`n      state: PENDING", "    acceptance:`n      state: ACCEPTED") },
    @{ Case = 'NEG-3 preflight inexistente en el árbol'; Expect = @('I-S13'); Text = $text.Replace('autoverificacion/preflight-1.json', 'autoverificacion/preflight-9.json') },
    @{ Case = 'NEG-4 clave desconocida (forma cerrada B.8.1)'; Expect = @('B.8.1'); Text = $text.Replace("counters:`n", "counters:`n  unknown_counter: []`n") }
)
$neg = foreach ($n in $negatives) {
    if ($n.Text -eq $text) { throw "negative $($n.Case) did not change the text" }
    $mp = [RackCad.Tests.StatePoint]::new([RackCad.Tests.YamlSubset]::Read($n.Text, [RackCad.Tests.YamlReadMode]::Strict), $tree)
    $got = @($v.ValidateFile($mp) | ForEach-Object { $_.Invariant } | Sort-Object -Unique)
    [ordered]@{ Case = $n.Case; Expected = $n.Expect; Observed = $got; Caught = (@($n.Expect | Where-Object { $got -contains $_ }).Count -eq $n.Expect.Count) }
}
$res = [ordered]@{
    Validator = 'RackCad.Tests.StateV2Validator (F4 production validator, I-62)'
    DllSha256 = (Get-FileHash -LiteralPath $Dll -Algorithm SHA256).Hash
    Repo = 'origin of I-66 (local worktree of fix/placedclone-copia-anotaciones)'
    Commit = $Commit; StatePath = $StatePath
    StateBlobSha256 = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($text))).Replace('-', '').ToLowerInvariant()
    Point = $st['custody']['point']; RecordVersion = $st['custody']['record_version']
    CanonicalForm = $canonical
    File = $file
    History = $hist
    Negatives = @($neg)
}
$res.Verdict = $(if ($file.Count -eq 0 -and $hist.Count -eq 0 -and $canonical -and @($neg | Where-Object { -not $_.Caught }).Count -eq 0) { 'PASS' } else { 'FAIL' })
$res | ConvertTo-Json -Depth 6 | Set-Content -Path $Out -Encoding utf8NoBOM
'{0} file={1} history={2} canonical={3} negatives={4}' -f $res.Verdict, $file.Count, $hist.Count, $canonical, (($neg | ForEach-Object { $_.Case + ':' + $_.Caught + '[' + ($_.Observed -join ',') + ']' }) -join ' | ')
