"""Recomprobacion v2: mis sondas N1-N8 de la §R (que dieron PASS en 4cd4c01f) contra la v2, sobre copias coherentes de PB-T3a.
Comprueba en datos reales las correcciones M-R1..M-R3 que entraron en 6ccaf12d despues de mi §R.
Uso (cwd = checkout del repo): python -I -B probes_v2_mr.py <dir-v2>. Solo escribe en <dir-v2>/review/probes-mr/."""
import hashlib, json, os, shutil, subprocess, sys

V = sys.argv[1]
OUT = os.path.join(V, 'review', 'probes-mr')
SCRIPT = os.path.join(V, 'nc2_exact_whitelist_substitute_f4b_v2.py')
BASE, CUR = '485b5651bcec132761347b40ed38b60cfec0db04', '6919e17af9fdd5c702d454a97a3c5b6cc6be1739'
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')


def rb(p):
    with open(p, 'rb') as f:
        return f.read()


def wb(p, b):
    with open(p, 'wb') as f:
        f.write(b)


def rep_hash(b, old, new):
    n = 0
    for o, w in ((old.upper(), new.upper()), (old.lower(), new.lower())):
        n += b.count(o.encode())
        b = b.replace(o.encode(), w.encode())
    assert n >= 1, old
    return b


def fresh(name):
    d = os.path.join(OUT, name)
    if os.path.exists(d):
        shutil.rmtree(d)
    for sub in ('real', 'nc2', 'planning'):
        shutil.copytree(os.path.join(V, 'data', 'pbt3a', sub), os.path.join(d, sub))
    return d


def P(d, *p):
    return os.path.join(d, *p)


def note_edit(d, fn):
    p = P(d, 'planning', 'session-inject-note.json')
    n = json.loads(rb(p)); fn(n); wb(p, enc(n))


def set_raw_bytes(d, new_raw):
    old = rb(P(d, 'planning', 'output.json'))
    wb(P(d, 'planning', 'output.json'), new_raw)
    p = P(d, 'real', 'inputs', 'relay-record-planning.json')
    wb(p, rep_hash(rb(p), sha(old), sha(new_raw)))
    note_edit(d, lambda n: n.update(OutputSha256=sha(new_raw)))
    m = rb(P(d, 'nc2', 'mutation.json'))
    if sha(old).encode() in m or sha(old).lower().encode() in m:
        wb(P(d, 'nc2', 'mutation.json'), rep_hash(m, sha(old), sha(new_raw)))


def raw_obj_edit(d, fn):
    old = rb(P(d, 'planning', 'output.json'))
    o = json.loads(old); fn(o)
    set_raw_bytes(d, json.dumps(o, ensure_ascii=False, indent=2).encode('utf-8') + (b'\n' if old.endswith(b'\n') else b''))


def n1(d): raw_obj_edit(d, lambda o: o.update(Attempt=float(o['Attempt'])))
def n2(d):
    def f(o):
        k = next(k for k, v in o.items() if v == 1 and type(v) is int)
        o[k] = True
    raw_obj_edit(d, f)


def n3(d):
    raw = rb(P(d, 'planning', 'output.json'))
    model = json.loads(raw)['Model']
    old = ('"Model": %s,' % json.dumps(model, ensure_ascii=False)).encode('utf-8')
    assert raw.count(old) == 1
    set_raw_bytes(d, raw.replace(old, b'"Model": "gpt-otro-modelo",\n  ' + old))


def n5(d):
    def f(o):
        del o['AllowedWriteScope']
    raw_obj_edit(d, f)


def n6(d):
    """RunId ausente en la nota, el bfc y los dos relevos (y en la delegacion queda): debe fallar."""
    note_edit(d, lambda n: n.pop('RunId', None))
    bo = json.loads(rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json')))
    old_b = rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    bo.pop('PlanningRunId', None)
    nb = enc(bo)
    for p in (P(d, 'real', 'inputs', 'byte-fidelity-check.json'), P(d, 'nc2', 'inputs', 'byte-fidelity-check.json'), P(d, 'planning', 'byte-fidelity-check.json')):
        wb(p, nb)
    m = rep_hash(rb(P(d, 'nc2', 'mutation.json')), sha(old_b), sha(nb))
    for side in ('real', 'nc2'):
        p = P(d, side, 'inputs', 'relay-record-planning.json')
        old = rb(p); r = json.loads(old); r.pop('RunId', None); new = enc(r); wb(p, new)
        if side == 'nc2':
            m = rep_hash(m, sha(old), sha(new))
    wb(P(d, 'nc2', 'mutation.json'), m)


def n8(d): note_edit(d, lambda n: n.update(Fidelity='FAIL (no PASS)'))
def ctrl(d): pass


CASES = [('MR0-control', ctrl, 'PASS'), ('MR-N1-int-as-float', n1, 'FAIL'), ('MR-N2-bool-for-int', n2, 'FAIL'), ('MR-N3-duplicate-key', n3, 'FAIL'),
         ('MR-N5-absent-collection', n5, 'FAIL'), ('MR-N6-runid-absent', n6, 'FAIL'), ('MR-N8-fidelity-substring', n8, 'FAIL')]
os.makedirs(OUT, exist_ok=True)
summary = []
for name, fn, expect in CASES:
    d = fresh(name)
    fn(d)
    out = os.path.join(OUT, name + '.json')
    if os.path.exists(out):
        os.remove(out)
    args = ['--task', 'PBT3A', P(d, 'real'), P(d, 'nc2'), BASE, CUR, out, P(d, 'planning', 'session-inject-note.json'), P(d, 'planning', 'output.json')]
    cp = subprocess.run([sys.executable, '-I', '-B', SCRIPT] + args, capture_output=True, text=True, encoding='utf-8')
    with open(os.path.join(OUT, name + '.txt'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('exit=%d\n--- stdout\n%s\n--- stderr\n%s\n' % (cp.returncode, cp.stdout, cp.stderr))
    r = json.loads(rb(out)) if os.path.exists(out) else {}
    pr, me = r.get('Properties') or {}, r.get('Measured') or {}
    summary.append({'Case': name, 'Expected': expect, 'Exit': cp.returncode, 'SubstituteResult': r.get('SubstituteResult', 'NO OUTPUT'),
                    'FalseProps': sorted(k for k, v in pr.items() if not v), 'BytesEq': me.get('RawPlusContractCollectionsEqualsDelegationBytes'),
                    'EmptyExact': me.get('RawCollectionsEmptyExact')})
with open(os.path.join(OUT, 'probes-mr-summary.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(summary, ensure_ascii=False, indent=1) + '\n')
for s in summary:
    print('%-26s exp=%-5s got=%-70s exit=%d false=%s bytesEq=%s emptyExact=%s' % (s['Case'], s['Expected'], s['SubstituteResult'][:70], s['Exit'], s['FalseProps'], s['BytesEq'], s['EmptyExact']))
