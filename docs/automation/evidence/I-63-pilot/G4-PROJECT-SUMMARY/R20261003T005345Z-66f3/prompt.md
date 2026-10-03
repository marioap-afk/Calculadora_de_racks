I-63 — DELEGACION G4-PROJECT-SUMMARY — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G4; tarea G4-PROJECT-SUMMARY; intento 2; RunId R20261003T005345Z-66f3;
           DelegationRunId R20261003T005345Z-66f3; AuthorityRevision 527e4b9185173e2f50c10d39a882749c09b77d2f;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261003T005345Z-66f3
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3 congelada + A-1 + A-2 + A-3 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para
           lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261003T005345Z-66f3/gate-contract.json. Esta delegacion SI se ejecutara:
           el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (partes de G4 de INV-11 e INV-32 segun la A-2, INV-29, 30, 31 y 34 por el resumen,
           con D-20, D-21, D-24 y D-25); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/ComputedParameters/ (G1: RackMetricRequest, RackMetricResolutionSide y su contador, lector
           D-26, MetricValue y RackMetricResults en RackMetricModels.cs, RackMetricIds; G2: ProjectPopulation,
           ProjectPopulationAggregator, RackOutputVerdict, RackPopulationEvaluationCounter; G3: RackComputedExpressionContext);
           src/RackCad.Application/RackCad.Application.csproj, src/RackCad.UI/RackCad.UI.csproj y
           src/RackCad.Plugin/RackCad.Plugin.csproj (solo lectura: objetivo y controles de INV-29 b);
           las pruebas existentes de tests/RackCad.Tests/ComputedParameters/ (G1-G3; solo lectura)
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

DELTA DE LA TAREA (G4, primera delegacion de la cadena G4-PROJECT-SUMMARY)
- Autorizacion: orden del Coordinator registrada en docs/automation/decisions/I-63.md §2 («Autorizacion de G4») en
  AuthorityRevision; una sola cadena RED -> GREEN; Architect no exigido por defecto.
- RunId = R20261003T005345Z-66f3 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por gate).
- Unit = "I-63"; Initiative = "I-63"; Gate = "G4"; TaskId = "G4-PROJECT-SUMMARY" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261003T005345Z-66f3" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V3 §21 da para G4 ROUTINE_IMPLEMENTATION +
  CHARACTERIZATION; la orden del Coordinator lo confirma.
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261003T005345Z-66f3/gate-contract.json y proyecta, sin reconstruir, resumir, corregir
  ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261003T005345Z-66f3/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes EXACTAMENTE donde las tiene el archivo: los Invariants,
  los textos de StopConditions y varias Section estan SIN tildes (p. ej. «propagacion», «nucleo», «arquitectonicas»,
  «semantica», «poblacion»), y otras Section SI las llevan (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator»,
  «§2 y §6»). No las «corrijas» en ningun sentido. Deben permanecer literalmente, entre otras, las etiquetas
  «## Convenciones arquitectonicas (obligatorias)» y «D1 (nucleo neutral) y D24».
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): nombra los archivos de produccion nuevos
  bajo src/RackCad.Application/ComputedParameters/ (ProjectSummary, RackSummary y provenance) y el cambio ADITIVO autorizado en
  RackComputedExpressionContext.cs; las clases de prueba nuevas, en archivos NUEVOS de tests/RackCad.Tests/ComputedParameters/,
  en namespace RackCad.Tests (guarda de I-23) y con nombre que empieza por ComputedParametersSummary para que las seleccione
  el filtro del contrato. Las pruebas existentes de G1, G2 y G3 no se modifican, borran ni renombran.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada INV del contrato tal como los definen
  docs/initiatives/I-63-proposal-v3.md §20 y las decisiones D-20, D-21, D-24, D-25 y D-28, con las partes de G4 de INV-11 e
  INV-32 de la A-2 y la A-1.1 para INV-29 (b). Incluye:
  - RED compilable primero (pruebas que fallan por asercion o excepcion contra un esqueleto que compila, seleccion >= 10),
    con push propio antes del GREEN para que su corrida push exista y su job Core falle; INV-29 (b) con su control positivo
    sobre la misma funcion;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, con las pruebas de G1, G2 y G3;
  - provenance fuera de MetricValue.Equals (si no es posible: parada C-07); el cambio de RackComputedExpressionContext es
    aditivo y no cambia Evaluate;
  - caracterizacion de D-24 con N = 1, 10, 100 y 1000 y tres vistas: contadores como oraculo, tiempos registrados en la
    entrega sin umbral (si N = 1000 no cabe en la CI: parada C-09, sin rebajar N ni omitir);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G4, CONTROLLER_PLANNING)".
