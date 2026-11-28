"""PB-T4b, precondición 6 de la v4.8, paso 8: pbt4b-precondition6.json. Es el de `p6_check_v48.py json` (Schema rackcad-pbt4b-precondition6/v1,
Revision, PartitionSha256, CopySha de las 21 copias, R_C, R_UI y MinSelected de las 42 clases, por su nombre), con un campo más, `Cost`, el registro
del coste de la lectura según la regla de la sesión (v48/session-cost-rerun-rule.md): su resultado, «2 de 3; la decisión, del Owner», y las tres
corridas de la sonda, cada una con su TRX por sha256 y su resultado con todos los criterios de §6.2.1.

Uso: python -I p6_json_v48.py <particion.tsv> <apply-red.txt> <revision> <salida.json> <regla.md> (<nombre-trx>=<trx>=<check-cost.txt>)...
Solo lee; escribe solo la salida."""
import hashlib
import importlib.util
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
spec = importlib.util.spec_from_file_location('p6c48', os.path.join(HERE, 'p6_check_v48.py'))
p6 = importlib.util.module_from_spec(spec)
spec.loader.exec_module(p6)
part, apply_out, revision, out, rule = sys.argv[1:6]
runs = [a.split('=', 2) for a in sys.argv[6:]]
assert len(runs) == 3, runs
tmp = out + '.base'
p6.cmd_json(part, apply_out, revision, tmp)
doc = json.loads(open(tmp, 'rb').read().decode('utf-8'))
os.remove(tmp)
cost_runs, ok = [], 0
for name, trx, check in runs:
    last = [l for l in open(check, encoding='utf-8').read().split('\n') if l.startswith('COSTE: ')]
    blocks = [l for l in open(check, encoding='utf-8').read().split('\n') if l.startswith('== ')]
    assert len(blocks) == 1, (check, 'una sola corrida por comprobacion')
    result = last[-1][len('COSTE: '):]
    ok += result == 'CUMPLE'
    cost_runs.append({'Name': name, 'TrxSha256': hashlib.sha256(open(trx, 'rb').read()).hexdigest(), 'Result': result})
assert ok == 2, ok
doc['Cost'] = {'Result': '2 de 3; la decisión, del Owner', 'RuleSha256': hashlib.sha256(open(rule, 'rb').read()).hexdigest(), 'Runs': cost_runs}
data = (json.dumps(doc, ensure_ascii=True, indent=2) + '\n').encode('utf-8')
with open(out, 'wb') as f:
    f.write(data)
print('json sha256 %s; R_C=%d R_UI=%d; copias %d; clases %d (con pruebas %d); coste: %s' % (
    hashlib.sha256(data).hexdigest(), doc['R_C'], doc['R_UI'], len(doc['CopySha']), len(doc['MinSelected']),
    sum(1 for v in doc['MinSelected'].values() if v), doc['Cost']['Result']))
for r in cost_runs:
    print('  %s %s %s' % (r['Name'], r['TrxSha256'], r['Result']))
