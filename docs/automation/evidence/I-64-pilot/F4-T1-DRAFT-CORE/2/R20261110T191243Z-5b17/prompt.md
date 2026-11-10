I-64 - DELEGACION F4-T1-DRAFT-CORE - Controller, perfil CONTROLLER_PLANNING (CORRECCION bajo AUTOMATION_PLAN 16.8; CorrectionOf R20261110T184730Z-e455; intento 2; una unica invocacion)
Identidad: unidad I-64; gate F4; tarea F4-T1-DRAFT-CORE; intento 2; RunId R20261110T191243Z-5b17;
           DelegationRunId R20261110T191243Z-5b17; AuthorityRevision bf429590170c6b7e4c1f17ec55616f5767f201d9 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 90a811045adf8af876f65e3b98e3c224646c033f, hijo directo de la AuthorityRevision:
           solo anade el contrato del intento 2, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T191243Z-5b17
Contrato de gate: artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/2/R20261110T191243Z-5b17/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/gate-contract-attempt2.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la correccion del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 24 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           lee docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/1/R20261110T184730Z-e455/analysis.md, docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/1/R20261110T184730Z-e455/independent-review.md, src/RackCad.Application/Workspace/Drafts/DraftSession.cs y src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 112 y section 113 de docs/automation/evidence/I-64-evidence.md)
- La entrega del intento 1 (RED a362744c, GREEN aefc34a5) quedo con EXECUTION_VERIFIED (verificacion R20261110T184730Z-e455) y nc1..nc3 CONTROL PASS, que se conservan. La revision directa de la sesion y la revision adversarial independiente hallaron 4 defectos importantes y 10 menores. Decision de correccion: F4-E07 en
  docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md. Analisis con las secuencias y la disposicion de cada hallazgo: docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/1/R20261110T184730Z-e455/analysis.md.
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3; 1 invocacion de Worker. nc4 ya satisfecho en la cadena por R20261110T181147Z-4f9b (README section 10: solo la primera delegacion).
- El contrato del intento 2 reescribe INV-F4T1-17 y F4T1-C03 para este intento: los diez archivos del alcance ya existen (los creo el intento 1) y se modifican; sustituir la guarda temporal de alcance por la guarda de preview de INV-F4T1-24 lo exige el contrato y no es debilitar una guarda.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Linea base: FullyQualifiedName~RackCad.Tests.Workspace = 1165, todas en verde (CI 42877329473); la suite Core completa 16703 en verde.
  * Las guardas recursivas sobre src/RackCad.Application/Workspace/ siguen aplicando a Drafts/ (sin Transaction, Commit(, Provider, Metric, Aggregat, Regen, async ni await en el codigo).
  * DraftRegistry.Begin(session, rackId, read, comparer) es hoy la unica via de nacimiento; el copiador de INV-F4T1-19 se anade a esa via. Cambiar la firma dentro de los diez archivos esta dentro del alcance.
  * WorkspaceDraftBoundaryTests usa la clase interna WorkspaceHostSource (Src.Files, Src.Text, Src.CodeOnly, Src.AllProduct, Src.ApplicationFiles).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las diez rutas del contrato, en su orden, sin anadir ni quitar ninguna. Nunca un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/2/R20261110T191243Z-5b17/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/2/R20261110T191243Z-5b17/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro, tambien RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (66 entradas). RequiredTests con el mismo elemento.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T1-C05 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261110T191243Z-5b17 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T1-DRAFT-CORE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "13775d8fb89f3ac983fbeeb6bdcf749e7fd746fc" (NO es el BaseSha: es el de la primera delegacion de la cadena).
- ChainRedSha = "a362744ceaf4c6a86315cd1fcac688f63cd1de93" (el RED acreditado vigente).
- ChainRedFiles = exactamente, en este orden:
    tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftSessionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftStaleBaseTests.cs
- CorrectionOf = { "RunId": "R20261110T184730Z-e455", "FailureClass": "StopCondition", "AnalysisSha256": "da6ea61a85772f943bae3fc5047cf5e7f0111cb7007dca572f6e69171c006681" }.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T191243Z-5b17" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria = EXACTAMENTE la lista del archivo artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/2/R20261110T191243Z-5b17/acceptance-criteria-attempt2.json, misma lista y mismo orden, byte a byte. CARGALA con una orden, igual que las colecciones opacas; no la redactes ni la resumas.
  Lee esos criterios y confirma que son ejecutables con el alcance dado; si alguno no lo fuera, para con P-03 explicado en RoutingReason.
- IssuedBy = "Controller Codex (planificacion de la correccion del intento 2 de F4-T1-DRAFT-CORE, CONTROLLER_PLANNING)".
