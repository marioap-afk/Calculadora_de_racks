"""Lista los filtros de UI de los RequiredTests de un contrato (solo lectura), uno por linea: <Filter>. Uso: python -I ui_filters.py <contrato.json>"""
import json, sys
c = json.load(open(sys.argv[1], encoding='utf-8'))
rt = c.get('RequiredTests') or []
for r in rt:
    proj = r.get('Project') or ''
    if 'UI' in proj:
        print(r.get('Filter') or r.get('Name'))
