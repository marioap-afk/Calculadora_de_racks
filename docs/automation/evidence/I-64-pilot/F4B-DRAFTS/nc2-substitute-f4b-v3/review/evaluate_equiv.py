"""Revision v3 (solo lectura): equivalencia a nivel de evaluate() y de la autoprueba entre la v2 y la v3 (modulos cargados del texto sin modificar,
con el bloque __main__ desactivado).
1) selftest() de cada version dos veces: misma lista, mismos casos, mismos resultados, determinista.
2) synthetic(): los 57 paquetes de la v2 y de la v3 son iguales; evaluate() de cada version sobre cada paquete: P igual y N igual salvo
   AlternativeCandidatesExcludingPath.
3) perturbaciones aleatorias de los paquetes sinteticos (alcances del contrato, de la delegacion real y de la mutada, diff y prohibido, con prefijos y
   duplicados, n <= 12), con los hashes del bfc recalculados para que la propiedad 1 pueda seguir en true: P y N iguales salvo el campo informativo,
   o la misma clase de excepcion. Comprueba ademas P1 y P2 => P10 (v3).
4) elementos no cadena en el alcance (contrato = delegacion real): resultado global de las dos versiones.
Uso: python -I -B evaluate_equiv.py <v2.py> <v3.py> <salida.json> [semilla]"""
import copy, hashlib, json, random, sys, time, types

v2p, v3p, outp = sys.argv[1], sys.argv[2], sys.argv[3]
SEED = int(sys.argv[4]) if len(sys.argv) > 4 else 7
MAIN = "if __name__ == '__main__':"
ALT = 'AlternativeCandidatesExcludingPath'
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')


def load(path, name):
    src = open(path, encoding='utf-8').read()
    assert src.count(MAIN) == 1
    mod = types.ModuleType(name)
    mod.__file__ = path
    exec(compile(src.replace(MAIN, 'if False:'), name, 'exec'), mod.__dict__)
    return mod


m2, m3 = load(v2p, 'v2'), load(v3p, 'v3')
out = {}
# 1) autoprueba
st2a, st2b, st3a, st3b = m2.selftest(), m2.selftest(), m3.selftest(), m3.selftest()
out['SelfTest'] = {'V2Count': len(st2a), 'V3Count': len(st3a), 'V2AllOk': all(x['Ok'] for x in st2a), 'V3AllOk': all(x['Ok'] for x in st3a),
                   'V2Deterministic': st2a == st2b, 'V3Deterministic': st3a == st3b, 'V2EqualsV3': st2a == st3a,
                   'Cases': [x['Case'] for x in st3a], 'PassCases': [x['Case'] for x in st3a if x['Result'] == 'PASS']}
# casos de la autoprueba en los que la propiedad 10 es falsa, y si alguno falla SOLO por la 10
s2, s3 = m2.synthetic(), m3.synthetic()
same_bundles = len(s2) == len(s3) and all(a[0] == b[0] and a[1] == b[1] and a[2] == b[2] and a[3] == b[3] for a, b in zip(s2, s3))
rows, only10 = [], []
for (c, d, bd, exp) in s2:
    P2, N2, ok2 = m2.evaluate(copy.deepcopy(bd))
    P3, N3, ok3 = m3.evaluate(copy.deepcopy(bd))
    n2, n3 = dict(N2), dict(N3)
    a2, a3 = n2.pop(ALT), n3.pop(ALT)
    rows.append({'Case': c, 'Same': P2 == P3 and n2 == n3 and ok2 == ok3, 'P10': P3['10_NoAlternativeOneFieldMutation'], 'Alt_v2': a2})
    if [k for k, v in P3.items() if not v] == ['10_NoAlternativeOneFieldMutation']:
        only10.append(c)
out['Synthetic'] = {'SameBundles': same_bundles, 'Count': len(s2), 'AllSame': all(r['Same'] for r in rows),
                    'P10FalseCases': [r['Case'] for r in rows if not r['P10']], 'CasesFailingOnlyBy10': only10, 'Rows': rows}
# 3) perturbaciones aleatorias
rnd = random.Random(SEED)
U = ['src/X/A.cs', 'src/X/B.cs', 'src/X/', 'src/', 'tests/T/TA.cs', 'tests/Fx/', 'tests/Fx/a.json', 'tests/Fx/b.json', 'docs/', 'src/Y/', 'src/Y/C.cs', 'a', 'a/']
base = [(c, bd) for (c, d, bd, exp) in s2 if c in ('A', 'N', 'O', 'AY')]
stats = {'n': 0, 'same': 0, 'diff': 0, 'exc_same': 0, 'P10True': 0, 'P10False': 0, 'P1P2True': 0, 'ImplicationViolations': 0, 'OverallPASS': 0}
diffs = []
t0 = time.time()
for trial in range(6000):
    c, bd0 = rnd.choice(base)
    bd = copy.deepcopy(bd0)
    ct, rd, md = json.loads(bd['contract']), json.loads(bd['real_deleg']), json.loads(bd['mut_deleg'])
    k = rnd.random()
    pick = lambda n: [rnd.choice(U) for _ in range(n)]
    if k < 0.3:  # contrato = real, alcance nuevo; mutada por README 10
        aws = pick(rnd.randrange(0, 13))
        ct['AllowedWriteScope'] = aws; rd['AllowedWriteScope'] = list(aws)
    elif k < 0.5:
        ct['AllowedWriteScope'] = pick(rnd.randrange(0, 13))
    elif k < 0.7:
        rd['AllowedWriteScope'] = pick(rnd.randrange(0, 13))
    if rnd.random() < 0.5:
        bd['changed'] = pick(rnd.randrange(0, 7))
    if rnd.random() < 0.3:
        f = pick(rnd.randrange(0, 4))
        ct['ForbiddenWriteScope'] = f
        if rnd.random() < 0.7:
            rd['ForbiddenWriteScope'] = list(f)
    md = copy.deepcopy(rd)
    if bd['changed'] and rnd.random() < 0.8:
        md['AllowedWriteScope'] = m2.readme10_mutation(rd['AllowedWriteScope'], bd['changed'])[1]
    else:
        md['AllowedWriteScope'] = pick(rnd.randrange(0, 13))
    bd['contract'], bd['real_deleg'], bd['mut_deleg'] = enc(ct), enc(rd), enc(md)
    bfc = json.loads(bd['real_bfc'])
    bfc['OutputSha256'], bfc['ContractSha256'] = sha(bd['real_deleg']), sha(bd['contract'])
    bd['real_bfc'] = bd['mut_bfc'] = enc(bfc)
    r = []
    for m in (m2, m3):
        try:
            P, N, ok = m.evaluate(copy.deepcopy(bd))
            N = dict(N); N.pop(ALT)
            r.append(('ok', P, N, ok))
        except Exception as ex:  # noqa
            r.append(('exc', type(ex).__name__))
    stats['n'] += 1
    if r[0] == r[1]:
        stats['same'] += 1
        if r[0][0] == 'exc':
            stats['exc_same'] += 1
        else:
            P = r[1][1]
            stats['P10True' if P['10_NoAlternativeOneFieldMutation'] else 'P10False'] += 1
            if P['1_AcceptedRealDelegationMatchesContract'] and P['2_RealChangedPathInsideAcceptedScope']:
                stats['P1P2True'] += 1
                if not P['10_NoAlternativeOneFieldMutation']:
                    stats['ImplicationViolations'] += 1
            if r[1][3]:
                stats['OverallPASS'] += 1
    else:
        stats['diff'] += 1
        if len(diffs) < 20:
            diffs.append({'base': c, 'v2': str(r[0])[:500], 'v3': str(r[1])[:500], 'ct_aws': ct['AllowedWriteScope'], 'rd_aws': rd['AllowedWriteScope'], 'changed': bd['changed']})
stats['seconds'] = round(time.time() - t0, 1)
out['Perturbations'] = {'Seed': SEED, 'Stats': stats, 'Diffs': diffs}
# 4) no cadena en el alcance (contrato y delegacion real), sobre el caso O (F4B, PASS)
o = next(bd for (c, d, bd, exp) in s2 if c == 'O')
probes = []
for desc, extra in [('int al final', 5), ('null al final', None), ('lista al final', ['x']), ('objeto al final', {'k': 1})]:
    bd = copy.deepcopy(o)
    ct, rd = json.loads(bd['contract']), json.loads(bd['real_deleg'])
    ct['AllowedWriteScope'] = ct['AllowedWriteScope'] + [extra]; rd['AllowedWriteScope'] = rd['AllowedWriteScope'] + [extra]
    bd['contract'], bd['real_deleg'] = enc(ct), enc(rd)
    res = []
    for m in (m2, m3):
        try:
            P, N, ok = m.evaluate(copy.deepcopy(bd))
            res.append({'Overall': 'PASS' if ok else 'FAIL', 'False': sorted(k for k, v in P.items() if not v)})
        except Exception as ex:  # noqa
            res.append({'Overall': 'FAIL (excepcion)', 'Exception': type(ex).__name__})
    probes.append({'Probe': desc, 'v2': res[0], 'v3': res[1]})
out['NonStringProbes'] = probes
open(outp, 'w', encoding='utf-8', newline='\n').write(json.dumps(out, ensure_ascii=False, indent=1) + '\n')
print(json.dumps({k: v for k, v in out['SelfTest'].items() if k != 'Cases'}, ensure_ascii=False))
print(json.dumps({k: v for k, v in out['Synthetic'].items() if k != 'Rows'}, ensure_ascii=False))
print(json.dumps(out['Perturbations']['Stats'], ensure_ascii=False), 'diffs:', len(diffs))
for p in probes:
    print('PROBE', json.dumps(p, ensure_ascii=False))
