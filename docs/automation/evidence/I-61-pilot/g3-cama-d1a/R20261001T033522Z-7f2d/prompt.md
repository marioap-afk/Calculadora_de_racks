I-61 — DELEGACION g3-cama-d1a — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-61; gate G3; tarea g3-cama-d1a; intento 0; RunId R20261001T033522Z-7f2d;
           DelegationRunId R20261001T032333Z-4a2d; AuthorityRevision 7b8662c5190bb22c0b4aa7ea49ea31367f4bb359;
           MainSha 95690c28dc6268e61dff32a0cbc33cc9fde3d47f; BaseSha b677cb57eafcf557e1f0f5d276e973394c2e2256;
           rama architecture/protocolo-ejecucion-agentes;
           worktree D:\Documentos\Codex\Calculadora de racks\.claude\worktrees\architecture-protocolo-ejecucion-agentes;
           Owner subagent:R20261001T032333Z-4a2d
Autoridades (leer desde Git, no copiar): docs/AUTOMATION_PLAN.md preámbulo, §2 y §16 (UNIT_CHANGE) y §3 (EXTERNAL);
           docs/WORKFLOW.md §3 y §4 (EXTERNAL) y §10 (UNIT_CHANGE); AGENTS.md completo (EXTERNAL);
           docs/initiatives/PROMPT_TEMPLATES.md §G (UNIT_CHANGE); docs/automation/agent-execution/README.md, routing.md y
           schemas/delegation, worker-handoff y controller-verification (UNIT_CHANGE);
           docs/initiatives/I-61-proposal-v9.md, docs/initiatives/I-61-protocolo-ejecucion-agentes.md y
           docs/initiatives/I-61-discovery.md §19 (UNIT_DOC);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: corregir D-1a (Discovery de I-61 §19). EditCama debe resolver el nombre editado con la misma semantica que
           las otras cinco rutas de edicion: blanco o espacios -> nombre del sobre, incluido null; en otro caso, el editado.
           Se hace con una funcion pura en src/RackCad.Application/Persistence/, y EditCama usa el valor resuelto en el
           payload y en SyncName. RED compilable (esqueleto con la semantica actual), GREEN y caracterizacion de la ventana.
Alcance: permitido EXACTAMENTE src/RackCad.Application/Persistence/EditedRackNameResolver.cs (nuevo),
           src/RackCad.Plugin/RackCamaCommands.cs, tests/RackCad.Tests/I61EditedRackNameTests.cs (nuevo),
           tests/RackCad.Tests/I61CamaEditWiringGuardTests.cs (nuevo), tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs;
           prohibido docs/, .github/, assets/, AGENTS.md, CLAUDE.md, .gitignore, .gitattributes, src/RackCad.UI/,
           src/RackCad.Domain/ y RackSelectivoCommands.cs, RackDinamicoCommands.cs, RackPushBackCommands.cs,
           RackCantileverCommands.cs y RackCabeceraCommands.cs de src/RackCad.Plugin/
Invariantes del Freeze: INV-01, INV-02, INV-03, INV-05, INV-10, INV-P1, INV-P2, INV-P3
           (INV-P2: EditCama usa el valor resuelto por la funcion en el payload y en SyncName via LinkedBase, con el
           nombre editado como primer argumento y el del sobre como segundo; INV-P3: las otras cinco rutas no cambian)
Rutas y archivos calientes: src/RackCad.Plugin/RackCamaCommands.cs EditCama (llamada a BuildCamaPayload con
           window.RackName y RackBlockRenamer.SyncName(..., RackViewBaseName.LinkedBase(window.RackName)));
           semantica de referencia, solo lectura: RackSelectivoCommands.cs:127, RackDinamicoCommands.cs:183,
           RackPushBackCommands.cs:188, RackCantileverCommands.cs:242 y RackCabeceraCommands.cs:270
           (string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName);
           pruebas STA existentes en tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs (StaTestRunner.Run);
           raiz del repositorio para leer fuentes desde una prueba Core: la carpeta que contiene RackCad.sln
Criterios de aceptacion:
           - OBL-P1 (Core, funcion pura): editado blanco -> nombre del sobre; solo espacios -> nombre del sobre;
             blanco con sobre null -> null; nombre propio -> el editado.
           - OBL-P2 (guarda Core sobre el texto de EditCama): falla si (a) el argumento del payload no es el valor resuelto;
             (b) SyncName no recibe ese mismo valor via LinkedBase; (c) window.RackName sigue como argumento directo en
             alguno de los dos sitios; (d) los argumentos editado/sobre estan invertidos.
           - OBL-P3 (STA, caracterizacion): con el campo de nombre vacio, window.RackName es "" (la resolucion vive en
             EditCama); el nombre del metodo contiene I61_P3; pasa sobre el codigo actual.
           - Pruebas Core con System.Text.Json si hace falta JSON; sin paquetes nuevos; rutas con la capitalizacion exacta.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~I61EditedRackNameTests, minimo 4, RED esperado;
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~I61CamaEditWiringGuardTests, minimo 1, RED esperado;
           - tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, filtro FullyQualifiedName~FlowBedEditorWindowTests&Name~I61_P3,
             minimo 1, sin RED;
           commit RED y push; commit GREEN y push con el resumen de estado; corridas RED, GREEN y relevantes;
           tres mutaciones temporales de OBL-P2 registradas y revertidas; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli); nada fuera del alcance permitido
Condiciones de parada: S-01..S-14, P-03, P-05 y C-01 (hacer falta tocar cualquiera de las otras cinco rutas de
           edicion o cualquier archivo fuera del alcance permitido); S-01 y S-10 son frontera (Owner Validation,
           AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier otra, parar sin continuar y devolver
           el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-61/g3-cama-d1a/0/R20261001T033522Z-7f2d/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = b677cb57eafcf557e1f0f5d276e973394c2e2256;
  ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) solo bajo
  artifacts/orchestration/I-61/g3-cama-d1a/0/R20261001T033522Z-7f2d/tests/<RunRef>/ (INV-05).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/protocolo-ejecucion-agentes = BaseSha. Si no, parada S-12/S-13.
- Paso RED (un solo commit):
  1. EditedRackNameResolver en namespace RackCad.Application.Persistence, con un metodo estatico publico que recibe
     (nombre editado, nombre del sobre) en ese orden y devuelve string; en el RED es un ESQUELETO COMPILABLE con la
     semantica actual de Cama: devuelve el nombre editado tal cual.
  2. I61EditedRackNameTests con los cuatro casos de OBL-P1 (al menos 4 pruebas seleccionables).
  3. I61CamaEditWiringGuardTests (OBL-P2): lee src/RackCad.Plugin/RackCamaCommands.cs desde la raiz del repositorio,
     aisla el cuerpo de EditCama y comprueba (a)-(d). Puede incluir mutaciones EN MEMORIA del texto real para
     demostrar que cada condicion se detecta.
  4. Sin tocar RackCamaCommands.cs. Corre los dos filtros RED: deben compilar y FALLAR POR ASERCION con seleccion > 0
     (no por error de compilacion). Commit RED (git add solo de esos archivos) y git push.
- Desde el commit RED NO modifiques I61EditedRackNameTests.cs ni I61CamaEditWiringGuardTests.cs (son RT; si una
  necesitara cambio, parada y bloqueo estructurado, no lo edites).
- Paso GREEN:
  1. Implementa la funcion (blanco/espacios -> sobre, incluido null; si no, el editado). En EditCama calcula una
     variable con EditedRackNameResolver.<metodo>(window.RackName, embed.Name) y usala en BuildCamaPayload y en
     RackViewBaseName.LinkedBase(...) de SyncName; window.RackName ya no es argumento directo en esos dos sitios.
  2. Anade a FlowBedEditorWindowTests una prueba STA cuyo nombre contenga I61_P3: LoadExisting con un nombre, vacia
     el campo de nombre de la ventana (busca como lo hacen las pruebas existentes) y comprueba RackName == "".
  3. Corridas GREEN de los tres filtros (seleccion > 0, todas en verde). Relevantes: la clase completa
     FullyQualifiedName~FlowBedEditorWindowTests (UI) y la suite Core completa
     tests/RackCad.Tests/RackCad.Tests.csproj (anota totales). Compila src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug;
     si AutoCAD bloquea los DLL, NO cierres AutoCAD: anotalo en KnownLimitations.
  4. Mutaciones de OBL-P2 en el arbol, de una en una, con la guarda tras cada una y reversion inmediata:
     M1 solo el sitio del payload vuelve a window.RackName; M2 solo el sitio de SyncName vuelve a window.RackName;
     M3 argumentos invertidos (embed.Name, window.RackName). Cada una debe hacer FALLAR la guarda; registralas en
     TestsExecuted con Phase MUTATION y TreeSha null. Tras revertir, git diff de RackCamaCommands.cs = el del GREEN.
  5. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push:
     "Estado: G3 en curso; entrega del Worker de g3-cama-d1a (WorkRunId R20261001T033522Z-7f2d) pendiente de la
     verificacion del Controller. attempts = 0. Owner pendiente: ADR-0046, DEV-G1C-01 y Owner Validation."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId g3-cama-d1a; RunId R20261001T033522Z-7f2d; DelegationRunId R20261001T032333Z-4a2d; Initiative I-61; Gate G3;
  Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT/MUTATION, TreeSha = commit
  probado o null si el arbol tenia cambios, ResultsFile = TRX o null); Worker = { Provider "Anthropic",
  ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
