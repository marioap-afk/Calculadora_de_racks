"""F4T3A-NC2-EXACT-WHITELIST-01: sustituto determinista de nc2 solo para F4-T3A-EDIT-PORT (intento 1), bajo la lista blanca exacta de F4-E21. No es A-4.
La corrida ordinaria de nc2 se conserva tal cual como CONTROL NOT PASSED; este sustituto no la reemplaza ni la reetiqueta.
Uso (desde la raiz del worktree): python nc2_exact_whitelist_substitute.py <dir-verificacion-real> <dir-nc2> <BaseSha> <CurrentSha> <salida.json>
Demuestra las propiedades 1-10 de la disposicion del Owner (F4-E29). Antes ejecuta su autoprueba sintetica (dos veces, determinista).
Solo biblioteca estandar; lee JSON y ejecuta git diff --name-only en local."""
import copy, hashlib, itertools, json, os, subprocess, sys

ID = 'F4T3A-NC2-EXACT-WHITELIST-01'
EXACT_CLAUSE = 'AllowedWriteScope de la delegacion = exactamente el AllowedWriteScope del contrato.'
SEMANTIC_OPAQUE = ['Objective', 'Authorities', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions']
sha = lambda b: hashlib.sha256(b).hexdigest().upper()


def covered(path, scope):
    return any(path == e or (e.endswith('/') and path.startswith(e)) for e in scope)


def scope_predicate(changed, allowed, forbidden):
    """Comprobacion Scope de 16.9: diff contenido en AllowedWriteScope y sin interseccion con ForbiddenWriteScope."""
    return 'pass' if all(covered(p, allowed) for p in changed) and not any(covered(p, forbidden) for p in changed) else 'fail'


def entry_covered_by(e, scope):
    """16.5: un prefijo de A solo lo cubre un prefijo de B que lo contenga."""
    if e.endswith('/'):
        return any(b.endswith('/') and e.startswith(b) for b in scope)
    return covered(e, scope)


def subset(a, b):
    return all(entry_covered_by(e, b) for e in a)


def stop_ids(x):
    return {s['Id'] if isinstance(s, dict) else s for s in x}


def plain_a3_a5(deleg, contract):
    """A3-A5 de 16.5 sin la clausula de exactitud: A3 subconjunto, A4 superconjunto, A5 superconjunto con MinSelected >= y el mismo ExpectRed."""
    rt_c = {t['Filter']: t for t in contract.get('RequiredTests', [])}
    rt_d = {t['Filter']: t for t in deleg.get('RequiredTests', [])}
    rt_ok = all(f in rt_d and rt_d[f].get('MinSelected', 0) >= rt_c[f].get('MinSelected', 0) and rt_d[f].get('ExpectRed') == rt_c[f].get('ExpectRed') for f in rt_c)
    auth = lambda x: {json.dumps(a, sort_keys=True, ensure_ascii=False) for a in x}
    return {'A3': subset(deleg.get('AllowedWriteScope', []), contract.get('AllowedWriteScope', [])),
            'A4': subset(contract.get('ForbiddenWriteScope', []), deleg.get('ForbiddenWriteScope', [])),
            'A5': set(contract.get('Invariants', [])) <= set(deleg.get('Invariants', []))
                  and stop_ids(contract.get('StopConditions', [])) <= stop_ids(deleg.get('StopConditions', []))
                  and auth(contract.get('Authorities', [])) <= auth(deleg.get('Authorities', [])) and rt_ok}


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
    """README 10: el primer archivo del diff en orden lexicografico sale del alcance; si lo cubre un prefijo, el prefijo se sustituye por los demas archivos del diff que cubria."""
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
    """b: real_deleg, mut_deleg, contract (bytes); real_rrp, mut_rrp (bytes); real_bfc, mut_bfc (bytes); changed (lista);
    nc_checks (dict Id -> Result); nc_failure_class; real_files, nc_files (nombre -> bytes); mutation (dict)."""
    P, N = {}, {}
    rd, md, ct = json.loads(b['real_deleg']), json.loads(b['mut_deleg']), json.loads(b['contract'])
    ra, ma, ca = rd.get('AllowedWriteScope', []), md.get('AllowedWriteScope', []), ct.get('AllowedWriteScope', [])
    changed = b['changed']
    bfc = json.loads(b['real_bfc'])
    bfc_fields = {f['Field']: f for f in bfc.get('Fields', [])}
    # 1. la delegacion real aceptada coincide con el contrato
    P['1_AcceptedRealDelegationMatchesContract'] = (ra == ca and all(rd.get(k) == ct.get(k) for k in SEMANTIC_OPAQUE)
                                                   and bfc.get('OutputSha256', '').upper() == sha(b['real_deleg']) and bfc.get('ContractSha256', '').upper() == sha(b['contract'])
                                                   and all(bfc_fields.get(k, {}).get('Utf8BytesEqual') is True for k in ['AllowedWriteScope'] + SEMANTIC_OPAQUE)
                                                   and all(plain_a3_a5(rd, ct).values()))
    # 2. la ruta cambiada real esta dentro del AllowedWriteScope aceptado (y todo el diff tambien)
    p_expected, _ = readme10_mutation(ra, changed) if changed else (None, None)
    removed = sorted(x for x in changed if covered(x, ra) and not covered(x, ma))
    p = removed[0] if len(removed) == 1 else None
    N['ChangedPath'] = p
    P['2_RealChangedPathInsideAcceptedScope'] = p is not None and p in changed and covered(p, ra) and scope_predicate(changed, ra, rd.get('ForbiddenWriteScope', [])) == 'pass'
    # 3. nc2 quita exactamente esa ruta (regla de README 10) y nada mas
    _, expect_mut = readme10_mutation(ra, changed) if changed else (None, None)
    P['3_Nc2RemovesExactlyThatPath'] = p is not None and p == p_expected and ma == expect_mut and all(covered(x, ma) for x in changed if x != p)
    # 4. ningun otro campo semantico de la delegacion cambia
    N['DelegationFieldsChanged'] = sorted(k for k in set(rd) | set(md) if rd.get(k) != md.get(k))
    P['4_NoOtherDelegationFieldChanges'] = N['DelegationFieldsChanged'] == ['AllowedWriteScope']
    # 5. los hashes derivados mecanicamente son coherentes
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
    # 6. tras la mutacion, la ruta cambiada no esta en el AllowedWriteScope
    P['6_ChangedPathNotInMutatedScope'] = p is not None and not covered(p, ma)
    # 7. por tanto Scope DEBE fallar (y pasa con el alcance real)
    N['ScopeOnReal'] = scope_predicate(changed, ra, rd.get('ForbiddenWriteScope', []))
    N['ScopeOnMutated'] = scope_predicate(changed, ma, md.get('ForbiddenWriteScope', []))
    P['7_ScopeMustFail'] = N['ScopeOnMutated'] == 'fail' and N['ScopeOnReal'] == 'pass'
    # 8. Contract falla solo porque el contrato exige la lista blanca exacta y la mutacion la viola a proposito
    N['PlainA3A5OnMutated'] = plain_a3_a5(md, ct)
    P['8_ContractFailsOnlyByExactWhitelist'] = (EXACT_CLAUSE in ct.get('ExpectedEvidence', []) and all(N['PlainA3A5OnMutated'].values())
                                               and ma != ca and all(md.get(k) == ct.get(k) for k in SEMANTIC_OPAQUE))
    # 9. el Controller informo Scope = fail
    N['ControllerScope'] = b['nc_checks'].get('Scope')
    N['ControllerContract'] = b['nc_checks'].get('Contract')
    P['9_ControllerReportedScopeFail'] = N['ControllerScope'] == 'fail'
    # 10. ninguna mutacion alternativa de un campo conserva el contrato exacto y a la vez excluye la ruta cambiada
    #     Solo AllowedWriteScope decide si la lista blanca admite la ruta (16.9 Scope). Como el contrato la cubre, toda lista que no la cubra difiere
    #     de la lista exacta del contrato. Se comprueba ademas por enumeracion: todos los subconjuntos del alcance del contrato y sus variantes de README 10.
    cands = []
    for r in range(len(ca) + 1):
        for comb in itertools.combinations(ca, r):
            cands.append(list(comb))
            if p is not None:
                cands.append(readme10_mutation(list(comb), changed)[1] if changed else list(comb))
    excl = [c for c in cands if p is not None and not covered(p, c)]
    N['AlternativeCandidatesExcludingPath'] = len(excl)
    P['10_NoAlternativeOneFieldMutation'] = p is not None and covered(p, ca) and len(excl) > 0 and all(c != ca for c in excl) \
        and not covered(p, ct.get('ForbiddenWriteScope', []))
    return P, N, all(P.values())


def synthetic():
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    A = ['src/X/A.cs', 'src/X/B.cs', 'tests/T/TA.cs', 'tests/Fx/']
    contract = {'Objective': 'o', 'Authorities': [{'Path': 'docs/a.md', 'Class': 'UNIT_DOC'}], 'AllowedWriteScope': A, 'ForbiddenWriteScope': ['src/Y/', 'docs/'],
                'Invariants': ['i1'], 'RequiredTests': [{'Filter': 'f', 'MinSelected': 10, 'ExpectRed': True}], 'ExpectedEvidence': ['e', EXACT_CLAUSE],
                'StopConditions': [{'Id': 'S-04', 'Text': 's'}]}
    real = dict(copy.deepcopy(contract), TaskId='T', Model='m')
    changed0 = ['src/X/A.cs', 'src/X/B.cs', 'tests/T/TA.cs']

    def bfc_for(rd_b, ct_b):
        fields = [{'Field': k, 'Utf8BytesEqual': True} for k in ['AllowedWriteScope'] + SEMANTIC_OPAQUE]
        return enc({'OutputSha256': sha(rd_b).lower(), 'ContractSha256': sha(ct_b).lower(), 'Fields': fields})

    def bundle(real=real, contract=contract, changed=changed0, mut_edit=None, scope='fail', bad_hash=False, extra_changed=False, mutate_bfc=False):
        rd = copy.deepcopy(real)
        md = copy.deepcopy(rd)
        md['AllowedWriteScope'] = readme10_mutation(rd['AllowedWriteScope'], changed)[1]
        if mut_edit:
            mut_edit(md)
        rdb, mdb, ctb = enc(rd), enc(md), enc(contract)
        rr0 = {'Outcome': {'OutputSha256': sha(rdb)}, 'Notes': ['n']}
        rr1 = copy.deepcopy(rr0); rr1['Outcome']['OutputSha256'] = ('0' * 64) if bad_hash else sha(mdb)
        bfc = bfc_for(rdb, ctb)
        common = {'gate-contract.json': ctb, 'ci-core/1/core.trx': b'trx', 'worker-handoff.json': b'{"h": 1}'}
        rf = dict(common, **{'delegation.json': rdb, 'relay-record-planning.json': enc(rr0), 'byte-fidelity-check.json': bfc})
        nf = dict(common, **{'delegation.json': mdb, 'relay-record-planning.json': enc(rr1), 'byte-fidelity-check.json': (bfc + b' ') if mutate_bfc else bfc})
        if extra_changed:
            nf['worker-handoff.json'] = b'{"h": 2}'
        mutation = {'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf[k]), 'InputSha256': {k: sha(v) for k, v in nf.items()}}
        return {'real_deleg': rdb, 'mut_deleg': mdb, 'contract': ctb, 'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
                'real_bfc': rf['byte-fidelity-check.json'], 'mut_bfc': nf['byte-fidelity-check.json'], 'changed': changed,
                'nc_checks': {'Contract': 'fail', 'Scope': scope}, 'real_files': rf, 'nc_files': nf, 'mutation': mutation}

    def keep(md): md['AllowedWriteScope'] = A
    def second(md): md['Model'] = 'otro'
    def forb(md): md['ForbiddenWriteScope'] = ['src/Y/']
    def add(md): md['AllowedWriteScope'] = md['AllowedWriteScope'] + ['src/X/Z.cs']
    def drop_inv(md): md['Invariants'] = []
    real_loose = dict(copy.deepcopy(real), AllowedWriteScope=A[:3])
    no_clause = dict(copy.deepcopy(contract), ExpectedEvidence=['e'])
    real_nc = dict(copy.deepcopy(real), ExpectedEvidence=['e'])
    def not_in_diff(md): md['AllowedWriteScope'] = A[:3]
    pref_changed = ['tests/Fx/a.json', 'tests/Fx/b.json', 'tests/T/TA.cs']
    A2 = ['tests/T/TA.cs', 'tests/Fx/']
    ct_pref = dict(copy.deepcopy(contract), AllowedWriteScope=A2)
    real_pref = dict(copy.deepcopy(ct_pref), TaskId='T', Model='m')
    return [
        ('A', 'ruta exacta cambiada; la mutacion de README 10 la quita; Controller con Scope = fail', bundle(), True),
        ('B', 'el alcance mutado sigue cubriendo la ruta cambiada', bundle(mut_edit=keep), False),
        ('C', 'cambia un segundo campo de la delegacion', bundle(mut_edit=second), False),
        ('D', 'cambia ForbiddenWriteScope', bundle(mut_edit=forb), False),
        ('E', 'la delegacion real no es la lista exacta del contrato', bundle(real=real_loose), False),
        ('F', 'el hash derivado no coincide con los bytes de la delegacion mutada', bundle(bad_hash=True), False),
        ('G', 'el Controller no informo Scope = fail', bundle(scope='pass'), False),
        ('H', 'el contrato no exige la lista blanca exacta', bundle(contract=no_clause, real=real_nc), False),
        ('I', 'la mutacion anade otra ruta', bundle(mut_edit=add), False),
        ('J', 'cambia otra entrada ademas de las derivadas', bundle(extra_changed=True), False),
        ('K', 'cambia la atestacion de fidelidad de la delegacion real', bundle(mutate_bfc=True), False),
        ('L', 'A5 llano tambien falla (se quitan invariantes): Contract no falla solo por la exactitud', bundle(mut_edit=drop_inv), False),
        ('M', 'la ruta quitada no esta en el diff', bundle(mut_edit=not_in_diff), False),
        ('N', 'prefijo: README 10 sustituye el prefijo por los demas archivos del diff', bundle(real=real_pref, contract=ct_pref, changed=pref_changed), True),
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
    res = {'Id': ID, 'NotA4': True, 'Scope': 'solo F4-T3A-EDIT-PORT, intento 1; no reemplaza al Controller ni a nc2 en otras tareas',
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
        bd = {'real_deleg': rf['delegation.json'], 'mut_deleg': nf['delegation.json'], 'contract': rf['gate-contract.json'],
              'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
              'real_bfc': rf['byte-fidelity-check.json'], 'mut_bfc': nf['byte-fidelity-check.json'], 'changed': changed,
              'nc_checks': nc_checks, 'real_files': rf, 'nc_files': nf, 'mutation': ld(os.path.join(nc_dir, 'mutation.json'))}
        P, N, ok = evaluate(bd)
        rd, md = json.loads(rf['delegation.json']), json.loads(nf['delegation.json'])
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc2Run': nc_dir.replace(os.sep, '/'), 'BaseSha': base, 'CurrentSha': cur,
                    'ChangedPaths': changed, 'RealAllowedWriteScope': rd['AllowedWriteScope'], 'MutatedAllowedWriteScope': md['AllowedWriteScope'],
                    'Nc2ControllerClassification': nc_out.get('Classification'), 'Nc2ControllerDisposition': nc_out.get('Disposition'),
                    'Nc2ControllerFailureClass': nc_out.get('FailureClass'), 'Nc2ControllerChecks': nc_checks,
                    'Measured': N, 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Notes': {
                        'ByteFidelityAttestation': 'byte-fidelity-check.json no cambia en la copia de nc2: atestigua la delegacion REAL (su OutputSha256 es el SHA de la delegacion real) y sigue siendo cierta; no es un hash derivado de la copia mutada',
                        'ForbiddenRoute': 'anadir la ruta al ForbiddenWriteScope no la excluye del AllowedWriteScope (fuera de la propiedad 10), no es la mutacion que README 10 define para nc2 y romperia la copia exacta aceptada que registra byte-fidelity-check.json',
                        'WhyOrdinaryOracleUnreachable': 'bajo la lista blanca exacta, quitar una ruta del AllowedWriteScope viola a la vez la clausula de exactitud del contrato (Contract, comprobacion 4) y la contencion del diff (Scope, comprobacion 7); el orden fijo de 16.9 da FailureClass = Contract'},
                    'Record': {'Nc2OrdinaryRun': 'CONTROL NOT PASSED (se conserva tal cual)',
                               'nc2': 'SATISFIED_UNDER_F4T3A_EXACT_WHITELIST_SUBSTITUTE' if ok else 'NOT SATISFIED'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'],
                      'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st], 'SubstituteResult': res['SubstituteResult'],
                      'Properties': res.get('Properties'), 'Measured': res.get('Measured')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:6]))
