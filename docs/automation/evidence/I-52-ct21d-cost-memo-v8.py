"""I-52 CT-21D cost memo V8: the run plan implied by the Owner and Coordinator values (NOT SEALED; arithmetic only).

Reads docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json (draft, unchanged, 437 scenarios) and evaluates the
symbolic run expression of BA-10 V8 section 2.3 for the values that BA-10 V8 section 2.4 records: the Owner pairs of Q-O1..Q-O4
(separate pairs: N_A from p_A, alpha_A; N_B from p_B, alpha_B; N_det from p_det, alpha_det; n from u, c), WU_DRY_RUNS = N_B, and
the Coordinator values PARAM-04 m and EVIDENCE_REPETITION (decisions section 233). The memo chooses nothing: it reads the pairs and
the values from BA-10 V8, derives the counts by the formulas of BA-10 V8 section 2.2 in exact rational arithmetic, and FAILS
(exit 1, no figures printed) if BA-10 V8 does not state N_A = 44, N_B = 29, N_det = 90, n = 59, WU_DRY_RUNS = 29, m = 3 and
EVIDENCE_REPETITION = 1, or if the counts derived from the pairs differ from the stated ones.

It keeps the structure of the V7 memo: the counts of the catalog (block A), the plan (block B: terms per rule, CHAR kinds, the split
by build under test, the PARAM-03 sample, the worst-case governing attempts m * G), the reconciliation with the V7 and V6 memos
(block C), sensitivities re-based on the actual pairs (block D), the checks of the rulings (block E) and the runs by blocker, status
and run prerequisite (block F). The V7 memo illustrated four rows with one pair for both classes; V8 has one choice.

HDM-CHAR-* (AR4-14 (b)(ii)): CHAR_RUNS(s) = N_A + V(s). V(s) is read per scenario from its declared CONTROL_PLANE_SETUP injections
in the catalog and checked against the closed key list of BA-08 V7 section 11.2 (rows HDM-2 and HDM-4). EV-L-COMPOSED (AR4-01):
CHAR_RUNS = N_det + 1 (BA-10 V8 section 2.2; ruled by AR5-08).

Usage: python I-52-ct21d-cost-memo-v8.py [<repo root>]
The output is plain text (deterministic). File hashes are SHA-256 over the LF-normalized bytes, lowercase hex; the git blob id is
the SHA-1 of "blob <size> NUL <raw bytes>" (what git hash-object prints).
"""
import hashlib
import io
import json
import math
import os
import re
import sys
from collections import Counter, OrderedDict
from decimal import Decimal, getcontext
from fractions import Fraction

getcontext().prec = 60

ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..'))
INIT = os.path.join(ROOT, 'docs', 'initiatives')
EVID = os.path.join(ROOT, 'docs', 'automation', 'evidence')
CAT7 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json')
CAT6 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-04-scenario-catalog-v6.json')
BA08 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md')
BA08_V6 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-08-selective-kind-baseline-v6.md')
BA10 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md')
MEMO6 = os.path.join(EVID, 'I-52-ct21d-cost-memo-v6.md')
MEMO7 = os.path.join(EVID, 'I-52-ct21d-cost-memo-v7.md')
MANDATORY = ('E4-LEARN-R1-', 'LK-', 'HDM-CHAR-', 'WU-DRY-')   # AR7-04: the families that feed a pin, a selection or the warm-up criterion

# The values BA-10 V8 must state (the Owner's Q-O1..Q-O4 counts, WU_DRY_RUNS = N_B, the Coordinator's m and EVIDENCE_REPETITION).
# They are the check, not an input: the memo uses what BA-10 V8 states and fails if it differs from these.
EXPECTED = OrderedDict([('N_A', 44), ('N_B', 29), ('N_det', 90), ('n', 59), ('WU_DRY_RUNS', 29), ('m', 3), ('EVIDENCE_REPETITION', 1)])

OUT = []


def say(line=''):
    OUT.append(line)


def rel(path):
    return os.path.relpath(path, ROOT).replace(os.sep, '/')


def lf_bytes(path):
    return io.open(path, 'rb').read().replace(b'\r\n', b'\n')


def lf_sha256(path):
    return hashlib.sha256(lf_bytes(path)).hexdigest()


def git_blob(path):
    raw = io.open(path, 'rb').read()
    return hashlib.sha1(b'blob %d\0' % len(raw) + raw).hexdigest()


def fail(msg):
    sys.stderr.write('FAIL: %s\n' % msg)
    sys.exit(1)


# ---------------------------------------------------------------- the formulas of BA-10 V8 section 2.2, in exact arithmetic
def n_zero_failure(p, alpha):
    """Smallest integer N with (1 - p)^N <= alpha (the formula N >= ln(alpha) / ln(1 - p), rounded up), in rational arithmetic.
    p and alpha are decimal strings or Fractions."""
    p, alpha = Fraction(p), Fraction(alpha)
    q = 1 - p
    n = max(1, int(math.ceil(math.log(float(alpha)) / math.log(float(q)))) - 1)
    while q ** n > alpha:
        n += 1
    while n > 1 and q ** (n - 1) <= alpha:
        n -= 1
    return n


def n_sample(u, c):
    """Smallest integer n with 1 - (1 - c)^(1/n) <= u, i.e. (1 - u)^n <= 1 - c."""
    return n_zero_failure(u, 1 - Fraction(c))


def cp_bound(c, n):
    """The k = 0 one-sided Clopper-Pearson upper bound at confidence c: 1 - (1 - c)^(1/n), 60-digit decimal arithmetic."""
    return 1 - (1 - Decimal(c)) ** (Decimal(1) / Decimal(n))


# ---------------------------------------------------------------- BA-10 V8: the values it states (read; fail closed)
ba10_text = io.open(BA10, encoding='utf-8').read()
block_src = ba10_text.split('The value block below', 1)
if len(block_src) != 2 or '```text\n' not in block_src[1]:
    fail('%s: the value block of section 2.4 was not found' % rel(BA10))
block = block_src[1].split('```text\n', 1)[1].split('```', 1)[0]
STATED = OrderedDict()
for ln in block.split('\n'):
    if ln.strip():
        m_ = re.match(r'^([A-Za-z_]+) = (\S+)$', ln)
        if not m_:
            fail('%s: the value block has a line that does not read "key = value": %r' % (rel(BA10), ln))
        STATED[m_.group(1)] = m_.group(2)
PAIR_KEYS = ('p_A', 'alpha_A', 'p_B', 'alpha_B', 'p_det', 'alpha_det', 'u', 'c')
for k in PAIR_KEYS + ('N_A', 'N_B', 'N_det', 'n', 'WU_DRY_RUNS'):
    if k not in STATED:
        fail('%s: the value block does not state %s' % (rel(BA10), k))


def table_value(first_cell_prefix, second_cell_part):
    """The 'Recorded value' cell (column 4) of the section 2.4 row whose first cell starts with the prefix and whose second cell
    contains the given text."""
    rows = [x for x in ba10_text.split('\n') if x.startswith(first_cell_prefix) and len(x.split('|')) > 4
            and second_cell_part in x.split('|')[2]]
    if len(rows) != 1:
        fail('%s: expected one section 2.4 row for %s, found %d' % (rel(BA10), first_cell_prefix, len(rows)))
    return rows[0].split('|')[3].strip().strip('`')


STATED['m'] = table_value('| `m` (`PARAM-04`)', 'MAX_ATTEMPTS')
STATED['EVIDENCE_REPETITION'] = table_value('| `EVIDENCE_REPETITION` |', 'runs of a scenario')
try:
    STATED_INT = OrderedDict((k, int(STATED[k])) for k in EXPECTED)
except ValueError:
    fail('%s: a stated count is not an integer: %r' % (rel(BA10), [STATED[k] for k in EXPECTED]))

PAIRS = OrderedDict((k, STATED[k]) for k in PAIR_KEYS)
DERIVED = OrderedDict([('N_A', n_zero_failure(PAIRS['p_A'], PAIRS['alpha_A'])),
                       ('N_B', n_zero_failure(PAIRS['p_B'], PAIRS['alpha_B'])),
                       ('N_det', n_zero_failure(PAIRS['p_det'], PAIRS['alpha_det'])),
                       ('n', n_sample(PAIRS['u'], PAIRS['c']))])
DERIVED['WU_DRY_RUNS'] = DERIVED['N_B']          # WU_DRY_RUNS = N_B (BA-10 V8 section 2.2)
for k in ('N_A', 'N_B', 'N_det', 'n', 'WU_DRY_RUNS'):
    if DERIVED[k] != STATED_INT[k]:
        fail('the count derived from the pairs differs from the one BA-10 V8 states: %s derived %d, stated %d' % (k, DERIVED[k], STATED_INT[k]))
for k, want in EXPECTED.items():
    if STATED_INT[k] != want:
        fail('BA-10 V8 states %s = %d, expected %d' % (k, STATED_INT[k], want))
if 'WU_DRY_RUNS = N_B' not in ba10_text or 'The worst case is `3 * G` governing attempts' not in ba10_text:
    fail('BA-10 V8 does not state WU_DRY_RUNS = N_B or the worst case 3 * G')
CP58 = cp_bound(PAIRS['c'], DERIVED['n'] - 1)
CP59 = cp_bound(PAIRS['c'], DERIVED['n'])
CP58_S = format(CP58, '.30f')
CP59_S = format(CP59, '.30f')
if not (CP58 > Decimal(PAIRS['u']) >= CP59):
    fail('the k = 0 bound is not above u at n - 1 and at most u at n')
if CP58_S not in ba10_text or CP59_S not in ba10_text:
    fail('BA-10 V8 does not print the k = 0 bound as computed here (%s at n-1, %s at n)' % (CP58_S, CP59_S))

# ---------------------------------------------------------------- BA-08 section 11.2: the closed key list of HOST_DEFAULT_MAP
HDM_VARIABLES = ('CLAYER', 'CECOLOR', 'CELTYPE', 'CELTSCALE', 'CELWEIGHT', 'CETRANSPARENCY', 'CPLOTSTYLE')
HDM_STYLES_PHRASE = 'and the current text style and dimension style'
HDM4_PHRASE = 'plus **exactly one** informative run per varied key'   # the same phrase in BA-08 V6 and V7


def hdm_keys(path, hdm4_phrase):
    """The key list of BA-08 section 11.2 HDM-2, read from the file; stops (fail closed) if the rows do not read as expected."""
    lines = io.open(path, encoding='utf-8').read().split('\n')
    hdm2 = [x for x in lines if x.startswith('| HDM-2 ')]
    hdm4 = [x for x in lines if x.startswith('| HDM-4 ')]
    if len(hdm2) != 1 or len(hdm4) != 1:
        fail('%s: expected one HDM-2 row and one HDM-4 row, found %d and %d' % (rel(path), len(hdm2), len(hdm4)))
    cell = hdm2[0].split('|')[2]
    head, sep, _ = cell.partition(HDM_STYLES_PHRASE)
    if not sep:
        fail('%s HDM-2 does not name the current text style and dimension style as keys' % rel(path))
    variables = tuple(re.findall(r'`([A-Z]+)`', head))
    if variables != HDM_VARIABLES:
        fail('%s HDM-2 context variables differ from the list this memo expects: %r' % (rel(path), variables))
    if hdm4_phrase not in hdm4[0]:
        fail('%s HDM-4 does not state the expected varied-key rule' % rel(path))
    return variables + ('current text style', 'current dimension style')


HDM_KEYS = hdm_keys(BA08, HDM4_PHRASE)
HDM_KEYS_V6 = hdm_keys(BA08_V6, HDM4_PHRASE)

# ---------------------------------------------------------------- V7 catalog
cat = json.load(io.open(CAT7, encoding='utf-8'))
GT = cat['GROUP_TABLE']
SAMPLE = list(cat['PARAM_03_SAMPLE'])
SC = cat['SCENARIOS']
byid = {s['SCENARIO_ID']: s for s in SC}
TWINS = list(cat['PER_BUILD_QUALIFICATION']['twins'])
TWINS_AR505 = list(cat['PER_BUILD_QUALIFICATION']['twins_by_content_ar5_05'])
I14_FAMILY = list(cat['PER_BUILD_QUALIFICATION']['characterization_build_only'])
BUILDS = ['PRODUCT_BUILD', 'CHARACTERIZATION_BUILD', 'NONE']
K = len(SAMPLE)

V = OrderedDict([('N_A', STATED_INT['N_A']), ('N_B', STATED_INT['N_B']), ('n', STATED_INT['n']), ('N_det', STATED_INT['N_det']),
                 ('E', STATED_INT['EVIDENCE_REPETITION']), ('K', K)])
M = STATED_INT['m']


def composition(s):
    """Classes of the class-A and class-B CONTROL groups the scenario serves (GROUPS_SERVED; KR and evidence groups excluded;
    CONTRACT_ASSIGNED_GROUPS never enter N_strict: AR4-14 (a), section 217)."""
    cls = sorted({GT[g]['class'] for g in s['GROUPS_SERVED'] if GT[g]['kind'] == 'CONTROL' and GT[g]['class'] in ('A', 'B')})
    return {'A': 'A_ONLY', 'AB': 'MIXED', 'B': 'B_ONLY', '': 'NONE'}[''.join(cls)]


def derived_rule(s):
    """The rule of BA-10 V8 section 2.2 (table of repetition rules, first matching row), derived from the scenario's properties."""
    comp = composition(s)
    if s['MODE'] == 'DRY_RUN':
        return 'DRY'
    if s['MODE'] == 'LEARNING':
        return 'LEARN'          # every learning micro-scenario, governing or not, whatever groups it serves (R1/R2:BA10-V4-01)
    if s['RUN_GOVERNANCE'] != 'GOVERNING':
        return 'CHAR'
    if s['SCENARIO_ID'] in SAMPLE:
        return 'CLEAN'
    if s['BUILD_UNDER_TEST'] == 'NONE' and comp == 'NONE':
        return 'SYNTH'
    if s['MODE'] == 'VALIDATION' and s['GROUP'] == 'E12-V':
        return 'VALIDATE'
    if comp == 'NONE':
        return 'EVIDENCE'
    if GT[s['GROUP']]['kind'] == 'EVIDENCE':
        return 'EVIDENCE_STRICT'
    return 'DEFAULT'


def char_kind(s):
    """CHAR_RUNS kind of BA-10 V8 section 2.2: exploratory; HOST_DEFAULT_MAP (label HDM); the event-model composition check
    (the non-governing E12-L scenario that is not a learning micro-scenario, AR4-01); other pin- or selection-feeding runs."""
    if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING':
        return 'EXPLORATORY'
    if s['GROUP'] == 'HDM':
        return 'HDM'
    if s['GROUP'] == 'E12-L':
        return 'COMPOSITION'
    return 'PIN_FEEDING'


KEY_OF_VARIANT = OrderedDict([(k, k) for k in HDM_VARIABLES] + [('the current text style', 'current text style'),
                                                                   ('the current dimension style', 'current dimension style')])


def varied_keys(s):
    """The keys an HDM-CHAR-* scenario varies: one CONTROL_PLANE_SETUP injection per key, 'informative run: <key> changed ...'."""
    keys = []
    for x in s['INJECTIONS']:
        m_ = re.match(r'informative run: (.+?) changed to ', x.get('variant', ''))
        if x.get('writer') != 'CONTROL_PLANE_SETUP' or not m_ or m_.group(1) not in KEY_OF_VARIANT:
            fail('%s: injection that is not a declared key change: %r' % (s['SCENARIO_ID'], x))
        keys.append(KEY_OF_VARIANT[m_.group(1)])
    return keys


HDM_SC = [s for s in SC if s['GROUP'] == 'HDM']
V_OF = {}
hdm_check = []
for s in HDM_SC:
    ks = varied_keys(s)
    once = len(ks) == len(set(ks))
    closed = set(ks) <= set(HDM_KEYS)
    stated = ('V(s) = %d' % len(ks)) in s['PLAN_OR_INPUT'] and 'exactly one informative run per key' in s['PLAN_OR_INPUT']
    if not (once and closed and stated):
        fail('%s: varied keys not exactly once, outside the HDM-2 list, or V(s) not stated: %r' % (s['SCENARIO_ID'], ks))
    V_OF[s['SCENARIO_ID']] = len(ks)
    hdm_check.append((s['SCENARIO_ID'], len(ks), len(set(HDM_KEYS) - set(ks))))

NS_LABEL = {'A_ONLY': 'N_A', 'MIXED': 'max(N_A, N_B)', 'B_ONLY': 'N_B'}
CHAR_LABEL = {'PIN_FEEDING': 'CHAR_RUNS = N_A',
              'HDM': 'CHAR_RUNS = N_A at the pinned key values, plus exactly one informative run per varied key (outside the intent)',
              'COMPOSITION': 'CHAR_RUNS = PARAM-02 (non-governing composition check)',
              'EXPLORATORY': 'CHAR_RUNS = EVIDENCE_REPETITION (exploratory)'}


def learn_strict(s, comp):
    """A governing learning micro-scenario that serves a class-A or class-B control group (R(s) = max(N_strict, N_det + 1))."""
    return s['RUN_GOVERNANCE'] == 'GOVERNING' and comp != 'NONE'


def derived_label(s, r, comp):
    """The exact REPETITION label (BA-04 notation) of the BA-10 rule, with N_strict and the CHAR_RUNS kind resolved.
    BA-04 has no label for a governing learning micro-scenario that serves a class-A or class-B group (none exists): None."""
    ns = NS_LABEL.get(comp)
    if r == 'LEARN' and learn_strict(s, comp):
        return None
    return {'DEFAULT': 'DEFAULT: %s' % ns,
            'CLEAN': 'CLEAN: max(%s, ceil(n / K))' % ns,
            'VALIDATE': 'VALIDATE: max(%s, PARAM-02 validation count)' % ns,
            'EVIDENCE': 'EVIDENCE: EVIDENCE_REPETITION',
            'EVIDENCE_STRICT': 'EVIDENCE_STRICT: max(%s, EVIDENCE_REPETITION)' % ns,
            'LEARN': 'LEARN: PARAM-02',
            'SYNTH': 'SYNTH: ONE',
            'DRY': 'DRY: WU_DRY_RUNS = N_B',
            'CHAR': 'CHAR: %s' % CHAR_LABEL[char_kind(s)]}[r]


rows = []
mismatch = []
label_mismatch = []
for s in SC:
    r = derived_rule(s)
    comp = composition(s)
    cat_rule = s['REPETITION'].split(':')[0].strip()
    if r != cat_rule:
        mismatch.append((s['SCENARIO_ID'], cat_rule, r))
    if s['REPETITION'] != derived_label(s, r, comp):
        label_mismatch.append((s['SCENARIO_ID'], s['REPETITION'], derived_label(s, r, comp)))
    rows.append((s, r, comp))

sample_ok = all(sid in byid and byid[sid]['RUN_GOVERNANCE'] == 'GOVERNING' and derived_rule(byid[sid]) == 'CLEAN' for sid in SAMPLE)
clean_not_in_sample = [s['SCENARIO_ID'] for s in SC if s['SCENARIO_ID'].startswith('CL-CLEAN-') and s['SCENARIO_ID'] not in SAMPLE]
mixed_governing = [s for s, r, comp in rows if comp == 'MIXED' and s['RUN_GOVERNANCE'] == 'GOVERNING']
mixed_without_max = [s['SCENARIO_ID'] for s in mixed_governing if 'max(N_A, N_B)' not in s['REPETITION']]
learn_serving_ab = [s['SCENARIO_ID'] for s, r, comp in rows if r == 'LEARN' and learn_strict(s, comp)]

RULES = ['DEFAULT', 'CLEAN', 'VALIDATE', 'LEARN', 'EVIDENCE', 'EVIDENCE_STRICT', 'SYNTH', 'DRY', 'CHAR']
CHAR_KINDS = ['PIN_FEEDING', 'HDM', 'COMPOSITION', 'EXPLORATORY']
COMPS = ['A_ONLY', 'MIXED', 'B_ONLY', 'NONE']
cnt = Counter((r, comp) for s, r, comp in rows)
rule_tot = Counter(r for s, r, comp in rows)
char_rows = [(s, r, comp) for s, r, comp in rows if r == 'CHAR']
char_cnt = Counter(char_kind(s) for s, r, comp in char_rows)
composition_ids = [s['SCENARIO_ID'] for s, r, comp in char_rows if char_kind(s) == 'COMPOSITION']
composition_profile_ok = all(byid[x]['EXPECTED_LOG_RECORD_SET'] == 'LP-COMPOSE' for x in composition_ids)

# ---------------------------------------------------------------- BA-10 V8: the rules this memo applies must be stated there
BA10_PHRASES = OrderedDict([
    ('HDM-CHAR-* rule (AR4-14 (b)(ii))', '`N_A` runs at the pinned key configuration plus **exactly one** informative run per varied key'),
    ('EV-L-COMPOSED rule (AR4-01)', '`CHAR_RUNS(s) = N_det + 1` for the composition check `EV-L-COMPOSED`'),
    ('LEARN rule (R1/R2:BA10-V4-01)', '`max(N_strict(s), N_det + 1)` when it is governing and serves a class-A or class-B control group'),
    ('N_strict over GROUPS_SERVED only (AR4-14 (a))', '`CONTRACT_ASSIGNED_GROUPS` never enter `N_strict` (ruled by AR4-14 (a)'),
    ('WU_DRY_RUNS = N_B (class of WU)', '`WU_DRY_RUNS = N_B`'),
    ('worst case m * G per repetition slot (PARAM-04)', 'The worst case is `3 * G` governing attempts'),
])
ba10_states = OrderedDict((k, v in ba10_text) for k, v in BA10_PHRASES.items())
ba10_v4_phrase_gone = 'at least one informative run' not in ba10_text
ba10_aq06_rows = [x for x in ba10_text.split('\n') if x.startswith('| **AQ-V5-06** ')]
ba10_aq06_ruled = (len(ba10_aq06_rows) == 1 and '**AR5-08**' in ba10_aq06_rows[0] and 'N_det + 1' in ba10_aq06_rows[0]
                   and 'EV-L-COMPOSED' in ba10_aq06_rows[0] and 'OPEN' not in ba10_aq06_rows[0])
ba10_rules = re.findall(r'^\| `([A-Z_]+)` \| ', ba10_text.split('**Repetition rules', 1)[1].split('**`WU_DRY_RUNS` (', 1)[0], re.M)
if not all(ba10_states.values()) or not ba10_aq06_ruled:
    fail('BA-10 V8 does not state a rule this memo applies: %r' % [k for k, v in ba10_states.items() if not v])
if sorted(ba10_rules) != sorted(RULES):
    fail('the rules of the BA-10 V8 section 2.2 table differ from the rules this memo applies: %r' % ba10_rules)


def n_strict(comp, NA, NB):
    return {'A_ONLY': NA, 'MIXED': max(NA, NB), 'B_ONLY': NB, 'NONE': 0}[comp]


def runs(s, r, comp, v):
    """Planned runs R(s) of BA-10 V8 section 2.2 for the parameter values v."""
    NA, NB, n, Nd, E, k = v['N_A'], v['N_B'], v['n'], v['N_det'], v['E'], v['K']
    ns = n_strict(comp, NA, NB)
    if r == 'DEFAULT':
        return ns
    if r == 'CLEAN':
        return max(ns, math.ceil(n / k))
    if r == 'VALIDATE':
        return max(ns, Nd)
    if r == 'LEARN':
        return max(ns, Nd + 1) if learn_strict(s, comp) else Nd + 1
    if r == 'EVIDENCE':
        return E
    if r == 'EVIDENCE_STRICT':
        return max(ns, E)
    if r == 'SYNTH':
        return 1
    if r == 'DRY':
        return NB
    if r == 'CHAR':
        kind = char_kind(s)
        if kind == 'EXPLORATORY':
            return E
        if kind == 'HDM':
            return NA + V_OF[s['SCENARIO_ID']]
        if kind == 'COMPOSITION':
            return Nd + 1
        return NA
    raise ValueError(r)


def evaluate(v, extra_sample=0):
    """TOTAL, split by rule, into governing planned runs G (repetition slots) and non-governing runs D, and by build.
    extra_sample adds that many new PARAM-03 sample members (mixed class, like the others) with one dry-run scenario each."""
    per = Counter()
    per_build = Counter()
    G = D = 0
    for s, r, comp in rows:
        x = runs(s, r, comp, v)
        per[r] += x
        gov = s['RUN_GOVERNANCE'] == 'GOVERNING'
        per_build[(s['BUILD_UNDER_TEST'], 'G' if gov else 'D')] += x
        if gov:
            G += x
        else:
            D += x
    for _ in range(extra_sample):
        x = max(n_strict('MIXED', v['N_A'], v['N_B']), math.ceil(v['n'] / v['K']))
        per['CLEAN'] += x
        per['DRY'] += v['N_B']
        G += x
        D += v['N_B']
    return per, G, D, G + D, per_build


def total(v, extra_sample=0):
    return evaluate(v, extra_sample)[3]


def rsum(sel, v=None):
    return sum(runs(s, r, comp, v or V) for s, r, comp in sel)


per, G, D, T, pb = evaluate(V)

say('I-52 CT-21D COST MEMO V8 - ARITHMETIC OUTPUT (NOT SEALED; NOT A REGISTRY ENTRY; ARITHMETIC ONLY; THE MEMO CHOOSES NOTHING)')
say('catalog file    : %s' % rel(CAT7))
say('catalog SHA-256 : %s (LF-normalized bytes)' % lf_sha256(CAT7))
say('catalog version : %s ; scenarios: %d' % (cat['CATALOG_VERSION'], len(SC)))
say('key list file   : %s (section 11.2, rows HDM-2 and HDM-4)' % rel(BA08))
say('key list hash   : %s (SHA-256, LF-normalized bytes)' % lf_sha256(BA08))
say('rule sheet file : %s' % rel(BA10))
say('rule sheet blob : %s (git blob id of the file bytes)' % git_blob(BA10))
say('rule sheet hash : %s (SHA-256, LF-normalized bytes)' % lf_sha256(BA10))
say()
say('P. VALUES (read from BA-10 V8 section 2.4; counts derived from the pairs by the formulas of section 2.2, exact rational arithmetic)')
say('   %-22s %-8s %-8s %8s %8s %8s' % ('count', 'p / u', 'alpha / c', 'derived', 'stated', 'expected'))
for name, a, b in (('N_A (Q-O1)', 'p_A', 'alpha_A'), ('N_B (Q-O2)', 'p_B', 'alpha_B'), ('N_det (Q-O3)', 'p_det', 'alpha_det'), ('n (Q-O4)', 'u', 'c')):
    key = name.split(' ')[0]
    say('   %-22s %-8s %-8s %8d %8d %8d' % (name, PAIRS[a], PAIRS[b], DERIVED[key], STATED_INT[key], EXPECTED[key]))
say('   %-22s %-8s %-8s %8d %8d %8d' % ('WU_DRY_RUNS (= N_B)', '', '', DERIVED['WU_DRY_RUNS'], STATED_INT['WU_DRY_RUNS'], EXPECTED['WU_DRY_RUNS']))
say('   %-22s %-8s %-8s %8s %8d %8d' % ('m (PARAM-04, Coordinator)', '', '', '-', STATED_INT['m'], EXPECTED['m']))
say('   %-22s %-8s %-8s %8s %8d %8d' % ('EVIDENCE_REPETITION', '', '', '-', STATED_INT['EVIDENCE_REPETITION'], EXPECTED['EVIDENCE_REPETITION']))
say('   every derived count equals the stated one and the expected one: yes (the script exits 1 otherwise)')
say('P.1 the ratios behind the counts (60-digit decimal arithmetic, BA-10 V8 section 2.4):')
for key, a, b in (('N_A', 'p_A', 'alpha_A'), ('N_B', 'p_B', 'alpha_B'), ('N_det', 'p_det', 'alpha_det')):
    ratio = Decimal(PAIRS[b]).ln() / (1 - Decimal(PAIRS[a])).ln()
    say('    %-6s ln(%s) / ln(1 - %s) = %s -> rounded up %d' % (key, PAIRS[b], PAIRS[a], format(ratio, '.15f'), DERIVED[key]))
ratio_n = (1 - Decimal(PAIRS['c'])).ln() / (1 - Decimal(PAIRS['u'])).ln()
say('    %-6s ln(1 - %s) / ln(1 - %s) = %s -> rounded up %d' % ('n', PAIRS['c'], PAIRS['u'], format(ratio_n, '.15f'), DERIVED['n']))
say('P.2 PARAM-03 method (k = 0): one-sided Clopper-Pearson upper bound at confidence c = 1 - (1 - c)^(1/n); the smallest n with the bound <= u')
say('    n = %d : 1 - (1 - %s)^(1/%d) = %s   > u = %s' % (DERIVED['n'] - 1, PAIRS['c'], DERIVED['n'] - 1, CP58_S, PAIRS['u']))
say('    n = %d : 1 - (1 - %s)^(1/%d) = %s   <= u = %s' % (DERIVED['n'], PAIRS['c'], DERIVED['n'], CP59_S, PAIRS['u']))
say('    both figures are printed in BA-10 V8 section 2.2 (PARAM-03): yes')
say('P.3 N_B < N_A (%d < %d): no ordering is assumed by the rules; N_strict(s) = N_A for class-A-only and for mixed-class scenarios,'
    % (V['N_B'], V['N_A']))
say('    N_B for class-B-only scenarios, and WU_DRY_RUNS = N_B. N_A is applied only where the rules of BA-10 V8 section 2.2 say so (block B.4).')
say()
say('A. COUNTS (read from the draft catalog; they change with the catalog)')
say('A.1 rule derived from the scenario properties (BA-10 V8 section 2.2) vs the REPETITION field of the catalog: %s'
    % ('all %d agree' % len(SC) if not mismatch else 'MISMATCH %r' % mismatch))
say('A.2 scenarios per rule and class composition (classes of the class-A/B CONTROL groups of GROUPS_SERVED; KR, evidence')
say('    groups and CONTRACT_ASSIGNED_GROUPS excluded, AR4-14 (a))')
say('    %-16s %6s %7s %6s %7s %6s' % ('rule', 'total', 'A_ONLY', 'MIXED', 'B_ONLY', 'NONE'))
for r in RULES:
    say('    %-16s %6d %7d %6d %7d %6d' % (r, rule_tot[r], cnt[(r, 'A_ONLY')], cnt[(r, 'MIXED')], cnt[(r, 'B_ONLY')], cnt[(r, 'NONE')]))
say('    %-16s %6d' % ('ALL', len(SC)))
say('    LEARN: %d learning micro-scenarios, %d governing; governing ones that serve a class-A or class-B group (R(s) = max(N_strict,'
    % (rule_tot['LEARN'], sum(1 for s, r, comp in rows if r == 'LEARN' and s['RUN_GOVERNANCE'] == 'GOVERNING')))
say('    N_det + 1)): %d%s' % (len(learn_serving_ab), (' (%s)' % ', '.join(learn_serving_ab)) if learn_serving_ab else ''))
say('A.3 CHAR split: pin- or selection-feeding %d (E4-LEARN-R1-*, LK-*); HOST_DEFAULT_MAP %d (HDM-CHAR-*); composition check %d (%s);'
    % (char_cnt['PIN_FEEDING'], char_cnt['HDM'], char_cnt['COMPOSITION'], ', '.join(composition_ids)))
say('    exploratory %d (%s); the composition check has the log profile LP-COMPOSE: %s'
    % (char_cnt['EXPLORATORY'], ', '.join(s['SCENARIO_ID'] for s, r, comp in char_rows if char_kind(s) == 'EXPLORATORY'),
       'yes' if composition_profile_ok else 'NO'))
say('A.4 PARAM-03 sample (catalog field PARAM_03_SAMPLE): K = %d; every member GOVERNING with rule CLEAN: %s' % (K, 'yes' if sample_ok else 'NO'))
say('    members: %s' % ', '.join(SAMPLE))
say('    members per build: %s' % ', '.join('%s %d' % (b, x) for b, x in sorted(Counter(byid[m_]['BUILD_UNDER_TEST'] for m_ in SAMPLE).items())))
say('    on the CHARACTERIZATION_BUILD: %s (its PARAM-03 contribution holds for the PRODUCT_BUILD only under the recorded build'
    % ', '.join(m_ for m_ in SAMPLE if byid[m_]['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD'))
say('    equivalence, AR4-06 (6))')
say('    CL-CLEAN-* ids outside the sample: %s' % (', '.join(clean_not_in_sample) or 'none'))
say('A.5 exact REPETITION label (N_strict resolved to N_A, N_B or max(N_A, N_B); CHAR_RUNS kind resolved) vs the catalog: %s'
    % ('all %d agree' % len(SC) if not label_mismatch else 'MISMATCH %r' % label_mismatch))
say('    governing mixed-class scenarios: %d; of them with a label without max(N_A, N_B): %d'
    % (len(mixed_governing), len(mixed_without_max)))
say('A.6 HOST_DEFAULT_MAP keys (BA-08 V7 section 11.2 HDM-2, closed list): %s = %d keys' % (', '.join(HDM_KEYS), len(HDM_KEYS)))
say('    BA-08 V7 HDM-4: N_A runs at the pinned key configuration plus exactly one informative run per varied key (AR4-14 (b)(ii))')
say('    V(s) read from the declared key-change injections of each scenario (each key once, from the closed list, V(s) stated):')
for sid, vs, missing in hdm_check:
    say('    %-22s V(s) = %d ; keys of the list it does not vary: %d' % (sid, vs, missing))
say('    informative runs in all: %d (outside the intent; no Owner answer moves them)' % sum(V_OF.values()))
say('A.7 BA-10 V8 states the rules this memo applies:')
for k, ok in ba10_states.items():
    say('    %-48s %s' % (k, 'yes' if ok else 'NO'))
say('    the V4 wording "at least one informative run" is absent: %s' % ('yes' if ba10_v4_phrase_gone else 'NO'))
say('    the composition-check count is recorded as ruled by AR5-08 (AQ-V5-06; BA-10 V8 section 6.1): %s'
    % ('yes' if ba10_aq06_ruled else 'NO'))
say('    rules of its section 2.2 table: %s' % ', '.join(ba10_rules))
say('A.8 scenarios per build under test (AR4-06) and governance')
say('    %-24s %9s %9s %9s %9s' % ('BUILD_UNDER_TEST', 'total', 'governing', 'non-gov.', 'explor.'))
for b in BUILDS:
    sel = [s for s in SC if s['BUILD_UNDER_TEST'] == b]
    say('    %-24s %9d %9d %9d %9d' % (b, len(sel), sum(1 for s in sel if s['RUN_GOVERNANCE'] == 'GOVERNING'),
                                        sum(1 for s in sel if s['RUN_GOVERNANCE'] == 'NON_GOVERNING_BY_DESIGN'),
                                        sum(1 for s in sel if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING')))
twin_ok = []
for t in TWINS:
    base_id = t[:-3]
    base = byid.get(base_id)
    tw = byid.get(t)
    twin_ok.append(bool(base and tw and base['BUILD_UNDER_TEST'] == 'PRODUCT_BUILD' and tw['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD'
                        and base['REPETITION'] == tw['REPETITION']))
qe = [s['SCENARIO_ID'] for s in SC if (s['SCENARIO_ID'].startswith('Q-') or s['SCENARIO_ID'].startswith('E6-C')
                                        or s['SCENARIO_ID'] + '-CB' in TWINS_AR505)
      and not s['SCENARIO_ID'].endswith('-CB') and s['SCENARIO_ID'] not in I14_FAMILY]
without_twin = [x for x in qe if x + '-CB' not in byid]
say('A.9 per-build qualification (AR4-06 (3); AR5-05 by content): %d twins <id>-CB on the CHARACTERIZATION_BUILD (%d of them by content:'
    % (len(TWINS), len(TWINS_AR505)))
say('    %s); each with its PRODUCT_BUILD scenario' % ', '.join(TWINS_AR505))
say('    and the same REPETITION label: %s; qualification scenarios outside the I-14 family without a twin: %s'
    % ('yes' if all(twin_ok) else 'NO', ', '.join(without_twin) or 'none'))
say('    twins per rule: %s' % ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(derived_rule(byid[t]) for t in TWINS).items())))
say('    the I-14 family (CHARACTERIZATION_BUILD only, no twin): %s; builds: %s'
    % (', '.join(I14_FAMILY), ', '.join(sorted({byid[x]['BUILD_UNDER_TEST'] for x in I14_FAMILY}))))
say()

# ---------------------------------------------------------------- B. the run plan
say('B. RUN PLAN IMPLIED BY THE VALUES OF BLOCK P (single choice; BA-10 V8 section 2.3)')
say('   N_A = %d, N_B = %d, N_det = %d, n = %d, WU_DRY_RUNS = %d, EVIDENCE_REPETITION = %d, m = %d, K = %d (read from the draft catalog);'
    % (V['N_A'], V['N_B'], V['N_det'], V['n'], STATED_INT['WU_DRY_RUNS'], V['E'], M, K))
say('   ceil(n / K) = %d. CHAR_RUNS = N_A (pin- or selection-feeding), N_A + V(s) (HDM-CHAR-*: N_A runs at the pinned key configuration' % math.ceil(V['n'] / K))
say('   plus exactly one informative run per varied key), N_det + 1 (composition check), EVIDENCE_REPETITION (exploratory)')
say('   G = %d   (governing planned runs = repetition slots)' % G)
say('   D = %d   (non-governing runs: dry runs, CHAR, any non-governing learning run; no slot)' % D)
say('   TOTAL = G + D = %d' % T)
say('   %d SYNTH runs are offline evaluations (BUILD_UNDER_TEST = NONE; no AutoCAD process).' % rule_tot['SYNTH'])
say('   worst-case governing attempts = m * G = %d * %d = %d   (PARAM-04, per repetition slot; every slot uses m attempts)' % (M, G, M * G))
say('   TOTAL if every governing slot used m attempts = m * G + D = %d' % (M * G + D))
say('B.1 terms per rule')
say('   %-16s %9s %6s %9s' % ('rule', 'scenarios', 'gov.', 'runs'))
for r in RULES:
    say('   %-16s %9d %6s %9d' % (r, rule_tot[r], 'G' if r in ('DEFAULT', 'CLEAN', 'VALIDATE', 'EVIDENCE', 'EVIDENCE_STRICT', 'SYNTH') else 'mixed' if r == 'LEARN' else 'D', per[r]))
say('   (LEARN: governing micro-scenarios count in G, non-governing ones in D; DRY and CHAR are in D)')
say('B.2 CHAR term per kind')
for kind in CHAR_KINDS:
    sel = [(s, r, comp) for s, r, comp in char_rows if char_kind(s) == kind]
    say('   %-16s %9d scenarios %9d runs' % (kind, len(sel), rsum(sel)))
say('   (HDM: %d scenarios * N_A plus %d informative runs, exactly one per varied key; COMPOSITION: N_det + 1)'
    % (char_cnt['HDM'], sum(V_OF.values())))
say('B.3 runs per build under test (AR4-06): G / D')
for bl in BUILDS:
    say('   %-24s %6d / %-6d   worst-case governing attempts m * G = %d' % (bl, pb[(bl, 'G')], pb[(bl, 'D')], M * pb[(bl, 'G')]))
twin_runs = sum(runs(byid[t], derived_rule(byid[t]), composition(byid[t]), V) for t in TWINS)
say('   of which the %d qualification twins (all on the CHARACTERIZATION_BUILD): %d runs' % (len(TWINS), twin_runs))
dry_builds = Counter(s['BUILD_UNDER_TEST'] for s in SC if s['MODE'] == 'DRY_RUN')
say('   dry-run scenarios per build: %s' % ', '.join('%s %d' % (b, x) for b, x in sorted(dry_builds.items())))
say('B.4 where each count enters (planned runs per scenario by rule and class composition; this is where the rules of BA-10 V8')
say('    section 2.2 apply N_A = %d, N_B = %d and N_det = %d; "-" = no scenario)' % (V['N_A'], V['N_B'], V['N_det']))
say('    %-16s %7s %7s %7s %7s' % ('rule', 'A_ONLY', 'MIXED', 'B_ONLY', 'NONE'))
for r in [x for x in RULES if x != 'CHAR']:
    cells = []
    for comp in COMPS:
        vals = sorted({runs(s, rr, c_, V) for s, rr, c_ in rows if rr == r and c_ == comp})
        cells.append('-' if cnt[(r, comp)] == 0 else '/'.join(str(x) for x in vals))
    say('    %-16s %7s %7s %7s %7s' % (r, cells[0], cells[1], cells[2], cells[3]))
say('    (the CHAR rule is in B.2: N_A for pin- or selection-feeding, N_A + V(s) for HDM-CHAR-*, N_det + 1 for the composition check,')
say('    EVIDENCE_REPETITION for the exploratory runs; the learning micro-scenarios and the evidence scenarios serve no class-A or class-B')
say('    control group, so they sit in the NONE column)')
say('B.5 PARAM-03 sample: K = %d members, n = %d, ceil(n / K) = %d; each member runs max(N_strict(s), ceil(n / K))' % (K, V['n'], math.ceil(V['n'] / K)))
say('    %-26s %-24s %-8s %-9s %6s' % ('member', 'BUILD_UNDER_TEST', 'class', 'N_strict', 'runs'))
sample_runs = 0
for sid in SAMPLE:
    s = byid[sid]
    comp = composition(s)
    x = runs(s, 'CLEAN', comp, V)
    sample_runs += x
    say('    %-26s %-24s %-8s %-9d %6d' % (sid, s['BUILD_UNDER_TEST'], comp, n_strict(comp, V['N_A'], V['N_B']), x))
say('    the sample holds %d runs >= n = %d: %s; on the PRODUCT_BUILD members %d runs, on the CHARACTERIZATION_BUILD member %d runs'
    % (sample_runs, V['n'], 'yes' if sample_runs >= V['n'] else 'NO',
       sum(runs(byid[x], 'CLEAN', composition(byid[x]), V) for x in SAMPLE if byid[x]['BUILD_UNDER_TEST'] == 'PRODUCT_BUILD'),
       sum(runs(byid[x], 'CLEAN', composition(byid[x]), V) for x in SAMPLE if byid[x]['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD')))
say('    every member is at its PARAM-01 floor (ceil(n / K) is below N_strict of each): %s'
    % ('yes' if all(math.ceil(V['n'] / K) <= n_strict(composition(byid[x]), V['N_A'], V['N_B']) for x in SAMPLE) else 'NO'))
say()

# ---------------------------------------------------------------- C. reconciliation
cat6 = json.load(io.open(CAT6, encoding='utf-8'))
SC6 = cat6['SCENARIOS']
by6 = {s['SCENARIO_ID']: s for s in SC6}
K6 = len(cat6['PARAM_03_SAMPLE'])
V6_OF = {}
for s in SC6:
    if s['GROUP'] == 'HDM':
        ks = varied_keys(s)
        if len(ks) != len(set(ks)) or not set(ks) <= set(HDM_KEYS_V6):
            fail('V6 %s: varied keys not exactly once or outside the BA-08 V6 HDM-2 list' % s['SCENARIO_ID'])
        V6_OF[s['SCENARIO_ID']] = len(ks)


def runs_from_label(s, v, k):
    """Planned runs of a V6 scenario from its REPETITION label (the V6 memo agreed with every V6 label, V6 block A.5)."""
    label = s['REPETITION']
    NA, NB, n, Nd, E = v['N_A'], v['N_B'], v['n'], v['N_det'], v['E']
    ns = {'N_A': NA, 'N_B': NB, 'max(N_A, N_B)': max(NA, NB)}
    rule, _, rest = label.partition(': ')
    if rule == 'DEFAULT':
        return ns[rest]
    if rule == 'CLEAN':
        return max(ns[rest[len('max('):-len(', ceil(n / K))')]], math.ceil(n / k))
    if rule == 'VALIDATE':
        return max(ns[rest[len('max('):-len(', PARAM-02 validation count)')]], Nd)
    if rule == 'EVIDENCE':
        return E
    if rule == 'EVIDENCE_STRICT':
        return max(ns[rest[len('max('):-len(', EVIDENCE_REPETITION)')]], E)
    if rule == 'LEARN':
        return Nd + 1
    if rule == 'SYNTH':
        return 1
    if rule == 'DRY':
        return NB
    if rule == 'CHAR':
        if rest == CHAR_LABEL['PIN_FEEDING']:
            return NA
        if rest == CHAR_LABEL['HDM']:
            return NA + V6_OF[s['SCENARIO_ID']]
        if rest == CHAR_LABEL['COMPOSITION']:
            return Nd + 1
        if rest == CHAR_LABEL['EXPLORATORY']:
            return E
    fail('V6 label not understood: %r' % label)


def evaluate6(v):
    per6 = Counter()
    g = d = 0
    for s in SC6:
        x = runs_from_label(s, v, K6)
        per6[s['REPETITION'].split(':')[0]] += x
        if s['RUN_GOVERNANCE'] == 'GOVERNING':
            g += x
        else:
            d += x
    return per6, g, d, g + d


# the V7 memo's four illustrative rows (one pair for both classes), re-evaluated by this engine: the engine is the V7 engine
memo7 = io.open(MEMO7, encoding='utf-8').read()
memo6 = io.open(MEMO6, encoding='utf-8').read()
V7_ROWS = [('0.10', '0.05', '0.05', '0.95'), ('0.20', '0.05', '0.10', '0.95'), ('0.30', '0.10', '0.10', '0.90'), ('0.50', '0.10', '0.20', '0.90')]
v7_reproduced = True
v6_reproduced = True
v7_row_values = []
for p_, a_, u_, c_ in V7_ROWS:
    nn = n_zero_failure(p_, a_)
    vv = OrderedDict([('N_A', nn), ('N_B', nn), ('n', n_sample(u_, c_)), ('N_det', nn), ('E', 1), ('K', K)])
    per_, g_, d_, t_, pb_ = evaluate(vv)
    line = '   %-5.2f %-6.2f %-5.2f %-5.2f %4d %4d %6d %9d %7d %7d %7d' % (float(p_), float(a_), float(u_), float(c_), vv['N_A'], vv['n'],
                                                                       vv['N_det'], math.ceil(vv['n'] / K), g_, d_, t_)
    v7_reproduced = v7_reproduced and (line in memo7)
    per6_, g6_, d6_, t6_ = evaluate6(vv)
    v6_reproduced = v6_reproduced and (line in memo6) and t6_ == t_
    v7_row_values.append((vv, t_))
if not v7_reproduced:
    fail('the V7 memo block B rows are not reproduced by this engine')

say('C. RECONCILIATION WITH THE V7 AND V6 MEMOS (structure kept: the same catalog, labels, engine and checks)')
say('   V7 memo file: %s ; SHA-256 (LF-normalized): %s' % (rel(MEMO7), lf_sha256(MEMO7)))
say('   V6 memo file: %s ; SHA-256 (LF-normalized): %s' % (rel(MEMO6), lf_sha256(MEMO6)))
say('   V6 catalog SHA-256 (LF-normalized): %s ; scenarios: %d ; K = %d ; HDM-CHAR informative runs: %d in all'
    % (lf_sha256(CAT6), len(SC6), K6, sum(V6_OF.values())))
say('C.1 engine regression: the four block B rows of the V7 memo (one pair for both classes) re-evaluated by this engine over the V7 catalog')
say('    are reproduced exactly (G, D and TOTAL): %s; row1 of the V7 memo (N = N_det = 29, n = 59): TOTAL %d' % ('yes', v7_row_values[0][1]))
say('    the same rows over the V6 catalog through its own REPETITION labels agree with this engine: %s' % ('yes' if v6_reproduced else 'NO'))
per_r1, G_r1, D_r1, T_r1, pb_r1 = evaluate(v7_row_values[0][0])
say('C.2 plan minus the V7 memo row1 (the nearest V7 row: N_A = N_B = N_det = 29, n = 59, E = 1), per rule')
say('    %-16s %9s %9s %9s' % ('rule', 'V7 row1', 'plan', 'delta'))
for r in RULES:
    say('    %-16s %9d %9d %+9d' % (r, per_r1[r], per[r], per[r] - per_r1[r]))
say('    %-16s %9d %9d %+9d   (G %+d, D %+d)' % ('TOTAL', T_r1, T, T - T_r1, G - G_r1, D - D_r1))
per6, G6, D6, T6 = evaluate6(V)
say('C.3 the V6 catalog at the plan values through its own labels vs the V7 catalog')
say('    %-8s %9s %9s %9s %9s %9s' % ('', 'V6', 'V7', 'delta', 'G', 'D'))
say('    %-8s %9d %9d %+9d %+9d %+9d' % ('plan', T6, T, T - T6, G - G6, D - D6))
added = sorted(set(byid) - set(by6))
removed = sorted(set(by6) - set(byid))
common = sorted(set(byid) & set(by6))
changed_label = [x for x in common if byid[x]['REPETITION'] != by6[x]['REPETITION']]
by_cause = []
for ids_ in (added, changed_label, removed):
    d = 0
    for x in ids_:
        r7 = runs(byid[x], derived_rule(byid[x]), composition(byid[x]), V) if x in byid else 0
        r6 = runs_from_label(by6[x], V, K6) if x in by6 else 0
        d += r7 - r6
    by_cause.append(d)
unchanged = sum(runs(byid[x], derived_rule(byid[x]), composition(byid[x]), V) - runs_from_label(by6[x], V, K6)
                for x in common if byid[x]['REPETITION'] == by6[x]['REPETITION'])
if unchanged:
    fail('a scenario with an unchanged label changed its runs')
say('    by cause (each scenario charged once): added %+d, changed label %+d, removed %+d' % tuple(by_cause))
say('C.4 scenario sets, V6 -> V7')
say('    scenarios %d -> %d; added %d: %s' % (len(SC6), len(SC), len(added), ', '.join(added) or 'none'))
say('    removed: %s' % (', '.join(removed) or 'none'))
for x in changed_label:
    say('    changed label: %s: %s -> %s' % (x, by6[x]['REPETITION'], byid[x]['REPETITION']))
gov_changed = [x for x in common if byid[x]['RUN_GOVERNANCE'] != by6[x]['RUN_GOVERNANCE']]
say('    changed governance: %s' % (', '.join('%s (%s -> %s)' % (x, by6[x]['RUN_GOVERNANCE'], byid[x]['RUN_GOVERNANCE']) for x in gov_changed) or 'none'))
build_changed = [x for x in common if byid[x]['BUILD_UNDER_TEST'] != by6[x]['BUILD_UNDER_TEST']]
say('    changed build under test: %d%s' % (len(build_changed), (': ' + ', '.join(build_changed)) if build_changed else ''))
blk_changed = [x for x in common if byid[x]['BLOCKED_BY'] != by6[x]['BLOCKED_BY']]
say('    changed BLOCKED_BY: %s' % (', '.join('%s (%s -> %s)' % (x, '+'.join(by6[x]['BLOCKED_BY']), '+'.join(byid[x]['BLOCKED_BY'])) for x in blk_changed) or 'none'))
pre_changed = [x for x in common if byid[x]['RUN_PREREQUISITES'] != by6[x]['RUN_PREREQUISITES']]
pre_append_only = all(byid[x]['RUN_PREREQUISITES'] == by6[x]['RUN_PREREQUISITES'] + ['BOUND_SHA_PRECONDITION_RECORD'] for x in pre_changed)
say('    changed RUN_PREREQUISITES: %d (AR7-04); in each, BOUND_SHA_PRECONDITION_RECORD appended and nothing else: %s'
    % (len(pre_changed), 'yes' if pre_append_only else 'NO'))
say()

# ---------------------------------------------------------------- D. sensitivities on the actual values
say('D. SENSITIVITIES (finite differences at the plan values; each parameter alone, everything else fixed)')
say('D.1 N_A and N_B moved by -1 and by +1 (N_B < N_A: every MIXED term follows N_A; MIXED would follow N_B only if N_B exceeded N_A)')
for key in ('N_A', 'N_B'):
    lo = dict(V, **{key: V[key] - 1})
    hi = dict(V, **{key: V[key] + 1})
    say('    %s %d -> %d: %+d runs ; %s %d -> %d: %+d runs' % (key, V[key], lo[key], total(lo) - T, key, V[key], hi[key], total(hi) - T))
nb_cross = dict(V, N_B=V['N_A'] + 1)
say('    N_B %d -> %d (one above N_A; MIXED then follows N_B): %+d runs' % (V['N_B'], nb_cross['N_B'], total(nb_cross) - T))
say('D.2 EVIDENCE_REPETITION 1 -> 2: %+d runs' % (total(dict(V, E=V['E'] + 1)) - T))
say('D.3 K + 1: one more PARAM-03 sample member (a new governing CL-CLEAN-<fixture> scenario, mixed class) with its dry-run scenario')
say('    %+d runs (the member itself %d, its dry runs %d)' % (
    total(dict(V, K=K + 1), extra_sample=1) - T, max(V['N_A'], V['N_B'], math.ceil(V['n'] / (K + 1))), V['N_B']))
say('D.4 u and c: they enter only through ceil(n / K); TOTAL changes only when ceil(n / K) > N_strict of a sample member.')
min_ns = min(n_strict(composition(byid[x]), V['N_A'], V['N_B']) for x in SAMPLE)
ustar = 1 - (1 - float(Fraction(PAIRS['c']))) ** (1.0 / (K * min_ns))
uh = n_sample(Fraction(PAIRS['u']) / 2, PAIRS['c'])
ch = n_sample(PAIRS['u'], 1 - (1 - Fraction(PAIRS['c'])) / 2)
say('    u* = 1 - (1 - c)^(1 / (K * N_strict)) = %.4f (N_strict = %d, the smallest of the members) is the value below which that happens'
    % (ustar, min_ns))
say('    u halved (%s): n %d -> %d, %+d runs ; 1 - c halved (c = %s): n %d -> %d, %+d runs' % (
    '%.3f' % (float(Fraction(PAIRS['u'])) / 2), V['n'], uh, total(dict(V, n=uh)) - T,
    '%.3f' % (1 - (1 - float(Fraction(PAIRS['c']))) / 2), V['n'], ch, total(dict(V, n=ch)) - T))
n01 = n_sample('0.005', PAIRS['c'])
say('    example below u* (u = 0.005 with c = %s): n = %d, ceil(n / K) = %d > N_strict = %d: %+d runs, all in the CLEAN term' % (
    PAIRS['c'], n01, math.ceil(n01 / K), min_ns, total(dict(V, n=n01)) - T))
say('D.5 each Owner pair alone: alpha halved and p halved (the count follows the formula; the other pairs fixed)')
for key, a, b in (('N_A', 'p_A', 'alpha_A'), ('N_B', 'p_B', 'alpha_B'), ('N_det', 'p_det', 'alpha_det')):
    pa, pq = Fraction(PAIRS[a]), Fraction(PAIRS[b])
    n_a = n_zero_failure(pa, pq / 2)
    n_p = n_zero_failure(pa / 2, pq)
    ta = total(dict(V, **{key: n_a}))
    tp = total(dict(V, **{key: n_p}))
    say('    %-5s alpha %s -> %s: N %d -> %d, TOTAL %d -> %d (%+.1f%%) ; p %s -> %s: N %d -> %d, TOTAL %d -> %d (%+.1f%%)' % (
        key, PAIRS[b], '%g' % (float(pq) / 2), V[key], n_a, T, ta, 100.0 * (ta - T) / T,
        PAIRS[a], '%g' % (float(pa) / 2), V[key], n_p, T, tp, 100.0 * (tp - T) / T))
say('D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts')
say('    m = %d: worst-case governing attempts = %d * %d = %d ; each unit of m adds up to %d attempts' % (M, M, G, M * G, G))
say('    the %d non-governing runs have no slot; m = 1 would mean no retry (%d governing attempts)' % (D, G))
say('D.7 HDM-CHAR-* informative runs: exactly one per varied key (AR4-14 (b)(ii)): %d in all; no Owner answer moves them and no other'
    % sum(V_OF.values()))
say('    authority adds runs')
hi_det = dict(V, N_det=V['N_det'] + 1)
per_hi = evaluate(hi_det)[0]
comp_hi = sum(runs(s, r, comp, hi_det) - runs(s, r, comp, V) for s, r, comp in char_rows if char_kind(s) == 'COMPOSITION')
say('D.8 N_det moved by +1 (Q-O3 alone): N_det %d -> %d: %+d runs (LEARN %+d, composition check %+d, VALIDATE %+d: max(N_strict, N_det)'
    % (V['N_det'], V['N_det'] + 1, total(hi_det) - T, per_hi['LEARN'] - per['LEARN'], comp_hi, per_hi['VALIDATE'] - per['VALIDATE']))
say('    follows N_det once it exceeds N_strict = %d; at N_det = %d it does)' % (V['N_A'], V['N_det']))
say()

# ---------------------------------------------------------------- E. rulings
say('E. RULINGS OF DECISIONS SECTIONS 217, 220 AND 226 APPLIED BY BA-10 V8 AND BA-04 V7 (checks; no other reading is sized)')
cag_rows = [(s, r, comp) for s, r, comp in rows if s['CONTRACT_ASSIGNED_GROUPS']]
say('E.1 AR4-14 (a): CONTRACT_ASSIGNED_GROUPS never enter N_strict. Scenarios that carry them: %d; each label resolves N_strict from'
    % len(cag_rows))
say('    GROUPS_SERVED only (A.5): %s' % ('yes' if not [x for x in label_mismatch if x[0] in {s['SCENARIO_ID'] for s, r, comp in cag_rows}] else 'NO'))
for s, r, comp in cag_rows:
    say('    %-26s rule %-16s class composition of GROUPS_SERVED %-7s contract-assigned %s'
        % (s['SCENARIO_ID'], r, comp, ', '.join(x['group'] for x in s['CONTRACT_ASSIGNED_GROUPS'])))
say('E.2 AR4-14 (b)(i): CHAR_RUNS = the derived N_c of the class that consumes the pin or the selection (today N_A); no Owner pair of')
say('    its own; the Q-O1 pair (p_A, alpha_A) sizes %d pin- or selection-feeding scenarios and the pinned-configuration runs of the %d'
    % (char_cnt['PIN_FEEDING'], char_cnt['HDM']))
say('    HDM-CHAR-* scenarios: %d runs' % ((char_cnt['PIN_FEEDING'] + char_cnt['HDM']) * V['N_A']))
say('E.3 AR4-14 (b)(ii) and AR4-07: %d HDM-CHAR-* scenarios, CHAR_RUNS(s) = N_A + V(s) with V(s) = %s; informative runs %d in all'
    % (len(HDM_SC), ', '.join('%d' % V_OF[s['SCENARIO_ID']] for s in HDM_SC), sum(V_OF.values())))
say('E.4 AR4-01: %d governing learning micro-scenarios (rule LEARN, N_det + 1 each, on the %s); EV-L-COMPOSED non-governing'
    % (sum(1 for s, r, comp in rows if r == 'LEARN' and s['RUN_GOVERNANCE'] == 'GOVERNING'),
       ', '.join(sorted({s['BUILD_UNDER_TEST'] for s, r, comp in rows if r == 'LEARN'}))))
say('    (CHAR, N_det + 1); the Q-I14 family on the CHARACTERIZATION_BUILD with %d dry-run scenarios'
    % sum(1 for s in SC if s['MODE'] == 'DRY_RUN' and s['SCENARIO_ID'][len('WU-DRY-'):] in I14_FAMILY))
say()

# ---------------------------------------------------------------- F. draft scenarios by blocker, status and prerequisite
say('F. RUNS OF THE DRAFT SCENARIOS (counted in the totals of block B as the draft catalog states them; a scenario with two')
say('   blockers is counted under each; the memo does not model any ruling)')
BLOCKERS = list(cat['BLOCKED_BY_VOCABULARY'].keys())
say('F.1 by blocker')
say('   %-26s %9s %9s %9s' % ('BLOCKED_BY', 'scenarios', 'governing', 'runs'))
for bl in BLOCKERS:
    sel = [(s, r, comp) for s, r, comp in rows if bl in s['BLOCKED_BY']]
    say('   %-26s %9d %9d %9d' % (bl, len(sel), sum(1 for s, r, comp in sel if s['RUN_GOVERNANCE'] == 'GOVERNING'), rsum(sel)))
say('   scenarios per rule under each blocker:')
for bl in BLOCKERS:
    sel = [(s, r, comp) for s, r, comp in rows if bl in s['BLOCKED_BY']]
    say('    %s: %s' % (bl, ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(r for s, r, comp in sel).items()))))
say('F.2 by status')
say('   %-30s %9s %9s' % ('STATUS', 'scenarios', 'runs'))
for st in sorted({s['STATUS'] for s in SC}):
    sel = [(s, r, comp) for s, r, comp in rows if s['STATUS'] == st]
    say('   %-30s %9d %9d' % (st, len(sel), rsum(sel)))
say('F.3 by run prerequisite (a scenario with two prerequisites is counted under each)')
say('   %-38s %9s %9s' % ('RUN_PREREQUISITES', 'scenarios', 'runs'))
for pr in sorted({x for s in SC for x in s['RUN_PREREQUISITES']}):
    sel = [(s, r, comp) for s, r, comp in rows if pr in s['RUN_PREREQUISITES']]
    say('   %-38s %9d %9d' % (pr, len(sel), rsum(sel)))
say('F.4 AR5-05 (the ruling of AQ-V5-02): the twins by content on the CHARACTERIZATION_BUILD, each with the rule of its source')
for x in TWINS_AR505:
    s = byid[x]
    src = byid[x[:-3]]
    dry = runs(byid['WU-DRY-' + x], 'DRY', 'NONE', V) if ('WU-DRY-' + x) in byid else 0
    say('    %-26s rule %-9s (source %s), REACH %s; dry-run scenario: %s; the twin with its dry runs: %+d' % (
        x, derived_rule(s), derived_rule(src), s['REACH'], 'yes' if ('WU-DRY-' + x) in byid else 'no',
        runs(s, derived_rule(s), composition(s), V) + dry))
say('    all seven with their dry runs: %+d runs' % sum(runs(byid[x], derived_rule(byid[x]), composition(byid[x]), V)
                                                      + (runs(byid['WU-DRY-' + x], 'DRY', 'NONE', V) if ('WU-DRY-' + x) in byid else 0)
                                                      for x in TWINS_AR505))
evm_cb = [(s, r, comp) for s, r, comp in rows if s['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD'
          and s['RUN_GOVERNANCE'] == 'GOVERNING' and 'EVM_FROZEN' in s['PIN_SLOTS']]
say('F.5 AR5-04 (the ruling of AQ-V5-01): governing CHARACTERIZATION_BUILD scenarios whose PIN_SLOTS name EVM_FROZEN (learned and')
say('    validated on the PRODUCT_BUILD, AR4-01): %d; per rule: %s'
    % (len(evm_cb), ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(r for s, r, comp in evm_cb).items()))))
say('    of them naming BUILD_EQUIVALENCE_RECORD in RUN_PREREQUISITES: %d; their runs: %d'
    % (sum(1 for s, r, comp in evm_cb if 'BUILD_EQUIVALENCE_RECORD' in s['RUN_PREREQUISITES']), rsum(evm_cb)))
say('    the ruling adds a run prerequisite and moves no run count')
bound = [(s, r, comp) for s, r, comp in rows if 'BOUND_SHA_PRECONDITION_RECORD' in s['RUN_PREREQUISITES']]
on_build = [(s, r, comp) for s, r, comp in rows if s['BUILD_UNDER_TEST'] in ('PRODUCT_BUILD', 'CHARACTERIZATION_BUILD')]
say('F.6 AR5-12 as ruled by AR7-04 (BOUND_SHA_PRECONDITION_RECORD, BA-04 V7): scenarios that name it: %d; the scenarios on a build under test,'
    % len(bound))
say('    governing or not: %d; the same set: %s; their runs: %d'
    % (len(on_build), 'yes' if [s['SCENARIO_ID'] for s, r, c in bound] == [s['SCENARIO_ID'] for s, r, c in on_build] else 'NO', rsum(bound)))
say('    per governance: %s' % ', '.join('%s %d' % kv for kv in sorted(Counter(s['RUN_GOVERNANCE'] for s, r, c in bound).items())))
nongov_on_build = [(s, r, comp) for s, r, comp in on_build if s['RUN_GOVERNANCE'] != 'GOVERNING']
say('    the non-governing ones (added by AR7-04): %d; their runs: %d' % (len(nongov_on_build), rsum(nongov_on_build)))
for p_ in MANDATORY:
    sel = [(s, r, comp) for s, r, comp in nongov_on_build if s['SCENARIO_ID'].startswith(p_)]
    say('    mandatory %-14s %4d scenarios; runs: %d' % (p_ + '*', len(sel), rsum(sel)))
rest = [(s, r, comp) for s, r, comp in nongov_on_build if not s['SCENARIO_ID'].startswith(MANDATORY)]
say('    under the single rule: %s; runs: %d' % (', '.join(s['SCENARIO_ID'] for s, r, c in rest), rsum(rest)))
say('    synthetic scenarios (BUILD_UNDER_TEST NONE) naming it: %d' % sum(1 for s, r, c in bound if s['BUILD_UNDER_TEST'] == 'NONE'))
say('    the ruling adds a run prerequisite only and moves no run count (block C)')
say()
say('END OF OUTPUT')

text = '\n'.join(OUT) + '\n'
sys.stdout.buffer.write(text.encode('utf-8'))
