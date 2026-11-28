I-64 - DELEGACION F4-T2B2A-INTENT-WIRING - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T2B2A-INTENT-WIRING)
Identidad: unidad I-64; gate F4; tarea F4-T2B2A-INTENT-WIRING; intento 1; RunId R20261112T134333Z-a409;
           DelegationRunId R20261112T134333Z-a409; AuthorityRevision b6130ef1176ef06c679e31535252d50822abeedb (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera e277dd42ab25144bc4ee73d373ca51592c6d7f02: commit de emision del contrato, hijo directo de la AuthorityRevision b6130ef1176ef06c679e31535252d50822abeedb:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261112T134333Z-a409
Contrato de gate: artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/1/R20261112T134333Z-a409/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2B2A-INTENT-WIRING/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 12 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t2b2-gate-contract-package.md (v5: section 2, hallazgos H-01 a H-14; section 4, decisiones D-T2B2-01 a D-T2B2-08 con la tabla de pasos de D-T2B2-06 y las trampas conocidas de D-T2B2-08; section 6, contratos propuestos; section 10); docs/initiatives/I-64-f4-t2b-gate-contract-package.md section 4 (D-T2B-03, las 14 reglas, para los arrastres); docs/initiatives/I-64-proposal-v5.md (congelada) D-02, D-03, D-04, D-10, D-11, D-12 y D-17; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (7 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T2B2A-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en las tareas anteriores de esta unidad, tres planificaciones se RECHAZARON por la copia: una cambio "lo leen" por "los leen" en un invariante (A5 FAIL), otra omitio UNA entrada de ForbiddenWriteScope (A4 FAIL) y otra anadio un espacio delante de UNA entrada (" .github/" en vez de ".github/", A4 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 138 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E26, F4-E29 y F4-E30 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1 (nucleo de borradores en src/RackCad.Application/Workspace/Drafts/), F4-T2A (piloto de Push Back) y F4-T2B1 (lectura pura de la intencion, PushBackPilotIntent{Scan,Reader,Result}) estan cerradas. F4-T2B2 se dividio en T2B2A y T2B2B (F4-E29). Esta tarea es T2B2A: el cableado de la intencion en el host. El ciclo de vida (asesor, veto de cierre, retirada, descarte, marcado de obsolescencia, recarga: T2B2B), IC-04 (T2C), la interfaz (T3) y la mutacion (T4) no se delegan aqui.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit fe7f136b (corrida 43190415649; mismo producto que el HEAD: desde entonces solo cambio documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 1592 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde. Esta tarea no anade pruebas de UI: el filtro de UI tiene ExpectRed false.
  * Build local del Plugin en el HEAD, medido por la sesion con las DLL de AutoCAD instaladas: dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal> -> 0 errores, resumen "2 Advertencia(s)", ambas MSB3277 (referencias de AutoCAD). La CI compila el Plugin en Release y no ejecuta pruebas del Plugin; las guardas de texto corren en Core (Linux).
  * Namespaces de prueba: RackCad.Tests (Core, file-scoped). Las clases nuevas se llaman WorkspaceEditIntent... en tests/RackCad.Tests/Workspace/ para que el filtro por subcadena las seleccione. RackCad.Application.csproj:11 tiene InternalsVisibleTo hacia RackCad.Tests.
  * src/RackCad.Plugin/Workspace tiene exactamente 9 archivos y ninguna subcarpeta (H-01); tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs tiene exactamente 45 pruebas (H-12); la superficie publica de EventBridgeCore esta fijada por reflexion (H-04); el puerto IWorkspaceHostPort solo admite nombres de lectura y un unico WorkspaceRequest (H-03).
  * Las referencias de linea de los hallazgos H-01 a H-14 y de las trampas de D-T2B2-08 del paquete se midieron en 51163211; entre 51163211 y el HEAD, src/, tests/ y .github/ no cambian.
  * Los arrastres T2B1/N-1 a T2B1/N-4 (D-T2B2-07) estan descritos en docs/automation/evidence/I-64-pilot/F4-T2B1-INTENT-READ/task-close-package.md, section 5.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 13 entradas del contrato, en su orden (13 archivos exactos, sin prefijos):
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/1/R20261112T134333Z-a409/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/1/R20261112T134333Z-a409/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 70 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (incluido ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2B2A-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T2B2A-INTENT-WIRING)
- RunId = R20261112T134333Z-a409 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2B2A-INTENT-WIRING".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2B2A-INTENT-WIRING/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261112T134333Z-a409" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED que compila y falla por asercion con el esqueleto inerte, el GREEN que no toca tests/, el build Release del Plugin sin errores ni avisos nuevos, la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T2B2A-INTENT-WIRING, primera delegacion de la cadena, CONTROLLER_PLANNING)".
