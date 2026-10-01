I-61 — DELEGACION g3-cama-d1a — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-61; gate G3; tarea g3-cama-d1a; intento 0; RunId R20261001T032333Z-4a2d;
           DelegationRunId R20261001T032333Z-4a2d; AuthorityRevision 7b8662c5190bb22c0b4aa7ea49ea31367f4bb359;
           MainSha 95690c28dc6268e61dff32a0cbc33cc9fde3d47f; BaseSha = el HEAD actual del worktree;
           rama architecture/protocolo-ejecucion-agentes; worktree = el directorio de trabajo actual; Owner subagent:R20261001T032333Z-4a2d
Autoridades (leer desde Git, no copiar): las del contrato de gate (campo Authorities, con ruta, seccion y clase);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el
           contrato de gate artifacts/orchestration/I-61/g3-cama-d1a/0/R20261001T032333Z-4a2d/gate-contract.json.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-01, INV-02, INV-05, INV-10 (una delegacion nunca amplia el contrato)
Rutas y archivos calientes: src/RackCad.Plugin/RackCamaCommands.cs (EditCama, payload y SyncName);
           referencia de la semantica: las otras cinco rutas de edicion (RackSelectivoCommands.cs, RackDinamicoCommands.cs,
           RackPushBackCommands.cs, RackCantileverCommands.cs, RackCabeceraCommands.cs), que no se tocan;
           src/RackCad.Application/Persistence/; tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs
Criterios de aceptacion: la salida valida contra el esquema; alcance, invariantes, pruebas, autoridades y paradas
           tomados del contrato sin ampliarlos; celda y effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03; ante cualquiera, para y explica el bloqueo en RoutingReason
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» en el texto libre
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

DELTA DE LA TAREA (G3, primera delegacion de la cadena)
- RunId = R20261001T032333Z-4a2d (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3.
- Unit = "I-61"; Initiative = "I-61"; Gate = "G3"; TaskId = "g3-cama-d1a" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-61/g3-cama-d1a/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261001T032333Z-4a2d" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda elegida;
  Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el effort semantico de routing.md.
- AllowedWriteScope: puedes estrecharlo a archivos exactos dentro del alcance del contrato (sintaxis: archivo exacto o
  prefijo terminado en /); nombra el archivo nuevo de la funcion pura en src/RackCad.Application/Persistence/ y los
  archivos nuevos de las pruebas en tests/RackCad.Tests/, con nombres de clase que casen con los filtros del contrato.
- ForbiddenWriteScope, Invariants, StopConditions, Authorities y RequiredTests: todos los del contrato (puedes anadir,
  nunca quitar); RequiredTests con MinSelected >= y el mismo ExpectRed.
- AcceptanceCriteria: concretos y comprobables (OBL-P1 con sus cuatro casos, OBL-P2 con sus cuatro condiciones de fallo
  y sus tres mutaciones, OBL-P3), tal como los define docs/initiatives/I-61-proposal-v9.md §13; RED compilable primero.
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G3, CONTROLLER_PLANNING)".
