I-64 - DELEGACION F2-T2B-NAVIGATION-UI - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F2; tarea F2-T2B-NAVIGATION-UI; intento 2; RunId R20261110T044730Z-20c9;
           DelegationRunId R20261110T044730Z-20c9; AuthorityRevision e67944220d76a760615f5fc816d44d36fff743b6 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 2deb0454e4c41c15e02382437176ecbb9bbb4eda, hijo directo de la AuthorityRevision:
           solo anade el contrato emitido, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T044730Z-20c9
Contrato de gate: artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/2/R20261110T044730Z-20c9/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F2-T2B-NAVIGATION-UI/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) D-01, D-02, D-03, D-05, D-07, INV-03, INV-14, INV-22, INV-36, INV-38, INV-40,
           section 11 (fila F2) y section 12.3; el paquete docs/initiatives/I-64-f2-t2-gate-contract-package.md; las decisiones autonomas; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Workspace (5 archivos) ; git ls-files tests/RackCad.UI.Tests/Workspace (7 archivos) ; y lee el codigo y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F2T2-C* y F2T2B-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 95 y section 96 de docs/automation/evidence/I-64-evidence.md)
- F2-T2A-NAVIGATION-HOST esta CERRADA (producto verificado 44a692a4): puerto IWorkspaceNavigationPort, host, hechos, efectos y eco. Esta tarea es la capa WPF.
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Proyectos: tests/RackCad.UI.Tests prueba src/RackCad.UI (WPF, net8.0-windows; ayudante StaTestRunner); tests/RackCad.Tests prueba Domain y Application y contiene
    GUARDAS DE TEXTO sobre fuentes del Plugin y de la UI (WorkspaceHostSource: Code, Text, CodeOnly, Files). El Plugin solo se compila en la CI.
  * Linea base (Release): FullyQualifiedName~RackCad.UI.Tests.Workspace = 64 y FullyQualifiedName~RackCad.Tests.Workspace = 756, todas en verde.
    tests/RackCad.UI.Tests/Workspace usa WorkspacePortDouble (fuera del alcance) y unas 40 llamadas new WorkspacePanelViewModel(port, _ => { }) que NO se pueden tocar:
    el constructor debe seguir aceptando dos argumentos (por ejemplo, un tercer parametro opcional IWorkspaceNavigationPort navigation = null).
  * Puerto (T2A, src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs): Task<WorkspaceNavigationResult> ExecuteAsync(WorkspaceRequest origin, string rackId,
    NavigationActionKind action, int copyIndex = 0, NavigationViewOrder? view = null, NavigationActionKind locateWith = NavigationActionKind.View). WorkspaceNavigationResult es
    readonly record struct (Status, Verdict, RefreshRequired, Reason) con estados Executed, Unavailable, Rejected, Stale, Failed. WorkspaceRequest(SessionId Origin).
    NavigationActionKind {Select, View, Center, LocateView}; NavigationViewOrder {Frontal, Lateral, Planta, Other}. WorkspaceNavigationRunner.Run planifica con la politica; TargetCount se
    calcula ahi desde NavigationTargets.Resolve(origin, rackId).Targets: todos los objetivos para View y Center, los de la vista pedida para LocateView, null o el total para Select segun fije la delegacion.
  * Snapshot: WorkspaceHostSnapshot trae ActiveSession (SessionId?), Actions (CanReadDrawing, CanWriteDrawing, Block) y Selection (Kind, RackId...).
  * Controlador: WorkspaceController.Navigation expone IWorkspaceNavigationPort (se crea en el primer acceso); WorkspacePaletteHost construye new WorkspacePanelViewModel(_controller.Port, _controller.SetInstrumentationEnabled).
  * GUARDAS que cambian (deltas exactos en el RED, todas en tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs): linea 188 (regex del constructor en el host);
    linea 533 (regex del constructor del view-model); TheUiFolderHasNoTimerNoPollingNoThreadNoAsyncAndNoGlobalExceptionHandler (linea 541, INV-F2T2B-08: excepcion por archivo exacto con recuentos).
    Otras guardas de la carpeta UI siguen vigentes: TheUiFolderWritesNothingAndCarriesNoMetricsConceptOrNamesOfI63 prohibe sin distinguir mayusculas Metric, Provider, Bom, ProjectSummary, TotalRacks, Aggregat, ComputedParameter;
    TheUiFolderUsesOnlyTheNeutralContractsOfTheApplicationWorkspace prohibe Application.Current, GetOrCreate, Singleton, .Instance y campos static no readonly string;
    TheRootControlReachesTheViewModelOnlyThroughExplicitInteractionsAndContainsThem exige catch (System.Exception en el code-behind y prohibe _viewModel.Refresh(, Loaded, IsVisibleChanged, SizeChanged, CompositionTarget;
    TheUiCodeFilesDeclareOneBlockScopedNamespaceOfTheirFolder exige namespace RackCad.UI.Workspace por bloque. WorkspaceModelBoundaryTests recorre src/RackCad.Application/Workspace/.
  * La vista WorkspacePanelView.xaml es un ScrollViewer con StackPanel; AppStyles aporta SectionTitle y SecondaryButtonStyle. El view-model del panel hereda de ObservableObject.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = PRIMERO los 4 archivos nuevos del contrato, exactamente y en su orden:
    src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationViewTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationPortDouble.cs
  DESPUES, la lista EXPLICITA de los archivos existentes que el diseno cambia de verdad, como SUBCONJUNTO EXACTO de estos 7 candidatos y en este mismo orden:
    src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
    src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs
    src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
  Para cada archivo existente elegido, una entrada de AcceptanceCriteria con su delta exacto y la razon. Nunca anadas una ruta fuera del contrato ni un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/2/R20261110T044730Z-20c9/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/2/R20261110T044730Z-20c9/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro: sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (62 entradas). RequiredTests con los mismos dos elementos.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F2T2-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F2-T2B-NAVIGATION-UI)
- RunId = R20261110T044730Z-20c9 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F2"; TaskId = "F2-T2B-NAVIGATION-UI".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T044730Z-20c9" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria (los redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5, del contrato y de los HECHOS DEL REPOSITORIO; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de las rutas exactas del AllowedWriteScope;
  (2) RED primero: commit RED con esqueletos que compilan, pruebas nuevas que fallan por asercion y deltas exactos de guardas; push del RED antes del GREEN; el GREEN no toca los archivos de prueba del RED;
  (3) la API exacta del view-model de navegacion: una entrada asincrona unica contenida, comandos o metodos para Seleccionar, Ver, Centrar, Localizar con vista explicita y copia siguiente, propiedades de habilitacion y de texto;
  (4) habilitacion segun INV-F2T2B-02 y captura atomica de sesion y RackId del snapshot presentado segun INV-F2T2B-03;
  (5) copia k/M segun INV-F2T2B-04 y TargetCount segun INV-F2T2B-07, sin cambios en estados, veredictos, efectos ni en WorkspaceNavigationHost.cs;
  (6) textos en espanol por estado y un unico refresco con RefreshRequired segun INV-F2T2B-06;
  (7) excepcion exacta de la guarda de async con sus recuentos segun INV-F2T2B-08;
  (8) controles sin anchos fijos y sin ventanas nuevas segun INV-F2T2B-09;
  (9) cableado exacto en WorkspacePaletteHost.cs segun INV-F2T2B-12;
  (10) en el GREEN: los dos filtros y las suites completas de Core y UI en verde; builds Release sin avisos nuevos; CI exacta 4/4;
  (11) worker-handoff.json conforme al esquema en ExpectedHandoffPath;
  (12) sin AutoCAD, sin comandos nuevos, sin F3/F4/F6 ni inventario global.
- IssuedBy = "Controller Codex (planificacion F2-T2B-NAVIGATION-UI, primera delegacion de la cadena, CONTROLLER_PLANNING)".
