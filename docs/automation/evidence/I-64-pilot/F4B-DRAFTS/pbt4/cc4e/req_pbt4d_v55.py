"""PB-T4e (paquete v5.5), precondicion 6: los RequiredTests del contrato emitido de PB-T4d (F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json, por
objetos git en la base), en su orden, como TSV: n, Project, Filter, MinSelected, ExpectRed. Son los heredados de PB-T4e. Solo lee; escribe UTF-8
con LF.

Uso: python -I req_pbt4d_v55.py <repo> <rev> <salida.tsv>
"""
import json
import subprocess
import sys

sys.dont_write_bytecode = True
repo, rev, out = sys.argv[1:4]
path = 'docs/automation/evidence/I-64-pilot/F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json'
raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig')
data = json.loads(raw)


def find(node):
    if isinstance(node, dict):
        for k, v in node.items():
            if k == 'RequiredTests':
                return v
            r = find(v)
            if r is not None:
                return r
    elif isinstance(node, list):
        for v in node:
            r = find(v)
            if r is not None:
                return r
    return None


req = find(data)
assert isinstance(req, list) and req, 'sin RequiredTests'
keys = sorted({k for r in req for k in r})
lines = ['n\tProject\tFilter\tMinSelected\tExpectRed']
for i, r in enumerate(req, 1):
    lines.append('\t'.join([str(i), r['Project'], r['Filter'], str(r['MinSelected']), 'true' if r['ExpectRed'] else 'false']))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('RequiredTests de PB-T4d: %d; claves %s; TaskId %s' % (len(req), keys, data.get('TaskId')))
