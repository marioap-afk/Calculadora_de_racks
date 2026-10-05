I-63 — DELEGACION G1-RACK-METRICS — Worker, perfil ROUTINE_IMPLEMENTATION (correccion 2)
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 2; RunId R20261002T143849Z-3698;
           DelegationRunId R20261002T143522Z-0c4d; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha a2d2b0a60709156b557292eb019957898db1fb09;
           rama architecture/parametros-calculados-resumen-proyecto;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-parametros-calculados-resumen-proyecto;
           Owner subagent:R20261002T143522Z-0c4d
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en particular
           docs/initiatives/I-63-proposal-v3.md §5 (D-02: identidad MetricId, tabla de los seis MetricId del catalogo V1 y
           regla del token) y §6 (D-05) (UNIT_DOC, en AuthorityRevision); AGENTS.md «Convenciones arquitectonicas»
           (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Contexto (no es autoridad): analysis.md del Coordinator que motiva esta correccion,
           docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T062554Z-e271/analysis.md
Objetivo: anadir la prueba explicita de D-02 que falta. El catalogo productivo ya declara bien los seis MetricId
           (GREEN b5ee157d); falta una prueba que lo verifique en los dos sentidos y la regla del token.
Alcance: el del paquete (= el del contrato): permitido src/RackCad.Application/ComputedParameters/ y
           tests/RackCad.Tests/ComputedParameters/; prohibido el ForbiddenWriteScope del paquete. RESTRICCION DE ESTA
           CORRECCION (Objective y AcceptanceCriteria del paquete, orden del Coordinator): creas SOLO el archivo nuevo
           tests/RackCad.Tests/ComputedParameters/RackMetricIdsTests.cs y no modificas ningun otro, en particular los siete
           archivos de src/RackCad.Application/ComputedParameters/, los dos archivos existentes de
           tests/RackCad.Tests/ComputedParameters/ (RT protegidos de la cadena), tests/RackCad.Tests/NamespaceFolderGuardTests.cs,
           cualquier .csproj y docs/
Invariantes del Freeze: los del paquete aceptado (INV-04, 05, 09, 14, 16, 34 via de peticion, D-02, D-08, D-28; ningun
           consumidor existente cambia de comportamiento). Esta correccion solo anade la prueba de D-02; las pruebas
           existentes de los INV siguen en verde sin cambios
Rutas y archivos calientes (solo lectura): src/RackCad.Application/ComputedParameters/RackMetricIds.cs (MetricScope,
           MetricId, RackMetricIds.All, RackMetricRequest no se toca); tests/RackCad.Tests/ComputedParameters/ (estilo de
           las pruebas existentes); tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda de I-23: namespace
           RackCad.Tests, una sola declaracion, al principio de linea)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado; en particular la prueba nueva cumple el oraculo
           de abajo, el filtro del contrato la selecciona, un solo commit que solo anade el archivo nuevo, y la suite Core
           completa queda en verde (incluida NamespaceFolderGuardTests)
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.ComputedParameters
           (seleccion >= 6, todas en verde); filtro FullyQualifiedName~ComputedParametersRackMetricIds (las pruebas nuevas,
           seleccion > 0, en verde); suite Core completa en verde; commit y push con el resumen de estado;
           worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli); nada fuera del archivo nuevo
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-06); en particular C-05
           (cualquier escritura fuera del archivo nuevo): si la prueba revela que hace falta cambiar produccion o las
           pruebas protegidas, NO lo hagas: parada S-04 con la evidencia, sin commit; ante cualquiera, parar sin
           continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T143849Z-3698/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; correccion 2 de la cadena, AUTOMATION_PLAN 16.8)
- Celda: claude-sonnet-5-5 x subagente, effort medium (Routine). BaseSha = a2d2b0a60709156b557292eb019957898db1fb09; ChainBaseSha =
  f71de12b33567e8b4109d3a17c60a17d54e96fdc; ChainRedSha = fc30dc6cf0d2f97e6732bbb8c512623db91c305d (acreditado);
  ChainRedFiles = tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs y
  tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs. Tu diff NO toca ChainRedFiles: esta entrega NO exige
  RED (16.8). Un solo commit; RedSha = null en la entrega.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T143849Z-3698/tests/<RunRef>; anota en Command la orden COMPLETA, con esos
  argumentos. Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T143849Z-3698/work/.
  Sin AutoCAD.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/parametros-calculados-resumen-proyecto = BaseSha. Si no, parada S-12/S-13.
- La prueba (archivo nuevo, namespace RackCad.Tests, clase ComputedParametersRackMetricIdsTests, xunit como las demas):
  1. Oraculo congelado (Proposal V3 §5 D-02 y §6 D-05), los seis pares en este orden: (Rack, "frentes"),
     (Rack, "frentesVacios"), (Project, "totalRacks"), (Project, "rackCount"), (Project, "totalFrentes"),
     (Project, "totalFrentesVacios"). Es la unica tabla de la prueba y viene del Freeze, no del codigo.
  2. (scope, token) -> MetricId: para cada par, new MetricId(scope, token) esta en RackMetricIds.All (catalogo productivo
     leido en la prueba) y es igual al MetricId productivo correspondiente; y MetricId -> (scope, token): para cada
     MetricId de RackMetricIds.All, su (Scope, Token) es uno de los seis pares congelados.
  3. Catalogo cerrado: RackMetricIds.All tiene exactamente seis elementos distintos y su conjunto de (Scope, Token) es
     igual al de los seis pares congelados.
  4. Regla del token de D-02 sobre cada token productivo: ASCII, empieza por minuscula, solo letras y digitos
     (^[a-z][A-Za-z0-9]*$); y comparador Ordinal: MetricId(scope, token) != MetricId(scope, token con otra caja, p. ej.
     "Frentes"), sin que eso lo exija ningun otro cambio.
  5. Nada de comparar el codigo consigo mismo (p. ej. RackMetricIds.Frentes == RackMetricIds.Frentes): cada asercion
     enfrenta el catalogo productivo con el oraculo congelado.
- Paso unico (GREEN):
  1. Crea el archivo y corre el filtro de la prueba nueva y el filtro del contrato: todo en verde. Si alguna asercion
     falla contra el codigo productivo, NO cambies produccion: parada S-04 con el detalle, sin commit.
  2. Suite Core completa tests/RackCad.Tests/RackCad.Tests.csproj en verde (anota totales).
  3. Commit (git add solo del archivo nuevo) con esta linea en el cuerpo, en UNA sola linea, y git push:
     "Estado: G1 en curso; entrega del Worker de G1-RACK-METRICS (WorkRunId R20261002T143849Z-3698, correccion 2) pendiente de la verificacion del Controller. attempts = 2."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un solo commit.
  No esperes a la CI: el Principal recoge la corrida push.
- Texto libre de la entrega (WorkCompleted, Evidence, UnexpectedFindings, KnownLimitations, Deviations, OpenQuestions,
  RecommendedNextAction): ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into
  main, merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId G1-RACK-METRICS; RunId R20261002T143849Z-3698; DelegationRunId R20261002T143522Z-0c4d; Initiative I-63; Gate G1; Attempt 2;
  BaseSha = el de arriba; RedSha = null; CurrentSha = el SHA del commit (= HEAD = remoto); Branch y Worktree reales;
  Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida
  (RunRef unico, Command completo, Phase GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios,
  ResultsFile = TRX); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium",
  Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
