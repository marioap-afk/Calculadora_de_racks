I-64 - DELEGACION F2-T2A-NAVIGATION-HOST - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F2; tarea F2-T2A-NAVIGATION-HOST; intento 1; RunId R20261109T171357Z-e137;
           DelegationRunId R20261109T171357Z-e137; AuthorityRevision 09a1499a46d16c8251ac4575d370b551603b6372 (la del contrato);
           MainSha a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60; BaseSha = el HEAD actual del worktree (se espera 35c3cd05bb180462c869324ca832f625ad735a0b, descendiente de la AuthorityRevision:
           solo anade la custodia de las decisiones del Coordinator C-F2T2-01..13 y del contrato emitido, y el estado;
           AUTOMATION_PLAN 16.3 admite AuthorityRevision ancestro de BaseSha con solo rutas UNIT_DOC entre ambos);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261109T171357Z-e137
Contrato de gate: artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/R20261109T171357Z-e137/gate-contract.json (ASCII puro; emitido por la sesion por las decisiones del Coordinator; custodiado en
           docs/automation/evidence/I-64-pilot/F2-T2A-NAVIGATION-HOST/coordinator/gate-contract.json junto con el literal de las decisiones
           coordinator-f2t2-contract-review-decisions.md; lee ese literal en el HEAD: es la fuente de las decisiones que el contrato codifica)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato.
           Esta delegacion NO se ejecuta todavia: pasara A1-A8 y nc4 y despues la revision explicita del Coordinator
           (COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED) antes de cualquier Worker. Debe ser ejecutable tal cual si se acepta.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 16 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) D-01, D-02, D-03, D-04, D-05, D-07, INV-02, INV-03, INV-14,
           INV-22, INV-36, INV-38, INV-40, section 11 (fila F2), section 12.3 y section 13; el cierre de F2-T1; el paquete
           docs/initiatives/I-64-f2-t2-gate-contract-package.md; el literal de las decisiones; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la
           DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (6 archivos) ; git ls-files src/RackCad.Application/Workspace (21 archivos) ;
           git ls-files tests/RackCad.Tests/Workspace (32 archivos); y lee el codigo y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03, F2T2-C01..C07 y F2T2A-C08..C10 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 76 a section 89 de docs/automation/evidence/I-64-evidence.md)
- F2-T1-NAVIGATION-CORE esta CERRADA con PASS (producto verificado c726410f). F2-T2 se dividio por decision del Coordinator en
  F2-T2A-NAVIGATION-HOST (esta: puerto, eco, adaptadores del Plugin, contexto de comando, foco y deltas de guardas; SIN UI) y
  F2-T2B-NAVIGATION-UI (despues; controles WPF; no autorizada todavia). Presupuesto de T2A: Attempt 1, AttemptsRemaining 2,
  MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada. La delegacion debe dejar al Worker
  todo lo necesario para acertar a la primera.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Proyectos: tests/RackCad.Tests referencia Domain y Application (nunca el Plugin ni la UI); tests/RackCad.UI.Tests referencia la UI.
    El Plugin solo se compila en la CI (job "Build Plugin without AutoCAD", AutoCAD.NET 25.0.1 con -p:UseAutoCADNuGetReferences=true) y
    nunca se prueba unitariamente: las pruebas de Core cubren los tipos puros de Application y GUARDAS DE TEXTO sobre las fuentes del
    Plugin, leidas con el ayudante WorkspaceHostSource (tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs: Code, Text,
    CodeOnly, Files, PluginFiles, ApplicationFiles, AllProduct, BodyOf, Count). El comportamiento real en AutoCAD lo comprueba Smoke-NAV.
  * Linea base en el HEAD (Release, local): FullyQualifiedName~RackCad.Tests.Workspace = 656 pruebas de Core y
    FullyQualifiedName~RackCad.UI.Tests.Workspace = 64 pruebas de UI, todas en verde. Las pruebas de tests/RackCad.Tests/Workspace/
    usan el namespace RackCad.Tests (file-scoped), clases Workspace* y #nullable enable por archivo; el CI rechaza avisos nuevos.
  * API de F2-T1 (sin cambio): NavigationFacts.cs: record NavigationDefinitionFact(long DefinitionHandle, bool IsXref, bool
    IsInterpretable, string? EnvelopeId, string? ProbedId, string? Kind, string? View, int Section, string? Name,
    IReadOnlyList<long> ModelSpaceReferenceHandles) e interface INavigationFactsPort { ReadDefinitionFacts(SessionId) }.
    NavigationTargets.cs: NavigationTargets(INavigationFactsPort) con Resolve, ResolveForContext, IsCached, Invalidate,
    InvalidateSession, ApplyDrain(SessionId, DrainResult), EndSession, IsSessionEnded y Revalidate. NavigationActions.cs:
    NavigationActionKind {Select, View, Center, LocateView}; NavigationActionVerdict {Act, RejectedNoTargets,
    RejectedSessionMismatch, RejectedSessionDestroyed, RejectedTargetGone, RejectedNotAttributable}; NavigationActionPlan;
    NavigationActionDecision; interface INavigationEffects { SelectReferences(SessionId, IReadOnlyList<long>);
    ViewReference(SessionId, long); CenterOnReference(SessionId, long) }; NavigationActionPolicy(NavigationTargets) con
    Plan(origin, rackId, action, copyIndex = 0, view = null, locateWith = View), Decide(plan, executingSession) y
    Execute(plan, executingSession, effects). Hoy nada en src/ construye NavigationTargets ni NavigationActionPolicy ni llama a
    ApplyDrain o EndSession.
  * GUARDAS que hoy bloquean T2A (todas en tests/RackCad.Tests/Workspace/; sus deltas exactos van en el RED, C-F2T2-08):
    - WorkspaceBridgeSourceGuardTests.TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs recorre TODO
      src/RackCad.Plugin/Workspace/ con ForbiddenCallNames (SetImpliedSelection, SetCurrentView, ZoomExtents, ZoomWindow, Zoom, PanTo,
      UpdateScreen, Regen, SendStringToExecute, StartCommand, GetSelection, GetEntity, GetString, GetPoint, GetKeywords, GetInteger,
      GetDouble, SelectImplied, SelectAll, SelectAtPoint, ShowModalDialog, ShowModelessDialog, ShowDialog, MessageBox, GetObject,
      OpenCloseTransaction, Highlight, Unhighlight); la unica excepcion es el lector (WorkspaceSelectionReader.cs, solo SelectImplied y
      GetObject) por archivo exacto, con metapruebas en WorkspaceSourceGuardExceptionTests.cs.
    - WorkspaceHostSourceGuardTests (43 [Fact]/[Theory], recuento fijado por WorkspacePaletteHostSourceGuardTests.
      TheRefinementsG1G2AndG3AreExactAndNothingWasRemovedFromTheT3AGuardFile, que tambien fija 3 += del controlador):
      NoHostFileWritesTheSelectionTheViewOrAnySystemVariable (SetImpliedSelection, SetCurrentView, SetSystemVariable, Zoom\w*, Regen,
      UpdateScreen, Highlight, Unhighlight, SendStringToExecute, PanTo en Plugin/Workspace y Application/Workspace: OJO, ningun
      identificador nuevo puede contener Zoom); NoHostFileStartsABackgroundThreadOrAsyncWork (Task.Run, Task.Factory, new Thread,
      ThreadPool, Timer, async, await, Parallel., BackgroundWorker, SynchronizationContext en Plugin y Application Workspace);
      NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType (Autodesk.AutoCAD.Internal, PaletteSet, AddVisual, System.Windows, RackCad.UI,
      Autodesk.AutoCAD.Windows, UserControl, Dispatcher; el host de la paleta es la unica excepcion G1); TheReaderApisAreUsedInNoOther
      FileOfThePluginWorkspace (GetObject, RackBlockData, RackEmbedStore, SelectImplied fuera del lector);
      EveryTransactionAndLockInTheHostIsInsideUsingAndNoTypeOfTheHostIsHeldInAField; las guardas del controlador y del comando.
    - WorkspacePaletteHostSourceGuardTests.TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism congela la
      lista EXACTA de los seis archivos de src/RackCad.Plugin/Workspace/.
    - WorkspaceNavigationAuthorityTests: OnlyTheAuthorityTraversesTheReferenceHandlesOfTheFacts (solo NavigationFacts.cs y
      NavigationTargets.cs nombran ModelSpaceReferenceHandles); solo NavigationTargets.cs llama a .ReadDefinitionFacts(;
      NoEventSideFileCanReachTheNavigationEffectsOrThePolicy (ningun archivo nombra a la vez INavigationEffects, NavigationActionPolicy o
      NavigationActionPlan y IHostEventSource, HintKind, EnqueueHint, EventBridgeCore o IWorkspaceBridgeLink; EventBridgeCore.cs,
      WorkspaceHostCore.cs, WorkspaceSession.cs, Hints.cs y BridgeSource.cs no pueden nombrar tipos de navegacion de esa familia).
      Por eso la politica vive en WorkspaceNavigationHost.cs y no en WorkspaceController.cs (que declara IWorkspaceBridgeLink).
    - WorkspaceHostPortGuardTests fija IWorkspaceHostPort como solo lectura: el puerto nuevo es otro tipo y esas guardas no cambian.
    - WorkspaceModelBoundaryTests recorre todo src/RackCad.Application/Workspace/: prohibe System.IO, la SUBCADENA Transaction, la
      palabra Database, Commit(, Autodesk.; nombres de tipos con Tab, Subscription, Editor o Draft (sin distinguir mayusculas; OJO
      Stable, Table, Editable); y en nombres publicos ProjectSummary, Metric, Provider, TotalRacks, Aggregat y Bom.
  * Seleccion y drenaje: WorkspaceSelectionRead (src/RackCad.Application/Workspace/WorkspaceSelectionRead.cs, FUERA del alcance)
    transporta un DefinitionFact por referencia seleccionada y el recuento de no-racks, pero NO los handles de las referencias;
    WorkspaceSelectionReader.cs (Plugin, candidato) obtiene los ObjectId con SelectImplied; WorkspaceHostCore.cs (candidato) drena:
    _reader.Read(...) y una sola llamada session.ApplyContext(WorkspaceSelectionMapper.ToContext(read)); guardas con recuentos
    exactos en WorkspaceHostCore.cs (.ApplyContext( una vez, _reader.Invalidate( una vez, WorkspaceSelectionMapper.ToContext( una
    vez, sin SelectionContext.One( ni SelectionContext.Selection(). El eco por CONJUNTO de handles tiene que fluir por los archivos
    permitidos (por ejemplo, una instancia compartida de NavigationEcho que el lector alimenta con el conjunto leido y que el nucleo
    consulta antes de aplicar el contexto) sin tocar WorkspaceSelectionRead.cs ni WorkspaceSession.cs; si no es posible, STOP P-03
    (F2T2A-C08) explicado en RoutingReason.
  * Controlador y puente: WorkspaceController.cs (candidato) es un singleton perezoso que posee AutoCadEventBridge, WorkspaceHostCore,
    WorkspaceSelectionReader y WorkspaceHostPort, expone event Action StateChanged, tiene exactamente 3 += y declara la clase anidada
    BridgeLink : IWorkspaceBridgeLink. El puente (prohibido) expone Drained(DrainOutcome), ActiveSessionChanged, CommandStateChanged y
    las fachadas ActiveSession, Authorize, TryGetSession y RequestReconciliation; no hay aviso que nombre una SessionId destruida
    (de ahi la purga perezosa con TryGetSession).
  * Autoridades integradas (prohibido cambiarlas): RackBlockFinder.ScanEnvelopes (src/RackCad.Plugin/RackBlockFinder.cs, internal
    static; omite xref y devuelve los sobres con su recuento, sin handles de referencias); RackInventarioCommands.ZoomToRack (privado:
    centro y extension GeometricExtents de la referencia con factor 1.10, planta XY, SetCurrentView con documento bloqueado) es la
    referencia de VIEW; no se llama ni se modifica.
  * Ningun proyecto de prueba referencia el Plugin: las pruebas de RED de T2A son de Core (tipos puros de Application con puertos
    simulados, y guardas de texto de los archivos nuevos del Plugin).

ALCANCE DE ESCRITURA (decision del Coordinator C-F2T2-13; A3 se evalua contra el contrato)
- AllowedWriteScope de la delegacion = PRIMERO los 7 archivos nuevos del contrato, exactamente y en su orden:
    src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs
    src/RackCad.Application/Workspace/NavigationEcho.cs
    src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
    src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs
    src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationEchoTests.cs
  DESPUES, la lista EXPLICITA de los archivos existentes que el diseno cambia de verdad, como SUBCONJUNTO EXACTO de estos 10
  candidatos del contrato y en este mismo orden (no incluyas un candidato que no cambie):
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationAuthorityTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostC2SourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs
  Para cada archivo existente elegido, una entrada de AcceptanceCriteria con su delta exacto y la razon (que guarda o que cableado).
  Nunca anadas una ruta que no este en el contrato, nunca uses un prefijo de carpeta, y no anadas mas archivos nuevos.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/R20261109T171357Z-e137/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese
  contrato. No los copies a mano: CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por
  ejemplo con python desde pwsh:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/R20261109T171357Z-e137/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- Los valores Section de Authorities son la transliteracion ASCII de los encabezados: copia el valor del contrato.
- TODA tu salida debe ser ASCII puro, tambien en los campos que redactas tu (TaskClass, RoutingReason, AcceptanceCriteria,
  IssuedBy): sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro
  caracter de control dentro de las cadenas.
- Antes de emitir: comprueba que, al decodificar tu salida, esos campos son identicos (cadena por cadena) a los del contrato, que
  AllowedWriteScope cumple ALCANCE DE ESCRITURA y que tu salida no contiene ningun byte no ASCII, ninguna barra invertida seguida de
  u y ningun tabulador; si no, corrige tu salida volviendo a usar la carga, nunca a mano.
- No expandas ni estreches ForbiddenWriteScope (56 entradas). RequiredTests con los mismos dos elementos (Project, Filter,
  MinSelected y ExpectRed).

TASKCLASS (decision del Coordinator C-F2T2-01)
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la
  eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda del contrato que
  uses. Si tu evaluacion da Horizonte High (Long-horizon) para esta tarea dividida: para con F2T2-C07 explicado en RoutingReason.
- Celda del Worker (la unica con write-commit-push del contrato): Executor.Cell = "anthropic/subagent/claude-sonnet-5-5";
  Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high";
  Effort.Semantic = "Deep"; ModelEscalationReason = null. La celda openai/process/gpt-6-luna es la del Controller, no la del Worker.

DELTA DE LA TAREA (primera delegacion de la cadena F2-T2A-NAVIGATION-HOST; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261109T171357Z-e137 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F2"; TaskId = "F2-T2A-NAVIGATION-HOST" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60 (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (09a1499a...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261109T171357Z-e137" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker".
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5 congelada, del cierre
  de F2-T1, del literal de las decisiones C-F2T2-01..13, del contrato y de los HECHOS DEL REPOSITORIO; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de las rutas exactas del AllowedWriteScope de la delegacion; los archivos de F2-T1
  y los prohibidos sin cambio;
  (2) RED primero: un commit RED con esqueletos que compilan, las pruebas nuevas de WorkspaceNavigationHostTests.cs y
  WorkspaceNavigationEchoTests.cs que fallan por asercion y los deltas exactos de las guardas existentes; push del RED antes del
  GREEN; el GREEN no toca los archivos de prueba del RED (16.8);
  (3) la API exacta propuesta de IWorkspaceNavigationPort y de su resultado inmutable (ejecutado, no disponible, rechazado, obsoleto,
  fallido), sin tipos de AutoCAD y sin excepciones como flujo normal;
  (4) el contrato de NavigationEcho (memoria por SessionId, comparacion por CONJUNTO, borrado al purgar) y como llega el conjunto
  de handles leido al drenaje sin tocar WorkspaceSelectionRead.cs;
  (5) el patron exacto del contexto de comando en WorkspaceNavigationHost.cs: ExecuteInCommandContextAsync con async/await, sin
  Wait, Result, GetResult, Task.Run, Task.Factory, Thread, Timer ni ConfigureAwait(false), ninguna Task desligada, y el orden de
  validaciones y efectos de INV-F2T2A-02 dentro del callback; el resultado se devuelve por el puerto, sin bus de eventos;
  (6) WorkspaceNavigationFacts.cs y WorkspaceNavigationEffects.cs segun INV-F2T2A-05/06, con transacciones en using y sin campos de
  tipos de AutoCAD; foco en un unico metodo aislado con API publica (INV-F2T2A-04);
  (7) purga perezosa con TryGetSession en cada drenaje de navegacion y antes de cada accion (INV-F2T2A-09);
  (8) cada delta de guarda nombrado por prueba y archivo exacto, con las excepciones nuevas fijadas en
  WorkspaceSourceGuardExceptionTests.cs y los recuentos de metaguardas actualizados de forma exacta (INV-F2T2A-10);
  (9) en el commit GREEN: los dos filtros requeridos y las suites completas de Core y UI en verde; builds Release sin avisos nuevos;
  (10) la CI exacta del CurrentSha con los cuatro jobs en success, incluido Build Plugin without AutoCAD;
  (11) worker-handoff.json conforme al esquema en ExpectedHandoffPath con RED, GREEN, alcance, trailer, conteos de los dos filtros y
  mapa evidencia-por-invariante;
  (12) sin UI, sin AutoCAD, sin comandos nuevos, sin F3/F4/F6 ni inventario global.
- IssuedBy = "Controller Codex (planificacion F2-T2A-NAVIGATION-HOST, primera delegacion de la cadena, CONTROLLER_PLANNING)".
