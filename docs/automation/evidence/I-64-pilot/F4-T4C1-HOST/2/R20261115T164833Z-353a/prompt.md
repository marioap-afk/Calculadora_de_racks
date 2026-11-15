I-64 - DELEGACION F4-T4C1-HOST - Controller, perfil CONTROLLER_PLANNING (CORRECCION, intento 2 de 3, de la cadena F4-T4C1-HOST; F4-E66; contrato gate-contract-attempt2.json)
Identidad: unidad I-64; gate F4; tarea F4-T4C1-HOST; intento 2; RunId R20261115T164833Z-353a;
           DelegationRunId R20261115T164833Z-353a; AuthorityRevision cd902cd9d46d92dcd959d6bcbb2901364167d262 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 12c248b039674b5e8589f10f51e6dd1150f99b12: commit de emision del contrato del intento 2, hijo directo de la AuthorityRevision cd902cd9d46d92dcd959d6bcbb2901364167d262:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261115T164833Z-353a
Contrato de gate: artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4C1-HOST/coordinator/gate-contract-attempt2.json; sustituye al del intento 1)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 20 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4c-gate-contract-package.md v5 (section 2, hechos H-01 a H-52, con lo que midio T4C-H0 en section 2.6; section 3, el reparto en H0, T4C1 y T4C2; section 4, decisiones D-T4C-01 a D-T4C-20, de las que esta tarea hace solo lo marcado [1] o [1, 2]: la operacion del puerto con el billete y su cota, el canal de finalizacion, el corredor puro con el objetivo opaco y el adaptador, la traduccion de los fallos del host, el ejecutor sin estado, el escritor de un solo uso, la revalidacion final, el ambito propio por la via A con IC-09, R-T4-07, los arrastres, la guarda sin segunda via, las pruebas y la contencion, T1B y el inventario de cambios de guarda; section 5, trazabilidad; section 6, contrato; section 7, Smoke-2; section 9, decisiones del Owner; section 10, revisiones), el resultado de la sonda docs/automation/evidence/I-64-pilot/F4-T4C-H0/h0-result.md, la aclaracion IC-09 de docs/initiatives/I-64-implementation-clarifications.md y las decisiones del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c1-emission-authorization.md, owner-f4t4c-package-decisions.md y owner-f4t4c-h0-c12-decision.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (12 archivos) ; git ls-files src/RackCad.Application/Workspace (40 archivos) ; git ls-files src/RackCad.Plugin/Views (14 archivos) ; git ls-files tests/RackCad.Tests/Workspace (77 archivos) ;
           git diff --name-only 44eafbc18c73bab333085d668e84c19ab15fb208 8e57061e62aa847eecab0b9653c8f9f008c434a2 -- tests/ (las 17 rutas del RED del intento 1) ;
           y lee el codigo, las pruebas y las guardas que cites
Fuente de la correccion: docs/automation/evidence/I-64-pilot/F4-T4C1-HOST/1/R20261115T163412Z-eca1/analysis.md (hallazgos de las revisiones del intento 1 y su disposicion), su independent-review.md y su direct-review.md (con archivos y lineas), F4-E66 y las decisiones del Owner F4-DRAFTS/coordinator/owner-f4t4c-decisions-d1-d6.md (D2 a D4).
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T4C-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
ANTECEDENTE DE ESTA TAREA: en el intento 1, la primera planificacion (R20261115T143542Z-0100) se RECHAZO solo por ForbiddenWriteScope: emitio 216 entradas en vez de 163 (omitio la entrada 43, tests/RackCad.Tests/RackViewBatchDriverGuardTests.cs, y al final repitio en bucle los archivos de src/RackCad.Application/Systems/PushBack/); la replanificacion reforzada, con la orden de copia de abajo, salio exacta. Esta delegacion de correccion tiene su PROPIA replanificacion reforzada unica (decision D4 del Owner): un segundo fallo de fidelidad de esta delegacion va con STOP al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 204 y siguientes de docs/automation/evidence/I-64-evidence.md, en particular 208 a 211; F4-E59 y F4-E61 a F4-E66 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3, F4-T4A, F4-T1B y F4-T4B estan cerradas, y la sonda de host T4C-H0 esta hecha (custodia 44fe029d). El Owner ratifico las desviaciones de H0, dispuso O-18 aparte y autorizo la emision y la ejecucion de F4-T4C1-HOST con el paquete v5 y su section 9 decidido: la integracion en el host de la escritura de T4B (contexto de comando solo en el camino de Actualizar, bloqueo del documento exacto, billete en vuelo con su cota, almacen del resultado, escritor de un solo uso, ambito de mutacion propia por la via A con IC-09 y R-T4-07), sin una segunda politica de mutacion, sin UI del panel (eso es F4-T4C2) y sin AutoCAD en las pruebas. Tras esta tarea, RequestUpdate no tiene gesto de producto (D-T4C-19). Su primera validacion fisica es Smoke-2.
- CORRECCION de F4-T4C1-HOST (intento 2): la entrega del intento 1 (RED 8e57061e, GREEN 271a045c) dio EXECUTION_VERIFIED 14/14 en R20261115T163412Z-eca1, pero la revision adversarial independiente dio REWORK (1/1/10): B-01, un billete que paso por Execute no caduca nunca porque la marca en Execute no se retira, de modo que si la compleccion no llega la sesion queda en Busy sin cota (contra D-T4C-02 y D-T4C-04); I-01, las guardas G-1 y G-10 acotadas por carpeta sin la necesidad que alego la entrega; y diez menores, mas los avisos xUnit2029 que registro la verificacion. El contrato del intento 2 conserva el alcance de 30 rutas, la lista prohibida, los 18 invariantes, los RequiredTests y las paradas, y anade INV-F4T4C-21 (las correcciones) e INV-F4T4C-22 (la mecanica de la correccion).
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; el intento 3 no se gasta sin el Owner (D3).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Las 30 rutas del AllowedWriteScope existen (el intento 1 creo las siete nuevas). El producto y las pruebas del HEAD son los de 271a045c, el GREEN del intento 1 (git diff 271a045c HEAD -- src tests vacio; desde entonces solo documentacion).
  * Lineas base de la CI de 271a045c (corrida 43733257361; mismo producto que el HEAD): FullyQualifiedName~RackCad.Tests.Workspace = 3385 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 601, todas en verde, y la suite completa de Core 18966/18966 (los minimos del contrato siguen siendo 3103 y 601). Cada una de las 23 clases de regresion de Core tiene la cuenta que da su fila de RequiredTests.
  * El RED de la correccion (INV-F4T4C-22): sobre el producto de 271a045c, sin esqueleto, un commit RED con las pruebas nuevas o corregidas; fallan por asercion contra ese producto al menos las dos pruebas nuevas de B-01, la guarda corregida de M-01 y las pruebas de M-06, y las demas pruebas nuevas o corregidas pueden pasar si la entrega lo declara. La CI del RED: el job de Core en failure (ExpectRed true en el filtro de Workspace) y el de UI en success (ExpectRed false); build-plugin depende de tests y queda omitido en el RED. Desde el RED, el GREEN no toca tests/.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: RequestUpdate y TakeUpdateProgress con sus portadores, el billete con su testigo y su cota por compleccion o comando ajeno, y el almacen por clave (D-T4C-02 y D-T4C-03); el corredor puro PushBackPilotHostRun con IPushBackPilotHostTarget y su registro de llamadas terminales (D-T4C-04); el adaptador RackPilotUpdateCommandContext como la unica llamada a ExecuteInCommandContextAsync, con una sola OnCompleted sobre el ExecutionResult y el salto al planificador capturado (D-T4C-04); la tabla de traduccion de los fallos del host (D-T4C-05); el campo _updateHost asignado una vez por SetPilotUpdateHost (D-T4C-06); el escritor con el Document del bloqueo y de un solo uso (D-T4C-07, D-1); la revalidacion final (D-T4C-08); el ambito propio por la via A con el texto de IC-09 (D-T4C-09); R-T4-07 en el host (D-T4C-10); la frontera PilotBoundaryKind.Update al final del enum con su traduccion en CrossBoundary del nucleo; los arrastres I-01, M-01, M-03 en el tipo y M-06 (D-T4C-13); las guardas G-1 a G-7, G-9 y G-10 (D-T4C-14); las pruebas y los dobles (D-T4C-17); y las filas [1] del inventario de cambios de guarda (D-T4C-20); y las correcciones de INV-F4T4C-21, que siguen el analysis.md de la correccion.
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4c-gate-contract-package.md" (f4-t4c: el paquete de esta tarea); Authorities[6].Path = "docs/initiatives/I-64-f4-t4b-gate-contract-package.md" (f4-t4b); Authorities[8].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md" (f4-t4); Authorities[9].Path = "docs/initiatives/I-64-f4-gate-contract-package.md" (f4). No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 163 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, REFUERZO POR EL RECHAZO PREVIO: tiene exactamente 163 entradas, de la 0 a la 162, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON en una linea, y reproduce ese array tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True))"
  Puntos de control, comprobados contra la impresion numerada: la 42 es tests/RackCad.Tests/G9bInsertWiringGuardTests.cs, la 43 es tests/RackCad.Tests/RackViewBatchDriverGuardTests.cs y la 44 es tests/RackCad.Tests/G16FirstViewCorrectiveTests.cs; la 65 es README.md; la 110 es src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs.
  Las entradas 111 a 162 son los 52 archivos de la raiz de src/RackCad.Application/Systems/PushBack/ (fuera de Pilot/), en el orden de git ls-files, CADA UNO UNA SOLA VEZ: la 111 es src/RackCad.Application/Systems/PushBack/PushBackAdvancedRackParameters.cs y la 162, la ULTIMA de la lista, es src/RackCad.Application/Systems/PushBack/PushBackTroquelGrid.cs. Despues de PushBackSideConfiguration.cs vienen PushBackSystemFrontalBuilder.cs, PushBackSystemLateralBuilder.cs, PushBackSystemPlantaBuilder.cs, PushBackTarimaPlacement.cs, PushBackTopeSurface.cs y PushBackTroquelGrid.cs, y ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 163), comprueba que no hay repetidos y compara los indices 40 a 46 y 105 a 162 con la impresion numerada, uno por uno.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T164833Z-353a/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 162, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 26 elementos, en el mismo orden y con los mismos valores (ExpectRed true solo en el primero, FullyQualifiedName~RackCad.Tests.Workspace; false en los otros 25).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: Application (la operacion del puerto, el nucleo, el corredor del host, el puente y la sesion), el Plugin (el adaptador nuevo de contexto de comando, el escritor, el controlador y el puente de AutoCAD) y pruebas de Core con dobles de las primitivas, del ejecutor y del ambito. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (CORRECCION, intento 2 de la cadena F4-T4C1-HOST)
- RunId = R20261115T164833Z-353a (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4C1-HOST".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = 44eafbc18c73bab333085d668e84c19ab15fb208 (BaseSha de la primera delegacion de la cadena); ChainRedSha = 8e57061e62aa847eecab0b9653c8f9f008c434a2 (RED acreditado por R20261115T163412Z-eca1); CorrectionOf: ver abajo.
- ChainRedFiles = exactamente estas 17 rutas, en este orden (las acreditadas en la comprobacion Tests de R20261115T163412Z-eca1): tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs, tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs, tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs, tests/RackCad.Tests/Workspace/WorkspaceOwnMutationTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostDouble.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostRunTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs, tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs, tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4C1-HOST/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261115T164833Z-353a" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato y al analysis.md de la correccion; incluye: el RED de la correccion sobre el producto de 271a045c (las dos pruebas nuevas de B-01, la guarda corregida de M-01 y las pruebas de M-06 fallan por asercion contra ese producto; las demas pueden pasar y se declaran); el GREEN dentro del AllowedWriteScope sin tocar tests/; la marca en Execute retirada al salir y la caducidad de un billete terminado sin compleccion, sin un segundo resultado; G-1 como censo de todo src/ con sus excepciones exactas y G-10 sin excluir carpetas, con las cuatro ventanas como excepciones por archivo exacto; el orden de M-01; las negativas de M-02 y M-03; M-04 a M-07; la declaracion de los cambios de guarda de M-08 y de todas las corridas locales; la higiene de M-10; ningun aviso nuevo de compilacion (M-11); el resto del contrato del intento 1 sin regresiones, con las pruebas de T1B y las 23 clases de regresion en verde; la contencion de las corridas locales; la CI exacta del RED y del GREEN.
- CorrectionOf = { RunId: "R20261115T163412Z-eca1", FailureClass: "Tests", AnalysisSha256: "20f52055ba2122ee2700a66d29514aec52f6a66562a10abb6f96edb0d772c790" } (la verificacion del intento 1 y su analysis.md custodiado; la clase Tests registra que la evidencia de pruebas no cubria una cota que el paquete exige y que el producto incumplia, F4-E66).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T4C1-HOST, intento 2, F4-E66, CONTROLLER_PLANNING)".
