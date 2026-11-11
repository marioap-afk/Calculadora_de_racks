I-64 - DELEGACION F4-T2A-PILOT-STATE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F4; tarea F4-T2A-PILOT-STATE; intento 1; RunId R20261111T204805Z-13b0;
           DelegationRunId R20261111T204805Z-13b0; AuthorityRevision 4dc111b54ffe72dc400373df16dc2530f81c0c1b (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 36214bb820edb6d5e4777d16f28bd5170477e2ce, hijo directo de la AuthorityRevision:
           solo anade el contrato emitido, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261111T204805Z-13b0
Contrato de gate: artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/1/R20261111T204805Z-13b0/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2A-PILOT-STATE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t2-gate-contract-package.md (section 4 decisiones D-T2-01 a D-T2-05 y section 6); docs/initiatives/I-64-implementation-clarifications.md IC-06 e IC-07; docs/initiatives/I-64-proposal-v5.md (congelada) D-10, D-11, D-12, D-13 y D-17; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack (52 archivos, ninguno en Pilot/) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T2A-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" en el texto libre, ni en nombres de archivo, rutas o artefactos
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 112 a section 126 de docs/automation/evidence/I-64-evidence.md)
- F4-T1-DRAFT-CORE esta cerrada (nucleo puro de borradores en src/RackCad.Application/Workspace/Drafts/). F4-T2 se dividio en T2A, T2B y T2C (F4-E18); esta tarea es T2A: estado del piloto de Push Back en Application pura y la frontera de propiedad N-2 en el nucleo. La lectura en el host (T2B), IC-04 (T2C), la interfaz (T3) y la mutacion (T4) no se delegan aqui.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 92b1527c (mismo producto que el HEAD): FullyQualifiedName~RackCad.Tests.Workspace = 1300 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 222, todas en verde (en UI hay ademas 17 pruebas NotExecuted fuera de esos filtros).
  * Namespaces de prueba: RackCad.Tests (Core, file-scoped) y RackCad.UI.Tests (UI). Las clases nuevas se llaman WorkspaceDraftPilot... en tests/RackCad.Tests/Workspace/ y tests/RackCad.UI.Tests/Workspace/ para que los filtros por subcadena las seleccionen.
  * Producto nuevo: namespace RackCad.Application.Systems.PushBack.Pilot, UN namespace de bloque por archivo (NamespaceFolderGuardTests: el namespace es la ruta de la carpeta y la subcarpeta de Systems debe ser un bucket declarado; PushBack lo es). Cada archivo lleva #nullable enable si el proyecto lo exige; comprueba el estilo de los archivos vecinos.
  * Nucleo: DraftSession.cs guarda el InitialState de la lectura sin copiar (lineas 252 y 434) y conserva el argumento de SetCurrentDraftState (linea 324); IsDirty usa los getters (linea 275). El copiador inyectado es Func<TState,TState>. INV-F4T1-19 y las pruebas WorkspaceDraftSessionTests.cs:1000-1054 y :1128-1141 fijan que editar en sitio CurrentDraftState deja Dirty y veta: no se enmienda.
  * GUARDAS existentes: WorkspaceModelBoundaryTests recorre src/RackCad.Application/Workspace/ de forma recursiva (sin Autodesk., System.Windows, RackCad.Plugin, RackCad.UI, System.IO, Transaction, LockDocument, Database ni Commit( en el codigo); WorkspaceDraftBoundaryTests prohibe en los archivos de Drafts los nombres de sistema (PushBack, Selectiv, Cantilever, Cabecera, Dynamic, FlowBed, Cama, Tarima, Fondo, Header) y la regla BOM; WorkspaceHostSourceGuardTests prohibe Regen, async y await en las carpetas Workspace. Ninguna guarda se modifica en esta tarea.
  * Ruta de lectura existente (la de RACKEDITAR, RackPushBackCommands.cs:154 y :162-169): RackEmbedStore.Deserialize(string) en RackEmbedDocument.cs (null si el JSON es invalido o la version mayor es mas nueva), comprobar kind Push Back, y RackProjectStore.TryDeserialize hasta PushBackDesign (RackProjectStore.cs:128-148, con incompatibleMajor).
  * Carga clasica: RackPushBackSystemWindow.LoadExisting (xaml.cs:300-317) hace SideA.LoadFromDesign(SideAOnly(design)), LoadCompositeFromDesign (Composite.cs:788-907) y LoadFromModel (:377-426, con Recompute). SideAOnly(design) == design cuando !design.IsComposite. El ensamblador compuesto escribe siempre design.Composite no nulo.
  * UI tests: StaTestRunner (tests/RackCad.UI.Tests/StaTestRunner.cs); pruebas que ya construyen la ventana y llaman a LoadExisting: BlankFrontSafetyGridTests.cs:294-295; seams internos visibles para RackCad.UI.Tests: Session, LastComputation (xaml.cs:182) y CurrentInputsAreValid; patron RepoRoot para leer archivos del repositorio: BoundedEditorCharacterizationTests.cs:96-100. RackCad.UI.Tests NO referencia RackCad.Tests.
  * Validacion de AnnotationScaleBox: NumericField con Minimum=0 y MinimumInclusive=False y obligatorio (xaml:68-69); NumericFieldValidation.Validate (src/RackCad.UI/Controls) sobre LocalizedNumberParser.TryDouble (Application, rechaza no finitos).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 15 entradas del contrato, en su orden (14 archivos y el prefijo de fixtures):
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotFieldAdapter.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs
    src/RackCad.Application/Workspace/Drafts/DraftSession.cs
    src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftSessionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs
    tests/RackCad.Tests/Fixtures/I64/
  Nunca anadas una ruta fuera del contrato ni otro prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/1/R20261111T204805Z-13b0/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/1/R20261111T204805Z-13b0/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (139 entradas). RequiredTests con los mismos dos elementos.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2A-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T2A-PILOT-STATE)
- RunId = R20261111T204805Z-13b0 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2A-PILOT-STATE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261111T204805Z-13b0" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria (los redactas tu, en ASCII): concretos y comprobables, derivados SOLO de la Proposal V5, del paquete, de IC-06 e IC-07, del contrato y de los HECHOS DEL REPOSITORIO; como minimo uno por cada invariante INV-F4T2A-01 a INV-F4T2A-15, y ademas:
  git diff --name-only BaseSha..CurrentSha dentro del AllowedWriteScope; RED primero (esqueleto inerte que compila y pruebas que fallan por asercion en los dos proyectos), con push del RED antes del GREEN, y el GREEN no toca ningun archivo de tests/ del RED;
  en el GREEN: los dos filtros y las suites completas de los dos proyectos en verde, build Release sin avisos nuevos y CI exacta 4/4; worker-handoff.json conforme al esquema en ExpectedHandoffPath con el delta de caracterizacion por diseno y OpenQuestions vacia salvo ambiguedad material real; sin AutoCAD.
- IssuedBy = "Controller Codex (planificacion F4-T2A-PILOT-STATE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
