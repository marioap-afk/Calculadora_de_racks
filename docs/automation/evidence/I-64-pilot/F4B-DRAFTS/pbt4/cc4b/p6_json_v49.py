"""PB-T4b, precondición 6 de la v4.9, paso 8: pbt4b-precondition6.json. Es el de `p6_check_v48.py json` (Schema rackcad-pbt4b-precondition6/v1,
Revision, PartitionSha256, CopySha de las 21 copias, R_C, R_UI y MinSelected de las 42 clases, por su nombre), sobre los pasos 1 a 6 de la v4.9, con el
campo `Cost`: el paso 7 se queda con las corridas de la v4.8 (owner-pbt4b-cost-decision.md, la séptima tanda), porque no cambia ninguna de sus
entradas (step7_inputs_v49.py). `Cost` remite a la decisión del Owner, a la regla de la tercera corrida, al JSON de la v4.8 y a las tres corridas, cada
una con su TRX por sha256 y su resultado, y lleva el sha256 de la comprobación de las entradas.

Uso: python -I p6_json_v49.py <particion.tsv> <apply-red.txt> <revision> <salida.json> <decision.md> <regla.md> <json-v48> <comprobacion-paso7.txt>
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
part, apply_out, revision, out, decision, rule, json48, step7 = sys.argv[1:9]


def h(path):
    return hashlib.sha256(open(path, 'rb').read()).hexdigest()


assert open(step7, encoding='utf-8').read().rstrip('\n').endswith('EL PASO 7 DE LA v4.8 SIGUE VALIENDO: SI'), step7
old = json.loads(open(json48, 'rb').read().decode('utf-8'))
assert old['Cost']['Result'] == '2 de 3; la decisión, del Owner' and len(old['Cost']['Runs']) == 3
tmp = out + '.base'
p6.cmd_json(part, apply_out, revision, tmp)
doc = json.loads(open(tmp, 'rb').read().decode('utf-8'))
os.remove(tmp)
assert doc['PartitionSha256'] == old['PartitionSha256'] and doc['R_C'] == old['R_C'] and doc['R_UI'] == old['R_UI'] and doc['MinSelected'] == old['MinSelected']
doc['Cost'] = {
    'Result': 'Dado por cumplido por el Owner (owner-pbt4b-cost-decision.md, la septima tanda): 2 de 3 corridas de la v4.8',
    'DecisionSha256': h(decision),
    'RuleSha256': h(rule),
    'FromPackage': 'v4.8',
    'FromPrecondition6JsonSha256': h(json48),
    'Runs': old['Cost']['Runs'],
    'Step7InputsUnchanged': True,
    'Step7InputsCheckSha256': h(step7),
}
data = (json.dumps(doc, ensure_ascii=True, indent=2) + '\n').encode('utf-8')
with open(out, 'wb') as f:
    f.write(data)
print('json sha256 %s; R_C=%d R_UI=%d; copias %d; clases %d (con pruebas %d); coste: %s' % (
    hashlib.sha256(data).hexdigest(), doc['R_C'], doc['R_UI'], len(doc['CopySha']), len(doc['MinSelected']),
    sum(1 for v in doc['MinSelected'].values() if v), doc['Cost']['Result']))
changed = sorted(k for k in doc['CopySha'] if doc['CopySha'][k] != old['CopySha'].get(k))
print('CopySha que cambian frente a la v4.8: %s' % changed)
