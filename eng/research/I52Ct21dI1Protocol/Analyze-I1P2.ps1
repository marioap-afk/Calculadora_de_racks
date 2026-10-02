#Requires -Version 7.0
<#
 Analyze-I1P2.ps1  --  I-1 protocol family 2, artifact P2: OFFLINE analysis of the P2 evidence files. DRAFT FOR ARCHITECT EXACT REVIEW.

 NOT EXECUTED AGAINST ANY HOST EVIDENCE. NOT AUTHORIZED TO RUN ON EVIDENCE. NON-GOVERNING (activity H10). It is not an instrument and starts nothing.

 What it does
   * READS (read-only): the two fixed files p2-a1.txt and p2-a2.txt of the P2 evidence directory (when they exist), the directory listing of that
     directory (names only; other entries are not opened), the files of this family listed in HASHES-I1-V2.txt (to verify them), and the optional
     screen-results JSON file.
   * WRITES NOTHING TO DISK. It prints the analysis record to stdout as canonical JSON (keys sorted ordinally, LF line terminator, UTF-8 without BOM,
     ASCII only: every other character is a six-character escape). The caller may redirect stdout; that redirection is the caller's act, not this
     script's.
   * Fail-closed: any problem makes it print one line "REFUSED: <reason>" to stderr, print nothing to stdout, and exit 2. Exit 0 = record printed.
     Exit 2 = refused. Exit 1 = usage / parameter error.
   * It starts no process, touches no system store, uses no network, loads no code, builds no dynamic code, and has no write primitive.
   * It rejects any transcription that appears to reveal a governed attribute value (product-key-like strings, profile names, semicolon path
     chains, machine GUIDs, OS build numbers, in any case, escaping, splitting or hex encoding) and any line outside the closed P2 output vocabulary.

 Usage
   pwsh -NoProfile -File .\Analyze-I1P2.ps1 -EvidenceDirectory <D:\...\HGP-H10-yyyymmddThhmmssZ-nn> -RunId <HGP-H10-yyyymmddThhmmssZ-nn> [-ScreenInput <path to json>]

 The leaf name of -EvidenceDirectory must equal -RunId. The directory must exist on a local fixed drive (drive-letter path; no UNC, no device path,
 no ".." segment, not a reparse point).

 Screen-results JSON (all members optional; closed):
   { "screen": [ { "id": "P2-E03", "classification": "OK|ERROR_TEXT_VERBATIM|REFUSED|DEVIATION", "text": "<verbatim result lines only>" } ],
     "screenshotSha256": [ "<64 lowercase hex>" ],
     "checkpoints": [ { "after": "P2-E05|P2-E09|P2-E17|P2-E19|P2-E25", "sha256": "<64 lowercase hex, also listed in screenshotSha256>" } ] }
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
$script:RunIdPattern = '^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$'
$script:ScreenIdPattern = '^P2-E(0[1-9]|1[0-9]|2[0-5])$'
$script:CheckpointIds = @('P2-E05', 'P2-E09', 'P2-E17', 'P2-E19', 'P2-E25')
$script:RequiredLedgerNames = @('Analyze-I1P2.ps1', 'I-1-observation-protocol-v2-P2.md', 'P2-ALLOWLIST.txt', 'schemas/ct21d.i1-p2.v1.json')
$script:Strict = [System.Text.UTF8Encoding]::new($false, $true)
$script:ArityRegex = 'too (many|few) arguments|wrong number of arguments'
$script:HelperMissingRegex = 'no function definition: ct21d-p2-'
$script:ArgValueRegex = 'bad argument|invalid argument|incorrect argument'
$script:DefinitionEchoes = @{
    'P2-E02' = 'CT21D-P2-CTL'; 'P2-E04' = 'CT21D-P2-CTL2'; 'P2-E06' = 'CT21D-P2-SHOW'; 'P2-E12' = 'CT21D-P2-LINES'; 'P2-E13' = 'CT21D-P2-SAY'
    'P2-E14' = 'CT21D-P2-AFTER'; 'P2-E15' = 'CT21D-P2-TRY'; 'P2-E21' = 'CT21D-P2-TF'; 'P2-E22' = 'CT21D-P2-PRE'; 'P2-E23' = 'CT21D-P2-STR'; 'P2-E24' = 'CT21D-P2-SHAPE'
}
$script:ClassOk = 'OK'
$script:Classifications = @('OK', 'ERROR_TEXT_VERBATIM', 'REFUSED', 'DEVIATION')

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

# What a future C2 would require of a write form: UTF-8 without BOM, the accent bytes C3 A9, a terminator LF or CRLF, the three expected lines.
function Test-C2Behaviour {
    param($Rec)
    return ($Rec.exists -and -not $Rec.bomDetected -and $Rec.encodingInterpretation -ceq 'UTF8_NO_BOM' -and $Rec.nonAsciiBytesHex -ceq 'c3a9' -and $Rec.expectedLinesPresent -and ($Rec.terminator -ceq 'LF' -or $Rec.terminator -ceq 'CRLF'))
}

# ---- governed-attribute isolation: leakage guard -----------------------------------------------------------------------------------------
# P2 prints no governed value. A transcription is refused if any view of it (as typed, with escapes expanded, with hex runs decoded, and with all
# separators removed) looks like a governed value or names a governed attribute. The reason is never echoed together with the text.
function Expand-Escapes {
    param([string]$T)
    $u = $T
    $rules = @(
        @('\\[xX]([0-9a-fA-F]{2})', 16), @('\\[uU]\+?([0-9a-fA-F]{4})', 16), @('%([0-9a-fA-F]{2})', 16),
        @('&#[xX]([0-9a-fA-F]{1,4});', 16), @('&#([0-9]{1,5});', 10), @('\\([0-7]{3})', 8),
        @('0[xX]([0-9a-fA-F]{2})', 16)
    )
    for ($pass = 0; $pass -lt 2; $pass++) {
        foreach ($rule in $rules) {
            $base = [int]$rule[1]
            $ev = [System.Text.RegularExpressions.MatchEvaluator] {
                param($m)
                $n = [System.Convert]::ToInt32($m.Groups[1].Value, $base)
                if ($n -lt 1 -or $n -gt 0xFFFF) { return $m.Value }
                return [string][char]$n
            }
            $u = [regex]::Replace($u, $rule[0], $ev)
        }
    }
    return $u
}

function Get-HexFragments {
    param([string]$T)
    $out = [System.Collections.Generic.List[string]]::new()
    foreach ($m in [regex]::Matches($T, '(?i)(?<![0-9a-f])(?:[0-9a-f]{2}(?:[\s:,\-]|\\x|0x)?){6,}')) {
        $v = [regex]::Replace($m.Value, '(?i)0x|\\x', '')
        $v = [regex]::Replace($v, '[^0-9a-fA-F]', '')
        if (($v.Length % 2) -ne 0) { $v = $v.Substring(0, $v.Length - 1) }
        $sb = [System.Text.StringBuilder]::new()
        for ($i = 0; $i -lt $v.Length; $i += 2) { $null = $sb.Append([char][System.Convert]::ToInt32($v.Substring($i, 2), 16)) }
        $out.Add($sb.ToString())
    }
    return , $out
}

# Unicode look-alikes are folded before the patterns are evaluated: compatibility normalization (NFKC: fullwidth letters, digits, backslash, semicolon),
# zero-width and variation characters removed, combining marks removed, every dash-like character mapped to '-'. Throws when the text cannot be
# normalized (for example a lone surrogate); Test-GovernedLeak treats that as a leak (fail closed).
function Get-NormalizedForLeak {
    param([string]$T)
    $n = $T.Normalize([System.Text.NormalizationForm]::FormKC)
    $n = [regex]::Replace($n, '[\u200B-\u200F\u202A-\u202E\u2060-\u2064\u00AD\uFEFF\uFE00-\uFE0F]', '')
    $n = [regex]::Replace($n, '\p{M}', '')
    $n = [regex]::Replace($n, '[\u2010-\u2015\u2212\uFE58\uFE63\uFF0D]', '-')
    $n = [regex]::Replace($n, '[\u037E\uFE54\uFF1B]', ';')   # Greek question mark, small and fullwidth semicolons
    return $n
}

function Get-LeakViews {
    param([string]$T)
    $views = [System.Collections.Generic.List[string]]::new()
    $bases = [System.Collections.Generic.List[string]]::new()
    $bases.Add($T)
    $norm = Get-NormalizedForLeak -T $T
    # ordinal comparison on purpose: -cne is culture-aware and treats canonically equivalent strings (U+037E and ';') as equal
    if (-not [string]::Equals($norm, $T, [System.StringComparison]::Ordinal)) { $bases.Add($norm) }
    foreach ($b in $bases) {
        $views.Add($b)
        $u = Expand-Escapes -T $b
        if ($u -cne $b) { $views.Add($u) }
        foreach ($f in (Get-HexFragments -T $b)) { $views.Add($f) }
        if ($u -cne $b) { foreach ($f in (Get-HexFragments -T $u)) { $views.Add($f) } }
    }
    return , $views
}

function Test-ViewLeak {
    param([string]$V)
    $low = $V.ToLowerInvariant()
    $comp0 = [regex]::Replace($low, '[^a-z0-9]', '')
    $comp1 = [regex]::Replace($comp0, 'p2e[0-9]{2}', '')   # a value split across lines that each begin with a P2 id
    foreach ($comp in @($comp0, $comp1)) {
        if ($comp.Contains('softwareautodesk') -or $comp.Contains('autodeskautocad')) { return 'a product-key-like string' }
        if ($comp -cmatch 'acad[0-9]{4}[0-9a-f]{3}') { return 'a product-key-like suffix' }
        foreach ($n in 'trustedpaths', 'secureload', 'cprofile', 'machineguid', 'osversionbuild') { if ($comp.Contains($n)) { return 'the name of a governed attribute' } }
        if ($comp.Contains('profile')) { return 'a profile-like name' }
    }
    if ($V -match '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}') { return 'a GUID' }
    $strip = [regex]::Replace($low, '[\s\-{}"''_,.]', '')
    if ($strip -cmatch '(?<![0-9a-f])[0-9a-f]{32}(?![0-9a-f])') { return 'a GUID without separators' }
    if ($V -match '(?<![0-9])10\W{1,3}0\W{1,3}[0-9]{4,6}(?![0-9])') { return 'an OS build number' }
    foreach ($line in ($V -split "`r?`n")) {
        $x = [regex]::Replace($line, '^\s*;\s*error:', '')
        if ($x.Contains(';')) { return 'a semicolon-separated list' }
    }
    return ''
}

function Test-GovernedLeak {
    param([string]$T)
    try { $views = Get-LeakViews -T $T } catch { return 'a text that cannot be normalized' }
    foreach ($v in $views) {
        $r = Test-ViewLeak -V $v
        if ($r) { return $r }
    }
    return ''
}

# ---- the closed P2 output vocabulary --------------------------------------------------------------------------------------------------------
function Test-RestVocabulary {
    param([string]$Rest, [string]$Id = '')
    $valueStr = '"(?:[A-Za-z]:[\\/][^"";\x00-\x1f]{0,200}|P2-[^"";\x00-\x1f]{1,60})"'
    $val = '(?:nil|FILE-DESCRIPTOR|-?[0-9]{1,6}|' + $valueStr + ')'
    $err = 'ERROR: [^;\x00-\x1f]{1,200}'
    $patterns = @(
        '^(AB-CD|raw-K\.txt-V)$',
        ('^VALUE: ' + $val + '$'),
        ('^' + $err + '$'),
        ('^(open|write-line|close) (VALUE: ' + $val + '|' + $err + ')$'),
        '^REFUSED, exists: p2-a[12]\.txt$',
        '^type=STR len=[0-9]{1,4} startsExact=(T|nil) startsAnyCase=(T|nil) hasWowAnyCase=(T|nil)$',
        '^type=([A-Z0-9-]{1,24}|nil)$'
    )
    foreach ($p in $patterns) { if ($Rest -cmatch $p) { return $true } }
    # P2-E20 only (protocol 6.6, any VALUE): vl-load-com may print T, nil or a symbol name. A bare symbol token: at most 41 characters, no run of 4 digits.
    if ($Id -ceq 'P2-E20' -and $Rest -cmatch '^VALUE: [A-Za-z][A-Za-z0-9*_-]{0,40}$' -and $Rest -cnotmatch '[0-9]{4}') { return $true }
    return $false
}

function Test-ScreenVocabulary {
    param([string]$Id, [string]$Text)
    foreach ($raw in ($Text -split "`r?`n")) {
        $l = $raw.Trim()
        if ($l.Length -eq 0) { continue }
        if ($l -cmatch '^; error: [^\x00-\x1f]{1,200}$') { continue }
        if ($l -cmatch '^CT21D-P2-[A-Z0-9]{1,12}$') { continue }
        if ($Id -ceq 'P2-E01' -and $l -cmatch '^"D:/I52-CT21D-HOST/evidence/HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}/"$') { continue }
        $prefix = $Id + ' '
        if ($l.StartsWith($prefix, [System.StringComparison]::Ordinal) -and (Test-RestVocabulary -Rest $l.Substring($prefix.Length) -Id $Id)) { continue }
        return $false
    }
    return $true
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
    $cps = [System.Collections.Generic.List[object]]::new()
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
                    if ($entry.id -cnotmatch $script:ScreenIdPattern) { Stop-Refused ('bad screen id: ' + $entry.id) }
                    if ($entry.classification -cnotin $script:Classifications) { Stop-Refused ('bad classification for ' + $entry.id) }
                    if ($entry.text.Length -gt 4000) { Stop-Refused ('text too long for ' + $entry.id) }
                    foreach ($other in $screen) { if ($other.id -ceq $entry.id) { Stop-Refused ('duplicate screen id: ' + $entry.id) } }
                    $t = $entry.text
                    # P2 prints no governed value; a transcription that looks like one is refused, never echoed into the record
                    $leak = Test-GovernedLeak -T $t
                    if ($leak) { Stop-Refused ('the text of ' + $entry.id + ' looks like a governed value or names a governed attribute (' + $leak + ')') }
                    if ($t.Contains('#<')) { Stop-Refused ('the text of ' + $entry.id + ' holds a stringified host object; P2 prints the token FILE-DESCRIPTOR instead') }
                    if ($entry.classification -cne 'DEVIATION' -and -not (Test-ScreenVocabulary -Id $entry.id -Text $t)) { Stop-Refused ('the text of ' + $entry.id + ' is outside the closed P2 output vocabulary') }
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
            } elseif ($p.Name -ceq 'checkpoints') {
                if ($p.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::Array) { Stop-Refused '"checkpoints" must be an array' }
                foreach ($e in $p.Value.EnumerateArray()) {
                    if ($e.ValueKind -ne [System.Text.Json.JsonValueKind]::Object) { Stop-Refused 'a checkpoint must be an object' }
                    $cp = @{}
                    foreach ($q in $e.EnumerateObject()) {
                        if ($cp.ContainsKey($q.Name)) { Stop-Refused ('duplicate member in a checkpoint: ' + $q.Name) }
                        if ($q.Name -cnotin @('after', 'sha256')) { Stop-Refused ('unknown member in a checkpoint: ' + $q.Name) }
                        if ($q.Value.ValueKind -ne [System.Text.Json.JsonValueKind]::String) { Stop-Refused ('checkpoint member must be a string: ' + $q.Name) }
                        $cp[$q.Name] = $q.Value.GetString()
                    }
                    foreach ($need in 'after', 'sha256') { if (-not $cp.ContainsKey($need)) { Stop-Refused ('checkpoint lacks ' + $need) } }
                    if ($script:CheckpointIds -cnotcontains $cp.after) { Stop-Refused ('bad checkpoint id: ' + $cp.after) }
                    if ($cp.sha256 -cnotmatch '^[0-9a-f]{64}$') { Stop-Refused 'a checkpoint hash must be 64 lowercase hex digits' }
                    foreach ($other in $cps) { if ($other.after -ceq $cp.after) { Stop-Refused ('duplicate checkpoint: ' + $cp.after) } }
                    $cps.Add($cp)
                }
            } else {
                Stop-Refused ('unknown member in ScreenInput: ' + $p.Name)
            }
        }
    } finally {
        if ($null -ne $doc) { $doc.Dispose() }
    }
    foreach ($cp in $cps) { if (-not $shots.Contains($cp.sha256)) { Stop-Refused ('the checkpoint hash after ' + $cp.after + ' is not listed in screenshotSha256') } }
    return @{ Screen = $screen; Shots = $shots; Checkpoints = $cps }
}

# ---- reading of the screen text -----------------------------------------------------------------------------------------------------------
function New-Characterization {
    param([string]$Status, [string]$Detail)
    if ($Detail.Length -gt 3900) { $Detail = $Detail.Substring(0, 3900) + ' [TRUNCATED]' }
    return [ordered]@{ status = $Status; detail = $Detail }
}

function New-OpenItem {
    param([string]$Status, [string]$Detail, [string]$Result, [string]$Form)
    if ($Detail.Length -gt 3900) { $Detail = $Detail.Substring(0, 3900) + ' [TRUNCATED]' }
    return [ordered]@{ status = $Status; detail = $Detail; result = $Result; formVerdict = $Form }
}

function New-StrlenItem {
    param([string]$Status, [string]$Detail, [string]$Label)
    if ($Detail.Length -gt 3900) { $Detail = $Detail.Substring(0, 3900) + ' [TRUNCATED]' }
    return [ordered]@{ status = $Status; detail = $Detail; label = $Label }
}

function New-EscapeItem {
    param([string]$Status, [string]$Detail, [string]$Label, [string]$Code)
    if ($Detail.Length -gt 3900) { $Detail = $Detail.Substring(0, 3900) + ' [TRUNCATED]' }
    return [ordered]@{ status = $Status; detail = $Detail; label = $Label; printedCode = $Code }
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

# kind: missing | error | nil | descriptor | string | number | other
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
            if ($rest -ceq 'VALUE: FILE-DESCRIPTOR') { return @{ kind = 'descriptor'; value = 'FILE-DESCRIPTOR' } }
            if ($rest.StartsWith('VALUE: "', [System.StringComparison]::Ordinal)) { return @{ kind = 'string'; value = $rest.Substring(7) } }
            if ($rest -cmatch '^VALUE: (-?[0-9]+)$') { return @{ kind = 'number'; value = $Matches[1] } }
            if ($rest -cmatch '^VALUE: ([A-Za-z][A-Za-z0-9*_-]{0,40})$') { return @{ kind = 'symbol'; value = $Matches[1] } }
            if ($rest.StartsWith('ERROR: ', [System.StringComparison]::Ordinal)) { return @{ kind = 'error'; value = $rest } }
            return @{ kind = 'other'; value = $rest }
        }
    }
    return @{ kind = 'other'; value = (Get-Verbatim -Entry $Entry) }
}

function Test-HelperFailure {
    param($R)
    return ($R.kind -ceq 'error' -and $R.value -imatch $script:HelperMissingRegex)
}

function Get-ErrorClass {
    param([string]$Text)
    if ($Text -imatch $script:ArityRegex) { return @{ result = 'UNSUPPORTED'; category = 'ARITY' } }
    if ($Text -imatch $script:HelperMissingRegex) { return @{ result = 'HOST_ERROR'; category = 'HELPER' } }
    if ($Text -imatch $script:ArgValueRegex) { return @{ result = 'HOST_ERROR'; category = 'ARGUMENT_VALUE' } }
    return @{ result = 'HOST_ERROR'; category = 'OTHER' }
}

# kind: none | helperfail | refused | openError | openNil | descriptor | other
function Get-OpenScreen {
    param([string]$Id, $Entry)
    if ($null -eq $Entry) { return @{ kind = 'none'; text = ''; notes = '' } }
    $lines = @(Get-EntryLines -Entry $Entry)
    $seenOpen = $false
    $kind = 'none'
    $text = ''
    $notes = [System.Collections.Generic.List[string]]::new()
    foreach ($l in $lines) {
        if (-not $seenOpen) {
            if ($l.StartsWith('; error:', [System.StringComparison]::Ordinal)) { return @{ kind = 'helperfail'; text = $l; notes = '' } }
            if ($l.StartsWith($Id + ' REFUSED', [System.StringComparison]::Ordinal)) { return @{ kind = 'refused'; text = $l; notes = '' } }
            if ($l.StartsWith($Id + ' open ', [System.StringComparison]::Ordinal)) {
                $seenOpen = $true
                $rest = $l.Substring($Id.Length + 6)
                if ($rest.StartsWith('ERROR: ', [System.StringComparison]::Ordinal)) { $kind = 'openError'; $text = $rest.Substring(7) }
                elseif ($rest -ceq 'VALUE: nil') { $kind = 'openNil'; $text = 'nil' }
                elseif ($rest -ceq 'VALUE: FILE-DESCRIPTOR') { $kind = 'descriptor'; $text = 'FILE-DESCRIPTOR' }
                else { $kind = 'other'; $text = $rest }
            }
        } elseif ($l.StartsWith($Id + ' write-line ', [System.StringComparison]::Ordinal) -or $l.StartsWith($Id + ' close ', [System.StringComparison]::Ordinal)) {
            if ($l -cmatch 'ERROR:') { $notes.Add($l) }
        } elseif ($l.StartsWith('; error:', [System.StringComparison]::Ordinal)) {
            # a host error line after a valid open line: recorded verbatim, as HOST_ERROR / OTHER, in the detail of the item
            $notes.Add('HOST_ERROR/OTHER after the open line: ' + $l)
        }
    }
    return @{ kind = $kind; text = $text; notes = ($notes -join ' | ') }
}

# The closed per-expression result model: SUPPORTED, UNSUPPORTED, HOST_ERROR, OPERATOR_DEVIATION, INCOMPLETE, UNKNOWN (+ a category).
function Get-ExpressionResult {
    param([string]$Id, $Entry, [string]$RunIdValue)
    if ($Entry.classification -ceq 'DEVIATION') { return @{ result = 'OPERATOR_DEVIATION'; category = 'NONE' } }
    if ($Entry.classification -ceq 'REFUSED') { return @{ result = 'INCOMPLETE'; category = 'GUARD' } }
    $lines = @(Get-EntryLines -Entry $Entry)
    $first = if ($lines.Count -ge 1) { $lines[0] } else { '' }
    $isErr = ($first.StartsWith('; error:', [System.StringComparison]::Ordinal) -or $first -cmatch '^P2-E[0-9]{2} ERROR:')
    if ($Id -ceq 'P2-E01') {
        if ($lines.Count -eq 1 -and $lines[0] -ceq ('"D:/I52-CT21D-HOST/evidence/' + $RunIdValue + '/"')) { return @{ result = 'SUPPORTED'; category = 'NONE' } }
        # protocol 6.1: anything else on P2-E01, a host error included, is the E01 deviation (stop)
        return @{ result = 'OPERATOR_DEVIATION'; category = 'NONE' }
    }
    if ($script:DefinitionEchoes.ContainsKey($Id)) {
        if ($lines.Count -eq 1 -and $lines[0] -ceq $script:DefinitionEchoes[$Id]) { return @{ result = 'SUPPORTED'; category = 'NONE' } }
        if ($isErr) { return @{ result = 'HOST_ERROR'; category = 'DEFINITION' } }
        return @{ result = 'UNKNOWN'; category = 'OTHER' }
    }
    if ($Id -ceq 'P2-E03' -or $Id -ceq 'P2-E05') {
        if ($isErr) { return (Get-ErrorClass -Text (Get-Verbatim -Entry $Entry)) }
        return @{ result = 'SUPPORTED'; category = 'NONE' }
    }
    if ($Id -ceq 'P2-E16' -or $Id -ceq 'P2-E17') {
        $a = Get-OpenScreen -Id $Id -Entry $Entry
        switch ($a.kind) {
            'descriptor' { return @{ result = 'SUPPORTED'; category = 'NONE' } }
            'openError' { return (Get-ErrorClass -Text $a.text) }
            'helperfail' { return @{ result = 'INCOMPLETE'; category = 'HELPER' } }
            'refused' { return @{ result = 'INCOMPLETE'; category = 'GUARD' } }
            default { return @{ result = 'UNKNOWN'; category = 'OTHER' } }
        }
    }
    if ($Id -ceq 'P2-E25') {
        if ($isErr) { return (Get-ErrorClass -Text (Get-Verbatim -Entry $Entry)) }
        if ($first -cmatch '^P2-E25 type=') { return @{ result = 'SUPPORTED'; category = 'NONE' } }
        return @{ result = 'UNKNOWN'; category = 'OTHER' }
    }
    $r = Read-ShowResult -Entry $Entry -Id $Id
    if ($r.kind -ceq 'error') { return (Get-ErrorClass -Text $r.value) }
    if ($r.kind -ceq 'other') { return @{ result = 'UNKNOWN'; category = 'OTHER' } }
    return @{ result = 'SUPPORTED'; category = 'NONE' }
}

# ---- derivation of the characterization --------------------------------------------------------------------------------------------------
function Get-OpenItem {
    param([string]$Id, $Rec, $Entry, [bool]$Custody, [string]$Gate)
    $fileSummary = $Rec.name + ': exists=' + $Rec.exists + ' encoding=' + $Rec.encodingInterpretation + ' terminator=' + $Rec.terminator + ' accentSegmentHex=' + $Rec.accentSegmentHex + ' linesOk=' + $Rec.expectedLinesPresent
    $a = Get-OpenScreen -Id $Id -Entry $Entry
    $notes = if ($a.notes) { ' Later lines: ' + $a.notes + '.' } else { '' }
    if ($Gate -ceq 'STOP_BEFORE_OPEN') {
        return (New-OpenItem 'NOT_OBSERVED' 'STOP_BEFORE_OPEN: the findfile controls did not print the required lines; no open candidate was executed' 'INCOMPLETE' 'INCOMPLETE')
    }
    if ($Rec.exists) {
        if ($a.kind -cin @('openError', 'openNil', 'helperfail', 'refused')) {
            return (New-OpenItem 'UNKNOWN' ('the screen text and the file state disagree (' + $a.kind + '). ' + $fileSummary) 'UNKNOWN' 'UNKNOWN')
        }
        if (Test-C2Behaviour -Rec $Rec) {
            return (New-OpenItem 'OBSERVED' ('the form was accepted and the file shows the behaviour a future C2 would require (UTF-8 without BOM, accent bytes C3A9, terminator LF or CRLF, three lines). ' + $fileSummary + $notes) 'SUPPORTED' 'SUPPORTED')
        }
        return (New-OpenItem 'OBSERVED_DIFFERS' ('the form was accepted but the file differs from the behaviour a future C2 would require (UTF-8 without BOM, accent bytes C3A9, terminator LF or CRLF, three lines). ' + $fileSummary + $notes) 'SUPPORTED' 'DIFFERENT')
    }
    if (-not $Custody -or $null -eq $Entry) { return (New-OpenItem 'UNKNOWN' ('no file and no custodied screen result for ' + $Id) 'UNKNOWN' 'UNKNOWN') }
    switch ($a.kind) {
        'helperfail' { return (New-OpenItem 'NOT_OBSERVED' ('a host error was printed before any open line; it is not evidence about open: ' + $a.text) 'INCOMPLETE' 'INCOMPLETE') }
        'refused' { return (New-OpenItem 'NOT_OBSERVED' ('the create-new guard refused; open was not attempted: ' + $a.text) 'INCOMPLETE' 'INCOMPLETE') }
        'openError' {
            $cls = Get-ErrorClass -Text $a.text
            return (New-OpenItem 'OBSERVED_DIFFERS' ('open was rejected by the host, text verbatim: ' + $a.text) $cls.result $cls.result)
        }
        'openNil' { return (New-OpenItem 'UNKNOWN' 'open returned nil (no descriptor, no error); the cause is not separable on the screen' 'UNKNOWN' 'UNKNOWN') }
        'descriptor' { return (New-OpenItem 'UNKNOWN' ('a descriptor was returned but no file exists to analyze offline.' + $notes) 'UNKNOWN' 'UNKNOWN') }
        default { return (New-OpenItem 'UNKNOWN' ('no recognizable open line for ' + $Id + ' in the screen text') 'UNKNOWN' 'UNKNOWN') }
    }
}

function Get-Characterization {
    param($Files, $ScreenById, [bool]$Custody, [bool]$Stop, [string]$Gate)
    $c = [ordered]@{}
    if ($Stop) {
        $why = 'STOP_CONDITION_RECORDED (a DEVIATION or REFUSED entry, or a write the protocol forbids); fail-closed'
        foreach ($n in 'findfileAbsent', 'findfilePresent', 'writeLineTerminator', 'productKeyShape', 'userFunctionArityControl') { $c[$n] = New-Characterization 'UNKNOWN' $why }
        foreach ($n in 'openTwoArg', 'openThreeArgUtf8') { $c[$n] = New-OpenItem 'UNKNOWN' $why 'UNKNOWN' 'UNKNOWN' }
        $c['strlenSemantics'] = New-StrlenItem 'UNKNOWN' $why 'NOT_DETERMINED'
        $c['escapeInterpretation'] = New-EscapeItem 'UNKNOWN' $why 'NOT_DETERMINED' ''
        return $c
    }
    $noCustody = 'screen result without screenshot custody (screenshotSha256 empty)'
    function Get-Entry { param($Id) if ($Custody -and $ScreenById.ContainsKey($Id)) { return $ScreenById[$Id] } return $null }
    function Get-Why { param($Id) if (-not $Custody) { return $noCustody } return ($Id + ' missing') }
    $f1 = $Files[0]; $f2 = $Files[1]

    # userFunctionArityControl: E02..E05
    $e3 = Get-Entry 'P2-E03'; $e5 = Get-Entry 'P2-E05'
    $defFail = $false
    foreach ($id in 'P2-E02', 'P2-E04') {
        $d = Get-Entry $id
        if ($null -ne $d -and (Get-ExpressionResult -Id $id -Entry $d -RunIdValue '').result -ceq 'HOST_ERROR') { $defFail = $true }
    }
    if ($null -eq $e3 -or $null -eq $e5) {
        $c['userFunctionArityControl'] = New-Characterization 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E03 or P2-E05 missing' })
    } else {
        $x3 = Get-ExpressionResult -Id 'P2-E03' -Entry $e3 -RunIdValue ''
        $x5 = Get-ExpressionResult -Id 'P2-E05' -Entry $e5 -RunIdValue ''
        $l3 = @(Get-EntryLines -Entry $e3); $l5 = @(Get-EntryLines -Entry $e5)
        $ok3 = ($e3.classification -ceq 'OK' -and $l3.Count -eq 1 -and $l3[0] -ceq 'P2-E03 AB-CD')
        $ok5 = ($e5.classification -ceq 'OK' -and $l5.Count -eq 1 -and $l5[0] -ceq 'P2-E05 raw-K.txt-V')
        if ($defFail -or $x3.category -ceq 'HELPER' -or $x5.category -ceq 'HELPER') {
            $c['userFunctionArityControl'] = New-Characterization 'NOT_OBSERVED' ('a definition or helper error is a recorded protocol result, not a v1 compatibility mismatch. E03: ' + (Get-Verbatim -Entry $e3) + ' | E05: ' + (Get-Verbatim -Entry $e5))
        } elseif ($ok3 -and $ok5) {
            $c['userFunctionArityControl'] = New-Characterization 'OBSERVED' 'both two-argument calls of the v1-shaped user functions returned the expected strings'
        } else {
            $c['userFunctionArityControl'] = New-Characterization 'OBSERVED_DIFFERS' ('E03: ' + (Get-Verbatim -Entry $e3) + ' | E05: ' + (Get-Verbatim -Entry $e5))
        }
    }

    # findfileAbsent (E10) / findfilePresent (E11)
    $r10 = Read-ShowResult -Entry (Get-Entry 'P2-E10') -Id 'P2-E10'
    if (Test-HelperFailure $r10) { $c['findfileAbsent'] = New-Characterization 'NOT_OBSERVED' ('helper error, not evidence about findfile: ' + $r10.value) }
    else {
        switch ($r10.kind) {
            'nil' { $c['findfileAbsent'] = New-Characterization 'OBSERVED' 'findfile returned nil for an absent path' }
            'missing' { $c['findfileAbsent'] = New-Characterization 'UNKNOWN' (Get-Why 'P2-E10') }
            'other' { $c['findfileAbsent'] = New-Characterization 'UNKNOWN' ('unparsable P2-E10 text: ' + $r10.value) }
            default { $c['findfileAbsent'] = New-Characterization 'OBSERVED_DIFFERS' ('findfile on an absent path gave ' + $r10.kind + ': ' + $r10.value) }
        }
    }
    $r11 = Read-ShowResult -Entry (Get-Entry 'P2-E11') -Id 'P2-E11'
    $cross = [System.Collections.Generic.List[string]]::new()
    $crossBad = $false
    foreach ($pair in @(@('P2-E18', $f1), @('P2-E19', $f2))) {
        $r = Read-ShowResult -Entry (Get-Entry $pair[0]) -Id $pair[0]
        if ($r.kind -ceq 'missing') { continue }
        if ($r.kind -ceq 'string' -and $pair[1].exists) { $cross.Add($pair[0] + ' consistent') }
        elseif ($r.kind -ceq 'nil' -and -not $pair[1].exists) { $cross.Add($pair[0] + ' consistent') }
        else { $cross.Add($pair[0] + ' INCONSISTENT with the file state (' + $r.kind + ', exists=' + $pair[1].exists + ')'); $crossBad = $true }
    }
    $crossText = if ($cross.Count -gt 0) { ' After-write checks: ' + ($cross -join '; ') + '.' } else { '' }
    if (Test-HelperFailure $r11) { $c['findfilePresent'] = New-Characterization 'NOT_OBSERVED' ('helper error, not evidence about findfile: ' + $r11.value) }
    else {
        switch ($r11.kind) {
            'string' {
                if ($crossBad) { $c['findfilePresent'] = New-Characterization 'OBSERVED_DIFFERS' ('findfile found an existing system file but disagrees with the file state.' + $crossText) }
                else { $c['findfilePresent'] = New-Characterization 'OBSERVED' ('findfile returned a string for an existing file.' + $crossText) }
            }
            'missing' { $c['findfilePresent'] = New-Characterization 'UNKNOWN' (Get-Why 'P2-E11') }
            'other' { $c['findfilePresent'] = New-Characterization 'UNKNOWN' ('unparsable P2-E11 text: ' + $r11.value) }
            default { $c['findfilePresent'] = New-Characterization 'OBSERVED_DIFFERS' ('findfile on an existing file gave ' + $r11.kind + ': ' + $r11.value + $crossText) }
        }
    }

    # escapeInterpretation: E08 (ascii of the escape), separate from strlen
    $r8 = Read-ShowResult -Entry (Get-Entry 'P2-E08') -Id 'P2-E08'
    if ($r8.kind -ceq 'missing') { $c['escapeInterpretation'] = New-EscapeItem 'UNKNOWN' (Get-Why 'P2-E08') 'NOT_DETERMINED' '' }
    elseif (Test-HelperFailure $r8) { $c['escapeInterpretation'] = New-EscapeItem 'NOT_OBSERVED' ('helper error, not evidence about ascii: ' + $r8.value) 'NOT_DETERMINED' '' }
    elseif ($r8.kind -ceq 'number') {
        $code = [string]$r8.value
        $lead = 'the printed code of the first character produced by the escape is ' + $code + '. This is a characterization label, not an assumption; the byte encoding is determined only by the offline file bytes.'
        if ($code -ceq '233') { $c['escapeInterpretation'] = New-EscapeItem 'OBSERVED' ($lead + ' 233 is the code point of e-acute: an accent-code-point-like observation.') 'ACCENT_CODEPOINT_LIKE' $code }
        elseif ($code -ceq '195') { $c['escapeInterpretation'] = New-EscapeItem 'OBSERVED_DIFFERS' ($lead + ' 195 is the first byte of the UTF-8 pair C3 A9: a UTF-8-lead-byte-like observation.') 'UTF8_LEAD_BYTE_LIKE' $code }
        elseif ($code -ceq '92') { $c['escapeInterpretation'] = New-EscapeItem 'OBSERVED_DIFFERS' ($lead + ' 92 is the backslash: the escape appears uninterpreted.') 'BACKSLASH_UNINTERPRETED' $code }
        else { $c['escapeInterpretation'] = New-EscapeItem 'OBSERVED_DIFFERS' ($lead + ' Another code.') 'OTHER_CODE' $code }
    } else {
        $c['escapeInterpretation'] = New-EscapeItem 'OBSERVED_DIFFERS' ('ascii of the escape gave ' + $r8.kind + ': ' + $r8.value) 'NOT_DETERMINED' ''
    }

    # strlenSemantics: E07 (control) and E09 (escaped string), separate from the escape reading
    $r7 = Read-ShowResult -Entry (Get-Entry 'P2-E07') -Id 'P2-E07'
    $r9 = Read-ShowResult -Entry (Get-Entry 'P2-E09') -Id 'P2-E09'
    $labelNote = ' This is a characterization label, not an assumption; the byte encoding is determined only by the offline file bytes.'
    if ($r7.kind -ceq 'missing' -or $r9.kind -ceq 'missing') {
        $c['strlenSemantics'] = New-StrlenItem 'UNKNOWN' $(if (-not $Custody) { $noCustody } else { 'P2-E07 or P2-E09 missing' }) 'NOT_DETERMINED'
    } elseif ((Test-HelperFailure $r7) -or (Test-HelperFailure $r9)) {
        $c['strlenSemantics'] = New-StrlenItem 'NOT_OBSERVED' ('helper error, not evidence about strlen. E07: ' + $r7.value + ' | E09: ' + $r9.value) 'NOT_DETERMINED'
    } elseif (-not ($r7.kind -ceq 'number' -and $r7.value -ceq '3')) {
        $c['strlenSemantics'] = New-StrlenItem 'OBSERVED_DIFFERS' ('the control strlen of ABC was not 3. E07: ' + $r7.kind + ' ' + $r7.value + ' | E09: ' + $r9.kind + ' ' + $r9.value) 'OTHER_VALUE'
    } elseif ($r9.kind -ceq 'number' -and $r9.value -ceq '3') {
        $c['strlenSemantics'] = New-StrlenItem 'OBSERVED' ('strlen of A, one escaped e-acute and Z is 3: a character-interpretation candidate.' + $labelNote) 'CHARACTER_INTERPRETATION_CANDIDATE'
    } elseif ($r9.kind -ceq 'number' -and $r9.value -ceq '4') {
        $c['strlenSemantics'] = New-StrlenItem 'OBSERVED_DIFFERS' ('strlen of A, one escaped e-acute and Z is 4: a UTF-8-byte-count-like observation (not an assumption that strlen counts bytes).' + $labelNote) 'UTF8_BYTE_COUNT_LIKE'
    } elseif ($r9.kind -ceq 'number' -and $r9.value -ceq '9') {
        $c['strlenSemantics'] = New-StrlenItem 'OBSERVED_DIFFERS' ('strlen of the escaped string is 9: the escape appears uninterpreted (9 literal characters).' + $labelNote) 'ESCAPE_UNINTERPRETED'
    } else {
        $c['strlenSemantics'] = New-StrlenItem 'OBSERVED_DIFFERS' ('E09: ' + $r9.kind + ' ' + $r9.value) 'OTHER_VALUE'
    }

    # productKeyShape: E25 (shape only; the value is never printed or stored)
    $e25 = Get-Entry 'P2-E25'
    if ($null -eq $e25) {
        $c['productKeyShape'] = New-Characterization 'UNKNOWN' (Get-Why 'P2-E25')
    } else {
        $l25 = @(Get-EntryLines -Entry $e25)
        $shape = $null
        foreach ($l in $l25) { if ($l -cmatch '^P2-E25 type=STR len=([0-9]+) startsExact=(T|nil) startsAnyCase=(T|nil) hasWowAnyCase=(T|nil)$') { $shape = @($Matches[1], $Matches[2], $Matches[3], $Matches[4]) } }
        $v25 = Get-Verbatim -Entry $e25
        if ($v25 -imatch $script:HelperMissingRegex) {
            $c['productKeyShape'] = New-Characterization 'NOT_OBSERVED' ('helper error, not evidence about the product-key shape: ' + $v25)
        } elseif ($null -ne $shape) {
            $txt = 'len=' + $shape[0] + ' startsExact=' + $shape[1] + ' startsAnyCase=' + $shape[2] + ' hasWowAnyCase=' + $shape[3]
            if ($shape[1] -ceq 'T' -and $shape[3] -ceq 'nil') { $c['productKeyShape'] = New-Characterization 'OBSERVED' ('string; ' + $txt) }
            else { $c['productKeyShape'] = New-Characterization 'OBSERVED_DIFFERS' ('string; ' + $txt) }
        } elseif ($e25.classification -ceq 'ERROR_TEXT_VERBATIM' -or ($l25.Count -ge 1 -and $l25[0] -cmatch '^P2-E25 (ERROR:|type=)')) {
            $c['productKeyShape'] = New-Characterization 'OBSERVED_DIFFERS' $v25
        } else {
            $c['productKeyShape'] = New-Characterization 'UNKNOWN' ('unparsable P2-E25 text: ' + $v25)
        }
    }

    # open candidates
    $c['openTwoArg'] = Get-OpenItem -Id 'P2-E16' -Rec $f1 -Entry (Get-Entry 'P2-E16') -Custody $Custody -Gate $Gate
    $c['openThreeArgUtf8'] = Get-OpenItem -Id 'P2-E17' -Rec $f2 -Entry (Get-Entry 'P2-E17') -Custody $Custody -Gate $Gate

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
    if ($RunId -cnotmatch $script:RunIdPattern) { Stop-Refused 'RunId does not match ^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$' }
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
    $cpIn = [System.Collections.Generic.List[object]]::new()
    if ($PSBoundParameters.ContainsKey('ScreenInput')) {
        $in = Read-ScreenInput -Path $ScreenInput
        foreach ($e in $in.Screen) { $screen.Add($e) }
        foreach ($s in $in.Shots) { $shots.Add($s) }
        foreach ($cp in $in.Checkpoints) { $cpIn.Add($cp) }
    }
    $screen.Sort([System.Comparison[object]] { param($a, $b) [string]::CompareOrdinal($a.id, $b.id) })
    $shots.Sort([System.StringComparer]::Ordinal)
    $cpIn.Sort([System.Comparison[object]] { param($a, $b) [string]::CompareOrdinal($a.after, $b.after) })
    $byId = @{}
    foreach ($e in $screen) { $byId[$e.id] = $e }
    $custody = ($shots.Count -gt 0)

    # the findfile gate that precedes the open candidates: E10 must print VALUE: nil and E11 must print a string
    $gate = 'UNKNOWN'
    if ($custody -and $byId.ContainsKey('P2-E10') -and $byId.ContainsKey('P2-E11')) {
        $g10 = Read-ShowResult -Entry $byId['P2-E10'] -Id 'P2-E10'
        $g11 = Read-ShowResult -Entry $byId['P2-E11'] -Id 'P2-E11'
        $gate = if ($g10.kind -ceq 'nil' -and $g11.kind -ceq 'string') { 'PASSED' } else { 'STOP_BEFORE_OPEN' }
    }

    # the closed per-expression results
    $exprOut = [System.Collections.Generic.List[object]]::new()
    $stop = $false
    foreach ($e in $screen) {
        $x = Get-ExpressionResult -Id $e.id -Entry $e -RunIdValue $RunId
        $num = [int]$e.id.Substring(4)
        if (-not $custody) { $x = @{ result = 'UNKNOWN'; category = 'NONE' } }
        if ($gate -ceq 'STOP_BEFORE_OPEN' -and $num -ge 12) { $x = @{ result = 'OPERATOR_DEVIATION'; category = 'GUARD' } }
        if ($e.classification -ceq 'DEVIATION' -or $e.classification -ceq 'REFUSED' -or $x.result -ceq 'OPERATOR_DEVIATION') { $stop = $true }
        $exprOut.Add([ordered]@{ id = $e.id; result = $x.result; category = $x.category })
    }
    if ($gate -ceq 'STOP_BEFORE_OPEN') {
        for ($n = 12; $n -le 25; $n++) {
            $id = 'P2-E{0:00}' -f $n
            if (-not $byId.ContainsKey($id)) { $exprOut.Add([ordered]@{ id = $id; result = 'INCOMPLETE'; category = 'GUARD' }) }
        }
        foreach ($f in $files) { if ($f.exists) { $stop = $true } }
    }
    $exprSorted = [System.Collections.Generic.List[object]]::new()
    foreach ($x in ($exprOut | Sort-Object -Property { $_.id } -CaseSensitive)) { $exprSorted.Add($x) }

    $char = Get-Characterization -Files $files.ToArray() -ScreenById $byId -Custody $custody -Stop $stop -Gate $gate

    $cpOut = [System.Collections.Generic.List[object]]::new()
    $cpHave = @{}
    foreach ($cp in $cpIn) { $cpOut.Add([ordered]@{ after = $cp.after; sha256 = $cp.sha256 }); $cpHave[$cp.after] = $true }
    $cpMissing = [System.Collections.Generic.List[string]]::new()
    foreach ($id in $script:CheckpointIds) { if (-not $cpHave.ContainsKey($id)) { $cpMissing.Add($id) } }

    $screenOut = [System.Collections.Generic.List[object]]::new()
    foreach ($e in $screen) { $screenOut.Add([ordered]@{ id = $e.id; classification = $e.classification; text = $e.text }) }
    $record = [ordered]@{
        schemaVersion         = 'ct21d.i1-p2.v1'
        runId                 = $RunId
        activity              = 'H10'
        governing             = $false
        protocolFamily        = 'I1-F2'
        p2TextSha256          = $family.P2
        ledgerSha256          = $family.Ledger
        files                 = $files.ToArray()
        otherEntryNames       = $others.ToArray()
        screen                = $screenOut.ToArray()
        expressionResults     = $exprSorted.ToArray()
        openGate              = $gate
        screenshotSha256      = $shots.ToArray()
        checkpoints           = $cpOut.ToArray()
        checkpointsMissing    = $cpMissing.ToArray()
        screenCustodyPresent  = $custody
        stopConditionRecorded = $stop
        characterization      = $char
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
