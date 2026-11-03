I-65 — DELEGACION F4-T6-COMPARATOR-HARDENING — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-T6-COMPARATOR-HARDENING; intento 3; RunId R20261013T213942Z-4bec;
           DelegationRunId R20261013T213942Z-4bec; AuthorityRevision ef3536156f6a69cdee42a687930877eefc4a585b;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261013T213942Z-4bec
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, docs/initiatives/I-65-f4-implementation.md (secciones 9 y 10), el paquete de
           validacion de ingenieria, el paquete de preparacion, decisiones de I-65 (§36, §40 y §41), ordenes literales del
           Owner de la reanudacion, la conformidad del Architect de T1-T4, ADR-0050 y las demas; vigencia: AuthorityRevision
           para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T213942Z-4bec/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (A65-F4-01 nudos euclideos y valor de carga respecto del manual con el caso cero;
           A65-F4-02 sin PASS global con combinaciones no leidas; A65-F4-03 ligadura del volcado del generado y entradas;
           A65-F4-04 paquetes de inyeccion solo en tools/; A65-F4-05 categoria vacia con semantica NO MEDIDA; A65-F4-06 ensayo
           general; lo que no cambia del Freeze; forma del cambio; controles operativos del Owner); una delegacion nunca amplia
           el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs (comparador de F4-T4);
           src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs (tipos neutrales de lectura de T3);
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamTransferReaders.cs, RamDumpReader.cs y
           RamBuildScriptTemplate.cs (funcion read_back, solo lectura); tools/RackCad.StructuralAnalysis.RamTool/ (compare, inject);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisComparison*.cs y StructuralAnalysisRamElementsDump*.cs
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

DELTA DE LA TAREA (F4, primera delegacion de la cadena F4-T6-COMPARATOR-HARDENING)
- Autorizacion: orden del Owner «I-65 — OWNER AUTHORIZATION: F4 Controlled Resumption — Final GO» (decisiones I-65 §41, OD-GO:
  IMPLEMENTATION AUTHORIZATION = F4-T6 + F4-T5 ONLY; entorno original con el riesgo residual aceptado, OD-RET; sin corridas en RAM
  Elements), con la concrecion de docs/initiatives/I-65-f4-implementation.md secciones 9 y 10, en AuthorityRevision; una sola cadena
  RED -> GREEN; Architect no exigido en la tarea (revisa al terminar las dos cadenas).
- RunId = R20261013T213942Z-4bec (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 3; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por tarea: vale 3 en el estado de HEAD; max_attempts 6 en el contrato de la
  iniciativa por decision del Owner). Esta es la PRIMERA delegacion de la cadena F4-T6-COMPARATOR-HARDENING.
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-T6-COMPARATOR-HARDENING" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261013T213942Z-4bec" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  tarea toca un comparador neutral puro, dos lectores estrictos del proveedor, una herramienta de consola y sus pruebas, con un
  esqueleto RED y fixtures sinteticos, sin ejecutar nada externo; cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T213942Z-4bec/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T213942Z-4bec/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «euclidea», «numerico», «ambiguedad», «diagonal»,
  «decision», «comparacion», «combinaciones», «leidas», «inyeccion», «semantica», «validacion», «ingenieria», «seccion»,
  «variante», «posiciones», «porcentaje», «fixtures», «anadido», «minusculo», «solucion», «interprete», «unico»), y algunas
  Section SI llevan tildes o simbolos (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «## G. Delegación de
  ejecución», «archivo completo (en especial §5, §9 y §10)», «archivo completo (en especial §36, §40 y §41)», «archivo completo
  (en especial §2 y §3)», «archivo completo (en especial §5 y §6)» y la Section de la Proposal con §3 y §4). No las «corrijas»
  en ningun sentido. ATENCION: escribe el simbolo de seccion tal cual (el caracter U+00A7), nunca como secuencia de escape (en
  la planificacion R20261011T205717Z-064f salio corrompido en un caracter de control), y en el preflight compara cada Section
  caracter a caracter con el archivo. Algunos Invariants contienen corchetes, barras y signos (p. ej. «sqrt(dx2+dy2+dz2) <= 0.02
  in», «|g - m| / |m| <= 0.1 %», «[Environment]::CurrentDirectory», «C:/Users/alejandra-mendoza/.codex/worktrees/...»,
  «D:/Documentos/Codex/Calculadora de racks»): copialos exactos; ningun valor del contrato contiene barras invertidas: no las
  introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): produccion en
  src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs, src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs
  (solo anadidos), src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamTransferReaders.cs,
  src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamDumpReader.cs y archivos de tools/RackCad.StructuralAnalysis.RamTool/
  (sin su .csproj); ediciones nombradas de tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisComparisonRowTests.cs,
  StructuralAnalysisComparisonToolTests.cs y StructuralAnalysisComparisonSupport.cs; clases de prueba nuevas en archivos NUEVOS de
  tests/RackCad.Tests/StructuralAnalysis/ (namespace RackCad.Tests, nombre que empieza por StructuralAnalysisComparisonHardening) y
  fixtures NUEVOS en tests/RackCad.Tests/StructuralAnalysis/Fixtures/rehearsal/.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-f4-implementation.md secciones 9 y 10, las decisiones §41 (OD-GO, CD-I65-F4-31..34) y
  docs/initiatives/I-65-proposal-v3.1.md (D-11, D-12, D-13; §3 INV-13; §4). Incluye:
  - RED compilable primero (esqueleto sin comportamiento, todas las pruebas nuevas, fixtures y ediciones nombradas; seleccion
    >= 101), con push propio antes del GREEN para que su corrida push exista y su job Core falle;
  - el GREEN no modifica ningun archivo de pruebas ni fixture del RED; suite Core completa en verde, incluidas F1, F3, T1-T4 y
    todas las guardas; la solucion completa compila en Release sin advertencias nuevas atribuibles al diff;
  - ninguna tolerancia del Freeze cambia; las filas de propiedades siguen NOT_EVALUATED; ningun texto presenta la comparacion
    como validacion estructural;
  - ninguna prueba ejecuta AutoCAD, Python ni RAM Elements; el Worker no invoca un interprete Python por ningun motivo (C-08);
  - el Worker informa su directorio efectivo al empezar y usa rutas absolutas (C-12);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F4-T6, CONTROLLER_PLANNING)".
