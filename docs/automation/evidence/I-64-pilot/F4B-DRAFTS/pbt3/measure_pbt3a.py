# -*- coding: utf-8 -*-
"""Solo lectura: la remedicion de PB-T3a sobre el cierre de PB-T2c (precondicion 2 del paquete de PB-T3 v4.4), que es el seco del generador
sobre ese cierre. Corre make_pbt3a_contract.py en seco por objetos git (--objects-rev <cierre> --pt2c-close <cierre> --repo <checkout>, con el
paquete v4.4 y el informe de CA-T3a de esta carpeta), extrae del seco la seccion de anclas y los recuentos, y los compara con lo previsto en el
paquete:
- la base: las cinco referencias, src == el cierre de PB-T2b, y el cierre con su entrada (F4B-E40);
- las anclas de W, por su contenido, frente a las medidas tras el GREEN de PB-T2b que la v4.4 da por previstas (su precondicion 2, V43-10);
- los recuentos: las listas de tests/ (Core 29, UI 68), las nueve clases de PB-T2 en la revision, los MinSelected medidos frente a los del
  paquete, Pasada, I58F1, Fixtures/I64, la lista cerrada de C13 y los nombres nuevos libres;
- la Snapshot K, C11, C07 y C18, F4B-E35, BOUNDED_RESULT y la tabla de RR-3, tal cual los da el seco;
- la linea base de la CI (la de la ExpectedEvidence del contrato en seco) frente a la prevista (Core 19 094, UI 2 450 con 21 omitidas);
- los AVISOS: solo los de la custodia de la emision (el commit X), ninguno del cierre de PB-T2c.
Uso: python -I measure_pbt3a.py <checkout principal> <cierre de PB-T2c>   -> escribe pbt3a-anchors-out.txt en esta carpeta (y nada mas: el
contrato en seco va a un archivo temporal de esta carpeta que se borra al terminar). El checkout va por argumento: el guion no lleva rutas locales."""
import hashlib, json, os, re, subprocess, sys

REPO, CLOSE = sys.argv[1], sys.argv[2]
HERE = os.path.dirname(os.path.abspath(__file__))
GEN = 'make_pbt3a_contract.py'
PKG = 'I-64-f4b-pbt3-gate-contract-package-v44.md'
BOUNDED = 'independent-review-pbt3a-v43-check.md'
TMP = '_measure_pbt3a_contract.json'
OUT = 'pbt3a-anchors-out.txt'
assert re.fullmatch(r'[0-9a-f]{40}', CLOSE), 'el cierre de PB-T2c, sha de 40'
# lo previsto en la v4.4 (precondicion 2, su nota; V43-10): las anclas de W medidas tras el GREEN de PB-T2b
W_EXPECTED = {'adopcion': 'W:624-669', 'catch': 'W:627-635', 'ensambladores': 'W:150-151', 'LastComputation': 'W:221', 'LoadExisting': 'W:345',
              'costura': 'W:197', 'Recompute(); final de LoadFromModel': 'W:404'}
W_NOTE = ('la adopci' + chr(243) + 'n, W:624-669; su `catch`, W:627-635; `LoadFromModel`, W:370, con su `Recompute();` final en W:404, la '
          + chr(250) + 'ltima sentencia; los ensambladores, creados en W:150-151')
CI_FORECAST = (19094, 2450, 21)


def sha(path):
    return hashlib.sha256(open(os.path.join(HERE, path), 'rb').read()).hexdigest()


pkg = open(os.path.join(HERE, PKG), 'rb').read().decode('utf-8')
assert W_NOTE in pkg and 'la costura `EditableStateForTest => editable;`, W:197; `LastComputation`, W:221; `LoadExisting`, W:345' in pkg, \
    'las anclas previstas de W no son las de la nota de la precondicion 2 de la v4.4'
cmd = [sys.executable, '-I', GEN, '2026-11-19T00:00:00Z', '0' * 40, '812baf879612320653094a8aaf46aca0bac81383', '--dry', '--objects-rev', CLOSE,
       '--repo', REPO, '--pt2c-close', CLOSE, '--package-file', PKG, '--bounded-file', BOUNDED, '--json-out', TMP]
p = subprocess.run(cmd, cwd=HERE, capture_output=True)
out = p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')
try:
    assert p.returncode == 0, ('el seco sobre el cierre fallo', p.stderr.decode('utf-8', 'replace')[-600:])
    contract = json.load(open(os.path.join(HERE, TMP), encoding='ascii'))
finally:
    if os.path.exists(os.path.join(HERE, TMP)):
        os.remove(os.path.join(HERE, TMP))
L = out.split('\n')


def line(prefix):
    hits = [l for l in L if l.startswith(prefix)]
    assert len(hits) == 1, (prefix, len(hits))
    return hits[0]


head = line('DRY rev ')
assert head.startswith('DRY rev ' + CLOSE[:8] + ' '), head
anc = line('anclaje: ')
assert ('cierre de PB-T2c: %s (F4B-E40)' % CLOSE[:8]) in anc and 'src == cierre de PB-T2b' in anc, anc
wline = line('W por contenido (V42-10)')
# '[:]' en lugar de ':' tras la W: la misma expresion, sin un texto que FORBIDDEN de la redaccion comun tome por una ruta con letra de unidad
w_got = dict(re.findall(r'(adopcion|catch|ensambladores|LastComputation|LoadExisting|costura|Recompute\(\); final de LoadFromModel) (W[:][0-9]+(?:-[0-9]+)?)',
                        wline))
t2c = line('contrato de PB-T2c: ')
req = [l for l in L if re.match(r'  (Core|UI) +\S+ +(new|measured|fixed|pt2) +paquete=', l)]
rows = [re.match(r'  (Core|UI) +(\S+) +(\w+) +paquete=(\d+) .*-> MinSelected=(\d+) ExpectRed=(\w+)', l).groups() for l in req]
ia = L.index([l for l in L if l.startswith('AVISOS (')][0])
avisos = [l for l in L[ia + 1:] if l.startswith('  - ')]
ci = contract['ExpectedEvidence'][-1]
m = re.search(r'run (\d+) sobre ([0-9a-f]{8}): Core (\d+) Passed; UI (\d+) resultados, (\d+) Skipped', ci)
assert m, ci
ci_got = (int(m.group(3)), int(m.group(4)), int(m.group(5)))

cmp = []  # (que, previsto, medido, igual)
cmp.append(('anclas de W (por contenido)', '; '.join('%s %s' % kv for kv in W_EXPECTED.items()), '; '.join('%s %s' % kv for kv in w_got.items()),
            w_got == W_EXPECTED))
cmp.append(('linea base de la CI (Core, UI, Skipped)', '%d, %d, %d' % CI_FORECAST, '%d, %d, %d (run %s sobre %s)' % (ci_got + (m.group(1), m.group(2))),
            ci_got == CI_FORECAST))
cmp.append(('tests/ de PB-T3a (Core, UI)', '29, 68', re.search(r'Core (\d+) y UI (\d+)', t2c).group(1) + ', ' + re.search(r'Core (\d+) y UI (\d+)', t2c).group(2),
            'Core 29 y UI 68' in t2c))
cmp.append(('clases de PB-T2 en la revision', '9 de 9', re.search(r'(\d+ de 9)', t2c).group(1), '9 de 9' in t2c))
for proj, cls, kind, pk, mn, er in rows:
    if kind in ('measured', 'pt2', 'fixed'):
        cmp.append(('MinSelected %s %s (%s)' % (proj, cls, kind), pk, mn, pk == mn))
cmp.append(('AVISOS del cierre de PB-T2c', '0', str(len([a for a in avisos if 'cierre' in a and 'custodiar' not in a])),
            all('a custodiar con la emision' in a for a in avisos)))
diff = [c for c in cmp if not c[3]]

R = []
R.append('# measure_pbt3a.py: la remedicion de PB-T3a sobre el cierre de PB-T2c (precondicion 2 del paquete de PB-T3 v4.4), que es el seco del')
R.append('# generador sobre ese cierre: su seccion de anclas y sus recuentos, comparados con lo previsto en el paquete.')
R.append('')
R.append('## La corrida')
R.append('- cierre de PB-T2c: %s (F4B-E40)' % CLOSE)
R.append('- generador: %s, sha256 %s' % (GEN, sha(GEN).upper()))
R.append('- paquete: %s, sha256 %s' % (PKG, sha(PKG)))
R.append('- comprobacion acotada: %s, sha256 %s' % (BOUNDED, sha(BOUNDED)))
R.append('- orden: python -I %s <IssuedUtc> <AuthorityRevision> <MainSha> --dry --objects-rev %s --repo <checkout-principal> --pt2c-close %s '
         '--package-file %s --bounded-file %s --json-out <temporal>' % (GEN, CLOSE[:8], CLOSE[:8], PKG, BOUNDED))
R.append('- rc %d; contrato en seco: %d entradas de evidencia, %d autoridades' % (p.returncode, len(contract['ExpectedEvidence']), len(contract['Authorities'])))
R.append('')
R.append('## Frente a lo previsto: %s' % ('SIN CAMBIOS' if not diff else '%d DIFERENCIAS' % len(diff)))
for what, a, b, ok in cmp:
    R.append('%-6s %s: previsto %s | medido %s' % ('igual' if ok else 'DISTINTO', what, a, b))
R.append('')
R.append('## Las anclas y los recuentos del seco, tal cual')
for pref in ('DRY rev ', 'anclaje: ', 'contrato de PB-T2c: ', 'W por contenido (V42-10)', 'Pasada de [PKT1]', 'CT-PBT3A-04: ', 'PBT3A-C13: ',
             'nombres de custodia nuevos', 'Snapshot K (F4B-E37)', 'BOUNDED_RESULT: ', 'tabla de RR-3 ', 'D1: '):
    R.append(line(pref))
R.append('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
R += req
R.append('AVISOS del seco (%d), todos de la custodia de la emision de PB-T3a (el commit X):' % len(avisos))
R += avisos
R.append('')
R.append('## La linea base de la CI, en la ExpectedEvidence del contrato en seco')
R.append(ci)
text = '\n'.join(R) + '\n'
for bad in (r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]', r'\bUs[e]rs[\\/]', r'\bApp[D]ata\b'):  # sin rutas locales: se custodia
    assert not re.search(bad, text), bad
open(os.path.join(HERE, OUT), 'w', encoding='utf-8', newline='\n').write(text)
print('%s: %d lineas; frente a lo previsto: %s' % (OUT, len(R), 'SIN CAMBIOS' if not diff else ', '.join(c[0] for c in diff)))
