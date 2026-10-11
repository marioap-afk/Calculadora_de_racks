I-65 — DELEGACION F1-T2-CANTILEVER-SOURCE — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F1; tarea F1-T2-CANTILEVER-SOURCE; intento 0; RunId R20261011T044815Z-7eae;
           DelegationRunId R20261011T044358Z-f76f; AuthorityRevision ff0c57c7125ae268bcb42b2d56c0d5255e9a69ed;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha ff0c57c7125ae268bcb42b2d56c0d5255e9a69ed;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T044358Z-f76f
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes): D-04, D-05 (IDL-01..10 y REF-CANT-01), D-10, D-10b, D-12, D-13,
           §3 filas INV-04, 05, 06, 07 y 09, y §15; docs/initiatives/I-65-f1-ref-cant-01.md (fixture, entradas resueltas y GOLDEN);
           docs/automation/decisions/I-65.md §10-§13 (CD-I65-F1-01..16) (UNIT_DOC, en AuthorityRevision); ADR-0050 (UNIT_CHANGE);
           AGENTS.md «Convenciones arquitectonicas», ADR-0009, ADR-0022, ADR-0024, ADR-0025, ADR-0017 y docs/AUTOMATION_PLAN.md
           §16 (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044358Z-f76f/delegation.json. En resumen: la fuente
           Cantilever del modelo neutral (un archivo nuevo, CantileverStructuralModelSource.cs) que implementa IStructuralModelSource
           de F1-T1 y produce, con BuildStructuralModel, el modelo Valid de REF-CANT-01 igual al golden derivado a mano.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/Systems/Cantilever/CantileverStructuralModelSource.cs
           (archivo NUEVO), cambios ADITIVOS en src/RackCad.Application/StructuralAnalysis/ (fuera de Providers/) solo si hacen
           falta, y archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/ (incluido Fixtures/). Prohibido el
           ForbiddenWriteScope del paquete (las nueve pruebas de F1-T1, Providers/, Units/, StructuralSections/, los demas sistemas,
           Domain, Plugin, UI, cualquier .csproj, las guardas existentes, docs/, etc.). Ninguna prueba existente se modifica, borra
           ni renombra: si hiciera falta, parada (C-01)
Invariantes del Freeze: los del paquete aceptado (Invariants), con las concreciones CD-I65-F1-13..16 y el golden
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/StructuralAnalysis/ (StructuralModelBuilder.BuildStructuralModel, IStructuralModelSource,
             StructuralScope, StructuralSourceDescription, StructuralSourceMember, StructuralNode, StructuralSupport,
             StructuralMemberReleases, StructuralMemberPointLoad, AnalysisAssumption, StructuralDiagnostic, StructuralLoadFileReader,
             StructuralLoadFile/StructuralLoadEntry, IStructuralMaterialProfile, StructuralFingerprints);
           src/RackCad.Application/Systems/Cantilever/ (CantileverLineResolver.Resolve, CantileverLineStationPlacement.ScopedId,
             CantileverStationAssembly: FaceMode, SingleSide, IsBlocked, ColumnBase.Column, Levels[k].Arm(side).Body con
             Arrangement, AngleRadians y Members; CantileverStructuralMemberPlan.Placement; CantileverCataloguePolicies);
           src/RackCad.Application/StructuralSections/Geometry/PrismaticSectionInstance.cs (Frame, Length, RotationRadians,
             Mirrored, SectionTransform);
           src/RackCad.Application/Persistence/RackProjectStore.cs (solo desde las PRUEBAS, para deserializar el fixture);
           tests/RackCad.Tests/CantileverSourceGuardTests.cs, CantileverRoundTwoSourceGuardTests.cs y
             CantileverPluginSourceGuardTests.cs (deben seguir en verde SIN cambios) y las nueve pruebas de F1-T1
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisCantilever,
           minimo 20, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); en particular C-01 (tocar
           algo fuera del alcance o una prueba existente), C-02 (una regla congelada o CD-I65-F1-13..16 no implementable tal como
           esta), C-03 (el golden no coincide con lo que dan las reglas IDL: parar con la diferencia exacta, sin ajustar nada),
           C-06 (un INV sin RED observable), C-07 (inferir material o exponer un modelo parcial), C-08 (dependencia binaria,
           NuGet, Python o RAM Elements), C-09 (habria que debilitar una guarda existente) y C-10 (cuestion material de
           arquitectura, autoridad o semantica congelada); ante cualquiera, parar sin continuar y devolver el bloqueo
           estructurado en la entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044815Z-7eae/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion de la cadena F1-T2-CANTILEVER-SOURCE)
- Celda: claude-sonnet-5-5 x subagente, effort high (Deep). ChainBaseSha = BaseSha = ff0c57c7125ae268bcb42b2d56c0d5255e9a69ed;
  ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044815Z-7eae/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F1-T2-CANTILEVER-SOURCE/0/R20261011T044815Z-7eae/work/.
  Sin AutoCAD, sin Python y sin RAM Elements. Para escribir archivos largos usa la herramienta de escritura de archivos, no
  heredocs de Bash.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura del Freeze aplicada a F1-T2 (no son decisiones nuevas; si el Freeze, CD-I65-F1-13..16 y el documento del golden no
  deciden algo que necesitas decidir con efecto observable, no lo inventes: parada S-03 con la pregunta exacta):
  - Entradas de la fuente (CD-I65-F1-03, CD-I65-F1-16): el CantileverLineDesign YA deserializado y el StructuralSectionCatalog;
    la fuente no lee el sobre ni deserializa bytes; resuelve con CantileverLineResolver.Resolve(design, catalog, new
    StructuralSectionGeometryFactory(catalog), CantileverCataloguePolicies.ColumnBase(catalog), CantileverCataloguePolicies.Arm(catalog))
    y toma line.Stations[indice].Station en coordenadas LOCALES de la estacion (sin trasladar por OriginX). Las piezas de
    origen son el id de pieza con alcance de estacion (ScopedId), p. ej. CANT-S0-STN-COL.
  - Guardas de Cantilever (CantileverSourceGuardTests): el archivo nuevo no puede contener, ni en identificadores ni en texto
    de codigo (los comentarios se quitan), las subcadenas Document, RackProject, Xrecord, ViewBuilder, Preview,
    RackSystemKind ni Interval, ni StructuralSectionUnits, .Dimensions, .d/.bf/.tw/.tf, LocalFrame3D.Create/FromAxes,
    File./Directory., mensula ni bahia; tampoco tokens de proveedor (G1). Comprueba la suite de guardas antes del commit.
  - Fuera de V1 (IDL-09, IDL-10, CD-I65-F1-14): doble cara, una cara NegativeY, canales pareados (por plantilla o por
    excepcion de celda), inclinacion > 10 grados y estacion o linea bloqueada -> Unsupported; placas, separadores,
    arriostramientos y cartelas NO se modelan y no bloquean. Kind distinto de Cantilever o indice de estacion fuera de rango
    -> Invalid; nivel de carga fuera de rango o lado distinto del de la estacion -> Invalid.
  - Golden (CD-I65-F1-15): copia como literales, digito a digito, las tablas de docs/initiatives/I-65-f1-ref-cant-01.md §2
    (colocaciones resueltas, comprobadas en la prueba) y §3 (nudos, miembros, apoyos, supuestos) y §5 (posiciones de carga);
    tolerancias 1e-9 in y 1e-12. Nunca derives el golden con el codigo bajo prueba.
  - Peralte (INV-07, parte neutral): el oraculo de la prueba es PrismaticSectionInstance.SectionTransform aplicado a la
    direccion local de peralte (0, 1) y llevado a global con el Frame; colocaciones sinteticas de giro 0, 90 y 12.3456 grados,
    con y sin espejo, construidas en la PRUEBA.
  - Fixtures: el diseno de REF-CANT-01 es el JSON de docs/automation/evidence/I-65-f1/ref-cant-01.design.json, copiado a
    tests/RackCad.Tests/StructuralAnalysis/Fixtures/; las pruebas lo leen desde la raiz del repositorio (como las guardas,
    RepoRoot), lo deserializan con RackProjectStore y pasan sus bytes al StructuralScope; RackId del alcance = un GUID fijo
    distinto del Line.Id; Kind = Cantilever. El archivo de cargas del fixture (§5 del documento) va al mismo directorio. El
    perfil de material de las pruebas tiene dos filas de fixture (W12X26 y HSS4X4X_250) declaradas como fixtures, no como
    verificacion de ingenieria. Ninguna prueba asserta un hash literal de los bytes de un archivo de texto.
- Paso RED (un solo commit):
  1. Esqueleto COMPILABLE de CantileverStructuralModelSource (namespace RackCad.Application.Systems.Cantilever, con bloque) con
     cuerpos que lanzan NotImplementedException o devuelven un valor que las pruebas rechazan.
  2. Fixtures y pruebas en archivos NUEVOS de tests/RackCad.Tests/StructuralAnalysis/, namespace RackCad.Tests, clases cuyo
     nombre empieza por StructuralAnalysisCantilever, >= 20 pruebas seleccionables que cubren todos los invariantes del paquete.
  3. Corre el filtro del contrato: compila, selecciona >= 20 y FALLA (asercion o excepcion; nunca compilacion). Las pruebas que
     comprueban solo las colocaciones del resolver pueden pasar ya en el RED: registralo en Evidence. Commit RED (git add solo de
     archivos del alcance) y git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas ni fixture del RED (si hiciera falta: parada, sin editarlo).
- Paso GREEN:
  1. Implementa en el alcance.
  2. Corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota totales; incluye las pruebas de F1-T1 y
     las guardas de Cantilever, LengthUnitsAuthorityGuardTests y NamespaceFolderGuardTests).
  3. Comprueba: git diff --name-only BaseSha..HEAD dentro del alcance permitido y fuera del prohibido; git diff --name-status
     BaseSha..HEAD -- tests/ solo con altas (A) bajo tests/RackCad.Tests/StructuralAnalysis/.
  4. Commit GREEN (git add solo de archivos del alcance) con esta linea en el cuerpo, en UNA sola linea, y git push:
     "Estado: F1 en curso; entrega del Worker de F1-T2-CANTILEVER-SOURCE (WorkRunId R20261011T044815Z-7eae) pendiente de la verificacion del Controller. attempts = 0."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F1-T2-CANTILEVER-SOURCE; RunId R20261011T044815Z-7eae; DelegationRunId R20261011T044358Z-f76f; Initiative I-65; Gate F1;
  Attempt 0; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults
  de cada corrida (RunRef unico, Command completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol
  tenia cambios, ResultsFile = TRX); Evidence con: (1) por invariante, la prueba que lo observa; (2) la comparacion del golden
  (nudos y miembros) con la diferencia maxima observada; (3) los cambios aditivos en StructuralAnalysis, o que no hubo;
  (4) las pruebas que pasan ya en el RED y por que; Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5",
  EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus
  IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada (una S-xx o C-xx de comportamiento STOP va con
  Disposition STOP); listas vacias si no hay nada.
- Tope: 60 minutos (README de ejecucion delegada). Administra el tiempo: itera con el filtro focal y corre la suite completa al
  final. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
