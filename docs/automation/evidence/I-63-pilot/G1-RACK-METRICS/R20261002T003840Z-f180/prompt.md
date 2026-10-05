I-63 — DELEGACION G1-RACK-METRICS — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 0; RunId R20261002T003840Z-f180;
           DelegationRunId R20261002T002817Z-6294; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha f71de12b33567e8b4109d3a17c60a17d54e96fdc;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261002T002817Z-6294
Autoridades (leer desde Git, no copiar): docs/initiatives/I-63-proposal-v3.md (Frozen: YES; D-02, D-05..D-09, D-10a,
           D-11, D-17, D-26, D-28, §20 filas INV-04, 05, 09, 14, 16, 34 y §21) y
           docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md (A-1.3) (UNIT_DOC);
           docs/initiatives/I-63-parametros-calculados-resumen-proyecto.md y docs/automation/decisions/I-63.md §2 y §6
           (UNIT_DOC); AGENTS.md «Convenciones arquitectonicas», docs/INITIATIVE_LIFECYCLE.md §7, ADR-0043 D1 y D24,
           docs/AUTOMATION_PLAN.md §16, docs/automation/agent-execution/README.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: implementar en Application pura, bajo src/RackCad.Application/ComputedParameters/, la operacion
           RackMetricRequest -> RackMetricResults para UN RackId segun el Freeze V3 + A-1: proyeccion de definicion D-10a
           (kind con Ordinal), orden canonico D-11, lector de diseno por kind D-26 con contador de lecturas en su costado
           (A-1.3), precedencia D-28, catalogo cerrado D-05 con MetricId D-02, providers puros por kind D-08 con registro
           cerrado por KindDispatch, Rack.Frentes y Rack.FrentesVacios del Selectivo sobre el sistema resuelto del fondo 0
           y estados D-09 para los seis kinds. NO incorporar RackComputedExpressionContext (es de G3), ni poblacion,
           ProjectSummary, RackOutputVerdict ni provenance (G2/G4).
Alcance: permitido EXACTAMENTE estos nueve archivos nuevos:
           src/RackCad.Application/ComputedParameters/RackMetricIds.cs
           src/RackCad.Application/ComputedParameters/RackMetricDefinitionProjection.cs
           src/RackCad.Application/ComputedParameters/RackMetricModels.cs
           src/RackCad.Application/ComputedParameters/RackMetricDesignReader.cs
           src/RackCad.Application/ComputedParameters/RackMetricProviders.cs
           src/RackCad.Application/ComputedParameters/RackMetricProviderRegistry.cs
           src/RackCad.Application/ComputedParameters/RackMetricRequest.cs
           tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs
           tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs
           prohibido: todo lo demas, en particular src/RackCad.Application/{Expressions,Persistence,ProjectVariables,
           Systems,Bom}/, src/RackCad.Domain/, src/RackCad.Plugin/, src/RackCad.UI/, tests/RackCad.UI.Tests/, assets/,
           deploy/, eng/, tools/, .github/, docs/, RackCad.sln, Directory.Build.*, global.json, NuGet.Config, .git*,
           .editorconfig, AGENTS.md, CLAUDE.md, README.md y cualquier .csproj
Invariantes del Freeze (los del paquete aceptado; criterio de aceptacion de cada uno entre parentesis):
           INV-04 (fondo 0 con 4 frentes, uno vacio, y fondo 1 con 6 -> Rack.Frentes = 4; el caso equivalente «diseno sin
             celdas» da lo mismo);
           INV-05 (Rack.FrentesVacios cuenta en el fondo 0 las bahias con Levels.Count == 0 && FloorPalletCount <= 0; una
             tarima de piso sin larguero NO es vacia; un frente sin celdas SI);
           INV-09 (una sola funcion de guarda ForbiddenProviderDependencies(codigoFuente), sobre el codigo sin comentarios,
             aplicada a cada archivo real de provider, que pasa, y a un fixture de texto invalido que llama a
             SelectiveGeometryResolver, que la MISMA funcion debe detectar; tipos prohibidos: resolvers, RegistryEvaluation,
             ExpressionBinder, ProjectVariables*, Autodesk, catalogos y stores);
           INV-14 + A-1.3 (Selectivo -> exactamente 1 resolucion en el costado de resolucion; Push Back y Cabecera ->
             0 resoluciones y 0 lecturas contadas en el costado del lector D-26, no en los stores);
           INV-16 (kinds mezclados -> Unavailable(KindIncoherent); Kind ausente -> Unavailable(KindAbsent));
           INV-34 via de peticion + A-1.3 (Push Back con diseno ilegible -> NotSupported con 0 lecturas y 0 resoluciones;
             Cabecera -> NotApplicable; Selectivo con hermanas divergentes -> Unavailable(SiblingsDivergent) con
             0 resoluciones; Selectivo ilegible -> Unavailable(DesignUnreadable); Selectivo valido -> Available);
           D-02/D-05 (seis MetricId(Rack|Project, token) congelados; G1 no crea SymbolId ni namespaces);
           D-08 (providers puros, sin otro provider, estado ni cache; registro con los seis tokens RackEmbedDocument.Kind*
             construido con KindDispatch, sin reflexion); D-28 (primer paso que decide: kind/identidad -> soporte de diseno
             -> E5 -> E4 -> efectivo/resuelto -> Available; NotSupported y NotApplicable no leen ni resuelven);
           ningun consumidor existente cambia de comportamiento
Rutas y archivos calientes (solo lectura): src/RackCad.Application/Persistence/RackEmbedDocument.cs (Kind*, RackEmbedStore),
           KindDispatch.cs, SelectivePalletDesignStore, RackProjectStore, FlowBedConfigurationStore;
           src/RackCad.Application/Bom/BomAuthoredAuthority.cs (Resolve(rackId, IReadOnlyList<ProjectVariableScanEntry>));
           src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs (Project(definitionId, embed, refs);
             SOLO como entrada tipada de E4 y solo para hermanas Known, coherentes y legibles: nunca para clasificar) y
             ProjectVariablesReadResult; src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs
             (ResolveAccredited(authored, read)), SelectiveGeometryResolver.cs (Resolve(design, catalog)) y
             SelectiveDepthLayout.cs (BaysOfFondo(system, 0)); src/RackCad.Domain/Systems/Selective/;
           src/RackCad.Plugin/KindHandlers/*KindHandler.cs (referencia de las filas D-26; no se toca);
           guardas de fuente de referencia: tests/RackCad.Tests/CantileverPluginSourceGuardTests.cs (raiz del repositorio =
             la carpeta con RackCad.sln; CodeOnly quita comentarios)
Criterios de aceptacion: RED compilable primero; RED con push propio antes del GREEN (su corrida push y su job Core
           fallan por las aserciones); el GREEN no modifica ningun archivo de pruebas del RED; suite Core completa en
           verde en el GREEN; cada invariante de arriba con al menos una prueba que lo observe
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParameters,
           minimo 6, RED esperado; commit RED y push; commit GREEN y push con el resumen de estado; corridas RED, GREEN y
           relevantes; contadores de resoluciones y lecturas observados en los costados de A-1.3; worker-handoff.json
           valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli); nada fuera de los nueve archivos
Condiciones de parada: S-02..S-09, S-12..S-14, P-01..P-08 y C-01..C-06 del paquete; en particular C-01 (hace falta tocar
           Expressions, Persistence, ProjectVariables, Systems, Bom, Domain, Plugin o UI), C-02 (D-07, D-26 o D-28 no se
           pueden implementar tal como estan congelados), C-03 (el Freeze contradice el codigo consumido), C-04 (hace falta
           codigo no integrado de I-64, I-62 o I-52), C-05 (cualquier escritura fuera del alcance) y C-06 (un INV de G1 no
           puede mostrar un RED observable); ante cualquiera, parar sin continuar y devolver el bloqueo estructurado en la
           entrega (WorkerStatus BLOCKED, Disposition y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-63/G1-RACK-METRICS/0/R20261002T003840Z-f180/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena)
- Celda: claude-sonnet-5-5 x subagente, effort medium (Balanced). ChainBaseSha = BaseSha =
  f71de12b33567e8b4109d3a17c60a17d54e96fdc; ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) solo bajo
  artifacts/orchestration/I-63/G1-RACK-METRICS/0/R20261002T003840Z-f180/tests/<RunRef>/.
  Sin AutoCAD: nada de este trabajo lo necesita.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a la peticion por rack (no son decisiones nuevas):
  - Entrada (D-17, D-25): las definiciones hermanas de UN RackId como (DefinitionKey, envelopeJson,
    DirectReferenceCount), la lectura del registro (ProjectVariablesReadResult) y el catalogo como entrada tipada
    Loaded(RackCatalog) | LoadFailed. La peticion no enumera el proyecto ni evalua poblacion.
  - D-10a clasifica cada hermana con RackEmbedStore y comparador Ordinal contra los seis tokens. Una hermana sin
    identidad atribuible (EnvelopeUnreadable, IdAbsent) no pertenece a ningun RackId (asimetria O-PV2-4 de §10); una
    KindAbsent o KindUnknown con Id si pertenece y participa en el paso 1 de D-28.
  - D-11: igualdad de RackId OrdinalIgnoreCase; orden canonico por DefinitionKey Ordinal ANTES de los pasos 1-5; el
    representante es el que devuelve E4 sobre esa lista.
  - D-28 por metrica (Rack, frentes) y (Rack, frentesVacios): paso 1 kind (todas KindAbsent -> KindAbsent; todas
    KindUnknown con el mismo token -> KindUnknown; cualquier mezcla -> KindIncoherent); paso 2 soporte D-06
    (selective Supported; dynamic, pushback, cantilever NotSupported; cabecera, cama NotApplicable) SIN leer ni
    resolver; paso 3 E5 con el lector D-26 para cada hermana; paso 4 E4 (DivergentSiblings -> SiblingsDivergent;
    UnreadableSibling -> DesignUnreadable); paso 5 efectivo (ResolveAccredited) y resuelto (SelectiveGeometryResolver)
    -> RegistryUnreadable | EffectiveFailed(outcome) | CatalogUnavailable (solo con LoadFailed) | ResolveFailed;
    paso 6 Available(valor) con el valor de D-07 (BaysOfFondo(system, 0)). Como maximo UNA Φ2+Φ3 por peticion.
  - Contadores (D-24: «los contadores inyectados si» son oraculo): inyecta en RackMetricRequest el lector D-26 y el
    costado de resolucion (lo que ejecuta Φ2+Φ3 del Selectivo), con la implementacion real por defecto; las pruebas
    los envuelven para contar. No cuentes en los stores ni modifiques resolvers o stores.
  - Los nombres de tipos son ilustrativos (D-08); la forma y las reglas no. Si el Freeze no decide un caso que
    necesitas decidir, no lo inventes: parada S-03 con la pregunta exacta. No anadas comportamiento ni pruebas para
    casos que no piden los INV de G1.
- Paso RED (un solo commit):
  1. Los siete archivos de produccion como ESQUELETO COMPILABLE: tipos y firmas definitivos; los cuerpos devuelven un
     resultado determinista que las pruebas rechazan (por ejemplo, ningun provider registrado o todas las metricas en
     un estado fijo), nunca NotImplementedException: las pruebas deben fallar por ASERCION, no por excepcion.
  2. Las dos clases de prueba en el espacio de nombres RackCad.Tests.ComputedParameters, con al menos 6 pruebas
     seleccionables que cubren INV-04, 05, 09, 14, 16 y 34 (peticion). Fixtures de diseno construidos en la prueba con
     los stores y tipos de Domain existentes (sin archivos nuevos fuera del alcance).
  3. INV-09: la guarda ForbiddenProviderDependencies y su fixture invalido quedan COMPLETOS en el RED. El RED
     observable viene de que el esqueleto todavia no tiene los providers de los seis tokens (D-08): la prueba exige el
     registro completo y aplica la guarda a cada archivo real de provider. Si no consigues un RED observable de algun
     INV, parada C-06; no lo simules.
  4. Corre el filtro RED: debe compilar y FALLAR POR ASERCION con seleccion >= 6 (no por error de compilacion).
     Commit RED (git add solo de los nueve archivos) y git push.
- Desde el commit RED NO modifiques RackMetricRequestTests.cs ni RackMetricProviderPurityTests.cs (son RT; si una
  necesitara cambio, parada y bloqueo estructurado, no la edites).
- Paso GREEN:
  1. Implementa el comportamiento congelado solo en los siete archivos de produccion.
  2. Corrida GREEN del filtro (seleccion >= 6, todas en verde). Relevante: la suite Core completa
     tests/RackCad.Tests/RackCad.Tests.csproj (anota totales).
  3. Commit GREEN (git add solo de archivos del alcance) con esta linea en el cuerpo, en UNA sola linea, y git push:
     "Estado: G1 en curso; entrega del Worker de G1-RACK-METRICS (WorkRunId R20261002T003840Z-f180) pendiente de la verificacion del Controller. attempts = 0."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push de RED y GREEN.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G1-RACK-METRICS; RunId R20261002T003840Z-f180; DelegationRunId R20261002T002817Z-6294; Initiative I-63;
  Gate G1; Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN
  (= HEAD = remoto); Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha;
  TestsExecuted y TestResults de cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit
  probado o null si el arbol tenia cambios, ResultsFile = TRX o null); Evidence con los contadores observados por
  prueba (resoluciones y lecturas); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5",
  EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
