I-64 - DELEGACION F1-T2-BRIDGE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena; planificacion 1 de 1 valida)
Identidad: unidad I-64; gate F1; tarea F1-T2-BRIDGE; intento 3; RunId R20261010T082206Z-21c1;
           DelegationRunId R20261010T082206Z-21c1; AuthorityRevision d5bdcb7aff31ba54901a3a0acdbfd81be92ab516;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha = el HEAD actual del worktree (se espera d5bdcb7aff31ba54901a3a0acdbfd81be92ab516);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261010T082206Z-21c1
Contrato de gate: artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T082206Z-21c1/gate-contract.json (ASCII puro; emitido por orden del Coordinator, decision custodiada en
           docs/automation/evidence/I-64-pilot/F1-T2-BRIDGE/coordinator-decision-gate-contract.md)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los del contrato (INV-F1-T2-01..14); una delegacion nunca amplia el contrato
Rutas y archivos calientes: carpetas nuevas src/RackCad.Plugin/Workspace/ (no existe) y archivos NUEVOS en
           src/RackCad.Application/Workspace/ y tests/RackCad.Tests/Workspace/; los 5 archivos de producto de T1 en
           src/RackCad.Application/Workspace/ son de solo lectura (prohibidos en el contrato). Fuente de diseno:
           docs/initiatives/I-64-proposal-v5.md (congelada) D-02, D-03, D-04, Sec.10 (INV-02, INV-03, INV-14, INV-21) y Sec.11 (F1);
           docs/initiatives/I-64-f1-t2-gate-contract-package.md; la decision del Coordinator citada arriba.
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Workspace (los 5 archivos de T1) ; git ls-files src/RackCad.Plugin/Workspace (vacio) ;
           git ls-files tests/RackCad.Tests/Workspace (las 4 pruebas de T1)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y C-01..C-17 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia Sec.38 a Sec.40 de docs/automation/evidence/I-64-evidence.md)
- F1-T1-MODEL (modelo puro de Workspace en Application) esta cerrada por el Coordinator como COMPLETE_UNDER_I64_SCOPE_BRIDGE.
  Sus 5 archivos de producto y sus pruebas existen en este HEAD; T1 no se reabre.
- La rama se rebaso sobre main bb0d5522 (I-63 integrada) y el producto de T1 se revalido (evidencia Sec.39).
- Decision del Coordinator para T2 (evidencia Sec.40): presupuesto de 1 salida valida por fase; Attempt 3, AttemptsRemaining 0,
  MaxReworkLoops 3; CorrectionsAuthorized false: toda entrega que exija correccion o REWORK es STOP S-11. Por eso la delegacion
  debe dejar al Worker todo lo necesario para acertar a la primera.
- HECHOS DEL REPOSITORIO medidos por la sesion (evidencia Sec.40), relevantes para AcceptanceCriteria:
  * tests/RackCad.Tests solo referencia Domain y Application (y el importador): no puede instanciar tipos del Plugin. El
    comportamiento con fuente de eventos simulada se prueba sobre tipos NUEVOS de src/RackCad.Application/Workspace/; el
    Plugin se comprueba con guardas de texto que leen sus fuentes desde la raiz del repositorio (RackCad.sln).
  * Las guardas de T1 (tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs) se aplican tambien a los archivos nuevos:
    el texto de codigo (sin comentarios) de todo src/RackCad.Application/Workspace/ (AllDirectories) no puede contener System.IO,
    File., Directory., StreamWriter, Microsoft.Win32, Registry., UserSettings, AtomicFile, Transaction, LockDocument, Database
    como palabra, Commit(, Autodesk., System.Windows, RackCad.Plugin ni RackCad.UI; ningun tipo del namespace exacto
    RackCad.Application.Workspace puede tener en el nombre Tab, Subscription, Editor o Draft, ni nombre o miembro publico con
    ProjectSummary, Metric, Provider, TotalRacks, Aggregat o Bom (sin distinguir mayusculas). Las pruebas de T1 no se modifican.
  * NamespaceFolderGuardTests: en src/ el namespace de cada archivo es la ruta de su carpeta (src/RackCad.Plugin/Workspace/X.cs ->
    RackCad.Plugin.Workspace) con exactamente un namespace de bloque (con llaves); en tests/RackCad.Tests el unico namespace raiz
    es RackCad.Tests (no RackCad.Tests.Workspace; ese error causo un REWORK en T1). El filtro FullyQualifiedName~RackCad.Tests.Workspace
    selecciona por subcadena: las clases de prueba nuevas deben llamarse Workspace... para quedar seleccionadas.
  * Cada archivo de prueba lleva #nullable enable (o enable annotations) si usa anotaciones ?: el aviso CS8632 rompio el CI de T1.
    El job de CI "Build Plugin without AutoCAD" falla ante cualquier aviso nuevo del Plugin. Las builds Release no deben
    emitir avisos nuevos MSB/CS/NU/NETSDK.
  * El Plugin actual no tiene ninguna suscripcion a eventos de AutoCAD: la guarda de INV-F1-T2-02 no necesita excepciones.
  * La API publica del modelo de T1 que el puente usa: WorkspaceSessionRegistry (GetOrCreate(long), TryGet, Destroy,
    Authorize), WorkspaceSession (EnqueueHint(HintKind), TryDrain(DrainConditions), ApplyContext), HintKind, DrainConditions
    (IsQuiescent, ModalRackCadActive, PanelVisible), DrainResult, SessionId, RackKey, WorkspaceRequest y RequestDecision.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T082206Z-21c1/gate-contract.json son ASCII puro: los diacriticos se
  eliminaron a proposito (por ejemplo "sesion", "Seleccion", "planificacion", "ambiguedad"). Se copian byte a byte.
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective
  son DATOS OPACOS de ese contrato. No los copies a mano: CARGALOS desde el archivo con una orden y reproducelos en tu salida
  a partir de esa carga, por ejemplo con python desde pwsh:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T082206Z-21c1/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','AllowedWriteScope','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective']}, ensure_ascii=True))"
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
- No estreches AllowedWriteScope (3 entradas) ni expandas ForbiddenWriteScope (42 entradas). RequiredTests con el mismo
  Project, Filter, MinSelected y ExpectRed.

TASKCLASS (decision del Coordinator)
- TaskClass = exactamente "Implementacion transversal a capas (corta)" (fila "Implementacion transversal a capas" de routing.md
  Sec.1, variante corta, en ASCII); PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason, con las
  siete dimensiones y la fecha de verificacion de la entrada del catalogo que uses.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "high"; Effort.Semantic = "Deep";
  ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F1-T2-BRIDGE; AUTOMATION_PLAN 16.6 y 16.8)
- RunId = R20261010T082206Z-21c1 (tambien es el DelegationRunId); Attempt = 3; AttemptsRemaining = 0; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T2-BRIDGE" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (d5bdcb7a...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T2-BRIDGE/3/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261010T082206Z-21c1" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5 congelada
  (D-02, D-03, D-04, Sec.10), del contrato, de la decision del Coordinator y de los HECHOS DEL REPOSITORIO de arriba; como minimo:
  (1) git diff --name-only BaseSha..CurrentSha dentro de AllowedWriteScope: solo archivos NUEVOS en src/RackCad.Application/Workspace/
  (ninguno de los 5 de T1), src/RackCad.Plugin/Workspace/ y tests/RackCad.Tests/Workspace/; las pruebas de T1 sin cambios;
  (2) RED primero: pruebas compilables, seleccionadas por el filtro del contrato (>= 1), que fallan por asercion de al menos un
  comportamiento congelado de T2 aun no implementado, contra un esqueleto; sin fallo de compilacion ni prueba artificial; despues
  GREEN sin tocar los archivos de prueba del RED (16.8);
  (3) nucleo del puente puro y sin AutoCAD en archivos nuevos de Application, probado con una fuente de eventos simulada:
  suscripciones = funcion exacta de los documentos con sesion en los ciclos de INV-F1-T2-01 (cero tras la destruccion),
  manejadores que solo encolan con la identidad de su sesion, drenaje diferido y coalescido solo en reposo y sin modal RackCad
  activo, Idle solo con pistas pendientes, BeginDocumentClose vivo toda la sesion tambien degradada, degradacion local a la
  sesion; ningun evento escribe seleccion o vista;
  (4) el unico suscriptor real (+= sobre eventos de AutoCAD) vive en src/RackCad.Plugin/Workspace/; nada se suscribe al cargar el
  plugin; sin cambios en PluginInitializer, comandos, csproj ni sln; sin puerto de host/UI;
  (5) guardas de texto en tests/RackCad.Tests/Workspace/ para INV-F1-T2-02, 07, 08, 12 y 14 sobre src/RackCad.Plugin/Workspace/;
  (6) convenciones del repositorio de los HECHOS DEL REPOSITORIO (namespaces, nombres de clases de prueba, #nullable, guardas de T1);
  (7) en el commit GREEN: el filtro del contrato y la suite Core completa pasan; las builds Release no emiten avisos nuevos
  MSB/CS/NU/NETSDK; el Plugin compila sin avisos nuevos;
  (8) la CI exacta del CurrentSha tiene los cuatro jobs requeridos en success;
  (9) sin AutoCAD, sin cargar el DLL, sin perfil, registro, TRUSTEDPATHS, SECURELOAD ni ACL; sin Smoke-1.
- IssuedBy = "Controller Codex (planificacion F1-T2-BRIDGE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
