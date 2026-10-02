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
$script:MaxLine = 255

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
    $run = 'HGP-H10-20261002T100000Z-{0:00}' -f $script:RunCounter
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
    param([array]$Entries, [array]$Shots, [array]$Checkpoints = @())
    $o = [ordered]@{ screen = @($Entries | ForEach-Object { [ordered]@{ id = $_[0]; classification = $_[1]; text = $_[2] } }); screenshotSha256 = @($Shots) }
    if ($Checkpoints.Count -gt 0) { $o['checkpoints'] = @($Checkpoints | ForEach-Object { [ordered]@{ after = $_[0]; sha256 = $_[1] } }) }
    return ($o | ConvertTo-Json -Depth 6 -Compress)
}

function Write-ScreenFile {
    param([string]$Json, [string]$Name = 'screen.json')
    $p = Join-Path -Path $script:Temp -ChildPath ([guid]::NewGuid().ToString('N') + '-' + $Name)
    [System.IO.File]::WriteAllBytes($p, [System.Text.UTF8Encoding]::new($false).GetBytes($Json))
    return $p
}

function Invoke-WithScreen {
    param($Run, [array]$Entries, [array]$Shots, [array]$Checkpoints = @())
    $sf = Write-ScreenFile (Get-ScreenJson $Entries $Shots $Checkpoints)
    return (Invoke-Analyzer -Dir $Run.Dir -Run $Run.Run -Screen $sf)
}

function ConvertFrom-Record { param($Res) return ((ConvertFrom-Out $Res) | ConvertFrom-Json -AsHashtable) }

$script:Shot = ('ab' * 32)
$script:Shot2 = ('cd' * 32)

# AutoLISP one-line checker: printable ASCII only, no comment, one balanced top-level form, balanced quotes
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

# the full per-line gate of the allowlist: length, ASCII, parentheses, quotes
function Test-AllowlistLine {
    param([string]$Line)
    if ($Line.Length -gt $script:MaxLine) { return 'too long' }
    return (Test-LispLine -Line $Line)
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

function Get-Sha256Of { param([byte[]]$B) return [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($B)).ToLowerInvariant() }

try {
    # =====================================================================================================================================
    Start-Section 'allowlist: the closed set of typed expressions'
    $allowPath = Join-Path -Path $script:Family -ChildPath 'P2-ALLOWLIST.txt'
    $allowBytes = [System.IO.File]::ReadAllBytes($allowPath)
    $allowText = [System.Text.Encoding]::ASCII.GetString($allowBytes)
    Assert-True 'allowlist is pure ASCII (no byte above 0x7E except none)' (@($allowBytes | Where-Object { $_ -gt 126 -or ($_ -lt 32 -and $_ -ne 10) }).Count -eq 0)
    Assert-True 'allowlist has no CR' (-not $allowText.Contains("`r"))
    Assert-True 'allowlist ends with exactly one LF' ($allowText.EndsWith("`n") -and -not $allowText.EndsWith("`n`n"))
    $lines = @($allowText.Substring(0, $allowText.Length - 1).Split("`n"))
    Assert-Equal 'allowlist has 25 expressions' 25 $lines.Count
    $i = 0
    $maxLen = 0
    foreach ($l in $lines) {
        $i++
        if ($l.Length -gt $maxLen) { $maxLen = $l.Length }
        Assert-Equal ('line {0:00} is one balanced ASCII form within the length limit' -f $i) 'OK' (Test-AllowlistLine -Line $l)
        Assert-True ('line {0:00} is at most 255 characters' -f $i) ($l.Length -le 255) ([string]$l.Length)
    }
    Write-Host ('MAX_EXPRESSION_LENGTH=' + $maxLen)
    $substituted = $lines[0].Replace('<RUNID>', 'HGP-H10-20261002T100000Z-01')
    Assert-True 'the substituted line P2-E01 is also at most 255 characters' ($substituted.Length -le 255 -and (Test-LispLine -Line $substituted) -ceq 'OK')
    Assert-Equal 'exactly one <RUNID> token' 1 ([regex]::Matches($allowText, '<RUNID>').Count)
    Assert-True '<RUNID> is in line 1' ($lines[0].Contains('<RUNID>'))
    Assert-True 'no other angle-bracket tokens' ([regex]::Matches($allowText, '[<>]').Count -eq 2)
    $headOk = $true; $nsOk = $true
    foreach ($l in $lines) {
        if ($l -cmatch '^\(defun (\S+) ') { if (-not $Matches[1].StartsWith('ct21d-p2-')) { $nsOk = $false } }
        elseif ($l -cmatch '^\(setq (\S+) ') { if ($Matches[1] -cne 'ct21d-p2-root') { $nsOk = $false } }
        elseif ($l -cmatch '^\((progn|ct21d-p2-try|ct21d-p2-shape) ') { }
        else { $headOk = $false }
    }
    Assert-True 'every top-level head is defun / setq / progn / try / shape' $headOk
    Assert-True 'every defun and every global setq is in the ct21d-p2- namespace' $nsOk
    $forbiddenLisp = @('(load ', '(command', '(setvar', 'vl-registry', 'vla-', 'startapp', 'vl-file', 'vl-mkdir', 'delete', 'getenv', 'setenv', 'dos_', 'vl-cmdf', 'arxload', 'appload', 'netload', '(getvar', '(open ', 'sssetfirst', 'fboundp')
    foreach ($f in $forbiddenLisp) { Assert-True ('allowlist does not contain ' + $f) (-not $allowText.Contains($f)) }
    foreach ($g in 'cprofile', 'secureload', 'trustedpaths', 'machineguid', 'osversionbuild', 'getvar') {
        Assert-True ('P2 does not read or name ' + $g) (-not $allowText.ToLowerInvariant().Contains($g))
    }
    Assert-Equal 'vlax- appears only in the one product-key call' 1 ([regex]::Matches($allowText, 'vlax-').Count)
    Assert-True 'the product-key call is the last line and is passed to the shape helper only' ($lines[24] -ceq '(ct21d-p2-shape "P2-E25" (vl-catch-all-apply ''vlax-product-key nil))')
    Assert-True 'the product-key value is never stored: only three setq exist (the root, and the local variables of the control and try helpers)' ((([regex]::Matches($allowText, '\(setq ')).Count -eq 3) -and $lines[0].StartsWith('(setq ct21d-p2-root ') -and $lines[1].Contains('(setq p name f text)') -and $lines[14].Contains('(setq p (strcat ct21d-p2-root name))'))
    Assert-True 'write-line is called only in the helper of line 12' (([regex]::Matches($allowText, '\(write-line')).Count -eq 3 -and ([regex]::Matches($lines[11], '\(write-line')).Count -eq 3)
    $quoted = [string[]]@([regex]::Matches($allowText, "'([A-Za-z][A-Za-z0-9-]*)") | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
    [System.Array]::Sort($quoted, [System.StringComparer]::Ordinal)
    Assert-Equal 'the only quoted symbols are the expected ones' 'FILE,STR,ascii,close,ct21d-p2-lines,findfile,open,strlen,vl-load-com,vlax-product-key' ($quoted -join ',')
    # the file descriptor is never stringified
    Assert-True 'the display helper (line 6) maps a file descriptor to the constant token and tests it before prin1' ($lines[5].Contains('(= (type r) ''FILE) "VALUE: FILE-DESCRIPTOR"') -and $lines[5].IndexOf('FILE-DESCRIPTOR') -lt $lines[5].IndexOf('vl-prin1-to-string'))
    Assert-True 'no other line stringifies a value except the display helper and the type printer of the shape helper' (([regex]::Matches($allowText, 'vl-prin1-to-string')).Count -eq 2 -and $lines[5].Contains('vl-prin1-to-string') -and $lines[23].Contains('(vl-prin1-to-string (type k))'))
    # the printed ids are the line numbers
    $idOk = $true
    $i = 0
    foreach ($l in $lines) {
        $i++
        foreach ($m in [regex]::Matches($l, 'P2-E([0-9]{2})')) { if ([int]$m.Groups[1].Value -ne $i) { $idOk = $false } }
    }
    Assert-True 'every P2-Enn printed or passed by line N is N' $idOk
    # table equality, both directions
    $mdText = [System.IO.File]::ReadAllText((Join-Path -Path $script:Family -ChildPath 'I-1-observation-protocol-v2-P2.md'), [System.Text.UTF8Encoding]::new($false, $true))
    $s5 = $mdText.IndexOf("`n## 5. ", [System.StringComparison]::Ordinal)
    $s6 = $mdText.IndexOf("`n## 6. ", [System.StringComparison]::Ordinal)
    Assert-True 'section 5 and section 6 headings are found in order' ($s5 -ge 0 -and $s6 -gt $s5)
    $sec5 = $mdText.Substring($s5, $s6 - $s5)
    $rows = @([regex]::Matches($sec5, '(?m)^\| P2-E([0-9]{2}) \| `([^`\r\n]+)` \| ') | ForEach-Object { , @($_.Groups[1].Value, $_.Groups[2].Value) })
    Assert-Equal 'the table of the P2 text has 25 expression rows' 25 $rows.Count
    $i = 0
    foreach ($row in $rows) {
        $i++
        Assert-True ('table row {0:00} id and text equal line {0:00} of the allowlist file' -f $i) ($row[0] -ceq ('{0:00}' -f $i) -and $row[1] -ceq $lines[$i - 1])
    }
    $i = 0
    foreach ($l in $lines) {
        $i++
        $row = '| P2-E{0:00} | `{1}` | ' -f $i, $l
        Assert-True ('the table row of P2-E{0:00} in the P2 text equals the allowlist line' -f $i) ($mdText.Contains($row))
    }
    Assert-True 'the P2 text has no CR and no BOM' (-not $mdText.Contains("`r") -and -not $mdText.StartsWith([string][char]0xFEFF, [System.StringComparison]::Ordinal))
    Assert-True 'the P2 text records both full v1 blob ids' ($mdText.Contains('3a77ba62da77dfa2fc8bd2ffeb176a9454864193') -and $mdText.Contains('a9081cff612b2290c35c86c0ded72f42c62b5e5a'))
    # negative controls of the checkers themselves
    Assert-Equal 'checker flags an unbalanced form' 'unbalanced' (Test-LispLine -Line '(a (b)')
    Assert-Equal 'checker flags an extra close' 'extra close' (Test-LispLine -Line '(a))(b)')
    Assert-Equal 'checker flags two forms' 'more than one top-level form' (Test-LispLine -Line '(a)(b)')
    Assert-Equal 'checker flags a comment' 'comment' (Test-LispLine -Line '(a) ; x')
    Assert-Equal 'checker flags non-ASCII' 'non-ASCII or control' (Test-LispLine -Line ('(a "' + [char]0xE9 + '")'))
    Assert-Equal 'checker flags an open string' 'open string' (Test-LispLine -Line '(a "b)')
    Assert-Equal 'checker accepts a backslash-quote inside a string' 'OK' (Test-LispLine -Line '(a "b\"c")')
    Assert-Equal 'length gate accepts exactly 255 characters' 'OK' (Test-AllowlistLine -Line ('(' + ('a' * 253) + ')'))
    Assert-Equal 'length gate flags 256 characters' 'too long' (Test-AllowlistLine -Line ('(' + ('a' * 254) + ')'))
    Assert-Equal 'length gate flags a long balanced line (the old 487-character form)' 'too long' (Test-AllowlistLine -Line ('(progn ' + (('(princ "x") ') * 40) + ')'))
    Assert-Equal 'length gate flags an unbalanced short line' 'unbalanced' (Test-AllowlistLine -Line '(progn (princ "x")')
    Assert-Equal 'length gate flags unbalanced quotes in a short line' 'open string' (Test-AllowlistLine -Line '(princ "x)')
    Assert-Equal 'length gate flags a non-ASCII short line' 'non-ASCII or control' (Test-AllowlistLine -Line ('(princ "' + [char]0xE9 + '")'))
    # a file that differs from the table is detected by the same comparison
    $tamperedRows = @($lines.Clone()); $tamperedRows[3] = $tamperedRows[3] + ' '
    Assert-True 'negative control: a changed allowlist line no longer matches its table row' (-not $mdText.Contains('| P2-E04 | `' + $tamperedRows[3] + '` | '))

    # =====================================================================================================================================
    Start-Section 'P2 text: ruled content'
    Assert-True 'activity H10 and the H10 RunId pattern' ($mdText.Contains('ACTIVITY        = H10') -and $mdText.Contains('^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$'))
    Assert-True 'no H5 RunId remains' (-not $mdText.Contains('HGP-H5-') -and -not $mdText.Contains('<H5-RunId>'))
    Assert-True 'the CAD manager pre-host attestation is named and has a fill-in template' ($mdText.Contains('CAD-MANAGER PRE-HOST ATTESTATION') -and $mdText.Contains('ATTESTATION TEMPLATE'))
    Assert-True 'the phase-2 capture waiver is recorded' ($mdText.Contains('PHASE2_CAPTURE_FOR_P2 = WAIVED'))
    Assert-True 'the attestation is never called phase-2 binding' (-not $mdText.ToLowerInvariant().Contains('phase-2 binding') -and -not $mdText.ToLowerInvariant().Contains('phase 2 binding'))
    Assert-True 'the TRUSTEDPATHS baseline hash and entry count are in the attestation' ($mdText.Contains('a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498') -and $mdText.Contains('19 entries'))
    Assert-True 'the screenshot limitation and its sha256 are recorded' ($mdText.Contains('9837dcd08445a3cc53954dbf91583a0485d76414e0e03d67a17c14d90b089818') -and $mdText.Contains('cropped at the top'))
    Assert-True 'no stringified file descriptor appears in the P2 text' (-not $mdText.Contains('#<file'))
    Assert-True 'the file descriptor token is the vocabulary' ($mdText.Contains('FILE-DESCRIPTOR'))
    Assert-True 'the closed result model names its six results' (@(@('SUPPORTED', 'UNSUPPORTED', 'HOST_ERROR', 'OPERATOR_DEVIATION', 'INCOMPLETE', 'UNKNOWN') | Where-Object { -not $mdText.Contains($_) }).Count -eq 0)
    Assert-True 'the form verdict DIFFERENT and the C2 requirement are stated' ($mdText.Contains('DIFFERENT') -and $mdText.Contains('C3 A9'))
    Assert-True 'the findfile gate is a conditional gate with STOP_BEFORE_OPEN' ($mdText.Contains('STOP_BEFORE_OPEN') -and $mdText.Contains('P2-E10 VALUE: nil'))
    Assert-True 'the operator discipline prohibits the clipboard path and names the stop signals' ($mdText.Contains('Ctrl+V') -and $mdText.Contains('_pasteclip') -and $mdText.Contains('PASTECLIP') -and $mdText.Contains('Specify insertion point') -and $mdText.Contains('press ESC once'))
    Assert-True 'the five checkpoint screenshots are named' (@(@('after P2-E05', 'after P2-E09', 'after P2-E17', 'after P2-E19', 'after P2-E25') | Where-Object { -not $mdText.Contains($_) }).Count -eq 0)
    Assert-True 'the ambiguous old wording is gone' (-not $mdText.Contains('executes a paste without waiting'))
    Assert-True 'strlen labels are characterization labels' ($mdText.Contains('characterization label') -and $mdText.Contains('do not assume strlen counts bytes'))
    Assert-True 'C2 is not written' (-not $mdText.Contains('ACTIVITY        = H11') -and $mdText.Contains('C2_TEXT = NOT_WRITTEN'))

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
    Assert-True 'the analyzer uses the H10 pattern and no H5 pattern' ($src.Contains('^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$') -and -not $src.Contains('HGP-H5-'))

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
            Assert-Equal ('ledger hash of ' + $Matches[2]) $Matches[1] (Get-Sha256Of ([System.IO.File]::ReadAllBytes($full)))
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
    Assert-Equal 'activity' 'H10' $j.activity
    Assert-True 'governing false' ($j.governing -eq $false)
    Assert-Equal 'protocol family' 'I1-F2' $j.protocolFamily
    Assert-Equal 'run id' $r.Run $j.runId
    Assert-Equal 'p2TextSha256 is the sha256 of the P2 text' (Get-Sha256Of ([System.IO.File]::ReadAllBytes((Join-Path -Path $script:Family -ChildPath 'I-1-observation-protocol-v2-P2.md')))) $j.p2TextSha256
    Assert-Equal 'ledgerSha256 is the sha256 of the ledger' (Get-Sha256Of ([System.IO.File]::ReadAllBytes($ledgerPath))) $j.ledgerSha256
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
    Assert-Equal 'a2 sha256' (Get-Sha256Of (Get-FileBytes "`r`n" $script:Utf8E)) $f2.sha256
    Assert-Equal 'a2 first 64 bytes hex' ([System.Convert]::ToHexString((Get-FileBytes "`r`n" $script:Utf8E)[0..([Math]::Min(63, (Get-FileBytes "`r`n" $script:Utf8E).Length - 1))]).ToLowerInvariant()) $f2.first64BytesHex
    Assert-Equal 'terminator characterization OBSERVED' 'OBSERVED' $j.characterization.writeLineTerminator.status
    Assert-Equal 'file only: a1 accepted but the literal escape differs from the C2 requirement -> DIFFERENT' 'DIFFERENT' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'file only: a1 DIFFERENT maps to OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.openTwoArg.status
    Assert-Equal 'file only: a2 UTF-8 without BOM, C3A9, CRLF, three lines -> SUPPORTED' 'SUPPORTED' $j.characterization.openThreeArgUtf8.formVerdict
    Assert-Equal 'file only: a2 maps to OBSERVED' 'OBSERVED' $j.characterization.openThreeArgUtf8.status
    Assert-Equal 'no screen: the gate is UNKNOWN' 'UNKNOWN' $j.openGate
    Assert-Equal 'no screen: five checkpoints missing' 'P2-E05,P2-E09,P2-E17,P2-E19,P2-E25' ($j.checkpointsMissing -join ',')
    Assert-True 'schema validates the good record' (Test-Json -Json $outText -SchemaFile (Join-Path -Path $script:Family -ChildPath 'schemas/ct21d.i1-p2.v1.json'))
    $res2 = Invoke-Analyzer -Dir $r.Dir -Run $r.Run
    Assert-True 'deterministic: two runs give identical bytes' ([System.Convert]::ToHexString([byte[]]$res.Bytes) -ceq [System.Convert]::ToHexString([byte[]]$res2.Bytes))
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
    $jo = $outText | ConvertFrom-Json -AsHashtable
    Assert-True 'keys are sorted ordinally at every level' (& $check $jo)

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`n" $script:Utf8E ([byte[]](0xEF, 0xBB, 0xBF)))
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:LatinE)
    $j = ConvertFrom-Record (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)
    Assert-True 'BOM file: bomDetected' ($j.files[0].bomDetected -eq $true)
    Assert-Equal 'BOM file: UTF8_WITH_BOM' 'UTF8_WITH_BOM' $j.files[0].encodingInterpretation
    Assert-True 'BOM file: expected lines still recognized' ($j.files[0].expectedLinesPresent -eq $true)
    Assert-Equal 'BOM file: the form is DIFFERENT from the C2 requirement' 'DIFFERENT' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'single-byte e-acute: SINGLE_BYTE' 'SINGLE_BYTE' $j.files[1].encodingInterpretation
    Assert-Equal 'single-byte e-acute: bytes e9' 'e9' $j.files[1].nonAsciiBytesHex
    Assert-Equal 'single-byte e-acute: terminator LF' 'LF' $j.files[1].terminator
    Assert-Equal 'single-byte e-acute: DIFFERENT' 'DIFFERENT' $j.characterization.openThreeArgUtf8.formVerdict

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' ([System.Text.Encoding]::Unicode.GetPreamble() + [System.Text.Encoding]::Unicode.GetBytes("P2-ASCII-LINE-1`nP2-ACCENT-" + [char]0xE9 + "-END`nP2-ASCII-LINE-3`n"))
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`r" $script:Utf8E)
    $j = ConvertFrom-Record (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)
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
    $j = ConvertFrom-Record (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)
    Assert-Equal 'no terminator: NONE' 'NONE' $j.files[0].terminator
    Assert-Equal 'mixed terminators: MIXED' 'MIXED' $j.files[1].terminator
    Assert-Equal 'NONE/MIXED terminator is OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.writeLineTerminator.status

    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' ([byte[]]@())
    $j = ConvertFrom-Record (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)
    Assert-True 'empty file exists with length 0' ($j.files[0].exists -eq $true -and $j.files[0].length -eq 0)
    Assert-Equal 'empty file: UNKNOWN encoding' 'UNKNOWN' $j.files[0].encodingInterpretation
    Assert-Equal 'empty file sha256 is the empty-input hash' 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855' $j.files[0].sha256

    # =====================================================================================================================================
    Start-Section 'missing, extra, oversized and hostile entries'
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-Analyzer -Dir $r.Dir -Run $r.Run)
    Assert-True 'no files: both absent' ($j.files[0].exists -eq $false -and $j.files[1].exists -eq $false)
    Assert-Equal 'no files: terminator NOT_OBSERVABLE' 'NOT_OBSERVABLE' $j.characterization.writeLineTerminator.status
    Assert-Equal 'no files: no screen -> openTwoArg UNKNOWN' 'UNKNOWN' $j.characterization.openTwoArg.status
    Assert-Equal 'no files: no screen -> form verdict UNKNOWN' 'UNKNOWN' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'no files: strlen UNKNOWN' 'UNKNOWN' $j.characterization.strlenSemantics.status
    Assert-Equal 'no files: escape UNKNOWN' 'UNKNOWN' $j.characterization.escapeInterpretation.status
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

    $real = New-RunDir
    $linkRun = 'HGP-H10-20261002T100000Z-99'
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
    $res = Invoke-Analyzer -Dir (Join-Path -Path $script:Temp -ChildPath 'HGP-H10-20261002T100000Z-77') -Run 'HGP-H10-20261002T100000Z-77'
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
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'HGP-H10-20261002T100000Z-02'
    Assert-Equal 'a leaf that differs from the RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'HGP-H1-20261002T100000Z-01'
    Assert-Equal 'an H1 RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'HGP-H5-20261002T100000Z-01'
    Assert-Equal 'an H5 RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run 'hgp-h10-20261002t100000z-01'
    Assert-Equal 'a lower-case RunId is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir $r.Dir -Run ($r.Run + 'x')
    Assert-Equal 'a RunId with a suffix is refused' 2 $res.Exit
    $res = Invoke-Analyzer -Dir 'C:\Windows\System32\drivers' -Run 'HGP-H10-20261002T100000Z-01'
    Assert-Equal 'a system directory with the wrong leaf is refused' 2 $res.Exit
    Assert-True 'every refusal printed nothing on stdout' ($res.Bytes.Length -eq 0)

    # =====================================================================================================================================
    Start-Section 'screen results and the closed result model'
    $r = New-RunDir
    $okE16 = "P2-E16 open VALUE: FILE-DESCRIPTOR`nP2-E16 write-line VALUE: `"P2-ASCII-LINE-3`"`nP2-E16 close VALUE: nil"
    $pk ='P2-E25 type=STR len=57 startsExact=T startsAnyCase=T hasWowAnyCase=nil'
    $good = @(
        @('P2-E01', 'OK', ('"D:/I52-CT21D-HOST/evidence/' + $r.Run + '/"')),
        @('P2-E02', 'OK', 'CT21D-P2-CTL'),
        @('P2-E03', 'OK', 'P2-E03 AB-CD'),
        @('P2-E04', 'OK', 'CT21D-P2-CTL2'),
        @('P2-E05', 'OK', 'P2-E05 raw-K.txt-V'),
        @('P2-E06', 'OK', 'CT21D-P2-SHOW'),
        @('P2-E07', 'OK', 'P2-E07 VALUE: 3'),
        @('P2-E08', 'OK', 'P2-E08 VALUE: 233'),
        @('P2-E09', 'OK', 'P2-E09 VALUE: 3'),
        @('P2-E10', 'OK', 'P2-E10 VALUE: nil'),
        @('P2-E11', 'OK', 'P2-E11 VALUE: "C:\\Windows\\System32\\kernel32.dll"'),
        @('P2-E16', 'OK', $okE16),
        @('P2-E17', 'ERROR_TEXT_VERBATIM', 'P2-E17 open ERROR: too many arguments'),
        @('P2-E18', 'OK', 'P2-E18 VALUE: "D:/x/p2-a1.txt"'),
        @('P2-E19', 'OK', 'P2-E19 VALUE: nil'),
        @('P2-E25', 'OK', $pk)
    )
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`r`n" $script:Utf8E)
    $cps = @(@('P2-E05', $script:Shot), @('P2-E09', $script:Shot2))
    $sf = Write-ScreenFile (Get-ScreenJson $good @($script:Shot, $script:Shot2) $cps)
    $snapBefore = Get-Snapshot $script:Temp
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf
    Assert-Equal 'exit 0 with a screen input' 0 $res.Exit
    $txt = ConvertFrom-Out $res
    $j = $txt | ConvertFrom-Json -AsHashtable
    $ch = $j.characterization
    Assert-Equal 'arity control OBSERVED' 'OBSERVED' $ch.userFunctionArityControl.status
    Assert-Equal 'findfileAbsent OBSERVED' 'OBSERVED' $ch.findfileAbsent.status
    Assert-Equal 'findfilePresent OBSERVED' 'OBSERVED' $ch.findfilePresent.status
    Assert-Equal 'strlen OBSERVED (character-interpretation candidate)' 'OBSERVED' $ch.strlenSemantics.status
    Assert-Equal 'strlen label' 'CHARACTER_INTERPRETATION_CANDIDATE' $ch.strlenSemantics.label
    Assert-Equal 'escape OBSERVED separately' 'OBSERVED' $ch.escapeInterpretation.status
    Assert-Equal 'escape printed code recorded separately' '233' $ch.escapeInterpretation.printedCode
    Assert-Equal 'escape label' 'ACCENT_CODEPOINT_LIKE' $ch.escapeInterpretation.label
    Assert-Equal 'productKeyShape OBSERVED' 'OBSERVED' $ch.productKeyShape.status
    Assert-Equal 'openTwoArg: descriptor, CRLF UTF-8 C3A9 three lines -> SUPPORTED' 'SUPPORTED' $ch.openTwoArg.formVerdict
    Assert-Equal 'openTwoArg result SUPPORTED' 'SUPPORTED' $ch.openTwoArg.result
    Assert-Equal 'openTwoArg status OBSERVED' 'OBSERVED' $ch.openTwoArg.status
    Assert-Equal 'openThreeArgUtf8: arity rejection -> UNSUPPORTED' 'UNSUPPORTED' $ch.openThreeArgUtf8.formVerdict
    Assert-Equal 'openThreeArgUtf8 result UNSUPPORTED' 'UNSUPPORTED' $ch.openThreeArgUtf8.result
    Assert-Equal 'openThreeArgUtf8 maps to OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.openThreeArgUtf8.status
    Assert-True 'the host error text is carried verbatim in the detail' $ch.openThreeArgUtf8.detail.Contains('too many arguments')
    Assert-Equal 'terminator CRLF only file OBSERVED' 'OBSERVED' $ch.writeLineTerminator.status
    Assert-Equal 'the gate passed' 'PASSED' $j.openGate
    Assert-True 'custody true, stop false' ($j.screenCustodyPresent -eq $true -and $j.stopConditionRecorded -eq $false)
    Assert-Equal 'two checkpoints recorded, three missing' 'P2-E17,P2-E19,P2-E25' ($j.checkpointsMissing -join ',')
    Assert-Equal 'checkpoints are sorted by id' 'P2-E05,P2-E09' ((@($j.checkpoints | ForEach-Object { $_.after })) -join ',')
    Assert-Equal 'screen entries are sorted by id' 'P2-E01,P2-E02,P2-E03,P2-E04,P2-E05,P2-E06,P2-E07,P2-E08,P2-E09,P2-E10,P2-E11,P2-E16,P2-E17,P2-E18,P2-E19,P2-E25' ((@($j.screen | ForEach-Object { $_.id })) -join ',')
    $er = @{}; foreach ($x in $j.expressionResults) { $er[$x.id] = $x }
    Assert-Equal 'one result per transcribed expression' 16 $j.expressionResults.Count
    Assert-Equal 'E01 echo equal to the root is SUPPORTED' 'SUPPORTED' $er['P2-E01'].result
    Assert-Equal 'a defun echo is SUPPORTED' 'SUPPORTED' $er['P2-E06'].result
    Assert-Equal 'E17 result UNSUPPORTED with category ARITY' 'UNSUPPORTED/ARITY' ($er['P2-E17'].result + '/' + $er['P2-E17'].category)
    Assert-Equal 'multi-line text round trips' $okE16 ($j.screen | Where-Object { $_.id -ceq 'P2-E16' }).text
    Assert-True 'schema validates the record with screen data' (Test-Json -Json $txt -SchemaFile (Join-Path -Path $script:Family -ChildPath 'schemas/ct21d.i1-p2.v1.json'))
    Assert-Equal 'the analyzer wrote nothing next to the inputs' $snapBefore (Get-Snapshot $script:Temp)

    # differences are characterization results
    $diff = @(
        @('P2-E03', 'ERROR_TEXT_VERBATIM', '; error: too many arguments'),
        @('P2-E05', 'OK', 'P2-E05 raw-K.txt-V'),
        @('P2-E07', 'OK', 'P2-E07 VALUE: 3'),
        @('P2-E08', 'OK', 'P2-E08 VALUE: 195'),
        @('P2-E09', 'OK', 'P2-E09 VALUE: 4'),
        @('P2-E10', 'OK', 'P2-E10 VALUE: nil'),
        @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/Windows/System32/kernel32.dll"'),
        @('P2-E18', 'OK', 'P2-E18 VALUE: nil'),
        @('P2-E25', 'OK', 'P2-E25 type=STR len=57 startsExact=nil startsAnyCase=T hasWowAnyCase=nil')
    )
    $j = ConvertFrom-Record (Invoke-WithScreen $r $diff @($script:Shot))
    $ch = $j.characterization
    Assert-Equal 'arity control OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.userFunctionArityControl.status
    Assert-Equal 'E03 arity rejection is UNSUPPORTED/ARITY' 'UNSUPPORTED/ARITY' ((@($j.expressionResults | Where-Object { $_.id -ceq 'P2-E03' }))[0].result + '/' + (@($j.expressionResults | Where-Object { $_.id -ceq 'P2-E03' }))[0].category)
    Assert-Equal 'strlen 4 -> OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.strlenSemantics.status
    Assert-Equal 'strlen 4 label is byte-count-like' 'UTF8_BYTE_COUNT_LIKE' $ch.strlenSemantics.label
    Assert-True 'strlen detail does not assume bytes' ($ch.strlenSemantics.detail.Contains('byte-count-like') -and $ch.strlenSemantics.detail.Contains('not an assumption'))
    Assert-Equal 'ascii 195 label' 'UTF8_LEAD_BYTE_LIKE' $ch.escapeInterpretation.label
    Assert-Equal 'ascii 195 OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.escapeInterpretation.status
    Assert-Equal 'productKeyShape prefix case OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.productKeyShape.status
    Assert-Equal 'findfile after-write disagreement makes findfilePresent OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.findfilePresent.status
    Assert-True 'a difference never sets stopConditionRecorded' ($j.stopConditionRecorded -eq $false)
    Assert-Equal 'a1 file present and no E16 screen: the file decides (SUPPORTED)' 'SUPPORTED' $ch.openTwoArg.formVerdict

    # the strlen and escape readings are independent
    foreach ($case in @(@('9', 'ESCAPE_UNINTERPRETED'), @('3', 'CHARACTER_INTERPRETATION_CANDIDATE'), @('17', 'OTHER_VALUE'))) {
        $r2 = New-RunDir
        $j = ConvertFrom-Record (Invoke-WithScreen $r2 @(@('P2-E07', 'OK', 'P2-E07 VALUE: 3'), @('P2-E09', 'OK', ('P2-E09 VALUE: ' + $case[0])), @('P2-E08', 'OK', 'P2-E08 VALUE: 92')) @($script:Shot))
        Assert-Equal ('strlen ' + $case[0] + ' label') $case[1] $j.characterization.strlenSemantics.label
        Assert-Equal ('ascii 92 stays recorded apart (strlen ' + $case[0] + ')') 'BACKSLASH_UNINTERPRETED' $j.characterization.escapeInterpretation.label
    }
    $r2 = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r2 @(@('P2-E07', 'OK', 'P2-E07 VALUE: 4'), @('P2-E09', 'OK', 'P2-E09 VALUE: 4'), @('P2-E08', 'OK', 'P2-E08 VALUE: 7')) @($script:Shot))
    Assert-Equal 'a failed strlen control is OTHER_VALUE' 'OTHER_VALUE' $j.characterization.strlenSemantics.label
    Assert-Equal 'another ascii code is OTHER_CODE' 'OTHER_CODE' $j.characterization.escapeInterpretation.label
    $j = ConvertFrom-Record (Invoke-WithScreen $r2 @(, @('P2-E08', 'ERROR_TEXT_VERBATIM', 'P2-E08 ERROR: bad argument type: stringp nil')) @($script:Shot))
    Assert-Equal 'an ascii argument-value error is OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.escapeInterpretation.status
    Assert-Equal 'an ascii error is HOST_ERROR/ARGUMENT_VALUE' 'HOST_ERROR/ARGUMENT_VALUE' ($j.expressionResults[0].result + '/' + $j.expressionResults[0].category)

    # open forms: argument-value error versus arity, helper error before the open line, nil, descriptor without a file
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"'), @('P2-E16', 'ERROR_TEXT_VERBATIM', 'P2-E16 open ERROR: bad argument value: "w"'), @('P2-E17', 'ERROR_TEXT_VERBATIM', 'P2-E17 open ERROR: too many arguments')) @($script:Shot))
    Assert-Equal 'A1 argument-value error -> form HOST_ERROR' 'HOST_ERROR' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'A1 argument-value error -> OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $j.characterization.openTwoArg.status
    Assert-True 'A1 argument-value error text verbatim' $j.characterization.openTwoArg.detail.Contains('bad argument value')
    Assert-Equal 'A2 arity -> form UNSUPPORTED' 'UNSUPPORTED' $j.characterization.openThreeArgUtf8.formVerdict
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"'), @('P2-E16', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-AFTER')) @($script:Shot))
    Assert-Equal 'helper error before the open line -> form INCOMPLETE' 'INCOMPLETE' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'helper error before the open line -> result INCOMPLETE' 'INCOMPLETE' $j.characterization.openTwoArg.result
    Assert-Equal 'helper error before the open line maps to NOT_OBSERVED' 'NOT_OBSERVED' $j.characterization.openTwoArg.status
    Assert-True 'the detail says it is not evidence about open' $j.characterization.openTwoArg.detail.Contains('not evidence about open')
    Assert-True 'the helper-error expression is INCOMPLETE/HELPER' (@($j.expressionResults | Where-Object { $_.id -ceq 'P2-E16' })[0].result -ceq 'INCOMPLETE' -and @($j.expressionResults | Where-Object { $_.id -ceq 'P2-E16' })[0].category -ceq 'HELPER')
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"'), @('P2-E16', 'OK', 'P2-E16 open VALUE: nil'), @('P2-E17', 'OK', 'P2-E17 open VALUE: FILE-DESCRIPTOR')) @($script:Shot))
    Assert-Equal 'open nil and no file -> UNKNOWN' 'UNKNOWN' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'descriptor without any file -> UNKNOWN' 'UNKNOWN' $j.characterization.openThreeArgUtf8.formVerdict
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"'), @('P2-E17', 'ERROR_TEXT_VERBATIM', 'P2-E17 open ERROR: too many arguments')) @($script:Shot))
    Assert-Equal 'a file that contradicts a screen rejection -> UNKNOWN (fail closed)' 'UNKNOWN' $j.characterization.openThreeArgUtf8.formVerdict

    # definition and helper errors are protocol results, not v1 compatibility mismatches
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E02', 'ERROR_TEXT_VERBATIM', '; error: malformed list on input'), @('P2-E03', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-CTL'), @('P2-E05', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-CTL2'), @('P2-E06', 'ERROR_TEXT_VERBATIM', '; error: malformed list on input'), @('P2-E10', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-SHOW')) @($script:Shot))
    $er = @{}; foreach ($x in $j.expressionResults) { $er[$x.id] = $x }
    Assert-Equal 'a defun that printed an error is HOST_ERROR/DEFINITION' 'HOST_ERROR/DEFINITION' ($er['P2-E02'].result + '/' + $er['P2-E02'].category)
    Assert-Equal 'a call of a missing helper is HOST_ERROR/HELPER' 'HOST_ERROR/HELPER' ($er['P2-E03'].result + '/' + $er['P2-E03'].category)
    Assert-Equal 'the arity control becomes NOT_OBSERVED, not OBSERVED_DIFFERS' 'NOT_OBSERVED' $j.characterization.userFunctionArityControl.status
    Assert-True 'the arity detail says it is not a v1 compatibility mismatch' $j.characterization.userFunctionArityControl.detail.Contains('not a v1 compatibility mismatch')
    Assert-Equal 'findfile through a missing display helper is NOT_OBSERVED' 'NOT_OBSERVED' $j.characterization.findfileAbsent.status
    Assert-Equal 'a gate with the E11 entry missing stays UNKNOWN' 'UNKNOWN' $j.openGate

    # the findfile gate
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: "D:/I52-CT21D-HOST/evidence/x/p2-absent.txt"'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/Windows/System32/kernel32.dll"')) @($script:Shot))
    Assert-Equal 'a missing-file control that printed a string -> STOP_BEFORE_OPEN' 'STOP_BEFORE_OPEN' $j.openGate
    Assert-Equal 'STOP_BEFORE_OPEN: A1 form INCOMPLETE' 'INCOMPLETE' $j.characterization.openTwoArg.formVerdict
    Assert-Equal 'STOP_BEFORE_OPEN: A2 NOT_OBSERVED' 'NOT_OBSERVED' $j.characterization.openThreeArgUtf8.status
    Assert-True 'STOP_BEFORE_OPEN: E12..E25 are INCOMPLETE/GUARD' (@($j.expressionResults | Where-Object { $_.id -cmatch '^P2-E(1[2-9]|2[0-5])$' -and $_.result -ceq 'INCOMPLETE' -and $_.category -ceq 'GUARD' }).Count -eq 14)
    Assert-True 'STOP_BEFORE_OPEN without any later entry is not a deviation' ($j.stopConditionRecorded -eq $false)
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: nil')) @($script:Shot))
    Assert-Equal 'an existing-file control that printed nil -> STOP_BEFORE_OPEN' 'STOP_BEFORE_OPEN' $j.openGate
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'ERROR_TEXT_VERBATIM', 'P2-E10 ERROR: bad argument type'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"'), @('P2-E16', 'OK', 'P2-E16 open VALUE: FILE-DESCRIPTOR')) @($script:Shot))
    Assert-Equal 'a control error -> STOP_BEFORE_OPEN' 'STOP_BEFORE_OPEN' $j.openGate
    Assert-True 'an open entry after a failed gate is an operator deviation and a stop' ($j.stopConditionRecorded -eq $true -and @($j.expressionResults | Where-Object { $_.id -ceq 'P2-E16' })[0].result -ceq 'OPERATOR_DEVIATION')
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a1.txt' (Get-FileBytes "`n" $script:Utf8E)
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: nil')) @($script:Shot))
    Assert-True 'a file that exists after a failed gate is a stop' ($j.stopConditionRecorded -eq $true)
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"')) @())
    Assert-Equal 'no screenshot custody -> the gate is UNKNOWN' 'UNKNOWN' $j.openGate

    # E01
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E01', 'OK', '"D:/I52-CT21D-HOST/evidence/HGP-H10-20261002T100000Z-99/"')) @($script:Shot))
    Assert-True 'an E01 echo that differs from the authorized root is an operator deviation and a stop' ($j.stopConditionRecorded -eq $true -and $j.expressionResults[0].result -ceq 'OPERATOR_DEVIATION')

    # escape not interpreted, E09 missing, open nil, shape errors, no custody
    $r = New-RunDir
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E07', 'OK', 'P2-E07 VALUE: 3'), @('P2-E09', 'OK', 'P2-E09 VALUE: 9'), @('P2-E10', 'OK', 'P2-E10 VALUE: nil'), @('P2-E11', 'OK', 'P2-E11 VALUE: "C:/x"'), @('P2-E16', 'OK', 'P2-E16 open VALUE: nil'), @('P2-E25', 'ERROR_TEXT_VERBATIM', 'P2-E25 ERROR: no function definition: VLAX-PRODUCT-KEY')) @($script:Shot))
    $ch = $j.characterization
    Assert-Equal 'E09 = 9 -> OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.strlenSemantics.status
    Assert-Equal 'E09 = 9 label' 'ESCAPE_UNINTERPRETED' $ch.strlenSemantics.label
    Assert-True 'E09 = 9 detail says uninterpreted' $ch.strlenSemantics.detail.Contains('uninterpreted')
    Assert-Equal 'open returned nil and no file -> UNKNOWN' 'UNKNOWN' $ch.openTwoArg.status
    Assert-Equal 'vlax-product-key not defined (a built-in) -> OBSERVED_DIFFERS' 'OBSERVED_DIFFERS' $ch.productKeyShape.status
    Assert-Equal 'no E03/E05 -> arity UNKNOWN' 'UNKNOWN' $ch.userFunctionArityControl.status
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E07', 'OK', 'P2-E07 VALUE: 3')) @($script:Shot))
    Assert-Equal 'E09 missing -> strlen UNKNOWN' 'UNKNOWN' $j.characterization.strlenSemantics.status
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E25', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-SHAPE')) @($script:Shot))
    Assert-Equal 'a missing shape helper is NOT_OBSERVED' 'NOT_OBSERVED' $j.characterization.productKeyShape.status

    # no custody: screen-derived items are UNKNOWN, file-derived items survive
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
    $j = ConvertFrom-Record (Invoke-WithScreen $r $good @())
    Assert-True 'no screenshot hash -> custody false' ($j.screenCustodyPresent -eq $false)
    Assert-Equal 'no custody: strlen UNKNOWN' 'UNKNOWN' $j.characterization.strlenSemantics.status
    Assert-Equal 'no custody: escape UNKNOWN' 'UNKNOWN' $j.characterization.escapeInterpretation.status
    Assert-Equal 'no custody: arity UNKNOWN' 'UNKNOWN' $j.characterization.userFunctionArityControl.status
    Assert-Equal 'no custody: productKeyShape UNKNOWN' 'UNKNOWN' $j.characterization.productKeyShape.status
    Assert-Equal 'no custody: the file still shows the form SUPPORTED' 'SUPPORTED' $j.characterization.openThreeArgUtf8.formVerdict
    Assert-Equal 'no custody: the file still shows the terminator' 'OBSERVED' $j.characterization.writeLineTerminator.status
    Assert-True 'no custody: every expression result is UNKNOWN' (@($j.expressionResults | Where-Object { $_.result -cne 'UNKNOWN' }).Count -eq 0)

    # a stop condition makes everything UNKNOWN
    foreach ($cls in 'DEVIATION', 'REFUSED') {
        $r = New-RunDir
        Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
        $txtEntry = if ($cls -ceq 'REFUSED') { 'P2-E16 REFUSED, exists: p2-a1.txt' } else { 'an unexpected PASTECLIP prompt appeared' }
        $j = ConvertFrom-Record (Invoke-WithScreen $r @(@('P2-E09', 'OK', 'P2-E09 VALUE: 3'), @('P2-E16', $cls, $txtEntry)) @($script:Shot))
        Assert-True ($cls + ': stopConditionRecorded true') ($j.stopConditionRecorded -eq $true)
        $allUnknown = (@($j.characterization.Values | Where-Object { $_.status -cne 'UNKNOWN' }).Count -eq 0)
        Assert-True ($cls + ': every item UNKNOWN even with a good file present') $allUnknown
        $x = @($j.expressionResults | Where-Object { $_.id -ceq 'P2-E16' })[0]
        $expect = if ($cls -ceq 'REFUSED') { 'INCOMPLETE' } else { 'OPERATOR_DEVIATION' }
        Assert-Equal ($cls + ': the expression result') $expect $x.result
    }

    # =====================================================================================================================================
    Start-Section 'checkpoint screenshots'
    $r = New-RunDir
    $five = @(@('P2-E05', $script:Shot), @('P2-E09', $script:Shot), @('P2-E17', $script:Shot2), @('P2-E19', $script:Shot2), @('P2-E25', $script:Shot2))
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E07', 'OK', 'P2-E07 VALUE: 3')) @($script:Shot, $script:Shot2) $five)
    Assert-Equal 'five checkpoints recorded' 5 $j.checkpoints.Count
    Assert-Equal 'no checkpoint missing' 0 $j.checkpointsMissing.Count
    $badCheckpoints = [ordered]@{
        'checkpoint id out of the closed set' = '{"screenshotSha256":["' + $script:Shot + '"],"checkpoints":[{"after":"P2-E06","sha256":"' + $script:Shot + '"}]}'
        'checkpoint hash not among the screenshot hashes' = '{"screenshotSha256":["' + $script:Shot + '"],"checkpoints":[{"after":"P2-E05","sha256":"' + $script:Shot2 + '"}]}'
        'duplicate checkpoint' = '{"screenshotSha256":["' + $script:Shot + '"],"checkpoints":[{"after":"P2-E05","sha256":"' + $script:Shot + '"},{"after":"P2-E05","sha256":"' + $script:Shot + '"}]}'
        'checkpoint hash upper case' = '{"screenshotSha256":["' + $script:Shot + '"],"checkpoints":[{"after":"P2-E05","sha256":"' + ('AB' * 32) + '"}]}'
        'unknown checkpoint member' = '{"screenshotSha256":["' + $script:Shot + '"],"checkpoints":[{"after":"P2-E05","sha256":"' + $script:Shot + '","x":1}]}'
        'checkpoint missing member' = '{"screenshotSha256":["' + $script:Shot + '"],"checkpoints":[{"after":"P2-E05"}]}'
        'checkpoints not an array' = '{"checkpoints":{}}'
    }
    foreach ($k in $badCheckpoints.Keys) {
        $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen (Write-ScreenFile $badCheckpoints[$k])
        Assert-True ('refused: ' + $k) ($res.Exit -eq 2 -and $res.Bytes.Length -eq 0 -and $res.Err.StartsWith('REFUSED:')) ('exit=' + $res.Exit + ' err=' + $res.Err)
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
        'id out of range'               = '{"screen":[{"id":"P2-E26","classification":"OK","text":"x"}]}'
        'old id range edge P2-E00'      = '{"screen":[{"id":"P2-E00","classification":"OK","text":"x"}]}'
        'id of another family'          = '{"screen":[{"id":"S1-E01","classification":"OK","text":"x"}]}'
        'bad classification'            = '{"screen":[{"id":"P2-E09","classification":"PASS","text":"x"}]}'
        'OK contradicts the text'       = '{"screen":[{"id":"P2-E09","classification":"OK","text":"P2-E09 ERROR: boom"}]}'
        'OK with REFUSED text'          = '{"screen":[{"id":"P2-E16","classification":"OK","text":"P2-E16 REFUSED, exists: p2-a1.txt"}]}'
        'ERROR class without error text'= '{"screen":[{"id":"P2-E09","classification":"ERROR_TEXT_VERBATIM","text":"P2-E09 VALUE: 3"}]}'
        'REFUSED class without text'    = '{"screen":[{"id":"P2-E16","classification":"REFUSED","text":"nothing"}]}'
        'text is not a string'          = '{"screen":[{"id":"P2-E09","classification":"OK","text":5}]}'
        'screen is not an array'        = '{"screen":{}}'
        'screenshot hash upper case'    = '{"screenshotSha256":["' + ('AB' * 32) + '"]}'
        'screenshot hash too short'     = '{"screenshotSha256":["abcd"]}'
        'duplicate screenshot hash'     = '{"screenshotSha256":["' + $script:Shot + '","' + $script:Shot + '"]}'
        'trailing comma'                = '{"screen":[],}'
        'not JSON'                      = 'screen'
        'root is an array'              = '[]'
        'text too long'                 = '{"screen":[{"id":"P2-E09","classification":"OK","text":"' + ('a' * 4001) + '"}]}'
        'stringified file descriptor'   = '{"screen":[{"id":"P2-E16","classification":"OK","text":"P2-E16 open VALUE: #<file \"D:/x/p2-a1.txt\">"}]}'
        'old stringified file object'   = '{"screen":[{"id":"P2-E16","classification":"DEVIATION","text":"#<file x>"}]}'
        'outside the vocabulary: free text' = '{"screen":[{"id":"P2-E07","classification":"OK","text":"hello"}]}'
        'outside the vocabulary: a prompt line' = '{"screen":[{"id":"P2-E07","classification":"OK","text":"Command: P2-E07 VALUE: 3"}]}'
        'outside the vocabulary: a non-path string' = '{"screen":[{"id":"P2-E10","classification":"OK","text":"P2-E10 VALUE: \"Rack1\""}]}'
        'outside the vocabulary: another id inside the entry' = '{"screen":[{"id":"P2-E07","classification":"OK","text":"P2-E08 VALUE: 3"}]}'
        'outside the vocabulary: trailing text' = '{"screen":[{"id":"P2-E07","classification":"OK","text":"P2-E07 VALUE: 3 extra"}]}'
        'outside the vocabulary: a symbol echo of another family' = '{"screen":[{"id":"P2-E06","classification":"OK","text":"CT21D-PUT"}]}'
        'old E01-style echo with another root' = '{"screen":[{"id":"P2-E01","classification":"OK","text":"\"C:/other/\""}]}'
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

    # non-ASCII verbatim text is escaped, never emitted raw (allowed inside a returned P2 string)
    $sf = Write-ScreenFile ('{"screen":[{"id":"P2-E16","classification":"OK","text":"P2-E16 write-line VALUE: \"P2-ACCENT-' + [char]0xE9 + '-END\""}],"screenshotSha256":["' + $script:Shot + '"]}')
    $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen $sf
    Assert-Equal 'non-ASCII text inside the vocabulary accepted' 0 $res.Exit
    $o = ConvertFrom-Out $res
    Assert-True 'non-ASCII text is emitted as a six-character escape' ($o.Contains([string][char]92 + 'u00e9') -and ($o.Substring(0, $o.Length - 1) -cmatch '^[\x20-\x7e]+$'))

    # =====================================================================================================================================
    Start-Section 'governed-attribute isolation: adversarial transcript and attribute leakage'
    $pkv = 'Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409'
    $pkBytes = [System.Text.Encoding]::ASCII.GetBytes($pkv)
    $hexPlain = (($pkBytes | ForEach-Object { $_.ToString('x2') }) -join '')
    $hexUpper = $hexPlain.ToUpperInvariant()
    $hexSpaced = (($pkBytes | ForEach-Object { $_.ToString('x2') }) -join ' ')
    $hexColon = (($pkBytes | ForEach-Object { $_.ToString('x2') }) -join ':')
    $hexX = (($pkBytes | ForEach-Object { '\x' + $_.ToString('x2') }) -join '')
    $pctAll = (($pkBytes | ForEach-Object { '%' + $_.ToString('X2') }) -join '')
    $buildHex = (([System.Text.Encoding]::ASCII.GetBytes('10.0.26200') | ForEach-Object { $_.ToString('x2') }) -join '')
    $guid = '3f2504e0-4f89-11d3-9a0c-0305e82c3301'
    # fullwidth look-alikes (U+FF01..U+FF5E) of an ASCII string; written with char codes so that this file stays ASCII
    function ConvertTo-Fullwidth { param([string]$S) return (-join ($S.ToCharArray() | ForEach-Object { if ([int]$_ -ge 33 -and [int]$_ -le 126) { [char]([int]$_ + 0xFEE0) } else { $_ } })) }
    $leaks = [ordered]@{
        # Unicode look-alikes: folded (NFKC, dash-like characters, zero-width) before the patterns are evaluated
        'unicode, fullwidth product key'      = ConvertTo-Fullwidth $pkv
        'unicode, fullwidth key prefix'       = ConvertTo-Fullwidth 'Software\Autodesk\AutoCAD'
        'unicode, fullwidth GUID'             = ConvertTo-Fullwidth $guid
        'unicode, GUID with U+2010 hyphens'   = $guid.Replace('-', [string][char]0x2010)
        'unicode, GUID with U+2011 hyphens'   = $guid.Replace('-', [string][char]0x2011)
        'unicode, GUID with U+2212 minus'     = $guid.Replace('-', [string][char]0x2212)
        'unicode, fullwidth SECURELOAD'       = ConvertTo-Fullwidth 'SECURELOAD'
        'unicode, zero-width inside a name'   = ('SECURE' + [char]0x200B + 'LOAD')
        'unicode, combining mark inside a name' = ('SECURE' + [char]0x0301 + 'LOAD')
        'unicode, fullwidth build number'     = ConvertTo-Fullwidth '10.0.26200'
        'unicode, U+037E separated paths'     = ('C:\a' + [char]0x037E + 'C:\b')
        'unicode, fullwidth semicolon paths'  = ('C:\a' + [char]0xFF1B + 'C:\b')        # product-key-like strings: case, escaping, splitting, hex
        'product key, exact'                  = ('P2-E25 ' + $pkv)
        'product key, upper case'             = $pkv.ToUpperInvariant()
        'product key, lower case'             = $pkv.ToLowerInvariant()
        'product key, forward slashes'        = 'Software/Autodesk/AutoCAD/R25.0/ACAD-8101:409'
        'product key, doubled backslashes'    = 'Software\\Autodesk\\AutoCAD\\R25.0\\ACAD-8101:409'
        'product key, inside a quoted VALUE'  = ('P2-E25 VALUE: "' + $pkv.Replace('\', '\\') + '"')
        'product key, split with spaces'      = 'Soft ware \ Auto desk \ Auto CAD \ R25.0 \ ACAD - 8101 : 409'
        'product key, split across lines'     = "Soft`nware\Auto`ndesk\AutoCAD`nR25.0\ACAD-8101:409"
        'product key, split in two entries lines' = "P2-E25 Soft`nP2-E25 ware\Autodesk"
        'product key, ACAD suffix only'       = 'ACAD-8101:409'
        'product key, ACAD suffix lower case' = 'acad-8101:409'
        'product key, ACAD suffix, other digits' = 'ACAD-1234:AB0'
        'product key, \x escapes'             = ('\x53' + 'oftware\x5cAutodesk\x5cAutoCAD')
        'product key, every char as \x'       = $hexX
        'product key, percent escapes'        = $pctAll
        'product key, partial percent escapes' = '%53oftware%5CAutodesk'
        'product key, \u escapes'             = '\u0053oftware\u005cAutodesk'
        'product key, AutoLISP \U+ escapes'   = '\U+0053oftware\U+005CAutodesk'
        'product key, HTML decimal entities'  = '&#83;oftware&#92;Autodesk'
        'product key, HTML hex entities'      = '&#x53;oftware&#x5C;Autodesk'
        'product key, octal escapes'          = '\123oftware\134Autodesk\134AutoCAD'
        'product key, 0x pairs'               = '0x53oftware0x5cAutodesk'
        'product key, hex string'             = $hexPlain
        'product key, hex string upper case'  = $hexUpper
        'product key, hex string spaced'      = $hexSpaced
        'product key, hex string colon-separated' = $hexColon
        'product key, hex of the first bytes only' = '536f6674776172655c4175746f6465736b'
        'product key, percent escapes, reversed order' = '%5CSoftware%5CAutodesk'
        'product key, Autodesk AutoCAD pair'  = 'Autodesk\AutoCAD'
        'product key, tab separated'          = "Software`tAutodesk`tAutoCAD"
        'product key, inside a host error'    = '; error: bad argument Software\Autodesk\AutoCAD'
        'product key, with the WOW6432Node'   = 'Software\WOW6432Node\Autodesk\AutoCAD\R25.0\ACAD-8101:409'
        'product key, another release'        = 'Software\Autodesk\AutoCAD\R25.1\ACAD-8102:409'
        'product key, with a registry hive'   = 'HKEY_LOCAL_MACHINE\Software\Autodesk'
        'product key, backslash only escaped' = 'Software\x5cAutodesk'
        'product key, hex digits mixed case'  = '536F6674776172655C4175746F6465736B'
        # CPROFILE-like profile names
        'profile, unnamed profile'            = '<<Unnamed Profile>>'
        'profile, a named profile'            = 'RackCad-Profile'
        'profile, split letters'              = 'P R O F I L E'
        'profile, in a VALUE line'            = 'P2-E10 VALUE: "D:/Profile1"'
        'profile, name of the variable'       = 'CPROFILE'
        'profile, name of the variable lower' = 'cprofile'
        'profile, name split'                 = 'C P R O F I L E'
        'profile, name hex-escaped'           = '\x43\x50\x52\x4f\x46\x49\x4c\x45'
        'profile, name hex string'            = (([System.Text.Encoding]::ASCII.GetBytes('CPROFILE-X') | ForEach-Object { $_.ToString('x2') }) -join '')
        # TRUSTEDPATHS-like semicolon path chains
        'paths, two drive paths'              = 'C:\Program Files\Foo;C:\Program Files\Bar'
        'paths, nineteen-style chain'         = ((1..19 | ForEach-Object { 'C:\Trusted\P' + $_ }) -join ';')
        'paths, forward slashes'              = 'C:/a;D:/b'
        'paths, UNC style'                    = '\\server\share\x;\\server\share\y'
        'paths, single trailing semicolon'    = 'C:\Program Files\Foo;'
        'paths, percent-encoded semicolon'    = 'C:\a%3BD:\b'
        'paths, \x3b semicolon'               = 'C:\a\x3bD:\b'
        'paths, entity semicolon'             = 'C:\a&#59;D:\b'
        'paths, in a VALUE line'              = 'P2-E11 VALUE: "C:/a;C:/b"'
        'paths, spaced semicolon'             = 'C:\a ; C:\b'
        'paths, trailing backslashes'         = 'C:\a\;C:\b\'
        'paths, name of the variable'         = 'TRUSTEDPATHS'
        'paths, name lower case'              = 'trustedpaths'
        'paths, name split'                   = 'T-R-U-S-T-E-D P-A-T-H-S'
        'paths, name with underscore'         = 'trusted_paths'
        'paths, name hex string'              = (([System.Text.Encoding]::ASCII.GetBytes('TRUSTEDPATHS') | ForEach-Object { $_.ToString('x2') }) -join '')
        # MachineGuid GUID patterns
        'guid, plain'                         = $guid
        'guid, upper case'                    = $guid.ToUpperInvariant()
        'guid, braces'                        = ('{' + $guid.ToUpperInvariant() + '}')
        'guid, without hyphens'               = $guid.Replace('-', '')
        'guid, spaces instead of hyphens'     = $guid.Replace('-', ' ')
        'guid, split by newlines'             = $guid.Replace('-', "`n")
        'guid, percent-escaped first digit'   = ('%33' + $guid.Substring(1))
        'guid, \x-escaped first digit'        = ('\x33' + $guid.Substring(1))
        'guid, in a VALUE line'               = ('P2-E11 VALUE: "' + $guid + '"')
        'guid, hex string of its text'        = (([System.Text.Encoding]::ASCII.GetBytes($guid) | ForEach-Object { $_.ToString('x2') }) -join '')
        'guid, \x2d hyphens'                  = '3f2504e0\x2d4f89\x2d11d3\x2d9a0c\x2d0305e82c3301'
        'guid, percent hyphens'               = '3f2504e0%2d4f89%2d11d3%2d9a0c%2d0305e82c3301'
        'guid, name of the attribute'         = 'MachineGuid'
        'guid, name lower case'               = 'machineguid'
        'guid, name split'                    = 'Machine Guid'
        # OsVersionBuild
        'build, dotted'                       = '10.0.26200'
        'build, with a revision'              = '10.0.26200.1234'
        'build, older build'                  = '10.0.19045'
        'build, in a VALUE line'              = 'P2-E07 VALUE: 10.0.26200'
        'build, spaced dots'                  = '10 . 0 . 26200'
        'build, commas'                       = '10,0,26200'
        'build, \x2e dots'                    = '10\x2e0\x2e26200'
        'build, percent dots'                 = '10%2e0%2e26200'
        'build, entity dots'                  = '10&#46;0&#46;26200'
        'build, hex string'                   = $buildHex
        'build, hex string spaced'            = ((($buildHex -split '(..)' | Where-Object { $_ }) -join ' '))
        'build, another build'                = '10.0.22631'
        'build, hex entity dots'              = '10&#x2e;0&#x2e;26200'
        'build, with trailing words'          = 'Windows 10.0.26200 x64'
        'build, name of the attribute'        = 'OsVersionBuild'
        'build, name lower case'              = 'osversionbuild'
        # other governed names
        'name, SECURELOAD'                    = 'SECURELOAD'
        'name, SECURELOAD in a VALUE line'    = 'P2-E07 VALUE: "SECURELOAD"'
        'name, secureload lower case'         = 'secureload'
        'name, secure load split'             = 'SECURE LOAD'
        'name, SECURELOAD hex-escaped'        = '\x53\x45\x43\x55\x52\x45\x4c\x4f\x41\x44'
    }
    $leakRun = New-RunDir
    $caught = 0
    $expectedCaught = 0
    $leakIndex = 0
    foreach ($k in $leaks.Keys) {
        $t = [string]$leaks[$k]
        $leakIndex++
        # every leak is tried as a normal result entry; every third one is also tried inside an operator-deviation note
        if (($leakIndex % 3) -eq 0) { $variants = @(@('P2-E25', 'OK'), @('P2-E16', 'DEVIATION')) } else { $variants = @(, @('P2-E25', 'OK')) }
        $expectedCaught += $variants.Count
        foreach ($variant in $variants) {
            $sf = Write-ScreenFile (Get-ScreenJson @(,@($variant[0], $variant[1], $t)) @($script:Shot))
            $res = Invoke-Analyzer -Dir $leakRun.Dir -Run $leakRun.Run -Screen $sf
            $isRefused = ($res.Exit -eq 2 -and $res.Bytes.Length -eq 0 -and $res.Err.Contains('looks like a governed value or names a governed attribute'))
            Assert-True ('leak refused (' + $variant[1] + '): ' + $k) $isRefused ('exit=' + $res.Exit + ' stdout=' + $res.Bytes.Length + ' err=' + $res.Err)
            if ($isRefused) { $caught++ }
        }
    }
    Assert-Equal 'every leak variant was caught' $expectedCaught $caught
    Assert-True 'the adversarial list is large enough to matter' ($leaks.Count -ge 90)
    # the refusal reason never echoes the text
    $sf = Write-ScreenFile (Get-ScreenJson @(,@('P2-E25', 'OK', ('P2-E25 ' + $pkv))) @($script:Shot))
    $res = Invoke-Analyzer -Dir $leakRun.Dir -Run $leakRun.Run -Screen $sf
    Assert-True 'the refusal does not echo the leaked value' (-not $res.Err.Contains('Autodesk') -and -not $res.Err.Contains('8101'))
    # a leak in a checkpoint or hash member is not possible (closed members), and a leak in an unknown member is refused as unknown
    $res = Invoke-Analyzer -Dir $leakRun.Dir -Run $leakRun.Run -Screen (Write-ScreenFile ('{"note":"' + $pkv.Replace('\', '\\') + '"}'))
    Assert-True 'a leak in an unknown top-level member is refused' ($res.Exit -eq 2 -and $res.Bytes.Length -eq 0)
    # the leak never reaches the record through the file names either: extra names are listed by name only and never opened
    Write-Bytes $leakRun.Dir 'raw-TRUSTEDPATHS.txt' ([System.Text.Encoding]::ASCII.GetBytes('C:\a;C:\b'))
    $res = Invoke-Analyzer -Dir $leakRun.Dir -Run $leakRun.Run
    Assert-True 'a file of the evidence child is never read, so its content cannot reach the record' ($res.Exit -eq 0 -and -not (ConvertFrom-Out $res).Contains('C:\\a') -and -not (ConvertFrom-Out $res).Contains('C:/a'))
    # near misses that are legitimate P2 output are NOT refused (no vacuous guard)
    $legit = @(
        @('P2-E01', 'OK', '"D:/I52-CT21D-HOST/evidence/HGP-H10-20261002T100000Z-01/"'),
        @('P2-E10', 'OK', 'P2-E10 VALUE: nil'),
        @('P2-E11', 'OK', 'P2-E11 VALUE: "C:\\Windows\\System32\\kernel32.dll"'),
        @('P2-E18', 'OK', 'P2-E18 VALUE: "D:\\I52-CT21D-HOST\\evidence\\HGP-H10-20261002T100000Z-01\\p2-a1.txt"'),
        @('P2-E07', 'OK', 'P2-E07 VALUE: 3'),
        @('P2-E09', 'OK', 'P2-E09 VALUE: 9'),
        @('P2-E25', 'OK', 'P2-E25 type=STR len=57 startsExact=T startsAnyCase=T hasWowAnyCase=nil'),
        @('P2-E20', 'OK', 'P2-E20 VALUE: nil'),
        @('P2-E17', 'ERROR_TEXT_VERBATIM', 'P2-E17 open ERROR: too many arguments'),
        @('P2-E16', 'ERROR_TEXT_VERBATIM', '; error: no function definition: CT21D-P2-AFTER')
    )
    foreach ($e in $legit) {
        $res = Invoke-Analyzer -Dir $leakRun.Dir -Run $leakRun.Run -Screen (Write-ScreenFile (Get-ScreenJson @(,$e) @($script:Shot)))
        Assert-True ('legitimate P2 output is accepted: ' + $e[0] + ' ' + $e[2].Substring(0, [Math]::Min(30, $e[2].Length))) ($res.Exit -eq 0) ('exit=' + $res.Exit + ' err=' + $res.Err)
    }

    # a lone surrogate (valid JSON escape, not normalizable) fails closed
    $res = Invoke-Analyzer -Dir $leakRun.Dir -Run $leakRun.Run -Screen (Write-ScreenFile ('{"screen":[{"id":"P2-E25","classification":"OK","text":"abc\ud800def"}],"screenshotSha256":["' + $script:Shot + '"]}'))
    Assert-True 'a lone surrogate in the text is refused (fail closed)' ($res.Exit -eq 2 -and $res.Bytes.Length -eq 0)

    # =====================================================================================================================================
    Start-Section 'P2-E20 accepts any VALUE inside the closed vocabulary; P2-E01 error is the deviation'
    $r = New-RunDir
    foreach ($v in 'nil', 'T', 'VL-LOAD-COM', 'vl_load_com', 'LOADED*', 'A') {
        $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E20', 'OK', ('P2-E20 VALUE: ' + $v))) @($script:Shot))
        Assert-True ('E20 VALUE: ' + $v + ' is accepted and SUPPORTED') ($j -and $j.expressionResults[0].id -ceq 'P2-E20' -and $j.expressionResults[0].result -ceq 'SUPPORTED')
    }
    $negE20 = [ordered]@{
        'symbol that is a 32-hex GUID'        = 'P2-E20 VALUE: abcdefabcdefabcdefabcdefabcdefab'
        'symbol with a digit run'             = 'P2-E20 VALUE: ACAD8101'
        'symbol that names an attribute'      = 'P2-E20 VALUE: SECURELOAD'
        'symbol with profile'                 = 'P2-E20 VALUE: VANILLA-PROFILE'
        'overlong symbol'                     = ('P2-E20 VALUE: A' + ('B' * 41))
        'symbol starting with a star'         = 'P2-E20 VALUE: *X'
        'symbol starting with a digit'        = 'P2-E20 VALUE: 7ABC'
        'symbol with a path separator'        = 'P2-E20 VALUE: A\B'
        'symbol with a colon'                 = 'P2-E20 VALUE: A:B'
        'two tokens'                          = 'P2-E20 VALUE: T NIL'
        'symbol on another expression'        = 'P2-E07 VALUE: T'
    }
    foreach ($k in $negE20.Keys) {
        $t = [string]$negE20[$k]
        $id = $t.Substring(0, 6)
        $res = Invoke-Analyzer -Dir $r.Dir -Run $r.Run -Screen (Write-ScreenFile (Get-ScreenJson @(,@($id, 'OK', $t)) @($script:Shot)))
        Assert-True ('E20 negative: ' + $k) ($res.Exit -eq 2 -and $res.Bytes.Length -eq 0) ('exit=' + $res.Exit + ' err=' + $res.Err)
    }
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E01', 'ERROR_TEXT_VERBATIM', '; error: bad argument type: stringp nil')) @($script:Shot))
    Assert-True 'an E01 host error is an operator deviation and a stop (protocol 6.1)' ($j.stopConditionRecorded -eq $true -and $j.expressionResults[0].result -ceq 'OPERATOR_DEVIATION' -and $j.expressionResults[0].category -ceq 'NONE')
    # a host error line after a valid open line is recorded in the detail of the item as HOST_ERROR/OTHER
    $j = ConvertFrom-Record (Invoke-WithScreen $r @(,@('P2-E16', 'ERROR_TEXT_VERBATIM', "P2-E16 open VALUE: FILE-DESCRIPTOR`n; error: bad argument type: FILE nil")) @($script:Shot))
    Assert-True 'a host error after a valid open line is recorded verbatim with HOST_ERROR/OTHER in the detail' ($j.characterization.openTwoArg.detail.Contains('HOST_ERROR/OTHER after the open line: ; error: bad argument type: FILE nil'))

    # =====================================================================================================================================
    Start-Section 'schema negative controls'
    $schema = Join-Path -Path $script:Family -ChildPath 'schemas/ct21d.i1-p2.v1.json'
    $r = New-RunDir
    Write-Bytes $r.Dir 'p2-a2.txt' (Get-FileBytes "`n" $script:Utf8E)
    $goodJson = ConvertFrom-Out (Invoke-WithScreen $r @(@('P2-E09', 'OK', 'P2-E09 VALUE: 3'), @('P2-E08', 'OK', 'P2-E08 VALUE: 233')) @($script:Shot) @(,@('P2-E05', $script:Shot)))
    Assert-True 'baseline record validates' (Test-Json -Json $goodJson -SchemaFile $schema)
    Assert-True 'control: the unmutated re-serialization validates (the mutation harness is not vacuous)' (Test-Json -Json (($goodJson | ConvertFrom-Json -AsHashtable) | ConvertTo-Json -Depth 10 -Compress) -SchemaFile $schema)
    $mutations = [ordered]@{
        'extra top-level property'      = { param($o) $o['extra'] = 1 }
        'governing true'                = { param($o) $o['governing'] = $true }
        'activity H1'                   = { param($o) $o['activity'] = 'H1' }
        'activity H5'                   = { param($o) $o['activity'] = 'H5' }
        'protocol family 2'             = { param($o) $o['protocolFamily'] = '2' }
        'run id of H1'                  = { param($o) $o['runId'] = 'HGP-H1-20261002T100000Z-01' }
        'run id of H5'                  = { param($o) $o['runId'] = 'HGP-H5-20261002T100000Z-01' }
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
        'status not in enum'            = { param($o) $o['characterization']['findfileAbsent']['status'] = 'MAYBE' }
        'status PASS'                   = { param($o) $o['characterization']['findfileAbsent']['status'] = 'PASS' }
        'strlen label not in enum'      = { param($o) $o['characterization']['strlenSemantics']['label'] = 'BYTES' }
        'strlen label missing'          = { param($o) $o['characterization']['strlenSemantics'].Remove('label') }
        'escape label not in enum'      = { param($o) $o['characterization']['escapeInterpretation']['label'] = 'OK' }
        'escape printed code not digits' = { param($o) $o['characterization']['escapeInterpretation']['printedCode'] = 'abc' }
        'escape printed code too long'  = { param($o) $o['characterization']['escapeInterpretation']['printedCode'] = '1234567' }
        'open form verdict not in enum' = { param($o) $o['characterization']['openTwoArg']['formVerdict'] = 'PASS' }
        'open form verdict OBSERVED'    = { param($o) $o['characterization']['openTwoArg']['formVerdict'] = 'OBSERVED' }
        'open result not in enum'       = { param($o) $o['characterization']['openTwoArg']['result'] = 'DIFFERENT' }
        'open result missing'           = { param($o) $o['characterization']['openThreeArgUtf8'].Remove('result') }
        'characterization item missing' = { param($o) $o['characterization'].Remove('productKeyShape') }
        'escape item missing'           = { param($o) $o['characterization'].Remove('escapeInterpretation') }
        'characterization extra item'   = { param($o) $o['characterization']['other'] = @{ status = 'OBSERVED'; detail = '' } }
        'screen id out of range'        = { param($o) $o['screen'][0]['id'] = 'P2-E26' }
        'screen classification'         = { param($o) $o['screen'][0]['classification'] = 'PASS' }
        'expression result not in enum' = { param($o) $o['expressionResults'][0]['result'] = 'PASS' }
        'expression result DIFFERENT'   = { param($o) $o['expressionResults'][0]['result'] = 'DIFFERENT' }
        'expression category not in enum' = { param($o) $o['expressionResults'][0]['category'] = 'WHATEVER' }
        'expression id out of range'    = { param($o) $o['expressionResults'][0]['id'] = 'P2-E00' }
        'expression extra member'       = { param($o) $o['expressionResults'][0]['x'] = 1 }
        'open gate not in enum'         = { param($o) $o['openGate'] = 'GO' }
        'checkpoint id not in enum'     = { param($o) $o['checkpoints'][0]['after'] = 'P2-E06' }
        'checkpoint hash short'         = { param($o) $o['checkpoints'][0]['sha256'] = 'abcd' }
        'checkpoint missing id not in enum' = { param($o) $o['checkpointsMissing'] = @('P2-E01') }
        'six checkpoints'               = { param($o) $o['checkpoints'] = @(1..6 | ForEach-Object { @{ after = 'P2-E05'; sha256 = ('ab' * 32) } }) }
        'screenshot hash not hex'       = { param($o) $o['screenshotSha256'] = @('zz') }
        'custody flag missing'          = { param($o) $o.Remove('screenCustodyPresent') }
        'gate field missing'            = { param($o) $o.Remove('openGate') }
        'expression results missing'    = { param($o) $o.Remove('expressionResults') }
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
