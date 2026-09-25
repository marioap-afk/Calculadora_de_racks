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
    CleanupActionId = @('CleanupAction'); CompletionFence = @('CompletionFence')
}
$nonExecutable = @('ProbeId', 'Threats', 'DA-P/H-P')
$unbound = 0; $freeText = 0; $idCount = 0
foreach ($probe in $rows.Keys) {
    $r = $rows[$probe]
    foreach ($c in $cols) {
        if ($nonExecutable -contains $c) { continue }
        foreach ($tok in @($r[$c])) {
            $parts = if ($c -eq 'ExpectedBefore' -or $c -eq 'ExpectedAfter') { $tok.Split('+') } else { @($tok) }
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
            if ($res['kind'] -notin 'PersistentFixtureIdentity', 'DisposableTriggerResource', 'StateCarrier', 'PayloadResource', 'ObserverRegistration', 'CommandResource') { C 'TRACE-RESOURCE' $probe $res['kind'] }
        }
        foreach ($y in $sa) { if ($t['SupportActions'] -notcontains $y) { C 'TRACE-SUPPORT' $probe $y } }
    }
    # derived execution plan: every element bound
    $plan = @('RUN-ENV-01', 'BOOT-01', 'DRIVER-SCRIPT-01', $drv) + $obs + $setup + $guards + @($trg, $chain) + $seq + @($exec, 'CMD-FINISH') + @($r['VerifierIds']) + @($clean) + $steps + @((One $r 'CompletionFence'), 'RESULT-RULE-V35')
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
            'R3-B01' { (One $r 'TriggerActionId') -eq 'TRG-APPEND-TRIGGER' -and (@($r['GuardIds']) -contains 'RG-DB-APPEND' -or (One $r 'ScheduleOriginEventId') -eq 'N-DB-APPEND') -and $Cat['F-TRIGGER-APPEND']['kind'] -eq 'DisposableTriggerResource' }
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
    ExecutionCatalogClosed = ($unbound -eq 0) -and ($catalogRefErrors -eq 0) -and ($freeText -eq 0)
    TriggerAuthorityClosed = ($unbound -eq 0) -and -not ($contra.Keys | Where-Object { $_ -in 'DRIVER-TRIGGER', 'TRIGGER-PRODUCES', 'TRIGGER-DISJOINT', 'ERASE-TRIGGER-DISJOINT', 'MARKER-PRODUCER', 'GUARD-TARGET' })
    SetupAuthorityClosed = $zero -and -not ($contra.Keys | Where-Object { $_ -in 'PRIMARY-T', 'OUTCOME-COMMIT', 'NESTED-T', 'STAGE-T', 'CHAIN-DATA', 'PAYLOAD-PATH' })
    PredicateAuthorityClosed = $zero -and -not ($contra.Keys | Where-Object { $_ -in 'PASS-RULE', 'UNKNOWN-RULE', 'CANDIDATE-BOUNDARY', 'VETO-FAIL', 'RESULT-CLASS', 'EXPECTED-DETERMINISM' })
    ResourceAuthorityClosed = $zero -and -not ($contra.Keys | Where-Object { $_ -like 'TRACE*' -or $_ -in 'FIXTURE', 'BODY-OBSERVER', 'MARKER-OBSERVER' })
    FixtureExecutionContractClosedV35 = $zero -and ($contradictions -eq 0) -and ($openBlockers -eq 0) -and $countsOk
    PlanDerivationClosed = ($plans -eq $rows.Count) -and ($rows.Count -eq 100) -and $zero
}
$result = if (@($pred.Values | Where-Object { -not $_ }).Count -eq 0 -and $findings.Count -eq 0) { 'PASS' } else { 'FAIL' }

$report = [ordered]@{
    schemaVersion = 1; validator = 'eng/research/I52Ctda/validate-v35-catalog.ps1'; scope = 'documents/catalog only; no runtime semantics'
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
