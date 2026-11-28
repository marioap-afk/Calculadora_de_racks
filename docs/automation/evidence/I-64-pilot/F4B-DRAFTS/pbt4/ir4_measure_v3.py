"""ir4_measure_v3: mediciones de solo lectura del revisor para la recomprobacion de la v3 de PB-T4.

Uso: python -I ir4_measure_v3.py <checkout principal> <rev> <salida.txt>
Solo llama a `git -C <checkout> show|grep|ls-tree|diff` (objetos git) y escribe solo <salida.txt>.
Partes:
  1. Que cambia en src/ y tests/ entre 7dfc7c21 y <rev>.
  2. Las 9 clases del lector doble: metodos de prueba delimitados por llaves (no por el atributo siguiente), y por cada uno
     las senales que lo harian fallar en PB-T4b con la opcion 1 de Q4 (el kit editado), separando el texto del metodo y el
     de los ayudantes del archivo que el metodo llama.
  3. Los usos directos de FromPilot( fuera del kit.
  4. Las pruebas propias del piloto (Q1, opcion Retirar): metodos que nombran lo que se retira.
"""
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]
out = []


def git(*args, ok=(0,)):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True)
    if p.returncode not in ok:
        raise SystemExit('git %s -> %d: %s' % (' '.join(args), p.returncode, p.stderr.decode('utf-8', 'replace')))
    return p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')


def show(path):
    return git('show', '%s:%s' % (rev, path))


# ------------------------------------------------------------------ 1
out.append('## 1. src/ y tests/ entre 7dfc7c21 y %s' % rev)
for l in git('diff', '--stat', '7dfc7c21', rev, '--', 'src', 'tests').splitlines():
    out.append('    ' + l)


# ------------------------------------------------------------------ utilidades de C#
def strip_strings_and_comments(text):
    # Sustituye cadenas y comentarios por espacios del mismo largo, para contar llaves sin tropezar con ellas.
    res = list(text)
    i, n = 0, len(text)
    while i < n:
        c = text[i]
        if text.startswith('//', i):
            j = text.find('\n', i)
            j = n if j < 0 else j
            for k in range(i, j):
                res[k] = ' '
            i = j
        elif text.startswith('/*', i):
            j = text.find('*/', i + 2)
            j = n if j < 0 else j + 2
            for k in range(i, j):
                if res[k] != '\n':
                    res[k] = ' '
            i = j
        elif text.startswith('@"', i):
            j = i + 2
            while j < n:
                if text[j] == '"' and j + 1 < n and text[j + 1] == '"':
                    j += 2
                    continue
                if text[j] == '"':
                    break
                j += 1
            for k in range(i, min(j + 1, n)):
                if res[k] != '\n':
                    res[k] = ' '
            i = j + 1
        elif c == '"':
            j = i + 1
            while j < n and text[j] != '"' and text[j] != '\n':
                j += 2 if text[j] == '\\' else 1
            for k in range(i, min(j + 1, n)):
                res[k] = ' '
            i = j + 1
        elif c == "'":
            j = i + 1
            while j < n and text[j] != "'" and text[j] != '\n':
                j += 2 if text[j] == '\\' else 1
            for k in range(i, min(j + 1, n)):
                res[k] = ' '
            i = j + 1
        else:
            i += 1
    return ''.join(res)


ATTR = re.compile(r'^\s*\[(Fact|Theory)\b', re.M)
SIG = re.compile(r'\b(?:public|private|internal|protected)\s+(?:static\s+)?(?:async\s+)?[\w<>\[\],?. ]+?\s+(\w+)\s*\(')


def block_end(clean, start):
    # El cuerpo que empieza en la primera '{' despues de start; devuelve el indice tras su '}' (o una sentencia => ...;).
    brace = clean.find('{', start)
    arrow = clean.find('=>', start)
    semi = clean.find(';', start)
    if arrow >= 0 and (brace < 0 or arrow < brace) and semi >= 0:
        return semi + 1
    depth = 0
    for k in range(brace, len(clean)):
        if clean[k] == '{':
            depth += 1
        elif clean[k] == '}':
            depth -= 1
            if depth == 0:
                return k + 1
    return len(clean)


def split_tests(text):
    clean = strip_strings_and_comments(text)
    tests = []
    spans = []
    for m in ATTR.finditer(clean):
        sig = SIG.search(clean, m.end())
        if not sig:
            continue
        end = block_end(clean, sig.end())
        tests.append((sig.group(1), m.start(), end))
        spans.append((m.start(), end))
    # Lo demas del archivo: los ayudantes (metodos, propiedades y clases anidadas que no son pruebas).
    rest = list(text)
    for a, b in spans:
        for k in range(a, b):
            if rest[k] != '\n':
                rest[k] = ' '
    return tests, ''.join(rest), clean


def helper_names(rest_text, token):
    # Nombres de miembros del resto del archivo cuyo cuerpo contiene el token (un nivel, nada transitivo).
    clean = strip_strings_and_comments(rest_text)
    names = set()
    for m in SIG.finditer(clean):
        end = block_end(clean, m.end())
        if token.search(rest_text[m.start():end]):
            names.add(m.group(1))
    return names


# ------------------------------------------------------------------ 2
TW = 'tests/RackCad.Tests/Workspace/'
NINE = ['WorkspaceLifecycleHostTests', 'WorkspacePilotPrepareTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotEditHostTests',
        'WorkspaceCommandAdvisorTests', 'WorkspacePilotUpdateRequestTests', 'WorkspacePilotUpdateHostTests',
        'WorkspacePilotReconciliationHostTests', 'WorkspaceAssociativeBookkeepingTests']
SIGNALS = [
    ('C1-tipo', re.compile(r'Find<PushBackPilotDraftState>|\bPushBackPilotDraftState\b|PushBackPilotStateComparer')),
    ('C2-AB', re.compile(r'OutOfPilotDiagnostic|CompositeOutOfPilotReason')),
    ('C3-conservar', re.compile(r'TryKeepMine|TryKeepPilotEdit|KeepPilotEditsOn')),
    ('C4-actualizar', re.compile(r'PushBackPilotUpdateRun\.(?:Execute|Precheck)\b|NonPilotContentReason|TryUpdatePilot')),
    ('C6-adaptador', re.compile(r'PushBackPilotFieldAdapter')),
    ('campo-sin-regla', re.compile(r'\.SetPendingText\s*\(|\.CommitField\s*\(|PilotBoundaryKind\.(?:TabChange|RackChange|Update)\b')),
    ('FromPilot-directo', re.compile(r'FromPilot\s*\(|PrepareKit\.Reread\s*\(')),
]
out.append('')
out.append('## 2. Las 9 clases: metodos (por llaves) y senales de fallo con la opcion 1 (texto del metodo + ayudantes del archivo, un nivel)')
grand = {'metodos': 0}
any_direct = 0
any_with_helpers = 0
for cls in NINE:
    text = show(TW + cls + '.cs')
    tests, rest, clean = split_tests(text)
    helpers = {name: helper_names(rest, rx) for name, rx in SIGNALS}
    per = {name: 0 for name, _ in SIGNALS}
    direct_any = 0
    full_any = 0
    for tname, a, b in tests:
        body = text[a:b]
        hit_direct = False
        hit_any = False
        for name, rx in SIGNALS:
            direct = bool(rx.search(body))
            via = any(re.search(r'\b' + re.escape(h) + r'\b', body) for h in helpers[name]) if helpers[name] else False
            if direct or via:
                per[name] += 1
                hit_any = True
            if direct and name in ('C1-tipo', 'C2-AB', 'C3-conservar', 'C4-actualizar', 'C6-adaptador'):
                hit_direct = True
        direct_any += hit_direct
        full_any += hit_any
    grand['metodos'] += len(tests)
    any_direct += direct_any
    any_with_helpers += full_any
    out.append('- %s: %d metodos; directos C1-C6: %d; con ayudantes y las senales nuevas: %d; %s' % (
        cls, len(tests), direct_any, full_any, ', '.join('%s=%d' % (k, v) for k, v in per.items())))
    for name, hs in helpers.items():
        if hs:
            out.append('    ayudantes con %s: %s' % (name, ', '.join(sorted(hs))[:300]))
out.append('- TOTAL: %d metodos; directos C1-C6: %d; con ayudantes y senales nuevas: %d' % (grand['metodos'], any_direct, any_with_helpers))

# ------------------------------------------------------------------ 3
out.append('')
out.append('## 3. FromPilot( y PrepareKit.Reread( fuera del kit (siguen sin FullRead con la opcion 1)')
for l in git('grep', '-n', '-E', '-e', r'FromPilot\s*\(|PrepareKit\.Reread\s*\(', rev, '--', 'tests', ok=(0, 1)).splitlines():
    out.append('    ' + l[len(rev) + 1:][:200])

# ------------------------------------------------------------------ 4
out.append('')
out.append('## 4. Q1 (Retirar): metodos de prueba que nombran lo que se retira, por archivo')
RETIRE = re.compile(r'PushBackPilotStateComparer|PushBackPilotFieldAdapter|KeepPilotEditsOn|ComposeForWrite|PushBackPilotPrepareInput|'
                    r'PushBackPilotUpdatePlanner\.Prepare\b|PushBackPilotUpdateRun\.(?:Execute|Precheck)\b')
total4 = 0
files4 = 0
for path in [p for p in git('ls-tree', '-r', '--name-only', rev, 'tests').splitlines() if p.endswith('.cs')]:
    text = show(path)
    if not RETIRE.search(text):
        continue
    tests, rest, clean = split_tests(text)
    hs = helper_names(rest, RETIRE)
    n = 0
    for tname, a, b in tests:
        body = text[a:b]
        if RETIRE.search(body) or any(re.search(r'\b' + re.escape(h) + r'\b', body) for h in hs):
            n += 1
    if tests:
        files4 += 1
        total4 += n
        out.append('- %s: %d de %d metodos' % (path, n, len(tests)))
    else:
        out.append('- %s: (kit, sin pruebas)' % path)
out.append('- TOTAL: %d metodos en %d archivos con pruebas' % (total4, files4))

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('# ir4_measure_v3: salida (rev %s)\n' % rev)
    f.write('\n'.join(out) + '\n')
print('lineas', len(out))
