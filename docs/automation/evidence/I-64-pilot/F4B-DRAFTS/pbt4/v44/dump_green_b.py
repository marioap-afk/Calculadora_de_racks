"""Vuelca el GREEN literal de PB-T4b (green_pbt4b.py) sobre 5aaacce3 a v44/green_b/, para leerlo y medirlo. Solo objetos git. Uso: python -I dump_green_b.py"""
import hashlib, importlib.util, os, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
P4 = os.path.dirname(HERE)
REPO = r'D:' + chr(92) + 'Documentos' + chr(92) + 'Codex' + chr(92) + 'Calculadora de racks'
spec = importlib.util.spec_from_file_location('gb', os.path.join(P4, 'green_pbt4b.py'))
gb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gb)
base, green = gb.green(REPO, '5aaacce3')
out = os.path.join(HERE, 'green_b')
for rel, text in sorted(green.items()):
    p = os.path.join(out, *rel.split('/'))
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    print('%s  %6d  %s' % (hashlib.sha256(text.encode('utf-8')).hexdigest()[:16], len(text.encode('utf-8')), rel))
