I-63 — DELEGACION G1-RACK-METRICS — Worker, perfil ROUTINE_IMPLEMENTATION (correccion 1)
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 1; RunId R20261002T061533Z-8e64;
           DelegationRunId R20261002T061154Z-9f77; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha 44fb8dd06ee930cc1da64f91b9a167dca0c06c46;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261002T061154Z-9f77
Autoridades (leer desde Git, no copiar): docs/initiatives/I-63-proposal-v3.md (Frozen: YES; D-02, D-05..D-09, D-10a,
           D-11, D-17, D-26, D-28, §20 filas INV-04, 05, 09, 14, 16, 34 y §21) y
           docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md (A-1.3) (UNIT_DOC);
           docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T005944Z-1192/analysis.md (analisis y decision del
           Coordinator que motiva esta correccion; UNIT_DOC);
           docs/initiatives/I-63-parametros-calculados-resumen-proyecto.md y docs/automation/decisions/I-63.md §2 y §6
           (UNIT_DOC); AGENTS.md «Convenciones arquitectonicas», docs/INITIATIVE_LIFECYCLE.md §7, ADR-0043 D1 y D24,
           docs/AUTOMATION_PLAN.md §16, docs/automation/agent-execution/README.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: correccion 1 de G1. Primero un RED nuevo que solo cambia los dos archivos de pruebas (namespace RackCad.Tests y
           clases ComputedParameters*), con su propio push; despues implementar en Application pura, bajo
           src/RackCad.Application/ComputedParameters/, la operacion RackMetricRequest -> RackMetricResults para UN RackId
           segun el Freeze V3 + A-1: proyeccion D-10a (kind con Ordinal), orden canonico D-11, lector de diseno por kind
           D-26 con contador de lecturas en su costado (A-1.3), precedencia D-28, catalogo cerrado D-05 con MetricId D-02,
           providers puros por kind D-08 con registro cerrado por KindDispatch, Rack.Frentes y Rack.FrentesVacios del
           Selectivo sobre el sistema resuelto del fondo 0 y estados D-09 para los seis kinds; peticion sin hermanas con
           RackId identificable -> ArgumentException (precondicion de D-17). NO incorporar RackComputedExpressionContext
           (G3), ni poblacion, ProjectSummary, RackOutputVerdict ni provenance (G2/G4).
Alcance: permitido EXACTAMENTE estos nueve archivos:
           src/RackCad.Application/ComputedParameters/RackMetricIds.cs
           src/RackCad.Application/ComputedParameters/RackMetricDefinitionProjection.cs
           src/RackCad.Application/ComputedParameters/RackMetricModels.cs
           src/RackCad.Application/ComputedParameters/RackMetricDesignReader.cs
           src/RackCad.Application/ComputedParameters/RackMetricProviders.cs
           src/RackCad.Application/ComputedParameters/RackMetricProviderRegistry.cs
           src/RackCad.Application/ComputedParameters/RackMetricRequest.cs
           tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs
           tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs
           prohibido: todo lo demas, en particular tests/RackCad.Tests/NamespaceFolderGuardTests.cs,
           src/RackCad.Application/{Expressions,Persistence,ProjectVariables,Systems,Bom}/, src/RackCad.Domain/,
           src/RackCad.Plugin/, src/RackCad.UI/, tests/RackCad.UI.Tests/, assets/, deploy/, eng/, tools/, .github/, docs/,
           RackCad.sln, Directory.Build.*, global.json, NuGet.Config, .git*, .editorconfig, AGENTS.md, CLAUDE.md,
           README.md y cualquier .csproj
Invariantes del Freeze (los del paquete aceptado; criterio de aceptacion de cada uno entre parentesis):
           INV-04 (fondo 0 con 4 frentes, uno vacio, y fondo 1 con 6 -> Rack.Frentes = 4; el caso equivalente «diseno sin
             celdas» da lo mismo);
           INV-05 (Rack.FrentesVacios cuenta en el fondo 0 las bahias con Levels.Count == 0 && FloorPalletCount <= 0; una
             tarima de piso sin larguero NO es vacia; un frente sin celdas SI);
           INV-09 (una sola funcion de guarda ForbiddenProviderDependencies(codigoFuente), sobre el codigo sin comentarios,
             aplicada a cada archivo real de provider, que pasa, y a un fixture de texto invalido que llama a
             SelectiveGeometryResolver, que la MISMA funcion debe detectar);
           INV-14 + A-1.3 (Selectivo -> exactamente 1 resolucion en el costado de resolucion; Push Back y Cabecera ->
             0 resoluciones y 0 lecturas contadas en el costado del lector D-26, no en los stores);
           INV-16 (kinds mezclados -> Unavailable(KindIncoherent); Kind ausente -> Unavailable(KindAbsent));
           INV-34 via de peticion + A-1.3 (Push Back con diseno ilegible -> NotSupported con 0 lecturas y 0 resoluciones;
             Cabecera -> NotApplicable; Selectivo con hermanas divergentes -> Unavailable(SiblingsDivergent) con
             0 resoluciones; Selectivo ilegible -> Unavailable(DesignUnreadable); Selectivo valido -> Available);
           D-02/D-05 (MetricId(Rack|Project, token); G1 no crea SymbolId ni namespaces); D-08 (providers puros; registro con
             los seis tokens RackEmbedDocument.Kind* construido con KindDispatch, sin reflexion); D-28 (primer paso que
             decide: kind/identidad -> soporte de diseno -> E5 -> E4 -> efectivo/resuelto -> Available; NotSupported y
             NotApplicable no leen ni resuelven); ningun consumidor existente cambia de comportamiento
Rutas y archivos calientes (solo lectura): tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda de I-23);
           src/RackCad.Application/Persistence/RackEmbedDocument.cs (Kind*, RackEmbedStore), KindDispatch.cs,
           SelectivePalletDesignStore, RackProjectStore, FlowBedConfigurationStore;
           src/RackCad.Application/Bom/BomAuthoredAuthority.cs (Resolve(rackId, IReadOnlyList<ProjectVariableScanEntry>));
           src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs (solo como entrada tipada de E4, para
             hermanas Known, coherentes y legibles; nunca para clasificar) y ProjectVariablesReadResult;
           src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs (ResolveAccredited(authored, read)),
           SelectiveGeometryResolver.cs (Resolve(design, catalog)) y SelectiveDepthLayout.cs (BaysOfFondo(system, 0));
           src/RackCad.Domain/Systems/Selective/; src/RackCad.Plugin/KindHandlers/*KindHandler.cs (referencia de D-26);
           el estado actual de los nueve archivos en BaseSha (esqueleto y pruebas del RED 1013449d)
Criterios de aceptacion: los de AcceptanceCriteria del paquete aceptado: RED nuevo solo de los dos archivos de pruebas,
           con push propio, cuyo job Core falla solo por aserciones focales contra el esqueleto vigente y con
           NamespaceFolderGuardTests en verde; pruebas en namespace RackCad.Tests con clases ComputedParameters*; el filtro
           del contrato selecciona >= 6; GREEN solo en los siete archivos de produccion, sin tocar RT ni ChainRedFiles;
           suite Core completa en verde; ArgumentException sin hermanas con RackId identificable; cada invariante de arriba
           con al menos una prueba que lo observe
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParameters,
           minimo 6, RED esperado; ademas FullyQualifiedName~NamespaceFolderGuardTests en verde en el RED y en el GREEN;
           commit RED y push; commit GREEN y push con el resumen de estado; corridas RED, GREEN y relevantes; contadores de
           resoluciones y lecturas observados en los costados de A-1.3; worker-handoff.json valido en la ruta esperada
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
           artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T061533Z-8e64/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; correccion 1 de la cadena, AUTOMATION_PLAN 16.8)
- Celda: claude-sonnet-5-5 x subagente, effort medium (Balanced). BaseSha = 44fb8dd06ee930cc1da64f91b9a167dca0c06c46; ChainBaseSha =
  f71de12b33567e8b4109d3a17c60a17d54e96fdc; ChainRedSha = 1013449d61b8292b77273ceeac30b82282b975a2; ChainRedFiles =
  tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs y
  tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs. Tu diff toca ChainRedFiles: esta entrega EXIGE RED.
- Causa de la correccion (lee el analysis.md del Coordinator:
  docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T005944Z-1192/analysis.md): en el intento 0 las pruebas
  declararon namespace RackCad.Tests.ComputedParameters y la guarda de I-23
  (tests/RackCad.Tests/NamespaceFolderGuardTests.cs, TestProjects_KeepExactlyOneAssemblyRootNamespace) exige exactamente
  namespace RackCad.Tests en todo .cs de tests/RackCad.Tests. No toques esa guarda ni el contrato ni su filtro.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) solo bajo
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T061533Z-8e64/tests/<RunRef>/. Escribe tus scripts auxiliares, si los
  necesitas, en artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T061533Z-8e64/work/ (ignorado por git), no en otras carpetas.
  Sin AutoCAD: nada de este trabajo lo necesita.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- Paso RED (un solo commit, SOLO de pruebas):
  1. En los dos archivos de ChainRedFiles (misma ruta): namespace RackCad.Tests (una sola declaracion, con bloque, al
     principio de linea) y clases renombradas ComputedParametersRackMetricRequestTests y
     ComputedParametersRackMetricProviderPurityTests (o el nombre que empiece por ComputedParameters), de modo que el
     filtro FullyQualifiedName~RackCad.Tests.ComputedParameters las seleccione. Conserva las pruebas de INV-04, 05, 09,
     14, 16 y 34 y sus aserciones; no las debilites. Ajusta las referencias internas a los nombres nuevos.
  2. Anade una prueba de la aclaracion de D-17 del analysis.md: una peticion cuyas hermanas no tienen ningun RackId
     identificable (sobres ilegibles o con Id en blanco) lanza ArgumentException (Assert.Throws).
  3. No toques ningun archivo de produccion en este commit. Corre el filtro del contrato: debe compilar, seleccionar
     >= 6 y FALLAR POR ASERCION contra el esqueleto vigente de 1013449d (no por compilacion). Corre ademas
     FullyQualifiedName~NamespaceFolderGuardTests: debe pasar entero. Commit RED (git add solo de esos dos archivos)
     y git push.
- Desde ese commit RED NO modifiques RackMetricRequestTests.cs ni RackMetricProviderPurityTests.cs (si una prueba
  necesitara cambio, parada y bloqueo estructurado, no la edites).
- Paso GREEN:
  1. Implementa el comportamiento congelado solo en los siete archivos de produccion (Freeze V3 + A-1, D-28 por metrica,
     contadores inyectados en los costados de A-1.3), con la aclaracion de D-17: sin hermanas con RackId identificable ->
     ArgumentException; hermanas de RackIds distintos (OrdinalIgnoreCase) -> ArgumentException. Puedes partir del parche
     no commiteado del intento 0, copiado en artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T061533Z-8e64/prior/prior-green.patch,
     revisandolo contra el Freeze: no es autoridad, y su rama «sin hermanas atribuibles -> Unavailable(EnvelopeUnreadable)»
     esta prohibida por el analysis.md.
  2. Corrida GREEN del filtro (seleccion >= 6, todas en verde). Relevante: la suite Core completa
     tests/RackCad.Tests/RackCad.Tests.csproj (anota totales; debe quedar en verde, incluida NamespaceFolderGuardTests).
  3. Commit GREEN (git add solo de los siete archivos de produccion) con esta linea en el cuerpo, en UNA sola linea, y
     git push:
     "Estado: G1 en curso; entrega del Worker de G1-RACK-METRICS (WorkRunId R20261002T061533Z-8e64, correccion 1) pendiente de la verificacion del Controller. attempts = 1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push de RED y GREEN.
- Texto libre de la entrega (WorkCompleted, Evidence, UnexpectedFindings, KnownLimitations, Deviations, OpenQuestions,
  RecommendedNextAction): ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into
  main, merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
  Di «implementacion», «parche previo» o «commit GREEN».
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G1-RACK-METRICS; RunId R20261002T061533Z-8e64; DelegationRunId R20261002T061154Z-9f77; Initiative I-63; Gate G1; Attempt 1;
  BaseSha = el de arriba; RedSha = el SHA del commit RED nuevo; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null
  si el arbol tenia cambios, ResultsFile = TRX o null); Evidence con los contadores observados por prueba (resoluciones
  y lecturas); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium",
  Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada (una condicion S-xx de comportamiento STOP va con Disposition STOP); listas
  vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
