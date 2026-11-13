I-64 - DELEGACION F4-T3A-EDIT-PORT - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T3A-EDIT-PORT)
Identidad: unidad I-64; gate F4; tarea F4-T3A-EDIT-PORT; intento 1; RunId R20261113T012340Z-4a5e;
           DelegationRunId R20261113T012340Z-4a5e; AuthorityRevision 7aea8cec7532f0ec1495a6bcce84f8ee7f4e3143 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera ea34bf7d41019539e3361675e98b821f3c77bc58: commit de emision del contrato, hijo directo de la AuthorityRevision 7aea8cec7532f0ec1495a6bcce84f8ee7f4e3143:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261113T012340Z-4a5e
Contrato de gate: artifacts/orchestration/I-64/F4-T3A-EDIT-PORT/1/R20261113T012340Z-4a5e/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T3A-EDIT-PORT/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t3-gate-contract-package.md (v2: section 2, hallazgos H-01 a H-17 y el ciclo K = 1; section 3, division en T3A, T3B y T3C; section 4, decisiones D-T3-02 a D-T3-11 de F4-T3A; section 5, contrato propuesto; section 9 y section 10, disposiciones; las sections 7 y 8 son reglas de T3B y T3C, fuera de esta tarea); docs/initiatives/I-64-proposal-v5.md (congelada) D-02, D-04, D-06, D-08, D-10 y D-17 y P-03; docs/initiatives/I-64-implementation-clarifications.md IC-04; las ordenes del Owner owner-f4t3-authorization.md y owner-f4t3-decomposition-decision.md; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.Application/Workspace (31 archivos fuera de Drafts y 5 en Drafts) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T3A-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cuatro planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); y "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T3A-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 163 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E37 y F4-E38 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1 y F4-T2 (T2A, T2B1, T2B2A, T2B2B y T2C) estan cerradas. El Owner autorizo F4-T3 y la dividio en tres subtareas en orden A, B, C, cada una con su propio presupuesto. Esta tarea es F4-T3A-EDIT-PORT: el puerto de edicion del piloto en Application y en el host, SIN UI y SIN comando. Crea IWorkspacePilotEditPort (archivo nuevo, implementado de forma explicita por WorkspaceHostCore) con sus valores y enums nombrados; WorkspacePilotSupport.cs (archivo nuevo) con WorkspacePilotValueText, WorkspacePanelModalScope, WorkspaceNoticeDepth y el decorador WorkspacePilotEditGate; la compuerta de reentrada de las tres lecturas de la intencion (mutaciones Busy, lecturas anidadas Waiting, marca en try/finally); la compuerta de avisos (ninguna lectura dentro de StateChanged); la cola de avisos de veto en AdviseClose; el ambito del modal del panel en el controlador (EnterPanelModal y la propiedad PilotEdit); y los arrastres M-2..M-5 y D-1 de F4-T2B2B y 1-4 de F4-T2C. La UI del panel (T3B), el comando RACKPANELDESCARTAR y los mensajes en la linea de ordenes (T3C) y Actualizar (T4) no se delegan aqui. Oraculo critico del Owner: cero escrituras authored en el DWG.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 33e43f9f (corrida 43319785427; mismo producto que el HEAD: desde entonces solo cambio documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 2071 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde. Esta tarea no anade pruebas de UI: el filtro de UI tiene ExpectRed false.
  * La CI compila el Plugin en Release (job build-plugin) y no ejecuta pruebas del Plugin; el build local del Plugin en Release produce 2 avisos MSB3277 (referencias de AutoCAD) como linea base. Lo que sea solo Debug no puede producir avisos en Release.
  * La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug (lo que corre la CI): condicion previa conocida; esta tarea no toca la UI.
  * src/RackCad.Plugin/Workspace tiene 9 archivos y src/RackCad.Application/Workspace/Drafts 5 (H-11): ningun archivo nuevo en ninguno de los dos. WorkspaceHostCore no tiene API publica para editar un borrador fuera de Application (H-04), e IWorkspaceHostPort es de solo lectura por guarda (H-09). En el controlador hay 3 +=, 0 -= y un event, sin Dispatcher, Timer, Task ni async, y SetModalRackCadActive publico se conserva literal (H-12).
  * Las trampas de nombres y de texto de H-10 son vinculantes: los nombres de tipo nuevos (anidados incluidos) en RackCad.Application.Workspace no contienen Draft, Editor, Subscription ni la subcadena Tab sin distinguir mayusculas, ni coinciden con una palabra de BrowserSession.cs o de Drafts/*.cs; el codigo no contiene Transaction, la palabra Database, LockDocument, Commit(, RackLog ni, sin distinguir mayusculas, Provider (tampoco IFormatProvider), Metric, Aggregat, Bom, TotalRacks ni ProjectSummary.
  * El banco de pruebas permite montar el host con lectores dobles sin tocar WorkspaceHostTestSupport.cs ni WorkspaceBridgeTestSupport.cs; los archivos nuevos de pruebas son WorkspacePilotEditHostTests.cs y WorkspacePilotEditGuardTests.cs.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 12 entradas del contrato, en su orden (12 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
    src/RackCad.Application/Workspace/WorkspacePilotSupport.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulerTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T3A-EDIT-PORT/1/R20261113T012340Z-4a5e/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3A-EDIT-PORT/1/R20261113T012340Z-4a5e/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 97 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3A-EDIT-PORT/1/R20261113T012340Z-4a5e/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 96, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (incluido ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T3A-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T3A-EDIT-PORT)
- RunId = R20261113T012340Z-4a5e (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T3A-EDIT-PORT".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T3A-EDIT-PORT/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261113T012340Z-4a5e" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED que compila y falla por asercion con el esqueleto inerte (los arrastres que solo tocan guardas pueden pasar en el RED si demuestran que discriminan con autopruebas de mutacion), las pruebas de host y de guarda de D-T3-10, el oraculo de cero escrituras authored, el GREEN que no toca tests/, el build Release del Plugin sin errores ni avisos nuevos, la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T3A-EDIT-PORT, primera delegacion de la cadena, CONTROLLER_PLANNING)".
