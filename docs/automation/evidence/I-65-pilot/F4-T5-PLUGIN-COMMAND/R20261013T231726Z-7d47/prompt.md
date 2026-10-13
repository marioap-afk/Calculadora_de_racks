I-65 — DELEGACION F4-T5-PLUGIN-COMMAND (CORRECCION) — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-65; gate F4; tarea F4-T5-PLUGIN-COMMAND; intento 5; RunId R20261013T231726Z-7d47;
           DelegationRunId R20261013T231726Z-7d47; AuthorityRevision 2151ef557a116cd45eb127d8205f902ae8d335a6;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree;
           rama architecture/interop-analisis-estructural; worktree = el directorio de trabajo actual;
           Owner subagent:R20261013T231726Z-7d47
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase): Proposal V3.1 congelada, docs/initiatives/I-65-f4-implementation.md (secciones 6 y 11), el paquete de preparacion,
           decisiones de I-65 (§26, §29, §37 y §41), ordenes literales del Owner de F4 y de la reanudacion, conformidades del Architect de F1 y F3, ADR-0050 y las demas; vigencia:
           AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea descrita en el contrato
           de gate artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/5/R20261013T231726Z-7d47/gate-contract.json. Esta
           delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (comando minimo de D-13, fallo cerrado y visible, perfil solo de la tabla, prohibiciones
           de D-13 e INV-12 con su guarda, distribucion de la tabla en la salida de compilacion, ediciones exactas de cuatro pruebas
           existentes, compilacion, forma del cambio y restricciones del Owner, regla de censos frente a I-64 y controles
           operativos del Owner); una delegacion nunca amplia el contrato
Rutas y archivos calientes (solo lectura para el Worker salvo lo que permite el contrato):
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralExport.cs (entrada de F4-T2 con decodificador inyectado);
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (RamSectionMapReader, CreateRamBuildPackage y la carpeta
           embebida del paquete); src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs (StructuralPackagePublisher);
           los comandos existentes del Plugin y sus ayudantes de seleccion y del almacen de proyecto; StructuralSectionCatalogAccess;
           src/RackCad.Plugin/RackCad.Plugin.csproj; tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs,
           tests/RackCad.Tests/SelectiveEditorOpenTests.cs y las dos guardas de G1 en tests/RackCad.Tests/StructuralAnalysis/
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

DELTA DE LA TAREA (F4, delegacion de CORRECCION de la cadena F4-T5-PLUGIN-COMMAND)
- Autorizacion: orden del Owner «I-65 — OWNER AUTHORIZATION: F4 Controlled Resumption — Final GO» (decisiones I-65 §41, OD-GO:
  IMPLEMENTATION AUTHORIZATION = F4-T6 + F4-T5 ONLY; entorno original con el riesgo residual aceptado, OD-RET; sin corridas en RAM
  Elements), con la concrecion de docs/initiatives/I-65-f4-implementation.md secciones 6 y 11, en AuthorityRevision; una sola cadena
  RED -> GREEN; Architect no exigido en la tarea (revisa al terminar las dos cadenas).
- Hechos de la cadena: primera delegacion R20261011T215537Z-f944 (STOP C-08/C-05 sin commits); correccion del intento 4
  R20261013T224954Z-16f8 (contrato emitido contra 2151ef55, el MISMO que se copia en el directorio de esta planificacion) y trabajo
  R20261013T225238Z-960c: el Worker se detuvo con STOP C-09 tras su commit RED 3e1ef603 (con push; sin GREEN) porque una muestra de su
  propio archivo de pruebas nuevo tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs tiene una linea
  que empieza por «namespace» en la columna 0 y hace fallar NamespaceFolderGuardTests. El commit de sesion af04ddb539f5bfd15cf057d0bad336cba9315f30 registra la resolucion
  (decisiones I-65 §43: CD-I65-F4-39 y DEV-I65-F4-20), su analisis en
  docs/automation/evidence/I-65-pilot/F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/analysis.md (SHA-256 bb18fede5a6fe6dcc911646a65c2d01d87cd51722352ca45c33ae68a5f9cc20a) y attempts = 5.
  El analisis se cita SOLO por CorrectionOf (no es autoridad). El RED 3e1ef603 nunca se acredito (no hubo verificacion).
- RunId = R20261013T231726Z-7d47 (tambien es el DelegationRunId); Attempt = 5; AttemptsRemaining = 1; MaxReworkLoops = 3 (attempts es por
  unidad y no se reinicia por tarea; max_attempts 6 en el contrato de la iniciativa por decision del Owner).
- CorrectionOf = { RunId: "R20261013T225238Z-960c", FailureClass: "StopCondition", AnalysisSha256: "bb18fede5a6fe6dcc911646a65c2d01d87cd51722352ca45c33ae68a5f9cc20a" }. Copia ese
  AnalysisSha256 caracter a caracter (64 hexadecimales en minusculas, ya en el formato del esquema).
- Unit = "I-65"; Initiative = "I-65"; Gate = "F4"; TaskId = "F4-T5-PLUGIN-COMMAND" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = 283ed6137a8669012369abb6e4a8543c27d34f1b (BaseSha de la primera delegacion de la cadena); ChainRedSha = null y
  ChainRedFiles = [] (la cadena no acredito ningun RED: el RED 3e1ef603 del intento 4 no se verifico), asi que esta entrega EXIGE RED.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`; si no
  coincide, explicalo en RoutingReason como S-13); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/5/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261013T231726Z-7d47" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato con
  capacidad write-commit-push, copiados del archivo SIN cambios (Executor.Transport es exactamente "subagente de la sesion de
  escritorio", sin tilde: una planificacion anterior fue rechazada por anadirla, CD-I65-F3-11); Effort.Provider = uno de los
  TestedEfforts de esa celda; Effort.Semantic = el de routing.md para la clase elegida; PromptProfile = el de esa clase. La
  tarea es un comando de linea del Plugin con su guarda, una edicion del .csproj y cuatro ediciones exactas de pruebas existentes,
  con compilacion local del Plugin contra AutoCAD 2025 sin ejecutarlo; cabe en una invocacion del Worker (tope de 60 min).
- SEIS COLECCIONES: PROYECCION MECANICA DEL CONTRATO. Lee el archivo
  artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/5/R20261013T231726Z-7d47/gate-contract.json y proyecta, sin reconstruir,
  resumir, corregir ortografia, anadir tildes, estrechar ni copiar desde prosa:
    delegation.Authorities         = contract.Authorities
    delegation.AllowedWriteScope   = contract.AllowedWriteScope
    delegation.ForbiddenWriteScope = contract.ForbiddenWriteScope
    delegation.Invariants          = contract.Invariants
    delegation.RequiredTests       = contract.RequiredTests
    delegation.StopConditions      = contract.StopConditions
  Para ver los valores exactos, ejecuta por ejemplo:
    pwsh -NoProfile -Command "$c = Get-Content -Raw -Encoding utf8 artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/5/R20261013T231726Z-7d47/gate-contract.json | ConvertFrom-Json; $c.Invariants | ConvertTo-Json -Depth 6; $c.Authorities | ConvertTo-Json -Depth 6; $c.StopConditions | ConvertTo-Json -Depth 6"
  OJO: copia cada cadena del contrato byte a byte, con las tildes y simbolos EXACTAMENTE donde los tiene el archivo: los
  Invariants y los textos de StopConditions estan SIN tildes (p. ej. «minimo», «publica», «unico», «canonica», «parentesis»,
  «indice», «estacion», «catalogo», «diagnosticos», «excepcion», «construccion», «edicion», «menus», «conversion», «diseno»,
  «distribucion», «compilacion», «solucion», «advertencias», «interfaz grafica»), y algunas Section SI llevan tildes o simbolos
  (p. ej. «## 16. Ejecución delegada bajo orden del Coordinator», «## G. Delegación de ejecución», «archivo completo (en especial
  §26, §29, §37 y §41)», «archivo completo (en especial §0, §1, §3, §6 y §11)», «archivo completo (en especial §5 y §6)» y la
  Section de la Proposal con §3). Dos Invariants nuevos contienen corchetes, barras y rutas (p. ej. «[Environment]::CurrentDirectory»,
  «C:/Users/alejandra-mendoza/.codex/worktrees/...», «D:/Documentos/Codex/Calculadora de racks»): copialos exactos. No las «corrijas» en ningun sentido. ATENCION: escribe el simbolo de seccion tal cual (el caracter U+00A7),
  nunca como secuencia de escape (en la planificacion R20261011T205717Z-064f salio corrompido en un caracter de control), y en el
  preflight compara cada Section caracter a caracter con el archivo. Ningun valor del contrato contiene barras invertidas: no las introduzcas.
- PREFLIGHT OBLIGATORIO antes de terminar: compara cada una de las seis colecciones de tu salida con el contrato leido del
  archivo, elemento a elemento y en orden; en Authorities, igualdad exacta de Path, Section y Class; en Invariants, igualdad
  exacta de cada cadena. Si algo difiere, corrige tu salida antes de terminar; nunca entregues una delegacion que sabes
  invalida. Registra en RoutingReason que el preflight dio igualdad en las seis.
- Archivos (van en Objective y AcceptanceCriteria, no en las colecciones de alcance): el archivo NUEVO del Plugin
  src/RackCad.Plugin/RackRamElementsCommands.cs; la edicion de src/RackCad.Plugin/RackCad.Plugin.csproj (un elemento Content); las
  ediciones exactas de tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs, tests/RackCad.Tests/SelectiveEditorOpenTests.cs,
  tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs y
  tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs; las clases de prueba nuevas, en archivos NUEVOS
  de tests/RackCad.Tests/StructuralAnalysis/, en namespace RackCad.Tests y con nombre que empieza por StructuralAnalysisPluginCommand.
- AcceptanceCriteria: concretos y comprobables, uno o mas por cada invariante del contrato tal como los definen
  docs/initiatives/I-65-f4-implementation.md seccion 6 y docs/initiatives/I-65-proposal-v3.1.md (D-08, D-12, D-13; §3 INV-01 e
  INV-12). Incluye:
  - RED de la correccion = solo la correccion de la muestra de StructuralAnalysisPluginCommandGuardTests.cs (ninguna linea del
    archivo empieza por «namespace» salvo su propia declaracion; las muestras la componen en tiempo de ejecucion), con la suite Core
    completa ANTES del commit RED (fallos admitidos: el filtro y las tres ediciones que dependen del archivo del comando);
  - RED compilable primero (la guarda nueva del comando y sus autopruebas, seleccion >= 12, mas las cuatro ediciones exactas), con
    push propio antes del GREEN para que su corrida push exista y su job Core falle;
  - el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en verde, incluidas F1, F3, T1-T4 y todas las
    guardas del Plugin; el Plugin compila localmente hacia artifacts/ y la solucion completa compila en Release sin advertencias nuevas
    atribuibles al diff (las preexistentes locales se listan aparte; la senal es el job Build Plugin without AutoCAD de la CI);
  - con la tabla del producto tal como se entrega, el comando no escribe nada (si hiciera falta tratar una fila proposed como
    verified: parada C-07);
  - ninguna prueba ejecuta AutoCAD, Python ni RAM Elements; el Worker no invoca un interprete Python por ningun motivo (C-08);
  - censos: de 37 a 38 solo con RACKRAMPAQUETE en una lista propia de I-65 (I-64 no integrada; CD-I65-F4-33);
  - el Worker informa su directorio efectivo al empezar y usa rutas absolutas (C-12);
  - los campos de texto libre de la entrega sin terminos de 16.10 (tampoco «candidato» ni «candidate»).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T5, CONTROLLER_PLANNING)".
