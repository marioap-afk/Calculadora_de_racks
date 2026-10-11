I-65 — DELEGACION F4-T4-COMPARATOR-AND-TOOL — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T4-COMPARATOR-AND-TOOL; intento 2; RunId R20261011T202738Z-6356;
           DelegationRunId R20261011T202527Z-1167; AuthorityRevision 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T202527Z-1167
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§0, §1, §5 y §7), docs/initiatives/I-65-f4-engineering-validation-package.md
           (§2 y §3, en particular la tabla de evaluadores de §3 F), docs/initiatives/I-65-f4-preparation.md (§5 y §6),
           docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes: D-11, D-12, D-13; §3 INV-13; §4), docs/automation/decisions/I-65.md
           §21, §26, §27 y §29 (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia:
           AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/2/R20261011T202527Z-1167/delegation.json. En resumen: comparador neutral puro
           CompareReadbacks con informe determinista, y herramienta de consola fuera del producto (script de volcado, comparacion,
           inyeccion de fallos).
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs
           (NUEVO), tools/RackCad.StructuralAnalysis.RamTool/ (NUEVO), RackCad.sln (solo la entrada del proyecto nuevo),
           tests/RackCad.Tests/RackCad.Tests.csproj (solo la ProjectReference nueva) y archivos NUEVOS de
           tests/RackCad.Tests/StructuralAnalysis/. Prohibido el ForbiddenWriteScope del paquete (todas las pruebas existentes; el
           proveedor y los tipos de lectura; Systems/; assets/; tools/RackCad.StructuralSections.Import/; Domain, Plugin, UI, docs/, etc.)
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §5
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs (tipos neutrales de lectura; NO los cambies: si no bastan,
             parada C-03); lectores del proveedor en src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (volcado,
             lectura de vuelta, manifiesto y plantilla del volcado con su Render; RamBuildPackageFactory para la regla de carpeta);
           src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs (StructuralPackagePublisher: unica E/S de carpetas
             nuevas atomicas, nunca sobrescribe);
           tools/RackCad.StructuralSections.Import/ (precedente: .csproj net8.0 Exe con RollForward LatestMajor, ProjectReference a
             Application, InternalsVisibleTo RackCad.Tests; Program.Main con codigos de salida; las pruebas llaman clases, no Main);
           RackCad.sln (carpeta de solucion tools) y tests/RackCad.Tests/RackCad.Tests.csproj (ProjectReference de la herramienta existente)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisComparison,
           minimo 25, RED esperado; suite Core completa en verde y solucion en Release sin advertencias en el GREEN; commit RED y push;
           commit GREEN y push con el resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); en particular C-03 (los tipos de
           lectura no bastan), C-07 (tolerancias o validacion estructural) y C-08 (Python por cualquier motivo); ante cualquiera, parar
           sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/2/R20261011T202738Z-6356/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F4-T4-COMPARATOR-AND-TOOL)
- Celda: la del paquete aceptado. ChainBaseSha = BaseSha = 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50; ChainRedSha = null y ChainRedFiles = []: esta entrega
  EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/2/R20261011T202738Z-6356/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/2/R20261011T202738Z-6356/work/.
  PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3, py, ni siquiera --version ni
  dentro de otra orden; en esta unidad ya hubo cuatro desviaciones por eso, la ultima en F4-T3: una sonda «python --version»
  anadida al final de una comprobacion del texto libre, que activo la parada C-08). NO compruebes la disponibilidad de ninguna
  herramienta (ninguna orden «<herramienta> --version» salvo git y dotnet) y revisa cada orden antes de lanzarla; usa pwsh o las
  herramientas de lectura; sin AutoCAD y sin RAM Elements. Escribe los archivos con las herramientas de escritura y edicion (no con
  heredocs de Bash). La suite Core de la CI corre en ubuntu-latest: pruebas validas en Linux y en Windows; rutas temporales con
  Path.GetTempPath(); contenido generado con "\n" explicito.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura de I-65-f4-implementation.md §5 aplicada (no son decisiones nuevas; si algo con efecto observable no esta decidido ahi, en el
  paquete de ingenieria §3 F ni en el Freeze, no lo inventes: parada S-03 con la pregunta exacta; nombres de tipos libres salvo los
  archivos y el proyecto):
  - Comparador: CompareReadbacks(generado, manual, transferencia, huella del manifiesto) -> informe; filas segun la tabla de evaluadores
    del paquete de ingenieria §3 F, con los identificadores estables de fila en el informe; tolerancias del Freeze §4 como constantes con
    nombre (0.02 in; 0.1 grados; 0.1 por ciento); angulo entre vectores por acos del producto escalar normalizado con recorte a [-1, 1].
  - Informe: JSON determinista rackcad-structural-comparison/v1 (orden estable, cultura invariante, LF, sin marcas de tiempo).
  - Herramienta: Program.Main con subordenes dump-script, compare e inject (nombres libres pero documentados en el uso), argumentos
    explicitos (--folder, --package, --generated, --manual, --report, --fault, --out o equivalentes) y codigos de salida distintos para
    PASS, FAIL o rechazo, y uso o entrada invalida; las pruebas llaman a clases internas de la herramienta (InternalsVisibleTo), no a
    procesos. La inyeccion lee el paquete valido con los lectores, decodifica la carga util del script, aplica UNA mutacion (seccion
    inexistente que conserva el formato con espacio; l2 esperado de un miembro invertido; version esperada distinta; una funcion del
    nucleo inexistente anadida a la lista exigida), vuelve a renderizar el script con la plantilla de construccion y la carpeta embebida
    nueva, y escribe un manifiesto marcado (campo de inyeccion con el tipo de fallo, la mutacion y el SHA-256 del paquete de origen) en
    una carpeta NUEVA con el escritor atomico; la carpeta de origen no se toca.
  - Solucion y referencia: el proyecto nuevo bajo la carpeta de solucion tools de RackCad.sln (con su configuracion Debug y Release como
    el precedente) y una unica ProjectReference en tests/RackCad.Tests/RackCad.Tests.csproj; comprueba dotnet build RackCad.sln -c
    Release sin advertencias.
  - INV-14 (CD-I65-F4-13): ninguna prueba nueva escribe el nombre del modulo del proveedor ni del interprete en el codigo (ni partido
    en varios literales: una guarda de F4-T1 lo detecta); si una prueba lo necesita, lo obtiene de
    StructuralAnalysisRamElementsHardeningScans.ToolName() y compone las cadenas en tiempo de ejecucion. La exencion de INV-14 sigue en
    sus cuatro rutas exactas. Corre las autopruebas de INV-14 y la de literales partidos antes del commit RED.
- Paso RED (un solo commit): esqueleto COMPILABLE del comparador y de la herramienta (cuerpos que lanzan NotImplementedException o
  devuelven valores que las pruebas rechazan), con el proyecto, la entrada de la solucion y la referencia; pruebas NUEVAS (namespace
  RackCad.Tests, clases StructuralAnalysisComparison*), >= 25 seleccionables, que fallan por asercion o excepcion (nunca compilacion).
  Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si hiciera falta: parada, sin editarlo). Revisa bien tus
  aserciones ANTES del commit RED (literales con comillas o barras invertidas, saltos de linea, orden de filas): en F4-T1 una asercion
  defectuosa del RED obligo a una correccion.
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde), suite Core completa en verde y solucion en Release sin
  advertencias (anota totales); comprueba el alcance del diff y que en tests/ solo hay altas (ademas del .csproj de pruebas); commit GREEN
  con esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-T4-COMPARATOR-AND-TOOL (WorkRunId R20261011T202738Z-6356) pendiente de la verificacion del Controller. attempts = 2."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F4-T4-COMPARATOR-AND-TOOL; RunId R20261011T202738Z-6356; DelegationRunId R20261011T202527Z-1167; Initiative I-65; Gate F4; Attempt 2; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) por invariante la prueba que lo observa; (2) el informe serializado de un caso PASS y de un caso FAIL; (3) la salida de dotnet
  build RackCad.sln -c Release (errores y advertencias); (4) el uso de la herramienta y sus codigos de salida; (5) las pruebas que pasan
  ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium", Trailer
  "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition
  NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Itera con el filtro focal y corre la suite completa al final. Al terminar, tu respuesta final es una linea con
  WorkerStatus, RedSha y CurrentSha.
