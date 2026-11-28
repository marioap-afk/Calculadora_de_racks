I-64 - DELEGACION F4-T3B2-EDIT-CLOSE-SYNC - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T3B2-EDIT-CLOSE-SYNC)
Identidad: unidad I-64; gate F4; tarea F4-T3B2-EDIT-CLOSE-SYNC; intento 1; RunId R20261113T113012Z-e025;
           DelegationRunId R20261113T113012Z-e025; AuthorityRevision 41ca9a52ba0974da32aab4bc6f5ec0ef4e88e276 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 2c76c52f1d8fba938e2741cbc9bc38a8912ef5ce: commit de emision del contrato, hijo directo de la AuthorityRevision 41ca9a52ba0974da32aab4bc6f5ec0ef4e88e276:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261113T113012Z-e025
Contrato de gate: artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T113012Z-e025/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T3B2-EDIT-CLOSE-SYNC/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t3b2-gate-contract-package.md (v3: section 2, hechos H-01 a H-08, pruebas P-01 a P-05 y trampas de guardas G-01 a G-09; section 3, decisiones D-T3B2-01 a D-T3B2-09; section 4, pruebas; section 5, contrato propuesto; sections 7 y 8, disposiciones de las revisiones; section 9, disposicion del Owner y del Coordinator F4-E43); docs/initiatives/I-64-f4-t3b-gate-contract-package.md section 6 (R-B2-1 a R-B2-6); docs/initiatives/I-64-f4-t3-gate-contract-package.md (section 2, tabla K = 1; section 7, R-B6 a R-B9 y R-C2); docs/initiatives/I-64-proposal-v5.md (congelada) D-08, D-17 y P5; las ordenes del Owner owner-f4t3-authorization.md, owner-f4t3b-decomposition-decision.md y owner-f4t3b1-close-disposition.md (disposicion del Owner y del Coordinator sobre el cierre de F4-T3B1, F4-E43); el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.UI/Workspace (8 archivos) ; git ls-files tests/RackCad.UI.Tests/Workspace (19 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T3B2-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
RECHAZO PREVIO DE ESTA TAREA: la planificacion R20261113T112617Z-af63 de esta misma tarea se RECHAZO antes del Worker (comparacion byte a byte REJECTED y A5 FAIL): en Authorities[1].Path escribio "docs/initiatives/I-64-f4-t3-gate-contract-package.md" en vez de "docs/initiatives/I-64-f4-t3b-gate-contract-package.md" (le falto la letra b). Todo lo demas de esa planificacion era correcto. Esta es la REPLANIFICACION REFORZADA UNICA que permite la parada F4T3B2-C08: un segundo fallo de fidelidad de la planificacion va con STOP al Coordinator y no hay tercera.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y, en la tarea anterior, "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T3B2-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 171 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E42, F4-E43 y F4-E44 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3A y F4-T3B1 estan cerradas. El Owner dividio F4-T3 en T3A, T3B y T3C, y F4-T3B en T3B1 y T3B2, cada una con su propio presupuesto, en orden B1, B2, C. Esta tarea es F4-T3B2-EDIT-CLOSE-SYNC: sobre el campo de F4-T3B1, el cierre de pestanas con Descartar o Cancelar en linea, Descartar del campo en Error, el descarte de los borradores del documento con su TOCTOU, los avisos de veto y de perdida consumidos solo en Present, la retencion de la pestana activa con texto pendiente (BrowserSession.ApplySelectionContext con el parametro opcional keepActive) y los arrastres N-1 y N-3 de F4-T3B1. Archivos nuevos: WorkspacePilotCloseSyncGuardTests.cs (Core) y WorkspacePilotCloseSyncTests.cs y WorkspacePilotCloseSyncViewTests.cs (UI). Oraculo critico del Owner: cero escrituras authored en el DWG; cerrar nunca escribe ni hace Actualizar. La disposicion del Owner y del Coordinator (F4-E43) cerro F4-T3B1, autorizo esta tarea, vinculo N-1 y N-3 y acepto en el contrato tres tratamientos: sin borrador la pestana se cierra sin DiscardRack; el aviso de veto nombra solo la via del panel hasta F4-T3C; el fallo de GetRacksAtRisk queda como limitacion declarada hasta su endurecimiento en F4-T3C.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base medidas en el GREEN de F4-T3B1 (1708e764, CI 43380342743; desde entonces solo documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 2300 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 352, todas en verde. Las dos tienen ExpectRed true: en el RED fallan solo las pruebas nuevas, por asercion, y todas las existentes pasan (INV-F4T3B2-14).
  * El filtro de UI selecciona las clases cuyo nombre completo contiene RackCad.UI.Tests.Workspace: las clases de prueba estan en el namespace RackCad.UI.Tests y su nombre empieza por Workspace, asi que las clases nuevas tambien empiezan por Workspace.
  * La CI corre las pruebas en Debug y compila el Plugin en Release (job build-plugin); el build local del Plugin en Release produce 2 avisos MSB3277 como linea base. La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug: condicion previa conocida.
  * La forma de la UI esta FIJADA en D-T3B2-07 del paquete (API publica nueva del VM del navegador, el record WorkspaceBrowserNoticeItem, ShowDiscardButton del campo, los nombres del XAML y de automatizacion, la vida del aviso del navegador y la contencion) y el doble en D-T3B2-08: trasladalos a los AcceptanceCriteria como receta, sin reconstruirlos. Las guardas nuevas buscan llamadas, no subcadenas (section 4).
  * Las trampas G-01 a G-09 del section 2 del paquete estan medidas: sin += ni -= en los VM; .IsSessionAlive( exactamente una vez en el VM del navegador; el vocabulario de BrowserSession sin Draft ni Dirty; sin Registry ni RackLog en la UI; sin tamanos fijos ni ContentControl en la seccion del navegador. Cambios declarados de pruebas existentes: solo P-01 (WorkspacePilotFieldViewTests.cs, la asercion que excluye TakeLossNotices y TakeCloseNotices) y P-05 (T06).
  * El doble WorkspacePilotEditPortDouble de F4-T3B1 existe; DiscardRack y DiscardDocument responden Rejected por defecto y asi siguen; esta tarea le anade respondedores, colas, el ayudante DiscardAsTheHostDoes y ThrowIfAsked (D-T3B2-08).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 13 entradas del contrato, en su orden (13 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/BrowserSession.cs
    src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
    src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserSessionTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T113012Z-e025/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T113012Z-e025/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T113012Z-e025/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo. Las expresiones regulares de INV-F4T3B2-12 llevan barras invertidas: se copian con las mismas barras.
- AUTHORITIES: los tres primeros elementos citan TRES paquetes DISTINTOS con nombres casi iguales; se copian los tres, cada uno en su indice y con su nombre exacto:
    Authorities[0].Path = "docs/initiatives/I-64-f4-t3b2-gate-contract-package.md" (t3b2: el paquete de esta tarea);
    Authorities[1].Path = "docs/initiatives/I-64-f4-t3b-gate-contract-package.md" (t3b, con b y sin 2: el rechazo de af63 fue aqui);
    Authorities[2].Path = "docs/initiatives/I-64-f4-t3-gate-contract-package.md" (t3, sin b).
  Antes de terminar, ejecuta una orden que imprima cada autoridad del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T113012Z-e025/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['Authorities'])]") y compara tu lista con esa impresion, indice por indice y letra por letra en cada Path y cada Section.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 102 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T113012Z-e025/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 101, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (ExpectRed true en los dos: en el de Core y en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T3B2-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T3B2-EDIT-CLOSE-SYNC)
- RunId = R20261113T113012Z-e025 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T3B2-EDIT-CLOSE-SYNC".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T3B2-EDIT-CLOSE-SYNC/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261113T113012Z-e025" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED con el esqueleto inerte completo (solo las pruebas nuevas fallan, por asercion), las pruebas de VM y de vista (STA) y las guardas de la section 4 del paquete, la cobertura 11-14 del Owner, el oraculo de cero escrituras authored, el GREEN que no toca tests/, el build Release del Plugin sin errores ni avisos nuevos, la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T3B2-EDIT-CLOSE-SYNC, primera delegacion de la cadena, CONTROLLER_PLANNING)".
