"""PB-T4d (paquete v5.4, solo de texto): de los fragmentos de la v5.3 (v53/frag/, ya con los cambios de la v5.3) a los de la v5.4 (v54/frag/), con
los menores de la recomprobacion 1 de CA-T4d (M-08, M-09, M-11 y M-13; M-10 y M-12 van en make_v54.py y en las piezas). Por anclaje exacto y
contado. Escribe UTF-8 con LF. Uso: python -I edit_frags_v54.py
"""
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
V53 = os.path.join(os.path.dirname(HERE), 'v53', 'frag')
EDITS = {n: [] for n in ('d10.md', 's44.md', 's56.md', 's64.md')}


def E(name, old, new, times=1):
    EDITS[name].append((old, new, times))


for old, new in (('pbt4d-red-v53.txt', 'pbt4d-red-v54.txt'), ('pbt4d-green-v53.txt', 'pbt4d-green-v54.txt'),
                 ('pbt4d_test_edits_v53.py', 'pbt4d_test_edits_v54.py'), ('v53/make_red_v53.py', 'v54/make_red_v54.py'),
                 ('v53/make_list_v53.py', 'v54/make_list_v54.py'), ('cov_p6_v53.py', 'cov_p6_v54.py'), ('anexo V53-A', 'anexo V54-A')):
    for n in EDITS:
        E(n, old, new, None)
E('s64.md', '(D-S11 a D-S15 y lo que la sesion fijo tras CA-T4d)', '(D-S11 a D-S16)')  # M-09
E('s64.md', 'paquete del contrato de PB-T4 v5.3', 'paquete del contrato de PB-T4 v5.4')
E('s64.md', 'esta versión del paquete (v5.3)', 'esta versión del paquete (v5.4)')
E('s64.md', '(V53-06; la v5.2, historia)', '(V53-06 y V54-01; la v5.2, historia)')
E('s64.md', '### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT` (entero; V52-01 a V52-08; V53-01 a V53-11)',
  '### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT` (entero; V52-01 a V52-08; V53-01 a V53-11; V54-01 a V54-07)')
E('s44.md', 'Las 436 líneas de la v5.2 y sus 9 ramas se quedan como conjunto vigilado.',
  'Las 436 líneas de la v5.2 y sus 9 ramas se quedan como conjunto vigilado; **desde la v5.3 se cuentan 537** (433 se quedan), porque ahora entran '
  'también las líneas de WPUR que caen dentro de los tramos que el GREEN borra enteros, que la v5.2 quitaba antes de contar (recomprobación 1 de CA-T4d, M-13).')
E('s44.md', '### 4.4 PB-T4d (V52-01 a V52-04; V53-01 a V53-04)', '### 4.4 PB-T4d (V52-01 a V52-04; V53-01 a V53-04; V54-01, V54-04 y V54-06)')
E('s44.md', '**Bajan** (informativo; D-S16: lo que cubren más corridas en la base que en el GREEN, por componente emparejado; no cambia el PASA): @@BAJAN@@.',
  '**Bajan** (informativo; D-S16: lo que cubren más corridas en la base que en el GREEN, por componente emparejado; no cambia el PASA; cada entrada '
  'nombra la corrida que la cubría, no la prueba, porque coverlet no da la prueba; y ESTABLE solo por la UI exige las tres corridas juntas y al menos un '
  'filtro en su proceso: recomprobación 1 de CA-T4d, M-11): @@BAJAN@@.')


def apply(name, text):
    for old, new, times in EDITS[name]:
        n = text.count(old)
        assert times is None or n == times, (name, old[:100], n, times)
        text = text.replace(old, new)
    if name == 's64.md':  # M-08: las claves del JSON, las de verdad (las da make_v54.py, de pbt4d-precondition6.json)
        rx = re.compile(r'Schema `rackcad-pbt4d-precondition6/v1`: [^|]*?\. sha256 `@@P6_SHA@@`')
        assert len(rx.findall(text)) == 1
        text = rx.sub('Schema `rackcad-pbt4d-precondition6/v1`; @@P6_KEYS@@. sha256 `@@P6_SHA@@`', text)
    return text


os.makedirs(os.path.join(HERE, 'frag'), exist_ok=True)
for name in EDITS:
    text = apply(name, open(os.path.join(V53, name), encoding='utf-8').read())
    with open(os.path.join(HERE, 'frag', name), 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    print(name, len(EDITS[name]), 'cambios')
