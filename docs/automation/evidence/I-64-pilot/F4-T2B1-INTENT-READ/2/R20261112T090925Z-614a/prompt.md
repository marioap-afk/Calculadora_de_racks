I-64 - DELEGACION F4-T2B1-INTENT-READ - Controller, perfil CONTROLLER_PLANNING (CORRECCION, intento 2 de 3, de la cadena F4-T2B1-INTENT-READ; F4-E27; contrato gate-contract-attempt2.json)
Identidad: unidad I-64; gate F4; tarea F4-T2B1-INTENT-READ; intento 2; RunId R20261112T090925Z-614a;
           DelegationRunId R20261112T090925Z-614a; AuthorityRevision e20a294c5e9f7160581e5674e08f8156accf876a (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera b9707bc638e04f8351790485c2e6bfbd23276c55: commit de emision del contrato del intento 2, hijo directo de la AuthorityRevision e20a294c5e9f7160581e5674e08f8156accf876a:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261112T090925Z-614a
Contrato de gate: artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/2/R20261112T090925Z-614a/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2B1-INTENT-READ/coordinator/gate-contract-attempt2.json; sustituye al del intento 1)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t2b-gate-contract-package.md (section 4, decisiones D-T2B-01 a D-T2B-08, con la tabla de las 14 reglas y los textos exactos de D-T2B-03; section 5, el contrato propuesto); docs/initiatives/I-64-proposal-v5.md (congelada) D-10, D-11, D-12 y D-17; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (7 archivos: los 4 del piloto cerrado y los 3 que creo el intento 1) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T2B1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Fuente de la correccion: docs/automation/evidence/I-64-pilot/F4-T2B1-INTENT-READ/1/R20261112T084307Z-16bc/analysis.md (hallazgos de las revisiones del intento 1 y su disposicion) y F4-E27.
Antecedentes: la planificacion anterior de ESTA correccion, R20261112T090535Z-58d4, se RECHAZO (A4 FAIL) porque copio ForbiddenWriteScope[47] como " .github/", con un espacio delante, en vez de ".github/". Esta es la UNICA replanificacion autorizada: si vuelve a fallar la copia exacta, no hay otra. Antes, en la tarea F4-T2A de esta unidad, dos planificaciones se RECHAZARON por la copia: una cambio "lo leen" por "los leen" en un invariante (A5 FAIL) y otra omitio UNA entrada de ForbiddenWriteScope (A4 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 125 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E18, F4-E25 y F4-E26 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- CORRECCION de F4-T2B1 (intento 2): la entrega del intento 1 (RED 04717913, GREEN 23792b2d) dio EXECUTION_VERIFIED 14/14 en R20261112T084307Z-16bc, pero la revision adversarial independiente, obligatoria (F4-E02), dio NO ACEPTABLE por cobertura de pruebas: la regla 13 no tiene prueba y es alcanzable (I-1) y ninguna prueba ejercita el catch de Read ante una excepcion del comparador (I-2); ademas hay tres menores de producto (M-3 testigo Unchanged con el origen ausente de Facts, M-5 RawEnvelopes expone el Dictionary interno, M-7 Detail vacio en vez de null) y menores de pruebas (M-1, M-2, M-4, M-9, M-10). El contrato del intento 2 solo cambia y anade invariantes (INV-F4T2B1-02, 05, 07, 10, 13 y el nuevo 15).
  Presupuesto: Attempt 2, AttemptsRemaining 1, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 23792b2d (corrida 43184046954; mismo producto que el HEAD): FullyQualifiedName~RackCad.Tests.Workspace = 1584 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde (los minimos del contrato siguen siendo 1468 y 248). Esta tarea no anade pruebas de UI: el filtro de UI tiene ExpectRed false.
  * Producto del intento 1 (a corregir solo en lo que piden los invariantes): src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentScan.cs (RawEnvelopes se expone hoy como el Dictionary interno, lineas 26, 75 y 90), PushBackPilotIntentReader.cs (testigo en las lineas 228-239; regla 9 con originKind ?? string.Empty en la linea 196; m.Embed!.Design! en la linea 201; reglas 5 y 6 en las lineas 174-181) y PushBackPilotIntentResult.cs. Pruebas del intento 1: tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntent{StaleBase,Read,Guard}Tests.cs.
  * Namespaces de prueba: RackCad.Tests (Core, file-scoped). Las clases nuevas se llaman WorkspaceDraftPilotIntent... en tests/RackCad.Tests/Workspace/ para que el filtro por subcadena las seleccione.
  * Kit de pruebas existente y reutilizable: PilotKit (internal static, en tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:30-189): Catalog (JsonRackCatalogProvider.FromBaseDirectory().Load()), RepoRoot(), Text(fixture) que lee tests/RackCad.Tests/Fixtures/I64/<fixture>.json, Admitted(text) y Normalize. Fixtures existentes: pb-01-base (un lado, admitido), pb-20-ab (A/B, fuera del piloto), pb-21-cola (cola, fuera del piloto) y otros pb-*.json; son sobres completos con Kind "pushback", Id, View, Section y Design.
  * Producto nuevo: namespace RackCad.Application.Systems.PushBack.Pilot, UN namespace de bloque por archivo (NamespaceFolderGuardTests: el namespace es la ruta de la carpeta); los cuatro archivos vecinos del piloto llevan #nullable enable.
  * Hechos que consume la tarea, sin cambiarlos: RackSiblingMembership.Classify(scan, rackId, originalSourceId, originalSourceIdIsAttributable) en src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:93-155 (lanza si el rackId esta en blanco o si los hechos violan su estructura; un sobre interpretable con Id vacio es NotMember; atribucion OrdinalIgnoreCase); RackAuthoredComparatorPorts.PushBack() en src/RackCad.Application/Systems/Shared/RackAuthoredComparator.FourKinds.cs:18-20 y :49-82; AuthoredRawReader.Read en src/RackCad.Application/Systems/Shared/AuthoredRawReader.cs:19-52; RackEmbedStore.Deserialize en src/RackCad.Application/Persistence/RackEmbedDocument.cs:97-129 (null si el JSON es invalido o la version mayor es mas nueva); DraftStaleBase.Create y CompareWith en src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs; DraftIntentRead, CheckNow y DraftApplyOutcome<TState>.CommitThrew en Drafts/DraftSession.cs y Drafts/DraftApplyOutcome.cs; PushBackPilotMaterializer.Materialize y sus constantes en Pilot/PushBackPilotMaterializer.cs.
  * Regla de IsComplete de los llamadores integrados: src/RackCad.Plugin/Views/RackProjectionKindSessions.cs:223-237 (solo lectura).
  * GUARDAS existentes: WorkspaceModelBoundaryTests recorre src/RackCad.Application/Workspace/ (no el piloto); WorkspaceDraftBoundaryTests prohibe nombres de sistema en Drafts y recorre 4 archivos de preview (PreviewFiles, :547-567). Ninguna guarda existente se modifica en esta tarea; la guarda nueva vive en WorkspaceDraftPilotIntentGuardTests.cs.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 7 entradas del contrato, en su orden (6 archivos y el prefijo de fixtures), las mismas que en el intento 1:
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentScan.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs
    tests/RackCad.Tests/Fixtures/I64/intent/
  Nunca anadas una ruta fuera del contrato ni otro prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/2/R20261112T090925Z-614a/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/2/R20261112T090925Z-614a/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
- Ninguna cadena copiada lleva espacios, tabuladores ni saltos de linea anadidos al principio, al final ni entre elementos: escribe cada lista como el JSON que produce la orden de carga (json.dumps de la lista cargada), sin reescribir sus elementos a mano.
  ForbiddenWriteScope tiene exactamente las 58 entradas del contrato: ni una mas ni una menos.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (incluido ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2B1-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (correccion, intento 2, de la cadena F4-T2B1-INTENT-READ)
- RunId = R20261112T090925Z-614a (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2B1-INTENT-READ".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = 445c46cefe587a6ba3274143bbbee86cc4ff279b (BaseSha de la primera delegacion de la cadena); ChainRedSha = 04717913786d80f28149180fa2ce42b6051d0a5d (RED acreditado por R20261112T084307Z-16bc); CorrectionOf: ver abajo.
- ChainRedFiles = exactamente estas 3 rutas, en este orden (las acreditadas en la comprobacion Tests de R20261112T084307Z-16bc): tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs, tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261112T090925Z-614a" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato y al analysis.md de la correccion; incluye el RED de la correccion (pruebas que compilan; las de M-3, M-5 y M-7 fallan por asercion contra 23792b2d), el GREEN que solo cambia producto y no toca tests/, la CI del SHA exacto y los conteos minimos de RequiredTests.
- CorrectionOf = { RunId: "R20261112T084307Z-16bc", FailureClass: "Tests", AnalysisSha256: "90d2db6cf55c1d695c4d326a7d33e3c22f3054d68e0abd559cf0802882f2ac34" } (la verificacion del intento 1 y su analysis.md custodiado; la clase Tests registra que el hallazgo es de cobertura de las pruebas exigidas, F4-E27).
- IssuedBy = "Controller Codex (planificacion de la correccion F4-T2B1-INTENT-READ, intento 2, F4-E27, CONTROLLER_PLANNING)".
