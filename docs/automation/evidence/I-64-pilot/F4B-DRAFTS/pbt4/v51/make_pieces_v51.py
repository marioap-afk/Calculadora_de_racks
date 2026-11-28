"""PB-T4c (paquete v5.1): las piezas literales de la v5.1 desde las de la v5.0, con los cambios de CA-T4c y nada mas (cada uno, una sustitucion
exacta que aparece exactamente una vez):
- I-01: sin `#nullable enable` en los dos archivos nuevos de Draft/ (PushBackFullDraftLaunch.cs y PushBackFullDraftStatePort.cs), en el GREEN y
  en el esqueleto, porque la pureza de CT-PBT4C-15 (la de CT-PBT3A-14) la prohibe; y sin el `?` de `RackProject? ProjectOf`, que sin la directiva
  daria CS8632 (Nullable en disable; la leccion de CA-T4a I-01);
- M-03: el comentario del GREEN en PendingWork() de la ventana, que ahora casa con D-S10;
- y la cabecera de cada pieza, que nombra la v5.1 y su generador.
Lee pbt4c-green-v50.txt y pbt4c-skeleton-v50.txt (por su sha256) y escribe pbt4c-green-v51.txt y pbt4c-skeleton-v51.txt, con \n. Imprime el
sha256 de cada una (UTF-8, LF).

Uso: python -I make_pieces_v51.py
"""
import hashlib
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
PB4 = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def read(name, sha):
    data = open(os.path.join(PB4, name), 'rb').read()
    assert hashlib.sha256(data).hexdigest() == sha, name
    assert b'\r' not in data
    return data.decode('utf-8')


def once(text, old, new, what):
    assert text.count(old) == 1, (what, text.count(old))
    return text.replace(old, new, 1)


LAUNCH = '@@@@ NEWFILE src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs\n'
PORT = '@@@@ NEWFILE src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs\n'
NULLABLE = '#nullable enable\n'

g = read('pbt4c-green-v50.txt', '6b304a009d05ff586603ed4bbb9fa69b9973977bea09222c688ef80faa983e1b')
g = once(g, '@@@@ PB-T4c (paquete v5.0): el texto literal del GREEN de src/.', '@@@@ PB-T4c (paquete v5.1): el texto literal del GREEN de src/.', 'cabecera 1')
g = once(g, 'green_pbt4c_v50.py lo aplica y lo comprueba.', 'green_pbt4c_v51.py lo aplica y lo comprueba.', 'cabecera 2')
g = once(g, LAUNCH + NULLABLE, LAUNCH, 'I-01 GREEN launch')
g = once(g, PORT + NULLABLE, PORT, 'I-01 GREEN port')
g = once(g, '        private static RackProject? ProjectOf(string origin)\n', '        private static RackProject ProjectOf(string origin)\n', 'I-01 ?')
g = once(g,
         '            // I-64 F4B PB-T4c (D-PBT4-17; A-T2b-OWNER-01): in WORKSPACE, a staged module AND unapplied changes ask one question that names\n'
         '            // both; each of them alone asks the question of today. In CLASSIC there are never unapplied changes, so nothing changes there.\n',
         '            // I-64 F4B PB-T4c (D-PBT4-17; A-T2b-OWNER-01): a staged module AND unapplied changes ask one question that names both. In\n'
         '            // WORKSPACE a staged module always leaves unapplied changes (D-S10), so the module question alone is the one of CLASSIC, where\n'
         '            // there are never unapplied changes; unapplied changes alone ask the question of today.\n',
         'M-03')
s = read('pbt4c-skeleton-v50.txt', '48e097a7a579bd311fdeb2f6b87ff57ec0ada157573b174ad3a02935e5ab816c')
s = once(s, '@@@@ PB-T4c (paquete v5.0): el esqueleto exacto de src/ del RED. FROMGREEN toma enteros los bloques del GREEN de esa ruta (pbt4c-green-v50.txt);',
         '@@@@ PB-T4c (paquete v5.1): el esqueleto exacto de src/ del RED. FROMGREEN toma enteros los bloques del GREEN de esa ruta (pbt4c-green-v51.txt);',
         'cabecera')
s = once(s, LAUNCH + NULLABLE, LAUNCH, 'I-01 esqueleto launch')
s = once(s, PORT + NULLABLE, PORT, 'I-01 esqueleto port')
for text in (g, s):
    for head in (LAUNCH, PORT):
        block = text.split(head)[1].split('@@@@ END')[0]
        assert '#nullable' not in block, head
        assert not re.search(r'\w\?[\s\[]', block), ('una anotacion de nulabilidad sin #nullable', head)  # T? x o T?[]; no el ternario ni ??
for name, text in (('pbt4c-green-v51.txt', g), ('pbt4c-skeleton-v51.txt', s)):
    data = text.encode('utf-8')
    open(os.path.join(PB4, name), 'wb').write(data)
    print(hashlib.sha256(data).hexdigest(), name)
