"""PB-T4, v4.1: las listas de sustituciones por texto de lo que eligio el Owner (owner-pbt4-decisions.md), metodo a metodo y con su causa.

Uso: python -I lists_pbt4.py <checkout principal> <rev> <salida.md> <salida.tsv>
Solo llama a `git -C <checkout> show|ls-tree` (objetos git, via lists_core.py); escribe solo las dos salidas.

Lo elegido: Q1 «Retirarlo», Q3 «Saltar y duplicar», Q2 «Por grupos» (no cambia las listas).
Una firma se mide en el texto de la prueba (sus atributos y su cuerpo) mas el de los ayudantes de su mismo archivo
que nombra, a un nivel. Una prueba que ya lleva `Skip` en la revision no entra en ninguna lista: ya esta sustituida.
"""
import re
import sys
import importlib.util
from collections import Counter

spec = importlib.util.spec_from_file_location('lc', __file__.replace('lists_pbt4.py', 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
lc.REPO, lc.REV = sys.argv[1:3]
out_md, out_tsv = sys.argv[3:5]
# El destino de cada prueba de las 9 clases (copia mecanica o a mano), de copy_map_pbt4.py.
DEST = {}
if len(sys.argv) > 5:
    with open(sys.argv[5], encoding='utf-8') as f:
        for line in f.read().splitlines()[1:]:
            c, n, _, _, d = line.split('\t')
            DEST[(c, n)] = d
REV = lc.REV

CAUSES = [
    ('C-1', r'Find<PushBackPilotDraftState>|DraftSession<PushBackPilotDraftState>|\bPushBackPilotDraftState\b|PushBackPilotStateComparer'),
    ('C-2', r'OutOfPilotDiagnostic|CompositeOutOfPilotReason'),
    ('C-3', r'\bTryKeepMine\b|KeepPilotEditsOn'),
    ('C-4', r'PushBackPilotUpdateRun\.(?:Execute|Precheck)\b|NonPilotContentReason|TryUpdatePilot\b'),
    ('C-6', r'PushBackPilotFieldAdapter'),
    ('C-9', r'\bFromPilot\s*\(|PrepareKit\.Reread\s*\('),
]
CRX = [(c, re.compile(r)) for c, r in CAUSES]
RETIRED = re.compile(r'PushBackPilotStateComparer|PushBackPilotFieldAdapter|KeepPilotEditsOn|ComposeForWrite|PushBackPilotUpdatePlanner\.Prepare\s*\(|PushBackPilotUpdateRun\.(?:Execute|Precheck)\s*\(')
UI_KEEP = re.compile(r'\bRequestKeepMine\b|BrowserFieldKeepMineButton|\bKeepMine\b')
SKIPPED = re.compile(r'\bSkip\s*=')
NINE = ['WorkspaceLifecycleHostTests', 'WorkspacePilotPrepareTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotEditHostTests',
        'WorkspaceCommandAdvisorTests', 'WorkspacePilotUpdateRequestTests', 'WorkspacePilotUpdateHostTests',
        'WorkspacePilotReconciliationHostTests', 'WorkspaceAssociativeBookkeepingTests']
TW = 'tests/RackCad.Tests/Workspace/'


def tests(path):
    """(miembro, texto) de cada prueba sin Skip; el texto lleva los atributos."""
    return [(m, text) for m, text in lc.tests_with_text(path) if not SKIPPED.search(m['attrs'])]


def at(path, line):
    for m, _ in tests(path):
        if m['first'] <= line <= m['last']:
            return m
    raise SystemExit('sin prueba en %s:%d' % (path, line))


def by_pattern(path, pattern):
    """Las pruebas cuyo texto PROPIO (atributos y cuerpo, sin ayudantes) casa con el patron."""
    rx = re.compile(pattern)
    return [m for m, text in tests(path) if rx.search(m['attrs'] + m['body'])]


rows = []   # (lista, archivo, metodo, lineas, causa)


def add(lst, path, m, cause):
    key = (path, m['name'])
    if any((r[1], r[2]) == key for r in rows):
        raise SystemExit('duplicada: %s %s' % key)
    rows.append((lst, path, m['name'], '%d-%d' % (m['first'], m['last']), cause))


# ------------------------------------------------------------------ PB-T4a: 1 (C-5)
add('PB-T4a', 'tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs', at('tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs', 61), 'C-5: CT-PBT3A-12 (RIA nombra `PushBackFullDraftReader`, D-PBT4-02; la sustituye CT-PBT4A-06)')

# ------------------------------------------------------------------ PB-T4b, C-5: las filas de §4.2, metodo a metodo
PWP = 'tests/RackCad.Tests/PushBackWritePathGuardTests.cs'
IG = TW + 'WorkspacePilotUpdateIntegrationGuardTests.cs'
EIG = TW + 'WorkspaceEditIntentGuardTests.cs'
PEG = TW + 'WorkspacePilotEditGuardTests.cs'
RG = TW + 'WorkspacePilotReconciliationGuardTests.cs'
add('PB-T4b C-5', PWP, at(PWP, 252), 'C-5: CT-PBT3B-13 (el anfitrion encuentra el borrador completo)')
for m in by_pattern(IG, r'Assert\.Empty\(\s*G4Violations\s*\('):
    add('PB-T4b C-5', IG, m, 'C-5: G-4 (IG:497-498, `PushBackPilotUpdateRun.Execute` deja de nombrarse; la prueba exige `Empty` sobre el texto real)')
for line, why in [(677, 'EIG:664-690, `pilot.Read` en `Begin` pasa a `full.Read`'),
                  (711, 'EIG:692-721, `KeepPilotEditsOn` en :711 (C-3)'),
                  (735, 'EIG:725-737, las seis `var pilot = read.PilotRead;`'),
                  (776, 'EIG:754-778, `KeepPilotEditsOn` en :776 (C-3)'),
                  (805, 'EIG:799-810, el consumidor de :805 con `KeepPilotEditsOn` (C-3)')]:
    add('PB-T4b C-5', EIG, at(EIG, line), 'C-5: ' + why)
for m in by_pattern(PEG, r'Assert\.Empty\(\s*HostViolations\s*\(|PushBackPilotFieldAdapter'):
    add('PB-T4b C-5', PEG, m, 'C-6: la regla del adaptador estático de `HostViolations` (PEG:280); la prueba exige `Empty` sobre el texto real o nombra el adaptador en sus datos')
add('PB-T4b C-5', RG, at(RG, 116), 'C-5: RG:109-123, `Mutate` sobre `RequestKeepMine` (RG:116)')
c5_newline = ('PB-T4b C-5', '(PB-T4a) ' + 'tests/RackCad.Tests/PushBackFullReadGuardTests.cs', 'CT_PBT4A_06 (nueva en PB-T4a)', '-', 'C-5: el censo de PB-T4a admite WHC en PB-T4b (la sustituye CT-PBT4B-13)')
rows.append(c5_newline)

# ------------------------------------------------------------------ PB-T4b, la UI de la eleccion (C-3)
for path in lc.ls('tests/RackCad.UI.Tests/Workspace'):
    for m, text in tests(path):
        if UI_KEEP.search(text):
            add('PB-T4b UI', path, m, 'C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09)')

# ------------------------------------------------------------------ PB-T4b, las 9 clases: «Saltar y duplicar» (C-0)
per_class = []
for cls in NINE:
    path = TW + cls + '.cs'
    n = 0
    for m, text in tests(path):
        causes = [c for c, r in CRX if r.search(text)]
        d = DEST.get((cls, m['name']))
        where = '' if d is None else ('; copia mecánica en `%s`' % cls.replace('Tests', 'FullTests') if d == 'mecanica' else '; a mano (%s)' % d)
        add('PB-T4b C-0', path, m, ', '.join(['C-0'] + causes) + where)
        n += 1
    per_class.append((cls, n))

# ------------------------------------------------------------------ PB-T4b, «Retirarlo» (C-7)
listed = {(r[1], r[2]) for r in rows}
nine_paths = {TW + c + '.cs' for c in NINE}
ret_files = Counter()
for path in lc.ls('tests'):
    if path in nine_paths:
        continue
    src = lc.show(path)
    if not RETIRED.search(src):
        continue
    for m, text in tests(path):
        if RETIRED.search(text) and (path, m['name']) not in listed:
            names = sorted(set(x.group(0) for x in RETIRED.finditer(text)))
            add('PB-T4b C-7', path, m, 'C-7: nombra ' + ', '.join('`%s`' % re.sub(r'\s*\($', '', n) for n in names))
            ret_files[path] += 1
# Lo que deja de compilar al retirar (retire_compile_pbt4.py), fuera de las 9 clases y no contado ya: tambien C-7.
if len(sys.argv) > 6:
    with open(sys.argv[6], encoding='utf-8') as f:
        for line in f.read().splitlines()[1:]:
            p, n, span, state = line.split('\t')
            if state != 'NUEVA' or (p, n) in {(r[1], r[2]) for r in rows}:
                continue
            rows.append(('PB-T4b C-7', p, n, span, 'C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`)'))
            ret_files[p] += 1
EQ = 'tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs'
for line, e in [(137, 'E-3'), (221, 'E-5'), (231, 'E-6')]:
    m = at(EQ, line)
    if (EQ, m['name']) in {(r[1], r[2]) for r in rows}:
        raise SystemExit('el equivalente %s ya esta en la lista' % e)
    add('PB-T4b C-7', EQ, m, 'C-7: %s de D-1 (F4B-E29: «PB-T4 sustituirá tres de esos equivalentes al quitar el piloto»)' % e)
    ret_files[EQ] += 1

# ------------------------------------------------------------------ PB-T4c: 2 (C-5)
add('PB-T4c', PEG, at(PEG, 158), 'C-5: PEG:155-163, las 17 operaciones del puerto pasan a 18 (`TryEditAll`)')
PSR = 'tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs'
add('PB-T4c', PSR, at(PSR, 307), 'C-5: CT-PBT1B-15, «archivos=14» pasa a «archivos=15» con `RackWorkspaceEditorWindow.cs`')

# ------------------------------------------------------------------ salida
count = Counter(r[0] for r in rows)
n_b = count['PB-T4b C-5'] + count['PB-T4b UI'] + count['PB-T4b C-0']
n_ret = count['PB-T4b C-7']
cause_mix = Counter()
for r in rows:
    if r[0] == 'PB-T4b C-0':
        cause_mix[r[4].split(';')[0]] += 1
hand_mix = Counter(re.search(r'a mano \(([^)]*)\)', r[4]).group(1) for r in rows if r[0] == 'PB-T4b C-0' and 'a mano' in r[4])
hand = sum(hand_mix.values())

L = ['> **Redacción para el repo público:** sin rutas locales.', '',
     '# I-64 F4B — PB-T4: las listas de sustituciones (rev `%s`)' % REV, '',
     '**Lo que eligió el Owner** (`owner-pbt4-decisions.md`): Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones». Estas son las listas de lo elegido. **Son la cota que vio el Owner:** la lista que se emite en cada subtarea es la suya o una parte de ella; una prueba de más vuelve al Owner.', '',
     '**Cómo se generó:** `lists_pbt4.py` y `lists_core.py`, con `python -I`, solo por objetos git en `%s`. Una firma se mide en el texto de la prueba (sus atributos y su cuerpo) más el de los ayudantes de su mismo archivo que nombra, a un nivel. Las pruebas que ya llevan `Skip` en la revisión no entran. Las llaves se cuentan sin el texto de cadenas ni comentarios. La misma tabla, en `pbt4-substitution-lists.tsv`.' % REV, '',
     '## Resumen', '',
     '| Subtarea | Lista | Métodos |', '|---|---|---|',
     '| PB-T4a | C-5 | **%d** |' % count['PB-T4a'],
     '| PB-T4b | C-0, las 9 clases enteras («Saltar y duplicar»): %d con copia mecánica y %d a mano | %d |' % (
         sum(1 for r in rows if r[0] == 'PB-T4b C-0' and 'copia mecánica' in r[4]),
         sum(1 for r in rows if r[0] == 'PB-T4b C-0' and 'a mano' in r[4]), count['PB-T4b C-0']),
     '| PB-T4b | C-5 y C-6, la letra de las guardas | %d |' % count['PB-T4b C-5'],
     '| PB-T4b | C-3, la UI de la elección | %d |' % count['PB-T4b UI'],
     '| PB-T4b | **Subtotal sin «Retirarlo»** (la cifra «unas 465») | **%d** |' % n_b,
     '| PB-T4b | C-7, lo que añade «Retirarlo» (la cifra «unas 100»), sin las ya contadas | **%d** |' % n_ret,
     '| PB-T4b | **Total** | **%d** |' % (n_b + n_ret),
     '| PB-T4c | C-5 | **%d** |' % count['PB-T4c'], '',
     '**Las 9 clases, por clase:** ' + '; '.join('`%s` %d' % (c, n) for c, n in per_class) + '.', '',
     '**Las causas dentro de las 9 clases** (informativas: toda la clase se salta y se duplica): ' + '; '.join('%s: %d' % (k, v) for k, v in sorted(cause_mix.items())) + '.', '',
     '**Las que no admiten copia mecánica** (las sustituye un equivalente a mano con su fila de RR-3, D-PBT4-11): **%d**, por lo que queda tras el mapa M: ' % hand
     + '; '.join('%s %d' % (k, v) for k, v in sorted(hand_mix.items(), key=lambda x: -x[1])) + '. Los motivos están en `copy_map_defs.py`.', '',
     '**«Retirarlo», por archivo:** ' + '; '.join('`%s` %d' % (p.rsplit('/', 1)[-1], n) for p, n in sorted(ret_files.items())) + '.', '']
titles = [('PB-T4a', 'PB-T4a'), ('PB-T4b C-0', 'PB-T4b — C-0: las 9 clases, «Saltar y duplicar»'),
          ('PB-T4b C-5', 'PB-T4b — C-5 y C-6: la letra de las guardas'), ('PB-T4b UI', 'PB-T4b — C-3: la UI de la elección'),
          ('PB-T4b C-7', 'PB-T4b — C-7: «Retirarlo»'), ('PB-T4c', 'PB-T4c')]
for key, title in titles:
    sel = [r for r in rows if r[0] == key]
    L += ['## %s (%d)' % (title, len(sel)), '', '| # | Archivo | Método | Líneas | Causa |', '|---|---|---|---|---|']
    for i, r in enumerate(sel, 1):
        L.append('| %d | `%s` | `%s` | %s | %s |' % (i, r[1].rsplit('/', 1)[-1], r[2], r[3], r[4]))
    L.append('')
with open(out_md, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(L))
with open(out_tsv, 'w', encoding='utf-8', newline='\n') as f:
    f.write('lista\tarchivo\tmetodo\tlineas\tcausa\n')
    for r in rows:
        f.write('\t'.join(r) + '\n')
print('PB-T4a', count['PB-T4a'], '| C-0', count['PB-T4b C-0'], 'C-5', count['PB-T4b C-5'], 'UI', count['PB-T4b UI'], 'sub', n_b,
      '| C-7', n_ret, 'total', n_b + n_ret, '| PB-T4c', count['PB-T4c'], '| hand', hand)
print('por clase', per_class)
print('causas', sorted(cause_mix.items()))
print('retirar', sorted((p.rsplit('/', 1)[-1], n) for p, n in ret_files.items()))
