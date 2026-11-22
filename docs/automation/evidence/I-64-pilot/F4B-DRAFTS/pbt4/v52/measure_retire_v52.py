# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
"""PB-T4d (paquete v5.2, fase 2): el bucle de la medida de lo que deja de compilar al retirar el piloto, en una copia exportada de 6c2820ba.

1. Las bajas iniciales: las 222 filas de pbt4d-retire-list-138.tsv (138 C-7, 55 originales y 29 de apoyo, con su ruta resuelta) y la Theory de
   SUS-01 (WorkspaceDraftPilotWindowLoadCharacterizationTests.TheWindowAndThePilotDifferOnlyInTheKnownClasses).
2. En cada vuelta: retire_pbt4d_v52.py escribe la retirada (variante A o B) y las bajas desde el texto de 6c2820ba; se compilan los dos proyectos de
   pruebas (Debug, con el SDK de usuario y --no-incremental -nodeReuse:false -p:UseSharedCompilation=false); cada error de un archivo de tests/
   lleva su miembro (el metodo, la propiedad o el campo que contiene la linea; si no, el tipo anidado) a las bajas, con su codigo y su texto.
3. Para cuando ya no hay errores (o cuando un error no se puede llevar a un miembro, o cae en src/: entonces PARA).
Solo escribe en la copia y en <dir-salida> (las bajas de cada vuelta, los registros de compilacion y el resumen), en UTF-8 con LF.

Uso: python -I measure_retire_v52.py <copia> <A|B> <dir-salida> --repo <checkout>
"""
import collections
import importlib.util
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.dirname(HERE)
spec = importlib.util.spec_from_file_location('lists_core', os.path.join(PB4, 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
COPY, VARIANT, OUTDIR = sys.argv[1:4]
REPO = sys.argv[sys.argv.index('--repo') + 1]
DN = os.path.join(os.environ['<datos-locales-del-usuario>'], 'Microsoft', 'dotnet', 'dotnet.exe')
os.makedirs(OUTDIR, exist_ok=True)
LIST = os.path.join(PB4, 'pbt4d-retire-list-138.tsv')
SUS01 = ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'TheWindowAndThePilotDifferOnlyInTheKnownClasses')
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?() \t]+?\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)
ERR = re.compile(r'^(?P<path>[A-Za-z]:\\[^(]+)\((?P<line>\d+),(?P<col>\d+)\): error (?P<code>CS\d+): (?P<msg>.*?)(?: \[[^\]]+\])?$', re.M)


def tree_paths():
    out = subprocess.run(['git', '-C', REPO, 'ls-tree', '-r', '--name-only', '6c2820ba', 'tests'], capture_output=True, check=True).stdout.decode()
    return [p for p in out.splitlines() if p.endswith('.cs')]


PATHS = tree_paths()


def resolve(path):
    if '/' in path:
        return path
    hits = [p for p in PATHS if p.endswith('/' + path)]
    assert len(hits) == 1, (path, hits)
    return hits[0]


rows = [l.split('\t') for l in open(LIST, encoding='utf-8').read().rstrip('\n').split('\n')[1:]]
bajas = collections.OrderedDict()
for kind, path, member, lines, action in rows:
    bajas[(resolve(path), member)] = 'lista ' + kind
bajas[SUS01] = 'SUS-01 (PB-TC)'


def write_bajas(k):
    path = os.path.join(OUTDIR, 'bajas-%d.tsv' % k)
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('origen\truta\tmiembro\n')
        for (p, m), why in bajas.items():
            f.write('%s\t%s\t%s\n' % (why, p, m))
    return path


def build(project, label):
    log = os.path.join(OUTDIR, label + '.log')
    p = subprocess.run([DN, 'build', project, '-c', 'Debug', '--no-incremental', '-nodeReuse:false', '-p:UseSharedCompilation=false', '-v:minimal'],
                       cwd=COPY, capture_output=True)
    text = p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n') + p.stderr.decode('utf-8', 'replace').replace('\r\n', '\n')
    with open(log, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    return p.returncode, text


DECL = re.compile(r'^[ \t]*(?:(?:public|private|internal|protected|static|async|override|sealed|readonly|virtual|unsafe|extern)\s+)+'
                  r'[\w<>\[\],.?()\s]*?\b(\w+)\s*(?:<[^>()]*>)?\s*\(', re.M)


def decl_end(masked, i, start):
    """El final de una declaracion con parametros (la de retire_pbt4d_v52.py): su llave de cierre o el ; de su cuerpo de expresion."""
    depth = masked[start:i].count('(') - masked[start:i].count(')')
    j = i
    while j < len(masked):
        ch = masked[j]
        if ch == '(':
            depth += 1
        elif ch == ')':
            depth -= 1
        elif depth == 0 and ch == '{':
            d = 0
            for k in range(j, len(masked)):
                if masked[k] == '{':
                    d += 1
                elif masked[k] == '}':
                    d -= 1
                    if d == 0:
                        return k
        elif depth == 0 and masked.startswith('=>', j):
            d = 0
            for k in range(j, len(masked)):
                if masked[k] in '({[':
                    d += 1
                elif masked[k] in ')}]':
                    d -= 1
                elif masked[k] == ';' and d <= 0:
                    return k
        elif depth == 0 and ch == ';':
            return j
        j += 1
    return len(masked) - 1


def member_at(rel, line):
    text = open(os.path.join(COPY, rel), encoding='utf-8-sig').read().replace('\r\n', '\n')
    masked = lc.mask(text)
    best = None
    for dm in DECL.finditer(masked):
        a = masked.count('\n', 0, dm.start()) + 1
        b = masked.count('\n', 0, decl_end(masked, dm.end(), dm.start())) + 1
        if a <= line <= b and (best is None or b - a < best[1] - best[0]):
            best = (a, b, dm.group(1))
    if best:
        # un constructor deja su tipo a medias (sus propiedades sin valor: CS8618): se quita el tipo entero
        for tm in re.finditer(r'\b(?:class|struct|record)\s+' + re.escape(best[2]) + r'\b', masked):
            return 'tipo:' + best[2]
        return best[2]
    for fm in FIELD.finditer(masked):
        a = masked.count('\n', 0, fm.start()) + 1
        end = masked.find(';', fm.end() - 1)
        b = masked.count('\n', 0, end) + 1
        if a <= line <= b:
            return fm.group(1)
    # el tipo de apoyo que contiene la linea: el mas pequeno, y nunca una clase de pruebas (una que tiene [Fact] o [Theory] dentro)
    best_t = None
    for tm in re.finditer(r'^([ \t]*)(?:(?:public|private|internal|protected|static|sealed|abstract|file)\s+)*(?:class|struct|record)\s+(\w+)', masked, re.M):
        a = masked.count('\n', 0, tm.start()) + 1
        i = masked.index('{', tm.end())
        d = 0
        for k in range(i, len(masked)):
            if masked[k] == '{':
                d += 1
            elif masked[k] == '}':
                d -= 1
                if d == 0:
                    break
        b = masked.count('\n', 0, k) + 1
        if a <= line <= b and (best_t is None or b - a < best_t[1] - best_t[0]):
            best_t = (a, b, tm.group(2), text[tm.start():k])
    if best_t and '[Fact' not in best_t[3] and '[Theory' not in best_t[3]:
        return 'tipo:' + best_t[2]
    return None


if '--solo-bajas' in sys.argv:  # escribe solo las bajas iniciales (para la comprobacion en seco de retire_pbt4d_v52.py)
    print(write_bajas(0))
    sys.exit(0)
summary = []
for k in range(12):
    bpath = write_bajas(k)
    r = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'retire_pbt4d_v52.py'), COPY, VARIANT, bpath, os.path.join(OUTDIR, 'retire-%d.txt' % k),
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
with open(os.path.join(OUTDIR, 'resumen.txt'), 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(summary) + '\n')
    for (p, m), why in bajas.items():
        if not why.startswith('lista'):
            f.write('%s\t%s\t%s\n' % (why, p, m))
print('bajas finales %d (de la lista %d, SUS-01 1, por compilacion %d)' % (len(bajas), sum(1 for v in bajas.values() if v.startswith('lista')),
                                                                         sum(1 for v in bajas.values() if v.startswith('compilacion'))))
sys.exit(0 if not stop and not seen else 6)
