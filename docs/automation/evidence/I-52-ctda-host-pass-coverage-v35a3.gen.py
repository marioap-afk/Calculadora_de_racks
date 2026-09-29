#!/usr/bin/env python3
"""
Generator of the CTDA_HOST_PASS(V35-A3) coverage assessment (I-52), revision after decisions section 198 (structural campaign CLOSED after row 4; CTDA_HOST_PASS = FALSE).

Documentation / analysis only. It reads the canonical catalogs and the published evidence of the repository and writes:
  docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.json   (machine-readable, authoritative)
  docs/initiatives/I-52-ctda-host-pass-coverage-v35a3.md             (reviewable, generated from the same data)

Run from the repository root:  python docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.gen.py
Deterministic: no timestamps, stable ordering, input SHA-256 computed over LF-normalized content (so it does not depend on the
checkout's line-ending configuration). Nothing is inferred or transferred between rows, families or anchors: a row is PASS-GOVERNED
only if a governing execution of exactly that ProbeId produced PASS.
"""
import collections
import hashlib
import json
import os
import re

ROOT = os.getcwd()
NPM = 'docs/initiatives/I-52-native-probe-matrix-v35.md'
HEC = 'docs/initiatives/I-52-host-event-catalog-v27-correction.md'
HECT = 'docs/initiatives/I-52-host-event-catalog-v27.md'
CAT = 'docs/initiatives/I-52-execution-catalog-v35.json'
FIX = 'docs/initiatives/I-52-fixture-execution-contract-v35.md'
PROP = 'docs/initiatives/I-52-proposal-v35.md'
ORACLE = 'eng/research/I52Ctda/v35-oracle.json'
EV = 'docs/automation/evidence/'
SMOKE = EV + 'I-52-r3-host-smoke-bef6091b.json'
OUT_JSON = EV + 'I-52-ctda-host-pass-coverage-v35a3.json'
OUT_MD = 'docs/initiatives/I-52-ctda-host-pass-coverage-v35a3.md'
STATE = 'FALSE'  # decisions section 198: terminal and monotone (governing structural UNKNOWN on 04NO-S); it was NOT EVALUATED until then (sections 196/197)


def read(path):
    return open(os.path.join(ROOT, path), encoding='utf-8').read().replace('\r\n', '\n')


def sha(path):
    """SHA-256 over the LF-normalized content, so the value does not depend on core.autocrlf."""
    return hashlib.sha256(read(path).encode('utf-8')).hexdigest().upper()


# ---------------------------------------------------------------------------------------------- native rows (NPM-V35 section 5)
lines = read(NPM).split('\n')
schema = [x.strip().rstrip('.') for x in [l for l in lines if l.startswith('ProbeId; PrimaryAuthorityId')][0].split(';')]
start = [i for i, l in enumerate(lines) if l.startswith('## 5. Full normative matrix')][0]
end = [i for i, l in enumerate(lines) if l.startswith('## 6. Closure')][0]
raw_rows = [l.split(';') for l in lines[start + 1:end] if l.strip() and not l.startswith('```')]
assert len(schema) == 36 and len(raw_rows) == 100 and all(len(r) == 36 for r in raw_rows), 'NPM-V35 shape'
ix = {n: i for i, n in enumerate(schema)}
native = []
for pos, r in enumerate(raw_rows):
    native.append({
        'probeId': r[0], 'catalog': 'NPM-V35', 'matrixOrder': pos + 1,
        'family': r[ix['HeaderAuthority']], 'schedulerChain': r[ix['SchedulerChainId']], 'anchor': r[ix['ExecutionContextId']],
        'driver': r[ix['DriverId']], 'primaryAuthority': r[ix['PrimaryAuthorityId']], 'surface': r[ix['Surface']],
        'verifiers': r[ix['VerifierIds']].split(','), 'threats': r[ix['Threats']].split(','), 'daPHp': r[ix['DA-P/H-P']].split(','),
        'fixtureIdentity': 'fixture-v35 / FEC-V35-1 (native)',
        'referencedIdentifiers': sorted({t for k in ('PrimaryAuthorityId', 'ScheduleOriginEventId', 'SchedulerChainId', 'HeaderAuthority', 'DriverId',
                                                    'ObserverRegistrationIds', 'BodyObserverId', 'GuardIds', 'SetupActionIds', 'TriggerActionId',
                                                    'ExecutionContextId', 'MutationActionId', 'VerifierIds', 'Markers', 'MarkerStageBindings',
                                                    'ObservationPredicateIds', 'UnknownPredicateIds', 'FailPredicateIds', 'CleanupActionId',
                                                    'CompletionTokenIds') for t in re.split(r'[,+]', r[ix[k]]) if t}),
    })
assert len({n['probeId'] for n in native}) == 100
native_by_id = {n['probeId']: n for n in native}

# ---------------------------------------------------------------------------------------------- managed rows (HEC-V27-C1 section 2)
hl = read(HEC).split('\n')
s2 = [i for i, l in enumerate(hl) if l.startswith('## 2. Matriz normativa')][0]
s3 = [i for i, l in enumerate(hl) if l.startswith('## 3. Module registration')][0]
managed = []
for l in hl[s2:s3]:
    if l.startswith('| ') and not l.startswith('| ProbeId') and not l.startswith('|---'):
        c = [x.strip() for x in l.strip().strip('|').split('|')]
        if len(c) == 11:
            managed.append({
                'probeId': c[0], 'catalog': 'HEC-V27-C1',
                'primaryTriggerEventId': c[1], 'schedulerId': c[2], 'directActionId': c[3],
                'threats': [x.strip() for x in c[6].split(',')], 'surface': c[7].split('/')[0].strip(),
                'verifiers': [x.strip() for x in c[8].split(';')], 'daPHp': [x.strip() for x in c[9].split(',')],
                'fixtureIdentity': 'HF-V31-1 (managed; F-NOD / F-SRC are managed-fixture identities)',
            })
assert len(managed) == 27, len(managed)
REQUIRED18 = ['01', '02', '03', '04', '05', '06', '07', '08', '09C', '09E', '10S', '10M', '10SM', '11', '12', '13', '14', '15']
assert [m['probeId'] for m in managed if m['probeId'] in REQUIRED18] == REQUIRED18
for m in managed:
    m['predicateStatus'] = 'REQUIRED' if m['probeId'] in REQUIRED18 else 'OUTSIDE-PREDICATE'
literal18 = [m for m in managed if m['predicateStatus'] == 'REQUIRED']
outside9 = [m for m in managed if m['predicateStatus'] == 'OUTSIDE-PREDICATE']
assert len(literal18) == 18 and len(outside9) == 9 and all(m['probeId'].startswith('16') for m in outside9)

# ---------------------------------------------------------------------------------------------- structural-UNKNOWN table (proposal V35, RC-14 / M4)
prop = read(PROP).split('\n')
hdr = [i for i, l in enumerate(prop) if l.startswith('| Row | Reason | Conditional host fact | UNKNOWN through |')][0]
structural = {}
for l in prop[hdr + 2:]:
    if not l.startswith('|'):
        break
    c = [x.strip() for x in l.strip().strip('|').split('|')]
    for probe in [x.strip() for x in c[0].split(',')]:
        structural[probe] = {'reason': c[1], 'conditionalHostFact': c[2], 'unknownThrough': c[3]}
assert len(structural) == 15, len(structural)
assert all(p in native_by_id for p in structural)
# Documentation drift (decisions 198.8): the frozen proposal shorthand for the 04NO rows names OBS-PRIMARY-CALLBACK. The frozen row and RESULT-RULE-V35 are NOT amended;
# only the derived explanatory text carries the canonical clarification.
for _p, _s in structural.items():
    _s['unknownThroughFrozenShorthand'] = _s['unknownThrough']
    _s['unknownThroughCanonical'] = _s['unknownThrough']
structural['04NO-S']['unknownThroughCanonical'] = (
    'OBS-MARKERS unavailable (RESULT-RULE-V35 step 2): OBS-PRIMARY-CALLBACK was AVAILABLE (N-OBJ-CANCEL / cancelled), but the required positive markers '
    '+N-OBJ-UNDO, +N-OBJ-MOD and +N-OBJ-CLOSED were absent (EVIDENCE-COMPLETE clause 3)')
structural['04NO-UNDO-S']['unknownThroughCanonical'] = (
    'frozen shorthand: OBS-PRIMARY-CALLBACK unavailable (RESULT-RULE-V35 step 2). NOT EXECUTED. Clarification (decisions 198.8): the shorthand names one route only; on 04NO-S the '
    'observed route was OBS-MARKERS unavailable with the primary callback available')
PRE_CAMPAIGN_PASS = ['10N-S']

# ---------------------------------------------------------------------------------------------- baseline (campaign build identity)
smoke = json.load(open(os.path.join(ROOT, SMOKE), encoding='utf-8'))
baseline = {
    'status': 'CAMPAIGN BASELINE (used by structural campaign rows 1-4; the campaign is CLOSED, decisions 198)',
    'campaignSourceSha': smoke['package']['sourceSha'],
    'packageId': smoke['package']['id'],
    'packagePath': smoke['host']['packageHostCopy'],
    'packageZipSha256': smoke['package']['zipSha256'],
    'packageZipBytes': smoke['package']['zipBytes'],
    'sha256Sums': smoke['package']['sha256Sums'],
    'nativeArxSha256': smoke['package']['R-NATIVE'],
    'payloadArxSha256': smoke['package']['R-PAYLOAD'],
    'managedObserverDllSha256': smoke['package']['R-MANAGED'],
    'buildTupleHash': smoke['package']['buildTupleHash'],
    'freezeRevision': 'V35-A3', 'freezePackageHash': smoke['r3PreRunTuple']['V35_FREEZE_PACKAGE_HASH'],
    'acadExeSha256': smoke['r3PreRunTuple']['ACAD_EXE_SHA256'],
    'smokePreRunTupleHash': smoke['R3_PRE_RUN_TUPLE_HASH'],
    'includesFixes': smoke['package'].get('fixes'),
    'supersededPackage': 'bef11ce2 (A2 metadata)',
    'note': ('R3_HOST_EXECUTION_TUPLE_HASH is generated per execution (it binds ProbeId, row hash and scratch) and is not part of the baseline. '
             'Source drift since the baseline: none (eng/ and the frozen V35 documents are byte-identical between bef6091b and the published branch).'),
}

# ---------------------------------------------------------------------------------------------- executions (11)
EXEC = [
    ('E01', '09N-B', 'I-52-r3-canary-09N-B.json', '176', 'PASS-T (engine)', False,
     "NOT VALID: the operator confirmed that the save-changes dialog appeared and pressed 'Don't save' by hand (Coordinator ruling, section 181)"),
    ('E02', '02NDBMOD-S', 'I-52-r3-canary-02NDBMOD-S.json', '178', 'PASS-S (engine)', False,
     'NOT ACCEPTED: D-2 (corrupt CommandIdentity, use-after-free in I52Ctda_TokenSet)'),
    ('E03', '02NDBMOD-S', 'I-52-r3-canary-02NDBMOD-S-rerun.json', '180', 'UNKNOWN', False,
     'NOT VALID: D-3 led to an interactive exit and Save changed the scratch DWG (section 181)'),
    ('E04', '09N-B', 'I-52-r3-canary-09N-B-restart.json', '183-184', 'PASS-T', True, 'VALID PASS-T (Coordinator ruling on H-1, section 184)'),
    ('E05', '02NDBMOD-S', 'I-52-r3-canary-02NDBMOD-S-restart.json', '184', 'PASS-S', True, 'VALID PASS-S'),
    ('E06', '02NO-S', 'I-52-r3-canary-02NO-S-restart.json', '185', 'PASS-S', True, 'VALID PASS-S'),
    ('E07', '16N-S', 'I-52-r3-canary-16N-S-restart.json', '186', 'UNKNOWN', False,
     'NOT VALID: operator interaction on the host (independent of the engine UNKNOWN)'),
    ('E08', '16N-S', 'I-52-r3-canary-16N-S-rerun.json', '186', 'UNKNOWN', False,
     'Historical A2 UNKNOWN (UNK-MARKER-BINDING): D-4 was a V35 contract defect (section 187); contract amended (A3, section 188); not re-graded. '
     'Superseded under the narrow rule of section 197.5'),
    ('E09', '16N-S', 'I-52-r3-canary-16N-S-v35a3.json', '191', 'PASS-S', True, 'VALID PASS-S under V35-A3 (D-4 SEND anchor)'),
    ('E10', '10N-S', 'I-52-r3-canary-10N-S-v35a3.json', '192', 'PASS-S', True, 'VALID PASS-S under V35-A3 (D-4 FIXTURE anchor)'),
    ('E11', '16C-S', 'I-52-r3-canary-16C-S-v35a3.json', '193', 'PASS-S', True, 'VALID PASS-S under V35-A3 (D-4 CMDCTX anchor)'),
    # structural falsification campaign on the current baseline bef6091b (decisions section 198)
    ('E12', '09N-D', 'I-52-ctda-campaign-01-09N-D-bef6091b.json', '198', 'PASS-S', True, 'VALID GOVERNING PASS-S (structural campaign row 1)'),
    ('E13', '10N-M', 'I-52-ctda-campaign-02-10N-M-bef6091b.json', '198', 'PASS-M', True, 'VALID GOVERNING PASS-M (structural campaign row 2)'),
    ('E14', '10N-SM', 'I-52-ctda-campaign-03-10N-SM-bef6091b.json', '198', 'PASS-SM', True,
     'VALID GOVERNING PASS-SM (structural campaign row 3; Coordinator ruling: the raw scratch-runner label INVALID was a runner defect only, CLN-BASE erased 9 = 8 + F-REF-C)'),
    ('E15', '04NO-S', 'I-52-ctda-campaign-04-04NO-S-bef6091b.json', '198', 'UNKNOWN', True,
     'VALID GOVERNING STRUCTURAL UNKNOWN (structural campaign row 4; RESULT-RULE-V35 step 2; the raw scratch-runner label INVALID is non-authoritative; no rerun)'),
]
VER_SURFACE = {'VER-S': ['S'], 'VER-M': ['M'], 'VER-SM': ['SM'], 'VER-ALL': ['S', 'M', 'SM'], 'VER-NOD': ['NOD']}


def dig(d, *ks):
    for k in ks:
        if isinstance(d, dict) and k in d:
            d = d[k]
        else:
            return None
    return d


executions = []
for eid, probe, fname, sec, res, gov, why in EXEC:
    d = json.load(open(os.path.join(ROOT, EV + fname), encoding='utf-8'))
    assert d.get('probeId') == probe, (fname, d.get('probeId'))
    rec = d.get('hostRecord') if isinstance(d.get('hostRecord'), dict) else d  # the two earliest files nest the host record
    pk = d.get('package') if isinstance(d.get('package'), dict) else {}
    tup = rec.get('r3PreRunTuple') if isinstance(rec.get('r3PreRunTuple'), dict) else {}
    fz = tup.get('V35_FREEZE_PACKAGE_HASH') or rec.get('freezePackageHash')
    revision = 'V35-A3' if fz and fz.startswith('D9FD41B4') else ('V35-A2' if fz and fz.startswith('43DCE809') else 'UNKNOWN')
    log = rec.get('eventLog') if isinstance(rec.get('eventLog'), list) else []
    pay = lambda e: e.get('Payload') or {}
    boot = [(pay(e).get('declared'), pay(e).get('resolved')) for e in log if e.get('EventOrMarkerId') == 'BOOT-01' and pay(e).get('declared') is not None]
    cln = [pay(e).get('erased') for e in log if pay(e).get('step') == 'FIXTURE-RESOURCES']
    ver = []
    for e in log:
        eid_ = str(e.get('EventOrMarkerId', ''))
        if eid_.startswith('VER-'):
            ver.append({'verifier': eid_, 'phase': pay(e).get('phase'), 'sequence': e.get('Sequence')})
    fresh = [v for v in ver if v['phase'] == 'FRESH']
    surfaces = sorted({s for v in fresh for s in VER_SURFACE.get(v['verifier'], [])})
    source = tup.get('R3_SOURCE_SHA') or pk.get('sourceSha')
    executions.append({
        'executionId': eid, 'probeId': probe, 'decisionsSection': sec, 'engineResult': res,
        'governingStatus': 'GOVERNING' if gov else 'NON-GOVERNING', 'governing': gov, 'reason': why,
        'packageSha': pk.get('id') or ((source or '')[:8] or None),  # package ids are the first 8 hex of the R3 source SHA
        'sourceSha': source,
        'buildBinaryIdentity': {'buildTupleHash': tup.get('BUILD_MACHINE_TOOLCHAIN_TUPLE_HASH') or pk.get('buildTupleHash'),
                                'nativeArxSha256': tup.get('R_NATIVE_ARX_SHA256'), 'payloadArxSha256': tup.get('R_PAYLOAD_ARX_SHA256'),
                                'managedObserverSha256': tup.get('R_MANAGED_OBSERVER_SHA256'), 'packageZipSha256': pk.get('zipSha256')},
        'freezeRevisionOfPackage': revision, 'freezePackageHash': fz,
        'originalExecutionTuple': {**tup, 'R3_PRE_RUN_TUPLE_HASH': rec.get('R3_PRE_RUN_TUPLE_HASH'),
                                   'r3HostExecutionTupleHash': rec.get('r3HostExecutionTupleHash')},
        'rowApprovalHash': rec.get('rowApprovalHash') or d.get('rowApprovalHash'),
        'carriedForward': bool(gov and revision != 'V35-A3'),
        'campaignExecution': eid in ('E12', 'E13', 'E14', 'E15'),
        'currentBaselineStatus': 'CURRENT-BASELINE' if source == baseline['campaignSourceSha'] else 'SUPERSEDED-BUILD (baseline ' + baseline['packageId'] + ')',
        'boot01DeclaredResolved': ('%s/%s' % boot[0]) if boot else None,
        'clnBaseErased': cln[0] if cln else None,
        'verifierObservations': ver, 'freshSurfacesRead': surfaces,
        'processId': dig(rec, 'process', 'ProcessId') or dig(d, 'processRun', 'process', 'ProcessId'),
        'evidence': EV + fname, 'evidenceSha256': sha(EV + fname),
    })
assert len(executions) == 15 and sum(1 for e in executions if e['governing']) == 10
for e in executions:
    e['originalExecutionTuple'] = {k: v for k, v in e['originalExecutionTuple'].items() if v is not None}

# ---------------------------------------------------------------------------------------------- carry-forward conditions (decisions 197.4)
oracle = json.load(open(os.path.join(ROOT, ORACLE), encoding='utf-8'))['Z_approvalFreeze']['rows']
AMENDED_A3 = {'LOCK-RELEASE-BIND-01', 'OBS-LOCK-RELEASE-BOUND', 'UNK-MARKER-BINDING'}
carry = []
for e in executions:
    if not e['carriedForward']:
        continue
    n = native_by_id[e['probeId']]
    referenced_amended = sorted(set(n['referencedIdentifiers']) & AMENDED_A3)
    carry.append({
        'executionId': e['executionId'], 'probeId': e['probeId'],
        'rowHashUnchanged': e['rowApprovalHash'] == oracle[e['probeId']],
        'referencedCatalogEntriesUnchanged': not referenced_amended, 'amendedEntriesReferencedByRow': referenced_amended,
        'retainedLogReproducesResult': ('YES: research test "r3 prior valid governed results (6) unchanged offline" (eng/research/I52Ctda/tests/R3Tests.cs, '
                                       'D4PriorValidResults) re-evaluates the retained log under the current engine and asserts the recorded class'),
        'originalTupleRecorded': bool(e['originalExecutionTuple']), 'carriedForwardFlag': True,
        'provisionallyGoverning': True,
        'beforeCtdaHostPassTrue': 'must be re-executed on the campaign build OR carry an explicit Architect equivalence ruling (decisions 197.4)',
    })
    assert carry[-1]['rowHashUnchanged'] and carry[-1]['referencedCatalogEntriesUnchanged'], carry[-1]

# ---------------------------------------------------------------------------------------------- classification of rows
gov_by_probe = {e['probeId']: e for e in executions if e['governing']}
nong_by_probe = collections.defaultdict(list)
for e in executions:
    if not e['governing']:
        nong_by_probe[e['probeId']].append(e['executionId'])
D4 = {'16N-S': 'SEND', '10N-S': 'FIXTURE', '16C-S': 'CMDCTX'}
DEFERRED = {'16A-S': 'APPCTX anchor characterization (D-4 text unchanged for APPCTX); deferred in decisions sections 194/195 and HANDOFF; NOT in the structural group'}
campaign_rows = sorted([p for p in structural if p not in PRE_CAMPAIGN_PASS], key=lambda p: native_by_id[p]['matrixOrder'])
assert len(campaign_rows) == 14 and [p for p in campaign_rows if p in gov_by_probe] == ['09N-D', '10N-M', '10N-SM', '04NO-S']
campaign_stopped = [p for p in campaign_rows if p not in gov_by_probe]
assert len(campaign_stopped) == 10 and campaign_stopped[0] == '04NO-UNDO-S' and campaign_rows[:4] == ['09N-D', '10N-M', '10N-SM', '04NO-S']
cat_entries = json.load(open(os.path.join(ROOT, CAT), encoding='utf-8'))['entries']

for n in native:
    p = n['probeId']
    n['structuralUnknownCandidate'] = p in structural
    if p in structural:
        n['structural'] = structural[p]
    n['nonGoverningExecutions'] = nong_by_probe.get(p, [])
    n['mutationActionId'] = r_mut = raw_rows[n['matrixOrder'] - 1][ix['MutationActionId']]
    n['expectedClnBaseErased'] = 9 if 'F-REF-C' in cat_entries[r_mut].get('writeSet', []) else 8
    if p in gov_by_probe:
        e = gov_by_probe[p]
        assert e['engineResult'].startswith('PASS') or e['engineResult'] == 'UNKNOWN', e['engineResult']
        n.update({'classification': 'PASS-GOVERNED' if e['engineResult'].startswith('PASS') else 'UNKNOWN-GOVERNED', 'result': e['engineResult'], 'governingExecution': e['executionId'],
                  'carriedForward': e['carriedForward'], 'currentBaselineStatus': e['currentBaselineStatus'],
                  'executionTuple': {'package': e['packageSha'], 'sourceSha': e['sourceSha'], 'freezeRevisionOfPackage': e['freezeRevisionOfPackage'],
                                     'r3HostExecutionTupleHash': e['originalExecutionTuple'].get('r3HostExecutionTupleHash'), 'rowApprovalHash': e['rowApprovalHash']},
                  'a3Validity': ('native V35-A3 execution' if not e['carriedForward'] else 'carried forward (decisions 188 / 197.4): provisional; re-execute or Architect equivalence ruling before TRUE'),
                  'd4Anchor': D4.get(p), 'reasonIfNotGoverning': None,
                  'surfacesReadInPass': e['freshSurfacesRead'] if e['engineResult'].startswith('PASS') else [], 'evidenceRefs': [e['evidence']]})
    elif p in DEFERRED:
        n.update({'classification': 'DEFERRED', 'result': None, 'reasonIfNotGoverning': DEFERRED[p], 'surfacesReadInPass': [], 'evidenceRefs': [],
                  'carriedForward': False, 'currentBaselineStatus': None})
    elif p in campaign_stopped:
        why = ('the structural campaign was stopped after row 4 (04NO-S governing structural UNKNOWN => CTDA_HOST_PASS = FALSE, terminal; decisions section 198): '
               'NOT EXECUTED and NOT AUTHORIZED; not FAIL, not UNKNOWN, not deferred')
        if p == '13A-SM':
            why += '. Previously listed DEFERRED (decisions 194/195, APPCTX chain) and then included in the authorized 14-row group at position %d' % (campaign_rows.index(p) + 1)
        n.update({'classification': 'NOT-EXECUTED', 'result': None, 'reasonIfNotGoverning': why,
                  'surfacesReadInPass': [], 'evidenceRefs': [], 'carriedForward': False, 'currentBaselineStatus': None})
    else:
        n.update({'classification': 'NOT-EXECUTED', 'result': None, 'reasonIfNotGoverning': 'no governing or non-governing execution of this ProbeId exists',
                  'surfacesReadInPass': [], 'evidenceRefs': [], 'carriedForward': False, 'currentBaselineStatus': None})
    n['executed'] = any(x['probeId'] == p for x in executions)
    n['governing'] = p in gov_by_probe
    n['structuralUnknown'] = bool(p in structural and p in gov_by_probe and gov_by_probe[p]['engineResult'] == 'UNKNOWN')
    n['terminalForHostPass'] = bool(p in gov_by_probe and gov_by_probe[p]['engineResult'] in ('UNKNOWN', 'FAIL'))
    n['campaignRow'] = campaign_rows.index(p) + 1 if p in campaign_rows else None
    n['campaignStoppedBeforeExecution'] = p in campaign_stopped
    n['contributesTo'] = {'DA-P7': n['surfacesReadInPass'] if n['classification'] == 'PASS-GOVERNED' else [], 'DA-P8': False, 'DA-P10': False}

for m in managed:
    m['classification'] = 'NOT-EXECUTED' if m['predicateStatus'] == 'REQUIRED' else 'OUTSIDE-PREDICATE'
    m['result'] = None
    m['reasonIfNotGoverning'] = ('no executable authority exists for this managed row yet (decisions 197.3: MISSING, presently considered IMPLEMENTABLE; '
                                 'not an inability determination). Its CTDA_HOST_PASS purpose is MOOT: the predicate is terminal FALSE (decisions 198)') if m['predicateStatus'] == 'REQUIRED' else \
        'HEC-V27-C1 scheduler row kept in the catalog but outside the CTDA_HOST_PASS predicate (decisions 197.2)'
    m['nonGoverningExecutions'] = []
    m['evidenceRefs'] = []
    m['surfacesReadInPass'] = []
    m['contributesTo'] = {'DA-P7': [], 'DA-P8': False, 'DA-P10': False}
    m['wouldContribute'] = {'DA-P8': m['probeId'] == '11', 'DA-P10': m['probeId'] in ('14', '15'), 'DA-P7-surface': 'NOD' if m['probeId'] == '11' else None}

# ---------------------------------------------------------------------------------------------- structural campaign design (NOT authorized)
remaining14 = campaign_rows
already = PRE_CAMPAIGN_PASS
assert len(remaining14) == 14 and already == ['10N-S'], (remaining14, already)
COORD14 = ['10NDOC-WILL-SM', '02NEDW-ALL', '02NEDC-ALL', '13A-SM', '02NTAS-ALL', '02NTA-ALL', '02NAPP-SM', '04NO-S', '04NO-UNDO-S',
           'COBJUNDO16SND-ALL', 'COBJUNDO16APP-ALL', '09N-D', '10N-M', '10N-SM']
assert sorted(remaining14) == sorted(COORD14)
run_order = []
for i, p in enumerate(remaining14, 1):
    n = native_by_id[p]
    s = structural[p]
    ge = gov_by_probe.get(p)
    run_order.append({
        'order': i, 'probeId': p, 'matrixOrder': n['matrixOrder'], 'anchor': n['anchor'], 'schedulerChain': n['schedulerChain'], 'family': n['family'],
        'surface': n['surface'], 'verifiers': n['verifiers'],
        'expectedPossibleStructuralOutcome': 'UNKNOWN through ' + s['unknownThroughCanonical'] + ' if the conditional host fact does not hold; otherwise the row may reach its PASS class',
        'unknownThroughFrozenShorthand': s['unknownThroughFrozenShorthand'],
        'conditionalHostFact': s['conditionalHostFact'], 'reasonStructural': s['reason'],
        'whyInThisCampaign': ('RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. '
                              'A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining native rows or the managed authority.'),
        'state': ('EXECUTED - %s (%s)' % (ge['engineResult'], ge['executionId'])) if ge else 'NOT EXECUTED - CAMPAIGN STOPPED BEFORE EXECUTION (NOT AUTHORIZED)',
        'executed': bool(ge), 'governing': bool(ge), 'result': ge['engineResult'] if ge else None, 'governingExecution': ge['executionId'] if ge else None,
        'structuralUnknown': bool(ge and ge['engineResult'] == 'UNKNOWN'), 'terminalForHostPass': bool(ge and ge['engineResult'] in ('UNKNOWN', 'FAIL')),
        'campaignStoppedBeforeExecution': not ge,
    })
campaign = {
    'status': 'EXECUTED AND CLOSED AFTER ROW 4 (decisions 198): rows 1-3 governing PASS, row 4 governing structural UNKNOWN => CTDA_HOST_PASS = FALSE (terminal); rows 5-14 NOT EXECUTED / NOT AUTHORIZED',
    'designHistory': 'Designed in decisions 197.9 (NOT AUTHORIZED at that point); authorized and executed afterwards by Coordinator order; the rules below are the design rules as authorized.',
    'structuralRowsTotal': 15, 'structuralAlreadyPass': already, 'structuralRemaining': 14,
    'campaignRowsExecuted': 4, 'campaignRowsGoverningPass': 3, 'campaignRowsGoverningUnknown': 1, 'campaignRowsStoppedBeforeExecution': 10,
    'stopRule': 'first governing UNKNOWN or FAIL => CTDA_HOST_PASS = FALSE (terminal), STOP, no rerun; INVALID or non-governing => STOP for a Coordinator ruling (never inferred FALSE)',
    'orderRule': ('Deterministic: ascending NPM-V35 section 5 row order (the catalog order). The corpus has no prior probability model of which host fact fails, '
                  'so no risk-based order is invented. The order changes only how early a FALSE would appear, never the outcome, because FALSE is monotone; '
                  'the Coordinator may reorder before authorization.'),
    'rules': [
        'Exactly one ProbeId per AutoCAD process.',
        'The same exact campaign build/tuple (baseline below) for every row; a build change stops the campaign for a Coordinator ruling.',
        'No result transfer between rows, families or anchors.',
        'No retry of an UNKNOWN or FAIL.',
        'A governing UNKNOWN => CTDA_HOST_PASS = FALSE (terminal). A governing FAIL => CTDA_HOST_PASS = FALSE (terminal).',
        'Stop the campaign immediately on the first governing UNKNOWN or FAIL. PASS => continue to the next row.',
        'A run that is not a valid governed execution (operator interaction, environment/tuple/package/trust failure, harness defect, timeout) is NON-GOVERNING: '
        'the campaign halts for a Coordinator ruling; it is neither PASS nor FALSE and is never retried automatically.',
        'Each row needs its own Coordinator authorization, the exact TRUSTEDPATHS entry, and the Owner no-touch declaration.',
        '16A-S is NOT part of this group; 13A-SM is.',
    ],
    'baseline': baseline, 'runOrder': run_order,
}
assert sum(1 for r in run_order if r['executed']) == 4 and sum(1 for r in run_order if r['campaignStoppedBeforeExecution']) == 10

# ---------------------------------------------------------------------------------------------- counts
nat_counts = collections.Counter(n['classification'] for n in native)
man18_counts = collections.Counter(m['classification'] for m in literal18)
not_executed_required = nat_counts['NOT-EXECUTED'] + nat_counts['DEFERRED'] + man18_counts['NOT-EXECUTED']
counts = {
    'nativeRowsTotal': 100, 'managedRowsRequired': 18, 'managedRowsOutsidePredicate': 9, 'managedRowsInHEC-V27-C1-section2': 27,
    'native': dict(nat_counts), 'managedRequired18': dict(man18_counts), 'managedOutsidePredicate9': {'OUTSIDE-PREDICATE': len(outside9)},
    'requiredRows118': {
        'PASS-GOVERNED': nat_counts['PASS-GOVERNED'], 'FAIL-GOVERNED': nat_counts['FAIL-GOVERNED'], 'UNKNOWN-GOVERNED': nat_counts['UNKNOWN-GOVERNED'],
        'EXECUTED-NON-GOVERNING (rows with ONLY non-governing executions)': 0,
        'NOT-EXECUTED': nat_counts['NOT-EXECUTED'] + man18_counts['NOT-EXECUTED'], 'DEFERRED': nat_counts['DEFERRED'],
        'NOT-EXECUTED-REQUIRED (not executed + deferred)': not_executed_required,
        'of which CAMPAIGN-STOPPED-BEFORE-EXECUTION (native, structural rows 5-14)': len(campaign_stopped),
    },
    'executions': {'total': 15, 'governing': 10, 'governingPass': 9, 'governingUnknown': 1, 'nonGoverning': 5, 'distinctValidProbeIds': 10},
    'structural': {'total': 15, 'alreadyPassBeforeCampaign': 1, 'campaignRows': 14, 'campaignExecuted': 4, 'campaignGoverningPass': 3, 'campaignGoverningUnknown': 1,
                   'campaignStoppedBeforeExecution': 10},
}
assert nat_counts['PASS-GOVERNED'] == 9 and nat_counts['UNKNOWN-GOVERNED'] == 1 and nat_counts['DEFERRED'] == 1 and nat_counts['NOT-EXECUTED'] == 89 and not_executed_required == 108

# ---------------------------------------------------------------------------------------------- tag coverage
allrows = native + literal18
DAHP = ['P%d' % i for i in range(1, 13)]
hp = {}
for t in DAHP:
    tagged = [r for r in allrows if t in r['daPHp']]
    hp[t] = {'rowsTagged': len(tagged), 'passGoverned': sum(1 for r in tagged if r['classification'] == 'PASS-GOVERNED'),
             'nativeTagged': sum(1 for r in tagged if r['catalog'] == 'NPM-V35'), 'managedTagged': sum(1 for r in tagged if r['catalog'] == 'HEC-V27-C1'),
             'passingProbeIds': [r['probeId'] for r in tagged if r['classification'] == 'PASS-GOVERNED']}
    hp[t]['satisfied'] = hp[t]['rowsTagged'] > 0 and hp[t]['passGoverned'] == hp[t]['rowsTagged']


def tbase(t):
    return re.match(r'T\d+', t).group(0)


tt = {}
for i in range(1, 17):
    t = 'T%d' % i
    tagged = [r for r in allrows if any(tbase(x) == t for x in r['threats'] if re.match(r'T\d+', x))]
    tt[t] = {'rowsTagged': len(tagged), 'passGoverned': sum(1 for r in tagged if r['classification'] == 'PASS-GOVERNED'),
             'nativeTagged': sum(1 for r in tagged if r['catalog'] == 'NPM-V35'), 'managedTagged': sum(1 for r in tagged if r['catalog'] == 'HEC-V27-C1'),
             'scope': 'OUT (diagnostic only; oracle: no state change)' if t == 'T14' else 'IN'}
    tt[t]['satisfied'] = (t == 'T14') or (tt[t]['rowsTagged'] > 0 and tt[t]['passGoverned'] == tt[t]['rowsTagged'])

# DA-P7: a surface counts only if a completed FRESH verifier read for it is recorded in the log of a governing PASS execution (decisions 197.7)
read_by_surface = {s: sorted({e['executionId'] + ' ' + e['probeId'] for e in executions if e['governing'] and e['engineResult'].startswith('PASS') and s in e['freshSurfacesRead']})
                   for s in ('S', 'M', 'SM', 'NOD')}
dap7 = {'rule': 'a physical surface counts only if a completed FRESH verifier read for it is recorded in the log of a governing PASS execution; catalog labels alone do not count',
        'surfaces': {s: ('SATISFIED' if read_by_surface[s] else 'NOT SATISFIED') for s in ('S', 'M', 'SM', 'NOD')},
        'evidence': read_by_surface,
        'note': ('09N-B is labelled Surface S/M, but its log has VER-S and VER-T FRESH reads and no VER-M: M is not credited by that row; M is credited by 10N-M (E13) and SM by 10N-SM (E14). '
             'NOD is a surface of managed row 11 only. DA-P7 is now moot for the predicate (terminal FALSE, decisions 198).')}
dap8 = {'requiredBy': 'HEC row 11', 'rowClassification': [m['classification'] for m in managed if m['probeId'] == '11'][0], 'satisfied': False}
dap10 = {'requiredBy': 'HEC rows 14 and 15', 'rowClassifications': {m['probeId']: m['classification'] for m in managed if m['probeId'] in ('14', '15')}, 'satisfied': False}
surfaces_read = sorted(s for s in ('S', 'M', 'SM', 'NOD') if read_by_surface[s])
missing_surfaces = [s for s in ('S', 'M', 'SM', 'NOD') if not read_by_surface[s]]

# ---------------------------------------------------------------------------------------------- clauses
native_fixture_ok = all(e['boot01DeclaredResolved'] == '8/8' and e['clnBaseErased'] == str(native_by_id[e['probeId']]['expectedClnBaseErased']) and e['freezePackageHash']
                        for e in executions if e['governing'])
assert native_fixture_ok
clauses = [
    {'clause': 1, 'name': 'fixture identity', 'status': 'INCOMPLETE',
     'halves': {'native fixture-v35 / FEC-V35': 'SATISFIED for the ten governing native rows',
                'managed HF-V31-1 incl. F-NOD and F-SRC': 'NOT SATISFIED (never instantiated)'},
     'why': ('Native half: each of the ten governing logs records BOOT-01 declared/resolved = 8/8 and CLN-BASE FIXTURE-RESOURCES erased = 8 (9 for 10N-SM: MUT-SM appends F-REF-C), and binds the '
             'freeze package hash (A2 43DCE809.. or A3 D9FD41B4..) and the manifest rowApprovalHash (decisions 197.6, 198). Managed half: HF-V31-1 with F-NOD and F-SRC (identities that exist only in '
             'the managed HF-V26 primitives, not in the eight native PersistentFixtureIdentity entries) has never been instantiated in a governed execution. Moot for the predicate (terminal FALSE).')},
    {'clause': 2, 'name': 'complete execution (100 native + 18 managed, exact governed identity/tuple)', 'status': 'INCOMPLETE',
     'why': ('native %d/100 governing PASS + %d governing UNKNOWN (04NO-S); %d not executed (of which %d stopped by the closed campaign), %d deferred; managed 0/18. The three A2-package results are '
             'provisionally governing by conditional carry-forward. Incomplete and now unreachable for the predicate: it is terminal FALSE (decisions 198).'
             % (nat_counts['PASS-GOVERNED'], nat_counts['UNKNOWN-GOVERNED'], nat_counts['NOT-EXECUTED'], len(campaign_stopped), nat_counts['DEFERRED']))},
    {'clause': 3, 'name': 'result completeness (H-P1..H-P12, IN T1..T16, T14 OUT, every tagged row PASS, no FAIL, no UNKNOWN)', 'status': 'NOT SATISFIED (DISQUALIFIED)',
     'why': 'A governing UNKNOWN exists in the required native matrix (04NO-S, structural, RESULT-RULE-V35 step 2). H-P satisfied: %s; IN T satisfied: %s (rows tagged and all PASS).' % (
         [t for t in DAHP if hp[t]['satisfied']] or 'none', [t for t in tt if tt[t]['satisfied'] and t != 'T14'] or 'none')},
    {'clause': 4, 'name': 'observability (DA-P7 / DA-P8 / DA-P10)', 'status': 'INCOMPLETE',
     'why': 'DA-P7: S, M, SM SATISFIED; NOD NOT SATISFIED; DA-P8 needs HEC row 11 (NOT-EXECUTED); DA-P10 needs HEC rows 14 and 15 (NOT-EXECUTED). Moot for the predicate (terminal FALSE).'},
    {'clause': 5, 'name': 'no FAIL / UNKNOWN anywhere in the governed matrix', 'status': 'NOT SATISFIED (DISQUALIFIED)',
     'why': ('0 FAIL-GOVERNED and 1 UNKNOWN-GOVERNED: 04NO-S (structural UNKNOWN: valid executed result; authorizes no rerun; implies no architecture defect; forces CTDA_HOST_PASS = FALSE, '
             'decisions 196.1 / 197.1). The two historical A2 UNKNOWN executions of 16N-S (E07, E08) remain in the execution history and are outside the governing matrix under the narrow '
             'supersession rule of decisions 197.5; that rule does not apply to 04NO-S (a genuine host-behavior UNKNOWN).')},
]

state_model = {
    'values': ['TRUE', 'FALSE', 'NOT EVALUATED'],
    'TRUE': 'all five clauses hold on governing evidence',
    'FALSE': 'terminal and monotone: a governing FAIL; a governing UNKNOWN (structural or defensive: a valid executed result that authorizes no rerun); or an explicit recorded '
             'inability determination for a required row, fixture or instrument (V26 section 9). Inability is never implicit.',
    'NOT EVALUATED': 'no disqualifier exists and at least one required row is unexecuted but still considered producible. It is NOT TRUE.',
    'retiredTerm': 'NOT EVALUABLE',
    'terminology': 'docs use exactly TRUE / FALSE / NOT EVALUATED for the host predicate; "PASS" is used only for row results and in the predicate name',
}
state = {'CTDA_HOST_PASS(V35-A3)': STATE, 'terminal': True, 'monotone': True, 'notTrue': True,
         'falseBecause': 'one governing structural UNKNOWN in the required native matrix: 04NO-S (E15, RESULT-RULE-V35 step 2); it is NOT an inability determination',
         'previousState': 'NOT EVALUATED (decisions 196 / 197, until the structural campaign row 4)',
         'stateHistory': [{'state': 'NOT EVALUATED', 'sections': '196, 197', 'until': 'structural campaign row 4 (04NO-S)'},
                          {'state': 'FALSE (terminal)', 'section': '198', 'trigger': '04NO-S governing structural UNKNOWN'}],
         'rerun': 'NONE; no attempt to obtain a better result',
         'managedAuthority': 'MISSING but presently considered IMPLEMENTABLE (not an inability determination); its CTDA_HOST_PASS purpose is MOOT (terminal FALSE)',
         'ContextIsolationAuthority': 'UNKNOWN (not proven, not resolved and not refuted by CTDA_HOST_PASS = FALSE)', 'SafeOperationalState': 'FALSE_FOR_ADMISSION', 'G3': 'STOPPED',
         'ALT-21C': 'NOT ADMISSIBLE under the current condition', 'ALT-21E': 'DEFAULT FALLBACK IN EFFECT (V18: NO SUPPORTED EXECUTION ENVIRONMENT)',
         'ALT-21D': 'NOT SELECTED (possible only after a concrete Proposal, Coordinator review, Architect review and an explicit Owner decision)',
         'ADR-0036': 'PROPOSED / AMENDED FOR V18', 'freezeV18': 'GOVERNING', 'freezeV35': 'UNCHANGED'}

doc = {
    'schemaVersion': 3, 'initiative': 'I-52', 'kind': 'CTDA_HOST_PASS coverage assessment', 'definitionRevision': 'V35-A3',
    'mode': 'DOCUMENTATION / ANALYSIS ONLY (no AutoCAD in this gate, no probe executed by this gate, no source change); records the campaign results of decisions section 198',
    'inputsSha256OfLfNormalizedContent': {p: sha(p) for p in (NPM, HEC, HECT, CAT, FIX, PROP, ORACLE, SMOKE)},
    'stateModel': state_model, 'state': state, 'counts': counts, 'clauses': clauses,
    'daP7': dap7, 'daP8': dap8, 'daP10': dap10, 'hp': hp, 'threats': tt,
    'managedPredicate': {'required18': REQUIRED18, 'outsidePredicate9': [m['probeId'] for m in outside9]},
    'carryForward': carry, 'structuralCampaign': campaign,
    'executions': executions, 'nativeRows': native, 'managedRows': managed,
}
for n in doc['nativeRows']:
    n.pop('referencedIdentifiers', None)
with open(os.path.join(ROOT, OUT_JSON), 'w', encoding='utf-8', newline='\n') as f:
    json.dump(doc, f, indent=2, ensure_ascii=False)
    f.write('\n')

# ---------------------------------------------------------------------------------------------- markdown
o = []
w = o.append
w('# I-52 — CTDA_HOST_PASS(V35-A3): coverage assessment')
w('')
w('> **Generated** from `I-52-ctda-host-pass-coverage-v35a3.json` by `docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.gen.py`. Documentation / analysis only: this gate ran no AutoCAD and no probe and changed no source; it renders the recorded structural-campaign results (decisions section 198).')
w('> The JSON is authoritative; this file is its reviewable rendering. Decision record: `docs/automation/decisions/I-52.md` sections 196, 197 and 198.')
w('')
w('## 1. Result')
w('')
w('```text')
w('CTDA_HOST_PASS(V35-A3) = FALSE   (TERMINAL / MONOTONE; not TRUE; no rerun)')
w('Reason: one governing structural UNKNOWN in the required native matrix: 04NO-S (RESULT-RULE-V35 step 2). Not an inability determination.')
w('Clause 1 fixture identity                 INCOMPLETE   native half SATISFIED (10 governing rows); managed half NOT SATISFIED')
w('Clause 2 complete execution               INCOMPLETE   native %d PASS + %d UNKNOWN governing of 100 (%d not executed, %d deferred); managed 0/18' % (
    nat_counts['PASS-GOVERNED'], nat_counts['UNKNOWN-GOVERNED'], nat_counts['NOT-EXECUTED'], nat_counts['DEFERRED']))
w('Clause 3 result completeness              NOT SATISFIED (DISQUALIFIED: governing UNKNOWN 04NO-S)')
w('Clause 4 observability DA-P7/P8/P10       INCOMPLETE   DA-P7: S, M, SM satisfied; NOD not; DA-P8 and DA-P10 need managed rows (not executed)')
w('Clause 5 no FAIL/UNKNOWN                  NOT SATISFIED (DISQUALIFIED: 0 FAIL, 1 UNKNOWN governing)')
w('```')
w('')
w('Three-state model (decisions 197.1): **TRUE** — all five clauses hold on governing evidence. **FALSE** — terminal and monotone: a governing FAIL, a governing UNKNOWN, or an explicit '
  'recorded inability determination for a required row, fixture or instrument. **NOT EVALUATED** — no disqualifier exists and at least one required row is unexecuted but still considered '
  'producible. The term "NOT EVALUABLE" is retired.')
w('')
w('State history: `NOT EVALUATED` (decisions 196 / 197, until the structural campaign) → **`FALSE`** (decisions 198, after row 4 `04NO-S`). The earlier state is recorded history; it is not the current state.')
w('')
w('```text')
w('ContextIsolationAuthority = UNKNOWN   (not proven, not resolved, not refuted by this FALSE)')
w('SafeOperationalState      = FALSE_FOR_ADMISSION')
w('G3 = STOPPED   ALT-21C = NOT ADMISSIBLE   ALT-21E = DEFAULT FALLBACK IN EFFECT   ALT-21D = NOT SELECTED')
w('```')
w('')
w('## 2. Counts')
w('')
w('| Class | Native (100) | Managed required (18) | Required 118 |')
w('|---|---|---|---|')
for k in ['PASS-GOVERNED', 'FAIL-GOVERNED', 'UNKNOWN-GOVERNED', 'EXECUTED-NON-GOVERNING', 'NOT-EXECUTED', 'DEFERRED']:
    nn = nat_counts.get(k, 0)
    mm = man18_counts.get(k, 0)
    w('| %s | %d | %d | %d |' % (k, nn, mm, nn + mm))
w('')
w('Required rows not executed (not executed + deferred): **%d** = %d native not executed (of which %d were stopped before execution by the closed campaign, structural rows 5-14) + %d deferred native (16A-S) + 18 managed. '
  '*EXECUTED-NON-GOVERNING* is counted per ROW (a row whose only executions are non-governing): there is none. Per EXECUTION: 15 attempts, 10 governing (9 PASS, 1 UNKNOWN) and 5 not (section 3).' % (
      not_executed_required, nat_counts['NOT-EXECUTED'], len(campaign_stopped), nat_counts['DEFERRED']))
w('')
w('The nine `16{S,M,SM}-{SEND,CONTEXT,IDLE}` managed scheduler rows of HEC-V27-C1 are **OUTSIDE-PREDICATE** (decisions 197.2); they are listed in section 7 and are not counted above.')
w('')
w('## 3. The 15 executions (11 before the structural campaign, 4 campaign rows E12-E15)')
w('')
w('| Id | ProbeId | Package / source | Freeze of package | Governing | Carried fwd | Baseline status | Fixture BOOT-01 / CLN-BASE | FRESH surfaces | Engine result | Reason / ruling |')
w('|---|---|---|---|---|---|---|---|---|---|---|')
for e in executions:
    w('| %s | %s | `%s` / `%s` | %s | %s | %s | %s | %s / %s | %s | %s | %s |' % (
        e['executionId'], e['probeId'], e['packageSha'], (e['sourceSha'] or '—')[:8], e['freezeRevisionOfPackage'], e['governingStatus'],
        'yes' if e['carriedForward'] else 'no', e['currentBaselineStatus'], e['boot01DeclaredResolved'] or '—', e['clnBaseErased'] or '—',
        ','.join(e['freshSurfacesRead']) or '—', e['engineResult'], e['reason']))
w('')
w('Full tuple per execution (build tuple hash, native/payload/managed binary SHA-256, `R3_PRE_RUN_TUPLE_HASH`, `R3_HOST_EXECUTION_TUPLE_HASH`, row approval hash, verifier observations, evidence path and SHA-256) is in the JSON.')
w('The six earlier governing results (E04-E06, E09-E11) ran on builds **other than the current baseline** `%s` (decisions 197.4); the four campaign results (E12-E15) ran on the baseline. '
  'Both facts are historical: the predicate is terminal FALSE (decisions 198).' % baseline['packageId'])
w('')
w('### 3.1 Carry-forward conditions (A2 → A3, decisions 197.4), checked mechanically for E04–E06')
w('')
w('| Execution | ProbeId | Row hash = oracle | Amended A3 entries referenced | Retained log reproduces result | Tuple recorded | carriedForward |')
w('|---|---|---|---|---|---|---|')
for c in carry:
    w('| %s | %s | %s | %s | yes (research test D4PriorValidResults) | %s | true |' % (
        c['executionId'], c['probeId'], 'yes' if c['rowHashUnchanged'] else 'NO', ','.join(c['amendedEntriesReferencedByRow']) or 'none',
        'yes' if c['originalTupleRecorded'] else 'NO'))
w('')
w('Provisionally governing. The re-execution / equivalence-ruling requirement before `CTDA_HOST_PASS` could be TRUE is moot: the predicate is terminal FALSE (decisions 198). Recorded for history.')
w('')
w('## 4. Clause evaluation')
w('')
w('| Clause | Status | Why |')
w('|---|---|---|')
for c in clauses:
    w('| %d — %s | **%s** | %s |' % (c['clause'], c['name'], c['status'], c['why'].replace('|', '/')))
w('')
w('### 4.1 DA-P7 / DA-P8 / DA-P10')
w('')
w('Rule (decisions 197.7): a physical surface counts for DA-P7 only if a completed FRESH verifier read for it is recorded in the log of a governing PASS execution; catalog labels alone do not count.')
w('')
w('| Surface | DA-P7 | Evidence (governing FRESH reads) |')
w('|---|---|---|')
for s in ('S', 'M', 'SM', 'NOD'):
    w('| %s | **%s** | %s |' % (s, dap7['surfaces'][s], '; '.join(read_by_surface[s]) or '—'))
w('')
w('- %s' % dap7['note'])
w('- DA-P8: satisfied only by HEC row 11 (F-NOD): NOT-EXECUTED.')
w('- DA-P10: satisfied only by HEC rows 14 and 15 (F-SRC): NOT-EXECUTED.')
w('')
w('### 4.2 DA-P / H-P tag coverage (native + managed required 18; every tagged row must PASS)')
w('')
w('| Tag | Rows tagged | of which native | of which managed | PASS-GOVERNED | Satisfied |')
w('|---|---|---|---|---|---|')
for t in DAHP:
    h = hp[t]
    w('| %s | %d | %d | %d | %d (%s) | %s |' % (t, h['rowsTagged'], h['nativeTagged'], h['managedTagged'], h['passGoverned'], ', '.join(h['passingProbeIds']) or '—',
                                                 ('no (cross-cutting property, no row carries it: see 4.1)' if t == 'P7' else ('YES' if h['satisfied'] else 'no'))))
w('')
w('### 4.3 Threat coverage (IN T1..T16; T14 OUT). `T2-DOC`, `T2-OBJ`, `T2-ENTITY` are counted under T2.')
w('')
w('| Threat | Scope | Rows tagged | native | managed | PASS-GOVERNED | Satisfied |')
w('|---|---|---|---|---|---|---|')
for i in range(1, 17):
    t = 'T%d' % i
    h = tt[t]
    w('| %s | %s | %d | %d | %d | %d | %s |' % (t, h['scope'], h['rowsTagged'], h['nativeTagged'], h['managedTagged'], h['passGoverned'], 'n/a (OUT)' if t == 'T14' else ('YES' if h['satisfied'] else 'no')))
w('')
w('## 5. Structural-UNKNOWN candidate rows (15; proposal V35 RC-14 / M4)')
w('')
w('UNKNOWN is a legitimate possible outcome of a correct run for these rows, not a guaranteed one. Outcomes: `10N-S`, `09N-D`, `10N-M`, `10N-SM` returned PASS; `04NO-S` returned a governing structural UNKNOWN '
  '(the campaign stopped there); the other ten were not executed.')
w('')
w('| ProbeId | Status | Anchor | Surface | Conditional host fact | UNKNOWN through |')
w('|---|---|---|---|---|---|')
for p in sorted(structural, key=lambda x: native_by_id[x]['matrixOrder']):
    n = native_by_id[p]
    st = ('%s (%s)' % (n['classification'], gov_by_probe[p]['executionId'])) if p in gov_by_probe else (n['classification'] + (' - campaign stopped' if n['campaignStoppedBeforeExecution'] else ''))
    w('| %s | %s | %s | %s | %s | %s |' % (p, st, n['anchor'], n['surface'], structural[p]['conditionalHostFact'].replace('|', '/'), structural[p]['unknownThroughCanonical'].replace('|', '/')))
w('')
w('### 5.1 Wording clarification for `04NO-S` (decisions 198.8; documentation drift only)')
w('')
w('The frozen shorthand in `proposal-v35.md` (line 220) describes the 04NO rows as "UNKNOWN through OBS-PRIMARY-CALLBACK unavailable". On `04NO-S` that is **not** what occurred: '
  '`OBS-PRIMARY-CALLBACK` was **available** (`N-OBJ-CANCEL` / `cancelled`), but the required `OBS-MARKERS` positive callbacks (`+N-OBJ-UNDO`, `+N-OBJ-MOD`, `+N-OBJ-CLOSED`) were absent, so '
  '`RESULT-RULE-V35` step 2 returned UNKNOWN. The frozen row, the proposal text and `RESULT-RULE-V35` are **not** amended; only this derived rendering carries the clarification, and the JSON '
  'keeps the frozen shorthand in `unknownThroughFrozenShorthand`.')
w('')
w('## 6. Structural falsification campaign — EXECUTED, STOPPED AFTER ROW 4, CLOSED')
w('')
w('Status: %s' % campaign['status'])
w('')
w('Rules as authorized:')
w('')
for r in campaign['rules']:
    w('- %s' % r)
w('')
w('Stop rule applied: %s.' % campaign['stopRule'])
w('')
w('Order rule: %s' % campaign['orderRule'])
w('')
w('| # | ProbeId | Anchor | Chain | Surface | Verifiers | Campaign state | executed | governing | structuralUnknown | terminalForHostPass | campaignStoppedBeforeExecution | Structural route |')
w('|---|---|---|---|---|---|---|---|---|---|---|---|---|')
for r in run_order:
    w('| %d | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s |' % (
        r['order'], r['probeId'], r['anchor'], r['schedulerChain'], r['surface'], ','.join(r['verifiers']), r['state'], 'yes' if r['executed'] else 'no', 'yes' if r['governing'] else 'no',
        str(r['structuralUnknown']).lower(), str(r['terminalForHostPass']).lower(), str(r['campaignStoppedBeforeExecution']).lower(),
        structural[r['probeId']]['unknownThroughCanonical'].replace('|', '/')))
w('')
w('Rows 5-14 are **not executed and not authorized**; they are not FAIL, not UNKNOWN and not deferred.')
w('')
w('### 6.0 Campaign results (canonical)')
w('')
w('| # | ProbeId | Execution | PID | Engine result | Raw scratch-runner label | Canonical status | Fresh reads | BOOT-01 / CLN-BASE | Evidence |')
w('|---|---|---|---|---|---|---|---|---|---|')
for e in [x for x in executions if x['campaignExecution']]:
    ev = json.load(open(os.path.join(ROOT, e['evidence']), encoding='utf-8'))
    w('| %d | %s | %s | %s | %s | %s | %s | %s | %s / %s | `%s` (`%s`) |' % (
        ev['campaignIndex'], e['probeId'], e['executionId'], e['processId'], e['engineResult'], ev['runnerRawLabel'], ev['canonicalResult'].split(' (')[0],
        ','.join(v['verifier'] for v in e['verifierObservations'] if v['phase'] == 'FRESH'), e['boot01DeclaredResolved'], e['clnBaseErased'], e['evidence'], e['evidenceSha256'][:16]))
w('')
w('The raw scratch-runner labels are non-authoritative tooling classifications and are preserved unchanged in the evidence: row 3 `INVALID` was a runner defect (CLN-BASE erased 9 = 8 + `F-REF-C` appended by MUT-SM), '
  'row 4 `INVALID` treated `EvidenceComplete=false` as invalid, whereas a structural UNKNOWN is a valid executed result (decisions 198).')
w('')
w('### 6.1 Campaign baseline identity (from the published zero-probe smoke evidence; package verified read-only)')
w('')
w('| Field | Value |')
w('|---|---|')
for k, label in (('campaignSourceSha', 'CAMPAIGN_SOURCE_SHA'), ('packagePath', 'PACKAGE_PATH'), ('packageZipSha256', 'PACKAGE_ZIP_SHA256'), ('nativeArxSha256', 'NATIVE (R-NATIVE-ARX) SHA-256'),
                 ('payloadArxSha256', 'PAYLOAD (R-PAYLOAD-ARX) SHA-256'), ('managedObserverDllSha256', 'MANAGED (R-MANAGED-OBSERVER) SHA-256'), ('buildTupleHash', 'BUILD_TUPLE'),
                 ('freezePackageHash', 'V35-A3 freeze package hash'), ('acadExeSha256', 'acad.exe SHA-256'), ('sha256Sums', 'SHA256SUMS'), ('smokePreRunTupleHash', 'zero-probe smoke R3_PRE_RUN_TUPLE_HASH')):
    w('| %s | `%s` |' % (label, baseline[k]))
w('')
w('%s' % baseline['note'])
w('')
w('## 7. Managed rows')
w('')
w('Required for CTDA_HOST_PASS(V35-A3): exactly 18 — `%s`. Fixture `HF-V31-1`. Every required row is NOT-EXECUTED. Executable authority is MISSING but presently considered IMPLEMENTABLE (decisions 197.3); '
  'its purpose for this predicate is MOOT because CTDA_HOST_PASS is terminal FALSE (decisions 198). This is not an inability determination.' % '`, `'.join(REQUIRED18))
w('')
w('| ProbeId | Predicate status | Class | Trigger / scheduler / direct action | Surface | Tags | Would contribute if a governing PASS |')
w('|---|---|---|---|---|---|---|')
for m in managed:
    trig = m['primaryTriggerEventId'] if m['primaryTriggerEventId'] != 'NONE' else (m['schedulerId'] if m['schedulerId'] != 'NONE' else m['directActionId'])
    wc = []
    if m['wouldContribute']['DA-P8']:
        wc.append('DA-P8 (+NOD surface for DA-P7)')
    if m['wouldContribute']['DA-P10']:
        wc.append('DA-P10')
    w('| %s | %s | %s | %s | %s | %s | %s |' % (m['probeId'], m['predicateStatus'], m['classification'], trig, m['surface'], ','.join(m['daPHp']), '; '.join(wc) or '—'))
w('')
w('## 8. Deferred and campaign-stopped APPCTX probes')
w('')
by = native_by_id
for p in ('16A-S', '13A-SM'):
    n = by[p]
    w('- **%s** — family `%s`, chain `%s`, anchor `%s`, surface `%s`, verifiers `%s`, threats `%s`, tags `%s`. %s' % (
        p, n['family'], n['schedulerChain'], n['anchor'], n['surface'], ','.join(n['verifiers']), ','.join(n['threats']), ','.join(n['daPHp']), n['reasonIfNotGoverning']))
w('')
w('## 9. Native matrix (100 rows)')
w('')
w('Fixture identity for every native row: `fixture-v35 / FEC-V35-1`. "Anchor" is the row\'s `ExecutionContextId`; "family" is its `HeaderAuthority`. "S*" marks the 15 structural-UNKNOWN candidates. '
  '"D-4" is the A3 anchor characterization. Results are never transferred between families or anchors.')
w('')
w('| ProbeId | S* | Class | Family | Chain | Anchor | Surface | Tuple (pkg / source / freeze) | Carried fwd | Result | D-4 | FRESH surfaces | Non-governing exec | Campaign |')
w('|---|---|---|---|---|---|---|---|---|---|---|---|---|---|')
for n in native:
    tup = n.get('executionTuple')
    tp = '`%s` / `%s` / %s' % (tup['package'], (tup['sourceSha'] or '')[:8], tup['freezeRevisionOfPackage']) if tup else '—'
    camp = ('row %d: %s' % (n['campaignRow'], n['result'])) if n['campaignRow'] and n['executed'] else ('row %d: STOPPED BEFORE EXECUTION' % n['campaignRow'] if n['campaignRow'] else '—')
    w('| %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s |' % (
        n['probeId'], 'S*' if n['structuralUnknownCandidate'] else '', n['classification'], n['family'], n['schedulerChain'], n['anchor'], n['surface'], tp,
        'yes' if n.get('carriedForward') else ('no' if tup else '—'), n['result'] or '—', n.get('d4Anchor') or '—', ','.join(n['contributesTo']['DA-P7']) or '—',
        ','.join(n['nonGoverningExecutions']) or '—', camp))
w('')
w('## 10. Inputs (SHA-256 of LF-normalized content)')
w('')
for p, h in doc['inputsSha256OfLfNormalizedContent'].items():
    w('- `%s` — `%s`' % (p, h))
w('')
with open(os.path.join(ROOT, OUT_MD), 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(o))
print('wrote', OUT_JSON, OUT_MD)
print(json.dumps(counts['requiredRows118']), STATE)
