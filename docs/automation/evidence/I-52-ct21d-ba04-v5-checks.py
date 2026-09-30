"""Mechanical checks of BA-04 V5 (scenario catalog) and BA-02b V5 (coverage matrix), I-52 CT-21D authority baseline.

Usage (from any directory):  python I-52-ct21d-ba04-v5-checks.py <output.json>

Reads only repository files (paths relative to the repository root, found from this file's location):
  docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json and .md
  docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v5.md
  docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md   (registry and group table, verbatim V2.2 PA-1.2 / PA-1.3)
  docs/initiatives/I-52-ct21d-authority-contract-v2.1.md, -v2.2.md      (verbatim rows of V2.1 31, V2.2 PA-1.4; V2.1 4.2, 4.3, 17.2; V2.2 PA-12)
  docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.json          (BA-05 V5 vector ids cited by Q-FPSPEC-VECTORS and Q-SIGNED-ZERO)
  docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v5.md      (BA-07 V5: the build-equivalence record the catalog points to)
Recomputes every derived value from the JSON, the registry and the contract independently of the generator, compares it with what the
artifacts state, and writes a JSON report. Exit code 0 when every check passes, 1 otherwise. No check is weakened to pass: a failing
check lists its offending items.

Round V5 (decisions section 217): every check of the V4 checker is kept or tightened; new checks verify the per-build qualification twins
(AR4-06 (3)), the CHARACTERIZATION_TUPLE profile (AR4-06 (1), (2)), the BUILD_EQUIVALENCE_RECORD prerequisite (AR4-06 (4)), the EV-L-* fields
(AR4-01), the dry-run branch switches (AR4-02), the canary-load exclusion of the dry runs (AR4-16), NC-E12A by CER-WIT (AR4-16), the
GROUPS_SERVED / CONTRACT_ASSIGNED_GROUPS definition of BA-04 section 1 applied by BA-02b (AR4-17), the admitted contract-assigned sources
(AR3-06, AR4-03), the log deltas (LP-INJ, LP-WIT, LP-PAR), the four HDM-CHAR-* (AR4-07, AR4-14), the enumerated sets of BA-02b rule 6, and the
Delta V5 tables against the finding lists of the round.

Correction pass of round V5 (the coverage and consistency critics' gaps on BA-04 and BA-02b): no check is removed or relaxed for the scenarios it
already covered. The checks that stated the V5 draft's readings are replaced by stricter ones: A-1 exercised exactly by EA1 and the PARAM-03 sample
with CER-A2 and A1_EVALUATION (in place of "A1 only in EA1"), the open Architect questions AQ-V5-01 / AQ-V5-02 named as before-candidate items (in
place of "no open question remains"), the E03 key with ENFORCED_NOT_EXERCISED (in place of the non-registry key), the per-predicate instance rule
of BA-05 V5 3.3.1 (in place of "exact-arithmetic check"), the pointer of section 2.1 to BA-07 V5. New checks: the driver DLL in the one
PRODUCT_BUILD profile, the REVIEWED_RECORD, the build-equivalence record's location, content and custody, the qualification record of every
governing I-14 writer injection (the fifth vehicle Q-I14-P included), HDM-CHAR-FXDIM, EV-L-COMPOSED's outcome, sections 7.4 and 8.1, and the
correction-pass rows of both Delta V5 tables.

Correction pass (final) of round V5 (the re-check of the correction pass): the build-equivalence check no longer requires the catalog's own
location and field names for the record (the CP-09 variant, which contradicted BA-07 V5): it requires the pointer to BA-07 V5 sections 2, 4
and 5.3 and section 7, U5, the catalog's own rules (which runs name the record; the citation of its record hash in RUN_START), U5 (never U1)
for the record in the vocabulary, the citation rule and step 6 of 2.2, and the absence of the superseded path and field names in both files;
it reads BA-07 V5 to confirm that the sections pointed to hold the record (section 2 row, section 4, the 5.3 schema and custody, U5). The
open item AQ-V5-08 (the admission of the vehicle Q-I14-P) is checked as the third BLOCKED_BY value, exactly on Q-I14-P and WU-DRY-Q-I14-P,
and as a before-candidate item (header, 7.1 C-3 and the sentence after it, 8.2, section 1.1); section 2.1 points to U1-U5; the
'correction pass (final)' rows of both Delta V5 tables are checked. No check is removed or relaxed for what it already covered.
"""
import hashlib
import io
import json
import os
import re
import sys
from collections import Counter, OrderedDict

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..'))
P_JSON = 'docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json'
P_MD = 'docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.md'
P_COV = 'docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v5.md'
P_REG = 'docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md'
P_V21 = 'docs/initiatives/I-52-ct21d-authority-contract-v2.1.md'
P_V22 = 'docs/initiatives/I-52-ct21d-authority-contract-v2.2.md'
P_FPV = 'docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.json'
P_SELF = 'docs/automation/evidence/I-52-ct21d-ba04-v5-checks.py'
P_BA07 = 'docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v5.md'
O = OrderedDict
PB, CB = 'PRODUCT_BUILD', 'CHARACTERIZATION_BUILD'
AR301_SET = ['E-03', 'E-04', 'E-12 phase A', 'E-12 phase C', 'S-1', 'S-2', 'S-3']
EVL_SET = ['E-04', 'E-12 phase A', 'E-12 phase C']
Q14_VEHICLES = ['Q-I14-Y', 'Q-I14-X', 'Q-I14-SILENT-Y', 'Q-I14-SILENT-X', 'Q-I14-P']   # Q-I14-P: correction pass (PREPARE-phase coordinate)
Q14_FAMILY = ['Q-I14'] + Q14_VEHICLES
FXOPEN = ('FX-IMP', 'FX-IMP-CASE', 'FX-IMP-NESTED', 'FX-IMP-SYMTAB', 'FX-IMP-XREF-HOMONYM')
AQ508 = ['Q-I14-P']                        # correction pass (final): the vehicle whose admission is the open Architect item AQ-V5-08
AQ508_ALL = ['Q-I14-P', 'WU-DRY-Q-I14-P']  # ... and its dry run, which inherits the item like K-8


def read(rel):
    return io.open(os.path.join(ROOT, rel), encoding='utf-8', newline='').read().replace('\r\n', '\n')


def sha(txt):
    return hashlib.sha256(txt.encode('utf-8')).hexdigest()


TXT = {p: read(p) for p in (P_JSON, P_MD, P_COV, P_REG, P_V21, P_V22, P_FPV, P_SELF, P_BA07)}
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


def section(text, start, stop):
    return text.split(start, 1)[1].split(stop, 1)[0]


def strings(o):
    if isinstance(o, str):
        yield o
    elif isinstance(o, dict):
        for v in o.values():
            yield from strings(v)
    elif isinstance(o, list):
        for v in o:
            yield from strings(v)


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
RNE_KEY = {'E03': 'E-03', 'E04': 'E-04', 'E12A': 'E-12 phase A', 'E12C': 'E-12 phase C', 'S1': 'S-1', 'S2': 'S-2', 'S3': 'S-3'}

# ------------------------------------------------------------------------------------------------------------------
# 1. identity and schema
# ------------------------------------------------------------------------------------------------------------------
ids = [s['SCENARIO_ID'] for s in SC]
check('scenario_ids_unique', [i for i, n in Counter(ids).items() if n > 1])
V5_FIELDS = ['SCENARIO_ID', 'SCENARIO_VERSION', 'GROUP', 'GROUPS_SERVED', 'CONTRACT_ASSIGNED_GROUPS', 'PURPOSE', 'MODE', 'RUN_GOVERNANCE', 'BUILD_UNDER_TEST',
             'RECORDED_NOT_ENFORCED', 'TUPLE_REQUIREMENTS', 'PLAN_OR_INPUT', 'FIXTURE', 'INPUT_STATUS', 'REACH', 'INJECTIONS', 'INJECTION_COORDINATE',
             'DESIGNED_STIMULUS', 'DELIBERATE_VIOLATION_FLAG', 'TARGET_NEGATIVE_CONTROL', 'EXPECTED_PRODUCT_OUTCOME', 'EXPECTED_CONTROL_RESULTS',
             'EXPECTED_INSTRUMENT_STATES', 'EXPECTED_GROUP_VERDICT', 'EXPECTED_EVENTS', 'EXPECTED_DETECTION_CHANNEL', 'EXPECTED_LOG_RECORD_SET',
             'EXPECTED_EVIDENCE_SET', 'EXPECTED_CLEANUP', 'RETRY_APPLICABILITY', 'REPETITION', 'CONTROLS_EXERCISED', 'CONTROL_CLASSES_EXERCISED',
             'EVIDENCE_GROUPS_DECLARED', 'EVM_VERSION', 'AUTHORITY_SOURCE_FOR_EXPECTED_RESULT', 'EXPECTED_PROCEDURAL_RESULT', 'CONDITIONAL_EXPECTATION_RULE',
             'PIN_SLOTS', 'RUN_PREREQUISITES', 'BLOCKED_BY', 'STATUS', 'SEALED_AT', 'NOTE']
check('v5_schema_fields_present', [s['SCENARIO_ID'] + ': missing ' + ','.join(f for f in V5_FIELDS if f not in s) + ' extra ' + ','.join(f for f in s if f not in V5_FIELDS)
                                   for s in SC if set(s) != set(V5_FIELDS)], 'every scenario has exactly the V5 schema fields (the V4 fields plus RUN_PREREQUISITES)')
check('scenario_version_5', [s['SCENARIO_ID'] for s in SC if s['SCENARIO_VERSION'] != '5-DRAFT'] + ([] if CAT['CATALOG_VERSION'] == '5-DRAFT' else ['CATALOG_VERSION']))
check('authority_source_present', [s['SCENARIO_ID'] for s in SC if not s['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']], 'V2.1 26.1: mandatory')
check('build_under_test_values', [s['SCENARIO_ID'] for s in SC if s['BUILD_UNDER_TEST'] not in (PB, CB, 'NONE')
                                  or (s['BUILD_UNDER_TEST'] == 'NONE') != (s['REACH'] == 'SYN')], 'NONE only for synthetic scenarios (REACH SYN)')

# ------------------------------------------------------------------------------------------------------------------
# 2. GROUPS_SERVED and CONTRACT_ASSIGNED_GROUPS: the normative definition of BA-04 section 1 (AR4-17), applied by BA-02b rule 1
# ------------------------------------------------------------------------------------------------------------------
sec1 = section(TXT[P_MD], '## 1. Schema', '### 1.1 ')
row_gs = [l for l in sec1.split('\n') if l.startswith('| `GROUPS_SERVED` |')]
row_cag = [l for l in sec1.split('\n') if l.startswith('| `CONTRACT_ASSIGNED_GROUPS` |')]
bad = []
if not (len(row_gs) == 1 and 'Normative definition of this artifact' in row_gs[0] and 'AR4-17' in row_gs[0] and 'AR2-08' in row_gs[0]
        and 'ordered union of AFFECTED_GROUPS(c)' in row_gs[0] and 'EVIDENCE_GROUPS_DECLARED' in row_gs[0] and 'nothing else' in row_gs[0]):
    bad.append('BA-04 md section 1: GROUPS_SERVED is not defined normatively there (AR4-17; AR2-08)')
if not (len(row_cag) == 1 and 'Normative definition of this artifact' in row_cag[0] and all(x in row_cag[0] for x in ('AR4-17', 'AR3-06', 'AR4-03', 'V2.1 10.8',
                                                                                                                     'V2.1 10.6 and 21.4', 'V2.2 PA-12', 'never PASS', 'N_strict'))):
    bad.append('BA-04 md section 1: CONTRACT_ASSIGNED_GROUPS is not defined normatively there with its admitted sources (AR4-17; AR3-06; AR4-03)')
gfd = CAT.get('GROUP_FIELD_DEFINITIONS', {})
if not (gfd.get('GROUPS_SERVED', '').split(': ', 1)[-1] in row_gs[0] if row_gs else False) or not (gfd.get('CONTRACT_ASSIGNED_GROUPS', '').split(': ', 1)[-1] in row_cag[0] if row_cag else False):
    bad.append('JSON GROUP_FIELD_DEFINITIONS differ from the md section 1 definitions')
if re.search(r'normative rule of BA-02b', TXT[P_MD] + TXT[P_JSON]):
    bad.append('BA-04 still takes the definition from BA-02b (cycle, R2:BA11V4-01)')
cov_rule1 = section(TXT[P_COV], '## 1. Normative derivation rules', '2. **Fail-closed').strip()
if (not cov_rule1.startswith('1. **Control to group (the definition of BA-04 section 1; AR4-17).**') or 'does not restate it' not in cov_rule1
        or re.search(r'union of `?AFFECTED_GROUPS', cov_rule1) or 'BA-04 V4 derives' in cov_rule1):
    bad.append('BA-02b rule 1 does not apply "the definition of BA-04 section 1", or restates the derivation')
check('group_fields_defined_in_ba04_section1_and_applied_by_ba02b', bad,
      'AR4-17: GROUPS_SERVED and CONTRACT_ASSIGNED_GROUPS are defined normatively in BA-04 section 1 (and the JSON); BA-02b rule 1 applies that definition; no BA-04 -> BA-02b edge')

bad, bad_cag, bad_cls, bad_ev = [], [], [], []
ADMITTED = {'x7': 'V2.1 10.8', 'res': 'V2.1 10.6 and 21.4', 'pa12': 'V2.2 PA-12', 'ks': 'AR4-03'}
for s in SC:
    der = []
    if len(set(s['CONTROLS_EXERCISED'])) != len(s['CONTROLS_EXERCISED']):
        bad.append(s['SCENARIO_ID'] + ': a control listed twice in CONTROLS_EXERCISED')
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
    sid = s['SCENARIO_ID']
    for cg in s['CONTRACT_ASSIGNED_GROUPS']:
        if set(cg) != {'group', 'clause'} or not cg['clause'] or cg['group'] not in CTRLG or cg['group'] in s['GROUPS_SERVED']:
            bad_cag.append(sid + ': ' + json.dumps(cg))
            continue
        # the admitted source must fit the scenario (R1:BA02V4-N01)
        if re.match(r'^WR-[A-G]-X7$', sid):
            want = 'x7'
        elif sid.startswith('WR-'):
            want = 'res'
        elif sid == 'PR-CLOSURE-COMPLETENESS':
            want = 'pa12'
        elif sid.startswith('KS-'):
            want = 'ks'
        else:
            want = None
        if want is None or ADMITTED[want] not in cg['clause'] or (want == 'ks' and not ('V2.1 26.3' in cg['clause'] and 'V2.1 31' in cg['clause'])):
            bad_cag.append(sid + ': clause does not cite the admitted source that fits: ' + cg['clause'][:80])
    if s['CONTRACT_ASSIGNED_GROUPS'] and 'never PASS' not in s['EXPECTED_GROUP_VERDICT']:
        bad_cag.append(sid + ': the group verdict does not mark the contract-assigned groups as evidence only, never PASS')
    cls = sorted({REG[c]['class'] for c in s['CONTROLS_EXERCISED'] if c in REG})
    if not s['CONTROLS_EXERCISED'] or s['EVIDENCE_GROUPS_DECLARED'] or s['CONTRACT_ASSIGNED_GROUPS']:
        cls = cls + ['NOT_A_CONTROL']
    if s['CONTROL_CLASSES_EXERCISED'] != cls:
        bad_cls.append(sid)
check('groups_served_equals_definition_of_ba04_section1', bad, 'GROUPS_SERVED == ordered union of AFFECTED_GROUPS(CONTROLS_EXERCISED) then EVIDENCE_GROUPS_DECLARED; nothing else')
check('evidence_groups_declared_are_evidence_kind', bad_ev)
check('contract_assigned_groups_admitted_sources', bad_cag,
      'each entry is {group, clause}, a CONTROL group, never in GROUPS_SERVED; the clause cites the admitted source that fits the scenario (V2.1 10.8 for X7 '
      'cells; V2.1 10.6 and 21.4 for residual cells; V2.2 PA-12 for closure completeness; AR4-03 with V2.1 26.3 and 31 for KS-*); the verdict says never PASS')
check('control_classes_derived', bad_cls)
check('group_is_served_or_contract_assigned', [s['SCENARIO_ID'] for s in SC if s['GROUP'] not in s['GROUPS_SERVED'] and s['GROUP'] not in
                                               [c['group'] for c in s['CONTRACT_ASSIGNED_GROUPS']] and s['GROUP'] != 'HDM' and s['SCENARIO_ID'] != 'EV-L-COMPOSED'],
      'the primary GROUP is served or contract-assigned; HDM (not a PA-1.3 group) and the non-governing composition check EV-L-COMPOSED (AR4-01) are the declared exceptions')
check('ar3_06_specific', [x for x, ok in [
    ('PW-CLONE-WRONG-TYPE exercises CLONE', 'CLONE' in BY['PW-CLONE-WRONG-TYPE']['CONTROLS_EXERCISED']),
    ('PR-CLOSURE-COMPLETENESS in Q with PR-REC contract-assigned', BY['PR-CLOSURE-COMPLETENESS']['GROUP'] == 'Q' and
     [c['group'] for c in BY['PR-CLOSURE-COMPLETENESS']['CONTRACT_ASSIGNED_GROUPS']] == ['PR-REC'] and 'PR-REC' not in BY['PR-CLOSURE-COMPLETENESS']['GROUPS_SERVED']),
    ('every X7 cell of writers A..G contract-assigns SC-B', all(any(c['group'] == 'SC-B' for c in BY['WR-%s-X7' % w]['CONTRACT_ASSIGNED_GROUPS']) for w in 'ABCDEFG')),
] if not ok])

# ------------------------------------------------------------------------------------------------------------------
# 3. the AR3-01 mode (ruled scope), EV-L-* (AR4-01) and the builds (AR4-06)
# ------------------------------------------------------------------------------------------------------------------
RNE = CAT['AR3_01_MODE']['RECORDED_NOT_ENFORCED']
check('ar3_01_set_exact', [] if RNE == AR301_SET else ['AR3_01_MODE set: ' + json.dumps(RNE)])
mode = [s for s in SC if s['RECORDED_NOT_ENFORCED'] == AR301_SET]
must_mode = sorted(i for i in ids if i.startswith(('E4-LEARN-R1-', 'LK-', 'HDM-CHAR-', 'WU-DRY-')) or i in Q14_VEHICLES)
bad = []
if sorted(s['SCENARIO_ID'] for s in mode) != must_mode:
    bad.append('the AR3-01 mode is not exactly the ruled scope: ' + json.dumps(sorted(set(s['SCENARIO_ID'] for s in mode) ^ set(must_mode))))
bad += ['vehicle missing: ' + v for v in Q14_VEHICLES if v not in BY]
for s in SC:
    if s['RECORDED_NOT_ENFORCED'] and s['RECORDED_NOT_ENFORCED'] not in (AR301_SET, EVL_SET):
        bad.append(s['SCENARIO_ID'] + ': RECORDED_NOT_ENFORCED is neither the AR3-01 set nor the EV-L set')
    if s['RECORDED_NOT_ENFORCED'] == EVL_SET and not s['SCENARIO_ID'].startswith('EV-L-'):
        bad.append(s['SCENARIO_ID'] + ': the EV-L set outside EV-L-*')
for s in mode:
    pins = s['PIN_SLOTS']
    if s['BUILD_UNDER_TEST'] != CB:
        bad.append(s['SCENARIO_ID'] + ': build')
    if s['EXPECTED_PRODUCT_OUTCOME'] != 'PROVISIONAL_NON_GOVERNING':
        bad.append(s['SCENARIO_ID'] + ': outcome')
    if s['EVM_VERSION'] != 'NONE' or any(p.startswith(('EVM_FROZEN', 'HOST_DEFAULT_MAP', 'E04_ADMITTED_SET')) for p in pins) or 'K-3' in s['BLOCKED_BY']:
        bad.append(s['SCENARIO_ID'] + ': depends on EVM, HOST_DEFAULT_MAP, E-04 or K-3')
    if 'CHARACTERIZATION_TUPLE' not in s['TUPLE_REQUIREMENTS']:
        bad.append(s['SCENARIO_ID'] + ': tuple does not bind the CHARACTERIZATION_BUILD profile')
    # a vehicle carries no blocker, except K-8 exactly when its fixture is of the FX-IMP family (Q-I14-P, the fixture of PR-REC-REOBS-MISMATCH)
    # and, for Q-I14-P, the open item AQ-V5-08 after it (its admission under the Q-I14-* pattern of AR4-01; correction pass (final))
    if s['SCENARIO_ID'] in Q14_VEHICLES and not ('AR4-01' in s['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT'] and s['RUN_GOVERNANCE'] == 'GOVERNING'
                                                 and s['BLOCKED_BY'] == (['K-8'] if s['FIXTURE'] in FXOPEN else [])
                                                 + (['AQ-V5-08'] if s['SCENARIO_ID'] in AQ508 else [])):
        bad.append(s['SCENARIO_ID'] + ': vehicle not ruled by AR4-01, not governing, or blocked beyond its fixture blocker')
    if s['RUN_GOVERNANCE'] != 'GOVERNING' and s['SCENARIO_ID'] in Q14_VEHICLES:
        bad.append(s['SCENARIO_ID'] + ': vehicle not governing')
    if s['SCENARIO_ID'] not in Q14_VEHICLES and s['RUN_GOVERNANCE'] != 'NON_GOVERNING_BY_DESIGN':
        bad.append(s['SCENARIO_ID'] + ': a governing scenario in the mode outside the Q-I14-* vehicles')
if any('PROPOSAL' in t or 'AQ-V4-01' in t for s in SC for t in strings(s)):
    bad.append('a scenario still carries a PROPOSAL / AQ-V4-01 label: ' + json.dumps([s['SCENARIO_ID'] for s in SC if any('PROPOSAL' in t or 'AQ-V4-01' in t for t in strings(s))][:10]))
check('provisional_outcome_only_in_ar3_01_mode', [s['SCENARIO_ID'] for s in SC if s['EXPECTED_PRODUCT_OUTCOME'].startswith('PROVISIONAL') and s['RECORDED_NOT_ENFORCED'] != AR301_SET])
check('ar3_01_mode_ruled_scope', bad, 'the AR3-01 mode applies exactly to E4-LEARN-R1-*, LK-*, HDM-CHAR-*, WU-DRY-* (non-governing) and the Q-I14-* vehicles (governing, AR4-01): '
      'CHARACTERIZATION_BUILD, PROVISIONAL_NON_GOVERNING, CHARACTERIZATION_TUPLE; no EVM, HOST_DEFAULT_MAP, E-04 pin or K-3; no proposal label remains')

bad = []
evl = [s for s in SC if s['SCENARIO_ID'].startswith('EV-L-')]
for s in evl:
    sid, r = s['SCENARIO_ID'], s['EXPECTED_CONTROL_RESULTS']
    comp = sid == 'EV-L-COMPOSED'
    if not (s['BUILD_UNDER_TEST'] == PB and s['RECORDED_NOT_ENFORCED'] == EVL_SET and s['EXPECTED_PRODUCT_OUTCOME'].startswith('NONE') and s['GROUP'] == 'E12-L'
            and 'AR4-01' in s['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT'] and set(s['BLOCKED_BY']) <= {'K-8'}
            and s['CONTROLS_EXERCISED'] == [] and 'harness DLL' in s['TUPLE_REQUIREMENTS'] and 'LOCK_MODE' in s['PIN_SLOTS']
            and 'BUILD_EQUIVALENCE_RECORD' in s['RUN_PREREQUISITES']):
        bad.append(sid + ': build, set, outcome, group, authority, controls, tuple driver DLL, LOCK_MODE pin or equivalence record')
    if not (r.get('E04', '').startswith('RECORDED_NOT_ENFORCED') and 'V2.1 3.3' in r.get('E04', '') and 'designated route' in r.get('E04', '')):
        bad.append(sid + ': the basis of E-04 (V2.1 3.3) is not stated')
    for k in ('E12A', 'E12C'):
        if not (r.get(k, '').startswith('RECORDED_NOT_ENFORCED') and all(x in r.get(k, '') for x in ('V2.1 27.2', '14.3', 'PA-2'))):
            bad.append(sid + ': the basis of ' + k + ' (V2.1 27.2, 14.3, V2.2 PA-2) is not stated')
    # correction pass: E-03 under its registry key, ENFORCED_NOT_EXERCISED (section 1), never the non-registry key of the V5 draft
    if (not r.get('E03', '').startswith('ENFORCED_NOT_EXERCISED') or 'enforced under the pinned LOCK_MODE' not in r.get('E03', '')
            or 'E-03 (enforced)' in r or 'E03' in s['CONTROLS_EXERCISED']
            or 'NOT_EVALUATED' not in r.get('S-1, S-2, S-3', '') or 'AR2-25' not in r.get('S-1, S-2, S-3', '')):
        bad.append(sid + ': E-03 not under the key E03 as ENFORCED_NOT_EXERCISED, or S-1..S-3 NOT_EVALUATED (AR2-25) not stated')
    if 'fail-closed' not in r.get('OTHER CONTROLS', '') or 'ENFORCED_NOT_EXERCISED' not in r.get('OTHER CONTROLS', ''):
        bad.append(sid + ': every other control fail-closed and ENFORCED_NOT_EXERCISED not stated')
    if 'REVIEWED_RECORD' not in s['RUN_PREREQUISITES']:
        bad.append(sid + ': the REVIEWED_RECORD (V2.4; BA-06 V5 section 9) is not named')
    if s['EXPECTED_LOG_RECORD_SET'] != ('LP-COMPOSE' if comp else 'LP-LEARN'):
        bad.append(sid + ': log profile')
    if comp and s['EXPECTED_PRODUCT_OUTCOME'] != 'NONE (a non-governing composition check driven by the micro-scenario driver: no mirror product run; AR4-01)':
        bad.append(sid + ': the outcome does not name the composition check (correction pass; AR4-01)')
    if comp:
        if not (s['RUN_GOVERNANCE'] == 'NON_GOVERNING_BY_DESIGN' and s['GROUPS_SERVED'] == [] and s['EVIDENCE_GROUPS_DECLARED'] == [] and s['MODE'] != 'LEARNING'
                and 'UNEXPLAINED_EVENTS' in s['PLAN_OR_INPUT'] and 'LEARNING_CORPUS_HASH' in s['PLAN_OR_INPUT'] and 'no model entry' in s['NOTE']
                and 'DRAFT or UNDER_REVIEW' in s['PLAN_OR_INPUT'] + s['PURPOSE']):
            bad.append(sid + ': not a non-governing composition check outside the learning corpus (AR4-01, BA06V4-N04)')
    elif not (s['RUN_GOVERNANCE'] == 'GOVERNING' and s['MODE'] == 'LEARNING' and s['GROUPS_SERVED'] == ['E12-L']):
        bad.append(sid + ': not a governing learning run of E12-L')
if sorted(s['SCENARIO_ID'] for s in evl if s['SCENARIO_ID'] != 'EV-L-COMPOSED') != sorted(i for i in ids if i.startswith('EV-L-OC-')):
    bad.append('EV-L ids')
esc = CAT.get('EV_L_SET', {})
if esc.get('RECORDED_NOT_ENFORCED') != EVL_SET or esc.get('BUILD_UNDER_TEST') != PB:
    bad.append('JSON EV_L_SET')
check('ev_l_fields_ar4_01', bad, 'AR4-01: EV-L-OC-* governing E12-L on the PRODUCT_BUILD, the driver a declared harness DLL; RECORDED_NOT_ENFORCED = E-04, E-12 phase A, '
      'E-12 phase C with the basis per control; E-03 enforced under LOCK_MODE; S-1..S-3 NOT_EVALUATED; outcome NONE; EV-L-COMPOSED non-governing, outside the corpus')

# per-build qualification twins (AR4-06 (3))
bad = []
srcs = [s for s in SC if s['SCENARIO_ID'].startswith(('Q-', 'E6-C')) and not s['SCENARIO_ID'].endswith('-CB')]
IGN = ('SCENARIO_ID', 'BUILD_UNDER_TEST', 'TUPLE_REQUIREMENTS', 'SEALED_AT', 'NOTE', 'PURPOSE')
for s in srcs:
    sid = s['SCENARIO_ID']
    twin = BY.get(sid + '-CB')
    if sid in Q14_FAMILY:
        if twin is not None or s['BUILD_UNDER_TEST'] != CB:
            bad.append(sid + ': the I-14 family runs only on the CHARACTERIZATION_BUILD, with no twin')
        continue
    if s['BUILD_UNDER_TEST'] != PB:
        bad.append(sid + ': the base scenario is not on the PRODUCT_BUILD')
    if twin is None:
        bad.append(sid + ': no -CB twin')
        continue
    if twin['BUILD_UNDER_TEST'] != CB:
        bad.append(sid + '-CB: build')
    diff = [k for k in s if k not in IGN and s[k] != twin[k]]
    if diff:
        bad.append(sid + '-CB: expectation differs in ' + ','.join(diff))
    for t, b in ((s, PB), (twin, CB)):
        tr = t['TUPLE_REQUIREMENTS']
        if 'BUILD_UNDER_TEST = ' + b not in tr or ('QUALIFICATION_TUPLE' not in tr and 'FPSPEC_CONFORMANCE_TUPLE' not in tr):
            bad.append(t['SCENARIO_ID'] + ': the qualification tuple does not name its build')
extra = [i for i in ids if i.endswith('-CB') and i[:-3] not in BY]
bad += ['orphan twin ' + i for i in extra]
exp_twins = sorted(s['SCENARIO_ID'] + '-CB' for s in srcs if s['SCENARIO_ID'] not in Q14_FAMILY)
if sorted(CAT['PER_BUILD_QUALIFICATION']['twins']) != exp_twins or CAT['PER_BUILD_QUALIFICATION']['characterization_build_only'] != Q14_FAMILY:
    bad.append('JSON PER_BUILD_QUALIFICATION')
# every instrument I-01..I-16 of V2.1 4.3 qualified on each build (I-14 only on the characterization build)
V21_43 = TXT[P_V21].split('### 4.3 Instrument qualification')[1].split('### 4.3.1')[0]
INSTR = [re.match(r'^\| (I-\d\d) ', l).group(1) for l in V21_43.split('\n') if re.match(r'^\| I-\d\d ', l)]
for inst in INSTR:
    qid = 'Q-' + inst.replace('-', '')
    blds = {BY[i]['BUILD_UNDER_TEST'] for i in (qid, qid + '-CB') if i in BY}
    if blds != ({CB} if inst == 'I-14' else {PB, CB}):
        bad.append(inst + ': qualified on ' + json.dumps(sorted(blds)))
check('per_build_qualification_twins', bad, 'AR4-06 (3): every Q-* and E6-C* outside the I-14 family has a twin <id>-CB on the CHARACTERIZATION_BUILD with the same '
      'expectation and a qualification tuple naming its build; the I-14 family runs only on the CHARACTERIZATION_BUILD; each instrument of V2.1 4.3 per build')

bad = []
bp = CAT['BUILD_PROFILES']
ct = bp.get(CB, '')
for x in ('one admissible BUILD_BOUND profile per BUILD_UNDER_TEST value', 'Plugin DLL SHA-256', 'harness DLL SHA-256', 'manifest build layer', 'E-10',
          'WARMUP_MEMBERSHIP', 'warm-up definition', 'qualification records of that build', 'CATALOG_FOLDER of the build under test'):
    if x not in ct:
        bad.append('CHARACTERIZATION_TUPLE lacks: ' + x)
if 'every other field as in FULL_BASELINE_TUPLE' in TXT[P_JSON] + TXT[P_MD]:
    bad.append('CHARACTERIZATION_TUPLE still copies the product build fields')
for s in SC:
    tr = s['TUPLE_REQUIREMENTS']
    if s['BUILD_UNDER_TEST'] == CB and s['FIXTURE'] != 'NONE' and not ('CHARACTERIZATION_TUPLE' in tr):
        bad.append(s['SCENARIO_ID'] + ': a characterization-build host run not bound to CHARACTERIZATION_TUPLE')
    if s['BUILD_UNDER_TEST'] == PB and s['FIXTURE'] != 'NONE' and 'CHARACTERIZATION_TUPLE' in tr:
        bad.append(s['SCENARIO_ID'] + ': a product-build run bound to CHARACTERIZATION_TUPLE')
sec11 = section(TXT[P_MD], '### 1.1 ', '### 1.2 ')
for x in ('AR4-06 (1)', 'AR4-06 (3)', 'AR4-06 (4)', 'AR4-06 (6)', 'BUILD_EQUIVALENCE_RECORD', 'CATALOG_FOLDER'):
    if x not in sec11:
        bad.append('md 1.1 does not state ' + x)
check('characterization_tuple_profile_per_build', bad, 'AR4-06 (1), (2): one admissible BUILD_BOUND profile per build; CHARACTERIZATION_TUPLE with its own build-dependent fields')

bad = []
EQUIV = ('E04_ADMITTED_SET@R-1', 'LOCK_MODE', 'HOST_DEFAULT_MAP')
VOC = CAT['RUN_PREREQUISITE_VOCABULARY']
BE_POINTER = ('location, content schema (recordKind CT21D_BUILD_EQUIVALENCE) and custody: BA-07 V5 sections 2, 4 and 5.3; use precondition: '
              'BA-07 V5 section 7, U5')
# the in-catalog variant of CP-09 (its own path and field names, and the file-hash custody) superseded by the pointer: absent from both files
BE_SUPERSEDED = ('ct21d/review-records/', 'sourceSha', 'pluginDllSha256', 'buildReceiptSha256', 'switchSet', 'buildId', 'harnessDlls [{name',
                 'subjectHash = the file hash', 'which gives its location, content and custody', 'the review-record location of BA-07 V5')
for s in SC:
    pre = s['RUN_PREREQUISITES']
    need = s['BUILD_UNDER_TEST'] == PB and any(p in EQUIV for p in s['PIN_SLOTS'])
    if need != ('BUILD_EQUIVALENCE_RECORD' in pre):
        bad.append(s['SCENARIO_ID'] + ': BUILD_EQUIVALENCE_RECORD ' + ('missing' if need else 'where no characterization-derived pin is consumed on the product build'))
    for p in pre:
        if p not in VOC:
            bad.append(s['SCENARIO_ID'] + ': ' + p + ' not in RUN_PREREQUISITE_VOCABULARY')
    rule = s['CONDITIONAL_EXPECTATION_RULE']
    if s['MODE'] == 'DRY_RUN':
        continue
    coord = s['INJECTION_COORDINATE']
    if any(r in rule for r in ('CER-WIT', 'CER-A2', 'CER-PERSIST')):
        want = 'QUALIFICATION_RECORD@Q-I14-Y' if coord.startswith('Y') else 'QUALIFICATION_RECORD@Q-I14-X'
        if want not in pre:
            bad.append(s['SCENARIO_ID'] + ': ' + want + ' missing')
    if 'CER-SIL' in rule:
        want = 'QUALIFICATION_RECORD@Q-I14-SILENT-Y' if coord.startswith('Y') else 'QUALIFICATION_RECORD@Q-I14-SILENT-X'
        if want not in pre:
            bad.append(s['SCENARIO_ID'] + ': ' + want + ' missing')
for i in ('NC-E12A', 'NC-S5-O6'):
    if 'QUALIFICATION_RECORD@Q-I14-Y' not in BY[i]['RUN_PREREQUISITES']:
        bad.append(i + ': the Q-I14-Y record (XDATA_WRITE / Wr-OCT at Y1) is not named')
# correction pass: every governing scenario that injects an I-14 writer names the record of the vehicle that qualifies it at its coordinate
I14W = ('Wr-A', 'Wr-B', 'Wr-C', 'Wr-D', 'Wr-E', 'Wr-F', 'Wr-G', 'Wr-A..Wr-G', 'Wr-OCT', 'XDATA_WRITE')


def i14_rec(writer, pos):
    if writer == 'Wr-S':
        return 'QUALIFICATION_RECORD@Q-I14-SILENT-' + ('Y' if pos.startswith('Y') else 'X')
    if writer not in I14W:
        return None
    if pos.startswith('X'):
        return 'QUALIFICATION_RECORD@Q-I14-X'
    if pos.startswith('Y'):
        return 'QUALIFICATION_RECORD@Q-I14-Y'
    return 'QUALIFICATION_RECORD@Q-I14-P' if pos == 'between PR and PW' else 'UNQUALIFIED_COORDINATE:' + pos


for s in SC:
    if s['RUN_GOVERNANCE'] != 'GOVERNING' or s['MODE'] == 'DRY_RUN' or s['SCENARIO_ID'] in Q14_FAMILY:
        continue
    for inj_ in s['INJECTIONS']:
        rec = i14_rec(inj_['writer'], inj_['position'])
        if rec and rec not in s['RUN_PREREQUISITES']:
            bad.append(s['SCENARIO_ID'] + ': injects ' + inj_['writer'] + ' at ' + inj_['position'] + ' without ' + rec)
for w in 'ADEFG':
    for x in (0, 7, 8):
        if BY['WR-%s-X%d' % (w, x)]['RUN_PREREQUISITES'] != ['QUALIFICATION_RECORD@Q-I14-X']:
            bad.append('WR-%s-X%d: QUALIFICATION_RECORD@Q-I14-X' % (w, x))
if BY['PR-REC-REOBS-MISMATCH']['RUN_PREREQUISITES'] != ['QUALIFICATION_RECORD@Q-I14-P']:
    bad.append('PR-REC-REOBS-MISMATCH: QUALIFICATION_RECORD@Q-I14-P')
# correction pass: the REVIEWED_RECORD exactly on the EV-L-* scenarios; the vocabulary and the citation rule
for s in SC:
    if ('REVIEWED_RECORD' in s['RUN_PREREQUISITES']) != s['SCENARIO_ID'].startswith('EV-L-'):
        bad.append(s['SCENARIO_ID'] + ': REVIEWED_RECORD')
for k, need in (('REVIEWED_RECORD', ('V2.4', 'BA-06 V5 section 9', 'REVIEW_RECORD_ADDED', 'runStartAnchor', 'REVIEWED_REF')),
                ('QUALIFICATION_RECORD@Q-I14-P', ('Q-I14-P', 'between PR and PW', 'PR-REC-REOBS-MISMATCH')),
                ('BUILD_EQUIVALENCE_RECORD', ('section 1.1', BE_POINTER))):
    if k not in VOC or not all(x in VOC[k] for x in need):
        bad.append('RUN_PREREQUISITE_VOCABULARY ' + k)
# correction pass (final): the record's use precondition is BA-07 V5 U5, never U1 (U1 stays for the REVIEWED_RECORD)
if 'BUILD_EQUIVALENCE_RECORD' in VOC and ('U1' in VOC['BUILD_EQUIVALENCE_RECORD'] or 'which gives its location' in VOC['BUILD_EQUIVALENCE_RECORD']):
    bad.append('RUN_PREREQUISITE_VOCABULARY BUILD_EQUIVALENCE_RECORD cites U1 or the superseded in-catalog definition')
rpr = CAT.get('RUN_PREREQUISITE_RULE', '')
if not all(x in rpr for x in ('RUN_START', 'QUAL_START', 'SHA-256', 'REVIEWED_REF', 'AR4-06 (4)', 'V2.4', 'V2.1 4.3', 'REVIEW_RECORD_ADDED',
                              'run-start anchor', 'for the BUILD_EQUIVALENCE_RECORD, the use precondition of BA-07 V5 section 7, U5',
                              'for the REVIEWED_RECORD, BA-07 V5 sections 5.1, 6.1 and 7, U1')):
    bad.append('JSON RUN_PREREQUISITE_RULE')
if 'RUN_PREREQUISITE_RULE' not in section(TXT[P_MD], '## 4. ', '## 5. ') or rpr not in TXT[P_MD]:
    bad.append('md section 4 does not state the RUN_PREREQUISITE_RULE')
be = CAT['BUILD_EQUIVALENCE']
# correction pass (final; re-check MAJOR): BA-07 V5 is the single authority for the record; the catalog points to it and keeps its own rules
BE_OWN = ('E04_ADMITTED_SET@R-1', 'LOCK_MODE', 'HOST_DEFAULT_MAP', 'apply to a PRODUCT_BUILD run only under this record', 'E4-VAL-R1',
          'names the record in RUN_PREREQUISITES', 'cites the record hash (BA-07 V5 section 4) in its RUN_START record', 'RUN_PREREQUISITE_RULE',
          'not a pin, not a tuple field and not a seal blocker', 'single authority')
if (BE_POINTER not in be or not all(x in be for x in BE_OWN) or be not in section(TXT[P_MD], '### 1.1 ', '### 1.2 ')
        or be not in section(TXT[P_MD], '## 4. ', '## 5. ')):
    bad.append('BUILD_EQUIVALENCE: the pointer to BA-07 V5 (sections 2, 4, 5.3; section 7, U5) or the catalog\'s own rules missing, or md 1.1 or '
               'section 4 differ (correction pass (final); AR4-06 (4))')
# the superseded in-catalog variant (CP-09: its own path, field set and use rule) must not survive in either file
for p_ in (P_JSON, P_MD):
    for tok in BE_SUPERSEDED:
        if tok in TXT[p_]:
            bad.append(p_.split('/')[-1] + ': the superseded in-catalog record text survives ("' + tok + '")')
step6 = [l for l in section(TXT[P_MD], '### 2.2 ', '## 3. ').split('\n') if l.startswith('6. the `BUILD_EQUIVALENCE_RECORD`')]
if len(step6) != 1 or 'BA-07 V5 section 7, U5' not in step6[0] or 'U1' in step6[0]:
    bad.append('md 2.2 step 6 does not cite BA-07 V5 section 7, U5 for the record')
# the sections pointed to hold the record in BA-07 V5 (read-only input; the catalog restates none of it)
b7 = TXT[P_BA07]
b7_2 = section(b7, '## 2. What is stored where', '## 3. ')
b7_4 = section(b7, '## 4. Canonical serialization', '### 4.1 ')
b7_53 = section(b7, '### 5.3 Build-equivalence record', '## 6. ')
b7_7 = section(b7, '## 7. Roles and transitions', '### 7.1 ')
for ok_, what in ((('build-equivalence records (section 5.3' in b7_2 and 'build-equivalence/equivalence-<recordHash>.json' in b7_2), 'section 2 row'),
                  (('build-equivalence records' in b7_4 and 'the build-equivalence record hash' in b7_4), 'section 4'),
                  (all(x in b7_53 for x in ('`"CT21D_BUILD_EQUIVALENCE"`', '`productBuild`', '`characterizationBuild`', '`switchAssemblies`',
                                            '**Custody.**', '`REVIEW_RECORD_ADDED`', '**Use** (section 7, U5)')), 'section 5.3'),
                  (all(x in b7_7 for x in ('**U1, run-start anchor.**', '**U2, manifest.**', '**U3, pins.**', '**U4, no pending closure**',
                                           '**U5, build equivalence**')), 'section 7, U1-U5')):
    if not ok_:
        bad.append('BA-07 V5 ' + what + ' does not hold what the catalog points to')
if 'it is recorded before the first PRODUCT_BUILD run that consumes such a pin' in TXT[P_JSON] + TXT[P_MD]:
    bad.append('BUILD_EQUIVALENCE keeps the V5 draft timing without custody')
check('run_prerequisites_equivalence_and_qualification', bad,
      'AR4-06 (4): BUILD_EQUIVALENCE_RECORD exactly on the PRODUCT_BUILD scenarios that consume E04_ADMITTED_SET, LOCK_MODE or HOST_DEFAULT_MAP; every I-14 record '
      'a rule resolves from is named (Q-I14-X, Q-I14-Y, Q-I14-SILENT-*); NC-E12A and NC-S5-O6 name Q-I14-Y (AR4-01, AR4-04, AR4-16); correction pass: every '
      'governing I-14 writer injection names its vehicle\'s record (X: Q-I14-X, Y: Q-I14-Y, between PR and PW: Q-I14-P, silent: Q-I14-SILENT-*); the '
      'REVIEWED_RECORD exactly on EV-L-*; the RUN_PREREQUISITE_RULE; correction pass (final): the BUILD_EQUIVALENCE_RECORD by pointer to '
      'BA-07 V5 (location, content schema and custody: sections 2, 4 and 5.3; use precondition: section 7, U5), no superseded in-catalog '
      'path or field names, U5 in the vocabulary, the citation rule and step 6 of 2.2, and BA-07 V5 holds what is pointed to')
check('i14_records_only_on_characterization_build', [s['SCENARIO_ID'] for s in SC if re.search(r'CER-(WIT|SIL|A2|PERSIST)', s['CONDITIONAL_EXPECTATION_RULE'])
                                                      and s['BUILD_UNDER_TEST'] != CB] + [i for i in ('EA2-DEFERRED-EVENTS',) if BY[i]['BUILD_UNDER_TEST'] != CB],
      'V2.1 4.3 row I-14 and AR4-06 (6): a rule that resolves from an I-14 record is used only on the CHARACTERIZATION_BUILD; EA2-DEFERRED-EVENTS runs there')

# ------------------------------------------------------------------------------------------------------------------
# 4. WU-DRY (AR3-02, AR3-24), branch switches (AR4-02), load exclusion (AR4-16)
# ------------------------------------------------------------------------------------------------------------------
WINDOW = ('P0', 'PL', 'PR', 'PW', 'PS', 'PV', 'CP')


def enters_window(s):
    """The one rule of BA-09 section 4 (AR3-02): GOVERNING, MODE CHARACTERIZATION_RUN or VALIDATION, REACH in P0..CP."""
    return s['RUN_GOVERNANCE'] == 'GOVERNING' and s['MODE'] in ('CHARACTERIZATION_RUN', 'VALIDATION') and s['REACH'] in WINDOW


def branch_source(s):
    return s['TARGET_NEGATIVE_CONTROL'] in RNE_KEY and bool(re.search(r'\bO[12]\b', s['EXPECTED_PRODUCT_OUTCOME']))


def is_load(i):
    return i['writer'] == 'CANARY_LOAD' or (i['writer'] == 'HARNESS_FAULT' and 'foreign warm-up load' in i['variant'])


def src_log(src):
    lg = src['EXPECTED_LOG_RECORD_SET']
    return 'LP-DRY over ' + (('(' + lg + ')') if ' + ' in lg else lg)


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
    exp_block = [b for b in src['BLOCKED_BY'] if b in ('K-8', 'AQ-V5-08')]
    exp_inj = src['INJECTIONS'] + ([d['INJECTIONS'][-1]] if branch_source(src) and d['INJECTIONS'] else [])
    if not (d['MODE'] == 'DRY_RUN' and d['RUN_GOVERNANCE'] == 'NON_GOVERNING_BY_DESIGN' and d['REACH'] == src['REACH'] and d['FIXTURE'] == src['FIXTURE']
            and src['FIXTURE'] != 'NONE' and d['PIN_SLOTS'] == exp_slots and d['REPETITION'] == 'DRY: WU_DRY_RUNS = N_B'
            and d['DELIBERATE_VIOLATION_FLAG'] == 'NO' and d['INJECTIONS'] == exp_inj and d['RECORDED_NOT_ENFORCED'] == AR301_SET
            and d['BLOCKED_BY'] == exp_block and d['EXPECTED_LOG_RECORD_SET'] == src_log(src) and d['BUILD_UNDER_TEST'] == CB):
        bad.append('WU-DRY-' + x + ': fields differ from the rule')
for i in ids:
    if i.startswith('WU-DRY-WU-DRY-'):
        bad.append(i)
sec52 = section(TXT[P_MD], '### 5.2 ', '## 6. ')
if 'outside the AR3-01 mode' in sec52:
    bad.append('md 5.2 keeps the qualifier "outside the AR3-01 mode" that BA-09 section 4 does not have')
if not all(x in sec52 for x in ('`RUN_GOVERNANCE = GOVERNING`', '`CHARACTERIZATION_RUN` or `VALIDATION`', '`REACH` in `P0`..`CP`', '`REACH = NONE`',
                                'its variant library (`FIXTURE_INSTANCE@L-VAR-*`)')):
    bad.append('md 5.2 does not state the one rule of BA-09 section 4 and the PIN_SLOTS wording')
check('wu_dry_exactly_one_per_governing_window_scenario', bad,
      'the one rule of BA-09 section 4: every GOVERNING scenario of MODE CHARACTERIZATION_RUN or VALIDATION with REACH in P0..CP has exactly one WU-DRY-<id>; '
      'no other scenario has one; a dry run carries only the FIXTURE_INSTANCE slots of its source, WU_DRY_RUNS = N_B, the source injections (plus the branch '
      'switch where AR4-02 applies), the AR3-01 set, the source blockers K-8 and AQ-V5-08, the source REACH and "LP-DRY over" the source log')
check('wu_dry_for_the_named_scenarios', [i for i in ('U-1', 'U-2', 'U-3', 'U-4', 'U-5', 'U-7', 'U-8', 'U-6b', 'S1-SAVE-REOPEN', 'PP-POSTCP', 'CL-CLEANUP-CHECK',
                                                      'CL-CLEAN-FXANNBLANK-FP', 'NC-CLONE-REPLACE', 'Q-I14-Y', 'Q-I14-X', 'Q-I14-SILENT-Y', 'Q-I14-SILENT-X',
                                                      'Q-I14-P')
                                         if 'WU-DRY-' + i not in BY])
bad = []
exp_branch = sorted(x for x in dry if branch_source(BY[x]))
if exp_branch != sorted(['NC-E12A', 'NC-E04-R2', 'NC-E03-NOT-HELD', 'NC-E03-REACQUIRE', 'NC-S1-Y1', 'NC-S1-Y1-SILENT']):
    bad.append('the recomputed branch sources are not the six of AR4-02: ' + json.dumps(exp_branch))
m = re.search(r'\*\*Branches taken by switch \(AR4-02\)\.\*\*.*? This concerns (.*?)\. For', sec52, re.S)
if not m or sorted(ids_in(m.group(1))) != exp_branch:
    bad.append('md 5.2 does not list exactly the branch sources')
if sorted(CAT['DRY_RUN_RULES']['branch_sources']) != exp_branch:
    bad.append('JSON DRY_RUN_RULES.branch_sources')
for x, d in dry.items():
    sw = [i for i in d['INJECTIONS'] if i['writer'] == 'DRY_RUN_BRANCH_SWITCH']
    if x in exp_branch:
        tname = RNE_KEY[BY[x]['TARGET_NEGATIVE_CONTROL']]
        if not (len(sw) == 1 and d['INJECTIONS'][-1] == sw[0] and 'DRY_RUN_BRANCH_SWITCH' in d['PLAN_OR_INPUT'] and 'AR4-02' in d['PLAN_OR_INPUT']
                and 'records ' + tname in sw[0]['variant'] and d['REACH'] == BY[x]['REACH'] and d['EXPECTED_LOG_RECORD_SET'] == src_log(BY[x])):
            bad.append('WU-DRY-' + x + ': the switch is not named in PLAN_OR_INPUT and INJECTIONS, or REACH / profile differ from the source')
        if x == 'NC-E12A' and not ('abort at PV' in sw[0]['variant'] and 'commits' in sw[0]['variant']):
            bad.append('WU-DRY-NC-E12A: the switch does not follow both CER-WIT branches of its source')
    elif sw:
        bad.append('WU-DRY-' + x + ': a branch switch on a dry run whose source refuses by an enforced control')
    if 'does not take that branch' in d['PLAN_OR_INPUT']:
        bad.append('WU-DRY-' + x + ': still "does not take that branch"')
if 'BRANCH_SWITCH' not in CAT['LOG_PROFILES']['LP-DRY'] or 'does not take the refusal' in CAT['LOG_PROFILES']['LP-DRY']:
    bad.append('LP-DRY does not state the switch')
check('dry_run_branch_switches_ar4_02', bad, 'AR4-02: the dry run of each source whose refusal or abort comes from a RECORDED_NOT_ENFORCED control takes the declared '
      'branch at the same checkpoint through the DRY_RUN_BRANCH_SWITCH, named in PLAN_OR_INPUT and INJECTIONS; REACH and profile as the source; no other dry run has it')
bad = []
exp_load = sorted(x for x in dry if any(is_load(i) for i in BY[x]['INJECTIONS']))
if sorted(CAT['DRY_RUN_RULES']['load_sources']) != exp_load or not exp_load:
    bad.append('JSON DRY_RUN_RULES.load_sources ' + json.dumps(exp_load))
for s in SC:
    for i in s['INJECTIONS']:
        if is_load(i) and 'declared identity' not in i['variant']:
            bad.append(s['SCENARIO_ID'] + ': a load injection without a declared identity')
for x, d in dry.items():
    txt = d['PLAN_OR_INPUT']
    has = all(k in txt for k in ('excluded from the measurement', 'STIMULUS_LOAD', 'never preloads', 'declared identity', 'AR4-16'))
    if x in exp_load and not (has and 'excluded stimulus loads' in d['EXPECTED_CONTROL_RESULTS']['E11M']):
        bad.append('WU-DRY-' + x + ': the stimulus loads are not excluded and recorded apart')
    if x not in exp_load and ('STIMULUS_LOAD' in txt or 'excluded stimulus' in d['EXPECTED_CONTROL_RESULTS']['E11M']):
        bad.append('WU-DRY-' + x + ': an exclusion without a designed load injection')
m = re.search(r'\*\*Designed load injections \(AR4-16\)\.\*\*(.*?)This concerns (.*?)\.\n', sec52, re.S)
if not m or sorted(ids_in(m.group(2))) != exp_load or 'never preloads an assembly named by an' not in m.group(1):
    bad.append('md 5.2 does not state the AR4-16 rule and its sources')
if 'STIMULUS_LOAD' not in CAT['LOG_PROFILES']['LP-DRY']:
    bad.append('LP-DRY has no STIMULUS_LOAD record')
check('dry_run_canary_load_exclusion_ar4_16', bad, 'AR4-16: the dry runs of the sources with a designed load injection (CANARY_LOAD, foreign-load HARNESS_FAULT) exclude the '
      'loads of the named assembly or module (declared identity), record them as STIMULUS_LOAD, count every other load; a warm-up revision never preloads it')

# ------------------------------------------------------------------------------------------------------------------
# 5. flags, targets, designed stimuli (AR3-05) and conditional negative controls (AR4-16, AR4-20 (d))
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
check('yes_target_result_names_the_failure', [s['SCENARIO_ID'] for s in SC if s['DELIBERATE_VIOLATION_FLAG'] == 'YES' and s['TARGET_NEGATIVE_CONTROL'] in REG
                                              and not any(k in s['EXPECTED_CONTROL_RESULTS'].get(s['TARGET_NEGATIVE_CONTROL'], '') for k in ('FAIL', 'UNKNOWN', 'difference', 'REJECT'))],
      'AR2-09: the target control\'s expected result names its FAIL, UNKNOWN, difference or rejection')
check('conditional_e12c_cells_target_e12c', [s['SCENARIO_ID'] for s in SC if s['SCENARIO_ID'].startswith('WR-')
                                              and s['EXPECTED_CONTROL_RESULTS'].get('E12C', '').startswith('CONDITIONAL (CER-WIT): FAIL when')
                                              and s['TARGET_NEGATIVE_CONTROL'] != 'E12C'],
      'AR3-05: a scan cell whose detection is conditional on E-12 phase C (not a secondary channel) targets E12C')


def conditional(s, c):
    return s['EXPECTED_CONTROL_RESULTS'].get(c, '').startswith('CONDITIONAL') or 'CER-SIL' in s['CONDITIONAL_EXPECTATION_RULE'] or 'CER-PERSIST' in s['CONDITIONAL_EXPECTATION_RULE']


bad = []
for s in SC:
    t = s['TARGET_NEGATIVE_CONTROL']
    if s['DELIBERATE_VIOLATION_FLAG'] != 'YES' or t not in REG or not conditional(s, t):
        continue
    v, r, rule = s['EXPECTED_GROUP_VERDICT'], s['EXPECTED_CONTROL_RESULTS'][t], s['CONDITIONAL_EXPECTATION_RULE']
    if not ('NEGATIVE_CONTROL_NOT_EVALUATED' in v or 'stays uncovered' in v) or not ('detection branch' in v or 'RUNNABLE' in v) or 'AR4-20 (d)' not in v:
        bad.append(s['SCENARIO_ID'] + ': the group verdict is not branch-dependent (AR4-20 (d))')
    if r.startswith('CONDITIONAL (CER-WIT)') and not ('otherwise PASS' in r and 'NOT_RUNNABLE' in r):
        bad.append(s['SCENARIO_ID'] + ': the non-detection branch of ' + t + ' does not follow the one convention (PASS, NOT_RUNNABLE as a negative control)')
    if 'CER-PERSIST' in rule and not ('otherwise PASS' in r and 'NOT_RUNNABLE' in r):
        bad.append(s['SCENARIO_ID'] + ': the non-persisted branch of S-5 is not stated')
cw = CAT['CONDITIONAL_EXPECTATION_RULES']['CER-WIT']
if not all(x in cw for x in ('One convention', 'PASS', 'NOT_RUNNABLE as a negative control', 'no designed detection', 'NEGATIVE_CONTROL_NOT_EVALUATED', 'XDATA_WRITE')):
    bad.append('CER-WIT does not state the single convention or the XDATA_WRITE qualification')
check('conditional_negative_controls_branch_dependent', bad, 'AR4-16, AR4-20 (d), R1:BA02V4-N02: a conditional negative control gives a designed detection only on its detection '
      'branch; the target result on the non-detection branch is PASS with the scenario NOT_RUNNABLE as a negative control')
n = BY['NC-E12A']
e = n['EXPECTED_CONTROL_RESULTS'].get('E12A', '')
check('nc_e12a_conditional_by_cer_wit', [x for x, ok in [
    ('E12A conditional by CER-WIT', e.startswith('CONDITIONAL (CER-WIT): FAIL') and 'otherwise PASS' in e and 'NOT_RUNNABLE' in e),
    ('outcome: delivered => abort, O2/O6 by CER-ABV; not delivered => NOT_RUNNABLE, commits (O3)', n['EXPECTED_PRODUCT_OUTCOME'].startswith('CONDITIONAL (CER-WIT)')
     and all(x in n['EXPECTED_PRODUCT_OUTCOME'] for x in ('aborts before Commit()', 'O2 or O6 by CER-ABV', 'NOT_RUNNABLE', 'commits (O3', 'NEGATIVE_CONTROL_NOT_EVALUATED'))),
    ('rule field', n['CONDITIONAL_EXPECTATION_RULE'].startswith('CER-WIT') and 'CER-ABV' in n['CONDITIONAL_EXPECTATION_RULE']),
    ('REACH CP and the log by branch', n['REACH'] == 'CP' and n['EXPECTED_LOG_RECORD_SET'].startswith('LP-PV (delivery branch') and 'LP-CP (non-delivery branch' in n['EXPECTED_LOG_RECORD_SET']),
    ('XDATA_WRITE at Y1, qualified by Q-I14-Y', [i['writer'] for i in n['INJECTIONS']] == ['XDATA_WRITE'] and n['INJECTION_COORDINATE'] == 'Y1'
     and 'QUALIFICATION_RECORD@Q-I14-Y' in n['RUN_PREREQUISITES'] and 'AR4-16' in n['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']),
    ('Q-I14-Y qualifies XDATA_WRITE and Wr-OCT at Y1', [i['writer'] for i in BY['Q-I14-Y']['INJECTIONS']] == ['Wr-A..Wr-G', 'Wr-OCT', 'XDATA_WRITE']
     and all(i['position'] == 'Y1' for i in BY['Q-I14-Y']['INJECTIONS'][1:]) and 'post-abort read' in BY['Q-I14-Y']['PLAN_OR_INPUT']),
] if not ok], 'AR4-16 (R1:BA04V4-04, R2:BA04V4-05): NC-E12A conditional by CER-WIT; the witness qualified for XDATA_WRITE at Y1')

# ------------------------------------------------------------------------------------------------------------------
# 6. SEALED_AT (AR3-25; R1:BA04V4-10, R2:BA04V4-09)
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
check('sealed_at_fields', [f for f in fields if f not in V5_FIELDS or f in ('SEALED_AT', 'STATUS', 'GROUPS_SERVED', 'CONTROL_CLASSES_EXERCISED')] +
      [f for f in ('GROUP', 'EVIDENCE_GROUPS_DECLARED', 'CONTRACT_ASSIGNED_GROUPS', 'REPETITION', 'RETRY_APPLICABILITY', 'RUN_PREREQUISITES', 'EXPECTED_PRODUCT_OUTCOME',
                   'EXPECTED_CONTROL_RESULTS', 'CONTROLS_EXERCISED', 'PIN_SLOTS', 'TUPLE_REQUIREMENTS', 'BUILD_UNDER_TEST', 'RECORDED_NOT_ENFORCED') if f not in fields],
      'the SEALED_AT fields are schema fields, exclude the derived ones, and include the group-attribution and repetition fields (R1:BA04V4-10, R2:BA04V4-09)')

# ------------------------------------------------------------------------------------------------------------------
# 7. outcomes, conditional rules, pins, blockers, repetition, log deltas
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
    gov_window = host and s['RUN_GOVERNANCE'] == 'GOVERNING' and s['RECORDED_NOT_ENFORCED'] != AR301_SET and s['REACH'] in WINDOW
    pre_baseline = s['EXPECTED_LOG_RECORD_SET'].startswith('LP-P0-STEP')   # refused at V2.1 13.1 step 3 or 4 (R2:BA04V4-04)
    idx = ORDER.index(s['REACH'])
    exp = (['FIXTURE_INSTANCE@' + s['FIXTURE']] if host else [])
    exp += [p for p in s['PIN_SLOTS'] if p.startswith('FIXTURE_INSTANCE@L-VAR')]
    if gov_window and not pre_baseline:
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
check('pin_slots_rule', bad_pin, 'section 2.1 of BA-04 V5 (E-04 pin only where the run reaches the P0 control evaluation)')
check('k3_exactly_where_s1_s3_are_adjudicated', bad_k3, 'governing host scenario outside the AR3-01 mode with REACH PV or CP')
check('k8_exactly_on_fx_imp_family', bad_k8)
VOCAB = ('K-3', 'K-8', 'AQ-V5-08')
check('blocked_by_values', [s['SCENARIO_ID'] + ': ' + b for s in SC for b in s['BLOCKED_BY'] if b not in VOCAB] +
      ([] if list(CAT['BLOCKED_BY_VOCABULARY']) == list(VOCAB) else ['JSON BLOCKED_BY_VOCABULARY ' + json.dumps(list(CAT['BLOCKED_BY_VOCABULARY']))]),
      'BLOCKED_BY uses only K-3, K-8 and AQ-V5-08 (the open item on the admission of Q-I14-P; correction pass (final)): AQ-V4-01, AQ-V4-03, K-6 '
      'and E6-C4 are ruled (AR4-01, AR4-03, AR4-07, AR4-04)')
check('aq_v5_08_exactly_on_q_i14_p_and_its_dry_run',
      [s['SCENARIO_ID'] for s in SC if ('AQ-V5-08' in s['BLOCKED_BY']) != (s['SCENARIO_ID'] in AQ508_ALL)] +
      [i + ': ' + json.dumps(BY[i]['BLOCKED_BY']) + ' ' + BY[i]['STATUS'] for i in AQ508_ALL
       if i not in BY or BY[i]['BLOCKED_BY'] != ['K-8', 'AQ-V5-08'] or BY[i]['STATUS'] != 'DRAFT'] +
      [x for x in ('AQ-V5-08',) if x not in BY['Q-I14-P']['NOTE'] or x not in CAT['AR3_01_MODE']['ruled_for']],
      'correction pass (final): AQ-V5-08 is carried by Q-I14-P and WU-DRY-Q-I14-P only, after K-8 (status DRAFT), and the Q-I14-P NOTE and '
      'AR3_01_MODE.ruled_for name it')
bad = []
brow = [l for l in sec1.split('\n') if l.startswith('| `BLOCKED_BY` |')]
if len(brow) != 1 or not all('`' + v + '`' in brow[0] for v in VOCAB) or re.search(r'AQ-V4|K-6|E6-C4', brow[0]):
    bad.append('md section 1: the BLOCKED_BY row is not exactly the K-3/K-8/AQ-V5-08 vocabulary')
sec71 = section(TXT[P_MD], '### 7.1 ', '### 7.2 ')
c_rows = [r[0] for r in table_after(TXT[P_MD], '### 7.1 ', '### 7.2 ')]
if c_rows != ['C-1', 'C-2', 'C-3'] or 'Remaining seal blockers: K-3 and K-8' not in sec71:
    bad.append('md 7.1 is not only K-3, K-8 and the Architect review, or does not say which remain: ' + json.dumps(c_rows))
# correction pass: the open Architect questions of round V5 are before-candidate items; no text claims that none is open while one is
c3 = [r for r in table_after(TXT[P_MD], '### 7.1 ', '### 7.2 ') if r[0] == 'C-3']
if not c3 or not all(x in c3[0][1] for x in ('AQ-V5-01', 'AQ-V5-02', 'AQ-V5-08', 'AQ-V5-04', 'AQ-V5-06', 'before the candidate',
                                             'no AQ-V4 question remains', 'CP-13')):
    bad.append('md 7.1 C-3 does not name the open Architect questions AQ-V5-01, AQ-V5-02, AQ-V5-08 (and the inherited AQ-V5-04, AQ-V5-06) '
               'before the candidate')
if not all(x in sec71 for x in ('AQ-V5-01, AQ-V5-02 and AQ-V5-08 (section 8.2) go with the Architect', 'may change scenario bytes before the candidate',
                                'AQ-V5-08 is also the `BLOCKED_BY` value of')):
    bad.append('md 7.1 closing sentence does not name the new questions')
hdr = section(TXT[P_MD], 'PREREQUISITES', 'BLOCKER ')
for v in ('K-3', 'K-8', 'before the candidate', 'AQ-V5-01', 'AQ-V5-02', 'AQ-V5-08', 'AQ-V5-04', 'AQ-V5-06', 'no AQ-V4 question remains'):
    if v not in re.sub(r'\s+>\s+', ' ', hdr):
        bad.append('header PREREQUISITES does not name ' + v)
for p_ in (P_MD, P_COV):
    for pat in (r'no open question remains', r'no Architect open question remains', r'no other\s+Architect item is open', r'open question\s+remains'):
        if re.search(pat, TXT[p_], re.I):
            bad.append(p_.split('/')[-1] + ': claims that no Architect question is open ("' + pat + '")')
sec82 = section(TXT[P_MD], '### 8.2 ', '## 9. ')
rows82 = {r[0].split(' (')[0]: r for r in table_after(TXT[P_MD], '### 8.2 ', '## 9. ')}
for q in ('AQ-V5-01', 'AQ-V5-02', 'AQ-V5-08', 'AQ-V5-04', 'AQ-V5-06'):
    if q not in rows82 or not rows82[q][3].startswith('OPEN') or 'before the candidate' not in rows82[q][3] or not rows82[q][2].startswith('yes'):
        bad.append('md 8.2: ' + q + ' is not recorded as open before the candidate with the bytes it can change')
q3 = rows82.get('Q-V5-BA04-3')
if not q3 or 'CLOSED' not in q3[3] or 'BA-10 V5 section 2.2' not in q3[3] or 'N_det + 1' not in q3[2]:
    bad.append('md 8.2: Q-V5-BA04-3 is not closed by the pointer to BA-10 V5 section 2.2')
if 'BA-10 names no `CHAR_RUNS` case' in TXT[P_MD]:
    bad.append('md 8.2 keeps the stale Q-V5-BA04-3 text')
if 'WU-BOUNDARY' not in rows82.get('AQ-V5-02', ['', ''])[1] or 'PR-CLOSURE-COMPLETENESS' not in rows82.get('AQ-V5-02', ['', ''])[1]:
    bad.append('md 8.2: AQ-V5-02 does not name PR-CLOSURE-COMPLETENESS and WU-BOUNDARY')
cb_evm = [s['SCENARIO_ID'] for s in SC if s['BUILD_UNDER_TEST'] == CB and s['RUN_GOVERNANCE'] == 'GOVERNING' and 'EVM_FROZEN' in s['PIN_SLOTS']
          and 'BUILD_EQUIVALENCE_RECORD' not in s['RUN_PREREQUISITES']]
if ('**%d**' % len(cb_evm)) not in rows82.get('AQ-V5-01', ['', ''])[1] or ('the %d characterization-build runs' % len(cb_evm)) not in re.sub(r'\s+>\s+', ' ', hdr):
    bad.append('AQ-V5-01 does not state the recomputed count %d' % len(cb_evm))
for twin in ('PR-CLOSURE-COMPLETENESS-CB', 'WU-BOUNDARY-CB'):
    if twin in BY:
        bad.append(twin + ' exists although AQ-V5-02 is open (the question is not decided here)')
if re.search(r'AQ-V4-0[1-6]|K-6|AQ-V4-14', hdr):
    bad.append('header PREREQUISITES still names a ruled question')
dep = section(TXT[P_MD], 'DEPENDS_ON', '\n> PREREQ')
if not all(x in dep for x in ('BA-01', 'BA-02a', 'BA-03', 'BA-05', 'BA-06', 'BA-07', 'BA-08', 'BA-09', 'BA-10')) or 'BA-02b' in dep:
    bad.append('header DEPENDS_ON of BA-04 is not BA-01, BA-02a, BA-03, BA-05..BA-10 (AR4-17)')
cdep = section(TXT[P_COV], 'DEPENDS_ON', '\n> PREREQ').split('(entries')[0]
if [x for x in re.findall(r'BA-\d\d[ab]?', cdep)] != ['BA-01', 'BA-02a', 'BA-04']:
    bad.append('header DEPENDS_ON of BA-02b is not BA-01, BA-02a, BA-04 (AR4-17)')
check('blockers_prerequisites_and_edges', bad, 'the section 1 vocabulary is K-3, K-8 and AQ-V5-08; section 7.1 lists K-3, K-8 and the Architect review '
      '(AQ-V5-08 among its items) and says which remain; the header '
      'PREREQUISITES names no ruled question; DEPENDS_ON adds BA-07 (BA-04) and BA-01 (BA-02b) (AR4-17)')
check('status_rule', [s['SCENARIO_ID'] for s in SC if s['STATUS'] != ('EXPLORATORY_NON_GOVERNING' if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING' else 'DRAFT' if s['BLOCKED_BY']
                                                                      else 'SEALED_PENDING_PIN_CANDIDATE' if s['PIN_SLOTS'] else 'SEALED_CANDIDATE')])
sample = CAT['PARAM_03_SAMPLE']
exp_sample = [i for i in ids if i.startswith('CL-CLEAN-FX')] + ['EA2-DEFERRED-EVENTS']
check('param_03_sample', ([] if sample == exp_sample else ['sample ' + json.dumps(sample)]) +
      [i for i in ids if BY[i]['REPETITION'].startswith('CLEAN') != (i in sample)] + [i for i in sample if BY[i]['RUN_GOVERNANCE'] != 'GOVERNING'] +
      ([] if 'AR4-06 (6)' in CAT['REPETITION_RULES']['CLEAN'] else ['CLEAN rule does not state the EA2 equivalence condition']),
      'the governing CL-CLEAN-<fixture> scenarios plus EA2-DEFERRED-EVENTS carry the CLEAN rule, and no other (AR3-24); EA2 valid for the product build only under the equivalence')


def n_strict(s):
    classes = {GT[g][0] for g in s['GROUPS_SERVED'] if GT[g][1] == 'CONTROL' and GT[g][0] in ('A', 'B')}
    return {frozenset(['A']): 'N_A', frozenset(['B']): 'N_B', frozenset(['A', 'B']): 'max(N_A, N_B)', frozenset(): None}[frozenset(classes)]


CHAR_LABEL = {'PIN': 'CHAR: CHAR_RUNS = N_A', 'EXPL': 'CHAR: CHAR_RUNS = EVIDENCE_REPETITION (exploratory)',
              'HDM': 'CHAR: CHAR_RUNS = N_A at the pinned key values, plus exactly one informative run per varied key (outside the intent)',
              'COMP': 'CHAR: CHAR_RUNS = PARAM-02 (non-governing composition check)'}


def expected_repetition(s):
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
        if s['SCENARIO_ID'] == 'EV-L-COMPOSED':
            return CHAR_LABEL['COMP']
        return 'UNDERIVABLE: a non-governing scenario that feeds no pin, selection or check'
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
rr = CAT['REPETITION_RULES']
if not ('max(N_A, N_B)' in rr['DEFAULT'] and 'no ordering' in rr['DEFAULT'] and 'AR4-14 (a)' in rr['DEFAULT'] and 'pending' not in rr['DEFAULT']):
    bad.append('REPETITION_RULES.DEFAULT does not state N_strict with AR4-14 (a)')
if not ('CHAR_RUNS(s) = N_A + V(s)' in rr['CHAR'] and 'exactly one informative run per varied key' in rr['CHAR'] and 'AR4-14 (b)(ii)' in rr['CHAR']
        and 'at least one' not in rr['CHAR'] and 'pending' not in rr['CHAR']):
    bad.append('REPETITION_RULES.CHAR does not state CHAR_RUNS with exactly one informative run per varied key (AR4-14 (b)(ii))')
md4 = []
for line in TXT[P_MD].split('| Repetition rule | Value |', 1)[1].split('\n')[2:]:
    if not line.startswith('|'):
        break
    md4.append([c.strip() for c in line.strip()[1:-1].split('|')])
if {r[0].strip('`'): r[1] for r in md4} != dict(rr):
    bad.append('md section 4 repetition rules differ from the JSON REPETITION_RULES')
check('repetition_label_equals_ba10_derivation', bad,
      'the REPETITION label of every scenario is the first matching row of the BA-10 rule table (N_strict: N_A, N_B or max(N_A, N_B); CONTRACT_ASSIGNED_GROUPS '
      'excluded, AR4-14 (a); CHAR_RUNS with exactly one informative run per varied key, AR4-14 (b)(ii))')
check('dry_rule_n_b', [s['SCENARIO_ID'] for s in SC if (s['MODE'] == 'DRY_RUN') != s['REPETITION'].startswith('DRY: WU_DRY_RUNS = N_B')])

bad = []
LP = CAT['LOG_PROFILES']
for p in ('LP-INJ', 'LP-WIT', 'LP-PAR'):
    if p not in LP:
        bad.append(p + ' missing')
for s in SC:
    if s['MODE'] == 'DRY_RUN':
        continue
    lg, rule = s['EXPECTED_LOG_RECORD_SET'], s['CONDITIONAL_EXPECTATION_RULE']
    wit = any(r in rule for r in ('CER-WIT', 'CER-A2', 'CER-PERSIST'))
    if bool(s['INJECTIONS']) != ('LP-INJ' in lg):
        bad.append(s['SCENARIO_ID'] + ': LP-INJ')
    if wit != ('LP-WIT' in lg):
        bad.append(s['SCENARIO_ID'] + ': LP-WIT')
    if ('CER-PERSIST' in rule or s['SCENARIO_ID'] == 'Q-I14-Y') != ('LP-PAR' in lg):
        bad.append(s['SCENARIO_ID'] + ': LP-PAR')
    if wit and 'delivery-witness log' not in s['EXPECTED_EVIDENCE_SET']:
        bad.append(s['SCENARIO_ID'] + ': the evidence set does not name the I-14 witness log')
    if 'CER-PERSIST' in rule and 'post-abort-read record' not in s['EXPECTED_EVIDENCE_SET']:
        bad.append(s['SCENARIO_ID'] + ': the evidence set does not name the post-abort read')
    for tok in re.findall(r'\bLP-[A-Z0-9-]+', lg):
        if tok not in LP:
            bad.append(s['SCENARIO_ID'] + ': unknown profile ' + tok)
check('log_record_deltas_and_evidence_sets', bad, 'R1:BA04V4-03: INJECTION records for every injected scenario, WITNESS records where CER-WIT, CER-A2 or CER-PERSIST resolve, '
      'the POST_ABORT_READ record where CER-PERSIST resolves (and in Q-I14-Y); the evidence set names the I-14 records; every profile token is defined')

# ------------------------------------------------------------------------------------------------------------------
# 8. rulings with a mechanical trace
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
if not (n['EXPECTED_CONTROL_RESULTS'].get('E08', '').startswith('FAIL at PV') and not n['BLOCKED_BY'] == ['E6-C4'] and n['BLOCKED_BY'] == ['K-3'] and 'E06' in n['CONTROLS_EXERCISED']
        and n['EXPECTED_CONTROL_RESULTS'].get('E06', '').startswith('PASS at every sampling point') and 'AR4-04' in n['EXPECTED_CONTROL_RESULTS']['E06']
        and n['EXPECTED_CONTROL_RESULTS'].get('E12A', '').startswith('CONDITIONAL (CER-WIT)') and 'CER-PERSIST' in n['CONDITIONAL_EXPECTATION_RULE']
        and 'QUALIFICATION_RECORD@Q-I14-Y' in n['RUN_PREREQUISITES']):
    t.append('NC-S5-O6 (AR3-04 as revised by AR4-04: E-06 PASS at every defined sampling point, no E6-C4 blocker, K-3 and Q-I14-Y)')
n = BY['NC-CLONE-REPLACE']
if not (n['FIXTURE'] == 'FX-IMP-NESTED' and n['GROUP'] == 'PW-CLONE' and n['TARGET_NEGATIVE_CONTROL'] == 'CLONE' and n['DELIBERATE_VIOLATION_FLAG'] == 'YES'
        and n['BLOCKED_BY'] == ['K-8'] and n['EXPECTED_PRODUCT_OUTCOME'].startswith('O6') and n['BUILD_UNDER_TEST'] == CB
        and 'stays on the allow-list' in n['INJECTIONS'][0]['variant'] and 'below the' in n['PLAN_OR_INPUT'] and 'effect check' in n['EXPECTED_DETECTION_CHANNEL']):
    t.append('NC-CLONE-REPLACE (AR3-07; the declared mode stays on the allow-list, R1:BA04V4-06)')
w = BY['PW-CLONE-WRONG-TYPE']
if not (w['EXPECTED_CONTROL_RESULTS'].get('S5CLO', '').startswith('FAIL') and 'nothing imported' in w['EXPECTED_PROCEDURAL_RESULT'] and 'import or reuse executed' not in w['EXPECTED_PROCEDURAL_RESULT']):
    t.append('PW-CLONE-WRONG-TYPE (R1:BA04V4-07)')
hdm = [i for i in ids if i.startswith('HDM-CHAR-')]
if sorted(hdm) != ['HDM-CHAR-FX1F', 'HDM-CHAR-FXANN', 'HDM-CHAR-FXANNBLANK', 'HDM-CHAR-FXDIM']:
    t.append('HDM-CHAR-* are not the four of AR3-11 and AR4-07')
for sid in hdm:
    h = BY[sid]
    injs = h['INJECTIONS']
    if not (h['GROUP'] == 'HDM' and h['GROUPS_SERVED'] == [] and h['RUN_GOVERNANCE'] == 'NON_GOVERNING_BY_DESIGN' and h['RECORDED_NOT_ENFORCED'] == AR301_SET
            and h['PIN_SLOTS'] == ['FIXTURE_INSTANCE@' + h['FIXTURE'], 'LOCK_MODE']):
        t.append(sid + ' (AR3-11 / AR4-07 fields)')
    if not (len(injs) == 9 and all(i['writer'] == 'CONTROL_PLANE_SETUP' and i['position'] == 'before P0' and i['variant'].startswith('informative run: ') for i in injs)):
        t.append(sid + ' (R1:BA04V4-08: one CONTROL_PLANE_SETUP injection per key before P0)')
    p = h['PLAN_OR_INPUT']
    if not all(x in p for x in ('HDM-3', 'HDM-4', 'as pinned in the fixture instances', 'CHAR_RUNS(s) = N_A + V(s)', 'exactly one informative run per key variable',
                                'marked informative in the pin record', 'UNKNOWN (O5', 'every object class its mirror creates')) or 'at least one' in p:
        t.append(sid + ' (AR4-14 (b)(ii) and BA-08 11.2 HDM-3/HDM-4 wording)')
hb = BY['HDM-CHAR-FXANNBLANK']
if not (hb['FIXTURE'] == 'FX-ANN-BLANK' and 'SYMBOL_RECORD_LAYER' in hb['PLAN_OR_INPUT'] and 'AR4-07' in hb['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']):
    t.append('HDM-CHAR-FXANNBLANK (AR4-07)')
c = BY['CL-CLEAN-FXANNBLANK-FP']
if not (c['FIXTURE'] == 'FX-ANN-BLANK' and c['GROUP'] == BY['CL-CLEAN-FXANN-FP']['GROUP'] and c['BLOCKED_BY'] == ['K-3'] and 'WU-DRY-CL-CLEAN-FXANNBLANK-FP' in BY
        and c['EXPECTED_PRODUCT_OUTCOME'] == 'O3 SUCCESS' and 'HOST_DEFAULT_MAP' in c['PIN_SLOTS'] and 'HDM-CHAR-FXANNBLANK' in c['NOTE'] and c['STATUS'] == 'DRAFT'):
    t.append('CL-CLEAN-FXANNBLANK-FP (AR4-07: K-6 dropped, K-3 and HOST_DEFAULT_MAP kept)')
for sid in ('KS-SL1-POSTWRITE', 'KS-SL1-TX-MISMATCH', 'KS-SL4-NEG-DET'):
    k = BY[sid]
    cag = k['CONTRACT_ASSIGNED_GROUPS']
    if not (sorted(x['group'] for x in cag) == ['KS', 'SC-A'] and all('AR4-03' in x['clause'] for x in cag) and k['BLOCKED_BY'] == [] and k['GROUP'] == 'AB'
            and 'KS' not in k['GROUPS_SERVED'] and 'SC-A' not in k['GROUPS_SERVED'] and 'SEAM' not in k['EXPECTED_CONTROL_RESULTS']
            and 'EXPECTED_PROCEDURAL_RESULT' in k['EXPECTED_PROCEDURAL_RESULT'] and 'seam-contract finding' in k['EXPECTED_PROCEDURAL_RESULT']
            and 'NO_VERDICT for KR' in k['EXPECTED_GROUP_VERDICT']):
        t.append(sid + ' (AR4-03)')
if not ('S2' in BY['U-3']['CONTROLS_EXERCISED'] and 'KS' in BY['U-3']['GROUPS_SERVED']):
    t.append('U-3 S2 exercised (AR3-09)')
d = BY['NC-E12D']
if not (d['CONTROLS_EXERCISED'] == ['WUM', 'E12D'] and d['EXPECTED_CONTROL_RESULTS'].get('E11M', '').startswith('NOT_DECLARED')
        and not [p for p in d['PIN_SLOTS'] + BY['NC-WU-FOREIGN']['PIN_SLOTS'] if p.startswith('E04')]):
    t.append('NC-E12D / NC-WU-FOREIGN (R2:BA04V4-04)')
# A-1 (R1:BA04V4-09, second option; correction pass): V2.1 10.6 requires the baseline runs without writers to evaluate A-1 and A-2
a = sorted(s['SCENARIO_ID'] for s in SC if 'A1' in s['CONTROLS_EXERCISED'])
if a != sorted(['EA1-READONLY-OPENS'] + list(CAT['PARAM_03_SAMPLE'])):
    t.append('A1 is not exercised exactly by EA1-READONLY-OPENS and the PARAM-03 sample (V2.1 10.6; R1:BA04V4-09): ' + json.dumps(a[:12]))
ca2 = CAT['CONDITIONAL_EXPECTATION_RULES']['CER-A2']
if not all(x in ca2 for x in ('A-1 PASS', 'A-1 UNKNOWN', 'V2.1 14: no origin inference', 'never A-1 FAIL', 'V2.1 10.6')) or 'A-1 is not evaluated in a mirror run' in ca2:
    t.append('CER-A2 does not make A-1 conditional (no W-scan event => PASS; a delivered W-scan event => UNKNOWN)')
e2r = BY['EA2-DEFERRED-EVENTS']['EXPECTED_CONTROL_RESULTS'].get('A1', '')
if not (e2r.startswith('CONDITIONAL (CER-A2): PASS') and 'UNKNOWN' in e2r and 'never FAIL' in e2r):
    t.append('EA2-DEFERRED-EVENTS: A1 is not conditional by CER-A2')
for i in CAT['PARAM_03_SAMPLE']:
    if i == 'EA2-DEFERRED-EVENTS':
        continue
    cr = BY[i]['EXPECTED_CONTROL_RESULTS'].get('A1', '')
    if not (cr.startswith('PASS') and 'UNKNOWN' in cr and 'V2.1 10.6' in cr and 'CER-' not in cr) or BY[i]['BUILD_UNDER_TEST'] != PB:
        t.append(i + ': the A-1 expectation of a clean run (PASS on its W-scan record; UNKNOWN never FAIL) is not stated')
a1e = CAT.get('A1_EVALUATION', '')
if not (a1e and a1e in TXT[P_MD] and all(x in a1e for x in ('EA1-READONLY-OPENS', 'PARAM-03', 'V2.1 10.6', 'A-1 PASS', 'A-1 UNKNOWN, never FAIL',
                                                             'CER-A2', 'AR2-12'))):
    t.append('JSON A1_EVALUATION missing, incomplete or not in md section 3')
crow = [l for l in sec1.split('\n') if l.startswith('| `CONTROLS_EXERCISED` |')]
if not crow or 'A-1 is exercised only by' in crow[0] or 'PARAM-03' not in crow[0]:
    t.append('md section 1 CONTROLS_EXERCISED row does not state the A-1 scope')
bm = {r[0].strip('`'): r for r in table_after(TXT[P_COV], '## 2. Control coverage', '## 3.')}
if 'a mirror run does not evaluate A-1' in TXT[P_COV] or 'V2.1 10.6' not in bm.get('A1', [''] * 13)[12]:
    t.append('BA-02b A1 row: the no-violation reason does not follow the corrected A-1 scope')
e2 = BY['EA2-DEFERRED-EVENTS']
if not (e2['BUILD_UNDER_TEST'] == CB and 'CHARACTERIZATION_TUPLE' in e2['TUPLE_REQUIREMENTS'] and 'QUALIFICATION_RECORD@Q-I14-X' in e2['RUN_PREREQUISITES']
        and 'BUILD_EQUIVALENCE_RECORD' in e2['NOTE']):
    t.append('EA2-DEFERRED-EVENTS (AR4-06 (6); R2:BA04V4-02)')
q = BY['Q-I14-Y']
if not ('LP-PV (Wr-F, Wr-G, Wr-OCT' in q['EXPECTED_LOG_RECORD_SET'] and 'abort' in q['EXPECTED_CONTROL_RESULTS'].get('vehicle path', '')):
    t.append('Q-I14-Y per-writer path (R2:BA04V4-06)')
sec21 = section(TXT[P_MD], '### 2.1 ', '### 2.2 ')
if not all(x in sec21 for x in ('`PIN_RECORDED`', '`ANCHOR_RECORDED`', '`IN_USE`', '`IN_USE.pinRecordHashes`', 'no BA-11 value is written', 'COORDINATOR', 'ARCHITECT',
                                '`subjectKind PIN`', 'at most one live pin per slot', 'fail-closed', 'AR4-13', 'BUILD_EQUIVALENCE_RECORD', 'BA-07 V5')):
    t.append('md 2.1 pin custody (AR3-23, AR4-13, AR4-06 (4))')
# correction pass: usability and carry-over by pointer to BA-07 V5, never the weaker V5 draft restatement
if (not all(x in sec21 for x in ('BA-07 V5 sections 5.1 (rules 1-4), 7 (U1-U5) and 8', '`IN_USE.runStartAnchor`', 'BA-07 V5 sections 5.1, 7 and 8',
                                   'use preconditions U1-U5 of BA-07 V5 section 7', 'BA-07 V5 section 7, U5'))
        or 'U1-U4' in sec21
        or 'anchor recorded before that run\'s `IN_USE` entry' in sec21 or 'before the first governing use (AR4-13)' in sec21):
    t.append('md 2.1 does not point to BA-07 V5 sections 5.1 (rules 1-4), 7 (U1-U5) and 8, or keeps the weaker restatement or the U1-U4 range')
sec8 = section(TXT[P_MD], '## 8. ', '## 9. ')
rows8 = {r[0]: r for r in table_after(TXT[P_MD], '### 8.1 ', '### 8.2 ')}
for nq in range(1, 15):
    qid = 'AQ-V4-%02d' % nq
    if qid not in rows8 or not rows8[qid][1].startswith('ruled by AR4-'):
        t.append('md 8.1: ' + qid + ' not recorded as ruled by an AR4 decision')
if 'no expectation change in the twenty abort scenarios' not in sec8:
    t.append('md 8.1: AQ-V4-14 must change no expectation (AR4-15)')
if 'AQ-V4-11' in rows8 and not all(x in rows8['AQ-V4-11'][1] for x in ('CQ-03 (iii)', 'CQ-04', 'part of CQ-05', 'the rest of CQ-07 (to EXEC-3)')):
    t.append('md 8.1: AQ-V4-11 does not list every AR4-12 deferral (the rest of CQ-07 to EXEC-3)')
check('ruling_traces', t)

# ------------------------------------------------------------------------------------------------------------------
# 8b. correction pass of round V5 (the coverage and consistency critics' gaps on BA-04 / BA-02b)
# ------------------------------------------------------------------------------------------------------------------
t = []
# HDM-CHAR-FXDIM: the dimension style is writer-assigned and excluded from HDM-1 (BA-08 V5 section 11.2, section 11 "Rule 1, dimension style")
FXDIM = ('RotatedDimension (Color, Linetype, LinetypeScale, LineWeight, Transparency; its dimension style is writer-assigned and excluded: '
         'BA-08 V5 section 11.2 HDM-1 and section 11, paragraph "Rule 1, dimension style")')
if FXDIM not in BY['HDM-CHAR-FXDIM']['EXPECTED_CONTROL_RESULTS'].get('HOST_DEFAULT_MAP characterization', ''):
    t.append('HDM-CHAR-FXDIM: the class text does not exclude the writer-assigned dimension style')
if any('and the dimension style when no named style is used' in x for s in SC for x in strings(s)):
    t.append('a scenario keeps the V4 HDM-1 wording of the dimension style')
# the learning micro-scenario driver: a declared harness DLL of the one PRODUCT_BUILD profile
full = CAT['BUILD_PROFILES'][PB]
DRV = ('the learning micro-scenario driver is a declared harness DLL of the one PRODUCT_BUILD profile', 'present in every PRODUCT_BUILD run',
       'E-10 known-module set', 'WARMUP_MEMBERSHIP set', 'inert outside EV-L-*')
if not (full.startswith('FULL_BASELINE_TUPLE') and all(x in full for x in DRV)):
    t.append('BUILD_PROFILES.PRODUCT_BUILD does not carry the driver clause')
for s in SC:
    tr = s['TUPLE_REQUIREMENTS']
    if tr.startswith('FULL_BASELINE_TUPLE') and tr != full and not tr.startswith('FULL_BASELINE_TUPLE except the block-library field'):
        t.append(s['SCENARIO_ID'] + ': a FULL_BASELINE_TUPLE text that is not the one PRODUCT_BUILD profile')
    if s['SCENARIO_ID'].startswith(('EV-L-', 'EV-V-')) and tr != full:
        t.append(s['SCENARIO_ID'] + ': its tuple is not the one PRODUCT_BUILD profile')
    if (s['BUILD_UNDER_TEST'] == PB and s['FIXTURE'] != 'NONE' and s['REACH'] != 'NONE'
            and not tr.startswith('FULL_BASELINE_TUPLE')):
        t.append(s['SCENARIO_ID'] + ': a product-build run with a product process outside FULL_BASELINE_TUPLE')
    if s['BUILD_UNDER_TEST'] == PB and s['FIXTURE'] != 'NONE' and s['REACH'] == 'NONE' and not tr.startswith(('FULL_BASELINE_TUPLE', 'QUALIFICATION_TUPLE (BUILD_UNDER_TEST = PRODUCT_BUILD)')):
        t.append(s['SCENARIO_ID'] + ': a product-build qualification or measurement outside the PRODUCT_BUILD profile')
if '| `learning tuple' in TXT[P_MD] or 'present and inert in the EV-V-* validation' in TXT[P_JSON] + TXT[P_MD]:
    t.append('a separate learning tuple or the V5 draft driver wording remains')
if not all(x in CAT['EV_L_SET']['rule'] for x in DRV) or not all(x in CAT['LOG_PROFILES']['LP-LEARN'] for x in ('one PRODUCT_BUILD profile', 'REVIEWED_REF x1')):
    t.append('EV_L_SET or LP-LEARN does not carry the driver clause or REVIEWED_REF')
if 'REVIEWED_REF included' not in CAT['LOG_PROFILES']['LP-COMPOSE']:
    t.append('LP-COMPOSE does not inherit REVIEWED_REF')
# ENFORCED_NOT_EXERCISED: the normative rule of section 1 and its default
enr = CAT.get('ENFORCED_NOT_EXERCISED_RULE', '')
if not (all(x in enr for x in ('Enforced, not exercised', 'AR4-01', 'E12-L', 'ENFORCED_NOT_EXERCISED', 'BA-02b V5 rule 2', 'not in CONTROLS_EXERCISED'))
        and crow and 'Enforced, not exercised (EV-L-*; AR4-01)' in crow[0] and 'ENFORCED_NOT_EXERCISED' in CAT['DEFAULTS']['UNLISTED_CONTROLS']):
    t.append('the normative rule "Enforced, not exercised" is not stated in section 1, the JSON and UNLISTED_CONTROLS')
for s in SC:
    if s['SCENARIO_ID'].startswith('EV-L-') and (s['CONTROLS_EXERCISED'] or s['GROUPS_SERVED'] not in ([], ['E12-L'])):
        t.append(s['SCENARIO_ID'] + ': an EV-L-* exercises a control or serves a group other than E12-L')
# Q-I14-P, the fifth vehicle: the PREPARE-phase coordinate of PR-REC-REOBS-MISMATCH
q = BY.get('Q-I14-P')
if not q or not (q['FIXTURE'] == BY['PR-REC-REOBS-MISMATCH']['FIXTURE'] and q['INJECTION_COORDINATE'] == 'between PR and PW'
                 and [i['writer'] for i in q['INJECTIONS']] == ['Wr-G'] and q['TARGET_NEGATIVE_CONTROL'] == 'INSTRUMENT:I-14'
                 and q['RECORDED_NOT_ENFORCED'] == AR301_SET and q['BUILD_UNDER_TEST'] == CB and q['GROUP'] == 'Q' and q['RUN_GOVERNANCE'] == 'GOVERNING'
                 and 'LP-PW-REFUSE' in q['EXPECTED_LOG_RECORD_SET'] and 'WU-DRY-Q-I14-P' in BY and 'BA04V3-17' in q['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']):
    t.append('Q-I14-P is not the vehicle of the PREPARE-phase coordinate of PR-REC-REOBS-MISMATCH')
qp = BY['Q-I14']['PLAN_OR_INPUT']
if 'qualified by the vehicles Q-I14-X and Q-I14-Y.' in qp or not all(x in qp for x in ('X0..X8 by Q-I14-X', 'Y0..Y2', 'by Q-I14-Y', 'by Q-I14-P',
                                                                                     'between PR and PW')):
    t.append('Q-I14: the coordinate sentence does not name every vehicle and coordinate')
if list(CAT['PER_BUILD_QUALIFICATION']['characterization_build_only']) != Q14_FAMILY:
    t.append('JSON PER_BUILD_QUALIFICATION.characterization_build_only')
# correction pass (final): section 1.1 does not call Q-I14-P and WU-DRY-Q-I14-P ruled; their admission is AQ-V5-08
sec11f = section(TXT[P_MD], '### 1.1 ', '### 1.2 ')
if ('(all by ruling)' in sec11f or 'scenarios; none is a proposal' in sec11f or not all(x in sec11f for x in (
        '`Q-I14-P` and `WU-DRY-Q-I14-P` apply AR4-01 through its `Q-I14-*` pattern; they were added by the correction pass, and their admission is '
        'the open Architect item AQ-V5-08', '**%d** scenarios: %d by ruling' % (len(mode), len(mode) - len(AQ508_ALL))))):
    t.append('md 1.1 calls Q-I14-P and WU-DRY-Q-I14-P ruled, or does not name AQ-V5-08 as their open admission')
# EV-L-COMPOSED is no longer called a learning micro-scenario in its outcome
if 'learning micro-scenario' in BY['EV-L-COMPOSED']['EXPECTED_PRODUCT_OUTCOME']:
    t.append('EV-L-COMPOSED: the outcome still names a learning micro-scenario')
# section 7.4 heading and C-10 (BA-08 V5 section 16.1)
if '### 7.4 Prerequisites of `BASELINE_READY` held by other artifacts (not direct `BLOCKED_BY` blockers)' not in TXT[P_MD]:
    t.append('md 7.4 heading')
c10 = [r for r in table_after(TXT[P_MD], '### 7.4 ', '```text') if r[0] == 'C-10']
if not c10 or not all(x in c10[0][1] for x in ('K-5', 'K-2', 'BA-08 seal', 'AR3-25', 'through BA-05', 'BA-08 V5 section 16.1')):
    t.append('md 7.4 C-10 does not state how K-5 and K-2 precede the catalog seal and candidate')
check('correction_pass_consistency', t, 'correction pass of round V5: HDM-CHAR-FXDIM excludes the writer-assigned dimension style; the driver DLL is in the one '
      'PRODUCT_BUILD profile (EV-L-* and EV-V-* use it); "Enforced, not exercised" is normative; Q-I14-P qualifies the PREPARE-phase coordinate; '
      'EV-L-COMPOSED\'s outcome; md 7.4 and C-10; md 8.1 AQ-V4-11 (in ruling_traces)')

RNE_NAME = RNE_KEY
bad = []
for s in SC:
    for k, v in s['EXPECTED_CONTROL_RESULTS'].items():
        if k not in REG or k in s['CONTROLS_EXERCISED']:
            continue
        if v.startswith('NOT_DECLARED'):
            continue
        if k in RNE_NAME and RNE_NAME[k] in s['RECORDED_NOT_ENFORCED'] and v.startswith('RECORDED_NOT_ENFORCED'):
            continue
        if s['SCENARIO_ID'].startswith('EV-L-') and v.startswith('ENFORCED_NOT_EXERCISED'):   # section 1, "Enforced, not exercised" (AR4-01)
            continue
        bad.append(s['SCENARIO_ID'] + ': ' + k + ' = ' + v[:60])
    for k, v in s['EXPECTED_CONTROL_RESULTS'].items():
        if 'ENFORCED_NOT_EXERCISED' in v[:25] and not (s['SCENARIO_ID'].startswith('EV-L-') and k in REG and k not in s['CONTROLS_EXERCISED']):
            bad.append(s['SCENARIO_ID'] + ': ENFORCED_NOT_EXERCISED outside an EV-L-* registry key, or on an exercised control (' + k + ')')
    if s['SCENARIO_ID'].startswith('EV-L-') and [k for k in s['EXPECTED_CONTROL_RESULTS'] if k not in REG and k not in
                                                ('E-12 learning', 'E-12 composition check', 'S-1, S-2, S-3', 'OTHER CONTROLS')]:
        bad.append(s['SCENARIO_ID'] + ': a control expectation under a non-registry key')
check('expected_result_keys_exercised_or_declared', bad,
      'every registry key of EXPECTED_CONTROL_RESULTS is in CONTROLS_EXERCISED, or its value is NOT_DECLARED, or it is a control of the scenario\'s own '
      'RECORDED_NOT_ENFORCED set with the value RECORDED_NOT_ENFORCED, or, in an EV-L-* scenario only, a control it enforces with the value '
      'ENFORCED_NOT_EXERCISED (section 1; correction pass); no EV-L-* control expectation hides under a non-registry key')

# Q-FPSPEC-VECTORS and Q-SIGNED-ZERO (and their twins) against the BA-05 vectors file (AR3-17)
bad = []
ev = {x['id']: x for x in FPV['exactVectors']}
rj = [x['id'] for x in FPV['rejectedInputs']]
sv = {x['id']: x for x in FPV['semanticVectors']}
for qid in ('Q-FPSPEC-VECTORS', 'Q-FPSPEC-VECTORS-CB'):
    q = BY[qid]
    r = q['EXPECTED_CONTROL_RESULTS'].get('instrument I-12', '')
    if not ('every REJECTED input (rejectedInputs ' + rj[0] + '..' + rj[-1] + ') reproduced as REJECTED' in r and 'every DIGEST reproduced' in r
            and 'every UNKNOWN reproduced' in r and 'or whose enclosure straddles the slot, is INVALID and reported, never PASS' in r):
        bad.append(qid + ': expected results miss REJECTED, UNKNOWN, DIGEST or the INVALID instance rule')
    # correction pass (R2:BA05V4-N01): the per-predicate realized quantity of BA-05 V5 3.3.1, as the vectors file states it
    rule_txt = FPV.get('semanticSlotInstanceRule', '')
    per_pred = (('ABS_DIFF_LE', 'rational arithmetic'), ('ANGLE_DIFF_LE', 'NORM_ANGLE'), ('ANGLE_DIFF_LE', 'certified enclosure with exact pi'),
                ('NORMAL_ANGLE_LE', 'certified enclosure'), ('straddles the slot', 'INVALID'))
    if not all(a_ in r and b_ in r and a_ in rule_txt and b_ in rule_txt for a_, b_ in per_pred) or 'semanticSlotInstanceRule' not in r:
        bad.append(qid + ': the I-12 expectation does not carry the per-predicate instance rule of BA-05 V5 3.3.1 (semanticSlotInstanceRule)')
    if 'exact-arithmetic check' in r or 'checked in exact arithmetic' in q['EXPECTED_PROCEDURAL_RESULT'] or 'certified enclosure' not in q['EXPECTED_PROCEDURAL_RESULT']:
        bad.append(qid + ': the V5 draft "exact arithmetic" wording remains, or the procedural result lacks the per-predicate evaluation')
    if not all(x in q['PLAN_OR_INPUT'] for x in ('exactVectors', 'rejectedInputs', 'semanticVectors', P_FPV)):
        bad.append(qid + ': the input sections of the vectors file are not named')
    slot_units = sorted(i for i, x in sv.items() if isinstance(x.get('persisted'), dict) and 'slotExpression' in x['persisted'])
    if not all(i in q['PLAN_OR_INPUT'] for i in slot_units):
        bad.append(qid + ': slot-unit vectors not named: ' + json.dumps(slot_units))
    if 'SEAL_HASH of BA-08' not in q['TUPLE_REQUIREMENTS']:
        bad.append(qid + ': FPSPEC_CONFORMANCE_TUPLE does not bind the BA-08 SEAL_HASH')
if rj != ['RJ-%02d' % n for n in range(1, len(rj) + 1)] or not all(x['result'] == 'REJECTED' for x in FPV['rejectedInputs']):
    bad.append('vectors file: rejectedInputs are not RJ-01..RJ-nn, all REJECTED')
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
      'Q-FPSPEC-VECTORS (and its twin) names exactVectors, rejectedInputs and semanticVectors, expects every DIGEST, UNKNOWN and REJECTED and the INVALID instance '
      'rule, binds the BA-08 SEAL_HASH; Q-SIGNED-ZERO cites EV-01, EV-02, SV-05, EV-06, EV-07, which hold those outcomes')

# ------------------------------------------------------------------------------------------------------------------
# 9. md companion equals the JSON; no stale markers; delta ruling equal in both files
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
           'Y' if s['DESIGNED_STIMULUS'] != 'NONE' else '-', {PB: 'P', CB: 'C', 'NONE': '-'}[s['BUILD_UNDER_TEST']], oc,
           s['STATUS'], ', '.join(s['BLOCKED_BY']) or '-', '`%s`' % s['SEALED_AT'][:12]]
    if r != exp:
        bad.append(s['SCENARIO_ID'])
m = re.search(r'Totals: \*\*(\d+)\*\* scenarios \((\d+) own scenarios and (\d+) `WU-DRY-\*`\)', TXT[P_MD])
if not m or (int(m.group(1)), int(m.group(2)), int(m.group(3))) != (len(SC), len(own), len(SC) - len(own)):
    bad.append('totals line')
check('md_summary_equals_json', bad)
bad = []
for p in (P_JSON, P_MD, P_COV):
    for pat in (r'pending AQ-V4', r'BA-11 V4', r'\(AQ-V4-0[1-7]\)'):
        for mm in re.finditer(pat, TXT[p]):
            bad.append(p.split('/')[-1] + ': "' + TXT[p][max(0, mm.start() - 30):mm.end() + 10].replace('\n', ' ') + '"')
hdr_delta = re.sub(r'\s+', ' ', section(TXT[P_MD], 'DELTA RULING APPLIED', '\n> DEPENDS_ON').replace('>', ' ')).strip(' =')
if hdr_delta != CAT['DELTA_RULING_APPLIED']:
    bad.append('md header DELTA RULING APPLIED differs from the JSON DELTA_RULING_APPLIED')
if 'AR3-23' not in CAT['DELTA_RULING_APPLIED'] or 'AR3-29 is applied by BA-02b' not in CAT['DELTA_RULING_APPLIED']:
    bad.append('DELTA_RULING_APPLIED: AR3-23 applied, AR3-29 stated as BA-02b\'s (R1:BA04V4-11, R2:BA04V4-10)')
if 'textually verified' not in CAT['CONTRACT'] or 'draft revision 2' not in CAT['CONTRACT']:
    bad.append('CONTRACT line is not the V5 line')
check('no_stale_markers_and_equal_delta_ruling', bad, 'no "pending AQ-V4", no version-labelled BA-11 citation (AR4-17), no AQ-V4-01..07 carried as pending; the md header and the '
      'JSON hold the same DELTA_RULING_APPLIED; the contract line of round V5')

# ------------------------------------------------------------------------------------------------------------------
# 10. BA-02b V5: control coverage recomputed (rules 3..5), group coverage, verbatim rows, discharges and enumerated sets
# ------------------------------------------------------------------------------------------------------------------


def countable(s):
    return (s['RUN_GOVERNANCE'] == 'GOVERNING' and s['MODE'] in ('CHARACTERIZATION_RUN', 'VALIDATION') and s['STATUS'] != 'EXPLORATORY_NON_GOVERNING'
            and s['GROUP'] not in EVID and s['RECORDED_NOT_ENFORCED'] != AR301_SET)


def names_fail(s, c):
    r = s['EXPECTED_CONTROL_RESULTS'].get(c, '')
    return any(k in r for k in ('FAIL', 'UNKNOWN', 'difference', 'REJECT'))


cov_md = {r[0].strip('`'): r for r in table_after(TXT[P_COV], '## 2. Control coverage', '## 3.')}
REASONED = ('E01', 'E13', 'A1', 'A2', 'E12DET', 'E14', 'E15')
bad, gaps, cond, cb_only = [], [], [], []
for k in REG:
    cnt = [s for s in SC if k in s['CONTROLS_EXERCISED'] and countable(s)]
    clean = [s for s in cnt if s['DELIBERATE_VIOLATION_FLAG'] == 'NO']
    negs = [s for s in cnt if s['DELIBERATE_VIOLATION_FLAG'] == 'YES' and s['TARGET_NEGATIVE_CONTROL'] == k and names_fail(s, k)]
    neg_u = [s for s in negs if not conditional(s, k)]
    neg_c = [s for s in negs if conditional(s, k)]
    reason = k in REASONED
    if k in ('E14', 'E15'):
        neg_u, neg_c = [], []
    ncb = [s for s in neg_u + neg_c if s['BUILD_UNDER_TEST'] == CB]
    if (neg_u or neg_c) and len(ncb) == len(neg_u + neg_c):
        cb_only.append(k)
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
    if not row or row[-1] != st or row[-2] != '0' or row[3] != ('YES' if REG[k]['inPredicate'] else 'NO') or row[8] != '%d of %d' % (len(ncb), len(neg_u + neg_c)):
        bad.append(k + ': md ' + (json.dumps(row[-1:] + row[8:9]) if row else 'missing') + ' != ' + st)
check('ba02b_no_coverage_gap_except_declared_conditionals', gaps,
      'every control is COVERED_DRAFT, or COVERED_DRAFT_CONDITIONAL (only conditional negative controls: NEGATIVE_CONTROL_NOT_EVALUATED at acceptance, AR3-03)')
check('ba02b_section2_states_and_characterization_column_equal_recomputation', bad)
m = re.search(r'Conditional rows \(rule 5\): (.*?)\. The column', TXT[P_COV])
declared = re.findall(r'`([A-Z0-9]+)` \(', m.group(1)) if m else None
check('ba02b_conditional_rows_declared', ([] if declared == cond else ['declared ' + json.dumps(declared) + ' recomputed ' + json.dumps(cond)]) +
      ([] if {'S5', 'E12A', 'E12C'} <= set(cond) else ['S5, E12A and E12C are expected conditional (AR4-04, AR4-16): ' + json.dumps(cond)]))
m = re.search(r'the controls whose every counted negative control runs there are (.*?): their', TXT[P_COV])
check('ba02b_characterization_build_dependency_declared', [] if (m and ids_in(m.group(1)) == cb_only) else ['declared ' + (m.group(1) if m else 'missing') + ' recomputed ' + json.dumps(cb_only)],
      'R1:BA02V4-N03: the controls whose every counted negative control runs on the CHARACTERIZATION_BUILD are named (AR4-06)')
check('ba02b_section2_rows_all_controls', sorted(set(REG) ^ set(cov_md)))
r5 = section(TXT[P_COV], '5. **Conditional negative controls', '6. **Section 3')
check('ba02b_rule5_ar4_20_r3', [x for x in ('(a)', '(b)', '(c)', '(d)', 'NOT MET', 'ALT21D_HOST_PASS', 'NOT_EVALUATED', 'checks 1 to 4', 'detection branch') if x not in r5],
      'AR4-20 R-3 (a)..(d): per control; check 2 NOT MET; ALT21D_HOST_PASS NOT_EVALUATED, never TRUE; designed detection only on the detection branch')

g_md = {r[0].strip('`'): r for r in table_after(TXT[P_COV], '## 4. Group coverage', '## 5.')}
bad = []
for g, (cls, kind) in GT.items():
    prim = [s for s in SC if s['GROUP'] == g]
    serv = [s for s in SC if g in s['GROUPS_SERVED']]
    cag = [s for s in SC if any(c['group'] == g for c in s['CONTRACT_ASSIGNED_GROUPS'])]
    cnt = 'N/A (PA-3)' if kind == 'EVIDENCE' else 'N/A (PA-4)' if kind == 'CONTROL_OUTSIDE_PREDICATE' else str(len([s for s in serv if countable(s)]))
    gov = len([s for s in serv if s['RUN_GOVERNANCE'] == 'GOVERNING'])
    exp = ['`%s`' % g, kind, cls, str(len(prim)), str(len(serv)), str(len(cag)), cnt, str(gov), '0']
    if g_md.get(g) != exp:
        bad.append(g + ': md ' + json.dumps(g_md.get(g)) + ' != ' + json.dumps(exp))
    if not serv and not cag:
        bad.append(g + ': no serving or contract-assigned scenario')
check('ba02b_group_coverage_recomputed', bad, 'contract-assigned scenarios are counted apart and never as serving, countable or governing-serving (R1:BA02V4-N06)')


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
                   'E6-C1..E6-C8': bool(re.match(r'^E6-C[1-8](-CB)?$', sid)), 'OU-O1..O7': bool(re.match(r'^OU-O[1-7]$', sid)),
                   'CLOSURE_COMPLETENESS_CONTROL': sid == 'PR-CLOSURE-COMPLETENESS',
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


# enumerated sets recomputed from the contract and the catalog (rule 6; R1:BA02V4-N04)
E6C = [mm.group(1) for mm in (re.match(r'^\| (E6-C\d) \|', l) for l in TXT[P_V21].split('\n')) if mm]
ROWS172 = sorted({int(x) for x in re.findall(r'^\| (\d) \|', TXT[P_V21].split('### 17.2')[1].split('\n### ')[0], re.M)})
pa12 = TXT[P_V22].split('## PA-12')[1].split('## PA-13')[0]
pa12_var = [r[0] for r in table_after(pa12, '**Replaced: `CLONE_NON_REPLACE_CONTROL`**')]
VAR_ID = {'only by case': 'CASE', 'wrong object or type': 'WRONG-TYPE', 'nested dependencies': 'NESTED', 'symbol-table records': 'SYMTAB'}
PA12_SET = ['BASE'] + [v for txt in pa12_var for key, v in VAR_ID.items() if key in txt]
WSRC = [s['SCENARIO_ID'] for s in SC if enters_window(s)]


def enum_members(rid, text):
    if (rid, text) == ('R01', 'each persistent-change writer at X1..X4 detected by the semantic comparison'):
        return {'Wr-%s at X%d' % (w, x): (lambda i, w=w, x=x: i == 'WR-%s-X%d' % (w, x)) for w in 'ADEFG' for x in range(1, 5)}
    if (rid, text) == ('R04', 'one deliberate violation per control refused at P0'):
        return {k: (lambda i, k=k: BY[i]['TARGET_NEGATIVE_CONTROL'] == k.replace('-', '') or (k == 'E-01' and i == 'Q-I02')) for k in ('E-01', 'E-02', 'E-05', 'E-07')}
    if (rid, text) == ('R05', 'each candidate mode'):
        return {c: (lambda i, c=c: i.startswith('LK-' + c + '-')) for c in sorted({i.split('-')[1] for i in ids if i.startswith('LK-')})}
    if (rid, text) == ('R06', 'each designated invocation route'):
        return {'R-1': (lambda i: 'R1' in i)}
    if (rid, text) == ('R07', 'all counter controls, including `STALE_WRAPPER_CONTROL`'):
        return {k + ' ' + b: (lambda i, k=k, b=b: i.split('-CB')[0] == k and BY[i]['BUILD_UNDER_TEST'] == b) for k in E6C for b in (PB, CB)}
    if (rid, text) in (('R11', 'the coverage criterion measured'), ('P7', 'coverage criterion measured')):
        return {x: (lambda i, x=x: i == 'WU-DRY-' + x) for x in WSRC}
    if (rid, text) == ('R13', 'event-triggered and silent writer variants'):
        return {'event-triggered': (lambda i: i.endswith('-EV')), 'silent': (lambda i: i.startswith('WR-S'))}
    if (rid, text) == ('R19', 'one qualification scenario per instrument I-01..I-16'):
        out = {}
        for inst in INSTR:
            for b in ((CB,) if inst == 'I-14' else (PB, CB)):
                out[inst + ' ' + b] = (lambda i, inst=inst, b=b: i.split('-CB')[0] == 'Q-' + inst.replace('-', '') and BY[i]['BUILD_UNDER_TEST'] == b)
        return out
    if (rid, text) == ('R20', 'one scenario per row of 17.2'):
        return {'row %d' % n: (lambda i, n=n: bool(re.search(r'17\.2 row %d\b' % n, BY[i]['AUTHORITY_SOURCE_FOR_EXPECTED_RESULT']))) for n in ROWS172}
    if (rid, text) == ('P4', 'all variants of PA-12'):
        return {v: (lambda i, v=v: i == 'PW-CLONE-' + v) for v in PA12_SET}
    return None


bad, bad_state, bad_enum = [], [], []
clauses = table_after(TXT[P_COV], '### 3.2 Discharge by clause', 'Declared exceptions')
seen_rows = set()
quantified_seen = set()
for rid, text, lst_cell, setcell, state in clauses:
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
    mem = enum_members(rid, text)
    if mem is not None:
        quantified_seen.add((rid, text))
        miss = [mname for mname, f in mem.items() if not [i for i in lst if i in BY and f(i) and (BY[i]['STATUS'] != 'EXPLORATORY_NON_GOVERNING' or mname.startswith('E6-C4 '))]]
        mm = re.search(r': (\d+) of (\d+) members covered$', setcell)
        if miss or not mm or int(mm.group(1)) != len(mem) or int(mm.group(2)) != len(mem):
            bad_enum.append(rid + ' ' + text[:40] + ': missing ' + json.dumps(miss[:6]) + ' declared ' + setcell[-30:])
    elif setcell != '-':
        bad_enum.append(rid + ' ' + text[:40] + ': an enumerated set on a clause the check does not recognize as quantified')
if sorted(ex) != sorted([('R01', 'Q-I11'), ('R04', 'Q-I02'), ('R10', 'Q-I09-LOSS'), ('R14', 'Q-I04'), ('R20', 'OU-ROW1-INVALID'), ('R21', 'Q-I16-CLEANUP-NEG'), ('R22', 'PP-POSTCP')]):
    bad.append('declared exceptions: ' + json.dumps(sorted(ex)))
if len(quantified_seen) != 11 or ('R14', 'profile equality') not in {(r[0], r[1]) for r in clauses} or ('R14', 'a deliberate difference') not in {(r[0], r[1]) for r in clauses}:
    bad_enum.append('the eleven quantified clauses or the R14 split are missing: ' + json.dumps(sorted(quantified_seen)))
check('ba02b_section3_discharges', bad, 'each clause is a verbatim fragment of its row; each listed scenario exists and is a class member (OU-O1..O7 strict) or one of '
      'the seven declared exceptions (the PR-TORN-READ rows removed, OU-ROW1-INVALID declared: R1:BA02V4-N05)')
check('ba02b_section3_every_row_discharged', bad_state + [r for r in row_by if r not in seen_rows],
      'every row has clauses and every clause has at least one non-exploratory scenario (MET_IN_DRAFT)')
check('ba02b_section3_enumerated_sets_covered', bad_enum,
      'rule 6 (R1:BA02V4-N04): every clause quantified over an enumerated set covers every member, recomputed from V2.1 4.2, 4.3, 17.2, V2.2 PA-12, V2.1 10.6 and the '
      'catalog (I-01..I-16 and E6-C1..C8 per build; E6-C4 present, G-3); the conjunctive clause of R14 is split')
st3 = table_after(TXT[P_COV], '### 3.3 State per control', '## 4.')
bad = [r[0] for r in st3 if r[2] != 'MET_IN_DRAFT'] + sorted(set(REG) - {r[0].strip('`') for r in st3})
s5rows = {r[0].strip('`'): r[1] for r in st3}
for k in ('S5REC', 'S5MAN', 'S5CLO'):
    if 'R03' not in s5rows.get(k, ''):
        bad.append(k + ': R03 not in its rows (AR4-05 (3))')
check('ba02b_section3_control_states', bad)
check('ba02b_checks_section', [l for l in table_after(TXT[P_COV], '## 5. Design-level checks', '## 6.') if not l[1].startswith('PASS')])
sec3 = section(TXT[P_COV], '## 3. Required scenario classes', '### 3.1')
bad = [x for x in ('ruled by AR4-05', '(1)', '(2)', '(3)', 'LIBRARY_FRESHNESS', 'CLONE_POLICY_VERIFIED', 'GROUP column', 'S5REC, S5MAN and S5CLO') if x not in sec3]
if 'pending' in sec3:
    bad.append('section 3 still reads PA-1.4 as pending')
check('ba02b_pa14_reading_ar4_05', bad)
sec0 = section(TXT[P_COV], '## 0. Inputs', '## 1.')
own_sha = sha(TXT[P_SELF])
check('ba02b_binds_this_check', [] if (own_sha in sec0 and 'BA-02b entry of the BA-11 registry' in sec0 and 'never in the' in sec0) else ['section 0 does not name the SHA-256 ' + own_sha],
      'AR4-17 / R1:BA11V4-04: BA-02b section 0 names this check with its SHA-256 and binds the check and its output to the BA-02b entry of the registry, never to BA-04')

# ------------------------------------------------------------------------------------------------------------------
# 11. Delta V5 tables against the finding lists of the round (decisions section 217 bundles)
# ------------------------------------------------------------------------------------------------------------------
EXP_BA04 = ['R1:BA04V4-01', 'R1:BA04V4-04', 'R1:BA04V4-02', 'R1:BA04V4-03', 'R1:BA04V4-05', 'R1:BA04V4-06', 'R1:BA04V4-07', 'R1:BA04V4-08', 'R1:BA04V4-09',
            'R1:BA04V4-10', 'R1:BA04V4-11', 'R2:CLOSURE BA04V3-17', 'R2:BA04V4-01', 'R2:BA04V4-02', 'R2:BA04V4-03', 'R2:BA04V4-04', 'R2:BA04V4-05',
            'R2:BA04V4-06', 'R2:BA04V4-07', 'R2:BA04V4-08', 'R2:BA04V4-09', 'R2:BA04V4-10', 'R1:BA11V4-01', 'R2:BA11V4-01', 'R2:BA11V4-03', 'R1:BA11V4-04']
EXP_BA02 = ['R1:BA02V4-N01', 'R1:BA02V4-N02', 'R1:BA02V4-N03', 'R1:BA02V4-N04', 'R1:BA02V4-N05', 'R1:BA02V4-N06', 'R-1', 'R-2', 'R-3', 'R-4', 'R-5',
            'R1:BA11V4-01', 'R1:BA11V4-02', 'R2:BA11V4-01', 'R1:BA11V4-04']
d4 = [r[0] for r in table_after(TXT[P_MD], '## 9. Delta V5 (decisions section 217)')]
d2 = [r[0] for r in table_after(TXT[P_COV], '## 7. Delta V5 (decisions section 217)')]
bad = ['BA-04: ' + f for f in EXP_BA04 if not any(r.startswith(f + ' ') or r == f or r.startswith(f + ' [') for r in d4)]
bad += ['BA-02b: ' + f for f in EXP_BA02 if not any(r.startswith(f + ' ') or r == f for r in d2)]
bad += ['BA-04: AR4 decision row ' + a for a in ('AR4-01', 'AR4-02', 'AR4-03', 'AR4-04', 'AR4-06', 'AR4-07', 'AR4-13', 'AR4-14', 'AR4-15', 'AR4-16', 'AR4-17', 'AR4-20')
        if a not in d4]
if not TXT[P_MD].rstrip('\n').split('\n## ')[-1].startswith('9. Delta V5') or not TXT[P_COV].rstrip('\n').split('\n## ')[-1].startswith('7. Delta V5'):
    bad.append('an artifact does not end with its Delta V5 section')
# correction pass of round V5: one labelled row per gap group, each FIXED
d4r = table_after(TXT[P_MD], '## 9. Delta V5 (decisions section 217)')
d2r = table_after(TXT[P_COV], '## 7. Delta V5 (decisions section 217)')
for lst, n, name in ((d4r, 13, 'BA-04'), (d2r, 3, 'BA-02b')):
    for k in range(1, n + 1):
        rr = [r for r in lst if r[0].startswith('correction pass CP-%02d ' % k)]
        if len(rr) != 1 or not rr[0][2].startswith('FIXED'):
            bad.append(name + ': correction pass CP-%02d row missing or not FIXED' % k)
    if len([r for r in lst if r[0].startswith('correction pass CP-')]) != n:
        bad.append(name + ': unexpected number of correction pass rows')
# correction pass (final): the rows of the re-check, each FIXED (BA-04: F-1 build-equivalence pointer, F-2 AQ-V5-08; BA-02b: F-1)
for lst, fin, name in ((d4r, ('F-1', 'F-2'), 'BA-04'), (d2r, ('F-1',), 'BA-02b')):
    for f in fin:
        rr = [r for r in lst if r[0].startswith('correction pass (final) %s ' % f)]
        if len(rr) != 1 or not rr[0][2].startswith('FIXED'):
            bad.append(name + ': correction pass (final) %s row missing or not FIXED' % f)
    if len([r for r in lst if r[0].startswith('correction pass (final)')]) != len(fin):
        bad.append(name + ': unexpected number of correction pass (final) rows')
cp = {r[0].split(' (')[0]: r for r in d4r if r[0].startswith('correction pass CP-')}
if 'AQ-V5-08' not in cp.get('correction pass CP-13', ['', '', ''])[2] or 'review-record location' in cp.get('correction pass CP-09', ['', '', ''])[2]:
    bad.append('BA-04: the rows CP-13 (AQ-V5-08) and CP-09 (superseded in-catalog record) do not follow the correction pass (final)')
r09 = [r for r in d4r if r[0] == 'R1:BA04V4-09']
if not r09 or 'second option' not in r09[0][2] or 'A1 is removed from the controls a mirror run reaches' in r09[0][2]:
    bad.append('BA-04: the R1:BA04V4-09 row does not record the second option of the correction pass')
check('delta_v5_tables_cover_the_findings', bad, 'every finding of the round for BA-04 and BA-02b (both passes, R1:/R2:) and every AR4 decision applied has a row in the '
      'Delta V5 table that ends each artifact')

# ------------------------------------------------------------------------------------------------------------------
# report
# ------------------------------------------------------------------------------------------------------------------
counts = O([
    ('total', len(SC)), ('own', len(own)), ('wu_dry', len(SC) - len(own)),
    ('by_run_governance', O(sorted(Counter(s['RUN_GOVERNANCE'] for s in SC).items()))),
    ('by_group', O(sorted(Counter(s['GROUP'] for s in SC).items()))),
    ('by_status', O(sorted(Counter(s['STATUS'] for s in SC).items()))),
    ('by_build_under_test', O(sorted(Counter(s['BUILD_UNDER_TEST'] for s in SC).items()))),
    ('by_repetition', O(sorted(Counter(s['REPETITION'] for s in SC).items()))),
    ('blockers', O(sorted(Counter(b for s in SC for b in s['BLOCKED_BY']).items()))),
    ('pin_slots', O(sorted(Counter(p.split('@')[0] for s in SC for p in s['PIN_SLOTS']).items()))),
    ('run_prerequisites', O(sorted(Counter(p for s in SC for p in s['RUN_PREREQUISITES']).items()))),
    ('ar3_01_mode', len(mode)), ('per_build_twins', len(exp_twins)), ('ev_l', len(evl)),
    ('dry_run_branch_sources', exp_branch), ('dry_run_load_sources', exp_load),
    ('cl_clean', [i for i in ids if i.startswith('CL-CLEAN-FX')]),
    ('param_03_sample', sample),
    ('conditional_negative_control_rows', cond),
    ('controls_with_characterization_build_negatives_only', cb_only),
])
failed = [k for k, v in RESULTS.items() if v['result'] != 'PASS']
report = O([
    ('checker', P_SELF),
    ('checker_sha256_lf', own_sha),
    ('inputs_sha256_lf', O((p, sha(TXT[p])) for p in (P_JSON, P_MD, P_COV, P_REG, P_V21, P_V22, P_FPV, P_BA07))),
    ('hash_method', 'SHA-256 of the UTF-8 file text after CRLF -> LF normalization, lowercase hex'),
    ('summary', O([('checks', len(RESULTS)), ('passed', len(RESULTS) - len(failed)), ('failed', failed)])),
    ('counts', counts),
    ('checks', RESULTS),
])
if len(sys.argv) != 2:
    sys.stderr.write('usage: python I-52-ct21d-ba04-v5-checks.py <output.json>\n')
    sys.exit(2)
io.open(sys.argv[1], 'w', encoding='utf-8', newline='').write(json.dumps(report, indent=1, ensure_ascii=False) + '\n')
print('checks %d, passed %d, failed %s' % (len(RESULTS), len(RESULTS) - len(failed), failed))
sys.exit(0 if not failed else 1)
