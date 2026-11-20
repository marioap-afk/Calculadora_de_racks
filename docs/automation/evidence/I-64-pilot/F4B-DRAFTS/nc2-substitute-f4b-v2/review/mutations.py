"""Revision independiente: mutaciones propias de los datos (copias en review/mut/) y ejecucion del sustituto adaptado sobre cada una.
Uso (cwd = checkout del repo): python -I mutations.py <dir-nc2-substitute-f4b> <BaseSha> <CurrentSha>
Solo escribe dentro de <dir>/review/."""
import hashlib, json, os, shutil, subprocess, sys

ROOT, BASE, CUR = sys.argv[1], sys.argv[2], sys.argv[3]
REVIEW = os.path.join(ROOT, 'review')
MUT = os.path.join(REVIEW, 'mut')
RUNS = os.path.join(REVIEW, 'runs')
SCRIPT = os.path.join(ROOT, 'nc2_exact_whitelist_substitute_f4b.py')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
OLD_EV0 = 'Antes del Worker: el contrato, la salida del Controller (output.json, con las colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json), la comparacion byte a byte, A1-A8 y nc4.'


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


def fresh(name):
    d = os.path.join(MUT, name)
    if os.path.exists(d):
        shutil.rmtree(d)
    for sub in ('real', 'nc2', 'planning'):
        shutil.copytree(os.path.join(ROOT, 'data', sub), os.path.join(d, sub))
    return d


def P(d, *parts):
    return os.path.join(d, *parts)


def set_note(d, **kv):
    p = P(d, 'planning', 'session-inject-note.json')
    n = json.loads(rb(p))
    n.update(kv)
    wb(p, (json.dumps(n, ensure_ascii=False, indent=2)).encode('utf-8'))


def ev_field_hash(ee):
    """Serializacion que reproduce el SourceSha256 de ExpectedEvidence del bfc."""
    bfc = json.loads(rb(os.path.join(ROOT, 'data', 'real', 'inputs', 'byte-fidelity-check.json')))
    target = {f['Field']: f for f in bfc['Fields']}['ExpectedEvidence']['SourceSha256'].upper()
    ct = json.loads(rb(os.path.join(ROOT, 'data', 'real', 'inputs', 'gate-contract.json')))
    for ea in (False, True):
        if sha(json.dumps(ct['ExpectedEvidence'], ensure_ascii=ea, separators=(',', ':')).encode('utf-8')) == target:
            return target, sha(json.dumps(ee, ensure_ascii=ea, separators=(',', ':')).encode('utf-8'))
    raise SystemExit('no se reproduce el hash de campo de ExpectedEvidence')


def coherent_ev0(d, new_ev0):
    """Reescribe ExpectedEvidence[0] en contrato y delegaciones (real y nc2) y recalcula TODO lo derivado: bfc, nota, relevo de nc2, mutation.json."""
    old_new = (json.dumps(OLD_EV0, ensure_ascii=False).encode('utf-8'), json.dumps(new_ev0, ensure_ascii=False).encode('utf-8'))
    ct_old = rb(P(d, 'real', 'inputs', 'gate-contract.json'))
    rd_old = rb(P(d, 'real', 'inputs', 'delegation.json'))
    md_old = rb(P(d, 'nc2', 'inputs', 'delegation.json'))
    rr1_old = rb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'))
    bfc_old = rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    ct_new, rd_new, md_new = rep(ct_old, *old_new, 1), rep(rd_old, *old_new, 1), rep(md_old, *old_new, 1)
    ee_new = json.loads(ct_new)['ExpectedEvidence']
    fh_old, fh_new = ev_field_hash(ee_new)
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
    m = rb(P(d, 'nc2', 'mutation.json'))
    for o, n in ((sha(md_old), sha(md_new)), (sha(ct_old), sha(ct_new)), (sha(rr1_old), sha(rr1_new)), (sha(bfc_old), sha(bfc_new))):
        m = rep(m, o.encode(), n.encode())
    wb(P(d, 'nc2', 'mutation.json'), m)
    set_note(d, DelegationSha256=sha(rd_new), ContractSha256=sha(ct_new))


def m_note_other_raw(d):
    set_note(d, OutputSha256=sha(b'otra salida cruda del Controller'))


def m_bfc_result_fail(d):
    old = rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    new = rep(old, b'"Result": "PASS"', b'"Result": "FAIL"', 1)
    for side in ('real', 'nc2'):
        wb(P(d, side, 'inputs', 'byte-fidelity-check.json'), new)
    wb(P(d, 'nc2', 'mutation.json'), rep(rb(P(d, 'nc2', 'mutation.json')), sha(old).encode(), sha(new).encode(), 1))


def m_bfc_order_false(d):
    old = rb(P(d, 'real', 'inputs', 'byte-fidelity-check.json'))
    new = rep(old, b'"AllowedWriteScopeElementwiseInOrder": true', b'"AllowedWriteScopeElementwiseInOrder": false', 1)
    for side in ('real', 'nc2'):
        wb(P(d, side, 'inputs', 'byte-fidelity-check.json'), new)
    wb(P(d, 'nc2', 'mutation.json'), rep(rb(P(d, 'nc2', 'mutation.json')), sha(old).encode(), sha(new).encode(), 1))


def m_contract_no_phrase(d):
    coherent_ev0(d, OLD_EV0.replace('la comparacion byte a byte, ', ''))


def m_note_injected_partial(d):
    set_note(d, Injected=['Authorities', 'Objective', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions'])


def m_note_missing(d):
    os.remove(P(d, 'planning', 'session-inject-note.json'))


def m_note_malformed(d):
    wb(P(d, 'planning', 'session-inject-note.json'), b'["no", "es", "un", "objeto"]\n')


def p_note_incoherent(d):
    set_note(d, RunId='R-INVENTADA', ContractSha256='0' * 64, Fidelity='FAIL (inventada)', Decision='ninguna')


def p_relay_raw_unbound(d):
    x = sha(b'esto no es la salida cruda del Controller')
    p = P(d, 'real', 'inputs', 'relay-record-planning.json')
    wb(p, rep(rb(p), b'100A7695160F5A5FF622470128957399BB2172FF90D0F66EDC414C5E811472A8', x.encode(), 1))
    set_note(d, OutputSha256=x)
    wb(P(d, 'planning', 'output.json'), b'{}\n')


def p_negated_phrase(d):
    coherent_ev0(d, 'Antes del Worker: no se exige la comparacion byte a byte de AllowedWriteScope; basta el subconjunto A3, A1-A8 y nc4.')


def p_f4b_faithful_nc2(d):
    """nc2 al estilo F4B-E21: el relevo (SHA de la salida cruda) no cambia; solo cambia delegation.json."""
    rr0 = rb(P(d, 'real', 'inputs', 'relay-record-planning.json'))
    rr1 = rb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'))
    wb(P(d, 'nc2', 'inputs', 'relay-record-planning.json'), rr0)
    m = json.loads(rb(P(d, 'nc2', 'mutation.json')))
    m['ChangedVsVerifiedInputs'] = ['delegation.json']
    m['InputSha256']['relay-record-planning.json'] = sha(rr0)
    assert sha(rr1) != sha(rr0)
    wb(P(d, 'nc2', 'mutation.json'), (json.dumps(m, ensure_ascii=False, indent=2) + '\n').encode('utf-8'))


CASES = [
    ('M1-note-other-raw-sha', m_note_other_raw, 'FAIL', 'la nota con otro SHA de salida cruda'),
    ('M2-bfc-result-fail', m_bfc_result_fail, 'FAIL', 'bfc (real y nc2, mismo byte) con Result = FAIL; mutation.json recalculado'),
    ('M3-bfc-order-false', m_bfc_order_false, 'FAIL', 'bfc con AllowedWriteScopeElementwiseInOrder = false; mutation.json recalculado'),
    ('M4-contract-no-phrase', m_contract_no_phrase, 'FAIL', 'contrato sin la frase (todo lo derivado recalculado de forma coherente)'),
    ('M5-note-injected-partial', m_note_injected_partial, 'FAIL', 'la nota sin AllowedWriteScope en Injected'),
    ('M6-note-missing', m_note_missing, 'FAIL', 'la nota no existe (ruta pasada)'),
    ('M7-note-malformed', m_note_malformed, 'FAIL', 'la nota es una lista JSON'),
    ('X1-note-incoherent', p_note_incoherent, 'FAIL', 'SONDA: nota con RunId, ContractSha256, Fidelity y Decision inventados'),
    ('X2-relay-raw-unbound', p_relay_raw_unbound, 'FAIL', 'SONDA: relevo real y nota con un SHA que no es el de ninguna salida cruda; output.json crudo = {}'),
    ('X3-negated-phrase', p_negated_phrase, 'FAIL', 'SONDA: el contrato dice que NO se exige la comparacion byte a byte (coherente)'),
    ('X4-f4b-faithful-nc2', p_f4b_faithful_nc2, 'PASS?', 'SONDA: nc2 al estilo F4B-E21 (relevo sin cambiar; solo delegation.json)'),
]

os.makedirs(RUNS, exist_ok=True)
summary = []
for name, fn, expect, desc in CASES:
    d = fresh(name)
    fn(d)
    out = os.path.join(RUNS, name + '.json')
    if os.path.exists(out):
        os.remove(out)
    cp = subprocess.run([sys.executable, '-I', SCRIPT, P(d, 'real'), P(d, 'nc2'), BASE, CUR, out, P(d, 'planning', 'session-inject-note.json')],
                        capture_output=True, text=True, encoding='utf-8')
    with open(os.path.join(RUNS, name + '.txt'), 'w', encoding='utf-8', newline='\n') as f:
        f.write('exit=%d\n--- stdout\n%s\n--- stderr\n%s\n' % (cp.returncode, cp.stdout, cp.stderr))
    res = json.loads(rb(out)) if os.path.exists(out) else None
    props = (res or {}).get('Properties') or {}
    summary.append({'Case': name, 'Description': desc, 'Expected': expect, 'Exit': cp.returncode,
                    'SubstituteResult': (res or {}).get('SubstituteResult', 'NO OUTPUT (excepcion: %s)' % (cp.stderr.strip().splitlines() or ['?'])[-1]),
                    'FalseProps': sorted(k for k, v in props.items() if not v),
                    'ExactWhitelistBy': ((res or {}).get('Measured') or {}).get('ExactWhitelistBy')})
with open(os.path.join(RUNS, 'mutations-summary.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(summary, ensure_ascii=False, indent=1) + '\n')
print(json.dumps(summary, ensure_ascii=False, indent=1))
