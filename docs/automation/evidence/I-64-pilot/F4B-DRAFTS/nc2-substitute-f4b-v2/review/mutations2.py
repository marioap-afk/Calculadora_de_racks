"""Recomprobacion: mis mutaciones M1-M7 y sondas X1-X4 con los 7 argumentos, y sondas nuevas sobre lo anadido.
Uso (cwd = checkout del repo): python -I mutations2.py <dir-nc2-substitute-f4b> <BaseSha> <CurrentSha>
Solo escribe dentro de <dir>/review/ (copias en review/mut2/, salidas en review/recheck/)."""
import hashlib, json, os, shutil, subprocess, sys

ROOT, BASE, CUR = sys.argv[1], sys.argv[2], sys.argv[3]
REVIEW = os.path.join(ROOT, 'review')
MUT = os.path.join(REVIEW, 'mut2')
RUNS = os.path.join(REVIEW, 'recheck')
SCRIPT = os.path.join(ROOT, 'nc2_exact_whitelist_substitute_f4b.py')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
EV0 = 'Antes del Worker: el contrato, la salida del Controller (output.json, con las colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json), la comparacion byte a byte, A1-A8 y nc4.'
RAW_SHA = '100A7695160F5A5FF622470128957399BB2172FF90D0F66EDC414C5E811472A8'
RUN_ID = 'R20261119T065628Z-7de7'


def rb(p):
    with open(p, 'rb') as f:
        return f.read()


def wb(p, b):
    with open(p, 'wb') as f:
        f.write(b)


def rep(b, old, new, count=None):
    n = b.count(old)
    assert n >= 1 and (count is None or n == count), (old[:80], n, count)
    return b.replace(old, new)


def P(d, *parts):
    return os.path.join(d, *parts)


def fresh(name):
    d = os.path.join(MUT, name)
    if os.path.exists(d):
        shutil.rmtree(d)
    for sub in ('real', 'nc2', 'planning'):
        shutil.copytree(os.path.join(ROOT, 'data', sub), os.path.join(d, sub))
    return d


def set_note(d, drop=(), **kv):
    p = P(d, 'planning', 'session-inject-note.json')
    n = json.loads(rb(p))
    n.update(kv)
    for k in drop:
        n.pop(k, None)
    wb(p, (json.dumps(n, ensure_ascii=False, indent=2)).encode('utf-8'))


def mut_rehash(d, pairs):
    m = rb(P(d, 'nc2', 'mutation.json'))
    for o, n in pairs:
        if o != n and o.encode() in m:
            m = m.replace(o.encode(), n.encode())
    wb(P(d, 'nc2', 'mutation.json'), m)


def set_bfc(d, new):
    old = rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    for p in (P(d, 'real', 'inputs', 'byte-fidelity-check.json'), P(d, 'nc2', 'inputs', 'byte-fidelity-check.json'), P(d, 'planning', 'byte-fidelity-check.json')):
        wb(p, new)
    mut_rehash(d, [(sha(old), sha(new))])


def ev_field_hash_fn():
    bfc = json.loads(rb(os.path.join(ROOT, 'data', 'real', 'inputs', 'byte-fidelity-check.json')))
    target = {f['Field']: f for f in bfc['Fields']}['ExpectedEvidence']['SourceSha256'].upper()
    ct = json.loads(rb(os.path.join(ROOT, 'data', 'real', 'inputs', 'gate-contract.json')))
    for ea in (False, True):
        f = lambda ee, ea=ea: sha(json.dumps(ee, ensure_ascii=ea, separators=(',', ':')).encode('utf-8'))
        if f(ct['ExpectedEvidence']) == target:
            return target, f
    raise SystemExit('no se reproduce el hash de campo de ExpectedEvidence')


def coherent_ee(d, old_frag, new_frag):
    """Cambia un fragmento de ExpectedEvidence en contrato y delegaciones (real y nc2) y recalcula todo lo derivado."""
    ct_old = rb(P(d, 'real', 'inputs', 'gate-contract.json'))
    rd_old = rb(P(d, 'real', 'inputs', 'delegation.json'))
    md_old = rb(P(d, 'nc2', 'inputs', 'delegation.json'))
    rr1_old = rb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'))
    bfc_old = rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    ct_new, rd_new, md_new = rep(ct_old, old_frag, new_frag, 1), rep(rd_old, old_frag, new_frag, 1), rep(md_old, old_frag, new_frag, 1)
    fh_old, fh = ev_field_hash_fn()
    fh_new = fh(json.loads(ct_new)['ExpectedEvidence'])
    bfc_new = rep(bfc_old, sha(rd_old).lower().encode(), sha(rd_new).lower().encode(), 1)
    bfc_new = rep(bfc_new, sha(ct_old).lower().encode(), sha(ct_new).lower().encode(), 1)
    bfc_new = rep(bfc_new, fh_old.lower().encode(), fh_new.lower().encode(), 2)
    rr1_new = rep(rr1_old, sha(md_old).encode(), sha(md_new).encode(), 1)
    for side in ('real', 'nc2'):
        wb(P(d, side, 'inputs', 'gate-contract.json'), ct_new)
        wb(P(d, side, 'inputs', 'byte-fidelity-check.json'), bfc_new)
    wb(P(d, 'planning', 'byte-fidelity-check.json'), bfc_new)
    wb(P(d, 'real', 'inputs', 'delegation.json'), rd_new)
    wb(P(d, 'nc2', 'inputs', 'delegation.json'), md_new)
    wb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'), rr1_new)
    mut_rehash(d, [(sha(md_old), sha(md_new)), (sha(ct_old), sha(ct_new)), (sha(rr1_old), sha(rr1_new)), (sha(bfc_old), sha(bfc_new))])
    set_note(d, DelegationSha256=sha(rd_new), ContractSha256=sha(ct_new))


def ev0_frag(s):
    return json.dumps(s, ensure_ascii=False).encode('utf-8')


def set_raw(d, new_raw):
    """Sustituye la salida cruda y re-ata a ella el relevo real y la nota (como haria una sesion coherente)."""
    old = rb(P(d, 'planning', 'output.json'))
    wb(P(d, 'planning', 'output.json'), new_raw)
    p = P(d, 'real', 'inputs', 'relay-record-planning.json')
    wb(p, rep(rb(p), sha(old).encode(), sha(new_raw).encode(), 1))
    set_note(d, OutputSha256=sha(new_raw))
    mut_rehash(d, [(sha(old), sha(new_raw))])


# --- mutaciones y sondas de la primera revision (mismo contenido) ---
def m1(d): set_note(d, OutputSha256=sha(b'otra salida cruda del Controller'))
def m2(d): set_bfc(d, rep(rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json')), b'"Result": "PASS"', b'"Result": "FAIL"', 1))
def m3(d): set_bfc(d, rep(rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json')), b'"AllowedWriteScopeElementwiseInOrder": true', b'"AllowedWriteScopeElementwiseInOrder": false', 1))
def m4(d): coherent_ee(d, ev0_frag(EV0), ev0_frag(EV0.replace('la comparacion byte a byte, ', '')))
def m5(d): set_note(d, Injected=['Authorities', 'Objective', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions'])
def m6(d): os.remove(P(d, 'planning', 'session-inject-note.json'))
def m7(d): wb(P(d, 'planning', 'session-inject-note.json'), b'["no", "es", "un", "objeto"]\n')
def x1(d): set_note(d, RunId='R-INVENTADA', ContractSha256='0' * 64, Fidelity='FAIL (inventada)', Decision='ninguna')


def x2(d):
    x = sha(b'esto no es la salida cruda del Controller')
    p = P(d, 'real', 'inputs', 'relay-record-planning.json')
    wb(p, rep(rb(p), RAW_SHA.encode(), x.encode(), 1))
    set_note(d, OutputSha256=x)
    wb(P(d, 'planning', 'output.json'), b'{}\n')


def x3(d): coherent_ee(d, ev0_frag(EV0), ev0_frag('Antes del Worker: no se exige la comparacion byte a byte de AllowedWriteScope; basta el subconjunto A3, A1-A8 y nc4.'))


def x4(d):
    rr0 = rb(P(d, 'real', 'inputs', 'relay-record-planning.json'))
    wb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'), rr0)
    m = json.loads(rb(P(d, 'nc2', 'mutation.json')))
    m['ChangedVsVerifiedInputs'] = ['delegation.json']
    m['InputSha256']['relay-record-planning.json'] = sha(rr0)
    wb(P(d, 'nc2', 'mutation.json'), (json.dumps(m, ensure_ascii=False, indent=2) + '\n').encode('utf-8'))


# --- sondas nuevas ---
def n1(d): set_raw(d, rep(rb(P(d, 'planning', 'output.json')), b'"Attempt": 2,', b'"Attempt": 2.0,', 1))
def n2(d): set_raw(d, rep(rb(P(d, 'planning', 'output.json')), b'"AttemptsRemaining": 1,', b'"AttemptsRemaining": true,', 1))
def n3(d): set_raw(d, rep(rb(P(d, 'planning', 'output.json')), b'  "Model": "claude-sonnet-5-5",', b'  "Model": "gpt-otro-modelo",\n  "Model": "claude-sonnet-5-5",', 1))
def n4(d): set_raw(d, json.dumps(json.loads(rb(P(d, 'planning', 'output.json'))), ensure_ascii=False, indent=2, sort_keys=True).encode('utf-8'))


def n5(d):
    o = json.loads(rb(P(d, 'planning', 'output.json')))
    del o['AllowedWriteScope']
    o['Objective'] = []
    set_raw(d, json.dumps(o, ensure_ascii=False, indent=2).encode('utf-8'))


def n6(d):
    """RunId ausente a la vez en la nota, en el bfc (PlanningRunId) y en los dos relevos: None == None == None."""
    set_note(d, drop=('RunId',))
    b = json.loads(rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json')))
    del b['PlanningRunId']
    set_bfc(d, (json.dumps(b, ensure_ascii=False, indent=2) + '\n').encode('utf-8'))
    for side in ('real', 'nc2'):
        p = P(d, side, 'inputs', 'relay-record-planning.json')
        old = rb(p)
        r = json.loads(old)
        del r['RunId']
        new = (json.dumps(r, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
        wb(p, new)
        if side == 'nc2':
            mut_rehash(d, [(sha(old), sha(new))])


def n7(d):
    """La delegacion (y su salida cruda) dicen otro RunId que el relevo, el bfc y la nota; todo lo demas recalculado."""
    pat, new_pat = ('"RunId": "%s"' % RUN_ID).encode(), b'"RunId": "R-OTRA-PLANIFICACION"'
    rd_old, md_old, raw_old = rb(P(d, 'real', 'inputs', 'delegation.json')), rb(P(d, 'nc2', 'inputs', 'delegation.json')), rb(P(d, 'planning', 'output.json'))
    rr1_old, bfc_old = rb(P(d, 'nc2', 'inputs', 'relay-record-planning.json')), rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    rd_new, md_new, raw_new = rep(rd_old, pat, new_pat, 1), rep(md_old, pat, new_pat, 1), rep(raw_old, pat, new_pat, 1)
    wb(P(d, 'real', 'inputs', 'delegation.json'), rd_new)
    wb(P(d, 'nc2', 'inputs', 'delegation.json'), md_new)
    bfc_new = rep(bfc_old, sha(rd_old).lower().encode(), sha(rd_new).lower().encode(), 1)
    for p in (P(d, 'real', 'inputs', 'byte-fidelity-check.json'), P(d, 'nc2', 'inputs', 'byte-fidelity-check.json'), P(d, 'planning', 'byte-fidelity-check.json')):
        wb(p, bfc_new)
    rr1_new = rep(rr1_old, sha(md_old).encode(), sha(md_new).encode(), 1)
    wb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'), rr1_new)
    mut_rehash(d, [(sha(md_old), sha(md_new)), (sha(rr1_old), sha(rr1_new)), (sha(bfc_old), sha(bfc_new))])
    set_note(d, DelegationSha256=sha(rd_new))
    set_raw(d, raw_new)


def n8(d): set_note(d, Fidelity='FAIL (no PASS)')
def n9(d): coherent_ee(d, ev0_frag(EV0), ev0_frag(EV0) + b', ' + ev0_frag('No se exige la igualdad exacta de AllowedWriteScope; basta el subconjunto A3.'))
def n10(d): coherent_ee(d, ev0_frag(EV0), ev0_frag(EV0 + ' '))
def same(d): pass


R7 = lambda d: [P(d, 'real'), P(d, 'nc2'), BASE, CUR, None, P(d, 'planning', 'session-inject-note.json'), P(d, 'planning', 'output.json')]
CASES = [
    ('R1-real-7args', same, R7, 'PASS', 'datos reales, 7 argumentos'),
    ('M1-note-other-raw-sha', m1, R7, 'FAIL', 'nota con otro SHA crudo'),
    ('M2-bfc-result-fail', m2, R7, 'FAIL', 'bfc Result = FAIL'),
    ('M3-bfc-order-false', m3, R7, 'FAIL', 'bfc AllowedWriteScopeElementwiseInOrder = false'),
    ('M4-contract-no-phrase', m4, R7, 'FAIL', 'contrato sin la frase (coherente)'),
    ('M5-note-injected-partial', m5, R7, 'FAIL', 'Injected sin AllowedWriteScope'),
    ('M6-note-missing', m6, R7, 'FAIL', 'nota ausente: resultado FAIL escrito'),
    ('M7-note-malformed', m7, R7, 'FAIL', 'nota = lista JSON: resultado FAIL escrito'),
    ('X1-note-incoherent', x1, R7, 'FAIL', 'nota con RunId/ContractSha256/Fidelity/Decision inventados'),
    ('X2-relay-raw-unbound', x2, R7, 'FAIL', 'relevo y nota con un SHA ajeno; salida cruda = {}'),
    ('X3-negated-phrase', x3, R7, 'FAIL', 'frase negada (coherente)'),
    ('X4-f4b-faithful-nc2', x4, R7, 'FAIL', 'nc2 al estilo F4B-E21 (FAIL esperado por I-4 declarado)'),
    ('N1-raw-int-as-float', n1, R7, 'FAIL?', 'salida cruda con "Attempt": 2.0 (delegacion 2); relevo y nota re-atados'),
    ('N2-raw-bool-for-int', n2, R7, 'FAIL?', 'salida cruda con "AttemptsRemaining": true (delegacion 1)'),
    ('N3-raw-duplicate-key', n3, R7, 'FAIL?', 'salida cruda con "Model" duplicado (primero distinto; JSON se queda con el ultimo)'),
    ('N4-raw-reordered', n4, R7, 'PASS?', 'salida cruda con las claves reordenadas (bytes distintos, mismo contenido)'),
    ('N5-raw-absent-collection', n5, R7, 'FAIL?', 'salida cruda SIN la clave AllowedWriteScope y con Objective = [] en vez de ""'),
    ('N6-runid-all-absent', n6, R7, 'FAIL?', 'RunId ausente en nota, bfc y relevos (None == None == None)'),
    ('N7-runid-deleg-differs', n7, R7, 'FAIL?', 'RunId de la delegacion y la salida cruda distinto del de relevo/bfc/nota'),
    ('N8-fidelity-substring', n8, R7, 'FAIL?', 'Fidelity = "FAIL (no PASS)"'),
    ('N9-evidence-plus-relaxing', n9, R7, 'FAIL?', 'contrato con F4B_EXACT_EVIDENCE y ademas una entrada que releva la exactitud (coherente)'),
    ('N10-evidence-trailing-space', n10, R7, 'FAIL', 'F4B_EXACT_EVIDENCE con un espacio final (coherente)'),
    ('K1-five-args', same, lambda d: [P(d, 'real'), P(d, 'nc2'), BASE, CUR, None], 'FAIL', 'modo F4 sobre los datos F4B'),
    ('K2-note-only', same, lambda d: [P(d, 'real'), P(d, 'nc2'), BASE, CUR, None, P(d, 'planning', 'session-inject-note.json')], 'FAIL', 'solo la nota (6 argumentos)'),
    ('K3-raw-only', same, lambda d: [P(d, 'real'), P(d, 'nc2'), BASE, CUR, None, '', P(d, 'planning', 'output.json')], 'FAIL', 'nota vacia y salida cruda'),
    ('K4-raw-is-delegation', same, lambda d: [P(d, 'real'), P(d, 'nc2'), BASE, CUR, None, P(d, 'planning', 'session-inject-note.json'), P(d, 'real', 'inputs', 'delegation.json')], 'FAIL', 'salida cruda = delegation.json'),
    ('K5-swapped', same, lambda d: [P(d, 'real'), P(d, 'nc2'), BASE, CUR, None, P(d, 'planning', 'output.json'), P(d, 'planning', 'session-inject-note.json')], 'FAIL', 'nota y salida cruda intercambiadas'),
]

os.makedirs(RUNS, exist_ok=True)
summary = []
for name, fn, argf, expect, desc in CASES:
    d = fresh(name)
    fn(d)
    out = os.path.join(RUNS, name + '.json')
    if not os.path.exists(out):
        wb(out, b'{"Stale": "PASS"}\n')  # para comprobar que el script borra un resultado anterior
    args = argf(d)
    args[4] = out
    cp = subprocess.run([sys.executable, '-I', SCRIPT] + args, capture_output=True, text=True, encoding='utf-8')
    with open(os.path.join(RUNS, name + '.txt'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('exit=%d\nargs=%r\n--- stdout\n%s\n--- stderr\n%s\n' % (cp.returncode, args, cp.stdout, cp.stderr))
    res = json.loads(rb(out)) if os.path.exists(out) else None
    props = (res or {}).get('Properties') or {}
    meas = (res or {}).get('Measured') or {}
    summary.append({'Case': name, 'Description': desc, 'Expected': expect, 'Exit': cp.returncode, 'Stderr': cp.stderr.strip()[-200:],
                    'Mode': (res or {}).get('Mode'), 'SubstituteResult': (res or {}).get('SubstituteResult', 'NO OUTPUT'),
                    'FalseProps': sorted(k for k, v in props.items() if not v), 'ExactWhitelistBy': meas.get('ExactWhitelistBy'),
                    'RawJsonEq': meas.get('RawPlusContractCollectionsEqualsDelegationJson'), 'RawBytesEq': meas.get('RawPlusContractCollectionsEqualsDelegationBytes'),
                    'RecordsNote': 'InjectNote' in (res or {}), 'RecordsRaw': 'RawControllerOutput' in (res or {})})
with open(os.path.join(RUNS, 'mutations2-summary.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(summary, ensure_ascii=False, indent=1) + '\n')
for s in summary:
    print('%-30s exp=%-6s got=%-60s exit=%d false=%s by=%s jsonEq=%s bytesEq=%s' % (s['Case'], s['Expected'], s['SubstituteResult'][:60], s['Exit'], s['FalseProps'], s['ExactWhitelistBy'], s['RawJsonEq'], s['RawBytesEq']))
