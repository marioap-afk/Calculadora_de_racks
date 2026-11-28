"""PB-T4d (paquete v5.2, fase 2): escribe en una copia exportada de 6c2820ba la retirada del codigo del piloto (un borrador de medida, no el GREEN
de PB-T4d) y quita de tests/ los miembros de un archivo de bajas. Siempre parte del texto de 6c2820ba por objetos git, asi que se puede repetir sobre
la misma copia con un archivo de bajas mayor: es el bucle de la medida (compilar, leer los errores, anadir sus miembros, otra vez).

- src/, la variante A: lo de D-PBT4-10 (el comparador y el adaptador del campo, enteros; ComposeForWrite y KeepPilotEditsOn; Prepare, Precheck y
  Execute del piloto) y lo que hace falta para que src/ compile sin ellos (el Build privado del planificador, que llama a ComposeForWrite, y
  PushBackPilotPrepareInput.From, que copia con el comparador);
- src/, la variante B: la A y lo que queda sin uso en src/ (los tipos PushBackPilotPrepareInput y PushBackPilotComposition y el ComparePost del
  piloto);
- tests/: cada fila del archivo de bajas (ruta, miembro) quita ese miembro (sus comentarios /// y sus atributos incluidos; todas sus sobrecargas),
  o, con el miembro `tipo:<Nombre>`, el tipo anidado entero.
Cada borrado se comprueba (el anclaje esta exactamente las veces que se espera). Escribe con los saltos de linea y el BOM de cada archivo; imprime
el sha256 de cada archivo escrito. Nunca escribe fuera de la copia.

Uso: python -I retire_pbt4d_v52.py <copia> <A|B> <bajas.tsv> <salida.txt> --repo <checkout>
"""
import collections
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('lists_core', os.path.join(os.path.dirname(HERE), 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
COPY, VARIANT, BAJAS, OUT = sys.argv[1:5]
REPO = sys.argv[sys.argv.index('--repo') + 1]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
assert VARIANT in ('A', 'B')
DRY = '--dry' in sys.argv  # en seco: calcula los borrados y los comprueba, sin copia y sin escribir nada salvo <salida.txt>
assert DRY or os.path.isfile(os.path.join(COPY, 'RackCad.sln')), 'no es una copia del repositorio'
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'


def show(path):
    raw = subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout
    bom = raw.startswith(b'\xef\xbb\xbf')
    text = raw.decode('utf-8-sig')
    crlf = '\r\n' in text
    return text.replace('\r\n', '\n'), bom, crlf


def lines_of_span(text, start, end):
    """De la linea del caracter start a la del caracter end (inclusive), extendida hacia arriba por los /// y los atributos."""
    lines = text.split('\n')
    a = text.count('\n', 0, start)
    b = text.count('\n', 0, end)
    while a > 0 and (lines[a - 1].strip().startswith('///') or lines[a - 1].strip().startswith('[')):
        a -= 1
    return a, b


def body_end(masked, i, start=None):
    """Desde i (tras el nombre), el final del miembro: la llave que cierra la primera que abre, o el ; de un cuerpo de expresion. Con start, la
    profundidad de parentesis de partida es la del tramo start..i (un ancla que acaba en el '(' de los parametros ya esta dentro de ellos)."""
    depth = 0 if start is None else masked[start:i].count('(') - masked[start:i].count(')')
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
    raise AssertionError('sin final')


def remove_spans(text, spans):
    lines = text.split('\n')
    masked_lines = lc.mask(text).split('\n')
    for a, b in spans:
        cut = '\n'.join(masked_lines[a:b + 1])
        assert cut.count('{') == cut.count('}'), ('un borrado deja llaves sueltas', a + 1, b + 1, lines[a][:80])
    drop = set()
    for a, b in spans:
        drop.update(range(a, b + 1))
        # una linea en blanco de mas, si el borrado deja dos seguidas
        if a > 0 and b + 1 < len(lines) and not lines[a - 1].strip() and not lines[b + 1].strip():
            drop.add(b + 1)
    return '\n'.join(l for k, l in enumerate(lines) if k not in drop)


def remove_by_regex(text, rx, times=1):
    masked = lc.mask(text)
    ms = list(re.finditer(rx, masked))
    assert len(ms) == times, (rx, len(ms), times)
    spans = [lines_of_span(text, m.start(), body_end(masked, m.end(), m.start())) for m in ms]
    return remove_spans(text, spans)


SRC_A = [
    ('borrar', P + 'PushBackPilotStateComparer.cs', None),
    ('borrar', P + 'PushBackPilotFieldAdapter.cs', None),
    ('miembro', P + 'PushBackPilotMaterializer.cs', r'public\s+static\s+PushBackPilotComposition\s+ComposeForWrite\s*\('),
    ('miembro', P + 'PushBackPilotMaterializer.cs', r'public\s+static\s+PushBackPilotDraftState\s+KeepPilotEditsOn\s*\('),
    ('miembro', P + 'PushBackPilotUpdatePlan.cs', r'public\s+static\s+PushBackPilotPrepareResult\s+Prepare\s*\('),
    ('miembro', P + 'PushBackPilotUpdatePlan.cs', r'private\s+static\s+PushBackPilotPrepareResult\s+Build\s*\('),
    ('miembro', P + 'PushBackPilotUpdatePlan.cs', r'public\s+static\s+PushBackPilotPrepareInput\s+From\s*\('),
    ('miembro', P + 'PushBackPilotUpdateRun.cs', r'public\s+static\s+PushBackPilotPrecheck\s+Precheck\s*\(\s*DraftSession<PushBackPilotDraftState>'),
    ('miembro', P + 'PushBackPilotUpdateRun.cs', r'public\s+static\s+DraftApplyOutcome<PushBackPilotDraftState>\s+Execute\s*\('),
]
SRC_B = [
    ('miembro', P + 'PushBackPilotUpdatePlan.cs', r'public\s+sealed\s+class\s+PushBackPilotPrepareInput\b'),
    ('miembro', P + 'PushBackPilotMaterializer.cs', r'public\s+sealed\s+class\s+PushBackPilotComposition\b'),
    ('miembro', P + 'PushBackPilotUpdateRun.cs', r'public\s+static\s+string\?\s+ComparePost\s*\('),
]
OPS = SRC_A + (SRC_B if VARIANT == 'B' else [])

files = collections.OrderedDict()   # ruta -> [texto, bom, crlf]
deleted = []
log = ['# retirada del piloto, variante %s, sobre %s' % (VARIANT, REV)]
for op, path, rx in OPS:
    if op == 'borrar':
        deleted.append(path)
        continue
    if path not in files:
        files[path] = list(show(path))
    files[path][0] = remove_by_regex(files[path][0], rx)
    log.append('src %s %s' % (path, rx[:70]))

# tests/: las bajas
rows = [l.split('\t') for l in open(BAJAS, encoding='utf-8').read().rstrip('\n').split('\n')[1:] if l.strip()]
by_file = collections.defaultdict(list)
for r in rows:
    by_file[r[1]].append(r[2])
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?() \t]+?\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)


def statement_end(masked, i):
    """El ; que cierra la declaracion que sigue en i, fuera de parentesis, llaves y corchetes."""
    d = 0
    for k in range(i, len(masked)):
        ch = masked[k]
        if ch in '({[':
            d += 1
        elif ch in ')}]':
            d -= 1
        elif ch == ';' and d <= 0:
            return k
    raise AssertionError('sin ;')


def field_end(masked, m):
    mt = m.group(0)
    if mt.rstrip().endswith('get') and '{' in mt:
        brace = m.start() + mt.rindex('{')
        d = 0
        for k in range(brace, len(masked)):
            if masked[k] == '{':
                d += 1
            elif masked[k] == '}':
                d -= 1
                if d == 0:
                    rest = re.match(r'\s*=(?!>)', masked[k + 1:k + 40])
                    return statement_end(masked, k + 1) if rest else k
    return statement_end(masked, m.end() - 1)
missing = []
for path, names in sorted(by_file.items()):
    text, bom, crlf = show(path)
    masked = lc.mask(text)
    spans = []
    for name in sorted(set(names)):
        found = 0
        if name.startswith('tipo:'):
            for m in re.finditer(r'\b(?:class|struct|record)\s+' + re.escape(name[5:]) + r'\b', masked):
                spans.append(lines_of_span(text, m.start(), body_end(masked, m.end(), m.start())))
                found += 1
        else:
            # una declaracion empieza su linea por sus modificadores (asi no casa una llamada con `new` dentro de un cuerpo)
            for m in re.finditer(r'^[ \t]*(?:(?:public|private|internal|protected|static|async|override|sealed|readonly|virtual|unsafe|extern)\s+)+'
                                 r'[\w<>\[\],.?()\s]*?\b' + re.escape(name) + r'\s*(?:<[^>()]*>)?\s*\(', masked, re.M):
                spans.append(lines_of_span(text, m.start(), body_end(masked, m.end(), m.start())))
                found += 1
            if not found:
                for m in FIELD.finditer(masked):
                    if m.group(1) == name:
                        spans.append(lines_of_span(text, m.start(), field_end(masked, m)))
                        found += 1
        if not found:
            missing.append((path, name))
    # el comienzo de cada tramo, al principio de su linea; los tramos solapados se unen
    files[path] = [remove_spans(text, sorted(set(spans))), bom, crlf]
    log.append('tests %s: %d miembros (%d tramos)' % (path, len(set(names)), len(set(spans))))
for path, name in missing:
    log.append('NO ENCONTRADO %s %s' % (path, name))

for path in deleted:
    target = os.path.join(COPY, path)
    if not DRY and os.path.exists(target):
        os.remove(target)
    log.append('borrado %s' % path)
for path, (text, bom, crlf) in files.items():
    if DRY:
        before = show(path)[0]
        log.append('en seco %s: %d -> %d lineas' % (path, before.count('\n'), text.count('\n')))
        continue
    data = text.replace('\n', '\r\n') if crlf else text
    raw = (b'\xef\xbb\xbf' if bom else b'') + data.encode('utf-8')
    target = os.path.join(COPY, path)
    assert os.path.isfile(target), target
    open(target, 'wb').write(raw)
    log.append('escrito %s %s' % (hashlib.sha256(text.encode('utf-8')).hexdigest(), path))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print('variante %s: %d archivos escritos, %d borrados, %d bajas de tests, %d no encontradas' % (VARIANT, len(files), len(deleted), len(rows), len(missing)))
