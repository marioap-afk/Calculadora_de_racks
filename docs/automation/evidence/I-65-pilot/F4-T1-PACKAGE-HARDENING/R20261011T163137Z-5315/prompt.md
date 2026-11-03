I-65 — DELEGACION F4-T1-PACKAGE-HARDENING (correccion, intento 1) — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-T1-PACKAGE-HARDENING; intento 1; RunId R20261011T163137Z-5315;
           DelegationRunId R20261011T163137Z-5315; AuthorityRevision f64014f312f3b4f849e26dcee6dd713cbace146e;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261011T163137Z-5315
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase), leidas en AuthorityRevision para la unidad y en MainSha para lo demas
Objetivo: emitir el paquete de delegacion de CORRECCION (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el
           contrato de gate artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163137Z-5315/gate-contract.json (el mismo
           contrato del intento 0, sin cambios). Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato; una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite la correccion):
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs (la prueba
           LosDosLiteralesQueEstabanPartidosSeEscribenEnteros, cuya asercion es la defectuosa);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs y
           StructuralAnalysisRamElementsAssumptionsTests.cs (los archivos que esa prueba observa; solo lectura)
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato proyectadas sin cambios; celda y
           effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ; git log --oneline -5
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

DELTA DE LA TAREA (F4, correccion de la cadena F4-T1-PACKAGE-HARDENING tras el REWORK de la verificacion R20261011T162337Z-4794)
- Hechos de la cadena: delegacion del intento 0 R20261011T154535Z-39b9; trabajo R20261011T155212Z-11d5 con RED
  53bfcd94a99f4d6aa201596349d90427ca9f4229 y GREEN a6fa5a35d0297f87afea36417bf98a4f2d2971f1; verificacion R20261011T162337Z-4794 =
  EXECUTION_REWORK_REQUIRED/REWORK con FailureClass Ci: el GREEN es rojo por UNA asercion defectuosa del propio RED
  (StructuralAnalysisRamElementsHardeningGuardTests.LosDosLiteralesQueEstabanPartidosSeEscribenEnteros: su patron exige el token
  entre comillas y en StructuralAnalysisRamElementsGuardTests.cs el literal entero esta dentro de una cadena C# con la comilla
  escapada); el RED quedo acreditado. Despues, el commit de la sesion ab2a0fe249f065af0403ce78a25cb17e0826b806 registra la custodia, attempts = 1 y la decision de
  correccion con su analisis (decisiones I-65 §27; analisis en
  docs/automation/evidence/I-65-pilot/F4-T1-PACKAGE-HARDENING/R20261011T162337Z-4794/analysis.md, SHA-256
  2aedd7d047a1b78694e77ceead8ea34035161e8e7f726e58ab91e30e44d3248e). Ese commit solo cambia docs/automation (UNIT_DOC posteriores a
  AuthorityRevision, que no se leen como autoridad); el analisis se cita SOLO por CorrectionOf.
- RunId = R20261011T163137Z-5315 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-T1-PACKAGE-HARDENING" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD` (debe ser ab2a0fe249f065af0403ce78a25cb17e0826b806);
  ChainBaseSha = f64014f312f3b4f849e26dcee6dd713cbace146e (BaseSha de la primera delegacion de la cadena);
  ChainRedSha = 53bfcd94a99f4d6aa201596349d90427ca9f4229 (RED vigente, acreditado por R20261011T162337Z-4794);
  ChainRedFiles = exactamente estas nueve rutas, en este orden:
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisModelBuilderTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsAssumptionsTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningPackageTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningRaceTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningTemplateTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs
  CorrectionOf = { RunId: "R20261011T162337Z-4794", FailureClass: "Ci", AnalysisSha256:
  "2aedd7d047a1b78694e77ceead8ea34035161e8e7f726e58ab91e30e44d3248e" }.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, explicalo en RoutingReason como
  S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T163137Z-5315" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde); Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic y PromptProfile = los de
  routing.md para la clase elegida. Es una correccion acotada de una asercion de prueba.
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163137Z-5315/gate-contract.json y proyecta, sin reconstruir, resumir, corregir
  ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-T1-PACKAGE-HARDENING/1/R20261011T163137Z-5315/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes, y algunas Section SI llevan tildes o simbolos (p. ej. «## 16.
  Ejecución delegada bajo orden del Coordinator», «## G. Delegación de ejecución», «archivo completo (en especial §21 y §26)»,
  «archivo completo (en especial §0, §1, §2 y §7)», «archivo completo (en especial §5 y §6)» y la Section de la Proposal con §3). No
  las «corrijas» en ningun sentido. Ningun valor del contrato contiene barras invertidas: no las introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; si algo difiere, corrige tu salida antes de terminar. Registra en RoutingReason que el
  preflight dio igualdad en las seis.
- Objective y AcceptanceCriteria de la CORRECCION (no son colecciones de alcance): el Worker cambia SOLO, en
  tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs, el patron de la asercion de
  LosDosLiteralesQueEstabanPartidosSeEscribenEnteros para que admita una barra invertida opcional antes de la comilla de cierre (el
  literal entero dentro de una cadena C#) sin dejar de exigir el token entero, y anade en ese mismo archivo una autoprueba del patron
  con entradas sinteticas (token partido en dos literales concatenados -> no coincide; token entero entre comillas -> coincide;
  token entero con comillas escapadas -> coincide); ningun otro archivo; ningun cambio de produccion; suite Core completa en verde.
  Por la disposicion del Coordinator DEV-I65-F4-02 (decisiones §27) la correccion es UN SOLO commit de prueba, sin commit RED nuevo:
  RedSha = null en la entrega. Los campos de texto libre sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F4-T1 correccion 1, CONTROLLER_PLANNING)".
