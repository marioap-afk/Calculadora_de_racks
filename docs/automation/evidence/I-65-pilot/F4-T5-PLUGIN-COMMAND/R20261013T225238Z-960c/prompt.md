I-65 — DELEGACION F4-T5-PLUGIN-COMMAND (CORRECCION) — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T5-PLUGIN-COMMAND; intento 4; RunId R20261013T225238Z-960c;
           DelegationRunId R20261013T224954Z-16f8; AuthorityRevision 2151ef557a116cd45eb127d8205f902ae8d335a6;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha 2151ef557a116cd45eb127d8205f902ae8d335a6;
           rama architecture/interop-analisis-estructural;
           worktree C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural;
           Owner subagent:R20261013T224954Z-16f8
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§0, §1, §3, §6 y §11), docs/initiatives/I-65-f4-preparation.md (§5 y §6),
           docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes: D-08, D-12, D-13; §3 INV-01 e INV-12), docs/automation/decisions/I-65.md
           §26, §29, §37 y §41 (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia:
           AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/4/R20261013T224954Z-16f8/delegation.json (ruta relativa al worktree; leela con su
           ruta ABSOLUTA). Analisis de la correccion (se cita por CorrectionOf; no es autoridad):
           docs/automation/evidence/I-65-pilot/F4-T5-PLUGIN-COMMAND/R20261011T215838Z-09a6/analysis.md. En resumen: el comando de linea
           RACKRAMPAQUETE del Plugin (D-13) que exporta una estacion de un rack Cantilever al paquete de construccion, con su guarda.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Plugin/RackRamElementsCommands.cs (NUEVO),
           src/RackCad.Plugin/RackCad.Plugin.csproj (solo el elemento Content de la tabla), las ediciones exactas de
           tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs, tests/RackCad.Tests/SelectiveEditorOpenTests.cs,
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs y
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsGuardTests.cs, y archivos NUEVOS de
           tests/RackCad.Tests/StructuralAnalysis/. Prohibido el ForbiddenWriteScope del paquete (src/RackCad.Application/ entero;
           assets/; src/RackCad.Plugin/Workspace/; las demas pruebas existentes; Domain, UI, tools, deploy, docs/, etc.)
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §6
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Plugin/RackCommandSupport.cs (PickRackBlock(document, prompt, out RackEmbedDocument embed, out ObjectId
             blockId): false solo si se cancela; embed null si el bloque no es un rack); RackMenuCommands.cs (su uso en RACKEDITAR) y
             RackCantileverCommands.cs (kind con RackEmbedDocument.KindCantilever y StringComparison.OrdinalIgnoreCase; decodificacion
             con new RackProjectStore().Deserialize(embed.Design) en try/catch y su .CantileverLineDesign);
           src/RackCad.Plugin/StructuralSectionCatalogAccess.cs (TryLoad(editor, out StructuralSectionCatalog): la unica entrada al
             catalogo en el Plugin);
           src/RackCad.Application/Persistence/RackEmbedDocument.cs (Design es TEXTO JSON: la entrada de F4-T2 recibe sus bytes UTF-8 y
             un decodificador que los vuelve texto y llama a RackProjectStore.Deserialize);
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralExport.cs (entrada de F4-T2);
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (RamSectionMapReader; RamBuildPackageFactory con
             CreateRamBuildPackage y la carpeta embebida del paquete) y src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs
             (StructuralPackagePublisher); todo Application es SOLO LECTURA;
           src/RackCad.Plugin/RackCad.Plugin.csproj (el ItemGroup Content de catalogs es el precedente; el destino del paquete del
             autoloader no se toca);
           pruebas: tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs (arrays por iniciativa OpeningCommands, G7Commands y
             G15Commands; Baseline los concatena), tests/RackCad.Tests/SelectiveEditorOpenTests.cs (conteo BRUTO de la subcadena
             [CommandMethod( en todos los .cs del Plugin = 37, comentarios incluidos), las dos guardas de G1 de
             tests/RackCad.Tests/StructuralAnalysis/ y las guardas que barren el Plugin (NamespaceFolderGuardTests: namespace
             RackCad.Plugin en bloque; CantileverPluginSourceGuardTests; MainMenuStructuralSectionAccessGuardTests;
             SiblingRedrawSeamGuardTests; RackUnitsGuardSourceTests; I55G15Auth15ConsumerTests; CustomPropertiesEnvelopeGuardTests;
             LengthUnitsAuthorityGuardTests; PushBackRoundTripSourceGuardTests: nombres de comando unicos); todas solo lectura salvo
             las cuatro ediciones
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisPluginCommand,
           minimo 12, RED esperado; suite Core completa en verde, Plugin compilado localmente hacia artifacts/ y solucion en Release
           sin advertencias nuevas atribuibles al diff en el GREEN; commit RED y push; commit GREEN y push con el resumen de estado; worker-handoff.json valido
           en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:/Documentos/Codex/Calculadora de racks (nunca escribas ni leas alli; es el directorio de proceso de la sesion, no el tuyo)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-12); en particular C-03 (la entrada de
           F4-T2, el paquete o el escritor no bastan sin cambiarlos), C-07 (fila proposed tratada como verified o tabla en el paquete
           del autoloader), C-08 (Python por cualquier motivo o ejecutar AutoCAD) y C-11 (el Plugin no compila o hay advertencias
           evitables solo fuera del alcance); ante cualquiera, parar sin continuar y devolver el bloqueo estructurado en la entrega
           (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions). Esta es una CORRECCION (Attempt 4, AttemptsRemaining
           2): el intento anterior de esta tarea se detuvo por invocar un interprete Python (C-08) y por crear un archivo en el
           checkout principal (C-05). Revisa cada paso, cada orden, cada asercion y el texto libre antes de entregar
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/4/R20261013T225238Z-960c/worker-handoff.json (dentro del worktree);
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; delegacion de CORRECCION de la cadena F4-T5-PLUGIN-COMMAND)
- Celda: la del paquete aceptado. ChainBaseSha = 283ed6137a8669012369abb6e4a8543c27d34f1b; BaseSha = 2151ef557a116cd45eb127d8205f902ae8d335a6; ChainRedSha = null y
  ChainRedFiles = [] (la cadena no acredito ningun RED): esta entrega EXIGE RED. CorrectionOf = R20261011T215838Z-09a6 (StopCondition).
- CONTROLES DEL OWNER (decisiones §41, CD-I65-F4-32; condicion C-12). Tu directorio de proceso NO es el worktree: es el checkout
  principal del Owner (D:/Documentos/Codex/Calculadora de racks), zona prohibida. En esta unidad un Worker reescribio un archivo de
  ese checkout con una ruta relativa ([IO.File] tras Set-Location) y el intento anterior de ESTA tarea creo un archivo alli. Por eso:
  1. PRIMERA ACCION, antes de leer o escribir nada: informa pwd (Bash), y en PowerShell Get-Location y [Environment]::CurrentDirectory;
     anotalos tal cual en la entrega (Evidence) aunque no sean el worktree.
  2. Toda operacion sobre archivos con ruta ABSOLUTA bajo C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural
     (herramientas de lectura, escritura y edicion, y cualquier API .NET); git SIEMPRE con git -C C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural ...;
     dotnet con la ruta absoluta del proyecto o de la solucion; NO uses cd seguido de nombres relativos (en F4-T6 eso quedo como
     desviacion de procedimiento). NUNCA una ruta relativa en una API de archivos ni dependas de cd o Set-Location entre llamadas
     (cada llamada de Bash vuelve a empezar en el checkout principal). Si alguna vez escribes fuera del worktree, para (C-12) y declaralo.
  La sesion fotografia el checkout principal y los demas worktrees antes y despues de tu trabajo: cualquier cambio alli es una parada.
- Censos (CD-I65-F4-33): I-64 NO esta integrada en origin/main (bb0d5522e8411f66a51fdfb3f1f0d0514b737453); el conteo de SelectiveEditorOpenTests pasa de 37 a 38 y
  CustomPropertiesCommandGuardTests anade SOLO una lista propia de I-65 con RACKRAMPAQUETE; no anticipes RACKPANEL ni nada de I-64.
- Entorno: Windows. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/4/R20261013T225238Z-960c/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/4/R20261013T225238Z-960c/work/.
  NUNCA crees archivos de prueba, marcadores ni temporales fuera de esa carpeta del worktree (el intento anterior creo placeholder.txt
  en el checkout principal: C-05).
  PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3, py, ni siquiera --version ni
  dentro de otra orden; en esta unidad ya hubo cinco desviaciones por eso, la ultima en el intento anterior de ESTA tarea: una sonda
  «python3 --version» que activo la parada C-08). NO compruebes la disponibilidad de ninguna
  herramienta (ninguna orden «<herramienta> --version» salvo git y dotnet) y revisa cada orden antes de lanzarla; usa pwsh o las
  herramientas de lectura; NO ejecutes AutoCAD (solo compilas contra sus ensamblados) ni RAM Elements. Escribe los archivos con
  las herramientas de escritura y edicion (no con heredocs de Bash). La suite Core de la CI corre en ubuntu-latest: pruebas validas
  en Linux y en Windows; contenido generado con "\n" explicito.
- Compilacion del Plugin: SIEMPRE hacia una carpeta de salida bajo
  C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/4/R20261013T225238Z-960c/build/ (por ejemplo dotnet build
  C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -o <esa carpeta>), nunca hacia la salida normal (AutoCAD puede tener bloqueado el DLL de esa salida); anota errores y
  advertencias. Solucion: dotnet build RackCad.sln -c Release sin advertencias NUEVAS de los patrones CS, MSB, NU o NETSDK atribuibles
  al diff. En este equipo hay unas 63 advertencias locales PREEXISTENTES (MSB3277 de conflictos de referencias de AutoCAD y analizadores
  de pruebas existentes): listalas aparte y no son motivo de parada; la senal definitiva es el job Build Plugin without AutoCAD de la
  CI del GREEN, que compila la solucion en Release con -NoWarnings. C-11 solo aplica si el cambio introduce advertencias nuevas que
  solo se evitarian fuera del alcance.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura de I-65-f4-implementation.md §6 aplicada (no son decisiones nuevas; si algo con efecto observable no esta decidido ahi ni en
  el Freeze, no lo inventes: parada S-03 con la pregunta exacta):
  - Flujo: seleccion con RackCommandSupport.PickRackBlock (embed null o sin Design -> mensaje visible); kind cantilever sin distinguir
    mayusculas (otro -> Unsupported visible); indice de estacion, ruta del archivo de cargas y ruta de una carpeta NUEVA (el escritor
    rechaza una carpeta existente: nunca sobrescribe); catalogo con StructuralSectionCatalogAccess.TryLoad; tabla con
    RamSectionMapReader desde el directorio de Assembly.Location del Plugin + structural-analysis/ram-elements/section-material-map.json;
    entrada de F4-T2 con los bytes UTF-8 de embed.Design y un decodificador que los vuelve texto y devuelve
    new RackProjectStore().Deserialize(texto).CantileverLineDesign; CreateRamBuildPackage; publicacion con la carpeta embebida del
    paquete; mensajes con el resultado, los diagnosticos, la huella de entrada y los archivos escritos.
  - El archivo del comando contiene UNA sola vez la subcadena [CommandMethod( y el nombre RACKRAMPAQUETE solo en ese atributo (el
    conteo de SelectiveEditorOpenTests es bruto y cuenta comentarios). Sin ayuda nueva en RackCommandReference (UI intacta).
  - La guarda nueva lee src/RackCad.Plugin/RackRamElementsCommands.cs como TEXTO (la suite Core no compila el Plugin) y se autoprueba
    sobre variantes sinteticas antes de barrer el archivo real.
- Paso RED (un solo commit): la guarda nueva del comando y sus autopruebas sobre codigo sintetico (pruebas NUEVAS, namespace
  RackCad.Tests, clases StructuralAnalysisPluginCommand*, >= 12 seleccionables) que fallan por asercion o excepcion porque el archivo
  del comando aun no existe (nunca por compilacion: la guarda lee el archivo como texto), mas las cuatro ediciones exactas de pruebas
  existentes. Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si hiciera falta: parada, sin editarlo). Revisa bien tus
  aserciones y patrones ANTES del commit RED (comillas escapadas, barras invertidas, saltos de linea, la forma canonica del atributo):
  en F4-T1 una asercion defectuosa del RED obligo a una correccion.
- Paso GREEN: el archivo del comando y el elemento Content del .csproj; corrida GREEN del filtro (todas en verde), suite Core completa
  en verde, Plugin compilado hacia artifacts/ y solucion en Release sin advertencias nuevas atribuibles al diff (anota totales y las
  preexistentes aparte); comprueba el alcance del diff y que
  en tests/ solo hay altas bajo tests/RackCad.Tests/StructuralAnalysis/ y las cuatro ediciones; commit GREEN con esta linea en el
  cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-T5-PLUGIN-COMMAND (WorkRunId R20261013T225238Z-960c) pendiente de la verificacion del Controller. attempts = 4."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas ni de nombres de prueba (si un nombre de prueba los
  contiene, nombralo por su archivo y numero de linea): GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile usando rutas absolutas):
  TaskId F4-T5-PLUGIN-COMMAND; RunId R20261013T225238Z-960c; DelegationRunId R20261013T224954Z-16f8; Initiative I-65; Gate F4; Attempt 4; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (0) el directorio efectivo informado al empezar (pwd, Get-Location, [Environment]::CurrentDirectory) y la declaracion de que toda
  operacion de archivos uso rutas absolutas del worktree; (1) por invariante la prueba que lo observa; (2) la salida de la compilacion del Plugin hacia artifacts/ (errores y advertencias) y
  de dotnet build RackCad.sln -c Release; (3) el texto del elemento Content anadido; (4) las cuatro ediciones de pruebas existentes
  (archivo y lineas); (5) las pruebas que pasan ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested
  "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Itera con el filtro focal y corre la suite completa al final. Al terminar, tu respuesta final es una linea con
  WorkerStatus, RedSha y CurrentSha.
