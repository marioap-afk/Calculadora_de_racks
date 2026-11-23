"""Compone worker-handoff.json a partir de inv/runs.json y de los datos medidos (rutas relativas comprobadas con os.path.exists).
Uso: python -I make_handoff.py"""
import json
import os
import subprocess
import sys

A = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e'
BASE = '7da361ebe8919bf31e2874c42dc946b98c62635a'
RED = '265f9e31cb33ce312c04d2d7959c999aee763b3c'
CUR = '0eac7911a025a59628b446db8b768035175f96de'
TRAILER = 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>'


def rel(p):
    full = '%s/%s' % (A, p)
    assert os.path.exists(full), full
    return full


def git(*a):
    return subprocess.run(['git'] + list(a), capture_output=True, check=True).stdout.decode('utf-8').strip()


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    runs = json.load(open(A + '/inv/runs.json', encoding='utf-8'))
    head = git('rev-parse', 'HEAD')
    assert head == CUR, head
    assert git('ls-remote', 'origin', 'refs/heads/architecture/workspace-persistente-rackcad').split()[0] == CUR
    files = [l for l in git('diff', '--name-only', '%s..%s' % (BASE, CUR)).split('\n') if l]
    worktree = git('rev-parse', '--show-toplevel')
    branch = git('branch', '--show-current')
    # el trailer real de los dos commits
    for sha in (RED, CUR):
        assert git('log', '-1', '--format=%B', sha).rstrip().endswith(TRAILER), sha

    work = '''PB-T4e (F4B-PBT4-EDIT-ALL-RELOAD), intento 1, hecho en dos commits: RED (RedSha 265f9e31cb33ce312c04d2d7959c999aee763b3c) y GREEN (CurrentSha 0eac7911a025a59628b446db8b768035175f96de); sin reparacion del RED (RR no se uso: no hubo RedRepairSha).

MAPA CT -> METODO (section 5.7; un [Fact] por CT; ninguna Theory):
- CT-PBT4E-01: RackCad.Tests.PushBackEditAllReloadTests.CT_PBT4E_01_EditAllReloadsACleanDraftMarkedAsPossiblyStaleBeforeTheEditorOpensAndLeavesEveryOtherDraftAsToday
- CT-PBT4E-02: RackCad.Tests.PushBackEditAllReloadTests.CT_PBT4E_02_AReloadThatDoesNotReloadTheDraftDoesNotOpenTheEditorAndCarriesWhatTheReloadOfEditarAnswers
- CT-PBT4E-04: RackCad.Tests.PushBackEditAllReloadTests.CT_PBT4E_04_FromTheChangeOutsideAndItsMarkThroughEditAllToTheUpdateWithoutAutoCadTheUpdateAppliesWithoutAConflict
- CT-PBT4E-03: RackCad.UI.Tests.PushBackEditAllReloadFieldTests.CT_PBT4E_03_TheFieldShowsAReloadOfEditAllThatDidNotReloadAsEditarShowsItAndOnlyAsksThePortForEditAll
Las autopruebas de las CT van dentro de sus [Fact] (el contrato no trae ninguna aparte); no se escribio ninguna prueba ni ayudante a mano: las dos clases son la pieza literal del RED.

ANCLAS DEL PASO 1 (sobre BaseSha, con git show, sin compilar): cada bloque EDIT de pbt4e-green-v56.txt tiene su OLD exactamente una vez en su archivo (los cuatro archivos de src/), los dos archivos NEWFILE del RED no existen en BaseSha, ninguno de los nombres nuevos se nombra en src/ ni en tests/ (git grep -w), y no hay archivos nuevos de src/. Salida: %(anchors)s

HERRAMIENTAS (los seis archivos copiados con git show desde BaseSha, sha256 comprobados, todos iguales a los del contrato): apply_red_pbt4e_v56.py 59a0fcb3e316b1fbde880fca87629bc2144b233859eccdf9ddb86c244d2a0ae5; red_pbt4e_v56.py 8c38e532bf991a111e7f82193ad3a812993a64a1c2e43084a46278ad72dc54a5; pbt4e-red-v56.txt d052f73d726817f4240f3b5edfd89a0ec8aeb7b9cc17dbce684aa8e4f433c9e1; apply_green_pbt4e_v56.py 492aaab155fe64cc96b317441c1bd18b2bfc47f92c44bd1dac24f131bc8378a7; green_pbt4e_v56.py b3aba680d45ce6aebb3dbac549e228de9b832a76d870fd719ea0801302c10e73; pbt4e-green-v56.txt a745c2737a7e0fe908489a1bf17ed86569577ed3dbf54ecd3661c488faf88158.

SALIDA DE LA HERRAMIENTA DEL RED (%(applyred)s): nuevo 9e849864709a964a864902c6f44e5d92014d962cc56c374cebfc7dfe42c6b017 tests/RackCad.Tests/PushBackEditAllReloadTests.cs; nuevo a29d7a362cd5de49bd787eb76e7c847dc30b46ef1314e94ae5f67041b6a4595d tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs; iguales a inputs/red-files.txt (sha256 ea38d71b7d78f347d689a7a79d8a2898be0ed5f095d997a76982b4379b7bc579). Ningun archivo mas escrito. Tras aplicarla, ANTES de nada mas, se compilaron los tres proyectos en Release (RunRef r0): 0 errores, 0 lineas "warning CS", y los demas avisos por codigo iguales a la linea base (Core: xUnit1026 20, xUnit2000 6, xUnit2010 4, xUnit2012 2, xUnit2013 34, xUnit2017 2, xUnit2029 32, xUnit2031 32; UI: xUnit1026 2, xUnit2013 4, xUnit2029 4, xUnit2030 2, xUnit2031 16; Plugin: MSB3277 104). Antes del commit RED se volvio a compilar (r4): lo mismo. En el GREEN (g0): lo mismo. Ningun archivo existente de tests/ ni de src/ se toco en el RED; git diff --cached --name-status dio exactamente A en las dos clases y nada mas.

SALIDA DE LA HERRAMIENTA DEL GREEN (%(applygreen)s): green 49b5245c112e3d4f0b3b95114f9ff0444469b316a3ef912947cf3b696e0c6ebd src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs; green bbc6882283833b02bab79c9bb70c082f3ac14b2a88650722bf8d72be0e13b5e2 src/RackCad.Application/Workspace/WorkspaceEditIntent.cs; green 6eb9bf7ab81666b33c1280c832257624937c95a042e46f53dcc2c371baac4d7d src/RackCad.Application/Workspace/WorkspaceHostCore.cs; green c4b5c6990c5623e53be607fdbb461db95103b957c96627e1d63300dabe3d643b src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs; iguales a la lista del contrato; tests/ sin tocar. El sha256 de cada uno de los cuatro archivos de src/ en CurrentSha (CRLF -> LF) y de los dos archivos nuevos de tests/ en CurrentSha y en RedSha es el de esa lista y el de inputs/red-files.txt (comprobado por inv/check_inv.py).

TABLA "CT -> LA ASERCION QUE FALLA EN EL RED" (sobre RedSha, src/ de BaseSha; cada una cae por su Assert.Equal en el Finish de su clase: Core, PushBackEditAllReloadTests.cs linea 50; UI, PushBackEditAllReloadFieldTests.cs linea 46, la primera linea del StackTrace bajo tests/; ninguna por excepcion; la CI del RED da las mismas lineas):
- CT-PBT4E-01 cae en su primera linea (marcado-limpio): esperado "lecturas=1 lecturas-antes-de-abrir=1 ..." / actual "lecturas=0 ..." (src/ no recarga).
- CT-PBT4E-02 cae en su primera linea (no-admitida): esperado "estado=NotBegun inicio=NotAdmitted ..." / actual "estado=Closed inicio=Unavailable ..." (abre sin recargar).
- CT-PBT4E-04 cae en su linea editar-todo: esperado "lecturas=1 nombre-del-lanzamiento=[Otro nombre] obsoleto=False" / actual "lecturas=0 ...".
- CT-PBT4E-03 cae en su primera linea (no-recargado editar-todo): esperado "aviso=[No se recargo: el borrador se conserva tal como estaba.]" / actual "aviso=[]".
Mensajes completos: %(redmsg)s.

LOS DOS FILTROS CON ExpectRed: sobre RedSha, PushBackEditAllReloadTests Selected 3, Failed 3 (Failed == Selected, ninguno NotExecuted ni Skipped) y PushBackEditAllReloadFieldTests Selected 1, Failed 1, ambos por asercion (RunRef r1/*). Los otros 88 filtros del RequiredTests, uno a uno (r3/*), pasan sin Failed y con Passed >= MinSelected (verificado con inv/checkall.py: 88 filtros, 0 violaciones; %(r3check)s). Sobre el GREEN, los 88 pasan igual (ga/*, %(gacheck)s) y los dos con ExpectRed pasan tres veces consecutivas: g1, g2 y g3 (3 de 3 y 1 de 1 Passed cada vez).

TOTALES LOCALES (sin alternativa, F4B-E46): linea base Core 19418 = 19015 Passed + 0 Failed + 403 Skipped (b0/core-full). RED Core 19421 = 19015 Passed + 3 Failed + 403 Skipped, fallan solo las tres CT de Core (r2/core-full). GREEN Core 19421 = 19018 Passed + 0 Failed + 403 Skipped (gf/core-full). En UI no se corre la suite completa en local (regla del contrato): los 19 filtros de UI uno a uno; el total de UI es el de la CI. Los Skipped de Core son los de la linea base (403, con los de D-1 y CT-PBT3A-12), ninguno mas.

TOTALES DE LA CI (rutas en Evidence): RED, run 45119881969 sobre RedSha: tests failure, ui-tests failure, build-ui success, build-plugin skipped (omitido), sin cuelgue; Core 19421 = 19015 Passed + 3 Failed + 403 Skipped (fallan CT-PBT4E-01, 02 y 04) y UI 2465 = 2416 Passed + 1 Failed + 48 Skipped (falla CT-PBT4E-03); la primera linea del StackTrace de cada fallo esta bajo tests/ (Finish). GREEN, run 45123886125 sobre CurrentSha: 4/4 en success (tests, build-ui, ui-tests y build-plugin con el Plugin compilado, 0 avisos MSB, CS, NU o NETSDK en su registro); Core 19421 = 19018 Passed + 0 Failed + 403 Skipped y UI 2465 = 2417 Passed + 0 Failed + 48 Skipped.

BUILDS RELEASE (por proyectos, logs completos sin filtrar): linea base b0, RED r0 y r4, GREEN g0; cada una con Core, UI y Plugin (Plugin con -o bajo tests/<RunRef>/plugin-out): 0 errores y 0 lineas "warning CS" en todos; los demas conteos de avisos por codigo iguales a la linea base.

INV-PBT4E-01 e INV-PBT4E-07 (salida de inv/check_inv.py sobre BaseSha..CurrentSha: %(invfinal)s): git diff --name-only RedSha..CurrentSha -- src tests = exactamente los cuatro archivos de src/ (nada de tests/); git diff --name-only BaseSha..CurrentSha = las 6 rutas exactas del alcance (FilesChanged); git diff --name-status BaseSha..CurrentSha -- tests = solo A de los dos archivos nuevos; git diff --name-only BaseSha..CurrentSha sobre src/RackCad.Domain, src/RackCad.Application/Systems/PushBack/Pilot/, RackPushBackCommands.cs, RackCommandSupport.cs, RackPilotIntentAdapter.cs y WorkspaceNavigationHost.cs = vacio.

CENSO DE DECLARACIONES (PBT4E-C13, INV-PBT4E-13; heuristico por llaves, una sobrecarga cuenta como una declaracion mas; listas completas por archivo y SHA en inv/census/base-*.txt y green-*.txt; BaseSha / CurrentSha): IWorkspacePilotEditPort.cs 108 / 108; WorkspaceEditIntent.cs 220 / 220; WorkspaceHostCore.cs 136 / 136; WorkspacePilotFieldViewModel.cs 189 / 190. Diferencia: UNA declaracion anadida, la constante privada WorkspacePilotFieldViewModel::const NotReloadedText (la de la lista cerrada); NINGUNA quitada. Cambios que no son declaraciones (D-S19): en EditAllOfPilot las locales begunHere y reload y la rama de recarga tras las tres comprobaciones de la vista; en WorkspacePilotFieldViewModel la linea de BeginEdit que usa la constante y la linea de EditAll que pone el aviso si la vista existia; los comentarios de los cuatro archivos.

D2 (PBT4E-C17, INV-PBT4E-14): las lineas anadidas con codigo en src/ son: var begunHere = false; begunHere = true; if (!begunHere && draft.NeedsLazyReload) { var reload = TryReloadPilotEdit(request); if (reload.Status != EditIntentStatus.Reloaded) return EditAllOutcome.NotBegun(reload); } private const string NotReloadedText = ...; if (stale) _noticeText = NotReloadedText; if (view.Exists) _noticeText = NotReloadedText;. Ninguna nombra ni muta un PushBackSystem ni un RackModuleReconciliationResult, y ninguna comparte un objeto mutable. La recarga es TryReloadPilotEdit (la de Editar, una sola llamada), sin copia; ningun miembro nuevo del puerto, ningun valor nuevo de EditAllStatus ni de PilotBoundaryKind.

"warning CS": 0 lineas "warning CS<n>" en todos los registros de build y de prueba bajo tests/ de esta entrega (conteo por archivo en inv/check_inv.py: ninguno con coincidencias).

LAS CT FIJAN (resumen): con Clean y marcado, TryEditAll recarga con TryReloadPilotEdit despues de la compuerta, de la actualizacion en vuelo, del anfitrion del editor y de las tres comprobaciones de la vista, y antes de ShowModal; solo Reloaded abre; los demas resultados devuelven EditAllOutcome.NotBegun con el resultado de la recarga y no abren; el campo muestra un NotBegun de recarga como Editar (diagnostico, pista y "No se recargo: el borrador se conserva tal como estaba." para NotAdmitted sobre un borrador que ya existia); Dirty, Conflict, Sin decidir, Unknown, Error, Applying, texto pendiente y borrador sin marca siguen como en BaseSha; sin aviso cuando recarga; una lectura como mucho por gesto y ninguna durante ShowModal.

PRUEBA DE LIMPIEZA (sin rutas): ANTES (inv/cleanliness-before.txt, antes de cualquier corrida) y DESPUES de la linea base (inv/cleanliness-after-b0.txt), DESPUES del RED (inv/cleanliness-after-red.txt) y DESPUES del GREEN (inv/cleanliness-after-green.txt): identicas las cuatro, con diff vacio. Disposicion del configurador (RackFrameConfigurator.layout): sha256 7AAB8FF7A3E52CE52399DE3B2C8A8AFEEC0C01720D90DAD0CA49D108BFF82621, fecha UTC 2026-11-19T21:41:29.4447298Z. Carpeta de biblioteca: fecha UTC 2026-10-13T21:42:51.4479430Z, 11 entradas, sha256 del listado recursivo ordenado (ruta relativa, tamano, fecha UTC) BB21C85D0D5A5990C2539D9923050093B985853557D8E907698EE15AE32A1D87. settings.json (informativo): sha256 A7667DBDCEDA5A5175C844ED396BA1F0B17207295F40E9986C29D826B48B2873, fecha UTC 2026-10-08T22:50:58.9250581Z. Ningun cambio del perfil ni de la biblioteca.

CONFORMIDAD DE ALCANCE: el Worker no escribio en docs/ ni en el checkout principal; las unicas escrituras fuera de artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e/ son las seis rutas del alcance. No se ejecuto, abrio ni cerro AutoCAD. Dos commits, cada uno con push y con el trailer real; sin --force, sin rebase, sin stash, sin amend, sin otras ramas.''' % {
        'anchors': rel('inv/step1-anchors.txt'),
        'applyred': rel('gen/apply-red.txt'),
        'applygreen': rel('gen/apply-green.txt'),
        'redmsg': rel('inv/red-assertion-messages.txt'),
        'r3check': rel('tests/r3/check.txt'),
        'gacheck': rel('tests/ga/check.txt'),
        'invfinal': rel('inv/inv-final.txt'),
    }

    evidence = [
        'RedSha 265f9e31cb33ce312c04d2d7959c999aee763b3c (hijo directo de BaseSha, empujado); CurrentSha 0eac7911a025a59628b446db8b768035175f96de (hijo directo de RedSha, empujado); remoto de la rama = CurrentSha.',
        'Trailer real de los dos commits (git log --format=%B BaseSha..HEAD): ' + TRAILER,
        'Herramienta del RED, salida: ' + rel('gen/apply-red.txt'),
        'Herramienta del GREEN, salida: ' + rel('gen/apply-green.txt'),
        'Piezas copiadas (RED y GREEN, con sus herramientas): ' + rel('gen/pbt4e-red-v56.txt') + '; ' + rel('gen/pbt4e-green-v56.txt'),
        'Anclas del paso 1: ' + rel('inv/step1-anchors.txt'),
        'Comprobaciones de INV-PBT4E-01, 07, RED/GREEN literales, censo, D2 y warning CS (sobre BaseSha..CurrentSha): ' + rel('inv/inv-final.txt'),
        'Mismas comprobaciones antes del commit GREEN (arbol de trabajo): ' + rel('inv/inv-pre-green-commit.txt'),
        'Censo de declaraciones BaseSha / GREEN por archivo: ' + rel('inv/census/base-WorkspaceHostCore.txt') + ' ' + rel('inv/census/green-WorkspaceHostCore.txt') + ' ' + rel('inv/census/base-WorkspaceEditIntent.txt') + ' ' + rel('inv/census/green-WorkspaceEditIntent.txt') + ' ' + rel('inv/census/base-IWorkspacePilotEditPort.txt') + ' ' + rel('inv/census/green-IWorkspacePilotEditPort.txt') + ' ' + rel('inv/census/base-WorkspacePilotFieldViewModel.txt') + ' ' + rel('inv/census/green-WorkspacePilotFieldViewModel.txt'),
        'Mensajes de las cuatro aserciones del RED: ' + rel('inv/red-assertion-messages.txt'),
        'Builds Release (log completo cada una): linea base ' + rel('tests/b0/build-RackCad.Tests.log') + ' ' + rel('tests/b0/build-RackCad.UI.Tests.log') + ' ' + rel('tests/b0/build-RackCad.Plugin.log') + '; RED tras aplicar la pieza ' + rel('tests/r0/build-RackCad.Tests.log') + ' ' + rel('tests/r0/build-RackCad.UI.Tests.log') + ' ' + rel('tests/r0/build-RackCad.Plugin.log') + '; RED antes del commit ' + rel('tests/r4/build-RackCad.Tests.log') + ' ' + rel('tests/r4/build-RackCad.UI.Tests.log') + ' ' + rel('tests/r4/build-RackCad.Plugin.log') + '; GREEN ' + rel('tests/g0/build-RackCad.Tests.log') + ' ' + rel('tests/g0/build-RackCad.UI.Tests.log') + ' ' + rel('tests/g0/build-RackCad.Plugin.log'),
        'Ordenes de build ejecutadas (cada una desde la raiz del worktree, con -v:minimal y --no-incremental, y log redirigido): <dotnet> build tests/RackCad.Tests/RackCad.Tests.csproj -c Release --no-incremental -v:minimal; <dotnet> build tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Release --no-incremental -v:minimal; <dotnet> build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e/tests/<RunRef>/plugin-out (con <dotnet> = el dotnet.exe del SDK de usuario que el encargo prescribe).',
        'Resumen de los 88 filtros sobre el RED: ' + rel('tests/r3/all-summary.txt') + ' (comprobacion: ' + rel('tests/r3/check.txt') + '); sobre el GREEN: ' + rel('tests/ga/all-summary.txt') + ' (comprobacion: ' + rel('tests/ga/check.txt') + ')',
        'Baseline UI de los 18 filtros de UI heredados (la clase PushBackEditAllReloadFieldTests no existe en BaseSha): ' + rel('tests/b0/ui-filters-summary.txt'),
        'TRX de la suite completa de Core: linea base ' + rel('tests/b0/core-full.trx') + '; RED ' + rel('tests/r2/core-full.trx') + '; GREEN ' + rel('tests/gf/core-full.trx'),
        'Filtros con ExpectRed: RED ' + rel('tests/r1/core-PushBackEditAllReloadTests.trx') + ' ' + rel('tests/r1/ui-PushBackEditAllReloadFieldTests.trx') + '; GREEN, tres corridas consecutivas g1 ' + rel('tests/g1/core-PushBackEditAllReloadTests.trx') + ' ' + rel('tests/g1/ui-PushBackEditAllReloadFieldTests.trx') + ', g2 ' + rel('tests/g2/core-PushBackEditAllReloadTests.trx') + ' ' + rel('tests/g2/ui-PushBackEditAllReloadFieldTests.trx') + ', g3 ' + rel('tests/g3/core-PushBackEditAllReloadTests.trx') + ' ' + rel('tests/g3/ui-PushBackEditAllReloadFieldTests.trx'),
        'CI push del RED (RedSha), run 45119881969: tests failure; ui-tests failure; build-ui success; build-plugin skipped; TRX descargados: ' + rel('tests/redci/core/core.trx') + ' (Core 19421 = 19015 + 3 Failed + 403 Skipped) y ' + rel('tests/redci/ui/ui.trx') + ' (UI 2465 = 2416 + 1 Failed + 48 Skipped).',
        'CI push del GREEN (CurrentSha), run 45123886125: 4/4 success (tests, build-ui, ui-tests, build-plugin con el Plugin compilado); TRX descargados: ' + rel('tests/greenci/core/core.trx') + ' (Core 19421 = 19018 + 0 Failed + 403 Skipped) y ' + rel('tests/greenci/ui/ui.trx') + ' (UI 2465 = 2417 + 0 Failed + 48 Skipped).',
        'Prueba de limpieza: ' + rel('inv/cleanliness-before.txt') + ' ' + rel('inv/cleanliness-after-b0.txt') + ' ' + rel('inv/cleanliness-after-red.txt') + ' ' + rel('inv/cleanliness-after-green.txt') + ' (identicas).',
        'Entrada de la herramienta de comprobacion y lista de filtros: ' + rel('inputs/red-files.txt') + ' ' + rel('inv/filters.tsv') + '; ayudantes de solo lectura del Worker: ' + rel('inv/check_inv.py') + ' ' + rel('inv/census.py') + ' ' + rel('inv/checkall.py') + ' ' + rel('inv/trxsum.py') + ' ' + rel('inv/collect_runs.py') + ' ' + rel('inv/make_handoff.py') + ' ' + rel('inv/runf.sh') + ' ' + rel('inv/runall.sh') + ' ' + rel('inv/cleanliness.ps1'),
        'Corridas (TestsExecuted/TestResults) reunidas de los TRX con ' + rel('inv/runs.json'),
    ]

    unexpected = [
        'Los comandos largos (suite completa de Core, el bucle de filtros) pasaron a segundo plano por la propia herramienta de shell tras 180 s; no los lance yo con & ni como proceso desacoplado, y espere cada uno en primer plano con Wait-Process sobre su proceso. En tres llamadas lance gh run watch (con & en shell, o con Start-Process en PowerShell) y lo espere en la misma llamada; y una espera de PowerShell basada en ping (un retardo) se detuvo en cuanto me di cuenta de que equivalia a un temporizador (TaskStop); ninguna orden de espera con sleep. Ningun proceso quedo vivo (dotnet build-server shutdown al final).',
        'git add de las dos clases nuevas avisa de que LF se sustituira por CRLF en el arbol de trabajo (efecto de .gitattributes/autocrlf); el blob del commit RED, con CRLF -> LF, es el literal (sha256 igual a inputs/red-files.txt, comprobado sobre RedSha y sobre CurrentSha).',
        'La prueba de limpieza no cambio en ninguna de las cuatro mediciones: la disposicion del configurador y la biblioteca quedaron iguales.',
    ]
    limits = [
        'No se ejecuto AutoCAD: lo que el paquete deja para el Smoke-PB (el gesto real de Editar todo sobre un dibujo que cambio fuera del panel, con el modal real y la lectura real del DWG) no se comprueba aqui.',
        'Quedan para las subtareas siguientes lo que el paquete les asigna (la tarea PB-T5: CI, Smoke-PB y cierre de la fase); esta entrega es solo PB-T4e.',
        'En UI no se corre la suite completa en local (regla del contrato): los totales de UI de la entrega, 2465 = 2416 + 1 Failed + 48 (RED) y 2465 = 2417 + 0 + 48 (GREEN), son los de la CI; en local solo los 19 filtros de UI uno a uno.',
        'El censo de declaraciones es un analisis por llaves escrito por el Worker (inv/census.py), no un compilador; sus listas completas van en inv/census/. La diferencia (una constante privada, ninguna quitada) coincide con la lista cerrada de PBT4E-C13 y con el diff literal del GREEN.',
        'Los campos Command de TestsExecuted llevan, literal, la ruta del dotnet.exe del SDK de usuario que el encargo prescribe (el dotnet del PATH no sirve); es la orden exacta ejecutada. Los TreeSha son null: todas las corridas locales fueron antes de su commit.',
        'No puedo observar el uso del equipo mientras corren las pruebas; no hice nada interactivo durante ellas.',
        'Cobertura de INV-PBT4E-06 (linea a linea y rama por rama con coverlet) se midio en la precondicion 6 de la sesion; el Worker no la repite: el GREEN es el texto literal medido alli y los 88 filtros mas la suite completa de Core pasan sin ningun Failed ni NotExecuted fuera de los 403/48 de la linea base.',
    ]

    handoff = {
        'Schema': 'rackcad-worker-handoff/v1',
        'TaskId': 'F4B-PBT4-EDIT-ALL-RELOAD',
        'RunId': 'R20261123T045827Z-6c9e',
        'DelegationRunId': 'R20261123T045429Z-48a1',
        'Initiative': 'I-64',
        'Gate': 'F4B',
        'Attempt': 1,
        'BaseSha': BASE,
        'RedSha': RED,
        'CurrentSha': CUR,
        'Branch': branch,
        'Worktree': worktree.replace('/', '\\') if ':' in worktree else worktree,
        'Pushed': True,
        'FilesChanged': files,
        'WorkCompleted': work,
        'TestsExecuted': runs['TestsExecuted'],
        'TestResults': runs['TestResults'],
        'Evidence': evidence,
        'UnexpectedFindings': unexpected,
        'KnownLimitations': limits,
        'Deviations': [],
        'OpenQuestions': [],
        'WorkerStatus': 'IMPLEMENTATION_COMPLETE',
        'Disposition': 'NONE',
        'TriggeredStopConditions': [],
        'RecommendedNextAction': 'El Controller ejecuta la verificacion (las tres comprobaciones deterministas de F4B-E15, nc1 a nc3 y la comparacion de las piezas RED y GREEN con sus NewFileSha y sus sha256) sobre RedSha y CurrentSha; despues, las revisiones directa e independiente del paquete.',
        'Worker': {'Provider': 'Anthropic', 'ModelRequested': 'claude-sonnet-5-5', 'EffortRequested': 'high', 'Trailer': TRAILER},
    }
    out = A + '/worker-handoff.json'
    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(handoff, f, indent=2, ensure_ascii=False)
        f.write('\n')
    print('escrito', out, os.path.getsize(out))


if __name__ == '__main__':
    main()
