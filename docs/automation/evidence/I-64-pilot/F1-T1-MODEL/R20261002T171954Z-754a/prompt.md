I-64 - DELEGACION F1-T1-MODEL - Controller, perfil CONTROLLER_PLANNING (planificacion de RECUPERACION, attempt 3; planificacion 7 de 7, la ultima)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 3; RunId R20261002T171954Z-754a;
           DelegationRunId R20261002T171954Z-754a; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree (se espera 0b7db52f12ddbf7ed36c8c2d020645818335c0fa);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261002T171954Z-754a
Contrato de gate: artifacts/orchestration/I-64/F1-T1-MODEL/3/R20261002T171954Z-754a/gate-contract.json (ASCII puro; reemision ASCII del Coordinator,
           REISSUE_GATE_CONTRACT_ASCII: el mismo contrato sin diacriticos; mismos IDs, rutas, alcance, pruebas y celdas)
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.26 a Sec.30 de docs/automation/evidence/I-64-evidence.md)
- La entrega R20261002T141605Z-dc2b (delegacion R20261002T140200Z-3886) quedo EXECUTION_VERIFIED sobre
  8d9a0c6e6ea148caf3e8f20d26a95a864a2943ce (verificacion R20261002T143410Z-ae0b; RED acreditado a9778068).
- La sesion publico commits de custodia antes de los controles negativos (DEV-F1T1-02): HEAD ya no es aquel CurrentSha y
  los controles nc1/nc2 de entonces (R20261002T150553Z-09df, R20261002T151201Z-abb1) quedaron SUPERSEDED_BY_CONTROL_RECOVERY.
- El Coordinator confirmo el STOP y autorizo la recuperacion (d): una entrega final semanticamente neutra sobre la punta
  actual, su verificacion y nc1..nc3 inmediatos. attempts = 3 (maximo).
- La quinta planificacion (R20261002T164250Z-f842) quedo REJECTED_BEFORE_WORKER: su salida corrompio los caracteres no
  ASCII como U+0000 seguido de texto hexadecimal (A5 FAIL). El Coordinator autorizo una reejecucion de la fase
  (6 de 6); attempts y Attempt siguen en 3. Analisis: docs/automation/evidence/I-64-pilot/F1-T1-MODEL/
  R20261002T164250Z-f842/analysis.md, SHA-256 415811c5c48be9bebaae74fb454c0ba73d39b9c4644f4eed93a0d604506b6fb7.
- La sexta planificacion (R20261002T165806Z-eef1) tambien quedo REJECTED_BEFORE_WORKER (A5 FAIL): su salida transformo los
  caracteres no ASCII en escapes de caracteres de control (por ejemplo, TaskClass "Documentaci" + tabulador + "n"). El
  Coordinator reemitio el contrato en ASCII puro (sin cambiar su significado) y autorizo esta planificacion 7 de 7, la ultima;
  attempts y Attempt siguen en 3.
- Analisis del Coordinator (literal): docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/R20261002T151201Z-abb1/analysis.md,
  SHA-256 6fb6b914dd47cb43dff341868edf701ffe1fd89c8096bbcdad2b304e98b68b66. Leelo completo.

COPIA EXACTA DEL CONTRATO ASCII (regla del Coordinator; A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F1-T1-MODEL/3/R20261002T171954Z-754a/gate-contract.json son ahora ASCII puro: por orden
  del Coordinator se eliminaron los diacriticos a proposito (por ejemplo "sesion", "Seleccion", "planificacion", "ambiguedad"). Se copian byte a byte.
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective
  son DATOS OPACOS de ese contrato. No los copies a mano: CARGALOS desde el archivo con una orden y reproducelos en tu salida
  a partir de esa carga, por ejemplo con python desde pwsh:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F1-T1-MODEL/3/R20261002T171954Z-754a/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','AllowedWriteScope','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- Los valores Section de Authorities son la transliteracion ASCII de los encabezados (en Git los documentos conservan sus
  tildes): copia el valor del contrato, nunca el encabezado del documento.
- TODA tu salida debe ser ASCII puro, tambien en los campos que redactas tu (TaskClass, RoutingReason, AcceptanceCriteria,
  IssuedBy): escribe sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun
  tabulador ni otro caracter de control dentro de las cadenas. La unica excepcion son los saltos de linea que ya trae el
  Objective del contrato (en JSON, barra invertida + n).
- Antes de emitir: comprueba que, al decodificar tu salida, cada uno de esos campos es identico (cadena por cadena) al del
  contrato, y que tu salida no contiene ningun byte no ASCII, ninguna barra invertida seguida de u y ningun tabulador; si no,
  corrige tu salida volviendo a usar la carga, nunca a mano.
- No estreches AllowedWriteScope (3 entradas) ni expandas ForbiddenWriteScope (17 entradas): la restriccion a un solo
  archivo de esta recuperacion va en AcceptanceCriteria. RequiredTests con el mismo Project, Filter, MinSelected y ExpectRed.

TASKCLASS (regla del Coordinator)
- TaskClass describe el trabajo que ejecutara el WORKER, no al Controller: NUNCA "Controller: planificacion / verificacion".
  Usa la etiqueta de la columna "Clase" de routing.md Sec.1 que corresponda, escrita en ASCII sin tildes (para este trabajo
  documental autorizado: exactamente Documentacion), y su Perfil como PromptProfile; explica la eleccion en RoutingReason.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "medium"; Effort.Semantic = "Balanced";
  ModelEscalationReason = null salvo escalado respaldado por evidencia dentro de las celdas del contrato.

DELTA DE LA TAREA (recuperacion de controles, attempt 3; AUTOMATION_PLAN 16.6, 16.8 y 16.11)
- RunId = R20261002T171954Z-754a (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T1-MODEL".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "e0587355b0e84d98807057a56dfc50591da87489" (el de la primera delegacion, no el BaseSha).
- ChainRedSha = "a9778068af566bc2b84ba6a0005711b6452a2666" (el RED acreditado vigente).
- ChainRedFiles = exactamente, en este orden:
  "tests/RackCad.Tests/Workspace/SelectionContextTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs".
- CorrectionOf = { "RunId": "R20261002T164250Z-f842", "FailureClass": "StopCondition",
  "AnalysisSha256": "415811c5c48be9bebaae74fb454c0ba73d39b9c4644f4eed93a0d604506b6fb7" } (minusculas).
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (e0587355...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T1-MODEL/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T171954Z-754a" }; RoutingEnforcement = "required".
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la decision del Coordinator
  (evidencia Sec.28 y Sec.29), de los analisis y del contrato; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha = exactamente src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs;
  (2) el unico cambio es el comentario de documentacion XML existente de la clase WorkspaceSessionRegistry, que deja
  explicito que es un registro puro y transitorio en memoria de sesiones vivas (una por instancia abierta de documento,
  D-03) y que no tiene autoridad de persistencia; sin cambio ejecutable, de API, de pruebas, del ADR, de Plugin, UI, Domain
  ni de configuracion, y sin marcadores de control;
  (3) esta entrega no toca ChainRedFiles, asi que no exige RED (16.8): un solo commit;
  (4) en el commit, el filtro del contrato y la suite Core completa pasan, y las builds Release no emiten avisos nuevos
  MSB/CS/NU/NETSDK;
  (5) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success.
- IssuedBy = "Controller Codex (planificacion de recuperacion F1-T1-MODEL attempt 3, planificacion 7/7, CONTROLLER_PLANNING)".
