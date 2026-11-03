I-65 — DELEGACION F1-T1-NEUTRAL-MODEL — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F1; tarea F1-T1-NEUTRAL-MODEL; intento 0; RunId R20261011T032206Z-7516;
           DelegationRunId R20261011T032206Z-7516; AuthorityRevision 9d0c661968542350929af267ada689b943898206;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261011T032206Z-7516
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, decisiones de I-65 (§10 y §11), ADR-0050 y las demas; vigencia:
           AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032206Z-7516/gate-contract.json. Esta delegacion
           SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-01, 02, 03, 04, 05 en sus partes neutrales, 11, 14, 15 y 16, con D-02, D-03,
           D-06, D-07, D-08, D-10b, D-12 y D-13 y las concreciones CD-I65-F1-04..10); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/Units/LengthUnits.cs (autoridad de longitud; no cambia);
           src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs (unico archivo de StructuralSections que cambia);
           src/RackCad.Application/StructuralSections/ (catalogo neutral: StructuralSectionCatalog, StructuralSectionDefinition,
           StructuralSectionProperties, StructuralSectionFamily, StructuralSectionId; solo lectura);
           tests/RackCad.Tests/LengthUnitsAuthorityGuardTests.cs, NamespaceFolderGuardTests.cs, CantileverSourceGuardTests.cs y
           StructuralSectionModelTests.cs (solo lectura; modelos de guarda y de prueba)
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

DELTA DE LA TAREA (F1, primera delegacion de la cadena F1-T1-NEUTRAL-MODEL)
- Autorizacion: orden del Owner «Conditional GO for Freeze and F1» (decisiones I-65 §9: F1 IMPLEMENTATION AUTHORIZATION = YES
  solo para el alcance de F1 de la Proposal V3.1) y decisiones del Coordinator §10 (F1 en dos tareas; esta es la primera) y
  §11 (concreciones CD-I65-F1-04..10), en AuthorityRevision; una sola cadena RED -> GREEN; Architect no exigido por defecto.
- RunId = R20261011T032206Z-7516 (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3
  (attempts es por unidad: 0 en el estado de HEAD; max_attempts 3 en el contrato de la iniciativa).
- Unit = "I-65"; Initiative = "I-65"; Gate = "F1"; TaskId = "F1-T1-NEUTRAL-MODEL" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T032206Z-7516" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el de routing.md
  para la clase elegida; PromptProfile = el de esa clase. La Proposal V1 §11 (vigente por V3.1 §11) da para F1 «Implementacion
  transversal» con effort Deep y Worker claude-sonnet-5-5/high; la tarea cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032206Z-7516/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL/0/R20261011T032206Z-7516/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «codigo», «guarda», «diseno», «interprete»), y algunas
  Section SI llevan tildes o simbolos (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «## G. Delegación de
  ejecución», «archivo completo (en especial §5, §9, §10 y §11)»). No las «corrijas» en ningun sentido. El Invariant de INV-01
  contiene la expresion \bRam[A-Z] con una sola barra invertida en el valor (dos en el texto JSON del archivo): conservala.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): nombra los archivos de produccion
  nuevos exactos (src/RackCad.Application/Units/MassUnits.cs, src/RackCad.Application/Units/ForceUnits.cs y los de
  src/RackCad.Application/StructuralAnalysis/ para cantidades, modelo, resultado y diagnosticos, lector de cargas, perfil de
  material, constructor del modelo, forma canonica y huellas) y el cambio de StructuralSectionUnits.cs; las clases de prueba
  nuevas, en archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/, en namespace RackCad.Tests (guarda de I-23) y con
  nombre que empieza por StructuralAnalysis (no por StructuralAnalysisCantilever, reservado a F1-T2) para que las seleccione
  el filtro del contrato. Las pruebas existentes no se modifican, borran ni renombran.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-proposal-v3.1.md (§2 y §3) y las concreciones CD-I65-F1-04..10 de docs/automation/decisions/I-65.md §11.
  Incluye:
  - RED compilable primero (esqueleto de producto que compila y pruebas que fallan por asercion o excepcion, seleccion >= 40),
    con push propio antes del GREEN para que su corrida push exista y su job Core falle; las guardas con autoprueba sobre
    codigo sintetico; la guarda de INV-16 debe fallar en el RED mientras StructuralSectionUnits conserve su literal;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, incluidas
    StructuralSectionModelTests, LengthUnitsAuthorityGuardTests, NamespaceFolderGuardTests y CantileverSourceGuardTests;
  - StructuralSectionUnits identico bit a bit y con su API publica sin cambios (si no es posible: parada C-03);
  - ninguna inferencia de material desde MaterialGrade, familia, designacion o nombre (si hiciera falta: parada C-07);
  - ningun archivo en Systems/, Providers/, Domain, Plugin, UI ni .csproj (si hiciera falta: parada C-01);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F1-T1, CONTROLLER_PLANNING)".
