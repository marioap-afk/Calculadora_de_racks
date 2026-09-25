# I-52 V35-A1/V35-A2 negative controls for validate-v35-catalog.ps1. Each control copies the validator inputs to a temporary
# directory, applies one deliberate corruption and runs the validator there. Every control must make the validator fail
# (exit code 1) without a script error. Static only: no AutoCAD, no build, no runtime semantics.
param(
    [string]$Repository = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path,
    [string]$EvidencePath = ''
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$inputs = @(
    'docs/initiatives/I-52-execution-catalog-v35.json', 'docs/initiatives/I-52-native-probe-matrix-v34.md',
    'docs/initiatives/I-52-native-probe-matrix-v35.md', 'docs/initiatives/I-52-native-event-catalog-v34.md',
    'docs/initiatives/I-52-native-event-catalog-v35.md', 'eng/research/I52Ctda/fixture-v35.json',
    'eng/research/I52Ctda/traceability-v35.json', 'eng/research/I52Ctda/traceability-v34.json',
    'eng/research/I52Ctda/v35-oracle.json', 'docs/automation/evidence/I-52-v35-clause-lineage.json',
    'docs/automation/evidence/I-52-r3-governed-executor-discovery.json', 'eng/research/I52Ctda/validate-v35-catalog.ps1',
    'eng/research/I52Ctda/oracle/make_oracle.py', 'eng/research/I52Ctda/oracle/oracle_predicates.py')
$catalogPath = 'docs/initiatives/I-52-execution-catalog-v35.json'
$npmPath = 'docs/initiatives/I-52-native-probe-matrix-v35.md'
$tracePath = 'eng/research/I52Ctda/traceability-v35.json'
$linPath = 'docs/automation/evidence/I-52-v35-clause-lineage.json'
$necPath = 'docs/initiatives/I-52-native-event-catalog-v35.md'
$script:tmp = $null

function T([string]$rel) { Join-Path $script:tmp $rel }
function ReadJ([string]$rel) { [IO.File]::ReadAllText((T $rel)) | ConvertFrom-Json -AsHashtable }
function WriteJ([string]$rel, $obj) { [IO.File]::WriteAllText((T $rel), ($obj | ConvertTo-Json -Depth 64)) }
function EditCatalog([scriptblock]$fn) { $c = ReadJ $catalogPath; & $fn $c; WriteJ $catalogPath $c }
function EditRow([string]$probe, [string]$col, [scriptblock]$fn) {
    $cols = [string[]](ReadJ $catalogPath)['rowSchema']
    $lines = [IO.File]::ReadAllText((T $npmPath)).Split("`n")
    $hit = $false
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $f = $lines[$i].TrimEnd("`r").Split(';')
        if ($f.Count -eq $cols.Count -and $f[0] -eq $probe) { $cr = if ($lines[$i].EndsWith("`r")) { "`r" } else { '' }; $j = [Array]::IndexOf($cols, $col); $f[$j] = & $fn $f[$j]; $lines[$i] = ($f -join ';') + $cr; $hit = $true }
    }
    if (-not $hit) { throw "row $probe not found" }
    [IO.File]::WriteAllText((T $npmPath), ($lines -join "`n"))
}
function EditLineage([string]$probe, [string]$column, [scriptblock]$fn) {
    $d = ReadJ $linPath
    foreach ($r in $d['rows']) { if ($r['ProbeId'] -eq $probe) { foreach ($rec in $r['lineage']) { if ($rec['column'] -eq $column) { $rec['v35'] = @(& $fn @($rec['v35'])) } } } }
    WriteJ $linPath $d
}
function EditTrace([string]$probe, [scriptblock]$fn) {
    $d = @(ReadJ $tracePath); foreach ($r in $d) { if ($r['ProbeId'] -eq $probe) { & $fn $r } }; WriteJ $tracePath $d
}
# V35-A2 (Architect MINOR M2): exact text edits work on LF-normalized text and write back with the input's own line ending,
# so the same controls run on an LF or a CRLF (core.autocrlf=true) checkout.
function EditText([string]$rel, [string]$find, [string]$replace) {
    $raw = [IO.File]::ReadAllText((T $rel)); $crlf = $raw.Contains("`r`n")
    $t = $raw.Replace("`r`n", "`n"); $find = $find.Replace("`r`n", "`n"); $replace = $replace.Replace("`r`n", "`n")
    if (-not $t.Contains($find)) { throw "text not found in $rel" }
    $i = $t.IndexOf($find); $t = $t.Substring(0, $i) + $replace + $t.Substring($i + $find.Length)
    if ($crlf) { $t = $t.Replace("`n", "`r`n") }
    [IO.File]::WriteAllText((T $rel), $t)
}
function Swap([string]$list, [string]$old, [string]$new) { (@($list.Split(',') | ForEach-Object { if ($_ -eq $old) { $new } else { $_ } })) -join ',' }
function Drop([string]$list, [string]$old) { $v = @($list.Split(',') | Where-Object { $_ -ne $old }); if ($v.Count) { $v -join ',' } else { 'NONE' } }
function MapList($v, [string]$old, [string]$new) { @($v | ForEach-Object { if ($_ -eq $old) { $new } else { $_ } }) }

$controls = [ordered]@{
    # ---------------- V35 controls (24)
    'V35-01 free-text trigger'               = { EditRow '02N' 'TriggerActionId' { 'modify DB notifier A' } }
    'V35-02 or-NONE alternative in T'        = { EditRow 'CAPP16SND-ALL' 'PrimaryTransactionId' { 'origin T or NONE' } }
    'V35-03 drop body observer'              = { EditRow '02NO-M' 'ObserverRegistrationIds' { 'ER-REF-A' } }
    'V35-04 wrong guard family'              = { EditRow '02NDBMOD-S' 'GuardIds' { 'RG-DB-ERASE' } }
    'V35-05 missing marker observer'         = { EditRow '16N-S' 'ObserverRegistrationIds' { 'RR-DB,RR-ED' } }
    'V35-06 SA-LOCK in callback (B08)'       = { EditRow '02NTAS-ALL' 'ExecutionContextId' { 'APPCTX-EXEC-01' } }
    'V35-07 veto polarity reverted (B10)'    = { EditRow '10NDOC-WILL-SM' 'Markers' { param($v) $v.Replace('-N-DOC-LOCK-VETO', '+N-DOC-LOCK-VETO') } }
    'V35-08 retained column drift'           = { EditRow '09N-O' 'ExpectedAfter' { 'STATE-S-1' } }
    'V35-09 PASS obligation removed'         = { EditRow 'CDBOPEN16SND-ALL' 'ObservationPredicateIds' { param($v) Drop $v 'OBS-DELIVERY' } }
    'V35-10 candidate boundary removed'      = { EditRow '10N-M' 'ObservationPredicateIds' { param($v) Drop $v 'OBS-CANDIDATE-BOUNDARY' } }
    'V35-11 fence mismatch'                  = { EditRow '02NO-SM' 'CompletionFence' { 'FENCE-CLEANUP-TOKEN' } }
    'V35-12 double CLN-BASE (C02)'           = { EditCatalog { param($c) $c['entries']['CLEAN-OBJ-DEFER']['steps'] = @('CLN-OBJ-REMOVE', 'CLN-BASE', 'CLN-DEFER-DRAIN', 'CLN-BASE') } }
    'V35-13 ninth identity'                  = { EditCatalog { param($c) $c['entries']['F-EXTRA'] = [ordered]@{ kind = 'PersistentFixtureIdentity'; definition = 'x' } } }
    'V35-14 trigger does not produce body'   = { EditCatalog { param($c) $c['entries']['TRG-APPEND-TRIGGER']['produces'] = @('N-DB-MOD') } }
    'V35-15 dangling catalog reference'      = { EditCatalog { param($c) $c['entries']['CB-EXEC-01']['locks'] = @('CB-LOCK-99') } }
    'V35-16 class D lineage'                 = { EditText $linPath '"class": "B"' '"class": "D"' }
    'V35-17 NEC relation dropped'            = { EditText $necPath "| ``N-DB-OPEN`` | ``PRIMARY_FOR`` | ``02N`` |`n" '' }
    'V35-18 trigger disjointness'            = { EditCatalog { param($c) $c['entries']['MUT-M']['writeSet'] += 'F-REF-A' } }
    'V35-19 marker without producer'         = { EditCatalog { param($c) $c['entries']['TRG-MODIFY-CLOSE-REF-A']['produces'] = @('N-OBJ-MOD', 'N-OBJ-CLOSED', 'N-DB-MOD') } }
    'V35-20 second body observer'            = { EditRow 'C15N16N-SM' 'BodyObserverId' { 'RR-DB' } }
    'V35-21 staging UNKNOWN dropped'         = { EditRow '04N' 'UnknownPredicateIds' { param($v) Drop $v 'UNK-STAGING' } }
    'V35-22 nested-in-abort UNKNOWN dropped' = { EditRow '02NTA-ALL' 'UnknownPredicateIds' { param($v) Drop $v 'UNK-NESTED-IN-ABORT' } }
    'V35-23 guard arm target mismatch'       = { EditRow 'CDBERASE16SND-ALL' 'TriggerActionId' { 'TRG-ERASE-TRIGGER-OBJ' } }
    'V35-24 body observer on observation row' = { EditRow '09N-B' 'BodyObserverId' { 'RR-TX' } }
    # ---------------- Architect C1..C7 (coherent corruption across files)
    'C1 semantic wrong target, coherent'     = {
        EditRow 'COBJMOD16APP-ALL' 'TriggerActionId' { 'TRG-ASSERT-WRITE-TRIGGER-XR' }
        EditLineage 'COBJMOD16APP-ALL' 'Trigger' { param($v) MapList $v 'TRG-MODIFY-CLOSE-REF-A' 'TRG-ASSERT-WRITE-TRIGGER-XR' }
        EditTrace 'COBJMOD16APP-ALL' { param($r) $r['TriggerActionId'] = 'TRG-ASSERT-WRITE-TRIGGER-XR' } }
    'C2 coherent wrong resource category'    = {
        EditCatalog { param($c) $c['entries']['R-SM-LINK']['kind'] = 'DisposableTriggerResource' }
        $d = @(ReadJ $tracePath); foreach ($r in $d) { foreach ($x in $r['Resources']) { if ($x['id'] -eq 'R-SM-LINK') { $x['kind'] = 'DisposableTriggerResource' } } }; WriteJ $tracePath $d }
    'C3 semantically invalid FailPredicate'  = {
        EditRow '02NDBMOD-S' 'FailPredicateIds' { 'FP-VETO-BYPASS,FP-CLEANUP-SAFETY' }
        EditLineage '02NDBMOD-S' 'FAIL' { param($v) if (@($v) -contains 'FP-CLEANUP-SAFETY') { @('FP-CLEANUP-SAFETY') } else { @('FAIL-S', 'FP-VETO-BYPASS') } } }
    'C4 degraded cleanup, coherent'          = {
        EditRow '02NO-M' 'CleanupActionId' { 'CLEAN-BASE' }
        EditLineage '02NO-M' 'Cleanup' { param($v) @('CLEAN-BASE') }
        EditTrace '02NO-M' { param($r) $r['Cleanup'] = 'CLEAN-BASE' } }
    'C5 retained authority rewritten'        = { EditCatalog { param($c) $c['entries']['APPCTX-TX-01']['definition'] = 'Reuse the origin transaction; never start T_APP.' } }
    'C6 changed ObservationPredicate'        = {
        EditRow '16A-S' 'ObservationPredicateIds' { param($v) Swap $v 'OBS-DELIVERY' 'OBS-REREAD' }
        EditLineage '16A-S' 'PASS' { param($v) MapList $v 'OBS-DELIVERY' 'OBS-REREAD' } }
    'C7 lock trigger removed from APPCTX driver' = {
        EditCatalog { param($c) $c['entries']['TRG-LOCK-CYCLE']['driver'] = 'DRIVER-CMD-01' }
        foreach ($p in '10NDOC-WILL-SM', '10NDOC-CHANGED-SM', 'C2DOCW16N-SM', 'C2DOCC16N-SM', 'CDOCLOCKCHANGED16APP-ALL', 'CDOCLOCKWILL16APP-ALL') {
            EditRow $p 'DriverId' { 'DRIVER-CMD-01' }; EditTrace $p { param($r) $r['DriverId'] = 'DRIVER-CMD-01' } } }
    # ---------------- RC-01..RC-13 controls
    'RC-01a FINISH before final delivery'    = {
        EditRow '16N-S' 'CompletionTokenIds' { param($v) Drop $v 'TOK-SEND-CMD-END' }
        EditTrace '16N-S' { param($r) $r['CompletionTokenIds'] = @($r['CompletionTokenIds'] | Where-Object { $_ -ne 'TOK-SEND-CMD-END' }) } }
    'RC-01b FINISH issued by the script'     = { EditCatalog { param($c) $c['entries']['DRIVER-SCRIPT-01']['containsFinish'] = $true; $c['entries']['DRIVER-SCRIPT-01']['scriptSteps'] += 'I52CTDA_FINISH <ProbeId>' } }
    'RC-02a wrong unlock satisfies MARK-LOCK-RELEASE' = {
        EditRow '16N-S' 'MarkerStageBindings' { param($v) Swap $v 'MARK-LOCK-RELEASE@STG-SEND-DELIVERY' 'MARK-LOCK-RELEASE@STG-PROBE-CMD' }
        EditLineage '16N-S' 'LifecycleMarkers' { param($v) MapList $v 'MarkerStageBindings=MARK-LOCK-RELEASE@STG-SEND-DELIVERY' 'MarkerStageBindings=MARK-LOCK-RELEASE@STG-PROBE-CMD' } }
    'RC-02b FINISH unlock satisfies MARK-LOCK-RELEASE' = {
        EditCatalog { param($c) $c['entries']['MARKER-STAGE-BIND-01']['markerStages']['MARK-LOCK-RELEASE'] += 'STG-FINISH' }
        EditRow '16N-S' 'MarkerStageBindings' { param($v) Swap $v 'MARK-LOCK-RELEASE@STG-SEND-DELIVERY' 'MARK-LOCK-RELEASE@STG-FINISH' }
        EditLineage '16N-S' 'LifecycleMarkers' { param($v) MapList $v 'MarkerStageBindings=MARK-LOCK-RELEASE@STG-SEND-DELIVERY' 'MarkerStageBindings=MARK-LOCK-RELEASE@STG-FINISH' } }
    'RC-03 driver APPCTX marker satisfies probe delivery' = {
        EditCatalog { param($c) $c['entries']['MARKER-STAGE-BIND-01']['markerStages']['MARK-APPCTX-DELIVERY'] += 'STG-DRIVER-APP' }
        EditRow 'CDOCLOCKCHANGED16APP-ALL' 'MarkerStageBindings' { param($v) Swap $v 'MARK-APPCTX-DELIVERY@STG-APPCTX-DELIVERY' 'MARK-APPCTX-DELIVERY@STG-DRIVER-APP' }
        EditLineage 'CDOCLOCKCHANGED16APP-ALL' 'LifecycleMarkers' { param($v) MapList $v 'MarkerStageBindings=MARK-APPCTX-DELIVERY@STG-APPCTX-DELIVERY' 'MarkerStageBindings=MARK-APPCTX-DELIVERY@STG-DRIVER-APP' } }
    'RC-04a cleanup safety hidden by UNKNOWN' = { EditCatalog { param($c) $c['entries']['RESULT-RULE-V35']['evaluationOrder'] = @('UNKNOWN', 'SAFETY-FAIL', 'FAIL', 'PASS', 'UNKNOWN-RESIDUAL') } }
    'RC-04b safety exception removed from a row' = {
        EditRow '02N' 'FailPredicateIds' { param($v) Drop $v 'FP-CLEANUP-SAFETY' }
        EditLineage '02N' 'FAIL' { param($v) @($v | Where-Object { $_ -ne 'FP-CLEANUP-SAFETY' }) } }
    'RC-05a incomplete evidence reaches FAIL/PASS' = { EditCatalog { param($c) $c['entries'].Remove('EVIDENCE-COMPLETE') } }
    'RC-05b in-process FINISH classifies'    = { EditCatalog { param($c) $c['entries']['CMD-FINISH']['neverClassifies'] = $false } }
    'RC-06 unauthorized retained extension'  = { EditCatalog { param($c) $c['entries']['APPCTX-UNLOCK-01']['retainedStatus'] = 'RETAINED-EXTENDED'; $c['entries']['APPCTX-UNLOCK-01']['definition'] = 'Unlock only if convenient.' } }
    'RC-07a managed host unclassified'       = {
        EditCatalog { param($c) $c['entries']['R-MANAGED-OBSERVER']['kind'] = 'ObserverRegistration' }
        $d = @(ReadJ $tracePath); foreach ($r in $d) { foreach ($x in $r['Resources']) { if ($x['id'] -eq 'R-MANAGED-OBSERVER') { $x['kind'] = 'ObserverRegistration' } } }; WriteJ $tracePath $d }
    'RC-07b managed host never loaded'       = { EditCatalog { param($c) $c['entries']['DRIVER-SCRIPT-01']['scriptSteps'] = @($c['entries']['DRIVER-SCRIPT-01']['scriptSteps'] | Where-Object { $_ -notmatch 'NETLOAD' }) } }
    'RC-08a payload bound to any database'   = { EditCatalog { param($c) $c['entries']['R-PAYLOAD-ARX']['dbBinding'] = 'ANY-DATABASE' } }
    'RC-08b payload DB observation dropped'  = {
        EditRow 'C15N16N-SM' 'ObservationPredicateIds' { param($v) Drop $v 'OBS-PAYLOAD-DB-BINDING' }
        EditLineage 'C15N16N-SM' 'PASS' { param($v) @($v | Where-Object { $_ -ne 'OBS-PAYLOAD-DB-BINDING' }) } }
    'RC-09 cancel staging equals ExpectedAfter bytes' = { EditCatalog { param($c) $c['entries']['TRG-CANCEL-OPEN-XR']['stagedBytes'] = 'HFV30:S:1' } }
    'RC-10 02NAPP-SM competing Model Space transaction' = {
        EditRow '02NAPP-SM' 'TriggerActionId' { 'TRG-APPEND-TRIGGER' }
        EditLineage '02NAPP-SM' 'Trigger' { param($v) MapList $v 'TRG-APPEND-TRIGGER-IN-PRIMARY' 'TRG-APPEND-TRIGGER' }
        EditLineage '02NAPP-SM' 'Setup' { param($v) MapList $v 'TRG-APPEND-TRIGGER-IN-PRIMARY' 'TRG-APPEND-TRIGGER' }
        EditTrace '02NAPP-SM' { param($r) $r['TriggerActionId'] = 'TRG-APPEND-TRIGGER' } }
    'RC-11a dynamic-linker guard key without module path' = { EditCatalog { param($c) $c['entries']['RG-ONESHOT']['keySchema']['N-RX-'] = @('ProbeId', 'GuardId', 'EventId', 'CallbackMember', 'StageId') } }
    'RC-11b open guard key without target'   = { EditCatalog { param($c) $c['entries']['RG-DB-OPEN']['keySchema'] = @('ProbeId', 'GuardId', 'DatabaseId', 'EventId', 'CallbackMember', 'StageId') } }
    'RC-12 guard disarmed before obligations' = { EditCatalog { param($c) $c['entries']['DRIVER-CMD-01']['phaseOrder'] = @('REGISTER', 'SETUP', 'ARM', 'TRIGGER', 'CALLBACK-WINDOW', 'DISARM', 'POST-TRIGGER-OBLIGATIONS', 'OUTCOME-RECORD', 'TOKEN') } }
    'RC-13 body notifier coherently corrupted' = { EditCatalog { param($c) $c['entries']['OR-REF-A']['notifier'] = 'F-REF-B' } }
    # ---------------- pre-publication review, lifecycle lens (L1..L6)
    'L1 APPCTX lock release anchored after the stage token' = { EditCatalog { param($c) $c['entries']['LOCK-RELEASE-BIND-01']['anchors']['STG-APPCTX-DELIVERY'] = 'COMMAND-END-WINDOW' } }
    'L2 cleanup closes the document inside FINISH' = { EditCatalog { param($c) $c['entries']['CLN-BASE']['closesDocument'] = $true } }
    'L3 late delivery not fenced'            = { EditCatalog { param($c) $c['entries']['CMD-FINISH'].Remove('firstAction') } }
    'L4 payload reactor not removable'       = { EditCatalog { param($c) $c['entries']['R-PAYLOAD-ARX']['exports'] = @() } }
    'L5 sync caller return outside its stage' = { EditCatalog { param($c) $c['entries']['STG-SYNC-APPCTX']['includesCallerReturn'] = $false } }
    'L6 cleanup records counted as safety violations' = { EditCatalog { param($c) $c['entries']['FP-CLEANUP-SAFETY']['scope'] = 'ALL-NAMESPACES' } }
}


# Additional controls from the pre-publication adversarial review (independence lens X*, never-fired checks N*).
$controls['X1 drop FP-STATE, row and lineage'] = { EditRow '02NDBMOD-S' 'FailPredicateIds' { param($v) Drop $v 'FP-STATE' }; EditLineage '02NDBMOD-S' 'FAIL' { param($v) @($v | Where-Object { $_ -ne 'FP-STATE' }) } }
$controls['X2 drop OBS-ORDER-RULES, row and lineage'] = { EditRow '02NO-M' 'ObservationPredicateIds' { param($v) Drop $v 'OBS-ORDER-RULES' }; EditLineage '02NO-M' 'PASS' { param($v) @($v | Where-Object { $_ -ne 'OBS-ORDER-RULES' }) } }
$controls['X3 drop UNKNOWN predicates, row and lineage'] = {
    EditRow '02NDBMOD-S' 'UnknownPredicateIds' { param($v) Drop (Drop $v 'UNK-ILLEGAL-MUTATION') 'UNK-RECURSION' }
    EditLineage '02NDBMOD-S' 'UNKNOWN' { param($v) @($v | Where-Object { $_ -notin 'UNK-ILLEGAL-MUTATION', 'UNK-RECURSION' }) } }
$controls['X4 same-class trigger swap, coherent'] = {
    EditRow '02NO-M' 'TriggerActionId' { 'TRG-TRANSFORM-CLOSE-REF-A' }
    EditLineage '02NO-M' 'Trigger' { param($v) MapList $v 'TRG-MODIFY-CLOSE-REF-A' 'TRG-TRANSFORM-CLOSE-REF-A' }
    EditTrace '02NO-M' { param($r) $r['TriggerActionId'] = 'TRG-TRANSFORM-CLOSE-REF-A' } }
$controls['X5 traceability resource dropped'] = { EditTrace '02NDBMOD-S' { param($r) $r['Resources'] = @($r['Resources'] | Where-Object { $_['id'] -ne 'F-TRIGGER-MOD' }) } }
$controls['X6 traceability bindings forged'] = { EditTrace '16A-S' { param($r) $r['MarkerStageBindings'] = @('MARK-ORIGIN-RETURN@STG-PROBE-CMD') } }
$controls['X7 token dropped in row, trace and oracle'] = {
    EditRow '16N-S' 'CompletionTokenIds' { param($v) Drop $v 'TOK-SEND-CMD-END' }
    EditTrace '16N-S' { param($r) $r['CompletionTokenIds'] = @($r['CompletionTokenIds'] | Where-Object { $_ -ne 'TOK-SEND-CMD-END' }) }
    $o = ReadJ 'eng/research/I52Ctda/v35-oracle.json'; $o['G_completionTokens']['16N-S'] = @($o['G_completionTokens']['16N-S'] | Where-Object { $_ -ne 'TOK-SEND-CMD-END' }); WriteJ 'eng/research/I52Ctda/v35-oracle.json' $o }
$controls['X8 V34 baseline and catalog rewritten together'] = {
    EditCatalog { param($c) $c['entries']['APPCTX-TX-01']['definition'] = 'Reuse the origin transaction.' }
    EditText 'docs/initiatives/I-52-native-probe-matrix-v34.md' '| APPCTX-TX-01 | After APPCTX-LOCK-01' '| APPCTX-TX-01 | Reuse the origin transaction. After APPCTX-LOCK-01' }
$controls['X9 APPCTX unlock support removed'] = { EditCatalog { param($c) $c['entries']['APPCTX-EXEC-01']['usesSupport'] = @($c['entries']['APPCTX-EXEC-01']['usesSupport'] | Where-Object { $_ -ne 'SA-UNLOCK' }) } }
$controls['X11 approval hash replaced in the oracle'] = { $o = ReadJ 'eng/research/I52Ctda/v35-oracle.json'; $o['D_retainedAuthority']['RG-DB-ERASE']['approvedSha256'] = ('0' * 64); WriteJ 'eng/research/I52Ctda/v35-oracle.json' $o }
$controls['X12 FP-STATE removed from every row and lineage'] = {
    $cols = [string[]](ReadJ $catalogPath)['rowSchema']; $j = [Array]::IndexOf($cols, 'FailPredicateIds')
    $lines = [IO.File]::ReadAllText((T $npmPath)).Split("`n")
    for ($i = 0; $i -lt $lines.Count; $i++) { $cr = if ($lines[$i].EndsWith("`r")) { "`r" } else { '' }; $f = $lines[$i].TrimEnd("`r").Split(';'); if ($f.Count -eq $cols.Count -and $f[0] -ne 'ProbeId') { $f[$j] = Drop $f[$j] 'FP-STATE'; $lines[$i] = ($f -join ';') + $cr } }
    [IO.File]::WriteAllText((T $npmPath), ($lines -join "`n"))
    $d = ReadJ $linPath; foreach ($r in $d['rows']) { foreach ($rec in $r['lineage']) { if ($rec['column'] -eq 'FAIL') { $rec['v35'] = @($rec['v35'] | Where-Object { $_ -ne 'FP-STATE' }) } } }; WriteJ $linPath $d }
$controls['X13 APPCTX driver disarms before obligations'] = { EditCatalog { param($c) $c['entries']['DRIVER-APP-01']['phaseOrder'] = @('ENQUEUE', 'DELIVERY', 'ARM', 'TRIGGER', 'CALLBACK-WINDOW', 'DISARM', 'POST-TRIGGER-OBLIGATIONS', 'OUTCOME-RECORD', 'TOKEN') } }
$controls['N2 erase trigger target in a write set'] = { EditCatalog { param($c) $c['entries']['MUT-SM']['writeSet'] += 'F-REF-B' } }
$controls['N3 extended entry text changed'] = { EditCatalog { param($c) $c['entries']['CLEAN-OBJ']['definition'] += ' Remove only when convenient.' } }
$controls['N4 deferred cleanup without drain'] = { EditCatalog { param($c) $c['entries']['CLEAN-DEFER']['steps'] = @('CLN-BASE') } }
$controls['N5 FINISH timeout UNKNOWN dropped'] = { EditRow '02N' 'UnknownPredicateIds' { param($v) Drop $v 'UNK-FINISH-TIMEOUT' }; EditLineage '02N' 'UNKNOWN' { param($v) @($v | Where-Object { $_ -ne 'UNK-FINISH-TIMEOUT' }) } }
$controls['N6 document guard key needs a module path'] = { EditCatalog { param($c) $c['entries']['RG-ONESHOT']['keySchema']['N-DOC-'] += 'ModulePath' } }
$controls['N7 transaction guard key without stage'] = { EditCatalog { param($c) $c['entries']['RG-TX']['keySchema'] = @($c['entries']['RG-TX']['keySchema'] | Where-Object { $_ -ne 'StageId' }) } }
$controls['N8 lock-release observation dropped'] = { EditRow '16N-S' 'ObservationPredicateIds' { param($v) Drop $v 'OBS-LOCK-RELEASE-BOUND' }; EditLineage '16N-S' 'PASS' { param($v) @($v | Where-Object { $_ -ne 'OBS-LOCK-RELEASE-BOUND' }) } }
$controls['N9 cancel restoration observation dropped'] = { EditRow 'COBJCANCEL16SND-ALL' 'ObservationPredicateIds' { param($v) Drop $v 'OBS-CANCEL-RESTORED' }; EditLineage 'COBJCANCEL16SND-ALL' 'PASS' { param($v) @($v | Where-Object { $_ -ne 'OBS-CANCEL-RESTORED' }) } }
$controls['N10 staging value becomes an expected state'] = { EditCatalog { param($c) $c['entries']['STATE-ALL-1']['definition'] += ' HFV35:XR:CANCEL-STAGED' } }

# The check each control is meant to trigger (at least one listed check must appear in the validator findings).
$expect = @{
    'V35-01 free-text trigger' = 'UNBOUND'; 'V35-02 or-NONE alternative in T' = 'UNBOUND'; 'V35-03 drop body observer' = 'BODY-OBSERVER'
    'V35-04 wrong guard family' = 'GUARD-FAMILY'; 'V35-05 missing marker observer' = 'MARKER-OBSERVER'; 'V35-06 SA-LOCK in callback (B08)' = 'EXEC-TRANSACTION'
    'V35-07 veto polarity reverted (B10)' = 'NEC-PROJECTION'; 'V35-08 retained column drift' = 'RETAINED-DRIFT'; 'V35-09 PASS obligation removed' = 'PREDICATE-SET'
    'V35-10 candidate boundary removed' = 'CANDIDATE-BOUNDARY'; 'V35-11 fence mismatch' = 'FENCE'; 'V35-12 double CLN-BASE (C02)' = 'TERMINAL-BASE'
    'V35-13 ninth identity' = 'FIXTURE'; 'V35-14 trigger does not produce body' = 'TRIGGER-PRODUCES'; 'V35-15 dangling catalog reference' = 'CATALOG-REF'
    'V35-16 class D lineage' = 'CLASS-D'; 'V35-17 NEC relation dropped' = 'NEC-PROJECTION'; 'V35-18 trigger disjointness' = 'TRIGGER-DISJOINT'
    'V35-19 marker without producer' = 'MARKER-PRODUCER'; 'V35-20 second body observer' = 'BODY-OBSERVER'; 'V35-21 staging UNKNOWN dropped' = 'UNKNOWN-RULE'
    'V35-22 nested-in-abort UNKNOWN dropped' = 'UNKNOWN-RULE'; 'V35-23 guard arm target mismatch' = 'GUARD-TARGET'; 'V35-24 body observer on observation row' = 'BODY-OBSERVER'
    'C1 semantic wrong target, coherent' = 'BODY-NOTIFIER'; 'C2 coherent wrong resource category' = 'RESOURCE-TYPE'; 'C3 semantically invalid FailPredicate' = 'FP-APPLICABILITY'
    'C4 degraded cleanup, coherent' = 'CLEANUP-OBJ'; 'C5 retained authority rewritten' = 'RETAINED-V34-DRIFT'; 'C6 changed ObservationPredicate' = 'PREDICATE-SET'
    'C7 lock trigger removed from APPCTX driver' = 'SA-LEGAL'
    'RC-01a FINISH before final delivery' = 'FINISH-TOKEN-SET'; 'RC-01b FINISH issued by the script' = 'FINISH-ORDER'
    'RC-02a wrong unlock satisfies MARK-LOCK-RELEASE' = 'MARKER-STAGE-BINDING'; 'RC-02b FINISH unlock satisfies MARK-LOCK-RELEASE' = 'MARKER-STAGE-BINDING'
    'RC-03 driver APPCTX marker satisfies probe delivery' = 'DRIVER-MARKER-DISJOINT'; 'RC-04a cleanup safety hidden by UNKNOWN' = 'SAFETY-PRECEDENCE'
    'RC-04b safety exception removed from a row' = 'SAFETY-PRECEDENCE'; 'RC-05a incomplete evidence reaches FAIL/PASS' = 'PLAN'; 'RC-05b in-process FINISH classifies' = 'CONTROL-PLANE-RESULT'
    'RC-06 unauthorized retained extension' = 'RETAINED-AUTHORITY-STATUS'; 'RC-07a managed host unclassified' = 'RESOURCE-TYPE'; 'RC-07b managed host never loaded' = 'MANAGED-HOST'
    'RC-08a payload bound to any database' = 'PAYLOAD-DB-BINDING'; 'RC-08b payload DB observation dropped' = 'PAYLOAD-DB-BINDING'
    'RC-09 cancel staging equals ExpectedAfter bytes' = 'CANCEL-STAGING-DISTINCT'; 'RC-10 02NAPP-SM competing Model Space transaction' = 'APPEND-TRANSACTION-COMPATIBILITY'
    'RC-11a dynamic-linker guard key without module path' = 'GUARD-KEY-SCHEMA'; 'RC-11b open guard key without target' = 'GUARD-KEY-SCHEMA'
    'RC-12 guard disarmed before obligations' = 'GUARD-DISARM-AFTER-OBLIGATIONS'; 'RC-13 body notifier coherently corrupted' = 'BODY-NOTIFIER'
    'L1 APPCTX lock release anchored after the stage token' = 'LOCK-RELEASE-BINDING'; 'L2 cleanup closes the document inside FINISH' = 'FINISH-ORDER'
    'L3 late delivery not fenced' = 'FINISH-DRAIN'; 'L4 payload reactor not removable' = 'PAYLOAD-DB-BINDING'; 'L5 sync caller return outside its stage' = 'MARKER-STAGE-BINDING'
    'L6 cleanup records counted as safety violations' = 'SAFETY-PRECEDENCE'
    'X1 drop FP-STATE, row and lineage' = 'PREDICATE-SET'; 'X2 drop OBS-ORDER-RULES, row and lineage' = 'PREDICATE-SET'; 'X3 drop UNKNOWN predicates, row and lineage' = 'PREDICATE-SET'
    'X4 same-class trigger swap, coherent' = 'TRIGGER-ACTION'; 'X5 traceability resource dropped' = 'TRACE-RESOURCE-SET'; 'X6 traceability bindings forged' = 'TRACE'
    'X7 token dropped in row, trace and oracle' = 'INPUT-PIN'; 'X8 V34 baseline and catalog rewritten together' = 'INPUT-PIN'; 'X9 APPCTX unlock support removed' = 'SA-LEGAL'
    'X11 approval hash replaced in the oracle' = 'INPUT-PIN'; 'X12 FP-STATE removed from every row and lineage' = 'PREDICATE-SET'; 'X13 APPCTX driver disarms before obligations' = 'GUARD-DISARM-AFTER-OBLIGATIONS'
    'N2 erase trigger target in a write set' = 'ERASE-TRIGGER-DISJOINT'; 'N3 extended entry text changed' = 'RETAINED-EXTENSION-COMPATIBILITY'; 'N4 deferred cleanup without drain' = 'FINISH-DRAIN'
    'N5 FINISH timeout UNKNOWN dropped' = 'FINISH-TIMEOUT'; 'N6 document guard key needs a module path' = 'GUARD-KEY-TARGET'; 'N7 transaction guard key without stage' = 'GUARD-KEY-STAGE'
    'N8 lock-release observation dropped' = 'LOCK-RELEASE-BINDING'; 'N9 cancel restoration observation dropped' = 'CANCEL-STAGING-RESTORED'; 'N10 staging value becomes an expected state' = 'CANCEL-STAGING-NOT-EXPECTED'
}

# Controls from the fix-verification round (V*).
$controls['V1 late deliveries after removal no longer counted'] = { EditCatalog { param($c) $c['entries']['FINISH-FENCE-01']['lateDeliverySafety'] = 'NEVER-COUNTED' } }
$controls['V2a APPCTX driver phases rotated'] = { EditCatalog { param($c) $c['entries']['DRIVER-APP-01']['phaseOrder'] = @('TOKEN', 'ENQUEUE', 'TRIGGER', 'CALLBACK-WINDOW', 'POST-TRIGGER-OBLIGATIONS', 'OUTCOME-RECORD', 'DISARM', 'DELIVERY', 'ARM') } }
$controls['V2b command driver registers last'] = { EditCatalog { param($c) $c['entries']['DRIVER-CMD-01']['phaseOrder'] = @('TOKEN', 'TRIGGER', 'CALLBACK-WINDOW', 'POST-TRIGGER-OBLIGATIONS', 'OUTCOME-RECORD', 'DISARM', 'ARM', 'SETUP', 'REGISTER') } }
$controls['V3a cleanup text closes the drawing again'] = { EditCatalog { param($c) $c['entries']['CLN-BASE']['definition'] += ' Close the scratch DWG without save; flush.' } }
$controls['V3b finish fence text ignored by deliveries'] = { EditCatalog { param($c) $c['entries']['FINISH-FENCE-01']['definition'] = 'Deliveries ignore it and run their body.' } }
$controls['V3c payload reactor on an unarmed row'] = {
    EditRow '02NRXW-ALL' 'ObserverRegistrationIds' { param($v) "$v,RR-PAYLOAD-DB" }
    EditTrace '02NRXW-ALL' { param($r) $r['Resources'] += [ordered]@{ id = 'RR-PAYLOAD-DB'; kind = 'ObserverRegistration' } } }
$controls['V3d lock-release rule rewritten'] = { EditCatalog { param($c) $c['entries']['LOCK-RELEASE-BIND-01']['definition'] = 'First unlock anywhere, including FINISH.' } }
$controls['V4m cleanup family changed, coherent'] = {
    EditRow '16A-S' 'CleanupActionId' { 'CLEAN-CHAIN-PROCESS' }
    EditLineage '16A-S' 'Cleanup' { param($v) @('CLEAN-CHAIN-PROCESS') }
    EditTrace '16A-S' { param($r) $r['Cleanup'] = 'CLEAN-CHAIN-PROCESS' } }
$controls['V4q setup reordered'] = { EditRow 'C15N16N-SM' 'SetupActionIds' { 'SET-OUTCOME-COMMIT,SET-LOAD-PAYLOAD,SET-PAYLOAD-PATH,SET-OPEN-PRIMARY-T,SET-RETAIN-CALLBACK-DATA' } }
$controls['V4h extra locks'] = { EditRow '16A-S' 'DocumentLockIds' { param($v) "$v,CMDCTX-LOCK-01,CB-LOCK-01" } }
$controls['V4n extra guard'] = { EditRow '02NDBMOD-S' 'GuardIds' { param($v) "$v,RG-ONESHOT" } }
$controls['V4f lineage record emptied'] = { EditLineage '02N' 'FAIL' { param($v) @() } }
$controls['V4g traceability event forged'] = { EditTrace '02N' { param($r) $r['EventId'] = 'N-DB-ERASE' } }
$controls['V6 lock-release token on the FINISH stage'] = { EditCatalog { param($c) $c['entries']['TOK-LOCK-RELEASE']['stage'] = 'STG-FINISH' } }
$controls['V7 oracle source edited'] = { EditText 'eng/research/I52Ctda/oracle/oracle_predicates.py' 'FAIL_V34 = [' 'FAIL_V34 = [  # edited' }
$expect['V1 late deliveries after removal no longer counted'] = 'SAFETY-PRECEDENCE'; $expect['V2a APPCTX driver phases rotated'] = 'GUARD-DISARM-AFTER-OBLIGATIONS'
$expect['V2b command driver registers last'] = 'GUARD-DISARM-AFTER-OBLIGATIONS'; $expect['V3a cleanup text closes the drawing again'] = 'CATALOG-APPROVAL'
$expect['V3b finish fence text ignored by deliveries'] = 'CATALOG-APPROVAL'; $expect['V3c payload reactor on an unarmed row'] = 'PAYLOAD-DB-BINDING'
$expect['V3d lock-release rule rewritten'] = 'CATALOG-APPROVAL'; $expect['V4m cleanup family changed, coherent'] = 'RETAINED-DRIFT'
$expect['V4q setup reordered'] = 'SETUP-ORDER'; $expect['V4h extra locks'] = 'EXEC-LOCK'; $expect['V4n extra guard'] = 'GUARD-FAMILY'
$expect['V4f lineage record emptied'] = 'LINEAGE-EMPTY'; $expect['V4g traceability event forged'] = 'TRACE'
$expect['V6 lock-release token on the FINISH stage'] = 'LOCK-RELEASE-BINDING'; $expect['V7 oracle source edited'] = 'INPUT-PIN'

# V35-A2 controls (Architect MINOR M1): FINISH-FENCE-01 read ABI.
$controls['M1a fence read export removed'] = { EditCatalog { param($c) $c['entries']['LOG-SEQ-01']['exports'] = @($c['entries']['LOG-SEQ-01']['exports'] | Where-Object { $_ -ne 'I52Ctda_FinishFenceIsSet' }) } }
$controls['M1b fence read export renamed, coherent'] = { EditCatalog { param($c)
        foreach ($id in 'LOG-SEQ-01', 'FINISH-FENCE-01', 'R-PAYLOAD-ARX', 'R-MANAGED-OBSERVER') {
            $e = $c['entries'][$id]
            foreach ($a in @($e.Keys)) {
                if ($e[$a] -is [string]) { $e[$a] = $e[$a].Replace('I52Ctda_FinishFenceIsSet', 'I52Ctda_FenceQuery') }
                elseif ($e[$a] -is [System.Collections.IList] -and $e[$a].Count -and $e[$a][0] -is [string]) { $e[$a] = @($e[$a] | ForEach-Object { $_.Replace('I52Ctda_FinishFenceIsSet', 'I52Ctda_FenceQuery') }) }
            } } } }
$controls['M1c fence setter exported'] = { EditCatalog { param($c) $c['entries']['LOG-SEQ-01']['exports'] = @($c['entries']['LOG-SEQ-01']['exports']) + 'I52Ctda_FinishFenceSet' } }
$controls['M1d managed observer cannot read the fence'] = { EditCatalog { param($c) $c['entries']['R-MANAGED-OBSERVER']['imports'] = @($c['entries']['R-MANAGED-OBSERVER']['imports'] | Where-Object { $_ -ne 'I52Ctda_FinishFenceIsSet' }) } }
$controls['M1e payload imports a function LOG-SEQ-01 does not export'] = { EditCatalog { param($c) $c['entries']['R-PAYLOAD-ARX']['imports'] = @($c['entries']['R-PAYLOAD-ARX']['imports']) + 'I52Ctda_FinishFenceSet' } }
foreach ($m in 'M1a fence read export removed', 'M1b fence read export renamed, coherent', 'M1c fence setter exported', 'M1d managed observer cannot read the fence', 'M1e payload imports a function LOG-SEQ-01 does not export') { $expect[$m] = 'FINISH-FENCE-READ-ABI' }

$validator = 'eng/research/I52Ctda/validate-v35-catalog.ps1'
$inputEol = if ([IO.File]::ReadAllText((Join-Path $Repository $npmPath)).Contains("`r`n")) { 'CRLF' } else { 'LF' }
"INPUT EOL = $inputEol"
$results = [System.Collections.Generic.List[object]]::new()
$root = Join-Path ([IO.Path]::GetTempPath()) ("i52-v35-nc-" + [Guid]::NewGuid().ToString('N'))
try {
    # baseline: the unmodified copy must pass
    foreach ($name in @('BASELINE') + @($controls.Keys)) {
        $script:tmp = Join-Path $root ($results.Count.ToString('000'))
        foreach ($f in $inputs) { $dst = T $f; [void](New-Item -ItemType Directory -Force (Split-Path $dst)); Copy-Item (Join-Path $Repository $f) $dst }
        if ($name -ne 'BASELINE') { & $controls[$name] }
        $out = & pwsh -NoProfile -File (T $validator) -Repository $script:tmp -EvidencePath 'nc-evidence.json' 2>&1
        $code = $LASTEXITCODE
        $err = @($out | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] })
        $checks = @()
        if (Test-Path (T 'nc-evidence.json')) { $checks = @(((ReadJ 'nc-evidence.json')['findings']) | ForEach-Object { ($_['check'] -replace '^CONTRADICTION/', '') } | Sort-Object -Unique) }
        $want = if ($name -eq 'BASELINE') { $null } else { $expect[$name] }
        if ($name -ne 'BASELINE' -and -not $want) { throw "control '$name' declares no expected check" }
        $hit = ($name -eq 'BASELINE') -or ($checks -contains $want)
        $expectedExit = if ($name -eq 'BASELINE') { 0 } else { 1 }
        $ok = ($code -eq $expectedExit) -and ($err.Count -eq 0) -and $hit
        $results.Add([ordered]@{ control = $name; exit = $code; expectedCheck = $want; expectedCheckFired = $hit; checksFired = $checks; detected = ($name -ne 'BASELINE' -and $ok); ok = $ok; scriptErrors = $err.Count })
        '{0,-56} exit={1} {2} expect={3} fired={4}' -f $name, $code, $(if ($ok) { 'OK' } else { 'NOT OK' }), $want, (($checks | Select-Object -First 4) -join ',')
    }
} finally {
    if (Test-Path $root) { [IO.Directory]::Delete($root, $true) }
}
$total = $controls.Count
$caught = @($results | Where-Object { $_.control -ne 'BASELINE' -and $_.ok }).Count
$baselineOk = @($results | Where-Object { $_.control -eq 'BASELINE' -and $_.ok }).Count -eq 1
"BASELINE PASS = $baselineOk"
"NEGATIVE CONTROLS DETECTED = $caught/$total"
if ($EvidencePath) {
    $ev = [ordered]@{ schemaVersion = 1; revision = 'V35-A2'; harness = 'eng/research/I52Ctda/validate-v35-negative-controls.ps1'; inputEol = $inputEol; baselinePass = $baselineOk; total = $total; detected = $caught; controls = $results }
    [IO.File]::WriteAllText((Join-Path $Repository $EvidencePath), ($ev | ConvertTo-Json -Depth 6) + "`n")
}
if (-not $baselineOk -or $caught -ne $total) { exit 1 }
