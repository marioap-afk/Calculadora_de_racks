I-64 - DELEGACION F2-T2B-NAVIGATION-UI - Controller, perfil CONTROLLER_PLANNING (CORRECCION bajo AUTOMATION_PLAN 16.8; CorrectionOf R20261110T053114Z-ffd2; intento 3; una unica invocacion)
Identidad: unidad I-64; gate F2; tarea F2-T2B-NAVIGATION-UI; intento 3; RunId R20261110T055138Z-d695;
           DelegationRunId R20261110T055138Z-d695; AuthorityRevision 988ccc3d2793b7627372dc58129981cdd884a04c (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 346edb66310da06e4b7296463ffd583d461f0235, hijo directo de la AuthorityRevision:
           solo anade el contrato del intento 3, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T055138Z-d695
Contrato de gate: artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/R20261110T055138Z-d695/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F2-T2B-NAVIGATION-UI/coordinator/gate-contract-attempt3.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la correccion del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           lee src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs, tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationPortDouble.cs y el analysis.md citado abajo
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 97 de docs/automation/evidence/I-64-evidence.md)
- La entrega del intento 2 (RED f64a13b3, GREEN 6c01f644) quedo con EXECUTION_VERIFIED (verificacion R20261110T053114Z-ffd2), que se conserva. Una revision de producto hallo tres defectos que ninguna prueba ejercia. Decision de correccion: D-04 en
  docs/automation/evidence/I-64-overnight/overnight-dispositions.md. Analisis con las secuencias: docs/automation/evidence/I-64-pilot/F2-T2B-NAVIGATION-UI/2/R20261110T053114Z-ffd2/analysis.md.
  Presupuesto: Attempt 3, AttemptsRemaining 0, MaxReworkLoops 3; 1 invocacion de Worker. nc4 ya satisfecho en la cadena por R20261110T045014Z-131c (README section 10: solo la primera delegacion).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Linea base: FullyQualifiedName~RackCad.UI.Tests.Workspace = 143 y FullyQualifiedName~RackCad.Tests.Workspace = 780, todas en verde (CI 42761455131).
  * El producto de la correccion vive solo en WorkspaceNavigationViewModel.cs: metodos EnterScope, ResetCopy, TextOf y ReasonOfBlock. WorkspaceNavigationPortDouble tiene Result, Responder y Calls y el ayudante Executed(int); no se modifica.
  * tests/RackCad.UI.Tests pertenece a la UI (WPF, net8.0-windows); el Plugin solo se compila en la CI y localmente con DLL de AutoCAD.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las dos rutas del contrato, en su orden, sin anadir ni quitar ninguna. Nunca un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/R20261110T055138Z-d695/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/R20261110T055138Z-d695/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro: sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (62 entradas). RequiredTests con los mismos dos elementos.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F2T2-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261110T055138Z-d695 (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F2"; TaskId = "F2-T2B-NAVIGATION-UI".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "2deb0454e4c41c15e02382437176ecbb9bbb4eda" (NO es el BaseSha: es el de la primera delegacion de la cadena).
- ChainRedSha = "f64a13b3827bf89ca6e65ddbbcaa7d124ccb3d74" (el RED acreditado vigente).
- ChainRedFiles = exactamente, en este orden:
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationViewTests.cs
- CorrectionOf = { "RunId": "R20261110T053114Z-ffd2", "FailureClass": "StopCondition", "AnalysisSha256": "3d0b5c17cef472958d912af433e53e3ba5661f57b0a0dd1052718f0abbe91be9" }.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T055138Z-d695" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria = EXACTAMENTE la lista del archivo artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/R20261110T055138Z-d695/acceptance-criteria-attempt3.json, misma lista y mismo orden, byte a byte. CARGALA con una orden, igual que las colecciones opacas; no la redactes ni la resumas.
  Lee esos criterios y confirma que son ejecutables con el alcance dado; si alguno no lo fuera, para con P-03 explicado en RoutingReason.
- IssuedBy = "Controller Codex (planificacion de la correccion del intento 3 de F2-T2B-NAVIGATION-UI, CONTROLLER_PLANNING)".
