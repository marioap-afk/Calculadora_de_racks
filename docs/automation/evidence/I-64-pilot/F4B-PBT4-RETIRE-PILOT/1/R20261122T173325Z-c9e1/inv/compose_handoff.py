"""Compone worker-handoff.json (cwd = worktree; WORKTREE_PATH en el entorno). Lee los resumenes y TRX de las corridas; todas las rutas son relativas."""
import json
import os
import re

REL = 'artifacts/orchestration/I-64/F4B-PBT4-RETIRE-PILOT/1/R20261122T173325Z-c9e1'
BASE = '399c9bec6d4778aa50096ab6204859c0bd015419'
RED = 'aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a'
GREEN = '7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2'
T_BASE = '9c3e4d410539970fefa7fb7e1e48cf95a4ae679d'
T_RED = '7d934138b9868a2aceecc5b4ad1537572e59d284'
T_GREEN = '9dbb148b138bbaa4551ea3c4c65bb00075a83571'
CORE = 'tests/RackCad.Tests/RackCad.Tests.csproj'
UI = 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj'
TRAILER = 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>'

executed, results = [], []


def must(p):
    assert os.path.exists(p), p
    return p


def filt_runs(runref, folder, proj, phase, tree, ref_prefix):
    summ = must('%s/tests/%s/summary.txt' % (REL, folder))
    txt = open(summ, encoding='utf-8', newline='').read().replace('\r', '')
    for chunk in re.split(r'\n(?=(?:core|ui)\d\d \|)', txt):
        m = re.match(r'(\w+) \| (.+?) \| exit=(\d+) \| .*?Con error:\s*(\d+), Superado:\s*(\d+), Omitido:\s*(\d+), Total:\s*(\d+)', chunk.replace('\n', ' '))
        if not m:
            continue
        n, f, ex, fa, pa, sk, to = m.groups()
        fa, pa, sk, to = int(fa), int(pa), int(sk), int(to)
        ref = '%s-%s' % (ref_prefix, n)
        out = '%s/tests/%s' % (REL, folder)
        cmd = '"$DOTNET" test %s --no-build -c Debug --filter "%s" --logger "trx;LogFileName=%s.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none' % (proj, f, n, out)
        trx = must('%s/%s.trx' % (out, n))
        failed = []
        if fa:
            t = open(trx, encoding='utf-8').read()
            failed = re.findall(r'<UnitTestResult [^>]*testName="([^"]+)"[^>]*outcome="Failed"', t) or re.findall(r'<UnitTestResult [^>]*outcome="Failed"[^>]*testName="([^"]+)"', t)
            if not failed:
                for r in re.findall(r'<UnitTestResult [^>]*>', t):
                    if 'outcome="Failed"' in r:
                        failed.append(re.search(r'testName="([^"]+)"', r).group(1))
        executed.append({'RunRef': ref, 'Command': cmd, 'Phase': phase, 'TreeSha': tree, 'ResultsFile': trx})
        results.append({'RunRef': ref, 'Selected': to, 'Passed': pa, 'Failed': fa, 'Skipped': sk, 'FailedTests': failed})


def full_run(ref, folder, phase, tree):
    out = '%s/tests/%s' % (REL, folder)
    trx = must(out + '/core-full.trx')
    t = open(trx, encoding='utf-8').read()
    c = re.search(r'<Counters total="(\d+)" executed="(\d+)" passed="(\d+)" failed="(\d+)"', t)
    total, ex, pa, fa = map(int, c.groups())
    failed = []
    for r in re.findall(r'<UnitTestResult [^>]*>', t):
        if 'outcome="Failed"' in r:
            failed.append(re.search(r'testName="([^"]+)"', r).group(1))
    cmd = '"$DOTNET" test %s --no-build -c Debug --logger "trx;LogFileName=core-full.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none' % (CORE, out)
    executed.append({'RunRef': ref, 'Command': cmd, 'Phase': phase, 'TreeSha': tree, 'ResultsFile': trx})
    results.append({'RunRef': ref, 'Selected': total, 'Passed': pa, 'Failed': fa, 'Skipped': total - ex, 'FailedTests': failed})


def build(ref, folder, phase, tree, kind):
    out = '%s/tests/%s' % (REL, folder)
    if kind == 'core':
        cmd = '"$DOTNET" build %s -c Release --no-incremental -v:minimal > %s/build-RackCad.Tests.log' % (CORE, out)
        log = out + '/build-RackCad.Tests.log'
    elif kind == 'ui':
        cmd = '"$DOTNET" build %s -c Release --no-incremental -v:minimal > %s/build-RackCad.UI.Tests.log' % (UI, out)
        log = out + '/build-RackCad.UI.Tests.log'
    else:
        cmd = '"$DOTNET" build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o %s/plugin-out > %s/build-RackCad.Plugin.log' % (out, out)
        log = out + '/build-RackCad.Plugin.log'
    executed.append({'RunRef': '%s-build-%s' % (ref, kind), 'Command': cmd, 'Phase': phase, 'TreeSha': tree, 'ResultsFile': must(log)})


# Linea base (BaseSha, arbol limpio)
for k in ('core', 'ui', 'plugin'):
    build('b0', 'b0', 'RELEVANT', T_BASE, k)
full_run('b0-core-full', 'b0', 'RELEVANT', T_BASE)
filt_runs('b0ui', 'b0ui', UI, 'RELEVANT', T_BASE, 'b0ui')
# RED
for k in ('core', 'ui', 'plugin'):
    build('r1', 'r1', 'RED', T_RED, k)
filt_runs('r1c', 'r1c', CORE, 'RED', T_RED, 'r1c')
filt_runs('r1u', 'r1u', UI, 'RED', T_RED, 'r1u')
full_run('r1-core-full', 'r1', 'RED', T_RED)
# GREEN
for k in ('core', 'ui', 'plugin'):
    build('g1', 'g1', 'GREEN', T_GREEN, k)
filt_runs('g1c', 'g1c', CORE, 'GREEN', T_GREEN, 'g1c')
filt_runs('g1u', 'g1u', UI, 'GREEN', T_GREEN, 'g1u')
full_run('g1-core-full', 'g1', 'GREEN', T_GREEN)
for r in ('g1r', 'g2r', 'g3r'):
    filt_runs(r, r, CORE, 'GREEN', T_GREEN, r)

assert len({e['RunRef'] for e in executed}) == len(executed)
assert len({e['RunRef'] for e in results}) == len(results)

files = [l for l in open(REL + '/inv/files-changed.txt', encoding='utf-8').read().split('\n') if l]
assert len(files) == 37

ev = [
    'Rutas relativas comprobadas (existen): ' + ', '.join(must(REL + '/' + p) for p in (
        'gen/apply-red.txt', 'gen/apply-green.txt', 'inputs/red-files.txt', 'inv/step1-output.txt', 'inv/check-inv-prefix.txt', 'inv/check-inv-final.txt',
        'inv/census-base.txt', 'inv/census-green.txt', 'inv/red-name-status.txt', 'inv/files-changed.txt', 'tests/b0/profile-before.txt', 'tests/g1/profile-after.txt')),
    'RunRefs de los filtros: <fase>-<nombre>; summary.txt por carpeta: tests/b0ui, tests/r1c, tests/r1u, tests/g1c, tests/g1u, tests/g1r, tests/g2r, tests/g3r; TRX y log de cada corrida en su carpeta.',
    'Autoridades: lista pbt4d-retire-list-271.tsv leida de AuthorityRevision 23c29236 con sha256 05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac (284 filas, comprobado); delegacion y prompt.md con sha256 c2f22906 y 127378 bytes (comprobados al empezar).',
    'Piezas copiadas con git show de BaseSha y comprobadas por sha256: apply_red_pbt4d_v53.py b99635ea, red_pbt4d_v53.py 7a67652e, pbt4d-red-v54.txt 80f8cc0c, apply_green_pbt4d_v53.py 70e7dd04, green_pbt4d_v53.py ea7b2086, pbt4d-green-v54.txt aaf6cab8; inputs/red-files.txt 4065207e (31 lineas).',
    'Paso 1 (inv/step1-output.txt, solo lectura sobre BaseSha): 119 bloques EDIT del RED y 34 del GREEN con su OLD exactamente una vez; 5 archivos nuevos no existen en BaseSha; 281 miembros/archivos de la lista presentes (las 3 observaciones son las filas de archivo entero a borrar, que existen).',
    'Herramienta del RED (gen/apply-red.txt): 23 archivos existentes escritos con los sha256 de inputs/red-files.txt, 5 nuevos y 3 borrados (PushBackDeclaredCorrectionPilotParityTests.cs, PushBackFieldValidationConcordanceTests.cs, WorkspaceDraftPilotCanonicalLineEndingTests.cs); sin archivo extra; src/ sin tocar. Compilacion de los tres proyectos tras ella: tests/r1/build-*.log, 0 errores y 0 warning CS.',
    'NewFileSha de los 5 archivos nuevos (iguales a RedFiles de la precondicion 6): PushBackRetirePilotCensusGuardTests 1874c478...3f20 (1874c47811ddda428745a1f5566f1e7290b31fa2a2e65b07c2ff52059f863f20); PushBackRetirePilotFullCopyGuardTests ee9b6f1513f549065da391c10d6da947cc8e197644efbc7d72cf41e46443e6cf; PushBackRetirePilotGuardTests 0d64806a53b36ea0719bc7647dcfd3c15e8169a66a4494517097a82ce12fadb3; PushBackRetirePilotPlanGuardTests 48c2f4a9619e2f3e7e5de4f1ad98bf68024635c268866fa1242f65f329d8ca43; PushBackRetirePilotRunTests 3554df0d960f18ef6bb2b930f4ac7757e8ca7438ebf6a9eda8934ef9acf8fe69. Los 23 existentes editados coinciden con inputs/red-files.txt en RedSha y en CurrentSha (inv/check-inv-final.txt, RED literal OK); los 3 borrados ausentes.',
    'Mapa CT -> metodo y clase: CT-PBT4D-01 = RackCad.Tests.PushBackRetirePilotGuardTests.CT_PBT4D_01_TheCodeOfThePilotThatThePanelNoLongerUsesIsRetiredAndWhatTheFullModelUsesStays (ExpectRed true); CT-PBT4D-02 = RackCad.Tests.PushBackRetirePilotCensusGuardTests.CT_PBT4D_02_NoFileOfTheTestsNamesWhatTheRetirementOfThePilotRemoves (ExpectRed false). Un [Fact] por CT, ninguna Theory nueva.',
    'Tabla CT -> la asercion que falla en el RED: CT-PBT4D-01 cae por su Assert.Equal (tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs, linea 165; primera linea del StackTrace bajo tests/; TRX tests/r1c/core66.trx; "retirado-archivo PushBackPilotStateComparer.cs existe=False" esperado contra existe=True real, porque src/ sigue con el piloto); Failed 1 == Selected 1, ningun NotExecuted ni Skipped. CT-PBT4D-02 pasa en el RED (1/1). Las autopruebas de las dos van dentro de su [Fact] sobre texto inyectado en memoria y pasan en RED y GREEN. En el GREEN las dos pasan; CT-PBT4D-01 en verde tres veces consecutivas (g1r, g2r, g3r: 1 Passed cada una).',
    'Skip (D-S12), literales en la pieza del RED, un motivo por linea de atributo: PushBackEditAllFullCopyGuardTests (CT-PBT4B-20): "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1)."; dos en WorkspacePilotPrepareGuardTests con motivo "... sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (mismo texto)". Equivalentes: PushBackRetirePilotFullCopyGuardTests 1/1, PushBackRetirePilotPlanGuardTests 2/2, PushBackRetirePilotRunTests 31/31 (27 de la corrida + 2 de PushBackPilotSystemRebuild; una Theory de 3), en RED y en GREEN (ExpectRed false, pasan en las dos ramas).',
    'Totales exactos locales, Core suite completa en Debug (TRX en tests/b0, tests/r1, tests/g1): linea base 19619 = 19158 Passed + 0 Failed + 461 Skipped; RED 19418 = 19014 Passed + 1 Failed (CT-PBT4D-01) + 403 Skipped; GREEN 19418 = 19015 Passed + 0 Failed + 403 Skipped. UI: solo los 18 filtros del contrato, uno a uno (nunca la suite completa en local): linea base y RED y GREEN, todos con Failed 0 y Passed >= MinSelected (tests/b0ui, r1u, g1u); la suite completa de UI se midio solo en la CI.',
    'Los 70 filtros de Core del RequiredTests y los 18 de UI, uno a uno, en RED (tests/r1c, tests/r1u: Failed 0 y Passed >= MinSelected salvo PushBackRetirePilotGuardTests Failed 1 == Selected 1) y en GREEN (tests/g1c, tests/g1u: 70 y 18 sin Failed y con Passed >= MinSelected; evaluados con inv/eval_filters.py). Cada corrida filtrada demuestra Selected > 0 (Selected == Total).',
    'Builds Release por proyectos con log completo: linea base (tests/b0), RED (tests/r1) y GREEN (tests/g1): 0 errores y 0 lineas warning CS en todos los registros; conteos de avisos por codigo identicos a la linea base (Core: xUnit1026 20, xUnit2000 6, xUnit2010 4, xUnit2012 2, xUnit2013 34, xUnit2017 2, xUnit2029 32, xUnit2031 32; UI: xUnit1026 2, xUnit2013 4, xUnit2029 4, xUnit2030 2, xUnit2031 16; Plugin: MSB3277 104, de las referencias de AutoCAD).',
    'CI del RED: run 45054587789 sobre RedSha aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a: tests failure (solo CT-PBT4D-01), ui-tests success, build-ui success, build-plugin omitido (skipped), sin cuelgue; TRX tests/redci/core/core.trx: Core 19418 = 19014 Passed + 1 Failed + 403 Skipped; tests/redci/ui/ui.trx: UI 2464 = 2416 Passed + 0 Failed + 48 Skipped. Sin reparacion del RED (RED repair no usado).',
    'CI del GREEN: run 45057868915 sobre CurrentSha 7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2: 4 de 4 jobs en success (tests, ui-tests, build-ui y build-plugin compilado); TRX tests/greenci/core/core.trx: Core 19418 = 19015 Passed + 0 Failed + 403 Skipped; tests/greenci/ui/ui.trx: UI 2464 = 2416 Passed + 0 Failed + 48 Skipped.',
    'Herramienta del GREEN (gen/apply-green.txt): los sha256 de DraftState a545dbff, Materializer 7b7f2fb4, UpdatePlan f70febaa y UpdateRun 5c936519 coinciden con el contrato; PushBackPilotStateComparer.cs y PushBackPilotFieldAdapter.cs borrados; seis archivos, 4 M y 2 D; commit GREEN solo con src/.',
    'INV-PBT4D-01..14 (inv/check-inv-prefix.txt sobre el arbol del GREEN antes del commit y inv/check-inv-final.txt sobre git diff BaseSha..CurrentSha, TODO OK): desde RedSha solo los seis de Pilot/ y nada de tests/; Domain, UI, Plugin y los cinco archivos nombrados vacios; src/ con exactamente los seis (4 M, 2 D); tests/ cambiados desde BaseSha = los 31 del RED, con CRLF normalizado a LF y el sha256 de inputs/red-files.txt, los 3 borrados ausentes; allowErase no aparece en src/; 0 lineas warning CS; en las 37 lineas anadidas de src/ solo 5 no son comentario (condiciones de BuildCore que dejan de nombrar allowErase) y ninguna nombra PushBackSystem ni RackModuleReconciliationResult (D2, PBT4D-C17, sin mutacion nueva).',
    'Censo de declaraciones de PBT4D-C13 (inv/census.py; una a una con su nombre y su firma; una sobrecarga es una mas): seis archivos de Pilot/ en BaseSha 247 declaraciones y en CurrentSha 190; anadidas 0; quitadas 57, exactamente la lista cerrada: PushBackPilotStateComparer entero con StructuralEquality (22), PushBackPilotFieldAdapter entero (8), ComposeForWrite, KeepPilotEditsOn y PushBackPilotComposition con sus 7 miembros (10), PushBackPilotPrepareInput con sus 9 miembros (10), Prepare, Build, ViewUnavailablePrefix y ViewUnavailableSuffix (4), Precheck(DraftSession<PushBackPilotDraftState>), Execute y ComparePost (3). Total 22+8+10+10+4+3 = 57.',
    'Prueba de limpieza (comando en tests/b0/profile-probe-command.txt, sin rutas): antes (tests/b0/profile-before.txt) y despues (tests/g1/profile-after.txt) identicas: disposicion del configurador con el mismo sha256 y la misma fecha UTC de modificacion; carpeta de biblioteca con la misma fecha UTC y el mismo sha256 de su listado recursivo ordenado (11 entradas).',
    'Trailer real de los commits (git log --format=%B BaseSha..HEAD): RED aa65ecfd y GREEN 7db6fd0c, los dos con "' + TRAILER + '"; sin RED repair. Cuerpo del commit GREEN el exigido por el contrato (attempts = 1).',
    '$DOTNET en los Command = dotnet.exe del SDK 8.0.423 de nivel de usuario indicado por la delegacion (el dotnet del PATH no sirve).',
]

handoff = {
    'Schema': 'rackcad-worker-handoff/v1',
    'TaskId': 'F4B-PBT4-RETIRE-PILOT',
    'RunId': 'R20261122T173325Z-c9e1',
    'DelegationRunId': 'R20261122T172759Z-039a',
    'Initiative': 'I-64',
    'Gate': 'F4B',
    'Attempt': 1,
    'BaseSha': BASE,
    'RedSha': RED,
    'CurrentSha': GREEN,
    'Branch': 'architecture/workspace-persistente-rackcad',
    'Worktree': os.environ['WORKTREE_PATH'],
    'Pushed': True,
    'FilesChanged': files,
    'WorkCompleted': 'PB-T4d aplicada al pie de la letra: RED = pieza literal pbt4d-red-v54 (31 archivos de tests/: 23 editados, 3 borrados, 5 nuevos; src/ sin cambio) con CT-PBT4D-01 fallando por asercion y CI del RED exacta; GREEN = pieza literal pbt4d-green-v54 (4 archivos de src/ cambiados y 2 borrados en Pilot/) con CI 4/4 y totales exactos. Core 19418 = 19015 + 0 + 403 y UI 2464 = 2416 + 0 + 48 en la CI del GREEN. Sin RED repair, sin paradas, sin cambios de comportamiento, sin AutoCAD, sin escribir en docs/.',
    'TestsExecuted': executed,
    'TestResults': results,
    'Evidence': ev,
    'UnexpectedFindings': [
        'Ninguno de importancia. La suite completa de Core tarda unos 12 min en la linea base (12 m 23 s) y 5 min en RED y GREEN; la herramienta de ejecucion movio por si misma las corridas largas a segundo plano y se esperaron hasta su terminacion (sin lanzar procesos propios en segundo plano ni subagentes).',
        'Los 3 archivos borrados no dejan referencias; la prueba de limpieza no cambio ni el configurador ni la biblioteca.',
    ],
    'KnownLimitations': [
        'No se ejecuto AutoCAD: lo que depende de un dibujo real (RACKEDITAR, panel y ventana contra un DWG, cero escrituras en el DWG observadas) queda para el Smoke-PB.',
        'La recarga de Editar todo sobre un borrador limpio posiblemente obsoleto queda para su tarea propia posterior (owner-pbt4d-list-decisions.md, pregunta 2); no se toco.',
        'La suite completa de UI no se corrio en local (regla): su total (2464 = 2416 + 0 + 48) se midio solo en la CI de RED y de GREEN; en local solo los 18 filtros de UI del contrato.',
        'La cobertura linea a linea y rama a rama de INV-PBT4D-06 es la medida de la precondicion 6 (coverlet) de la sesion; el Worker no la volvio a medir y ninguna prueba existente fuera de la lista cayo en la suite completa.',
        'No se puede observar el uso del equipo mientras corren las pruebas; durante ellas no se hizo nada interactivo.',
    ],
    'Deviations': [],
    'OpenQuestions': [],
    'WorkerStatus': 'IMPLEMENTATION_COMPLETE',
    'Disposition': 'NONE',
    'TriggeredStopConditions': [],
    'RecommendedNextAction': 'La sesion ejecuta la verificacion con las tres comprobaciones deterministas de F4B-E15 y nc1 a nc3, y mantiene la evidencia y el estado.',
    'Worker': {'Provider': 'Anthropic', 'ModelRequested': 'claude-sonnet-5-5', 'EffortRequested': 'high', 'Trailer': TRAILER},
}
with open(REL + '/worker-handoff.json', 'w', encoding='utf-8', newline='\n') as f:
    json.dump(handoff, f, ensure_ascii=False, indent=2)
    f.write('\n')
print('escrito: %d TestsExecuted, %d TestResults' % (len(executed), len(results)))
