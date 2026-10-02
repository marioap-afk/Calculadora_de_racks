I-63 — DELEGACION G3-RACK-BUILTINS (G3-T1, RED-BUILTINS) — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-63; gate G3; tarea G3-RACK-BUILTINS (tramo T1); intento 2; RunId R20261002T184507Z-9800;
           DelegationRunId R20261002T184114Z-1c2f; AuthorityRevision e99621f9e285b1a7ddfcf51cd8ccf970828e0b66;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha e99621f9e285b1a7ddfcf51cd8ccf970828e0b66;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261002T184114Z-1c2f
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-63-proposal-v3.md (Frozen: YES): §5 D-02, D-03 y D-04; §11 D-13 y D-14; §12 D-15; §13 D-16, D-18 y
           D-19; §8 D-17 puntos 4-6; §17 D-23; §20 filas INV-15 y 17-28; §21; docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md
           (A-1.2 para INV-20); la A-2; docs/automation/decisions/I-63.md §2 (autorizacion escalonada de G3) (UNIT_DOC);
           AGENTS.md «Convenciones arquitectonicas», ADR-0043 D1, D9 y D24, docs/AUTOMATION_PLAN.md §16 (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: G3-T1 = SOLO el RED de G3. Escribir las pruebas que fijan el comportamiento congelado de G3 (INV-15 y 17-27; INV-28
           son las suites existentes, que no se tocan) y actualizar en ExpressionSymbolModelTests.cs solo las aserciones pre-ID20
           que D-16 cambia. Nada en src/: las pruebas compilan contra la API actual y FALLAN EN EJECUCION. Observar y fijar el
           resultado diagnostico exacto de INV-22. Un commit RED con push propio. La implementacion es de G3-T2, en otra
           delegacion, tras la A-3 del Coordinator.
Alcance: el del paquete (= el del contrato de T1): permitido SOLO tests/RackCad.Tests/ComputedParameters/ (archivos nuevos) y
           tests/RackCad.Tests/ExpressionSymbolModelTests.cs; prohibido src/ entero, las suites de INV-28
           (ExpressionRoundTripTests, ExpressionFormatterTests, ExpressionBinderTests, G8PersistenceIntegrationContractTests),
           ExpressionCoreGuardTests.cs, ExpressionDiagnosticCatalogTests.cs, NamespaceFolderGuardTests.cs, cualquier otra prueba
           existente, .csproj, docs/ y todo lo prohibido por el contrato
Invariantes del Freeze: los del paquete aceptado (Invariants), incluidas las reglas de G3-T1
Rutas y archivos calientes (solo lectura): src/RackCad.Application/Expressions/ (SymbolId.cs: SymbolNamespace, SymbolNamespaces y
           la validez de clave; SymbolTable.cs: SymbolScope, SymbolDefinitionKind, SymbolEntry, indices y OperatorNames;
           ExpressionBinder.cs; ExpressionFormatter.cs y CanonicalShape; ExpressionSyntaxParser.cs; RegistryEvaluation.cs;
           DependencyGraph.cs; BoundExpressionDependencies.cs; ExpressionEvaluator.cs; ExpressionDiagnosticCode.cs);
           src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs; src/RackCad.Application/ComputedParameters/
           (RackMetricRequest, RackMetricResults, MetricValue y estados de G1); los consumidores vigentes de RACKEDITAR
           (formulas de propiedad vinculada) y RACKVARIABLES (definiciones de variables), para INV-18; las suites de INV-28 y
           tests/RackCad.Tests/ExpressionBinderTests.cs:301-317 (control de OperatorInName de referencia para A-1.2)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParametersSymbols,
           minimo 12, RED esperado; commit RED y push; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado; en particular C-01 (tocar algo fuera del alcance de T1), C-02 (D-14..D-19 no
           implementables tal como estan), C-06 (un INV de G3 sin RED observable), C-10 (INV-22 exigiria un codigo de diagnostico
           nuevo) y C-11 (una expectativa no puede expresarse en una prueba que compile sin tocar src/); ante cualquiera, parar
           sin continuar y devolver el bloqueo estructurado (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T184507Z-9800/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; G3-T1, primera delegacion de la cadena G3-RACK-BUILTINS)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = e99621f9e285b1a7ddfcf51cd8ccf970828e0b66; ChainRedSha = null y
  ChainRedFiles = []: esta entrega ES el RED de la cadena. Sin GREEN en esta delegacion.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T184507Z-9800/tests/<RunRef>; anota en Command la orden COMPLETA. Scripts o pruebas
  auxiliares desechables, si los necesitas, solo en artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T184507Z-9800/work/ (fuera del
  repositorio versionado). Sin AutoCAD.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- Como escribir un RED sin tocar src/ (las pruebas SON la especificacion que G3-T2 implementara sin poder editarlas):
  - Compilan contra la API ACTUAL. Los miembros nuevos de enums se escriben con su valor numerico definitivo en una constante
    con nombre (p. ej. private static readonly SymbolNamespace RackNamespace = (SymbolNamespace)2; lo mismo para
    SymbolDefinitionKind.Computed), y los tipos nuevos de Application (p. ej. RackComputedExpressionContext y su resultado
    ComputedReferencesNotAvailable en src/RackCad.Application/ComputedParameters/, o una tabla separada de tokens persistidos de
    D9 en el nucleo) se alcanzan por reflexion con nombres y firmas exactos que tu fijas. Los nombres de tipos son ilustrativos en
    el Freeze (D-08); la forma y las reglas no. Elige nombres claros y consistentes con el codigo vigente.
  - Hoy esas pruebas fallan en ejecucion (asercion, valor de enum indefinido, tipo inexistente o excepcion del constructor);
    nunca por compilacion. Tras G3-T2 deben pasar sin cambios.
  - Cada INV necesita al menos una prueba que falle hoy por la razon del INV (RED observable). Donde el Freeze pide control
    positivo (INV-18, INV-20 con A-1.2, INV-24), usa la MISMA funcion o helper para objetivo y control. Si algun INV no puede
    tener RED observable sin tocar src/, parada C-06 o C-11; no lo simules.
- INV-22 (orden del Coordinator: observar, no elegir por inspeccion estatica): ejecuta de verdad el parser y el binder actuales
  sobre el texto Rack.#{zzz} (por ejemplo, con una prueba o un programa desechable en work/) y registra el resultado exacto:
  codigo o secuencia de codigos de ExpressionDiagnosticCode, sus spans y la ausencia de arbol enlazado. Despues fija ESE
  resultado en la prueba de INV-22 (junto a la parte del formatter, Rack.#{zzz} para un SymbolId rack ausente, que hoy falla).
  Si el comportamiento congelado exigiera un codigo que no existe en el catalogo V6, parada C-10.
- ExpressionSymbolModelTests.cs: cambia SOLO las aserciones pre-ID20 que D-16 cambia (miembros de SymbolNamespace con Rack y de
  SymbolDefinitionKind con Computed; rack como token activo de la tabla de namespaces en memoria, distinta de la de tokens
  persistidos de D9; las reglas de identidad y ambito congeladas). Ninguna otra expectativa historica de I-49.
- Paso RED (un solo commit): crea las pruebas en tests/RackCad.Tests/ComputedParameters/ (namespace RackCad.Tests, una sola
  declaracion con bloque; clases que empiezan por ComputedParametersSymbols) y ajusta ExpressionSymbolModelTests.cs. Corre el
  filtro del contrato: compila, selecciona >= 12 y falla. Corre tambien la suite Core completa y anota cuantas fallan fuera del
  filtro (solo deben fallar las aserciones de ExpressionSymbolModelTests.cs que cambiaste). Commit RED (git add solo de esos
  archivos) con esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: G3-T1 RED publicado de G3-RACK-BUILTINS (WorkRunId R20261002T184507Z-9800); pendiente del analisis de INV-22 y la A-3. attempts = 2."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un solo commit. No esperes a la CI.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion de dotnet
  pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G3-RACK-BUILTINS; RunId R20261002T184507Z-9800; DelegationRunId R20261002T184114Z-1c2f; Initiative I-63; Gate G3; Attempt 2; BaseSha = el de arriba;
  RedSha = CurrentSha = el SHA del commit RED (= HEAD = remoto); Branch y Worktree reales; Pushed true; FilesChanged =
  git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command completo, Phase
  RED o RELEVANT, TreeSha, ResultsFile = TRX); Evidence con: (1) el resultado diagnostico observado de INV-22 (codigos, spans,
  ausencia de arbol y la orden que lo observo), (2) la API nueva que las pruebas fijan para G3-T2 (tipos, miembros, firmas y
  valores de enum), (3) por INV, la prueba que lo observa y por que falla hoy; Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE (el objetivo de T1 es el RED), PARTIAL o BLOCKED; Disposition NONE salvo parada; listas
  vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
