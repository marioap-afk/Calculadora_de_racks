"""I-52 CT-21D baseline artifact BA-06 V4: reproducible checks of the statements that BA-06 V4 makes about itself.

Usage: python I-52-ct21d-ba06-v4-checks.py <repository root> <output file>

Reads only repository files (the artifact, the V3 artifact, the contract texts and the evidence files it binds) and writes one
JSON result file. Fails closed: a missing input, a table that cannot be parsed or any failed check gives exit code 1. The output
records no hash of the artifact, because the artifact binds this output by hash (section 14). Deterministic: no clock, no network.
Hashes: SHA-256 of the LF-normalized bytes, lowercase hexadecimal; git blob ids: SHA-1 of "blob <size>\\0" + LF-normalized bytes.
"""
import hashlib
import io
import json
import os
import re
import sys

ART = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v4.md'
V3 = 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v3.md'
SELF = 'docs/automation/evidence/I-52-ct21d-ba06-v4-checks.py'
OUTPUT_NAME = 'I-52-ct21d-ba06-v4-checks-output.json'
DL = 'docs/automation/evidence/I-52-ct21d-phase2-delta/'
SA = 'docs/automation/evidence/I-52-ct21d-phase2-static-analysis/'
TI = 'docs/automation/evidence/I-52-ct21d-ba06-v4-task-inputs.json'
CONTRACT = {'V2.1': 'docs/initiatives/I-52-ct21d-authority-contract-v2.1.md',
            'V2.2': 'docs/initiatives/I-52-ct21d-authority-contract-v2.2.md',
            'V2.4': 'docs/initiatives/I-52-ct21d-authority-contract-v2.4.md'}
FRAG_V21 = {'4.2', '7', '7.1', '15.2', '15.3', '15.4', '15.5', '15.6', '15.7', '19.1', '27.2'}
FRAG_V22 = {'PA-6', 'PA-10'}
MARK = 'The computed task text follows:\n'


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

    results = []

    def record(cid, statement, ok, detail=''):
        results.append({'ID': cid, 'STATEMENT': statement, 'RESULT': 'PASS' if ok else 'FAIL', 'DETAIL': detail})

    md = text(ART)
    lines = md.split('\n')

    def table(header_start):
        idx = [i for i, l in enumerate(lines) if l.startswith(header_start)]
        if len(idx) != 1:
            raise SystemExit('table header not unique or missing: ' + header_start)
        i = idx[0]
        head = [c.strip() for c in lines[i].strip().strip('|').split(' | ')]
        rows = []
        j = i + 2
        while j < len(lines) and lines[j].startswith('|'):
            cells = [c.strip() for c in lines[j].strip()[2:-2].split(' | ')]
            if len(cells) != len(head):
                raise SystemExit('row with %d cells under %s: %s' % (len(cells), header_start, lines[j][:80]))
            rows.append(cells)
            j += 1
        return head, rows

    def esc(t):
        return t.replace('|', '/').replace('\n', ' ')

    # BA06-C1: matrix columns = the classes of section 10.2
    head, mrows = table('| Entry point | SH-F1 |')
    cols = set(re.sub(r' \(\d\)$', '', c) for c in head[1:])
    _, shapes = table('| Class | Description | Fixture (BA-08 V4) |')
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
    _, octab = table('| Class | Operations | Actor and phase | Micro-scenario (BA-04 V4) or reasoned exclusion |')
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
    _, cqs = table('| Id | Question | Source | Reading used in this artifact until answered |')
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
           len(ev) == 26 and not wrong, 'rows %d; mismatches %s' % (len(ev), wrong))

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
    record('BA06-C10', 'the published task inputs are internally consistent and reproduce, from repository files, the checks '
           'TI-CRIT-A, TI-CRIT-B, TI-CRIT-CENSUS, TI-BLIND, TI-COMPILE and TI-LIST1 (the TI-EV checks compare with the local '
           'workflow record and are not reproducible from the repository)', all(sub.values()),
           '; '.join('%s: %s' % (k, 'PASS' if v else 'FAIL') for k, v in sub.items()))

    # BA06-C11: section 5 cites no superseded R-nn
    s5 = md[md.index('## 5. Residual facts'):md.index('## 6. Mutable static-state census')]
    rr = sorted(set(re.findall(r'\bR-\d\d\b', s5)))
    record('BA06-C11', 'the residual table of section 5 cites no row of the superseded V2 R-list', not rr, 'found %s' % rr)

    # BA06-C12: header identity
    hdr = md[:md.index('## 1.')]
    v3 = raw(V3)
    blob = hashlib.sha1(b'blob %d\x00' % len(v3) + v3).hexdigest()
    ok12 = ('ARTIFACT_VERSION          = 4-DRAFT (supersedes 3-DRAFT, blob %s, which stays as history)' % blob) in hdr
    ok12 = ok12 and 'CANDIDATE_HASH' not in hdr and 'SEAL_HASH' not in hdr and 'DEPENDS_ON                = BA-01, BA-03, BA-05' in hdr
    record('BA06-C12', 'the header cites the git blob of the V3 file, lists DEPENDS_ON = BA-01, BA-03, BA-05 and has no '
           'CANDIDATE_HASH or SEAL_HASH field', ok12, 'V3 blob %s' % blob)

    out = {
        'CHECKER': SELF,
        'CHECKER_SHA256': sha256(raw(SELF)),
        'ARTIFACT': ART,
        'NOTE': 'no hash of the artifact is recorded here: the artifact binds this output by hash (section 14)',
        'RESULTS': results,
        'ALL_PASS': all(r['RESULT'] == 'PASS' for r in results),
    }
    data = json.dumps(out, ensure_ascii=False, indent=1) + '\n'
    io.open(out_path, 'w', encoding='utf-8', newline='\n').write(data)
    for r in results:
        print(r['ID'], r['RESULT'], r['DETAIL'][:160])
    return 0 if out['ALL_PASS'] else 1


if __name__ == '__main__':
    sys.exit(main())
