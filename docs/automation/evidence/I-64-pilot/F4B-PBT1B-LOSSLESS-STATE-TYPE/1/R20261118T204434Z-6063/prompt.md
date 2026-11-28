I-64 - DELEGACION F4B-PBT1B-LOSSLESS-STATE-TYPE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4B-PBT1B-LOSSLESS-STATE-TYPE)
Identidad: unidad I-64; gate F4B; tarea F4B-PBT1B-LOSSLESS-STATE-TYPE; intento 1; RunId R20261118T204434Z-6063;
           DelegationRunId R20261118T204434Z-6063; AuthorityRevision 5e52aaa34037d8cc6b46849c89d1fce946187324 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 4b0498e3e05f65813c5e6274b3beb51217587d65: commit de emision del contrato, hijo directo de la AuthorityRevision 5e52aaa34037d8cc6b46849c89d1fce946187324:
           solo anade el contrato, su script generador, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261118T204434Z-6063
Contrato de gate: artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204434Z-6063/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md v2.7 (section 2, hechos medidos, con los anclajes de PB-T1b sobre el cierre de PB-T1a 5043b49a; section 3, D-PBT1-02 a D-PBT1-11 con D-PBT1-09 (c-02) y D-PBT1-09b (c-09), D-PBT1-14 y D-PBT1-15 con la decision D2 del Owner, sus diez condiciones, la guarda G-L1 y su limite; section 5.0, las recetas y el ayudante Pasada; section 5.3, el regimen de PB-T1b, el esqueleto exacto, las clases y la lista cerrada CT-PBT1B-01 a CT-PBT1B-17; section 6.0 y 6.2, el contrato; section 7, riesgos; section 8, las decisiones del Owner; section 10, los arrastres), con su registro de mediciones docs/initiatives/I-64-f4b-pbt1-measurements.md (apartado P), las decisiones literales del Owner (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md, owner-d1-d2-decisions.md y owner-pbt1a-decisions.md) y la red de PB-T1a (tests/RackCad.UI.Tests/PushBackStateNet*.cs) como oraculo de la ventana
Criterios de aceptacion: la salida valida contra el esquema; las ocho colecciones opacas del contrato (Authorities, Objective, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions) VACIAS, porque las pone la sesion (F4B-E21);
           celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack (64 archivos) ; git ls-files tests/RackCad.UI.Tests (210 archivos) ; git ls-files tests/RackCad.Tests (707 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las PBT1B-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COLECCIONES OPACAS (F4B-E21, decision del Owner del 2026-11-18): el contrato de esta tarea ya no cabe en tu salida. NO copies las ocho colecciones opacas del contrato (Authorities, Objective, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions): emitelas VACIAS (listas vacias y Objective como cadena vacia). La sesion las pone despues, byte a byte desde el contrato canonico, y compara la delegacion final con el. Tu trabajo es el resto: identidad, TaskClass, Dimensions, Executor, Effort, RoutingReason y AcceptanceCriteria.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" en el texto libre, ni en nombres de archivo, rutas o artefactos
Trailer: no aplica (no hay commits)
Informe esperado: la salida exacta del esquema rackcad-delegation/v1 (la escribe el CLI por -o)

PERFIL CONTROLLER_PLANNING
Metodo: clasificar y enrutar dentro del contrato de gate, sin escribir en Git.
1. Lee el contrato, docs/automation/agent-execution/routing.md y el catalogo.
2. Clasifica la tarea, puntua las siete dimensiones y elige entre las celdas del
   contrato el nivel mas bajo adecuado; registra RoutingReason con fechas.
3. Copia del contrato el alcance, los invariantes, las pruebas y las paradas; nunca
   los amplies.
4. Rellena el esquema de delegacion completo; null solo donde el esquema lo admite.

CONTEXTO (hechos versionados en el HEAD; evidencia section 231 y siguientes de docs/automation/evidence/I-64-evidence.md; F4B-E01 a F4B-E17 en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md)
- F4 esta cerrada. En F4B, PB-T0, PB-TC, PB-TC2 y PB-T1a estan CLOSED / PASS WITH LIMITATIONS (F4B-E05, F4B-E08, F4B-E12 y F4B-E16). PB-T1 (sacar de la ventana WPF el estado editable de Push Back sin perdidas) se divide en tres subtareas secuenciales (D-PBT1-01); esta es la segunda, PB-T1b: crear en Application PushBackEditableState, su comparador, su mapa de propiedad y PushBackEditorOutputGate, con las reglas explicitas de c-02 y de c-09 (F4B-E09 y F4B-E14), la copia con la excepcion de dos referencias compartidas de D2 y sus guardas. La ventana NO cambia: eso es de PB-T1c. La decision D2 del Owner (F4B-E11) y su limite van como la parada PBT1B-C17. Emitida por F4B-E17.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; un intento 3 vuelve siempre al Owner (PBT1B-C12).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Los filtros con ExpectRed true (FullyQualifiedName~RackCad.Tests.PushBackEditableState, FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate y FullyQualifiedName~RackCad.UI.Tests.PushBackEditableStateParityTests) seleccionan 0 pruebas en el HEAD. Las cinco clases de Core van en namespace RackCad.Tests y la de UI en namespace RackCad.UI.Tests, en EditorDiscardPromptCollection.
  * El RED es el ESQUELETO INERTE (regimen de F4-T2A; section 5.3): el esqueleto exacto de la section 5.3 en los archivos de src/ del AllowedWriteScope (los cuatro nuevos de Application y los miembros nuevos de ES y RMES) MAS las seis clases de prueba; cada metodo de un filtro con ExpectRed falla por asercion sobre el esqueleto, salvo las dos guardas de D2 (CT-PBT1B-14 y 15, filtro PushBackSharedReferenceGuardTests, ExpectRed false), que pasan en el RED y discriminan por sus autopruebas en negativo. El GREEN implementa solo src/ del AllowedWriteScope y no toca tests/. La ventana, el piloto, el Plugin, CS y el XAML no cambian.
  * Antes del RED (paso 1, INV-PBT1B-06): una corrida TEMPORAL de UI sobre la BaseSha, fuera de todo commit, con Assert.Fail, mide las filas (a) a (e) de c-02 y la fila (f) de c-09; las filas (b) y (f) fijan la rama de cada regla; cualquier otra linea es PBT1B-C14 (STOP antes del RED).
  * La CI corre las suites completas en Debug y compila el Plugin en Release. En el RED se espera tests y ui-tests en failure, build-ui en success y build-plugin omitido por su needs; ninguna prueba existente falla en el RED. GREEN previsto: Core 19 086 y UI 2 437 (section 6.2).
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la lista cerrada CT-PBT1B-01 a 17 con su reparto en las clases; el esqueleto exacto; la API exacta de D-PBT1-02 a D-PBT1-11 con D-PBT1-09b (INV-PBT1B-03); las reglas y las autopruebas de G-L1 de D-PBT1-15; las lineas literales de CT-PBT1B-12, 15 y 17; el Worker no ajusta lo esperado (PBT1B-C07).
  * D2 (F4B-E11): solo la linea base resuelta y el informe de modulos de cada lado se comparten por referencia (las diez condiciones de D-PBT1-15); si hiciera falta mutarlos dentro de la vida de la edicion, o compartir otro objeto mutable: PBT1B-C17, STOP y se reabre L-1.
  * Sin AutoCAD.

ALCANCE DE ESCRITURA
- AllowedWriteScope lo pone la sesion desde el contrato (F4B-E21): emitelo vacio. Leelo en el contrato para redactar los AcceptanceCriteria.

LECTURA DEL CONTRATO (sin copiarlo)
- Lee el contrato canonico artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204434Z-6063/gate-contract.json (ASCII puro) con ordenes que impriman solo lo que necesitas (por ejemplo, los invariantes y las paradas numerados) y redacta los AcceptanceCriteria trazables a sus invariantes, sus RequiredTests y sus paradas. No reproduzcas en tu salida ninguna de las ocho colecciones opacas.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: un tipo nuevo de Application con su copiador, su comparador, su mapa y su barrera de salida, pruebas nuevas en Core y en UI, un esqueleto inerte en el RED y una medicion temporal previa (paso 1). Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4B-PBT1B-LOSSLESS-STATE-TYPE)
- RunId = R20261118T204434Z-6063 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4B"; TaskId = "F4B-PBT1B-LOSSLESS-STATE-TYPE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261118T204434Z-6063" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el paso 1 con las filas (a) a (f) y las dos ramas declaradas (PBT1B-C14); el RED con el esqueleto exacto de la section 5.3 y las seis clases, con el criterio mecanico de INV-PBT1B-02 en cada filtro con ExpectRed y las dos guardas de D2 en verde con sus autopruebas en negativo; el GREEN solo en src/ del AllowedWriteScope con la API exacta (INV-PBT1B-03) y sin campos nuevos (INV-PBT1B-04); cada CT de CT-PBT1B-01 a 17 con su metodo en su clase y sus lineas esperadas sin ajustarlas (PBT1B-C07); D2 (INV-PBT1B-07c y PBT1B-C17); PBT1B-C15 si la paridad o la ida y vuelta dan una diferencia que no sale del esqueleto; la contencion de las corridas locales y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4B-PBT1B-LOSSLESS-STATE-TYPE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
