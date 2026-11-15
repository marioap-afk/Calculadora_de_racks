I-64 - DELEGACION F4-T4C2-PANEL - Controller, perfil CONTROLLER_PLANNING (CORRECCION, intento 2 de 3, de la cadena F4-T4C2-PANEL; F4-E69; contrato gate-contract-attempt2.json)
Identidad: unidad I-64; gate F4; tarea F4-T4C2-PANEL; intento 2; RunId R20261115T195135Z-d34e;
           DelegationRunId R20261115T195135Z-d34e; AuthorityRevision bbc259dd7c54c00edda2f6742961fa9daf5df3fe (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera a912f645933c476e8b4e0c1cdbb42ac46966546d: commit de emision del contrato del intento 2, hijo directo de la AuthorityRevision bbc259dd7c54c00edda2f6742961fa9daf5df3fe:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261115T195135Z-d34e
Contrato de gate: artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4C2-PANEL/coordinator/gate-contract-attempt2.json; sustituye al del intento 1)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 18 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4c-gate-contract-package.md v5, parte [2] (D-T4C-19): D-T4C-03 (el canal de finalizacion F-1 y sus cinco reglas), D-T4C-11 (la UI de Actualizar: el boton, el gesto y sus pasos 1 a 5, la presentacion de cada desenlace, IsMine, K = 1, los avisos y PICKFIRST), D-T4C-12 (el diagnostico diferido de T4A2), D-T4C-13 en lo que toca a M-03 en la UI, D-T4C-17 en lo que toca a la UI y a la contencion, D-T4C-20 filas 12, 18 y 21 y su apartado (a); section 6 (contrato, lo marcado [2] o [1, 2]); section 7 (Smoke-2); el cierre de F4-T4C1-HOST docs/automation/evidence/I-64-pilot/F4-T4C1-HOST/task-close-package.md; docs/initiatives/I-64-f4-t4a2-gate-contract-package.md (section 8, la tabla H-19 y D-T4A2-06); y la decision D5 del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4c-decisions-d1-d6.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (12 archivos) ; git ls-files src/RackCad.Application/Workspace (40 archivos) ; git ls-files src/RackCad.Plugin/Views (14 archivos) ; git ls-files tests/RackCad.Tests/Workspace (77 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
Fuente de la correccion: docs/automation/evidence/I-64-pilot/F4-T4C2-PANEL/1/R20261115T190629Z-9b55/analysis.md (la parada del Worker del intento 1, la causa raiz y la disposicion), su worker-handoff.json, el parche docs/automation/evidence/I-64-pilot/F4-T4C2-PANEL/1/R20261115T190629Z-9b55/wip-green-impl.patch (el producto completo, sin confirmar), F4-E69 y la decision del Owner F4-DRAFTS/coordinator/owner-f4t4c2-attempt2-decision.md.
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T4C-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
ANTECEDENTE DE ESTA TAREA: en el intento 1, tres planificaciones se rechazaron solo por ForbiddenWriteScope (una omitio la entrada 102; dos anadieron un espacio al principio de la entrada 71) y la cuarta, con la orden de copia de abajo, salio exacta. NINGUNA entrada empieza ni termina con un espacio: copia cada cadena exactamente como aparece entre comillas en la salida de la orden de copia, que imprime el array sin ningun espacio. Esta delegacion de correccion tiene su PROPIA replanificacion reforzada unica (decision D4 del Owner): un segundo fallo de fidelidad va con STOP al Owner.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido al principio de una entrada (A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T4C-C15 del contrato, un rechazo admite una sola replanificacion y un segundo va con STOP al Owner (D4).
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 212 y siguientes de docs/automation/evidence/I-64-evidence.md, en particular 215 y 216; F4-E67 a F4-E69 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- CORRECCION de F4-T4C2-PANEL (intento 2): en el intento 1 el Worker hizo el RED a6511c04 (el esqueleto inerte de la UI de Actualizar y del diagnostico, y las pruebas; CI con Core y UI en failure, como pide ExpectRed) y paro antes del GREEN (BLOCKED) por tres defectos de pruebas que el mismo escribio en el RED: una vista previa que nunca ocurre, porque el navegador fija las pestanas con trabajo protegido; un registro que esperaba cuatro entradas donde el repintado de F-1 anade una quinta; y un aviso xUnit2013 nuevo. El producto completo quedo en el parche custodiado, y aplicado en local deja todo en verde salvo esas dos pruebas. No hay ningun defecto del producto ni ninguna decision material nueva. El contrato del intento 2 conserva el alcance de 14 rutas, la lista prohibida, los 16 invariantes, los RequiredTests y las paradas, y anade INV-F4T4C-26 (las tres pruebas, exactamente, y el producto del parche como GREEN) e INV-F4T4C-27 (la mecanica).
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; el intento 3 no se gasta sin el Owner.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Las 14 rutas del AllowedWriteScope existen. El producto y las pruebas del HEAD son los del RED a6511c04 del intento 1 (git diff a6511c04 HEAD -- src tests vacio; desde entonces solo documentacion): el producto de la parte [2] es todavia el esqueleto inerte.
  * Cuentas: en la CI del RED del intento 1 (43753137730), el filtro de Workspace de Core selecciona 3437 y falla en 10, y el de UI selecciona 694 y falla en 75, todos por asercion; los minimos del contrato siguen siendo 3421 y 601; las 23 clases de regresion y PushBackClassicCharacterization pasan.
  * El RED de la correccion (INV-F4T4C-27): un commit que solo cambia esas tres pruebas, sobre el producto inerte del HEAD; los filtros de Workspace de Core y de UI siguen fallando por asercion (ExpectRed true en los dos), y la CI del RED da Core y UI en failure. El GREEN aplica el producto del parche y solo cambia src/.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: el boton y cuando se ofrece; el gesto Update() y el orden de sus pasos 1 a 5 (D-T4C-11); las cinco reglas de la presentacion de D-T4C-03 (una vez; el repintado anidado con InFlight falso y Unavailable no toca nada; el consumo por superficie tras IsMine; el valor de inicio que no es Accepted se presenta en el gesto; una peticion nueva nunca borra un resultado sin presentar); la tabla de presentacion de cada EditUpdateOutcome sin reclasificar, con CommitThrew y AppliedPostRereadFailed por los textos A2-11 y A2-12 que ya existen; M-03 en la UI; la opcion (a) del diagnostico de D-T4C-12; las filas 12, 18 y 21 de D-T4C-20; la frontera de INV-F4T4C-23; el doble de UI de INV-F4T4C-25.
  * Reglas de las corridas locales (F4-E59, INV-F4T4C-18): nunca la suite completa de UI en local, solo sus dos filtros de RequiredTests; ninguna sonda con entradas validas en caminos con dialogo o escritura, tampoco las confirmaciones del panel; ninguna prueba nueva escribe fuera de una ubicacion temporal aislada.
  * Trampas medidas (comprobacion independiente del contrato, M-09): tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:481-484 fija dos _knownClean en CheckBody y la forma literal de los case Recovered y Conflict (return EditCheckOutcome.Recovered( y Conflict(); tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:1003-1016 prohibe palabras tambien en comentarios de CORE y de WorkspaceEditIntent.cs; los nombres de tipo de H-20 (tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs); tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:323-347 y :383-399 (Content unicos en la superficie, ningun Width ni Height); tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs:414-477 (algunos textos una sola vez en la carpeta); el censo de Task y async de G-10 (0/0 en el VM del campo); y tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs:758-783 (los VM sin mecanismos). Ninguna se toca salvo las filas [2]; la entrega dice como las respeta.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 14 entradas del contrato, en su orden (14 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4c-gate-contract-package.md" (f4-t4c: el paquete de esta tarea); Authorities[7].Path = "docs/initiatives/I-64-f4-t4a2-gate-contract-package.md" (f4-t4a2); Authorities[8].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md" (f4-t4); Authorities[9].Path = "docs/initiatives/I-64-f4-gate-contract-package.md" (f4). No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 150 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA DESDE EL PRIMER INTENTO (por el rechazo de la primera planificacion de F4-T4C1-HOST): tiene exactamente 150 entradas, de la 0 a la 149, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: la 0 es src/RackCad.Plugin/ y la 1 src/RackCad.Application/Systems/; de la 2 a la 10, las nueve subraices de src/RackCad.UI/ (Controls/, Editor/, Preview/, RackFrames/, Shell/, StructuralSections/, Systems/, Themes/ y Views/); de la 11 a la 44, los 34 archivos de la raiz de src/RackCad.UI/ en el orden de git ls-files (la 11 es src/RackCad.UI/CatalogOption.cs y la 44 src/RackCad.UI/UiSupport.cs); de la 45 a la 49, los cinco archivos de src/RackCad.UI/Workspace/ que no se tocan (la 45 es WorkspaceBrowserViewModel.cs y la 49 WorkspacePanelViewModel.cs); la 50 es src/RackCad.Domain/; la 70 es docs/ y la 72 eng/; la 101 es tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs y la 102 tests/RackCad.Tests/Workspace/WorkspaceBridgeHintAndDrainTests.cs; la 109 es tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs; la 110 es src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs; la 129 es tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs.
  Las entradas 130 a 149 son los 20 archivos de tests/RackCad.UI.Tests/Workspace/ que no se tocan, en el orden de git ls-files, CADA UNO UNA SOLA VEZ: la 130 es tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserScenarioHarnessTests.cs y la 149, la ULTIMA de la lista, es tests/RackCad.UI.Tests/Workspace/WorkspacePortDouble.cs. Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 150), comprueba que no hay repetidos y compara los indices 0 a 12, 40 a 52 y 65 a 149 con la impresion numerada, uno por uno; comprueba que estan las dos entradas 101 y 102 y que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4C2-PANEL/2/R20261115T195135Z-d34e/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 149, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 26 elementos, en el mismo orden y con los mismos valores (ExpectRed true en el primero, FullyQualifiedName~RackCad.Tests.Workspace, y en el penultimo, FullyQualifiedName~RackCad.UI.Tests.Workspace; false en los otros 24).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: Application (TryCheckPilotEdit y EditCheckOutcome), la UI (el VM del campo, la vista XAML y su codigo subyacente) y pruebas de Core y de UI con el doble del puerto. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (CORRECCION, intento 2 de la cadena F4-T4C2-PANEL)
- RunId = R20261115T195135Z-d34e (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4C2-PANEL".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = 1cce48c00575f48e15106e4007c887c08746b228 (BaseSha de la primera delegacion de la cadena); ChainRedSha = null y ChainRedFiles = [] (ninguna verificacion acredito el RED a6511c04, porque no hubo GREEN); CorrectionOf: ver abajo.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4C2-PANEL/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261115T195135Z-d34e" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato y al analysis.md de la correccion; incluye: el RED de la correccion, que solo cambia exactamente las tres pruebas de INV-F4T4C-26 sobre el producto inerte, con Core y UI todavia en rojo por asercion; el GREEN, que aplica el producto del parche (con su SHA-256) y solo cambia src/, declarando frente al parche cualquier diferencia que exija una prueba corregida o un aviso; ninguna otra prueba cambia; el resto del contrato del intento 1 (el boton, el gesto, la presentacion, el diagnostico, M-03, las filas 12, 18 y 21, la frontera de INV-F4T4C-23 y el doble de INV-F4T4C-25) se cumple; ningun aviso nuevo de compilacion; las pruebas de T1B y las 23 clases de regresion en verde; la contencion de las corridas locales, sin la suite completa de UI; la CI exacta del RED y del GREEN.
- CorrectionOf = { RunId: "R20261115T190629Z-9b55", FailureClass: "StopCondition", AnalysisSha256: "113903c41b434f1b064f0851c1f442fa9ceffe1f8787485dda42ba1701e31de6" } (el Worker del intento 1, que paro antes del GREEN, y el analysis.md custodiado, F4-E69).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T4C2-PANEL, intento 2, F4-E69, CONTROLLER_PLANNING)".
