"""I-52 CT-21D baseline artifact BA-06 V6: reproducible checks of the statements that BA-06 V6 makes about itself, and the
frozen-parts check of the exact delta check against V5 (AR5-02, AR6-03; decisions sections 220 and 223).

Usage: python I-52-ct21d-ba06-v6-checks.py <repository root> <output file>

Reads only repository files (the V6 and V5 artifacts, the decisions file, the Gate 2 evidence, the blind recompile, the contract
texts, the evidence files the artifact binds, and, for BA06-C19 only, the source files of the checkout) and writes one JSON result
file. Fails closed: a missing input, a table or section that cannot be parsed, or any failed check gives exit code 1. The output
records no hash of the V6 artifact, because the V6 artifact binds this output by hash (section 14). Deterministic: no clock, no
network. Hashes: SHA-256 of the LF-normalized bytes, lowercase hexadecimal; git blob ids: SHA-1 of "blob <size>\\0" + LF-normalized
bytes.

BA06-C1 to BA06-C12 repeat the V5 checks on the V6 bytes (C5 with the declared V6 amendment of C-72; C9 with 32 evidence rows; C12
with the V6 header). BA06-C13: the V5 file is the one that AR5-01 carried (blob prefix e69a141d) and the one the V6 header supersedes.
BA06-C14 is the FROZEN-PARTS CHECK asked by AR5-02 and AR6-03: it compares V6 with V5 over the inventory (3.1 U-rows, 3.2 C-rows,
3.2.5 X-rows, the class table 3.3, the residual rows of section 5, the EVM fields of section 12) and the trace plan (section 7,
10.2, section 11), and over the review records inside those spans (3.2.2, 4.1, 4.2 and the non-table lines of the frozen spans). It
lists EXACTLY which rows and lines changed, with the kind of each change (CONTENT, NON_CONTENT_RULING_ORDERED, RECORD or LABEL) and
the ruling that orders it, and fails on any undeclared change or on a declared change that did not happen. BA06-C15: declared limit
(4) unchanged. BA06-C16: the mapping of section 4.4 is a partition of the 689 operations of the blind recompile, every MUTATING
operation is assigned to a U-row, and every assigned file is cited by that row. BA06-C17: the classifications of the relink ledger
3.1.1 equal the Gate 2 evidence, whose SHA-256 equals the one decisions section 223 states. BA06-C18: the per-class carry table 3.4
is recomputed from BA06-C14 and 3.3, and its carry candidates are exactly the AR6-03 list. BA06-C19: the identity table of section
2.1 (the 95690c28 column recomputed from the checkout; PASS only when the checkout's src equals 95690c28 for those paths). BA06-C20
to BA06-C24: the AR5-03 corrections, AR6-04, the AR6-02 wording, the naming state and the version labels.
"""
import difflib
import hashlib
import io
import json
import os
import re
import sys

ART = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v6.md'
V5 = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v5.md'
SELF = 'docs/automation/evidence/I-52-ct21d-ba06-v6-checks.py'
OUTPUT_NAME = 'I-52-ct21d-ba06-v6-checks-output.json'
DECISIONS = 'docs/automation/decisions/I-52.md'
GATE2 = 'docs/automation/evidence/I-52-ct21d-gate2-relink-assessment.json'
RECOMPILE = 'docs/automation/evidence/I-52-ct21d-ba06-v6-blind-recompile.json'
DL = 'docs/automation/evidence/I-52-ct21d-phase2-delta/'
SA = 'docs/automation/evidence/I-52-ct21d-phase2-static-analysis/'
TI = 'docs/automation/evidence/I-52-ct21d-ba06-v4-task-inputs.json'
CONTRACT = {'V2.1': 'docs/initiatives/I-52-ct21d-authority-contract-v2.1.md',
            'V2.2': 'docs/initiatives/I-52-ct21d-authority-contract-v2.2.md',
            'V2.4': 'docs/initiatives/I-52-ct21d-authority-contract-v2.4.md'}
FRAG_V21 = {'4.2', '7', '7.1', '15.2', '15.3', '15.4', '15.5', '15.6', '15.7', '19.1', '27.2'}
FRAG_V22 = {'PA-6', 'PA-10'}
MARK = 'The computed task text follows:\n'
BOUND = '95690c28dc6268e61dff32a0cbc33cc9fde3d47f'
FIXTURE_BUILD = '69daf03a35c630e453e1d9e98136f128bd0325a4'
LIMIT4_START = ' (4) **Build relation'
LIMIT4_PHRASES = ['same source SHA as the product build', 'every harness switch is off', 'every I-14 writer included',
                  '`RECORDED_NOT_ENFORCED` handling of the AR3-01 set', 'control-plane decisions only',
                  'both build identities', 'this attestation', 'only because of a harness difference',
                  'recorded as a finding, not added to the inventory']
RULED = {'AQ-V4-01': 'AR4-01', 'AQ-V4-11': 'AR4-12', 'AQ-V4-14': 'AR4-15'}
TOK = re.compile(r'\w+|\s+|[^\w\s]')
# closed list of the V6 version-label substitutions (kind LABEL); applied to V5 before comparing
LABELS = [('BA-03 V4', 'BA-03 V5'), ('BA-04 V5', 'BA-04 V6'), ('BA-07 V5', 'BA-07 V6'), ('BA-08 V5', 'BA-08 V6'),
          ('BA-09 V5', 'BA-09 V6'), ('BA-10 V5', 'BA-10 V6')]

# ---------------------------------------------------------------------------------------------- BA06-C14 declarations
# (part, row key) -> (kind, ruling and reason). Kinds: CONTENT (AR5-02: an operation row, a class row, a residual row or its
# "affects" cell, an EVM field or a trace-plan cell), NON_CONTENT_RULING_ORDERED (AR5-02: a reformulation ordered by a ruling that
# changes no operation, class, membership, residual membership, "affects" cell, EVM field or trace cell), RECORD (a record of the
# review, not an inventory row).
R603 = 'AR6-03 (re-derived at 95690c28; Gate 2 RELINK-0605)'
DECL_ROWS = {
    ('3.1', 'U-02'): ('CONTENT', R603 + ': R1-only call site; location through EnsureForPlan; the R2/R3 site is U-45'),
    ('3.1', 'U-03'): ('CONTENT', R603 + ', RL-02: callee read tx under L1 (R1) or L0 (R2, R3)'),
    ('3.1', 'U-04'): ('CONTENT', R603 + ', RL-02: L1 or L0'),
    ('3.1', 'U-07'): ('CONTENT', R603 + ', RL-02: L1 or L0'),
    ('3.1', 'U-08'): ('CONTENT', R603 + ', RL-02: the only PREPARE-W write under L0, released before T_create\''),
    ('3.1', 'U-09'): ('CONTENT', 'AR5-03 (class OC-PREP-READ, seam role) and ' + R603 + ' (facts consumed on R2/R3; L0; new call site)'),
    ('3.1', 'U-25'): ('CONTENT', R603 + ': second caller CreatePreparedBlock :54-57 (OC-ENVELOPE confirmation)'),
    ('3.1', 'U-26'): ('CONTENT', R603 + ': T_create\' context, as U-25 (OC-ENVELOPE confirmation)'),
    ('3.1', 'U-28'): ('CONTENT', R603 + ', RL-04: re-pinned to PlaceBlockWithJigStatus :237-238; R1 and R2/R3 entries'),
    ('3.1', 'U-29'): ('CONTENT', R603 + ', RL-04: re-pinned'),
    ('3.1', 'U-30'): ('CONTENT', R603 + ': JigPlacementResult with the prompt status; R2/R3 status mapping'),
    ('3.1', 'U-31'): ('CONTENT', R603 + ', RL-04: re-pinned; the hook point U-48 precedes it (OC-REF-TOP confirmation)'),
    ('3.1', 'U-32'): ('CONTENT', R603 + ', RL-04: re-pinned (OC-REF-TOP confirmation)'),
    ('3.1', 'U-33'): ('CONTENT', R603 + ', RL-04: re-pinned'),
    ('3.1', 'U-34'): ('CONTENT', R603 + ', RL-05, N-06: new cleanup triggers on every route; failure observable'),
    ('3.1', 'U-35'): ('CONTENT', R603 + ': call site moved to BlockPlacement.cs:213'),
    ('3.1', 'U-36'): ('CONTENT', R603 + ': re-pinned to BlockPlacement.cs:196-203'),
    ('3.1', 'U-37'): ('CONTENT', R603 + ': callers re-pinned (RSC :39, :84; RMC :33)'),
    ('3.1', 'U-38'): ('CONTENT', R603 + ': callers re-pinned (RSC :45, :119; RMC :52)'),
    ('3.1', 'U-39'): ('CONTENT', R603 + ': callers re-pinned; the batch lateral stays OUT (D-11)'),
    ('3.1', 'U-40'): ('CONTENT', R603 + ': re-pinned; batch composition site'),
    ('3.1', 'U-41'): ('CONTENT', R603 + ', RL-01: replaced (the V5 preliminaries serve only Actualizar)'),
    ('3.1', 'U-42'): ('CONTENT', R603 + ', N-01: new row (lock of CreatePreparedBlock; no class)'),
    ('3.1', 'U-43'): ('CONTENT', R603 + ', N-01: new row (T_create\' and the drawer call; site of the same primitives)'),
    ('3.1', 'U-44'): ('CONTENT', R603 + ', N-01: new row (T_create\' commit; no class)'),
    ('3.1', 'U-45'): ('CONTENT', R603 + ', N-02: new row (import call site under L0; class OC-IMPORT)'),
    ('3.1', 'U-46'): ('CONTENT', R603 + ', N-03: new row (I-60 name read; no class; NOT_YET_ADOPTED)'),
    ('3.1', 'U-47'): ('CONTENT', R603 + ', N-04: new row (TopTransaction null guard; no class)'),
    ('3.1', 'U-48'): ('CONTENT', R603 + ', N-07: new row (dormant hook point in T_place; watch item)'),
    ('3.1', 'U-49'): ('CONTENT', R603 + ', N-05: new row (regen; not a database operation)'),
    ('3.1', 'U-50'): ('CONTENT', R603 + ', N-08: new row (the Insertar edit chain; OUT, D-11)'),
    ('3.2', 'C-72'): ('CONTENT', 'AR5-03 (R1:BA06V4-N01): marked amendment of C-72'),
    ('3.2.5', 'X-05'): ('CONTENT', 'AR5-03 (R2:BA06V4-N01): new row X-05, class OC-VERIFY-READ'),
    ('3.2.2', 'DC-18'): ('RECORD', 'AR5-03 (R2:BA06V4-N01): DC-18 disposition corrected'),
    ('3.2.2', 'DC-41'): ('RECORD', 'AR5-03 (R1:BA06V4-N01): DC-41 aligned with the C-72 amendment'),
    ('3.3', '`OC-PREP-READ`'): ('CONTENT', 'AR5-03: U-09 joins'),
    ('3.3', '`OC-IMPORT`'): ('CONTENT', 'AR5-03 (U-09 leaves) and AR6-03 (U-45 joins)'),
    ('3.3', '`OC-VERIFY-READ`'): ('CONTENT', 'AR5-03: X-05 joins'),
    ('3.3', '`OC-LAYER`'): ('NON_CONTENT_RULING_ORDERED', 'AR6-02, AR5-11: fixture-cell wording (legacy product state); no '
                                                          'operation, member or fixture change (AQ-V6-BA06-04)'),
    ('5', 2): ('CONTENT', R603 + ', RL-06, RD-06: commit grouping of the batch and edit chains; "affects" adds U-42..U-45, U-50'),
    ('5', 3): ('CONTENT', R603 + ', RL-06: new lock sites; "affects" adds U-42, U-45, U-46, U-50'),
    ('5', 12): ('CONTENT', R603 + ', RL-06: batch user answers; "affects" adds U-42..U-46'),
    ('5', 15): ('CONTENT', R603 + ', RL-06, N-05: batch regen; "affects" adds U-49'),
    ('10.2', '`SH-LAY-ADDED`'): ('NON_CONTENT_RULING_ORDERED', 'AR6-02, AR5-11 (FXB-01..FXB-07), MAIN95-02, RL-07: legacy-state '
                                                              'wording; fixture and trace cells unchanged; NOT_YET_ADOPTED'),
    ('4.1', 'D-01'): ('RECORD', 'AR6-04, RL-01, RL-03'), ('4.1', 'D-04'): ('RECORD', R603 + ' (U-46, U-47)'),
    ('4.1', 'D-11'): ('RECORD', R603 + ' (U-50)'), ('4.1', 'D-13'): ('RECORD', R603 + ' (TX-A\', L0 variants)'),
    ('4.2', 'TX-A..TX-E (transaction sites)'): ('RECORD', R603 + ' (TX-A\', L0)'),
}
for _d in range(17, 26):
    DECL_ROWS[('4.1', 'D-%02d' % _d)] = ('RECORD', R603 + ' / AR6-04: new difference D-%02d' % _d)
# non-table lines of the frozen spans that change (identified by a V6 prefix)
DECL_LINES = [
    ('3', '## 3. Design-time inventory (V6: re-derived at `95690c28`)', 'RECORD', 'AR6-01, AR6-03: heading'),
    ('3', '**Composition (BA06V3-N04; V6).**', 'RECORD', 'AR6-03, AR5-03: composition names U-42..U-50, the C-72 amendment and X-05'),
    ('3', '### 3.1 Reused product path at `BOUND_PRODUCT_SHA`', 'RECORD', 'AR6-01, AR6-03: heading'),
    ('3', 'Paths of the repository at `95690c28`', 'RECORD', 'AR6-01, AR6-03: intro (RC, routes R1..R3; RACKPROYECTAR not a route)'),
    ('3', '**Source.** The 203 rows below', 'RECORD', 'AR5-03: the Source paragraph names the V6 amendment of C-72'),
    ('3', '#### 3.2.5 Supplementary contract rows (V4; V6: X-05;', 'RECORD', 'AR5-03: heading'),
    ('3', 'The compiler\'s input did not contain V2.1 3.4, 5 or 8.', 'RECORD', 'AR5-03: intro names X-05'),
    ('3.3', '- **V6 (AR6-03; AR5-03).**', 'RECORD', 'AR6-03, AR5-03: bullet on the V6 class changes'),
    ('7', '**Coverage matrix.**', 'NON_CONTENT_RULING_ORDERED', 'AR6-02: FX-ANN-BLANK named a legacy-product-state fixture; '
                                                                'no matrix cell changes'),
]
EXCLUDED_SUBSECTIONS = ['#### 3.1.1 ', '### 3.4 ']  # new records (V6), reported as added subsections


def main():
    if len(sys.argv) != 3:
        print(__doc__)
        return 2
    root, out_path = sys.argv[1], sys.argv[2]

    def path(rel):
        return os.path.join(root, rel.replace('/', os.sep))

    def raw(rel):
        return open(path(rel), 'rb').read().replace(b'\r\n', b'\n')

    def text(rel):
        return raw(rel).decode('utf-8')

    def load(rel):
        return json.loads(text(rel))

    def sha256(b):
        return hashlib.sha256(b).hexdigest()

    def blob(b):
        return hashlib.sha1(b'blob %d\x00' % len(b) + b).hexdigest()

    results = []

    def record(cid, statement, ok, detail='', report=None):
        r = {'ID': cid, 'STATEMENT': statement, 'RESULT': 'PASS' if ok else 'FAIL', 'DETAIL': detail}
        if report is not None:
            r['REPORT'] = report
        results.append(r)

    md = text(ART)
    lines = md.split('\n')
    md5 = text(V5)
    lines5 = md5.split('\n')

    def table_in(ls, header_start):
        idx = [i for i, l in enumerate(ls) if l.startswith(header_start)]
        if len(idx) != 1:
            raise SystemExit('table header not unique or missing: ' + header_start)
        i = idx[0]
        head = [c.strip() for c in ls[i].strip().strip('|').split(' | ')]
        rows = []
        j = i + 2
        while j < len(ls) and ls[j].startswith('|'):
            cells = [c.strip() for c in ls[j].strip()[2:-2].split(' | ')]
            if len(cells) != len(head):
                raise SystemExit('row with %d cells under %s: %s' % (len(cells), header_start, ls[j][:80]))
            rows.append(cells)
            j += 1
        return head, rows

    def table(header_start):
        return table_in(lines, header_start)

    def esc(t):
        return t.replace('|', '/').replace('\n', ' ')

    def span(ls, start, end):
        i = [k for k, l in enumerate(ls) if l.startswith(start)]
        j = [k for k, l in enumerate(ls) if l.startswith(end)]
        if len(i) != 1 or len(j) != 1 or j[0] <= i[0]:
            raise SystemExit('section span not unique or missing: %s .. %s' % (start, end))
        return i[0], ls[i[0]:j[0]]

    def relabel(t):
        for a, b in LABELS:
            t = t.replace(a, b)
        return t

    def edits(a, b):
        ta, tb = TOK.findall(a), TOK.findall(b)
        sm = difflib.SequenceMatcher(None, ta, tb, autojunk=False)
        return [{'OP': op.upper(), 'V5': ''.join(ta[a1:a2]), 'V6': ''.join(tb[b1:b2])}
                for op, a1, a2, b1, b2 in sm.get_opcodes() if op != 'equal']

    # BA06-C1: matrix columns = the classes of section 10.2
    head, mrows = table('| Entry point | SH-F1 |')
    cols = set(re.sub(r' \(\d\)$', '', c) for c in head[1:])
    _, shapes = table('| Class | Description | Fixture (BA-08 V6) |')
    classes = set(r[0].strip('`') for r in shapes)
    record('BA06-C1', 'the columns of the coverage matrix of section 7, with the fondo counts dropped, are exactly the plan-shape '
           'classes of section 10.2', cols == classes and len(classes) == 9,
           'columns %s; classes %s' % (sorted(cols), sorted(classes)))

    # BA06-C2..C5: the compiled rows of section 3.2
    _, crows = table('| id | actor | kind | operation | clause fragments | R-list (V2) | class |')
    comp = load(DL + 'sa1-compiled-operations.json')['operations']
    by_id = {r[0]: r for r in crows}
    mutating = [r for r in crows if r[2] == 'MUTATING']
    mapped = [r for r in mutating if r[6].startswith('OC-')]
    c105 = by_id.get('C-105', ['', '', '', '', '', '', ''])
    record('BA06-C2', '15 MUTATING rows; 14 map to a class of section 3.3; C-105 (SL-3) is n/a by the sealed scope',
           len(mutating) == 15 and len(mapped) == 14 and c105[6].startswith('n/a'),
           'MUTATING %d, mapped %d, C-105 class "%s"' % (len(mutating), len(mapped), c105[6]))
    nmr = [r for r in crows if r[2] == 'NON_MUTATING_READ']
    record('BA06-C3', 'every NON_MUTATING_READ row has a class; C-13 is marked as an instrument sample',
           len(nmr) == 10 and all(r[6] != '-' for r in nmr) and by_id['C-13'][6].startswith('instrument'),
           ', '.join('%s=%s' % (r[0], r[6]) for r in nmr))
    _, octab = table('| Class | Operations | Actor and phase | Micro-scenario (BA-04 V6) or reasoned exclusion |')
    oc_names = [r[0].strip('`') for r in octab]
    _, xrows = table('| id | actor | kind | operation | clause (outside the fragments) | origin | class |')
    cited = set(r[6].split(' ')[0] for r in crows + xrows if r[6].startswith('OC-'))
    micro_ok = all(('`EV-L-%s`' % n) in r[3] or r[3].startswith('none. Reasoned exclusion') for n, r in zip(oc_names, octab))
    n_micro = sum(1 for r in octab if r[3].startswith('`EV-L-'))
    record('BA06-C4', 'every class cited in sections 3.2 and 3.2.5 is a class of section 3.3, and every class of 3.3 has its '
           'micro-scenario EV-L-<class> or a reasoned exclusion; 21 classes (16 and 5)', cited <= set(oc_names) and micro_ok and
           len(oc_names) == len(set(oc_names)) == 21 and n_micro == 16,
           'classes %d (micro-scenario %d, exclusion %d); cited not in 3.3: %s' %
           (len(oc_names), n_micro, len(oc_names) - n_micro, sorted(cited - set(oc_names))))
    bad = []
    for o in comp:
        r = by_id.get(o['id'])
        frag = '; '.join(sorted(set(c['fragment'] for c in o['clauses'])))
        if r is None or r[1] != o['actor'] or r[2] != o['kind'] or r[4] != frag:
            bad.append(o['id'])
        elif o['id'] == 'C-98':
            if not (r[3].startswith('Keep the manifest as the exact list of what PREPARE-W added; it may persist after an abort.')
                    and '[V4 amendment' in r[3] and 'added (or changed)' in o['operation']):
                bad.append('C-98')
        elif o['id'] == 'C-72':
            if not (r[3].startswith(esc(o['operation'])) and '**[V6 amendment (AR5-03; R1:BA06V4-N01): captured once by the '
                    'orchestrator in PREPARE-R, at PR, inside the command after PL (V2.1 5 row PR, 6; AMB-09)' in r[3]):
                bad.append('C-72')
        elif r[3] != esc(o['operation']):
            bad.append(o['id'])
    record('BA06-C5', 'the 203 rows of section 3.2 transcribe sa1-compiled-operations.json (id, actor, kind, operation, clause '
           'fragments), except the declared V4 amendment of C-98 and the declared V6 amendment of C-72 (AR5-03)',
           len(crows) == len(comp) == 203 and not bad, 'rows %d; mismatches %s' % (len(crows), bad))

    # BA06-C6: difference log
    _, dcrows = table('| Id | Difference | Resolution (comparator) |')
    diffs = load(DL + 'sa1-comparison-with-t3-and-r-list.json')['differences']
    ok6 = len(dcrows) == len(diffs) == 51
    ok6 = ok6 and all(d['id'] == 'DF-%02d' % n and r[0] == 'DC-%02d' % n and r[1] == esc(d['description']) and r[5]
                      for n, (d, r) in enumerate(zip(diffs, dcrows), 1))
    record('BA06-C6', 'DF-nn = DC-nn in order; every DC row transcribes its comparator difference and carries a disposition', ok6,
           'rows %d' % len(dcrows))

    # BA06-C7: ambiguities
    _, amb = table('| Id | Ambiguity (compiler) | V4 disposition |')
    _, cqs = table('| Id | Question | Source | Reading used in V4 | Ruling or recorded deferral (section 217) |')
    cq_ids = set(r[0] for r in cqs)
    ambs = load(DL + 'sa1-compiled-operations.json')['ambiguities']
    problems = []
    for n, (a, r) in enumerate(zip(ambs, amb), 1):
        disp = r[2]
        if r[0] != 'AMB-%02d' % n or r[1] != esc(a):
            problems.append('AMB-%02d transcription' % n)
            continue
        if not (disp.startswith('RESOLVED') or disp.startswith('CONTRACT QUESTION')):
            problems.append('AMB-%02d prefix' % n)
        for q in re.findall(r'CQ-\d\d', disp):
            if q not in cq_ids:
                problems.append('AMB-%02d cites unknown %s' % (n, q))
        if disp.startswith('CONTRACT QUESTION') and not re.search(r'CQ-\d\d', disp):
            problems.append('AMB-%02d no CQ id' % n)
        if disp.startswith('RESOLVED'):
            v21 = set(re.findall(r'V2\.1 (\d+(?:\.\d+)?)', disp)) - FRAG_V21
            v22 = set(re.findall(r'V2\.2 ((?:PA|PW)-\d+|0\.\d)', disp)) - FRAG_V22
            other = re.search(r'ALT-21D proposal|AR[23]-\d\d', disp)
            if not (v21 or v22 or other):
                problems.append('AMB-%02d cites no clause outside the fragments' % n)
    record('BA06-C7', 'each of the 25 ambiguities is RESOLVED by a clause outside the compiler fragments, or is a CONTRACT QUESTION '
           'with an id of section 3.2.6', len(amb) == len(ambs) == 25 and not problems, '; '.join(problems) or 'none')

    # BA06-C8: the fragments are verbatim contract text
    fr = text(DL + 'sa1-contract-fragments.md')
    parts = re.split(r'<!-- FRAGMENT: (.*?) -->\n', fr)
    contract = {k: text(v) for k, v in CONTRACT.items()}
    missing = []
    for i in range(1, len(parts), 2):
        body_ = re.sub(r'\n---\s*$', '', parts[i + 1].rstrip('\n')).strip('\n')
        if body_ not in contract[parts[i].split()[0]]:
            missing.append(parts[i])
    record('BA06-C8', 'every fragment of sa1-contract-fragments.md is contained verbatim in the current V2.1, V2.2 or V2.4 text',
           len(parts) // 2 == 8 and not missing, '%d fragments; not found: %s' % (len(parts) // 2, missing))

    # BA06-C9: evidence hashes of section 14
    _, ev = table('| Path | SHA-256 | Version |')
    wrong = []
    for r in ev:
        p = r[0].strip('`')
        if p.endswith(OUTPUT_NAME):
            continue
        if not os.path.exists(path(p)) or sha256(raw(p)) != r[1].strip('`'):
            wrong.append(p)
    new6 = sorted(r[0].strip('`') for r in ev if r[2] == 'new in V6')
    record('BA06-C9', 'every SHA-256 of section 14, except the row of this output, equals the SHA-256 of the LF-normalized file; '
           '32 rows, 4 of them new in V6 (the blind recompile, the Gate 2 evidence, this script and its output)',
           len(ev) == 32 and not wrong and new6 == sorted([RECOMPILE, GATE2, SELF, 'docs/automation/evidence/' + OUTPUT_NAME]),
           'rows %d; mismatches %s; new in V6 %s' % (len(ev), wrong, new6))

    # BA06-C10: task inputs (BA06V3-N06), recomputed from repository files (unchanged from V5)
    ti = load(TI)

    def body(t):
        rest = t[t.index(MARK) + len(MARK):]
        return '\n'.join(l[2:] if l.startswith('  ') else l for l in rest.split('\n'))

    tasks = {t['LABEL']: t for w in ti['WORKFLOWS'] for t in w['TASKS']}
    hash_ok = all(hashlib.sha256(t['TASK_TEXT'].encode('utf-8')).hexdigest() == t['TASK_TEXT_SHA256'] and
                  hashlib.sha256(body(t['TASK_TEXT']).encode('utf-8')).hexdigest() == t['TASK_BODY_SHA256']
                  for t in tasks.values())
    dec = json.JSONDecoder()
    crit = body(tasks['critic:op-reviews']['TASK_TEXT'])
    ra, _ = dec.raw_decode(crit, crit.index('REVIEW A: ') + len('REVIEW A: '))
    rb, _ = dec.raw_decode(crit, crit.index('REVIEW B: ') + len('REVIEW B: '))
    cc = body(tasks['critic:census']['TASK_TEXT'])
    key = 'The census entries reported so far follow as JSON: '
    emb, _ = dec.raw_decode(cc, cc.index(key) + len(key))
    exp = [{'key': k, 'entries': load(SA + f).get('entries', []), 'scope': load(SA + f).get('scope')}
           for k, f in (('B2a', 'B2a-census-domain.json'), ('B2b', 'B2b-census-application.json'),
                        ('B2c', 'B2c-census-plugin-drawing-systems.json'), ('B2d', 'B2d-census-plugin-rest.json'))]
    prompts = load(DL + 'prompts.json')
    compact = json.dumps(load(DL + 'sa1-compiled-operations.json'), ensure_ascii=False, separators=(',', ':'))
    sub = {
        'hashes of the 15 task texts and bodies': hash_ok and len(tasks) == 15,
        'critic REVIEW A = B1a': ra == load(SA + 'B1a-blind-op-review-call-graph.json'),
        'critic REVIEW B = B1b': rb == load(SA + 'B1b-blind-op-review-api-sweep.json'),
        'census critic input = B2a..B2d entries': emb == exp,
        'compiler body = prompts.json compile': body(tasks['sa1-compile (contract only)']['TASK_TEXT']) == prompts['compile'],
        'comparator body = compare_template with LIST 1': body(tasks['sa1-compare (vs T3 and R-list)']['TASK_TEXT']) ==
        prompts['compare_template'].replace('"<LIST 1 JSON>"', compact),
        'all checks recorded at extraction PASS': all(c['RESULT'] == 'PASS' for c in ti['CHECKS']),
    }
    record('BA06-C10', 'the published V4 task inputs are internally consistent and reproduce, from repository files, the checks '
           'TI-CRIT-A, TI-CRIT-B, TI-CRIT-CENSUS, TI-COMPILE and TI-LIST1', all(sub.values()),
           '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub.items()))

    # BA06-C11: section 5 cites no superseded R-nn
    s5 = md[md.index('## 5. Residual facts'):md.index('## 6. Mutable static-state census')]
    rr = sorted(set(re.findall(r'\bR-\d\d\b', s5)))
    record('BA06-C11', 'the residual table of section 5 cites no row of the superseded V2 R-list', not rr, 'found %s' % rr)

    # BA06-C12: header identity (V6)
    hdr = md[:md.index('## 1.')]
    b5 = raw(V5)
    v5_blob = blob(b5)
    hdr1 = re.sub(r'\n>\s+', ' ', hdr)
    checks12 = {
        'ARTIFACT_VERSION cites the git blob of the V5 file':
            ('ARTIFACT_VERSION          = 6-DRAFT (supersedes 5-DRAFT, blob %s)' % v5_blob) in hdr,
        'AUTHORITY_CONTRACT': 'AUTHORITY_CONTRACT        = CT-21D V2.1..V2.5 (V2.4 rev. 3 and V2.5 rev. 2 textually verified) + '
                              'V2.6 (draft, pending review);' in hdr,
        'BOUND_PRODUCT_SHA': ('BOUND_PRODUCT_SHA         = %s (AR6-01' % BOUND) in hdr,
        'DELTA RULING APPLIED names sections 223 (AR6-01..AR6-07) and 220':
            'DELTA RULING APPLIED      = decisions section 223 (AR6-01, AR6-02, AR6-03, AR6-04, AR6-05, AR6-06, AR6-07;' in hdr
            and 'and section 220 (AR5-01, AR5-02, AR5-03,' in hdr1,
        'DEPENDS_ON = BA-01, BA-03, BA-05, BA-07': 'DEPENDS_ON                = BA-01, BA-03, BA-05, BA-07 (registry entries only;' in hdr,
        'STATUS AUTHORITY cites the BA-11 registry entry': 'the BA-11 registry entry of this artifact is the only authority' in hdr,
        'I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED': 'I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED' in hdr,
        'no CANDIDATE_HASH or SEAL_HASH': 'CANDIDATE_HASH' not in hdr and 'SEAL_HASH' not in hdr,
        'last section is Delta V6 (decisions 220 and 223)':
            [l for l in lines if l.startswith('## ')][-1] == '## 19. Delta V6 (decisions 220 and 223)',
    }
    record('BA06-C12', 'the header cites the git blob of the V5 file, the contract chain with V2.6 pending review, BOUND_PRODUCT_SHA '
           '95690c28, sections 223 and 220, DEPENDS_ON = BA-01, BA-03, BA-05, BA-07, the BA-11 registry entry and the naming state, has '
           'no CANDIDATE_HASH or SEAL_HASH field, and the artifact ends with the Delta V6 section', all(checks12.values()),
           'V5 blob %s; ' % v5_blob + '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in checks12.items()))

    # BA06-C13: the V5 file is the one AR5-01 carried
    dec_text = text(DECISIONS)
    s220 = dec_text[dec_text.index('\n## 220. '):dec_text.index('\n## 221. ')]
    s223 = dec_text[dec_text.index('\n## 223. '):dec_text.index('\n## 224. ')]
    ar501 = [l for l in s220.split('\n') if l.startswith('| AR5-01 |')]
    m501 = re.search(r'blob `([0-9a-f]{7,40})`', ar501[0]) if len(ar501) == 1 else None
    record('BA06-C13', 'the V5 file is the one AR5-01 carried STATIC_REVIEWED to (its git blob starts with the blob that the AR5-01 '
           'row of decisions section 220 states) and the one that the V6 header supersedes',
           m501 is not None and v5_blob.startswith(m501.group(1)) and checks12['ARTIFACT_VERSION cites the git blob of the V5 file'],
           'V5 blob %s; AR5-01 blob %s' % (v5_blob, m501.group(1) if m501 else None))

    # ------------------------------------------------------------------------------------------ BA06-C14 frozen-parts check
    rep14 = {'LABELS': [{'V5': a, 'V6': b, 'KIND': 'LABEL'} for a, b in LABELS], 'TABLES': {}, 'LINES': {}, 'ADDED_SUBSECTIONS': []}
    problems14 = []
    seen_decl = set()
    tables = [
        ('3.1', '| id | category | api_call | location | seam_role | transaction_context | found_by |', 'key'),
        ('3.2', '| id | actor | kind | operation | clause fragments | R-list (V2) | class |', 'key'),
        ('3.2.2', '| Id | Difference | Resolution (comparator) |', 'key'),
        ('3.2.5', '| id | actor | kind | operation | clause (outside the fragments) | origin | class |', 'key'),
        ('3.3', '| Class | Operations | Actor and phase |', 'key'),
        ('5', '| residual | affects | why not statically visible |', 'index'),
        ('7 sources', '| Source | Role in the trace | Why |', 'key'),
        ('7 matrix', '| Entry point | SH-F1 |', 'key'),
        ('10.2', '| Class | Description | Fixture (BA-08', 'key'),
        ('11', '| `q` | Quantity | Source |', 'key'),
        ('12', '| Field | Content |', 'key'),
        ('4.1', '| Id | Topic | BA-06 V1 inventory | Independent review | Resolution |', 'key'),
        ('4.2', '| V1 item | V2/V3/V4 counterpart | Disposition |', 'key'),
    ]
    for part, hstart, mode in tables:
        h5, a = table_in(lines5, hstart)
        h6, b = table_in(lines, hstart)
        head_change = None if h5 == h6 else ('LABEL' if [relabel(x) for x in h5] == h6 else 'UNDECLARED')
        if head_change == 'UNDECLARED':
            problems14.append('%s header changed' % part)
        rows_rep = []
        if mode == 'index':
            if len(a) != len(b):
                problems14.append('%s row count %d -> %d' % (part, len(a), len(b)))
            pairs = [(i + 1, x, y) for i, (x, y) in enumerate(zip(a, b))]
        else:
            ka = {relabel(r[0]): r for r in a}
            kb = {r[0]: r for r in b}
            order = [relabel(r[0]) for r in a] + [r[0] for r in b if r[0] not in ka]
            pairs = [(k, ka.get(k), kb.get(k)) for k in order]
        for k, x, y in pairs:
            if x is not None and y is not None and x == y:
                continue
            if x is not None and y is not None and [relabel(c) for c in x] == y:
                rows_rep.append({'ROW': k, 'CHANGE': 'LABEL_ONLY', 'KIND': 'LABEL'})
                continue
            d = DECL_ROWS.get((part, k))
            status = 'ADDED' if x is None else ('REMOVED' if y is None else 'CHANGED')
            entry = {'ROW': k, 'CHANGE': status, 'KIND': d[0] if d else 'UNDECLARED', 'RULING': d[1] if d else None}
            if status == 'CHANGED':
                entry['CHANGED_CELLS'] = [{'COLUMN': ci + 1, 'EDITS': edits(relabel(p), q)}
                                          for ci, (p, q) in enumerate(zip(x, y)) if relabel(p) != q]
                if part == '3.3' and d and d[0] == 'NON_CONTENT_RULING_ORDERED' and [c['COLUMN'] for c in entry['CHANGED_CELLS']] != [5]:
                    problems14.append('3.3 %s: a non-content edit outside the fixture cell' % k)
                if part == '10.2' and d and d[0] == 'NON_CONTENT_RULING_ORDERED' and [c['COLUMN'] for c in entry['CHANGED_CELLS']] != [2]:
                    problems14.append('10.2 %s: a non-content edit outside the description cell' % k)
            if d is None or status == 'REMOVED':
                problems14.append('%s %s %s undeclared or removed' % (part, k, status))
            seen_decl.add((part, k))
            rows_rep.append(entry)
        rep14['TABLES'][part] = {'HEADER_CHANGE': head_change, 'ROWS_V5': len(a), 'ROWS_V6': len(b), 'CHANGED_ROWS': rows_rep}
    for dk in DECL_ROWS:
        if dk not in seen_decl:
            problems14.append('declared change did not happen: %s %s' % dk)
    # non-table lines of the frozen spans (3 to 3.2.5 without 3.1.1; 3.3 without 3.4; 7; 10.2; 11) and section 5 paragraph
    spans = [('3', '## 3. ', '#### 3.2.6 '), ('3.3', '### 3.3 ', '## 4. '), ('7', '## 7. ', '## 8. '), ('10.2', '### 10.2 ', '### 10.3 '),
             ('11', '## 11. ', '## 12. '), ('5', '## 5. ', '## 6. '), ('12', '## 12. ', '## 13. ')]
    decl_lines_seen = set()
    for part, st, en in spans:
        _, l5 = span(lines5, st, en)
        _, l6 = span(lines, st, en)
        cut = []
        for ex in EXCLUDED_SUBSECTIONS:
            ks = [k for k, l in enumerate(l6) if l.startswith(ex)]
            if ks:
                k0 = ks[0]
                k1 = next((k for k in range(k0 + 1, len(l6)) if l6[k].startswith('#')), len(l6))
                rep14['ADDED_SUBSECTIONS'].append({'PART': part, 'HEADING': l6[k0], 'LINES': k1 - k0, 'KIND': 'RECORD',
                                                   'RULING': 'AR6-03 (a record of the review, not an inventory row)'})
                cut.append((k0, k1))
        l6k = [l for k, l in enumerate(l6) if not any(a_ <= k < b_ for a_, b_ in cut)]
        n5 = [relabel(l) for l in l5 if not l.startswith('|')]
        n6 = [l for l in l6k if not l.startswith('|')]
        sm = difflib.SequenceMatcher(None, n5, n6, autojunk=False)
        ch = []
        for op, a1, a2, b1, b2 in sm.get_opcodes():
            if op == 'equal':
                continue
            for l in n6[b1:b2]:
                if l == '':
                    continue
                d = [x for x in DECL_LINES if x[0] == part and l.startswith(x[1])]
                if len(d) != 1:
                    problems14.append('%s: undeclared line %s' % (part, l[:80]))
                    ch.append({'OP': op.upper(), 'V6_LINE_START': l[:100], 'KIND': 'UNDECLARED'})
                    continue
                decl_lines_seen.add(d[0][1])
                old = n5[a1:a2]
                ch.append({'OP': op.upper(), 'V6_LINE_START': l[:100], 'KIND': d[0][2], 'RULING': d[0][3],
                           'EDITS': edits(old[0], l) if len(old) == 1 else [{'OP': 'INSERT', 'V5': '', 'V6': l}]})
            if op == 'delete':
                problems14.append('%s: lines removed %s' % (part, [x[:60] for x in n5[a1:a2]]))
        rep14['LINES'][part] = ch
    for x in DECL_LINES:
        if x[1] not in decl_lines_seen:
            problems14.append('declared line change did not happen: %s' % x[1][:60])
    kinds = {}
    for part, t_ in rep14['TABLES'].items():
        for e in t_['CHANGED_ROWS']:
            kinds.setdefault(e['KIND'], []).append('%s %s' % (part, e['ROW']))
    for part, ch in rep14['LINES'].items():
        for e in ch:
            kinds.setdefault(e['KIND'], []).append('%s line "%s"' % (part, e['V6_LINE_START'][:40]))
    rep14['SUMMARY_BY_KIND'] = kinds
    rep14['PROBLEMS'] = problems14
    record('BA06-C14', 'FROZEN-PARTS CHECK (AR5-02, AR6-03): V6 differs from V5, over the inventory (3.1, 3.2, 3.2.5, 3.3, section 5 '
           'rows and "affects" cells, section 12 fields), the trace plan (section 7, 10.2, section 11) and the review records inside '
           'those spans (3.2.2, 4.1, 4.2, non-table lines), exactly by the declared changes, each with its kind (CONTENT, '
           'NON_CONTENT_RULING_ORDERED, RECORD, LABEL) and its ruling; the REPORT lists every changed row and line with its cell '
           'edits, and the new record subsections 3.1.1 and 3.4',
           not problems14, 'CONTENT %d, NON_CONTENT_RULING_ORDERED %d, RECORD %d, LABEL %d; problems %s' % (
               len(kinds.get('CONTENT', [])), len(kinds.get('NON_CONTENT_RULING_ORDERED', [])), len(kinds.get('RECORD', [])),
               len(kinds.get('LABEL', [])), problems14), rep14)

    # BA06-C15: declared limit (4) unchanged from V5
    lim6 = [l for l in lines if l.startswith('**Declared limits (condition 2).**')]
    lim5 = [l for l in lines5 if l.startswith('**Declared limits (condition 2).**')]
    lim4 = lim6[0][lim6[0].index(LIMIT4_START):] if len(lim6) == 1 and LIMIT4_START in lim6[0] else ''
    record('BA06-C15', 'section 7 declared limit (4) is present with the four elements of AR4-12 condition (1), and the '
           'declared-limits paragraph equals the V5 paragraph after the version labels',
           all(p in lim4 for p in LIMIT4_PHRASES) and len(lim5) == 1 and relabel(lim5[0]) == lim6[0],
           '; '.join('%s: %s' % (p, p in lim4) for p in LIMIT4_PHRASES))

    # BA06-C16: the recompile mapping of section 4.4 is a partition
    rec = load(RECOMPILE)
    ops = {o['id']: o for o in rec['operations']}
    _, mrows44 = table('| V6 row or disposition | R1 | R2 | R3 | Operations |')

    def expand(cell):
        out = []
        if cell == '-':
            return out
        for part_ in cell.split(', '):
            if '..' in part_:
                a_, b_ = part_.split('..')
                out += ['%s-%03d' % (a_[:2], n) for n in range(int(a_[3:]), int(b_[3:]) + 1)]
            else:
                out.append(part_)
        return out

    assigned = {}
    dup = []
    count_bad = []
    for r in mrows44:
        ids = expand(r[1]) + expand(r[2]) + expand(r[3])
        if len(ids) != int(r[4]):
            count_bad.append(r[0][:10])
        for i in ids:
            if i in assigned:
                dup.append(i)
            assigned[i] = r[0].split(':')[0]
    _, u31 = table('| id | category | api_call | location | seam_role | transaction_context | found_by |')
    uloc = {r[0]: r[3] for r in u31}
    aggregate = {'U-39', 'U-41', 'U-50'}
    file_bad, mut_bad = [], []
    for i, u in assigned.items():
        o = ops.get(i)
        if o is None:
            continue
        if o['mutating'] == 'MUTATING' and not u.startswith('U-'):
            mut_bad.append(i)
        if u.startswith('U-') and u not in aggregate:
            base = o['file_line'].split(':')[0].split('/')[-1]
            if base not in uloc.get(u, ''):
                file_bad.append('%s->%s (%s)' % (i, u, base))
    not_assigned = sorted(set(ops) - set(assigned))
    extra = sorted(set(assigned) - set(ops))
    hdr_ok = rec['header']['source_sha'] == BOUND and 'BA-06 V5 section 3 and the Gate 2 assessment were NOT read' in \
        rec['header']['inputs_read']
    record('BA06-C16', 'section 4.4 assigns each of the 689 operations of the blind recompile (source SHA 95690c28, which did not '
           'read BA-06 V5 section 3 or the Gate 2 assessment) to exactly one V6 row or to ND; every MUTATING operation goes to a '
           'U-row; for every non-aggregate U-row, the file of each assigned operation is cited in the row\'s location cell; the '
           'counts column is exact', hdr_ok and len(ops) == 689 and not not_assigned and not extra and not dup and not mut_bad and
           not file_bad and not count_bad,
           'header %s; unassigned %s; unknown %s; duplicates %s; MUTATING not on a U-row %s; file not cited %s; count mismatch %s' %
           (hdr_ok, not_assigned[:10], extra[:10], dup[:10], mut_bad[:10], file_bad[:10], count_bad))

    # BA06-C17: the ledger classifications equal the Gate 2 evidence
    g2raw = raw(GATE2)
    g2sha = re.search(r'I-52-ct21d-gate2-relink-assessment\.json` \(SHA-256 `([0-9a-f]{64})`\)', s223)
    g2 = json.loads(g2raw.decode('utf-8'))
    facts = {f['fact'].split(' ')[0]: f['classification'] for r in g2['reviews'] if r['key'] == 'RELINK-0605'
             for f in r['facts'] if f['artifact'] == 'BA-06 V5' and re.match(r'(U|N)-\d\d', f['fact'])}
    _, led = table('| Row | Gate 2 classification | First cited file |')
    bad17 = []
    for r in led:
        m = re.match(r'([A-Z_]+)(?: \(as (N-\d\d)\))?$', r[1])
        k = m.group(2) if m and m.group(2) else r[0]
        if not m or facts.get(k) != m.group(1):
            bad17.append(r[0])
    record('BA06-C17', 'the Gate 2 evidence has the SHA-256 that decisions section 223 states, and every row of the relink ledger '
           '3.1.1 (U-01..U-50 and U-26x) carries exactly the classification that the evidence gives to that fact (new rows by their '
           'Gate 2 id N-01..N-08)', g2sha is not None and g2sha.group(1) == sha256(g2raw) and len(led) == len(u31) == 51 and
           [r[0] for r in led] == [r[0] for r in u31] and not bad17,
           'stated %s; rows %d; mismatches %s' % (g2sha.group(1) if g2sha else None, len(led), bad17))

    # BA06-C18: the carry table recomputed; carry candidates = AR6-03
    ar603 = [l for l in s223.split('\n') if l.startswith('| AR6-03 |')][0]
    carry603 = sorted(re.findall(r'`(OC-[A-Z-]+)`', ar603[ar603.index('candidatas:'):]))
    _, ct = table('| Class | Members (V6) | Changed members vs V5 | Membership change |')
    changed_ids = set()
    for part in ('3.1', '3.2', '3.2.5'):
        for e in rep14['TABLES'][part]['CHANGED_ROWS']:
            if e['KIND'] != 'LABEL':
                changed_ids.add(e['ROW'])
    bad18 = []
    for r_oc, r_ct in zip(octab, ct):
        if r_oc[0] != r_ct[0]:
            bad18.append('order %s' % r_oc[0])
            continue
        mem = re.findall(r'\b([UCX]-\d+x?)\b', re.sub(r'\([^)]*\)', '', r_oc[1]))
        if ', '.join(mem) != r_ct[1]:
            bad18.append('members %s' % r_oc[0])
        chg = [m for m in mem if m in changed_ids]
        if (', '.join(chg) or 'none') != r_ct[2]:
            bad18.append('changed %s' % r_oc[0])
        cls_rows = [e for e in rep14['TABLES']['3.3']['CHANGED_ROWS'] if e['ROW'] == r_oc[0]]
        content_change = bool(chg) or any(e['KIND'] == 'CONTENT' for e in cls_rows)
        carry = r_ct[5].startswith('CARRY CANDIDATE')
        if carry == content_change:
            bad18.append('status %s' % r_oc[0])
    cands = sorted(r[0].strip('`') for r in ct if r[5].startswith('CARRY CANDIDATE'))
    others = {r[0].strip('`'): r[5] for r in ct if not r[5].startswith('CARRY CANDIDATE')}
    want_others = {'OC-IMPORT': 'FULL RE-REVIEW', 'OC-REF-TOP': 'CONFIRMATION', 'OC-ENVELOPE': 'CONFIRMATION',
                   'OC-PREP-READ': 'RE-REVIEW', 'OC-VERIFY-READ': 'BOUNDED RE-REVIEW'}
    ok_others = sorted(others) == sorted(want_others) and all(others[k].startswith(v) for k, v in want_others.items())
    record('BA06-C18', 'the carry table 3.4 lists the classes of 3.3 in order with their members, the changed members computed by '
           'BA06-C14 and a status consistent with them (a carry candidate has no CONTENT change); the carry candidates are exactly '
           'the 16 classes that AR6-03 lists; OC-IMPORT full re-review, OC-REF-TOP and OC-ENVELOPE confirmation, OC-PREP-READ and '
           'OC-VERIFY-READ re-review', not bad18 and cands == carry603 and len(cands) == 16 and ok_others,
           'candidates %s; AR6-03 %s; others %s; problems %s' % (cands, carry603, others, bad18))

    # BA06-C19: the identity table of section 2.1 against the checkout
    _, idt = table('| Path | Role | blob at `3375aadb` | blob at `95690c28` | Relation |')
    bad19 = []
    for r in idt:
        p = r[0].strip('`')
        if not os.path.exists(path(p)) or blob(raw(p)) != r[3].strip('`'):
            bad19.append(p)
        rel = r[4]
        a_ = r[2].strip('`')
        if (a_ == '-' and not rel.startswith('NEW')) or (a_ != '-' and rel != ('IDENTICAL' if a_ == r[3].strip('`') else 'CHANGED')):
            bad19.append('relation ' + p)
    record('BA06-C19', 'every file of the identity table of section 2.1 has, in the checkout, the git blob of its 95690c28 column '
           '(so the checkout\'s src equals 95690c28 for those paths), and the Relation column agrees with the two blob columns',
           len(idt) >= 40 and not bad19, 'rows %d; mismatches %s' % (len(idt), bad19))

    # BA06-C20: AR5-03 corrections
    s325 = {r[0]: r for r in xrows}
    dc = {r[0]: r for r in dcrows}
    ocr = {r[0].strip('`'): r for r in octab}

    def mem_of(c):
        return re.findall(r'\b([UCX]-\d+x?)\b', re.sub(r'\([^)]*\)', '', ocr[c][1]))

    sub20 = {
        'C-72 marked amendment': 'V6 amendment (AR5-03; R1:BA06V4-N01)' in by_id['C-72'][3] and by_id['C-72'][6] == 'OC-PREP-READ',
        'DC-41 aligned': 'aligned with the marked amendment of C-72' in dc['DC-41'][5],
        'DC-18 corrected': 'V6 correction (AR5-03; R2:BA06V4-N01)' in dc['DC-18'][5] and 'pass 1 and pass 2' in dc['DC-18'][5] and
        'V2.1 15.7 has no such clause' not in dc['DC-18'][5],
        'X-05 row, class OC-VERIFY-READ': 'X-05' in s325 and s325['X-05'][6] == 'OC-VERIFY-READ' and 'V2.1 10.1' in s325['X-05'][4],
        'U-09 in OC-PREP-READ, not in OC-IMPORT': 'U-09' in mem_of('OC-PREP-READ') and 'U-09' not in mem_of('OC-IMPORT'),
        'U-03 stays in OC-IMPORT': 'U-03' in mem_of('OC-IMPORT') and 'U-03' not in mem_of('OC-PREP-READ'),
        'X-05 in OC-VERIFY-READ': 'X-05' in mem_of('OC-VERIFY-READ'),
    }
    record('BA06-C20', 'the AR5-03 corrections are applied: C-72 amendment, DC-41 aligned, DC-18 corrected, row X-05 (class '
           'OC-VERIFY-READ), U-09 moved to OC-PREP-READ, U-03 kept in OC-IMPORT', all(sub20.values()),
           '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub20.items()))

    # history span (sections 15, 16, 16.1, 17)
    i15, l15 = span(lines, '## 15. ', '## 18. ')
    hist = set(range(i15, i15 + len(l15)))
    s8 = md[md.index('## 8. `RackDefinitionCreator`'):md.index('## 9. Learning-corpus definition')]
    _, d41 = table('| Id | Topic | BA-06 V1 inventory | Independent review | Resolution |')
    dd = {r[0]: r for r in d41}
    npc = [k + 1 for k, l in enumerate(lines) if k not in hist and 'no production caller' in l and 'superseded' not in l and
           'AppendReference' not in l and 'RackSiblingInsertRun' not in l]
    sub21 = {
        'section 8: the V5 sentence superseded, RACKPROYECTAR a precedent, P-8': 'is **superseded at `95690c28`**' in s8 and
        'RACKPROYECTAR is a **precedent, not an entry of the reused path**' in s8 and 'P-8 remains required' in s8 and
        'AR3-12' in s8,
        'D-01 excludes RACKPROYECTAR with the reason': 'RACKPROYECTAR/RPY is **not** an entry of the reused path' in dd['D-01'][4]
        and 'does not create the new logical rack that RACKMIRROR reuses' in dd['D-01'][4],
        'D-25 precedent only': 'PRECEDENT ONLY (AR6-04)' in dd['D-25'][4],
        'no other line says "no production caller" of AUTH-15': not npc,
        'no 3.1 row cites RACKPROYECTAR files': not any('RackProjection' in r[3] or 'RackProyectar' in r[3] for r in u31),
    }
    record('BA06-C21', 'AR6-04: section 8 and D-01 record RACKPROYECTAR as a precedent only with the exclusion reason; the V5 '
           'sentence "no production caller" is superseded; P-8 stays required; no inventory row cites the RACKPROYECTAR files',
           all(sub21.values()), '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub21.items()) +
           '; lines %s' % npc)

    # BA06-C22: AR6-02 wording
    s7 = md[md.index('## 7. Dynamic-trace plan'):md.index('## 8. ')]
    sh = [r for r in shapes if r[0] == '`SH-LAY-ADDED`'][0]
    pr = [k + 1 for k, l in enumerate(lines) if k not in hist and 'product-reachable' in l]
    sub22 = {
        '3.3 OC-LAYER fixture cell': 'legacy-product-state construction' in ocr['OC-LAYER'][4] and 'AR6-02' in ocr['OC-LAYER'][4],
        'section 7 coverage-matrix sentence': 'a legacy-product-state fixture built by the declared `FIXTURE_CONSTRUCTION_BUILD`' in s7,
        '10.2 SH-LAY-ADDED': '**legacy product state**' in sh[1] and FIXTURE_BUILD in sh[1] and 'FXB-01..FXB-07' in sh[1] and
        sh[2] == '`FX-ANN-BLANK`',
        'no "product-reachable" outside the history': not pr,
    }
    record('BA06-C22', 'AR6-02: 3.3, section 7 and 10.2 state FX-ANN-BLANK as a legacy product state built by the declared '
           'FIXTURE_CONSTRUCTION_BUILD 69daf03a; the fixture cell stays FX-ANN-BLANK; no "product-reachable" outside the history',
           all(sub22.values()), '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub22.items()) + '; lines %s' % pr)

    # BA06-C23: naming
    u46 = [r for r in u31 if r[0] == 'U-46'][0]
    sub23 = {
        'header': 'I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED' in hdr,
        'U-46: the mirror orchestrator does not call it': 'The mirror orchestrator does not call it' in u46[4] and
        'NOT_YET_ADOPTED' in u46[4] and 'not a code fact' in u46[4],
        '10.2 mirror name kept': '"<base> - espejo" (V17, never blank; `I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED`)' in sh[1],
        'no mirror named Selectivo N': not re.search(r'mirror[^.|]{0,40}Selectivo N', md),
    }
    record('BA06-C23', 'the naming state: I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED in the header; U-46 records the I-60 read as '
           'a product fact that the mirror orchestrator does not call; the mirror name "<base> - espejo" is kept',
           all(sub23.values()), '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub23.items()))

    # BA06-C24: version labels
    stale = [k + 1 for k, l in enumerate(lines) if k not in hist and re.search(r'BA-03 V4|BA-(0[4789]|10) V5|BA-11 V\d', l)]
    record('BA06-C24', 'outside the history of sections 15 to 17, no stale sibling label (BA-03 V4; BA-04, BA-07, BA-08, BA-09, '
           'BA-10 V5) and no BA-11 version label remain (BA-11 is cited by registry entry)', not stale, 'lines %s' % stale)

    out = {
        'CHECKER': SELF,
        'CHECKER_SHA256': sha256(raw(SELF)),
        'ARTIFACT': ART,
        'COMPARED_WITH': V5,
        'COMPARED_WITH_BLOB': v5_blob,
        'NOTE': 'no hash of the V6 artifact is recorded here: the artifact binds this output by hash (section 14)',
        'RESULTS': results,
        'ALL_PASS': all(r['RESULT'] == 'PASS' for r in results),
    }
    data = json.dumps(out, ensure_ascii=False, indent=1) + '\n'
    io.open(out_path, 'w', encoding='utf-8', newline='\n').write(data)
    for r in results:
        print(r['ID'], r['RESULT'], r['DETAIL'][:300])
    return 0 if out['ALL_PASS'] else 1


if __name__ == '__main__':
    sys.exit(main())
