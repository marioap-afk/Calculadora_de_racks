I-64 - DELEGACION F2-T1-NAVIGATION-CORE - Worker, perfil ROUTINE_IMPLEMENTATION (primera delegacion de la cadena; una sola invocacion del Worker para esta delegacion)
Identidad: unidad I-64; gate F2; tarea F2-T1-NAVIGATION-CORE; intento 1; RunId R20261108T182756Z-ec7e;
           DelegationRunId R20261108T181429Z-adae; AuthorityRevision 90f5872e89660655a93606e16d50f4bc19aa3c9c;
           MainSha a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60; BaseSha 9082bcbcefbf33ca1f9540c34e6f0e96e20cdcfb;
           rama architecture/workspace-persistente-rackcad;
           worktree <worktree-de-I-64>;
           Owner subagent:R20261108T181429Z-adae
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | D-03, D-05, D-06, D-07; invariantes INV-03, INV-22, INV-36, INV-38 e INV-40; section 11 F2; section 12.3 Smoke-NAV; section 13 routing de F2 | UNIT_DOC
           - docs/automation/state/I-64.yml | estado vigente completo; en especial F1/Smoke-1 PASS, f1_rebase_i65, gate post-I65 CLOSED, Snapshot D y persistent_next_action | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/POST-I65-HOST-SANITY/R20261108T065253Z-aa47/disposition/coordinator-post-i65-host-sanity-review.md | archivo completo: aa47 PASS, P-01 CLEARED y F2 MAY START | UNIT_DOC
           - docs/AUTOMATION_PLAN.md | section 16.1 a section 16.8 | EXTERNAL
           - docs/automation/agent-execution/routing.md | archivo completo | EXTERNAL
           - docs/automation/agent-execution/model-catalog.md | celdas medidas y vigentes para Controller y Worker | EXTERNAL
           - docs/WORKFLOW.md | relevo, Git, rebase/integracion y prohibicion de integracion automatica | EXTERNAL
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
           Los valores de seccion son ASCII (sin diacriticos ni signo de seccion); en Git los encabezados conservan sus tildes y signos.
Objetivo (texto literal del contrato):
           Implementar la primera tarea de F2 - Context Navigation: la autoridad unica NavigationTargets y las politicas puras necesarias para navegar desde el rack del contexto, sin AutoCAD, sin UI host y sin inventario global. Para (sesion, RackId), NavigationTargets obtiene bajo demanda y cachea el conjunto determinista de objetivos navegables conforme a D-05: solo definiciones no-xref con sobre interpretable e Id no vacio igual por OrdinalIgnoreCase; nunca miembros atribuidos solo por sondeo; referencias top-level de Model Space; orden frontal, lateral, planta, resto por codec y luego handle. Debe existir una sola derivacion consumible posteriormente por contexto, pestanas y navegador. Debe modelarse la invalidacion de la cache y la politica pura de acciones/revalidacion necesaria para que F2-T2 pueda ejecutar Seleccionar, Ver, Centrar y Localizar sin introducir una segunda autoridad. Esta tarea NO implementa todavia llamadas reales a AutoCAD, cambios de seleccion/vista, controles WPF, pestanas F3, inventario global F6 ni Smoke-NAV.
Alcance: permitido EXACTAMENTE src/RackCad.Application/Workspace/, tests/RackCad.Tests/Workspace/ (prefijos terminados en /);
           prohibido src/RackCad.Plugin/, src/RackCad.UI/, src/RackCad.Domain/, src/RackCad.Application/StructuralAnalysis/, assets/, deploy/, eng/, tools/, tests/RackCad.UI.Tests/, tests/RackCad.Tests/StructuralAnalysis/, RackCad.sln, Directory.Build.props, Directory.Build.targets, global.json, NuGet.Config, README.md, docs/.
Invariantes del Freeze (vinculantes en su totalidad):
           - D-03: toda clave y peticion de navegacion queda ligada a la instancia de sesion del documento; nunca por ruta, nombre o titulo; una peticion de A no puede actuar sobre B.
           - D-05: NavigationTargets es la unica autoridad de objetivos de navegacion para (sesion, RackId); F2 implementa la entrada desde contexto y deja la misma autoridad preparada para F3/F6.
           - D-05: miembros navegables son solo definiciones no-xref cuyo sobre se interpreta y cuyo Id no vacio coincide con RackId por OrdinalIgnoreCase; un Id obtenido solo por RackEnvelopeIdProbe sirve exclusivamente para diagnostico y nunca crea un objetivo.
           - D-05: los objetivos son referencias top-level de Model Space y su orden es determinista: frontal, lateral, planta, demas vistas por orden del codec y dentro de cada vista por handle.
           - D-05: NavigationTargets se calcula bajo demanda, se cachea por (sesion, RackId) y se invalida por las pistas pertinentes; no se persiste.
           - D-05/INV-36: NavigationTargets y sus hechos neutrales son privados del panel; ningun codigo de metricas, ProjectSummary, BOM, TotalRacks, agregacion, listas o comandos ajenos al panel puede consumirlos; no se crea una API/fundacion compartida ni dependencia de I-63.
           - D-05: antes de F6 no se introduce enumeracion global permanente del documento ni navegador document-wide; F2 resuelve objetivos para el rack solicitado bajo demanda.
           - D-07: F2 prepara las acciones Seleccionar, Ver, Centrar y Localizar vista sin escritura del authored; la ejecucion real en AutoCAD pertenece a F2-T2.
           - INV-03: ningun evento ni cambio de contexto escribe seleccion o vista; el eco se reconocera por contenido, nunca por la nocion de 'siguiente evento'.
           - INV-22: una peticion ligada a una sesion distinta del documento ejecutor se rechaza sin efectos.
           - INV-38: el mismo rack debe producir el mismo conjunto NavigationTargets desde cualquier entrada futura; F2 no puede crear una derivacion exclusiva del contexto que luego compita con pestanas o navegador.
           - INV-40: la politica de accion debe admitir revalidacion justo antes de ejecutar: documento, existencia de objetivos y atribucion; fallo => cero accion, invalidacion de cache y refresco. T1 modela la decision pura; T2 implementa el adaptador host.
           - No se modifica RACKEDITAR, RACKPANEL, RACKRAMPAQUETE ni ningun comando clasico.
           - No se introduce estado de borrador, dirty, pestanas, preview, edicion, Actualizar, inventario global F6 ni persistencia propia.
Celda: anthropic/subagent/claude-sonnet-5-5; modelo claude-sonnet-5-5; effort high (Deep); clase "Implementacion transversal a capas (corta)"; RoutingEnforcement required
Criterios de aceptacion (del paquete aceptado):
           - git diff --name-only BaseSha..CurrentSha queda dentro de AllowedWriteScope; ningun archivo de prueba existente se modifica o elimina y las pruebas nuevas se agregan en archivos nuevos. Todo cambio en archivos existentes de producto Workspace es aditivo y conserva el comportamiento de F1.
           - Un commit RED contiene el esqueleto que compila y pruebas F2 nuevas que fallan por asercion de comportamiento no implementado; FullyQualifiedName~Workspace selecciona mas de 645 pruebas, las 645 existentes pasan, y no hay fallo de compilacion ni prueba artificial. El RED se empuja antes del GREEN.
           - El commit GREEN no modifica ningun archivo de prueba incluido en RED.
           - Existe una sola autoridad pura NavigationTargets para sesion y RackId. Solo incluye referencias top-level de Model Space a definiciones no-xref con sobre interpretable e Id no vacio igual a RackId por OrdinalIgnoreCase; un Id solo sondeado genera diagnostico y cero objetivos.
           - Los objetivos se calculan bajo demanda a partir de hechos neutrales del puerto puro previsto para F2-T2, sin enumeracion global retenida del documento; el orden es frontal, lateral, planta, resto por orden del codec y handle dentro de cada vista.
           - La cache se identifica por sesion y RackId con comparacion OrdinalIgnoreCase para RackId, no persiste y se invalida por las pistas pertinentes, incluida DrainOperation.InvalidateNavigationCaches, y al destruir la sesion.
           - La politica pura decide Seleccionar, Ver, Centrar y Localizar vista tras revalidar sesion/documento, existencia y atribucion; objetivo borrado o desatribuido implica no actuar, invalidar y refrescar, con cero efectos en el puerto simulado. Peticiones de otra sesion se rechazan sin efectos.
           - Pruebas puras demuestran que el contexto obtiene objetivos de NavigationTargets, dejando la misma autoridad para pestanas y navegador futuros.
           - Guardas nuevas en tests/RackCad.Tests/Workspace prueban que ningun archivo src/ fuera de Workspace consume NavigationTargets ni sus hechos neutrales, incluyendo StructuralAnalysis, ComputedParameters, ProjectSummary, BOM, TotalRacks y comandos ajenos al panel.
           - Los nombres nuevos cumplen las guardas de texto existentes y todas las pruebas F1 siguen verdes sin modificar sus archivos.
           - En GREEN, FullyQualifiedName~Workspace y la suite Core completa pasan; build Release de RackCad.Tests sin avisos nuevos MSB/CS/NU/NETSDK.
           - La CI de push del CurrentSha exacto tiene los cuatro jobs requeridos en success.
           - worker-handoff.json cumple el esquema en ExpectedHandoffPath, ruta ignorada por Git, e incluye RED, GREEN, alcance, trailer, conteos y mapa evidencia-por-invariante.
           - No se implementan AutoCAD, Plugin, UI, Domain, pestanas, borradores ni inventario global.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~Workspace, minimo 645, RED esperado: si
           evidencia esperada del contrato: Commit RED y push propios antes del GREEN. El RED compila y falla por aserciones nuevas de F2, no por errores de compilacion. Debe incluir casos de NavigationTargets, orden determinista, cache/invalidation, probe-solo-diagnostico, aislamiento por sesion, privacidad INV-36, autoridad unica INV-38 y politica de revalidacion INV-40.; El oraculo RED debe demostrar que el comportamiento de F2 no existia antes del producto; las pruebas F1 existentes permanecen verdes salvo las nuevas aserciones intencionalmente RED.; Commit GREEN y push con solo cambios dentro del AllowedWriteScope. Ningun archivo del RED se debilita o elimina para obtener GREEN.; Workspace Core completo en verde y Core Full completo en verde sobre el GREEN.; Guardas de privacidad que demuestren que NavigationTargets no es consumido por StructuralAnalysis, metricas, ProjectSummary, BOM, TotalRacks, comandos ajenos al panel ni I-63.; Pruebas puras de INV-38: el contexto usa la misma autoridad NavigationTargets que quedara disponible para las entradas futuras; un miembro ilegible con Id solo sondeable produce diagnostico pero cero objetivo.; Pruebas puras de INV-40: objetivo borrado, desatribuido o de otra sesion entre resolucion y ejecucion => decision de no actuar e invalidar la cache; cero efecto de seleccion/zoom en el puerto simulado.; worker-handoff.json conforme al esquema con SHA RED, SHA GREEN, alcance real, trailer del Worker, conteos RED/GREEN y mapa evidencia-por-invariante.; CI push del SHA exacto GREEN con los cuatro jobs en success antes de que la entrega pueda ser considerada para verificacion.
           La sesion, no tu, mantiene evidencia y estado (docs/ esta prohibido) y ejecuta la CI, la verificacion y los controles.
No-touch: todo lo prohibido arriba; el checkout principal <checkout-principal> (nunca escribas alli);
           las ramas de otras iniciativas y main; AutoCAD: no lo abras, no lo cierres, no cargues DLL, no toques perfil, TRUSTEDPATHS,
           SECURELOAD, ACL ni registro.
Condiciones de parada: las 25 del contrato, en especial:
           F2-C01: NavigationTargets would require a second rack-identity authority, a new persisted identity or use of probe-only Id as navigable identity
           F2-C02: implementation requires document-wide inventory/list/search behavior reserved for F6
           F2-C03: implementation requires tabs, preview, pinning or Browser Session behavior reserved for F3
           F2-C04: implementation requires drafts, dirty, editing, Actualizar or any DWG mutation reserved for F4+
           F2-C05: implementation requires consuming I-63 or exposing NavigationTargets as reusable/public authority outside the panel
           F2-C06: implementation requires touching Plugin/UI/Domain or any path outside the T1 AllowedWriteScope; return for F2-T2 or contract change
           F2-C07: a new material architecture/authority question appears or INV-38/INV-40 cannot be represented under the frozen D-05/D-07 semantics
           F2-C08: existing F1/C1/C2 behavior or I-65 StructuralAnalysis behavior regresses
           Ante cualquiera: parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions). Una correccion exige una delegacion nueva: no se repara nada fuera de este paso.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/R20261108T182756Z-ec7e/worker-handoff.json;
           Worker: commits y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (delegacion aceptada: A1-A8 y nc4 PASS; primera delegacion de la cadena)
- ChainBaseSha = BaseSha = 9082bcbcefbf33ca1f9540c34e6f0e96e20cdcfb; ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED en el proyecto requerido
  (tests/RackCad.Tests). Attempt 1; AttemptsRemaining 2; CorrectionsAuthorized true; esta es la unica invocacion del Worker para esta
  delegacion: entrega lo correcto a la primera.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal) o usa git -C <worktree>. dotnet: usa SIEMPRE <perfil-de-usuario>/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  <perfil-de-usuario>\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) y logs de build solo bajo artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/R20261108T182756Z-ec7e/tests/<RunRef>/.
  Edita con la herramienta de edicion o de escritura, no con heredocs de shell.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha, git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha y git rev-parse origin/main (tras git fetch) = MainSha.
  Si no, parada (S-12/S-13).
- QUE CONSTRUIR (solo lo que fijan el contrato y la Proposal V5 congelada; los nombres y la forma concreta los eliges tu dentro de
  las guardas de abajo, y los describes en la entrega):
  * Una sola autoridad pura NavigationTargets en src/RackCad.Application/Workspace/ que, para (sesion, RackId), resuelve bajo demanda
    los objetivos de navegacion a partir de hechos NEUTRALES que le entrega un puerto puro (una interfaz en la misma carpeta que en
    F2-T2 implementara el Plugin con RackBlockFinder.ScanEnvelopes; en T1 solo existe en pruebas un puerto simulado). Hechos
    neutrales de D-05: por definicion, su clave estable (handle), si es xref, si el sobre se interpreta, el Id tal como figura en el
    sobre (sin Id = IsNullOrWhiteSpace), el Id sondeado (solo diagnostico), Kind, View, Section y Name, y los handles de sus
    referencias de nivel superior en Model Space. Regla de miembros: definiciones no xref, sobre interpretable e Id no vacio igual al
    RackId por OrdinalIgnoreCase; un Id solo sondeado produce un diagnostico de miembro ilegible y CERO objetivos. Objetivos = las
    referencias de nivel superior de Model Space de los miembros; orden determinista: frontal, lateral, planta (RackViewCodec /
    DimensionViewKind), el resto de vistas despues por el orden del codec, y dentro de cada vista por handle. Una sola derivacion:
    el contexto (F2) y las entradas futuras (pestanas F3, navegador F6) deben poder pedir lo mismo a la misma autoridad (INV-38).
  * Cache por (sesion, RackId) con igualdad OrdinalIgnoreCase del RackId (no uses la igualdad de RackKey, que es ordinal), en
    memoria, sin persistencia, invalidada por las pistas pertinentes (DrainOperation.InvalidateNavigationCaches) y al destruir la
    sesion; sin enumeracion global retenida del documento.
  * La decision pura de accion para Seleccionar, Ver, Centrar y Localizar vista (sin llamadas a AutoCAD y sin nombres prohibidos):
    rechazo sin efectos de una peticion de otra sesion (INV-22/D-03, reutilizando WorkspaceRequest y RequestDecision si encajan) y
    revalidacion justo antes de ejecutar (INV-40): documento o sesion distinta, objetivo borrado o desatribuido => no actuar,
    invalidar la cache y refrescar; con un puerto simulado de efectos, cero llamadas de seleccion o de vista en esos casos.
  * Nada de eventos que escriban seleccion o vista (INV-03); nada de pestanas, preview, borradores, edicion, Actualizar ni
    inventario global; no toques IWorkspaceHostPort (puerto de solo lectura de F1 fijado por WorkspaceHostPortGuardTests).
- Hechos medidos del repositorio que debes respetar (inventario de la sesion):
  * tests/RackCad.Tests referencia Domain y Application (no el Plugin ni la UI). src/RackCad.Application/Workspace/ tiene 17 archivos,
    todos con #nullable enable y namespace de bloque RackCad.Application.Workspace (NamespaceFolderGuardTests). Las pruebas de
    tests/RackCad.Tests/Workspace/ declaran namespace RackCad.Tests (file-scoped) y #nullable enable; sus clases se llaman
    Workspace... para que FullyQualifiedName~Workspace las seleccione (hoy 645 en Core, todas en verde).
  * Tipos existentes: SessionId(long) y RackKey(SessionId Session, string RackId) en SessionId.cs; WorkspaceRequest(SessionId
    Origin) y RequestDecision {Accepted, RejectedSessionMismatch, RejectedSessionDestroyed}; HintKind y DrainOperation en Hints.cs
    (InvalidateNavigationCaches ya existe y WorkspaceSession la produce en el drenaje); DefinitionFact(bool IsInterpretable, string?
    EnvelopeId, string? ProbedId) y SelectionContext.Classify en SelectionContext.cs (ya aplican "sondeo solo diagnostico").
    Puras en Application y consumibles sin cambios: RackViewCodec y DimensionViewKind (src/RackCad.Application/Systems/Shared/),
    RackEnvelopeIdProbe (Views/Insertion), RackEmbedStore/RackEmbedDocument (Persistence). RackSiblingMembership.Classify es para
    MUTAR (D-11): no se usa para navegar. RackBlockFinder.ScanEnvelopes esta en el Plugin: no se llama desde T1.
  * GUARDAS DE TEXTO que ya recorren TODO .cs de src/RackCad.Application/Workspace/ (codigo sin comentarios); no las toques y no
    las rompas:
    - WorkspaceModelBoundaryTests: sin System.IO, File., Directory., StreamWriter, Microsoft.Win32, Registry., UserSettings, AtomicFile,
      la SUBCADENA Transaction (tambien dentro de un identificador), LockDocument, la palabra Database, Commit( ni Autodesk.; ningun
      tipo del namespace cuyo nombre contenga Tab, Subscription, Editor o Draft sin distinguir mayusculas (OJO: Stable, Table,
      Editable, Mutable o Notable contienen "tab"); ningun nombre de tipo ni de miembro publico con ProjectSummary, Metric, Provider,
      TotalRacks, Aggregat o Bom (sin distinguir mayusculas).
    - WorkspaceHostSourceGuardTests y WorkspaceBridgeSourceGuardTests: sin las palabras SetImpliedSelection, SetCurrentView,
      SetSystemVariable, Zoom seguido de letras (Zoom, ZoomTo, ZoomExtents...), Regen, UpdateScreen, Highlight, Unhighlight,
      SendStringToExecute ni PanTo; sin Autodesk.AutoCAD.Internal, PaletteSet, AddVisual, System.Windows, RackCad.UI, RackCad.Plugin,
      UserControl ni Dispatcher; sin Task.Run, Task.Factory, new Thread, ThreadPool, Timer, async, await, Parallel.,
      BackgroundWorker ni SynchronizationContext; sin System.IO, Registry, Xrecord, XData, DBDictionary, NamedObjectsDictionary,
      AtomicFile, UserSettings ni Environment.SetEnvironmentVariable; sin RackLog ni RackCad.Application.Diagnostics; sin
      Guid.NewGuid, new RackEmbedDocument, RackEmbedComposer ni RackBlockData.Write; ningun tipo cuyo nombre contenga Log (tambien
      Catalog, Dialog, Logic); y, sin distinguir mayusculas, sin ComputedParameter, Metric, ProjectSummary, TotalRacks, Aggregat,
      Provider ni Bom en el texto (tambien IFormatProvider o Symmetric).
    - Usuarios exactos (recorren todo src/): WorkspaceSessionRegistry solo en EventBridgeCore.cs y en su propio archivo; new
      EventBridgeCore( solo en el puente del Plugin; WorkspaceHostCore.cs tiene recuentos exactos (_reader.Invalidate( una vez,
      .ApplyContext( una vez, WorkspaceSelectionMapper.ToContext( una vez, sin SelectionContext.One( ni SelectionContext.Selection().
      Un archivo nuevo no nombra WorkspaceSessionRegistry ni EventBridgeCore; un cambio en un archivo existente es aditivo y no anade
      esas llamadas.
    Las guardas de privacidad nuevas de INV-36 van en archivos NUEVOS de tests/RackCad.Tests/Workspace/ y pueden nombrar esos
    conceptos en sus patrones. Usa los ayudantes de tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs (WorkspaceHostSource:
    Code, Files, ApplicationFiles, PluginFiles, AllProduct, BodyOf, Count, que ya quitan comentarios) para leer fuentes desde la raiz.
  * El CI rechaza avisos nuevos: cada archivo de prueba nuevo lleva #nullable enable (una anotacion string? sin contexto nullable da
    CS8632); los archivos nuevos de Application/Workspace llevan #nullable enable como los existentes. Los avisos xUnit2030/xUnit2031
    de dos pruebas de F1 ya existen y no se tocan.
- Paso RED (un solo commit, con TODAS las pruebas nuevas de la cadena):
  1. Producto como ESQUELETO COMPILABLE en src/RackCad.Application/Workspace/ (firmas completas y cuerpos que aun no cumplen el
     comportamiento congelado, p. ej. valores por defecto o colecciones vacias; nunca NotImplementedException que haga fallar todo
     por excepcion).
  2. Pruebas xunit nuevas en archivos NUEVOS de tests/RackCad.Tests/Workspace/ (namespace RackCad.Tests): NavigationTargets (regla de
     miembros, xref excluido, ilegible excluido, Id vacio excluido, igualdad OrdinalIgnoreCase), orden determinista, cache e
     invalidacion, sondeo solo diagnostico (cero objetivos), aislamiento por sesion (INV-22/D-03), privacidad INV-36 por guarda de
     fuente, autoridad unica INV-38 y revalidacion INV-40 con puerto simulado de efectos.
  3. Corre el filtro del contrato: debe compilar y FALLAR POR ASERCION (seleccion > 645), nunca por error de compilacion; las 645
     pruebas existentes siguen en verde. Commit RED (git add solo de archivos del alcance) y git push. Registra cada corrida como
     Phase RED con TreeSha = el commit RED.
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (son RT; 16.8: el GREEN no toca RT). Si una prueba necesitara
  cambio: parada y bloqueo estructurado, no la edites.
- Paso GREEN:
  1. Implementa el comportamiento en src/RackCad.Application/Workspace/ hasta que el filtro quede verde.
  2. Sobre el commit GREEN (arbol limpio, TreeSha = CurrentSha): el filtro (todas en verde, Phase GREEN); la suite completa
     tests/RackCad.Tests/RackCad.Tests.csproj (0 fallidas; Phase RELEVANT; incluye las guardas y censos de F1 e I-65); y
     dotnet build -c Release --no-incremental de tests/RackCad.Tests/RackCad.Tests.csproj con 0 errores y 0 lineas que casen
     \bwarning\s+(MSB|CS|NU|NETSDK)\d+\b (Phase RELEVANT). Registra comando, seleccion, resultado y conteos. Si algo falla, no lo
     arregles con otro commit: parada y bloqueo estructurado.
  3. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push (fast-forward):
     "Estado: F2 en curso; entrega del Worker de F2-T1-NAVIGATION-CORE (WorkRunId R20261108T182756Z-ec7e, intento 1) pendiente de la CI exacta, de la
     verificacion del Controller y de nc1..nc3. attempts = 1. F2-T2 no autorizada."
- Git: sin --force, sin rebase, sin stash, sin amend, sin checkout de otros SHA, sin tocar otras ramas ni main; exactamente un
  commit RED y un commit GREEN, cada uno con su push.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje dotnet
  (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- En la entrega y en los mensajes de commit no escribas rutas locales distintas de las del worktree y de artifacts/; nada de
  config.toml.
- Entrega (artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE/1/R20261108T182756Z-ec7e/worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  Schema rackcad-worker-handoff/v1; TaskId F2-T1-NAVIGATION-CORE; RunId R20261108T182756Z-ec7e; DelegationRunId R20261108T181429Z-adae; Initiative I-64; Gate F2;
  Attempt 1; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de
  cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado, ResultsFile = TRX o log, o null;
  FailedTests con los nombres reales); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested
  "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada. En WorkCompleted y Evidence describe los tipos creados y el
  mapa evidencia-por-invariante (D-03, D-05, D-07, INV-03, INV-22, INV-36, INV-38, INV-40) con las pruebas que lo demuestran.
- Tope: 60 minutos (AUTOMATION_PLAN 16.4). Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
