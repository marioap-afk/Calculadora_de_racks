# I-52 V35 document/catalog validator. Static only: parses the V35 documents and catalog, never runs AutoCAD,
# never builds native code and never evaluates runtime semantics. It proves the V35 closure predicates mechanically.
param(
    [string]$Repository = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path,
    [string]$SdkRoot = '',
    [string]$EvidencePath = ''
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function P([string]$rel) { Join-Path $Repository $rel }
function ReadText([string]$rel) { [IO.File]::ReadAllText((P $rel)) }
function ReadJson([string]$rel) { ReadText $rel | ConvertFrom-Json -AsHashtable }

$findings = [System.Collections.Generic.List[object]]::new()
function Flag([string]$check, [string]$probe, [string]$detail) {
    $findings.Add([ordered]@{ check = $check; probeId = $probe; detail = $detail })
}

# ------------------------------------------------------------------ inputs
$catalog = ReadJson 'docs/initiatives/I-52-execution-catalog-v35.json'
$Cat = $catalog['entries']
$cols = [string[]]$catalog['rowSchema']
$fixture = ReadJson 'eng/research/I52Ctda/fixture-v35.json'
$trace = ReadJson 'eng/research/I52Ctda/traceability-v35.json'
$trace34 = ReadJson 'eng/research/I52Ctda/traceability-v34.json'
$lineage = ReadJson 'docs/automation/evidence/I-52-v35-clause-lineage.json'
$discovery = ReadJson 'docs/automation/evidence/I-52-r3-governed-executor-discovery.json'
$oracle = ReadJson 'eng/research/I52Ctda/v35-oracle.json'

function MatrixLines([string]$rel, [string]$heading) {
    $text = ReadText $rel
    $at = $text.IndexOf($heading)
    if ($at -lt 0) { throw "heading '$heading' missing in $rel" }
    $open = $text.IndexOf('```text', $at)
    $close = $text.IndexOf('```', $open + 7)
    $text.Substring($open + 7, $close - $open - 7).Split("`n") | ForEach-Object { $_.Trim() } | Where-Object { $_ }
}

$cols34 = 'ProbeId;PrimaryAuthorityId;ScheduleOriginEventId;HeaderAuthority;Setup;Trigger;PrimaryTransactionId;TopTransactionId;DocumentLock;Thread/Context;Mutation;S/M/SM;SchedulePoint;ExecutionPoint;ExpectedBefore;ExpectedAfter;Verifier;LifecycleMarkers;Threats;DA-P/H-P;PASS;FAIL;UNKNOWN;Cleanup'.Split(';')
$rows34 = [ordered]@{}
foreach ($line in (MatrixLines 'docs/initiatives/I-52-native-probe-matrix-v34.md' '## 10. Full normative matrix')) {
    $f = $line.Split(';')
    if ($f.Count -ne 24) { Flag 'V34-PARSE' $f[0] "fields=$($f.Count)"; continue }
    $h = [ordered]@{}; for ($i = 0; $i -lt 24; $i++) { $h[$cols34[$i]] = $f[$i] }
    $rows34[$f[0]] = $h
}

$rows = [ordered]@{}
$rawRows = [ordered]@{}
$inputLines = @(MatrixLines 'docs/initiatives/I-52-native-probe-matrix-v35.md' '## 5. Full normative matrix')
foreach ($line in $inputLines) {
    $f = $line.Split(';')
    if ($f.Count -ne $cols.Count) { Flag 'V35-PARSE' $f[0] "fields=$($f.Count) expected=$($cols.Count)"; continue }
    if ($rows.Contains($f[0])) { Flag 'V35-DUPLICATE' $f[0] 'duplicate ProbeId'; continue }
    $h = [ordered]@{}
    for ($i = 0; $i -lt $cols.Count; $i++) {
        $v = $f[$i]
        $h[$cols[$i]] = if ($v -eq 'NONE') { @() } else { @($v.Split(',')) }
    }
    $rows[$f[0]] = $h
    $raw = [ordered]@{}; for ($i = 0; $i -lt $cols.Count; $i++) { $raw[$cols[$i]] = $f[$i] }; $rawRows[$f[0]] = $raw
}
function One($row, [string]$col) { $v = @($row[$col]); if ($v.Count -eq 0) { return 'NONE' }; return [string]$v[0] }
function Kind([string]$id) { if ($Cat.ContainsKey($id)) { return [string]$Cat[$id]['kind'] }; return $null }

# ------------------------------------------------------------------ binding: every identifier resolves with the expected kind
$kinds = @{
    PrimaryAuthorityId = @('EventId', 'SchedulerId', 'RetainedContract'); ScheduleOriginEventId = @('EventId', 'Sentinel')
    SchedulerChainId = @('SchedulerChain'); HeaderAuthority = @('RetainedContract'); DriverId = @('Driver')
    ObserverRegistrationIds = @('ObserverRegistration'); BodyObserverId = @('ObserverRegistration'); GuardIds = @('Guard'); SetupActionIds = @('SetupAction')
    TriggerActionId = @('TriggerAction'); ExecutionContextId = @('ExecutionContext'); PrimaryTransactionId = @('TransactionId')
    ExecutionTransactionId = @('TransactionId'); TopTransactionAuthorityIds = @('TopTransactionAuthority')
    DocumentLockIds = @('DocumentLockAuthority'); ThreadContextIds = @('RetainedContract'); MutationActionId = @('MutationAction')
    Surface = @('Surface'); SchedulePoint = @('RetainedContract'); ExecutionPoint = @('RetainedContract')
    ExpectedBefore = @('RetainedContract'); ExpectedAfter = @('RetainedContract'); VerifierIds = @('RetainedContract')
    Markers = @('EventId', 'RetainedContract', 'Marker'); ObservationPredicateIds = @('ObservationPredicate')
    UnknownPredicateIds = @('UnknownPredicate', 'RetainedContract'); FailPredicateIds = @('FailPredicate')
    PassClass = @('RetainedContract', 'ResultClass'); FailClass = @('RetainedContract', 'ResultClass')
    CleanupActionId = @('CleanupAction'); CompletionFence = @('CompletionFence'); CompletionTokenIds = @('CompletionToken')
    MarkerStageBindings = @('Stage')
}
$nonExecutable = @('ProbeId', 'Threats', 'DA-P/H-P')
$unbound = 0; $freeText = 0; $idCount = 0
foreach ($probe in $rows.Keys) {
    $r = $rows[$probe]
    foreach ($c in $cols) {
        if ($nonExecutable -contains $c) { continue }
        foreach ($tok in @($r[$c])) {
            $parts = if ($c -eq 'ExpectedBefore' -or $c -eq 'ExpectedAfter') { $tok.Split('+') } elseif ($c -eq 'MarkerStageBindings') { if ($tok -notmatch '^[A-Z-]+@STG-[A-Z-]+$') { $freeText++; Flag 'FREE-TEXT' $probe "$c=$tok" }; if (-not $Cat.ContainsKey($tok.Split('@')[0])) { $unbound++; Flag 'UNBOUND' $probe "$c=$tok" }; @($tok.Split('@')[1]) } else { @($tok) }
            foreach ($x in $parts) {
                $id = if ($c -eq 'Markers') { if ($x -notmatch '^[+-]') { Flag 'MARKER-POLARITY' $probe $x }; $x.Substring(1) } else { $x }
                $idCount++
                $k = Kind $id
                if ($null -eq $k) { $unbound++; Flag 'UNBOUND' $probe "$c=$id"; continue }
                if ($k -ne 'Sentinel' -and $id -match '\s') { $freeText++; Flag 'FREE-TEXT' $probe "$c=$id" }
                if ($kinds[$c] -notcontains $k) { $unbound++; Flag 'WRONG-KIND' $probe "$c=$id kind=$k" }
            }
        }
    }
    foreach ($c in 'Threats', 'DA-P/H-P') {
        foreach ($tok in @($r[$c])) { if ($tok -notmatch '^(T\d+(-[A-Z]+)?|P\d+)$') { $freeText++; Flag 'FREE-TEXT' $probe "$c=$tok" } }
    }
}

# ------------------------------------------------------------------ catalog internal references
$catalogRefErrors = 0
foreach ($id in $Cat.Keys) {
    $e = $Cat[$id]
    $refs = [System.Collections.Generic.List[string]]::new()
    switch ($e['kind']) {
        'ExecutionContext' { $refs.Add($e['transaction']); foreach ($x in $e['locks']) { $refs.Add($x) }; foreach ($x in $e['usesSupport']) { $refs.Add($x) } }
        'TriggerAction' { if ($e['target']) { $refs.Add($e['target']) }; $refs.Add($e['driver']); foreach ($x in $e['produces']) { $refs.Add($x) }; foreach ($x in $e['usesSupport']) { $refs.Add($x) } }
        'SetupAction' { foreach ($x in $e['usesSupport']) { $refs.Add($x) } }
        'CleanupAction' { foreach ($x in $e['steps']) { $refs.Add($x) }; $refs.Add($e['fence']) }
        'SchedulerChain' { foreach ($x in $e['sequence']) { $refs.Add($x) } }
        'MutationAction' { foreach ($x in $e['writeSet']) { $refs.Add($x) } }
        'Surface' { $refs.Add($e['passClass']); $refs.Add($e['failClass']); foreach ($x in $e['mutations']) { $refs.Add($x) } }
        'ObserverRegistration' { if ($e['notifier'] -match '^F-') { $refs.Add($e['notifier']) } }
    }
    if ($e.ContainsKey('produces')) { foreach ($x in $e['produces']) { if ((Kind $x) -ne 'EventId') { $refs.Add("EVENT:$x") } } }
    foreach ($x in $refs) { if (-not $Cat.ContainsKey($x)) { $catalogRefErrors++; Flag 'CATALOG-REF' $id "unresolved $x" } }
}
foreach ($k in 'instrumentationMarkers') { foreach ($x in $catalog[$k]) { if (-not $Cat.ContainsKey($x)) { $catalogRefErrors++; Flag 'CATALOG-REF' $k $x } } }

# ------------------------------------------------------------------ lineage: every V34 executable clause has a class A/B/C record
$classD = 0; $lineageMissing = 0
$lin = @{}; foreach ($lr in $lineage['rows']) { $lin[$lr['ProbeId']] = $lr['lineage'] }
$lineageField = @{
    ObserverRegistration = 'ObserverRegistrationIds'; SetupAction = 'SetupActionIds'; TriggerAction = 'TriggerActionId'; Guard = 'GuardIds'
    SchedulerChain = 'SchedulerChainId'; ExecutionContext = 'ExecutionContextId'; Driver = 'DriverId'; CleanupAction = 'CleanupActionId'
    MutationAction = 'MutationActionId'; TransactionId = 'PrimaryTransactionId'; TopTransactionAuthority = 'TopTransactionAuthorityIds'
    DocumentLockAuthority = 'DocumentLockIds'; ObservationPredicate = 'ObservationPredicateIds'; FailPredicate = 'FailPredicateIds'
    UnknownPredicate = 'UnknownPredicateIds'; ResultClass = 'PassClass'
}
$linCols = @('Setup', 'Trigger', 'PrimaryTransactionId', 'TopTransactionId', 'DocumentLock', 'Thread/Context', 'Mutation', 'LifecycleMarkers', 'PASS', 'FAIL', 'UNKNOWN', 'Cleanup')
foreach ($probe in $rows34.Keys) {
    if (-not $lin.ContainsKey($probe)) { $lineageMissing++; Flag 'LINEAGE-ROW' $probe 'missing'; continue }
    $recs = $lin[$probe]
    foreach ($rec in $recs) {
        if ('A', 'B', 'C' -notcontains $rec['class']) { $classD++; Flag 'CLASS-D' $probe "$($rec['column']): $($rec['clause'])" }
        foreach ($x in $rec['v35']) {
            $id = ($x -replace '^[A-Za-z]+=', '') -replace '^[+-]', ''
            if ($id -in 'ExpectedBefore', 'GuardIds', 'body observer' -or $rec['column'] -in 'Threats', 'DA-P/H-P') { continue }
            if ($id -match '@') { if (-not ($Cat.ContainsKey($id.Split('@')[0]) -and $Cat.ContainsKey($id.Split('@')[1]))) { $lineageMissing++; Flag 'LINEAGE-ID' $probe "$($rec['column']) -> $x" }; continue }
            if (-not $Cat.ContainsKey($id)) { $lineageMissing++; Flag 'LINEAGE-ID' $probe "$($rec['column']) -> $x" }
        }
    }
    # lineage -> row agreement: an identifier that a V34 clause was formalized into must be present in the V35 row field
    $r35 = $rows[$probe]
    foreach ($rec in $recs) {
        foreach ($x in $rec['v35']) {
            $field = $null; $id = $x
            if ($x -match '^([A-Za-z]+)=(.+)$') { $field = $Matches[1]; $id = $Matches[2] }
            elseif ($rec['column'] -eq 'LifecycleMarkers' -and $x -match '^[+-]') { $field = 'Markers' }
            else { $kd = Kind $id; $field = if ($kd -and $lineageField.ContainsKey($kd)) { $lineageField[$kd] } else { $null } }
            if (-not $field -or -not $r35) { continue }
            if ($rec['column'] -eq 'FAIL' -and $field -eq 'PassClass') { $field = 'FailClass' }
            if (@($r35[$field]) -notcontains $id) { $lineageMissing++; Flag 'LINEAGE-ROW-AGREEMENT' $probe "$($rec['column']) -> $x not in $field" }
        }
    }
    foreach ($c in $linCols) {
        $cell = $rows34[$probe][$c]
        $clauses = if ($c -eq 'Setup') { $cell.Split(',') | ForEach-Object { $_.Trim() } } elseif ($c -eq 'LifecycleMarkers' -and $cell -notmatch '^RG-') { $cell.Split(',') } else { @($cell) }
        foreach ($cl in $clauses) {
            if (-not ($recs | Where-Object { $_['column'] -eq $c -and $_['clause'] -eq $cl })) { $lineageMissing++; Flag 'LINEAGE-CLAUSE' $probe "$c`: $cl" }
        }
    }
}

# ------------------------------------------------------------------ retained columns unchanged from V34
$retainedDrift = 0
$sameCols = @{ PrimaryAuthorityId = 'PrimaryAuthorityId'; ScheduleOriginEventId = 'ScheduleOriginEventId'; Surface = 'S/M/SM'; SchedulePoint = 'SchedulePoint'
    ExecutionPoint = 'ExecutionPoint'; ExpectedBefore = 'ExpectedBefore'; ExpectedAfter = 'ExpectedAfter' }
foreach ($probe in $rows.Keys) {
    if (-not $rows34.Contains($probe)) { $retainedDrift++; Flag 'PROBEID-SET' $probe 'not in V34'; continue }
    $a = $rows[$probe]; $b = $rows34[$probe]
    foreach ($k in $sameCols.Keys) { if ((One $a $k) -ne $b[$sameCols[$k]]) { $retainedDrift++; Flag 'RETAINED-DRIFT' $probe $k } }
    if ((@($a['HeaderAuthority']) -join '+') -ne $b['HeaderAuthority']) { $retainedDrift++; Flag 'RETAINED-DRIFT' $probe 'HeaderAuthority' }
    if ((@($a['VerifierIds']) -join '+') -ne $b['Verifier']) { $retainedDrift++; Flag 'RETAINED-DRIFT' $probe 'Verifier' }
    if ((@($a['Threats']) -join '/') -ne $b['Threats']) { $retainedDrift++; Flag 'RETAINED-DRIFT' $probe 'Threats' }
    if ((@($a['DA-P/H-P']) -join '/') -ne $b['DA-P/H-P']) { $retainedDrift++; Flag 'RETAINED-DRIFT' $probe 'DA-P/H-P' }
    if ((@($a['ThreadContextIds']) -join ' then ') -ne $b['Thread/Context']) { $retainedDrift++; Flag 'RETAINED-DRIFT' $probe 'Thread/Context' }
}
foreach ($probe in $rows34.Keys) { if (-not $rows.Contains($probe)) { $retainedDrift++; Flag 'PROBEID-SET' $probe 'missing in V35' } }

# ------------------------------------------------------------------ contradictions
$contra = [ordered]@{}
function C([string]$name, [string]$probe, [string]$detail) { if (-not $contra.Contains($name)) { $contra[$name] = 0 }; $contra[$name]++; Flag "CONTRADICTION/$name" $probe $detail }
function Fam([string]$id) { foreach ($p in 'N-DB-', 'N-TR-', 'N-ED-', 'N-DOC-', 'N-OBJ-', 'N-ENT-', 'N-RX-') { if ($id.StartsWith($p)) { return $p } }; return $null }
$instr = [string[]]$catalog['instrumentationMarkers']
$guardByEvent = @{ 'N-DB-MOD' = 'RG-DB-MOD'; 'N-DB-ERASE' = 'RG-DB-ERASE'; 'N-DB-APPEND' = 'RG-DB-APPEND'; 'N-DB-OPEN' = 'RG-DB-OPEN' }
$chainPoints = @{
    'CHAIN-SEND' = @('SP-SEND', 'EP-COMMAND', 'SEND-EXEC-01'); 'CHAIN-APPCTX' = @('SP-APPCTX', 'EP-APPCTX', 'APPCTX-EXEC-01')
    'CHAIN-APPCTX-CMDCTX' = @('SP-CMDCTX', 'EP-CMDCTX', 'CMDCTX-EXEC-01'); 'CHAIN-SYNC-CMDCTX' = @('SP-CMDCTX', 'EP-CMDCTX', 'CMDCTX-EXEC-01')
    'CHAIN-SYNC' = @('SP-IMMEDIATE', 'EP-APPCTX', 'APPCTX-EXEC-01')
}
$forbidden = @(' or ', 'either', 'chosen', 'actual result', 'whatever occurred', 'recorded outcome')
$saUsed = [System.Collections.Generic.HashSet[string]]::new()
$traceById = @{}; foreach ($t in $trace) { $traceById[$t['ProbeId']] = $t }
$plans = 0

foreach ($probe in $rows.Keys) {
    $r = $rows[$probe]
    $prim = One $r 'PrimaryAuthorityId'; $orig = One $r 'ScheduleOriginEventId'; $chain = One $r 'SchedulerChainId'
    $trg = One $r 'TriggerActionId'; $exec = One $r 'ExecutionContextId'; $drv = One $r 'DriverId'; $mut = One $r 'MutationActionId'
    $surf = One $r 'Surface'; $ptx = One $r 'PrimaryTransactionId'; $clean = One $r 'CleanupActionId'
    $setup = @($r['SetupActionIds']); $obs = @($r['ObserverRegistrationIds']); $guards = @($r['GuardIds']); $markers = @($r['Markers'])
    $opreds = @($r['ObservationPredicateIds']); $fpreds = @($r['FailPredicateIds'])
    $body = if ((Kind $prim) -eq 'EventId') { $prim } elseif ((Kind $orig) -eq 'EventId') { $orig } else { $null }
    $TR = $Cat[$trg]; $XC = $Cat[$exec]; $CLN = $Cat[$clean]; $SF = $Cat[$surf]; $CHN = $Cat[$chain]
    if ($null -eq $TR -or $null -eq $XC -or $null -eq $CLN -or $null -eq $SF -or $null -eq $CHN) { continue }

    # driver and trigger
    if ($TR['driver'] -ne $drv) { C 'DRIVER-TRIGGER' $probe "$drv vs $($TR['driver'])" }
    if ($body -and $TR['produces'].Count -gt 0 -and ($TR['produces'] -notcontains $body)) { C 'TRIGGER-PRODUCES' $probe "$trg does not produce $body" }
    # body observer and guard (INV-BODY-OBSERVER)
    $bodyObs = One $r 'BodyObserverId'
    $bodyRuns = [bool]$body -and (($exec -ne 'EXEC-OBSERVE') -or ($chain -ne 'CHAIN-NONE'))
    if ($bodyRuns) {
        if ($obs -notcontains $bodyObs -or $Cat[$bodyObs]['families'] -notcontains (Fam $body)) { C 'BODY-OBSERVER' $probe "BodyObserverId $bodyObs does not observe $body" }
    } elseif ($bodyObs -ne 'NONE') { C 'BODY-OBSERVER' $probe "BodyObserverId $bodyObs on a row without a probe body" }
    if ($body) {
        $bf = Fam $body
        $bobs = @($obs | Where-Object { $Cat[$_]['families'] -contains $bf })
        if ($bobs.Count -lt 1) { C 'BODY-OBSERVER' $probe "no observer for $body" }
        if ($bodyRuns) {
            $want = if ($guardByEvent.ContainsKey($body)) { $guardByEvent[$body] } elseif ($bf -eq 'N-TR-') { 'RG-TX' } elseif ($body -eq 'N-OBJ-ERASE' -and $TR['target'] -eq 'F-TRIGGER-ERASE-OBJ') { 'RG-OBJ-ERASE' } else { 'RG-ONESHOT' }
            if ($guards -notcontains $want) { C 'GUARD-FAMILY' $probe "expected $want" }
        }
    }
    foreach ($g in $guards) {
        $arm = [string]$Cat[$g]['armTarget']
        if ($arm -match '^F-' -and $TR['target'] -ne $arm) { C 'GUARD-TARGET' $probe "$g arms $arm but $trg targets $($TR['target'])" }
        if ($arm -eq 'TRIGGER-TARGET' -and -not $TR['target']) { C 'GUARD-TARGET' $probe "$g needs a trigger target" }
        $gf = $Cat[$g]['family']
        $ok = ($gf -eq '*') -or ($body -and ($body -eq $gf -or $body.StartsWith($gf)))
        if (-not $ok) { C 'GUARD-FAMILY' $probe "$g does not match $body" }
    }
    if ($exec -eq 'EXEC-OBSERVE' -and $chain -eq 'CHAIN-NONE') {
        foreach ($g in $guards) { if (-not ($g -eq 'RG-ONESHOT' -and $setup -contains 'SET-ARM-VETO')) { C 'OBSERVE-GUARD' $probe $g } }
    }
    # markers (INV-MARKER-OBSERVER, INV-MARKER-PRODUCER, polarity)
    $producible = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($src in @($drv, $trg, $chain, $exec, $mut) + $setup) {
        if ($Cat.ContainsKey($src) -and $Cat[$src].ContainsKey('produces')) { foreach ($y in $Cat[$src]['produces']) { [void]$producible.Add($y) } }
    }
    $seen = @{}
    foreach ($m in $markers) {
        $id = $m.Substring(1); $pol = $m.Substring(0, 1)
        if ($seen.ContainsKey($id) -and $seen[$id] -ne $pol) { C 'MARKER-POLARITY' $probe "$id both polarities" }
        $seen[$id] = $pol
        if ($pol -eq '-' -and (Kind $id) -ne 'EventId') { C 'MARKER-POLARITY' $probe "absent marker $id is not an EventId" }
        if ($pol -eq '+' -and (Kind $id) -eq 'EventId') {
            if (-not $producible.Contains($id)) { C 'MARKER-PRODUCER' $probe "no declared producer for $m" }
            elseif ((Fam $id) -in 'N-OBJ-', 'N-ENT-') {
                # object/entity events must be produced on an object that a row observer watches
                $watched = @($obs | Where-Object { $Cat[$_]['objectReactor'] } | ForEach-Object { $Cat[$_]['notifier'] })
                $staged = @($setup | ForEach-Object { if ($_ -eq 'SET-STAGE-S-IN-PRIMARY') { $Cat[$_]['touches'] } })
                $hit = $false
                foreach ($src in @($trg, $mut) + $setup) {
                    $e2 = $Cat[$src]
                    if (-not $e2.ContainsKey('produces') -or $e2['produces'] -notcontains $id) { continue }
                    $touch = if ($e2['kind'] -eq 'MutationAction') { @($e2['writeSet']) } else { @($e2['touches']) }
                    $touch = @($touch | ForEach-Object { if ($_ -eq 'STAGED') { $staged } elseif ($_ -eq 'ROW-WRITES') { $Cat[$mut]['writeSet'] + $staged } else { $_ } })
                    if (@($touch | Where-Object { $watched -contains $_ }).Count -gt 0) { $hit = $true }
                }
                if (-not $hit) { C 'MARKER-PRODUCER' $probe "no producer of $m touches a watched object ($($watched -join ','))" }
            }
        }
        if ($instr -contains $id) { continue }
        $f = Fam $id
        $has = @($obs | Where-Object { ($f -and $Cat[$_]['families'] -contains $f) -or ($Cat[$_]['families'] -contains $id) })
        if ($has.Count -eq 0) { C 'MARKER-OBSERVER' $probe "no observer for $m" }
    }
    # execution context, transactions and locks
    if ((One $r 'ExecutionTransactionId') -ne $XC['transaction']) { C 'EXEC-TRANSACTION' $probe "$exec vs $(One $r 'ExecutionTransactionId')" }
    foreach ($l in $XC['locks']) { if (@($r['DocumentLockIds']) -notcontains $l) { C 'EXEC-LOCK' $probe "missing $l" } }
    if (@($r['DocumentLockIds']) -notcontains 'LOCK-OBS') { C 'EXEC-LOCK' $probe 'missing LOCK-OBS' }
    if ((@($r['DocumentLockIds']) -contains 'DRIVER-LOCK-01') -ne ($trg -in 'TRG-LOCK-CYCLE', 'TRG-LOCK-VETO')) { C 'DRIVER-LOCK' $probe $trg }
    $opensT = $setup -contains 'SET-OPEN-PRIMARY-T'
    switch ($ptx) {
        'T-ABORTING' { if (-not $opensT -or $trg -ne 'TRG-ABORT-PRIMARY-T') { C 'PRIMARY-T' $probe $ptx } }
        'T-ENDED' { if (-not $opensT -or $trg -ne 'TRG-END-PRIMARY-T') { C 'PRIMARY-T' $probe $ptx } }
        'T-PRIMARY' { if (-not $opensT -or $trg -eq 'TRG-ABORT-PRIMARY-T') { C 'PRIMARY-T' $probe $ptx } }
        'T-CONTROLLED' { if ($opensT -or $trg -ne 'TRG-START-OUTER-T') { C 'PRIMARY-T' $probe $ptx } }
        'NONE-OPENED' { if ($opensT -or $trg -in 'TRG-ABORT-PRIMARY-T', 'TRG-END-PRIMARY-T', 'TRG-END-NESTED-T', 'TRG-START-OUTER-T') { C 'PRIMARY-T' $probe $ptx } }
        default { C 'PRIMARY-T' $probe "unexpected $ptx" }
    }
    $wantCommit = $opensT -and ($trg -notin 'TRG-ABORT-PRIMARY-T', 'TRG-END-PRIMARY-T')
    if (($setup -contains 'SET-OUTCOME-COMMIT') -ne $wantCommit) { C 'OUTCOME-COMMIT' $probe 'SET-OUTCOME-COMMIT mismatch' }
    if (($setup -contains 'SET-OPEN-NESTED-T') -ne ($trg -eq 'TRG-END-NESTED-T')) { C 'NESTED-T' $probe 'nested setup/trigger mismatch' }
    if (($setup -contains 'SET-STAGE-S-IN-PRIMARY') -and -not $opensT) { C 'STAGE-T' $probe 'staging without T-PRIMARY' }
    if ($exec -eq 'CB-PRIMARY-01' -and $ptx -ne 'T-PRIMARY') { C 'CB-PRIMARY-T' $probe $ptx }
    # chain, points and scheduler authority
    $seq = @($CHN['sequence'])
    if ((Kind $prim) -eq 'SchedulerId' -and $seq[-1] -ne $prim) { C 'CHAIN-PRIMARY' $probe "$chain vs $prim" }
    if ($prim -eq 'NX-APPCTX-SYNC' -and $chain -ne 'CHAIN-SYNC') { C 'CHAIN-PRIMARY' $probe $chain }
    if ((Kind $prim) -eq 'EventId' -and $chain -ne 'CHAIN-NONE') { C 'CHAIN-PRIMARY' $probe $chain }
    if ($chain -eq 'CHAIN-NONE') {
        if ((One $r 'SchedulePoint') -ne 'SP-IMMEDIATE' -or (One $r 'ExecutionPoint') -ne 'EP-CALLBACK' -or $exec -notin 'EXEC-OBSERVE', 'CB-PRIMARY-01', 'CB-EXEC-01', 'EDC-EXEC-01') { C 'CHAIN-POINTS' $probe $chain }
    } else {
        $cp = $chainPoints[$chain]
        if ((One $r 'SchedulePoint') -ne $cp[0] -or (One $r 'ExecutionPoint') -ne $cp[1] -or $exec -ne $cp[2]) { C 'CHAIN-POINTS' $probe $chain }
        if (-not ($setup | Where-Object { $_ -like 'SET-RETAIN-*' })) { C 'CHAIN-DATA' $probe 'no retained data' }
    }
    # INV-TRIGGER-DISJOINT
    if ($exec -in 'CB-PRIMARY-01', 'CB-EXEC-01', 'EDC-EXEC-01' -and $TR['target'] -and ($Cat[$mut]['writeSet'] -contains $TR['target'])) { C 'TRIGGER-DISJOINT' $probe "$trg target in $mut" }
    # INV-ERASE-TRIGGER-DISJOINT
    if ($trg -like 'TRG-ERASE-*' -and ($Cat[$mut]['writeSet'] -contains $TR['target'])) { C 'ERASE-TRIGGER-DISJOINT' $probe "$trg target in $mut" }
    # surface / mutation / expected state / result classes
    if ($SF['mutations'] -notcontains $mut) { C 'SURFACE-MUTATION' $probe "$surf $mut" }
    $ea = One $r 'ExpectedAfter'
    if ($mut -ne 'MUT-NONE') {
        if ($ea -ne ($SF['statePrefix'] + '1')) { C 'SURFACE-STATE' $probe "$mut -> $ea" }
    } elseif (-not ($ea.StartsWith('STATE-T-') -or $ea -eq ($SF['statePrefix'] + '0'))) { C 'SURFACE-STATE' $probe "observation -> $ea" }
    if ((One $r 'PassClass') -ne $SF['passClass'] -or (One $r 'FailClass') -ne $SF['failClass']) { C 'RESULT-CLASS' $probe $surf }
    if ($exec -eq 'EXEC-OBSERVE' -and $mut -ne 'MUT-NONE') { C 'OBSERVE-MUTATION' $probe $mut }
    if ($exec -ne 'EXEC-OBSERVE' -and $mut -eq 'MUT-NONE') { C 'EXEC-MUTATION' $probe $exec }
    # ExpectedAfter determinism (NPM-V34 §6 applied to resolved definitions)
    foreach ($part in $ea.Split('+')) {
        $def = [string]$Cat[$part]['definition']
        foreach ($w in $forbidden) {
            if ($def.ToLowerInvariant().Contains($w) -and -not $catalog['determinismExceptions'].ContainsKey($part)) { C 'EXPECTED-DETERMINISM' $probe "$part contains '$w'" }
        }
    }
    # cleanup coverage (INV-SINGLE-TERMINAL-BASE and resource fences)
    $steps = @($CLN['steps'])
    $bases = @($steps | Where-Object { $_ -eq 'CLN-BASE' })
    $bi = [Array]::IndexOf($steps, 'CLN-BASE')
    $tail = if ($bi -ge 0 -and $bi + 1 -lt $steps.Count) { @($steps[($bi + 1)..($steps.Count - 1)]) } else { @() }
    if ($bases.Count -ne 1 -or ($tail | Where-Object { $_ -ne 'CLN-PROCESS-EXIT' })) { C 'TERMINAL-BASE' $probe $clean }
    if ((One $r 'CompletionFence') -ne $CLN['fence']) { C 'FENCE' $probe $clean }
    $fenced = $CLN['fence'] -eq 'FENCE-PROCESS-EXIT'
    $objObs = @($obs | Where-Object { $Cat[$_]['objectReactor'] })
    if ($objObs.Count -gt 0 -and -not ($steps | Where-Object { $_ -like 'CLN-OBJ-*' }) -and -not $fenced) { C 'CLEANUP-OBJ' $probe $clean }
    if ($steps -contains 'CLN-OBJ-RETAIN-FENCED' -and -not $fenced) { C 'CLEANUP-OBJ' $probe 'retained reactor without process fence' }
    if (($steps | Where-Object { $_ -like 'CLN-OBJ-*' }) -and $objObs.Count -eq 0) { C 'CLEANUP-OBJ' $probe 'object cleanup without object observer' }
    if ($chain -notin 'CHAIN-NONE', 'CHAIN-SYNC' -and $steps -notcontains 'CLN-DEFER-DRAIN' -and -not $fenced) { C 'CLEANUP-DEFER' $probe $clean }
    if ($exec -in 'APPCTX-EXEC-01', 'CMDCTX-EXEC-01', 'EDC-EXEC-01' -and -not $fenced) { C 'CLEANUP-PROCESS' $probe $clean }
    $payload = ($setup -contains 'SET-PAYLOAD-PATH')
    if ($payload -and -not $fenced) { C 'CLEANUP-PAYLOAD' $probe $clean }
    if (($trg -eq 'TRG-LOAD-PAYLOAD' -or $setup -contains 'SET-LOAD-PAYLOAD') -and -not $payload) { C 'PAYLOAD-PATH' $probe $trg }
    # PASS rule completeness (independent re-derivation of RESULT-RULE-V35 obligations)
    $need = [System.Collections.Generic.List[string]]::new()
    if ($markers.Count) { $need.Add('OBS-MARKERS') }
    if ($exec -ne 'EXEC-OBSERVE') { $need.Add('OBS-EXEC-COMPLETE'); $need.Add('OBS-T-AFFILIATION') }
    if ($guards.Count) { $need.Add('OBS-GUARD-RESET') }
    if ($setup -contains 'SET-OUTCOME-COMMIT') { $need.Add('OBS-OUTCOME-COMMIT') }
    if ((Kind $prim) -eq 'EventId') { $need.Add('OBS-PRIMARY-CALLBACK') }
    if ((Kind $orig) -eq 'EventId') { $need.Add('OBS-ORIGIN-CALLBACK') }
    if ($seq | Where-Object { (Kind $_) -eq 'SchedulerId' }) { $need.Add('OBS-ENQUEUE-OK'); $need.Add('OBS-DELIVERY'); $need.Add('OBS-CAUSAL-ORDER') }
    if ($seq -contains 'NX-APPCTX-SYNC') { $need.Add('OBS-SYNC-ENTRY-RETURN') }
    if (@($r['ThreadContextIds']) | Where-Object { $_ -in 'CTX-APPLICATION', 'CTX-COMMAND' }) { $need.Add('OBS-CONTEXT') }
    if ($trg -eq 'TRG-CANCEL-CMDCTX') { $need.Add('OBS-CANCELLATION') }
    if ($trg -eq 'TRG-LOCK-VETO') { $need.Add('OBS-VETO-OK'); if ($fpreds -notcontains 'FP-VETO-BYPASS') { C 'VETO-FAIL' $probe 'FP-VETO-BYPASS missing' } }
    if ($payload) { $need.Add('OBS-LOAD') }
    if ($setup -contains 'SET-STAGE-S-IN-PRIMARY') { $need.Add('OBS-STAGED'); if (@($r['UnknownPredicateIds']) -notcontains 'UNK-STAGING') { C 'UNKNOWN-RULE' $probe 'UNK-STAGING missing' } }
    if ($exec -eq 'CB-EXEC-01' -and $ptx -eq 'T-ABORTING' -and @($r['UnknownPredicateIds']) -notcontains 'UNK-NESTED-IN-ABORT') { C 'UNKNOWN-RULE' $probe 'UNK-NESTED-IN-ABORT missing' }
    foreach ($n in $need) { if ($opreds -notcontains $n) { C 'PASS-RULE' $probe "missing $n" } }
    $isB12 = $probe -in '10N-S', '10N-M', '10N-SM'
    if (($opreds -contains 'OBS-CANDIDATE-BOUNDARY') -ne $isB12 -or ($fpreds -contains 'FP-BOUNDARY') -ne $isB12) { C 'CANDIDATE-BOUNDARY' $probe 'B12 binding' }
    # support actions and their legal contexts
    $sa = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($x in $setup) { foreach ($y in $Cat[$x]['usesSupport']) { [void]$sa.Add($y) } }
    foreach ($y in $TR['usesSupport']) { [void]$sa.Add($y) }
    foreach ($y in $XC['usesSupport']) { [void]$sa.Add($y) }
    foreach ($y in $sa) { [void]$saUsed.Add($y) }
    if ($sa.Contains('SA-LOCK') -and -not ($exec -eq 'APPCTX-EXEC-01' -or ($drv -eq 'DRIVER-APP-01' -and $trg -in 'TRG-LOCK-CYCLE', 'TRG-LOCK-VETO'))) { C 'SA-LEGAL' $probe 'SA-LOCK outside APPCTX delivery' }
    if ($sa.Contains('SA-UNLOCK') -and -not $sa.Contains('SA-LOCK')) { C 'SA-LEGAL' $probe 'SA-UNLOCK without SA-LOCK' }
    if ($sa.Contains('SA-VETO') -and -not ($trg -eq 'TRG-LOCK-VETO' -and $obs -contains 'RR-DOC')) { C 'SA-LEGAL' $probe 'SA-VETO outside the derived document reactor' }
    if ($exec -eq 'CB-EXEC-01' -and $XC['usesSupport'] -contains 'SA-LOCK') { C 'SA-LEGAL' $probe 'CB-EXEC-01 must not lock' }
    # traceability agreement
    $t = $traceById[$probe]
    if (-not $t) { C 'TRACE' $probe 'missing' } else {
        foreach ($pair in @(@('PrimaryAuthorityId', $prim), @('ScheduleOriginEventId', $orig), @('SchedulerChainId', $chain), @('DriverId', $drv), @('TriggerActionId', $trg), @('ExecutionContextId', $exec), @('Cleanup', $clean), @('CompletionFence', (One $r 'CompletionFence')), @('Verifier', (@($r['VerifierIds']) -join '+')))) {
            if ([string]$t[$pair[0]] -ne [string]$pair[1]) { C 'TRACE' $probe "$($pair[0])" }
        }
        foreach ($res in $t['Resources']) {
            if ((Kind $res['id']) -ne $res['kind']) { C 'TRACE-RESOURCE' $probe "$($res['id']) kind $($res['kind'])" }
            if ($res['kind'] -notin 'PersistentFixtureIdentity', 'DisposableTriggerResource', 'StateCarrier', 'PayloadResource', 'RuntimeModule', 'ObserverRegistration', 'CommandResource') { C 'TRACE-RESOURCE' $probe $res['kind'] }
        }
        foreach ($y in $sa) { if ($t['SupportActions'] -notcontains $y) { C 'TRACE-SUPPORT' $probe $y } }
    }
    # derived execution plan: every element bound
    $plan = @('RUN-ENV-01', 'DRIVER-SCRIPT-01', 'BOOT-01', $drv, 'FIN-GATE-01') + $obs + $setup + $guards + @($trg, $chain) + $seq + @($exec) + @($r['CompletionTokenIds']) + @('FIN-GATE-01', 'CMD-FINISH') + @($r['VerifierIds']) + @($clean) + $steps + @((One $r 'CompletionFence'), 'CONTROL-PLANE-RESULT-01', 'RESULT-RULE-V35', 'EVIDENCE-COMPLETE')
    $planOk = $true
    foreach ($x in $plan) { if (-not $Cat.ContainsKey($x)) { $planOk = $false; Flag 'PLAN' $probe "unbound $x" } }
    if ($planOk) { $plans++ }
}

# ------------------------------------------------------------------ fixture identities and resources
$ids = @($fixture['identities'] | ForEach-Object { $_['id'] })
$catIds = @($Cat.Keys | Where-Object { $Cat[$_]['kind'] -eq 'PersistentFixtureIdentity' })
if ($ids.Count -ne 8 -or $catIds.Count -ne 8 -or (Compare-Object $ids $catIds)) { C 'FIXTURE' 'fixture-v35' "identities $($ids.Count)/$($catIds.Count)" }
foreach ($x in 'F-TRIGGER-APPEND', 'R-SM-LINK', 'R-PAYLOAD-ARX') { if ($ids -contains $x) { C 'FIXTURE' 'fixture-v35' "$x must not be an identity" } }
if ($ids -notcontains 'F-TRIGGER-XR') { C 'FIXTURE' 'fixture-v35' 'F-TRIGGER-XR missing' }
foreach ($x in $trace) { foreach ($res in $x['Resources']) { if ($res['kind'] -eq 'PersistentFixtureIdentity' -and $ids -notcontains $res['id']) { C 'FIXTURE' $x['ProbeId'] $res['id'] } } }
$probe34 = @($trace34 | ForEach-Object { $_['ProbeId'] } | Sort-Object)
$probe35 = @($trace | ForEach-Object { $_['ProbeId'] } | Sort-Object)
if (Compare-Object $probe34 $probe35) { C 'TRACE' 'traceability' 'ProbeId set differs from V34' }

# ------------------------------------------------------------------ NEC / NSC projections
$events = @($Cat.Keys | Where-Object { $Cat[$_]['kind'] -eq 'EventId' })
$reachable = @($events | Where-Object { $Cat[$_]['reachable'] })
function Relations($rowSet, [string]$markerCol, [bool]$v35) {
    $set = [System.Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
    foreach ($probe in $rowSet.Keys) {
        $r = $rowSet[$probe]
        $p = if ($v35) { One $r 'PrimaryAuthorityId' } else { $r['PrimaryAuthorityId'] }
        $o = if ($v35) { One $r 'ScheduleOriginEventId' } else { $r['ScheduleOriginEventId'] }
        if ($events -contains $p) { [void]$set.Add("$p|PRIMARY_FOR|$probe") }
        if ($events -contains $o) { [void]$set.Add("$o|SCHEDULE_ORIGIN_FOR|$probe") }
        $ms = if ($v35) { @($r[$markerCol]) } else { $r[$markerCol].Split(',') | ForEach-Object { "+$_" } }
        foreach ($m in $ms) { $id = $m.Substring(1); if ($events -contains $id) { [void]$set.Add("$id|$(if ($m[0] -eq '+') { 'MARKER_FOR' } else { 'MARKER_ABSENT_FOR' })|$probe") } }
    }
    return , $set
}
function TableTriples([string]$rel) {
    $set = [System.Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
    foreach ($l in (ReadText $rel).Split("`n")) { if ($l -match '^\| `(N-[A-Z-]+)` \| `([A-Z_]+)` \| `([A-Za-z0-9-]+)` \|\s*$') { [void]$set.Add("$($Matches[1])|$($Matches[2])|$($Matches[3])") } }
    return , $set
}
function TablePairs([string]$rel) {
    $set = [System.Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
    foreach ($l in (ReadText $rel).Split("`n")) { if ($l -match '^\| `(NS-[A-Z-]+)` \| `([A-Za-z0-9-]+)` \|\s*$') { [void]$set.Add("$($Matches[1])|$($Matches[2])") } }
    return , $set
}
$rel34 = Relations $rows34 'LifecycleMarkers' $false
$tab34 = TableTriples 'docs/initiatives/I-52-native-event-catalog-v34.md'
if (-not $rel34.SetEquals($tab34)) { C 'NEC-RULE' 'NEC-V34' "rule does not reproduce NEC-V34 ($($rel34.Count) vs $($tab34.Count))" }
$rel35 = Relations $rows 'Markers' $true
$tab35 = TableTriples 'docs/initiatives/I-52-native-event-catalog-v35.md'
if (-not $rel35.SetEquals($tab35)) { C 'NEC-PROJECTION' 'NEC-V35' "table $($tab35.Count) vs derived $($rel35.Count)" }
$primCovered = @($reachable | Where-Object { $e = $_; $rel35 | Where-Object { $_.StartsWith("$e|PRIMARY_FOR|") } })
$pairs = [System.Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
foreach ($probe in $rows.Keys) { $p = One $rows[$probe] 'PrimaryAuthorityId'; if ((Kind $p) -eq 'SchedulerId') { [void]$pairs.Add("$p|$probe") } }
$tabPairs = TablePairs 'docs/initiatives/I-52-native-event-catalog-v35.md'
if (-not $pairs.SetEquals($tabPairs)) { C 'NSC-PAIRS' 'NEC-V35' "table $($tabPairs.Count) vs derived $($pairs.Count)" }
$pairs34 = TablePairs 'docs/initiatives/I-52-native-event-catalog-v34.md'
if (-not $pairs.SetEquals($pairs34)) { C 'NSC-PAIRS' 'NEC-V34' 'scheduler pairs changed' }

# ------------------------------------------------------------------ Architect blocker closure (R3-B01..B12, R3-C02)
$blockerRows = @{}
foreach ($b in $discovery['hardBlockers']) { $blockerRows[$b['id']] = @($b['affectedProbeIds']) }
foreach ($c in $discovery['contestedCandidates']) { $blockerRows[$c['id']] = @($c['affectedProbeIds']) }
$closure = [ordered]@{}
foreach ($bid in $catalog['blockerClosure'].Keys) {
    $spec = $catalog['blockerClosure'][$bid]
    $affected = if ($spec['rows'] -is [string]) { $blockerRows[$bid] } else { @($spec['rows']) }
    $bad = 0
    foreach ($probe in $affected) {
        $r = $rows[$probe]
        if (-not $r) { $bad++; continue }
        $ok = switch ($bid) {
            'R3-B01' { (One $r 'TriggerActionId') -in 'TRG-APPEND-TRIGGER', 'TRG-APPEND-TRIGGER-IN-PRIMARY' -and (@($r['GuardIds']) -contains 'RG-DB-APPEND' -or (One $r 'ScheduleOriginEventId') -eq 'N-DB-APPEND') -and $Cat['F-TRIGGER-APPEND']['kind'] -eq 'DisposableTriggerResource' }
            'R3-B02' { if ($probe -eq '02N') { (One $r 'TriggerActionId') -eq 'TRG-MODIFY-TRIGGER-MOD' -and @($r['GuardIds']) -contains 'RG-DB-OPEN' } else { @($r['ObserverRegistrationIds']) -contains 'OR-TXR' -and (One $r 'TriggerActionId') -eq 'TRG-ASSERT-WRITE-TRIGGER-XR' } }
            'R3-B03' { @($r['SetupActionIds']) -contains 'SET-PAYLOAD-PATH' -and (One $r 'CompletionFence') -eq 'FENCE-PROCESS-EXIT' -and @($r['ObservationPredicateIds']) -contains 'OBS-LOAD' }
            'R3-B04' { $st = @($Cat[(One $r 'CleanupActionId')]['steps']); (($st -contains 'CLN-OBJ-REMOVE') -or ($st -contains 'CLN-OBJ-RETAIN-FENCED')) -and -not (@($r['Markers']) -match 'GOODBYE') }
            'R3-B05' { @(@($r['ObserverRegistrationIds']) | Where-Object { $Cat[$_]['objectReactor'] }).Count -gt 0 }
            'R3-B06' { (One $r 'MutationActionId') -eq 'MUT-SM' -and $Cat['MUT-SM']['writeSet'] -notcontains 'F-REF-B' -and $Cat['MUT-SM']['writeSet'] -contains 'R-SM-LINK' }
            'R3-B07' { (One $r 'ExecutionContextId') -eq 'EXEC-OBSERVE' -and (One $r 'MutationActionId') -eq 'MUT-NONE' }
            'R3-B08' { (One $r 'ExecutionContextId') -eq 'CB-EXEC-01' -and @($r['DocumentLockIds']) -contains 'CB-LOCK-01' -and (One $r 'ExecutionTransactionId') -eq 'T_CB' }
            'R3-B09' { @($r['Markers']) -contains '+MARK-APPCTX-CALLBACK-RETURN' -and @($r['Markers']) -notcontains '+MARK-APPCTX-RETURN' }
            'R3-B10' { @($r['Markers']) -contains '-N-DOC-LOCK-VETO' }
            'R3-B11' { (One $r 'DriverId') -eq 'DRIVER-APP-01' -and @($r['DocumentLockIds']) -contains 'DRIVER-LOCK-01' -and @($r['ObserverRegistrationIds']) -contains 'RR-DOC' }
            'R3-B12' { @($r['ObservationPredicateIds']) -contains 'OBS-CANDIDATE-BOUNDARY' }
            'R3-C02' { @($Cat[(One $r 'CleanupActionId')]['steps'] | Where-Object { $_ -eq 'CLN-BASE' }).Count -eq 1 }
            default { $false }
        }
        if (-not $ok) { $bad++; Flag "BLOCKER/$bid" $probe 'closure predicate false' }
    }
    $closure[$bid] = [ordered]@{ rows = @($affected).Count; open = $bad }
}
$openBlockers = @($closure.Values | Where-Object { $_['open'] -gt 0 }).Count
if (@($catalog['blockerClosure'].Keys).Count -ne 13) { $openBlockers++ }

# ------------------------------------------------------------------ optional exact-header check of referenced ObjectARX members
$api = [ordered]@{ checked = $false; members = @($catalog['apiMembers']).Count; missing = 0 }
if ($SdkRoot) {
    $api['checked'] = $true
    foreach ($m in $catalog['apiMembers']) {
        $h = Join-Path $SdkRoot "inc\$($m['header'])"
        if (-not (Test-Path $h) -or -not ([regex]::IsMatch([IO.File]::ReadAllText($h), $m['pattern']))) { $api['missing']++; Flag 'API-MEMBER' $m['member'] $m['header'] }
    }
}

# ------------------------------------------------------------------ V35-A1: Architect RC-01..RC-13 checks against the frozen oracle
# The oracle (eng/research/I52Ctda/v35-oracle.json) is architecture-review data; nothing below regenerates it from NPM-V35.
function Has($row, [string]$col, [string]$id) { return @($row[$col]) -contains $id }
function Attr($id, [string]$name) { if ($Cat.ContainsKey($id) -and $Cat[$id].Contains($name)) { return $Cat[$id][$name] }; return $null }
$v34text = @{}; $sec = $null
foreach ($l in (ReadText 'docs/initiatives/I-52-native-probe-matrix-v34.md').Split("`n")) {
    $l = $l.TrimEnd("`r")
    if ($l -match '^## (\d+)\.') { $sec = [int]$Matches[1] }
    if ($sec -in 2, 3, 4, 5, 7, 8 -and $l -match '^\| ([A-Z][A-Z0-9_-]+(?:-[A-Z0-9]+)*) \| (.+) \|$' -and $Matches[1] -ne 'ContractId') { $v34text[$Matches[1]] = $Matches[2] }
}
$deliveryTokens = @('TOK-SEND-CMD-END', 'TOK-APPCTX-DELIVERY-DONE', 'TOK-CMDCTX-DELIVERY-DONE', 'TOK-CANCEL-CMDCTX-DONE')
$bindRule = $Cat['MARKER-STAGE-BIND-01']
$usedIds = [System.Collections.Generic.HashSet[string]]::new()
foreach ($probe in $rows.Keys) { foreach ($c in $cols) { foreach ($tok in @($rows[$probe][$c])) { foreach ($x in ($tok -split '[+@,]')) { if ($x) { [void]$usedIds.Add($x.TrimStart('-')) } } } } }
$defText = ($Cat.Keys | Where-Object { $Cat[$_]['kind'] -ne 'RetainedContract' } | ForEach-Object { [string]$Cat[$_]['definition'] }) -join ' '

foreach ($probe in $rows.Keys) {
    $r = $rows[$probe]
    $trg = One $r 'TriggerActionId'; $exec = One $r 'ExecutionContextId'; $chain = One $r 'SchedulerChainId'; $mut = One $r 'MutationActionId'
    $TR = $Cat[$trg]; $setup = @($r['SetupActionIds']); $guards = @($r['GuardIds']); $markers = @($r['Markers']); $tokens = @($r['CompletionTokenIds'])
    $binds = @($r['MarkerStageBindings']); $fps = @($r['FailPredicateIds']); $unks = @($r['UnknownPredicateIds']); $obsp = @($r['ObservationPredicateIds'])
    $prim = One $r 'PrimaryAuthorityId'; $orig = One $r 'ScheduleOriginEventId'
    $body = if ((Kind $prim) -eq 'EventId') { $prim } elseif ((Kind $orig) -eq 'EventId') { $orig } else { $null }
    if ($null -eq $TR -or -not $Cat.ContainsKey($chain) -or -not $Cat.ContainsKey($mut)) { continue }   # already reported as UNBOUND
    $seq = @($Cat[$chain]['sequence'])

    # RC-01 FINISH-TOKEN-SET / FINISH-TIMEOUT / FINISH-DRAIN
    $expTok = @($oracle['G_completionTokens'][$probe])
    if (Compare-Object @($tokens | Sort-Object) @($expTok | Sort-Object)) { C 'FINISH-TOKEN-SET' $probe "tokens [$($tokens -join ',')] vs oracle [$($expTok -join ',')]" }
    foreach ($x in $tokens) { if ((Kind $x) -ne 'CompletionToken') { C 'FINISH-TOKEN-SET' $probe "$x is not a CompletionToken" } }
    if ($unks -notcontains 'UNK-FINISH-TIMEOUT') { C 'FINISH-TIMEOUT' $probe 'UNK-FINISH-TIMEOUT missing' }
    if (@($tokens | Where-Object { $deliveryTokens -contains $_ }).Count -gt 0) {
        $st = @($Cat[(One $r 'CleanupActionId')]['steps'])
        if ($st -notcontains 'CLN-DEFER-DRAIN' -and (One $r 'CompletionFence') -ne 'FENCE-PROCESS-EXIT') { C 'FINISH-DRAIN' $probe 'async delivery without drain step or process fence' }
    }
    # RC-02 / RC-03 MARKER-STAGE-BINDING, DRIVER-MARKER-DISJOINT, ASYNC-BOUNDARY-BINDING, LOCK-RELEASE-BINDING
    $boundMarkers = @($markers | ForEach-Object { $_.Substring(1) } | Where-Object { $_ -like 'MARK-*' -or $_ -like 'N-ED-*' })
    foreach ($m in $boundMarkers) {
        $n = @($binds | Where-Object { $_.Split('@')[0] -eq $m }).Count
        if ($n -ne 1) { C 'MARKER-STAGE-BINDING' $probe "$m has $n bindings" }
    }
    foreach ($b in $binds) {
        $m, $s = $b.Split('@')
        if ($boundMarkers -notcontains $m) { C 'MARKER-STAGE-BINDING' $probe "binding $b for a marker not in the row" }
        if (@($bindRule['markerStages'][$m]) -notcontains $s -or @($oracle['F_markerStages'][$m]) -notcontains $s) { C 'MARKER-STAGE-BINDING' $probe "$b stage not allowed" }
        if ((Attr $s 'namespace') -ne 'GOVERNED' -or $oracle['F_stageNamespace'][$s] -ne 'GOVERNED') { C 'DRIVER-MARKER-DISJOINT' $probe "$b binds a non-governed stage" }
        if ((Attr $m 'namespace') -eq 'DRIVER') { C 'DRIVER-MARKER-DISJOINT' $probe "driver marker $m in a governed row" }
        $req = [string](Attr $s 'requires')
        $ok = switch -Regex ($req) {
            '^always$' { $true }
            '^origin$' { (Kind $orig) -eq 'EventId' }
            '^primaryBody$' { (Kind $prim) -eq 'EventId' }
            '^chain:(.+)$' { $seq -contains $Matches[1] }
            '^syncEntry$' { ($seq -contains 'NX-APPCTX-SYNC') -or $trg -eq 'TRG-CANCEL-CMDCTX' }
            '^trigger:(.+)$' { $trg -eq $Matches[1] }
            '^payload$' { $setup -contains 'SET-PAYLOAD-PATH' }
            '^exec$' { $exec -ne 'EXEC-OBSERVE' }
            default { $false }
        }
        if (-not $ok) { C 'ASYNC-BOUNDARY-BINDING' $probe "$b requires $req" }
    }
    $expBind = @($oracle['F_markerStageBindings'][$probe])
    if (Compare-Object @($binds | Sort-Object) @($expBind | Sort-Object)) { C 'MARKER-STAGE-BINDING' $probe "bindings differ from oracle" }
    foreach ($m in $markers) { if ((Attr $m.Substring(1) 'namespace') -eq 'DRIVER') { C 'DRIVER-MARKER-DISJOINT' $probe "driver marker $m listed" } }
    if ($markers -contains '+MARK-LOCK-RELEASE') {
        foreach ($need in @('OBS-LOCK-RELEASE-BOUND')) { if ($obsp -notcontains $need) { C 'LOCK-RELEASE-BINDING' $probe "$need missing" } }
        if ($unks -notcontains 'UNK-MARKER-BINDING') { C 'LOCK-RELEASE-BINDING' $probe 'UNK-MARKER-BINDING missing' }
        if ($tokens -notcontains 'TOK-LOCK-RELEASE') { C 'LOCK-RELEASE-BINDING' $probe 'TOK-LOCK-RELEASE missing' }
    }
    if ($binds.Count -gt 0 -and $unks -notcontains 'UNK-MARKER-BINDING') { C 'MARKER-STAGE-BINDING' $probe 'UNK-MARKER-BINDING missing' }
    # RC-04 SAFETY-PRECEDENCE (row part)
    if ($fps -notcontains 'FP-CLEANUP-SAFETY') { C 'SAFETY-PRECEDENCE' $probe 'FP-CLEANUP-SAFETY missing' }
    # RC-13 A BODY-NOTIFIER, E TRIGGER-TARGET, C FP-APPLICABILITY, H PROCESS-FENCE
    $expBody = $oracle['A_bodyObserver'][$probe]
    $bo = One $r 'BodyObserverId'
    if ($bo -ne $expBody['BodyObserverId']) { C 'BODY-NOTIFIER' $probe "BodyObserverId $bo vs oracle $($expBody['BodyObserverId'])" }
    if ($expBody['notifier']) {
        if ((Attr $bo 'notifier') -ne $expBody['notifier']) { C 'BODY-NOTIFIER' $probe "notifier $(Attr $bo 'notifier') vs oracle $($expBody['notifier'])" }
        $touch = @($TR['touches'])
        $touched = ($touch -contains $expBody['notifier']) -or ($touch -contains 'STAGED' -and $setup -contains 'SET-STAGE-S-IN-PRIMARY' -and $expBody['notifier'] -eq 'F-XR')
        if (-not $touched) { C 'BODY-NOTIFIER' $probe "$trg does not touch $($expBody['notifier'])" }
    }
    if ((Attr $trg 'targetClass') -ne $oracle['E_triggerTargetClass'][$probe]) { C 'TRIGGER-TARGET' $probe "$trg targetClass $(Attr $trg 'targetClass') vs oracle $($oracle['E_triggerTargetClass'][$probe])" }
    foreach ($x in $fps) { if (@($oracle['C_failPredicateApplicability'][$probe]) -notcontains $x) { C 'FP-APPLICABILITY' $probe "$x not applicable" } }
    if ((One $r 'CompletionFence') -ne $oracle['H_processFence'][$probe]['fence']) { C 'PROCESS-FENCE' $probe "fence vs oracle $($oracle['H_processFence'][$probe]['fence'])" }
    # RC-07 MANAGED-HOST, RC-08 PAYLOAD-DB-BINDING
    $t = $traceById[$probe]
    if (Has $r 'ObserverRegistrationIds' 'RR-MANAGED-CMD') {
        $hostMod = $oracle['managedHost']['RR-MANAGED-CMD']
        if ((Attr 'RR-MANAGED-CMD' 'hostModule') -ne $hostMod) { C 'MANAGED-HOST' $probe 'RR-MANAGED-CMD host module' }
        if (-not ($t['Resources'] | Where-Object { $_["id"] -eq $hostMod -and $_['kind'] -eq 'RuntimeModule' })) { C 'MANAGED-HOST' $probe "$hostMod missing from traceability resources" }
    }
    if (@($oracle['payloadRows']) -contains $probe) {
        if ($obsp -notcontains 'OBS-PAYLOAD-DB-BINDING' -or $unks -notcontains 'UNK-PAYLOAD-DB') { C 'PAYLOAD-DB-BINDING' $probe 'payload DB binding predicates missing' }
    }
    # RC-09 CANCEL-STAGING-RESTORED
    if ($trg -eq 'TRG-CANCEL-OPEN-XR' -and ($obsp -notcontains 'OBS-CANCEL-RESTORED' -or $unks -notcontains 'UNK-CANCEL-STAGING')) { C 'CANCEL-STAGING-RESTORED' $probe 'restore predicates missing' }
    # RC-10 APPEND-TRANSACTION-COMPATIBILITY
    if ($exec -eq 'CB-PRIMARY-01' -and (Attr $trg 'modelSpaceWrite') -and (@(Attr $mut 'writeSet') -contains 'F-REF-C') -and (Attr $trg 'openMode') -ne 'T-PRIMARY') { C 'APPEND-TRANSACTION-COMPATIBILITY' $probe "$trg holds Model Space outside T-PRIMARY" }
    if ($oracle['appendTransaction'].Contains($probe) -and (Attr $trg 'openMode') -ne $oracle['appendTransaction'][$probe]) { C 'APPEND-TRANSACTION-COMPATIBILITY' $probe 'openMode vs oracle' }
    # RC-11 GUARD-KEY-TARGET / GUARD-KEY-STAGE
    foreach ($g in $guards) {
        $ks = Attr $g 'keySchema'
        $keys = if ($g -eq 'RG-ONESHOT') {
            $fam = if ($body -and ($exec -ne 'EXEC-OBSERVE' -or $chain -ne 'CHAIN-NONE')) { Fam $body } else { 'SA-VETO' }
            if (-not $ks.Contains($fam)) { C 'GUARD-KEY-SCHEMA' $probe "RG-ONESHOT has no key for $fam"; @() } else { @($ks[$fam]) }
        } else { @($ks) }
        if ($keys -notcontains 'StageId') { C 'GUARD-KEY-STAGE' $probe "$g key lacks StageId" }
        if (($keys -contains 'TargetObjectId' -or $keys -contains 'TargetInstancePointer') -and -not (($TR['target'] -and $TR['target'] -like 'F-*') -or (Attr $g 'armTarget') -like 'F-*')) { C 'GUARD-KEY-TARGET' $probe "$g needs an object target" }
        if ($keys -contains 'ModulePath' -and $setup -notcontains 'SET-PAYLOAD-PATH') { C 'GUARD-KEY-TARGET' $probe "$g needs NL-ARX-PATH" }
        if ($keys -contains 'RequestId' -and $trg -notin 'TRG-LOCK-CYCLE', 'TRG-LOCK-VETO') { C 'GUARD-KEY-TARGET' $probe "$g needs a lock request" }
        if ($keys -contains 'CommandName' -and -not ($binds | Where-Object { $_ -like 'N-ED-*' }) -and $trg -notin 'TRG-RUN-FIXTURE-CMD', 'TRG-CANCEL-CMDCTX') { C 'GUARD-KEY-TARGET' $probe "$g needs a bound command" }
    }
    # RC-05 CONTROL-PLANE-RESULT (row part)
    if ($t -and $t['ResultAuthority'] -ne 'CONTROL-PLANE-RESULT-01') { C 'CONTROL-PLANE-RESULT' $probe 'traceability ResultAuthority' }
    if ($t -and (Compare-Object @($t['CompletionTokenIds']) @($tokens))) { C 'TRACE' $probe 'CompletionTokenIds' }
}

# global A1 checks
if ((Attr 'DRIVER-SCRIPT-01' 'containsFinish') -ne $false -or ((@(Attr 'DRIVER-SCRIPT-01' 'scriptSteps') -join '|') -match 'FINISH|QUIT')) { C 'FINISH-ORDER' 'DRIVER-SCRIPT-01' 'script must not contain FINISH or QUIT' }
if ((Kind 'FIN-GATE-01') -ne 'FinishGate' -or -not ((Attr 'FIN-GATE-01' 'timeoutSeconds') -gt 0) -or -not ((Attr 'FIN-GATE-01' 'externalDeadlineSeconds') -gt (Attr 'FIN-GATE-01' 'timeoutSeconds'))) { C 'FINISH-TIMEOUT' 'FIN-GATE-01' 'gate/timeout definition' }
if ((Attr 'FIN-GATE-01' 'schedulerUse') -ne 'FINISH-INFRA') { C 'FINISH-ORDER' 'FIN-GATE-01' 'gate must be FINISH-INFRA' }
if ((Attr 'CMD-FINISH' 'neverClassifies') -ne $true) { C 'CONTROL-PLANE-RESULT' 'CMD-FINISH' 'FINISH may not classify' }
if ((Attr 'RESULT-RULE-V35' 'resultAuthority') -ne $oracle['resultAuthority']['authority'] -or (Kind 'CONTROL-PLANE-RESULT-01') -ne 'ResultAuthority') { C 'CONTROL-PLANE-RESULT' 'RESULT-RULE-V35' 'result authority' }
if ((@(Attr 'RESULT-RULE-V35' 'evaluationOrder') -join '>') -ne (@($oracle['resultAuthority']['evaluationOrder']) -join '>')) { C 'SAFETY-PRECEDENCE' 'RESULT-RULE-V35' 'evaluation order' }
foreach ($x in 'EVIDENCE-COMPLETE', 'SAFETY-EVIDENCE-COMPLETE', 'FENCED-RETENTION-SAFE', 'PAYLOAD-DB-BINDING') { if ((Kind $x) -ne 'ResultPredicate') { C 'SAFETY-PRECEDENCE' $x 'result predicate missing' } }
foreach ($d in 'DRIVER-CMD-01', 'DRIVER-APP-01') {
    $po = @(Attr $d 'phaseOrder'); $need = @($oracle['driverPhaseOrder'][$d]); $last = -1
    foreach ($p in $need) { $i = [Array]::IndexOf($po, $p); if ($i -le $last) { C 'GUARD-DISARM-AFTER-OBLIGATIONS' $d "phase $p out of order" }; $last = $i }
}
foreach ($g in $oracle['guardKeySchema'].Keys) { if ((@(Attr $g 'keySchema') -join ',') -ne (@($oracle['guardKeySchema'][$g]) -join ',')) { C 'GUARD-KEY-SCHEMA' $g 'key schema vs oracle' } }
foreach ($fam in $oracle['guardKeyOneShot'].Keys) {
    $ks = Attr 'RG-ONESHOT' 'keySchema'
    if (-not $ks -or -not $ks.Contains($fam)) { C 'GUARD-KEY-SCHEMA' 'RG-ONESHOT' "no key for $fam"; continue }
    foreach ($k in @($oracle['guardKeyOneShot'][$fam]) + 'StageId') { if (@($ks[$fam]) -notcontains $k) { C 'GUARD-KEY-SCHEMA' 'RG-ONESHOT' "$fam key lacks $k" } }
}
foreach ($id in $oracle['B_resourceTypes'].Keys) { if ((Kind $id) -ne $oracle['B_resourceTypes'][$id]) { C 'RESOURCE-TYPE' $id "kind $(Kind $id) vs oracle $($oracle['B_resourceTypes'][$id])" } }
foreach ($x in $trace) { foreach ($res in $x['Resources']) { if ($oracle['B_resourceTypes'].Contains($res['id']) -and $res['kind'] -ne $oracle['B_resourceTypes'][$res['id']]) { C 'RESOURCE-TYPE' $x['ProbeId'] "$($res['id']) traced as $($res['kind'])" } } }
foreach ($m in 'R-NATIVE-ARX', 'R-MANAGED-OBSERVER', 'R-PAYLOAD-ARX') {
    foreach ($a in 'sourcePath', 'artifact', 'toolchain', 'identity') { if (-not (Attr $m $a)) { C 'MANAGED-HOST' $m "$a missing" } }
    if ((Attr $m 'researchOnly') -ne $true) { C 'MANAGED-HOST' $m 'must be research-only' }
}
foreach ($a in 'loadMechanism', 'unload') { if (-not (Attr 'R-MANAGED-OBSERVER' $a)) { C 'MANAGED-HOST' 'R-MANAGED-OBSERVER' "$a missing" } }
if (-not ((@(Attr 'DRIVER-SCRIPT-01' 'scriptSteps') -join '|') -match 'NETLOAD')) { C 'MANAGED-HOST' 'DRIVER-SCRIPT-01' 'no NETLOAD step for R-MANAGED-OBSERVER' }
if ((Attr 'R-PAYLOAD-ARX' 'dbBinding') -ne 'PAYLOAD-DB-BINDING' -or -not (Attr 'R-PAYLOAD-ARX' 'interface')) { C 'PAYLOAD-DB-BINDING' 'R-PAYLOAD-ARX' 'db binding / interface' }
if ((Attr 'LOG-SEQ-01' 'owner') -ne 'R-NATIVE-ARX') { C 'LOG-SEQUENCE' 'LOG-SEQ-01' 'sequencer owner' }
# V35-A2 (Architect MINOR M1): FINISH-FENCE-01 is read through one LOG-SEQ-01 export; its setter is internal and never exported
$ff = $oracle['finishFenceAbi']; $readApi = [string]$ff['readApi']; $logExports = @(Attr 'LOG-SEQ-01' 'exports')
if ($logExports -notcontains $readApi -or (Attr 'LOG-SEQ-01' 'fenceRead') -ne $readApi -or -not ([string](Attr 'LOG-SEQ-01' 'definition')).Contains($readApi)) { C 'FINISH-FENCE-READ-ABI' 'LOG-SEQ-01' "read export $readApi missing" }
if ((Attr 'LOG-SEQ-01' 'fenceSetterExported') -ne $ff['setterExported'] -or @($logExports | Where-Object { $_ -match 'FinishFence' -and $_ -ne $readApi }).Count) { C 'FINISH-FENCE-READ-ABI' 'LOG-SEQ-01' 'the fence setter must not be exported' }
if ((Attr $ff['fence'] 'readApi') -ne $readApi -or (Attr $ff['fence'] 'setter') -ne $ff['setter'] -or -not ([string](Attr $ff['fence'] 'definition')).Contains($readApi)) { C 'FINISH-FENCE-READ-ABI' $ff['fence'] 'the fence must name its read export and its internal setter' }
foreach ($m in @($ff['readers'])) {
    $imp = @(Attr $m 'imports')
    if ($imp -notcontains $readApi -or -not ([string](Attr $m 'interface')).Contains($readApi)) { C 'FINISH-FENCE-READ-ABI' $m "does not import $readApi" }
    foreach ($x in $imp) { if ($logExports -notcontains $x) { C 'FINISH-FENCE-READ-ABI' $m "imports $x, which LOG-SEQ-01 does not export" } }
}
foreach ($id in $Cat.Keys) { if (($Cat[$id] | ConvertTo-Json -Depth 20 -Compress) -match 'I52Ctda_FinishFenceSet') { C 'FINISH-FENCE-READ-ABI' $id 'names a fence setter as ABI' } }
foreach ($f in 'Sequence', 'ProbeId', 'StageId', 'DeliveryId', 'ModuleId', 'PID', 'TID', 'DocumentId', 'DatabaseId', 'CommandIdentity') { if (@(Attr 'LOG-RECORD-01' 'fields') -notcontains $f) { C 'LOG-SEQUENCE' 'LOG-RECORD-01' "field $f missing" } }
$cs = $oracle['cancelStaging']['TRG-CANCEL-OPEN-XR']; $staged = [string](Attr 'TRG-CANCEL-OPEN-XR' 'stagedBytes')
if ($staged -ne $cs['stagedBytes']) { C 'CANCEL-STAGING-DISTINCT' 'TRG-CANCEL-OPEN-XR' "staged $staged vs oracle" }
if (@($cs['forbiddenValues']) -contains $staged -or $staged -like 'HFV30:*') { C 'CANCEL-STAGING-DISTINCT' 'TRG-CANCEL-OPEN-XR' 'staged value equals a canonical state value' }
foreach ($probe in $rows.Keys) {
    foreach ($col in 'ExpectedBefore', 'ExpectedAfter') { foreach ($part in (One $rows[$probe] $col).Split('+')) { if ($staged -and ([string]$Cat[$part]['definition']).Contains($staged)) { C 'CANCEL-STAGING-NOT-EXPECTED' $probe "$part contains the staged value" } } }
}
# pre-publication review (lifecycle lens): lock-release anchors, FINISH fence, cleanup close, payload removal, sync stage, safety scope
foreach ($probe in $rows.Keys) {
    foreach ($b in @($rows[$probe]['MarkerStageBindings'])) {
        if ($b -like 'MARK-LOCK-RELEASE@*') {
            $s = $b.Split('@')[1]; $anch = Attr 'LOCK-RELEASE-BIND-01' 'anchors'
            if (-not $anch -or $anch[$s] -ne $oracle['lockReleaseAnchor'][$s]) { C 'LOCK-RELEASE-BINDING' $probe "anchor for $s vs oracle $($oracle['lockReleaseAnchor'][$s])" }
        }
    }
    if (@($rows[$probe]['UnknownPredicateIds']) -notcontains $oracle['finishGate']['lateDeliveryResult']) { C 'FINISH-DRAIN' $probe 'UNK-LATE-DELIVERY missing' }
}
if ((Attr 'CMD-FINISH' 'firstAction') -ne $oracle['finishGate']['finishFirstAction'] -or (Kind 'FINISH-FENCE-01') -ne 'FinishGate') { C 'FINISH-DRAIN' 'CMD-FINISH' 'FINISH-FENCE-01 must be the first action' }
if (-not ((Attr 'FIN-GATE-01' 'postFinishDeadlineSeconds') -gt 0)) { C 'FINISH-TIMEOUT' 'FIN-GATE-01' 'post-FINISH deadline missing' }
if ((Attr 'CLN-BASE' 'closesDocument') -ne $oracle['finishGate']['cleanupClosesDocument'] -or (Attr 'CMD-FINISH' 'exitAction') -ne 'QUIT-DISCARD') { C 'FINISH-ORDER' 'CLN-BASE' 'the scratch document must be closed only by QUIT-DISCARD after the flush' }
foreach ($x in @($oracle['payloadModule']['exports'])) { if (@(Attr 'R-PAYLOAD-ARX' 'exports') -notcontains $x) { C 'PAYLOAD-DB-BINDING' 'R-PAYLOAD-ARX' "export $x missing" } }
if ((Attr 'R-PAYLOAD-ARX' 'registersOnlyWhenArmed') -ne $oracle['payloadModule']['registersOnlyWhenArmed']) { C 'PAYLOAD-DB-BINDING' 'R-PAYLOAD-ARX' 'must register only when armed' }
if ((Attr 'STG-SYNC-APPCTX' 'includesCallerReturn') -ne $oracle['syncStageIncludesCallerReturn']) { C 'MARKER-STAGE-BINDING' 'STG-SYNC-APPCTX' 'caller return must belong to the sync stage' }
if ((Attr 'FP-CLEANUP-SAFETY' 'scope') -ne $oracle['cleanupSafetyScope']) { C 'SAFETY-PRECEDENCE' 'FP-CLEANUP-SAFETY' 'scope vs oracle' }

# pre-publication review (independence lens): pinned inputs, exact per-row sets, canonical approvals, two-sided traceability
function GitBlobId([string]$rel) {
    $bytes = [IO.File]::ReadAllBytes((P $rel))
    $text = [Text.Encoding]::UTF8.GetString($bytes).Replace("`r`n", "`n")
    $body = [Text.Encoding]::UTF8.GetBytes($text)
    $head = [Text.Encoding]::ASCII.GetBytes("blob $($body.Length)`0")
    return [Convert]::ToHexString([Security.Cryptography.SHA1]::HashData([byte[]]($head + $body))).ToLowerInvariant()
}
function Canon($v) {
    if ($null -eq $v) { return 'null' }
    if ($v -is [System.Collections.IDictionary]) {
        $keys = [string[]]@($v.Keys); [Array]::Sort($keys, [StringComparer]::Ordinal)
        return '{' + (($keys | ForEach-Object { (Canon $_) + ':' + (Canon $v[$_]) }) -join ',') + '}'
    }
    if ($v -is [bool]) { if ($v) { return 'true' } else { return 'false' } }
    if ($v -is [int] -or $v -is [long]) { return [string]$v }
    if ($v -is [string]) {
        $sb = [Text.StringBuilder]::new('"')
        foreach ($ch in $v.ToCharArray()) {
            $c = [int]$ch
            if ($ch -eq '"' -or $ch -eq '\') { [void]$sb.Append('\').Append($ch) }
            elseif ($c -ge 0x20 -and $c -le 0x7E) { [void]$sb.Append($ch) }
            else { [void]$sb.Append('\u').Append($c.ToString('X4')) }
        }
        return $sb.Append('"').ToString()
    }
    if ($v -is [System.Collections.IEnumerable]) { return '[' + ((@($v) | ForEach-Object { Canon $_ }) -join ',') + ']' }
    return [string]$v
}
# Pinned inputs: the V34 baseline blobs at 0c609269 and the architecture-review oracle approved in the V35-A1 delta review, re-pinned by the V35-A2 errata (M1 only).
$pins = [ordered]@{
    'docs/initiatives/I-52-native-probe-matrix-v34.md'                  = '7a29a1c11d308879488e43dc460602df1426697b'
    'docs/initiatives/I-52-native-event-catalog-v34.md'                 = '1f16dd0641c1706e6874a68b9136406c19e33848'
    'eng/research/I52Ctda/traceability-v34.json'                        = '0424c0b6788cbbacf5deaf533317f9202f305482'
    'docs/automation/evidence/I-52-r3-governed-executor-discovery.json' = '38104836975dc4e0adc5c930e8550fc0abab5ae0'
    'eng/research/I52Ctda/v35-oracle.json'                              = '0171e5de9e79ce99cf70d97777346f4aacd9da05'
}
foreach ($k in $pins.Keys) { if ((GitBlobId $k) -ne $pins[$k]) { C 'INPUT-PIN' $k "git blob $(GitBlobId $k) is not the pinned $($pins[$k])" } }
foreach ($k in $oracle['V34_inputBlobPins'].Keys) { if ($pins[$k] -ne $oracle['V34_inputBlobPins'][$k]) { C 'INPUT-PIN' $k 'oracle pin differs from the validator pin' } }

foreach ($probe in $rows.Keys) {
    $r = $rows[$probe]; $exp = $oracle['P_rowPredicatesAndTrigger'][$probe]
    if ((One $r 'TriggerActionId') -ne $exp['TriggerActionId']) { C 'TRIGGER-ACTION' $probe "$(One $r 'TriggerActionId') vs oracle $($exp['TriggerActionId'])" }
    foreach ($c in 'ObservationPredicateIds', 'FailPredicateIds', 'UnknownPredicateIds') {
        if (Compare-Object @(@($r[$c]) | Sort-Object) @(@($exp[$c]) | Sort-Object)) { C 'PREDICATE-SET' $probe "$c differs from oracle" }
    }
    # two-sided traceability
    $t = $traceById[$probe]; if (-not $t) { continue }
    $trgE = $Cat[(One $r 'TriggerActionId')]; $mutE = $Cat[(One $r 'MutationActionId')]; $setup = @($r['SetupActionIds']); $obs = @($r['ObserverRegistrationIds'])
    if ($null -eq $trgE -or $null -eq $mutE) { continue }
    $expRes = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($x in $mutE['writeSet']) { [void]$expRes.Add($x) }
    if ($trgE['target']) { [void]$expRes.Add($trgE['target']) }
    foreach ($ob in $obs) { [void]$expRes.Add($ob); $n = [string]$Cat[$ob]['notifier']; if ($Cat.ContainsKey($n)) { [void]$expRes.Add($n) } }
    $surf = One $r 'Surface'
    if ($surf -in 'SM', 'S+M+SM') { [void]$expRes.Add('R-SM-LINK'); [void]$expRes.Add('F-REF-A') }
    if ((One $r 'ExpectedBefore').StartsWith('STATE-ALL') -or $surf -eq 'S+M+SM') { foreach ($x in 'F-XR', 'F-REF-B', 'F-REF-A', 'F-REF-C', 'R-SM-LINK') { [void]$expRes.Add($x) } }
    if ($setup -contains 'SET-PAYLOAD-PATH') { [void]$expRes.Add('R-PAYLOAD-ARX') }
    foreach ($x in 'CMD-BOOT', 'CMD-PROBE', 'CMD-FINISH', 'R-NATIVE-ARX') { [void]$expRes.Add($x) }
    if ((One $r 'TriggerActionId') -eq 'TRG-RUN-FIXTURE-CMD') { [void]$expRes.Add('CMD-FIXTURE') }
    if ((One $r 'TriggerActionId') -eq 'TRG-CANCEL-CMDCTX') { [void]$expRes.Add('CMD-CANCEL') }
    if ((One $r 'ExecutionContextId') -eq 'SEND-EXEC-01') { [void]$expRes.Add('CMD-QUEUED') }
    if ($obs -contains 'RR-MANAGED-CMD') { [void]$expRes.Add('R-MANAGED-OBSERVER') }
    $gotRes = @($t['Resources'] | ForEach-Object { $_['id'] })
    if (Compare-Object @($expRes | Sort-Object) @($gotRes | Sort-Object)) { C 'TRACE-RESOURCE-SET' $probe 'traceability resources differ from the catalog-derived set' }
    $sa = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($x in $setup) { foreach ($y in $Cat[$x]['usesSupport']) { [void]$sa.Add($y) } }
    foreach ($y in $trgE['usesSupport']) { [void]$sa.Add($y) }
    foreach ($y in $Cat[(One $r 'ExecutionContextId')]['usesSupport']) { [void]$sa.Add($y) }
    if ($sa.Contains('SA-TX-START') -or (One $r 'PrimaryTransactionId') -ne 'NONE-OPENED') { [void]$sa.Add('SA-TX-ABORT') }
    if (Compare-Object @($sa | Sort-Object) @(@($t['SupportActions']) | Sort-Object)) { C 'TRACE-SUPPORT-SET' $probe 'traceability SupportActions differ' }
    if ($sa.Contains('SA-LOCK') -and -not $sa.Contains('SA-UNLOCK')) { C 'SA-LEGAL' $probe 'SA-LOCK without SA-UNLOCK' }
    if (Compare-Object @(@($t['MarkerStageBindings']) | Sort-Object) @(@($r['MarkerStageBindings']) | Sort-Object)) { C 'TRACE' $probe 'MarkerStageBindings' }
}
# RC-06 hardening: extended entries keep the V34 text verbatim and are approved by a canonical hash of the whole entry
foreach ($id in $oracle['D_retainedAuthority'].Keys) {
    if ($oracle['D_retainedAuthority'][$id]['status'] -ne 'RETAINED-EXTENDED' -or -not $Cat.ContainsKey($id)) { continue }
    $e = $Cat[$id]
    if ([string](Attr $id 'v34Definition') -ne $v34text[$id]) { C 'RETAINED-EXTENSION-COMPATIBILITY' $id 'v34Definition differs from NPM-V34' }
    if (-not ([string]$e['definition']).Contains($v34text[$id])) { C 'RETAINED-EXTENSION-COMPATIBILITY' $id 'definition does not contain the V34 text verbatim' }
}

# verification round: approval freeze, full phase order, exact row sets, lineage completeness, traceability identity
$freeze = $oracle['Z_approvalFreeze']
foreach ($id in $Cat.Keys) {
    if (-not $freeze['catalogEntries'].Contains($id)) { C 'CATALOG-APPROVAL' $id 'entry not in the approved catalog'; continue }
    $h = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::ASCII.GetBytes((Canon $Cat[$id]))))
    if ($h -ne $freeze['catalogEntries'][$id]) { C 'CATALOG-APPROVAL' $id 'entry differs from the approved text' }
}
foreach ($id in $freeze['catalogEntries'].Keys) { if (-not $Cat.ContainsKey($id)) { C 'CATALOG-APPROVAL' $id 'approved entry missing' } }
foreach ($probe in $rawRows.Keys) {
    $h = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::ASCII.GetBytes((Canon $rawRows[$probe]))))
    if ($h -ne $freeze['rows'][$probe]) { C 'ROW-APPROVAL' $probe 'row differs from the approved row' }
}
foreach ($k in $freeze['blobs'].Keys) { if ((GitBlobId $k) -ne $freeze['blobs'][$k]) { C 'INPUT-PIN' $k 'differs from the approved blob' } }
$oracleSources = [ordered]@{ 'eng/research/I52Ctda/oracle/make_oracle.py' = 'e90aa1dc921d0778e6be36a1b1183abe83a289f2'; 'eng/research/I52Ctda/oracle/oracle_predicates.py' = '78c750d863240b99bcc6621ef9289320d725cc97' }
foreach ($k in $oracleSources.Keys) { if ((GitBlobId $k) -ne $oracleSources[$k]) { C 'INPUT-PIN' $k 'oracle source differs from the pinned source' } }
foreach ($d in 'DRIVER-CMD-01', 'DRIVER-APP-01') {
    if ((@(Attr $d 'phaseOrder') -join '>') -ne (@($oracle['driverPhaseOrder'][$d]) -join '>')) { C 'GUARD-DISARM-AFTER-OBLIGATIONS' $d 'phase order differs from the oracle sequence' }
}
if ((@(Attr 'DRIVER-CMD-01' 'setupOrder') -join '>') -ne (@($oracle['setupOrder']) -join '>')) { C 'SETUP-ORDER' 'DRIVER-CMD-01' 'setup order differs from the oracle' }
if ((Attr 'FINISH-FENCE-01' 'lateDeliverySafety') -ne 'COUNTED-WHEN-REMOVAL-OR-FENCE-RECORDED' -or (Attr 'FP-CLEANUP-SAFETY' 'lateDeliveryRule') -ne 'COUNTED-WHEN-REMOVAL-OR-FENCE-RECORDED' -or (Attr 'FP-CLEANUP-SAFETY' 'windowStart') -ne 'CleanupActionId') { C 'SAFETY-PRECEDENCE' 'FP-CLEANUP-SAFETY' 'late deliveries after removal/fence must count; window starts at CleanupActionId' }
if ((Attr 'TOK-LOCK-RELEASE' 'stage') -ne 'BOUND:MARK-LOCK-RELEASE') { C 'LOCK-RELEASE-BINDING' 'TOK-LOCK-RELEASE' 'token must follow the bound stage anchor' }
if ((Attr 'CLN-PROCESS-EXIT' 'executor') -ne 'CONTROL-PLANE') { C 'CONTROL-PLANE-RESULT' 'CLN-PROCESS-EXIT' 'process exit is verified by the control plane' }
$so = @($oracle['setupOrder'])
foreach ($probe in $rows.Keys) {
    $r = $rows[$probe]; $v34 = $rows34[$probe]
    # Cleanup is a retained V34 column except the one authorized extension
    $expClean = if ($oracle['allowedCleanupExtension'].Contains($probe)) { $oracle['allowedCleanupExtension'][$probe] } else { $v34['Cleanup'] }
    if ((One $r 'CleanupActionId') -ne $expClean) { C 'RETAINED-DRIFT' $probe "Cleanup $(One $r 'CleanupActionId') vs $expClean" }
    # setup order
    $idx = @(@($r['SetupActionIds']) | ForEach-Object { [Array]::IndexOf($so, $_) })
    for ($i = 1; $i -lt $idx.Count; $i++) { if ($idx[$i] -le $idx[$i - 1]) { C 'SETUP-ORDER' $probe 'SetupActionIds out of the canonical order'; break } }
    # exact lock and guard sets
    $exec = One $r 'ExecutionContextId'; $trg = One $r 'TriggerActionId'
    if ($Cat.ContainsKey($exec)) {
        $expLocks = @('LOCK-OBS') + @($Cat[$exec]['locks']) + @(if ($trg -in 'TRG-LOCK-CYCLE', 'TRG-LOCK-VETO') { 'DRIVER-LOCK-01' })
        if (Compare-Object @($expLocks | Sort-Object) @(@($r['DocumentLockIds']) | Sort-Object)) { C 'EXEC-LOCK' $probe 'DocumentLockIds differ from the execution-context lock set' }
    }
    $prim = One $r 'PrimaryAuthorityId'; $orig = One $r 'ScheduleOriginEventId'
    $body = if ((Kind $prim) -eq 'EventId') { $prim } elseif ((Kind $orig) -eq 'EventId') { $orig } else { $null }
    $expG = [System.Collections.Generic.List[string]]::new()
    if ($body -and ($exec -ne 'EXEC-OBSERVE' -or (One $r 'SchedulerChainId') -ne 'CHAIN-NONE')) {
        $expG.Add($(if ($guardByEvent.ContainsKey($body)) { $guardByEvent[$body] } elseif ((Fam $body) -eq 'N-TR-') { 'RG-TX' } elseif ($body -eq 'N-OBJ-ERASE' -and $Cat[$trg]['target'] -eq 'F-TRIGGER-ERASE-OBJ') { 'RG-OBJ-ERASE' } else { 'RG-ONESHOT' }))
    }
    if (@($r['SetupActionIds']) -contains 'SET-ARM-VETO' -and -not $expG.Contains('RG-ONESHOT')) { $expG.Add('RG-ONESHOT') }
    if (Compare-Object @($expG | Sort-Object) @(@($r['GuardIds']) | Sort-Object)) { C 'GUARD-FAMILY' $probe 'GuardIds differ from the exact guard set' }
    # payload reactor only on the armed C15 row
    if ((@($r['ObserverRegistrationIds']) -contains 'RR-PAYLOAD-DB') -ne (@($oracle['payloadReactorRows']) -contains $probe)) { C 'PAYLOAD-DB-BINDING' $probe 'RR-PAYLOAD-DB is registered only on the armed C15 row' }
    # lineage completeness
    foreach ($rec in @($lin[$probe])) { if (@($rec['v35']).Count -eq 0) { C 'LINEAGE-EMPTY' $probe "$($rec['column']): $($rec['clause'])" } }
    # traceability identity
    $t = $traceById[$probe]
    if ($t) {
        $expEv = if ((Kind $prim) -eq 'EventId') { $prim } elseif ((Kind $orig) -eq 'EventId') { $orig } else { $null }
        $expSched = if ((Kind $prim) -eq 'SchedulerId') { $prim } else { $null }
        if ([string]$t['EventId'] -ne [string]$expEv -or [string]$t['SchedulerId'] -ne [string]$expSched -or $t['HandlerId'] -ne ('Probe_' + $probe.Replace('-', '_')) -or $t['Status'] -ne 'ContractCandidateV35-A1') { C 'TRACE' $probe 'EventId/SchedulerId/HandlerId/Status' }
    }
}

# RC-06 retained authority (independent table + V34 text parsed here)
foreach ($id in $oracle['D_retainedAuthority'].Keys) {
    $exp = $oracle['D_retainedAuthority'][$id]
    if (-not $Cat.ContainsKey($id)) { C 'RETAINED-AUTHORITY-STATUS' $id 'missing from catalog'; continue }
    if ((Attr $id 'retainedStatus') -ne $exp['status']) { C 'RETAINED-AUTHORITY-STATUS' $id "status $(Attr $id 'retainedStatus') vs oracle $($exp['status'])" }
    switch ($exp['status']) {
        'RETAINED-UNCHANGED' { if ([string]$Cat[$id]['definition'] -ne $v34text[$id]) { C 'RETAINED-V34-DRIFT' $id 'definition differs from NPM-V34' } }
        'RETAINED-EXTENDED' {
            $h = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::ASCII.GetBytes((Canon $Cat[$id]))))
            if ($h -ne $exp['approvedSha256']) { C 'RETAINED-EXTENSION-COMPATIBILITY' $id 'extension text differs from the approved V35-A1 text' }
        }
        'SUPERSEDED' { if ($usedIds.Contains($id)) { C 'RETAINED-AUTHORITY-STATUS' $id 'superseded id used by a row' } }
        'UNUSED' { if ($usedIds.Contains($id)) { C 'RETAINED-AUTHORITY-STATUS' $id 'unused id used by a row' } }
    }
    if ($exp['status'] -in 'RETAINED-UNCHANGED', 'RETAINED-EXTENDED' -and -not $usedIds.Contains($id) -and $defText -notmatch "(?<![A-Z0-9-])$([regex]::Escape($id))(?![A-Z0-9-])") { C 'RETAINED-AUTHORITY-STATUS' $id 'active retained id is never used' }
}
foreach ($id in $Cat.Keys) { if ($Cat[$id]['kind'] -eq 'RetainedContract' -and -not $oracle['D_retainedAuthority'].Contains($id)) { C 'RETAINED-AUTHORITY-STATUS' $id 'retained id not in the oracle table' } }

# ------------------------------------------------------------------ counts and predicates
$contradictions = 0; foreach ($v in $contra.Values) { $contradictions += $v }
$contradictions += $retainedDrift + $catalogRefErrors
$saCount = @($Cat.Keys | Where-Object { $Cat[$_]['kind'] -eq 'SupportAction' }).Count
$schedCount = @($Cat.Keys | Where-Object { $Cat[$_]['kind'] -eq 'SchedulerId' }).Count
if ($saUsed.Count -ne $saCount) { $contradictions++; Flag 'SA-COVERAGE' 'all' "used $($saUsed.Count) of $saCount" }
$counts = [ordered]@{
    inputRowsV34 = $rows34.Count; inputRowsV35 = $inputLines.Count; parsed = $rows.Count; derivedPlans = $plans
    unboundIds = $unbound; executableFreeText = $freeText; classD = $classD; lineageMissing = $lineageMissing
    contradictions = $contradictions; identifiersChecked = $idCount
    eventIds = $events.Count; reachableCallbacks = $reachable.Count; primaryCoverage = "$($primCovered.Count)/$($reachable.Count)"
    eventRelations = $rel35.Count; eventRelationsRuleOnV34 = "$($rel34.Count)/$($tab34.Count)"; schedulers = $schedCount; schedulerPairs = $pairs.Count
    supportActions = $saCount; supportActionsUsed = $saUsed.Count; fixtureIdentities = $ids.Count; openBlockerGroups = $openBlockers
}
$frozen = $catalog['frozenCounts']
$countsOk = ($rows.Count -eq $frozen['probeIds']) -and ($events.Count -eq $frozen['eventIds']) -and ($reachable.Count -eq $frozen['reachableCallbacks']) -and
    ($schedCount -eq $frozen['schedulers']) -and ($saCount -eq $frozen['supportActions']) -and ($ids.Count -eq $frozen['fixtureIdentities'])
$zero = ($unbound -eq 0) -and ($freeText -eq 0) -and ($classD -eq 0) -and ($lineageMissing -eq 0)
$pred = [ordered]@{
    ExecutionCatalogClosed = ($unbound -eq 0) -and ($catalogRefErrors -eq 0) -and ($freeText -eq 0) -and -not ($contra.Keys | Where-Object { $_ -like 'RETAINED-*' -or $_ -in 'FINISH-ORDER', 'LOG-SEQUENCE', 'CONTROL-PLANE-RESULT', 'INPUT-PIN', 'CATALOG-APPROVAL', 'ROW-APPROVAL' })
    TriggerAuthorityClosed = ($unbound -eq 0) -and -not ($contra.Keys | Where-Object { $_ -like 'CANCEL-STAGING-*' -or $_ -in 'DRIVER-TRIGGER', 'TRIGGER-PRODUCES', 'TRIGGER-DISJOINT', 'ERASE-TRIGGER-DISJOINT', 'MARKER-PRODUCER', 'GUARD-TARGET', 'TRIGGER-TARGET', 'BODY-NOTIFIER', 'APPEND-TRANSACTION-COMPATIBILITY', 'TRIGGER-ACTION' })
    SetupAuthorityClosed = $zero -and -not ($contra.Keys | Where-Object { $_ -like 'GUARD-KEY-*' -or $_ -in 'PRIMARY-T', 'OUTCOME-COMMIT', 'NESTED-T', 'STAGE-T', 'CHAIN-DATA', 'PAYLOAD-PATH', 'GUARD-DISARM-AFTER-OBLIGATIONS', 'SETUP-ORDER', 'EXEC-LOCK', 'GUARD-FAMILY', 'LINEAGE-EMPTY' })
    PredicateAuthorityClosed = $zero -and -not ($contra.Keys | Where-Object { $_ -in 'PASS-RULE', 'UNKNOWN-RULE', 'CANDIDATE-BOUNDARY', 'VETO-FAIL', 'RESULT-CLASS', 'EXPECTED-DETERMINISM', 'MARKER-STAGE-BINDING', 'DRIVER-MARKER-DISJOINT', 'ASYNC-BOUNDARY-BINDING', 'LOCK-RELEASE-BINDING', 'SAFETY-PRECEDENCE', 'FP-APPLICABILITY', 'FINISH-TIMEOUT', 'CONTROL-PLANE-RESULT', 'PREDICATE-SET' })
    ResourceAuthorityClosed = $zero -and -not ($contra.Keys | Where-Object { $_ -like 'TRACE*' -or $_ -in 'FIXTURE', 'BODY-OBSERVER', 'MARKER-OBSERVER', 'RESOURCE-TYPE', 'MANAGED-HOST', 'PAYLOAD-DB-BINDING', 'LOG-SEQUENCE', 'FINISH-FENCE-READ-ABI' })
    FixtureExecutionContractClosedV35 = $zero -and ($contradictions -eq 0) -and ($openBlockers -eq 0) -and $countsOk
    PlanDerivationClosed = ($plans -eq $rows.Count) -and ($rows.Count -eq 100) -and $zero -and -not ($contra.Keys | Where-Object { $_ -in 'FINISH-TOKEN-SET', 'FINISH-ORDER', 'FINISH-DRAIN', 'PROCESS-FENCE' })
}
$result = if (@($pred.Values | Where-Object { -not $_ }).Count -eq 0 -and $findings.Count -eq 0) { 'PASS' } else { 'FAIL' }

$report = [ordered]@{
    schemaVersion = 2; revision = 'V35-A2'; validator = 'eng/research/I52Ctda/validate-v35-catalog.ps1'; oracle = 'eng/research/I52Ctda/v35-oracle.json'; scope = 'documents/catalog only; no runtime semantics'
    result = $result; counts = $counts; frozenCountsHold = $countsOk; contradictionsByCheck = $contra; blockerClosure = $closure
    apiHeaderCheck = $api; predicates = $pred; findings = $findings
}
$json = $report | ConvertTo-Json -Depth 8
if ($EvidencePath) { [IO.File]::WriteAllText((P $EvidencePath), $json + "`n") }

"INPUT ROWS = $($rows34.Count)"
"PARSED = $($rows.Count)"
"DERIVED PLANS = $plans"
"UNBOUND IDS = $unbound"
"EXECUTABLE FREE-TEXT = $freeText"
"CLASS-D = $classD"
"CONTRADICTIONS = $contradictions"
"LINEAGE MISSING = $lineageMissing"
"EVENT RELATIONS = $($rel35.Count) / SCHEDULER PAIRS = $($pairs.Count) / PRIMARY COVERAGE = $($primCovered.Count)/$($reachable.Count)"
"CALLBACKS = $($reachable.Count) / SCHEDULERS = $schedCount / SUPPORT ACTIONS = $saCount / FIXTURE IDENTITIES = $($ids.Count)"
"OPEN BLOCKER GROUPS = $openBlockers"
"API MEMBERS = $($api['members']) checked=$($api['checked']) missing=$($api['missing'])"
foreach ($k in $pred.Keys) { "$k = $($pred[$k].ToString().ToUpperInvariant())" }
if ($findings.Count) { $findings | Select-Object -First 40 | ForEach-Object { "  [$($_.check)] $($_.probeId): $($_.detail)" } }
"RESULT = $result"
if ($result -ne 'PASS') { exit 1 }
