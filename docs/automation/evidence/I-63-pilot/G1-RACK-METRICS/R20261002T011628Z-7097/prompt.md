I-63 — DELEGACION G1-RACK-METRICS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 1 (correccion 1); RunId R20261002T011628Z-7097;
           DelegationRunId R20261002T011628Z-7097; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T011628Z-7097
Autoridades (leer desde Git, no copiar): las del contrato de gate (campo Authorities, con ruta, seccion y clase), mas el
           analisis del Coordinator docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T005944Z-1192/analysis.md
           (UNIT_DOC, en HEAD); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la CORRECCION 1 de la tarea
           descrita en el contrato de gate artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T011628Z-7097/gate-contract.json,
           segun el analysis.md citado. Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-04, INV-05, INV-09, INV-14, INV-16, INV-34 via de peticion y las
           decisiones D-02, D-08, D-28 que cita); una delegacion nunca amplia el contrato (AUTOMATION_PLAN 16.5)
Rutas y archivos calientes (solo lectura para el Worker, fuera de su alcance de escritura):
           tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda de I-23: todo .cs de tests/RackCad.Tests declara
           exactamente namespace RackCad.Tests; los de src/ declaran el namespace de su carpeta, con bloque);
           src/RackCad.Application/Systems/Selective/ (SelectiveEffectiveDesignResolver, SelectiveGeometryResolver,
           SelectiveDepthLayout); src/RackCad.Application/Bom/BomAuthoredAuthority.cs;
           src/RackCad.Application/Persistence/ (RackEmbedDocument, KindDispatch, SelectivePalletDesignStore,
           RackProjectStore, FlowBedConfigurationStore); src/RackCad.Application/ProjectVariables/
           (ProjectVariableScanProjection, ProjectVariablesReadResult); src/RackCad.Domain/Systems/Selective/;
           src/RackCad.Plugin/KindHandlers/ (referencia de D-26; no se toca)
Criterios de aceptacion: la salida valida contra el esquema; alcance, invariantes, pruebas, autoridades y paradas
           tomados del contrato sin ampliarlos; celda y effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git show --stat 1013449d61b8292b77273ceeac30b82282b975a2 ; git log --oneline -4
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

DELTA DE LA TAREA (G1, correccion 1 de la cadena; AUTOMATION_PLAN 16.8 y README de ejecucion delegada §9)
- RunId = R20261002T011628Z-7097 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-63"; Initiative = "I-63"; Gate = "G1"; TaskId = "G1-RACK-METRICS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- Cadena (custodiada; copiala tal cual): ChainBaseSha = f71de12b33567e8b4109d3a17c60a17d54e96fdc;
  ChainRedSha = 1013449d61b8292b77273ceeac30b82282b975a2 (RED acreditado en la verificacion R20261002T005944Z-1192);
  ChainRedFiles = ["tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs",
  "tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs"];
  CorrectionOf = { RunId: "R20261002T005944Z-1192", FailureClass: "Ci",
  AnalysisSha256: "6080f3d008d032d4c6565f5f38df849f294c1a3c981d8ce9c31a14c5259a1fb8" }.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G1-RACK-METRICS/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T011628Z-7097" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato
  con capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el effort
  semantico de routing.md para la clase elegida; PromptProfile = el de esa clase.
- AllowedWriteScope: los mismos nueve archivos exactos de la delegacion anterior (siete de produccion bajo
  src/RackCad.Application/ComputedParameters/ y los dos de ChainRedFiles, que conservan su ruta); no amplies nada.
- ForbiddenWriteScope, Invariants, StopConditions, Authorities y RequiredTests: todos los del contrato (puedes anadir,
  nunca quitar); RequiredTests con MinSelected >= y el mismo ExpectRed. Anade a ForbiddenWriteScope
  tests/RackCad.Tests/NamespaceFolderGuardTests.cs.
- Reglas del analysis.md (obligatorias; reflejalas en Objective y AcceptanceCriteria):
  - las pruebas de G1 declaran namespace RackCad.Tests (nunca un subnamespace) y sus clases empiezan por
    ComputedParameters, para que el filtro del contrato FullyQualifiedName~RackCad.Tests.ComputedParameters las seleccione;
  - no se modifica la guarda de I-23 ni el contrato o su filtro;
  - RED nuevo SOLO de pruebas antes del GREEN: un commit que toca unicamente los dos archivos de ChainRedFiles (namespace,
    nombres de clase y, si se anade, la prueba de la precondicion de abajo), empujado con su propio push; su job Core
    falla solo por aserciones de las pruebas focales contra el esqueleto vigente (NamespaceFolderGuardTests ya en verde);
  - el GREEN toca solo los siete archivos de produccion, nunca RT ni ChainRedFiles; suite Core completa en verde;
  - aclaracion de D-17: si ninguna hermana tiene un RackId identificable, RackMetricRequest rechaza la peticion con
    ArgumentException (precondicion «hermanas de UN RackId»); nunca fabrica Unavailable(EnvelopeUnreadable);
  - los campos de texto libre de la entrega no contienen terminos de 16.10 (tampoco «candidato» ni «candidate»).
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada INV del contrato tal como los definen
  docs/initiatives/I-63-proposal-v3.md §20 y D-28, con A-1.3 (lecturas contadas en el costado del lector D-26), mas los
  de las reglas del analysis.md.
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G1, correccion 1, CONTROLLER_PLANNING)".
