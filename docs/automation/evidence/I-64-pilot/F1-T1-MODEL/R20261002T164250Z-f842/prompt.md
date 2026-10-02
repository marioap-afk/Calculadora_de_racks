I-64 - DELEGACION F1-T1-MODEL - Controller, perfil CONTROLLER_PLANNING (planificacion de RECUPERACION, attempt 3; quinta y ultima)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 3; RunId R20261002T164250Z-f842;
           DelegationRunId R20261002T164250Z-f842; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree (se espera 58d1140c895335ac78d3e61f77c461ad60e1f2b0);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261002T164250Z-f842
Contrato de gate: artifacts/orchestration/I-64/F1-T1-MODEL/3/R20261002T164250Z-f842/gate-contract.json (UTF-8; el mismo contrato
           reemitido de las delegaciones aceptadas, sin cambios)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) de la RECUPERACION del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-F1-T1-01..12); una delegacion nunca amplia el contrato
Rutas y archivos calientes: src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs (comentario XML de la clase, linea 6);
           las guardas de texto de tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs (solo lectura).
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass del trabajo del Worker; campos de cadena y
           CorrectionOf exactamente como en el DELTA
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git log --format=%H --diff-filter=A -- docs/adr/0047-workspace-persistente-modeless-rackcad.md (debe dar 36c344dc...,
           el RED de esta misma cadena) ; git ls-tree --name-only origin/main docs/adr/ (0047 no esta en main)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y C-01..C-12 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
           C-02 ("ADR 0047 deja de estar libre") se refiere a que OTRA unidad ocupe el numero; que el archivo exista en esta
           rama desde 36c344dc (creado por esta cadena) no lo activa.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" en el texto libre
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.26, Sec.27 y Sec.28 de docs/automation/evidence/I-64-evidence.md)
- La entrega R20261002T141605Z-dc2b (delegacion R20261002T140200Z-3886) quedo EXECUTION_VERIFIED sobre
  8d9a0c6e6ea148caf3e8f20d26a95a864a2943ce (verificacion R20261002T143410Z-ae0b; RED acreditado a9778068).
- La sesion publico commits de custodia antes de los controles negativos (DEV-F1T1-02): HEAD ya no es aquel CurrentSha y
  los controles nc1/nc2 de entonces (R20261002T150553Z-09df, R20261002T151201Z-abb1) quedaron SUPERSEDED_BY_CONTROL_RECOVERY.
- El Coordinator confirmo el STOP y autorizo la recuperacion (d): una entrega final semanticamente neutra sobre la punta
  actual, su verificacion y nc1..nc3 inmediatos. attempts = 3 (maximo); esta es la quinta y ultima planificacion.
- Analisis del Coordinator (literal): docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/R20261002T151201Z-abb1/analysis.md,
  SHA-256 6fb6b914dd47cb43dff341868edf701ffe1fd89c8096bbcdad2b304e98b68b66. Leelo completo.

COPIA EXACTA (regla del Coordinator; A4 y A5 se evaluan contra el texto literal del contrato)
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions
  son DATOS OPACOS. Copialos EXACTAMENTE desde gate-contract.json, elemento a elemento, en el mismo orden y con los
  mismos caracteres Unicode: no anadas ni quites tildes, enes o rayas; no normalices Unicode; no reformules, resumas,
  corrijas, traduzcas ni cambies mayusculas o comillas; no cambies los titulos de seccion (Section) ni las rutas;
  no interpretes equivalencias semanticas. No anadas ni quites elementos. No estreches AllowedWriteScope (3 entradas)
  ni expandas ForbiddenWriteScope (17 entradas): la restriccion a un solo archivo de esta recuperacion va en
  AcceptanceCriteria. RequiredTests con el mismo Project, Filter, MinSelected y ExpectRed.
- Copia tambien Objective exactamente desde el contrato.
- Lee el contrato con una orden que decodifique UTF-8 y vuelve a comprobar tu salida contra el archivo antes de emitirla.

TASKCLASS (regla del Coordinator)
- TaskClass describe el trabajo que ejecutara el WORKER, no al Controller: NUNCA "Controller: planificacion / verificacion".
  Usa literalmente la etiqueta de la columna "Clase" de routing.md Sec.1 que corresponda y su Perfil como PromptProfile;
  explica la eleccion en RoutingReason.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "medium"; Effort.Semantic = "Balanced";
  ModelEscalationReason = null salvo escalado respaldado por evidencia dentro de las celdas del contrato.

DELTA DE LA TAREA (recuperacion de controles, attempt 3; AUTOMATION_PLAN 16.6, 16.8 y 16.11)
- RunId = R20261002T164250Z-f842 (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T1-MODEL".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "e0587355b0e84d98807057a56dfc50591da87489" (el de la primera delegacion, no el BaseSha).
- ChainRedSha = "a9778068af566bc2b84ba6a0005711b6452a2666" (el RED acreditado vigente).
- ChainRedFiles = exactamente, en este orden:
  "tests/RackCad.Tests/Workspace/SelectionContextTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs".
- CorrectionOf = { "RunId": "R20261002T151201Z-abb1", "FailureClass": "StopCondition",
  "AnalysisSha256": "6fb6b914dd47cb43dff341868edf701ffe1fd89c8096bbcdad2b304e98b68b66" } (minusculas).
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (e0587355...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T1-MODEL/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T164250Z-f842" }; RoutingEnforcement = "required".
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la decision del Coordinator
  (evidencia Sec.28), del analisis y del contrato; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha = exactamente src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs;
  (2) el unico cambio es el comentario de documentacion XML existente de la clase WorkspaceSessionRegistry, que deja
  explicito que es un registro puro y transitorio en memoria de sesiones vivas (una por instancia abierta de documento,
  D-03) y que no tiene autoridad de persistencia; sin cambio ejecutable, de API, de pruebas, del ADR, de Plugin, UI, Domain
  ni de configuracion, y sin marcadores de control;
  (3) esta entrega no toca ChainRedFiles, asi que no exige RED (16.8): un solo commit;
  (4) en el commit, el filtro del contrato y la suite Core completa pasan, y las builds Release no emiten avisos nuevos
  MSB/CS/NU/NETSDK;
  (5) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success.
- IssuedBy = "Controller Codex (planificacion de recuperacion F1-T1-MODEL attempt 3, CONTROLLER_PLANNING)".
