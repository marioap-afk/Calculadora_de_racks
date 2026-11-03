I-65 — DELEGACION F3-T2-PACKAGE-AND-WRITER — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F3; tarea F3-T2-PACKAGE-AND-WRITER; intento 0; RunId R20261011T073033Z-b3ec;
           DelegationRunId R20261011T072637Z-c92e; AuthorityRevision e0c1120bcda13f90b7c4c8ad861ddd702c1ac891;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha e0c1120bcda13f90b7c4c8ad861ddd702c1ac891;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T072637Z-c92e
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f3-ram-elements-adapter.md (secciones 1, 3, 6, 7 y 8), docs/initiatives/I-65-proposal-v3.1.md
           (Frozen: yes: D-01, D-08, D-09 reglas 1-10, D-10, D-12, D-13; §3 INV-04, 05, 10, 11 y 14),
           docs/automation/decisions/I-65.md §17-§19 (CD-I65-F3-01..11), docs/automation/evidence/I-65-f0/p1-results.md,
           p1-run/p1-report.json y p1-run/ram-probe-p1.executed.py (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md, ADR-0017 y
           docs/AUTOMATION_PLAN.md §16 (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F3-T2-PACKAGE-AND-WRITER/0/R20261011T072637Z-c92e/delegation.json. En resumen: carga util,
           plantilla Python, manifiesto y CreateRamBuildPackage en el proveedor (devuelve contenido) y el escritor atomico neutral.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/StructuralAnalysis/Providers/RamElements/
           (archivos nuevos; cambios aditivos en los de F3-T1), src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs
           (nuevo) y archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/. Prohibido el ForbiddenWriteScope del paquete
           (todas las pruebas y fixtures existentes, incluidas las de F3-T1 y la guarda; Systems/; assets/; Units/;
           StructuralSections/; otros archivos de StructuralAnalysis; Domain, Plugin, UI, cualquier .csproj, docs/, etc.)
Invariantes del Freeze: los del paquete aceptado (Invariants), con CD-I65-F3-08, 09 y 11 y el documento de F3
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (F3-T1: RamSectionMaterialTable y su lector,
             RamElementsTranslator, RamBuildPlan, RamLocalAxes, RamElementsDiagnosticCodes);
           src/RackCad.Application/StructuralAnalysis/ (StructuralModel con Metadata y hash del archivo de cargas,
             StructuralFingerprints.AnalysisInputFingerprint, StructuralModelCanonicalizer, StructuralExportOutcome, StructuralDiagnostic);
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisCantileverFixtures.cs y StructuralAnalysisRamElementsFixtures.cs
             (helpers de REF-CANT-01 y de tablas de fixture; solo lectura, se pueden usar);
           docs/automation/evidence/I-65-f0/p1-run/p1-report.json (E1_environment.core_functions: las 207 funciones del nucleo);
           fuente del paquete instalado, solo lectura: C:\Program Files\Bentley\Engineering\RAM Elements\python\ramelements
             (entities/node.py Restraints, entities/member.py, loads/load_condition.py, enums.py)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElementsPackage,
           minimo 30, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-11); en particular C-03 (carga
           util de REF-CANT-01 distinta del golden de RAM: parar con la diferencia exacta), C-07 (tratar una fila proposed como
           verified o producir contenido parcial), C-08 (ejecutar Python o RAM Elements) y C-11 (llamada o atributo fuera del
           documento §7, funcion del nucleo fuera de P1, o escritura antes de las puertas); ante cualquiera, parar sin continuar y
           devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F3-T2-PACKAGE-AND-WRITER/0/R20261011T073033Z-b3ec/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F3-T2-PACKAGE-AND-WRITER)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = e0c1120bcda13f90b7c4c8ad861ddd702c1ac891;
  ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F3-T2-PACKAGE-AND-WRITER/0/R20261011T073033Z-b3ec/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F3-T2-PACKAGE-AND-WRITER/0/R20261011T073033Z-b3ec/work/.
  Sin AutoCAD, sin Python y sin RAM Elements (no ejecutes la plantilla). Escribe los archivos con la herramienta de escritura.
  La suite Core de la CI corre en ubuntu-latest: las pruebas deben pasar en Linux y en Windows (rutas temporales con
  Path.GetTempPath(), sin depender del separador del sistema; contenido generado con "\n" explicito, nunca Environment.NewLine).
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a F3-T2 (no son decisiones nuevas; si el Freeze, CD-I65-F3-08..11 y el documento no deciden algo
  que necesitas decidir con efecto observable, no lo inventes: parada S-03 con la pregunta exacta; nombres de tipos libres):
  - Ruta de la carpeta del paquete: es una ruta ABSOLUTA DE WINDOWS (el script corre dentro de RAM en Windows): unidad y «:\» o
    UNC «\\servidor\recurso\...», sin «/», sin segmentos «.» ni «..», sin separador final ni separadores dobles (salvo el
    prefijo UNC); se valida con esas reglas explicitas (no con Path.IsPathRooted, que depende del sistema). Las pruebas de
    contenido usan una ruta fija de ese tipo y no escriben en disco; las del escritor usan una carpeta temporal real.
  - Carga util: JSON compacto, determinista y solo ASCII (los no ASCII como \uXXXX). Se incrusta en el script como literal de
    cadena Python entre comillas dobles, escapando «\» como «\\» y «"» como «\"»; json.loads la decodifica. La prueba de
    equivalencia extrae el literal del script, deshace ese escape y compara el JSON con el golden de RAM (§8 del documento).
  - Script: solo ASCII; el estado de fallo es exactamente FAILED — DISCARD MODEL, escrito en el codigo fuente con la secuencia
    de escape \u2014 de Python. Orden: imports; carga util; puerta de capacidad (ramelements.__version__ == "0.1.0" y presencia
    de las funciones del nucleo listadas en la carga util, leidas como hizo la sonda P1 con get_core); modelo m = model() y
    puerta de modelo vacio (len(m.nodes) == 0, len(m.members) == 0, como maximo un caso y ninguno con id REF1); apertura de
    <carpeta>\ram-readback.json en modo "x"; solo entonces units.set(Units.LB_IN) y las escrituras (nudos con Restraints y
    description = id neutral; miembros con section, material SIEMPRE explicito, rotation y description; cardinal_point =
    CENTROID; caso REF1 con categoria "OTHER" como texto; add_point_load con LoadDirection.GLOBAL_Y y porcentaje); lectura de
    vuelta y comparacion: coordenadas y valores |d| <= 1e-9 * max(1, |v|), porcentajes y componentes de ejes |d| <= 1e-9,
    seccion, material, Restraints y liberaciones exactos; valores por defecto medidos en P1 (int(cardinal_point) == 0,
    rigid_end_offset_i/j == (0, 0, 0), get_stiffness_factors() == (1, 1, 1, 1), int(axial_behavior) == 0,
    rotational_stiffness_i/j == 0, los diez campos de end_releases en False, self_weight del caso == (0, 0, 0)); cualquier
    excepcion o diferencia -> estado FAILED — DISCARD MODEL; al final escribe la lectura de vuelta (estado, AnalysisInputFingerprint,
    diferencias y valores leidos) en el archivo ya abierto y la imprime en consola. Sin save, save_as, new, analyze, os, sys ni
    setters de offsets, rigideces, axial, resortes o liberaciones.
  - Manifiesto: JSON determinista con lo que pide el invariante; advertencia literal de CD-I65-F3-09 y exclusiones de V1.
  - CreateRamBuildPackage: devuelve los tres archivos (nombre y bytes) o diagnosticos; con la tabla del producto (filas proposed)
    da Unsupported. Las pruebas de contenido usan la tabla de fixture verified de F3-T1 (declarada como fixture).
  - Escritor: costura interna (p. ej. una interfaz de sistema de archivos) para inyectar fallos en cada escritura y en el
    renombrado; sin tipos ni nombres de proveedor.
  - Pruebas de la plantilla: SOLO en tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs
    (unico archivo de pruebas que puede nombrar el modulo del proveedor o el interprete); guarda estatica por expresiones
    regulares sobre la plantilla y sobre el script generado, con autoprueba sobre variantes sinteticas (sin puerta, con
    save_as, con analyze, con otro open, con la categoria como enum, sin material) que deben fallar.
- Paso RED (un solo commit): esqueleto COMPILABLE (namespaces = carpeta, con bloque) con cuerpos que lanzan
  NotImplementedException o devuelven un valor que las pruebas rechazan; pruebas en archivos NUEVOS (namespace RackCad.Tests,
  clases StructuralAnalysisRamElementsPackage*), >= 30 seleccionables, que fallan por asercion o excepcion (nunca compilacion).
  Commit RED (git add solo de archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si hiciera falta: parada, sin editarlo).
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota
  totales); comprueba el alcance del diff y que en tests/ solo hay altas; commit GREEN con esta linea en el cuerpo, en UNA sola
  linea, y git push:
  "Estado: F3 en curso; entrega del Worker de F3-T2-PACKAGE-AND-WRITER (WorkRunId R20261011T073033Z-b3ec) pendiente de la verificacion del Controller. attempts = 0."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F3-T2-PACKAGE-AND-WRITER; RunId R20261011T073033Z-b3ec; DelegationRunId R20261011T072637Z-c92e; Initiative I-65;
  Gate F3; Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD =
  remoto); Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y
  TestResults de cada corrida (RunRef unico, Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el
  arbol tenia cambios, ResultsFile = TRX); Evidence con: (1) por invariante, la prueba que lo observa; (2) la comparacion de la
  carga util con el golden de RAM y la diferencia maxima; (3) los SHA-256 de los tres archivos del paquete de REF-CANT-01 con la
  tabla de fixture y la ruta fija de prueba; (4) las pruebas que pasan ya en el RED y por que; Worker = { Provider "Anthropic",
  ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" };
  WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada (una S-xx o C-xx de comportamiento
  STOP va con Disposition STOP); listas vacias si no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa al
  final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
