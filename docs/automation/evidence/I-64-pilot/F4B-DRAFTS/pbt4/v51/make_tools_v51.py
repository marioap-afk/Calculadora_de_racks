"""PB-T4c (paquete v5.1): los tres guiones de las piezas de la v5.1, derivados de los de la v5.0 (por su sha256) con solo los nombres cambiados:
green_pbt4c_v51.py lee pbt4c-green-v51.txt y pbt4c-skeleton-v51.txt; apply_red_pbt4c_v51.py y apply_green_pbt4c_v51.py cargan green_pbt4c_v51.
Los Skip (pbt4c_skips_v50.py), los equivalentes (make_equivalents_pbt4c_v50.py) y su comprobacion (check_skips_pbt4c_v50.py) no cambian y se
siguen cargando por su nombre de la v5.0. Cada sustitucion se cuenta. Escribe con \n e imprime el sha256 de cada uno.

Uso: python -I make_tools_v51.py
"""
import hashlib
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
PB4 = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SOURCES = {
    'green_pbt4c_v50.py': ('d459501c7657706be6133123ff82608f8035d135827b5b92b1839ace0cb56c6a', 'green_pbt4c_v51.py', [
        ('PB-T4c (paquete v5.0)', 'PB-T4c (paquete v5.1)', 1),
        ('pbt4c-green-v50.txt', 'pbt4c-green-v51.txt', 3),
        ('pbt4c-skeleton-v50.txt', 'pbt4c-skeleton-v51.txt', 3),
        ('green_pbt4c_v50.py', 'green_pbt4c_v51.py', 1),
        ('# PB-T4c v5.0:', '# PB-T4c v5.1:', 1),
    ]),
    'apply_red_pbt4c_v50.py': ('3fee9212e5ad57cdd3e437dc46f62682fc1d3d7f0c921c236e1bc6e2a30f4003', 'apply_red_pbt4c_v51.py', [
        ('PB-T4c (paquete v5.0)', 'PB-T4c (paquete v5.1)', 1),
        ('green_pbt4c_v50', 'green_pbt4c_v51', 2),
        ('apply_red_pbt4c_v50.py', 'apply_red_pbt4c_v51.py', 1),
    ]),
    'apply_green_pbt4c_v50.py': ('eb4b4d6d94f93767469c7bc6debcabf39e0f63cd7a3d67877a559b06a31fa3ff', 'apply_green_pbt4c_v51.py', [
        ('PB-T4c (paquete v5.0)', 'PB-T4c (paquete v5.1)', 1),
        ('apply_green_pbt4c_v50.py', 'apply_green_pbt4c_v51.py', 1),  # antes que green_pbt4c_v50, que es parte de este nombre
        ('green_pbt4c_v50', 'green_pbt4c_v51', 2),
        ('apply_red_pbt4c_v50.py', 'apply_red_pbt4c_v51.py', 1),
    ]),
    # Las CT como datos: las mismas CT y las mismas lineas; solo su docstring dice, por M-01 de CA-T4c, de donde sale cada linea.
    'pbt4c_ct_table_v50.py': ('39728a5419264d6ce4f444fedc81e2982944dde45c391d830704c39715500e36', 'pbt4c_ct_table_v51.py', [
        ('"""PB-T4c (paquete v5.0): la lista cerrada CT-PBT4C-01 a 15, como datos: que fija cada CT, sus lineas esperadas literales (el Assert.Equal\n'
         'de las lineas unidas por \\n), su clase, su proyecto y su ExpectRed. make_v50.py la escribe en §5.3 (una fila por CT, con las lineas entre\n'
         'comillas invertidas, como lee la cadena: rr3_table_check) y en §6.0 (la tabla de RR-3). Las lineas de CT-03, 04, 05 y 12 son las que dio la sonda\n'
         'de la precondicion 6 sobre el GREEN literal (pbt4ccc/compile, `pbt4c-probe-editall.cs`), y las de CT-09, las de la sonda de UI\n'
         '(`pbt4c-probe-close-ui.cs`; D-S10); las demas, del texto literal del GREEN.\n"""',
         '"""PB-T4c (paquete v5.1): la lista cerrada CT-PBT4C-01 a 15, como datos: que fija cada CT, sus lineas esperadas literales (el Assert.Equal\n'
         'de las lineas unidas por \\n), su clase, su proyecto y su ExpectRed; las mismas CT y las mismas lineas que pbt4c_ct_table_v50.py. make_v51.py\n'
         'la escribe en §5.3 (una fila por CT, con las lineas entre comillas invertidas, como lee la cadena: rr3_table_check) y en §6.0 (la tabla de RR-3).\n'
         'De donde sale cada linea (M-01 de CA-T4c), linea a linea en §5.3 de la v5.1: las de CT-03, 04, 05 y 12, de la sonda de Core sobre el GREEN\n'
         'literal (`pbt4c-probe-editall.cs`): unas tal cual, otras con los campos de la CT, tres derivadas y cuatro, del texto del GREEN; las de CT-01,\n'
         '07, 08, 13 y 15, del ensayo de las CT de texto (`pbt4c-rehearsal-textct.cs`, el paso 8), sobre el GREEN y sobre el RED; las de CT-09, de la\n'
         'sonda de UI (`pbt4c-probe-close-ui.cs`; D-S10); las de CT-02, 06, 10, 11 y 14, del texto literal del GREEN, sin ejecutar.\n"""', 1),
    ]),
}
for src, (want, dst, subs) in SOURCES.items():
    data = open(os.path.join(PB4, src), 'rb').read()
    assert hashlib.sha256(data).hexdigest() == want, src
    text = data.decode('utf-8')
    for old, new, n in subs:
        assert text.count(old) == n, (src, old, text.count(old), n)
        text = text.replace(old, new)
    assert 'v50' not in text.replace('pbt4c_skips_v50', '').replace('make_equivalents_pbt4c_v50', '').replace('check_skips_pbt4c_v50', '').replace(
        'pbt4c_ct_table_v50.py', ''), (dst, [
        l for l in text.split('\n') if 'v50' in l])
    out = text.encode('utf-8')
    open(os.path.join(PB4, dst), 'wb').write(out)
    print(hashlib.sha256(out).hexdigest(), dst)
