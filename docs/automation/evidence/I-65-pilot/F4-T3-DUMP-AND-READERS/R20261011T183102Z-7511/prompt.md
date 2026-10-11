I-65 — DELEGACION F4-T3-DUMP-AND-READERS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-T3-DUMP-AND-READERS; intento 2; RunId R20261011T183102Z-7511;
           DelegationRunId R20261011T183102Z-7511; AuthorityRevision c73058f88368196309975616298523317ace2813;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261011T183102Z-7511
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, docs/initiatives/I-65-f4-implementation.md, el paquete de validacion de ingenieria,
           el documento de F3, decisiones de I-65 (§17, §26, §27 y §31), orden del Owner de F4, informe de P1, ADR-0050 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183102Z-7511/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (script fijo de volcado, solo lectura y contrato de unidades, contenido del
           volcado y API frente a P1, lector estricto con conversion al marco neutral e ida y vuelta, lectores de la lectura de vuelta
           y del manifiesto, tipos neutrales, INV-14 y forma del cambio, restricciones del Owner); una delegacion nunca amplia el
           contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (F3 y F4-T1 verificadas: plantilla de construccion,
           carga util, manifiesto, traduccion y RamBuildPlan, regla de carpeta, codigos); src/RackCad.Application/StructuralAnalysis/
           (modelo neutral, cantidades tipadas, diagnosticos; solo lectura salvo StructuralReadback.cs NUEVO);
           docs/automation/evidence/I-65-f0/p1-run/p1-report.json (las 207 funciones del nucleo); fuente instalada del paquete del
           proveedor, solo lectura (entities/node.py, entities/member.py, loads/load_condition.py, properties/units.py, model.py);
           las pruebas existentes (solo lectura, sin excepcion: CD-I65-F4-13)
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

DELTA DE LA TAREA (F4, primera delegacion de la cadena F4-T3-DUMP-AND-READERS)
- Autorizacion: orden del Owner «I-65 — Owner Authorization: Conditional F4 Implementation» (decisiones I-65 §26:
  F4 IMPLEMENTATION AUTHORIZATION = YES, CONDITIONAL, F4 ONLY; sin corridas en RAM Elements), decisiones del Coordinator §26
  (F4 en cinco tareas; esta es la tercera), con la concrecion de docs/initiatives/I-65-f4-implementation.md seccion 4, en
  AuthorityRevision; una sola cadena RED -> GREEN; Architect no exigido en la tarea (revisa al terminar la parte implementable).
- RunId = R20261011T183102Z-7511 (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por tarea: vale 2 en el estado de HEAD (correcciones de F4-T1 y F4-T2, decisiones §27 y §29);
  max_attempts 3 en el contrato de la iniciativa). Esta es la PRIMERA delegacion de la cadena F4-T3-DUMP-AND-READERS.
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-T3-DUMP-AND-READERS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T183102Z-7511" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  Proposal V1 §11 (vigente por V3.1 §11) da para «Adaptador y plantilla Python» Deep con Worker claude-sonnet-5-5/high; la tarea
  cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183102Z-7511/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183102Z-7511/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «carga util», «volcado», «solo lectura», «unico»,
  «asignacion», «restauracion», «autoprueba», «sinteticas», «descripcion», «liberaciones», «categoria», «combinaciones»,
  «terminos», «conversion», «posicion», «senal», «exencion», «analisis», «afirmacion», «validacion»), y algunas Section SI llevan
  tildes o simbolos (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «## G. Delegación de ejecución», «archivo
  completo (en especial §17, §26, §27 y §31)», «archivo completo (en especial §0, §1 y §4)», «archivo completo (en especial §2 y §3)» y la
  Section de la Proposal con §3). No las «corrijas» en ningun sentido. Ningun
  valor del contrato contiene barras invertidas: no las introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): los archivos NUEVOS de produccion bajo
  src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (plantilla del volcado, su carga util, lector del volcado, lector de
  la lectura de vuelta y del manifiesto) y src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs (tipos neutrales); las clases
  de prueba nuevas, en archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/, en namespace RackCad.Tests y con nombre que empieza
  por StructuralAnalysisRamElementsDump; ninguna prueba existente se edita y la exencion de INV-14 no cambia (CD-I65-F4-13): las
  comprobaciones de la plantilla del volcado obtienen el nombre del modulo del texto de la plantilla de construccion (precedente F4-T1,
  StructuralAnalysisRamElementsHardeningScans.ToolName), sin partir literales.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-f4-implementation.md seccion 4 y docs/initiatives/I-65-proposal-v3.1.md (D-01, D-03, D-04, D-08, D-09, D-11,
  D-12; §3 INV-01, 02, 10 y 14). Incluye:
  - RED compilable primero (esqueleto y pruebas que fallan por asercion o excepcion, seleccion >= 25), con push
    propio antes del GREEN para que su corrida push exista y su job Core falle;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, incluidas F1, F3, T1, T2 y las guardas;
  - la ida y vuelta de REF-CANT-01 por el volcado sintetico reproduce el modelo neutral (si no: parada C-03);
  - ninguna prueba ejecuta Python ni RAM Elements; la plantilla del volcado se comprueba de forma estatica; el Worker no invoca un
    interprete Python por ningun motivo; ninguna llamada fuera de la fuente instalada ni funcion del nucleo fuera de P1 (si no: C-11);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F4-T3, CONTROLLER_PLANNING)".
