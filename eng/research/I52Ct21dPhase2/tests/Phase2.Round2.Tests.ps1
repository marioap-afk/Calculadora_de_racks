# Tests of the fixer round 2 changes: every reproduction of the two round 2 reviews is a negative control here (or in
# StaticScan.Reproductions2.Tests.ps1 for the scan). Dot-sourced by Run-AllTests.ps1 after Phase2.Remediation.Tests.ps1
# (uses $schema, New-ExpectedFor, Test-RecordOffline, Invoke-FakeCapture, New-BaselineFile and the corpus of TestSupport.ps1).

# ======================================================================================================================
Start-TestSection 'round 2: load-command detection (review 1 M2)'
foreach ($line in $script:LoadLinesBoth + $script:LoadLinesTranscriptOnly) {
    $got = Get-Phase2LoadLines -Lines @($line)   # (assignment, not @(...): the function returns the array as one object, an empty one would count as 1)
    Assert-Equal ('detected: [' + $line + ']') 1 @($got).Count
}
foreach ($line in $script:LoadLinesClean) {
    $got = Get-Phase2LoadLines -Lines @($line)
    Assert-Equal ('not a load line: [' + $line + ']') 0 @($got).Count
}
Write-Host ('  ' + ($script:LoadLinesBoth.Count + $script:LoadLinesTranscriptOnly.Count) + ' load-line fixtures, ' + $script:LoadLinesClean.Count + ' clean-line fixtures')
# the reviewer's transcript, through the capture: lineCount=3, loadLines=2, the check fails in S1-A
$rT = Invoke-FakeCapture {
    param($st, $p, $ds, $dir)
    $t = Join-Path $dir 'ribbon.txt'
    Write-TestFile -Path $t -Bytes (Get-TestUtf8 ("Command: _NETLOAD`r`nEnter assembly file name: C:\x\Evil.dll`r`nCommand: _appload`r`n"))
    $p.TranscriptPath = $t
}
Assert-Equal 'reviewer transcript: 3 lines' 3 $rT['transcript']['lineCount']
Assert-Equal 'reviewer transcript: 2 load lines (the _NETLOAD and _appload echoes)' '1,3' (($rT['transcript']['loadLines'] | ForEach-Object { $_['line'] }) -join ',')
Assert-Equal 'reviewer transcript: TRANSCRIPT_CONSISTENT_WITH_SESSION fails' 'FAIL' (Get-CheckResult -Record $rT -Name 'TRANSCRIPT_CONSISTENT_WITH_SESSION')
Assert-Equal 'reviewer transcript: verdict INVALID' 'PHASE2_INVALID' $rT['validation']['verdict']
$rT2 = Invoke-FakeCapture -Session 'S2' {
    param($st, $p, $ds, $dir)
    & $script:WithBaseline $st $p $ds $dir
    $t = Join-Path $dir 'macro.txt'
    Write-TestFile -Path $t -Bytes (Get-TestUtf8 "^C^C_netload`n(vl-load-all `"x.lsp`")`n")
    $p.TranscriptPath = $t
}
Assert-Equal 'a menu-macro netload and a vl-load-all form fail every session (S2)' 'PHASE2_INVALID' $rT2['validation']['verdict']

# ======================================================================================================================
Start-TestSection 'round 2: the writer pins its root and its callers (review 1 m1, m2, m7; review 2 m5)'
$wb = New-CaseDir
$notRun = Join-Path $wb 'notarun'; [void][System.IO.Directory]::CreateDirectory($notRun)
Assert-Refused 'the writer refuses a root whose leaf is not a run id (Write-Phase2NewFile with another root)' { Write-Phase2NewFile -RootFullPath $notRun -Name 'evil.dll' -Bytes ([byte[]]@(1)) } 'run id'
Assert-Equal '... and wrote nothing there' 0 @(Get-ChildItem -LiteralPath $notRun -Force).Count
$runW = 'HGP-H1-20261001T150000Z-05'
$rootW = Join-Path $wb $runW; [void][System.IO.Directory]::CreateDirectory($rootW)
Assert-Refused 'the pair writer refuses a root whose leaf differs from the run id' { Write-Phase2RecordAndSidecar -RootFullPath $notRun -RunId $runW -RecordBytes (Get-TestUtf8 "x`n") } 'run id'
Assert-Refused 'the pair writer refuses a malformed run id' { Write-Phase2RecordAndSidecar -RootFullPath $rootW -RunId 'nope' -RecordBytes (Get-TestUtf8 "x`n") } 'run id'
$wr = Write-Phase2RecordAndSidecar -RootFullPath $rootW -RunId $runW -RecordBytes (Get-TestUtf8 "{}`n")
Assert-Equal 'pair writer: not partial' $false $wr.Partial
Assert-Equal 'pair writer: record name from the run id' ('phase2-record-' + $runW + '.json') $wr.RecordName
Assert-Equal 'pair writer: the sidecar content' ($wr.RecordSha + '  ' + $wr.RecordName + "`n") ([System.IO.File]::ReadAllText((Join-Path $rootW $wr.SidecarName)))
Assert-Equal 'pair writer: exactly two files' 2 @(Get-ChildItem -LiteralPath $rootW -Force).Count
# partial write (review 1 m7, review 2 m5): the sidecar write fails after the record was written
$runP = 'HGP-H1-20261001T150000Z-06'
$rootP = Join-Path $wb $runP; [void][System.IO.Directory]::CreateDirectory($rootP)
Write-TestFile -Path (Join-Path $rootP ('phase2-record-' + $runP + '.json.sha256')) -Bytes (Get-TestUtf8 'taken')
$wp = Write-Phase2RecordAndSidecar -RootFullPath $rootP -RunId $runP -RecordBytes (Get-TestUtf8 "{}`n")
Assert-Equal 'partial write: reported as Partial' $true $wp.Partial
Assert-True 'partial write: the error is kept' ($wp.Error -match 'overwrite') $wp.Error
Assert-True 'partial write: the record stays on disk (nothing is deleted)' ([System.IO.File]::Exists((Join-Path $rootP ('phase2-record-' + $runP + '.json'))))
Assert-Equal 'partial write: the pre-existing sidecar is untouched' 'taken' ([System.IO.File]::ReadAllText((Join-Path $rootP ('phase2-record-' + $runP + '.json.sha256'))))

# ======================================================================================================================
Start-TestSection 'round 2: the evidence root must be empty (review 2 m1)'
$eb = New-CaseDir
$runE = 'HGP-H1-20261001T150000Z-07'
$rootE = Join-Path $eb $runE; [void][System.IO.Directory]::CreateDirectory($rootE)
$threw = $false; try { Assert-Phase2EvidenceRootEmpty -FullPath $rootE } catch { $threw = $true }
Assert-True 'an empty root is accepted' (-not $threw)
Write-TestFile -Path (Join-Path $rootE 'stray.txt') -Bytes (Get-TestUtf8 'x')
Assert-Refused 'a root with a stray file is refused' { Assert-Phase2EvidenceRootEmpty -FullPath $rootE } 'not empty'
Remove-Item -LiteralPath (Join-Path $rootE 'stray.txt') -Force
$hid = Join-Path $rootE 'hidden.txt'; Write-TestFile -Path $hid -Bytes (Get-TestUtf8 'x'); [System.IO.File]::SetAttributes($hid, [System.IO.FileAttributes]::Hidden)
Assert-Refused 'a hidden file counts' { Assert-Phase2EvidenceRootEmpty -FullPath $rootE } 'not empty'
Remove-Item -LiteralPath $hid -Force
[void][System.IO.Directory]::CreateDirectory((Join-Path $rootE 'sub'))
Assert-Refused 'a subfolder counts' { Assert-Phase2EvidenceRootEmpty -FullPath $rootE } 'not empty'

# ======================================================================================================================
Start-TestSection 'round 2: declared-set paths get the local-drive and reparse guards (review 1 m5)'
# a drive letter that is not a local fixed drive (an unused letter): nothing is read, the entries are NOT_LOCAL
$used = @([System.IO.DriveInfo]::GetDrives() | ForEach-Object { $_.Name.Substring(0, 1).ToUpperInvariant() })
$free = $null; foreach ($c in 'Z', 'Y', 'X', 'W', 'V', 'U', 'T') { if ($used -notcontains $c) { $free = $c; break } }
if ($null -eq $free) { Write-Host 'NOTE no unused drive letter on this machine: the NOT_LOCAL case is skipped' } else {
    $rNL = Invoke-FakeCapture {
        param($st, $p, $ds, $dir)
        $e = @()
        foreach ($n in 'Fake.Core.dll', 'Fake.R0.dll') { $e += , ([ordered]@{ path = ($free + ':\declared\' + $n); sha256 = ('a' * 64) }) }
        Write-TestFile -Path $ds.Json -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value $e)); $p.ExpectedDeclaredSetSha256 = (Get-Phase2FileSha256 -Path $ds.Json)
    }
    Assert-Equal 'entries on a non-local drive letter: NOT_LOCAL' 'NOT_LOCAL,NOT_LOCAL' (($rNL['pinCheck']['entries'] | ForEach-Object { $_['fileStatus'] }) -join ',')
    Assert-Equal '... the folder is NOT_LOCAL_OR_REPARSE and was not listed' 'NOT_LOCAL_OR_REPARSE,0' ($rNL['pinCheck']['folder']['status'] + ',' + @($rNL['pinCheck']['folder']['listing']).Count)
    Assert-Equal '... DECLARED_DLL_HASHES_EQUAL_ENTRY fails' 'FAIL' (Get-CheckResult -Record $rNL -Name 'DECLARED_DLL_HASHES_EQUAL_ENTRY')
    Assert-Equal '... DECLARED_FOLDER_CONTENT_EXACT fails' 'FAIL' (Get-CheckResult -Record $rNL -Name 'DECLARED_FOLDER_CONTENT_EXACT')
    Assert-Equal '... verdict INVALID' 'PHASE2_INVALID' $rNL['validation']['verdict']
    $vNL = Test-RecordOffline -Record $rNL
    Assert-True '... and the record still satisfies its schema (the new status values are in it)' (($vNL.Lines -join "`n") -match 'CHECK SCHEMA PASS')
}
# the declared-set folder is reached through a junction (a cloud placeholder or redirected folder behaves the same)
$rJ = Invoke-FakeCapture {
    param($st, $p, $ds, $dir)
    $jn = Join-Path $dir 'declared-junction'
    [void](New-Item -ItemType Junction -Path $jn -Target $ds.Folder)
    $e = @(); foreach ($x in $ds.Entries) { $e += , ([ordered]@{ path = (Join-Path $jn ([System.IO.Path]::GetFileName($x.path))); sha256 = $x.sha256 }) }
    Write-TestFile -Path $ds.Json -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value $e)); $p.ExpectedDeclaredSetSha256 = (Get-Phase2FileSha256 -Path $ds.Json)
}
Assert-Equal 'entries reached through a junction: REPARSE (not hashed)' 'REPARSE,REPARSE,REPARSE,REPARSE' (($rJ['pinCheck']['entries'] | ForEach-Object { $_['fileStatus'] }) -join ',')
Assert-Equal '... no hash was taken' 0 @($rJ['pinCheck']['entries'] | Where-Object { $null -ne $_['actualSha256'] }).Count
Assert-Equal '... the folder is NOT_LOCAL_OR_REPARSE' 'NOT_LOCAL_OR_REPARSE' $rJ['pinCheck']['folder']['status']
Assert-Equal '... verdict INVALID' 'PHASE2_INVALID' $rJ['validation']['verdict']

# ======================================================================================================================
Start-TestSection 'round 2: the schema the validator ran against is pinned (review 2 M1)'
$rS = Invoke-FakeCapture
$forgedS = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $rS); $forgedS['approvedByCadManager'] = $true
$forgedBytes = ConvertTo-RecordBytes -Record $forgedS
$vReal = Test-Phase2RecordBytes -RecordBytes $forgedBytes -Schema $schema -Expected (New-ExpectedFor -Record $rS)
Assert-True 'control: with the real schema an extra top-level property fails SCHEMA' (-not $vReal.Valid -and ($vReal.Lines -join "`n") -match 'CHECK SCHEMA FAIL')
$emptySchema = ConvertFrom-Phase2Json -Text '{}'
$emptySchemaSha = Get-Phase2BytesSha256 -Bytes (Get-TestUtf8 '{}')
$dirSP = New-CaseDir
$ledgerSP = Join-Path $dirSP 'HASHES.txt'
Write-TestFile -Path $ledgerSP -Bytes (Get-TestUtf8 (New-TestLedgerText))
$vEmpty = Test-Phase2RecordBytes -RecordBytes $forgedBytes -Schema $emptySchema -Expected (New-ExpectedFor -Record $rS) -HashesLedgerPath $ledgerSP -SchemaSha256 $emptySchemaSha -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'reviewer reproduction: an empty {} schema now FAILS the schema pin, so the forged record is invalid' (-not $vEmpty.Valid -and ($vEmpty.Lines -join "`n") -match 'CHECK SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD FAIL') ($vEmpty.Lines -join ' | ')
# the ledger lists the {} hash too (a tampered ledger) but the capture recorded another schema hash: still FAIL
Write-TestFile -Path $ledgerSP -Bytes (Get-TestUtf8 (New-TestLedgerText -Schema $emptySchemaSha))
$vEmpty2 = Test-Phase2RecordBytes -RecordBytes $forgedBytes -Schema $emptySchema -Expected (New-ExpectedFor -Record $rS) -HashesLedgerPath $ledgerSP -SchemaSha256 $emptySchemaSha -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True '... also when a tampered ledger lists the {} hash (the capture-recorded schema hash must agree)' (-not $vEmpty2.Valid -and ($vEmpty2.Lines -join "`n") -match 'CHECK SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD FAIL')
# positive control
Write-TestFile -Path $ledgerSP -Bytes (Get-TestUtf8 (New-TestLedgerText))
$vOk = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rS) -Schema $schema -Expected (New-ExpectedFor -Record $rS) -HashesLedgerPath $ledgerSP -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'positive control: matching schema hashes are accepted' ($vOk.Valid -and ($vOk.Lines -join "`n") -match 'CHECK SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD PASS' -and ($vOk.Lines -join "`n") -match 'CHECK EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER PASS') ($vOk.Lines -join ' | ')
$vNoHash = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rS) -Schema $schema -Expected (New-ExpectedFor -Record $rS) -HashesLedgerPath $ledgerSP
Assert-True 'a ledger without the schema hashes supplied fails closed' (-not $vNoHash.Valid -and ($vNoHash.Lines -join "`n") -match 'CHECK SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD FAIL' -and ($vNoHash.Lines -join "`n") -match 'CHECK EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER FAIL')
$vOtherExp = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rS) -Schema $schema -Expected (New-ExpectedFor -Record $rS) -HashesLedgerPath $ledgerSP -SchemaSha256 ('3' * 64) -ExpectedInputsSchemaSha256 ('6' * 64)
Assert-True 'an expected-inputs schema whose hash is not the ledger one fails' (-not $vOtherExp.Valid -and ($vOtherExp.Lines -join "`n") -match 'CHECK EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER FAIL')
$vOtherSch = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $rS) -Schema $schema -Expected (New-ExpectedFor -Record $rS) -HashesLedgerPath $ledgerSP -SchemaSha256 ('7' * 64) -ExpectedInputsSchemaSha256 ('5' * 64)
Assert-True 'a schema whose hash differs from the ledger and from the capture record fails' (-not $vOtherSch.Valid -and ($vOtherSch.Lines -join "`n") -match 'CHECK SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD FAIL')

# ======================================================================================================================
Start-TestSection 'round 2: the designation placed between the capture and the validation is caught (review 2 M2)'
$dirD = New-CaseDir; $dsD = New-TestDeclaredSet -Dir $dirD; $stD = New-FakeState -Dir $dirD; $pD = New-TestParams -Dir $dirD -Ds $dsD -St $stD
$recD = Invoke-Phase2Capture -Params $pD -P (New-FakeProviders -St $stD)
$vD0 = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $recD) -Schema $schema -Expected (New-ExpectedFor -Record $recD) -DeclaredSetJson $dsD.Json
Assert-True 'before the designation: the folder is exact now and unchanged' ($vD0.Valid -and ($vD0.Lines -join "`n") -match 'CHECK OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW PASS') ($vD0.Lines -join ' | ')
$desFile = Join-Path $dsD.Folder 'run-designation.json'
Write-TestFile -Path $desFile -Bytes (Get-TestUtf8 ('{"schema":"ct21d.designation.v1","runId":"' + $recD['runId'] + '","sessionId":"SES-S1-20261001-0001"}' + "`n"))
$vD1 = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $recD) -Schema $schema -Expected (New-ExpectedFor -Record $recD) -DeclaredSetJson $dsD.Json
Assert-True 'reviewer reproduction: a run-designation.json written into the folder after the capture makes the validation INVALID' (-not $vD1.Valid -and ($vD1.Lines -join "`n") -match 'CHECK OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW FAIL') ($vD1.Lines -join ' | ')
Assert-True '... while the DLL drift check alone does not see it (the gap the new check closes)' (($vD1.Lines -join "`n") -match 'CHECK OFFLINE_DECLARED_SET_UNCHANGED_SINCE_CAPTURE PASS')
Remove-Item -LiteralPath $desFile -Force
$pinFile = $dsD.Entries[0].path + '.pin'
$pinBytes = [System.IO.File]::ReadAllBytes($pinFile); Remove-Item -LiteralPath $pinFile -Force
$vD2 = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $recD) -Schema $schema -Expected (New-ExpectedFor -Record $recD) -DeclaredSetJson $dsD.Json
Assert-True 'a .pin removed after the capture (missing now) is also caught' (-not $vD2.Valid -and ($vD2.Lines -join "`n") -match 'CHECK OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW FAIL')
Write-TestFile -Path $pinFile -Bytes $pinBytes
$vD3 = Test-Phase2RecordBytes -RecordBytes (ConvertTo-RecordBytes -Record $recD) -Schema $schema -Expected (New-ExpectedFor -Record $recD) -DeclaredSetJson $dsD.Json
Assert-True 'restored to the hashed state: accepted again' ($vD3.Valid) ($vD3.Lines -join ' | ')

# ======================================================================================================================
Start-TestSection 'round 2: the module baseline is mandatory for every session except S1-A (review 2 M3)'
$rA = Invoke-FakeCapture
Assert-Equal 'S1-A without a baseline: still valid (the S1-A exception is encoded, it produces the baseline)' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $rA['validation']['verdict']
Assert-Equal '... check passes' 'PASS' (Get-CheckResult -Record $rA -Name 'MODULE_BASELINE_COMPARISON')
foreach ($sess in 'S1-B', 'S1-C', 'S2', 'S3', 'S4') {
    $rN = Invoke-FakeCapture -Session $sess
    Assert-Equal ($sess + ' without a baseline (default flags): MODULE_BASELINE_COMPARISON FAILS') 'FAIL' (Get-CheckResult -Record $rN -Name 'MODULE_BASELINE_COMPARISON')
    Assert-Equal ('... ' + $sess + ' verdict INVALID, never the plain VALID') 'PHASE2_INVALID' $rN['validation']['verdict']
    Assert-Equal ('... ' + $sess + ' flag requireModuleBaseline stays false: the switch is not what decides') $false $rN['tupleInputs']['requireModuleBaseline']
    $vN = Test-RecordOffline -Record $rN
    Assert-True ('... ' + $sess + ' the offline validator re-derives the failure') (-not $vN.Valid -and ($vN.Lines -join "`n") -match 'CHECK RECORD_MODULE_BASELINE_COMPARISON FAIL')
    $rB = Invoke-FakeCapture -Session $sess $script:WithBaseline
    Assert-Equal ($sess + ' with a provided and compared baseline: valid') 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $rB['validation']['verdict']
    Assert-Equal ('... ' + $sess + ' coverage says COMPARED') 'COMPARED' $rB['coverage']['moduleBaselineComparison']
}

# ======================================================================================================================
Start-TestSection 'round 2: seconds since the process start (review 2 m6) and the information-only fields'
$rE = Invoke-FakeCapture { param($st) $st.Now = '2026-10-01T15:00:00.123Z' }
Assert-Equal 'capture 60.000 s after the process start: 60' 60 $rE['volatile']['secondsSinceProcessStart']
$rE2 = Invoke-FakeCapture { param($st) $st.Now = '2026-10-01T16:30:00.123Z' }
Assert-Equal 'a capture 91 minutes after the start records 5460 (visible to the CAD manager, judged by no check)' 5460 $rE2['volatile']['secondsSinceProcessStart']
Assert-Equal '... the verdict does not depend on it' 'PHASE2_EXTERNAL_OBSERVATIONS_VALID' $rE2['validation']['verdict']
$rE3 = Invoke-FakeCapture { param($st) $st.Now = 'not a time' }
Assert-True 'an unparsable capture time records null' ($null -eq $rE3['volatile']['secondsSinceProcessStart'])
$rd = ConvertFrom-Phase2Json -Text (ConvertTo-Phase2CanonicalJson -Value $rE); $rd['volatile'].Remove('secondsSinceProcessStart')
Assert-True 'the field is required by the schema' ((Test-Phase2AgainstSchema -Value $rd -Schema $schema).Count -gt 0)
Assert-True 'the field is in volatile and in no check name' (@($rE['validation']['checks'] | Where-Object { $_['name'] -match 'SECONDS|ELAPSED' }).Count -eq 0)

# ======================================================================================================================
Start-TestSection 'round 4: -AllowedEvidenceParent (Capture) and the input-file guard of -Schema / -ExpectedBuildTupleInputs / -HashesLedger (Validate)'
$pb = New-CaseDir
$runP4 = 'HGP-H1-20261001T150000Z-09'
$rootP4 = Join-Path $pb $runP4; [void][System.IO.Directory]::CreateDirectory($rootP4)
Assert-Equal 'the root is a direct child of the allowed parent: accepted' $pb (Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath $pb)
Assert-Equal '... a trailing backslash on the parent is accepted' $pb (Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath ($pb + '\'))
Assert-True '... the comparison ignores case' ((Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath ($pb.ToUpperInvariant())) -ieq $pb)
$other4 = New-CaseDir
Assert-Refused 'another parent folder is refused' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath $other4 } 'direct child'
Assert-Refused 'a grandparent is refused (the root must be a DIRECT child)' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath (Split-Path -Parent $pb) } 'direct child'
Assert-Refused 'a parent that does not exist is refused' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath (Join-Path $pb 'nope') } 'does not exist'
Assert-Refused 'a relative parent is refused' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath '.\evidence' } 'drive-letter'
Assert-Refused 'a UNC parent is refused' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath '\\localhost\c$\x' } 'UNC'
Assert-Refused 'a parent with a dot segment is refused' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath ($pb + '\..\' + (Split-Path -Leaf $pb)) } 'dot segment'
Assert-Refused 'an empty parent is refused' { Assert-Phase2AllowedEvidenceParent -RootFullPath $rootP4 -ParentPath '' } 'empty'
$pwshP4 = (Get-Process -Id $PID).Path
$u4 = & $pwshP4 -NoProfile -NonInteractive -File (Join-Path $script:ToolRoot 'Capture-Phase2.ps1') -ProcessId 123456 -Session S1-A -SessionId SES-1 -RunId $runP4 -EvidenceRoot $rootP4 -DeclaredSetJson 'C:\nope\d.json' -ExpectedDeclaredSetSha256 ('a' * 64) -ExpectedAcadSha256 ('b' * 64) -ExpectedProfile p 2>&1
Assert-True 'Capture without -AllowedEvidenceParent: usage error naming the parameter' ($LASTEXITCODE -eq 1 -and (($u4 | ForEach-Object { [string]$_ }) -join "`n") -match 'AllowedEvidenceParent') (($u4 | ForEach-Object { [string]$_ }) -join "`n")
Assert-Equal '... and nothing was written' 0 @(Get-ChildItem -LiteralPath $rootP4 -Force).Count

# Validate: the three file parameters go through the same local / non-UNC / non-device / reparse guard as the record
Push-Location -LiteralPath $script:ToolRoot
try {
    $vs = Join-Path $script:ToolRoot 'schemas/ct21d.phase2.v1.json'
    $ve = Join-Path $script:ToolRoot 'schemas/ct21d.phase2.expected-inputs.v1.json'
    $vl = Join-Path $script:ToolRoot 'HASHES-PHASE2.txt'
    $vrec = Join-Path $pb 'rec.json'; Write-TestFile -Path $vrec -Bytes (Get-TestUtf8 "{}`n")
    $vds = Join-Path $pb 'ds.json'; Write-TestFile -Path $vds -Bytes (Get-TestUtf8 "[]`n")
    $cases4 = @(
        @('relative -Schema', @($vrec, '-Schema', 'schemas\ct21d.phase2.v1.json', '-ExpectedBuildTupleInputs', $ve, '-HashesLedger', $vl, '-DeclaredSetJson', $vds), '-Schema'),
        @('relative -ExpectedBuildTupleInputs', @($vrec, '-Schema', $vs, '-ExpectedBuildTupleInputs', 'schemas\ct21d.phase2.expected-inputs.v1.json', '-HashesLedger', $vl, '-DeclaredSetJson', $vds), '-ExpectedBuildTupleInputs'),
        @('relative -HashesLedger', @($vrec, '-Schema', $vs, '-ExpectedBuildTupleInputs', $ve, '-HashesLedger', 'HASHES-PHASE2.txt', '-DeclaredSetJson', $vds), '-HashesLedger')
    )
    foreach ($c4 in $cases4) {
        $o4 = & $pwshP4 -NoProfile -NonInteractive -File (Join-Path $script:ToolRoot 'Validate-Phase2.ps1') @($c4[1]) 2>&1
        $t4 = ($o4 | ForEach-Object { [string]$_ }) -join "`n"
        Assert-True ('Validate refuses a ' + $c4[0] + ' (exit 1, names the parameter)') ($LASTEXITCODE -eq 1 -and $t4.Contains($c4[2]) -and $t4 -match 'drive-letter') $t4
    }
} finally { Pop-Location }
