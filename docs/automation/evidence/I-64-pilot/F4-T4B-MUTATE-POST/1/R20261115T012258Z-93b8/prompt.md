I-64 - DELEGACION F4-T4B-MUTATE-POST - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T4B-MUTATE-POST)
Identidad: unidad I-64; gate F4; tarea F4-T4B-MUTATE-POST; intento 1; RunId R20261115T012258Z-93b8;
           DelegationRunId R20261115T012258Z-93b8; AuthorityRevision 56041cd0cce9bc20ab70b4192aa2f48080f4a8a0 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 3633f08d1082a8a049bca1ac24432900919253d3: commit de emision del contrato, hijo directo de la AuthorityRevision 56041cd0cce9bc20ab70b4192aa2f48080f4a8a0:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261115T012258Z-93b8
Contrato de gate: artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012258Z-93b8/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 18 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4b-gate-contract-package.md v7 (section 2, hechos H-01 a H-26; section 3, el reparto con T4C; section 4, decisiones D-T4B-02 a D-T4B-09: el puerto por ambitos y la corrida pura y total con su tabla de frontera, el escritor del Plugin, la lectura caller-owned y el movimiento de Traverse con guardas exactas, el comparador de POST, CompleteApply por rama, TryUpdatePilot, los arrastres y las guardas; section 5, trazabilidad; section 6, contrato; section 7, Smoke-2; section 9, interpretaciones aprobadas; section 10, revisiones) y las decisiones del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4b-emission-authorization.md y owner-f4t4b-traverse-decision.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (8 archivos) ; git ls-files src/RackCad.Application/Workspace (40 archivos) ; git ls-files src/RackCad.Plugin/Views (12 archivos) ; git ls-files tests/RackCad.Tests/Workspace (68 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T4B-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T4B-C07 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 201 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E59 y F4-E60 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3, F4-T4A y F4-T1B estan cerradas. El Owner autorizo la emision y la ejecucion de F4-T4B-MUTATE-POST con el paquete v7 y su section 9 aprobado: MUTATE y POST del Actualizar del piloto de Push Back por la via propia del piloto (no la costura de I-55), la primera escritura authored del piloto, sin contexto de comando ni LockDocument en el escritor (eso es de F4-T4C) y sin AutoCAD en las pruebas. Su primera validacion fisica es Smoke-2.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Los ocho archivos nuevos del AllowedWriteScope no existen; los quince modificados si. El producto del HEAD es el de c3ae1c97 (la base medida del paquete; git diff c3ae1c97 HEAD -- src vacio).
  * Cuentas de la base (CI push 43650040527): el filtro FullyQualifiedName~RackCad.Tests.Workspace selecciona 2923 (por prefijo del nombre de clase: los archivos de tests/RackCad.Tests/Workspace declaran namespace RackCad.Tests); los de UI, Workspace 601 y PushBackClassicCharacterization 55; y cada una de las 21 clases de regresion de Core la cuenta que da su fila de RequiredTests.
  * El RED: pruebas nuevas y cambiadas que compilan contra un esqueleto inerte de los archivos nuevos (cuerpos inertes tambien en fabricas, igualdades y reenvios); fallan por asercion solo las nuevas o las cambiadas, salvo las guardas que el paquete declara que pasan ya en el RED (D-T4B-04 (8)). El movimiento del nucleo de Traverse va en el GREEN. La CI del RED: el job de Core en failure (ExpectRed true en el filtro de Workspace) y el de UI en success (ExpectRed false); build-plugin depende de tests y queda omitido en el RED.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: las operaciones y los tipos de valor del puerto (D-T4B-02, con PushBackPilotPostRequest y PushBackPilotPostValues); la tabla de frontera total con las filas del aborto y la precedencia; el escritor que solo hace operaciones primitivas y devuelve hechos (D-T4B-03, con el orden de Post y la regla por vista de DesiredNames); el nucleo internal static TraverseCore y sus guardas exactas (D-T4B-04 (1) a (8)); la purga de categoria b (INV-F4T4B-16); CompleteApply por rama (D-T4B-06); TryUpdatePilot y la ranura (D-T4B-07); los cambios de guarda declarados (D-T4B-07 y D-T4B-09).
  * Reglas de las corridas locales (F4-E59, INV-F4T4B-18): nunca la suite completa de UI en local, solo sus dos filtros de RequiredTests; ninguna sonda con entradas validas en caminos con dialogo o escritura; ninguna prueba nueva escribe fuera de una ubicacion temporal aislada.
  * Trampas medidas (section 2 del paquete): las guardas de pureza de Pilot/ y del host prohiben la subcadena Transaction tambien en comentarios e identificadores (por eso ForeignScopeOpenAtPost y ForeignScopeOpenCause); la guarda de TW/WorkspacePilotPrepareGuardTests.cs:303-306 busca literalmente el bloque diferido de DraftSession.cs; ningun campo del nucleo puede ser de un tipo de RackCad.Application.Systems; CT-27 de T1B congela las firmas de las fachadas de Push Back.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 23 entradas del contrato, en su orden (23 archivos exactos, sin prefijos):
    src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs
    src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
    src/RackCad.Application/Workspace/Drafts/DraftSession.cs
    src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.Plugin/Views/RackSiblingScan.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs
    tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs
  Nunca anadas una ruta fuera del contrato ni un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
Do not add or remove spaces.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012258Z-93b8/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012258Z-93b8/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012258Z-93b8/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4b-gate-contract-package.md" (f4-t4b: el paquete de esta tarea); Authorities[7].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md" (f4-t4); Authorities[8].Path = "docs/initiatives/I-64-f4-t4a2-gate-contract-package.md" (f4-t4a2). No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012258Z-93b8/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 70 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012258Z-93b8/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 69, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 24 elementos, en el mismo orden y con los mismos valores (ExpectRed true solo en el primero, FullyQualifiedName~RackCad.Tests.Workspace; false en los otros 23).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: Application (puerto, corrida pura, ayuda, borrador y host), el Plugin (el escritor nuevo y el movimiento del nucleo de Traverse) y pruebas de Core con un doble del ambito. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T4B-MUTATE-POST)
- RunId = R20261115T012258Z-93b8 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4B-MUTATE-POST".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261115T012258Z-93b8" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el RED con pruebas que compilan contra el esqueleto inerte y solo las nuevas o cambiadas en rojo por asercion, con las guardas que pasan en el RED declaradas; el GREEN dentro del AllowedWriteScope sin tocar tests/ del RED; la corrida total con su tabla de frontera, las filas del aborto y la precedencia; la relectura de MUTATE como autoridad y cero WriteUnit ante una diferencia; la transaccion poseida y el rechazo TOP_TRANSACTION_OPEN como valor; el escritor que solo hace operaciones primitivas y devuelve hechos; POST por la relectura posterior a la purga, comparado sin normalizar; la purga de categoria b con su orden; el movimiento de Traverse con sus guardas exactas y autopruebas negativas; CompleteApply por rama con una prueba por fila; los caminos clasicos y la costura de I-55 sin cambios; las pruebas de T1B y las 21 clases de regresion en verde; la contencion de las corridas locales; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4-T4B-MUTATE-POST, primera delegacion de la cadena, CONTROLLER_PLANNING)".
