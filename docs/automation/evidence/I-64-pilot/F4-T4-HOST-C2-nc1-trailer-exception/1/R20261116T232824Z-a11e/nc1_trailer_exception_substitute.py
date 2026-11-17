"""F4C2-NC1-TRAILER-EXCEPTION-01: sustituto determinista de nc1 SOLO para F4-T4-HOST-C2 (intento 1), por la disposicion del Owner/Coordinator
«A — SOLO C2» (F4-DRAFTS/coordinator/owner-f4-c2-nc1-decision.md). No es A-4. No modifica el sustituto general ratificado (F4T4*-NC1-IDENTITY-SIGNAL-01):
es un script propio, local a C2, no global y no reutilizable automaticamente por otras tareas.
La corrida ordinaria de nc1 se conserva tal cual: CONTROL NOT PASSED.
Una sola excepcion: el resultado Trailer del Controller puede ignorarse SOLO si se demuestran las diez condiciones C01..C10 de la disposicion.
Uso (desde la raiz del worktree): python nc1_trailer_exception_substitute.py <dir-verificacion-real> <dir-nc1> <esquema-worker-handoff> <salida.json>
Antes ejecuta su autoprueba sintetica dos veces (determinista). Solo biblioteca estandar; lee JSON y ejecuta git rev-parse y git cat-file -e en local."""
import copy, hashlib, json, os, re, subprocess, sys

ID = 'F4C2-NC1-TRAILER-EXCEPTION-01'
RECORD_OK = 'SATISFIED_UNDER_F4C2_NC1_TRAILER_EXCEPTION'
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing', 'FreeText', 'Denials']
# Los consumidores del CurrentSha que admite el sustituto general ratificado (sin cambios) y la UNICA excepcion de esta disposicion.
CONSUMERS = {'Handoff', 'Identity', 'Remote', 'Ci', 'Tests'}
EXCEPTION = 'Trailer'
HEX40 = re.compile(r'^[0-9a-f]{40}$')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()


def walk(o, path=''):
    if isinstance(o, dict):
        for k, v in o.items():
            yield from walk(v, path + '.' + k)
    elif isinstance(o, list):
        for i, v in enumerate(o):
            yield from walk(v, '%s[%d]' % (path, i))
    else:
        yield path, o


def diffpaths(a, b):
    wa, wb = dict(walk(a)), dict(walk(b))
    return sorted(k for k in set(wa) | set(wb) if wa.get(k, '<absent>') != wb.get(k, '<absent>'))


def names(text, s):
    return bool(s) and (s in (text or '') or s[:8] in (text or ''))


def evaluate(b):
    P, N = {}, {}
    rh, mh = json.loads(b['real_handoff']), json.loads(b['mut_handoff'])
    real_cur, fake, head = rh.get('CurrentSha'), mh.get('CurrentSha'), b['head']
    real_checks, nc, out, rout, ev = b['real_checks'], b['nc_checks'], b['nc_out'], b['real_out'], b['evidence']
    N['RealCurrentSha'], N['MutatedCurrentSha'], N['Head'] = real_cur, fake, head
    # C01: la verificacion real es 14/14.
    N['RealChecks'] = {c: real_checks.get(c) for c in ORDER}
    P['C01_RealVerification14of14'] = (sorted(real_checks) == sorted(ORDER) and all(real_checks[c] == 'pass' for c in ORDER)
                                       and rout.get('Classification') == 'EXECUTION_VERIFIED' and rout.get('VerifiedSha') == head == real_cur)
    # C02: la mutacion cambia solo el CurrentSha falso y los campos derivados mecanicamente.
    hdiff = diffpaths(rh, mh)
    N['HandoffFieldsChanged'] = hdiff
    rr0, rr1 = json.loads(b['real_rrw']), json.loads(b['mut_rrw'])
    changed_inputs = sorted(k for k in set(b['real_files']) | set(b['nc_files']) if b['real_files'].get(k) != b['nc_files'].get(k))
    N['ChangedInputs'] = changed_inputs
    N['RelayRecordWorkFieldsChanged'] = diffpaths(rr0, rr1)
    mut = b['mutation']
    mm = mut.get('Mutation', {})
    P['C02_MutationOnlyFalseCurrentShaPlusDerived'] = (
        hdiff == ['.CurrentSha'] and isinstance(fake, str) and bool(HEX40.match(fake)) and fake != real_cur and b['fake_exists'] is False
        and mm.get('File') == 'worker-handoff.json' and mm.get('Field') == 'CurrentSha' and mm.get('Old') == real_cur and mm.get('New') == fake
        and sorted(b['real_files']) == sorted(b['nc_files']) and changed_inputs == ['relay-record-work.json', 'worker-handoff.json']
        and N['RelayRecordWorkFieldsChanged'] == ['.Outcome.OutputSha256']
        and rr0['Outcome']['OutputSha256'].upper() == sha(b['real_handoff']) and rr1['Outcome']['OutputSha256'].upper() == sha(b['mut_handoff'])
        and [d.get('Field') for d in mut.get('DerivedRecompute', [])] == ['Outcome.OutputSha256']
        and sorted(mut.get('ChangedVsVerifiedInputs', [])) == changed_inputs
        and bool(mut.get('InputSha256')) and all(k in b['nc_files'] and v.upper() == sha(b['nc_files'][k]) for k, v in mut.get('InputSha256', {}).items()))
    # C03: el Controller detecta y rechaza el CurrentSha falso.
    notpass = [c for c in ORDER if nc.get(c) != 'pass']
    fc = out.get('FailureClass')
    N['Controller'] = {'Classification': out.get('Classification'), 'Disposition': out.get('Disposition'), 'FailureClass': fc,
                       'TriggeredStopConditions': out.get('TriggeredStopConditions'), 'VerifiedSha': out.get('VerifiedSha'), 'Results': {c: nc.get(c) for c in ORDER}}
    N['FirstNotPass'] = notpass[0] if notpass else None
    P['C03_ControllerDetectsAndRejectsFalseCurrentSha'] = (
        nc.get('Identity') == 'fail' and names(ev.get('Identity'), fake) and names(ev.get('Identity'), head)
        and bool(notpass) and fc == notpass[0] and fc in ('Handoff', 'Identity') and (fc != 'Handoff' or names(ev.get('Handoff'), fake)))
    # C04: el Trailer de la entrega mutada es equivalente en bytes y en valor al de la entrega real verificada.
    rw, mw = rh.get('Worker') if isinstance(rh.get('Worker'), dict) else {}, mh.get('Worker') if isinstance(mh.get('Worker'), dict) else {}
    rt, mt = rw.get('Trailer'), mw.get('Trailer')
    N['TrailerReal'], N['TrailerMutated'] = rt, mt
    P['C04_TrailerEquivalentToRealDelivery'] = (isinstance(rt, str) and bool(rt.strip()) and rt == mt
                                                and json.dumps(rt, ensure_ascii=False).encode('utf-8') == json.dumps(mt, ensure_ascii=False).encode('utf-8'))
    # C05: Trailer esta bajo Worker, como exige el esquema real de la entrega.
    sc = b['schema']
    sw = (sc.get('properties') or {}).get('Worker') or {}
    P['C05_TrailerUnderWorkerPerSchema'] = (
        'Trailer' not in (sc.get('properties') or {}) and sc.get('additionalProperties') is False and 'Worker' in (sc.get('required') or [])
        and 'Trailer' in (sw.get('properties') or {}) and 'Trailer' in (sw.get('required') or [])
        and 'Trailer' not in rh and 'Trailer' not in mh and 'Trailer' in rw and 'Trailer' in mw)
    # C06: la verificacion real dio Trailer en pass.
    P['C06_RealVerificationTrailerPass'] = real_checks.get('Trailer') == 'pass'
    # C07: la mutacion de nc1 no altera Trailer.
    P['C07_MutationDoesNotAlterTrailer'] = (not any('Trailer' in p for p in hdiff) and 'Trailer' not in (mm.get('Field') or '')
                                            and not any('Trailer' in (d.get('Field') or '') for d in mut.get('DerivedRecompute', [])))
    # C08: ninguna otra comprobacion ajena difiere de forma inesperada.
    differs = [c for c in ORDER if nc.get(c) != real_checks.get(c)]
    N['DiffersFromReal'] = differs
    N['DiffersOutsideRatifiedConsumers'] = [c for c in differs if c not in CONSUMERS]
    P['C08_NoOtherUnrelatedCheckDiffers'] = 'Identity' in differs and set(differs) <= (CONSUMERS | {EXCEPTION})
    # C09: el resultado sigue sin ser VERIFIED: BLOCKED.
    P['C09_OutcomeBlockedNotVerified'] = (out.get('Classification') == 'EXECUTION_BLOCKED' and out.get('Disposition') == 'STOP'
                                          and not out.get('VerifiedSha'))
    # C10: la senal de identidad sigue fallando para el SHA falso.
    P['C10_IdentitySignalFailsForFalseSha'] = (real_checks.get('Identity') == 'pass' and nc.get('Identity') == 'fail'
                                               and head == real_cur and fake != head)
    N['TrailerExceptionUsed'] = EXCEPTION in differs
    return P, N, all(P.values())


SCHEMA = {'type': 'object', 'additionalProperties': False, 'required': ['CurrentSha', 'Worker'],
          'properties': {'CurrentSha': {'type': 'string'}, 'WorkCompleted': {'type': 'string'},
                         'Worker': {'type': 'object', 'additionalProperties': False, 'required': ['Trailer'], 'properties': {'Trailer': {'type': 'string'}}}}}


def synthetic():
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    HEAD = 'a' * 40
    FAKE = 'deadbeef' * 5
    TR = 'Co-Authored-By: Model <noreply@example.invalid>'
    real_h = {'CurrentSha': HEAD, 'WorkCompleted': 'w', 'Worker': {'Trailer': TR}}
    allpass = {c: 'pass' for c in ORDER}

    def bundle(edit=None, fake=FAKE, fake_exists=False, bad_hash=False, extra=False, checks=None, cls='EXECUTION_BLOCKED', disp='STOP',
               fc='Handoff', vsha=None, real_checks=None, schema=None, real_cls='EXECUTION_VERIFIED',
               ev_id='HEAD %s != CurrentSha %s' % (HEAD, FAKE), ev_h='CurrentSha %s does not resolve' % FAKE):
        mh = copy.deepcopy(real_h); mh['CurrentSha'] = fake
        if edit:
            edit(mh)
        rhb, mhb = enc(real_h), enc(mh)
        rr0 = {'Outcome': {'OutputSha256': sha(rhb)}}
        rr1 = copy.deepcopy(rr0); rr1['Outcome']['OutputSha256'] = ('0' * 64) if bad_hash else sha(mhb)
        common = {'gate-contract.json': b'{"c": 1}', 'delegation.json': b'{"d": 1}'}
        rf = dict(common, **{'worker-handoff.json': rhb, 'relay-record-work.json': enc(rr0)})
        nf = dict(common, **{'worker-handoff.json': mhb, 'relay-record-work.json': enc(rr1)})
        if extra:
            nf['delegation.json'] = b'{"d": 2}'
        mutation = {'Mutation': {'File': 'worker-handoff.json', 'Field': 'CurrentSha', 'Old': HEAD, 'New': fake},
                    'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf[k]),
                    'DerivedRecompute': [{'File': 'relay-record-work.json', 'Field': 'Outcome.OutputSha256'}],
                    'InputSha256': {k: sha(v) for k, v in nf.items()}}
        rc = dict(allpass) if real_checks is None else real_checks
        nc = dict(rc)
        nc.update(checks if checks is not None else {'Handoff': 'fail', 'Identity': 'fail', 'Remote': 'fail', 'Ci': 'fail', 'Tests': 'fail', 'Trailer': 'fail'})
        return {'real_handoff': rhb, 'mut_handoff': mhb, 'real_rrw': rf['relay-record-work.json'], 'mut_rrw': nf['relay-record-work.json'],
                'real_files': rf, 'nc_files': nf, 'mutation': mutation, 'head': HEAD, 'fake_exists': fake_exists, 'real_checks': rc,
                'real_out': {'Classification': real_cls, 'VerifiedSha': HEAD},
                'nc_out': {'Classification': cls, 'Disposition': disp, 'FailureClass': fc, 'TriggeredStopConditions': ['S-04'], 'VerifiedSha': vsha},
                'nc_checks': nc, 'evidence': {'Identity': ev_id, 'Handoff': ev_h}, 'schema': schema if schema is not None else SCHEMA}

    def second(mh): mh['WorkCompleted'] = 'otra'

    def trailer(mh): mh['Worker']['Trailer'] = 'Co-Authored-By: Other <x@example.invalid>'
    root_schema = copy.deepcopy(SCHEMA); root_schema['properties']['Trailer'] = {'type': 'string'}
    real13 = dict(allpass); real13['Scope'] = 'fail'
    realtr = dict(allpass); realtr['Trailer'] = 'fail'
    return [
        ('A', 'la corrida real de C2: consumidores del CurrentSha y Trailer en fail', bundle(), True),
        ('A2', 'sin la excepcion: solo los consumidores en fail', bundle(checks={'Handoff': 'fail', 'Identity': 'fail', 'Remote': 'fail', 'Ci': 'fail', 'Tests': 'fail'}), True),
        ('A3', 'el oraculo ordinario: solo Identity en fail', bundle(checks={'Identity': 'fail'}, fc='Identity'), True),
        ('B', 'C01: la verificacion real no es 14/14', bundle(real_checks=real13, checks={'Handoff': 'fail', 'Identity': 'fail', 'Scope': 'fail'}), False),
        ('B2', 'C01: la verificacion real no es VERIFIED', bundle(real_cls='EXECUTION_BLOCKED'), False),
        ('C', 'C02: la mutacion cambia un segundo campo', bundle(edit=second), False),
        ('C2', 'C02: el SHA sustituido existe en el repositorio', bundle(fake_exists=True), False),
        ('C3', 'C02: hash derivado incoherente', bundle(bad_hash=True), False),
        ('C4', 'C02: cambia otra entrada', bundle(extra=True), False),
        ('D', 'C03: la Evidence de Identity no nombra el SHA falso', bundle(ev_id='HEAD %s differs' % HEAD), False),
        ('D2', 'C03: FailureClass que no es la primera en fail', bundle(fc='Identity'), False),
        ('E', 'C04/C07: la mutacion altera Trailer', bundle(edit=trailer), False),
        ('F', 'C05: el esquema admite Trailer en la raiz', bundle(schema=root_schema), False),
        ('G', 'C06: la verificacion real dio Trailer en fail', bundle(real_checks=realtr, checks={'Handoff': 'fail', 'Identity': 'fail', 'Trailer': 'fail'}), False),
        ('H', 'C08: difiere una comprobacion ajena (Scope)', bundle(checks={'Handoff': 'fail', 'Identity': 'fail', 'Trailer': 'fail', 'Scope': 'fail'}), False),
        ('H2', 'C08: difiere otra comprobacion ajena (Routing) ademas de Trailer', bundle(checks={'Handoff': 'fail', 'Identity': 'fail', 'Trailer': 'fail', 'Routing': 'fail'}), False),
        ('I', 'C09: resultado VERIFIED', bundle(cls='EXECUTION_VERIFIED', disp='NONE', vsha=FAKE), False),
        ('J', 'C10: el Controller dio Identity en pass', bundle(checks={'Handoff': 'fail', 'Trailer': 'fail'}), False),
    ]


def selftest():
    out = []
    for c, d, bd, exp in synthetic():
        _, _, ok = evaluate(bd)
        out.append({'Case': c, 'Description': d, 'Result': 'PASS' if ok else 'FAIL', 'Expected': 'PASS' if exp else 'FAIL', 'Ok': ok == exp})
    return out


def load_inputs(d):
    files = {}
    base = os.path.join(d, 'inputs')
    for r, _, fs in os.walk(base):
        for f in fs:
            p = os.path.join(r, f)
            files[os.path.relpath(p, base).replace(os.sep, '/')] = open(p, 'rb').read()
    return files


def checks_of(out):
    ch = out['Checks']
    if isinstance(ch, dict):
        return {k: v['Result'] for k, v in ch.items()}, {k: v.get('Evidence', '') for k, v in ch.items()}
    return {c['Id']: c['Result'] for c in ch}, {c['Id']: c.get('Evidence', '') for c in ch}


def main(real_dir, nc_dir, schema_path, out_path):
    st, st2 = selftest(), selftest()
    res = {'Id': ID, 'NotA4': True,
           'Scope': 'solo F4-T4-HOST-C2, intento 1; local, no global y no reutilizable automaticamente; no modifica el sustituto general ratificado',
           'Authority': 'F4-DRAFTS/coordinator/owner-f4-c2-nc1-decision.md (A - SOLO C2)',
           'SelfTest': st, 'SelfTestPass': all(x['Ok'] for x in st), 'SelfTestDeterministic': st == st2}
    if not (res['SelfTestPass'] and res['SelfTestDeterministic']):
        res['SubstituteResult'] = 'FAIL (autoprueba)'
    else:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        rf, nf = load_inputs(real_dir), load_inputs(nc_dir)
        head = subprocess.run(['git', 'rev-parse', 'HEAD'], capture_output=True, text=True, check=True).stdout.strip()
        fake = json.loads(nf['worker-handoff.json']).get('CurrentSha', '')
        fake_exists = subprocess.run(['git', 'cat-file', '-e', fake], capture_output=True).returncode == 0
        real_out, nc_out = ld(os.path.join(real_dir, 'output.json')), ld(os.path.join(nc_dir, 'output.json'))
        real_checks, _ = checks_of(real_out)
        nc_checks, ev = checks_of(nc_out)
        bd = {'real_handoff': rf['worker-handoff.json'], 'mut_handoff': nf['worker-handoff.json'],
              'real_rrw': rf['relay-record-work.json'], 'mut_rrw': nf['relay-record-work.json'],
              'real_files': rf, 'nc_files': nf, 'mutation': ld(os.path.join(nc_dir, 'mutation.json')), 'head': head, 'fake_exists': fake_exists,
              'real_checks': real_checks, 'real_out': real_out, 'nc_out': nc_out, 'nc_checks': nc_checks, 'evidence': ev, 'schema': ld(schema_path)}
        P, N, ok = evaluate(bd)
        N['FakeShaExistsInRepo'] = fake_exists
        N['SchemaSha256'] = sha(open(schema_path, 'rb').read())
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc1Run': nc_dir.replace(os.sep, '/'), 'Head': head,
                    'Nc1ControllerEvidence': {'Handoff': ev.get('Handoff'), 'Identity': ev.get('Identity'), 'Trailer': ev.get('Trailer')},
                    'Measured': N, 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Record': {'Nc1OrdinaryRun': 'CONTROL NOT PASSED (se conserva tal cual)',
                               'nc1': RECORD_OK if ok else 'NOT SATISFIED',
                               'ProtocolDebt': 'PD-F4-08: el Controller puede buscar Trailer en la raiz de la entrega en lugar de bajo Worker durante una corrida de control negativo'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'], 'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st],
                      'SubstituteResult': res['SubstituteResult'], 'Properties': res.get('Properties')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:5]))
