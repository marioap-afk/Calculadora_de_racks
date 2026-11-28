I-64 - DELEGACION F4-T2B1-INTENT-READ - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T2B1-INTENT-READ)
Identidad: unidad I-64; gate F4; tarea F4-T2B1-INTENT-READ; intento 1; RunId R20261112T081014Z-84ff;
           DelegationRunId R20261112T081014Z-84ff; AuthorityRevision d6587a06803ab618698cbd342f38e2cf31f87af7 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 445c46cefe587a6ba3274143bbbee86cc4ff279b: commit de emision del contrato, hijo directo de la AuthorityRevision d6587a06803ab618698cbd342f38e2cf31f87af7:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261112T081014Z-84ff
Contrato de gate: artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/1/R20261112T081014Z-84ff/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2B1-INTENT-READ/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t2b-gate-contract-package.md (section 4, decisiones D-T2B-01 a D-T2B-08, con la tabla de las 14 reglas y los textos exactos de D-T2B-03; section 5, el contrato propuesto); docs/initiatives/I-64-proposal-v5.md (congelada) D-10, D-11, D-12 y D-17; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (4 archivos) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T2B1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en la tarea anterior de esta unidad, dos planificaciones se RECHAZARON por la copia: una cambio "lo leen" por "los leen" en un invariante (A5 FAIL) y otra omitio UNA entrada de ForbiddenWriteScope (A4 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 125 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E18, F4-E25 y F4-E26 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1 (nucleo de borradores en src/RackCad.Application/Workspace/Drafts/) y F4-T2A (piloto de Push Back en src/RackCad.Application/Systems/PushBack/Pilot/) estan cerradas. F4-T2B se dividio en T2B1 y T2B2 (F4-E26). Esta tarea es T2B1: la lectura de la intencion en Application pura. La proyeccion del barrido, el adaptador del Plugin y el host (T2B2), IC-04 (T2C), la interfaz (T3) y la mutacion (T4) no se delegan aqui.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 5c9cc24e (corrida 43171988530; mismo producto que el HEAD): FullyQualifiedName~RackCad.Tests.Workspace = 1468 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde. Esta tarea no anade pruebas de UI: el filtro de UI tiene ExpectRed false.
  * Namespaces de prueba: RackCad.Tests (Core, file-scoped). Las clases nuevas se llaman WorkspaceDraftPilotIntent... en tests/RackCad.Tests/Workspace/ para que el filtro por subcadena las seleccione.
  * Kit de pruebas existente y reutilizable sin modificarlo: PilotKit (internal static, en tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:30-189): Catalog (JsonRackCatalogProvider.FromBaseDirectory().Load()), RepoRoot(), Text(fixture) que lee tests/RackCad.Tests/Fixtures/I64/<fixture>.json, Admitted(text) y Normalize. Fixtures existentes: pb-01-base (un lado, admitido), pb-20-ab (A/B, fuera del piloto), pb-21-cola (cola, fuera del piloto) y otros pb-*.json; son sobres completos con Kind "pushback", Id, View, Section y Design.
  * Producto nuevo: namespace RackCad.Application.Systems.PushBack.Pilot, UN namespace de bloque por archivo (NamespaceFolderGuardTests: el namespace es la ruta de la carpeta); los cuatro archivos vecinos del piloto llevan #nullable enable.
  * Hechos que consume la tarea, sin cambiarlos: RackSiblingMembership.Classify(scan, rackId, originalSourceId, originalSourceIdIsAttributable) en src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:93-155 (lanza si el rackId esta en blanco o si los hechos violan su estructura; un sobre interpretable con Id vacio es NotMember; atribucion OrdinalIgnoreCase); RackAuthoredComparatorPorts.PushBack() en src/RackCad.Application/Systems/Shared/RackAuthoredComparator.FourKinds.cs:18-20 y :49-82; AuthoredRawReader.Read en src/RackCad.Application/Systems/Shared/AuthoredRawReader.cs:19-52; RackEmbedStore.Deserialize en src/RackCad.Application/Persistence/RackEmbedDocument.cs:97-129 (null si el JSON es invalido o la version mayor es mas nueva); DraftStaleBase.Create y CompareWith en src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs; DraftIntentRead, CheckNow y DraftApplyOutcome<TState>.CommitThrew en Drafts/DraftSession.cs y Drafts/DraftApplyOutcome.cs; PushBackPilotMaterializer.Materialize y sus constantes en Pilot/PushBackPilotMaterializer.cs.
  * Regla de IsComplete de los llamadores integrados: src/RackCad.Plugin/Views/RackProjectionKindSessions.cs:223-237 (solo lectura).
  * GUARDAS existentes: WorkspaceModelBoundaryTests recorre src/RackCad.Application/Workspace/ (no el piloto); WorkspaceDraftBoundaryTests prohibe nombres de sistema en Drafts y recorre 4 archivos de preview (PreviewFiles, :547-567). Ninguna guarda existente se modifica en esta tarea; la guarda nueva vive en WorkspaceDraftPilotIntentGuardTests.cs.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 7 entradas del contrato, en su orden (6 archivos y el prefijo de fixtures):
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentScan.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs
    tests/RackCad.Tests/Fixtures/I64/intent/
  Nunca anadas una ruta fuera del contrato ni otro prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/1/R20261112T081014Z-84ff/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/1/R20261112T081014Z-84ff/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 58 entradas del contrato: ni una mas ni una menos.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (incluido ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2B1-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T2B1-INTENT-READ)
- RunId = R20261112T081014Z-84ff (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2B1-INTENT-READ".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261112T081014Z-84ff" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED que compila y falla por asercion con el esqueleto inerte, el GREEN que no toca tests/, la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T2B1-INTENT-READ, primera delegacion de la cadena, CONTROLLER_PLANNING)".
