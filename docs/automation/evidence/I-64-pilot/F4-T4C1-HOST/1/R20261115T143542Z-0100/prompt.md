I-64 - DELEGACION F4-T4C1-HOST - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T4C1-HOST)
Identidad: unidad I-64; gate F4; tarea F4-T4C1-HOST; intento 1; RunId R20261115T143542Z-0100;
           DelegationRunId R20261115T143542Z-0100; AuthorityRevision 12eda7de2307950f64d4bbf532fc56a05365fca6 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 44eafbc18c73bab333085d668e84c19ab15fb208: commit de emision del contrato, hijo directo de la AuthorityRevision 12eda7de2307950f64d4bbf532fc56a05365fca6:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261115T143542Z-0100
Contrato de gate: artifacts/orchestration/I-64/F4-T4C1-HOST/1/R20261115T143542Z-0100/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4C1-HOST/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 18 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4c-gate-contract-package.md v5 (section 2, hechos H-01 a H-52, con lo que midio T4C-H0 en section 2.6; section 3, el reparto en H0, T4C1 y T4C2; section 4, decisiones D-T4C-01 a D-T4C-20, de las que esta tarea hace solo lo marcado [1] o [1, 2]: la operacion del puerto con el billete y su cota, el canal de finalizacion, el corredor puro con el objetivo opaco y el adaptador, la traduccion de los fallos del host, el ejecutor sin estado, el escritor de un solo uso, la revalidacion final, el ambito propio por la via A con IC-09, R-T4-07, los arrastres, la guarda sin segunda via, las pruebas y la contencion, T1B y el inventario de cambios de guarda; section 5, trazabilidad; section 6, contrato; section 7, Smoke-2; section 9, decisiones del Owner; section 10, revisiones), el resultado de la sonda docs/automation/evidence/I-64-pilot/F4-T4C-H0/h0-result.md, la aclaracion IC-09 de docs/initiatives/I-64-implementation-clarifications.md y las decisiones del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c1-emission-authorization.md, owner-f4t4c-package-decisions.md y owner-f4t4c-h0-c12-decision.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (11 archivos) ; git ls-files src/RackCad.Application/Workspace (40 archivos) ; git ls-files src/RackCad.Plugin/Views (13 archivos) ; git ls-files tests/RackCad.Tests/Workspace (72 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T4C-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T4C-C15 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 204 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E59 y F4-E61 a F4-E65 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3, F4-T4A, F4-T1B y F4-T4B estan cerradas, y la sonda de host T4C-H0 esta hecha (custodia 44fe029d). El Owner ratifico las desviaciones de H0, dispuso O-18 aparte y autorizo la emision y la ejecucion de F4-T4C1-HOST con el paquete v5 y su section 9 decidido: la integracion en el host de la escritura de T4B (contexto de comando solo en el camino de Actualizar, bloqueo del documento exacto, billete en vuelo con su cota, almacen del resultado, escritor de un solo uso, ambito de mutacion propia por la via A con IC-09 y R-T4-07), sin una segunda politica de mutacion, sin UI del panel (eso es F4-T4C2) y sin AutoCAD en las pruebas. Tras esta tarea, RequestUpdate no tiene gesto de producto (D-T4C-19). Su primera validacion fisica es Smoke-2.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Los siete archivos nuevos del AllowedWriteScope no existen; los veintitres modificados si. El producto y las pruebas del HEAD son los de b15072ff, el cierre de F4-T4B, cuyo producto es el de 4314c180 (la base medida del paquete; git diff b15072ff HEAD -- src tests vacio).
  * Cuentas de la base (CI push 43668817337 de 4314c180, H-31 del paquete): el filtro FullyQualifiedName~RackCad.Tests.Workspace selecciona 3103 (por prefijo del nombre de clase: los archivos de tests/RackCad.Tests/Workspace declaran namespace RackCad.Tests, y las clases nuevas empiezan por Workspace); los de UI, Workspace 601 y PushBackClassicCharacterization 55; y cada una de las 23 clases de regresion de Core la cuenta que da su fila de RequiredTests.
  * El RED (INV-F4T4C-20): pruebas nuevas y cambiadas que compilan contra un esqueleto inerte de los archivos y los miembros nuevos (cuerpos inertes tambien en las fabricas, los reenvios, el objetivo y las primitivas); las pruebas existentes que cambian van en el RED; fallan por asercion solo las nuevas o las cambiadas, y la entrega declara las que pasan ya en el RED. La CI del RED: el job de Core en failure (ExpectRed true en el filtro de Workspace) y el de UI en success (ExpectRed false; el doble de UI implementa las dos operaciones nuevas inertes); build-plugin depende de tests y queda omitido en el RED.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: RequestUpdate y TakeUpdateProgress con sus portadores, el billete con su testigo y su cota por compleccion o comando ajeno, y el almacen por clave (D-T4C-02 y D-T4C-03); el corredor puro PushBackPilotHostRun con IPushBackPilotHostTarget y su registro de llamadas terminales (D-T4C-04); el adaptador RackPilotUpdateCommandContext como la unica llamada a ExecuteInCommandContextAsync, con una sola OnCompleted sobre el ExecutionResult y el salto al planificador capturado (D-T4C-04); la tabla de traduccion de los fallos del host (D-T4C-05); el campo _updateHost asignado una vez por SetPilotUpdateHost (D-T4C-06); el escritor con el Document del bloqueo y de un solo uso (D-T4C-07, D-1); la revalidacion final (D-T4C-08); el ambito propio por la via A con el texto de IC-09 (D-T4C-09); R-T4-07 en el host (D-T4C-10); la frontera PilotBoundaryKind.Update al final del enum con su traduccion en CrossBoundary del nucleo; los arrastres I-01, M-01, M-03 en el tipo y M-06 (D-T4C-13); las guardas G-1 a G-7, G-9 y G-10 (D-T4C-14); las pruebas y los dobles (D-T4C-17); y las filas [1] del inventario de cambios de guarda (D-T4C-20).
  * Hechos de la sonda H0 (section 2.6 del paquete): ExecuteInCommandContextAsync devuelve DocumentCollection.ExecutionResult, no un Task (H-50); el callback es diferido y la llamada se completa siempre; el orden medido es dos EnteringQuiescentState y el CommandWillStart de EXECUTEFUNCTION antes del callback, y CommandEnded(EXECUTEFUNCTION) y EnteringQuiescentState despues (H-41, H-43); las entradas en reposo no cuentan para la cota del billete (decision del Owner sobre el C12 de H0).
  * Reglas de las corridas locales (F4-E59, INV-F4T4C-18): nunca la suite completa de UI en local, solo sus dos filtros de RequiredTests; ninguna sonda con entradas validas en caminos con dialogo o escritura; ninguna prueba nueva escribe fuera de una ubicacion temporal aislada.
  * Trampas medidas (section 2 y section 6 del paquete): HOSTRUN no contiene la palabra Transaction, ni en un comentario (H-06: ningun archivo de la carpeta del piloto la contiene); ningun tipo nuevo de RackCad.Application.Workspace, anidados incluidos, se llama con Subscription, Editor, Draft, Tab, ProjectSummary, Metric, Provider, TotalRacks, Aggregat ni Bom (H-20); el cuerpo de SessionState no contiene Close, Allowed ni SessionId? (H-17); los casos de reposo de EventBridgeCore estan fijados literalmente y el cierre del ambito va en el cuerpo de SetQuiescent, antes de su comparacion (H-17, fila 17 de D-T4C-20); la guarda de TW/WorkspacePilotPrepareGuardTests.cs busca literalmente el bloque diferido de DraftSession.cs (C11); CT-27 de T1B congela las firmas de las fachadas de Push Back.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 30 entradas del contrato, en su orden (30 archivos exactos, sin prefijos):
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs
    src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
    src/RackCad.Application/Workspace/WorkspacePilotSupport.cs
    src/RackCad.Application/Workspace/IWorkspaceBridgeLink.cs
    src/RackCad.Application/Workspace/EventBridgeCore.cs
    src/RackCad.Application/Workspace/WorkspaceSession.cs
    src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostRunTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceOwnMutationTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostDouble.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4C1-HOST/1/R20261115T143542Z-0100/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/1/R20261115T143542Z-0100/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/1/R20261115T143542Z-0100/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4c-gate-contract-package.md" (f4-t4c: el paquete de esta tarea); Authorities[6].Path = "docs/initiatives/I-64-f4-t4b-gate-contract-package.md" (f4-t4b); Authorities[8].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md" (f4-t4); Authorities[9].Path = "docs/initiatives/I-64-f4-gate-contract-package.md" (f4). No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/1/R20261115T143542Z-0100/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 163 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/1/R20261115T143542Z-0100/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 162, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 26 elementos, en el mismo orden y con los mismos valores (ExpectRed true solo en el primero, FullyQualifiedName~RackCad.Tests.Workspace; false en los otros 25).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: Application (la operacion del puerto, el nucleo, el corredor del host, el puente y la sesion), el Plugin (el adaptador nuevo de contexto de comando, el escritor, el controlador y el puente de AutoCAD) y pruebas de Core con dobles de las primitivas, del ejecutor y del ambito. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T4C1-HOST)
- RunId = R20261115T143542Z-0100 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4C1-HOST".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4C1-HOST/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261115T143542Z-0100" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el RED con pruebas que compilan contra el esqueleto inerte y solo las nuevas o cambiadas en rojo por asercion, con las que pasan en el RED declaradas; el GREEN dentro del AllowedWriteScope sin tocar tests/ del RED; TryUpdatePilot llamado solo desde el Execute de la corrida anidada del nucleo, una vez por corrida admitida; el billete con su testigo, su liberacion por la compleccion y su caducidad por comando ajeno, cierre o retirada, sin contar las entradas en reposo; exactamente una llamada terminal por billete empezado; Busy para un segundo pedido; el almacen por clave y su consumo; el adaptador como la unica llamada a ExecuteInCommandContextAsync con una sola OnCompleted y sin Task, async ni await fuera de el; el bloqueo del documento exacto en el orden fijado y el escritor de un solo uso; los fallos del host por un solo canal, sin traducir Unknown ni reclasificar; la revalidacion final; el ambito propio segun IC-09 con su residual; R-T4-07 con DraftRegistry sin cambios; la frontera Update en el nucleo; I-01, M-01, M-03 y M-06; las guardas G-1 a G-7, G-9 y G-10 con sus negativas; las 20 pruebas de la orden y las de la revision (D-T4C-17); los cambios de guarda de las filas [1] de D-T4C-20, exactos y con su autoprueba en negativo, listados frente al inventario; los caminos clasicos y la costura de I-55 sin cambios; las pruebas de T1B y las 23 clases de regresion en verde; la contencion de las corridas locales; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4-T4C1-HOST, primera delegacion de la cadena, CONTROLLER_PLANNING)".
