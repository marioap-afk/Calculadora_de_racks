I-65 — DELEGACION F4-T2-SOURCE-ENTRY — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T2-SOURCE-ENTRY; intento 1; RunId R20261011T171727Z-2e11;
           DelegationRunId R20261011T171258Z-689c; AuthorityRevision cd3fb69b92390ba737d0034dc19cace83398efd5;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha cd3fb69b92390ba737d0034dc19cace83398efd5;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T171258Z-689c
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§0, §1 y §3), docs/initiatives/I-65-f4-preparation.md (§5 y §6),
           docs/initiatives/I-65-f1-ref-cant-01.md, docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes: D-05, D-08, D-12; §3
           INV-04, 05 y 06), docs/automation/decisions/I-65.md §16, §21, §24 y §26 (CD-I65-F1-14, CD-I65-F1-20, CD-I65-F3-16,
           CD-I65-F4P-06), docs/automation/evidence/I-65-f1/architect-f1-conformance.md (A65-F1-03, 08, 09, 10, 11 y 12)
           (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md, ADR-0009 y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia:
           AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T2-SOURCE-ENTRY/1/R20261011T171258Z-689c/delegation.json. En resumen: entrada neutral de una sola
           lectura del diseno persistido del sobre y endurecimiento de la fuente de Cantilever y del constructor del modelo.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs,
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralExport.cs (NUEVO),
           src/RackCad.Application/StructuralAnalysis/StructuralModelBuilder.cs, src/RackCad.Application/StructuralAnalysis/StructuralDiagnostic.cs,
           archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/ y las ediciones nombradas de dos pruebas existentes.
           Prohibido el ForbiddenWriteScope del paquete (las demas pruebas y fixtures existentes; Providers/; assets/; Persistence/;
           otros archivos de Systems; Units/; StructuralSections/; Domain, Plugin, UI, tools, cualquier .csproj, docs/, etc.)
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §3
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs (Describe: kind canonico Cantilever,
             estacion bloqueada y linea bloqueada con el mismo codigo hoy, cara, lado, brazos; Idealise; Perpendicular);
           src/RackCad.Application/StructuralAnalysis/StructuralModelBuilder.cs (StructuralScope con los bytes del diseno;
             BuildStructuralModel; lista de familias Channel y Angle como rechazo; source.Describe sin proteger);
           src/RackCad.Application/StructuralAnalysis/StructuralDiagnostic.cs (StructuralDiagnosticCodes; SOURCE-UNAVAILABLE ya existe);
           src/RackCad.Application/Persistence/ (RackProjectStore.Deserialize; RackEmbedDocument.KindCantilever = "cantilever");
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisCantileverFixtures.cs (helpers de REF-CANT-01: bytes del diseno,
             cargas, catalogo; solo lectura) y StructuralAnalysisCantileverGoldenTests.cs (golden de INV-06; solo lectura)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisSourceEntry,
           minimo 15, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); en particular C-01, C-03 (el golden
           de INV-06 deja de reproducirse), C-07 (clasificar una pieza fuera de lo que fija CD-I65-F1-14) y C-08; ante cualquiera,
           parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T2-SOURCE-ENTRY/1/R20261011T171727Z-2e11/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F4-T2-SOURCE-ENTRY)
- Celda: la del paquete aceptado. ChainBaseSha = BaseSha = cd3fb69b92390ba737d0034dc19cace83398efd5; ChainRedSha = null y ChainRedFiles = []: esta entrega
  EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F4-T2-SOURCE-ENTRY/1/R20261011T171727Z-2e11/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F4-T2-SOURCE-ENTRY/1/R20261011T171727Z-2e11/work/.
  PROHIBIDO invocar un interprete Python por cualquier motivo (ni para comprobar si existe; usa pwsh si necesitas un script);
  sin AutoCAD y sin RAM Elements. Escribe los archivos con las herramientas de escritura y edicion (no con heredocs de Bash).
  La suite Core de la CI corre en ubuntu-latest: las pruebas deben pasar en Linux y en Windows.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura de I-65-f4-implementation.md §3 aplicada (no son decisiones nuevas; si algo con efecto observable no esta decidido ahi,
  en el Freeze ni en CD-I65-F1-14, no lo inventes: parada S-03 con la pregunta exacta; nombres libres salvo el archivo de la entrada):
  - Entrada (A65-F1-08): clase publica en CantileverStructuralExport.cs (namespace RackCad.Application.Systems.Cantilever, bloque)
    con un metodo que recibe rackId, el kind del sobre tal como esta persistido, stationIndex, designBytes (UTF-8 del texto Design
    del sobre), loadFileBytes, el perfil de material (IStructuralMaterialProfile) y el catalogo, y devuelve StructuralModelResult. Decodifica
    designBytes como UTF-8, deserializa con RackProjectStore, toma su CantileverLineDesign, construye StructuralScope con ESOS mismos
    bytes y el kind canonico de la fuente, lee las cargas con StructuralLoadFileReader y llama al constructor. El kind del sobre
    "cantilever" (RackEmbedDocument.KindCantilever) se compara sin distinguir mayusculas; otro kind -> el diagnostico existente de kind
    distinto, sin deserializar. Un diseno sin linea Cantilever -> Invalid con un codigo (nuevo o existente) sin excepcion.
  - Excepciones (A65-F1-09): en BuildStructuralModel, la llamada a source.Describe va protegida: una excepcion (salvo
    OutOfMemoryException) -> resultado Unavailable con el diagnostico existente de fuente no disponible y sin modelo; en la entrada,
    igual para la decodificacion y la deserializacion. Los ArgumentNullException de argumentos nulos se conservan. El constructor
    rechaza como Invalid con codigos nuevos: modelo sin nudos o sin miembros; dos apoyos en el mismo nudo; un id de supuesto
    referenciado (por apoyos, miembros o liberaciones) que no esta en la lista de supuestos.
  - Bloqueo de linea (A65-F1-11): en Describe, line.IsBlocked -> un codigo nuevo con sujeto "line"; la estacion bloqueada conserva su
    codigo y su sujeto station-N.
  - Familias (A65-F1-03): lista de permiso W, HssRectangular y S en el constructor; cualquier otro valor (Channel, Angle o un valor no
    definido del enum) -> el diagnostico existente de familia no soportada.
  - Re-ortogonalizacion (A65-F1-10): en Perpendicular, si |along| (la componente que se elimina) supera Math.Sin(0.01 grados) -> la
    estacion es Unsupported con un codigo nuevo y sin modelo (sin excepcion); dentro de la tolerancia, el resultado es el de hoy.
  - Inventario (A65-F1-12, CD-I65-F1-14): una funcion pura que clasifica cada pieza estructural de la estacion resuelta en modelada
    (columna, cuerpo de brazo) o no modelada (base, placas, separadores, arriostramientos, barras redondas, cartelas y demas piezas que
    el resolver produce hoy para la estacion); una pieza fuera de ambas listas -> Unsupported con un codigo nuevo. La lista de no
    modeladas se fija a partir de las piezas que el resolver produce de verdad para REF-CANT-01 (anotalas en Evidence); si una pieza
    real no encaja en CD-I65-F1-14, parada C-07 con la pieza exacta. Prueba con una pieza sintetica no clasificada.
  - Golden: REF-CANT-01, por la fuente y por la entrada, reproduce el golden de INV-06 sin cambio (C-03 si no).
  - Pruebas existentes: solo en StructuralAnalysisModelBuilderTests.cs y StructuralAnalysisCantileverBoundaryTests.cs, y solo las
    aserciones que encodan el comportamiento viejo de A65-F1-03, 09 u 11 (p. ej. una prueba que espere el codigo de estacion bloqueada
    para una linea bloqueada); anota cada edicion y su motivo en Evidence.
- Paso RED (un solo commit): esqueleto COMPILABLE de la entrada (cuerpo que lanza NotImplementedException o devuelve un valor que las
  pruebas rechazan) y codigos nuevos declarados si hacen falta para compilar; pruebas NUEVAS (namespace RackCad.Tests, clases
  StructuralAnalysisSourceEntry*), >= 15 seleccionables, que fallan por asercion o excepcion (nunca compilacion); y las ediciones de
  las dos pruebas existentes. Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si hiciera falta: parada, sin editarlo). Revisa bien tus
  aserciones ANTES del commit RED: en la tarea anterior una asercion defectuosa del RED obligo a una correccion.
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota totales);
  comprueba el alcance del diff y que en tests/ solo hay altas y las dos ediciones; commit GREEN con esta linea en el cuerpo, en UNA
  sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-T2-SOURCE-ENTRY (WorkRunId R20261011T171727Z-2e11) pendiente de la verificacion del Controller. attempts = 1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F4-T2-SOURCE-ENTRY; RunId R20261011T171727Z-2e11; DelegationRunId R20261011T171258Z-689c; Initiative I-65; Gate F4; Attempt 1; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) por correccion (A65-F1-03, 08, 09, 10, 11 y 12) la prueba que la observa; (2) las aserciones editadas de pruebas existentes,
  archivo y motivo; (3) los codigos nuevos; (4) la clasificacion de las piezas de REF-CANT-01; (5) la confirmacion del golden de
  INV-06; (6) las pruebas que pasan ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5",
  EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus
  IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Itera con el filtro focal y corre la suite completa al final. Al terminar, tu respuesta final es una linea con
  WorkerStatus, RedSha y CurrentSha.
