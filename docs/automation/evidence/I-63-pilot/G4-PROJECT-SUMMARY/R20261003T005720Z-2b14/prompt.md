I-63 — DELEGACION G4-PROJECT-SUMMARY — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-63; gate G4; tarea G4-PROJECT-SUMMARY; intento 2; RunId R20261003T005720Z-2b14;
           DelegationRunId R20261003T005345Z-66f3; AuthorityRevision 527e4b9185173e2f50c10d39a882749c09b77d2f;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha 527e4b9185173e2f50c10d39a882749c09b77d2f;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261003T005345Z-66f3
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-63-proposal-v3.md (Frozen: YES): §8 D-17 y D-28, §10 D-11 y D-12, §14 D-20, §15 D-21, §18 D-24,
           §19 D-25, §20 filas INV-11, 29, 30, 31, 32 y 34, y §21; la A-1 (A-1.1 para INV-29 b, A-1.3 para INV-34); la A-2 (partes
           de G4 de INV-11 e INV-32); la A-3; docs/automation/decisions/I-63.md §2 (autorizacion de G4) (UNIT_DOC, en
           AuthorityRevision); AGENTS.md «Convenciones arquitectonicas», ADR-0043 D1 y D24, docs/AUTOMATION_PLAN.md §16 (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria). En resumen: en Application pura, bajo
           src/RackCad.Application/ComputedParameters/, ProjectSummary Full (D-20) sobre G1+G2 con RackSummary.Metrics por D-28,
           provenance en memoria (D-21) fuera de MetricValue.Equals, el cambio ADITIVO de RackComputedExpressionContext (D-21:
           BoundExpression evaluado y SymbolId leidos) y la caracterizacion de rendimiento de D-24 con contadores.
Alcance: el del paquete (= el del contrato): permitido src/RackCad.Application/ComputedParameters/ y archivos NUEVOS de
           tests/RackCad.Tests/ComputedParameters/; prohibido el ForbiddenWriteScope del paquete (Expressions, Persistence,
           ProjectVariables, Systems, Bom, Catalogs, Units, Domain, Plugin, UI, cualquier .csproj, las suites protegidas, docs/,
           etc.). Las pruebas EXISTENTES de tests/RackCad.Tests/ComputedParameters/ (G1, G2 y G3) no se modifican, borran ni
           renombran: si hiciera falta, parada (C-01). Los archivos de produccion de G1-G3 se pueden ampliar sin romper su
           comportamiento ni sus pruebas
Invariantes del Freeze: los del paquete aceptado (Invariants), con las partes de G4 de INV-11 e INV-32 segun la A-2
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/ComputedParameters/ (G1: RackMetricRequest con RackMetricResolutionSide y su contador de
             resoluciones, lector D-26 con su contador de lecturas (A-1.3), MetricValue/RackMetricResults/UnavailableReason en
             RackMetricModels.cs, RackMetricIds, providers; G2: ProjectPopulation, ProjectPopulationAggregator (funcion pura que
             G4 alimenta con las metricas por rack), RackOutputVerdict, RackPopulationEvaluationCounter (INV-32); G3:
             RackComputedExpressionContext y RackComputedEvaluation);
           src/RackCad.Application/RackCad.Application.csproj, src/RackCad.UI/RackCad.UI.csproj y
             src/RackCad.Plugin/RackCad.Plugin.csproj (solo lectura: INV-29 b; el Plugin declara AutoCAD por dos ItemGroup con
             condiciones opuestas, lineas 11-16 y 28-40, y UseWPF en la linea 4);
           tests/RackCad.Tests/ComputedParameters/ (pruebas de G1-G3 como referencia de estilo y de kits; solo lectura);
           tests/RackCad.Tests/NamespaceFolderGuardTests.cs (namespace RackCad.Tests en pruebas; namespace por carpeta y con
             bloque en produccion); tests/RackCad.Tests/RegistryCommitAccreditationTests.cs:228-232 (lectura de .csproj de
             referencia)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParametersSummary,
           minimo 10, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); en particular C-01 (tocar
           algo fuera del alcance o una prueba existente), C-02 (D-20, D-21, D-24 o D-28 no implementables tal como estan),
           C-06 (un INV de G4 sin RED observable), C-07 (la provenance cambiaria la igualdad o el comportamiento de G1-G3),
           C-08 (persistir o presentar el resumen, o abrir Create), C-09 (N = 1000 no cabe, o un tiempo como oraculo) y C-10
           (cuestion material de arquitectura, autoridad o semantica congelada); ante cualquiera, parar sin continuar y
           devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261003T005720Z-2b14/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena G4-PROJECT-SUMMARY)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = 527e4b9185173e2f50c10d39a882749c09b77d2f; ChainRedSha = null y
  ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261003T005720Z-2b14/tests/<RunRef>; anota en Command la orden COMPLETA. Scripts
  auxiliares, si los necesitas, solo en artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261003T005720Z-2b14/work/. Sin AutoCAD.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a G4 (no son decisiones nuevas; si el Freeze no decide algo que necesitas decidir, no lo
  inventes: parada S-03 con la pregunta exacta):
  - D-20 Full: ProjectSummary con Totals (Project, totalRacks); BySystem con los seis tokens en el orden fijo de G2 y sus tres
    metricas (la agregacion de G2, ProjectPopulationAggregator, alimentada con las metricas por rack del resumen); Racks con
    TODOS los RackIds atribuibles (incluidos, excluidos tambien NotPlaced, e indeterminados) con RackId canonico, KindToken,
    DisplayName, Membership y Metrics (Rack, *) siempre presentes; Diagnostics; orden de Racks por RackId canonico Ordinal y
    de Diagnostics por (codigo, DefinitionKey Ordinal). Puro e inmutable; no se persiste; sin UI. Population (ProjectPopulation)
    no cambia.
  - D-28 en RackSummary.Metrics: la MISMA tabla que RackMetricRequest (kind E3 -> soporte de diseno D-06 sin leer -> E5 -> E4
    -> efectivo/resuelto -> Available), reutilizando la logica de G1, sin otra regla; para un mismo rack, el mismo resultado
    por las dos vias (INV-34). Lecturas de diseno contadas en el costado del lector D-26 (A-1.3).
  - D-24: en una peticion Full, una captura, una lectura del registro y un catalogo; por RackId una proyeccion y una
    autoridad; una Phi2+Phi3 solo por Selectivo y un veredicto E6 por Push Back; nunca por vista; sin caches ni resolucion
    repetida dentro de la peticion. INV-30: N RackIds con 3 vistas -> N resoluciones del Selectivo en Full y 0 en Population.
  - INV-32 (A-2.1): el MISMO contador de G2 (RackPopulationEvaluationCounter): RackMetricRequest directo -> 0; ProjectPopulation
    -> > 0; ProjectSummary Full sobre el mismo rack -> > 0. Las pruebas de xunit corren en paralelo: no uses estado estatico
    compartido sin alcance.
  - INV-11 (A-2.2): Full(original) == Full(snapshot invertido), incluidos orden, representante, DisplayName, grafia,
    Membership, metricas y Diagnostics.
  - D-21 / INV-31: tabla CERRADA de identificadores de autoridad fuente: selective.resolved.fondo0.bays,
    selective.resolved.fondo0.emptyBays, population.cotizable y aggregate.sum; la prueba la compara en los dos sentidos. Cada
    metrica del resumen lleva su provenance (MetricId, RackId canonico, identificador de autoridad, fase, DefinitionKey del
    representante, outcomes de E4 y E6 y el efectivo); cada agregado, los RackIds incluidos y excluidos con su motivo. La
    provenance va en una estructura asociada o en un envoltorio del resumen: MetricValue.Equals NO cambia (si no es posible,
    parada C-07). RackComputedExpressionContext: anade (sin cambiar nada existente) la exposicion del BoundExpression evaluado
    y de la coleccion determinista de SymbolId efectivamente leidos; Evaluate devuelve exactamente lo mismo que hoy; no abras
    Create ni crees un motor de dependencias.
  - INV-29: (a) mismo snapshot en otro orden -> mismo ProjectSummary; ProjectPopulation sin campos de metricas por rack.
    (b) una sola funcion ForbiddenReferenceFamilies(rutaCsproj) en la prueba, segun A-1.1 (AutoCAD: PackageReference
    AutoCAD.NET o Reference AcCoreMgd/AcDbMgd/AcMgd; WPF: UseWPF=true o Reference PresentationFramework/PresentationCore/
    WindowsBase; union de todos los ItemGroup y PropertyGroup sin evaluar Condition; ProjectReference recursivo; sin
    transitivas): Application -> vacio; UI -> WPF; Plugin -> contiene AutoCAD. Su RED discriminante es el control positivo
    con la misma funcion (el objetivo ya pasa hoy).
  - D-24 caracterizacion (Core, sintetica): N = 1, 10, 100 y 1000 racks con tres vistas cada uno; Population frente a Full;
    lecturas repetidas; una sola peticion por rack. Oraculo = contadores inyectados (resoluciones, lecturas, evaluaciones de
    poblacion); los tiempos se miden y se registran (en la salida de la prueba y en el Evidence de la entrega), nunca se
    asertan. N = 1000 es obligatorio: si no se puede ejecutar en un tiempo razonable, parada C-09; no rebajes N, no lo dividas
    ni lo marques omitido.
- Paso RED (un solo commit):
  1. Esqueleto COMPILABLE de los tipos nuevos de produccion (namespace RackCad.Application.ComputedParameters, con bloque, como
     G1-G3): tipos y firmas definitivos; cuerpos que devuelven un resultado determinista que las pruebas rechazan, nunca
     NotImplementedException. En RackComputedExpressionContext, solo las firmas aditivas.
  2. Pruebas en archivos NUEVOS, namespace RackCad.Tests, clases cuyo nombre empieza por ComputedParametersSummary, >= 10
     pruebas seleccionables que cubren INV-11 (G4), 29, 30, 31, 32 (G4) y 34 (resumen), D-20, D-21 y D-24.
  3. Corre el filtro del contrato: compila, selecciona >= 10 y FALLA (asercion o excepcion; nunca compilacion). Commit RED
     (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si una prueba necesitara cambio: parada, sin editarla).
- Paso GREEN:
  1. Implementa en Application.
  2. Corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota totales; incluye G1, G2, G3, las suites
     de INV-28 y las guardas). Registra en el Evidence los tiempos medidos de la caracterizacion por N y nivel.
  3. Comprueba: git diff --name-status BaseSha..HEAD -- tests/RackCad.Tests/ComputedParameters/ solo contiene altas (A).
  4. Commit GREEN (git add solo de archivos del alcance) con esta linea en el cuerpo, en UNA sola linea, y git push:
     "Estado: G4 en curso; entrega del Worker de G4-PROJECT-SUMMARY (WorkRunId R20261003T005720Z-2b14) pendiente de la verificacion del Controller. attempts = 2."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G4-PROJECT-SUMMARY; RunId R20261003T005720Z-2b14; DelegationRunId R20261003T005345Z-66f3; Initiative I-63; Gate G4; Attempt 2; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales;
  Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef
  unico, Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile =
  TRX); Evidence con: (1) por INV, la prueba que lo observa; (2) los contadores observados (INV-30, INV-32, INV-34) y los
  tiempos medidos de la caracterizacion por N y nivel; (3) la forma de la provenance y la confirmacion de que MetricValue.Equals
  no cambia; (4) el cambio aditivo de RackComputedExpressionContext; Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada (una S-xx o C-xx de comportamiento
  STOP va con Disposition STOP); listas vacias si no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa al
  final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
