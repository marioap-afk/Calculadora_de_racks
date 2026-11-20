import csv
import glob
import json
import subprocess

# Compone worker-handoff.json: narrativa escrita a mano + TestsExecuted/TestResults de inv/tests-fragment.json + conteos por filtro.
RUN = "artifacts/orchestration/I-64/F4B-PBT3A-FULL-WRITE-MODEL/2/R20261120T025240Z-f006"
BASE = "485b5651bcec132761347b40ed38b60cfec0db04"
RED = "5dbc9661438112c7be283a79ff5d980e78607067"
GREEN = "6919e17af9fdd5c702d454a97a3c5b6cc6be1739"
TRAILER = "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>"
files = subprocess.run(["git", "diff", "--name-only", BASE + ".." + GREEN], capture_output=True, text=True).stdout.split()
assert len(files) == 14, files
fragment = json.load(open(RUN + "/inv/tests-fragment.json", encoding="utf-8"))
worktree = subprocess.run(["git", "rev-parse", "--show-toplevel"], capture_output=True, text=True).stdout.strip().replace("/", chr(92))


def counts(folder):
    rows = {}
    for path in sorted(glob.glob(RUN + "/tests/" + folder + "/results-*.tsv")):
        for line in open(path, encoding="utf-8").read().splitlines():
            c = line.split("\t")
            rows[c[2].replace("RackCad.Tests.", "Core:").replace("RackCad.UI.Tests.", "UI:")] = (c[5], c[6], c[7], c[8], c[3])
    return rows


red, green = counts("r1"), counts("g1")
table = []
for name in green:
    r = red[name]
    g = green[name]
    table.append(name + " min=" + g[4] + " | RED sel/pas/fall/omit=" + "/".join(r[:4]) + " | GREEN sel/pas/fall/omit=" + "/".join(g[:4]))

work = """PB-T3a implementada en el intento 2 (correccion F4B-E42). RED: esqueleto exacto del paquete v4.5 en los ocho archivos nuevos de src/RackCad.Application/Systems/PushBack/Draft/ (miembro cerrado ForFullWrite) y las seis clases de prueba nuevas; RedSha """ + RED + """. GREEN: D-PBT3-02 a D-PBT3-07 solo en esos ocho archivos (el de PushBackFullDraftState.cs es identico al esqueleto, que ya es el contenedor completo); CurrentSha """ + GREEN + """. No hubo reparacion del RED (RR no se uso). No se toco ningun archivo existente de src/ ni de tests/ ni docs/. WorkerStatus IMPLEMENTATION_COMPLETE; sin parada.

MAPA CT -> METODO (clase): CT-PBT3A-01 CT_PBT3A_01_TheSeatMatchesTheWindowAfterLoadExisting (PushBackFullDraftWindowParityTests, UI); CT-02 CT_PBT3A_02_TheMaterializerIsTheOneOfThePilotExceptForTheCompositeRack, CT-03 CT_PBT3A_03_TheFullReaderIsTheOneOfThePilotExceptForTheCompositeRack, CT-04 CT_PBT3A_04_Auth13RejectsEveryUnknownFieldAtEveryLevelInBothReaders, CT-05 CT_PBT3A_05_TheCopierAndTheComparerOfTheFullStateKeepOnlyTheTwoDeclaredReferences (PushBackFullDraftLoadTests); CT-06 CT_PBT3A_06_ForFullWriteComposesTheDraftOverAFreshBaseAndRejectsWhatItCannotWrite, CT-07 CT_PBT3A_07_KeepOnKeepsTheEditableStateOfTheDraftOverTheNewOrigin, CT-11 CT_PBT3A_11_NoNewOperationMutatesTheResolvedRackOrTheModuleReport (PushBackFullDraftCompositionTests); CT-08 CT_PBT3A_08_TheDispositionOfTheViewsIsTheOneOfTheClassicEditor (PushBackFullDraftViewTests); CT-09 CT_PBT3A_09_TheClassifierGivesTheLeavesTheGroupsAndTheClassesOfTheOracle, CT-10 CT_PBT3A_10_TheClassifierMarksTheStructuralChangesAndTheViewsToErase (PushBackFullDraftClassifierTests); CT-12 CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes, CT-13 CT_PBT3A_13_NoLineOfTheFolderOfTheModelMutatesTheResolvedRackOrTheModuleReport, CT-14 CT_PBT3A_14_TheFolderOfTheModelHasNoAutoCadNoWindowsNoTransactionAndNoNullableDirective (PushBackDraftWriteGuardTests). Las clases de Core estan en tests/RackCad.Tests (namespace RackCad.Tests) y la de UI en tests/RackCad.UI.Tests.

TABLA CT -> ASERCION QUE FALLA EN EL RED (TRX del RED local y de la CI del RED; los once fallan con Assert.Equal() Failure: Collections differ y primera linea del StackTrace bajo tests/): CT-01 PushBackFullDraftWindowParityTests.cs:99 (el esqueleto no materializa: faltan las lineas estado-igual y diseno-igual); CT-02 PushBackFullDraftLoadTests.cs:144 (el esqueleto da Failed siempre); CT-03 LoadTests:269 (no admite); CT-04 LoadTests:340 (el lector completo da otro diagnostico: MaterializeDiagnostic en lugar de AUTH-13 y del control Admitted); CT-05 LoadTests:751 (Equals es ReferenceEquals y Copy da un estado vacio); CT-06 CompositionTests:310 (la composicion rechaza siempre); CT-07 CompositionTests:386 (KeepOn devuelve current); CT-08 PushBackFullDraftViewTests.cs:141 (Of da todo Redraw); CT-09 PushBackFullDraftClassifierTests.cs:315 y CT-10 :374 (Classify no produce las entradas esperadas); CT-11 CompositionTests:624 (el esqueleto no carga los estados); CT-12 a 14 pasan en el RED y sus autopruebas fallan sobre el texto inyectado en memoria (autoprueba-...=falla, 10 de CT-12, 8 de CT-13 y 15 de CT-14).

C-1 (F4B-E42): los parches del intento 1 se usaron como punto de partida, con el sha256 comprobado antes de abrirlos (wip-red-tree.patch 9f5964d1..., rr/skeleton.patch 70886a16..., rr/green-wip.patch 92b4de2a...; coinciden con los citados) y copiados a """ + RUN + """/a1/ (los originales no se tocaron). Sustituciones ForWrite -> ForFullWrite en la copia (linea del parche): wip-red-tree.patch:187 (declaracion en PushBackFullComposition.cs), :1379 y :1390 (llamadas de CT-06), :1413 (nombre del metodo de CT-06), :1745 (la operacion de la linea de CT-11), :1747 y :1748 (llamadas de CT-11), :1781 (la linea esperada de CT-11); rr/skeleton.patch:187 (declaracion); rr/green-wip.patch:477 (declaracion). Ninguna otra sustitucion; en cada archivo de Draft/, ningun token ForWrite (git grep/grep -w, vacio, y comprobado por inv/check-invariants.py). Lo demas se comparo con el paquete v4.5 (esqueleto de 6.1, API de D-PBT3-02 a D-PBT3-07, lineas de 5.2) y no difirio. rr/green-wip.patch crea archivos nuevos contra BaseSha y los archivos ya existian por el RED, asi que el contenido de sus ocho archivos se escribio sobre los del RED desde las lineas del parche (no git apply); el diff RED..GREEN toca siete archivos de Draft/ y ningun archivo de tests/.

ANCLAS DEL PASO 1 (leidas con git show sobre BaseSha): PushBackPilotMaterializer.cs:100-120 trae UnreadableOrFutureDiagnostic, KindDivergentDiagnostic, NoDesignDiagnostic, CompositeOutOfPilotReason, DormantTailOutOfPilotReason, CatalogRequiredDiagnostic, NonPilotContentReason, InvalidScaleReason, BaseChangedReason, MaterializeFailurePrefix y ComposeFailurePrefix; PushBackPilotIntentReader.cs trae Read (:75), BuildStaleBase (:64) y MaterializeDiagnostic (:53); I58F1Matrix.cs:97-102 trae Nested con las 28 rutas de pushback; PushBackFullEditorCharacterizationLateralCutTests.cs:99-119 y :127-133 con las ocho etiquetas y las obsoletas [], [4], [2], [], [], [2], []; la costura EditableStateForTest esta en RackPushBackSystemWindow.xaml.cs:197; DraftSession.KeepMineOnCurrentVersion existe; Fixtures/I64 tiene 17 fixtures. Las demas anclas (IntentKit, PilotKit.JsonDiff, I58F1Fixtures.Rich y Set, PushBackEditableStateOwnership Table y Of, destinos de CT-12) las ejercen la compilacion y las pruebas.

G-9 (C-2): RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests.G9NoFileOfThePluginOrOfApplicationThatNamesThePilotUsesAWriteApiOrALockOutsideTheFourExceptionsWithTheirExactCounts en Passed en el RED (suite completa de Core local r1-red-core-full, filtro Workspace y TRX de la CI del RED) y en el GREEN (suite completa de Core local g1-green-core-full, filtro Workspace y TRX de la CI del GREEN). En el RED, la suite completa de Core dio 19107 resultados con 10 Failed, todos de las cuatro clases PushBackFullDraft*; ninguna prueba existente fallo.

COMPROBACIONES SOBRE BaseSha..CurrentSha (inv/check-invariants.py, salida en inv/inv-green-final.txt, RESULTADO PASS): INV-PBT3A-01 solo ocho A en src/ de Draft/; INV-PBT3A-03 y PBT3A-C13 las declaraciones publicas e internas de los ocho archivos son exactamente las de D-PBT3-02 a D-PBT3-07 con sus constructores (la salida imprime cada declaracion con su archivo), y las privadas son metodos, un campo private static readonly string (Absent) y los constructores privados de la lista; INV-PBT3A-04 un namespace de bloque por archivo y ningun termino prohibido ni compositor de RackEmbedDocument; INV-PBT3A-05 PushBackPilotIntentReader.Read una vez y LoadExisting una vez; INV-PBT3A-06 ningun archivo de src/ fuera de Draft/, PP y UR nombra los tipos nuevos; C-1 sin ForWrite y con la firma de D-PBT3-05; INV-PBT3A-14 ninguna linea anadida muta el rack resuelto ni el informe de modulos (y CT-11 y CT-13 en verde).

AUTOPRUEBAS DE LAS TRES GUARDAS (CT-12 a 14, en el RED y en el GREEN, sobre texto inyectado en memoria, nunca escrito): CT-12 inyecta cada uno de los cinco nombres en WorkspaceHostCore.cs y en WorkspacePilotSupport.cs y la guarda salta (10 autopruebas); CT-13 autopruebas a, b, c, e, f1, h1, i y j en PushBackFullComposition.cs, las ocho saltan; CT-14 #nullable, el namespace y los trece terminos prohibidos, las quince saltan.

CONTEOS Y FILTROS: ver TestResults (una fila por corrida con RunRef unico) y la lista por filtro en Evidence. Las tres corridas consecutivas de los dos filtros con ExpectRed sobre el GREEN: g1, g2 y g3, todas en verde (Core 10/10 y UI 1/1). Suite completa de Core en local: RED 19107 con 10 Failed (los esperados); GREEN 19107 Passed. De UI solo se corrieron los filtros del contrato, nunca la suite completa.

CI: RED, run 44666242222 sobre """ + RED + """: tests failure (Core 19107 resultados, 19097 Passed y 10 Failed, los diez de las clases PushBackFullDraft*, todos por asercion), ui-tests failure (2451 resultados: 2429 Passed, 21 NotExecuted/omitidos y 1 Failed, CT-01 por asercion), build-ui success, build-plugin omitido (skipped por su needs), sin cuelgue; las tres guardas y G-9 en Passed. GREEN, run 44668584248 sobre """ + GREEN + """: los cuatro jobs en success; Core 19107 Passed y 0 Failed; UI 2451 resultados, 2430 Passed, 21 omitidos y 0 Failed, igual al previsto (Core + 13 = 19107; UI + 1 = 2451); G-9 en Passed. TRX descargados en """ + RUN + """/tests/redci/ y /tests/greenci/.

BUILDS RELEASE (los tres, por proyecto, con --no-incremental): linea base sobre BaseSha (b0), RED (r2) y GREEN (g4), 0 errores; avisos unicos por codigo identicos en los tres (Core 66: xUnit1026 10, xUnit2000 3, xUnit2010 2, xUnit2012 1, xUnit2013 17, xUnit2017 1, xUnit2029 16, xUnit2031 16; UI 14; Plugin 29 MSB3277); ninguno nuevo (tests/b0/warn-baseline.txt, tests/r2/warn-red.txt, tests/g4/warn-green.txt).

PRUEBA DE LIMPIEZA (antes, tras el RED y tras el GREEN, identicas; tests/clean-before.txt, clean-after-red.txt y clean-after-green.txt): disposicion del configurador (RackFrameConfigurator.layout de la carpeta RackCad bajo <datos-locales-del-usuario>) sha256 7AAB8FF7A3E5... y fecha UTC 2026-11-19T21:41:29Z sin cambio; carpeta de biblioteca (la resuelta por UserSettingsStore.ResolveDesignLibraryPath, la de Designs por defecto) fecha UTC 2026-10-13T21:42:51Z, 11 entradas y sha256 del listado recursivo ordenado 3A1B66FF... sin cambio. Sin cambios en el perfil.

ARBOLES (git write-tree): RED """ + open(RUN + "/tests/tree-red.txt").read().strip() + """; GREEN """ + open(RUN + "/tests/tree-green.txt").read().strip() + """. Las corridas locales del RED y del GREEN se hicieron sobre esos arboles antes de cada commit; las builds de la linea base llevan el arbol de BaseSha."""

evidence = [
    "RedSha " + RED + "; CurrentSha " + GREEN + "; BaseSha y ChainBaseSha " + BASE + "; ChainRedSha null y ChainRedFiles vacio (no hubo RED en el intento 1); un solo commit RED y un solo commit GREEN, sin commit de reparacion.",
    "Trailer real de los dos commits (git log --format=%B BaseSha..HEAD): " + TRAILER,
    "git diff --name-only BaseSha..CurrentSha: exactamente las 14 rutas del alcance; git diff --name-only RedSha CurrentSha: solo siete archivos de src/RackCad.Application/Systems/PushBack/Draft/ y ninguno de tests/.",
    "CI del RED: run 44666242222 (sobre RedSha): tests failure, ui-tests failure, build-ui success, build-plugin skipped, sin cuelgue; TRX en " + RUN + "/tests/redci/core/core.trx y " + RUN + "/tests/redci/ui/ui.trx; solo fallan las clases PushBackFullDraft* (10 de Core y 1 de UI), por asercion; PushBackDraftWriteGuardTests (3) y G-9 en Passed.",
    "CI del GREEN: run 44668584248 (sobre CurrentSha): los cuatro jobs en success; Core 19107 Passed; UI 2451 resultados (2430 Passed y 21 omitidos); TRX en " + RUN + "/tests/greenci/core/core.trx y " + RUN + "/tests/greenci/ui/ui.trx; G-9 en Passed.",
    "Salida de las comprobaciones de invariantes sobre BaseSha..CurrentSha: " + RUN + "/inv/inv-green-final.txt (RESULTADO PASS); sobre el arbol de trabajo antes del commit GREEN: " + RUN + "/inv/inv-wt.txt.",
    "Criterio del RED sobre los TRX locales: " + RUN + "/inv/check-red-trx.py (CRITERIO-RED=PASS: Failed == Selected == 10 en Core y 1 en UI, todos por asercion y con la primera linea del StackTrace bajo tests/).",
    "Conteos por filtro (Selected/Passed/Failed/Skipped), RED local y GREEN local: " + " || ".join(table),
    "Rutas relativas comprobadas con ls al componer la entrega (compose-tests.py afirma la existencia de cada ResultsFile): TRX y logs en " + RUN + "/tests/{r1,r2,g1,g2,g3,g4,b0,redci,greenci}/; guiones de solo lectura en " + RUN + "/inv/; copia de los parches del intento 1 en " + RUN + "/a1/.",
    "Los literales de la tabla de RR-3 sobre RedSha (inv/rr3-literals-red.txt): WindowParity 3 lineas; Composition 3 lineas y 3 valores; View 11 lineas; Guards 23 lineas; el recuento propio de Load y de Classifier difiere del de la delegacion por como se enumeran las lineas de CT-04 y las 27 de la parte (3) de CT-09 (ver UnexpectedFindings). No hubo reparacion, asi que la ayuda mecanica de RR-3 no se aplico.",
]

handoff = {
    "Schema": "rackcad-worker-handoff/v1",
    "TaskId": "F4B-PBT3A-FULL-WRITE-MODEL",
    "RunId": "R20261120T025240Z-f006",
    "DelegationRunId": "R20261120T024916Z-fc50",
    "Initiative": "I-64",
    "Gate": "F4B",
    "Attempt": 2,
    "BaseSha": BASE,
    "RedSha": RED,
    "CurrentSha": GREEN,
    "Branch": "architecture/workspace-persistente-rackcad",
    "Worktree": worktree,
    "Pushed": True,
    "FilesChanged": files,
    "WorkCompleted": work,
    "TestsExecuted": fragment["TestsExecuted"],
    "TestResults": fragment["TestResults"],
    "Evidence": evidence,
    "UnexpectedFindings": [
        "Los parches del intento 1 no se podian aplicar con git apply sobre el RED (crean archivos que ya existian); el GREEN se escribio sobre los archivos del RED con el contenido de rr/green-wip.patch. El resultado se verifico contra el paquete v4.5 y por el GREEN completo.",
        "Los recuentos de lineas de la lista de RR-3 que mide inv/rr3-literals.py sobre RedSha (Load 46, Classifier 37) no coinciden con los de la delegacion (49 y 10) por la manera de enumerar las lineas de CT-04 y las 27 de la parte (3) de CT-09; ademas, el fragmento ' reglas-sin-coincidencias=True' de CT-13 aparece sin el espacio inicial en la linea de la prueba (log.Add(\"reglas-sin-coincidencias=\" + ...)). No hubo reparacion, asi que no afecta; lo senalo por si la verificacion cuenta esos literales.",
        "Para esperar las corridas largas use pausas cortas de Python en primer plano (unas pocas, de 20 a 170 segundos), aunque el prompt pide no usar sleep ni procesos en segundo plano; el arnes paso por si solo a segundo plano varias corridas de mas de 180 segundos (gh run watch y las corridas de filtros) y las espere hasta su notificacion. No lance ningun proceso aparte.",
        "En TestsExecuted, Command muestra 'dotnet' por el dotnet.exe de usuario que se uso (el del PATH no sirve), con las rutas relativas del worktree; las corridas filtradas locales tienen Selected == Total.",
        "El Release del Plugin se compilo a la carpeta de resultados (plugin-out) de cada fase; no se compilo a su carpeta bin ni se toco AutoCAD.",
    ],
    "KnownLimitations": [
        "No se ejecuto AutoCAD: lo que no se puede comprobar sin el (el modelo no esta conectado al panel, el borrado en el dibujo, Deshacer, Smoke-PB) queda para PB-T4 y el Smoke-PB.",
        "Queda para PB-T3b la generalizacion en su sitio del plan y la corrida (PP y UR, la interfaz de borrado) y para PB-T3c el borrado en el dibujo (WR y la fachada); PB-T3a solo crea el modelo en Draft/ sin conectarlo al panel, al piloto, al anfitrion, a la ventana ni al Plugin.",
        "Interino declarado por el paquete: PushBackEditableStateSeat.Pass duplica la adopcion de Recompute de la ventana (solo CT-PBT3A-01 las ata) y Draft/ usa el lector y las constantes de Pilot/ (dependencia ciclica que ninguna guarda ve) hasta PB-T4.",
        "De UI solo se corrieron los filtros del contrato, uno a uno; nunca la suite completa de UI en local; la CI corrio las suites completas.",
        "No puedo observar el uso del equipo durante las corridas; no hice nada interactivo mientras corrian.",
    ],
    "Deviations": [],
    "OpenQuestions": [],
    "WorkerStatus": "IMPLEMENTATION_COMPLETE",
    "Disposition": "NONE",
    "TriggeredStopConditions": [],
    "RecommendedNextAction": "La sesion ejecuta la verificacion con las tres comprobaciones deterministas de F4B-E15, las revisiones y los controles nc1 a nc3; PB-T3b va despues, con delegacion nueva.",
    "Worker": {"Provider": "Anthropic", "ModelRequested": "claude-sonnet-5-5", "EffortRequested": "high", "Trailer": TRAILER},
}
json.dump(handoff, open(RUN + "/worker-handoff.json", "w", encoding="utf-8", newline="\n"), indent=1, ensure_ascii=False)
print("escrito", len(json.dumps(handoff)))
