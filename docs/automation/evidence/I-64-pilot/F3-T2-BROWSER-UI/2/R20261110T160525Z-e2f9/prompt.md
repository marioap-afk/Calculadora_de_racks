I-64 - DELEGACION F3-T2-BROWSER-UI - Controller, perfil CONTROLLER_PLANNING (CORRECCION bajo AUTOMATION_PLAN 16.8; CorrectionOf R20261110T154803Z-fc18; intento 2; una unica invocacion)
Identidad: unidad I-64; gate F3; tarea F3-T2-BROWSER-UI; intento 2; RunId R20261110T160525Z-e2f9;
           DelegationRunId R20261110T160525Z-e2f9; AuthorityRevision 33869690cef828dbf011afb9a2fe05af9250b375 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 29cf0d3e9c538a2bf613ac91afebbe2350cecb18, hijo directo de la AuthorityRevision:
           solo anade el contrato del intento 2, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261110T160525Z-e2f9
Contrato de gate: artifacts/orchestration/I-64/F3-T2-BROWSER-UI/2/R20261110T160525Z-e2f9/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F3-T2-BROWSER-UI/coordinator/gate-contract-attempt2.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la correccion del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 26 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           lee docs/automation/evidence/I-64-pilot/F3-T2-BROWSER-UI/1/R20261110T154803Z-fc18/analysis.md, src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs, src/RackCad.Plugin/Workspace/WorkspaceController.cs y src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F3T2-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 106 y section 107 de docs/automation/evidence/I-64-evidence.md)
- La entrega del intento 1 (RED 1c379a23, GREEN 2c36b36b) quedo con EXECUTION_VERIFIED (verificacion R20261110T154803Z-fc18), que se conserva. La revision de producto hallo seis defectos que ninguna prueba ejercia. Decision de correccion: F3-E09 en
  docs/automation/evidence/I-64-pilot/F3-BROWSER/f3-dispositions.md. Analisis con las secuencias: docs/automation/evidence/I-64-pilot/F3-T2-BROWSER-UI/1/R20261110T154803Z-fc18/analysis.md.
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3; 1 invocacion de Worker. nc4 ya satisfecho en la cadena por R20261110T145634Z-c2c6 (README section 10: solo la primera delegacion).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Linea base: FullyQualifiedName~RackCad.UI.Tests.Workspace = 203 y FullyQualifiedName~RackCad.Tests.Workspace = 891, todas en verde (CI 42847080959).
  * WorkspaceHostC2SourceGuardTests.ARelayedCommandStateChangeOnlyAnnouncesTheState fija OnCommandStateChanged() => NotifyStateChanged(); y no se toca. WorkspacePaletteHostSourceGuardTests.ADrainKeepsTheHostCallAndItsContainmentAndThenAnnouncesTheState exige NotifyStateChanged() despues de _host.OnDrained(outcome) dentro de OnDrained, y una ventana alrededor lo cumple.
  * WorkspaceNavigationAuthorityTests.OnlyTheAuthorityTraversesTheReferenceHandlesOfTheFacts admite ModelSpaceReferenceHandles en WorkspaceNavigationFacts.cs; BrowserRackSummaries.cs no puede nombrarlo.
  * RackViewCodec esta en RackCad.Application.Systems.Shared; NavigationTargets lo usa con RackViewCodec.Decode(fact.Kind ?? string.Empty, fact.View, fact.Section), HasAddress y Address.Kind.
  * En el host, sin cadenas .Result ni += ni -=; RackLog.Exception una sola vez.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las rutas del contrato, en su orden, sin anadir ni quitar ninguna. Nunca un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F3-T2-BROWSER-UI/2/R20261110T160525Z-e2f9/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F3-T2-BROWSER-UI/2/R20261110T160525Z-e2f9/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- TODA tu salida debe ser ASCII puro, tambien RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- No expandas ni estreches ForbiddenWriteScope (89 entradas). RequiredTests con los mismos dos elementos.

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F3T2-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261110T160525Z-e2f9 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F3"; TaskId = "F3-T2-BROWSER-UI".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "8757c50b46b668ca454395a2f8ab87f4715c1b1a" (NO es el BaseSha: es el de la primera delegacion de la cadena).
- ChainRedSha = "1c379a23d2c8a728136c3d16d1b8ad85d87bc584" (el RED acreditado vigente).
- ChainRedFiles = exactamente, en este orden:
    tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserSummaryTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserScenarioHarnessTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserSummaryPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewTests.cs
- CorrectionOf = { "RunId": "R20261110T154803Z-fc18", "FailureClass": "StopCondition", "AnalysisSha256": "c2b19ce27a053760f007336f9d62d29d75b2247d2e8f51aa2f9219497e06a3bb" }.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F3-T2-BROWSER-UI/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261110T160525Z-e2f9" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria = EXACTAMENTE la lista del archivo artifacts/orchestration/I-64/F3-T2-BROWSER-UI/2/R20261110T160525Z-e2f9/acceptance-criteria-attempt2.json, misma lista y mismo orden, byte a byte. CARGALA con una orden, igual que las colecciones opacas; no la redactes ni la resumas.
  Lee esos criterios y confirma que son ejecutables con el alcance dado; si alguno no lo fuera, para con P-03 explicado en RoutingReason.
- IssuedBy = "Controller Codex (planificacion de la correccion del intento 2 de F3-T2-BROWSER-UI, CONTROLLER_PLANNING)".
