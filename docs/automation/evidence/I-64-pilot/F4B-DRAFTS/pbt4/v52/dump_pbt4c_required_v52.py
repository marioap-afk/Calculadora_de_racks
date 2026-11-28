"""PB-T4d (paquete v5.2): los RequiredTests del contrato de PB-T4c emitido (por objetos git de 6c2820ba), en TSV, para heredarlos.
Uso: python -I dump_pbt4c_required_v52.py <repo> <salida.tsv>"""
import json
import subprocess
import sys

REPO, OUT = sys.argv[1:3]
PATH = 'docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json'
raw = subprocess.run(['git', '-C', REPO, 'show', '6c2820bab4d8b41e3e229e23f3a1638ccbb04735:' + PATH], capture_output=True, check=True).stdout
contract = json.loads(raw.decode('utf-8-sig'))
rows = contract['RequiredTests']
keys = sorted({k for r in rows for k in r})
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\t'.join(keys) + '\n')
    for r in rows:
        f.write('\t'.join(str(r.get(k, '')) for k in keys) + '\n')
print(len(rows), keys)
