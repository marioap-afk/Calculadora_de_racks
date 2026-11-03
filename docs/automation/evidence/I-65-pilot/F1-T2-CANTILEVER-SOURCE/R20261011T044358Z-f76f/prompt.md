I-65 — DELEGACION F1-T2-CANTILEVER-SOURCE — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F1; tarea F1-T2-CANTILEVER-SOURCE; intento 0; RunId R20261011T044358Z-f76f;
           DelegationRunId R20261011T044358Z-f76f; AuthorityRevision ff0c57c7125ae268bcb42b2d56c0d5255e9a69ed;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261011T044358Z-f76f
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, docs/initiatives/I-65-f1-ref-cant-01.md (golden), decisiones de I-65 (§10-§13),
           ADR-0050 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044358Z-f76f/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-04, 05, 06, 07 en su parte neutral y 09, con IDL-01..10, D-04, D-10, D-10b,
           D-12, D-13 y las concreciones CD-I65-F1-13..16); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/StructuralAnalysis/ (F1-T1 verificada en 1692804a: IStructuralModelSource, StructuralScope,
           StructuralSourceDescription, StructuralModelBuilder, StructuralLoadFileReader; ForceUnits en Units/);
           src/RackCad.Application/Systems/Cantilever/ (CantileverLineResolver, CantileverLineAssembly, CantileverStationAssembly,
           CantileverStructuralMemberPlan, CantileverColumnBaseDatum, CantileverArmFrameResolver, CantileverCataloguePolicies;
           solo lectura salvo el archivo nuevo CantileverStructuralModelSource.cs);
           src/RackCad.Application/StructuralSections/Geometry/PrismaticSectionInstance.cs (Frame, RotationRadians, Mirrored,
           SectionTransform; solo lectura);
           tests/RackCad.Tests/CantileverSourceGuardTests.cs (tokens prohibidos en Systems/Cantilever, entre otros Document,
           RackProject, Interval, StructuralSectionUnits, .Dimensions y LocalFrame3D.Create; solo lectura) y las nueve pruebas de
           F1-T1 en tests/RackCad.Tests/StructuralAnalysis/ (solo lectura)
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

DELTA DE LA TAREA (F1, primera delegacion de la cadena F1-T2-CANTILEVER-SOURCE)
- Autorizacion: orden del Owner «Conditional GO for Freeze and F1» (decisiones I-65 §9: F1 IMPLEMENTATION AUTHORIZATION = YES
  solo para el alcance de F1 de la Proposal V3.1) y decisiones del Coordinator §10 (F1 en dos tareas; esta es la segunda),
  §12 (F1-T1 aceptada en 1692804a) y §13 (concreciones CD-I65-F1-13..16), con el golden de
  docs/initiatives/I-65-f1-ref-cant-01.md, en AuthorityRevision; una sola cadena RED -> GREEN; Architect no exigido por defecto.
- RunId = R20261011T044358Z-f76f (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3
  (attempts es por unidad: 0 en el estado de HEAD; max_attempts 3 en el contrato de la iniciativa).
- Unit = "I-65"; Initiative = "I-65"; Gate = "F1"; TaskId = "F1-T2-CANTILEVER-SOURCE" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T044358Z-f76f" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V1 §11 (vigente por V3.1 §11) da para F1 «Implementacion
  transversal» con effort Deep y Worker claude-sonnet-5-5/high; la tarea cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044358Z-f76f/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044358Z-f76f/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «geometria», «insercion», «interseccion», «fisico»,
  «seccion», «dextrogiro», «rotacion», «inclinacion», «diseno», «excepcion», «codigo», «validacion»), y algunas Section SI
  llevan tildes o simbolos (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «## G. Delegación de ejecución»,
  «archivo completo (en especial §9, §10, §11, §12 y §13)» y la Section de la Proposal con §3 y §15). No las «corrijas» en
  ningun sentido. Los numeros del golden (p. ej. 20.26716169689737, 48.267161696897375, 36.22609804415268) se copian digito
  a digito.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): el archivo de produccion nuevo exacto
  src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs; los cambios aditivos que hicieran falta en
  src/RackCad.Application/StructuralAnalysis/ (si ninguno, dilo); los fixtures nuevos de diseno y de cargas de REF-CANT-01 bajo
  tests/RackCad.Tests/StructuralAnalysis/Fixtures/; las clases de prueba nuevas, en archivos NUEVOS de
  tests/RackCad.Tests/StructuralAnalysis/, en namespace RackCad.Tests (guarda de I-23) y con nombre que empieza por
  StructuralAnalysisCantilever para que las seleccione el filtro del contrato. Las pruebas existentes (incluidas las de F1-T1)
  no se modifican, borran ni renombran.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-proposal-v3.1.md (D-04, D-05, D-10, D-10b, D-12, §3 y §15), docs/initiatives/I-65-f1-ref-cant-01.md y
  las concreciones CD-I65-F1-13..16 de docs/automation/decisions/I-65.md §13. Incluye:
  - RED compilable primero (esqueleto de produccion que compila y pruebas que fallan por asercion o excepcion, seleccion >= 20),
    con push propio antes del GREEN para que su corrida push exista y su job Core falle;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, incluidas las pruebas de F1-T1 y las
    guardas de Cantilever;
  - el golden se copia como literales de la tabla del documento y no se recalcula; si no coincide con lo que dan las reglas
    IDL sobre las colocaciones del resolver: parada C-03 con la diferencia exacta, sin ajustar nada;
  - si el archivo nuevo no puede cumplir las guardas de Cantilever sin debilitarlas: parada C-09;
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F1-T2, CONTROLLER_PLANNING)".
