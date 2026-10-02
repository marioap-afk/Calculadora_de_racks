I-64 - DELEGACION F1-T1-MODEL - Controller, perfil CONTROLLER_PLANNING (planificacion de CORRECCION, attempt 2; cuarta y ultima)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 2; RunId R20261002T140200Z-3886;
           DelegationRunId R20261002T140200Z-3886; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree (se espera 1504c938dc8ac050a8e0d61c5db6c9977b638600);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261002T140200Z-3886
Contrato de gate: artifacts/orchestration/I-64/F1-T1-MODEL/2/R20261002T140200Z-3886/gate-contract.json (UTF-8; el mismo contrato
           reemitido de las delegaciones aceptadas, sin cambios)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) de la CORRECCION del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-F1-T1-01..12); una delegacion nunca amplia el contrato
Rutas y archivos calientes: tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs (CS8632 en la linea 62),
           las demas pruebas de la cadena, src/RackCad.Application/Workspace/ (implementacion del GREEN b38e54f9),
           eng/ci/verify-autocad-references.ps1 y .github/workflows/ci.yml (job "Build Plugin without AutoCAD"; solo lectura),
           docs/adr/0047-workspace-persistente-modeless-rackcad.md (creado por esta cadena en 36c344dc; no cambia).
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.24 y Sec.25 de docs/automation/evidence/I-64-evidence.md)
- Correccion attempt 1 (delegacion R20261002T061355Z-c5cd) -> Worker R20261002T062258Z-de0f: RED nuevo
  189353f8df954b05cbbcdc0f15dca0cb3bf21002, GREEN b38e54f9f434fee7aa41ce6dcfd1804978afd619 -> verificacion
  R20261002T063546Z-0cd7: EXECUTION_REWORK_REQUIRED / REWORK, FailureClass Ci; RED nuevo acreditado.
- Causa aceptada por el Coordinator: la build Release del job "Build Plugin without AutoCAD" (eng/ci/verify-autocad-references.ps1,
  Assert-DotNetSuccess -NoWarnings) rechaza CS8632 en tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs(62,75):
  `string? id` en un archivo sin contexto de anotaciones nullable. No es defecto del Plugin. Latente desde 36c344dc.
- El Coordinator acepto el REWORK, autorizo esta CUARTA Y ULTIMA planificacion y fijo attempt = 2 (estado: attempts 2,
  max_attempts 3). Misma FailureClass que el REWORK anterior: AUTOMATION_PLAN 16.8 no exige analysis.md nuevo y el
  Coordinator ordeno no inventarlo, asi que CorrectionOf.AnalysisSha256 = null.

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
  Usa literalmente la etiqueta de la columna "Clase" de routing.md Sec.1 que corresponda y su Perfil como PromptProfile;
  explica la eleccion en RoutingReason.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "medium"; Effort.Semantic = "Balanced";
  ModelEscalationReason = null salvo escalado respaldado por evidencia dentro de las celdas del contrato.

DELTA DE LA TAREA (correccion attempt 2; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261002T140200Z-3886 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T1-MODEL".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- ChainBaseSha = "e0587355b0e84d98807057a56dfc50591da87489" (el de la primera delegacion, no el BaseSha).
- ChainRedSha = "189353f8df954b05cbbcdc0f15dca0cb3bf21002" (el RED acreditado vigente).
- ChainRedFiles = exactamente, en este orden:
  "tests/RackCad.Tests/Workspace/SelectionContextTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs", "tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs",
  "tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs".
- CorrectionOf = { "RunId": "R20261002T063546Z-0cd7", "FailureClass": "Ci", "AnalysisSha256": null }.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (e0587355...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T1-MODEL/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T140200Z-3886" }; RoutingEnforcement = "required".
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la decision del Coordinator
  (evidencia Sec.25), de la Proposal V5 congelada y del contrato; como minimo:
  (1) WorkspaceSelectionContextTests.cs habilita localmente solo el contexto de anotaciones nullable con la directiva
  `#nullable enable annotations` (o la forma minima local equivalente que acepte el compilador del repositorio),
  conservando la semantica de `string?`; sin tocar NamespaceFolderGuardTests, scripts de CI, el nullable global, ningun
  .csproj ni el Plugin; sin suprimir ni debilitar avisos (nada de #pragma warning disable ni NoWarn) y sin quitar cobertura;
  (2) RED nuevo segun 16.8 (esta entrega toca ChainRedFiles): un commit con la correccion nullable y el comportamiento
  congelado desactivado en src/RackCad.Application/Workspace/, que compila y falla por asercion real con seleccion >= 1,
  sin fallo ni aviso artificial;
  (3) GREEN: restaura el comportamiento y no toca ningun archivo de RT ni de ChainRedFiles;
  (4) en GREEN, el filtro del contrato y la suite Core completa pasan;
  (5) la build Release no produce CS8632 ni ningun aviso nuevo CS/MSB/NU/NETSDK: el Worker ejecuta en local, antes del push
  final, eng/ci/verify-autocad-references.ps1 con rutas aisladas como en .github/workflows/ci.yml (no requiere AutoCAD); si
  no puede ejecutarse por causas de entorno ajenas al codigo, lo registra en KnownLimitations y comprueba al menos la build
  Release del proyecto de pruebas sin avisos CS en los archivos de Workspace;
  (6) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success;
  (7) el ADR 0047 no cambia; todos los archivos modificados quedan dentro de AllowedWriteScope.
- IssuedBy = "Controller Codex (planificacion de correccion F1-T1-MODEL attempt 2, CONTROLLER_PLANNING)".
