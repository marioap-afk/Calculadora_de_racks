# -*- coding: utf-8 -*-
"""Solo lectura: la remedicion de PB-T3b sobre el cierre de PB-T3a (precondicion 2 y section 6.0 del paquete de PB-T3 v4.5). Es el seco del generador
(make_pbt3b_contract.py --dry --objects-rev <cierre> --repo <checkout> --package-file <v4.5>) mas lo que el generador no mide y la remedicion de PB-T3b
pide:
- de la salida del seco: el anclaje, las anclas, D-1 (S-1 a S-6 y sus filas), Workspace/ (79 y 76), los recuentos y alcances, los MinSelected, la
  Snapshot K, C11, la tabla de RR-3 y los AVISOS, frente a lo previsto en el paquete (98 en src/, 76 de 79, Core + 21 y la linea base de PB-T3a);
- el censo de los disenos que usan las ocho clases de comportamiento del piloto de F4 (section 4), con su estado de bloqueo (precondicion 2; V2-09):
  los fixtures de Fixtures/I64 que nombran y los terminos que fabrican un diseno bloqueado (StructureOverride, IsBlocked, huecos y separadores), y
  la linea de R-8 (bloqueado=True en PushBackStateNetFingerprintTests.cs), por objetos git;
- el barrido de las guardas sobre el cierre (section 5.0 y 4.1, V45-03): la comparacion de guard_sweep.py en 18e4b783 y en el cierre
  (sweep-pbt3b-close.txt, de sweep_t3b_diff.py), con la lectura del alcance de la unica guarda nueva que lee src/ (PushBackDraftWriteGuardTests).
Uso: python -I measure_pbt3b.py <checkout principal> <cierre de PB-T3a>   -> escribe pbt3b-anchors-out.txt en esta carpeta (y nada mas: el contrato en
seco va a un temporal de esta carpeta que se borra al terminar). El checkout va por argumento: el guion no lleva rutas locales."""
import hashlib, json, os, re, subprocess, sys

REPO, CLOSE = sys.argv[1], sys.argv[2]
HERE = os.path.dirname(os.path.abspath(__file__))
GEN = 'make_pbt3b_contract.py'
PKG = 'I-64-f4b-pbt3-gate-contract-package-v45.md'
SWEEP = 'sweep-pbt3b-close.txt'
TMP = '_measure_pbt3b_contract.json'
OUT = 'pbt3b-anchors-out.txt'
assert re.fullmatch(r'[0-9a-f]{40}', CLOSE), 'el cierre de PB-T3a, sha de 40'
CI_FORECAST = (19107, 2451, 21)  # lo previsto del GREEN de PB-T3a (section 6.1), la base de "en cadena" de PB-T3b
WS = 'tests/RackCad.Tests/Workspace/'
F4_CLASSES = [('WorkspacePilotPrepareTests', 67), ('WorkspacePilotUpdateRunTests', 49), ('WorkspaceDraftPilotMaterializationTests', 41),
              ('WorkspacePilotUpdateHostTests', 35), ('WorkspacePilotUpdateRequestTests', 44), ('WorkspacePilotReconciliationHostTests', 28),
              ('WorkspaceDraftPilotIntentReadTests', 63), ('WorkspaceDraftPilotStateTests', 27)]  # section 4 del paquete, con sus metodos de R-8
BLOCK_TERMS = r'StructureOverride[AB]|IsBlocked|StatusOr|NegativeGap|\.Gap\s*=|Separator\w*\s*=|BlockedPrefix'


def sha(path):
    return hashlib.sha256(open(os.path.join(HERE, path), 'rb').read()).hexdigest()


def gshow(path):
    r = subprocess.run(['git', '-C', REPO, 'show', CLOSE + ':' + path], capture_output=True)
    assert r.returncode == 0, path
    return r.stdout.decode('utf-8').replace('\r\n', '\n')


cmd = [sys.executable, '-I', GEN, '2026-11-20T00:00:00Z', CLOSE, '812baf879612320653094a8aaf46aca0bac81383', '--dry', '--objects-rev', CLOSE,
       '--repo', REPO, '--package-file', PKG, '--json-out', TMP]
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
assert ('el cierre %s (F4B-E43)' % CLOSE[:8]) in anc and 'src y tests == el cierre == el GREEN de PB-T3a' in anc, anc
m = re.search(r'forbidden (\d+) \(src (\d+) tests (\d+) \[core (\d+) ui (\d+)\] root (\d+) \)', head)
forb, src_n, tests_n, core_n, ui_n, root_n = (int(x) for x in m.groups())
d1 = line('D-1 (F4B-E29): ')
c13 = line('PBT3B-C13: ')
req = [l for l in L if re.match(r'  (Core|UI) +\S+ +(new|ws|t3a) +paquete=', l)]
rows = [re.match(r'  (Core|UI) +(\S+) +(\w+) +paquete=(\S+) .*-> MinSelected=(\d+) ExpectRed=(\w+)', l).groups() for l in req]
ia = L.index([l for l in L if l.startswith('AVISOS (')][0])
avisos = [l for l in L[ia + 1:] if l.startswith('  - ')]
ci = contract['ExpectedEvidence'][-1]
mc = re.search(r'run (\d+) sobre ([0-9a-f]{8}): Core (\d+); UI (\d+) resultados, (\d+) Skipped', ci)
assert mc, ci
ci_got = (int(mc.group(3)), int(mc.group(4)), int(mc.group(5)))

# el censo de los disenos de las clases de comportamiento del piloto de F4 (precondicion 2; V2-09)
census = []
for cls, methods in F4_CLASSES:
    t = gshow(WS + cls + '.cs')
    facts = len(re.findall(r'^\s*\[Fact', t, re.M))
    theories = len(re.findall(r'^\s*\[Theory', t, re.M))
    fixtures = sorted(set(x.rstrip('-') for x in re.findall(r'pb-\d\d-[a-z0-9-]+', t)))
    # la barrera vive en el armado del plan (D-PBT3-09 (d2)): solo cuenta en las clases que llegan a Prepare, Precheck o Execute; un termino en una
    # linea esperada (por ejemplo una diferencia JSON "... -> null") no es un gesto
    reaches = len(re.findall(r'PushBackPilotUpdatePlanner\.Prepare\(|PushBackPilotUpdateRun\.(?:Execute|Precheck)\(|RequestUpdate\(|PilotUpdateHost', t))
    blocks = sorted(set(m_.group(0).strip() for m_ in re.finditer(BLOCK_TERMS, t)))
    gestures = sorted(set(m_.group(0).strip() for m_ in re.finditer(r'(?:' + BLOCK_TERMS + r')\s*=(?!=)|\["(?:StructureOverride[AB]|Gap|Separator\w*)"\]\s*=', t)))
    census.append((cls, facts + theories, methods, fixtures, blocks, reaches, gestures))
fx_all = sorted({f for c_ in census for f in c_[3]})
corpus = sorted(p_[len('tests/RackCad.Tests/Fixtures/I64/'):-5] for p_ in subprocess.run(['git', '-C', REPO, 'ls-tree', '-r', '--name-only', CLOSE, '--',
                                                                                         'tests/RackCad.Tests/Fixtures/I64/'],
                                                                                        capture_output=True).stdout.decode().split('\n') if p_)
fp = gshow('tests/RackCad.UI.Tests/PushBackStateNetFingerprintTests.cs')
blocked_true = fp.count('bloqueado=True')
# el barrido de las guardas sobre el cierre: la unica guarda nueva que lee src/ es la de PB-T3a, y su alcance deja fuera la interfaz nueva
sw = open(os.path.join(HERE, SWEEP), encoding='utf-8').read()
sw_new = re.search(r'filas del corpus nuevas: (\d+) \| perdidas: (\d+)', sw)
sw_guards = re.search(r'guardas con coincidencias \(cualquier corpus\): antes (\d+), despues (\d+); nuevas: (.*)', sw)
dwg = gshow('tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs')
may = re.search(r'private static bool MayNameTheModel\(string relative\)\s*=>(.*?);', dwg, re.S).group(1)
may_ok = ('src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs' in may and
          'src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs' in may and 'DraftFolder' in may and 'IPushBackPilotViewEraser' not in may)
names = re.search(r'ConnectedNames =\s*\{(.*?)\};', dwg, re.S).group(1)
connected = re.findall(r'"(\w+)"', names)
draft_only = re.search(r'private static readonly string\[\] DraftFiles =\s*\{(.*?)\};', dwg, re.S).group(1).count('DraftFolder +') == 8
eraser_names = ['PushBackPilotUnitResult', 'IPushBackPilotViewEraser', 'EraseUnit']  # el texto de la interfaz en D-PBT3-10
sw_ok = (sw_new and sw_new.group(2) == '0' and sw_guards and sw_guards.group(3).strip() == 'tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs' and may_ok
         and draft_only and not set(connected) & set(eraser_names))

cmp = []  # (que, previsto, medido, igual, explicacion si difiere)
cmp.append(('src/ del ForbiddenWriteScope', '98 (compat-pbt3-vs-pbt2.md section 3)', str(src_n), src_n == 98,
            'Draft/, la carpeta que creo PB-T3a, es una entrada mas del algoritmo de [PKC-C] (98 + 1); el resto, igual'))
cmp.append(('tests/ (Core, UI)', '109 (28 + 76 + 5), 69 (68 + 1)', '%d, %d' % (core_n, ui_n), (core_n, ui_n) == (109, 69), ''))
cmp.append(('Workspace/ (archivos, lista explicita)', '79, 76', re.search(r'Workspace/: (\d+) archivos, (\d+)', d1).group(1) + ', ' +
            re.search(r'Workspace/: (\d+) archivos, (\d+)', d1).group(2), 'Workspace/: 79 archivos, 76 en la lista explicita' in d1, ''))
cmp.append(('D-1: S-1 a S-6 sin Skip, filas', '10 (5 Fact y 5 de S-2)', re.search(r'(\d+) filas', d1).group(1), '10 filas' in d1, ''))
cmp.append(('PBT3B-C13: nombres nuevos libres en Pilot/', '19', re.search(r'(\d+) nombres nuevos', c13).group(1), '19 nombres nuevos libres' in c13, ''))
cmp.append(('linea base de la CI (Core, UI, Skipped)', '%d, %d, %d' % CI_FORECAST, '%d, %d, %d (run %s sobre %s)' % (ci_got + (mc.group(1), mc.group(2))),
            ci_got == CI_FORECAST, ''))
for proj, cls, kind, pk, mn, er in rows:
    if kind == 't3a':
        cmp.append(('MinSelected %s %s (contrato de PB-T3a)' % (proj, cls), pk, mn, pk == mn, ''))
    if kind == 'ws':
        cmp.append(('MinSelected Core Workspace (3 271 - 10 filas de D-1)', '3261', mn, mn == '3261', ''))
cmp.append(('AVISOS del cierre de PB-T3a', '0 (solo de la emision de PB-T3b)', str(len([a for a in avisos if not a.startswith('  - emision: ')])),
            all(a.startswith('  - emision: ') for a in avisos), ''))
cmp.append(('disenos de las clases de F4: fuera de Fixtures/I64', '0', str(len([f for f in fx_all if f not in corpus])), all(f in corpus for f in fx_all), ''))
cmp.append(('gestos de un diseno bloqueado en las clases de F4 que llegan al plan', '0', str(sum(len(c_[6]) for c_ in census if c_[5])),
            not any(c_[6] for c_ in census if c_[5]), ''))
cmp.append(('R-8: bloqueado=True en PushBackStateNetFingerprintTests.cs', '0', str(blocked_true), blocked_true == 0, ''))
cmp.append(('barrido de las guardas sobre el cierre: colisiones nuevas con PP, UR o la interfaz', '0', '0' if sw_ok else 'por leer', sw_ok, ''))
diff = [c for c in cmp if not c[3]]
material = [c for c in diff if not c[4]]

R = []
R.append('# measure_pbt3b.py: la remedicion de PB-T3b sobre el cierre de PB-T3a (precondicion 2 y section 6.0 del paquete de PB-T3 v4.5): el seco del')
R.append('# generador sobre ese cierre, el censo de los disenos de las clases de F4 y el barrido de las guardas, frente a lo previsto.')
R.append('')
R.append('## La corrida')
R.append('- cierre de PB-T3a: %s (F4B-E43)' % CLOSE)
R.append('- generador: %s, sha256 %s' % (GEN, sha(GEN).upper()))
R.append('- paquete: %s, sha256 %s' % (PKG, sha(PKG)))
R.append('- barrido: %s, sha256 %s (de guard_sweep.py, sha256 %s, y sweep_t3b_diff.py, sha256 %s)' % (SWEEP, sha(SWEEP), sha('guard_sweep.py'),
                                                                                                     sha('sweep_t3b_diff.py')))
R.append('- orden: python -I %s <IssuedUtc> <AuthorityRevision> <MainSha> --dry --objects-rev %s --repo <checkout-principal> --package-file %s '
         '--json-out <temporal>' % (GEN, CLOSE[:8], PKG))
R.append('- rc %d; contrato en seco: %d entradas de evidencia, %d autoridades' % (p.returncode, len(contract['ExpectedEvidence']), len(contract['Authorities'])))
R.append('')
R.append('## Frente a lo previsto: %s' % ('SIN CAMBIOS' if not diff else ('%d DIFERENCIAS, %d sin explicar' % (len(diff), len(material)))))
for what, a, b_, ok, why in cmp:
    R.append('%-8s %s: previsto %s | medido %s%s' % ('igual' if ok else 'DISTINTO', what, a, b_, ('' if ok or not why else ' | ' + why)))
R.append('')
R.append('## El censo de los disenos de las clases de comportamiento del piloto de F4 (precondicion 2; V2-09)')
R.append('Lectura estatica por objetos git (no se ejecuto nada): los fixtures de Fixtures/I64 que nombra cada clase y los terminos que fabrican un diseno')
R.append('bloqueado. El corpus de Fixtures/I64 no esta bloqueado (R-8: CT-PBT1A-14 da bloqueado=False en todas sus lineas). Un diseno hecho en codigo')
R.append('con otros gestos no se mide aqui: lo cubre PBT3B-C14 (una prueba de comportamiento del piloto que cae).')
for cls, n, methods, fx, blocks, reaches, gestures in census:
    R.append('- %s: %d metodos (R-8: %d); llega al plan: %s; fixtures: %s; terminos de bloqueo: %s; gestos que los ponen: %s'
             % (cls, n, methods, ('si (%d)' % reaches) if reaches else 'no', ', '.join(fx) or 'ninguno', ', '.join(blocks) or 'ninguno',
                ', '.join(gestures) or 'ninguno'))
R.append('- Fixtures/I64 en el cierre: %d; nombrados por las clases: %d, %s' % (len(corpus), len(fx_all), 'todos del corpus' if all(f in corpus for f in fx_all)
                                                                              else 'FUERA del corpus: ' + ', '.join(f for f in fx_all if f not in corpus)))
R.append('')
R.append('## El barrido de las guardas sobre el cierre (section 5.0 y 4.1; V45-03)')
R += sw.rstrip('\n').split('\n')[:5]
R.append('- la guarda nueva, PushBackDraftWriteGuardTests (PB-T3a): CT-12 deja nombrar el modelo a Draft/, PP y UR (MayNameTheModel: %s) y busca %s; '
         'la interfaz nueva no los nombra (%s); CT-13 y CT-14 leen solo los ocho de Draft/ (%s). Sin colision con PB-T3b'
         % ('si' if may_ok else 'NO', ', '.join(connected), ', '.join(eraser_names), 'si' if draft_only else 'NO'))
R.append('')
R.append('## Las anclas y los recuentos del seco, tal cual')
for pref in ('DRY rev ', 'anclaje: ', 'contrato de PB-T3a: ', 'D-1 (F4B-E29): ', 'PBT3B-C13: ', 'PBT3B-C11: ', 'nombres de custodia nuevos',
             'Snapshot K (F4B-E37)', 'BOUNDED_RESULT: ', 'tabla de RR-3 ', 'D1: '):
    R.append(line(pref))
R.append('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
R += [l for l in L if l.startswith('  Core ') or l.startswith('  UI ') or l.startswith('       ')]
R.append('AVISOS del seco (%d), todos de la emision de PB-T3b (el commit X, la comprobacion acotada y el sustituto de nc2 v2):' % len(avisos))
R += avisos
R.append('')
R.append('## La linea base de la CI, en la ExpectedEvidence del contrato en seco')
R.append(ci)
text = '\n'.join(R) + '\n'
for bad in (r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]', r'\bUs[e]rs[\\/]', r'\bApp[D]ata\b'):  # sin rutas locales: se custodia
    assert not re.search(bad, text), bad
open(os.path.join(HERE, OUT), 'w', encoding='utf-8', newline='\n').write(text)
print('%s: %d lineas; frente a lo previsto: %s' % (OUT, len(R), 'SIN CAMBIOS' if not diff else '; '.join(c[0] + (' (explicada)' if c[4] else ' (SIN EXPLICAR)')
                                                                                                         for c in diff)))
