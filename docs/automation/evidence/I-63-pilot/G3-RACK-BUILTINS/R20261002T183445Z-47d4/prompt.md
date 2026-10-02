I-63 — DELEGACION G3-RACK-BUILTINS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G3; tarea G3-RACK-BUILTINS; intento 2; RunId R20261002T183445Z-47d4;
           DelegationRunId R20261002T183445Z-47d4; AuthorityRevision e99621f9e285b1a7ddfcf51cd8ccf970828e0b66;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T183445Z-47d4
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3 congelada + A-1 + A-2 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T183445Z-47d4/gate-contract.json. Esta delegacion SI se ejecutara:
           el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-15, 17-27 y 28, con D-14..D-19, y las reglas de G3-T1); una delegacion nunca amplia
           el contrato
Rutas y archivos calientes (solo lectura para el Worker; G3-T1 escribe solo pruebas):
           src/RackCad.Application/Expressions/ (SymbolId.cs con SymbolNamespace y SymbolNamespaces; SymbolTable.cs con
           SymbolScope, SymbolDefinitionKind e indices; ExpressionBinder.cs; ExpressionFormatter.cs con CanonicalShape;
           ExpressionSyntaxParser.cs; RegistryEvaluation.cs; DependencyGraph.cs; BoundExpressionDependencies.cs;
           ExpressionEvaluator.cs); src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs;
           src/RackCad.Application/ComputedParameters/ (RackMetricRequest y RackMetricResults de G1);
           tests/RackCad.Tests/ExpressionSymbolModelTests.cs (aserciones pre-ID20), las suites de INV-28,
           ExpressionCoreGuardTests.cs, ExpressionDiagnosticCatalogTests.cs y NamespaceFolderGuardTests.cs (solo lectura)
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

DELTA DE LA TAREA (G3-T1, primera delegacion de la cadena G3-RACK-BUILTINS)
- RunId = R20261002T183445Z-47d4 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por gate; el RED inicial de G3 no consume attempts: orden del Coordinator).
- Unit = "I-63"; Initiative = "I-63"; Gate = "G3"; TaskId = "G3-RACK-BUILTINS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T183445Z-47d4" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V3 §21 da para G3 LONG_HORIZON_IMPLEMENTATION o ROUTINE
  (Deep); G3-T1 es la escritura de pruebas RED que fijan la API y el comportamiento esperados.
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T183445Z-47d4/gate-contract.json y proyecta, sin reconstruir, resumir, corregir
  ortografia, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T183445Z-47d4/gate-contract.json | ConvertFrom-Json; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  Deben permanecer literalmente, entre otras, las etiquetas sin tilde «## Convenciones arquitectonicas (obligatorias)» y
  «D1 (nucleo neutral) y D24» (asi estan en el contrato y en el encabezado real de AGENTS.md:57).
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class. Si algo difiere,
  corrige tu salida antes de terminar; nunca entregues una delegacion que sabes invalida. Registra en RoutingReason que el
  preflight dio igualdad en las seis.
- Archivos exactos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): los archivos de prueba nuevos bajo
  tests/RackCad.Tests/ComputedParameters/, en namespace RackCad.Tests (guarda de I-23) y con clases que empiezan por
  ComputedParametersSymbols para que las seleccione el filtro del contrato, y tests/RackCad.Tests/ExpressionSymbolModelTests.cs
  (solo sus aserciones pre-ID20). Ningun archivo de src/.
- AcceptanceCriteria: concretos y comprobables para G3-T1, uno o mas por cada INV del contrato tal como los define
  docs/initiatives/I-63-proposal-v3.md §20 (INV-20 con A-1.2). Incluye:
  - un solo commit RED, solo de pruebas y con push propio; compila contra la API actual sin tocar src/ y falla en ejecucion
    (asercion o excepcion) con seleccion >= 12; su corrida push tiene el job Core en failure por esas pruebas;
  - ExpressionSymbolModelTests.cs cambia solo en las aserciones pre-ID20 que D-16 cambia;
  - INV-22: el RED observa y fija el resultado diagnostico exacto de Rack.#{zzz} (codigos existentes del catalogo V6, spans y
    ausencia de arbol), sin elegirlo por inspeccion estatica; un codigo nuevo necesario es parada C-10;
  - la entrega registra la API nueva que las pruebas fijan para G3-T2 (tipos y miembros esperados);
  - las suites de INV-28, ExpressionCoreGuardTests, ExpressionDiagnosticCatalogTests y las pruebas de G1 y G2 no se modifican;
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G3-T1, CONTROLLER_PLANNING)".
