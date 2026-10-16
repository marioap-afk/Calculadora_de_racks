I-65 — DELEGACION F4-D-SECTION-MAP-VERIFIED — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-D-SECTION-MAP-VERIFIED; intento 7; RunId R20261016T153224Z-5bb3;
           DelegationRunId R20261016T153224Z-5bb3; AuthorityRevision e886b5577ad565ef4a55bb2e535b7d463f345b79;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261016T153224Z-5bb3
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): el contrato de F4-D docs/initiatives/I-65-f4-d-mapping-verified-contract.md, la autorizacion literal del Owner
           (D-09 y D-10), las decisiones de I-65 (§62), la tabla objetivo docs/automation/evidence/I-65-f4/f4d-table-target.json,
           el paquete de aprobacion de ingenieria y su seguimiento, la Proposal V3.1 congelada, el contrato de la iniciativa,
           ADR-0050 y las demas; vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la CORRECCION de la tarea descrita en el
           contrato de gate artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T153224Z-5bb3/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (D-INV-01 solo las dos filas y sus campos; D-INV-02 blob de la tabla igual al del
           archivo objetivo y pruebas con sus literales; D-INV-03 DIF-01 no cambia el valor esperado ni las tolerancias; D-INV-04
           REF-CANT-01 Valid y paquete Valid en memoria con la tabla distribuida, y la huella cambia con un campo verificado;
           D-INV-05 la cobertura proposed se conserva con tablas de fixture; D-INV-06 sin escrituras fuera de la carpeta temporal
           ni publicacion; D-INV-07 controles del Owner; forma RED/GREEN; INV-14 y guardas; lo que no cambia del Freeze;
           controles operativos del Owner); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           assets/structural-analysis/ram-elements/section-material-map.json (hoy con las dos filas verified, blob del objetivo);
           docs/automation/evidence/I-65-f4/f4d-table-target.json (la tabla objetivo, literal);
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamBuildPackageFactory.cs (IsAbsoluteNormalisedFolder: la
           carpeta del paquete debe ser una ruta WINDOWS absoluta; solo lectura);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs y
           StructuralAnalysisPluginCommandProductTableTests.cs (las dos pruebas que corrige la delegacion)
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

DELTA DE LA TAREA (F4, delegacion de CORRECCION de la cadena F4-D-SECTION-MAP-VERIFIED)
- Autorizacion: orden del Owner «I-65 — OWNER AUTHORIZATION D-09 Engineering Sign-Off / D-10 F4-D Implementation» (decisiones
  I-65 §62: hasta tres Workers; tope de Codex 48; max_attempts 9; sin AutoCAD ni RAM Elements; sin cambios en src/), con el contrato
  de F4-D en su revision 9f6fff6e; el contrato de gate es el MISMO de la primera delegacion (sin reemision).
- Hechos de la cadena: primera delegacion R20261016T144346Z-4eaf (contrato contra e886b557); trabajo R20261016T145459Z-8301:
  RED fe9b6720 (CI con Core en failure, 13 fallidas por asercion) y GREEN d548fb0b (solo la tabla; blob igual al del objetivo);
  verificacion R20261016T152242Z-2e6f: EXECUTION_REWORK_REQUIRED, FailureClass Ci: la CI del GREEN tuvo 3 fallidas porque tres
  pruebas pasan Path.GetTempPath() como carpeta del paquete (en el runner Linux /tmp/...) y la fabrica exige una ruta WINDOWS absoluta
  (RAM-PACKAGE-FOLDER-INVALID). RED acreditado (RedPart pass en Ci y Tests). El commit de sesion 2a1ed99a0a2540f80d45321963a28fcb391c8a05 registra el REWORK
  (decisiones §63, CD-I65-F4-109..113) y attempts = 7. El analisis esta en
  docs/automation/evidence/I-65-pilot/F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/analysis.md (SHA-256 15c626b405fb279d6f86519fe544175a0446a04555f00e9e50abd2961d4c4696); se cita SOLO
  por CorrectionOf (no es autoridad).
- RunId = R20261016T153224Z-5bb3 (tambien es el DelegationRunId); Attempt = 7; AttemptsRemaining = 2; MaxReworkLoops = 3 (attempts es por
  unidad y no se reinicia por tarea; max_attempts 9 en el contrato de la iniciativa por decision del Owner, decisiones §62).
- CorrectionOf = { RunId: "R20261016T152242Z-2e6f", FailureClass: "Ci", AnalysisSha256: "15c626b405fb279d6f86519fe544175a0446a04555f00e9e50abd2961d4c4696" }. Copia ese AnalysisSha256
  caracter a caracter (64 hexadecimales en minusculas, ya en el formato del esquema).
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-D-SECTION-MAP-VERIFIED" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = e886b5577ad565ef4a55bb2e535b7d463f345b79 (BaseSha de la primera delegacion de la cadena);
  ChainRedSha = fe9b6720b7b261a63b4ff7b4a246255dec8b7265 (RED acreditado);
  ChainRedFiles = EXACTAMENTE esta lista, en este orden (git diff --name-only e886b557 fe9b6720 -- tests/):
    ["tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandProductTableTests.cs",
     "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsMapReaderTests.cs",
     "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageValidationTests.cs",
     "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsTranslationTests.cs",
     "tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs"]
  La correccion toca ChainRedFiles, asi que esta entrega EXIGE RED (RED caducado).
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261016T153224Z-5bb3" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  correccion es de pruebas: cambiar la carpeta del paquete en tres pruebas de dos archivos por una ruta WINDOWS absoluta fija, con un
  RED por correccion desactivada (pruebas corregidas mas la tabla devuelta a su contenido de e886b557) y un GREEN que vuelve a copiar
  el archivo objetivo sobre la tabla; sin codigo de produccion ni nada externo; cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T153224Z-5bb3/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T153224Z-5bb3/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
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
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): ediciones de
  tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs (las pruebas
  LaFabricaDaUnPaqueteValidoEnMemoriaConLaTablaDistribuidaSinEscribirLaCarpeta y CambiarUnCampoDeUnaFilaVerifiedCambiaLaHuellaDelPaquete)
  y de tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandProductTableTests.cs (la prueba
  ConLaTablaDeProducto_LaFabricaDaUnPaqueteValidoEnMemoria_SinCrearLaCarpeta); la tabla
  assets/structural-analysis/ram-elements/section-material-map.json SOLO como correccion desactivada en el RED (contenido de
  e886b557) y restaurada en el GREEN copiando docs/automation/evidence/I-65-f4/f4d-table-target.json. Ningun archivo de src/ ni de tools/.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen el contrato de F4-D
  (secciones 3, 4 y 5), las decisiones §62 y §63 y la tabla objetivo. Incluye:
  - la carpeta del paquete de las tres pruebas es una ruta WINDOWS absoluta fija que no existe (valida en Windows y en Linux, porque la
    fabrica valida la ruta sin escribir); ninguna otra asercion ni prueba cambia;
  - RED compilable primero: las dos pruebas corregidas mas la tabla devuelta a su contenido de e886b557; con ella las pruebas que
    esperan la tabla verified fallan por asercion (nunca compilacion); seleccion del filtro >= 1789; push propio antes del GREEN para que
    su corrida push exista y su job Core falle;
  - el GREEN cambia SOLO la tabla, copiada de nuevo del archivo objetivo, con su blob igual al del objetivo en AuthorityRevision; no
    modifica ningun archivo de pruebas (git diff RED..GREEN -- tests vacio); suite Core completa en verde; la solucion completa compila en
    Release sin advertencias nuevas atribuibles al diff; el diff neto BaseSha..CurrentSha son solo las dos pruebas;
  - con la tabla distribuida, REF-CANT-01 da Valid con modelo y sin diagnosticos bloqueantes, y el paquete es Valid en memoria sin
    publicarse; si aparece cualquier diagnostico distinto de los previstos, parada C-03 con su causa;
  - ninguna prueba ejecuta AutoCAD, Python ni RAM Elements; el Worker no invoca un interprete Python por ningun motivo (C-08); ningun
    archivo temporal fuera de la carpeta work/ del intento ni sondas compiladas en tests/ (DEV-I65-F4-23);
  - el Worker informa su directorio efectivo al empezar y usa rutas absolutas (C-12);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-D, CONTROLLER_PLANNING)".
