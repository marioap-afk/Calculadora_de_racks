"""Resume los resultados de checks/. Uso: python -I summarize_checks.py <dir-v2>"""
import json, os, sys

D = os.path.join(sys.argv[1], 'checks')
for name in sorted(os.listdir(D)):
    if not name.endswith('.json'):
        continue
    d = json.load(open(os.path.join(D, name), encoding='utf-8'))
    print(name, '|', d['SubstituteResult'], '| falsas:', sorted(k for k, v in (d.get('Properties') or {}).items() if not v), '| binding:', (d.get('TaskBinding') or {}).get('Ok'),
          '| record:', (d.get('Record') or {}).get('nc2'), '| id:', d.get('Id'))
