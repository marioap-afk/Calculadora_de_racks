I-64 - DELEGACION F4-T2A-PILOT-STATE - Controller, perfil CONTROLLER_PLANNING (CORRECCION, intento 2 de 3, de la cadena F4-T2A; F4-E24; contrato gate-contract-attempt2.json)
Identidad: unidad I-64; gate F4; tarea F4-T2A-PILOT-STATE; intento 2; RunId R20261112T054738Z-a765;
           DelegationRunId R20261112T054738Z-a765; AuthorityRevision 6e5879a1c3b56fad7668fbcb9d3b5bcc446a829e (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 539614aed17b0880a98cab201cd9cba33e968112: commit de emision del contrato del intento 2, hijo directo de la AuthorityRevision 6e5879a1c3b56fad7668fbcb9d3b5bcc446a829e:
           solo anade ese contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261112T054738Z-a765
Contrato de gate: artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/2/R20261112T054738Z-a765/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2A-PILOT-STATE/coordinator/gate-contract-attempt2.json; parte de la revision 2)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 16 del contrato; una delegacion nunca amplia el contrato
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
Antecedentes: en esta cadena se rechazaron por copia inexacta las planificaciones R20261111T204805Z-13b0 (un invariante reescrito) y R20261111T211626Z-61f3 (una entrada de ForbiddenWriteScope omitida). Las colecciones del contrato se copian EXACTAMENTE; una sola diferencia basta para rechazar.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 112 a section 134 de docs/automation/evidence/I-64-evidence.md; F4-E19 a F4-E24 y docs/automation/evidence/I-64-pilot/F4-T2A-PILOT-STATE/1/R20261112T052453Z-8bc4/analysis.md; docs/automation/evidence/I-64-pilot/F4-T2A-PILOT-STATE/1/R20261111T204805Z-13b0/analysis.md)
- CORRECCION de F4-T2A (intento 2): la entrega del intento 1 (RED 19123dfe, GREEN 77658968) dio EXECUTION_BLOCKED / STOP por Ci en la verificacion R20261112T052453Z-8bc4 (comparaciones textuales de documentos dependientes de la plataforma, 9 pruebas en la CI de Linux) y las revisiones hallaron que ComposeForWrite transfiere todo el contenido editable en vez de solo el campo del piloto (IC-07 punto 2) y seis menores; el contrato del intento 2 reescribe INV-F4T2A-07 y amplia INV-F4T2A-06, 08, 09 y 10, y anade INV-F4T2A-16. La lectura en el host (T2B), IC-04 (T2C), la interfaz (T3) y la mutacion (T4) no se delegan aqui.
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Producto actual (GREEN 77658968 del intento 1, en el HEAD): FullyQualifiedName~RackCad.Tests.Workspace selecciona 1446 pruebas (9 fallan en la CI de Linux por la comparacion textual) y FullyQualifiedName~RackCad.UI.Tests.Workspace 245, en verde. Los minimos del contrato siguen siendo 1300 y 222.
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/2/R20261112T054738Z-a765/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/2/R20261112T054738Z-a765/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas del contrato: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
  Ejemplo real del rechazo anterior: el contrato dice "los dos proyectos de prueba lo leen por ruta" (lo = el corpus) y debe quedar asi, nunca "los leen".
- AllowedWriteScope es ahora la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada. ForbiddenWriteScope tiene exactamente las entradas del contrato de la revision 2: ni una mas ni una menos.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (57 entradas). RequiredTests con los mismos dos elementos.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2A-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion, intento 2, de la cadena F4-T2A-PILOT-STATE)
- RunId = R20261112T054738Z-a765 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2A-PILOT-STATE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ed680e1f2eb780b6ead4a4c8a0c06ba17444f0cb (BaseSha de la primera delegacion de la cadena); ChainRedSha = 19123dfe5d54c81f96d8aabcf1d1d25af64112b6 (RED acreditado por R20261112T052453Z-8bc4); CorrectionOf: ver abajo.
- ChainRedFiles = exactamente estas 24 rutas, en este orden (el custodiado en el relevo de R20261112T052453Z-8bc4): tests/RackCad.Tests/Fixtures/I64/pb-01-base.json, tests/RackCad.Tests/Fixtures/I64/pb-02-modulos.json, tests/RackCad.Tests/Fixtures/I64/pb-03-fondos.json, tests/RackCad.Tests/Fixtures/I64/pb-04-blanco.json, tests/RackCad.Tests/Fixtures/I64/pb-05-topes.json, tests/RackCad.Tests/Fixtures/I64/pb-06-seguridad.json, tests/RackCad.Tests/Fixtures/I64/pb-07-combinado.json, tests/RackCad.Tests/Fixtures/I64/pb-10-sin-datum.json, tests/RackCad.Tests/Fixtures/I64/pb-11-sin-i41.json, tests/RackCad.Tests/Fixtures/I64/pb-12-poste-nulo.json, tests/RackCad.Tests/Fixtures/I64/pb-13-sideb-no-presente.json, tests/RackCad.Tests/Fixtures/I64/pb-14-peralte-heredado.json, tests/RackCad.Tests/Fixtures/I64/pb-15-sin-composite.json, tests/RackCad.Tests/Fixtures/I64/pb-20-ab.json, tests/RackCad.Tests/Fixtures/I64/pb-21-cola.json, tests/RackCad.Tests/Fixtures/I64/pb-31-v2-modulos.json, tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftSessionTests.cs, tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs, tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2A-PILOT-STATE/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261112T054738Z-a765" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria (los redactas tu, en ASCII): concretos y comprobables, derivados SOLO de la Proposal V5, del paquete, de IC-06 e IC-07, del contrato, de docs/automation/evidence/I-64-pilot/F4-T2A-PILOT-STATE/1/R20261112T052453Z-8bc4/analysis.md y de los HECHOS DEL REPOSITORIO; como minimo uno por cada invariante INV-F4T2A-01 a INV-F4T2A-16, y ademas:
  git diff --name-only BaseSha..CurrentSha dentro del AllowedWriteScope; RED de la correccion primero (pruebas nuevas o corregidas que compilan y fallan por asercion sobre el producto actual), con push del RED antes del GREEN, y el GREEN no toca ningun archivo de tests/;
  en el GREEN: los dos filtros y las suites completas de los dos proyectos en verde en Windows y en la CI exacta 4/4 (Core se ejecuta en Linux), build Release sin avisos nuevos; worker-handoff.json conforme al esquema en ExpectedHandoffPath; sin AutoCAD.
- CorrectionOf = { RunId: "R20261112T052453Z-8bc4", FailureClass: "Ci", AnalysisSha256: "4f1635760902cf97f3f55f12c208dea8987296a80d5c30563139743876447eb7" } (la verificacion del intento 1 y su analysis.md custodiado).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T2A-PILOT-STATE, intento 2, F4-E24, CONTROLLER_PLANNING)".
