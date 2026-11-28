I-64 - DELEGACION F4-T1-DRAFT-CORE - Controller, perfil CONTROLLER_PLANNING (CORRECCION bajo AUTOMATION_PLAN 16.8; CorrectionOf R20261110T194129Z-0d3b; intento 3; una unica invocacion; solo N-1)
Identidad: unidad I-64; gate F4; tarea F4-T1-DRAFT-CORE; intento 3; RunId R20261110T224523Z-d088;
           DelegationRunId R20261110T224523Z-d088; AuthorityRevision c31854f81c3c301eba53e35cd99c66bcaf97e587 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera d737b4853b204d6b8cc6adcf2abe97d96d7ce88e, hijo directo de la AuthorityRevision:
           solo anade el contrato del intento 3, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T224523Z-d088
Contrato de gate: artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/3/R20261110T224523Z-d088/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/gate-contract-attempt3.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la correccion del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 27 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           lee docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/2/R20261110T194129Z-0d3b/analysis.md, docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/2/R20261110T194129Z-0d3b/independent-review.md (N-1), docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1-attempt3-exception.md, docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/n1_hashes.py y tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 115 a section 117 de docs/automation/evidence/I-64-evidence.md)
- La entrega del intento 2 (RED 500a336d, GREEN 09955667) quedo con EXECUTION_VERIFIED (verificacion R20261110T194129Z-0d3b) y nc1..nc3 CONTROL PASS, que se conservan. La revision independiente hallo N-1: la regla BOM de WorkspaceDraftBoundaryTests distingue mayusculas y ya no detecta BOM, contra INV-F4T1-22.
  El Owner autorizo el intento 3 de 3 solo para N-1 (F4-E09) y una excepcion acotada a 16.8 (F4-E10): RED solo con el bloque de regresion; GREEN solo con la region de la regla; prueba fina con n1_hashes.py (H1, H2 y H3).
  Presupuesto: Attempt 3, AttemptsRemaining 0, MaxReworkLoops 3; 1 invocacion de Worker. nc4 ya satisfecho en la cadena por R20261110T181147Z-4f9b (README section 10: solo la primera delegacion). N-2 queda fuera (obligacion de F4-T2).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Linea base: FullyQualifiedName~RackCad.Tests.Workspace = 1273, todas en verde (CI 42886956643); la suite Core completa 16811 en verde.
  * En tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs la regla BOM es una sola constante: private const string BomInAnyIdentifier, con un comentario de una linea encima y una linea en blanco antes; la usan I63Concepts (texto de codigo) y ForbiddenMemberNames (nombres de miembro), ambas con RegexOptions.IgnoreCase.
  * El valor actual envuelve la regla en (?-i:...), y por eso no detecta BOM en mayusculas. Las reglas de 90a81104 eran Bom al inicio de palabra en el texto de codigo y Bom al final en los nombres de miembro, sin distinguir mayusculas; la guarda recursiva del padre (WorkspaceBridgeSourceGuardTests) usa la primera.
  * La clase termina en una llave de cierre en su ultima linea; el bloque del RED puede ir justo antes, lejos de la region de la regla.
  * n1_hashes.py se ejecuta desde el worktree: python docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/n1_hashes.py <BaseSha> <RedSha> <CurrentSha>.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE la unica ruta del contrato, sin anadir ninguna. Nunca un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/3/R20261110T224523Z-d088/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/3/R20261110T224523Z-d088/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro, tambien RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (66 entradas). RequiredTests con el mismo elemento.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T1-C05 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261110T224523Z-d088 (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T1-DRAFT-CORE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "13775d8fb89f3ac983fbeeb6bdcf749e7fd746fc" (NO es el BaseSha: es el de la primera delegacion de la cadena).
- ChainRedSha = "500a336dbb690faef463c6effc52b402feade87e" (el RED acreditado vigente, por la verificacion R20261110T194129Z-0d3b).
- ChainRedFiles = exactamente, en este orden:
    tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftSessionTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftStaleBaseTests.cs
- CorrectionOf = { "RunId": "R20261110T194129Z-0d3b", "FailureClass": "StopCondition", "AnalysisSha256": "e675a3705fa413fa4ca08f22cf08a247be6427ba0335d27c678324b92a556274" }.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T224523Z-d088" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria = EXACTAMENTE la lista del archivo artifacts/orchestration/I-64/F4-T1-DRAFT-CORE/3/R20261110T224523Z-d088/acceptance-criteria-attempt3.json, misma lista y mismo orden, byte a byte. CARGALA con una orden, igual que las colecciones opacas; no la redactes ni la resumas.
  Lee esos criterios y confirma que son ejecutables con el alcance dado; si alguno no lo fuera, para con P-03 explicado en RoutingReason.
- IssuedBy = "Controller Codex (planificacion de la correccion del intento 3 de F4-T1-DRAFT-CORE, solo N-1, CONTROLLER_PLANNING)".
