I-63 — DELEGACION G3-RACK-BUILTINS (G3-T2, GREEN-BUILTINS) — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-63; gate G3; tarea G3-RACK-BUILTINS (tramo T2); intento 2; RunId R20261002T214618Z-be6d;
           DelegationRunId R20261002T214309Z-19d7; AuthorityRevision ea70e3b333efec49d3c3b9bc1e5d0395adc71a01;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha 712052354e33f84da552fe0be3393df41e0521f4;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261002T214309Z-19d7
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-63-proposal-v3.md (Frozen: YES): §5 D-02; §11 D-14; §12 D-15; §13 D-16, D-18 y D-19; §8 D-17 puntos
           4-6; §17 D-23; §20 filas INV-15 y 17-28; §21; la A-1 (A-1.2 para INV-20); la A-2; la A-3
           (docs/initiatives/I-63-proposal-v3-amendment-a3-inv22-diagnostico.md, resultado de INV-22); docs/automation/decisions/I-63.md
           §2 (UNIT_DOC); AGENTS.md «Convenciones arquitectonicas», ADR-0043 D1, D9 y D24, docs/AUTOMATION_PLAN.md §16 (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: G3-T2 = el GREEN de G3. Implementar en produccion lo que fijan las pruebas del RED acreditado de G3-T1
           (637dce7e): D-16 en el nucleo, D-18 en la persistencia y D-17 (4-6) con RackComputedExpressionContext, mas la guarda R4.
           Las 89 pruebas del filtro pasan SIN modificarlas y la suite Core completa queda en verde. Un commit GREEN con push propio.
Alcance: el del paquete (= el del contrato): produccion SOLO en src/RackCad.Application/Expressions/,
           src/RackCad.Application/ComputedParameters/ y src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs;
           pruebas nuevas adicionales solo si hacen falta, en archivos NUEVOS de tests/RackCad.Tests/ComputedParameters/.
           PROHIBIDO modificar los archivos del RED de la cadena (ChainRedFiles: los siete ComputedParametersSymbols*.cs del RED y
           tests/RackCad.Tests/ExpressionSymbolModelTests.cs), las suites de INV-28 (ExpressionRoundTripTests,
           ExpressionFormatterTests, ExpressionBinderTests, G8PersistenceIntegrationContractTests), ExpressionCoreGuardTests.cs,
           ExpressionDiagnosticCatalogTests.cs, NamespaceFolderGuardTests.cs, cualquier otra prueba existente, .csproj, docs/ y
           todo lo prohibido por el contrato
Invariantes del Freeze: los del paquete aceptado (Invariants), incluidas INV-22 con la A-3 y las reglas de G3-T2
Rutas y archivos calientes: src/RackCad.Application/Expressions/ (SymbolId.cs: SymbolNamespace, SymbolNamespaces, validez de
           clave; SymbolTable.cs: SymbolScope, SymbolDefinitionKind, SymbolDefinition, SymbolEntry e indices; ExpressionBinder.cs;
           ExpressionFormatter.cs y CanonicalShape; RegistryEvaluation.cs; DependencyGraph.cs); ExpressionLexer.cs y
           ExpressionSyntaxParser.cs (SOLO LECTURA: D-16 no los cambia); src/RackCad.Application/Persistence/PersistedBoundExpressionJson.cs;
           src/RackCad.Application/ComputedParameters/ (RackMetricRequest, RackMetricResults, MetricValue y estados de G1)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParametersSymbols
           (89/89 en verde) y suite Core completa en verde; commit GREEN y push; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado; en particular C-01 (tocar algo fuera del alcance), C-02 (D-14..D-19 no
           implementables tal como estan), C-07 (el nucleo nombraria racks, sistemas o metricas mas alla de D-16, o romperia
           ExpressionCoreGuardTests), C-09 (cambiaria projectVariable o una suite de INV-28), C-10 (codigo de diagnostico nuevo),
           C-11 (el GREEN exigiria modificar un archivo de ChainRedFiles, o una expectativa del RED es incompatible con el Freeze o
           las A-n) y C-12 (cambiaria el resultado vigente de Rack.#{<clave rack de 32 caracteres hexadecimales>}); ante cualquiera,
           parar sin continuar y devolver el bloqueo estructurado (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T214618Z-be6d/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; G3-T2, continuacion autorizada de la cadena G3-RACK-BUILTINS, sin correccion)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = e99621f9; ChainRedSha = 637dce7e (RED de
  G3-T1, acreditado); ChainRedFiles = los ocho archivos de arriba. Esta entrega NO lleva RED nuevo: RedSha = null.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T214618Z-be6d/tests/<RunRef>; anota en Command la orden COMPLETA. Scripts o archivos
  auxiliares desechables, si los necesitas, solo en artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T214618Z-be6d/work/ (fuera del
  repositorio versionado). Sin AutoCAD.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- La API la fijan las pruebas protegidas del RED (leelas primero: tests/RackCad.Tests/ComputedParameters/ComputedParametersSymbols*.cs
  y tests/RackCad.Tests/ExpressionSymbolModelTests.cs). Resumen NO normativo en el punto (2) del Evidence de la entrega de T1:
  docs/automation/evidence/I-63-pilot/G3-RACK-BUILTINS/R20261002T184507Z-9800/worker-handoff.json.
- Referencia de trabajo OPCIONAL y NO normativa: durante G3-T1, una copia desechable del repositorio paso las pruebas del RED
  con una implementacion minima, cuyo parche esta en
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T184507Z-9800/work/shadow_patch.py (y
  .../work/shadow-evidence/RackComputedExpressionContext.shadow.cs.txt). Puedes leerlo para orientarte; NO es autoridad ni
  garantia: tu implementacion debe cumplir el Freeze (D-14..D-19, A-1, A-3) y las convenciones de AGENTS.md por si misma, y
  respondes de ella. No ejecutes ese script sobre el worktree sin revisar cada cambio.
- Reglas del Freeze que la implementacion debe respetar (ademas de las pruebas):
  - D-16: nucleo neutral; lexer, parser, evaluador, limites y catalogo de diagnosticos SIN cambios; DependencyGraph y
    RegistryEvaluation solo con la guarda R4 (Computed -> InvalidOperationException con «Computed» en el mensaje);
    projectVariable intacto (sus claves, comparador, busqueda, formato y persistencia).
  - D-17 (4-6): RackComputedExpressionContext en src/RackCad.Application/ComputedParameters/ (fuera de Expressions), construido
    con resultados terminados de RackMetricRequest; evaluar nunca resuelve; propagacion por estado sin valor parcial.
  - D-18: PersistedBoundExpressionJson usa la tabla de tokens persistidos de D9 (solo projectVariable): leer rack ->
    PresentButUnreadable; escribir rack -> InvalidOperationException sin escribir nada.
  - INV-22 (A-3): Rack.#{zzz} -> exactamente un InvalidQualifier (11, SyntaxAndLimits) en 5+6, sin arbol; y el resultado vigente
    de Rack.#{abcdef0123456789abcdef0123456789} (un UnexpectedToken en 5+35) NO cambia (si cambiara, parada C-12).
- Paso GREEN (un solo commit): implementa; corre el filtro del contrato hasta 89/89 en verde; despues corre UNA vez la suite
  Core completa (debe quedar en verde: incluye las suites de INV-28, ExpressionCoreGuardTests, ExpressionDiagnosticCatalogTests,
  NamespaceFolderGuardTests y las pruebas de G1 y G2), dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug y la suite
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj. Si algo falla, arregla la produccion (nunca las pruebas protegidas) y repite.
  Commit GREEN (git add solo de los archivos de produccion y, si los hubiera, de las pruebas NUEVAS) con esta linea en el
  cuerpo, en UNA sola linea, y git push:
  "Estado: G3-T2 GREEN publicado de G3-RACK-BUILTINS (WorkRunId R20261002T214618Z-be6d); RED de la cadena 637dce7e. attempts = 2."
- Antes de entregar: git diff --name-only BaseSha..HEAD no contiene NINGUN archivo de ChainRedFiles ni nada fuera del alcance.
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un solo commit. No esperes a la CI.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion de dotnet
  pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G3-RACK-BUILTINS; RunId R20261002T214618Z-be6d; DelegationRunId R20261002T214309Z-19d7; Initiative I-63; Gate G3; Attempt 2; BaseSha = el de arriba;
  RedSha = null (sin RED nuevo; el de la cadena es 637dce7e); CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y
  Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada
  corrida (RunRef unico, Command completo, Phase GREEN o RELEVANT, TreeSha, ResultsFile = TRX); Evidence con: (1) por INV, la
  prueba que lo cubre y el cambio de produccion que la pone en verde, (2) INV-22 tras el GREEN (Rack.#{zzz} y el caso de 32
  caracteres hexadecimales, observados de verdad), (3) los cambios del nucleo frente a D-16 y la confirmacion de que lexer,
  parser y catalogo no cambian; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested
  "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE,
  PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa
  y la de UI al final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
