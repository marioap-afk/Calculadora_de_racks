I-64 - DELEGACION F1-T3A-HOST-CORE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F1; tarea F1-T3A-HOST-CORE; intento 3; RunId R20261011T043044Z-ed7b;
           DelegationRunId R20261011T043044Z-ed7b; AuthorityRevision 42d45776c695ed32987c08213532a9f75edb4a9a;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree (se espera 42d45776c695ed32987c08213532a9f75edb4a9a);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261011T043044Z-ed7b
Contrato de gate: artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T043044Z-ed7b/gate-contract.json (ASCII puro; emitido por orden del Coordinator, decision custodiada en
           docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-decision-t3-decomposition.md)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los del contrato (INV-F1-T3A-01..18); una delegacion nunca amplia el contrato
Rutas y archivos calientes: solo archivos NUEVOS en src/RackCad.Application/Workspace/, src/RackCad.Plugin/Workspace/ y
           tests/RackCad.Tests/Workspace/, mas la modificacion acotada de tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs
           y de los dos censos de comandos (tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs y
           tests/RackCad.Tests/SelectiveEditorOpenTests.cs) en los terminos del contrato. Los 10 archivos de producto de T1/T2 y
           los demas archivos de prueba de T1/T2 son de solo lectura. Fuente de diseno: docs/initiatives/I-64-proposal-v5.md
           (congelada) D-01, D-02, D-03, D-04, D-06, D-17, D-18, D-22, Sec.10 y Sec.11; docs/initiatives/I-64-f1-t3-gate-contract-package.md;
           la decision del Coordinator citada arriba.
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Workspace (9 archivos de T1/T2) ; git ls-files src/RackCad.Plugin/Workspace (1 archivo de T2) ;
           git ls-files tests/RackCad.Tests/Workspace (9 archivos de T1/T2)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y C-01..C-16 y C-T3-INSTRUMENTATION del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" en el texto libre
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.46 a Sec.49 de docs/automation/evidence/I-64-evidence.md)
- F1-T1-MODEL y F1-T2-BRIDGE estan cerradas por el Coordinator; su producto existe en este HEAD y no se reabre.
- El Coordinator descompuso F1-T3-HOST en F1-T3A-HOST-CORE (esta tarea: base no visual y comprobable) y F1-T3B-HOST-UI
  (PaletteSet, AddVisual, UserControl raiz, view-model, foco, contencion de UI; NO se emite todavia). T3A no contiene PaletteSet
  ni WPF ni cambios en src/RackCad.UI.
- Presupuesto de T3A (evidencia Sec.49): 1 salida valida por fase; Attempt 3, AttemptsRemaining 0, MaxReworkLoops 3;
  CorrectionsAuthorized false: toda entrega que exija correccion o REWORK es STOP S-11. Por eso la delegacion debe dejar al
  Worker todo lo necesario para acertar a la primera.
- HECHOS DEL REPOSITORIO medidos por la sesion (paquete de T3, Sec.3), relevantes para AcceptanceCriteria:
  * tests/RackCad.Tests solo referencia Domain y Application (y el importador): no puede instanciar tipos del Plugin. Las politicas
    se prueban sobre tipos NUEVOS de src/RackCad.Application/Workspace/ con host y lector simulados; el Plugin se comprueba con
    guardas de texto que leen sus fuentes desde la raiz del repositorio (RackCad.sln).
  * API de T1/T2 disponible sin tocarla: AutoCadEventBridge (Start, SetPanelVisible, SetModalRackCadActive, evento Drained con
    DrainOutcome, Dispose); EventBridgeCore (ActiveSession, TryGetSession, IsDegraded, LiveSessionCount); WorkspaceSession
    (Id, Selection, ApplyContext(SelectionContext)); SelectionContext.Classify(IReadOnlyList<DefinitionFact>, int) y
    DefinitionFact(IsInterpretable, EnvelopeId, ProbedId); WorkspaceSessionRegistry.Authorize(WorkspaceRequest, long).
  * Lector integrado de sobres (el mismo de RackCommandSupport.PickRackBlock): RackBlockData.Read(transaction, definitionId)
    devuelve el JSON del sobre y new RackEmbedStore().Deserialize(json) el RackEmbedDocument. No crear un codec nuevo.
  * Guarda de T2 tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs (texto sin comentarios, literales incluidos):
    NothingIsHookedWhenThePluginLoadsAndNoCommandOrInitializerIsAdded prohibe [CommandMethod en TODO src/RackCad.Plugin/Workspace/;
    TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs prohibe SelectImplied, GetObject,
    OpenCloseTransaction, MessageBox, Zoom, Regen, etc. en TODO src/RackCad.Plugin/Workspace/;
    NoExistingPluginCommandFileReferencesTheWorkspaceBridge prohibe que un archivo del Plugin FUERA de Workspace/ referencie
    RackCad.Plugin.Workspace o RackCad.Application.Workspace; ThePluginWorkspaceDeclaresNoFieldOf... prohibe campos de tipo
    Transaction|OpenCloseTransaction|DBObject|Document|Entity|BlockReference; ThePublicApiOfTheBridgeExposesNoAutoCadType prohibe
    lineas public/protected que nombren Document|Editor|Database|ObjectId|Entity|Autodesk o *EventArgs; TheBridgeHasNoPersistence
    prohibe System.IO, File., Microsoft.Win32, Registry., .Commit(, Save(, DocumentExtension; TheBridgeFilesCarry...Concept prohibe
    ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|Bom (sin distinguir mayusculas) en Plugin/Workspace y
    Application/Workspace; NoBridgeFileUsesTheInternalAutoCadNamespace prohibe Autodesk.AutoCAD.Internal. La decision del
    Coordinator solo autoriza dos excepciones de ARCHIVO EXACTO (los dos archivos nombrados en el contrato); todo lo demas sigue.
  * Censos de comandos: CustomPropertiesCommandGuardTests (TGrd08: lista exacta Baseline) y
    SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA (Assert.Equal(37, total) contando "[CommandMethod(" en
    src/RackCad.Plugin/**/*.cs). Precedente: I-55 G15 anadio RACKPROYECTAR/RPY pasando de 35 a 37.
  * Guardas de T1 (tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs) sobre todo src/RackCad.Application/Workspace/
    (AllDirectories, texto sin comentarios): prohibido System.IO, File., Directory., StreamWriter, Microsoft.Win32, Registry.,
    UserSettings, AtomicFile, Transaction (como subcadena), LockDocument, Database como palabra, Commit(, Autodesk., System.Windows,
    RackCad.Plugin y RackCad.UI; ningun tipo del namespace RackCad.Application.Workspace puede tener en el nombre Tab, Subscription,
    Editor o Draft (sin distinguir mayusculas; Tab tambien casa con Table, Mutable o Database), ni nombre o miembro publico con
    ProjectSummary, Metric, Provider, TotalRacks, Aggregat o Bom.
  * RackLog (src/RackCad.Application/Diagnostics/RackLog.cs, I-03, PROHIBIDO) solo tiene Exception(context, ex) y
    Warning(context, message); no tiene nivel de informacion ni de tiempos. Registrar una medida normal como aviso o excepcion
    abusaria de su semantica: la instrumentacion de T3A queda en memoria (STOP C-T3-INSTRUMENTATION si hiciera falta el registro).
  * NamespaceFolderGuardTests: en src/ el namespace de cada archivo es la ruta de su carpeta (src/RackCad.Plugin/Workspace/X.cs ->
    RackCad.Plugin.Workspace) con exactamente un namespace de bloque (con llaves); en tests/RackCad.Tests el unico namespace raiz
    es RackCad.Tests. El filtro FullyQualifiedName~RackCad.Tests.Workspace selecciona por subcadena: las clases de prueba nuevas
    deben llamarse Workspace... para quedar seleccionadas (hoy 159 seleccionadas, todas en verde).
  * Cada archivo de prueba lleva #nullable enable (o enable annotations) si usa anotaciones ?: el aviso CS8632 rompio el CI de T1.
    El job de CI "Build Plugin without AutoCAD" falla ante cualquier aviso nuevo del Plugin; las builds Release no deben emitir
    avisos nuevos MSB/CS/NU/NETSDK. Los proyectos son SDK y compilan los archivos nuevos sin tocar ningun csproj.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T043044Z-ed7b/gate-contract.json son ASCII puro: los diacriticos se
  eliminaron a proposito (por ejemplo "sesion", "Seleccion", "planificacion", "ambiguedad"). Se copian byte a byte.
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective
  son DATOS OPACOS de ese contrato. No los copies a mano: CARGALOS desde el archivo con una orden y reproducelos en tu salida
  a partir de esa carga, por ejemplo con python desde pwsh:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T043044Z-ed7b/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','AllowedWriteScope','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- Los valores Section de Authorities son la transliteracion ASCII de los encabezados (en Git los documentos conservan sus
  tildes): copia el valor del contrato, nunca el encabezado del documento.
- TODA tu salida debe ser ASCII puro, tambien en los campos que redactas tu (TaskClass, RoutingReason, AcceptanceCriteria,
  IssuedBy): escribe sin tildes ni signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun
  tabulador ni otro caracter de control dentro de las cadenas. La unica excepcion son los saltos de linea que ya trae el
  Objective del contrato (en JSON, barra invertida + n).
- Antes de emitir: comprueba que, al decodificar tu salida, cada uno de esos campos es identico (cadena por cadena) al del
  contrato, y que tu salida no contiene ningun byte no ASCII, ninguna barra invertida seguida de u y ningun tabulador; si no,
  corrige tu salida volviendo a usar la carga, nunca a mano.
- No estreches AllowedWriteScope (5 entradas) ni expandas ForbiddenWriteScope (92 entradas). RequiredTests con el mismo
  Project, Filter, MinSelected y ExpectRed.

TASKCLASS (decision del Coordinator)
- TaskClass = exactamente "Implementacion transversal a capas (corta)" (fila "Implementacion transversal a capas" de routing.md
  Sec.1, variante corta, en ASCII); PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason, con las
  siete dimensiones y la fecha de verificacion de la entrada del catalogo que uses.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "high"; Effort.Semantic = "Deep";
  ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F1-T3A-HOST-CORE; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261011T043044Z-ed7b (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T3A-HOST-CORE" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (42d45776...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261011T043044Z-ed7b" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5 congelada
  (D-01, D-02, D-03, D-04, D-06, D-17, D-18, D-22, Sec.10), del contrato, de la decision del Coordinator y de los HECHOS DEL
  REPOSITORIO de arriba; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de AllowedWriteScope: solo archivos NUEVOS en src/RackCad.Application/Workspace/,
  src/RackCad.Plugin/Workspace/ y tests/RackCad.Tests/Workspace/, mas los tres archivos de prueba existentes autorizados; los 10
  archivos de producto de T1/T2 y las demas pruebas de T1/T2 sin cambios;
  (2) RED primero: pruebas compilables, seleccionadas por el filtro del contrato (>= 1), que fallan por asercion de al menos un
  comportamiento congelado de T3A aun no implementado, contra un esqueleto; sin fallo de compilacion ni prueba artificial; despues
  GREEN sin tocar los archivos de prueba del RED (16.8);
  (3) el comando RACKPANEL y el lector de seleccion viven EXACTAMENTE en los dos archivos que nombra el contrato; la guarda de T2
  solo gana esas dos excepciones de archivo exacto y sigue prohibiendo todo lo demas; los censos solo +RACKPANEL y 37 -> 38;
  (4) politicas puras en archivos nuevos de Application probadas con host y lector simulados: controlador perezoso unico, RACKPANEL
  repetido sin duplicados, autorizacion por SessionId (peticion de otra sesion rechazada sin efectos), puerto sin escrituras, panel
  oculto con cero lecturas y reconciliacion al mostrar, ocioso sin trabajo, estado vacio, mapeo de la seleccion a los cinco
  contextos con Classify/ApplyContext;
  (5) instrumentacion en memoria (interruptor, tiempos, memoria del proceso, puntos de medida estructurados) sin tocar RackLog.cs
  y sin registrar medidas normales como aviso o excepcion;
  (6) convenciones del repositorio de los HECHOS DEL REPOSITORIO (namespaces, nombres de clases de prueba, #nullable, guardas de T1/T2);
  (7) en el commit GREEN: el filtro del contrato y la suite Core completa pasan; las builds Release no emiten avisos nuevos
  MSB/CS/NU/NETSDK; el Plugin compila sin avisos nuevos;
  (8) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success;
  (9) sin PaletteSet, WPF ni cambios en RackCad.UI; sin AutoCAD, sin cargar el DLL, sin perfil, registro, TRUSTEDPATHS, SECURELOAD
  ni ACL; sin Smoke-1.
- IssuedBy = "Controller Codex (planificacion F1-T3A-HOST-CORE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
