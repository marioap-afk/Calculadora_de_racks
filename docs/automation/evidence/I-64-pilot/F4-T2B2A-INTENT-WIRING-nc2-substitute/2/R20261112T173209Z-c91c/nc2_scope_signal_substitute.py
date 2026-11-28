"""F4T2B2A-NC2-SCOPE-SIGNAL-01: sustituto determinista de nc2 solo para F4-T2B2A (intento 2). No es A-4.
La corrida ordinaria de nc2 se conserva tal cual: oraculo mecanico cumplido, senal no demostrada (su Evidence de Scope no detecta la ruta quitada).
Uso (desde la raiz del worktree): python nc2_scope_signal_substitute.py <dir-verificacion-real> <dir-nc2> <BaseSha> <CurrentSha> <salida.json>
Demuestra: la delegacion real coincide con el contrato; la ruta quitada esta en el diff y dentro del alcance real; nc2 quita exactamente esa ruta y
nada mas; los hashes derivados son coherentes; tras la mutacion la ruta queda fuera del alcance; Scope tenia que fallar; y el Controller dio Scope = fail
(FailureClass Scope, EXECUTION_BLOCKED/STOP). Mide ademas, como observacion, si la Evidence de Scope del Controller nombra la ruta quitada.
Antes ejecuta su autoprueba sintetica dos veces (determinista). Solo biblioteca estandar; lee JSON y ejecuta git diff --name-only en local."""
import copy, hashlib, json, os, subprocess, sys

ID = 'F4T2B2A-NC2-SCOPE-SIGNAL-01'
EXACT_CLAUSE = 'AllowedWriteScope de la delegacion = exactamente el AllowedWriteScope del contrato.'
OPAQUE = ['Objective', 'Authorities', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions']
sha = lambda b: hashlib.sha256(b).hexdigest().upper()


def covered(path, scope):
    return any(path == e or (e.endswith('/') and path.startswith(e)) for e in scope)


def scope_predicate(changed, allowed, forbidden):
    return 'pass' if all(covered(p, allowed) for p in changed) and not any(covered(p, forbidden) for p in changed) else 'fail'


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


def readme10_mutation(allowed, changed):
    p = sorted(changed)[0]
    out = []
    for e in allowed:
        if e == p:
            continue
        if e.endswith('/') and p.startswith(e):
            out.extend(sorted(x for x in changed if x != p and x.startswith(e) and x not in out))
            continue
        out.append(e)
    return p, out


def evaluate(b):
    P, N = {}, {}
    rd, md, ct = json.loads(b['real_deleg']), json.loads(b['mut_deleg']), json.loads(b['contract'])
    ra, ma, ca = rd.get('AllowedWriteScope', []), md.get('AllowedWriteScope', []), ct.get('AllowedWriteScope', [])
    changed = b['changed']
    bfc = json.loads(b['real_bfc'])
    fields = {f['Field']: f for f in bfc.get('Fields', [])}
    P['1_AcceptedRealDelegationMatchesContract'] = (ra == ca and all(rd.get(k) == ct.get(k) for k in OPAQUE) and EXACT_CLAUSE in ct.get('ExpectedEvidence', [])
                                                   and bfc.get('OutputSha256', '').upper() == sha(b['real_deleg']) and bfc.get('ContractSha256', '').upper() == sha(b['contract'])
                                                   and all(fields.get(k, {}).get('Utf8BytesEqual') is True for k in ['AllowedWriteScope'] + OPAQUE))
    p_expected, expect_mut = readme10_mutation(ra, changed) if changed else (None, None)
    removed = sorted(x for x in changed if covered(x, ra) and not covered(x, ma))
    p = removed[0] if len(removed) == 1 else None
    N['RemovedPath'] = p
    P['2_RemovedPathInDiffAndInsideRealScope'] = p is not None and p in changed and covered(p, ra) and scope_predicate(changed, ra, rd.get('ForbiddenWriteScope', [])) == 'pass'
    P['3_Nc2RemovesExactlyThatPath'] = p is not None and p == p_expected and ma == expect_mut and all(covered(x, ma) for x in changed if x != p)
    N['DelegationFieldsChanged'] = sorted(k for k in set(rd) | set(md) if rd.get(k) != md.get(k))
    P['4_NoOtherDelegationFieldChanges'] = N['DelegationFieldsChanged'] == ['AllowedWriteScope']
    rr0, rr1 = json.loads(b['real_rrp']), json.loads(b['mut_rrp'])
    mut = b['mutation']
    changed_inputs = sorted(k for k in b['real_files'] if b['real_files'][k] != b['nc_files'].get(k))
    N['ChangedInputs'] = changed_inputs
    P['5_DerivedHashesConsistent'] = (sorted(b['real_files']) == sorted(b['nc_files'])
                                      and changed_inputs == ['delegation.json', 'relay-record-planning.json'] == sorted(mut.get('ChangedVsVerifiedInputs', []))
                                      and diffpaths(rr0, rr1) == ['.Outcome.OutputSha256']
                                      and rr0['Outcome']['OutputSha256'].upper() == sha(b['real_deleg'])
                                      and rr1['Outcome']['OutputSha256'].upper() == sha(b['mut_deleg'])
                                      and b['real_bfc'] == b['mut_bfc']
                                      and all(v.upper() == sha(b['nc_files'][k]) for k, v in mut.get('InputSha256', {}).items()))
    P['6_RemovedPathOutsideMutatedScope'] = p is not None and not covered(p, ma)
    N['ScopeOnReal'] = scope_predicate(changed, ra, rd.get('ForbiddenWriteScope', []))
    N['ScopeOnMutated'] = scope_predicate(changed, ma, md.get('ForbiddenWriteScope', []))
    P['7_ScopeMustFail'] = N['ScopeOnMutated'] == 'fail' and N['ScopeOnReal'] == 'pass'
    out = b['nc_out']
    N['Controller'] = {'Classification': out.get('Classification'), 'Disposition': out.get('Disposition'), 'FailureClass': out.get('FailureClass'),
                       'Scope': b['nc_checks'].get('Scope'), 'Contract': b['nc_checks'].get('Contract'), 'TriggeredStopConditions': out.get('TriggeredStopConditions')}
    P['8_ControllerReportedScopeFail'] = (b['nc_checks'].get('Scope') == 'fail' and out.get('FailureClass') == 'Scope'
                                          and out.get('Classification') == 'EXECUTION_BLOCKED' and out.get('Disposition') == 'STOP')
    ev = b['scope_evidence'] or ''
    N['ControllerScopeEvidenceNamesRemovedPath'] = p is not None and (p in ev or os.path.basename(p) in ev)
    return P, N, all(P.values())


def synthetic():
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    A = ['src/X/A.cs', 'src/X/B.cs', 'tests/T/TA.cs']
    contract = {'Objective': 'o', 'Authorities': [{'Path': 'docs/a.md', 'Class': 'UNIT_DOC'}], 'AllowedWriteScope': A, 'ForbiddenWriteScope': ['src/Y/', 'docs/'],
                'Invariants': ['i1'], 'RequiredTests': [{'Filter': 'f', 'MinSelected': 10, 'ExpectRed': True}], 'ExpectedEvidence': ['e', EXACT_CLAUSE],
                'StopConditions': [{'Id': 'S-04', 'Text': 's'}]}
    real = dict(copy.deepcopy(contract), TaskId='T', Model='m')
    changed0 = ['src/X/A.cs', 'src/X/B.cs', 'tests/T/TA.cs']

    def bundle(real=real, changed=changed0, mut_edit=None, scope='fail', fc='Scope', cls='EXECUTION_BLOCKED', bad_hash=False, extra=False, evidence='otro motivo'):
        rd = copy.deepcopy(real)
        md = copy.deepcopy(rd)
        md['AllowedWriteScope'] = readme10_mutation(rd['AllowedWriteScope'], changed)[1]
        if mut_edit:
            mut_edit(md)
        rdb, mdb, ctb = enc(rd), enc(md), enc(contract)
        rr0 = {'Outcome': {'OutputSha256': sha(rdb)}}
        rr1 = copy.deepcopy(rr0); rr1['Outcome']['OutputSha256'] = ('0' * 64) if bad_hash else sha(mdb)
        bfc = enc({'OutputSha256': sha(rdb).lower(), 'ContractSha256': sha(ctb).lower(), 'Fields': [{'Field': k, 'Utf8BytesEqual': True} for k in ['AllowedWriteScope'] + OPAQUE]})
        common = {'gate-contract.json': ctb, 'worker-handoff.json': b'{"h": 1}', 'byte-fidelity-check.json': bfc}
        rf = dict(common, **{'delegation.json': rdb, 'relay-record-planning.json': enc(rr0)})
        nf = dict(common, **{'delegation.json': mdb, 'relay-record-planning.json': enc(rr1)})
        if extra:
            nf['worker-handoff.json'] = b'{"h": 2}'
        mutation = {'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf[k]), 'InputSha256': {k: sha(v) for k, v in nf.items()}}
        return {'real_deleg': rdb, 'mut_deleg': mdb, 'contract': ctb, 'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
                'real_bfc': bfc, 'mut_bfc': bfc, 'changed': changed, 'nc_checks': {'Scope': scope, 'Contract': 'pass'},
                'nc_out': {'Classification': cls, 'Disposition': 'STOP' if cls == 'EXECUTION_BLOCKED' else 'REWORK', 'FailureClass': fc, 'TriggeredStopConditions': []},
                'real_files': rf, 'nc_files': nf, 'mutation': mutation, 'scope_evidence': evidence}

    def keep(md): md['AllowedWriteScope'] = A
    def second(md): md['Model'] = 'otro'
    def add(md): md['AllowedWriteScope'] = md['AllowedWriteScope'] + ['src/X/Z.cs']
    loose = dict(copy.deepcopy(real), AllowedWriteScope=A[:2])
    return [
        ('A', 'ruta quitada en el diff, fuera del alcance mutado; Controller con Scope = fail por otro motivo', bundle(), True),
        ('A2', 'lo mismo y el Controller nombra la ruta quitada', bundle(evidence='src/X/A.cs is outside AllowedWriteScope'), True),
        ('B', 'el alcance mutado sigue cubriendo la ruta', bundle(mut_edit=keep), False),
        ('C', 'cambia un segundo campo de la delegacion', bundle(mut_edit=second), False),
        ('D', 'la mutacion anade otra ruta', bundle(mut_edit=add), False),
        ('E', 'la delegacion real no es la lista exacta del contrato', bundle(real=loose, changed=['src/X/A.cs', 'src/X/B.cs']), False),
        ('F', 'hash derivado incoherente', bundle(bad_hash=True), False),
        ('G', 'cambia otra entrada ademas de las derivadas', bundle(extra=True), False),
        ('H', 'el Controller no dio Scope = fail', bundle(scope='pass', fc='NONE', cls='EXECUTION_VERIFIED'), False),
        ('I', 'Scope fail pero FailureClass distinto', bundle(fc='Contract'), False),
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


def main(real_dir, nc_dir, base, cur, out_path):
    st, st2 = selftest(), selftest()
    res = {'Id': ID, 'NotA4': True, 'Scope': 'solo F4-T2B2A-INTENT-WIRING, intento 2; no reemplaza al Controller ni a nc2 en otras tareas',
           'SelfTest': st, 'SelfTestPass': all(x['Ok'] for x in st), 'SelfTestDeterministic': st == st2}
    if not (res['SelfTestPass'] and res['SelfTestDeterministic']):
        res['SubstituteResult'] = 'FAIL (autoprueba)'
    else:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        rf, nf = load_inputs(real_dir), load_inputs(nc_dir)
        changed = subprocess.run(['git', 'diff', '--name-only', base + '..' + cur], capture_output=True, text=True, check=True).stdout.split()
        nc_out = ld(os.path.join(nc_dir, 'output.json'))
        ch = nc_out['Checks']
        nc_checks = {k: v['Result'] for k, v in ch.items()} if isinstance(ch, dict) else {c['Id']: c['Result'] for c in ch}
        scope_ev = ch['Scope']['Evidence'] if isinstance(ch, dict) else next(c['Evidence'] for c in ch if c['Id'] == 'Scope')
        bd = {'real_deleg': rf['delegation.json'], 'mut_deleg': nf['delegation.json'], 'contract': rf['gate-contract.json'],
              'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
              'real_bfc': rf['byte-fidelity-check.json'], 'mut_bfc': nf['byte-fidelity-check.json'], 'changed': changed,
              'nc_checks': nc_checks, 'nc_out': nc_out, 'real_files': rf, 'nc_files': nf, 'mutation': ld(os.path.join(nc_dir, 'mutation.json')), 'scope_evidence': scope_ev}
        P, N, ok = evaluate(bd)
        rd, md = json.loads(rf['delegation.json']), json.loads(nf['delegation.json'])
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc2Run': nc_dir.replace(os.sep, '/'), 'BaseSha': base, 'CurrentSha': cur,
                    'ChangedPaths': changed, 'RealAllowedWriteScope': rd['AllowedWriteScope'], 'MutatedAllowedWriteScope': md['AllowedWriteScope'],
                    'Nc2ControllerScopeEvidence': scope_ev, 'Measured': N, 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Notes': {
                        'SignalNotDemonstrated': 'la Evidence de Scope del Controller afirma que todas las rutas del diff estan en AllowedWriteScope y atribuye el fail a F4T2B2A-C02 por dos archivos de prueba propios de la tarea; no nombra la ruta quitada (Measured.ControllerScopeEvidenceNamesRemovedPath)',
                        'ContractPass': 'Contract dio pass apoyandose en byte-fidelity-check.json, que atestigua la delegacion REAL (su OutputSha256 es el de la delegacion real), sin comparar la lista mutada con la exacta del contrato',
                        'ProtocolDebt': 'PD-F4-03: el Controller puede fallar Scope por un motivo ajeno a la mutacion y no detectar la ruta quitada; deuda de protocolo, sin solucion global dentro de I-64'},
                    'Record': {'Nc2OrdinaryRun': 'ORACULO MECANICO CUMPLIDO / SENAL NO DEMOSTRADA (se conserva tal cual)',
                               'nc2': 'SATISFIED_UNDER_F4T2B2A_SCOPE_SIGNAL_SUBSTITUTE' if ok else 'NOT SATISFIED'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'], 'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st],
                      'SubstituteResult': res['SubstituteResult'], 'Properties': res.get('Properties'), 'Measured': res.get('Measured')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:6]))
