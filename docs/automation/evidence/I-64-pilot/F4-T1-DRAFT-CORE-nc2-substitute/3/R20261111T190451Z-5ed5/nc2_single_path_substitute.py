"""F4T1-NC2-SINGLE-PATH-SCOPE-01: sustituto de nc2 solo para F4-T1, intento 3 (alcance delegado de una sola ruta). No es A-4.
Uso: python nc2_single_path_substitute.py <dir-de-la-verificacion-real> <dir-de-nc2> <BaseSha> <CurrentSha> <salida.json>
Demuestra de forma determinista las propiedades 1-10 de la disposicion del Owner; antes ejecuta su autoprueba (casos A-H y extras).
Solo biblioteca estandar; lee JSON y ejecuta git diff --name-only en local."""
import copy, json, subprocess, sys

EXPECTED_PATH = 'tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs'
EXACT_PHRASE = 'exactamente la unica ruta'


def covered(path, scope):
    return any(path == e or (e.endswith('/') and path.startswith(e)) for e in scope)


def scope_predicate(changed, allowed, forbidden):
    """La comprobacion Scope de 16.9: diff contenido en AllowedWriteScope y sin interseccion con ForbiddenWriteScope."""
    inside = all(covered(p, allowed) for p in changed)
    clean = not any(covered(p, forbidden) for p in changed)
    return 'pass' if inside and clean else 'fail'


def evaluate(real, mut, contract, changed, controller_scope, changed_inputs):
    P = {}
    ra, ma = real.get('AllowedWriteScope', []), mut.get('AllowedWriteScope', [])
    P['1_RealDelegationHasExactlyOneAllowedPath'] = len(ra) == 1
    P['2_ThatPathIsTheContractedPath'] = ra == [EXPECTED_PATH] and contract.get('AllowedWriteScope') == [EXPECTED_PATH]
    P['3_BaseToCurrentTouchesThatPath'] = EXPECTED_PATH in changed
    P['4_MutationRemovesExactlyThatPath'] = sorted(set(ra) - set(ma)) == [EXPECTED_PATH] and not (set(ma) - set(ra))
    P['5_ChangedPathOutsideMutatedScope'] = not covered(EXPECTED_PATH, ma)
    P['6_ScopePredicateOnMutatedScopeIsFail'] = scope_predicate(changed, ma, mut.get('ForbiddenWriteScope', [])) == 'fail'
    P['6b_ScopePredicateOnRealScopeIsPass'] = scope_predicate(changed, ra, real.get('ForbiddenWriteScope', [])) == 'pass'
    P['7_ForbiddenWriteScopeUnchanged'] = mut.get('ForbiddenWriteScope') == real.get('ForbiddenWriteScope')
    P['8_OnlyDelegationMutationIsAllowedWriteScope'] = sorted(k for k in set(real) | set(mut) if real.get(k) != mut.get(k)) == ['AllowedWriteScope'] \
        and changed_inputs == ['delegation.json', 'relay-record-planning.json']
    opaque = ['Authorities', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions', 'Objective']
    P['9_ContractFailureIsUnavoidableConsequence'] = (contract.get('AllowedWriteScope') == [EXPECTED_PATH]
                                                     and any(EXACT_PHRASE in x for x in contract.get('ExpectedEvidence', []))
                                                     and all(real.get(k) == contract.get(k) for k in opaque) and real.get('AllowedWriteScope') == contract.get('AllowedWriteScope')
                                                     and all(mut.get(k) == contract.get(k) for k in opaque) and mut.get('AllowedWriteScope') != contract.get('AllowedWriteScope'))
    P['10_ControllerReportedScopeFail'] = controller_scope == 'fail'
    return P, all(P.values())


def selftest():
    P_ = EXPECTED_PATH
    base_real = {'TaskId': 'F4-T1-DRAFT-CORE', 'Model': 'claude-sonnet-5-5', 'AllowedWriteScope': [P_], 'ForbiddenWriteScope': ['src/RackCad.Plugin/', 'docs/'],
                 'Authorities': ['a'], 'Invariants': ['i'], 'RequiredTests': ['t'], 'ExpectedEvidence': ['AllowedWriteScope de la delegacion = ' + EXACT_PHRASE + ' de este contrato.'],
                 'StopConditions': ['s'], 'Objective': 'o'}
    contract = {k: copy.deepcopy(v) for k, v in base_real.items() if k not in ('TaskId', 'Model')}
    ok_inputs = ['delegation.json', 'relay-record-planning.json']

    def case(real=None, mut_edit=None, changed=None, scope='fail', inputs=None, contract_=None):
        r = copy.deepcopy(base_real if real is None else real)
        m = copy.deepcopy(r)
        m['AllowedWriteScope'] = [x for x in m['AllowedWriteScope'] if x != P_]
        if mut_edit:
            mut_edit(m)
        return evaluate(r, m, contract if contract_ is None else contract_, [P_] if changed is None else changed, scope, ok_inputs if inputs is None else inputs)[1]

    def keep_path(m): m['AllowedWriteScope'] = [P_]
    def other_field(m): m['Model'] = 'otro-modelo'
    def forbidden(m): m['ForbiddenWriteScope'] = ['src/RackCad.Plugin/']
    def add_path(m): m['AllowedWriteScope'] = ['tests/RackCad.Tests/Workspace/Otro.cs']
    two = copy.deepcopy(base_real); two['AllowedWriteScope'] = [P_, 'tests/RackCad.Tests/Workspace/Otro.cs']
    cases = [
        ('A', 'una ruta real; se quita la ruta cambiada', case(), True),
        ('B', 'el alcance mutado sigue cubriendo la ruta cambiada', case(mut_edit=keep_path), False),
        ('C', 'la ruta cambiada es distinta de la quitada', case(changed=['tests/RackCad.Tests/Workspace/Otro.cs']), False),
        ('D', 'cambia un segundo campo de la delegacion', case(mut_edit=other_field), False),
        ('E', 'cambia ForbiddenWriteScope', case(mut_edit=forbidden), False),
        ('F', 'la delegacion real tiene mas de una ruta (no aplica)', case(real=two), False),
        ('G', 'BaseSha..CurrentSha no toca la ruta quitada', case(changed=['src/RackCad.Application/Workspace/Drafts/DraftSession.cs']), False),
        ('H', 'el Controller no informo Scope = fail', case(scope='pass'), False),
        ('X1', 'el alcance mutado anade otra ruta', case(mut_edit=add_path), False),
        ('X2', 'cambian otras entradas ademas de las derivadas', case(inputs=['delegation.json', 'relay-record-planning.json', 'worker-handoff.json']), False),
        ('X3', 'el contrato no exige la ruta exacta', case(contract_={**contract, 'ExpectedEvidence': ['otra cosa']}), False),
    ]
    return [{'Case': c, 'Description': d, 'Result': 'PASS' if got else 'FAIL', 'Expected': 'PASS' if exp else 'FAIL', 'Ok': got == exp} for c, d, got, exp in cases]


def main(real_dir, nc_dir, base, cur, out):
    st = selftest()
    res = {'Id': 'F4T1-NC2-SINGLE-PATH-SCOPE-01', 'NotA4': True, 'SelfTest': st, 'SelfTestPass': all(x['Ok'] for x in st)}
    if not res['SelfTestPass']:
        res['SubstituteResult'] = 'FAIL (autoprueba)'
    else:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        real, mut, contract = ld(real_dir + '/inputs/delegation.json'), ld(nc_dir + '/inputs/delegation.json'), ld(real_dir + '/inputs/gate-contract.json')
        changed = subprocess.run(['git', 'diff', '--name-only', base + '..' + cur], capture_output=True, text=True, check=True).stdout.split()
        out_nc = ld(nc_dir + '/output.json')
        ch = out_nc['Checks']
        scope = (ch.get('Scope') or {}).get('Result') if isinstance(ch, dict) else next(c['Result'] for c in ch if c.get('Id') == 'Scope')
        mutation = ld(nc_dir + '/mutation.json')
        P, ok = evaluate(real, mut, contract, changed, scope, mutation.get('ChangedVsVerifiedInputs'))
        res.update({'RealVerification': real_dir, 'Nc2Run': nc_dir, 'BaseSha': base, 'CurrentSha': cur, 'ChangedPaths': changed,
                    'RealAllowedWriteScope': real['AllowedWriteScope'], 'MutatedAllowedWriteScope': mut['AllowedWriteScope'],
                    'Nc2ControllerClassification': out_nc.get('Classification'), 'Nc2ControllerFailureClass': out_nc.get('FailureClass'), 'Nc2ControllerScope': scope,
                    'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Record': {'Nc2Run': 'CONTROL NOT PASSED (se conserva tal cual)', 'nc2': 'SATISFIED_UNDER_F4T1_SINGLE_PATH_SCOPE_SUBSTITUTE' if ok else 'NOT SATISFIED'}})
    open(out, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st], 'SubstituteResult': res['SubstituteResult'],
                      'Properties': res.get('Properties')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:6]))
