"""PB-T4, paquete v2: extractos y busquedas de solo lectura sobre una revision (por defecto 7dfc7c21).

Uso: python -I measure_pbt4_v2.py <checkout principal> <rev> <salida.txt>
Solo llama a `git -C <checkout> show|grep|ls-tree` (objetos git); no lee el disco del checkout ni ningun worktree, y escribe solo <salida.txt>.
Cada extracto lleva su ruta y sus lineas; las lineas en blanco se omiten y cada linea se corta a 200 caracteres.
"""
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]
out = []


def git(*args, ok=(0,)):
    p = subprocess.run(['git', '-C', repo] + list(args), capture_output=True)
    if p.returncode not in ok:
        raise SystemExit('git %s -> %d: %s' % (' '.join(args), p.returncode, p.stderr.decode('utf-8', 'replace')))
    return p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')


def find(name):
    hits = [p for p in git('ls-tree', '-r', '--name-only', rev, 'src').splitlines() if p.endswith('/' + name) or p == name]
    return hits[0] if hits else None


def excerpt(tag, path, a, b):
    out.append('')
    out.append('### %s — %s:%d-%d' % (tag, path, a, b))
    if path is None:
        out.append('    (no existe)')
        return
    lines = git('show', '%s:%s' % (rev, path)).split('\n')
    for i in range(a - 1, min(b, len(lines))):
        if lines[i].strip():
            out.append('%5d %s' % (i + 1, lines[i].rstrip()[:200]))


def grep(tag, pattern, paths, limit=40):
    out.append('')
    out.append('### %s — grep -E %r en %s' % (tag, pattern, ' '.join(paths)))
    res = git('grep', '-n', '-E', '-e', pattern, rev, '--', *paths, ok=(0, 1)).splitlines()
    out.append('    %d coincidencias' % len(res))
    for l in res[:limit]:
        out.append('    ' + l[len(rev) + 1:][:200])


WS = 'src/RackCad.Application/Workspace/'
TW = 'tests/RackCad.Tests/Workspace/'
UTW = 'tests/RackCad.UI.Tests/Workspace/'
WHC = WS + 'WorkspaceHostCore.cs'

# B-01: el camino de lectura
excerpt('B01-a resultado de la lectura', WS + 'WorkspaceEditIntent.cs', 1, 120)
excerpt('B01-b eleccion del origen', WS + 'WorkspaceEditIntent.cs', 670, 700)
excerpt('B01-c el lector (interfaz)', find('IWorkspaceEditIntentReader.cs'), 1, 40)
excerpt('B01-d anfitrion de navegacion: la lectura', find('WorkspaceNavigationHost.cs'), 195, 260)
excerpt('B01-e adaptador de la intencion', find('RackPilotIntentAdapter.cs'), 1, 160)
excerpt('B01-f el resultado del piloto', 'src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs', 1, 80)
grep('B01-g PreviewWitness y PilotRead en WHC', r'PreviewWitness|\.PilotRead\b|ReadIntent\s*\(', [WHC])
grep('B01-h FromPilot en tests', r'FromPilot\s*\(', ['tests'])
excerpt('B01-i kit de lecturas de EIH', TW + 'WorkspaceEditIntentHostTests.cs', 40, 60)
excerpt('B01-j forma del resultado (EIH)', TW + 'WorkspaceEditIntentHostTests.cs', 1080, 1100)
excerpt('B01-k forma del resultado (EIH)', TW + 'WorkspaceEditIntentHostTests.cs', 1170, 1197)
excerpt('B01-l guarda de lecturas del anfitrion', TW + 'WorkspaceEditIntentGuardTests.cs', 725, 740)
excerpt('B01-m guarda del cableado', TW + 'WorkspaceEditIntentGuardTests.cs', 1150, 1190)
grep('B01-n guardas que fijan el adaptador o el anfitrion de navegacion', r'RackPilotIntentAdapter|NavigationHostPath\s*=|IntentAdapterPath\s*=', ['tests'])
grep('B01-o PushBackFullDraftReader.Read y su firma', r'public static PushBackFullIntentResult Read|PushBackPilotIntentReader\.Read', ['src'])

# I-01: K = 1
excerpt('I01-a operaciones del campo', WHC, 1118, 1185)
excerpt('I01-b autorizacion y modal', WHC, 240, 260)
excerpt('I01-c marca de lectura', WHC, 1012, 1035)
excerpt('I01-d PassGate y UpdateGate', WHC, 990, 1000)
excerpt('I01-e PassGate', WHC, 1936, 1945)
excerpt('I01-f estados del campo y fronteras', WS + 'IWorkspacePilotEditPort.cs', 120, 195)

# I-02: guardas de los miembros nuevos
excerpt('I02-a PEG operaciones del puerto', TW + 'WorkspacePilotEditGuardTests.cs', 40, 95)
excerpt('I02-b PEG reflexion', TW + 'WorkspacePilotEditGuardTests.cs', 150, 166)
excerpt('I02-c PEG reglas del anfitrion', TW + 'WorkspacePilotEditGuardTests.cs', 262, 290)
excerpt('I02-d PEG interfaces', TW + 'WorkspacePilotEditGuardTests.cs', 375, 390)
excerpt('I02-e nombres del enumerado', TW + 'WorkspacePilotUpdateRequestTests.cs', 1100, 1112)
excerpt('I02-f CommitField como frontera', WHC, 1140, 1160)
excerpt('I02-g Mutate anclado', TW + 'WorkspacePilotReconciliationGuardTests.cs', 105, 125)
excerpt('I02-h UI: la eleccion solo lee memoria', UTW + 'WorkspacePilotReconciliationTests.cs', 770, 800)
grep('I02-i UI: RequestKeepMine y ConfirmChoice', r'RequestKeepMine\s*\(|ConfirmChoice\s*\(', [UTW])

# I-07: NaN
excerpt('I07-a SameAs', find('RackModuleEditSession.cs'), 70, 140)
excerpt('I07-b comparador', 'src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs', 400, 412)

# B-02: la regla
grep('B02-a referencias de proyecto de las pruebas', r'ProjectReference', ['tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', 'tests/RackCad.Tests/RackCad.Tests.csproj'])
grep('B02-b propiedades de NumericField', r'public\s+[\w?<>]+\s+(Minimum|Maximum|MinimumInclusive|MaximumInclusive|IsOptional|IntegerOnly)\b|Validate\s*\(', ['src/RackCad.UI/Controls'])
excerpt('B02-c la caja de la escala', 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml', 66, 72)
grep('B02-d otras cajas con las mismas reglas', r'AnnotationScale', ['src/RackCad.UI'])

# M-09, M-12
excerpt('M09 el abridor clasico', 'tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs', 196, 210)
excerpt('M12 la CI de UI', '.github/workflows/ci.yml', 205, 225)

# I-05: A-5, R-T1-17, A-9
excerpt('I05-a PB-T1 R-T1-17', 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md', 1362, 1370)
excerpt('I05-b PB-T1 A-5 a A-10', 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md', 1545, 1556)
excerpt('I05-c D-1', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md', 5, 27)

# lo que usa el piloto fuera del panel
grep('Q1-a quien usa el codigo del piloto en src', r'PushBackPilotMaterializer\.|PushBackPilotStateComparer|PushBackPilotFieldAdapter|PushBackPilotUpdatePlanner\.Prepare\b|PushBackPilotUpdateRun\.(Execute|Precheck)\b', ['src'])

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('# measure_pbt4_v2: salida (rev %s)\n' % rev)
    f.write('\n'.join(out) + '\n')
print('lineas', len(out))
