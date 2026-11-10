I-64 - DELEGACION F3-T2-BROWSER-UI - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F3; tarea F3-T2-BROWSER-UI; intento 1; RunId R20261110T145323Z-22d9;
           DelegationRunId R20261110T145323Z-22d9; AuthorityRevision f6923a288c6d65deb096cd3b3c00399d158c136c (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 8757c50b46b668ca454395a2f8ab87f4715c1b1a, hijo directo de la AuthorityRevision:
           solo anade el contrato emitido, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T145323Z-22d9
Contrato de gate: artifacts/orchestration/I-64/F3-T2-BROWSER-UI/1/R20261110T145323Z-22d9/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F3-T2-BROWSER-UI/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 20 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada), las secciones del campo Authorities; docs/initiatives/I-64-f3-gate-contract-package.md (su seccion 6 manda); la orden del Owner; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Workspace (6 archivos) ; git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; y lee el codigo y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F3T2-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" en el texto libre
Trailer: no aplica (no hay commits)
Informe esperado: la salida exacta del esquema rackcad-delegation/v1 (la escribe el CLI por -o)

PERFIL CONTROLLER_PLANNING
Metodo: clasificar y enrutar dentro del contrato de gate, sin escribir en Git.
1. Lee el contrato, docs/automation/agent-execution/routing.md y el catalogo.
2. Clasifica la tarea, puntua las siete dimensiones y elige entre las celdas del
   contrato el nivel mas bajo adecuado; registra RoutingReason con fechas.
3. Copia del contrato el alcance, los invariantes, las pruebas y las paradas; nunca
   los amplies.
4. Rellena el esquema de delegacion completo; null solo donde el esquema lo admite.

CONTEXTO (hechos versionados en el HEAD; evidencia section 102 a section 105 de docs/automation/evidence/I-64-evidence.md)
- F3-T1-BROWSER-CORE esta cerrada (producto a92039bb): BrowserSession y BrowserSessionRegistry. Esta tarea conecta ese nucleo al panel.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Proyectos: tests/RackCad.UI.Tests prueba src/RackCad.UI (WPF, net8.0-windows; ayudante StaTestRunner); tests/RackCad.Tests prueba Domain y Application y contiene GUARDAS DE TEXTO sobre fuentes del Plugin y de la UI (WorkspaceHostSource: Code, Text, CodeOnly, Files). El Plugin solo se compila en la CI y con la build local indicada al Worker.
  * Linea base (Release): FullyQualifiedName~RackCad.UI.Tests.Workspace = 155 y FullyQualifiedName~RackCad.Tests.Workspace = 855, todas en verde.
  * Forma publica pedida: en Application, interfaz nueva IWorkspaceBrowserSummaryPort con BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId) y bool IsSessionAlive(SessionId session); enum BrowserSummaryStatus {Ready, NotFound, Busy, Rejected, Failed}; sealed record BrowserRackSummary(Status, Name, Kind, Views, Reason);
    sealed record BrowserRackFact(IsInterpretable, EnvelopeId, Kind, View, Name); clase pura BrowserRackSummaries con constructor (funcion de lectura de hechos por SessionId, funcion de vida de sesion), Read(SessionId, rackId), OnDrained(DrainOutcome) y EndSession(SessionId).
    En UI, WorkspaceBrowserViewModel con Tabs, ActiveRackId, LiveSurfaceCount, HasSurface, SurfaceTitle, SurfaceKindText, SurfaceViewsText, SurfaceStatusText, Present(sesion, contexto, canRead, block, blockText), ActivateTab, PinTab y CloseTab, y WorkspaceBrowserTabItem en el mismo archivo.
  * Decision de implementacion dada: la comparacion de RackId en BrowserRackSummaries es sin distinguir mayusculas, como en NavigationTargets. Nombre y Kind: el primero no vacio entre los hechos interpretables de ese RackId, en orden de lectura. Vistas: distintas, no vacias, en orden determinista. Sin hechos interpretables: NotFound.
  * GUARDAS que exigen delta exacto en el RED:
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs linea 536 (TheUiFolderUsesOnlyTheNeutralContractsOfTheApplicationWorkspace: regex del constructor de 3 parametros; insertar el cuarto parametro opcional antes del cierre) y linea 190 (TheViewModelIsFedOnlyByTheInjectedPortAndTheNeutralToggleDelegate: regex de new WorkspacePanelViewModel; anadir _controller.BrowserSummary);
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs linea 947 (ThePaletteHostOnlyGainsTheNavigationPortAsTheThirdArgumentAndNothingElse: misma regex). El resto de esa prueba sigue: _controller.Navigation una vez, cuatro using, dos +=, y sin nombrar el tipo del puerto en el palette host.
  * GUARDAS que se respetan sin delta (no choques con ellas):
    PG 546-559: en src/RackCad.UI/Workspace/ prohibidos Task, async, await, Timer, Dispatcher, Thread y Sleep salvo en WorkspaceNavigationViewModel.cs; WorkspaceBrowserViewModel.cs es sincrono y no contiene la palabra Task. PG 578: Metric, Provider, Bom como inicio de palabra, ProjectSummary, TotalRacks, Aggregat, ComputedParameter, sin distinguir mayusculas, en .cs y .xaml.
    PG 589: namespace RackCad.UI.Workspace de bloque. NH 866: la carpeta UI no nombra NavigationDefinitionFact, NavigationTargets, NavigationTarget ni WorkspaceNavigationHost. NH 919: la carpeta UI no usa Keyboard, FocusManager, SetFocus, .Focus( ni InputManager. NH 885: WorkspaceNavigationViewModel.cs no cambia.
    NH 895: new WorkspaceNavigationViewModel(navigation, Refresh) una sola vez. NH 911 y PG 616: el .xaml.cs no contiene Navigation.Present(, _viewModel.Refresh(, Loaded, IsVisibleChanged ni SizeChanged. NH 933: sin Width ni Height en la seccion de navegacion del xaml; la seccion nueva va antes de ella.
    NH 686: el Plugin mantiene exactamente nueve archivos en Workspace. NH 92: IWorkspaceNavigationPort sigue con un metodo. NH 422: IWorkspaceNavigationPort es la primera base del host. NH 528: RackLog.Exception una sola vez en el host. NH 555-560: ScanEnvelopes( una sola vez en WorkspaceNavigationFacts.cs y el adaptador no contiene .ReadDefinitionFacts(. NH 672-676, HG 191, PG 411: new WorkspaceNavigationHost( una sola vez, tres += y cero -= en el controlador.
    AU 98: .ReadDefinitionFacts( una sola vez en todo el producto, en NavigationTargets.cs. AU 112: BrowserRackFact no lleva handles y la clase pura no nombra ModelSpaceReferenceHandles. HG 428 y NH 443: la cadena .Result prohibida en el Plugin, asi que la lectura de outcome.Result queda en Application. BG 142 y 159, EX 185, HG 293: el host no usa GetObject, SelectImplied, RackBlockData ni RackEmbedStore. BG 199 y HG 457: LockDocument y StartTransaction solo en la misma linea que using. PG 644: exactamente 45 Fact o Theory en WorkspaceHostSourceGuardTests.cs, no se anaden pruebas alli.
    WorkspaceModelBoundaryTests: tipos de Application sin Tab, Subscription, Editor ni Draft en el nombre salvo los dos de F3-T1; nada de Transaction, LockDocument, Database, System.IO ni Commit( en el codigo de Application.
  * Constructor existente: new WorkspacePanelViewModel(port, setInstrumentation, navigation = null); hay 41 construcciones en pruebas (34 con 2 argumentos y 7 con 3) que NO se tocan: el cuarto parametro es opcional. WorkspaceNavigationViewModel.Present(session, kind, rackId, canRead, block, blockText) no cambia de firma.
  * Refresh se dispara tambien dentro de CommandWillStart: por eso el resumen se condiciona a CanReadDrawing y ActionBlock None, y el host comprueba el reposo.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = PRIMERO los 9 archivos nuevos del contrato, exactamente y en su orden:
    src/RackCad.Application/Workspace/IWorkspaceBrowserSummaryPort.cs
    src/RackCad.Application/Workspace/BrowserRackSummaries.cs
    src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserSummaryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserSummaryPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserScenarioHarnessTests.cs
  DESPUES, los 12 archivos existentes candidatos, TODOS y en este mismo orden, para que un delta de guarda imprevisto no bloquee la tarea; un candidato que no necesite cambio no se toca:
    src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
    src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs
    src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs
  Para cada archivo existente, una entrada de AcceptanceCriteria con su delta y la razon, o la indicacion de que solo cambia si una guarda lo exige. Nunca anadas una ruta fuera del contrato ni un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F3-T2-BROWSER-UI/1/R20261110T145323Z-22d9/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F3-T2-BROWSER-UI/1/R20261110T145323Z-22d9/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (89 entradas). RequiredTests con los mismos dos elementos.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F3T2-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F3-T2-BROWSER-UI)
- RunId = R20261110T145323Z-22d9 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F3"; TaskId = "F3-T2-BROWSER-UI".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F3-T2-BROWSER-UI/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T145323Z-22d9" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria (los redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5, del contrato y de los HECHOS DEL REPOSITORIO; como minimo uno por cada invariante INV-F3T2-01 a INV-F3T2-20, y ademas:
  git diff --name-only BaseSha..CurrentSha dentro de las rutas exactas del AllowedWriteScope; RED primero, con push del RED antes del GREEN, y el GREEN no toca los archivos de prueba del RED;
  en el GREEN: los dos filtros y las suites completas de Core y UI en verde, builds Release sin avisos nuevos y CI exacta 4/4; worker-handoff.json conforme al esquema en ExpectedHandoffPath; sin AutoCAD.
- IssuedBy = "Controller Codex (planificacion F3-T2-BROWSER-UI, primera delegacion de la cadena, CONTROLLER_PLANNING)".
