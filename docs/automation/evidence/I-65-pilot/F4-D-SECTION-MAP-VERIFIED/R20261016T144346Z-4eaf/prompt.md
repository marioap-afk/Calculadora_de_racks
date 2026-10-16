I-65 — DELEGACION F4-D-SECTION-MAP-VERIFIED — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-D-SECTION-MAP-VERIFIED; intento 6; RunId R20261016T144346Z-4eaf;
           DelegationRunId R20261016T144346Z-4eaf; AuthorityRevision e886b5577ad565ef4a55bb2e535b7d463f345b79;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261016T144346Z-4eaf
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): el contrato de F4-D docs/initiatives/I-65-f4-d-mapping-verified-contract.md, la autorizacion literal del Owner
           (D-09 y D-10), las decisiones de I-65 (§62), la tabla objetivo docs/automation/evidence/I-65-f4/f4d-table-target.json,
           el paquete de aprobacion de ingenieria y su seguimiento, la Proposal V3.1 congelada, el contrato de la iniciativa,
           ADR-0050 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T144346Z-4eaf/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (D-INV-01 solo las dos filas y sus campos; D-INV-02 blob de la tabla igual al del
           archivo objetivo y pruebas con sus literales; D-INV-03 DIF-01 no cambia el valor esperado ni las tolerancias; D-INV-04
           REF-CANT-01 Valid y paquete Valid en memoria con la tabla distribuida, y la huella cambia con un campo verificado;
           D-INV-05 la cobertura proposed se conserva con tablas de fixture; D-INV-06 sin escrituras fuera de la carpeta temporal
           ni publicacion; D-INV-07 controles del Owner; forma RED/GREEN; INV-14 y guardas; lo que no cambia del Freeze;
           controles operativos del Owner); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           assets/structural-analysis/ram-elements/section-material-map.json (la tabla del producto, hoy con dos filas proposed);
           docs/automation/evidence/I-65-f4/f4d-table-target.json (la tabla objetivo, literal);
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamSectionMapReader.cs, RamSectionMapRow.cs,
           RamBuildPackageFactory.cs (solo lectura); tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsFixtures.cs y
           StructuralAnalysisCantileverFixtures.cs (solo lectura); los cuatro archivos de prueba con ediciones nombradas
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

DELTA DE LA TAREA (F4, primera delegacion de la cadena F4-D-SECTION-MAP-VERIFIED)
- Autorizacion: orden del Owner «I-65 — OWNER AUTHORIZATION D-09 Engineering Sign-Off / D-10 F4-D Implementation» (decisiones
  I-65 §62: D-09 APPROVED, D-10 APPROVED, F4-D-SECTION-MAP-VERIFIED = IMPLEMENTATION AUTHORIZED; tope de Codex 48; max_attempts 9;
  sin AutoCAD ni RAM Elements; sin cambios en src/), con el contrato de F4-D en su revision 9f6fff6e; una sola cadena RED -> GREEN.
- RunId = R20261016T144346Z-4eaf (tambien es el DelegationRunId); Attempt = 6; AttemptsRemaining = 3; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por tarea: vale 6 en el estado de HEAD; max_attempts 9 en el contrato de la
  iniciativa por decision del Owner, decisiones §62). Esta es la PRIMERA delegacion de la cadena F4-D-SECTION-MAP-VERIFIED.
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-D-SECTION-MAP-VERIFIED" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261016T144346Z-4eaf" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  tarea es de datos y pruebas: copiar un archivo JSON objetivo sobre la tabla del producto, una clase de prueba nueva y
  ediciones nombradas de cuatro archivos de prueba, con un RED y un GREEN, sin codigo de produccion ni nada externo; cabe en
  una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T144346Z-4eaf/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T144346Z-4eaf/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «seccion», «decisiones», «asercion», «excepcion»,
  «compilacion», «linea», «proyeccion», «traduccion», «propagacion», «condicion», «modulo», «interprete», «edicion»,
  «catalogo», «validacion», «certificacion», «comparacion», «oraculo», «anade», «anadir», «estacion», «diagnosticos»,
  «solucion», «disposicion», «semantica», «fotografia», «despues», «cesion», «unica», «operacion», «autorizacion»), y la
  Section «## Convenciones arquitectonicas (obligatorias)» de AGENTS.md esta SIN tilde en «arquitectonicas» (un Controller
  anterior la acentuo y la delegacion fue rechazada); en cambio algunas Section SI llevan tildes (p. ej. «## 16. Ejecución
  delegada bajo orden del Coordinator», «## G. Delegación de ejecución»). No las «corrijas» en ningun sentido. Escribe el
  simbolo de seccion tal cual (el caracter U+00A7), nunca como secuencia de escape, y en el preflight compara cada Section
  caracter a caracter con el archivo. Algunos Invariants contienen corchetes, angulos, barras, dos puntos y signos (p. ej.
  «git rev-parse <GREEN>:assets/structural-analysis/ram-elements/section-material-map.json», «<AuthorityRevision>:», «U+00E9»,
  «U+2014», «D-INV-01..07», «[Environment]::CurrentDirectory», «C:/Users/alejandra-mendoza/.codex/worktrees/...»,
  «D:/Documentos/Codex/Calculadora de racks»): copialos exactos; ningun valor del contrato contiene barras invertidas: no las
  introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): la tabla
  assets/structural-analysis/ram-elements/section-material-map.json (GREEN: copia byte a byte de
  docs/automation/evidence/I-65-f4/f4d-table-target.json); la prueba NUEVA
  tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs (namespace RackCad.Tests); ediciones
  nombradas de tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsMapReaderTests.cs,
  StructuralAnalysisRamElementsPackageValidationTests.cs, StructuralAnalysisRamElementsTranslationTests.cs y
  StructuralAnalysisPluginCommandProductTableTests.cs. Ningun archivo de src/ ni de tools/.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen el contrato de F4-D
  (secciones 3, 4 y 5), las decisiones §62 y la tabla objetivo. Incluye:
  - RED compilable primero (la prueba nueva y las ediciones nombradas, que esperan la tabla verified; seleccion del filtro >= 1789;
    fallan por asercion o excepcion, nunca por compilacion), con push propio antes del GREEN para que su corrida push exista y su
    job Core falle;
  - el GREEN cambia SOLO la tabla y su blob es igual al del archivo objetivo en AuthorityRevision; no modifica ningun archivo de
    pruebas del RED; suite Core completa en verde; la solucion completa compila en Release sin advertencias nuevas atribuibles al
    diff;
  - con la tabla distribuida, REF-CANT-01 da Valid con modelo y sin diagnosticos bloqueantes, y el paquete es Valid en memoria sin
    publicarse; si aparece cualquier diagnostico distinto de los previstos, parada C-03 con su causa;
  - ninguna tolerancia cambia; unitWeight es 490 pcf, nunca el de RAM; shearModulus null; nada presenta la tabla verified como
    validacion estructural ni certificacion del material;
  - ninguna prueba ejecuta AutoCAD, Python ni RAM Elements; el Worker no invoca un interprete Python por ningun motivo (C-08);
  - el Worker informa su directorio efectivo al empezar y usa rutas absolutas (C-12);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion F4-D, CONTROLLER_PLANNING)".
