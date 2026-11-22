"""Compone worker-handoff.json de F4B-PBT4-EDIT-ALL-WINDOW desde los TRX y los registros guardados (solo lee; escribe la entrega).

Uso (desde la raiz del worktree): python -I make_handoff.py <ruta-del-worktree>
"""
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
WORKTREE = sys.argv[1]
RUN = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205731Z-41db'
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
D = '<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe'
BASE = 'd502601477c21972ca2109b0d5c25c6fddfe4989'
RED = '280c79beac61d31b03f86488c03686eef4f694bc'
CUR = '7cc1076eae8159294a1498ab556851977223b64c'
TRAILER = 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>'
CORE = 'tests/RackCad.Tests/RackCad.Tests.csproj'
UI = 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj'

executed, results = [], []


def trx_counts(path):
    rs = ET.parse(path).getroot().findall('.//t:UnitTestResult', NS)
    failed = sorted(r.get('testName') for r in rs if r.get('outcome') == 'Failed')
    return (len(rs), sum(1 for r in rs if r.get('outcome') == 'Passed'), len(failed), sum(1 for r in rs if r.get('outcome') == 'NotExecuted'), failed)


def test_run(ref, proj, flt, dirn, trx, phase, nobuild=True, timeout='10m'):
    cmd = '%s test %s %s--filter "%s" --logger "trx;LogFileName=%s" --results-directory %s/tests/%s --blame-hang-timeout %s --blame-hang-dump-type none' % (
        D, proj, '--no-build ' if nobuild else '', flt, trx, RUN, dirn, timeout)
    rel = '%s/tests/%s/%s' % (RUN, dirn, trx)
    assert os.path.exists(rel), rel
    executed.append({'RunRef': ref, 'Command': cmd, 'Phase': phase, 'TreeSha': None, 'ResultsFile': rel})
    sel, p, f, s, failed = trx_counts(rel)
    results.append({'RunRef': ref, 'Selected': sel, 'Passed': p, 'Failed': f, 'Skipped': s, 'FailedTests': failed})


def build_run(ref, kind, dirn, phase):
    if kind == 'core':
        cmd = '%s build %s -c Release --no-incremental -v:minimal > %s/tests/%s/build-core.log 2>&1' % (D, CORE, RUN, dirn)
    elif kind == 'ui':
        cmd = '%s build %s -c Release --no-incremental -v:minimal > %s/tests/%s/build-ui.log 2>&1' % (D, UI, RUN, dirn)
    else:
        cmd = ('%s build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o %s/tests/%s/plugin-out > %s/tests/%s/build-plugin.log 2>&1'
               % (D, RUN, dirn, RUN, dirn))
    rel = '%s/tests/%s/build-%s.log' % (RUN, dirn, kind)
    assert os.path.exists(rel), rel
    executed.append({'RunRef': ref, 'Command': cmd, 'Phase': phase, 'TreeSha': None, 'ResultsFile': rel})


WS = 'FullyQualifiedName~RackCad.Tests.Workspace'
REST = 'FullyQualifiedName!~RackCad.Tests.Workspace'
REST2 = 'FullyQualifiedName!~RackCad.Tests.Workspace&FullyQualifiedName!~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests'
PREP = 'FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests'
ui_filters = [l.strip() for l in open('%s/tests/r3/ui-filters.txt' % RUN, encoding='utf-8') if l.strip()]
core_classes = ['PushBackEditAllTests', 'PushBackEditAllGuardTests', 'PushBackOpenerClassicPathGuardTests', 'PushBackDraftLaunchGuardTests',
                'PushBackEditAllPortEquivalenceGuardTests', 'PushBackEditAllNameWritersGuardTests', 'PushBackEditAllFullCopyGuardTests']
ui_classes = ['PushBackEditAllWindowTests', 'PushBackEditAllApplyCloseTests']

# --- linea base sobre BaseSha (Paso 0)
for k in ('core', 'ui', 'plugin'):
    build_run('b0-build-' + k, k, 'b0', 'RELEVANT')
test_run('b1-core-workspace', CORE, WS, 'b1', 'core-ws.trx', 'RELEVANT', nobuild=False)
test_run('b1-core-resto', CORE, REST, 'b1', 'core-rest.trx', 'RELEVANT')
for i, f in enumerate(ui_filters[:17], 1):
    test_run('b2-ui-%02d' % i, UI, 'FullyQualifiedName~' + f, 'b2', 'ui-%d.trx' % i, 'RELEVANT', nobuild=(i > 1))
# --- el RED
for k in ('core', 'ui', 'plugin'):
    build_run('s1-build-' + k, k, 's1', 'RED')
for k in ('core', 'ui', 'plugin'):
    build_run('r1-build-' + k, k, 'r1', 'RED')
for c in core_classes:
    test_run('d0-core-' + c, CORE, 'FullyQualifiedName~RackCad.Tests.' + c, 'd0', c + '.trx', 'RED')
for c in ui_classes:
    test_run('d0-ui-' + c, UI, 'FullyQualifiedName~RackCad.UI.Tests.' + c, 'd0', c + '.trx', 'RED')
test_run('r2-core-workspace', CORE, WS, 'r2', 'core-ws.trx', 'RED')
test_run('r2-core-resto-abortada-1', CORE, REST, 'r2', 'core-rest.trx', 'RED')
test_run('r2b-core-resto-abortada-2', CORE, REST, 'r2b', 'core-rest.trx', 'RED')
test_run('r2c-core-preparegreen-solo', CORE, PREP, 'r2c', 'prep-green.trx', 'RED', timeout='20m')
test_run('r2d-core-resto-sin-preparegreen', CORE, REST2, 'r2d', 'core-rest2.trx', 'RED')
for i, f in enumerate(ui_filters, 1):
    test_run('r3-ui-%02d' % i, UI, 'FullyQualifiedName~' + f, 'r3', 'ui-%d.trx' % i, 'RED')
# --- el GREEN
for c in core_classes:
    test_run('g0-core-' + c, CORE, 'FullyQualifiedName~RackCad.Tests.' + c, 'g0', c + '.trx', 'GREEN')
for c in ui_classes:
    test_run('g0-ui-' + c, UI, 'FullyQualifiedName~RackCad.UI.Tests.' + c, 'g0', c + '.trx', 'GREEN')
for k in ('core', 'ui', 'plugin'):
    build_run('g1-build-' + k, k, 'g1', 'GREEN')
test_run('g2-core-workspace', CORE, WS, 'g2', 'core-ws.trx', 'GREEN')
test_run('g2-core-resto-sin-preparegreen', CORE, REST2, 'g2', 'core-rest.trx', 'GREEN')
test_run('g2-core-preparegreen-solo', CORE, PREP, 'g2', 'core-prep.trx', 'GREEN', timeout='30m')
for i, f in enumerate(ui_filters, 1):
    test_run('g3-ui-%02d' % i, UI, 'FullyQualifiedName~' + f, 'g3', 'ui-%d.trx' % i, 'GREEN')
for n in (1, 2, 3):
    for c in core_classes[:2]:
        test_run('e%d-core-%s' % (n, c), CORE, 'FullyQualifiedName~RackCad.Tests.' + c, 'e%d' % n, 'RackCad.Tests.%s.trx' % c, 'GREEN')
    for c in ui_classes:
        test_run('e%d-ui-%s' % (n, c), UI, 'FullyQualifiedName~RackCad.UI.Tests.' + c, 'e%d' % n, 'RackCad.UI.Tests.%s.trx' % c, 'GREEN')

# --- tabla CT -> asercion que falla en el RED (de los TRX locales del RED, d0)
ct_methods = {
    '01': ('PushBackEditAllGuardTests', 'CT_PBT4C_01_TheEditAllButtonIsInThePanelOfTheFieldAfterEditWithItsFourAttributesAndItsHandlerIsOneContainedCall', 'Core'),
    '02': ('PushBackEditAllWindowTests', 'CT_PBT4C_02_EditAllCommitsThePendingTextFirstAndOnlyOpensTheEditorWhenTheTextIsAppliedOrTheFieldIsNotEditable', 'UI'),
    '03': ('PushBackEditAllTests', 'CT_PBT4C_03_TheHostBeginsTheDraftOpensTheEditorOnlyOverADraftThatAdmitsItAndAnswersTheOtherCasesWithoutOpeningIt', 'Core'),
    '04': ('PushBackEditAllTests', 'CT_PBT4C_04_TheLaunchCarriesTheDraftAndTheOriginAndThePortHandsEveryEditToTheDraftKeepingTheOrigin', 'Core'),
    '05': ('PushBackEditAllTests', 'CT_PBT4C_05_WhileTheEditorIsOpenEveryOperationOfThePortAnswersBusyOrWaitingOrFromMemoryAndNoneReadsTheDrawing', 'Core'),
    '06': ('PushBackEditAllWindowTests', 'CT_PBT4C_06_WhenTheEditorClosesTheFieldShowsTheSummaryAndModifiedAndTheBrowserIsToldOnce', 'UI'),
    '07': ('PushBackOpenerClassicPathGuardTests', 'CT_PBT4C_07_NoneOfTheSixSubjectsOfThePathOfEditAllNamesTheClassicPathTheInsertionOrTheDrawing', 'Core'),
    '08': ('PushBackEditAllGuardTests', 'CT_PBT4C_08_TheTwoPiecesOfTheOpenerAndTheWiringOfTheModalAreTheOnesOfThePackage', 'Core'),
    '09': ('PushBackEditAllWindowTests', 'CT_PBT4C_09_ClosingInWorkspaceAsksOneQuestionWhenAStagedModuleAndUnappliedChangesWouldBeLostTogether', 'UI'),
    '10': ('PushBackEditAllTests', 'CT_PBT4C_10_FromThePortToTheUpdateWithoutAutoCadTheEditThatTheEditorHandsToTheDraftIsTheOneThatUpdateWrites', 'Core'),
    '11': ('PushBackEditAllTests', 'CT_PBT4C_11_ApplyToDraftNeverMutatesTheTwoSharedReferencesAndReplacingOneNeverMakesTheDraftDirty', 'Core'),
    '12': ('PushBackEditAllTests', 'CT_PBT4C_12_TheGateOfNoticesForwardsEditAllAndWaitsInsideANotice', 'Core'),
    '13': ('PushBackEditAllGuardTests', 'CT_PBT4C_13_TheAdapterOfThePluginForwardsEditAllAndItsGuardSeesAMissingForwardAndOneThatDoesMoreThanDelegate', 'Core'),
    '14': ('PushBackEditAllWindowTests', 'CT_PBT4C_14_TheFieldCallsThePortInItsOrderWithTheRequestOfTheGestureAndOffersTheButtonOnlyWhereTheEditorOpens', 'UI'),
    '15': ('PushBackDraftLaunchGuardTests', 'CT_PBT4C_15_TheTwoNewFilesOfTheFolderOfTheFullModelDoNotMutateTheResolvedRackAreProbablyPureAndCoverApplyToDraft', 'Core'),
}
red_msgs = {}
for c in core_classes[:2]:
    for r in ET.parse('%s/tests/d0/%s.trx' % (RUN, c)).getroot().findall('.//t:UnitTestResult', NS):
        m = r.find('.//t:Message', NS)
        red_msgs[r.get('testName').split('.')[-1]] = (m.text or '') if m is not None else ''
for c in ui_classes:
    for r in ET.parse('%s/tests/d0/%s.trx' % (RUN, c)).getroot().findall('.//t:UnitTestResult', NS):
        m = r.find('.//t:Message', NS)
        red_msgs[r.get('testName').split('.')[-1]] = (m.text or '') if m is not None else ''


def first_diff(msg):
    lines = [l.strip() for l in msg.split('\n')]
    exp = next((l for l in lines if l.startswith('Expected:')), '')
    act = next((l for l in lines if l.startswith('Actual:')), '')
    return (exp + ' || ' + act)[:230]


table = []
for ct, (cls, method, proj) in sorted(ct_methods.items()):
    if ct in ('07', '15'):
        table.append('CT-PBT4C-%s -> %s.%s (%s, ExpectRed false): pasa en el RED y en el GREEN; sus autopruebas inyectadas fallan (07: 48 de 48; 15: 15 de G-L1 y 15 de pureza)' % (ct, cls, method, proj))
    else:
        table.append('CT-PBT4C-%s -> %s.%s (%s): en el RED falla por Assert.Equal; %s' % (ct, cls, method, proj, first_diff(red_msgs.get(method, ''))))
table.append('Equivalente de CT-PBT2B-06 -> PushBackEditAllApplyCloseTests.CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied (UI): en el RED falla por Assert.Equal (la ventana 4 pregunta lo de hoy, no la combinada)')
table.append('CT-13: 2 autopruebas (sin el reenvio de TryEditAll; reenvio que no solo delega) dan falla en el GREEN; en el RED la guarda ya falla en su primera linea (el adaptador no tiene TryEditAll)')

evidence = [
    'RedSha %s (push antes del GREEN); CurrentSha %s; sin reparacion del RED; BaseSha..RedSha y RedSha..CurrentSha: el RED toca 8 de src/ y 13 de tests/, el GREEN solo 10 archivos de src/ (los 3 restantes del GREEN, IWorkspaceFullEditHost.cs, IWorkspacePilotEditPort.cs y WorkspaceEditIntent.cs, ya eran identicos al esqueleto)' % (RED, CUR),
    'Anclas del paso 1: green_pbt4c_v51.py sobre BaseSha comprobo, sin que fallara ninguna asercion, cada OLD exactamente una vez; %s/gen/anchors-sha.txt; los 4 archivos nuevos de src/ y las 9 pruebas nuevas no existian en BaseSha' % RUN,
    'Herramienta del RED: %s/gen/apply-red.txt (16 archivos, sha256 iguales a los de la lista del encargo: 8 del esqueleto, 4 de los Skip y 4 equivalentes); comprobador de los Skip: %s/gen/check-skips-red.txt y %s/gen/check-skips-green.txt (PASA, 4 archivos, 4 Skip)' % (RUN, RUN, RUN),
    'Herramienta del GREEN: %s/gen/apply-green.txt (13 archivos de src/, sha256 iguales a los de la lista del encargo)' % RUN,
    'Equivalentes (sha256 del texto con LF, iguales a EquivalentSha, comprobados en %s/inv/inv-green-final.txt): PushBackEditAllPortEquivalenceGuardTests 1a2607d3a35ae8f9737fffb0eb32a4acd8100318fe83e5b0098d30428f57d250; PushBackEditAllNameWritersGuardTests 3744992127be8b7e1d8d822a769b8eb2a6886789c73285c8436e5baec6bf7b6b; PushBackEditAllFullCopyGuardTests 4619b6d310340f9c97a7d9f31bda1db852d2fcda905b91bb857510f7e6b7e6ce; PushBackEditAllApplyCloseTests 7c2458260ef301d7f05bfaff8e1e8c4a4a618f25fd7022705d55dee32dbe78d2' % RUN,
    'Lista cerrada leida con git show a8e9cbe686785eef5f741a798f1e54f4419f35a1: sha256 9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de (4 filas)',
    'Compilacion tras el esqueleto y antes de escribir las CT: %s/tests/s1/ (Core, UI y Plugin Release, 0 errores, 0 "warning CS", conteos por codigo iguales a la linea base %s/tests/b0/warn-counts.txt); antes del commit RED: %s/tests/r1/; sobre el GREEN: %s/tests/g1/ (los conteos por codigo, en cada warn-counts.txt)' % (RUN, RUN, RUN, RUN),
    'Mapa CT -> metodo y tabla "CT -> la asercion que falla en el RED":',
] + table + [
    'Totales locales RED (suite completa de Core en 3 particiones disjuntas, %s/inv/filters-red.txt): 19619 = 19149 Passed + 9 Failed + 461 Skipped; UI, los 17 filtros heredados en verde y los 2 nuevos con ExpectRed en Failed == Selected (4 y 1; las 5 fallas de UI = las 4 CT y el equivalente de CT-PBT2B-06)' % RUN,
    'Totales locales GREEN (%s/inv/filters-green.txt): Core 19619 = 19158 Passed + 0 Failed + 461 Skipped; UI, los 19 filtros del contrato sin Failed (los dos de ExpectRed con 4 y 1 Passed); los 85 filtros del RequiredTests cumplen su minimo (RED: 85 OK en %s/inv/filters-red.txt, con los 4 de ExpectRed en Failed == Selected)' % (RUN, RUN),
    'Tres corridas consecutivas en verde de los cuatro filtros con ExpectRed sobre el GREEN: %s/tests/e1, e2 y e3 (6, 3, 4 y 1 Passed cada vez)' % RUN,
    'CI del RED (run 44956801926 sobre %s): tests failure, ui-tests failure, build-ui success, build-plugin skipped, sin cuelgue; Core 19619 = 19149 Passed + 9 Failed + 461 Skipped y UI 2486 = 2432 Passed + 5 Failed + 49 Skipped (TRX en %s/tests/redci/{core,ui}); el primer StackTrace de cada falla esta bajo tests/' % (RED, RUN),
    'CI del GREEN (run 44959653142 sobre %s): los cuatro jobs en success, Plugin compilado; Core 19619 = 19158 Passed + 0 Failed + 461 Skipped y UI 2486 = 2437 Passed + 0 Failed + 49 Skipped (TRX en %s/tests/greenci/{core,ui})' % (CUR, RUN),
    'INV-PBT4C-01 (alcance), Skip, equivalentes, GREEN literal (13 sha256), CENSO de declaraciones por nombre de BaseSha a CurrentSha (la diferencia es exactamente la lista cerrada de PBT4C-C13 en cada uno de los 13 archivos), "warning CS" (0 en todos los registros) y D2 (ninguna linea anadida de src/ nombra WorkingBaseline, LastModuleReconciliation, RackModuleReconciliationResult ni PushBackSystem fuera de comentarios): %s/inv/inv-green-final.txt (RESULTADO PASA); sobre el arbol del GREEN antes del commit: %s/inv/inv-green-worktree.txt; sobre el RED: %s/inv/inv-red.txt; script: %s/inv/check_inv.py' % (RUN, RUN, RUN, RUN),
    'FilesChanged = git diff --name-only BaseSha..CurrentSha (26 rutas, las del AllowedWriteScope): %s/inv/files-changed.txt' % RUN,
    'Prueba de limpieza (SHA-256 y fecha UTC del archivo de disposicion del configurador; fecha UTC y SHA-256 del listado de la carpeta de biblioteca): antes %s/clean/before.txt, tras la linea base %s/clean/after-baseline.txt y despues de todas las corridas %s/clean/after.txt: identicas' % (RUN, RUN, RUN),
    'Trailer real de los commits RED y GREEN (git log --format=%B BaseSha..HEAD): ' + TRAILER,
    'Registros de build de las tres compilaciones Release de linea base: %s/tests/b0/ (build-core.log, build-ui.log, build-plugin.log y warn-counts.txt)' % RUN,
]

work = (
    'PB-T4c hecho en un commit RED y un commit GREEN, sin reparacion del RED. RED (%s): el esqueleto exacto de src/ (8 archivos) escrito por apply_red_pbt4c_v51.py, los 4 Skip literales de la lista cerrada, los 4 equivalentes byte a byte '
    '(EquivalentSha) y las cinco clases de las CT (CT-PBT4C-01 a 15, un [Fact] por CT, ninguna Theory nueva; 11 de Core y 4 de UI). GREEN (%s): el texto literal del paquete v5.1 (apply_green_pbt4c_v51.py, 13 archivos de src/, sha256 iguales a los de la lista), sin tocar tests/. '
    'Todo lo pedido se cumple en local y en la CI, sin AutoCAD. Las CT de texto (01, 07, 08, 13, 15) parten de las lineas del ensayo del paquete; las de comportamiento de Core (03, 04, 05, 12) siguen la sonda del paquete; las de CT-10 y CT-11 y las de UI (02, 06, 14) las escribi desde las lineas de la seccion 5.3 y el texto del GREEN, y CT-09 sigue la sonda de UI. '
    'Ninguna prueba salio mal sobre el GREEN, asi que no se uso la reparacion del RED.' % (RED, CUR))

deviations = [
    'La suite completa de Core no se corrio en una sola orden sino en tres particiones disjuntas cuya suma es la suite (filtro FullyQualifiedName~RackCad.Tests.Workspace; el complemento sin la clase FullPanelCopyWorkspacePilotPrepareGreenTests; y esa clase sola), porque el entorno pasa a segundo plano toda orden de mas de 3 minutos y la prueba InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten de esa clase tarda mas de 10 minutos en esta maquina. Los totales de las particiones suman 19619 en el RED y en el GREEN, con las cifras exactas del contrato.',
    '--blame-hang-timeout: la clase FullPanelCopyWorkspacePilotPrepareGreenTests se corrio sola con 20m (RED) y 30m (GREEN) en lugar de 10m, tras dos corridas del complemento del RED abortadas por el detector de cuelgue a los 10m con esa misma prueba en curso (%s/tests/r2 y %s/tests/r2b; resultados parciales, 16233 resultados, 9 Failed = las 9 CT de Core con ExpectRed). La prueba es una prueba existente que no se toco: sobre BaseSha tardo 6m45s, sobre el RED sola 14m18s y sobre el GREEN 8m45s, y pasa en las tres. La primera corrida abortada coincidio con una corrida mia de filtros de UI en paralelo (error mio); la segunda corrio sola y se aborto igual. No se reintento mas el complemento con 10m; el resto de las corridas uso 10m.' % (RUN, RUN),
    'Los filtros de UI de la linea base se corrieron sobre BaseSha (17 filtros) y los del RED y del GREEN sobre los 19 del contrato (17 heredados y los 2 nuevos), uno a uno y nunca la suite completa de UI.',
]

unexpected = [
    'La prueba existente InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten (FullPanelCopyWorkspacePilotPrepareGreenTests) es muy lenta y de duracion muy variable en esta maquina (6m45s, 8m45s y 14m18s en tres corridas, tambien aislada); ver Deviations. La CI de GitHub la corrio sin cuelgue en el RED y en el GREEN.',
    'En las corridas locales los compiladores avisan los mismos conteos por codigo que la linea base (xUnit2013, xUnit2029, xUnit2031 y demas) y ninguna linea "warning CS"; los MSB3277 del Plugin son las referencias de AutoCAD locales.',
    'La prueba de limpieza no cambio nada: ni el archivo de disposicion del configurador ni la carpeta de biblioteca.',
]

limits = [
    'No se ejecuto AutoCAD: lo que solo se ve alli queda para el Smoke-PB (el modal WPF de la ventana de Push Back abierta con ShowModalWindow sin dueno desde la paleta, la paleta deshabilitada mientras el modal esta abierto, el foco y el teclado reales, el redibujo con Actualizar tras editar).',
    'Las CT de UI (02, 06, 14) usan la FVM real sobre un doble del puerto y CT-09 lee PendingWork() de la ventana real sin mostrarla; ninguna abre la ventana desde el panel ni crea un configurador.',
    'Para las subtareas siguientes queda PB-T4d (retirar el codigo del piloto, Pilot/), que vuelve al Owner antes de emitirse; este paso no toca Pilot/ ni el camino clasico, RACKEDITAR o la ventana en CLASSIC.',
    'No puedo observar el uso del equipo mientras corren las pruebas; las duraciones de las corridas locales son las del reloj del entorno, con la variabilidad descrita.',
    'Compilaciones de desarrollo en Debug para correr las pruebas, sin registro guardado (solo las builds Release por proyecto, pedidas, tienen su registro completo).',
]

handoff = {
    'Schema': 'rackcad-worker-handoff/v1',
    'TaskId': 'F4B-PBT4-EDIT-ALL-WINDOW',
    'RunId': 'R20261121T205731Z-41db',
    'DelegationRunId': 'R20261121T205457Z-af49',
    'Initiative': 'I-64',
    'Gate': 'F4B',
    'Attempt': 1,
    'BaseSha': BASE,
    'RedSha': RED,
    'CurrentSha': CUR,
    'Branch': 'architecture/workspace-persistente-rackcad',
    'Worktree': WORKTREE,
    'Pushed': True,
    'FilesChanged': [l.strip() for l in open('%s/inv/files-changed.txt' % RUN, encoding='utf-8') if l.strip()],
    'WorkCompleted': work,
    'TestsExecuted': executed,
    'TestResults': results,
    'Evidence': evidence,
    'UnexpectedFindings': unexpected,
    'KnownLimitations': limits,
    'Deviations': deviations,
    'OpenQuestions': [],
    'WorkerStatus': 'IMPLEMENTATION_COMPLETE',
    'Disposition': 'NONE',
    'TriggeredStopConditions': [],
    'RecommendedNextAction': 'Verificacion del Controller con las tres comprobaciones de F4B-E15, nc1 a nc3, las revisiones y el Smoke-PB.',
    'Worker': {'Provider': 'Anthropic', 'ModelRequested': 'claude-sonnet-5-5', 'EffortRequested': 'high', 'Trailer': TRAILER},
}
with open('%s/worker-handoff.json' % RUN, 'w', encoding='utf-8', newline='\n') as f:
    json.dump(handoff, f, ensure_ascii=False, indent=2)
    f.write('\n')
print('entregados: TestsExecuted=%d TestResults=%d Evidence=%d' % (len(executed), len(results), len(evidence)))
