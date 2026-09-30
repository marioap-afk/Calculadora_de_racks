"""I-52 CT-21D baseline artifact BA-06 V5: reproducible checks of the statements that BA-06 V5 makes about itself, and the
STATIC_REVIEWED carry-over checks of AR4-12 condition (2) (decisions section 217).

Usage: python I-52-ct21d-ba06-v5-checks.py <repository root> <output file>

Reads only repository files (the V5 and V4 artifacts, the decisions file, the Architect's review evidence of section 217, the
contract texts and the evidence files the artifact binds) and writes one JSON result file. Fails closed: a missing input, a table
or section that cannot be parsed, or any failed check gives exit code 1. The output records no hash of the V5 artifact, because the
V5 artifact binds this output by hash (section 14); it records the hashes of the V4 artifact, which BA06-C13 compares with the
approval of AR4-12. Deterministic: no clock, no network.
Hashes: SHA-256 of the LF-normalized bytes, lowercase hexadecimal; git blob ids: SHA-1 of "blob <size>\\0" + LF-normalized bytes.

BA06-C1 to BA06-C12 repeat the V4 checks on the V5 bytes. BA06-C13 extracts the frozen sections (the design-time inventory
sections 3 to 3.2.5 and 3.3, the dynamic-trace plan of section 7, and sections 10.2 and 11, which the trace plan uses) from V4
and V5, and shows that V5 equals V4 after the closed list of declared label substitutions, except the insertion of declared limit
(4); it reports every changed line of those sections. The frozen sections are not the whole inventory: the section 3
"Composition" paragraph also counts the residuals (section 5) and the EVM schema (section 12), which are outside them. BA06-C14
reports the content changes outside the frozen sections: the review record 3.2.6 and the inventory components of sections 5 and
12, and verifies their invariants (same residual rows, "affects" cells and EVM fields). BA06-C15 verifies limit (4) against AR4-12
condition (1). BA06-C16 verifies that no round-V4 open question is left pending. BA06-C17 verifies that the Delta V5 table covers
every BA-06 finding of both review passes of section 217. BA06-C18 (correction pass of round V5) verifies the statements that the
correction pass made (section 9 item (2) and the section 12 pointer, the driver wording of sections 9 and 10.1, the EV-L-COMPOSED
wording of section 9, the inventory row of section 13, and the open Architect questions of section 16.1 and the header). The correction
pass (final) of round V5 (the lead's option (a): the edge BA-06 -> BA-07) tightens BA06-C12 (DEPENDS_ON = BA-01, BA-03, BA-05, BA-07
exactly) and BA06-C18 (the Dependencies paragraph takes the run-start anchor condition from BA-07; the section 9 cross-note cites BA-07 V5
section 5.1 and 6.2 step 5(c); the correction pass (final) row). No check is relaxed.
"""
import difflib
import hashlib
import io
import json
import os
import re
import sys

ART = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v5.md'
V4 = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v4.md'
SELF = 'docs/automation/evidence/I-52-ct21d-ba06-v5-checks.py'
OUTPUT_NAME = 'I-52-ct21d-ba06-v5-checks-output.json'
DECISIONS = 'docs/automation/decisions/I-52.md'
REVIEW = 'docs/automation/evidence/I-52-ct21d-phase2-delta2-architect-review.json'
DL = 'docs/automation/evidence/I-52-ct21d-phase2-delta/'
SA = 'docs/automation/evidence/I-52-ct21d-phase2-static-analysis/'
TI = 'docs/automation/evidence/I-52-ct21d-ba06-v4-task-inputs.json'
CONTRACT = {'V2.1': 'docs/initiatives/I-52-ct21d-authority-contract-v2.1.md',
            'V2.2': 'docs/initiatives/I-52-ct21d-authority-contract-v2.2.md',
            'V2.4': 'docs/initiatives/I-52-ct21d-authority-contract-v2.4.md'}
FRAG_V21 = {'4.2', '7', '7.1', '15.2', '15.3', '15.4', '15.5', '15.6', '15.7', '19.1', '27.2'}
FRAG_V22 = {'PA-6', 'PA-10'}
MARK = 'The computed task text follows:\n'

# BA06-C13: the frozen sections (start heading prefix, end heading prefix) and the closed list of declared label substitutions
FROZEN = [('3 to 3.2.5', '## 3. ', '#### 3.2.6 '), ('3.3', '### 3.3 ', '## 4. '), ('7', '## 7. ', '## 8. '),
          ('10.2', '### 10.2 ', '### 10.3 '), ('11', '## 11. ', '## 12. ')]
LABELS = [('SIBLING_VERSION_LABEL', 'BA-04 V4', 'BA-04 V5'), ('SIBLING_VERSION_LABEL', 'BA-05 V4', 'BA-05 V5'),
          ('SIBLING_VERSION_LABEL', 'BA-07 V4', 'BA-07 V5'), ('SIBLING_VERSION_LABEL', 'BA-08 V4', 'BA-08 V5'),
          ('SIBLING_VERSION_LABEL', 'BA-09 V4', 'BA-09 V5'), ('SIBLING_VERSION_LABEL', 'BA-10 V4', 'BA-10 V5'),
          ('BA11_REGISTRY_CITATION_LABEL (AR4-17)', 'BA-11 V4 section', 'BA-11 registry section'),
          ('ROUND_V4_QUESTION_LABEL (ruled; coordination sheet section 0)', 'is open question AQ-V4-14.',
           'is ruled by AR4-15 (section 217; section 5).')]
LIMIT4_START = ' (4) **Build relation'
LIMIT4_LINE = '**Declared limits (condition 2).**'
LIMIT4_PHRASES = ['same source SHA as the product build', 'every harness switch is off', 'every I-14 writer included',
                  '`RECORDED_NOT_ENFORCED` handling of the AR3-01 set', 'control-plane decisions only',
                  'both build identities', 'this attestation', 'only because of a harness difference',
                  'recorded as a finding, not added to the inventory']
# BA06-C14: the rows that a ruling of section 217 changes outside the frozen sections (key = first cell of the V4 row)
DECLARED_S5 = {'Defaults applied on AppendEntity': 'AR4-07', '**V4 (BA06V3-N08):** event classes of the failure paths': 'AR4-15'}
DECLARED_S12 = {'`INVENTORY_REVIEW_RECORDS`': 'R1:BA06V4-N02, R2:BA06V4-N04'}
# BA06-C16: the round-V4 open questions of BA-06 and the decision that rules each (decisions section 217)
RULED = {'AQ-V4-01': 'AR4-01', 'AQ-V4-11': 'AR4-12', 'AQ-V4-14': 'AR4-15'}
TOK = re.compile(r'\w+|\s+|[^\w\s]')


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
    md4 = text(V4)
    lines4 = md4.split('\n')

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
        counts = {}
        for cat, a, b in LABELS:
            n = t.count(a)
            if n:
                counts['%s: %s -> %s' % (cat, a, b)] = n
                t = t.replace(a, b)
        return t, counts

    def edits(a, b):
        ta, tb = TOK.findall(a), TOK.findall(b)
        sm = difflib.SequenceMatcher(None, ta, tb, autojunk=False)
        out = []
        for op, a1, a2, b1, b2 in sm.get_opcodes():
            if op != 'equal':
                out.append({'OP': op.upper(), 'V4': ''.join(ta[a1:a2]), 'V5': ''.join(tb[b1:b2]),
                            'V5_CONTEXT': ''.join(tb[max(0, b1 - 8):b2 + 8])})
        return out

    # BA06-C1: matrix columns = the classes of section 10.2
    head, mrows = table('| Entry point | SH-F1 |')
    cols = set(re.sub(r' \(\d\)$', '', c) for c in head[1:])
    _, shapes = table('| Class | Description | Fixture (BA-08 V5) |')
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
    _, octab = table('| Class | Operations | Actor and phase | Micro-scenario (BA-04 V5) or reasoned exclusion |')
    oc_names = [r[0].strip('`') for r in octab]
    _, xrows = table('| id | actor | kind | operation | clause (outside the fragments) | origin | class |')
    cited = set(r[6].split(' ')[0] for r in crows + xrows if r[6].startswith('OC-'))
    micro_ok = all(('`EV-L-%s`' % n) in r[3] or r[3].startswith('none. Reasoned exclusion') for n, r in zip(oc_names, octab))
    n_micro = sum(1 for r in octab if r[3].startswith('`EV-L-'))
    record('BA06-C4', 'every class cited in sections 3.2 and 3.2.5 is a class of section 3.3, and every class of 3.3 has its '
           'micro-scenario EV-L-<class> or a reasoned exclusion', cited <= set(oc_names) and micro_ok and
           len(oc_names) == len(set(oc_names)), 'classes %d (micro-scenario %d, exclusion %d); cited not in 3.3: %s' %
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
        elif r[3] != esc(o['operation']):
            bad.append(o['id'])
    record('BA06-C5', 'the 203 rows of section 3.2 transcribe sa1-compiled-operations.json (id, actor, kind, operation, clause '
           'fragments), except the declared V4 amendment of C-98', len(crows) == len(comp) == 203 and not bad,
           'rows %d; mismatches %s' % (len(crows), bad))

    # BA06-C6: difference log
    _, dcrows = table('| Id | Difference | Resolution (comparator) |')
    diffs = load(DL + 'sa1-comparison-with-t3-and-r-list.json')['differences']
    ok6 = len(dcrows) == len(diffs) == 51
    ok6 = ok6 and all(d['id'] == 'DF-%02d' % n and r[0] == 'DC-%02d' % n and r[1] == esc(d['description']) and r[5]
                      for n, (d, r) in enumerate(zip(diffs, dcrows), 1))
    record('BA06-C6', 'DF-nn = DC-nn in order; every DC row transcribes its comparator difference and carries a V4 disposition', ok6,
           'rows %d' % len(dcrows))

    # BA06-C7: ambiguities: a clause outside the fragments, or a contract question that exists in 3.2.6
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
        body = re.sub(r'\n---\s*$', '', parts[i + 1].rstrip('\n')).strip('\n')
        if body not in contract[parts[i].split()[0]]:
            missing.append(parts[i])
    record('BA06-C8', 'every fragment of sa1-contract-fragments.md is contained verbatim in the current V2.1, V2.2 or V2.4 text',
           len(parts) // 2 == 8 and not missing, '%d fragments; not found: %s' % (len(parts) // 2, missing))

    # BA06-C9: evidence hashes of section 14 (the output row is skipped: it is this file's product)
    _, ev = table('| Path | SHA-256 | Version |')
    wrong = []
    for r in ev:
        p = r[0].strip('`')
        if p.endswith(OUTPUT_NAME):
            continue
        if not os.path.exists(path(p)) or sha256(raw(p)) != r[1].strip('`'):
            wrong.append(p)
    record('BA06-C9', 'every SHA-256 of section 14, except the row of this output, equals the SHA-256 of the LF-normalized file',
           len(ev) == 28 and not wrong, 'rows %d; mismatches %s' % (len(ev), wrong))

    # BA06-C10: task inputs (BA06V3-N06), recomputed from repository files
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
    blind = all('INDEPENDENCE RULE: you are a BLIND independent reviewer' in body(tasks[l]['TASK_TEXT'])
                for l in ('blind-op-review:call-graph', 'blind-op-review:api-sweep', 'critic:op-reviews'))
    prompts = load(DL + 'prompts.json')
    compact = json.dumps(load(DL + 'sa1-compiled-operations.json'), ensure_ascii=False, separators=(',', ':'))
    b1a = tasks['blind-op-review:call-graph']['TASK_TEXT']
    b1b = tasks['blind-op-review:api-sweep']['TASK_TEXT']
    framing = {
        'entry points in B1a': 'RackSelectivoCommands RACKSELECTIVO/RS, RackMenuCommands RACKCAD' in b1a,
        'path chain in B1a and B1b': all('RACKSELECTIVO -> DrawSelectiveView' in t and 'library import via BlockLibraryImporter' in t
                                         for t in (b1a, b1b)),
        'seam paraphrase points in B1a and B1b': all('ONE block reference in model space' in t and
                                                     'must set position/scale/rotation/normal from the transform' in t
                                                     for t in (b1a, b1b)),
        'REACH list only in the census and dependency tasks': sorted(l for l, t in tasks.items()
                                                                   if 'Seam paths (for reachability)' in t['TASK_TEXT']) ==
        ['census:application', 'census:domain', 'census:plugin-drawing-systems', 'census:plugin-rest', 'deps-loadset-commands'],
    }
    sub = {
        'hashes of the 15 task texts and bodies': hash_ok and len(tasks) == 15,
        'critic REVIEW A = B1a': ra == load(SA + 'B1a-blind-op-review-call-graph.json'),
        'critic REVIEW B = B1b': rb == load(SA + 'B1b-blind-op-review-api-sweep.json'),
        'census critic input = B2a..B2d entries': emb == exp,
        'blind rule in B1a, B1b, critic': blind,
        'compiler body = prompts.json compile': body(tasks['sa1-compile (contract only)']['TASK_TEXT']) == prompts['compile'],
        'comparator body = compare_template with LIST 1 = compact sa1-compiled-operations.json':
            body(tasks['sa1-compare (vs T3 and R-list)']['TASK_TEXT']) ==
            prompts['compare_template'].replace('"<LIST 1 JSON>"', compact),
        'all checks recorded at extraction PASS': all(c['RESULT'] == 'PASS' for c in ti['CHECKS']),
    }
    sub.update(('declared framing (R2:BA06V4-N05, section 2 row 5): ' + k, v) for k, v in framing.items())
    record('BA06-C10', 'the published task inputs are internally consistent and reproduce, from repository files, the checks '
           'TI-CRIT-A, TI-CRIT-B, TI-CRIT-CENSUS, TI-BLIND, TI-COMPILE and TI-LIST1 (the TI-EV checks compare with the local '
           'workflow record and are not reproducible from the repository); the framing that section 2 row 5 declares is in the '
           'task texts as stated', all(sub.values()),
           '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub.items()))

    # BA06-C11: section 5 cites no superseded R-nn
    s5 = md[md.index('## 5. Residual facts'):md.index('## 6. Mutable static-state census')]
    rr = sorted(set(re.findall(r'\bR-\d\d\b', s5)))
    record('BA06-C11', 'the residual table of section 5 cites no row of the superseded V2 R-list', not rr, 'found %s' % rr)

    # BA06-C12: header identity (coordination sheet section 1)
    hdr = md[:md.index('## 1.')]
    b4 = raw(V4)
    v4_blob = blob(b4)
    checks12 = {
        'ARTIFACT_VERSION cites the git blob of the V4 file':
            ('ARTIFACT_VERSION          = 5-DRAFT (supersedes 4-DRAFT, blob %s)' % v4_blob) in hdr,
        'AUTHORITY_CONTRACT': 'CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3, textually verified) + V2.5 (draft revision 2);'
                              in hdr and 'V2.5 pending Coordinator textual verification' in hdr,
        'DELTA RULING APPLIED': 'DELTA RULING APPLIED      = decisions section 217 (' in hdr,
        'DEPENDS_ON = BA-01, BA-03, BA-05, BA-07 (correction pass (final): the edge BA-06 -> BA-07)':
            'DEPENDS_ON                = BA-01, BA-03, BA-05, BA-07 (registry entries only;' in hdr,
        'no CANDIDATE_HASH or SEAL_HASH': 'CANDIDATE_HASH' not in hdr and 'SEAL_HASH' not in hdr,
        'last section is Delta V5': [l for l in lines if l.startswith('## ')][-1] == '## 17. Delta V5 (decisions section 217)',
    }
    record('BA06-C12', 'the header cites the git blob of the V4 file, the effective contract and section 217, lists DEPENDS_ON = '
           'BA-01, BA-03, BA-05, BA-07 exactly (correction pass (final): the edge BA-06 -> BA-07), has no CANDIDATE_HASH or SEAL_HASH '
           'field, and the artifact ends with the Delta V5 section',
           all(checks12.values()), 'V4 blob %s; ' % v4_blob + '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL')
                                                                         for k, v in checks12.items()))

    # BA06-C13: STATIC_REVIEWED carry-over (AR4-12 condition (2)): the frozen sections of V5 equal those of V4 except the
    # declared labels and the insertion of declared limit (4)
    dec_text = text(DECISIONS)
    s217 = dec_text[dec_text.index('\n## 217. '):dec_text.index('\n## 218. ')]
    ar412 = [l for l in s217.split('\n') if l.startswith('| AR4-12 |')]
    appr_sha = re.search(r'SHA-256 LF `([0-9a-f]{64})`', ar412[0]).group(1) if len(ar412) == 1 else None
    appr_blob = re.search(r'blob `([0-9a-f]{7,40})`', ar412[0]).group(1) if len(ar412) == 1 else None
    v4_ok = appr_sha == sha256(b4) and appr_blob is not None and v4_blob.startswith(appr_blob)
    part_reports = []
    all_parts_ok = True
    limit4 = None
    for name, start, end in FROZEN:
        i4, l4 = span(lines4, start, end)
        i5, l5 = span(lines, start, end)
        t4, t5 = '\n'.join(l4), '\n'.join(l5)
        norm, counts = relabel(t4)
        rep = {'PART': name, 'V4_LINES': '%d-%d' % (i4 + 1, i4 + len(l4)), 'V5_LINES': '%d-%d' % (i5 + 1, i5 + len(l5)),
               'DECLARED_LABEL_SUBSTITUTIONS': counts}
        if name == '7':
            cand = [l for l in l5 if l.startswith(LIMIT4_LINE)]
            if len(cand) == 1 and cand[0].count(LIMIT4_START) == 1:
                limit4 = cand[0][cand[0].index(LIMIT4_START):]
                t5_wo = t5.replace(limit4, '', 1)
                rep['INSERTION (declared limit (4), AR4-12 condition (1))'] = limit4
            else:
                t5_wo = None
            ok = t5_wo == norm
        else:
            ok = t5 == norm
        changed = []
        if len(l4) == len(l5):
            for k, (a, b) in enumerate(zip(l4, l5)):
                if a != b:
                    if limit4 is not None and name == '7' and limit4 in b:
                        e = edits(a, b.replace(limit4, '', 1)) + [{'OP': 'INSERT (declared limit (4))', 'V4': '', 'V5': limit4,
                                                                   'V5_CONTEXT': 'end of the declared-limits paragraph'}]
                    else:
                        e = edits(a, b)
                    changed.append({'V5_LINE': i5 + k + 1, 'EDITS': e})
        else:
            ok = False
        rep['IDENTICAL_EXCEPT_DECLARED'] = ok
        rep['CHANGED_LINES'] = changed
        all_parts_ok = all_parts_ok and ok
        part_reports.append(rep)
    total = {}
    for rep in part_reports:
        for k, v in rep['DECLARED_LABEL_SUBSTITUTIONS'].items():
            total[k] = total.get(k, 0) + v
    record('BA06-C13', 'the V4 file is the one AR4-12 approved (LF SHA-256 and blob prefix of the AR4-12 row of decisions section '
           '217), and the frozen sections of V5 (3 to 3.2.5, 3.3, 7, 10.2, 11) equal those of V4 after the closed list of declared '
           'label substitutions, except the insertion of declared limit (4) in section 7; every changed line is reported. The '
           'frozen sections are not the whole inventory: its components of sections 5 and 12 are outside them (BA06-C14)',
           v4_ok and all_parts_ok and limit4 is not None,
           'V4 SHA-256 %s, blob %s; approved SHA-256 %s, blob %s; parts identical except declared: %s; label substitutions %s' %
           (sha256(b4), v4_blob, appr_sha, appr_blob, ', '.join('%s=%s' % (r['PART'], r['IDENTICAL_EXCEPT_DECLARED'])
                                                               for r in part_reports), total),
           {'LABELS': [{'CATEGORY': c, 'V4': a, 'V5': b} for c, a, b in LABELS], 'PARTS': part_reports})

    # BA06-C14: records outside the frozen sections that the rulings change (3.2.6, section 5, section 12)
    rep14 = {}
    ok14 = True
    _, cq4 = table_in(lines4, '| Id | Question | Source | Reading used in this artifact until answered |')
    cq_same = len(cq4) == len(cqs) == 8 and all(a == b[:4] for a, b in zip(cq4, cqs))
    cq_ruled = all(re.search(r'AR4-(01|12)', r[4]) and re.search(r'\*\*(ADOPTED|RULED|DEFERRED|MOOT)\*\*', r[4]) for r in cqs)
    rep14['3.2.6'] = {'CELLS_OF_COLUMNS_1_TO_4_EQUAL_V4': cq_same, 'EVERY_ROW_HAS_A_RULING_OR_DEFERRAL': bool(cq_ruled),
                      'RULINGS': {r[0]: r[4] for r in cqs}}
    ok14 = ok14 and cq_same and bool(cq_ruled)

    def compare_tables(header, declared, key_col=0, fixed_cols=()):
        _, a = table_in(lines4, header)
        _, b = table_in(lines, header)
        rows = []
        ok = len(a) == len(b)
        for ra_, rb_ in zip(a, b):
            ra_n = [relabel(c)[0] for c in ra_]
            if ra_n != rb_:
                k = next((d for d in declared if ra_[key_col].startswith(d)), None)
                rows.append({'KEY': ra_[key_col][:70], 'DECLARED_BY': declared.get(k) if k else None,
                             'CHANGED_CELLS': [{'COLUMN': ci + 1, 'V4_CELL': x, 'V5_CELL': y, 'EDITS': edits(x, y)}
                                               for ci, (x, y) in enumerate(zip(ra_, rb_)) if x != y]})
                ok = ok and k is not None and all(ra_n[c] == rb_[c] for c in fixed_cols)
            ok = ok and ra_[key_col][:30] == rb_[key_col][:30]
        return ok, {'ROWS_V4': len(a), 'ROWS_V5': len(b), 'CHANGED_ROWS': rows}

    ok5, r5 = compare_tables('| residual | affects | why not statically visible |', DECLARED_S5, 0, (1,))
    p4 = md4[md4.index('Each residual is expected to surface'):md4.index('## 6. Mutable static-state census')].strip()
    p5 = md[md.index('Each residual is expected to surface'):md.index('## 6. Mutable static-state census')].strip()
    reasons = all(('(%s) ' % x) in p5 for x in 'abcde') and 'AR4-15' in p5
    r5['PARAGRAPH_V4'] = p4
    r5['PARAGRAPH_V5'] = p5
    r5['PARAGRAPH_EDITS'] = edits(p4, p5)
    r5['PARAGRAPH_STATES_AR4_15_REASONS_A_TO_E'] = reasons
    rep14['5'] = r5
    ok12, r12 = compare_tables('| Field | Content |', DECLARED_S12, 0, ())
    rep14['12'] = r12
    ok14 = ok14 and ok5 and reasons and ok12
    record('BA06-C14', 'outside the frozen sections, the content changes are only these: in the review record 3.2.6, column 5 '
           '(columns 1 to 4 keep the V4 cells); in the inventory component section 5 (residuals), the host-defaults row (AR4-07) '
           'and the failure-path row (AR4-15), with the same residual rows and the same "affects" cells, and the paragraph, which '
           'states the reasons (a) to (e) of AR4-15; in the inventory component section 12 (EVM schema), the row '
           'INVENTORY_REVIEW_RECORDS (R1:BA06V4-N02, R2:BA06V4-N04), with the same fields in the same order. No operation '
           'class, row set, "affects" cell or EVM field changes; the content edits of sections 5 and 12 are inventory edits '
           'reported for the Architect\'s exact delta check (AR4-12 condition (2))',
           ok14, '3.2.6 %s; section 5 %s (inventory component; changed rows %d, paragraph changed %s); section 12 %s '
           '(inventory component; changed rows %d)' %
           (cq_same and bool(cq_ruled), ok5 and reasons, len(r5['CHANGED_ROWS']), p4 != p5, ok12, len(r12['CHANGED_ROWS'])),
           rep14)

    # BA06-C15: declared limit (4) as AR4-12 condition (1) states it
    ph = {p: (limit4 is not None and p in limit4) for p in LIMIT4_PHRASES}
    ar_ok = len(ar412) == 1 and '(4)' in ar412[0] and 'BA06V4-N03' in ar412[0]
    record('BA06-C15', 'section 7 declared limit (4) states the four elements of AR4-12 condition (1): the same source SHA; the '
           'harness switches off, every I-14 writer included, except the RECORDED_NOT_ENFORCED handling of the AR3-01 set; both '
           'build identities and this attestation in the REVIEWED record; an event class due only to a harness difference is a '
           'finding, not an inventory entry', all(ph.values()) and ar_ok,
           'AR4-12 row cites limit (4) of BA06V4-N03: %s; ' % ar_ok + '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL')
                                                                              for k, v in ph.items()))

    # BA06-C16: no round-V4 open question left pending outside the V4 history of section 15
    i15, l15 = span(lines, '## 15. ', '## 16. ')
    hist = set(range(i15, i15 + len(l15)))
    cq_head = [k for k, l in enumerate(lines) if l.startswith('| Id | Question | Source | Reading used in V4 |')][0]
    cq_rows_idx = set(range(cq_head + 2, cq_head + 2 + len(cqs)))  # V4 cells kept verbatim in columns 1 to 4 of 3.2.6
    bad16, n16 = [], 0
    for k, l in enumerate(lines):
        if k in hist:
            continue
        for q in re.findall(r'AQ-V4-\d\d', l):
            n16 += 1
            if q not in RULED or RULED[q] not in l:
                bad16.append('line %d: %s' % (k + 1, q))
        if k not in cq_rows_idx and re.search(r'pending AQ-V4|open question AQ-V4|open question \*\*AQ-V4|Proposal pending', l):
            bad16.append('line %d: pending wording' % (k + 1))
    record('BA06-C16', 'outside the V4 history of section 15, every mention of a round-V4 open question (AQ-V4-01, AQ-V4-11, '
           'AQ-V4-14) is on a line that cites the decision of section 217 that rules it (AR4-01, AR4-12, AR4-15), and no line '
           'calls one pending or open, except the V4 cells kept verbatim in columns 1 to 4 of 3.2.6, whose row carries the ruling '
           'in column 5', not bad16, 'mentions %d; problems %s' % (n16, bad16))

    # BA06-C17: the Delta V5 table covers every BA-06 finding of both passes of the Architect's review (section 217)
    rv_raw = raw(REVIEW)
    rv_sha_stated = re.search(r'I-52-ct21d-phase2-delta2-architect-review\.json` \(SHA-256 `([0-9a-f]{64})`\)', s217)
    rv = json.loads(rv_raw.decode('utf-8'))
    want = []
    for run, pre in (('run1', 'R1:'), ('run2', 'R2:')):
        for r in rv['runs'][run]['reviews']:
            if r.get('key') == 'BA06':
                want += [pre + f['id'] for f in r['findings']]
    _, d5 = table('| Finding or item | Decision (section 217) | Fix (section / field) |')
    status = {}
    for w in want:
        rows = [r for r in d5 if r[0] == w]
        status[w] = rows[0][2].split(':')[0].split(' ')[0] if len(rows) == 1 else 'MISSING'
    ok17 = (rv_sha_stated is not None and rv_sha_stated.group(1) == sha256(rv_raw) and len(want) == 10 and
            all(s in ('FIXED', 'NOT_ADDRESSED') for s in status.values()))
    record('BA06-C17', 'the review evidence has the SHA-256 that decisions section 217 states, and every BA-06 finding of both '
           'passes (R1:, R2:) has one row in the Delta V5 table whose fix starts with FIXED or NOT_ADDRESSED', ok17,
           'review SHA-256 stated %s; findings %s' % (rv_sha_stated.group(1) if rv_sha_stated else None,
                                                      ', '.join('%s=%s' % kv for kv in status.items())))

    # BA06-C18 (correction pass of round V5): the statements that the correction pass made
    s1 = md[md.index('## 1. Two-stage review'):md.index('## 2. The independent static review')]
    s9 = md[md.index('## 9. Learning-corpus definition'):md.index('## 10. Validation-corpus definition')]
    s101 = md[md.index('### 10.1 Rules'):md.index('### 10.2 ')]
    _, s12rows = table('| Field | Content |')
    irr = [r[1] for r in s12rows if r[0] == '`INVENTORY_REVIEW_RECORDS`']
    _, s13rows = table('| Item | State |')
    inv13 = [r[1] for r in s13rows if r[0] == 'design-time inventory']
    _, q161 = table('| Id | Question (as recorded for the Architect) | Where its ruling can change these bytes | State |')
    item2 = [l for l in s9.split('\n') if l.startswith('- **`REVIEWED` before the first `LEARNING` run')]
    composed = [l for l in s9.split('\n') if l.startswith('- **`EV-L-COMPOSED` is outside the learning corpus')]
    pre = hdr[hdr.index('> PREREQUISITES'):hdr.index('> INVENTORY_REVIEW_STATUS')]
    pre = re.sub(r'\n>\s+', ' ', pre)
    pre3 = pre[pre.index('(3) '):] if '(3) ' in pre else ''
    aq_ids = ('AQ-V5-01', 'AQ-V5-03', 'AQ-V5-04', 'AQ-V5-07')
    deps = md[md.index('**Dependencies (AR3-25).**'):md.index('## 1. Two-stage review')]
    driver = 'present in every `PRODUCT_BUILD` run and inert outside `EV-L-*`'
    sub18 = {
        'section 9 item (2): the REVIEW_RECORD_ADDED index is at most the h of the run-start anchor of every EV-L-OC-* run':
            len(item2) == 1 and ('`REVIEW_RECORD_ADDED` entry that custodies the `REVIEWED` record in BA-07 has an index at most '
                                 'the `h` of the run-start anchor (`IN_USE.runStartAnchor`) of every `EV-L-OC-*` run') in item2[0],
        'no text of sections 1, 9 and 12 bases the order on the index of the IN_USE entry':
            len(irr) == 1 and not any('lower custody index than the `IN_USE` entry' in t or 'precedes the `IN_USE` entry' in t
                                      for t in (s1, s9, irr[0])),
        'section 12 row INVENTORY_REVIEW_RECORDS points to section 9 item (2) and the run-start anchor':
            len(irr) == 1 and 'section 9, item (2)' in irr[0] and '`IN_USE.runStartAnchor`' in irr[0],
        'driver wording in section 9 and in section 10.1': driver in s9 and driver in s101,
        'EV-L-COMPOSED bullet of section 9: a full composed plan, no mirror product run, runs by pointer to BA-10 V5 2.2':
            len(composed) == 1 and 'full mirror of' not in composed[0] and 'full composed plan of `FX-1F`' in composed[0] and
            'no mirror product run' in composed[0] and 'BA-10 V5 section 2.2' in composed[0] and 'Q-V5-BA04-3' in composed[0],
        'section 13 inventory row: no "unchanged from V4"; names the section 5 and section 12 edits and AQ-V5-07':
            len(inv13) == 1 and 'unchanged from V4' not in inv13[0] and all(x in inv13[0] for x in (
                'in section 5, the host-defaults row (AR4-07), the failure-path row (AR4-15) and the paragraph',
                'in section 12, the row `INVENTORY_REVIEW_RECORDS`', 'BA06-C14', 'AQ-V5-07')),
        'section 16.1 has exactly the rows AQ-V5-03, AQ-V5-07, AQ-V5-04, AQ-V5-01, each OPEN with a BEFORE CANDIDATE gate':
            sorted(r[0].split(' ')[0] for r in q161) == sorted(aq_ids) and
            all(r[3].startswith('OPEN') and 'BEFORE CANDIDATE' in r[3] for r in q161),
        'header PREREQUISITES (3) names the four questions with a BEFORE CANDIDATE gate':
            bool(pre3) and all(q in pre3 for q in aq_ids) and pre3.rstrip().endswith('gate BEFORE CANDIDATE'),
        'no sentence claims that no open question remains':
            not re.search(r'no (Architect )?open question remains|no other Architect item is open|no open Architect question',
                          md, re.I),
        'the Delta V5 table has rows labelled correction pass': sum(1 for r in d5 if r[1].startswith('correction pass')) >= 1,
        'correction pass (final): the Dependencies paragraph takes the run-start anchor condition of section 9 item (2) normatively from BA-07':
            'from four registry entries' in deps and 'and BA-07 (' in deps and all(x in deps for x in (
                '`IN_USE.runStartAnchor`', 'its `h`', 'U1 to U3', 'BA-07 V5 sections 6.1 and 7', 'BA-07 V5 section 5.1', 'acyclic')),
        'correction pass (final): the section 9 cross-note cites BA-07 V5 section 5.1 and 6.2 step 5(c) (REVIEW_AFTER_LEARNING_START)':
            'BA-07 V5 states this order in section 5.1' in s9 and '`REVIEW_AFTER_LEARNING_START`' in s9 and
            'step 5(c) of its verification algorithm (6.2)' in s9 and 'the use preconditions of step 5' not in s9,
        'correction pass (final): the Delta V5 table has exactly one row labelled correction pass (final)':
            sum(1 for r in d5 if r[0].startswith('correction pass (final)')) == 1,
    }
    record('BA06-C18', 'the correction pass of round V5 is stated as the Delta V5 table says: section 9 item (2) binds the order '
           '"REVIEWED before the first LEARNING run" to the run-start anchor of every EV-L-OC-* run, and section 12 points to it; '
           'sections 9 and 10.1 state the driver present in every PRODUCT_BUILD run and inert outside EV-L-*; section 9 states '
           'EV-L-COMPOSED as a composed plan with no mirror product run; the section 13 inventory row names the content edits of '
           'sections 5 and 12; the open Architect questions AQ-V5-01, AQ-V5-03, AQ-V5-04 and AQ-V5-07 are OPEN in section 16.1 '
           'and before-candidate prerequisites in the header, and no sentence claims that no open question remains; correction pass '
           '(final): the Dependencies paragraph takes the run-start anchor condition from BA-07, the section 9 cross-note cites BA-07 V5 '
           'section 5.1 and 6.2 step 5(c), and the Delta V5 table has its correction pass (final) row',
           all(sub18.values()), '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub18.items()))

    out = {
        'CHECKER': SELF,
        'CHECKER_SHA256': sha256(raw(SELF)),
        'ARTIFACT': ART,
        'COMPARED_WITH': V4,
        'NOTE': 'no hash of the V5 artifact is recorded here: the artifact binds this output by hash (section 14)',
        'RESULTS': results,
        'ALL_PASS': all(r['RESULT'] == 'PASS' for r in results),
    }
    data = json.dumps(out, ensure_ascii=False, indent=1) + '\n'
    io.open(out_path, 'w', encoding='utf-8', newline='\n').write(data)
    for r in results:
        print(r['ID'], r['RESULT'], r['DETAIL'][:200])
    return 0 if out['ALL_PASS'] else 1


if __name__ == '__main__':
    sys.exit(main())
