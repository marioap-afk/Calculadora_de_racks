"""Recomprobacion: matriz de la autoprueba del sustituto corregido (40 casos), conjunto EXACTO de propiedades falsas por caso,
y comparacion de A-N con el original (mismos paquetes salvo note/raw_output = None, mismos vectores)."""
import importlib.util, json, os, sys

BASE = sys.argv[1]


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


orig = load('orig_sub', os.path.join(BASE, 'data', 'pbt2a-substitute-as-run.py'))
adap = load('adap_sub2', os.path.join(BASE, 'nc2_exact_whitelist_substitute_f4b.py'))
ob = {c: (d, bd, e) for c, d, bd, e in orig.synthetic()}
cases = adap.synthetic()
out = {'CaseCount': len(cases), 'Cases': {}, 'Mismatches': []}
for c, d, bd, e in cases:
    P, N, ok = adap.evaluate(bd)
    fp = sorted(k for k, v in P.items() if not v)
    exp_set = adap.EXPECT_FAILED.get(c)
    row = {'Description': d, 'ExpectedGlobal': 'PASS' if e else 'FAIL', 'Global': 'PASS' if ok else 'FAIL', 'FalseProps': fp,
           'ExpectFailed': sorted(exp_set) if exp_set is not None else None, 'Mode': N.get('Mode'), 'ExactWhitelistBy': N.get('ExactWhitelistBy')}
    if c in ob:
        o_bd = dict(ob[c][1])
        a_bd = dict(bd)
        extra = {k: a_bd.pop(k, 'MISSING') for k in ('note', 'raw_output')}
        oP, _, ook = orig.evaluate(ob[c][1])
        row['SameBundleAsOriginal'] = (o_bd == a_bd and all(v is None for v in extra.values()))
        row['SameVectorAsOriginal'] = (sorted(k for k, v in oP.items() if not v) == fp and ook == ok)
        if not (row['SameBundleAsOriginal'] and row['SameVectorAsOriginal']):
            out['Mismatches'].append(c)
    if (ok != e) or (exp_set is not None and set(fp) != exp_set):
        out['Mismatches'].append(c)
    out['Cases'][c] = row
st1, st2 = adap.selftest(), adap.selftest()
out['SelfTestOk'] = sum(1 for x in st1 if x['Ok'])
out['SelfTestPass'] = all(x['Ok'] for x in st1)
out['SelfTestDeterministic'] = st1 == st2
out['CasesWithoutExactSet'] = [c for c, _, _, _ in cases if c not in adap.EXPECT_FAILED]
print(json.dumps(out, ensure_ascii=False, indent=1))
