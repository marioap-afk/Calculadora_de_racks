"""PB-T4d (paquete v5.2): la medida de NECESIDAD de las bajas de tests/ al retirar el piloto (variante B), en una copia exportada de 6c2820ba.

El bucle de measure_retire_v52.py empieza con las 222 filas de la lista de la v4.4 y la Theory de SUS-01 ya quitadas, asi que nunca mide si cada
una hace falta. Este empieza con NINGUNA baja: retire_pbt4d_v52.py escribe la retirada B en src/ y las bajas de la vuelta (desde el texto de
6c2820ba), se compilan los dos proyectos de pruebas y cada error de tests/ lleva su miembro a las bajas (el mismo mapeo de measure_retire_v52.py,
que se importa sin tocarlo). Para cuando compila, o cuando un error no se puede llevar a un miembro o cae en src/ (PARA).

Al final escribe <dir-salida>/necesidad.tsv: cada fila de las 235 bajas de la variante B (r6e-B/bucle/bajas-4.tsv), con `necesaria` (la quita
la compilacion) o `no-necesaria` (compila sin quitarla), y las bajas por compilacion que no esten entre esas 235 (`fuera-de-B`).
Solo escribe en la copia y en <dir-salida>, en UTF-8 con LF.

Uso: python -I measure_need_v52.py <copia> <dir-salida> <bajas-B.tsv> --repo <checkout>
"""
import collections
import importlib.util
import os
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
COPY, OUTDIR, BAJAS_B = sys.argv[1:4]
REPO = sys.argv[sys.argv.index('--repo') + 1]
os.makedirs(OUTDIR, exist_ok=True)

# measure_retire_v52.py lee sys.argv al cargarse: se le da un argv propio y se para antes de su bucle con --solo-bajas no; se importan sus
# funciones copiando el modulo con un argv de medida y sin ejecutar su parte final.
_src = open(os.path.join(HERE, 'measure_retire_v52.py'), encoding='utf-8').read()
_cut = _src.index("if '--solo-bajas' in sys.argv:")
_ns = {'__name__': 'mr', '__file__': os.path.join(HERE, 'measure_retire_v52.py')}
_saved = sys.argv
sys.argv = ['measure_retire_v52.py', COPY, 'B', OUTDIR, '--repo', REPO]
exec(compile(_src[:_cut], 'measure_retire_v52.py', 'exec'), _ns)
sys.argv = _saved
_member_at, build, ERR = _ns['member_at'], _ns['build'], _ns['ERR']


def member_at(rel, line):
    """El de measure_retire_v52.py; y una linea de atributo ([InlineData(...)], [Theory(...)]) encima de un metodo lleva a ese metodo (en /d/r6g
    no se llevaba: el error de una fila de InlineData quedaba sin miembro)."""
    name = _member_at(rel, line)
    if name is not None:
        return name
    lines = open(os.path.join(COPY, rel), encoding='utf-8-sig').read().replace('\r\n', '\n').split('\n')
    k = line - 1
    if not lines[k].strip().startswith('['):
        return None
    while k < len(lines) and (lines[k].strip().startswith('[') or lines[k].strip().startswith('//') or not lines[k].strip()):
        k += 1
    return _member_at(rel, k + 1) if k < len(lines) else None


bajas = collections.OrderedDict()


def write_bajas(k):
    path = os.path.join(OUTDIR, 'bajas-%d.tsv' % k)
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('origen\truta\tmiembro\n')
        for (p, m), why in bajas.items():
            f.write('%s\t%s\t%s\n' % (why, p, m))
    return path


summary = []
stop, seen = [], {}
for k in range(20):
    bpath = write_bajas(k)
    r = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'retire_pbt4d_v52.py'), COPY, 'B', bpath, os.path.join(OUTDIR, 'retire-%d.txt' % k),
                        '--repo', REPO], capture_output=True)
    if r.returncode != 0:
        print('PARA: retire_pbt4d_v52.py fallo en la vuelta %d' % k)
        print(r.stdout.decode('utf-8', 'replace'), r.stderr.decode('utf-8', 'replace'))
        sys.exit(5)
    errors = []
    for proj, label in (('tests/RackCad.Tests/RackCad.Tests.csproj', 'v%d-core' % k), ('tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', 'v%d-ui' % k)):
        rc, text = build(proj, label)
        errors += [m.groupdict() for m in ERR.finditer(text)]
    seen = {}
    for e in errors:
        rel = os.path.relpath(e['path'], COPY).replace(os.sep, '/')
        seen[(rel, int(e['line']), e['code'])] = e['msg']
    new = collections.OrderedDict()
    stop = []
    for (rel, line, code), msg in sorted(seen.items()):
        if not rel.startswith('tests/'):
            stop.append('src %s:%d %s %s' % (rel, line, code, msg))
            continue
        name = member_at(rel, line)
        if name is None:
            stop.append('sin miembro %s:%d %s %s' % (rel, line, code, msg))
            continue
        if (rel, name) not in bajas and (rel, name) not in new:
            new[(rel, name)] = 'compilacion v%d %s: %s' % (k, code, msg[:160])
    summary.append('vuelta %d: bajas %d, errores %d (distintos %d), miembros nuevos %d, sin resolver %d' % (k, len(bajas), len(errors), len(seen), len(new),
                                                                                                         len(stop)))
    print(summary[-1])
    if stop:
        for s in stop:
            print('  PARA', s)
        break
    if not seen:
        break
    bajas.update(new)

rows_b = [l.split('\t') for l in open(BAJAS_B, encoding='utf-8').read().rstrip('\n').split('\n')[1:] if l.strip()]
keys_b = [(r[1], r[2]) for r in rows_b]
with open(os.path.join(OUTDIR, 'necesidad.tsv'), 'w', encoding='utf-8', newline='\n') as f:
    f.write('necesidad\torigen-en-B\truta\tmiembro\tpor-que\n')
    for (origin, path, member) in rows_b:
        why = bajas.get((path, member))
        f.write('%s\t%s\t%s\t%s\t%s\n' % ('necesaria' if why else 'no-necesaria', origin, path, member, why or '-'))
    for (p, m), why in bajas.items():
        if (p, m) not in keys_b:
            f.write('fuera-de-B\t-\t%s\t%s\t%s\n' % (p, m, why))
with open(os.path.join(OUTDIR, 'resumen.txt'), 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(summary) + '\n')
c = collections.Counter('necesaria' if (r[1], r[2]) in bajas else 'no-necesaria' for r in rows_b)
out_b = sum(1 for key in bajas if key not in keys_b)
print('bajas por compilacion %d; de las %d de B: necesarias %d, no necesarias %d; fuera de B %d' % (len(bajas), len(rows_b), c['necesaria'],
                                                                                                   c['no-necesaria'], out_b))
sys.exit(0 if not stop and not seen else 6)
