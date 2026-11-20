"""Revision independiente: matriz de propiedades por caso de la autoprueba (original y adaptado).
Carga ambos modulos por ruta (sin ejecutar main) y compara los casos A-N entre original y adaptado."""
import importlib.util, json, os, sys

BASE = sys.argv[1]


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


orig = load('orig_sub', os.path.join(BASE, 'data', 'pbt2a-substitute-as-run.py'))
adap = load('adap_sub', os.path.join(BASE, 'nc2_exact_whitelist_substitute_f4b.py'))

out = {'Original': {}, 'Adapted': {}, 'A_N_SameVectors': None, 'A_N_SameBundles': None}
ob = {c: (d, bd, e) for c, d, bd, e in orig.synthetic()}
ab = {c: (d, bd, e) for c, d, bd, e in adap.synthetic()}
for c, (d, bd, e) in ob.items():
    P, N, ok = orig.evaluate(bd)
    out['Original'][c] = {'Expected': e, 'Ok': ok, 'FalseProps': sorted(k for k, v in P.items() if not v)}
for c, (d, bd, e) in ab.items():
    P, N, ok = adap.evaluate(bd)
    out['Adapted'][c] = {'Expected': e, 'Ok': ok, 'FalseProps': sorted(k for k, v in P.items() if not v),
                         'Target': adap.TARGET_PROPERTY.get(c), 'ExactWhitelistBy': N.get('ExactWhitelistBy'), 'Property5Rule': N.get('Property5Rule')}
same_vec, same_bundle = True, True
for c in ob:
    if out['Original'][c]['FalseProps'] != out['Adapted'][c]['FalseProps'] or out['Original'][c]['Ok'] != out['Adapted'][c]['Ok']:
        same_vec = False
    o_bd = dict(ob[c][1])
    a_bd = dict(ab[c][1])
    if a_bd.pop('note', 'MISSING') is not None:
        same_bundle = False
    if o_bd != a_bd:
        same_bundle = False
out['A_N_SameVectors'] = same_vec
out['A_N_SameBundles'] = same_bundle
# exactitud del objetivo: los casos nuevos negativos, que propiedades rompen ademas de la declarada
extra = {}
for c, t in adap.TARGET_PROPERTY.items():
    fp = out['Adapted'][c]['FalseProps']
    extra[c] = {'Target': t, 'TargetBroken': t in fp, 'AlsoBroken': [x for x in fp if x != t]}
out['NewNegativeCases'] = extra
st1, st2 = adap.selftest(), adap.selftest()
out['AdaptedSelfTestPass'] = all(x['Ok'] for x in st1)
out['AdaptedSelfTestDeterministic'] = st1 == st2
print(json.dumps(out, ensure_ascii=False, indent=1))
