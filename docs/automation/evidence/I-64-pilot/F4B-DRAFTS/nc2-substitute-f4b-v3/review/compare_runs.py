"""Revision v3 (solo lectura): compara campo a campo dos salidas del sustituto (JSON) y lista TODAS las rutas que difieren.
Uso: python -I -B compare_runs.py <a.json> <b.json> [<a.json> <b.json> ...]"""
import json, sys


def walk(o, path=''):
    if isinstance(o, dict):
        if not o:
            yield path, '{}'
        for k, v in o.items():
            yield from walk(v, path + '.' + k)
    elif isinstance(o, list):
        if not o:
            yield path, '[]'
        for i, v in enumerate(o):
            yield from walk(v, '%s[%d]' % (path, i))
    else:
        yield path, o


args = sys.argv[1:]
for a, b in zip(args[0::2], args[1::2]):
    ja, jb = json.load(open(a, encoding='utf-8')), json.load(open(b, encoding='utf-8'))
    wa, wb = dict(walk(ja)), dict(walk(jb))
    diff = sorted(k for k in set(wa) | set(wb) if wa.get(k, '<ausente>') != wb.get(k, '<ausente>'))
    print('== %s  vs  %s' % (a.split('/')[-1], b.split('/')[-1]))
    print('  SubstituteResult:', ja.get('SubstituteResult'), '|', jb.get('SubstituteResult'))
    pa, pb = ja.get('Properties') or {}, jb.get('Properties') or {}
    print('  Properties iguales:', pa == pb, '| falsas:', sorted(k for k, v in pa.items() if not v), sorted(k for k, v in pb.items() if not v), '| n:', len(pa), len(pb))
    ma, mb = dict(ja.get('Measured') or {}), dict(jb.get('Measured') or {})
    alt = (ma.pop('AlternativeCandidatesExcludingPath', '<ausente>'), mb.pop('AlternativeCandidatesExcludingPath', '<ausente>'))
    print('  Measured iguales salvo AlternativeCandidatesExcludingPath:', ma == mb, '| Alt:', alt)
    print('  SelfTest iguales:', ja.get('SelfTest') == jb.get('SelfTest'), '| casos:', len(ja.get('SelfTest') or []), len(jb.get('SelfTest') or []),
          '| Ok:', sum(1 for x in ja.get('SelfTest') or [] if x.get('Ok')), sum(1 for x in jb.get('SelfTest') or [] if x.get('Ok')),
          '| SelfTestPass/Deterministic:', ja.get('SelfTestPass'), ja.get('SelfTestDeterministic'), jb.get('SelfTestPass'), jb.get('SelfTestDeterministic'))
    print('  Record:', (ja.get('Record') or {}).get('nc2'), '|', (jb.get('Record') or {}).get('nc2'))
    print('  rutas que difieren (%d):' % len(diff))
    for k in diff[:40]:
        print('    %s: %s | %s' % (k, str(wa.get(k, '<ausente>'))[:110], str(wb.get(k, '<ausente>'))[:110]))
