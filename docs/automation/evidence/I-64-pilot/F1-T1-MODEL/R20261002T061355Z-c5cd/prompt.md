I-64 - DELEGACION F1-T1-MODEL - Controller, perfil CONTROLLER_PLANNING (planificacion de CORRECCION, attempt 1)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 1; RunId R20261002T061355Z-c5cd;
           DelegationRunId R20261002T061355Z-c5cd; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree (se espera b2db5326924cd66def7a83628bc6102db50b80f2);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261002T061355Z-c5cd
Contrato de gate: artifacts/orchestration/I-64/F1-T1-MODEL/1/R20261002T061355Z-c5cd/gate-contract.json (UTF-8; el mismo contrato
           reemitido de la delegacion aceptada R20261002T003827Z-3a76, sin cambios)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) de la CORRECCION del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-F1-T1-01..12); una delegacion nunca amplia el contrato
Rutas y archivos calientes: tests/RackCad.Tests/Workspace/ (las 4 pruebas de la cadena), src/RackCad.Application/Workspace/
           (implementacion del GREEN 66d34af3), tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda integrada; solo lectura,
           NO se modifica), docs/adr/0047-workspace-persistente-modeless-rackcad.md (creado por esta cadena en 36c344dc).
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass del trabajo del Worker; campos de cadena y
           CorrectionOf exactamente como en el DELTA
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git log --format=%H --diff-filter=A -- docs/adr/0047-workspace-persistente-modeless-rackcad.md (debe dar 36c344dc...,
           el RED de esta misma cadena) ; git ls-tree --name-only origin/main docs/adr/ (0047 no esta en main) ;
           sha256 del analisis citado abajo
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.22 y Sec.23 de docs/automation/evidence/I-64-evidence.md)
- Delegacion aceptada R20261002T003827Z-3a76 -> Worker R20261002T005244Z-f656: RED 36c344dc283d4a9855110a31ca6b20a2da7fd972,
  GREEN 66d34af31524c49214e38479d5892cd9ec985eb9 -> verificacion R20261002T011239Z-647e: EXECUTION_REWORK_REQUIRED / REWORK,
  FailureClass Ci; RED acreditado (ChainRedFiles = las 4 pruebas de abajo).
- El Coordinator acepto el REWORK, autorizo esta planificacion (3 de 3) y fijo attempt = 1 (estado: attempts 1, max_attempts 3).
- Analisis del Coordinator (literal): artifacts/orchestration/I-64/F1-T1-MODEL/0/R20261002T011239Z-647e/analysis.md, custodiado en
  docs/automation/evidence/I-64-pilot/F1-T1-MODEL/R20261002T011239Z-647e/analysis.md; SHA-256
  b0ddc8de37199ba703efa180b6bf8caab1a2230dc1469d9a53c6159315b98664. Leelo completo: fija las dos causas y la correccion.

COPIA EXACTA (regla del Coordinator; A4 y A5 se evaluan contra el texto literal del contrato)
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions
  son DATOS OPACOS. Copialos EXACTAMENTE desde gate-contract.json, elemento a elemento, en el mismo orden y con los
  mismos caracteres Unicode: no anadas ni quites tildes, enes o rayas; no normalices Unicode; no reformules, resumas,
  corrijas, traduzcas ni cambies mayusculas o comillas; no cambies los titulos de seccion (Section) ni las rutas;
  no interpretes equivalencias semanticas. No anadas ni quites elementos. No estreches AllowedWriteScope (3 entradas)
  ni expandas ForbiddenWriteScope (17 entradas). RequiredTests con el mismo Project, Filter, MinSelected y ExpectRed.
- Copia tambien Objective exactamente desde el contrato.
- Lee el contrato con una orden que decodifique UTF-8 y vuelve a comprobar tu salida contra el archivo antes de emitirla.

TASKCLASS (regla del Coordinator)
- TaskClass describe el trabajo que ejecutara el WORKER, no al Controller: NUNCA "Controller: planificacion / verificacion".
  El trabajo es la correccion de las pruebas de la cadena y su RED/GREEN sobre el modelo puro de Workspace, con escritura,
  commit y push, alcance acotado, effort semantico Balanced. Usa literalmente la etiqueta de la columna "Clase" de
  routing.md Sec.1 que corresponda y su Perfil como PromptProfile; explica la eleccion en RoutingReason.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "medium"; Effort.Semantic = "Balanced";
  ModelEscalationReason = null salvo escalado respaldado por evidencia dentro de las celdas del contrato.

DELTA DE LA TAREA (correccion; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261002T061355Z-c5cd (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T1-MODEL".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "e0587355b0e84d98807057a56dfc50591da87489" (NO es el BaseSha: es el de la primera delegacion).
- ChainRedSha = "36c344dc283d4a9855110a31ca6b20a2da7fd972" (el RED acreditado vigente; se conserva).
- ChainRedFiles = exactamente, en este orden:
  "tests/RackCad.Tests/Workspace/SelectionContextTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs".
- CorrectionOf = { "RunId": "R20261002T011239Z-647e", "FailureClass": "Ci",
  "AnalysisSha256": "b0ddc8de37199ba703efa180b6bf8caab1a2230dc1469d9a53c6159315b98664" } (minusculas).
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (e0587355...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T1-MODEL/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T061355Z-c5cd" }; RoutingEnforcement = "required".
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO del analisis del Coordinator,
  de la Proposal V5 congelada y del contrato; como minimo:
  (1) las pruebas de la cadena declaran exactamente `namespace RackCad.Tests` y sus clases se llaman Workspace...Tests, de
  modo que el filtro del contrato FullyQualifiedName~RackCad.Tests.Workspace las sigue seleccionando (seleccion >= 1);
  (2) NamespaceFolderGuardTests no se modifica y queda en verde;
  (3) la guarda de datos authored se acota a autoridades concretas de persistencia o escritura y APIs prohibidas, sigue
  detectando dependencias reales de persistencia o escritura authored y no rechaza un registro de sesiones en memoria por
  llamarse ...Registry; INV-F1-T1-03 e INV-F1-T1-04 no se debilitan;
  (4) RED segun 16.8: esta entrega toca ChainRedFiles, asi que exige RED: un commit RED con la correccion desactivada
  (la implementacion de src/RackCad.Application/Workspace/ sin el comportamiento congelado, por ejemplo el esqueleto
  de 36c344dc) y TODOS los cambios de las pruebas de la cadena, que falla por asercion de comportamiento congelado con
  seleccion >= 1, sin fallo de compilacion ni prueba artificial; despues un GREEN que restaura la implementacion y no
  toca RT ni ChainRedFiles;
  (5) GREEN: el filtro del contrato en verde y la suite Core completa en verde (incluida NamespaceFolderGuardTests);
  (6) el ADR 0047 solo cambia si no hay cambio semantico; si no, queda intacto; sigue propuesto;
  (7) todos los archivos modificados estan dentro de AllowedWriteScope; nada en Plugin, UI, Domain ni AutoCAD.
- IssuedBy = "Controller Codex (planificacion de correccion F1-T1-MODEL attempt 1, CONTROLLER_PLANNING)".
