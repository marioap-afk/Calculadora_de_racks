I-65 — DELEGACION F4-T4-COMPARATOR-AND-TOOL — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-T4-COMPARATOR-AND-TOOL; intento 3; RunId R20261011T210328Z-2ee6;
           DelegationRunId R20261011T210328Z-2ee6; AuthorityRevision 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261011T210328Z-2ee6
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, docs/initiatives/I-65-f4-implementation.md, el paquete de validacion de ingenieria,
           el paquete de preparacion, decisiones de I-65 (§21, §26 y §27), orden del Owner de F4, ADR-0050 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210328Z-2ee6/gate-contract.json (el MISMO contrato del intento 2,
           sin reemitir). Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (comparador puro, rechazos de D-11, emparejamiento, filas de la matriz de la
           seccion 4 con las tolerancias del Freeze, categoria y filas no evaluadas, informe determinista, INV-13, herramienta de
           consola fuera del producto y forma del cambio); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs (tipos neutrales de lectura de la tarea anterior) y los
           lectores del proveedor en src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (volcado, lectura de vuelta,
           manifiesto, plantilla del volcado); el escritor atomico neutral StructuralPackageWriter.cs; el precedente de herramienta
           tools/RackCad.StructuralSections.Import/ (Program.cs y .csproj); RackCad.sln y tests/RackCad.Tests/RackCad.Tests.csproj
           (solo la entrada y la referencia de la herramienta nueva)
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

DELTA DE LA TAREA (F4, delegacion de CORRECCION de la cadena F4-T4-COMPARATOR-AND-TOOL)
- Autorizacion: orden del Owner «I-65 — Owner Authorization: Conditional F4 Implementation» (decisiones I-65 §26:
  F4 IMPLEMENTATION AUTHORIZATION = YES, CONDITIONAL, F4 ONLY; sin corridas en RAM Elements), decisiones del Coordinator §26
  (F4 en cinco tareas; esta es la cuarta, con el reparto de CD-I65-F4-09), con la concrecion de docs/initiatives/I-65-f4-implementation.md
  seccion 5, en
  AuthorityRevision; una sola cadena RED -> GREEN; Architect no exigido en la tarea (revisa al terminar la parte implementable).
- Hechos de la cadena: primera delegacion R20261011T202527Z-1167 (contrato contra AuthorityRevision 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50); trabajo
  R20261011T202738Z-6356: el Worker se detuvo con STOP (C-01) tras sus commits RED b51adf28 y GREEN 0e9bbc29 porque dos aserciones
  de su propio RED en tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisComparisonToolTests.cs son defectuosas (literal
  $.faultInjection frente al marcador RamToolInjection.Marker = rackcadFaultInjection; orden ordinal esperado mal calculado) y dos
  advertencias de compilacion estan en archivos del RED (CS0649 en StructuralAnalysisComparisonSupport.cs, xUnit2013 en
  StructuralAnalysisComparisonToolTests.cs). El commit de sesion c8f3aa884abbc86c1c0cf4ff112dadda0c26fa68 registra la resolucion (decisiones I-65 §33: CD-I65-F4-18 y
  DEV-I65-F4-09; §34: CD-I65-F4-19, reemision de esta planificacion tras el rechazo P-03 de R20261011T205717Z-064f), su analisis en docs/automation/evidence/I-65-pilot/F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/analysis.md
  (SHA-256 5cafa306b7430f696af915c0348ac2d28981cfa07115b32ef83c828a82924802) y attempts = 3. El analisis se cita SOLO por CorrectionOf (no es autoridad).
- RunId = R20261011T210328Z-2ee6 (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3 (attempts es por unidad y no
  se reinicia por tarea; max_attempts 3 en el contrato de la iniciativa; esta es la ultima correccion posible de la unidad).
- CorrectionOf = { RunId: "R20261011T202738Z-6356", FailureClass: "StopCondition", AnalysisSha256: "5cafa306b7430f696af915c0348ac2d28981cfa07115b32ef83c828a82924802" }. Copia ese
  AnalysisSha256 caracter a caracter (64 hexadecimales en minusculas, ya en el formato del esquema): en la planificacion rechazada
  R20261011T205717Z-064f salio con cinco caracteres distintos.
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-T4-COMPARATOR-AND-TOOL" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50 (BaseSha de la primera delegacion de la cadena); ChainRedSha = null y
  ChainRedFiles = [] (la cadena no acredito ningun RED: el intento 2 se detuvo sin verificacion), asi que esta entrega EXIGE RED.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T210328Z-2ee6" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  tarea es un comparador neutral con muchas filas y una herramienta de consola nueva con su proyecto: alcance amplio pero mecanico;
  cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210328Z-2ee6/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210328Z-2ee6/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «funcion», «publica», «lectura», «huella», «emparejamiento»,
  «posicion», «conectividad», «seccion», «categoria», «evaluadas», «informe», «serializacion», «herramienta», «inyeccion»,
  «mutacion», «semantica», «solucion», «advertencia»), y algunas Section SI llevan tildes o simbolos (p. ej. «## 16. Ejecución
  delegada bajo orden del Coordinator», «## G. Delegación de ejecución», «archivo completo (en especial §21, §26 y §27)», «archivo
  completo (en especial §0, §1, §5 y §7)», «archivo completo (en especial §2 y §3)», «archivo completo (en especial §5 y §6)» y la
  Section de la Proposal con §3 y §4). No las «corrijas» en ningun sentido. ATENCION: en la planificacion rechazada
  R20261011T205717Z-064f el simbolo de seccion de Authorities[0].Section salio corrompido en un caracter de control: escribe el
  simbolo de seccion tal cual (el caracter U+00A7), nunca como secuencia de escape, y en el preflight compara cada Section caracter a
  caracter con el archivo. Ningun valor del contrato contiene barras invertidas: no
  las introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): el archivo NUEVO de produccion
  src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs; el proyecto NUEVO tools/RackCad.StructuralAnalysis.RamTool/
  (su .csproj y sus archivos); una entrada nueva en RackCad.sln y una ProjectReference nueva en tests/RackCad.Tests/RackCad.Tests.csproj;
  las clases de prueba nuevas, en archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/, en namespace RackCad.Tests y con nombre que
  empieza por StructuralAnalysisComparison. Ninguna prueba existente cambia.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-f4-implementation.md seccion 5, el paquete de validacion de ingenieria §3 F y docs/initiatives/I-65-proposal-v3.1.md
  (D-11, D-12, D-13; §3 INV-13; §4). Incluye:
  - correccion por RED desactivado (AUTOMATION_PLAN 16.8; CD-I65-F4-18): commit RED = las pruebas de la cadena corregidas (SOLO las dos
    aserciones defectuosas, que pasan a esperar "$." + RamToolInjection.Marker y el orden ordinal real, y la eliminacion de las dos
    advertencias sin debilitar lo observado) mas la implementacion DESACTIVADA (StructuralReadbackComparison.cs y
    tools/RackCad.StructuralAnalysis.RamTool/ devueltos al esqueleto compilable de b51adf28), de modo que la seleccion focal falla por
    excepcion o asercion; push propio antes del GREEN para que su corrida push exista y su job Core falle;
  - commit GREEN = la implementacion restaurada byte a byte desde 0e9bbc29 (git diff 0e9bbc29 <GREEN> -- src tools RackCad.sln vacio)
    sin tocar ningun archivo de tests/;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, incluidas F1, F3, T1, T2, T3 y las guardas;
    la solucion completa compila en Release sin advertencias;
  - las tolerancias del Freeze no se alteran (si hiciera falta: parada C-07);
  - ninguna prueba ejecuta Python ni RAM Elements; el Worker no invoca un interprete Python por ningun motivo (C-08);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T4, CONTROLLER_PLANNING)".
