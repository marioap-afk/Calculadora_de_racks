"""Validacion determinista del paquete de F4-T4B (v4 a v7) contra la rama posterior a F4-T1B.
Uso (desde la raiz del worktree, con el paquete junto al script): python validate_t4b_package.py <salida.json>"""
import json, os, re, subprocess, sys
import xml.etree.ElementTree as ET
H = os.path.dirname(os.path.abspath(__file__))
WT = os.path.abspath(os.environ.get('RACKCAD_WT', '.'))  # la raiz del worktree; por defecto, el directorio actual
HEAD = '7c29a5e55ce623b52ac240665cdf4e795d817d9f'
BASE_PRODUCT = 'c3ae1c97'
PKG = open(os.path.join(H, 'I-64-f4-t4b-gate-contract-package.md'), encoding='utf-8').read()
TRX = os.path.join(WT, 'artifacts', 'orchestration', 'I-64', 'F4-T1B-PILOT-CHARACTERIZATION', '1', 'R20261114T214644Z-c077', 'ci-core', 'core.trx')
TRX_UI = TRX.replace('ci-core', 'ci-ui').replace('core.trx', 'ui.trx')


def git(*a):
    p = subprocess.run(['git', '--no-optional-locks', '-C', WT] + list(a), capture_output=True, text=True, encoding='utf-8')
    return p.returncode, p.stdout


def exists(path):
    return git('cat-file', '-e', f'{HEAD}:{path}')[0] == 0


def lines(path, a, b):
    rc, out = git('show', f'{HEAD}:{path}')
    assert rc == 0, path
    ls = out.splitlines()
    return '\n'.join(ls[a - 1:b])


R = {'Head': HEAD, 'Checks': []}


def check(name, ok, detail=''):
    R['Checks'].append({'Check': name, 'Result': 'PASS' if ok else 'FAIL', 'Detail': detail})


assert git('rev-parse', 'HEAD')[1].strip() == HEAD
# 1. Producto identico a la base medida; solo los 7 archivos de T1B en tests/
src_diff = git('diff', '--name-only', BASE_PRODUCT, HEAD, '--', 'src')[1].split()
tests_diff = git('diff', '--name-only', BASE_PRODUCT, HEAD, '--', 'tests')[1].split()
check('1 producto identico a c3ae1c97', src_diff == [], str(src_diff))
check('1 tests/ solo cambia en las 7 clases de T1B', len(tests_diff) == 7 and all('PushBackClassicCharacterization' in p for p in tests_diff), str(tests_diff))
# 2. Alcance: nuevos ausentes, modificados presentes, raices prohibidas presentes
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'
TW = 'tests/RackCad.Tests/Workspace/'
new = [P + 'IPushBackPilotUpdatePort.cs', P + 'PushBackPilotUpdateRun.cs', P + 'PushBackPilotSystemRebuild.cs', 'src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs',
       TW + 'WorkspacePilotUpdateRunTests.cs', TW + 'WorkspacePilotUpdateHostTests.cs', TW + 'WorkspacePilotUpdateGuardTests.cs', TW + 'WorkspacePilotUpdateScopeDouble.cs']
mod = [P + 'PushBackPilotUpdatePlan.cs', 'src/RackCad.Application/Workspace/Drafts/DraftSession.cs', 'src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs',
       'src/RackCad.Application/Workspace/WorkspaceHostCore.cs', 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs', 'src/RackCad.Plugin/Views/RackSiblingScan.cs',
       'tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs', TW + 'WorkspaceDraftPilotOwnershipTests.cs', TW + 'WorkspacePilotPrepareTests.cs', TW + 'WorkspacePilotPrepareGuardTests.cs',
       TW + 'WorkspaceLifecycleGuardTests.cs', TW + 'WorkspacePilotEditGuardTests.cs', TW + 'WorkspaceEditIntentGuardTests.cs', TW + 'WorkspaceEditIntentHostTests.cs', TW + 'WorkspaceHostTestSupport.cs']
check('2 los archivos nuevos no existen', all(not exists(p) for p in new), str([p for p in new if exists(p)]))
check('2 los archivos modificados existen', all(exists(p) for p in mod), str([p for p in mod if not exists(p)]))
forb = ['src/RackCad.UI/RackCad.UI.csproj', 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', 'src/RackCad.Plugin/Workspace/WorkspaceController.cs', 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs',
        P + 'PushBackPilotMaterializer.cs', 'src/RackCad.Plugin/RackSelectivoInsertIntegration.cs', 'src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs', 'src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs']
check('2 las rutas del prohibido citadas existen', all(exists(p) for p in forb), str([p for p in forb if not exists(p)]))
for p in mod + new:
    check('2 la ruta aparece en el paquete: ' + p, p.split('/')[-1] in PKG)
# 3. RequiredTests: clases unicas y cuentas de la base (CI 43650040527)
ns = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'


def results(path):
    root = ET.parse(path).getroot()
    defs = {u.get('id'): u.find(ns + 'TestMethod').get('className') + '.' + u.find(ns + 'TestMethod').get('name') for u in root.iter(ns + 'UnitTest')}
    return [defs[r.get('testId')] for r in root.iter(ns + 'UnitTestResult')]


core, ui = results(TRX), results(TRX_UI)
cnt = lambda res, f: sum(1 for x in res if f.lower() in x.lower())
table = dict(re.findall(r'`([A-Za-z0-9]+Tests)`(?: \(v4\))? \| (\d+)', PKG))
check('3 la tabla de clases tiene 21 filas', len(table) == 21, str(sorted(table)))
for c, n in sorted(table.items()):
    hits = [l for l in git('grep', '-n', '-E', r'class ' + c + r'\b', HEAD, '--', 'tests/')[1].splitlines() if l]
    m = cnt(core, 'RackCad.Tests.' + c)
    check(f'3 {c}: una clase y cuenta {n}', len(hits) == 1 and m == int(n), f'clases={len(hits)} cuenta_ci={m}')
check('3 Core Workspace 2923', cnt(core, 'RackCad.Tests.Workspace') == 2923 and '**2923** (el suelo de la base)' in PKG)
check('3 UI Workspace 601', cnt(ui, 'RackCad.UI.Tests.Workspace') == 601)
check('3 UI PushBackClassicCharacterization 55', cnt(ui, 'RackCad.UI.Tests.PushBackClassicCharacterization') == 55)
# 4. Anclas nuevas de la v4: el texto citado existe en esas lineas
A = [('src/RackCad.Plugin/Views/RackSiblingScan.cs', 112, 112, 'LockDocument'), ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 113, 113, 'StartTransaction'),
     ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 163, 163, '.Commit('), ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 98, 98, 'Traverse('),
     ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 103, 110, 'StringComparer.Ordinal'), ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 166, 169, 'new RackSiblingDrawingScan('),
     ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 119, 122, 'IsLayout'), ('src/RackCad.Plugin/RackSelectivoInsertIntegration.cs', 81, 81, 'RackSiblingScan.Capture('),
     ('src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs', 43, 43, 'RackSiblingScan.Capture('), ('src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs', 127, 127, 'RackSiblingScan.CaptureDrawing('),
     ('src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs', 116, 116, 'RackSiblingScan.CaptureDrawing('),
     ('src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs', 147, 150, 'PurgeAfterCommit'), ('src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs', 193, 238, 'PurgeUnreferenced'),
     ('src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs', 202, 202, 'StartTransaction'), ('src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs', 220, 220, 'Purge('),
     ('src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs', 233, 237, 'catch'), ('src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs', 87, 87, 'Commit'),
     ('src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs', 91, 91, 'Purge'),
     ('src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs', 64, 66, 'RedrawInTransaction'), ('src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs', 62, 64, 'RedrawInTransaction'),
     ('src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs', 56, 58, 'RedrawInTransaction'),
     ('tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs', 197, 238, 'Purge'), ('tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs', 322, 354, 'PrepareRedraw('),
     ('tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs', 366, 382, 'Lacks(source, "PrepareRedraw("'), ('tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs', 88, 128, 'Traverse'),
     ('tests/RackCad.Tests/G9bInsertWiringGuardTests.cs', 7, 35, 'SingleScanInsertGuardTests'),
     ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 77, 77, 'internal static class RackSiblingScan'), ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 172, 179, 'AddLayer'),
     ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 115, 115, 'document.Database.BlockTableId'), ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 111, 114, 'StartTransaction'),
     ('src/RackCad.Plugin/Views/RackSiblingScan.cs', 162, 165, 'transaction.Commit();'), ('src/RackCad.Application/BlockNaming.cs', 1, 40, 'SanitizeBlockName'),
     ('src/RackCad.Application/Drawing/HeaderInstanceGrouper.cs', 96, 96, '_'), ('src/RackCad.Plugin/Drawing/BlockPlacement.cs', 213, 213, 'PurgeUnreferenced'),
     ('src/RackCad.Plugin/RackCommandSupport.cs', 274, 274, 'PurgeUnreferenced'), ('src/RackCad.Plugin/Systems/Shared/SiblingRedrawTransaction.cs', 23, 23, 'Commit'),
     ('src/RackCad.Plugin/RackSelectivoInsertIntegration.cs', 253, 253, 'Purge'), ('tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs', 24, 34, 'ConstructionsOutsideComposer'),
     ('tests/RackCad.Tests/SelectiveEditorOpenTests.cs', 565, 572, 'CommandMethod'),
     ('src/RackCad.Plugin/RackPushBackCommands.cs', 189, 189, 'name.Trim()'), ('src/RackCad.Plugin/RackPushBackCommands.cs', 282, 282, 'LinkedPlanta(baseName)'),
     ('src/RackCad.Plugin/RackPushBackCommands.cs', 300, 309, 'IsComposite'), ('src/RackCad.Plugin/RackPushBackCommands.cs', 339, 342, 'LinkedLateral(baseName, postIndex)'),
     ('src/RackCad.Application/BlockNaming.cs', 12, 17, 'Cabecera'), ('tests/RackCad.Tests/RackUnitsGuardSourceTests.cs', 87, 101, 'Insunits')]
for p, a, b, tok in A:
    ok = exists(p) and tok in lines(p, a, b)
    check(f'4 {p}:{a}-{b} contiene {tok}', ok)
# 5. Toda ruta src/ o tests/ citada en el paquete existe (salvo las nuevas declaradas)
cited = sorted(set(re.findall(r'`((?:src|tests)/[A-Za-z0-9_./{},*-]+?\.(?:cs|xaml|yml|csproj|json))', PKG)))
missing = [c for c in cited if '{' not in c and '*' not in c and c not in new and not exists(c)]
check('5 rutas citadas existentes', missing == [], str(missing))
R['Summary'] = {'Pass': sum(1 for c in R['Checks'] if c['Result'] == 'PASS'), 'Fail': sum(1 for c in R['Checks'] if c['Result'] == 'FAIL')}
R['Result'] = 'PASS' if R['Summary']['Fail'] == 0 else 'FAIL'
json.dump(R, open(sys.argv[1], 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=2)
print(R['Result'], R['Summary'])
assert 'internal static RackSiblingDrawingScan TraverseCore' in PKG and 'Post(PushBackPilotPostRequest request)' in PKG and 'ABORT_FAILED' in PKG and all(x not in PKG.split('## 10. Disposición de las revisiones')[0] for x in ('TopTransactionAtPost', 'using (var m = start.Scope)', 'using (var scope = port.Begin'))
for c in R['Checks']:
    if c['Result'] == 'FAIL':
        print('FAIL', c['Check'], c['Detail'][:300])
