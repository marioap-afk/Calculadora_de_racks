I-65 — DELEGACION F4-D-SECTION-MAP-VERIFIED — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-D-SECTION-MAP-VERIFIED; intento 6; RunId R20261016T145459Z-8301;
           DelegationRunId R20261016T144346Z-4eaf; AuthorityRevision e886b5577ad565ef4a55bb2e535b7d463f345b79;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha e886b5577ad565ef4a55bb2e535b7d463f345b79;
           rama architecture/interop-analisis-estructural;
           worktree C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural;
           Owner subagent:R20261016T144346Z-4eaf
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-d-mapping-verified-contract.md (secciones 3, 4 y 5: alcance, D-INV-01..07, cadena),
           docs/automation/decisions/I-65.md (§62: D-09 y D-10 APPROVED, CD-I65-F4-102..108),
           docs/automation/decisions/I-65-owner-authorization-d09-d10-f4d.original.txt (orden literal del Owner),
           docs/automation/evidence/I-65-f4/f4d-table-target.json (la tabla objetivo, literal) (UNIT_DOC); ADR-0050 (UNIT_CHANGE);
           AGENTS.md y docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T144346Z-4eaf/delegation.json (ruta relativa al worktree; leela con
           su ruta ABSOLUTA). En resumen: la tabla del producto pasa de dos filas proposed a dos filas verified con los valores
           aprobados por el Owner, que estan literalmente en el archivo objetivo; RED con la prueba nueva y las ediciones nombradas,
           GREEN solo con la tabla.
Alcance: el del paquete (= el del contrato): assets/structural-analysis/ram-elements/section-material-map.json,
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs (NUEVO) y las ediciones nombradas de
           StructuralAnalysisRamElementsMapReaderTests.cs, StructuralAnalysisRamElementsPackageValidationTests.cs,
           StructuralAnalysisRamElementsTranslationTests.cs y StructuralAnalysisPluginCommandProductTableTests.cs. Prohibido el
           ForbiddenWriteScope del paquete y cualquier otro cambio: nada en src/, tools/, Fixtures (tampoco
           StructuralAnalysisRamElementsFixtures.cs ni StructuralAnalysisCantileverFixtures.cs), otras pruebas, docs/ ni ningun .csproj.
Invariantes del Freeze: los del paquete aceptado (Invariants), con el contrato de F4-D seccion 4
Rutas y archivos calientes (solo lectura salvo lo indicado): RamSectionMapReader.cs, RamSectionMapRow.cs, RamBuildPackageFactory.cs,
           RamBuildPackageOptions, CantileverStructuralExport, StructuralAnalysisRamElementsFixtures.cs (ayudantes F.Row, F.Document,
           F.Table, F.RefTable, F.BuildRef, F.DataFileText), StructuralAnalysisCantileverFixtures.cs (C.RackId, C.DesignBytes,
           C.LoadsBytes, C.Catalog, C.RepoRoot) y las pruebas existentes de la tabla
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
           artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T145459Z-8301/worker-handoff.json (dentro del worktree);
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F4-D-SECTION-MAP-VERIFIED)
- Celda: la del paquete aceptado. ChainBaseSha = BaseSha = e886b5577ad565ef4a55bb2e535b7d463f345b79; ChainRedSha = null y ChainRedFiles = []: esta entrega
  EXIGE RED.
- CONTROLES DEL OWNER (decisiones §41, CD-I65-F4-32; condicion C-12). Tu directorio de proceso NO es el worktree: es el checkout
  principal del Owner (D:/Documentos/Codex/Calculadora de racks), zona prohibida. En esta unidad un Worker anterior reescribio un
  archivo de ese checkout con una ruta relativa ([IO.File] tras Set-Location). Por eso:
  1. PRIMERA ACCION, antes de leer o escribir nada: informa pwd (Bash), y en PowerShell Get-Location y [Environment]::CurrentDirectory;
     anotalos tal cual en la entrega (Evidence) aunque no sean el worktree.
  2. Toda operacion sobre archivos con ruta ABSOLUTA bajo C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural
     (herramientas de lectura, escritura y edicion, cp y cualquier API .NET); git SIEMPRE con
     git -C C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural ...; dotnet con la ruta absoluta del
     proyecto o de la solucion. NUNCA una ruta relativa en una API de archivos ni dependas de cd o Set-Location (ni siquiera dentro de
     la misma llamada: no empieces llamadas con cd; cada llamada de Bash vuelve a empezar en el checkout principal). Si alguna vez
     escribes fuera del worktree, para (C-12) y declaralo.
  La sesion fotografia el checkout principal y los demas worktrees antes y despues de tu trabajo: cualquier cambio alli es una parada.
- Entorno: Windows. dotnet: usa SIEMPRE /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T145459Z-8301/tests/<RunRef>;
  anota en Command la orden COMPLETA. Scripts auxiliares, si los necesitas, solo en
  .../artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T145459Z-8301/work/ (ruta absoluta).
  PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3, py, ni siquiera --version ni
  dentro de otra orden; en esta unidad ya hubo cinco desviaciones por eso, siempre sondas «--version» o «print»: activan la parada
  C-08). NO compruebes la disponibilidad de ninguna herramienta (ninguna orden «<herramienta> --version» salvo git y dotnet) y revisa
  cada orden antes de lanzarla; usa pwsh o las herramientas de lectura; sin AutoCAD y sin RAM Elements. Escribe los archivos C# con
  las herramientas de escritura y edicion (no con heredocs de Bash). La suite Core de la CI corre en ubuntu-latest: pruebas validas en
  Linux y en Windows; rutas temporales con Path.GetTempPath(); fixtures y tabla leidos con rutas relativas al repositorio como hacen las
  pruebas existentes (C.RepoRoot(), F.DataFileText()); nunca rutas absolutas de esta maquina dentro del codigo; una carpeta de paquete
  de prueba debe ser absoluta en los dos sistemas: Path.Combine(Path.GetTempPath(), ...) (una ruta «C:\...» no es absoluta en Linux).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- LA TABLA (D-INV-01, D-INV-02): el GREEN escribe la tabla copiando el archivo objetivo byte a byte, por ejemplo
  cp C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/docs/automation/evidence/I-65-f4/f4d-table-target.json C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/assets/structural-analysis/ram-elements/section-material-map.json
  y comprueba ANTES del commit que git -C <worktree> hash-object assets/structural-analysis/ram-elements/section-material-map.json es
  igual a git -C <worktree> rev-parse e886b5577ad565ef4a55bb2e535b7d463f345b79:docs/automation/evidence/I-65-f4/f4d-table-target.json, y DESPUES del commit que
  git -C <worktree> rev-parse HEAD:assets/structural-analysis/ram-elements/section-material-map.json es ese mismo blob. Nunca edites
  la tabla a mano ni la reformatees. El archivo objetivo es UTF-8 sin BOM con LF; verifiedBy contiene U+00E9 («é») y U+2014 («—»).
- PRUEBA NUEVA tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSectionMapVerifiedTests.cs (namespace RackCad.Tests; clase
  StructuralAnalysisSectionMapVerifiedTests; ayudantes privados en el mismo archivo; lee la tabla distribuida con F.DataFileText() o
  como StructuralAnalysisPluginCommandProductTableTests). Debe observar, como minimo:
  1. Lector: RamSectionMapReader.Read de la tabla distribuida -> Valid, sin diagnosticos, exactamente las dos filas
     (AISC-HSS-RECT-HSS4X4X_250 y AISC-W-W12X26), ambas Status Verified e IsVerified.
  2. Literales de cada fila (D-INV-02), leidos del archivo objetivo y escritos en C# de modo que el valor decodificado sea el mismo:
     ramSection name/table/country (W 12X26 / W / United States; HSS_SQR 4X4X1_4 / HSS_SQR / United States); materialId y ramMaterial
     (ASTM-A992-GR50 / A992 Gr50; ASTM-A500-GRC / A500 GrC rectangular); elasticModulus 29000 ksi; poissonRatio 0.3; shearModulus
     null; unitWeight 490 pcf (nunca 486.94); el texto source COMPLETO de cada fila (copialo del archivo objetivo sin cambiar un
     caracter); verifiedBy exactamente «Mario Pérez — Owner / Engineering Validator» (43 caracteres) y verifiedOn «2026-10-16».
  3. REF-CANT-01 (D-INV-04): CantileverStructuralExport.Build(C.RackId, "Cantilever", 0, C.DesignBytes(), <decodificador como en
     StructuralAnalysisPluginCommandProductTableTests>, C.LoadsBytes(), tabla distribuida, C.Catalog) -> Outcome Valid, Model no
     null, ningun diagnostico bloqueante y el MISMO conjunto de codigos de diagnostico que da la misma construccion con la tabla
     verified de fixture F.RefTable() (esa es la definicion de «diagnosticos previstos»; si difiere, parada C-03 con la lista).
  4. Paquete en memoria: RamBuildPackageFactory.CreateRamBuildPackage(modelo, new RamBuildPackageOptions(tabla distribuida, "1.0.0",
     Path.Combine(Path.GetTempPath(), "<nombre unico>"), C.LoadsBytes())) -> Outcome Valid y Package no null; la carpeta no existe
     antes ni despues (la fabrica no escribe; ninguna prueba publica).
  5. Huella (INV-04): leer una copia en memoria del texto de la tabla con verifiedOn u otro campo de una fila verificada cambiado
     (sigue Valid) y comprobar que AnalysisInputFingerprint del paquete cambia respecto del de la tabla distribuida.
  Usa nombres de prueba en espanol, como las demas pruebas de la carpeta.
- EDICIONES NOMBRADAS (solo estas, y solo para el cambio proposed -> verified; puedes renombrar la prueba editada para que diga
  verified; ninguna otra asercion existente se debilita):
  - StructuralAnalysisRamElementsMapReaderTests: ElArchivoDeDatosEsValidoYTieneExactamenteLasDosFilasProposedDeRefCant01 -> las dos
    filas de REF-CANT-01 Verified e IsVerified; ElArchivoDeDatosNoTrajoValoresDeIngenieriaNiVerificacion -> la tabla trae los valores
    de ingenieria y la verificacion aprobados (table, country, E, nu, G null, peso, source no vacio, verifiedBy y verifiedOn);
    ConLaTablaTalComoSeEntregaTodaExportacionEsUnsupportedYNoSeAsignaNingunMaterial -> con la tabla tal como se entrega, F.BuildRef da
    Valid, con modelo y sin MaterialRowUnverified. LasDosFilasDelArchivoLlevanLosNombresQueRamAceptoEnP1 NO se toca.
  - StructuralAnalysisRamElementsPackageValidationTests: ConLaTablaDelProductoTalComoSeEntregaElResultadoEsUnsupportedYSinContenido ->
    con la tabla del producto, el paquete del modelo valido de fixture es Valid (con contenido en memoria);
    LaPrecedenciaEsLaDeCdI65F104InvalidGanaAUnsupported -> conserva TODA su cobertura (Invalid gana a Unsupported, FolderInvalid y
    SectionRowUnverified presentes, un diagnostico con ImpliedOutcome Unsupported) usando una tabla proposed de fixture construida en
    ese mismo archivo (ya existe el ayudante privado ProposedArmRow(); por ejemplo F.Table(F.Document(F.ColumnRow(), ProposedArmRow())))
    en lugar de la tabla distribuida.
  - StructuralAnalysisRamElementsTranslationTests: UnResultadoQueNoEsValidoSePropagaConSuResultadoYSinPlan -> conserva su cobertura
    (Unsupported con MaterialRowUnverified propagado y sin plan) usando una tabla proposed de fixture construida en ese mismo archivo
    (un ayudante privado nuevo en ese archivo; no toques los Fixtures) en lugar de la tabla distribuida.
  - StructuralAnalysisPluginCommandProductTableTests (clase): la tabla de producto se lee y todas sus filas estan Verified; con ella la
    estacion es Valid con modelo; la fabrica da un paquete Valid en memoria con una carpeta absoluta bajo Path.GetTempPath() que no se
    crea; UnKindDistintoDeCantilever_NoEsElDelSobreCantilever se conserva sin cambios; actualiza el comentario de la clase.
- INV-14 y guardas: ninguna prueba nueva o editada escribe el nombre del modulo del proveedor ni del interprete (ni partido en
  literales); si hiciera falta, StructuralAnalysisRamElementsHardeningScans.ToolName(). No edites ni eximas ninguna guarda. Los textos
  source no contienen esos nombres: copialos tal cual.
- Paso RED (un solo commit): la prueba nueva y las ediciones nombradas, compilables, que esperan la tabla verified; con la tabla
  proposed actual fallan por asercion o excepcion (nunca compilacion); seleccion del filtro >= 1789 (las pruebas que pasan ya en el
  RED, como las que usan tablas proposed de fixture, se declaran en Evidence). Commit RED (git add solo de archivos del alcance) y
  git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas (si hiciera falta: parada, sin editarlo). Revisa bien tus aserciones
  ANTES del commit RED (literales largos copiados del archivo objetivo, caracteres no ASCII, comillas, conteos, nombres de filas):
  en esta unidad aserciones defectuosas del RED obligaron a correcciones (F4-T1 y F4-T4). Puedes comprobarlas antes del commit RED
  copiando TEMPORALMENTE el archivo objetivo sobre la tabla, corriendo el filtro y restaurando la tabla con
  git -C <worktree> checkout -- assets/structural-analysis/ram-elements/section-material-map.json (la tabla del RED debe ser la de
  BaseSha: git -C <worktree> diff --name-only BaseSha RED no debe incluirla).
- Paso GREEN (un solo commit, SOLO la tabla): copia el archivo objetivo como arriba; corrida GREEN del filtro (todas en verde),
  suite Core completa en verde y
  dotnet build C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural/RackCad.sln -c Release (anota
  totales; ninguna advertencia CS, MSB, NU o NETSDK en archivos del diff); git diff --name-only RED GREEN = solo la tabla; commit GREEN
  con esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-D-SECTION-MAP-VERIFIED (WorkRunId R20261016T145459Z-8301) pendiente de la verificacion del Controller. attempts = 6."
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
  TaskId F4-D-SECTION-MAP-VERIFIED; RunId R20261016T145459Z-8301; DelegationRunId R20261016T144346Z-4eaf; Initiative I-65; Gate F4; Attempt 6; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) el directorio efectivo informado al empezar (pwd, Get-Location, [Environment]::CurrentDirectory) y la declaracion de que toda
  operacion de archivos uso rutas absolutas del worktree; (2) por invariante la prueba que lo observa; (3) la comparacion de blobs de
  la tabla (hash-object, rev-parse del objetivo y de HEAD); (4) lo observado con la tabla verified: Outcome del lector y estado de las
  filas; Outcome del modelo de REF-CANT-01 con la lista completa de sus diagnosticos (codigo, sujeto, gravedad) y la de la tabla de
  fixture; Outcome del paquete en memoria y las dos huellas; (5) la salida de dotnet build RackCad.sln -c Release (errores y
  advertencias); (6) las pruebas que pasan ya en el RED y por que; (7) el diff de cada edicion nombrada y por que no debilita lo
  observado; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "medium", Trailer
  "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED;
  Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Itera con el filtro focal (por ejemplo FullyQualifiedName~RackCad.Tests.StructuralAnalysisSectionMapVerifiedTests)
  y corre el filtro del contrato y la suite completa al final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y
  CurrentSha.
