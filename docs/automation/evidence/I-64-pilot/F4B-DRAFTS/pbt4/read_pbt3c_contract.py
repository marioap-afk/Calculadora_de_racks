"""Lee por objetos git el contrato de PB-T3c y vuelca lo que la v4.1 necesita copiar: claves, TaskId, StopConditions,
RequiredTests, ExpectedEvidence, invariantes y el resto de campos de texto.

Uso: python -I read_pbt3c_contract.py <checkout> <rev> <salida.txt>
"""
import json
import subprocess
import sys

repo, rev, out = sys.argv[1:4]
p = subprocess.run(['git', '-C', repo, 'show', rev + ':docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json'], capture_output=True)
if p.returncode:
    raise SystemExit('git show -> %d' % p.returncode)
c = json.loads(p.stdout.decode('utf-8'))
lines = ['bytes=%d' % len(p.stdout), 'keys=' + ', '.join(c.keys()), '']


def dump(k, v, ind=''):
    if isinstance(v, dict):
        lines.append('%s%s: {' % (ind, k))
        for kk, vv in v.items():
            dump(kk, vv, ind + '  ')
        lines.append(ind + '}')
    elif isinstance(v, list):
        lines.append('%s%s: [%d]' % (ind, k, len(v)))
        for i, vv in enumerate(v):
            dump('[%d]' % i, vv, ind + '  ')
    else:
        lines.append('%s%s: %s' % (ind, k, json.dumps(v, ensure_ascii=False)))


for k, v in c.items():
    dump(k, v)
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('ok', len(lines))
