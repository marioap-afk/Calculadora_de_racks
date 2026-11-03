I-65 — DELEGACION F4-T3-DUMP-AND-READERS — Worker, perfil ROUTINE_IMPLEMENTATION
Identidad: unidad I-65; gate F4; tarea F4-T3-DUMP-AND-READERS; intento 2; RunId R20261011T183618Z-05a9;
           DelegationRunId R20261011T183102Z-7511; AuthorityRevision c73058f88368196309975616298523317ace2813;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha c73058f88368196309975616298523317ace2813;
           rama architecture/interop-analisis-estructural;
           worktree C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural;
           Owner subagent:R20261011T183102Z-7511
Autoridades (leer desde Git, no copiar): las del paquete aceptado (= las del contrato), en especial
           docs/initiatives/I-65-f4-implementation.md (§0, §1 y §4), docs/initiatives/I-65-f4-engineering-validation-package.md (§2 y
           §3), docs/initiatives/I-65-f3-ram-elements-adapter.md (§4, §5, §7), docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes:
           D-01, D-03, D-04, D-08, D-09, D-11, D-12; §3 INV-01, 02, 10 y 14), docs/automation/decisions/I-65.md §17, §26, §27 y §31,
           p1-results.md y p1-run/p1-report.json (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md y docs/AUTOMATION_PLAN.md §16
           (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: el paquete aceptado (Objective y AcceptanceCriteria) en
           artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183102Z-7511/delegation.json. En resumen: script fijo de volcado de
           solo lectura del modelo abierto y lectores del proveedor (volcado, lectura de vuelta, manifiesto) hacia tipos neutrales.
Alcance: el del paquete (= el del contrato). Permitido: src/RackCad.Application/StructuralAnalysis/Providers/RamElements/ (archivos
           nuevos; cambios aditivos en los existentes solo si hace falta reutilizar la regla de carpeta),
           src/RackCad.Application/StructuralAnalysis/StructuralReadback.cs (NUEVO), archivos NUEVOS de
           tests/RackCad.Tests/StructuralAnalysis/ (ninguna prueba existente se edita). Prohibido el
           ForbiddenWriteScope del paquete (todas las pruebas existentes; Systems/; assets/; Units/; StructuralSections/; otros
           archivos neutrales de StructuralAnalysis; Domain, Plugin, UI, tools, cualquier .csproj, docs/, etc.)
Invariantes del Freeze: los del paquete aceptado (Invariants), con docs/initiatives/I-65-f4-implementation.md §4
Rutas y archivos calientes (solo lectura salvo el alcance):
           src/RackCad.Application/StructuralAnalysis/Providers/RamElements/RamBuildScriptTemplate.cs (estilo de plantilla fija:
             marcador de carga util, Render, Escape, LF, ASCII; puerta de capacidad con get_core), RamBuildPayload.cs (carga util
             ASCII compacta), RamBuildPackageFactory.cs (IsAbsoluteNormalisedFolder), RamElementsTranslator.cs y RamBuildPlan.cs
             (D-04 hacia el programa: la inversa es la del lector), RamBuildManifest.cs (rackcad-ram-build-manifest/v1);
           src/RackCad.Application/StructuralAnalysis/ (StructuralModel, cantidades tipadas, StructuralDiagnostic);
           docs/automation/evidence/I-65-f0/p1-run/p1-report.json (E1_environment.core_functions: las 207 funciones del nucleo);
           fuente instalada del paquete del proveedor, SOLO LECTURA (no la ejecutes ni la importes):
             C:\Program Files\Bentley\Engineering\RAM Elements\python\ramelements (model.py, entities/node.py, entities/member.py,
             loads/load_condition.py, properties/units.py, _p4d/engine.py): de ahi salen las llamadas del volcado que P1 no ejecuto
             (iteracion de nudos, miembros y casos; node_i y node_j; categoria; combinaciones y sus terminos; units.current)
           tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsPackageTemplateTests.cs (estilo de la guarda estatica
             con autoprueba sobre variantes sinteticas; solo lectura) y StructuralAnalysisRamElementsFixtures.cs (REF-CANT-01 y su plan
             de RAM; solo lectura) y StructuralAnalysisRamElementsHardeningGuardTests.cs (ToolName: el nombre del modulo leido
             de la plantilla de construccion; la guarda de literales partidos; la autoprueba de las cuatro rutas exentas de INV-14;
             solo lectura)
Criterios de aceptacion: los AcceptanceCriteria del paquete aceptado
Evidencia requerida: tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElementsDump,
           minimo 25, RED esperado; suite Core completa en verde en el GREEN; commit RED y push; commit GREEN y push con el
           resumen de estado; worker-handoff.json valido en la ruta esperada
No-touch: todo lo prohibido arriba, el estado y la evidencia de la unidad (docs/automation/) y el checkout principal
           D:\Documentos\Codex\Calculadora de racks (nunca escribas alli)
Condiciones de parada: las del paquete aceptado (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-11); en particular C-03 (la ida y vuelta
           de REF-CANT-01 no reproduce el modelo), C-08 y C-11 (llamada fuera de la fuente instalada, funcion del nucleo fuera de P1
           o cualquier escritura en el modelo); ante cualquiera, parar sin continuar y devolver el bloqueo estructurado en la
           entrega (WorkerStatus BLOCKED, Disposition STOP y TriggeredStopConditions)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183618Z-05a9/worker-handoff.json;
           Worker: commit y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado; primera delegacion aceptada de la cadena F4-T3-DUMP-AND-READERS)
- Celda: la del paquete aceptado. ChainBaseSha = BaseSha = c73058f88368196309975616298523317ace2813; ChainRedSha = null y ChainRedFiles = []: esta entrega
  EXIGE RED.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta. dotnet: usa SIEMPRE
  /c/Users/alejandra-mendoza/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  C:\Users\alejandra-mendoza\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas solo con --logger "trx;LogFileName=<RunRef>.trx" y --results-directory
  artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183618Z-05a9/tests/<RunRef>; anota en Command la orden COMPLETA.
  Scripts auxiliares, si los necesitas, solo en artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T183618Z-05a9/work/.
  PROHIBIDO invocar un interprete Python por cualquier motivo, en ninguna forma (python, python3, py, ni siquiera --version ni
  dentro de otra orden; en esta unidad ya hubo tres desviaciones por eso, y en este contrato es la parada C-08); usa pwsh o las
  herramientas de lectura; sin AutoCAD y sin RAM Elements. Escribe los archivos con las herramientas de escritura y edicion
  (no con heredocs de Bash: danan las barras invertidas). La suite Core de la CI corre en ubuntu-latest: pruebas validas en Linux y
  en Windows; contenido generado con "\n" explicito.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha y git ls-remote origin
  refs/heads/architecture/interop-analisis-estructural = BaseSha. Si no, parada S-12/S-13.
- Lectura de I-65-f4-implementation.md §4 aplicada (no son decisiones nuevas; si algo con efecto observable no esta decidido ahi ni
  en el Freeze, no lo inventes: parada S-03 con la pregunta exacta; nombres de tipos libres salvo StructuralReadback.cs):
  - Plantilla del volcado: mismo estilo que la de construccion (texto fijo, una linea de carga util con marcador, Render con Escape,
    LF, solo ASCII); orden: imports; carga util; puerta de capacidad (version y funciones del nucleo leidas como hace la plantilla de
    construccion); m = rm.model(); par inicial = m.units.current; apertura de <carpeta>\ram-dump.json en modo "x"; try: si el par no
    es LB_IN, units.set(LB_IN); lectura de todo; finally: si se cambio, units.set(par inicial); escritura del json (con respaldo de
    fallo como la plantilla de construccion) y estado en consola. Sin ninguna otra escritura en el modelo.
  - Esquema rackcad-ram-dump/v1: schema, status, adapterVersion, initialUnits, readUnits, unitsRestored, nodes, members, loadCases,
    combinations y errors (o equivalentes que el lector valide de forma estricta); ids del programa para enlazar nudos y miembros.
  - Lector del volcado: los tipos neutrales llevan el marco neutral (inversa de D-04) y cantidades tipadas; un volcado leido en otro
    par -> Invalid con un codigo que nombre el par de unidades; un estado de fallo -> rechazo con su propio codigo. La prueba de ida y
    vuelta construye el volcado sintetico desde el RamBuildPlan de REF-CANT-01 (traduccion de F3 con la tabla de fixture verified) y
    compara con el StructuralModel neutral con tolerancia 1e-9.
  - Lectores de la lectura de vuelta y del manifiesto: estrictos; aprobado solo con READBACK OK; huella y version del adaptador.
  - INV-14 (CD-I65-F4-13): NINGUNA prueba existente se edita y la exencion de INV-14 sigue en sus cuatro rutas exactas (dos autopruebas
    lo exigen: INV14_EXIME_EXACTAMENTE_CUATRO_RUTAS_EXACTAS y ElInv14EximeExactamenteCuatroRutasExactas, que barre todas las pruebas).
    Ninguna prueba nueva escribe el nombre del modulo del proveedor ni del interprete en el codigo (ni partido en
    varios literales: otra guarda de F4-T1 lo detecta); cuando una comprobacion lo necesita (import, sinteticos de la guarda estatica),
    lo obtiene de StructuralAnalysisRamElementsHardeningScans.ToolName() (lee el import de la plantilla de construccion) y compone
    las cadenas en tiempo de ejecucion. Corre esas dos autopruebas y la de literales partidos antes del commit RED.
- Paso RED (un solo commit): esqueleto COMPILABLE (cuerpos que lanzan NotImplementedException o devuelven un valor que las pruebas
  rechazan); pruebas NUEVAS (namespace RackCad.Tests, clases StructuralAnalysisRamElementsDump*), >= 25 seleccionables, que fallan por
  asercion o excepcion (nunca compilacion). Commit RED (git add solo de archivos del alcance) y
  git push.
- Desde el commit RED NO modifiques ningun archivo de pruebas del RED (si hiciera falta: parada, sin editarlo). Revisa bien tus
  aserciones y patrones ANTES del commit RED (comillas escapadas, barras invertidas, saltos de linea): en F4-T1 una asercion
  defectuosa del RED obligo a una correccion.
- Paso GREEN: implementa en el alcance; corrida GREEN del filtro (todas en verde) y suite Core completa en verde (anota totales);
  comprueba el alcance del diff y que en tests/ solo hay altas; commit GREEN con esta linea en el cuerpo, en UNA
  sola linea, y git push:
  "Estado: F4 en curso; entrega del Worker de F4-T3-DUMP-AND-READERS (WorkRunId R20261011T183618Z-05a9) pendiente de la verificacion del Controller. attempts = 2."
- Git: sin --force, sin rebase, sin stash, sin amend, sin tocar otras ramas ni main; un commit RED y un commit GREEN.
  No esperes a la CI: el Principal recoge las corridas push.
- Texto libre de la entrega: ninguno de estos terminos, ni dentro de rutas: GATE PASS, GATE_PASS, Candidato, Candidate,
  FINAL_CANDIDATE_SHA, gate cerrado, gate closed, cierre del gate, rama integrada, integrada en main, integrated into main,
  merged into main, lista para integrar, ready to merge, Owner Validation APROBADA, Owner Validation APPROVED.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje
  dotnet (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas.
- Entrega (worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  TaskId F4-T3-DUMP-AND-READERS; RunId R20261011T183618Z-05a9; DelegationRunId R20261011T183102Z-7511; Initiative I-65; Gate F4; Attempt 2; BaseSha = el de
  arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto); Branch y Worktree reales; Pushed
  true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico, Command
  completo, Phase RED/GREEN/RELEVANT, TreeSha = commit probado o null si el arbol tenia cambios, ResultsFile = TRX); Evidence con:
  (1) por invariante la prueba que lo observa; (2) la lista de llamadas y atributos del volcado que P1 no ejecuto, con el archivo de la
  fuente instalada de donde salen y su funcion del nucleo; (3) el SHA-256 de un script de volcado renderizado con una carpeta fija de
  prueba; (4) la diferencia maxima de la ida y vuelta de REF-CANT-01; (5) las pruebas que pasan ya en el RED y por que; Worker = {
  Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5
  <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si
  no hay nada.
- Tope: 60 minutos. Itera con el filtro focal y corre la suite completa al final. Al terminar, tu respuesta final es una linea con
  WorkerStatus, RedSha y CurrentSha.
