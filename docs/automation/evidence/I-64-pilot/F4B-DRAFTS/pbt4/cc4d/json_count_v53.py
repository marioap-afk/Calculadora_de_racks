"""PB-T4d (paquete v5.3): cuantas lineas ejecutadas (hits > 0) tiene cada cobertura JSON de coverlet, por ensamblado. Solo lee.

Uso: python -I json_count_v53.py <json>...
"""
import json
import sys

for path in sys.argv[1:]:
    data = json.load(open(path, encoding='utf-8'))
    parts = []
    for module, files in data.items():
        n = 0
        for file_path, classes in files.items():
            seen = set()
            for cls, methods in classes.items():
                for method, body in methods.items():
                    for line, h in body.get('Lines', {}).items():
                        if h > 0:
                            seen.add(int(line))
            n += len(seen)
        parts.append('%s=%d' % (module.replace('.dll', ''), n))
    print(path.split('/')[-1], ' '.join(parts))
