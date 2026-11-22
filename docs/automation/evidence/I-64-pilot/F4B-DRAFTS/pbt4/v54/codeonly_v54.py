"""PB-T4d (paquete v5.4): compara dos arboles del RED (los textos de make_red) sin comentarios // ni lineas en blanco, archivo a archivo, y dice
si el codigo es el mismo. Solo lee. Uso: python -I codeonly_v54.py <arbol-a> <arbol-b>
"""
import os
import sys

A, B = sys.argv[1:3]


def files(root):
    out = {}
    for d, _, names in os.walk(os.path.join(root, 'tests')):
        if os.sep + 'bin' in d or os.sep + 'obj' in d or '/bin' in d or '/obj' in d:
            continue
        for n in [x for x in names if x.endswith('.cs')]:
            p = os.path.join(d, n)
            out[os.path.relpath(p, root).replace('\\', '/')] = p
    return out


def code(path):
    return [l.rstrip() for l in open(path, encoding='utf-8').read().split('\n') if l.strip() and not l.strip().startswith('//')]


fa, fb = files(A), files(B)
diff = sorted(set(fa) ^ set(fb))
changed_text = [p for p in sorted(set(fa) & set(fb)) if open(fa[p], encoding='utf-8').read() != open(fb[p], encoding='utf-8').read()]
changed_code = [p for p in changed_text if code(fa[p]) != code(fb[p])]
print('archivos solo en uno: %s' % diff)
print('cambia el texto: %s' % changed_text)
print('cambia el codigo (sin comentarios ni lineas en blanco): %s' % changed_code)
