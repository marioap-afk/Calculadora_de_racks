"""Mechanical checks of BA-04 V4 (scenario catalog) and BA-02b V4 (coverage matrix), I-52 CT-21D authority baseline.

Usage (from any directory):  python I-52-ct21d-ba04-v4-checks.py <output.json>

Reads only repository files (paths relative to the repository root, found from this file's location):
  docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json and .md
  docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v4.md
  docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md   (registry and group table, verbatim V2.2 PA-1.2 / PA-1.3)
  docs/initiatives/I-52-ct21d-authority-contract-v2.1.md, -v2.2.md      (verbatim rows of V2.1 31 and V2.2 PA-1.4)
  docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v4.json          (BA-05 V4 vector ids cited by Q-FPSPEC-VECTORS and Q-SIGNED-ZERO)
Recomputes every derived value from the JSON and the registry independently of the generator, compares it with what the
artifacts state, and writes a JSON report. Exit code 0 when every check passes, 1 otherwise. No check is weakened to pass:
a failing check lists its offending items.

Correction pass of round V4: the repetition labels are derived by the rule table and the N_strict of BA-10 V4 section 2.2
(no ordering of N_A and N_B); the dry-run rule is the one rule of BA-09 V4 section 4; the pending rulings AQ-V4-01, AQ-V4-03
and K-6 (AQ-V4-07) are checked where they apply; every registry key of EXPECTED_CONTROL_RESULTS is exercised, NOT_DECLARED
or RECORDED_NOT_ENFORCED of the scenario's own mode; the vector ids of Q-FPSPEC-VECTORS and Q-SIGNED-ZERO are checked
against the BA-05 V4 vectors file.
"""
import hashlib
import io
import json
import os
import re
import sys
from collections import Counter, OrderedDict

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..'))
INIT = os.path.join(ROOT, 'docs', 'initiatives')
P_JSON = 'docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json'
P_MD = 'docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.md'
P_COV = 'docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v4.md'
P_REG = 'docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md'
P_V21 = 'docs/initiatives/I-52-ct21d-authority-contract-v2.1.md'
P_V22 = 'docs/initiatives/I-52-ct21d-authority-contract-v2.2.md'
P_FPV = 'docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v4.json'
O = OrderedDict
AQ01, AQ03, K6 = 'AQ-V4-01', 'AQ-V4-03', 'K-6 (AQ-V4-07, Q-HDM-1)'
AR301_RULED_PREFIXES = ('E4-LEARN-R1-', 'LK-', 'HDM-CHAR-', 'WU-DRY-')   # AR3-01 (E4-LEARN-R1-*, LK-*), AR3-11 (HDM-CHAR-*), AR3-02 (WU-DRY-*)


def read(rel):
    return io.open(os.path.join(ROOT, rel), encoding='utf-8', newline='').read().replace('\r\n', '\n')


def sha(txt):
    return hashlib.sha256(txt.encode('utf-8')).hexdigest()


TXT = {p: read(p) for p in (P_JSON, P_MD, P_COV, P_REG, P_V21, P_V22, P_FPV)}
CAT = json.loads(TXT[P_JSON], object_pairs_hook=OrderedDict)
FPV = json.loads(TXT[P_FPV], object_pairs_hook=OrderedDict)
SC = CAT['SCENARIOS']
BY = {s['SCENARIO_ID']: s for s in SC}
RESULTS = O()


def check(name, offenders, detail=''):
    offenders = list(offenders)
    RESULTS[name] = O([('result', 'PASS' if not offenders else 'FAIL'), ('offenders', offenders[:50]), ('offender_count', len(offenders)), ('detail', detail)])


def table_after(text, heading, stop=None):
    """Rows (lists of cells) of the first markdown table after `heading` (and before `stop`)."""
    part = text.split(heading, 1)[1]
    if stop:
        part = part.split(stop, 1)[0]
    rows, started = [], False
    for line in part.split('\n'):
        if line.startswith('|'):
            started = True
            if re.match(r'^\|(-+\|)+$', line.replace(' ', '')):
                continue
            rows.append([c.strip() for c in line.strip()[1:-1].split('|')])
        elif started:
            break
    return rows[1:]  # without the header


def ids_in(cell):
    return re.findall(r'`([^`]+)`', cell)


# ------------------------------------------------------------------------------------------------------------------
# Registry and group table from BA-02a V3 (the authority), compared with the catalog's REGISTRY_MAP and GROUP_TABLE
# ------------------------------------------------------------------------------------------------------------------
KEY_PREFIX = O([('S1', 'S-1 staged reads'), ('S2', 'S-2 pass 1'), ('S3', 'S-3 pass 2'), ('E08', 'E-08 read-set'), ('S5', 'S-5 abort verification'),
                ('S5REC', 'S-5 registry component'), ('S5MAN', 'S-5 manifest component'), ('S5CLO', 'S-5 closure component'), ('E01', 'E-01 '),
                ('E02', 'E-02 '), ('E03', 'E-03 '), ('E04', 'E-04 '), ('E05', 'E-05 '), ('E06', 'E-06 '), ('E07', 'E-07 '), ('E09', 'E-09 '), ('E10', 'E-10 '),
                ('E11M', 'E-11M '), ('E12A', 'E-12 phase A'), ('E12C', 'E-12 phase C'), ('E12D', 'E-12 phase D'), ('E12DET', 'E-12 determinism'),
                ('E13', 'E-13 '), ('E14', 'E-14 '), ('E15', 'E-15 '), ('A1', 'A-1 '), ('A2', 'A-2 '), ('WUM', '`WARMUP_MEMBERSHIP`'),
                ('LIBF', '`LIBRARY_FRESHNESS`'), ('CLONE', '`CLONE_POLICY_VERIFIED`')])
reg_rows = table_after(TXT[P_REG], '## 2. Control class registry', '## 3.')
REG = O()
for key, pre in KEY_PREFIX.items():
    hit = [r for r in reg_rows if r[0].startswith(pre)]
    assert len(hit) == 1, (key, hit)
    r = hit[0]
    REG[key] = O([('class', r[3]), ('affectedGroups', [g.strip() for g in r[7].split(',')]), ('inPredicate', r[5].startswith('YES'))])
grp_rows = table_after(TXT[P_REG], '## 3. Group table', '## 4.')
GT = O()
for r in grp_rows:
    kind = r[2].replace('*', '')
    kind = 'CONTROL_OUTSIDE_PREDICATE' if kind.startswith('CONTROL, outside') else kind
    for g in ids_in(r[0]):
        GT[g] = (r[1].replace('*', ''), kind)
EVID = [g for g, (c, k) in GT.items() if k == 'EVIDENCE']
CTRLG = [g for g, (c, k) in GT.items() if k.startswith('CONTROL')]
check('registry_map_equals_BA-02a', [k for k in REG if (CAT['REGISTRY_MAP'][k]['class'], CAT['REGISTRY_MAP'][k]['affectedGroups'], CAT['REGISTRY_MAP'][k]['inPredicate'])
                                     != (REG[k]['class'], REG[k]['affectedGroups'], REG[k]['inPredicate'])] + [k for k in CAT['REGISTRY_MAP'] if k not in REG],
      'class, AFFECTED_GROUPS and IN_PREDICATE of every key of the JSON REGISTRY_MAP equal BA-02a V3 section 2 (V2.2 PA-1.2)')
check('group_table_equals_BA-02a', [g for g in GT if g not in CAT['GROUP_TABLE'] or (CAT['GROUP_TABLE'][g]['class'], CAT['GROUP_TABLE'][g]['kind']) != GT[g]] +
      [g for g in CAT['GROUP_TABLE'] if g not in GT], 'the JSON GROUP_TABLE equals BA-02a V3 section 3 (V2.2 PA-1.3)')

# ------------------------------------------------------------------------------------------------------------------
# 1. identity and schema
# ------------------------------------------------------------------------------------------------------------------
ids = [s['SCENARIO_ID'] for s in SC]
check('scenario_ids_unique', [i for i, n in Counter(ids).items() if n > 1])
V4_FIELDS = ['SCENARIO_ID', 'SCENARIO_VERSION', 'GROUP', 'GROUPS_SERVED', 'CONTRACT_ASSIGNED_GROUPS', 'PURPOSE', 'MODE', 'RUN_GOVERNANCE', 'BUILD_UNDER_TEST',
             'RECORDED_NOT_ENFORCED', 'TUPLE_REQUIREMENTS', 'PLAN_OR_INPUT', 'FIXTURE', 'INPUT_STATUS', 'REACH', 'INJECTIONS', 'INJECTION_COORDINATE',
             'DESIGNED_STIMULUS', 'DELIBERATE_VIOLATION_FLAG', 'TARGET_NEGATIVE_CONTROL', 'EXPECTED_PRODUCT_OUTCOME', 'EXPECTED_CONTROL_RESULTS',
             'EXPECTED_INSTRUMENT_STATES', 'EXPECTED_GROUP_VERDICT', 'EXPECTED_EVENTS', 'EXPECTED_DETECTION_CHANNEL', 'EXPECTED_LOG_RECORD_SET',
             'EXPECTED_EVIDENCE_SET', 'EXPECTED_CLEANUP', 'RETRY_APPLICABILITY', 'REPETITION', 'CONTROLS_EXERCISED', 'CONTROL_CLASSES_EXERCISED',
             'EVIDENCE_GROUPS_DECLARED', 'EVM_VERSION', 'AUTHORITY_SOURCE_FOR_EXPECTED_RESULT', 'EXPECTED_PROCEDURAL_RESULT', 'CONDITIONAL_EXPECTATION_RULE',
             'PIN_SLOTS', 'BLOCKED_BY', 'STATUS', 'SEALED_AT', 'NOTE']
check('v4_schema_fields_present', [s['SCENARIO_ID'] + ': missing ' + ','.join(f for f in V4_FIELDS if f not in s) + ' extra ' + ','.join(f for f in s if f not in V4_FIELDS)
                                   for s in SC if set(s) != set(V4_FIELDS)], 'every scenario has exactly the V4 schema fields')
check('scenario_version_4', [s['SCENARIO_ID'] for s in SC if s['SCENARIO_VERSION'] != '4-DRAFT'])
check('authority_source_present', [s['SCENARIO_ID'] for s in SC if not s['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']], 'V2.1 26.1: mandatory')
check('build_under_test_values', [s['SCENARIO_ID'] for s in SC if s['BUILD_UNDER_TEST'] not in ('PRODUCT_BUILD', 'CHARACTERIZATION_BUILD', 'NONE')
                                  or (s['BUILD_UNDER_TEST'] == 'NONE') != (s['REACH'] == 'SYN')], 'NONE only for synthetic scenarios (REACH SYN)')

# ------------------------------------------------------------------------------------------------------------------
# 2. GROUPS_SERVED = derivation (BA-02b V4 section 1, AR2-08 as extended by AR3-06); CONTRACT_ASSIGNED_GROUPS separate
# ------------------------------------------------------------------------------------------------------------------
bad, bad_cag, bad_cls, bad_ev = [], [], [], []
for s in SC:
    der = []
    for c in s['CONTROLS_EXERCISED']:
        if c not in REG:
            bad.append(s['SCENARIO_ID'] + ': unknown control ' + c)
            continue
        for g in REG[c]['affectedGroups']:
            if g not in der:
                der.append(g)
    for g in s['EVIDENCE_GROUPS_DECLARED']:
        if g not in EVID:
            bad_ev.append(s['SCENARIO_ID'] + ': declared evidence group ' + g + ' is not of kind EVIDENCE')
        if g not in der:
            der.append(g)
    if s['GROUPS_SERVED'] != der:
        bad.append(s['SCENARIO_ID'] + ': ' + json.dumps(s['GROUPS_SERVED']) + ' != ' + json.dumps(der))
    for cg in s['CONTRACT_ASSIGNED_GROUPS']:
        if set(cg) != {'group', 'clause'} or not cg['clause'] or cg['group'] not in CTRLG or cg['group'] in s['GROUPS_SERVED']:
            bad_cag.append(s['SCENARIO_ID'] + ': ' + json.dumps(cg))
    cls = sorted({REG[c]['class'] for c in s['CONTROLS_EXERCISED'] if c in REG})
    if not s['CONTROLS_EXERCISED'] or s['EVIDENCE_GROUPS_DECLARED'] or s['CONTRACT_ASSIGNED_GROUPS']:
        cls = cls + ['NOT_A_CONTROL']
    if s['CONTROL_CLASSES_EXERCISED'] != cls:
        bad_cls.append(s['SCENARIO_ID'])
check('groups_served_equals_derivation', bad, 'GROUPS_SERVED == ordered union of AFFECTED_GROUPS(CONTROLS_EXERCISED) then EVIDENCE_GROUPS_DECLARED; nothing else')
check('evidence_groups_declared_are_evidence_kind', bad_ev)
check('contract_assigned_groups_separate', bad_cag, 'each entry is {group, clause}, a CONTROL group, never in GROUPS_SERVED (evidence only, never PASS-bearing; AR3-06)')
check('control_classes_derived', bad_cls)
check('group_is_served_or_contract_assigned', [s['SCENARIO_ID'] for s in SC if s['GROUP'] not in s['GROUPS_SERVED'] and s['GROUP'] not in
                                               [c['group'] for c in s['CONTRACT_ASSIGNED_GROUPS']] and s['GROUP'] != 'HDM'],
      'the primary GROUP is served or contract-assigned; HDM (not a PA-1.3 group) is the declared exception')
check('ar3_06_specific', [x for x, ok in [
    ('PW-CLONE-WRONG-TYPE exercises CLONE', 'CLONE' in BY['PW-CLONE-WRONG-TYPE']['CONTROLS_EXERCISED']),
    ('PR-CLOSURE-COMPLETENESS in Q with PR-REC contract-assigned', BY['PR-CLOSURE-COMPLETENESS']['GROUP'] == 'Q' and
     [c['group'] for c in BY['PR-CLOSURE-COMPLETENESS']['CONTRACT_ASSIGNED_GROUPS']] == ['PR-REC'] and 'PR-REC' not in BY['PR-CLOSURE-COMPLETENESS']['GROUPS_SERVED']),
    ('every X7 cell of writers A..G contract-assigns SC-B', all(any(c['group'] == 'SC-B' for c in BY['WR-%s-X7' % w]['CONTRACT_ASSIGNED_GROUPS']) for w in 'ABCDEFG')),
] if not ok])

# ------------------------------------------------------------------------------------------------------------------
# 3. AR3-01 mode
# ------------------------------------------------------------------------------------------------------------------
RNE = CAT['AR3_01_MODE']['RECORDED_NOT_ENFORCED']
check('ar3_01_set_exact', [] if RNE == ['E-03', 'E-04', 'E-12 phase A', 'E-12 phase C', 'S-1', 'S-2', 'S-3'] else ['AR3_01_MODE set: ' + json.dumps(RNE)])
must_mode = [i for i in ids if i.startswith(('E4-LEARN-R1-', 'LK-', 'HDM-CHAR-', 'WU-DRY-', 'EV-L-', 'Q-I14-'))]
mode = [s for s in SC if s['RECORDED_NOT_ENFORCED']]
check('ar3_01_named_scenarios_in_mode', [i for i in must_mode if not BY[i]['RECORDED_NOT_ENFORCED']])
bad = []
for s in mode:
    pins = s['PIN_SLOTS']
    if s['RECORDED_NOT_ENFORCED'] != RNE:
        bad.append(s['SCENARIO_ID'] + ': set differs')
    if s['BUILD_UNDER_TEST'] != 'CHARACTERIZATION_BUILD':
        bad.append(s['SCENARIO_ID'] + ': build')
    if s['EXPECTED_PRODUCT_OUTCOME'] != 'PROVISIONAL_NON_GOVERNING':
        bad.append(s['SCENARIO_ID'] + ': outcome')
    if s['EVM_VERSION'] != 'NONE' or any(p.startswith(('EVM_FROZEN', 'HOST_DEFAULT_MAP', 'E04_ADMITTED_SET')) for p in pins) or 'K-3' in s['BLOCKED_BY']:
        bad.append(s['SCENARIO_ID'] + ': depends on EVM, HOST_DEFAULT_MAP, E-04 or K-3')
    if 'TUPLE' not in s['TUPLE_REQUIREMENTS'] or 'CHARACTERIZATION_TUPLE' not in s['TUPLE_REQUIREMENTS']:
        bad.append(s['SCENARIO_ID'] + ': tuple does not bind the CHARACTERIZATION_BUILD')
check('ar3_01_mode_fields', bad, 'RECORDED_NOT_ENFORCED set, CHARACTERIZATION_BUILD, PROVISIONAL_NON_GOVERNING, CHARACTERIZATION_TUPLE; no EVM, HOST_DEFAULT_MAP, E-04 pin or K-3')
check('provisional_outcome_only_in_ar3_01_mode', [s['SCENARIO_ID'] for s in SC if s['EXPECTED_PRODUCT_OUTCOME'].startswith('PROVISIONAL') and not s['RECORDED_NOT_ENFORCED']])
# AR3-01 rules the mode only for E4-LEARN-R1-*, LK-* (and HDM-CHAR-*, WU-DRY-* by AR3-11, AR3-02); elsewhere it is a proposal pending AQ-V4-01
bad = []
proposal = [s for s in mode if not s['SCENARIO_ID'].startswith(AR301_RULED_PREFIXES)]
for s in proposal:
    if not (AQ01 in s['BLOCKED_BY'] and s['STATUS'] == 'DRAFT' and 'PROPOSAL' in s['NOTE'] and 'AQ-V4-01' in s['NOTE']
            and 'AQ-V4-01' in s['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']):
        bad.append(s['SCENARIO_ID'] + ': proposal not labelled or not BLOCKED_BY AQ-V4-01 / DRAFT')
    if s['RUN_GOVERNANCE'] != 'GOVERNING':
        bad.append(s['SCENARIO_ID'] + ': a non-governing scenario outside the ruled families')
if sorted(s['SCENARIO_ID'] for s in proposal) != sorted(i for i in ids if i.startswith(('EV-L-', 'Q-I14-'))):
    bad.append('the proposal set is not exactly EV-L-* and Q-I14-*: ' + json.dumps(sorted(s['SCENARIO_ID'] for s in proposal)))
for s in SC:
    if s['SCENARIO_ID'].startswith('EV-L-'):
        r = s['EXPECTED_CONTROL_RESULTS']
        grounded = r.get('E-12 phases A and C', '')
        prop = r.get('E-03, E-04, S-1, S-2, S-3', '')
        if not (grounded.startswith('RECORDED_NOT_ENFORCED, contract-grounded') and 'V2.1 27.2' in grounded and 'BA-06 V4 section 9' in grounded
                and 'PROPOSAL' in prop and 'AQ-V4-01' in prop and 'V2.1 27.2' in s['EXPECTED_EVENTS'].get('LEARNING', '')):
            bad.append(s['SCENARIO_ID'] + ': the contract-grounded part (V2.1 27.2) and the proposal are not stated apart')
sec11 = TXT[P_MD].split('### 1.1 ', 1)[1].split('### 1.2 ', 1)[0]
if 'does not change the phase policy of the governing runs' in sec11:
    bad.append('md 1.1 still holds the contradictory sentence')
if not all(x in sec11 for x in ('AQ-V4-01', '`E4-LEARN-R1-*`', '`LK-*`', '`HDM-CHAR-*`', '`WU-DRY-*`', 'V2.1 27.2', 'E-03, E-04, S-1, S-2, S-3')):
    bad.append('md 1.1 does not state the ruled families, the proposal (AQ-V4-01) and the set of EV-L-*')
def _strings(o):
    if isinstance(o, str):
        yield o
    elif isinstance(o, dict):
        for v in o.values():
            yield from _strings(v)
    elif isinstance(o, list):
        for v in o:
            yield from _strings(v)


for s in proposal:
    for k, v in s.items():
        if k in ('TUPLE_REQUIREMENTS', 'SCENARIO_ID'):
            continue
        for txt in _strings(v):
            if '(AR3-01)' in txt or '(AR3-01);' in txt or '(AR3-01):' in txt:
                bad.append(s['SCENARIO_ID'] + ': field ' + k + ' cites AR3-01 without the PROPOSAL / AQ-V4-01 label')
check('ar3_01_extension_is_a_labelled_proposal', bad,
      'every AR3-01-mode scenario outside E4-LEARN-R1-*, LK-*, HDM-CHAR-*, WU-DRY-* is governing, DRAFT, BLOCKED_BY AQ-V4-01 and labels its mode as a PROPOSAL; '
      'for EV-L-* the contract-grounded part (V2.1 27.2: E-12 phases A and C) and the proposal (E-03, E-04, S-1..S-3) are stated apart; md 1.1 states the scope')

# ------------------------------------------------------------------------------------------------------------------
# 4. WU-DRY (AR3-02, AR3-24)
# ------------------------------------------------------------------------------------------------------------------
WINDOW = ('P0', 'PL', 'PR', 'PW', 'PS', 'PV', 'CP')


def enters_window(s):
    """The one rule of BA-09 V4 section 4 (AR3-02): GOVERNING, MODE CHARACTERIZATION_RUN or VALIDATION, REACH in P0..CP;
    excluded: non-governing runs, learning runs and REACH = NONE (and SYN). No other qualifier."""
    return s['RUN_GOVERNANCE'] == 'GOVERNING' and s['MODE'] in ('CHARACTERIZATION_RUN', 'VALIDATION') and s['REACH'] in WINDOW


dry = {i[len('WU-DRY-'):]: BY[i] for i in ids if i.startswith('WU-DRY-')}
bad = []
for s in SC:
    if s['SCENARIO_ID'].startswith('WU-DRY-'):
        continue
    n = sum(1 for i in ids if i == 'WU-DRY-' + s['SCENARIO_ID'])
    if enters_window(s) and n != 1:
        bad.append(s['SCENARIO_ID'] + ': enters the governed window, dry runs = %d' % n)
    if not enters_window(s) and n != 0:
        bad.append(s['SCENARIO_ID'] + ': does not enter the governed window, dry runs = %d' % n)
for x, d in dry.items():
    if x not in BY:
        bad.append('WU-DRY-' + x + ': no source scenario')
        continue
    src = BY[x]
    exp_slots = ['FIXTURE_INSTANCE@' + src['FIXTURE']] + [p for p in src['PIN_SLOTS'] if p.startswith('FIXTURE_INSTANCE@L-VAR')]
    exp_block = [b for b in src['BLOCKED_BY'] if b in (AQ01, 'K-8')]
    if not (d['MODE'] == 'DRY_RUN' and d['RUN_GOVERNANCE'] == 'NON_GOVERNING_BY_DESIGN' and d['REACH'] == src['REACH'] and d['FIXTURE'] == src['FIXTURE']
            and src['FIXTURE'] != 'NONE' and d['PIN_SLOTS'] == exp_slots and d['REPETITION'] == 'DRY: WU_DRY_RUNS = N_B'
            and d['DELIBERATE_VIOLATION_FLAG'] == 'NO' and d['INJECTIONS'] == src['INJECTIONS'] and d['RECORDED_NOT_ENFORCED'] == RNE
            and d['BLOCKED_BY'] == exp_block):
        bad.append('WU-DRY-' + x + ': fields differ from the rule')
    if AQ01 in src['BLOCKED_BY'] and not ('withdrawn' in d['NOTE'] and 'AQ-V4-01' in d['NOTE']):
        bad.append('WU-DRY-' + x + ': the withdrawal condition of AQ-V4-01 is not stated')
for i in ids:
    if i.startswith('WU-DRY-WU-DRY-'):
        bad.append(i)
sec52 = TXT[P_MD].split('### 5.2 ', 1)[1].split('## 6. ', 1)[0]
if 'outside the AR3-01 mode' in sec52:
    bad.append('md 5.2 keeps the qualifier "outside the AR3-01 mode" that BA-09 V4 section 4 does not have')
if not all(x in sec52 for x in ('`RUN_GOVERNANCE = GOVERNING`', '`CHARACTERIZATION_RUN` or `VALIDATION`', '`REACH` in `P0`..`CP`', '`REACH = NONE`',
                                'its variant library (`FIXTURE_INSTANCE@L-VAR-*`)')):
    bad.append('md 5.2 does not state the one rule of BA-09 V4 section 4 and the PIN_SLOTS wording')
check('wu_dry_exactly_one_per_governing_window_scenario', bad,
      'the one rule of BA-09 V4 section 4: every GOVERNING scenario of MODE CHARACTERIZATION_RUN or VALIDATION with REACH in P0..CP has exactly one '
      'WU-DRY-<id>; no other scenario has one; a dry run carries only the FIXTURE_INSTANCE slots of its source (fixture and variant library), '
      'WU_DRY_RUNS = N_B, the source injections, the AR3-01 set and the source blockers AQ-V4-01 and K-8')
check('wu_dry_for_the_named_scenarios', [i for i in ('U-1', 'U-2', 'U-3', 'U-4', 'U-5', 'U-7', 'U-8', 'U-6b', 'S1-SAVE-REOPEN', 'PP-POSTCP', 'CL-CLEANUP-CHECK',
                                                      'CL-CLEAN-FXANNBLANK-FP', 'NC-CLONE-REPLACE', 'Q-I14-Y', 'Q-I14-X', 'Q-I14-SILENT-Y', 'Q-I14-SILENT-X')
                                         if 'WU-DRY-' + i not in BY],
      'the 11 evidence scenarios of BA04V3-02 / D-05, the two new governing scenarios and the four Q-I14-* vehicles (AR3-02 read literally)')
exp_res = [x for x in dry if BY[x]['TARGET_NEGATIVE_CONTROL'] in ('E03', 'E04', 'E12A', 'E12C', 'S1', 'S2', 'S3') and BY[x]['REACH'] in ('P0', 'PL', 'PV')]
m = re.search(r'\*\*Branches the dry runs do not take\.\*\*.*? This concerns (.*?) \(AQ-V4-02', TXT[P_MD])
listed = ids_in(m.group(1)) if m else []
check('dry_run_residual_branches_listed', sorted(set(listed) ^ set(exp_res)) + [x for x in exp_res if 'does not take that branch' not in BY['WU-DRY-' + x]['PLAN_OR_INPUT']],
      'the sources whose declared refusal or abort comes from a control of the RECORDED_NOT_ENFORCED set are listed in section 5.2 and in their dry run (BA-09 V4 residual)')

# ------------------------------------------------------------------------------------------------------------------
# 5. flags, targets and designed stimuli (AR3-05)
# ------------------------------------------------------------------------------------------------------------------
bad = []
for s in SC:
    f, t, ds = s['DELIBERATE_VIOLATION_FLAG'], s['TARGET_NEGATIVE_CONTROL'], s['DESIGNED_STIMULUS']
    if ds != 'NONE' and f != 'NO':
        bad.append(s['SCENARIO_ID'] + ': designed stimulus with flag ' + f)
    if f == 'YES' and not (t in REG or re.match(r'^INSTRUMENT:I-\d\d$', t)):
        bad.append(s['SCENARIO_ID'] + ': YES without a valid target (' + t + ')')
    if f == 'NO' and t != 'NONE':
        bad.append(s['SCENARIO_ID'] + ': NO with target ' + t)
    if f not in ('YES', 'NO'):
        bad.append(s['SCENARIO_ID'] + ': flag ' + f)
check('flags_targets_designed_stimuli', bad, 'DESIGNED_STIMULUS != NONE => flag NO; flag YES => a registry control or INSTRUMENT:I-nn target; flag NO => target NONE')
bad = []
for s in SC:
    if s['DELIBERATE_VIOLATION_FLAG'] == 'YES' and s['TARGET_NEGATIVE_CONTROL'] in REG:
        r = s['EXPECTED_CONTROL_RESULTS'].get(s['TARGET_NEGATIVE_CONTROL'], '')
        if not any(k in r for k in ('FAIL', 'UNKNOWN', 'difference', 'REJECT')):
            bad.append(s['SCENARIO_ID'])
check('yes_target_result_names_the_failure', bad, 'AR2-09: the target control\'s expected result names its FAIL, UNKNOWN, difference or rejection')
check('conditional_e12c_cells_target_e12c', [s['SCENARIO_ID'] for s in SC if s['SCENARIO_ID'].startswith('WR-')
                                              and s['EXPECTED_CONTROL_RESULTS'].get('E12C', '').startswith('CONDITIONAL (CER-WIT): FAIL when')
                                              and s['TARGET_NEGATIVE_CONTROL'] != 'E12C'],
      'AR3-05: a scan cell whose detection is conditional on E-12 phase C (not a secondary channel) targets E12C')

# ------------------------------------------------------------------------------------------------------------------
# 6. SEALED_AT (AR3-25)
# ------------------------------------------------------------------------------------------------------------------
rule = CAT['SEALED_AT_RULE']
fields = rule['fields']
bad = []


def plain(v):
    if isinstance(v, str):
        return True
    if isinstance(v, list):
        return all(plain(x) for x in v)
    if isinstance(v, dict):
        return all(isinstance(k, str) and k.isascii() and plain(x) for k, x in v.items())
    return False


for s in SC:
    obj = {k: s[k] for k in fields}
    if not plain(obj):
        bad.append(s['SCENARIO_ID'] + ': a value is not a string, array or object with ASCII keys')
        continue
    h = hashlib.sha256(json.dumps(obj, sort_keys=True, separators=(',', ':'), ensure_ascii=False).encode('utf-8')).hexdigest()
    if s['SEALED_AT'] != h:
        bad.append(s['SCENARIO_ID'])
check('sealed_at_recomputes', bad, 'SHA-256 over the canonical serialization (sorted keys, UTF-8, separators "," and ":", no ASCII escaping) of the fields of SEALED_AT_RULE')
check('sealed_at_fields_are_schema_fields', [f for f in fields if f not in V4_FIELDS or f in ('SEALED_AT', 'STATUS', 'GROUPS_SERVED', 'CONTROL_CLASSES_EXERCISED')])

# ------------------------------------------------------------------------------------------------------------------
# 7. outcomes, conditional rules, pins, blockers, repetition
# ------------------------------------------------------------------------------------------------------------------
check('expected_outcome_kinds', [s['SCENARIO_ID'] for s in SC if not re.match(r'^(O[1-7]|NONE|CONDITIONAL \(CER-|PROVISIONAL_NON_GOVERNING$|preceding run: (O[1-7]|CONDITIONAL))',
                                                                               s['EXPECTED_PRODUCT_OUTCOME'])],
      'exact O1..O7, NONE, a declared conditional, PROVISIONAL_NON_GOVERNING or the outcome of the preceding run')
cer_json = list(CAT['CONDITIONAL_EXPECTATION_RULES'])
cer_md = [r[0].strip('`') for r in table_after(TXT[P_MD], '## 3. Conditional-expectation rules', '## 4.')]
check('cer_registry_same_in_json_and_md', ([] if cer_json == cer_md else ['json ' + json.dumps(cer_json) + ' md ' + json.dumps(cer_md)]) +
      [k for k in cer_json if k not in ('CER-ABV', 'CER-WIT', 'CER-SIL', 'CER-A2', 'CER-PERSIST')])
check('cer_references_registered', [s['SCENARIO_ID'] + ': ' + c for s in SC for c in re.findall(r'CER-[A-Z0-9]+', s['CONDITIONAL_EXPECTATION_RULE'] + ' ' +
                                                                                                  s['EXPECTED_PRODUCT_OUTCOME'] + ' ' + json.dumps(s['EXPECTED_CONTROL_RESULTS']))
                                    if c not in cer_json])
check('cer_used_is_declared_in_rule_field', [s['SCENARIO_ID'] for s in SC for c in set(re.findall(r'CER-[A-Z0-9]+', s['EXPECTED_PRODUCT_OUTCOME'] + json.dumps(s['EXPECTED_CONTROL_RESULTS'])))
                                             if c not in s['CONDITIONAL_EXPECTATION_RULE']])
ORDER = ['NONE', 'SYN', 'LEARN', 'P0', 'PL', 'PR', 'PW', 'PS', 'PV', 'CP']
bad_pin, bad_k3, bad_k8 = [], [], []
FXOPEN = ('FX-IMP', 'FX-IMP-CASE', 'FX-IMP-NESTED', 'FX-IMP-SYMTAB', 'FX-IMP-XREF-HOMONYM')
for s in SC:
    host = s['FIXTURE'] != 'NONE'
    gov_window = host and s['RUN_GOVERNANCE'] == 'GOVERNING' and not s['RECORDED_NOT_ENFORCED'] and s['REACH'] in WINDOW
    idx = ORDER.index(s['REACH'])
    exp = (['FIXTURE_INSTANCE@' + s['FIXTURE']] if host else [])
    exp += [p for p in s['PIN_SLOTS'] if p.startswith('FIXTURE_INSTANCE@L-VAR')]
    if gov_window:
        exp.append('E04_ADMITTED_SET@R-1')
        if idx >= ORDER.index('PL'):
            exp.append('LOCK_MODE')
        if idx >= ORDER.index('PV'):
            exp += ['EVM_FROZEN', 'HOST_DEFAULT_MAP']
    if s['REACH'] == 'LEARN' or s['SCENARIO_ID'].startswith('HDM-CHAR-'):
        exp.append('LOCK_MODE')
    if s['PIN_SLOTS'] != exp:
        bad_pin.append(s['SCENARIO_ID'] + ': ' + json.dumps(s['PIN_SLOTS']) + ' != ' + json.dumps(exp))
    if ('K-3' in s['BLOCKED_BY']) != (gov_window and idx >= ORDER.index('PV')):
        bad_k3.append(s['SCENARIO_ID'])
    if ('K-8' in s['BLOCKED_BY']) != (s['FIXTURE'] in FXOPEN):
        bad_k8.append(s['SCENARIO_ID'])
    if s['EVM_VERSION'] != ('PIN_SLOT EVM_FROZEN' if 'EVM_FROZEN' in s['PIN_SLOTS'] else 'NONE'):
        bad_pin.append(s['SCENARIO_ID'] + ': EVM_VERSION')
check('pin_slots_rule', bad_pin, 'section 2.1 of BA-04 V4')
check('k3_exactly_where_s1_s3_are_adjudicated', bad_k3, 'governing host scenario outside the AR3-01 mode with REACH PV or CP')
check('k8_exactly_on_fx_imp_family', bad_k8)
VOCAB = ('K-3', 'K-8', K6, AQ01, AQ03, 'E6-C4')
exp_where = {'E6-C4': ['NC-S5-O6'], K6: ['CL-CLEAN-FXANNBLANK-FP'], AQ03: sorted(i for i in ids if i.startswith('KS-')),
             AQ01: sorted(i for i in ids if i.startswith(('EV-L-', 'Q-I14-', 'WU-DRY-Q-I14-')))}
check('blocked_by_values', [s['SCENARIO_ID'] + ': ' + b for s in SC for b in s['BLOCKED_BY'] if b not in VOCAB] +
      [b + ' on ' + json.dumps(sorted(i for i in ids if b in BY[i]['BLOCKED_BY'])) for b, where in exp_where.items()
       if sorted(i for i in ids if b in BY[i]['BLOCKED_BY']) != where] +
      ([] if list(CAT['BLOCKED_BY_VOCABULARY']) == list(VOCAB) else ['JSON BLOCKED_BY_VOCABULARY ' + json.dumps(list(CAT['BLOCKED_BY_VOCABULARY']))]),
      'BLOCKED_BY uses only the vocabulary (K-3, K-8, K-6 (AQ-V4-07, Q-HDM-1), AQ-V4-01, AQ-V4-03, E6-C4); E6-C4 only on NC-S5-O6; K-6 only on '
      'CL-CLEAN-FXANNBLANK-FP; AQ-V4-03 exactly on KS-*; AQ-V4-01 exactly on EV-L-*, Q-I14-* and their dry runs')
bad = []
sec1 = TXT[P_MD].split('## 1. Schema', 1)[1].split('### 1.1 ', 1)[0]
brow = [l for l in sec1.split('\n') if l.startswith('| `BLOCKED_BY` |')]
if len(brow) != 1 or not all('`' + v + '`' in brow[0] for v in VOCAB):
    bad.append('md section 1: the BLOCKED_BY row does not list the whole vocabulary')
sec71 = TXT[P_MD].split('### 7.1 ', 1)[1].split('### 7.2 ', 1)[0]
for v in ('K-3', 'K-8', 'K-6 (AQ-V4-07, Q-HDM-1)', 'AQ-V4-01', 'AQ-V4-03'):
    if v not in sec71:
        bad.append('md 7.1 does not name the seal blocker ' + v)
if 'not read as a seal blocker (AQ-V4-04)' not in sec71:
    bad.append('md 7.1 does not state E6-C4 as not a seal blocker, pending AQ-V4-04')
hdr = TXT[P_MD].split('PREREQUISITES', 1)[1].split('BLOCKER ', 1)[0]
for v in ('K-3', 'K-8', 'K-6', 'Q-HDM-1', 'AQ-V4-07', 'AQ-V4-01..AQ-V4-06', 'AQ-V4-14', 'before the candidate'):
    if v not in hdr:
        bad.append('header PREREQUISITES does not name ' + v)
if 'before the seal' in hdr:
    bad.append('header PREREQUISITES gates a seal blocker before the seal (BA-11 V4: before the candidate)')
check('blockers_declared_in_sections_1_and_7_and_header', bad,
      'every BLOCKED_BY value is in the section 1 vocabulary; every value other than E6-C4 is a section 7.1 seal blocker; K-6 and the AQ blockers are in the header PREREQUISITES')
check('status_rule', [s['SCENARIO_ID'] for s in SC if s['STATUS'] != ('EXPLORATORY_NON_GOVERNING' if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING' else 'DRAFT' if s['BLOCKED_BY']
                                                                      else 'SEALED_PENDING_PIN_CANDIDATE' if s['PIN_SLOTS'] else 'SEALED_CANDIDATE')])
sample = CAT['PARAM_03_SAMPLE']
exp_sample = [i for i in ids if i.startswith('CL-CLEAN-FX')] + ['EA2-DEFERRED-EVENTS']
check('param_03_sample', ([] if sample == exp_sample else ['sample ' + json.dumps(sample)]) +
      [i for i in ids if BY[i]['REPETITION'].startswith('CLEAN') != (i in sample)] + [i for i in sample if BY[i]['RUN_GOVERNANCE'] != 'GOVERNING'],
      'the governing CL-CLEAN-<fixture> scenarios plus EA2-DEFERRED-EVENTS carry the CLEAN rule, and no other (AR3-24)')


def n_strict(s):
    """N_strict exactly as BA-10 V4 section 2.2: over the class-A and class-B CONTROL groups of GROUPS_SERVED (KR, evidence groups and
    CONTRACT_ASSIGNED_GROUPS excluded): N_A if only class A, N_B if only class B, max(N_A, N_B) if both. No ordering of N_A and N_B."""
    classes = {GT[g][0] for g in s['GROUPS_SERVED'] if GT[g][1] == 'CONTROL' and GT[g][0] in ('A', 'B')}
    return {frozenset(['A']): 'N_A', frozenset(['B']): 'N_B', frozenset(['A', 'B']): 'max(N_A, N_B)', frozenset(): None}[frozenset(classes)]


CHAR_LABEL = {'PIN': 'CHAR: CHAR_RUNS = N_A', 'EXPL': 'CHAR: CHAR_RUNS = EVIDENCE_REPETITION (exploratory)',
              'HDM': 'CHAR: CHAR_RUNS = N_A at the pinned key values, plus at least one informative run per varied key (outside the intent)'}


def expected_repetition(s):
    """The first row of the BA-10 V4 section 2.2 rule table that matches, with N_strict and CHAR_RUNS resolved."""
    ns = n_strict(s)
    if s['MODE'] == 'DRY_RUN':
        return 'DRY: WU_DRY_RUNS = N_B'
    if s['MODE'] == 'LEARNING' and ns is None:
        return 'LEARN: PARAM-02'
    if s['RUN_GOVERNANCE'] != 'GOVERNING':
        if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING':
            return CHAR_LABEL['EXPL']
        if s['SCENARIO_ID'].startswith('HDM-CHAR-'):
            return CHAR_LABEL['HDM']
        if s['SCENARIO_ID'].startswith(('E4-LEARN-R1-', 'LK-')):
            return CHAR_LABEL['PIN']
        return 'UNDERIVABLE: a non-governing scenario that feeds no pin or selection'
    if s['SCENARIO_ID'] in CAT['PARAM_03_SAMPLE']:
        return 'CLEAN: max(%s, ceil(n / K))' % ns
    if s['BUILD_UNDER_TEST'] == 'NONE' and ns is None:
        return 'SYNTH: ONE'
    if s['MODE'] == 'VALIDATION' and s['GROUP'] == 'E12-V':
        return 'VALIDATE: max(%s, PARAM-02 validation count)' % ns
    if ns is None:
        return 'EVIDENCE: EVIDENCE_REPETITION'
    if GT[s['GROUP']][1] == 'EVIDENCE':
        return 'EVIDENCE_STRICT: max(%s, EVIDENCE_REPETITION)' % ns
    return 'DEFAULT: ' + ns


bad = [s['SCENARIO_ID'] + ': ' + s['REPETITION'] + ' != ' + expected_repetition(s) for s in SC if s['REPETITION'] != expected_repetition(s)]
bad += [s['SCENARIO_ID'] + ': mixed-class label without max(N_A, N_B)' for s in SC
        if s['RUN_GOVERNANCE'] == 'GOVERNING' and n_strict(s) == 'max(N_A, N_B)' and 'max(N_A, N_B)' not in s['REPETITION']]
rr = CAT['REPETITION_RULES']
if not ('max(N_A, N_B)' in rr['DEFAULT'] and 'no ordering' in rr['DEFAULT'] and 'N_A if any class-A group' not in rr['DEFAULT']):
    bad.append('REPETITION_RULES.DEFAULT does not state N_strict as BA-10 V4 section 2.2')
if 'CHAR_RUNS' not in rr['CHAR'] or 'CHAR_RUNS(s) = N_A + V(s)' not in rr['CHAR']:
    bad.append('REPETITION_RULES.CHAR does not name CHAR_RUNS of BA-10 V4')
md4 = []
for line in TXT[P_MD].split('| Repetition rule | Value |', 1)[1].split('\n')[2:]:
    if not line.startswith('|'):
        break
    md4.append([c.strip() for c in line.strip()[1:-1].split('|')])
if {r[0].strip('`'): r[1] for r in md4} != dict(rr):
    bad.append('md section 4 repetition rules differ from the JSON REPETITION_RULES')
check('repetition_label_equals_ba10_derivation', bad,
      'AR3-24 and BA-10 V4 section 2.2: the REPETITION label of every scenario is the first matching row of the BA-10 V4 rule table with N_strict = N_A '
      '(class A only), N_B (class B only) or max(N_A, N_B) (both; no ordering assumed) and CHAR_RUNS resolved; every mixed-class governing label reads max(N_A, N_B)')
check('dry_rule_n_b', [s['SCENARIO_ID'] for s in SC if (s['MODE'] == 'DRY_RUN') != s['REPETITION'].startswith('DRY: WU_DRY_RUNS = N_B')])

# ------------------------------------------------------------------------------------------------------------------
# 8. rulings with a mechanical trace (AR3-04, AR3-07, AR3-08, AR3-10, AR3-11, AR3-15)
# ------------------------------------------------------------------------------------------------------------------
t = []
for w in 'ADEFG':
    for sid in ('WR-%s-X1' % w, 'WR-%s-X1-EV' % w):
        r = BY[sid]['EXPECTED_CONTROL_RESULTS']
        if not (r.get('S2') == 'difference' and r.get('S3') == 'difference'):
            t.append(sid + ' (AR3-08)')
if not (BY['WR-SA-X1']['EXPECTED_CONTROL_RESULTS'].get('S2') == 'difference' and BY['WR-SA-X1']['EXPECTED_CONTROL_RESULTS'].get('S3') == 'difference'):
    t.append('WR-SA-X1 (AR3-08)')
for x in (2, 3, 4):
    r = BY['WR-SA-X%d' % x]['EXPECTED_CONTROL_RESULTS']
    if not (r.get('S2') == 'equal' and r.get('S3') == 'difference'):
        t.append('WR-SA-X%d (AR3-08)' % x)
for w in 'FG':
    for sid in ['WR-%s-X%d' % (w, x) for x in (5, 6, 7)] + ['WR-%s-X%d-EV' % (w, x) for x in (5, 6)]:
        if BY[sid]['GROUP'] != 'E8':
            t.append(sid + ' (AR3-10)')
n = BY['NC-S5-O6']
if not (n['EXPECTED_CONTROL_RESULTS'].get('E08', '').startswith('FAIL at PV') and 'E6-C4' in n['BLOCKED_BY'] and 'E06' not in n['CONTROLS_EXERCISED']
        and n['EXPECTED_CONTROL_RESULTS'].get('E12A', '').startswith('CONDITIONAL (CER-WIT)') and 'CER-PERSIST' in n['CONDITIONAL_EXPECTATION_RULE']):
    t.append('NC-S5-O6 (AR3-04)')
n = BY['NC-CLONE-REPLACE']
if not (n['FIXTURE'] == 'FX-IMP-NESTED' and n['GROUP'] == 'PW-CLONE' and n['TARGET_NEGATIVE_CONTROL'] == 'CLONE' and n['DELIBERATE_VIOLATION_FLAG'] == 'YES'
        and n['BLOCKED_BY'] == ['K-8'] and n['EXPECTED_PRODUCT_OUTCOME'].startswith('O6') and n['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD'):
    t.append('NC-CLONE-REPLACE (AR3-07)')
for sid in ('HDM-CHAR-FX1F', 'HDM-CHAR-FXANN', 'HDM-CHAR-FXDIM'):
    h = BY[sid]
    if not (h['GROUP'] == 'HDM' and h['GROUPS_SERVED'] == [] and h['RUN_GOVERNANCE'] == 'NON_GOVERNING_BY_DESIGN' and h['RECORDED_NOT_ENFORCED']):
        t.append(sid + ' (AR3-11)')
c = BY['CL-CLEAN-FXANNBLANK-FP']
if not (c['FIXTURE'] == 'FX-ANN-BLANK' and c['GROUP'] == BY['CL-CLEAN-FXANN-FP']['GROUP'] and c['BLOCKED_BY'] == ['K-3', K6] and 'WU-DRY-CL-CLEAN-FXANNBLANK-FP' in BY
        and c['EXPECTED_PRODUCT_OUTCOME'] == 'O3 SUCCESS' and 'once the ruling supplies' in c['NOTE'] and c['STATUS'] == 'DRAFT'):
    t.append('CL-CLEAN-FXANNBLANK-FP (AR3-15; K-6 AQ-V4-07: BLOCKED_BY K-3 and K-6, O3 kept as the expectation once the ruling supplies the field source)')
for sid in ('KS-SL1-POSTWRITE', 'KS-SL1-TX-MISMATCH', 'KS-SL4-NEG-DET'):
    k = BY[sid]
    cag = k['CONTRACT_ASSIGNED_GROUPS']
    if not (sorted(x['group'] for x in cag) == ['KS', 'SC-A'] and all('V2.1 26.3' in x['clause'] and 'V2.1 31' in x['clause'] and 'AQ-V4-03' in x['clause'] for x in cag)
            and k['BLOCKED_BY'] == [AQ03] and k['STATUS'] == 'DRAFT' and 'KS' not in k['GROUPS_SERVED'] and 'SC-A' not in k['GROUPS_SERVED']):
        t.append(sid + ' (KS-* contract membership V2.1 26.3 and 31 in CONTRACT_ASSIGNED_GROUPS; BLOCKED_BY AQ-V4-03)')
if not ('S2' in BY['U-3']['CONTROLS_EXERCISED'] and 'KS' in BY['U-3']['GROUPS_SERVED']):
    t.append('U-3 S2 exercised (AR3-09)')
for sid in ('HDM-CHAR-FX1F', 'HDM-CHAR-FXANN', 'HDM-CHAR-FXDIM'):
    p = BY[sid]['PLAN_OR_INPUT']
    if 'CHAR_RUNS = N_A of them' in p or 'outside the intent of CHAR_RUNS' in p:
        t.append(sid + ' (CHAR_RUNS used with a second meaning)')
    if not all(x in p for x in ('HDM-3', 'HDM-4', 'as pinned in the fixture instances', 'CHAR_RUNS(s) = N_A + V(s)', 'at least one informative run per key variable',
                                'every object class its mirror creates')):
        t.append(sid + ' (BA-08 V4 11.2 HDM-3/HDM-4 and CHAR_RUNS)')
sec21 = TXT[P_MD].split('### 2.1 ', 1)[1].split('### 2.2 ', 1)[0]
if not all(x in sec21 for x in ('ratification records of the Coordinator and of the Architect', '`PIN_RECORDED`', '`ANCHOR_RECORDED`', '`IN_USE`',
                                '`IN_USE.pinRecordHashes`', 'no BA-11 value is written')) or 'then ANCHORED' in sec21:
    t.append('md 2.1 pin custody (AR3-23; BA-07 V4)')
sec8 = TXT[P_MD].split('## 8. Open questions', 1)[1].split('## 9. ', 1)[0]
for q in ('AQ-V4-01', 'AQ-V4-02', 'AQ-V4-03', 'AQ-V4-04', 'AQ-V4-05', 'AQ-V4-06', 'AQ-V4-14'):
    if '**' + q + ' ' not in sec8:
        t.append('md 8: ' + q + ' missing')
if re.search(r'^\d+\. \*\*', sec8, re.M):
    t.append('md 8 still holds numbered questions')
if 'no expectation of this catalog is changed' not in sec8:
    t.append('md 8: AQ-V4-14 must change no expectation')
for sid, k, v in (('U-3', 'S3', 'difference'), ('U-4', 'S3', 'UNKNOWN'), ('U-5', 'E11M', 'FAIL at PC'), ('U-5', 'E10', 'FAIL at PC')):
    if not BY[sid]['EXPECTED_CONTROL_RESULTS'].get(k, '').startswith(v) or k not in BY[sid]['CONTROLS_EXERCISED']:
        t.append(sid + ' ' + k + ' (AR3-09)')
if not (BY['U-1']['DESIGNED_STIMULUS'] != 'NONE' and BY['U-1']['INJECTIONS'] and BY['U-6b']['CONDITIONAL_EXPECTATION_RULE'] == 'CER-ABV'
        and BY['EA1-READONLY-OPENS']['REACH'] == 'NONE' and BY['EA1-READONLY-OPENS']['EXPECTED_PRODUCT_OUTCOME'].startswith('NONE')):
    t.append('U-1 / U-6b / EA1 (AR3-09)')
check('ruling_traces', t)

# every registry-control key of EXPECTED_CONTROL_RESULTS is in CONTROLS_EXERCISED, or explicitly NOT_DECLARED, or (for a control of the scenario's
# own RECORDED_NOT_ENFORCED set) explicitly RECORDED_NOT_ENFORCED: a declared result cannot bypass the GROUPS_SERVED derivation (U-3 finding)
RNE_NAME = {'E03': 'E-03', 'E04': 'E-04', 'E12A': 'E-12 phase A', 'E12C': 'E-12 phase C', 'S1': 'S-1', 'S2': 'S-2', 'S3': 'S-3'}
bad = []
for s in SC:
    for k, v in s['EXPECTED_CONTROL_RESULTS'].items():
        if k not in REG or k in s['CONTROLS_EXERCISED']:
            continue
        if v.startswith('NOT_DECLARED'):
            continue
        if k in RNE_NAME and RNE_NAME[k] in s['RECORDED_NOT_ENFORCED'] and v.startswith('RECORDED_NOT_ENFORCED'):
            continue
        bad.append(s['SCENARIO_ID'] + ': ' + k + ' = ' + v[:60])
check('expected_result_keys_exercised_or_declared', bad,
      'every registry key of EXPECTED_CONTROL_RESULTS is in CONTROLS_EXERCISED, or its value is NOT_DECLARED, or it is a control of the scenario\'s own '
      'RECORDED_NOT_ENFORCED set with the value RECORDED_NOT_ENFORCED')

# Q-FPSPEC-VECTORS and Q-SIGNED-ZERO against the BA-05 V4 vectors file (AR3-17; BA-05 V4 sections 2.1, 2.7, 3.3, 3.3.1)
bad = []
ev = {x['id']: x for x in FPV['exactVectors']}
rj = [x['id'] for x in FPV['rejectedInputs']]
sv = {x['id']: x for x in FPV['semanticVectors']}
q = BY['Q-FPSPEC-VECTORS']
r = q['EXPECTED_CONTROL_RESULTS'].get('instrument I-12', '')
if rj != ['RJ-%02d' % n for n in range(1, len(rj) + 1)] or not all(x['result'] == 'REJECTED' for x in FPV['rejectedInputs']):
    bad.append('vectors file: rejectedInputs are not RJ-01..RJ-nn, all REJECTED')
if not ('every REJECTED input (rejectedInputs ' + rj[0] + '..' + rj[-1] + ') reproduced as REJECTED' in r and 'every DIGEST reproduced' in r
        and 'every UNKNOWN reproduced' in r and 'fails the exact-arithmetic check of BA-05 V4 3.3.1 is INVALID and reported, never PASS' in r):
    bad.append('Q-FPSPEC-VECTORS: expected results miss REJECTED, UNKNOWN, DIGEST or the INVALID instance rule')
if not all(x in q['PLAN_OR_INPUT'] for x in ('exactVectors', 'rejectedInputs', 'semanticVectors', P_FPV)):
    bad.append('Q-FPSPEC-VECTORS: the input sections of the vectors file are not named')
slot_units = sorted(i for i, x in sv.items() if isinstance(x.get('persisted'), dict) and 'slotExpression' in x['persisted'])
if not all(i in q['PLAN_OR_INPUT'] for i in slot_units):
    bad.append('Q-FPSPEC-VECTORS: slot-unit vectors not named: ' + json.dumps(slot_units))
if 'SEAL_HASH of BA-08' not in q['TUPLE_REQUIREMENTS']:
    bad.append('Q-FPSPEC-VECTORS: FPSPEC_CONFORMANCE_TUPLE does not bind the BA-08 SEAL_HASH')
z = BY['Q-SIGNED-ZERO']
zt = z['PLAN_OR_INPUT'] + ' ' + z['EXPECTED_CONTROL_RESULTS'].get('instrument I-12', '')
cited = sorted(set(re.findall(r'\b(EV-\d\d|SV-\d\d)\b', zt)))
if cited != ['EV-01', 'EV-02', 'EV-06', 'EV-07', 'SV-05']:
    bad.append('Q-SIGNED-ZERO cites ' + json.dumps(cited))
try:
    if not (ev['EV-01']['result'] == ev['EV-02']['result'] == 'DIGEST' and ev['EV-01']['sha256'] != ev['EV-02']['sha256']
            and ev['EV-01']['input']['y']['binary64'] == '0000000000000000' and ev['EV-02']['input']['y']['binary64'] == '8000000000000000'
            and ev['EV-06']['result'] == ev['EV-07']['result'] == 'UNKNOWN' and 'non-finite' in ev['EV-06']['reason'] + ev['EV-07']['reason']
            and sv['SV-05']['result'] == 'EQUAL' and sv['SV-05']['expected'] == '0000000000000000' and sv['SV-05']['persisted'] == '8000000000000000'):
        bad.append('vectors file: EV-01/EV-02 (signed zero), EV-06/EV-07 (non-finite) or SV-05 do not hold what Q-SIGNED-ZERO cites')
except KeyError as e:
    bad.append('vectors file: missing ' + str(e))
check('fpspec_vector_ids_and_outcomes', bad,
      'Q-FPSPEC-VECTORS names exactVectors, rejectedInputs and semanticVectors, expects every DIGEST, UNKNOWN and REJECTED (RJ-01..RJ-nn) and the INVALID '
      'instance rule of BA-05 V4 3.3.1, binds the BA-08 SEAL_HASH; Q-SIGNED-ZERO cites EV-01, EV-02, SV-05, EV-06, EV-07, which hold those outcomes')

# ------------------------------------------------------------------------------------------------------------------
# 9. md companion equals the JSON (section 5.1 and totals)
# ------------------------------------------------------------------------------------------------------------------
rows = table_after(TXT[P_MD], '### 5.1 Scenario summary', '### 5.2')
bad = []
own = [s for s in SC if not s['SCENARIO_ID'].startswith('WU-DRY-')]
if len(rows) != len(own):
    bad.append('row count %d != %d' % (len(rows), len(own)))
for r, s in zip(rows, own):
    oc = s['EXPECTED_PRODUCT_OUTCOME']
    oc = (oc if len(oc) <= 70 else oc[:67] + '...').replace('|', '/')
    exp = ['`%s`' % s['SCENARIO_ID'], '`%s`' % s['GROUP'], s['REACH'], s['INJECTION_COORDINATE'].replace('|', '/'), s['TARGET_NEGATIVE_CONTROL'],
           'Y' if s['DESIGNED_STIMULUS'] != 'NONE' else '-', {'PRODUCT_BUILD': 'P', 'CHARACTERIZATION_BUILD': 'C', 'NONE': '-'}[s['BUILD_UNDER_TEST']], oc,
           s['STATUS'], ', '.join(s['BLOCKED_BY']) or '-', '`%s`' % s['SEALED_AT'][:12]]
    if r != exp:
        bad.append(s['SCENARIO_ID'])
m = re.search(r'Totals: \*\*(\d+)\*\* scenarios \((\d+) own scenarios and (\d+) `WU-DRY-\*`\)', TXT[P_MD])
if not m or (int(m.group(1)), int(m.group(2)), int(m.group(3))) != (len(SC), len(own), len(SC) - len(own)):
    bad.append('totals line')
check('md_summary_equals_json', bad)

# ------------------------------------------------------------------------------------------------------------------
# 10. BA-02b V4: control coverage recomputed (rules 3..5, AR3-03), group coverage, verbatim rows and discharges
# ------------------------------------------------------------------------------------------------------------------


def countable(s):
    return (s['RUN_GOVERNANCE'] == 'GOVERNING' and s['MODE'] in ('CHARACTERIZATION_RUN', 'VALIDATION') and s['STATUS'] != 'EXPLORATORY_NON_GOVERNING'
            and s['GROUP'] not in EVID and not s['RECORDED_NOT_ENFORCED'])


def names_fail(s, c):
    r = s['EXPECTED_CONTROL_RESULTS'].get(c, '')
    return any(k in r for k in ('FAIL', 'UNKNOWN', 'difference', 'REJECT'))


def conditional(s, c):
    return s['EXPECTED_CONTROL_RESULTS'].get(c, '').startswith('CONDITIONAL') or 'CER-SIL' in s['CONDITIONAL_EXPECTATION_RULE'] or 'CER-PERSIST' in s['CONDITIONAL_EXPECTATION_RULE']


cov_md = {r[0].strip('`'): r for r in table_after(TXT[P_COV], '## 2. Control coverage', '## 3.')}
REASONED = ('E01', 'E13', 'A1', 'A2', 'E12DET', 'E14', 'E15')
bad, gaps, cond = [], [], []
for k in REG:
    cnt = [s for s in SC if k in s['CONTROLS_EXERCISED'] and countable(s)]
    clean = [s for s in cnt if s['DELIBERATE_VIOLATION_FLAG'] == 'NO']
    negs = [s for s in cnt if s['DELIBERATE_VIOLATION_FLAG'] == 'YES' and s['TARGET_NEGATIVE_CONTROL'] == k and names_fail(s, k)]
    neg_u = [s for s in negs if not conditional(s, k)]
    neg_c = [s for s in negs if conditional(s, k)]
    reason = k in REASONED
    if k in ('E14', 'E15'):
        neg_u, neg_c = [], []
    if not [s for s in SC if k in s['CONTROLS_EXERCISED']]:
        st = 'GAP'
    elif not (neg_u or neg_c or reason):
        st = 'NEGATIVE_CONTROL_MISSING'
    elif not clean and k not in ('E14', 'E15'):
        st = 'CLEAN_SCENARIO_MISSING'
    elif not neg_u and neg_c and not reason:
        st = 'COVERED_DRAFT_CONDITIONAL'
    else:
        st = 'COVERED_DRAFT'
    if st not in ('COVERED_DRAFT', 'COVERED_DRAFT_CONDITIONAL'):
        gaps.append(k + ': ' + st)
    if st == 'COVERED_DRAFT_CONDITIONAL':
        cond.append(k)
    row = cov_md.get(k)
    if not row or row[-1] != st or row[-2] != '0' or row[3] != ('YES' if REG[k]['inPredicate'] else 'NO'):
        bad.append(k + ': md ' + (row[-1] if row else 'missing') + ' != ' + st)
check('ba02b_no_coverage_gap_except_declared_conditionals', gaps,
      'every control is COVERED_DRAFT, or COVERED_DRAFT_CONDITIONAL (only conditional negative controls: NEGATIVE_CONTROL_NOT_EVALUATED at acceptance, AR3-03)')
check('ba02b_section2_states_equal_recomputation', bad)
m = re.search(r'Conditional rows \(rule 5\): (.*?)\. The column', TXT[P_COV])
declared = re.findall(r'`([A-Z0-9]+)` \(', m.group(1)) if m else None
check('ba02b_conditional_rows_declared', [] if declared == cond else ['declared ' + json.dumps(declared) + ' recomputed ' + json.dumps(cond)])
check('ba02b_section2_rows_all_controls', sorted(set(REG) ^ set(cov_md)))

# group coverage
g_md = {r[0].strip('`'): r for r in table_after(TXT[P_COV], '## 4. Group coverage', '## 5.')}
bad = []
for g, (cls, kind) in GT.items():
    prim = [s for s in SC if s['GROUP'] == g]
    serv = [s for s in SC if g in s['GROUPS_SERVED']]
    cag = [s for s in SC if any(c['group'] == g for c in s['CONTRACT_ASSIGNED_GROUPS'])]
    cnt = 'N/A (PA-3)' if kind == 'EVIDENCE' else 'N/A (PA-4)' if kind == 'CONTROL_OUTSIDE_PREDICATE' else str(len([s for s in serv if countable(s)]))
    gov = len({s['SCENARIO_ID'] for s in prim + serv if s['RUN_GOVERNANCE'] == 'GOVERNING'})
    exp = ['`%s`' % g, kind, cls, str(len(prim)), str(len(serv)), str(len(cag)), cnt, str(gov), '0']
    if g_md.get(g) != exp:
        bad.append(g + ': md ' + json.dumps(g_md.get(g)) + ' != ' + json.dumps(exp))
    if not serv and not cag:
        bad.append(g + ': no serving or contract-assigned scenario')
check('ba02b_group_coverage_recomputed', bad, 'contract-assigned scenarios are counted apart and never as serving or countable')

# section 3: verbatim rows and discharges


def contract_rows(text, start, end):
    part = text.split(start)[1].split(end)[0]
    return [[x.strip() for x in line.strip()[1:-1].split('|')] for line in part.split('\n')
            if line.startswith('| ') and not line.startswith('| CONTROL |') and not line.startswith('|---')]


V21R = contract_rows(TXT[P_V21], '## 31. CONTROL TO GROUP TO SCENARIO COVERAGE MATRIX', '**Design-level coverage checks')
PA14R = contract_rows(TXT[P_V22], '- **Section 31 (coverage matrix), replaced or added rows.**', '### PA-1.5')
exp_rows = [('R%02d' % (i + 1), 'V2.1 31', r) for i, r in enumerate(V21R)] + [('P%d' % (i + 1), 'V2.2 PA-1.4', r) for i, r in enumerate(PA14R)]
md_rows = table_after(TXT[P_COV], '### 3.1 Rows in effect', '### 3.2')
bad = []
if len(md_rows) != len(exp_rows):
    bad.append('row count %d != %d' % (len(md_rows), len(exp_rows)))
for mr, (rid, src, cells) in zip(md_rows, exp_rows):
    if mr != [rid, src] + cells:
        bad.append(rid)
check('ba02b_section3_rows_verbatim', bad, 'the five cells of every row in effect equal the contract cells (V2.1 31: 22 rows incl. the four NOT_A_CONTROL rows; V2.2 PA-1.4: 7 rows)')
check('ba02b_section3_not_a_control_rows_present', [x for x in ('INSTRUMENT_QUALIFICATION', 'EVALUATOR_CONFORMANCE', 'CLEANUP', 'UNDO, SAVE_REOPEN, POST_CP, tail log, E-12 learning')
                                                     if not any(r[2] == x for r in md_rows)])
row_by = {r[0]: r for r in md_rows}
ex = {(r[0], r[1].strip('`')) for r in table_after(TXT[P_COV], 'Declared exceptions to the class membership', '### 3.3')}
GROUP_NAMES = set(GT)


def member(cls_cell, sid):
    s = BY[sid]
    for cl in re.findall(r'`([^`]+)`', cls_cell):
        if cl in GROUP_NAMES:
            if cl in s['GROUPS_SERVED'] or s['GROUP'] == cl:
                return True
            continue
        special = {'CL-CLEAN': sid.startswith('CL-CLEAN-FX'), 'S1': sid == 'S1-SAVE-REOPEN', 'WR-*-X7': bool(re.match(r'^WR-[A-G]-X7$', sid)),
                   'E6-C1..E6-C8': sid.startswith('E6-C'), 'OU-O1..O7': sid.startswith('OU-'), 'CLOSURE_COMPLETENESS_CONTROL': sid == 'PR-CLOSURE-COMPLETENESS',
                   'SOURCE_SWAP_CONTROL': sid.startswith('PR-SOURCE-SWAP-'), 'CLONE_NON_REPLACE_CONTROL': sid.startswith('PW-CLONE-'),
                   'WARMUP_BOUNDARY_CONTROL': sid == 'WU-BOUNDARY', 'Q-I13': sid == 'Q-I13'}
        if cl in special:
            if special[cl]:
                return True
            continue
        if cl.startswith('NC-'):
            if sid.startswith(cl):
                return True
            continue
        pre = cl.split('<')[0] if '<' in cl else cl[:-1] if cl.endswith('*') else ''
        if pre.endswith('-') and sid.startswith(pre):
            if cl.startswith('WR-') and '(X0..X6)' in cls_cell:
                mm = re.search(r'-X(\d)', sid)
                if not (mm and int(mm.group(1)) <= 6):
                    continue
            return True
    return False


bad, bad_state = [], []
clauses = table_after(TXT[P_COV], '### 3.2 Discharge by clause', 'Declared exceptions')
seen_rows = set()
for rid, text, lst_cell, state in clauses:
    seen_rows.add(rid)
    if rid not in row_by:
        bad.append(rid + ': unknown row')
        continue
    req = row_by[rid][6]
    if text not in req:
        bad.append(rid + ': clause not a verbatim fragment: ' + text)
    lst = ids_in(lst_cell)
    usable = []
    for sid in lst:
        if sid not in BY:
            bad.append(rid + ': ' + sid + ' not in the catalog')
            continue
        if not (member(row_by[rid][5], sid) or (rid, sid) in ex):
            bad.append(rid + ': ' + sid + ' not in a class of the row and not a declared exception')
        if BY[sid]['STATUS'] != 'EXPLORATORY_NON_GOVERNING':
            usable.append(sid)
    if not usable or not state.startswith('MET_IN_DRAFT'):
        bad_state.append(rid + ': ' + text + ' -> ' + state)
check('ba02b_section3_discharges', bad, 'each clause is a verbatim fragment of its row; each listed scenario exists and is a class member or a declared exception')
check('ba02b_section3_every_row_discharged', bad_state + [r for r in row_by if r not in seen_rows],
      'every row has clauses and every clause has at least one non-exploratory scenario (MET_IN_DRAFT)')
st3 = table_after(TXT[P_COV], '### 3.3 State per control', '## 4.')
check('ba02b_section3_control_states', [r[0] for r in st3 if r[2] != 'MET_IN_DRAFT'] + sorted(set(REG) - {r[0].strip('`') for r in st3}))
check('ba02b_checks_section', [l for l in table_after(TXT[P_COV], '## 5. Design-level checks', '## 6.') if not l[1].startswith('PASS')])

# ------------------------------------------------------------------------------------------------------------------
# report
# ------------------------------------------------------------------------------------------------------------------
own_ids = [s['SCENARIO_ID'] for s in own]
counts = O([
    ('total', len(SC)), ('own', len(own)), ('wu_dry', len(SC) - len(own)),
    ('by_run_governance', O(sorted(Counter(s['RUN_GOVERNANCE'] for s in SC).items()))),
    ('by_group', O(sorted(Counter(s['GROUP'] for s in SC).items()))),
    ('by_status', O(sorted(Counter(s['STATUS'] for s in SC).items()))),
    ('by_build_under_test', O(sorted(Counter(s['BUILD_UNDER_TEST'] for s in SC).items()))),
    ('by_repetition', O(sorted(Counter(s['REPETITION'] for s in SC).items()))),
    ('blockers', O(sorted(Counter(b for s in SC for b in s['BLOCKED_BY']).items()))),
    ('pin_slots', O(sorted(Counter(p.split('@')[0] for s in SC for p in s['PIN_SLOTS']).items()))),
    ('ar3_01_mode', len(mode)), ('ar3_01_mode_by_ruling', len(mode) - len(proposal)), ('ar3_01_mode_proposal_aq_v4_01', len(proposal)),
    ('pending_ruling_blockers', O((b, len(exp_where[b])) for b in (AQ01, AQ03, K6))),
    ('cl_clean', [i for i in ids if i.startswith('CL-CLEAN-FX')]),
    ('param_03_sample', sample),
    ('conditional_negative_control_rows', cond),
])
failed = [k for k, v in RESULTS.items() if v['result'] != 'PASS']
report = O([
    ('checker', 'docs/automation/evidence/I-52-ct21d-ba04-v4-checks.py'),
    ('checker_sha256_lf', sha(read('docs/automation/evidence/I-52-ct21d-ba04-v4-checks.py'))),
    ('inputs_sha256_lf', O((p, sha(TXT[p])) for p in (P_JSON, P_MD, P_COV, P_REG, P_V21, P_V22, P_FPV))),
    ('hash_method', 'SHA-256 of the UTF-8 file text after CRLF -> LF normalization, lowercase hex'),
    ('summary', O([('checks', len(RESULTS)), ('passed', len(RESULTS) - len(failed)), ('failed', failed)])),
    ('counts', counts),
    ('checks', RESULTS),
])
if len(sys.argv) != 2:
    sys.stderr.write('usage: python I-52-ct21d-ba04-v4-checks.py <output.json>\n')
    sys.exit(2)
io.open(sys.argv[1], 'w', encoding='utf-8', newline='').write(json.dumps(report, indent=1, ensure_ascii=False) + '\n')
print('checks %d, passed %d, failed %s' % (len(RESULTS), len(RESULTS) - len(failed), failed))
sys.exit(0 if not failed else 1)
