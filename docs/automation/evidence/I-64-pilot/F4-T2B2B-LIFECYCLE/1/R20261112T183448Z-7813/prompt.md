I-64 - DELEGACION F4-T2B2B-LIFECYCLE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T2B2B-LIFECYCLE)
Identidad: unidad I-64; gate F4; tarea F4-T2B2B-LIFECYCLE; intento 1; RunId R20261112T183448Z-7813;
           DelegationRunId R20261112T183448Z-7813; AuthorityRevision 06ef19150ea4401d622d135ebc439e112d75eadf (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 32369c7b24d393931c26daab33f53ca450bdcf2e: commit de emision del contrato, hijo directo de la AuthorityRevision 06ef19150ea4401d622d135ebc439e112d75eadf:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261112T183448Z-7813
Contrato de gate: artifacts/orchestration/I-64/F4-T2B2B-LIFECYCLE/1/R20261112T183448Z-7813/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T2B2B-LIFECYCLE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 13 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t2b2-gate-contract-package.md (v8: section 3, cambios declarados (1) y (3); section 5, decisiones D-T2B2-09 a D-T2B2-15 con la distincion del Owner de D-T2B2-10 y su tabla de veto; section 6, alcance, regla de I-1, trampas, residuales y arrastres; section 11); docs/initiatives/I-64-implementation-clarifications.md IC-08; docs/initiatives/I-64-proposal-v5.md (congelada) D-04, D-10 a D-13 y D-17, INV-20 e INV-28; la orden del Owner owner-f4t2b1-t2b2-disposition.md; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T2B2B-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cuatro planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); y "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T2B2B-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 147 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E29 a F4-E34 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1 (nucleo de borradores), F4-T2A (piloto), F4-T2B1 (lectura pura de la intencion) y F4-T2B2A (cableado de la intencion en el host: TryBeginPilotEdit y TryCheckPilotEdit, el registro de borradores del host y el lector anidado) estan cerradas. Esta tarea es F4-T2B2B: el ciclo de vida de los borradores (asesor de cierre y de destruccion en el puente, veto de cierre segun la distincion del Owner, retirada sincrona con operaciones explicitas y aditivas de DraftRegistry, el conocimiento limpio conocido, marcado de obsolescencia, recarga perezosa, registro de la perdida y descarte explicito por el host). La interfaz (T3), IC-04 (T2C) y la mutacion (T4) no se delegan aqui. El Owner exige revision directa e independiente de esta tarea y su comportamiento real se mide despues en Smoke-2.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 56b0d497 (corrida 43259514731; mismo producto que el HEAD: desde entonces solo cambio documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 1754 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde. Esta tarea no anade pruebas de UI: el filtro de UI tiene ExpectRed false.
  * Build local del Plugin en el HEAD, medido por la sesion: dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal> -> 0 errores, resumen "2 Advertencia(s)", ambas MSB3277 (referencias de AutoCAD). La CI compila el Plugin en Release y no ejecuta pruebas del Plugin.
  * La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug (lo que corre la CI): condicion previa conocida; esta tarea no toca la UI.
  * src/RackCad.Plugin/Workspace tiene 9 archivos (H-01); src/RackCad.Application/Workspace/Drafts tiene 5 archivos; WorkspaceHostSourceGuardTests.cs tiene 45 pruebas (H-12); la superficie publica de EventBridgeCore esta fijada por reflexion (WorkspaceBridgeFacadeSourceGuardTests.cs:204-224), con un constructor de 2 parametros; el puente tiene 16 +=, 16 -= y 16 manejadores.
  * El banco de pruebas permite probar esta tarea sin tocar WorkspaceHostTestSupport.cs: WorkspaceHostRig.Bridge.Core es publico; un comparador que lanza se siembra con el accesor internal OpenDrafts del host y con DraftThrowingComparer y DraftKit (internal, WorkspaceDraftSessionTests.cs).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 12 entradas del contrato, en su orden (12 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/EventBridgeCore.cs
    src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs
    src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryContainmentTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T2B2B-LIFECYCLE/1/R20261112T183448Z-7813/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B2B-LIFECYCLE/1/R20261112T183448Z-7813/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 77 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T2B2B-LIFECYCLE/1/R20261112T183448Z-7813/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 76, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (incluido ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T2B2B-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T2B2B-LIFECYCLE)
- RunId = R20261112T183448Z-7813 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T2B2B-LIFECYCLE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T2B2B-LIFECYCLE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261112T183448Z-7813" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED que compila y falla por asercion con el esqueleto inerte (con los cambios declarados (1) y (3) en el RED), el GREEN que no toca tests/, la tabla de veto del Owner, el build Release del Plugin sin errores ni avisos nuevos, la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T2B2B-LIFECYCLE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
