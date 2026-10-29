I-65 — DELEGACION RD11-AUTOLOADER-TABLE — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate R-D11; tarea RD11-AUTOLOADER-TABLE; intento 7; RunId R20261029T004516Z-bf20;
           DelegationRunId R20261029T004516Z-bf20; AuthorityRevision 91f82d6d83731f48d445c7ad007a814b65c6caa6;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261029T004516Z-bf20
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): el contrato del gate docs/initiatives/I-65-rd11-autoloader-gate.md, la disposicion literal del Owner y su
           respuesta sobre el techo de Codex, las decisiones de I-65 (§105), la preparacion de READY (§4), la Proposal V3.1
           congelada, el contrato de la iniciativa, ADR-0050, ADR-0003 y las demas; vigencia: AuthorityRevision para la unidad,
           MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T004516Z-bf20/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (RD-INV-01 el target copia la tabla byte a byte a
           Contents/structural-analysis/ram-elements/section-material-map.json; RD-INV-02 el verificador admite solo esa ruta,
           exige presencia e igualdad SHA-256 con la fuente y no relaja nada; RD-INV-03 harness con el valido y los negativos
           nuevos; RD-INV-04 solo cambia la prueba de distribucion; RD-INV-05 tabla, src/*.cs y el comando sin cambio;
           RD-INV-06 documentacion minima sin presentar la tabla como validacion estructural; RD-INV-07 controles del Owner;
           forma RED/GREEN; lo que no cambia del Freeze; controles operativos del Owner); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Plugin/RackCad.Plugin.csproj (target AssembleAutoloaderBundle y elemento Content de la tabla);
           deploy/verify-bundle.ps1 y deploy/test-verify-bundle.ps1; deploy/build-bundle.ps1 (solo lectura: publica y verifica);
           src/RackCad.Plugin/RackRamElementsCommands.cs (solo lectura: busca la tabla junto al DLL del Plugin);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs (la prueba
           LaTablaNoViajaEnElPaqueteDelAutoloader_NiElDestinoDelPaqueteSeToca); eng/ci/verify-autocad-references.ps1 (solo
           lectura: la CI publica y ejecuta el verificador y el harness)
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

DELTA DE LA TAREA (gate correctivo R-D11, primera delegacion de la cadena RD11-AUTOLOADER-TABLE)
- Autorizacion: orden del Owner «I-65 — OWNER DISPOSITION / READY Decisions / Autoloader Correction Gate» (decisiones I-65 §105:
  R-D11 = OPTION B, R-D11 IMPLEMENTATION = AUTHORIZED WITHIN DEFINED SCOPE; techo de Codex 57 por respuesta del Owner;
  max_attempts 11; UN solo Worker; sin AutoCAD ni RAM Elements), con el contrato del gate en su revision 91f82d6d83731f48d445c7ad007a814b65c6caa6; una sola cadena
  RED -> GREEN; CorrectionsAuthorized = false en el contrato (un REWORK sera una parada).
- RunId = R20261029T004516Z-bf20 (tambien es el DelegationRunId); Attempt = 7; AttemptsRemaining = 4; MaxReworkLoops = 3
  (attempts es por unidad y no se reinicia por tarea: vale 7 en el estado de HEAD; max_attempts 11 en el contrato de la
  iniciativa por decision del Owner, decisiones §105). Esta es la PRIMERA delegacion de la cadena RD11-AUTOLOADER-TABLE.
- Unit = "I-65"; Initiative = "I-65"; Gate = "R-D11"; TaskId = "RD11-AUTOLOADER-TABLE" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261029T004516Z-bf20" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  tarea es de distribucion y pruebas: un target de MSBuild, un script de verificacion fail-closed y su harness en PowerShell,
  una prueba C# que se sustituye por pruebas positivas y tres documentos, con un RED y un GREEN, sin codigo de produccion C#;
  cabe en una invocacion del Worker (tope de 60 min). Como un REWORK sera una parada (un solo Worker), pondera la fiabilidad.
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T004516Z-bf20/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar, reordenar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE/7/R20261029T004516Z-bf20/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.ForbiddenWriteScope | ConvertTo-Json; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «seccion», «decisiones», «asercion», «compilacion»,
  «unica», «ejecucion», «instalacion», «documentacion», «limitacion», «edicion», «catalogo», «validacion», «certificacion»,
  «modificacion», «comprobacion», «regeneracion», «operacion», «fotografia», «despues», «cesion», «condicion», «semantica»,
  «interprete», «linea», «anade», «anadido», «anadir», «publicacion», «verificacion», «distribucion», «ningun», «arquitectura»), y
  la Section «## Convenciones arquitectonicas (obligatorias)» de AGENTS.md esta SIN tilde en «arquitectonicas» (un Controller
  anterior la acentuo y la delegacion fue rechazada); en cambio algunas Section SI llevan tildes (p. ej. «## 16. Ejecución
  delegada bajo orden del Coordinator», «## G. Delegación de ejecución»). No las «corrijas» en ningun sentido. Escribe el
  simbolo de seccion tal cual (el caracter U+00A7) si aparece, nunca como secuencia de escape, y en el preflight compara cada
  Section caracter a caracter con el archivo. ForbiddenWriteScope tiene 65 entradas en un orden fijo: sus dos primeras son
  la tabla assets/structural-analysis/ram-elements/section-material-map.json y el prefijo assets/ (ambas a proposito); ninguna
  entrada lleva espacios al principio ni al final (una planificacion anterior fue rechazada por un espacio inicial,
  CD-I65-F4-114). Algunos Invariants contienen barras, dos puntos, guiones y signos (p. ej. «Contents/structural-analysis/ram-
  elements/section-material-map.json», «RD-INV-01», «C01-C18», «-SectionMapSourcePath», «-CatalogsSourceDir», «-Output»,
  «[Environment]::CurrentDirectory», «C:/Users/<usuario>/.codex/worktrees/...», «D:/Documentos/Codex/Calculadora de
  racks»): copialos exactos; ningun valor del contrato contiene barras invertidas: no las introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants y
  ForbiddenWriteScope, igualdad exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una
  delegacion que sabes invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): RED = la prueba de distribucion de
  tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs y deploy/test-verify-bundle.ps1; GREEN =
  el target AssembleAutoloaderBundle de src/RackCad.Plugin/RackCad.Plugin.csproj, deploy/verify-bundle.ps1, README.md (fila de
  RACKRAMPAQUETE), docs/guias/interoperabilidad-ram-elements.md (secciones 2, 9, 10 y 11) y docs/guias/despliegue.md (seccion 4).
  Ningun archivo nuevo. Ningun .cs de src/.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen el contrato del
  gate (secciones 1, 2 y 7) y las decisiones §105. Incluye:
  - RED compilable primero (solo la prueba de distribucion y el harness; seleccion del filtro >= 1795; las pruebas positivas
    fallan por asercion, nunca por compilacion; el harness local falla sobre el bundle sin tabla), con push propio antes del
    GREEN para que su corrida push exista y su job Core falle;
  - el GREEN cambia SOLO el target, el verificador y los tres documentos; no modifica ningun archivo del RED; el blob de la tabla
    sigue siendo d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2; filtro y suite Core completa en verde; la solucion completa compila en
    Release sin advertencias nuevas atribuibles al diff;
  - en GREEN, deploy/build-bundle.ps1 -Output dentro de la carpeta work/ del Worker publica un bundle con la tabla en
    Contents/structural-analysis/ram-elements/section-material-map.json (SHA-256 igual al de la fuente), el verificador lo acepta y
    el harness da todos sus casos en verde (el valido, los nueve negativos actuales y los nuevos);
  - el verificador no relaja ni elimina ninguna comprobacion existente ni cambia sus mensajes;
  - los documentos solo quitan la limitacion de CD-I65-F4-10 y conservan las demas limitaciones; nada presenta la tabla como
    validacion estructural;
  - ninguna ejecucion de AutoCAD, Python, RAM Elements ni install-bundle.ps1; si AutoCAD esta abierto, parada C-11;
  - el Worker informa su directorio efectivo al empezar y usa rutas absolutas (C-12);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion RD11, CONTROLLER_PLANNING)".
