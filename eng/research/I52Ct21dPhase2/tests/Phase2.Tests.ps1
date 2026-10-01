# Functional tests of the phase 2 scripts. Dot-sourced by Run-AllTests.ps1 (needs TestSupport.ps1 loaded first).
# Everything here runs without AutoCAD: logic branches use injected fake providers; the end-to-end section uses a benign stand-in
# process (a renamed copy of ping.exe) and the documented environment hook CT21D_PHASE2_TEST_STANDIN.

$schemaPath = Join-Path -Path $script:ToolRoot -ChildPath 'schemas/ct21d.phase2.v1.json'
$expSchemaPath = Join-Path -Path $script:ToolRoot -ChildPath 'schemas/ct21d.phase2.expected-inputs.v1.json'
$strictUtf8 = New-Object System.Text.UTF8Encoding($false, $true)
$schema = ConvertFrom-Phase2Json -Text ([System.IO.File]::ReadAllText($schemaPath, $strictUtf8))
$expSchema = ConvertFrom-Phase2Json -Text ([System.IO.File]::ReadAllText($expSchemaPath, $strictUtf8))

function New-ExpectedFor {
    param($Record, [hashtable]$Override = @{})
    $e = @{
        schema = 'ct21d.phase2.expected-inputs.v1'; session = $Record['session']; runId = $Record['runId']
        sessionId = $Record['tupleInputs']['sessionId']
        declaredSetSha256 = $Record['tupleInputs']['declaredSetSha256']; acadExeSha256 = $Record['tupleInputs']['acadExeSha256']
        profileExpected = $Record['tupleInputs']['profileExpected']; requireTranscript = $Record['tupleInputs']['requireTranscript']
        requireModuleBaseline = $Record['tupleInputs']['requireModuleBaseline']; moduleBaselineSha256 = $Record['tupleInputs']['moduleBaselineSha256']
    }
    foreach ($k in $Override.Keys) { $e[$k] = $Override[$k] }
    return $e
}

function Test-RecordOffline {
    param($Record, $Expected = $null)
    if ($null -eq $Expected) { $Expected = New-ExpectedFor -Record $Record }
    return (Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $Record) -Schema $schema -Expected $Expected)
}

# ======================================================================================================================
Start-TestSection 'canonical JSON'
$o = [ordered]@{ b = 1; a = @('x', 'y'); c = [ordered]@{ z = $true; y = $null; x = 'q"\' + "`n" + [char]1 }; Aa = 0; e = @() }
$j = ConvertTo-Phase2CanonicalJson -Value $o
$lines = $j -split "`n"
Assert-True 'keys are sorted by ordinal comparison (Aa before a before b)' ($j.IndexOf('"Aa"') -lt $j.IndexOf('"a"') -and $j.IndexOf('"a"') -lt $j.IndexOf('"b"'))
Assert-True 'ends with exactly one LF and has no CR' ($j.EndsWith("`n") -and -not $j.EndsWith("`n`n") -and -not $j.Contains("`r"))
Assert-True 'empty array is []' ($j.Contains('"e": []'))
Assert-True 'string escapes are minimal JSON escapes' ($j.Contains('"x": "q\"\\\n\u0001"'))
$bytes = ConvertTo-Phase2Utf8Bytes -Text $j
Assert-True 'no BOM' (-not ($bytes[0] -eq 0xEF))
$back = ConvertFrom-Phase2Json -Text $j
Assert-Equal 'round trip is stable' $j (ConvertTo-Phase2CanonicalJson -Value $back)
Assert-Equal 'same value in a different insertion order gives identical text' $j (ConvertTo-Phase2CanonicalJson -Value ([ordered]@{ e = @(); Aa = 0; c = [ordered]@{ x = 'q"\' + "`n" + [char]1; y = $null; z = $true }; a = @('x', 'y'); b = 1 }))
$threw = $false; try { ConvertTo-Phase2CanonicalJson -Value @{ f = 1.5 } | Out-Null } catch { $threw = $true }
Assert-True 'floating point numbers are rejected' $threw
$threw = $false; try { ConvertTo-Phase2CanonicalJson -Value @{ f = ([string][char]0xD800) } | Out-Null } catch { $threw = $true }
Assert-True 'a lone surrogate is rejected' $threw
$threw = $false; try { ConvertFrom-Phase2Json -Text '{"a":1,"a":2}' | Out-Null } catch { $threw = $true }
Assert-True 'duplicate keys are rejected by the parser' $threw
$threw = $false; try { ConvertFrom-Phase2Json -Text '{"a":1.0}' | Out-Null } catch { $threw = $true }
Assert-True 'non-integer numbers are rejected by the parser' $threw
$threw = $false; try { ConvertFrom-Phase2Json -Text '{"a":1,}' | Out-Null } catch { $threw = $true }
Assert-True 'trailing commas are rejected by the parser' $threw
$threw = $false; try { ConvertFrom-Phase2CanonicalBytes -Bytes ([byte[]](0xEF, 0xBB, 0xBF, 0x7B, 0x7D)) | Out-Null } catch { $threw = $true }
Assert-True 'a UTF-8 BOM is rejected' $threw
$threw = $false; try { ConvertFrom-Phase2CanonicalBytes -Bytes ([byte[]](0x7B, 0x0D, 0x0A, 0x7D)) | Out-Null } catch { $threw = $true }
Assert-True 'CR is rejected' $threw
$uni = [string][char]0x00E1 + [string][char]0x4E2D
Assert-True 'non-ASCII characters are written raw (UTF-8)' ((ConvertTo-Phase2CanonicalJson -Value @{ k = $uni }).Contains($uni))

# ======================================================================================================================
Start-TestSection 'schema validator (closed subset)'
$good = Invoke-FakeCapture
Assert-Equal 'fake happy path: verdict' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $good['validation']['verdict']
$goodObj = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $good)
Assert-Equal 'positive: a real record matches the schema' 0 (Test-Phase2AgainstSchema -Value $goodObj -Schema $schema).Count
function Copy-Rec { return (ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $good)) }
$neg = @(
    @('missing required property', { param($r) $r.Remove('runId') }),
    @('additional top-level property', { param($r) $r['extra'] = 1 }),
    @('additional nested property', { param($r) $r['process']['extra'] = 1 }),
    @('governing true', { param($r) $r['governing'] = $true }),
    @('phase 3', { param($r) $r['phase'] = 3 }),
    @('bad enum (session)', { param($r) $r['session'] = 'S9' }),
    @('bad pattern (runId)', { param($r) $r['runId'] = 'HGP-H1-bad' }),
    @('bad pattern (sha256 uppercase)', { param($r) $r['tupleInputs']['acadExeSha256'] = ('A' * 64) }),
    @('wrong type (pid string)', { param($r) $r['process']['pid'] = '12' }),
    @('pid below minimum', { param($r) $r['process']['pid'] = 0 }),
    @('null where string required', { param($r) $r['process']['name'] = $null }),
    @('verdict not in enum', { param($r) $r['validation']['verdict'] = 'OK' }),
    @('check result not in enum', { param($r) $r['validation']['checks'][0]['result'] = 'MAYBE' }),
    @('empty hostToConfirm (minItems)', { param($r) $r['hostToConfirm'] = @() }),
    @('bad utc form', { param($r) $r['process']['startUtc'] = '2026-10-01 14:59:00' }),
    @('modules item with extra key', { param($r) $r['modules']['items'][0]['extra'] = 1 })
)
$schemaText = [System.IO.File]::ReadAllText($schemaPath, $strictUtf8)
$tjOk = $false
try { $tjOk = [bool](Test-Json -Json (ConvertTo-Phase2CanonicalJson -Value $goodObj) -Schema $schemaText -ErrorAction Stop) } catch { $tjOk = $false }
Assert-True 'cross-check: the built-in Test-Json (draft 2020-12) accepts the same record' $tjOk
foreach ($n in $neg) {
    $r = Copy-Rec; & $n[1] $r
    Assert-True ('negative: ' + $n[0]) ((Test-Phase2AgainstSchema -Value $r -Schema $schema).Count -gt 0)
    $tjRejects = $false
    try { $tjRejects = -not [bool](Test-Json -Json (ConvertTo-Phase2CanonicalJson -Value $r) -Schema $schemaText -ErrorAction Stop) } catch { $tjRejects = $true }
    Assert-True ('cross-check: Test-Json also rejects: ' + $n[0]) $tjRejects
}
$bad = ConvertFrom-Phase2Json -Text '{"type":"object","minProperties":1}'
Assert-True 'a schema keyword outside the closed subset is an error' ((Test-Phase2AgainstSchema -Value @{} -Schema $bad).Count -gt 0)
Assert-Equal 'expected-inputs schema: positive' 0 (Test-Phase2AgainstSchema -Value (ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value (New-ExpectedFor -Record $good))) -Schema $expSchema).Count
$e2 = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value (New-ExpectedFor -Record $good)); $e2['extra'] = 1
Assert-True 'expected-inputs schema: additional property rejected' ((Test-Phase2AgainstSchema -Value $e2 -Schema $expSchema).Count -gt 0)
# The schema file is itself well formed and declares draft 2020-12.
Assert-Equal 'schema declares draft 2020-12' 'https://json-schema.org/draft/2020-12/schema' $schema['$schema']
Assert-True 'schema top level is closed' ($schema['additionalProperties'] -eq $false)

# ======================================================================================================================
Start-TestSection 'capture with fakes: happy path and record content'
Assert-Equal 'record is non-governing' $false $good['governing']
Assert-Equal 'phase is 2' 2 $good['phase']
Assert-Equal 'all checks present in the fixed order' ($script:Phase2CheckNames -join ',') (($good['validation']['checks'] | ForEach-Object { $_['name'] }) -join ',')
Assert-Equal 'every check passes' 0 (@($good['validation']['checks'] | Where-Object { $_['result'] -ne 'PASS' })).Count
Assert-Equal 'captureBindingId = first 16 hex of the documented derivation' (Get-Phase2CaptureBindingId -ProcessIdValue 4242 -StartUtc '2026-10-01T14:59:00.123Z' -RunId $good['runId']) $good['captureBinding']['captureBindingId']
Assert-Equal 'captureBindingId has 16 lowercase hex' 1 ([regex]::Matches($good['captureBinding']['captureBindingId'], '^[0-9a-f]{16}$')).Count
Assert-True 'a different pid changes the captureBindingId' ((Get-Phase2CaptureBindingId -ProcessIdValue 4243 -StartUtc '2026-10-01T14:59:00.123Z' -RunId $good['runId']) -ne $good['captureBinding']['captureBindingId'])
Assert-True 'a different start time changes the captureBindingId' ((Get-Phase2CaptureBindingId -ProcessIdValue 4242 -StartUtc '2026-10-01T14:59:00.124Z' -RunId $good['runId']) -ne $good['captureBinding']['captureBindingId'])
Assert-True 'a different run id changes the captureBindingId' ((Get-Phase2CaptureBindingId -ProcessIdValue 4242 -StartUtc '2026-10-01T14:59:00.123Z' -RunId 'HGP-H1-20261001T150000Z-02') -ne $good['captureBinding']['captureBindingId'])
Assert-Equal 'transcript is NOT_PROVIDED and explicit for S1-A' 'NOT_APPLICABLE_S1A_NO_LOAD_STEP' $good['transcript']['requirement']
Assert-Equal 'transcript status' 'NOT_PROVIDED' $good['transcript']['status']
Assert-Equal 'effective profile from the registry default' 'REGISTRY_DEFAULT_AT_CAPTURE' $good['profile']['effectiveSource']
Assert-Equal 'profile confirmed from inside the process is HOST-TO-CONFIRM' 'HOST-TO-CONFIRM' $good['profile']['confirmedFromInsideProcess']
Assert-Equal 'module count' 5 $good['modules']['count']
Assert-Equal 'modules sorted by lowercase path (first is acad.exe)' 'acad.exe' ([System.IO.Path]::GetFileName($good['modules']['items'][0]['path']))
Assert-Equal 'modules sorted (last is ntdll.dll)' 'ntdll.dll' ([System.IO.Path]::GetFileName($good['modules']['items'][4]['path']))
Assert-Equal 'declared-set subset is empty' 0 @($good['modules']['declaredSetSubset']).Count
Assert-Equal 'process list sorted by pid' '4,900,4242' (($good['processList']['items'] | ForEach-Object { $_['pid'] }) -join ',')
Assert-Equal 'process list start time of an unreadable process is explicit' 'UNREADABLE' $good['processList']['items'][0]['startUtc']
Assert-Equal 'tool identity is recorded' ('1' * 64) $good['tool']['scriptSha256']
Assert-True 'host-to-confirm list is recorded' (@($good['hostToConfirm']).Count -ge 5)
Assert-Equal 'declared-set entries recorded' 4 @($good['pinCheck']['entries']).Count
Assert-Equal 'folder listing: 8 files (4 DLL + 4 pins)' 8 @($good['pinCheck']['folder']['listing']).Count
$off = Test-RecordOffline -Record $good
Assert-True 'offline validator accepts the happy record' $off.Valid ($off.Lines -join ' | ')

# ======================================================================================================================
Start-TestSection 'fail-closed branches (record is written INVALID or the capture refuses)'
function Assert-Fails {
    param([string]$Name, $Record, [string]$Check)
    Assert-Equal ($Name + ' -> ' + $Check + ' FAIL') 'FAIL' (Get-CheckResult -Record $Record -Name $Check)
    Assert-Equal ($Name + ' -> verdict') 'PHASE2_INVALID' $Record['validation']['verdict']
    $v = Test-RecordOffline -Record $Record
    Assert-True ($Name + ' -> offline validator says invalid') (-not $v.Valid)
}
$r = Invoke-FakeCapture { param($st) $st.Process = [pscustomobject]@{ Pid = 4242; Name = 'notacad'; StartUtc = $st.Process.StartUtc; MainStatus = 'OBSERVED'; MainPath = $st.Process.MainPath; FileVersion = '1'; ProductVersion = '1' } }
Assert-Fails 'wrong process name' $r 'PROCESS_NAME_IS_EXPECTED'
$r = Invoke-FakeCapture { param($st) $st.Processes = [pscustomobject]@{ Ok = $true; Items = @($st.Processes.Items) + [pscustomobject]@{ Pid = 5000; Name = 'acad'; StartUtc = '2026-10-01T14:00:00.000Z' } } }
Assert-Fails 'two acad processes' $r 'SINGLE_TARGET_PROCESS'
$r = Invoke-FakeCapture { param($st) $st.Processes = [pscustomobject]@{ Ok = $true; Items = @($st.Processes.Items | Where-Object { $_.Pid -ne 4242 }) } }
Assert-Fails 'target pid missing from the process list' $r 'SINGLE_TARGET_PROCESS'
$r = Invoke-FakeCapture { param($st) $st.Processes = [pscustomobject]@{ Ok = $false; Items = @() } }
Assert-Fails 'process list not enumerable' $r 'PROCESS_LIST_ENUMERATED'
$r = Invoke-FakeCapture { param($st) $st.Processes = [pscustomobject]@{ Ok = $true; Items = @($st.Processes.Items) + [pscustomobject]@{ Pid = 5001; Name = 'acadlt'; StartUtc = $null } + [pscustomobject]@{ Pid = 5002; Name = 'AdskLicensingService'; StartUtc = $null } } }
Assert-Equal 'other acad* and Autodesk-related processes are flagged' 'ACAD_PREFIX,AUTODESK_RELATED' ((@($r['processList']['related'] | Where-Object { $_['reason'] -ne 'TARGET_NAME' }) | ForEach-Object { $_['reason'] }) -join ',')
Assert-Equal 'flagging alone does not fail the record' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $r['validation']['verdict']
$r = Invoke-FakeCapture { param($st) $st.Owner = [pscustomobject]@{ Status = 'OBSERVED'; Value = 'OTHER\user' } }
Assert-Fails 'process owner differs from the invoking user' $r 'PROCESS_OWNER_IS_INVOKING_USER'
$r = Invoke-FakeCapture { param($st) $st.Owner = [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
Assert-Fails 'owner not observable' $r 'PROCESS_OWNER_IS_INVOKING_USER'
$r = Invoke-FakeCapture { param($st) $st.Process = [pscustomobject]@{ Pid = 4242; Name = 'acad'; StartUtc = $st.Process.StartUtc; MainStatus = 'UNREADABLE'; MainPath = $null; FileVersion = $null; ProductVersion = $null } }
Assert-Fails 'main module unreadable' $r 'ACAD_BUILD_OBSERVED'
$r = Invoke-FakeCapture { param($st, $p) $p.ExpectedAcadSha256 = ('b' * 64) }
Assert-Fails 'acad sha256 differs from the tuple' $r 'ACAD_SHA256_EQUALS_TUPLE'
$r = Invoke-FakeCapture { param($st, $p) $p.ExpectedProfile = 'Other Profile' }
Assert-Fails 'profile differs from the expected one' $r 'PROFILE_EQUALS_EXPECTED'
$r = Invoke-FakeCapture { param($st) $st.Registry = [pscustomobject]@{ Status = 'KEY_ABSENT'; Value = $null } }
Assert-Fails 'profile registry key absent (no guess)' $r 'PROFILE_EFFECTIVE_OBSERVED'
$r = Invoke-FakeCapture { param($st) $st.Registry = [pscustomobject]@{ Status = 'UNREADABLE'; Value = $null } }
Assert-Fails 'profile registry unreadable (no guess)' $r 'PROFILE_EFFECTIVE_OBSERVED'
$r = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
Assert-Fails 'command line not observable -> profile not asserted' $r 'PROFILE_EFFECTIVE_OBSERVED'
$r = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'OBSERVED'; Value = '"C:\x\acad.exe" /p "<<Unnamed Profile>>"' } }
Assert-Equal '/p switch with quotes: source' 'COMMAND_LINE_P_SWITCH' $r['profile']['effectiveSource']
Assert-Equal '/p switch with quotes: value' '<<Unnamed Profile>>' $r['profile']['effective']
Assert-Equal '/p switch agreeing with the registry is valid' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $r['validation']['verdict']
$r = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'OBSERVED'; Value = 'acad.exe /P Other /nologo' } }
Assert-Equal '/P switch without quotes: value' 'Other' $r['profile']['effective']
Assert-Fails 'command-line profile conflicts with the registry default' $r 'PROFILE_SOURCES_CONSISTENT'
$r = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'OBSERVED'; Value = 'acad.exe /p C:\x\profile.arg' } }
Assert-Equal '/p with an .arg file: status' 'ARG_FILE' $r['profile']['commandLineProfileStatus']
Assert-Fails '/p with an .arg file is not guessed' $r 'PROFILE_EFFECTIVE_OBSERVED'
$r = Invoke-FakeCapture { param($st) $st.Modules = [pscustomobject]@{ Ok = $false; Items = @() } }
Assert-Fails 'module enumeration failed' $r 'MODULES_ENUMERATED'
$r = Invoke-FakeCapture { param($st) $st.Modules = [pscustomobject]@{ Ok = $true; Items = @($st.Modules.Items) + [pscustomobject]@{ Path = 'C:\no\such\file.dll'; FileVersion = $null } } }
Assert-Fails 'a module file cannot be hashed' $r 'MODULE_HASHES_READABLE'
$r = Invoke-FakeCapture { param($st, $p, $ds) $st.Modules = [pscustomobject]@{ Ok = $true; Items = @($st.Modules.Items) + [pscustomobject]@{ Path = $ds.Entries[3].path; FileVersion = '1' } } }
Assert-Fails 'a declared-set DLL is already loaded at phase 2' $r 'NO_DECLARED_SET_MODULE_LOADED'
Assert-Equal 'the loaded declared-set module is listed in the subset' 1 @($r['modules']['declaredSetSubset']).Count
$r = Invoke-FakeCapture { param($st, $p, $ds) $st.Modules = [pscustomobject]@{ Ok = $true; Items = @($st.Modules.Items | Where-Object { $_.Path -notlike '*acad.exe' }) } }
Assert-Fails 'main module missing from the inventory' $r 'MAIN_MODULE_IN_INVENTORY'

# declared set: DLL vs pin vs declared-set.json
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path $ds.Entries[1].path -Bytes (Get-TestUtf8 'tampered dll') }
Assert-Fails 'DLL bytes differ from the declared-set entry (pin and entry agree with each other)' $r 'DECLARED_DLL_HASHES_EQUAL_ENTRY'
Assert-Equal '... and the pin no longer equals the DLL' 'FAIL' (Get-CheckResult -Record $r -Name 'PIN_FILES_EQUAL_DLL_AND_ENTRY')
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path ($ds.Entries[0].path + '.pin') -Bytes (Get-TestUtf8 (('0' * 64) + "`n")) }
Assert-Fails 'pin differs from DLL and entry' $r 'PIN_FILES_EQUAL_DLL_AND_ENTRY'
Assert-Equal '... the DLL itself still equals its entry' 'PASS' (Get-CheckResult -Record $r -Name 'DECLARED_DLL_HASHES_EQUAL_ENTRY')
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path ($ds.Entries[0].path + '.pin') -Bytes (Get-TestUtf8 ($ds.Entries[0].sha256)) }
Assert-Fails 'pin without the trailing LF (format)' $r 'PIN_FILES_EQUAL_DLL_AND_ENTRY'
Assert-Equal '... pin status is FORMAT_INVALID' 'FORMAT_INVALID' $r['pinCheck']['entries'][0]['pinStatus']
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path ($ds.Entries[0].path + '.pin') -Bytes (Get-TestUtf8 ($ds.Entries[0].sha256.ToUpperInvariant() + "`n")) }
Assert-Fails 'pin with uppercase hex (format)' $r 'PIN_FILES_EQUAL_DLL_AND_ENTRY'
$r = Invoke-FakeCapture { param($st, $p, $ds) [System.IO.File]::Delete($ds.Entries[2].path + '.pin') }
Assert-Fails 'pin file missing' $r 'PIN_FILES_EQUAL_DLL_AND_ENTRY'
Assert-Fails 'pin file missing -> the folder content is not exact' $r 'DECLARED_FOLDER_CONTENT_EXACT'
$r = Invoke-FakeCapture { param($st, $p, $ds) [System.IO.File]::Delete($ds.Entries[3].path) }
Assert-Fails 'DLL file missing' $r 'DECLARED_DLL_HASHES_EQUAL_ENTRY'
$r = Invoke-FakeCapture { param($st, $p, $ds) $p.ExpectedDeclaredSetSha256 = ('c' * 64) }
Assert-Fails 'declared-set.json hash differs from the tuple value' $r 'DECLARED_SET_SHA256_EQUALS_TUPLE'
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path (Join-Path $ds.Folder 'extra.txt') -Bytes (Get-TestUtf8 'x') }
Assert-Fails 'extra file in the declared-set folder (designation not yet placed)' $r 'DECLARED_FOLDER_CONTENT_EXACT'
Assert-Equal '... the extra file is named' 'extra.txt' $r['pinCheck']['folder']['unexpected'][0]
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path (Join-Path $ds.Folder 'run-designation.json') -Bytes (Get-TestUtf8 '{}') }
Assert-Fails 'a designation file already placed is detected' $r 'DECLARED_FOLDER_CONTENT_EXACT'
$r = Invoke-FakeCapture { param($st, $p, $ds) [void][System.IO.Directory]::CreateDirectory((Join-Path $ds.Folder 'sub')) }
Assert-Fails 'a subdirectory in the declared-set folder' $r 'DECLARED_FOLDER_CONTENT_EXACT'
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path $ds.Json -Bytes (Get-TestUtf8 '[{"path":"C:\\x\\a.dll"}]') }
Assert-Fails 'declared-set.json malformed (entry without sha256)' $r 'DECLARED_SET_JSON_WELL_FORMED'
Assert-Equal '... status MALFORMED' 'MALFORMED' $r['pinCheck']['declaredSet']['status']
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path $ds.Json -Bytes (Get-TestUtf8 '[]') }
Assert-Fails 'declared-set.json empty array' $r 'DECLARED_SET_JSON_WELL_FORMED'
$r = Invoke-FakeCapture { param($st, $p, $ds) Write-TestFile -Path $ds.Json -Bytes (Get-TestUtf8 'not json') }
Assert-Fails 'declared-set.json not JSON' $r 'DECLARED_SET_JSON_WELL_FORMED'
$r = Invoke-FakeCapture {
    param($st, $p, $ds, $dir)
    $other = Join-Path $dir 'other'; [void][System.IO.Directory]::CreateDirectory($other)
    $e = @([ordered]@{ path = $ds.Entries[0].path; sha256 = $ds.Entries[0].sha256 }, [ordered]@{ path = (Join-Path $other 'X.dll'); sha256 = ('d' * 64) })
    Write-TestFile -Path $ds.Json -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value $e)); $p.ExpectedDeclaredSetSha256 = (Get-Phase2FileSha256 -Path $ds.Json)
}
Assert-Fails 'declared set spread over two folders' $r 'DECLARED_SET_SINGLE_FOLDER'

# transcript
$r = Invoke-FakeCapture { param($st, $p) $p.RequireTranscript = $true }
Assert-Fails 'transcript required but not provided' $r 'TRANSCRIPT_REQUIREMENT_MET'
Assert-Equal '... requirement label' 'REQUIRED_NOW' $r['transcript']['requirement']
$r = Invoke-FakeCapture -Session 'S1-B' $script:WithBaseline
Assert-Equal 'S1-B without transcript at phase 2 (with the baseline every later session needs): required later, still valid' 'REQUIRED_LATER' $r['transcript']['requirement']
Assert-Equal '... verdict' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $r['validation']['verdict']
$r = Invoke-FakeCapture {
    param($st, $p, $ds, $dir)
    $t = Join-Path $dir 'log.txt'; Write-TestFile -Path $t -Bytes (Get-TestUtf8 "Command: NETLOAD`r`nAssembly file name: x.dll`nnetload again`n`nlast"); $p.TranscriptPath = $t
}
Assert-Fails 'S1-A transcript containing NETLOAD lines (S1-A has no load step)' $r 'TRANSCRIPT_CONSISTENT_WITH_SESSION'
Assert-Equal '... lines counted' 5 $r['transcript']['lineCount']
Assert-Equal '... NETLOAD lines extracted verbatim (first)' 'Command: NETLOAD' $r['transcript']['loadLines'][0]['text']
Assert-Equal '... NETLOAD line numbers' '1,3' (($r['transcript']['loadLines'] | ForEach-Object { $_['line'] }) -join ',')
Assert-Equal '... NETLOAD lines extracted verbatim (second)' 'netload again' $r['transcript']['loadLines'][1]['text']
$r = Invoke-FakeCapture -Session 'S1-B' {
    param($st, $p, $ds, $dir)
    & $script:WithBaseline $st $p $ds $dir
    $t = Join-Path $dir 'log.txt'; Write-TestFile -Path $t -Bytes (Get-TestUtf8 "Command: NETLOAD`nNETLOAD ok`nNOTNETLOAD x`n"); $p.TranscriptPath = $t
}
Assert-Equal 'S1-B transcript with NETLOAD lines is INVALID too (a load before phase 2 validates is an ordering violation, R3-2)' 'PHASE2_INVALID' $r['validation']['verdict']
Assert-Equal '... through TRANSCRIPT_CONSISTENT_WITH_SESSION' 'FAIL' (Get-CheckResult -Record $r -Name 'TRANSCRIPT_CONSISTENT_WITH_SESSION')
Assert-Equal '... only whole-word matches' 2 @($r['transcript']['loadLines']).Count
Assert-Equal '... line count with trailing LF' 3 $r['transcript']['lineCount']
$r = Invoke-FakeCapture { param($st, $p, $ds, $dir) $t = Join-Path $dir 'empty.txt'; Write-TestFile -Path $t -Bytes ([byte[]]@()); $p.TranscriptPath = $t }
Assert-Equal 'empty transcript has 0 lines' 0 $r['transcript']['lineCount']
$r = Invoke-FakeCapture { param($st, $p, $ds, $dir) $t = Join-Path $dir 'u16.txt'; Write-TestFile -Path $t -Bytes ([System.Text.Encoding]::Unicode.GetPreamble() + [System.Text.Encoding]::Unicode.GetBytes("a`nb")); $p.TranscriptPath = $t }
Assert-Equal 'UTF-16LE transcript is decoded' 'utf-16le' $r['transcript']['encoding']
Assert-Equal '... 2 lines' 2 $r['transcript']['lineCount']
$r = Invoke-FakeCapture { param($st, $p, $ds, $dir) $t = Join-Path $dir 'l1.txt'; Write-TestFile -Path $t -Bytes ([byte[]](0x61, 0xE9, 0x0A)); $p.TranscriptPath = $t }
Assert-Equal 'non UTF-8 transcript falls back to iso-8859-1' 'iso-8859-1' $r['transcript']['encoding']
$r = Invoke-FakeCapture { param($st, $p, $ds, $dir) $t = Join-Path $dir 'h.txt'; Write-TestFile -Path $t -Bytes (Get-TestUtf8 "abc`n"); $p.TranscriptPath = $t }
Assert-Equal 'transcript sha256 is the hash of the file bytes' (Get-Phase2BytesSha256 -Bytes (Get-TestUtf8 "abc`n")) $r['transcript']['sha256']

# refusals (nothing is produced)
$dir = New-CaseDir; $ds = New-TestDeclaredSet -Dir $dir; $st = New-FakeState -Dir $dir; $p = New-TestParams -Dir $dir -Ds $ds -St $st
$st.Process = $null
Assert-Refused 'process id not running' { Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st) } 'no running process'
$st = New-FakeState -Dir $dir
$st.Process = [pscustomobject]@{ Pid = 4242; Name = 'acad'; StartUtc = $null; MainStatus = 'OBSERVED'; MainPath = $st.Process.MainPath; FileVersion = '1'; ProductVersion = '1' }
Assert-Refused 'process start time unreadable -> cannot bind' { Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st) } 'start time'

$st = New-FakeState -Dir $dir
$pr = New-FakeProviders -St $st
$counter = @{ n = 0 }
$first = $st.Process
$pr.Process = { param($i) $counter.n++; if ($counter.n -le 1) { $first } else { [pscustomobject]@{ Pid = 4242; Name = 'acad'; StartUtc = '2026-10-01T15:30:00.000Z'; MainStatus = 'OBSERVED'; MainPath = $first.MainPath; FileVersion = '1'; ProductVersion = '1' } } }.GetNewClosure()
Assert-Refused 'pid reused / process changed during the capture' { Invoke-Phase2Capture -Params $p -P $pr } 'changed during the capture'
$counter.n = 0
$pr.Process = { param($i) $counter.n++; if ($counter.n -le 1) { $first } else { $null } }.GetNewClosure()
Assert-Refused 'process ended during the capture' { Invoke-Phase2Capture -Params $p -P $pr } 'changed during the capture'

# test hook semantics (record level)
$st = New-FakeState -Dir $dir -ProcessName 'standinx'; $p = New-TestParams -Dir $dir -Ds $ds -St $st; $p.ExpectedProcessName = 'standinx'; $p.TestHookName = 'standinx'
$r = Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st)
Assert-Equal 'hook active: verdict is never PHASE2_EXTERNAL_OBSERVATIONS_VALID' 'PHASE2_TEST_STANDIN_OBSERVATIONS_OK' $r['validation']['verdict']
Assert-Equal 'hook active: recorded' $true $r['testHook']['active']
$envBackup = [Environment]::GetEnvironmentVariable('CT21D_PHASE2_TEST_STANDIN')
[Environment]::SetEnvironmentVariable('CT21D_PHASE2_TEST_STANDIN', $null)
$v = Test-RecordOffline -Record $r
Assert-True 'hook record refused by the validator when the variable is not set' (-not $v.Valid)
Assert-True '... through the TEST_HOOK check' (($v.Lines -join "`n") -match 'CHECK TEST_HOOK_NOT_IN_PRODUCTION_RECORD FAIL')
[Environment]::SetEnvironmentVariable('CT21D_PHASE2_TEST_STANDIN', 'standinx')
$v = Test-RecordOffline -Record $r
Assert-True 'hook record accepted for test purposes when the variable names the same stand-in' $v.Valid ($v.Lines -join ' | ')
[Environment]::SetEnvironmentVariable('CT21D_PHASE2_TEST_STANDIN', $envBackup)
# a production-looking record that claims a non-acad expected name without the hook
$forged = Copy-Rec; $forged['tupleInputs']['expectedProcessName'] = 'ping'; $forged['process']['name'] = 'ping'
$v = Test-RecordOffline -Record $forged
Assert-True 'a record expecting a non-acad name without the hook is invalid' (-not $v.Valid)
# the in-process name check cannot be bypassed by claiming the hook in the record alone
$forged = Copy-Rec; $forged['tupleInputs']['expectedProcessName'] = 'ping'; $forged['process']['name'] = 'ping'; $forged['testHook']['active'] = $true; $forged['testHook']['processName'] = 'other'
Assert-Equal 'hook name must equal the expected name' 'FAIL' (Get-CheckResult -Record ([ordered]@{ validation = [ordered]@{ checks = (Get-Phase2Checks -Record $forged) } }) -Name 'PROCESS_NAME_IS_EXPECTED')

# ======================================================================================================================
Start-TestSection 'determinism'
$dir = New-CaseDir; $ds = New-TestDeclaredSet -Dir $dir; $st = New-FakeState -Dir $dir; $p = New-TestParams -Dir $dir -Ds $ds -St $st
$a = Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st)
$st.Now = '2026-10-02T01:02:03.004Z'
$b = Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st)
function Remove-Volatile { param($Rec) $c = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $Rec); $c.Remove('volatile'); return $c }
Assert-True 'the declared volatile field differs between the two captures' ($a['volatile']['capturedAtUtc'] -ne $b['volatile']['capturedAtUtc'])
Assert-Equal 'same inputs: byte-identical apart from the volatile object' (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $a)) (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $b))
$bytesA = ConvertTo-RecordBytes -Record $a
Assert-Equal 'serialization of one record is repeatable' (Get-Phase2BytesSha256 -Bytes $bytesA) (Get-Phase2BytesSha256 -Bytes (ConvertTo-RecordBytes -Record $a))
$st.Modules = [pscustomobject]@{ Ok = $true; Items = @($st.Modules.Items)[4, 3, 2, 1, 0] }
$m = Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st)
Assert-Equal 'module input order does not change the record' (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $a)['modules']) (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $m)['modules'])
$st.Processes = [pscustomobject]@{ Ok = $true; Items = @($st.Processes.Items)[2, 1, 0] }
$pp = Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st)
Assert-Equal 'process input order does not change the record' (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $a)['processList']) (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $pp)['processList'])
Assert-Equal 'the whole record is identical after all re-orderings (apart from volatile)' (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $a)) (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile $pp))

# ======================================================================================================================
Start-TestSection 'paths, root hygiene and the single writer'
$base = New-CaseDir
$run = 'HGP-H1-20261001T150000Z-01'
$root = Join-Path $base $run; [void][System.IO.Directory]::CreateDirectory($root)
Assert-Equal 'a valid evidence root is accepted' $root (Assert-Phase2EvidenceRoot -Path $root -RunId $run)
Assert-Refused 'relative path' { Assert-Phase2EvidenceRoot -Path ('.\' + $run) -RunId $run } 'drive-letter'
Assert-Refused 'UNC path' { Assert-Phase2EvidenceRoot -Path ('\\localhost\c$\' + $run) -RunId $run } 'UNC'
Assert-Refused 'UNC path with forward slashes' { Assert-Phase2EvidenceRoot -Path ('//localhost/c$/' + $run) -RunId $run } 'UNC'
Assert-Refused 'device path' { Assert-Phase2EvidenceRoot -Path ('\\?\C:\' + $run) -RunId $run } 'UNC or device'
Assert-Refused 'dot-dot segment' { Assert-Phase2EvidenceRoot -Path ($base + '\..\' + [System.IO.Path]::GetFileName($base) + '\' + $run) -RunId $run } 'dot segment'
Assert-Refused 'alternate data stream' { Assert-Phase2EvidenceRoot -Path ($root + ':stream') -RunId $run } 'stream'
Assert-Refused 'wildcard character' { Assert-Phase2EvidenceRoot -Path ($root + '*') -RunId $run } 'forbidden character'
Assert-Refused 'empty' { Assert-Phase2EvidenceRoot -Path '' -RunId $run } 'empty'
Assert-Refused 'non-existent root (the script creates no folder)' { Assert-Phase2EvidenceRoot -Path (Join-Path $base 'HGP-H1-20261001T150000Z-02') -RunId 'HGP-H1-20261001T150000Z-02' } 'does not exist'
Assert-Refused 'root whose leaf is not the run id (path outside the child root)' { Assert-Phase2EvidenceRoot -Path $base -RunId $run } 'run id'
$fileRoot = Join-Path $base 'afile'; Write-TestFile -Path $fileRoot -Bytes (Get-TestUtf8 'x')
Assert-Refused 'root is a file' { Assert-Phase2EvidenceRoot -Path $fileRoot -RunId 'afile' } 'does not exist or is not a directory'
# reparse points
$target = Join-Path $base 'target'; [void][System.IO.Directory]::CreateDirectory($target)
$junc = Join-Path $base $run.Replace('-01', '-07')
[void](New-Item -ItemType Junction -Path $junc -Target $target)
Assert-Refused 'root is a junction' { Assert-Phase2EvidenceRoot -Path $junc -RunId ([System.IO.Path]::GetFileName($junc)) } 'reparse point'
$jparent = Join-Path $base 'jparent'; [void](New-Item -ItemType Junction -Path $jparent -Target $base)
Assert-Refused 'an ancestor of the root is a junction' { Assert-Phase2EvidenceRoot -Path (Join-Path $jparent $run) -RunId $run } 'reparse point'
$sym = Join-Path $base 'sym-HGP'
$symOk = $true; try { [void](New-Item -ItemType SymbolicLink -Path $sym -Target $target -ErrorAction Stop) } catch { $symOk = $false }
if ($symOk) { Assert-Refused 'root is a symbolic link' { Assert-Phase2EvidenceRoot -Path $sym -RunId 'sym-HGP' } 'reparse point' } else { Write-Host 'NOTE symbolic link creation not permitted here; junction cases cover the reparse branch' }
# the writer
$h = Write-Phase2NewFile -RootFullPath $root -Name 'a.json' -Bytes (Get-TestUtf8 "x`n")
Assert-Equal 'writer returns the sha256 of the bytes' (Get-Phase2BytesSha256 -Bytes (Get-TestUtf8 "x`n")) $h
Assert-Equal 'writer wrote exactly those bytes' $h (Get-Phase2FileSha256 -Path (Join-Path $root 'a.json'))
Assert-Refused 'writer refuses to overwrite' { Write-Phase2NewFile -RootFullPath $root -Name 'a.json' -Bytes (Get-TestUtf8 'y') } 'overwrite'
Assert-Equal 'the existing file is unchanged after the refusal' $h (Get-Phase2FileSha256 -Path (Join-Path $root 'a.json'))
[void][System.IO.Directory]::CreateDirectory((Join-Path $root 'dirname'))
Assert-Refused 'writer refuses a name held by a directory' { Write-Phase2NewFile -RootFullPath $root -Name 'dirname' -Bytes (Get-TestUtf8 'y') } 'overwrite'
foreach ($badName in @('..\x.json', 'sub\x.json', 'sub/x.json', '.hidden', 'a b.json', 'x:y', '', 'a..b', ('n' * 130))) {
    Assert-Refused ('writer refuses the name [' + $badName + ']') { Write-Phase2NewFile -RootFullPath $root -Name $badName -Bytes (Get-TestUtf8 'y') } 'bad evidence file name'
}
Assert-Refused 'writer refuses a root that is a junction' { Write-Phase2NewFile -RootFullPath $junc -Name 'z.json' -Bytes (Get-TestUtf8 'y') } 'reparse'
Assert-Equal 'nothing was written through the junction' 0 @(Get-ChildItem -LiteralPath $target -Force).Count
Assert-Refused 'writer refuses a UNC root' { Write-Phase2NewFile -RootFullPath '\\localhost\c$\x' -Name 'z.json' -Bytes (Get-TestUtf8 'y') } 'UNC'
Assert-Equal 'only a.json and the directory exist in the root' 'a.json,dirname' ((Get-ChildItem -LiteralPath $root -Force | ForEach-Object { $_.Name } | Sort-Object) -join ',')
Assert-Refused 'input file in a UNC path' { Assert-Phase2InputFile -Path '\\localhost\c$\x.txt' -What 'TranscriptPath' } 'UNC'
Assert-Refused 'input file that does not exist' { Assert-Phase2InputFile -Path (Join-Path $base 'nope.txt') -What 'TranscriptPath' } 'does not exist'
Assert-Refused 'input file under a junction' { Assert-Phase2InputFile -Path (Join-Path $jparent 'afile') -What 'TranscriptPath' } 'reparse'
Assert-Refused 'declared-set path through a drive-relative form' { Assert-Phase2InputFile -Path 'C:declared-set.json' -What 'DeclaredSetJson' } 'drive-letter'

# ======================================================================================================================
Start-TestSection 'offline validator: tampering and cross-field rules'
$base2 = Invoke-FakeCapture
function Assert-OfflineInvalid {
    param([string]$Name, [scriptblock]$Mutate, [string]$Check)
    $rec = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $base2)
    & $Mutate $rec
    $v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rec) -Schema $schema -Expected (New-ExpectedFor -Record $base2)
    Assert-True ($Name + ' -> invalid') (-not $v.Valid)
    if ($Check) { Assert-True ($Name + ' -> ' + $Check + ' FAIL') (($v.Lines -join "`n") -match ('CHECK ' + $Check + ' FAIL')) (($v.Lines | Select-Object -First 12) -join ' | ') }
}
Assert-OfflineInvalid 'captureBindingId altered' { param($r) $r['captureBinding']['captureBindingId'] = ('0' * 16) } 'CAPTURE_BINDING_DERIVATION'
Assert-OfflineInvalid 'pid altered after the fact' { param($r) $r['process']['pid'] = 4243 } 'CAPTURE_BINDING_DERIVATION'
Assert-OfflineInvalid 'run id not matching the session activity' { param($r) $r['runId'] = 'HGP-H9-20261001T150000Z-01' } 'RUN_ID_MATCHES_SESSION'
Assert-OfflineInvalid 'evidence root leaf is not the run id' { param($r) $r['volatile']['evidenceRoot'] = 'C:\x\other' } 'EVIDENCE_ROOT_LEAF_IS_RUN_ID'
Assert-OfflineInvalid 'a check result flipped to FAIL while the verdict says valid' { param($r) $r['validation']['checks'][3]['result'] = 'FAIL' } 'CHECKS_RECOMPUTED_EQUAL'
Assert-OfflineInvalid 'verdict says invalid' { param($r) $r['validation']['verdict'] = 'PHASE2_INVALID' } 'VERDICT_CONSISTENT'
Assert-OfflineInvalid 'checks reordered' { param($r) $c = $r['validation']['checks']; $t = $c[0]; $c[0] = $c[1]; $c[1] = $t } 'CHECK_LIST_COMPLETE_AND_ORDERED'
Assert-OfflineInvalid 'a check removed' { param($r) $r['validation']['checks'] = @($r['validation']['checks'][1..23]) } 'CHECK_LIST_COMPLETE_AND_ORDERED'
Assert-OfflineInvalid 'DLL hash edited in the record but not the check' { param($r) $r['pinCheck']['entries'][0]['actualSha256'] = ('e' * 64) } 'CHECKS_RECOMPUTED_EQUAL'
Assert-OfflineInvalid 'module sha edited so the main module no longer matches' { param($r) $r['modules']['items'][0]['sha256'] = ('e' * 64) } 'CHECKS_RECOMPUTED_EQUAL'
Assert-OfflineInvalid 'extra property' { param($r) $r['x'] = 1 } 'SCHEMA'
Assert-OfflineInvalid 'governing flipped' { param($r) $r['governing'] = $true } 'SCHEMA'
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $base2) -Schema $schema -Expected (New-ExpectedFor -Record $base2 -Override @{ acadExeSha256 = ('f' * 64) })
Assert-True 'independent expected acad hash differs from the record echo' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_ACAD_SHA256 FAIL')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $base2) -Schema $schema -Expected (New-ExpectedFor -Record $base2 -Override @{ declaredSetSha256 = ('f' * 64) })
Assert-True 'independent expected declared-set hash differs' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_DECLARED_SET_SHA256 FAIL')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $base2) -Schema $schema -Expected (New-ExpectedFor -Record $base2 -Override @{ profileExpected = 'Another' })
Assert-True 'independent expected profile differs' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_PROFILE FAIL')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $base2) -Schema $schema -Expected (New-ExpectedFor -Record $base2 -Override @{ runId = 'HGP-H1-20261001T150000Z-02' })
Assert-True 'independent expected run id differs' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_RUN_ID FAIL')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $base2) -Schema $schema -Expected (New-ExpectedFor -Record $base2 -Override @{ session = 'S1-B' })
Assert-True 'independent expected session differs' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_SESSION FAIL')
$good2 = ConvertTo-RecordBytes -Record $base2
$bom = [byte[]](0xEF, 0xBB, 0xBF) + $good2
$v = Test-Phase2RecordBytes -RecordBytes $bom -Schema $schema -Expected (New-ExpectedFor -Record $base2)
Assert-True 'BOM refused' (-not $v.Valid -and $v.Lines[0] -match 'BYTES_STRICT_JSON FAIL')
$crlf = (New-Object System.Text.UTF8Encoding($false)).GetBytes(((New-Object System.Text.UTF8Encoding($false)).GetString($good2)).Replace("`n", "`r`n"))
$v = Test-Phase2RecordBytes -RecordBytes $crlf -Schema $schema -Expected (New-ExpectedFor -Record $base2)
Assert-True 'CRLF refused' (-not $v.Valid -and $v.Lines[0] -match 'BYTES_STRICT_JSON FAIL')
$v = Test-Phase2RecordBytes -RecordBytes ($good2 + [byte[]](0x0A)) -Schema $schema -Expected (New-ExpectedFor -Record $base2)
Assert-True 'extra trailing LF -> not canonical' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CANONICAL_FORM FAIL')
$spaced = (New-Object System.Text.UTF8Encoding($false)).GetBytes(((New-Object System.Text.UTF8Encoding($false)).GetString($good2)).Replace('": ', '":'))
$v = Test-Phase2RecordBytes -RecordBytes $spaced -Schema $schema -Expected (New-ExpectedFor -Record $base2)
Assert-True 'reformatted JSON -> not canonical' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CANONICAL_FORM FAIL')
$dup = (New-Object System.Text.UTF8Encoding($false)).GetBytes('{"a":1,"a":2}' + "`n")
$v = Test-Phase2RecordBytes -RecordBytes $dup -Schema $schema -Expected (New-ExpectedFor -Record $base2)
Assert-True 'duplicate keys refused' (-not $v.Valid)
# offline re-check of the declared set on disk and the tool ledger
$dir = New-CaseDir; $ds = New-TestDeclaredSet -Dir $dir; $st = New-FakeState -Dir $dir; $p = New-TestParams -Dir $dir -Ds $ds -St $st
$rr = Invoke-Phase2Capture -Params $p -P (New-FakeProviders -St $st)
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rr) -Schema $schema -Expected (New-ExpectedFor -Record $rr) -DeclaredSetJson $ds.Json
Assert-True 'offline: declared set unchanged since capture' ($v.Valid -and ($v.Lines -join "`n") -match 'OFFLINE_DECLARED_SET_UNCHANGED_SINCE_CAPTURE PASS')
Write-TestFile -Path $ds.Entries[0].path -Bytes (Get-TestUtf8 'drifted after capture')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rr) -Schema $schema -Expected (New-ExpectedFor -Record $rr) -DeclaredSetJson $ds.Json
Assert-True 'offline: a DLL changed after the capture is detected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'OFFLINE_DECLARED_SET_UNCHANGED_SINCE_CAPTURE FAIL')
$ledger = Join-Path $dir 'HASHES.txt'
Write-TestFile -Path $ledger -Bytes (Get-TestUtf8 (New-TestLedgerText))
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rr) -Schema $schema -Expected (New-ExpectedFor -Record $rr) -HashesLedgerPath $ledger -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'tool identity found in the hash ledger' ($v.Valid -and ($v.Lines -join "`n") -match 'TOOL_IDENTITY_IN_HASHES_LEDGER PASS') ($v.Lines -join ' | ')
Write-TestFile -Path $ledger -Bytes (Get-TestUtf8 (New-TestLedgerText -Capture ('9' * 64)))
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rr) -Schema $schema -Expected (New-ExpectedFor -Record $rr) -HashesLedgerPath $ledger -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'tool identity not in the ledger -> invalid' (-not $v.Valid -and ($v.Lines -join "`n") -match 'TOOL_IDENTITY_IN_HASHES_LEDGER FAIL')

# ======================================================================================================================
Start-TestSection 'end to end with a stand-in process (real providers, scripts run as subprocesses)'
$pwshExe = (Get-Process -Id $PID).Path
$captureScript = Join-Path $script:ToolRoot 'Capture-Phase2.ps1'
$validateScript = Join-Path $script:ToolRoot 'Validate-Phase2.ps1'
$sd = New-CaseDir
$standName = 'ct21dstandin' + [guid]::NewGuid().ToString('N').Substring(0, 6)
$standExe = Join-Path $sd ($standName + '.exe')
Copy-Item -LiteralPath (Join-Path $env:SystemRoot 'System32\ping.exe') -Destination $standExe
$standProc = Start-Process -FilePath $standExe -ArgumentList '-n', '600', '127.0.0.1' -PassThru -WindowStyle Hidden
try {
    Start-Sleep -Milliseconds 1200
    $eds = New-TestDeclaredSet -Dir $sd
    $standSha = Get-Phase2FileSha256 -Path $standExe
    # a ledger made from the files of this tool folder as they are NOW (the real HASHES-PHASE2.txt is regenerated after the last edit)
    $e2eLedger = Join-Path $sd 'ledger.txt'
    $ledgerLines = New-Object 'System.Collections.Generic.List[string]'
    $rootLen0 = $script:ToolRoot.TrimEnd('\').Length + 1
    foreach ($lf in (Get-ChildItem -LiteralPath $script:ToolRoot -Recurse -Force -File)) {
        $rel0 = $lf.FullName.Substring($rootLen0).Replace('\', '/')
        if ($rel0 -ceq 'HASHES-PHASE2.txt') { continue }
        $ledgerLines.Add((Get-Phase2FileSha256 -Path $lf.FullName) + '  ' + $rel0)
    }
    Write-TestFile -Path $e2eLedger -Bytes (Get-TestUtf8 ((($ledgerLines.ToArray() | Sort-Object { $_.Substring(66) }) -join "`n") + "`n"))
    $regKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($script:Phase2DefaultProfilesSubKey, $false)
    $regProfile = $null; if ($null -ne $regKey) { $regProfile = $regKey.GetValue('', $null); $regKey.Dispose() }
    $haveProfile = -not [string]::IsNullOrEmpty($regProfile)
    if (-not $haveProfile) { Write-Host 'NOTE no AutoCAD profile key on this machine: the e2e happy path is skipped (registry fallback covered by fakes)' ; $regProfile = 'none' }
    function Invoke-Tool {
        param([string]$Script, [string[]]$Arguments, [hashtable]$Env = @{})
        # round 4: -AllowedEvidenceParent is mandatory; unless a case passes its own, the parent of the case's -EvidenceRoot is used
        if ($Script -eq $captureScript -and $Arguments -notcontains '-AllowedEvidenceParent') {
            $ix = [array]::IndexOf($Arguments, '-EvidenceRoot')
            if ($ix -ge 0 -and $ix + 1 -lt $Arguments.Count) {
                $pp = $null; try { $pp = Split-Path -Parent $Arguments[$ix + 1] } catch { $pp = $null }
                if ($pp) { $Arguments = @($Arguments) + @('-AllowedEvidenceParent', $pp) }
            }
        }
        foreach ($k in $Env.Keys) { [Environment]::SetEnvironmentVariable($k, $Env[$k]) }
        try {
            $out = & $pwshExe -NoProfile -NonInteractive -File $Script @Arguments 2>&1
            return [pscustomobject]@{ Exit = $LASTEXITCODE; Out = ($out | ForEach-Object { [string]$_ }) -join "`n" }
        } finally { foreach ($k in $Env.Keys) { [Environment]::SetEnvironmentVariable($k, $null) } }
    }
    function New-RunRoot { param([string]$Run) $d = Join-Path (New-CaseDir) $Run; [void][System.IO.Directory]::CreateDirectory($d); return $d }
    $run1 = 'HGP-H1-20261001T150000Z-01'
    $e2eSessionId = 'SES-E2E-0001'
    $common = @('-ProcessId', [string]$standProc.Id, '-Session', 'S1-A', '-SessionId', $e2eSessionId, '-RunId', $run1, '-DeclaredSetJson', $eds.Json, '-ExpectedDeclaredSetSha256', $eds.Sha256, '-ExpectedAcadSha256', $standSha, '-ExpectedProfile', $regProfile)
    $hook = @{ CT21D_PHASE2_TEST_STANDIN = $standName }

    if ($haveProfile) {
        $root1 = New-RunRoot $run1
        $res = Invoke-Tool $captureScript (@('-EvidenceRoot', $root1) + $common) $hook
        Assert-Equal 'e2e happy path (test hook): exit 3, never 0' 3 $res.Exit
        Assert-True 'e2e happy path: verdict line' ($res.Out -match 'VERDICT PHASE2_TEST_STANDIN_OBSERVATIONS_OK') $res.Out
        $recPath = Join-Path $root1 ('phase2-record-' + $run1 + '.json')
        Assert-True 'e2e: record written' ([System.IO.File]::Exists($recPath))
        Assert-True 'e2e: sidecar written' ([System.IO.File]::Exists($recPath + '.sha256'))
        Assert-Equal 'e2e: only the two files exist in the root' 2 @(Get-ChildItem -LiteralPath $root1 -Force).Count
        $recSha = Get-Phase2FileSha256 -Path $recPath
        Assert-Equal 'e2e: sidecar content' ($recSha + '  phase2-record-' + $run1 + ".json`n") ([System.IO.File]::ReadAllText($recPath + '.sha256'))
        $rb = [System.IO.File]::ReadAllBytes($recPath)
        Assert-True 'e2e: record has no BOM, no CR, ends with LF' ($rb[0] -ne 0xEF -and -not ((New-Object System.Text.UTF8Encoding($false)).GetString($rb)).Contains("`r") -and $rb[$rb.Length - 1] -eq 10)
        $rec = ConvertFrom-Phase2Json -Text ((New-Object System.Text.UTF8Encoding($false, $true)).GetString($rb))
        Assert-Equal 'e2e: pid recorded' $standProc.Id $rec['process']['pid']
        Assert-Equal 'e2e: process name recorded' $standName $rec['process']['name']
        Assert-Equal 'e2e: start time equals the process object' $standProc.StartTime.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fff''Z''') $rec['process']['startUtc']
        Assert-Equal 'e2e: main module sha256' $standSha $rec['acadBuild']['sha256']
        Assert-Equal 'e2e: profile from the registry default' $regProfile $rec['profile']['effective']
        Assert-True 'e2e: module inventory is non-empty and hashed' ($rec['modules']['count'] -gt 3 -and @($rec['modules']['items'] | Where-Object { $_['hashStatus'] -ne 'OK' }).Count -eq 0)
        Assert-Equal 'e2e: tool script hash recorded' (Get-Phase2FileSha256 -Path $captureScript) $rec['tool']['scriptSha256']
        Assert-Equal 'e2e: tool library hash recorded' (Get-Phase2FileSha256 -Path (Join-Path $script:ToolRoot 'lib/Phase2Common.ps1')) $rec['tool']['librarySha256']
        Assert-Equal 'e2e: record verdict' 'PHASE2_TEST_STANDIN_OBSERVATIONS_OK' $rec['validation']['verdict']
        Assert-Equal 'e2e: captureBindingId matches the documented derivation' (Get-Phase2CaptureBindingId -ProcessIdValue $standProc.Id -StartUtc $rec['process']['startUtc'] -RunId $run1) $rec['captureBinding']['captureBindingId']
        Assert-Equal 'e2e: the assigned sessionId is the one given on the command line' $e2eSessionId $rec['tupleInputs']['sessionId']
        $expPath = Join-Path $sd 'expected.json'
        Write-TestFile -Path $expPath -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value (New-ExpectedFor -Record $rec)))
        $vArgs = @($recPath, '-Schema', $schemaPath, '-ExpectedBuildTupleInputs', $expPath, '-HashesLedger', $e2eLedger, '-DeclaredSetJson', $eds.Json)
        $vr = Invoke-Tool $validateScript $vArgs $hook
        Assert-Equal 'e2e validator with the hook variable: exit 3 (valid only as a test hook record)' 3 $vr.Exit
        Assert-True 'e2e validator: RESULT VALID' ($vr.Out -match 'RESULT VALID') $vr.Out
        Assert-True 'e2e validator: the folder is listed again at validation time' ($vr.Out -match 'CHECK OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW PASS') $vr.Out
        Assert-True 'e2e validator: the schema files are pinned to the ledger' ($vr.Out -match 'CHECK SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD PASS' -and $vr.Out -match 'CHECK EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER PASS') $vr.Out
        Assert-True 'e2e validator: the ledger hash is printed' ($vr.Out -match ('LEDGER_SHA256 ' + (Get-Phase2FileSha256 -Path $e2eLedger))) $vr.Out
        $vr = Invoke-Tool $validateScript $vArgs
        Assert-Equal 'e2e validator without the hook variable refuses the stand-in record: exit 2' 2 $vr.Exit
        Assert-True 'e2e validator: RESULT INVALID' ($vr.Out -match 'RESULT INVALID')
        # tamper with the file on disk
        $tam = Join-Path $sd 'tampered.json'
        $txt = (New-Object System.Text.UTF8Encoding($false)).GetString($rb)
        $i0 = $txt.IndexOf('"result": "PASS"'); $txtT = $txt.Substring(0, $i0) + '"result": "FAIL"' + $txt.Substring($i0 + 16)
        Write-TestFile -Path $tam -Bytes (Get-TestUtf8 $txtT)
        $vr = Invoke-Tool $validateScript (@($tam) + $vArgs[1..($vArgs.Count - 1)]) $hook
        Assert-Equal 'e2e validator: tampered record exit 2' 2 $vr.Exit
        # usage errors of the validator
        Assert-Equal 'validator: no arguments -> exit 1' 1 (Invoke-Tool $validateScript @()).Exit
        Assert-Equal 'validator: missing file -> exit 1' 1 (Invoke-Tool $validateScript @((Join-Path $sd 'nope.json'), '-Schema', $schemaPath, '-ExpectedBuildTupleInputs', $expPath, '-HashesLedger', $e2eLedger)).Exit
        Assert-Equal 'validator: missing -ExpectedBuildTupleInputs -> exit 1' 1 (Invoke-Tool $validateScript @($recPath, '-Schema', $schemaPath, '-HashesLedger', $e2eLedger)).Exit
        Assert-Equal 'validator: missing -HashesLedger -> exit 1 (mandatory)' 1 (Invoke-Tool $validateScript @($recPath, '-Schema', $schemaPath, '-ExpectedBuildTupleInputs', $expPath, '-DeclaredSetJson', $eds.Json) $hook).Exit
        Assert-Equal 'validator: missing -DeclaredSetJson -> exit 1 (mandatory, round 2)' 1 (Invoke-Tool $validateScript @($recPath, '-Schema', $schemaPath, '-ExpectedBuildTupleInputs', $expPath, '-HashesLedger', $e2eLedger) $hook).Exit
        Assert-Equal 'validator: relative record path -> exit 1 (absolute local paths only)' 1 (Invoke-Tool $validateScript @(('..\' + (Split-Path -Leaf (Split-Path -Parent $recPath)) + '\' + (Split-Path -Leaf $recPath)), '-Schema', $schemaPath, '-ExpectedBuildTupleInputs', $expPath, '-HashesLedger', $e2eLedger, '-DeclaredSetJson', $eds.Json) $hook).Exit
        Write-TestFile -Path (Join-Path $sd 'badexp.json') -Bytes (Get-TestUtf8 '{"schema":"x"}')
        Assert-Equal 'validator: expected inputs not matching their schema -> exit 1' 1 (Invoke-Tool $validateScript @($recPath, '-Schema', $schemaPath, '-ExpectedBuildTupleInputs', (Join-Path $sd 'badexp.json'), '-HashesLedger', $e2eLedger) $hook).Exit

        # output exists: a second run into the same root is refused and changes nothing
        $res2 = Invoke-Tool $captureScript (@('-EvidenceRoot', $root1) + $common) $hook
        Assert-Equal 'e2e: second run into the same root is refused (exit 1)' 1 $res2.Exit
        Assert-True 'e2e: refusal names the existing output' ($res2.Out -match 'already exists') $res2.Out
        Assert-Equal 'e2e: the first record is unchanged' $recSha (Get-Phase2FileSha256 -Path $recPath)
        Assert-Equal 'e2e: still only two files' 2 @(Get-ChildItem -LiteralPath $root1 -Force).Count
        # a record that exists but not the sidecar is also refused (no partial overwrite)
        $root1b = New-RunRoot $run1
        Write-TestFile -Path (Join-Path $root1b ('phase2-record-' + $run1 + '.json.sha256')) -Bytes (Get-TestUtf8 'x')
        $res2b = Invoke-Tool $captureScript (@('-EvidenceRoot', $root1b) + $common) $hook
        Assert-Equal 'e2e: pre-existing sidecar is refused (exit 1)' 1 $res2b.Exit
        Assert-Equal 'e2e: no record was created next to it' 1 @(Get-ChildItem -LiteralPath $root1b -Force).Count

        # determinism of a real capture: a second capture of the same process into an equal-named root elsewhere
        $root3 = New-RunRoot $run1
        $res3 = Invoke-Tool $captureScript (@('-EvidenceRoot', $root3) + $common) $hook
        Assert-Equal 'e2e determinism: second capture exit 3 (test hook)' 3 $res3.Exit
        $rec3 = ConvertFrom-Phase2Json -Text ([System.IO.File]::ReadAllText((Join-Path $root3 ('phase2-record-' + $run1 + '.json')), $strictUtf8))
        foreach ($sec in 'captureBinding', 'tool', 'testHook', 'tupleInputs', 'process', 'acadBuild', 'profile', 'modules', 'moduleBaseline', 'profileAtRest', 'pinCheck', 'transcript', 'coverage', 'hostToConfirm', 'session', 'runId') {
            Assert-Equal ('e2e determinism: section ' + $sec + ' identical') (ConvertTo-Phase2CanonicalJson -Value $rec[$sec]) (ConvertTo-Phase2CanonicalJson -Value $rec3[$sec])
        }
        Assert-True 'e2e determinism: the live process list is the only intrinsically non-repeatable section (other processes come and go)' $true
    }

    # without the hook the stand-in is not acad: the record is written INVALID (exit 2)
    $root4 = New-RunRoot $run1
    $res4 = Invoke-Tool $captureScript (@('-EvidenceRoot', $root4) + $common)
    Assert-Equal 'e2e: stand-in without the hook -> INVALID record, exit 2' 2 $res4.Exit
    Assert-True 'e2e: PROCESS_NAME_IS_EXPECTED FAIL printed' ($res4.Out -match 'CHECK PROCESS_NAME_IS_EXPECTED FAIL') $res4.Out
    Assert-True 'e2e: record still written as evidence' ([System.IO.File]::Exists((Join-Path $root4 ('phase2-record-' + $run1 + '.json'))))
    $rec4 = ConvertFrom-Phase2Json -Text ([System.IO.File]::ReadAllText((Join-Path $root4 ('phase2-record-' + $run1 + '.json')), $strictUtf8))
    Assert-Equal 'e2e: verdict' 'PHASE2_INVALID' $rec4['validation']['verdict']
    Assert-Equal 'e2e: no hook recorded' $false $rec4['testHook']['active']

    # usage / refusal paths (exit 1, nothing written)
    $rootU = New-RunRoot $run1
    function Assert-Usage {
        param([string]$Name, [string[]]$Arguments, [string]$Match = '', [hashtable]$Env = @{}, [string]$CheckRoot = $rootU)
        $r = Invoke-Tool $captureScript $Arguments $Env
        Assert-Equal ($Name + ': exit 1') 1 $r.Exit
        if ($Match) { Assert-True ($Name + ': message') ($r.Out -match $Match) $r.Out }
        if ($CheckRoot) { Assert-Equal ($Name + ': nothing written') 0 @(Get-ChildItem -LiteralPath $CheckRoot -Force).Count }
    }
    $noPid = @('-Session', 'S1-A', '-SessionId', $e2eSessionId, '-RunId', $run1, '-EvidenceRoot', $rootU, '-DeclaredSetJson', $eds.Json, '-ExpectedDeclaredSetSha256', $eds.Sha256, '-ExpectedAcadSha256', $standSha, '-ExpectedProfile', $regProfile)
    Assert-Usage 'missing PID' $noPid 'ProcessId'
    Assert-Usage 'PID not numeric' (@('-ProcessId', 'abc') + $noPid) 'ProcessId'
    Assert-Usage 'PID zero' (@('-ProcessId', '0') + $noPid) 'ProcessId'
    Assert-Usage 'PID negative' (@('-ProcessId', '-5') + $noPid) 'ProcessId'
    Assert-Usage 'PID of no running process' (@('-ProcessId', '2147483646') + $noPid) 'no running process' $hook
    $withPid = @('-ProcessId', [string]$standProc.Id) + $noPid
    function Replace-Arg { param([string[]]$Args0, [string]$Name, [string]$Value) $o = @($Args0); for ($i = 0; $i -lt $o.Count; $i++) { if ($o[$i] -eq $Name) { $o[$i + 1] = $Value } }; return , $o }
    Assert-Usage 'invalid run id' (Replace-Arg $withPid '-RunId' 'HGP-H1-bad') 'RunId'
    Assert-Usage 'run id of another session activity' (Replace-Arg $withPid '-RunId' 'HGP-H9-20261001T150000Z-01') 'activity number'
    Assert-Usage 'invalid session' (Replace-Arg $withPid '-Session' 'S7') 'Session'
    Assert-Usage 'lower-case session s1-a is refused (ordinal match, round 2)' (Replace-Arg $withPid '-Session' 's1-a') 'Session'
    Assert-Usage 'mixed-case session S1-a is refused' (Replace-Arg $withPid '-Session' 'S1-a') 'Session'
    # a non-empty evidence root is refused and nothing is added to it (round 2, review 2 m1)
    $rootN = New-RunRoot $run1
    Write-TestFile -Path (Join-Path $rootN 'stray.txt') -Bytes (Get-TestUtf8 'x')
    $resN = Invoke-Tool $captureScript (Replace-Arg $withPid '-EvidenceRoot' $rootN) $hook
    Assert-Equal 'e2e: evidence root with a stray file -> exit 1' 1 $resN.Exit
    Assert-True 'e2e: message says the root is not empty' ($resN.Out -match 'not empty') $resN.Out
    Assert-Equal 'e2e: nothing was added to the non-empty root' 1 @(Get-ChildItem -LiteralPath $rootN -Force).Count
    Assert-Usage 'SessionId placeholder from the template is refused' (Replace-Arg $withPid '-SessionId' '<assigned at authorization>') 'SessionId'
    Assert-Usage 'SessionId empty is refused' (Replace-Arg $withPid '-SessionId' '') 'SessionId'
    $noSid = @(); for ($ix = 0; $ix -lt $withPid.Count; $ix++) { if ($withPid[$ix] -eq '-SessionId') { $ix++ } else { $noSid += $withPid[$ix] } }
    Assert-Usage 'SessionId missing is refused' $noSid 'SessionId'
    Assert-Usage 'missing evidence root' (@($withPid | Select-Object -First 0) + @('-ProcessId', [string]$standProc.Id, '-Session', 'S1-A', '-SessionId', $e2eSessionId, '-RunId', $run1, '-DeclaredSetJson', $eds.Json, '-ExpectedDeclaredSetSha256', $eds.Sha256, '-ExpectedAcadSha256', $standSha, '-ExpectedProfile', $regProfile)) 'EvidenceRoot'
    Assert-Usage 'bad expected declared-set hash' (Replace-Arg $withPid '-ExpectedDeclaredSetSha256' 'XYZ') 'ExpectedDeclaredSetSha256'
    Assert-Usage 'bad expected acad hash' (Replace-Arg $withPid '-ExpectedAcadSha256' ('A' * 64)) 'ExpectedAcadSha256'
    Assert-Usage 'path outside the child root (leaf is not the run id)' (Replace-Arg $withPid '-EvidenceRoot' (Split-Path -Parent $rootU)) 'run id' $hook
    Assert-Usage 'UNC evidence root' (Replace-Arg $withPid '-EvidenceRoot' ('\\localhost\c$\' + $run1)) 'UNC' $hook
    Assert-Usage 'relative evidence root' (Replace-Arg $withPid '-EvidenceRoot' ('.\' + $run1)) 'drive-letter' $hook
    $jb = New-CaseDir; $jt = New-CaseDir; $jl = Join-Path $jb $run1; [void](New-Item -ItemType Junction -Path $jl -Target $jt)
    Assert-Usage 'reparse point (junction) evidence root' (Replace-Arg $withPid '-EvidenceRoot' $jl) 'reparse' $hook $jt
    Assert-Usage 'transcript path that does not exist' ($withPid + @('-TranscriptPath', (Join-Path $sd 'nolog.txt'))) 'TranscriptPath' $hook
    Assert-Usage 'transcript on a UNC path' ($withPid + @('-TranscriptPath', '\\localhost\c$\log.txt')) 'UNC' $hook
    Assert-Usage 'declared-set.json missing' (Replace-Arg $withPid '-DeclaredSetJson' (Join-Path $sd 'nodeclared.json')) 'DeclaredSetJson' $hook
    Assert-Usage 'unknown parameter' ($withPid + @('-Nope', '1')) '' $hook
    $badHook = @{ CT21D_PHASE2_TEST_STANDIN = 'bad name!' }
    Assert-Usage 'malformed hook value' $withPid 'CT21D_PHASE2_TEST_STANDIN' $badHook

    # transcript through the script (S1-A, NETLOAD line present -> INVALID, exit 2)
    $tlog = Join-Path $sd 'acad-log.txt'; Write-TestFile -Path $tlog -Bytes (Get-TestUtf8 "Command: NETLOAD`n")
    $root5 = New-RunRoot $run1
    $res5 = Invoke-Tool $captureScript (@('-EvidenceRoot', $root5) + $common + @('-TranscriptPath', $tlog)) $hook
    Assert-Equal 'e2e: S1-A transcript with a load line -> exit 2' 2 $res5.Exit
    Assert-True 'e2e: TRANSCRIPT_CONSISTENT_WITH_SESSION FAIL printed' ($res5.Out -match 'CHECK TRANSCRIPT_CONSISTENT_WITH_SESSION FAIL')
    $root6 = New-RunRoot $run1
    $res6 = Invoke-Tool $captureScript (@('-EvidenceRoot', $root6) + $common + @('-RequireTranscript')) $hook
    Assert-Equal 'e2e: transcript required but missing -> exit 2' 2 $res6.Exit
    Assert-True 'e2e: TRANSCRIPT_REQUIREMENT_MET FAIL printed' ($res6.Out -match 'CHECK TRANSCRIPT_REQUIREMENT_MET FAIL')
}
finally {
    if ($null -ne $standProc -and -not $standProc.HasExited) { Stop-Process -Id $standProc.Id -Force }
}

# The read-only promise, observed: the stand-in test above must not have created anything outside the temp tree. The registry and
# the declared set of the real host are never touched by the tests (they only use temp declared sets).
Assert-True 'tests never wrote into the real host folder D:\I52-CT21D-HOST\evidence' (-not [System.IO.Directory]::Exists('D:\I52-CT21D-HOST\evidence') -or @(Get-ChildItem -LiteralPath 'D:\I52-CT21D-HOST\evidence' -Force | Where-Object { $_.Name -like 'HGP-H1-20261001T150000Z-0*' }).Count -eq 0)
