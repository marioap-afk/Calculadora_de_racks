"""I-52 CT-21D cost memo V4: arithmetic over the draft scenario catalog (NOT SEALED; ILLUSTRATIVE ONLY).

Reads docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json and evaluates the symbolic run expression of
BA-10 V4 section 2.3 for the illustrative choices used in BA-10 V3 section 2.3. It also recomputes the V3 figures from the
V3 catalog (reconciliation of 9358 -> 9377, decisions section 214.1) and prints sensitivities.

It reads the key variables of the HOST_DEFAULT_MAP pin rule from BA-08 V4 section 11.2 (rows HDM-2 and HDM-4): the
HDM-CHAR-* scenarios run CHAR_RUNS = N_A runs at the pinned key values plus at least one informative run per varied key
(outside the intent) (BA-10 V4 section 2.2). The memo counts the minimum, one informative run per key. It also checks that
BA-10 V4 states that rule.

Nothing printed here is a proposal, a choice or a decision. Every parameter of BA-10 V4 stays UNSET, and no open Architect
question (AQ-V4-01, AQ-V4-03, AQ-V4-07, AQ-V4-13) is decided: blocks E and F only give sizes.

Usage: python I-52-ct21d-cost-memo-v4.py [<repo root>]
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
CAT4 = os.path.join(ROOT, 'docs', 'initiatives', 'I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json')
CAT3 = os.path.join(ROOT, 'docs', 'initiatives', 'I-52-ct21d-baseline-ba-04-scenario-catalog-v3.json')
BA08 = os.path.join(ROOT, 'docs', 'initiatives', 'I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md')
BA10 = os.path.join(ROOT, 'docs', 'initiatives', 'I-52-ct21d-baseline-ba-10-parameter-sheet-v4.md')

OUT = []


def say(line=''):
    OUT.append(line)


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


# ---------------------------------------------------------------- BA-08 V4 section 11.2: the keys of the HOST_DEFAULT_MAP pin
HDM_VARIABLES = ('CLAYER', 'CECOLOR', 'CELTYPE', 'CELTSCALE', 'CELWEIGHT', 'CETRANSPARENCY', 'CPLOTSTYLE')
HDM_STYLES_PHRASE = 'and the current text style and dimension style'
HDM_STYLES = ('current text style', 'current dimension style')
HDM4_PHRASE = 'once or more with each key variable changed'


def hdm_keys():
    """The key set of BA-08 V4 section 11.2 HDM-2, read from the file; stops (fail closed) if the row does not read as expected."""
    lines = io.open(BA08, encoding='utf-8').read().split('\n')
    hdm2 = [x for x in lines if x.startswith('| HDM-2 ')]
    hdm4 = [x for x in lines if x.startswith('| HDM-4 ')]
    if len(hdm2) != 1 or len(hdm4) != 1:
        raise SystemExit('BA-08 V4: expected one HDM-2 row and one HDM-4 row, found %d and %d' % (len(hdm2), len(hdm4)))
    cell = hdm2[0].split('|')[2]
    head, sep, _ = cell.partition(HDM_STYLES_PHRASE)
    if not sep:
        raise SystemExit('BA-08 V4 HDM-2 does not name the current text style and dimension style as keys')
    variables = tuple(re.findall(r'`([A-Z]+)`', head))
    if variables != HDM_VARIABLES:
        raise SystemExit('BA-08 V4 HDM-2 context variables differ from the list this memo expects: %r' % (variables,))
    if HDM4_PHRASE not in hdm4[0]:
        raise SystemExit('BA-08 V4 HDM-4 does not require runs with each key variable changed')
    return variables + HDM_STYLES


HDM_KEYS = hdm_keys()
V_HDM = len(HDM_KEYS)   # the minimum number of informative runs per HDM-CHAR scenario: one per varied key

# ---------------------------------------------------------------- V4 catalog: derive the BA-10 V4 rule of every scenario
cat = json.load(io.open(CAT4, encoding='utf-8'))
GT = cat['GROUP_TABLE']
SAMPLE = list(cat['PARAM_03_SAMPLE'])
SC = cat['SCENARIOS']
byid = {s['SCENARIO_ID']: s for s in SC}


def composition(s):
    """Classes of the class-A and class-B CONTROL groups the scenario serves (GROUPS_SERVED; KR and evidence groups excluded;
    CONTRACT_ASSIGNED_GROUPS never enter, AR3-06; BA-10 V4 Q-A1 = AQ-V4-13 (a))."""
    cls = sorted({GT[g]['class'] for g in s['GROUPS_SERVED'] if GT[g]['kind'] == 'CONTROL' and GT[g]['class'] in ('A', 'B')})
    return {'A': 'A_ONLY', 'AB': 'MIXED', 'B': 'B_ONLY', '': 'NONE'}[''.join(cls)]


def derived_rule(s):
    """The rule of BA-10 V4 section 2.2 (table of repetition rules), derived from the scenario's properties."""
    comp = composition(s)
    if s['MODE'] == 'DRY_RUN':
        return 'DRY'
    if s['MODE'] == 'LEARNING' and comp == 'NONE':
        return 'LEARN'
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
    """CHAR_RUNS kind of BA-10 V4 section 2.2: exploratory; the HOST_DEFAULT_MAP scenarios (group HDM); other pin or selection."""
    if s['RUN_GOVERNANCE'] == 'EXPLORATORY_NON_GOVERNING':
        return 'EXPLORATORY'
    if s['GROUP'] == 'HDM':
        return 'HDM'
    return 'PIN_FEEDING'


NS_LABEL = {'A_ONLY': 'N_A', 'MIXED': 'max(N_A, N_B)', 'B_ONLY': 'N_B'}
CHAR_LABEL = {'PIN_FEEDING': 'CHAR_RUNS = N_A',
              'HDM': 'CHAR_RUNS = N_A at the pinned key values, plus at least one informative run per varied key (outside the intent)',
              'EXPLORATORY': 'CHAR_RUNS = EVIDENCE_REPETITION (exploratory)'}


def derived_label(s, r, comp):
    """The exact REPETITION label (BA-04 V4 notation) of the BA-10 V4 rule, with N_strict and the CHAR_RUNS kind resolved."""
    ns = NS_LABEL.get(comp)
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

RULES = ['DEFAULT', 'CLEAN', 'VALIDATE', 'LEARN', 'EVIDENCE', 'EVIDENCE_STRICT', 'SYNTH', 'DRY', 'CHAR']
cnt = Counter((r, comp) for s, r, comp in rows)
rule_tot = Counter(r for s, r, comp in rows)
char_rows = [(s, r, comp) for s, r, comp in rows if r == 'CHAR']
char_cnt = Counter(char_kind(s) for s, r, comp in char_rows)

ba10_text = io.open(BA10, encoding='utf-8').read()
BA10_HDM_PHRASE = 'N_A` runs at the pinned key values plus at least one informative run per varied key (outside the intent)'
ba10_states_hdm = BA10_HDM_PHRASE in ba10_text
ba10_rules = re.findall(r'^\| `([A-Z_]+)` \| ', ba10_text.split('**Repetition rules', 1)[1].split('**`WU_DRY_RUNS`', 1)[0], re.M)


def n_strict(comp, NA, NB):
    return {'A_ONLY': NA, 'MIXED': max(NA, NB), 'B_ONLY': NB, 'NONE': 0}[comp]


def runs(s, r, comp, v):
    """Planned runs R(s) of BA-10 V4 section 2.2 for the parameter values v."""
    NA, NB, n, Nd, E, k = v['N_A'], v['N_B'], v['n'], v['N_det'], v['E'], v['K']
    ns = n_strict(comp, NA, NB)
    if r == 'DEFAULT':
        return ns
    if r == 'CLEAN':
        return max(ns, math.ceil(n / k))
    if r == 'VALIDATE':
        return max(ns, Nd)
    if r == 'LEARN':
        return Nd + 1
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
            return NA * v['HDM_PINNED_CONFIGS'] + v['V']
        return NA
    raise ValueError(r)


def evaluate(v, extra_sample=0):
    """TOTAL, split by rule, into governing planned runs G (repetition slots) and non-governing runs D.
    extra_sample adds that many new PARAM-03 sample members (mixed class, like the others) with one dry-run scenario each."""
    per = Counter()
    G = D = 0
    for s, r, comp in rows:
        x = runs(s, r, comp, v)
        per[r] += x
        if s['RUN_GOVERNANCE'] == 'GOVERNING':
            G += x
        else:
            D += x
    for _ in range(extra_sample):
        x = max(n_strict('MIXED', v['N_A'], v['N_B']), math.ceil(v['n'] / v['K']))
        per['CLEAN'] += x
        per['DRY'] += v['N_B']
        G += x
        D += v['N_B']
    return per, G, D, G + D


def params(p, alpha, u, c, E=1, pB=None, aB=None, pd=None, ad=None, k=None):
    NA = n_zero_failure(p, alpha)
    NB = n_zero_failure(pB if pB is not None else p, aB if aB is not None else alpha)
    Nd = n_zero_failure(pd if pd is not None else p, ad if ad is not None else alpha)
    return OrderedDict([('N_A', NA), ('N_B', NB), ('n', n_sample(u, c)), ('N_det', Nd), ('E', E), ('K', k if k is not None else K),
                        ('V', V_HDM), ('HDM_PINNED_CONFIGS', 1)])


ILLUSTRATIVE = [(0.10, 0.05, 0.05, 0.95), (0.20, 0.05, 0.10, 0.95), (0.30, 0.10, 0.10, 0.90), (0.50, 0.10, 0.20, 0.90)]

say('I-52 CT-21D COST MEMO V4 - ARITHMETIC OUTPUT (ILLUSTRATIVE; NOT A PROPOSAL, NOT A CHOICE, NOT A DECISION)')
say('catalog file    : docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json')
say('catalog SHA-256 : %s (LF-normalized bytes)' % lf_sha256(CAT4))
say('catalog version : %s ; scenarios: %d' % (cat['CATALOG_VERSION'], len(SC)))
say('key source file : docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md (section 11.2, rows HDM-2 and HDM-4)')
say('key source hash : %s (SHA-256, LF-normalized bytes)' % lf_sha256(BA08))
say()
say('A. COUNTS (read from the draft catalog; they change with the catalog)')
say('A.1 rule derived from the scenario properties (BA-10 V4 section 2.2) vs the REPETITION field of the catalog: %s'
    % ('all %d agree' % len(SC) if not mismatch else 'MISMATCH %r' % mismatch))
say('A.2 scenarios per rule and class composition (classes of the class-A/B CONTROL groups of GROUPS_SERVED; KR, evidence')
say('    groups and CONTRACT_ASSIGNED_GROUPS excluded)')
say('    %-16s %6s %7s %6s %7s %6s' % ('rule', 'total', 'A_ONLY', 'MIXED', 'B_ONLY', 'NONE'))
for r in RULES:
    say('    %-16s %6d %7d %6d %7d %6d' % (r, rule_tot[r], cnt[(r, 'A_ONLY')], cnt[(r, 'MIXED')], cnt[(r, 'B_ONLY')], cnt[(r, 'NONE')]))
say('    %-16s %6d' % ('ALL', len(SC)))
say('A.3 CHAR split: pin- or selection-feeding %d (E4-LEARN-R1-*, LK-*); HOST_DEFAULT_MAP %d (HDM-CHAR-*, group HDM); exploratory %d (E6-C4)'
    % (char_cnt['PIN_FEEDING'], char_cnt['HDM'], char_cnt['EXPLORATORY']))
say('A.4 PARAM-03 sample (catalog field PARAM_03_SAMPLE): K = %d; every member GOVERNING with rule CLEAN: %s' % (K, 'yes' if sample_ok else 'NO'))
say('    members: %s' % ', '.join(SAMPLE))
say('    CL-CLEAN-* ids outside the sample: %s' % (', '.join(clean_not_in_sample) or 'none'))
say('A.5 exact REPETITION label (N_strict resolved to N_A, N_B or max(N_A, N_B); CHAR_RUNS kind resolved) vs the catalog: %s'
    % ('all %d agree' % len(SC) if not label_mismatch else 'MISMATCH %r' % label_mismatch))
say('    governing mixed-class scenarios: %d; of them with a label without max(N_A, N_B): %d'
    % (len(mixed_governing), len(mixed_without_max)))
say('A.6 HOST_DEFAULT_MAP keys (BA-08 V4 section 11.2 HDM-2): %s = %d keys' % (', '.join(HDM_KEYS), V_HDM))
say('    HDM-4 requires, for each HDM-CHAR-* scenario, runs at the pinned key values and one or more runs with each key variable')
say('    changed; this memo counts the minimum: V = %d informative runs per HDM-CHAR-* scenario (outside the intent)' % V_HDM)
say('A.7 BA-10 V4 states the HDM-CHAR-* CHAR_RUNS rule (N_A runs at the pinned key values plus at least one informative run per')
say('    varied key): %s; rules of its section 2.2 table: %s' % ('yes' if ba10_states_hdm else 'NO', ', '.join(ba10_rules)))
say()

say('B. ILLUSTRATIVE TOTALS FOR THE CHOICES USED IN BA-10 V3 SECTION 2.3')
say('   assumptions (ILLUSTRATIVE): one pair for both classes (p_A = p_B = p, alpha_A = alpha_B = alpha), p_det = p, alpha_det = alpha,')
say('   EVIDENCE_REPETITION = 1, K = %d; CHAR_RUNS = N_A (pin- or selection-feeding), N_A + %d (HDM-CHAR-*: N_A runs at the pinned' % (K, V_HDM))
say('   key values plus one informative run per varied key, the minimum) and EVIDENCE_REPETITION (exploratory)')
say('   %-5s %-6s %-5s %-5s %4s %4s %6s %9s %7s %7s %7s' % ('p', 'alpha', 'u', 'c', 'N', 'n', 'N_det', 'ceil(n/K)', 'G', 'D', 'TOTAL'))
base = []
for p, a, u, c in ILLUSTRATIVE:
    v = params(p, a, u, c)
    per, G, D, T = evaluate(v)
    base.append((p, a, u, c, v, per, G, D, T))
    say('   %-5.2f %-6.2f %-5.2f %-5.2f %4d %4d %6d %9d %7d %7d %7d' % (p, a, u, c, v['N_A'], v['n'], v['N_det'], math.ceil(v['n'] / K), G, D, T))
say('   G = governing planned runs (repetition slots); D = non-governing runs (dry runs, CHAR, any non-governing learning run); TOTAL = G + D.')
say('   %d SYNTH runs per row are offline evaluations (no AutoCAD process).' % rule_tot['SYNTH'])
say()
say('B.1 terms per rule')
say('   %-16s' % 'rule' + ''.join('%9s' % ('row%d' % (i + 1)) for i in range(len(base))))
for r in RULES:
    say('   %-16s' % r + ''.join('%9d' % b[5][r] for b in base))
say('B.2 CHAR term per kind')
for kind in ('PIN_FEEDING', 'HDM', 'EXPLORATORY'):
    say('   %-16s' % kind + ''.join('%9d' % sum(runs(s, r, comp, b[4]) for s, r, comp in char_rows if char_kind(s) == kind) for b in base))
say('   (HDM: %d scenarios * (N_A + %d); the %d informative runs per row are outside the intent)' % (char_cnt['HDM'], V_HDM, char_cnt['HDM'] * V_HDM))
say()

say('C. RECONCILIATION WITH BA-10 V3 SECTION 2.3 (V3 formula over the V3 catalog)')
cat3 = json.load(io.open(CAT3, encoding='utf-8'))
REG3 = cat3['REGISTRY_MAP']
inv3 = {v: k for k, v in cat3['REPETITION_RULES'].items()}
c3 = Counter()
for s in cat3['SCENARIOS']:
    rule = inv3[s['REPETITION']]
    if rule in ('DEFAULT', 'CHAR', 'CLEAN'):
        classes = {REG3[k]['class'] for k in s['CONTROLS_EXERCISED']}
        c3[rule + ('_A' if 'A' in classes else '_B')] += 1
    else:
        c3[rule] += 1
nc3 = c3['CLEAN_A'] + c3['CLEAN_B']
say('   V3 catalog SHA-256 (LF-normalized): %s ; scenarios: %d' % (lf_sha256(CAT3), len(cat3['SCENARIOS'])))
say('   V3 counts: CLEAN %d, DEFAULT_A %d, DEFAULT_B %d, CHAR %d, LEARN %d, EVIDENCE %d, SYNTH %d, DRY %d'
    % (nc3, c3['DEFAULT_A'], c3['DEFAULT_B'], c3['CHAR_A'] + c3['CHAR_B'], c3['LEARN'], c3['EVIDENCE'], c3['SYNTH'], c3['DRY']))
say('   %-5s %-6s %-5s %-5s %15s %22s %11s' % ('p', 'alpha', 'u', 'c', 'V3 formula', 'V3 printed (19*N_det)', 'difference'))
for p, a, u, c in ILLUSTRATIVE:
    N, n = n_zero_failure(p, a), n_sample(u, c)
    common = nc3 * max(N, math.ceil(n / nc3)) + c3['DEFAULT_A'] * N + c3['DEFAULT_B'] * N + (c3['CHAR_A'] + c3['CHAR_B']) * N \
        + c3['EVIDENCE'] * 1 + c3['SYNTH'] + c3['DRY'] * N
    t_formula = common + c3['LEARN'] * (N + 1)
    t_printed = common + c3['LEARN'] * N
    say('   %-5.2f %-6.2f %-5.2f %-5.2f %15d %22d %11d' % (p, a, u, c, t_formula, t_printed, t_formula - t_printed))
say('   The V3 table printed the right-hand figures: it charged LEARN as 19 * N_det instead of 19 * (N_det + 1).')
say('C.1 V4 minus V3 (V3 formula) per row, by comparable block')
say('   %-5s %8s %8s %8s %13s %9s %15s %10s %7s' % ('row', 'V3', 'V4', 'delta', 'DEFAULT+CHAR', 'CLEAN', 'LEARN+VALIDATE', 'EVIDENCE*', 'DRY'))
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    N, n = v['N_A'], v['n']
    b3 = OrderedDict([
        ('DC', (c3['DEFAULT_A'] + c3['DEFAULT_B'] + c3['CHAR_A'] + c3['CHAR_B']) * N),
        ('CL', nc3 * max(N, math.ceil(n / nc3))),
        ('LV', c3['LEARN'] * (N + 1)),
        ('EV', c3['EVIDENCE'] * 1 + c3['SYNTH']),
        ('DR', c3['DRY'] * N)])
    b4 = OrderedDict([
        ('DC', per['DEFAULT'] + per['CHAR']),
        ('CL', per['CLEAN']),
        ('LV', per['LEARN'] + per['VALIDATE']),
        ('EV', per['EVIDENCE'] + per['EVIDENCE_STRICT'] + per['SYNTH']),
        ('DR', per['DRY'])])
    t3 = sum(b3.values())
    say('   row%-2d %8d %8d %+8d %+13d %+9d %+15d %+10d %+7d' % (
        i + 1, t3, T, T - t3, b4['DC'] - b3['DC'], b4['CL'] - b3['CL'], b4['LV'] - b3['LV'], b4['EV'] - b3['EV'], b4['DR'] - b3['DR']))
say('   EVIDENCE* = EVIDENCE + EVIDENCE_STRICT + SYNTH (V4) against EVIDENCE + SYNTH (V3)')
dry3 = sorted(s['SCENARIO_ID'] for s in cat3['SCENARIOS'] if s['MODE'] == 'DRY_RUN')
dry4 = sorted(s['SCENARIO_ID'] for s in SC if s['MODE'] == 'DRY_RUN')
own3 = sorted(s['SCENARIO_ID'] for s in cat3['SCENARIOS'] if s['MODE'] != 'DRY_RUN')
own4 = sorted(s['SCENARIO_ID'] for s in SC if s['MODE'] != 'DRY_RUN')
say('C.2 scenario sets, V3 -> V4')
say('   own scenarios %d -> %d; added: %s' % (len(own3), len(own4), ', '.join(sorted(set(own4) - set(own3))) or 'none'))
say('   own scenarios removed: %s' % (', '.join(sorted(set(own3) - set(own4))) or 'none'))
say('   dry-run scenarios %d -> %d; added: %s' % (len(dry3), len(dry4), ', '.join(sorted(set(dry4) - set(dry3))) or 'none'))
say('   dry-run scenarios removed: %s' % (', '.join(sorted(set(dry3) - set(dry4))) or 'none'))
say()

say('D. SENSITIVITIES (finite differences at each illustrative row; ILLUSTRATIVE)')
say('D.1 N_B moved by -1 and by +1 with N_A, N_det, n, E, K fixed (N_B > N_A makes every MIXED term follow N_B)')
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    lo = dict(v, N_B=v['N_B'] - 1)
    hi = dict(v, N_B=v['N_B'] + 1)
    say('    row%d: N_B %d -> %d: %+d runs ; N_B %d -> %d: %+d runs' % (
        i + 1, v['N_B'], lo['N_B'], evaluate(lo)[3] - T, v['N_B'], hi['N_B'], evaluate(hi)[3] - T))
say('D.2 EVIDENCE_REPETITION 1 -> 2 (everything else fixed)')
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    say('    row%d: %+d runs' % (i + 1, evaluate(dict(v, E=2))[3] - T))
say('D.3 K + 1: one more PARAM-03 sample member (a new governing CL-CLEAN-<fixture> scenario, mixed class) with its dry-run scenario')
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    say('    row%d: %+d runs (the member itself %d, its dry runs %d)' % (
        i + 1, evaluate(dict(v, K=K + 1), extra_sample=1)[3] - T, max(v['N_A'], v['N_B'], math.ceil(v['n'] / (K + 1))), v['N_B']))
p, a, u, c, v, per, G, D, T = base[0]
v01 = params(p, a, 0.01, c)
T01 = evaluate(v01)[3]
say('    example where n dominates (row1 with u = 0.01, n = %d): K %d -> %d: %+d runs' % (
    v01['n'], K, K + 1, evaluate(dict(v01, K=K + 1), extra_sample=1)[3] - T01))
say('D.4 u and c: they enter only through ceil(n / K); TOTAL changes only when ceil(n / K) > N_strict of the sample members.')
say('    u* = 1 - (1 - c)^(1 / (K * N_strict)) is the value below which that happens (then only the CLEAN term grows)')
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    ustar = 1 - (1 - c) ** (1.0 / (K * v['N_A']))
    uh = params(p, a, u / 2, c)
    ch = params(p, a, u, 1 - (1 - c) / 2)
    say('    row%d: u* = %.4f ; u halved (%.3f): n %d -> %d, %+d runs ; 1 - c halved (c = %.3f): n %d -> %d, %+d runs' % (
        i + 1, ustar, u / 2, v['n'], uh['n'], evaluate(uh)[3] - T, 1 - (1 - c) / 2, v['n'], ch['n'], evaluate(ch)[3] - T))
p, a, u, c, v, per, G, D, T = base[0]
v01 = params(p, a, 0.01, c)
say('    example below u* (row1 with u = 0.01): n = %d, ceil(n / K) = %d > N = %d: %+d runs, all in the CLEAN term' % (
    v01['n'], math.ceil(v01['n'] / K), v01['N_A'], evaluate(v01)[3] - T))
say('D.5 alpha halved and p halved (for both classes and for the determinism pair at once)')
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    va = params(p, a / 2, u, c)
    vp = params(p / 2, a, u, c)
    say('    row%d: alpha %.3f -> %.4f: N %d -> %d, TOTAL %d -> %d (%+.0f%%) ; p %.2f -> %.3f: N %d -> %d, TOTAL %d -> %d (%+.0f%%)' % (
        i + 1, a, a / 2, v['N_A'], va['N_A'], T, evaluate(va)[3], 100.0 * (evaluate(va)[3] - T) / T,
        p, p / 2, v['N_A'], vp['N_A'], T, evaluate(vp)[3], 100.0 * (evaluate(vp)[3] - T) / T))
say('D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts')
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    say('    row%d: G = %d slots: worst-case governing attempts = %d * m (each unit of m adds up to %d attempts); '
        'the %d non-governing runs have no slot' % (i + 1, G, G, G, D))
say('D.7 HDM-CHAR-* informative runs (outside the intent; no Owner answer moves them): the minimum is %d per scenario, %d in all;'
    % (V_HDM, char_cnt['HDM'] * V_HDM))
say('    one more informative run per varied key in every HDM-CHAR-* scenario adds %+d runs at every row' % (
    evaluate(dict(base[0][4], V=2 * V_HDM))[3] - base[0][8]))
say()

say('E. OPEN ARCHITECT QUESTION AQ-V4-13 OF BA-10 V4 SECTION 6: size of the other readings (no reading is decided here)')
say('E.1 AQ-V4-13 (a) = Q-A1 (contract-assigned groups and the strictest pair)')
cag_rows = [(s, r, comp) for s, r, comp in rows if s['CONTRACT_ASSIGNED_GROUPS']]


def composition_with_cag(s):
    cls = {GT[g]['class'] for g in s['GROUPS_SERVED'] if GT[g]['kind'] == 'CONTROL' and GT[g]['class'] in ('A', 'B')}
    cls |= {GT[x['group']]['class'] for x in s['CONTRACT_ASSIGNED_GROUPS'] if GT[x['group']]['class'] in ('A', 'B')}
    return {'A': 'A_ONLY', 'AB': 'MIXED', 'B': 'B_ONLY', '': 'NONE'}[''.join(sorted(cls))]


changed = [(s['SCENARIO_ID'], comp, composition_with_cag(s)) for s, r, comp in cag_rows if composition_with_cag(s) != comp]
unchanged = [s['SCENARIO_ID'] for s, r, comp in cag_rows if composition_with_cag(s) == comp]
say('   scenarios with CONTRACT_ASSIGNED_GROUPS: %d; class composition changed by the other reading: %d' % (len(cag_rows), len(changed)))
for sid, c0, c1 in changed:
    say('    %s: %s -> %s' % (sid, c0, c1))
say('   unchanged (already of the same composition): %s' % (', '.join(unchanged) or 'none'))


def alt_runs(s, r, comp, v):
    c1 = composition_with_cag(s)
    if c1 == comp:
        return runs(s, r, comp, v)
    ns = n_strict(c1, v['N_A'], v['N_B'])
    base_value = runs(s, r, comp, v) if r != 'DEFAULT' else 0
    return max(ns, base_value)


for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    d_equal = sum(alt_runs(s, r, comp, v) - runs(s, r, comp, v) for s, r, comp in cag_rows)
    vb = dict(v, N_B=v['N_A'] + 1)
    d_nb = sum(alt_runs(s, r, comp, vb) - runs(s, r, comp, vb) for s, r, comp in cag_rows)
    say('    row%d: %+d runs with N_B = N_A ; %+d runs with N_B = N_A + 1' % (i + 1, d_equal, d_nb))
say('E.2 AQ-V4-13 (b) (the CHAR_RUNS intent), part (ii): N_A per key configuration of HDM-4 instead of N_A at the pinned key values')
say('    only: each HDM-CHAR-* scenario would run (1 + %d) * N_A times instead of N_A + %d' % (V_HDM, V_HDM))
for i, (p, a, u, c, v, per, G, D, T) in enumerate(base):
    say('    row%d: %+d runs (all non-governing, in D)' % (i + 1, evaluate(dict(v, HDM_PINNED_CONFIGS=1 + V_HDM, V=0))[3] - T))
say('    part (i) (own Owner pair for the CHAR runs) has no size without a value; it is not computed')
say()

say('F. RUNS OF THE DRAFT SCENARIOS, BY BLOCKER (counted in the totals of block B as the draft catalog states them; a scenario with')
say('   two blockers is counted under each; the memo does not model any ruling)')
BLOCKERS = list(cat['BLOCKED_BY_VOCABULARY'].keys())
say('   %-26s %9s %9s' % ('BLOCKED_BY', 'scenarios', 'governing') + ''.join('%9s' % ('row%d' % (i + 1)) for i in range(len(base))))
for bl in BLOCKERS:
    sel = [(s, r, comp) for s, r, comp in rows if bl in s['BLOCKED_BY']]
    say('   %-26s %9d %9d' % (bl, len(sel), sum(1 for s, r, comp in sel if s['RUN_GOVERNANCE'] == 'GOVERNING'))
        + ''.join('%9d' % sum(runs(s, r, comp, b[4]) for s, r, comp in sel) for b in base))
say('   scenarios per rule under each blocker:')
for bl in BLOCKERS:
    sel = [(s, r, comp) for s, r, comp in rows if bl in s['BLOCKED_BY']]
    say('    %s: %s' % (bl, ', '.join('%s %d' % (k, x) for k, x in sorted(Counter(r for s, r, comp in sel).items()))))
say('   BA-10 V4 section 2.2: the rule LEARN does not depend on the governance of a learning micro-scenario (only its place in G or D')
say('   does); a dry run exists only for a governing scenario that enters the governed window (AR3-02)')
say()
say('END OF OUTPUT')

text = '\n'.join(OUT) + '\n'
sys.stdout.buffer.write(text.encode('utf-8'))
