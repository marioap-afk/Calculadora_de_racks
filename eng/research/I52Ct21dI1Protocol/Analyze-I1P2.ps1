#Requires -Version 7.0
<#
 Analyze-I1P2.ps1  --  I-1 protocol family 2, artifact P2: OFFLINE analysis of the P2 evidence files. DRAFT FOR ARCHITECT EXACT REVIEW.

 NOT EXECUTED AGAINST ANY HOST EVIDENCE. NOT AUTHORIZED TO RUN ON EVIDENCE. NON-GOVERNING (activity H5). It is not an instrument and starts nothing.

 What it does
   * READS (read-only): the two fixed files p2-a1.txt and p2-a2.txt of the P2 evidence directory (when they exist), the directory listing of that
     directory (names only; other entries are not opened), the files of this family listed in HASHES-I1-V2.txt (to verify them), and the optional
     screen-results JSON file.
   * WRITES NOTHING TO DISK. It prints the analysis record to stdout as canonical JSON (keys sorted ordinally, LF line terminator, UTF-8 without BOM,
     ASCII only: every other character is a six-character escape). The caller may redirect stdout; that redirection is the caller's act, not this
     script's.
   * Fail-closed: any problem makes it print one line "REFUSED: <reason>" to stderr, print nothing to stdout, and exit 2. Exit 0 = record printed.
     Exit 2 = refused. Exit 1 = usage / parameter error.
   * It starts no process, touches no registry, uses no network, loads no code, builds no dynamic code, and has no write primitive.

 Usage
   pwsh -NoProfile -File .\Analyze-I1P2.ps1 -EvidenceDirectory <D:\...\HGP-H5-yyyymmddThhmmssZ-nn> -RunId <HGP-H5-yyyymmddThhmmssZ-nn> [-ScreenInput <path to json>]

 The leaf name of -EvidenceDirectory must equal -RunId. The directory must exist on a local fixed drive (drive-letter path; no UNC, no device path,
 no ".." segment, not a reparse point).

 Screen-results JSON (all members optional; closed):
   { "screen": [ { "id": "P2-E03", "classification": "OK|ERROR_TEXT_VERBATIM|REFUSED|DEVIATION", "text": "<verbatim lines>" } ],
     "screenshotSha256": [ "<64 lowercase hex>" ] }
 Without at least one screenshot hash the items derived from the screen are UNKNOWN (no custody). A DEVIATION or REFUSED entry makes every item UNKNOWN
 (stopConditionRecorded = true). The screenshots govern any difference with this transcription.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
    [Parameter(Mandatory = $true)][string]$RunId,
    [string]$ScreenInput
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:MaxFileBytes = 4096
$script:MaxInputBytes = 262144
$script:FixedNames = @('p2-a1.txt', 'p2-a2.txt')
$script:RunIdPattern = '^HGP-H5-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$'
$script:RequiredLedgerNames = @('Analyze-I1P2.ps1', 'I-1-observation-protocol-v2-P2.md', 'P2-ALLOWLIST.txt', 'schemas/ct21d.i1-p2.v1.json')
$script:Strict = [System.Text.UTF8Encoding]::new($false, $true)

function Stop-Refused {
    param([string]$Reason)
    throw [System.InvalidOperationException]::new('REFUSED: ' + $Reason)
}

function Get-Sha256Hex {
    param([byte[]]$Bytes)
    return [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Bytes)).ToLowerInvariant()
}

function Get-HexString {
    param([byte[]]$Bytes, [int]$Start, [int]$Count)
    if ($Count -le 0) { return '' }
    return [System.Convert]::ToHexString($Bytes, $Start, $Count).ToLowerInvariant()
}

function Find-Bytes {
    param([byte[]]$Haystack, [byte[]]$Needle, [int]$From)
    for ($i = $From; $i -le $Haystack.Length - $Needle.Length; $i++) {
        $hit = $true
        for ($j = 0; $j -lt $Needle.Length; $j++) {
            if ($Haystack[$i + $j] -ne $Needle[$j]) { $hit = $false; break }
        }
        if ($hit) { return $i }
    }
    return -1
}

# ---- path guards -----------------------------------------------------------------------------------------------------------------------
function Assert-LocalDriveLetterPath {
    param([string]$Path, [string]$What)
    if ([string]::IsNullOrEmpty($Path) -or $Path -notmatch '^[A-Za-z]:[\\/]') { Stop-Refused ($What + ' must be an absolute drive-letter path (no UNC, no device path, no relative path)') }
    if (($Path -split '[\\/]') -contains '..') { Stop-Refused ($What + ' must not contain a ".." segment') }
    if ($Path.IndexOf([char]0) -ge 0) { Stop-Refused ($What + ' contains a NUL') }
    $drive = [System.IO.DriveInfo]::new($Path.Substring(0, 1))
    if ($drive.DriveType -ne [System.IO.DriveType]::Fixed) { Stop-Refused ($What + ' is not on a local fixed drive') }
}

function Assert-NotReparse {
    param([System.IO.FileSystemInfo]$Item, [string]$What)
    if (($Item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { Stop-Refused ($What + ' is a reparse point') }
}

# ---- the family must be the reviewed bytes ---------------------------------------------------------------------------------------------
function Test-Family {
    $ledgerPath = Join-Path -Path $PSScriptRoot -ChildPath 'HASHES-I1-V2.txt'
    if (-not (Test-Path -LiteralPath $ledgerPath -PathType Leaf)) { Stop-Refused 'HASHES-I1-V2.txt is missing next to the script' }
    $ledgerBytes = [System.IO.File]::ReadAllBytes($ledgerPath)
    if ($ledgerBytes.Length -gt 65536) { Stop-Refused 'the ledger is too large' }
    $ledgerText = $script:Strict.GetString($ledgerBytes)
    if ($ledgerText.Contains("`r") -or -not $ledgerText.EndsWith("`n")) { Stop-Refused 'the ledger must be LF-terminated text without CR' }
    $seen = @{}
    foreach ($line in $ledgerText.Substring(0, $ledgerText.Length - 1).Split("`n")) {
        if ($line -cnotmatch '^([0-9a-f]{64})  ([A-Za-z0-9.][A-Za-z0-9._/-]*)$') { Stop-Refused ('bad ledger line: ' + $line) }
        $expected = $Matches[1]
        $rel = $Matches[2]
        if ($rel.Contains('..') -or $rel -ceq 'HASHES-I1-V2.txt' -or $seen.ContainsKey($rel)) { Stop-Refused ('bad or repeated ledger name: ' + $rel) }
        $seen[$rel] = $true
        $full = Join-Path -Path $PSScriptRoot -ChildPath $rel
        if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { Stop-Refused ('ledger file missing: ' + $rel) }
        Assert-NotReparse -Item (Get-Item -LiteralPath $full -Force) -What ('ledger file ' + $rel)
        $actual = Get-Sha256Hex -Bytes ([System.IO.File]::ReadAllBytes($full))
        if ($actual -cne $expected) { Stop-Refused ('family file differs from the ledger: ' + $rel) }
    }
    foreach ($req in $script:RequiredLedgerNames) {
        if (-not $seen.ContainsKey($req)) { Stop-Refused ('the ledger does not list ' + $req) }
    }
    $p2 = Get-Sha256Hex -Bytes ([System.IO.File]::ReadAllBytes((Join-Path -Path $PSScriptRoot -ChildPath 'I-1-observation-protocol-v2-P2.md')))
    return @{ P2 = $p2; Ledger = (Get-Sha256Hex -Bytes $ledgerBytes) }
}

# ---- analysis of one fixed file -------------------------------------------------------------------------------------------------------
function Get-FileRecord {
    param([string]$Directory, [string]$Name, $Entry)
    $rec = [ordered]@{
        name = $Name; exists = $false; length = 0; sha256 = ''; first64BytesHex = ''; bomDetected = $false; terminator = 'NONE'
        accentMarkersFound = $false; accentSegmentHex = ''; nonAsciiBytesHex = ''; expectedLinesPresent = $false; encodingInterpretation = 'UNKNOWN'
    }
    if ($null -eq $Entry) { return $rec }
    if ($Entry.PSIsContainer) { Stop-Refused ($Name + ' is a directory') }
    Assert-NotReparse -Item $Entry -What $Name
    if ($Entry.Length -gt $script:MaxFileBytes) { Stop-Refused ($Name + ' is larger than ' + $script:MaxFileBytes + ' bytes') }
    $b = [System.IO.File]::ReadAllBytes((Join-Path -Path $Directory -ChildPath $Name))
    if ($b.Length -gt $script:MaxFileBytes) { Stop-Refused ($Name + ' grew beyond ' + $script:MaxFileBytes + ' bytes while being read') }
    $rec.exists = $true
    $rec.length = $b.Length
    $rec.sha256 = Get-Sha256Hex -Bytes $b
    $rec.first64BytesHex = Get-HexString -Bytes $b -Start 0 -Count ([Math]::Min(64, $b.Length))

    $utf8Bom = ($b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF)
    $otherBom = ($b.Length -ge 2 -and (($b[0] -eq 0xFF -and $b[1] -eq 0xFE) -or ($b[0] -eq 0xFE -and $b[1] -eq 0xFF)))
    $rec.bomDetected = ($utf8Bom -or $otherBom)

    $crlf = 0; $lf = 0; $cr = 0
    for ($i = 0; $i -lt $b.Length; $i++) {
        if ($b[$i] -eq 13) {
            if ($i + 1 -lt $b.Length -and $b[$i + 1] -eq 10) { $crlf++; $i++ } else { $cr++ }
        } elseif ($b[$i] -eq 10) { $lf++ }
    }
    if (($crlf + $lf + $cr) -eq 0) { $rec.terminator = 'NONE' }
    elseif ($lf -gt 0 -and $crlf -eq 0 -and $cr -eq 0) { $rec.terminator = 'LF' }
    elseif ($crlf -gt 0 -and $lf -eq 0 -and $cr -eq 0) { $rec.terminator = 'CRLF' }
    elseif ($cr -gt 0 -and $lf -eq 0 -and $crlf -eq 0) { $rec.terminator = 'CR' }
    else { $rec.terminator = 'MIXED' }

    $m1 = [System.Text.Encoding]::ASCII.GetBytes('P2-ACCENT-')
    $m2 = [System.Text.Encoding]::ASCII.GetBytes('-END')
    $segment = [byte[]]@()
    $at1 = Find-Bytes -Haystack $b -Needle $m1 -From 0
    if ($at1 -ge 0) {
        $segStart = $at1 + $m1.Length
        $at2 = Find-Bytes -Haystack $b -Needle $m2 -From $segStart
        if ($at2 -ge 0) {
            $rec.accentMarkersFound = $true
            $segment = [byte[]]$b[$segStart..($at2 - 1)]
            if ($at2 -eq $segStart) { $segment = [byte[]]@() }
        }
    }
    $rec.accentSegmentHex = Get-HexString -Bytes $segment -Start 0 -Count $segment.Length
    $nonAscii = [System.Collections.Generic.List[byte]]::new()
    foreach ($x in $segment) { if ($x -ge 0x80) { $nonAscii.Add($x) } }
    $rec.nonAsciiBytesHex = Get-HexString -Bytes $nonAscii.ToArray() -Start 0 -Count $nonAscii.Count

    # the three fixed lines (Latin-1 view: one char per byte; a UTF-8 BOM is skipped)
    if ($rec.terminator -ceq 'LF' -or $rec.terminator -ceq 'CRLF') {
        $unit = if ($rec.terminator -ceq 'LF') { "`n" } else { "`r`n" }
        $skip = if ($utf8Bom) { 3 } else { 0 }
        $text = [System.Text.Encoding]::Latin1.GetString($b, $skip, $b.Length - $skip)
        if ($text.EndsWith($unit, [System.StringComparison]::Ordinal)) {
            $parts = $text.Substring(0, $text.Length - $unit.Length).Split($unit)
            if ($parts.Count -eq 3 -and $parts[0] -ceq 'P2-ASCII-LINE-1' -and $parts[2] -ceq 'P2-ASCII-LINE-3' -and $parts[1] -cmatch '^P2-ACCENT-.*-END$') {
                $rec.expectedLinesPresent = $true
            }
        }
    }

    $hasZero = $false; $hasHigh = $false
    foreach ($x in $b) { if ($x -eq 0) { $hasZero = $true }; if ($x -ge 0x80) { $hasHigh = $true } }
    $validUtf8 = $true
    try { $null = $script:Strict.GetString($b) } catch [System.Text.DecoderFallbackException] { $validUtf8 = $false }
    if ($b.Length -eq 0) { $rec.encodingInterpretation = 'UNKNOWN' }
    elseif ($utf8Bom) { $rec.encodingInterpretation = 'UTF8_WITH_BOM' }
    elseif ($otherBom -or $hasZero) { $rec.encodingInterpretation = 'OTHER' }
    elseif (-not $hasHigh) { $rec.encodingInterpretation = 'ASCII_ONLY' }
    elseif ($validUtf8) { $rec.encodingInterpretation = 'UTF8_NO_BOM' }
    elseif ($rec.accentMarkersFound -and $segment.Length -eq 1 -and $segment[0] -ge 0x80) { $rec.encodingInterpretation = 'SINGLE_BYTE' }
    else { $rec.encodingInterpretation = 'OTHER' }
    return $rec
}

# ---- the optional screen-results JSON ---------------------------------------------------------------------------------------------------
function Read-ScreenInput {
    param([string]$Path)
    Assert-LocalDriveLetterPath -Path $Path -What 'ScreenInput'
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { Stop-Refused 'ScreenInput is not an existing file' }
    $item = Get-Item -LiteralPath $Path -Force
    Assert-NotReparse -Item $item -What 'ScreenInput'
    if ($item.Length -gt $script:MaxInputBytes) { Stop-Refused 'ScreenInput is too large' }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) { Stop-Refused 'ScreenInput must be UTF-8 without BOM' }
    try { $text = $script:Strict.GetString($bytes) } catch { Stop-Refused 'ScreenInput is not valid UTF-8' }

    $screen = [System.Collections.Generic.List[object]]::new()
    $shots = [System.Collections.Generic.List[string]]::new()
    $doc = $null
    try {
        try { $doc = [System.Text.Json.JsonDocument]::Parse($text) } catch { Stop-Refused 'ScreenInput is not valid JSON' }
        $root = $doc.RootElement
        if ($root.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) { Stop-Refused 'ScreenInput root must be an object' }
        $topSeen = @{}
        foreach ($p in $root.EnumerateObject()) {
            if ($topSeen.ContainsKey($p.Name)) { Stop-Refused ('duplicate member in ScreenInput: ' + $p.Name) }
            $topSeen[$p.Name] = $true
            if ($p.Name -ceq 'screen') {
                if ($p.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::Array) { Stop-Refused '"screen" must be an array' }
                foreach ($e in $p.Value.EnumerateArray()) {
                    if ($e.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) { Stop-Refused 'a screen entry must be an object' }
                    $entry = @{}
                    foreach ($q in $e.EnumerateObject()) {
                        if ($entry.ContainsKey($q.Name)) { Stop-Refused ('duplicate member in a screen entry: ' + $q.Name) }
                        if ($q.Name -cnotin @('id', 'classification', 'text')) { Stop-Refused ('unknown member in a screen entry: ' + $q.Name) }
                        if ($q.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::String) { Stop-Refused ('screen member must be a string: ' + $q.Name) }
                        $entry[$q.Name] = $q.Value.GetString()
                    }
                    foreach ($need in 'id', 'classification', 'text') { if (-not $entry.ContainsKey($need)) { Stop-Refused ('screen entry lacks ' + $need) } }
                    if ($entry.id -cnotmatch '^P2-E(0[1-9]|1[0-9])$') { Stop-Refused ('bad screen id: ' + $entry.id) }
                    if ($entry.classification -cnotin @('OK', 'ERROR_TEXT_VERBATIM', 'REFUSED', 'DEVIATION')) { Stop-Refused ('bad classification for ' + $entry.id) }
                    if ($entry.text.Length -gt 4000) { Stop-Refused ('text too long for ' + $entry.id) }
                    foreach ($other in $screen) { if ($other.id -ceq $entry.id) { Stop-Refused ('duplicate screen id: ' + $entry.id) } }
                    $t = $entry.text
                    # P2 prints no governed value; a transcription that looks like one is refused, never echoed into the record
                    if ($t -cmatch '(?i)software\\autodesk|acad-[0-9]{4}:[0-9a-f]{3}|TRUSTEDPATHS|SECURELOAD|CPROFILE|MachineGuid|OsVersionBuild') { Stop-Refused ('the text of ' + $entry.id + ' looks like a governed value or names a governed attribute') }
                    $looksError = ($t -cmatch 'ERROR:') -or ($t -cmatch '; error:')
                    $looksRefused = $t.Contains('REFUSED')
                    if ($entry.classification -ceq 'OK' -and ($looksError -or $looksRefused)) { Stop-Refused ('classification OK contradicts the text of ' + $entry.id) }
                    if ($entry.classification -ceq 'ERROR_TEXT_VERBATIM' -and -not $looksError) { Stop-Refused ('classification ERROR_TEXT_VERBATIM but no error text in ' + $entry.id) }
                    if ($entry.classification -ceq 'REFUSED' -and -not $looksRefused) { Stop-Refused ('classification REFUSED but no REFUSED text in ' + $entry.id) }
                    $screen.Add($entry)
                }
            } elseif ($p.Name -ceq 'screenshotSha256') {
                if ($p.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::Array) { Stop-Refused '"screenshotSha256" must be an array' }
                foreach ($e in $p.Value.EnumerateArray()) {
                    if ($e.ValueKind -ne [System.Text.Json.JsonValueKind]::String) { Stop-Refused 'a screenshot hash must be a string' }
                    $h = $e.GetString()
                    if ($h -cnotmatch '^[0-9a-f]{64}$') { Stop-Refused 'a screenshot hash must be 64 lowercase hex digits' }
                    if ($shots.Contains($h)) { Stop-Refused 'duplicate screenshot hash' }
                    $shots.Add($h)
                }
            } else {
                Stop-Refused ('unknown member in ScreenInput: ' + $p.Name)
            }
        }
    } finally {
        if ($null -ne $doc) { $doc.Dispose() }
    }
    return @{ Screen = $screen; Shots = $shots }
}

# ---- derivation of the characterization --------------------------------------------------------------------------------------------------
function New-Characterization {
    param([string]$Status, [string]$Detail)
    if ($Detail.Length -gt 3900) { $Detail = $Detail.Substring(0, 3900) + ' [TRUNCATED]' }
    return [ordered]@{ status = $Status; detail = $Detail }
}

function Get-Verbatim {
    param($Entry)
    $t = ($Entry.text -replace "`r`n", "`n").Trim()
    if ($t.Length -gt 600) { $t = $t.Substring(0, 600) + ' [TRUNCATED]' }
    return $t
}

function Get-EntryLines {
    param($Entry)
    if ($null -eq $Entry) { return @() }
    return @(($Entry.text -split "`r?`n") | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 })
}

# kind: missing | error | nil | string | number | other
function Read-ShowResult {
    param($Entry, [string]$Id)
    if ($null -eq $Entry) { return @{ kind = 'missing'; value = '' } }
    $lines = @(Get-EntryLines -Entry $Entry)
    if ($lines.Count -ge 1 -and $lines[0].StartsWith('; error:', [System.StringComparison]::Ordinal)) { return @{ kind = 'error'; value = (Get-Verbatim -Entry $Entry) } }
    $prefix = $Id + ' '
    foreach ($l in $lines) {
        if ($l.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            $rest = $l.Substring($prefix.Length)
            if ($rest -ceq 'VALUE: nil') { return @{ kind = 'nil'; value = 'nil' } }
            if ($rest.StartsWith('VALUE: "', [System.StringComparison]::Ordinal)) { return @{ kind = 'string'; value = $rest.Substring(7) } }
            if ($rest -cmatch '^VALUE: (-?[0-9]+)$') { return @{ kind = 'number'; value = $Matches[1] } }
            if ($rest.StartsWith('ERROR: ', [System.StringComparison]::Ordinal)) { return @{ kind = 'error'; value = $rest } }
            return @{ kind = 'other'; value = $rest }
        }
    }
    return @{ kind = 'other'; value = (Get-Verbatim -Entry $Entry) }
}

function Get-OpenVerdict {
    param([string]$Id, $Rec, $Entry, [bool]$Custody, [bool]$Utf8Candidate)
    $fileSummary = $Rec.name + ': exists=' + $Rec.exists + ' encoding=' + $Rec.encodingInterpretation + ' terminator=' + $Rec.terminator + ' accentSegmentHex=' + $Rec.accentSegmentHex + ' linesOk=' + $Rec.expectedLinesPresent
    if ($Rec.exists) {
        if (-not $Utf8Candidate) {
            return (New-Characterization 'OBSERVED' ('open with two arguments created the file. ' + $fileSummary))
        }
        if ($Rec.encodingInterpretation -ceq 'UTF8_NO_BOM' -and $Rec.nonAsciiBytesHex -ceq 'c3a9' -and $Rec.expectedLinesPresent) {
            return (New-Characterization 'OBSERVED' ('open with the encoding argument created a UTF-8 file without BOM with C3A9. ' + $fileSummary))
        }
        return (New-Characterization 'OBSERVED_DIFFERS' ('open with the encoding argument created a file that is not UTF-8 without BOM with C3A9 and the three expected lines. ' + $fileSummary))
    }
    if (-not $Custody -or $null -eq $Entry) { return (New-Characterization 'UNKNOWN' ('no file and no custodied screen result for ' + $Id)) }
    $lines = @(Get-EntryLines -Entry $Entry)
    if ($lines.Count -ge 1 -and $lines[0].StartsWith('; error:', [System.StringComparison]::Ordinal)) {
        return (New-Characterization 'OBSERVED_DIFFERS' ('the expression aborted with the host text: ' + (Get-Verbatim -Entry $Entry)))
    }
    $prefix = $Id + ' open '
    foreach ($l in $lines) {
        if ($l.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            $rest = $l.Substring($prefix.Length)
            if ($rest.StartsWith('ERROR: ', [System.StringComparison]::Ordinal)) { return (New-Characterization 'OBSERVED_DIFFERS' ('open was rejected by the host, text: ' + $rest.Substring(7))) }
            if ($rest -ceq 'VALUE: nil') { return (New-Characterization 'UNKNOWN' 'open returned nil (no descriptor, no error); the cause is not separable on the screen') }
            return (New-Characterization 'UNKNOWN' ('open line without a file: ' + $rest))
        }
    }
    return (New-Characterization 'UNKNOWN' ('no open line for ' + $Id + ' in the screen text'))
}

function Get-Characterization {
    param($Files, $ScreenById, [bool]$Custody, [bool]$Stop)
    $names = 'openTwoArg', 'openThreeArgUtf8', 'findfileAbsent', 'findfilePresent', 'writeLineTerminator', 'strlenSemantics', 'productKeyShape', 'userFunctionArityControl'
    $c = [ordered]@{}
    if ($Stop) {
        foreach ($n in $names) { $c[$n] = New-Characterization 'UNKNOWN' 'STOP_CONDITION_RECORDED (a DEVIATION or REFUSED entry); fail-closed' }
        return $c
    }
    $noCustody = 'screen result without screenshot custody (screenshotSha256 empty)'
    function Get-Entry { param($Id) if ($Custody -and $ScreenById.ContainsKey($Id)) { return $ScreenById[$Id] } return $null }
    $f1 = $Files[0]; $f2 = $Files[1]

    # userFunctionArityControl: E03 and E05
    $e3 = Get-Entry 'P2-E03'; $e5 = Get-Entry 'P2-E05'
    if ($null -eq $e3 -or $null -eq $e5) {
        $c['userFunctionArityControl'] = New-Characterization 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E03 or P2-E05 missing' })
    } else {
        $l3 = @(Get-EntryLines -Entry $e3); $l5 = @(Get-EntryLines -Entry $e5)
        $ok3 = ($e3.classification -ceq 'OK' -and $l3.Count -eq 1 -and $l3[0] -ceq 'P2-E03 AB-CD')
        $ok5 = ($e5.classification -ceq 'OK' -and $l5.Count -eq 1 -and $l5[0] -ceq 'P2-E05 raw-K.txt-V')
        if ($ok3 -and $ok5) { $c['userFunctionArityControl'] = New-Characterization 'OBSERVED' 'both two-argument calls of the v1-shaped user functions returned the expected strings' }
        else { $c['userFunctionArityControl'] = New-Characterization 'OBSERVED_DIFFERS' ('E03: ' + (Get-Verbatim -Entry $e3) + ' | E05: ' + (Get-Verbatim -Entry $e5)) }
    }

    # findfileAbsent / findfilePresent
    $r7 = Read-ShowResult -Entry (Get-Entry 'P2-E07') -Id 'P2-E07'
    switch ($r7.kind) {
        'nil' { $c['findfileAbsent'] = New-Characterization 'OBSERVED' 'findfile returned nil for an absent path' }
        'missing' { $c['findfileAbsent'] = New-Characterization 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E07 missing' }) }
        'other' { $c['findfileAbsent'] = New-Characterization 'UNKNOWN' ('unparsable P2-E07 text: ' + $r7.value) }
        default { $c['findfileAbsent'] = New-Characterization 'OBSERVED_DIFFERS' ('findfile on an absent path gave ' + $r7.kind + ': ' + $r7.value) }
    }
    $r8 = Read-ShowResult -Entry (Get-Entry 'P2-E08') -Id 'P2-E08'
    $cross = [System.Collections.Generic.List[string]]::new()
    $crossBad = $false
    foreach ($pair in @(@('P2-E15', $f1), @('P2-E16', $f2))) {
        $r = Read-ShowResult -Entry (Get-Entry $pair[0]) -Id $pair[0]
        if ($r.kind -ceq 'missing') { continue }
        if ($r.kind -ceq 'string' -and $pair[1].exists) { $cross.Add($pair[0] + ' consistent') }
        elseif ($r.kind -ceq 'nil' -and -not $pair[1].exists) { $cross.Add($pair[0] + ' consistent') }
        else { $cross.Add($pair[0] + ' INCONSISTENT with the file state (' + $r.kind + ', exists=' + $pair[1].exists + ')'); $crossBad = $true }
    }
    $crossText = if ($cross.Count -gt 0) { ' After-write checks: ' + ($cross -join '; ') + '.' } else { '' }
    switch ($r8.kind) {
        'string' {
            if ($crossBad) { $c['findfilePresent'] = New-Characterization 'OBSERVED_DIFFERS' ('findfile found an existing system file but disagrees with the file state.' + $crossText) }
            else { $c['findfilePresent'] = New-Characterization 'OBSERVED' ('findfile returned a string for an existing file.' + $crossText) }
        }
        'missing' { $c['findfilePresent'] = New-Characterization 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E08 missing' }) }
        'other' { $c['findfilePresent'] = New-Characterization 'UNKNOWN' ('unparsable P2-E08 text: ' + $r8.value) }
        default { $c['findfilePresent'] = New-Characterization 'OBSERVED_DIFFERS' ('findfile on an existing file gave ' + $r8.kind + ': ' + $r8.value + $crossText) }
    }

    # strlenSemantics: E09 and E10
    $r9 = Read-ShowResult -Entry (Get-Entry 'P2-E09') -Id 'P2-E09'
    $r10 = Read-ShowResult -Entry (Get-Entry 'P2-E10') -Id 'P2-E10'
    if ($r9.kind -ceq 'missing' -or $r10.kind -ceq 'missing') {
        $c['strlenSemantics'] = New-Characterization 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E09 or P2-E10 missing' })
    } elseif ($r9.kind -ceq 'number' -and $r9.value -ceq '3' -and $r10.kind -ceq 'number' -and $r10.value -ceq '3') {
        $c['strlenSemantics'] = New-Characterization 'OBSERVED' 'strlen counts characters (ABC = 3; A + one escaped e-acute + Z = 3)'
    } elseif ($r10.kind -ceq 'number' -and $r10.value -ceq '4' -and $r9.kind -ceq 'number' -and $r9.value -ceq '3') {
        $c['strlenSemantics'] = New-Characterization 'OBSERVED_DIFFERS' 'strlen counts bytes (the escaped e-acute counted as 2)'
    } elseif ($r10.kind -ceq 'number' -and $r10.value -ceq '9') {
        $c['strlenSemantics'] = New-Characterization 'OBSERVED_DIFFERS' 'the U+00E9 escape was not interpreted (9 literal characters)'
    } else {
        $c['strlenSemantics'] = New-Characterization 'OBSERVED_DIFFERS' ('E09: ' + $r9.kind + ' ' + $r9.value + ' | E10: ' + $r10.kind + ' ' + $r10.value)
    }

    # productKeyShape: E19 (shape only; the value is never printed or stored)
    $e19 = Get-Entry 'P2-E19'
    if ($null -eq $e19) {
        $c['productKeyShape'] = New-Characterization 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E19 missing' })
    } else {
        $l19 = @(Get-EntryLines -Entry $e19)
        $shape = $null
        foreach ($l in $l19) { if ($l -cmatch '^P2-E19 type=STR len=([0-9]+) startsExact=(T|nil) startsAnyCase=(T|nil) hasWowAnyCase=(T|nil)$') { $shape = @($Matches[1], $Matches[2], $Matches[3], $Matches[4]) } }
        if ($null -ne $shape) {
            $txt = 'len=' + $shape[0] + ' startsExact=' + $shape[1] + ' startsAnyCase=' + $shape[2] + ' hasWowAnyCase=' + $shape[3]
            if ($shape[1] -ceq 'T' -and $shape[3] -ceq 'nil') { $c['productKeyShape'] = New-Characterization 'OBSERVED' ('string; ' + $txt) }
            else { $c['productKeyShape'] = New-Characterization 'OBSERVED_DIFFERS' ('string; ' + $txt) }
        } elseif ($e19.classification -ceq 'ERROR_TEXT_VERBATIM' -or ($l19.Count -ge 1 -and $l19[0] -cmatch '^P2-E19 (ERROR:|type=)')) {
            $c['productKeyShape'] = New-Characterization 'OBSERVED_DIFFERS' (Get-Verbatim -Entry $e19)
        } else {
            $c['productKeyShape'] = New-Characterization 'UNKNOWN' ('unparsable P2-E19 text: ' + (Get-Verbatim -Entry $e19))
        }
    }

    # open candidates
    $c['openTwoArg'] = Get-OpenVerdict -Id 'P2-E13' -Rec $f1 -Entry (Get-Entry 'P2-E13') -Custody $Custody -Utf8Candidate $false
    $c['openThreeArgUtf8'] = Get-OpenVerdict -Id 'P2-E14' -Rec $f2 -Entry (Get-Entry 'P2-E14') -Custody $Custody -Utf8Candidate $true

    # writeLineTerminator: from the file bytes only
    $existing = @($Files | Where-Object { $_.exists })
    if ($existing.Count -eq 0) {
        $c['writeLineTerminator'] = New-Characterization 'NOT_OBSERVABLE' 'no P2 file exists; the terminator is not shown on the command line'
    } else {
        $parts = @($existing | ForEach-Object { $_.name + '=' + $_.terminator })
        $allGood = (@($existing | Where-Object { $_.terminator -cne 'LF' -and $_.terminator -cne 'CRLF' }).Count -eq 0)
        if ($allGood) { $c['writeLineTerminator'] = New-Characterization 'OBSERVED' ($parts -join '; ') }
        else { $c['writeLineTerminator'] = New-Characterization 'OBSERVED_DIFFERS' ($parts -join '; ') }
    }
    return $c
}

# ---- canonical JSON ----------------------------------------------------------------------------------------------------------------------
function ConvertTo-JsonString {
    param([string]$S)
    $sb = [System.Text.StringBuilder]::new()
    $null = $sb.Append([char]34)
    foreach ($ch in $S.ToCharArray()) {
        $n = [int]$ch
        if ($n -eq 34) { $null = $sb.Append([char]92).Append([char]34) }
        elseif ($n -eq 92) { $null = $sb.Append([char]92).Append([char]92) }
        elseif ($n -eq 10) { $null = $sb.Append([char]92).Append('n') }
        elseif ($n -eq 13) { $null = $sb.Append([char]92).Append('r') }
        elseif ($n -eq 9) { $null = $sb.Append([char]92).Append('t') }
        elseif ($n -lt 32 -or $n -gt 126) { $null = $sb.Append([char]92).Append('u').Append($n.ToString('x4')) }
        else { $null = $sb.Append($ch) }
    }
    $null = $sb.Append([char]34)
    return $sb.ToString()
}

function ConvertTo-CanonicalJson {
    param($Value)
    if ($null -eq $Value) { return 'null' }
    if ($Value -is [bool]) { return $(if ($Value) { 'true' } else { 'false' }) }
    if ($Value -is [int] -or $Value -is [long]) { return ([long]$Value).ToString([System.Globalization.CultureInfo]::InvariantCulture) }
    if ($Value -is [string]) { return (ConvertTo-JsonString -S $Value) }
    if ($Value -is [System.Collections.IDictionary]) {
        $keys = [System.Collections.Generic.List[string]]::new()
        foreach ($k in $Value.Keys) { $keys.Add([string]$k) }
        $keys.Sort([System.StringComparer]::Ordinal)
        $parts = foreach ($k in $keys) { (ConvertTo-JsonString -S $k) + ':' + (ConvertTo-CanonicalJson -Value $Value[$k]) }
        return '{' + (@($parts) -join ',') + '}'
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        $parts = foreach ($v in $Value) { ConvertTo-CanonicalJson -Value $v }
        return '[' + (@($parts) -join ',') + ']'
    }
    Stop-Refused ('cannot serialize a value of type ' + $Value.GetType().FullName)
}

# ---- main ------------------------------------------------------------------------------------------------------------------------------------
try {
    if ($RunId -cnotmatch $script:RunIdPattern) { Stop-Refused 'RunId does not match ^HGP-H5-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$' }
    Assert-LocalDriveLetterPath -Path $EvidenceDirectory -What 'EvidenceDirectory'
    if (-not (Test-Path -LiteralPath $EvidenceDirectory -PathType Container)) { Stop-Refused 'EvidenceDirectory is not an existing directory' }
    $dirItem = Get-Item -LiteralPath $EvidenceDirectory -Force
    Assert-NotReparse -Item $dirItem -What 'EvidenceDirectory'
    if ($dirItem.Name -cne $RunId) { Stop-Refused 'the leaf name of EvidenceDirectory must equal RunId' }
    $dirFull = $dirItem.FullName

    $family = Test-Family

    $entries = @(Get-ChildItem -LiteralPath $dirFull -Force)
    $files = [System.Collections.Generic.List[object]]::new()
    foreach ($name in $script:FixedNames) {
        $hit = $null
        foreach ($en in $entries) { if ($en.Name -ceq $name) { $hit = $en } }
        $files.Add((Get-FileRecord -Directory $dirFull -Name $name -Entry $hit))
    }
    $others = [System.Collections.Generic.List[string]]::new()
    foreach ($en in $entries) { if ($script:FixedNames -cnotcontains $en.Name) { $others.Add($en.Name) } }
    $others.Sort([System.StringComparer]::Ordinal)

    $screen = [System.Collections.Generic.List[object]]::new()
    $shots = [System.Collections.Generic.List[string]]::new()
    if ($PSBoundParameters.ContainsKey('ScreenInput')) {
        $in = Read-ScreenInput -Path $ScreenInput
        foreach ($e in $in.Screen) { $screen.Add($e) }
        foreach ($s in $in.Shots) { $shots.Add($s) }
    }
    $screen.Sort([System.Comparison[object]] { param($a, $b) [string]::CompareOrdinal($a.id, $b.id) })
    $shots.Sort([System.StringComparer]::Ordinal)
    $byId = @{}
    foreach ($e in $screen) { $byId[$e.id] = $e }
    $custody = ($shots.Count -gt 0)
    $stop = $false
    foreach ($e in $screen) { if ($e.classification -ceq 'DEVIATION' -or $e.classification -ceq 'REFUSED') { $stop = $true } }

    $char = Get-Characterization -Files $files.ToArray() -ScreenById $byId -Custody $custody -Stop $stop

    $screenOut = [System.Collections.Generic.List[object]]::new()
    foreach ($e in $screen) { $screenOut.Add([ordered]@{ id = $e.id; classification = $e.classification; text = $e.text }) }
    $record = [ordered]@{
        schemaVersion        = 'ct21d.i1-p2.v1'
        runId                = $RunId
        activity             = 'H5'
        governing            = $false
        protocolFamily       = 'I1-F2'
        p2TextSha256         = $family.P2
        ledgerSha256         = $family.Ledger
        files                = $files.ToArray()
        otherEntryNames      = $others.ToArray()
        screen               = $screenOut.ToArray()
        screenshotSha256     = $shots.ToArray()
        screenCustodyPresent = $custody
        stopConditionRecorded = $stop
        characterization     = $char
    }
    $json = ConvertTo-CanonicalJson -Value $record
    $schemaPath = Join-Path -Path $PSScriptRoot -ChildPath 'schemas/ct21d.i1-p2.v1.json'
    $valid = Test-Json -Json $json -SchemaFile $schemaPath -ErrorAction SilentlyContinue
    if ($valid -ne $true) { Stop-Refused 'the analysis record does not validate against schemas/ct21d.i1-p2.v1.json' }

    $outBytes = [System.Text.UTF8Encoding]::new($false).GetBytes($json + "`n")
    $stdout = [System.Console]::OpenStandardOutput()
    $stdout.Write($outBytes, 0, $outBytes.Length)
    $stdout.Flush()
    exit 0
} catch {
    $msg = $_.Exception.Message
    if (-not $msg.StartsWith('REFUSED:', [System.StringComparison]::Ordinal)) { $msg = 'REFUSED: unexpected failure: ' + $_.Exception.GetType().Name + ': ' + $msg }
    [System.Console]::Error.WriteLine($msg)
    exit 2
}
