I-63 — DELEGACION G1-RACK-METRICS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 2 (correccion 2); RunId R20261002T143522Z-0c4d;
           DelegationRunId R20261002T143522Z-0c4d; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T143522Z-0c4d
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Contexto que motiva la correccion (NO es autoridad; se cita solo por CorrectionOf): el analysis.md del Coordinator
           docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T062554Z-e271/analysis.md, en HEAD
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la CORRECCION 2 de la tarea
           descrita en el contrato de gate artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T143522Z-0c4d/gate-contract.json,
           segun el analysis.md citado. Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-04, INV-05, INV-09, INV-14, INV-16, INV-34 via de peticion y las
           decisiones D-02, D-08, D-28 que cita); una delegacion nunca amplia el contrato (AUTOMATION_PLAN 16.5)
Rutas y archivos calientes (solo lectura para el Worker): src/RackCad.Application/ComputedParameters/RackMetricIds.cs
           (catalogo productivo de MetricId); docs/initiatives/I-63-proposal-v3.md §5 (D-02: tabla de los seis MetricId y
           regla del token) y §6 (D-05); tests/RackCad.Tests/ComputedParameters/ (pruebas existentes, protegidas);
           tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda de I-23)
Criterios de aceptacion: la salida valida contra el esquema; alcance, invariantes, pruebas, autoridades y paradas
           tomados del contrato sin ampliarlos; celda y effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git show --stat fc30dc6cf0d2f97e6732bbb8c512623db91c305d ; git show --stat b5ee157d7c42a3bada7d6804cbbe3cd731405e00 ;
           git log --oneline -4
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

DELTA DE LA TAREA (G1, correccion 2 de la cadena; AUTOMATION_PLAN 16.8 y README de ejecucion delegada §9)
- RunId = R20261002T143522Z-0c4d (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-63"; Initiative = "I-63"; Gate = "G1"; TaskId = "G1-RACK-METRICS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- Cadena (custodiada; copiala tal cual): ChainBaseSha = f71de12b33567e8b4109d3a17c60a17d54e96fdc;
  ChainRedSha = fc30dc6cf0d2f97e6732bbb8c512623db91c305d (RED de la correccion 1, acreditado en R20261002T062554Z-e271);
  ChainRedFiles = ["tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs",
  "tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs"];
  CorrectionOf = { RunId: "R20261002T062554Z-e271", FailureClass: "Authority", AnalysisSha256: "a860e8e959f7ab4e491c7856e661370cb9505af63246d785f47691f248260696" }.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G1-RACK-METRICS/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T143522Z-0c4d" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato
  con capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el effort
  semantico de routing.md para la clase elegida; PromptProfile = el de esa clase.
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO (orden del Coordinator). Lee el archivo
  artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T143522Z-0c4d/gate-contract.json y proyecta, sin reconstruir, resumir, corregir
  ortografia ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T143522Z-0c4d/gate-contract.json | ConvertFrom-Json; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  Deben permanecer literalmente (sin tilde, como en el contrato y en el encabezado real de AGENTS.md:57):
    ## Convenciones arquitectonicas (obligatorias)
    D1 (nucleo neutral) y D24
  Ningun analysis.md entra en Authorities: los analisis van solo por CorrectionOf.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de cadenas en Path, Section y Class. Si algo
  difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes invalida. Registra en
  RoutingReason que el preflight dio igualdad en las seis.
- La restriccion de esta correccion (crear SOLO el archivo nuevo tests/RackCad.Tests/ComputedParameters/RackMetricIdsTests.cs,
  sin tocar ningun otro: ni los siete de produccion de src/RackCad.Application/ComputedParameters/, ni los dos de
  ChainRedFiles, ni tests/RackCad.Tests/NamespaceFolderGuardTests.cs) va en Objective y AcceptanceCriteria, NO en las
  colecciones de alcance, que son las del contrato.
- Reglas del analysis.md (obligatorias; reflejalas en Objective y AcceptanceCriteria):
  - prueba explicita de D-02 en el archivo nuevo, namespace RackCad.Tests (una sola declaracion) y clase cuyo nombre
    empiece por ComputedParameters (p. ej. ComputedParametersRackMetricIdsTests), seleccionada por el filtro del contrato;
  - para cada uno de los seis MetricId congelados de D-02/D-05 [(Rack, frentes), (Rack, frentesVacios),
    (Project, totalRacks), (Project, rackCount), (Project, totalFrentes), (Project, totalFrentesVacios)]:
    (scope, token) -> el MetricId del catalogo productivo (RackMetricIds) y ese MetricId -> (scope, token); el catalogo
    productivo contiene exactamente esos seis; regla del token de D-02 (ASCII, empieza por minuscula, solo letras y
    digitos, comparador Ordinal);
  - el oraculo son los seis valores congelados del Freeze contrastados con el catalogo productivo leido en la prueba; no
    una copia tautologica del codigo productivo;
  - sin RED nuevo (16.8: ChainRedSha no es null y la entrega no toca ChainRedFiles); la prueba puede nacer verde;
  - un solo commit del Worker, que solo anade el archivo nuevo; suite Core completa en verde, con NamespaceFolderGuardTests;
  - si la prueba exigiera tocar produccion o las pruebas protegidas: parada, sin tocarlas;
  - los campos de texto libre de la entrega no contienen terminos de 16.10 (tampoco «candidato» ni «candidate»).
- AcceptanceCriteria: concretos y comprobables, con los de las reglas anteriores y la conservacion de los INV del contrato
  (las pruebas existentes de INV-04, 05, 09, 14, 16 y 34 siguen en verde sin cambios).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G1, correccion 2, CONTROLLER_PLANNING)".
