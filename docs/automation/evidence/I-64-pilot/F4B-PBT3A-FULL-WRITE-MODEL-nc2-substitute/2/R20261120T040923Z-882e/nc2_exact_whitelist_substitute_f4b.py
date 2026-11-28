"""PBT3A-NC2-EXACT-WHITELIST-F4B-01: sustituto determinista de nc2 solo para F4B-PBT3A-FULL-WRITE-MODEL (intento 2, verificacion R20261120T034433Z-6d41), copia de PBT2A-NC2-EXACT-WHITELIST-01, que a su vez es copia de F4T1B-NC2-EXACT-WHITELIST-01, adaptada a F4B-E21 solo en las propiedades 5 y 8, por la firma exacta de PD-F4-01 (PBT2A-C11, F4B-E03), bajo la lista blanca exacta de F4-E21. No es A-4.
Dos modos, acoplados por los argumentos. Con la nota de inyeccion y la salida cruda de la planificacion (modo F4B-E21): propiedad 5 = el relevo registra el SHA de la salida CRUDA del Controller, que se lee y se ata a la nota, a las 8 colecciones opacas vacias (calculadas) y a delegation.json real (salida cruda con las 8 colecciones del contrato = delegacion real, igualdad JSON y byte a byte con la serializacion de inject_opaque.py; las 8 claves presentes en la salida cruda con '' exacto en Objective y [] exacto en las otras 7; JSON sin claves duplicadas); la nota se valida entera (RunId no vacio e igual en nota, bfc, relevo y delegacion; Fidelity empieza por PASS). Propiedad 8 = ExpectedEvidence contiene como entrada exacta F4B_EXACT_EVIDENCE y byte-fidelity-check.json atestigua la comparacion (Result PASS, AllowedWriteScopeElementwiseInOrder true, AllowedWriteScope en Fields). Sin ellos (modo F4): propiedades 5 y 8 exactamente como en el original (EXACT_CLAUSE).
Recalculo del relevo de nc2 (Nc2KitRelayRecompute): el kit de nc2 recalcula Outcome.OutputSha256 del relevo de la copia con la regla de F4 (= SHA de la delegacion mutada). El sustituto exige exactamente eso, en los dos modos, y solo eso. Una copia que siguiera F4B-E21 seria otra mutacion del kit, fuera de este sustituto.
La corrida ordinaria de nc2 se conserva tal cual como CONTROL NOT PASSED; este sustituto no la reemplaza ni la reetiqueta.
Uso (desde un checkout que comparta objetos con BaseSha y CurrentSha): python nc2_exact_whitelist_substitute_f4b.py <dir-verificacion-real> <dir-nc2> <BaseSha> <CurrentSha> <salida.json> [<session-inject-note.json> <salida-cruda-planificacion/output.json>]
Demuestra las propiedades 1-10 de la disposicion del Owner (F4-E29). Antes ejecuta su autoprueba sintetica (dos veces, determinista).
Solo biblioteca estandar; lee JSON y ejecuta git diff --name-only en local."""
import copy, hashlib, itertools, json, os, subprocess, sys

ID = 'PBT3A-NC2-EXACT-WHITELIST-F4B-01'
EXACT_CLAUSE = 'AllowedWriteScope de la delegacion = exactamente el AllowedWriteScope del contrato.'
F4B_EXACT_EVIDENCE = 'Antes del Worker: el contrato, la salida del Controller (output.json, con las colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json), la comparacion byte a byte, A1-A8 y nc4.'
SEMANTIC_OPAQUE = ['Objective', 'Authorities', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions']
INJECTED = ['AllowedWriteScope'] + SEMANTIC_OPAQUE
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
up = lambda v: v.upper() if isinstance(v, str) else None


def no_duplicate_keys(pairs):
    keys = [k for k, _ in pairs]
    if len(keys) != len(set(keys)):
        raise ValueError('clave duplicada en un objeto JSON')
    return dict(pairs)


def loads_strict(b):
    """json.loads que rechaza claves duplicadas (ValueError)."""
    return json.loads(b, object_pairs_hook=no_duplicate_keys)


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
    note, raw_b = b.get('note'), b.get('raw_output')
    f4b_mode = note is not None or raw_b is not None  # modo F4B-E21: exige las dos; un solo argumento no es un modo valido
    N['Mode'] = 'F4B-E21' if f4b_mode else 'F4'
    if not f4b_mode:
        real_rule = rr0['Outcome']['OutputSha256'].upper() == sha(b['real_deleg'])
    elif not isinstance(note, dict) or raw_b is None:
        real_rule = False
    else:  # F4B-E21: el relevo registra el SHA de la salida CRUDA; la nota se valida entera y se ata a la salida cruda leida
        try:  # la salida cruda y la delegacion real se leen rechazando claves duplicadas
            raw, rd_strict = loads_strict(raw_b), loads_strict(b['real_deleg'])
        except ValueError:
            raw = rd_strict = None
        raw_sha = sha(raw_b)
        N['RawOutputSha256'] = raw_sha
        if not isinstance(raw, dict) or not isinstance(rd_strict, dict):
            real_rule = False
        else:
            computed_nonempty = [k for k in INJECTED if raw.get(k)]
            # las 8 claves presentes en la salida cruda, con '' exacto en Objective y [] exacto en las otras 7
            raw_empty = all(k in raw and (raw[k] == '' and isinstance(raw[k], str) if k == 'Objective' else raw[k] == [] and isinstance(raw[k], list)) for k in INJECTED)
            injected = dict(raw)
            for k in INJECTED:
                injected[k] = ct.get(k)
            # serializacion de inject_opaque.py: json.dumps(indent=2, ensure_ascii=False) + '\n', UTF-8, orden de la salida cruda
            bytes_equal = (json.dumps(injected, ensure_ascii=False, indent=2) + '\n').encode('utf-8') == b['real_deleg']
            N['RawNonEmptyComputed'] = computed_nonempty
            N['RawCollectionsEmptyExact'] = raw_empty
            N['RawPlusContractCollectionsEqualsDelegationJson'] = injected == rd_strict
            N['RawPlusContractCollectionsEqualsDelegationBytes'] = bytes_equal
            inj = note.get('Injected')
            fid, dec, rid = note.get('Fidelity'), note.get('Decision'), note.get('RunId')
            real_rule = (up(rr0['Outcome'].get('OutputSha256')) == up(note.get('OutputSha256')) == raw_sha
                         and raw_empty and computed_nonempty == [] and note.get('NonEmptyInControllerOutput') == computed_nonempty
                         and injected == rd_strict and bytes_equal
                         and isinstance(inj, list) and all(isinstance(x, str) for x in inj) and len(inj) == len(set(inj)) == len(INJECTED) and sorted(inj) == sorted(INJECTED)
                         and isinstance(rid, str) and rid != '' and rid == bfc.get('PlanningRunId') == rr0.get('RunId') == rd_strict.get('RunId')
                         and up(note.get('ContractSha256')) == sha(b['contract'])
                         and up(note.get('DelegationSha256')) == sha(b['real_deleg']) == up(bfc.get('OutputSha256'))
                         and isinstance(fid, str) and fid.startswith('PASS') and isinstance(dec, str) and dec.startswith('F4B-E21'))
    P['5_DerivedHashesConsistent'] = (sorted(b['real_files']) == sorted(b['nc_files'])
                                      and changed_inputs == ['delegation.json', 'relay-record-planning.json'] == sorted(mut.get('ChangedVsVerifiedInputs', []))
                                      and diffpaths(rr0, rr1) == ['.Outcome.OutputSha256']
                                      and real_rule
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
    ee = ct.get('ExpectedEvidence', [])
    if not f4b_mode:
        N['ExactWhitelistBy'] = 'EXACT_CLAUSE' if EXACT_CLAUSE in ee else None
    else:  # F4B-E21: la exactitud viene de la comparacion byte a byte que el contrato exige (entrada exacta) y que el bfc atestigua
        by_bytes = (F4B_EXACT_EVIDENCE in ee and bfc.get('Result') == 'PASS' and bfc.get('AllowedWriteScopeElementwiseInOrder') is True
                    and 'AllowedWriteScope' in bfc_fields)
        N['ExactWhitelistBy'] = 'F4B_EXACT_EVIDENCE+BFC' if by_bytes else None
    P['8_ContractFailsOnlyByExactWhitelist'] = (N['ExactWhitelistBy'] is not None and all(N['PlainA3A5OnMutated'].values())
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

    def bfc_for(rd_b, ct_b, f4b=False, bfc_edit=None, run_id=None):
        fields = [{'Field': k, 'Utf8BytesEqual': True} for k in ['AllowedWriteScope'] + SEMANTIC_OPAQUE]
        o = {'OutputSha256': sha(rd_b).lower(), 'ContractSha256': sha(ct_b).lower(), 'Fields': fields}
        if f4b:
            o.update({'Result': 'PASS', 'AllowedWriteScopeElementwiseInOrder': True})
            if run_id is not None:
                o['PlanningRunId'] = run_id
        if bfc_edit:
            bfc_edit(o)
        return enc(o)

    def bundle(real=real, contract=contract, changed=changed0, mut_edit=None, scope='fail', bad_hash=False, extra_changed=False, mutate_bfc=False,
               f4b=False, note_edit=None, bfc_edit=None, raw_edit=None, rr0_sha=None, raw_bytes_edit=None, run_id='RUN1', rd_run=0):
        run_ids = (run_id, run_id if rd_run == 0 else rd_run)  # (nota, bfc y relevo; delegacion real y salida cruda); None = ausente
        rd = copy.deepcopy(real)
        if f4b and run_ids[1] is not None:
            rd['RunId'] = run_ids[1]
        md = copy.deepcopy(rd)
        md['AllowedWriteScope'] = readme10_mutation(rd['AllowedWriteScope'], changed)[1]
        if mut_edit:
            mut_edit(md)
        rdb, mdb, ctb = enc(rd), enc(md), enc(contract)
        note, raw_b, raw_sha = None, None, None
        if f4b:  # salida cruda del Controller = delegacion real con las 8 colecciones vacias (como la deja el Controller en F4B-E21)
            raw = copy.deepcopy(rd)
            for k in INJECTED:
                raw[k] = '' if k == 'Objective' else []
            if raw_edit:
                raw_edit(raw)
            raw_b = enc(raw)
            if raw_bytes_edit:
                raw_b = raw_bytes_edit(raw_b)
            raw_sha = sha(raw_b)
            note = {'Decision': 'F4B-E21 (sintetico)', 'OutputSha256': raw_sha, 'ContractSha256': sha(ctb), 'DelegationSha256': sha(rdb),
                    'Injected': list(INJECTED), 'NonEmptyInControllerOutput': [], 'Fidelity': 'PASS sobre delegation.json'}
            if run_ids[0] is not None:
                note['RunId'] = run_ids[0]
            if note_edit:
                note_edit(note)
        rr0 = {'Outcome': {'OutputSha256': (rr0_sha or raw_sha) if f4b else sha(rdb)}, 'Notes': ['n']}
        if f4b and run_ids[0] is not None:
            rr0['RunId'] = run_ids[0]
        rr1 = copy.deepcopy(rr0); rr1['Outcome']['OutputSha256'] = ('0' * 64) if bad_hash else sha(mdb)
        bfc = bfc_for(rdb, ctb, f4b, bfc_edit, run_ids[0])
        common = {'gate-contract.json': ctb, 'ci-core/1/core.trx': b'trx', 'worker-handoff.json': b'{"h": 1}'}
        rf = dict(common, **{'delegation.json': rdb, 'relay-record-planning.json': enc(rr0), 'byte-fidelity-check.json': bfc})
        nf = dict(common, **{'delegation.json': mdb, 'relay-record-planning.json': enc(rr1), 'byte-fidelity-check.json': (bfc + b' ') if mutate_bfc else bfc})
        if extra_changed:
            nf['worker-handoff.json'] = b'{"h": 2}'
        mutation = {'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf[k]), 'InputSha256': {k: sha(v) for k, v in nf.items()}}
        return {'real_deleg': rdb, 'mut_deleg': mdb, 'contract': ctb, 'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
                'real_bfc': rf['byte-fidelity-check.json'], 'mut_bfc': nf['byte-fidelity-check.json'], 'changed': changed,
                'nc_checks': {'Contract': 'fail', 'Scope': scope}, 'real_files': rf, 'nc_files': nf, 'mutation': mutation, 'note': note, 'raw_output': raw_b}

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
    # F4B: el contrato no lleva EXACT_CLAUSE; la exactitud la exige la entrada exacta F4B_EXACT_EVIDENCE de ExpectedEvidence y la atestigua el bfc
    def pair(ev):
        ct = dict(copy.deepcopy(contract), ExpectedEvidence=ev)
        return ct, dict(copy.deepcopy(ct), TaskId='T', Model='m', Attempt=2, AttemptsRemaining=1)
    ct_f4b, real_f4b = pair(['e', F4B_EXACT_EVIDENCE])
    ct_f4b_none, real_f4b_none = pair(['e'])
    ct_f4b_clause, real_f4b_clause = pair(['e', EXACT_CLAUSE])
    neg = F4B_EXACT_EVIDENCE.replace('la comparacion byte a byte', 'sin la comparacion byte a byte')
    assert neg != F4B_EXACT_EVIDENCE
    ct_f4b_neg, real_f4b_neg = pair(['e', neg])
    f4b = dict(real=real_f4b, contract=ct_f4b, f4b=True)

    def note_set(**kv):
        def f(n): n.update(kv)
        return f
    def note_inj_dup(n): n['Injected'] = n['Injected'][:-1] + [n['Injected'][0]]
    def raw_nonempty(r): r['Invariants'] = ['x']
    def raw_model(r): r['Model'] = 'otro'
    def raw_float(r): r['Attempt'] = 2.0
    def raw_bool(r): r['AttemptsRemaining'] = True
    def raw_dupkey(b): return b.replace(b'  "Model": "m",', b'  "Model": "x",\n  "Model": "m",')
    def raw_nokey(r): del r['AllowedWriteScope']
    def raw_obj_list(r): r['Objective'] = []
    def raw_inv_str(r): r['Invariants'] = ''
    def bfc_order(o): o['AllowedWriteScopeElementwiseInOrder'] = False
    def bfc_result(o): o['Result'] = 'FAIL'
    def bfc_nofield(o): o['Fields'] = [f for f in o['Fields'] if f['Field'] != 'AllowedWriteScope']
    other = sha(b'otra salida')
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
        ('O', 'F4B-E21: relevo = SHA de la salida cruda, nota coherente, exactitud por la evidencia exacta y el bfc', bundle(**f4b), True),
        ('P', 'F4B: la nota no coincide con el relevo ni con la salida cruda (OutputSha256)', bundle(note_edit=note_set(OutputSha256=other), **f4b), False),
        ('Q', 'F4B: DelegationSha256 de la nota distinto del SHA de la delegacion', bundle(note_edit=note_set(DelegationSha256=other), **f4b), False),
        ('R', 'F4B: la nota no registra las 8 colecciones inyectadas (7)', bundle(note_edit=lambda n: n.update(Injected=n['Injected'][:-1]), **f4b), False),
        ('S', 'F4B: la nota dice NonEmptyInControllerOutput no vacio con la salida cruda vacia', bundle(note_edit=note_set(NonEmptyInControllerOutput=['Objective']), **f4b), False),
        ('T', 'F4B: el contrato no lleva la evidencia exacta ni EXACT_CLAUSE', bundle(real=real_f4b_none, contract=ct_f4b_none, f4b=True), False),
        ('U', 'F4B: el bfc con AllowedWriteScopeElementwiseInOrder = false', bundle(bfc_edit=bfc_order, **f4b), False),
        ('V', 'F4B: el bfc con Result = FAIL', bundle(bfc_edit=bfc_result, **f4b), False),
        ('W', 'F4B: el bfc sin AllowedWriteScope en Fields (solapa con la propiedad 1)', bundle(bfc_edit=bfc_nofield, **f4b), False),
        ('X', 'F4B: el relevo de nc2 no recalcula el SHA de la delegacion mutada', bundle(bad_hash=True, **f4b), False),
        ('Y', 'modo F4B con EXACT_CLAUSE en el contrato y sin la evidencia exacta: no se acepta (modos acoplados)', bundle(real=real_f4b_clause, contract=ct_f4b_clause, f4b=True), False),
        ('Z', 'modo F4 (sin nota ni salida cruda) con la evidencia F4B y sin EXACT_CLAUSE: no se acepta', bundle(real=real_f4b, contract=ct_f4b), False),
        ('AA', 'F4B: el relevo no coincide con la salida cruda (la nota si)', bundle(rr0_sha=other, **f4b), False),
        ('AB', 'F4B: relevo y nota coinciden entre si pero no con el SHA de la salida cruda leida (X2)', bundle(rr0_sha=other, note_edit=note_set(OutputSha256=other), **f4b), False),
        ('AC', 'F4B: una coleccion no vacia en la salida cruda y la nota lo declara', bundle(raw_edit=raw_nonempty, note_edit=note_set(NonEmptyInControllerOutput=['Invariants']), **f4b), False),
        ('AD', 'F4B: una coleccion no vacia en la salida cruda y la nota dice que no', bundle(raw_edit=raw_nonempty, **f4b), False),
        ('AE', 'F4B: la salida cruda con las 8 colecciones del contrato no es la delegacion real', bundle(raw_edit=raw_model, **f4b), False),
        ('AF', 'F4B: Injected con un elemento de mas', bundle(note_edit=lambda n: n['Injected'].append('Extra'), **f4b), False),
        ('AG', 'F4B: Injected con un elemento repetido (8 elementos, falta uno)', bundle(note_edit=note_inj_dup, **f4b), False),
        ('AH', 'F4B: RunId de la nota distinto del de byte-fidelity-check', bundle(note_edit=note_set(RunId='OTRO'), **f4b), False),
        ('AI', 'F4B: ContractSha256 de la nota distinto del SHA del contrato', bundle(note_edit=note_set(ContractSha256=other), **f4b), False),
        ('AJ', 'F4B: Fidelity de la nota sin PASS', bundle(note_edit=note_set(Fidelity='FAIL sobre delegation.json'), **f4b), False),
        ('AK', 'F4B: Decision de la nota que no empieza por F4B-E21', bundle(note_edit=note_set(Decision='otra decision'), **f4b), False),
        ('AL', 'F4B: entrada de ExpectedEvidence parecida pero negada', bundle(real=real_f4b_neg, contract=ct_f4b_neg, f4b=True), False),
        ('AO', 'M-R1: clave duplicada en la salida cruda (la ultima coincide con la delegacion)', bundle(raw_bytes_edit=raw_dupkey, **f4b), False),
        ('AP', 'M-R1: salida cruda con Attempt = 2.0 y la delegacion con 2 (iguales por JSON, distintos en bytes)', bundle(raw_edit=raw_float, **f4b), False),
        ('AQ', 'M-R1: salida cruda con AttemptsRemaining = true y la delegacion con 1', bundle(raw_edit=raw_bool, **f4b), False),
        ('AR', 'M-R1: salida cruda sin la clave AllowedWriteScope', bundle(raw_edit=raw_nokey, **f4b), False),
        ('AS', 'M-R1: salida cruda con Objective = [] en vez de la cadena vacia', bundle(raw_edit=raw_obj_list, **f4b), False),
        ('AT', 'M-R1: salida cruda con Invariants = "" en vez de la lista vacia', bundle(raw_edit=raw_inv_str, **f4b), False),
        ('AU', 'M-R2: RunId vacio en la nota, el bfc, el relevo y la delegacion', bundle(run_id='', **f4b), False),
        ('AV', 'M-R2: RunId ausente en la nota, el bfc, el relevo y la delegacion', bundle(run_id=None, **f4b), False),
        ('AW', 'M-R2: RunId de la delegacion real distinto del de la nota, el bfc y el relevo', bundle(rd_run='OTRO', **f4b), False),
        ('AX', 'M-R3: Fidelity contiene PASS pero no empieza por PASS', bundle(note_edit=note_set(Fidelity='FAIL (no PASS)'), **f4b), False),
        ('AM', 'F4B: nota sin salida cruda', dict(bundle(**f4b), raw_output=None), False),
        ('AN', 'F4B: salida cruda sin nota', dict(bundle(**f4b), note=None), False),
    ]


# Casos nuevos (O en adelante): el conjunto EXACTO de propiedades que deben fallar (vacio = pasa). Los casos A-N solo se juzgan por el resultado global.
P1, P5, P8 = '1_AcceptedRealDelegationMatchesContract', '5_DerivedHashesConsistent', '8_ContractFailsOnlyByExactWhitelist'
EXPECT_FAILED = dict({'O': set(), 'W': {P1, P8}},
                     **{c: {P5} for c in ['P', 'Q', 'R', 'S', 'X', 'AA', 'AB', 'AC', 'AD', 'AE', 'AF', 'AG', 'AH', 'AI', 'AJ', 'AK', 'AM', 'AN', 'AO', 'AP', 'AQ', 'AR', 'AS', 'AT', 'AU', 'AV', 'AW', 'AX']},
                     **{c: {P8} for c in ['T', 'U', 'V', 'Y', 'Z', 'AL']})


def selftest():
    out = []
    for c, d, bd, exp in synthetic():
        P, _, ok = evaluate(bd)
        hit = c not in EXPECT_FAILED or {k for k, v in P.items() if not v} == EXPECT_FAILED[c]
        out.append({'Case': c, 'Description': d, 'Result': 'PASS' if ok else 'FAIL', 'Expected': 'PASS' if exp else 'FAIL', 'Ok': ok == exp and hit})
    return out


def load_inputs(d):
    files = {}
    base = os.path.join(d, 'inputs')
    for r, _, fs in os.walk(base):
        for f in fs:
            p = os.path.join(r, f)
            files[os.path.relpath(p, base).replace(os.sep, '/')] = open(p, 'rb').read()
    return files


def read_json_object(path, what):
    raw = open(path, 'rb').read()
    obj = loads_strict(raw.decode('utf-8'))
    if not isinstance(obj, dict):
        raise ValueError(what + ' no es un objeto JSON')
    return raw, obj


def main(real_dir, nc_dir, base, cur, out_path, note_path=None, raw_path=None):
    if os.path.exists(out_path):
        os.remove(out_path)  # nunca queda un resultado anterior
    st, st2 = selftest(), selftest()
    mode = 'F4B-E21' if note_path and raw_path else ('F4' if not note_path and not raw_path else 'INCOMPLETO')
    res = {'Id': ID, 'NotA4': True, 'Scope': 'solo F4B-PBT3A-FULL-WRITE-MODEL, intento 2 (verificacion R20261120T034433Z-6d41); no reemplaza al Controller ni a nc2 en otras tareas',
           'Mode': mode, 'SelfTest': st, 'SelfTestPass': all(x['Ok'] for x in st), 'SelfTestDeterministic': st == st2}
    if not (res['SelfTestPass'] and res['SelfTestDeterministic']):
        res['SubstituteResult'] = 'FAIL (autoprueba)'
    elif mode == 'INCOMPLETO':
        res['SubstituteResult'] = 'FAIL (modo F4B-E21 incompleto: exige la nota de inyeccion y la salida cruda juntas)'
    else:
      try:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        note = raw_b = None
        if mode == 'F4B-E21':
            ab = lambda p: os.path.abspath(p).replace(os.sep, '/')
            nb, note = read_json_object(note_path, 'la nota de inyeccion')
            res['InjectNote'] = {'Path': ab(note_path), 'Sha256': sha(nb)}
            raw_b, _ = read_json_object(raw_path, 'la salida cruda de la planificacion')
            res['RawControllerOutput'] = {'Path': ab(raw_path), 'Sha256': sha(raw_b)}
        rf, nf = load_inputs(real_dir), load_inputs(nc_dir)
        changed = subprocess.run(['git', 'diff', '--name-only', base + '..' + cur], capture_output=True, text=True, check=True).stdout.split()
        nc_out = ld(os.path.join(nc_dir, 'output.json'))
        ch = nc_out['Checks']
        nc_checks = {k: v['Result'] for k, v in ch.items()} if isinstance(ch, dict) else {c['Id']: c['Result'] for c in ch}
        bd = {'real_deleg': rf['delegation.json'], 'mut_deleg': nf['delegation.json'], 'contract': rf['gate-contract.json'],
              'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
              'real_bfc': rf['byte-fidelity-check.json'], 'mut_bfc': nf['byte-fidelity-check.json'], 'changed': changed,
              'nc_checks': nc_checks, 'real_files': rf, 'nc_files': nf, 'mutation': ld(os.path.join(nc_dir, 'mutation.json')),
              'note': note, 'raw_output': raw_b}
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
                        'WhyOrdinaryOracleUnreachable': 'bajo la lista blanca exacta, quitar una ruta del AllowedWriteScope viola a la vez la exactitud de la lista que el contrato exige (' + (
                            'en F4B por la comparacion byte a byte de ExpectedEvidence' if mode == 'F4B-E21' else 'por la clausula EXACT_CLAUSE') +
                            '; Contract, comprobacion 4) y la contencion del diff (Scope, comprobacion 7); el orden fijo de 16.9 da FailureClass = Contract',
                        'Nc2KitRelayRecompute': 'el kit de nc2 recalcula Outcome.OutputSha256 del relevo de la copia con la regla de F4 (= SHA de la delegacion mutada); el sustituto exige exactamente eso y solo eso (rr1 = SHA(delegacion mutada)), en los dos modos. Una copia que siguiera F4B-E21 seria otra mutacion del kit, fuera de este sustituto'},
                    'Record': {'Nc2OrdinaryRun': 'CONTROL NOT PASSED (se conserva tal cual)',
                               'nc2': 'SATISFIED_UNDER_PBT3A_EXACT_WHITELIST_SUBSTITUTE_F4B' if ok else 'NOT SATISFIED'}})
      except Exception as ex:  # entrada ausente o mal formada: resultado FAIL con el motivo, sin excepcion
        res['SubstituteResult'] = 'FAIL (entrada ausente o mal formada: %s: %s)' % (type(ex).__name__, ex)
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'],
                      'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st], 'SubstituteResult': res['SubstituteResult'],
                      'Properties': res.get('Properties'), 'Measured': res.get('Measured')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:8]))
