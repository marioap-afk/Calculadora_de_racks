"""I-52 CT-21D baseline artifact BA-06 V7: reproducible checks of the statements that BA-06 V7 makes about itself, and the
exact delta check of the V7 bytes against the V6 bytes that AR7-01 asks for (decisions section 226: AR7-01, AR7-02, AR7-09).

Usage: python I-52-ct21d-ba06-v7-checks.py <repository root> <output file>

Reads only repository files (the V7, V6 and V5 artifacts, the decisions file, the BA-11 V6 registry, the V6 architect review
evidence, the Gate 2 evidence, the blind recompile, the V6 check output, the contract texts, the evidence files the artifact binds,
and, for BA06-C19 only, the source files of the checkout) and writes one JSON result file. Fails closed: a missing input, a table or
section that cannot be parsed, or any failed check gives exit code 1. The output records no hash of the V7 artifact, because the V7
artifact binds this output by hash (section 14). Deterministic: no clock, no network. Hashes: SHA-256 of the LF-normalized bytes,
lowercase hexadecimal; git blob ids: SHA-1 of "blob <size>\\0" + LF-normalized bytes.

BA06-C1 to BA06-C11 repeat the V6 checks on the V7 bytes (C9 with 34 evidence rows, 2 new in V7). BA06-C12: the V7 header.
BA06-C13: the V6 bytes are the ones the BA-11 V6 registry records for BA-06 and the ones the V7 header supersedes; the V6 architect
review evidence has the SHA-256 that decisions section 226 states. BA06-C14 is the EXACT DELTA CHECK of AR7-01: it compares V7 with V6
over the whole file (the header fields, every table row, every other line, every section), lists every change with its kind
(CONTENT, RECORD) and its ruling, and fails unless the set of changes equals the declared set (BA06V6-01..04 and AR7-02); it also
asserts that the only CONTENT changes are the U-45 row of 3.1 (seam role) and the OC-IMPORT row of 3.3. BA06-C15: the declared-limits
paragraph of section 7 equals V6. BA06-C16: the mapping of section 4.4 is a partition of the 689 operations of the blind recompile
(R3-001 and R3-009 in ND, BA06V6-03), and every row carries its classes of 3.3 or a disposition, the MUTATING-carrying rows included
(BA06V6-04). BA06-C17: the relink ledger against the Gate 2 evidence. BA06-C18: the carry table 3.4 recomputed from the V5 and V6
bytes (changed member rows, class rows and section-5 rows, cross-checked with the bound V6 check output) and the V7 membership, with
the section-5 column of BA06V6-01. BA06-C19: the identity table of section 2.1 against the checkout. BA06-C20 to BA06-C24 repeat the
V6 checks of the AR5-03 corrections, AR6-04, the AR6-02 wording, the naming state and the version labels. BA06-C25: the AR7-02
membership of OC-IMPORT, its call sites and the U-48 watch item.
"""
import difflib
import hashlib
import io
import json
import os
import re
import sys

ART = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v7.md'
V6 = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v6.md'
V5 = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v5.md'
SELF = 'docs/automation/evidence/I-52-ct21d-ba06-v7-checks.py'
OUTPUT_NAME = 'I-52-ct21d-ba06-v7-checks-output.json'
V6_OUTPUT = 'docs/automation/evidence/I-52-ct21d-ba06-v6-checks-output.json'
BA11V6 = 'docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v6.md'
REVIEW = 'docs/automation/evidence/I-52-ct21d-relink-v6-architect-review.json'
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
TOK = re.compile(r'\w+|\s+|[^\w\s]')
# the V6 version-label substitutions (V5 -> V6), used only to recompute the V5 -> V6 comparison of BA06-C18
LABELS6 = [('BA-03 V4', 'BA-03 V5'), ('BA-04 V5', 'BA-04 V6'), ('BA-07 V5', 'BA-07 V6'), ('BA-08 V5', 'BA-08 V6'),
           ('BA-09 V5', 'BA-09 V6'), ('BA-10 V5', 'BA-10 V6')]
OC_IMPORT_AR702 = ['U-03', 'U-07', 'U-08', 'C-81', 'C-82']

# ---------------------------------------------------------------------------------------------- BA06-C14 declarations
# The declared set of changes of V7 against V6 (AR7-01: only BA06V6-01..04 and AR7-02). Kinds: CONTENT (an inventory row of
# 3.1 or a class row of 3.3, AR5-02) and RECORD (a record of the review or of the status, not an inventory or trace-plan row).
H33, H34, H44, H13, H14, H18 = ('### 3.3 ', '### 3.4 ', '### 4.4 ', '## 13. ', '## 14. ', '## 18. ')
DECL_HEADER = {
    'TITLE': 'AR7-01: version label V7',
    'BANNER': 'AR7-01: version label V7',
    'ARTIFACT_VERSION': 'AR7-01: 7-DRAFT, supersedes the V6 blob',
    'DELTA RULING APPLIED': 'decisions sections 226 (AR7-01, AR7-02, AR7-09) and 227 added',
    'PREREQUISITES': 'AR7-01: (1) the exact delta check against V6; AR7-02: (4) none open',
    'INVENTORY_REVIEW_STATUS': 'AR7-01: STATIC_REVIEWED approved on the V6 bytes by class; carried to V7 through the check',
}
# (heading prefix, table (first header cell) or None for a non-table line, key or '*' for every row of the table, allowed change,
#  kind, ruling, allowed changed columns (1-based, in the V7 row; None = any), added column (1-based) or None)
DECL = [
    ('### 3.1 ', 'id', 'U-45', 'CHANGED', 'CONTENT', 'BA06V6-02; AR7-02 (01): seam role = call site of OC-IMPORT, not a member',
     [5], None),
    ('#### 3.1.1 ', 'Row', 'U-41', 'CHANGED', 'RECORD', 'BA06V6-03: R3-001 and R3-009 are ND', [5, 6], None),
    (H33, 'Class', '`OC-IMPORT`', 'CHANGED', 'CONTENT', 'BA06V6-02; AR7-02 (01): members U-03, U-07, U-08, C-81, C-82; sites U-02 '
     'and U-45 in the class row; X-01 / C-89 context of the micro-scenario', [2, 3], None),
    (H33, None, '- **V7 (AR7-02; BA06V6-02).**', 'ADDED', 'RECORD', 'AR7-02 (01), (02): bullet on the V7 class record', None, None),
    (H34, None, 'The members are those of 3.3;', 'CHANGED', 'RECORD', 'BA06V6-01; AR7-01; AR7-02: intro of the carry table', None,
     None),
    (H34, 'Class', '<header>', 'CHANGED', 'RECORD', 'BA06V6-01: column 5 added; Members (V7)', None, 5),
    (H34, 'Class', '*', 'CHANGED', 'RECORD', 'BA06V6-01: the section-5 column only', [5], 5),
    (H34, 'Class', '`OC-IMPORT`', 'CHANGED', 'RECORD', 'BA06V6-01 and BA06V6-02 (AR7-02 (01)): members, changed members, '
     'membership change, section-5 column and reason', [2, 3, 4, 5, 6], 5),
    (H34, 'Class', '`OC-REF-TOP`', 'CHANGED', 'RECORD', 'BA06V6-01 and AR7-02 (02): section-5 column and reason (every entry; '
     'U-48 watch item)', [5, 6], 5),
    (H44, None, 'Source: `docs/automation/evidence/I-52-ct21d-ba06-v6-blind-recompile.json`', 'CHANGED', 'RECORD',
     'BA06V6-03, BA06V6-04: intro sentence', None, None),
    (H44, 'V6 row or disposition', '<header>', 'CHANGED', 'RECORD', 'BA06V6-04: column 6 added', None, 6),
    (H44, 'V6 row or disposition', '*', 'CHANGED', 'RECORD', 'BA06V6-04: the class-or-disposition column only', [6], 6),
    (H44, 'V6 row or disposition', 'U-41', 'CHANGED', 'RECORD', 'BA06V6-03 and BA06V6-04: R3-001 and R3-009 leave U-41',
     [4, 5, 6], 6),
    (H44, 'V6 row or disposition', 'ND: not a database operation (command entry, modal UI, prompt, catalog file read, pure '
     'step, dispatch, status mapping, message, exception report)', 'CHANGED', 'RECORD', 'BA06V6-03 and BA06V6-04: R3-001 and '
     'R3-009 join ND', [4, 5, 6], 6),
    (H13, 'Item', 'per-class `STATIC_REVIEWED`', 'CHANGED', 'RECORD', 'AR7-01: decision recorded', [2], None),
    (H13, 'Item', '`STATIC_REVIEWED`', 'CHANGED', 'RECORD', 'AR7-01: approval on the V6 bytes recorded', [2], None),
    (H13, 'Item', 'open Architect questions', 'CHANGED', 'RECORD', 'AR7-02: none open', [2], None),
    (H13, 'Item', 'V7 minors', 'ADDED', 'RECORD', 'AR7-09: BA06V6-01..04', None, None),
    (H14, None, 'The two files marked "new in V7"', 'CHANGED', 'RECORD', 'section 14 intro for V7', None, None),
    (H14, 'Path', '`docs/automation/evidence/I-52-ct21d-ba06-v6-checks.py`', 'CHANGED', 'RECORD', 'version cell: V6', [3], None),
    (H14, 'Path', '`docs/automation/evidence/I-52-ct21d-ba06-v6-checks-output.json`', 'CHANGED', 'RECORD', 'version cell: V6', [3],
     None),
    (H14, 'Path', '`docs/automation/evidence/I-52-ct21d-ba06-v7-checks.py`', 'ADDED', 'RECORD', 'new in V7', None, None),
    (H14, 'Path', '`docs/automation/evidence/I-52-ct21d-ba06-v7-checks-output.json`', 'ADDED', 'RECORD', 'new in V7', None, None),
    (H18, None, 'These questions needed an Architect ruling;', 'CHANGED', 'RECORD', 'AR7-02: rulings recorded', None, None),
] + [(H18, 'Id', 'AQ-V6-BA06-%02d' % n, 'CHANGED', 'RECORD', 'AR7-02 (%02d): ruling recorded' % n, [4], None) for n in (1, 2, 3, 4)]
DECL_SECTIONS = {'## 20. Delta V7 (decisions 226)': ('RECORD', 'the Delta V7 table (decisions 226)')}
CONTENT_EXPECTED = {('### 3.1 ', 'U-45'), (H33, '`OC-IMPORT`')}


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
    md6 = text(V6)
    lines6 = md6.split('\n')
    md5 = text(V5)
    lines5 = md5.split('\n')

    def split_row(l):
        return [c.strip() for c in l.strip()[2:-2].split(' | ')]

    def table_in(ls, header_start):
        idx = [i for i, l in enumerate(ls) if l.startswith(header_start)]
        if len(idx) != 1:
            raise SystemExit('table header not unique or missing: ' + header_start)
        i = idx[0]
        head = [c.strip() for c in ls[i].strip().strip('|').split(' | ')]
        rows = []
        j = i + 2
        while j < len(ls) and ls[j].startswith('|'):
            cells = split_row(ls[j])
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

    def relabel6(t):
        for a, b in LABELS6:
            t = t.replace(a, b)
        return t

    def edits(a, b):
        ta, tb = TOK.findall(a), TOK.findall(b)
        sm = difflib.SequenceMatcher(None, ta, tb, autojunk=False)
        return [{'OP': op.upper(), 'OLD': ''.join(ta[a1:a2]), 'NEW': ''.join(tb[b1:b2])}
                for op, a1, a2, b1, b2 in sm.get_opcodes() if op != 'equal']

    def members(cell):
        return re.findall(r'\b([UCX]-\d+x?)\b', re.sub(r'\([^)]*\)', '', cell))

    def ids_of(cell):
        out = set()
        for p in re.sub(r'\([^)]*\)', '', cell).split(','):
            p = p.strip()
            m = re.match(r'^([UCX])-(\d+)\.\.([UCX])-(\d+)$', p)
            if m:
                out |= set('%s-%02d' % (m.group(1), n) for n in range(int(m.group(2)), int(m.group(4)) + 1))
            elif re.match(r'^[UCX]-\d+x?$', p):
                out.add(p)
        return out

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
    new7 = sorted(r[0].strip('`') for r in ev if r[2] == 'new in V7')
    record('BA06-C9', 'every SHA-256 of section 14, except the row of this output, equals the SHA-256 of the LF-normalized file; '
           '34 rows, 2 of them new in V7 (this script and its output)',
           len(ev) == 34 and not wrong and new7 == sorted([SELF, 'docs/automation/evidence/' + OUTPUT_NAME]),
           'rows %d; mismatches %s; new in V7 %s' % (len(ev), wrong, new7))

    # BA06-C10: task inputs (BA06V3-N06), recomputed from repository files (unchanged from V6)
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

    # BA06-C12: header identity (V7)
    hdr = md[:md.index('## 1.')]
    b6 = raw(V6)
    v6_blob = blob(b6)
    hdr1 = re.sub(r'\n>\s+', ' ', hdr)
    checks12 = {
        'title and banner V7': hdr.startswith('# I-52 — CT-21D Baseline Artifact BA-06: EVM Preparation (DRAFT V7, RELINK TO '
                                              '95690c28)') and '> **BASELINE ARTIFACT BA-06 V7 — DRAFT, NOT SEALED.' in hdr,
        'ARTIFACT_VERSION cites the git blob of the V6 file':
            ('ARTIFACT_VERSION          = 7-DRAFT (supersedes 6-DRAFT, blob %s)' % v6_blob) in hdr,
        'AUTHORITY_CONTRACT unchanged': 'AUTHORITY_CONTRACT        = CT-21D V2.1..V2.5 (V2.4 rev. 3 and V2.5 rev. 2 textually '
                                        'verified) + V2.6 (draft, pending review);' in hdr,
        'BOUND_PRODUCT_SHA': ('BOUND_PRODUCT_SHA         = %s (AR6-01' % BOUND) in hdr,
        'DELTA RULING APPLIED names sections 226 (AR7-01, AR7-02, AR7-09) and 227, then 223 and 220':
            'DELTA RULING APPLIED      = decisions section 226 (AR7-01; AR7-02; AR7-09: the minors BA06V6-01..04) and section 227'
            in hdr and 'section 223 (AR6-01, AR6-02, AR6-03, AR6-04, AR6-05, AR6-06, AR6-07;' in hdr1 and
            'section 220 (AR5-01, AR5-02, AR5-03,' in hdr1,
        'PREREQUISITES (1) is the AR7-01 check against V6; (4) none':
            'exact delta check of these V7 bytes against the V6 bytes (blob %s)' % v6_blob[:16] in hdr and
            'under AR7-01' in hdr1 and '(4) none: the open Architect questions of section 18 are ruled by AR7-02' in hdr1,
        'INVENTORY_REVIEW_STATUS records AR7-01': 'APPROVED on the V6 bytes as the design-time inventory at BOUND_PRODUCT_SHA '
                                                  '95690c28 (AR7-01)' in hdr1 and 'INVENTORY_COMPLETENESS    = NOT_ESTABLISHED' in hdr,
        'DEPENDS_ON = BA-01, BA-03, BA-05, BA-07': 'DEPENDS_ON                = BA-01, BA-03, BA-05, BA-07 (registry entries only;' in hdr,
        'STATUS AUTHORITY cites the BA-11 registry entry': 'the BA-11 registry entry of this artifact is the only authority' in hdr,
        'I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED': 'I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED' in hdr,
        'no CANDIDATE_HASH or SEAL_HASH': 'CANDIDATE_HASH' not in hdr and 'SEAL_HASH' not in hdr,
        'last section is Delta V7 (decisions 226)': [l for l in lines if l.startswith('## ')][-1] == '## 20. Delta V7 (decisions 226)',
    }
    record('BA06-C12', 'the header is V7: it cites the git blob of the V6 file, the unchanged contract chain, BOUND_PRODUCT_SHA '
           '95690c28, sections 226 and 227 before 223 and 220, the AR7-01 prerequisite and review status, DEPENDS_ON = BA-01, BA-03, '
           'BA-05, BA-07, the BA-11 registry entry and the naming state, has no CANDIDATE_HASH or SEAL_HASH field, and the artifact '
           'ends with the Delta V7 section', all(checks12.values()),
           'V6 blob %s; ' % v6_blob + '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in checks12.items()))

    # BA06-C13: the V6 bytes are the ones the BA-11 V6 registry records (the bytes AR7-01 approved)
    dec_text = text(DECISIONS)
    s220 = dec_text[dec_text.index('\n## 220. '):dec_text.index('\n## 221. ')]
    s223 = dec_text[dec_text.index('\n## 223. '):dec_text.index('\n## 224. ')]
    s226 = dec_text[dec_text.index('\n## 226. '):dec_text.index('\n## 227. ')]
    reg = [split_row(l) for l in text(BA11V6).split('\n') if l.startswith('| BA-06 | `CT21D-BASE-EVM-PREPARATION` |')]
    reg_ok = len(reg) == 1 and reg[0][2] == '`%s`' % V6 and reg[0][4] == '`%s`' % sha256(b6)
    rv_sha = re.search(r'I-52-ct21d-relink-v6-architect-review\.json` \(SHA-256 `([0-9a-f]{64})`\)', s226)
    rv = load(REVIEW)
    rv_ok = rv_sha is not None and rv_sha.group(1) == sha256(raw(REVIEW))
    ar501 = [l for l in s220.split('\n') if l.startswith('| AR5-01 |')]
    m501 = re.search(r'blob `([0-9a-f]{7,40})`', ar501[0]) if len(ar501) == 1 else None
    v5_ok = m501 is not None and blob(raw(V5)).startswith(m501.group(1))
    record('BA06-C13', 'the V6 file is the one that the BA-11 V6 registry entry of BA-06 records (path and SHA-256) and the one the '
           'V7 header supersedes; the V6 architect review evidence has the SHA-256 that decisions section 226 states (AR7-01 was '
           'decided on these V6 bytes); the V5 file is still the one AR5-01 carried', reg_ok and rv_ok and v5_ok and
           checks12['ARTIFACT_VERSION cites the git blob of the V6 file'],
           'V6 sha256 %s blob %s; registry %s; review %s (reviewed commit %s); V5 blob prefix %s' % (
               sha256(b6), v6_blob, reg_ok, rv_ok, rv.get('reviewed_commit'), m501.group(1) if m501 else None))

    # ------------------------------------------------------------------------------------------ BA06-C14 exact delta check
    def parse(ls):
        """Header fields, and per section (heading line): tables (by first header cell) and non-table lines."""
        i1 = [k for k, l in enumerate(ls) if l.startswith('## ')][0]
        fields, cur = {}, None
        for l in ls[:i1]:
            if l.startswith('# '):
                fields['TITLE'] = l
            elif l.startswith('> **BASELINE ARTIFACT'):
                fields['BANNER'] = l
            else:
                m = re.match(r'^> ([A-Z0-9_ ]+?)\s+= ', l)
                if m and not l.startswith('> CT21D_AUTHORITY'):
                    cur = m.group(1).strip()
                    fields[cur] = l
                elif cur is not None and l.startswith('>                             '):
                    fields[cur] += '\n' + l
                else:
                    cur = None
                    fields.setdefault('OTHER', []).append(l)
        secs, order, h = {}, [], None
        k = i1
        while k < len(ls):
            l = ls[k]
            if l.startswith('#'):
                h = l
                if h in secs:
                    raise SystemExit('duplicate heading ' + h)
                secs[h] = {'TABLES': {}, 'LINES': []}
                order.append(h)
                k += 1
                continue
            if l.startswith('|'):
                head = split_row(l) if l.strip().endswith('|') else [l]
                tname = head[0]
                sep = ls[k + 1] if k + 1 < len(ls) else ''
                rows, seen = [], {}
                j = k + 2
                while j < len(ls) and ls[j].startswith('|'):
                    cs = split_row(ls[j])
                    rk = cs[0]
                    seen[rk] = seen.get(rk, 0) + 1
                    if seen[rk] > 1:
                        rk = '%s#%d' % (rk, seen[rk])
                    rows.append((rk, cs))
                    j += 1
                if tname in secs[h]['TABLES']:
                    raise SystemExit('two tables with the same first header cell under ' + h)
                secs[h]['TABLES'][tname] = {'HEAD': head, 'SEP': sep, 'ROWS': rows}
                secs[h]['LINES'].append('<table %s>' % tname)
                k = j
                continue
            secs[h]['LINES'].append(l)
            k += 1
        return fields, secs, order

    f6, secs6, order6 = parse(lines6)
    f7, secs7, order7 = parse(lines)
    items = []   # every change found: dict with SECTION, TABLE, KEY, CHANGE, and cell/line edits
    prob14 = []
    # header fields
    for fk in sorted(set(f6) | set(f7), key=str):
        if f6.get(fk) != f7.get(fk):
            items.append({'SECTION': '(header)', 'TABLE': None, 'KEY': 'FIELD ' + fk,
                          'CHANGE': 'CHANGED' if fk in f6 and fk in f7 else ('ADDED' if fk in f7 else 'REMOVED'),
                          'EDITS': edits(str(f6.get(fk, '')), str(f7.get(fk, '')))})
    # sections
    common = [h for h in order6 if h in secs7]
    if [h for h in order7 if h in secs6] != common:
        prob14.append('section order changed')
    for h in order6:
        if h not in secs7:
            items.append({'SECTION': h, 'TABLE': None, 'KEY': '<section>', 'CHANGE': 'REMOVED'})
    for h in order7:
        if h not in secs6:
            items.append({'SECTION': h, 'TABLE': None, 'KEY': '<section>', 'CHANGE': 'ADDED',
                          'LINES': len(secs7[h]['LINES'])})
    for h in common:
        a, b = secs6[h], secs7[h]
        # non-table lines (tables appear as placeholders, so a table added or removed shows here too)
        sm = difflib.SequenceMatcher(None, a['LINES'], b['LINES'], autojunk=False)
        for op, a1, a2, b1, b2 in sm.get_opcodes():
            if op == 'equal':
                continue
            old, new = a['LINES'][a1:a2], b['LINES'][b1:b2]
            if op == 'replace' and len(old) == len(new):
                for x, y in zip(old, new):
                    items.append({'SECTION': h, 'TABLE': None, 'KEY': y[:120], 'CHANGE': 'CHANGED', 'EDITS': edits(x, y)})
            else:
                for y in new:
                    items.append({'SECTION': h, 'TABLE': None, 'KEY': y[:120], 'CHANGE': 'ADDED'})
                for x in old:
                    items.append({'SECTION': h, 'TABLE': None, 'KEY': x[:120], 'CHANGE': 'REMOVED'})
        for tn in set(a['TABLES']) | set(b['TABLES']):
            ta, tb = a['TABLES'].get(tn), b['TABLES'].get(tn)
            if ta is None or tb is None:
                continue  # reported by the placeholder line
            if ta['HEAD'] != tb['HEAD'] or ta['SEP'] != tb['SEP']:
                items.append({'SECTION': h, 'TABLE': tn, 'KEY': '<header>', 'CHANGE': 'CHANGED',
                              'OLD_COLUMNS': len(ta['HEAD']), 'NEW_COLUMNS': len(tb['HEAD']),
                              'EDITS': edits(' | '.join(ta['HEAD']), ' | '.join(tb['HEAD']))})
            ra_, rb_ = dict(ta['ROWS']), dict(tb['ROWS'])
            ka = [k_ for k_, _ in ta['ROWS']]
            kb = [k_ for k_, _ in tb['ROWS']]
            if [k_ for k_ in ka if k_ in rb_] != [k_ for k_ in kb if k_ in ra_]:
                prob14.append('row order changed in %s / %s' % (h[:30], tn))
            for k_ in ka:
                if k_ not in rb_:
                    items.append({'SECTION': h, 'TABLE': tn, 'KEY': k_, 'CHANGE': 'REMOVED'})
                elif ra_[k_] != rb_[k_]:
                    x, y = ra_[k_], rb_[k_]
                    items.append({'SECTION': h, 'TABLE': tn, 'KEY': k_, 'CHANGE': 'CHANGED', 'OLD': x, 'NEW': y})
            for k_ in kb:
                if k_ not in ra_:
                    items.append({'SECTION': h, 'TABLE': tn, 'KEY': k_, 'CHANGE': 'ADDED'})

    # the declared set, expanded ('*' = every row of that table in V6)
    expected = {}
    for fk, why in DECL_HEADER.items():
        expected[('(header)', None, 'FIELD ' + fk)] = ('CHANGED', 'RECORD', why, None, None)
    for hs, why in DECL_SECTIONS.items():
        expected[(hs, None, '<section>')] = ('ADDED', why[0], why[1], None, None)

    def sec_of(prefix):
        hs = [h for h in order7 if h.startswith(prefix)]
        if len(hs) != 1:
            raise SystemExit('heading prefix not unique: ' + prefix)
        return hs[0]

    line_decl = []
    for hp, tn, k_, chg, kind, why, colset, addcol in DECL:
        h = sec_of(hp)
        if tn is None:
            line_decl.append((h, k_, chg, kind, why))
            continue
        keys = [x for x, _ in secs6[h]['TABLES'][tn]['ROWS']] if k_ == '*' else [k_]
        for kk in keys:
            if (h, tn, kk) in expected and k_ == '*':
                continue  # a specific declaration wins over '*'
            expected[(h, tn, kk)] = (chg, kind, why, colset, addcol)
    for it in items:
        if it['TABLE'] is None and it['KEY'] not in ('<section>',) and not it['KEY'].startswith('FIELD '):
            d = [x for x in line_decl if x[0] == it['SECTION'] and it['KEY'].startswith(x[1][:120])]
            if len(d) == 1:
                it['_ID'] = (d[0][0], None, d[0][1])
                expected.setdefault((d[0][0], None, d[0][1]), (d[0][2], d[0][3], d[0][4], None, None))
            else:
                it['_ID'] = (it['SECTION'], None, it['KEY'])
        else:
            it['_ID'] = (it['SECTION'], it['TABLE'], it['KEY'])
    for x in line_decl:
        expected.setdefault((x[0], None, x[1]), (x[2], x[3], x[4], None, None))
    found = {}
    for it in items:
        if it['_ID'] in found:
            prob14.append('two changes map to one declared item: %s' % (it['_ID'],))
        found[it['_ID']] = it
        d = expected.get(it['_ID'])
        if d is None:
            it['KIND'] = 'UNDECLARED'
            prob14.append('undeclared change: %s %s %s' % (it['SECTION'][:40], it['TABLE'], str(it['KEY'])[:60]))
            continue
        it['KIND'], it['RULING'] = d[1], d[2]
        if it['CHANGE'] != d[0]:
            prob14.append('%s: change %s, declared %s' % (it['_ID'], it['CHANGE'], d[0]))
        if it['TABLE'] is not None and it['KEY'] == '<header>':
            if d[4] is not None and it['NEW_COLUMNS'] != it['OLD_COLUMNS'] + 1:
                prob14.append('%s: not exactly one column added' % (it['_ID'],))
        elif it['CHANGE'] == 'CHANGED' and it['TABLE'] is not None:
            x, y = it['OLD'], it['NEW']
            addcol = d[4]
            if addcol is not None:
                if len(y) != len(x) + 1:
                    prob14.append('%s: no added column' % (it['_ID'],))
                    continue
                xa = x[:addcol - 1] + [None] + x[addcol - 1:]
            else:
                xa = x
            changed_cols = [c + 1 for c, (p, q) in enumerate(zip(xa, y)) if p != q]
            it['CHANGED_COLUMNS'] = changed_cols
            it['CELL_EDITS'] = [{'COLUMN': c, 'EDITS': edits(xa[c - 1] or '', y[c - 1])} for c in changed_cols]
            if d[3] is not None and not set(changed_cols) <= set(d[3]):
                prob14.append('%s: columns %s outside the declared %s' % (it['_ID'], changed_cols, d[3]))
            del it['OLD'], it['NEW']
    missing_decl = sorted(str(k_) for k_ in expected if k_ not in found)
    for m in missing_decl:
        prob14.append('declared change did not happen: %s' % m)
    content = sorted((next((p for p in ('### 3.1 ', H33) if it['SECTION'].startswith(p)), it['SECTION']), it['KEY'])
                     for it in items if it.get('KIND') == 'CONTENT')
    content_ok = set(content) == CONTENT_EXPECTED
    if not content_ok:
        prob14.append('CONTENT changes %s differ from %s' % (content, sorted(CONTENT_EXPECTED)))
    unchanged_parts = {}
    for label, prefixes in (('3.2 contract rows', ['### 3.2 ']), ('3.2.5 supplementary rows', ['#### 3.2.5 ']),
                            ('section 5 residuals', ['## 5. ']), ('section 7 trace plan', ['## 7. ']),
                            ('10.2 plan-shape classes', ['### 10.2 ']), ('section 11 f(plan)', ['## 11. ']),
                            ('section 12 EVM schema', ['## 12. ']), ('sections 1, 2, 2.1, 8, 9, 10.1', ['## 1. ', '## 2. ', '### 2.1 ',
                                                                                                    '## 8. ', '## 9. ', '### 10.1 '])):
        hs = [h for h in order7 if any(h.startswith(p) for p in prefixes)]
        unchanged_parts[label] = not any(it['SECTION'] in hs for it in items) and len(hs) == len(prefixes)
    if not all(unchanged_parts.values()):
        prob14.append('changed parts that must stay unchanged: %s' % [k_ for k_, v in unchanged_parts.items() if not v])
    oc33 = secs7[sec_of(H33)]['TABLES']['Class']['ROWS']
    oc33_6 = secs6[sec_of(H33)]['TABLES']['Class']['ROWS']
    other_class_rows_same = [k_ for (k_, r7), (_, r6) in zip(oc33, oc33_6) if k_ != '`OC-IMPORT`' and r7 != r6]
    if other_class_rows_same:
        prob14.append('3.3 class rows other than OC-IMPORT changed: %s' % other_class_rows_same)
    for it in items:
        it.pop('_ID', None)
    rep14 = {'COMPARED_WITH': V6, 'COMPARED_WITH_BLOB': v6_blob, 'CHANGES': items,
             'DECLARED_ITEMS': len(expected), 'CHANGED_ITEMS': len(items),
             'SET_EQUALS_DECLARED_SET': not missing_decl and all(it.get('KIND') != 'UNDECLARED' for it in items),
             'CONTENT_CHANGES': content, 'UNCHANGED_PARTS': unchanged_parts, 'PROBLEMS': prob14}
    kinds = {}
    for it in items:
        kinds.setdefault(it.get('KIND'), 0)
        kinds[it.get('KIND')] += 1
    record('BA06-C14', 'EXACT DELTA CHECK (AR7-01): V7 differs from the V6 bytes, over the whole file (header fields, every table '
           'row, every other line, every section), exactly by the declared set of changes (BA06V6-01..04 and AR7-02), each with its '
           'kind and ruling; the only CONTENT changes are the U-45 row of 3.1 (seam role) and the OC-IMPORT row of 3.3; the contract '
           'rows, section 5, section 12, the trace plan and the other class rows are unchanged; the REPORT lists every change with its '
           'cell or line edits', not prob14,
           'changes %d (declared %d); kinds %s; content %s; problems %s' % (len(items), len(expected), kinds, content, prob14), rep14)

    # BA06-C15: the declared-limits paragraph of section 7 equals V6
    lim7 = [l for l in lines if l.startswith('**Declared limits (condition 2).**')]
    lim6 = [l for l in lines6 if l.startswith('**Declared limits (condition 2).**')]
    lim4 = lim7[0][lim7[0].index(LIMIT4_START):] if len(lim7) == 1 and LIMIT4_START in lim7[0] else ''
    record('BA06-C15', 'section 7 declared limit (4) is present with the four elements of AR4-12 condition (1), and the '
           'declared-limits paragraph equals the V6 paragraph', all(p in lim4 for p in LIMIT4_PHRASES) and len(lim6) == 1 and
           lim6 == lim7, '; '.join('%s: %s' % (p, p in lim4) for p in LIMIT4_PHRASES))

    # BA06-C16: the recompile mapping of section 4.4 is a partition, with the class-or-disposition column
    rec = load(RECOMPILE)
    ops = {o['id']: o for o in rec['operations']}
    h44, mrows44 = table('| V6 row or disposition | R1 | R2 | R3 | Operations |')

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
    entries_nd = [i for i in ('R1-001', 'R2-001', 'R3-001', 'R3-009') if assigned.get(i) != 'ND']
    oc7 = {r[0]: members(r[1]) for r in octab}
    class_of = {}
    for c, m in oc7.items():
        for u in m:
            class_of.setdefault(u, []).append(c)
    disp_bad, mut_rows = [], {}
    has_col = h44[-1].startswith('Class of 3.3 or disposition')
    for r in mrows44:
        k_ = r[0].split(':')[0]
        cell = r[5] if has_col and len(r) == 6 else ''
        if k_ in class_of:
            if not cell.startswith('class: ') or sorted(re.findall(r'`OC-[A-Z-]+`', cell)) != sorted(class_of[k_]):
                disp_bad.append(k_)
        elif not re.match(r'^(no class|OUT|excluded|n/a): ', cell):
            disp_bad.append(k_)
        n_mut = sum(1 for i in expand(r[1]) + expand(r[2]) + expand(r[3]) if ops.get(i, {}).get('mutating') == 'MUTATING')
        if n_mut:
            mut_rows[k_] = {'MUTATING': n_mut, 'CLASS_OR_DISPOSITION': cell}
    record('BA06-C16', 'section 4.4 assigns each of the 689 operations of the blind recompile (source SHA 95690c28, which did not '
           'read BA-06 V5 section 3 or the Gate 2 assessment) to exactly one V6 row or to ND; the command entries R1-001, R2-001, '
           'R3-001 and the dispatch R3-009 are ND (BA06V6-03); every MUTATING operation goes to a U-row; for every non-aggregate '
           'U-row, the file of each assigned operation is cited in the row\'s location cell; the counts column is exact; every row '
           'names exactly its classes of 3.3 ("class: ...") or states a disposition with no class ("no class", "OUT", "excluded", '
           '"n/a"), so every row carrying a MUTATING operation is resolved to a class or a disposition (BA06V6-04; REPORT lists them)',
           hdr_ok and len(ops) == 689 and not not_assigned and not extra and not dup and not mut_bad and not file_bad and
           not count_bad and not entries_nd and has_col and not disp_bad,
           'header %s; unassigned %s; unknown %s; duplicates %s; MUTATING not on a U-row %s; file not cited %s; count mismatch %s; '
           'entries not ND %s; column %s; class/disposition mismatches %s; MUTATING-carrying rows %d' %
           (hdr_ok, not_assigned[:10], extra[:10], dup[:10], mut_bad[:10], file_bad[:10], count_bad, entries_nd, has_col,
            disp_bad, len(mut_rows)), {'MUTATING_CARRYING_ROWS': mut_rows})

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

    # BA06-C18: the carry table recomputed from the V5 and V6 bytes and the V7 membership (BA06V6-01)
    ar603 = [l for l in s223.split('\n') if l.startswith('| AR6-03 |')][0]
    carry603 = sorted(re.findall(r'`(OC-[A-Z-]+)`', ar603[ar603.index('candidatas:'):]))
    v6out = load(V6_OUTPUT)
    rep6 = [r for r in v6out['RESULTS'] if r['ID'] == 'BA06-C14'][0]['REPORT']['TABLES']

    def changed_keys(hstart, mode):
        _, a = table_in(lines5, hstart)
        _, b = table_in(lines6, hstart)
        if mode == 'index':
            pairs = [(n, x, y) for n, (x, y) in enumerate(zip(a, b), 1)]
            assert len(a) == len(b)
        else:
            ka = {relabel6(r[0]): r for r in a}
            pairs = [(r[0], ka.get(r[0]), r) for r in b]
        return {k_: (x, y) for k_, x, y in pairs if x is None or [relabel6(c) for c in x] != y}

    changed_ids = set()
    for part, hs in (('3.1', '| id | category | api_call | location | seam_role | transaction_context | found_by |'),
                     ('3.2', '| id | actor | kind | operation | clause fragments | R-list (V2) | class |'),
                     ('3.2.5', '| id | actor | kind | operation | clause (outside the fragments) | origin | class |')):
        ck = set(changed_keys(hs, 'key'))
        recorded = set(e['ROW'] for e in rep6[part]['CHANGED_ROWS'] if e['KIND'] != 'LABEL')
        if ck != recorded:
            raise SystemExit('V5->V6 recomputation differs from the V6 output in %s: %s' % (part, sorted(ck ^ recorded)))
        changed_ids |= ck
    cls56 = changed_keys('| Class | Operations | Actor and phase |', 'key')
    cls_kind = {e['ROW']: e['KIND'] for e in rep6['3.3']['CHANGED_ROWS'] if e['KIND'] != 'LABEL'}
    if set(cls56) != set(cls_kind):
        raise SystemExit('V5->V6 class-row recomputation differs from the V6 output')
    s5ch = changed_keys('| residual | affects | why not statically visible |', 'index')
    s5_kind = {e['ROW']: e['KIND'] for e in rep6['5']['CHANGED_ROWS'] if e['KIND'] != 'LABEL'}
    if set(s5ch) != set(s5_kind) or any(v != 'CONTENT' for v in s5_kind.values()):
        raise SystemExit('V5->V6 section-5 recomputation differs from the V6 output')
    _, oc5 = table_in(lines5, '| Class | Operations | Actor and phase |')
    mem5 = {r[0]: members(r[1]) for r in oc5}
    _, ct = table('| Class | Members (V7) | Changed members vs V5 | Membership change |')
    bad18, s5_report = [], {}
    for r_oc, r_ct in zip(octab, ct):
        if r_oc[0] != r_ct[0]:
            bad18.append('order %s' % r_oc[0])
            continue
        mem = members(r_oc[1])
        if ', '.join(mem) != r_ct[1]:
            bad18.append('members %s' % r_oc[0])
        chg = [m for m in mem if m in changed_ids]
        if (', '.join(chg) or 'none') != r_ct[2]:
            bad18.append('changed %s' % r_oc[0])
        m5 = mem5.get(r_oc[0], [])
        adds, rems = [m for m in mem if m not in m5], [m for m in m5 if m not in mem]
        mc = '; '.join(([('+ ' + ', '.join(adds))] if adds else []) + ([('- ' + ', '.join(rems))] if rems else [])) or 'none'
        if mc != r_ct[3]:
            bad18.append('membership change %s' % r_oc[0])
        want5, memb_changed = [], False
        for n, (x, y) in sorted(s5ch.items()):
            a_, b_ = ids_of(x[1]) & set(mem), ids_of(y[1]) & set(mem)
            if not (a_ or b_):
                continue
            same = a_ == b_
            memb_changed = memb_changed or not same
            want5.append((n, 'residual text changed, membership unchanged' if same else 'residual membership changed'))
        got5 = [(int(a_), b_) for a_, b_ in re.findall(r'row (\d+): (residual text changed, membership unchanged|residual '
                                                            r'membership changed)', r_ct[4])]
        if got5 != want5 or (not want5 and r_ct[4] != 'none'):
            bad18.append('section-5 column %s' % r_oc[0])
        s5_report[r_oc[0]] = want5
        content_change = bool(chg) or cls_kind.get(r_oc[0]) == 'CONTENT' or memb_changed
        carry = r_ct[6].startswith('CARRY CANDIDATE')
        if carry == content_change:
            bad18.append('status %s' % r_oc[0])
    cands = sorted(r[0].strip('`') for r in ct if r[6].startswith('CARRY CANDIDATE'))
    others = {r[0].strip('`'): r[6] for r in ct if not r[6].startswith('CARRY CANDIDATE')}
    want_others = {'OC-IMPORT': 'FULL RE-REVIEW', 'OC-REF-TOP': 'CONFIRMATION', 'OC-ENVELOPE': 'CONFIRMATION',
                   'OC-PREP-READ': 'RE-REVIEW', 'OC-VERIFY-READ': 'BOUNDED RE-REVIEW'}
    ok_others = sorted(others) == sorted(want_others) and all(others[k].startswith(v) for k, v in want_others.items())
    record('BA06-C18', 'the carry table 3.4 lists the classes of 3.3 in order with their V7 members, the members changed from V5 '
           'to V6 and the membership change from V5 (recomputed from the V5 and V6 bytes and equal to the rows the bound V6 check '
           'output records), and, for every section-5 row that changed in content from V5 to V6 and covers a member, whether its '
           'residual membership changed (BA06V6-01); a class counts as content-changed if a member row, its class row (CONTENT) or '
           'the residual membership changed, and a carry candidate has no content change; the carry candidates are exactly the 16 '
           'classes that AR6-03 lists', not bad18 and cands == carry603 and len(cands) == 16 and ok_others,
           'candidates %s; AR6-03 %s; others %s; problems %s' % (cands, carry603, others, bad18),
           {'CHANGED_MEMBER_ROWS_V5_TO_V6': sorted(changed_ids), 'SECTION5_ROWS_V5_TO_V6': sorted(s5ch),
            'SECTION5_COLUMN': s5_report})

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
        return members(ocr[c][1])

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

    # BA06-C25: AR7-02 applied
    ar702 = [l for l in s226.split('\n') if l.startswith('| AR7-02 |')]
    m702 = re.search(r'`OC-IMPORT` = ((?:[UC]-\d+, )*[UC]-\d+);', ar702[0]) if len(ar702) == 1 else None
    oci = ocr['OC-IMPORT']
    u = {r[0]: r for r in u31}
    s18 = {r[0]: r for r in table_in(lines[[k for k, l in enumerate(lines) if l.startswith('## 18. ')][0]:],
                                         '| Id | Question (as recorded for the Architect) |')[1]}
    in_class = set(m for ms in oc7.values() for m in ms)
    sub25 = {
        'decisions 226 AR7-02 states the OC-IMPORT membership': m702 is not None and m702.group(1).split(', ') == OC_IMPORT_AR702,
        'OC-IMPORT members = U-03, U-07, U-08, C-81, C-82': members(oci[1]) == OC_IMPORT_AR702,
        'U-02 and U-45 named as sites in the class row, not members': 'U-02 on R1' in oci[1] and 'U-45 on R2 and R3' in oci[1] and
        'not members' in oci[1] and 'U-02' not in in_class and 'U-45' not in in_class,
        'micro-scenario context X-01, C-89, no L1/L0 reproduction': 'held lock (X-01)' in oci[2] and 'outside any RackCad '
        'transaction (C-89)' in oci[2] and 'does not reproduce the product\'s L1 and L0 contexts' in oci[2],
        'micro-scenario and fixture unchanged': oci[3] == '`EV-L-OC-IMPORT`' and oci[4] == 'FX-IMP (K-8)',
        'U-45 seam role: call site of OC-IMPORT, not a member': 'call site of `OC-IMPORT`' in u['U-45'][4] and
        'not a class member' in u['U-45'][4],
        'U-48 stays a watch item that reopens OC-REF-TOP': u['U-48'][4].startswith('Watch item') and 'reopens OC-REF-TOP' in
        u['U-48'][4] and 'U-48' not in in_class and members(ocr['OC-REF-TOP'][1])[:2] == ['U-31', 'U-32'],
        'section 18 records the four rulings': all(s18['AQ-V6-BA06-%02d' % n][3].startswith('RULED by AR7-02 (%02d)' % n)
                                                   for n in (1, 2, 3, 4)),
    }
    record('BA06-C25', 'AR7-02: OC-IMPORT = U-03, U-07, U-08, C-81, C-82 as decisions section 226 states; U-02 (R1) and U-45 (R2, '
           'R3) are recorded as call sites in the class row and are members of no class; the micro-scenario runs under X-01 outside '
           'any RackCad transaction (C-89); U-45\'s seam role says so; U-48 stays a watch item; section 18 records the rulings of '
           'AQ-V6-BA06-01..04', all(sub25.values()),
           '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub25.items()))

    out = {
        'CHECKER': SELF,
        'CHECKER_SHA256': sha256(raw(SELF)),
        'ARTIFACT': ART,
        'COMPARED_WITH': V6,
        'COMPARED_WITH_BLOB': v6_blob,
        'NOTE': 'no hash of the V7 artifact is recorded here: the artifact binds this output by hash (section 14)',
        'RESULTS': results,
        'ALL_PASS': all(r['RESULT'] == 'PASS' for r in results),
    }
    data = json.dumps(out, ensure_ascii=False, indent=1) + '\n'
    io.open(out_path, 'w', encoding='utf-8', newline='\n').write(data)
    for r in results:
        print(r['ID'], r['RESULT'], r['DETAIL'][:400])
    return 0 if out['ALL_PASS'] else 1


if __name__ == '__main__':
    sys.exit(main())
