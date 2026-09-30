"""I-52 CT-21D cost memo V5: arithmetic over the draft V5 scenario catalog (NOT SEALED; ILLUSTRATIVE ONLY).

Reads docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json and evaluates the symbolic run expression of
BA-10 V5 section 2.3 for the illustrative choices used since BA-10 V3 section 2.3. It reconciles the totals with the V4 memo
(the V4 catalog evaluated through its own REPETITION labels, checked against the V4 memo figures), splits the runs by build
under test (AR4-06: the qualification twins and the characterization build are in the counts) and prints sensitivities and
the runs of the draft scenarios by blocker.

HDM-CHAR-* (AR4-14 (b)(ii)): CHAR_RUNS(s) = N_A + V(s): N_A runs at the pinned key configuration plus exactly one
informative run per varied key. V(s) is read per scenario from its declared CONTROL_PLANE_SETUP injections in the catalog
and checked against the closed key list of BA-08 V5 section 11.2 (rows HDM-2 and HDM-4). EV-L-COMPOSED (AR4-01): the
non-governing composition check, CHAR_RUNS = N_det + 1 (BA-10 V5 section 2.2). The script checks that BA-10 V5 states these
rules and the LEARN rule of R1/R2:BA10-V4-01.

Nothing printed here is a proposal, a choice or a decision. Every parameter of BA-10 V5 stays UNSET. The rulings of decisions
section 217 are applied as BA-10 V5 and BA-04 V5 state them; block E checks them and sizes no other reading. The composition-check
count is BA-10 V5's reading, open as AQ-V5-06 (block A.7 checks that BA-10 V5 records it). Blocks F.4 and F.5 count what the
open round V5 questions AQ-V5-02 and AQ-V5-01 touch, without modeling any ruling.

Usage: python I-52-ct21d-cost-memo-v5.py [<repo root>]
The output is plain text (deterministic). File hashes are SHA-256 over the LF-normalized bytes, lowercase hex.
"""
import hashlib
import io
import json
import math
import os
import re
import sys
from collections import Counter, OrderedDict

ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..'))
INIT = os.path.join(ROOT, 'docs', 'initiatives')
EVID = os.path.join(ROOT, 'docs', 'automation', 'evidence')
CAT5 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json')
CAT4 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json')
BA08 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-08-selective-kind-baseline-v5.md')
BA08_V4 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md')
BA10 = os.path.join(INIT, 'I-52-ct21d-baseline-ba-10-parameter-sheet-v5.md')
MEMO4 = os.path.join(EVID, 'I-52-ct21d-cost-memo-v4.md')

OUT = []


def say(line=''):
    OUT.append(line)


def rel(path):
    return os.path.relpath(path, ROOT).replace(os.sep, '/')


def lf_bytes(path):
    return io.open(path, 'rb').read().replace(b'\r\n', b'\n')


def lf_sha256(path):
    return hashlib.sha256(lf_bytes(path)).hexdigest()


def n_zero_failure(p, alpha):
    """Smallest integer N with (1 - p)^N <= alpha (the formula N >= ln(alpha) / ln(1 - p), rounded up)."""
    n = max(1, math.ceil(math.log(alpha) / math.log(1 - p)) - 1)
    while (1 - p) ** n > alpha:
        n += 1
    while n > 1 and (1 - p) ** (n - 1) <= alpha:
        n -= 1
    return n


def n_sample(u, c):
    """Smallest integer n with 1 - (1 - c)^(1/n) <= u, i.e. (1 - u)^n <= 1 - c."""
    return n_zero_failure(u, 1 - c)


# ---------------------------------------------------------------- BA-08 section 11.2: the closed key list of HOST_DEFAULT_MAP
HDM_VARIABLES = ('CLAYER', 'CECOLOR', 'CELTYPE', 'CELTSCALE', 'CELWEIGHT', 'CETRANSPARENCY', 'CPLOTSTYLE')
HDM_STYLES_PHRASE = 'and the current text style and dimension style'
HDM_STYLES = ('current text style', 'current dimension style')
HDM4_PHRASE_V5 = 'plus **exactly one** informative run per varied key'
HDM4_PHRASE_V4 = 'once or more with each key variable changed'


def hdm_keys(path, hdm4_phrase):
    """The key list of BA-08 section 11.2 HDM-2, read from the file; stops (fail closed) if the rows do not read as expected."""
    lines = io.open(path, encoding='utf-8').read().split('\n')
    hdm2 = [x for x in lines if x.startswith('| HDM-2 ')]
    hdm4 = [x for x in lines if x.startswith('| HDM-4 ')]
    if len(hdm2) != 1 or len(hdm4) != 1:
        raise SystemExit('%s: expected one HDM-2 row and one HDM-4 row, found %d and %d' % (rel(path), len(hdm2), len(hdm4)))
    cell = hdm2[0].split('|')[2]
    head, sep, _ = cell.partition(HDM_STYLES_PHRASE)
    if not sep:
        raise SystemExit('%s HDM-2 does not name the current text style and dimension style as keys' % rel(path))
    variables = tuple(re.findall(r'`([A-Z]+)`', head))
    if variables != HDM_VARIABLES:
        raise SystemExit('%s HDM-2 context variables differ from the list this memo expects: %r' % (rel(path), variables))
    if hdm4_phrase not in hdm4[0]:
        raise SystemExit('%s HDM-4 does not state the expected varied-key rule' % rel(path))
    return variables + HDM_STYLES


HDM_KEYS = hdm_keys(BA08, HDM4_PHRASE_V5)
HDM_KEYS_V4 = hdm_keys(BA08_V4, HDM4_PHRASE_V4)

# ---------------------------------------------------------------- V5 catalog
cat = json.load(io.open(CAT5, encoding='utf-8'))
GT = cat['GROUP_TABLE']
SAMPLE = list(cat['PARAM_03_SAMPLE'])
SC = cat['SCENARIOS']
byid = {s['SCENARIO_ID']: s for s in SC}
TWINS = list(cat['PER_BUILD_QUALIFICATION']['twins'])
I14_FAMILY = list(cat['PER_BUILD_QUALIFICATION']['characterization_build_only'])
BUILDS = ['PRODUCT_BUILD', 'CHARACTERIZATION_BUILD', 'NONE']


def composition(s):
    """Classes of the class-A and class-B CONTROL groups the scenario serves (GROUPS_SERVED; KR and evidence groups excluded;
    CONTRACT_ASSIGNED_GROUPS never enter N_strict: AR4-14 (a), section 217)."""
    cls = sorted({GT[g]['class'] for g in s['GROUPS_SERVED'] if GT[g]['kind'] == 'CONTROL' and GT[g]['class'] in ('A', 'B')})
    return {'A': 'A_ONLY', 'AB': 'MIXED', 'B': 'B_ONLY', '': 'NONE'}[''.join(cls)]


def derived_rule(s):
    """The rule of BA-10 V5 section 2.2 (table of repetition rules, first matching row), derived from the scenario's properties."""
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
    """CHAR_RUNS kind of BA-10 V5 section 2.2: exploratory; HOST_DEFAULT_MAP (label HDM); the event-model composition check
    (the non-governing E12-L scenario that is not a learning micro-scenario, AR4-01); other pin- or selection-feeding runs."""
    if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING':
        return 'EXPLORATORY'
    if s['GROUP'] == 'HDM':
        return 'HDM'
    if s['GROUP'] == 'E12-L':
        return 'COMPOSITION'
    return 'PIN_FEEDING'


# ---------------------------------------------------------------- V(s) of each HDM-CHAR-* scenario, from its declared injections
KEY_OF_VARIANT = OrderedDict([(k, k) for k in HDM_VARIABLES] + [('the current text style', 'current text style'),
                                                                   ('the current dimension style', 'current dimension style')])


def varied_keys(s):
    """The keys an HDM-CHAR-* scenario varies: one CONTROL_PLANE_SETUP injection per key, 'informative run: <key> changed ...'."""
    keys = []
    for x in s['INJECTIONS']:
        m = re.match(r'informative run: (.+?) changed to ', x.get('variant', ''))
        if x.get('writer') != 'CONTROL_PLANE_SETUP' or not m or m.group(1) not in KEY_OF_VARIANT:
            raise SystemExit('%s: injection that is not a declared key change: %r' % (s['SCENARIO_ID'], x))
        keys.append(KEY_OF_VARIANT[m.group(1)])
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
        raise SystemExit('%s: varied keys not exactly once, outside the HDM-2 list, or V(s) not stated: %r' % (s['SCENARIO_ID'], ks))
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
    """The exact REPETITION label (BA-04 V5 notation) of the BA-10 V5 rule, with N_strict and the CHAR_RUNS kind resolved.
    BA-04 V5 has no label for a governing learning micro-scenario that serves a class-A or class-B group (none exists): None."""
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

K = len(SAMPLE)
sample_ok = all(sid in byid and byid[sid]['RUN_GOVERNANCE'] == 'GOVERNING' and derived_rule(byid[sid]) == 'CLEAN' for sid in SAMPLE)
clean_not_in_sample = [s['SCENARIO_ID'] for s in SC if s['SCENARIO_ID'].startswith('CL-CLEAN-') and s['SCENARIO_ID'] not in SAMPLE]
mixed_governing = [s for s, r, comp in rows if comp == 'MIXED' and s['RUN_GOVERNANCE'] == 'GOVERNING']
mixed_without_max = [s['SCENARIO_ID'] for s in mixed_governing if 'max(N_A, N_B)' not in s['REPETITION']]
learn_serving_ab = [s['SCENARIO_ID'] for s, r, comp in rows if r == 'LEARN' and learn_strict(s, comp)]

RULES = ['DEFAULT', 'CLEAN', 'VALIDATE', 'LEARN', 'EVIDENCE', 'EVIDENCE_STRICT', 'SYNTH', 'DRY', 'CHAR']
CHAR_KINDS = ['PIN_FEEDING', 'HDM', 'COMPOSITION', 'EXPLORATORY']
cnt = Counter((r, comp) for s, r, comp in rows)
rule_tot = Counter(r for s, r, comp in rows)
char_rows = [(s, r, comp) for s, r, comp in rows if r == 'CHAR']
char_cnt = Counter(char_kind(s) for s, r, comp in char_rows)
composition_ids = [s['SCENARIO_ID'] for s, r, comp in char_rows if char_kind(s) == 'COMPOSITION']
composition_profile_ok = all(byid[x]['EXPECTED_LOG_RECORD_SET'] == 'LP-COMPOSE' for x in composition_ids)

# ---------------------------------------------------------------- BA-10 V5: the rules this memo applies must be stated there
ba10_text = io.open(BA10, encoding='utf-8').read()
BA10_PHRASES = OrderedDict([
    ('HDM-CHAR-* rule (AR4-14 (b)(ii))', '`N_A` runs at the pinned key configuration plus **exactly one** informative run per varied key'),
    ('EV-L-COMPOSED rule (AR4-01)', '`CHAR_RUNS(s) = N_det + 1` for the composition check `EV-L-COMPOSED`'),
    ('LEARN rule (R1/R2:BA10-V4-01)', '`max(N_strict(s), N_det + 1)` when it is governing and serves a class-A or class-B control group'),
    ('N_strict over GROUPS_SERVED only (AR4-14 (a))', '`CONTRACT_ASSIGNED_GROUPS` never enter `N_strict` (ruled by AR4-14 (a)'),
])
ba10_states = OrderedDict((k, v in ba10_text) for k, v in BA10_PHRASES.items())
ba10_v4_phrase_gone = 'at least one informative run' not in ba10_text
# the composition-check count is BA-10 V5's own reading: BA-10 V5 section 6.1 must record it as the open question AQ-V5-06
ba10_aq06_rows = [x for x in ba10_text.split('\n') if x.startswith('| **AQ-V5-06** ')]
ba10_aq06_open = (len(ba10_aq06_rows) == 1 and ba10_aq06_rows[0].rstrip().endswith('| OPEN, before the candidate |')
                  and 'N_det + 1' in ba10_aq06_rows[0] and 'EV-L-COMPOSED' in ba10_aq06_rows[0])
ba10_rules = re.findall(r'^\| `([A-Z_]+)` \| ', ba10_text.split('**Repetition rules', 1)[1].split('**`WU_DRY_RUNS`', 1)[0], re.M)


def n_strict(comp, NA, NB):
    return {'A_ONLY': NA, 'MIXED': max(NA, NB), 'B_ONLY': NB, 'NONE': 0}[comp]


def runs(s, r, comp, v):
    """Planned runs R(s) of BA-10 V5 section 2.2 for the parameter values v."""
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


def params(p, alpha, u, c, E=1, k=None):
    NA = n_zero_failure(p, alpha)
    return OrderedDict([('N_A', NA), ('N_B', NA), ('n', n_sample(u, c)), ('N_det', NA), ('E', E), ('K', k if k is not None else K)])


ILLUSTRATIVE = [(0.10, 0.05, 0.05, 0.95), (0.20, 0.05, 0.10, 0.95), (0.30, 0.10, 0.10, 0.90), (0.50, 0.10, 0.20, 0.90)]

say('I-52 CT-21D COST MEMO V5 - ARITHMETIC OUTPUT (ILLUSTRATIVE; NOT A PROPOSAL, NOT A CHOICE, NOT A DECISION)')
say('catalog file    : %s' % rel(CAT5))
say('catalog SHA-256 : %s (LF-normalized bytes)' % lf_sha256(CAT5))
say('catalog version : %s ; scenarios: %d' % (cat['CATALOG_VERSION'], len(SC)))
say('key list file   : %s (section 11.2, rows HDM-2 and HDM-4)' % rel(BA08))
say('key list hash   : %s (SHA-256, LF-normalized bytes)' % lf_sha256(BA08))
say('rule sheet file : %s' % rel(BA10))
say('rule sheet hash : %s (SHA-256, LF-normalized bytes)' % lf_sha256(BA10))
say()
say('A. COUNTS (read from the draft catalog; they change with the catalog)')
say('A.1 rule derived from the scenario properties (BA-10 V5 section 2.2) vs the REPETITION field of the catalog: %s'
    % ('all %d agree' % len(SC) if not mismatch else 'MISMATCH %r' % mismatch))
say('A.2 scenarios per rule and class composition (classes of the class-A/B CONTROL groups of GROUPS_SERVED; KR, evidence')
say('    groups and CONTRACT_ASSIGNED_GROUPS excluded, AR4-14 (a))')
say('    %-16s %6s %7s %6s %7s %6s' % ('rule', 'total', 'A_ONLY', 'MIXED', 'B_ONLY', 'NONE'))
for r in RULES:
    say('    %-16s %6d %7d %6d %7d %6d' % (r, rule_tot[r], cnt[(r, 'A_ONLY')], cnt[(r, 'MIXED')], cnt[(r, 'B_ONLY')], cnt[(r, 'NONE')]))
say('    %-16s %6d' % ('ALL', len(SC)))
say('    LEARN: %d learning micro-scenarios, %d governing; governing ones that serve a class-A or class-B group (R(s) = max(N_strict,'
    % (rule_tot['LEARN'], sum(1 for s, r, comp in rows if r == 'LEARN' and s['RUN_GOVERNANCE'] == 'GOVERNING'), ))
say('    N_det + 1)): %d%s' % (len(learn_serving_ab), (' (%s)' % ', '.join(learn_serving_ab)) if learn_serving_ab else ''))
say('A.3 CHAR split: pin- or selection-feeding %d (E4-LEARN-R1-*, LK-*); HOST_DEFAULT_MAP %d (HDM-CHAR-*); composition check %d (%s);'
    % (char_cnt['PIN_FEEDING'], char_cnt['HDM'], char_cnt['COMPOSITION'], ', '.join(composition_ids)))
say('    exploratory %d (%s); the composition check has the log profile LP-COMPOSE: %s'
    % (char_cnt['EXPLORATORY'], ', '.join(s['SCENARIO_ID'] for s, r, comp in char_rows if char_kind(s) == 'EXPLORATORY'),
       'yes' if composition_profile_ok else 'NO'))
say('A.4 PARAM-03 sample (catalog field PARAM_03_SAMPLE): K = %d; every member GOVERNING with rule CLEAN: %s' % (K, 'yes' if sample_ok else 'NO'))
say('    members: %s' % ', '.join(SAMPLE))
say('    members per build: %s' % ', '.join('%s %d' % (b, x) for b, x in sorted(Counter(byid[m]['BUILD_UNDER_TEST'] for m in SAMPLE).items())))
say('    on the CHARACTERIZATION_BUILD: %s (its PARAM-03 contribution holds for the PRODUCT_BUILD only under the recorded build'
    % ', '.join(m for m in SAMPLE if byid[m]['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD'))
say('    equivalence, AR4-06 (6))')
say('    CL-CLEAN-* ids outside the sample: %s' % (', '.join(clean_not_in_sample) or 'none'))
say('A.5 exact REPETITION label (N_strict resolved to N_A, N_B or max(N_A, N_B); CHAR_RUNS kind resolved) vs the catalog: %s'
    % ('all %d agree' % len(SC) if not label_mismatch else 'MISMATCH %r' % label_mismatch))
say('    governing mixed-class scenarios: %d; of them with a label without max(N_A, N_B): %d'
    % (len(mixed_governing), len(mixed_without_max)))
say('A.6 HOST_DEFAULT_MAP keys (BA-08 V5 section 11.2 HDM-2, closed list): %s = %d keys' % (', '.join(HDM_KEYS), len(HDM_KEYS)))
say('    BA-08 V5 HDM-4: N_A runs at the pinned key configuration plus exactly one informative run per varied key (AR4-14 (b)(ii))')
say('    V(s) read from the declared key-change injections of each scenario (each key once, from the closed list, V(s) stated):')
for sid, vs, missing in hdm_check:
    say('    %-22s V(s) = %d ; keys of the list it does not vary: %d' % (sid, vs, missing))
say('    informative runs in all: %d (outside the intent; no Owner answer moves them)' % sum(V_OF.values()))
say('A.7 BA-10 V5 states the rules this memo applies:')
for k, ok in ba10_states.items():
    say('    %-48s %s' % (k, 'yes' if ok else 'NO'))
say('    the V4 wording "at least one informative run" is absent: %s' % ('yes' if ba10_v4_phrase_gone else 'NO'))
say('    the composition-check count is recorded as the open question AQ-V5-06 (BA-10 V5 section 6.1): %s'
    % ('yes' if ba10_aq06_open else 'NO'))
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
qe = [s['SCENARIO_ID'] for s in SC if (s['SCENARIO_ID'].startswith('Q-') or s['SCENARIO_ID'].startswith('E6-C'))
      and not s['SCENARIO_ID'].endswith('-CB') and s['SCENARIO_ID'] not in I14_FAMILY]
without_twin = [x for x in qe if x + '-CB' not in byid]
say('A.9 per-build qualification (AR4-06 (3)): %d twins <id>-CB on the CHARACTERIZATION_BUILD; each with its PRODUCT_BUILD scenario'
    % len(TWINS))
say('    and the same REPETITION label: %s; Q-*/E6-C* scenarios outside the I-14 family without a twin: %s'
    % ('yes' if all(twin_ok) else 'NO', ', '.join(without_twin) or 'none'))
say('    twins per rule: %s' % ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(derived_rule(byid[t]) for t in TWINS).items())))
say('    the I-14 family (CHARACTERIZATION_BUILD only, no twin): %s; builds: %s'
    % (', '.join(I14_FAMILY), ', '.join(sorted({byid[x]['BUILD_UNDER_TEST'] for x in I14_FAMILY}))))
say()

say('B. ILLUSTRATIVE TOTALS FOR THE CHOICES USED SINCE BA-10 V3 SECTION 2.3')
say('   assumptions (ILLUSTRATIVE): one pair for both classes (p_A = p_B = p, alpha_A = alpha_B = alpha), p_det = p, alpha_det = alpha,')
say('   EVIDENCE_REPETITION = 1 (the implementer\'s draft proposal in BA-10 for the Coordinator value; not a value and not a Coordinator')
say('   position), K = %d; CHAR_RUNS = N_A (pin- or selection-feeding), N_A + V(s) (HDM-CHAR-*: N_A runs at the pinned key' % K)
say('   configuration plus exactly one informative run per varied key), N_det + 1 (composition check), EVIDENCE_REPETITION (exploratory)')
say('   %-5s %-6s %-5s %-5s %4s %4s %6s %9s %7s %7s %7s' % ('p', 'alpha', 'u', 'c', 'N', 'n', 'N_det', 'ceil(n/K)', 'G', 'D', 'TOTAL'))
base = []
for p, a, u, c in ILLUSTRATIVE:
    v = params(p, a, u, c)
    per, G, D, T, pb = evaluate(v)
    base.append((p, a, u, c, v, per, G, D, T, pb))
    say('   %-5.2f %-6.2f %-5.2f %-5.2f %4d %4d %6d %9d %7d %7d %7d' % (p, a, u, c, v['N_A'], v['n'], v['N_det'], math.ceil(v['n'] / K), G, D, T))
say('   G = governing planned runs (repetition slots); D = non-governing runs (dry runs, CHAR, any non-governing learning run); TOTAL = G + D.')
say('   %d SYNTH runs per row are offline evaluations (BUILD_UNDER_TEST = NONE; no AutoCAD process).' % rule_tot['SYNTH'])
say()
say('B.1 terms per rule')
say('   %-16s' % 'rule' + ''.join('%9s' % ('row%d' % (i + 1)) for i in range(len(base))))
for r in RULES:
    say('   %-16s' % r + ''.join('%9d' % b[5][r] for b in base))
say('B.2 CHAR term per kind')
for kind in CHAR_KINDS:
    say('   %-16s' % kind + ''.join('%9d' % sum(runs(s, r, comp, b[4]) for s, r, comp in char_rows if char_kind(s) == kind) for b in base))
say('   (HDM: %d scenarios * N_A plus %d informative runs, exactly one per varied key; COMPOSITION: N_det + 1)'
    % (char_cnt['HDM'], sum(V_OF.values())))
say('B.3 runs per build under test (AR4-06): G / D')
say('   %-24s' % 'BUILD_UNDER_TEST' + ''.join('%14s' % ('row%d' % (i + 1)) for i in range(len(base))))
for bl in BUILDS:
    say('   %-24s' % bl + ''.join('%14s' % ('%d / %d' % (b[9][(bl, 'G')], b[9][(bl, 'D')])) for b in base))
twin_runs = [sum(runs(byid[t], derived_rule(byid[t]), composition(byid[t]), b[4]) for t in TWINS) for b in base]
say('   of which the %d qualification twins (all on the CHARACTERIZATION_BUILD): %s'
    % (len(TWINS), ' ; '.join('row%d %d' % (i + 1, x) for i, x in enumerate(twin_runs))))
dry_builds = Counter(s['BUILD_UNDER_TEST'] for s in SC if s['MODE'] == 'DRY_RUN')
say('   dry-run scenarios per build: %s' % ', '.join('%s %d' % (b, x) for b, x in sorted(dry_builds.items())))
say()

# ---------------------------------------------------------------- C. reconciliation with the V4 memo
cat4 = json.load(io.open(CAT4, encoding='utf-8'))
SC4 = cat4['SCENARIOS']
by4 = {s['SCENARIO_ID']: s for s in SC4}
K4 = len(cat4['PARAM_03_SAMPLE'])
V4_HDM = len(HDM_KEYS_V4)


def runs_from_label(label, v, k):
    """Planned runs of a V4 scenario from its REPETITION label (the V4 memo agreed with every V4 label, V4 block A.5)."""
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
        if rest == 'CHAR_RUNS = N_A':
            return NA
        if rest.startswith('CHAR_RUNS = N_A at the pinned key values, plus at least one informative run'):
            return NA + V4_HDM
        if rest == 'CHAR_RUNS = EVIDENCE_REPETITION (exploratory)':
            return E
    raise SystemExit('V4 label not understood: %r' % label)


def evaluate4(v):
    per = Counter()
    G = D = 0
    for s in SC4:
        x = runs_from_label(s['REPETITION'], v, K4)
        per[s['REPETITION'].split(':')[0]] += x
        if s['RUN_GOVERNANCE'] == 'GOVERNING':
            G += x
        else:
            D += x
    return per, G, D, G + D


memo4 = io.open(MEMO4, encoding='utf-8').read()
v4rows = []
v4_reproduced = True
for p, a, u, c, v, per, G, D, T, pb in base:
    per4, G4, D4, T4 = evaluate4(v)
    v4rows.append((per4, G4, D4, T4))
    line = '   %-5.2f %-6.2f %-5.2f %-5.2f %4d %4d %6d %9d %7d %7d %7d' % (p, a, u, c, v['N_A'], v['n'], v['N_det'], math.ceil(v['n'] / K4), G4, D4, T4)
    v4_reproduced = v4_reproduced and (line in memo4)
say('C. RECONCILIATION WITH THE V4 MEMO (the V4 catalog evaluated through its own REPETITION labels)')
say('   V4 catalog SHA-256 (LF-normalized): %s ; scenarios: %d ; K = %d ; HDM-CHAR informative runs counted by the V4 memo: %d per scenario'
    % (lf_sha256(CAT4), len(SC4), K4, V4_HDM))
say('   V4 memo file: %s ; SHA-256 (LF-normalized): %s' % (rel(MEMO4), lf_sha256(MEMO4)))
say('   V4 block B rows reproduced exactly from the V4 catalog: %s' % ('yes' if v4_reproduced else 'NO'))
say('C.1 V5 minus V4 per row')
say('   %-5s %8s %8s %8s %8s %8s' % ('row', 'V4', 'V5', 'delta', 'G', 'D'))
for i, ((p, a, u, c, v, per, G, D, T, pb), (per4, G4, D4, T4)) in enumerate(zip(base, v4rows)):
    say('   row%-2d %8d %8d %+8d %+8d %+8d' % (i + 1, T4, T, T - T4, G - G4, D - D4))
say('   by cause (each scenario charged once; the four columns add up to the delta):')
added = sorted(set(byid) - set(by4))
removed = sorted(set(by4) - set(byid))
common = sorted(set(byid) & set(by4))
causes = OrderedDict([('twins (AR4-06 (3))', [x for x in added if x in TWINS]),
                      ('other added', [x for x in added if x not in TWINS]),
                      ('changed label', [x for x in common if byid[x]['REPETITION'] != by4[x]['REPETITION']]),
                      ('removed', removed)])
say('   %-5s' % 'row' + ''.join('%22s' % k for k in causes))
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    cells = []
    for k, ids in causes.items():
        d = 0
        for x in ids:
            r5 = runs(byid[x], derived_rule(byid[x]), composition(byid[x]), v) if x in byid else 0
            r4 = runs_from_label(by4[x]['REPETITION'], v, K4) if x in by4 else 0
            d += r5 - r4
        cells.append(d)
    unchanged = sum(runs(byid[x], derived_rule(byid[x]), composition(byid[x]), v) - runs_from_label(by4[x]['REPETITION'], v, K4)
                    for x in common if byid[x]['REPETITION'] == by4[x]['REPETITION'])
    if unchanged:
        raise SystemExit('a scenario with an unchanged label changed its runs')
    say('   row%-2d' % (i + 1) + ''.join('%+22d' % d for d in cells))
say('C.2 scenario sets, V4 -> V5')
say('   scenarios %d -> %d; added %d: %s' % (len(SC4), len(SC), len(added), ', '.join(added) or 'none'))
say('   removed: %s' % (', '.join(removed) or 'none'))
for x in causes['changed label']:
    say('   changed label: %s: %s -> %s' % (x, by4[x]['REPETITION'], byid[x]['REPETITION']))
gov_changed = [x for x in common if byid[x]['RUN_GOVERNANCE'] != by4[x]['RUN_GOVERNANCE']]
for x in gov_changed:
    say('   changed governance: %s: %s -> %s' % (x, by4[x]['RUN_GOVERNANCE'], byid[x]['RUN_GOVERNANCE']))
build_changed = [x for x in common if byid[x]['BUILD_UNDER_TEST'] != by4[x]['BUILD_UNDER_TEST']]
say('   changed build under test: %d: %s' % (len(build_changed), ', '.join('%s (%s -> %s)' % (x, by4[x]['BUILD_UNDER_TEST'], byid[x]['BUILD_UNDER_TEST'])
                                                                          for x in build_changed)))
say()

say('D. SENSITIVITIES (finite differences at each illustrative row; ILLUSTRATIVE)')
say('D.1 N_B moved by -1 and by +1 with N_A, N_det, n, E, K fixed (N_B > N_A makes every MIXED term follow N_B)')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    lo = dict(v, N_B=v['N_B'] - 1)
    hi = dict(v, N_B=v['N_B'] + 1)
    say('    row%d: N_B %d -> %d: %+d runs ; N_B %d -> %d: %+d runs' % (
        i + 1, v['N_B'], lo['N_B'], evaluate(lo)[3] - T, v['N_B'], hi['N_B'], evaluate(hi)[3] - T))
say('D.2 EVIDENCE_REPETITION 1 -> 2 (everything else fixed)')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    say('    row%d: %+d runs' % (i + 1, evaluate(dict(v, E=2))[3] - T))
say('D.3 K + 1: one more PARAM-03 sample member (a new governing CL-CLEAN-<fixture> scenario, mixed class) with its dry-run scenario')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    say('    row%d: %+d runs (the member itself %d, its dry runs %d)' % (
        i + 1, evaluate(dict(v, K=K + 1), extra_sample=1)[3] - T, max(v['N_A'], v['N_B'], math.ceil(v['n'] / (K + 1))), v['N_B']))
p, a, u, c, v, per, G, D, T, pb = base[0]
v01 = params(p, a, 0.01, c)
T01 = evaluate(v01)[3]
say('    example where n dominates (row1 with u = 0.01, n = %d): K %d -> %d: %+d runs' % (
    v01['n'], K, K + 1, evaluate(dict(v01, K=K + 1), extra_sample=1)[3] - T01))
say('D.4 u and c: they enter only through ceil(n / K); TOTAL changes only when ceil(n / K) > N_strict of the sample members.')
say('    u* = 1 - (1 - c)^(1 / (K * N_strict)) is the value below which that happens (then only the CLEAN term grows)')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    ustar = 1 - (1 - c) ** (1.0 / (K * v['N_A']))
    uh = params(p, a, u / 2, c)
    ch = params(p, a, u, 1 - (1 - c) / 2)
    say('    row%d: u* = %.4f ; u halved (%.3f): n %d -> %d, %+d runs ; 1 - c halved (c = %.3f): n %d -> %d, %+d runs' % (
        i + 1, ustar, u / 2, v['n'], uh['n'], evaluate(uh)[3] - T, 1 - (1 - c) / 2, v['n'], ch['n'], evaluate(ch)[3] - T))
say('    example below u* (row1 with u = 0.01): n = %d, ceil(n / K) = %d > N = %d: %+d runs, all in the CLEAN term' % (
    v01['n'], math.ceil(v01['n'] / K), v01['N_A'], evaluate(v01)[3] - base[0][8]))
say('D.5 alpha halved and p halved (for both classes and for the determinism pair at once)')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    va = params(p, a / 2, u, c)
    vp = params(p / 2, a, u, c)
    say('    row%d: alpha %.3f -> %.4f: N %d -> %d, TOTAL %d -> %d (%+.0f%%) ; p %.2f -> %.3f: N %d -> %d, TOTAL %d -> %d (%+.0f%%)' % (
        i + 1, a, a / 2, v['N_A'], va['N_A'], T, evaluate(va)[3], 100.0 * (evaluate(va)[3] - T) / T,
        p, p / 2, v['N_A'], vp['N_A'], T, evaluate(vp)[3], 100.0 * (evaluate(vp)[3] - T) / T))
say('D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    say('    row%d: G = %d slots: worst-case governing attempts = %d * m (each unit of m adds up to %d attempts); '
        'the %d non-governing runs have no slot' % (i + 1, G, G, G, D))
say('D.7 HDM-CHAR-* informative runs: exactly one per varied key (AR4-14 (b)(ii)): %d in all, the same at every row; no Owner'
    % sum(V_OF.values()))
say('    answer moves them and no other authority adds runs')
say('D.8 N_det moved by +1 with N_A, N_B, n, E, K fixed (Q-O3 alone)')
for i, (p, a, u, c, v, per, G, D, T, pb) in enumerate(base):
    per_hi, G_hi, D_hi, T_hi, pb_hi = evaluate(dict(v, N_det=v['N_det'] + 1))
    comp_hi = sum(runs(s, r, comp, dict(v, N_det=v['N_det'] + 1)) - runs(s, r, comp, v)
                  for s, r, comp in char_rows if char_kind(s) == 'COMPOSITION')
    say('    row%d: N_det %d -> %d: %+d runs (LEARN %+d, composition check %+d, VALIDATE %+d: max(N_strict, N_det) follows N_det once'
        % (i + 1, v['N_det'], v['N_det'] + 1, T_hi - T, per_hi['LEARN'] - per['LEARN'], comp_hi, per_hi['VALIDATE'] - per['VALIDATE']))
    say('           it exceeds N_strict = %d)' % v['N_A'])
say()

say('E. RULINGS OF DECISIONS SECTION 217 APPLIED BY BA-10 V5 (checks; no other reading is sized)')
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
say('    HDM-CHAR-* scenarios: row1 %d runs, row2 %d, row3 %d, row4 %d'
    % tuple(char_cnt['PIN_FEEDING'] * b[4]['N_A'] + char_cnt['HDM'] * b[4]['N_A'] for b in base))
say('E.3 AR4-14 (b)(ii) and AR4-07: %d HDM-CHAR-* scenarios, CHAR_RUNS(s) = N_A + V(s) with V(s) = %s; informative runs %d in all'
    % (len(HDM_SC), ', '.join('%d' % V_OF[s['SCENARIO_ID']] for s in HDM_SC), sum(V_OF.values())))
say('E.4 AR4-01: %d governing learning micro-scenarios (rule LEARN, N_det + 1 each, on the %s); EV-L-COMPOSED non-governing'
    % (sum(1 for s, r, comp in rows if r == 'LEARN' and s['RUN_GOVERNANCE'] == 'GOVERNING'),
       ', '.join(sorted({s['BUILD_UNDER_TEST'] for s, r, comp in rows if r == 'LEARN'}))))
say('    (CHAR, N_det + 1); the Q-I14 family on the CHARACTERIZATION_BUILD with %d dry-run scenarios'
    % sum(1 for s in SC if s['MODE'] == 'DRY_RUN' and s['SCENARIO_ID'][len('WU-DRY-'):] in I14_FAMILY))
say()

say('F. RUNS OF THE DRAFT SCENARIOS (counted in the totals of block B as the draft catalog states them; a scenario with two')
say('   blockers is counted under each; the memo does not model any ruling)')
BLOCKERS = list(cat['BLOCKED_BY_VOCABULARY'].keys())
say('F.1 by blocker')
say('   %-26s %9s %9s' % ('BLOCKED_BY', 'scenarios', 'governing') + ''.join('%9s' % ('row%d' % (i + 1)) for i in range(len(base))))
for bl in BLOCKERS:
    sel = [(s, r, comp) for s, r, comp in rows if bl in s['BLOCKED_BY']]
    say('   %-26s %9d %9d' % (bl, len(sel), sum(1 for s, r, comp in sel if s['RUN_GOVERNANCE'] == 'GOVERNING'))
        + ''.join('%9d' % sum(runs(s, r, comp, b[4]) for s, r, comp in sel) for b in base))
say('   scenarios per rule under each blocker:')
for bl in BLOCKERS:
    sel = [(s, r, comp) for s, r, comp in rows if bl in s['BLOCKED_BY']]
    say('    %s: %s' % (bl, ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(r for s, r, comp in sel).items()))))
say('F.2 by status')
say('   %-30s %9s' % ('STATUS', 'scenarios') + ''.join('%9s' % ('row%d' % (i + 1)) for i in range(len(base))))
for st in sorted({s['STATUS'] for s in SC}):
    sel = [(s, r, comp) for s, r, comp in rows if s['STATUS'] == st]
    say('   %-30s %9d' % (st, len(sel)) + ''.join('%9d' % sum(runs(s, r, comp, b[4]) for s, r, comp in sel) for b in base))
say('F.3 by run prerequisite (a scenario with two prerequisites is counted under each)')
say('   %-38s %9s' % ('RUN_PREREQUISITES', 'scenarios') + ''.join('%9s' % ('row%d' % (i + 1)) for i in range(len(base))))
for pr in sorted({x for s in SC for x in s['RUN_PREREQUISITES']}):
    sel = [(s, r, comp) for s, r, comp in rows if pr in s['RUN_PREREQUISITES']]
    say('   %-38s %9d' % (pr, len(sel)) + ''.join('%9d' % sum(runs(s, r, comp, b[4]) for s, r, comp in sel) for b in base))
AQ02_SOURCES = ['PR-CLOSURE-COMPLETENESS', 'WU-BOUNDARY']
say('F.4 open question AQ-V5-02 (BA-04 V5 section 8.2 and BA-09 V5; recorded in BA-04 V5 as Q-V5-BA04-2): per-build qualification')
say('    of the qualification scenarios whose id is neither Q-* nor E6-C*. An affirmative ruling would add a twin <id>-CB on the')
say('    CHARACTERIZATION_BUILD for each, with the rule of its source (as for the twins of A.9); the memo does not model the ruling')
aq02 = [byid[x] for x in AQ02_SOURCES if x in byid]
for s in aq02:
    say('    %-24s build %s, rule %s, REACH %s; its source has a dry-run scenario: %s; the twin itself: %s' % (
        s['SCENARIO_ID'], s['BUILD_UNDER_TEST'], derived_rule(s), s['REACH'], 'yes' if ('WU-DRY-' + s['SCENARIO_ID']) in byid else 'no',
        ' ; '.join('row%d %+d' % (i + 1, runs(s, derived_rule(s), composition(s), b[4])) for i, b in enumerate(base))))
say('    missing from the catalog: %s' % (', '.join(x for x in AQ02_SOURCES if x not in byid) or 'none'))
say('    both twins: %s runs' % ' ; '.join('row%d %+d' % (i + 1, sum(runs(s, derived_rule(s), composition(s), b[4]) for s in aq02))
                                          for i, b in enumerate(base)))
evm_cb = [(s, r, comp) for s, r, comp in rows if s['BUILD_UNDER_TEST'] == 'CHARACTERIZATION_BUILD'
          and s['RUN_GOVERNANCE'] == 'GOVERNING' and 'EVM_FROZEN' in s['PIN_SLOTS']]
say('F.5 open question AQ-V5-01 (BA-04 V5 section 8.2; recorded in BA-04 V5 as Q-V5-BA04-1): governing CHARACTERIZATION_BUILD')
say('    scenarios whose PIN_SLOTS name EVM_FROZEN (learned and validated on the PRODUCT_BUILD, AR4-01): %d; per rule: %s'
    % (len(evm_cb), ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(r for s, r, comp in evm_cb).items()))))
say('    of them naming BUILD_EQUIVALENCE_RECORD in RUN_PREREQUISITES: %d; their runs: %s'
    % (sum(1 for s, r, comp in evm_cb if 'BUILD_EQUIVALENCE_RECORD' in s['RUN_PREREQUISITES']),
       ' ; '.join('row%d %d' % (i + 1, sum(runs(s, r, comp, b[4]) for s, r, comp in evm_cb)) for i, b in enumerate(base))))
say('    an affirmative ruling adds the BUILD_EQUIVALENCE_RECORD to their RUN_PREREQUISITES and moves no run count; the effect of any')
say('    other answer is the Architect\'s to state and is not modeled')
say()
say('END OF OUTPUT')

text = '\n'.join(OUT) + '\n'
sys.stdout.buffer.write(text.encode('utf-8'))
