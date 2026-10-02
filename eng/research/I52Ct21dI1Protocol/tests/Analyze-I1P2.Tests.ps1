# Tests of the I-1 family 2 / P2 material, WITHOUT AutoCAD:  pwsh -NoProfile -File .\tests\Analyze-I1P2.Tests.ps1
# Exit 0 = all passed, 1 = at least one failure. Prints the counts. Creates and removes its own temp folder under the user temp directory.
# The tests start child pwsh processes (the analyzer itself starts none) and read the family files; they never touch the repository tree.
# Requires the final HASHES-I1-V2.txt (the analyzer verifies the family against it before it analyzes anything).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:Family = Split-Path -Parent $PSScriptRoot
$script:Analyzer = Join-Path -Path $script:Family -ChildPath 'Analyze-I1P2.ps1'
$script:Pwsh = (Get-Process -Id $PID).Path
$script:Temp = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath ('I1P2Tests-' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $script:Temp
$script:Pass = 0
$script:Fail = 0
$script:Failures = New-Object 'System.Collections.Generic.List[string]'
$script:Skipped = 0
$script:Section = ''
$script:RunCounter = 0

function Start-Section { param([string]$Name) $script:Section = $Name; Write-Host ('--- ' + $Name) }
function Assert-True {
    param([string]$Name, $Condition, [string]$Info = '')
    if ($Condition -eq $true) { $script:Pass++ }
    else {
        $script:Fail++
        $script:Failures.Add($script:Section + ' :: ' + $Name + $(if ($Info) { ' :: ' + $Info } else { '' }))
        Write-Host ('FAIL ' + $Name + $(if ($Info) { ' :: ' + $Info } else { '' }))
    }
}
function Assert-Equal { param([string]$Name, $Expected, $Actual) Assert-True -Name $Name -Condition ($Expected -ceq $Actual) -Info ('expected [' + $Expected + '] actual [' + $Actual + ']') }

# ---- helpers --------------------------------------------------------------------------------------------------------------------------
function New-RunDir {
    $script:RunCounter++
    $run = 'HGP-H5-20261002T100000Z-{0:00}' -f $script:RunCounter
    $dir = Join-Path -Path $script:Temp -ChildPath $run
    $null = New-Item -ItemType Directory -Path $dir
    return @{ Run = $run; Dir = $dir }
}

function Get-FileBytes {
    param([string]$Terminator, [byte[]]$Accent, [byte[]]$Prefix = @())
    $t = [System.Text.Encoding]::ASCII.GetBytes($Terminator)
    $l = New-Object 'System.Collections.Generic.List[byte]'
    foreach ($x in $Prefix) { $l.Add($x) }
    foreach ($x in [System.Text.Encoding]::ASCII.GetBytes('P2-ASCII-LINE-1')) { $l.Add($x) }
    foreach ($x in $t) { $l.Add($x) }
    foreach ($x in [System.Text.Encoding]::ASCII.GetBytes('P2-ACCENT-')) { $l.Add($x) }
    foreach ($x in $Accent) { $l.Add($x) }
    foreach ($x in [System.Text.Encoding]::ASCII.GetBytes('-END')) { $l.Add($x) }
    foreach ($x in $t) { $l.Add($x) }
    foreach ($x in [System.Text.Encoding]::ASCII.GetBytes('P2-ASCII-LINE-3')) { $l.Add($x) }
    foreach ($x in $t) { $l.Add($x) }
    return $l.ToArray()
}

$script:Utf8E = [byte[]](0xC3, 0xA9)
$script:LatinE = [byte[]](0xE9)
$script:LiteralEscape = [System.Text.Encoding]::ASCII.GetBytes('\U+00E9')

function Write-Bytes { param([string]$Dir, [string]$Name, [byte[]]$Bytes) [System.IO.File]::WriteAllBytes((Join-Path -Path $Dir -ChildPath $Name), $Bytes) }

function Invoke-Analyzer {
    param([string]$Dir, [string]$Run, [string]$Screen = '', [string]$ScriptPath = $script:Analyzer)
    $psi = New-Object System.Diagnostics.ProcessStartInfo($script:Pwsh)
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    foreach ($a in '-NoProfile', '-NonInteractive', '-File', $ScriptPath, '-EvidenceDirectory', $Dir, '-RunId', $Run) { $psi.ArgumentList.Add($a) }
    if ($Screen) { $psi.ArgumentList.Add('-ScreenInput'); $psi.ArgumentList.Add($Screen) }
    $p = [System.Diagnostics.Process]::Start($psi)
    $errTask = $p.StandardError.ReadToEndAsync()
    $ms = New-Object System.IO.MemoryStream
    $p.StandardOutput.BaseStream.CopyTo($ms)
    $p.WaitForExit()
    return @{ Exit = $p.ExitCode; Bytes = $ms.ToArray(); Err = $errTask.Result }
}

function ConvertFrom-Out { param($Res) return [System.Text.Encoding]::UTF8.GetString($Res.Bytes) }

function Get-Snapshot {
    param([string]$Dir)
    $rows = foreach ($f in (Get-ChildItem -LiteralPath $Dir -Recurse -Force)) {
        $h = ''; $len = 0; $ticks = 0   # directory timestamps are updated lazily by NTFS and are not compared
        if (-not $f.PSIsContainer) { $len = $f.Length; $ticks = $f.LastWriteTimeUtc.Ticks; $h = [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.IO.File]::ReadAllBytes($f.FullName))) }
        $f.FullName + '|' + $len + '|' + $ticks + '|' + $h
    }
    return (@($rows | Sort-Object) -join "`n")
}

function Get-ScreenJson {
    param([array]$Entries, [array]$Shots)
    $o = [ordered]@{ screen = @($Entries | ForEach-Object { [ordered]@{ id = $_[0]; classification = $_[1]; text = $_[2] } }); screenshotSha256 = @($Shots) }
    return ($o | ConvertTo-Json -Depth 6 -Compress)
}

function Write-ScreenFile {
    param([string]$Json, [string]$Name = 'screen.json')
    $p = Join-Path -Path $script:Temp -ChildPath ([guid]::NewGuid().ToString('N') + '-' + $Name)
    [System.IO.File]::WriteAllBytes($p, [System.Text.UTF8Encoding]::new($false).GetBytes($Json))
    return $p
}

$script:Shot = ('ab' * 32)

# AutoLISP one-line checker: printable ASCII only, no comment, one balanced top-level form
function Test-LispLine {
    param([string]$Line)
    if ($Line -cnotmatch '^[\x20-\x7e]+$') { return 'non-ASCII or control' }
    $depth = 0; $inStr = $false; $closedAt = -1; $closedEarly = $false
    for ($i = 0; $i -lt $Line.Length; $i++) {
        $c = $Line[$i]
        if ($inStr) {
            if ($c -eq [char]92) { $i++ } elseif ($c -eq [char]34) { $inStr = $false }
        } else {
            if ($c -eq [char]34) { $inStr = $true }
            elseif ($c -eq [char]59) { return 'comment' }
            elseif ($c -eq [char]40) { $depth++ }
            elseif ($c -eq [char]41) { $depth--; if ($depth -lt 0) { return 'extra close' }; if ($depth -eq 0) { $closedAt = $i; if ($i -lt $Line.Length - 1) { $closedEarly = $true } } }
        }
    }
    if ($inStr) { return 'open string' }
    if ($depth -ne 0) { return 'unbalanced' }
    if ($closedEarly -or $closedAt -ne $Line.Length - 1) { return 'more than one top-level form' }
    return 'OK'
}

function Test-ForbiddenTokens {
    param([string]$Text)
    $bad = @('Invoke-Expression', 'iex ', 'Add-Type', 'Start-Process', 'Set-Content', 'Add-Content', 'Out-File', 'New-Item', 'Remove-Item', 'Move-Item', 'Copy-Item', 'Rename-Item',
        'Set-ItemProperty', 'New-ItemProperty', 'Invoke-WebRequest', 'Invoke-RestMethod', 'Invoke-Command', 'Start-Job', 'WriteAllBytes', 'WriteAllText', 'WriteAllLines',
        'File]::Create', 'OpenWrite', 'StreamWriter', 'FileStream', 'Registry', 'HKLM', 'HKCU', 'Get-ItemProperty', 'System.Net', 'Diagnostics.Process', 'ProcessStartInfo',
        'ScriptBlock]::Create', 'Invoke-Item', 'Set-Location', 'Push-Location', 'Out-Host', 'Tee-Object', 'Export-', 'Clear-Content', 'Set-Acl', 'Set-ItemProperty')
    $hits = @($bad | Where-Object { $Text.Contains($_) })
    return , $hits
}

try {
    # =====================================================================================================================================
    Start-Section 'allowlist: the closed set of typed expressions'
    $allowPath = Join-Path -Path $script:Family -ChildPath 'P2-ALLOWLIST.txt'
    $allowBytes = [System.IO.File]::ReadAllBytes($allowPath)
    $allowText = [System.Text.Encoding]::ASCII.GetString($allowBytes)
    Assert-True 'allowlist has no CR' (-not $allowText.Contains("`r"))
    Assert-True 'allowlist ends with exactly one LF' ($allowText.EndsWith("`n") -and -not $allowText.EndsWith("`n`n"))
    $lines = @($allowText.Substring(0, $allowText.Length - 1).Split("`n"))
    Assert-Equal 'allowlist has 19 expressions' 19 $lines.Count
    $i = 0
    foreach ($l in $lines) {
        $i++
        Assert-Equal ('line {0:00} is one balanced ASCII form' -f $i) 'OK' (Test-LispLine -Line $l)
        Assert-True ('line {0:00} is at most 600 characters' -f $i) ($l.Length -le 600) ([string]$l.Length)
    }
    Assert-Equal 'exactly one <RUNID> token' 1 ([regex]::Matches($allowText, '<RUNID>').Count)
    Assert-True '<RUNID> is in line 1' ($lines[0].Contains('<RUNID>'))
    Assert-True 'no other angle-bracket tokens' ([regex]::Matches($allowText, '[<>]').Count -eq 2)
    $headOk = $true; $nsOk = $true
    $i = 0
    foreach ($l in $lines) {
        $i++
        if ($l -cmatch '^\(defun (\S+) ') { if (-not $Matches[1].StartsWith('ct21d-p2-')) { $nsOk = $false } }
        elseif ($l -cmatch '^\(setq (\S+) ') { if ($Matches[1] -cne 'ct21d-p2-root') { $nsOk = $false } }
        elseif ($l -cmatch '^\((progn|ct21d-p2-try|ct21d-p2-shape) ') { }
        else { $headOk = $false }
    }
    Assert-True 'every top-level head is defun / setq / progn / try / shape' $headOk
    Assert-True 'every defun and every global setq is in the ct21d-p2- namespace' $nsOk
    $forbiddenLisp = @('(load ', '(command', '(setvar', 'vl-registry', 'vla-', 'startapp', 'vl-file', 'vl-mkdir', 'delete', 'getenv', 'setenv', 'dos_', 'vl-cmdf', 'arxload', 'appload', 'netload', '(getvar', '(open ', 'sssetfirst')
    foreach ($f in $forbiddenLisp) { Assert-True ('allowlist does not contain ' + $f) (-not $allowText.Contains($f)) }
    Assert-Equal 'vlax- appears only in the one product-key call' 1 ([regex]::Matches($allowText, 'vlax-').Count)
    Assert-True 'write-line is called only in the helper of line 11' (([regex]::Matches($allowText, '\(write-line')).Count -eq 3 -and ([regex]::Matches($lines[10], '\(write-line')).Count -eq 3)
    $applied = [string[]]@([regex]::Matches($allowText, "vl-catch-all-apply '([^ ]+)") | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
    [System.Array]::Sort($applied, [System.StringComparer]::Ordinal)
    Assert-Equal 'the only quoted function symbols applied are the expected ones' 'close,ct21d-p2-lines,findfile,open,strlen,vl-load-com,vlax-product-key' ($applied -join ',')
    $mdText = [System.IO.File]::ReadAllText((Join-Path -Path $script:Family -ChildPath 'I-1-observation-protocol-v2-P2.md'), [System.Text.UTF8Encoding]::new($false, $true))
    $i = 0
    foreach ($l in $lines) {
        $i++
        $row = '| P2-E{0:00} | `{1}` | ' -f $i, $l
        Assert-True ('the table row of P2-E{0:00} in the P2 text equals the allowlist line' -f $i) ($mdText.Contains($row))
    }
    Assert-True 'the P2 text has no CR and no BOM' (-not $mdText.Contains("`r") -and -not $mdText.StartsWith([string][char]0xFEFF, [System.StringComparison]::Ordinal))
    Assert-True 'the P2 text records both full v1 blob ids' ($mdText.Contains('3a77ba62da77dfa2fc8bd2ffeb176a9454864193') -and $mdText.Contains('a9081cff612b2290c35c86c0ded72f42c62b5e5a'))
    # negative controls of the checker itself
    Assert-Equal 'checker flags an unbalanced form' 'unbalanced' (Test-LispLine -Line '(a (b)')
    Assert-Equal 'checker flags an extra close' 'extra close' (Test-LispLine -Line '(a))(b)')
    Assert-Equal 'checker flags two forms' 'more than one top-level form' (Test-LispLine -Line '(a)(b)')
    Assert-Equal 'checker flags a comment' 'comment' (Test-LispLine -Line '(a) ; x')
    Assert-Equal 'checker flags non-ASCII' 'non-ASCII or control' (Test-LispLine -Line ('(a "' + [char]0xE9 + '")'))
    Assert-Equal 'checker flags an open string' 'open string' (Test-LispLine -Line '(a "b)')
    Assert-Equal 'checker accepts a backslash-quote inside a string' 'OK' (Test-LispLine -Line '(a "b\"c")')

    # =====================================================================================================================================
    Start-Section 'static scan of the analyzer'
    $src = [System.IO.File]::ReadAllText($script:Analyzer)
    Assert-Equal 'the analyzer contains no forbidden token' '' ((Test-ForbiddenTokens -Text $src) -join ',')
    Assert-True 'the analyzer writes only through the stdout stream' ($src.Contains('OpenStandardOutput'))
    Assert-True 'negative control: scan flags a dynamic-code token' ((Test-ForbiddenTokens -Text 'x; Invoke-Expression $y').Count -eq 1)
    Assert-True 'negative control: scan flags a file write token' ((Test-ForbiddenTokens -Text 'Set-Content -Path a -Value b').Count -eq 1)
    Assert-True 'negative control: scan flags a process token' ((Test-ForbiddenTokens -Text 'Start-Process notepad').Count -eq 1)
    Assert-True 'negative control: scan flags a registry token' ((Test-ForbiddenTokens -Text 'Get-ItemProperty HKLM:\x').Count -ge 1)
    Assert-True 'the analyzer has no CR and no BOM' (-not $src.Contains("`r") -and -not $src.StartsWith([string][char]0xFEFF, [System.StringComparison]::Ordinal))

    # =====================================================================================================================================
    Start-Section 'ledger'
    $ledgerPath = Join-Path -Path $script:Family -ChildPath 'HASHES-I1-V2.txt'
    $ledger = [System.IO.File]::ReadAllText($ledgerPath)
    Assert-True 'ledger is LF text ending in one LF' (-not $ledger.Contains("`r") -and $ledger.EndsWith("`n") -and -not $ledger.EndsWith("`n`n"))
    $listed = @{}
    foreach ($row in $ledger.Substring(0, $ledger.Length - 1).Split("`n")) {
        $ok = $row -cmatch '^([0-9a-f]{64})  (\S+)$'
        Assert-True ('ledger row is sha256sum -c format: ' + $row.Substring(0, [Math]::Min(40, $row.Length))) $ok
        if ($ok) {
            $listed[$Matches[2]] = $true
            $full = Join-Path -Path $script:Family -ChildPath $Matches[2]
            $actual = [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.IO.File]::ReadAllBytes($full))).ToLowerInvariant()
            Assert-Equal ('ledger hash of ' + $Matches[2]) $Matches[1] $actual
        }
    }
    Assert-True 'ledger does not list itself' (-not $listed.ContainsKey('HASHES-I1-V2.txt'))
    $onDisk = @(Get-ChildItem -LiteralPath $script:Family -Recurse -File -Force | ForEach-Object { $_.FullName.Substring($script:Family.Length + 1).Replace('\', '/') } | Where-Object { $_ -cne 'HASHES-I1-V2.txt' })
    Assert-Equal 'ledger lists exactly the files of the family' ((@($onDisk | Sort-Object) -join ',')) ((@($listed.Keys | Sort-Object) -join ','))
    Assert-True 'the family has no C2 file' (@($onDisk | Where-Object { $_ -match 'C2' }).Count -eq 0)

    # a tampered copy of the family must be refused
    $copy = Join-Path -Path $script:Temp -ChildPath 'family-copy'
    Copy-Item -LiteralPath $script:Family -Destination $copy -Recurse
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -ScriptPath (Join-Path -Path $copy -ChildPath 'Analyze-I1P2.ps1')
    Assert-Equal 'untampered copy runs' 0 $res.Exit
    [System.IO.File]::AppendAllText((Join-Path -Path $copy -ChildPath 'I-1-observation-protocol-v2-P2.md'), 'x')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -ScriptPath (Join-Path -Path $copy -ChildPath 'Analyze-I1P2.ps1')
    Assert-Equal 'tampered P2 text is refused' 2 $res.Exit
    Assert-True 'tampered family: stdout empty, stderr names the file' ($res.Bytes.Length -eq 0 -and $res.Err.Contains('REFUSED: family file differs from the ledger'))
    Copy-Item -LiteralPath (Join-Path -Path $script:Family -ChildPath 'I-1-observation-protocol-v2-P2.md') -Destination (Join-Path -Path $copy -ChildPath 'I-1-observation-protocol-v2-P2.md') -Force
    Remove-Item -LiteralPath (Join-Path -Path $copy -ChildPath 'P2-ALLOWLIST.txt')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -ScriptPath (Join-Path -Path $copy -ChildPath 'Analyze-I1P2.ps1')
    Assert-Equal 'a missing family file is refused' 2 $res.Exit
    Copy-Item -LiteralPath (Join-Path -Path $script:Family -ChildPath 'P2-ALLOWLIST.txt') -Destination (Join-Path -Path $copy -ChildPath 'P2-ALLOWLIST.txt')
    Remove-Item -LiteralPath (Join-Path -Path $copy -ChildPath 'HASHES-I1-V2.txt')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -ScriptPath (Join-Path -Path $copy -ChildPath 'Analyze-I1P2.ps1')
    Assert-Equal 'a missing ledger is refused' 2 $res.Exit

    # =====================================================================================================================================
    Start-Section 'file analysis: terminators, BOM, encodings'
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`n" $script:LiteralEscape)
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`r`n" $script:Utf8E)
    $before = Get-Snapshot $r.Dir
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run
    $after = Get-Snapshot $r.Dir
    Assert-Equal 'exit 0' 0 $res.Exit
    Assert-Equal 'stderr empty' '' $res.Err
    $outText = ConvertFrom-Out $res
    Assert-True 'stdout ends with exactly one LF' ($outText.EndsWith("`n") -and -not $outText.EndsWith("`n`n"))
    Assert-True 'stdout has no CR, no BOM, only printable ASCII plus the final LF' (($outText.Substring(0, $outText.Length - 1) -cmatch '^[\x20-\x7e]+$') -and $res.Bytes[0] -ne 0xEF)
    Assert-Equal 'the analyzer wrote nothing (evidence directory snapshot equal)' $before $after
    $j = $outText | ConvertFrom-Json -AsHashtable
    Assert-Equal 'activity' 'H5' $j.activity
    Assert-True 'governing false' ($j.governing -eq $false)
    Assert-Equal 'protocol family' 'I1-F2' $j.protocolFamily
    Assert-Equal 'run id' $r.Run $j.runId
    Assert-Equal 'p2TextSha256 is the sha256 of the P2 text' ([System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.IO.File]::ReadAllBytes((Join-Path -Path $script:Family -ChildPath 'I-1-observation-protocol-v2-P2.md')))).ToLowerInvariant()) $j.p2TextSha256
    Assert-Equal 'ledgerSha256 is the sha256 of the ledger' ([System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.IO.File]::ReadAllBytes($ledgerPath))).ToLowerInvariant()) $j.ledgerSha256
    $f1 = $j.files[0]; $f2 = $j.files[1]
    Assert-Equal 'a1 name' 'p2-a1.txt' $f1.name
    Assert-Equal 'a1 terminator LF' 'LF' $f1.terminator
    Assert-Equal 'a1 literal escape = ASCII_ONLY' 'ASCII_ONLY' $f1.encodingInterpretation
    Assert-Equal 'a1 non-ASCII bytes empty' '' $f1.nonAsciiBytesHex
    Assert-Equal 'a1 accent segment is the literal escape' '5c552b30304539' $f1.accentSegmentHex
    Assert-True 'a1 expected lines present' $f1.expectedLinesPresent
    Assert-Equal 'a2 terminator CRLF' 'CRLF' $f2.terminator
    Assert-Equal 'a2 UTF8_NO_BOM' 'UTF8_NO_BOM' $f2.encodingInterpretation
    Assert-Equal 'a2 non-ASCII bytes c3a9' 'c3a9' $f2.nonAsciiBytesHex
    Assert-True 'a2 bom false' ($f2.bomDetected -eq $false)
    Assert-Equal 'a2 length' ((Get-FileBytes "`r`n" $script:Utf8E).Length) $f2.length
    Assert-Equal 'a2 sha256' ([System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData((Get-FileBytes "`r`n" $script:Utf8E))).ToLowerInvariant()) $f2.sha256
    Assert-Equal 'a2 first 64 bytes hex' ([System.Convert]::ToHexString((Get-FileBytes "`r`n" $script:Utf8E)[0..([Math]::Min(63, (Get-FileBytes "`r`n" $script:Utf8E).Length - 1))]).ToLowerInvariant()) $f2.first64BytesHex
    Assert-Equal 'terminator characterization OBSERVED' 'OBSERVED' $j.characterization.writeLineTerminator.status
    Assert-True 'schema validates the good record' (Test-Json -Json $outText -SchemaFile (Join-Path -Path $script:Family -ChildPath 'schemas/ct21d.i1-p2.v1.json'))
    $res2 = Invoke-Analyzer -Dir $r.Dir -Run $r.Run
    Assert-True 'deterministic: two runs give identical bytes' ([System.Convert]::ToHexString([byte[]]$res.Bytes) -ceq [System.Convert]::ToHexString([byte[]]$res2.Bytes))
    # keys sorted at every level (the canonical text equals a re-serialization with sorted keys)
    $keysOk = $true
    $check = {
        param($o)
        if ($o -is [System.Collections.IDictionary]) {
            $ks = @($o.Keys | ForEach-Object { [string]$_ })
            $sorted = [string[]]$ks.Clone(); [System.Array]::Sort($sorted, [System.StringComparer]::Ordinal)
            if (($ks -join ',') -cne ($sorted -join ',')) { return $false }
            foreach ($v in $o.Values) { if (-not (& $check $v)) { return $false } }
        } elseif ($o -is [System.Collections.IList]) { foreach ($v in $o) { if (-not (& $check $v)) { return $false } } }
        return $true
    }
    # ConvertFrom-Json keeps the file order in the ordered hashtable
    $jo = $outText | ConvertFrom-Json -AsHashtable
    Assert-True 'keys are sorted ordinally at every level' (& $check $jo)

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`n" $script:Utf8E ([byte[]](0xEF, 0xBB, 0xBF)))
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:LatinE)
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)) | ConvertFrom-Json -AsHashtable
    Assert-True 'BOM file: bomDetected' ($j.files[0].bomDetected -eq $true)
    Assert-Equal 'BOM file: UTF8_WITH_BOM' 'UTF8_WITH_BOM' $j.files[0].encodingInterpretation
    Assert-True 'BOM file: expected lines still recognized' ($j.files[0].expectedLinesPresent -eq $true)
    Assert-Equal 'single-byte e-acute: SINGLE_BYTE' 'SINGLE_BYTE' $j.files[1].encodingInterpretation
    Assert-Equal 'single-byte e-acute: bytes e9' 'e9' $j.files[1].nonAsciiBytesHex
    Assert-Equal 'single-byte e-acute: terminator LF' 'LF' $j.files[1].terminator

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' ([System.Text.Encoding]::Unicode.GetPreamble() + [System.Text.Encoding]::Unicode.GetBytes("P2-ASCII-LINE-1`nP2-ACCENT-" + [char]0xE9 + "-END`nP2-ASCII-LINE-3`n"))
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`r" $script:Utf8E)
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)) | ConvertFrom-Json -AsHashtable
    Assert-Equal 'UTF-16 file: OTHER' 'OTHER' $j.files[0].encodingInterpretation
    Assert-True 'UTF-16 file: bomDetected' ($j.files[0].bomDetected -eq $true)
    Assert-True 'UTF-16 file: expected lines not recognized' ($j.files[0].expectedLinesPresent -eq $false)
    Assert-Equal 'CR-only file: terminator CR' 'CR' $j.files[1].terminator
    Assert-Equal 'CR-only terminator is OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.writeLineTerminator.status

    $r = New-RunDir
    $none = [System.Text.Encoding]::ASCII.GetBytes('P2-ASCII-LINE-1')
    Write-Bytes $r.Dir 'p2-a1.txt' $none
    $mixed = (Get-FileBytes "`n" $script:Utf8E)
    $mixed[15] = 13; $mixed = [byte[]]($mixed[0..15] + [byte[]](10) + $mixed[16..($mixed.Length - 1)])
    Write-Bytes $r.Dir 'p2-a2.txt' $mixed
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)) | ConvertFrom-Json -AsHashtable
    Assert-Equal 'no terminator: NONE' 'NONE' $j.files[0].terminator
    Assert-Equal 'mixed terminators: MIXED' 'MIXED' $j.files[1].terminator
    Assert-Equal 'NONE/MIXED terminator is OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.writeLineTerminator.status

    # empty file
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' ([byte[]]@())
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)) | ConvertFrom-Json -AsHashtable
    Assert-True 'empty file exists with length 0' ($j.files[0].exists -eq $true -and $j.files[0].length -eq 0)
    Assert-Equal 'empty file: UNKNOWN encoding' 'UNKNOWN' $j.files[0].encodingInterpretation
    Assert-Equal 'empty file sha256 is the empty-input hash' 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855' $j.files[0].sha256

    # =====================================================================================================================================
    Start-Section 'missing, extra, oversized and hostile entries'
    $r = New-RunDir
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)) | ConvertFrom-Json -AsHashtable
    Assert-True 'no files: both absent' ($j.files[0].exists -eq $false -and $j.files[1].exists -eq $false)
    Assert-Equal 'no files: terminator NOT_OBSERVABLE' 'NOT_OBSERVABLE' $j.characterization.writeLineTerminator.status
    Assert-Equal 'no files: no screen -> openTwoArg UNKNOWN' 'UNKNOWN' $j.characterization.openTwoArg.status
    Assert-Equal 'no files: strlen UNKNOWN' 'UNKNOWN' $j.characterization.strlenSemantics.status
    Assert-True 'no files: stopCondition false, custody false' ($j.stopConditionRecorded -eq $false -and $j.screenCustodyPresent -eq $false)

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`n" $script:Utf8E)
    Write-Bytes $r.Dir 'phase2-record-x.json' ([byte[]](1, 2, 3))
    Write-Bytes $r.Dir 'raw-TRUSTEDPATHS.txt' ([byte[]](65))
    Write-Bytes $r.Dir 'P2-A2.TXT' (Get-FileBytes "`n" $script:Utf8E)
    Write-Bytes $r.Dir 'huge-other.bin' ([byte[]]::new(100000))
    $null = New-Item -ItemType Directory -Path (Join-Path -Path $r.Dir -ChildPath 'subdir')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run
    Assert-Equal 'extra entries do not stop the analysis' 0 $res.Exit
    $j = (ConvertFrom-Out $res) | ConvertFrom-Json -AsHashtable
    Assert-Equal 'extra entries are listed by name only, sorted ordinally' 'P2-A2.TXT,huge-other.bin,phase2-record-x.json,raw-TRUSTEDPATHS.txt,subdir' (@($j.otherEntryNames) -join ',')
    Assert-True 'a wrong-case name is not read as p2-a2.txt' ($j.files[1].exists -eq $false)
    Assert-True 'the extra files are not echoed into the record' (-not (ConvertFrom-Out $res).Contains('raw-TRUSTEDPATHS.txt":') -and -not (ConvertFrom-Out $res).Contains('"length":100000'))

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' ([byte[]]::new(4097))
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run
    Assert-Equal 'oversized file is refused' 2 $res.Exit
    Assert-True 'oversized: stdout empty, stderr says larger' ($res.Bytes.Length -eq 0 -and $res.Err.Contains('REFUSED: p2-a1.txt is larger than 4096'))
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' ([byte[]]::new(4096))
    Assert-Equal 'a file of exactly 4096 bytes is accepted' 0 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run).Exit
    $r = New-RunDir
    $null = New-Item -ItemType Directory -Path (Join-Path -Path $r.Dir -ChildPath 'p2-a1.txt')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run
    Assert-Equal 'a directory named p2-a1.txt is refused' 2 $res.Exit
    Assert-True 'directory: stdout empty' ($res.Bytes.Length -eq 0)

    # junction as the evidence directory (no privilege needed)
    $real = New-RunDir
    $linkRun = 'HGP-H5-20261002T100000Z-99'
    $link = Join-Path -Path $script:Temp -ChildPath $linkRun
    try {
        $null = New-Item -ItemType Junction -Path $link -Target $real.Dir
        $res = Invoke-Analyzer -Dir $link -Run $linkRun
        Assert-Equal 'a reparse-point evidence directory is refused' 2 $res.Exit
        Assert-True 'junction: message names the reparse point' $res.Err.Contains('reparse point')
        [System.IO.Directory]::Delete($link)
    } catch {
        $script:Skipped++
        Write-Host ('SKIPPED junction test: ' + $_.Exception.Message)
    }

    # =====================================================================================================================================
    Start-Section 'path and parameter guards'
    $r = New-RunDir
    $res = Invoke-Analyzer -Dir (Join-Path -Path $script:Temp -ChildPath 'HGP-H5-20261002T100000Z-77') -Run 'HGP-H5-20261002T100000Z-77'
    Assert-Equal 'a non-existent directory is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Run -Run $r.Run
    Assert-Equal 'a relative path is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir ('\\localhost\C$\x\' + $r.Run) -Run $r.Run
    Assert-Equal 'a UNC path is refused' 2 $res.Exit
    Assert-True 'UNC: names the drive-letter rule' $res.Err.Contains('drive-letter')
    $res = Invoke-Analyzer -Dir ('\\?\' + $r.Dir) -Run $r.Run
    Assert-Equal 'a device path is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir ($script:Temp + '\other\..\' + $r.Run) -Run $r.Run
    Assert-Equal 'a path with a dot-dot segment is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'HGP-H5-20261002T100000Z-02'
    Assert-Equal 'a leaf that differs from the RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'HGP-H1-20261002T100000Z-01'
    Assert-Equal 'an H1 RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'hgp-h5-20261002t100000z-01'
    Assert-Equal 'a lower-case RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run ($r.Run + 'x')
    Assert-Equal 'a RunId with a suffix is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir 'C:\Windows\System32\drivers' -Run 'HGP-H5-20261002T100000Z-01'
    Assert-Equal 'a system directory with the wrong leaf is refused' 2 $res.Exit
    Assert-True 'every refusal printed nothing on stdout' ($res.Bytes.Length -eq 0)

    # =====================================================================================================================================
    Start-Section 'screen results and the characterization'
    $okE13 = "P2-E13 open VALUE: #<file `"D:/x/p2-a1.txt`">`nP2-E13 write-line VALUE: `"P2-ASCII-LINE-3`"`nP2-E13 close VALUE: nil"
    $good = @(
        @('P2-E03', 'OK', 'P2-E03 AB-CD'),
        @('P2-E05', 'OK', 'P2-E05 raw-K.txt-V'),
        @('P2-E07', 'OK', 'P2-E07 VALUE: nil'),
        @('P2-E08', 'OK', 'P2-E08 VALUE: "C:/Windows/System32/kernel32.dll"'),
        @('P2-E09', 'OK', 'P2-E09 VALUE: 3'),
        @('P2-E10', 'OK', 'P2-E10 VALUE: 3'),
        @('P2-E13', 'OK', $okE13),
        @('P2-E14', 'ERROR_TEXT_VERBATIM', 'P2-E14 open ERROR: too many arguments'),
        @('P2-E15', 'OK', 'P2-E15 VALUE: "D:/x/p2-a1.txt"'),
        @('P2-E16', 'OK', 'P2-E16 VALUE: nil'),
        @('P2-E19', 'OK', 'P2-E19 type=STR len=57 startsExact=T startsAnyCase=T hasWowAnyCase=nil')
    )
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`r`n" $script:LiteralEscape)
    $sf = Write-ScreenFile (Get-ScreenJson $good @($script:Shot))
    $snapBefore = Get-Snapshot $script:Temp
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf
    Assert-Equal 'exit 0 with a screen input' 0 $res.Exit
    $txt = ConvertFrom-Out $res
    $j = $txt | ConvertFrom-Json -AsHashtable
    $ch = $j.characterization
    Assert-Equal 'arity control OBSERVED' 'OBSERVED' $ch.userFunctionArityControl.status
    Assert-Equal 'findfileAbsent OBSERVED' 'OBSERVED' $ch.findfileAbsent.status
    Assert-Equal 'findfilePresent OBSERVED' 'OBSERVED' $ch.findfilePresent.status
    Assert-Equal 'strlen OBSERVED (characters)' 'OBSERVED' $ch.strlenSemantics.status
    Assert-Equal 'productKeyShape OBSERVED' 'OBSERVED' $ch.productKeyShape.status
    Assert-Equal 'openTwoArg OBSERVED' 'OBSERVED' $ch.openTwoArg.status
    Assert-Equal 'openThreeArgUtf8 OBSERVED_DIFFERS (host rejected)' 'OBSERVED_DIFFERS' $ch.openThreeArgUtf8.status
    Assert-True 'the host error text is carried verbatim in the detail' $ch.openThreeArgUtf8.detail.Contains('too many arguments')
    Assert-Equal 'terminator CRLF only file OBSERVED' 'OBSERVED' $ch.writeLineTerminator.status
    Assert-True 'custody true, stop false' ($j.screenCustodyPresent -eq $true -and $j.stopConditionRecorded -eq $false)
    Assert-Equal 'screen entries are sorted by id' 'P2-E03,P2-E05,P2-E07,P2-E08,P2-E09,P2-E10,P2-E13,P2-E14,P2-E15,P2-E16,P2-E19' ((@($j.screen | ForEach-Object { $_.id })) -join ',')
    Assert-Equal 'multi-line text round trips' $okE13 $j.screen[6].text
    Assert-True 'schema validates the record with screen data' (Test-Json -Json $txt -SchemaFile (Join-Path -Path $script:Family -ChildPath 'schemas/ct21d.i1-p2.v1.json'))
    Assert-Equal 'the analyzer wrote nothing next to the inputs' $snapBefore (Get-Snapshot $script:Temp)

    # differences are characterization results
    $diff = @(
        @('P2-E03', 'ERROR_TEXT_VERBATIM', '; error: too many arguments'),
        @('P2-E05', 'OK', 'P2-E05 raw-K.txt-V'),
        @('P2-E07', 'OK', 'P2-E07 VALUE: "D:/x/p2-absent.txt"'),
        @('P2-E08', 'OK', 'P2-E08 VALUE: nil'),
        @('P2-E09', 'OK', 'P2-E09 VALUE: 3'),
        @('P2-E10', 'OK', 'P2-E10 VALUE: 4'),
        @('P2-E15', 'OK', 'P2-E15 VALUE: nil'),
        @('P2-E19', 'OK', 'P2-E19 type=STR len=57 startsExact=nil startsAnyCase=T hasWowAnyCase=nil')
    )
    $sf = Write-ScreenFile (Get-ScreenJson $diff @($script:Shot))
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf)) | ConvertFrom-Json -AsHashtable
    $ch = $j.characterization
    Assert-Equal 'arity control OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.userFunctionArityControl.status
    Assert-Equal 'findfileAbsent OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.findfileAbsent.status
    Assert-Equal 'findfilePresent OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.findfilePresent.status
    Assert-Equal 'strlen bytes OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.strlenSemantics.status
    Assert-True 'strlen detail says bytes' $ch.strlenSemantics.detail.Contains('bytes')
    Assert-Equal 'productKeyShape prefix case OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.productKeyShape.status
    Assert-True 'a difference never sets stopConditionRecorded' ($j.stopConditionRecorded -eq $false)
    Assert-Equal 'a1 file present and no E13 screen: openTwoArg still OBSERVED' 'OBSERVED' $ch.openTwoArg.status

    # escape not interpreted, E10 missing, open nil, shape errors, no custody
    $r = New-RunDir
    $sf = Write-ScreenFile (Get-ScreenJson @(@('P2-E09', 'OK', 'P2-E09 VALUE: 3'), @('P2-E10', 'OK', 'P2-E10 VALUE: 9'), @('P2-E13', 'OK', 'P2-E13 open VALUE: nil'), @('P2-E19', 'ERROR_TEXT_VERBATIM', 'P2-E19 ERROR: no function definition: VLAX-PRODUCT-KEY')) @($script:Shot))
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf)) | ConvertFrom-Json -AsHashtable
    $ch = $j.characterization
    Assert-Equal 'E10 = 9 -> OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.strlenSemantics.status
    Assert-True 'E10 = 9 detail says not interpreted' $ch.strlenSemantics.detail.Contains('not interpreted')
    Assert-Equal 'open returned nil and no file -> UNKNOWN' 'UNKNOWN' $ch.openTwoArg.status
    Assert-Equal 'vlax-product-key not defined -> OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.productKeyShape.status
    Assert-Equal 'no E03/E05 -> arity UNKNOWN' 'UNKNOWN' $ch.userFunctionArityControl.status
    $sf = Write-ScreenFile (Get-ScreenJson @(,@('P2-E09', 'OK', 'P2-E09 VALUE: 3')) @($script:Shot))
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf)) | ConvertFrom-Json -AsHashtable
    Assert-Equal 'E10 missing -> strlen UNKNOWN' 'UNKNOWN' $j.characterization.strlenSemantics.status
    $sf = Write-ScreenFile (Get-ScreenJson @(@('P2-E13', 'ERROR_TEXT_VERBATIM', 'P2-E13 open ERROR: too many arguments'), @('P2-E14', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-TRY')) @($script:Shot))
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf)) | ConvertFrom-Json -AsHashtable
    Assert-Equal 'A1 rejected -> openTwoArg OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.openTwoArg.status
    Assert-True 'A2 aborted expression -> OBSERVED_DIFFERS with the host text' ($j.characterization.openThreeArgUtf8.status -ceq 'OBSERVED_DIFFERS' -and $j.characterization.openThreeArgUtf8.detail.Contains('CT21D-P2-TRY'))

    # no custody: screen-derived items are UNKNOWN, file-derived items survive
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
    $sf = Write-ScreenFile (Get-ScreenJson $good @())
    $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf)) | ConvertFrom-Json -AsHashtable
    Assert-True 'no screenshot hash -> custody false' ($j.screenCustodyPresent -eq $false)
    Assert-Equal 'no custody: strlen UNKNOWN' 'UNKNOWN' $j.characterization.strlenSemantics.status
    Assert-Equal 'no custody: arity UNKNOWN' 'UNKNOWN' $j.characterization.userFunctionArityControl.status
    Assert-Equal 'no custody: productKeyShape UNKNOWN' 'UNKNOWN' $j.characterization.productKeyShape.status
    Assert-Equal 'no custody: the file still shows UTF-8 acceptance' 'OBSERVED' $j.characterization.openThreeArgUtf8.status
    Assert-Equal 'no custody: the file still shows the terminator' 'OBSERVED' $j.characterization.writeLineTerminator.status

    # a stop condition makes everything UNKNOWN
    foreach ($cls in 'DEVIATION', 'REFUSED') {
        $r = New-RunDir
        Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
        $txtEntry = if ($cls -ceq 'REFUSED') { 'P2-E13 REFUSED, exists: p2-a1.txt' } else { 'an unexpected PASTECLIP prompt appeared' }
        $sf = Write-ScreenFile (Get-ScreenJson @(@('P2-E09', 'OK', 'P2-E09 VALUE: 3'), @('P2-E13', $cls, $txtEntry)) @($script:Shot))
        $j = (ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf)) | ConvertFrom-Json -AsHashtable
        Assert-True ($cls + ': stopConditionRecorded true') ($j.stopConditionRecorded -eq $true)
        $allUnknown = (@($j.characterization.Values | Where-Object { $_.status -cne 'UNKNOWN' }).Count -eq 0)
        Assert-True ($cls + ': every item UNKNOWN even with a good file present') $allUnknown
    }

    # =====================================================================================================================================
    Start-Section 'hostile or malformed screen input'
    $r = New-RunDir
    $badInputs = [ordered]@{
        'unknown top-level member'      = '{"screen":[],"screenshotSha256":[],"extra":1}'
        'duplicate top-level member'    = '{"screen":[],"screen":[]}'
        'duplicate screen id'           = '{"screen":[{"id":"P2-E09","classification":"OK","text":"P2-E09 VALUE: 3"},{"id":"P2-E09","classification":"OK","text":"P2-E09 VALUE: 3"}]}'
        'duplicate member in an entry'  = '{"screen":[{"id":"P2-E09","id":"P2-E10","classification":"OK","text":"x"}]}'
        'unknown member in an entry'    = '{"screen":[{"id":"P2-E09","classification":"OK","text":"x","note":"y"}]}'
        'missing member'                = '{"screen":[{"id":"P2-E09","classification":"OK"}]}'
        'id out of range'               = '{"screen":[{"id":"P2-E20","classification":"OK","text":"x"}]}'
        'id of another family'          = '{"screen":[{"id":"S1-E01","classification":"OK","text":"x"}]}'
        'bad classification'            = '{"screen":[{"id":"P2-E09","classification":"PASS","text":"x"}]}'
        'OK contradicts the text'       = '{"screen":[{"id":"P2-E09","classification":"OK","text":"P2-E09 ERROR: boom"}]}'
        'OK with REFUSED text'          = '{"screen":[{"id":"P2-E13","classification":"OK","text":"P2-E13 REFUSED, exists: x"}]}'
        'ERROR class without error text'= '{"screen":[{"id":"P2-E09","classification":"ERROR_TEXT_VERBATIM","text":"P2-E09 VALUE: 3"}]}'
        'REFUSED class without text'    = '{"screen":[{"id":"P2-E13","classification":"REFUSED","text":"nothing"}]}'
        'text is not a string'          = '{"screen":[{"id":"P2-E09","classification":"OK","text":5}]}'
        'screen is not an array'        = '{"screen":{}}'
        'screenshot hash upper case'    = '{"screenshotSha256":["' + ('AB' * 32) + '"]}'
        'screenshot hash too short'     = '{"screenshotSha256":["abcd"]}'
        'duplicate screenshot hash'     = '{"screenshotSha256":["' + $script:Shot + '","' + $script:Shot + '"]}'
        'trailing comma'                = '{"screen":[],}'
        'not JSON'                      = 'screen'
        'root is an array'              = '[]'
        'text too long'                 = '{"screen":[{"id":"P2-E09","classification":"OK","text":"' + ('a' * 4001) + '"}]}'
        'product key value in a text'   = '{"screen":[{"id":"P2-E19","classification":"OK","text":"SOFTWARE\\Autodesk\\AutoCAD\\R25.0\\ACAD-8101:409"}]}'
        'registry-like value, other case' = '{"screen":[{"id":"P2-E19","classification":"OK","text":"software\\autodesk\\x"}]}'
        'names TRUSTEDPATHS'            = '{"screen":[{"id":"P2-E09","classification":"OK","text":"TRUSTEDPATHS=C:/x"}]}'
        'names CPROFILE'                = '{"screen":[{"id":"P2-E09","classification":"OK","text":"cprofile value"}]}'
        'names MachineGuid'             = '{"screen":[{"id":"P2-E09","classification":"OK","text":"MachineGuid 1234"}]}'
    }
    foreach ($k in $badInputs.Keys) {
        $sf = Write-ScreenFile $badInputs[$k]
        $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf
        Assert-True ('refused: ' + $k) ($res.Exit -eq 2 -and $res.Bytes.Length -eq 0 -and $res.Err.StartsWith('REFUSED:')) ('exit=' + $res.Exit + ' err=' + $res.Err)
    }
    $bomFile = Join-Path -Path $script:Temp -ChildPath 'bom.json'
    [System.IO.File]::WriteAllBytes($bomFile, [byte[]](0xEF, 0xBB, 0xBF) + [System.Text.Encoding]::ASCII.GetBytes('{}'))
    Assert-Equal 'refused: BOM in the screen input' 2 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $bomFile).Exit
    $badUtf8 = Join-Path -Path $script:Temp -ChildPath 'bad8.json'
    [System.IO.File]::WriteAllBytes($badUtf8, [System.Text.Encoding]::ASCII.GetBytes('{"screen":[{"id":"P2-E09","classification":"OK","text":"') + [byte[]](0xFF) + [System.Text.Encoding]::ASCII.GetBytes('"}]}'))
    Assert-Equal 'refused: invalid UTF-8 in the screen input' 2 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $badUtf8).Exit
    Assert-Equal 'refused: a relative screen input path' 2 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen 'screen.json').Exit
    Assert-Equal 'refused: a missing screen input file' 2 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen (Join-Path -Path $script:Temp -ChildPath 'nope.json')).Exit
    Assert-Equal 'refused: the screen input is a directory' 2 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $r.Dir).Exit
    $bigFile = Join-Path -Path $script:Temp -ChildPath 'big.json'
    [System.IO.File]::WriteAllBytes($bigFile, [byte[]]::new(262145))
    Assert-Equal 'refused: an oversized screen input' 2 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $bigFile).Exit
    Assert-Equal 'an empty object is a valid screen input' 0 (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen (Write-ScreenFile '{}')).Exit

    # non-ASCII verbatim text is escaped, never emitted raw
    $sf = Write-ScreenFile ('{"screen":[{"id":"P2-E09","classification":"OK","text":"P2-E09 VALUE: 3 ' + [char]0xE9 + '"}],"screenshotSha256":["' + $script:Shot + '"]}')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf
    Assert-Equal 'non-ASCII text accepted' 0 $res.Exit
    $o = ConvertFrom-Out $res
    Assert-True 'non-ASCII text is emitted as a six-character escape' ($o.Contains([string][char]92 + 'u00e9') -and ($o.Substring(0, $o.Length - 1) -cmatch '^[\x20-\x7e]+$'))

    # =====================================================================================================================================
    Start-Section 'schema negative controls'
    $schema = Join-Path -Path $script:Family -ChildPath 'schemas/ct21d.i1-p2.v1.json'
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
    $goodJson = ConvertFrom-Out (Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen (Write-ScreenFile (Get-ScreenJson @(,@('P2-E09', 'OK', 'P2-E09 VALUE: 3')) @($script:Shot))))
    Assert-True 'baseline record validates' (Test-Json -Json $goodJson -SchemaFile $schema)
    Assert-True 'control: the unmutated re-serialization validates (the mutation harness is not vacuous)' (Test-Json -Json (($goodJson | ConvertFrom-Json -AsHashtable) | ConvertTo-Json -Depth 10 -Compress) -SchemaFile $schema)
    $mutations = [ordered]@{
        'extra top-level property'      = { param($o) $o['extra'] = 1 }
        'governing true'                = { param($o) $o['governing'] = $true }
        'activity H1'                   = { param($o) $o['activity'] = 'H1' }
        'protocol family 2'             = { param($o) $o['protocolFamily'] = '2' }
        'run id of H1'                  = { param($o) $o['runId'] = 'HGP-H1-20261002T100000Z-01' }
        'schema version'                = { param($o) $o['schemaVersion'] = 'ct21d.i1-p2.v2' }
        'p2 hash upper case'            = { param($o) $o['p2TextSha256'] = ('AB' * 32) }
        'ledger hash short'             = { param($o) $o['ledgerSha256'] = 'abcd' }
        'file name not fixed'           = { param($o) $o['files'][0]['name'] = 'p2-a3.txt' }
        'terminator not in enum'        = { param($o) $o['files'][1]['terminator'] = 'LFCR' }
        'encoding not in enum'          = { param($o) $o['files'][1]['encodingInterpretation'] = 'UTF16' }
        'extra file property'           = { param($o) $o['files'][1]['extra'] = 1 }
        'hex with odd length'           = { param($o) $o['files'][1]['first64BytesHex'] = 'abc' }
        'three files'                   = { param($o) $o['files'] = @($o['files'][0], $o['files'][1], $o['files'][1]) }
        'length above the cap'          = { param($o) $o['files'][1]['length'] = 4097 }
        'status not in enum'            = { param($o) $o['characterization']['strlenSemantics']['status'] = 'MAYBE' }
        'characterization item missing' = { param($o) $o['characterization'].Remove('productKeyShape') }
        'characterization extra item'   = { param($o) $o['characterization']['other'] = @{ status = 'OBSERVED'; detail = '' } }
        'screen id out of range'        = { param($o) $o['screen'][0]['id'] = 'P2-E20' }
        'screen classification'         = { param($o) $o['screen'][0]['classification'] = 'PASS' }
        'screenshot hash not hex'       = { param($o) $o['screenshotSha256'] = @('zz') }
        'custody flag missing'          = { param($o) $o.Remove('screenCustodyPresent') }
    }
    foreach ($k in $mutations.Keys) {
        $o = $goodJson | ConvertFrom-Json -AsHashtable
        & $mutations[$k] $o
        $mj = $o | ConvertTo-Json -Depth 10 -Compress
        Assert-True ('schema rejects: ' + $k) (-not (Test-Json -Json $mj -SchemaFile $schema -ErrorAction SilentlyContinue))
    }
}
finally {
    if (Test-Path -LiteralPath $script:Temp) { Remove-Item -LiteralPath $script:Temp -Recurse -Force -ErrorAction SilentlyContinue }
}

Write-Host ''
Write-Host ('TESTS passed=' + $script:Pass + ' failed=' + $script:Fail + ' skipped=' + $script:Skipped + ' total=' + ($script:Pass + $script:Fail))
foreach ($f in $script:Failures) { Write-Host ('  FAILED: ' + $f) }
if ($script:Fail -gt 0) { exit 1 }
exit 0
