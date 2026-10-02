I-64 - DELEGACION F1-T1-MODEL - Worker, perfil DOCUMENTATION (RECUPERACION DE CONTROLES, intento 3, entrega final)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 3; RunId R20261002T181905Z-faf1;
           DelegationRunId R20261002T171954Z-754a; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha 0b7db52f12ddbf7ed36c8c2d020645818335c0fa;
           rama architecture/workspace-persistente-rackcad;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-workspace-persistente-rackcad;
           Owner subagent:R20261002T171954Z-754a
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-discovery.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-workspace-persistente-rackcad.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 18. Consensus Freeze (I64-CONSENSUS-V5-01) y cierre de F0 | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 19. F1-T0-ADR - Contrato recibido, preflight, relevo de salida y STOP antes de la planificacion | UNIT_DOC
           - docs/automation/state/I-64.yml | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-gate-contract-proposal.md | documento completo | UNIT_DOC
           - AGENTS.md | documento completo | EXTERNAL
           - docs/WORKFLOW.md | documento completo | EXTERNAL
           - docs/INITIATIVE_LIFECYCLE.md | documento completo | EXTERNAL
           - docs/AUTOMATION_PLAN.md | ## 16. Ejecucion delegada bajo orden del Coordinator | EXTERNAL
           - docs/automation/agent-execution/README.md | documento completo | EXTERNAL
           - docs/automation/agent-execution/routing.md | documento completo | EXTERNAL
           - docs/adr/0006-autocad-solo-en-plugin.md | documento completo | EXTERNAL
           - docs/adr/0019-shell-visual-de-editores-por-composicion.md | documento completo | EXTERNAL
           - docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md | documento completo | EXTERNAL
           - docs/adr/0044-hechos-neutrales-de-vistas-compartidas.md | documento completo | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.4 La transaccion la posee `Mutate(units)` (AR4-19) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.5 Capas bloqueadas (AR4-20) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.6 R-25: la primera colocacion sin supervivientes (AR4-21) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.7 Un solo barrido (AR4-42) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.8 Estados e informe (AR4-40) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.9 Guardas caller-owned (AR4-43) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.10 Cuenta completa de transacciones (AR4-44) | EXTERNAL
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
           Los valores de seccion son ASCII (contrato reemitido sin diacriticos); en Git los encabezados conservan sus tildes.
           Decision del Coordinator para esta recuperacion: docs/automation/evidence/I-64-evidence.md Sec.28 y Sec.29 (en el HEAD)
Objetivo (texto literal del contrato; vinetas en linea, sin lineas en blanco):
           Construir la primera fundacion pura de Workspace de I-64:
           * ADR 0047 en estado `propuesto`, fiel al Freeze; * modelo puro en RackCad.Application para sesion por documento; * identidad de instancia; * SelectionContext; * estado minimo de pistas/cola; * reglas puras necesarias para el drenaje; * politicas puras congeladas por F1 que no dependan de AutoCAD.
           La cadena debe demostrar RED->GREEN real.
           No implementar todavia:
           * PaletteSet; * WPF host; * AutoCAD event bridge real; * seleccion real; * navegacion; * escritura authored; * AutoCAD.
           ADR 0047:
           Ruta:
           docs/adr/0047-workspace-persistente-modeless-rackcad.md
           Estado:
           propuesto
           Debe ser una proyeccion del Anexo A del Freeze.
           No:
           * crea decisiones nuevas; * reinterpreta el Freeze; * adopta optional historicos; * se marca accepted; * se convierte en autoridad superior al Freeze.
           El Worker debe materializar el ADR antes de la implementacion GREEN.
           Puede estar en el commit inicial documental o en el RED, siempre que:
           * aparezca antes del GREEN; * permanezca estrictamente fiel al Freeze; * la cadena obtenga un RED tecnico real independiente del ADR.
           RED/GREEN:
           La primera cadena debe acreditar un RED real:
           * tests compilables; * seleccion >= 1; * fallo por asercion de comportamiento congelado aun no implementado; * no fallo artificial de compilacion; * no test inventado que falle deliberadamente sin expresar un invariante.
           Despues:
           GREEN para el mismo conjunto.
           Esta delegacion es la RECUPERACION DE CONTROLES (attempt 3); CorrectionOf R20261002T164250Z-f842 (StopCondition).
           El producto de la tarea ya existe en esta rama; esta entrega final solo aporta un CurrentSha nuevo y semanticamente neutro.
Alcance: permitido EXACTAMENTE docs/adr/0047-workspace-persistente-modeless-rackcad.md, src/RackCad.Application/Workspace/, tests/RackCad.Tests/Workspace/ (archivo exacto o prefijo terminado en /);
           prohibido docs/initiatives/, docs/automation/, src/RackCad.Plugin/, src/RackCad.UI/, src/RackCad.Domain/, tests/RackCad.UI.Tests/, .github/, assets/, eng/, deploy/, global.json, Directory.Build.props, Directory.Build.targets, RackCad.sln, src/RackCad.Application/RackCad.Application.csproj, tests/RackCad.Tests/RackCad.Tests.csproj, tools/RackCad.StructuralSections.Import/RackCad.StructuralSections.Import.csproj;
           y, por los criterios de aceptacion de esta entrega, el UNICO archivo que se modifica es src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs
           (todo lo demas, incluidos el ADR 0047, docs/adr/README.md y las pruebas, queda fuera)
Invariantes del Freeze (vinculantes en su totalidad, aunque los criterios sean menos exhaustivos):
           - INV-F1-T1-01: Cero referencias a AutoCAD desde el modelo puro de Application.
           - INV-F1-T1-02: Una sesion representa una instancia abierta de documento, no ruta/nombre/titulo.
           - INV-F1-T1-03: El modelo no persiste nada.
           - INV-F1-T1-04: El modelo no escribe authored ni modela ninguna escritura authored.
           - INV-F1-T1-05: SelectionContext soporta los estados exigidos por el Freeze para F1 sin inventar RackId.
           - INV-F1-T1-06: Las pistas son hints, nunca autoridad.
           - INV-F1-T1-07: El modelo de cola/drenaje no exige ausencia de eventos durante modales.
           - INV-F1-T1-08: No existe estado por pestana que duplique suscripciones o autoridad documental.
           - INV-F1-T1-09: La destruccion de una sesion no permite reemparejarla automaticamente con otro documento posterior.
           - INV-F1-T1-10: MASTER-I63-I64-02 permanece intacto: ninguna metrica, provider, ProjectSummary o agregacion entra en este modelo.
           - INV-F1-T1-11: ADR 0047 permanece propuesto y no anade semantica.
           - INV-F1-T1-12: RACKEDITAR y comandos existentes no cambian.
Rutas y archivos calientes: src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs (comentario XML de la clase, linea 6);
           guardas que deben quedar verdes SIN tocarlas: tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs (sus
           guardas de texto ignoran los comentarios //), tests/RackCad.Tests/NamespaceFolderGuardTests.cs y
           CantileverRoundTwoSourceGuardTests.
Criterios de aceptacion (del paquete aceptado):
           - git diff --name-only BaseSha..CurrentSha devuelve exactamente src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs.
           - El unico cambio es el comentario XML existente de la clase WorkspaceSessionRegistry, que aclara que es un registro puro y transitorio en memoria de sesiones vivas, una por instancia abierta de documento (D-03), y que no tiene autoridad de persistencia; no hay cambios ejecutables, de API, pruebas, ADR, Plugin, UI, Domain ni configuracion, ni marcadores de control.
           - Esta entrega no toca ChainRedFiles, por lo que no requiere RED; entrega un solo commit.
           - En el commit pasan el filtro del contrato y la suite Core completa; las builds Release no emiten avisos nuevos MSB, CS, NU o NETSDK.
           - La CI del CurrentSha exacto tiene los cuatro jobs requeridos en success.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.Workspace, minimo 1, RED esperado por el contrato: si
             (esta entrega no toca ChainRedFiles: AUTOMATION_PLAN 16.8 no exige RED; el RED de la cadena ya esta acreditado);
           evidencia esperada del contrato: gate/delegation package valido; nc4 segun I-61 en la primera delegacion aplicable; aceptacion A1-A8 antes de Worker; commit RED real; ChainRedSha / ChainRedFiles correctamente acreditados; commit GREEN; pruebas RED y GREEN con seleccion; worker-handoff valido; push fast-forward; CI exacta del CurrentSha verde; Controller verification valida; diff dentro de AllowedWriteScope; ADR 0047 fiel al Freeze.
           La sesion, no tu, mantiene evidencia y estado (docs/automation/ esta prohibido).
No-touch: todo lo prohibido arriba; el checkout principal D:\Documentos\Codex\Calculadora de racks (nunca escribas alli);
           AutoCAD: el acad.exe abierto en esta maquina pertenece a otra iniciativa: no lo cierres, no lo uses, no cargues DLL,
           no toques perfil, TRUSTEDPATHS, SECURELOAD, ACL ni registro; nada fuera del alcance permitido
Condiciones de parada: S-01..S-14, P-01..P-08 y C-01..C-12 del contrato:
           C-01: origin/main cambia antes de escribir
           C-02: ADR 0047 deja de estar libre
           C-03: se necesita tocar Plugin/UI
           C-04: se necesita AutoCAD
           C-05: se necesita ampliar scope
           C-06: aparece ambiguedad material del Freeze
           C-07: cambia config.toml
           C-08: hay conflicto de ownership
           C-09: la tarea deriva hacia Host/Bridge
           C-10: se intenta introducir metricas de I-63
           C-11: se intenta modificar comandos existentes
           C-12: presupuesto agotado
           C-02 se refiere a que OTRA unidad ocupe el numero 0047; el archivo existe en esta rama desde 36c344dc (esta cadena).
           S-01 y S-10 son frontera (Owner Validation, AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier
           otra, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F1-T1-MODEL/3/R20261002T181905Z-faf1/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL DOCUMENTATION
Metodo: documento breve que cita y no copia.
1. Cita las autoridades por ruta y seccion; no reproduzcas sus clausulas.
2. Separa hechos medidos de inferencias y marca lo desconocido como UNKNOWN.
3. Sigue el estilo del documento que editas (idioma, tablas, longitud de linea).

DELTA DE LA TAREA (recuperacion de controles, attempt 3, aceptada por el Coordinator)
- Celda: claude-sonnet-5-5|subagent, effort medium (Balanced); clase "Documentacion".
  ChainBaseSha = e0587355b0e84d98807057a56dfc50591da87489; ChainRedSha = a9778068af566bc2b84ba6a0005711b6452a2666;
  ChainRedFiles = tests/RackCad.Tests/Workspace/SelectionContextTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs.
  Esta entrega NO toca ChainRedFiles: NO exige RED (AUTOMATION_PLAN 16.8). Un solo commit.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) solo bajo artifacts/orchestration/I-64/F1-T1-MODEL/3/R20261002T181905Z-faf1/tests/<RunRef>/.
  No compiles src/RackCad.Plugin/ (no cambia; la CI lo compila). Edita con la herramienta de edicion, no con heredocs.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha. Si no, parada (S-12/S-13).
- Cambio UNICO permitido: en src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs, el comentario de documentacion XML existente de la clase
  (la linea `/// <summary>Pure registry of the live sessions: one per open document instance (D-03).</summary>`).
  Reescribelo para dejar explicito que es un registro puro y transitorio en memoria de las sesiones vivas de documento, una
  por instancia abierta de documento (D-03), y que no tiene autoridad de persistencia. Por ejemplo, de forma equivalente:
  `/// <summary>Pure, transient in-memory registry of the live document sessions: one per open document instance (D-03); it has no persistence authority.</summary>`
  XML bien formado, en ingles como el resto del archivo, en una sola linea. NADA MAS: ni codigo ejecutable, ni API, ni otros
  comentarios, ni pruebas, ni el ADR, ni otros archivos; sin marcadores de control ni texto ajeno al invariante congelado.
- Pasos:
  1. Edita la linea; comprueba que `git diff --name-only` = exactamente ese archivo y que `git diff` solo cambia esa linea.
  2. Commit (git add solo de ese archivo) con este cuerpo de resumen de estado, y git push (fast-forward, sin --force):
     "Estado: F1 en curso; entrega final de recuperacion de F1-T1-MODEL (WorkRunId R20261002T181905Z-faf1, intento 3): solo documentacion
     XML de WorkspaceSessionRegistry, sin cambio ejecutable; pendiente de la verificacion del Controller y de nc1..nc3.
     attempts = 3. Owner pendiente: aceptacion del ADR 0047 (propuesto) y Smoke-1 tras el cierre de F1."
  3. Sobre el commit (TreeSha = CurrentSha, arbol limpio): el filtro del contrato (todas en verde, seleccion > 0), la suite
     Core completa tests/RackCad.Tests/RackCad.Tests.csproj (0 fallidas) y dotnet build -c Release --no-incremental de
     tests/RackCad.Tests/RackCad.Tests.csproj y de tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj con 0 lineas que casen
     `\bwarning\s+(MSB|CS|NU|NETSDK)\d+\b` y 0 errores (registra comando, seleccion, resultado y conteos; Phase GREEN para el
     filtro, RELEVANT para la suite y las builds).
     Si algo falla por causa del cambio, no lo corrijas con otro cambio: parada y bloqueo estructurado.
  4. Escribe la entrega y termina. La CI del CurrentSha la espera la sesion, no tu.
- Git: sin --force, sin rebase, sin stash, sin amend, sin checkout de otros SHA, sin tocar otras ramas ni main; un solo commit.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que
  deje dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F1-T1-MODEL; RunId R20261002T181905Z-faf1; DelegationRunId R20261002T171954Z-754a; Initiative I-64; Gate F1; Attempt 3;
  BaseSha = el de arriba; RedSha = null (esta entrega no exige RED); CurrentSha = el SHA del commit (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command exacto, Phase GREEN/RELEVANT, TreeSha = commit probado, ResultsFile =
  TRX o null; FailedTests con los nombres reales); Worker = { Provider
  "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus y CurrentSha.
