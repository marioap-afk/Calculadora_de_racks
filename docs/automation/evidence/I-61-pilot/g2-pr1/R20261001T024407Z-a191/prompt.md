I-61 — DELEGACION g2-pr1-dryrun — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-61; gate G2; tarea g2-pr1-dryrun; intento 0; RunId R20261001T024407Z-a191;
           DelegationRunId R20261001T024407Z-a191; AuthorityRevision 4da82667f47b227507166428817ececa8239d73a;
           MainSha 95690c28dc6268e61dff32a0cbc33cc9fde3d47f; BaseSha = el HEAD actual del worktree;
           rama architecture/protocolo-ejecucion-agentes; worktree = el directorio de trabajo actual; Owner subagent:R20261001T024407Z-a191
Autoridades (leer desde Git, no copiar): docs/AUTOMATION_PLAN.md §16 (UNIT_CHANGE);
           docs/automation/agent-execution/routing.md (UNIT_CHANGE); docs/automation/agent-execution/model-catalog.md;
           docs/initiatives/I-61-proposal-v9.md (UNIT_DOC); AGENTS.md (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) para la tarea descrita en el
           contrato de gate artifacts/orchestration/I-61/g2-pr1/0/R20261001T024407Z-a191/gate-contract.json.
           Es un ensayo de planificacion: nadie ejecutara la delegacion.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-01, INV-02, INV-05, INV-10 (una delegacion nunca amplia el contrato)
Rutas y archivos calientes: ninguno se modifica
Criterios de aceptacion: la salida valida contra el esquema; alcance, invariantes, pruebas y paradas copiados
           del contrato sin ampliarlos; celda elegida entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; (git ls-files | Measure-Object).Count
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03; ante cualquiera, para y explica el bloqueo en RoutingReason
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner
Trailer: no aplica (no hay commits)
Informe esperado: la salida exacta del esquema rackcad-delegation/v1 (la escribe el CLI por -o)

PERFIL CONTROLLER_PLANNING
Metodo: clasificar y enrutar dentro del contrato de gate, sin escribir en Git.
1. Lee el contrato, docs/automation/agent-execution/routing.md y el catalogo.
2. Clasifica la tarea, puntua las siete dimensiones y elige entre las celdas del
   contrato el nivel mas bajo adecuado; registra RoutingReason con fechas.
3. Copia del contrato el alcance, los invariantes, las pruebas y las paradas; nunca
   los amplies. Nombra los archivos nuevos exactos.
4. Rellena el esquema de delegacion completo; null solo donde el esquema lo admite.

DELTA DE LA TAREA (sonda PR-1)
- RunId = R20261001T024407Z-a191 (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3.
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f; AuthorityRevision = 4da82667f47b227507166428817ececa8239d73a.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-61/g2-pr1-dryrun/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261001T024407Z-a191" }; RoutingEnforcement = "advisory" (el del contrato).
- RoutingReason DEBE empezar exactamente por el texto `PR1: ls-files=<n>; ` donde <n> es el numero que devuelve
  `(git ls-files | Measure-Object).Count`; despues, la razon del enrutamiento con la fecha de verificacion de la celda.
- IssuedBy = "Controller Codex (planificacion, sonda PR-1)".
