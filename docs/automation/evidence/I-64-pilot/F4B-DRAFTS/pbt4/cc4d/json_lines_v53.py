"""PB-T4d (paquete v5.3): los golpes de unas lineas de un archivo de src/ en una o varias coberturas JSON de coverlet (para ver si una linea que
cambia entre la base y el GREEN es ruido de la medida). Solo lee. Imprime una linea por JSON.

Uso: python -I json_lines_v53.py <ruta-que-acaba-en> <lineas,separadas,por,comas> <json>...
"""
import collections
import json
import sys

END, LINES = sys.argv[1], [int(x) for x in sys.argv[2].split(',')]
for path in sys.argv[3:]:
    data = json.load(open(path, encoding='utf-8'))
    hits = collections.Counter()
    for module in data.values():
        for file_path, classes in module.items():
            if not file_path.replace('\\', '/').endswith(END):
                continue
            for cls, methods in classes.items():
                for method, body in methods.items():
                    for n, h in body.get('Lines', {}).items():
                        if int(n) in LINES:
                            hits[int(n)] += h
    print(path, ' '.join('%d:%d' % (n, hits[n]) for n in LINES))
