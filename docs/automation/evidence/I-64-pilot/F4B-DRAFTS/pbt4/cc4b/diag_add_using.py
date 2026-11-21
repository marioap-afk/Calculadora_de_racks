"""DIAGNOSTICO (no es la precondición 6 ni corrige el paquete): sobre una copia exportada con el RED de la v4.5 en modo de medida, añade
`using RackCad.Application.Systems.PushBack.Draft;` tras el último `using` de cabecera de cada copia (*Full.cs) que nombra un tipo de Draft/ y no
lleva esa directiva, para ver si es la única causa de los errores de compilación de la fase probe. Escribe solo en la copia que se le da.

Uso: python -I diag_add_using.py <dir-copia>"""
import os
import re
import sys

root = sys.argv[1]
USING = 'using RackCad.Application.Systems.PushBack.Draft;'
TYPES = re.compile(r'\bPushBackFull(?:DraftState|FieldAdapter|DraftStateComparer)\b')
for d, _, fs in os.walk(os.path.join(root, 'tests')):
    for f in sorted(fs):
        if not f.endswith('Full.cs'):
            continue
        p = os.path.join(d, f)
        text = open(p, 'rb').read().decode('utf-8')
        if USING in text or not TYPES.search(text):
            continue
        lines = text.split('\n')
        last = max(i for i, l in enumerate(lines[:60]) if l.startswith('using '))
        lines.insert(last + 1, USING)
        open(p, 'wb').write('\n'.join(lines).encode('utf-8'))
        print('using anadido:', os.path.relpath(p, root).replace('\\', '/'))
