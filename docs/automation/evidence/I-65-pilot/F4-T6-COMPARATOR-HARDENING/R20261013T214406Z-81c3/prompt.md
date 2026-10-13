I-65 — DELEGACION F4-T6-COMPARATOR-HARDENING — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T6-COMPARATOR-HARDENING; intento 3; RunId R20261013T214406Z-81c3;
           DelegationRunId R20261013T213942Z-4bec; AuthorityRevision ef3536156f6a69cdee42a687930877eefc4a585b;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha ef3536156f6a69cdee42a687930877eefc4a585b;
           rama architecture/interop-analisis-estructural;
           worktree C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural;
           Owner subagent:R20261013T213942Z-4bec
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§5, §9 y §10), docs/automation/decisions/I-65.md (§36, §40 y §41: OD-GO,
           CD-I65-F4-31 a 34), docs/automation/evidence/I-65-f4/architect-f4-t1-t4-conformance.md (observaciones A65-F4-01..06),
           docs/initiatives/I-65-f4-engineering-validation-package.md (§2 y §3), docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes:
           D-11, D-12, D-13; §3 INV-13; §4) (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md y docs/AUTOMATION_PLAN.md §16 (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T213942Z-4bec/delegation.json (ruta relativa al worktree; leela con
           su ruta ABSOLUTA). En resumen: endurecer el comparador neutral, dos lectores del proveedor y la herramienta de consola de
           F4-T4 segun A65-F4-01..05 y anadir el ensayo general A65-F4-06, sin cambiar el Freeze ni sus tolerancias.
Alcance: el del paquete (= el del contrato): src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs,
           src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs (solo anadidos),
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamTransferReaders.cs y RamDumpReader.cs,
           tools/RackCad.StructuralAnalysis.RamTool/ (sin su .csproj), archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/ y de
           tests/RackCad.Tests/StructuralAnalysis/Fixtures/rehearsal/, y las ediciones nombradas de StructuralAnalysisComparisonRowTests.cs,
           StructuralAnalysisComparisonToolTests.cs y StructuralAnalysisComparisonSupport.cs. Prohibido el ForbiddenWriteScope del
           paquete y cualquier otro cambio.
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §10
Rutas y archivos calientes (solo lectura salvo lo indicado): RamBuildScriptTemplate.cs (funcion read_back: claves exactas de
           «values»), RamDumpScriptTemplate.cs, RamToolInject.cs (marca de inyeccion), StructuralAnalysisRamElementsDump*.cs y
           StructuralAnalysisRamElementsHardening*.cs (guardas de INV-14 y G1), los fixtures de F1 y F3
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisComparison,
           minimo 101, RED esperado; suite Core completa en verde y solucion en Release sin advertencias nuevas en el GREEN; commit
           RED y push; commit GREEN y push con el resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:/Documentos/Codex/Calculadora de racks (nunca escribas ni leas alli; es el directorio de proceso de la sesion, no el tuyo)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10, C-12, C-13); ante cualquiera,
           parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions).
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T214406Z-81c3/worker-handoff.json (dentro del worktree);
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F4-T6-COMPARATOR-HARDENING)
- Celda: la del paquete aceptado. ChainBaseSha = BaseSha = ef3536156f6a69cdee42a687930877eefc4a585b; ChainRedSha = null y ChainRedFiles = []: esta entrega
  EXIGE RED.
- CONTROLES DEL OWNER (decisiones §41, CD-I65-F4-32; condicion C-12). Tu directorio de proceso NO es el worktree: es el checkout
  principal del Owner (D:/Documentos/Codex/Calculadora de racks), zona prohibida. En esta unidad un Worker anterior reescribio un
  archivo de ese checkout con una ruta relativa ([IO.File] tras Set-Location). Por eso:
  1. PRIMERA ACCION, antes de leer o escribir nada: informa pwd (Bash), y en PowerShell Get-Location y [Environment]::CurrentDirectory;
     anotalos tal cual en la entrega (Evidence) aunque no sean el worktree.
  2. Toda operacion sobre archivos con ruta ABSOLUTA bajo C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural
     (herramientas de lectura, escritura y edicion, y cualquier API .NET); git SIEMPRE con
     git -C C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural ...; dotnet con la ruta absoluta del
     proyecto o de la solucion. NUNCA una ruta relativa en una API de archivos ni dependas de cd o Set-Location entre llamadas (cada
     llamada de Bash vuelve a empezar en el checkout principal). Si alguna vez escribes fuera del worktree, para (C-12) y declaralo.
  La sesion fotografia el checkout principal y los demas worktrees antes y despues de tu trabajo: cualquier cambio alli es una parada.
- Entorno: Windows. dotnet: usa SIEMPRE /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T214406Z-81c3/tests/<RunRef>;
  anota en Command la orden COMPLETA. Scripts auxiliares, si los necesitas, solo en
  .../artifacts/orchestration/I-65/F4-T6-COMPARATOR-HARDENING/3/R20261013T214406Z-81c3/work/ (ruta absoluta).
  PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3, py, ni siquiera --version ni
  dentro de otra orden; en esta unidad ya hubo cinco desviaciones por eso, siempre sondas «--version» o «print»: activan la parada
  C-08). NO compruebes la disponibilidad de ninguna herramienta (ninguna orden «<herramienta> --version» salvo git y dotnet) y revisa
  cada orden antes de lanzarla; usa pwsh o las herramientas de lectura; sin AutoCAD y sin RAM Elements. Escribe los archivos con las
  herramientas de escritura y edicion (no con heredocs de Bash). La suite Core de la CI corre en ubuntu-latest: pruebas validas en
  Linux y en Windows; rutas temporales con Path.GetTempPath(); fixtures leidos con rutas relativas al ensamblado o al repositorio
  como hacen las pruebas existentes de F1 (nunca rutas absolutas de esta maquina dentro del codigo); contenido generado con "\n".
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura de I-65-f4-implementation.md §10 y de las decisiones §41 aplicada (no son decisiones nuevas; si algo con efecto observable
  no esta decidido ahi, en el paquete de ingenieria ni en el Freeze, no lo inventes: parada S-03 con la pregunta exacta):
  - A65-F4-01: Delta de nudos = distancia euclidea; constante de tolerancia sin cambio (0.02 in) con su epsilon; nota de la fila
    actualizada. Valor de carga: diferencia relativa con el valor del MANUAL como base; si el manual es 0 -> |g| <= 1e-6 lbf con una
    constante con nombre (p. ej. ZeroLoadAbsoluteToleranceLbf) y sin division; localiza que argumento es el manual en la llamada actual
    a PercentDelta y no cambies ninguna otra fila.
  - A65-F4-02: anade Incomplete al final de StructuralComparisonOutcome (valor JSON INCOMPLETE) sin renumerar los existentes; el
    global es Incomplete cuando la fila de combinaciones sale NOT_EVALUATED por no leida y ninguna fila falla. En la herramienta:
    --combinations-decision <texto> obligatoria con cualquiera de las dos opciones de combinaciones no leidas (sin ella o en blanco
    -> InvalidUsage), el texto se escribe en el informe (campo nuevo, solo cuando hay decision) y el codigo de salida
    RamToolExitCodes.Incomplete es distinto de todos los existentes; actualiza el texto de uso.
  - A65-F4-03: lee en RamBuildScriptTemplate.cs la funcion read_back y la composicion de «values» (claves exactas de nodes, members,
    loads y loadCase, y que loadCase puede ser null); RamReadbackReader las lee de forma ESTRICTA (claves desconocidas o tipos
    erroneos -> Invalid con los codigos existentes; los codigos actuales de «sin valores» y «valores como lista» no cambian y la
    prueba existente con arrays vacios y loadCase null sigue Valid) hacia un tipo neutral nuevo sin identidad de proveedor, expuesto
    por StructuralTransferReadback con un constructor ADICIONAL (el existente se conserva). CompareReadbacks: con la lectura de vuelta
    aprobada y la huella del manifiesto correcta, comprueba que la lectura del volcado del GENERADO coincide con esos valores (ids y
    coordenadas de nudos dentro de 1e-6 in; miembros con nudos, seccion y material; cargas por miembro con valor y distancia dentro de
    1e-6; recuentos iguales); desajuste o valores ausentes -> Rejected con el primer desajuste en el motivo. En la herramienta: misma
    ruta o mismo SHA-256 de bytes para --generated y --manual -> REJECTED; seccion inputs del informe (huellas y SHA-256 en minusculas
    de las cuatro entradas) SOLO cuando la herramienta la aporta; el informe sin inputs no cambia ni un byte.
  - A65-F4-04: solo en tools/: compare reconoce la marca (RamToolInjection.Marker) en el manifiesto, nunca aprueba el paquete y evalua
    la lectura de vuelta (fallo -> REJECTED citando la inyeccion; no fallida -> REJECTED: un paquete de inyeccion no puede aprobarse),
    con el codigo Rejected; src/ no cambia frente a la marca.
  - A65-F4-05: RamDumpReader: categoria ausente o null -> Invalid; solo espacios -> Invalid; cadena vacia exacta valida SOLO en casos sin
    cargas de miembro; vacia con cargas -> Invalid con un diagnostico que nombra el caso; comentario: la semantica del programa es NO
    MEDIDA hasta la validacion real (decisiones §41); no supongas nada mas de la API (C-13).
  - A65-F4-06: fixtures NUEVOS JSON en tests/RackCad.Tests/StructuralAnalysis/Fixtures/rehearsal/ (volcado del generado, su lectura de
    vuelta y los volcados manuales) construidos con los valores del paquete de ingenieria §2 para REF-CANT-01 (4 decimales; giro -90 y
    270; porcentajes con 3 decimales; otros ids y orden; caso CM sin cargas) y una clase de prueba que espera PASS en las dos variantes
    y FAIL con +90, con porcentaje de 1 decimal y con un miembro invertido. Si un manual correcto no da PASS por un defecto del
    comparador, corrigelo en StructuralReadbackComparison.cs sin tocar tolerancias; si la causa es otra: parada C-03.
  - Ediciones nombradas (y solo con este fin): RowTests -> la prueba Combinations_a_dump_could_not_read_are_not_evaluated espera
    Incomplete; ToolTests -> pruebas y ayudantes cuyo comportamiento cambia (volcados iguales, values coherentes del paquete,
    combinaciones, inyeccion); Support -> valores de lectura de vuelta coherentes y una opcion para desajustarlos. Ninguna otra
    asercion existente se debilita.
  - INV-14 (CD-I65-F4-13): ninguna prueba nueva escribe el nombre del modulo del proveedor ni del interprete en el codigo (ni partido
    en varios literales: una guarda de F4-T1 lo detecta); si una prueba lo necesita, lo obtiene de
    StructuralAnalysisRamElementsHardeningScans.ToolName() y compone las cadenas en tiempo de ejecucion. La exencion de INV-14 sigue en
    sus cuatro rutas exactas. G1/G2: el tipo neutral nuevo y StructuralReadbackComparison.cs no contienen tokens del proveedor. Corre
    las autopruebas de INV-14, la de literales partidos y las guardas de G1 antes del commit RED.
- Paso RED (un solo commit): esqueleto COMPILABLE (Incomplete, RamToolExitCodes.Incomplete, el tipo neutral de valores, el constructor
  adicional y las firmas nuevas sin comportamiento: cuerpos que lanzan NotImplementedException o devuelven valores que las pruebas
  rechazan), todas las pruebas NUEVAS (namespace RackCad.Tests, clases StructuralAnalysisComparisonHardening*), los fixtures y las
  ediciones nombradas; la seleccion del filtro (>= 101) falla por asercion o excepcion (nunca compilacion). Commit RED (git add solo de
  archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas ni fixture del RED (si hiciera falta: parada, sin editarlo). Revisa bien
  tus aserciones ANTES del commit RED (literales con comillas o barras, saltos de linea, orden, conteos): en esta unidad dos aserciones
  defectuosas del RED obligaron a correcciones (F4-T1 y F4-T4).
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde), suite Core completa en verde y
  dotnet build C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/RackCad.sln -c Release (anota
  totales; ninguna advertencia CS, MSB, NU o NETSDK en archivos del diff); comprueba el alcance del diff y que en tests/ solo hay altas y
  las tres ediciones nombradas; commit GREEN con esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-T6-COMPARATOR-HARDENING (WorkRunId R20261013T214406Z-81c3) pendiente de la verificacion del Controller. attempts = 3."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas ni de nombres de prueba (si un nombre de prueba los
  contiene, nombralo por su archivo y numero de linea): GATE PASS, GATE_PASS, Candidato, Candidate, FINAL_CANDIDATE_SHA, gate cerrado,
  gate closed, cierre del gate, rama integrada, integrada en main, integrated into main, merged into main, lista para integrar,
  ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile usando rutas absolutas):
  TaskId F4-T6-COMPARATOR-HARDENING; RunId R20261013T214406Z-81c3; DelegationRunId R20261013T213942Z-4bec; Initiative I-65; Gate F4; Attempt 3; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) el directorio efectivo informado al empezar (pwd, Get-Location, [Environment]::CurrentDirectory) y la declaracion de que toda
  operacion de archivos uso rutas absolutas del worktree; (2) por invariante la prueba que lo observa; (3) el informe serializado de un
  caso PASS del ensayo y de un caso INCOMPLETE; (4) la salida de dotnet build RackCad.sln -c Release (errores y advertencias); (5) las
  pruebas que pasan ya en el RED y por que; (6) el diff de cada edicion nombrada y por que no debilita lo observado; Worker = {
  Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5
  <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si
  no hay nada.
- Tope: 60 minutos. Itera con el filtro focal y corre la suite completa al final. Al terminar, tu respuesta final es una linea con
  WorkerStatus, RedSha y CurrentSha.
