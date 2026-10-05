I-63 — DELEGACION G3-RACK-BUILTINS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G3; tarea G3-RACK-BUILTINS (tramo T2, GREEN-BUILTINS); intento 2; RunId R20261002T214309Z-19d7;
           DelegationRunId R20261002T214309Z-19d7; AuthorityRevision ea70e3b333efec49d3c3b9bc1e5d0395adc71a01;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T214309Z-19d7
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3 congelada + A-1 + A-2 + A-3 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo
           demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T214309Z-19d7/gate-contract.json. Esta delegacion SI se ejecutara:
           el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-15, 17-27 y 28, con D-14..D-19, INV-22 con la A-3 y las reglas de G3-T2); una
           delegacion nunca amplia el contrato
Rutas y archivos calientes (el Worker escribe produccion solo dentro del alcance del contrato):
           src/RackCad.Application/Expressions/ (SymbolId.cs con SymbolNamespace y SymbolNamespaces; SymbolTable.cs con
           SymbolScope, SymbolDefinitionKind e indices; ExpressionBinder.cs; ExpressionFormatter.cs con CanonicalShape;
           RegistryEvaluation.cs; DependencyGraph.cs); src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs;
           src/RackCad.Application/ComputedParameters/ (RackMetricRequest y RackMetricResults de G1; RackComputedExpressionContext
           nuevo); las pruebas del RED de la cadena (solo lectura: fijan la API); las suites de INV-28, ExpressionCoreGuardTests.cs,
           ExpressionDiagnosticCatalogTests.cs y NamespaceFolderGuardTests.cs (solo lectura)
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato proyectadas sin cambios; celda y
           effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ; git log --oneline -4
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

DELTA DE LA TAREA (G3-T2, continuacion autorizada de la cadena G3-RACK-BUILTINS; AUTOMATION_PLAN 16.8)
- Autorizacion: orden del Coordinator registrada en docs/automation/decisions/I-63.md §2 («Autorizacion de G3-T2 y condicion
  corregida de la A-3») en el commit f5c4a5a2 (solo docs, posterior a AuthorityRevision). Esa misma orden fija
  AuthorityRevision = ea70e3b3 (la A-3) y los valores de la cadena de abajo. G3-T2 NO es una correccion: CorrectionOf = null.
- Esta planificacion REPITE la de R20261002T212850Z-6064, detenida por STOP P-01 (cambio de config.toml por la actualizacion de la
  app Codex del Owner); el Owner aprobo el cambio y ordeno repetirla con el mismo contrato y un RunId nuevo, sin reutilizar
  aquella delegacion (decisiones §2, commit 71205235 = BaseSha). No leas ni copies nada de aquel RunId.
- RunId = R20261002T214309Z-19d7 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia; el RED de G3-T1 y esta continuacion no consumen attempts: orden del Coordinator).
- Unit = "I-63"; Initiative = "I-63"; Gate = "G3"; TaskId = "G3-RACK-BUILTINS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- Cadena (custodiada; copiala tal cual): ChainBaseSha = e99621f9e285b1a7ddfcf51cd8ccf970828e0b66;
  ChainRedSha = 637dce7e5e7811992330b3c305358d9a7542b9f6 (RED de G3-T1, acreditado en la verificacion R20261002T194212Z-73b1);
  ChainRedFiles = ["tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsBindingTests.cs",
  "tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsConsumersTests.cs",
  "tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsContextTests.cs",
  "tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsFormattingTests.cs",
  "tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsIdentityTests.cs",
  "tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsKit.cs",
  "tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbolsPersistenceTests.cs",
  "tests/RackCad.Tests/ExpressionSymbolModelTests.cs"]; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T214309Z-19d7" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V3 §21 da para G3 LONG_HORIZON_IMPLEMENTATION o ROUTINE
  (Deep); G3-T2 es la implementacion de produccion del nucleo, la persistencia y el contexto de rack contra 89 pruebas RED ya
  escritas que fijan la API.
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T214309Z-19d7/gate-contract.json y proyecta, sin reconstruir, resumir, corregir
  ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T214309Z-19d7/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes EXACTAMENTE donde las tiene el archivo: los Invariants,
  los textos de StopConditions y varias Section estan SIN tildes (p. ej. «propagacion», «nucleo», «arquitectonicas»,
  «semantica»), y otras Section SI las llevan (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «§2 y §6»).
  No las «corrijas» en ningun sentido. Deben permanecer literalmente, entre otras, las etiquetas
  «## Convenciones arquitectonicas (obligatorias)» y «D1 (nucleo neutral), D9 (tokens persistidos) y D24».
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Restricciones de T2 (van en Objective y AcceptanceCriteria, no en las colecciones de alcance, que son las del contrato):
  - un commit GREEN de produccion SOLO en src/RackCad.Application/Expressions/, src/RackCad.Application/ComputedParameters/ y
    src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs; sin RED nuevo;
  - ningun archivo de ChainRedFiles se modifica (16.8: el GREEN no toca RT ∪ ChainRedFiles); si el GREEN lo exigiera, parada
    C-11; tampoco se tocan las suites de INV-28, ExpressionCoreGuardTests, ExpressionDiagnosticCatalogTests ni
    NamespaceFolderGuardTests;
  - pruebas nuevas adicionales solo si hacen falta, en archivos NUEVOS de tests/RackCad.Tests/ComputedParameters/ (namespace
    RackCad.Tests, clases que empiezan por ComputedParametersSymbols);
  - la API la fijan las pruebas protegidas del RED; resumen no normativo en el Evidence de la entrega de T1
    (docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/R20261002T184507Z-9800/worker-handoff.json, punto (2)).
- AcceptanceCriteria: concretos y comprobables para G3-T2, uno o mas por cada INV del contrato tal como los define
  docs/initiatives/I-63-proposal-v3.md §20 (INV-20 con A-1.2; INV-22 con la A-3). Incluye:
  - las 89 pruebas del filtro del contrato en verde sin modificar ninguna; suite Core completa en verde (incluidas las suites de
    INV-28, ExpressionCoreGuardTests, ExpressionDiagnosticCatalogTests, NamespaceFolderGuardTests y las pruebas de G1 y G2);
  - INV-22: Rack.#{zzz} -> exactamente un InvalidQualifier (11, SyntaxAndLimits) en 5+6, sin arbol sintactico ni enlazado,
    determinista; sin codigo nuevo; lexer y parser sin cambios; el resultado vigente para una clave rack de 32 caracteres
    hexadecimales (un UnexpectedToken) se conserva (si cambiara, parada C-12);
  - D-16 neutral (ningun tipo del nucleo nombra racks, sistemas ni metricas mas alla del miembro de namespace y su token);
    projectVariable intacto (C-09); RackComputedExpressionContext fuera de RackCad.Application.Expressions (D-17);
    PersistedBoundExpressionJson con la tabla de tokens persistidos de D9 (D-18);
  - un commit GREEN con push propio sin --force; la corrida push del SHA exacto con los cuatro jobs requeridos en success;
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G3-T2, CONTROLLER_PLANNING)".
