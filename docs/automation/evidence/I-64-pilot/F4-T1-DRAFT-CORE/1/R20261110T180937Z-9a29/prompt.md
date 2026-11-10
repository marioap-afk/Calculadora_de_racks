I-64 - DELEGACION F4-T1-DRAFT-CORE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F4; tarea F4-T1-DRAFT-CORE; intento 1; RunId R20261110T180937Z-9a29;
           DelegationRunId R20261110T180937Z-9a29; AuthorityRevision 80e139859e0455ceb5a906a799a20b796e0cb8db (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 13775d8fb89f3ac983fbeeb6bdcf749e7fd746fc, hijo directo de la AuthorityRevision:
           solo anade el contrato emitido, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T180937Z-9a29
Contrato de gate: artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/1/R20261110T180937Z-9a29/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 18 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) D-10, D-12, D-13 y D-17; docs/initiatives/I-64-f4-gate-contract-package.md section 6; la orden del Owner; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Workspace (27 archivos, ninguno en Drafts) ; y lee el codigo y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 109 a section 111 de docs/automation/evidence/I-64-evidence.md)
- F4 esta autorizada por el Owner con presupuesto propio. Esta tarea es el nucleo puro de los borradores; la lectura en el host, la interfaz y la mutacion son tareas posteriores y no se delegan aqui.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Proyecto: tests/RackCad.Tests prueba Domain y Application (net8.0). Namespace unico de pruebas: RackCad.Tests, file-scoped. Las clases nuevas se llaman WorkspaceDraft... en tests/RackCad.Tests/Workspace/ para que el filtro por subcadena las seleccione.
  * Linea base (Release): FullyQualifiedName~RackCad.Tests.Workspace = 902, todas en verde.
  * Namespace de los archivos nuevos: RackCad.Application.Workspace.Drafts (NamespaceFolderGuardTests exige que el namespace sea la ruta de la carpeta, con un unico namespace de bloque). Cada archivo lleva #nullable enable.
  * Tipo que se puede usar del namespace padre: SessionId (readonly record struct con Value long, monotono y nunca reutilizado).
  * GUARDAS existentes que recorren src/RackCad.Application/Workspace/ de forma RECURSIVA y por tanto aplican a Drafts/:
    WorkspaceModelBoundaryTests.ModelFilesExistAndHaveNoAutoCadWpfOrPluginReferences (sin Autodesk., System.Windows, RackCad.Plugin ni RackCad.UI);
    WorkspaceModelBoundaryTests.ModelDoesNotPersistNorWriteAuthoredData (en el codigo, sin System.IO, File., Directory., StreamWriter, Microsoft.Win32, Registry., UserSettings, AtomicFile, Transaction, LockDocument, Database, Commit( ni Autodesk.): ningun identificador puede contener Transaction ni terminar en Commit(;
    WorkspaceBridgeSourceGuardTests.TheBridgeFilesCarryNoMetricsProviderProjectSummaryOrAggregationConcept (texto de codigo sin ComputedParameter, Metric, ProjectSummary, TotalRacks, Aggregat, Provider ni palabra que empiece por Bom, sin distinguir mayusculas);
    WorkspaceHostSourceGuardTests (sin Regen, async ni await en las carpetas Workspace).
  * GUARDAS que miran solo el namespace exacto RackCad.Application.Workspace y por eso NO cubren Drafts: WorkspaceModelBoundaryTests.ModelHasNoMetricsProviderOrProjectSummaryConcepts y ThereIsNoPerTabStateOrPerTabSubscriptions. INV-F4T1-16 pide una guarda equivalente nueva para el subnamespace.
  * El tipo generico usa un IEqualityComparer<TState> inyectado; el validador y la aplicacion del texto pendiente los aporta el llamador. No hay estado de ningun sistema en esta tarea.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE los 10 archivos nuevos del contrato, en su orden; ningun archivo existente:
    src/RackCad.Application/Workspace/Drafts/DraftSession.cs
    src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs
    src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs
    src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs
    src/RackCad.Application/Workspace/Drafts/DraftClosePolicy.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftSessionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftStaleBaseTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs
  Nunca anadas una ruta fuera del contrato ni un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/1/R20261110T180937Z-9a29/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/1/R20261110T180937Z-9a29/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (66 entradas). RequiredTests con el mismo elemento.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T1-C05 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T1-DRAFT-CORE)
- RunId = R20261110T180937Z-9a29 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T1-DRAFT-CORE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T180937Z-9a29" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria (los redactas tu, en ASCII): concretos y comprobables, derivados SOLO de la Proposal V5, del paquete, del contrato y de los HECHOS DEL REPOSITORIO; como minimo uno por cada invariante INV-F4T1-01 a INV-F4T1-18, y ademas:
  git diff --name-only BaseSha..CurrentSha dentro de las rutas exactas del AllowedWriteScope; RED primero, con push del RED antes del GREEN, y el GREEN no toca los archivos de prueba del RED;
  en el GREEN: el filtro y la suite completa de Core en verde, build Release sin avisos nuevos y CI exacta 4/4; worker-handoff.json conforme al esquema en ExpectedHandoffPath, con OpenQuestions vacia salvo ambiguedad material real; sin AutoCAD.
- IssuedBy = "Controller Codex (planificacion F4-T1-DRAFT-CORE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
