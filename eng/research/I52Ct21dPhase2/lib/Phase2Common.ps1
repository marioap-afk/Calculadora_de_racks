# CT-21D phase 2 session-binding attestation: shared library (dot-sourced by Capture-Phase2.ps1 and Validate-Phase2.ps1).
#
# STATUS: REVIEW MATERIAL. NOT RUN ON ANY HOST. Nothing here starts AutoCAD, loads a DLL into AutoCAD, runs NETLOAD, writes the
# registry, changes an ACL, or touches TRUSTEDPATHS / SECURELOAD. The ONLY write primitive is Write-Phase2NewFile (create-new, flat
# name, inside a validated local evidence root). See README.md for the authority (decisions sections 237-248).
#
# Everything the capture decides is computed by the PURE function Get-Phase2Checks over the record data, so that the offline
# validator re-derives exactly the same checks from the written record.

Set-StrictMode -Version Latest

$script:Phase2SchemaId = 'ct21d.phase2.v1'
# The CAPTURE BINDING ID is derived from the observed process; it is NOT the CAD-manager-assigned sessionId of package v3 3.5.3 / OI-6
# (that one is an input, tupleInputs.sessionId, assigned in phase 1 and echoed by the designation file).
$script:Phase2CaptureBindingDerivationText = "sha256(utf8('CT21D-PHASE2-CAPTURE-BINDING-V1' LF pid LF processStartUtc LF runId LF)) first 16 lowercase hex"
$script:Phase2RunIdPattern = '^HGP-H[0-9]+-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$'
$script:Phase2AssignedSessionIdPattern = '^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$'
$script:Phase2JsonLenientNumbers = $false
$script:Phase2VerdictOk ='PHASE2_EXTERNAL_OBSERVATIONS_VALID'
$script:Phase2VerdictInvalid = 'PHASE2_INVALID'
$script:Phase2VerdictStandIn = 'PHASE2_TEST_STANDIN_OBSERVATIONS_OK'
$script:Phase2UtcPattern = '^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\.[0-9]{3}Z$'
$script:Phase2DefaultProfilesSubKey = 'Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles'

# Activity numbers of the run ids (decisions section 247, ruling 2).
$script:Phase2SessionActivity = @{ 'S1-A' = 1; 'S1-B' = 2; 'S1-C' = 3; 'S2' = 8; 'S3' = 9; 'S4' = 4 }

# The fixed, ordered list of validation checks. The offline validator requires exactly this list in this order.
$script:Phase2CheckNames = @(
    'PROCESS_NAME_IS_EXPECTED',
    'SINGLE_TARGET_PROCESS',
    'PROCESS_OWNER_IS_INVOKING_USER',
    'ACAD_BUILD_OBSERVED',
    'ACAD_SHA256_EQUALS_TUPLE',
    'PROFILE_EFFECTIVE_OBSERVED',
    'PROFILE_SOURCES_CONSISTENT',
    'PROFILE_EQUALS_EXPECTED',
    'MODULES_ENUMERATED',
    'MODULE_HASHES_READABLE',
    'MODULE_INVENTORY_SORTED_UNIQUE',
    'MAIN_MODULE_IN_INVENTORY',
    'NO_DECLARED_SET_MODULE_LOADED',
    'PROCESS_LIST_ENUMERATED',
    'DECLARED_SET_JSON_WELL_FORMED',
    'DECLARED_SET_SHA256_EQUALS_TUPLE',
    'DECLARED_SET_SINGLE_FOLDER',
    'DECLARED_FOLDER_CONTENT_EXACT',
    'DECLARED_DLL_HASHES_EQUAL_ENTRY',
    'PIN_FILES_EQUAL_DLL_AND_ENTRY',
    'TRANSCRIPT_REQUIREMENT_MET',
    'TRANSCRIPT_CONSISTENT_WITH_SESSION',
    'SESSION_ID_ASSIGNED_WELL_FORMED',
    'MODULE_BASELINE_COMPARISON'
)

# What an external capture cannot observe. Constant, recorded verbatim in every record.
$script:Phase2HostToConfirm = @(
    [ordered]@{ item = 'CPROFILE_INSIDE_PROCESS'; reason = 'The profile name seen from inside the running AutoCAD cannot be read from outside; the effective profile here is the command-line /p value or the registry default at capture time.'; resolvedBy = 'I-1 protocol A1-R4 (typed AutoLISP getvar CPROFILE, raw-AutoCadProfile.txt)' },
    [ordered]@{ item = 'SECURELOAD_INSIDE_PROCESS'; reason = 'The in-process value is not observable externally. The at-rest value stored in the profile registry key is recorded read-only as profileAtRest (informational, in no check); it is not the in-process value.'; resolvedBy = 'I-1 protocol A1-R5 (typed AutoLISP, raw-SECURELOAD.txt)' },
    [ordered]@{ item = 'TRUSTEDPATHS_INSIDE_PROCESS'; reason = 'The in-process value is not observable externally. The at-rest value stored in the profile registry key is recorded read-only as profileAtRest (informational, in no check); it is not the in-process value.'; resolvedBy = 'I-1 protocol A1-R6 (typed AutoLISP, raw-TRUSTEDPATHS.txt)' },
    [ordered]@{ item = 'STARTUP_SWITCHES_RECORDED'; reason = 'The command line switches are recorded (process.commandLineSwitches). A startup script (/b) or a language/product switch (/ld) can run code at start-up; no check judges them.'; resolvedBy = 'CAD manager attestation of the exact start-up command (package v3 3.7 step 2)' },
    [ordered]@{ item = 'RUNTIME_IDENTITY_AND_SESSION_PROFILE_FACTS_NOT_COVERED'; reason = 'The template 3.5.3 fields sessionProfileFacts (CPROFILE, SECURELOAD, TRUSTEDPATHS) and runtimeIdentities are not covered by the verdict; see the coverage section of the record.'; resolvedBy = 'I-1 protocol A1-R4..R6 and the instruments; CAD manager human fields' },
    [ordered]@{ item = 'PIN_CHECK_AFTER_LOAD_NOT_COVERED'; reason = 'Template 3.5.3 defines pinCheck as hashes taken after the instrument load step, but phase 2 runs before any load (package v3 3.7 step 3 precedes step 5). The pinCheck section and the check PIN_FILES_EQUAL_DLL_AND_ENTRY are an at-rest, pre-load check only; a PHASE2_EXTERNAL_OBSERVATIONS_VALID verdict does not satisfy the after-load pin check.'; resolvedBy = 'The step 5 pin check after the load (CAD manager / instrument protocol); see the coverage section of the record' },
    [ordered]@{ item = 'PROFILE_SWITCH_AFTER_START'; reason = 'A profile switch inside the session after start-up changes the registry default; this capture reads it once, at capture time.'; resolvedBy = 'CAD manager attestation; profile read again at the end of the session (package v3 3.7 step 8)' },
    [ordered]@{ item = 'ON_DISK_VERSUS_IN_MEMORY_IMAGE'; reason = 'Module hashes are hashes of the files on disk named by the loader, not of the mapped images.'; resolvedBy = 'Accepted residual (README); the .pin check has the same limit (R0 README, Pinning)' },
    [ordered]@{ item = 'SIGNER_STATUS_REVOCATION'; reason = 'Authenticode status can depend on certificate-revocation lookups (network); it is informational and not part of any check.'; resolvedBy = 'Informational only' },
    [ordered]@{ item = 'PROFILE_REGISTRY_LOCATION'; reason = 'The profile registry key (default ACAD-8101:409 for AutoCAD 2025 English) and the reading of its default value as the current profile come from the preparation records, not from a file of the repository; an absent key fails closed.'; resolvedBy = 'CAD manager confirms the key on the designated machine (ProfilesSubKey parameter)' },
    [ordered]@{ item = 'RUNTIME_DOCUMENT_DATABASE_IDENTITY'; reason = 'Document and database identity exist only inside the process, and no instrument is loaded at phase 2.'; resolvedBy = 'The instrument records runtimeIdentities where the command reads them' }
)

# ---------------------------------------------------------------------------------------------------------------------------
# Refusals (nothing is written; exit 1)
# ---------------------------------------------------------------------------------------------------------------------------
function Stop-Phase2Refusal {
    param([string]$Message)
    throw ('REFUSED: ' + $Message)
}

# ---------------------------------------------------------------------------------------------------------------------------
# Hashing
# ---------------------------------------------------------------------------------------------------------------------------
function ConvertTo-Phase2Hex {
    param([byte[]]$Bytes)
    $sb = New-Object System.Text.StringBuilder
    foreach ($b in $Bytes) { [void]$sb.Append($b.ToString('x2')) }
    return $sb.ToString()
}

function Get-Phase2BytesSha256 {
    param([byte[]]$Bytes)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return (ConvertTo-Phase2Hex -Bytes $sha.ComputeHash($Bytes)) } finally { $sha.Dispose() }
}

# SHA-256 of a file, opened read-only with sharing (loaded DLLs are readable). Returns $null when it cannot be read.
function Get-Phase2FileSha256 {
    param([string]$Path)
    try {
        $fs = New-Object System.IO.FileStream($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
        try {
            $sha = [System.Security.Cryptography.SHA256]::Create()
            try { return (ConvertTo-Phase2Hex -Bytes $sha.ComputeHash($fs)) } finally { $sha.Dispose() }
        } finally { $fs.Dispose() }
    } catch {
        return $null
    }
}

# Reads at most $MaxBytes of a file (read-only, shared). Throws when the file is larger than $MaxBytes.
function Read-Phase2FileBytes {
    param([string]$Path, [long]$MaxBytes)
    $fs = New-Object System.IO.FileStream($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))
    try {
        if ($fs.Length -gt $MaxBytes) { throw ('file larger than ' + $MaxBytes + ' bytes') }
        $buf = New-Object byte[] ([int]$fs.Length)
        $read = 0
        while ($read -lt $buf.Length) {
            $n = $fs.Read($buf, $read, $buf.Length - $read)
            if ($n -le 0) { break }
            $read += $n
        }
        if ($read -ne $buf.Length) { throw 'short read' }
        return , $buf
    } finally { $fs.Dispose() }
}

function Get-Phase2CaptureBindingId {
    param([int]$ProcessIdValue, [string]$StartUtc, [string]$RunId)
    $text = 'CT21D-PHASE2-CAPTURE-BINDING-V1' + "`n" + $ProcessIdValue.ToString([System.Globalization.CultureInfo]::InvariantCulture) + "`n" + $StartUtc + "`n" + $RunId + "`n"
    $bytes = (New-Object System.Text.UTF8Encoding($false)).GetBytes($text)
    return (Get-Phase2BytesSha256 -Bytes $bytes).Substring(0, 16)
}

# ---------------------------------------------------------------------------------------------------------------------------
# Ordinal sorting by a named key (no culture, no pipeline sort)
# ---------------------------------------------------------------------------------------------------------------------------
function Sort-Phase2ByKey {
    param([object[]]$Items, [string]$KeyName, [switch]$Lower, [switch]$Numeric)
    $list = New-Object 'System.Collections.Generic.List[object]'
    foreach ($item in $Items) {
        $raw = $item[$KeyName]
        if ($Numeric) { $k = ([long]$raw).ToString('D12', [System.Globalization.CultureInfo]::InvariantCulture) }
        elseif ($Lower) { $k = ([string]$raw).ToLowerInvariant() + [string][char]0 + [string]$raw }
        else { $k = [string]$raw }
        $list.Add([pscustomobject]@{ K = $k; V = $item })
    }
    $list.Sort([System.Comparison[object]] { param($a, $b) [string]::CompareOrdinal($a.K, $b.K) })
    $out = New-Object 'System.Collections.Generic.List[object]'
    foreach ($e in $list) { $out.Add($e.V) }
    return , $out.ToArray()
}

# ---------------------------------------------------------------------------------------------------------------------------
# Canonical JSON (sorted keys by ordinal comparison, 2-space indent, LF, UTF-8 without BOM, one trailing LF, integers only)
# ---------------------------------------------------------------------------------------------------------------------------
function Write-Phase2JsonString {
    param([System.Text.StringBuilder]$Sb, [string]$Text)
    [void]$Sb.Append('"')
    for ($i = 0; $i -lt $Text.Length; $i++) {
        $c = $Text[$i]
        $code = [int]$c
        if ($c -eq [char]'"') { [void]$Sb.Append('\"') }
        elseif ($c -eq [char]'\') { [void]$Sb.Append('\\') }
        elseif ($code -eq 8) { [void]$Sb.Append('\b') }
        elseif ($code -eq 9) { [void]$Sb.Append('\t') }
        elseif ($code -eq 10) { [void]$Sb.Append('\n') }
        elseif ($code -eq 12) { [void]$Sb.Append('\f') }
        elseif ($code -eq 13) { [void]$Sb.Append('\r') }
        elseif ($code -lt 32) { [void]$Sb.Append('\u' + $code.ToString('x4')) }
        elseif ([char]::IsHighSurrogate($c)) {
            if ($i + 1 -ge $Text.Length -or -not [char]::IsLowSurrogate($Text[$i + 1])) { throw 'canonical JSON: lone high surrogate' }
            [void]$Sb.Append($c)
            $i++
            [void]$Sb.Append($Text[$i])
        }
        elseif ([char]::IsLowSurrogate($c)) { throw 'canonical JSON: lone low surrogate' }
        else { [void]$Sb.Append($c) }
    }
    [void]$Sb.Append('"')
}

function Write-Phase2JsonValue {
    param([System.Text.StringBuilder]$Sb, $Value, [int]$Level)
    $pad = '  ' * ($Level + 1)
    $padEnd = '  ' * $Level
    if ($null -eq $Value) { [void]$Sb.Append('null'); return }
    if ($Value -is [bool]) { [void]$Sb.Append($(if ($Value) { 'true' } else { 'false' })); return }
    if ($Value -is [string]) { Write-Phase2JsonString -Sb $Sb -Text $Value; return }
    if ($Value -is [int] -or $Value -is [long] -or $Value -is [uint32] -or $Value -is [int16] -or $Value -is [byte]) {
        [void]$Sb.Append(([long]$Value).ToString([System.Globalization.CultureInfo]::InvariantCulture)); return
    }
    if ($Value -is [System.Collections.IDictionary]) {
        $keys = New-Object 'System.Collections.Generic.List[string]'
        foreach ($k in $Value.Keys) { $keys.Add([string]$k) }
        $keys.Sort([System.StringComparer]::Ordinal)
        if ($keys.Count -eq 0) { [void]$Sb.Append('{}'); return }
        [void]$Sb.Append("{`n")
        for ($i = 0; $i -lt $keys.Count; $i++) {
            [void]$Sb.Append($pad)
            Write-Phase2JsonString -Sb $Sb -Text $keys[$i]
            [void]$Sb.Append(': ')
            Write-Phase2JsonValue -Sb $Sb -Value $Value[$keys[$i]] -Level ($Level + 1)
            if ($i -lt $keys.Count - 1) { [void]$Sb.Append(',') }
            [void]$Sb.Append("`n")
        }
        [void]$Sb.Append($padEnd + '}')
        return
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        $items = New-Object 'System.Collections.Generic.List[object]'
        foreach ($e in $Value) { $items.Add($e) }
        if ($items.Count -eq 0) { [void]$Sb.Append('[]'); return }
        [void]$Sb.Append("[`n")
        for ($i = 0; $i -lt $items.Count; $i++) {
            [void]$Sb.Append($pad)
            Write-Phase2JsonValue -Sb $Sb -Value $items[$i] -Level ($Level + 1)
            if ($i -lt $items.Count - 1) { [void]$Sb.Append(',') }
            [void]$Sb.Append("`n")
        }
        [void]$Sb.Append($padEnd + ']')
        return
    }
    throw 'canonical JSON: unsupported value type'
}

function ConvertTo-Phase2CanonicalJson {
    param($Value)
    $sb = New-Object System.Text.StringBuilder
    Write-Phase2JsonValue -Sb $sb -Value $Value -Level 0
    [void]$sb.Append("`n")
    return $sb.ToString()
}

function ConvertTo-Phase2Utf8Bytes {
    param([string]$Text)
    return , ((New-Object System.Text.UTF8Encoding($false)).GetBytes($Text))
}

# ---------------------------------------------------------------------------------------------------------------------------
# JSON parser: strict (no comments, no trailing commas, no duplicate keys, integers only); objects become ordinal Hashtables.
# ---------------------------------------------------------------------------------------------------------------------------
function ConvertFrom-Phase2JsonElement {
    param([System.Text.Json.JsonElement]$Element)
    switch ($Element.ValueKind.ToString()) {
        'Object' {
            $h = New-Object System.Collections.Hashtable ([System.StringComparer]::Ordinal)
            foreach ($p in $Element.EnumerateObject()) {
                if ($h.ContainsKey($p.Name)) { throw ('duplicate JSON key: ' + $p.Name) }
                $h[$p.Name] = ConvertFrom-Phase2JsonElement -Element $p.Value
            }
            return $h
        }
        'Array' {
            $list = New-Object 'System.Collections.Generic.List[object]'
            foreach ($e in $Element.EnumerateArray()) { $list.Add((ConvertFrom-Phase2JsonElement -Element $e)) }
            return , $list.ToArray()
        }
        'String' { return $Element.GetString() }
        'Number' {
            $l = [long]0
            if ($script:Phase2JsonLenientNumbers -eq $true) {
                # only for reading a file the tool does not own (the designation): a non-integer number is kept as its raw text
                if ($Element.TryGetInt64([ref]$l) -and $Element.GetRawText() -notmatch '[.eE]') { return $l }
                return [string]$Element.GetRawText()
            }
            if (-not $Element.TryGetInt64([ref]$l)) { throw 'JSON number is not an int64 (only integers are allowed)' }
            if ($Element.GetRawText() -match '[.eE]') { throw 'JSON number is not written as a plain integer' }
            return $l
        }
        'True' { return $true }
        'False' { return $false }
        'Null' { return $null }
        default { throw ('unsupported JSON value kind ' + $Element.ValueKind) }
    }
}

function ConvertFrom-Phase2Json {
    param([string]$Text)
    $opts = New-Object System.Text.Json.JsonDocumentOptions
    $opts.MaxDepth = 64
    $doc = [System.Text.Json.JsonDocument]::Parse($Text, $opts)
    try { return , (ConvertFrom-Phase2JsonElement -Element $doc.RootElement) } finally { $doc.Dispose() }
}

# Strict UTF-8 decode that rejects a BOM and CR; returns the text.
function ConvertFrom-Phase2CanonicalBytes {
    param([byte[]]$Bytes)
    if ($Bytes.Length -ge 3 -and $Bytes[0] -eq 0xEF -and $Bytes[1] -eq 0xBB -and $Bytes[2] -eq 0xBF) { throw 'UTF-8 BOM present' }
    $text = (New-Object System.Text.UTF8Encoding($false, $true)).GetString($Bytes)
    if ($text.Contains("`r")) { throw 'CR present (LF only)' }
    return $text
}

# ---------------------------------------------------------------------------------------------------------------------------
# Closed JSON Schema subset validator. Unknown keywords are an error (the subset is closed on purpose).
# ---------------------------------------------------------------------------------------------------------------------------
$script:Phase2SchemaKeywords = @('$schema', '$id', '$defs', '$ref', 'title', 'description', '$comment', 'type', 'enum', 'const', 'pattern',
    'required', 'properties', 'additionalProperties', 'items', 'minItems', 'maxItems', 'minLength', 'maxLength', 'minimum', 'maximum')

function Test-Phase2JsonType {
    param($Value, [string]$Type)
    switch ($Type) {
        'object' { return ($Value -is [System.Collections.IDictionary]) }
        'array' { return ($null -ne $Value -and $Value -is [System.Array]) }
        'string' { return ($Value -is [string]) }
        'boolean' { return ($Value -is [bool]) }
        'integer' { return ($Value -is [long] -or $Value -is [int]) }
        'null' { return ($null -eq $Value) }
        default { throw ('schema: unsupported type ' + $Type) }
    }
}

function Test-Phase2JsonEqual {
    param($A, $B)
    if ($null -eq $A -or $null -eq $B) { return ($null -eq $A -and $null -eq $B) }
    if ($A -is [bool] -or $B -is [bool]) { return ($A -is [bool] -and $B -is [bool] -and $A -eq $B) }
    if ($A -is [string] -or $B -is [string]) { return ($A -is [string] -and $B -is [string] -and [string]::Equals($A, $B, [System.StringComparison]::Ordinal)) }
    return ([long]$A -eq [long]$B)
}

function Test-Phase2Schema {
    param($Value, $Schema, $Root, [string]$At, [System.Collections.Generic.List[string]]$Errors)
    foreach ($k in $Schema.Keys) {
        if ($script:Phase2SchemaKeywords -cnotcontains $k) { $Errors.Add($At + ': schema uses unsupported keyword ' + $k); return }
    }
    if ($Schema.ContainsKey('$ref')) {
        $ref = [string]$Schema['$ref']
        if ($ref -notmatch '^#/\$defs/([A-Za-z0-9_]+)$') { $Errors.Add($At + ': unsupported $ref ' + $ref); return }
        $target = $Root['$defs'][$Matches[1]]
        if ($null -eq $target) { $Errors.Add($At + ': unresolved $ref ' + $ref); return }
        Test-Phase2Schema -Value $Value -Schema $target -Root $Root -At $At -Errors $Errors
    }
    if ($Schema.ContainsKey('type')) {
        $types = @($Schema['type'])
        $ok = $false
        foreach ($t in $types) { if (Test-Phase2JsonType -Value $Value -Type ([string]$t)) { $ok = $true; break } }
        if (-not $ok) { $Errors.Add($At + ': type is not ' + ($types -join '|')); return }
    }
    if ($Schema.ContainsKey('const')) {
        if (-not (Test-Phase2JsonEqual -A $Value -B $Schema['const'])) { $Errors.Add($At + ': value differs from const') }
    }
    if ($Schema.ContainsKey('enum')) {
        $found = $false
        foreach ($e in $Schema['enum']) { if (Test-Phase2JsonEqual -A $Value -B $e) { $found = $true; break } }
        if (-not $found) { $Errors.Add($At + ': value is not in the enum') }
    }
    if ($Value -is [string]) {
        if ($Schema.ContainsKey('pattern') -and -not [regex]::IsMatch($Value, [string]$Schema['pattern'], [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)) { $Errors.Add($At + ': does not match pattern ' + $Schema['pattern']) }
        if ($Schema.ContainsKey('minLength') -and $Value.Length -lt [long]$Schema['minLength']) { $Errors.Add($At + ': shorter than minLength') }
        if ($Schema.ContainsKey('maxLength') -and $Value.Length -gt [long]$Schema['maxLength']) { $Errors.Add($At + ': longer than maxLength') }
    }
    if (($Value -is [long] -or $Value -is [int]) -and $Value -isnot [bool]) {
        if ($Schema.ContainsKey('minimum') -and [long]$Value -lt [long]$Schema['minimum']) { $Errors.Add($At + ': below minimum') }
        if ($Schema.ContainsKey('maximum') -and [long]$Value -gt [long]$Schema['maximum']) { $Errors.Add($At + ': above maximum') }
    }
    if ($Value -is [System.Collections.IDictionary]) {
        $props = @{}
        if ($Schema.ContainsKey('properties')) { $props = $Schema['properties'] }
        if ($Schema.ContainsKey('required')) {
            foreach ($r in $Schema['required']) { if (-not $Value.Contains($r)) { $Errors.Add($At + ': missing required property ' + $r) } }
        }
        foreach ($k in $Value.Keys) {
            if ($props.ContainsKey($k)) {
                Test-Phase2Schema -Value $Value[$k] -Schema $props[$k] -Root $Root -At ($At + '/' + $k) -Errors $Errors
            } elseif ($Schema.ContainsKey('additionalProperties') -and $Schema['additionalProperties'] -eq $false) {
                $Errors.Add($At + ': additional property ' + $k + ' is not allowed')
            }
        }
    }
    if ($null -ne $Value -and $Value -is [System.Array]) {
        if ($Schema.ContainsKey('minItems') -and $Value.Count -lt [long]$Schema['minItems']) { $Errors.Add($At + ': fewer than minItems') }
        if ($Schema.ContainsKey('maxItems') -and $Value.Count -gt [long]$Schema['maxItems']) { $Errors.Add($At + ': more than maxItems') }
        if ($Schema.ContainsKey('items')) {
            for ($i = 0; $i -lt $Value.Count; $i++) {
                Test-Phase2Schema -Value $Value[$i] -Schema $Schema['items'] -Root $Root -At ($At + '/' + $i) -Errors $Errors
            }
        }
    }
}

function Test-Phase2AgainstSchema {
    param($Value, $Schema)
    $errors = New-Object 'System.Collections.Generic.List[string]'
    Test-Phase2Schema -Value $Value -Schema $Schema -Root $Schema -At '#' -Errors $errors
    return , $errors.ToArray()
}

# ---------------------------------------------------------------------------------------------------------------------------
# Paths
# ---------------------------------------------------------------------------------------------------------------------------
# Accepts only a local drive-letter absolute path: no UNC, no device paths, no relative, no '..', no alternate data streams.
function Assert-Phase2LocalPath {
    param([string]$Path, [string]$What)
    if ([string]::IsNullOrWhiteSpace($Path)) { Stop-Phase2Refusal ($What + ' is empty') }
    if ($Path.Length -gt 240) { Stop-Phase2Refusal ($What + ' is longer than 240 characters') }
    if ($Path.StartsWith('\\') -or $Path.StartsWith('//')) { Stop-Phase2Refusal ($What + ' is a UNC or device path') }
    if ($Path -match '[\x00-\x1f*?"<>|]') { Stop-Phase2Refusal ($What + ' contains a forbidden character') }
    if ($Path -notmatch '^[A-Za-z]:[\\/]') { Stop-Phase2Refusal ($What + ' is not a local drive-letter absolute path') }
    $rest = $Path.Substring(2)
    if ($rest.Contains(':')) { Stop-Phase2Refusal ($What + ' contains a stream or drive separator') }
    if ($rest -match '[\\/]{2,}') { Stop-Phase2Refusal ($What + ' contains an empty path segment') }
    foreach ($seg in ($rest.Trim('\', '/') -split '[\\/]')) {
        if ($seg -eq '.' -or $seg -eq '..') { Stop-Phase2Refusal ($What + ' contains a dot segment') }
        if ($seg.EndsWith('.') -or $seg.EndsWith(' ')) { Stop-Phase2Refusal ($What + ' contains a segment ending in a dot or space') }
    }
    $full = [System.IO.Path]::GetFullPath($Path)
    if ($full.Length -gt 3) { $full = $full.TrimEnd('\') }
    $drive = $null
    try { $drive = New-Object System.IO.DriveInfo ($full.Substring(0, 1)) } catch { Stop-Phase2Refusal ($What + ': drive not readable') }
    if ($drive.DriveType -ne [System.IO.DriveType]::Fixed) { Stop-Phase2Refusal ($What + ' is not on a local fixed drive') }
    return $full
}

# Round 4: the evidence root must be a DIRECT child of the parent folder the CAD manager names (-AllowedEvidenceParent): an existing local
# fixed-drive folder without reparse points. The run-id leaf rule is checked by Assert-Phase2EvidenceRoot.
function Assert-Phase2AllowedEvidenceParent {
    param([string]$RootFullPath, [string]$ParentPath)
    $parent = Assert-Phase2LocalPath -Path $ParentPath -What '-AllowedEvidenceParent'
    if (-not [System.IO.Directory]::Exists($parent)) { Stop-Phase2Refusal '-AllowedEvidenceParent does not exist or is not a directory' }
    Assert-Phase2NoReparseChain -FullPath $parent -What '-AllowedEvidenceParent'
    $actual = [System.IO.Path]::GetDirectoryName([System.IO.Path]::GetFullPath($RootFullPath))
    if ($null -eq $actual -or -not [string]::Equals($actual.TrimEnd([char]92), $parent.TrimEnd([char]92), [System.StringComparison]::OrdinalIgnoreCase)) {
        Stop-Phase2Refusal 'EvidenceRoot is not a direct child of -AllowedEvidenceParent'
    }
    return $parent
}

# Refuses if the path or any ancestor (below the drive root) is a reparse point (junction, symbolic link, mount point, placeholder).
function Assert-Phase2NoReparseChain {
    param([string]$FullPath, [string]$What)
    $cur = $FullPath
    while ($null -ne $cur -and $cur.Length -gt 3) {
        $attr = $null
        try { $attr = [System.IO.File]::GetAttributes($cur) } catch { Stop-Phase2Refusal ($What + ': cannot read attributes of ' + $cur) }
        if (($attr -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { Stop-Phase2Refusal ($What + ': reparse point at ' + $cur) }
        $cur = [System.IO.Path]::GetDirectoryName($cur)
    }
}

function Assert-Phase2EvidenceRoot {
    param([string]$Path, [string]$RunId)
    $full = Assert-Phase2LocalPath -Path $Path -What 'EvidenceRoot'
    if (-not [System.IO.Directory]::Exists($full)) { Stop-Phase2Refusal 'EvidenceRoot does not exist or is not a directory (the script creates no folder)' }
    Assert-Phase2NoReparseChain -FullPath $full -What 'EvidenceRoot'
    if ($RunId -and -not [string]::Equals([System.IO.Path]::GetFileName($full), $RunId, [System.StringComparison]::Ordinal)) {
        Stop-Phase2Refusal 'the last component of EvidenceRoot must equal the run id (a fresh child root per run)'
    }
    return $full
}

function Assert-Phase2InputFile {
    param([string]$Path, [string]$What)
    $full = Assert-Phase2LocalPath -Path $Path -What $What
    if (-not [System.IO.File]::Exists($full)) { Stop-Phase2Refusal ($What + ' does not exist or is not a file') }
    Assert-Phase2NoReparseChain -FullPath $full -What $What
    return $full
}

# ---------------------------------------------------------------------------------------------------------------------------
# THE SINGLE WRITER. The only function of this folder that creates a file. Create-new, flat name, UTF-8 bytes given by the caller,
# inside a validated local evidence root; refuses an existing target of any kind. Returns the SHA-256 of the bytes written.
# ---------------------------------------------------------------------------------------------------------------------------
function Write-Phase2NewFile {
    param([string]$RootFullPath, [string]$Name, [byte[]]$Bytes)
    if ($Name -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$' -or $Name.Contains('..')) { Stop-Phase2Refusal ('bad evidence file name: ' + $Name) }
    $root = Assert-Phase2LocalPath -Path $RootFullPath -What 'EvidenceRoot'
    if (-not [System.IO.Directory]::Exists($root)) { Stop-Phase2Refusal 'EvidenceRoot does not exist' }
    Assert-Phase2NoReparseChain -FullPath $root -What 'EvidenceRoot'
    $target = [System.IO.Path]::Combine($root, $Name)
    if (-not [string]::Equals([System.IO.Path]::GetDirectoryName([System.IO.Path]::GetFullPath($target)), $root, [System.StringComparison]::OrdinalIgnoreCase)) {
        Stop-Phase2Refusal 'target is outside the evidence root'
    }
    # The writer pins its own root (round 2, reviewer m1): the root must be a run-id child folder, whoever calls the writer.
    if (-not ([System.IO.Path]::GetFileName($root) -cmatch $script:Phase2RunIdPattern)) { Stop-Phase2Refusal 'the evidence root leaf is not a run id (HGP-H<n>-<yyyymmddThhmmssZ>-<nn>)' }
    if ([System.IO.File]::Exists($target) -or [System.IO.Directory]::Exists($target)) { Stop-Phase2Refusal ('refusing to overwrite an existing entry: ' + $Name) }
    $stream = $null
    try {
        $stream = New-Object System.IO.FileStream($target, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    } catch [System.IO.IOException] {
        Stop-Phase2Refusal ('refusing to overwrite an existing entry (create-new failed): ' + $Name)
    }
    try {
        $stream.Write($Bytes, 0, $Bytes.Length)
        $stream.Flush($true)
    } finally { $stream.Dispose() }
    if (([System.IO.File]::GetAttributes($target) -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { Stop-Phase2Refusal ('written entry is a reparse point: ' + $Name) }
    # The reparse chain of the root is checked again AFTER the write (reviewer m2): the check before the write and the create-new are not
    # atomic, so a directory swapped for a junction in between is detected here (detection, not prevention; README section 7).
    Assert-Phase2NoReparseChain -FullPath $root -What 'EvidenceRoot (after the write)'
    return (Get-Phase2BytesSha256 -Bytes $Bytes)
}

# The ONLY caller of the writer (pinned by the static scan: exactly these two call texts, inside this function). Writes the record, then
# its .sha256 sidecar, both flat names derived from the run id, into a root whose leaf equals the run id. A failure of the SECOND write
# leaves the record on disk without a sidecar: it is returned as Partial (nothing is deleted); the caller exits 4.
function Write-Phase2RecordAndSidecar {
    param([string]$RootFullPath, [string]$RunId, [byte[]]$RecordBytes)
    if ($RunId -cnotmatch $script:Phase2RunIdPattern) { Stop-Phase2Refusal 'run id is not well formed' }
    if (-not [string]::Equals([System.IO.Path]::GetFileName($RootFullPath.TrimEnd('\')), $RunId, [System.StringComparison]::Ordinal)) { Stop-Phase2Refusal 'the last component of the evidence root must equal the run id' }
    $recordName = 'phase2-record-' + $RunId + '.json'
    $sidecarName = $recordName + '.sha256'
    $recordSha = Write-Phase2NewFile -RootFullPath $RootFullPath -Name $recordName -Bytes $RecordBytes
    $sidecarBytes = ConvertTo-Phase2Utf8Bytes -Text ($recordSha + '  ' + $recordName + "`n")
    $partial = $false; $errorText = $null
    try { [void](Write-Phase2NewFile -RootFullPath $RootFullPath -Name $sidecarName -Bytes $sidecarBytes) }
    catch { $partial = $true; $errorText = $_.Exception.Message }
    return [pscustomobject]@{ RecordName = $recordName; SidecarName = $sidecarName; RecordSha = $recordSha; Partial = $partial; Error = $errorText }
}

# Round 2 (review 2, m1): the evidence root must be empty when the capture starts (package v3 3.7 step 1 / 3.8). Lists with -Force so
# hidden entries count. The caller has already checked that the two output names are free (that refusal has its own message).
function Assert-Phase2EvidenceRootEmpty {
    param([string]$FullPath)
    $names = New-Object 'System.Collections.Generic.List[string]'
    foreach ($item in (Get-ChildItem -LiteralPath $FullPath -Force)) { $names.Add($item.Name) }
    if ($names.Count -gt 0) { Stop-Phase2Refusal ('EvidenceRoot is not empty (' + $names.Count + ' entries, first: ' + $names[0] + '); a fresh empty child root per run is required') }
}

# ---------------------------------------------------------------------------------------------------------------------------
# Transcript (read-only). The script never runs NETLOAD (or any load); it only reads a text file the CAD manager provides.
# ---------------------------------------------------------------------------------------------------------------------------
function Get-Phase2LoadLines {
    param([string[]]$Lines)
    # Every command word or AutoLISP form that loads or launches code, in the forms AutoCAD echoes (reviewer reproduction, round 2): a bare
    # word, the international prefix "_", the dot "._" / "_." forms, the dash "-" form (core words only), "^C^C_netload" menu macros, "(command ""_netload"")".
    # The word is bounded by "not a letter or digit" on both sides (the regex word-boundary escape is NOT used: "_" is a word character, so it missed "_NETLOAD").
    # A CLOSED LIST of known commands: it is a detection aid for the CAD manager, not proof that no code was loaded (README section 7).
    $coreWords = 'NETLOAD|APPLOAD|ARXLOAD|ARXUNLOAD|DBXLOAD|VLLOAD|FASLOAD|VLRUN|VBALOAD|VBARUN|STARTAPP'
    $scriptWords = 'RUNSCRIPT|RSCRIPT|SCRIPT'
    $lispForms = 'load|arxload|autoload|vl-load-all|vl-vbaload|vl-vbarun|startapp'
    $pattern = '(?<![A-Za-z0-9])(?:' + $coreWords + ')(?![A-Za-z0-9])' +
        '|(?:(?<=[_."])|(?<=^\s*)|(?<=Command:\s*))(?:' + $scriptWords + ')(?![A-Za-z0-9])' +
        '|\(\s*(?:' + $lispForms + ')(?![A-Za-z0-9])'
    $out = New-Object 'System.Collections.Generic.List[object]'
    for ($i = 0; $i -lt $Lines.Count; $i++) {
        if ([regex]::IsMatch($Lines[$i], $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
            $out.Add([ordered]@{ line = ($i + 1); text = $Lines[$i] })
        }
    }
    return , $out.ToArray()
}

function Read-Phase2Transcript {
    param([string]$FullPath)
    $bytes = $null
    try { $bytes = Read-Phase2FileBytes -Path $FullPath -MaxBytes 33554432 } catch { Stop-Phase2Refusal ('transcript cannot be read: ' + $_.Exception.Message) }
    $encName = $null
    $text = $null
    if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        $encName = 'utf-16le'; $text = [System.Text.Encoding]::Unicode.GetString($bytes, 2, $bytes.Length - 2)
    } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        $encName = 'utf-16be'; $text = [System.Text.Encoding]::BigEndianUnicode.GetString($bytes, 2, $bytes.Length - 2)
    } elseif ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $encName = 'utf-8-bom'; $text = (New-Object System.Text.UTF8Encoding($false, $true)).GetString($bytes, 3, $bytes.Length - 3)
    } else {
        try { $text = (New-Object System.Text.UTF8Encoding($false, $true)).GetString($bytes); $encName = 'utf-8' }
        catch { $text = [System.Text.Encoding]::GetEncoding(28591).GetString($bytes); $encName = 'iso-8859-1' }
    }
    $lines = New-Object 'System.Collections.Generic.List[string]'
    if ($text.Length -gt 0) {
        $parts = [regex]::Split($text, '\r\n|\n|\r')
        $n = $parts.Length
        if ($parts[$n - 1].Length -eq 0) { $n-- }
        for ($i = 0; $i -lt $n; $i++) { $lines.Add($parts[$i]) }
    }
    return [ordered]@{
        sha256       = (Get-Phase2BytesSha256 -Bytes $bytes)
        encoding     = $encName
        lineCount    = $lines.Count
        loadLines    = (Get-Phase2LoadLines -Lines $lines.ToArray())
    }
}

# ---------------------------------------------------------------------------------------------------------------------------
# Declared set (declared-set.json), pins, folder
# ---------------------------------------------------------------------------------------------------------------------------
function Read-Phase2DeclaredSet {
    param([string]$FullPath)
    $res = [ordered]@{ status = 'UNREADABLE'; sha256 = $null; reason = $null; entries = @() }
    $bytes = $null
    try { $bytes = Read-Phase2FileBytes -Path $FullPath -MaxBytes 1048576 } catch { $res.reason = 'cannot read: ' + $_.Exception.Message; return $res }
    $res.sha256 = Get-Phase2BytesSha256 -Bytes $bytes
    $res.status = 'MALFORMED'
    try {
        $text = ConvertFrom-Phase2CanonicalBytes -Bytes $bytes
        $data = ConvertFrom-Phase2Json -Text $text
    } catch { $res.reason = 'not strict JSON: ' + $_.Exception.Message; return $res }
    if ($null -eq $data -or $data -isnot [System.Array] -or $data.Count -eq 0) { $res.reason = 'not a non-empty array'; return $res }
    $seen = New-Object 'System.Collections.Generic.HashSet[string]'
    $entries = New-Object 'System.Collections.Generic.List[object]'
    foreach ($e in $data) {
        if ($e -isnot [System.Collections.IDictionary] -or $e.Count -ne 2 -or -not $e.Contains('path') -or -not $e.Contains('sha256')) { $res.reason = 'entry is not exactly {path, sha256}'; return $res }
        if ($e['path'] -isnot [string] -or $e['sha256'] -isnot [string]) { $res.reason = 'entry fields are not strings'; return $res }
        if ($e['sha256'] -cnotmatch '^[0-9a-f]{64}$') { $res.reason = 'entry sha256 is not 64 lowercase hex'; return $res }
        if ($e['path'] -cnotmatch '^[A-Za-z]:\\[^\\/:*?"<>|\x00-\x1f]+(\\[^\\/:*?"<>|\x00-\x1f]+)*\.dll$') { $res.reason = 'entry path is not an absolute drive-letter .dll path'; return $res }
        if (-not $seen.Add(([string]$e['path']).ToLowerInvariant())) { $res.reason = 'duplicate path'; return $res }
        $entries.Add([ordered]@{ path = [string]$e['path']; sha256 = [string]$e['sha256'] })
    }
    $res.entries = Sort-Phase2ByKey -Items $entries.ToArray() -KeyName 'path' -Lower
    $res.status = 'OK'
    return $res
}

function Read-Phase2PinFile {
    param([string]$Path)
    if (-not [System.IO.File]::Exists($Path)) { return [ordered]@{ status = 'MISSING'; value = $null } }
    $bytes = $null
    try { $bytes = Read-Phase2FileBytes -Path $Path -MaxBytes 1024 } catch { return [ordered]@{ status = 'UNREADABLE'; value = $null } }
    if ($bytes.Length -ne 65 -or $bytes[64] -ne 10) { return [ordered]@{ status = 'FORMAT_INVALID'; value = $null } }
    $s = [System.Text.Encoding]::ASCII.GetString($bytes, 0, 64)
    if ($s -cnotmatch '^[0-9a-f]{64}$') { return [ordered]@{ status = 'FORMAT_INVALID'; value = $null } }
    return [ordered]@{ status = 'OK'; value = $s }
}

# True when neither the path nor any ancestor below the drive root is a reparse point (and all attributes are readable).
function Test-Phase2ReparseFree {
    param([string]$FullPath)
    try { Assert-Phase2NoReparseChain -FullPath $FullPath -What 'path' } catch { return $false }
    return $true
}

function Get-Phase2PinCheck {
    param([string]$DeclaredSetFullPath, $DeclaredSet)
    $entries = New-Object 'System.Collections.Generic.List[object]'
    $folder = $null
    $reason = $DeclaredSet.reason
    $folders = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($e in $DeclaredSet.entries) {
        [void]$folders.Add(([System.IO.Path]::GetDirectoryName($e.path)).ToLowerInvariant())
        $fileStatus = 'OK'; $actual = $null
        # Round 2 (review 1 m5 / review 2): the declared-set paths get the same local-drive and reparse guards as the evidence root and
        # the input files. A network or removable drive letter is NOT_LOCAL; a reparse point (junction, symbolic link, cloud placeholder)
        # in the file or in any ancestor is REPARSE. Nothing is read from such a path.
        $localOk = $true
        try { [void](Assert-Phase2LocalPath -Path $e.path -What 'declared-set entry') } catch { $localOk = $false }
        $pin = [ordered]@{ status = 'UNREADABLE'; value = $null }
        if (-not $localOk) { $fileStatus = 'NOT_LOCAL' }
        elseif (-not [System.IO.File]::Exists($e.path)) { $fileStatus = 'MISSING'; $pin = Read-Phase2PinFile -Path ($e.path + '.pin') }
        elseif (-not (Test-Phase2ReparseFree -FullPath $e.path)) { $fileStatus = 'REPARSE' }
        else {
            $actual = Get-Phase2FileSha256 -Path $e.path
            if ($null -eq $actual) { $fileStatus = 'UNREADABLE' }
            $pin = Read-Phase2PinFile -Path ($e.path + '.pin')
        }
        $entries.Add([ordered]@{
            path = $e.path; declaredSha256 = $e.sha256; fileStatus = $fileStatus; actualSha256 = $actual
            pinPath = ($e.path + '.pin'); pinStatus = $pin.status; pinValue = $pin.value
        })
    }
    $listing = New-Object 'System.Collections.Generic.List[object]'
    $unexpected = New-Object 'System.Collections.Generic.List[string]'
    $missing = New-Object 'System.Collections.Generic.List[string]'
    $folderStatus = 'NOT_DETERMINED'
    if ($DeclaredSet.status -eq 'OK' -and $folders.Count -eq 1) {
        $folder = [System.IO.Path]::GetDirectoryName($DeclaredSet.entries[0].path)
        $folderStatus = 'OBSERVED'
        $folderLocalOk = $true
        try { [void](Assert-Phase2LocalPath -Path $folder -What 'declared-set folder') } catch { $folderLocalOk = $false }
        if ($folderLocalOk -and [System.IO.Directory]::Exists($folder) -and -not (Test-Phase2ReparseFree -FullPath $folder)) { $folderLocalOk = $false }
        if (-not $folderLocalOk) { $folderStatus = 'NOT_LOCAL_OR_REPARSE' }
        else { try {
            $expected = New-Object 'System.Collections.Generic.HashSet[string]'
            foreach ($e in $DeclaredSet.entries) {
                $leaf = [System.IO.Path]::GetFileName($e.path)
                [void]$expected.Add($leaf.ToLowerInvariant()); [void]$expected.Add(($leaf + '.pin').ToLowerInvariant())
            }
            $present = New-Object 'System.Collections.Generic.HashSet[string]'
            foreach ($item in (Get-ChildItem -LiteralPath $folder -Force)) {
                $isDir = $item.PSIsContainer
                $reparse = (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0)
                $size = $null; $sha = $null
                if (-not $isDir -and -not $reparse) { $size = [long]$item.Length; $sha = Get-Phase2FileSha256 -Path $item.FullName }
                $listing.Add([ordered]@{ name = $item.Name; kind = $(if ($isDir) { 'DIRECTORY' } else { 'FILE' }); size = $size; sha256 = $sha; reparse = $reparse })
                [void]$present.Add($item.Name.ToLowerInvariant())
                if (-not $expected.Contains($item.Name.ToLowerInvariant()) -or $isDir) { $unexpected.Add($item.Name) }
            }
            foreach ($x in $expected) { if (-not $present.Contains($x)) { $missing.Add($x) } }
        } catch { $folderStatus = 'UNREADABLE' } }
    }
    $listingSorted = Sort-Phase2ByKey -Items $listing.ToArray() -KeyName 'name' -Lower
    $unexpectedSorted = $unexpected.ToArray(); [System.Array]::Sort($unexpectedSorted, [System.StringComparer]::Ordinal)
    $missingSorted = $missing.ToArray(); [System.Array]::Sort($missingSorted, [System.StringComparer]::Ordinal)
    return [ordered]@{
        declaredSet = [ordered]@{
            path = $DeclaredSetFullPath; status = $DeclaredSet.status; sha256 = $DeclaredSet.sha256
            entryCount = @($DeclaredSet.entries).Count; folder = $folder; reason = $reason
        }
        entries     = $entries.ToArray()
        folder      = [ordered]@{ status = $folderStatus; listing = $listingSorted; unexpected = $unexpectedSorted; missing = $missingSorted }
    }
}

# ---------------------------------------------------------------------------------------------------------------------------
# Module baseline (the phase 1 module baseline of package v3 3.5.3 / P3 / C-03, hashed file by file) and its comparison
# ---------------------------------------------------------------------------------------------------------------------------
# File form: a strict JSON array of {path, sha256}; absolute drive-letter paths, unique after lower-casing. Any file extension.
function Read-Phase2ModuleBaseline {
    param([string]$FullPath)
    $res = [ordered]@{ status = 'UNREADABLE'; sha256 = $null; reason = $null; entries = @() }
    $bytes = $null
    try { $bytes = Read-Phase2FileBytes -Path $FullPath -MaxBytes 16777216 } catch { $res.reason = 'cannot read: ' + $_.Exception.Message; return $res }
    $res.sha256 = Get-Phase2BytesSha256 -Bytes $bytes
    $res.status = 'MALFORMED'
    try {
        $text = ConvertFrom-Phase2CanonicalBytes -Bytes $bytes
        $data = ConvertFrom-Phase2Json -Text $text
    } catch { $res.reason = 'not strict JSON: ' + $_.Exception.Message; return $res }
    if ($null -eq $data -or $data -isnot [System.Array] -or $data.Count -eq 0) { $res.reason = 'not a non-empty array'; return $res }
    $seen = New-Object 'System.Collections.Generic.HashSet[string]'
    $entries = New-Object 'System.Collections.Generic.List[object]'
    foreach ($e in $data) {
        if ($e -isnot [System.Collections.IDictionary] -or $e.Count -ne 2 -or -not $e.Contains('path') -or -not $e.Contains('sha256')) { $res.reason = 'entry is not exactly {path, sha256}'; return $res }
        if ($e['path'] -isnot [string] -or $e['sha256'] -isnot [string]) { $res.reason = 'entry fields are not strings'; return $res }
        if ($e['sha256'] -cnotmatch '^[0-9a-f]{64}$') { $res.reason = 'entry sha256 is not 64 lowercase hex'; return $res }
        if ($e['path'] -cnotmatch '^[A-Za-z]:\\[^\\/:*?"<>|\x00-\x1f]+(\\[^\\/:*?"<>|\x00-\x1f]+)*$') { $res.reason = 'entry path is not an absolute drive-letter path'; return $res }
        if (-not $seen.Add(([string]$e['path']).ToLowerInvariant())) { $res.reason = 'duplicate path'; return $res }
        $entries.Add([ordered]@{ path = [string]$e['path']; sha256 = [string]$e['sha256'] })
    }
    $res.entries = Sort-Phase2ByKey -Items $entries.ToArray() -KeyName 'path' -Lower
    $res.status = 'OK'
    return $res
}

# Modules loaded now that are not in the baseline (by lower-cased path) or whose hash differs. Returns the list in the module order.
function Get-Phase2ModulesOutsideBaseline {
    param($BaselineEntries, $ModuleItems)
    $map = New-Object System.Collections.Hashtable ([System.StringComparer]::Ordinal)
    foreach ($b in $BaselineEntries) { $map[([string]$b['path']).ToLowerInvariant()] = [string]$b['sha256'] }
    $outside = New-Object 'System.Collections.Generic.List[object]'
    foreach ($m in $ModuleItems) {
        $k = ([string]$m['path']).ToLowerInvariant()
        if (-not $map.ContainsKey($k)) { $outside.Add([ordered]@{ path = [string]$m['path']; sha256 = $m['sha256']; reason = 'NOT_IN_BASELINE' }) }
        elseif ($null -eq $m['sha256'] -or ([string]$m['sha256']) -cne [string]$map[$k]) { $outside.Add([ordered]@{ path = [string]$m['path']; sha256 = $m['sha256']; reason = 'HASH_DIFFERS' }) }
    }
    return , $outside.ToArray()
}

# ---------------------------------------------------------------------------------------------------------------------------
# Command line helpers (the executable token is removed first, so a switch-like text inside the executable path is never read)
# ---------------------------------------------------------------------------------------------------------------------------
function Get-Phase2CommandLineArguments {
    param([string]$CommandLine)
    $t = $CommandLine.TrimStart()
    if ($t.StartsWith('"')) {
        $q = $t.IndexOf('"', 1)
        if ($q -lt 0) { return '' }
        return $t.Substring($q + 1)
    }
    $m = [regex]::Match($t, '\s')
    if (-not $m.Success) { return '' }
    return $t.Substring($m.Index)
}

function Get-Phase2CommandLineSwitches {
    param([string]$Arguments)
    $list = New-Object 'System.Collections.Generic.List[string]'
    foreach ($m in [regex]::Matches($Arguments, '(?:^|\s)(/[A-Za-z][A-Za-z0-9]*)(?=\s|$)')) {
        $s = $m.Groups[1].Value.ToLowerInvariant()
        if (-not $list.Contains($s)) { $list.Add($s) }
    }
    $arr = $list.ToArray()
    [System.Array]::Sort($arr, [System.StringComparer]::Ordinal)
    return , $arr
}

# ---------------------------------------------------------------------------------------------------------------------------
# Process classification
# ---------------------------------------------------------------------------------------------------------------------------
function Get-Phase2RelatedReason {
    param([string]$Name, [string]$ExpectedName)
    if ([string]::Equals($Name, $ExpectedName, [System.StringComparison]::OrdinalIgnoreCase)) { return 'TARGET_NAME' }
    if ($Name -match '^(?i)acad') { return 'ACAD_PREFIX' }
    if ($Name -match '^(?i)(accoreconsole|acsignapply|acqmod|acwebbrowser.*|adsk.*|autodesk.*|autocad.*|adsso|adappmgr.*|genuineservice|adpclientservice)$') { return 'AUTODESK_RELATED' }
    return $null
}

# ---------------------------------------------------------------------------------------------------------------------------
# PURE checks over a record (no process access). Used by the capture to fill validation and by the offline validator.
# ---------------------------------------------------------------------------------------------------------------------------
function Get-Phase2Checks {
    param($Record)
    $checks = New-Object 'System.Collections.Generic.List[object]'
    $add = {
        param([string]$Name, [bool]$Ok, [string]$Detail)
        $checks.Add([ordered]@{ name = $Name; result = $(if ($Ok) { 'PASS' } else { 'FAIL' }); detail = $Detail })
    }
    $ti = $Record['tupleInputs']; $proc = $Record['process']; $build = $Record['acadBuild']; $prof = $Record['profile']
    $mods = $Record['modules']; $pl = $Record['processList']; $pin = $Record['pinCheck']; $tr = $Record['transcript']; $hook = $Record['testHook']
    $expectedName = [string]$ti['expectedProcessName']
    $ic = [System.StringComparison]::OrdinalIgnoreCase

    $nameOk = [string]::Equals([string]$proc['name'], $expectedName, $ic) -and ([string]::Equals($expectedName, 'acad', $ic) -or ($hook['active'] -eq $true -and [string]::Equals($expectedName, [string]$hook['processName'], [System.StringComparison]::Ordinal)))
    $add.Invoke('PROCESS_NAME_IS_EXPECTED', $nameOk, ('name=' + $proc['name'] + ' expected=' + $expectedName)) | Out-Null

    $same = @($pl['items'] | Where-Object { [string]::Equals([string]$_['name'], $expectedName, $ic) })
    $singleOk = ($pl['status'] -eq 'OBSERVED') -and ($same.Count -eq 1) -and ([long]$same[0]['pid'] -eq [long]$proc['pid'])
    $add.Invoke('SINGLE_TARGET_PROCESS', $singleOk, ('processesNamedExpected=' + $same.Count + ' targetPid=' + $proc['pid'])) | Out-Null

    $ownerOk = ($proc['ownerStatus'] -eq 'OBSERVED') -and ($null -ne $proc['owner']) -and [string]::Equals([string]$proc['owner'], [string]$proc['invokingUser'], $ic)
    $add.Invoke('PROCESS_OWNER_IS_INVOKING_USER', $ownerOk, ('ownerStatus=' + $proc['ownerStatus'])) | Out-Null

    $buildOk = ($build['status'] -eq 'OBSERVED') -and ($null -ne $build['path']) -and ($null -ne $build['sha256']) -and ($null -ne $build['fileVersion'])
    $add.Invoke('ACAD_BUILD_OBSERVED', $buildOk, ('status=' + $build['status'])) | Out-Null
    $shaOk = ($null -ne $build['sha256']) -and ($build['sha256'] -ceq $ti['acadExeSha256'])
    $add.Invoke('ACAD_SHA256_EQUALS_TUPLE', $shaOk, $(if ($shaOk) { 'equal' } else { 'differs or unreadable' })) | Out-Null

    $add.Invoke('PROFILE_EFFECTIVE_OBSERVED', ($null -ne $prof['effective'] -and $prof['effectiveSource'] -ne 'NONE'), ('source=' + $prof['effectiveSource'] + ' registry=' + $prof['registryStatus'] + ' commandLine=' + $prof['commandLineProfileStatus'])) | Out-Null
    $both = ($prof['registryStatus'] -eq 'OBSERVED') -and ($prof['commandLineProfileStatus'] -eq 'PRESENT')
    $consistent = $true
    if ($both) { $consistent = [string]::Equals([string]$prof['registryDefaultValue'], [string]$prof['commandLineProfile'], [System.StringComparison]::Ordinal) }
    if ($prof['commandLineProfileStatus'] -eq 'ARG_FILE') { $consistent = $false }
    $add.Invoke('PROFILE_SOURCES_CONSISTENT', $consistent, $(if ($both) { 'both sources observed' } else { 'single or no source' })) | Out-Null
    $profOk = ($null -ne $prof['effective']) -and [string]::Equals([string]$prof['effective'], [string]$ti['profileExpected'], [System.StringComparison]::Ordinal)
    $add.Invoke('PROFILE_EQUALS_EXPECTED', $profOk, $(if ($profOk) { 'equal' } else { 'differs or unobserved' })) | Out-Null

    $items = @($mods['items'])
    $enumOk = ($mods['status'] -eq 'OBSERVED') -and ($items.Count -gt 0) -and ([long]$mods['count'] -eq $items.Count)
    $add.Invoke('MODULES_ENUMERATED', $enumOk, ('status=' + $mods['status'] + ' count=' + $mods['count'] + ' items=' + $items.Count)) | Out-Null
    $unread = @($items | Where-Object { $_['hashStatus'] -ne 'OK' -or $null -eq $_['sha256'] })
    $add.Invoke('MODULE_HASHES_READABLE', ($unread.Count -eq 0), ('unreadable=' + $unread.Count)) | Out-Null
    $sorted = $true; $prev = $null
    foreach ($m in $items) {
        $k = ([string]$m['path']).ToLowerInvariant()
        if ($null -ne $prev -and [string]::CompareOrdinal($prev, $k) -ge 0) { $sorted = $false; break }
        $prev = $k
    }
    $add.Invoke('MODULE_INVENTORY_SORTED_UNIQUE', $sorted, 'ordinal order of lowercase path, no duplicates') | Out-Null
    $mainHit = @($items | Where-Object { $null -ne $build['path'] -and [string]::Equals([string]$_['path'], [string]$build['path'], $ic) })
    $mainOk = ($mainHit.Count -eq 1) -and ($null -ne $build['sha256']) -and ([string]$mainHit[0]['sha256'] -ceq [string]$build['sha256'])
    $add.Invoke('MAIN_MODULE_IN_INVENTORY', $mainOk, ('matches=' + $mainHit.Count)) | Out-Null

    $folder = $mods['declaredSetFolder']
    $subsetOk = $false; $subsetDetail = 'declared-set folder unknown'
    if ($null -ne $folder) {
        $prefix = ([string]$folder).TrimEnd('\') + '\'
        $under = @($items | Where-Object { ([string]$_['path']).StartsWith($prefix, $ic) })
        $recorded = @($mods['declaredSetSubset'])
        $subsetOk = ($under.Count -eq 0) -and ($recorded.Count -eq 0)
        $subsetDetail = 'modulesUnderDeclaredFolder=' + $under.Count + ' recordedSubset=' + $recorded.Count
    }
    $add.Invoke('NO_DECLARED_SET_MODULE_LOADED', $subsetOk, $subsetDetail) | Out-Null

    $plItems = @($pl['items'])
    $targetItems = @($plItems | Where-Object { [long]$_['pid'] -eq [long]$proc['pid'] })
    $plOk = ($pl['status'] -eq 'OBSERVED') -and ($plItems.Count -gt 0) -and ([long]$pl['count'] -eq $plItems.Count) -and ($targetItems.Count -eq 1) -and ([string]$targetItems[0]['startUtc'] -ceq [string]$proc['startUtc'])
    $add.Invoke('PROCESS_LIST_ENUMERATED', $plOk, ('status=' + $pl['status'] + ' count=' + $pl['count']) ) | Out-Null

    $ds = $pin['declaredSet']
    $add.Invoke('DECLARED_SET_JSON_WELL_FORMED', ($ds['status'] -eq 'OK' -and [long]$ds['entryCount'] -gt 0), ('status=' + $ds['status'] + ' entries=' + $ds['entryCount'])) | Out-Null
    $dsShaOk = ($null -ne $ds['sha256']) -and ($ds['sha256'] -ceq $ti['declaredSetSha256'])
    $add.Invoke('DECLARED_SET_SHA256_EQUALS_TUPLE', $dsShaOk, $(if ($dsShaOk) { 'equal' } else { 'differs or unreadable' })) | Out-Null
    $add.Invoke('DECLARED_SET_SINGLE_FOLDER', ($null -ne $ds['folder']), $(if ($null -ne $ds['folder']) { 'one folder' } else { 'no single folder' })) | Out-Null
    $fo = $pin['folder']
    $anyReparse = @(@($fo['listing']) | Where-Object { $_['reparse'] -eq $true -or $_['kind'] -ne 'FILE' })
    $folderOk = ($fo['status'] -eq 'OBSERVED') -and (@($fo['unexpected']).Count -eq 0) -and (@($fo['missing']).Count -eq 0) -and ($anyReparse.Count -eq 0)
    $add.Invoke('DECLARED_FOLDER_CONTENT_EXACT', $folderOk, ('status=' + $fo['status'] + ' unexpected=' + @($fo['unexpected']).Count + ' missing=' + @($fo['missing']).Count)) | Out-Null

    $entries = @($pin['entries'])
    $badDll = @($entries | Where-Object { $_['fileStatus'] -ne 'OK' -or $null -eq $_['actualSha256'] -or $_['actualSha256'] -cne $_['declaredSha256'] })
    $add.Invoke('DECLARED_DLL_HASHES_EQUAL_ENTRY', ($entries.Count -gt 0 -and $badDll.Count -eq 0), ('entries=' + $entries.Count + ' mismatchOrUnreadable=' + $badDll.Count)) | Out-Null
    $badPin = @($entries | Where-Object { $_['pinStatus'] -ne 'OK' -or $null -eq $_['pinValue'] -or $_['pinValue'] -cne $_['actualSha256'] -or $_['pinValue'] -cne $_['declaredSha256'] })
    $add.Invoke('PIN_FILES_EQUAL_DLL_AND_ENTRY', ($entries.Count -gt 0 -and $badPin.Count -eq 0), ('entries=' + $entries.Count + ' pinMismatchOrInvalid=' + $badPin.Count)) | Out-Null

    $provided = ($tr['status'] -eq 'PROVIDED')
    $trShape = $true
    if ($provided) { $trShape = ($null -ne $tr['path']) -and ($null -ne $tr['sha256']) -and ($null -ne $tr['lineCount']) -and ($null -ne $tr['encoding']) }
    else { $trShape = ($null -eq $tr['path']) -and ($null -eq $tr['sha256']) -and ($null -eq $tr['lineCount']) -and (@($tr['loadLines']).Count -eq 0) }
    $reqOk = $trShape
    if ($ti['requireTranscript'] -eq $true -and -not $provided) { $reqOk = $false }
    $add.Invoke('TRANSCRIPT_REQUIREMENT_MET', $reqOk, ('status=' + $tr['status'] + ' requirement=' + $tr['requirement'])) | Out-Null
    $expectedReq = $(if ($ti['requireTranscript'] -eq $true) { 'REQUIRED_NOW' } elseif ($Record['session'] -eq 'S1-A') { 'NOT_APPLICABLE_S1A_NO_LOAD_STEP' } else { 'REQUIRED_LATER' })
    $sessOk = ($tr['requirement'] -ceq $expectedReq)
    # Phase 2 is captured BEFORE any load step (package v3 3.7: step 3 precedes step 5), in every session: a load command or form
    # already present in the transcript means code may have been loaded before the attestation (ordering violation, R3-2).
    if (@($tr['loadLines']).Count -gt 0) { $sessOk = $false }
    $add.Invoke('TRANSCRIPT_CONSISTENT_WITH_SESSION', $sessOk, ('expectedRequirement=' + $expectedReq + ' loadLines=' + @($tr['loadLines']).Count)) | Out-Null

    # The session identifier is the CAD-manager-assigned one (phase 1); a placeholder or an empty value is not an identifier.
    $sid = $ti['sessionId']
    $sidOk = ($sid -is [string]) -and [regex]::IsMatch([string]$sid, $script:Phase2AssignedSessionIdPattern)
    $add.Invoke('SESSION_ID_ASSIGNED_WELL_FORMED', $sidOk, $(if ($sidOk) { 'assigned sessionId present and well formed' } else { 'assigned sessionId missing or not a well formed identifier' })) | Out-Null

    # Module baseline comparison (template 3.5.3: actualLoadedModules compared with the phase 1 baseline).
    $mb = $Record['moduleBaseline']
    $mbOk = $false; $mbDetail = ''
    if ($mb['status'] -eq 'PROVIDED') {
        $mbOk = ($mods['status'] -eq 'OBSERVED') -and ($null -ne $mb['sha256']) -and ($mb['sha256'] -ceq $ti['moduleBaselineSha256']) -and (@($mb['outsideBaseline']).Count -eq 0)
        $mbDetail = 'baseline entries=' + $mb['entryCount'] + ' modulesOutsideBaseline=' + @($mb['outsideBaseline']).Count + ' baselineHashEqualsTuple=' + ($null -ne $mb['sha256'] -and $mb['sha256'] -ceq $ti['moduleBaselineSha256'])
    } elseif ($mb['status'] -eq 'NOT_PROVIDED') {
        # Round 2 (review 2 M3): the S1-A exception is ENCODED. Only S1-A may run without a baseline (it produces the SESSION_START list
        # that becomes the baseline of the later sessions); every other session without a provided and compared baseline FAILS, whatever
        # the flags say. The CAD manager's switches can no longer make the comparison optional for S1-B .. S4.
        $mbOk = ([string]$Record['session'] -ceq 'S1-A') -and ($ti['requireModuleBaseline'] -ne $true) -and ($null -eq $ti['moduleBaselineSha256'])
        $mbDetail = 'NOT_PROVIDED session=' + $Record['session'] + ' requireModuleBaseline=' + $ti['requireModuleBaseline'] + ' (only S1-A may run without a baseline; no comparison was made; see coverage)'
    } else {
        $mbDetail = 'baseline status=' + $mb['status']
    }
    $add.Invoke('MODULE_BASELINE_COMPARISON', $mbOk, $mbDetail) | Out-Null

    return , $checks.ToArray()
}

# What the verdict does and does not cover (template 3.5.3 fields). Pure: derived only from the module baseline status.
function Get-Phase2Coverage {
    param([string]$ModuleBaselineStatus)
    return [ordered]@{
        verdictCovers            = 'EXTERNAL_OBSERVATIONS_OF_THIS_TOOL_ONLY'
        templateComplete         = $false
        moduleBaselineComparison = $(if ($ModuleBaselineStatus -eq 'PROVIDED') { 'COMPARED' } else { 'NOT_COMPARED' })
        sessionProfileFacts      = 'HOST-TO-CONFIRM'
        runtimeIdentities        = 'HOST-TO-CONFIRM'
        pinCheckAfterLoad     = 'HOST-TO-CONFIRM'
        humanFields              = 'CAD_MANAGER'
    }
}

function Get-Phase2Verdict {
    param($Checks, [bool]$HookActive)
    foreach ($c in $Checks) { if ($c['result'] -ne 'PASS') { return $script:Phase2VerdictInvalid } }
    if ($HookActive) { return $script:Phase2VerdictStandIn }
    return $script:Phase2VerdictOk
}

# ---------------------------------------------------------------------------------------------------------------------------
# Providers: the only code that touches the live system. All READ-ONLY. Tests inject fakes with the same shape.
# ---------------------------------------------------------------------------------------------------------------------------
function New-Phase2RealProviders {
    $p = @{}
    $p.UtcNow = { [System.DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ss.fffZ', [System.Globalization.CultureInfo]::InvariantCulture) }
    $p.InvokingUser = { [System.Security.Principal.WindowsIdentity]::GetCurrent().Name }
    $p.Process = {
        param($ProcId)
        $pr = $null
        try { $pr = [System.Diagnostics.Process]::GetProcessById([int]$ProcId) } catch { return $null }
        $start = $null
        try { $start = $pr.StartTime.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ', [System.Globalization.CultureInfo]::InvariantCulture) } catch { $start = $null }
        $ms = 'UNREADABLE'; $mp = $null; $fv = $null; $pv = $null
        try {
            $mm = $pr.MainModule
            $mp = $mm.FileName; $fv = $mm.FileVersionInfo.FileVersion; $pv = $mm.FileVersionInfo.ProductVersion; $ms = 'OBSERVED'
        } catch { $ms = 'UNREADABLE' }
        return [pscustomobject]@{ Pid = $pr.Id; Name = $pr.ProcessName; StartUtc = $start; MainStatus = $ms; MainPath = $mp; FileVersion = $fv; ProductVersion = $pv }
    }
    $p.Modules = {
        param($ProcId)
        try {
            $pr = [System.Diagnostics.Process]::GetProcessById([int]$ProcId)
            $list = New-Object 'System.Collections.Generic.List[object]'
            foreach ($m in $pr.Modules) {
                $v = $null
                try { $v = $m.FileVersionInfo.FileVersion } catch { $v = $null }
                $list.Add([pscustomobject]@{ Path = $m.FileName; FileVersion = $v })
            }
            return [pscustomobject]@{ Ok = $true; Items = $list.ToArray() }
        } catch {
            return [pscustomobject]@{ Ok = $false; Items = @() }
        }
    }
    $p.Processes = {
        try {
            $list = New-Object 'System.Collections.Generic.List[object]'
            foreach ($pr in [System.Diagnostics.Process]::GetProcesses()) {
                $s = $null
                try { $s = $pr.StartTime.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ', [System.Globalization.CultureInfo]::InvariantCulture) } catch { $s = $null }
                $list.Add([pscustomobject]@{ Pid = $pr.Id; Name = $pr.ProcessName; StartUtc = $s })
            }
            return [pscustomobject]@{ Ok = $true; Items = $list.ToArray() }
        } catch {
            return [pscustomobject]@{ Ok = $false; Items = @() }
        }
    }
    $p.CommandLine = {
        param($ProcId)
        try {
            $cim = Get-CimInstance -ClassName Win32_Process -Filter ('ProcessId=' + [int]$ProcId)
            if ($null -eq $cim -or $null -eq $cim.CommandLine) { return [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
            return [pscustomobject]@{ Status = 'OBSERVED'; Value = [string]$cim.CommandLine }
        } catch { return [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
    }
    $p.Owner = {
        param($ProcId)
        try {
            $cim = Get-CimInstance -ClassName Win32_Process -Filter ('ProcessId=' + [int]$ProcId)
            $o = Invoke-CimMethod -InputObject $cim -MethodName GetOwner
            if ($null -eq $o -or $o.ReturnValue -ne 0 -or [string]::IsNullOrEmpty($o.User)) { return [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
            return [pscustomobject]@{ Status = 'OBSERVED'; Value = ([string]$o.Domain + '\' + [string]$o.User) }
        } catch { return [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
    }
    $p.RegistryDefault = {
        param($SubKey)
        try {
            $k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($SubKey, $false)
            if ($null -eq $k) { return [pscustomobject]@{ Status = 'KEY_ABSENT'; Value = $null } }
            try {
                $v = $k.GetValue('', $null)
                if ($null -eq $v -or $v -isnot [string] -or $v.Length -eq 0) { return [pscustomobject]@{ Status = 'VALUE_ABSENT'; Value = $null } }
                return [pscustomobject]@{ Status = 'OBSERVED'; Value = $v }
            } finally { $k.Dispose() }
        } catch { return [pscustomobject]@{ Status = 'UNREADABLE'; Value = $null } }
    }
    # AT-REST value of one profile variable (SECURELOAD, TRUSTEDPATHS) as stored in the profile registry key. Read-only; informational.
    $p.ProfileVariable = {
        param($SubKey, $Name)
        try {
            $k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($SubKey, $false)
            if ($null -eq $k) { return [pscustomobject]@{ Status = 'KEY_ABSENT'; Kind = $null; Value = $null } }
            try {
                $v = $k.GetValue($Name, $null)
                if ($null -eq $v) { return [pscustomobject]@{ Status = 'VALUE_ABSENT'; Kind = $null; Value = $null } }
                $kind = [string]$k.GetValueKind($Name)
                return [pscustomobject]@{ Status = 'OBSERVED'; Kind = $kind; Value = [string]$v }
            } finally { $k.Dispose() }
        } catch { return [pscustomobject]@{ Status = 'UNREADABLE'; Kind = $null; Value = $null } }
    }
    $p.Signer = {
        param($Path)
        try {
            $s = Get-AuthenticodeSignature -LiteralPath $Path
            $subj = $null
            if ($null -ne $s.SignerCertificate) { $subj = [string]$s.SignerCertificate.Subject }
            return [pscustomobject]@{ Status = [string]$s.Status; Subject = $subj }
        } catch { return [pscustomobject]@{ Status = 'UNREADABLE'; Subject = $null } }
    }
    return $p
}

# ---------------------------------------------------------------------------------------------------------------------------
# The capture: builds the whole record (including validation) from the providers. Writes nothing.
# $Params: ProcessId, Session, SessionId (assigned by the CAD manager), RunId, EvidenceRoot (full), DeclaredSetJson (full),
#          TranscriptPath (full or $null), RequireTranscript, ModuleBaselineJson (full or $null), ExpectedModuleBaselineSha256 (or $null),
#          RequireModuleBaseline, ExpectedDeclaredSetSha256, ExpectedAcadSha256, ExpectedProfile, ProfilesSubKey, ExpectedProcessName,
#          TestHookName, ScriptSha256, LibrarySha256, SchemaSha256, PowerShellVersion
# ---------------------------------------------------------------------------------------------------------------------------
function Invoke-Phase2Capture {
    param([hashtable]$Params, [hashtable]$P)
    $procId = [int]$Params.ProcessId
    $proc = $P.Process.InvokeReturnAsIs($procId)
    if ($null -eq $proc) { Stop-Phase2Refusal ('no running process with id ' + $procId) }
    if ($null -eq $proc.StartUtc) { Stop-Phase2Refusal 'the process start time cannot be read; the session cannot be bound (fail-closed)' }
    $startUtc = [string]$proc.StartUtc
    $runId = [string]$Params.RunId
    $expectedName = [string]$Params.ExpectedProcessName
    $hookActive = -not [string]::IsNullOrEmpty([string]$Params.TestHookName)

    $cmd = $P.CommandLine.InvokeReturnAsIs($procId)
    $own = $P.Owner.InvokeReturnAsIs($procId)
    $invoking = [string]$P.InvokingUser.InvokeReturnAsIs()

    $build = [ordered]@{ status = 'UNREADABLE'; path = $null; sha256 = $null; fileVersion = $null; productVersion = $null }
    if ($proc.MainStatus -eq 'OBSERVED' -and -not [string]::IsNullOrEmpty([string]$proc.MainPath)) {
        $h = Get-Phase2FileSha256 -Path ([string]$proc.MainPath)
        if ($null -ne $h) {
            $build = [ordered]@{ status = 'OBSERVED'; path = [string]$proc.MainPath; sha256 = $h; fileVersion = $(if ($null -ne $proc.FileVersion) { [string]$proc.FileVersion } else { $null }); productVersion = $(if ($null -ne $proc.ProductVersion) { [string]$proc.ProductVersion } else { $null }) }
        } else { $build['path'] = [string]$proc.MainPath }
    }

    # Profile: command-line /p, then the registry default at capture time. Never guessed.
    $regRes = $P.RegistryDefault.InvokeReturnAsIs([string]$Params.ProfilesSubKey)
    $clStatus = 'NOT_OBSERVABLE'; $clProfile = $null
    $clArguments = ''
    $clSwitches = @()
    if ($cmd.Status -eq 'OBSERVED') {
        $clArguments = Get-Phase2CommandLineArguments -CommandLine ([string]$cmd.Value)
        $clSwitches = Get-Phase2CommandLineSwitches -Arguments $clArguments
        $m = [regex]::Match($clArguments, '(?i)(?:^|\s)/p\s+(?:"([^"]*)"|(\S+))')
        if (-not $m.Success) { $clStatus = 'NOT_PRESENT' }
        else {
            $v = $(if ($m.Groups[1].Success) { $m.Groups[1].Value } else { $m.Groups[2].Value })
            if ($v -match '(?i)\.arg$') { $clStatus = 'ARG_FILE' } else { $clStatus = 'PRESENT'; $clProfile = $v }
        }
    }
    $effective = $null; $source = 'NONE'
    if ($clStatus -eq 'PRESENT') { $effective = $clProfile; $source = 'COMMAND_LINE_P_SWITCH' }
    elseif ($clStatus -eq 'NOT_PRESENT' -and $regRes.Status -eq 'OBSERVED') { $effective = [string]$regRes.Value; $source = 'REGISTRY_DEFAULT_AT_CAPTURE' }

    # Modules
    $modRes = $P.Modules.InvokeReturnAsIs($procId)
    $modItems = New-Object 'System.Collections.Generic.List[object]'
    $signerItems = New-Object 'System.Collections.Generic.List[object]'
    if ($modRes.Ok) {
        foreach ($m in $modRes.Items) {
            $hash = Get-Phase2FileSha256 -Path ([string]$m.Path)
            $sig = $P.Signer.InvokeReturnAsIs([string]$m.Path)
            $modItems.Add([ordered]@{
                path = [string]$m.Path; sha256 = $hash; hashStatus = $(if ($null -ne $hash) { 'OK' } else { 'UNREADABLE' })
                fileVersion = $(if ($null -ne $m.FileVersion) { [string]$m.FileVersion } else { $null })
            })
            # Authenticode status can depend on revocation lookups, so it is NOT part of the deterministic body: it goes to volatile.
            $signerItems.Add([ordered]@{ path = [string]$m.Path; status = [string]$sig.Status; subject = $(if ($null -ne $sig.Subject) { [string]$sig.Subject } else { $null }) })
        }
    }
    $modSorted = Sort-Phase2ByKey -Items $modItems.ToArray() -KeyName 'path' -Lower
    $signersSorted = Sort-Phase2ByKey -Items $signerItems.ToArray() -KeyName 'path' -Lower

    # Module baseline (phase 1), compared file by file with the modules loaded now.
    $moduleBaseline = [ordered]@{ status = 'NOT_PROVIDED'; path = $null; sha256 = $null; entryCount = 0; reason = $null; outsideBaseline = @() }
    $expectedBaselineSha = $(if (-not [string]::IsNullOrEmpty([string]$Params.ExpectedModuleBaselineSha256)) { [string]$Params.ExpectedModuleBaselineSha256 } else { $null })
    if (-not [string]::IsNullOrEmpty([string]$Params.ModuleBaselineJson)) {
        $bl = Read-Phase2ModuleBaseline -FullPath ([string]$Params.ModuleBaselineJson)
        $outside = @()
        if ($bl.status -eq 'OK') { $outside = Get-Phase2ModulesOutsideBaseline -BaselineEntries $bl.entries -ModuleItems $modSorted }
        $moduleBaseline = [ordered]@{
            status = $(if ($bl.status -eq 'OK') { 'PROVIDED' } else { [string]$bl.status }); path = [string]$Params.ModuleBaselineJson; sha256 = $bl.sha256
            entryCount = @($bl.entries).Count; reason = $bl.reason; outsideBaseline = $outside
        }
    }

    # Declared set, pins, folder
    $ds = Read-Phase2DeclaredSet -FullPath ([string]$Params.DeclaredSetJson)
    $pinCheck = Get-Phase2PinCheck -DeclaredSetFullPath ([string]$Params.DeclaredSetJson) -DeclaredSet $ds
    $dsFolder = $pinCheck.declaredSet.folder
    $subset = New-Object 'System.Collections.Generic.List[object]'
    if ($null -ne $dsFolder) {
        $prefix = $dsFolder.TrimEnd('\') + '\'
        foreach ($m in $modSorted) { if (([string]$m['path']).StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) { $subset.Add([ordered]@{ path = $m['path']; sha256 = $m['sha256'] }) } }
    }

    # Process list
    $plRes = $P.Processes.InvokeReturnAsIs()
    $plItems = New-Object 'System.Collections.Generic.List[object]'
    $related = New-Object 'System.Collections.Generic.List[object]'
    if ($plRes.Ok) {
        foreach ($q in $plRes.Items) {
            $plItems.Add([ordered]@{ pid = [int]$q.Pid; name = [string]$q.Name; startUtc = $(if ($null -ne $q.StartUtc) { [string]$q.StartUtc } else { 'UNREADABLE' }) })
            $why = Get-Phase2RelatedReason -Name ([string]$q.Name) -ExpectedName $expectedName
            if ($null -ne $why) { $related.Add([ordered]@{ pid = [int]$q.Pid; name = [string]$q.Name; reason = $why }) }
        }
    }
    $plSorted = Sort-Phase2ByKey -Items $plItems.ToArray() -KeyName 'pid' -Numeric
    $relSorted = Sort-Phase2ByKey -Items $related.ToArray() -KeyName 'pid' -Numeric

    # Transcript
    $requirement = $(if ($Params.RequireTranscript) { 'REQUIRED_NOW' } elseif ($Params.Session -eq 'S1-A') { 'NOT_APPLICABLE_S1A_NO_LOAD_STEP' } else { 'REQUIRED_LATER' })
    $transcript = [ordered]@{ status = 'NOT_PROVIDED'; requirement = $requirement; path = $null; sha256 = $null; encoding = $null; lineCount = $null; loadLines = @() }
    if (-not [string]::IsNullOrEmpty([string]$Params.TranscriptPath)) {
        $t = Read-Phase2Transcript -FullPath ([string]$Params.TranscriptPath)
        $transcript = [ordered]@{ status = 'PROVIDED'; requirement = $requirement; path = [string]$Params.TranscriptPath; sha256 = $t.sha256; encoding = $t.encoding; lineCount = $t.lineCount; loadLines = $t.loadLines }
    }

    # At-rest profile variables (informational, read-only, in no check): the value stored in the profile registry key of the effective
    # profile. It is NOT the in-process value (that is the I-1 protocol A1-R5 / A1-R6 read).
    $variablesSubKey = $null
    $atRestSecureload = [ordered]@{ status = 'NOT_ATTEMPTED'; kind = $null; value = $null }
    $atRestTrusted = [ordered]@{ status = 'NOT_ATTEMPTED'; kind = $null; value = $null }
    if ($null -ne $effective -and $effective.Length -gt 0 -and $effective -notmatch '[\\/]') {
        $variablesSubKey = ([string]$Params.ProfilesSubKey).TrimEnd('\') + '\' + $effective + '\Variables'
        $sv = $P.ProfileVariable.InvokeReturnAsIs($variablesSubKey, 'SECURELOAD')
        $tv = $P.ProfileVariable.InvokeReturnAsIs($variablesSubKey, 'TRUSTEDPATHS')
        $atRestSecureload = [ordered]@{ status = [string]$sv.Status; kind = $(if ($null -ne $sv.Kind) { [string]$sv.Kind } else { $null }); value = $(if ($null -ne $sv.Value) { [string]$sv.Value } else { $null }) }
        $atRestTrusted = [ordered]@{ status = [string]$tv.Status; kind = $(if ($null -ne $tv.Kind) { [string]$tv.Kind } else { $null }); value = $(if ($null -ne $tv.Value) { [string]$tv.Value } else { $null }) }
    }

    # The process must still be the same one (same pid, same start time, same name) after all observations: a process that ended
    # or a reused pid would make the whole capture meaningless, so nothing is written.
    $again = $P.Process.InvokeReturnAsIs($procId)
    if ($null -eq $again -or [string]$again.StartUtc -cne $startUtc -or [string]$again.Name -cne [string]$proc.Name) {
        Stop-Phase2Refusal 'the process ended or changed during the capture (pid reuse); capture again'
    }

    # Review 2 m6: the time between the process start and this capture, for the CAD manager (the phrase "immediately after the session
    # starts" is otherwise not measured). Information only: volatile, in no check. Whole seconds; null when a time cannot be parsed.
    $capturedAt = [string]$P.UtcNow.InvokeReturnAsIs()
    $elapsedSeconds = $null
    try {
        $utcStyles = [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor [System.Globalization.DateTimeStyles]::AssumeUniversal
        $t0 = [System.DateTime]::ParseExact($startUtc, 'yyyy-MM-ddTHH:mm:ss.fffZ', [System.Globalization.CultureInfo]::InvariantCulture, $utcStyles)
        $t1 = [System.DateTime]::ParseExact($capturedAt, 'yyyy-MM-ddTHH:mm:ss.fffZ', [System.Globalization.CultureInfo]::InvariantCulture, $utcStyles)
        $elapsedSeconds = [long](($t1 - $t0).TotalSeconds)
    } catch { $elapsedSeconds = $null }

    $record = [ordered]@{
        recordKind     = 'CT21D_PHASE2_SESSION_BINDING_CAPTURE'
        schema         = $script:Phase2SchemaId
        governing      = $false
        phase          = 2
        session        = [string]$Params.Session
        runId          = $runId
        captureBinding = [ordered]@{ captureBindingId = (Get-Phase2CaptureBindingId -ProcessIdValue $procId -StartUtc $startUtc -RunId $runId); inputs = 'pid+processStartUtc+runId'; derivation = $script:Phase2CaptureBindingDerivationText; isNotTheAssignedSessionId = $true }
        tool           = [ordered]@{ name = 'Capture-Phase2.ps1'; version = '1'; scriptSha256 = [string]$Params.ScriptSha256; librarySha256 = [string]$Params.LibrarySha256; schemaSha256 = [string]$Params.SchemaSha256; powershellVersion = [string]$Params.PowerShellVersion }
        testHook       = [ordered]@{ active = $hookActive; processName = $(if ($hookActive) { [string]$Params.TestHookName } else { $null }) }
        tupleInputs    = [ordered]@{
            sessionId = [string]$Params.SessionId; declaredSetSha256 = [string]$Params.ExpectedDeclaredSetSha256; acadExeSha256 = [string]$Params.ExpectedAcadSha256
            profileExpected = [string]$Params.ExpectedProfile; requireTranscript = [bool]$Params.RequireTranscript; expectedProcessName = $expectedName
            requireModuleBaseline = [bool]$Params.RequireModuleBaseline; moduleBaselineSha256 = $expectedBaselineSha
        }
        process        = [ordered]@{
            pid = $procId; name = [string]$proc.Name; startUtc = $startUtc
            owner = $(if ($own.Status -eq 'OBSERVED') { [string]$own.Value } else { $null }); ownerStatus = [string]$own.Status
            invokingUser = $invoking
            commandLine = $(if ($cmd.Status -eq 'OBSERVED') { [string]$cmd.Value } else { $null }); commandLineStatus = [string]$cmd.Status
            commandLineSwitches = $clSwitches
        }
        acadBuild      = $build
        profile        = [ordered]@{
            registrySubKey = [string]$Params.ProfilesSubKey; registryStatus = [string]$regRes.Status; registryDefaultValue = $(if ($regRes.Status -eq 'OBSERVED') { [string]$regRes.Value } else { $null })
            commandLineProfileStatus = $clStatus; commandLineProfile = $clProfile
            effective = $effective; effectiveSource = $source; confirmedFromInsideProcess = 'HOST-TO-CONFIRM'
        }
        modules        = [ordered]@{ status = $(if ($modRes.Ok) { 'OBSERVED' } else { 'UNREADABLE' }); count = $modSorted.Count; items = $modSorted; declaredSetFolder = $dsFolder; declaredSetSubset = $subset.ToArray() }
        processList    = [ordered]@{ status = $(if ($plRes.Ok) { 'OBSERVED' } else { 'UNREADABLE' }); count = $plSorted.Count; items = $plSorted; related = $relSorted }
        moduleBaseline = $moduleBaseline
        profileAtRest  = [ordered]@{
            informationalOnly = $true
            note = 'AT-REST values stored in the profile registry key of the effective profile, read-only. They are not the in-process values and are in no check.'
            variablesSubKey = $variablesSubKey; secureload = $atRestSecureload; trustedpaths = $atRestTrusted
        }
        pinCheck       = $pinCheck
        transcript     = $transcript
        coverage       = (Get-Phase2Coverage -ModuleBaselineStatus ([string]$moduleBaseline.status))
        hostToConfirm  = $script:Phase2HostToConfirm
        validation     = [ordered]@{ checks = @(); verdict = $script:Phase2VerdictInvalid }
        volatile       = [ordered]@{ capturedAtUtc = $capturedAt; evidenceRoot = [string]$Params.EvidenceRoot; moduleSigners = $signersSorted; secondsSinceProcessStart = $elapsedSeconds }
    }
    $checks = Get-Phase2Checks -Record $record
    $record['validation'] = [ordered]@{ checks = $checks; verdict = (Get-Phase2Verdict -Checks $checks -HookActive $hookActive) }
    return $record
}

# ---------------------------------------------------------------------------------------------------------------------------
# Offline validation core (no process access). Returns a list of 'CHECK name PASS|FAIL detail' lines and a boolean.
# ---------------------------------------------------------------------------------------------------------------------------
function Test-Phase2RecordBytes {
    param(
        [byte[]]$RecordBytes, $Schema, $Expected, [string]$DeclaredSetJson, [string]$HashesLedgerPath,
        [switch]$CheckSidecar, [string]$SidecarText, [string]$RecordFileName,
        [string]$ValidatorScriptSha256, [string]$ValidatorLibrarySha256, [string]$DesignationJson, [string]$ModuleBaselineJson,
        [string]$SchemaSha256, [string]$ExpectedInputsSchemaSha256
    )
    $lines = New-Object 'System.Collections.Generic.List[string]'
    $valid = $true
    $say = {
        param([string]$Name, [bool]$Ok, [string]$Detail)
        $lines.Add('CHECK ' + $Name + ' ' + $(if ($Ok) { 'PASS' } else { 'FAIL' }) + ' ' + $Detail)
    }
    $rec = $null
    try {
        $text = ConvertFrom-Phase2CanonicalBytes -Bytes $RecordBytes
        $rec = ConvertFrom-Phase2Json -Text $text
        $say.Invoke('BYTES_STRICT_JSON', $true, 'UTF-8, no BOM, LF only, no duplicate keys, integers only') | Out-Null
    } catch {
        $say.Invoke('BYTES_STRICT_JSON', $false, $_.Exception.Message) | Out-Null
        return [pscustomobject]@{ Valid = $false; Lines = $lines.ToArray() }
    }
    $schemaErrors = Test-Phase2AgainstSchema -Value $rec -Schema $Schema
    $say.Invoke('SCHEMA', ($schemaErrors.Count -eq 0), $(if ($schemaErrors.Count -eq 0) { 'record matches ct21d.phase2.v1' } else { ($schemaErrors | Select-Object -First 5) -join ' ; ' })) | Out-Null
    if ($schemaErrors.Count -gt 0) { return [pscustomobject]@{ Valid = $false; Lines = $lines.ToArray() } }

    $canon = ConvertTo-Phase2CanonicalJson -Value $rec
    $say.Invoke('CANONICAL_FORM', ($canon -ceq $text), 'record bytes equal the canonical serialization of their own content') | Out-Null

    $hookActive = ($rec['testHook']['active'] -eq $true)
    $hookEnv = [System.Environment]::GetEnvironmentVariable('CT21D_PHASE2_TEST_STANDIN')
    $hookOk = (-not $hookActive) -or ((-not [string]::IsNullOrEmpty($hookEnv)) -and $hookEnv -ceq [string]$rec['testHook']['processName'])
    $say.Invoke('TEST_HOOK_NOT_IN_PRODUCTION_RECORD', $hookOk, $(if ($hookActive) { 'record carries the test hook' } else { 'no test hook' })) | Out-Null
    $nameRule = $true
    if (-not $hookActive) { $nameRule = ([string]$rec['tupleInputs']['expectedProcessName'] -ceq 'acad') }
    $say.Invoke('EXPECTED_PROCESS_NAME_RULE', $nameRule, 'production records expect the process name acad') | Out-Null

    if ($CheckSidecar) {
        $expectedSidecar = (Get-Phase2BytesSha256 -Bytes $RecordBytes) + '  ' + $RecordFileName + "`n"
        $sideOk = ($null -ne $SidecarText) -and ($SidecarText -ceq $expectedSidecar)
        $say.Invoke('SIDECAR_EQUALS_RECORD_SHA256', $sideOk, $(if ($null -eq $SidecarText) { 'the .sha256 sidecar next to the record is missing' } else { 'the .sha256 sidecar equals the SHA-256 of the record bytes and its name' })) | Out-Null
    }
    $sidOk = ([string]$rec['captureBinding']['captureBindingId'] -ceq (Get-Phase2CaptureBindingId -ProcessIdValue ([int]$rec['process']['pid']) -StartUtc ([string]$rec['process']['startUtc']) -RunId ([string]$rec['runId'])))
    $say.Invoke('CAPTURE_BINDING_DERIVATION', $sidOk, 'captureBindingId recomputed from pid, processStartUtc and runId (it is not the assigned sessionId)') | Out-Null
    $covExpected = Get-Phase2Coverage -ModuleBaselineStatus ([string]$rec['moduleBaseline']['status'])
    $say.Invoke('COVERAGE_CONSISTENT', ((ConvertTo-Phase2CanonicalJson -Value $covExpected) -ceq (ConvertTo-Phase2CanonicalJson -Value $rec['coverage'])), 'coverage section equals the one derived from the module baseline status') | Out-Null
    $session = [string]$rec['session']
    $act = $script:Phase2SessionActivity[$session]
    $runOk = ([string]$rec['runId'] -cmatch ('^HGP-H' + $act + '-'))
    $say.Invoke('RUN_ID_MATCHES_SESSION', $runOk, ('session ' + $session + ' uses activity number ' + $act)) | Out-Null
    $leafOk = ([System.IO.Path]::GetFileName(([string]$rec['volatile']['evidenceRoot']).TrimEnd('\')) -ceq [string]$rec['runId'])
    $say.Invoke('EVIDENCE_ROOT_LEAF_IS_RUN_ID', $leafOk, 'last component of the evidence root equals the run id') | Out-Null

    $names = @($rec['validation']['checks'] | ForEach-Object { [string]$_['name'] })
    $say.Invoke('CHECK_LIST_COMPLETE_AND_ORDERED', (($names -join ',') -ceq ($script:Phase2CheckNames -join ',')), ('expected ' + $script:Phase2CheckNames.Count + ' named checks in fixed order')) | Out-Null
    $re = Get-Phase2Checks -Record $rec
    $reHash = (Get-Phase2BytesSha256 -Bytes (ConvertTo-Phase2Utf8Bytes -Text (ConvertTo-Phase2CanonicalJson -Value $re)))
    $recHash = (Get-Phase2BytesSha256 -Bytes (ConvertTo-Phase2Utf8Bytes -Text (ConvertTo-Phase2CanonicalJson -Value $rec['validation']['checks'])))
    $say.Invoke('CHECKS_RECOMPUTED_EQUAL', ($reHash -ceq $recHash), 'every recorded check result equals the offline re-derivation') | Out-Null
    $verdictExpected = Get-Phase2Verdict -Checks $re -HookActive $hookActive
    $say.Invoke('VERDICT_CONSISTENT', ([string]$rec['validation']['verdict'] -ceq $verdictExpected), ('recorded ' + $rec['validation']['verdict'] + ' derived ' + $verdictExpected)) | Out-Null
    foreach ($c in $re) { $say.Invoke('RECORD_' + $c['name'], ($c['result'] -eq 'PASS'), [string]$c['detail']) | Out-Null }

    # Independent expected tuple inputs
    $e = $Expected
    $say.Invoke('EXPECTED_SESSION', ([string]$e['session'] -ceq $session), '') | Out-Null
    $say.Invoke('EXPECTED_RUN_ID', ([string]$e['runId'] -ceq [string]$rec['runId']), '') | Out-Null
    $say.Invoke('EXPECTED_DECLARED_SET_SHA256', ([string]$e['declaredSetSha256'] -ceq [string]$rec['tupleInputs']['declaredSetSha256']), '') | Out-Null
    $say.Invoke('EXPECTED_ACAD_SHA256', ([string]$e['acadExeSha256'] -ceq [string]$rec['tupleInputs']['acadExeSha256']), '') | Out-Null
    $say.Invoke('EXPECTED_PROFILE', ([string]$e['profileExpected'] -ceq [string]$rec['tupleInputs']['profileExpected']), '') | Out-Null
    $say.Invoke('EXPECTED_REQUIRE_TRANSCRIPT', ($e['requireTranscript'] -eq $rec['tupleInputs']['requireTranscript']), '') | Out-Null
    $say.Invoke('EXPECTED_SESSION_ID', ([string]$e['sessionId'] -ceq [string]$rec['tupleInputs']['sessionId']), 'the CAD-manager-assigned sessionId of phase 1 equals the one the capture was given') | Out-Null
    $say.Invoke('EXPECTED_REQUIRE_MODULE_BASELINE', ($e['requireModuleBaseline'] -eq $rec['tupleInputs']['requireModuleBaseline']), '') | Out-Null
    $say.Invoke('EXPECTED_MODULE_BASELINE_SHA256', ($null -eq $e['moduleBaselineSha256'] -and $null -eq $rec['tupleInputs']['moduleBaselineSha256']) -or ($null -ne $e['moduleBaselineSha256'] -and [string]$e['moduleBaselineSha256'] -ceq [string]$rec['tupleInputs']['moduleBaselineSha256']), '') | Out-Null
    # An expected-inputs file that REQUIRES a baseline cannot be satisfied by a record that has none (the record cannot waive it).
    if ($e['requireModuleBaseline'] -eq $true) {
        $say.Invoke('EXPECTED_MODULE_BASELINE_PROVIDED', ([string]$rec['moduleBaseline']['status'] -ceq 'PROVIDED'), 'the expected inputs require a module baseline comparison') | Out-Null
    }

    if ($DeclaredSetJson) {
        $cur = Read-Phase2DeclaredSet -FullPath $DeclaredSetJson
        $drift = ($cur.status -eq 'OK') -and ($cur.sha256 -ceq [string]$rec['pinCheck']['declaredSet']['sha256'])
        $folderNowOk = $false
        $folderNowDetail = 'declared-set.json unreadable or different from the recorded one'
        if ($drift) {
            $pc = Get-Phase2PinCheck -DeclaredSetFullPath $DeclaredSetJson -DeclaredSet $cur
            $recEntries = @($rec['pinCheck']['entries'])
            if (@($pc.entries).Count -ne $recEntries.Count) { $drift = $false }
            else {
                for ($i = 0; $i -lt $recEntries.Count; $i++) { if ($pc.entries[$i].actualSha256 -cne [string]$recEntries[$i]['actualSha256']) { $drift = $false } }
            }
            # Round 2 (review 2 M2): the folder is listed AGAIN now. The rule "the designation is placed only after phase 2 validates"
            # (R3-2, package v3 3.7 steps 3-4) is checked at the validation instant too: any unexpected or missing entry now (for
            # example a run-designation.json placed between the capture and this validation), or any difference from the recorded
            # listing, fails. Re-validating after the designation was legitimately placed therefore FAILS by design.
            $fn = $pc['folder']
            $folderNowOk = ($fn['status'] -eq 'OBSERVED') -and (@($fn['unexpected']).Count -eq 0) -and (@($fn['missing']).Count -eq 0) -and
                ((ConvertTo-Phase2CanonicalJson -Value $fn) -ceq (ConvertTo-Phase2CanonicalJson -Value $rec['pinCheck']['folder']))
            $folderNowDetail = 'status=' + $fn['status'] + ' unexpectedNow=' + @($fn['unexpected']).Count + ' missingNow=' + @($fn['missing']).Count + ' listingEqualsRecorded=' + ((ConvertTo-Phase2CanonicalJson -Value $fn) -ceq (ConvertTo-Phase2CanonicalJson -Value $rec['pinCheck']['folder']))
        }
        $say.Invoke('OFFLINE_DECLARED_SET_UNCHANGED_SINCE_CAPTURE', $drift, 'declared-set.json and the DLLs hashed again now equal the recorded values') | Out-Null
        $say.Invoke('OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW', $folderNowOk, ('the declared-set folder listed again now holds exactly the recorded entries (no designation placed yet): ' + $folderNowDetail)) | Out-Null
    }
    if ($ModuleBaselineJson) {
        $bl = Read-Phase2ModuleBaseline -FullPath $ModuleBaselineJson
        $blOk = ($bl.status -eq 'OK') -and ([string]$rec['moduleBaseline']['status'] -ceq 'PROVIDED') -and ($bl.sha256 -ceq [string]$rec['moduleBaseline']['sha256'])
        if ($blOk) {
            $outside = Get-Phase2ModulesOutsideBaseline -BaselineEntries $bl.entries -ModuleItems @($rec['modules']['items'])
            $blOk = ((ConvertTo-Phase2CanonicalJson -Value $outside) -ceq (ConvertTo-Phase2CanonicalJson -Value @($rec['moduleBaseline']['outsideBaseline'])))
        }
        $say.Invoke('OFFLINE_MODULE_BASELINE_RECOMPUTED', $blOk, 'the baseline file hashed now equals the recorded hash and the modules outside it are recomputed from the record modules') | Out-Null
    }
    if ($DesignationJson) {
        $dBytes = $null; $dObj = $null
        try {
            $dBytes = Read-Phase2FileBytes -Path $DesignationJson -MaxBytes 4194304
            $script:Phase2JsonLenientNumbers = $true
            $dObj = ConvertFrom-Phase2Json -Text ((New-Object System.Text.UTF8Encoding($false, $true)).GetString($dBytes).TrimStart([char]0xFEFF))
        } catch { $dObj = $null } finally { $script:Phase2JsonLenientNumbers = $false }
        $dSid = $null; $dRun = $null
        if ($dObj -is [System.Collections.IDictionary]) { $dSid = $dObj['sessionId']; $dRun = $dObj['runId'] }
        $say.Invoke('DESIGNATION_SESSION_ID_EQUALS_RECORD', ($dSid -is [string] -and $dSid -ceq [string]$rec['tupleInputs']['sessionId']), 'the composed designation file carries the same sessionId as the record') | Out-Null
        $say.Invoke('DESIGNATION_RUN_ID_EQUALS_RECORD', ($dRun -is [string] -and $dRun -ceq [string]$rec['runId']), 'the composed designation file carries the same runId as the record') | Out-Null
    }
    if ($HashesLedgerPath) {
        $ledger = @{}
        foreach ($l in ([System.IO.File]::ReadAllText($HashesLedgerPath) -split "`n")) {
            if ($l -cmatch '^([0-9a-f]{64})  (.+)$') { $ledger[$Matches[2]] = $Matches[1] }
        }
        $lok = ($ledger['Capture-Phase2.ps1'] -ceq [string]$rec['tool']['scriptSha256']) -and ($ledger['lib/Phase2Common.ps1'] -ceq [string]$rec['tool']['librarySha256']) -and ($ledger['schemas/ct21d.phase2.v1.json'] -ceq [string]$rec['tool']['schemaSha256'])
        $say.Invoke('TOOL_IDENTITY_IN_HASHES_LEDGER', $lok, 'script, library and schema hashes of the record appear in HASHES-PHASE2.txt') | Out-Null
        # Round 2 (review 2 M1): the schema this validation ran against is itself pinned. The bytes of the -Schema file must equal the
        # ledger line AND the schemaSha256 the capture recorded; the expected-inputs schema must equal its ledger line. Without a hash
        # supplied the check FAILS (fail-closed): a validator given an empty schema would otherwise accept an extra-property record.
        $schemaPinOk = (-not [string]::IsNullOrEmpty($SchemaSha256)) -and ($ledger['schemas/ct21d.phase2.v1.json'] -ceq $SchemaSha256) -and ($SchemaSha256 -ceq [string]$rec['tool']['schemaSha256'])
        $say.Invoke('SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD', $schemaPinOk, 'the bytes of the -Schema file equal the ledger and the schemaSha256 the capture recorded') | Out-Null
        $expSchemaPinOk = (-not [string]::IsNullOrEmpty($ExpectedInputsSchemaSha256)) -and ($ledger['schemas/ct21d.phase2.expected-inputs.v1.json'] -ceq $ExpectedInputsSchemaSha256)
        $say.Invoke('EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER', $expSchemaPinOk, 'the bytes of the expected-inputs schema file equal the ledger') | Out-Null
        if ($ValidatorScriptSha256) {
            $vok = ($ledger['Validate-Phase2.ps1'] -ceq $ValidatorScriptSha256) -and ($ledger['lib/Phase2Common.ps1'] -ceq $ValidatorLibrarySha256)
            $say.Invoke('VALIDATOR_IDENTITY_IN_HASHES_LEDGER', $vok, 'the running Validate-Phase2.ps1 and the library it loaded equal the ledger') | Out-Null
        }
    }

    foreach ($l in $lines) { if ($l -match '^CHECK \S+ FAIL') { $valid = $false } }
    if ([string]$rec['validation']['verdict'] -eq $script:Phase2VerdictInvalid) { $valid = $false; $lines.Add('NOTE the record itself carries the verdict ' + $script:Phase2VerdictInvalid) }
    return [pscustomobject]@{ Valid = $valid; Lines = $lines.ToArray(); StandIn = $hookActive }
}
