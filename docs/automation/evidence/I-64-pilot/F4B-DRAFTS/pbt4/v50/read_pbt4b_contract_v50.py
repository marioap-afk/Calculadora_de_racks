"""PB-T4c (v5.0): lee el contrato emitido de PB-T4b (gate-contract.json del cierre 6adff90d, guardado en v50/) y vuelca, para la v5.0, sus
RequiredTests, sus StopConditions, sus Invariants, su ExpectedEvidence y sus Authorities, en UTF-8 con LF. Solo lee el json; escribe la salida.
Uso: python -I read_pbt4b_contract_v50.py <gate-contract.json> <salida.txt>
"""
import json
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
src, out = sys.argv[1], sys.argv[2]
c = json.load(open(src, encoding='utf-8'))
lines = ['keys: ' + ', '.join(c.keys())]
for k in ('TaskId', 'AuthorityRevision', 'BaseSha', 'Attempts'):
    if k in c:
        lines.append('%s: %s' % (k, json.dumps(c[k], ensure_ascii=False)))
rt = c.get('RequiredTests', [])
lines.append('RequiredTests: %d' % len(rt))
for i, r in enumerate(rt, 1):
    lines.append('%d\t%s\t%s\t%s\t%s' % (i, r.get('Project'), r.get('Filter'), r.get('MinSelected'), r.get('ExpectRed')))
for key in ('Authorities', 'AllowedWriteScope', 'Invariants', 'StopConditions', 'ExpectedEvidence'):
    v = c.get(key, [])
    lines.append('%s: %d' % (key, len(v) if isinstance(v, list) else -1))
    if isinstance(v, list):
        for i, x in enumerate(v):
            lines.append('  [%d] %s' % (i, x if isinstance(x, str) else json.dumps(x, ensure_ascii=False)))
fws = c.get('ForbiddenWriteScope', [])
lines.append('ForbiddenWriteScope: %d' % len(fws))
for k, v in c.items():
    if k not in ('RequiredTests', 'Authorities', 'AllowedWriteScope', 'Invariants', 'StopConditions', 'ExpectedEvidence', 'ForbiddenWriteScope', 'Objective'):
        if not isinstance(v, (list, dict)):
            lines.append('%s = %s' % (k, json.dumps(v, ensure_ascii=False)))
        else:
            lines.append('%s = %s' % (k, json.dumps(v, ensure_ascii=False)[:600]))
lines.append('Objective: ' + c.get('Objective', ''))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('lineas=%d' % len(lines))
