"""Recomprobacion v2: sondas propias sobre lo nuevo (lista cerrada de literales y --task), en copias coherentes de los datos.
Uso (cwd = checkout del repo): python -I -B probes_v2.py <dir-v2>
Solo escribe en <dir-v2>/review/probes/."""
import hashlib, json, os, shutil, subprocess, sys

V = sys.argv[1]
OUT = os.path.join(V, 'review', 'probes')
SCRIPT = os.path.join(V, 'nc2_exact_whitelist_substitute_f4b_v2.py')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
SHAS = {'pbt2a': ('17872be8179475a3c080a9df158488a4a7b4f870', '5ddeb3b4ed99cb17d05142c67d47a26e89e238f9'),
        'pbt3a': ('485b5651bcec132761347b40ed38b60cfec0db04', '6919e17af9fdd5c702d454a97a3c5b6cc6be1739')}
sys.path.insert(0, V)
import importlib.util
_spec = importlib.util.spec_from_file_location('sub_v2', SCRIPT)
_m = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_m)
EV_T2, EV_T3 = _m.F4B_EXACT_EVIDENCES
sys.path.pop(0)


def rb(p):
    with open(p, 'rb') as f:
        return f.read()


def wb(p, b):
    with open(p, 'wb') as f:
        f.write(b)


def fresh(name, src):
    d = os.path.join(OUT, name)
    if os.path.exists(d):
        shutil.rmtree(d)
    for sub in ('real', 'nc2', 'planning'):
        shutil.copytree(os.path.join(V, 'data', src, sub), os.path.join(d, sub))
    return d


def rep_hash(b, old, new):
    """Sustituye un SHA en mayusculas o minusculas (al menos una vez)."""
    n = 0
    for o, w in ((old.upper(), new.upper()), (old.lower(), new.lower())):
        n += b.count(o.encode())
        b = b.replace(o.encode(), w.encode())
    assert n >= 1, old
    return b


def field_hash(v):
    return sha(json.dumps(v, ensure_ascii=False, separators=(',', ':')).encode('utf-8'))


def regen(d, ct_edit=None, deleg_edit=None, raw_edit=None, ct_bytes_edit=None):
    """Aplica el cambio y recalcula TODO lo derivado (bfc, nota, relevos, mutation.json) para que la copia sea coherente."""
    P = lambda *p: os.path.join(d, *p)
    ct_old, rd_old, md_old = rb(P('real', 'inputs', 'gate-contract.json')), rb(P('real', 'inputs', 'delegation.json')), rb(P('nc2', 'inputs', 'delegation.json'))
    raw_old, bfc_old = rb(P('planning', 'output.json')), rb(P('real', 'inputs', 'byte-fidelity-check.json'))
    rr0_old, rr1_old = rb(P('real', 'inputs', 'relay-record-planning.json')), rb(P('nc2', 'inputs', 'relay-record-planning.json'))
    ct_new = ct_old
    if ct_bytes_edit:
        ct_new = ct_bytes_edit(ct_old)
    elif ct_edit:
        o = json.loads(ct_old); ct_edit(o); ct_new = enc(o)
    rd_new, md_new = rd_old, md_old
    if deleg_edit:
        assert enc(json.loads(rd_old)) == rd_old, 'la delegacion real no es json.dumps(indent=2)'
        o = json.loads(rd_old); deleg_edit(o); rd_new = enc(o)
        o = json.loads(md_old); deleg_edit(o); md_new = enc(o)
    raw_new = raw_old
    if raw_edit:
        o = json.loads(raw_old); raw_edit(o)
        raw_new = json.dumps(o, ensure_ascii=False, indent=2).encode('utf-8') + (b'\n' if raw_old.endswith(b'\n') else b'')
    bfc_new = bfc_old
    if ct_new != ct_old or rd_new != rd_old:
        bo, cto, rdo = json.loads(bfc_old), json.loads(ct_new), json.loads(rd_new)
        cto_old, rdo_old = json.loads(ct_old), json.loads(rd_old)
        bo['OutputSha256'], bo['ContractSha256'] = sha(rd_new).lower(), sha(ct_new).lower()
        for f in bo.get('Fields', []):
            k = f['Field']
            if f.get('SourceSha256', '').upper() == field_hash(cto_old.get(k)):
                f['SourceSha256'] = field_hash(cto.get(k)).lower()
            if f.get('OutputSha256', '').upper() == field_hash(rdo_old.get(k)):
                f['OutputSha256'] = field_hash(rdo.get(k)).lower()
        bfc_new = enc(bo)
    rr0_new = rep_hash(rr0_old, sha(raw_old), sha(raw_new)) if raw_new != raw_old else rr0_old
    rr1_new = rep_hash(rr1_old, sha(md_old), sha(md_new)) if md_new != md_old else rr1_old
    for side in ('real', 'nc2'):
        wb(P(side, 'inputs', 'gate-contract.json'), ct_new)
        wb(P(side, 'inputs', 'byte-fidelity-check.json'), bfc_new)
    wb(P('planning', 'byte-fidelity-check.json'), bfc_new)
    wb(P('real', 'inputs', 'delegation.json'), rd_new)
    wb(P('nc2', 'inputs', 'delegation.json'), md_new)
    wb(P('planning', 'output.json'), raw_new)
    wb(P('real', 'inputs', 'relay-record-planning.json'), rr0_new)
    wb(P('nc2', 'inputs', 'relay-record-planning.json'), rr1_new)
    m = rb(P('nc2', 'mutation.json'))
    for o, n in ((md_old, md_new), (ct_old, ct_new), (rr1_old, rr1_new), (bfc_old, bfc_new), (raw_old, raw_new)):
        if o != n and (sha(o).encode() in m or sha(o).lower().encode() in m):
            m = rep_hash(m, sha(o), sha(n))
    wb(P('nc2', 'mutation.json'), m)
    note = json.loads(rb(P('planning', 'session-inject-note.json')))
    note.update(DelegationSha256=sha(rd_new), ContractSha256=sha(ct_new), OutputSha256=sha(raw_new))
    wb(P('planning', 'session-inject-note.json'), enc(note))


def set_ee0(value):
    def f(o): o['ExpectedEvidence'][0] = value
    return f


def move_ee0_last(o):
    ee = o['ExpectedEvidence']
    o['ExpectedEvidence'] = ee[1:] + ee[:1]


def set_task(tid):
    def f(o): o['TaskId'] = tid
    return f


def escape_first_char(b):
    old = json.dumps(EV_T3, ensure_ascii=False).encode('utf-8')
    assert b.count(old) == 1
    return b.replace(old, b'"\\u0061' + old[2:])


def nothing(d):
    pass


T3B = 'F4B-PBT3B-FULL-WRITE-MODEL'
CASES = [
    # (nombre, datos, preparacion, --task (lista de argumentos antes de los posicionales), esperado ideal, descripcion)
    ('S1-literal-not-at-0', 'pbt3a', lambda d: regen(d, ct_edit=move_ee0_last, deleg_edit=move_ee0_last), ['--task', 'PBT3A'], '?', 'el literal de PB-T3 pasa de ExpectedEvidence[0] al final (coherente)'),
    ('S2-cross-package-literal', 'pbt3a', lambda d: regen(d, ct_edit=set_ee0(EV_T2), deleg_edit=set_ee0(EV_T2)), ['--task', 'PBT3A'], '?', 'contrato de PB-T3a con el literal de PB-T2 (coherente), --task PBT3A'),
    ('S3-json-escape', 'pbt3a', lambda d: regen(d, ct_bytes_edit=escape_first_char), ['--task', 'PBT3A'], 'PASS', 'el literal escrito con un escape \\u0061 en los bytes del contrato (mismo texto decodificado)'),
    ('S4-nbsp', 'pbt3a', lambda d: regen(d, ct_edit=set_ee0(EV_T3.replace('la comparacion', 'la' + chr(0xA0) + 'comparacion')), deleg_edit=set_ee0(EV_T3.replace('la comparacion', 'la' + chr(0xA0) + 'comparacion'))), ['--task', 'PBT3A'], 'FAIL', 'un espacio cambiado por NBSP (coherente)'),
    ('S5-homoglyph', 'pbt3a', lambda d: regen(d, ct_edit=set_ee0(chr(0x430) + EV_T3[1:]), deleg_edit=set_ee0(chr(0x430) + EV_T3[1:])), ['--task', 'PBT3A'], 'FAIL', 'la "a" inicial cirilica (U+0430) (coherente)'),
    ('S6-accent', 'pbt3a', lambda d: regen(d, ct_edit=set_ee0(EV_T3.replace('comparacion', 'comparaci' + chr(0xF3) + 'n')), deleg_edit=set_ee0(EV_T3.replace('comparacion', 'comparaci' + chr(0xF3) + 'n'))), ['--task', 'PBT3A'], 'FAIL', '"comparacion" con tilde (coherente)'),
    ('S7-contract-taskid-other', 'pbt3a', lambda d: regen(d, ct_edit=set_task(T3B)), ['--task', 'PBT3A'], '?', 'el contrato dice TaskId PB-T3b; la delegacion PB-T3a; --task PBT3A (coherente)'),
    ('S8-deleg-taskid-spoof', 'pbt3a', lambda d: regen(d, deleg_edit=set_task(T3B), raw_edit=set_task(T3B)), ['--task', 'PBT3B'], '?', 'delegaciones y salida cruda con TaskId PB-T3b; contrato, relevos, bfc y salidas siguen en PB-T3a; --task PBT3B'),
    ('S9-validation', 'pbt3a', nothing, ['--task', 'VALIDATION'], '?', '--task VALIDATION sobre PB-T3a'),
    ('S10-lowercase-task', 'pbt3a', nothing, ['--task', 'pbt3a'], 'PASS', '--task en minusculas'),
    ('S11-wrong-task-pbt2a', 'pbt2a', nothing, ['--task', 'PBT3A'], 'FAIL', '--task PBT3A sobre datos de PB-T2a'),
    ('S12-task-flag-no-value', 'pbt3a', nothing, ['@END', '--task'], 'FAIL', '--task al final sin valor'),
    ('S13-task-twice', 'pbt3a', nothing, ['--task', 'PBT3B', '--task', 'PBT3A'], '?', '--task dos veces (PBT3B y luego PBT3A)'),
    ('S14-pbt2a-literal-on-pbt2a', 'pbt2a', nothing, ['--task', 'PBT2A'], 'PASS', 'control: PB-T2a con su literal'),
]

os.makedirs(OUT, exist_ok=True)
summary = []
for name, src, prep, targs, expect, desc in CASES:
    d = fresh(name, src)
    prep(d)
    out = os.path.join(OUT, name + '.json')
    if os.path.exists(out):
        os.remove(out)
    pos = [os.path.join(d, 'real'), os.path.join(d, 'nc2'), SHAS[src][0], SHAS[src][1], out,
           os.path.join(d, 'planning', 'session-inject-note.json'), os.path.join(d, 'planning', 'output.json')]
    args = pos + targs[1:] if targs and targs[0] == '@END' else targs + pos
    cp = subprocess.run([sys.executable, '-I', '-B', SCRIPT] + args, capture_output=True, text=True, encoding='utf-8')
    with open(os.path.join(OUT, name + '.txt'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('exit=%d\nargs=%r\n--- stdout\n%s\n--- stderr\n%s\n' % (cp.returncode, args, cp.stdout, cp.stderr))
    r = json.loads(rb(out)) if os.path.exists(out) else {}
    pr, me = r.get('Properties') or {}, r.get('Measured') or {}
    summary.append({'Case': name, 'Description': desc, 'IdealExpected': expect, 'Exit': cp.returncode, 'SubstituteResult': r.get('SubstituteResult', 'NO OUTPUT: ' + cp.stderr.strip()[-150:]),
                    'Id': r.get('Id'), 'Task': r.get('Task'), 'FalseProps': sorted(k for k, v in pr.items() if not v), 'Matched': me.get('ExactEvidenceMatched'),
                    'BytesEq': me.get('RawPlusContractCollectionsEqualsDelegationBytes'), 'Binding': r.get('TaskBinding'), 'Record': (r.get('Record') or {}).get('nc2'), 'Scope': r.get('Scope')})
with open(os.path.join(OUT, 'probes-summary.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(summary, ensure_ascii=False, indent=1) + '\n')
for s in summary:
    print('%-28s ideal=%-5s got=%-5s exit=%d false=%s matched=%s bytesEq=%s bind=%s record=%s id=%s' % (
        s['Case'], s['IdealExpected'], s['SubstituteResult'][:60], s['Exit'], s['FalseProps'], s['Matched'], s['BytesEq'],
        (s['Binding'] or {}).get('Ok'), s['Record'], s['Id']))
