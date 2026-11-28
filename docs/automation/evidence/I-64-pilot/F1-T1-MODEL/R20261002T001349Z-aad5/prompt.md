I-64 — DELEGACION F1-T1-MODEL — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 0; RunId R20261002T001349Z-aad5;
           DelegationRunId R20261002T001349Z-aad5; AuthorityRevision 8a3fb8e26a260317b28d5573654b0221f1341029;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261002T001349Z-aad5
Autoridades (leer desde Git, no copiar): las del contrato de gate (campo Authorities, con ruta, seccion y clase);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el
           contrato de gate artifacts/orchestration/I-64/F1-T1-MODEL/0/R20261002T001349Z-aad5/gate-contract.json.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-F1-T1-01..12); una delegacion nunca amplia el contrato
Rutas y archivos calientes: ninguno existente de produccion; todo lo nuevo vive en src/RackCad.Application/Workspace/,
           tests/RackCad.Tests/Workspace/ y docs/adr/0047-workspace-persistente-modeless-rackcad.md (archivo nuevo, hoy inexistente).
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) §0, D-02, D-03, D-04, D-05, D-06, §10, §11 (F1) y Anexo A.
           Formato de ADR del repositorio: docs/adr/README.md y docs/adr/plantilla.md (lectura).
Criterios de aceptacion: la salida valida contra el esquema; alcance, invariantes, pruebas, autoridades y paradas
           tomados del contrato sin ampliarlos; celda y effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-tree --name-only HEAD docs/adr/ (para confirmar que docs/adr/0047-* no existe)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y C-01..C-12 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason
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

DELTA DE LA TAREA (F1, primera delegacion de la cadena)
- RunId = R20261002T001349Z-aad5 (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T1-MODEL" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T1-MODEL/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T001349Z-aad5" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato
  (claude-sonnet-5-5|subagent); Effort.Provider = "medium" (unico TestedEffort de la celda en el contrato);
  Effort.Semantic = el effort semantico de routing.md (el Coordinator la declara Balanced).
- ModelEscalationReason = null (no hay escalado: Balanced admite el nivel Equilibrado de la celda).
- AllowedWriteScope: puedes estrecharlo a archivos exactos o prefijos dentro del alcance del contrato (sintaxis: archivo exacto
  o prefijo terminado en /); incluye siempre el archivo exacto docs/adr/0047-workspace-persistente-modeless-rackcad.md;
  nombra los archivos nuevos del modelo en src/RackCad.Application/Workspace/ y de las pruebas en tests/RackCad.Tests/Workspace/,
  con espacio de nombres RackCad.Tests.Workspace para que casen con el filtro del contrato.
- ForbiddenWriteScope, Invariants, StopConditions, Authorities y RequiredTests: todos los del contrato (puedes anadir,
  nunca quitar; puedes anadir docs/adr/README.md y docs/adr/plantilla.md como autoridades EXTERNAL de formato);
  RequiredTests con MinSelected >= y el mismo ExpectRed; puedes precisar filtros dentro de tests/RackCad.Tests/Workspace/.
- AcceptanceCriteria: concretos y comprobables, derivados SOLO de la Proposal V5 congelada y del contrato, limitados al modelo puro
  de F1 (sin host, sin puente real, sin seleccion real, sin navegacion, sin escritura authored):
  sesion por instancia abierta de documento y sin reemparejar tras destruirse (D-03, INV-07, INV-22);
  pistas encoladas como hints y drenaje solo en reposo y sin modal RackCad, coalescido, con las operaciones permitidas
  (D-04, INV-01, INV-23, INV-33); contextos de seleccion Ninguno, Uno, Seleccion (N), Sin identidad y Diagnostico, con
  «sin Id» = IsNullOrWhiteSpace y sin inventar RackId (D-05, D-06, INV-16, INV-17); sin estado por pestana que duplique
  suscripciones (INV-F1-T1-08); sin referencias a AutoCAD (INV-F1-T1-01);
  ADR 0047 en estado propuesto, proyeccion del Anexo A, publicado antes del commit GREEN.
  RED real primero: pruebas compilables contra un esqueleto, seleccion >= 1, fallo por asercion de comportamiento congelado;
  sin fallo de compilacion ni prueba artificial; despues GREEN para el mismo conjunto.
- IssuedBy = "Controller Codex (planificacion F1-T1-MODEL, CONTROLLER_PLANNING)".
