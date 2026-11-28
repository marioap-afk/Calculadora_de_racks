I-64 - DELEGACION F3-T1-BROWSER-CORE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F3; tarea F3-T1-BROWSER-CORE; intento 1; RunId R20261110T140050Z-8317;
           DelegationRunId R20261110T140050Z-8317; AuthorityRevision 84dd689f810b42b4f4ccaecb26a6f4642d150963 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera b1911aa01719cb9d0121ca5f14c1166d97d7ee1b, hijo directo de la AuthorityRevision:
           solo anade el contrato emitido, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T140050Z-8317
Contrato de gate: artifacts/orchestration/I-64/F3-T1-BROWSER-CORE/1/R20261110T140050Z-8317/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F3-T1-BROWSER-CORE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 17 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) D-03, D-06, D-08, D-09, INV-02, INV-04, INV-05, INV-07, INV-09, INV-15, INV-36, P-06, P-07,
           section 11 (fila F3); el paquete docs/initiatives/I-64-f3-gate-contract-package.md (su seccion 6 manda); la orden del Owner; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Workspace (23 archivos) ; y lee el codigo y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F3T1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 102 y section 103 de docs/automation/evidence/I-64-evidence.md)
- F2 esta cerrada con limitaciones y F3 esta autorizada por el Owner con presupuesto propio. Esta tarea es el nucleo puro de la sesion de navegador; la UI es F3-T2-BROWSER-UI, que no se delega aqui.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Proyecto: tests/RackCad.Tests prueba Domain y Application (net8.0). Namespace unico de pruebas: RackCad.Tests, file-scoped. Las clases de prueba nuevas se llaman Workspace... dentro de tests/RackCad.Tests/Workspace/ para que el filtro por subcadena las seleccione.
  * Linea base (Release): FullyQualifiedName~RackCad.Tests.Workspace = 782, todas en verde.
  * Tipos que se pueden usar: SessionId (readonly record struct con Value long, monotono y nunca reutilizado) y SelectionContext (sealed record; Kind SelectionContextKind {None, One, Selection, NoIdentity, Diagnostic}; RackId solo con One; fabricas None, NoIdentity, Diagnostic, One(rackId), Selection(rackCount, otherCount)).
  * Cada archivo de src/RackCad.Application/Workspace/ lleva #nullable enable y un unico namespace de bloque RackCad.Application.Workspace.
  * Forma publica pedida por el contrato: enum BrowserTabStatus {NotLoaded, Ready, Unavailable, Error}; enum BrowserChange {None, Activated, Created, Replaced, Pinned, Closed, SummaryApplied}; sealed record BrowserTabRecord(RackId, Name, Kind, IsPreview, Status);
    clase BrowserSession con const MaxLiveSurfaces, Id, IsDestroyed, Tabs, ActiveRackId, LiveSurfaceRackId, ApplySelectionContext, Activate, Pin, Close, ApplySummary(rackId, status, name, kind), constructor no publico;
    clase BrowserSessionRegistry con LiveCount, KnownSessions, GetOrCreate(SessionId) que devuelve null para una sesion destruida, TryGet(SessionId, out BrowserSession) y Destroy(SessionId).
  * GUARDAS existentes sobre src/RackCad.Application/Workspace/ que siguen vigentes:
    WorkspaceModelBoundaryTests.ModelFilesExistAndHaveNoAutoCadWpfOrPluginReferences (sin Autodesk., System.Windows, RackCad.Plugin ni RackCad.UI);
    WorkspaceModelBoundaryTests.ModelDoesNotPersistNorWriteAuthoredData (en el codigo, sin System.IO, File., Directory., StreamWriter, Microsoft.Win32, Registry., UserSettings, AtomicFile, Transaction, LockDocument, Database, Commit( ni Autodesk.);
    WorkspaceModelBoundaryTests.ModelHasNoMetricsProviderOrProjectSummaryConcepts (por reflexion: nombres de tipos y miembros publicos del namespace sin ProjectSummary, Metric, Provider, TotalRacks, Aggregat ni Bom);
    WorkspaceBridgeSourceGuardTests.TheBridgeFilesCarryNoMetricsProviderProjectSummaryOrAggregationConcept (texto de codigo sin ComputedParameter, Metric, ProjectSummary, TotalRacks, Aggregat, Provider ni palabra que empiece por Bom, sin distinguir mayusculas);
    WorkspaceBridgeFacadeSourceGuardTests.NoWorkspaceSessionRegistryIsCreatedOutsideTheCoreAndTheBridgeNamesNone (ningun archivo nuevo nombra WorkspaceSessionRegistry como palabra completa).
  * GUARDA que cambia con delta exacto en el RED: tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs, ThereIsNoPerTabStateOrPerTabSubscriptions (linea 94). Recorre por reflexion TODOS los tipos del namespace RackCad.Application.Workspace, incluidos los anidados y los generados por el compilador,
    y rechaza nombres con Tab, Subscription, Editor o Draft sin distinguir mayusculas. Por eso una lambda con captura o un iterador dentro de un miembro con Tab en el nombre genera un tipo anidado que la guarda rechaza: la implementacion evita lambdas con captura e iteradores en esos miembros.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = PRIMERO los 5 archivos nuevos del contrato, exactamente y en su orden:
    src/RackCad.Application/Workspace/BrowserSession.cs
    src/RackCad.Application/Workspace/BrowserSessionRegistry.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserSessionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserSessionRegistryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserBoundaryTests.cs
  DESPUES, el archivo existente candidato, que cambia por el delta exacto de INV-F3T1-15:
    tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs
  Para el archivo existente, una entrada de AcceptanceCriteria con su delta exacto y la razon. Nunca anadas una ruta fuera del contrato ni un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F3-T1-BROWSER-CORE/1/R20261110T140050Z-8317/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F3-T1-BROWSER-CORE/1/R20261110T140050Z-8317/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro: sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (62 entradas). RequiredTests con el mismo elemento.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F3T1-C06 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F3-T1-BROWSER-CORE)
- RunId = R20261110T140050Z-8317 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F3"; TaskId = "F3-T1-BROWSER-CORE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F3-T1-BROWSER-CORE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T140050Z-8317" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria (los redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5, del contrato y de los HECHOS DEL REPOSITORIO; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de las rutas exactas del AllowedWriteScope;
  (2) RED primero: commit RED con esqueletos que compilan, pruebas nuevas que fallan por asercion y el delta exacto de la guarda; push del RED antes del GREEN; el GREEN no toca los archivos de prueba del RED;
  (3) la forma publica exacta de BrowserSession, BrowserSessionRegistry, BrowserTabRecord, BrowserTabStatus y BrowserChange;
  (4) las reglas de ApplySelectionContext e idempotencia por transicion segun INV-F3T1-03 e INV-F3T1-04;
  (5) activar, fijar, cerrar y pestana activa determinista segun INV-F3T1-05 a INV-F3T1-08;
  (6) K = 1 y resumen perezoso segun INV-F3T1-09 e INV-F3T1-10;
  (7) resultados como valor segun INV-F3T1-11;
  (8) registro por SessionId, destruccion y no resurreccion segun INV-F3T1-12 e INV-F3T1-13;
  (9) pureza y guarda de fuente nueva segun INV-F3T1-14;
  (10) delta exacto de guarda segun INV-F3T1-15;
  (11) cobertura minima de pruebas segun INV-F3T1-17;
  (12) en el GREEN: el filtro y la suite completa de Core en verde; build Release sin avisos nuevos; CI exacta 4/4;
  (13) worker-handoff.json conforme al esquema en ExpectedHandoffPath;
  (14) sin UI, sin Plugin, sin AutoCAD, sin comandos nuevos y sin comportamiento de F4, F5 o F6.
- IssuedBy = "Controller Codex (planificacion F3-T1-BROWSER-CORE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
