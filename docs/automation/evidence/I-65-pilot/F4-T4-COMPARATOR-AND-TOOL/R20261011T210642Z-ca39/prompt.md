I-65 — DELEGACION F4-T4-COMPARATOR-AND-TOOL (CORRECCION) — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T4-COMPARATOR-AND-TOOL; intento 3; RunId R20261011T210642Z-ca39;
           DelegationRunId R20261011T210328Z-2ee6; AuthorityRevision 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha c8f3aa884abbc86c1c0cf4ff112dadda0c26fa68;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T210328Z-2ee6
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§0, §1, §5 y §7), docs/initiatives/I-65-f4-engineering-validation-package.md
           (§2 y §3), docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes: D-11, D-12, D-13; §3 INV-13; §4) (UNIT_DOC);
           ADR-0050 (UNIT_CHANGE); AGENTS.md y docs/AUTOMATION_PLAN.md §16, en especial 16.8 (EXTERNAL); vigencia: AuthorityRevision
           para la unidad, MainSha para lo demas
Analisis de la correccion (se cita por CorrectionOf; no es autoridad):
           docs/automation/evidence/I-65-pilot/F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/analysis.md
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210328Z-2ee6/delegation.json. En resumen: CORREGIR las dos aserciones
           defectuosas del RED de la cadena y sus dos advertencias, con un RED honesto por implementacion desactivada y un GREEN que
           restaura la implementacion de 0e9bbc29 sin cambiarla.
Alcance: el del paquete (= el del contrato). Archivos que esta correccion toca: tests/RackCad.Tests/StructuralAnalysis/
           StructuralAnalysisComparisonToolTests.cs y StructuralAnalysisComparisonSupport.cs (solo lo indicado abajo);
           src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs y tools/RackCad.StructuralAnalysis.RamTool/
           (solo desactivar en el RED y restaurar en el GREEN). Prohibido el ForbiddenWriteScope del paquete y cualquier otro cambio.
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §5
Rutas y archivos calientes (solo lectura salvo lo indicado): los commits b51adf28 (RED de la cadena: esqueleto) y 0e9bbc29 (GREEN del
           intento 2: implementacion completa); worker-handoff.json del intento 2 en
           docs/automation/evidence/I-65-pilot/F4-T4-COMPARATOR-AND-TOOL/R20261011T202738Z-6356/worker-handoff.json (UnexpectedFindings
           y OpenQuestions describen los defectos y la correccion minima)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisComparison,
           minimo 25, RED esperado; suite Core completa en verde y solucion en Release sin advertencias nuevas en el GREEN; commit
           RED y push; commit GREEN y push con el resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); ante cualquiera, parar sin continuar
           y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions).
           Esta es la ULTIMA correccion posible de la unidad (Attempt 3, AttemptsRemaining 0): revisa cada paso antes de hacerlo.
           C-07 rige con el texto completo del contrato (la delegacion lo abrevio): «Hace falta cambiar una tolerancia del Freeze,
           convertir una fila informativa en criterio o al reves, o presentar la comparacion como validacion estructural»
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210642Z-ca39/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; delegacion de CORRECCION de la cadena F4-T4-COMPARATOR-AND-TOOL)
- Celda: la del paquete aceptado. ChainBaseSha = 34831380a2f2fd7a2cf5bdd8de05c3281f9efc50; BaseSha = c8f3aa884abbc86c1c0cf4ff112dadda0c26fa68; ChainRedSha = null y
  ChainRedFiles = [] (la cadena no acredito ningun RED): esta entrega EXIGE RED. CorrectionOf = R20261011T202738Z-6356 (StopCondition).
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210642Z-ca39/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F4-T4-COMPARATOR-AND-TOOL/3/R20261011T210642Z-ca39/work/.
  PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3, py, ni siquiera --version ni
  dentro de otra orden; en esta unidad ya hubo cuatro desviaciones por eso, la ultima en F4-T3: una sonda «python --version»
  anadida al final de una comprobacion del texto libre, que activo la parada C-08). NO compruebes la disponibilidad de ninguna
  herramienta (ninguna orden «<herramienta> --version» salvo git y dotnet) y revisa cada orden antes de lanzarla; usa pwsh o las
  herramientas de lectura; sin AutoCAD y sin RAM Elements. Escribe los archivos con las herramientas de escritura y edicion (no con
  heredocs de Bash).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Paso RED (un solo commit; correccion desactivada de AUTOMATION_PLAN 16.8):
  1. Pruebas de la cadena corregidas, SOLO esto:
     a. StructuralAnalysisComparisonToolTests.The_manifest_of_an_injection_is_marked_with_the_mutation_and_the_hash_of_the_source: el
        arreglo esperado de rutas cambiadas usa "$." + RamToolInjection.Marker en lugar del literal "$.faultInjection" y queda en orden
        ordinal (la prueba compara con la lista ordenada con StringComparer.Ordinal: ordena tambien el esperado).
     b. StructuralAnalysisComparisonToolTests.An_injected_package_is_a_new_folder_with_the_three_files_and_the_script_of_its_payload: el
        arreglo esperado de nombres queda en el orden ordinal real (manifest.json, rackcad-ram-build.py, structural-loads.json, usando
        las constantes de RamBuildPackage), o se ordena con StringComparer.Ordinal antes de comparar.
     c. CS0649 en StructuralAnalysisComparisonSupport.cs (campo SpringI sin asignar): asignalo donde corresponde o eliminalo si no se
        usa, sin cambiar lo que observa ninguna prueba.
     d. xUnit2013 en StructuralAnalysisComparisonToolTests.cs: usa la asercion que pide el analizador (Assert.Single o Assert.Empty)
        con la misma semantica.
     Nada mas cambia en las pruebas: ninguna otra asercion, ningun otro archivo de prueba.
  2. Implementacion DESACTIVADA: devuelve src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs y
     tools/RackCad.StructuralAnalysis.RamTool/ al esqueleto compilable de b51adf28 (git checkout b51adf28 -- <esas rutas> y git rm de
     los archivos que solo existen en 0e9bbc29). RackCad.sln y tests/RackCad.Tests/RackCad.Tests.csproj no cambian.
  3. Compila y corre el filtro: debe compilar sin errores y la seleccion focal (>= 25) debe fallar por excepcion o asercion (nunca por
     compilacion). Corre tambien las autopruebas de INV-14 y la de literales partidos.
  4. Commit RED (git add solo de esas rutas) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas (si hiciera falta: parada, sin editarlo).
- Paso GREEN: restaura la implementacion byte a byte desde 0e9bbc29 (git checkout 0e9bbc29 -- src/RackCad.Application/StructuralAnalysis/
  StructuralReadbackComparison.cs tools/RackCad.StructuralAnalysis.RamTool/), sin ningun otro cambio. Comprueba ANTES del commit:
  git diff 0e9bbc29 -- src tools RackCad.sln vacio y git diff <RED> -- tests vacio. Corrida GREEN del filtro (81/81 esperadas),
  suite Core completa en verde y dotnet build RackCad.sln -c Release: ninguna advertencia de los patrones CS, MSB, NU o NETSDK en
  archivos del diff, y CS0649 y xUnit2013 ya no aparecen (anota los totales y las advertencias preexistentes ajenas al diff). Si la
  implementacion restaurada no pasa las pruebas corregidas: parada, sin cambiar la implementacion. Commit GREEN con esta linea en el
  cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de la correccion de F4-T4-COMPARATOR-AND-TOOL (WorkRunId R20261011T210642Z-ca39) pendiente de la verificacion del Controller. attempts = 3."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas ni de nombres de prueba (si un nombre de prueba los
  contiene, nombralo por su archivo y numero de linea): GATE PASS, GATE_PASS, Candidato, Candidate, FINAL_CANDIDATE_SHA, gate cerrado,
  gate closed, cierre del gate, rama integrada, integrada en main, integrated into main, merged into main, lista para integrar,
  ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F4-T4-COMPARATOR-AND-TOOL; RunId R20261011T210642Z-ca39; DelegationRunId R20261011T210328Z-2ee6; Initiative I-65; Gate F4; Attempt 3; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) el diff exacto de las pruebas (archivo y lineas) y por que no debilita lo observado; (2) git diff 0e9bbc29 <GREEN> -- src tools
  RackCad.sln vacio; (3) los conteos del filtro en RED y GREEN y las pruebas que pasan ya en el RED y por que; (4) la salida de dotnet
  build RackCad.sln -c Release (errores y advertencias, separando las del diff); Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
