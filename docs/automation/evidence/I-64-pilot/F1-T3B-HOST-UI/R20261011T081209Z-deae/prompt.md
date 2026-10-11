I-64 - DELEGACION F1-T3B-HOST-UI - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F1; tarea F1-T3B-HOST-UI; intento 3; RunId R20261011T081209Z-deae;
           DelegationRunId R20261011T081209Z-deae; AuthorityRevision c7f389cbb2eff4be42f861d8114adc2af753a1c9 (commit de la decision del Coordinator);
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree (se espera f3a9f40e0d0057e14a6eb7a6681590b78cecf37f, hijo de la AuthorityRevision:
           solo anade la revision del paquete NOT EMITTED, la evidencia Sec.55 y el estado; 16.3 admite AuthorityRevision ancestro de BaseSha);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261011T081209Z-deae
Contrato de gate: artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T081209Z-deae/gate-contract.json (ASCII puro; emitido por orden del Coordinator, decision custodiada en
           docs/automation/evidence/I-64-pilot/F1-T3B-HOST-UI/coordinator-decision-t3b-contract.md)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los del contrato (INV-F1-T3B-01..16); una delegacion nunca amplia el contrato
Rutas y archivos calientes: exactamente las 8 entradas de AllowedWriteScope del contrato: archivos NUEVOS en src/RackCad.UI/Workspace/
           y tests/RackCad.UI.Tests/Workspace/; los archivos nuevos exactos src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs y
           tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs; y los deltas exactos de
           src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs (INV-F1-T3B-03), src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
           (INV-F1-T3B-04), src/RackCad.Plugin/Workspace/WorkspaceController.cs (INV-F1-T3B-05) y
           tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs (solo G1/G2/G3, INV-F1-T3B-14). Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) D-01, D-02, D-03, D-04, D-06, D-17, D-18, Sec.10 y
           Sec.11; la decision del Coordinator citada arriba; el contrato. El paquete docs/initiatives/I-64-f1-t3b-gate-contract-package.md
           NO es autoridad: si lo lees, solo vale su seccion 15 (la opcion C de sondeo de sus secciones anteriores quedo RECHAZADA).
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (5 archivos de T2/T3A) ; git ls-files src/RackCad.UI/Workspace (0 archivos) ;
           git ls-files tests/RackCad.UI.Tests/Workspace (0 archivos) ; git ls-files tests/RackCad.Tests/Workspace (22 archivos de T1/T2/T3A)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y C-01..C-20 y C-T3-INSTRUMENTATION del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" en el texto libre
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.51 a Sec.55 de docs/automation/evidence/I-64-evidence.md)
- F1-T1-MODEL, F1-T2-BRIDGE y F1-T3A-HOST-CORE estan cerradas por el Coordinator; su producto existe en este HEAD y no se reabre
  salvo los tres deltas exactos que autoriza este contrato. T3A dejo la base no visual: puerto, controlador perezoso, RACKPANEL,
  lector fino, politicas e instrumentacion en memoria. T3B es el host visual sobre esa base.
- Presupuesto de T3B (decision del Coordinator, Sec.54): 1 salida valida por fase; Attempt 3, AttemptsRemaining 0,
  MaxReworkLoops 3; CorrectionsAuthorized false: toda entrega que exija correccion o REWORK es STOP S-11. La delegacion debe dejar
  al Worker todo lo necesario para acertar a la primera.
- El Coordinator RECHAZO el refresco por sondeo (sin DispatcherTimer, System.Threading.Timer, sondeo periodico ni GetSnapshot por paso
  del tiempo) y fijo un RELEVO POR EVENTOS: AutoCadEventBridge dispara una notificacion interna ActiveSessionChanged desde sus
  manejadores existentes de DocumentActivated (y DocumentToBeDestroyed si cambia la sesion activa) tras actualizar el nucleo;
  WorkspaceController se suscribe a Drained y a ActiveSessionChanged, llama a RefreshPanel solo con el panel visible y expone
  StateChanged; WorkspacePaletteHost se suscribe a StateChanged y entonces el view-model consulta GetSnapshot / GetMeasurementReport.
- HECHOS DEL REPOSITORIO medidos por la sesion (paquete, secciones 3, 4 y 15; inventario de solo lectura), relevantes para
  AcceptanceCriteria:
  * Proyectos de prueba: tests/RackCad.Tests referencia Domain y Application; tests/RackCad.UI.Tests referencia Domain, Application y
    UI; ninguno referencia el Plugin. El Plugin se comprueba con guardas de texto que leen sus fuentes desde la raiz del repositorio
    (RackCad.sln); la UI se prueba de verdad en RackCad.UI.Tests con StaTestRunner.Run (un hilo STA compartido con una Application
    vacia y ShutdownMode OnExplicitShutdown), construyendo los controles dentro de la llamada.
  * Superficie de T3A: IWorkspaceHostPort (GetSnapshot, Authorize, GetSelectionContext, GetMeasurementReport), de solo lectura y SIN
    evento (no cambia); WorkspaceHostSnapshot(Status, ActiveSession, PanelVisible, Actions, Selection, PickFirstDisabled,
    InstrumentationEnabled); WorkspaceHostStatus {NotStarted, NoDocument, Ready, Degraded}; WorkspaceActionBlock {None, NotStarted,
    NoDocument, Degraded, ModalActive, CommandActive}; WorkspaceActionAvailability(CanReadDrawing, CanWriteDrawing, Block);
    SelectionContextKind {None, One, Selection, NoIdentity, Diagnostic} con RackId (solo One), RackCount y OtherCount;
    WorkspaceMeasurementReport(Enabled, Points, CurrentProcessMemoryBytes) y WorkspaceMeasurement(Sequence, Point, Milliseconds,
    ProcessMemoryBytes, Count). WorkspaceController: GetOrCreate, IsCreated, Port, PanelVisible, OpenPanel, ShowPanel, HidePanel,
    RefreshPanel, SetModalRackCadActive, SetInstrumentationEnabled; OnDrained privado suscrito a _bridge.Drained; LogFault estatico.
  * AutoCadEventBridge.cs: su superficie PUBLICA la fija WorkspaceBridgeFacadeSourceGuardTests (prueba de T3A NO modificable):
    ActiveSession, Authorize, Dispose, Drained, RequestReconciliation, SetModalRackCadActive, SetPanelVisible, Start, TryGetSession;
    por eso ActiveSessionChanged es internal. La misma prueba exige 16 +=, 16 -= y 16 lineas "handler = (", cada una con Contain( o
    ContainSession( y sin RequestReconciliation; Start y Dispose sin RequestReconciliation. Los manejadores de DocumentActivated y
    DocumentToBeDestroyed son lambdas de una linea Contain("X", () => _core.OnX(...)). El archivo NO tiene #nullable: una anotacion
    de referencia con ? produce CS8632 y rompe el job del Plugin. EventBridgeCore.cs no cambia (guarda
    StartAndActivationOfTheCoreDoNotQueueAReconciliation).
  * WorkspacePanelCommand.cs: la guarda TheCommandOnlyObtainsTheControllerAndOpensThePanel exige WorkspaceController.GetOrCreate() y
    .OpenPanel() y prohibe new WorkspaceController, PaletteSet, AddVisual, System.Windows, AutoCadEventBridge, StartTransaction,
    LockDocument, SelectImplied y GetObject en el comando; ademas catch (System.Exception y RackCommandSupport.Report(; es el unico
    archivo de Plugin/Workspace que puede usar WriteMessage y el unico con [CommandMethod. Ninguna prueba fija el texto del mensaje.
  * Guardas de T3A sobre TODO archivo de src/RackCad.Plugin/Workspace/ y src/RackCad.Application/Workspace/ (texto sin comentarios):
    NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType prohibe Autodesk.AutoCAD.Internal|PaletteSet|AddVisual|System.Windows|
    RackCad.UI|Autodesk.AutoCAD.Windows|UserControl|Dispatcher (G1 lo acota para WorkspacePaletteHost.cs); NoHostFileStartsA
    BackgroundThreadOrAsyncWork prohibe Task.Run, Thread, ThreadPool, Timer, async, await, Parallel, BackgroundWorker y
    SynchronizationContext; sin System.IO, Registry, UserSettings, XData, NOD ni Environment.SetEnvironmentVariable; sin constructor
    estatico "static Workspace...()", ModuleInitializer, IExtensionApplication ni [assembly; sin Guid.NewGuid; sin SetImpliedSelection,
    Zoom, Regen, Highlight, SendStringToExecute; RackLog. solo dentro de "static void LogFault(...)" y nunca RackLog.Warning; ningun
    tipo cuyo nombre contenga Log (tambien Dialog, Catalog o Logic); nada que contenga Metric, Provider, Aggregat, ProjectSummary,
    TotalRacks o Bom sin distinguir mayusculas (tambien IFormatProvider, AggregateException o Symmetric); ningun campo de tipo
    Transaction, DBObject, Document, Database, Entity, BlockReference, ObjectId ni Editor. TheControllerSubscribesOnlyToTheDrainedEvent
    OfTheBridge exige hoy un solo += (G3 lo lleva a dos). NothingOutsideThePluginWorkspaceReachesTheController busca hoy
    WorkspaceController|WorkspaceHostPort|WorkspaceSelectionReader|WorkspaceHostCore SIN limites de palabra en todo src/ fuera de las
    dos carpetas Workspace: IWorkspaceHostPort en la UI fallaria hasta G2.
  * Guardas de T2 sobre todo archivo de src/RackCad.Plugin/Workspace/: += y -= sobre eventos gobernados de AutoCAD solo en
    AutoCadEventBridge.cs (PaletteSet.StateChanged no es gobernado); prohibidos como palabra completa SetImpliedSelection,
    SetCurrentView, Zoom, PanTo, UpdateScreen, Regen, SendStringToExecute, StartCommand, GetSelection, GetEntity, GetString, GetPoint,
    GetKeywords, GetInteger, GetDouble, SelectImplied, SelectAll, SelectAtPoint, ShowModalDialog, ShowModelessDialog, ShowDialog,
    MessageBox, GetObject, OpenCloseTransaction, Highlight y Unhighlight; ninguna linea public o protected cuya firma nombre Document,
    DocumentCollection, Editor, Database, Transaction, DBObject, ObjectId, Entity, Autodesk o un tipo *EventArgs (el manejador de
    StateChanged de la paleta debe ser privado); sin Save(, SaveAs, Registry., UserSettings, DocumentExtension ni .Commit(;
    WorkspaceSourceGuardExceptionTests mantiene exactamente dos excepciones de archivo exacto en la guarda de T2 (no se toca); ningun
    archivo del Plugin nombra WorkspaceSessionRegistry. PluginInitializer.cs no puede contener Workspace, RACKPANEL, Controller ni
    EventBridge.
  * Censos: los de comandos (CustomPropertiesCommandGuardTests TGrd08 y SelectiveEditorOpenTests, total 38) leen el texto crudo de
    src/RackCad.Plugin/**/*.cs, comentarios incluidos: un comentario con [CommandMethod(" en un archivo nuevo los rompe.
    WindowCensusGuardTests cuenta subclases concretas de Window (31): un UserControl no lo cambia. RACKPANEL no esta en la ayuda
    (CustomPropertiesHelpCensusTests) y no se anade. No hay censo de XAML ni de controles.
  * Frontera UI/AutoCAD (todas recorren el proyecto entero): "Autodesk.AutoCAD" ausente de toda linea no comentada de src/RackCad.UI
    (literales incluidos); \bAutodesk\. ausente del codigo; el cierre de referencias de RackCad.UI.csproj es solo WPF; las pruebas de
    ensamblado exigen que RackCad.UI no referencie ningun ensamblado Ac* ni Autodesk.
  * NamespaceFolderGuardTests: src/RackCad.UI/Workspace/X.cs -> "namespace RackCad.UI.Workspace" de bloque, en la columna 0, uno
    solo (no file-scoped); los archivos de prueba declaran exactamente RackCad.UI.Tests o RackCad.Tests (file-scoped permitido); no
    usar una carpeta Systems/. UiSystemBoundaryGuardTests: cada .xaml con x:Class = RackCad.UI.Workspace.<Archivo> y su .xaml.cs con
    ese namespace; toda URI /RackCad.UI;component/... apunta a un archivo existente y todo Source="...xaml" lleva ;component/.
  * Recursos: no hay App.xaml; las ventanas fusionan <ResourceDictionary Source="/RackCad.UI;component/Themes/AppStyles.xaml" /> en
    sus MergedDictionaries (ejemplo: src/RackCad.UI/RackListWindow.xaml); RackCad.UI.Shell.ShellResources (internal) expone el
    diccionario compartido para codigo; un DynamicResource no cae al diccionario del tema. RackCad.UI no tiene ningun UserControl;
    Nullable esta deshabilitado en RackCad.UI (Directory.Build.props); base RackCad.UI.ObservableObject. Precedente a NO copiar:
    RackFrameConfiguratorWindow se suscribe a Dispatcher.UnhandledException (manejador global prohibido para el panel, C-19).
  * Filtros: FullyQualifiedName~RackCad.UI.Tests.Workspace selecciona hoy 0 (clases Workspace* en el namespace RackCad.UI.Tests);
    FullyQualifiedName~RackCad.Tests.Workspace selecciona hoy 486, todas en verde (clases Workspace* en el namespace RackCad.Tests).
  * PaletteSet vive en Autodesk.AutoCAD.Windows (AcMgd); el Plugin ya referencia AcMgd y RackCad.UI y la CI lo compila con AutoCAD.NET
    25.0.1 y verify-autocad-references.ps1 -NoWarnings. Las firmas exactas (constructor solo con nombre, AddVisual, StateChanged,
    Visible, KeepFocus) las confirma la compilacion. En esta estacion verify-autocad-references.ps1 aborta (AutoCAD instalado): el
    equivalente local es una build Release --no-incremental del Plugin a un directorio temporal bajo artifacts/, con 0 errores y solo
    los 2 avisos MSB3277 de la linea base.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T081209Z-deae/gate-contract.json son ASCII puro: los diacriticos se
  eliminaron a proposito (por ejemplo "sesion", "Seleccion", "planificacion", "ambiguedad"). Se copian byte a byte.
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective
  son DATOS OPACOS de ese contrato. No los copies a mano: CARGALOS desde el archivo con una orden y reproducelos en tu salida
  a partir de esa carga, por ejemplo con python desde pwsh:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T081209Z-deae/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','AllowedWriteScope','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- Los valores Section de Authorities son la transliteracion ASCII de los encabezados (en Git los documentos conservan sus
  tildes): copia el valor del contrato, nunca el encabezado del documento.
- TODA tu salida debe ser ASCII puro, tambien en los campos que redactas tu (TaskClass, RoutingReason, AcceptanceCriteria,
  IssuedBy): escribe sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun
  tabulador ni otro caracter de control dentro de las cadenas. La unica excepcion son los saltos de linea que ya trae el
  Objective del contrato (en JSON, barra invertida + n).
- Antes de emitir: comprueba que, al decodificar tu salida, cada uno de esos campos es identico (cadena por cadena) al del
  contrato, y que tu salida no contiene ningun byte no ASCII, ninguna barra invertida seguida de u y ningun tabulador; si no,
  corrige tu salida volviendo a usar la carga, nunca a mano.
- No estreches AllowedWriteScope (8 entradas) ni expandas ForbiddenWriteScope (146 entradas). RequiredTests con los mismos
  dos elementos (Project, Filter, MinSelected y ExpectRed).

TASKCLASS (decision del Coordinator)
- TaskClass = exactamente "Implementacion transversal a capas (corta)" (fila "Implementacion transversal a capas" de routing.md
  Sec.1, variante corta, en ASCII); PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason, con las
  siete dimensiones y la fecha de verificacion de la entrada del catalogo que uses. Si el alcance te resultara Long-horizon: para
  con C-20 explicado en RoutingReason.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "high"; Effort.Semantic = "Deep";
  ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F1-T3B-HOST-UI; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261011T081209Z-deae (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T3B-HOST-UI" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (c7f389cb...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T081209Z-deae" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5 congelada
  (D-01, D-02, D-03, D-04, D-06, D-17, D-18, Sec.10), del contrato, de la decision del Coordinator y de los HECHOS DEL REPOSITORIO
  de arriba; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de las 8 entradas de AllowedWriteScope: archivos NUEVOS en
  src/RackCad.UI/Workspace/ y tests/RackCad.UI.Tests/Workspace/, los dos archivos nuevos exactos y solo los deltas exactos de
  WorkspacePanelCommand.cs, AutoCadEventBridge.cs, WorkspaceController.cs y WorkspaceHostSourceGuardTests.cs (INV-F1-T3B-03, 04, 05
  y 14); ningun otro archivo de producto o de prueba de T1/T2/T3A cambia;
  (2) RED primero en los DOS proyectos requeridos: pruebas compilables, seleccionadas por cada filtro (UI >= 1; Core >= 487), que
  fallan por asercion de comportamiento real de T3B aun no implementado, contra un esqueleto que compila; sin fallo de compilacion
  ni prueba artificial; G1/G2/G3 en el commit RED para que las pruebas existentes sigan en verde; despues GREEN sin tocar los
  archivos de prueba del RED (16.8);
  (3) WorkspacePaletteHost.cs segun INV-F1-T3B-01, 02, 06, 09 y 12, cubierto por la guarda de fuente nueva;
  (4) el delta del comando segun INV-F1-T3B-03 (y su guarda existente en verde);
  (5) ActiveSessionChanged internal en el puente segun INV-F1-T3B-04, con la superficie publica y los recuentos 16/16/16 intactos;
  (6) el relevo del controlador segun INV-F1-T3B-05 y G3;
  (7) la raiz y el view-model de src/RackCad.UI/Workspace/ segun INV-F1-T3B-06, 07, 08, 10, 11 y 13, sin temporizadores ni sondeo,
  probados en RackCad.UI.Tests con un puerto simulado;
  (8) la contencion segun INV-F1-T3B-09, probada en UI (puerto que lanza, delegado que lanza) sin manejadores globales;
  (9) G1/G2/G3 exactos segun INV-F1-T3B-14; las demas guardas y pruebas de T1/T2/T3A sin modificar y en verde;
  (10) en el commit GREEN: los dos filtros y las dos suites completas (Core y UI) pasan; las builds Release de RackCad.Tests,
  RackCad.UI.Tests y del Plugin no emiten avisos nuevos MSB/CS/NU/NETSDK (el Plugin solo los 2 MSB3277 de la linea base);
  (11) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success;
  (12) sin AutoCAD, sin cargar el DLL, sin perfil, registro, TRUSTEDPATHS, SECURELOAD ni ACL; sin Smoke-1.
- IssuedBy = "Controller Codex (planificacion F1-T3B-HOST-UI, primera delegacion de la cadena, CONTROLLER_PLANNING)".
