"""PB-T4e (paquete v5.6): valores de PRUEBA para un ensayo en seco de make_v56.py (no la version). Lee las claves del bloque values de
p6_json_pbt4e_v56.py y escribe <dir>/values-v56.json con 'ENSAYO-<CLAVE>' en cada una, y copia los RequiredTests de la v5.5
(required-pbt4e.tsv) como <dir>/required-pbt4e-v56.tsv. El ensayo escribe en <dir>; la version solo sale de post_p6_v56.sh.

Uso (desde v56/): python -I ensayo_values_v56.py <p6_json_pbt4e_v56.py> <required-pbt4e.tsv> <dir>
"""
import json
import os
import re
import shutil
import sys

sys.dont_write_bytecode = True
src, req, out = sys.argv[1:4]
text = open(src, encoding='utf-8').read()
block = text[text.index('    values = {'):]
block = block[:block.index('\n    }\n')]
keys = re.findall(r"'([A-Z0-9_]+)':", block)
assert len(keys) == len(set(keys)) and 'COVX_CORE' in keys and 'COVX_U18' in keys, keys
os.makedirs(out, exist_ok=True)
with open(os.path.join(out, 'values-v56.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps({k: 'ENSAYO-' + k for k in keys}, indent=2) + '\n')
shutil.copyfile(req, os.path.join(out, 'required-pbt4e-v56.tsv'))
print('claves %d: %s' % (len(keys), ' '.join(keys)))
