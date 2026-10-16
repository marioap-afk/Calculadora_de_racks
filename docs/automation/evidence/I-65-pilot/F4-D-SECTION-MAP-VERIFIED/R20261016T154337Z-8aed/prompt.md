I-65 — DELEGACION F4-D-SECTION-MAP-VERIFIED — Worker, perfil DEBUGGING
Identidad: unidad I-65; gate F4; tarea F4-D-SECTION-MAP-VERIFIED; intento 7; RunId R20261016T154337Z-8aed;
           DelegationRunId R20261016T153804Z-5fdd; AuthorityRevision e886b5577ad565ef4a55bb2e535b7d463f345b79;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha c487262fca9536bc769b6a6b704b82d556fce8fb;
           rama architecture/interop-analisis-estructural;
           worktree C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural;
           Owner subagent:R20261016T153804Z-5fdd
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-d-mapping-verified-contract.md (secciones 3, 4 y 5), docs/automation/decisions/I-65.md (§62, §63 y
           §64), docs/automation/evidence/I-65-f4/f4d-table-target.json (la tabla objetivo, literal) (UNIT_DOC); ADR-0050
           (UNIT_CHANGE); AGENTS.md y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para
           lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T153804Z-5fdd/delegation.json (ruta relativa al worktree; leela con
           su ruta ABSOLUTA). En resumen: CORRECCION de la entrega anterior (REWORK por Ci): tres pruebas pasan como carpeta del paquete
           una ruta que en Linux no es Windows y la fabrica la rechaza; se cambia SOLO esa carpeta por una ruta WINDOWS absoluta fija.
Alcance: el del paquete (= el del contrato); en esta correccion solo se tocan
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs,
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandProductTableTests.cs y, como correccion desactivada,
           assets/structural-analysis/ram-elements/section-material-map.json. Prohibido el ForbiddenWriteScope del paquete y cualquier
           otro cambio: nada en src/, tools/, Fixtures, otras pruebas, docs/ ni ningun .csproj.
Invariantes del Freeze: los del paquete aceptado (Invariants), con el contrato de F4-D seccion 4
Rutas y archivos calientes (solo lectura salvo lo indicado): RamBuildPackageFactory.cs (IsAbsoluteNormalisedFolder, solo lectura) y
           las dos pruebas a corregir
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysis,
           minimo 1789, RED esperado; suite Core completa en verde y solucion en Release sin advertencias nuevas en el GREEN; commit
           RED y push; commit GREEN y push con el resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:/Documentos/Codex/Calculadora de racks (nunca escribas ni leas alli; es el directorio de proceso de la sesion, no el tuyo)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10, C-12); ante cualquiera,
           parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y
           TriggeredStopConditions).
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T154337Z-8aed/worker-handoff.json (dentro del worktree);
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL DEBUGGING
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; delegacion de CORRECCION de la cadena F4-D-SECTION-MAP-VERIFIED)
- Cadena: ChainBaseSha = e886b5577ad565ef4a55bb2e535b7d463f345b79; ChainRedSha = fe9b6720b7b261a63b4ff7b4a246255dec8b7265 (RED
  acreditado); ChainRedFiles = las cinco pruebas de e886b557..fe9b6720 -- tests/. Esta correccion toca ChainRedFiles: EXIGE RED.
  CorrectionOf = R20261016T152242Z-2e6f (Ci); el analisis esta en
  docs/automation/evidence/I-65-pilot/F4-D-SECTION-MAP-VERIFIED/R20261016T152242Z-2e6f/analysis.md (leelo).
- Hechos: la entrega anterior (RED fe9b6720, GREEN d548fb0b) paso en Windows, pero la CI de Linux del GREEN fallo en tres pruebas con
  "Expected: Valid / Actual: Invalid" porque su carpeta de paquete era Path.Combine(Path.GetTempPath(), ...), que en Linux es /tmp/... y
  RamBuildPackageFactory.IsAbsoluteNormalisedFolder solo acepta una ruta WINDOWS absoluta (letra de unidad y «:\», o UNC; sin «/», sin
  segmentos vacios, «.» o «..», sin separador final). Esa regla es sintactica: la fabrica no escribe nada, asi que una cadena de ruta
  Windows absoluta es valida en Windows y en Linux. (La instruccion de usar Path.GetTempPath() venia del prompt anterior; era un error.)
- CONTROLES DEL OWNER (decisiones §41, CD-I65-F4-32; condicion C-12). Tu directorio de proceso NO es el worktree: es el checkout
  principal del Owner (D:/Documentos/Codex/Calculadora de racks), zona prohibida. Por eso:
  1. PRIMERA ACCION, antes de leer o escribir nada: informa pwd (Bash), y en PowerShell Get-Location y [Environment]::CurrentDirectory;
     anotalos tal cual en la entrega (Evidence) aunque no sean el worktree.
  2. Toda operacion sobre archivos con ruta ABSOLUTA bajo C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural
     (herramientas de lectura, escritura y edicion, cp y cualquier API .NET); git SIEMPRE con
     git -C C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural ...; dotnet con la ruta absoluta del
     proyecto o de la solucion. NUNCA una ruta relativa en una API de archivos ni dependas de cd o Set-Location (no empieces llamadas
     con cd; cada llamada de Bash vuelve a empezar en el checkout principal). Si alguna vez escribes fuera del worktree, para (C-12).
  3. NINGUN archivo temporal fuera de .../artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T154337Z-8aed/work/ y NINGUNA sonda
     compilada en tests/ (el Worker anterior escribio y borro un TmpProbe.cs en tests/: desviacion DEV-I65-F4-23; no la repitas).
     Edita los archivos C# SOLO con las herramientas de edicion (nada de heredocs de Bash).
  La sesion fotografia el checkout principal y los demas worktrees antes y despues de tu trabajo: cualquier cambio alli es una parada.
- Entorno: Windows. dotnet: usa SIEMPRE /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/7/R20261016T154337Z-8aed/tests/<RunRef>;
  anota en Command la orden COMPLETA. PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3,
  py, ni siquiera --version ni dentro de otra orden: parada C-08). NO compruebes la disponibilidad de ninguna herramienta (ninguna
  orden «<herramienta> --version» salvo git y dotnet); sin AutoCAD y sin RAM Elements. La suite Core de la CI corre en ubuntu-latest:
  las pruebas deben pasar en Linux y en Windows.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13. La tabla en BaseSha ya es la verified
  (blob d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2).
- LA CORRECCION (solo esto):
  - En StructuralAnalysisSectionMapVerifiedTests.cs, pruebas LaFabricaDaUnPaqueteValidoEnMemoriaConLaTablaDistribuidaSinEscribirLaCarpeta y
    CambiarUnCampoDeUnaFilaVerifiedCambiaLaHuellaDelPaquete, y en StructuralAnalysisPluginCommandProductTableTests.cs, prueba
    ConLaTablaDeProducto_LaFabricaDaUnPaqueteValidoEnMemoria_SinCrearLaCarpeta: la carpeta del paquete pasa a ser la cadena literal de
    ruta Windows absoluta @"C:\RackCad-F4D-NoExiste\paquete-estacion-0" (una constante privada por archivo, si quieres). Si una prueba
    comprobaba que la carpeta no existe antes o despues, conserva esa comprobacion con la nueva cadena (Directory.Exists). Elimina las
    variables, usings o ayudantes que queden sin uso por el cambio (solo eso).
  - Ninguna otra asercion, prueba ni archivo cambia. La tabla, el modelo, la huella y el lector siguen igual.
- Paso RED (un solo commit, correccion desactivada): las dos pruebas corregidas MAS la tabla devuelta a su contenido de e886b557, por
  ejemplo git -C <worktree> show e886b5577ad565ef4a55bb2e535b7d463f345b79:assets/structural-analysis/ram-elements/section-material-map.json
  redirigido con ruta ABSOLUTA a C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/assets/structural-analysis/ram-elements/section-material-map.json
  (comprueba que git -C <worktree> hash-object de la tabla da entonces el blob de e886b557:assets/...); con la tabla proposed, las
  pruebas que esperan la tabla verified fallan por asercion (nunca compilacion); seleccion del filtro >= 1789. Antes del commit RED
  comprueba tu correccion: con la tabla verified (la de BaseSha) corre las tres pruebas y el filtro del contrato y deben pasar.
  Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas (si hiciera falta: parada, sin editarlo).
- Paso GREEN (un solo commit, SOLO la tabla): copia de nuevo el archivo objetivo
  cp C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/docs/automation/evidence/I-65-f4/f4d-table-target.json C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/assets/structural-analysis/ram-elements/section-material-map.json
  y comprueba que git -C <worktree> hash-object de la tabla = d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2 = git -C <worktree> rev-parse
  e886b5577ad565ef4a55bb2e535b7d463f345b79:docs/automation/evidence/I-65-f4/f4d-table-target.json, y despues del commit que git -C <worktree> rev-parse
  HEAD:assets/structural-analysis/ram-elements/section-material-map.json es ese mismo blob. Corrida GREEN del filtro (todas en verde),
  suite Core completa en verde y
  dotnet build C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/RackCad.sln -c Release (anota
  totales; ninguna advertencia CS, MSB, NU o NETSDK en archivos del diff); git diff --name-only RED GREEN = solo la tabla;
  git diff --name-only BaseSha GREEN = solo las dos pruebas; commit GREEN con esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de la correccion de F4-D-SECTION-MAP-VERIFIED (WorkRunId R20261016T154337Z-8aed) pendiente de la verificacion del Controller. attempts = 7."
- Si con la tabla verified REF-CANT-01 no da Valid, la fabrica no da un paquete Valid o aparece un diagnostico distinto de los previstos:
  parada C-03 con la lista exacta de diagnosticos (codigo, sujeto, gravedad) y su causa; no amplies el alcance.
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas ni de nombres de prueba (si un nombre de prueba los
  contiene, nombralo por su archivo y numero de linea): GATE PASS, GATE_PASS, Candidato, Candidate, FINAL_CANDIDATE_SHA, gate cerrado,
  gate closed, cierre del gate, rama integrada, integrada en main, integrated into main, merged into main, lista para integrar,
  ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile usando rutas absolutas):
  TaskId F4-D-SECTION-MAP-VERIFIED; RunId R20261016T154337Z-8aed; DelegationRunId R20261016T153804Z-5fdd; Initiative I-65; Gate F4; Attempt 7; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) el directorio efectivo informado al empezar y la declaracion de rutas absolutas y de que no hubo archivos temporales fuera de work/;
  (2) por invariante la prueba que lo observa; (3) la comparacion de blobs de la tabla en RED (proposed) y GREEN (objetivo); (4) el diff
  de cada prueba corregida y por que no debilita lo observado; (5) la salida de dotnet build RackCad.sln -c Release (errores y
  advertencias); (6) las pruebas que pasan ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
