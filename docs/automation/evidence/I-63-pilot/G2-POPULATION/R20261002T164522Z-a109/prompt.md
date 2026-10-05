I-63 — DELEGACION G2-POPULATION — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-63; gate G2; tarea G2-POPULATION; intento 2; RunId R20261002T164522Z-a109;
           DelegationRunId R20261002T164053Z-586f; AuthorityRevision 669d8a391f208e1077f136a406fd89058bc0ce6e;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha 669d8a391f208e1077f136a406fd89058bc0ce6e;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261002T164053Z-586f
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-63-proposal-v3.md (Frozen: YES): §8 D-17, §9 D-09, §10 D-10, D-10a, D-11, D-12, D-26 y D-27,
           §11 D-13, §14 D-20 (nivel Population), §19 D-25, §20 filas INV-01, 02, 03, 06, 07, 08, 10, 11, 12, 13, 32, 33 y 35, y
           §21; docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md; docs/initiatives/
           I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md (A-2: parte de G2 de INV-11 e INV-32) (UNIT_DOC, en
           AuthorityRevision); docs/automation/decisions/I-63.md §2 (incluida la precondicion ArgumentException de D-17) (UNIT_DOC);
           AGENTS.md «Convenciones arquitectonicas», ADR-0043 D1 y D24, docs/AUTOMATION_PLAN.md §16 (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria). En resumen: en Application pura, bajo
           src/RackCad.Application/ComputedParameters/, la poblacion cotizable de D-10 (E1..E6) sobre un contrato de entrada
           sintetico, ProjectPopulation (D-20 nivel Population), la agregacion de D-12 como funcion pura, el veredicto unico
           RackOutputVerdict de D-27 con la correspondencia del handler y el contador unico de evaluaciones de poblacion; y,
           en el Plugin, SOLO la delegacion de PushBackKindHandler.OutputBlockedReason en esa funcion de Application.
           NO ProjectSummary Full, RackSummary.Metrics ni RackComputedExpressionContext (G3/G4): si hicieran falta, parada C-08.
Alcance: el del paquete (= el del contrato): permitido src/RackCad.Application/ComputedParameters/,
           tests/RackCad.Tests/ComputedParameters/ y el archivo src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs;
           prohibido el ForbiddenWriteScope del paquete (los demas handlers, el resto del Plugin, Expressions, Persistence,
           ProjectVariables, Systems, Bom, Domain, UI, docs/, tests/RackCad.Tests/NamespaceFolderGuardTests.cs, cualquier
           .csproj, etc.). Los archivos de G1 de ComputedParameters se pueden ampliar sin romper G1 ni sus pruebas
Invariantes del Freeze: los del paquete aceptado (Invariants), con la parte de G2 de INV-11 e INV-32 segun la A-2
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/ComputedParameters/ (G1: RackDefinitionCapture y RackMetricDefinitionProjection D-10a con
             CanonicalOrder D-11; IRackMetricDesignReader/RackMetricDesignReader D-26 de los seis kinds; RackCatalogInput
             Loaded|LoadFailed en RackMetricModels.cs; RackMetricRequest; RackMetricIds; providers);
           src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs (OutputBlockedReason, lineas 56-71 en 819955d6) y los
             demas *KindHandler.cs (BuildBom: objetivo de la guarda de INV-35, no se tocan);
           src/RackCad.Application/Bom/RackBomOutputGate.cs (For(system).Reason) y BomAuthoredAuthority.cs;
           src/RackCad.Application/Systems/PushBack/PushBackResolver.cs (catalog ?? new RackCatalog());
           src/RackCad.Application/Views/Insertion/RackEnvelopeIdProbe.cs (tanteo de Id solo diagnostico);
           src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs (SOLO entrada tipada de E4);
           tests/RackCad.Tests/NamespaceFolderGuardTests.cs (namespace RackCad.Tests en pruebas; namespace por carpeta y con
             bloque en produccion); tests/RackCad.Tests/CantileverPluginSourceGuardTests.cs (RepoRoot y CodeOnly de referencia)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParametersPopulation,
           minimo 13, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-08); en particular C-01
           (tocar algo fuera del alcance, incluido cualquier otro archivo del Plugin), C-02 (D-10, D-10a, D-11, D-12, D-26 o
           D-27 no implementables tal como estan), C-06 (un INV de G2 sin RED observable), C-07 (la delegacion cambiaria el
           comportamiento observable del handler) y C-08 (haria falta ProjectSummary Full o el nucleo); ante cualquiera,
           parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164522Z-a109/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena G2-POPULATION)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = 669d8a391f208e1077f136a406fd89058bc0ce6e; ChainRedSha = null y
  ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164522Z-a109/tests/<RunRef>; anota en Command la orden COMPLETA. Scripts auxiliares,
  si los necesitas, solo en artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164522Z-a109/work/. Sin AutoCAD.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a G2 (no son decisiones nuevas; si el Freeze no decide algo que necesitas decidir, no lo
  inventes: parada S-03 con la pregunta exacta):
  - Entrada (D-25, Phi0): lista de (DefinitionKey, envelopeJson, DirectReferenceCount) (puedes reutilizar
    RackDefinitionCapture de G1), la lectura del registro (ProjectVariablesReadResult) y RackCatalogInput Loaded|LoadFailed.
    La construyen las pruebas.
  - D-10, en este orden y la primera que decide: E1 identidad (D-10a; una definicion COLOCADA EnvelopeUnreadable o IdAbsent
    -> cobertura no acreditada; sin colocar -> se ignora); agrupacion por RackId OrdinalIgnoreCase con grafia canonica
    minima en Ordinal y hermanas colocadas o no (D-11.3, asimetria O-PV2-4), en orden canonico por DefinitionKey Ordinal;
    E2 colocado (alguna hermana con DirectReferenceCount > 0) o Excluded(NotPlaced) sin diagnostico; E3 kind coherente
    (todas KindAbsent -> Excluded(KindAbsent); todas KindUnknown con el mismo token -> Excluded(KindUnknown); mezcla ->
    Undetermined(KindIncoherent)); E5 diseno legible por hermana con el lector D-26 (Undetermined(DesignUnreadable)); E4
    BomAuthoredAuthority sobre la entrada tipada (DivergentSiblings -> Excluded(SiblingsDivergent); UnreadableSibling ->
    Undetermined(DesignUnreadable)); E6 RackOutputVerdict (Deny -> Excluded(OutputDenied); Undetermined ->
    Undetermined(razon)). Registro, efectivo y resuelto de metricas NO tocan la pertenencia (P-03).
  - D-12: totalRacks = Available(n incluidos) si y solo si la cobertura esta acreditada (ninguna colocada sin identidad
    atribuible y ningun rack Undetermined); si no, Unavailable(CoverageNotAccredited) con diagnosticos (D-09). BySystem con
    los seis tokens en orden fijo; rackCount Available(n >= 0) cuando totalRacks es Available. totalFrentes y
    totalFrentesVacios: selective por suma de los MetricValue por rack de los incluidos (Unavailable si falta cobertura o
    algun miembro no es Available; nunca parcial), dynamic/pushback/cantilever NotSupported, cabecera/cama NotApplicable.
    D-20: ProjectPopulation no tiene campos de metricas por rack (pertenencia, totalRacks y rackCount) y nunca ejecuta Phi2
    ni Phi3 de metricas (solo la Phi3 de E6 en Push Back); por eso la agregacion de frentes es una funcion pura sobre
    pertenencia y MetricValue por rack que las pruebas de G2 alimentan (en G4 la alimentara ProjectSummary Full).
  - D-27: RackOutputVerdict(kindToken, designJson, CatalogInput) -> Allow | Deny(motivo) | Undetermined(razon). pushback:
    RackProjectStore.Deserialize; sin PushBackDesign -> Undetermined(DesignUnreadable); LoadFailed ->
    Undetermined(CatalogUnavailable); Loaded(c) -> PushBackResolver(c).Resolve -> RackBomOutputGate.For(system): con motivo
    Deny(motivo), sin motivo Allow; cualquier excepcion -> Undetermined(ResolveFailed). Los demas kinds -> Allow sin leer.
    Via del handler (funcion de Application): catalogo null -> Loaded(new RackCatalog()) (nunca LoadFailed); despues
    Deny(m) -> m, Allow -> null, Undetermined -> null.
  - Contador de INV-32 (A-2.1): un solo contador del orquestador real de poblacion, observable sea cual sea el punto de
    entrada (las pruebas de xunit corren en paralelo: evita un estatico compartido sin alcance). RackMetricRequest directo ->
    0; ProjectPopulation sobre el mismo rack -> el mismo contador > 0.
  - INV-11 (A-2.2): con el orden de entrada invertido, misma Membership, representante, DisplayName (primer Name no vacio en
    orden canonico, con Trim), grafia canonica, ProjectPopulation y agregados de G2.
- Paso RED (un solo commit):
  1. Esqueleto COMPILABLE de los tipos nuevos de produccion (namespace RackCad.Application.ComputedParameters, con bloque,
     como G1): tipos y firmas definitivos; cuerpos que devuelven un resultado determinista que las pruebas rechazan, nunca
     NotImplementedException. No toques el handler en el RED.
  2. Pruebas nuevas en namespace RackCad.Tests, clases cuyo nombre empieza por ComputedParametersPopulation, >= 13 pruebas
     seleccionables que cubren INV-01, 02, 03, 06, 07, 08, 10, 11, 12, 13, 32, 33 y 35.
  3. Guardas de fuente (INV-33, INV-35): una sola funcion por guarda, sobre el codigo sin comentarios (raiz del repositorio
     = la carpeta con RackCad.sln), completa en el RED. INV-33 tiene RED natural: la guarda rechaza el OutputBlockedReason
     vigente. Para INV-35 el RED observable puede venir de que la prueba exija las seis filas de D-26 declaradas por la
     produccion (el esqueleto aun no las declara); si no consigues un RED observable de algun INV, parada C-06; no lo simules.
  4. Corre el filtro del contrato: compila, selecciona >= 13 y FALLA POR ASERCION. Commit RED (git add solo de archivos del
     alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si una prueba necesitara cambio: parada, sin editarla).
- Paso GREEN:
  1. Implementa en Application y delega OutputBlockedReason en la funcion de Application (sin PushBackResolver ni
     RackBomOutputGate.For en el texto del metodo), conservando su comportamiento observable (INV-13).
  2. Corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota totales; incluye G1 y
     NamespaceFolderGuardTests). Compila tambien el Plugin con salida temporal para no tocar DLL cargados:
     dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -o artifacts/orchestration/I-63/G2-POPULATION/2/R20261002T164522Z-a109/work/plugin-build
     Si falla por referencias de AutoCAD ausentes o bloqueos, NO cierres AutoCAD: anotalo en KnownLimitations (la senal es el
     job «Build Plugin without AutoCAD» de la CI).
  3. Commit GREEN (git add solo de archivos del alcance) con esta linea en el cuerpo, en UNA sola linea, y git push:
     "Estado: G2 en curso; entrega del Worker de G2-POPULATION (WorkRunId R20261002T164522Z-a109) pendiente de la verificacion del Controller. attempts = 2."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G2-POPULATION; RunId R20261002T164522Z-a109; DelegationRunId R20261002T164053Z-586f; Initiative I-63; Gate G2; Attempt 2; BaseSha = el de arriba;
  RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico,
  Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX);
  Evidence con el contador de poblacion observado (INV-32); Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada (una S-xx o C-xx de comportamiento
  STOP va con Disposition STOP); listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
