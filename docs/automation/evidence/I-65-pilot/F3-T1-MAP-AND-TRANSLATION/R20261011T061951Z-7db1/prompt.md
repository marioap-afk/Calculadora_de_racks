I-65 — DELEGACION F3-T1-MAP-AND-TRANSLATION — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F3; tarea F3-T1-MAP-AND-TRANSLATION; intento 0; RunId R20261011T061951Z-7db1;
           DelegationRunId R20261011T061951Z-7db1; AuthorityRevision bf2d347836904275955e1a8059051271a76c51e2;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261011T061951Z-7db1
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, docs/initiatives/I-65-f3-ram-elements-adapter.md, decisiones de I-65 (§14, §15, §17),
           resultados e informe de P1, ADR-0050 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T061951Z-7db1/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (D-06/D-07, A65-F1-01/02, INV-07, INV-08, D-04 con el golden de RAM, D-08,
           A65-F1-07, ediciones de guarda A65-F1-04/05 y guardas nuevas del proveedor); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/StructuralAnalysis/ (F1 cerrada: StructuralModel, StructuralMaterialProfile.cs con
           StructuralMaterialRow e IStructuralMaterialProfile, StructuralFingerprints, StructuralModelBuilder, ForceUnits en Units/);
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs (solo los enunciados de supuestos);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs (solo las ediciones (a)-(c) de CD-I65-F3-05);
           docs/automation/evidence/I-65-f0/p1-run/p1-report.json (oraculo de INV-07, solo lectura);
           las demas pruebas de tests/RackCad.Tests/StructuralAnalysis/ y sus fixtures (solo lectura)
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

DELTA DE LA TAREA (F3, primera delegacion de la cadena F3-T1-MAP-AND-TRANSLATION)
- Autorizacion: orden del Owner «I-65 — Owner Authorization: F3 RAM Elements Adapter» (decisiones I-65 §17:
  IMPLEMENTATION AUTHORIZATION = F3 ONLY) y decisiones del Coordinator CD-I65-F3-01..07 (§17: F3 en dos tareas; esta es la
  primera), con la concrecion de docs/initiatives/I-65-f3-ram-elements-adapter.md, en AuthorityRevision; una sola cadena
  RED -> GREEN; Architect no exigido en la tarea (revisa al cierre de F3).
- RunId = R20261011T061951Z-7db1 (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3
  (attempts es por unidad: 0 en el estado de HEAD; max_attempts 3 en el contrato de la iniciativa).
- Unit = "I-65"; Initiative = "I-65"; Gate = "F3"; TaskId = "F3-T1-MAP-AND-TRANSLATION" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T061951Z-7db1" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V1 §11 (vigente por V3.1 §11) da para «Tablas de mapeo, pruebas de unidades»
  Balanced y para «Adaptador y plantilla Python» Deep con Worker claude-sonnet-5-5/high; la tarea cabe en una invocacion del
  Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T061951Z-7db1/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T061951Z-7db1/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «unica», «proyeccion», «validacion», «traduccion»,
  «seccion», «inclinacion», «orientacion», «diagnostico», «asercion», «edicion», «ingenieria», «codigo», «diseno»), y algunas
  Section SI llevan tildes o simbolos (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «## G. Delegación de
  ejecución», «archivo completo (en especial §14, §15 y §17)» y la Section de la Proposal con §3). No las «corrijas» en
  ningun sentido. Los numeros del golden (p. ej. 57.50128206169155, 36.25192309253731, 78.75064103084577) y las expresiones
  como l3_0 = unit(l1 x Y) se copian caracter a caracter.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): los archivos de produccion nuevos
  exactos bajo src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (tabla y validador, proyeccion, traduccion,
  diagnosticos del proveedor); el cambio aditivo de StructuralMaterialProfile.cs; el cambio de los enunciados de supuestos de
  CantileverStructuralModelSource.cs; el archivo de datos assets/structural-analysis/ram-elements/section-material-map.json;
  las ediciones (a)-(c) de StructuralAnalysisSourceGuardTests.cs; las clases de prueba nuevas, en archivos NUEVOS de
  tests/RackCad.Tests/StructuralAnalysis/, en namespace RackCad.Tests y con nombre que empieza por StructuralAnalysisRamElements
  (no StructuralAnalysisRamElementsPackage) para que las seleccione el filtro del contrato. Ninguna otra prueba existente cambia.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-proposal-v3.1.md (D-03, D-04, D-06, D-07, D-08, D-10, D-12; §3 INV-07 e INV-08),
  docs/initiatives/I-65-f3-ram-elements-adapter.md (secciones 2 a 6 y 8) y las decisiones CD-I65-F3-03..07. Incluye:
  - RED compilable primero (esqueleto que compila, archivo de datos, ediciones de guarda y pruebas que fallan por asercion o
    excepcion, seleccion >= 30), con push propio antes del GREEN para que su corrida push exista y su job Core falle;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, incluidas todas las pruebas de F1;
  - el golden de RAM y la tabla de P1 se usan como oraculo literal; si no coinciden con las reglas: parada C-03 sin ajustar nada;
  - ninguna fila del archivo de datos queda verified y ningun valor de ingenieria se rellena (si hiciera falta: parada C-07);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F3-T1, CONTROLLER_PLANNING)".
