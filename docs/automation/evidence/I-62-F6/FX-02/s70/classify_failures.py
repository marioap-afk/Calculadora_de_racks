# Classify every failed key of the two Controller probes (A4-1 cmd route; A2-P2 native) into cause categories (§70, point 4).
import collections, json, os
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
OD = os.path.join(WT, 'docs', 'automation', 'evidence', 'I-62-F6', 'OD-2')
RUNS = {'A4-1 (cmd.exe, 73890CA3)': 'R20261010T000542Z-a4-1-probe', 'A2-P2 sonda 1 (pwsh nativo, F909F480)': 'R20261011T050156Z-a2p2-v4-controller'}
SANDBOX_STEPS = {'9'}
REPL = chr(0xFFFD)


def nonascii(x):
    return any(ord(ch) > 127 for ch in json.dumps(x, ensure_ascii=False))


def cat(k, v, stepok):
    step = k.split(':')[0]
    got = v.get('got')
    exp = v.get('expected')
    gs = got if isinstance(got, str) else json.dumps(got, ensure_ascii=False)
    if step in SANDBOX_STEPS and (gs in ('', 'FALTA') or 'remote' in gs.lower()):
        return 'SANDBOX'
    if gs in ('', 'FALTA', None):
        return 'OMITIDA'
    if REPL in gs or chr(0) in gs or ('\\u00' in gs) or (nonascii(exp) and not nonascii(got)):
        return 'UTF8'
    if k.endswith('nonascii-count') or k.endswith('nonascii-chars'):
        return 'UTF8'
    return 'ERROR_MODELO'


out = {}
for label, rid in RUNS.items():
    c = json.load(open(os.path.join(OD, rid, 'comparison.json'), encoding='utf-8'))
    o = json.load(open(os.path.join(OD, rid, 'probe-last.json'), encoding='utf-8'))
    stepok = {str(s['id']): s.get('ok') for s in o['steps']}
    per = collections.Counter()
    ex = collections.defaultdict(list)
    for k, v in c['FailedKeys'].items():
        cc = cat(k, v, stepok.get(k.split(':')[0]))
        per[cc] += 1
        if len(ex[cc]) < 6:
            ex[cc].append(k)
    out[label] = {'Total': len(c['FailedKeys']), 'PorCausa': dict(per), 'Ejemplos': dict(ex),
                  'Comandos': c.get('EventsCommandAudit', {}).get('commands'), 'Proceso': c.get('Process')}
json.dump(out, open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'failure-classes.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
for k, v in out.items():
    print(k, v['Total'], v['PorCausa'], v['Comandos'])
    for c2, e in v['Ejemplos'].items():
        print('   ', c2, e)
