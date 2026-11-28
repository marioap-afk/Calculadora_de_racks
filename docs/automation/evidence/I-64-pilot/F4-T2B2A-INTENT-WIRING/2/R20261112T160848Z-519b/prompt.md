I-64 - DELEGACION F4-T2B2A-INTENT-WIRING - Controller, perfil CONTROLLER_PLANNING (CORRECCION, intento 2 de 3, de la cadena F4-T2B2A-INTENT-WIRING; F4-E32; contrato gate-contract-attempt2.json)
Identidad: unidad I-64; gate F4; tarea F4-T2B2A-INTENT-WIRING; intento 2; RunId R20261112T160848Z-519b;
           DelegationRunId R20261112T160848Z-519b; AuthorityRevision 32a01d0dbc113626c491d5e428c5535844c78696 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 36b497ad20a522e4aebd27d9632485f80ff68a19: commit de emision del contrato del intento 2, hijo directo de la AuthorityRevision 32a01d0dbc113626c491d5e428c5535844c78696:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261112T160848Z-519b
Contrato de gate: artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/2/R20261112T160848Z-519b/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2B2A-INTENT-WIRING/coordinator/gate-contract-attempt2.json; sustituye al del intento 1)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 13 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t2b2-gate-contract-package.md (v5: section 2, section 4 D-T2B2-01 a D-T2B2-08, section 5 para saber que es de F4-T2B2B, section 6 y section 10); docs/initiatives/I-64-proposal-v5.md (congelada) D-02, D-10, D-11, D-12 y D-17; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ;
           git diff --name-only e277dd42ab25144bc4ee73d373ca51592c6d7f02 db37e30db013107ad865523f8e5412da4c5aeb27 -- tests/ (las 5 rutas del RED del intento 1) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T2B2A-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Fuente de la correccion: docs/automation/evidence/I-64-pilot/F4-T2B2A-INTENT-WIRING/1/R20261112T155354Z-46e1/analysis.md (hallazgos de las revisiones del intento 1 y su disposicion), docs/automation/evidence/I-64-pilot/F4-T2B2A-INTENT-WIRING/1/R20261112T155354Z-46e1/independent-review.md (el informe de la revision independiente, con archivos y lineas) y F4-E32.
Antecedentes: en esta unidad, cuatro planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); y, en el intento 1 de esta misma tarea (R20261112T134333Z-a409), "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" en ForbiddenWriteScope[43] (A4 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T2B2A-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 147 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E29 a F4-E32 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- CORRECCION de F4-T2B2A (intento 2): la entrega del intento 1 (RED db37e30d, GREEN 2d743e46) dio EXECUTION_VERIFIED 14/14 en R20261112T155354Z-46e1, pero la revision adversarial independiente, obligatoria (F4-E02), dio NO ACEPTABLE: el producto no tiene defectos de correccion; el hallazgo IMPORTANTE I-1 es que varias pruebas y guardas nuevas prohiben para siempre lo que el paquete v5 asigna a F4-T2B2B (y a F4-T3), que no podria cambiarlas sin salir de su alcance. Ademas hay menores de producto (M-3 testigo con clave nula o en blanco, M-4 TopTransaction incompleto, M-5 firma del constructor del barrido, M-7 Differences null en default) y de pruebas (M-2, M-6, M-9). El contrato del intento 2 solo anade texto a INV-F4T2B2A-01, 02, 06, 07, 08 y 09, anade INV-F4T2B2A-13 y la parada F4T2B2A-C08.
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 2d743e46 (corrida 43247864348; mismo producto que el HEAD): FullyQualifiedName~RackCad.Tests.Workspace = 1727 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde (los minimos del contrato siguen siendo 1592 y 248). Esta tarea no anade pruebas de UI: el filtro de UI tiene ExpectRed false.
  * Build local del Plugin en el BaseSha del intento 1, medido por la sesion: dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal> -> 0 errores y "2 Advertencia(s)", ambas MSB3277; el intento 1 no anadio ninguna. La CI compila el Plugin en Release y no ejecuta pruebas del Plugin.
  * La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug (lo que corre la CI): condicion previa conocida, medida tambien en F4-T2A; esta tarea no toca la UI.
  * Producto del intento 1 (a corregir solo en lo que piden los invariantes): src/RackCad.Plugin/Views/RackSiblingScan.cs (constructor internal de RackSiblingDrawingScan en las lineas 43-49, con el parametro opcional rawEnvelopes); src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs (Read en la linea 94, comprobaciones Debug en las lineas 104 y 120; ReadPreviewWitness en la linea 142; AssertNoOpenTransaction en la linea 181); src/RackCad.Application/Workspace/WorkspaceEditIntent.cs (documentacion de Rejected en la linea 85; Differences en la linea 201); src/RackCad.Application/Workspace/WorkspaceHostCore.cs (TryBeginPilotEdit en la linea 253 y TryCheckPilotEdit en la linea 332, con su paso 1). Pruebas del intento 1: tests/RackCad.Tests/Workspace/WorkspaceEditIntent{Host,Guard}Tests.cs y los tres archivos de los arrastres de F4-T2B1.
  * Namespaces de prueba: RackCad.Tests (Core, file-scoped). Plugin/Workspace tiene 9 archivos (H-01); WorkspaceHostSourceGuardTests.cs tiene 45 pruebas (H-12).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 13 entradas del contrato, en su orden (13 archivos exactos, sin prefijos), las mismas que en el intento 1:
    src/RackCad.Plugin/Views/RackSiblingScan.cs
    src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs
    src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/2/R20261112T160848Z-519b/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/2/R20261112T160848Z-519b/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 70 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos ni con una palabra de menos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/2/R20261112T160848Z-519b/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]"
  y comprueba tu lista contra esa salida, indice por indice, de 0 a 69, mirando cada palabra de cada nombre de archivo (el intento 1 perdio la palabra Source en el indice 43).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (incluido ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2B2A-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion, intento 2, de la cadena F4-T2B2A-INTENT-WIRING)
- RunId = R20261112T160848Z-519b (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2B2A-INTENT-WIRING".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = e277dd42ab25144bc4ee73d373ca51592c6d7f02 (BaseSha de la primera delegacion de la cadena); ChainRedSha = db37e30db013107ad865523f8e5412da4c5aeb27 (RED acreditado por R20261112T155354Z-46e1); CorrectionOf: ver abajo.
- ChainRedFiles = exactamente estas 5 rutas, en este orden (las acreditadas en la comprobacion Tests de R20261112T155354Z-46e1): tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs, tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261112T160848Z-519b" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato y al analysis.md de la correccion; incluye el RED de la correccion (pruebas y guardas que compilan; las de M-3, M-4, M-5 y M-7 fallan por asercion contra 2d743e46), el GREEN que solo cambia producto y no toca tests/, el build Release del Plugin sin errores ni avisos nuevos, la CI del SHA exacto y los conteos minimos de RequiredTests.
- CorrectionOf = { RunId: "R20261112T155354Z-46e1", FailureClass: "Tests", AnalysisSha256: "d58d868a45c9b3fb6d548f6dfc16bb5653ac57ca9fa7021ffbaa249cc44d3dc8" } (la verificacion del intento 1 y su analysis.md custodiado; la clase Tests registra que el hallazgo importante es de las pruebas y guardas exigidas, F4-E32).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T2B2A-INTENT-WIRING, intento 2, F4-E32, CONTROLLER_PLANNING)".
