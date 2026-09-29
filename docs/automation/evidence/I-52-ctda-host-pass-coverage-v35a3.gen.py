#!/usr/bin/env python3
"""
Generator of the CTDA_HOST_PASS(V35-A3) coverage assessment (I-52).

Documentation / analysis only. It reads the canonical catalogs and the published evidence of the repository and writes:
  docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.json   (machine-readable, authoritative)
  docs/initiatives/I-52-ctda-host-pass-coverage-v35a3.md             (reviewable, generated from the same data)

Run from the repository root:  python docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.gen.py
Nothing is inferred or transferred between rows, families or anchors: a row is PASS-GOVERNED only if a governing
execution of exactly that ProbeId produced PASS.
"""
import collections
import hashlib
import json
import os
import re
import sys

ROOT = os.getcwd()
NPM = 'docs/initiatives/I-52-native-probe-matrix-v35.md'
HEC = 'docs/initiatives/I-52-host-event-catalog-v27-correction.md'
HECT = 'docs/initiatives/I-52-host-event-catalog-v27.md'
CAT = 'docs/initiatives/I-52-execution-catalog-v35.json'
FIX = 'docs/initiatives/I-52-fixture-execution-contract-v35.md'
EV = 'docs/automation/evidence/'
OUT_JSON = EV + 'I-52-ctda-host-pass-coverage-v35a3.json'
OUT_MD = 'docs/initiatives/I-52-ctda-host-pass-coverage-v35a3.md'


def sha(path):
    return hashlib.sha256(open(os.path.join(ROOT, path), 'rb').read()).hexdigest().upper()


def read(path):
    return open(os.path.join(ROOT, path), encoding='utf-8').read().replace('\r\n', '\n')


# ---------------------------------------------------------------------------------------------- native rows (NPM-V35 section 5)
lines = read(NPM).split('\n')
schema = [x.strip().rstrip('.') for x in [l for l in lines if l.startswith('ProbeId; PrimaryAuthorityId')][0].split(';')]
start = [i for i, l in enumerate(lines) if l.startswith('## 5. Full normative matrix')][0]
end = [i for i, l in enumerate(lines) if l.startswith('## 6. Closure')][0]
raw_rows = [l.split(';') for l in lines[start + 1:end] if l.strip() and not l.startswith('```')]
assert len(schema) == 36 and len(raw_rows) == 100 and all(len(r) == 36 for r in raw_rows), 'NPM-V35 shape'
ix = {n: i for i, n in enumerate(schema)}
native = []
for r in raw_rows:
    native.append({
        'probeId': r[0],
        'catalog': 'NPM-V35',
        'family': r[ix['HeaderAuthority']],
        'schedulerChain': r[ix['SchedulerChainId']],
        'anchor': r[ix['ExecutionContextId']],
        'driver': r[ix['DriverId']],
        'primaryAuthority': r[ix['PrimaryAuthorityId']],
        'surface': r[ix['Surface']],
        'verifiers': r[ix['VerifierIds']].split(','),
        'threats': r[ix['Threats']].split(','),
        'daPHp': r[ix['DA-P/H-P']].split(','),
        'fixtureIdentity': 'fixture-v35 / FEC-V35-1 (native)',
    })
assert len({n['probeId'] for n in native}) == 100

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
                'probeId': c[0],
                'catalog': 'HEC-V27-C1',
                'primaryTriggerEventId': c[1],
                'schedulerId': c[2],
                'directActionId': c[3],
                'threats': [x.strip() for x in c[6].split(',')],
                'surface': c[7].split('/')[0].strip(),
                'verifiers': [x.strip() for x in c[8].split(';')],
                'daPHp': [x.strip() for x in c[9].split(',')],
                'fixtureIdentity': 'HF-V31-1 (managed; F-NOD / F-SRC are managed-fixture identities)',
            })
assert len(managed) == 27, len(managed)
# The literal definition speaks of "18 managed HEC-V27-C1 rows": probes 01..15 (with 09C/09E and 10S/10M/10SM split) = 18.
# The nine 16{S,M,SM}-{SEND,CONTEXT,IDLE} scheduler rows are also in HEC-V27-C1 section 2 and are kept apart, not silently dropped.
literal18 = [m for m in managed if not m['probeId'].startswith('16')]
assert len(literal18) == 18, len(literal18)
for m in managed:
    m['inLiteral18'] = not m['probeId'].startswith('16')

# ---------------------------------------------------------------------------------------------- executions (11)
EXEC = [
    # id, probe, package, evidence file, section, engine result, governing?, reason / ruling
    ('E01', '09N-B', '6e445fa8', 'I-52-r3-canary-09N-B.json', '176', 'PASS-T (engine)', False,
     "NOT VALID: the operator confirmed that the save-changes dialog appeared and pressed 'Don't save' by hand (Coordinator ruling, section 181)"),
    ('E02', '02NDBMOD-S', 'c4037098', 'I-52-r3-canary-02NDBMOD-S.json', '178', 'PASS-S (engine)', False,
     'NOT ACCEPTED: D-2 (corrupt CommandIdentity, use-after-free in I52Ctda_TokenSet)'),
    ('E03', '02NDBMOD-S', '2887d6c5', 'I-52-r3-canary-02NDBMOD-S-rerun.json', '180', 'UNKNOWN', False,
     'NOT VALID: D-3 led to an interactive exit and Save changed the scratch DWG (section 181)'),
    ('E04', '09N-B', 'e864a093', 'I-52-r3-canary-09N-B-restart.json', '183-184', 'PASS-T', True,
     'VALID PASS-T (Coordinator ruling on H-1, section 184)'),
    ('E05', '02NDBMOD-S', 'e864a093', 'I-52-r3-canary-02NDBMOD-S-restart.json', '184', 'PASS-S', True, 'VALID PASS-S'),
    ('E06', '02NO-S', 'e864a093', 'I-52-r3-canary-02NO-S-restart.json', '185', 'PASS-S', True, 'VALID PASS-S'),
    ('E07', '16N-S', 'e864a093', 'I-52-r3-canary-16N-S-restart.json', '186', 'UNKNOWN', False,
     'NOT VALID: operator interaction on the host (independent of the engine UNKNOWN)'),
    ('E08', '16N-S', 'e864a093', 'I-52-r3-canary-16N-S-rerun.json', '186', 'UNKNOWN', False,
     'Historical A2 UNKNOWN (UNK-MARKER-BINDING): D-4 was a V35 contract defect (section 187); not re-graded (section 188)'),
    ('E09', '16N-S', 'afa65bc0', 'I-52-r3-canary-16N-S-v35a3.json', '191', 'PASS-S', True, 'VALID PASS-S under V35-A3 (D-4 SEND anchor)'),
    ('E10', '10N-S', 'afa65bc0', 'I-52-r3-canary-10N-S-v35a3.json', '192', 'PASS-S', True, 'VALID PASS-S under V35-A3 (D-4 FIXTURE anchor)'),
    ('E11', '16C-S', 'afa65bc0', 'I-52-r3-canary-16C-S-v35a3.json', '193', 'PASS-S', True, 'VALID PASS-S under V35-A3 (D-4 CMDCTX anchor)'),
]


def g(d, *ks):
    for k in ks:
        if isinstance(d, dict) and k in d:
            d = d[k]
        else:
            return None
    return d


executions = []
for eid, probe, pkg, fname, sec, res, gov, why in EXEC:
    d = json.load(open(os.path.join(ROOT, EV + fname), encoding='utf-8'))
    assert d.get('probeId') == probe, (fname, d.get('probeId'))
    pk = d.get('package')
    pkid = pk.get('id') if isinstance(pk, dict) else None
    tup = d.get('r3PreRunTuple') if isinstance(d.get('r3PreRunTuple'), dict) else {}
    fz = tup.get('V35_FREEZE_PACKAGE_HASH')
    revision = 'V35-A3' if fz and fz.startswith('D9FD41B4') else ('V35-A2' if fz and fz.startswith('43DCE809') else 'V35-A2 era (no tuple record in the evidence file)')
    executions.append({
        'executionId': eid, 'probeId': probe, 'package': pkid or pkg, 'decisionsSection': sec,
        'engineResult': res, 'governing': gov, 'reason': why,
        'freezeRevisionOfPackage': revision, 'freezePackageHash': fz,
        'rowApprovalHash': d.get('rowApprovalHash'), 'r3HostExecutionTupleHash': d.get('r3HostExecutionTupleHash'),
        'r3PreRunTupleHash': d.get('R3_PRE_RUN_TUPLE_HASH'),
        'processId': g(d, 'processRun', 'process', 'ProcessId'), 'startedUtc': g(d, 'processRun', 'process', 'StartedAtUtc'),
        'evidence': EV + fname, 'evidenceSha256': sha(EV + fname),
    })
assert len(executions) == 11 and sum(1 for e in executions if e['governing']) == 6

# governing A3 validity per governed ProbeId
A3_NATIVE = {'16N-S', '10N-S', '16C-S'}
CARRY = {'09N-B', '02NDBMOD-S', '02NO-S'}  # A2-package results carried forward by decisions section 188 (ROW_HASH_DELTA 0/100)
D4 = {'16N-S': 'SEND', '10N-S': 'FIXTURE', '16C-S': 'CMDCTX'}
gov_by_probe = {e['probeId']: e for e in executions if e['governing']}
nong_by_probe = collections.defaultdict(list)
for e in executions:
    if not e['governing']:
        nong_by_probe[e['probeId']].append(e['executionId'])

DEFERRED = {'16A-S': 'APPCTX anchor characterization (D-4 text unchanged for APPCTX); deferred in decisions sections 194/195 and HANDOFF',
            '13A-SM': 'APPCTX synchronous chain (lock/T from another context); deferred in decisions sections 194/195 and HANDOFF'}

SURF_OF_VER = {'VER-S': 'S', 'VER-M': 'M', 'VER-SM': 'SM', 'VER-ALL': 'S+M+SM', 'VER-NOD': 'NOD'}

for n in native:
    p = n['probeId']
    n['nonGoverningExecutions'] = nong_by_probe.get(p, [])
    if p in gov_by_probe:
        e = gov_by_probe[p]
        n['classification'] = 'PASS-GOVERNED'
        n['result'] = e['engineResult']
        n['governingExecution'] = e['executionId']
        n['executionTuple'] = {'package': e['package'], 'freezeRevisionOfPackage': e['freezeRevisionOfPackage'],
                               'freezePackageHash': e['freezePackageHash'], 'r3HostExecutionTupleHash': e['r3HostExecutionTupleHash'],
                               'rowApprovalHash': e['rowApprovalHash']}
        n['a3Validity'] = ('native V35-A3 execution' if p in A3_NATIVE else
                           'carried forward from a V35-A2 package by decisions section 188 (row hash unchanged); NOT re-executed under the A3 tuple')
        n['d4Anchor'] = D4.get(p)
        n['reasonIfNotGoverning'] = None
        read_s = sorted({SURF_OF_VER[v] for v in n['verifiers'] if v in SURF_OF_VER})
        n['surfacesReadInPass'] = read_s
        n['evidenceRefs'] = [e['evidence']]
    elif p in DEFERRED:
        n['classification'] = 'DEFERRED'
        n['result'] = None
        n['reasonIfNotGoverning'] = DEFERRED[p]
        n['surfacesReadInPass'] = []
        n['evidenceRefs'] = []
    else:
        n['classification'] = 'NOT-EXECUTED'
        n['result'] = None
        n['reasonIfNotGoverning'] = 'no governing or non-governing execution of this ProbeId exists'
        n['surfacesReadInPass'] = []
        n['evidenceRefs'] = []
    n['contributesTo'] = {'DA-P7': n['surfacesReadInPass'] if n['classification'] == 'PASS-GOVERNED' else [], 'DA-P8': False, 'DA-P10': False}

for m in managed:
    m['classification'] = 'NOT-EXECUTED'
    m['result'] = None
    m['reasonIfNotGoverning'] = ('no executable contract or instrument exists for this managed row (the CT-DA harness only hosts RR-MANAGED-CMD, '
                                 'the Document.CommandEnded observer of row 09E, inside native runs)')
    m['nonGoverningExecutions'] = []
    m['evidenceRefs'] = []
    m['surfacesReadInPass'] = []
    m['contributesTo'] = {'DA-P7': [], 'DA-P8': False, 'DA-P10': False}
    # what the row WOULD contribute if it were a governing PASS (definition text): row 11 -> DA-P8; rows 14 and 15 -> DA-P10
    m['wouldContribute'] = {'DA-P8': m['probeId'] == '11', 'DA-P10': m['probeId'] in ('14', '15'),
                            'DA-P7-surface': 'NOD' if m['probeId'] == '11' else None}

# ---------------------------------------------------------------------------------------------- counts
nat_counts = collections.Counter(n['classification'] for n in native)
man18_counts = collections.Counter(m['classification'] for m in literal18)
man_all_counts = collections.Counter(m['classification'] for m in managed)
counts = {
    'nativeRowsTotal': 100, 'managedRowsTotal(literal18)': 18, 'managedRowsInHEC-V27-C1-section2': 27,
    'native': dict(nat_counts), 'managed18': dict(man18_counts), 'managedAll27': dict(man_all_counts),
    'combined118': {
        'PASS-GOVERNED': nat_counts['PASS-GOVERNED'] + man18_counts['PASS-GOVERNED'],
        'FAIL-GOVERNED': 0, 'UNKNOWN-GOVERNED': 0,
        'EXECUTED-NON-GOVERNING (rows with ONLY non-governing executions)': 0,
        'NOT-EXECUTED': nat_counts['NOT-EXECUTED'] + man18_counts['NOT-EXECUTED'],
        'DEFERRED': nat_counts['DEFERRED'] + man18_counts['DEFERRED'],
    },
    'executions': {'total': 11, 'governing': 6, 'nonGoverning': 5, 'distinctValidProbeIds': 6},
}
assert counts['combined118']['PASS-GOVERNED'] + counts['combined118']['NOT-EXECUTED'] + counts['combined118']['DEFERRED'] == 118
assert nat_counts['PASS-GOVERNED'] == 6 and nat_counts['DEFERRED'] == 2 and nat_counts['NOT-EXECUTED'] == 92

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

surfaces_read = sorted({s for n in native if n['classification'] == 'PASS-GOVERNED' for s in n['surfacesReadInPass']})
dap7 = {'requiredSurfaces': ['S', 'M', 'SM', 'NOD'], 'readInAtLeastOnePass': surfaces_read,
        'missing': [s for s in ['S', 'M', 'SM', 'NOD'] if s not in surfaces_read],
        'note': ('09N-B is labelled Surface S/M in NPM-V35, but its VerifierIds are VER-T,VER-S: only S (and T) is read. '
                 'NOD is a surface of managed row 11 only (native rows have no NOD surface). Nothing is inferred from the S/M label.')}
dap8 = {'requiredBy': 'HEC row 11', 'rowClassification': [m['classification'] for m in managed if m['probeId'] == '11'][0], 'satisfied': False}
dap10 = {'requiredBy': 'HEC rows 14 and 15', 'rowClassifications': {m['probeId']: m['classification'] for m in managed if m['probeId'] in ('14', '15')}, 'satisfied': False}

# ---------------------------------------------------------------------------------------------- clauses
clauses = [
    {'clause': 1, 'name': 'fixture identity', 'status': 'INCOMPLETE',
     'why': ('Native fixture-v35 / FEC-V35-1: the six governing executions ran packages whose freeze/plan identity declares the frozen V35 fixture '
             '(A3: D9FD41B4.., A2: 43DCE809..), but no evidence record carries an explicit fixture-identity field. The managed fixture HF-V31-1 with F-NOD and '
             'F-SRC (identities that exist only in the managed HF/HEC fixture contract, not in the eight native PersistentFixtureIdentity entries) has never been '
             'instantiated in any governed execution.')},
    {'clause': 2, 'name': 'complete execution (100 native + 18 managed, exact governed identity/tuple)', 'status': 'INCOMPLETE',
     'why': 'native %d/100 executed under a governing result (92 not executed, 2 deferred); managed 0/18 executed. The three A2-package results are carried forward, not re-executed under the A3 tuple.' % nat_counts['PASS-GOVERNED']},
    {'clause': 3, 'name': 'result completeness (H-P1..H-P12, IN T1..T16, T14 OUT, every tagged row PASS)', 'status': 'INCOMPLETE',
     'why': 'H-P satisfied: %s; IN T satisfied: %s (rows tagged and all PASS). No FAIL among governing results.' % (
         [t for t in DAHP if hp[t]['satisfied']] or 'none', [t for t in tt if tt[t]['satisfied'] and t != 'T14'] or 'none')},
    {'clause': 4, 'name': 'observability (DA-P7 / DA-P8 / DA-P10)', 'status': 'INCOMPLETE',
     'why': 'DA-P7 surfaces read in a PASS: %s (missing %s); DA-P8 needs HEC row 11 (NOT-EXECUTED); DA-P10 needs HEC rows 14 and 15 (NOT-EXECUTED).' % (surfaces_read, dap7['missing'])},
    {'clause': 5, 'name': 'no FAIL / UNKNOWN anywhere in the governed matrix', 'status': 'INCOMPLETE',
     'why': ('0 FAIL-GOVERNED and 0 UNKNOWN-GOVERNED among the 6 governing results; nothing disqualifying is recorded. The absence cannot be established for '
             'the 110 rows not executed and 2 deferred. The two historical A2 UNKNOWN executions of 16N-S (E07, E08) are non-governing under A3 (sections 187/188).')},
]

state = {
    'CTDA_HOST_PASS(V35-A3)': 'NOT EVALUABLE',
    'meaning': ('TRUE requires all five clauses PASS. FALSE is produced by an executed governing FAIL or UNKNOWN (clause 5 violated; the literal text: a structural '
                'UNKNOWN is a valid executed result that forces FALSE). None exists. While any clause is INCOMPLETE and none is violated, the value is NOT EVALUABLE: it is '
                'NOT TRUE, and it is not (yet) FALSE.'),
    'notTrue': True,
    'openInterpretation': 'Q1 (see the assessment): if clause 2 is read as a state predicate over the current record, the conjunction is FALSE now; that reading is not adopted here.',
}

doc = {
    'schemaVersion': 1, 'initiative': 'I-52', 'kind': 'CTDA_HOST_PASS coverage assessment', 'definitionRevision': 'V35-A3',
    'mode': 'DOCUMENTATION / ANALYSIS ONLY (no AutoCAD, no probe executed, no source change)',
    'inputs': {p: sha(p) for p in (NPM, HEC, HECT, CAT, FIX)},
    'counts': counts, 'clauses': clauses, 'state': state,
    'daP7': dap7, 'daP8': dap8, 'daP10': dap10, 'hp': hp, 'threats': tt,
    'executions': executions,
    'nativeRows': native, 'managedRows': managed,
}
json.dump(doc, open(os.path.join(ROOT, OUT_JSON), 'w', encoding='utf-8', newline='\n'), indent=2, ensure_ascii=False)
open(os.path.join(ROOT, OUT_JSON), 'a', encoding='utf-8', newline='\n').write('\n')

# ---------------------------------------------------------------------------------------------- markdown
o = []
w = o.append
w('# I-52 — CTDA_HOST_PASS(V35-A3): coverage assessment')
w('')
w('> **Generated** from `%s` by `docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.gen.py`. Documentation / analysis only: no AutoCAD, no probe, no source change.' % OUT_JSON.split('/')[-1])
w('> The JSON is authoritative; this file is its reviewable rendering. Decision record: `docs/automation/decisions/I-52.md` section 196.')
w('')
w('## 1. Result')
w('')
w('```text')
w('CTDA_HOST_PASS(V35-A3) = NOT EVALUABLE   (not TRUE; no disqualifying FAIL/UNKNOWN recorded, so not FALSE)')
w('Clause 1 fixture identity                 INCOMPLETE')
w('Clause 2 complete execution               INCOMPLETE   native 6/100 governing (2 deferred, 92 not executed); managed 0/18')
w('Clause 3 result completeness              INCOMPLETE')
w('Clause 4 observability DA-P7/P8/P10       INCOMPLETE')
w('Clause 5 no FAIL/UNKNOWN                  INCOMPLETE   0 FAIL, 0 UNKNOWN among the 6 governing results')
w('```')
w('')
w('## 2. Counts')
w('')
w('| Class | Native (100) | Managed literal 18 | Combined 118 |')
w('|---|---|---|---|')
for k in ['PASS-GOVERNED', 'FAIL-GOVERNED', 'UNKNOWN-GOVERNED', 'EXECUTED-NON-GOVERNING', 'NOT-EXECUTED', 'DEFERRED']:
    nn = nat_counts.get(k, 0)
    mm = man18_counts.get(k, 0)
    w('| %s | %d | %d | %d |' % (k, nn, mm, nn + mm))
w('')
w('*EXECUTED-NON-GOVERNING* is counted per ROW: a row whose only executions are non-governing. There is none (every ProbeId with a non-governing execution also has a governing PASS). '
  'Per EXECUTION there are 11 governing-tuple attempts: 6 governing, 5 non-governing (section 3).')
w('')
w('HEC-V27-C1 section 2 holds **27** managed ProbeId rows. The literal definition counts **18** = probes 01..15 with 09C/09E and 10S/10M/10SM split. The nine '
  '`16{S,M,SM}-{SEND,CONTEXT,IDLE}` scheduler rows are also in the catalog and are shown separately (all NOT-EXECUTED); see open question Q2.')
w('')
w('## 3. The 11 governing-tuple executions')
w('')
w('| Id | ProbeId | Package | Freeze of package | R3 execution tuple | Engine result | Governing | Reason / ruling | Section |')
w('|---|---|---|---|---|---|---|---|---|')
for e in executions:
    w('| %s | %s | `%s` | %s | `%s` | %s | %s | %s | %s |' % (
        e['executionId'], e['probeId'], e['package'], e['freezeRevisionOfPackage'], (e['r3HostExecutionTupleHash'] or '—')[:10], e['engineResult'],
        'YES' if e['governing'] else 'no', e['reason'], e['decisionsSection']))
w('')
w('Evidence: `docs/automation/evidence/I-52-r3-canary-*.json` (paths and SHA-256 per execution in the JSON). The three A2-package results (E04-E06) are carried forward by decisions section 188 '
  '(`ROW_HASH_DELTA = 0/100`); they were not re-executed under the A3 tuple (open question Q3).')
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
w('- DA-P7: surfaces required S, M, SM, NOD. Read in at least one PASS: **%s**. Missing: **%s**. %s' % (', '.join(surfaces_read), ', '.join(dap7['missing']), dap7['note']))
w('- DA-P8: satisfied only by HEC row 11 (F-NOD): NOT-EXECUTED.')
w('- DA-P10: satisfied only by HEC rows 14 and 15 (F-SRC): NOT-EXECUTED.')
w('')
w('### 4.2 DA-P / H-P tag coverage (native + managed literal 18; every tagged row must PASS)')
w('')
w('| Tag | Rows tagged | of which native | of which managed | PASS-GOVERNED | Satisfied |')
w('|---|---|---|---|---|---|')
for t in DAHP:
    h = hp[t]
    w('| %s | %d | %d | %d | %d (%s) | %s |' % (t, h['rowsTagged'], h['nativeTagged'], h['managedTagged'], h['passGoverned'], ', '.join(h['passingProbeIds']) or '—', ('no (cross-cutting property, no row carries it: see 4.1)' if t == 'P7' else ('YES' if h['satisfied'] else 'no'))))
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
w('## 5. Deferred probes (16A-S, 13A-SM)')
w('')
by = {n['probeId']: n for n in native}
for p in ('16A-S', '13A-SM'):
    n = by[p]
    w('- **%s** — family `%s`, chain `%s`, anchor `%s`, surface `%s`, verifiers `%s`, threats `%s`, tags `%s`. %s' % (
        p, n['family'], n['schedulerChain'], n['anchor'], n['surface'], ','.join(n['verifiers']), ','.join(n['threats']), ','.join(n['daPHp']), n['reasonIfNotGoverning']))
w('')
w('## 6. Native matrix (100 rows)')
w('')
w('Fixture identity for every native row: `fixture-v35 / FEC-V35-1`. "Anchor" is the row\'s `ExecutionContextId`; "family" is its `HeaderAuthority`. '
  '"D-4" is the A3 anchor characterization (SEND / FIXTURE / CMDCTX). Results are never transferred between families or anchors.')
w('')
w('| ProbeId | Class | Family | Chain | Anchor | Surface | Tuple (pkg / freeze / exec) | A3 validity | Result | D-4 | DA-P7 read | Non-governing exec |')
w('|---|---|---|---|---|---|---|---|---|---|---|---|')
for n in native:
    tup = n.get('executionTuple')
    tp = '`%s` / %s / `%s`' % (tup['package'], tup['freezeRevisionOfPackage'].split(' ')[0], (tup['r3HostExecutionTupleHash'] or '')[:8]) if tup else '—'
    w('| %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s | %s |' % (
        n['probeId'], n['classification'], n['family'], n['schedulerChain'], n['anchor'], n['surface'], tp,
        ('A3' if n.get('a3Validity', '').startswith('native') else ('carry-fwd §188' if n.get('a3Validity') else '—')),
        n['result'] or '—', n.get('d4Anchor') or '—', ','.join(n['contributesTo']['DA-P7']) or '—', ','.join(n['nonGoverningExecutions']) or '—'))
w('')
w('## 7. Managed matrix (HEC-V27-C1 section 2; fixture `HF-V31-1`)')
w('')
w('| ProbeId | In literal 18 | Class | Trigger / scheduler / direct action | Surface | Tags | Would contribute if a governing PASS |')
w('|---|---|---|---|---|---|---|')
for m in managed:
    trig = m['primaryTriggerEventId'] if m['primaryTriggerEventId'] != 'NONE' else (m['schedulerId'] if m['schedulerId'] != 'NONE' else m['directActionId'])
    wc = []
    if m['wouldContribute']['DA-P8']:
        wc.append('DA-P8 (+NOD surface for DA-P7)')
    if m['wouldContribute']['DA-P10']:
        wc.append('DA-P10')
    w('| %s | %s | %s | %s | %s | %s | %s |' % (m['probeId'], 'yes' if m['inLiteral18'] else 'no (16-family scheduler row)', m['classification'], trig, m['surface'], ','.join(m['daPHp']), '; '.join(wc) or '—'))
w('')
w('Every managed row: NOT-EXECUTED. %s' % managed[0]['reasonIfNotGoverning'])
w('')
w('## 8. Inputs (SHA-256)')
w('')
for p, h in doc['inputs'].items():
    w('- `%s` — `%s`' % (p, h))
w('')
open(os.path.join(ROOT, OUT_MD), 'w', encoding='utf-8', newline='\n').write('\n'.join(o))
print('wrote', OUT_JSON, OUT_MD)
print(json.dumps(counts['combined118']), json.dumps(state['CTDA_HOST_PASS(V35-A3)']))
