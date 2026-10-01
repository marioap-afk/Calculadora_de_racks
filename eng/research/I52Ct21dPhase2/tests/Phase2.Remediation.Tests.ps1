# Tests of the fixer round 1 changes (review findings of the S1-A pre-flight): the assigned sessionId vs the derived capture binding id,
# the module baseline comparison and the honest coverage, the sidecar and tool identity checks of the validator, the at-rest profile
# variables, the command line handling, the signer status in volatile, the load-command words, and the small defects. Dot-sourced by
# Run-AllTests.ps1 after Phase2.Tests.ps1 (uses $schema, New-ExpectedFor, Test-RecordOffline and Invoke-FakeCapture from there).

# ======================================================================================================================
Start-TestSection 'remediation: the CAD-manager sessionId is an input, the derived value is a different thing (M1 of review 2)'
$r = Invoke-FakeCapture
Assert-Equal 'the assigned sessionId is recorded in tupleInputs' 'SES-S1-20261001-0001' $r['tupleInputs']['sessionId']
Assert-Equal 'check SESSION_ID_ASSIGNED_WELL_FORMED passes for a real identifier' 'PASS' (Get-CheckResult -Record $r -Name 'SESSION_ID_ASSIGNED_WELL_FORMED')
Assert-True 'the derived binding id has no field named sessionId' (-not $r['captureBinding'].Contains('sessionId'))
foreach ($bad in @('<assigned at authorization>', '', ' ', 'has space', '-leading-dash', ('x' * 129), 'a/b', 'a\b')) {
    $rb = Invoke-FakeCapture { param($st, $p) $p.SessionId = $bad }
    Assert-Equal ('a placeholder or malformed sessionId [' + $bad + '] fails the check') 'FAIL' (Get-CheckResult -Record $rb -Name 'SESSION_ID_ASSIGNED_WELL_FORMED')
    Assert-Equal '... and the verdict is INVALID' 'PHASE2_INVALID' $rb['validation']['verdict']
}
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r -Override @{ sessionId = 'SES-OTHER' })
Assert-True 'the independent expected sessionId (phase 1) must equal the one in the record' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_SESSION_ID FAIL')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r)
Assert-True 'matching expected sessionId is accepted' ($v.Valid -and ($v.Lines -join "`n") -match 'CHECK EXPECTED_SESSION_ID PASS')
# designation file compare (the composed designation carries the phase 1 sessionId and the runId)
$dd = New-CaseDir
$desPath = Join-Path $dd 'run-designation.json'
Write-TestFile -Path $desPath -Bytes (Get-TestUtf8 ('{"schema":"ct21d.designation.v1","runId":"' + $r['runId'] + '","attempt":1,"sessionId":"SES-S1-20261001-0001","tolerance":1.5}' + "`n"))
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r) -DesignationJson $desPath
Assert-True 'designation with the same sessionId and runId (and a non-integer number elsewhere) is accepted' ($v.Valid -and ($v.Lines -join "`n") -match 'DESIGNATION_SESSION_ID_EQUALS_RECORD PASS' -and ($v.Lines -join "`n") -match 'DESIGNATION_RUN_ID_EQUALS_RECORD PASS') ($v.Lines -join ' | ')
Write-TestFile -Path $desPath -Bytes (Get-TestUtf8 ('{"runId":"' + $r['runId'] + '","sessionId":"SES-DIFFERENT"}' + "`n"))
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r) -DesignationJson $desPath
Assert-True 'designation with another sessionId is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK DESIGNATION_SESSION_ID_EQUALS_RECORD FAIL')
Write-TestFile -Path $desPath -Bytes (Get-TestUtf8 ('{"runId":"HGP-H1-20261001T150000Z-02","sessionId":"SES-S1-20261001-0001"}' + "`n"))
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r) -DesignationJson $desPath
Assert-True 'designation with another runId is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK DESIGNATION_RUN_ID_EQUALS_RECORD FAIL')
Write-TestFile -Path $desPath -Bytes (Get-TestUtf8 'not json')
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r) -DesignationJson $desPath
Assert-True 'an unreadable designation is rejected' (-not $v.Valid)

# ======================================================================================================================
Start-TestSection 'remediation: module baseline comparison and honest coverage (M2 of review 2)'
Assert-Equal 'no baseline: status NOT_PROVIDED' 'NOT_PROVIDED' $r['moduleBaseline']['status']
Assert-Equal 'no baseline: the comparison check passes only because none is required' 'PASS' (Get-CheckResult -Record $r -Name 'MODULE_BASELINE_COMPARISON')
Assert-Equal 'no baseline: coverage says NOT_COMPARED' 'NOT_COMPARED' $r['coverage']['moduleBaselineComparison']
Assert-Equal 'coverage: the template is not complete' $false $r['coverage']['templateComplete']
Assert-Equal 'coverage: sessionProfileFacts stays HOST-TO-CONFIRM' 'HOST-TO-CONFIRM' $r['coverage']['sessionProfileFacts']
Assert-Equal 'coverage: runtimeIdentities stays HOST-TO-CONFIRM' 'HOST-TO-CONFIRM' $r['coverage']['runtimeIdentities']
Assert-Equal 'coverage (round 3, review 2 m1): pinCheckAfterLoad is HOST-TO-CONFIRM' 'HOST-TO-CONFIRM' $r['coverage']['pinCheckAfterLoad']
Assert-True 'host-to-confirm (round 3, review 2 m1): the after-load pin check is named' (@($r['hostToConfirm'] | Where-Object { $_['item'] -eq 'PIN_CHECK_AFTER_LOAD_NOT_COVERED' }).Count -eq 1)
Assert-Equal 'coverage: the verdict scope is stated' 'EXTERNAL_OBSERVATIONS_OF_THIS_TOOL_ONLY' $r['coverage']['verdictCovers']
Assert-True 'the verdict is not called PHASE2_VALID any more' ($r['validation']['verdict'] -ne 'PHASE2_VALID' -and $r['validation']['verdict'] -eq 'PHASE2_EXTERNAL_OBSERVATIONS_VALID')
$rv = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $r); $rv['validation']['verdict'] = 'PHASE2_VALID'
Assert-True 'the old verdict name is rejected by the schema' ((Test-Phase2AgainstSchema -Value $rv -Schema $schema).Count -gt 0)
$rv = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $r); $rv['coverage']['templateComplete'] = $true
Assert-True 'a coverage that claims a complete template is rejected by the schema' ((Test-Phase2AgainstSchema -Value $rv -Schema $schema).Count -gt 0)
$rv = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $r); $rv['coverage']['moduleBaselineComparison'] = 'COMPARED'
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rv) -Schema $schema -Expected (New-ExpectedFor -Record $r)
Assert-True 'a coverage that claims a comparison that did not happen is invalid offline' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK COVERAGE_CONSISTENT FAIL')

# with a baseline equal to the loaded modules
$withBaseline = { param($st, $p, $ds, $dir) $b = New-BaselineFile $dir $st.Modules.Items; $p.ModuleBaselineJson = $b; $p.ExpectedModuleBaselineSha256 = (Get-Phase2FileSha256 -Path $b) }
$rb = Invoke-FakeCapture $withBaseline
Assert-Equal 'baseline equal to the loaded modules: PROVIDED' 'PROVIDED' $rb['moduleBaseline']['status']
Assert-Equal '... 5 entries' 5 $rb['moduleBaseline']['entryCount']
Assert-Equal '... nothing outside the baseline' 0 @($rb['moduleBaseline']['outsideBaseline']).Count
Assert-Equal '... check passes' 'PASS' (Get-CheckResult -Record $rb -Name 'MODULE_BASELINE_COMPARISON')
Assert-Equal '... coverage says COMPARED' 'COMPARED' $rb['coverage']['moduleBaselineComparison']
Assert-Equal '... verdict valid' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $rb['validation']['verdict']
$v = Test-RecordOffline -Record $rb
Assert-True '... the offline validator accepts it' $v.Valid ($v.Lines -join ' | ')
# a module that is not in the baseline (stop condition)
$rx = Invoke-FakeCapture {
    param($st, $p, $ds, $dir)
    $b = New-BaselineFile $dir $st.Modules.Items; $p.ModuleBaselineJson = $b; $p.ExpectedModuleBaselineSha256 = (Get-Phase2FileSha256 -Path $b)
    $extra = Join-Path $dir 'mods\extra-plugin.dll'; Write-TestFile -Path $extra -Bytes (Get-TestUtf8 'extra module')
    $st.Modules = [pscustomobject]@{ Ok = $true; Items = @($st.Modules.Items) + [pscustomobject]@{ Path = $extra; FileVersion = '1' } }
}
Assert-Equal 'module outside the baseline: check FAIL' 'FAIL' (Get-CheckResult -Record $rx -Name 'MODULE_BASELINE_COMPARISON')
Assert-Equal '... listed as NOT_IN_BASELINE' 'NOT_IN_BASELINE' $rx['moduleBaseline']['outsideBaseline'][0]['reason']
Assert-Equal '... verdict INVALID' 'PHASE2_INVALID' $rx['validation']['verdict']
# a module whose hash differs from the baseline
$rh = Invoke-FakeCapture {
    param($st, $p, $ds, $dir)
    $b = New-BaselineFile $dir $st.Modules.Items; $p.ModuleBaselineJson = $b; $p.ExpectedModuleBaselineSha256 = (Get-Phase2FileSha256 -Path $b)
    Write-TestFile -Path $st.Modules.Items[0].Path -Bytes (Get-TestUtf8 'modified after the baseline was taken')
}
Assert-Equal 'module with another hash than the baseline: check FAIL' 'FAIL' (Get-CheckResult -Record $rh -Name 'MODULE_BASELINE_COMPARISON')
Assert-Equal '... listed as HASH_DIFFERS' 'HASH_DIFFERS' $rh['moduleBaseline']['outsideBaseline'][0]['reason']
# the baseline hash must be the one of the signed phase 1 record
$rs = Invoke-FakeCapture { param($st, $p, $ds, $dir) $b = New-BaselineFile $dir $st.Modules.Items; $p.ModuleBaselineJson = $b; $p.ExpectedModuleBaselineSha256 = ('a' * 64) }
Assert-Equal 'baseline file hash differs from the phase 1 value: check FAIL' 'FAIL' (Get-CheckResult -Record $rs -Name 'MODULE_BASELINE_COMPARISON')
# required but not provided
$rr = Invoke-FakeCapture { param($st, $p) $p.RequireModuleBaseline = $true }
Assert-Equal 'baseline required but not provided: check FAIL' 'FAIL' (Get-CheckResult -Record $rr -Name 'MODULE_BASELINE_COMPARISON')
Assert-Equal '... verdict INVALID' 'PHASE2_INVALID' $rr['validation']['verdict']
# malformed baseline files
foreach ($case in @(@('not JSON', 'nope'), @('empty array', '[]'), @('entry without sha256', '[{"path":"C:\\x\\a.dll"}]'), @('relative path', '[{"path":"a.dll","sha256":"' + ('a' * 64) + '"}]'), @('duplicate path (case)', '[{"path":"C:\\x\\a.dll","sha256":"' + ('a' * 64) + '"},{"path":"C:\\X\\A.dll","sha256":"' + ('b' * 64) + '"}]'))) {
    $rm = Invoke-FakeCapture { param($st, $p, $ds, $dir) $b = Join-Path $dir 'baseline.json'; Write-TestFile -Path $b -Bytes (Get-TestUtf8 ($case[1] + "`n")); $p.ModuleBaselineJson = $b; $p.ExpectedModuleBaselineSha256 = (Get-Phase2FileSha256 -Path $b) }
    Assert-Equal ('malformed baseline (' + $case[0] + '): MALFORMED') 'MALFORMED' $rm['moduleBaseline']['status']
    Assert-Equal '... check FAIL' 'FAIL' (Get-CheckResult -Record $rm -Name 'MODULE_BASELINE_COMPARISON')
}
# the independent expected inputs cannot be waived by the record
$vw = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $r) -Schema $schema -Expected (New-ExpectedFor -Record $r -Override @{ requireModuleBaseline = $true })
Assert-True 'expected inputs that REQUIRE a baseline reject a record without one' (-not $vw.Valid -and ($vw.Lines -join "`n") -match 'CHECK EXPECTED_MODULE_BASELINE_PROVIDED FAIL' -and ($vw.Lines -join "`n") -match 'CHECK EXPECTED_REQUIRE_MODULE_BASELINE FAIL')
$vw = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rb) -Schema $schema -Expected (New-ExpectedFor -Record $rb -Override @{ moduleBaselineSha256 = ('c' * 64) })
Assert-True 'expected baseline hash that differs from the record echo is rejected' (-not $vw.Valid -and ($vw.Lines -join "`n") -match 'CHECK EXPECTED_MODULE_BASELINE_SHA256 FAIL')
# offline recomputation from the baseline file
$dirB = New-CaseDir; $dsB = New-TestDeclaredSet -Dir $dirB; $stB = New-FakeState -Dir $dirB; $pB = New-TestParams -Dir $dirB -Ds $dsB -St $stB
$bf = New-BaselineFile $dirB $stB.Modules.Items; $pB.ModuleBaselineJson = $bf; $pB.ExpectedModuleBaselineSha256 = (Get-Phase2FileSha256 -Path $bf)
$recB = Invoke-Phase2Capture -Params $pB -P (New-FakeProviders -St $stB)
$vb = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $recB) -Schema $schema -Expected (New-ExpectedFor -Record $recB) -ModuleBaselineJson $bf
Assert-True 'offline: the baseline file recomputes to the recorded comparison' ($vb.Valid -and ($vb.Lines -join "`n") -match 'OFFLINE_MODULE_BASELINE_RECOMPUTED PASS') ($vb.Lines -join ' | ')
$forged = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $recB)
$forged['modules']['items'] = @($forged['modules']['items'][0..2]); $forged['modules']['count'] = 3
$vf = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $forged) -Schema $schema -Expected (New-ExpectedFor -Record $recB) -ModuleBaselineJson $bf
Assert-True 'offline: a record with modules removed after the fact is invalid' (-not $vf.Valid)
Write-TestFile -Path $bf -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value @([ordered]@{ path = $stB.Modules.Items[0].Path; sha256 = ('d' * 64) })))
$vb = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $recB) -Schema $schema -Expected (New-ExpectedFor -Record $recB) -ModuleBaselineJson $bf
Assert-True 'offline: a baseline file that changed since the capture is detected' (-not $vb.Valid -and ($vb.Lines -join "`n") -match 'CHECK OFFLINE_MODULE_BASELINE_RECOMPUTED FAIL')

# ======================================================================================================================
Start-TestSection 'remediation: the check list, the duplicate module and the count guard'
Assert-Equal 'the fixed check list has 24 named checks' 24 $script:Phase2CheckNames.Count
$rd = Invoke-FakeCapture { param($st) $first = @($st.Modules.Items)[0]; $st.Modules = [pscustomobject]@{ Ok = $true; Items = @($st.Modules.Items) + $first } }
Assert-Equal 'a duplicate module path in the inventory fails MODULE_INVENTORY_SORTED_UNIQUE' 'FAIL' (Get-CheckResult -Record $rd -Name 'MODULE_INVENTORY_SORTED_UNIQUE')
Assert-Equal '... verdict INVALID' 'PHASE2_INVALID' $rd['validation']['verdict']
$unsorted = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $r)
$tmpItem = $unsorted['modules']['items'][0]; $unsorted['modules']['items'][0] = $unsorted['modules']['items'][1]; $unsorted['modules']['items'][1] = $tmpItem
Assert-Equal 'a swapped (unsorted) inventory fails MODULE_INVENTORY_SORTED_UNIQUE when re-derived' 'FAIL' (Get-CheckResult -Record ([ordered]@{ validation = [ordered]@{ checks = (Get-Phase2Checks -Record $unsorted) } }) -Name 'MODULE_INVENTORY_SORTED_UNIQUE')
$vu = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $unsorted) -Schema $schema -Expected (New-ExpectedFor -Record $r)
Assert-True '... and the offline validator rejects it' (-not $vu.Valid -and ($vu.Lines -join "`n") -match 'CHECK CHECKS_RECOMPUTED_EQUAL FAIL')
# offline drift check with a record whose entry count differs: exit 2 path, no exception (review 2 m5)
$dirC = New-CaseDir; $dsC = New-TestDeclaredSet -Dir $dirC; $stC = New-FakeState -Dir $dirC; $pC = New-TestParams -Dir $dirC -Ds $dsC -St $stC
$recC = Invoke-Phase2Capture -Params $pC -P (New-FakeProviders -St $stC)
$cut = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $recC)
$cut['pinCheck']['entries'] = @($cut['pinCheck']['entries'][0..1])
$threw = $false; $vc = $null
try { $vc = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $cut) -Schema $schema -Expected (New-ExpectedFor -Record $recC) -DeclaredSetJson $dsC.Json } catch { $threw = $true }
Assert-True 'offline drift check with fewer recorded entries: no exception' (-not $threw)
Assert-True '... and the record is invalid' ($null -ne $vc -and -not $vc.Valid)

# ======================================================================================================================
Start-TestSection 'remediation: sidecar, tool identity of the validator, mandatory independent inputs (m1 of review 1)'
$recBytes = ConvertTo-RecordBytes -Record $r
$name = 'phase2-record-' + $r['runId'] + '.json'
$goodSide = (Get-Phase2BytesSha256 -Bytes $recBytes) + '  ' + $name + "`n"
$exp = New-ExpectedFor -Record $r
$v = Test-Phase2RecordBytes -RecordBytes $recBytes -Schema $schema -Expected $exp -CheckSidecar -SidecarText $goodSide -RecordFileName $name
Assert-True 'a correct sidecar is accepted' ($v.Valid -and ($v.Lines -join "`n") -match 'CHECK SIDECAR_EQUALS_RECORD_SHA256 PASS')
$v = Test-Phase2RecordBytes -RecordBytes $recBytes -Schema $schema -Expected $exp -CheckSidecar -SidecarText (('0' * 64) + '  ' + $name + "`n") -RecordFileName $name
Assert-True 'a sidecar with another hash is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK SIDECAR_EQUALS_RECORD_SHA256 FAIL')
$v = Test-Phase2RecordBytes -RecordBytes $recBytes -Schema $schema -Expected $exp -CheckSidecar -SidecarText $goodSide -RecordFileName 'other.json'
Assert-True 'a sidecar naming another file is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK SIDECAR_EQUALS_RECORD_SHA256 FAIL')
$v = Test-Phase2RecordBytes -RecordBytes $recBytes -Schema $schema -Expected $exp -CheckSidecar -RecordFileName $name
Assert-True 'a missing sidecar is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK SIDECAR_EQUALS_RECORD_SHA256 FAIL')
$v = Test-Phase2RecordBytes -RecordBytes $recBytes -Schema $schema -Expected $exp -CheckSidecar -SidecarText ($goodSide.TrimEnd("`n")) -RecordFileName $name
Assert-True 'a sidecar without the final LF is rejected' (-not $v.Valid)
# a record edited by hand and re-canonicalized no longer matches the sidecar the capture wrote
$handEdited = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $r); $handEdited['process']['invokingUser'] = 'SOMEONE\else'
$v = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $handEdited) -Schema $schema -Expected $exp -CheckSidecar -SidecarText $goodSide -RecordFileName $name
Assert-True 'a hand-edited, re-canonicalized record is caught by the sidecar' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK SIDECAR_EQUALS_RECORD_SHA256 FAIL')

$dirL = New-CaseDir
$ledgerPath = Join-Path $dirL 'HASHES.txt'
$mk = { param($v, $l) (New-TestLedgerText -Validator $v) }
$rr2 = Invoke-FakeCapture
$recBytes2 = ConvertTo-RecordBytes -Record $rr2
Write-TestFile -Path $ledgerPath -Bytes (Get-TestUtf8 (& $mk ('4' * 64) ('2' * 64)))
$v = Test-Phase2RecordBytes -RecordBytes $recBytes2 -Schema $schema -Expected (New-ExpectedFor -Record $rr2) -HashesLedgerPath $ledgerPath -ValidatorScriptSha256 ('4' * 64) -ValidatorLibrarySha256 ('2' * 64) -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'the running validator and library equal the ledger: accepted' ($v.Valid -and ($v.Lines -join "`n") -match 'VALIDATOR_IDENTITY_IN_HASHES_LEDGER PASS') ($v.Lines -join ' | ')
$v = Test-Phase2RecordBytes -RecordBytes $recBytes2 -Schema $schema -Expected (New-ExpectedFor -Record $rr2) -HashesLedgerPath $ledgerPath -ValidatorScriptSha256 ('9' * 64) -ValidatorLibrarySha256 ('2' * 64) -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'a validator whose hash is not the ledger one is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK VALIDATOR_IDENTITY_IN_HASHES_LEDGER FAIL')
$v = Test-Phase2RecordBytes -RecordBytes $recBytes2 -Schema $schema -Expected (New-ExpectedFor -Record $rr2) -HashesLedgerPath $ledgerPath -ValidatorScriptSha256 ('4' * 64) -ValidatorLibrarySha256 ('8' * 64) -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'a library loaded by the validator whose hash is not the ledger one is rejected' (-not $v.Valid -and ($v.Lines -join "`n") -match 'CHECK VALIDATOR_IDENTITY_IN_HASHES_LEDGER FAIL')

# ======================================================================================================================
Start-TestSection 'remediation: at-rest profile variables, command line, signers (informational and hygiene)'
$ra = Invoke-FakeCapture
Assert-Equal 'at-rest variables are informational' $true $ra['profileAtRest']['informationalOnly']
Assert-Equal 'at-rest SECURELOAD recorded' '1' $ra['profileAtRest']['secureload']['value']
Assert-Equal 'at-rest TRUSTEDPATHS recorded verbatim' 'FAKE-ONE;FAKE-TWO' $ra['profileAtRest']['trustedpaths']['value']
Assert-Equal 'at-rest kind recorded' 'String' $ra['profileAtRest']['trustedpaths']['kind']
Assert-Equal 'the Variables subkey is built from the profiles key and the effective profile' ($script:Phase2DefaultProfilesSubKey + '\<<Unnamed Profile>>\Variables') $ra['profileAtRest']['variablesSubKey']
Assert-True 'the at-rest values are in no check name' (@($ra['validation']['checks'] | Where-Object { $_['name'] -match 'SECURELOAD|TRUSTEDPATHS|AT_REST' }).Count -eq 0)
$st1 = $null
$rk = Invoke-FakeCapture { param($st) $st.ProfileVars = @{ SECURELOAD = [pscustomobject]@{ Status = 'KEY_ABSENT'; Kind = $null; Value = $null }; TRUSTEDPATHS = [pscustomobject]@{ Status = 'UNREADABLE'; Kind = $null; Value = $null } } }
Assert-Equal 'an absent or unreadable at-rest key does not change the verdict (informational)' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $rk['validation']['verdict']
Assert-Equal '... the status is recorded' 'KEY_ABSENT,UNREADABLE' ($rk['profileAtRest']['secureload']['status'] + ',' + $rk['profileAtRest']['trustedpaths']['status'])
$rn = Invoke-FakeCapture { param($st) $st.Registry = [pscustomobject]@{ Status = 'KEY_ABSENT'; Value = $null } }
Assert-Equal 'no effective profile: at-rest read is NOT_ATTEMPTED' 'NOT_ATTEMPTED' $rn['profileAtRest']['secureload']['status']
Assert-True '... and the subkey is null' ($null -eq $rn['profileAtRest']['variablesSubKey'])
$rp = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'OBSERVED'; Value = '"C:\x\acad.exe" /p "a\b"' } }
Assert-Equal 'a profile name with a path separator is never concatenated into a registry path' 'NOT_ATTEMPTED' $rp['profileAtRest']['trustedpaths']['status']

# command line: executable token removed first (review 2 m8), switches recorded (review 1 m5)
$rc = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'OBSERVED'; Value = '"C:\x /p Evil\acad.exe" /nologo' } }
Assert-Equal 'a /p inside a quoted executable path is not read as the switch' 'NOT_PRESENT' $rc['profile']['commandLineProfileStatus']
Assert-Equal '... the effective profile stays the registry default' 'REGISTRY_DEFAULT_AT_CAPTURE' $rc['profile']['effectiveSource']
Assert-Equal 'Get-Phase2CommandLineArguments: quoted executable' ' /nologo' (Get-Phase2CommandLineArguments -CommandLine '"C:\a b\acad.exe" /nologo')
Assert-Equal 'Get-Phase2CommandLineArguments: bare executable' ' /nologo /b x.scr' (Get-Phase2CommandLineArguments -CommandLine 'acad.exe /nologo /b x.scr')
Assert-Equal 'Get-Phase2CommandLineArguments: executable only' '' (Get-Phase2CommandLineArguments -CommandLine 'acad.exe')
Assert-Equal 'Get-Phase2CommandLineArguments: unterminated quote' '' (Get-Phase2CommandLineArguments -CommandLine '"C:\a\acad.exe')
$rw = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'OBSERVED'; Value = '"C:\x\acad.exe" /B C:\s\start.scr /ld C:\x\a.arx /nologo /P "<<Unnamed Profile>>" /b' } }
Assert-Equal 'startup switches are recorded lower-case, unique, ordinal-sorted' '/b,/ld,/nologo,/p' ($rw['process']['commandLineSwitches'] -join ',')
Assert-Equal '... they fail no check (flagged as HOST-TO-CONFIRM facts)' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $rw['validation']['verdict']
Assert-True '... and the host-to-confirm list names them' (@($rw['hostToConfirm'] | Where-Object { $_['item'] -eq 'STARTUP_SWITCHES_RECORDED' }).Count -eq 1)
$rcn = Invoke-FakeCapture { param($st) $st.Command = [pscustomobject]@{ Status = 'NOT_OBSERVABLE'; Value = $null } }
Assert-Equal 'command line not observable: no switches recorded' 0 @($rcn['process']['commandLineSwitches']).Count

# signer status belongs to volatile (review 2 m4)
Assert-True 'module items carry no signer fields' (@($ra['modules']['items'] | Where-Object { $_.Contains('signerStatus') -or $_.Contains('signerSubject') }).Count -eq 0)
Assert-Equal 'the signers are recorded in volatile, one per module' 5 @($ra['volatile']['moduleSigners']).Count
Assert-Equal '... sorted like the modules' (($ra['modules']['items'] | ForEach-Object { $_['path'] }) -join '|') (($ra['volatile']['moduleSigners'] | ForEach-Object { $_['path'] }) -join '|')
$dirS = New-CaseDir; $dsS = New-TestDeclaredSet -Dir $dirS; $stS = New-FakeState -Dir $dirS; $pS = New-TestParams -Dir $dirS -Ds $dsS -St $stS
$s1 = Invoke-Phase2Capture -Params $pS -P (New-FakeProviders -St $stS)
$stS.Signer = [pscustomobject]@{ Status = 'UnknownError'; Subject = $null }
$s2 = Invoke-Phase2Capture -Params $pS -P (New-FakeProviders -St $stS)
function Remove-Volatile2 { param($Rec) $c = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $Rec); $c.Remove('volatile'); return $c }
Assert-Equal 'two captures of the same state with different signer lookups are identical apart from volatile' (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile2 $s1)) (ConvertTo-Phase2CanonicalJson -Value (Remove-Volatile2 $s2))
Assert-True '... and their volatile signers differ' ((ConvertTo-Phase2CanonicalJson -Value $s1['volatile']) -ne (ConvertTo-Phase2CanonicalJson -Value $s2['volatile']))

# the load-command reader
$ll = Get-Phase2LoadLines -Lines @('Command: NETLOAD', 'appload', '(load "a")', ' ( LOAD "a")', '(loader x)', 'overload', 'unloaded', 'arxload', 'x NETLOADED', 'VLRUN')
Assert-Equal 'load lines: command words and AutoLISP load forms' '1,2,3,4,8,10' (($ll | ForEach-Object { $_['line'] }) -join ',')

# ======================================================================================================================
Start-TestSection 'remediation: exit codes and usage of the scripts (no process needed: the checks run before the PID is used)'
$pwshExe2 = (Get-Process -Id $PID).Path
$cap2 = Join-Path $script:ToolRoot 'Capture-Phase2.ps1'
$base2 = @('-ProcessId', '123456', '-Session', 'S1-A', '-SessionId', 'SES-1', '-RunId', 'HGP-H1-20261001T150000Z-01', '-EvidenceRoot', 'C:\nope\HGP-H1-20261001T150000Z-01', '-DeclaredSetJson', 'C:\nope\d.json', '-ExpectedDeclaredSetSha256', ('a' * 64), '-ExpectedAcadSha256', ('b' * 64), '-ExpectedProfile', 'p')
function Invoke-Cap2 { param([string[]]$Arguments) $o = & $pwshExe2 -NoProfile -NonInteractive -File $cap2 @Arguments 2>&1; return [pscustomobject]@{ Exit = $LASTEXITCODE; Out = ($o | ForEach-Object { [string]$_ }) -join "`n" } }
$u = Invoke-Cap2 ($base2 + @('-ModuleBaselineJson', 'C:\nope\b.json'))
Assert-True 'baseline file without the expected baseline hash: usage error' ($u.Exit -eq 1 -and $u.Out -match 'ExpectedModuleBaselineSha256') $u.Out
$u = Invoke-Cap2 ($base2 + @('-ExpectedModuleBaselineSha256', ('c' * 64)))
Assert-True 'expected baseline hash without a baseline file: usage error' ($u.Exit -eq 1 -and $u.Out -match 'without ModuleBaselineJson') $u.Out
$u = Invoke-Cap2 ($base2 + @('-ModuleBaselineJson', 'C:\nope\b.json', '-ExpectedModuleBaselineSha256', 'XYZ'))
Assert-True 'malformed expected baseline hash: usage error' ($u.Exit -eq 1 -and $u.Out -match 'ExpectedModuleBaselineSha256') $u.Out
