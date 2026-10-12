# Render the A-6 round-2 candidate from a6r2.src.md: KEEP directives copy round-1 sections verbatim (blob eac473c0), Q directives insert literal
# quotes from the sources at 1652dd61, LINE_A/LINE_B copy the Owner lines verbatim, and the remaining placeholders come from fill files.
import hashlib, json, os, re, subprocess, sys
NL = chr(10)
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
HERE = os.path.dirname(os.path.abspath(__file__))
R1_COMMIT, R1_BLOB, LIT = '7b5ffac9', 'eac473c0d2e58cf1c4594f48a8816b3833af6509', '1652dd61'
SRC = {'AP': 'docs/AUTOMATION_PLAN.md', 'RAE': 'docs/automation/agent-execution/README.md', 'V14': 'docs/initiatives/I-62-proposal-v14.md'}
show = lambda rev, p: subprocess.run(['git', '-C', WT, 'show', rev + ':' + p], capture_output=True, check=True).stdout.decode('utf-8')
assert subprocess.run(['git', '-C', WT, 'rev-parse', R1_COMMIT + ':docs/initiatives/I-62-A-6.md'], capture_output=True, text=True).stdout.strip() == R1_BLOB
r1 = show(R1_COMMIT, 'docs/initiatives/I-62-A-6.md').split(NL)
srcl = {k: show(LIT, v).split(NL) for k, v in SRC.items()}


def section(start_pred, stop_re):
    i = next(k for k, l in enumerate(r1) if start_pred(l))
    j = next((k for k in range(i + 1, len(r1)) if re.match(stop_re, r1[k])), len(r1))
    out = r1[i:j]
    while out and out[-1] == '':
        out.pop()
    return out


def keep(tag):
    if tag == 'SECTION_2':
        return section(lambda l: l.startswith('## 2. '), r'^## 3\. ')
    if re.fullmatch(r'\d+', tag):
        return section(lambda l: l.startswith('## ' + tag + '. '), r'^## ')
    return section(lambda l: l.startswith('### ' + tag + ' '), r'^### |^## ')


def quote(src, a, b):
    return ['>' if srcl[src][n - 1] == '' else '> ' + srcl[src][n - 1] for n in range(a, b + 1)]


line_a = [l for l in r1 if l.startswith('A6-ADR0046-9-FX02 = A (')]
line_b = [l for l in r1 if l.startswith('A6-CONTROLLER-MEDICION-CONSUMO = A (')]
assert len(line_a) == 1 and len(line_b) == 1
assert hashlib.sha256(line_a[0].encode('utf-8')).hexdigest() == '83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14'
fills = json.load(open(os.path.join(HERE, 'fills.json'), encoding='utf-8'))
out = []
for l in open(os.path.join(HERE, 'a6r2.src.md'), encoding='utf-8').read().split(NL):
    m = re.fullmatch(r'@@KEEP (\S+)@@', l)
    if m:
        out += keep(m.group(1))
        continue
    if l == '@@KEEP_SECTION_2@@':
        out += keep('SECTION_2')
        continue
    m = re.fullmatch(r'@@Q (AP|RAE|V14) (\d+)-(\d+)@@', l)
    if m:
        out += quote(m.group(1), int(m.group(2)), int(m.group(3)))
        continue
    if l == '@@LINE_A@@':
        out.append(line_a[0])
        continue
    if l == '@@LINE_B@@':
        out.append(line_b[0])
        continue
    if l == '@@KEEP_3.3_WITH_VERIFY@@':
        s = keep('3.3')
        txt = NL.join(s)
        txt = txt.replace('(A6-1,\nregla 6; A6-7, regla 5)', '(A6-1,\nregla 8; A6-7, regla 5)')
        txt = txt.replace('2. la declaración del intérprete en el texto de la invocación: ruta, versión y SHA-256 del ejecutable, que nombra la disposición del Coordinator.',
                          '2. la declaración del intérprete y de Git en el texto de la invocación: rutas y SHA-256 de los ejecutables, y versión del intérprete, que nombra'
                          + NL + '   la disposición del Coordinator.')
        assert 'declaración del intérprete y de Git' in txt, 'KEEP 3.3 item 2'
        txt = txt.replace('**Insumos que recibe el Controller por A-6** (A6-1, regla 5; A6-2), y ninguno más:',
                          '**Insumos que recibe el Controller por A-6**, solo en sus invocaciones VERIFY (A6-1, regla 6; A6-2), y ninguno más:')
        assert 'regla 8; A6-7, regla 5' in txt and 'solo en sus invocaciones VERIFY (A6-1, regla 6' in txt, 'KEEP 3.3 edit'
        out += txt.split(NL)
        continue
    if l == '@@KEEP_3.13_PLAN@@':
        s = keep('3.13')
        k = s.index('**Lo que A-6 no hace:**')
        s = s[:k + 2] + ['- No cambia la planificación (PLAN) del Controller: sigue el procedimiento vigente, sin el instrumento ni el aceptante (A6-1, regla 2).'] + s[k + 2:]
        out += s
        continue
    if l == '@@KEEP_6.1_SURFACES@@':
        out += fills['SURFACES'].rstrip(NL).split(NL)
        out.append('')
        out += keep('6.2')
        continue
    for key, val in fills.items():
        l = l.replace('@@' + key + '@@', val.rstrip(NL))
    out.append(l)
text = NL.join(out).rstrip(NL) + NL
left = re.findall(r'@@[A-Z0-9_. ]+@@', text)
assert not left, left
dst = os.path.join(HERE, 'I-62-A-6.r2.md')
open(dst, 'w', encoding='utf-8', newline=NL).write(text)
b = text.encode('utf-8')
print('ok', len(text.split(NL)), hashlib.sha1(b'blob %d\0' % len(b) + b).hexdigest(), hashlib.sha256(b).hexdigest()[:16])
