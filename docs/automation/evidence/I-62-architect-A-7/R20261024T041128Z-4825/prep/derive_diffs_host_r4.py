"""Deriva prep/make_diffs_r4.py y prep/host_checks_r4.py de los de la ronda 3 (decisiones §98, punto 6: reutilizar componentes; textos, base de
los diffs = base3, clones y run de las rondas 4 y 3). Uso: python -I -B derive_diffs_host_r4.py <prep de la ronda 3> <prep de la ronda 4>
"""
import os
import sys

P3, P4 = sys.argv[1:3]
BS = chr(92)


def derive(src_name, dst_name, R):
    s = open(os.path.join(P3, src_name), encoding="utf-8").read()
    for a, b in R:
        assert s.count(a) == 1, (src_name, a[:90], s.count(a))
        s = s.replace(a, b)
    open(os.path.join(P4, dst_name), "w", encoding="utf-8", newline="\n").write(s)
    print(dst_name, "ok")


derive("make_diffs_r3.py", "make_diffs_r4.py", [
    ('"""Kit de la ronda 3 (decisiones §96, punto 6: reutilizar los componentes congelados compatibles y validar por incrementos según §88): escribe en\n'
     'prep/diffs/ el diff unificado de cada componente de kit/ frente al del kit de la ronda 2 tal como se lanzó y se custodió (kit-review2/kit/, igual\n'
     'blob a blob a la custodia de 3f912c84),',
     '"""Kit de la ronda 4 (decisiones §98, punto 6: reutilizar componentes congelados idénticos y sus evidencias; validación incremental): escribe en\n'
     'prep/diffs/ el diff unificado de cada componente de kit/ frente al del kit de la ronda 3 con la autorización PENDIENTE (kit-review4/base3/, igual\n'
     'blob a blob a la custodia de 000fd6eb),'),
    ("Uso: python -I -B make_diffs_r3.py <kit-review3> <kit de la ronda 2>", "Uso: python -I -B make_diffs_r4.py <kit-review4> <kit de la ronda 3 (base3)>"),
    ('"r2/" + name, "r3/" + name', '"r3/" + name, "r4/" + name')])
r = 'r"D:' + BS + 'r62-arch-a7r'
derive("host_checks_r3.py", "host_checks_r4.py", [
    ('"""Comprobaciones del host para el kit de la ronda 3, solo lectura (variante de host_checks_r2.py del kit de la ronda 2)',
     '"""Comprobaciones del host para el kit de la ronda 4, solo lectura (variante de host_checks_r3.py del kit de la ronda 3)'),
    ("clon de\nla ronda 3 y de la ronda 2 (HEAD y estado con ignorados), el run de la ronda 2",
     "clon de\nla ronda 4 y de la ronda 3 (HEAD y estado con ignorados), el run de la ronda 3"),
    ("Uso: python -I -B host_checks_r3.py", "Uso: python -I -B host_checks_r4.py"),
    ('R3, R2, R2RUN = %s3", %s2", %s2-run"' % (r, r, r), 'R3, R2, R2RUN = %s4", %s3", %s3-run"' % (r, r, r)),
    ('"Schema": "a7r3-host-checks/1"', '"Schema": "a7r4-host-checks/1"'),
    ('"R3Clone": {', '"R4Clone": {'), ('"R2Clone": {', '"R3Clone": {'), ('"R2Run": {', '"R3Run": {')])
