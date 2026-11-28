I-64 - DELEGACION F2-T1-NAVIGATION-CORE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F2; tarea F2-T1-NAVIGATION-CORE; intento 1; RunId R20261108T181429Z-adae;
           DelegationRunId R20261108T181429Z-adae; AuthorityRevision 90f5872e89660655a93606e16d50f4bc19aa3c9c (la del contrato);
           MainSha a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60; BaseSha = el HEAD actual del worktree (se espera 9082bcbcefbf33ca1f9540c34e6f0e96e20cdcfb, descendiente de la AuthorityRevision:
           solo anade la custodia de la apertura de F2, la sonda del Controller, el contrato ASCII reemitido, la evidencia section 76 y
           el estado; AUTOMATION_PLAN 16.3 admite AuthorityRevision ancestro de BaseSha con solo rutas UNIT_DOC entre ambos);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261108T181429Z-adae
Contrato de gate: artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/R20261108T181429Z-adae/gate-contract.json (ASCII puro; reemision ASCII REISSUE_GATE_CONTRACT_ASCII del contrato del Coordinator, con su
           registro de equivalencia en el mismo directorio; custodiado en docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas si pasa A1-A8 y nc4.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas y archivos calientes: exactamente las 2 entradas de AllowedWriteScope del contrato: src/RackCad.Application/Workspace/ y
           tests/RackCad.Tests/Workspace/. Todo lo demas es de solo lectura (Plugin, UI, Domain, StructuralAnalysis y docs, prohibidos).
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) D-03, D-05, D-06, D-07, INV-03, INV-22, INV-36, INV-38,
           INV-40, section 11 (fila F2), section 12.3 y section 13; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Workspace (17 archivos) ; git ls-files tests/RackCad.Tests/Workspace (26 archivos)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y F2-C01..F2-C08 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" en el texto libre
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 64 a section 76 de docs/automation/evidence/I-64-evidence.md)
- F1 (Workspace Foundation) esta cerrada con PASS y su producto existe en este HEAD; la sincronizacion con I-65 y la comprobacion
  de host posterior tambien pasaron. F2 (Context Navigation) esta abierta y se divide en dos tareas: F2-T1-NAVIGATION-CORE (esta,
  pura, sin AutoCAD) y F2-T2-NAVIGATION-HOST-UI (adaptador AutoCAD, acciones reales, UI, eco y foco; NO autorizada todavia).
- Presupuesto de F2-T1 (disposicion del Coordinator, section 76): Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3;
  CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada. La delegacion debe dejar al Worker todo lo necesario
  para acertar a la primera.
- HECHOS DEL REPOSITORIO medidos por la sesion (inventario de solo lectura), relevantes para AcceptanceCriteria:
  * Proyectos: tests/RackCad.Tests referencia Domain y Application (nunca el Plugin ni la UI). src/RackCad.Application/Workspace/
    tiene hoy 17 archivos (modelo de F1-T1, nucleo del puente de F1-T2, nucleo del host de F1-T3A); todos con #nullable enable y
    namespace de bloque RackCad.Application.Workspace. Las pruebas de tests/RackCad.Tests/Workspace/ usan el namespace RackCad.Tests
    (file-scoped) y #nullable enable por archivo. FullyQualifiedName~Workspace selecciona hoy 645 pruebas de Core, todas en verde.
  * Tipos existentes reutilizables (no se duplican): SessionId(long) y RackKey(SessionId Session, string RackId) en SessionId.cs
    (OJO: la igualdad de RackKey compara RackId de forma ordinal y sensible a mayusculas; D-05 exige OrdinalIgnoreCase para el
    RackId); WorkspaceRequest(SessionId Origin) y RequestDecision {Accepted, RejectedSessionMismatch, RejectedSessionDestroyed} (la
    comprobacion pura de sesion de INV-22 ya existe en WorkspaceSession/WorkspaceSessionRegistry); HintKind {ImpliedSelectionChanged,
    DatabaseObjectChanged, CommandEnded} y DrainOperation {ReadImplicitSelection, RecomputeSelectionContext,
    InvalidateNavigationCaches, MarkDraftsPossiblyStale} en Hints.cs: InvalidateNavigationCaches YA existe y el drenaje de
    WorkspaceSession la produce; DefinitionFact(bool IsInterpretable, string? EnvelopeId, string? ProbedId) y
    SelectionContext.Classify en SelectionContext.cs ya aplican la regla de D-05/D-06: Uno(R) solo desde un sobre interpretable con
    Id no vacio; un Id solo sondeado nunca crea identidad y una definicion ilegible es Diagnostic.
  * Autoridades integradas (fuera del alcance de escritura; solo se consumen): RackBlockFinder.ScanEnvelopes vive en el PLUGIN
    (src/RackCad.Plugin/RackBlockFinder.cs; omite layouts, anonimas y xref; devuelve tambien las definiciones cuyo sobre no se
    interpreta, con su recuento de referencias directas); T1 no puede llamarla: los hechos que T2 obtendra con ella entran en T1
    como valores neutrales. Puras y en Application: RackEnvelopeIdProbe.Probe(string json) (src/RackCad.Application/Views/Insertion/,
    ambito declarado = diagnostico), RackSiblingMembership.Classify (src/RackCad.Application/Views/Redraw/; pertenencia para MUTAR,
    D-11: no se usa para navegar), RackViewCodec (src/RackCad.Application/Systems/Shared/RackViewCodec.cs: TryDecodeViewKind
    reconoce solo "frontal", "lateral" y "planta" por OrdinalIgnoreCase con DimensionViewKind {Frontal, Lateral, Planta}; Decode(kind,
    view, section) es el codec total de View/Section) y RackEmbedStore/RackEmbedDocument (src/RackCad.Application/Persistence/).
  * IWorkspaceHostPort (puerto de F1) esta fijado por WorkspaceHostPortGuardTests: solo lecturas (nombres que empiezan por Get,
    Authorize, Is, Can o Try), sin eventos ni delegados, cada operacion con un unico argumento WorkspaceRequest, y declarado e
    implementado en un solo archivo cada uno. Ningun nombre que empiece por Set, Select, Zoom, Navigate, Execute, Apply, Refresh,
    Update, etc. cabe en ese puerto: T1 no lo modifica; la autoridad de navegacion se modela con tipos propios.
  * GUARDAS DE TEXTO que ya recorren TODO archivo .cs de src/RackCad.Application/Workspace/ (codigo sin comentarios; un archivo
    nuevo que las viole rompe pruebas de F1 que no se pueden tocar):
    - WorkspaceModelBoundaryTests: prohibe System.IO, File., Directory., StreamWriter, Microsoft.Win32, Registry., UserSettings,
      AtomicFile, la SUBCADENA Transaction (tambien dentro de un identificador), LockDocument, la palabra Database, Commit( y
      Autodesk.; en tipos del namespace RackCad.Application.Workspace prohibe nombres que contengan Tab, Subscription, Editor o Draft
      sin distinguir mayusculas (OJO: Stable, Table, Editable, Mutable, Notable contienen "tab"); en nombres de tipos y de miembros
      publicos prohibe ProjectSummary, Metric, Provider, TotalRacks, Aggregat y Bom (sin distinguir mayusculas).
    - WorkspaceHostSourceGuardTests y WorkspaceBridgeSourceGuardTests: prohiben las palabras SetImpliedSelection, SetCurrentView,
      SetSystemVariable, Zoom seguido de cualquier letra (Zoom, ZoomTo, ZoomExtents...), Regen, UpdateScreen, Highlight,
      Unhighlight, SendStringToExecute y PanTo; Autodesk.AutoCAD.Internal, PaletteSet, AddVisual, System.Windows, RackCad.UI,
      RackCad.Plugin, UserControl y Dispatcher; Task.Run, Task.Factory, new Thread, ThreadPool, Timer, async, await, Parallel.,
      BackgroundWorker y SynchronizationContext; System.IO, Registry, Xrecord, XData, DBDictionary, NamedObjectsDictionary,
      AtomicFile, UserSettings y Environment.SetEnvironmentVariable; RackLog y RackCad.Application.Diagnostics; Guid.NewGuid, new
      RackEmbedDocument, RackEmbedComposer y RackBlockData.Write; ningun tipo cuyo nombre contenga Log (tambien Catalog, Dialog,
      Logic); y, sin distinguir mayusculas, ComputedParameter, Metric, ProjectSummary, TotalRacks, Aggregat, Provider y Bom en el
      texto (tambien IFormatProvider o Symmetric).
    Las guardas de privacidad nuevas de INV-36 viven en tests/RackCad.Tests/Workspace/ (fuera de esas guardas) y pueden nombrar
    esos conceptos en sus patrones.
  * Guardas de usuarios exactos (recorren todo src/): WorkspaceSessionRegistry solo puede nombrarse en EventBridgeCore.cs y en su
    propio archivo (WorkspaceBridgeFacadeSourceGuardTests); new EventBridgeCore( solo en el puente del Plugin; en WorkspaceHostCore.cs
    hay recuentos exactos (_reader.Invalidate( una vez, .ApplyContext( una vez, WorkspaceSelectionMapper.ToContext( una vez, sin
    SelectionContext.One( ni SelectionContext.Selection(). Un archivo nuevo no nombra WorkspaceSessionRegistry ni EventBridgeCore, y
    un cambio en WorkspaceHostCore.cs no anade ninguna de esas llamadas.
  * Las pruebas de texto leen las fuentes desde la raiz del repositorio (RackCad.sln) con el ayudante WorkspaceHostSource
    (tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs: Code, Files, ApplicationFiles, PluginFiles, AllProduct, BodyOf, Count).
  * El CI rechaza avisos nuevos: los archivos de prueba llevan #nullable enable; una anotacion string? sin contexto nullable da
    CS8632. Equivalente local del job de Core: dotnet test tests/RackCad.Tests/RackCad.Tests.csproj (Release) sin avisos nuevos
    MSB/CS/NU/NETSDK. Los avisos xUnit2030/xUnit2031 de dos pruebas de F1 ya existen y no se corrigen aqui.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/R20261108T181429Z-adae/gate-contract.json son ASCII puro: los diacriticos se
  eliminaron a proposito (por ejemplo "unica", "politicas", "pestanas", "section 11"). Se copian byte a byte.
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective
  son DATOS OPACOS de ese contrato. No los copies a mano: CARGALOS desde el archivo con una orden y reproducelos en tu salida
  a partir de esa carga, por ejemplo con python desde pwsh:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/R20261108T181429Z-adae/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','AllowedWriteScope','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- Los valores Section de Authorities son la transliteracion ASCII de los encabezados (en Git los documentos conservan sus
  tildes y signos): copia el valor del contrato, nunca el encabezado del documento.
- TODA tu salida debe ser ASCII puro, tambien en los campos que redactas tu (TaskClass, RoutingReason, AcceptanceCriteria,
  IssuedBy): escribe sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun
  tabulador ni otro caracter de control dentro de las cadenas.
- Antes de emitir: comprueba que, al decodificar tu salida, cada uno de esos campos es identico (cadena por cadena) al del
  contrato, y que tu salida no contiene ningun byte no ASCII, ninguna barra invertida seguida de u y ningun tabulador; si no,
  corrige tu salida volviendo a usar la carga, nunca a mano.
- No estreches AllowedWriteScope (2 entradas) ni expandas ForbiddenWriteScope (17 entradas). RequiredTests con el mismo
  elemento (Project, Filter, MinSelected y ExpectRed).

TASKCLASS (decision del Coordinator)
- TaskClass = exactamente "Implementacion transversal a capas (corta)" (fila "Implementacion transversal a capas" de routing.md
  section 1, variante corta, en ASCII); PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason, con las
  siete dimensiones y la fecha de verificacion de la celda del contrato que uses. Si el alcance te resultara Long-horizon: para
  con F2-C07 explicado en RoutingReason.
- Celda del Worker (la unica con write-commit-push del contrato): Executor.Cell = "anthropic/subagent/claude-sonnet-5-5";
  Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high";
  Effort.Semantic = "Deep"; ModelEscalationReason = null. La celda openai/process/gpt-6-luna es la del Controller, no la del Worker.

DELTA DE LA TAREA (primera delegacion de la cadena F2-T1-NAVIGATION-CORE; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261108T181429Z-adae (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F2"; TaskId = "F2-T1-NAVIGATION-CORE" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60 (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (90f5872e...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261108T181429Z-adae" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker".
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5 congelada
  (D-03, D-05, D-06, D-07, INV-03, INV-22, INV-36, INV-38, INV-40), del contrato y de los HECHOS DEL REPOSITORIO de arriba; como
  minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de las 2 entradas de AllowedWriteScope; ningun archivo existente de prueba
  se modifica ni se borra (las pruebas nuevas van en archivos nuevos); cualquier cambio en un archivo de producto existente de
  src/RackCad.Application/Workspace/ es solo aditivo y no altera el comportamiento de F1;
  (2) RED primero: un commit RED con un esqueleto que compila y pruebas nuevas seleccionadas por FullyQualifiedName~Workspace
  (mas de 645 en total) que fallan por asercion del comportamiento de F2-T1 aun no implementado, sin fallo de compilacion ni
  prueba artificial, y las 645 existentes en verde; push del RED antes del GREEN; despues un commit GREEN que no toca los
  archivos de prueba del RED (16.8);
  (3) una sola autoridad pura NavigationTargets para (sesion, RackId) con la regla de miembros de D-05 (definiciones no xref,
  sobre interpretable, Id no vacio igual a RackId por OrdinalIgnoreCase; un Id solo sondeado da diagnostico y cero objetivos),
  objetivos = referencias de nivel superior de Model Space, orden determinista de D-07 (frontal, lateral, planta, el resto por el
  orden del codec y dentro de cada vista por handle), calculada bajo demanda a partir de hechos neutrales suministrados por un
  puerto puro que implementara F2-T2, sin enumeracion global retenida del documento (F2-C02);
  (4) cache por (sesion, RackId) con igualdad OrdinalIgnoreCase del RackId, sin persistencia, e invalidacion por las pistas
  pertinentes (DrainOperation.InvalidateNavigationCaches) y al destruir la sesion;
  (5) la decision pura de accion para Seleccionar, Ver, Centrar y Localizar vista con revalidacion previa (INV-40: documento o
  sesion distinta, objetivo borrado o desatribuido => no actuar, invalidar la cache y refrescar; cero efectos en el puerto
  simulado) y rechazo por sesion (INV-22, D-03), sin ninguna llamada real a AutoCAD;
  (6) INV-38: el contexto, las pestanas futuras y el navegador futuro obtienen los objetivos de la misma autoridad (prueba pura);
  (7) INV-36: guardas de fuente en tests/RackCad.Tests/Workspace/ que prueban que ningun archivo de src/ fuera de las carpetas
  Workspace del panel nombra los tipos de NavigationTargets ni sus hechos neutrales (incluidos StructuralAnalysis,
  ComputedParameters, ProjectSummary, BOM, TotalRacks y comandos ajenos al panel);
  (8) los nombres nuevos cumplen las GUARDAS DE TEXTO de arriba y todas las pruebas de F1 siguen en verde sin tocarlas;
  (9) en el commit GREEN: el filtro requerido y la suite Core completa pasan; la build Release de RackCad.Tests no emite avisos
  nuevos MSB/CS/NU/NETSDK;
  (10) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success;
  (11) worker-handoff.json conforme al esquema en ExpectedHandoffPath (ruta ignorada por Git) con RED, GREEN, alcance, trailer,
  conteos y mapa evidencia-por-invariante;
  (12) sin AutoCAD, sin Plugin, sin UI, sin Domain, sin pestanas, borradores ni inventario global.
- IssuedBy = "Controller Codex (planificacion F2-T1-NAVIGATION-CORE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
