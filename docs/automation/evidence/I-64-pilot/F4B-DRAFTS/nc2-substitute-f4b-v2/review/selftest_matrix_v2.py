"""Recomprobacion v2: autoprueba (63 casos), conjunto exacto de propiedades por caso, A-AX identicos al v1 (6ccaf12d),
y que AY-BE solo cambien el literal en lo que dicen. Uso: python -I -B selftest_matrix_v2.py <dir-v2>"""
import difflib, importlib.util, json, os, sys

D = sys.argv[1]


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


v1 = load('sub_v1', os.path.join(D, 'base-6ccaf12d-original.py'))
v2 = load('sub_v2', os.path.join(D, 'nc2_exact_whitelist_substitute_f4b_v2.py'))
c1 = {c: (d, bd, e) for c, d, bd, e in v1.synthetic()}
c2 = v2.synthetic()
out = {'SyntheticCount': len(c2), 'Cases': {}, 'Mismatches': []}
EV_T3 = v2.F4B_EXACT_EVIDENCES[1]
for c, d, bd, e in c2:
    P, N, ok = v2.evaluate(bd)
    fp = sorted(k for k, v in P.items() if not v)
    exp = v2.EXPECT_FAILED.get(c)
    row = {'Global': 'PASS' if ok else 'FAIL', 'Expected': 'PASS' if e else 'FAIL', 'FalseProps': fp,
           'ExpectFailed': sorted(exp) if exp is not None else None, 'Matched': N.get('ExactEvidenceMatched'), 'By': N.get('ExactWhitelistBy')}
    if c in c1:
        P1_, _, ok1 = v1.evaluate(c1[c][1])
        row['SameBundleAsV1'] = c1[c][1] == bd
        row['SameVectorAsV1'] = sorted(k for k, v in P1_.items() if not v) == fp and ok1 == ok
        if not (row['SameBundleAsV1'] and row['SameVectorAsV1']):
            out['Mismatches'].append(c)
    else:  # casos nuevos: cual es la entrada del contrato y en que difiere del literal de PB-T3
        ee = json.loads(bd['contract'])['ExpectedEvidence']
        ent = ee[1]
        sm = difflib.SequenceMatcher(None, EV_T3, ent, autojunk=False)
        row['EntryEqualsT3'] = ent == EV_T3
        row['EntryEqualsT2'] = ent == v2.F4B_EXACT_EVIDENCES[0]
        row['DiffVsT3'] = [(t, EV_T3[i1:i2], ent[j1:j2]) for t, i1, i2, j1, j2 in sm.get_opcodes() if t != 'equal']
        row['RealDelegationEqualsContractEE'] = json.loads(bd['real_deleg'])['ExpectedEvidence'] == ee
    if ok != e or (exp is not None and set(fp) != exp):
        out['Mismatches'].append(c)
    out['Cases'][c] = row
st1, st2 = v2.selftest(), v2.selftest()
out['SelfTestCount'] = len(st1)
out['SelfTestOk'] = sum(1 for x in st1 if x['Ok'])
out['SelfTestDeterministic'] = st1 == st2
out['TaskCases'] = [(x['Case'], x['Description'], x['Result'], x['Expected'], x['Ok']) for x in st1 if x['Case'].startswith('T') and len(x['Case']) == 2 and x['Case'][1].isdigit()]
# check_task en bordes que la autoprueba no cubre
ct = v2.check_task
out['CheckTaskEdges'] = {
    'PBT3A sobre F4B-PBT3A (sin sufijo)': ct('PBT3A', 'F4B-PBT3A'),
    'PBT3A sobre F4B-PBT3A-': ct('PBT3A', 'F4B-PBT3A-'),
    'PBT3A sobre f4b-pbt3a-x (minusculas)': ct('PBT3A', 'f4b-pbt3a-x'),
    'VALIDATION sobre F4B-': ct('VALIDATION', 'F4B-'),
    'VALIDATION sobre F4B-X': ct('VALIDATION', 'F4B-X'),
    'PBT2A sobre F4B-PBT2A-X': ct('PBT2A', 'F4B-PBT2A-X'),
    'None como task': ct(None, 'F4B-PBT3A-X'),
}
out['A_AX_Count'] = sum(1 for c in out['Cases'] if c in c1)
print(json.dumps(out, ensure_ascii=False, indent=1))
