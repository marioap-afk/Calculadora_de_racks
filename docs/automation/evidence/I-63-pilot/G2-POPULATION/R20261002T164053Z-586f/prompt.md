I-63 — DELEGACION G2-POPULATION — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G2; tarea G2-POPULATION; intento 2; RunId R20261002T164053Z-586f;
           DelegationRunId R20261002T164053Z-586f; AuthorityRevision 669d8a391f208e1077f136a406fd89058bc0ce6e;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T164053Z-586f
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3 congelada + A-1 + A-2 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164053Z-586f/gate-contract.json. Esta delegacion SI se ejecutara:
           el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-01, 02, 03, 06, 07, 08, 10, 11 y 32 en su parte de G2 segun la A-2, 12, 13, 33
           y 35, con D-10, D-10a, D-11, D-12, D-20 nivel Population, D-26 y D-27); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/ComputedParameters/ (G1: RackMetricDefinitionProjection D-10a, RackMetricDesignReader D-26,
           RackMetricRequest, RackCatalogInput, RackMetricIds, modelos y providers; se amplia en G2 sin romper G1);
           src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs (OutputBlockedReason, lineas 56-71 en 819955d6: la unica
           edicion del Plugin) y los demas *KindHandler.cs (solo lectura, objetivo de la guarda INV-35);
           src/RackCad.Application/Bom/RackBomOutputGate.cs y BomAuthoredAuthority.cs; src/RackCad.Application/Systems/PushBack/
           PushBackResolver.cs; src/RackCad.Application/Views/Insertion/RackEnvelopeIdProbe.cs;
           src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs (solo entrada tipada de E4);
           tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda de I-23)
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato proyectadas sin cambios; celda y
           effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ; git log --oneline -3
No-touch: todo el repositorio
Si tu mismo no puedes planificar (ambiguedad material o un paquete que saldria del contrato): no emitas un paquete parcial;
           explica el bloqueo en RoutingReason. Esta linea es para ti; NO es el campo StopConditions del paquete
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

DELTA DE LA TAREA (G2, primera delegacion de la cadena G2-POPULATION)
- RunId = R20261002T164053Z-586f (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por gate: orden del Coordinator, Q-G2-03).
- Unit = "I-63"; Initiative = "I-63"; Gate = "G2"; TaskId = "G2-POPULATION" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G2-POPULATION/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T164053Z-586f" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V3 §21 da para G2 el perfil inicial
  ROUTINE_IMPLEMENTATION (Deep, transversal).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164053Z-586f/gate-contract.json y proyecta, sin reconstruir, resumir, corregir
  ortografia, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164053Z-586f/gate-contract.json | ConvertFrom-Json; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  Deben permanecer literalmente, entre otras, las etiquetas sin tilde «## Convenciones arquitectonicas (obligatorias)» y
  «D1 (nucleo neutral) y D24» (asi estan en el contrato y en el encabezado real de AGENTS.md:57).
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class. Si algo difiere,
  corrige tu salida antes de terminar; nunca entregues una delegacion que sabes invalida. Registra en RoutingReason que el
  preflight dio igualdad en las seis.
- Archivos nuevos exactos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): nombra los archivos de
  produccion nuevos bajo src/RackCad.Application/ComputedParameters/ (poblacion, veredicto de salida, agregacion, contador) y
  las clases de prueba nuevas bajo tests/RackCad.Tests/ComputedParameters/, en namespace RackCad.Tests (guarda de I-23) y con
  nombre que empieza por ComputedParametersPopulation para que las seleccione el filtro del contrato. La unica edicion del
  Plugin es OutputBlockedReason de src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada INV del contrato tal como los definen
  docs/initiatives/I-63-proposal-v3.md §20 y las decisiones D-10, D-10a, D-11, D-12, D-26 y D-27, con la parte de G2 de INV-11
  e INV-32 de la A-2. Incluye:
  - RED compilable primero (esqueleto de produccion que compila y pruebas que fallan por asercion, seleccion >= 13), con push
    propio antes del GREEN para que su corrida push exista y su job Core falle por las aserciones; el RED no toca el handler;
  - INV-33: la guarda rechaza el OutputBlockedReason vigente en el RED (RED observable) y lo acepta tras la delegacion; el
    fixture con el texto literal de 819955d6 es rechazado por la misma funcion;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde (incluidas las pruebas de G1 y
    NamespaceFolderGuardTests); el build del Plugin sin AutoCAD en verde en la CI;
  - G1 intacto (RackMetricRequest y su precondicion ArgumentException); ProjectSummary Full, RackSummary.Metrics y
    RackComputedExpressionContext NO se implementan (si hicieran falta: parada C-08);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G2, CONTROLLER_PLANNING)".
