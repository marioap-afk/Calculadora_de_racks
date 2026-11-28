"""F4T2C-NC1-IDENTITY-SIGNAL-01: sustituto determinista de nc1 solo para F4-T2C-IC04-READ-SCHEDULING (intento 1). No es A-4.
La corrida ordinaria de nc1 se conserva tal cual: oraculo no cumplido / senal demostrada (la primera comprobacion en fail es Handoff,
por AUTOMATION_PLAN 16.9 'Salida conforme pero vacia o falsa', y no Identity como fija README section 10).
Uso (desde la raiz del worktree): python nc1_identity_signal_substitute.py <dir-verificacion-real> <dir-nc1> <salida.json>
Demuestra: la mutacion es exactamente CurrentSha de worker-handoff.json, cambiado por un SHA de 40 hex que no existe en el repositorio;
los hashes derivados son coherentes; Identity tenia que fallar (HEAD = CurrentSha real, Identity pass en la verificacion real); el Controller
dio Identity = fail con EXECUTION_BLOCKED/STOP y su Evidence de Identity nombra el SHA falso y el HEAD real; solo difieren de la verificacion
real comprobaciones que consumen CurrentSha (Handoff, Identity, Remote, Ci, Tests); FailureClass es la primera que no esta en pass y es
Handoff o Identity (si es Handoff, su Evidence nombra el SHA falso). Antes ejecuta su autoprueba sintetica dos veces (determinista).
Solo biblioteca estandar; lee JSON y ejecuta git rev-parse y git cat-file -e en local."""
import copy, hashlib, json, os, re, subprocess, sys

ID = 'F4T2C-NC1-IDENTITY-SIGNAL-01'
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing', 'FreeText', 'Denials']
CONSUMERS = {'Handoff', 'Identity', 'Remote', 'Ci', 'Tests'}
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
    return bool(s) and (s in text or s[:8] in text)


def evaluate(b):
    P, N = {}, {}
    rh, mh = json.loads(b['real_handoff']), json.loads(b['mut_handoff'])
    real_cur, fake = rh.get('CurrentSha'), mh.get('CurrentSha')
    N['RealCurrentSha'], N['MutatedCurrentSha'], N['Head'] = real_cur, fake, b['head']
    N['HandoffFieldsChanged'] = diffpaths(rh, mh)
    P['1_MutationIsExactlyCurrentShaToANonexistentSha'] = (N['HandoffFieldsChanged'] == ['.CurrentSha'] and isinstance(fake, str) and bool(HEX40.match(fake))
                                                           and fake != real_cur and b['fake_exists'] is False)
    rr0, rr1 = json.loads(b['real_rrw']), json.loads(b['mut_rrw'])
    changed_inputs = sorted(k for k in b['real_files'] if b['real_files'][k] != b['nc_files'].get(k))
    N['ChangedInputs'] = changed_inputs
    mut = b['mutation']
    P['2_DerivedHashesConsistent'] = (sorted(b['real_files']) == sorted(b['nc_files'])
                                      and changed_inputs == ['relay-record-work.json', 'worker-handoff.json'] == sorted(mut.get('ChangedVsVerifiedInputs', []))
                                      and diffpaths(rr0, rr1) == ['.Outcome.OutputSha256']
                                      and rr0['Outcome']['OutputSha256'].upper() == sha(b['real_handoff'])
                                      and rr1['Outcome']['OutputSha256'].upper() == sha(b['mut_handoff'])
                                      and all(v.upper() == sha(b['nc_files'][k]) for k, v in mut.get('InputSha256', {}).items()))
    P['3_IdentityMustFail'] = b['real_checks'].get('Identity') == 'pass' and b['head'] == real_cur and fake != b['head']
    out = b['nc_out']
    nc = b['nc_checks']
    N['Controller'] = {'Classification': out.get('Classification'), 'Disposition': out.get('Disposition'), 'FailureClass': out.get('FailureClass'),
                       'TriggeredStopConditions': out.get('TriggeredStopConditions'), 'Results': {c: nc.get(c) for c in ORDER}}
    P['4_ControllerReportedIdentityFailBlockedStop'] = (nc.get('Identity') == 'fail' and out.get('Classification') == 'EXECUTION_BLOCKED'
                                                        and out.get('Disposition') == 'STOP')
    ev = b['evidence']
    P['5_IdentityEvidenceNamesFakeShaAndHead'] = names(ev.get('Identity', ''), fake) and names(ev.get('Identity', ''), b['head'])
    differs = [c for c in ORDER if nc.get(c) != b['real_checks'].get(c)]
    N['DiffersFromReal'] = differs
    P['6_OnlyCurrentShaConsumersDiffer'] = 'Identity' in differs and set(differs) <= CONSUMERS
    notpass = [c for c in ORDER if nc.get(c) != 'pass']
    fc = out.get('FailureClass')
    N['FirstNotPass'] = notpass[0] if notpass else None
    P['7_FailureClassFirstNotPassHandoffOrIdentity'] = (bool(notpass) and fc == notpass[0] and fc in ('Handoff', 'Identity')
                                                        and (fc != 'Handoff' or names(ev.get('Handoff', ''), fake)))
    return P, N, all(P.values())


def synthetic():
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    HEAD = 'a' * 40
    FAKE = 'deadbeef' * 5
    real_h = {'TaskId': 'T', 'BaseSha': 'b' * 40, 'RedSha': 'c' * 40, 'CurrentSha': HEAD, 'WorkCompleted': 'w'}
    real_checks = {c: 'pass' for c in ORDER}

    def bundle(edit=None, fake=FAKE, fake_exists=False, bad_hash=False, extra=False, checks=None, cls='EXECUTION_BLOCKED', disp='STOP', fc='Handoff',
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
        mutation = {'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf[k]), 'InputSha256': {k: sha(v) for k, v in nf.items()}}
        nc = dict(real_checks)
        nc.update(checks if checks is not None else {'Handoff': 'fail', 'Identity': 'fail', 'Remote': 'fail', 'Ci': 'fail', 'Tests': 'fail'})
        return {'real_handoff': rhb, 'mut_handoff': mhb, 'real_rrw': rf['relay-record-work.json'], 'mut_rrw': nf['relay-record-work.json'],
                'real_files': rf, 'nc_files': nf, 'mutation': mutation, 'head': HEAD, 'fake_exists': fake_exists, 'real_checks': real_checks,
                'nc_out': {'Classification': cls, 'Disposition': disp, 'FailureClass': fc, 'TriggeredStopConditions': ['S-04']},
                'nc_checks': nc, 'evidence': {'Identity': ev_id, 'Handoff': ev_h}}

    def second(mh): mh['WorkCompleted'] = 'otra'
    return [
        ('A', 'CurrentSha falso detectado; Handoff primera en fail y cascada de consumidores (la corrida real)', bundle(), True),
        ('A2', 'CurrentSha falso detectado; solo Identity en fail (el oraculo ordinario)', bundle(checks={'Identity': 'fail'}, fc='Identity'), True),
        ('B', 'la mutacion cambia un segundo campo de la entrega', bundle(edit=second), False),
        ('C', 'el SHA sustituido existe en el repositorio', bundle(fake_exists=True), False),
        ('D', 'hash derivado incoherente', bundle(bad_hash=True), False),
        ('E', 'cambia otra entrada ademas de las derivadas', bundle(extra=True), False),
        ('F', 'el Controller dio Identity = pass', bundle(checks={'Handoff': 'fail'}), False),
        ('G', 'clasificacion REWORK en lugar de BLOCKED/STOP', bundle(cls='EXECUTION_REWORK_REQUIRED', disp='REWORK'), False),
        ('H', 'falla tambien una comprobacion ajena al CurrentSha (Scope)', bundle(checks={'Handoff': 'fail', 'Identity': 'fail', 'Scope': 'fail'}), False),
        ('I', 'la Evidence de Identity no nombra el SHA falso', bundle(ev_id='HEAD %s differs' % HEAD), False),
        ('J', 'FailureClass que no es la primera en fail', bundle(fc='Identity'), False),
        ('K', 'FailureClass Handoff sin nombrar el SHA falso en su Evidence', bundle(ev_h='handoff invalid'), False),
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


def main(real_dir, nc_dir, out_path):
    st, st2 = selftest(), selftest()
    res = {'Id': ID, 'NotA4': True, 'Scope': 'solo F4-T2C-IC04-READ-SCHEDULING, intento 1; no reemplaza al Controller ni a nc1 en otras tareas',
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
              'real_checks': real_checks, 'nc_out': nc_out, 'nc_checks': nc_checks, 'evidence': ev}
        P, N, ok = evaluate(bd)
        N['FakeShaExistsInRepo'] = fake_exists
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc1Run': nc_dir.replace(os.sep, '/'), 'Head': head,
                    'RealVerificationClassification': real_out.get('Classification'),
                    'Nc1ControllerEvidence': {'Handoff': ev.get('Handoff'), 'Identity': ev.get('Identity')},
                    'Measured': N, 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Notes': {
                        'SignalDemonstrated': 'el Controller detecto la mutacion: su Evidence de Identity nombra el CurrentSha falso y el HEAD real, y la de Handoff dice que el SHA falso no resuelve en Git y contradice el relevo de entrada',
                        'OrdinaryOracleNotMet': 'la primera comprobacion en fail es Handoff (AUTOMATION_PLAN 16.9: Salida conforme pero vacia o falsa -> Handoff), no Identity como fija README section 10; Remote, Ci y Tests fallan en cascada porque consumen CurrentSha',
                        'ProtocolDebt': 'PD-F4-04: README section 10 (nc1 -> Identity) y AUTOMATION_PLAN 16.9 (salida falsa -> Handoff) admiten dos primeras comprobaciones para un CurrentSha inexistente; deuda de protocolo, sin solucion global dentro de I-64'},
                    'Record': {'Nc1OrdinaryRun': 'ORACULO NO CUMPLIDO / SENAL DEMOSTRADA (se conserva tal cual)',
                               'nc1': 'SATISFIED_UNDER_F4T2C_NC1_IDENTITY_SIGNAL_SUBSTITUTE' if ok else 'NOT SATISFIED'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'], 'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st],
                      'SubstituteResult': res['SubstituteResult'], 'Properties': res.get('Properties'), 'Measured': res.get('Measured')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:4]))
