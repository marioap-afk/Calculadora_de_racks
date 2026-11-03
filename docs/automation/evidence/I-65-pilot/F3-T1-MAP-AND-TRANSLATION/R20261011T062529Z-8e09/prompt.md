I-65 — DELEGACION F3-T1-MAP-AND-TRANSLATION — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F3; tarea F3-T1-MAP-AND-TRANSLATION; intento 0; RunId R20261011T062529Z-8e09;
           DelegationRunId R20261011T061951Z-7db1; AuthorityRevision bf2d347836904275955e1a8059051271a76c51e2;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha bf2d347836904275955e1a8059051271a76c51e2;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T061951Z-7db1
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f3-ram-elements-adapter.md (secciones 2 a 6 y 8: tabla, unidades, apoyos, ejes, cargas, golden de
           RAM), docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes: D-03, D-04, D-06, D-07, D-10, D-12; §3 INV-07 e INV-08),
           docs/automation/decisions/I-65.md §17 (CD-I65-F3-01..07), docs/automation/evidence/I-65-f0/p1-results.md y
           p1-run/p1-report.json (UNIT_DOC, en AuthorityRevision); ADR-0050 (UNIT_CHANGE); AGENTS.md, ADR-0005, ADR-0017 y
           docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T061951Z-7db1/delegation.json. En resumen: tabla de
           mapeo del proveedor (lector, validador, proyeccion), traduccion determinista del StructuralModel a RAM, cambios aditivos
           de A65-F1-01/02 y A65-F1-07, ediciones de guarda autorizadas y guardas nuevas del proveedor.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/StructuralAnalysis/Providers/RamElements/,
           src/RackCad.Application/StructuralAnalysis/StructuralMaterialProfile.cs (aditivo),
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs (solo los enunciados de supuestos),
           assets/structural-analysis/ram-elements/section-material-map.json (nuevo) y tests/RackCad.Tests/StructuralAnalysis/
           (archivos NUEVOS, mas las ediciones (a)-(c) de StructuralAnalysisSourceGuardTests.cs en el RED). Prohibido el
           ForbiddenWriteScope del paquete (las demas pruebas y fixtures existentes, Units/, StructuralSections/, otros sistemas,
           Domain, Plugin, UI, cualquier .csproj, assets/catalogs/, docs/, etc.). Si hiciera falta mas: parada (C-01)
Invariantes del Freeze: los del paquete aceptado (Invariants), con CD-I65-F3-03..07 y el documento de F3
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/StructuralAnalysis/ (StructuralModel, StructuralMaterialRow e IStructuralMaterialProfile en
             StructuralMaterialProfile.cs, StructuralFingerprints, StructuralModelBuilder, StructuralDiagnostic, ForceUnits en Units/);
           src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs (metodo Assumptions(), lineas ~464-473);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs (lineas ~30-110 y ~270-290, 360-460);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisCantileverFixtures.cs (helpers de REF-CANT-01, solo lectura);
           docs/automation/evidence/I-65-f0/p1-run/p1-report.json (steps.A1_orientation_coverage.observed: claves
             "<dir>-fwd|rev" con rot0, rot90 y rot12.3456 = [l1, l2, l3]) y p1-run/ram-probe-p1.executed.py lineas 202-217 (origen
             (3000, 0, 0) y extremos de las seis direcciones; fwd = origen -> extremo, rev = extremo -> origen)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElements,
           minimo 30, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); en particular C-01, C-02,
           C-03 (golden de RAM o tabla de P1 en desacuerdo con las reglas: parar con la diferencia exacta, sin ajustar nada),
           C-06, C-07 (rellenar valores de ingenieria, marcar verified una fila, inferir material o exponer un plan parcial),
           C-08 (Python o RAM Elements para compilar o probar), C-09 (debilitar una guarda mas alla de (a)-(c)) y C-10; ante
           cualquiera, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition
           STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T062529Z-8e09/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena F3-T1-MAP-AND-TRANSLATION)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = bf2d347836904275955e1a8059051271a76c51e2;
  ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T062529Z-8e09/tests/<RunRef>; anota en Command la orden
  COMPLETA. Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F3-T1-MAP-AND-TRANSLATION/0/R20261011T062529Z-8e09/work/.
  Sin AutoCAD, sin Python y sin RAM Elements. Escribe los archivos con la herramienta de escritura de archivos, no con heredocs.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a F3-T1 (no son decisiones nuevas; si el Freeze, CD-I65-F3-03..07 y el documento de F3 no deciden
  algo que necesitas decidir con efecto observable, no lo inventes: parada S-03 con la pregunta exacta; nombres de tipos libres):
  - Tabla (documento §2): JSON con exactamente "schema" = "rackcad-ram-elements-section-map/v1" y "rows". Cada fila con
    exactamente: sectionId, ramSection {name, table, country}, materialId, ramMaterial, expectedMaterial {elasticModulus,
    poissonRatio, shearModulus, unitWeight, source} (elasticModulus, shearModulus y unitWeight son null u objetos
    {value, unit}; poissonRatio null o numero), status ("proposed" o "verified"), verifiedBy y verifiedOn (null o texto;
    fecha yyyy-MM-dd). El archivo de datos lleva las dos filas proposed del documento (AISC-W-W12X26 -> "W 12X26" /
    ASTM-A992-GR50 / "A992 Gr50"; AISC-HSS-RECT-HSS4X4X_250 -> "HSS_SQR 4X4X1_4" / ASTM-A500-GRC / "A500 GrC rectangular"),
    con table, country, expectedMaterial (todos sus campos), verifiedBy y verifiedOn en null. El lector recibe TEXTO (el
    adaptador no hace E/S; las pruebas leen el archivo desde la raiz del repositorio). Las pruebas de una fila verified usan
    tablas de FIXTURE declaradas como fixture (p. ej. verificador "TEST-FIXTURE", fuente "TEST-FIXTURE-SOURCE").
  - Proyeccion (A65-F1-01/02): la tabla implementa IStructuralMaterialProfile; una fila proposed se proyecta sin verificador ni
    fecha (el constructor del modelo da Unsupported). Cambio ADITIVO en StructuralMaterialRow: un constructor u opcion que
    recibe el texto canonico de la fila del proveedor; CanonicalText pasa a prefijos de longitud (p. ej. "<n>:<valor>" por
    campo) e incluye ese texto cuando existe. Las pruebas de F1 deben seguir en verde sin tocarlas.
  - Traduccion: entrada = StructuralModel Valid + la tabla. El material del modelo debe coincidir con el materialId de la fila
    de su seccion (si no: Invalid, la tabla no es la que construyo el modelo). Orden determinista: nudos, miembros y cargas
    por id ordinal. Coordenadas, Restraints, ejes, theta y cargas segun el documento §4-§6; el golden de §8 se copia como
    literales. Para INV-07, la funcion pura de la regla se prueba con las 12 direcciones x 3 giros de P1 (incluidas
    vert_+Y y arm_Zup, que la regla trata como vertical y no vertical respectivamente); la admision de orientacion de V1
    (vertical o <= 10 grados de la horizontal) es una comprobacion aparte de la traduccion.
  - Supuestos (CD-I65-F3-06): sustituye EXACTAMENTE los siete enunciados por estos (sin otros cambios en la fuente):
    SUP-01 "COL.FOOT is fixed in its six degrees of freedom. Declared limitation: no anchor, base plate or slab stiffness is represented."
    SUP-02 "The base is not modelled; the idealisation starts at the column foot. Declared limitation: the real base bears on the floor."
    SUP-03 "Plane analysis: every node other than the foot restrains translation along X and rotation about Y and Z. Declared limitation: no out-of-plane buckling and no bracing effect are represented."
    CON-01 "The arm-to-column connection is continuous: no member end is released. Declared limitation: the real connection is semi-rigid."
    IDL-NO-OFFSETS "No rigid offsets: each analytical arm runs from the column axis to its tip and includes half the column and the plate. Declared limitation: the arm flexibility is overestimated, and no conclusion about real behaviour is drawn from V1."
    ASM-PROP-01 "Section and material properties used by the analysis are those of the analysis program database, selected by the section and material names of the verified mapping rows. Declared limitation: database editions and material values may differ from the usual ones; they are compared against the catalogue and against the expected values with their cited source."
    LOD-01 "A single load case REF1 carries the loads of the load file; there is no self weight and there are no combinations. Declared limitation: the loads are not derived from pallets, impact, seismic action or tributary areas."
  - Guarda (CD-I65-F3-05), solo en el RED: (a) la asercion «F1 no crea la carpeta Providers» pasa a afirmar que bajo
    src/RackCad.Application/StructuralAnalysis/Providers/ solo existe RamElements/; (b) INV-14 exime ademas exactamente
    tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs; (c) la lista G1 sigue vacia.
    Guardas nuevas en un archivo nuevo, con autoprueba: Providers/RamElements/ sin File, Directory, FileStream, StreamReader ni
    StreamWriter y sin referencias a RackCad.Application.Systems.
- Paso RED (un solo commit): esqueleto COMPILABLE (namespace RackCad.Application.StructuralAnalysis.Providers.RamElements, con
  bloque) con cuerpos que lanzan NotImplementedException o devuelven un valor que las pruebas rechazan; archivo de datos;
  ediciones (a)-(c); pruebas en archivos NUEVOS (namespace RackCad.Tests, clases StructuralAnalysisRamElements*, no
  StructuralAnalysisRamElementsPackage*), >= 30 seleccionables, que fallan por asercion o excepcion (nunca compilacion). Los
  cambios de StructuralMaterialProfile.cs y de los enunciados van en el GREEN (sus pruebas nuevas fallan en el RED). Commit RED
  (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas ni fixture del RED, ni la guarda (si hiciera falta: parada).
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota
  totales); comprueba el alcance del diff y que en tests/ solo hay altas mas la guarda modificada en el RED; commit GREEN con
  esta linea en el cuerpo, en UNA sola linea, y git push:
  "Estado: F3 en curso; entrega del Worker de F3-T1-MAP-AND-TRANSLATION (WorkRunId R20261011T062529Z-8e09) pendiente de la verificacion del Controller. attempts = 0."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F3-T1-MAP-AND-TRANSLATION; RunId R20261011T062529Z-8e09; DelegationRunId R20261011T061951Z-7db1; Initiative I-65;
  Gate F3; Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD =
  remoto); Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el
  arbol tenia cambios, ResultsFile = TRX); Evidence con: (1) por invariante, la prueba que lo observa; (2) la comparacion del
  golden de RAM y de la tabla de P1 con la diferencia maxima observada; (3) el diff exacto de la guarda; (4) las pruebas que
  pasan ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "high",
  Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada (una S-xx o C-xx de comportamiento STOP va con Disposition STOP); listas vacias si
  no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa al
  final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
