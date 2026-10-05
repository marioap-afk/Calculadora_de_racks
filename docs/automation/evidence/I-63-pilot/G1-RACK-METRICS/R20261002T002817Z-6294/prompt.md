I-63 — DELEGACION G1-RACK-METRICS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 0; RunId R20261002T002817Z-6294;
           DelegationRunId R20261002T002817Z-6294; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T002817Z-6294
Autoridades (leer desde Git, no copiar): las del contrato de gate (campo Authorities, con ruta, seccion y clase);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el
           contrato de gate artifacts/orchestration/I-63/G1-RACK-METRICS/0/R20261002T002817Z-6294/gate-contract.json.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-04, INV-05, INV-09, INV-14, INV-16, INV-34 via de peticion y las
           decisiones D-02, D-08, D-28 que cita); una delegacion nunca amplia el contrato (AUTOMATION_PLAN 16.5)
Rutas y archivos calientes (solo lectura para el Worker, fuera de su alcance de escritura):
           src/RackCad.Application/Systems/Selective/ (SelectiveEffectiveDesignResolver, SelectiveGeometryResolver,
           SelectiveDepthLayout); src/RackCad.Application/Bom/BomAuthoredAuthority.cs;
           src/RackCad.Application/Persistence/ (RackEmbedDocument, KindDispatch, SelectivePalletDesignStore,
           RackProjectStore, FlowBedConfigurationStore); src/RackCad.Application/ProjectVariables/
           (ProjectVariableScanProjection, ProjectVariablesReadResult); src/RackCad.Domain/Systems/Selective/
           (SelectivePalletDesign, SelectiveRackSystem); src/RackCad.Plugin/KindHandlers/ (referencia de D-26; no se toca)
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

DELTA DE LA TAREA (G1, primera delegacion de la cadena)
- RunId = R20261002T002817Z-6294 (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3.
- Unit = "I-63"; Initiative = "I-63"; Gate = "G1"; TaskId = "G1-RACK-METRICS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G1-RACK-METRICS/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T002817Z-6294" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato
  con capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el effort
  semantico de routing.md para la clase elegida; PromptProfile = el de esa clase.
- AllowedWriteScope: puedes estrecharlo a archivos exactos dentro de los dos prefijos del contrato (sintaxis: archivo
  exacto o prefijo terminado en /); nombra los archivos nuevos exactos de produccion bajo
  src/RackCad.Application/ComputedParameters/ y de pruebas bajo tests/RackCad.Tests/ComputedParameters/; las clases de
  prueba en el espacio de nombres RackCad.Tests.ComputedParameters para que casen con el filtro del contrato.
- ForbiddenWriteScope, Invariants, StopConditions, Authorities y RequiredTests: todos los del contrato (puedes anadir,
  nunca quitar); RequiredTests con MinSelected >= y el mismo ExpectRed.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada INV del contrato tal como los definen
  docs/initiatives/I-63-proposal-v3.md §20 (filas INV-04, INV-05, INV-09, INV-14, INV-16, INV-34) y D-28, con las
  precisiones de docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md (A-1.3: lecturas contadas en el costado
  del lector D-26). Incluye: RED compilable primero (esqueleto que compila y pruebas que fallan por asercion, seleccion
  > 0); el commit RED se publica con un push PROPIO antes del GREEN, para que su corrida push exista y su job Core falle;
  el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde en el GREEN.
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G1, CONTROLLER_PLANNING)".
